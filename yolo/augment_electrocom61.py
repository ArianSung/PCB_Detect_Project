#!/usr/bin/env python3
"""
ElectroCom61 Through-hole 데이터 증강 스크립트

목적:
- 객체 수가 적은 Through-hole 클래스를 증강하여 데이터 균형 맞추기
- 목표: 각 Through-hole 클래스당 최소 200-300개 객체

증강 기법:
- Horizontal Flip
- Vertical Flip
- Rotation (90도 단위)
- Brightness/Contrast 조정
"""

import os
import cv2
import random
import shutil
import albumentations as A
from pathlib import Path
from collections import Counter

# 경로 설정
SOURCE_DIR = "/home/sys1041/work_project/data/raw/ElectroCom61_rebalanced"
OUTPUT_DIR = "/home/sys1041/work_project/data/raw/ElectroCom61_augmented"

# Through-hole 클래스 매핑 (ElectroCom61 원본 ID)
TH_CLASSES = {
    50: 'Resistor',
    20: 'Film-Capacitor',
    13: 'Capacitor-10mf',
    14: 'Capacitor-470mf',
    37: 'LED-Light',
    16: 'Diode',
    60: 'Zener-Diode',
    7: 'BJT-Transistor',
    33: 'Inductor',
    21: 'Fuse'
}

# 증강 목표 (클래스당 최소 객체 수)
TARGET_MIN_OBJECTS = 200

random.seed(42)

# 증강 변환 정의
augmentations = [
    A.Compose([
        A.HorizontalFlip(p=1.0)
    ], bbox_params=A.BboxParams(format='yolo', label_fields=['class_labels'])),

    A.Compose([
        A.VerticalFlip(p=1.0)
    ], bbox_params=A.BboxParams(format='yolo', label_fields=['class_labels'])),

    A.Compose([
        A.Rotate(limit=[90, 90], p=1.0)
    ], bbox_params=A.BboxParams(format='yolo', label_fields=['class_labels'])),

    A.Compose([
        A.Rotate(limit=[180, 180], p=1.0)
    ], bbox_params=A.BboxParams(format='yolo', label_fields=['class_labels'])),

    A.Compose([
        A.Rotate(limit=[270, 270], p=1.0)
    ], bbox_params=A.BboxParams(format='yolo', label_fields=['class_labels'])),

    A.Compose([
        A.RandomBrightnessContrast(brightness_limit=0.2, contrast_limit=0.2, p=1.0)
    ], bbox_params=A.BboxParams(format='yolo', label_fields=['class_labels'])),
]

def count_class_objects(split):
    """Split별 클래스별 객체 수 카운트"""
    label_dir = os.path.join(SOURCE_DIR, split, 'labels')
    class_counts = Counter()

    for label_file in os.listdir(label_dir):
        if not label_file.endswith('.txt'):
            continue

        with open(os.path.join(label_dir, label_file), 'r') as f:
            for line in f:
                parts = line.strip().split()
                if len(parts) >= 5:
                    class_id = int(parts[0])
                    if class_id in TH_CLASSES:
                        class_counts[class_id] += 1

    return class_counts

def read_yolo_labels(label_path):
    """YOLO 라벨 읽기"""
    bboxes = []
    class_labels = []

    with open(label_path, 'r') as f:
        for line in f:
            parts = line.strip().split()
            if len(parts) >= 5:
                class_id = int(parts[0])
                x, y, w, h = map(float, parts[1:5])
                bboxes.append([x, y, w, h])
                class_labels.append(class_id)

    return bboxes, class_labels

def write_yolo_labels(label_path, bboxes, class_labels):
    """YOLO 라벨 쓰기"""
    with open(label_path, 'w') as f:
        for bbox, class_id in zip(bboxes, class_labels):
            x, y, w, h = bbox
            f.write(f"{class_id} {x:.6f} {y:.6f} {w:.6f} {h:.6f}\n")

def augment_image(image_path, label_path, output_image_path, output_label_path, aug_transform):
    """이미지 증강 및 저장"""
    # 이미지 읽기
    image = cv2.imread(image_path)
    if image is None:
        return False

    image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)

    # 라벨 읽기
    bboxes, class_labels = read_yolo_labels(label_path)

    if not bboxes:
        return False

    try:
        # 증강 적용
        transformed = aug_transform(image=image, bboxes=bboxes, class_labels=class_labels)

        # 결과 저장
        aug_image = cv2.cvtColor(transformed['image'], cv2.COLOR_RGB2BGR)
        cv2.imwrite(output_image_path, aug_image)

        # 라벨 저장
        write_yolo_labels(output_label_path, transformed['bboxes'], transformed['class_labels'])

        return True
    except Exception as e:
        print(f"  ⚠️ 증강 실패: {os.path.basename(image_path)} - {e}")
        return False

def main():
    print("=" * 80)
    print("ElectroCom61 Through-hole 데이터 증강")
    print("=" * 80)

    # 출력 디렉토리 생성
    for split in ['train', 'valid', 'test']:
        for subdir in ['images', 'labels']:
            os.makedirs(os.path.join(OUTPUT_DIR, split, subdir), exist_ok=True)

    print("\n1️⃣ 기존 데이터 복사...")

    for split in ['train', 'valid', 'test']:
        src_img_dir = os.path.join(SOURCE_DIR, split, 'images')
        src_lbl_dir = os.path.join(SOURCE_DIR, split, 'labels')
        dst_img_dir = os.path.join(OUTPUT_DIR, split, 'images')
        dst_lbl_dir = os.path.join(OUTPUT_DIR, split, 'labels')

        for img_file in os.listdir(src_img_dir):
            shutil.copy2(os.path.join(src_img_dir, img_file), os.path.join(dst_img_dir, img_file))

        for lbl_file in os.listdir(src_lbl_dir):
            shutil.copy2(os.path.join(src_lbl_dir, lbl_file), os.path.join(dst_lbl_dir, lbl_file))

    print("  ✅ 기존 데이터 복사 완료")

    print("\n2️⃣ Train 세트 클래스 분석...")

    train_counts = count_class_objects('train')

    print(f"\n{'클래스 ID':<12} {'클래스 이름':<20} {'현재 객체':>12} {'목표':>10} {'증강 필요':>10}")
    print("-" * 80)

    classes_to_augment = {}

    for class_id, class_name in sorted(TH_CLASSES.items()):
        current_count = train_counts.get(class_id, 0)
        need_augment = max(0, TARGET_MIN_OBJECTS - current_count)

        status = "✅" if current_count >= TARGET_MIN_OBJECTS else f"⚠️ +{need_augment}"
        print(f"{class_id:<12} {class_name:<20} {current_count:>12} {TARGET_MIN_OBJECTS:>10} {status:>10}")

        if need_augment > 0:
            classes_to_augment[class_id] = {
                'name': class_name,
                'current': current_count,
                'needed': need_augment
            }

    if not classes_to_augment:
        print("\n✅ 모든 클래스가 목표 객체 수를 만족합니다!")
        return

    print(f"\n3️⃣ 증강 시작 (Train 세트, {len(classes_to_augment)}개 클래스)...")

    # Train 세트 증강
    train_img_dir = os.path.join(SOURCE_DIR, 'train', 'images')
    train_lbl_dir = os.path.join(SOURCE_DIR, 'train', 'labels')
    aug_img_dir = os.path.join(OUTPUT_DIR, 'train', 'images')
    aug_lbl_dir = os.path.join(OUTPUT_DIR, 'train', 'labels')

    for class_id, info in classes_to_augment.items():
        print(f"\n[{info['name']} (클래스 {class_id})]")
        print(f"  현재: {info['current']}개, 목표: {TARGET_MIN_OBJECTS}개, 증강 필요: {info['needed']}개")

        # 해당 클래스를 포함한 이미지 찾기
        candidate_files = []

        for label_file in os.listdir(train_lbl_dir):
            if not label_file.endswith('.txt'):
                continue

            with open(os.path.join(train_lbl_dir, label_file), 'r') as f:
                has_class = any(
                    int(line.split()[0]) == class_id
                    for line in f if line.strip() and len(line.split()) >= 5
                )

            if has_class:
                image_file = label_file.replace('.txt', '.jpg')
                candidate_files.append((image_file, label_file))

        if not candidate_files:
            print(f"  ⚠️  해당 클래스를 포함한 이미지 없음")
            continue

        print(f"  후보 이미지: {len(candidate_files)}개")

        # 증강 실행
        augmented_count = 0
        target_augment = info['needed']

        # 필요한 만큼 증강
        aug_idx = 0
        while augmented_count < target_augment:
            # 후보 이미지에서 랜덤 선택
            image_file, label_file = random.choice(candidate_files)

            # 랜덤 증강 기법 선택
            aug_transform = random.choice(augmentations)

            # 파일 경로
            src_image_path = os.path.join(train_img_dir, image_file)
            src_label_path = os.path.join(train_lbl_dir, label_file)

            # 증강된 파일 이름
            base_name = image_file.rsplit('.', 1)[0]
            aug_image_file = f"{base_name}_aug{aug_idx}.jpg"
            aug_label_file = f"{base_name}_aug{aug_idx}.txt"

            dst_image_path = os.path.join(aug_img_dir, aug_image_file)
            dst_label_path = os.path.join(aug_lbl_dir, aug_label_file)

            # 증강 실행
            if augment_image(src_image_path, src_label_path, dst_image_path, dst_label_path, aug_transform):
                augmented_count += 1
                aug_idx += 1
            else:
                aug_idx += 1  # 실패해도 인덱스는 증가

        print(f"  ✅ 증강 완료: {augmented_count}개 추가")

    # data.yaml 복사
    src_yaml = os.path.join(SOURCE_DIR, 'data.yaml')
    dst_yaml = os.path.join(OUTPUT_DIR, 'data.yaml')

    with open(src_yaml, 'r') as f:
        yaml_content = f.read()

    # 경로 수정
    yaml_content = yaml_content.replace(
        SOURCE_DIR,
        OUTPUT_DIR
    )

    with open(dst_yaml, 'w') as f:
        f.write(yaml_content)

    print("\n" + "=" * 80)
    print("✅ 증강 완료!")
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
