# PCB 불량 검사 데이터셋 가이드

> **⚠️ 중요: 데이터 수집 방식 변경**
>
> **기존 방식** (이 문서의 대부분 내용):
> - 공개 데이터셋 사용 (FPIC-Component 6,260장 + SolDef_AI 1,150장)
>
> **현재 방식** (2025-01-11 변경) ⭐:
> - **자체 촬영 데이터셋 사용**
> - 부품 검출: 직접 촬영 (목표 200-300장)
> - 납땜 불량: 직접 촬영 (목표 200-300장)
> - **실제 사용 웹캠으로 촬영하여 실전 환경 최적화**
>
> **자체 데이터 수집 가이드**: `Data_Collection_Guide_Simple.md` ⭐⭐⭐ 참조

---

## 목표
이중 YOLO 모델 아키텍처를 위한 **자체 수집** 데이터셋 준비

**핵심 변경** ⭐:
- **기존**: 공개 데이터셋 사용 (FPIC-Component, SolDef_AI)
- **신규**: 자체 촬영 데이터셋
  - **부품 검출 데이터셋**: 실제 웹캠으로 촬영 (200-300장)
  - **납땜 불량 데이터셋**: 실제 웹캠으로 촬영 (200-300장)
  - Roboflow를 통한 라벨링 및 증강

**YOLO 환경 구축 및 학습 방법은 `docs/Phase1_YOLO_Setup.md`를 참조하세요.**

**참고**:
- `Dual_Model_Architecture.md` (이중 모델 아키텍처 설계)
- `Data_Collection_Guide_Simple.md` ⭐⭐⭐ (자체 데이터 수집 완전 가이드)

---

## 이 프로젝트에서 사용하는 데이터셋 ⭐

### 현재 방식: 자체 수집 데이터셋 ⭐⭐⭐

**참고 문서**: `Data_Collection_Guide_Simple.md` (완전한 촬영 및 라벨링 가이드)

**데이터셋 구조**:
```
data/
├── custom_component/        # 부품 검출 데이터셋
│   ├── images/
│   │   ├── train/          # 70%
│   │   ├── valid/          # 20%
│   │   └── test/           # 10%
│   ├── labels/             # YOLO 형식
│   └── data.yaml
└── custom_solder/          # 납땜 불량 데이터셋
    ├── images/
    │   ├── train/
    │   ├── valid/
    │   └── test/
    ├── labels/
    └── data.yaml
```

**장점**:
- ✅ 실제 사용 환경 (웹캠, 조명, PCB)과 100% 일치
- ✅ 프로젝트 요구사항에 완벽히 맞춤화
- ✅ 라벨링 품질 완전 제어 가능
- ✅ 추가 데이터 수집 및 개선 용이

---

## 참고: 공개 데이터셋 정보 (아카이브) 📦

> **⚠️ 주의**: 아래 내용은 참고용입니다. 현재 프로젝트에서는 자체 수집 데이터셋을 사용합니다.

### 구 방식에서 고려했던 공개 데이터셋

### 1. FPIC-Component Dataset ⭐⭐⭐ (모델 1 - 부품 검출)

**출처**: FPIC-Component (IIT, India)

**설명**:
PCB 전자 부품 검출을 위한 전문 데이터셋. 25종의 전자 부품을 포함하며, 균형 잡힌 클래스 분포를 가진 고품질 데이터셋입니다.

**데이터셋 통계**:
- **이미지 수**: 6,260장
- **클래스 수**: 25개
- **라벨 객체 수**: 29,639개
- **평균 객체/이미지**: ~4.7개
- **형식**: YOLO v11l 어노테이션 (바로 사용 가능)

**25개 부품 클래스**:
```
0: capacitor         (커패시터)
1: resistor          (저항)
2: IC                (집적 회로)
3: LED               (발광 다이오드)
4: diode             (다이오드)
5: transistor        (트랜지스터)
6: connector         (커넥터)
7: inductor          (인덕터)
8: relay             (릴레이)
9: switch            (스위치)
10: potentiometer    (가변저항)
11: crystal          (크리스탈)
12: fuse             (퓨즈)
13: battery          (배터리)
14: transformer      (변압기)
15: coil             (코일)
16: sensor           (센서)
17: microcontroller  (마이크로컨트롤러)
18: capacitor_electrolytic (전해 커패시터)
19: capacitor_ceramic (세라믹 커패시터)
20: resistor_smd     (SMD 저항)
21: pad              (패드)
22: via              (비아)
23: trace            (트레이스)
24: hole             (홀)
```

**다운로드 방법**:

이 데이터셋은 IIT India에서 제공하는 학술 데이터셋입니다. 다운로드 방법:

1. **Google Drive 링크** (추천):
```bash
# gdown을 사용한 다운로드
pip install gdown

# Google Drive에서 다운로드
gdown --id <GOOGLE_DRIVE_FILE_ID> -O data/raw/fpic_component.zip

# 압축 해제
cd data/raw
unzip fpic_component.zip
```

2. **공식 사이트 접근**:
- 출처: [IIT Research Repository]
- 논문: "FPIC: A Novel Semantic Dataset for Optical PCB Assurance"
- 접근 방법: 논문 저자 연락 또는 기관 라이선스

**데이터 구조**:
```
fpic_component/
├── images/
│   ├── train/          # 4,382 images (70%)
│   ├── valid/          # 1,252 images (20%)
│   └── test/           # 626 images (10%)
├── labels/
│   ├── train/          # YOLO format .txt
│   ├── valid/
│   └── test/
└── data.yaml           # YOLO 설정 파일
```

**장점**:
- ✅ 균형 잡힌 클래스 분포 (불균형 문제 없음)
- ✅ YOLO 형식 바로 제공 (전처리 불필요)
- ✅ 고해상도 이미지 (640x640)
- ✅ 실제 산업 환경 반영
- ✅ 학술적으로 검증됨

**활용**:
- 모델 1 (좌측 카메라): 부품 존재 여부, 위치 정확도, 잘못된 부품 검출

---

### 2. SolDef_AI Dataset ⭐⭐⭐ (모델 2 - 납땜 불량)

**출처**: Roboflow Universe - SolDef_AI

**설명**:
우주항공 표준(ECSS-Q-ST-70-38C)을 따르는 고품질 납땜 불량 검출 데이터셋. 실제 산업 현장의 납땜 품질 기준을 반영합니다.

**데이터셋 통계**:
- **이미지 수**: 1,150장 (원본), 429장 (Roboflow 버전)
- **클래스 수**: 5-6개
- **형식**: YOLO v11l 어노테이션 (바로 사용 가능)
- **표준**: ECSS-Q-ST-70-38C (유럽우주국 납땜 표준)

**5-6개 납땜 불량 클래스**:
```
0: no_good         (일반적인 납땜 불량)
1: exc_solder      (과다 납땜 - Excessive Solder)
2: spike           (납땜 스파이크)
3: poor_solder     (불충분한 납땜 - Poor Solder Joint)
4: solder_bridge   (납땜 브릿지 - 치명적 결함 ⚠️)
5: tombstone       (툼스톤 현상 - 선택적)
```

**심각도 분류**:
- **치명적 (Critical)**: solder_bridge → 즉시 폐기
- **심각 (Major)**: exc_solder, poor_solder → 수리 필요
- **경미 (Minor)**: spike, no_good → 재검사 필요

**다운로드 방법 (Roboflow)** ⭐:

```bash
# 1. Roboflow API 설치
pip install roboflow

# 2. Python 스크립트로 다운로드
python3 << 'EOF'
from roboflow import Roboflow

# API 키 설정 (Roboflow 웹사이트에서 발급)
rf = Roboflow(api_key="YOUR_ROBOFLOW_API_KEY")

# SolDef_AI 프로젝트 접근
project = rf.workspace("soldef-ai").project("soldering-defects")
dataset = project.version(1).download("yolo11")

print("✅ SolDef_AI 데이터셋 다운로드 완료!")
print(f"경로: {dataset.location}")
EOF
```

**또는 웹 UI 다운로드**:
1. https://universe.roboflow.com/soldef-ai/soldering-defects 접속
2. "Download Dataset" 클릭
3. Format: "YOLO v11l" 선택
4. 다운로드 후 `data/raw/soldef_ai/`에 압축 해제

**데이터 구조**:
```
soldef_ai/
├── train/
│   ├── images/         # 300 images (70%)
│   └── labels/         # YOLO format .txt
├── valid/
│   ├── images/         # 86 images (20%)
│   └── labels/
├── test/
│   ├── images/         # 43 images (10%)
│   └── labels/
└── data.yaml           # YOLO 설정 파일
```

**data.yaml 예시**:
```yaml
names:
  - no_good
  - exc_solder
  - spike
  - poor_solder
  - solder_bridge
  - tombstone

nc: 6
train: train/images
val: valid/images
test: test/images
```

**장점**:
- ✅ 우주항공 표준 기반 (ECSS-Q-ST-70-38C)
- ✅ YOLO 형식 바로 제공
- ✅ Roboflow에서 간편 다운로드
- ✅ 실제 산업 납땜 기준 반영
- ✅ 치명적 결함 명확히 정의됨

**활용**:
- 모델 2 (우측 카메라): 납땜 품질 검사, 브릿지 검출, 과다/불충분 납땜 검출

---

## 데이터셋 준비 절차 ⭐

### Step 1: 두 데이터셋 다운로드

```bash
# 프로젝트 루트로 이동
cd ~/work_project

# 데이터 폴더 생성
mkdir -p data/raw
cd data/raw

# 1. FPIC-Component 다운로드
# (Google Drive 링크 또는 공식 사이트에서 수동 다운로드)
gdown --id <FILE_ID> -O fpic_component.zip
unzip fpic_component.zip -d fpic_component/

# 2. SolDef_AI 다운로드 (Roboflow)
pip install roboflow
python3 download_soldef.py  # 위 스크립트 사용
```

### Step 2: 데이터셋 구조 확인

```bash
# FPIC-Component 구조 확인
echo "=== FPIC-Component ==="
ls -R fpic_component/

# SolDef_AI 구조 확인
echo "=== SolDef_AI ==="
ls -R soldef_ai/

# 이미지 수 확인
echo "FPIC-Component train images: $(ls fpic_component/images/train/ | wc -l)"
echo "SolDef_AI train images: $(ls soldef_ai/train/images/ | wc -l)"
```

### Step 3: YOLO 형식으로 통합

```bash
# 통합 데이터셋 폴더 생성
mkdir -p ../processed/component_model
mkdir -p ../processed/solder_model

# FPIC-Component 복사 (이미 YOLO 형식)
cp -r fpic_component/* ../processed/component_model/

# SolDef_AI 복사 (이미 YOLO 형식)
cp -r soldef_ai/* ../processed/solder_model/
```

### Step 4: data.yaml 생성

**Component Model** (`data/processed/component_model/data.yaml`):
```yaml
# FPIC-Component Dataset for YOLOv11l

path: /home/<사용자명>/work_project/data/processed/component_model
train: images/train
val: images/valid
test: images/test

nc: 25

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
  15: coil
  16: sensor
  17: microcontroller
  18: capacitor_electrolytic
  19: capacitor_ceramic
  20: resistor_smd
  21: pad
  22: via
  23: trace
  24: hole
```

**Solder Model** (`data/processed/solder_model/data.yaml`):
```yaml
# SolDef_AI Dataset for YOLOv11l

path: /home/<사용자명>/work_project/data/processed/solder_model
train: train/images
val: valid/images
test: test/images

nc: 6

names:
  0: no_good
  1: exc_solder
  2: spike
  3: poor_solder
  4: solder_bridge
  5: tombstone
```

---

## 참고: 구버전 데이터셋 (아카이브) 📦

이 프로젝트는 이전에 다음 데이터셋들을 사용했으나, **이중 모델 아키텍처 전환**으로 인해 더 이상 사용하지 않습니다:

### 아카이브된 데이터셋
- **DeepPCB Dataset**: 6가지 PCB 불량 (Open, Short, Mouse bite 등)
- **Kaggle PCB Defects**: 1,386장 (Akhatova)
- **병합 데이터셋**: 22-29 클래스 (심각한 클래스 불균형)

**변경 이유**:
- 클래스 불균형 문제 (일부 클래스 < 50 샘플)
- 부품 검출 + 납땜 불량이 혼재되어 학습 효율 저하
- 전문화된 모델이 더 높은 정확도 달성

**참고 링크** (학습 자료용):
- DeepPCB: https://github.com/tangsanli5201/DeepPCB
- Kaggle: https://www.kaggle.com/datasets/akhatova/pcb-defects
- Roboflow Universe: https://universe.roboflow.com/search?q=pcb+defect

---

## 데이터 전처리 가이드

**참고**: FPIC-Component와 SolDef_AI는 이미 YOLO 형식으로 제공되므로 대부분의 전처리가 불필요합니다.

### YOLO 어노테이션 형식 (참고)

```
<class_id> <x_center> <y_center> <width> <height>
```

- 모든 좌표는 **정규화된 값** (0~1 사이)
- `x_center`, `y_center`: 바운딩 박스 중심점
- `width`, `height`: 바운딩 박스 너비/높이

**예시** (image_001.txt):
```
0 0.5 0.5 0.2 0.3
1 0.3 0.7 0.15 0.1
```

### 데이터 증강 (Augmentation)

#### YOLO 기본 증강 (자동 적용)
YOLO v11l은 학습 시 다음 증강을 자동으로 적용:
- Random crop
- Random flip (horizontal/vertical)
- Mosaic augmentation
- MixUp
- HSV augmentation (색상, 채도, 밝기)

**권장사항**: FPIC-Component와 SolDef_AI는 충분한 데이터 양과 증강을 제공하므로 추가 증강은 선택 사항입니다.

---

## 데이터 품질 확인

### 데이터 시각화 스크립트

`visualize_dataset.py`:

```python
import cv2
import matplotlib.pyplot as plt
from pathlib import Path
import yaml

def visualize_yolo_annotation(image_path, label_path, class_names):
    """YOLO 어노테이션을 시각화"""
    # 이미지 로드
    image = cv2.imread(str(image_path))
    image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
    h, w = image.shape[:2]

    # 라벨 로드
    with open(label_path, 'r') as f:
        labels = f.readlines()

    # 바운딩 박스 그리기
    for label in labels:
        parts = label.strip().split()
        class_id = int(parts[0])
        x_center, y_center, width, height = map(float, parts[1:])

        # 픽셀 좌표로 변환
        x1 = int((x_center - width / 2) * w)
        y1 = int((y_center - height / 2) * h)
        x2 = int((x_center + width / 2) * w)
        y2 = int((y_center + height / 2) * h)

        # 박스 및 라벨 그리기
        cv2.rectangle(image, (x1, y1), (x2, y2), (0, 255, 0), 2)
        cv2.putText(image, class_names[class_id], (x1, y1 - 10),
                    cv2.FONT_HERSHEY_SIMPLEX, 0.5, (0, 255, 0), 2)

    plt.figure(figsize=(10, 10))
    plt.imshow(image)
    plt.axis('off')
    plt.show()

# 사용 예시 - Component Model
with open('data/processed/component_model/data.yaml', 'r') as f:
    config = yaml.safe_load(f)
    class_names = [config['names'][i] for i in range(config['nc'])]

visualize_yolo_annotation(
    'data/processed/component_model/images/train/image_001.jpg',
    'data/processed/component_model/labels/train/image_001.txt',
    class_names
)
```

**활용**: 두 모델 모두에 적용 가능 (경로만 변경)

---

## 데이터셋 체크리스트

학습 전 반드시 확인:

### Component Model (FPIC-Component)
- [ ] 이미지 수: 6,260장 확인
- [ ] 클래스 수: 25개 확인
- [ ] data.yaml 경로 설정 완료
- [ ] Train/Val/Test 분할 확인 (70/20/10)
- [ ] 클래스 불균형 없음 확인 (균형 잡힌 분포)

### Solder Model (SolDef_AI)
- [ ] 이미지 수: 429장 확인 (Roboflow 버전)
- [ ] 클래스 수: 5-6개 확인
- [ ] data.yaml 경로 설정 완료
- [ ] Train/Val/Test 분할 확인 (70/20/10)
- [ ] 치명적 결함 클래스 확인 (solder_bridge)

### 공통
- [ ] 이미지와 라벨 파일 이름 동일 확인
- [ ] 라벨 파일 YOLO 형식 확인 (정규화된 좌표)
- [ ] 모든 좌표 값 0~1 사이 확인
- [ ] 데이터 시각화로 어노테이션 정확성 확인

---

## 다음 단계 ⭐

### 1. 자체 데이터 수집 시작 (우선)
```bash
# 촬영 스크립트 사용 (Data_Collection_Guide_Simple.md 참조)
conda activate pcb_defect
python scripts/capture_simple.py

# 목표:
# - 부품 검출: 200-300장
# - 납땜 불량: 200-300장
# - Roboflow 라벨링 및 증강 (3배)
```

**완전한 가이드**: `Data_Collection_Guide_Simple.md` ⭐⭐⭐

### 2. 라벨링 완료 후 모델 학습 ⭐
```bash
# Component Model (자체 데이터셋) 학습
yolo detect train \
  data=data/custom_component/data.yaml \
  model=yolo11l.pt \
  epochs=100-150 \
  imgsz=640 \
  batch=16 \
  device=0 \
  project=runs/detect \
  name=component_model

# Solder Model (자체 데이터셋) 학습
yolo detect train \
  data=data/custom_solder/data.yaml \
  model=yolo11l.pt \
  epochs=100-150 \
  imgsz=640 \
  batch=16 \
  device=0 \
  project=runs/detect \
  name=solder_model
```

### 3. 성능 평가
- Component Model: mAP@0.5, Precision, Recall
- Solder Model: mAP@0.5, Precision, Recall
- 실제 환경 테스트 (웹캠으로 실시간 추론)

### 4. Flask 서버 통합
- `docs/Flask_Server_Setup.md` 참조
- 이중 모델 로드 및 결과 융합 로직 구현

**자세한 학습 가이드**: `docs/YOLO_Training_Guide.md` 참조

---

## 참고 자료

### 이 프로젝트 관련 문서
- **이중 모델 아키텍처**: `Dual_Model_Architecture.md`
- **Flask 서버 구현**: `Flask_Server_Setup.md`
- **YOLO 학습 가이드**: `YOLO_Training_Guide.md`
- **프로젝트 전체 개요**: `PCB_Defect_Detection_Project.md`

### 데이터셋 출처
- **FPIC-Component**: IIT India (논문: "FPIC: A Novel Semantic Dataset for Optical PCB Assurance")
- **SolDef_AI**: Roboflow Universe - https://universe.roboflow.com/soldef-ai/soldering-defects
- **우주항공 표준**: ECSS-Q-ST-70-38C (European Space Agency)

### 도구
- [Roboflow](https://roboflow.com/) - SolDef_AI 다운로드
- [Ultralytics YOLO](https://docs.ultralytics.com/) - 모델 학습 프레임워크 (YOLOv11l)

---

**작성일**: 2025-10-31
**업데이트**: 2025-01-11 (자체 데이터셋 수집 방식으로 변경)
**버전**: 3.0 ⭐⭐ (자체 촬영 데이터셋 사용)
**다음 단계**: 자체 데이터 수집 (`Data_Collection_Guide_Simple.md`) → 모델 학습 → Flask 서버 통합
