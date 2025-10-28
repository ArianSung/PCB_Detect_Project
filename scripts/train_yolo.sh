#!/bin/bash
# YOLO v8 Large 모델 학습 스크립트
#
# 사용법:
#   bash scripts/train_yolo.sh
#
# 필수 조건:
#   - conda activate pcb_defect
#   - 데이터셋 다운로드 완료 (bash scripts/download_pcb_dataset.sh)

set -e  # 에러 발생 시 중단

echo "======================================"
echo "YOLO v8 Large 모델 학습 시작"
echo "======================================"

# 프로젝트 루트로 이동
cd "$(dirname "$0")/.."
PROJECT_ROOT=$(pwd)

echo "프로젝트 루트: $PROJECT_ROOT"

# 환경 확인
echo ""
echo "[1/4] 환경 확인..."

# Conda 환경 확인
if [ -z "$CONDA_DEFAULT_ENV" ]; then
    echo "✗ Conda 환경이 활성화되지 않았습니다."
    echo "실행: conda activate pcb_defect"
    exit 1
else
    echo "✓ Conda 환경: $CONDA_DEFAULT_ENV"
fi

# CUDA 확인
if command -v nvidia-smi &> /dev/null; then
    echo "✓ CUDA 사용 가능"
    nvidia-smi --query-gpu=name,memory.total,memory.free --format=csv,noheader
else
    echo "✗ CUDA 없음 (CPU 모드로 학습)"
fi

# 데이터셋 확인
echo ""
echo "[2/4] 데이터셋 확인..."

DATA_YAML=""
if [ -f "data/pcb_defects_roboflow.yaml" ]; then
    DATA_YAML="data/pcb_defects_roboflow.yaml"
    echo "✓ 데이터셋 설정 파일: $DATA_YAML"
else
    echo "✗ 데이터셋 설정 파일 없음"
    echo "먼저 데이터셋을 다운로드하세요:"
    echo "  bash scripts/download_pcb_dataset.sh"
    exit 1
fi

# 학습 파라미터 설정
echo ""
echo "[3/4] 학습 파라미터 설정..."

MODEL="yolov8l.pt"       # YOLOv8 Large 모델
EPOCHS=200               # 에포크 수 (전략 3: 150 → 200)
BATCH=24                 # 배치 사이즈 (전략 3: 32 → 24, VRAM 안정화)
IMGSZ=640                # 이미지 크기
DEVICE=0                 # GPU 디바이스 ID (0 = 첫 번째 GPU)

echo "  모델: $MODEL"
echo "  에포크: $EPOCHS"
echo "  배치 사이즈: $BATCH"
echo "  이미지 크기: $IMGSZ"
echo "  디바이스: GPU $DEVICE"

# 사용자 확인
echo ""
read -p "학습을 시작하시겠습니까? (y/n): " CONFIRM

if [ "$CONFIRM" != "y" ] && [ "$CONFIRM" != "Y" ]; then
    echo "학습 취소"
    exit 0
fi

# 학습 시작
echo ""
echo "[4/4] 학습 시작..."
echo "======================================"
echo ""
echo "주의사항:"
echo "  - 학습은 약 1-2시간 소요됩니다"
echo "  - 학습 중 GPU 사용률을 모니터링하려면:"
echo "    watch -n 1 nvidia-smi"
echo "  - 학습 중단: Ctrl+C"
echo ""
echo "======================================"
echo ""

# YOLO 학습 실행
python yolo/train_yolo.py \
    --data "$DATA_YAML" \
    --model "$MODEL" \
    --epochs "$EPOCHS" \
    --batch "$BATCH" \
    --imgsz "$IMGSZ" \
    --device "$DEVICE"

# 학습 완료 후 결과 확인
echo ""
echo "======================================"
echo "학습 완료!"
echo "======================================"
echo ""

# 최신 학습 결과 폴더 찾기
LATEST_RUN=$(ls -td yolo/runs/train/pcb_defect* 2>/dev/null | head -1)

if [ -n "$LATEST_RUN" ]; then
    echo "결과 폴더: $LATEST_RUN"
    echo ""
    echo "생성된 파일:"
    ls -lh "$LATEST_RUN/weights/"
    echo ""
    echo "다음 단계:"
    echo "  1. 학습 그래프 확인: $LATEST_RUN/results.png"
    echo "  2. 모델 평가:"
    echo "     python yolo/evaluate_yolo.py --model $LATEST_RUN/weights/best.pt --data $DATA_YAML"
    echo ""
else
    echo "✗ 학습 결과를 찾을 수 없습니다"
fi
