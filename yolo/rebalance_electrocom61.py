#!/usr/bin/env python3
"""
ElectroCom61 데이터셋 재분할 스크립트

목적:
- Inductor(클래스 33)와 LED-Light(클래스 37)가 Train에만 있는 문제 해결
- Train 데이터의 일부를 Valid/Test로 이동하여 균형 맞추기

분할 비율: Train 70% / Valid 20% / Test 10%
"""

import os
import shutil
import random
from pathlib import Path
from collections import defaultdict

# 경로 설정
SOURCE_DIR = "/home/sys1041/work_project/data/raw/ElectroCom61/dataset/ElectroCom-61_v2"
OUTPUT_DIR = "/home/sys1041/work_project/data/raw/ElectroCom61_rebalanced"

# 재분할 대상 클래스
REBALANCE_CLASSES = {33: 'Inductor', 37: 'LED-Light'}

# 분할 비율
SPLIT_RATIOS = {
    'train': 0.70,
    'valid': 0.20,
    'test': 0.10
}

random.seed(42)

def find_images_with_classes(split, class_ids):
    """특정 클래스를 포함한 이미지 찾기"""
    label_dir = os.path.join(SOURCE_DIR, split, 'labels')
    image_files = defaultdict(set)  # class_id -> set of image files

    for label_file in os.listdir(label_dir):
        if not label_file.endswith('.txt'):
            continue

        label_path = os.path.join(label_dir, label_file)
        with open(label_path, 'r') as f:
            for line in f:
                parts = line.strip().split()
                if len(parts) >= 5:
                    class_id = int(parts[0])
                    if class_id in class_ids:
                        image_name = label_file.replace('.txt', '.jpg')
                        image_files[class_id].add((label_file, image_name))

    return image_files

def copy_files(file_list, src_split, dst_split):
    """파일을 소스에서 목적지로 복사"""
    copied = 0

    for label_file, image_file in file_list:
        # 라벨 복사
        src_label = os.path.join(SOURCE_DIR, src_split, 'labels', label_file)
        dst_label = os.path.join(OUTPUT_DIR, dst_split, 'labels', label_file)
        if os.path.exists(src_label):
            shutil.copy2(src_label, dst_label)

        # 이미지 복사
        src_image = os.path.join(SOURCE_DIR, src_split, 'images', image_file)
        dst_image = os.path.join(OUTPUT_DIR, dst_split, 'images', image_file)
        if os.path.exists(src_image):
            shutil.copy2(src_image, dst_image)
            copied += 1

    return copied

def main():
    print("=" * 80)
    print("ElectroCom61 데이터셋 재분할")
    print("=" * 80)

    # 출력 디렉토리 생성
    for split in ['train', 'valid', 'test']:
        for subdir in ['images', 'labels']:
            os.makedirs(os.path.join(OUTPUT_DIR, split, subdir), exist_ok=True)

    print("\n1️⃣ 기존 데이터 복사 (Inductor/LED 제외)...")

    # 모든 split의 모든 파일 복사 (Inductor/LED 포함 이미지 제외)
    for split in ['train', 'valid', 'test']:
        print(f"\n[{split}]")

        # Inductor/LED를 포함한 이미지 찾기
        rebalance_images = find_images_with_classes(split, REBALANCE_CLASSES.keys())
        excluded_files = set()
        for class_id, files in rebalance_images.items():
            excluded_files.update(files)

        print(f"  제외할 이미지: {len(excluded_files)}개")

        # 나머지 모든 파일 복사
        copied_count = 0
        label_dir = os.path.join(SOURCE_DIR, split, 'labels')

        for label_file in os.listdir(label_dir):
            if not label_file.endswith('.txt'):
                continue

            image_file = label_file.replace('.txt', '.jpg')

            # 제외 목록에 있으면 스킵
            if (label_file, image_file) in excluded_files:
                continue

            # 라벨 복사
            src_label = os.path.join(SOURCE_DIR, split, 'labels', label_file)
            dst_label = os.path.join(OUTPUT_DIR, split, 'labels', label_file)
            shutil.copy2(src_label, dst_label)

            # 이미지 복사
            src_image = os.path.join(SOURCE_DIR, split, 'images', image_file)
            dst_image = os.path.join(OUTPUT_DIR, split, 'images', image_file)
            if os.path.exists(src_image):
                shutil.copy2(src_image, dst_image)
                copied_count += 1

        print(f"  복사 완료: {copied_count}개")

    print("\n2️⃣ Inductor/LED 데이터 재분할...")

    # Train에서 Inductor/LED를 포함한 이미지 찾기
    train_rebalance = find_images_with_classes('train', REBALANCE_CLASSES.keys())

    for class_id, class_name in REBALANCE_CLASSES.items():
        files = list(train_rebalance[class_id])

        if not files:
            print(f"\n⚠️  {class_name}: Train에 데이터 없음")
            continue

        print(f"\n[{class_name} (클래스 {class_id})]")
        print(f"  Train 원본: {len(files)}개")

        # 랜덤 셔플
        random.shuffle(files)

        # 분할 계산
        total = len(files)
        train_size = int(total * SPLIT_RATIOS['train'])
        valid_size = int(total * SPLIT_RATIOS['valid'])

        train_files = files[:train_size]
        valid_files = files[train_size:train_size + valid_size]
        test_files = files[train_size + valid_size:]

        # 파일 복사
        train_copied = copy_files(train_files, 'train', 'train')
        valid_copied = copy_files(valid_files, 'train', 'valid')
        test_copied = copy_files(test_files, 'train', 'test')

        print(f"  → Train: {train_copied}개")
        print(f"  → Valid: {valid_copied}개")
        print(f"  → Test: {test_copied}개")

    # data.yaml 복사
    src_yaml = os.path.join(SOURCE_DIR, 'data.yaml')
    dst_yaml = os.path.join(OUTPUT_DIR, 'data.yaml')

    with open(src_yaml, 'r') as f:
        yaml_content = f.read()

    # 경로 수정
    yaml_content = yaml_content.replace(
        'train: ../train/images',
        f'train: {OUTPUT_DIR}/train/images'
    ).replace(
        'val: ../valid/images',
        f'val: {OUTPUT_DIR}/valid/images'
    ).replace(
        'test: ../test/images',
        f'test: {OUTPUT_DIR}/test/images'
    )

    with open(dst_yaml, 'w') as f:
        f.write(yaml_content)

    print("\n" + "=" * 80)
    print("✅ 재분할 완료!")
    print("=" * 80)

    # 최종 통계
    print("\n📊 최종 데이터 수:")
    for split in ['train', 'valid', 'test']:
        image_count = len(os.listdir(os.path.join(OUTPUT_DIR, split, 'images')))
        label_count = len(os.listdir(os.path.join(OUTPUT_DIR, split, 'labels')))
        print(f"  {split}: {image_count}개 이미지, {label_count}개 라벨")

    print(f"\n📂 출력 디렉토리: {OUTPUT_DIR}")

if __name__ == '__main__':
    main()
