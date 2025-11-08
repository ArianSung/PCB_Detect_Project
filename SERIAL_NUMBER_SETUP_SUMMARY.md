# 일련번호 검출 시스템 구축 완료 ✅

## 생성된 파일 목록

### 1️⃣ 데이터베이스 스키마 (두 가지 방법)

#### 방법 1: inspections 테이블에 컬럼 추가 (간단, 권장)
- **파일**: `database/01_add_serial_number_to_inspections.sql`
- **내용**:
  ```sql
  ALTER TABLE inspections
  ADD COLUMN serial_number VARCHAR(50) NULL,
  ADD COLUMN serial_number_confidence DECIMAL(4,3) NULL,
  ADD COLUMN serial_number_bbox JSON NULL;
  ```
- **실행 방법**:
  ```bash
  mysql -u root -p pcb_inspection < database/01_add_serial_number_to_inspections.sql
  ```

#### 방법 2: 별도 pcb_products 테이블 생성 (고급)
- **파일**: `database/02_create_pcb_products_table.sql`
- **내용**:
  - `pcb_products` 테이블 생성 (일련번호 마스터)
  - `inspections` 테이블에 FK 추가
  - 트리거 및 저장 프로시저
- **실행 방법**:
  ```bash
  mysql -u root -p pcb_inspection < database/02_create_pcb_products_table.sql
  ```

**추천**: 두 가지 방법 **모두 적용**하면 윈폼 모니터링에서 편리합니다!

---

### 2️⃣ 문서

- **파일**: `docs/Serial_Number_Detection_Guide.md`
- **내용**:
  - YOLO + OCR 조합 원리
  - 라벨링 방법 (LabelImg/Roboflow)
  - 학습 가이드
  - Flask 서버 통합 방법

---

### 3️⃣ 구현 코드

#### 셋업 스크립트
- **파일**: `scripts/setup_serial_number_detection.sh`
- **기능**:
  - EasyOCR 설치
  - 데이터셋 디렉토리 구조 생성
  - data.yaml 템플릿 생성
- **실행**:
  ```bash
  bash scripts/setup_serial_number_detection.sh
  ```

#### 학습 스크립트
- **파일**: `scripts/train_serial_number_detector.py`
- **기능**:
  - YOLOv11n (Nano 모델) 학습
  - 50 에폭, 증강 최소화
  - 일련번호 영역 검출 모델 생성
- **실행**:
  ```bash
  conda activate pcb_defect
  python scripts/train_serial_number_detector.py
  ```

#### YOLO + OCR 통합 클래스
- **파일**: `server/serial_number_detector.py`
- **기능**:
  - `SerialNumberDetector` 클래스
  - YOLO로 ROI 검출 → OCR로 텍스트 인식
  - 시각화 기능 포함
- **사용 예시**:
  ```python
  from serial_number_detector import SerialNumberDetector

  detector = SerialNumberDetector(
      yolo_model_path='runs/detect/serial_number_detector/weights/best.pt',
      languages=['en']
  )

  result = detector.detect(image)
  print(result['serial_number'])
  ```

---

## 전체 워크플로우 📋

### 1단계: 환경 셋업
```bash
# 셋업 스크립트 실행
bash scripts/setup_serial_number_detection.sh
```

**결과**:
- EasyOCR 설치 완료
- 데이터셋 디렉토리 생성:
  ```
  data/raw/serial_number_detection/
  ├── train/images/
  ├── train/labels/
  ├── valid/images/
  ├── valid/labels/
  ├── test/images/
  └── test/labels/
  ```

### 2단계: 데이터 수집 및 라벨링

#### 이미지 수집
- PCB 앞면 이미지 **100-300장** 촬영
- `train/images`에 저장 (70%)
- `valid/images`에 저장 (15%)
- `test/images`에 저장 (15%)

#### 라벨링 (LabelImg 사용)
```bash
conda activate pcb_defect
pip install labelImg
labelImg
```

**라벨링 순서**:
1. `Open Dir` → `train/images` 선택
2. `Change Save Dir` → `train/labels` 선택
3. YOLO 포맷 선택
4. `Create Classes` → `serial_number` 입력
5. 단축키 `W` → 일련번호 영역에 박스 그리기
6. `Ctrl + S` 저장
7. `D` 다음 이미지

**중요**: 일련번호 **텍스트가 있는 영역**만 박스로 그리면 됩니다!

```
┌──────────────────┐
│ PCB-2025-001234  │ ← 이 영역만 선택!
└──────────────────┘
```

### 3단계: YOLO 모델 학습

```bash
conda activate pcb_defect
python scripts/train_serial_number_detector.py
```

**학습 시간**: 5-10분 (RTX 4080 Super 기준)

**결과 모델**:
- `runs/detect/serial_number_detector/weights/best.pt` ✅

### 4단계: 데이터베이스 스키마 적용

```bash
# 방법 1 (권장): inspections 테이블에 컬럼 추가
mysql -u root -p pcb_inspection < database/01_add_serial_number_to_inspections.sql

# 방법 2 (선택): 별도 pcb_products 테이블 생성
mysql -u root -p pcb_inspection < database/02_create_pcb_products_table.sql
```

### 5단계: Flask 서버 통합

**server/app.py 수정**:
```python
from serial_number_detector import SerialNumberDetector

# 전역 변수
serial_detector = None

@app.before_first_request
def initialize():
    global serial_detector

    # 일련번호 검출기 초기화
    serial_detector = SerialNumberDetector(
        yolo_model_path='runs/detect/serial_number_detector/weights/best.pt',
        languages=['en']
    )

@app.route('/predict_dual', methods=['POST'])
def predict_dual():
    # 1. 일련번호 검출 (좌측 프레임)
    serial_result = serial_detector.detect(left_frame)

    # 2. 부품 검출 (기존)
    component_result = component_model.predict(left_frame)

    # 3. 납땜 검출 (기존)
    solder_result = solder_model.predict(right_frame)

    # 4. 융합 판정 (기존)
    fusion_result = fuse_results(component_result, solder_result)

    # 5. DB 저장 (일련번호 포함)
    save_to_db(
        serial_number=serial_result['serial_number'],
        serial_confidence=serial_result['confidence'],
        serial_bbox=serial_result['yolo_bbox'],
        fusion_decision=fusion_result['decision'],
        ...
    )

    return jsonify({
        'serial_number': serial_result['serial_number'],
        'serial_confidence': serial_result['confidence'],
        'fusion_decision': fusion_result['decision'],
        ...
    })
```

---

## YOLO + OCR 원리 🧠

### 왜 YOLO + OCR 조합인가?

#### ❌ OCR만 사용 (문제점)
```
전체 PCB 이미지에서 OCR 실행
→ "R1", "IC1", "PCB-2025-001234", "3.3V", "GND" 모두 인식
→ 어떤 것이 일련번호인지 구분 어려움 ❌
```

#### ✅ YOLO + OCR 조합 (해결책)
```
1단계: YOLO가 일련번호 영역만 검출 (bbox)
        ↓
2단계: 해당 영역만 잘라내기 (ROI 추출)
        ↓
3단계: OCR이 ROI 내의 텍스트만 인식
        ↓
결과: "PCB-2025-001234" ✅ (정확!)
```

---

## 데이터 라벨링 상세 설명 📝

### 라벨링 대상

**❌ 잘못된 라벨링**:
- 부품 라벨 (R1, IC1 등) ← 이건 라벨링 안 함
- 전압 표시 (3.3V, GND 등) ← 이것도 안 함

**✅ 올바른 라벨링**:
- 일련번호가 있는 영역만! (예: "PCB-2025-001234")

### 라벨 파일 형식 (YOLO)

**파일 구조**:
```
train/
├── images/
│   └── pcb_001.jpg
└── labels/
    └── pcb_001.txt  ← YOLO 라벨 파일
```

**pcb_001.txt 내용**:
```
0 0.5 0.15 0.4 0.08
```

**형식 설명**:
```
class_id center_x center_y width height
   0       0.5      0.15    0.4   0.08

- class_id: 0 (serial_number 클래스, 항상 0)
- center_x: 박스 중심 X (이미지 너비 대비 비율, 0.0~1.0)
- center_y: 박스 중심 Y (이미지 높이 대비 비율, 0.0~1.0)
- width: 박스 너비 (이미지 너비 대비 비율)
- height: 박스 높이 (이미지 높이 대비 비율)
```

**예시 계산**:
- 이미지 크기: 640 × 480
- 일련번호 위치: (200, 50) ~ (400, 100)

```python
center_x = ((200 + 400) / 2) / 640 = 0.46875
center_y = ((50 + 100) / 2) / 480 = 0.15625
width = (400 - 200) / 640 = 0.3125
height = (100 - 50) / 480 = 0.10417

# pcb_001.txt:
# 0 0.46875 0.15625 0.3125 0.10417
```

**중요**: LabelImg는 자동으로 이 계산을 해주므로 직접 계산할 필요 없습니다!

---

## 학습 설정 상세 ⚙️

### 왜 YOLOv11n (Nano)?

- **가볍고 빠름**: 파라미터 수 최소
- **클래스 1개만**: serial_number 하나만 검출
- **위치 고정**: 일련번호가 항상 같은 위치에 있으므로 복잡한 모델 불필요
- **추론 속도**: < 10ms (실시간 처리 충분)

### 왜 증강을 최소화?

```python
degrees=5,        # 회전 ±5도만 (일련번호 기울어질 가능성 낮음)
translate=0.05,   # 이동 5%만 (위치 거의 고정)
scale=0.2,        # 크기 조정 최소
flipud=0.0,       # 상하 반전 안 함
fliplr=0.0,       # 좌우 반전 안 함 (일련번호는 항상 정방향)
mosaic=0.0,       # Mosaic 안 함
mixup=0.0,        # MixUp 안 함
```

**이유**: 일련번호가 **같은 위치, 같은 방향**에 있으므로, 과도한 증강은 오히려 정확도를 떨어뜨립니다.

### 학습 시간

- **데이터셋**: 200장
- **에폭**: 50
- **배치**: 16
- **예상 시간**: **5-10분** (RTX 4080 Super)

---

## 테스트 방법 🧪

### 단독 테스트

```bash
conda activate pcb_defect
python server/serial_number_detector.py
```

**결과**:
```
검출 결과:
  성공: True
  일련번호: PCB-2025-001234
  OCR 신뢰도: 0.923
  YOLO 신뢰도: 0.987
  YOLO bbox: [120, 50, 320, 80]

✓ 시각화 결과 저장: serial_number_detection_result.jpg
```

---

## FAQ ❓

### Q1: 일련번호 패턴이 다양한데, 어떻게 유효성 검사하나요?

**A**: `serial_number_detector.py`의 `is_valid_serial_number()` 함수를 수정하세요.

```python
def is_valid_serial_number(self, text):
    patterns = [
        r'^PCB[-_]?\d{4}[-_]?\d{6}$',  # 여기에 실제 패턴 추가
        r'^YOUR_PATTERN_HERE$',
    ]
    ...
```

### Q2: 데이터를 몇 장 모아야 하나요?

**A**: **100-300장**이면 충분합니다. 일련번호가 같은 위치에 있으므로 적은 데이터로도 높은 정확도를 얻을 수 있습니다.

### Q3: 라벨링이 어려운데 더 쉬운 방법은?

**A**: **Roboflow** (https://roboflow.com) 사용을 추천합니다.
- 웹 기반 (설치 불필요)
- 자동 라벨링 도구
- YOLO 포맷으로 Export

### Q4: OCR 신뢰도가 낮으면 어떻게 하나요?

**A**:
1. ROI 이미지 품질 확인 (흐릿한지, 너무 작은지)
2. 조명 개선
3. OCR 언어 추가 (숫자만 있으면 `languages=['en']`로 충분)

### Q5: 일련번호가 여러 위치에 있으면?

**A**: YOLO는 여러 개의 bbox를 검출할 수 있습니다. `detect()` 함수에서 `yolo_results[0].boxes`를 순회하면 됩니다.

---

## 요약 정리 📌

### 핵심 포인트

1. **YOLO + OCR 조합**: YOLO로 영역 찾기 → OCR로 텍스트 읽기
2. **라벨링**: 일련번호 영역만 박스로 그리기 (LabelImg 사용)
3. **학습**: YOLOv11n, 50 에폭, 증강 최소화
4. **데이터베이스**: 두 가지 방법 모두 적용 (inspections 컬럼 추가 + pcb_products 테이블)
5. **Flask 통합**: `serial_detector.detect(left_frame)` 추가

### 다음 단계

1. ✅ 환경 셋업 (`bash scripts/setup_serial_number_detection.sh`)
2. 📸 이미지 수집 (100-300장)
3. 🏷️ 라벨링 (LabelImg)
4. 🤖 YOLO 학습 (`python scripts/train_serial_number_detector.py`)
5. 💾 데이터베이스 스키마 적용 (SQL 파일 실행)
6. 🚀 Flask 서버 통합

---

**작성일**: 2025-11-07
**문서 버전**: 1.0
**작성자**: Claude Code
