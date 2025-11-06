#!/bin/bash
# Aggressive Training Script - mAP50 95%+ 달성을 위한 적극적 학습
# 변경사항:
# 1. 학습률 3배 증가 (0.0003 → 0.001) - 빠른 학습
# 2. Batch size = 16 (VRAM 약 55-65% 사용), nbs=64 (gradient accumulation)
# 3. Dropout 완전히 제거 (0.0) - 과적합 방지 OFF
# 4. 데이터 증강 완전히 제거 - 순수 학습
# 5. Weight decay 제거 (0.0) - 가중치 업데이트 완전 자유

cd /home/sys1041/work_project

# 로그 디렉토리 생성
mkdir -p logs

# best.pt에서 재개하여 공격적 학습
/home/sys1041/miniconda3/envs/pcb_defect/bin/yolo detect train \
  data=/home/sys1041/work_project/data/processed/complete_pcb_model/data.yaml \
  model=/home/sys1041/work_project/runs/detect/complete_pcb_base_model3/weights/best.pt \
  epochs=500 \
  batch=16 \
  nbs=64 \
  imgsz=640 \
  device=0 \
  patience=100 \
  lr0=0.001 \
  lrf=0.01 \
  warmup_epochs=5 \
  warmup_momentum=0.8 \
  warmup_bias_lr=0.1 \
  dropout=0.0 \
  weight_decay=0.0 \
  mixup=0.0 \
  copy_paste=0.0 \
  mosaic=1.0 \
  close_mosaic=5 \
  hsv_h=0.0 \
  hsv_s=0.0 \
  hsv_v=0.0 \
  degrees=0.0 \
  translate=0.0 \
  scale=0.0 \
  shear=0.0 \
  perspective=0.0 \
  flipud=0.5 \
  fliplr=0.5 \
  project=runs/detect \
  name=complete_pcb_resumed 2>&1 | tee logs/training_resumed.log
