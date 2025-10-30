#!/usr/bin/env python3
"""
Kaggle PCB Defects 데이터셋(Pascal VOC XML)을 YOLO 형식으로 변환하는 스크립트

데이터셋 구조:
- images/[클래스명]/[이미지.jpg]
- Annotations/[클래스명]/[이미지.xml]

Pascal VOC XML → YOLO 형식 변환
"""

import os
import shutil
import xml.etree.ElementTree as ET
from pathlib import Path
from PIL import Image
import argparse
from tqdm import tqdm
import yaml


def convert_kaggle_to_yolo(kaggle_root, output_dir, train_ratio=0.7, val_ratio=0.2, test_ratio=0.1):
    """
    Kaggle PCB Defects (Pascal VOC XML)를 YOLO 형식으로 변환

    Args:
        kaggle_root: Kaggle 데이터셋 루트 경로
        output_dir: 출력 디렉토리
        train_ratio: 학습 데이터 비율
        val_ratio: 검증 데이터 비율
        test_ratio: 테스트 데이터 비율
    """
    kaggle_path = Path(kaggle_root) / 'PCB_DATASET'
    if not kaggle_path.exists():
        raise FileNotFoundError(f"PCB_DATASET not found: {kaggle_path}")

    images_path = kaggle_path / 'images'
    annotations_path = kaggle_path / 'Annotations'

    if not images_path.exists() or not annotations_path.exists():
        raise FileNotFoundError(f"images or Annotations folder not found in {kaggle_path}")

    output_path = Path(output_dir)

    # 출력 디렉토리 생성
    for split in ['train', 'val', 'test']:
        (output_path / split / 'images').mkdir(parents=True, exist_ok=True)
        (output_path / split / 'labels').mkdir(parents=True, exist_ok=True)

    # 클래스 매핑 (DeepPCB와 동일한 순서)
    class_map = {
        'open_circuit': 0,      # open
        'missing_hole': 0,      # open과 유사
        'short': 1,             # short
        'mouse_bite': 2,        # mousebite
        'spur': 3,              # spur
        'spurious_copper': 4,   # copper
        'copper': 4,            # copper
    }

    print(f"=== Kaggle PCB Defects → YOLO Conversion ===")
    print(f"Source: {kaggle_path}")
    print(f"Output: {output_path}")
    print()

    # 모든 클래스 폴더에서 XML 파일 수집
    all_xml_files = []
    class_folders = [d for d in annotations_path.iterdir() if d.is_dir()]

    print(f"Found {len(class_folders)} class folders:")
    for class_folder in class_folders:
        xml_files = list(class_folder.glob('*.xml'))
        all_xml_files.extend(xml_files)
        print(f"  - {class_folder.name}: {len(xml_files)} images")

    print(f"\nTotal images: {len(all_xml_files)}")

    if len(all_xml_files) == 0:
        raise ValueError("No XML files found")

    # 데이터 분할
    import random
    random.seed(42)
    random.shuffle(all_xml_files)

    n_train = int(len(all_xml_files) * train_ratio)
    n_val = int(len(all_xml_files) * val_ratio)

    train_files = all_xml_files[:n_train]
    val_files = all_xml_files[n_train:n_train + n_val]
    test_files = all_xml_files[n_train + n_val:]

    print(f"\nSplit: Train={len(train_files)}, Val={len(val_files)}, Test={len(test_files)}")

    # 변환 및 복사
    def process_split(xml_list, split_name):
        """특정 split의 이미지 처리"""
        stats = {'total': 0, 'converted': 0, 'errors': 0, 'skipped': 0}

        for xml_path in tqdm(xml_list, desc=f"Processing {split_name}"):
            stats['total'] += 1

            try:
                # XML 파싱
                tree = ET.parse(xml_path)
                root = tree.getroot()

                # 파일명 및 크기 가져오기
                filename = root.find('filename').text
                size = root.find('size')
                img_width = int(size.find('width').text)
                img_height = int(size.find('height').text)

                # 이미지 파일 경로
                class_folder = xml_path.parent.name
                img_path = images_path / class_folder / filename

                if not img_path.exists():
                    print(f"Warning: Image not found: {img_path}")
                    stats['skipped'] += 1
                    continue

                # 모든 객체 처리
                yolo_annotations = []
                objects = root.findall('object')

                for obj in objects:
                    # 클래스명 정규화
                    class_name = obj.find('name').text.lower().strip()
                    class_name = class_name.replace(' ', '_').replace('-', '_')

                    if class_name not in class_map:
                        print(f"Warning: Unknown class '{class_name}' in {xml_path}")
                        continue

                    yolo_class_id = class_map[class_name]

                    # Bounding box 좌표
                    bndbox = obj.find('bndbox')
                    xmin = float(bndbox.find('xmin').text)
                    ymin = float(bndbox.find('ymin').text)
                    xmax = float(bndbox.find('xmax').text)
                    ymax = float(bndbox.find('ymax').text)

                    # YOLO 형식으로 변환 (정규화)
                    x_center = ((xmin + xmax) / 2) / img_width
                    y_center = ((ymin + ymax) / 2) / img_height
                    width = (xmax - xmin) / img_width
                    height = (ymax - ymin) / img_height

                    # 좌표 검증 (0~1 범위)
                    if not (0 <= x_center <= 1 and 0 <= y_center <= 1 and
                           0 <= width <= 1 and 0 <= height <= 1):
                        print(f"Warning: Invalid coordinates in {xml_path}")
                        continue

                    yolo_annotations.append(f"{yolo_class_id} {x_center:.6f} {y_center:.6f} {width:.6f} {height:.6f}")

                if len(yolo_annotations) == 0:
                    print(f"Warning: No valid annotations in {xml_path}")
                    stats['skipped'] += 1
                    continue

                # 이미지 복사
                dst_img = output_path / split_name / 'images' / filename
                shutil.copy(img_path, dst_img)

                # YOLO 라벨 저장
                dst_label = output_path / split_name / 'labels' / f"{Path(filename).stem}.txt"
                with open(dst_label, 'w') as f:
                    f.write('\n'.join(yolo_annotations))

                stats['converted'] += 1

            except Exception as e:
                print(f"Error processing {xml_path}: {e}")
                stats['errors'] += 1

        return stats

    # 각 split 처리
    print()
    train_stats = process_split(train_files, 'train')
    val_stats = process_split(val_files, 'val')
    test_stats = process_split(test_files, 'test')

    # 통계 출력
    print("\n=== Conversion Statistics ===")
    print(f"Train: {train_stats['converted']}/{train_stats['total']} images " +
          f"(errors: {train_stats['errors']}, skipped: {train_stats['skipped']})")
    print(f"Val: {val_stats['converted']}/{val_stats['total']} images " +
          f"(errors: {val_stats['errors']}, skipped: {val_stats['skipped']})")
    print(f"Test: {test_stats['converted']}/{test_stats['total']} images " +
          f"(errors: {test_stats['errors']}, skipped: {test_stats['skipped']})")
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


def main():
    parser = argparse.ArgumentParser(description='Convert Kaggle PCB Defects (Pascal VOC XML) to YOLO format')
    parser.add_argument('--kaggle-root', type=str,
                       default='/home/sys1041/work_project/data/raw/kaggle_pcb_defects',
                       help='Kaggle dataset root directory')
    parser.add_argument('--output-dir', type=str,
                       default='/home/sys1041/work_project/data/processed/kaggle_yolo',
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

    convert_kaggle_to_yolo(
        args.kaggle_root,
        args.output_dir,
        args.train_ratio,
        args.val_ratio,
        args.test_ratio
    )


if __name__ == '__main__':
    main()
