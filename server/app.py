"""
PCB 불량 검사 Flask 추론 서버

실행 방법:
    python server/app.py

또는:
    flask --app server/app run --host=0.0.0.0 --port=5000
"""

from flask import Flask, request, jsonify
from flask_cors import CORS
import base64
import cv2
import numpy as np
from datetime import datetime
import logging
import time
import os
from pathlib import Path

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

# Flask 앱 초기화
app = Flask(__name__)
CORS(app)  # C# WinForms 연동을 위한 CORS 활성화

# 로깅 설정
logging.basicConfig(
    level=logging.INFO,
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

# TODO: YOLO 모델 로드 (학습 완료 후)
# from ultralytics import YOLO
# yolo_component_model = YOLO('models/fpic_component_best.pt')  # 부품 검출
# yolo_solder_model = YOLO('models/soldef_ai_best.pt')  # 납땜 불량 검출
yolo_component_model = None  # 임시 (부품 검출 모델)
yolo_solder_model = None  # 임시 (납땜 불량 모델)

# 테스트용: 최신 프레임 저장 (웹캠 스트리밍)
latest_frames = {
    'left': None,
    'right': None
}

# YOLO 어노테이션된 최신 프레임 저장 (C# WinForms 모니터링용 MJPEG 스트리밍)
latest_annotated_frames = {
    'left': None,
    'right': None
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

        # 3. AI 추론 (YOLO 모델 학습 완료 후 구현)
        # TODO: YOLO 모델 구현 후 활성화
        # if yolo_model is not None:
        #     results = yolo_model(frame)
        #     defect_type, confidence, boxes = parse_yolo_results(results)
        # else:
        #     defect_type = "정상"
        #     confidence = 0.95
        #     boxes = []

        # 임시 응답 (모델 미구현)
        defect_type = "정상"
        confidence = 0.95
        boxes = []  # 바운딩 박스 리스트 (x, y, w, h, class, confidence)

        # 4. GPIO 핀 결정
        gpio_pin = get_gpio_pin(defect_type)

        # 5. 추론 시간 계산
        inference_time_ms = (time.time() - start_time) * 1000

        # 6. 데이터베이스 저장
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

        # 7. 응답 생성
        response = {
            'status': 'ok',
            'camera_id': camera_id,
            'defect_type': defect_type,
            'confidence': confidence,
            'gpio_pin': gpio_pin,
            'inference_time_ms': round(inference_time_ms, 2),
            'timestamp': datetime.now().isoformat()
        }

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

        # 4. AI 추론 (YOLO 이중 모델)
        left_annotated_base64 = None
        right_annotated_base64 = None

        # 좌측 프레임: 부품 검출 모델
        if yolo_component_model is not None:
            left_results = yolo_component_model(left_frame)

            # YOLO 어노테이션 이미지 생성
            left_annotated = left_results[0].plot()  # 바운딩 박스 그리기

            # JPEG 인코딩 → Base64 (C# WinForms 응답용)
            _, buffer = cv2.imencode('.jpg', left_annotated, [cv2.IMWRITE_JPEG_QUALITY, 85])
            left_annotated_base64 = base64.b64encode(buffer).decode('utf-8')

            # MJPEG 스트리밍용 최신 프레임 업데이트
            latest_annotated_frames['left'] = buffer.tobytes()

            # 추론 결과 파싱
            left_result = parse_yolo_results(left_results, 'component')
            logger.info(f"좌측(부품) 추론 완료: {left_result['defect_type']}, confidence: {left_result['confidence']:.2f}")
        else:
            # 모델 미구현 시 임시 응답
            left_result = {'defect_type': '정상', 'confidence': 0.95, 'boxes': []}
            # 원본 프레임을 어노테이션 이미지로 사용
            _, buffer = cv2.imencode('.jpg', left_frame, [cv2.IMWRITE_JPEG_QUALITY, 85])
            left_annotated_base64 = base64.b64encode(buffer).decode('utf-8')
            latest_annotated_frames['left'] = buffer.tobytes()

        # 우측 프레임: 납땜 불량 모델
        if yolo_solder_model is not None:
            right_results = yolo_solder_model(right_frame)

            # YOLO 어노테이션 이미지 생성
            right_annotated = right_results[0].plot()

            # JPEG 인코딩 → Base64
            _, buffer = cv2.imencode('.jpg', right_annotated, [cv2.IMWRITE_JPEG_QUALITY, 85])
            right_annotated_base64 = base64.b64encode(buffer).decode('utf-8')

            # MJPEG 스트리밍용 최신 프레임 업데이트
            latest_annotated_frames['right'] = buffer.tobytes()

            # 추론 결과 파싱
            right_result = parse_yolo_results(right_results, 'solder')
            logger.info(f"우측(납땜) 추론 완료: {right_result['defect_type']}, confidence: {right_result['confidence']:.2f}")
        else:
            # 모델 미구현 시 임시 응답
            right_result = {'defect_type': '정상', 'confidence': 0.95, 'boxes': []}
            _, buffer = cv2.imencode('.jpg', right_frame, [cv2.IMWRITE_JPEG_QUALITY, 85])
            right_annotated_base64 = base64.b64encode(buffer).decode('utf-8')
            latest_annotated_frames['right'] = buffer.tobytes()

        # 결과 융합 (부품 + 납땜 결과 통합)
        final_defect_type, final_confidence = merge_dual_results(left_result, right_result)

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

            # YOLO 어노테이션 이미지 (바운딩 박스 그려진 이미지)
            'left_annotated_image': left_annotated_base64,
            'right_annotated_image': right_annotated_base64,

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


# 유틸리티 함수
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


def parse_yolo_results(results, model_type):
    """
    YOLO 추론 결과를 파싱하여 불량 유형과 신뢰도 추출

    Args:
        results: YOLO 추론 결과 객체
        model_type: 'component' (부품 검출) 또는 'solder' (납땜 불량)

    Returns:
        dict: {'defect_type': str, 'confidence': float, 'boxes': list}
    """
    boxes_list = []
    max_confidence = 0.0
    defect_classes = []

    # 검출된 객체 순회
    for box in results[0].boxes:
        cls_id = int(box.cls[0])
        confidence = float(box.conf[0])
        xyxy = box.xyxy[0].tolist()  # [x1, y1, x2, y2]

        # 클래스 이름 가져오기
        class_name = results[0].names[cls_id]

        boxes_list.append({
            'class': class_name,
            'confidence': confidence,
            'bbox': xyxy
        })

        defect_classes.append(class_name)
        max_confidence = max(max_confidence, confidence)

    # 불량 유형 판정
    if len(boxes_list) == 0:
        defect_type = '정상'
        confidence = 0.95
    else:
        # 부품 검출 모델
        if model_type == 'component':
            # Missing Component, Wrong Component 등이 검출되면 부품불량
            critical_defects = ['missing_component', 'wrong_component', 'misalignment']
            if any(cls in critical_defects for cls in defect_classes):
                defect_type = '부품불량'
            else:
                defect_type = '정상'

        # 납땜 불량 모델
        elif model_type == 'solder':
            # Cold Joint, Solder Bridge 등이 검출되면 납땜불량
            if len(defect_classes) > 0:
                defect_type = '납땜불량'
            else:
                defect_type = '정상'
        else:
            defect_type = '정상'

        confidence = max_confidence

    return {
        'defect_type': defect_type,
        'confidence': confidence,
        'boxes': boxes_list
    }


def merge_dual_results(left_result, right_result):
    """
    좌측(부품) + 우측(납땜) 추론 결과를 융합하여 최종 판정

    Args:
        left_result: 좌측 추론 결과 {'defect_type': str, 'confidence': float, ...}
        right_result: 우측 추론 결과 {'defect_type': str, 'confidence': float, ...}

    Returns:
        tuple: (final_defect_type: str, final_confidence: float)
    """
    left_defect = left_result['defect_type']
    right_defect = right_result['defect_type']

    # 우선순위: 폐기 > 부품불량 > 납땜불량 > 정상

    # 1. 양쪽 모두 불량이면 폐기
    if left_defect != '정상' and right_defect != '정상':
        return ('폐기', min(left_result['confidence'], right_result['confidence']))

    # 2. 부품불량 우선
    if left_defect == '부품불량':
        return (left_defect, left_result['confidence'])

    # 3. 납땜불량
    if right_defect == '납땜불량':
        return (right_defect, right_result['confidence'])

    # 4. 둘 다 정상
    return ('정상', max(left_result['confidence'], right_result['confidence']))


# ============================================================================
# 테스트용 엔드포인트: 웹캠 스트리밍 뷰어
# ============================================================================

@app.route('/upload_frame', methods=['POST'])
def upload_frame():
    """
    테스트용: 라즈베리파이에서 프레임 업로드

    Request JSON:
        {
            "camera_id": "left" or "right",
            "image": "base64_encoded_jpeg_image"
        }

    Response JSON:
        {
            "status": "ok",
            "camera_id": "left",
            "timestamp": "2025-11-08T..."
        }
    """
    try:
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

        if camera_id not in ['left', 'right']:
            return jsonify({
                'status': 'error',
                'error': 'Invalid camera_id. Must be "left" or "right"'
            }), 400

        # Base64 디코딩 및 검증
        try:
            image_bytes = base64.b64decode(image_base64)
            nparr = np.frombuffer(image_bytes, np.uint8)
            frame = cv2.imdecode(nparr, cv2.IMREAD_COLOR)

            if frame is None or frame.size == 0:
                return jsonify({
                    'status': 'error',
                    'error': 'Invalid image data'
                }), 400

        except Exception as decode_error:
            return jsonify({
                'status': 'error',
                'error': f'Failed to decode image: {str(decode_error)}'
            }), 400

        # 최신 프레임 저장
        latest_frames[camera_id] = image_bytes

        logger.info(f"프레임 업로드 성공: {camera_id} (shape: {frame.shape})")

        return jsonify({
            'status': 'ok',
            'camera_id': camera_id,
            'timestamp': datetime.now().isoformat()
        })

    except Exception as e:
        logger.error(f"프레임 업로드 실패: {str(e)}", exc_info=True)
        return jsonify({
            'status': 'error',
            'error': str(e)
        }), 500


@app.route('/latest_frame/<camera_id>', methods=['GET'])
def get_latest_frame(camera_id):
    """
    테스트용: 최신 프레임 JPEG 반환

    Args:
        camera_id: "left" or "right"

    Response:
        JPEG 이미지 (Content-Type: image/jpeg)
    """
    if camera_id not in ['left', 'right']:
        return jsonify({
            'status': 'error',
            'error': 'Invalid camera_id. Must be "left" or "right"'
        }), 400

    frame_data = latest_frames.get(camera_id)

    if frame_data is None:
        # 프레임이 없으면 빈 이미지 반환
        empty_frame = np.zeros((480, 640, 3), dtype=np.uint8)
        cv2.putText(empty_frame, f'No frame from {camera_id} camera',
                    (50, 240), cv2.FONT_HERSHEY_SIMPLEX, 1, (255, 255, 255), 2)
        _, buffer = cv2.imencode('.jpg', empty_frame)
        frame_data = buffer.tobytes()

    from flask import Response
    return Response(frame_data, mimetype='image/jpeg')


@app.route('/viewer', methods=['GET'])
def viewer():
    """
    테스트용: 웹캠 스트리밍 뷰어 HTML 페이지
    """
    html_content = '''
<!DOCTYPE html>
<html lang="ko">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>PCB 웹캠 뷰어 (테스트용)</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            padding: 20px;
        }

        .container {
            max-width: 1400px;
            margin: 0 auto;
        }

        h1 {
            text-align: center;
            color: white;
            margin-bottom: 10px;
            font-size: 2.5em;
            text-shadow: 2px 2px 4px rgba(0,0,0,0.3);
        }

        .subtitle {
            text-align: center;
            color: rgba(255, 255, 255, 0.9);
            margin-bottom: 30px;
            font-size: 1.1em;
        }

        .camera-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(500px, 1fr));
            gap: 30px;
            margin-bottom: 30px;
        }

        .camera-box {
            background: white;
            border-radius: 15px;
            padding: 20px;
            box-shadow: 0 10px 30px rgba(0,0,0,0.3);
            transition: transform 0.3s ease;
        }

        .camera-box:hover {
            transform: translateY(-5px);
        }

        .camera-title {
            font-size: 1.5em;
            font-weight: bold;
            margin-bottom: 15px;
            color: #333;
            display: flex;
            align-items: center;
            gap: 10px;
        }

        .status-indicator {
            width: 12px;
            height: 12px;
            border-radius: 50%;
            background: #4ade80;
            animation: pulse 2s infinite;
        }

        @keyframes pulse {
            0%, 100% {
                opacity: 1;
            }
            50% {
                opacity: 0.5;
            }
        }

        .camera-frame {
            width: 100%;
            height: auto;
            border-radius: 10px;
            background: #f0f0f0;
            min-height: 360px;
            object-fit: contain;
        }

        .info-panel {
            background: white;
            border-radius: 15px;
            padding: 20px;
            box-shadow: 0 10px 30px rgba(0,0,0,0.3);
        }

        .info-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 20px;
        }

        .info-item {
            text-align: center;
        }

        .info-label {
            font-size: 0.9em;
            color: #666;
            margin-bottom: 5px;
        }

        .info-value {
            font-size: 1.8em;
            font-weight: bold;
            color: #667eea;
        }

        .refresh-rate {
            text-align: center;
            color: rgba(255, 255, 255, 0.8);
            margin-top: 20px;
            font-size: 0.9em;
        }
    </style>
</head>
<body>
    <div class="container">
        <h1>🎥 PCB 웹캠 뷰어</h1>
        <p class="subtitle">라즈베리파이 양면 웹캠 실시간 스트리밍 (테스트용)</p>

        <div class="camera-grid">
            <div class="camera-box">
                <div class="camera-title">
                    <span class="status-indicator"></span>
                    좌측 카메라 (부품 검출)
                </div>
                <img id="left-frame" class="camera-frame" alt="Left Camera">
            </div>

            <div class="camera-box">
                <div class="camera-title">
                    <span class="status-indicator"></span>
                    우측 카메라 (납땜 검출)
                </div>
                <img id="right-frame" class="camera-frame" alt="Right Camera">
            </div>
        </div>

        <div class="info-panel">
            <div class="info-grid">
                <div class="info-item">
                    <div class="info-label">갱신 주기</div>
                    <div class="info-value">1초</div>
                </div>
                <div class="info-item">
                    <div class="info-label">해상도</div>
                    <div class="info-value">640x480</div>
                </div>
                <div class="info-item">
                    <div class="info-label">서버 상태</div>
                    <div class="info-value" id="server-status">연결됨</div>
                </div>
                <div class="info-item">
                    <div class="info-label">마지막 업데이트</div>
                    <div class="info-value" id="last-update" style="font-size: 1.2em;">-</div>
                </div>
            </div>
        </div>

        <p class="refresh-rate">
            💡 팁: 라즈베리파이에서 <code>python3 raspberry_pi/test_webcam_stream.py</code> 실행 후 이 페이지를 새로고침하세요.
        </p>
    </div>

    <script>
        // 이미지 갱신 함수
        function refreshImages() {
            const timestamp = new Date().getTime();
            const leftImg = document.getElementById('left-frame');
            const rightImg = document.getElementById('right-frame');
            const lastUpdate = document.getElementById('last-update');

            leftImg.src = '/latest_frame/left?' + timestamp;
            rightImg.src = '/latest_frame/right?' + timestamp;

            const now = new Date();
            lastUpdate.textContent = now.toLocaleTimeString('ko-KR');
        }

        // 1초마다 이미지 갱신
        setInterval(refreshImages, 1000);

        // 페이지 로드 시 즉시 갱신
        refreshImages();

        // 이미지 로드 오류 처리
        document.getElementById('left-frame').onerror = function() {
            document.getElementById('server-status').textContent = '오류';
            document.getElementById('server-status').style.color = '#ef4444';
        };
    </script>
</body>
</html>
    '''
    return html_content


@app.route('/video_feed_annotated/<camera_id>', methods=['GET'])
def video_feed_annotated(camera_id):
    """
    YOLO 바운딩 박스가 그려진 MJPEG 스트림

    C# WinForms 모니터링 앱에서 실시간 30fps로 어노테이션된 영상 표시

    Args:
        camera_id: "left" (부품 검출) 또는 "right" (납땜 불량)

    사용 예시 (C# AForge 라이브러리):
        MJPEGStream stream = new MJPEGStream("http://100.64.1.1:5000/video_feed_annotated/left");
        stream.Start();

    Response:
        MJPEG 스트림 (multipart/x-mixed-replace; boundary=frame)
    """
    if camera_id not in ['left', 'right']:
        return jsonify({
            'status': 'error',
            'error': 'Invalid camera_id. Must be "left" or "right"'
        }), 400

    def generate_mjpeg_stream():
        """MJPEG 스트림 생성기 (무한 루프)"""
        while True:
            frame_data = latest_annotated_frames.get(camera_id)

            if frame_data is not None:
                # multipart/x-mixed-replace 형식으로 JPEG 전송
                yield (b'--frame\r\n'
                       b'Content-Type: image/jpeg\r\n\r\n' + frame_data + b'\r\n')
            else:
                # 프레임이 없으면 빈 이미지 생성
                empty_frame = np.zeros((480, 640, 3), dtype=np.uint8)
                cv2.putText(
                    empty_frame,
                    f'No annotated frame from {camera_id} camera',
                    (50, 240),
                    cv2.FONT_HERSHEY_SIMPLEX,
                    0.8,
                    (255, 255, 255),
                    2
                )
                _, buffer = cv2.imencode('.jpg', empty_frame)
                yield (b'--frame\r\n'
                       b'Content-Type: image/jpeg\r\n\r\n' + buffer.tobytes() + b'\r\n')

            time.sleep(0.033)  # 30fps = 33ms

    from flask import Response
    return Response(
        generate_mjpeg_stream(),
        mimetype='multipart/x-mixed-replace; boundary=frame'
    )


if __name__ == '__main__':
    logger.info("Flask 추론 서버 시작...")
    logger.info("포트: 5000")
    logger.info("호스트: 0.0.0.0 (모든 인터페이스)")

    app.run(
        host='0.0.0.0',  # 외부 접근 허용
        port=5000,
        debug=False,     # 프로덕션에서는 False
        threaded=True    # 멀티스레딩 활성화
    )
