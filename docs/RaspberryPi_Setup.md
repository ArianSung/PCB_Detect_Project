# 라즈베리파이 4 PCB 검사 시스템 설정 가이드

## 개요

이 가이드는 라즈베리파이 4를 사용하여 웹캠 프레임 캡처 및 GPIO 제어를 수행하는 방법을 설명합니다.

**중요**: 이 시스템에서는 **라즈베리파이 1 (IP: 192.168.0.20)만** GPIO 제어를 수행합니다.
- **라즈베리파이 1**: 좌측 웹캠 + GPIO 출력 (분류 게이트, LED 제어)
- **라즈베리파이 2 (IP: 192.168.0.21)**: 우측 웹캠 전용 (GPIO 제어 없음)

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
- Python 3.9+

### 주요 라이브러리
- OpenCV
- RPi.GPIO
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
   - 호스트명: `raspberrypi-left` (좌측 카메라) 또는 `raspberrypi-right` (우측 카메라)
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

# 또는 IP 주소로 접속
ssh pi@192.168.0.20
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
# 출력: Python 3.9.x 이상
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

## Phase 4: GPIO 설정 및 릴레이 제어 ⭐ 라즈베리파이 1 전용

**중요**: GPIO 제어는 **라즈베리파이 1 (192.168.0.20)에만** 적용됩니다.
- Flask 서버가 양면(좌측+우측) 검사 결과를 통합 판정
- 최종 불량 분류 결과를 라즈베리파이 1에만 전송
- 라즈베리파이 2 (192.168.0.21)는 카메라 전용 (GPIO 사용 안 함)

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
- GPIO 27 (BCM 13) → 납땜 불량 (릴레이 채널 2)
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
PIN_SOLDER_DEFECT = 27     # 납땜 불량
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

        print("\n2. 납땜 불량 신호 (GPIO 27)")
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

## Phase 5: Flask Client 및 GPIO 통합

### 5-1. 프로젝트 구조

```
~/pcb_inspection_client/
├── camera_client.py       # 웹캠 + GPIO 통합 클라이언트
├── gpio_controller.py     # GPIO 제어 모듈
├── config.py              # 설정 파일
└── start.sh               # 자동 시작 스크립트
```

### 5-2. GPIO 제어 모듈

**gpio_controller.py**

```python
import RPi.GPIO as GPIO
import time
import logging

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

class GPIOController:
    """GPIO 릴레이 제어 클래스"""

    # GPIO 핀 매핑
    PIN_MAP = {
        '부품불량': 17,
        '납땜불량': 27,
        '폐기': 22,
        '정상': 23
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
        불량 유형에 따라 GPIO 신호 출력

        Args:
            defect_type: '정상', '부품불량', '납땜불량', '폐기'
            duration_ms: 신호 지속 시간 (밀리초)
        """
        if defect_type not in self.PIN_MAP:
            logger.warning(f"알 수 없는 불량 유형: {defect_type}")
            return

        pin = self.PIN_MAP[defect_type]
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

### 5-3. 통합 카메라 클라이언트

**camera_client.py**

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

class CameraClient:
    """웹캠 프레임 전송 및 GPIO 제어 통합 클라이언트"""

    def __init__(self, camera_id, camera_index, server_url, fps=10):
        self.camera_id = camera_id
        self.camera_index = camera_index
        self.server_url = server_url
        self.fps = fps
        self.frame_interval = 1.0 / fps

        # 웹캠 초기화
        self.cap = cv2.VideoCapture(camera_index)
        if not self.cap.isOpened():
            raise RuntimeError(f"카메라 {camera_index} 열기 실패")

        self.cap.set(cv2.CAP_PROP_FRAME_WIDTH, 640)
        self.cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 480)
        self.cap.set(cv2.CAP_PROP_FPS, fps)

        # GPIO 컨트롤러 초기화
        self.gpio = get_gpio_controller()

        logger.info(f"카메라 클라이언트 초기화: {camera_id} (인덱스 {camera_index})")

    def encode_frame(self, frame):
        """프레임을 JPEG → Base64 인코딩"""
        _, buffer = cv2.imencode('.jpg', frame, [cv2.IMWRITE_JPEG_QUALITY, 85])
        return base64.b64encode(buffer).decode('utf-8')

    def send_frame(self, frame):
        """프레임을 Flask 서버로 전송하고 GPIO 제어"""
        try:
            frame_base64 = self.encode_frame(frame)

            data = {
                'camera_id': self.camera_id,
                'frame': frame_base64,
                'timestamp': datetime.now().isoformat()
            }

            response = requests.post(
                f"{self.server_url}/predict",
                json=data,
                timeout=5
            )

            if response.status_code == 200:
                result = response.json()
                defect_type = result.get('defect_type', '정상')
                confidence = result.get('confidence', 0.0)

                logger.info(f"[{self.camera_id}] 결과: {defect_type} (신뢰도: {confidence:.2f})")

                # GPIO 신호 출력
                gpio_signal = result.get('gpio_signal', {})
                duration_ms = gpio_signal.get('duration_ms', 500)
                self.gpio.trigger(defect_type, duration_ms)

                return result
            else:
                logger.error(f"서버 오류: {response.status_code}")
                return None

        except requests.exceptions.Timeout:
            logger.error("요청 타임아웃")
            return None
        except Exception as e:
            logger.error(f"프레임 전송 오류: {str(e)}")
            return None

    def run(self):
        """메인 루프"""
        logger.info(f"카메라 클라이언트 시작: {self.camera_id}")

        frame_count = 0
        last_send_time = time.time()

        try:
            while True:
                ret, frame = self.cap.read()
                if not ret:
                    logger.warning("프레임 읽기 실패")
                    continue

                frame_count += 1
                current_time = time.time()

                # FPS 제어
                if current_time - last_send_time >= self.frame_interval:
                    self.send_frame(frame)
                    last_send_time = current_time

                # 프레임 정보 출력 (100프레임마다)
                if frame_count % 100 == 0:
                    logger.info(f"전송 프레임 수: {frame_count}")

        except KeyboardInterrupt:
            logger.info("사용자에 의해 중단됨")

        finally:
            self.cap.release()
            self.gpio.cleanup()
            logger.info("카메라 클라이언트 종료")

if __name__ == '__main__':
    import sys
    import os

    # 설정
    CAMERA_ID = sys.argv[1] if len(sys.argv) > 1 else 'left'
    CAMERA_INDEX = int(sys.argv[2]) if len(sys.argv) > 2 else 0
    # 환경 변수 우선, 인자 전달 다음, 기본값으로 Tailscale IP 사용
    SERVER_URL = sys.argv[3] if len(sys.argv) > 3 else os.getenv('FLASK_SERVER_URL', 'http://100.64.1.1:5000')
    FPS = int(sys.argv[4]) if len(sys.argv) > 4 else 10

    # 클라이언트 실행
    client = CameraClient(CAMERA_ID, CAMERA_INDEX, SERVER_URL, FPS)
    client.run()
```

---

## Phase 6: 자동 시작 설정

### 6-1. systemd 서비스 생성 (2가지 버전)

#### 버전 1: 라즈베리파이 1 (좌측 카메라 + GPIO 제어) - IP: 192.168.0.20

**camera-client-left.service**

```bash
sudo nano /etc/systemd/system/camera-client-left.service
```

내용:
```ini
[Unit]
Description=PCB Inspection Camera Client - Left (with GPIO)
After=network.target

[Service]
Type=simple
User=pi
WorkingDirectory=/home/pi/pcb_inspection_client
Environment="FLASK_SERVER_URL=http://100.64.1.1:5000"
ExecStart=/usr/bin/python3 /home/pi/pcb_inspection_client/camera_client.py left 0 $FLASK_SERVER_URL 10
Restart=always
RestartSec=10
Environment="ENABLE_GPIO=1"

[Install]
WantedBy=multi-user.target
```

**참고**: 이 버전은 GPIO 제어 기능이 포함되어 있으며, Flask 서버로부터 GPIO 제어 신호를 수신합니다.

---

#### 버전 2: 라즈베리파이 2 (우측 카메라 전용) - IP: 192.168.0.21

**camera-client-right.service**

```bash
sudo nano /etc/systemd/system/camera-client-right.service
```

내용:
```ini
[Unit]
Description=PCB Inspection Camera Client - Right (Camera Only)
After=network.target

[Service]
Type=simple
User=pi
WorkingDirectory=/home/pi/pcb_inspection_client
Environment="FLASK_SERVER_URL=http://100.64.1.1:5000"
ExecStart=/usr/bin/python3 /home/pi/pcb_inspection_client/camera_client.py right 0 $FLASK_SERVER_URL 10
Restart=always
RestartSec=10
Environment="ENABLE_GPIO=0"

[Install]
WantedBy=multi-user.target
```

**참고**: 이 버전은 카메라 전용이며, GPIO 제어 기능이 비활성화되어 있습니다.

### 6-2. 서비스 활성화

#### 라즈베리파이 1 (좌측 + GPIO)

```bash
# 서비스 리로드
sudo systemctl daemon-reload

# 서비스 활성화 (부팅 시 자동 시작)
sudo systemctl enable camera-client-left.service

# 서비스 시작
sudo systemctl start camera-client-left.service

# 서비스 상태 확인
sudo systemctl status camera-client-left.service

# 로그 확인
sudo journalctl -u camera-client-left.service -f
```

#### 라즈베리파이 2 (우측 전용)

```bash
# 서비스 리로드
sudo systemctl daemon-reload

# 서비스 활성화 (부팅 시 자동 시작)
sudo systemctl enable camera-client-right.service

# 서비스 시작
sudo systemctl start camera-client-right.service

# 서비스 상태 확인
sudo systemctl status camera-client-right.service

# 로그 확인
sudo journalctl -u camera-client-right.service -f
```

---

## Phase 7: 네트워크 설정

### 7-1. 고정 IP 설정

#### 라즈베리파이 1 (좌측 카메라 + GPIO) - IP: 192.168.0.20

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

#### 라즈베리파이 2 (우측 카메라 전용) - IP: 192.168.0.21

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

### camera_client.py 설정 수정

```python
# Tailscale IP로 서버 URL 설정
SERVER_URL = 'http://100.64.1.1:5000'  # GPU PC의 Tailscale IP

# 또는 환경 변수로 관리
import os
SERVER_URL = os.getenv('FLASK_SERVER_URL', 'http://100.64.1.1:5000')
```

### 환경 변수 설정 (권장)

```bash
# ~/.bashrc에 추가
echo 'export FLASK_SERVER_URL="http://100.64.1.1:5000"' >> ~/.bashrc
source ~/.bashrc
```

### 클라이언트 실행 및 테스트

```bash
cd ~/pcb_project/raspberry_pi
python3 camera_client.py left 0 http://100.64.1.1:5000 10

# 출력에서 네트워크 지연 확인:
# [left] Result: 정상 (confidence: 0.95, inference: 18.5ms)
# Total latency: 125ms  ← 전체 처리 시간 (목표 300ms 이내)
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

### systemd 서비스 파일 수정 (Tailscale IP 사용)

```bash
sudo nano /etc/systemd/system/camera-client-left.service
```

```ini
[Unit]
Description=PCB Camera Client (Left) - Tailscale
After=network.target tailscaled.service
Wants=tailscaled.service

[Service]
Type=simple
User=pi
WorkingDirectory=/home/pi/pcb_project/raspberry_pi
Environment="FLASK_SERVER_URL=http://100.64.1.1:5000"
ExecStart=/usr/bin/python3 camera_client.py left 0 http://100.64.1.1:5000 10
Restart=always
RestartSec=10

[Install]
WantedBy=multi-user.target
```

```bash
sudo systemctl daemon-reload
sudo systemctl restart camera-client-left.service
sudo systemctl status camera-client-left.service
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

## 다음 단계

1. **원격 네트워크 설정**: `Remote_Network_Setup.md` ⭐
2. **MySQL 데이터베이스 설계**: `MySQL_Database_Design.md`
3. **Flask 서버 업데이트**: `Flask_Server_Setup.md`
4. **C# WinForms 연동**: `CSharp_WinForms_Guide.md`

---

**작성일**: 2025-10-22
**최종 수정일**: 2025-10-23
**버전**: 1.1
**하드웨어**: Raspberry Pi 4 Model B
**OS**: Raspberry Pi OS 64-bit (Bullseye/Bookworm)
**주요 변경사항**: Tailscale VPN 원격 연결 섹션 추가
