#!/bin/bash
# 간단한 학습 실시간 모니터링 스크립트
# 사용법: bash scripts/monitor_training_simple.sh

echo "=========================================="
echo "  YOLO 학습 실시간 모니터링"
echo "=========================================="
echo ""
echo "💡 Tip: Ctrl+C를 눌러 종료하세요"
echo ""

# 갱신 주기 (초)
INTERVAL=3

# results.csv 경로
RESULTS_CSV="yolo/runs/train/pcb_defect/results.csv"

while true; do
    clear

    echo "=========================================="
    echo "  YOLO 학습 실시간 모니터링"
    echo "=========================================="
    echo "업데이트: $(date '+%Y-%m-%d %H:%M:%S')"
    echo ""

    # GPU 상태
    echo "🖥️  GPU 상태:"
    echo "----------------------------------------"
    if command -v nvidia-smi &> /dev/null; then
        nvidia-smi --query-gpu=name,temperature.gpu,utilization.gpu,memory.used,memory.total \
            --format=csv,noheader,nounits | \
            awk -F', ' '{
                printf "   GPU: %s\n", $1
                printf "   온도: %s°C | 사용률: %s%% | VRAM: %s/%s MiB\n", $2, $3, $4, $5
            }'
    else
        echo "   nvidia-smi를 찾을 수 없습니다."
    fi
    echo ""

    # 학습 진행 상황
    echo "📊 학습 진행 상황:"
    echo "----------------------------------------"
    if [ -f "$RESULTS_CSV" ]; then
        # 헤더 출력
        head -1 "$RESULTS_CSV" | awk -F',' '{
            printf "   %-10s %-12s %-12s %-12s\n", "Epoch", "box_loss", "cls_loss", "dfl_loss"
        }'

        # 최근 5개 에포크 출력
        tail -5 "$RESULTS_CSV" | awk -F',' '{
            # epoch가 숫자인 경우만 출력 (헤더 제외)
            if ($1 ~ /^[0-9]+$/) {
                printf "   %-10s %-12.4f %-12.4f %-12.4f\n", $1, $5, $6, $7
            }
        }'

        echo ""

        # 검증 메트릭 (마지막 에포크)
        echo "📈 검증 메트릭 (최신):"
        tail -1 "$RESULTS_CSV" | awk -F',' '{
            if ($1 ~ /^[0-9]+$/) {
                printf "   mAP@50: %.4f | mAP@50-95: %.4f\n", $10, $11
                printf "   Precision: %.4f | Recall: %.4f\n", $8, $9
            }
        }'
    else
        echo "   ⏳ results.csv 파일 생성 대기 중..."
        echo "   학습이 시작되면 여기에 결과가 표시됩니다."
    fi

    echo ""
    echo "=========================================="
    echo "갱신 주기: ${INTERVAL}초 | Ctrl+C로 종료"
    echo "=========================================="

    sleep $INTERVAL
done
