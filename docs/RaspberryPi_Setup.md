# 라즈베리파이 4 PCB 검사 시스템 설정 가이드 ⭐ (제품별 검증 아키텍처 v3.0)

## 개요

이 가이드는 라즈베리파이 4를 사용하여 **뒷면 제품 식별 → 앞면 부품 검증** 순차 파이프라인과 GPIO/OHT 제어를 구성하는 방법을 설명합니다.

**⭐ 제품별 검증 파이프라인**:
- **뒷면 식별 (Backscan)**: 우측 카메라 → 시리얼 넘버 OCR + QR 코드 스캔 → 제품 코드 및 기준 데이터 결정
- **앞면 검증 (Frontscan)**: 좌측 카메라 → YOLO 부품 검출 + ComponentVerifier → missing/position_error/extra 판단
- **최종 판정**: normal / missing / position_error / discard → GPIO + 로봇팔 제어

**시스템 구성**:
- **라즈베리파이 1 (Tailscale: 100.64.1.2)**: 좌측 웹캠(앞면) + GPIO 출력 + 로봇팔/릴레이 제어. 라즈베리파이 2에서 전달받은 제품 코드로 앞면 검증을 수행하고 최종 응답을 수신합니다.
- **라즈베리파이 2 (Tailscale: 100.64.1.3)**: 우측 웹캠(뒷면) 전용. 시리얼/QR을 읽어 Flask 서버의 Backscan API를 호출한 뒤, 발급된 `inspection_token` 을 라즈베리파이 1에 전달합니다.
- **라즈베리파이 3 (Tailscale: 100.64.1.4)**: OHT/레일 전용 제어기 ⭐ (pigpio 기반 스텝모터 + 서보 컨트롤)

---

## 하드웨어 요구사항

### 라즈베리파이 4 사양
- **모델**: Raspberry Pi 4 Model B
- **RAM**: 4GB 이상 권장 (2GB도 가능)
- **저장장치**: microSD 카드 32GB 이상 (Class 10, A1/A2 권장)
- **전원**: 5V 3A USB-C 어댑터

### 추가 하드웨어
- **웹캠**: USB 웹캠 (720p 이상)
- **릴레이 모듈**: 4채널 릴레이 모듈 (5V)
- **점퍼 와이어**: GPIO 연결용
- **케이스**: 라즈베리파이 4용 케이스 (방열판 포함)

---

## 소프트웨어 환경

### 운영체제
- **Raspberry Pi OS (64-bit)** - Bullseye 또는 Bookworm
- Python 3.10+

### 주요 라이브러리
- OpenCV
- RPi.GPIO (라즈베리파이 1)
- pigpio (라즈베리파이 3번 OHT 컨트롤러)
- Requests
- Pillow

---

## Phase 1: 라즈베리파이 OS 설치

### 1-1. Raspberry Pi Imager 사용

1. **Raspberry Pi Imager 다운로드**
   - https://www.raspberrypi.com/software/

2. **OS 선택**
   - "Raspberry Pi OS (64-bit)"
   - **추천**: Raspberry Pi OS Lite (데스크톱 불필요 시)

3. **설정**
   - 톱니바퀴 아이콘 클릭 → 고급 옵션
   - 호스트명: `raspberrypi-left`, `raspberrypi-right`, `raspberrypi-oht`
   - SSH 활성화: ✅
   - 사용자명: `pi`
   - 비밀번호: 원하는 비밀번호
   - Wi-Fi 설정 (선택)

4. **이미지 쓰기**
   - microSD 카드 선택 → 쓰기

### 1-2. 초기 부팅 및 SSH 접속

```bash
# Windows에서 (PowerShell 또는 PuTTY)
ssh pi@raspberrypi-left.local

# 또는 Tailscale IP로 접속
ssh pi@100.64.1.2
```

### 1-3. 시스템 업데이트

```bash
sudo apt update
sudo apt upgrade -y
sudo reboot
```

---

## Phase 2: Python 환경 구축

### 2-1. Python 3 확인

```bash
python3 --version
# 출력: Python 3.10.x
```

### 2-2. 필수 패키지 설치

```bash
# 시스템 패키지
sudo apt install -y python3-pip python3-opencv python3-dev
sudo apt install -y libatlas-base-dev libhdf5-dev libhdf5-serial-dev
sudo apt install -y libjpeg-dev zlib1g-dev libfreetype6-dev liblcms2-dev
sudo apt install -y libopenblas-dev

# v4l-utils (웹캠 관리)
sudo apt install -y v4l-utils
```

### 2-3. Python 라이브러리 설치

```bash
# pip 업그레이드
pip3 install --upgrade pip

# 주요 라이브러리
pip3 install opencv-python
pip3 install requests
pip3 install Pillow
pip3 install RPi.GPIO
pip3 install numpy
```

---

## Phase 3: 웹캠 설정

### 3-1. 웹캠 연결 확인

```bash
# 연결된 비디오 장치 확인
ls /dev/video*
# 출력 예: /dev/video0

# 웹캠 정보 확인
v4l2-ctl --list-devices

# 지원 해상도 확인
v4l2-ctl -d /dev/video0 --list-formats-ext
```

### 3-2. 웹캠 테스트 스크립트

**test_camera.py**

```python
import cv2
import sys

def test_camera(device_id=0):
    """웹캠 테스트"""
    cap = cv2.VideoCapture(device_id)

    if not cap.isOpened():
        print(f"Error: Cannot open camera {device_id}")
        return False

    # 해상도 설정
    cap.set(cv2.CAP_PROP_FRAME_WIDTH, 640)
    cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 480)

    # 프레임 읽기
    ret, frame = cap.read()
    if ret:
        print(f"✅ Camera {device_id} working!")
        print(f"   Resolution: {frame.shape[1]}x{frame.shape[0]}")

        # 프레임 저장
        cv2.imwrite('test_frame.jpg', frame)
        print("   Test frame saved as 'test_frame.jpg'")
    else:
        print(f"❌ Cannot read frame from camera {device_id}")
        return False

    cap.release()
    return True

if __name__ == '__main__':
    device_id = int(sys.argv[1]) if len(sys.argv) > 1 else 0
    test_camera(device_id)
```

실행:
```bash
python3 test_camera.py 0
```

---

## Phase 4: GPIO 설정 및 릴레이 제어 ⭐ 라즈베리파이 1 전용 (이중 모델 융합 결과 기반)

**⭐ 이중 모델 아키텍처에서의 GPIO 제어**:
- Flask 서버가 **양면(좌측+우측) 동시 검사** 후 두 모델 결과를 융합 (Result Fusion)
- 최종 판정 (normal, missing, position_error, discard)을 라즈베리파이 1에 전송
- GPIO 제어는 **융합 결과(fusion_result)**에 따라 실행

**중요**: GPIO 제어는 **라즈베리파이 1 (100.64.1.2)에만** 적용됩니다.
- 라즈베리파이 2 (100.64.1.3)와 라즈베리파이 3번 (100.64.1.4)는 카메라/OHT 전용 (GPIO 사용 안 함)

### 4-1. GPIO 핀 매핑 (BCM 모드)

```
라즈베리파이 4 GPIO 핀아웃 (BCM 번호):

         3V3  (1) (2)  5V
       GPIO2  (3) (4)  5V
       GPIO3  (5) (6)  GND
       GPIO4  (7) (8)  GPIO14
         GND  (9) (10) GPIO15
      GPIO17 (11) (12) GPIO18
      GPIO27 (13) (14) GND
      GPIO22 (15) (16) GPIO23
         3V3 (17) (18) GPIO24
      GPIO10 (19) (20) GND
       GPIO9 (21) (22) GPIO25
      GPIO11 (23) (24) GPIO8
         GND (25) (26) GPIO7
...

[불량 분류용 GPIO 핀]
- GPIO 17 (BCM 11) → 부품 불량 (릴레이 채널 1)
- GPIO 27 (BCM 13) → 위치 오류 (릴레이 채널 2)
- GPIO 22 (BCM 15) → 폐기 (릴레이 채널 3)
- GPIO 23 (BCM 16) → 정상 (릴레이 채널 4)
```

### 4-2. 4채널 릴레이 모듈 연결

```
릴레이 모듈 → 라즈베리파이
VCC        → 5V (핀 2 또는 4)
GND        → GND (핀 6, 9, 14, 20, 25 중 아무거나)
IN1        → GPIO 17 (핀 11)
IN2        → GPIO 27 (핀 13)
IN3        → GPIO 22 (핀 15)
IN4        → GPIO 23 (핀 16)
```

### 4-3. GPIO 테스트 스크립트

**test_gpio.py**

```python
import RPi.GPIO as GPIO
import time

# GPIO 핀 정의 (BCM 모드)
PIN_COMPONENT_DEFECT = 17  # 부품 불량
PIN_SOLDER_DEFECT = 27     # 위치 오류
PIN_DISCARD = 22           # 폐기
PIN_NORMAL = 23            # 정상

# GPIO 초기화
GPIO.setmode(GPIO.BCM)
GPIO.setwarnings(False)

# 출력 핀 설정
pins = [PIN_COMPONENT_DEFECT, PIN_SOLDER_DEFECT, PIN_DISCARD, PIN_NORMAL]
for pin in pins:
    GPIO.setup(pin, GPIO.OUT)
    GPIO.output(pin, GPIO.LOW)  # 초기 상태: LOW

def trigger_gpio(pin, duration_ms=500):
    """GPIO 핀을 지정된 시간 동안 HIGH로 설정"""
    print(f"Triggering GPIO {pin} for {duration_ms}ms")
    GPIO.output(pin, GPIO.HIGH)
    time.sleep(duration_ms / 1000.0)
    GPIO.output(pin, GPIO.LOW)

if __name__ == '__main__':
    try:
        print("GPIO 릴레이 테스트 시작...")

        print("\n1. 부품 불량 신호 (GPIO 17)")
        trigger_gpio(PIN_COMPONENT_DEFECT, 500)
        time.sleep(1)

        print("\n2. 위치 오류 신호 (GPIO 27)")
        trigger_gpio(PIN_SOLDER_DEFECT, 500)
        time.sleep(1)

        print("\n3. 폐기 신호 (GPIO 22)")
        trigger_gpio(PIN_DISCARD, 500)
        time.sleep(1)

        print("\n4. 정상 신호 (GPIO 23)")
        trigger_gpio(PIN_NORMAL, 500)

        print("\n✅ GPIO 테스트 완료!")

    finally:
        GPIO.cleanup()
```

실행:
```bash
sudo python3 test_gpio.py
```

**주의**: GPIO 제어는 root 권한 필요 (`sudo`)

---

## Phase 5: Backscan + Frontscan 파이프라인 (v3.0)

라즈베리파이 2(우측)는 **Backscan** 전용, 라즈베리파이 1(좌측)은 **Frontscan + GPIO 제어** 전용으로 동작합니다. 두 디바이스는 `inspection_token` 으로 동일한 PCB를 식별합니다.

### 5-1. Backscan 클라이언트 (라즈베리파이 2)

1. 우측 카메라에서 프레임를 촬영하고 640x480으로 리사이즈합니다.
2. `base64` 로 인코딩한 뒤 Flask 서버 `POST /api/v3/backscan` (세부 사항은 `docs/Flask_Server_Setup.md`) 로 전송합니다.
3. 서버는 Serial OCR + QR 디코딩을 수행하고 다음 정보를 반환합니다.
   ```json
   {
     "inspection_token": "20251130-FT-000123",
     "product_code": "FT",
     "serial_number": "MBFT00012345",
     "backscan_status": "ok"
   }
   ```
4. 라즈베리파이 2는 이 응답을 로컬 메시지 큐/Redis/파일(`tmp/latest_backscan.json`)에 저장하고 라즈베리파이 1에 전달합니다.

```python
payload = {
    \"camera_id\": \"right\",
    \"frame\": encode_frame(frame),
    \"request_id\": str(uuid.uuid4())
}
r = requests.post(f\"{SERVER_URL}/api/v3/backscan\", json=payload, timeout=5)
result = r.json()
token = result[\"inspection_token\"]
publish_token(token, result[\"product_code\"], result[\"serial_number\"])
```

### 5-2. Frontscan + GPIO (라즈베리파이 1)

1. 메시지 큐에서 `inspection_token` 이 도착하면 좌측 카메라에서 앞면을 촬영합니다.
2. 캡처된 프레임과 token, product_code 를 `POST /api/v3/frontscan` 으로 전송합니다.
3. Flask 서버는 YOLOv11l 부품 검출 + ComponentVerifier 로 missing/position_error/extra 부품을 계산하고 최종 판정을 반환합니다.
4. 라즈베리파이 1은 응답을 기준으로 GPIO/로봇팔을 제어합니다.

```python
front_payload = {
    \"inspection_token\": token,
    \"product_code\": product_code,
    \"frame\": encode_frame(front_frame),
    \"camera_id\": \"left\",
    \"gpio_enabled\": True
}
r = requests.post(f\"{SERVER_URL}/api/v3/frontscan\", json=front_payload, timeout=5)
decision = r.json()[\"decision\"]  # normal/missing/position_error/discard
gpio_controller.trigger(decision, duration_ms=500)
```

### 5-3. inspection_token 전달 전략

- **Redis Pub/Sub**: 가장 권장. 라즈베리파이 2가 `backscan:token` 채널로 발행 → 라즈베리파이 1이 구독.
- **파일 기반**: 간단한 테스트용. `/tmp/backscan_token.json` 에 쓰고 `inotify` 로 감지.
- **MQTT**: 이미 MQTT 브로커가 있다면 `pcb/backscan` 토픽 사용.

토큰에는 최소한 `inspection_token`, `product_code`, `serial_number`, `timestamp` 를 포함시키고, 30초 내 소비되지 않으면 만료 처리합니다.

> 📌 **Legacy 이중 모델 자료**는 아래 [아카이브 섹션](#아카이브-phase-5-dual-model-architecture) 에 남겨 두었습니다.

---

## [아카이브] Phase 5: Flask Client 및 GPIO 통합

### 5-1. 프로젝트 구조 (이중 모델 아키텍처)

```
~/pcb_inspection_client/
├── dual_camera_client.py  # 양면 동시 캡처 + GPIO 통합 클라이언트 ⭐ 신규
├── gpio_controller.py     # GPIO 제어 모듈 (융합 결과 기반)
├── config.py              # 설정 파일
├── test_camera.py         # 카메라 테스트 스크립트
└── start.sh               # 자동 시작 스크립트
```

**⭐ 주요 변경사항**:
- `camera_client.py` (단일 카메라) → `dual_camera_client.py` (양면 동시 캡처)
- API 엔드포인트: `/predict` → `/predict_dual`
- GPIO 제어: 단일 모델 결과 → 융합 결과 (normal, missing, position_error, discard)

### 5-2. GPIO 제어 모듈

**gpio_controller.py**

```python
import RPi.GPIO as GPIO
import time
import logging

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

class GPIOController:
    """GPIO 릴레이 제어 클래스 (이중 모델 융합 결과 기반)"""

    # GPIO 핀 매핑 (Flask API 융합 결과와 매칭)
    PIN_MAP = {
        'normal': 23,            # 정상
        'missing': 17,           # 부품 누락
        'position_error': 27,    # 위치 오류
        'discard': 22            # 폐기
    }

    # 한글 매핑 (호환성)
    PIN_MAP_KR = {
        '정상': 23,
        '부품 누락': 17,
        '위치 오류': 27,
        '폐기': 22
    }

    def __init__(self):
        """GPIO 초기화"""
        GPIO.setmode(GPIO.BCM)
        GPIO.setwarnings(False)

        # 모든 핀을 출력으로 설정
        for pin in self.PIN_MAP.values():
            GPIO.setup(pin, GPIO.OUT)
            GPIO.output(pin, GPIO.LOW)

        logger.info("GPIO 컨트롤러 초기화 완료")

    def trigger(self, defect_type, duration_ms=500):
        """
        불량 유형에 따라 GPIO 신호 출력 (이중 모델 융합 결과 기반)

        Args:
            defect_type: 'normal', 'missing', 'position_error', 'discard'
                        또는 한글: '정상', '부품 누락', '위치 오류', '폐기'
            duration_ms: 신호 지속 시간 (밀리초)
        """
        # 영문 키 우선, 한글 키 호환
        pin = self.PIN_MAP.get(defect_type) or self.PIN_MAP_KR.get(defect_type)

        if pin is None:
            logger.warning(f"알 수 없는 불량 유형: {defect_type}")
            return

        logger.info(f"GPIO 신호 출력: {defect_type} (핀 {pin}, {duration_ms}ms)")

        try:
            GPIO.output(pin, GPIO.HIGH)
            time.sleep(duration_ms / 1000.0)
            GPIO.output(pin, GPIO.LOW)
        except Exception as e:
            logger.error(f"GPIO 제어 오류: {str(e)}")

    def cleanup(self):
        """GPIO 정리"""
        GPIO.cleanup()
        logger.info("GPIO 정리 완료")

# 전역 GPIO 컨트롤러 인스턴스
_gpio_controller = None

def get_gpio_controller():
    """GPIO 컨트롤러 싱글톤 인스턴스 반환"""
    global _gpio_controller
    if _gpio_controller is None:
        _gpio_controller = GPIOController()
    return _gpio_controller
```

### 5-3. 양면 동시 캡처 통합 클라이언트 ⭐ (이중 모델 아키텍처)

**dual_camera_client.py** (라즈베리파이 1 전용)

```python
import cv2
import requests
import base64
import time
import logging
from datetime import datetime
from gpio_controller import get_gpio_controller

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

class DualCameraClient:
    """양면 동시 웹캠 캡처 및 이중 모델 추론 클라이언트 (라즈베리파이 1 전용)"""

    def __init__(self, left_camera_index, right_camera_index, server_url, fps=10):
        """
        Args:
            left_camera_index: 좌측 카메라 인덱스 (보통 0)
            right_camera_index: 우측 카메라 인덱스 (보통 1)
            server_url: Flask 서버 URL
            fps: 프레임 전송 속도
        """
        self.left_camera_index = left_camera_index
        self.right_camera_index = right_camera_index
        self.server_url = server_url
        self.fps = fps
        self.frame_interval = 1.0 / fps

        # 좌측 웹캠 초기화 (부품 검출용)
        self.cap_left = cv2.VideoCapture(left_camera_index)
        if not self.cap_left.isOpened():
            raise RuntimeError(f"좌측 카메라 {left_camera_index} 열기 실패")

        # 우측 웹캠 초기화 (제품 식별용)
        self.cap_right = cv2.VideoCapture(right_camera_index)
        if not self.cap_right.isOpened():
            raise RuntimeError(f"우측 카메라 {right_camera_index} 열기 실패")

        # 해상도 설정
        for cap in [self.cap_left, self.cap_right]:
            cap.set(cv2.CAP_PROP_FRAME_WIDTH, 640)
            cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 480)
            cap.set(cv2.CAP_PROP_FPS, fps)

        # GPIO 컨트롤러 초기화
        self.gpio = get_gpio_controller()

        logger.info(f"양면 카메라 클라이언트 초기화 완료")
        logger.info(f"  - 좌측 카메라: {left_camera_index} (부품 검출)")
        logger.info(f"  - 우측 카메라: {right_camera_index} (제품 식별)")
        logger.info(f"  - Flask 서버: {server_url}")

    def encode_frame(self, frame):
        """프레임을 JPEG → Base64 인코딩"""
        _, buffer = cv2.imencode('.jpg', frame, [cv2.IMWRITE_JPEG_QUALITY, 85])
        return base64.b64encode(buffer).decode('utf-8')

    def send_dual_frames(self, left_frame, right_frame):
        """
        양면 프레임을 Flask 서버로 동시 전송 (이중 모델 추론 요청)

        Args:
            left_frame: 좌측 카메라 프레임 (부품면)
            right_frame: 우측 카메라 프레임 (뒷면)

        Returns:
            dict: Flask 서버 응답 (fusion_result, component_result, solder_result)
        """
        try:
            # 양면 프레임 인코딩
            left_base64 = self.encode_frame(left_frame)
            right_base64 = self.encode_frame(right_frame)

            # API 요청 데이터 (양면 동시 전송)
            data = {
                'left_frame': {
                    'image': left_base64,
                    'camera_id': 'left',
                    'timestamp': datetime.now().isoformat()
                },
                'right_frame': {
                    'image': right_base64,
                    'camera_id': 'right',
                    'timestamp': datetime.now().isoformat()
                }
            }

            # Flask 서버로 이중 모델 추론 요청
            response = requests.post(
                f"{self.server_url}/predict_dual",
                json=data,
                timeout=5
            )

            if response.status_code == 200:
                result = response.json()

                # 융합 결과 출력
                fusion_result = result.get('fusion_result', {})
                decision = fusion_result.get('decision', 'normal')
                component_count = len(result.get('component_result', {}).get('defects', []))
                solder_count = len(result.get('solder_result', {}).get('defects', []))

                logger.info(
                    f"[이중 모델 결과] 판정: {decision} "
                    f"(부품불량: {component_count}개, 위치 오류: {solder_count}개)"
                )

                # GPIO 신호 출력 (융합 결과 기반)
                self.gpio.trigger(decision, duration_ms=500)

                return result
            else:
                logger.error(f"서버 오류: {response.status_code}")
                return None

        except requests.exceptions.Timeout:
            logger.error("요청 타임아웃")
            return None
        except Exception as e:
            logger.error(f"양면 프레임 전송 오류: {str(e)}")
            return None

    def run(self):
        """메인 루프 (양면 동시 캡처 및 전송)"""
        logger.info("양면 카메라 클라이언트 시작")

        frame_count = 0
        last_send_time = time.time()

        try:
            while True:
                # 양면 프레임 동시 캡처
                ret_left, left_frame = self.cap_left.read()
                ret_right, right_frame = self.cap_right.read()

                if not ret_left or not ret_right:
                    logger.warning("프레임 읽기 실패 (좌측 또는 우측)")
                    continue

                frame_count += 1
                current_time = time.time()

                # FPS 제어
                if current_time - last_send_time >= self.frame_interval:
                    self.send_dual_frames(left_frame, right_frame)
                    last_send_time = current_time

                # 프레임 정보 출력 (100프레임마다)
                if frame_count % 100 == 0:
                    logger.info(f"전송 프레임 수: {frame_count}")

        except KeyboardInterrupt:
            logger.info("사용자에 의해 중단됨")

        finally:
            self.cap_left.release()
            self.cap_right.release()
            self.gpio.cleanup()
            logger.info("양면 카메라 클라이언트 종료")

if __name__ == '__main__':
    import sys
    import os

    # 설정 (라즈베리파이 1 전용)
    LEFT_CAMERA_INDEX = int(sys.argv[1]) if len(sys.argv) > 1 else 0   # 좌측 카메라 (부품)
    RIGHT_CAMERA_INDEX = int(sys.argv[2]) if len(sys.argv) > 2 else 1  # 우측 카메라 (납땜)
    SERVER_URL = sys.argv[3] if len(sys.argv) > 3 else os.getenv('FLASK_SERVER_URL', 'http://100.64.1.1:5000')
    FPS = int(sys.argv[4]) if len(sys.argv) > 4 else 10

    # 양면 동시 캡처 클라이언트 실행
    client = DualCameraClient(LEFT_CAMERA_INDEX, RIGHT_CAMERA_INDEX, SERVER_URL, FPS)
    client.run()
```

---

## Phase 6: 자동 시작 설정 ⭐ (이중 모델 아키텍처)

### 6-1. 시스템 구성 방식 선택

**⭐ 권장 방식: 양면 카메라 모두 라즈베리파이 1에 연결**
- 라즈베리파이 1: 좌/우 웹캠 2대 + GPIO 제어
- 라즈베리파이 2: 사용하지 않거나 OHT 전용으로 재활용
- 장점: 양면 프레임이 완벽히 동기화됨, 네트워크 지연 없음

**대안 방식: 라즈베리파이 2대 분산**
- 라즈베리파이 1: 좌측 카메라 + GPIO 제어
- 라즈베리파이 2: 우측 카메라 전용
- Flask 서버가 좌/우 프레임을 시간순으로 매칭
- 단점: 프레임 동기화 어려움, 네트워크 지연 발생 가능

**본 가이드는 권장 방식(양면 카메라 모두 RPi 1 연결)을 기준으로 작성되었습니다.**

### 6-2. systemd 서비스 생성 (라즈베리파이 1 - 양면 동시 캡처)

**dual-camera-client.service** (권장)

```bash
sudo nano /etc/systemd/system/dual-camera-client.service
```

내용:
```ini
[Unit]
Description=PCB Sequential Camera Client - Backside ID + Front Verification
After=network.target

[Service]
Type=simple
User=pi
WorkingDirectory=/home/pi/pcb_inspection_client
Environment="FLASK_SERVER_URL=http://100.64.1.1:5000"
ExecStart=/usr/bin/python3 /home/pi/pcb_inspection_client/dual_camera_client.py 0 1 $FLASK_SERVER_URL 10
Restart=always
RestartSec=10

[Install]
WantedBy=multi-user.target
```

**파라미터 설명**:
- `0`: 좌측 카메라 인덱스 (부품 검출용, /dev/video0)
- `1`: 우측 카메라 인덱스 (제품 식별용, /dev/video1)
- `$FLASK_SERVER_URL`: Flask 서버 URL (Tailscale VPN: 100.64.1.1:5000)
- `10`: FPS (초당 10프레임 전송)

### 6-3. 서비스 활성화 (라즈베리파이 1 - 양면 동시 캡처)

```bash
# 서비스 리로드
sudo systemctl daemon-reload

# 서비스 활성화 (부팅 시 자동 시작)
sudo systemctl enable dual-camera-client.service

# 서비스 시작
sudo systemctl start dual-camera-client.service

# 서비스 상태 확인
sudo systemctl status dual-camera-client.service

# 로그 확인 (실시간)
sudo journalctl -u dual-camera-client.service -f

# 로그 확인 (최근 100줄)
sudo journalctl -u dual-camera-client.service -n 100
```

**예상 로그 출력**:
```
양면 카메라 클라이언트 초기화 완료
  - 좌측 카메라: 0 (부품 검출)
  - 우측 카메라: 1 (제품 식별)
  - Flask 서버: http://100.64.1.1:5000
양면 카메라 클라이언트 시작
[이중 모델 결과] 판정: missing (부품 누락: 2개, 위치 오류: 0개)
GPIO 신호 출력: missing (핀 17, 500ms)
전송 프레임 수: 100
```

### 6-4. 수동 실행 (테스트용)

```bash
# 양면 동시 캡처 클라이언트 수동 실행
cd ~/pcb_inspection_client
python3 dual_camera_client.py 0 1 http://100.64.1.1:5000 10

# 파라미터:
# - 0: 좌측 카메라 (/dev/video0)
# - 1: 우측 카메라 (/dev/video1)
# - http://100.64.1.1:5000: Flask 서버 URL
# - 10: FPS
```

---

## Phase 7: 네트워크 설정

### 7-1. 고정 IP 설정

#### 라즈베리파이 1 (좌측 카메라 + GPIO) - 로컬 고정 IP 예시: 192.168.0.20 (Tailscale 사용 시 생략)

```bash
sudo nano /etc/dhcpcd.conf
```

맨 아래 추가:
```
# 고정 IP 설정 (라즈베리파이 1 - 좌측 카메라 + GPIO)
interface eth0
static ip_address=192.168.0.20/24
static routers=192.168.0.1
static domain_name_servers=8.8.8.8 8.8.4.4

# 유선 연결 필수 (Wi-Fi 사용 시 지연 발생 가능)
```

재부팅:
```bash
sudo reboot
```

---

#### 라즈베리파이 2 (우측 카메라 전용) - 로컬 고정 IP 예시: 192.168.0.21 (Tailscale 사용 시 생략)

```bash
sudo nano /etc/dhcpcd.conf
```

맨 아래 추가:
```
# 고정 IP 설정 (라즈베리파이 2 - 우측 카메라 전용)
interface eth0
static ip_address=192.168.0.21/24
static routers=192.168.0.1
static domain_name_servers=8.8.8.8 8.8.4.4

# 유선 연결 필수 (Wi-Fi 사용 시 지연 발생 가능)
```

재부팅:
```bash
sudo reboot
```

---

## 트러블슈팅

### 문제 1: 웹캠 인식 안 됨

```bash
# USB 장치 확인
lsusb

# 비디오 장치 확인
ls -l /dev/video*

# 권한 확인
sudo usermod -a -G video pi
```

### 문제 2: GPIO 권한 오류

```bash
# GPIO 그룹 추가
sudo usermod -a -G gpio pi

# 재로그인
exit
ssh pi@raspberrypi-left.local
```

### 문제 3: 메모리 부족

```bash
# 스왑 메모리 증가
sudo dphys-swapfile swapoff
sudo nano /etc/dphys-swapfile
# CONF_SWAPSIZE=2048 (1024 → 2048)

sudo dphys-swapfile setup
sudo dphys-swapfile swapon
```

---

## 성능 최적화

### CPU 오버클럭 (선택)

```bash
sudo nano /boot/config.txt
```

추가:
```
over_voltage=2
arm_freq=1750
```

**주의**: 발열 증가, 방열판 필수

---

## 원격 Flask 서버 연결 (Tailscale VPN)

### GPU PC가 원격지에 있을 경우

**프로젝트 환경**: GPU PC가 다른 위치 (같은 도시 내)에 있을 때 Tailscale VPN 사용

### Tailscale 설치 (라즈베리파이)

```bash
# Tailscale 설치
curl -fsSL https://tailscale.com/install.sh | sh

# Tailscale 시작 (GPU PC와 동일한 계정으로 로그인)
sudo tailscale up

# Tailscale IP 확인
tailscale ip -4
# 출력 예시: 100.64.1.2 (라즈베리파이 1)
#          100.64.1.3 (라즈베리파이 2)
```

### dual_camera_client.py 설정 수정 (이중 모델 아키텍처)

```python
# Tailscale IP로 서버 URL 설정
SERVER_URL = 'http://100.64.1.1:5000'  # GPU PC의 Tailscale IP

# 또는 환경 변수로 관리
import os
SERVER_URL = os.getenv('FLASK_SERVER_URL', 'http://100.64.1.1:5000')

# 양면 동시 캡처 클라이언트 실행
client = DualCameraClient(0, 1, SERVER_URL, 10)
client.run()
```

### 환경 변수 설정 (권장)

```bash
# ~/.bashrc에 추가
echo 'export FLASK_SERVER_URL="http://100.64.1.1:5000"' >> ~/.bashrc
source ~/.bashrc
```

### 클라이언트 실행 및 테스트 (이중 모델 아키텍처)

```bash
cd ~/pcb_project/raspberry_pi
python3 dual_camera_client.py 0 1 http://100.64.1.1:5000 10

# 출력에서 네트워크 지연 및 융합 결과 확인:
# 양면 카메라 클라이언트 초기화 완료
#   - 좌측 카메라: 0 (부품 검출)
#   - 우측 카메라: 1 (제품 식별)
# [이중 모델 결과] 판정: position_error (부품 누락: 0개, 위치 오류: 3개)
# GPIO 신호 출력: position_error (핀 27, 500ms)
# Total latency: 125ms  ← 전체 처리 시간 (목표 300ms 이내) ✅
```

### 네트워크 연결 테스트

```bash
# GPU PC Ping 테스트
ping -c 4 100.64.1.1

# 정상 출력:
# 64 bytes from 100.64.1.1: icmp_seq=1 ttl=64 time=25.3 ms

# Flask 서버 Health Check
curl http://100.64.1.1:5000/health
# {"status":"ok","timestamp":"2025-10-23T10:30:00"}
```

### systemd 서비스 파일 수정 (Tailscale IP 사용 - 이중 모델)

```bash
sudo nano /etc/systemd/system/dual-camera-client.service
```

```ini
[Unit]
Description=PCB Sequential Camera Client - Backside ID + Front Verification (Tailscale)
After=network.target tailscaled.service
Wants=tailscaled.service

[Service]
Type=simple
User=pi
WorkingDirectory=/home/pi/pcb_project/raspberry_pi
Environment="FLASK_SERVER_URL=http://100.64.1.1:5000"
ExecStart=/usr/bin/python3 dual_camera_client.py 0 1 http://100.64.1.1:5000 10
Restart=always
RestartSec=10

[Install]
WantedBy=multi-user.target
```

**파라미터 설명**:
- `0`: 좌측 카메라 (부품 검출, /dev/video0)
- `1`: 우측 카메라 (제품 식별, /dev/video1)
- `http://100.64.1.1:5000`: Flask 서버 Tailscale IP
- `10`: FPS

```bash
sudo systemctl daemon-reload
sudo systemctl restart dual-camera-client.service
sudo systemctl status dual-camera-client.service
```

### 성능 확인

네트워크 지연 측정 및 성능 최적화에 대한 **상세 가이드는 `docs/Remote_Network_Setup.md` 참조**하세요.

간단한 연결 테스트:
```bash
# Flask 서버 Health Check
curl http://100.64.1.1:5000/health
# 정상 출력: {"status":"ok","timestamp":"2025-10-23T10:30:00"}
```

---

## Phase 6: USB 시리얼 통신 (Arduino 로봇팔 제어) ⭐ 신규 - 라즈베리파이 1 전용

### 6-1. pyserial 라이브러리 설치

```bash
# pyserial 설치
pip3 install pyserial

# 설치 확인
python3 -c "import serial; print(serial.__version__)"
```

### 6-2. USB 포트 확인

```bash
# 연결된 USB 장치 확인
ls /dev/ttyUSB* /dev/ttyACM*

# Arduino Mega는 보통 /dev/ttyACM0 또는 /dev/ttyUSB0로 인식
# 장치 정보 확인
dmesg | grep tty
```

### 6-3. 시리얼 권한 설정

```bash
# 사용자를 dialout 그룹에 추가
sudo usermod -a -G dialout $USER

# 재부팅 필요
sudo reboot
```

### 6-4. Arduino 시리얼 컨트롤러 모듈

**serial_controller.py**

```python
import serial
import json
import time
import logging

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

class ArduinoSerialController:
    """Arduino Mega와 USB 시리얼 통신 클래스"""

    def __init__(self, port='/dev/ttyACM0', baudrate=115200, timeout=5):
        """
        Arduino 시리얼 포트 초기화

        Args:
            port: 시리얼 포트 경로 (기본: /dev/ttyACM0)
            baudrate: 보드레이트 (기본: 115200)
            timeout: 읽기 타임아웃 (초)
        """
        self.port = port
        self.baudrate = baudrate
        self.timeout = timeout
        self.serial_connection = None

        try:
            self.serial_connection = serial.Serial(
                port=self.port,
                baudrate=self.baudrate,
                timeout=self.timeout
            )
            time.sleep(2)  # Arduino 리셋 대기
            logger.info(f"Arduino 연결 성공: {self.port} at {self.baudrate} baud")
        except serial.SerialException as e:
            logger.error(f"Arduino 연결 실패: {str(e)}")
            raise

    def send_command(self, command_dict):
        """
        Arduino에 JSON 명령 전송

        Args:
            command_dict: 명령 딕셔너리
                {
                    "command": "place_pcb",
                    "box_id": "NORMAL_A",
                    "slot_number": 2,
                    "coordinates": {"x": 120.5, "y": 85.3, "z": 30.0}
                }

        Returns:
            dict: Arduino 응답
                {
                    "status": "success",
                    "message": "PCB placed successfully",
                    "execution_time_ms": 2350
                }
        """
        try:
            # JSON 문자열로 변환 후 전송
            json_str = json.dumps(command_dict) + '\n'
            self.serial_connection.write(json_str.encode('utf-8'))
            logger.info(f"Arduino 명령 전송: {command_dict}")

            # 응답 대기 (최대 timeout 초)
            response_line = self.serial_connection.readline().decode('utf-8').strip()

            if response_line:
                response = json.loads(response_line)
                logger.info(f"Arduino 응답: {response}")
                return response
            else:
                logger.warning("Arduino 응답 없음 (timeout)")
                return {"status": "error", "message": "No response from Arduino"}

        except json.JSONDecodeError as e:
            logger.error(f"JSON 디코딩 오류: {str(e)}")
            return {"status": "error", "message": f"JSON decode error: {str(e)}"}
        except Exception as e:
            logger.error(f"시리얼 통신 오류: {str(e)}")
            return {"status": "error", "message": str(e)}

    def is_connected(self):
        """Arduino 연결 상태 확인"""
        return self.serial_connection and self.serial_connection.is_open

    def close(self):
        """시리얼 연결 종료"""
        if self.serial_connection and self.serial_connection.is_open:
            self.serial_connection.close()
            logger.info("Arduino 연결 종료")

# 전역 Arduino 컨트롤러 인스턴스
_arduino_controller = None

def get_arduino_controller(port='/dev/ttyACM0', baudrate=115200):
    """Arduino 컨트롤러 싱글톤 인스턴스 반환"""
    global _arduino_controller
    if _arduino_controller is None or not _arduino_controller.is_connected():
        _arduino_controller = ArduinoSerialController(port, baudrate)
    return _arduino_controller
```

### 6-5. 통합 클라이언트 업데이트 (camera_client.py에 추가)

기존 `camera_client.py`에 Arduino 제어를 통합:

```python
# camera_client.py 상단에 import 추가
from serial_controller import get_arduino_controller

# 메인 루프에서 Flask 응답 처리 부분에 추가
def process_flask_response(response_json):
    """Flask 서버 응답 처리"""

    # GPIO 제어 (기존 코드)
    if 'gpio_signal' in response_json:
        gpio_controller = get_gpio_controller()
        gpio_controller.trigger(
            response_json['defect_type'],
            duration_ms=response_json['gpio_signal'].get('duration_ms', 500)
        )

    # 로봇팔 제어 (신규) ⭐
    if 'robot_arm_command' in response_json:
        arduino_controller = get_arduino_controller()
        robot_command = response_json['robot_arm_command']

        # Arduino에 명령 전송
        arduino_response = arduino_controller.send_command(robot_command)

        if arduino_response.get('status') == 'success':
            logger.info(f"로봇팔 동작 완료: {arduino_response.get('execution_time_ms')}ms")
        else:
            logger.error(f"로봇팔 동작 실패: {arduino_response.get('message')}")
```

### 6-6. 시리얼 통신 테스트

**test_serial.py**

```python
import time
from serial_controller import get_arduino_controller

def test_arduino_communication():
    """Arduino 통신 테스트"""
    try:
        arduino = get_arduino_controller('/dev/ttyACM0', 115200)

        # 테스트 명령 전송
        test_command = {
            "command": "place_pcb",
            "box_id": "NORMAL_A",
            "slot_number": 0,
            "coordinates": {"x": 100.0, "y": 80.0, "z": 30.0}
        }

        print(f"테스트 명령 전송: {test_command}")
        response = arduino.send_command(test_command)

        print(f"Arduino 응답: {response}")

        if response.get('status') == 'success':
            print("✅ 통신 성공!")
        else:
            print("❌ 통신 실패:", response.get('message'))

        arduino.close()

    except Exception as e:
        print(f"❌ 오류 발생: {str(e)}")

if __name__ == '__main__':
    test_arduino_communication()
```

### 6-7. 실행

```bash
# 테스트 실행
python3 test_serial.py

# 정상 출력 예시:
# 테스트 명령 전송: {'command': 'place_pcb', 'box_id': 'NORMAL_A', 'slot_number': 0, ...}
# Arduino 응답: {'status': 'success', 'message': 'PCB placed successfully', 'execution_time_ms': 2350}
# ✅ 통신 성공!
```

### 6-8. 프로젝트 구조 업데이트

```
~/pcb_inspection_client/
├── camera_client.py       # 웹캠 + GPIO + 로봇팔 통합 클라이언트 ⭐ 업데이트
├── gpio_controller.py     # GPIO 제어 모듈
├── serial_controller.py   # Arduino 시리얼 통신 모듈 ⭐ 신규
├── config.py              # 설정 파일
├── test_serial.py         # 시리얼 통신 테스트 ⭐ 신규
└── start.sh               # 자동 시작 스크립트
```

### 6-9. 주의사항

1. **Arduino 포트 자동 인식**:
   - Arduino Mega는 `/dev/ttyACM0` 또는 `/dev/ttyUSB0`로 인식
   - 연결 순서에 따라 포트 번호가 변경될 수 있음
   - `dmesg | grep tty`로 정확한 포트 확인

2. **시리얼 권한**:
   - `dialout` 그룹에 사용자 추가 필수
   - 추가 후 재부팅 필요

3. **타임아웃 설정**:
   - 로봇팔 동작 시간(2-3초)을 고려하여 timeout 5초 설정
   - Arduino 응답이 없으면 timeout 후 error 반환

4. **에러 처리**:
   - Arduino 연결 끊김 시 재연결 로직 필요
   - 명령 전송 실패 시 재시도 로직 구현 권장

---

## Phase 7: 라즈베리파이 3번 - OHT 시스템 제어기 설정 ⭐ 신규

### 7-1. 개요

라즈베리파이 3번(Raspberry Pi 4 Model B 하드웨어)은 OHT (Overhead Hoist Transport) 시스템 전용 제어기로 사용됩니다.

**주요 기능**:
- X축 스텝모터 제어 (천장 레일 이동)
- Z축 좌/우 스텝모터 동기 제어 (베드 상하 이동)
- 서보모터 걸쇠 제어 (박스 잠금/해제)
- 리미트 스위치 6개 상태 모니터링 (X축 2, Z축 4)
- Flask 서버와 HTTP 통신 (OHT 요청 폴링)
- 긴급 정지 버튼 처리

### 7-2. 필수 패키지 설치

```bash
# 기본 패키지 (Phase 2와 동일)
sudo apt update
sudo apt upgrade -y
sudo apt install -y python3-pip python3-dev pigpio python3-pigpio

# pigpiod 데몬 활성화 (이미 설정되어 있지 않은 경우)
sudo systemctl enable pigpiod
sudo systemctl start pigpiod

# OHT 전용 Python 패키지
pip3 install pigpio requests
```

### 7-3. GPIO 핀맵 (BCM 모드)

**OHT 모터 및 센서 핀맵**

```python
# oht_controller_config.py

# X축 스텝모터 (A4988 드라이버)
STEP_PIN_X = 18        # 스텝 신호
DIR_PIN_X = 23         # 방향 신호
ENABLE_PIN_X = 24      # 활성화 신호

# Z축 좌측 스텝모터 (A4988 드라이버)
STEP_PIN_Z_LEFT = 17
DIR_PIN_Z_LEFT = 27
ENABLE_PIN_Z_LEFT = 22

# Z축 우측 스텝모터 (A4988 드라이버)
STEP_PIN_Z_RIGHT = 25
DIR_PIN_Z_RIGHT = 8
ENABLE_PIN_Z_RIGHT = 7

# 베드 걸쇠 서보모터
SERVO_PIN_LATCH = 12   # PWM 제어 (pigpio)

# 리미트 스위치 (X축)
LIMIT_SW_WAREHOUSE = 5      # 창고 위치 (홈 포지션)
LIMIT_SW_END = 6            # 박스3 끝 (안전 한계)

# 리미트 스위치 (Z축 - 양쪽 4개)
LIMIT_SW_Z_LEFT_UP = 16
LIMIT_SW_Z_LEFT_DOWN = 20
LIMIT_SW_Z_RIGHT_UP = 21
LIMIT_SW_Z_RIGHT_DOWN = 19

# 긴급 정지 버튼
EMERGENCY_STOP_PIN = 26
```

### 7-4. OHT 컨트롤러 설치

**프로젝트 폴더 생성**

```bash
mkdir -p ~/oht_controller
cd ~/oht_controller
```

**파일 복사** (OHT_System_Setup.md 참조)

```bash
# OHT 제어 스크립트를 작성하거나 복사
# 상세 코드는 docs/OHT_System_Setup.md 참조
```

**핵심 파일**:
- `oht_controller.py` - 메인 컨트롤러
- `oht_motor_control.py` - 모터 제어 클래스
- `oht_controller_config.py` - GPIO 핀 설정

### 7-5. systemd 서비스 등록

```bash
sudo nano /etc/systemd/system/oht-controller.service
```

```ini
[Unit]
Description=OHT Controller Service
After=network.target

[Service]
Type=simple
User=pi
WorkingDirectory=/home/pi/oht_controller
Environment="FLASK_SERVER_URL=http://100.64.1.1:5000"
ExecStart=/usr/bin/python3 /home/pi/oht_controller/oht_controller.py
Restart=always
RestartSec=10

[Install]
WantedBy=multi-user.target
```

### 7-6. 서비스 활성화

```bash
sudo systemctl daemon-reload
sudo systemctl enable oht-controller.service
sudo systemctl start oht-controller.service
sudo systemctl status oht-controller.service
```

### 7-7. 로그 확인

```bash
# 실시간 로그 확인
sudo journalctl -u oht-controller.service -f

# 최근 100줄
sudo journalctl -u oht-controller.service -n 100
```

### 7-8. IP 주소 설정 (로컬 LAN 사용 시 선택)

```bash
sudo nano /etc/dhcpcd.conf
```

추가:
```ini
# 라즈베리파이 3번 (OHT 전용) - 로컬 고정 IP 예시
interface eth0
static ip_address=192.168.0.22/24
static routers=192.168.0.1
static domain_name_servers=8.8.8.8 8.8.4.4
```

재부팅:
```bash
sudo reboot
```

### 7-9. 테스트

**수동 테스트**

```bash
cd ~/oht_controller
python3 oht_controller.py
```

**Flask API 수동 호출 테스트**

```bash
# 정상 PCB OHT 호출
curl -X POST http://100.64.1.1:5000/api/oht/request \
  -H "Content-Type: application/json" \
  -d '{"category":"NORMAL","user_id":"test","user_role":"Admin"}'

# OHT 상태 확인
curl http://100.64.1.1:5000/api/oht/status
```

### 7-10. 문제 해결

**문제 1: GPIO 권한 오류**
```bash
# gpio 그룹 추가
sudo usermod -a -G gpio $USER
sudo reboot
```

**문제 2: 스텝모터가 움직이지 않음**
```bash
# ENABLE_PIN 상태 확인 (LOW = 활성화)
# 드라이버 전원 확인 (12V 2A)
```

**문제 3: 서보모터 떨림**
```bash
# PWM duty cycle을 0으로 설정 후 대기
# 별도 전원 공급 사용
# 캐패시터 추가 (1000µF)
```

**문제 4: Flask API 타임아웃**
```bash
# 네트워크 연결 확인
ping 100.64.1.1

# 방화벽 포트 5000 오픈
sudo ufw allow 5000/tcp
```

### 7-11. 상세 가이드

OHT 시스템의 상세한 하드웨어 사양, 제어 로직, API 설계는 다음 문서를 참조하세요:
- **docs/OHT_System_Setup.md** ⭐

---

## 다음 단계

1. **OHT 시스템 설정**: `OHT_System_Setup.md` ⭐ 신규
2. **Arduino 로봇팔 설정**: `Arduino_RobotArm_Setup.md` ⭐ 신규
3. **원격 네트워크 설정**: `Remote_Network_Setup.md`
4. **MySQL 데이터베이스 설계**: `MySQL_Database_Design.md`
5. **Flask 서버 업데이트**: `Flask_Server_Setup.md`
6. **C# WinForms 연동**: `CSharp_WinForms_Guide.md`

---

**작성일**: 2025-10-28
**최종 수정일**: 2025-11-30
**버전**: 3.0 ⭐ (제품별 검증 아키텍처)
**하드웨어**: Raspberry Pi 4 Model B
**OS**: Raspberry Pi OS 64-bit (Bullseye/Bookworm)
**주요 변경사항**:
- **3.0 (2025-11-30)**: 뒷면 Backscan + 앞면 Frontscan 순차 구조로 전환
  - 우측 카메라: 시리얼 넘버 OCR + QR 코드 스캔 후 제품 코드/inspection_token 발급
  - 좌측 카메라: YOLOv11l 부품 검출 + ComponentVerifier로 missing/position_error 계산
  - GPIO 제어 기준을 normal/missing/position_error/discard 로 통일
  - Backscan/Frontscan API와 inspection_token 전달 절차 문서화
- **2.0 (2025-10-31)**: 양면 동시 캡처 테스트 (아카이브)
- **1.1 (2025-10-23)**: Tailscale VPN 원격 연결 섹션 추가
