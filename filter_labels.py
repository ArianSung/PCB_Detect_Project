#!/usr/bin/env python3
"""
라벨 파일에서 특정 클래스 제거 스크립트
ceramic capacitor (class 3)와 zener diode (class 11) 제거
"""
import os
from pathlib import Path

# 제거할 클래스 ID
REMOVE_CLASSES = {3, 11}  # ceramic capacitor, zener diode

# 클래스 ID 재매핑 (제거 후 인덱스 조정)
# 기존: 0,1,2,3,4,5,6,7,8,9,10,11
# 새로: 0,1,2,X,3,4,5,6,7,8,9,X
CLASS_REMAP = {
    0: 0,  # Electrolytic capacitor
    1: 1,  # IC
    2: 2,  # cds
    # 3: ceramic capacitor (제거)
    4: 3,  # diode
    5: 4,  # led
    6: 5,  # pinheader
    7: 6,  # pinsocket
    8: 7,  # resistor
    9: 8,  # switch
    10: 9, # transistor
    # 11: zener diode (제거)
}

def filter_label_file(label_path):
    """라벨 파일에서 특정 클래스 제거 및 인덱스 재매핑"""
    if not label_path.exists():
        return

    lines = label_path.read_text().strip().split('\n')
    if not lines or lines == ['']:
        return

    new_lines = []
    removed_count = 0
    remapped_count = 0

    for line in lines:
        if not line.strip():
            continue

        parts = line.strip().split()
        if len(parts) < 5:
            continue

        class_id = int(parts[0])

        # 제거할 클래스는 스킵
        if class_id in REMOVE_CLASSES:
            removed_count += 1
            continue

        # 클래스 ID 재매핑
        if class_id in CLASS_REMAP:
            new_class_id = CLASS_REMAP[class_id]
            parts[0] = str(new_class_id)
            remapped_count += 1

        new_lines.append(' '.join(parts))

    # 파일 업데이트
    if new_lines:
        label_path.write_text('\n'.join(new_lines) + '\n')
    else:
        # 모든 라벨이 제거된 경우 파일 삭제 (선택사항)
        # label_path.unlink()
        label_path.write_text('')

    return removed_count, remapped_count

def process_directory(label_dir):
    """디렉토리 내 모든 라벨 파일 처리"""
    label_dir = Path(label_dir)
    if not label_dir.exists():
        print(f"❌ 디렉토리 없음: {label_dir}")
        return

    total_removed = 0
    total_remapped = 0
    file_count = 0

    for label_file in label_dir.glob('*.txt'):
        result = filter_label_file(label_file)
        if result:
            removed, remapped = result
            total_removed += removed
            total_remapped += remapped
            file_count += 1

    print(f"✅ {label_dir.name}: {file_count}개 파일 처리")
    print(f"   - 제거된 라벨: {total_removed}개")
    print(f"   - 재매핑된 라벨: {total_remapped}개")

if __name__ == '__main__':
    base_dir = Path('/home/sys1041/work_project/PCB_defect_detect-3')

    # train, valid, test 라벨 디렉토리 처리
    for split in ['train', 'valid', 'test']:
        label_dir = base_dir / split / 'labels'
        print(f"\n📂 처리 중: {split}/labels")
        process_directory(label_dir)

    print("\n✅ 라벨 필터링 완료!")
