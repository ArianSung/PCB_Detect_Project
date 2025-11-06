"""
PCBSegClassNet Segmentation Masks → YOLO Bounding Box 형식 변환

FPIC-Component 데이터셋 (PCBSegClassNet)의 segmentation masks를
YOLO object detection 형식으로 변환합니다.

Input:  data/raw/PCBSegClassNet/data/segmentation/
Output: data/processed/fpic_component_yolo/
"""

import cv2
import numpy as np
from pathlib import Path
from tqdm import tqdm
import yaml

# 25개 클래스 정의 (PCBSegClassNet 논문 참조)
CLASS_NAMES = [
    'C',           # 0: Capacitor
    'R',           # 1: Resistor
    'U',           # 2: IC/Chip
    'J',           # 3: Connector
    'L',           # 4: Inductor
    'Q',           # 5: Transistor
    'P',           # 6: Power
    'D',           # 7: Diode
    'IC',          # 8: Integrated Circuit
    'RN',          # 9: Resistor Network
    'CR',          # 10: Crystal
    'RA',          # 11: Resistor Array
    'M',           # 12: Module
    'T',           # 13: Transformer
    'V',           # 14: Varistor
    'TP',          # 15: Test Point
    'FB',          # 16: Ferrite Bead
    'S',           # 17: Switch
    'BTN',         # 18: Button
    'CRA',         # 19: Crystal Oscillator
    'QA',          # 20: Transistor Array
    'LED',         # 21: LED
    'F',           # 22: Fuse
    'SW',          # 23: Switch (alternative)
    'JP'           # 24: Jumper
]

def mask_to_bbox(mask, class_id):
    """
    Segmentation mask를 YOLO bounding box로 변환

    Args:
        mask: Binary mask (H, W)
        class_id: Class ID

    Returns:
        List of bounding boxes in YOLO format: [class_id, x_center, y_center, width, height]
    """
    # Connected components로 각 객체 찾기
    num_labels, labels, stats, centroids = cv2.connectedComponentsWithStats(mask.astype(np.uint8), connectivity=8)

    bboxes = []
    h, w = mask.shape

    for i in range(1, num_labels):  # 0은 배경
        x, y, w_box, h_box, area = stats[i]

        # 너무 작은 영역 제외 (노이즈)
        if area < 100:
            continue

        # YOLO 형식으로 변환 (normalized)
        x_center = (x + w_box / 2) / w
        y_center = (y + h_box / 2) / h
        width = w_box / w
        height = h_box / h

        bboxes.append([class_id, x_center, y_center, width, height])

    return bboxes

def convert_dataset(input_dir, output_dir, split='train'):
    """
    데이터셋 변환

    Args:
        input_dir: PCBSegClassNet segmentation 데이터 경로
        output_dir: YOLO 형식 출력 경로
        split: 'train' 또는 'val'
    """
    input_path = Path(input_dir) / split
    output_path = Path(output_dir) / split

    images_input = input_path / 'images'
    masks_input = input_path / 'masks'

    images_output = output_path / 'images'
    labels_output = output_path / 'labels'

    images_output.mkdir(parents=True, exist_ok=True)
    labels_output.mkdir(parents=True, exist_ok=True)

    # 모든 mask 파일 처리
    mask_files = sorted(masks_input.glob('*.png'))

    print(f"\n변환 중: {split} split ({len(mask_files)} 이미지)")

    for mask_file in tqdm(mask_files):
        # Mask 읽기 (grayscale)
        mask = cv2.imread(str(mask_file), cv2.IMREAD_GRAYSCALE)

        if mask is None:
            print(f"Warning: 읽기 실패 - {mask_file}")
            continue

        # 대응하는 이미지 복사
        image_name = mask_file.stem

        # 원본 이미지 찾기 (PNG 또는 JPG)
        image_file = None
        for ext in ['.png', '.jpg', '.jpeg']:
            candidate = images_input / f"{image_name}{ext}"
            if candidate.exists():
                image_file = candidate
                break

        if image_file is None:
            print(f"Warning: 이미지 없음 - {image_name}")
            continue

        # 이미지 복사
        image = cv2.imread(str(image_file))
        cv2.imwrite(str(images_output / f"{image_name}.jpg"), image)

        # 모든 bounding boxes 추출
        all_bboxes = []

        # 각 클래스별로 처리 (mask의 픽셀 값이 클래스 ID)
        unique_classes = np.unique(mask)

        for class_id in unique_classes:
            if class_id == 0:  # 배경 제외
                continue

            # 해당 클래스만 추출
            class_mask = (mask == class_id).astype(np.uint8) * 255

            # Bounding boxes 생성
            bboxes = mask_to_bbox(class_mask, class_id - 1)  # 0-indexed
            all_bboxes.extend(bboxes)

        # YOLO 라벨 파일 저장
        label_file = labels_output / f"{image_name}.txt"

        with open(label_file, 'w') as f:
            for bbox in all_bboxes:
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
        'nc': len(CLASS_NAMES),
        'names': CLASS_NAMES
    }

    yaml_path = Path(output_dir) / 'data.yaml'

    with open(yaml_path, 'w') as f:
        yaml.dump(data_yaml, f, default_flow_style=False)

    print(f"\n✅ data.yaml 생성 완료: {yaml_path}")
    print(f"   - 클래스 수: {len(CLASS_NAMES)}")
    print(f"   - 클래스: {', '.join(CLASS_NAMES[:5])}... (총 {len(CLASS_NAMES)}개)")

def main():
    input_dir = 'data/raw/PCBSegClassNet/data/segmentation'
    output_dir = 'data/processed/fpic_component_yolo'

    print("=" * 70)
    print("PCBSegClassNet → YOLO Detection 형식 변환")
    print("=" * 70)

    # Train set 변환
    convert_dataset(input_dir, output_dir, split='train')

    # Val set 변환
    convert_dataset(input_dir, output_dir, split='val')

    # data.yaml 생성
    create_data_yaml(output_dir)

    print("\n" + "=" * 70)
    print("✅ 변환 완료!")
    print("=" * 70)
    print(f"출력 경로: {output_dir}")
    print(f"  - Train: {output_dir}/train/")
    print(f"  - Val: {output_dir}/val/")
    print(f"  - data.yaml: {output_dir}/data.yaml")
    print()

if __name__ == "__main__":
    main()
