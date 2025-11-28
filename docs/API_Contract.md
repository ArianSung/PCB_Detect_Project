# API 계약 명세서 (API Contract)

PCB 불량 검사 시스템의 Flask 서버 REST API 공식 명세서입니다.
**모든 팀원은 이 명세서를 기준으로 개발해야 하며, 변경 시 전체 팀 합의가 필요합니다.**

---

## 📌 중요 공지

### API 버전 관리
- **현재 버전**: v3.0.0 ⭐ (Product Verification Architecture)
- **Base URL**: `http://{SERVER_IP}:5000`
- **마지막 업데이트**: 2025-11-28
- **주요 변경**: 이중 YOLO 모델 → 제품별 부품 위치 검증 시스템

### API 변경 규칙
1. **하위 호환성 유지**: 기존 API는 삭제하지 않고 deprecated 처리
2. **버전 업그레이드**: 큰 변경 시 `/api/v2` 생성
3. **변경 공지**: 최소 1주일 전 팀 전체 공지
4. **테스트 필수**: API 변경 시 계약 테스트 실행

---

## 🔗 API 엔드포인트 목록

| 엔드포인트 | 메서드 | 설명 | 담당 팀 |
|------------|--------|------|---------|
| `/health` | GET | 서버 상태 확인 | Flask |
| `/predict` | POST | PCB 불량 검사 (제품별 부품 위치 검증) ⭐ | Flask + AI |
| `/history` | GET | 검사 이력 조회 | Flask + DB |
| `/history/<id>` | GET | 특정 검사 결과 상세 조회 | Flask + DB |
| `/statistics` | GET | 통계 데이터 조회 (제품별, 시간별) ⭐ | Flask + DB |
| `/export` | GET | Excel 내보내기용 데이터 | Flask + DB |

**참고**: `/predict_dual` 엔드포인트는 v3.0에서 제거되었습니다 (이중 모델 아키텍처 → 제품별 검증 전환)

---

## 📡 API 상세 명세

### 1. 서버 상태 확인

**엔드포인트**: `/health`
**메서드**: `GET`
**설명**: Flask 서버 및 AI 모델 상태 확인

#### 요청 (Request)
```http
GET /health HTTP/1.1
Host: 100.64.1.1:5000
```

#### 응답 (Response)
**성공 (200 OK)**:
```json
{
  "status": "healthy",
  "server_time": "2025-11-28T14:30:00",
  "gpu_available": true,
  "models_loaded": {
    "yolo_model": true,
    "ocr_model": true
  },
  "model_info": {
    "yolo_model": "YOLOv11l - Custom PCB Components",
    "ocr_model": "EasyOCR - Serial Number Recognition",
    "products_loaded": ["FT", "RS", "BC"]
  },
  "database_connected": true,
  "version": "3.0.0"
}
```

**실패 (503 Service Unavailable)**:
```json
{
  "status": "unhealthy",
  "error": "YOLO model not loaded",
  "server_time": "2025-11-28T14:30:00"
}
```

---

### 2. PCB 불량 검사 (제품별 부품 위치 검증) ⭐

**엔드포인트**: `/predict`
**메서드**: `POST`
**설명**: PCB 이미지에서 Serial Number/QR Code 추출 → 제품 식별 → YOLO 부품 검출 → 기준 데이터와 위치 비교 → 불량 판정

**처리 흐름**:
```
[PCB 이미지] → [1. OCR/QR 인식] → [Serial Number: MBXX12345678]
                       ↓
            [2. 제품 식별: XX = FT/RS/BC]
                       ↓
            [3. YOLO 부품 검출] (YOLOv11l)
                       ↓
            [4. DB 기준 데이터 조회] (product_components 테이블)
                       ↓
            [5. ComponentVerifier 위치 비교]
                       ↓
            [6. 최종 판정] (normal/missing/position_error/discard)
```

#### 요청 (Request)
```http
POST /predict HTTP/1.1
Host: 100.64.1.1:5000
Content-Type: application/json

{
  "image": "base64_encoded_jpeg_string",
  "serial_number": "MBFT12345678",
  "qr_code": "MBFT12345678",
  "timestamp": "2025-11-28T14:30:00",
  "request_id": "uuid-v4-string"
}
```

**필드 설명:**
- `image` (string, 필수): Base64 인코딩된 JPEG 이미지 데이터
- `serial_number` (string, 선택): Serial Number (없으면 서버에서 OCR로 추출)
- `qr_code` (string, 선택): QR Code (없으면 서버에서 디코딩으로 추출)
- `timestamp` (string, 필수): ISO 8601 형식 타임스탬프
- `request_id` (string, 선택): 요청 추적용 UUID (없으면 서버 자동 생성)

**참고**: `serial_number`와 `qr_code` 모두 제공되지 않으면 서버가 자동으로 추출 시도

#### 응답 (Response)

**예시 1: 정상 PCB (200 OK)**:
```json
{
  "success": true,
  "request_id": "uuid-v4-string",
  "serial_number": "MBFT12345678",
  "product_code": "FT",
  "timestamp": "2025-11-28T14:30:00",
  "inference_time_ms": 95.2,
  "decision": "normal",
  "verification_result": {
    "missing_count": 0,
    "position_error_count": 0,
    "extra_count": 0,
    "expected_count": 12,
    "detected_count": 12,
    "match_rate": 100.0
  },
  "yolo_result": {
    "detections": [
      {
        "class_name": "resistor",
        "bbox": [120, 80, 160, 100],
        "confidence": 0.95,
        "center": [140, 90]
      }
    ],
    "detection_count": 12,
    "avg_confidence": 0.93
  },
  "missing_components": [],
  "position_errors": [],
  "extra_components": []
}
```

**예시 2: 부품 누락 (200 OK)**:
```json
{
  "success": true,
  "request_id": "uuid-v4-string",
  "serial_number": "MBRS87654321",
  "product_code": "RS",
  "timestamp": "2025-11-28T14:31:00",
  "inference_time_ms": 102.3,
  "decision": "missing",
  "verification_result": {
    "missing_count": 3,
    "position_error_count": 0,
    "extra_count": 0,
    "expected_count": 15,
    "detected_count": 12,
    "match_rate": 80.0
  },
  "yolo_result": {
    "detections": [],
    "detection_count": 12,
    "avg_confidence": 0.88
  },
  "missing_components": [
    {
      "component_class": "capacitor",
      "expected_center": [200, 150],
      "expected_bbox": [180, 130, 220, 170]
    },
    {
      "component_class": "resistor",
      "expected_center": [300, 250],
      "expected_bbox": [280, 230, 320, 270]
    },
    {
      "component_class": "IC",
      "expected_center": [400, 350],
      "expected_bbox": [370, 320, 430, 380]
    }
  ],
  "position_errors": [],
  "extra_components": []
}
```

**예시 3: 위치 오류 (200 OK)**:
```json
{
  "success": true,
  "request_id": "uuid-v4-string",
  "serial_number": "MBBC11111111",
  "product_code": "BC",
  "timestamp": "2025-11-28T14:32:00",
  "inference_time_ms": 98.7,
  "decision": "position_error",
  "verification_result": {
    "missing_count": 0,
    "position_error_count": 5,
    "extra_count": 0,
    "expected_count": 10,
    "detected_count": 10,
    "match_rate": 50.0
  },
  "yolo_result": {
    "detections": [],
    "detection_count": 10,
    "avg_confidence": 0.91
  },
  "missing_components": [],
  "position_errors": [
    {
      "component_class": "resistor",
      "expected_center": [150, 100],
      "detected_center": [175, 120],
      "distance": 32.0,
      "threshold": 20.0,
      "detected_bbox": [155, 100, 195, 140],
      "confidence": 0.89
    }
  ],
  "extra_components": []
}
```

**예시 4: 폐기 판정 (200 OK)**:
```json
{
  "success": true,
  "request_id": "uuid-v4-string",
  "serial_number": "MBFT99999999",
  "product_code": "FT",
  "timestamp": "2025-11-28T14:33:00",
  "inference_time_ms": 105.1,
  "decision": "discard",
  "verification_result": {
    "missing_count": 4,
    "position_error_count": 3,
    "extra_count": 1,
    "expected_count": 12,
    "detected_count": 9,
    "match_rate": 41.7
  },
  "yolo_result": {
    "detections": [],
    "detection_count": 9,
    "avg_confidence": 0.75
  },
  "missing_components": [],
  "position_errors": [],
  "extra_components": []
}
```

**실패 - Serial Number 인식 실패 (400 Bad Request)**:
```json
{
  "success": false,
  "error": "Serial number extraction failed",
  "message": "Could not extract serial number from image using OCR or QR code",
  "timestamp": "2025-11-28T14:30:00"
}
```

**실패 - 제품 정보 없음 (404 Not Found)**:
```json
{
  "success": false,
  "error": "Product not found",
  "message": "Product code 'XY' not found in database",
  "serial_number": "MBXY12345678",
  "timestamp": "2025-11-28T14:30:00"
}
```

**실패 - 서버 오류 (500 Internal Server Error)**:
```json
{
  "success": false,
  "error": "Inference failed",
  "message": "CUDA out of memory",
  "timestamp": "2025-11-28T14:30:00"
}
```

---

**판정 기준 (decision):**
- `"normal"`: 정상 - missing_count == 0 && position_error_count == 0
- `"missing"`: 부품 누락 - missing_count >= 3
- `"position_error"`: 위치 오류 - position_error_count >= 5
- `"discard"`: 폐기 - missing_count + position_error_count >= 7

**Serial Number 형식**: `MBXX12345678`
- `MB`: 브랜드 접두사 (고정)
- `XX`: 제품 코드 (FT, RS, BC)
- `12345678`: 일련번호 (8자리 숫자)

**제품 종류:**
- `FT`: Fast Type (빠른 유형)
- `RS`: Reliable Stable (안정적 유형)
- `BC`: Budget Compact (경제적 유형)

---

### 3. 검사 이력 조회 (제품별 필터링 지원) ⭐

**엔드포인트**: `/history`
**메서드**: `GET`
**설명**: PCB 검사 이력을 페이지네이션하여 조회 (제품별, 판정별, 날짜별 필터링)

#### 요청 (Request)
```http
GET /history?page=1&limit=20&product_code=FT&decision=all&start_date=2025-11-01&end_date=2025-11-28 HTTP/1.1
Host: 100.64.1.1:5000
```

**쿼리 파라미터:**
- `page` (int, 선택, 기본값: 1): 페이지 번호
- `limit` (int, 선택, 기본값: 20): 페이지당 항목 수 (최대 100)
- `product_code` (string, 선택, 기본값: "all"): 제품 필터 (`"all"`, `"FT"`, `"RS"`, `"BC"`)
- `decision` (string, 선택, 기본값: "all"): 판정 필터 (`"all"`, `"normal"`, `"missing"`, `"position_error"`, `"discard"`)
- `start_date` (string, 선택): 시작 날짜 (YYYY-MM-DD)
- `end_date` (string, 선택): 종료 날짜 (YYYY-MM-DD)

#### 응답 (Response)
```json
{
  "success": true,
  "page": 1,
  "limit": 20,
  "total_records": 152,
  "total_pages": 8,
  "filters": {
    "product_code": "FT",
    "decision": "all",
    "start_date": "2025-11-01",
    "end_date": "2025-11-28"
  },
  "records": [
    {
      "id": 152,
      "serial_number": "MBFT12345678",
      "product_code": "FT",
      "decision": "normal",
      "missing_count": 0,
      "position_error_count": 0,
      "detection_count": 12,
      "avg_confidence": 0.93,
      "inference_time_ms": 95.2,
      "inspection_time": "2025-11-28T14:30:00"
    },
    {
      "id": 151,
      "serial_number": "MBFT87654321",
      "product_code": "FT",
      "decision": "missing",
      "missing_count": 3,
      "position_error_count": 0,
      "detection_count": 9,
      "avg_confidence": 0.88,
      "inference_time_ms": 102.3,
      "inspection_time": "2025-11-28T14:29:50"
    }
  ]
}
```

---

### 4. 특정 검사 결과 상세 조회

**엔드포인트**: `/history/<id>`
**메서드**: `GET`
**설명**: 특정 검사 결과의 상세 정보 조회 (불량 상세 정보 포함)

#### 요청 (Request)
```http
GET /history/152 HTTP/1.1
Host: 100.64.1.1:5000
```

#### 응답 (Response)

**예시 1: 정상 PCB 상세**
```json
{
  "success": true,
  "record": {
    "id": 152,
    "serial_number": "MBFT12345678",
    "product_code": "FT",
    "decision": "normal",
    "missing_count": 0,
    "position_error_count": 0,
    "detection_count": 12,
    "avg_confidence": 0.93,
    "inference_time_ms": 95.2,
    "inspection_time": "2025-11-28T14:30:00",
    "missing_components": [],
    "position_errors": [],
    "extra_components": [],
    "yolo_detections": [
      {
        "class_name": "resistor",
        "bbox": [120, 80, 160, 100],
        "confidence": 0.95,
        "center": [140, 90]
      }
    ]
  }
}
```

**예시 2: 부품 누락 PCB 상세**
```json
{
  "success": true,
  "record": {
    "id": 151,
    "serial_number": "MBRS87654321",
    "product_code": "RS",
    "decision": "missing",
    "missing_count": 3,
    "position_error_count": 0,
    "detection_count": 12,
    "avg_confidence": 0.88,
    "inference_time_ms": 102.3,
    "inspection_time": "2025-11-28T14:29:50",
    "missing_components": [
      {
        "component_class": "capacitor",
        "expected_center": [200, 150],
        "expected_bbox": [180, 130, 220, 170]
      },
      {
        "component_class": "resistor",
        "expected_center": [300, 250],
        "expected_bbox": [280, 230, 320, 270]
      },
      {
        "component_class": "IC",
        "expected_center": [400, 350],
        "expected_bbox": [370, 320, 430, 380]
      }
    ],
    "position_errors": [],
    "extra_components": [],
    "yolo_detections": []
  }
}
```

**실패 (404 Not Found)**:
```json
{
  "success": false,
  "error": "Record not found",
  "message": "Inspection ID 999 does not exist",
  "timestamp": "2025-11-28T14:30:00"
}
```

---

### 5. 통계 데이터 조회 (제품별, 시간별 집계) ⭐

**엔드포인트**: `/statistics`
**메서드**: `GET`
**설명**: PCB 검사 통계 데이터 (제품별, 일별, 시간별, 월별 집계 테이블 활용)

#### 요청 (Request)
```http
GET /statistics?start_date=2025-11-01&end_date=2025-11-28&product_code=all&period=daily HTTP/1.1
Host: 100.64.1.1:5000
```

**쿼리 파라미터:**
- `start_date` (string, 필수): 시작 날짜 (YYYY-MM-DD)
- `end_date` (string, 필수): 종료 날짜 (YYYY-MM-DD)
- `product_code` (string, 선택, 기본값: "all"): 제품 필터 (`"all"`, `"FT"`, `"RS"`, `"BC"`)
- `period` (string, 선택, 기본값: "daily"): 집계 단위 (`"hourly"`, `"daily"`, `"monthly"`)

#### 응답 (Response)

**예시 1: 일별 통계 (전체 제품)**
```json
{
  "success": true,
  "period": {
    "start_date": "2025-11-01",
    "end_date": "2025-11-28",
    "aggregation": "daily"
  },
  "filters": {
    "product_code": "all"
  },
  "summary": {
    "total_inspections": 5420,
    "normal_count": 4850,
    "missing_count": 320,
    "position_error_count": 180,
    "discard_count": 70,
    "defect_rate": 10.5,
    "avg_inference_time_ms": 98.3
  },
  "by_product": {
    "FT": {
      "total": 2100,
      "normal": 1890,
      "missing": 120,
      "position_error": 60,
      "discard": 30,
      "defect_rate": 10.0
    },
    "RS": {
      "total": 2000,
      "normal": 1800,
      "missing": 110,
      "position_error": 70,
      "discard": 20,
      "defect_rate": 10.0
    },
    "BC": {
      "total": 1320,
      "normal": 1160,
      "missing": 90,
      "position_error": 50,
      "discard": 20,
      "defect_rate": 12.1
    }
  },
  "daily_statistics": [
    {
      "date": "2025-11-28",
      "total_inspections": 250,
      "normal_count": 220,
      "missing_count": 15,
      "position_error_count": 10,
      "discard_count": 5,
      "defect_rate": 12.0,
      "avg_inference_time_ms": 95.2
    },
    {
      "date": "2025-11-27",
      "total_inspections": 230,
      "normal_count": 205,
      "missing_count": 12,
      "position_error_count": 8,
      "discard_count": 5,
      "defect_rate": 10.9,
      "avg_inference_time_ms": 97.1
    }
  ]
}
```

**예시 2: 시간별 통계 (특정 제품)**
```json
{
  "success": true,
  "period": {
    "start_date": "2025-11-28",
    "end_date": "2025-11-28",
    "aggregation": "hourly"
  },
  "filters": {
    "product_code": "FT"
  },
  "summary": {
    "total_inspections": 120,
    "normal_count": 105,
    "missing_count": 8,
    "position_error_count": 5,
    "discard_count": 2,
    "defect_rate": 12.5,
    "avg_inference_time_ms": 93.1
  },
  "hourly_statistics": [
    {
      "hour": "2025-11-28 14:00:00",
      "product_code": "FT",
      "total_inspections": 15,
      "normal_count": 13,
      "missing_count": 1,
      "position_error_count": 1,
      "discard_count": 0,
      "defect_rate": 13.3,
      "avg_inference_time_ms": 95.2
    }
  ]
}
```

---

### 6. Excel 내보내기용 데이터

**엔드포인트**: `/export`
**메서드**: `GET`
**설명**: C# WinForms에서 Excel 내보내기를 위한 전체 데이터 조회

#### 요청 (Request)
```http
GET /export?start_date=2025-11-01&end_date=2025-11-28&product_code=all&format=json HTTP/1.1
Host: 100.64.1.1:5000
```

**쿼리 파라미터:**
- `start_date` (string, 필수): 시작 날짜 (YYYY-MM-DD)
- `end_date` (string, 필수): 종료 날짜 (YYYY-MM-DD)
- `product_code` (string, 선택, 기본값: "all"): 제품 필터 (`"all"`, `"FT"`, `"RS"`, `"BC"`)
- `format` (string, 선택, 기본값: "json"): 응답 형식 (`"json"` 또는 `"csv"`)

#### 응답 (Response)

**JSON 형식 (format=json)**:
```json
{
  "success": true,
  "export_date": "2025-11-28T15:00:00",
  "period": {
    "start_date": "2025-11-01",
    "end_date": "2025-11-28"
  },
  "filters": {
    "product_code": "all"
  },
  "total_records": 5420,
  "records": [
    {
      "id": 1,
      "serial_number": "MBFT12345678",
      "product_code": "FT",
      "decision": "normal",
      "missing_count": 0,
      "position_error_count": 0,
      "detection_count": 12,
      "avg_confidence": 0.93,
      "inference_time_ms": 95.2,
      "inspection_time": "2025-11-28T14:30:00"
    },
    {
      "id": 2,
      "serial_number": "MBRS87654321",
      "product_code": "RS",
      "decision": "missing",
      "missing_count": 3,
      "position_error_count": 0,
      "detection_count": 12,
      "avg_confidence": 0.88,
      "inference_time_ms": 102.3,
      "inspection_time": "2025-11-28T14:29:50"
    }
  ]
}
```

**CSV 형식 (format=csv)**:
```csv
id,serial_number,product_code,decision,missing_count,position_error_count,detection_count,avg_confidence,inference_time_ms,inspection_time
1,MBFT12345678,FT,normal,0,0,12,0.93,95.2,2025-11-28T14:30:00
2,MBRS87654321,RS,missing,3,0,12,0.88,102.3,2025-11-28T14:29:50
```

---

## 🔒 인증 및 권한 (Phase 6 구현 예정)

현재는 인증 없이 모든 API 접근 가능
Phase 6에서 JWT 토큰 기반 인증 추가 예정

---

## ⚠️ 에러 코드 및 처리

### HTTP 상태 코드

| 코드 | 의미 | 설명 |
|------|------|------|
| 200 | OK | 요청 성공 |
| 400 | Bad Request | 잘못된 요청 (필수 필드 누락, 형식 오류 등) |
| 404 | Not Found | 리소스를 찾을 수 없음 |
| 500 | Internal Server Error | 서버 내부 오류 (AI 모델 오류, DB 오류 등) |
| 503 | Service Unavailable | 서버 사용 불가 (모델 미로드, GPU 오류 등) |

### 공통 에러 응답 형식

```json
{
  "success": false,
  "error": "error_code",
  "message": "Human-readable error message",
  "timestamp": "2025-10-25T14:30:00",
  "request_id": "uuid-v4-string"
}
```

---

## 🧪 API 테스트 방법

### cURL 예시

```bash
# 1. 서버 상태 확인
curl -X GET http://100.64.1.1:5000/health

# 2. PCB 불량 검사 (Serial Number 자동 추출)
curl -X POST http://100.64.1.1:5000/predict \
  -H "Content-Type: application/json" \
  -d '{
    "image": "'"$(base64 -w 0 pcb_image.jpg)"'",
    "timestamp": "2025-11-28T14:30:00"
  }'

# 3. PCB 불량 검사 (Serial Number 제공)
curl -X POST http://100.64.1.1:5000/predict \
  -H "Content-Type: application/json" \
  -d '{
    "image": "'"$(base64 -w 0 pcb_image.jpg)"'",
    "serial_number": "MBFT12345678",
    "timestamp": "2025-11-28T14:30:00"
  }'

# 4. 검사 이력 조회 (제품별 필터링)
curl -X GET "http://100.64.1.1:5000/history?page=1&limit=20&product_code=FT&decision=missing"

# 5. 통계 데이터 조회 (일별 집계)
curl -X GET "http://100.64.1.1:5000/statistics?start_date=2025-11-01&end_date=2025-11-28&product_code=all&period=daily"

# 6. Excel 내보내기
curl -X GET "http://100.64.1.1:5000/export?start_date=2025-11-01&end_date=2025-11-28&product_code=FT&format=csv" -o export.csv
```

### Python 예시 (라즈베리파이 클라이언트)

```python
import requests
import base64
from datetime import datetime

# 이미지 읽기 및 Base64 인코딩
with open("pcb_image.jpg", "rb") as f:
    image_base64 = base64.b64encode(f.read()).decode("utf-8")

# API 요청 (Serial Number 자동 추출)
response = requests.post(
    "http://100.64.1.1:5000/predict",
    json={
        "image": image_base64,
        "timestamp": datetime.now().isoformat()
    },
    timeout=5
)

result = response.json()
if result["success"]:
    print(f"Serial Number: {result['serial_number']}")
    print(f"제품 코드: {result['product_code']}")
    print(f"판정: {result['decision']}")
    print(f"누락 부품: {result['verification_result']['missing_count']}개")
    print(f"위치 오류: {result['verification_result']['position_error_count']}개")
else:
    print(f"에러: {result['error']} - {result['message']}")
```

### C# 예시 (WinForms 앱)

```csharp
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

// 검사 이력 조회 (제품별 필터링)
public async Task<HistoryResponse> GetHistoryAsync(
    int page, int limit, string productCode = "all", string decision = "all")
{
    using (var client = new HttpClient())
    {
        var url = $"http://100.64.1.1:5000/history?" +
                  $"page={page}&limit={limit}&" +
                  $"product_code={productCode}&decision={decision}";

        var response = await client.GetAsync(url);
        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<HistoryResponse>(json);
    }
}

// 통계 데이터 조회 (일별 집계)
public async Task<StatisticsResponse> GetStatisticsAsync(
    string startDate, string endDate, string productCode = "all", string period = "daily")
{
    using (var client = new HttpClient())
    {
        var url = $"http://100.64.1.1:5000/statistics?" +
                  $"start_date={startDate}&end_date={endDate}&" +
                  $"product_code={productCode}&period={period}";

        var response = await client.GetAsync(url);
        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<StatisticsResponse>(json);
    }
}
```

---


## 📝 변경 이력

| 버전 | 날짜 | 변경 내용 | 변경자 |
|------|------|-----------|--------|
| 3.0.0 | 2025-11-28 | ⭐⭐⭐ Product Verification Architecture 전환 | 팀 리더 |
|  |  | - **이중 YOLO 모델** → **단일 YOLO 모델 + 제품별 부품 위치 검증** |  |
|  |  | - `/predict_dual` 엔드포인트 제거 (deprecated) |  |
|  |  | - `/predict` 엔드포인트 전면 개편 (Serial Number OCR + 제품 식별) |  |
|  |  | - decision 타입 변경: component_defect/solder_defect → missing/position_error |  |
|  |  | - 제품별 필터링 추가 (FT, RS, BC) |  |
|  |  | - 시간별/일별/월별 집계 테이블 활용 (aggregation tables) |  |
|  |  | - 모든 응답에 serial_number, product_code 추가 |  |
| 2.0.0 | 2025-10-31 | 이중 모델 아키텍처 전환 (FPIC-Component + SolDef_AI) [DEPRECATED] | 팀 리더 |
| 1.0.0 | 2025-10-25 | 초기 API 명세서 작성 [DEPRECATED] | 팀 리더 |

---

## 🔗 관련 문서

**v3.0 핵심 문서:**
- **⭐⭐⭐ [프로젝트 전체 로드맵](PCB_Defect_Detection_Project.md)** - v3.0 시스템 아키텍처
- **⭐ [Flask 서버 구축 가이드](Flask_Server_Setup.md)** - 제품별 부품 위치 검증 시스템
- **⭐ [MySQL 데이터베이스 설계](MySQL_Database_Design.md)** - v3.0 스키마 및 집계 테이블
- **⭐ [C# WinForms 설계 명세](CSharp_WinForms_Design_Specification.md)** - v4.0 UI 설계
- [데이터셋 가이드](Dataset_Guide.md) - 커스텀 데이터셋 (FT, RS, BC)
- [라즈베리파이 클라이언트 가이드](RaspberryPi_Setup.md)

**레거시 문서 (참고용):**
- [이중 모델 아키텍처 설계](Dual_Model_Architecture.md) - v2.0 [DEPRECATED]

---

**⚠️ 중요**: 이 문서는 팀 전체의 계약서입니다. API 변경 시 반드시 팀 회의 후 업데이트하세요!
