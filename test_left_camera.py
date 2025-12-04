"""
좌측 카메라 PCB 정렬 및 검증 테스트 스크립트

레퍼런스 이미지 중 하나를 사용하여 서버의 정렬 및 검증 기능을 테스트합니다.

실행 방법:
    python test_left_camera.py
"""

import requests
import base64
import cv2
import json
import sys

# 서버 URL
SERVER_URL = "http://localhost:5000"

# 테스트 이미지 경로
TEST_IMAGE_PATH = "server/reference_images/reference_left_20251122_180849_2.jpg"


def image_to_base64(image_path):
    """이미지 파일을 Base64로 인코딩"""
    img = cv2.imread(image_path)
    if img is None:
        print(f"[ERROR] 이미지를 읽을 수 없습니다: {image_path}")
        return None

    # JPEG 인코딩
    encode_params = [
        cv2.IMWRITE_JPEG_QUALITY, 85,
        cv2.IMWRITE_JPEG_PROGRESSIVE, 0,
        cv2.IMWRITE_JPEG_OPTIMIZE, 1
    ]
    _, buffer = cv2.imencode('.jpg', img, encode_params)
    img_base64 = base64.b64encode(buffer).decode('utf-8')

    return img_base64


def test_left_camera():
    """좌측 카메라 테스트"""
    print("=" * 60)
    print("좌측 카메라 PCB 정렬 및 검증 테스트")
    print("=" * 60)

    # 1. 서버 상태 확인
    print("\n[1/3] 서버 상태 확인 중...")
    try:
        response = requests.get(f"{SERVER_URL}/health", timeout=3)
        if response.status_code == 200:
            print("✅ 서버 연결 성공")
        else:
            print(f"❌ 서버 응답 오류: {response.status_code}")
            return False
    except Exception as e:
        print(f"❌ 서버 연결 실패: {e}")
        print("💡 서버를 먼저 실행하세요: python server/app.py")
        return False

    # 2. 테스트 이미지 준비
    print(f"\n[2/3] 테스트 이미지 로드 중...")
    print(f"  - 경로: {TEST_IMAGE_PATH}")

    img_base64 = image_to_base64(TEST_IMAGE_PATH)
    if img_base64 is None:
        return False

    print(f"  - Base64 인코딩 완료 (크기: {len(img_base64)} bytes)")

    # 3. 추론 API 호출
    print(f"\n[3/3] 추론 API 호출 중...")

    payload = {
        "camera_id": "left",
        "image": img_base64
    }

    try:
        response = requests.post(
            f"{SERVER_URL}/predict",
            json=payload,
            headers={"Content-Type": "application/json"},
            timeout=30
        )

        print(f"  - 응답 코드: {response.status_code}")

        if response.status_code == 200:
            result = response.json()

            print("\n" + "=" * 60)
            print("추론 결과")
            print("=" * 60)

            # 결과 출력
            print(f"\n카메라: {result.get('camera_id', 'N/A')}")
            print(f"타임스탬프: {result.get('timestamp', 'N/A')}")

            # 정렬 정보
            if 'alignment' in result:
                alignment = result['alignment']
                print(f"\n[ PCB 정렬 ]")
                print(f"  - 정렬 성공: {alignment.get('aligned', False)}")

                if 'detected_holes' in alignment:
                    holes = alignment['detected_holes']
                    print(f"  - 검출된 구멍: {len(holes)}개")
                    for i, hole in enumerate(holes, 1):
                        print(f"    {i}. {hole}")

                if 'transform_matrix' in alignment:
                    print(f"  - 변환 행렬: 적용됨 ✅")

            # 검출 결과
            if 'detections' in result:
                detections = result['detections']
                print(f"\n[ YOLO 검출 ]")
                print(f"  - 총 검출 개수: {len(detections)}개")

                # 클래스별 통계
                class_counts = {}
                for det in detections:
                    class_name = det.get('class_name', 'unknown')
                    class_counts[class_name] = class_counts.get(class_name, 0) + 1

                for class_name, count in sorted(class_counts.items()):
                    print(f"    - {class_name}: {count}개")

            # 검증 결과
            if 'verification' in result:
                verification = result['verification']
                summary = verification.get('summary', {})

                print(f"\n[ 컴포넌트 검증 ]")
                print(f"  - 기준 컴포넌트: {summary.get('total_reference', 0)}개")
                print(f"  - 검출 컴포넌트: {summary.get('total_detected', 0)}개")
                print(f"  - ✅ 정상: {summary.get('correct_count', 0)}개")
                print(f"  - ⚠️  위치 오류: {summary.get('misplaced_count', 0)}개")
                print(f"  - ❌ 누락: {summary.get('missing_count', 0)}개")
                print(f"  - ➕ 추가: {summary.get('extra_count', 0)}개")

                # 최종 판정
                if 'is_critical' in verification:
                    is_critical = verification['is_critical']
                    reason = verification.get('reason', '')

                    print(f"\n[ 최종 판정 ]")
                    if is_critical:
                        print(f"  🔴 치명적 불량")
                        print(f"  사유: {reason}")
                    elif summary.get('misplaced_count', 0) > 0 or summary.get('missing_count', 0) > 0:
                        print(f"  🟡 경미한 불량")
                        print(f"  사유: {reason}")
                    else:
                        print(f"  🟢 정상")

            print("\n" + "=" * 60)
            print("✅ 테스트 성공!")
            print("=" * 60)

            # 상세 결과를 파일로 저장
            with open('test_left_result.json', 'w', encoding='utf-8') as f:
                json.dump(result, f, indent=2, ensure_ascii=False)

            print(f"\n💾 상세 결과 저장: test_left_result.json")

            return True

        else:
            print(f"❌ API 호출 실패: {response.status_code}")
            print(f"응답: {response.text}")
            return False

    except Exception as e:
        print(f"❌ API 호출 오류: {e}")
        import traceback
        traceback.print_exc()
        return False


if __name__ == '__main__':
    success = test_left_camera()
    sys.exit(0 if success else 1)
