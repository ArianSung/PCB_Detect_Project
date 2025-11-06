#!/usr/bin/env python3
"""
GPU 실시간 모니터링 (Python)

사용법:
    python monitor_gpu.py           # 1초마다 갱신
    python monitor_gpu.py 5         # 5초마다 갱신
"""

import subprocess
import time
import sys
import os

def clear_screen():
    """화면 지우기"""
    os.system('clear' if os.name != 'nt' else 'cls')

def get_gpu_info():
    """GPU 정보 가져오기"""
    cmd = [
        'nvidia-smi',
        '--query-gpu=index,name,temperature.gpu,utilization.gpu,utilization.memory,memory.used,memory.total,power.draw,power.limit',
        '--format=csv,noheader,nounits'
    ]

    try:
        output = subprocess.check_output(cmd).decode('utf-8')
        return output.strip().split(', ')
    except Exception as e:
        return None

def get_process_info():
    """GPU에서 실행 중인 프로세스 정보"""
    cmd = [
        'nvidia-smi',
        '--query-compute-apps=pid,process_name,used_memory',
        '--format=csv,noheader,nounits'
    ]

    try:
        output = subprocess.check_output(cmd).decode('utf-8').strip()
        if output:
            return output.split('\n')
        return []
    except:
        return []

def format_memory(mb):
    """메모리를 적절한 단위로 표시"""
    mb = float(mb)
    if mb < 1024:
        return f"{mb:.0f} MB"
    else:
        return f"{mb/1024:.2f} GB"

def monitor_gpu(interval=1):
    """GPU 실시간 모니터링"""
    print("GPU 실시간 모니터링 시작 (Ctrl+C로 종료)\n")

    try:
        while True:
            clear_screen()

            # 현재 시간
            current_time = time.strftime("%Y-%m-%d %H:%M:%S")
            print("=" * 80)
            print(f"GPU 모니터링 - {current_time}")
            print("=" * 80)

            # GPU 정보
            gpu_info = get_gpu_info()

            if gpu_info and len(gpu_info) >= 9:
                gpu_id, name, temp, gpu_util, mem_util, mem_used, mem_total, power, power_limit = gpu_info

                print(f"\n🎮 GPU {gpu_id}: {name}")
                print(f"   온도: {temp}°C")
                print(f"   GPU 사용률: {gpu_util}%")
                print(f"   메모리 사용률: {mem_util}%")
                print(f"   VRAM: {format_memory(mem_used)} / {format_memory(mem_total)} ({float(mem_used)/float(mem_total)*100:.1f}%)")
                print(f"   전력: {power}W / {power_limit}W ({float(power)/float(power_limit)*100:.1f}%)")

                # 진행률 바 (VRAM)
                mem_percent = float(mem_used) / float(mem_total)
                bar_length = 50
                filled = int(bar_length * mem_percent)
                bar = '█' * filled + '░' * (bar_length - filled)
                print(f"\n   VRAM: [{bar}] {mem_percent*100:.1f}%")

                # 진행률 바 (GPU 사용률)
                gpu_percent = float(gpu_util) / 100
                filled = int(bar_length * gpu_percent)
                bar = '█' * filled + '░' * (bar_length - filled)
                print(f"   GPU:  [{bar}] {gpu_util}%")

            else:
                print("\n⚠️  GPU 정보를 가져올 수 없습니다.")

            # 실행 중인 프로세스
            processes = get_process_info()
            if processes:
                print("\n" + "=" * 80)
                print("🔧 실행 중인 프로세스:")
                print("=" * 80)
                print(f"{'PID':<10} {'프로세스':<40} {'VRAM':<15}")
                print("-" * 80)

                for proc in processes:
                    parts = proc.split(', ')
                    if len(parts) >= 3:
                        pid, name, mem = parts[0], parts[1], parts[2]
                        print(f"{pid:<10} {name[:40]:<40} {format_memory(mem):<15}")

            print("\n" + "=" * 80)
            print(f"갱신 주기: {interval}초 | Ctrl+C로 종료")
            print("=" * 80)

            time.sleep(interval)

    except KeyboardInterrupt:
        print("\n\n모니터링 종료.")
        sys.exit(0)

if __name__ == "__main__":
    interval = int(sys.argv[1]) if len(sys.argv) > 1 else 1
    monitor_gpu(interval)
