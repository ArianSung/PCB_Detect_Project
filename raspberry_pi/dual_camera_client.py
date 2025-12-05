#!/usr/bin/env python3
"""
PCB 검사 시스템 - 양면 동시 촬영 클라이언트 (제품별 부품 검증)

워크플로우:
    1. 좌측 카메라 (앞면) + 우측 카메라 (뒷면) 동시 촬영
    2. 양면 이미지를 Base64 인코딩
    3. /predict_dual 엔드포인트로 전송
    4. 서버가 뒷면 시리얼 넘버 OCR → 제품 코드 추출 → 앞면 부품 검증
    5. GPIO 제어 신호 수신 → 아두이노로 전송

실행 방법:
    python dual_camera_client.py

환경 변수 (.env 파일):
    SERVER_URL=http://100.80.24.53:5000
    LEFT_CAMERA_INDEX=0   # 앞면 (부품 검증용)
    RIGHT_CAMERA_INDEX=2  # 뒷면 (시리얼 넘버 OCR용)
    CAMERA_WIDTH=640
    CAMERA_HEIGHT=480
    JPEG_QUALITY=85
    TARGET_FPS=10

    # 좌측 카메라 (앞면) 파라미터
    LEFT_CAM_BRIGHTNESS=41
    LEFT_CAM_CONTRAST=52
    LEFT_CAM_SATURATION=59
    LEFT_CAM_EXPOSURE=1521
    LEFT_CAM_FOCUS=402

    # 우측 카메라 (뒷면) 파라미터
    RIGHT_CAM_BRIGHTNESS=41
    RIGHT_CAM_CONTRAST=52
    RIGHT_CAM_SATURATION=59
    RIGHT_CAM_EXPOSURE=1521
    RIGHT_CAM_FOCUS=402

    ARDUINO_ENABLED=true
    ARDUINO_PORT=/dev/ttyACM0
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
from arduino_serial_handler import ArduinoSerialHandler

# .env 파일 로드
load_dotenv()

# 환경 변수 읽기
SERVER_URL = os.getenv('SERVER_URL', 'http://100.80.24.53:5000')
LEFT_CAMERA_INDEX = int(os.getenv('LEFT_CAMERA_INDEX', 0))   # 앞면 (좌측)
RIGHT_CAMERA_INDEX = int(os.getenv('RIGHT_CAMERA_INDEX', 2)) # 뒷면 (우측)
CAMERA_WIDTH = int(os.getenv('CAMERA_WIDTH', 640))
CAMERA_HEIGHT = int(os.getenv('CAMERA_HEIGHT', 480))
JPEG_QUALITY = int(os.getenv('JPEG_QUALITY', 85))
TARGET_FPS = int(os.getenv('TARGET_FPS', 10))

# 아두이노 시리얼 통신 설정
ARDUINO_ENABLED = os.getenv('ARDUINO_ENABLED', 'false').lower() == 'true'
ARDUINO_PORT = os.getenv('ARDUINO_PORT', '/dev/ttyACM0')
ARDUINO_BAUDRATE = int(os.getenv('ARDUINO_BAUDRATE', 115200))

# 좌측 카메라 (앞면) 화질 파라미터
LEFT_CAM_BRIGHTNESS = int(os.getenv('LEFT_CAM_BRIGHTNESS', 41))
LEFT_CAM_CONTRAST = int(os.getenv('LEFT_CAM_CONTRAST', 52))
LEFT_CAM_SATURATION = int(os.getenv('LEFT_CAM_SATURATION', 59))
LEFT_CAM_EXPOSURE_ABS = int(os.getenv('LEFT_CAM_EXPOSURE', 1521))
LEFT_CAM_FOCUS_ABS = int(os.getenv('LEFT_CAM_FOCUS', 402))

# 우측 카메라 (뒷면) 화질 파라미터
RIGHT_CAM_BRIGHTNESS = int(os.getenv('RIGHT_CAM_BRIGHTNESS', 41))
RIGHT_CAM_CONTRAST = int(os.getenv('RIGHT_CAM_CONTRAST', 52))
RIGHT_CAM_SATURATION = int(os.getenv('RIGHT_CAM_SATURATION', 59))
RIGHT_CAM_EXPOSURE_ABS = int(os.getenv('RIGHT_CAM_EXPOSURE', 1521))
RIGHT_CAM_FOCUS_ABS = int(os.getenv('RIGHT_CAM_FOCUS', 402))

# 로깅 설정
logging.basicConfig(
    level=logging.INFO,
    format='[%(asctime)s] [%(levelname)s] [Dual-Camera-Client] - %(message)s'
)
logger = logging.getLogger(__name__)


class DualCameraClient:
    """양면 동시 촬영 클라이언트 클래스"""

    def __init__(self, server_url, left_camera_index, right_camera_index, arduino_handler=None):
        self.server_url = server_url
        self.left_camera_index = left_camera_index
        self.right_camera_index = right_camera_index
        self.api_endpoint = f"{server_url}/predict_dual"

        self.left_cap = None
        self.right_cap = None

        self.frame_count = 0
        self.success_count = 0
        self.error_count = 0
        self.arduino_handler = arduino_handler

    def setup_camera_v4l2(self, camera_index, exposure_abs, focus_abs):
        """v4l2-ctl을 사용해 카메라 고급 설정

        Args:
            camera_index: 카메라 인덱스
            exposure_abs: 노출 절대값
            focus_abs: 초점 절대값
        """
        try:
            device = f"/dev/video{camera_index}"

            # 자동 노출 끄기 (1 = manual mode)
            subprocess.run(
                ['v4l2-ctl', '-d', device, '-c', 'auto_exposure=1'],
                capture_output=True,
                timeout=2
            )

            # 노출 값 수동 설정
            subprocess.run(
                ['v4l2-ctl', '-d', device, '-c', f'exposure_absolute={exposure_abs}'],
                capture_output=True,
                timeout=2
            )

            # 자동 초점 끄기
            subprocess.run(
                ['v4l2-ctl', '-d', device, '-c', 'focus_auto=0'],
                capture_output=True,
                timeout=2
            )

            # 초점 값 수동 설정
            subprocess.run(
                ['v4l2-ctl', '-d', device, '-c', f'focus_absolute={focus_abs}'],
                capture_output=True,
                timeout=2
            )

            logger.info(f"✅ 카메라 {camera_index} v4l2 고급 설정 완료 (노출={exposure_abs}, 초점={focus_abs})")

        except Exception as e:
            logger.warning(f"⚠️  카메라 {camera_index} v4l2 설정 실패: {e}")

    def initialize_cameras(self):
        """양면 카메라 초기화"""
        try:
            # 좌측 카메라 (앞면) 초기화
            logger.info(f"좌측 카메라 초기화 중 (index: {self.left_camera_index})...")
            self.left_cap = cv2.VideoCapture(self.left_camera_index, cv2.CAP_V4L2)

            if not self.left_cap.isOpened():
                raise RuntimeError(f"좌측 카메라 열기 실패 (index: {self.left_camera_index})")

            # 좌측 카메라 설정
            self.left_cap.set(cv2.CAP_PROP_FRAME_WIDTH, CAMERA_WIDTH)
            self.left_cap.set(cv2.CAP_PROP_FRAME_HEIGHT, CAMERA_HEIGHT)
            self.left_cap.set(cv2.CAP_PROP_FPS, TARGET_FPS)
            self.left_cap.set(cv2.CAP_PROP_BRIGHTNESS, LEFT_CAM_BRIGHTNESS)
            self.left_cap.set(cv2.CAP_PROP_CONTRAST, LEFT_CAM_CONTRAST)
            self.left_cap.set(cv2.CAP_PROP_SATURATION, LEFT_CAM_SATURATION)
            self.left_cap.set(cv2.CAP_PROP_AUTO_EXPOSURE, 1)

            # v4l2 고급 설정 (좌측)
            self.setup_camera_v4l2(self.left_camera_index, LEFT_CAM_EXPOSURE_ABS, LEFT_CAM_FOCUS_ABS)

            logger.info("✅ 좌측 카메라 초기화 성공")

            # 우측 카메라 (뒷면) 초기화
            logger.info(f"우측 카메라 초기화 중 (index: {self.right_camera_index})...")
            self.right_cap = cv2.VideoCapture(self.right_camera_index, cv2.CAP_V4L2)

            if not self.right_cap.isOpened():
                raise RuntimeError(f"우측 카메라 열기 실패 (index: {self.right_camera_index})")

            # 우측 카메라 설정
            self.right_cap.set(cv2.CAP_PROP_FRAME_WIDTH, CAMERA_WIDTH)
            self.right_cap.set(cv2.CAP_PROP_FRAME_HEIGHT, CAMERA_HEIGHT)
            self.right_cap.set(cv2.CAP_PROP_FPS, TARGET_FPS)
            self.right_cap.set(cv2.CAP_PROP_BRIGHTNESS, RIGHT_CAM_BRIGHTNESS)
            self.right_cap.set(cv2.CAP_PROP_CONTRAST, RIGHT_CAM_CONTRAST)
            self.right_cap.set(cv2.CAP_PROP_SATURATION, RIGHT_CAM_SATURATION)
            self.right_cap.set(cv2.CAP_PROP_AUTO_EXPOSURE, 1)

            # v4l2 고급 설정 (우측)
            self.setup_camera_v4l2(self.right_camera_index, RIGHT_CAM_EXPOSURE_ABS, RIGHT_CAM_FOCUS_ABS)

            logger.info("✅ 우측 카메라 초기화 성공")

            # 초기 프레임 버리기 (카메라 안정화)
            for _ in range(10):
                self.left_cap.read()
                self.right_cap.read()
                time.sleep(0.1)

            logger.info("✅ 양면 카메라 초기화 완료")
            return True

        except Exception as e:
            logger.error(f"❌ 카메라 초기화 실패: {e}")
            return False

    def capture_frames(self):
        """양면 프레임 동시 촬영"""
        try:
            # 좌측 프레임 (앞면)
            ret_left, left_frame = self.left_cap.read()
            if not ret_left or left_frame is None:
                logger.error("좌측 프레임 촬영 실패")
                return None, None

            # 우측 프레임 (뒷면)
            ret_right, right_frame = self.right_cap.read()
            if not ret_right or right_frame is None:
                logger.error("우측 프레임 촬영 실패")
                return None, None

            return left_frame, right_frame

        except Exception as e:
            logger.error(f"프레임 촬영 오류: {e}")
            return None, None

    def send_frames(self, left_frame, right_frame):
        """양면 프레임을 서버로 전송"""
        try:
            # 프레임 인코딩
            _, left_buffer = cv2.imencode('.jpg', left_frame, [cv2.IMWRITE_JPEG_QUALITY, JPEG_QUALITY])
            _, right_buffer = cv2.imencode('.jpg', right_frame, [cv2.IMWRITE_JPEG_QUALITY, JPEG_QUALITY])

            # Base64 인코딩
            left_image_base64 = base64.b64encode(left_buffer).decode('utf-8')
            right_image_base64 = base64.b64encode(right_buffer).decode('utf-8')

            # 요청 데이터 구성
            payload = {
                'left_image': left_image_base64,
                'right_image': right_image_base64
            }

            # 서버로 전송
            response = requests.post(self.api_endpoint, json=payload, timeout=10)

            if response.status_code == 200:
                result = response.json()

                if result.get('status') == 'ok':
                    # 검증 결과 로깅
                    serial_number = result.get('serial_number', 'N/A')
                    product_code = result.get('product_code', 'N/A')
                    decision = result.get('decision', 'N/A')
                    gpio_pin = result.get('gpio_signal', {}).get('pin', 'N/A')

                    verification = result.get('verification', {})
                    missing_count = verification.get('missing_count', 0)
                    position_error_count = verification.get('position_error_count', 0)

                    logger.info(
                        f"✅ 검증 완료: 시리얼={serial_number}, 제품={product_code}, "
                        f"판정={decision}, GPIO={gpio_pin}, "
                        f"누락={missing_count}, 위치오류={position_error_count}"
                    )

                    # 아두이노로 GPIO 신호 전송
                    if self.arduino_handler:
                        self.arduino_handler.send_gpio_signal(gpio_pin, 300)

                    self.success_count += 1
                    return True
                else:
                    error_msg = result.get('error', 'Unknown error')
                    logger.error(f"❌ 서버 처리 실패: {error_msg}")
                    self.error_count += 1
                    return False
            else:
                logger.error(f"❌ HTTP 오류: {response.status_code}")
                self.error_count += 1
                return False

        except requests.exceptions.Timeout:
            logger.error("❌ 서버 응답 시간 초과 (10초)")
            self.error_count += 1
            return False
        except Exception as e:
            logger.error(f"❌ 전송 오류: {e}")
            self.error_count += 1
            return False

    def run(self):
        """메인 루프 실행"""
        logger.info("=" * 60)
        logger.info("양면 동시 촬영 클라이언트 시작")
        logger.info(f"서버 URL: {self.server_url}")
        logger.info(f"좌측 카메라 (앞면): index {self.left_camera_index}")
        logger.info(f"우측 카메라 (뒷면): index {self.right_camera_index}")
        logger.info(f"API 엔드포인트: {self.api_endpoint}")
        logger.info(f"아두이노 연결: {'활성화' if self.arduino_handler else '비활성화'}")
        logger.info("=" * 60)

        if not self.initialize_cameras():
            logger.error("카메라 초기화 실패. 종료합니다.")
            return

        logger.info("🎬 양면 촬영 시작...")

        frame_interval = 1.0 / TARGET_FPS
        last_capture_time = 0

        try:
            while True:
                current_time = time.time()

                # FPS 제어
                if current_time - last_capture_time < frame_interval:
                    time.sleep(0.01)
                    continue

                last_capture_time = current_time
                self.frame_count += 1

                # 양면 프레임 촬영
                left_frame, right_frame = self.capture_frames()

                if left_frame is None or right_frame is None:
                    logger.warning("프레임 촬영 실패, 재시도...")
                    continue

                # 서버로 전송
                self.send_frames(left_frame, right_frame)

                # 통계 출력 (10프레임마다)
                if self.frame_count % 10 == 0:
                    logger.info(
                        f"📊 통계: 총 {self.frame_count}프레임, "
                        f"성공 {self.success_count}, 실패 {self.error_count}"
                    )

        except KeyboardInterrupt:
            logger.info("\n사용자 중단 (Ctrl+C)")
        except Exception as e:
            logger.error(f"예상치 못한 오류: {e}")
        finally:
            self.cleanup()

    def cleanup(self):
        """리소스 정리"""
        logger.info("리소스 정리 중...")

        if self.left_cap:
            self.left_cap.release()
        if self.right_cap:
            self.right_cap.release()

        if self.arduino_handler:
            self.arduino_handler.close()

        logger.info("✅ 리소스 정리 완료")
        logger.info(f"최종 통계: 총 {self.frame_count}프레임, 성공 {self.success_count}, 실패 {self.error_count}")


def main():
    """메인 함수"""
    # 아두이노 핸들러 초기화
    arduino_handler = None
    if ARDUINO_ENABLED:
        try:
            arduino_handler = ArduinoSerialHandler(ARDUINO_PORT, ARDUINO_BAUDRATE)
            logger.info(f"✅ 아두이노 연결 성공: {ARDUINO_PORT}")
        except Exception as e:
            logger.warning(f"⚠️  아두이노 연결 실패: {e}")
            logger.warning("아두이노 없이 계속 진행합니다.")

    # 클라이언트 실행
    client = DualCameraClient(
        server_url=SERVER_URL,
        left_camera_index=LEFT_CAMERA_INDEX,
        right_camera_index=RIGHT_CAMERA_INDEX,
        arduino_handler=arduino_handler
    )

    client.run()


if __name__ == '__main__':
    main()
