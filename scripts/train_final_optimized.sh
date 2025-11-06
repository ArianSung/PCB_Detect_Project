#!/bin/bash
# 최종 최적화 학습 스크립트
# 목표: mAP50 95%+
#
# 최적화 설정:
# - Batch: 16 (VRAM 안정성)
# - Epochs: 300 (충분한 학습)
# - Patience: 80 (조기 종료 여유)
# - Dropout: 0.2 (과적합 방지)
# - Weight Decay: 0.001 (정규화)
# - Data Augmentation: 적절한 수준

cd /home/sys1041/work_project

# 로그 디렉토리 생성
mkdir -p logs

echo "=== 최종 최적화 학습 시작 ==="
echo "데이터셋: 9,291개 이미지, 136,746개 객체, 29개 클래스"
echo "모델: YOLOv8 Large (43.6M parameters)"
echo "목표: mAP50 95%+"
echo ""

/home/sys1041/miniconda3/envs/pcb_defect/bin/yolo detect train \
  data=/home/sys1041/work_project/data/processed/complete_pcb_model/data.yaml \
  model=yolov8l.pt \
  epochs=300 \
  batch=16 \
  imgsz=640 \
  device=0 \
  patience=80 \
  lr0=0.001 \
  lrf=0.01 \
  momentum=0.937 \
  weight_decay=0.001 \
  warmup_epochs=5 \
  warmup_momentum=0.8 \
  warmup_bias_lr=0.1 \
  dropout=0.2 \
  box=7.5 \
  cls=0.5 \
  dfl=1.5 \
  hsv_h=0.015 \
  hsv_s=0.7 \
  hsv_v=0.4 \
  degrees=10.0 \
  translate=0.1 \
  scale=0.5 \
  shear=5.0 \
  perspective=0.0001 \
  flipud=0.5 \
  fliplr=0.5 \
  mosaic=1.0 \
  mixup=0.1 \
  copy_paste=0.1 \
  auto_augment=randaugment \
  erasing=0.2 \
  close_mosaic=30 \
  amp=True \
  fraction=1.0 \
  project=runs/detect \
  name=complete_pcb_final_29classes_optimized \
  exist_ok=False \
  pretrained=True \
  optimizer=auto \
  verbose=True \
  seed=0 \
  deterministic=True \
  single_cls=False \
  rect=False \
  cos_lr=False \
  resume=False \
  nbs=64 \
  save=True \
  save_period=-1 \
  cache=False \
  plots=True \
  val=True 2>&1 | tee logs/training_final_optimized.log

echo ""
echo "=== 학습 완료 ==="
