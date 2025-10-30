#!/usr/bin/env python3
"""
OHT (Overhead Hoist Transport) Controller
라즈베리파이 3번(Raspberry Pi 4 Model B) 기반 OHT 제어 시스템

기능:
- Flask API 폴링 (5초마다)
- 3개 스텝모터 제어 (X축 1개, Z축 좌/우 2개)
- 서보모터 걸쇠 제어
- 리미트 스위치 6개 모니터링
- 10단계 OHT 시퀀스 실행
- 긴급 정지 버튼
"""

import pigpio
import time
import requests
import logging
from datetime import datetime
from enum import Enum

# 로깅 설정
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
    handlers=[
        logging.FileHandler('/home/pi/oht_controller.log'),
        logging.StreamHandler()
    ]
)
logger = logging.getLogger(__name__)

# GPIO 핀 매핑 (BCM 모드)
# X축 스텝모터 (A4988)
STEP_PIN_X = 18
DIR_PIN_X = 23
ENABLE_PIN_X = 24

# Z축 좌측 스텝모터 (A4988)
STEP_PIN_Z_LEFT = 17
DIR_PIN_Z_LEFT = 27
ENABLE_PIN_Z_LEFT = 22

# Z축 우측 스텝모터 (A4988)
STEP_PIN_Z_RIGHT = 25
DIR_PIN_Z_RIGHT = 8
ENABLE_PIN_Z_RIGHT = 7

# 서보모터 (MG996R)
SERVO_PIN_LATCH = 12

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

# 스텝모터 설정
STEPS_PER_REV = 200  # NEMA 17 기본 (1.8도)
MICROSTEPS = 16      # A4988 마이크로스테핑
STEPS_PER_MM = 10    # 타이밍 벨트 기준 (조정 필요)

# 박스 위치 (스텝 수)
BOX_POSITIONS = {
    'WAREHOUSE': 0,
    'NORMAL': 2000,           # 약 200mm
    'COMPONENT_DEFECT': 4000, # 약 400mm
    'SOLDER_DEFECT': 6000     # 약 600mm
}

# Flask 서버 URL (환경변수 또는 설정 파일에서 읽을 수 있음)
FLASK_SERVER_URL = "http://100.64.1.1:5000"

# 타임아웃 설정
OPERATION_TIMEOUT = 60  # 각 동작 최대 60초


class Direction(Enum):
    """스텝모터 방향"""
    CW = 1   # 시계방향
    CCW = 0  # 반시계방향


class StepperMotorPigpio:
    """
    pigpio 기반 스텝모터 제어 클래스 (A4988 드라이버)

    Args:
        pi: pigpio 인스턴스
        step_pin: 스텝 신호 핀
        dir_pin: 방향 신호 핀
        enable_pin: 활성화 신호 핀
    """

    def __init__(self, pi, step_pin, dir_pin, enable_pin):
        self.pi = pi
        self.step_pin = step_pin
        self.dir_pin = dir_pin
        self.enable_pin = enable_pin

        # 핀 모드 설정
        self.pi.set_mode(step_pin, pigpio.OUTPUT)
        self.pi.set_mode(dir_pin, pigpio.OUTPUT)
        self.pi.set_mode(enable_pin, pigpio.OUTPUT)

        # 모터 활성화 (LOW = 활성화)
        self.pi.write(enable_pin, 0)

        logger.debug(f"StepperMotor 초기화: STEP={step_pin}, DIR={dir_pin}, ENABLE={enable_pin}")

    def move_steps(self, steps, direction=Direction.CW, speed=0.0005):
        """
        스텝 이동

        Args:
            steps: 이동할 스텝 수
            direction: 방향 (Direction.CW 또는 Direction.CCW)
            speed: 스텝 간 딜레이 (초, 기본 0.5ms)
        """
        self.pi.write(self.dir_pin, direction.value)

        for _ in range(steps):
            self.pi.write(self.step_pin, 1)
            time.sleep(speed)
            self.pi.write(self.step_pin, 0)
            time.sleep(speed)

    def enable(self):
        """모터 활성화"""
        self.pi.write(self.enable_pin, 0)

    def disable(self):
        """모터 비활성화 (전력 절약)"""
        self.pi.write(self.enable_pin, 1)


class ServoMotorPigpio:
    """
    pigpio 기반 서보모터 제어 클래스

    Args:
        pi: pigpio 인스턴스
        servo_pin: 서보 제어 핀 (PWM)
    """

    def __init__(self, pi, servo_pin):
        self.pi = pi
        self.servo_pin = servo_pin

        # 핀 모드 설정
        self.pi.set_mode(servo_pin, pigpio.OUTPUT)

        logger.debug(f"ServoMotor 초기화: PIN={servo_pin}")

    def set_angle(self, angle):
        """
        서보모터 각도 설정

        Args:
            angle: 각도 (0-180도)
        """
        # 각도를 펄스 폭으로 변환 (500us ~ 2500us)
        # 0도 = 500us, 90도 = 1500us, 180도 = 2500us
        pulsewidth = int(500 + (angle / 180.0) * 2000)
        self.pi.set_servo_pulsewidth(self.servo_pin, pulsewidth)
        logger.debug(f"서보모터 각도 설정: {angle}도 (펄스폭: {pulsewidth}us)")

    def stop(self):
        """서보모터 PWM 정지"""
        self.pi.set_servo_pulsewidth(self.servo_pin, 0)


class OHTController:
    """
    OHT 메인 제어기

    Flask API를 폴링하여 OHT 요청을 처리하고,
    스텝모터와 서보모터를 제어하여 박스를 운반합니다.
    """

    def __init__(self, flask_url=FLASK_SERVER_URL):
        self.flask_url = flask_url
        self.pi = pigpio.pi()

        if not self.pi.connected:
            raise RuntimeError("pigpio 데몬에 연결할 수 없습니다. 'sudo pigpiod' 실행 확인")

        logger.info("pigpio 연결 성공")

        # 현재 위치
        self.current_position = 'WAREHOUSE'
        self.current_x_steps = 0

        # 긴급 정지 플래그
        self.emergency_stop_flag = False

        # 모터 초기화
        self.stepper_x = StepperMotorPigpio(
            self.pi, STEP_PIN_X, DIR_PIN_X, ENABLE_PIN_X
        )
        self.stepper_z_left = StepperMotorPigpio(
            self.pi, STEP_PIN_Z_LEFT, DIR_PIN_Z_LEFT, ENABLE_PIN_Z_LEFT
        )
        self.stepper_z_right = StepperMotorPigpio(
            self.pi, STEP_PIN_Z_RIGHT, DIR_PIN_Z_RIGHT, ENABLE_PIN_Z_RIGHT
        )
        self.servo_latch = ServoMotorPigpio(self.pi, SERVO_PIN_LATCH)

        # 센서 초기화
        self._setup_sensors()

        # 긴급 정지 버튼 초기화
        self.pi.set_mode(EMERGENCY_STOP_PIN, pigpio.INPUT)
        self.pi.set_pull_up_down(EMERGENCY_STOP_PIN, pigpio.PUD_UP)
        self.pi.callback(EMERGENCY_STOP_PIN, pigpio.FALLING_EDGE, self._emergency_stop_callback)

        logger.info("OHTController 초기화 완료")

    def _setup_sensors(self):
        """센서 핀 설정"""
        # X축 리미트 스위치
        self.pi.set_mode(LIMIT_SW_WAREHOUSE, pigpio.INPUT)
        self.pi.set_pull_up_down(LIMIT_SW_WAREHOUSE, pigpio.PUD_UP)

        self.pi.set_mode(LIMIT_SW_END, pigpio.INPUT)
        self.pi.set_pull_up_down(LIMIT_SW_END, pigpio.PUD_UP)

        # Z축 리미트 스위치 (4개)
        for pin in [LIMIT_SW_Z_LEFT_UP, LIMIT_SW_Z_LEFT_DOWN,
                    LIMIT_SW_Z_RIGHT_UP, LIMIT_SW_Z_RIGHT_DOWN]:
            self.pi.set_mode(pin, pigpio.INPUT)
            self.pi.set_pull_up_down(pin, pigpio.PUD_UP)

        logger.info("센서 초기화 완료")

    def _emergency_stop_callback(self, gpio, level, tick):
        """긴급 정지 콜백"""
        logger.critical("⚠️ 긴급 정지 버튼 눌림!")
        self.emergency_stop_flag = True
        self._stop_all_motors()

    def _stop_all_motors(self):
        """모든 모터 정지"""
        self.stepper_x.disable()
        self.stepper_z_left.disable()
        self.stepper_z_right.disable()
        self.servo_latch.stop()
        logger.warning("모든 모터 정지")

    def _check_emergency_stop(self):
        """긴급 정지 체크"""
        if self.emergency_stop_flag:
            raise RuntimeError("긴급 정지가 활성화되어 있습니다")

    def poll_flask_api(self):
        """
        Flask API 폴링하여 OHT 요청 확인

        Returns:
            dict: OHT 요청 정보 또는 None
        """
        try:
            response = requests.get(
                f"{self.flask_url}/api/oht/check_queue",
                timeout=5
            )

            if response.status_code == 200:
                data = response.json()
                if data.get('has_request'):
                    logger.info(f"OHT 요청 수신: {data['request']}")
                    return data['request']

            return None

        except requests.exceptions.RequestException as e:
            logger.error(f"Flask API 폴링 오류: {e}")
            return None

    def execute_sequence(self, category):
        """
        10단계 OHT 시퀀스 실행

        Args:
            category: 박스 카테고리 ('NORMAL', 'COMPONENT_DEFECT', 'SOLDER_DEFECT')

        Returns:
            bool: 성공 여부
        """
        logger.info(f"========== OHT 시퀀스 시작: {category} ==========")
        start_time = time.time()

        try:
            # 1단계: X축 박스로 이동
            logger.info("[1/10] X축 박스로 이동")
            self._check_emergency_stop()
            self.move_to_box(category)

            # 2단계: Z축 베드 하강 (동기화)
            logger.info("[2/10] Z축 베드 하강")
            self._check_emergency_stop()
            self.lower_bed_synchronized()

            # 3단계: 걸쇠 수평 (0도)
            logger.info("[3/10] 걸쇠 수평 위치")
            self._check_emergency_stop()
            self.servo_latch.set_angle(0)
            time.sleep(0.5)

            # 4단계: 대기 (안정화)
            logger.info("[4/10] 대기 (1초)")
            time.sleep(1.0)

            # 5단계: 걸쇠 회전 잠금 (90도)
            logger.info("[5/10] 걸쇠 회전 잠금")
            self._check_emergency_stop()
            self.servo_latch.set_angle(90)
            time.sleep(0.5)

            # 6단계: Z축 베드 상승 (박스 픽업)
            logger.info("[6/10] Z축 베드 상승 (박스 픽업)")
            self._check_emergency_stop()
            self.raise_bed_synchronized()

            # 7단계: X축 창고로 복귀
            logger.info("[7/10] X축 창고로 복귀")
            self._check_emergency_stop()
            self.move_to_warehouse()

            # 8단계: Z축 베드 하강 (박스 배치)
            logger.info("[8/10] Z축 베드 하강 (박스 배치)")
            self._check_emergency_stop()
            self.lower_bed_synchronized()

            # 9단계: 걸쇠 해제 (0도)
            logger.info("[9/10] 걸쇠 해제")
            self._check_emergency_stop()
            self.servo_latch.set_angle(0)
            time.sleep(0.5)

            # 10단계: Z축 베드 상승 (완료)
            logger.info("[10/10] Z축 베드 상승 (완료)")
            self._check_emergency_stop()
            self.raise_bed_synchronized()

            elapsed_time = time.time() - start_time
            logger.info(f"========== OHT 시퀀스 완료: {elapsed_time:.2f}초 ==========")
            return True

        except Exception as e:
            logger.error(f"OHT 시퀀스 오류: {e}")
            self._stop_all_motors()
            return False

    def move_to_box(self, category):
        """
        X축으로 박스 위치로 이동

        Args:
            category: 박스 카테고리
        """
        target_steps = BOX_POSITIONS.get(category, 0)
        current_steps = self.current_x_steps

        steps_to_move = abs(target_steps - current_steps)
        direction = Direction.CW if target_steps > current_steps else Direction.CCW

        logger.info(f"X축 이동: {self.current_position} → {category} ({steps_to_move} 스텝)")

        self.stepper_x.enable()
        self.stepper_x.move_steps(steps_to_move, direction, speed=0.001)

        self.current_x_steps = target_steps
        self.current_position = category

    def move_to_warehouse(self):
        """X축 창고로 복귀 (리미트 스위치 사용)"""
        logger.info("X축 창고로 복귀")

        self.stepper_x.enable()

        # 창고 리미트 스위치까지 이동
        timeout_start = time.time()
        while not self.pi.read(LIMIT_SW_WAREHOUSE):
            self._check_emergency_stop()

            if time.time() - timeout_start > OPERATION_TIMEOUT:
                raise TimeoutError("창고 복귀 타임아웃")

            self.stepper_x.move_steps(10, Direction.CCW, speed=0.001)

        logger.info("창고 도착")
        self.current_position = 'WAREHOUSE'
        self.current_x_steps = 0

    def lower_bed_synchronized(self):
        """Z축 양쪽 스텝모터 동기화하여 베드 내리기"""
        logger.info("Z축 베드 내리기 (동기화)")

        self.stepper_z_left.enable()
        self.stepper_z_right.enable()

        # 방향 설정 (둘 다 DOWN)
        self.pi.write(DIR_PIN_Z_LEFT, 0)
        self.pi.write(DIR_PIN_Z_RIGHT, 0)

        timeout_start = time.time()

        while True:
            self._check_emergency_stop()

            if time.time() - timeout_start > OPERATION_TIMEOUT:
                raise TimeoutError("베드 하강 타임아웃")

            # 리미트 스위치 상태 확인
            left_down = self.pi.read(LIMIT_SW_Z_LEFT_DOWN)
            right_down = self.pi.read(LIMIT_SW_Z_RIGHT_DOWN)

            # 둘 다 도달하면 정지
            if left_down and right_down:
                logger.info("베드 하강 완료 (양쪽 모두 하단 리미트 스위치 도달)")
                break

            # 아직 도달 안 한 쪽만 계속 이동
            if not left_down:
                self.pi.write(STEP_PIN_Z_LEFT, 1)
            if not right_down:
                self.pi.write(STEP_PIN_Z_RIGHT, 1)

            time.sleep(0.0005)  # 0.5ms

            self.pi.write(STEP_PIN_Z_LEFT, 0)
            self.pi.write(STEP_PIN_Z_RIGHT, 0)

            time.sleep(0.0005)

    def raise_bed_synchronized(self):
        """Z축 양쪽 스텝모터 동기화하여 베드 올리기"""
        logger.info("Z축 베드 올리기 (동기화)")

        self.stepper_z_left.enable()
        self.stepper_z_right.enable()

        # 방향 설정 (둘 다 UP)
        self.pi.write(DIR_PIN_Z_LEFT, 1)
        self.pi.write(DIR_PIN_Z_RIGHT, 1)

        timeout_start = time.time()

        while True:
            self._check_emergency_stop()

            if time.time() - timeout_start > OPERATION_TIMEOUT:
                raise TimeoutError("베드 상승 타임아웃")

            left_up = self.pi.read(LIMIT_SW_Z_LEFT_UP)
            right_up = self.pi.read(LIMIT_SW_Z_RIGHT_UP)

            if left_up and right_up:
                logger.info("베드 상승 완료 (양쪽 모두 상단 리미트 스위치 도달)")
                break

            if not left_up:
                self.pi.write(STEP_PIN_Z_LEFT, 1)
            if not right_up:
                self.pi.write(STEP_PIN_Z_RIGHT, 1)

            time.sleep(0.0005)

            self.pi.write(STEP_PIN_Z_LEFT, 0)
            self.pi.write(STEP_PIN_Z_RIGHT, 0)

            time.sleep(0.0005)

    def run(self, poll_interval=5):
        """
        메인 실행 루프

        Args:
            poll_interval: API 폴링 간격 (초)
        """
        logger.info(f"OHT Controller 시작 (폴링 간격: {poll_interval}초)")

        try:
            while True:
                if self.emergency_stop_flag:
                    logger.warning("긴급 정지 상태 - 대기 중")
                    time.sleep(poll_interval)
                    continue

                # Flask API 폴링
                request = self.poll_flask_api()

                if request:
                    category = request.get('category')
                    request_id = request.get('request_id')

                    logger.info(f"OHT 요청 처리 시작: {request_id} - {category}")

                    # OHT 시퀀스 실행
                    success = self.execute_sequence(category)

                    # Flask에 결과 보고
                    self._report_completion(request_id, success)

                # 대기
                time.sleep(poll_interval)

        except KeyboardInterrupt:
            logger.info("사용자 종료 (Ctrl+C)")

        finally:
            self.cleanup()

    def _report_completion(self, request_id, success):
        """
        Flask API에 OHT 완료 보고

        Args:
            request_id: 요청 ID
            success: 성공 여부
        """
        try:
            response = requests.post(
                f"{self.flask_url}/api/oht/complete",
                json={
                    'request_id': request_id,
                    'status': 'completed' if success else 'failed',
                    'timestamp': datetime.now().isoformat()
                },
                timeout=5
            )

            if response.status_code == 200:
                logger.info(f"OHT 완료 보고 성공: {request_id}")
            else:
                logger.error(f"OHT 완료 보고 실패: {response.status_code}")

        except requests.exceptions.RequestException as e:
            logger.error(f"OHT 완료 보고 오류: {e}")

    def cleanup(self):
        """정리"""
        logger.info("OHT Controller 종료 중...")
        self._stop_all_motors()
        self.servo_latch.stop()
        self.pi.stop()
        logger.info("GPIO 정리 완료")


def main():
    """메인 함수"""
    # Flask 서버 URL (환경변수에서 읽거나 하드코딩)
    import os
    flask_url = os.getenv('FLASK_SERVER_URL', FLASK_SERVER_URL)

    try:
        controller = OHTController(flask_url)
        controller.run(poll_interval=5)

    except Exception as e:
        logger.critical(f"치명적 오류: {e}")
        raise


if __name__ == '__main__':
    main()
