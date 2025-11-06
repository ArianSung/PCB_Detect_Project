#!/usr/bin/env python3
"""
YOLO 데이터셋 라벨 확인 스크립트

사용법:
    python check_labels.py --data data/processed/soldef_ai_yolo/data.yaml
"""

import yaml
import argparse
from pathlib import Path
from collections import Counter


def check_labels(data_yaml_path):
    """데이터셋의 라벨 정보 출력"""

    # YAML 로드
    with open(data_yaml_path, 'r') as f:
        data = yaml.safe_load(f)

    print("=" * 60)
    print("📋 데이터셋 라벨 정보")
    print("=" * 60)
    print(f"\n데이터셋 경로: {data_yaml_path}")
    print(f"클래스 수: {data['nc']}")
    print(f"\n클래스 이름 (총 {len(data['names'])}개):")
    for idx, name in enumerate(data['names']):
        print(f"  {idx}: {name}")

    # 라벨 파일 통계
    dataset_path = Path(data['path'])
    train_labels_path = dataset_path / 'train' / 'labels'

    if train_labels_path.exists():
        print(f"\n📊 Train 라벨 통계:")
        label_files = list(train_labels_path.glob('*.txt'))
        print(f"  - 라벨 파일 수: {len(label_files)}")

        # 클래스별 객체 수 집계
        class_counts = Counter()
        total_objects = 0

        for label_file in label_files:
            with open(label_file, 'r') as f:
                for line in f:
                    if line.strip():
                        class_id = int(line.split()[0])
                        class_counts[class_id] += 1
                        total_objects += 1

        print(f"  - 총 객체 수: {total_objects}")
        print(f"\n  클래스별 객체 분포:")
        for class_id in sorted(class_counts.keys()):
            class_name = data['names'][class_id]
            count = class_counts[class_id]
            percentage = (count / total_objects * 100) if total_objects > 0 else 0
            print(f"    {class_id}: {class_name:15s} - {count:5d} ({percentage:5.2f}%)")

    # Val 라벨 통계
    val_labels_path = dataset_path / 'val' / 'labels'
    if val_labels_path.exists():
        print(f"\n📊 Val 라벨 통계:")
        val_label_files = list(val_labels_path.glob('*.txt'))
        print(f"  - 라벨 파일 수: {len(val_label_files)}")

        val_class_counts = Counter()
        val_total_objects = 0

        for label_file in val_label_files:
            with open(label_file, 'r') as f:
                for line in f:
                    if line.strip():
                        class_id = int(line.split()[0])
                        val_class_counts[class_id] += 1
                        val_total_objects += 1

        print(f"  - 총 객체 수: {val_total_objects}")

    print("\n" + "=" * 60)


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description='YOLO 데이터셋 라벨 확인')
    parser.add_argument('--data', type=str, required=True,
                        help='데이터셋 YAML 파일 경로')

    args = parser.parse_args()
    check_labels(args.data)
