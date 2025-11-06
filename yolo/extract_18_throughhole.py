#!/usr/bin/env python3
"""
ElectroCom61에서 18개 Through-hole 클래스 추출 및 재분할 스크립트

단계:
1. 18개 Through-hole 클래스 추출
2. Train에만 있는 클래스들을 70/20/10으로 재분할
3. 클래스 ID 리매핑 (0-17)
"""

import os
import shutil
import random
from pathlib import Path
from collections import Counter, defaultdict

# 경로 설정
SOURCE_DIR = "/home/sys1041/work_project/data/raw/ElectroCom61"
OUTPUT_DIR = "/home/sys1041/work_project/data/processed/throughhole_18classes"

# 18개 Through-hole 클래스 (원본 클래스 ID)
# 분석 결과 상위 18개 클래스
SELECTED_CLASSES = {
    33: 'Inductor',                              # 293
    37: 'LED-Light',                             # 293
    39: 'MLC-Capacitor',                         # 292
    46: 'Push-Switch',                           # 277
    12: 'Buzzer',                                # 272
    16: 'Diode',                                 # 265
    26: 'High-Voltage-Ceramic-Capacitor',       # 257
    60: 'Zener-Diode',                          # 238
    10: 'Bridge-Rectifier',                      # 230
    7: 'BJT-Transistor',                        # 229
    58: 'Trimmer-Potentiometer',                # 228
    43: 'NTC-Thermistor',                       # 222
    14: 'Capacitor-470mf',                      # 219
    13: 'Capacitor-10mf',                       # 218
    38: 'Low-Voltage-Ceramic-Capacitor',        # 217
    31: 'IGBT',                                 # 216
    56: 'Tact-Switch',                          # 212
    40: 'MOSFET'                                # 207
}

# Train에만 있는 클래스들 (재분할 필요)
TRAIN_ONLY_CLASSES = {33, 37, 39, 46, 12, 26}

# 새로운 클래스 ID (0-17)
CLASS_ID_MAPPING = {old_id: new_id for new_id, old_id in enumerate(sorted(SELECTED_CLASSES.keys()))}

# 분할 비율
SPLIT_RATIOS = {
    'train': 0.70,
    'valid': 0.20,
    'test': 0.10
}

random.seed(42)

def find_images_with_class(split, class_id):
    """특정 클래스를 포함한 이미지 찾기"""
    label_dir = os.path.join(SOURCE_DIR, split, 'labels')
    image_files = set()

    for label_file in os.listdir(label_dir):
        if not label_file.endswith('.txt'):
            continue

        label_path = os.path.join(label_dir, label_file)
        with open(label_path, 'r') as f:
            for line in f:
                parts = line.strip().split()
                if len(parts) >= 5:
                    cls_id = int(float(parts[0]))
                    if cls_id == class_id:
                        image_name = label_file.replace('.txt', '.jpg')
                        image_files.add((label_file, image_name))
                        break

    return list(image_files)

def extract_selected_classes(src_split, dst_split, image_files):
    """선택된 클래스만 추출하여 복사 (클래스 ID 리매핑)"""
    copied = 0

    for label_file, image_file in image_files:
        src_label = os.path.join(SOURCE_DIR, src_split, 'labels', label_file)
        src_image = os.path.join(SOURCE_DIR, src_split, 'images', image_file)

        if not os.path.exists(src_label) or not os.path.exists(src_image):
            continue

        # 라벨 읽기 및 필터링
        with open(src_label, 'r') as f:
            lines = f.readlines()

        new_lines = []
        has_selected_class = False

        for line in lines:
            parts = line.strip().split()
            if len(parts) >= 5:
                old_class_id = int(float(parts[0]))

                # 선택된 클래스인 경우
                if old_class_id in SELECTED_CLASSES:
                    has_selected_class = True
                    new_class_id = CLASS_ID_MAPPING[old_class_id]
                    new_lines.append(f"{new_class_id} {' '.join(parts[1:])}\n")

        # 선택된 클래스가 있는 경우만 저장
        if has_selected_class and new_lines:
            # 라벨 저장
            dst_label = os.path.join(OUTPUT_DIR, dst_split, 'labels', label_file)
            with open(dst_label, 'w') as f:
                f.writelines(new_lines)

            # 이미지 복사
            dst_image = os.path.join(OUTPUT_DIR, dst_split, 'images', image_file)
            shutil.copy2(src_image, dst_image)
            copied += 1

    return copied

def main():
    print("=" * 100)
    print("18개 Through-hole 클래스 추출 및 재분할")
    print("=" * 100)

    # 출력 디렉토리 생성
    for split in ['train', 'valid', 'test']:
        for subdir in ['images', 'labels']:
            os.makedirs(os.path.join(OUTPUT_DIR, split, subdir), exist_ok=True)

    print(f"\n📋 선택된 18개 클래스:")
    for idx, (old_id, name) in enumerate(sorted(SELECTED_CLASSES.items())):
        new_id = CLASS_ID_MAPPING[old_id]
        print(f"  {new_id:2d}. {name:<35} (원본 ID: {old_id})")

    print("\n" + "=" * 100)
    print("1️⃣ Train에만 있는 클래스 재분할")
    print("=" * 100)

    rebalance_stats = {}

    for class_id in TRAIN_ONLY_CLASSES:
        class_name = SELECTED_CLASSES[class_id]
        print(f"\n[{class_name} (클래스 {class_id})]")

        # Train에서 해당 클래스 포함 이미지 찾기
        train_files = find_images_with_class('train', class_id)
        print(f"  Train 원본: {len(train_files)}개")

        if not train_files:
            print(f"  ⚠️  데이터 없음")
            continue

        # 랜덤 셔플
        random.shuffle(train_files)

        # 분할 계산
        total = len(train_files)
        train_size = int(total * SPLIT_RATIOS['train'])
        valid_size = int(total * SPLIT_RATIOS['valid'])

        train_split = train_files[:train_size]
        valid_split = train_files[train_size:train_size + valid_size]
        test_split = train_files[train_size + valid_size:]

        # 추출 및 복사
        train_copied = extract_selected_classes('train', 'train', train_split)
        valid_copied = extract_selected_classes('train', 'valid', valid_split)
        test_copied = extract_selected_classes('train', 'test', test_split)

        print(f"  → Train: {train_copied}개")
        print(f"  → Valid: {valid_copied}개")
        print(f"  → Test: {test_copied}개")

        rebalance_stats[class_id] = {
            'train': train_copied,
            'valid': valid_copied,
            'test': test_copied
        }

    print("\n" + "=" * 100)
    print("2️⃣ 나머지 클래스 추출 (기존 분할 유지)")
    print("=" * 100)

    normal_stats = defaultdict(lambda: {'train': 0, 'valid': 0, 'test': 0})

    for split in ['train', 'valid', 'test']:
        print(f"\n[{split}]")

        label_dir = os.path.join(SOURCE_DIR, split, 'labels')
        image_dir = os.path.join(SOURCE_DIR, split, 'images')

        split_copied = 0

        for label_file in os.listdir(label_dir):
            if not label_file.endswith('.txt'):
                continue

            image_file = label_file.replace('.txt', '.jpg')
            label_path = os.path.join(label_dir, label_file)
            image_path = os.path.join(image_dir, image_file)

            if not os.path.exists(image_path):
                continue

            # 라벨 읽기
            with open(label_path, 'r') as f:
                lines = f.readlines()

            new_lines = []
            has_selected_class = False
            has_train_only_class = False
            class_counts = Counter()

            for line in lines:
                parts = line.strip().split()
                if len(parts) >= 5:
                    old_class_id = int(float(parts[0]))

                    # Train에만 있는 클래스는 스킵 (이미 재분할했음)
                    if old_class_id in TRAIN_ONLY_CLASSES:
                        has_train_only_class = True
                        continue

                    # 선택된 클래스인 경우
                    if old_class_id in SELECTED_CLASSES:
                        has_selected_class = True
                        new_class_id = CLASS_ID_MAPPING[old_class_id]
                        new_lines.append(f"{new_class_id} {' '.join(parts[1:])}\n")
                        class_counts[old_class_id] += 1

            # Train에만 있는 클래스만 포함된 이미지는 스킵
            if has_train_only_class and not has_selected_class:
                continue

            # 선택된 클래스가 있는 경우만 저장
            if has_selected_class and new_lines:
                # 라벨 저장
                dst_label = os.path.join(OUTPUT_DIR, split, 'labels', label_file)
                with open(dst_label, 'w') as f:
                    f.writelines(new_lines)

                # 이미지 복사
                dst_image = os.path.join(OUTPUT_DIR, split, 'images', image_file)
                shutil.copy2(image_path, dst_image)
                split_copied += 1

                # 통계
                for cls_id in class_counts:
                    normal_stats[cls_id][split] += class_counts[cls_id]

        print(f"  복사 완료: {split_copied}개")

    print("\n" + "=" * 100)
    print("3️⃣ data.yaml 생성")
    print("=" * 100)

    # 새로운 클래스 이름 리스트 (0-17)
    new_class_names = [SELECTED_CLASSES[old_id] for old_id in sorted(SELECTED_CLASSES.keys())]

    yaml_content = f"""train: {OUTPUT_DIR}/train/images
val: {OUTPUT_DIR}/valid/images
test: {OUTPUT_DIR}/test/images

nc: 18
names: {new_class_names}

# 원본 클래스 ID 매핑
# 새ID -> 원본ID
class_mapping:
"""

    for new_id, old_id in enumerate(sorted(SELECTED_CLASSES.keys())):
        yaml_content += f"  {new_id}: {old_id}  # {SELECTED_CLASSES[old_id]}\n"

    with open(os.path.join(OUTPUT_DIR, 'data.yaml'), 'w') as f:
        f.write(yaml_content)

    print(f"✅ data.yaml 생성 완료")

    print("\n" + "=" * 100)
    print("✅ 추출 완료!")
    print("=" * 100)

    # 최종 통계
    print("\n📊 최종 데이터 수:")
    for split in ['train', 'valid', 'test']:
        image_count = len(os.listdir(os.path.join(OUTPUT_DIR, split, 'images')))
        label_count = len(os.listdir(os.path.join(OUTPUT_DIR, split, 'labels')))
        print(f"  {split}: {image_count}개 이미지, {label_count}개 라벨")

    # 클래스별 통계
    print("\n📊 클래스별 통계:")
    print(f"\n{'ID':<4} {'클래스명':<35} {'Train':>8} {'Valid':>8} {'Test':>8} {'Total':>8}")
    print("-" * 100)

    for new_id, old_id in enumerate(sorted(SELECTED_CLASSES.keys())):
        name = SELECTED_CLASSES[old_id]

        if old_id in TRAIN_ONLY_CLASSES:
            stats = rebalance_stats.get(old_id, {'train': 0, 'valid': 0, 'test': 0})
        else:
            stats = normal_stats.get(old_id, {'train': 0, 'valid': 0, 'test': 0})

        train_cnt = stats['train']
        valid_cnt = stats['valid']
        test_cnt = stats['test']
        total_cnt = train_cnt + valid_cnt + test_cnt

        print(f"{new_id:<4} {name:<35} {train_cnt:>8} {valid_cnt:>8} {test_cnt:>8} {total_cnt:>8}")

    print(f"\n📂 출력 디렉토리: {OUTPUT_DIR}")
    print(f"📄 data.yaml: {OUTPUT_DIR}/data.yaml")

if __name__ == '__main__':
    main()
