# Flask 웹서버 기반 PCB 제품별 검증 시스템 구축 가이드 v3.0

## 개요

이 가이드는 **Product Verification Architecture (v3.0)**를 사용하여 PCB 제품별 부품 위치를 검증하는 Flask 추론 서버 구축 방법을 설명합니다.

**v3.0 핵심 변경사항**:
- 이중 YOLO 모델 → 단일 YOLO 모델 (부품 검출만)
- 융합 로직 → 제품 식별 (시리얼 + QR) + 부품 위치 검증
- 양면 동시 촬영 → 순차 촬영 (뒷면 먼저 → 앞면)
- component_defects, solder_defects → missing_components, position_errors

---

## 시스템 구성

### 하드웨어 구성

- **추론 서버 (GPU PC)**:
  - GPU: NVIDIA RTX 4080 Super (16GB VRAM)
  - **AI 모델**: YOLOv11l (Large) - 부품 검출 전용
  - 위치: 원격지 (같은 도시 내)
  - Flask 서버 실행: http://100.x.x.x:5000 (Tailscale VPN)

- **라즈베리파이 1**: 좌측 웹캠 (앞면 - 부품) + GPIO 제어
- **라즈베리파이 2**: 우측 웹캠 (뒷면 - 시리얼+QR) 전용
- **Windows PC**: C# WinForms 모니터링 앱

- **네트워크**: Tailscale VPN 메시 네트워크 (100.x.x.x)

**참고 문서**:
- 라즈베리파이 설정: `RaspberryPi_Setup.md`
- 데이터베이스 설계: `MySQL_Database_Design.md`
- 컴포넌트 검증 로직: `server/component_verification.py`

---

## 시스템 처리 흐름

```
1. 뒷면 처리 (우측 카메라)
   └─ 시리얼 넘버 OCR (EasyOCR)
   └─ QR 코드 스캔 (pyzbar)
   └─ 제품 코드 추출 (MBXX12345678 → XX)
   └─ 제품 DB 조회 (product_code)

2. 앞면 처리 (좌측 카메라)
   └─ PCB 정렬 (Homography)
   └─ YOLO 부품 검출 (YOLOv11l)
   └─ 제품별 기준 위치 로드 (product_components 테이블)
   └─ 부품 위치 검증 (ComponentVerifier)
       ├─ 누락 부품 검출 (Missing)
       ├─ 위치 오류 검출 (Position Error, 20px 임계값)
       └─ 추가 부품 검출 (Extra)

3. 최종 판정
   └─ normal: 정상 (모든 부품 정상 위치)
   └─ missing: 부품 누락 (3개 이상)
   └─ position_error: 위치 오류 (5개 이상)
   └─ discard: 폐기 (누락 + 위치 오류 합계 7개 이상)

4. 결과 저장 (MySQL)
   └─ inspections 테이블 INSERT
   └─ 트리거 자동 실행 → 집계 테이블 UPDATE
```

---

## Phase 1: Flask 추론 서버 구축

### 1-1. 필수 패키지 설치

```bash
# 가상환경 활성화
conda activate pcb_defect

# Flask 및 관련 패키지
pip install flask flask-cors
pip install flask-socketio python-socketio

# 컴퓨터 비전
pip install pillow opencv-python-headless

# AI 모델
pip install torch torchvision --index-url https://download.pytorch.org/whl/cu118
pip install ultralytics  # YOLOv11

# OCR 및 QR 코드
pip install easyocr
pip install pyzbar

# 데이터베이스
pip install pymysql

# 기타
pip install requests numpy scipy python-dotenv
```

### 1-2. 프로젝트 폴더 구조

```
~/work_project/
├── server/                         # Flask 추론 서버 (GPU PC)
│   ├── app.py                      # Flask 메인 애플리케이션 ⭐
│   ├── db_manager.py               # 데이터베이스 매니저
│   ├── pcb_alignment.py            # PCB 정렬 (Homography)
│   ├── component_verification.py  # 부품 위치 검증 ⭐
│   ├── serial_detector.py          # 시리얼 넘버 OCR
│   ├── qr_detector.py              # QR 코드 스캔
│   ├── product_database.py         # 제품 DB 조회
│   └── .env                        # 환경 변수 (DB 설정)
│
├── models/                         # 학습된 모델 파일
│   └── yolo11l_component_best.pt   # 부품 검출 모델 (단일)
│
├── data/
│   └── custom_pcb/                 # 커스텀 데이터셋 (3개 제품)
│       └── data.yaml
│
├── database/                       # MySQL 스키마
│   ├── schema_v3.0_product_verification.sql
│   ├── triggers_v3.0.sql
│   └── events_v3.0.sql
│
└── configs/                        # 설정 파일
    ├── server_config.yaml
    └── camera_config.yaml
```

---

## Phase 2: 핵심 모듈 구현

### 2-1. 데이터베이스 매니저 (db_manager.py)

```python
import pymysql
import logging
from datetime import datetime
import json

logger = logging.getLogger(__name__)

class DatabaseManager:
    """MySQL 데이터베이스 관리 클래스"""

    def __init__(self, host, port, user, password, database):
        """
        Args:
            host (str): MySQL 호스트
            port (int): MySQL 포트
            user (str): 사용자명
            password (str): 비밀번호
            database (str): 데이터베이스명
        """
        self.config = {
            'host': host,
            'port': port,
            'user': user,
            'password': password,
            'database': database,
            'charset': 'utf8mb4',
            'cursorclass': pymysql.cursors.DictCursor
        }
        self._test_connection()

    def _test_connection(self):
        """데이터베이스 연결 테스트"""
        try:
            conn = pymysql.connect(**self.config)
            conn.close()
            logger.info("✅ MySQL 데이터베이스 연결 성공")
        except Exception as e:
            logger.error(f"❌ MySQL 데이터베이스 연결 실패: {e}")
            raise

    def get_product_by_serial(self, serial_number):
        """
        시리얼 넘버로 제품 정보 조회

        Args:
            serial_number (str): 시리얼 넘버 (예: MBFT12345678)

        Returns:
            dict: 제품 정보 또는 None
        """
        try:
            # 시리얼 넘버에서 제품 코드 추출 (3-4번째 문자)
            if len(serial_number) < 4:
                logger.error(f"시리얼 넘버 형식 오류: {serial_number}")
                return None

            # MBFT → FT, MBRS → RS
            product_code = serial_number[2:4]

            conn = pymysql.connect(**self.config)
            try:
                with conn.cursor() as cursor:
                    sql = """SELECT product_code, product_name, description,
                                    component_count, qr_url_template
                             FROM products
                             WHERE product_code = %s"""
                    cursor.execute(sql, (product_code,))
                    result = cursor.fetchone()
                    return result
            finally:
                conn.close()

        except Exception as e:
            logger.error(f"제품 조회 실패: {e}")
            return None

    def get_product_components(self, product_code):
        """
        제품별 부품 배치 기준 정보 조회

        Args:
            product_code (str): 제품 코드 (예: FT, RS, BC)

        Returns:
            list: 부품 배치 기준 리스트
                [
                    {
                        'class_name': str,
                        'center': [cx, cy],
                        'bbox': [x1, y1, x2, y2],
                        'tolerance_px': float,
                        'confidence': 1.0  # 기준 데이터는 신뢰도 1.0
                    },
                    ...
                ]
        """
        try:
            conn = pymysql.connect(**self.config)
            try:
                with conn.cursor() as cursor:
                    sql = """SELECT component_class, center_x, center_y,
                                    bbox_x1, bbox_y1, bbox_x2, bbox_y2,
                                    tolerance_px
                             FROM product_components
                             WHERE product_code = %s
                             ORDER BY component_class"""
                    cursor.execute(sql, (product_code,))
                    rows = cursor.fetchall()

                    # ComponentVerifier 형식으로 변환
                    components = []
                    for row in rows:
                        components.append({
                            'class_name': row['component_class'],
                            'center': [row['center_x'], row['center_y']],
                            'bbox': [
                                row['bbox_x1'], row['bbox_y1'],
                                row['bbox_x2'], row['bbox_y2']
                            ],
                            'tolerance_px': row['tolerance_px'],
                            'confidence': 1.0  # 기준 데이터
                        })

                    logger.info(f"제품 {product_code} 부품 배치 기준 로드: {len(components)}개")
                    return components

            finally:
                conn.close()

        except Exception as e:
            logger.error(f"부품 배치 기준 조회 실패: {e}")
            return []

    def save_inspection_result(self, inspection_data):
        """
        검사 결과 저장

        Args:
            inspection_data (dict): 검사 결과 데이터
                {
                    'serial_number': str,
                    'product_code': str,
                    'qr_data': str,
                    'qr_detected': bool,
                    'serial_detected': bool,
                    'decision': str,  # normal/missing/position_error/discard
                    'missing_count': int,
                    'position_error_count': int,
                    'extra_count': int,
                    'correct_count': int,
                    'missing_components': list (JSON),
                    'position_errors': list (JSON),
                    'extra_components': list (JSON),
                    'yolo_detections': list (JSON),
                    'detection_count': int,
                    'avg_confidence': float,
                    'inference_time_ms': float,
                    'verification_time_ms': float,
                    'total_time_ms': float,
                    'left_image_path': str,
                    'right_image_path': str,
                    'image_width': int,
                    'image_height': int,
                    'camera_id': str,
                    'client_ip': str
                }

        Returns:
            int: 삽입된 레코드 ID 또는 None
        """
        try:
            conn = pymysql.connect(**self.config)
            try:
                with conn.cursor() as cursor:
                    sql = """INSERT INTO inspections
                             (serial_number, product_code, qr_data,
                              qr_detected, serial_detected,
                              decision, missing_count, position_error_count,
                              extra_count, correct_count,
                              missing_components, position_errors, extra_components,
                              yolo_detections, detection_count, avg_confidence,
                              inference_time_ms, verification_time_ms, total_time_ms,
                              left_image_path, right_image_path,
                              image_width, image_height,
                              camera_id, client_ip)
                             VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s,
                                     %s, %s, %s, %s, %s, %s, %s, %s, %s, %s,
                                     %s, %s, %s, %s, %s)"""

                    # JSON 변환
                    missing_components = json.dumps(inspection_data.get('missing_components'))
                    position_errors = json.dumps(inspection_data.get('position_errors'))
                    extra_components = json.dumps(inspection_data.get('extra_components'))
                    yolo_detections = json.dumps(inspection_data.get('yolo_detections'))

                    cursor.execute(sql, (
                        inspection_data['serial_number'],
                        inspection_data['product_code'],
                        inspection_data.get('qr_data'),
                        inspection_data.get('qr_detected', False),
                        inspection_data.get('serial_detected', False),
                        inspection_data['decision'],
                        inspection_data['missing_count'],
                        inspection_data['position_error_count'],
                        inspection_data['extra_count'],
                        inspection_data['correct_count'],
                        missing_components,
                        position_errors,
                        extra_components,
                        yolo_detections,
                        inspection_data['detection_count'],
                        inspection_data['avg_confidence'],
                        inspection_data.get('inference_time_ms'),
                        inspection_data.get('verification_time_ms'),
                        inspection_data.get('total_time_ms'),
                        inspection_data.get('left_image_path'),
                        inspection_data.get('right_image_path'),
                        inspection_data.get('image_width'),
                        inspection_data.get('image_height'),
                        inspection_data.get('camera_id'),
                        inspection_data.get('client_ip')
                    ))

                    conn.commit()
                    inspection_id = cursor.lastrowid
                    logger.info(f"✅ 검사 결과 저장 완료: ID={inspection_id}")
                    return inspection_id

            finally:
                conn.close()

        except Exception as e:
            logger.error(f"❌ 검사 결과 저장 실패: {e}")
            return None
```

---

### 2-2. 컴포넌트 검증 모듈 (component_verification.py)

이 파일은 이미 `/home/sys1041/work_project/server/component_verification.py`에 존재합니다.

**주요 기능**:
- `ComponentVerifier` 클래스: 부품 위치 검증
- `verify_components()`: 검출 결과와 기준 위치 비교
- `is_critical_defect()`: 치명적 불량 판정 (누락 3개, 위치 오류 5개, 합계 7개)
- `generate_report()`: 검증 결과 리포트 생성

**참조**: `server/component_verification.py:23-369`

---

### 2-3. Flask 메인 애플리케이션 (app.py)

```python
from flask import Flask, request, jsonify
from flask_cors import CORS
import base64
import cv2
import numpy as np
from datetime import datetime
import logging
import os
from pathlib import Path

# 로컬 모듈
from db_manager import DatabaseManager
from pcb_alignment import PCBAligner
from component_verification import ComponentVerifier
from serial_detector import SerialDetector  # 시리얼 넘버 OCR
from qr_detector import QRDetector          # QR 코드 스캔

# Flask 앱 초기화
app = Flask(__name__)
CORS(app)

# 로깅 설정
logging.basicConfig(
    level=logging.INFO,
    format='[%(asctime)s] [%(levelname)s] [Flask-Server] - %(message)s'
)
logger = logging.getLogger(__name__)

# 환경 변수에서 DB 설정 읽기
DB_CONFIG = {
    'host': os.getenv('DB_HOST', 'localhost'),
    'port': int(os.getenv('DB_PORT', 3306)),
    'user': os.getenv('DB_USER', 'root'),
    'password': os.getenv('DB_PASSWORD', 'your_password'),
    'database': os.getenv('DB_NAME', 'pcb_inspection')
}

# 데이터베이스 매니저 초기화
db = DatabaseManager(**DB_CONFIG)

# YOLO 모델 로드 (부품 검출 전용)
try:
    from ultralytics import YOLO
    model_path = 'models/yolo11l_component_best.pt'
    yolo_model = YOLO(model_path)
    logger.info(f"✅ YOLO 모델 로드 완료: {model_path}")
except Exception as e:
    logger.error(f"⚠️  YOLO 모델 로드 실패: {e}")
    yolo_model = None

# PCB 정렬 및 검증 모듈 초기화
aligner = PCBAligner()
serial_detector = SerialDetector()
qr_detector = QRDetector()


@app.route('/health', methods=['GET'])
def health_check():
    """서버 상태 확인"""
    return jsonify({
        'status': 'ok',
        'timestamp': datetime.now().isoformat(),
        'model_loaded': yolo_model is not None,
        'database': 'connected'
    })


@app.route('/predict', methods=['POST'])
def predict():
    """
    PCB 검사 API (v3.0 Product Verification)

    Request:
        {
            "left_frame": "base64_encoded_image",   # 앞면 (부품)
            "right_frame": "base64_encoded_image",  # 뒷면 (시리얼+QR)
            "client_ip": "100.x.x.x"
        }

    Response:
        {
            "success": true,
            "decision": "normal",  # normal/missing/position_error/discard
            "serial_number": "MBFT12345678",
            "product_code": "FT",
            "product_name": "Fast Type PCB",
            "missing_count": 0,
            "position_error_count": 0,
            "extra_count": 0,
            "correct_count": 25,
            "total_time_ms": 120.5,
            "timestamp": "2025-11-28T10:30:00"
        }
    """
    start_time = datetime.now()

    try:
        # 1. 요청 데이터 파싱
        data = request.get_json()
        left_frame_b64 = data.get('left_frame')   # 앞면 (부품)
        right_frame_b64 = data.get('right_frame') # 뒷면 (시리얼+QR)
        client_ip = data.get('client_ip', request.remote_addr)

        # Base64 디코딩
        left_img = decode_base64_image(left_frame_b64)
        right_img = decode_base64_image(right_frame_b64)

        # 2. 뒷면 처리: 시리얼 넘버 + QR 코드
        serial_result = serial_detector.detect(right_img)
        qr_result = qr_detector.detect(right_img)

        serial_number = serial_result.get('serial_number')
        qr_data = qr_result.get('data')

        if not serial_number:
            return jsonify({
                'success': False,
                'error': 'Serial number not detected',
                'message': '시리얼 넘버를 검출할 수 없습니다.'
            }), 400

        # 3. 제품 정보 조회
        product_info = db.get_product_by_serial(serial_number)
        if not product_info:
            return jsonify({
                'success': False,
                'error': 'Product not found',
                'message': f'제품 코드를 찾을 수 없습니다: {serial_number}'
            }), 404

        product_code = product_info['product_code']
        product_name = product_info['product_name']

        # 4. 제품별 부품 배치 기준 로드
        reference_components = db.get_product_components(product_code)
        if not reference_components:
            return jsonify({
                'success': False,
                'error': 'Reference components not found',
                'message': f'제품 {product_code}의 부품 배치 기준이 없습니다.'
            }), 404

        # 5. 앞면 처리: PCB 정렬
        aligned_img = aligner.align(left_img)

        # 6. YOLO 부품 검출
        inference_start = datetime.now()
        results = yolo_model(aligned_img, conf=0.25)
        inference_time = (datetime.now() - inference_start).total_seconds() * 1000

        # 검출 결과 변환
        detected_components = []
        for result in results:
            boxes = result.boxes
            for box in boxes:
                x1, y1, x2, y2 = box.xyxy[0].cpu().numpy()
                cx = (x1 + x2) / 2
                cy = (y1 + y2) / 2
                conf = float(box.conf[0])
                cls_id = int(box.cls[0])
                cls_name = yolo_model.names[cls_id]

                detected_components.append({
                    'class_name': cls_name,
                    'center': [cx, cy],
                    'bbox': [x1, y1, x2, y2],
                    'confidence': conf
                })

        # 7. 부품 위치 검증
        verification_start = datetime.now()
        verifier = ComponentVerifier(
            reference_components=reference_components,
            position_threshold=20.0,  # 20px 허용 오차
            confidence_threshold=0.25
        )
        verification_result = verifier.verify_components(detected_components)
        verification_time = (datetime.now() - verification_start).total_seconds() * 1000

        # 8. 최종 판정
        is_critical, reason = verifier.is_critical_defect(verification_result)

        if is_critical:
            decision = determine_decision(verification_result)
        else:
            if verification_result['summary']['missing_count'] > 0 or \
               verification_result['summary']['misplaced_count'] > 0:
                decision = 'position_error'
            else:
                decision = 'normal'

        # 9. 검사 결과 저장
        total_time = (datetime.now() - start_time).total_seconds() * 1000

        inspection_data = {
            'serial_number': serial_number,
            'product_code': product_code,
            'qr_data': qr_data,
            'qr_detected': qr_result.get('detected', False),
            'serial_detected': serial_result.get('detected', False),
            'decision': decision,
            'missing_count': verification_result['summary']['missing_count'],
            'position_error_count': verification_result['summary']['misplaced_count'],
            'extra_count': verification_result['summary']['extra_count'],
            'correct_count': verification_result['summary']['correct_count'],
            'missing_components': verification_result['missing'],
            'position_errors': verification_result['misplaced'],
            'extra_components': verification_result['extra'],
            'yolo_detections': detected_components,
            'detection_count': len(detected_components),
            'avg_confidence': np.mean([c['confidence'] for c in detected_components]) if detected_components else 0.0,
            'inference_time_ms': inference_time,
            'verification_time_ms': verification_time,
            'total_time_ms': total_time,
            'left_image_path': None,  # 이미지 저장 시 경로 입력
            'right_image_path': None,
            'image_width': left_img.shape[1],
            'image_height': left_img.shape[0],
            'camera_id': 'left',
            'client_ip': client_ip
        }

        inspection_id = db.save_inspection_result(inspection_data)

        # 10. 응답 반환
        return jsonify({
            'success': True,
            'inspection_id': inspection_id,
            'decision': decision,
            'serial_number': serial_number,
            'product_code': product_code,
            'product_name': product_name,
            'missing_count': verification_result['summary']['missing_count'],
            'position_error_count': verification_result['summary']['misplaced_count'],
            'extra_count': verification_result['summary']['extra_count'],
            'correct_count': verification_result['summary']['correct_count'],
            'total_time_ms': total_time,
            'timestamp': datetime.now().isoformat()
        })

    except Exception as e:
        logger.error(f"검사 처리 중 오류: {e}", exc_info=True)
        return jsonify({
            'success': False,
            'error': str(e)
        }), 500


def determine_decision(verification_result):
    """
    최종 판정 결정

    Args:
        verification_result (dict): 검증 결과

    Returns:
        str: normal/missing/position_error/discard
    """
    missing_count = verification_result['summary']['missing_count']
    misplaced_count = verification_result['summary']['misplaced_count']
    total_issues = missing_count + misplaced_count

    # 판정 기준
    if missing_count >= 3:
        return 'missing'
    elif misplaced_count >= 5:
        return 'position_error'
    elif total_issues >= 7:
        return 'discard'
    else:
        return 'normal'


def decode_base64_image(base64_str):
    """Base64 이미지 디코딩"""
    img_data = base64.b64decode(base64_str)
    np_arr = np.frombuffer(img_data, np.uint8)
    img = cv2.imdecode(np_arr, cv2.IMREAD_COLOR)
    return img


if __name__ == '__main__':
    # 서버 실행
    app.run(host='0.0.0.0', port=5000, debug=False)
```

---

## Phase 3: 서버 실행

### 3-1. 환경 변수 설정 (.env)

```bash
# server/.env

# MySQL 데이터베이스 설정
DB_HOST=localhost
DB_PORT=3306
DB_USER=flask_server
DB_PASSWORD=your_strong_password
DB_NAME=pcb_inspection

# Flask 설정
FLASK_ENV=production
FLASK_DEBUG=False
```

### 3-2. 서버 실행

```bash
# 가상환경 활성화
conda activate pcb_defect

# Flask 서버 실행
cd server
python app.py

# 또는 Flask CLI 사용
flask --app app run --host=0.0.0.0 --port=5000
```

**실행 로그 예시**:
```
[2025-11-28 10:00:00] [INFO] [Flask-Server] - ✅ MySQL 데이터베이스 연결 성공
[2025-11-28 10:00:01] [INFO] [Flask-Server] - ✅ YOLO 모델 로드 완료: models/yolo11l_component_best.pt
[2025-11-28 10:00:01] [INFO] [Flask-Server] - 서버 시작: http://0.0.0.0:5000
```

---

## Phase 4: 테스트

### 4-1. 헬스 체크

```bash
curl http://localhost:5000/health
```

**응답**:
```json
{
  "status": "ok",
  "timestamp": "2025-11-28T10:00:00",
  "model_loaded": true,
  "database": "connected"
}
```

### 4-2. 검사 API 테스트

```python
import requests
import base64
import cv2

# 이미지 로드
left_img = cv2.imread('test_images/left_front.jpg')
right_img = cv2.imread('test_images/right_back.jpg')

# Base64 인코딩
_, left_buffer = cv2.imencode('.jpg', left_img)
_, right_buffer = cv2.imencode('.jpg', right_img)

left_b64 = base64.b64encode(left_buffer).decode('utf-8')
right_b64 = base64.b64encode(right_buffer).decode('utf-8')

# API 호출
url = 'http://localhost:5000/predict'
data = {
    'left_frame': left_b64,
    'right_frame': right_b64,
    'client_ip': '100.64.1.2'
}

response = requests.post(url, json=data)
print(response.json())
```

**응답 예시**:
```json
{
  "success": true,
  "inspection_id": 1234,
  "decision": "normal",
  "serial_number": "MBFT12345678",
  "product_code": "FT",
  "product_name": "Fast Type PCB",
  "missing_count": 0,
  "position_error_count": 0,
  "extra_count": 0,
  "correct_count": 25,
  "total_time_ms": 120.5,
  "timestamp": "2025-11-28T10:30:00"
}
```

---

## Phase 5: 성능 최적화

### 5-1. YOLO 모델 최적화

```python
# FP16 추론 (VRAM 절약 + 속도 향상)
results = yolo_model(image, half=True)

# 배치 추론 (여러 이미지 동시 처리)
results = yolo_model([img1, img2, img3])
```

### 5-2. 데이터베이스 연결 풀

```python
from pymysql.connections import Connection
from queue import Queue

class ConnectionPool:
    def __init__(self, config, pool_size=5):
        self.config = config
        self.pool = Queue(maxsize=pool_size)
        for _ in range(pool_size):
            conn = pymysql.connect(**config)
            self.pool.put(conn)

    def get_connection(self):
        return self.pool.get()

    def release_connection(self, conn):
        self.pool.put(conn)
```

---

## Phase 6: 배포 및 운영

### 6-1. systemd 서비스 등록

```bash
# /etc/systemd/system/flask-pcb-server.service

[Unit]
Description=Flask PCB Inspection Server
After=network.target mysql.service

[Service]
Type=simple
User=pcb_user
WorkingDirectory=/home/pcb_user/work_project/server
Environment="PATH=/home/pcb_user/miniconda3/envs/pcb_defect/bin"
ExecStart=/home/pcb_user/miniconda3/envs/pcb_defect/bin/python app.py
Restart=always
RestartSec=10

[Install]
WantedBy=multi-user.target
```

**서비스 실행**:
```bash
sudo systemctl daemon-reload
sudo systemctl start flask-pcb-server
sudo systemctl enable flask-pcb-server
sudo systemctl status flask-pcb-server
```

### 6-2. 로그 관리

```bash
# journalctl로 로그 확인
sudo journalctl -u flask-pcb-server -f

# 로그 파일로 저장
sudo journalctl -u flask-pcb-server > /var/log/flask-pcb-server.log
```

---

## 트러블슈팅

### 문제 1: YOLO 모델 로드 실패

**증상**: `FileNotFoundError: models/yolo11l_component_best.pt`

**해결**:
```bash
# 모델 파일 경로 확인
ls -la models/

# 심볼릭 링크 생성 (필요 시)
ln -s ../runs/detect/train/weights/best.pt models/yolo11l_component_best.pt
```

### 문제 2: MySQL 연결 실패

**증상**: `pymysql.err.OperationalError: (2003, "Can't connect to MySQL server")`

**해결**:
```bash
# MySQL 실행 확인
sudo systemctl status mysql

# 방화벽 확인
sudo ufw status

# MySQL 포트 개방
sudo ufw allow from 100.64.1.1 to any port 3306
```

### 문제 3: OCR 성능 저하

**증상**: 시리얼 넘버 검출 실패

**해결**:
- 이미지 전처리 개선 (이진화, 노이즈 제거)
- EasyOCR GPU 모드 활성화
- 시리얼 넘버 영역 ROI 지정

---

## 관련 문서

1. **PCB_Defect_Detection_Project.md** - 전체 시스템 아키텍처
2. **MySQL_Database_Design.md** - 데이터베이스 스키마 v3.0
3. **RaspberryPi_Setup.md** - 라즈베리파이 클라이언트
4. **CSharp_WinForms_Design_Specification.md** - WinForms 모니터링 앱
5. **Dataset_Guide.md** - 커스텀 데이터셋 가이드

---

**작성일**: 2025-11-28
**버전**: 3.0 (Product Verification Architecture)
**주요 변경사항**:
- 이중 YOLO 모델 → 단일 YOLO 모델 (부품 검출 전용)
- 융합 로직 제거 → 제품 식별 + 부품 위치 검증
- 시리얼 넘버 OCR + QR 코드 스캔 추가
- ComponentVerifier 기반 위치 검증
- 4단계 판정: normal/missing/position_error/discard
- 트리거 기반 자동 집계 시스템 연동
