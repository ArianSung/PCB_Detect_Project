#!/bin/bash
################################################################################
# FPIC-Component Model 최적화 학습 스크립트
#
# 목적: 부품 검출 모델 최적화 학습 (25개 클래스)
# 데이터: data/processed/fpic_component_yolo/ (6,260 이미지)
# 모델: YOLOv11m Medium (43M params)
# 예상 시간: 2-3시간 (RTX 4080 Super)
# 예상 성능: mAP@0.5 = 0.85-0.88 (데이터셋이 커서 높은 성능 기대)
################################################################################
#
# 최적화 기법 (Solder Model과 동일):
# 1. YOLOv11ml 모델 (43M params)
# 2. 이미지 해상도 800
# 3. 데이터 증강 강화 (mixup, copy_paste, hsv, flipud)
# 4. Cosine Annealing 학습률 스케줄링
# 5. 클래스 가중치 조정 (cls=1.0, box=5.0)
# 6. Patience=100 (더 긴 학습)
#
################################################################################

set -e  # 에러 발생 시 즉시 종료

echo "========================================="
echo "Component Model 최적화 학습 시작"
echo "========================================="
echo "Date: $(date)"
echo "GPU: $(nvidia-smi --query-gpu=name --format=csv,noheader | head -1)"
echo ""

# 경로 설정
PROJECT_ROOT="/home/sys1041/work_project"
DATA_YAML="$PROJECT_ROOT/data/processed/fpic_component_yolo/data.yaml"

# 가상환경 활성화 확인
if [ -z "$CONDA_DEFAULT_ENV" ]; then
    echo "⚠️  Conda 환경 활성화 필요!"
    echo "실행: conda activate pcb_defect"
    exit 1
fi

echo "✅ Conda 환경: $CONDA_DEFAULT_ENV"
echo ""

# 학습 파라미터 출력
echo "========================================="
echo "최적화 학습 파라미터"
echo "========================================="
echo "데이터:"
echo "  - Dataset: $DATA_YAML"
echo "  - Images: 6,260 (train: ~5,000, val: ~1,000)"
echo "  - Classes: 25"
echo ""
echo "모델 설정:"
echo "  - Model: yolo11m.pt (43M params) ⬆️"
echo "  - Image Size: 800 ⬆️"
echo "  - Batch: 16 (Effective: 64 via Gradient Accumulation)"
echo "  - Epochs: 200"
echo "  - Device: GPU 0"
echo ""
echo "최적화 기법:"
echo "  - 📊 데이터 증강 강화:"
echo "      - mixup=0.15 (객체 혼합)"
echo "      - copy_paste=0.3 (객체 복사-붙이기)"
echo "      - hsv_h=0.03, hsv_s=0.9, hsv_v=0.6 (색상 증강)"
echo "      - scale=0.9 (크기 변화)"
echo "      - translate=0.2 (위치 이동)"
echo "      - flipud=0.5 (상하 반전)"
echo ""
echo "  - 🎯 학습률 스케줄링:"
echo "      - Cosine Annealing (cos_lr=True)"
echo "      - lr0=0.002 (초기 학습률)"
echo "      - warmup_epochs=5"
echo ""
echo "  - ⚖️  손실 가중치 조정:"
echo "      - cls=1.0 (분류 손실 증가) ⬆️"
echo "      - box=5.0 (박스 손실 감소) ⬇️"
echo "      - dfl=1.5 (분포 초점 손실)"
echo ""
echo "  - ⏱️  Early Stopping:"
echo "      - patience=100 (더 긴 학습)"
echo ""
echo "예상 성능: mAP@0.5 = 0.85-0.88"
echo "예상 VRAM: 8-10GB"
echo "예상 시간: 2-3시간"
echo "========================================="
echo ""

# YOLOv11m 최적화 학습 실행
/home/sys1041/miniconda3/envs/pcb_defect/bin/yolo detect train \
    data="$DATA_YAML" \
    model=yolo11m.pt \
    epochs=200 \
    batch=16 \
    imgsz=800 \
    device=0 \
    project=runs/detect \
    name=component_model_optimized \
    exist_ok=True \
    pretrained=True \
    optimizer=AdamW \
    \
    cos_lr=True \
    lr0=0.002 \
    lrf=0.01 \
    momentum=0.937 \
    weight_decay=0.0005 \
    warmup_epochs=5 \
    warmup_momentum=0.8 \
    warmup_bias_lr=0.1 \
    \
    box=5.0 \
    cls=1.0 \
    dfl=1.5 \
    \
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
    \
    patience=100 \
    save=True \
    save_period=-1 \
    cache=False \
    workers=8 \
    amp=True \
    verbose=True

echo ""
echo "========================================="
echo "✅ Component Model 최적화 학습 완료!"
echo "========================================="
echo "Date: $(date)"
echo ""
echo "결과 위치:"
echo "  - Best Model: runs/detect/component_model_optimized/weights/best.pt"
echo "  - Last Model: runs/detect/component_model_optimized/weights/last.pt"
echo "  - Metrics: runs/detect/component_model_optimized/results.csv"
echo "  - Plots: runs/detect/component_model_optimized/*.png"
echo ""
echo "성능 비교:"
echo "  - Solder Model (YOLOv11ml, imgsz=800): mAP@0.5 = 0.86-0.89"
echo "  - Component Model (YOLOv11ml, imgsz=800): mAP@0.5 = ?"
echo ""
