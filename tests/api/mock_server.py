"""
Mock Flask 서버
실제 Flask 서버 없이도 라즈베리파이/C# 클라이언트 테스트 가능

실행 방법:
python tests/api/mock_server.py
"""

from flask import Flask, request, jsonify
from datetime import datetime
import uuid
import random

app = Flask(__name__)

# Mock 응답 데이터 생성 함수
def generate_mock_prediction(camera_id="left", has_defect=None):
    """Mock 예측 결과 생성"""

    # 랜덤하게 불량 여부 결정 (has_defect가 None인 경우)
    if has_defect is None:
        has_defect = random.random() < 0.3  # 30% 확률로 불량

    inference_time = random.uniform(80, 150)

    if has_defect:
        # 불량 타입 랜덤 선택
        defect_types = [
            "cold_joint", "solder_bridge", "insufficient_solder",
            "missing_component", "misalignment", "damaged_component"
        ]
        classifications = ["solder_defect", "component_defect", "discard"]

        classification = random.choice(classifications)
        defect_type = random.choice(defect_types)
        confidence = random.uniform(0.7, 0.95)

        defects = [
            {
                "type": defect_type,
                "bbox": [
                    random.randint(50, 300),
                    random.randint(50, 200),
                    random.randint(320, 500),
                    random.randint(220, 400)
                ],
                "confidence": confidence,
                "severity": random.choice(["low", "medium", "high"])
            }
        ]

        # GPIO 핀 매핑
        gpio_pins = {
            "component_defect": 17,
            "solder_defect": 27,
            "discard": 22
        }

        result = {
            "classification": classification,
            "confidence": confidence,
            "defects": defects,
            "total_defects": len(defects),
            "anomaly_score": random.uniform(0.5, 0.9)
        }

        gpio_action = {
            "enabled": True,
            "pin": gpio_pins.get(classification, 22),
            "action": "activate"
        }
    else:
        # 정상
        result = {
            "classification": "normal",
            "confidence": random.uniform(0.9, 0.99),
            "defects": [],
            "total_defects": 0,
            "anomaly_score": random.uniform(0.05, 0.2)
        }

        gpio_action = {
            "enabled": True,
            "pin": 23,
            "action": "activate"
        }

    return {
        "success": True,
        "request_id": str(uuid.uuid4()),
        "camera_id": camera_id,
        "timestamp": datetime.now().isoformat(),
        "inference_time_ms": round(inference_time, 2),
        "result": result,
        "gpio_action": gpio_action
    }


@app.route('/health', methods=['GET'])
def health():
    """서버 상태 확인 (Mock)"""
    return jsonify({
        "status": "healthy",
        "server_time": datetime.now().isoformat(),
        "gpu_available": True,
        "models_loaded": {
            "yolo": True,
            "anomaly": True
        },
        "version": "1.0.0-mock"
    })


@app.route('/predict', methods=['POST'])
def predict():
    """단일 프레임 PCB 불량 검사 (Mock)"""
    data = request.get_json()

    # 필수 필드 검증
    if not data or 'image' not in data or 'camera_id' not in data:
        return jsonify({
            "success": False,
            "error": "Invalid request format",
            "message": "Missing required field: image or camera_id",
            "timestamp": datetime.now().isoformat()
        }), 400

    camera_id = data.get('camera_id', 'left')

    # Mock 예측 결과 생성
    response = generate_mock_prediction(camera_id)

    print(f"[Mock Server] Predict request from {camera_id}: {response['result']['classification']}")

    return jsonify(response)


@app.route('/predict_dual', methods=['POST'])
def predict_dual():
    """양면 프레임 동시 검사 (Mock)"""
    data = request.get_json()

    if not data or 'left_camera' not in data or 'right_camera' not in data:
        return jsonify({
            "success": False,
            "error": "Invalid request format",
            "message": "Missing required field: left_camera or right_camera",
            "timestamp": datetime.now().isoformat()
        }), 400

    # 양쪽 결과 생성
    left_result = generate_mock_prediction("left")['result']
    right_result = generate_mock_prediction("right")['result']

    # 더 심각한 불량을 최종 분류로 선택
    priority = {"discard": 3, "component_defect": 2, "solder_defect": 1, "normal": 0}

    left_priority = priority.get(left_result['classification'], 0)
    right_priority = priority.get(right_result['classification'], 0)

    if left_priority >= right_priority:
        final_classification = left_result['classification']
        final_confidence = left_result['confidence']
    else:
        final_classification = right_result['classification']
        final_confidence = right_result['confidence']

    # GPIO 핀 매핑
    gpio_pins = {
        "component_defect": 17,
        "solder_defect": 27,
        "discard": 22,
        "normal": 23
    }

    response = {
        "success": True,
        "request_id": str(uuid.uuid4()),
        "timestamp": datetime.now().isoformat(),
        "inference_time_ms": round(random.uniform(150, 250), 2),
        "left_result": left_result,
        "right_result": right_result,
        "final_classification": final_classification,
        "final_confidence": final_confidence,
        "gpio_action": {
            "enabled": True,
            "pin": gpio_pins.get(final_classification, 22),
            "action": "activate"
        }
    }

    print(f"[Mock Server] Dual predict: Left={left_result['classification']}, Right={right_result['classification']}, Final={final_classification}")

    return jsonify(response)


@app.route('/history', methods=['GET'])
def history():
    """검사 이력 조회 (Mock)"""
    page = int(request.args.get('page', 1))
    limit = int(request.args.get('limit', 20))

    # Mock 데이터 생성
    total_records = 152
    records = []

    for i in range(limit):
        record_id = total_records - (page - 1) * limit - i
        if record_id < 1:
            break

        classification = random.choice(["normal", "normal", "normal", "solder_defect", "component_defect"])

        records.append({
            "id": record_id,
            "timestamp": datetime.now().isoformat(),
            "camera_id": random.choice(["left", "right"]),
            "classification": classification,
            "confidence": random.uniform(0.7, 0.99),
            "total_defects": 0 if classification == "normal" else random.randint(1, 3),
            "inference_time_ms": random.uniform(80, 150)
        })

    return jsonify({
        "success": True,
        "page": page,
        "limit": limit,
        "total_records": total_records,
        "total_pages": (total_records + limit - 1) // limit,
        "records": records
    })


@app.route('/statistics', methods=['GET'])
def statistics():
    """통계 데이터 조회 (Mock)"""
    start_date = request.args.get('start_date', '2025-10-01')
    end_date = request.args.get('end_date', '2025-10-25')

    total = 5420
    normal_count = int(total * 0.9)
    defect_count = total - normal_count

    return jsonify({
        "success": True,
        "period": {
            "start_date": start_date,
            "end_date": end_date
        },
        "total_inspections": total,
        "classification_counts": {
            "normal": normal_count,
            "solder_defect": int(defect_count * 0.55),
            "component_defect": int(defect_count * 0.35),
            "discard": int(defect_count * 0.1)
        },
        "defect_type_counts": {
            "cold_joint": 150,
            "solder_bridge": 120,
            "missing_component": 90,
            "misalignment": 80,
            "insufficient_solder": 50,
            "scratch": 30,
            "others": 50
        },
        "average_inference_time_ms": 110.3,
        "defect_rate": round(defect_count / total, 3)
    })


if __name__ == '__main__':
    print("=" * 60)
    print("Mock Flask Server 시작")
    print("=" * 60)
    print("Base URL: http://0.0.0.0:5000")
    print("")
    print("사용 가능한 엔드포인트:")
    print("  GET  /health")
    print("  POST /predict")
    print("  POST /predict_dual")
    print("  GET  /history")
    print("  GET  /statistics")
    print("")
    print("테스트 예시:")
    print("  curl http://localhost:5000/health")
    print("=" * 60)

    app.run(host='0.0.0.0', port=5000, debug=True)
