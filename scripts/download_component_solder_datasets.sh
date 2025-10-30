#!/bin/bash

###############################################################################
# 부품 실장 & 납땜 불량 데이터셋 다운로드 스크립트
###############################################################################

set -e

PROJECT_ROOT="/home/sys1041/work_project"
RAW_DATA_DIR="$PROJECT_ROOT/data/raw"

echo "=== PCB 부품 & 납땜 데이터셋 다운로드 시작 ==="

# 1. SolDef_AI 데이터셋 다운로드 (Kaggle)
echo ""
echo "📦 1. SolDef_AI 데이터셋 다운로드 (납땜 불량)"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

# Kaggle 설치 확인
if ! command -v kaggle &> /dev/null; then
    echo "Kaggle CLI 설치 중..."
    pip install kaggle
fi

# Kaggle 인증 확인
if [ ! -f ~/.kaggle/kaggle.json ]; then
    echo ""
    echo "⚠️  Kaggle API 토큰이 필요합니다!"
    echo ""
    echo "다음 단계를 따라주세요:"
    echo "1. https://www.kaggle.com/ 로그인"
    echo "2. Account → Settings → API → Create New API Token"
    echo "3. 다운로드된 kaggle.json 파일을 ~/.kaggle/ 에 복사"
    echo "   mkdir -p ~/.kaggle"
    echo "   cp /path/to/kaggle.json ~/.kaggle/"
    echo "   chmod 600 ~/.kaggle/kaggle.json"
    echo ""
    read -p "설정 완료 후 Enter를 누르세요..."
fi

# SolDef_AI 다운로드
mkdir -p "$RAW_DATA_DIR/soldef_ai"
cd "$RAW_DATA_DIR/soldef_ai"

echo "다운로드 중... (약 1.2GB)"
kaggle datasets download -d mauriziocalabrese/soldef-ai-pcb-dataset-for-defect-detection

echo "압축 해제 중..."
unzip -q soldef-ai-pcb-dataset-for-defect-detection.zip
rm soldef-ai-pcb-dataset-for-defect-detection.zip

echo "✅ SolDef_AI 데이터셋 다운로드 완료!"

# 2. PCBA-Dataset 다운로드 (GitHub)
echo ""
echo "📦 2. PCBA-Dataset 다운로드 (부품 실장 불량)"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

mkdir -p "$RAW_DATA_DIR/pcba_dataset"
cd "$RAW_DATA_DIR/pcba_dataset"

echo "GitHub에서 클론 중..."
git clone https://github.com/ismh16/PCBA-Dataset.git

echo "✅ PCBA-Dataset 다운로드 완료!"

# 3. 데이터셋 구조 확인
echo ""
echo "📊 데이터셋 구조 확인"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

echo ""
echo "SolDef_AI 구조:"
ls -lh "$RAW_DATA_DIR/soldef_ai/" | head -10

echo ""
echo "PCBA-Dataset 구조:"
ls -lh "$RAW_DATA_DIR/pcba_dataset/PCBA-Dataset/" | head -10

# 4. 완료 메시지
echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "✅ 모든 데이터셋 다운로드 완료!"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""
echo "다음 단계:"
echo "1. 데이터셋 전처리 (YOLO 형식 변환)"
echo "2. 기존 DeepPCB 데이터셋과 통합"
echo "3. 통합 모델 학습"
echo ""
echo "실행 명령어:"
echo "  python yolo/merge_all_datasets.py"
echo ""
