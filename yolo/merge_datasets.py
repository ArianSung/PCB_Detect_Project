#!/usr/bin/env python3
"""
DeepPCB와 Kaggle PCB Defects 데이터셋을 통합하는 스크립트

두 데이터셋을 YOLO 형식으로 통합하여 하나의 대규모 데이터셋 생성
"""

import os
import shutil
from pathlib import Path
import argparse
from tqdm import tqdm
import yaml
from collections import Counter


def merge_datasets(deeppcb_dir, kaggle_dir, output_dir, resplit=True):
    """
    DeepPCB와 Kaggle 데이터셋을 통합

    Args:
        deeppcb_dir: DeepPCB YOLO 형식 디렉토리
        kaggle_dir: Kaggle YOLO 형식 디렉토리 (선택적)
        output_dir: 통합 데이터셋 출력 디렉토리
        resplit: 통합 후 재분할 여부 (True면 전체를 다시 train/val/test로 분할)
    """
    deeppcb_path = Path(deeppcb_dir)
    kaggle_path = Path(kaggle_dir) if kaggle_dir else None
    output_path = Path(output_dir)

    # 출력 디렉토리 생성
    for split in ['train', 'val', 'test']:
        (output_path / split / 'images').mkdir(parents=True, exist_ok=True)
        (output_path / split / 'labels').mkdir(parents=True, exist_ok=True)

    print("=== Merging PCB Datasets ===")
    print(f"DeepPCB: {deeppcb_path}")
    if kaggle_path:
        print(f"Kaggle: {kaggle_path}")
    print(f"Output: {output_path}")
    print()

    # 통계
    stats = {
        'deeppcb': {'train': 0, 'val': 0, 'test': 0},
        'kaggle': {'train': 0, 'val': 0, 'test': 0},
        'total': {'train': 0, 'val': 0, 'test': 0},
        'class_distribution': Counter()
    }

    def copy_split(source_dir, split_name, prefix=''):
        """특정 split의 이미지와 라벨 복사"""
        if not source_dir.exists():
            print(f"Warning: {source_dir} does not exist")
            return 0

        images_dir = source_dir / split_name / 'images'
        labels_dir = source_dir / split_name / 'labels'

        if not images_dir.exists() or not labels_dir.exists():
            print(f"Warning: {split_name} split not found in {source_dir}")
            return 0

        count = 0
        for img_file in tqdm(list(images_dir.glob('*')), desc=f"Copying {prefix} {split_name}"):
            # 파일명 충돌 방지를 위해 prefix 추가
            new_name = f"{prefix}_{img_file.name}" if prefix else img_file.name

            # 이미지 복사
            dst_img = output_path / split_name / 'images' / new_name
            shutil.copy(img_file, dst_img)

            # 라벨 복사
            label_file = labels_dir / f"{img_file.stem}.txt"
            if label_file.exists():
                dst_label = output_path / split_name / 'labels' / f"{Path(new_name).stem}.txt"
                shutil.copy(label_file, dst_label)

                # 클래스 분포 통계
                with open(label_file, 'r') as f:
                    for line in f:
                        parts = line.strip().split()
                        if len(parts) >= 5:
                            class_id = int(parts[0])
                            stats['class_distribution'][class_id] += 1

                count += 1

        return count

    # DeepPCB 복사
    print("\n--- Copying DeepPCB ---")
    for split in ['train', 'val', 'test']:
        n = copy_split(deeppcb_path, split, 'deeppcb')
        stats['deeppcb'][split] = n
        stats['total'][split] += n

    # Kaggle 복사 (있는 경우)
    if kaggle_path and kaggle_path.exists():
        print("\n--- Copying Kaggle ---")
        for split in ['train', 'val', 'test']:
            n = copy_split(kaggle_path, split, 'kaggle')
            stats['kaggle'][split] = n
            stats['total'][split] += n

    # 통계 출력
    print("\n=== Merge Statistics ===")
    print(f"\nDeepPCB:")
    print(f"  Train: {stats['deeppcb']['train']}")
    print(f"  Val: {stats['deeppcb']['val']}")
    print(f"  Test: {stats['deeppcb']['test']}")
    print(f"  Total: {sum(stats['deeppcb'].values())}")

    if kaggle_path and kaggle_path.exists():
        print(f"\nKaggle:")
        print(f"  Train: {stats['kaggle']['train']}")
        print(f"  Val: {stats['kaggle']['val']}")
        print(f"  Test: {stats['kaggle']['test']}")
        print(f"  Total: {sum(stats['kaggle'].values())}")

    print(f"\nCombined Total:")
    print(f"  Train: {stats['total']['train']}")
    print(f"  Val: {stats['total']['val']}")
    print(f"  Test: {stats['total']['test']}")
    print(f"  Total: {sum(stats['total'].values())}")

    # 클래스 분포
    class_names = ['open', 'short', 'mousebite', 'spur', 'copper', 'pin-hole']
    print(f"\n=== Class Distribution ===")
    total_objects = sum(stats['class_distribution'].values())
    for class_id in range(6):
        count = stats['class_distribution'][class_id]
        percentage = (count / total_objects * 100) if total_objects > 0 else 0
        print(f"  {class_id} ({class_names[class_id]}): {count} ({percentage:.1f}%)")
    print(f"  Total objects: {total_objects}")

    # 클래스 불균형 확인
    if total_objects > 0:
        max_count = max(stats['class_distribution'].values())
        min_count = min(stats['class_distribution'].values())
        imbalance_ratio = max_count / min_count if min_count > 0 else float('inf')
        print(f"\n  Imbalance ratio: {imbalance_ratio:.2f}:1")
        if imbalance_ratio > 10:
            print(f"  ⚠️  Warning: High class imbalance detected (>{imbalance_ratio:.1f}:1)")
            print(f"     Consider using class weights or oversampling")

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

    # 데이터셋 요약 저장
    summary_path = output_path / 'dataset_summary.txt'
    with open(summary_path, 'w') as f:
        f.write("=== Combined PCB Dataset Summary ===\n\n")
        f.write(f"Total images: {sum(stats['total'].values())}\n")
        f.write(f"  Train: {stats['total']['train']}\n")
        f.write(f"  Val: {stats['total']['val']}\n")
        f.write(f"  Test: {stats['total']['test']}\n\n")

        f.write("Source datasets:\n")
        f.write(f"  DeepPCB: {sum(stats['deeppcb'].values())} images\n")
        if kaggle_path and kaggle_path.exists():
            f.write(f"  Kaggle: {sum(stats['kaggle'].values())} images\n")
        f.write("\n")

        f.write("Class distribution:\n")
        for class_id in range(6):
            count = stats['class_distribution'][class_id]
            percentage = (count / total_objects * 100) if total_objects > 0 else 0
            f.write(f"  {class_id} ({class_names[class_id]}): {count} ({percentage:.1f}%)\n")

    print(f"Summary saved to: {summary_path}")

    print("\n=== Merge Complete ===")
    print(f"\nYou can now train YOLO with:")
    print(f"  yolo detect train data={yaml_path} model=yolov8l.pt epochs=150 batch=32 imgsz=640")

    return stats


def main():
    parser = argparse.ArgumentParser(description='Merge DeepPCB and Kaggle PCB datasets')
    parser.add_argument('--deeppcb-dir', type=str,
                       default='/home/sys1041/work_project/data/processed/deeppcb_yolo',
                       help='DeepPCB YOLO format directory')
    parser.add_argument('--kaggle-dir', type=str,
                       default=None,
                       help='Kaggle YOLO format directory (optional)')
    parser.add_argument('--output-dir', type=str,
                       default='/home/sys1041/work_project/data/processed/combined_pcb_dataset',
                       help='Output directory for merged dataset')
    parser.add_argument('--resplit', action='store_true',
                       help='Re-split the combined dataset')

    args = parser.parse_args()

    # Kaggle 디렉토리 확인
    if args.kaggle_dir and not Path(args.kaggle_dir).exists():
        print(f"Warning: Kaggle directory not found: {args.kaggle_dir}")
        print("Proceeding with DeepPCB only...")
        args.kaggle_dir = None

    merge_datasets(
        args.deeppcb_dir,
        args.kaggle_dir,
        args.output_dir,
        args.resplit
    )


if __name__ == '__main__':
    main()
