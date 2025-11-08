#!/usr/bin/env python3
"""
라즈베리파이 웹캠 스트리밍 테스트 클라이언트

Flask 서버로 웹캠 프레임을 전송하여 웹 브라우저에서 실시간으로 확인

사용법:
    # 단일 웹캠 (좌측)
    python3 test_webcam_stream.py --mode left --camera 0 --server http://100.123.23.111:5000

    # 양면 웹캠 동시 전송 (1 FPS)
    python3 test_webcam_stream.py --mode dual --left 0 --right 1 --server http://100.123.23.111:5000

    # 양면 웹캠 (30 FPS)
    python3 test_webcam_stream.py --mode dual --left 0 --right 1 --server http://100.123.23.111:5000 --fps 30

    # 환경 변수 사용 (SERVER_URL)
    export SERVER_URL=http://100.123.23.111:5000
    python3 test_webcam_stream.py --mode dual --left 0 --right 1 --fps 30
"""

import cv2
import requests
import base64
import time
import os
import argparse
from datetime import datetime


class WebcamStreamer:
    """웹캠 스트리밍 클라이언트"""

    def __init__(self, server_url):
        """
        Args:
            server_url: Flask 서버 URL (예: http://100.64.1.1:5000)
        """
        self.server_url = server_url.rstrip('/')
        self.upload_url = f"{self.server_url}/upload_frame"

    def encode_frame(self, frame, quality=85):
        """
        프레임을 JPEG → Base64 인코딩

        Args:
            frame: OpenCV 프레임 (numpy array)
            quality: JPEG 압축 품질 (0-100, 기본 85)

        Returns:
            Base64 인코딩된 문자열
        """
        _, buffer = cv2.imencode('.jpg', frame, [cv2.IMWRITE_JPEG_QUALITY, quality])
        return base64.b64encode(buffer).decode('utf-8')

    def upload_frame(self, camera_id, frame):
        """
        프레임을 Flask 서버로 업로드

        Args:
            camera_id: "left" 또는 "right"
            frame: OpenCV 프레임

        Returns:
            성공 여부 (bool)
        """
        try:
            # Base64 인코딩
            image_base64 = self.encode_frame(frame)

            # API 요청
            response = requests.post(
                self.upload_url,
                json={
                    "camera_id": camera_id,
                    "image": image_base64
                },
                timeout=5
            )

            if response.status_code == 200:
                result = response.json()
                if result['status'] == 'ok':
                    print(f"[{datetime.now().strftime('%H:%M:%S')}] ✓ {camera_id} 프레임 업로드 성공")
                    return True
                else:
                    print(f"[{datetime.now().strftime('%H:%M:%S')}] ✗ 오류: {result.get('error')}")
                    return False
            else:
                print(f"[{datetime.now().strftime('%H:%M:%S')}] ✗ HTTP 오류: {response.status_code}")
                return False

        except requests.exceptions.Timeout:
            print(f"[{datetime.now().strftime('%H:%M:%S')}] ✗ 타임아웃: Flask 서버 응답 없음")
            return False
        except Exception as e:
            print(f"[{datetime.now().strftime('%H:%M:%S')}] ✗ 업로드 실패: {str(e)}")
            return False

    def stream_single_camera(self, camera_id, camera_index, fps=1):
        """
        단일 웹캠 스트리밍

        Args:
            camera_id: "left" 또는 "right"
            camera_index: 웹캠 장치 인덱스 (보통 0)
            fps: 초당 전송 프레임 수 (기본 1)
        """
        print(f"\n🎥 단일 웹캠 스트리밍 시작")
        print(f"   카메라: {camera_id}")
        print(f"   장치: /dev/video{camera_index}")
        print(f"   FPS: {fps}")
        print(f"   서버: {self.server_url}")
        print(f"\n💡 웹 브라우저에서 확인: {self.server_url}/viewer")
        print(f"종료: Ctrl+C\n")

        # 웹캠 열기
        cap = cv2.VideoCapture(camera_index)

        if not cap.isOpened():
            print(f"✗ 웹캠 열기 실패: /dev/video{camera_index}")
            print("  확인 사항:")
            print("  1. 웹캠이 연결되어 있는지 확인")
            print("  2. ls /dev/video* 로 장치 확인")
            print("  3. 권한 확인: groups pi (video 그룹 포함 여부)")
            return

        # 해상도 설정
        cap.set(cv2.CAP_PROP_FRAME_WIDTH, 640)
        cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 480)

        frame_interval = 1.0 / fps
        frame_count = 0

        try:
            while True:
                ret, frame = cap.read()

                if not ret:
                    print("✗ 프레임 읽기 실패")
                    time.sleep(1)
                    continue

                # 프레임 업로드
                self.upload_frame(camera_id, frame)

                frame_count += 1
                time.sleep(frame_interval)

        except KeyboardInterrupt:
            print(f"\n\n사용자에 의해 중단됨 (총 {frame_count}프레임 전송)")

        finally:
            cap.release()
            print("웹캠 종료")

    def stream_dual_camera(self, left_index, right_index, fps=1):
        """
        양면 웹캠 동시 스트리밍

        Args:
            left_index: 좌측 웹캠 장치 인덱스
            right_index: 우측 웹캠 장치 인덱스
            fps: 초당 전송 프레임 수 (기본 1)
        """
        print(f"\n🎥 양면 웹캠 스트리밍 시작")
        print(f"   좌측 카메라: /dev/video{left_index}")
        print(f"   우측 카메라: /dev/video{right_index}")
        print(f"   FPS: {fps}")
        print(f"   서버: {self.server_url}")
        print(f"\n💡 웹 브라우저에서 확인: {self.server_url}/viewer")
        print(f"종료: Ctrl+C\n")

        # 웹캠 열기
        cap_left = cv2.VideoCapture(left_index)
        cap_right = cv2.VideoCapture(right_index)

        if not cap_left.isOpened():
            print(f"✗ 좌측 웹캠 열기 실패: /dev/video{left_index}")
            return

        if not cap_right.isOpened():
            print(f"✗ 우측 웹캠 열기 실패: /dev/video{right_index}")
            cap_left.release()
            return

        # 해상도 설정
        for cap in [cap_left, cap_right]:
            cap.set(cv2.CAP_PROP_FRAME_WIDTH, 640)
            cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 480)

        frame_interval = 1.0 / fps
        frame_count = 0

        try:
            while True:
                ret_left, left_frame = cap_left.read()
                ret_right, right_frame = cap_right.read()

                if not ret_left or not ret_right:
                    print("✗ 프레임 읽기 실패 (좌측 또는 우측)")
                    time.sleep(1)
                    continue

                # 양면 프레임 업로드
                self.upload_frame('left', left_frame)
                self.upload_frame('right', right_frame)

                frame_count += 1
                time.sleep(frame_interval)

        except KeyboardInterrupt:
            print(f"\n\n사용자에 의해 중단됨 (총 {frame_count}프레임 전송)")

        finally:
            cap_left.release()
            cap_right.release()
            print("양면 웹캠 종료")


def main():
    """메인 함수"""
    parser = argparse.ArgumentParser(
        description='라즈베리파이 웹캠 스트리밍 테스트 클라이언트',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog='''
사용 예시:
  # 단일 웹캠 (좌측, 1 FPS)
  python3 test_webcam_stream.py --mode left --camera 0 --server http://100.123.23.111:5000

  # 양면 웹캠 (1 FPS)
  python3 test_webcam_stream.py --mode dual --left 0 --right 1 --server http://100.123.23.111:5000

  # 양면 웹캠 (30 FPS)
  python3 test_webcam_stream.py --mode dual --left 0 --right 1 --server http://100.123.23.111:5000 --fps 30

  # 서버 URL 기본값 사용 (환경변수 SERVER_URL 또는 100.64.1.1)
  python3 test_webcam_stream.py --mode dual --left 0 --right 1 --fps 30

웹 브라우저 확인:
  http://서버주소:5000/viewer
        '''
    )

    # 필수 인자
    parser.add_argument('--mode', required=True, choices=['left', 'right', 'dual'],
                       help='카메라 모드 (left: 좌측 단일, right: 우측 단일, dual: 양면)')
    parser.add_argument('--server', dest='server_url',
                       default=os.getenv('SERVER_URL', 'http://100.64.1.1:5000'),
                       help='Flask 서버 URL (기본값: 환경변수 SERVER_URL 또는 http://100.64.1.1:5000)')

    # 단일 웹캠용 인자
    parser.add_argument('--camera', type=int,
                       help='단일 웹캠 장치 인덱스 (--mode left 또는 right일 때 사용)')

    # 양면 웹캠용 인자
    parser.add_argument('--left', type=int,
                       help='좌측 웹캠 장치 인덱스 (--mode dual일 때 사용)')
    parser.add_argument('--right', type=int,
                       help='우측 웹캠 장치 인덱스 (--mode dual일 때 사용)')

    # 옵션 인자
    parser.add_argument('--fps', type=int, default=1,
                       help='초당 전송 프레임 수 (기본값: 1, 권장: 30)')

    args = parser.parse_args()

    mode = args.mode
    server_url = args.server_url
    fps = args.fps

    if mode in ['left', 'right']:
        # 단일 웹캠 모드
        if args.camera is None:
            parser.error(f"--mode {mode}일 때는 --camera 옵션이 필요합니다")

        streamer = WebcamStreamer(server_url)
        streamer.stream_single_camera(mode, args.camera, fps=fps)

    elif mode == 'dual':
        # 양면 웹캠 모드
        if args.left is None or args.right is None:
            parser.error("--mode dual일 때는 --left와 --right 옵션이 모두 필요합니다")

        streamer = WebcamStreamer(server_url)
        streamer.stream_dual_camera(args.left, args.right, fps=fps)


if __name__ == '__main__':
    main()
