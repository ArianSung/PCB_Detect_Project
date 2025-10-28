"""
API 계약 테스트 (Contract Testing)
API 변경 시 이 테스트를 실행하여 계약이 깨지지 않았는지 확인

실행 방법:
pytest tests/api/test_api_contract.py -v
"""

import pytest
import requests
import base64
import os
from datetime import datetime

# 테스트할 서버 URL (환경 변수로 설정 가능)
API_BASE_URL = os.getenv("API_BASE_URL", "http://localhost:5000")


class TestHealthEndpoint:
    """서버 상태 확인 API 계약 테스트"""

    def test_health_returns_200(self):
        """서버 상태 확인 API가 200 OK를 반환하는지 확인"""
        response = requests.get(f"{API_BASE_URL}/health")
        assert response.status_code == 200

    def test_health_response_format(self):
        """서버 상태 확인 API 응답 형식 확인"""
        response = requests.get(f"{API_BASE_URL}/health")
        data = response.json()

        # 필수 필드 확인
        assert "status" in data
        assert "server_time" in data
        assert "gpu_available" in data
        assert "models_loaded" in data
        assert "version" in data

        # 데이터 타입 확인
        assert isinstance(data["status"], str)
        assert isinstance(data["gpu_available"], bool)
        assert isinstance(data["models_loaded"], dict)

        # models_loaded 하위 필드 확인
        assert "yolo" in data["models_loaded"]
        assert "anomaly" in data["models_loaded"]


class TestPredictEndpoint:
    """단일 프레임 검사 API 계약 테스트"""

    @pytest.fixture
    def valid_request_data(self):
        """유효한 요청 데이터 생성"""
        # 1x1 빨간색 JPEG 이미지 (테스트용 최소 크기)
        test_image = b'\xff\xd8\xff\xe0\x00\x10JFIF\x00\x01\x01\x00\x00\x01\x00\x01\x00\x00\xff\xdb\x00C\x00\x08\x06\x06\x07\x06\x05\x08\x07\x07\x07\t\t\x08\n\x0c\x14\r\x0c\x0b\x0b\x0c\x19\x12\x13\x0f\x14\x1d\x1a\x1f\x1e\x1d\x1a\x1c\x1c $.\' ",#\x1c\x1c(7),01444\x1f\'9=82<.342\xff\xc0\x00\x0b\x08\x00\x01\x00\x01\x01\x01\x11\x00\xff\xc4\x00\x1f\x00\x00\x01\x05\x01\x01\x01\x01\x01\x01\x00\x00\x00\x00\x00\x00\x00\x00\x01\x02\x03\x04\x05\x06\x07\x08\t\n\x0b\xff\xc4\x00\xb5\x10\x00\x02\x01\x03\x03\x02\x04\x03\x05\x05\x04\x04\x00\x00\x01}\x01\x02\x03\x00\x04\x11\x05\x12!1A\x06\x13Qa\x07"q\x142\x81\x91\xa1\x08#B\xb1\xc1\x15R\xd1\xf0$3br\x82\t\n\x16\x17\x18\x19\x1a%&\'()*456789:CDEFGHIJSTUVWXYZcdefghijstuvwxyz\x83\x84\x85\x86\x87\x88\x89\x8a\x92\x93\x94\x95\x96\x97\x98\x99\x9a\xa2\xa3\xa4\xa5\xa6\xa7\xa8\xa9\xaa\xb2\xb3\xb4\xb5\xb6\xb7\xb8\xb9\xba\xc2\xc3\xc4\xc5\xc6\xc7\xc8\xc9\xca\xd2\xd3\xd4\xd5\xd6\xd7\xd8\xd9\xda\xe1\xe2\xe3\xe4\xe5\xe6\xe7\xe8\xe9\xea\xf1\xf2\xf3\xf4\xf5\xf6\xf7\xf8\xf9\xfa\xff\xda\x00\x08\x01\x01\x00\x00?\x00\xfb\xfe\xff\xd9'

        return {
            "camera_id": "left",
            "image": base64.b64encode(test_image).decode("utf-8"),
            "timestamp": datetime.now().isoformat()
        }

    def test_predict_requires_image_field(self, valid_request_data):
        """image 필드가 없으면 400 에러 반환"""
        invalid_data = valid_request_data.copy()
        del invalid_data["image"]

        response = requests.post(f"{API_BASE_URL}/predict", json=invalid_data)
        assert response.status_code == 400

        data = response.json()
        assert "success" in data
        assert data["success"] is False

    def test_predict_requires_camera_id_field(self, valid_request_data):
        """camera_id 필드가 없으면 400 에러 반환"""
        invalid_data = valid_request_data.copy()
        del invalid_data["camera_id"]

        response = requests.post(f"{API_BASE_URL}/predict", json=invalid_data)
        assert response.status_code == 400

    def test_predict_response_format(self, valid_request_data):
        """유효한 요청에 대한 응답 형식 확인"""
        response = requests.post(f"{API_BASE_URL}/predict", json=valid_request_data)
        assert response.status_code == 200

        data = response.json()

        # 최상위 필수 필드
        assert "success" in data
        assert "request_id" in data
        assert "camera_id" in data
        assert "timestamp" in data
        assert "inference_time_ms" in data
        assert "result" in data
        assert "gpio_action" in data

        # result 하위 필드
        result = data["result"]
        assert "classification" in result
        assert "confidence" in result
        assert "defects" in result
        assert "total_defects" in result
        assert "anomaly_score" in result

        # classification 값 검증
        valid_classifications = ["normal", "component_defect", "solder_defect", "discard"]
        assert result["classification"] in valid_classifications

        # 데이터 타입 검증
        assert isinstance(result["confidence"], (int, float))
        assert isinstance(result["defects"], list)
        assert isinstance(result["total_defects"], int)
        assert isinstance(result["anomaly_score"], (int, float))

        # confidence 범위 검증 (0~1)
        assert 0 <= result["confidence"] <= 1

        # gpio_action 필드
        gpio = data["gpio_action"]
        assert "enabled" in gpio
        assert "pin" in gpio
        assert "action" in gpio
        assert isinstance(gpio["enabled"], bool)
        assert isinstance(gpio["pin"], int)

    def test_predict_defect_structure(self, valid_request_data):
        """불량이 검출된 경우 defects 배열 구조 확인"""
        response = requests.post(f"{API_BASE_URL}/predict", json=valid_request_data)
        data = response.json()

        defects = data["result"]["defects"]

        # 불량이 있는 경우
        if len(defects) > 0:
            defect = defects[0]

            # 필수 필드
            assert "type" in defect
            assert "bbox" in defect
            assert "confidence" in defect
            assert "severity" in defect

            # bbox 형식 확인 (4개 좌표)
            assert isinstance(defect["bbox"], list)
            assert len(defect["bbox"]) == 4

            # severity 값 검증
            assert defect["severity"] in ["low", "medium", "high"]


class TestHistoryEndpoint:
    """검사 이력 조회 API 계약 테스트"""

    def test_history_returns_200(self):
        """검사 이력 조회 API가 200 OK를 반환하는지 확인"""
        response = requests.get(f"{API_BASE_URL}/history")
        assert response.status_code == 200

    def test_history_response_format(self):
        """검사 이력 조회 API 응답 형식 확인"""
        response = requests.get(f"{API_BASE_URL}/history?page=1&limit=10")
        data = response.json()

        # 필수 필드
        assert "success" in data
        assert "page" in data
        assert "limit" in data
        assert "total_records" in data
        assert "total_pages" in data
        assert "records" in data

        # 데이터 타입
        assert isinstance(data["page"], int)
        assert isinstance(data["limit"], int)
        assert isinstance(data["total_records"], int)
        assert isinstance(data["total_pages"], int)
        assert isinstance(data["records"], list)

    def test_history_record_structure(self):
        """검사 이력 레코드 구조 확인"""
        response = requests.get(f"{API_BASE_URL}/history?page=1&limit=1")
        data = response.json()

        if len(data["records"]) > 0:
            record = data["records"][0]

            # 필수 필드
            assert "id" in record
            assert "timestamp" in record
            assert "camera_id" in record
            assert "classification" in record
            assert "confidence" in record
            assert "total_defects" in record
            assert "inference_time_ms" in record

            # camera_id 값 검증
            assert record["camera_id"] in ["left", "right"]

            # classification 값 검증
            valid_classifications = ["normal", "component_defect", "solder_defect", "discard"]
            assert record["classification"] in valid_classifications


class TestStatisticsEndpoint:
    """통계 데이터 조회 API 계약 테스트"""

    def test_statistics_returns_200(self):
        """통계 데이터 조회 API가 200 OK를 반환하는지 확인"""
        response = requests.get(f"{API_BASE_URL}/statistics")
        assert response.status_code == 200

    def test_statistics_response_format(self):
        """통계 데이터 조회 API 응답 형식 확인"""
        response = requests.get(
            f"{API_BASE_URL}/statistics?start_date=2025-10-01&end_date=2025-10-25"
        )
        data = response.json()

        # 필수 필드
        assert "success" in data
        assert "period" in data
        assert "total_inspections" in data
        assert "classification_counts" in data
        assert "defect_type_counts" in data
        assert "average_inference_time_ms" in data
        assert "defect_rate" in data

        # period 하위 필드
        assert "start_date" in data["period"]
        assert "end_date" in data["period"]

        # classification_counts 하위 필드
        counts = data["classification_counts"]
        assert "normal" in counts
        assert "solder_defect" in counts
        assert "component_defect" in counts
        assert "discard" in counts


if __name__ == "__main__":
    # Mock 서버가 실행 중인지 확인
    try:
        response = requests.get(f"{API_BASE_URL}/health", timeout=2)
        print(f"✓ Mock 서버 실행 중: {API_BASE_URL}")
        print(f"  서버 버전: {response.json().get('version', 'unknown')}")
    except requests.exceptions.ConnectionError:
        print(f"✗ Mock 서버가 실행되지 않음: {API_BASE_URL}")
        print("\n다음 명령으로 Mock 서버를 먼저 실행하세요:")
        print("  python tests/api/mock_server.py")
        exit(1)

    # pytest 실행
    pytest.main([__file__, "-v", "--tb=short"])
