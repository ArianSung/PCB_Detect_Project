"""
MySQL 데이터베이스 관리자

Flask 서버에서 MySQL 데이터베이스와 연동하기 위한 클래스입니다.
- 검사 이력 저장
- 박스 상태 관리
- 통계 조회
- 시스템 로그 기록
"""

import pymysql
from pymysql.cursors import DictCursor
import json
from datetime import datetime
from typing import Optional, Dict, List, Any
import logging

logger = logging.getLogger(__name__)


class DatabaseManager:
    """MySQL 데이터베이스 관리 클래스"""

    def __init__(self, host: str, port: int, user: str, password: str, database: str):
        """
        데이터베이스 매니저 초기화

        Args:
            host: MySQL 서버 호스트 (예: '100.64.1.1')
            port: MySQL 포트 (기본: 3306)
            user: 사용자명 (예: 'pcb_server')
            password: 비밀번호
            database: 데이터베이스 이름 (예: 'pcb_inspection')
        """
        self.config = {
            'host': host,
            'port': port,
            'user': user,
            'password': password,
            'database': database,
            'charset': 'utf8mb4',
            'cursorclass': DictCursor,
            'autocommit': True  # 자동 커밋 활성화
        }
        self.connection = None
        logger.info(f"DatabaseManager 초기화: {user}@{host}:{port}/{database}")

    def connect(self) -> bool:
        """
        데이터베이스 연결

        Returns:
            bool: 연결 성공 여부
        """
        try:
            self.connection = pymysql.connect(**self.config)
            logger.info("데이터베이스 연결 성공")
            return True
        except pymysql.Error as e:
            logger.error(f"데이터베이스 연결 실패: {e}")
            return False

    def disconnect(self):
        """데이터베이스 연결 해제"""
        if self.connection:
            self.connection.close()
            logger.info("데이터베이스 연결 해제")

    def get_connection(self):
        """
        연결 상태 확인 및 재연결

        Returns:
            connection: MySQL 연결 객체
        """
        try:
            if self.connection is None or not self.connection.open:
                self.connect()
            else:
                # 연결 테스트
                self.connection.ping(reconnect=True)
            return self.connection
        except pymysql.Error as e:
            logger.error(f"연결 확인 실패: {e}")
            self.connect()
            return self.connection

    # ========================================
    # 검사 이력 관련 메서드
    # ========================================

    def insert_inspection(self, camera_id: str, defect_type: str, confidence: float,
                         boxes: List[Dict], gpio_pin: int,
                         image_path: Optional[str] = None) -> Optional[int]:
        """
        검사 결과를 데이터베이스에 저장

        Args:
            camera_id: 카메라 ID (left/right)
            defect_type: 불량 유형 (정상/부품불량/납땜불량/폐기)
            confidence: 신뢰도 (0.0 ~ 1.0)
            boxes: 바운딩 박스 리스트 (JSON 형식)
            gpio_pin: GPIO 핀 번호
            image_path: 이미지 경로 (선택)

        Returns:
            int: 삽입된 레코드의 ID, 실패 시 None
        """
        try:
            conn = self.get_connection()
            with conn.cursor() as cursor:
                sql = """
                    INSERT INTO inspections
                    (camera_id, defect_type, confidence, boxes, gpio_pin, image_path, inspection_time)
                    VALUES (%s, %s, %s, %s, %s, %s, NOW())
                """
                cursor.execute(sql, (
                    camera_id,
                    defect_type,
                    confidence,
                    json.dumps(boxes, ensure_ascii=False),  # JSON 직렬화
                    gpio_pin,
                    image_path
                ))

                # 삽입된 ID 가져오기
                inspection_id = cursor.lastrowid
                logger.info(f"검사 이력 저장 완료 (ID: {inspection_id})")
                return inspection_id

        except pymysql.Error as e:
            logger.error(f"검사 이력 저장 실패: {e}")
            return None

    def get_recent_inspections(self, limit: int = 20) -> List[Dict]:
        """
        최근 검사 이력 조회

        Args:
            limit: 조회할 개수 (기본: 20)

        Returns:
            List[Dict]: 검사 이력 리스트
        """
        try:
            conn = self.get_connection()
            with conn.cursor() as cursor:
                sql = """
                    SELECT
                        id, camera_id, defect_type, confidence,
                        inspection_time, image_path, gpio_pin
                    FROM inspections
                    ORDER BY inspection_time DESC
                    LIMIT %s
                """
                cursor.execute(sql, (limit,))
                results = cursor.fetchall()

                # datetime을 문자열로 변환
                for row in results:
                    if row['inspection_time']:
                        row['inspection_time'] = row['inspection_time'].isoformat()

                return results

        except pymysql.Error as e:
            logger.error(f"검사 이력 조회 실패: {e}")
            return []

    def get_inspection_by_id(self, inspection_id: int) -> Optional[Dict]:
        """
        특정 검사 이력 조회

        Args:
            inspection_id: 검사 ID

        Returns:
            Dict: 검사 이력, 없으면 None
        """
        try:
            conn = self.get_connection()
            with conn.cursor() as cursor:
                sql = """
                    SELECT * FROM inspections WHERE id = %s
                """
                cursor.execute(sql, (inspection_id,))
                result = cursor.fetchone()

                if result and result['inspection_time']:
                    result['inspection_time'] = result['inspection_time'].isoformat()

                return result

        except pymysql.Error as e:
            logger.error(f"검사 이력 조회 실패 (ID: {inspection_id}): {e}")
            return None

    # ========================================
    # 박스 상태 관리
    # ========================================

    def get_all_box_status(self) -> List[Dict]:
        """
        전체 박스 상태 조회

        Returns:
            List[Dict]: 박스 상태 리스트 (NORMAL, COMPONENT_DEFECT, SOLDER_DEFECT)
        """
        try:
            conn = self.get_connection()
            with conn.cursor() as cursor:
                sql = """
                    SELECT
                        box_id, category, current_slot, max_slots,
                        is_full, total_pcb_count, last_updated
                    FROM box_status
                    ORDER BY box_id
                """
                cursor.execute(sql)
                results = cursor.fetchall()

                # datetime을 문자열로 변환
                for row in results:
                    if row['last_updated']:
                        row['last_updated'] = row['last_updated'].isoformat()

                return results

        except pymysql.Error as e:
            logger.error(f"박스 상태 조회 실패: {e}")
            return []

    def get_box_status(self, box_id: str) -> Optional[Dict]:
        """
        특정 박스 상태 조회

        Args:
            box_id: 박스 ID (NORMAL, COMPONENT_DEFECT, SOLDER_DEFECT)

        Returns:
            Dict: 박스 상태, 없으면 None
        """
        try:
            conn = self.get_connection()
            with conn.cursor() as cursor:
                sql = """
                    SELECT * FROM box_status WHERE box_id = %s
                """
                cursor.execute(sql, (box_id,))
                result = cursor.fetchone()

                if result and result['last_updated']:
                    result['last_updated'] = result['last_updated'].isoformat()

                return result

        except pymysql.Error as e:
            logger.error(f"박스 상태 조회 실패 (box_id: {box_id}): {e}")
            return None

    def update_box_status(self, box_id: str, increment: bool = True) -> bool:
        """
        박스 슬롯 업데이트 (PCB 추가)

        Args:
            box_id: 박스 ID
            increment: True이면 슬롯 증가, False이면 감소

        Returns:
            bool: 성공 여부
        """
        try:
            conn = self.get_connection()
            with conn.cursor() as cursor:
                if increment:
                    sql = """
                        UPDATE box_status
                        SET
                            current_slot = current_slot + 1,
                            total_pcb_count = total_pcb_count + 1,
                            is_full = CASE
                                WHEN current_slot + 1 >= max_slots THEN TRUE
                                ELSE FALSE
                            END
                        WHERE box_id = %s AND current_slot < max_slots
                    """
                else:
                    sql = """
                        UPDATE box_status
                        SET
                            current_slot = GREATEST(current_slot - 1, 0),
                            is_full = FALSE
                        WHERE box_id = %s
                    """

                cursor.execute(sql, (box_id,))

                if cursor.rowcount > 0:
                    logger.info(f"박스 상태 업데이트 완료 (box_id: {box_id}, increment: {increment})")
                    return True
                else:
                    logger.warning(f"박스가 가득 차서 업데이트 불가 (box_id: {box_id})")
                    return False

        except pymysql.Error as e:
            logger.error(f"박스 상태 업데이트 실패: {e}")
            return False

    def reset_box_status(self, box_id: Optional[str] = None) -> bool:
        """
        박스 상태 리셋 (비우기)

        Args:
            box_id: 특정 박스 ID, None이면 전체 리셋

        Returns:
            bool: 성공 여부
        """
        try:
            conn = self.get_connection()
            with conn.cursor() as cursor:
                if box_id:
                    sql = """
                        UPDATE box_status
                        SET current_slot = 0, is_full = FALSE
                        WHERE box_id = %s
                    """
                    cursor.execute(sql, (box_id,))
                else:
                    sql = """
                        UPDATE box_status
                        SET current_slot = 0, is_full = FALSE
                    """
                    cursor.execute(sql)

                logger.info(f"박스 상태 리셋 완료 (box_id: {box_id or 'ALL'})")
                return True

        except pymysql.Error as e:
            logger.error(f"박스 상태 리셋 실패: {e}")
            return False

    # ========================================
    # 통계 관련 메서드
    # ========================================

    def get_daily_statistics(self, days: int = 7) -> List[Dict]:
        """
        일별 통계 조회

        Args:
            days: 조회할 일수 (기본: 7일)

        Returns:
            List[Dict]: 일별 통계 리스트
        """
        try:
            conn = self.get_connection()
            with conn.cursor() as cursor:
                sql = """
                    SELECT
                        stat_date, total_inspections, normal_count,
                        component_defect_count, solder_defect_count, discard_count,
                        defect_rate
                    FROM statistics_daily
                    WHERE stat_date >= DATE_SUB(CURDATE(), INTERVAL %s DAY)
                    ORDER BY stat_date DESC
                """
                cursor.execute(sql, (days,))
                results = cursor.fetchall()

                # date를 문자열로 변환
                for row in results:
                    if row['stat_date']:
                        row['stat_date'] = row['stat_date'].isoformat()
                    # Decimal을 float로 변환
                    if row['defect_rate']:
                        row['defect_rate'] = float(row['defect_rate'])

                return results

        except pymysql.Error as e:
            logger.error(f"일별 통계 조회 실패: {e}")
            return []

    def get_today_statistics(self) -> Optional[Dict]:
        """
        오늘의 통계 조회

        Returns:
            Dict: 오늘의 통계, 없으면 None
        """
        try:
            conn = self.get_connection()
            with conn.cursor() as cursor:
                sql = """
                    SELECT
                        COUNT(*) as total_inspections,
                        SUM(CASE WHEN defect_type = '정상' THEN 1 ELSE 0 END) as normal_count,
                        SUM(CASE WHEN defect_type = '부품불량' THEN 1 ELSE 0 END) as component_defect_count,
                        SUM(CASE WHEN defect_type = '납땜불량' THEN 1 ELSE 0 END) as solder_defect_count,
                        SUM(CASE WHEN defect_type = '폐기' THEN 1 ELSE 0 END) as discard_count
                    FROM inspections
                    WHERE DATE(inspection_time) = CURDATE()
                """
                cursor.execute(sql)
                result = cursor.fetchone()

                if result:
                    # 불량률 계산
                    total = result['total_inspections'] or 0
                    if total > 0:
                        defects = (result['component_defect_count'] or 0) + \
                                 (result['solder_defect_count'] or 0) + \
                                 (result['discard_count'] or 0)
                        result['defect_rate'] = round(defects * 100.0 / total, 2)
                    else:
                        result['defect_rate'] = 0.0

                return result

        except pymysql.Error as e:
            logger.error(f"오늘 통계 조회 실패: {e}")
            return None

    # ========================================
    # 시스템 로그
    # ========================================

    def insert_system_log(self, level: str, source: str, message: str,
                         details: Optional[Dict] = None) -> bool:
        """
        시스템 로그 저장

        Args:
            level: 로그 레벨 (DEBUG, INFO, WARNING, ERROR, CRITICAL)
            source: 로그 소스 (server/raspberry-pi-1/raspberry-pi-2/winforms)
            message: 로그 메시지
            details: 상세 정보 (JSON)

        Returns:
            bool: 성공 여부
        """
        try:
            conn = self.get_connection()
            with conn.cursor() as cursor:
                sql = """
                    INSERT INTO system_logs (log_level, source, message, details)
                    VALUES (%s, %s, %s, %s)
                """
                cursor.execute(sql, (
                    level,
                    source,
                    message,
                    json.dumps(details, ensure_ascii=False) if details else None
                ))
                return True

        except pymysql.Error as e:
            logger.error(f"시스템 로그 저장 실패: {e}")
            return False

    # ========================================
    # 헬스 체크
    # ========================================

    def health_check(self) -> bool:
        """
        데이터베이스 연결 상태 확인

        Returns:
            bool: 연결 정상 여부
        """
        try:
            conn = self.get_connection()
            with conn.cursor() as cursor:
                cursor.execute("SELECT 1")
                return True
        except pymysql.Error as e:
            logger.error(f"Health check 실패: {e}")
            return False

    def __enter__(self):
        """Context manager 진입"""
        self.connect()
        return self

    def __exit__(self, exc_type, exc_val, exc_tb):
        """Context manager 종료"""
        self.disconnect()


# ========================================
# 테스트 코드
# ========================================

if __name__ == '__main__':
    # 로깅 설정
    logging.basicConfig(
        level=logging.INFO,
        format='[%(asctime)s] [%(levelname)s] - %(message)s'
    )

    # 데이터베이스 연결 테스트
    db = DatabaseManager(
        host='localhost',  # 실제 환경에 맞게 변경
        port=3306,
        user='root',  # 실제 사용자명으로 변경
        password='your_password',  # 실제 비밀번호로 변경
        database='pcb_inspection'
    )

    if db.connect():
        print("✅ 데이터베이스 연결 성공!")

        # Health check
        if db.health_check():
            print("✅ Health check 성공!")

        # 박스 상태 조회
        boxes = db.get_all_box_status()
        print(f"✅ 박스 개수: {len(boxes)}")

        # 오늘 통계 조회
        stats = db.get_today_statistics()
        print(f"✅ 오늘 검사 수: {stats['total_inspections']}")

        db.disconnect()
    else:
        print("❌ 데이터베이스 연결 실패!")
