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

# Flask 앱 초기화
app = Flask(__name__)
CORS(app)  # C# WinForms 연동을 위한 CORS 활성화

# 로깅 설정
logging.basicConfig(
    level=logging.INFO,
    format='[%(asctime)s] [%(levelname)s] [Flask-Server] [%(module)s] - %(message)s'
)
logger = logging.getLogger(__name__)


# TODO: YOLO 모델 로드
# from ultralytics import YOLO
# yolo_model = YOLO('models/yolo/final/yolo_best.pt')


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
    try:
        data = request.get_json()
        camera_id = data.get('camera_id')
        image_base64 = data.get('image')

        # Base64 디코딩
        image_bytes = base64.b64decode(image_base64)
        nparr = np.frombuffer(image_bytes, np.uint8)
        frame = cv2.imdecode(nparr, cv2.IMREAD_COLOR)

        logger.info(f"프레임 수신: {camera_id} (shape: {frame.shape})")

        # TODO: YOLO 추론
        # results = yolo_model(frame)
        # defect_type, confidence = parse_yolo_results(results)

        # 임시 응답 (모델 미구현)
        defect_type = "정상"
        confidence = 0.95
        inference_time_ms = 85.3

        response = {
            'status': 'ok',
            'camera_id': camera_id,
            'defect_type': defect_type,
            'confidence': confidence,
            'inference_time_ms': inference_time_ms,
            'timestamp': datetime.now().isoformat()
        }

        logger.info(f"추론 완료: {defect_type} (confidence: {confidence:.2f})")
        return jsonify(response)

    except Exception as e:
        logger.error(f"추론 실패: {str(e)}")
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
    try:
        data = request.get_json()
        left_image = data.get('left_image')
        right_image = data.get('right_image')

        # TODO: 양면 추론 및 결과 융합
        # left_result = predict(left_image)
        # right_result = predict(right_image)
        # final_defect_type = merge_results(left_result, right_result)

        # 임시 응답
        final_defect_type = "정상"
        final_confidence = 0.95

        # GPIO 신호 결정
        gpio_pin = get_gpio_pin(final_defect_type)

        response = {
            'status': 'ok',
            'final_defect_type': final_defect_type,
            'final_confidence': final_confidence,
            'gpio_signal': {
                'pin': gpio_pin,
                'duration_ms': 300
            },
            'robot_command': {
                'category': defect_type_to_category(final_defect_type),
                'slot': 2  # TODO: 박스 슬롯 관리 (0-4)
            },
            'timestamp': datetime.now().isoformat()
        }

        logger.info(f"양면 추론 완료: {final_defect_type} (GPIO: {gpio_pin})")
        return jsonify(response)

    except Exception as e:
        logger.error(f"양면 추론 실패: {str(e)}")
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
    # TODO: MySQL에서 박스 상태 조회
    boxes = [
        {
            'box_id': 'NORMAL',
            'category': '정상',
            'current_slot': 3,
            'max_slots': 5,
            'is_full': False,
            'total_pcb_count': 15
        },
        {
            'box_id': 'COMPONENT_DEFECT',
            'category': '부품불량',
            'current_slot': 5,
            'max_slots': 5,
            'is_full': True,
            'total_pcb_count': 12
        },
        {
            'box_id': 'SOLDER_DEFECT',
            'category': '납땜불량',
            'current_slot': 1,
            'max_slots': 5,
            'is_full': False,
            'total_pcb_count': 2
        }
    ]

    summary = {
        'total_boxes': 3,
        'full_boxes': sum(1 for box in boxes if box['is_full']),
        'empty_boxes': sum(1 for box in boxes if box['current_slot'] == 0),
        'system_stopped': all(box['is_full'] for box in boxes)
    }

    return jsonify({
        'status': 'ok',
        'boxes': boxes,
        'summary': summary
    })


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
