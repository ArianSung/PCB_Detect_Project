#!/bin/bash
# Flask 추론 서버 시작 스크립트

# 가상환경 활성화
source ~/miniconda3/etc/profile.d/conda.sh
conda activate pcb_defect

# 프로젝트 루트로 이동
cd "$(dirname "$0")/.."

echo "========================================="
echo "Flask 추론 서버 시작"
echo "========================================="

# 설정 파일 확인
CONFIG_FILE="configs/server_config.yaml"
if [ ! -f "$CONFIG_FILE" ]; then
    echo "❌ 오류: 서버 설정 파일을 찾을 수 없습니다: $CONFIG_FILE"
    exit 1
fi

# 모델 파일 확인
MODEL_PATH="models/yolo/final/yolo_best.pt"
if [ ! -f "$MODEL_PATH" ]; then
    echo "⚠️  경고: 모델 파일을 찾을 수 없습니다: $MODEL_PATH"
    echo "💡 학습된 모델을 먼저 준비하거나, 사전 학습 모델을 다운로드해주세요."
    echo ""
    read -p "계속 진행하시겠습니까? (y/N): " response
    if [[ ! "$response" =~ ^[Yy]$ ]]; then
        exit 1
    fi
fi

echo "📡 서버 설정: $CONFIG_FILE"
echo "🤖 모델 경로: $MODEL_PATH"
echo ""

# Flask 서버 실행
python server/app.py \
    --config "$CONFIG_FILE" \
    "$@"
