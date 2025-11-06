#!/bin/bash
################################################################################
# 통합 Component Model 학습 스크립트 (SMD + Through-hole)
#
# 목적: SMD 부품 + Through-hole 부품을 모두 검출
# 데이터: FPIC-Component (25 SMD) + ElectroCom61 (10 Through-hole)
# 모델: YOLOv8 Large (43M params)
# 예상 시간: 3-4시간 (RTX 4080 Super)
# 예상 성능: mAP@0.5 = 0.87-0.90
################################################################################

set -e

echo "================================================================================"
echo "통합 Component Model 학습 시작 (SMD + Through-hole)"
echo "================================================================================"

# 경로 설정
DATA_YAML="/home/sys1041/work_project/data/processed/unified_component_yolo/data.yaml"
PROJECT_DIR="runs/detect"
NAME="unified_component_model"

# VRAM 확인
echo ""
echo "📊 GPU 상태 확인:"
nvidia-smi --query-gpu=name,memory.total,memory.free --format=csv,noheader

echo ""
echo "📋 학습 설정:"
echo "  - 데이터셋: FPIC (25 SMD) + ElectroCom61 (10 TH) = 35 클래스"
echo "  - 모델: YOLOv8 Large (43M params)"
echo "  - 이미지 크기: 800px"
echo "  - 배치 크기: 16"
echo "  - 에포크: 200"
echo "  - 예상 VRAM: 10-12GB"
echo "  - 예상 시간: 3-4시간"
echo ""
echo "🚀 학습 시작..."
echo ""

# YOLOv8 최적화 학습 실행
/home/sys1041/miniconda3/envs/pcb_defect/bin/yolo detect train \
    data="$DATA_YAML" \
    model=yolov8l.pt \
    epochs=200 \
    batch=16 \
    imgsz=800 \
    device=0 \
    project="$PROJECT_DIR" \
    name="$NAME" \
    exist_ok=True \
    pretrained=True \
    optimizer=AdamW \
    cos_lr=True \
    lr0=0.002 \
    lrf=0.01 \
    momentum=0.937 \
    weight_decay=0.0005 \
    warmup_epochs=5 \
    warmup_momentum=0.8 \
    warmup_bias_lr=0.1 \
    box=5.0 \
    cls=1.0 \
    dfl=1.5 \
    mosaic=1.0 \
    mixup=0.15 \
    copy_paste=0.3 \
    degrees=0.0 \
    translate=0.2 \
    scale=0.9 \
    shear=0.0 \
    perspective=0.0 \
    flipud=0.5 \
    fliplr=0.5 \
    hsv_h=0.03 \
    hsv_s=0.9 \
    hsv_v=0.6 \
    patience=100 \
    save=True \
    save_period=-1 \
    cache=False \
    workers=8 \
    amp=True \
    verbose=True

echo ""
echo "================================================================================"
echo "✅ 학습 완료!"
echo "================================================================================"
echo ""
echo "📊 결과 확인:"
echo "  - 최고 모델: $PROJECT_DIR/$NAME/weights/best.pt"
echo "  - 최종 모델: $PROJECT_DIR/$NAME/weights/last.pt"
echo "  - 학습 로그: $PROJECT_DIR/$NAME/results.csv"
echo "  - 시각화: $PROJECT_DIR/$NAME/*.png"
echo ""
echo "🧪 테스트 명령:"
echo "  yolo detect predict model=$PROJECT_DIR/$NAME/weights/best.pt source=YOUR_IMAGE.jpg"
echo ""
echo "================================================================================"
