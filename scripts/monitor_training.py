#!/usr/bin/env python3
"""
YOLO 학습 실시간 모니터링 스크립트

results.csv 파일을 읽어서 학습 진행 상황을 실시간으로 표시합니다.
"""

import os
import time
import sys
from pathlib import Path


def clear_screen():
    """화면 지우기"""
    os.system('clear' if os.name != 'nt' else 'cls')


def format_time(seconds):
    """초를 시:분:초 형식으로 변환"""
    hours = int(seconds // 3600)
    minutes = int((seconds % 3600) // 60)
    secs = int(seconds % 60)
    return f"{hours:02d}:{minutes:02d}:{secs:02d}"


def monitor_training(results_path, refresh_interval=5):
    """
    학습 모니터링

    Args:
        results_path: results.csv 파일 경로
        refresh_interval: 갱신 주기 (초)
    """
    print("=" * 80)
    print("YOLO 학습 모니터링 시작")
    print("=" * 80)
    print(f"파일: {results_path}")
    print(f"갱신 주기: {refresh_interval}초")
    print("\nCtrl+C를 눌러 종료하세요.\n")

    last_epoch = -1
    start_time = time.time()

    try:
        while True:
            if not os.path.exists(results_path):
                print(f"대기 중... results.csv 파일을 찾을 수 없습니다.")
                time.sleep(refresh_interval)
                continue

            # CSV 파일 읽기
            try:
                with open(results_path, 'r') as f:
                    lines = f.readlines()

                if len(lines) < 2:
                    print("학습 시작 대기 중...")
                    time.sleep(refresh_interval)
                    continue

                # 헤더와 최신 데이터
                header = lines[0].strip().split(',')
                latest_line = lines[-1].strip().split(',')

                # 데이터 파싱
                data = {}
                for i, key in enumerate(header):
                    key = key.strip()
                    if i < len(latest_line):
                        try:
                            data[key] = float(latest_line[i].strip())
                        except:
                            data[key] = latest_line[i].strip()

                # 에포크 정보
                current_epoch = int(data.get('epoch', 0))
                total_epochs = 150  # 기본값

                # 새로운 에포크일 때만 화면 갱신
                if current_epoch != last_epoch or True:  # 항상 갱신
                    elapsed = time.time() - start_time

                    clear_screen()

                    print("=" * 80)
                    print(f"{'YOLO 학습 모니터링':^80}")
                    print("=" * 80)

                    # 진행 상황
                    progress = (current_epoch / total_epochs) * 100 if total_epochs > 0 else 0
                    progress_bar = "█" * int(progress / 2) + "░" * (50 - int(progress / 2))

                    print(f"\n📊 진행 상황: [{progress_bar}] {progress:.1f}%")
                    print(f"   에포크: {current_epoch}/{total_epochs}")
                    print(f"   경과 시간: {format_time(elapsed)}")

                    if current_epoch > 0:
                        avg_time_per_epoch = elapsed / current_epoch
                        remaining_epochs = total_epochs - current_epoch
                        eta = avg_time_per_epoch * remaining_epochs
                        print(f"   예상 남은 시간: {format_time(eta)}")

                    # 손실 값
                    print("\n" + "=" * 80)
                    print("📉 손실 함수 (Loss)")
                    print("=" * 80)

                    if 'train/box_loss' in data:
                        box_loss = data['train/box_loss']
                        cls_loss = data['train/cls_loss']
                        dfl_loss = data['train/dfl_loss']

                        print(f"   Box Loss:   {box_loss:>8.4f}  (바운딩 박스 위치)")
                        print(f"   Class Loss: {cls_loss:>8.4f}  (클래스 분류)")
                        print(f"   DFL Loss:   {dfl_loss:>8.4f}  (분포 초점)")

                    # 검증 메트릭
                    print("\n" + "=" * 80)
                    print("📈 검증 메트릭 (Validation Metrics)")
                    print("=" * 80)

                    if 'metrics/mAP50(B)' in data:
                        map50 = data['metrics/mAP50(B)']
                        map50_95 = data['metrics/mAP50-95(B)']
                        precision = data['metrics/precision(B)']
                        recall = data['metrics/recall(B)']

                        print(f"   mAP@50:     {map50:>8.4f}  (정확도 - IoU 0.5)")
                        print(f"   mAP@50-95:  {map50_95:>8.4f}  (정확도 - IoU 0.5~0.95)")
                        print(f"   Precision:  {precision:>8.4f}  (정밀도)")
                        print(f"   Recall:     {recall:>8.4f}  (재현율)")
                    else:
                        print("   (검증 데이터 대기 중...)")

                    # 학습률
                    if 'lr/pg0' in data:
                        lr0 = data['lr/pg0']
                        lr1 = data['lr/pg1']
                        lr2 = data['lr/pg2']

                        print("\n" + "=" * 80)
                        print("🎓 학습률 (Learning Rate)")
                        print("=" * 80)
                        print(f"   LR (Group 0): {lr0:.6f}")
                        print(f"   LR (Group 1): {lr1:.6f}")
                        print(f"   LR (Group 2): {lr2:.6f}")

                    print("\n" + "=" * 80)
                    print(f"마지막 업데이트: {time.strftime('%Y-%m-%d %H:%M:%S')}")
                    print(f"다음 갱신까지: {refresh_interval}초")
                    print("=" * 80)
                    print("\n💡 Ctrl+C를 눌러 모니터링 종료\n")

                    last_epoch = current_epoch

            except Exception as e:
                print(f"데이터 파싱 오류: {e}")

            time.sleep(refresh_interval)

    except KeyboardInterrupt:
        print("\n\n모니터링 종료")
        print("=" * 80)


if __name__ == "__main__":
    # 기본 경로
    default_path = "yolo/runs/train/pcb_defect/results.csv"

    if len(sys.argv) > 1:
        results_path = sys.argv[1]
    else:
        results_path = default_path

    refresh_interval = 5
    if len(sys.argv) > 2:
        try:
            refresh_interval = int(sys.argv[2])
        except:
            pass

    monitor_training(results_path, refresh_interval)
