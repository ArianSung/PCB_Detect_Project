#!/usr/bin/env python3
"""
PCB 검사 시스템 - 테스트 데이터 삽입 스크립트
오늘 날짜로 700개의 현실적인 검사 데이터를 생성하여 MySQL에 삽입
"""

import pymysql
import random
import json
from datetime import datetime, timedelta

# MySQL 연결 설정
DB_CONFIG = {
    'host': '100.80.24.53',
    'user': 'pcb_admin',
    'password': '1234',
    'database': 'pcb_inspection',
    'charset': 'utf8mb4'
}

# 제품 정보 (product_code: (name, component_count))
PRODUCTS = {
    'FT': ('Fast Type PCB', 25),
    'RS': ('Reliable Stable PCB', 30),
    'BC': ('Budget Compact PCB', 18)
}

# 판정 타입 및 확률 분포
DECISION_WEIGHTS = {
    'normal': 0.80,           # 80% 정상
    'missing': 0.08,          # 8% 부품 누락
    'position_error': 0.08,   # 8% 위치 오류
    'discard': 0.04           # 4% 폐기
}

# 제품별 확률 분포
PRODUCT_WEIGHTS = {
    'FT': 0.40,  # 40%
    'RS': 0.40,  # 40%
    'BC': 0.20   # 20%
}

# 부품 클래스 예시 (YOLO 검출 클래스)
COMPONENT_CLASSES = [
    'capacitor', 'resistor', 'ic', 'led', 'connector',
    'transistor', 'diode', 'inductor', 'crystal', 'fuse'
]


def generate_serial_number(product_code):
    """시리얼 넘버 생성: MBXX12345678"""
    serial_num = random.randint(10000000, 99999999)
    return f"MB{product_code}{serial_num}"


def generate_inspection_time(base_date, index, total):
    """작업 시간 분산 (09:00~17:00, 8시간)"""
    # 09:00부터 시작
    start_hour = 9
    work_hours = 8

    # 균등 분산
    seconds_offset = (work_hours * 3600) * (index / total)

    # 약간의 랜덤성 추가 (±5분)
    random_offset = random.randint(-300, 300)

    inspection_datetime = base_date.replace(hour=start_hour, minute=0, second=0, microsecond=0)
    inspection_datetime += timedelta(seconds=seconds_offset + random_offset)

    return inspection_datetime


def generate_defect_counts(decision, component_count):
    """판정에 따른 불량 개수 생성"""
    if decision == 'normal':
        return {
            'missing_count': 0,
            'position_error_count': 0,
            'extra_count': 0,
            'correct_count': component_count
        }
    elif decision == 'missing':
        missing = random.randint(1, 2)  # 1~2개 누락
        return {
            'missing_count': missing,
            'position_error_count': 0,
            'extra_count': 0,
            'correct_count': component_count - missing
        }
    elif decision == 'position_error':
        position_err = random.randint(1, 4)  # 1~4개 위치 오류
        return {
            'missing_count': 0,
            'position_error_count': position_err,
            'extra_count': 0,
            'correct_count': component_count - position_err
        }
    else:  # discard
        missing = random.randint(3, 5)
        position_err = random.randint(2, 4)
        return {
            'missing_count': missing,
            'position_error_count': position_err,
            'extra_count': random.randint(0, 1),
            'correct_count': max(0, component_count - missing - position_err)
        }


def generate_missing_components(count):
    """누락 부품 JSON 생성"""
    if count == 0:
        return None

    components = []
    for i in range(count):
        components.append({
            'class_name': random.choice(COMPONENT_CLASSES),
            'expected_position': {
                'x': round(random.uniform(100, 540), 2),
                'y': round(random.uniform(100, 380), 2)
            }
        })
    return json.dumps(components)


def generate_position_errors(count):
    """위치 오류 JSON 생성"""
    if count == 0:
        return None

    errors = []
    for i in range(count):
        expected_x = round(random.uniform(100, 540), 2)
        expected_y = round(random.uniform(100, 380), 2)
        offset_x = round(random.uniform(25, 50), 2)
        offset_y = round(random.uniform(25, 50), 2)

        errors.append({
            'class_name': random.choice(COMPONENT_CLASSES),
            'expected': {'x': expected_x, 'y': expected_y},
            'actual': {'x': expected_x + offset_x, 'y': expected_y + offset_y},
            'offset': round(((offset_x**2 + offset_y**2)**0.5), 2)
        })
    return json.dumps(errors)


def generate_yolo_detections(detection_count):
    """YOLO 검출 결과 JSON 생성"""
    detections = []
    for i in range(detection_count):
        x1 = round(random.uniform(50, 500), 2)
        y1 = round(random.uniform(50, 350), 2)
        w = round(random.uniform(20, 80), 2)
        h = round(random.uniform(20, 80), 2)

        detections.append({
            'class': random.choice(COMPONENT_CLASSES),
            'confidence': round(random.uniform(0.75, 0.99), 4),
            'bbox': {
                'x1': x1,
                'y1': y1,
                'x2': x1 + w,
                'y2': y1 + h
            }
        })
    return json.dumps(detections)


def generate_inspection_record(index, total, base_date):
    """단일 검사 레코드 생성"""
    # 제품 선택
    product_code = random.choices(
        list(PRODUCT_WEIGHTS.keys()),
        weights=list(PRODUCT_WEIGHTS.values())
    )[0]
    product_name, component_count = PRODUCTS[product_code]

    # 판정 선택
    decision = random.choices(
        list(DECISION_WEIGHTS.keys()),
        weights=list(DECISION_WEIGHTS.values())
    )[0]

    # 시리얼 넘버 생성
    serial_number = generate_serial_number(product_code)

    # QR 데이터
    qr_detected = random.random() > 0.05  # 95% QR 검출 성공
    qr_data = f"http://localhost:8080/product/{serial_number}" if qr_detected else None

    # 불량 개수
    counts = generate_defect_counts(decision, component_count)
    detection_count = counts['correct_count'] + counts['position_error_count']

    # 처리 시간
    inference_time = round(random.uniform(30, 50), 2)
    verification_time = round(random.uniform(5, 15), 2)
    total_time = round(inference_time + verification_time, 2)

    # 평균 신뢰도
    avg_confidence = round(random.uniform(0.85, 0.98), 4)

    # 검사 시간
    inspection_time = generate_inspection_time(base_date, index, total)

    record = {
        'serial_number': serial_number,
        'product_code': product_code,
        'qr_data': qr_data,
        'qr_detected': qr_detected,
        'serial_detected': True,  # 시리얼은 항상 검출됨
        'decision': decision,
        'missing_count': counts['missing_count'],
        'position_error_count': counts['position_error_count'],
        'extra_count': counts['extra_count'],
        'correct_count': counts['correct_count'],
        'missing_components': generate_missing_components(counts['missing_count']),
        'position_errors': generate_position_errors(counts['position_error_count']),
        'extra_components': None,  # 간단화
        'yolo_detections': generate_yolo_detections(detection_count),
        'detection_count': detection_count,
        'avg_confidence': avg_confidence,
        'inference_time_ms': inference_time,
        'verification_time_ms': verification_time,
        'total_time_ms': total_time,
        'left_image_path': f'/data/images/{base_date.strftime("%Y%m%d")}/left_{index:04d}.jpg',
        'right_image_path': f'/data/images/{base_date.strftime("%Y%m%d")}/right_{index:04d}.jpg',
        'image_width': 640,
        'image_height': 480,
        'camera_id': 'dual',
        'client_ip': '100.64.1.2',
        'server_version': 'v3.1',
        'user_id': None,
        'notes': None,
        'inspection_time': inspection_time
    }

    return record


def insert_records(records):
    """레코드를 MySQL에 삽입"""
    connection = pymysql.connect(**DB_CONFIG)

    try:
        with connection.cursor() as cursor:
            sql = """
            INSERT INTO inspections (
                serial_number, product_code, qr_data, qr_detected, serial_detected,
                decision, missing_count, position_error_count, extra_count, correct_count,
                missing_components, position_errors, extra_components,
                yolo_detections, detection_count, avg_confidence,
                inference_time_ms, verification_time_ms, total_time_ms,
                left_image_path, right_image_path, image_width, image_height,
                camera_id, client_ip, server_version, user_id, notes, inspection_time
            ) VALUES (
                %(serial_number)s, %(product_code)s, %(qr_data)s, %(qr_detected)s, %(serial_detected)s,
                %(decision)s, %(missing_count)s, %(position_error_count)s, %(extra_count)s, %(correct_count)s,
                %(missing_components)s, %(position_errors)s, %(extra_components)s,
                %(yolo_detections)s, %(detection_count)s, %(avg_confidence)s,
                %(inference_time_ms)s, %(verification_time_ms)s, %(total_time_ms)s,
                %(left_image_path)s, %(right_image_path)s, %(image_width)s, %(image_height)s,
                %(camera_id)s, %(client_ip)s, %(server_version)s, %(user_id)s, %(notes)s, %(inspection_time)s
            )
            """

            # 배치 삽입
            cursor.executemany(sql, records)
            connection.commit()

            print(f"✓ {len(records)}개의 레코드가 성공적으로 삽입되었습니다.")

    except Exception as e:
        connection.rollback()
        print(f"✗ 삽입 중 오류 발생: {e}")
        raise
    finally:
        connection.close()


def main():
    """메인 함수"""
    print("=" * 60)
    print("PCB 검사 시스템 - 테스트 데이터 삽입")
    print("=" * 60)

    # 오늘 날짜
    today = datetime.now().replace(hour=0, minute=0, second=0, microsecond=0)
    print(f"\n날짜: {today.strftime('%Y-%m-%d')}")
    print(f"생성할 레코드 수: 700개")

    # 데이터 생성
    print("\n데이터 생성 중...")
    records = []
    for i in range(700):
        record = generate_inspection_record(i, 700, today)
        records.append(record)

        if (i + 1) % 100 == 0:
            print(f"  {i + 1}/700 레코드 생성 완료")

    # 통계 출력
    print("\n생성된 데이터 통계:")
    decision_counts = {}
    product_counts = {}

    for record in records:
        decision = record['decision']
        product = record['product_code']

        decision_counts[decision] = decision_counts.get(decision, 0) + 1
        product_counts[product] = product_counts.get(product, 0) + 1

    print("\n판정 분포:")
    for decision, count in sorted(decision_counts.items()):
        percentage = (count / 700) * 100
        print(f"  {decision:15s}: {count:3d}개 ({percentage:5.1f}%)")

    print("\n제품 분포:")
    for product, count in sorted(product_counts.items()):
        percentage = (count / 700) * 100
        print(f"  {product} ({PRODUCTS[product][0]:20s}): {count:3d}개 ({percentage:5.1f}%)")

    # 데이터베이스 삽입
    print("\n" + "=" * 60)
    print("MySQL 데이터베이스에 삽입 중...")
    print("=" * 60)

    insert_records(records)

    print("\n" + "=" * 60)
    print("✓ 작업 완료!")
    print("=" * 60)


if __name__ == '__main__':
    main()
