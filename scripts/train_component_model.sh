#!/bin/bash
################################################################################
# FPIC-Component (PCBSegClassNet) Component Model 학습 스크립트
#
# 목적: PCB 부품 검출 모델 학습 (25개 클래스)
# 데이터: data/processed/fpic_component_yolo/ (6,260 이미지)
# 모델: YOLOv11m Medium
# 예상 시간: 10-15시간 (RTX 4080 Super)
################################################################################

set -e  # 에러 발생 시 즉시 종료

echo "========================================="
echo "Component Model 학습 시작"
echo "========================================="
echo "Date: $(date)"
echo "GPU: $(nvidia-smi --query-gpu=name --format=csv,noheader | head -1)"
echo ""

# 경로 설정
PROJECT_ROOT="/home/sys1041/work_project"
DATA_YAML="$PROJECT_ROOT/data/processed/fpic_component_yolo/data.yaml"
OUTPUT_DIR="runs/detect/component_model"

# 가상환경 활성화 확인
if [ -z "$CONDA_DEFAULT_ENV" ]; then
    echo "⚠️  Conda 환경 활성화 필요!"
    echo "실행: conda activate pcb_defect"
    exit 1
fi

echo "✅ Conda 환경: $CONDA_DEFAULT_ENV"
echo ""

# 학습 시작
echo "학습 파라미터:"
echo "  - Data: $DATA_YAML"
echo "  - Model: yolo11m.pt"
echo "  - Epochs: 200"
echo "  - Batch: 16 (Gradient Accumulation: 자동)"
echo "  - Image Size: 640"
echo "  - Device: GPU 0"
echo "  - Optimizer: AdamW"
echo "  - Learning Rate: 0.001"
echo "  - Patience: 50 (Early Stopping)"
echo ""

# YOLOv11m 학습 실행
/home/sys1041/miniconda3/envs/pcb_defect/bin/yolo detect train \
    data="$DATA_YAML" \
    model=yolo11m.pt \
    epochs=200 \
    batch=16 \
    imgsz=640 \
    device=0 \
    project=runs/detect \
    name=component_model \
    exist_ok=True \
    pretrained=True \
    optimizer=AdamW \
    lr0=0.001 \
    lrf=0.01 \
    momentum=0.937 \
    weight_decay=0.0005 \
    warmup_epochs=3 \
    warmup_momentum=0.8 \
    warmup_bias_lr=0.1 \
    box=7.5 \
    cls=0.5 \
    dfl=1.5 \
    patience=50 \
    save=True \
    save_period=-1 \
    cache=False \
    workers=8 \
    amp=True \
    verbose=True

echo ""
echo "========================================="
echo "✅ Component Model 학습 완료!"
echo "========================================="
echo "Date: $(date)"
echo ""
echo "결과 위치:"
echo "  - Best Model: runs/detect/component_model/weights/best.pt"
echo "  - Last Model: runs/detect/component_model/weights/last.pt"
echo "  - Metrics: runs/detect/component_model/results.csv"
echo "  - Plots: runs/detect/component_model/*.png"
echo ""
