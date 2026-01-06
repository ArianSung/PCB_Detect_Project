import pigpio
import time
import sys
import requests

# 1. 핀 맵핑 및 하드웨어 설정
X_DIR = 17; X_STEP = 27
Z_DIR = 22; Z_STEP = 23
SERVO_PIN = 18
LIMIT_PIN = 5

# 2. 동작 설정값
X_CW = 1; X_CCW = 0  # CW: 오른쪽(슬롯 방향), CCW: 왼쪽(창고 방향)
Z_UP = 1; Z_DOWN = 0

LIFT_STEPS = 925
X_SPEED_DELAY = 0.04

Z_TARGET_SPEED = 125
Z_MIN_SPEED = 30
Z_ACCEL_STEPS = 50

ANGLE_OPEN = 500
ANGLE_LOCK = 1500

# 서버 설정
FLASK_IP = "http://100.80.24.53:5000"
URL_GET_COMMAND = f"{FLASK_IP}/get_oht_command"
URL_UPDATE_STATUS = f"{FLASK_IP}/update_oht_status" # 상태 보고용 엔드포인트

pi = pigpio.pi()

if not pi.connected:
    print("pigpio daemon 연결 실패")
    sys.exit()

def setup():
    for pin in [X_DIR, X_STEP, Z_DIR, Z_STEP]:
        pi.set_mode(pin, pigpio.OUTPUT)
    pi.set_mode(LIMIT_PIN, pi.INPUT)
    pi.set_pull_up_down(LIMIT_PIN, pigpio.PUD_DOWN)
    pi.write(X_STEP, 0); pi.write(Z_STEP, 0)
    pi.set_servo_pulsewidth(SERVO_PIN, ANGLE_OPEN)
    time.sleep(0.5)

# 서버에 현재 상태를 보고하는 함수
def report_status(status, slot=None):
    try:
        data = {"status": status, "slot": slot}
        response = requests.post(URL_UPDATE_STATUS, json=data, timeout=2)
        if response.status_code == 200:
            print(f"[상태 보고 완료] {status} (Slot: {slot})")
    except Exception as e:
        print(f"[상태 보고 실패] {e}")

# 3. 단위 동작 함수
def control_servo(action):
    if action == "LOCK":
        pi.set_servo_pulsewidth(SERVO_PIN, ANGLE_LOCK)
        time.sleep(1)
    elif action == "OPEN":
        pi.set_servo_pulsewidth(SERVO_PIN, ANGLE_OPEN)
        time.sleep(1)
        pi.set_servo_pulsewidth(SERVO_PIN, 0)

def move_z(direction, total_steps):
    pi.write(Z_DIR, direction)
    accel_dist = min(Z_ACCEL_STEPS, total_steps // 2)
    const_dist = total_steps - (2 * accel_dist)
    current_delay = 1.0 / Z_MIN_SPEED
    min_delay = 1.0 / Z_TARGET_SPEED

    for i in range(accel_dist):
        pi.write(Z_STEP, 1); time.sleep(0.000005); pi.write(Z_STEP, 0)
        current_delay = current_delay - ((current_delay - min_delay) / (accel_dist - i + 1))
        time.sleep(current_delay)
    for _ in range(const_dist):
        pi.write(Z_STEP, 1); time.sleep(0.000005); pi.write(Z_STEP, 0)
        time.sleep(min_delay)
    for i in range(accel_dist):
        pi.write(Z_STEP, 1); time.sleep(0.000005); pi.write(Z_STEP, 0)
        current_delay = current_delay + ((1.0/Z_MIN_SPEED - min_delay) / (accel_dist - i + 1))
        time.sleep(current_delay)

# 목표 슬롯으로 이동 (오른쪽 방향)
def move_x_to_target(target_count):
    pi.write(X_DIR, X_CW)
    current_count = 0
    last_state = pi.read(LIMIT_PIN)

    while True:
        pi.write(X_STEP, 1); time.sleep(0.00001); pi.write(X_STEP, 0)
        time.sleep(X_SPEED_DELAY)
        current_state = pi.read(LIMIT_PIN)
        if last_state == 0 and current_state == 1:
            current_count += 1
            if current_count == target_count:
                break
        last_state = current_state
    time.sleep(0.5)

# 창고(홈) 위치로 이동 (왼쪽 방향)
def move_x_to_home(current_slot):
    pi.write(X_DIR, X_CCW)
    print(f"창고로 복귀 시작 (현재 위치: {current_slot})")
    
    passed_count = 0
    last_state = pi.read(LIMIT_PIN)

    while passed_count < current_slot:
        pi.write(X_STEP, 1); time.sleep(0.00001); pi.write(X_STEP, 0)
        time.sleep(X_SPEED_DELAY)
        current_state = pi.read(LIMIT_PIN)
        if last_state == 0 and current_state == 1:
            passed_count += 1
        last_state = current_state
    time.sleep(0.5)

# 4. 메인 시퀀스
try:
    setup()
    is_working = False

    print(f"OHT 시스템 가동 (Server: {FLASK_IP})")
    
    while True:
        if not is_working:
            try:
                response = requests.get(URL_GET_COMMAND, timeout=2)
                if response.status_code == 200:
                    data = response.json()
                    if data.get('command') == 'START':
                        target_slot = data.get('slot')
                        is_working = True
                        
                        # [시작 보고]
                        report_status("MOVING_TO_SLOT", target_slot)

                        # 1. 목표 슬롯으로 이동
                        move_x_to_target(target_slot)
                        report_status("ARRIVED_AT_SLOT", target_slot)

                        # 2. 적재(Pick-up) 작업
                        move_z(Z_DOWN, LIFT_STEPS)
                        control_servo("LOCK")
                        move_z(Z_UP, LIFT_STEPS)
                        report_status("LOAD_COMPLETE", target_slot)

                        # 3. 창고로 이동 및 도착 보고
                        report_status("MOVING_TO_WAREHOUSE")
                        move_x_to_home(target_slot)
                        report_status("ARRIVED_AT_WAREHOUSE")

                        is_working = False
                        print("전체 공정 완료. 대기 모드.")
                
            except Exception as e:
                time.sleep(1)
        
        time.sleep(0.5)

except KeyboardInterrupt:
    print("\n종료")
finally:
    pi.write(X_STEP, 0); pi.write(Z_STEP, 0)
    pi.set_servo_pulsewidth(SERVO_PIN, 0)
    pi.stop()
