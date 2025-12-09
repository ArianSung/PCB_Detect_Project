#!/usr/bin/env python3
"""
더미 검사 데이터 및 이미지 생성 스크립트

- 12월 1일 ~ 12월 9일 데이터 생성
- 하루당 900 ± 50개 (850~950개)
- 불량률 20 ± 8% (12~28%)
- 더미 PCB 이미지 생성 (앞면/뒷면)
"""

import pymysql
import random
import json
from datetime import datetime, timedelta
import cv2
import numpy as np
from pathlib import Path
import os

# DB 연결 정보
DB_CONFIG = {
    'host': '100.80.24.53',
    'user': 'pcb_admin',
    'password': '1234',
    'database': 'pcb_inspection',
    'charset': 'utf8mb4'
}

# 제품 코드 리스트
PRODUCT_CODES = ['FT', 'RS', 'BC']
SERIAL_PREFIXES = {'FT': 'MBFT', 'RS': 'MBRS', 'BC': 'MBBC'}

# 더미 이미지 저장 경로 (윈도우에서 접근 가능한 경로)
WINDOWS_IMAGE_BASE = "C:\\PCBInspection\\Images"
IMAGE_DIR = Path("/home/sys1041/work_project/dummy_images")
IMAGE_DIR.mkdir(exist_ok=True)

# 판정 유형
DECISIONS = ['normal', 'missing', 'position_error', 'discard']


def create_dummy_pcb_image(width=640, height=480, is_front=True):
    """더미 PCB 이미지 생성"""
    # 배경색 (PCB 녹색)
    if is_front:
        bg_color = (50, 120, 50)  # 앞면: 어두운 녹색
    else:
        bg_color = (40, 100, 40)   # 뒷면: 더 어두운 녹색

    img = np.ones((height, width, 3), dtype=np.uint8) * np.array(bg_color, dtype=np.uint8)

    # 노이즈 추가
    noise = np.random.randint(-20, 20, (height, width, 3), dtype=np.int16)
    img = np.clip(img.astype(np.int16) + noise, 0, 255).astype(np.uint8)

    if is_front:
        # 앞면: 부품 표시 (원형 패드)
        num_components = random.randint(15, 30)
        for _ in range(num_components):
            x = random.randint(50, width - 50)
            y = random.randint(50, height - 50)
            radius = random.randint(3, 8)
            color = (random.randint(150, 255), random.randint(150, 255), random.randint(100, 200))
            cv2.circle(img, (x, y), radius, color, -1)

        # 회로 선 추가
        for _ in range(20):
            pt1 = (random.randint(0, width), random.randint(0, height))
            pt2 = (random.randint(0, width), random.randint(0, height))
            cv2.line(img, pt1, pt2, (180, 180, 120), 1)
    else:
        # 뒷면: 시리얼 넘버 영역
        cv2.rectangle(img, (50, 200), (300, 250), (255, 255, 255), -1)
        cv2.putText(img, "MB** ********", (60, 235), cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 0, 0), 2)

        # QR 코드 영역
        cv2.rectangle(img, (width - 150, 50), (width - 50, 150), (0, 0, 0), -1)
        cv2.rectangle(img, (width - 140, 60), (width - 60, 140), (255, 255, 255), -1)

    return img


def generate_serial_number(product_code):
    """시리얼 넘버 생성"""
    prefix = SERIAL_PREFIXES[product_code]
    number = random.randint(10000000, 99999999)
    return f"{prefix}{number}"


def generate_inspection_data(inspection_date, target_count, defect_rate):
    """검사 데이터 생성"""
    inspections = []

    for i in range(target_count):
        # 시간 분산 (하루 24시간)
        hour = random.randint(0, 23)
        minute = random.randint(0, 59)
        second = random.randint(0, 59)
        inspection_time = inspection_date.replace(hour=hour, minute=minute, second=second)

        # 제품 코드 랜덤 선택
        product_code = random.choice(PRODUCT_CODES)
        serial_number = generate_serial_number(product_code)

        # 판정 결정 (불량률 기반)
        is_defect = random.random() < (defect_rate / 100)

        if is_defect:
            decision = random.choices(
                ['missing', 'position_error', 'discard'],
                weights=[50, 40, 10]  # 누락 50%, 위치오류 40%, 폐기 10%
            )[0]
        else:
            decision = 'normal'

        # 불량 개수
        if decision == 'normal':
            missing_count = 0
            position_error_count = 0
            extra_count = 0
            correct_count = random.randint(20, 30)
        elif decision == 'missing':
            missing_count = random.randint(1, 3)
            position_error_count = 0
            extra_count = 0
            correct_count = random.randint(20, 28)
        elif decision == 'position_error':
            missing_count = 0
            position_error_count = random.randint(1, 5)
            extra_count = 0
            correct_count = random.randint(20, 28)
        else:  # discard
            missing_count = random.randint(3, 8)
            position_error_count = random.randint(5, 10)
            extra_count = random.randint(0, 2)
            correct_count = random.randint(10, 20)

        detection_count = correct_count + position_error_count + extra_count

        # 이미지 경로 (윈도우 경로)
        date_str = inspection_date.strftime("%Y%m%d")
        img_filename = f"{serial_number}_{i:04d}"
        left_image_path = f"{WINDOWS_IMAGE_BASE}\\{date_str}\\left_{img_filename}.jpg"
        right_image_path = f"{WINDOWS_IMAGE_BASE}\\{date_str}\\right_{img_filename}.jpg"

        # JSON 데이터 (간단한 더미 데이터)
        yolo_detections = json.dumps([
            {
                "class": f"component_{j}",
                "confidence": round(random.uniform(0.85, 0.99), 3),
                "bbox": [random.randint(0, 640), random.randint(0, 480),
                        random.randint(50, 100), random.randint(50, 100)]
            }
            for j in range(detection_count)
        ])

        missing_components = json.dumps([
            {"class_name": f"missing_{j}", "expected_position": [random.randint(0, 640), random.randint(0, 480)]}
            for j in range(missing_count)
        ]) if missing_count > 0 else None

        position_errors = json.dumps([
            {
                "class_name": f"error_{j}",
                "expected": [random.randint(0, 640), random.randint(0, 480)],
                "actual": [random.randint(0, 640), random.randint(0, 480)],
                "offset": round(random.uniform(20, 100), 2)
            }
            for j in range(position_error_count)
        ]) if position_error_count > 0 else None

        extra_components = json.dumps([
            {"class_name": f"extra_{j}", "position": [random.randint(0, 640), random.randint(0, 480)]}
            for j in range(extra_count)
        ]) if extra_count > 0 else None

        inspection = {
            'serial_number': serial_number,
            'product_code': product_code,
            'qr_data': f'http://localhost:8080/product/{serial_number}',
            'qr_detected': True,
            'serial_detected': True,
            'decision': decision,
            'missing_count': missing_count,
            'position_error_count': position_error_count,
            'extra_count': extra_count,
            'correct_count': correct_count,
            'missing_components': missing_components,
            'position_errors': position_errors,
            'extra_components': extra_components,
            'yolo_detections': yolo_detections,
            'detection_count': detection_count,
            'avg_confidence': round(random.uniform(0.85, 0.95), 3),
            'inference_time_ms': round(random.uniform(30, 80), 2),
            'verification_time_ms': round(random.uniform(5, 15), 2),
            'total_time_ms': round(random.uniform(100, 150), 2),
            'left_image_path': left_image_path,
            'right_image_path': right_image_path,
            'image_width': 640,
            'image_height': 480,
            'camera_id': 'dual',
            'client_ip': '100.64.1.2',
            'server_version': 'v3.0',
            'inspection_time': inspection_time
        }

        inspections.append(inspection)

    return inspections


def create_dummy_images_for_date(inspection_date, count):
    """해당 날짜의 더미 이미지 생성"""
    date_str = inspection_date.strftime("%Y%m%d")
    date_dir = IMAGE_DIR / date_str
    date_dir.mkdir(exist_ok=True)

    print(f"  - 더미 이미지 생성 중: {date_str} ({count}개)")

    # 대표 이미지만 생성 (모든 데이터가 같은 이미지 사용)
    left_img = create_dummy_pcb_image(is_front=True)
    right_img = create_dummy_pcb_image(is_front=False)

    left_path = date_dir / "left_sample.jpg"
    right_path = date_dir / "right_sample.jpg"

    cv2.imwrite(str(left_path), left_img)
    cv2.imwrite(str(right_path), right_img)

    print(f"    ✓ 대표 이미지 생성 완료: {left_path}")


def insert_inspections(connection, inspections):
    """검사 데이터 DB 삽입"""
    sql = """
    INSERT INTO inspections (
        serial_number, product_code, qr_data, qr_detected, serial_detected,
        decision, missing_count, position_error_count, extra_count, correct_count,
        missing_components, position_errors, extra_components,
        yolo_detections, detection_count, avg_confidence,
        inference_time_ms, verification_time_ms, total_time_ms,
        left_image_path, right_image_path, image_width, image_height,
        camera_id, client_ip, server_version, inspection_time
    ) VALUES (
        %(serial_number)s, %(product_code)s, %(qr_data)s, %(qr_detected)s, %(serial_detected)s,
        %(decision)s, %(missing_count)s, %(position_error_count)s, %(extra_count)s, %(correct_count)s,
        %(missing_components)s, %(position_errors)s, %(extra_components)s,
        %(yolo_detections)s, %(detection_count)s, %(avg_confidence)s,
        %(inference_time_ms)s, %(verification_time_ms)s, %(total_time_ms)s,
        %(left_image_path)s, %(right_image_path)s, %(image_width)s, %(image_height)s,
        %(camera_id)s, %(client_ip)s, %(server_version)s, %(inspection_time)s
    )
    """

    with connection.cursor() as cursor:
        cursor.executemany(sql, inspections)
    connection.commit()


def main():
    print("=" * 60)
    print("더미 검사 데이터 생성 시작")
    print("=" * 60)

    # DB 연결
    connection = pymysql.connect(**DB_CONFIG)
    print(f"✓ DB 연결 성공: {DB_CONFIG['host']}")

    # 12월 1일부터 9일까지
    start_date = datetime(2025, 12, 1)

    for day_offset in range(9):  # 0~8 (12월 1일 ~ 9일)
        current_date = start_date + timedelta(days=day_offset)
        date_str = current_date.strftime("%Y-%m-%d")

        # 하루당 검사 수 (900 ± 50)
        target_count = random.randint(850, 950)

        # 불량률 (20 ± 8%)
        defect_rate = random.uniform(12, 28)

        print(f"\n[{date_str}]")
        print(f"  - 목표 검사 수: {target_count}개")
        print(f"  - 불량률: {defect_rate:.1f}%")

        # 더미 이미지 생성
        create_dummy_images_for_date(current_date, target_count)

        # 검사 데이터 생성
        inspections = generate_inspection_data(current_date, target_count, defect_rate)

        # DB 삽입
        insert_inspections(connection, inspections)
        print(f"  ✓ DB 삽입 완료: {len(inspections)}개")

    connection.close()

    print("\n" + "=" * 60)
    print("완료!")
    print("=" * 60)
    print(f"\n더미 이미지 위치: {IMAGE_DIR}")
    print(f"윈도우 이미지 경로: {WINDOWS_IMAGE_BASE}")
    print("\n주의: 윈도우 PC에 C:\\PCBInspection\\Images 폴더를 생성하고")
    print(f"     {IMAGE_DIR} 의 이미지를 복사하세요.")


if __name__ == "__main__":
    main()
