"""
Flask 서버 테스트 스크립트

서버가 실행 중일 때 이 스크립트를 실행하여 API 엔드포인트를 테스트합니다.

실행 방법:
    python server/test_server.py

테스트 항목:
    1. /health - 서버 상태 확인
    2. /box_status - 박스 상태 조회
    3. /predict - 단일 프레임 추론 (테스트 이미지)
    4. /predict_dual - 양면 추론 (테스트 이미지)
"""

import requests
import base64
import cv2
import numpy as np
import json
from datetime import datetime

# 서버 URL 설정
SERVER_URL = "http://localhost:5000"

def create_test_image(width=640, height=480, text="TEST"):
    """테스트용 이미지 생성 (흰 배경에 텍스트)"""
    img = np.ones((height, width, 3), dtype=np.uint8) * 255

    # 텍스트 추가
    font = cv2.FONT_HERSHEY_SIMPLEX
    text_size = cv2.getTextSize(text, font, 2, 3)[0]
    text_x = (width - text_size[0]) // 2
    text_y = (height + text_size[1]) // 2
    cv2.putText(img, text, (text_x, text_y), font, 2, (0, 0, 255), 3)

    return img

def image_to_base64(image):
    """OpenCV 이미지를 Base64 문자열로 변환"""
    _, buffer = cv2.imencode('.jpg', image)
    img_base64 = base64.b64encode(buffer).decode('utf-8')
    return img_base64

def print_response(response, test_name):
    """응답 결과 출력"""
    print(f"\n{'='*60}")
    print(f"테스트: {test_name}")
    print(f"{'='*60}")
    print(f"상태 코드: {response.status_code}")

    try:
        data = response.json()
        print(f"응답 내용:")
        print(json.dumps(data, indent=2, ensure_ascii=False))
    except Exception as e:
        print(f"응답 파싱 실패: {e}")
        print(f"원본 응답: {response.text}")

    if response.status_code == 200:
        print("✅ 테스트 성공")
    else:
        print("❌ 테스트 실패")

def test_health():
    """1. 헬스 체크 테스트"""
    try:
        response = requests.get(f"{SERVER_URL}/health")
        print_response(response, "Health Check")
        return response.status_code == 200
    except Exception as e:
        print(f"❌ Health Check 실패: {e}")
        return False

def test_box_status():
    """2. 박스 상태 조회 테스트"""
    try:
        response = requests.get(f"{SERVER_URL}/box_status")
        print_response(response, "Box Status")
        return response.status_code == 200
    except Exception as e:
        print(f"❌ Box Status 실패: {e}")
        return False

def test_predict_single():
    """3. 단일 프레임 추론 테스트"""
    try:
        # 테스트 이미지 생성
        test_img = create_test_image(text="LEFT CAM")
        img_base64 = image_to_base64(test_img)

        # 요청 데이터
        payload = {
            "camera_id": "left",
            "image": img_base64
        }

        # API 호출
        response = requests.post(
            f"{SERVER_URL}/predict",
            json=payload,
            headers={"Content-Type": "application/json"}
        )

        print_response(response, "Single Frame Prediction (Left)")
        return response.status_code == 200
    except Exception as e:
        print(f"❌ Single Prediction 실패: {e}")
        return False

def test_predict_dual():
    """4. 양면 추론 테스트"""
    try:
        # 좌측 테스트 이미지
        left_img = create_test_image(text="LEFT")
        left_base64 = image_to_base64(left_img)

        # 우측 테스트 이미지
        right_img = create_test_image(text="RIGHT")
        right_base64 = image_to_base64(right_img)

        # 요청 데이터
        payload = {
            "left_image": left_base64,
            "right_image": right_base64
        }

        # API 호출
        response = requests.post(
            f"{SERVER_URL}/predict_dual",
            json=payload,
            headers={"Content-Type": "application/json"}
        )

        print_response(response, "Dual Frame Prediction")
        return response.status_code == 200
    except Exception as e:
        print(f"❌ Dual Prediction 실패: {e}")
        return False

def test_invalid_request():
    """5. 잘못된 요청 테스트 (에러 핸들링 검증)"""
    try:
        # 빈 데이터
        response = requests.post(
            f"{SERVER_URL}/predict",
            json={},
            headers={"Content-Type": "application/json"}
        )

        print_response(response, "Invalid Request (Empty Data)")
        return response.status_code == 400  # Bad Request 예상
    except Exception as e:
        print(f"❌ Invalid Request 테스트 실패: {e}")
        return False

def main():
    """메인 테스트 실행"""
    print("\n" + "="*60)
    print("Flask 서버 API 테스트 시작")
    print(f"서버 URL: {SERVER_URL}")
    print(f"테스트 시작 시간: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
    print("="*60)

    # 서버 연결 확인
    print("\n🔍 서버 연결 확인 중...")
    try:
        response = requests.get(f"{SERVER_URL}/health", timeout=3)
        print("✅ 서버 연결 성공")
    except requests.exceptions.ConnectionError:
        print("❌ 서버에 연결할 수 없습니다.")
        print("💡 서버가 실행 중인지 확인하세요: python server/app.py")
        return
    except Exception as e:
        print(f"❌ 연결 확인 실패: {e}")
        return

    # 테스트 실행
    results = []

    print("\n" + "-"*60)
    print("테스트 1/5: Health Check")
    print("-"*60)
    results.append(("Health Check", test_health()))

    print("\n" + "-"*60)
    print("테스트 2/5: Box Status")
    print("-"*60)
    results.append(("Box Status", test_box_status()))

    print("\n" + "-"*60)
    print("테스트 3/5: Single Prediction")
    print("-"*60)
    results.append(("Single Prediction", test_predict_single()))

    print("\n" + "-"*60)
    print("테스트 4/5: Dual Prediction")
    print("-"*60)
    results.append(("Dual Prediction", test_predict_dual()))

    print("\n" + "-"*60)
    print("테스트 5/5: Invalid Request Handling")
    print("-"*60)
    results.append(("Invalid Request", test_invalid_request()))

    # 결과 요약
    print("\n" + "="*60)
    print("테스트 결과 요약")
    print("="*60)

    passed = sum(1 for _, result in results if result)
    total = len(results)

    for test_name, result in results:
        status = "✅ PASS" if result else "❌ FAIL"
        print(f"{status} - {test_name}")

    print("-"*60)
    print(f"총 {total}개 테스트 중 {passed}개 성공 ({passed/total*100:.1f}%)")
    print("="*60)

    if passed == total:
        print("\n🎉 모든 테스트를 통과했습니다!")
        print("💡 이제 라즈베리파이 카메라 클라이언트를 연결할 수 있습니다.")
    else:
        print(f"\n⚠️  {total - passed}개의 테스트가 실패했습니다.")
        print("💡 서버 로그를 확인하여 오류를 해결하세요.")

if __name__ == '__main__':
    main()
