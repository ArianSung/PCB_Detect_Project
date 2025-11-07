#!/bin/bash
# 18개 Through-hole 클래스 YOLOv11m Medium 학습 스크립트

echo "======================================================================================================"
echo "18개 Through-hole 클래스 YOLOv11m Medium 학습"
echo "======================================================================================================"

# 환경 변수 설정
export CUDA_VISIBLE_DEVICES=0

# 학습 파라미터
MODEL="yolo11m.pt"
DATA="/home/sys1041/work_project/data/processed/throughhole_18classes_augmented/data.yaml"
EPOCHS=100
BATCH=16
IMGSZ=640
DEVICE=0
PROJECT="runs/detect"
NAME="throughhole_18classes_yolo11m"

# 학습 시작 시간 기록
START_TIME=$(date +%s)

echo ""
echo "📋 학습 설정:"
echo "  - 모델: $MODEL"
echo "  - 데이터셋: $DATA"
echo "  - Epochs: $EPOCHS"
echo "  - Batch Size: $BATCH"
echo "  - Image Size: $IMGSZ"
echo "  - Device: cuda:$DEVICE"
echo "  - 결과 저장: $PROJECT/$NAME"
echo ""
echo "======================================================================================================"
echo ""

# YOLOv11m 학습 실행
/home/sys1041/miniconda3/envs/pcb_defect/bin/yolo detect train \
    model=$MODEL \
    data=$DATA \
    epochs=$EPOCHS \
    batch=$BATCH \
    imgsz=$IMGSZ \
    device=$DEVICE \
    project=$PROJECT \
    name=$NAME \
    patience=20 \
    save=True \
    plots=True \
    cache=True \
    workers=8 \
    optimizer=AdamW \
    lr0=0.001 \
    lrf=0.01 \
    momentum=0.937 \
    weight_decay=0.0005 \
    warmup_epochs=3 \
    warmup_momentum=0.8 \
    warmup_bias_lr=0.1 \
    amp=True \
    verbose=True

# 학습 종료 시간 기록
END_TIME=$(date +%s)
ELAPSED=$((END_TIME - START_TIME))
HOURS=$((ELAPSED / 3600))
MINUTES=$(((ELAPSED % 3600) / 60))

echo ""
echo "======================================================================================================"
echo "✅ 학습 완료!"
echo "======================================================================================================"
echo ""
echo "학습 시간: ${HOURS}시간 ${MINUTES}분"
echo "결과 위치: $PROJECT/$NAME"
echo ""
echo "주요 결과 파일:"
echo "  - 최고 성능 모델: $PROJECT/$NAME/weights/best.pt"
echo "  - 마지막 모델: $PROJECT/$NAME/weights/last.pt"
echo "  - 학습 곡선: $PROJECT/$NAME/results.png"
echo "  - Confusion Matrix: $PROJECT/$NAME/confusion_matrix.png"
echo ""
