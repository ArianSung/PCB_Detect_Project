"""
SolDef_AI LabelMe Annotations → YOLO Bounding Box 형식 변환

SolDef_AI 데이터셋의 LabelMe polygon annotations를
YOLO object detection 형식으로 변환합니다.

Input:  data/raw/SolDef_AI/Labeled/ (JSON annotations)
        data/raw/SolDef_AI/Dataset/ (images)
Output: data/processed/soldef_ai_yolo/
"""

import json
import os
import shutil
from pathlib import Path
from tqdm import tqdm
import yaml
import numpy as np
from sklearn.model_selection import train_test_split

# 5개 클래스 정의 (SolDef_AI 납땜 불량 유형)
CLASS_NAMES = [
    'exc_solder',    # 0: 과다 납땜 (Excessive solder)
    'good',          # 1: 정상 (Good solder joint)
    'no_good',       # 2: 불량 (Defective solder joint)
    'poor_solder',   # 3: 부족 납땜 (Insufficient solder)
    'spike'          # 4: 스파이크 (Solder spike)
]

def polygon_to_bbox(points):
    """
    LabelMe polygon points를 YOLO bounding box로 변환

    Args:
        points: List of [x, y] coordinates

    Returns:
        (x_center, y_center, width, height) normalized to [0, 1]
    """
    points = np.array(points)

    x_min = points[:, 0].min()
    x_max = points[:, 0].max()
    y_min = points[:, 1].min()
    y_max = points[:, 1].max()

    return x_min, y_min, x_max, y_max

def find_image_file(image_name, dataset_dir):
    """
    이미지 파일을 Dataset 디렉토리에서 찾기

    Args:
        image_name: 이미지 파일명 (확장자 포함)
        dataset_dir: Dataset 루트 디렉토리

    Returns:
        이미지 파일의 전체 경로 또는 None
    """
    # Dataset 디렉토리에서 재귀적으로 이미지 찾기
    for root, dirs, files in os.walk(dataset_dir):
        if image_name in files:
            return os.path.join(root, image_name)

    return None

def convert_json_to_yolo(json_path, dataset_dir):
    """
    LabelMe JSON 파일을 YOLO 형식으로 변환

    Args:
        json_path: JSON 파일 경로
        dataset_dir: 이미지가 있는 Dataset 디렉토리

    Returns:
        (image_path, yolo_annotations) 또는 None
    """
    with open(json_path, 'r') as f:
        data = json.load(f)

    # 이미지 경로 찾기
    image_name = data['imagePath']
    image_path = find_image_file(image_name, dataset_dir)

    if image_path is None:
        return None

    # 이미지 크기
    img_width = data['imageWidth']
    img_height = data['imageHeight']

    # YOLO 어노테이션 생성
    yolo_annotations = []

    for shape in data.get('shapes', []):
        label = shape['label']

        if label not in CLASS_NAMES:
            print(f"Warning: Unknown label '{label}' in {json_path}")
            continue

        class_id = CLASS_NAMES.index(label)
        points = shape['points']

        # Polygon을 bounding box로 변환
        x_min, y_min, x_max, y_max = polygon_to_bbox(points)

        # YOLO 형식으로 변환 (normalized)
        x_center = ((x_min + x_max) / 2) / img_width
        y_center = ((y_min + y_max) / 2) / img_height
        width = (x_max - x_min) / img_width
        height = (y_max - y_min) / img_height

        # 유효성 검사
        if width <= 0 or height <= 0:
            print(f"Warning: Invalid bbox in {json_path}")
            continue

        yolo_annotations.append([class_id, x_center, y_center, width, height])

    return image_path, yolo_annotations

def create_yolo_dataset(labeled_dir, dataset_dir, output_dir, train_ratio=0.7, val_ratio=0.2):
    """
    전체 데이터셋을 YOLO 형식으로 변환하고 train/val/test 분할

    Args:
        labeled_dir: JSON 어노테이션 디렉토리
        dataset_dir: 이미지 데이터셋 디렉토리
        output_dir: 출력 디렉토리
        train_ratio: 학습 데이터 비율
        val_ratio: 검증 데이터 비율
    """
    output_path = Path(output_dir)

    # 출력 디렉토리 생성
    for split in ['train', 'val', 'test']:
        (output_path / split / 'images').mkdir(parents=True, exist_ok=True)
        (output_path / split / 'labels').mkdir(parents=True, exist_ok=True)

    # 모든 JSON 파일 처리
    json_files = list(Path(labeled_dir).glob('*.json'))

    print(f"\n변환 중: {len(json_files)} 개의 JSON 파일")

    valid_data = []

    for json_file in tqdm(json_files):
        result = convert_json_to_yolo(json_file, dataset_dir)

        if result is None:
            continue

        image_path, yolo_annotations = result

        if len(yolo_annotations) == 0:
            continue

        valid_data.append({
            'image_path': image_path,
            'annotations': yolo_annotations,
            'json_file': json_file.stem
        })

    print(f"✅ 유효한 데이터: {len(valid_data)} 개")

    # Train/Val/Test 분할
    test_ratio = 1.0 - train_ratio - val_ratio

    # 먼저 train+val과 test 분리
    train_val_data, test_data = train_test_split(
        valid_data,
        test_size=test_ratio,
        random_state=42
    )

    # train과 val 분리
    val_size = val_ratio / (train_ratio + val_ratio)
    train_data, val_data = train_test_split(
        train_val_data,
        test_size=val_size,
        random_state=42
    )

    print(f"\n데이터 분할:")
    print(f"  - Train: {len(train_data)} 이미지")
    print(f"  - Val:   {len(val_data)} 이미지")
    print(f"  - Test:  {len(test_data)} 이미지")

    # 각 split에 데이터 복사 및 라벨 생성
    splits = {
        'train': train_data,
        'val': val_data,
        'test': test_data
    }

    for split_name, split_data in splits.items():
        print(f"\n{split_name} split 처리 중...")

        for idx, item in enumerate(tqdm(split_data)):
            # 이미지 복사
            image_name = f"{item['json_file']}.jpg"
            dest_image = output_path / split_name / 'images' / image_name
            shutil.copy2(item['image_path'], dest_image)

            # YOLO 라벨 파일 생성
            label_file = output_path / split_name / 'labels' / f"{item['json_file']}.txt"

            with open(label_file, 'w') as f:
                for bbox in item['annotations']:
                    # YOLO 형식: class_id x_center y_center width height
                    f.write(f"{int(bbox[0])} {bbox[1]:.6f} {bbox[2]:.6f} {bbox[3]:.6f} {bbox[4]:.6f}\n")

def create_data_yaml(output_dir):
    """
    YOLO data.yaml 파일 생성
    """
    data_yaml = {
        'path': str(Path(output_dir).absolute()),
        'train': 'train/images',
        'val': 'val/images',
        'test': 'test/images',
        'nc': len(CLASS_NAMES),
        'names': CLASS_NAMES
    }

    yaml_path = Path(output_dir) / 'data.yaml'

    with open(yaml_path, 'w') as f:
        yaml.dump(data_yaml, f, default_flow_style=False)

    print(f"\n✅ data.yaml 생성 완료: {yaml_path}")
    print(f"   - 클래스 수: {len(CLASS_NAMES)}")
    print(f"   - 클래스: {', '.join(CLASS_NAMES)}")

def main():
    labeled_dir = 'data/raw/SolDef_AI/Labeled'
    dataset_dir = 'data/raw/SolDef_AI/Dataset'
    output_dir = 'data/processed/soldef_ai_yolo'

    print("=" * 70)
    print("SolDef_AI → YOLO Detection 형식 변환")
    print("=" * 70)

    # 데이터셋 변환 및 분할
    create_yolo_dataset(labeled_dir, dataset_dir, output_dir)

    # data.yaml 생성
    create_data_yaml(output_dir)

    print("\n" + "=" * 70)
    print("✅ 변환 완료!")
    print("=" * 70)
    print(f"출력 경로: {output_dir}")
    print(f"  - Train: {output_dir}/train/")
    print(f"  - Val: {output_dir}/val/")
    print(f"  - Test: {output_dir}/test/")
    print(f"  - data.yaml: {output_dir}/data.yaml")
    print()

if __name__ == "__main__":
    main()
