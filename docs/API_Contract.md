# API 계약 명세서 (API Contract)

PCB 불량 검사 시스템의 Flask 서버 REST API 공식 명세서입니다.
**모든 팀원은 이 명세서를 기준으로 개발해야 하며, 변경 시 전체 팀 합의가 필요합니다.**

---

## 📌 중요 공지

### API 버전 관리
- **현재 버전**: v1.0.0
- **Base URL**: `http://{SERVER_IP}:5000`
- **마지막 업데이트**: 2025-10-25

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
| `/predict` | POST | 단일 프레임 PCB 불량 검사 | Flask + AI |
| `/predict_dual` | POST | 양면 프레임 동시 검사 | Flask + AI |
| `/history` | GET | 검사 이력 조회 | Flask + DB |
| `/history/<id>` | GET | 특정 검사 결과 상세 조회 | Flask + DB |
| `/statistics` | GET | 통계 데이터 조회 | Flask + DB |
| `/export` | GET | Excel 내보내기용 데이터 | Flask + DB |
| `/box_status` | GET | 전체 박스 상태 조회 ⭐ | Flask + DB |
| `/box_status/<box_id>` | GET | 특정 박스 상태 조회 ⭐ | Flask + DB |
| `/box_status/reset` | POST | 박스 상태 리셋 ⭐ | Flask + DB |

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
  "server_time": "2025-10-25T14:30:00",
  "gpu_available": true,
  "models_loaded": {
    "yolo": true,
    "anomaly": true
  },
  "version": "1.0.0"
}
```

**실패 (503 Service Unavailable)**:
```json
{
  "status": "unhealthy",
  "error": "YOLO model not loaded",
  "server_time": "2025-10-25T14:30:00"
}
```

---

### 2. 단일 프레임 PCB 불량 검사

**엔드포인트**: `/predict`
**메서드**: `POST`
**설명**: 라즈베리파이에서 전송한 단일 프레임을 검사하고 불량 분류 결과 반환

#### 요청 (Request)
```http
POST /predict HTTP/1.1
Host: 100.64.1.1:5000
Content-Type: application/json

{
  "camera_id": "left",
  "image": "base64_encoded_jpeg_string",
  "timestamp": "2025-10-25T14:30:00",
  "request_id": "uuid-v4-string"
}
```

**필드 설명:**
- `camera_id` (string, 필수): 카메라 식별자 (`"left"` 또는 `"right"`)
- `image` (string, 필수): Base64 인코딩된 JPEG 이미지 데이터
- `timestamp` (string, 필수): ISO 8601 형식 타임스탬프
- `request_id` (string, 선택): 요청 추적용 UUID (없으면 서버 자동 생성)

#### 응답 (Response)
**성공 - 불량 검출 (200 OK)**:
```json
{
  "success": true,
  "request_id": "uuid-v4-string",
  "camera_id": "left",
  "timestamp": "2025-10-25T14:30:00",
  "inference_time_ms": 120.5,
  "result": {
    "classification": "solder_defect",
    "confidence": 0.87,
    "defects": [
      {
        "type": "cold_joint",
        "bbox": [120, 80, 200, 150],
        "confidence": 0.87,
        "severity": "medium"
      },
      {
        "type": "solder_bridge",
        "bbox": [300, 200, 380, 260],
        "confidence": 0.72,
        "severity": "low"
      }
    ],
    "total_defects": 2,
    "anomaly_score": 0.65
  },
  "gpio_action": {
    "enabled": true,
    "pin": 27,
    "action": "activate"
  }
}
```

**성공 - 정상 (200 OK)**:
```json
{
  "success": true,
  "request_id": "uuid-v4-string",
  "camera_id": "left",
  "timestamp": "2025-10-25T14:30:00",
  "inference_time_ms": 95.2,
  "result": {
    "classification": "normal",
    "confidence": 0.98,
    "defects": [],
    "total_defects": 0,
    "anomaly_score": 0.12
  },
  "gpio_action": {
    "enabled": true,
    "pin": 23,
    "action": "activate"
  }
}
```

**실패 (400 Bad Request)**:
```json
{
  "success": false,
  "error": "Invalid request format",
  "message": "Missing required field: image",
  "timestamp": "2025-10-25T14:30:00"
}
```

**실패 (500 Internal Server Error)**:
```json
{
  "success": false,
  "error": "Inference failed",
  "message": "CUDA out of memory",
  "timestamp": "2025-10-25T14:30:00"
}
```

**분류 타입 (classification):**
- `"normal"`: 정상 (GPIO 23)
- `"component_defect"`: 부품 불량 (GPIO 17)
- `"solder_defect"`: 납땜 불량 (GPIO 27)
- `"discard"`: 폐기 (GPIO 22)

**불량 타입 (defect type):**
- `"cold_joint"`: Cold Joint (차가운 납땜)
- `"solder_bridge"`: Solder Bridge (땜납 다리)
- `"insufficient_solder"`: 불충분한 납땜
- `"excess_solder"`: 과도한 납땜
- `"missing_component"`: 부품 누락
- `"misalignment"`: 부품 위치 불량
- `"wrong_component"`: 잘못된 부품
- `"damaged_component"`: 손상된 부품
- `"trace_damage"`: 회로 선로 손상
- `"pad_damage"`: 패드 손상
- `"scratch"`: 스크래치

**심각도 (severity):**
- `"low"`: 경미한 불량
- `"medium"`: 중간 불량
- `"high"`: 심각한 불량

---

### 3. 양면 프레임 동시 검사

**엔드포인트**: `/predict_dual`
**메서드**: `POST`
**설명**: 좌우 카메라 프레임을 동시에 검사하고 종합 결과 반환

#### 요청 (Request)
```http
POST /predict_dual HTTP/1.1
Host: 100.64.1.1:5000
Content-Type: application/json

{
  "left_camera": {
    "image": "base64_encoded_jpeg_string",
    "timestamp": "2025-10-25T14:30:00"
  },
  "right_camera": {
    "image": "base64_encoded_jpeg_string",
    "timestamp": "2025-10-25T14:30:00"
  },
  "request_id": "uuid-v4-string"
}
```

#### 응답 (Response)
```json
{
  "success": true,
  "request_id": "uuid-v4-string",
  "timestamp": "2025-10-25T14:30:00",
  "inference_time_ms": 185.3,
  "left_result": {
    "classification": "normal",
    "confidence": 0.95,
    "defects": [],
    "total_defects": 0
  },
  "right_result": {
    "classification": "solder_defect",
    "confidence": 0.82,
    "defects": [
      {
        "type": "cold_joint",
        "bbox": [150, 100, 230, 180],
        "confidence": 0.82,
        "severity": "medium"
      }
    ],
    "total_defects": 1
  },
  "final_classification": "solder_defect",
  "final_confidence": 0.82,
  "gpio_action": {
    "enabled": true,
    "pin": 27,
    "action": "activate"
  }
}
```

**최종 분류 규칙:**
- 양면 중 **더 심각한 불량**을 최종 분류로 선택
- 우선순위: `discard` > `component_defect` > `solder_defect` > `normal`

---

### 4. 검사 이력 조회

**엔드포인트**: `/history`
**메서드**: `GET`
**설명**: PCB 검사 이력을 페이지네이션하여 조회

#### 요청 (Request)
```http
GET /history?page=1&limit=20&classification=all&start_date=2025-10-01&end_date=2025-10-25 HTTP/1.1
Host: 100.64.1.1:5000
```

**쿼리 파라미터:**
- `page` (int, 선택, 기본값: 1): 페이지 번호
- `limit` (int, 선택, 기본값: 20): 페이지당 항목 수
- `classification` (string, 선택, 기본값: "all"): 필터링할 분류 타입
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
  "records": [
    {
      "id": 152,
      "timestamp": "2025-10-25T14:30:00",
      "camera_id": "left",
      "classification": "solder_defect",
      "confidence": 0.87,
      "total_defects": 2,
      "inference_time_ms": 120.5
    },
    {
      "id": 151,
      "timestamp": "2025-10-25T14:29:50",
      "camera_id": "right",
      "classification": "normal",
      "confidence": 0.98,
      "total_defects": 0,
      "inference_time_ms": 95.2
    }
  ]
}
```

---

### 5. 특정 검사 결과 상세 조회

**엔드포인트**: `/history/<id>`
**메서드**: `GET`
**설명**: 특정 검사 결과의 상세 정보 조회 (이미지 포함)

#### 요청 (Request)
```http
GET /history/152 HTTP/1.1
Host: 100.64.1.1:5000
```

#### 응답 (Response)
```json
{
  "success": true,
  "record": {
    "id": 152,
    "timestamp": "2025-10-25T14:30:00",
    "camera_id": "left",
    "classification": "solder_defect",
    "confidence": 0.87,
    "defects": [
      {
        "type": "cold_joint",
        "bbox": [120, 80, 200, 150],
        "confidence": 0.87,
        "severity": "medium"
      },
      {
        "type": "solder_bridge",
        "bbox": [300, 200, 380, 260],
        "confidence": 0.72,
        "severity": "low"
      }
    ],
    "total_defects": 2,
    "anomaly_score": 0.65,
    "inference_time_ms": 120.5,
    "image_url": "/images/152.jpg",
    "annotated_image_url": "/images/152_annotated.jpg"
  }
}
```

---

### 6. 통계 데이터 조회

**엔드포인트**: `/statistics`
**메서드**: `GET`
**설명**: PCB 검사 통계 데이터 (일별, 분류별, 불량 타입별)

#### 요청 (Request)
```http
GET /statistics?start_date=2025-10-01&end_date=2025-10-25 HTTP/1.1
Host: 100.64.1.1:5000
```

#### 응답 (Response)
```json
{
  "success": true,
  "period": {
    "start_date": "2025-10-01",
    "end_date": "2025-10-25"
  },
  "total_inspections": 5420,
  "classification_counts": {
    "normal": 4850,
    "solder_defect": 320,
    "component_defect": 180,
    "discard": 70
  },
  "defect_type_counts": {
    "cold_joint": 150,
    "solder_bridge": 120,
    "missing_component": 90,
    "misalignment": 80,
    "insufficient_solder": 50,
    "scratch": 30,
    "others": 50
  },
  "daily_statistics": [
    {
      "date": "2025-10-25",
      "total": 250,
      "normal": 220,
      "defects": 30,
      "defect_rate": 0.12
    }
  ],
  "average_inference_time_ms": 110.3,
  "defect_rate": 0.105
}
```

---

### 7. Excel 내보내기용 데이터

**엔드포인트**: `/export`
**메서드**: `GET`
**설명**: C# WinForms에서 Excel 내보내기를 위한 전체 데이터 조회

#### 요청 (Request)
```http
GET /export?start_date=2025-10-01&end_date=2025-10-25&format=json HTTP/1.1
Host: 100.64.1.1:5000
```

**쿼리 파라미터:**
- `start_date` (string, 필수): 시작 날짜 (YYYY-MM-DD)
- `end_date` (string, 필수): 종료 날짜 (YYYY-MM-DD)
- `format` (string, 선택, 기본값: "json"): 응답 형식 (`"json"` 또는 `"csv"`)

#### 응답 (Response)
```json
{
  "success": true,
  "export_date": "2025-10-25T15:00:00",
  "period": {
    "start_date": "2025-10-01",
    "end_date": "2025-10-25"
  },
  "total_records": 5420,
  "records": [
    {
      "id": 1,
      "timestamp": "2025-10-01T09:00:00",
      "camera_id": "left",
      "classification": "normal",
      "confidence": 0.98,
      "total_defects": 0,
      "defect_types": "",
      "inference_time_ms": 95.2
    },
    {
      "id": 2,
      "timestamp": "2025-10-01T09:00:10",
      "camera_id": "right",
      "classification": "solder_defect",
      "confidence": 0.87,
      "total_defects": 2,
      "defect_types": "cold_joint, solder_bridge",
      "inference_time_ms": 120.5
    }
  ]
}
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

# 2. 단일 프레임 검사 (Base64 인코딩 필요)
curl -X POST http://100.64.1.1:5000/predict \
  -H "Content-Type: application/json" \
  -d '{
    "camera_id": "left",
    "image": "'"$(base64 -w 0 test_image.jpg)"'",
    "timestamp": "2025-10-25T14:30:00"
  }'

# 3. 검사 이력 조회
curl -X GET "http://100.64.1.1:5000/history?page=1&limit=10"

# 4. 통계 데이터 조회
curl -X GET "http://100.64.1.1:5000/statistics?start_date=2025-10-01&end_date=2025-10-25"
```

### Python 예시 (라즈베리파이 클라이언트)

```python
import requests
import base64
from datetime import datetime

# 이미지 읽기 및 Base64 인코딩
with open("pcb_image.jpg", "rb") as f:
    image_base64 = base64.b64encode(f.read()).decode("utf-8")

# API 요청
response = requests.post(
    "http://100.64.1.1:5000/predict",
    json={
        "camera_id": "left",
        "image": image_base64,
        "timestamp": datetime.now().isoformat()
    },
    timeout=5
)

result = response.json()
print(f"분류: {result['result']['classification']}")
print(f"신뢰도: {result['result']['confidence']}")
```

### C# 예시 (WinForms 앱)

```csharp
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

// 검사 이력 조회
public async Task<List<InspectionRecord>> GetHistoryAsync(int page, int limit)
{
    using (var client = new HttpClient())
    {
        var response = await client.GetAsync(
            $"http://100.64.1.1:5000/history?page={page}&limit={limit}"
        );
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<HistoryResponse>(json);
        return result.Records;
    }
}
```

---

### 7. 전체 박스 상태 조회 ⭐ 신규

**엔드포인트**: `/box_status`
**메서드**: `GET`
**설명**: 3개 박스(정상/부품불량/납땜불량)의 상태 조회 (DISCARD는 슬롯 관리 안 함)

#### 요청 (Request)
```http
GET /box_status HTTP/1.1
Host: 100.64.1.1:5000
```

#### 응답 (Response)
**성공 (200 OK)**:
```json
{
  "status": "ok",
  "timestamp": "2025-10-27T10:30:00",
  "boxes": [
    {
      "box_id": "NORMAL",
      "category": "normal",
      "current_slot": 1,
      "max_slots": 2,
      "is_full": false,
      "total_pcb_count": 1,
      "utilization_rate": 50.0,
      "last_updated": "2025-10-27T10:25:00"
    },
    {
      "box_id": "COMPONENT_DEFECT",
      "category": "component_defect",
      "current_slot": 2,
      "max_slots": 2,
      "is_full": true,
      "total_pcb_count": 2,
      "utilization_rate": 100.0,
      "last_updated": "2025-10-27T10:20:00"
    },
    {
      "box_id": "SOLDER_DEFECT",
      "category": "solder_defect",
      "current_slot": 0,
      "max_slots": 2,
      "is_full": false,
      "total_pcb_count": 0,
      "utilization_rate": 0.0,
      "last_updated": "2025-10-27T09:00:00"
    }
  ],
  "summary": {
    "total_boxes": 3,
    "full_boxes": 1,
    "empty_boxes": 1,
    "total_pcb_stored": 3
  }
}
```

**필드 설명:**
- `box_id` (string): 박스 ID (예: "NORMAL", "COMPONENT_DEFECT", "SOLDER_DEFECT")
- `category` (string): 카테고리 (normal/component_defect/solder_defect)
- `current_slot` (int): 현재 사용 중인 슬롯 번호 (0-1, 수직 2단)
- `max_slots` (int): 최대 슬롯 개수 (2개, 수직 2단 적재)
- `is_full` (boolean): 박스 가득참 여부
- `total_pcb_count` (int): 박스에 저장된 총 PCB 개수
- `utilization_rate` (float): 사용률 (0.0 ~ 100.0)
- `last_updated` (string): 마지막 업데이트 시각

**참고**: DISCARD 카테고리는 슬롯 관리를 하지 않으므로 box_status에 포함되지 않습니다.

---

### 8. 특정 박스 상태 조회 ⭐ 신규

**엔드포인트**: `/box_status/<box_id>`
**메서드**: `GET`
**설명**: 특정 박스의 상태 조회

#### 요청 (Request)
```http
GET /box_status/NORMAL HTTP/1.1
Host: 100.64.1.1:5000
```

#### 응답 (Response)
**성공 (200 OK)**:
```json
{
  "status": "ok",
  "timestamp": "2025-10-27T10:30:00",
  "box": {
    "box_id": "NORMAL",
    "category": "normal",
    "current_slot": 1,
    "max_slots": 2,
    "is_full": false,
    "total_pcb_count": 1,
    "utilization_rate": 50.0,
    "created_at": "2025-10-27T08:00:00",
    "last_updated": "2025-10-27T10:25:00",
    "recent_inspections": [
      {
        "inspection_id": 12345,
        "slot_number": 0,
        "classification": "normal",
        "confidence": 0.985,
        "timestamp": "2025-10-27T10:25:00"
      }
    ]
  }
}
```

**실패 (404 Not Found)**:
```json
{
  "status": "error",
  "error": "Box not found",
  "message": "Box ID 'INVALID_ID' does not exist",
  "timestamp": "2025-10-27T10:30:00"
}
```

---

### 9. 박스 상태 리셋 ⭐ 신규

**엔드포인트**: `/box_status/reset`
**메서드**: `POST`
**설명**: 특정 박스 또는 전체 박스 상태 리셋 (비우기)

#### 요청 (Request)

**특정 박스 리셋:**
```http
POST /box_status/reset HTTP/1.1
Host: 100.64.1.1:5000
Content-Type: application/json

{
  "box_id": "NORMAL",
  "reason": "Box replaced",
  "operator": "admin"
}
```

**전체 박스 리셋:**
```http
POST /box_status/reset HTTP/1.1
Host: 100.64.1.1:5000
Content-Type: application/json

{
  "reset_all": true,
  "reason": "System maintenance",
  "operator": "admin"
}
```

**필드 설명:**
- `box_id` (string, 선택): 리셋할 박스 ID (지정 시 해당 박스만 리셋)
- `reset_all` (boolean, 선택): true일 경우 모든 박스 리셋 (3개)
- `reason` (string, 필수): 리셋 사유
- `operator` (string, 필수): 작업자 ID

#### 응답 (Response)
**성공 (200 OK)**:
```json
{
  "status": "ok",
  "message": "Box NORMAL has been reset",
  "box_id": "NORMAL",
  "reset_count": 1,
  "timestamp": "2025-10-27T10:30:00"
}
```

**전체 리셋 성공:**
```json
{
  "status": "ok",
  "message": "All boxes have been reset",
  "reset_count": 3,
  "boxes_reset": [
    "NORMAL",
    "COMPONENT_DEFECT",
    "SOLDER_DEFECT"
  ],
  "timestamp": "2025-10-27T10:30:00"
}
```

**실패 (400 Bad Request)**:
```json
{
  "status": "error",
  "error": "Invalid request",
  "message": "Either 'box_id' or 'reset_all' must be specified",
  "timestamp": "2025-10-27T10:30:00"
}
```

---

## 📝 변경 이력

| 버전 | 날짜 | 변경 내용 | 변경자 |
|------|------|-----------|--------|
| 1.1.0 | 2025-10-27 | 박스 상태 관리 API 추가 (로봇팔 시스템) | 팀 리더 |
| 1.0.0 | 2025-10-25 | 초기 API 명세서 작성 | 팀 리더 |

---

## 🔗 관련 문서

- [Flask 서버 구축 가이드](Flask_Server_Setup.md)
- [라즈베리파이 클라이언트 가이드](RaspberryPi_Setup.md)
- [C# WinForms 개발 가이드](CSharp_WinForms_Guide.md)
- [Git 워크플로우 가이드](Git_Workflow.md)

---

**⚠️ 중요**: 이 문서는 팀 전체의 계약서입니다. API 변경 시 반드시 팀 회의 후 업데이트하세요!
