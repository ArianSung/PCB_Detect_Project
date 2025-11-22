#!/usr/bin/env python3
"""
PCB 검사 시스템 - 라즈베리파이 카메라 클라이언트

실행 방법:
    python camera_client.py

환경 변수 (.env 파일):
    SERVER_URL=http://100.123.23.111:5000
    CAMERA_ID=left 또는 right
    CAMERA_INDEX=0
    FRAME_SIZE=720
    JPEG_QUALITY=85
    TARGET_FPS=30
"""

import os
import cv2
import base64
import requests
import time
import logging
import subprocess
from datetime import datetime
from dotenv import load_dotenv

# .env 파일 로드
load_dotenv()

# 환경 변수 읽기
SERVER_URL = os.getenv('SERVER_URL', 'http://100.123.23.111:5000')
CAMERA_ID = os.getenv('CAMERA_ID', 'left')
CAMERA_INDEX = int(os.getenv('CAMERA_INDEX', 0))
CAMERA_WIDTH = int(os.getenv('CAMERA_WIDTH', 640))
CAMERA_HEIGHT = int(os.getenv('CAMERA_HEIGHT', 640))  # 640x640 정사각형 해상도
JPEG_QUALITY = int(os.getenv('JPEG_QUALITY', 85))
TARGET_FPS = int(os.getenv('CAMERA_FPS', 30))

# 로깅 설정
logging.basicConfig(
    level=logging.INFO,
    format='[%(asctime)s] [%(levelname)s] [Camera-Client] - %(message)s'
)
logger = logging.getLogger(__name__)


class CameraClient:
    """카메라 클라이언트 클래스"""

    def __init__(self, server_url, camera_id, camera_index):
        self.server_url = server_url
        self.camera_id = camera_id
        self.camera_index = camera_index
        self.api_endpoint = f"{server_url}/predict_test"
        self.cap = None
        self.frame_count = 0
        self.success_count = 0
        self.error_count = 0

    def setup_camera_v4l2(self):
        """v4l2-ctl을 사용해 카메라 노출 설정"""
        try:
            device = f"/dev/video{self.camera_index}"

            # 자동 노출 끄기 (1 = manual mode)
            subprocess.run(
                ['v4l2-ctl', '-d', device, '-c', 'auto_exposure=1'],
                capture_output=True,
                timeout=2
            )

            # 노출 값 수동 설정 (낮게 설정하여 밝기 감소)
            # 값 범위: 보통 1-5000, 낮을수록 어둡게
            subprocess.run(
                ['v4l2-ctl', '-d', device, '-c', 'exposure_absolute=150'],
                capture_output=True,
                timeout=2
            )

            # 밝기 조정 (선택사항, 0-255 범위)
            subprocess.run(
                ['v4l2-ctl', '-d', device, '-c', 'brightness=128'],
                capture_output=True,
                timeout=2
            )

            logger.info(f"✅ 카메라 설정 완료 (device: {device})")
            logger.info(f"   - 자동 노출: OFF")
            logger.info(f"   - 노출 값: 150 (수동)")
            logger.info(f"   - 밝기: 128")
        except Exception as e:
            logger.warning(f"⚠️  v4l2-ctl 설정 실패 (계속 진행): {e}")

    def initialize_camera(self):
        """카메라 초기화"""
        try:
            # 카메라 설정 (v4l2-ctl)
            self.setup_camera_v4l2()

            # OpenCV VideoCapture 초기화
            self.cap = cv2.VideoCapture(self.camera_index)

            if not self.cap.isOpened():
                raise RuntimeError(f"카메라 열기 실패 (index: {self.camera_index})")

            # 카메라 설정
            self.cap.set(cv2.CAP_PROP_FRAME_WIDTH, CAMERA_WIDTH)
            self.cap.set(cv2.CAP_PROP_FRAME_HEIGHT, CAMERA_HEIGHT)
            self.cap.set(cv2.CAP_PROP_FPS, TARGET_FPS)

            # 실제 설정값 확인
            actual_width = int(self.cap.get(cv2.CAP_PROP_FRAME_WIDTH))
            actual_height = int(self.cap.get(cv2.CAP_PROP_FRAME_HEIGHT))
            actual_fps = int(self.cap.get(cv2.CAP_PROP_FPS))

            logger.info(f"✅ 카메라 초기화 완료")
            logger.info(f"   - 카메라 ID: {self.camera_id}")
            logger.info(f"   - 장치 인덱스: {self.camera_index}")
            logger.info(f"   - 해상도: {actual_width}x{actual_height}")
            logger.info(f"   - FPS: {actual_fps}")
            logger.info(f"   - 서버 URL: {self.server_url}")

        except Exception as e:
            logger.error(f"❌ 카메라 초기화 실패: {e}")
            raise

    def capture_and_send(self):
        """프레임 캡처 및 서버 전송"""
        ret, frame = self.cap.read()

        if not ret or frame is None:
            logger.error("프레임 캡처 실패")
            self.error_count += 1
            return False

        try:
            # JPEG 인코딩
            encode_param = [int(cv2.IMWRITE_JPEG_QUALITY), JPEG_QUALITY]
            _, buffer = cv2.imencode('.jpg', frame, encode_param)

            # Base64 인코딩
            image_base64 = base64.b64encode(buffer).decode('utf-8')

            # JSON 페이로드 생성
            payload = {
                'camera_id': self.camera_id,
                'image': image_base64
            }

            # HTTP POST 요청
            start_time = time.time()
            response = requests.post(
                self.api_endpoint,
                json=payload,
                timeout=5.0  # Tailscale VPN용 타임아웃
            )
            request_time_ms = (time.time() - start_time) * 1000

            # 응답 확인
            if response.status_code == 200:
                data = response.json()
                defect_type = data.get('defect_type', '알 수 없음')
                confidence = data.get('confidence', 0.0)
                gpio_pin = data.get('gpio_pin', 0)
                inference_time = data.get('inference_time_ms', 0.0)

                self.frame_count += 1
                self.success_count += 1

                # 10프레임마다 로그 출력
                if self.frame_count % 10 == 0:
                    logger.info(
                        f"[{self.camera_id}] 프레임 #{self.frame_count} | "
                        f"판정: {defect_type} | "
                        f"신뢰도: {confidence:.2f} | "
                        f"GPIO: {gpio_pin} | "
                        f"추론: {inference_time:.1f}ms | "
                        f"요청: {request_time_ms:.1f}ms"
                    )
                return True
            else:
                logger.error(f"서버 오류: {response.status_code}")
                self.error_count += 1
                return False

        except requests.exceptions.Timeout:
            logger.error(f"요청 타임아웃 (5초 초과)")
            self.error_count += 1
            return False
        except requests.exceptions.RequestException as e:
            logger.error(f"HTTP 요청 실패: {e}")
            self.error_count += 1
            return False
        except Exception as e:
            logger.error(f"프레임 전송 실패: {e}")
            self.error_count += 1
            return False

    def run(self):
        """메인 루프"""
        try:
            self.initialize_camera()

            logger.info("=" * 60)
            logger.info("카메라 클라이언트 시작")
            logger.info(f"종료: Ctrl+C")
            logger.info("=" * 60)

            frame_interval = 1.0 / TARGET_FPS
            last_frame_time = time.time()

            while True:
                current_time = time.time()
                elapsed = current_time - last_frame_time

                # FPS 유지
                if elapsed >= frame_interval:
                    self.capture_and_send()
                    last_frame_time = current_time
                else:
                    # 대기
                    time.sleep(frame_interval - elapsed)

        except KeyboardInterrupt:
            logger.info("\n")
            logger.info("=" * 60)
            logger.info("카메라 클라이언트 종료 (사용자 중단)")
            logger.info(f"총 프레임: {self.frame_count}")
            logger.info(f"성공: {self.success_count}")
            logger.info(f"실패: {self.error_count}")
            if self.frame_count > 0:
                success_rate = (self.success_count / self.frame_count) * 100
                logger.info(f"성공률: {success_rate:.1f}%")
            logger.info("=" * 60)
        except Exception as e:
            logger.error(f"❌ 클라이언트 오류: {e}", exc_info=True)
        finally:
            if self.cap is not None:
                self.cap.release()
                logger.info("카메라 리소스 해제 완료")


if __name__ == '__main__':
    # 설정 정보 출력
    print("=" * 60)
    print("PCB 검사 시스템 - 라즈베리파이 카메라 클라이언트")
    print("=" * 60)
    print(f"서버 URL: {SERVER_URL}")
    print(f"카메라 ID: {CAMERA_ID}")
    print(f"카메라 인덱스: {CAMERA_INDEX}")
    print(f"프레임 크기: {CAMERA_WIDTH}x{CAMERA_HEIGHT}")
    print(f"JPEG 품질: {JPEG_QUALITY}")
    print(f"목표 FPS: {TARGET_FPS}")
    print("=" * 60)
    print()

    # 클라이언트 실행
    client = CameraClient(SERVER_URL, CAMERA_ID, CAMERA_INDEX)
    client.run()
