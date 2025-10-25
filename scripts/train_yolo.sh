#!/bin/bash
# YOLO 모델 학습 스크립트

# 가상환경 활성화
source ~/miniconda3/etc/profile.d/conda.sh
conda activate pcb_defect

# 프로젝트 루트로 이동
cd "$(dirname "$0")/.."

echo "========================================="
echo "YOLO 모델 학습 시작"
echo "========================================="

# 설정 파일 경로
CONFIG_FILE="configs/yolo_config.yaml"

# 설정 파일 존재 확인
if [ ! -f "$CONFIG_FILE" ]; then
    echo "❌ 오류: 설정 파일을 찾을 수 없습니다: $CONFIG_FILE"
    exit 1
fi

# 데이터셋 확인
DATA_FILE="data/pcb_defects.yaml"
if [ ! -f "$DATA_FILE" ]; then
    echo "❌ 오류: 데이터셋 설정 파일을 찾을 수 없습니다: $DATA_FILE"
    echo "💡 데이터셋을 먼저 준비해주세요."
    exit 1
fi

# 학습 실행
echo "📚 학습 시작..."
echo "설정 파일: $CONFIG_FILE"
echo "데이터셋: $DATA_FILE"
echo ""

python src/training/train_yolo.py \
    --config "$CONFIG_FILE" \
    "$@"

echo ""
echo "========================================="
echo "✅ 학습 완료!"
echo "========================================="
echo "결과 저장 위치: models/yolo/experiments/"
