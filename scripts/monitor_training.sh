#!/bin/bash
################################################################################
# YOLO 학습 모니터링 스크립트
#
# 사용법: bash scripts/monitor_training.sh [model_name]
# 예시: bash scripts/monitor_training.sh solder_model_optimized
################################################################################

MODEL_NAME=${1:-"solder_model_optimized"}
PROJECT_ROOT="/home/sys1041/work_project"
RESULTS_DIR="$PROJECT_ROOT/runs/detect/$MODEL_NAME"
LOG_FILE="$PROJECT_ROOT/logs/${MODEL_NAME}_training.log"

clear

echo "========================================="
echo "📊 YOLO 학습 모니터링"
echo "========================================="
echo "모델: $MODEL_NAME"
echo "시작 시간: $(date)"
echo ""

# GPU 상태 확인
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "🖥️  GPU 상태"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
nvidia-smi --query-gpu=index,name,utilization.gpu,memory.used,memory.total,temperature.gpu \
    --format=csv,noheader,nounits | \
    awk -F', ' '{printf "GPU %s: %s\n  사용률: %s%%\n  VRAM: %sMB / %sMB (%.1f%%)\n  온도: %s°C\n\n",
                 $1, $2, $3, $4, $5, ($4/$5)*100, $6}'
echo ""

# 학습 진행 상황 확인
if [ -f "$RESULTS_DIR/results.csv" ]; then
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    echo "📈 학습 진행 상황"
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

    # 현재 에포크
    CURRENT_EPOCH=$(tail -n 1 "$RESULTS_DIR/results.csv" | cut -d',' -f1)
    echo "현재 에포크: $CURRENT_EPOCH / 200"
    echo ""

    # 최근 10개 에포크 성능
    echo "최근 10개 에포크 성능:"
    echo "┌─────────┬──────────┬──────────┬──────────┬──────────┐"
    echo "│  Epoch  │  mAP@0.5 │ mAP@0.95 │ Precision│  Recall  │"
    echo "├─────────┼──────────┼──────────┼──────────┼──────────┤"
    tail -n 10 "$RESULTS_DIR/results.csv" | \
        awk -F',' 'NR>1 {printf "│ %7s │  %.4f  │  %.4f  │  %.4f  │  %.4f  │\n",
                   int($1), $8, $9, $6, $7}'
    echo "└─────────┴──────────┴──────────┴──────────┴──────────┘"
    echo ""

    # Best 성능
    echo "🏆 Best 성능:"
    python3 << 'EOF'
import csv
with open('$RESULTS_DIR/results.csv', 'r') as f:
    reader = csv.DictReader(f)
    rows = [row for row in reader if row['epoch'].strip()]
    if rows:
        best_row = max(rows, key=lambda x: float(x['metrics/mAP50(B)']))
        print(f"  Epoch {int(float(best_row['epoch']))}")
        print(f"  - mAP@0.5     : {float(best_row['metrics/mAP50(B)']):.4f}")
        print(f"  - mAP@0.5-0.95: {float(best_row['metrics/mAP50-95(B)']):.4f}")
        print(f"  - Precision   : {float(best_row['metrics/precision(B)']):.4f}")
        print(f"  - Recall      : {float(best_row['metrics/recall(B)']):.4f}")
EOF
    echo ""

    # 예상 완료 시간
    if [ -n "$CURRENT_EPOCH" ] && [ "$CURRENT_EPOCH" -gt 0 ]; then
        ELAPSED_TIME=$(ps -o etime= -p $(pgrep -f "train.*$MODEL_NAME" | head -1) 2>/dev/null | tr -d ' ')
        if [ -n "$ELAPSED_TIME" ]; then
            echo "⏱️  예상 완료 시간 계산 중..."
            # 간단한 시간 추정 (평균 에포크 시간 기반)
        fi
    fi

else
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    echo "⏳ 학습 준비 중..."
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    echo "results.csv 파일이 아직 생성되지 않았습니다."
    echo "첫 번째 에포크가 완료될 때까지 기다려주세요."
    echo ""
fi

# 프로세스 상태
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "🔧 프로세스 상태"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
if pgrep -f "train.*$MODEL_NAME" > /dev/null; then
    echo "✅ 학습 프로세스 실행 중"
    ps aux | grep -E "train.*$MODEL_NAME" | grep -v grep | \
        awk '{printf "  PID: %s\n  CPU: %s%%\n  메모리: %s%%\n  시작 시간: %s\n", $2, $3, $4, $9}'
else
    echo "⚠️  학습 프로세스가 실행되지 않고 있습니다."
fi
echo ""

# 로그 파일 마지막 10줄
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "📝 최근 로그 (마지막 5줄)"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
if [ -f "$LOG_FILE" ]; then
    tail -n 5 "$LOG_FILE" | sed 's/\x1b\[[0-9;]*m//g'  # ANSI 색상 코드 제거
else
    echo "로그 파일이 없습니다: $LOG_FILE"
fi
echo ""

echo "========================================="
echo "💡 유용한 명령어"
echo "========================================="
echo "실시간 로그 확인:"
echo "  tail -f $LOG_FILE"
echo ""
echo "GPU 모니터링 (1초마다):"
echo "  watch -n 1 nvidia-smi"
echo ""
echo "학습 결과 그래프 확인:"
echo "  ls -lh $RESULTS_DIR/*.png"
echo ""
echo "현재 스크립트 재실행:"
echo "  bash scripts/monitor_training.sh $MODEL_NAME"
echo ""
