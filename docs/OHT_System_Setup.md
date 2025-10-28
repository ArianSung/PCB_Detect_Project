# OHT (Overhead Hoist Transport) 시스템 구축 가이드

## 개요

이 가이드는 PCB 분류 박스에서 창고로 PCB를 자동 운반하는 OHT 시스템 구축 방법을 설명합니다. OHT 시스템은 3층 구조의 박스를 사용하여 정상, 부품불량, 납땜불량 PCB를 각각 수거하고 창고로 이동합니다.

**핵심 기능**:
- 3층 박스 시스템 (정상 3층, 부품불량 2층, 납땜불량 1층)
- 천장 레일 기반 X축 이동
- 서보모터 + 벨트 기반 Z축 상하 이동
- 수동 호출 (WinForms, Admin/Operator 권한 필요) 및 자동 호출 (박스 꽉 참)
- 라즈베리파이 3 GPIO 제어

---

## 시스템 구성

### 하드웨어 구성

#### OHT 이동 시스템
- **천장 레일**: 알루미늄 프로파일 (길이: 분류 영역 ~ 창고, 약 3-5m)
- **X축 이동**:
  - 스텝모터: NEMA 17 (1.8° 스텝 각도, 홀딩 토크 40-50 Ncm)
  - 타이밍 벨트: GT2 (폭 6mm, 피치 2mm)
  - 드라이버: A4988 또는 DRV8825
- **Z축 상하 이동**:
  - 서보모터: MG996R (토크 9.4 kgf·cm, 각도 범위 180°)
  - 타이밍 벨트: GT2 (폭 6mm)
  - 층별 높이: 각 층 간격 약 15-20cm

#### OHT 박스 구조
```
┌─────────────────────┐
│   3층 (정상 PCB)     │  ← 최상층 (Z축 위치: 0°)
├─────────────────────┤
│   2층 (부품 불량)    │  ← 중간층 (Z축 위치: 60°)
├─────────────────────┤
│   1층 (납땜 불량)    │  ← 최하층 (Z축 위치: 120°)
└─────────────────────┘

박스 크기: 기존 분류 박스와 동일 (PCB 2개 수납 가능)
재질: 아크릴 또는 경량 플라스틱
무게: 빈 박스 약 500g, PCB 2개 적재 시 약 1kg
```

#### 센서 시스템
- **리미트 스위치** (X축):
  - 창고 위치 감지: 1개
  - 분류 박스 위치 감지: 3개 (정상/부품불량/납땜불량)
- **홀 효과 센서** (Z축):
  - 층별 위치 감지: 3개 (1층/2층/3층)
- **중량 센서** (옵션):
  - 각 층별 PCB 적재 확인

#### 제어 시스템
- **라즈베리파이 3**: OHT 전용 제어기
  - 위치: 로컬 (100.x.x.y, Tailscale)
  - OS: Raspberry Pi OS (64-bit)
  - Python 3.9+
- **전원**:
  - 라즈베리파이: 5V 3A
  - 스텝모터: 12V 2A
  - 서보모터: 5V 2A (별도 전원 권장)

### 소프트웨어 구성

- **라즈베리파이 3**: Python + RPi.GPIO + systemd 서비스
- **Flask 서버**: OHT API 엔드포인트
- **WinForms UI**: OHT 호출 패널 (권한 제어)
- **MySQL**: OHT 운영 이력 저장

---

## Phase 1: 하드웨어 조립

### 1-1. 천장 레일 설치

```
[창고]──────────── 레일 (3-5m) ────────────[분류 영역]
   ↑                                            ↑
 대기 위치                               정상/부품불량/납땜불량
                                           (3개 정지점)
```

**설치 순서**:
1. 천장에 레일 고정 (앵커 볼트 사용)
2. 레일 평행도 확인 (수평계 사용)
3. 타이밍 벨트 및 풀리 장착
4. 스텝모터 고정 및 벨트 장력 조정

### 1-2. OHT 박스 조립

```python
# 박스 레이어 구성
LAYER_CONFIG = {
    'NORMAL': {
        'layer': 3,
        'z_position': 0,      # 서보모터 각도 (0° = 최상층)
        'capacity': 2          # PCB 2개
    },
    'COMPONENT_DEFECT': {
        'layer': 2,
        'z_position': 60,     # 서보모터 각도 (60° = 중간층)
        'capacity': 2
    },
    'SOLDER_DEFECT': {
        'layer': 1,
        'z_position': 120,    # 서보모터 각도 (120° = 최하층)
        'capacity': 2
    }
}
```

### 1-3. 센서 배치

**X축 리미트 스위치 배치**:
```
[창고 SW]──────[정상 SW]─[부품불량 SW]─[납땜불량 SW]──
    ↑              ↑          ↑            ↑
  대기 위치      3층 박스    2층 박스      1층 박스
```

**Z축 홀 효과 센서 배치**:
- 3층 위치: Z축 0° (정상 PCB 적재)
- 2층 위치: Z축 60° (부품불량 적재)
- 1층 위치: Z축 120° (납땜불량 적재)

---

## Phase 2: 라즈베리파이 3 설정

### 2-1. GPIO 핀맵 (BCM 모드)

```python
# raspberry_pi/oht_controller_config.py

# X축 스텝모터 (A4988 드라이버)
STEP_PIN_X = 18        # 스텝 신호
DIR_PIN_X = 23         # 방향 신호
ENABLE_PIN_X = 24      # 활성화 신호

# Z축 서보모터
SERVO_PIN_Z = 12       # PWM 제어 (GPIO 12)

# 리미트 스위치 (X축)
LIMIT_SW_WAREHOUSE = 5      # 창고 위치
LIMIT_SW_NORMAL = 6         # 정상 박스 위치
LIMIT_SW_COMPONENT = 13     # 부품불량 박스 위치
LIMIT_SW_SOLDER = 19        # 납땜불량 박스 위치

# 홀 효과 센서 (Z축)
HALL_SENSOR_LAYER3 = 16     # 3층 (정상)
HALL_SENSOR_LAYER2 = 20     # 2층 (부품불량)
HALL_SENSOR_LAYER1 = 21     # 1층 (납땜불량)

# 긴급 정지 버튼
EMERGENCY_STOP_PIN = 26
```

### 2-2. 모터 제어 코드

```python
# raspberry_pi/oht_motor_control.py

import RPi.GPIO as GPIO
import time

class StepperMotor:
    """X축 스텝모터 제어"""

    def __init__(self, step_pin, dir_pin, enable_pin):
        self.step_pin = step_pin
        self.dir_pin = dir_pin
        self.enable_pin = enable_pin

        GPIO.setup(step_pin, GPIO.OUT)
        GPIO.setup(dir_pin, GPIO.OUT)
        GPIO.setup(enable_pin, GPIO.OUT)

        # 모터 활성화
        GPIO.output(enable_pin, GPIO.LOW)

    def move_steps(self, steps, direction='CW', speed=0.001):
        """
        스텝 이동

        Args:
            steps: 이동할 스텝 수
            direction: 'CW' (시계방향) 또는 'CCW' (반시계방향)
            speed: 스텝 간 딜레이 (초)
        """
        GPIO.output(self.dir_pin, GPIO.HIGH if direction == 'CW' else GPIO.LOW)

        for _ in range(steps):
            GPIO.output(self.step_pin, GPIO.HIGH)
            time.sleep(speed)
            GPIO.output(self.step_pin, GPIO.LOW)
            time.sleep(speed)

    def move_to_position(self, target_position):
        """
        지정 위치로 이동 (리미트 스위치 기반)

        Args:
            target_position: 'WAREHOUSE', 'NORMAL', 'COMPONENT_DEFECT', 'SOLDER_DEFECT'
        """
        # 실제 구현 시 리미트 스위치 읽기 로직 추가
        pass


class ServoMotor:
    """Z축 서보모터 제어"""

    def __init__(self, servo_pin):
        self.servo_pin = servo_pin
        GPIO.setup(servo_pin, GPIO.OUT)
        self.pwm = GPIO.PWM(servo_pin, 50)  # 50Hz
        self.pwm.start(0)

    def set_angle(self, angle):
        """
        서보모터 각도 설정

        Args:
            angle: 0-180도 (0° = 3층, 60° = 2층, 120° = 1층)
        """
        duty_cycle = 2 + (angle / 18)  # 0° = 2%, 180° = 12%
        self.pwm.ChangeDutyCycle(duty_cycle)
        time.sleep(0.5)
        self.pwm.ChangeDutyCycle(0)  # 지터 방지

    def move_to_layer(self, layer):
        """
        지정 층으로 이동

        Args:
            layer: 1, 2, 3
        """
        LAYER_ANGLES = {
            3: 0,    # 정상 (최상층)
            2: 60,   # 부품불량 (중간층)
            1: 120   # 납땜불량 (최하층)
        }

        if layer in LAYER_ANGLES:
            self.set_angle(LAYER_ANGLES[layer])
        else:
            raise ValueError(f"Invalid layer: {layer}")

    def cleanup(self):
        self.pwm.stop()
```

### 2-3. OHT 제어 메인 로직

```python
# raspberry_pi/oht_controller.py

import RPi.GPIO as GPIO
import requests
import time
import logging
from oht_motor_control import StepperMotor, ServoMotor
from oht_controller_config import *

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

class OHTController:
    """OHT 시스템 메인 컨트롤러"""

    def __init__(self, server_url):
        self.server_url = server_url
        self.current_position = 'WAREHOUSE'  # 초기 위치

        # GPIO 초기화
        GPIO.setmode(GPIO.BCM)
        GPIO.setwarnings(False)

        # 모터 초기화
        self.stepper = StepperMotor(STEP_PIN_X, DIR_PIN_X, ENABLE_PIN_X)
        self.servo = ServoMotor(SERVO_PIN_Z)

        # 센서 초기화
        self._setup_sensors()

        # 긴급 정지 버튼
        GPIO.setup(EMERGENCY_STOP_PIN, GPIO.IN, pull_up_down=GPIO.PUD_UP)
        GPIO.add_event_detect(EMERGENCY_STOP_PIN, GPIO.FALLING,
                              callback=self.emergency_stop, bouncetime=300)

        logger.info("OHT Controller initialized")

    def _setup_sensors(self):
        """센서 핀 설정"""
        # 리미트 스위치
        for pin in [LIMIT_SW_WAREHOUSE, LIMIT_SW_NORMAL,
                    LIMIT_SW_COMPONENT, LIMIT_SW_SOLDER]:
            GPIO.setup(pin, GPIO.IN, pull_up_down=GPIO.PUD_UP)

        # 홀 효과 센서
        for pin in [HALL_SENSOR_LAYER3, HALL_SENSOR_LAYER2, HALL_SENSOR_LAYER1]:
            GPIO.setup(pin, GPIO.IN, pull_up_down=GPIO.PUD_DOWN)

    def check_for_requests(self):
        """Flask 서버에 OHT 요청 확인 (폴링)"""
        try:
            response = requests.get(f"{self.server_url}/api/oht/check_pending", timeout=5)
            if response.status_code == 200:
                data = response.json()
                if data.get('has_pending'):
                    return data.get('request')
            return None
        except Exception as e:
            logger.error(f"Failed to check OHT request: {e}")
            return None

    def execute_request(self, request):
        """
        OHT 요청 실행

        Args:
            request: {
                'request_id': 'uuid',
                'category': 'NORMAL' | 'COMPONENT_DEFECT' | 'SOLDER_DEFECT',
                'is_auto': True/False
            }
        """
        category = request['category']
        request_id = request['request_id']

        logger.info(f"Executing OHT request {request_id} for {category}")

        try:
            # 1. 분류 박스로 이동
            self._move_to_box(category)

            # 2. 해당 층으로 박스 하강
            layer = self._get_layer_for_category(category)
            self.servo.move_to_layer(layer)
            time.sleep(2)  # 안정화 대기

            # 3. PCB 적재 (그리퍼 동작 - 실제 구현 필요)
            self._pick_pcbs(category, layer)

            # 4. 박스 상승
            self.servo.move_to_layer(3)  # 최상층으로 복귀
            time.sleep(1)

            # 5. 창고로 복귀
            self._move_to_warehouse()

            # 6. 완료 보고
            self._report_completion(request_id, success=True)

            logger.info(f"OHT request {request_id} completed")

        except Exception as e:
            logger.error(f"OHT request {request_id} failed: {e}")
            self._report_completion(request_id, success=False, error=str(e))

    def _move_to_box(self, category):
        """분류 박스 위치로 이동"""
        POSITION_MAP = {
            'NORMAL': 'NORMAL',
            'COMPONENT_DEFECT': 'COMPONENT_DEFECT',
            'SOLDER_DEFECT': 'SOLDER_DEFECT'
        }

        target = POSITION_MAP[category]
        logger.info(f"Moving from {self.current_position} to {target}")

        # 실제 구현: 리미트 스위치 기반 이동
        # self.stepper.move_to_position(target)

        self.current_position = target

    def _move_to_warehouse(self):
        """창고로 복귀"""
        logger.info("Returning to warehouse")
        # self.stepper.move_to_position('WAREHOUSE')
        self.current_position = 'WAREHOUSE'

    def _get_layer_for_category(self, category):
        """카테고리별 층 번호 반환"""
        LAYER_MAP = {
            'NORMAL': 3,
            'COMPONENT_DEFECT': 2,
            'SOLDER_DEFECT': 1
        }
        return LAYER_MAP[category]

    def _pick_pcbs(self, category, layer):
        """PCB 수거 (그리퍼 동작)"""
        logger.info(f"Picking PCBs from {category} layer {layer}")
        # 실제 구현: 그리퍼 제어 로직 (Arduino 시리얼 통신 등)
        time.sleep(3)  # 적재 시간 시뮬레이션

    def _report_completion(self, request_id, success, error=None):
        """완료 보고"""
        try:
            payload = {
                'request_id': request_id,
                'success': success,
                'error': error
            }
            requests.post(f"{self.server_url}/api/oht/complete",
                         json=payload, timeout=5)
        except Exception as e:
            logger.error(f"Failed to report completion: {e}")

    def emergency_stop(self, channel):
        """긴급 정지"""
        logger.warning("EMERGENCY STOP ACTIVATED!")
        # 모든 모터 정지
        GPIO.output(ENABLE_PIN_X, GPIO.HIGH)  # 스텝모터 비활성화
        self.servo.set_angle(0)  # 서보모터 초기 위치

    def run(self):
        """메인 루프"""
        logger.info("OHT Controller started. Polling for requests...")

        try:
            while True:
                # 요청 확인 (5초마다)
                request = self.check_for_requests()

                if request:
                    self.execute_request(request)

                time.sleep(5)

        except KeyboardInterrupt:
            logger.info("OHT Controller stopped by user")
        finally:
            self.cleanup()

    def cleanup(self):
        """정리"""
        self.servo.cleanup()
        GPIO.cleanup()
        logger.info("GPIO cleaned up")


if __name__ == "__main__":
    # Flask 서버 URL (Tailscale)
    SERVER_URL = "http://100.x.x.x:5000"

    controller = OHTController(SERVER_URL)
    controller.run()
```

### 2-4. systemd 서비스 등록

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

### 3-1. OHT API 엔드포인트 추가

```python
# server/oht_api.py

from flask import Blueprint, request, jsonify
from datetime import datetime
import uuid
import logging

oht_bp = Blueprint('oht', __name__, url_prefix='/api/oht')
logger = logging.getLogger(__name__)

# OHT 요청 큐 (실제 구현 시 Redis 또는 RabbitMQ 사용 권장)
oht_request_queue = []
oht_request_status = {}  # {request_id: {'status': 'pending'|'processing'|'completed', ...}}


@oht_bp.route('/request', methods=['POST'])
def request_oht():
    """
    OHT 호출 요청 (수동)

    요청:
        {
            "category": "NORMAL" | "COMPONENT_DEFECT" | "SOLDER_DEFECT",
            "user_id": "user_uuid",
            "user_role": "Admin" | "Operator"
        }

    응답:
        {
            "status": "ok",
            "request_id": "uuid",
            "message": "OHT request queued"
        }
    """
    try:
        data = request.get_json()
        category = data.get('category')
        user_id = data.get('user_id')
        user_role = data.get('user_role')

        # 권한 검증
        if user_role not in ['Admin', 'Operator']:
            return jsonify({
                'error': 'Insufficient permissions',
                'message': 'Only Admin and Operator can call OHT'
            }), 403

        # 카테고리 검증
        if category not in ['NORMAL', 'COMPONENT_DEFECT', 'SOLDER_DEFECT']:
            return jsonify({'error': 'Invalid category'}), 400

        # 요청 생성
        request_id = str(uuid.uuid4())
        oht_request = {
            'request_id': request_id,
            'category': category,
            'user_id': user_id,
            'user_role': user_role,
            'is_auto': False,
            'timestamp': datetime.now().isoformat()
        }

        # 큐에 추가
        oht_request_queue.append(oht_request)
        oht_request_status[request_id] = {
            'status': 'pending',
            'created_at': datetime.now().isoformat()
        }

        logger.info(f"OHT request {request_id} queued by {user_role} (category: {category})")

        # MySQL에 기록 (실제 구현)
        # db.insert_oht_request(...)

        return jsonify({
            'status': 'ok',
            'request_id': request_id,
            'message': 'OHT request queued'
        }), 200

    except Exception as e:
        logger.error(f"OHT request failed: {e}")
        return jsonify({'error': str(e)}), 500


@oht_bp.route('/check_pending', methods=['GET'])
def check_pending_requests():
    """
    대기 중인 OHT 요청 확인 (라즈베리파이 3 폴링용)

    응답:
        {
            "has_pending": true,
            "request": {...}
        }
    """
    if oht_request_queue:
        request_data = oht_request_queue[0]  # FIFO
        return jsonify({
            'has_pending': True,
            'request': request_data
        }), 200
    else:
        return jsonify({
            'has_pending': False
        }), 200


@oht_bp.route('/complete', methods=['POST'])
def complete_request():
    """
    OHT 요청 완료 보고 (라즈베리파이 3에서 호출)

    요청:
        {
            "request_id": "uuid",
            "success": true,
            "error": null
        }
    """
    try:
        data = request.get_json()
        request_id = data.get('request_id')
        success = data.get('success')
        error = data.get('error')

        # 큐에서 제거
        if oht_request_queue and oht_request_queue[0]['request_id'] == request_id:
            oht_request_queue.pop(0)

        # 상태 업데이트
        if request_id in oht_request_status:
            oht_request_status[request_id]['status'] = 'completed' if success else 'failed'
            oht_request_status[request_id]['completed_at'] = datetime.now().isoformat()
            oht_request_status[request_id]['error'] = error

        logger.info(f"OHT request {request_id} completed (success: {success})")

        # MySQL 업데이트 (실제 구현)
        # db.update_oht_request(request_id, success, error)

        return jsonify({'status': 'ok'}), 200

    except Exception as e:
        logger.error(f"Failed to complete OHT request: {e}")
        return jsonify({'error': str(e)}), 500


@oht_bp.route('/status', methods=['GET'])
def get_oht_status():
    """
    OHT 시스템 상태 조회 (WinForms UI용)

    응답:
        {
            "queue_length": 2,
            "current_request": {...},
            "recent_requests": [...]
        }
    """
    current_request = oht_request_queue[0] if oht_request_queue else None

    return jsonify({
        'queue_length': len(oht_request_queue),
        'current_request': current_request,
        'recent_requests': list(oht_request_status.values())[-10:]  # 최근 10개
    }), 200


@oht_bp.route('/auto_trigger', methods=['POST'])
def auto_trigger():
    """
    자동 OHT 호출 (박스 꽉 찬 경우)

    요청:
        {
            "category": "NORMAL" | "COMPONENT_DEFECT" | "SOLDER_DEFECT",
            "trigger_reason": "box_full"
        }
    """
    try:
        data = request.get_json()
        category = data.get('category')

        # 요청 생성 (자동)
        request_id = str(uuid.uuid4())
        oht_request = {
            'request_id': request_id,
            'category': category,
            'user_id': 'system',
            'user_role': 'System',
            'is_auto': True,
            'trigger_reason': 'box_full',
            'timestamp': datetime.now().isoformat()
        }

        oht_request_queue.append(oht_request)
        oht_request_status[request_id] = {
            'status': 'pending',
            'created_at': datetime.now().isoformat()
        }

        logger.info(f"Auto OHT request {request_id} triggered for {category} (box full)")

        return jsonify({
            'status': 'ok',
            'request_id': request_id
        }), 200

    except Exception as e:
        logger.error(f"Auto OHT trigger failed: {e}")
        return jsonify({'error': str(e)}), 500
```

### 3-2. Flask 서버에 OHT API 등록

```python
# server/app.py (기존 파일 수정)

from flask import Flask
from oht_api import oht_bp

app = Flask(__name__)

# OHT API 블루프린트 등록
app.register_blueprint(oht_bp)

# ... (기존 코드)
```

### 3-3. BoxManager에 자동 호출 로직 추가

```python
# server/box_manager.py (기존 파일 수정)

import requests

class BoxManager:
    def __init__(self):
        self.boxes = {
            'NORMAL': {'slots': [None, None]},
            'COMPONENT_DEFECT': {'slots': [None, None]},
            'SOLDER_DEFECT': {'slots': [None, None]},
            'DISCARD': {'slots': [None]}  # 슬롯 관리 없음
        }

    def update_box_status(self, box_id, slot_index, pcb_id):
        """박스 상태 업데이트 및 자동 OHT 호출"""
        self.boxes[box_id]['slots'][slot_index] = pcb_id

        # 박스 꽉 참 확인 (2/2)
        if box_id != 'DISCARD':
            if all(slot is not None for slot in self.boxes[box_id]['slots']):
                # 자동 OHT 호출
                self._trigger_auto_oht(box_id)

    def _trigger_auto_oht(self, category):
        """자동 OHT 호출"""
        try:
            payload = {
                'category': category,
                'trigger_reason': 'box_full'
            }
            response = requests.post('http://localhost:5000/api/oht/auto_trigger',
                                    json=payload, timeout=5)

            if response.status_code == 200:
                print(f"Auto OHT triggered for {category}")
            else:
                print(f"Failed to trigger auto OHT: {response.status_code}")

        except Exception as e:
            print(f"Auto OHT trigger error: {e}")
```

---

## Phase 4: WinForms UI 구현

### 4-1. OHT 제어 패널 추가

```csharp
// csharp_winforms/.../OHTControlPanel.cs

using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

public partial class OHTControlPanel : UserControl
{
    private readonly HttpClient _httpClient;
    private readonly string _serverUrl;

    public OHTControlPanel(string serverUrl)
    {
        InitializeComponent();
        _serverUrl = serverUrl;
        _httpClient = new HttpClient();

        // 권한 체크 및 UI 초기화
        InitializeUI();
    }

    private void InitializeUI()
    {
        // 권한 체크 (Admin/Operator만 버튼 활성화)
        bool hasPermission = SessionManager.HasPermission(Permission.CallOHT);

        btnCallNormal.Enabled = hasPermission;
        btnCallComponentDefect.Enabled = hasPermission;
        btnCallSolderDefect.Enabled = hasPermission;

        if (!hasPermission)
        {
            lblPermissionWarning.Text = "⚠ OHT 호출 권한이 없습니다 (Admin/Operator 전용)";
            lblPermissionWarning.Visible = true;
        }

        // OHT 상태 자동 갱신 (5초마다)
        Timer statusTimer = new Timer();
        statusTimer.Interval = 5000;
        statusTimer.Tick += async (s, e) => await RefreshOHTStatus();
        statusTimer.Start();
    }

    private async void btnCallNormal_Click(object sender, EventArgs e)
    {
        await CallOHT("NORMAL");
    }

    private async void btnCallComponentDefect_Click(object sender, EventArgs e)
    {
        await CallOHT("COMPONENT_DEFECT");
    }

    private async void btnCallSolderDefect_Click(object sender, EventArgs e)
    {
        await CallOHT("SOLDER_DEFECT");
    }

    private async Task CallOHT(string category)
    {
        try
        {
            var payload = new
            {
                category = category,
                user_id = SessionManager.CurrentUser.UserId,
                user_role = SessionManager.CurrentUser.Role.ToString()
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_serverUrl}/api/oht/request", content);

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                MessageBox.Show("OHT 호출 권한이 없습니다.", "권한 오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();

            MessageBox.Show($"{category} OHT가 호출되었습니다.", "성공",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            await RefreshOHTStatus();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"OHT 호출 실패: {ex.Message}", "오류",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task RefreshOHTStatus()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_serverUrl}/api/oht/status");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var status = JsonConvert.DeserializeObject<OHTStatus>(json);

            lblQueueLength.Text = $"대기 중: {status.QueueLength}";

            if (status.CurrentRequest != null)
            {
                lblCurrentRequest.Text = $"진행 중: {status.CurrentRequest.Category}";
            }
            else
            {
                lblCurrentRequest.Text = "진행 중: 없음";
            }
        }
        catch (Exception ex)
        {
            lblQueueLength.Text = "상태 조회 실패";
        }
    }
}

// Permission enum에 CallOHT 추가
public enum Permission
{
    ViewData,
    ExportData,
    ManageUsers,
    ChangeSettings,
    CallOHT  // 추가
}

// SessionManager에 권한 체크 로직 추가
public static class SessionManager
{
    public static bool HasPermission(Permission permission)
    {
        switch (permission)
        {
            case Permission.CallOHT:
                return CurrentUser.Role == UserRole.Admin ||
                       CurrentUser.Role == UserRole.Operator;
            // ... (기존 권한 로직)
        }
    }
}
```

---

## Phase 5: MySQL 데이터베이스 스키마

### 5-1. oht_operations 테이블

```sql
-- database/schema/oht_operations.sql

CREATE TABLE IF NOT EXISTS oht_operations (
    operation_id VARCHAR(36) PRIMARY KEY,
    category ENUM('NORMAL', 'COMPONENT_DEFECT', 'SOLDER_DEFECT') NOT NULL,
    user_id VARCHAR(36),
    user_role ENUM('Admin', 'Operator', 'System') NOT NULL,
    is_auto BOOLEAN DEFAULT FALSE,
    trigger_reason VARCHAR(50),

    -- 상태
    status ENUM('pending', 'processing', 'completed', 'failed') DEFAULT 'pending',

    -- 타임스탬프
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    started_at DATETIME,
    completed_at DATETIME,

    -- 결과
    pcb_count INT DEFAULT 0,
    success BOOLEAN,
    error_message TEXT,

    INDEX idx_category (category),
    INDEX idx_status (status),
    INDEX idx_created_at (created_at),

    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
```

---

## Phase 6: 운영 시나리오

### 6-1. 수동 호출 시나리오

```
1. WinForms에서 Admin/Operator가 "정상 호출" 버튼 클릭
   ↓
2. SessionManager.HasPermission(Permission.CallOHT) 확인
   ↓
3. Flask API /api/oht/request POST
   ↓
4. 요청 큐에 추가, MySQL에 기록
   ↓
5. 라즈베리파이 3 폴링 (/api/oht/check_pending)
   ↓
6. OHTController.execute_request() 실행
   ↓
7. 창고 → 정상 박스 → 3층 하강 → PCB 적재 → 창고 복귀
   ↓
8. Flask API /api/oht/complete POST
   ↓
9. MySQL 업데이트, WinForms 상태 갱신
```

### 6-2. 자동 호출 시나리오

```
1. 로봇팔이 정상 박스 2번 슬롯에 PCB 적재
   ↓
2. BoxManager.update_box_status() 호출
   ↓
3. 박스 꽉 참 감지 (2/2)
   ↓
4. BoxManager._trigger_auto_oht() 호출
   ↓
5. Flask API /api/oht/auto_trigger POST
   ↓
6. (이후 수동 호출과 동일한 흐름)
```

---

## 테스트 및 검증

### 테스트 항목

1. **하드웨어 테스트**:
   - X축 스텝모터 이동 정확도 (±2mm)
   - Z축 서보모터 층별 위치 정확도 (±5도)
   - 리미트 스위치 감지 신뢰성
   - 홀 효과 센서 층별 감지

2. **소프트웨어 테스트**:
   - Flask API 응답 시간 (< 100ms)
   - 라즈베리파이 폴링 주기 (5초)
   - WinForms 권한 체크 정확도
   - 자동 호출 트리거 정확도

3. **통합 테스트**:
   - 수동 호출 → 창고 → 분류 박스 → 적재 → 복귀 (< 2분)
   - 자동 호출 (박스 꽉 참) → 정상 동작
   - 긴급 정지 버튼 → 즉시 정지

---

## 문제 해결 가이드

### 자주 발생하는 문제

1. **스텝모터가 움직이지 않음**:
   - ENABLE_PIN 상태 확인 (LOW = 활성화)
   - 전원 공급 확인 (12V 2A)
   - 드라이버 방향 핀 확인

2. **서보모터 떨림 (지터)**:
   - PWM duty cycle을 0으로 설정 후 대기
   - 별도 전원 공급 사용
   - 캐패시터 추가 (1000µF)

3. **리미트 스위치 오작동**:
   - 풀업 저항 확인
   - 디바운스 시간 조정 (300ms)
   - 배선 접지 확인

4. **Flask API 타임아웃**:
   - 네트워크 연결 확인 (Tailscale)
   - 방화벽 포트 5000 오픈
   - 폴링 간격 조정 (5초 → 10초)

---

## 참고 문서

- `docs/PCB_Defect_Detection_Project.md`: 전체 시스템 아키텍처
- `docs/Flask_Server_Setup.md`: Flask API 설계
- `docs/RaspberryPi_Setup.md`: 라즈베리파이 GPIO 설정
- `docs/CSharp_WinForms_Design_Specification.md`: WinForms UI 및 권한 시스템
- `docs/MySQL_Database_Design.md`: 데이터베이스 스키마

---

## 추가 개선 사항

1. **성능 최적화**:
   - Redis 큐 사용 (폴링 대신 pub/sub)
   - WebSocket 실시간 통신
   - 스텝모터 가속/감속 프로파일

2. **안전성 향상**:
   - 중복 리미트 스위치 (안전 백업)
   - 모터 전류 모니터링
   - 자동 홈 포지셔닝

3. **UI 개선**:
   - OHT 이동 경로 실시간 애니메이션
   - 박스 적재 현황 3D 시각화
   - 긴급 정지 이력 로그

---

**작성일**: 2025-10-28
**버전**: 1.0
**작성자**: Claude Code
