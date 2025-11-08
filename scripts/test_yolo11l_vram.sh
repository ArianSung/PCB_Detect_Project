#!/bin/bash
################################################################################
# YOLOv11l VRAM 사용량 테스트 스크립트
#
# 목적: RTX 4080 Super 16GB에서 YOLOv11l 실제 VRAM 사용량 확인
# 테스트 시간: 약 5-10분 (5 epochs)
################################################################################

set -e

echo "========================================"
echo "YOLOv11l VRAM 사용량 테스트"
echo "========================================"
echo "Date: $(date)"
echo "GPU: $(nvidia-smi --query-gpu=name --format=csv,noheader | head -1)"
echo ""

# 경로 설정
PROJECT_ROOT="/home/sys1041/work_project"
DATA_YAML="$PROJECT_ROOT/data/processed/soldef_ai_yolo/data.yaml"

# 가상환경 확인
if [ -z "$CONDA_DEFAULT_ENV" ]; then
    echo "⚠️  Conda 환경 활성화 필요!"
    echo "실행: conda activate pcb_defect"
    exit 1
fi

echo "✅ Conda 환경: $CONDA_DEFAULT_ENV"
echo ""

# GPU 초기 상태 확인
echo "========================================"
echo "GPU 초기 상태"
echo "========================================"
nvidia-smi --query-gpu=memory.used,memory.total,temperature.gpu,utilization.gpu \
    --format=csv,noheader,nounits | \
    awk -F', ' '{
        printf "VRAM 사용: %s MB / %s MB (%.1f%%)\n", $1, $2, ($1/$2)*100
        printf "온도: %s°C\n", $3
        printf "GPU 사용률: %s%%\n", $4
    }'
echo ""

# 테스트 설정 출력
echo "========================================"
echo "테스트 설정"
echo "========================================"
echo "데이터셋: SolDef_AI (5 클래스)"
echo "모델: yolo11l.pt (25.3M params)"
echo "Epochs: 5 (테스트용)"
echo ""
echo "배치 사이즈 테스트:"
echo "  1. batch=16 (권장) - 예상 VRAM: 12-14GB"
echo "  2. batch=32 (비교) - 예상 VRAM: 18GB"
echo ""

# 사용자 선택
read -p "테스트할 배치 사이즈를 선택하세요 (16/32, 기본 16): " BATCH_SIZE
BATCH_SIZE=${BATCH_SIZE:-16}

if [ "$BATCH_SIZE" != "16" ] && [ "$BATCH_SIZE" != "32" ]; then
    echo "❌ 잘못된 입력. 16 또는 32를 입력하세요."
    exit 1
fi

echo ""
echo "선택된 배치 사이즈: $BATCH_SIZE"
echo ""

# VRAM 모니터링 백그라운드 실행
echo "VRAM 모니터링 시작... (logs/vram_usage.csv에 저장)"
mkdir -p logs
nvidia-smi --query-gpu=timestamp,memory.used,memory.total,utilization.gpu,temperature.gpu \
    --format=csv -l 1 > logs/vram_usage.csv &
MONITOR_PID=$!

echo "VRAM 모니터링 PID: $MONITOR_PID"
echo ""

# Trap으로 종료 시 모니터링 프로세스 종료
trap "kill $MONITOR_PID 2>/dev/null; echo 'VRAM 모니터링 종료'; exit" EXIT INT TERM

# 학습 시작
echo "========================================"
echo "YOLOv11l 학습 시작"
echo "========================================"
echo ""

# YOLOv11l 테스트 학습
/home/sys1041/miniconda3/envs/pcb_defect/bin/yolo detect train \
    data="$DATA_YAML" \
    model=yolo11l.pt \
    epochs=5 \
    batch=$BATCH_SIZE \
    imgsz=640 \
    device=0 \
    project=runs/detect \
    name=test_yolo11l_batch${BATCH_SIZE} \
    exist_ok=True \
    pretrained=True \
    optimizer=AdamW \
    lr0=0.001 \
    weight_decay=0.0005 \
    patience=10 \
    amp=True \
    verbose=True \
    plots=False \
    cache=False \
    workers=8

# 학습 완료 후 VRAM 모니터링 종료
kill $MONITOR_PID 2>/dev/null
trap - EXIT INT TERM

echo ""
echo "========================================"
echo "테스트 완료!"
echo "========================================"
echo "Date: $(date)"
echo ""

# 최종 GPU 상태
echo "최종 GPU 상태:"
nvidia-smi --query-gpu=memory.used,memory.total,temperature.gpu,utilization.gpu \
    --format=csv,noheader,nounits | \
    awk -F', ' '{
        printf "VRAM 사용: %s MB / %s MB (%.1f%%)\n", $1, $2, ($1/$2)*100
        printf "온도: %s°C\n", $3
        printf "GPU 사용률: %s%%\n", $4
    }'
echo ""

# VRAM 사용량 분석
echo "========================================"
echo "VRAM 사용량 분석"
echo "========================================"

if [ -f "logs/vram_usage.csv" ]; then
    echo "최대 VRAM 사용량:"
    tail -n +2 logs/vram_usage.csv | \
        awk -F', ' '{print $2}' | \
        sort -n | \
        tail -1 | \
        awk '{printf "  %s MB (%.2f GB)\n", $1, $1/1024}'

    echo ""
    echo "평균 VRAM 사용량:"
    tail -n +2 logs/vram_usage.csv | \
        awk -F', ' '{sum+=$2; count++} END {printf "  %.0f MB (%.2f GB)\n", sum/count, sum/count/1024}'

    echo ""
    echo "💡 전체 VRAM 로그: logs/vram_usage.csv"
fi

echo ""
echo "📊 학습 결과:"
echo "  - 모델: runs/detect/test_yolo11l_batch${BATCH_SIZE}/weights/best.pt"
echo "  - 메트릭: runs/detect/test_yolo11l_batch${BATCH_SIZE}/results.csv"
echo ""

# 결론
echo "========================================"
echo "테스트 결론"
echo "========================================"

if [ "$BATCH_SIZE" == "16" ]; then
    echo "✅ batch=16 테스트 완료"
    echo "   - 예상: 12-14GB"
    echo "   - 실제: logs/vram_usage.csv 확인"
    echo ""
    echo "💡 다음 단계:"
    echo "   1. VRAM 사용량이 14GB 이하면 안정적"
    echo "   2. batch=32 테스트하려면 다시 실행"
else
    echo "⚠️  batch=32 테스트 완료"
    echo "   - 예상: 18GB (스와핑 가능)"
    echo "   - 실제: logs/vram_usage.csv 확인"
    echo ""
    echo "💡 분석:"
    echo "   - 18GB 이상이면 스와핑 발생"
    echo "   - 학습 속도가 매우 느렸다면 스와핑 확인"
    echo "   - batch=16 + accumulate=2 권장"
fi

echo ""
