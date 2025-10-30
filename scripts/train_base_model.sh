#!/bin/bash
# Base Model 학습 스크립트
# 32개 클래스 (DeepPCB + SolDef_AI + PCB Components)

cd /home/sys1041/work_project

# 로그 디렉토리 생성
mkdir -p logs

# 학습 시작
/home/sys1041/miniconda3/envs/pcb_defect/bin/yolo detect train \
  data=/home/sys1041/work_project/data/processed/complete_pcb_model/data.yaml \
  model=yolov8l.pt \
  epochs=250 \
  batch=16 \
  imgsz=640 \
  device=0 \
  patience=50 \
  dropout=0.15 \
  weight_decay=0.0005 \
  mixup=0.15 \
  copy_paste=0.1 \
  mosaic=1.0 \
  close_mosaic=15 \
  project=runs/detect \
  name=complete_pcb_base_model 2>&1 | tee logs/training_base_model.log
