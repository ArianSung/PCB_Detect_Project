#!/usr/bin/env python3
"""
라즈베리파이 이중 목적 클라이언트 (Dual Purpose Client)

기능:
1. 모니터링용 30fps 영상 스트리밍 (2개 카메라)
   - Flask 서버의 /upload_frame 엔드포인트로 전송
   - C# WinForms에서 MJPEG로 실시간 모니터링 가능

2. AI 추론 요청 (PCB 감지 시만)
   - Flask 서버의 /predict_dual 엔드포인트로 전송
   - GPIO 제어 신호를 받아서 릴레이 모듈 제어

사용법:
    # 기본 실행 (서버 URL 기본값: http://100.64.1.1:5000)
    python3 dual_purpose_client.py --left 0 --right 1

    # 서버 URL 지정
    python3 dual_purpose_client.py --left 0 --right 1 --server http://100.64.1.1:5000

    # FPS 조정 (기본 30fps)
    python3 dual_purpose_client.py --left 0 --right 1 --fps 25

    # PCB 감지 방식 지정 (기본: gpio)
    python3 dual_purpose_client.py --left 0 --right 1 --detection gpio

    # 모션 감지 방식 (테스트용)
    python3 dual_purpose_client.py --left 0 --right 1 --detection motion

주의사항:
    - 라즈베리파이 1번에서만 실행 (GPIO 제어 포함)
    - RPi.GPIO 라이브러리 필요: sudo apt install python3-rpi.gpio
    - 릴레이 모듈 연결 필요 (GPIO 17, 27, 22, 23)
"""

import cv2
import requests
import base64
import time
import os
import argparse
import threading
import logging
import signal
import sys
from datetime import datetime

# GPIO 라이브러리 임포트 (라즈베리파이에서만 작동)
try:
    import RPi.GPIO as GPIO
    GPIO_AVAILABLE = True
except ImportError:
    print("⚠ RPi.GPIO 라이브러리를 찾을 수 없습니다. GPIO 제어가 비활성화됩니다.")
    GPIO_AVAILABLE = False

# 로깅 설정
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
    handlers=[
        logging.FileHandler('/home/pi/dual_purpose_client.log'),
        logging.StreamHandler()
    ]
)
logger = logging.getLogger(__name__)

# GPIO 핀 매핑 (BCM 모드)
GPIO_PIN_COMPONENT_DEFECT = 17  # 부품 불량
GPIO_PIN_SOLDER_DEFECT = 27     # 납땜 불량
GPIO_PIN_DISCARD = 22           # 폐기
GPIO_PIN_NORMAL = 23            # 정상

# GPIO 센서 핀 (PCB 감지용 - 옵션)
GPIO_PIN_PCB_SENSOR = 24        # PCB 감지 센서 (적외선 센서 또는 포토 센서)

# 전역 변수
stop_event = threading.Event()  # 종료 시그널
pcb_detected_flag = threading.Event()  # PCB 감지 플래그


class DualPurposeClient:
    """이중 목적 클라이언트 (모니터링 + AI 추론)"""

    def __init__(self, server_url, fps=30, detection_mode='gpio'):
        """
        Args:
            server_url: Flask 서버 URL (예: http://100.64.1.1:5000)
            fps: 모니터링 스트리밍 FPS (기본 30)
            detection_mode: PCB 감지 방식 ('gpio' 또는 'motion')
        """
        self.server_url = server_url.rstrip('/')
        self.upload_url = f"{self.server_url}/upload_frame"
        self.predict_url = f"{self.server_url}/predict_dual"
        self.fps = fps
        self.frame_interval = 1.0 / fps
        self.detection_mode = detection_mode

        # 통계
        self.monitoring_count = {'left': 0, 'right': 0}
        self.inference_count = 0
        self.gpio_control_count = 0

        # GPIO 초기화
        if GPIO_AVAILABLE:
            self.init_gpio()

        logger.info(f"DualPurposeClient 초기화 완료")
        logger.info(f"  서버: {self.server_url}")
        logger.info(f"  FPS: {self.fps}")
        logger.info(f"  PCB 감지 방식: {self.detection_mode}")

    def init_gpio(self):
        """GPIO 초기화"""
        try:
            # BCM 모드 설정
            GPIO.setmode(GPIO.BCM)
            GPIO.setwarnings(False)

            # 출력 핀 설정 (릴레이 모듈 제어)
            GPIO.setup(GPIO_PIN_COMPONENT_DEFECT, GPIO.OUT, initial=GPIO.LOW)
            GPIO.setup(GPIO_PIN_SOLDER_DEFECT, GPIO.OUT, initial=GPIO.LOW)
            GPIO.setup(GPIO_PIN_DISCARD, GPIO.OUT, initial=GPIO.LOW)
            GPIO.setup(GPIO_PIN_NORMAL, GPIO.OUT, initial=GPIO.LOW)

            # 입력 핀 설정 (PCB 감지 센서 - 옵션)
            if self.detection_mode == 'gpio':
                GPIO.setup(GPIO_PIN_PCB_SENSOR, GPIO.IN, pull_up_down=GPIO.PUD_DOWN)

            logger.info("GPIO 초기화 성공")

        except Exception as e:
            logger.error(f"GPIO 초기화 실패: {e}")

    def cleanup_gpio(self):
        """GPIO 정리"""
        if GPIO_AVAILABLE:
            try:
                GPIO.cleanup()
                logger.info("GPIO 정리 완료")
            except Exception as e:
                logger.error(f"GPIO 정리 실패: {e}")

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

    def monitoring_thread(self, camera_id, camera_index):
        """
        모니터링용 30fps 스트리밍 스레드

        Args:
            camera_id: "left" 또는 "right"
            camera_index: 웹캠 장치 인덱스
        """
        logger.info(f"[{camera_id}] 모니터링 스레드 시작 (FPS: {self.fps})")

        # 웹캠 열기
        cap = cv2.VideoCapture(camera_index)

        if not cap.isOpened():
            logger.error(f"[{camera_id}] 웹캠 열기 실패: /dev/video{camera_index}")
            return

        # 해상도 설정
        cap.set(cv2.CAP_PROP_FRAME_WIDTH, 640)
        cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 480)

        try:
            while not stop_event.is_set():
                ret, frame = cap.read()

                if not ret:
                    logger.warning(f"[{camera_id}] 프레임 읽기 실패")
                    time.sleep(1)
                    continue

                try:
                    # Base64 인코딩
                    image_base64 = self.encode_frame(frame)

                    # Flask 서버로 전송
                    response = requests.post(
                        self.upload_url,
                        json={
                            "camera_id": camera_id,
                            "image": image_base64
                        },
                        timeout=5
                    )

                    if response.status_code == 200:
                        self.monitoring_count[camera_id] += 1

                        # 10초마다 로그 출력
                        if self.monitoring_count[camera_id] % (self.fps * 10) == 0:
                            logger.info(f"[{camera_id}] 모니터링 프레임: {self.monitoring_count[camera_id]}개 전송")
                    else:
                        logger.warning(f"[{camera_id}] HTTP 오류: {response.status_code}")

                except requests.exceptions.Timeout:
                    logger.warning(f"[{camera_id}] 타임아웃: Flask 서버 응답 없음")
                except Exception as e:
                    logger.error(f"[{camera_id}] 업로드 실패: {e}")

                # FPS 유지
                time.sleep(self.frame_interval)

        except KeyboardInterrupt:
            pass

        finally:
            cap.release()
            logger.info(f"[{camera_id}] 모니터링 스레드 종료 (총 {self.monitoring_count[camera_id]}프레임)")

    def inference_thread(self, left_index, right_index):
        """
        AI 추론용 스레드 (PCB 감지 시만)

        Args:
            left_index: 좌측 웹캠 장치 인덱스
            right_index: 우측 웹캠 장치 인덱스
        """
        logger.info("AI 추론 스레드 시작")

        # 웹캠 열기
        cap_left = cv2.VideoCapture(left_index)
        cap_right = cv2.VideoCapture(right_index)

        if not cap_left.isOpened() or not cap_right.isOpened():
            logger.error("AI 추론용 웹캠 열기 실패")
            return

        # 해상도 설정
        for cap in [cap_left, cap_right]:
            cap.set(cv2.CAP_PROP_FRAME_WIDTH, 640)
            cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 480)

        # 모션 감지용 변수
        prev_frame_left = None
        prev_frame_right = None
        motion_threshold = 5000  # 픽셀 변화량 임계값

        try:
            while not stop_event.is_set():
                # PCB 감지
                pcb_detected = self.detect_pcb(cap_left, cap_right, prev_frame_left, prev_frame_right, motion_threshold)

                if pcb_detected:
                    logger.info("PCB 감지! AI 추론 요청 시작")

                    # 양면 프레임 읽기
                    ret_left, left_frame = cap_left.read()
                    ret_right, right_frame = cap_right.read()

                    if ret_left and ret_right:
                        # Base64 인코딩
                        left_base64 = self.encode_frame(left_frame)
                        right_base64 = self.encode_frame(right_frame)

                        try:
                            # AI 추론 요청
                            start_time = time.time()
                            response = requests.post(
                                self.predict_url,
                                json={
                                    "left_image": left_base64,
                                    "right_image": right_base64
                                },
                                timeout=10
                            )
                            inference_time = (time.time() - start_time) * 1000  # ms

                            if response.status_code == 200:
                                result = response.json()

                                self.inference_count += 1
                                logger.info(f"AI 추론 완료 ({inference_time:.1f}ms)")
                                logger.info(f"  최종 판정: {result['final_decision']['decision']}")
                                logger.info(f"  부품 불량: {len(result['component_results']['defects'])}개")
                                logger.info(f"  납땜 불량: {len(result['solder_results']['defects'])}개")

                                # GPIO 제어
                                if GPIO_AVAILABLE and 'gpio_signal' in result:
                                    self.control_gpio(result['gpio_signal']['pin'])
                            else:
                                logger.error(f"AI 추론 실패: HTTP {response.status_code}")

                        except requests.exceptions.Timeout:
                            logger.error("AI 추론 타임아웃 (10초 초과)")
                        except Exception as e:
                            logger.error(f"AI 추론 오류: {e}")

                        # 모션 감지용 이전 프레임 업데이트
                        if self.detection_mode == 'motion':
                            prev_frame_left = cv2.cvtColor(left_frame, cv2.COLOR_BGR2GRAY)
                            prev_frame_right = cv2.cvtColor(right_frame, cv2.COLOR_BGR2GRAY)

                # 10Hz 체크 (100ms)
                time.sleep(0.1)

        except KeyboardInterrupt:
            pass

        finally:
            cap_left.release()
            cap_right.release()
            logger.info(f"AI 추론 스레드 종료 (총 {self.inference_count}회 추론)")

    def detect_pcb(self, cap_left, cap_right, prev_frame_left, prev_frame_right, motion_threshold):
        """
        PCB 감지 함수

        Args:
            cap_left: 좌측 웹캠 캡처 객체
            cap_right: 우측 웹캠 캡처 객체
            prev_frame_left: 이전 좌측 프레임 (모션 감지용)
            prev_frame_right: 이전 우측 프레임 (모션 감지용)
            motion_threshold: 모션 감지 임계값

        Returns:
            PCB 감지 여부 (bool)
        """
        if self.detection_mode == 'gpio':
            # GPIO 센서로 PCB 감지
            if GPIO_AVAILABLE:
                return GPIO.input(GPIO_PIN_PCB_SENSOR) == GPIO.HIGH
            else:
                # GPIO 사용 불가 시 모션 감지로 대체
                return self._detect_motion(cap_left, cap_right, prev_frame_left, prev_frame_right, motion_threshold)

        elif self.detection_mode == 'motion':
            # 모션 감지로 PCB 감지
            return self._detect_motion(cap_left, cap_right, prev_frame_left, prev_frame_right, motion_threshold)

        else:
            # 알 수 없는 모드
            logger.error(f"알 수 없는 PCB 감지 모드: {self.detection_mode}")
            return False

    def _detect_motion(self, cap_left, cap_right, prev_frame_left, prev_frame_right, motion_threshold):
        """
        모션 감지 함수 (프레임 차이 기반)

        Args:
            cap_left: 좌측 웹캠 캡처 객체
            cap_right: 우측 웹캠 캡처 객체
            prev_frame_left: 이전 좌측 프레임
            prev_frame_right: 이전 우측 프레임
            motion_threshold: 모션 감지 임계값

        Returns:
            모션 감지 여부 (bool)
        """
        # 첫 프레임은 건너뜀
        if prev_frame_left is None or prev_frame_right is None:
            return False

        # 현재 프레임 읽기
        ret_left, left_frame = cap_left.read()
        ret_right, right_frame = cap_right.read()

        if not ret_left or not ret_right:
            return False

        # 그레이스케일 변환
        gray_left = cv2.cvtColor(left_frame, cv2.COLOR_BGR2GRAY)
        gray_right = cv2.cvtColor(right_frame, cv2.COLOR_BGR2GRAY)

        # 프레임 차이 계산
        diff_left = cv2.absdiff(prev_frame_left, gray_left)
        diff_right = cv2.absdiff(prev_frame_right, gray_right)

        # 임계값 적용
        _, thresh_left = cv2.threshold(diff_left, 25, 255, cv2.THRESH_BINARY)
        _, thresh_right = cv2.threshold(diff_right, 25, 255, cv2.THRESH_BINARY)

        # 변화량 계산
        motion_left = cv2.countNonZero(thresh_left)
        motion_right = cv2.countNonZero(thresh_right)

        # 양면 모두 움직임 감지 시 PCB 진입으로 판단
        return (motion_left > motion_threshold and motion_right > motion_threshold)

    def control_gpio(self, gpio_pin):
        """
        GPIO 핀 제어 (릴레이 모듈)

        Args:
            gpio_pin: GPIO 핀 번호 (17, 27, 22, 23)
        """
        if not GPIO_AVAILABLE:
            logger.warning(f"GPIO 제어 불가 (RPi.GPIO 라이브러리 없음)")
            return

        try:
            # 모든 핀 OFF
            GPIO.output(GPIO_PIN_COMPONENT_DEFECT, GPIO.LOW)
            GPIO.output(GPIO_PIN_SOLDER_DEFECT, GPIO.LOW)
            GPIO.output(GPIO_PIN_DISCARD, GPIO.LOW)
            GPIO.output(GPIO_PIN_NORMAL, GPIO.LOW)

            # 해당 핀 ON (1초 동안)
            GPIO.output(gpio_pin, GPIO.HIGH)
            self.gpio_control_count += 1

            # 판정 결과 매핑
            decision_map = {
                GPIO_PIN_COMPONENT_DEFECT: "부품 불량",
                GPIO_PIN_SOLDER_DEFECT: "납땜 불량",
                GPIO_PIN_DISCARD: "폐기",
                GPIO_PIN_NORMAL: "정상"
            }

            logger.info(f"GPIO 제어: 핀 {gpio_pin} ({decision_map.get(gpio_pin, '알 수 없음')}) ON")

            time.sleep(1)  # 1초 동안 신호 유지

            # 핀 OFF
            GPIO.output(gpio_pin, GPIO.LOW)

        except Exception as e:
            logger.error(f"GPIO 제어 오류: {e}")

    def run(self, left_index, right_index):
        """
        클라이언트 실행 (3개 스레드 시작)

        Args:
            left_index: 좌측 웹캠 장치 인덱스
            right_index: 우측 웹캠 장치 인덱스
        """
        logger.info("DualPurposeClient 실행")
        logger.info(f"  좌측 카메라: /dev/video{left_index}")
        logger.info(f"  우측 카메라: /dev/video{right_index}")
        logger.info(f"  모니터링 FPS: {self.fps}")
        logger.info(f"  PCB 감지 방식: {self.detection_mode}")
        print(f"\n💡 웹 브라우저에서 MJPEG 스트림 확인:")
        print(f"   좌측: {self.server_url}/video_feed_annotated/left")
        print(f"   우측: {self.server_url}/video_feed_annotated/right")
        print(f"\n종료: Ctrl+C\n")

        # 스레드 생성
        t1 = threading.Thread(
            target=self.monitoring_thread,
            args=('left', left_index),
            daemon=True,
            name='MonitoringLeft'
        )
        t2 = threading.Thread(
            target=self.monitoring_thread,
            args=('right', right_index),
            daemon=True,
            name='MonitoringRight'
        )
        t3 = threading.Thread(
            target=self.inference_thread,
            args=(left_index, right_index),
            daemon=True,
            name='AIInference'
        )

        # 스레드 시작
        t1.start()
        t2.start()
        t3.start()

        # 메인 스레드는 대기 (Ctrl+C 대기)
        try:
            while True:
                time.sleep(1)

                # 30초마다 통계 출력
                if int(time.time()) % 30 == 0:
                    logger.info("=== 통계 ===")
                    logger.info(f"  모니터링 프레임: 좌측 {self.monitoring_count['left']}, 우측 {self.monitoring_count['right']}")
                    logger.info(f"  AI 추론 횟수: {self.inference_count}")
                    logger.info(f"  GPIO 제어 횟수: {self.gpio_control_count}")

        except KeyboardInterrupt:
            logger.info("사용자에 의해 중단됨 (Ctrl+C)")

        finally:
            # 종료 시그널 전송
            stop_event.set()

            # 스레드 종료 대기 (최대 5초)
            t1.join(timeout=5)
            t2.join(timeout=5)
            t3.join(timeout=5)

            # GPIO 정리
            self.cleanup_gpio()

            # 최종 통계
            logger.info("=== 최종 통계 ===")
            logger.info(f"  모니터링 프레임: 좌측 {self.monitoring_count['left']}, 우측 {self.monitoring_count['right']}")
            logger.info(f"  AI 추론 횟수: {self.inference_count}")
            logger.info(f"  GPIO 제어 횟수: {self.gpio_control_count}")
            logger.info("DualPurposeClient 종료")


def signal_handler(signum, frame):
    """시그널 핸들러 (Ctrl+C 등)"""
    logger.info(f"시그널 수신: {signum}")
    stop_event.set()
    sys.exit(0)


def main():
    """메인 함수"""
    parser = argparse.ArgumentParser(
        description='라즈베리파이 이중 목적 클라이언트 (모니터링 + AI 추론)',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog='''
사용 예시:
  # 기본 실행 (30fps 모니터링 + GPIO 센서 기반 AI 추론)
  python3 dual_purpose_client.py --left 0 --right 1

  # 서버 URL 지정
  python3 dual_purpose_client.py --left 0 --right 1 --server http://100.64.1.1:5000

  # FPS 조정 (25fps)
  python3 dual_purpose_client.py --left 0 --right 1 --fps 25

  # 모션 감지 방식 (테스트용)
  python3 dual_purpose_client.py --left 0 --right 1 --detection motion

웹 브라우저 확인:
  MJPEG 스트림 (YOLO 어노테이션 포함):
    좌측: http://서버주소:5000/video_feed_annotated/left
    우측: http://서버주소:5000/video_feed_annotated/right

  원본 스트림:
    http://서버주소:5000/viewer
        '''
    )

    # 필수 인자
    parser.add_argument('--left', type=int, required=True,
                       help='좌측 웹캠 장치 인덱스 (예: 0)')
    parser.add_argument('--right', type=int, required=True,
                       help='우측 웹캠 장치 인덱스 (예: 1)')

    # 옵션 인자
    parser.add_argument('--server', dest='server_url',
                       default=os.getenv('SERVER_URL', 'http://100.64.1.1:5000'),
                       help='Flask 서버 URL (기본값: 환경변수 SERVER_URL 또는 http://100.64.1.1:5000)')
    parser.add_argument('--fps', type=int, default=30,
                       help='모니터링 스트리밍 FPS (기본값: 30)')
    parser.add_argument('--detection', choices=['gpio', 'motion'], default='gpio',
                       help='PCB 감지 방식 (gpio: GPIO 센서, motion: 모션 감지, 기본값: gpio)')

    args = parser.parse_args()

    # 시그널 핸들러 등록
    signal.signal(signal.SIGINT, signal_handler)
    signal.signal(signal.SIGTERM, signal_handler)

    # 클라이언트 실행
    client = DualPurposeClient(
        server_url=args.server_url,
        fps=args.fps,
        detection_mode=args.detection
    )

    client.run(args.left, args.right)


if __name__ == '__main__':
    main()
