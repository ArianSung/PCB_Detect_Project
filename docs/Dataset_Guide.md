# PCB 검사 시스템 커스텀 데이터셋 가이드 v3.0

## 목표
Product Verification Architecture를 위한 커스텀 데이터셋 수집 및 준비

**핵심 변경** ⭐:
- **기존 v2.0**: 공개 데이터셋 2개 (FPIC-Component + SolDef_AI)
- **신규 v3.0**: 제품별 커스텀 데이터셋 수집
  - **3개 제품**: FT, RS, BC
  - **부품 검출 모델**: 단일 YOLOv11l 모델
  - **기준 데이터**: 제품별 정상 부품 배치 위치

**YOLO 환경 구축 및 학습 방법은 `docs/Phase1_YOLO_Setup.md`를 참조하세요.**

**참고**: `MySQL_Database_Design.md` (제품별 기준 데이터 스키마)

---

## v3.0 아키텍처 개요

### 시스템 구조
```
PCB 뒷면 (Serial + QR) → 제품 식별 (FT/RS/BC)
                             ↓
PCB 앞면 (부품 배치) → YOLO 검출 → 제품별 기준 위치와 비교
                             ↓
                    4단계 판정 (normal/missing/position_error/discard)
```

### 필요한 데이터
1. **학습 데이터**: 3개 제품의 PCB 부품 이미지 (YOLO 학습용)
2. **기준 데이터**: 제품별 정상 부품 배치 위치 (product_components 테이블)

---

## 데이터 수집 전략

### 1. 학습 데이터 (Training Data)

**목적**: YOLOv11l 모델 학습용

**수집 대상**:
- 3개 제품 타입 (FT, RS, BC)의 PCB 앞면 이미지
- 정상 제품 + 불량 제품 (부품 누락, 위치 오류)
- 다양한 조명, 각도, 배경

**권장 데이터 양**:
```
최소 (Minimum):
- 제품당 200장 이상
- 총 600장 이상 (3개 제품)

권장 (Recommended):
- 제품당 500-1,000장
- 총 1,500-3,000장

최적 (Optimal):
- 제품당 2,000장 이상
- 총 6,000장 이상
```

**데이터 분할**:
- Train: 70% (학습)
- Valid: 20% (검증)
- Test: 10% (평가)

---

### 2. 기준 데이터 (Reference Data)

**목적**: 제품별 정상 부품 배치 위치 기준

**수집 대상**:
- 각 제품의 정상 제품 1개 (Golden Sample)
- 고해상도 이미지 (1920x1080 이상)
- 정확한 부품 위치 좌표

**저장 위치**:
- MySQL `product_components` 테이블
- JSON 형식 백업 (`server/reference_data/`)

---

## 데이터 수집 가이드

### Phase 1: 촬영 환경 구축

#### 하드웨어 요구사항
- 웹캠 2대 (좌/우측 카메라)
- 해상도: 1280x720 이상 (권장: 1920x1080)
- 조명: 균일한 LED 조명 (컨베이어 벨트 좌우)
- 배경: 단색 배경 (검은색 또는 흰색)

#### 소프트웨어 도구
```bash
# OpenCV 기반 이미지 캡처 스크립트
python3 tools/capture_dataset.py --camera 0 --product FT --output data/raw/FT/
```

---

### Phase 2: 학습 데이터 수집

#### Step 1: 정상 제품 촬영

**각 제품별로**:
1. 정상 제품 100-200개 준비
2. 컨베이어 벨트에 배치
3. 좌측 카메라로 PCB 앞면 촬영 (부품 배치)
4. 자동 저장 (`data/raw/{PRODUCT_CODE}/normal/`)

**촬영 팁**:
- 조명 일정하게 유지
- PCB 중앙 정렬
- 초점 맞추기
- 그림자 최소화

#### Step 2: 불량 제품 촬영 (선택)

**부품 누락 시뮬레이션**:
- 일부러 부품 제거 (1-3개)
- 촬영 후 `data/raw/{PRODUCT_CODE}/missing/` 저장

**위치 오류 시뮬레이션**:
- 부품을 살짝 이동 (20-50px)
- 촬영 후 `data/raw/{PRODUCT_CODE}/misaligned/` 저장

**권장 불량 비율**:
- 정상: 70-80%
- 부품 누락: 10-15%
- 위치 오류: 10-15%

---

### Phase 3: 기준 데이터 수집

#### Golden Sample 촬영

**각 제품별로**:
1. **완벽한 정상 제품 1개** 선택 (검수 완료된 제품)
2. 고해상도로 촬영 (1920x1080 이상)
3. YOLO 모델로 부품 검출
4. 검출 결과를 `product_components` 테이블에 저장

**자동화 스크립트**:
```bash
# Golden Sample에서 기준 데이터 생성
python3 tools/generate_reference_data.py \
  --image data/golden_samples/FT_golden.jpg \
  --product FT \
  --output server/reference_data/FT_reference.json
```

**생성된 JSON 예시** (`server/reference_data/FT_reference.json`):
```json
{
  "product_code": "FT",
  "component_count": 25,
  "components": [
    {
      "component_class": "capacitor",
      "center_x": 150.5,
      "center_y": 200.3,
      "bbox_x1": 140.0,
      "bbox_y1": 190.0,
      "bbox_x2": 161.0,
      "bbox_y2": 210.6,
      "tolerance_px": 20.0
    },
    {
      "component_class": "resistor",
      "center_x": 300.2,
      "center_y": 250.8,
      "bbox_x1": 290.0,
      "bbox_y1": 240.0,
      "bbox_x2": 310.4,
      "bbox_y2": 261.6,
      "tolerance_px": 20.0
    }
    // ... 나머지 부품들
  ]
}
```

#### 데이터베이스 삽입

```bash
# JSON → MySQL 자동 삽입
python3 tools/import_reference_data.py \
  --json server/reference_data/FT_reference.json \
  --database pcb_inspection
```

**MySQL 삽입 쿼리 예시**:
```sql
INSERT INTO product_components (
    product_code,
    component_class,
    center_x,
    center_y,
    bbox_x1,
    bbox_y1,
    bbox_x2,
    bbox_y2,
    tolerance_px
) VALUES
('FT', 'capacitor', 150.5, 200.3, 140.0, 190.0, 161.0, 210.6, 20.0),
('FT', 'resistor', 300.2, 250.8, 290.0, 240.0, 310.4, 261.6, 20.0);
-- ... 나머지 부품들
```

---

## 데이터 어노테이션 (Annotation)

### YOLO 형식 어노테이션

**도구**:
- [LabelImg](https://github.com/heartexlabs/labelImg) (무료, 오픈소스)
- [Roboflow](https://roboflow.com/) (웹 기반, 무료 티어)
- [CVAT](https://www.cvat.ai/) (협업 가능)

**YOLO 어노테이션 형식**:
```
<class_id> <x_center> <y_center> <width> <height>
```

- 모든 좌표는 **정규화된 값** (0~1 사이)
- `x_center`, `y_center`: 바운딩 박스 중심점 (이미지 너비/높이로 나눔)
- `width`, `height`: 바운딩 박스 너비/높이 (이미지 너비/높이로 나눔)

**예시** (`FT_image_001.txt`):
```
0 0.234 0.512 0.045 0.067  # capacitor
1 0.468 0.623 0.038 0.052  # resistor
2 0.789 0.345 0.102 0.089  # IC
```

### 클래스 정의 (classes.txt)

```
0: capacitor
1: resistor
2: IC
3: LED
4: diode
5: transistor
6: connector
7: inductor
8: relay
9: switch
10: potentiometer
11: crystal
12: fuse
13: battery
14: transformer
```

**참고**: 제품별로 사용되는 부품 종류가 다를 수 있으므로, 실제 제품에 맞게 조정하세요.

---

## 데이터셋 디렉토리 구조

### 원본 데이터 (Raw Data)

```
data/
├── raw/
│   ├── FT/                          # Fast Type 제품
│   │   ├── normal/                  # 정상 제품
│   │   │   ├── FT_0001.jpg
│   │   │   ├── FT_0002.jpg
│   │   │   └── ...
│   │   ├── missing/                 # 부품 누락
│   │   │   ├── FT_missing_0001.jpg
│   │   │   └── ...
│   │   └── misaligned/              # 위치 오류
│   │       ├── FT_misaligned_0001.jpg
│   │       └── ...
│   ├── RS/                          # Reliable Stable 제품
│   │   ├── normal/
│   │   ├── missing/
│   │   └── misaligned/
│   └── BC/                          # Budget Compact 제품
│       ├── normal/
│       ├── missing/
│       └── misaligned/
└── golden_samples/
    ├── FT_golden.jpg                # FT 정상 기준 샘플
    ├── RS_golden.jpg                # RS 정상 기준 샘플
    └── BC_golden.jpg                # BC 정상 기준 샘플
```

### YOLO 학습 데이터 (Processed Data)

```
data/
└── processed/
    └── pcb_components/              # 통합 데이터셋
        ├── images/
        │   ├── train/               # 70% 학습
        │   │   ├── FT_0001.jpg
        │   │   ├── RS_0001.jpg
        │   │   ├── BC_0001.jpg
        │   │   └── ...
        │   ├── valid/               # 20% 검증
        │   │   ├── FT_val_001.jpg
        │   │   └── ...
        │   └── test/                # 10% 평가
        │       ├── FT_test_001.jpg
        │       └── ...
        ├── labels/
        │   ├── train/               # YOLO .txt 파일
        │   │   ├── FT_0001.txt
        │   │   ├── RS_0001.txt
        │   │   └── ...
        │   ├── valid/
        │   │   ├── FT_val_001.txt
        │   │   └── ...
        │   └── test/
        │       ├── FT_test_001.txt
        │       └── ...
        ├── data.yaml                # YOLO 설정 파일
        └── classes.txt              # 클래스 정의
```

---

## 데이터셋 준비 절차

### Step 1: 원본 데이터 수집 완료 확인

```bash
# 각 제품별 이미지 수 확인
echo "=== FT 제품 ==="
ls data/raw/FT/normal/*.jpg | wc -l
ls data/raw/FT/missing/*.jpg | wc -l
ls data/raw/FT/misaligned/*.jpg | wc -l

echo "=== RS 제품 ==="
ls data/raw/RS/normal/*.jpg | wc -l
ls data/raw/RS/missing/*.jpg | wc -l
ls data/raw/RS/misaligned/*.jpg | wc -l

echo "=== BC 제품 ==="
ls data/raw/BC/normal/*.jpg | wc -l
ls data/raw/BC/missing/*.jpg | wc -l
ls data/raw/BC/misaligned/*.jpg | wc -l
```

### Step 2: 어노테이션 완료 확인

```bash
# LabelImg 또는 Roboflow로 모든 이미지 어노테이션 완료
# 각 이미지마다 대응하는 .txt 파일 생성 확인

# 예시: FT_0001.jpg → FT_0001.txt
ls data/raw/FT/normal/*.txt | wc -l
```

### Step 3: YOLO 데이터셋 생성

**자동화 스크립트** (`tools/prepare_dataset.py`):
```python
#!/usr/bin/env python3
"""
원본 데이터를 YOLO 형식으로 변환하고 Train/Val/Test 분할
"""
import os
import shutil
from pathlib import Path
from sklearn.model_selection import train_test_split

def prepare_yolo_dataset(raw_dir, output_dir, split_ratio=(0.7, 0.2, 0.1)):
    """
    raw_dir: data/raw/ 경로
    output_dir: data/processed/pcb_components/ 경로
    split_ratio: (train, valid, test) 비율
    """
    # 디렉토리 생성
    for split in ['train', 'valid', 'test']:
        (output_dir / 'images' / split).mkdir(parents=True, exist_ok=True)
        (output_dir / 'labels' / split).mkdir(parents=True, exist_ok=True)

    # 모든 제품의 이미지 수집
    all_images = []
    for product in ['FT', 'RS', 'BC']:
        product_dir = raw_dir / product
        for category in ['normal', 'missing', 'misaligned']:
            category_dir = product_dir / category
            if category_dir.exists():
                all_images.extend(list(category_dir.glob('*.jpg')))

    # Train/Val/Test 분할
    train_images, temp_images = train_test_split(
        all_images, test_size=(1 - split_ratio[0]), random_state=42
    )
    val_images, test_images = train_test_split(
        temp_images,
        test_size=(split_ratio[2] / (split_ratio[1] + split_ratio[2])),
        random_state=42
    )

    # 파일 복사
    for split_name, images in [('train', train_images), ('valid', val_images), ('test', test_images)]:
        for img_path in images:
            # 이미지 복사
            shutil.copy(img_path, output_dir / 'images' / split_name / img_path.name)

            # 라벨 복사
            label_path = img_path.with_suffix('.txt')
            if label_path.exists():
                shutil.copy(label_path, output_dir / 'labels' / split_name / label_path.name)

    print(f"✅ 데이터셋 준비 완료!")
    print(f"  Train: {len(train_images)}장")
    print(f"  Valid: {len(val_images)}장")
    print(f"  Test: {len(test_images)}장")

if __name__ == "__main__":
    prepare_yolo_dataset(
        raw_dir=Path('data/raw'),
        output_dir=Path('data/processed/pcb_components')
    )
```

**실행**:
```bash
python3 tools/prepare_dataset.py
```

### Step 4: data.yaml 생성

**파일 경로**: `data/processed/pcb_components/data.yaml`

```yaml
# PCB Component Detection Dataset for YOLOv11l
# Product Verification Architecture v3.0

path: /home/<사용자명>/work_project/data/processed/pcb_components
train: images/train
val: images/valid
test: images/test

# 클래스 수 (실제 제품에 따라 조정)
nc: 15

# 클래스 이름
names:
  0: capacitor
  1: resistor
  2: IC
  3: LED
  4: diode
  5: transistor
  6: connector
  7: inductor
  8: relay
  9: switch
  10: potentiometer
  11: crystal
  12: fuse
  13: battery
  14: transformer
```

**주의**: `nc`와 `names`는 실제 제품에 사용되는 부품 종류에 맞게 조정하세요.

---

## 데이터 품질 확인

### 어노테이션 시각화

**스크립트** (`tools/visualize_annotations.py`):
```python
#!/usr/bin/env python3
"""YOLO 어노테이션 시각화"""
import cv2
import matplotlib.pyplot as plt
from pathlib import Path
import yaml

def visualize_yolo_annotation(image_path, label_path, class_names):
    """YOLO 어노테이션을 이미지 위에 그리기"""
    # 이미지 로드
    image = cv2.imread(str(image_path))
    image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
    h, w = image.shape[:2]

    # 라벨 로드
    if not label_path.exists():
        print(f"⚠️  라벨 파일 없음: {label_path}")
        return

    with open(label_path, 'r') as f:
        labels = f.readlines()

    # 바운딩 박스 그리기
    for label in labels:
        parts = label.strip().split()
        class_id = int(parts[0])
        x_center, y_center, width, height = map(float, parts[1:])

        # 정규화된 좌표 → 픽셀 좌표 변환
        x1 = int((x_center - width / 2) * w)
        y1 = int((y_center - height / 2) * h)
        x2 = int((x_center + width / 2) * w)
        y2 = int((y_center + height / 2) * h)

        # 박스 및 라벨 그리기
        cv2.rectangle(image, (x1, y1), (x2, y2), (0, 255, 0), 2)
        cv2.putText(
            image,
            class_names[class_id],
            (x1, y1 - 10),
            cv2.FONT_HERSHEY_SIMPLEX,
            0.6,
            (0, 255, 0),
            2
        )

    # 시각화
    plt.figure(figsize=(12, 12))
    plt.imshow(image)
    plt.title(f"Annotations: {image_path.name}")
    plt.axis('off')
    plt.tight_layout()
    plt.show()

if __name__ == "__main__":
    # data.yaml 로드
    with open('data/processed/pcb_components/data.yaml', 'r') as f:
        config = yaml.safe_load(f)
        class_names = [config['names'][i] for i in range(config['nc'])]

    # 샘플 이미지 시각화
    sample_image = Path('data/processed/pcb_components/images/train/FT_0001.jpg')
    sample_label = Path('data/processed/pcb_components/labels/train/FT_0001.txt')

    visualize_yolo_annotation(sample_image, sample_label, class_names)
```

**실행**:
```bash
python3 tools/visualize_annotations.py
```

### 데이터셋 통계 확인

```bash
# 각 분할별 이미지 수
echo "Train: $(ls data/processed/pcb_components/images/train/*.jpg | wc -l)"
echo "Valid: $(ls data/processed/pcb_components/images/valid/*.jpg | wc -l)"
echo "Test: $(ls data/processed/pcb_components/images/test/*.jpg | wc -l)"

# 이미지-라벨 매칭 확인
echo "Train 이미지: $(ls data/processed/pcb_components/images/train/*.jpg | wc -l)"
echo "Train 라벨: $(ls data/processed/pcb_components/labels/train/*.txt | wc -l)"
```

---

## 데이터 증강 (Augmentation)

### YOLO 기본 증강 (자동 적용)

YOLO v11l은 학습 시 다음 증강을 자동으로 적용:
- Random crop
- Random flip (horizontal/vertical)
- Mosaic augmentation (4장 합성)
- MixUp (이미지 혼합)
- HSV augmentation (색상, 채도, 밝기 조정)

### 추가 증강 (선택)

**데이터가 부족한 경우** (제품당 200장 미만), Roboflow 또는 Albumentations로 추가 증강:
- 회전 (±15도)
- 밝기 조정 (±20%)
- 노이즈 추가
- 블러 효과

**Roboflow 예시**:
1. https://roboflow.com/ 계정 생성 (무료)
2. 데이터셋 업로드
3. Preprocessing: Auto-Orient, Resize (640x640)
4. Augmentation: Flip (horizontal), Rotation (±15°), Brightness (±20%)
5. Generate → Export (YOLO v11 format)

---

## 데이터셋 체크리스트

학습 전 반드시 확인:

### 학습 데이터
- [ ] 총 이미지 수: 600장 이상 (권장: 1,500장 이상)
- [ ] 3개 제품 골고루 포함 (FT, RS, BC)
- [ ] Train/Val/Test 분할 완료 (70/20/10)
- [ ] 모든 이미지에 대응하는 라벨 파일 존재
- [ ] 라벨 파일 YOLO 형식 확인 (정규화된 좌표 0~1)
- [ ] data.yaml 경로 설정 완료
- [ ] 클래스 수 (nc) 정확히 설정

### 기준 데이터
- [ ] 3개 제품 Golden Sample 촬영 완료
- [ ] YOLO 모델로 부품 검출 완료
- [ ] JSON 형식으로 저장 (`server/reference_data/`)
- [ ] MySQL `product_components` 테이블 삽입 완료
- [ ] 제품별 component_count 정확히 설정

### 데이터 품질
- [ ] 어노테이션 시각화로 정확성 확인
- [ ] 바운딩 박스가 부품을 정확히 포함
- [ ] 클래스 라벨 정확 (capacitor, resistor 등)
- [ ] 좌표 값 모두 0~1 사이

---

## 다음 단계

### 1. 모델 학습 시작 ⭐

```bash
# YOLOv11l 모델 학습
yolo detect train \
  data=data/processed/pcb_components/data.yaml \
  model=yolo11l.pt \
  epochs=150 \
  imgsz=640 \
  batch=16 \
  device=0 \
  project=runs/detect \
  name=component_model_v3

# 학습 완료 후 최적 모델 저장
cp runs/detect/component_model_v3/weights/best.pt models/component_detector_v3.0.pt
```

**주의**: batch=16 권장 (VRAM 12-14GB 사용)

### 2. 성능 평가

```bash
# 평가 실행
yolo detect val \
  model=models/component_detector_v3.0.pt \
  data=data/processed/pcb_components/data.yaml

# 주요 지표 확인:
# - mAP@0.5: 객체 검출 정확도
# - Precision: 검출된 것 중 실제 부품 비율
# - Recall: 실제 부품 중 검출된 비율
```

### 3. Flask 서버 통합

`docs/Flask_Server_Setup.md` 참조:
- 모델 로드 (`models/component_detector_v3.0.pt`)
- ComponentVerifier 통합 (부품 위치 검증)
- 기준 데이터 로드 (`product_components` 테이블)

**자세한 학습 가이드**: `docs/YOLO_Training_Guide.md` 참조

---

## 참고 자료

### 이 프로젝트 관련 문서
- **시스템 아키텍처**: `PCB_Defect_Detection_Project.md`
- **Flask 서버 구현**: `Flask_Server_Setup.md`
- **MySQL 스키마**: `MySQL_Database_Design.md`
- **YOLO 학습 가이드**: `YOLO_Training_Guide.md`

### 어노테이션 도구
- [LabelImg](https://github.com/heartexlabs/labelImg) - 오픈소스, YOLO 형식 지원
- [Roboflow](https://roboflow.com/) - 웹 기반, 증강 자동화
- [CVAT](https://www.cvat.ai/) - 협업 가능, 클라우드/로컬

### YOLO 공식 문서
- [Ultralytics YOLO](https://docs.ultralytics.com/)
- [YOLOv11 Documentation](https://docs.ultralytics.com/models/yolo11/)

---

## 아카이브: 구버전 데이터셋 📦

**v2.0 이중 모델 아키텍처**에서 사용했던 공개 데이터셋:
- **FPIC-Component**: 부품 검출 (25 클래스, 6,260 이미지)
- **SolDef_AI**: 납땜 불량 (5-6 클래스, 1,150 이미지)

**변경 이유**:
- v3.0에서는 커스텀 제품(FT, RS, BC) 기반으로 전환
- 공개 데이터셋은 일반적인 PCB이며 제품별 특화 불가
- 부품 위치 검증을 위해 정확한 기준 데이터 필요

공개 데이터셋 정보는 `docs/archives/Dataset_Guide_v2.0.md` 참조

---

**작성일**: 2025-11-28
**버전**: 3.0 ⭐ (Product Verification Architecture)
**다음 단계**: 커스텀 데이터셋 수집 → YOLO 모델 학습 → 기준 데이터 생성
