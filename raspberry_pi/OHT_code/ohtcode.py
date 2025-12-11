import pigpio
import time
import sys

# ==========================================
# 1. 핀 맵핑 & 하드웨어 설정
# ==========================================
X_DIR = 17; X_STEP = 27
Z_DIR = 22; Z_STEP = 23
SERVO_PIN = 18
LIMIT_PIN = 5

# ==========================================
# 2. 설정값
# ==========================================
X_CW = 1; X_CCW = 0
Z_UP = 1; Z_DOWN = 0

LIFT_STEPS = 925
X_SPEED_DELAY = 0.04

Z_TARGET_SPEED = 125
Z_MIN_SPEED = 30
Z_ACCEL_STEPS = 50

ANGLE_OPEN = 500
ANGLE_LOCK = 1500

pi = pigpio.pi()

if not pi.connected:
    exit()

def setup():
    for pin in [X_DIR, X_STEP, Z_DIR, Z_STEP]:
        pi.set_mode(pin, pigpio.OUTPUT)

    # 리미트 스위치 (풀다운: 평소 0, 눌리면 1)
    pi.set_mode(LIMIT_PIN, pigpio.INPUT)
    pi.set_pull_up_down(LIMIT_PIN, pigpio.PUD_DOWN)

    pi.write(X_STEP, 0); pi.write(Z_STEP, 0)
    pi.set_servo_pulsewidth(SERVO_PIN, ANGLE_OPEN)
    time.sleep(0.5)

# ==========================================
# 3. 단위 동작 함수
# ==========================================
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

    # 가속
    for i in range(accel_dist):
        pi.write(Z_STEP, 1); time.sleep(0.000005); pi.write(Z_STEP, 0)
        current_delay = current_delay - ((current_delay - min_delay) / (accel_dist - i + 1))
        time.sleep(current_delay)
    
    # 등속
    for _ in range(const_dist):
        pi.write(Z_STEP, 1); time.sleep(0.000005); pi.write(Z_STEP, 0)
        time.sleep(min_delay)
    
    # 감속
    for i in range(accel_dist):
        pi.write(Z_STEP, 1); time.sleep(0.000005); pi.write(Z_STEP, 0)
        current_delay = current_delay + ((1.0/Z_MIN_SPEED - min_delay) / (accel_dist - i + 1))
        time.sleep(current_delay)

# ==========================================
# [핵심] 스마트 타겟팅 이동 함수 (엣지 검출)
# ==========================================
def move_x_to_target(target_count):
    pi.write(X_DIR, X_CW) # 오른쪽 이동
    print(f" [이동] 목표: {target_count}번째 위치로 출발!")

    current_count = 0
    last_state = pi.read(LIMIT_PIN) # 초기 상태 읽기

    while True:
        # 1. 모터 계속 회전 (멈추지 않음)
        pi.write(X_STEP, 1)
        time.sleep(0.00001)
        pi.write(X_STEP, 0)
        time.sleep(X_SPEED_DELAY)

        # 2. 현재 스위치 상태 확인
        current_state = pi.read(LIMIT_PIN)

        # 3. 엣지 검출 (0 -> 1 : 눌리는 순간)
        if last_state == 0 and current_state == 1:
            current_count += 1
            print(f" 🔔 딸! (현재 위치: {current_count}번)")

            # [중요] 여기가 목표인지 확인 (눌렸을 때만 체크)
            if current_count == target_count:
                print(f" 🛑 목표({target_count}번) 도착! 정지합니다.")
                break
            else:
                print(f" -> 목표 아님 ({target_count}번 아님). 계속 갑니다.")

        # 4. 상태 업데이트
        last_state = current_state

    time.sleep(0.5)

# ==========================================
# 4. 메인 실행 (Flask 명령 시뮬레이션)
# ==========================================
try:
    setup()

    # ----------------------------------------------------
    FLASK_COMMAND = 3
    
    print(f"\n=== OHT 스마트 타겟팅 테스트 (목표: {FLASK_COMMAND}) ===")
    time.sleep(2)

    # 1. 초기화
    print("\n1️⃣ [초기화]")
    control_servo("OPEN")
    move_z(Z_UP, LIFT_STEPS)
    time.sleep(1)

    # 2. X축 이동 (엣지 검출 & 타겟팅)
    print(f"\n2️⃣ [이동] {FLASK_COMMAND}번 위치까지 이동")
    move_x_to_target(FLASK_COMMAND)
    
    time.sleep(1)

    # 3. 픽업
    print("\n3️⃣ [픽업]")
    move_z(Z_DOWN, LIFT_STEPS)
    control_servo("LOCK")
    move_z(Z_UP, LIFT_STEPS)
    time.sleep(2)

    # 4. 하차 (제자리)
    print("\n4️⃣ [하차]")
    move_z(Z_DOWN, LIFT_STEPS)
    control_servo("OPEN")
    move_z(Z_UP, LIFT_STEPS)

    print("\n✨ 종료.")

except KeyboardInterrupt:
    print("\n>> 비상 정지!")
finally:
    pi.write(X_STEP, 0); pi.write(Z_STEP, 0)
    pi.set_servo_pulsewidth(SERVO_PIN, 0)
    pi.stop()