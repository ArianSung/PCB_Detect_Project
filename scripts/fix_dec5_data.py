#!/usr/bin/env python3
"""12월 5일 데이터만 재생성"""

import pymysql
import random
import json
from datetime import datetime
import sys
import os

# 부모 디렉토리의 함수 임포트
sys.path.insert(0, os.path.dirname(__file__))

DB_CONFIG = {
    'host': '100.80.24.53',
    'user': 'pcb_admin',
    'password': '1234',
    'database': 'pcb_inspection',
    'charset': 'utf8mb4'
}

PRODUCT_CODES = ['FT', 'RS', 'BC']
SERIAL_PREFIXES = {'FT': 'MBFT', 'RS': 'MBRS', 'BC': 'MBBC'}
WINDOWS_IMAGE_BASE = "C:\\PCBInspection\\Images"


def generate_serial_number(product_code):
    prefix = SERIAL_PREFIXES[product_code]
    number = random.randint(10000000, 99999999)
    return f"{prefix}{number}"


def generate_inspection_data(inspection_date, target_count, defect_rate):
    inspections = []

    for i in range(target_count):
        hour = random.randint(0, 23)
        minute = random.randint(0, 59)
        second = random.randint(0, 59)
        inspection_time = inspection_date.replace(hour=hour, minute=minute, second=second)

        product_code = random.choice(PRODUCT_CODES)
        serial_number = generate_serial_number(product_code)

        is_defect = random.random() < (defect_rate / 100)

        if is_defect:
            decision = random.choices(['missing', 'position_error', 'discard'], weights=[50, 40, 10])[0]
        else:
            decision = 'normal'

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
        else:
            missing_count = random.randint(3, 8)
            position_error_count = random.randint(5, 10)
            extra_count = random.randint(0, 2)
            correct_count = random.randint(10, 20)

        detection_count = correct_count + position_error_count + extra_count

        date_str = inspection_date.strftime("%Y%m%d")
        img_filename = f"{serial_number}_{i:04d}"
        left_image_path = f"{WINDOWS_IMAGE_BASE}\\{date_str}\\left_{img_filename}.jpg"
        right_image_path = f"{WINDOWS_IMAGE_BASE}\\{date_str}\\right_{img_filename}.jpg"

        yolo_detections = json.dumps([
            {"class": f"component_{j}", "confidence": round(random.uniform(0.85, 0.99), 3),
             "bbox": [random.randint(0, 640), random.randint(0, 480), random.randint(50, 100), random.randint(50, 100)]}
            for j in range(detection_count)
        ])

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
            'missing_components': None,
            'position_errors': None,
            'extra_components': None,
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


def insert_inspections(connection, inspections):
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


connection = pymysql.connect(**DB_CONFIG)
current_date = datetime(2025, 12, 5)
target_count = random.randint(850, 950)
defect_rate = random.uniform(12, 28)

print(f"12월 5일 데이터 재생성: {target_count}개, 불량률 {defect_rate:.1f}%")
inspections = generate_inspection_data(current_date, target_count, defect_rate)
insert_inspections(connection, inspections)
connection.close()
print("완료!")
