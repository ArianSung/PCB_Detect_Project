#!/usr/bin/env python3
"""
PCB 검사 시스템 - 라즈베리파이 카메라 클라이언트 (고급 설정 적용)

기능:
    - 서버로 이미지 실시간 전송
    - 고급 카메라 제어 (초점, 노출, 밝기, 대비, 채도)
    - .env 파일 또는 하드코딩된 기본값 사용

실행 방법:
    python camera_client.py
"""

import os
import cv2
import base64
import requests
import time
import logging
from datetime import datetime
from dotenv import load_dotenv

# .env 파일 로드
load_dotenv()

# --- [설정 1] 서버 및 기본 설정 ---
SERVER_URL = os.getenv('SERVER_URL', 'http://100.80.24.53:5000')
CAMERA_ID = os.getenv('CAMERA_ID', 'left')
CAMERA_INDEX = int(os.getenv('CAMERA_INDEX', 0))
CAMERA_WIDTH = int(os.getenv('CAMERA_WIDTH', 640))
CAMERA_HEIGHT = int(os.getenv('CAMERA_HEIGHT', 480))
JPEG_QUALITY = int(os.getenv('JPEG_QUALITY', 90))
TARGET_FPS = int(os.getenv('CAMERA_FPS', 30))

# --- [설정 2] 카메라 화질 파라미터 (사진의 값 적용) ---
# .env에 값이 없으면, 사진에서 찾으신 최적값을 기본으로 사용합니다.
CAM_BRIGHTNESS = int(os.getenv('CAM_BRIGHTNESS', 41))   # 사진 값: 41
CAM_CONTRAST = int(os.getenv('CAM_CONTRAST', 52))       # 사진 값: 52
CAM_SATURATION = int(os.getenv('CAM_SATURATION', 59))   # 사진 값: 59

# --- [설정 3] 하드웨어 제어 (노출/초점) ---
# 자동 기능을 끄고(0), 수동 값(Manual)을 적용합니다.
CAM_AUTO_EXPOSURE = 0  # 0: 수동, 1: 자동 (OpenCV 기준, 0.25/0.75 등 드라이버마다 다를 수 있음)
CAM_EXPOSURE_ABS = int(os.getenv('CAM_EXPOSURE', 1521)) # 사진 값: 1521

CAM_AUTO_FOCUS = 0     # 0: 수동, 1: 자동
CAM_FOCUS_ABS = int(os.getenv('CAM_FOCUS', 402))        # 사진 값: 402 (접사 중요!)


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

    def initialize_camera(self):
        """카메라 초기화 및 고급 파라미터 설정"""
        try:
            # OpenCV VideoCapture 초기화
            self.cap = cv2.VideoCapture(self.camera_index, cv2.CAP_V4L2) # V4L2 백엔드 명시

            if not self.cap.isOpened():
                raise RuntimeError(f"카메라 열기 실패 (index: {self.camera_index})")

            # 1. 해상도 및 FPS 설정
            self.cap.set(cv2.CAP_PROP_FRAME_WIDTH, CAMERA_WIDTH)
            self.cap.set(cv2.CAP_PROP_FRAME_HEIGHT, CAMERA_HEIGHT)
            self.cap.set(cv2.CAP_PROP_FPS, TARGET_FPS)

            # 2. 소프트웨어/하드웨어 화질 설정 (밝기, 대비, 채도)
            # 주의: 일부 저가형 웹캠은 이 설정을 지원하지 않을 수 있음
            self.cap.set(cv2.CAP_PROP_BRIGHTNESS, CAM_BRIGHTNESS)
            self.cap.set(cv2.CAP_PROP_CONTRAST, CAM_CONTRAST)
            self.cap.set(cv2.CAP_PROP_SATURATION, CAM_SATURATION)

            # 3. 노출(Exposure) 제어
            # 자동 노출 끄기 (V4L2 backend: 1=Manual, 3=Auto)
            self.cap.set(cv2.CAP_PROP_AUTO_EXPOSURE, 1) 
            # 수동 노출 값 적용
            self.cap.set(cv2.CAP_PROP_EXPOSURE, CAM_EXPOSURE_ABS)

            # 4. 초점(Focus) 제어 (가장 중요!)
            # 자동 초점 끄기
            self.cap.set(cv2.CAP_PROP_AUTOFOCUS, 0)
            # 수동 초점 값 적용
            self.cap.set(cv2.CAP_PROP_FOCUS, CAM_FOCUS_ABS)

            # --- 설정 확인 로그 ---
            actual_w = int(self.cap.get(cv2.CAP_PROP_FRAME_WIDTH))
            actual_h = int(self.cap.get(cv2.CAP_PROP_FRAME_HEIGHT))
            actual_focus = int(self.cap.get(cv2.CAP_PROP_FOCUS))
            actual_exposure = int(self.cap.get(cv2.CAP_PROP_EXPOSURE))

            logger.info(f"✅ 카메라 초기화 완료 (ID: {self.camera_id})")
            logger.info(f"   - 해상도: {actual_w}x{actual_h}")
            logger.info(f"   - 초점(Focus): 설정값({CAM_FOCUS_ABS}) -> 실제값({actual_focus})")
            logger.info(f"   - 노출(Exp): 설정값({CAM_EXPOSURE_ABS}) -> 실제값({actual_exposure})")
            logger.info(f"   - B/C/S: {CAM_BRIGHTNESS}/{CAM_CONTRAST}/{CAM_SATURATION}")

        except Exception as e:
            logger.error(f"❌ 카메라 초기화 실패: {e}")
            raise

    def capture_and_send(self):
        """프레임 캡처 및 서버 전송"""
        ret, frame = self.cap.read()

        if not ret or frame is None:
            logger.error("프레임 캡처 실패")
            self.error_count += 1
            # 카메라 재연결 시도 로직이 필요할 수도 있음
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
                timeout=5.0
            )
            request_time_ms = (time.time() - start_time) * 1000

            # 응답 확인
            if response.status_code == 200:
                data = response.json()
                defect_type = data.get('defect_type', 'N/A')
                confidence = data.get('confidence', 0.0)
                inference_time = data.get('inference_time_ms', 0.0)

                self.frame_count += 1
                self.success_count += 1

                if self.frame_count % 30 == 0:  # 로그 너무 자주 뜨지 않게 30프레임마다
                    logger.info(
                        f"[{self.camera_id} #{self.frame_count}] "
                        f"판정: {defect_type} ({confidence:.2f}) | "
                        f"처리: {inference_time:.1f}ms | "
                        f"전송: {request_time_ms:.1f}ms"
                    )
                return True
            else:
                logger.error(f"서버 오류: {response.status_code}")
                self.error_count += 1
                return False

        except requests.exceptions.RequestException:
            # 네트워크 오류는 너무 자주 찍히면 로그가 지저분하므로 간략히
            logger.warning(f"서버 연결 실패 ({self.server_url})")
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
            logger.info("카메라 클라이언트 시작 (고급 제어 모드)")
            logger.info(f"Target FPS: {TARGET_FPS}")
            logger.info("=" * 60)

            frame_interval = 1.0 / TARGET_FPS
            last_frame_time = time.time()

            while True:
                current_time = time.time()
                elapsed = current_time - last_frame_time

                if elapsed >= frame_interval:
                    self.capture_and_send()
                    last_frame_time = current_time
                else:
                    time.sleep(0.001) # CPU 과부하 방지용 짧은 대기

        except KeyboardInterrupt:
            logger.info("종료 요청 (Ctrl+C)")
        except Exception as e:
            logger.error(f"치명적 오류: {e}", exc_info=True)
        finally:
            if self.cap is not None:
                self.cap.release()
                logger.info("카메라 리소스 해제됨")

if __name__ == '__main__':
    client = CameraClient(SERVER_URL, CAMERA_ID, CAMERA_INDEX)
    client.run()