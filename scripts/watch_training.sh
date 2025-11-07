#!/bin/bash
# 학습 진행 상황 실시간 모니터링

echo "학습 진행 상황 실시간 모니터링 (Ctrl+C로 종료)"
echo "=========================================="
echo ""

while true; do
    clear
    echo "=========================================="
    echo "  YOLO 학습 실시간 모니터링"
    echo "=========================================="
    echo "업데이트: $(date '+%Y-%m-%d %H:%M:%S')"
    echo ""

    # GPU 상태
    echo "📊 GPU 상태:"
    nvidia-smi --query-gpu=name,temperature.gpu,utilization.gpu,memory.used,memory.total --format=csv,noheader | \
        awk -F', ' '{printf "   GPU: %s\n   온도: %s°C | 사용률: %s%% | VRAM: %s/%s\n", $1, $2, $3, $4, $5}'
    echo ""

    # 학습 진행 상황 (results.csv 마지막 줄)
    echo "📈 학습 진행 상황:"
    if [ -f yolo/runs/train/pcb_defect/results.csv ]; then
        tail -1 yolo/runs/train/pcb_defect/results.csv | \
            awk -F',' '{printf "   Epoch: %s | mAP50: %s | mAP50-95: %s\n", $1, $10, $11}'
    else
        echo "   (아직 results.csv 생성 안됨)"
    fi
    echo ""

    # 최근 로그 (마지막 15줄)
    echo "📝 최근 로그:"
    echo "------------------------------------------"
    tail -15 yolo/runs/train/pcb_defect/results.csv 2>/dev/null || echo "   로그 대기 중..."
    echo "------------------------------------------"
    echo ""
    echo "갱신 주기: 3초 | Ctrl+C로 종료"

    sleep 3
done
