"""
PCB 불량 검사 Flask 추론 서버

실행 방법:
    python server/app.py

또는:
    flask --app server/app run --host=0.0.0.0 --port=5000
"""

from flask import Flask, request, jsonify, Response, render_template_string
from flask_cors import CORS
import base64
import cv2
import numpy as np
from datetime import datetime
import logging
import time
import os
from pathlib import Path
import threading

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

# YOLO 모델 로드
try:
    from ultralytics import YOLO
    model_path = 'runs/detect/roboflow_pcb_balanced/weights/best.pt'
    yolo_model = YOLO(model_path)
    logger.info(f"✅ YOLO 모델 로드 완료: {model_path}")
    logger.info(f"   - 모델 타입: YOLOv11l")
    logger.info(f"   - 클래스 수: 12개 (PCB 부품 검출)")
except Exception as e:
    logger.error(f"⚠️  YOLO 모델 로드 실패: {e}")
    logger.warning("   - 추론 시 더미 결과 반환됨")
    yolo_model = None

# 실시간 뷰어를 위한 전역 변수
latest_frames = {
    'left': None,
    'right': None
}
latest_results = {
    'left': {},
    'right': {}
}
frame_lock = threading.Lock()


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

            logger.info(f"[TEST] 프레임 수신 성공: {camera_id} (shape: {frame.shape})")

            # 뷰어를 위해 프레임 저장
            with frame_lock:
                latest_frames[camera_id] = frame.copy()

        except Exception as decode_error:
            return jsonify({
                'status': 'error',
                'error': f'Failed to decode image: {str(decode_error)}'
            }), 400

        # 3. 임시 추론 결과 (DB 저장 안 함)
        defect_type = "정상"
        confidence = 0.95
        gpio_pin = get_gpio_pin(defect_type)

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

        # 3. AI 추론 (YOLO 모델)
        if yolo_model is not None:
            try:
                # YOLO 추론 실행
                results = yolo_model(frame, verbose=False)
                defect_type, confidence, boxes = parse_yolo_results(results)
                logger.info(f"YOLO 추론 완료: {len(boxes)}개 객체 검출")
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

        # 4. AI 추론 (YOLO 모델 학습 완료 후 구현)
        # TODO: 양면 추론 및 결과 융합
        # left_result = yolo_model(left_frame)
        # right_result = yolo_model(right_frame)
        # final_defect_type, final_confidence = merge_dual_results(left_result, right_result)

        # 임시 응답 (모델 미구현)
        left_result = {
            'defect_type': '정상',
            'confidence': 0.95,
            'boxes': []
        }
        right_result = {
            'defect_type': '정상',
            'confidence': 0.95,
            'boxes': []
        }

        # 결과 융합 (임시 로직: 둘 중 하나라도 불량이면 불량)
        final_defect_type = "정상"
        final_confidence = min(left_result['confidence'], right_result['confidence'])

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
        while True:
            with frame_lock:
                frame = latest_frames.get(camera_id)

            if frame is not None:
                # JPEG 인코딩
                ret, buffer = cv2.imencode('.jpg', frame, [cv2.IMWRITE_JPEG_QUALITY, 85])
                if ret:
                    frame_bytes = buffer.tobytes()
                    yield (b'--frame\r\n'
                           b'Content-Type: image/jpeg\r\n\r\n' + frame_bytes + b'\r\n')
            else:
                # 프레임이 없으면 빈 프레임
                time.sleep(0.1)

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
