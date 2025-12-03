#!/usr/bin/env python3
"""
PCB 검사 시스템 - 뒷면 카메라 클라이언트 (시리얼 넘버 OCR)

실행 방법:
    python back_camera_client.py

환경 변수 (.env 파일):
    SERVER_URL=http://100.80.24.53:5000
    CAMERA_INDEX=1
    FRAME_SIZE=640
    JPEG_QUALITY=85
    TARGET_FPS=10
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
SERVER_URL = os.getenv('SERVER_URL', 'http://100.80.24.53:5000')
CAMERA_INDEX = int(os.getenv('BACK_CAMERA_INDEX', 1))  # 뒷면 카메라는 인덱스 1
CAMERA_WIDTH = int(os.getenv('CAMERA_WIDTH', 640))
CAMERA_HEIGHT = int(os.getenv('CAMERA_HEIGHT', 480))
JPEG_QUALITY = int(os.getenv('JPEG_QUALITY', 85))
TARGET_FPS = int(os.getenv('CAMERA_FPS', 10))  # 뒷면은 10 FPS면 충분

# 로깅 설정
logging.basicConfig(
    level=logging.INFO,
    format='[%(asctime)s] [%(levelname)s] [Back-Camera-Client] - %(message)s'
)
logger = logging.getLogger(__name__)


class BackCameraClient:
    """뒷면 카메라 클라이언트 클래스 (시리얼 넘버 OCR)"""

    def __init__(self, server_url, camera_index):
        self.server_url = server_url
        self.camera_index = camera_index
        self.api_endpoint = f"{server_url}/predict_serial"  # 시리얼 넘버 OCR API
        self.cap = None
        self.frame_count = 0
        self.success_count = 0
        self.error_count = 0
        self.last_product_code = None  # 마지막으로 검출된 제품 코드

    def setup_camera_v4l2(self):
        """v4l2-ctl을 사용해 카메라 설정"""
        try:
            device = f"/dev/video{self.camera_index}"

            # 자동 노출 끄기 (1 = manual mode)
            subprocess.run(
                ['v4l2-ctl', '-d', device, '-c', 'auto_exposure=1'],
                capture_output=True,
                timeout=2
            )

            # 노출 값 수동 설정 (밝게 - 시리얼 넘버 읽기 위해)
            subprocess.run(
                ['v4l2-ctl', '-d', device, '-c', 'exposure_absolute=200'],
                capture_output=True,
                timeout=2
            )

            # 밝기 조정
            subprocess.run(
                ['v4l2-ctl', '-d', device, '-c', 'brightness=140'],
                capture_output=True,
                timeout=2
            )

            # 선명도 증가 (텍스트 읽기 위해)
            subprocess.run(
                ['v4l2-ctl', '-d', device, '-c', 'sharpness=4'],
                capture_output=True,
                timeout=2
            )

            logger.info(f"✅ 뒷면 카메라 설정 완료 (device: {device})")
            logger.info(f"   - 자동 노출: OFF")
            logger.info(f"   - 노출 값: 200 (밝게)")
            logger.info(f"   - 밝기: 140")
            logger.info(f"   - 선명도: 4 (텍스트 읽기용)")
        except Exception as e:
            logger.warning(f"⚠️  v4l2-ctl 설정 실패 (계속 진행): {e}")

    def init_camera(self):
        """카메라 초기화"""
        logger.info(f"🎥 카메라 초기화 중... (인덱스: {self.camera_index})")

        # v4l2 설정 먼저 적용
        self.setup_camera_v4l2()

        # OpenCV 카메라 열기
        self.cap = cv2.VideoCapture(self.camera_index, cv2.CAP_V4L2)

        if not self.cap.isOpened():
            raise RuntimeError(f"❌ 카메라를 열 수 없습니다 (인덱스: {self.camera_index})")

        # 해상도 설정
        self.cap.set(cv2.CAP_PROP_FRAME_WIDTH, CAMERA_WIDTH)
        self.cap.set(cv2.CAP_PROP_FRAME_HEIGHT, CAMERA_HEIGHT)
        self.cap.set(cv2.CAP_PROP_FPS, TARGET_FPS)

        # 실제 설정된 값 확인
        actual_width = int(self.cap.get(cv2.CAP_PROP_FRAME_WIDTH))
        actual_height = int(self.cap.get(cv2.CAP_PROP_FRAME_HEIGHT))
        actual_fps = int(self.cap.get(cv2.CAP_PROP_FPS))

        logger.info(f"✅ 뒷면 카메라 초기화 완료")
        logger.info(f"   - 해상도: {actual_width}x{actual_height}")
        logger.info(f"   - FPS: {actual_fps}")
        logger.info(f"   - JPEG 품질: {JPEG_QUALITY}")

    def capture_frame(self):
        """프레임 캡처"""
        if self.cap is None or not self.cap.isOpened():
            raise RuntimeError("카메라가 초기화되지 않았습니다")

        ret, frame = self.cap.read()
        if not ret:
            raise RuntimeError("프레임 캡처 실패")

        return frame

    def encode_frame(self, frame):
        """프레임을 JPEG로 인코딩하고 Base64로 변환"""
        encode_param = [int(cv2.IMWRITE_JPEG_QUALITY), JPEG_QUALITY]
        _, buffer = cv2.imencode('.jpg', frame, encode_param)
        jpg_as_text = base64.b64encode(buffer).decode('utf-8')
        return jpg_as_text

    def send_to_server(self, frame_base64):
        """프레임을 서버에 전송하고 시리얼 넘버 OCR 결과 수신"""
        try:
            payload = {
                'camera_id': 'right',  # 뒷면 카메라
                'frame': frame_base64,
                'timestamp': datetime.now().isoformat()
            }

            response = requests.post(
                self.api_endpoint,
                json=payload,
                timeout=5.0
            )

            if response.status_code == 200:
                result = response.json()
                self.success_count += 1

                # 시리얼 넘버 검출 결과 확인
                if result.get('status') == 'ok':
                    serial_number = result.get('serial_number')
                    product_code = result.get('product_code')
                    confidence = result.get('confidence', 0.0)

                    if product_code:
                        self.last_product_code = product_code
                        logger.info(f"✅ 제품 검출: {serial_number} → 제품 코드: {product_code} (신뢰도: {confidence:.2%})")
                    else:
                        logger.info(f"⚠️  시리얼 넘버 미검출")

                    return result
                else:
                    logger.warning(f"⚠️  서버 응답 오류: {result.get('error', 'Unknown')}")
                    return None
            else:
                logger.error(f"❌ HTTP 오류: {response.status_code}")
                self.error_count += 1
                return None

        except requests.exceptions.Timeout:
            logger.error("❌ 서버 응답 시간 초과")
            self.error_count += 1
            return None
        except Exception as e:
            logger.error(f"❌ 전송 오류: {e}")
            self.error_count += 1
            return None

    def run(self):
        """메인 루프 실행"""
        logger.info("=" * 60)
        logger.info("PCB 뒷면 검사 시스템 - 시리얼 넘버 OCR 클라이언트")
        logger.info("=" * 60)
        logger.info(f"서버 URL: {self.server_url}")
        logger.info(f"API 엔드포인트: {self.api_endpoint}")
        logger.info(f"카메라 인덱스: {self.camera_index}")
        logger.info(f"목표 FPS: {TARGET_FPS}")
        logger.info("=" * 60)

        # 카메라 초기화
        self.init_camera()

        # FPS 제어 변수
        frame_interval = 1.0 / TARGET_FPS
        last_frame_time = time.time()

        logger.info("🚀 시리얼 넘버 OCR 시작...")
        logger.info("   - Ctrl+C로 종료")

        try:
            while True:
                current_time = time.time()
                elapsed = current_time - last_frame_time

                # FPS 제어
                if elapsed < frame_interval:
                    time.sleep(frame_interval - elapsed)
                    continue

                last_frame_time = current_time
                self.frame_count += 1

                # 프레임 캡처
                frame = self.capture_frame()

                # 프레임 인코딩
                frame_base64 = self.encode_frame(frame)

                # 서버로 전송
                result = self.send_to_server(frame_base64)

                # 5초마다 통계 출력
                if self.frame_count % (TARGET_FPS * 5) == 0:
                    total = self.success_count + self.error_count
                    success_rate = (self.success_count / total * 100) if total > 0 else 0.0
                    logger.info(f"📊 통계: 프레임={self.frame_count}, 성공={self.success_count}, "
                               f"실패={self.error_count}, 성공률={success_rate:.1f}%")
                    if self.last_product_code:
                        logger.info(f"   - 마지막 검출 제품: {self.last_product_code}")

        except KeyboardInterrupt:
            logger.info("\n🛑 사용자 중단 요청")
        except Exception as e:
            logger.error(f"❌ 예상치 못한 오류: {e}", exc_info=True)
        finally:
            self.cleanup()

    def cleanup(self):
        """리소스 정리"""
        logger.info("🧹 리소스 정리 중...")

        if self.cap is not None:
            self.cap.release()
            logger.info("   - 카메라 해제 완료")

        total = self.success_count + self.error_count
        success_rate = (self.success_count / total * 100) if total > 0 else 0.0

        logger.info("=" * 60)
        logger.info("최종 통계")
        logger.info("=" * 60)
        logger.info(f"총 프레임 수: {self.frame_count}")
        logger.info(f"성공: {self.success_count}")
        logger.info(f"실패: {self.error_count}")
        logger.info(f"성공률: {success_rate:.1f}%")
        if self.last_product_code:
            logger.info(f"마지막 검출 제품: {self.last_product_code}")
        logger.info("=" * 60)
        logger.info("✅ 클라이언트 종료")


if __name__ == '__main__':
    # 클라이언트 생성 및 실행
    client = BackCameraClient(
        server_url=SERVER_URL,
        camera_index=CAMERA_INDEX
    )

    try:
        client.run()
    except Exception as e:
        logger.error(f"❌ 클라이언트 실행 실패: {e}", exc_info=True)
        exit(1)
