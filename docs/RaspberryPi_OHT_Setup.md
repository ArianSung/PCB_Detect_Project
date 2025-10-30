# 라즈베리파이 OHT 시스템 설치 및 설정 가이드

## 개요

이 문서는 **라즈베리파이 3번**을 사용하여 OHT(Overhead Hoist Transport) 시스템을 구축하는 방법을 설명합니다.

**주요 기능**:
- Flask API 폴링 (5초마다 OHT 요청 확인)
- 스텝모터 3개 제어 (X축 1개, Z축 좌/우 2개)
- 서보모터 걸쇠 제어
- 리미트 스위치 6개 모니터링
- 10단계 OHT 시퀀스 자동 실행
- 긴급 정지 버튼

**하드웨어 구성**:
- 라즈베리파이 4 Model B (4GB 이상 권장)
- NEMA 17 스텝모터 × 3
- A4988 드라이버 × 3
- MG996R 서보모터 × 1
- 리미트 스위치 × 6
- 긴급 정지 버튼 × 1
- 12V 5A 전원 (스텝모터용)
- 5V 2A 전원 (서보모터용)

**소프트웨어 구성**:
- Raspberry Pi OS (64-bit)
- Python 3.9+
- **pigpio** (하드웨어 타이밍 GPIO 라이브러리)
- requests

---

## Phase 1: 라즈베리파이 OS 설치

### 1-1. Raspberry Pi Imager로 OS 설치

```bash
# 1. Raspberry Pi Imager 다운로드
# https://www.raspberrypi.com/software/

# 2. OS 선택: Raspberry Pi OS (64-bit)

# 3. 고급 설정 (톱니바퀴 아이콘):
# - 호스트명: pcb-pi-oht
# - SSH 활성화
# - Wi-Fi 설정 (또는 Ethernet 사용)
# - 사용자: pi
# - 비밀번호: [설정]
# - 로케일: Asia/Seoul, ko_KR.UTF-8

# 4. SD 카드에 설치 후 부팅
```

### 1-2. 초기 설정

```bash
# SSH 접속 (로컬 네트워크 또는 Tailscale)
ssh pi@pcb-pi-oht.local  # 또는 Tailscale IP (100.x.x.x)

# 시스템 업데이트
sudo apt update && sudo apt upgrade -y

# Python 버전 확인
python3 --version  # Python 3.10.x 이상

# pip 설치
sudo apt install python3-pip -y

# 재부팅
sudo reboot
```

---

## Phase 2: pigpio 설치 및 설정

### 2-1. pigpio 라이브러리 설치

**pigpio**는 하드웨어 타이밍을 제공하는 고성능 GPIO 라이브러리입니다. 스텝모터 제어 시 정밀한 타이밍을 보장합니다.

```bash
# pigpio 데몬 및 Python 라이브러리 설치
sudo apt install pigpio python3-pigpio -y

# pigpio 버전 확인
pigpiod -v
# 예상 출력: V79 또는 최신 버전
```

### 2-2. pigpiod 데몬 자동 시작 설정

pigpio는 데몬(pigpiod) 프로세스가 백그라운드에서 실행되어야 합니다.

```bash
# pigpiod 시스템 서비스 활성화
sudo systemctl enable pigpiod

# pigpiod 시작
sudo systemctl start pigpiod

# pigpiod 상태 확인
sudo systemctl status pigpiod

# 예상 출력:
# ● pigpiod.service - Daemon required to control GPIO pins via pigpio
#    Loaded: loaded (/lib/systemd/system/pigpiod.service; enabled)
#    Active: active (running)
```

**수동 시작 방법** (필요 시):
```bash
# pigpiod 수동 시작
sudo pigpiod

# pigpiod 정지
sudo killall pigpiod
```

---

## Phase 3: 하드웨어 연결

### 3-1. GPIO 핀맵 (BCM 모드)

| 기능 | GPIO 핀 (BCM) | 물리 핀 | 연결 대상 |
|------|---------------|---------|-----------|
| **X축 스텝모터 (A4988)** ||||
| STEP | GPIO 18 | Pin 12 | A4988 #1 STEP |
| DIR | GPIO 23 | Pin 16 | A4988 #1 DIR |
| ENABLE | GPIO 24 | Pin 18 | A4988 #1 ENABLE |
| **Z축 좌측 스텝모터 (A4988)** ||||
| STEP | GPIO 17 | Pin 11 | A4988 #2 STEP |
| DIR | GPIO 27 | Pin 13 | A4988 #2 DIR |
| ENABLE | GPIO 22 | Pin 15 | A4988 #2 ENABLE |
| **Z축 우측 스텝모터 (A4988)** ||||
| STEP | GPIO 25 | Pin 22 | A4988 #3 STEP |
| DIR | GPIO 8 | Pin 24 | A4988 #3 DIR |
| ENABLE | GPIO 7 | Pin 26 | A4988 #3 ENABLE |
| **서보모터 (MG996R)** ||||
| PWM | GPIO 12 | Pin 32 | 서보모터 신호선 |
| **리미트 스위치 (X축)** ||||
| 창고 위치 | GPIO 5 | Pin 29 | 리미트 스위치 #1 |
| 박스3 끝 | GPIO 6 | Pin 31 | 리미트 스위치 #2 |
| **리미트 스위치 (Z축)** ||||
| 좌측 상단 | GPIO 16 | Pin 36 | 리미트 스위치 #3 |
| 좌측 하단 | GPIO 20 | Pin 38 | 리미트 스위치 #4 |
| 우측 상단 | GPIO 21 | Pin 40 | 리미트 스위치 #5 |
| 우측 하단 | GPIO 19 | Pin 35 | 리미트 스위치 #6 |
| **긴급 정지 버튼** ||||
| 긴급 정지 | GPIO 26 | Pin 37 | 긴급 정지 버튼 |
| **전원** ||||
| 5V | 5V | Pin 2, 4 | A4988 VDD (3개) |
| GND | GND | Pin 6, 9, 14, 20, 25, 30, 34, 39 | 공통 그라운드 |

### 3-2. A4988 드라이버 배선 (스텝모터 3개)

각 A4988 드라이버는 다음과 같이 연결합니다:

```
[라즈베리파이]          [A4988 드라이버]         [NEMA 17 스텝모터]
GPIO 18 (STEP)    -->   STEP
GPIO 23 (DIR)     -->   DIR
GPIO 24 (ENABLE)  -->   ENABLE
5V                -->   VDD
GND               -->   GND
                        VMOT            <--   12V 전원 (+)
                        GND             <--   12V 전원 (-)
                        1B              <--   모터 코일 1 (빨강)
                        1A              <--   모터 코일 1 (초록)
                        2A              <--   모터 코일 2 (파랑)
                        2B              <--   모터 코일 2 (노랑)
```

**마이크로스테핑 설정** (A4988):
- MS1, MS2, MS3 핀을 HIGH로 설정하면 1/16 마이크로스테핑 가능
- 핀 상태:
  - MS1 = HIGH
  - MS2 = HIGH
  - MS3 = HIGH
- 점퍼를 사용하여 5V에 연결

### 3-3. 서보모터 배선 (MG996R)

```
[라즈베리파이]          [MG996R 서보모터]
GPIO 12 (PWM)     -->   신호선 (주황/노랑)
5V (별도 전원)    -->   전원선 (빨강)
GND (공통)        -->   그라운드선 (갈색/검정)
```

⚠️ **주의**: 서보모터는 전류 소비가 크므로 **별도 5V 2A 전원**을 사용하는 것이 권장됩니다. 라즈베리파이 5V 핀 사용 시 과부하 위험이 있습니다.

### 3-4. 리미트 스위치 배선

리미트 스위치는 **Pull-Up 저항** 방식으로 연결합니다:

```
[리미트 스위치]          [라즈베리파이]
NO (Normally Open)  -->  GPIO 핀 (5, 6, 16, 19, 20, 21)
COM (Common)        -->  GND
```

- 스위치 누르지 않은 상태: GPIO 읽기 = **HIGH** (Pull-Up)
- 스위치 눌린 상태: GPIO 읽기 = **LOW** (GND 연결)

코드에서 Pull-Up 설정:
```python
pi.set_pull_up_down(LIMIT_SW_WAREHOUSE, pigpio.PUD_UP)
```

### 3-5. 긴급 정지 버튼 배선

```
[긴급 정지 버튼]        [라즈베리파이]
NO (Normally Open)  -->  GPIO 26
COM (Common)        -->  GND
```

---

## Phase 4: 프로젝트 설치

### 4-1. 프로젝트 클론

```bash
# 프로젝트 클론
cd ~
git clone https://github.com/ArianSung/PCB_Detect_Project.git
cd PCB_Detect_Project

# 브랜치 전환 (필요 시)
git checkout develop
```

### 4-2. Python 패키지 설치

```bash
# 필수 패키지 설치
pip3 install requests pigpio

# 설치 확인
python3 -c "import pigpio; import requests; print('✓ 패키지 설치 완료')"
```

### 4-3. 환경 변수 설정 (선택)

Flask 서버 URL을 환경 변수로 설정할 수 있습니다:

```bash
# ~/.bashrc 편집
nano ~/.bashrc

# 마지막에 추가
export FLASK_SERVER_URL="http://100.64.1.1:5000"

# 적용
source ~/.bashrc
```

또는 스크립트 내 하드코딩된 URL을 직접 수정:

```python
# raspberry_pi/oht_controller.py
FLASK_SERVER_URL = "http://100.64.1.1:5000"  # GPU PC의 Tailscale IP로 변경
```

---

## Phase 5: 개별 테스트

하드웨어 연결 후, 각 구성 요소를 개별적으로 테스트합니다.

### 5-1. pigpio 연결 테스트

```bash
python3 << 'EOF'
import pigpio

pi = pigpio.pi()

if pi.connected:
    print("✓ pigpio 연결 성공")
    pi.stop()
else:
    print("✗ pigpio 연결 실패 - pigpiod 데몬 확인")
EOF
```

**예상 출력**:
```
✓ pigpio 연결 성공
```

### 5-2. 리미트 스위치 테스트

```bash
python3 << 'EOF'
import pigpio
import time

pi = pigpio.pi()

# 창고 리미트 스위치 (GPIO 5)
LIMIT_SW_WAREHOUSE = 5

pi.set_mode(LIMIT_SW_WAREHOUSE, pigpio.INPUT)
pi.set_pull_up_down(LIMIT_SW_WAREHOUSE, pigpio.PUD_UP)

print("리미트 스위치 테스트 (10초간, Ctrl+C로 종료)")
print("스위치를 눌렀다 떼면서 확인하세요.")

try:
    for _ in range(100):
        state = pi.read(LIMIT_SW_WAREHOUSE)
        print(f"GPIO 5 상태: {'LOW (눌림)' if state == 0 else 'HIGH (안 눌림)'}", end='\r')
        time.sleep(0.1)
except KeyboardInterrupt:
    print("\n종료")

pi.stop()
EOF
```

**예상 동작**:
- 스위치 안 누른 상태: `HIGH (안 눌림)`
- 스위치 누른 상태: `LOW (눌림)`

### 5-3. 스텝모터 테스트 (X축)

⚠️ **주의**: 모터가 실제로 연결되어 있고, 벨트가 장착되어 있는지 확인하세요.

```bash
python3 << 'EOF'
import pigpio
import time

pi = pigpio.pi()

STEP_PIN_X = 18
DIR_PIN_X = 23
ENABLE_PIN_X = 24

pi.set_mode(STEP_PIN_X, pigpio.OUTPUT)
pi.set_mode(DIR_PIN_X, pigpio.OUTPUT)
pi.set_mode(ENABLE_PIN_X, pigpio.OUTPUT)

# 모터 활성화
pi.write(ENABLE_PIN_X, 0)

# 시계방향 100 스텝
print("X축 모터 시계방향 100 스텝")
pi.write(DIR_PIN_X, 1)

for _ in range(100):
    pi.write(STEP_PIN_X, 1)
    time.sleep(0.001)
    pi.write(STEP_PIN_X, 0)
    time.sleep(0.001)

time.sleep(1)

# 반시계방향 100 스텝
print("X축 모터 반시계방향 100 스텝")
pi.write(DIR_PIN_X, 0)

for _ in range(100):
    pi.write(STEP_PIN_X, 1)
    time.sleep(0.001)
    pi.write(STEP_PIN_X, 0)
    time.sleep(0.001)

# 모터 비활성화
pi.write(ENABLE_PIN_X, 1)

print("✓ 스텝모터 테스트 완료")
pi.stop()
EOF
```

**예상 동작**:
- 모터가 시계방향으로 약간 회전 후, 반시계방향으로 원위치

### 5-4. 서보모터 테스트

```bash
python3 << 'EOF'
import pigpio
import time

pi = pigpio.pi()

SERVO_PIN_LATCH = 12

pi.set_mode(SERVO_PIN_LATCH, pigpio.OUTPUT)

print("서보모터 테스트")

# 0도
print("0도 (수평)")
pi.set_servo_pulsewidth(SERVO_PIN_LATCH, 500)
time.sleep(1)

# 90도
print("90도 (잠금)")
pi.set_servo_pulsewidth(SERVO_PIN_LATCH, 1500)
time.sleep(1)

# 0도
print("0도 (해제)")
pi.set_servo_pulsewidth(SERVO_PIN_LATCH, 500)
time.sleep(1)

# PWM 정지
pi.set_servo_pulsewidth(SERVO_PIN_LATCH, 0)

print("✓ 서보모터 테스트 완료")
pi.stop()
EOF
```

**예상 동작**:
- 서보모터가 0° → 90° → 0° 순서로 회전

### 5-5. Z축 동기화 테스트

⚠️ **주의**: Z축 모터 2개가 모두 연결되어 있고, 리미트 스위치 4개가 설치되어 있어야 합니다.

```bash
python3 << 'EOF'
import pigpio
import time

pi = pigpio.pi()

# Z축 좌측 스텝모터
STEP_PIN_Z_LEFT = 17
DIR_PIN_Z_LEFT = 27
ENABLE_PIN_Z_LEFT = 22

# Z축 우측 스텝모터
STEP_PIN_Z_RIGHT = 25
DIR_PIN_Z_RIGHT = 8
ENABLE_PIN_Z_RIGHT = 7

# 리미트 스위치 (Z축)
LIMIT_SW_Z_LEFT_UP = 16
LIMIT_SW_Z_LEFT_DOWN = 20
LIMIT_SW_Z_RIGHT_UP = 21
LIMIT_SW_Z_RIGHT_DOWN = 19

# 핀 모드 설정
for pin in [STEP_PIN_Z_LEFT, DIR_PIN_Z_LEFT, ENABLE_PIN_Z_LEFT]:
    pi.set_mode(pin, pigpio.OUTPUT)

for pin in [STEP_PIN_Z_RIGHT, DIR_PIN_Z_RIGHT, ENABLE_PIN_Z_RIGHT]:
    pi.set_mode(pin, pigpio.OUTPUT)

for pin in [LIMIT_SW_Z_LEFT_UP, LIMIT_SW_Z_LEFT_DOWN,
            LIMIT_SW_Z_RIGHT_UP, LIMIT_SW_Z_RIGHT_DOWN]:
    pi.set_mode(pin, pigpio.INPUT)
    pi.set_pull_up_down(pin, pigpio.PUD_UP)

# 모터 활성화
pi.write(ENABLE_PIN_Z_LEFT, 0)
pi.write(ENABLE_PIN_Z_RIGHT, 0)

# 베드 내리기 (DOWN)
print("Z축 베드 내리기 (동기화)")
pi.write(DIR_PIN_Z_LEFT, 0)
pi.write(DIR_PIN_Z_RIGHT, 0)

count = 0
while count < 500:  # 최대 500 스텝
    left_down = pi.read(LIMIT_SW_Z_LEFT_DOWN)
    right_down = pi.read(LIMIT_SW_Z_RIGHT_DOWN)

    if left_down and right_down:
        print("\n✓ 양쪽 하단 리미트 스위치 도달")
        break

    if not left_down:
        pi.write(STEP_PIN_Z_LEFT, 1)
    if not right_down:
        pi.write(STEP_PIN_Z_RIGHT, 1)

    time.sleep(0.001)

    pi.write(STEP_PIN_Z_LEFT, 0)
    pi.write(STEP_PIN_Z_RIGHT, 0)

    time.sleep(0.001)
    count += 1

time.sleep(1)

# 베드 올리기 (UP)
print("Z축 베드 올리기 (동기화)")
pi.write(DIR_PIN_Z_LEFT, 1)
pi.write(DIR_PIN_Z_RIGHT, 1)

count = 0
while count < 500:
    left_up = pi.read(LIMIT_SW_Z_LEFT_UP)
    right_up = pi.read(LIMIT_SW_Z_RIGHT_UP)

    if left_up and right_up:
        print("\n✓ 양쪽 상단 리미트 스위치 도달")
        break

    if not left_up:
        pi.write(STEP_PIN_Z_LEFT, 1)
    if not right_up:
        pi.write(STEP_PIN_Z_RIGHT, 1)

    time.sleep(0.001)

    pi.write(STEP_PIN_Z_LEFT, 0)
    pi.write(STEP_PIN_Z_RIGHT, 0)

    time.sleep(0.001)
    count += 1

# 모터 비활성화
pi.write(ENABLE_PIN_Z_LEFT, 1)
pi.write(ENABLE_PIN_Z_RIGHT, 1)

print("✓ Z축 동기화 테스트 완료")
pi.stop()
EOF
```

**예상 동작**:
- 베드가 동기화되어 내려갔다가 올라옴
- 양쪽 모터가 리미트 스위치에 도달하면 정지

---

## Phase 6: OHT Controller 실행

### 6-1. 수동 실행

```bash
cd ~/PCB_Detect_Project

# OHT Controller 실행
python3 raspberry_pi/oht_controller.py
```

**예상 출력**:
```
2025-10-30 14:30:00 - __main__ - INFO - pigpio 연결 성공
2025-10-30 14:30:00 - __main__ - INFO - 센서 초기화 완료
2025-10-30 14:30:00 - __main__ - INFO - OHTController 초기화 완료
2025-10-30 14:30:00 - __main__ - INFO - OHT Controller 시작 (폴링 간격: 5초)
```

### 6-2. Flask API 연결 확인

OHT Controller가 Flask API와 통신하는지 확인:

```bash
# 로그 확인
tail -f /home/pi/oht_controller.log
```

**정상 연결 시**:
```
2025-10-30 14:30:05 - __main__ - INFO - Flask API 폴링: 요청 없음
2025-10-30 14:30:10 - __main__ - INFO - Flask API 폴링: 요청 없음
```

**Flask 서버 미실행 시**:
```
2025-10-30 14:30:05 - __main__ - ERROR - Flask API 폴링 오류: Connection refused
```

### 6-3. 테스트 OHT 요청 (수동)

Flask 서버 없이 로컬에서 테스트하려면, 스크립트 내 `execute_sequence()` 함수를 직접 호출:

```bash
python3 << 'EOF'
import sys
sys.path.append('/home/pi/PCB_Detect_Project/raspberry_pi')

from oht_controller import OHTController

controller = OHTController()

# NORMAL 박스 테스트
success = controller.execute_sequence('NORMAL')

print(f"\n{'✓ 성공' if success else '✗ 실패'}")

controller.cleanup()
EOF
```

---

## Phase 7: Systemd 서비스 설정 (자동 시작)

### 7-1. 서비스 파일 생성

```bash
sudo nano /etc/systemd/system/oht-controller.service
```

**파일 내용**:

```ini
[Unit]
Description=PCB OHT Controller
After=network.target pigpiod.service
Requires=pigpiod.service

[Service]
Type=simple
User=pi
WorkingDirectory=/home/pi/PCB_Detect_Project
ExecStart=/usr/bin/python3 /home/pi/PCB_Detect_Project/raspberry_pi/oht_controller.py
Restart=always
RestartSec=10
StandardOutput=journal
StandardError=journal

# 환경 변수 (필요 시)
Environment="FLASK_SERVER_URL=http://100.64.1.1:5000"

[Install]
WantedBy=multi-user.target
```

### 7-2. 서비스 활성화

```bash
# 서비스 파일 리로드
sudo systemctl daemon-reload

# 서비스 활성화 (부팅 시 자동 시작)
sudo systemctl enable oht-controller.service

# 서비스 시작
sudo systemctl start oht-controller.service

# 상태 확인
sudo systemctl status oht-controller.service
```

**예상 출력**:
```
● oht-controller.service - PCB OHT Controller
   Loaded: loaded (/etc/systemd/system/oht-controller.service; enabled)
   Active: active (running) since Mon 2025-10-30 14:30:00 KST; 5s ago
 Main PID: 1234 (python3)
   CGroup: /system.slice/oht-controller.service
           └─1234 /usr/bin/python3 /home/pi/PCB_Detect_Project/raspberry_pi/oht_controller.py

Oct 30 14:30:00 pcb-pi-oht systemd[1]: Started PCB OHT Controller.
Oct 30 14:30:00 pcb-pi-oht python3[1234]: 2025-10-30 14:30:00 - __main__ - INFO - pigpio 연결 성공
```

### 7-3. 로그 확인

```bash
# systemd 저널 로그 확인
sudo journalctl -u oht-controller.service -f

# 또는 파일 로그 확인
tail -f /home/pi/oht_controller.log
```

---

## Phase 8: 트러블슈팅

### 문제 1: pigpio 연결 실패

**에러**:
```
RuntimeError: pigpio 데몬에 연결할 수 없습니다. 'sudo pigpiod' 실행 확인
```

**해결 방법**:

1. pigpiod 데몬 실행 확인:
```bash
sudo systemctl status pigpiod
```

2. pigpiod 수동 시작:
```bash
sudo pigpiod
```

3. pigpiod 재시작:
```bash
sudo systemctl restart pigpiod
```

---

### 문제 2: Flask API 연결 실패

**에러**:
```
Flask API 폴링 오류: Connection refused
```

**해결 방법**:

1. Flask 서버 실행 확인:
```bash
# GPU PC에서
curl http://100.64.1.1:5000/health
```

2. Tailscale VPN 연결 확인:
```bash
tailscale status
```

3. Flask 서버 URL 수정:
```python
# raspberry_pi/oht_controller.py
FLASK_SERVER_URL = "http://100.64.1.1:5000"  # 올바른 IP로 변경
```

---

### 문제 3: 스텝모터가 움직이지 않음

**원인**:
1. A4988 드라이버 전원 미연결
2. ENABLE 핀이 HIGH (비활성화 상태)
3. 모터 배선 오류

**해결 방법**:

1. A4988 VMOT에 12V 전원 연결 확인
2. ENABLE 핀 상태 확인 (LOW = 활성화):
```python
pi.write(ENABLE_PIN_X, 0)  # 활성화
```

3. 모터 코일 연결 확인 (멀티미터로 저항 측정)

---

### 문제 4: 리미트 스위치가 동작하지 않음

**원인**:
1. Pull-Up 저항 미설정
2. 스위치 배선 오류
3. 스위치 불량

**해결 방법**:

1. Pull-Up 설정 확인:
```python
pi.set_pull_up_down(LIMIT_SW_WAREHOUSE, pigpio.PUD_UP)
```

2. 스위치 수동 테스트:
```bash
python3 -c "import pigpio; pi = pigpio.pi(); pi.set_mode(5, pigpio.INPUT); pi.set_pull_up_down(5, pigpio.PUD_UP); print(pi.read(5)); pi.stop()"
# 예상: 1 (안 눌림) 또는 0 (눌림)
```

3. 배선 확인: NO → GPIO, COM → GND

---

### 문제 5: 서보모터가 떨림 (지터)

**원인**:
1. 전원 부족
2. PWM 주파수 불안정
3. 소프트웨어 PWM 사용 (RPi.GPIO)

**해결 방법**:

1. 서보모터에 **별도 5V 2A 전원** 사용
2. pigpio 사용 (하드웨어 타이밍)
3. 펄스폭 조정:
```python
# 0도: 500us ~ 1000us
# 90도: 1500us
# 180도: 2000us ~ 2500us
pi.set_servo_pulsewidth(SERVO_PIN_LATCH, 1500)  # 90도
```

---

### 문제 6: Z축 동기화 실패 (베드 기울어짐)

**원인**:
1. 양쪽 모터 속도 불일치
2. 벨트 장력 차이
3. 리미트 스위치 위치 오차

**해결 방법**:

1. 리미트 스위치 4개 위치 재조정 (정확히 수평)
2. 벨트 장력 균등하게 조정
3. 코드 내 `time.sleep(0.0005)` 값 동일하게 유지
4. 모터별로 독립 정지 로직 사용 (현재 코드 유지)

---

### 문제 7: 긴급 정지가 작동하지 않음

**원인**:
1. 긴급 정지 버튼 배선 오류
2. Pull-Up 미설정
3. 콜백 미등록

**해결 방법**:

1. 배선 확인: NO → GPIO 26, COM → GND
2. Pull-Up 설정:
```python
pi.set_pull_up_down(EMERGENCY_STOP_PIN, pigpio.PUD_UP)
```

3. 콜백 등록 확인:
```python
pi.callback(EMERGENCY_STOP_PIN, pigpio.FALLING_EDGE, self._emergency_stop_callback)
```

4. 수동 테스트:
```bash
python3 -c "import pigpio; pi = pigpio.pi(); pi.set_mode(26, pigpio.INPUT); pi.set_pull_up_down(26, pigpio.PUD_UP); print('눌렀을 때:', pi.read(26)); pi.stop()"
# 버튼 누른 상태에서 실행 → 0 출력 예상
```

---

## 참고 문서

- **[OHT_System_Setup.md](OHT_System_Setup.md)**: OHT 시스템 전체 설계 및 하드웨어 가이드
- **[PCB_Defect_Detection_Project.md](PCB_Defect_Detection_Project.md)**: 전체 시스템 아키텍처
- **[Flask_Server_Setup.md](Flask_Server_Setup.md)**: Flask API 명세
- **[RaspberryPi_Setup.md](RaspberryPi_Setup.md)**: 라즈베리파이 기본 설정

## pigpio 공식 문서

- **pigpio C 라이브러리**: http://abyz.me.uk/rpi/pigpio/
- **Python pigpio**: http://abyz.me.uk/rpi/pigpio/python.html

---

**마지막 업데이트**: 2025-10-30
**작성자**: 시스템 팀
