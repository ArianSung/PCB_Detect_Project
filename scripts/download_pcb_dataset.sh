#!/bin/bash
# PCB 불량 데이터셋 다운로드 스크립트
#
# 사용법:
#   bash scripts/download_pcb_dataset.sh
#
# 필수 조건:
#   - conda activate pcb_defect
#   - pip install roboflow

set -e  # 에러 발생 시 중단

echo "======================================"
echo "PCB 불량 데이터셋 다운로드"
echo "======================================"

# 프로젝트 루트로 이동
cd "$(dirname "$0")/.."
PROJECT_ROOT=$(pwd)

echo "프로젝트 루트: $PROJECT_ROOT"

# Roboflow 설치 확인
echo ""
echo "[1/5] Roboflow 라이브러리 확인..."
if ! python -c "import roboflow" 2>/dev/null; then
    echo "Roboflow 설치 중..."
    pip install roboflow
else
    echo "✓ Roboflow 이미 설치됨"
fi

# 데이터 폴더 생성
echo ""
echo "[2/5] 데이터 폴더 생성..."
mkdir -p data/raw
mkdir -p data/processed/{train,val,test}/{images,labels}

# Python 스크립트로 Roboflow에서 다운로드
echo ""
echo "[3/5] Roboflow에서 PCB 데이터셋 다운로드..."
echo "주의: Roboflow API 키가 필요합니다."
echo ""
echo "API 키 받는 방법:"
echo "1. https://roboflow.com/ 회원가입 (무료)"
echo "2. 로그인 후 Settings → API → Private API Key 복사"
echo ""
read -p "Roboflow API 키를 입력하세요: " API_KEY

if [ -z "$API_KEY" ]; then
    echo "API 키가 입력되지 않았습니다. 종료합니다."
    exit 1
fi

# Python 스크립트 실행
python << EOF
from roboflow import Roboflow
import os

print("\nRoboflow 인증 중...")
rf = Roboflow(api_key="$API_KEY")

print("PCB 불량 데이터셋 다운로드 중...")
print("데이터셋: PCB Defects (공개 데이터셋)")

try:
    # PCB Defects 공개 데이터셋 다운로드
    project = rf.workspace("roboflow-100").project("pcb-defects")
    dataset = project.version(1).download("yolov8", location="data/raw/pcb_defects")

    print("\n✓ 데이터셋 다운로드 완료!")
    print(f"저장 위치: data/raw/pcb_defects")

except Exception as e:
    print(f"\n✗ 다운로드 실패: {e}")
    print("\n대안: 수동 다운로드")
    print("1. https://universe.roboflow.com/roboflow-100/pcb-defects 방문")
    print("2. 'Download' → 'YOLO v8' 선택 → 다운로드")
    print("3. 압축 해제 후 data/raw/pcb_defects/ 에 복사")
    exit(1)
EOF

if [ $? -ne 0 ]; then
    echo ""
    echo "자동 다운로드 실패. 수동 다운로드를 진행하세요."
    exit 1
fi

# 다운로드된 데이터 확인
echo ""
echo "[4/5] 다운로드된 데이터 확인..."
if [ -d "data/raw/pcb_defects" ]; then
    echo "✓ data/raw/pcb_defects 폴더 존재"

    TRAIN_IMAGES=$(find data/raw/pcb_defects/train/images -type f 2>/dev/null | wc -l)
    VAL_IMAGES=$(find data/raw/pcb_defects/valid/images -type f 2>/dev/null | wc -l)
    TEST_IMAGES=$(find data/raw/pcb_defects/test/images -type f 2>/dev/null | wc -l)

    echo "  - Train 이미지: $TRAIN_IMAGES 개"
    echo "  - Valid 이미지: $VAL_IMAGES 개"
    echo "  - Test 이미지: $TEST_IMAGES 개"
else
    echo "✗ data/raw/pcb_defects 폴더 없음"
    exit 1
fi

# data.yaml 파일 복사 및 경로 수정
echo ""
echo "[5/5] YOLO 설정 파일 생성..."
if [ -f "data/raw/pcb_defects/data.yaml" ]; then
    cp data/raw/pcb_defects/data.yaml data/pcb_defects_roboflow.yaml

    # 경로를 절대 경로로 수정
    sed -i "s|train: .*|train: $PROJECT_ROOT/data/raw/pcb_defects/train/images|g" data/pcb_defects_roboflow.yaml
    sed -i "s|val: .*|val: $PROJECT_ROOT/data/raw/pcb_defects/valid/images|g" data/pcb_defects_roboflow.yaml
    sed -i "s|test: .*|test: $PROJECT_ROOT/data/raw/pcb_defects/test/images|g" data/pcb_defects_roboflow.yaml

    echo "✓ YOLO 설정 파일 생성: data/pcb_defects_roboflow.yaml"
else
    echo "✗ data.yaml 파일 없음"
fi

echo ""
echo "======================================"
echo "다운로드 완료!"
echo "======================================"
echo ""
echo "다음 단계:"
echo "  bash scripts/train_yolo.sh"
echo ""
