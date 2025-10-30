# OHT (Overhead Hoist Transport) 시스템 구축 가이드

## 개요

이 가이드는 PCB 분류 박스에서 창고로 PCB를 자동 운반하는 OHT 시스템 구축 방법을 설명합니다. OHT 시스템은 **수평으로 나란히 배치된 3개 박스**(각 5슬롯)를 사용하여 정상, 부품불량, 납땜불량 PCB를 수거하고 창고로 이동합니다.

**핵심 기능**:
- 3개 박스 수평 배치 (정상, 부품불량, 납땜불량)
- 각 박스 5슬롯 (총 15슬롯)
- 천장 레일 기반 X축 수평 이동
- 양쪽 스텝모터 기반 Z축 동기화 상하 이동 (베드 내리기/올리기)
- 서보모터 걸쇠 방식 박스 픽업
- 수동 호출 (WinForms, Admin/Operator 권한) 및 자동 호출 (박스 가득 참)
- 라즈베리파이 3번(OHT 전용) pigpio 제어

---

## 시스템 구성

### 하드웨어 구성

#### OHT 이동 시스템

**X축 수평 이동 (박스 간 이동)**:
- **천장 레일**: 알루미늄 프로파일 (길이: 창고 ~ 박스3, 약 3-5m)
- **스텝모터**: NEMA 17 (1.8° 스텝 각도, 홀딩 토크 40-50 Ncm)
- **드라이버**: A4988 (마이크로스테핑 1/16)
- **타이밍 벨트**: GT2 (폭 6mm, 피치 2mm)

**Z축 상하 이동 (베드 내리기/올리기)** ⭐ 신규 설계:
- **구조**: 양쪽 스텝모터 2개 + GT2 벨트 감기 방식
- **스텝모터**: NEMA 17 × 2 (좌측, 우측)
- **드라이버**: A4988 × 2
- **타이밍 벨트**: GT2 (폭 6mm) × 2
- **이동 거리**: 약 50-100cm (박스 높이 + 여유)
- **동기화 방식**: 양쪽 모터 동시 신호 + 리미트 스위치 4개로 수평 유지

```
[베드 프레임 - 양쪽에서 지지]
    │              │
벨트 │              │ 벨트
    ↓              ↓
┌────────────────────┐
│ 좌측 모터        우측 모터 │
│ (NEMA 17)      (NEMA 17) │
│   ↓              ↓      │
│ GT2 벨트       GT2 벨트  │
│ 감기/풀기      감기/풀기  │
└────────────────────┘
```

**베드 걸쇠 메커니즘** ⭐ 신규:
- **서보모터**: MG996R (토크 9.4 kgf·cm)
- **걸쇠 방식**: L자형 핀 회전 (0도 = 수평 삽입, 90도 = 잠금)
- **박스 구조**: 측면에 구멍 (직경 10-15mm)

```
[베드 내부 서보모터]
     │
     ↓
  L자 핀 ─→ 0도 (수평) ─→ 박스 구멍에 삽입
     │
     ↓
  90도 회전 ─→ 걸림 (잠금)
```

#### OHT 박스 배치 구조

```
[창고]───[박스1: 정상]───[박스2: 부품불량]───[박스3: 납땜불량]
         (5슬롯)          (5슬롯)             (5슬롯)
          │                │                  │
     PCB 5개 수납      PCB 5개 수납       PCB 5개 수납

박스 크기: PCB 5개 수납 가능 (세로로 배치)
재질: 아크릴 또는 경량 플라스틱
무게: 빈 박스 약 500g, PCB 5개 적재 시 약 1.5kg
```

#### 센서 시스템

**리미트 스위치 (X축)** - 2개:
- 창고 위치 감지 (홈 포지션)
- 최종 박스 위치 감지 (안전 한계)

**리미트 스위치 (Z축)** - 4개 ⭐ 신규:
- 좌측 상단: 베드 왼쪽 완전히 올라간 상태
- 좌측 하단: 베드 왼쪽 완전히 내려간 상태
- 우측 상단: 베드 오른쪽 완전히 올라간 상태
- 우측 하단: 베드 오른쪽 완전히 내려간 상태

**위치 제어 방식**:
- X축: 리미트 스위치 2개 + 스텝 카운팅 (하이브리드)
- Z축: 리미트 스위치 4개 + 양쪽 동기화

#### 제어 시스템

- **라즈베리파이 3번 (Raspberry Pi 4 Model B)**: OHT 전용 제어기
  - 위치: 로컬 (Tailscale VPN 또는 로컬 네트워크)
  - OS: Raspberry Pi OS (64-bit)
  - Python 3.9+
- **전원**:
  - 라즈베리파이: 5V 3A
  - 스텝모터 3개: 12V 5A (공용 전원)
  - 서보모터: 5V 2A (별도 전원 권장)

### 소프트웨어 구성

- **라즈베리파이 3번**: Python + pigpio + systemd 서비스
- **Flask 서버**: OHT API 엔드포인트
- **WinForms UI**: OHT 호출 패널 (권한 제어)
- **MySQL**: OHT 운영 이력 저장

---

## Phase 1: 하드웨어 조립

### 1-1. 천장 레일 설치 (X축)

```
[창고]──────── 레일 (3-5m) ────────[박스1]─[박스2]─[박스3]
   ↑                                  ↑      ↑      ↑
 대기 위치                          정상  부품불량 납땜불량
                                    (1m 간격)
```

**설치 순서**:
1. 천장에 레일 고정 (앵커 볼트 사용)
2. 레일 평행도 확인 (수평계 사용)
3. 타이밍 벨트 및 풀리 장착
4. 스텝모터 고정 및 벨트 장력 조정

### 1-2. Z축 양쪽 스텝모터 설치 ⭐ 신규

**구조**:
```
        [베드 프레임 상단]
           │         │
      GT2 벨트   GT2 벨트
       (감김)     (감김)
           │         │
    ┌──────┴─────────┴──────┐
    │                        │
NEMA 17 (좌)           NEMA 17 (우)
    │                        │
A4988 드라이버         A4988 드라이버
```

**설치 순서**:
1. 베드 프레임 양쪽에 스텝모터 고정
2. GT2 타이밍 벨트 양쪽에 장착
3. 벨트를 베드 상단에 연결
4. 양쪽 벨트 장력 균일하게 조정

### 1-3. 베드 걸쇠 메커니즘 조립 ⭐ 신규

**부품**:
- MG996R 서보모터 × 1
- L자 걸쇠 핀 (직경 8-10mm, 길이 5cm)
- 서보 혼 (horn) 및 고정 나사

**조립 순서**:
1. 베드 하단에 서보모터 고정
2. 서보 혼에 L자 핀 연결
3. 0도 위치에서 핀이 수평이 되도록 캘리브레이션
4. 90도 회전 시 핀이 수직이 되는지 확인

**박스 측면 구조**:
- 박스 양쪽에 구멍 뚫기 (직경 15mm)
- 구멍 높이: 베드가 내려왔을 때 L자 핀이 삽입될 위치

### 1-4. 센서 배치

**X축 리미트 스위치**:
```
[창고 SW]──────[박스3 끝 SW]────
    ↑               ↑
  홈 포지션      안전 한계
```

**Z축 리미트 스위치 (양쪽 4개)** ⭐:
```
[좌측]           [우측]
  ↑                ↑
[상단 SW]       [상단 SW]
  │                │
베드 이동        베드 이동
  │                │
[하단 SW]       [하단 SW]
```

---

## Phase 2: 라즈베리파이 3번 설정

### 2-1. GPIO 핀맵 (BCM 모드)

```python
# raspberry_pi/oht_controller_config.py

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
SERVO_PIN_LATCH = 12   # PWM 제어

# 리미트 스위치 (X축)
LIMIT_SW_WAREHOUSE = 5      # 창고 위치 (홈)
LIMIT_SW_END = 6            # 박스3 끝 (안전 한계)

# 리미트 스위치 (Z축 - 양쪽 4개)
LIMIT_SW_Z_LEFT_UP = 16     # 좌측 상단
LIMIT_SW_Z_LEFT_DOWN = 20   # 좌측 하단
LIMIT_SW_Z_RIGHT_UP = 21    # 우측 상단
LIMIT_SW_Z_RIGHT_DOWN = 19  # 우측 하단

# 긴급 정지 버튼
EMERGENCY_STOP_PIN = 26
```

### 2-2. A4988 드라이버 연결 (3개)

#### X축 스텝모터 + A4988 연결:
```
[라즈베리파이]
├─ GPIO 18 → A4988 #1 STEP
├─ GPIO 23 → A4988 #1 DIR
├─ GPIO 24 → A4988 #1 ENABLE
├─ 5V      → A4988 #1 VDD
└─ GND     → A4988 #1 GND

[12V 전원]
├─ 12V → A4988 #1 VMOT
└─ GND → A4988 #1 GND

[NEMA 17 스텝모터]
├─ A+ (빨강) → A4988 #1 1A
├─ A- (파랑) → A4988 #1 1B
├─ B+ (녹색) → A4988 #1 2A
└─ B- (검정) → A4988 #1 2B
```

#### Z축 좌측/우측 동일 방식으로 연결 (A4988 #2, #3)

### 2-3. 모터 제어 코드

```python
# raspberry_pi/oht_motor_control.py

import pigpio
import time

class StepperMotorA4988:
    """pigpio 기반 A4988 스텝모터 제어"""

    def __init__(self, pi: pigpio.pi, step_pin, dir_pin, enable_pin, step_delay=0.0005):
        self.pi = pi
        self.step_pin = step_pin
        self.dir_pin = dir_pin
        self.enable_pin = enable_pin
        self.step_delay = step_delay

        for pin in (step_pin, dir_pin, enable_pin):
            self.pi.set_mode(pin, pigpio.OUTPUT)

        # 기본 비활성화 (HIGH = disable)
        self.pi.write(self.enable_pin, 1)

    def move_steps(self, steps, direction='CW'):
        """
        스텝 이동 (A4988 STEP 신호)

        Args:
            steps: 이동할 스텝 수
            direction: 'CW' 또는 'CCW'
        """
        cw = (direction == 'CW')
        self.pi.write(self.dir_pin, 1 if cw else 0)
        self.enable()

        for _ in range(steps):
            self.pi.write(self.step_pin, 1)
            time.sleep(self.step_delay)
            self.pi.write(self.step_pin, 0)
            time.sleep(self.step_delay)

    def enable(self):
        """모터 활성화"""
        self.pi.write(self.enable_pin, 0)

    def disable(self):
        """모터 비활성화 (전력 절약)"""
        self.pi.write(self.enable_pin, 1)


class ServoMotor:
    """pigpio 기반 서보모터 제어 (걸쇠)"""

    def __init__(self, pi: pigpio.pi, servo_pin):
        self.pi = pi
        self.servo_pin = servo_pin
        self.pi.set_mode(servo_pin, pigpio.OUTPUT)
        self.lock_angle = 90
        self.unlock_angle = 0

    def set_angle(self, angle):
        """
        서보모터 각도 설정 (0° = 수평, 90° = 잠금)
        """
        # 0° ≈ 500µs, 180° ≈ 2500µs 기준
        pulse = 500 + (angle / 180.0) * 2000
        self.pi.set_servo_pulsewidth(self.servo_pin, pulse)
        time.sleep(0.5)
        self.pi.set_servo_pulsewidth(self.servo_pin, 0)  # 지터 방지

    def lock(self):
        self.set_angle(self.lock_angle)

    def unlock(self):
        self.set_angle(self.unlock_angle)

    def cleanup(self):
        self.pi.set_servo_pulsewidth(self.servo_pin, 0)
```

### 2-4. Z축 양쪽 동기화 제어 ⭐ 핵심 로직

```python
# raspberry_pi/oht_controller.py

STEP_DELAY = 0.0005  # 0.5ms

def lower_bed_synchronized(pi):
    """
    Z축 양쪽 스텝모터 동기화하여 베드 내리기

    - 좌측/우측 모터 동시 구동
    - 리미트 스위치 4개로 수평 유지 확인
    - 한쪽이 먼저 도달하면 해당 쪽만 정지
    """
    # 방향 설정 (둘 다 DOWN)
    pi.write(DIR_PIN_Z_LEFT, 0)
    pi.write(DIR_PIN_Z_RIGHT, 0)

    # 모터 활성화
    pi.write(ENABLE_PIN_Z_LEFT, 0)
    pi.write(ENABLE_PIN_Z_RIGHT, 0)

    logger.info("베드 내리기 시작")

    while True:
        left_down = (pi.read(LIMIT_SW_Z_LEFT_DOWN) == 0)
        right_down = (pi.read(LIMIT_SW_Z_RIGHT_DOWN) == 0)

        if left_down and right_down:
            logger.info("베드 하강 완료 (양쪽 도달)")
            break

        step_left = not left_down
        step_right = not right_down

        if step_left:
            pi.write(STEP_PIN_Z_LEFT, 1)
        if step_right:
            pi.write(STEP_PIN_Z_RIGHT, 1)

        time.sleep(STEP_DELAY)

        if step_left:
            pi.write(STEP_PIN_Z_LEFT, 0)
        if step_right:
            pi.write(STEP_PIN_Z_RIGHT, 0)

        time.sleep(STEP_DELAY)

    if left_down != right_down:
        logger.warning("⚠️ 베드가 기울어져 있을 수 있습니다! (좌/우 리미트 스위치 비동기)")


def raise_bed_synchronized(pi):
    """Z축 양쪽 스텝모터 동기화하여 베드 올리기"""
    pi.write(DIR_PIN_Z_LEFT, 1)
    pi.write(DIR_PIN_Z_RIGHT, 1)

    pi.write(ENABLE_PIN_Z_LEFT, 0)
    pi.write(ENABLE_PIN_Z_RIGHT, 0)

    logger.info("베드 올리기 시작")

    while True:
        left_up = (pi.read(LIMIT_SW_Z_LEFT_UP) == 0)
        right_up = (pi.read(LIMIT_SW_Z_RIGHT_UP) == 0)

        if left_up and right_up:
            logger.info("베드 상승 완료 (양쪽 도달)")
            break

        step_left = not left_up
        step_right = not right_up

        if step_left:
            pi.write(STEP_PIN_Z_LEFT, 1)
        if step_right:
            pi.write(STEP_PIN_Z_RIGHT, 1)

        time.sleep(STEP_DELAY)

        if step_left:
            pi.write(STEP_PIN_Z_LEFT, 0)
        if step_right:
            pi.write(STEP_PIN_Z_RIGHT, 0)

        time.sleep(STEP_DELAY)
```

### 2-5. OHT 제어 메인 로직 (10단계 시퀀스)

```python
# raspberry_pi/oht_controller.py

import pigpio
import requests
import time
import logging
from oht_motor_control import StepperMotorA4988, ServoMotor
from oht_controller_config import *

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

class OHTController:
    """OHT 시스템 메인 컨트롤러"""

    def __init__(self, server_url):
        self.server_url = server_url
        self.current_position = 'WAREHOUSE'

        self.pi = pigpio.pi()
        if not self.pi.connected:
            raise RuntimeError("pigpio 데몬에 연결할 수 없습니다. 'sudo pigpiod' 확인")

        self._configure_inputs()

        self.stepper_x = StepperMotorA4988(self.pi, STEP_PIN_X, DIR_PIN_X, ENABLE_PIN_X)
        self.stepper_z_left = StepperMotorA4988(self.pi, STEP_PIN_Z_LEFT, DIR_PIN_Z_LEFT, ENABLE_PIN_Z_LEFT)
        self.stepper_z_right = StepperMotorA4988(self.pi, STEP_PIN_Z_RIGHT, DIR_PIN_Z_RIGHT, ENABLE_PIN_Z_RIGHT)
        self.servo_latch = ServoMotor(self.pi, SERVO_PIN_LATCH)

        self.emergency_cb = self.pi.callback(
            EMERGENCY_STOP_PIN, pigpio.FALLING_EDGE, self.emergency_stop
        )

        logger.info("OHT Controller 초기화 완료")

    def _configure_inputs(self):
        """리미트 스위치 및 긴급 정지 입력 설정"""
        for pin in [LIMIT_SW_WAREHOUSE, LIMIT_SW_END,
                    LIMIT_SW_Z_LEFT_UP, LIMIT_SW_Z_LEFT_DOWN,
                    LIMIT_SW_Z_RIGHT_UP, LIMIT_SW_Z_RIGHT_DOWN,
                    EMERGENCY_STOP_PIN]:
            self.pi.set_mode(pin, pigpio.INPUT)
            self.pi.set_pull_up_down(pin, pigpio.PUD_UP)

    def execute_request(self, request):
        """OHT 요청 실행 (10단계 시퀀스)"""
        request_id = request['request_id']
        category = request['category']

        logger.info(f"🚀 OHT 작업 시작: {request_id} ({category})")

        try:
            start_time = time.time()

            self._ensure_home_position()
            self._move_to_box(category)

            logger.info("3. 베드 하강 중 (박스 접근)...")
            self.lower_bed_synchronized()

            logger.info("4. 대기 중 (1초)...")
            time.sleep(1)

            logger.info("5. 걸쇠 잠금...")
            self.servo_latch.lock()
            time.sleep(0.5)

            logger.info("6. 베드 상승 중 (박스 픽업)...")
            self.raise_bed_synchronized()

            logger.info("7. 창고로 복귀 중...")
            self._move_to_warehouse()

            logger.info("8. 베드 하강 중 (박스 내려놓기)...")
            self.lower_bed_synchronized()

            logger.info("9. 걸쇠 해제...")
            self.servo_latch.unlock()
            time.sleep(0.5)

            logger.info("10. 베드 상승 (완료)...")
            self.raise_bed_synchronized()

            elapsed_time = time.time() - start_time
            self._report_completion(request_id, success=True)
            logger.info(f"✅ OHT 작업 완료: {request_id} (소요 시간: {elapsed_time:.2f}초)")

        except Exception as e:
            logger.error(f"❌ OHT 작업 실패: {request_id} - {e}")
            self._report_completion(request_id, success=False, error=str(e))
            self.emergency_stop(None, None, None)

    def _ensure_home_position(self):
        """창고 리미트 스위치 기준으로 홈 포지션 정렬"""
        if self.pi.read(LIMIT_SW_WAREHOUSE) == 0:
            return

        logger.info("창고 리미트 스위치 탐색 중...")
        while self.pi.read(LIMIT_SW_WAREHOUSE) == 1:
            self.stepper_x.move_steps(20, 'CCW')
            time.sleep(0.01)

        logger.info("창고 위치 정렬 완료")
        self.current_position = 'WAREHOUSE'

    def _move_to_box(self, category):
        """X축 박스 위치로 이동"""
        BOX_POSITIONS = {
            'NORMAL': 5000,
            'COMPONENT_DEFECT': 10000,
            'SOLDER_DEFECT': 15000
        }
        target_steps = BOX_POSITIONS.get(category, 0)
        logger.info(f"박스로 이동: {category} (스텝: {target_steps})")
        self.stepper_x.move_steps(target_steps, 'CW')
        self.current_position = category

    def _move_to_warehouse(self):
        """X축 창고로 복귀 (홈 포지션)"""
        logger.info("창고로 복귀 중...")
        while self.pi.read(LIMIT_SW_WAREHOUSE) == 1:
            self.stepper_x.move_steps(20, 'CCW')
            time.sleep(0.01)

        self.current_position = 'WAREHOUSE'
        logger.info("창고 도착")

    def lower_bed_synchronized(self):
        lower_bed_synchronized(self.pi)

    def raise_bed_synchronized(self):
        raise_bed_synchronized(self.pi)

    def _report_completion(self, request_id, success, error=None):
        """완료 보고"""
        try:
            payload = {
                'request_id': request_id,
                'success': success,
                'error': error
            }
            requests.post(
                f"{self.server_url}/api/oht/complete",
                json=payload,
                timeout=5
            )
        except Exception as e:
            logger.error(f"완료 보고 실패: {e}")

    def emergency_stop(self, gpio, level, tick):
        """긴급 정지 콜백"""
        logger.warning("🚨 긴급 정지 활성화!")
        self.stepper_x.disable()
        self.stepper_z_left.disable()
        self.stepper_z_right.disable()

    def run(self):
        """메인 루프 (Flask API 폴링)"""
        logger.info("OHT 컨트롤러 시작 (폴링 중...)")
        try:
            while True:
                request = self._check_for_requests()
                if request:
                    self.execute_request(request)
                time.sleep(5)
        except KeyboardInterrupt:
            logger.info("사용자에 의해 중지됨")
        finally:
            self.cleanup()

    def _check_for_requests(self):
        """Flask 서버에 OHT 요청 확인"""
        try:
            response = requests.get(f"{self.server_url}/api/oht/check_pending", timeout=5)
            if response.status_code == 200:
                data = response.json()
                if data.get('has_pending'):
                    return data.get('request')
            return None
        except Exception as e:
            logger.error(f"요청 확인 실패: {e}")
            return None

    def cleanup(self):
        """정리"""
        logger.info("OHT 컨트롤러 종료 중...")
        self.servo_latch.cleanup()
        self.stepper_x.disable()
        self.stepper_z_left.disable()
        self.stepper_z_right.disable()
        if self.emergency_cb:
            self.emergency_cb.cancel()
        self.pi.stop()
        logger.info("pigpio 정리 완료")


if __name__ == "__main__":
    # Flask 서버 URL (Tailscale 또는 로컬)
    SERVER_URL = "http://100.x.x.x:5000"

    controller = OHTController(SERVER_URL)
    controller.run()
```

### 2-6. systemd 서비스 등록

```bash
# /etc/systemd/system/oht-controller.service

[Unit]
Description=OHT Controller Service
After=network.target

[Service]
Type=simple
User=pi
WorkingDirectory=/home/pi/work_project/raspberry_pi
ExecStart=/usr/bin/python3 /home/pi/work_project/raspberry_pi/oht_controller.py
Restart=always
RestartSec=10

[Install]
WantedBy=multi-user.target
```

**서비스 활성화**:
```bash
sudo systemctl daemon-reload
sudo systemctl enable oht-controller.service
sudo systemctl start oht-controller.service
sudo systemctl status oht-controller.service
```

---

## Phase 3: Flask API 구현

### 3-1. OHT API 엔드포인트 (기존 동일)

Flask API는 기존 OHT_System_Setup.md의 Phase 3 참조 (변경 없음)

---

## Phase 4: 부품 리스트 및 비용

### 4-1. 필수 부품

| 부품 | 수량 | 단가 | 총액 |
|------|------|------|------|
| NEMA 17 스텝모터 | 3개 | 8,000원 | 24,000원 |
| A4988 드라이버 | 3개 | 2,000원 | 6,000원 |
| GT2 타이밍 벨트 (5m) | 1개 | 5,000원 | 5,000원 |
| GT2 풀리 20T | 3개 | 1,000원 | 3,000원 |
| 리미트 스위치 | 6개 | 500원 | 3,000원 |
| MG996R 서보모터 | 1개 | 5,000원 | 5,000원 |
| 12V 5A 전원 공급기 | 1개 | 10,000원 | 10,000원 |
| 알루미늄 프로파일 | - | - | 20,000원 |
| 점퍼 와이어, 브레드보드 | - | - | 5,000원 |

**총 예상 비용: 약 81,000원**

---

## Phase 5: 캘리브레이션

### 5-1. X축 박스 위치 스텝 수 측정

```python
# 캘리브레이션 절차

# 1. 홈 포지션으로 이동
controller._move_to_warehouse()

# 2. 각 박스 위치로 수동 이동하며 스텝 수 기록
BOX_POSITIONS = {
    'WAREHOUSE': 0,
    'NORMAL': 5000,           # 박스1까지 스텝 수 (실측)
    'COMPONENT_DEFECT': 10000,  # 박스2까지 스텝 수 (실측)
    'SOLDER_DEFECT': 15000     # 박스3까지 스텝 수 (실측)
}

# 3. oht_controller_config.py에 저장
```

### 5-2. Z축 상하 거리 확인

```python
# 베드 완전히 올라간 상태 → 내려간 상태 스텝 수
# (리미트 스위치로 자동 제어되므로 별도 측정 불필요)
```

### 5-3. 걸쇠 서보모터 각도 조정

```python
# 0도: 수평 (박스 구멍에 삽입 가능)
# 90도: 수직 (걸쇠 잠금)

# 필요 시 미세 조정
LATCH_UNLOCK_ANGLE = 0
LATCH_LOCK_ANGLE = 90
```

---

## Phase 6: 테스트

### 6-1. 개별 컴포넌트 테스트

```bash
# X축 스텝모터 테스트
python3 test_x_axis.py

# Z축 양쪽 동기화 테스트
python3 test_z_axis_sync.py

# 걸쇠 서보모터 테스트
python3 test_latch.py

# 리미트 스위치 테스트
python3 test_limit_switches.py
```

### 6-2. 통합 테스트

```bash
# 전체 시퀀스 (창고 → 박스1 → 픽업 → 창고)
python3 test_full_sequence.py
```

### 6-3. 안정성 테스트

- 연속 작업 100회 (에러 없이 완료)
- 베드 수평 유지 확인 (양쪽 리미트 스위치)
- 타임아웃 시나리오 테스트

---

## 안전 기능

### 긴급 정지

- GPIO 핀 26 버튼 누르면 모든 모터 즉시 정지

### 타임아웃

```python
TIMEOUT_X_AXIS = 30000  # 30초
TIMEOUT_Z_AXIS = 15000  # 15초
TIMEOUT_LATCH = 5000    # 5초
```

### 베드 기울어짐 감지

```python
# 양쪽 리미트 스위치 도달 시간 차이 확인
if left_down != right_down:
    logger.warning("⚠️ 베드가 기울어져 있습니다!")
```

---

## 문제 해결 가이드

### 1. 베드가 기울어지는 경우

**원인**: 양쪽 벨트 장력 불균형

**해결 방법**:
1. 전원 끄기
2. 양쪽 벨트 장력 수동 확인
3. 느슨한 쪽 벨트 조정
4. 재시험

### 2. 걸쇠가 잠기지 않는 경우

**원인**: 박스 구멍 위치 불일치 또는 서보모터 각도 오차

**해결 방법**:
1. 박스 구멍 위치 재확인
2. 서보모터 각도 미세 조정 (85도 또는 95도)
3. L자 핀 길이 확인

### 3. X축 이동 오차 발생

**원인**: 스텝 카운팅 오차 누적

**해결 방법**:
1. 주기적으로 홈 포지션 복귀 (홈잉)
2. 박스 위치 리미트 스위치 추가 (선택)

---

## 참고 문서

- `docs/PCB_Defect_Detection_Project.md`: 전체 시스템 아키텍처
- `docs/Flask_Server_Setup.md`: Flask API 설계
- `docs/RaspberryPi_Setup.md`: 라즈베리파이 GPIO 설정
- `docs/MySQL_Database_Design.md`: 데이터베이스 스키마

---

## 개발 일정 (2주)

### Week 1: 하드웨어 조립
- Day 1-2: 부품 주문 및 수령
- Day 3-4: X축 레일 + 스텝모터 조립
- Day 5-6: Z축 양쪽 스텝모터 + 벨트 조립
- Day 7: 베드 걸쇠 메커니즘 조립

### Week 2: 소프트웨어 및 테스트
- Day 8-9: 라즈베리파이 제어 코드 작성
- Day 10: 캘리브레이션 (스텝 수 측정)
- Day 11: Flask API 연동 테스트
- Day 12: WinForms UI 업데이트
- Day 13: 통합 테스트 (10회 이상)
- Day 14: 문서 작성 및 최종 점검

---

**작성일**: 2025-10-30
**버전**: 2.0 (수평 박스 배치 + 양쪽 스텝모터)
**작성자**: Claude Code
