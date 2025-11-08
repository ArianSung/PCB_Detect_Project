#!/bin/bash
# 실시간 GPU 모니터링 (간단 버전)
# 사용법: bash scripts/monitor_gpu_simple.sh

echo "실시간 GPU 모니터링 (1초마다 갱신, Ctrl+C로 종료)"
echo ""

watch -n 1 "nvidia-smi --query-gpu=timestamp,memory.used,memory.total,utilization.gpu,utilization.memory,temperature.gpu,power.draw --format=csv,noheader,nounits | awk -F', ' '{
    printf \"시간: %s\\n\", \$1
    printf \"VRAM: %s MB / %s MB (%.1f%%)\\n\", \$2, \$3, (\$2/\$3)*100
    printf \"GPU 사용률: %s%%\\n\", \$4
    printf \"메모리 사용률: %s%%\\n\", \$5
    printf \"온도: %s°C\\n\", \$6
    printf \"전력: %s W\\n\", \$7
    printf \"\\n\"
    if (\$2/\$3 > 0.95) {
        printf \"⚠️  경고: VRAM 95%% 이상 사용 중!\\n\"
    }
    if (\$2 > 15000) {
        printf \"🔥 주의: VRAM 15GB 초과! 스와핑 가능성 있음!\\n\"
    }
}'"
