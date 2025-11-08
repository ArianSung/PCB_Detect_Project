#!/bin/bash
"""
균형 데이터셋으로 YOLOv11l 학습 (로그 저장 포함)
실시간 로그 출력 + 파일 저장
"""

# 프로젝트 루트
PROJECT_ROOT="/home/sys1041/work_project"
DATA_YAML="${PROJECT_ROOT}/data/processed/roboflow_pcb_balanced/data.yaml"

# 로그 디렉토리 생성
LOG_DIR="${PROJECT_ROOT}/logs"
mkdir -p "${LOG_DIR}"

# 타임스탬프
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
LOG_FILE="${LOG_DIR}/training_balanced_${TIMESTAMP}.log"

echo "================================================================================"
echo "균형 데이터셋으로 YOLOv11l 학습 시작"
echo "================================================================================"
echo "데이터셋: ${DATA_YAML}"
echo "로그 파일: ${LOG_FILE}"
echo "================================================================================"
echo ""

# Conda 환경 활성화
source ~/miniconda3/etc/profile.d/conda.sh
conda activate pcb_defect

# YOLO 학습 (로그 저장 + 실시간 출력)
yolo detect train \
  data="${DATA_YAML}" \
  model=yolo11l.pt \
  epochs=100 \
  batch=16 \
  imgsz=640 \
  device=0 \
  project=runs/detect \
  name=roboflow_pcb_balanced \
  exist_ok=True \
  pretrained=True \
  optimizer=AdamW \
  lr0=0.001 \
  lrf=0.01 \
  weight_decay=0.0005 \
  patience=50 \
  amp=True \
  verbose=True \
  plots=True \
  cache=False \
  workers=8 \
  hsv_h=0.015 \
  hsv_s=0.7 \
  hsv_v=0.4 \
  degrees=15 \
  translate=0.1 \
  scale=0.5 \
  shear=0.0 \
  perspective=0.0 \
  flipud=0.0 \
  fliplr=0.5 \
  mosaic=1.0 \
  mixup=0.15 \
  copy_paste=0.1 \
  2>&1 | tee "${LOG_FILE}"

# 학습 결과 요약
echo ""
echo "================================================================================"
echo "학습 완료!"
echo "================================================================================"
echo "로그 파일: ${LOG_FILE}"
echo "결과 저장 위치: runs/detect/roboflow_pcb_balanced"
echo ""
echo "로그 확인 방법:"
echo "  cat ${LOG_FILE}"
echo "  tail -f ${LOG_FILE}  # 실시간 모니터링"
echo "================================================================================"
