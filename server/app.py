"""
PCB 불량 검사 Flask 추론 서버

실행 방법:
    python server/app.py

또는:
    flask --app server/app run --host=0.0.0.0 --port=5000
"""

# eventlet monkey patching 제거 (YOLO + OpenCV와 충돌) ⚠️
# threading 모드로 변경하여 안정성 확보
# import eventlet
# eventlet.monkey_patch()

from flask import Flask, request, jsonify, Response, render_template_string
from flask_cors import CORS
from flask_socketio import SocketIO, emit  # WebSocket 지원
import base64
import cv2
import numpy as np
from datetime import datetime
import logging
import time
import os
from pathlib import Path
import threading
from collections import deque

# .env 파일 로드
try:
    from dotenv import load_dotenv
    # server/.env 파일 경로
    env_path = Path(__file__).parent / '.env'
    load_dotenv(dotenv_path=env_path)
    print(f"✅ 환경 변수 로드 완료: {env_path}")
except ImportError:
    print("⚠️  python-dotenv가 설치되지 않았습니다. 시스템 환경 변수를 사용합니다.")
    print("💡 설치 명령어: pip install python-dotenv")

# 로컬 모듈 임포트
from db_manager import DatabaseManager
from pcb_alignment import PCBAligner
from component_verification import ComponentVerifier
import json

# Flask 앱 초기화
app = Flask(__name__)
CORS(app)  # C# WinForms 연동을 위한 CORS 활성화

# SocketIO 초기화 (WebSocket 실시간 프레임 스트리밍)
socketio = SocketIO(
    app,
    cors_allowed_origins="*",  # 모든 Origin 허용 (프로덕션에서는 제한 필요)
    async_mode='threading',     # threading 모드 (YOLO/OpenCV 안정성 우선) ⭐⭐⭐
    logger=True,                # 디버깅용 로그
    engineio_logger=True,       # Engine.IO 디버깅 로그
    # 소켓 타임아웃 설정 (좀비 연결 방지) ⭐
    ping_timeout=60,            # 60초 동안 응답 없으면 연결 종료
    ping_interval=25,           # 25초마다 ping 전송
    # 연결 설정
    max_http_buffer_size=10 * 1024 * 1024,  # 10MB (프레임 크기 고려)
    async_handlers=True         # 비동기 핸들러 사용 (성능 향상)
)
logger_socketio = logging.getLogger('socketio')
logger_socketio.setLevel(logging.INFO)

# 로깅 설정
logging.basicConfig(
    level=logging.DEBUG,  # DEBUG로 변경하여 상세 로그 출력
    format='[%(asctime)s] [%(levelname)s] [Flask-Server] [%(module)s] - %(message)s'
)
logger = logging.getLogger(__name__)

# 데이터베이스 매니저 초기화
# 환경 변수 또는 설정 파일에서 읽기 (나중에 config.yaml로 이동)
DB_CONFIG = {
    'host': os.getenv('DB_HOST', 'localhost'),
    'port': int(os.getenv('DB_PORT', 3306)),
    'user': os.getenv('DB_USER', 'root'),
    'password': os.getenv('DB_PASSWORD', 'your_password'),
    'database': os.getenv('DB_NAME', 'pcb_inspection')
}

db = DatabaseManager(**DB_CONFIG)

# YOLO 모델 로드
try:
    from ultralytics import YOLO
    model_path = '../runs/detect/roboflow_pcb_balanced/weights/best.pt'  # 프로젝트 루트 기준
    yolo_model = YOLO(model_path)
    logger.info(f"✅ YOLO 모델 로드 완료: {model_path}")
    logger.info(f"   - 모델 타입: YOLOv11l")
    logger.info(f"   - 클래스 수: 12개 (PCB 부품 검출)")
except Exception as e:
    logger.error(f"⚠️  YOLO 모델 로드 실패: {e}")
    logger.warning("   - 추론 시 더미 결과 반환됨")
    yolo_model = None

# PCB 정렬 및 컴포넌트 검증 모듈 초기화
pcb_aligner_left = None
pcb_aligner_right = None
component_verifier_left = None
component_verifier_right = None

# 디버그 이미지 캐시 (실시간 뷰어용)
latest_debug_image = None

try:
    # 기준 데이터 로드 (좌측)
    left_ref_path = Path(__file__).parent / 'reference_data' / 'reference_left.json'
    if left_ref_path.exists():
        with open(left_ref_path, 'r', encoding='utf-8') as f:
            left_reference_data = json.load(f)

        # PCBAligner 초기화 (좌측)
        pcb_aligner_left = PCBAligner(left_reference_data)

        # ComponentVerifier 초기화 (좌측)
        component_verifier_left = ComponentVerifier(
            reference_components=left_reference_data['components'],
            position_threshold=20.0,  # 20픽셀 이내
            confidence_threshold=0.25
        )

        logger.info(f"✅ 좌측 PCB 기준 데이터 로드 완료: {left_ref_path}")
        logger.info(f"   - 나사 구멍: {len(left_reference_data['mounting_holes'])}개")
        logger.info(f"   - 기준 컴포넌트: {len(left_reference_data['components'])}개")
    else:
        logger.warning(f"⚠️  좌측 기준 데이터 파일이 없습니다: {left_ref_path}")
        logger.warning("   - PCB 정렬 및 컴포넌트 검증 비활성화 (좌측)")
except Exception as e:
    logger.error(f"⚠️  좌측 기준 데이터 로드 실패: {e}")
    logger.warning("   - PCB 정렬 및 컴포넌트 검증 비활성화 (좌측)")

try:
    # 기준 데이터 로드 (우측)
    right_ref_path = Path(__file__).parent / 'reference_data' / 'reference_right.json'
    if right_ref_path.exists():
        with open(right_ref_path, 'r', encoding='utf-8') as f:
            right_reference_data = json.load(f)

        # PCBAligner 초기화 (우측)
        pcb_aligner_right = PCBAligner(right_reference_data)

        # ComponentVerifier 초기화 (우측)
        component_verifier_right = ComponentVerifier(
            reference_components=right_reference_data['components'],
            position_threshold=20.0,  # 20픽셀 이내
            confidence_threshold=0.25
        )

        logger.info(f"✅ 우측 PCB 기준 데이터 로드 완료: {right_ref_path}")
        logger.info(f"   - 나사 구멍: {len(right_reference_data['mounting_holes'])}개")
        logger.info(f"   - 기준 컴포넌트: {len(right_reference_data['components'])}개")
    else:
        logger.warning(f"⚠️  우측 기준 데이터 파일이 없습니다: {right_ref_path}")
        logger.warning("   - PCB 정렬 및 컴포넌트 검증 비활성화 (우측)")
except Exception as e:
    logger.error(f"⚠️  우측 기준 데이터 로드 실패: {e}")
    logger.warning("   - PCB 정렬 및 컴포넌트 검증 비활성화 (우측)")

# 실시간 뷰어를 위한 전역 변수
latest_frames = {
    'left': None,
    'right': None
}
# JPEG 캐싱 (WebSocket 성능 최적화) ⭐
latest_frames_jpeg = {
    'left': None,  # Base64 encoded JPEG 문자열
    'right': None
}
latest_results = {
    'left': {},
    'right': {}
}
frame_lock = threading.Lock()

# Temporal Smoothing 설정 (깜빡거림 최소화)
HISTORY_SIZE = 15         # 최근 15프레임 저장
MIN_DETECTION_FRAMES = 5  # 최소 5프레임 검출 시 표시 (매우 안정적) ⭐
MAX_MISSING_FRAMES = 20   # 사라진 후 20프레임까지 유지 (매우 오래 유지) ⭐
IOU_THRESHOLD = 0.05      # 5% 이상만 겹쳐도 같은 객체로 판단 (극도로 관대) ⭐⭐
CONFIDENCE_THRESHOLD = 0.3  # 신뢰도 30% 이상만 사용
FREEZE_AFTER_FRAMES = 9999  # 프로즌 기능 비활성화 (사용자 요청) ⭐⭐⭐

# ROI 설정 (PCB 자동 감지 및 내부 영역만 검출) ⭐⭐⭐
PCB_COLOR_LOWER_HSV = np.array([35, 40, 40])    # 초록색 하한 (HSV)
PCB_COLOR_UPPER_HSV = np.array([85, 255, 255])  # 초록색 상한 (HSV)
PCB_INNER_MARGIN_PERCENT = 0.03  # PCB 테두리 3% 제외 (파란색 선에 가깝게) ⭐
PCB_EDGE_THRESHOLD = 10  # PCB가 프레임 경계에서 최소 10픽셀 떨어져야 전체로 간주

# 모션 감지 설정 (새 PCB 진입 감지) ⭐⭐⭐
MOTION_THRESHOLD = 30.0    # 프레임 차이 임계값 (픽셀 평균 차이)
STABLE_FRAMES_FOR_FREEZE = 10  # 10프레임 동안 안정 시 frozen 모드

# 검출 히스토리 (카메라별)
detection_history = {
    'left': deque(maxlen=HISTORY_SIZE),
    'right': deque(maxlen=HISTORY_SIZE)
}

# 추적 중인 객체 (카메라별)
tracked_objects = {
    'left': {},   # {object_id: {'box': {...}, 'class_id': int, 'class_name': str, 'confidence': float, 'count': int, 'missing': int}}
    'right': {}
}
next_object_id = 0
tracking_lock = threading.Lock()

# 완전 정지 모드 (모든 객체가 frozen 상태가 되면 프레임 업데이트 중지) ⭐⭐⭐
camera_frozen_state = {
    'left': False,   # True가 되면 프레임 업데이트 중지
    'right': False
}

# 모션 감지용 이전 프레임 저장 (새 PCB 진입 감지) ⭐⭐⭐
previous_frames = {
    'left': None,
    'right': None
}

# 안정 프레임 카운터 (움직임 없는 프레임 수) ⭐⭐⭐
stable_frame_count = {
    'left': 0,
    'right': 0
}


@app.route('/health', methods=['GET'])
def health_check():
    """서버 상태 체크"""
    return jsonify({
        'status': 'ok',
        'timestamp': datetime.now().isoformat(),
        'server': 'Flask PCB Inspection Server',
        'version': '1.0.0'
    })


@app.route('/predict_test', methods=['POST'])
def predict_test():
    """
    카메라 테스트용 엔드포인트 (DB 저장 없음)

    Request JSON:
        {
            "camera_id": "left" or "right",
            "image": "base64_encoded_jpeg_image"
        }

    Response JSON:
        {
            "status": "ok",
            "camera_id": "left",
            "defect_type": "정상",
            "confidence": 0.95,
            "inference_time_ms": 5.2,
            "timestamp": "2025-01-27T15:30:45.123456",
            "note": "테스트 모드 (DB 저장 안 함)"
        }
    """
    start_time = time.time()

    try:
        # 1. 요청 데이터 검증
        data = request.get_json()
        if not data:
            return jsonify({
                'status': 'error',
                'error': 'Request body is empty'
            }), 400

        camera_id = data.get('camera_id')
        image_base64 = data.get('image')

        if not camera_id or not image_base64:
            return jsonify({
                'status': 'error',
                'error': 'Missing required fields: camera_id, image'
            }), 400

        # 2. Base64 디코딩 및 프레임 검증
        try:
            image_bytes = base64.b64decode(image_base64)
            nparr = np.frombuffer(image_bytes, np.uint8)
            frame = cv2.imdecode(nparr, cv2.IMREAD_COLOR)

            if frame is None or frame.size == 0:
                return jsonify({
                    'status': 'error',
                    'error': 'Invalid image data: failed to decode'
                }), 400

            logger.info(f"[TEST] 프레임 수신 성공: {camera_id} (원본 shape: {frame.shape})")

            # 원본 프레임 저장 (PCB 외곽선 검출용) ⭐
            original_frame = frame.copy()

            # 중앙 크롭 (640x480 → 640x640) ⭐⭐⭐
            frame = crop_to_square(frame, target_size=640)
            logger.info(f"[TEST] 중앙 크롭 완료: {camera_id} (크롭 후 shape: {frame.shape})")

        except Exception as decode_error:
            return jsonify({
                'status': 'error',
                'error': f'Failed to decode image: {str(decode_error)}'
            }), 400

        # 2-1. 모션 감지 (새 PCB 진입 확인) ⭐⭐⭐
        motion_detected, motion_value = detect_motion(frame, previous_frames.get(camera_id), camera_id)

        if motion_detected:
            # 큰 움직임 감지 → 새 PCB 진입!
            logger.info(f"🚨 [{camera_id}] 모션 감지! (차이: {motion_value:.1f}) → 추론 재개")

            # frozen 상태 리셋
            with tracking_lock:
                camera_frozen_state[camera_id] = False
                stable_frame_count[camera_id] = 0
                tracked_objects[camera_id].clear()  # 모든 추적 객체 초기화
                logger.info(f"🔓 [{camera_id}] frozen 상태 리셋 완료")
        else:
            # 움직임 없음 → 안정 프레임 증가
            stable_frame_count[camera_id] += 1

        # 이전 프레임 업데이트
        previous_frames[camera_id] = frame.copy()

        # 2-2. 완전 정지 모드 확인 (모든 객체가 frozen 상태면 추론 건너뛰기) ⭐⭐⭐
        if camera_frozen_state.get(camera_id, False):
            # 이미 frozen 상태 - 추론하지 않고 기존 결과 반환
            with frame_lock:
                existing_result = latest_results.get(camera_id, {})

            if existing_result:
                logger.info(f"🔒 [{camera_id}] 정지 모드 - 기존 결과 반환 (추론 생략)")
                return jsonify(existing_result)
            else:
                # 결과가 없으면 한 번만 추론 실행 (초기화)
                logger.info(f"⚠️  [{camera_id}] 정지 모드지만 기존 결과 없음 - 초기 추론 실행")

        # 3. PCB 정렬 (Perspective Transform) ⭐⭐⭐
        aligned_frame = frame
        alignment_info = {'aligned': False}

        # camera_id에 따라 적절한 aligner 선택
        # PCB 외곽선 검출 - 엣지 기반 실시간 검출 ⭐⭐⭐
        global latest_debug_image
        pcb_aligner = pcb_aligner_left if camera_id == 'left' else pcb_aligner_right

        if pcb_aligner is not None:
            try:
                import os
                # 디버그 이미지 저장 경로 (temp_frames 폴더)
                debug_save_path = os.path.join(os.path.dirname(__file__), '..', 'temp_frames')
                os.makedirs(debug_save_path, exist_ok=True)

                # ROI 기반 실시간 PCB 컨투어 검출 ⭐⭐⭐
                pcb_corners, is_valid = pcb_aligner.detect_pcb_contour_realtime(
                    original_frame,
                    debug=True,
                    debug_path=debug_save_path
                )

                # 디버그 이미지를 temp_frames/06_detected_contour.jpg에서 읽어오기
                # (pcb_alignment.py에서 이미 ROI, 컨투어, 상태 메시지 모두 그려져 있음)
                debug_img_path = os.path.join(debug_save_path, "06_detected_contour.jpg")
                if os.path.exists(debug_img_path):
                    latest_debug_image = cv2.imread(debug_img_path)
                    logger.debug(f"📖 디버그 이미지 로드: {debug_img_path}")
                else:
                    latest_debug_image = original_frame.copy()

                # 검출 결과 로깅
                if pcb_corners is not None and is_valid:
                    logger.info(f"✅ PCB 검출 성공 (검증 통과): {camera_id} - PCB READY")
                elif pcb_corners is not None and not is_valid:
                    logger.warning(f"⚠️  PCB 검출됨 (검증 실패): {camera_id} - NOT READY")
                else:
                    logger.warning(f"⚠️  PCB 컨투어 검출 실패: {camera_id} - NOT READY")
            except Exception as e:
                logger.error(f"PCB 외곽선 검출 중 오류: {e}")
                import traceback
                traceback.print_exc()
                latest_debug_image = original_frame.copy()
        else:
            latest_debug_image = original_frame.copy()

        # PCB 정렬은 skip (사용하지 않음)
        alignment_info = {'aligned': False, 'message': 'PCB alignment disabled'}
        aligned_frame = frame.copy()

        # 3-1. PCB ROI 감지 (비활성화 - 암막 준비 후 활성화 예정) ⭐⭐⭐
        # roi_mask, pcb_bbox, roi_bbox = detect_pcb_roi(aligned_frame)
        # if pcb_bbox is not None:
        #     logger.info(f"[TEST] PCB 감지 성공: {camera_id} → PCB {pcb_bbox}, ROI {roi_bbox}")
        # else:
        #     logger.warning(f"[TEST] PCB 감지 실패: {camera_id} → 전체 프레임 사용")

        # ROI 비활성화 - 전체 프레임 사용
        pcb_bbox, roi_bbox = None, None

        # 4. AI 추론 (YOLO 모델, DB 저장 안 함) - 크롭된 원본 프레임으로 실행 ⭐
        boxes_data = []
        if yolo_model is not None:
            try:
                # YOLO 추론 실행 (원본 프레임 사용 - 보정 없이) ⭐
                # 참고: ROI 마스크를 직접 적용하지 않고, 추론 후 필터링으로 처리
                results = yolo_model(frame, verbose=False)
                defect_type, confidence, raw_boxes_data = parse_yolo_results(results)

                # 신뢰도 필터링 (낮은 신뢰도 제거)
                filtered_boxes = [box for box in raw_boxes_data if box['confidence'] >= CONFIDENCE_THRESHOLD]

                # ROI 필터링 (비활성화 - 전체 프레임 사용) ⭐
                # if roi_bbox is not None:
                #     rx, ry, rw, rh = roi_bbox
                #     roi_filtered_boxes = []
                #     for box in filtered_boxes:
                #         # 바운딩 박스 중심점이 ROI 안에 있는지 확인
                #         cx = box['x1'] + (box['x2'] - box['x1']) / 2
                #         cy = box['y1'] + (box['y2'] - box['y1']) / 2
                #         if rx <= cx <= rx + rw and ry <= cy <= ry + rh:
                #             roi_filtered_boxes.append(box)
                #     logger.info(f"[TEST] ROI 필터링: {camera_id} → {len(filtered_boxes)}개 → {len(roi_filtered_boxes)}개")
                #     filtered_boxes = roi_filtered_boxes

                # 검출 결과 평활화 (Temporal Smoothing)
                smoothed_boxes = smooth_detections(camera_id, filtered_boxes)

                # 평활화된 결과로 바운딩 박스 그리기 (원본 프레임 사용 - 보정 없이) ⭐
                annotated_frame = draw_bounding_boxes(frame.copy(), smoothed_boxes, pcb_bbox, roi_bbox)

                logger.info(f"[TEST] YOLO 추론 완료: {camera_id} → 원본 {len(raw_boxes_data)}개 → 필터링 {len(filtered_boxes)}개 → 평활화 {len(smoothed_boxes)}개 객체")

                # 최종 boxes_data는 평활화된 결과 사용
                boxes_data = smoothed_boxes
            except Exception as yolo_error:
                logger.error(f"[TEST] YOLO 추론 실패: {yolo_error}")
                defect_type = "정상"
                confidence = 0.0
                annotated_frame = frame.copy()
        else:
            logger.warning("[TEST] YOLO 모델이 로드되지 않음 - 더미 결과 반환")
            defect_type = "정상"
            confidence = 0.95
            annotated_frame = frame.copy()

        gpio_pin = get_gpio_pin(defect_type)

        # 뷰어를 위해 바운딩 박스가 그려진 프레임 저장
        with frame_lock:
            latest_frames[camera_id] = annotated_frame

            # JPEG 인코딩 및 캐싱 (WebSocket 성능 최적화) ⭐
            encode_params = [
                cv2.IMWRITE_JPEG_QUALITY, 85,
                cv2.IMWRITE_JPEG_PROGRESSIVE, 0,
                cv2.IMWRITE_JPEG_OPTIMIZE, 1
            ]
            ret, buffer = cv2.imencode('.jpg', annotated_frame, encode_params)
            if ret:
                frame_base64 = base64.b64encode(buffer.tobytes()).decode('utf-8')
                latest_frames_jpeg[camera_id] = frame_base64
                logger.debug(f"[TEST] JPEG 캐싱 완료: {camera_id} ({len(frame_base64)} bytes)")

        # 4. 추론 시간 계산
        inference_time_ms = (time.time() - start_time) * 1000

        # 5. 응답 생성 (DB 저장 생략)
        response = {
            'status': 'ok',
            'camera_id': camera_id,
            'defect_type': defect_type,
            'confidence': confidence,
            'gpio_pin': gpio_pin,
            'inference_time_ms': round(inference_time_ms, 2),
            'timestamp': datetime.now().isoformat(),
            'note': '테스트 모드 (DB 저장 안 함)'
        }

        # 뷰어를 위해 결과 저장
        with frame_lock:
            latest_results[camera_id] = response.copy()

        logger.info(f"[TEST] 추론 완료: {camera_id} → {defect_type} (time: {inference_time_ms:.1f}ms)")
        return jsonify(response)

    except Exception as e:
        logger.error(f"[TEST] 추론 실패: {str(e)}", exc_info=True)
        return jsonify({
            'status': 'error',
            'error': str(e)
        }), 500


@app.route('/predict', methods=['POST'])
def predict_single():
    """
    단일 프레임 추론 (좌측 또는 우측)

    Request JSON:
        {
            "camera_id": "left" or "right",
            "image": "base64_encoded_jpeg_image"
        }

    Response JSON:
        {
            "status": "ok",
            "camera_id": "left",
            "defect_type": "정상" | "부품불량" | "납땜불량" | "폐기",
            "confidence": 0.95,
            "inference_time_ms": 85.3,
            "timestamp": "2025-01-27T15:30:45.123456"
        }
    """
    start_time = time.time()

    try:
        # 1. 요청 데이터 검증
        data = request.get_json()
        if not data:
            logger.error("요청 데이터가 비어있음")
            return jsonify({
                'status': 'error',
                'error': 'Request body is empty'
            }), 400

        camera_id = data.get('camera_id')
        image_base64 = data.get('image')

        if not camera_id or not image_base64:
            logger.error(f"필수 필드 누락: camera_id={camera_id}, image={'있음' if image_base64 else '없음'}")
            return jsonify({
                'status': 'error',
                'error': 'Missing required fields: camera_id, image'
            }), 400

        # 2. Base64 디코딩 및 프레임 검증
        try:
            image_bytes = base64.b64decode(image_base64)
            nparr = np.frombuffer(image_bytes, np.uint8)
            frame = cv2.imdecode(nparr, cv2.IMREAD_COLOR)

            if frame is None or frame.size == 0:
                logger.error("프레임 디코딩 실패: 유효하지 않은 이미지 데이터")
                return jsonify({
                    'status': 'error',
                    'error': 'Invalid image data: failed to decode'
                }), 400

            logger.info(f"프레임 수신 성공: {camera_id} (shape: {frame.shape})")

        except Exception as decode_error:
            logger.error(f"Base64 디코딩 실패: {decode_error}")
            return jsonify({
                'status': 'error',
                'error': f'Failed to decode image: {str(decode_error)}'
            }), 400

        # 3. PCB 정렬 (Perspective Transform)
        aligned_frame = frame
        alignment_info = {'aligned': False}

        # camera_id에 따라 적절한 aligner 선택
        pcb_aligner = pcb_aligner_left if camera_id == 'left' else pcb_aligner_right

        if pcb_aligner is not None:
            try:
                # process_frame() 사용: 구멍 검출 + 정렬 통합 처리
                alignment_result = pcb_aligner.process_frame(frame, debug=True)

                # 디버그 이미지 저장 (있으면, 없으면 원본 프레임)
                global latest_debug_image
                if 'debug_image' in alignment_result.get('debug_info', {}):
                    latest_debug_image = alignment_result['debug_info']['debug_image']
                else:
                    # 정렬 실패 시 원본 프레임에 에러 메시지 표시
                    debug_frame = frame.copy()
                    error_msg = alignment_result.get('error', 'PCB 검출 실패')
                    cv2.putText(debug_frame, f"Alignment Failed: {error_msg}", (10, 30),
                               cv2.FONT_HERSHEY_SIMPLEX, 0.7, (0, 0, 255), 2)
                    cv2.putText(debug_frame, "Showing live frame from Raspberry Pi", (10, 60),
                               cv2.FONT_HERSHEY_SIMPLEX, 0.5, (255, 255, 0), 1)
                    latest_debug_image = debug_frame

                if alignment_result['success']:
                    aligned_frame = alignment_result['aligned_frame']
                    alignment_info = {
                        'aligned': True,
                        'method': alignment_result['method'],
                        'transform_matrix': alignment_result['transform_matrix'].tolist() if alignment_result['transform_matrix'] is not None else None,
                        'debug_info': alignment_result.get('debug_info', {})
                    }
                    logger.info(f"PCB 정렬 성공 ({camera_id}): {alignment_result['method']} 방식")
                else:
                    logger.warning(f"PCB 정렬 실패 ({camera_id}): {alignment_result.get('error', '알 수 없는 오류')}")
                    alignment_info = {
                        'aligned': False,
                        'error': alignment_result.get('error'),
                        'debug_info': alignment_result.get('debug_info', {})
                    }
            except Exception as align_error:
                logger.error(f"PCB 정렬 중 오류 ({camera_id}): {align_error}")
                alignment_info = {'aligned': False, 'error': str(align_error)}
        else:
            logger.debug(f"PCB 정렬 비활성화 ({camera_id}): 기준 데이터 없음")

        # 4. AI 추론 (YOLO 모델) - 정렬된 프레임으로 실행
        detections = []  # YOLO 검출 결과 (컴포넌트 검증용)
        if yolo_model is not None:
            try:
                # YOLO 추론 실행 (정렬된 프레임 사용!)
                results = yolo_model(aligned_frame, verbose=False)
                defect_type, confidence, boxes = parse_yolo_results(results)

                # 검출 결과를 컴포넌트 검증용 형식으로 변환
                for box in boxes:
                    x1, y1, x2, y2 = box['bbox']
                    cx = (x1 + x2) / 2
                    cy = (y1 + y2) / 2
                    detections.append({
                        'class_name': box['class_name'],
                        'bbox': [x1, y1, x2, y2],
                        'center': [cx, cy],
                        'confidence': box['confidence']
                    })

                logger.info(f"YOLO 추론 완료 ({camera_id}): {len(detections)}개 컴포넌트 검출")
            except Exception as yolo_error:
                logger.error(f"YOLO 추론 실패: {yolo_error}")
                # 추론 실패 시 더미 값 사용
                defect_type = "정상"
                confidence = 0.0
                boxes = []
        else:
            # 모델 미로드 시 더미 응답
            logger.warning("YOLO 모델이 로드되지 않음 - 더미 결과 반환")
            defect_type = "정상"
            confidence = 0.95
            boxes = []

        # 5. 컴포넌트 위치 검증
        verification_result = None
        component_verifier = component_verifier_left if camera_id == 'left' else component_verifier_right

        if component_verifier is not None and detections:
            try:
                verification_result = component_verifier.verify_components(detections, debug=False)

                # 치명적 불량 판정
                is_critical, reason = component_verifier.is_critical_defect(verification_result)

                if is_critical:
                    defect_type = "폐기"  # 치명적 불량
                    logger.warning(f"치명적 불량 검출 ({camera_id}): {reason}")
                elif verification_result['summary']['misplaced_count'] > 0:
                    defect_type = "부품불량"  # 위치 오류
                    logger.info(f"위치 오류 검출 ({camera_id}): {verification_result['summary']['misplaced_count']}개")
                elif verification_result['summary']['missing_count'] > 0:
                    defect_type = "부품불량"  # 누락
                    logger.info(f"누락 컴포넌트 검출 ({camera_id}): {verification_result['summary']['missing_count']}개")
                else:
                    defect_type = "정상"
                    logger.info(f"컴포넌트 검증 통과 ({camera_id}): 모두 정상")

                # 검증 결과에 판정 정보 추가
                verification_result['is_critical'] = is_critical
                verification_result['reason'] = reason

            except Exception as verify_error:
                logger.error(f"컴포넌트 검증 중 오류 ({camera_id}): {verify_error}")
                verification_result = {'error': str(verify_error)}
        else:
            logger.debug(f"컴포넌트 검증 비활성화 ({camera_id}): 기준 데이터 또는 검출 결과 없음")

        # 6. GPIO 핀 결정
        gpio_pin = get_gpio_pin(defect_type)

        # 7. 추론 시간 계산
        inference_time_ms = (time.time() - start_time) * 1000

        # 8. 데이터베이스 저장
        try:
            inspection_id = db.insert_inspection(
                camera_id=camera_id,
                defect_type=defect_type,
                confidence=confidence,
                boxes=boxes,
                gpio_pin=gpio_pin,
                image_path=None  # 이미지 저장 기능 추가 시 경로 지정
            )
            logger.info(f"검사 이력 저장 완료 (ID: {inspection_id})")
        except Exception as db_error:
            logger.warning(f"데이터베이스 저장 실패 (추론은 계속 진행): {db_error}")
            # DB 저장 실패해도 추론 결과는 반환

        # 9. 응답 생성
        response = {
            'status': 'ok',
            'camera_id': camera_id,
            'defect_type': defect_type,
            'confidence': confidence,
            'gpio_pin': gpio_pin,
            'inference_time_ms': round(inference_time_ms, 2),
            'timestamp': datetime.now().isoformat()
        }

        # 상세 정보 추가 (선택적)
        if alignment_info:
            response['alignment'] = alignment_info

        if detections:
            response['detections'] = detections

        if verification_result:
            response['verification'] = verification_result

        logger.info(f"추론 완료: {camera_id} → {defect_type} (confidence: {confidence:.2f}, GPIO: {gpio_pin}, time: {inference_time_ms:.1f}ms)")
        return jsonify(response)

    except Exception as e:
        logger.error(f"추론 실패: {str(e)}", exc_info=True)
        return jsonify({
            'status': 'error',
            'error': str(e)
        }), 500


@app.route('/predict_dual', methods=['POST'])
def predict_dual():
    """
    양면 동시 추론 (좌측 + 우측)

    Request JSON:
        {
            "left_image": "base64_encoded_jpeg_image",
            "right_image": "base64_encoded_jpeg_image"
        }

    Response JSON:
        {
            "status": "ok",
            "final_defect_type": "납땜불량",
            "final_confidence": 0.92,
            "left_result": {...},
            "right_result": {...},
            "gpio_signal": {
                "pin": 27,
                "duration_ms": 300
            },
            "robot_command": {
                "category": "SOLDER_DEFECT",
                "slot": 1
            }
        }
    """
    global latest_debug_image  # 디버그 이미지 캐시 (실시간 뷰어용)
    start_time = time.time()

    try:
        # 1. 요청 데이터 검증
        data = request.get_json()
        if not data:
            logger.error("요청 데이터가 비어있음")
            return jsonify({
                'status': 'error',
                'error': 'Request body is empty'
            }), 400

        left_image = data.get('left_image')
        right_image = data.get('right_image')

        if not left_image or not right_image:
            logger.error(f"필수 필드 누락: left_image={'있음' if left_image else '없음'}, right_image={'있음' if right_image else '없음'}")
            return jsonify({
                'status': 'error',
                'error': 'Missing required fields: left_image, right_image'
            }), 400

        # 2. 좌측 프레임 처리
        try:
            left_bytes = base64.b64decode(left_image)
            left_nparr = np.frombuffer(left_bytes, np.uint8)
            left_frame = cv2.imdecode(left_nparr, cv2.IMREAD_COLOR)

            if left_frame is None or left_frame.size == 0:
                raise ValueError("좌측 프레임 디코딩 실패")

            logger.info(f"좌측 프레임 수신 성공 (shape: {left_frame.shape})")
        except Exception as e:
            logger.error(f"좌측 이미지 처리 실패: {e}")
            return jsonify({
                'status': 'error',
                'error': f'Failed to process left image: {str(e)}'
            }), 400

        # 3. 우측 프레임 처리
        try:
            right_bytes = base64.b64decode(right_image)
            right_nparr = np.frombuffer(right_bytes, np.uint8)
            right_frame = cv2.imdecode(right_nparr, cv2.IMREAD_COLOR)

            if right_frame is None or right_frame.size == 0:
                raise ValueError("우측 프레임 디코딩 실패")

            logger.info(f"우측 프레임 수신 성공 (shape: {right_frame.shape})")
        except Exception as e:
            logger.error(f"우측 이미지 처리 실패: {e}")
            return jsonify({
                'status': 'error',
                'error': f'Failed to process right image: {str(e)}'
            }), 400

        # 4. PCB 정렬 및 AI 추론
        alignment_time_start = time.time()

        # 4-1. PCB 외곽선 검출 (마운팅 홀은 제외)
        global latest_debug_image

        # 좌측 PCB 외곽선 검출
        if pcb_aligner_left is not None:
            try:
                left_corners = pcb_aligner_left.detect_pcb_edges(left_frame, debug=True)

                debug_frame = left_frame.copy()
                if left_corners is not None:
                    # 외곽선 그리기 (cyan)
                    cv2.polylines(debug_frame, [left_corners], True, (255, 255, 0), 2)

                    # 코너 포인트 그리기 (빨간색)
                    for i, corner in enumerate(left_corners):
                        cv2.circle(debug_frame, tuple(corner), 8, (0, 0, 255), -1)
                        cv2.putText(debug_frame, f"{i+1}", (corner[0]+10, corner[1]+10),
                                   cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 255, 255), 2)

                    logger.info(f"✅ 좌측 PCB 외곽선 검출 성공")
                else:
                    logger.warning(f"⚠️  좌측 PCB 외곽선 검출 실패")
                    cv2.putText(debug_frame, "Left PCB contour detection failed", (10, 30),
                               cv2.FONT_HERSHEY_SIMPLEX, 0.7, (0, 0, 255), 2)

                latest_debug_image = debug_frame
            except Exception as e:
                logger.error(f"좌측 PCB 외곽선 검출 중 오류: {e}")
                latest_debug_image = left_frame.copy()
        else:
            latest_debug_image = left_frame.copy()

        # 정렬 없이 원본 프레임 사용
        left_aligned_frame = left_frame
        left_alignment_info = {'method': 'disabled', 'success': False, 'message': 'PCB alignment disabled'}

        # 4-2. 우측도 동일하게 처리
        right_aligned_frame = right_frame
        right_alignment_info = {'method': 'disabled', 'success': False, 'message': 'PCB alignment disabled'}

        alignment_time = (time.time() - alignment_time_start) * 1000  # ms
        logger.info(f"PCB 정렬 완료 (소요 시간: {alignment_time:.2f}ms)")

        # 4-3. YOLO 추론 (정렬된 프레임)
        inference_time_start = time.time()

        left_detections = []
        right_detections = []

        if yolo_model is not None:
            # 좌측 YOLO 추론
            left_yolo_results = yolo_model.predict(left_aligned_frame, conf=0.25, verbose=False)
            if len(left_yolo_results) > 0 and len(left_yolo_results[0].boxes) > 0:
                boxes = left_yolo_results[0].boxes
                for box in boxes:
                    x1, y1, x2, y2 = box.xyxy[0].cpu().numpy()
                    cx, cy = (x1 + x2) / 2, (y1 + y2) / 2
                    class_id = int(box.cls[0])
                    class_name = yolo_model.names[class_id]
                    confidence = float(box.conf[0])

                    left_detections.append({
                        'class_name': class_name,
                        'bbox': [float(x1), float(y1), float(x2), float(y2)],
                        'center': [float(cx), float(cy)],
                        'confidence': confidence
                    })

            # 우측 YOLO 추론
            right_yolo_results = yolo_model.predict(right_aligned_frame, conf=0.25, verbose=False)
            if len(right_yolo_results) > 0 and len(right_yolo_results[0].boxes) > 0:
                boxes = right_yolo_results[0].boxes
                for box in boxes:
                    x1, y1, x2, y2 = box.xyxy[0].cpu().numpy()
                    cx, cy = (x1 + x2) / 2, (y1 + y2) / 2
                    class_id = int(box.cls[0])
                    class_name = yolo_model.names[class_id]
                    confidence = float(box.conf[0])

                    right_detections.append({
                        'class_name': class_name,
                        'bbox': [float(x1), float(y1), float(x2), float(y2)],
                        'center': [float(cx), float(cy)],
                        'confidence': confidence
                    })

        inference_time = (time.time() - inference_time_start) * 1000  # ms
        logger.info(f"YOLO 추론 완료 (좌: {len(left_detections)}개, 우: {len(right_detections)}개, 소요 시간: {inference_time:.2f}ms)")

        # 4-4. 컴포넌트 위치 검증
        verification_time_start = time.time()

        left_verification = None
        right_verification = None

        if component_verifier_left is not None and left_alignment_info['success']:
            left_verification = component_verifier_left.verify_components(left_detections, debug=False)
            logger.info(f"좌측 컴포넌트 검증 완료: 정상 {left_verification['summary']['correct_count']}개, "
                       f"위치오류 {left_verification['summary']['misplaced_count']}개, "
                       f"누락 {left_verification['summary']['missing_count']}개")

        if component_verifier_right is not None and right_alignment_info['success']:
            right_verification = component_verifier_right.verify_components(right_detections, debug=False)
            logger.info(f"우측 컴포넌트 검증 완료: 정상 {right_verification['summary']['correct_count']}개, "
                       f"위치오류 {right_verification['summary']['misplaced_count']}개, "
                       f"누락 {right_verification['summary']['missing_count']}개")

        verification_time = (time.time() - verification_time_start) * 1000  # ms
        logger.info(f"컴포넌트 검증 완료 (소요 시간: {verification_time:.2f}ms)")

        # 4-5. 최종 불량 판정
        final_defect_type = "정상"
        final_confidence = 0.95

        # 치명적 불량 체크
        if left_verification is not None:
            is_critical, reason = component_verifier_left.is_critical_defect(left_verification)
            if is_critical:
                final_defect_type = "부품불량"
                final_confidence = 0.85
                logger.warning(f"좌측 치명적 불량: {reason}")

        if right_verification is not None:
            is_critical, reason = component_verifier_right.is_critical_defect(right_verification)
            if is_critical:
                final_defect_type = "부품불량"
                final_confidence = 0.85
                logger.warning(f"우측 치명적 불량: {reason}")

        # 결과 구조화
        left_result = {
            'defect_type': final_defect_type if left_verification and component_verifier_left.is_critical_defect(left_verification)[0] else '정상',
            'confidence': final_confidence,
            'boxes': left_detections,
            'alignment': left_alignment_info,
            'verification': left_verification
        }

        right_result = {
            'defect_type': final_defect_type if right_verification and component_verifier_right.is_critical_defect(right_verification)[0] else '정상',
            'confidence': final_confidence,
            'boxes': right_detections,
            'alignment': right_alignment_info,
            'verification': right_verification
        }

        # 5. GPIO 핀 결정
        gpio_pin = get_gpio_pin(final_defect_type)

        # 6. 박스 카테고리 및 슬롯 결정
        category = defect_type_to_category(final_defect_type)

        # 현재 박스 상태 조회
        try:
            box_status = db.get_box_status(category)
            if box_status and not box_status['is_full']:
                current_slot = box_status['current_slot']
                # 박스 상태 업데이트 (슬롯 증가)
                db.update_box_status(category, increment=True)
                logger.info(f"박스 슬롯 업데이트: {category} (slot {current_slot} → {current_slot + 1})")
            else:
                current_slot = 0
                logger.warning(f"박스 가득 참: {category}")
        except Exception as db_error:
            logger.warning(f"박스 상태 조회/업데이트 실패: {db_error}")
            current_slot = 0

        # 7. 양측 검사 이력 저장
        try:
            # 좌측 검사 이력
            left_inspection_id = db.insert_inspection(
                camera_id='left',
                defect_type=left_result['defect_type'],
                confidence=left_result['confidence'],
                boxes=left_result['boxes'],
                gpio_pin=gpio_pin
            )

            # 우측 검사 이력
            right_inspection_id = db.insert_inspection(
                camera_id='right',
                defect_type=right_result['defect_type'],
                confidence=right_result['confidence'],
                boxes=right_result['boxes'],
                gpio_pin=gpio_pin
            )

            logger.info(f"양면 검사 이력 저장 완료 (left: {left_inspection_id}, right: {right_inspection_id})")
        except Exception as db_error:
            logger.warning(f"데이터베이스 저장 실패: {db_error}")

        # 8. 추론 시간 계산
        inference_time_ms = (time.time() - start_time) * 1000

        # 9. 응답 생성
        response = {
            'status': 'ok',
            'final_defect_type': final_defect_type,
            'final_confidence': final_confidence,
            'left_result': left_result,
            'right_result': right_result,
            'gpio_signal': {
                'pin': gpio_pin,
                'duration_ms': 300
            },
            'robot_command': {
                'category': category,
                'slot': current_slot
            },
            'inference_time_ms': round(inference_time_ms, 2),
            'timestamp': datetime.now().isoformat()
        }

        logger.info(f"양면 추론 완료: {final_defect_type} (confidence: {final_confidence:.2f}, GPIO: {gpio_pin}, slot: {current_slot}, time: {inference_time_ms:.1f}ms)")
        return jsonify(response)

    except Exception as e:
        logger.error(f"양면 추론 실패: {str(e)}", exc_info=True)
        return jsonify({
            'status': 'error',
            'error': str(e)
        }), 500


@app.route('/box_status', methods=['GET'])
def get_box_status():
    """
    박스 상태 조회 (C# WinForms 모니터링용)

    Response JSON:
        {
            "status": "ok",
            "boxes": [
                {
                    "box_id": "NORMAL",
                    "current_slot": 3,
                    "max_slots": 5,
                    "is_full": false,
                    "total_pcb_count": 15
                },
                ...
            ],
            "summary": {
                "total_boxes": 3,
                "full_boxes": 0,
                "system_stopped": false
            }
        }
    """
    try:
        # MySQL에서 박스 상태 조회
        boxes = db.get_all_box_status()

        if not boxes:
            logger.warning("박스 상태 데이터가 없음 - 초기 데이터 확인 필요")
            return jsonify({
                'status': 'error',
                'error': 'No box status data found in database'
            }), 404

        # summary 통계 계산
        summary = {
            'total_boxes': len(boxes),
            'full_boxes': sum(1 for box in boxes if box['is_full']),
            'empty_boxes': sum(1 for box in boxes if box['current_slot'] == 0),
            'system_stopped': all(box['is_full'] for box in boxes)
        }

        logger.info(f"박스 상태 조회 완료: {summary['total_boxes']}개 박스, {summary['full_boxes']}개 가득참")

        return jsonify({
            'status': 'ok',
            'boxes': boxes,
            'summary': summary
        })

    except Exception as e:
        logger.error(f"박스 상태 조회 실패: {str(e)}", exc_info=True)
        return jsonify({
            'status': 'error',
            'error': str(e)
        }), 500


@app.route('/viewer', methods=['GET'])
def viewer():
    """실시간 카메라 뷰어 페이지"""
    html_template = """
    <!DOCTYPE html>
    <html lang="ko">
    <head>
        <meta charset="UTF-8">
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <title>PCB 검사 실시간 뷰어</title>
        <style>
            * { margin: 0; padding: 0; box-sizing: border-box; }
            body {
                font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                background: linear-gradient(135deg, #1e3c72 0%, #2a5298 100%);
                color: #fff;
                padding: 20px;
            }
            .container {
                max-width: 1400px;
                margin: 0 auto;
            }
            h1 {
                text-align: center;
                margin-bottom: 30px;
                font-size: 2.5em;
                text-shadow: 2px 2px 4px rgba(0,0,0,0.3);
            }
            .cameras {
                display: grid;
                grid-template-columns: repeat(auto-fit, minmax(500px, 1fr));
                gap: 30px;
                margin-bottom: 30px;
            }
            .camera-box {
                background: rgba(255,255,255,0.1);
                border-radius: 15px;
                padding: 20px;
                backdrop-filter: blur(10px);
                box-shadow: 0 8px 32px rgba(0,0,0,0.3);
            }
            .camera-title {
                font-size: 1.5em;
                margin-bottom: 15px;
                text-align: center;
                font-weight: bold;
            }
            .camera-stream {
                width: 100%;
                border-radius: 10px;
                background: #000;
                min-height: 400px;
                display: flex;
                align-items: center;
                justify-content: center;
                font-size: 1.2em;
                color: #aaa;
            }
            .camera-stream img {
                width: 100%;
                border-radius: 10px;
            }
            .camera-info {
                margin-top: 15px;
                padding: 15px;
                background: rgba(0,0,0,0.3);
                border-radius: 10px;
            }
            .info-row {
                display: flex;
                justify-content: space-between;
                margin: 8px 0;
                font-size: 1.1em;
            }
            .info-label {
                font-weight: bold;
                color: #aaf;
            }
            .status-ok { color: #4f4; }
            .status-defect { color: #f44; }
            .footer {
                text-align: center;
                margin-top: 20px;
                opacity: 0.7;
            }
        </style>
    </head>
    <body>
        <div class="container">
            <h1>🔍 PCB 검사 실시간 뷰어</h1>

            <div class="cameras">
                <div class="camera-box">
                    <div class="camera-title">📷 좌측 카메라 (Left)</div>
                    <div class="camera-stream">
                        <img id="left-stream" src="/video_feed/left" alt="좌측 카메라" onerror="this.style.display='none'; this.parentElement.innerText='카메라 연결 대기 중...'">
                    </div>
                    <div class="camera-info">
                        <div class="info-row">
                            <span class="info-label">판정:</span>
                            <span id="left-defect" class="status-ok">-</span>
                        </div>
                        <div class="info-row">
                            <span class="info-label">신뢰도:</span>
                            <span id="left-confidence">-</span>
                        </div>
                        <div class="info-row">
                            <span class="info-label">처리시간:</span>
                            <span id="left-time">-</span>
                        </div>
                    </div>
                </div>

                <div class="camera-box">
                    <div class="camera-title">📷 우측 카메라 (Right)</div>
                    <div class="camera-stream">
                        <img id="right-stream" src="/video_feed/right" alt="우측 카메라" onerror="this.style.display='none'; this.parentElement.innerText='카메라 연결 대기 중...'">
                    </div>
                    <div class="camera-info">
                        <div class="info-row">
                            <span class="info-label">판정:</span>
                            <span id="right-defect" class="status-ok">-</span>
                        </div>
                        <div class="info-row">
                            <span class="info-label">신뢰도:</span>
                            <span id="right-confidence">-</span>
                        </div>
                        <div class="info-row">
                            <span class="info-label">처리시간:</span>
                            <span id="right-time">-</span>
                        </div>
                    </div>
                </div>
            </div>

            <div class="footer">
                ✨ PCB 불량 검사 시스템 v1.0 | Flask Server | Real-time Streaming
            </div>
        </div>

        <script>
            // 1초마다 결과 정보 업데이트
            function updateResults() {
                fetch('/api/latest_results')
                    .then(res => res.json())
                    .then(data => {
                        // 좌측 카메라
                        if (data.left && data.left.defect_type) {
                            document.getElementById('left-defect').textContent = data.left.defect_type;
                            document.getElementById('left-defect').className = data.left.defect_type === '정상' ? 'status-ok' : 'status-defect';
                            document.getElementById('left-confidence').textContent = (data.left.confidence * 100).toFixed(1) + '%';
                            document.getElementById('left-time').textContent = data.left.inference_time_ms.toFixed(1) + 'ms';
                        }

                        // 우측 카메라
                        if (data.right && data.right.defect_type) {
                            document.getElementById('right-defect').textContent = data.right.defect_type;
                            document.getElementById('right-defect').className = data.right.defect_type === '정상' ? 'status-ok' : 'status-defect';
                            document.getElementById('right-confidence').textContent = (data.right.confidence * 100).toFixed(1) + '%';
                            document.getElementById('right-time').textContent = data.right.inference_time_ms.toFixed(1) + 'ms';
                        }
                    })
                    .catch(err => console.error('결과 업데이트 실패:', err));
            }

            // 1초마다 업데이트
            setInterval(updateResults, 1000);
            updateResults();
        </script>
    </body>
    </html>
    """
    return render_template_string(html_template)


@app.route('/video_feed/<camera_id>', methods=['GET'])
def video_feed(camera_id):
    """MJPEG 스트림 제공"""
    def generate():
        # 대기 화면용 더미 프레임 생성 (회색 배경 + 텍스트)
        dummy_frame = np.zeros((480, 640, 3), dtype=np.uint8)
        dummy_frame[:] = (50, 50, 50)  # 어두운 회색
        cv2.putText(dummy_frame, f"Waiting for {camera_id} camera...",
                   (100, 240), cv2.FONT_HERSHEY_SIMPLEX, 0.8, (255, 255, 255), 2)

        while True:
            with frame_lock:
                frame = latest_frames.get(camera_id)

            # 프레임이 없으면 더미 프레임 사용
            if frame is None:
                frame = dummy_frame

            # JPEG 인코딩 - Baseline 형식 강제 (C# Image.FromStream() 호환성)
            encode_params = [
                cv2.IMWRITE_JPEG_QUALITY, 85,        # 화질 85%
                cv2.IMWRITE_JPEG_PROGRESSIVE, 0,     # Progressive JPEG 비활성화 (Baseline 강제)
                cv2.IMWRITE_JPEG_OPTIMIZE, 1         # 허프만 테이블 최적화
            ]
            ret, buffer = cv2.imencode('.jpg', frame, encode_params)
            if ret:
                frame_bytes = buffer.tobytes()
                yield (b'--frame\r\n'
                       b'Content-Type: image/jpeg\r\n\r\n' + frame_bytes + b'\r\n')

            time.sleep(0.1)  # 10 FPS (깜빡거림 방지)

    return Response(generate(), mimetype='multipart/x-mixed-replace; boundary=frame')




@app.route('/api/latest_results', methods=['GET'])
def get_latest_results():
    """최신 추론 결과 반환 (JSON)"""
    with frame_lock:
        results = {
            'left': latest_results.get('left', {}),
            'right': latest_results.get('right', {})
        }
    return jsonify(results)


# 유틸리티 함수
def crop_to_square(frame, target_size=640):
    """
    프레임을 정사각형으로 중앙 크롭 후 리사이즈

    대부분의 웹캠은 640x480 (4:3) 해상도를 사용하므로,
    중앙 480x480을 크롭한 후 640x640으로 리사이즈합니다.

    Args:
        frame: 입력 프레임 (예: 640x480)
        target_size: 목표 크기 (기본값: 640)

    Returns:
        정사각형 프레임 (target_size x target_size)

    Example:
        640x480 입력 → 중앙 480x480 크롭 → 640x640 리사이즈
        (좌우 80픽셀씩 제거)
    """
    height, width = frame.shape[:2]

    # 이미 정사각형이면 그대로 리사이즈
    if height == width:
        if height != target_size:
            return cv2.resize(frame, (target_size, target_size), interpolation=cv2.INTER_LINEAR)
        return frame

    # 중앙 크롭 (짧은 쪽 기준)
    crop_size = min(height, width)

    # 중앙 시작 위치 계산
    start_x = (width - crop_size) // 2
    start_y = (height - crop_size) // 2

    # 크롭
    cropped = frame[start_y:start_y + crop_size, start_x:start_x + crop_size]

    # 리사이즈
    if crop_size != target_size:
        cropped = cv2.resize(cropped, (target_size, target_size), interpolation=cv2.INTER_LINEAR)

    return cropped


def detect_pcb_roi(frame):
    """
    초록색 PCB 자동 감지 및 내부 ROI 추출

    Args:
        frame: 원본 프레임 (BGR)

    Returns:
        roi_mask: ROI 마스크 (255=PCB 내부, 0=외부/테두리)
        pcb_bbox: PCB 바운딩 박스 (x, y, w, h) 또는 None
        roi_bbox: ROI 바운딩 박스 (x, y, w, h) 또는 None (테두리 제외)
    """
    h, w = frame.shape[:2]

    # 1. HSV 변환
    hsv = cv2.cvtColor(frame, cv2.COLOR_BGR2HSV)

    # 2. 초록색 PCB 마스크 생성
    pcb_mask = cv2.inRange(hsv, PCB_COLOR_LOWER_HSV, PCB_COLOR_UPPER_HSV)

    # 3. 노이즈 제거 (모폴로지 연산)
    kernel = np.ones((5, 5), np.uint8)
    pcb_mask = cv2.morphologyEx(pcb_mask, cv2.MORPH_CLOSE, kernel)
    pcb_mask = cv2.morphologyEx(pcb_mask, cv2.MORPH_OPEN, kernel)

    # 4. 가장 큰 컨투어 찾기 (PCB)
    contours, _ = cv2.findContours(pcb_mask, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)

    if not contours:
        # PCB 감지 실패 → 전체 프레임 사용
        roi_mask = np.ones((h, w), dtype=np.uint8) * 255
        return roi_mask, None, None

    # 가장 큰 컨투어 = PCB
    largest_contour = max(contours, key=cv2.contourArea)
    pcb_x, pcb_y, pcb_w, pcb_h = cv2.boundingRect(largest_contour)

    # 4-1. PCB가 프레임 경계에 붙어있는지 확인 ⭐⭐⭐
    # PCB 전체가 프레임에 나와야만 ROI 적용
    is_pcb_on_edge = (
        pcb_x < PCB_EDGE_THRESHOLD or  # 왼쪽 경계
        pcb_y < PCB_EDGE_THRESHOLD or  # 위쪽 경계
        pcb_x + pcb_w > w - PCB_EDGE_THRESHOLD or  # 오른쪽 경계
        pcb_y + pcb_h > h - PCB_EDGE_THRESHOLD  # 아래쪽 경계
    )

    if is_pcb_on_edge:
        # PCB가 프레임 경계에 붙어있음 → 부분적으로만 보임 → 전체 프레임 사용
        roi_mask = np.ones((h, w), dtype=np.uint8) * 255
        return roi_mask, None, None

    # 5. PCB 내부 영역만 ROI로 설정 (테두리 제외)
    margin_w = int(pcb_w * PCB_INNER_MARGIN_PERCENT)
    margin_h = int(pcb_h * PCB_INNER_MARGIN_PERCENT)

    roi_x = pcb_x + margin_w
    roi_y = pcb_y + margin_h
    roi_w = pcb_w - 2 * margin_w
    roi_h = pcb_h - 2 * margin_h

    # 경계 체크
    roi_x = max(0, roi_x)
    roi_y = max(0, roi_y)
    roi_w = min(roi_w, w - roi_x)
    roi_h = min(roi_h, h - roi_y)

    # 6. ROI 마스크 생성
    roi_mask = np.zeros((h, w), dtype=np.uint8)
    roi_mask[roi_y:roi_y+roi_h, roi_x:roi_x+roi_w] = 255

    return roi_mask, (pcb_x, pcb_y, pcb_w, pcb_h), (roi_x, roi_y, roi_w, roi_h)


def detect_motion(current_frame, previous_frame, camera_id):
    """
    프레임 차이로 모션 감지 (새 PCB 진입 감지)

    Args:
        current_frame: 현재 프레임 (컬러)
        previous_frame: 이전 프레임 (컬러)
        camera_id: 카메라 ID

    Returns:
        motion_detected: 모션 감지 여부
        motion_value: 평균 픽셀 차이
    """
    if previous_frame is None:
        return False, 0.0

    # 그레이스케일 변환
    gray_current = cv2.cvtColor(current_frame, cv2.COLOR_BGR2GRAY)
    gray_previous = cv2.cvtColor(previous_frame, cv2.COLOR_BGR2GRAY)

    # 프레임 차이 계산
    frame_diff = cv2.absdiff(gray_current, gray_previous)

    # 평균 차이 계산
    mean_diff = np.mean(frame_diff)

    # 모션 감지
    motion_detected = mean_diff > MOTION_THRESHOLD

    return motion_detected, mean_diff


def calculate_iou(box1, box2):
    """
    두 바운딩 박스의 IOU (Intersection over Union) 계산

    Args:
        box1, box2: {'x1': float, 'y1': float, 'x2': float, 'y2': float}

    Returns:
        iou (float): 0.0 ~ 1.0
    """
    # 교집합 영역 계산
    x1_inter = max(box1['x1'], box2['x1'])
    y1_inter = max(box1['y1'], box2['y1'])
    x2_inter = min(box1['x2'], box2['x2'])
    y2_inter = min(box1['y2'], box2['y2'])

    # 교집합 면적
    if x2_inter < x1_inter or y2_inter < y1_inter:
        return 0.0

    inter_area = (x2_inter - x1_inter) * (y2_inter - y1_inter)

    # 각 박스의 면적
    box1_area = (box1['x2'] - box1['x1']) * (box1['y2'] - box1['y1'])
    box2_area = (box2['x2'] - box2['x1']) * (box2['y2'] - box2['y1'])

    # 합집합 면적
    union_area = box1_area + box2_area - inter_area

    if union_area == 0:
        return 0.0

    return inter_area / union_area


def smooth_detections(camera_id, current_detections):
    """
    검출 결과 평활화 (Temporal Smoothing)

    이전 프레임의 검출 결과를 참고하여 안정적인 검출 유지:
    - 최소 MIN_DETECTION_FRAMES 프레임 동안 검출된 객체만 표시
    - 사라진 객체는 MAX_MISSING_FRAMES 프레임 동안 유지

    Args:
        camera_id (str): 'left' or 'right'
        current_detections (list): 현재 프레임의 검출 결과 [{...}, {...}]

    Returns:
        smoothed_detections (list): 평활화된 검출 결과
    """
    global next_object_id

    with tracking_lock:
        tracked = tracked_objects[camera_id]

        # 현재 검출 결과와 추적 중인 객체 매칭
        matched_ids = set()
        new_detections = []

        for detection in current_detections:
            best_match_id = None
            best_iou = 0.0

            # 추적 중인 객체와 매칭
            for obj_id, tracked_obj in tracked.items():
                iou = calculate_iou(detection, tracked_obj['box'])

                # 같은 클래스이면서 IOU가 높으면 매칭
                if (detection['class_id'] == tracked_obj['class_id'] and
                    iou > IOU_THRESHOLD and
                    iou > best_iou):
                    best_iou = iou
                    best_match_id = obj_id

            if best_match_id is not None:
                # 기존 객체 업데이트
                tracked[best_match_id]['count'] += 1
                tracked[best_match_id]['missing'] = 0

                # 30프레임 검출 후 값 고정 (컨베이어 벨트용)
                if not tracked[best_match_id]['frozen']:
                    tracked[best_match_id]['box'] = detection
                    tracked[best_match_id]['confidence'] = detection['confidence']

                    # 30프레임 도달 시 고정
                    if tracked[best_match_id]['count'] >= FREEZE_AFTER_FRAMES:
                        tracked[best_match_id]['frozen'] = True
                        logger.info(f"[FREEZE] 객체 {best_match_id} 고정 완료 (class: {tracked[best_match_id]['class_name']}, conf: {tracked[best_match_id]['confidence']:.2f})")

                # 이미 고정된 객체는 box와 confidence 업데이트 안 함

                matched_ids.add(best_match_id)
            else:
                # 새로운 객체 추가
                new_id = next_object_id
                next_object_id += 1

                tracked[new_id] = {
                    'box': detection,
                    'class_id': detection['class_id'],
                    'class_name': detection['class_name'],
                    'confidence': detection['confidence'],
                    'count': 1,        # 검출 횟수
                    'missing': 0,      # 미검출 횟수
                    'frozen': False    # 30프레임 후 고정 여부 ⭐
                }
                matched_ids.add(new_id)

        # 매칭되지 않은 객체는 missing 카운트 증가
        to_remove = []
        for obj_id in list(tracked.keys()):
            if obj_id not in matched_ids:
                # frozen 객체는 절대 삭제하지 않음 (영구 보존)
                if tracked[obj_id]['frozen']:
                    continue  # missing 카운트도 증가 안 함

                tracked[obj_id]['missing'] += 1

                # MAX_MISSING_FRAMES 초과 시 제거 (frozen 아닌 객체만)
                if tracked[obj_id]['missing'] > MAX_MISSING_FRAMES:
                    to_remove.append(obj_id)

        for obj_id in to_remove:
            del tracked[obj_id]

        # 평활화된 검출 결과 생성
        # - Frozen 객체: 무조건 표시 (고정된 박스는 절대 사라지면 안 됨) ⭐⭐⭐
        # - 일반 객체: MIN_DETECTION_FRAMES 이상 검출된 객체만 표시
        smoothed = []

        # obj_id로 정렬하여 항상 같은 순서로 그리기 (깜빡거림 방지)
        for obj_id in sorted(tracked.keys()):
            tracked_obj = tracked[obj_id]
            # Frozen 객체는 count와 관계없이 무조건 표시 ⭐
            if tracked_obj['frozen'] or tracked_obj['count'] >= MIN_DETECTION_FRAMES:
                smoothed.append({
                    'x1': tracked_obj['box']['x1'],
                    'y1': tracked_obj['box']['y1'],
                    'x2': tracked_obj['box']['x2'],
                    'y2': tracked_obj['box']['y2'],
                    'confidence': tracked_obj['confidence'],
                    'class_id': tracked_obj['class_id'],
                    'class_name': tracked_obj['class_name']
                })

        # 모든 객체가 frozen 상태인지 확인 (완전 정지 모드) ⭐⭐⭐
        if len(tracked) > 0:
            all_frozen = all(obj['frozen'] for obj in tracked.values())
            # 모든 객체 frozen + 충분한 안정 프레임 → 완전 정지 모드
            if all_frozen and stable_frame_count[camera_id] >= STABLE_FRAMES_FOR_FREEZE and not camera_frozen_state[camera_id]:
                camera_frozen_state[camera_id] = True
                logger.info(f"🔒 [{camera_id}] 완전 정지 모드 활성화 (객체: {len(tracked)}개, 안정 프레임: {stable_frame_count[camera_id]})")

        return smoothed


def draw_bounding_boxes(frame, boxes_data, pcb_bbox=None, roi_bbox=None):
    """
    프레임에 바운딩 박스 및 ROI 영역 그리기

    Args:
        frame: OpenCV 이미지 (numpy array)
        boxes_data: 바운딩 박스 정보 리스트
        pcb_bbox: PCB 바운딩 박스 (x, y, w, h) - 파란색 점선
        roi_bbox: ROI 바운딩 박스 (x, y, w, h) - 노란색 실선

    Returns:
        annotated_frame: 바운딩 박스가 그려진 이미지
    """
    annotated_frame = frame.copy()

    # ROI 영역 표시 (먼저 그려서 뒤에 있도록)
    if pcb_bbox is not None:
        px, py, pw, ph = pcb_bbox
        # PCB 전체 영역 (파란색 점선)
        cv2.rectangle(annotated_frame, (px, py), (px+pw, py+ph), (255, 0, 0), 2, cv2.LINE_AA)
        cv2.putText(annotated_frame, "PCB", (px+5, py+20), cv2.FONT_HERSHEY_SIMPLEX, 0.6, (255, 0, 0), 2)

    if roi_bbox is not None:
        rx, ry, rw, rh = roi_bbox
        # ROI 내부 영역 (노란색 실선)
        cv2.rectangle(annotated_frame, (rx, ry), (rx+rw, ry+rh), (0, 255, 255), 2, cv2.LINE_AA)
        cv2.putText(annotated_frame, "ROI (Detection Area)", (rx+5, ry+20), cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 255, 255), 2)

    # 클래스별 색상 (12개 클래스)
    colors = [
        (255, 0, 0),      # Electrolytic capacitor - 빨강
        (0, 255, 0),      # IC - 초록
        (0, 0, 255),      # cds - 파랑
        (255, 255, 0),    # ceramic capacitor - 노랑
        (255, 0, 255),    # diode - 자홍
        (0, 255, 255),    # led - 청록
        (128, 0, 0),      # pinheader - 어두운 빨강
        (0, 128, 0),      # pinsocket - 어두운 초록
        (0, 0, 128),      # resistor - 어두운 파랑
        (128, 128, 0),    # switch - 올리브
        (128, 0, 128),    # transistor - 보라
        (0, 128, 128),    # zener diode - 청록
    ]

    for box in boxes_data:
        x1 = int(box['x1'])
        y1 = int(box['y1'])
        x2 = int(box['x2'])
        y2 = int(box['y2'])
        conf = box['confidence']
        class_id = box['class_id']
        class_name = box['class_name']

        # 색상 선택
        color = colors[class_id % len(colors)]

        # 바운딩 박스 그리기
        cv2.rectangle(annotated_frame, (x1, y1), (x2, y2), color, 2)

        # 레이블 배경
        label = f"{class_name} {conf:.2f}"
        font = cv2.FONT_HERSHEY_SIMPLEX
        font_scale = 0.5
        thickness = 1
        (text_width, text_height), baseline = cv2.getTextSize(label, font, font_scale, thickness)

        # 레이블 배경 그리기
        cv2.rectangle(annotated_frame,
                     (x1, y1 - text_height - 10),
                     (x1 + text_width, y1),
                     color, -1)

        # 레이블 텍스트 그리기
        cv2.putText(annotated_frame, label,
                   (x1, y1 - 5),
                   font, font_scale, (255, 255, 255), thickness)

    return annotated_frame


def parse_yolo_results(results):
    """
    YOLO 추론 결과 파싱

    Returns:
        defect_type (str): 불량 유형 ("정상" | "부품불량" | "납땜불량" | "폐기")
        confidence (float): 평균 신뢰도 (0.0 ~ 1.0)
        boxes (list): 바운딩 박스 정보 리스트
    """
    if results is None or len(results) == 0:
        return "정상", 1.0, []

    result = results[0]  # 첫 번째 결과 (단일 이미지)
    boxes_data = []

    # 검출된 객체가 없으면 정상
    if result.boxes is None or len(result.boxes) == 0:
        return "정상", 1.0, []

    # 바운딩 박스 정보 추출
    for box in result.boxes:
        x1, y1, x2, y2 = box.xyxy[0].cpu().numpy()
        conf = float(box.conf[0].cpu().numpy())
        cls = int(box.cls[0].cpu().numpy())
        class_name = result.names[cls]

        boxes_data.append({
            'x1': float(x1),
            'y1': float(y1),
            'x2': float(x2),
            'y2': float(y2),
            'confidence': conf,
            'class_id': cls,
            'class_name': class_name
        })

    # 평균 신뢰도 계산
    if boxes_data:
        avg_confidence = sum(b['confidence'] for b in boxes_data) / len(boxes_data)
    else:
        avg_confidence = 1.0

    # 불량 판정 로직 (현재는 단순 버전)
    # TODO: 더 정교한 판정 로직 필요 (누락된 부품, 잘못된 위치 등)
    if len(boxes_data) > 0:
        # 일단 검출된 객체가 있으면 정상으로 판정
        # 향후 기준 PCB와 비교하여 누락/추가 부품 검출 필요
        defect_type = "정상"
    else:
        defect_type = "정상"

    return defect_type, avg_confidence, boxes_data


def get_gpio_pin(defect_type):
    """불량 유형에 따른 GPIO 핀 번호 반환"""
    gpio_map = {
        '정상': 23,
        '부품불량': 17,
        '납땜불량': 27,
        '폐기': 22
    }
    return gpio_map.get(defect_type, 23)


def defect_type_to_category(defect_type):
    """불량 유형을 박스 카테고리로 변환"""
    category_map = {
        '정상': 'NORMAL',
        '부품불량': 'COMPONENT_DEFECT',
        '납땜불량': 'SOLDER_DEFECT',
        '폐기': 'DISCARD'
    }
    return category_map.get(defect_type, 'NORMAL')


# ===================================
# SocketIO 이벤트 핸들러 (WebSocket)
# ===================================

@socketio.on('connect')
def handle_connect():
    """클라이언트 연결 이벤트"""
    logger.info(f"[WebSocket] 클라이언트 연결: {request.sid}")
    emit('connection_response', {'status': 'connected', 'message': 'Flask SocketIO 서버에 연결되었습니다'})


@socketio.on('disconnect')
def handle_disconnect():
    """
    클라이언트 연결 종료 이벤트
    리소스 정리 및 소켓 누수 방지 ⭐
    """
    session_id = request.sid
    logger.info(f"[WebSocket] 클라이언트 연결 종료: {session_id}")

    try:
        # 1. 세션 관련 리소스 정리
        # (필요시 세션별 추적 데이터 삭제)

        # 2. threading 모드에서는 소켓 정리 자동 처리
        # eventlet.sleep(0)  # (eventlet 모드 전용, threading에서는 불필요)

        logger.info(f"[WebSocket] 세션 {session_id} 리소스 정리 완료")
    except Exception as e:
        logger.error(f"[WebSocket] disconnect 처리 중 오류: {e}", exc_info=True)


@socketio.on('request_frame')
def handle_frame_request(data):
    """
    프레임 요청 이벤트
    클라이언트가 100ms 간격으로 요청

    Args:
        data (dict): {'camera_id': 'left' or 'right'}
    """
    try:
        camera_id = data.get('camera_id')

        if camera_id not in ['left', 'right']:
            logger.warning(f"[WebSocket] 잘못된 camera_id: {camera_id}")
            emit('error', {'message': 'Invalid camera_id. Use "left" or "right"'})
            return

        # 캐시된 JPEG 가져오기 (WebSocket 성능 최적화) ⭐
        with frame_lock:
            frame_base64 = latest_frames_jpeg.get(camera_id)

        # 캐시가 없으면 더미 프레임 생성 및 인코딩
        if frame_base64 is None:
            dummy_frame = np.zeros((640, 640, 3), dtype=np.uint8)
            dummy_frame[:] = (50, 50, 50)  # 어두운 회색
            cv2.putText(dummy_frame, f"Waiting for {camera_id} camera...",
                       (100, 320), cv2.FONT_HERSHEY_SIMPLEX, 0.8, (255, 255, 255), 2)

            # 더미 프레임 JPEG 인코딩
            encode_params = [
                cv2.IMWRITE_JPEG_QUALITY, 85,
                cv2.IMWRITE_JPEG_PROGRESSIVE, 0,
                cv2.IMWRITE_JPEG_OPTIMIZE, 1
            ]
            ret, buffer = cv2.imencode('.jpg', dummy_frame, encode_params)

            if not ret:
                logger.error(f"[WebSocket] JPEG 인코딩 실패: {camera_id}")
                emit('error', {'message': 'JPEG encoding failed'})
                return

            frame_base64 = base64.b64encode(buffer.tobytes()).decode('utf-8')
            logger.debug(f"[WebSocket] {camera_id} 프레임 없음 → 더미 프레임 전송 (640x640)")
        else:
            logger.debug(f"[WebSocket] {camera_id} 캐시된 JPEG 전송 (인코딩 생략)")

        # NOTE: 'frame' 필드명을 'frameData'로 변경하여 Flask-SocketIO의 binary 자동 변환 방지
        emit('frame_data', {
            'camera_id': camera_id,
            'frameData': frame_base64,  # 필드명 변경: frame → frameData
            'timestamp': time.time(),
            'size': len(frame_base64)  # Base64 문자열 길이
        })

        logger.debug(f"[WebSocket] 프레임 전송 완료: {camera_id} ({len(frame_base64)} bytes)")

    except Exception as e:
        logger.error(f"[WebSocket] 프레임 요청 처리 실패: {str(e)}", exc_info=True)
        emit('error', {'message': f'Frame request failed: {str(e)}'})


# ============================================================
# 디버그 뷰어 엔드포인트 (실시간 ROI 및 구멍 검출 시각화)
# ============================================================

@app.route('/debug_viewer')
def debug_viewer():
    """디버그 뷰어 HTML 페이지"""
    html_content = """
<!DOCTYPE html>
<html lang="ko">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>PCB 정렬 디버그 뷰어</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        body {
            background: #1a1a1a;
            color: #fff;
            font-family: 'Consolas', 'Monaco', monospace;
            padding: 20px;
        }
        .container {
            max-width: 1200px;
            margin: 0 auto;
        }
        header {
            text-align: center;
            margin-bottom: 30px;
            padding: 20px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            border-radius: 10px;
        }
        h1 {
            font-size: 2em;
            margin-bottom: 10px;
        }
        .subtitle {
            font-size: 0.9em;
            opacity: 0.9;
        }
        .image-container {
            background: #2a2a2a;
            padding: 20px;
            border-radius: 10px;
            margin-bottom: 20px;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.3);
        }
        img {
            width: 100%;
            height: auto;
            border: 3px solid #333;
            border-radius: 5px;
            display: block;
        }
        .legend {
            display: flex;
            justify-content: center;
            gap: 30px;
            margin: 20px 0;
            flex-wrap: wrap;
        }
        .legend-item {
            display: flex;
            align-items: center;
            gap: 10px;
            padding: 10px 20px;
            background: #2a2a2a;
            border-radius: 5px;
        }
        .color-box {
            width: 30px;
            height: 30px;
            border: 3px solid;
            border-radius: 3px;
        }
        .info {
            background: #2a2a2a;
            padding: 20px;
            margin: 10px 0;
            border-radius: 10px;
            border-left: 4px solid #667eea;
        }
        .info p {
            margin: 5px 0;
        }
        #status {
            display: inline-block;
            padding: 5px 15px;
            background: #00ff00;
            color: #000;
            font-weight: bold;
            border-radius: 3px;
            animation: pulse 2s infinite;
        }
        @keyframes pulse {
            0%, 100% { opacity: 1; }
            50% { opacity: 0.6; }
        }
        .no-image {
            text-align: center;
            padding: 100px 20px;
            background: #2a2a2a;
            border-radius: 10px;
            color: #999;
        }
    </style>
</head>
<body>
    <div class="container">
        <header>
            <h1>🔍 PCB 정렬 디버그 뷰어</h1>
            <p class="subtitle">실시간 ROI 경계 및 마운팅 홀 검출 시각화</p>
        </header>

        <div class="legend">
            <div class="legend-item">
                <div class="color-box" style="border-color: #00ff00;"></div>
                <span><strong>녹색</strong>: ROI 경계 (코너 영역)</span>
            </div>
            <div class="legend-item">
                <div class="color-box" style="border-color: #0000ff;"></div>
                <span><strong>파란색</strong>: 검출된 원 (모든 후보)</span>
            </div>
            <div class="legend-item">
                <div class="color-box" style="border-color: #ff0000;"></div>
                <span><strong>빨간색</strong>: 선택된 원 (마운팅 홀)</span>
            </div>
        </div>

        <div class="image-container">
            <img id="debugImage" src="/debug_viewer/latest_debug_image" alt="디버그 이미지" onerror="this.style.display='none'; document.getElementById('noImage').style.display='block';">
            <div id="noImage" class="no-image" style="display:none;">
                <h2>📷 대기 중...</h2>
                <p>PCB 프레임을 전송하면 여기에 디버그 이미지가 표시됩니다.</p>
            </div>
        </div>

        <div class="info">
            <p><strong>상태:</strong> <span id="status">자동 갱신 중</span></p>
            <p><strong>갱신 주기:</strong> 1초</p>
            <p id="timestamp">마지막 업데이트: 로딩 중...</p>
            <p><strong>ROI 크기:</strong> 30% width, 35% height (축소하여 마운팅 홀만 검출)</p>
        </div>
    </div>

    <script>
        // 1초마다 이미지 갱신
        setInterval(() => {
            const img = document.getElementById('debugImage');
            const noImg = document.getElementById('noImage');

            // 캐시 방지용 타임스탬프 추가
            img.src = '/debug_viewer/latest_debug_image?' + new Date().getTime();
            img.style.display = 'block';
            noImg.style.display = 'none';

            // 타임스탬프 업데이트
            document.getElementById('timestamp').textContent =
                '마지막 업데이트: ' + new Date().toLocaleString('ko-KR');
        }, 1000);
    </script>
</body>
</html>
    """
    return html_content


@app.route('/debug_viewer/latest_debug_image')
def latest_debug_image_endpoint():
    """최신 디버그 이미지 반환 (JPEG)"""
    global latest_debug_image

    if latest_debug_image is not None:
        try:
            # NumPy 배열을 JPEG로 인코딩
            _, buffer = cv2.imencode('.jpg', latest_debug_image)

            # bytes로 변환하여 반환
            from flask import make_response
            response = make_response(buffer.tobytes())
            response.headers['Content-Type'] = 'image/jpeg'
            response.headers['Cache-Control'] = 'no-cache, no-store, must-revalidate'
            response.headers['Pragma'] = 'no-cache'
            response.headers['Expires'] = '0'
            return response

        except Exception as e:
            logger.error(f"디버그 이미지 인코딩 실패: {e}")
            return "이미지 인코딩 실패", 500
    else:
        # 플레이스홀더 이미지 (640x480 회색 이미지)
        import numpy as np
        placeholder = np.full((480, 640, 3), 50, dtype=np.uint8)
        cv2.putText(placeholder, "Waiting for frame...", (150, 240),
                   cv2.FONT_HERSHEY_SIMPLEX, 1, (200, 200, 200), 2)

        _, buffer = cv2.imencode('.jpg', placeholder)
        from flask import make_response
        response = make_response(buffer.tobytes())
        response.headers['Content-Type'] = 'image/jpeg'
        return response


if __name__ == '__main__':
    logger.info("Flask 추론 서버 시작 (SocketIO + threading 활성화)...")
    logger.info("포트: 5000")
    logger.info("호스트: 0.0.0.0 (모든 인터페이스)")
    logger.info("WebSocket 엔드포인트: ws://0.0.0.0:5000/socket.io/")
    logger.info("비동기 모드: threading (YOLO/OpenCV 안정성 우선) ⭐")

    # SocketIO로 실행 (threading 서버 사용)
    socketio.run(
        app,
        host='0.0.0.0',  # 외부 접근 허용
        port=5000,
        debug=False,     # 프로덕션 모드
        use_reloader=False,  # 자동 재시작 비활성화 (프로덕션)
        log_output=True,  # 로그 출력 활성화
        allow_unsafe_werkzeug=True  # Werkzeug 프로덕션 경고 무시 (threading 모드)
    )
