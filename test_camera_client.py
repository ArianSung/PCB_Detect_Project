#!/usr/bin/env python3
"""
테스트용 카메라 클라이언트 - 로컬 웹캠에서 프레임을 캡처하여 Flask 서버로 전송
"""

import cv2
import base64
import requests
import time
import sys

# 서버 URL
SERVER_URL = "http://100.123.23.111:5000"

# JPEG 인코딩 품질
JPEG_QUALITY = 85
encode_params = [int(cv2.IMWRITE_JPEG_QUALITY), JPEG_QUALITY]

def main():
    # 웹캠 초기화 (카메라 0번)
    cap = cv2.VideoCapture(0)

    if not cap.isOpened():
        print("[ERROR] 웹캠을 열 수 없습니다. USB 웹캠이 연결되어 있는지 확인하세요.")
        return

    # 카메라 해상도 설정
    cap.set(cv2.CAP_PROP_FRAME_WIDTH, 640)
    cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 480)

    print(f"[INFO] 웹캠 초기화 성공")
    print(f"[INFO] 서버: {SERVER_URL}")
    print(f"[INFO] 프레임 전송 시작... (Ctrl+C로 중지)")

    frame_count = 0

    try:
        while True:
            # 프레임 캡처
            ret, frame = cap.read()

            if not ret:
                print("[ERROR] 프레임 캡처 실패")
                break

            # JPEG 인코딩
            ret_encode, buffer = cv2.imencode('.jpg', frame, encode_params)

            if not ret_encode:
                print("[ERROR] JPEG 인코딩 실패")
                continue

            # Base64 인코딩
            frame_base64 = base64.b64encode(buffer).decode('utf-8')

            # Flask 서버로 전송
            start_time = time.time()

            try:
                response = requests.post(
                    f"{SERVER_URL}/predict_test",
                    json={
                        "camera_id": "left",
                        "image": frame_base64
                    },
                    timeout=5.0
                )

                elapsed_ms = (time.time() - start_time) * 1000

                if response.status_code == 200:
                    result = response.json()
                    frame_count += 1

                    # 10프레임마다 로그 출력
                    if frame_count % 10 == 0:
                        print(f"[INFO] 프레임 #{frame_count} → "
                              f"판정: {result.get('defect_type', 'N/A')}, "
                              f"추론: {result.get('inference_time_ms', 0):.1f}ms, "
                              f"전송+응답: {elapsed_ms:.1f}ms")
                else:
                    print(f"[ERROR] 서버 응답 오류: {response.status_code}")

            except requests.exceptions.Timeout:
                print("[ERROR] 서버 응답 시간 초과 (5초)")
            except requests.exceptions.ConnectionError:
                print("[ERROR] 서버 연결 실패")
            except Exception as e:
                print(f"[ERROR] 요청 실패: {e}")

            # 200ms 대기 (5 FPS)
            time.sleep(0.2)

    except KeyboardInterrupt:
        print("\n[INFO] 사용자 중지 (Ctrl+C)")

    finally:
        cap.release()
        print(f"[INFO] 종료 - 총 {frame_count}개 프레임 전송")

if __name__ == '__main__':
    main()
