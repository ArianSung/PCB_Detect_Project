#!/usr/bin/env python3
"""
VRAM 모니터링과 함께 YOLO 학습 실행
VRAM이 지정된 임계값을 초과하면 학습을 자동으로 종료합니다.
"""

import os
import sys
import time
import signal
import subprocess
import threading
from pathlib import Path

# 프로젝트 루트
PROJECT_ROOT = Path(__file__).parent.parent

def get_gpu_memory_usage():
    """GPU 메모리 사용량 조회 (MB 단위)"""
    try:
        result = subprocess.run(
            ['nvidia-smi', '--query-gpu=memory.used', '--format=csv,noheader,nounits'],
            capture_output=True,
            text=True,
            check=True
        )
        memory_mb = int(result.stdout.strip())
        return memory_mb
    except Exception as e:
        print(f"❌ GPU 메모리 조회 실패: {e}")
        return 0

def monitor_vram(process, threshold_mb, check_interval=2):
    """
    VRAM 사용량을 모니터링하고 임계값 초과 시 프로세스 종료

    Args:
        process: 모니터링할 프로세스
        threshold_mb: VRAM 임계값 (MB)
        check_interval: 체크 간격 (초)
    """
    max_memory = 0

    print(f"\n{'='*80}")
    print(f"VRAM 모니터링 시작 (임계값: {threshold_mb} MB = {threshold_mb/1024:.1f} GB)")
    print(f"{'='*80}\n")

    try:
        while process.poll() is None:  # 프로세스가 실행 중인 동안
            current_memory = get_gpu_memory_usage()
            max_memory = max(max_memory, current_memory)

            # 상태 출력
            print(f"\r현재 VRAM: {current_memory:,} MB ({current_memory/1024:.2f} GB) | "
                  f"최대: {max_memory:,} MB ({max_memory/1024:.2f} GB) | "
                  f"임계값: {threshold_mb:,} MB ({threshold_mb/1024:.1f} GB)", end='', flush=True)

            # 임계값 초과 체크
            if current_memory > threshold_mb:
                print(f"\n\n{'='*80}")
                print(f"⚠️  VRAM 임계값 초과!")
                print(f"  - 현재 사용량: {current_memory:,} MB ({current_memory/1024:.2f} GB)")
                print(f"  - 임계값: {threshold_mb:,} MB ({threshold_mb/1024:.1f} GB)")
                print(f"  - 학습을 종료합니다...")
                print(f"{'='*80}\n")

                # 프로세스 종료 (SIGTERM)
                process.terminate()

                # 5초 대기 후에도 종료되지 않으면 강제 종료
                time.sleep(5)
                if process.poll() is None:
                    print("⚠️  프로세스가 종료되지 않아 강제 종료합니다...")
                    process.kill()

                return True

            time.sleep(check_interval)

    except KeyboardInterrupt:
        print(f"\n\n사용자에 의해 중단되었습니다.")
        process.terminate()
        return False

    print(f"\n\n✓ 학습 정상 완료")
    print(f"  - 최대 VRAM 사용량: {max_memory:,} MB ({max_memory/1024:.2f} GB)")
    return False

def train_with_monitoring(batch_size, epochs, threshold_gb=15.0):
    """
    VRAM 모니터링과 함께 학습 실행

    Args:
        batch_size: 배치 크기
        epochs: 에폭 수
        threshold_gb: VRAM 임계값 (GB)
    """
    threshold_mb = int(threshold_gb * 1024)

    print("=" * 80)
    print(f"VRAM 모니터링과 함께 YOLOv11l 학습 시작")
    print("=" * 80)
    print(f"  - 배치 크기: {batch_size}")
    print(f"  - 에폭: {epochs}")
    print(f"  - VRAM 임계값: {threshold_gb} GB ({threshold_mb:,} MB)")
    print("=" * 80)

    # 학습 명령어 구성 (직접 YOLO CLI 사용)
    conda_activate = "source /home/sys1041/miniconda3/etc/profile.d/conda.sh && conda activate pcb_defect"
    data_yaml = str(PROJECT_ROOT / "data" / "raw" / "roboflow_pcb" / "data.yaml")

    # YOLO 학습 명령어
    yolo_cmd = (
        f"yolo detect train "
        f"data={data_yaml} "
        f"model=yolo11l.pt "
        f"epochs={epochs} "
        f"batch={batch_size} "
        f"imgsz=640 "
        f"device=0 "
        f"project=runs/detect "
        f"name=batch{batch_size}_test "
        f"exist_ok=True "
        f"pretrained=True "
        f"optimizer=AdamW "
        f"lr0=0.001 "
        f"lrf=0.01 "
        f"weight_decay=0.0005 "
        f"patience=50 "
        f"amp=True "
        f"verbose=True "
        f"plots=False "
        f"cache=False "
        f"workers=8 "
        f"hsv_h=0.015 "
        f"hsv_s=0.7 "
        f"hsv_v=0.4 "
        f"degrees=15 "
        f"translate=0.1 "
        f"scale=0.5 "
        f"flipud=0.0 "
        f"fliplr=0.5 "
        f"mosaic=1.0 "
        f"mixup=0.15 "
        f"copy_paste=0.1"
    )

    cmd = f"{conda_activate} && {yolo_cmd}"
    env = os.environ.copy()

    # 프로세스 시작
    process = subprocess.Popen(
        cmd,
        shell=True,
        executable='/bin/bash',
        stdout=subprocess.PIPE,
        stderr=subprocess.STDOUT,
        text=True,
        bufsize=1,
        env=env
    )

    # 출력 스레드 (학습 로그 실시간 출력)
    def print_output():
        for line in process.stdout:
            print(line, end='')

    output_thread = threading.Thread(target=print_output, daemon=True)
    output_thread.start()

    # VRAM 모니터링 시작 (별도 스레드)
    exceeded = monitor_vram(process, threshold_mb)

    # 프로세스 종료 대기
    return_code = process.wait()

    print(f"\n{'='*80}")
    if exceeded:
        print("❌ VRAM 임계값 초과로 학습이 중단되었습니다.")
        print(f"  → batch={batch_size}는 16GB VRAM에 너무 큽니다.")
        print(f"  → batch=16 권장 (10-11GB 사용)")
    elif return_code == 0:
        print("✓ 학습이 성공적으로 완료되었습니다.")
    else:
        print(f"❌ 학습이 오류로 종료되었습니다 (return code: {return_code})")
    print(f"{'='*80}")

    return not exceeded

if __name__ == "__main__":
    import argparse

    parser = argparse.ArgumentParser(description="Train YOLO with VRAM monitoring")
    parser.add_argument("--batch", type=int, default=32, help="배치 크기")
    parser.add_argument("--epochs", type=int, default=1, help="에폭 수")
    parser.add_argument("--threshold", type=float, default=15.0, help="VRAM 임계값 (GB)")

    args = parser.parse_args()

    success = train_with_monitoring(args.batch, args.epochs, args.threshold)

    if success:
        print("\n✓ batch={} 사용 가능!".format(args.batch))
    else:
        print("\n❌ batch={} 사용 불가. 더 작은 배치 크기를 사용하세요.".format(args.batch))

    sys.exit(0 if success else 1)
