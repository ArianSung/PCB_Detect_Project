#!/usr/bin/env python3
"""
클래스 불균형 해결: 소수 클래스 오버샘플링
가중치를 기반으로 소수 클래스 이미지를 복제하여 데이터셋 균형 조정
"""

import yaml
import shutil
from pathlib import Path
from collections import Counter
import random

def load_class_weights(weights_file):
    """클래스 가중치 파일 로드"""
    with open(weights_file, 'r') as f:
        config = yaml.safe_load(f)
    return config['class_weights']

def analyze_dataset(data_yaml_path):
    """데이터셋 분석"""
    data_yaml_path = Path(data_yaml_path)

    with open(data_yaml_path, 'r') as f:
        data_config = yaml.safe_load(f)

    class_names = data_config.get('names', [])
    train_labels_path = data_yaml_path.parent / 'train' / 'labels'
    train_images_path = data_yaml_path.parent / 'train' / 'images'

    # 이미지별 클래스 분포
    image_classes = {}  # {image_name: [class_ids]}

    for label_file in train_labels_path.glob('*.txt'):
        image_name = label_file.stem
        classes_in_image = set()

        with open(label_file, 'r') as f:
            for line in f:
                parts = line.strip().split()
                if len(parts) >= 5:
                    class_id = int(parts[0])
                    classes_in_image.add(class_id)

        if classes_in_image:
            image_classes[image_name] = list(classes_in_image)

    return class_names, image_classes, train_images_path, train_labels_path

def balance_dataset_by_oversampling(data_yaml_path, class_weights, output_dir, target_balance=2.0):
    """
    클래스 가중치를 기반으로 소수 클래스 오버샘플링

    Args:
        data_yaml_path: data.yaml 경로
        class_weights: {class_id: weight} 딕셔너리
        output_dir: 출력 디렉토리
        target_balance: 목표 균형 비율 (가중치 >= target_balance인 클래스만 복제)
    """
    print("=" * 80)
    print("클래스 불균형 해결: 소수 클래스 오버샘플링")
    print("=" * 80)

    # 데이터셋 분석
    class_names, image_classes, train_images_path, train_labels_path = analyze_dataset(data_yaml_path)

    # 출력 디렉토리 생성
    output_dir = Path(output_dir)
    output_images_dir = output_dir / 'train' / 'images'
    output_labels_dir = output_dir / 'train' / 'labels'
    output_images_dir.mkdir(parents=True, exist_ok=True)
    output_labels_dir.mkdir(parents=True, exist_ok=True)

    print(f"\n출력 디렉토리: {output_dir}")
    print(f"목표 균형 비율: {target_balance}x 이상인 클래스만 복제\n")

    # 클래스별 이미지 찾기
    class_to_images = {i: [] for i in range(len(class_names))}

    for image_name, classes in image_classes.items():
        for class_id in classes:
            class_to_images[class_id].append(image_name)

    # 복제할 클래스 결정
    classes_to_oversample = {}
    for class_id, weight in class_weights.items():
        if weight >= target_balance:
            classes_to_oversample[class_id] = weight

    print("복제할 클래스:")
    for class_id, weight in sorted(classes_to_oversample.items(), key=lambda x: x[1], reverse=True):
        num_images = len(class_to_images[class_id])
        replicas = int(weight)
        print(f"  [{class_id:2d}] {class_names[class_id]:20s}: {weight:.2f}x → {num_images} 이미지 × {replicas}회 복제")

    # 원본 파일 복사
    print(f"\n원본 파일 복사 중...")
    copied_count = 0

    for label_file in train_labels_path.glob('*.txt'):
        image_name = label_file.stem
        image_file = train_images_path / f"{image_name}.jpg"

        if not image_file.exists():
            image_file = train_images_path / f"{image_name}.png"

        if image_file.exists():
            shutil.copy(image_file, output_images_dir / image_file.name)
            shutil.copy(label_file, output_labels_dir / label_file.name)
            copied_count += 1

    print(f"✓ {copied_count:,}개 파일 복사 완료")

    # 소수 클래스 복제
    print(f"\n소수 클래스 이미지 복제 중...")
    replicated_count = 0

    for class_id, weight in classes_to_oversample.items():
        images_with_class = class_to_images[class_id]

        if not images_with_class:
            continue

        # 복제 횟수 계산 (weight 반올림)
        replicas = int(round(weight)) - 1  # 원본 제외

        if replicas <= 0:
            continue

        # 랜덤하게 이미지 선택하여 복제
        for i in range(replicas):
            for image_name in images_with_class:
                # 원본 파일
                image_file = train_images_path / f"{image_name}.jpg"
                if not image_file.exists():
                    image_file = train_images_path / f"{image_name}.png"

                label_file = train_labels_path / f"{image_name}.txt"

                if not image_file.exists() or not label_file.exists():
                    continue

                # 복제 파일명 (중복 방지)
                new_name = f"{image_name}_aug_{class_id}_{i}"
                new_image_file = output_images_dir / f"{new_name}{image_file.suffix}"
                new_label_file = output_labels_dir / f"{new_name}.txt"

                # 복사
                shutil.copy(image_file, new_image_file)
                shutil.copy(label_file, new_label_file)
                replicated_count += 1

    print(f"✓ {replicated_count:,}개 이미지 복제 완료")

    # data.yaml 생성
    data_yaml_out = output_dir / 'data.yaml'
    data_yaml_content = {
        'path': str(output_dir.absolute()),
        'train': 'train/images',
        'val': str((Path(data_yaml_path).parent / 'valid' / 'images').absolute()),
        'test': str((Path(data_yaml_path).parent / 'test' / 'images').absolute()),
        'nc': len(class_names),
        'names': class_names
    }

    with open(data_yaml_out, 'w') as f:
        yaml.dump(data_yaml_content, f, default_flow_style=False, sort_keys=False)

    print(f"\n✓ data.yaml 생성: {data_yaml_out}")

    # 최종 통계
    total_files = len(list(output_images_dir.glob('*.jpg'))) + len(list(output_images_dir.glob('*.png')))

    print("\n" + "=" * 80)
    print("오버샘플링 완료!")
    print("=" * 80)
    print(f"  원본 이미지: {copied_count:,}")
    print(f"  복제 이미지: {replicated_count:,}")
    print(f"  총 이미지: {total_files:,}")
    print(f"\n이제 이 데이터셋으로 학습하세요:")
    print(f"  data={data_yaml_out}")

def main():
    import argparse

    parser = argparse.ArgumentParser(description="Balance dataset by oversampling minority classes")
    parser.add_argument("--data", type=str,
                       default="/home/sys1041/work_project/data/raw/roboflow_pcb/data.yaml",
                       help="Path to original data.yaml")
    parser.add_argument("--weights", type=str,
                       default="/home/sys1041/work_project/configs/class_weights.yaml",
                       help="Path to class weights file")
    parser.add_argument("--output", type=str,
                       default="/home/sys1041/work_project/data/processed/roboflow_pcb_balanced",
                       help="Output directory for balanced dataset")
    parser.add_argument("--target", type=float, default=2.0,
                       help="Target balance ratio (only classes with weight >= target will be replicated)")

    args = parser.parse_args()

    # 클래스 가중치 로드
    class_weights = load_class_weights(args.weights)

    # 오버샘플링 실행
    balance_dataset_by_oversampling(args.data, class_weights, args.output, args.target)

if __name__ == "__main__":
    main()
