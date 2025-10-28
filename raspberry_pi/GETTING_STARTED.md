# 라즈베리파이 팀 시작 가이드

> PCB 불량 검사 시스템 라즈베리파이 웹캠 클라이언트 개발을 시작하는 팀원을 위한 빠른 시작 가이드입니다.

---

## 🎯 라즈베리파이 팀의 역할

- **웹캠 프레임 캡처**: OpenCV로 PCB 이미지 실시간 캡처
- **이미지 인코딩**: JPEG 인코딩 및 Base64 변환
- **Flask API 호출**: HTTP POST로 추론 서버에 프레임 전송
- **GPIO 제어**: 릴레이 모듈로 불량 유형별 자동 분류 (라즈베리파이 1만)

---

## 📚 반드시 읽어야 할 문서

### 필수 문서 (우선순위 순)

1. **[RaspberryPi_Setup.md](../docs/RaspberryPi_Setup.md)** ⭐ 가장 중요!
   - 라즈베리파이 환경 설정 및 클라이언트 가이드

2. **[API_Contract.md](../docs/API_Contract.md)** ⭐ Flask API 명세!
   - Flask API 요청/응답 형식 (팀 전체 계약)

3. **[raspberry_pi/.env.example](.env.example)**
   - 환경 변수 템플릿

4. **[tests/api/mock_server.py](../tests/api/mock_server.py)**
   - Mock Flask 서버 (Flask 없이 독립 개발 가능)

### 참고 문서

- [Team_Collaboration_Guide.md](../docs/Team_Collaboration_Guide.md) - 팀 협업 규칙
- [Git_Workflow.md](../docs/Git_Workflow.md) - Git 브랜치 전략
- [Development_Setup.md](../docs/Development_Setup.md) - 로컬 환경 구성

---

## ⚙️ 개발 환경 설정

### 하드웨어 요구사항

- **Raspberry Pi 4 Model B** (4GB 이상 권장)
- **USB 웹캠** (640x480 이상)
- **4채널 릴레이 모듈** (라즈베리파이 1만 해당)
- **microSD 카드** (32GB 이상)
- **네트워크**: Wi-Fi 또는 Ethernet

### 라즈베리파이 구분

- **라즈베리파이 1**: 좌측 웹캠 + GPIO 제어 (릴레이 모듈 연결)
- **라즈베리파이 2**: 우측 웹캠 전용 (GPIO 제어 없음)
- **라즈베리파이 3**: OHT 컨트롤러 (모터/센서 제어)

---

## 🍓 Raspberry Pi OS 설치

### 1. OS 이미지 다운로드 및 설치

```bash
# 1. Raspberry Pi Imager 다운로드
# https://www.raspberrypi.com/software/

# 2. OS 선택: Raspberry Pi OS (64-bit)

# 3. 고급 설정 (톱니바퀴 아이콘):
# - 호스트명: pcb-pi-left / pcb-pi-right / pcb-pi-oht
# - SSH 활성화
# - Wi-Fi 설정
# - 사용자: pi
# - 비밀번호: [설정]

# 4. SD 카드에 설치 후 부팅
```

### 2. 초기 설정 (SSH 접속 후)

```bash
# SSH 접속
ssh pi@pcb-pi-left.local  # 또는 Tailscale IP (예: 100.64.1.2)

# 시스템 업데이트
sudo apt update && sudo apt upgrade -y

# Python 버전 확인
python3 --version  # Python 3.10.x

# pip 설치
sudo apt install python3-pip -y
```

---

## 📦 패키지 설치

### 1. 프로젝트 클론

```bash
# 프로젝트 클론
git clone https://github.com/ArianSung/PCB_Detect_Project.git
cd PCB_Detect_Project

# 브랜치 전환
git checkout develop
git checkout feature/raspberry-pi
```

### 2. 필수 패키지 설치

```bash
# Python 패키지 설치
pip3 install opencv-python requests RPi.GPIO python-dotenv

# 웹캠 접근 권한 설정
sudo usermod -a -G video pi

# 재부팅 (권한 적용)
sudo reboot
```

---

## 🎥 웹캠 테스트

### 1. 웹캠 장치 확인

```bash
# 웹캠 장치 확인
ls /dev/video*

# 예상 출력:
# /dev/video0
```

### 2. OpenCV 웹캠 캡처 테스트

```bash
python3 << 'EOF'
import cv2

# 웹캠 열기
cap = cv2.VideoCapture(0)

# 프레임 캡처
ret, frame = cap.read()

if ret:
    print(f"✓ 웹캠 OK: 해상도 {frame.shape}")
else:
    print("✗ 웹캠 Error")

cap.release()
EOF

# 예상 출력:
# ✓ 웹캠 OK: 해상도 (480, 640, 3)
```

---

## 🔌 GPIO 테스트 (라즈베리파이 1만)

### GPIO 핀 매핑 (BCM 모드)

| 불량 분류 | GPIO 핀 | 물리 핀 | 용도 |
|-----------|---------|---------|------|
| **부품 불량** | GPIO 17 | Pin 11 | 릴레이 1 |
| **납땜 불량** | GPIO 27 | Pin 13 | 릴레이 2 |
| **폐기** | GPIO 22 | Pin 15 | 릴레이 3 |
| **정상** | GPIO 23 | Pin 16 | 릴레이 4 |

### GPIO 테스트 스크립트

```bash
# ⚠️ 주의: 실제 릴레이 연결 전에는 LED로 먼저 테스트

python3 << 'EOF'
import RPi.GPIO as GPIO
import time

# BCM 모드 설정
GPIO.setmode(GPIO.BCM)

# 핀 설정
pins = [17, 27, 22, 23]  # 부품불량, 납땜불량, 폐기, 정상

for pin in pins:
    GPIO.setup(pin, GPIO.OUT)
    GPIO.output(pin, GPIO.LOW)

# 각 핀 순서대로 테스트
for pin in pins:
    print(f"GPIO {pin} 활성화")
    GPIO.output(pin, GPIO.HIGH)
    time.sleep(1)
    GPIO.output(pin, GPIO.LOW)

GPIO.cleanup()
print("✓ GPIO 테스트 완료")
EOF
```

---

## 🌐 환경 변수 설정

### 1. 환경 설정 스크립트 실행

```bash
# 프로젝트 루트에서 실행
bash scripts/setup_env.sh
```

### 2. `.env` 파일 수정

```bash
# .env 파일 편집
nano raspberry_pi/.env
```

**`raspberry_pi/.env` 파일 내용:**

```bash
# 카메라 설정
CAMERA_ID=left             # 라즈베리파이 1: left, 라즈베리파이 2: right
CAMERA_INDEX=0
FPS=10
JPEG_QUALITY=85

# Flask 서버
SERVER_URL=http://100.x.x.x:5000  # GPU PC의 Tailscale IP로 변경

# GPIO 제어 (라즈베리파이 1만 true)
GPIO_ENABLED=true          # 라즈베리파이 1: true, 라즈베리파이 2: false

# GPIO 핀 매핑 (BCM 모드)
GPIO_COMPONENT_DEFECT=17
GPIO_SOLDER_DEFECT=27
GPIO_DISCARD=22
GPIO_NORMAL=23
```

---

## 🚀 카메라 클라이언트 실행

### 1. Mock 서버로 테스트 (Flask 서버 없을 때)

```bash
# GPU PC에서 Mock 서버 실행
python tests/api/mock_server.py

# 라즈베리파이에서 카메라 클라이언트 실행
python3 raspberry_pi/camera_client.py
```

### 2. 실제 Flask 서버로 테스트

```bash
# Tailscale VPN 연결 확인
tailscale status
tailscale ip -4  # 라즈베리파이 IP 확인

# 카메라 클라이언트 실행
python3 raspberry_pi/camera_client.py

# 예상 출력:
# ✓ Flask 서버 연결 성공 (http://100.x.x.x:5000)
# ✓ 웹캠 캡처 시작 (left, 10 FPS)
# [2025-10-25 14:30:00] 프레임 전송 → 분류: normal, GPIO 23 활성화
```

---

## 📝 첫 번째 작업 제안

### 작업 1: 웹캠 캡처 및 Base64 인코딩

**목표**: OpenCV로 프레임 캡처 후 Base64 변환

```python
# raspberry_pi/camera_client.py (기본 구조)
import cv2
import base64
import requests
from datetime import datetime

# 웹캠 열기
cap = cv2.VideoCapture(0)

# 프레임 캡처
ret, frame = cap.read()

if ret:
    # JPEG 인코딩
    _, buffer = cv2.imencode('.jpg', frame, [cv2.IMWRITE_JPEG_QUALITY, 85])

    # Base64 변환
    image_base64 = base64.b64encode(buffer).decode('utf-8')

    print(f"✓ 프레임 캡처 성공 (크기: {len(image_base64)} bytes)")

cap.release()
```

### 작업 2: Flask API 호출

**목표**: `/predict` API에 프레임 전송

```python
# Flask API 호출
response = requests.post(
    "http://100.x.x.x:5000/predict",
    json={
        "camera_id": "left",
        "image": image_base64,
        "timestamp": datetime.now().isoformat()
    },
    timeout=5
)

result = response.json()
print(f"분류: {result['result']['classification']}")
print(f"신뢰도: {result['result']['confidence']}")
print(f"GPIO 핀: {result['gpio_action']['pin']}")
```

### 작업 3: GPIO 제어 (라즈베리파이 1만)

**목표**: Flask 응답 기반 GPIO 핀 제어

```python
import RPi.GPIO as GPIO

# GPIO 설정
GPIO.setmode(GPIO.BCM)
GPIO.setup(17, GPIO.OUT)  # 부품 불량
GPIO.setup(27, GPIO.OUT)  # 납땜 불량
GPIO.setup(22, GPIO.OUT)  # 폐기
GPIO.setup(23, GPIO.OUT)  # 정상

# Flask 응답에서 GPIO 핀 가져오기
gpio_pin = result['gpio_action']['pin']

# 해당 핀 활성화 (1초)
GPIO.output(gpio_pin, GPIO.HIGH)
time.sleep(1)
GPIO.output(gpio_pin, GPIO.LOW)
```

---

## 🤖 AI에게 물어볼 프롬프트

### 시작 프롬프트 (복사해서 사용하세요)

```
안녕! 나는 PCB 불량 검사 시스템의 라즈베리파이 팀원이야.

**내 역할:**
- 웹캠에서 PCB 프레임 캡처 (OpenCV)
- JPEG 인코딩 및 Base64 변환
- Flask API 호출 (`/predict`)
- GPIO 핀 제어 (릴레이 모듈 → 불량 분류 게이트)

**읽어야 할 핵심 문서:**
1. `docs/RaspberryPi_Setup.md` - 라즈베리파이 환경 설정 및 클라이언트 가이드
2. `docs/API_Contract.md` - Flask API 명세서
3. `raspberry_pi/.env.example` - 환경 변수 템플릿
4. `tests/api/mock_server.py` - Mock Flask 서버 (독립 개발용)

**개발 환경:**
- 하드웨어: Raspberry Pi 4 Model B (4GB)
- OS: Raspberry Pi OS (64-bit)
- 웹캠: USB 웹캠 (640x480)
- 릴레이: 4채널 릴레이 모듈 (라즈베리파이 1만 해당)
- 네트워크: Tailscale VPN (100.x.x.x)

**환경 변수 설정 (raspberry_pi/.env):**
```
CAMERA_ID=left             # 또는 right
CAMERA_INDEX=0
SERVER_URL=http://100.x.x.x:5000
FPS=10
JPEG_QUALITY=85
GPIO_ENABLED=true          # 라즈베리파이 1: true, 라즈베리파이 2: false
```

**GPIO 핀 매핑 (BCM 모드, 라즈베리파이 1만):**
- GPIO 17: 부품 불량
- GPIO 27: 납땜 불량
- GPIO 22: 폐기
- GPIO 23: 정상

**첫 번째 작업:**
1. 웹캠 테스트:
   ```python
   python3 -c "import cv2; cap = cv2.VideoCapture(0); print('OK' if cap.isOpened() else 'Error')"
   ```
2. GPIO 테스트 (라즈베리파이 1만):
   ```python
   python3 -c "import RPi.GPIO as GPIO; GPIO.setmode(GPIO.BCM); print('GPIO OK')"
   ```
3. Mock 서버로 테스트 (Flask 서버 없을 때):
   - GPU PC에서 `python tests/api/mock_server.py` 실행
   - 라즈베리파이에서 `python3 raspberry_pi/camera_client.py` 실행

위 정보를 바탕으로, 라즈베리파이에서 웹캠과 GPIO를 테스트하고 Flask API와 통신하는 과정을 안내해줘.
특히 Flask 서버가 아직 없을 때 Mock 서버로 독립 개발하는 방법을 알려줘.
```

---

## ✅ 체크리스트

### 하드웨어 설정 완료 체크리스트

- [ ] Raspberry Pi OS 설치 완료
- [ ] SSH 접속 확인 완료
- [ ] 웹캠 인식 확인 (`/dev/video0`)
- [ ] 웹캠 캡처 테스트 성공 (OpenCV)
- [ ] GPIO 핀 테스트 성공 (라즈베리파이 1만)
- [ ] Tailscale VPN 연결 확인

### 소프트웨어 설정 완료 체크리스트

- [ ] Python 패키지 설치 완료 (`opencv-python`, `requests`, `RPi.GPIO`)
- [ ] `raspberry_pi/.env` 파일 설정 완료
- [ ] Mock 서버 연결 테스트 성공
- [ ] 실제 Flask 서버 연결 테스트 성공

### 문서 읽기 체크리스트

- [ ] `docs/RaspberryPi_Setup.md` 읽기 완료
- [ ] `docs/API_Contract.md` 읽기 완료
- [ ] `docs/Team_Collaboration_Guide.md` 읽기 완료
- [ ] `docs/Git_Workflow.md` 읽기 완료

---

## 🚨 자주 발생하는 문제 및 해결

### 문제 1: 웹캠 인식 안 됨

**에러**: `/dev/video0` 장치 없음

**해결 방법:**
1. 웹캠 USB 재연결
2. 장치 확인: `ls -l /dev/video*`
3. 권한 확인: `groups pi` (video 그룹 포함 확인)
4. 재부팅: `sudo reboot`

### 문제 2: Flask API 연결 실패

**에러**: `Connection refused`

**해결 방법:**
1. Tailscale VPN 연결 확인: `tailscale status`
2. Flask 서버 실행 중인지 확인
3. `.env` 파일의 `SERVER_URL` 확인
4. Mock 서버로 테스트: `python tests/api/mock_server.py`

### 문제 3: GPIO 권한 오류

**에러**: `RuntimeError: No access to /dev/mem`

**해결 방법:**
1. `sudo` 권한으로 실행 또는
2. GPIO 그룹에 사용자 추가:
   ```bash
   sudo usermod -a -G gpio pi
   sudo reboot
   ```

---

## 🔗 Systemd 서비스 자동 실행 (선택)

### 서비스 파일 생성

```bash
# 서비스 파일 생성
sudo nano /etc/systemd/system/camera-client-left.service
```

**파일 내용:**

```ini
[Unit]
Description=PCB Camera Client (Left)
After=network.target

[Service]
Type=simple
User=pi
WorkingDirectory=/home/pi/PCB_Detect_Project
ExecStart=/usr/bin/python3 raspberry_pi/camera_client.py
Restart=always

[Install]
WantedBy=multi-user.target
```

### 서비스 활성화

```bash
# 서비스 활성화
sudo systemctl enable camera-client-left.service

# 서비스 시작
sudo systemctl start camera-client-left.service

# 상태 확인
sudo systemctl status camera-client-left.service
```

---

## 📞 도움 요청

- **라즈베리파이 팀 리더**: [연락처]
- **Flask 팀 (API)**: [연락처]
- **전체 팀 채팅방**: [링크]

---

**마지막 업데이트**: 2025-10-25
**작성자**: 팀 리더
