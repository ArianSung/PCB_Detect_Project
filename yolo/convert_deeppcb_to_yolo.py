#!/usr/bin/env python3
"""
DeepPCB Dataset을 YOLO 형식으로 변환하는 스크립트

DeepPCB 형식: x1 y1 x2 y2 class_id (absolute coordinates)
YOLO 형식: class_id x_center y_center width height (normalized 0~1)

DeepPCB 클래스:
  1: open
  2: short
  3: mousebite
  4: spur
  5: copper
  6: pin-hole
"""

import os
import shutil
from pathlib import Path
from PIL import Image
import argparse
from tqdm import tqdm
import yaml


def convert_deeppcb_to_yolo(deeppcb_root, output_dir, train_ratio=0.7, val_ratio=0.2, test_ratio=0.1):
    """
    DeepPCB 데이터셋을 YOLO 형식으로 변환

    Args:
        deeppcb_root: DeepPCB 데이터셋 루트 경로 (PCBData 포함)
        output_dir: 출력 디렉토리
        train_ratio: 학습 데이터 비율
        val_ratio: 검증 데이터 비율
        test_ratio: 테스트 데이터 비율
    """
    pcbdata_path = Path(deeppcb_root) / 'PCBData'
    if not pcbdata_path.exists():
        raise FileNotFoundError(f"PCBData not found: {pcbdata_path}")

    output_path = Path(output_dir)

    # 출력 디렉토리 생성
    for split in ['train', 'val', 'test']:
        (output_path / split / 'images').mkdir(parents=True, exist_ok=True)
        (output_path / split / 'labels').mkdir(parents=True, exist_ok=True)

    # DeepPCB 클래스 매핑 (1-based → 0-based)
    class_map = {
        1: 0,  # open
        2: 1,  # short
        3: 2,  # mousebite
        4: 3,  # spur
        5: 4,  # copper
        6: 5,  # pin-hole
    }

    # 모든 그룹 폴더 찾기
    group_dirs = sorted([d for d in pcbdata_path.iterdir() if d.is_dir() and d.name.startswith('group')])

    print(f"Found {len(group_dirs)} group folders")

    # 모든 이미지 수집
    all_images = []
    for group_dir in group_dirs:
        group_num = group_dir.name.replace('group', '')

        # _not 폴더 (annotation 파일)
        annotation_dir = group_dir / f"{group_num}_not"
        # 이미지 폴더 (template/test 이미지)
        image_dir = group_dir / group_num

        if not annotation_dir.exists() or not image_dir.exists():
            continue

        # annotation 파일 찾기
        for txt_file in annotation_dir.glob('*.txt'):
            # 대응하는 test 이미지 찾기
            img_file = image_dir / f"{txt_file.stem}_test.jpg"
            if img_file.exists():
                all_images.append((img_file, txt_file))

    print(f"Found {len(all_images)} image-label pairs")

    if len(all_images) == 0:
        raise ValueError("No image-label pairs found")

    # 데이터 분할
    import random
    random.seed(42)
    random.shuffle(all_images)

    n_train = int(len(all_images) * train_ratio)
    n_val = int(len(all_images) * val_ratio)

    train_images = all_images[:n_train]
    val_images = all_images[n_train:n_train + n_val]
    test_images = all_images[n_train + n_val:]

    print(f"Split: Train={len(train_images)}, Val={len(val_images)}, Test={len(test_images)}")

    # 변환 및 복사
    def process_split(image_list, split_name):
        """특정 split의 이미지 처리"""
        stats = {'total': 0, 'converted': 0, 'errors': 0}

        for img_path, txt_path in tqdm(image_list, desc=f"Processing {split_name}"):
            stats['total'] += 1

            try:
                # 이미지 크기 가져오기
                with Image.open(img_path) as img:
                    img_width, img_height = img.size

                # 어노테이션 읽기 및 변환
                yolo_annotations = []
                with open(txt_path, 'r') as f:
                    for line in f:
                        line = line.strip()
                        if not line:
                            continue

                        parts = line.split()
                        if len(parts) < 5:
                            continue

                        x1, y1, x2, y2, class_id = map(int, parts[:5])

                        # DeepPCB 클래스 ID (1-6) → YOLO 클래스 ID (0-5)
                        if class_id not in class_map:
                            print(f"Warning: Unknown class {class_id} in {txt_path}")
                            continue

                        yolo_class_id = class_map[class_id]

                        # Absolute → Normalized YOLO format
                        x_center = ((x1 + x2) / 2) / img_width
                        y_center = ((y1 + y2) / 2) / img_height
                        width = (x2 - x1) / img_width
                        height = (y2 - y1) / img_height

                        # 좌표 검증 (0~1 범위)
                        if not (0 <= x_center <= 1 and 0 <= y_center <= 1 and
                               0 <= width <= 1 and 0 <= height <= 1):
                            print(f"Warning: Invalid coordinates in {txt_path}: {line}")
                            continue

                        yolo_annotations.append(f"{yolo_class_id} {x_center:.6f} {y_center:.6f} {width:.6f} {height:.6f}")

                if len(yolo_annotations) == 0:
                    print(f"Warning: No valid annotations in {txt_path}")
                    continue

                # 이미지 복사
                dst_img = output_path / split_name / 'images' / img_path.name
                shutil.copy(img_path, dst_img)

                # YOLO 라벨 저장
                dst_label = output_path / split_name / 'labels' / f"{img_path.stem}.txt"
                with open(dst_label, 'w') as f:
                    f.write('\n'.join(yolo_annotations))

                stats['converted'] += 1

            except Exception as e:
                print(f"Error processing {img_path}: {e}")
                stats['errors'] += 1

        return stats

    # 각 split 처리
    train_stats = process_split(train_images, 'train')
    val_stats = process_split(val_images, 'val')
    test_stats = process_split(test_images, 'test')

    # 통계 출력
    print("\n=== Conversion Statistics ===")
    print(f"Train: {train_stats['converted']}/{train_stats['total']} images (errors: {train_stats['errors']})")
    print(f"Val: {val_stats['converted']}/{val_stats['total']} images (errors: {val_stats['errors']})")
    print(f"Test: {test_stats['converted']}/{test_stats['total']} images (errors: {test_stats['errors']})")
    print(f"\nTotal converted: {train_stats['converted'] + val_stats['converted'] + test_stats['converted']}")

    # YAML 파일 생성
    yaml_content = {
        'path': str(output_path.absolute()),
        'train': 'train/images',
        'val': 'val/images',
        'test': 'test/images',
        'nc': 6,
        'names': {
            0: 'open',
            1: 'short',
            2: 'mousebite',
            3: 'spur',
            4: 'copper',
            5: 'pin-hole'
        }
    }

    yaml_path = output_path / 'data.yaml'
    with open(yaml_path, 'w') as f:
        yaml.dump(yaml_content, f, default_flow_style=False, sort_keys=False)

    print(f"\nYAML config saved to: {yaml_path}")
    print("\n=== Conversion Complete ===")
    print(f"Output directory: {output_path}")
    print(f"\nYou can now train YOLO with:")
    print(f"  yolo detect train data={yaml_path} model=yolov8l.pt epochs=100 imgsz=640")


def main():
    parser = argparse.ArgumentParser(description='Convert DeepPCB to YOLO format')
    parser.add_argument('--deeppcb-root', type=str,
                       default='/home/sys1041/work_project/data/raw/DeepPCB',
                       help='DeepPCB dataset root directory')
    parser.add_argument('--output-dir', type=str,
                       default='/home/sys1041/work_project/data/processed/deeppcb_yolo',
                       help='Output directory for YOLO format dataset')
    parser.add_argument('--train-ratio', type=float, default=0.7,
                       help='Training data ratio')
    parser.add_argument('--val-ratio', type=float, default=0.2,
                       help='Validation data ratio')
    parser.add_argument('--test-ratio', type=float, default=0.1,
                       help='Test data ratio')

    args = parser.parse_args()

    # 비율 검증
    if abs(args.train_ratio + args.val_ratio + args.test_ratio - 1.0) > 0.01:
        raise ValueError("train_ratio + val_ratio + test_ratio must equal 1.0")

    convert_deeppcb_to_yolo(
        args.deeppcb_root,
        args.output_dir,
        args.train_ratio,
        args.val_ratio,
        args.test_ratio
    )


if __name__ == '__main__':
    main()
