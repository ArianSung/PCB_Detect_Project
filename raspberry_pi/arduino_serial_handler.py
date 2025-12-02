#!/usr/bin/env python3
"""
Arduino 시리얼 통신 핸들러

Flask 서버로부터 받은 PCB 검사 결과를 아두이노에 JSON 프로토콜로 전송하여
로봇팔을 제어하고 PCB를 해당 카테고리 박스에 배치합니다.

통신 프로토콜:
- Baudrate: 115200
- Format: JSON (UTF-8)
- Timeout: 5초

사용 예시:
    handler = ArduinoSerialHandler('/dev/ttyACM0')
    handler.send_classification_result('NORMAL', slot_index=0, confidence=0.95)
"""

import serial
import json
import logging
import time
from typing import Optional, Dict, Any

# 로깅 설정
logging.basicConfig(
    level=logging.INFO,
    format='[%(asctime)s] [%(levelname)s] [Arduino-Serial] - %(message)s'
)
logger = logging.getLogger(__name__)


class ArduinoSerialHandler:
    """아두이노 시리얼 통신 핸들러 클래스"""

    # 판정 결과 → 아두이노 카테고리 매핑
    CATEGORY_MAP = {
        '정상': 'NORMAL',
        '부품불량': 'COMPONENT_DEFECT',
        '납땜불량': 'SOLDER_DEFECT',
        '위치오류': 'SOLDER_DEFECT',  # 위치오류도 납땜불량 박스로
        '폐기': 'DISCARD',
        # 영문 지원 (API Contract 기준)
        'normal': 'NORMAL',
        'missing': 'COMPONENT_DEFECT',
        'position_error': 'SOLDER_DEFECT',
        'discard': 'DISCARD'
    }

    def __init__(
        self,
        port: str = '/dev/ttyACM0',
        baudrate: int = 115200,
        timeout: float = 5.0,
        auto_connect: bool = True
    ):
        """
        아두이노 시리얼 핸들러 초기화

        Args:
            port: 시리얼 포트 경로 (기본값: /dev/ttyACM0)
            baudrate: 통신 속도 (기본값: 115200)
            timeout: 읽기 타임아웃 (초) (기본값: 5.0)
            auto_connect: 초기화 시 자동 연결 여부
        """
        self.port = port
        self.baudrate = baudrate
        self.timeout = timeout
        self.serial_conn: Optional[serial.Serial] = None
        self.is_connected = False

        # 슬롯 관리 (각 카테고리별 다음 사용할 슬롯)
        self.slot_counters = {
            'NORMAL': 0,
            'COMPONENT_DEFECT': 0,
            'SOLDER_DEFECT': 0
        }
        self.max_slots = 5  # 각 카테고리당 최대 5개 슬롯

        if auto_connect:
            self.connect()

    def connect(self) -> bool:
        """
        아두이노와 시리얼 연결 수립

        Returns:
            bool: 연결 성공 여부
        """
        try:
            logger.info(f"아두이노 연결 시도: {self.port} (baudrate: {self.baudrate})")

            self.serial_conn = serial.Serial(
                port=self.port,
                baudrate=self.baudrate,
                timeout=self.timeout,
                write_timeout=self.timeout
            )

            # 아두이노 부팅 대기 (2초)
            logger.info("아두이노 부팅 대기 중...")
            time.sleep(2.0)

            # 연결 확인 (하트비트 대기)
            self._wait_for_arduino_ready()

            self.is_connected = True
            logger.info(f"✅ 아두이노 연결 성공: {self.port}")
            return True

        except serial.SerialException as e:
            logger.error(f"❌ 시리얼 연결 실패: {e}")
            self.is_connected = False
            return False
        except Exception as e:
            logger.error(f"❌ 연결 중 오류 발생: {e}")
            self.is_connected = False
            return False

    def _wait_for_arduino_ready(self, max_wait: float = 10.0) -> bool:
        """
        아두이노 준비 완료 대기 (하트비트 또는 초기화 메시지 확인)

        Args:
            max_wait: 최대 대기 시간 (초)

        Returns:
            bool: 준비 완료 여부
        """
        start_time = time.time()

        logger.info("아두이노 준비 상태 확인 중...")

        while time.time() - start_time < max_wait:
            try:
                if self.serial_conn and self.serial_conn.in_waiting > 0:
                    line = self.serial_conn.readline().decode('utf-8').strip()
                    logger.debug(f"[Arduino] {line}")

                    # 시스템 준비 메시지 확인
                    if "System ready" in line or "Waiting for commands" in line:
                        logger.info("✅ 아두이노 준비 완료")
                        return True

                time.sleep(0.1)

            except Exception as e:
                logger.warning(f"준비 확인 중 오류: {e}")
                continue

        # 타임아웃 발생 - 경고만 하고 계속 진행
        logger.warning("⚠️  아두이노 준비 메시지를 받지 못했지만 계속 진행합니다.")
        return True

    def disconnect(self):
        """시리얼 연결 종료"""
        if self.serial_conn and self.serial_conn.is_open:
            self.serial_conn.close()
            self.is_connected = False
            logger.info("아두이노 연결 종료")

    def send_json_command(self, command_data: Dict[str, Any]) -> Optional[Dict[str, Any]]:
        """
        JSON 명령을 아두이노로 전송하고 응답 수신

        Args:
            command_data: 전송할 JSON 데이터

        Returns:
            Dict[str, Any]: 아두이노 응답 (JSON 파싱된 딕셔너리) 또는 None
        """
        if not self.is_connected or not self.serial_conn:
            logger.error("❌ 아두이노가 연결되지 않았습니다.")
            return None

        try:
            # JSON 직렬화 및 전송
            json_str = json.dumps(command_data)
            command_line = json_str + '\n'

            logger.debug(f"→ Arduino: {json_str}")
            self.serial_conn.write(command_line.encode('utf-8'))
            self.serial_conn.flush()

            # 응답 수신 (타임아웃: 5초)
            start_time = time.time()
            while time.time() - start_time < self.timeout:
                if self.serial_conn.in_waiting > 0:
                    response_line = self.serial_conn.readline().decode('utf-8').strip()
                    logger.debug(f"← Arduino: {response_line}")

                    # JSON 응답 파싱
                    try:
                        response_data = json.loads(response_line)
                        return response_data
                    except json.JSONDecodeError:
                        # JSON이 아닌 로그 메시지는 무시
                        logger.debug(f"[Arduino Log] {response_line}")
                        continue

                time.sleep(0.05)

            # 타임아웃
            logger.warning("⚠️  아두이노 응답 타임아웃")
            return None

        except Exception as e:
            logger.error(f"❌ JSON 명령 전송 실패: {e}")
            return None

    def send_classification_result(
        self,
        defect_type: str,
        slot_index: Optional[int] = None,
        confidence: float = 0.0,
        serial_number: str = ""
    ) -> bool:
        """
        PCB 검사 결과를 아두이노로 전송 (로봇팔 제어)

        Args:
            defect_type: 판정 결과 ('정상', '부품불량', '납땜불량', '폐기')
            slot_index: 슬롯 인덱스 (None이면 자동 할당)
            confidence: 신뢰도 (0.0~1.0)
            serial_number: 시리얼 넘버 (선택)

        Returns:
            bool: 전송 및 배치 성공 여부
        """
        # 판정 결과 → 아두이노 카테고리 변환
        category = self.CATEGORY_MAP.get(defect_type)

        if not category:
            logger.error(f"❌ 알 수 없는 판정 결과: {defect_type}")
            return False

        # 슬롯 인덱스 결정
        if slot_index is None:
            if category == 'DISCARD':
                slot_index = 0  # 폐기는 슬롯 관리 안 함 (아두이노에서 무시됨)
            else:
                slot_index = self._get_next_slot(category)

        # JSON 명령 생성
        command_data = {
            "cmd": "place_pcb",
            "box_id": category,
            "slot_index": slot_index,
            "confidence": round(confidence, 2),
            "serial_number": serial_number
        }

        logger.info(
            f"[{defect_type}] PCB 배치 명령 전송: "
            f"{category} 박스 슬롯 {slot_index} "
            f"(신뢰도: {confidence:.2f})"
        )

        # 명령 전송
        response = self.send_json_command(command_data)

        if response:
            status = response.get('status')
            message = response.get('message', '')

            if status == 'ok':
                logger.info(f"✅ PCB 배치 성공: {message}")
                return True
            else:
                logger.error(f"❌ PCB 배치 실패: {message}")
                return False
        else:
            logger.error("❌ 아두이노 응답 없음")
            return False

    def _get_next_slot(self, category: str) -> int:
        """
        다음 사용 가능한 슬롯 인덱스 반환 (라운드 로빈)

        Args:
            category: 카테고리 ('NORMAL', 'COMPONENT_DEFECT', 'SOLDER_DEFECT')

        Returns:
            int: 슬롯 인덱스 (0~4)
        """
        if category not in self.slot_counters:
            return 0

        slot = self.slot_counters[category]
        self.slot_counters[category] = (slot + 1) % self.max_slots
        return slot

    def reset_slot_counters(self):
        """슬롯 카운터 초기화"""
        self.slot_counters = {
            'NORMAL': 0,
            'COMPONENT_DEFECT': 0,
            'SOLDER_DEFECT': 0
        }
        logger.info("슬롯 카운터 초기화 완료")

    def send_home_command(self) -> bool:
        """로봇팔을 홈 포지션으로 이동"""
        command_data = {"cmd": "home"}
        logger.info("홈 포지션 이동 명령 전송")

        response = self.send_json_command(command_data)

        if response and response.get('status') == 'ok':
            logger.info("✅ 홈 포지션 이동 성공")
            return True
        else:
            logger.error("❌ 홈 포지션 이동 실패")
            return False

    def __enter__(self):
        """컨텍스트 매니저 진입"""
        if not self.is_connected:
            self.connect()
        return self

    def __exit__(self, exc_type, exc_val, exc_tb):
        """컨텍스트 매니저 종료"""
        self.disconnect()

    def __del__(self):
        """소멸자: 연결 종료"""
        self.disconnect()


# ========================================
# 테스트 코드
# ========================================
if __name__ == '__main__':
    import sys

    print("=" * 60)
    print("Arduino Serial Handler 테스트")
    print("=" * 60)

    # 시리얼 포트 입력 (기본값: /dev/ttyACM0)
    port = input("시리얼 포트 입력 (기본값: /dev/ttyACM0): ").strip()
    if not port:
        port = '/dev/ttyACM0'

    # 핸들러 생성 및 연결
    handler = ArduinoSerialHandler(port=port, auto_connect=True)

    if not handler.is_connected:
        print("❌ 아두이노 연결 실패")
        sys.exit(1)

    try:
        # 홈 포지션으로 이동
        print("\n[1] 홈 포지션으로 이동")
        handler.send_home_command()
        time.sleep(2)

        # 테스트 시나리오
        test_cases = [
            ('정상', 0.95, 'MBFT12345678'),
            ('부품불량', 0.88, 'MBFT12345679'),
            ('납땜불량', 0.76, 'MBFT12345680'),
            ('폐기', 0.45, 'MBFT12345681'),
        ]

        print("\n[2] PCB 배치 테스트 시작")
        for i, (defect_type, confidence, serial_num) in enumerate(test_cases, 1):
            print(f"\n--- 테스트 {i}/{len(test_cases)} ---")
            success = handler.send_classification_result(
                defect_type=defect_type,
                confidence=confidence,
                serial_number=serial_num
            )

            if success:
                print(f"✅ {defect_type} PCB 배치 성공")
            else:
                print(f"❌ {defect_type} PCB 배치 실패")

            # 다음 테스트까지 대기
            time.sleep(3)

        print("\n테스트 완료!")

    except KeyboardInterrupt:
        print("\n\n사용자 중단")
    except Exception as e:
        print(f"\n오류 발생: {e}")
    finally:
        handler.disconnect()
        print("연결 종료")
