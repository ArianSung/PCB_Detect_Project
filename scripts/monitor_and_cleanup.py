#!/usr/bin/env python3
"""
v5 모델 학습 모니터링 및 v4 모델 자동 정리 스크립트
"""

import os
import time
import shutil
from pathlib import Path

def check_training_complete():
    """학습 완료 여부 확인"""
    best_model = "/home/sys1041/work_project/runs/detect/v5_component_detector/weights/best.pt"
    copied_model = "/home/sys1041/work_project/models/component_detector_v5_best.pt"

    # 두 모델 파일이 모두 존재하면 학습 완료로 간주
    return os.path.exists(best_model) and os.path.exists(copied_model)

def get_v4_directories():
    """v4 관련 디렉토리 및 파일 찾기"""
    v4_items = []

    # runs/detect 내 v4 관련 디렉토리
    runs_detect = Path("/home/sys1041/work_project/runs/detect")
    if runs_detect.exists():
        for item in runs_detect.iterdir():
            if 'v4' in item.name.lower():
                v4_items.append(str(item))

    # models 내 v4 모델 파일
    models_dir = Path("/home/sys1041/work_project/models")
    if models_dir.exists():
        for item in models_dir.iterdir():
            if 'v4' in item.name.lower() and item.is_file():
                v4_items.append(str(item))

    # PCB_defect_detect-4 데이터셋 디렉토리
    v4_dataset = Path("/home/sys1041/work_project/PCB_defect_detect-4")
    if v4_dataset.exists():
        v4_items.append(str(v4_dataset))

    return v4_items

def get_directory_size(path):
    """디렉토리 또는 파일 크기 계산 (MB)"""
    path_obj = Path(path)
    if path_obj.is_file():
        return path_obj.stat().st_size / (1024 * 1024)
    elif path_obj.is_dir():
        total = 0
        for item in path_obj.rglob('*'):
            if item.is_file():
                total += item.stat().st_size
        return total / (1024 * 1024)
    return 0

def delete_v4_models():
    """v4 모델 및 관련 파일 삭제"""
    v4_items = get_v4_directories()

    if not v4_items:
        print("삭제할 v4 관련 파일이 없습니다.")
        return

    print("\n" + "="*60)
    print("v4 모델 삭제 시작")
    print("="*60)

    total_freed = 0

    for item in v4_items:
        item_path = Path(item)
        if not item_path.exists():
            continue

        size_mb = get_directory_size(item)

        try:
            if item_path.is_dir():
                shutil.rmtree(item)
                print(f"✓ 디렉토리 삭제: {item} ({size_mb:.2f} MB)")
            else:
                item_path.unlink()
                print(f"✓ 파일 삭제: {item} ({size_mb:.2f} MB)")

            total_freed += size_mb
        except Exception as e:
            print(f"✗ 삭제 실패: {item} - {e}")

    print("\n" + "="*60)
    print(f"총 확보된 공간: {total_freed:.2f} MB ({total_freed/1024:.2f} GB)")
    print("="*60 + "\n")

def main():
    print("v5 모델 학습 모니터링 시작...")
    print("학습 완료 시 v4 모델을 자동으로 삭제합니다.\n")

    # 학습 완료 대기
    check_interval = 60  # 60초마다 확인

    while True:
        if check_training_complete():
            print("\n✓ v5 모델 학습이 완료되었습니다!")
            print("\nv4 모델 삭제를 시작합니다...\n")
            delete_v4_models()
            print("\n완료!")
            break
        else:
            print(f"학습 진행 중... (다음 확인: {check_interval}초 후)")
            time.sleep(check_interval)

if __name__ == "__main__":
    main()
