# Flask 추론 서버

PCB 불량 검사를 위한 Flask REST API 서버입니다.

## 실행 방법

### 1. 가상환경 활성화
```bash
conda activate pcb_defect
```

### 2. 서버 실행
```bash
# 방법 1: Python 직접 실행
python server/app.py

# 방법 2: Flask CLI
flask --app server/app run --host=0.0.0.0 --port=5000
```

### 3. 서버 상태 확인
```bash
curl http://localhost:5000/health
```

## API 엔드포인트

### 1. `/health` (GET)
서버 상태 체크

**응답 예시**:
```json
{
  "status": "ok",
  "timestamp": "2025-01-27T15:30:45.123456",
  "server": "Flask PCB Inspection Server",
  "version": "1.0.0"
}
```

### 2. `/predict` (POST)
단일 프레임 추론

**요청 예시**:
```json
{
  "camera_id": "left",
  "image": "base64_encoded_jpeg_image"
}
```

**응답 예시**:
```json
{
  "status": "ok",
  "camera_id": "left",
  "defect_type": "정상",
  "confidence": 0.95,
  "inference_time_ms": 85.3
}
```

### 3. `/predict_dual` (POST)
양면 동시 추론

### 4. `/api/v1/box_status` (GET)
박스 상태 조회 (C# WinForms용)

## 폴더 구조

```
server/
├── app.py              # Flask 메인 애플리케이션
├── inference/          # 추론 엔진 (TODO)
│   ├── yolo_inference.py
│   └── anomaly_detection.py
├── database/           # DB 연동 (TODO)
│   └── mysql_client.py
└── utils/              # 유틸리티 (TODO)
    └── image_processing.py
```

## 개발 가이드

상세한 Flask 서버 개발 가이드는 `docs/Flask_Server_Setup.md`를 참조하세요.
