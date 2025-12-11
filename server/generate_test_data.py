"""
오늘치 검사 데이터 삭제 및 500개의 테스트 데이터 생성 스크립트
불량율: 15-25% (평균 20%)
"""

import pymysql
from datetime import datetime, timedelta
import random
import json

# DB 연결 정보
DB_CONFIG = {
    'host': '100.80.24.53',
    'port': 3306,
    'user': 'pcb_admin',
    'password': '1234',
    'database': 'pcb_inspection'
}

# 제품 코드별 부품 수
PRODUCT_COMPONENTS = {
    'FT': 21,  # FT 제품: 21개 부품
    'RS': 21,  # RS 제품: 21개 부품
    'BC': 21   # BC 제품: 21개 부품
}

def generate_serial_number(product_code, index):
    """시리얼 넘버 생성"""
    return f"MB{product_code}-{index:08d}"

def generate_inspection_data(index, total_count):
    """검사 데이터 생성"""
    # 제품 코드 랜덤 선택 (FT:50%, RS:30%, BC:20%)
    product_code = random.choices(
        ['FT', 'RS', 'BC'],
        weights=[0.5, 0.3, 0.2]
    )[0]

    serial_number = generate_serial_number(product_code, index)
    total_components = PRODUCT_COMPONENTS[product_code]

    # 불량율 15-25% (평균 20%)
    defect_rate = random.uniform(0.15, 0.25)
    is_defective = random.random() < defect_rate

    if is_defective:
        # 불량 타입 결정
        defect_type = random.choices(
            ['missing', 'position_error', 'discard'],
            weights=[0.5, 0.3, 0.2]  # 누락 50%, 위치오류 30%, 폐기 20%
        )[0]

        if defect_type == 'missing':
            decision = 'missing_component'
            missing_count = random.randint(1, 3)
            position_error_count = 0
        elif defect_type == 'position_error':
            decision = 'position_error'
            missing_count = 0
            position_error_count = random.randint(1, 5)
        else:  # discard
            decision = 'discard'
            missing_count = random.randint(3, 6)
            position_error_count = random.randint(2, 5)
    else:
        decision = 'normal'
        missing_count = 0
        position_error_count = 0

    # 타임스탬프 (오늘 날짜, 시간은 랜덤)
    today = datetime.now().date()
    hour = random.randint(0, 23)
    minute = random.randint(0, 59)
    second = random.randint(0, 59)
    microsecond = random.randint(0, 999999)

    timestamp = datetime(
        today.year, today.month, today.day,
        hour, minute, second, microsecond
    )

    # 검출된 정상 부품 수
    correct_count = total_components - missing_count - position_error_count
    detection_count = correct_count + position_error_count  # 검출된 총 부품 수

    return {
        'serial_number': serial_number,
        'product_code': product_code,
        'decision': decision,
        'missing_count': missing_count,
        'position_error_count': position_error_count,
        'extra_count': 0,  # 추가 부품 없음
        'correct_count': correct_count,
        'detection_count': detection_count,
        'avg_confidence': round(random.uniform(0.75, 0.95), 2),
        'inference_time_ms': round(random.uniform(40, 60), 1),
        'verification_time_ms': round(random.uniform(5, 15), 1),
        'total_time_ms': round(random.uniform(100, 150), 1),
        'qr_detected': 1,
        'serial_detected': 1,
        'inspection_time': timestamp.strftime('%Y-%m-%d %H:%M:%S.%f')
    }

def main():
    print("=" * 60)
    print("검사 데이터 초기화 및 생성 스크립트")
    print("=" * 60)

    # DB 연결
    print(f"\n1️⃣  DB 연결 중... ({DB_CONFIG['host']}:{DB_CONFIG['port']})")
    conn = pymysql.connect(**DB_CONFIG)
    cursor = conn.cursor()
    print("   ✅ DB 연결 완료")

    # 오늘치 데이터 삭제
    today = datetime.now().date()
    print(f"\n2️⃣  오늘({today}) 검사 데이터 삭제 중...")

    # inspections 테이블 삭제
    delete_query = """
    DELETE FROM inspections
    WHERE DATE(inspection_time) = %s
    """
    cursor.execute(delete_query, (today,))
    inspections_deleted = cursor.rowcount
    print(f"   - inspections: {inspections_deleted}개 삭제")

    # inspection_summary_daily 테이블 삭제
    delete_query = """
    DELETE FROM inspection_summary_daily
    WHERE date = %s
    """
    cursor.execute(delete_query, (today,))
    daily_deleted = cursor.rowcount
    print(f"   - inspection_summary_daily: {daily_deleted}개 삭제")

    # inspection_summary_hourly 테이블 삭제 (오늘 날짜의 모든 시간대)
    delete_query = """
    DELETE FROM inspection_summary_hourly
    WHERE DATE(hour_timestamp) = %s
    """
    cursor.execute(delete_query, (today,))
    hourly_deleted = cursor.rowcount
    print(f"   - inspection_summary_hourly: {hourly_deleted}개 삭제")

    # inspection_summary_monthly 테이블 삭제 (이번 달)
    delete_query = """
    DELETE FROM inspection_summary_monthly
    WHERE year = %s AND month = %s
    """
    cursor.execute(delete_query, (today.year, today.month))
    monthly_deleted = cursor.rowcount
    print(f"   - inspection_summary_monthly: {monthly_deleted}개 삭제")

    conn.commit()
    total_deleted = inspections_deleted + daily_deleted + hourly_deleted + monthly_deleted
    print(f"   ✅ 총 {total_deleted}개 데이터 삭제 완료")

    # 500개 테스트 데이터 생성
    print(f"\n3️⃣  500개 테스트 데이터 생성 중...")

    insert_query = """
    INSERT INTO inspections
    (serial_number, product_code, decision, qr_detected, serial_detected,
     missing_count, position_error_count, extra_count, correct_count,
     detection_count, avg_confidence, inference_time_ms, verification_time_ms,
     total_time_ms, inspection_time)
    VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s)
    """

    normal_count = 0
    missing_count = 0
    position_error_count = 0
    discard_count = 0

    for i in range(1, 501):
        data = generate_inspection_data(i, 500)

        cursor.execute(insert_query, (
            data['serial_number'],
            data['product_code'],
            data['decision'],
            data['qr_detected'],
            data['serial_detected'],
            data['missing_count'],
            data['position_error_count'],
            data['extra_count'],
            data['correct_count'],
            data['detection_count'],
            data['avg_confidence'],
            data['inference_time_ms'],
            data['verification_time_ms'],
            data['total_time_ms'],
            data['inspection_time']
        ))

        # 통계 집계
        if data['decision'] == 'normal':
            normal_count += 1
        elif data['decision'] == 'missing_component':
            missing_count += 1
        elif data['decision'] == 'position_error':
            position_error_count += 1
        elif data['decision'] == 'discard':
            discard_count += 1

        if i % 100 == 0:
            print(f"   - {i}/500 데이터 생성 중...")

    conn.commit()
    print(f"   ✅ 500개 데이터 생성 완료")

    # 결과 출력
    defect_count = missing_count + position_error_count + discard_count
    defect_rate = (defect_count / 500) * 100

    print("\n" + "=" * 60)
    print("📊 생성된 데이터 통계")
    print("=" * 60)
    print(f"전체:        500개")
    print(f"정상:        {normal_count}개 ({(normal_count/500)*100:.1f}%)")
    print(f"부품누락:    {missing_count}개 ({(missing_count/500)*100:.1f}%)")
    print(f"위치오류:    {position_error_count}개 ({(position_error_count/500)*100:.1f}%)")
    print(f"폐기:        {discard_count}개 ({(discard_count/500)*100:.1f}%)")
    print("-" * 60)
    print(f"불량율:      {defect_rate:.1f}% (목표: 15-25%)")
    print("=" * 60)

    # 연결 종료
    cursor.close()
    conn.close()
    print("\n✅ 모든 작업 완료!")

if __name__ == '__main__':
    main()
