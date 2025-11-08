#!/bin/bash
###############################################################################
# 일련번호 검출 시스템 셋업 스크립트
# - EasyOCR 설치
# - 데이터셋 디렉토리 구조 생성
# - data.yaml 템플릿 생성
###############################################################################

echo "================================================================================"
echo "일련번호 검출 시스템 셋업"
echo "================================================================================"

# 프로젝트 루트
PROJECT_ROOT="/home/sys1041/work_project"
DATA_DIR="${PROJECT_ROOT}/data/raw/serial_number_detection"

# Conda 환경 활성화
source ~/miniconda3/etc/profile.d/conda.sh
conda activate pcb_defect

echo ""
echo "1. EasyOCR 설치 중..."
echo "--------------------------------------------------------------------------------"
pip install easyocr

echo ""
echo "2. 데이터셋 디렉토리 생성 중..."
echo "--------------------------------------------------------------------------------"

# 디렉토리 생성
mkdir -p "${DATA_DIR}/train/images"
mkdir -p "${DATA_DIR}/train/labels"
mkdir -p "${DATA_DIR}/valid/images"
mkdir -p "${DATA_DIR}/valid/labels"
mkdir -p "${DATA_DIR}/test/images"
mkdir -p "${DATA_DIR}/test/labels"

echo "✓ 디렉토리 생성 완료:"
echo "  ${DATA_DIR}/train/images"
echo "  ${DATA_DIR}/train/labels"
echo "  ${DATA_DIR}/valid/images"
echo "  ${DATA_DIR}/valid/labels"
echo "  ${DATA_DIR}/test/images"
echo "  ${DATA_DIR}/test/labels"

echo ""
echo "3. data.yaml 템플릿 생성 중..."
echo "--------------------------------------------------------------------------------"

# data.yaml 생성
cat > "${DATA_DIR}/data.yaml" <<EOF
# 일련번호 검출 YOLO 데이터셋 설정

# 경로
path: ${DATA_DIR}
train: train/images
val: valid/images
test: test/images

# 클래스
nc: 1  # 클래스 개수 (serial_number 1개만)
names: ['serial_number']  # 클래스 이름

# 설명
# - 일련번호가 있는 영역(bbox)만 라벨링
# - LabelImg 또는 Roboflow 사용
# - YOLO 포맷 (.txt 파일)
EOF

echo "✓ data.yaml 생성 완료: ${DATA_DIR}/data.yaml"

echo ""
echo "4. README 파일 생성 중..."
echo "--------------------------------------------------------------------------------"

# README 생성
cat > "${DATA_DIR}/README.md" <<'EOF'
# 일련번호 검출 데이터셋

## 디렉토리 구조

```
serial_number_detection/
├── data.yaml           # YOLO 학습 설정 파일
├── train/
│   ├── images/        # 학습 이미지 (200장 권장)
│   └── labels/        # YOLO 라벨 (.txt)
├── valid/
│   ├── images/        # 검증 이미지 (50장 권장)
│   └── labels/        # YOLO 라벨 (.txt)
└── test/
    ├── images/        # 테스트 이미지 (50장 권장)
    └── labels/        # YOLO 라벨 (.txt)
```

## 라벨링 방법

### 1. LabelImg 설치 및 실행

```bash
conda activate pcb_defect
pip install labelImg
labelImg
```

### 2. 라벨링 설정

1. `Open Dir` → `train/images` 선택
2. `Change Save Dir` → `train/labels` 선택
3. YOLO 포맷 선택 (왼쪽 메뉴)
4. `Create Classes` → `serial_number` 입력

### 3. 박스 그리기

- 단축키 `W` 누르기
- 일련번호가 있는 영역을 마우스로 드래그
- **텍스트 전체를 포함**하도록 박스 크기 조정
- 클래스 `serial_number` 선택
- `Ctrl + S` 저장
- `D` 키로 다음 이미지

### 4. 라벨 파일 형식 (YOLO)

**파일명**: `pcb_001.txt`

**내용**:
```
0 0.5 0.15 0.4 0.08
```

**형식**:
```
class_id center_x center_y width height
   0       0.5      0.15    0.4   0.08
```

- `class_id`: 0 (serial_number 클래스)
- `center_x`: 박스 중심 X 좌표 (0.0~1.0)
- `center_y`: 박스 중심 Y 좌표 (0.0~1.0)
- `width`: 박스 너비 (0.0~1.0)
- `height`: 박스 높이 (0.0~1.0)

## 데이터셋 권장 사항

- **최소 이미지 수**: 100-300장
- **Train:Valid:Test 비율**: 70:15:15 (200:50:50)
- **이미지 크기**: 640 × 640 권장 (YOLO 기본)
- **일련번호 위치**: 같은 위치에 있으므로 적은 데이터로도 충분

## 학습 실행

```bash
conda activate pcb_defect
python scripts/train_serial_number_detector.py
```

## 자세한 내용

- `docs/Serial_Number_Detection_Guide.md` 참조
EOF

echo "✓ README.md 생성 완료: ${DATA_DIR}/README.md"

echo ""
echo "================================================================================"
echo "✓ 셋업 완료!"
echo "================================================================================"
echo ""
echo "다음 단계:"
echo ""
echo "1. PCB 이미지 수집 (100-300장)"
echo "   → ${DATA_DIR}/train/images 에 저장"
echo ""
echo "2. LabelImg로 라벨링"
echo "   labelImg"
echo ""
echo "3. YOLO 모델 학습"
echo "   python scripts/train_serial_number_detector.py"
echo ""
echo "4. 테스트"
echo "   python server/serial_number_detector.py"
echo ""
echo "자세한 내용: docs/Serial_Number_Detection_Guide.md"
echo "================================================================================"
