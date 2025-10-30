#!/bin/bash
# Resume Training Script - 현재 모델에서 계속 학습
# 목표: mAP50 95%+ 달성

cd /home/sys1041/work_project

# 로그 디렉토리 생성
mkdir -p logs

# best.pt에서 재개하여 추가 학습
# - epochs=500: 총 500 에포크까지 학습 (현재 250에서 시작하므로 250 에포크 추가)
# - lr0=0.0003: 학습률 낮춤 (Plateau 탈출용)
# - warmup_epochs=10: 학습률 워밍업
# - patience=100: Early stopping patience 증가
# - batch=16: 기존과 동일
# - 데이터 증강 강화

/home/sys1041/miniconda3/envs/pcb_defect/bin/yolo detect train \
  data=/home/sys1041/work_project/data/processed/complete_pcb_model/data.yaml \
  model=/home/sys1041/work_project/runs/detect/complete_pcb_base_model3/weights/best.pt \
  epochs=500 \
  batch=16 \
  imgsz=640 \
  device=0 \
  patience=100 \
  lr0=0.0003 \
  lrf=0.01 \
  warmup_epochs=10 \
  warmup_momentum=0.8 \
  warmup_bias_lr=0.1 \
  dropout=0.2 \
  weight_decay=0.001 \
  mixup=0.2 \
  copy_paste=0.15 \
  mosaic=1.0 \
  close_mosaic=20 \
  hsv_h=0.015 \
  hsv_s=0.7 \
  hsv_v=0.4 \
  degrees=10.0 \
  translate=0.2 \
  scale=0.9 \
  shear=10.0 \
  perspective=0.001 \
  flipud=0.5 \
  fliplr=0.5 \
  project=runs/detect \
  name=complete_pcb_resumed 2>&1 | tee logs/training_resumed.log
