#!/bin/bash
# Flask 추론 서버 시작 스크립트

# 가상환경 활성화
source ~/miniconda3/etc/profile.d/conda.sh
conda activate pcb_defect

# 프로젝트 루트로 이동
cd "$(dirname "$0")/.."

echo "================================================"
echo "PCB 불량 검사 Flask 추론 서버 시작"
echo "================================================"

# 가상환경 확인
if [[ "$CONDA_DEFAULT_ENV" != "pcb_defect" ]]; then
    echo "⚠️  가상환경 활성화 실패"
    echo "💡 수동으로 활성화하세요: conda activate pcb_defect"
    exit 1
fi

echo "✅ 가상환경: $CONDA_DEFAULT_ENV"

# MySQL 연결 정보 표시
echo ""
echo "📊 MySQL 데이터베이스 설정:"
DB_HOST=${DB_HOST:-localhost}
DB_PORT=${DB_PORT:-3306}
DB_USER=${DB_USER:-root}
DB_NAME=${DB_NAME:-pcb_inspection}

echo "   호스트: $DB_HOST:$DB_PORT"
echo "   사용자: $DB_USER"
echo "   DB 이름: $DB_NAME"

if [ -z "$DB_PASSWORD" ]; then
    echo "   ⚠️  DB_PASSWORD 환경 변수가 설정되지 않았습니다."
    echo "   💡 export DB_PASSWORD=your_password"
fi

# 모델 파일 확인 (선택 사항)
MODEL_PATH="models/yolo/final/yolo_best.pt"
if [ -f "$MODEL_PATH" ]; then
    echo ""
    echo "🤖 AI 모델: $MODEL_PATH ✅"
else
    echo ""
    echo "⚠️  AI 모델이 없습니다 (개발 모드로 실행)"
    echo "💡 YOLO 학습 완료 후 모델을 배치하세요: $MODEL_PATH"
fi

# Flask 서버 시작
echo ""
echo "================================================"
echo "Flask 서버 시작 중..."
echo "================================================"
echo "호스트: 0.0.0.0 (모든 인터페이스)"
echo "포트: 5000"
echo "종료: Ctrl+C"
echo "================================================"
echo ""

# 서버 실행
python server/app.py
