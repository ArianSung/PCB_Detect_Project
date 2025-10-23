# PCB 불량 검사 데이터셋 가이드

## 목표
PCB 불량 검출을 위한 공개 데이터셋을 수집하고, YOLO v8 형식으로 전처리하여 학습 준비를 완료합니다.

**YOLO 환경 구축 및 학습 방법은 `docs/Phase1_YOLO_Setup.md`를 참조하세요.**

---

## 공개 PCB 불량 데이터셋 목록

### 1. DeepPCB Dataset ⭐ 추천

**설명**: PCB 결함 검출을 위한 대표적인 오픈소스 데이터셋

**특징**:
- 총 1,500+ 이미지 쌍 (정상/불량)
- 해상도: 640x640
- 6가지 불량 유형: Open, Short, Mouse bite, Spur, Copper, Pin-hole
- Template matching 방식 지원

**다운로드**:
- GitHub: https://github.com/tangsanli5201/DeepPCB
- 논문: "DeepPCB: A Deep Learning Framework for PCB Defect Detection"

**장점**:
- 잘 정리된 데이터셋
- 학술 연구에 많이 사용됨
- 정상/불량 이미지 쌍으로 제공

**단점**:
- YOLO 형식 어노테이션이 아님 (변환 필요)
- 실제 산업 현장과 차이 있을 수 있음

**사용법**:
```bash
# GitHub에서 클론
git clone https://github.com/tangsanli5201/DeepPCB.git
cd DeepPCB

# 데이터 구조 확인
ls PCBData/
# 출력: train/ test/
```

---

### 2. Kaggle - PCB Defects Dataset

**설명**: Kaggle에서 제공하는 다양한 PCB 불량 데이터셋

#### 2-1. "PCB Defects" by Akhatova
- **링크**: https://www.kaggle.com/datasets/akhatova/pcb-defects
- **이미지 수**: 1,386장
- **불량 유형**: 6가지 (Missing hole, Mouse bite, Open circuit, Short, Spur, Spurious copper)
- **형식**: CSV 파일 (bbox 좌표 포함)

#### 2-2. "PCB Defect Detection" by Tanishq Gautam
- **링크**: https://www.kaggle.com/datasets/tanishqgautam/pcb-defect-detection
- **이미지 수**: 693장
- **형식**: Pascal VOC XML

#### 2-3. "Solder Joint Quality Detection"
- **링크**: https://www.kaggle.com/search?q=solder+joint
- **특징**: 납땜 품질에 특화된 데이터셋

**다운로드 방법**:
```bash
# Kaggle API 설치
pip install kaggle

# Kaggle API 토큰 설정
# 1. Kaggle 계정 생성
# 2. Account -> Create New API Token (kaggle.json 다운로드)
# 3. WSL에서 설정
mkdir -p ~/.kaggle
cp /mnt/c/Users/<사용자명>/Downloads/kaggle.json ~/.kaggle/
chmod 600 ~/.kaggle/kaggle.json

# 데이터셋 다운로드
kaggle datasets download -d akhatova/pcb-defects
unzip pcb-defects.zip -d data/raw/pcb_defects_kaggle
```

---

### 3. Roboflow Universe - PCB Dataset

**설명**: Roboflow에서 제공하는 다양한 PCB 프로젝트

**특징**:
- YOLO 형식으로 바로 다운로드 가능 ⭐
- 여러 커뮤니티 프로젝트 존재
- 데이터 증강 자동 적용 옵션

**추천 데이터셋**:

#### 3-1. "PCB Defects" Project
- **링크**: https://universe.roboflow.com/search?q=pcb+defect
- **이미지 수**: 다양 (프로젝트마다 상이)
- **형식**: YOLO, COCO, Pascal VOC 선택 가능

#### 3-2. "Solder Joint Inspection"
- **링크**: https://universe.roboflow.com/search?q=solder
- **특징**: 납땜 검사에 특화

**다운로드 방법**:
```bash
# Roboflow API 사용
pip install roboflow

# Python 코드로 다운로드
from roboflow import Roboflow

rf = Roboflow(api_key="YOUR_API_KEY")
project = rf.workspace("workspace-name").project("project-name")
dataset = project.version(1).download("yolov8")
```

**장점**:
- YOLO 형식 바로 제공
- 온라인에서 어노테이션 가능
- 데이터 증강 자동화

---

### 4. Open Images Dataset - Electronics Category

**설명**: Google의 대규모 오픈 이미지 데이터셋

**링크**: https://storage.googleapis.com/openimages/web/index.html

**특징**:
- 'Electronics' 카테고리에서 PCB 관련 이미지 검색 가능
- 수백만 장의 이미지
- 다양한 각도/조명 조건

**다운로드**:
```bash
# OIDv6 툴킷 사용
pip install oidv6

# 특정 클래스 다운로드 (예: circuit board)
oidv6 downloader --classes "Circuit board" --type_csv train --limit 1000
```

**단점**:
- PCB 불량 검출에 특화되지 않음
- 추가 어노테이션 필요

---

### 5. MVTec Anomaly Detection Dataset (참고용)

**설명**: 산업 이상 탐지 벤치마크 데이터셋

**링크**: https://www.mvtec.com/company/research/datasets/mvtec-ad

**특징**:
- 'Transistor', 'PCB' 카테고리 포함
- 정상 이미지 위주 (이상 탐지 모델 학습용)
- 고해상도 이미지

**용도**:
- **이상 탐지 모델** 학습에 적합
- YOLO 학습보다는 AutoEncoder, PaDiM 학습에 사용

**다운로드**:
```bash
# 공식 사이트에서 수동 다운로드 필요
# 또는 Anomalib 라이브러리 사용
pip install anomalib

# Anomalib이 자동으로 다운로드
from anomalib.data import MVTec
datamodule = MVTec(category="transistor")
```

---

## 데이터셋 선택 가이드

### 프로젝트 초기 테스트용 (현재 단계)

**추천**: Roboflow Universe PCB 프로젝트

**이유**:
- YOLO 형식 바로 제공 (변환 불필요)
- 다운로드 간편
- 빠른 실습 가능

**예상 소요 시간**: 1-2시간

---

### 본격적인 학습용

**추천**: DeepPCB + Kaggle PCB Defects 조합

**이유**:
- 충분한 데이터 양 확보
- 다양한 불량 유형
- 학술적으로 검증됨

**예상 소요 시간**: 1-2일 (전처리 포함)

---

### 이상 탐지 모델 학습용

**추천**: MVTec AD Dataset

**이유**:
- 정상 이미지 위주로 구성
- Anomaly Detection 표준 데이터셋
- 벤치마크 가능

---

## 데이터 전처리 가이드

### Step 1: 데이터 다운로드 및 확인

```bash
# 프로젝트 폴더로 이동
cd ~/work_project

# 데이터 폴더 생성
mkdir -p data/raw
cd data/raw

# 예시: Roboflow에서 다운로드 (이미 YOLO 형식)
# 또는 DeepPCB 클론
git clone https://github.com/tangsanli5201/DeepPCB.git
```

**데이터 구조 확인**:
```bash
ls -R DeepPCB/PCBData/
```

---

### Step 2: YOLO 형식으로 변환 (필요 시)

#### YOLO 어노테이션 형식

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

#### Pascal VOC XML → YOLO 변환

`convert_voc_to_yolo.py` 스크립트:

```python
import os
import xml.etree.ElementTree as ET
from pathlib import Path

def convert_voc_to_yolo(xml_path, output_path, class_names):
    """
    Pascal VOC XML을 YOLO 형식으로 변환

    Args:
        xml_path: XML 파일 경로
        output_path: 출력 txt 파일 경로
        class_names: 클래스 이름 리스트 (예: ['open', 'short', 'mousebite'])
    """
    tree = ET.parse(xml_path)
    root = tree.getroot()

    # 이미지 크기 가져오기
    size = root.find('size')
    img_width = int(size.find('width').text)
    img_height = int(size.find('height').text)

    yolo_annotations = []

    # 각 객체에 대해
    for obj in root.findall('object'):
        class_name = obj.find('name').text
        if class_name not in class_names:
            continue

        class_id = class_names.index(class_name)

        # 바운딩 박스 좌표
        bbox = obj.find('bndbox')
        xmin = float(bbox.find('xmin').text)
        ymin = float(bbox.find('ymin').text)
        xmax = float(bbox.find('xmax').text)
        ymax = float(bbox.find('ymax').text)

        # YOLO 형식으로 변환 (정규화)
        x_center = ((xmin + xmax) / 2) / img_width
        y_center = ((ymin + ymax) / 2) / img_height
        width = (xmax - xmin) / img_width
        height = (ymax - ymin) / img_height

        yolo_annotations.append(f"{class_id} {x_center} {y_center} {width} {height}")

    # 파일 저장
    with open(output_path, 'w') as f:
        f.write('\n'.join(yolo_annotations))

# 사용 예시
# 클래스 이름은 data/pcb_defects.yaml에 정의된 것을 사용
# YOLO 클래스 순서: 0-10 (총 11개)
import yaml

# data/pcb_defects.yaml에서 클래스 로드
with open('data/pcb_defects.yaml', 'r') as f:
    config = yaml.safe_load(f)
    class_names = [config['names'][i] for i in range(config['nc'])]

print(f"로드된 클래스 ({len(class_names)}개): {class_names}")

xml_dir = 'data/raw/annotations/'
output_dir = 'data/processed/labels/'
os.makedirs(output_dir, exist_ok=True)

for xml_file in Path(xml_dir).glob('*.xml'):
    output_file = output_dir / (xml_file.stem + '.txt')
    convert_voc_to_yolo(xml_file, output_file, class_names)

print(f"변환 완료: {len(list(Path(output_dir).glob('*.txt')))} 파일")
```

---

### Step 3: 데이터 분할 (Train/Val/Test)

`split_dataset.py` 스크립트:

```python
import os
import shutil
from pathlib import Path
from sklearn.model_selection import train_test_split

def split_dataset(image_dir, label_dir, output_dir, train_ratio=0.7, val_ratio=0.2, test_ratio=0.1):
    """
    데이터셋을 Train/Val/Test로 분할

    Args:
        image_dir: 이미지 폴더 경로
        label_dir: 라벨 폴더 경로
        output_dir: 출력 폴더 경로
        train_ratio: 학습 데이터 비율
        val_ratio: 검증 데이터 비율
        test_ratio: 테스트 데이터 비율
    """
    assert train_ratio + val_ratio + test_ratio == 1.0, "비율의 합은 1.0이어야 합니다"

    # 이미지 파일 목록
    image_files = list(Path(image_dir).glob('*.jpg')) + list(Path(image_dir).glob('*.png'))

    # Train/Temp 분할
    train_files, temp_files = train_test_split(
        image_files,
        test_size=(val_ratio + test_ratio),
        random_state=42
    )

    # Val/Test 분할
    val_files, test_files = train_test_split(
        temp_files,
        test_size=test_ratio / (val_ratio + test_ratio),
        random_state=42
    )

    # 폴더 생성
    for split in ['train', 'val', 'test']:
        os.makedirs(f"{output_dir}/{split}/images", exist_ok=True)
        os.makedirs(f"{output_dir}/{split}/labels", exist_ok=True)

    # 파일 복사
    def copy_files(file_list, split_name):
        for img_path in file_list:
            # 이미지 복사
            shutil.copy(img_path, f"{output_dir}/{split_name}/images/{img_path.name}")

            # 라벨 복사
            label_path = Path(label_dir) / (img_path.stem + '.txt')
            if label_path.exists():
                shutil.copy(label_path, f"{output_dir}/{split_name}/labels/{label_path.name}")

    copy_files(train_files, 'train')
    copy_files(val_files, 'val')
    copy_files(test_files, 'test')

    print(f"데이터 분할 완료:")
    print(f"  Train: {len(train_files)} 이미지")
    print(f"  Val: {len(val_files)} 이미지")
    print(f"  Test: {len(test_files)} 이미지")

# 사용 예시
split_dataset(
    image_dir='data/raw/images',
    label_dir='data/raw/labels',
    output_dir='data/processed'
)
```

실행:
```bash
cd ~/work_project
python split_dataset.py
```

---

### Step 4: 데이터셋 YAML 파일 생성

YOLO 학습에 필요한 `data.yaml` 파일 생성:

`data/pcb_defects.yaml`:

```yaml
# PCB Defects Dataset

# 데이터셋 경로 (절대 경로 또는 상대 경로)
path: /home/<사용자명>/work_project/data/processed
train: train/images
val: val/images
test: test/images

# 클래스 수
nc: 6

# 클래스 이름
names:
  0: open
  1: short
  2: mousebite
  3: spur
  4: copper
  5: pin-hole
```

**주의**: `path`는 절대 경로로 설정하는 것이 안전합니다.

---

### Step 5: 데이터 증강 (Augmentation)

#### YOLO 기본 증강 (자동 적용)
YOLO v8은 학습 시 다음 증강을 자동으로 적용:
- Random crop
- Random flip (horizontal/vertical)
- Mosaic augmentation
- MixUp
- HSV augmentation (색상, 채도, 밝기)

#### 추가 증강 (선택)

`augment_data.py`:

```python
import albumentations as A
import cv2
from pathlib import Path

# Augmentation 파이프라인
transform = A.Compose([
    A.RandomRotate90(p=0.5),
    A.HorizontalFlip(p=0.5),
    A.VerticalFlip(p=0.5),
    A.GaussNoise(p=0.3),
    A.OneOf([
        A.MotionBlur(p=0.5),
        A.MedianBlur(blur_limit=3, p=0.5),
        A.Blur(blur_limit=3, p=0.5),
    ], p=0.3),
    A.ShiftScaleRotate(shift_limit=0.0625, scale_limit=0.1, rotate_limit=15, p=0.5),
    A.OneOf([
        A.OpticalDistortion(p=0.5),
        A.GridDistortion(p=0.5),
    ], p=0.3),
    A.RandomBrightnessContrast(p=0.3),
], bbox_params=A.BboxParams(format='yolo', label_fields=['class_labels']))

# 사용 예시
image = cv2.imread('image.jpg')
bboxes = [[0.5, 0.5, 0.2, 0.3]]  # YOLO format
class_labels = [0]

transformed = transform(image=image, bboxes=bboxes, class_labels=class_labels)
augmented_image = transformed['image']
augmented_bboxes = transformed['bboxes']
```

**주의**: 과도한 증강은 오히려 성능을 저하시킬 수 있습니다.

---

## 데이터 품질 확인

### Step 1: 데이터 시각화

`visualize_dataset.py`:

```python
import cv2
import matplotlib.pyplot as plt
from pathlib import Path

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

# 사용 예시
# data/pcb_defects.yaml에서 클래스 로드
import yaml
with open('data/pcb_defects.yaml', 'r') as f:
    config = yaml.safe_load(f)
    class_names = [config['names'][i] for i in range(config['nc'])]

visualize_yolo_annotation(
    'data/processed/train/images/image_001.jpg',
    'data/processed/train/labels/image_001.txt',
    class_names
)
```

---

### Step 2: 데이터 분포 분석

`analyze_dataset.py`:

```python
import os
from pathlib import Path
import matplotlib.pyplot as plt
import seaborn as sns
from collections import Counter

def analyze_dataset(label_dir, class_names):
    """데이터셋의 클래스 분포 분석"""
    all_classes = []

    # 모든 라벨 파일 읽기
    for label_file in Path(label_dir).glob('*.txt'):
        with open(label_file, 'r') as f:
            for line in f:
                class_id = int(line.strip().split()[0])
                all_classes.append(class_id)

    # 클래스별 개수 계산
    class_counts = Counter(all_classes)

    # 시각화
    plt.figure(figsize=(12, 6))

    plt.subplot(1, 2, 1)
    plt.bar([class_names[i] for i in sorted(class_counts.keys())],
            [class_counts[i] for i in sorted(class_counts.keys())])
    plt.xlabel('Class')
    plt.ylabel('Count')
    plt.title('Class Distribution')
    plt.xticks(rotation=45)

    plt.subplot(1, 2, 2)
    plt.pie([class_counts[i] for i in sorted(class_counts.keys())],
            labels=[class_names[i] for i in sorted(class_counts.keys())],
            autopct='%1.1f%%')
    plt.title('Class Percentage')

    plt.tight_layout()
    plt.savefig('results/class_distribution.png')
    plt.show()

    # 통계 출력
    print(f"총 객체 수: {len(all_classes)}")
    for class_id, count in sorted(class_counts.items()):
        print(f"  {class_names[class_id]}: {count} ({count/len(all_classes)*100:.1f}%)")

# 사용 예시
# data/pcb_defects.yaml에서 클래스 로드
import yaml
with open('data/pcb_defects.yaml', 'r') as f:
    config = yaml.safe_load(f)
    class_names = [config['names'][i] for i in range(config['nc'])]

analyze_dataset('data/processed/train/labels', class_names)
```

**분석 항목**:
- 클래스별 샘플 수
- 클래스 불균형 확인
- 이미지 해상도 분포
- 바운딩 박스 크기 분포

---

## 클래스 불균형 해결 방법

### 1. 가중치 적용

```python
# YOLO 학습 시 클래스별 가중치 설정
from ultralytics import YOLO

model = YOLO('yolov8n.pt')
model.train(
    data='data/pcb_defects.yaml',
    epochs=100,
    imgsz=640,
    # 클래스 가중치 (적은 클래스에 높은 가중치)
    cls_weight=[1.0, 1.5, 2.0, 1.2, 1.0, 1.8]
)
```

### 2. 오버샘플링

```python
# 적은 클래스의 이미지를 복제
import shutil
from pathlib import Path

def oversample_minority_class(label_dir, image_dir, target_class, factor=2):
    """특정 클래스를 포함하는 이미지를 factor배 만큼 복제"""
    for label_file in Path(label_dir).glob('*.txt'):
        with open(label_file, 'r') as f:
            classes = [int(line.split()[0]) for line in f]

        if target_class in classes:
            # 이미지와 라벨 복사
            for i in range(factor - 1):
                img_file = Path(image_dir) / (label_file.stem + '.jpg')
                new_img = Path(image_dir) / f"{label_file.stem}_aug{i}.jpg"
                new_label = Path(label_dir) / f"{label_file.stem}_aug{i}.txt"

                shutil.copy(img_file, new_img)
                shutil.copy(label_file, new_label)

# 사용 예시: 'mousebite' 클래스(class_id=2)를 2배로 증가
oversample_minority_class('data/processed/train/labels',
                          'data/processed/train/images',
                          target_class=2, factor=2)
```

---

## 데이터셋 체크리스트

학습 전 반드시 확인:

- [ ] 이미지와 라벨 파일 이름이 동일한가? (확장자 제외)
- [ ] 라벨 파일이 YOLO 형식인가? (정규화된 좌표)
- [ ] 모든 좌표 값이 0~1 사이인가?
- [ ] data.yaml 파일이 올바른 경로를 가리키는가?
- [ ] 클래스 수(nc)와 클래스 이름(names)이 일치하는가?
- [ ] Train/Val/Test 분할이 적절한가?
- [ ] 클래스 불균형이 심하지 않은가? (최대 10:1 이내 권장)
- [ ] 데이터 시각화로 어노테이션 확인했는가?
- [ ] 이미지 해상도가 일관적인가? (또는 리사이징 필요)

---

## 추천 데이터셋 조합 (PCB 프로젝트)

### 옵션 1: 빠른 시작 (1-2시간)
- **Roboflow Universe PCB 프로젝트** 1개
- 바로 YOLO 형식 다운로드
- 300-500 이미지 정도

### 옵션 2: 균형잡힌 학습 (1-2일)
- **Kaggle PCB Defects** (1,386장)
- **DeepPCB** (1,500장)
- 총 2,886장 → Train: 2,020 / Val: 577 / Test: 289

### 옵션 3: 대규모 학습 (3-5일)
- 위 데이터셋 + **Roboflow 여러 프로젝트**
- 데이터 증강 적극 활용
- 5,000+ 이미지

**프로젝트 초기 권장**: 옵션 1 또는 옵션 2

---

## 다음 단계

데이터 준비가 완료되면:

1. **YOLO 학습 시작**
   - `Phase1_YOLO_Setup.md`의 학습 가이드 참고
   - 기본 모델(YOLOv8s)로 학습 시작

2. **성능 평가**
   - mAP, Precision, Recall 측정
   - 클래스별 성능 분석

3. **모델 개선**
   - 하이퍼파라미터 튜닝
   - 데이터 증강 조정

---

## 참고 자료

### 데이터셋
- [DeepPCB GitHub](https://github.com/tangsanli5201/DeepPCB)
- [Kaggle PCB Datasets](https://www.kaggle.com/search?q=pcb+defect)
- [Roboflow Universe](https://universe.roboflow.com/)
- [MVTec AD](https://www.mvtec.com/company/research/datasets/mvtec-ad)

### 도구
- [Roboflow](https://roboflow.com/) - 온라인 어노테이션 및 데이터 관리
- [LabelImg](https://github.com/heartexlabs/labelImg) - 로컬 어노테이션 도구
- [CVAT](https://github.com/opencv/cvat) - 고급 어노테이션 플랫폼

### 논문
- "DeepPCB: A Deep Learning Framework for PCB Defect Detection" (2019)
- "PCB Defect Detection Using Deep Learning" - 관련 Survey 논문

---

**작성일**: 2025-10-22
**버전**: 1.0
**다음 단계**: Phase 3 YOLO 학습
