"""
PCBSegClassNet Segmentation Masks → YOLO Bounding Box 형식 변환 (수정 버전)

FPIC-Component 데이터셋 (PCBSegClassNet)의 segmentation masks를
YOLO object detection 형식으로 변환합니다.

Input:  data/raw/PCBSegClassNet/data/segmentation/
Output: data/processed/fpic_component_yolo/

수정사항: RGB 컬러 마스크를 올바르게 처리
"""

import cv2
import numpy as np
from pathlib import Path
from tqdm import tqdm
import yaml

# PCBSegClassNet color_values 매핑 (create_mask.py에서 가져옴)
COLOR_TO_CLASS = {
    (255, 0, 0): "R",           # Resistor
    (255, 255, 0): "C",         # Capacitor
    (0, 234, 255): "U",         # IC/Chip
    (170, 0, 255): "Q",         # Transistor
    (255, 127, 0): "J",         # Connector
    (191, 255, 0): "L",         # Inductor
    (0, 149, 255): "RA",        # Resistor Array
    (106, 255, 0): "D",         # Diode
    (0, 64, 255): "RN",         # Resistor Network
    (237, 185, 185): "TP",      # Test Point
    (185, 215, 237): "IC",      # Integrated Circuit
    (231, 233, 185): "P",       # Power
    (220, 185, 237): "CR",      # Crystal
    (185, 237, 224): "M",       # Module
    (143, 35, 35): "BTN",       # Button
    (35, 98, 143): "FB",        # Ferrite Bead
    (143, 106, 35): "CRA",      # Crystal Oscillator
    (107, 35, 143): "SW",       # Switch
    (79, 143, 35): "T",         # Transformer
    (115, 115, 115): "F",       # Fuse
    (204, 204, 204): "V",       # Varistor
    (245, 130, 48): "LED",      # LED
    (220, 190, 255): "S",       # Switch
    (170, 255, 195): "QA",      # Transistor Array
    (255, 250, 200): "JP"       # Jumper
}

# 25개 클래스 정의 (정렬된 순서)
CLASS_NAMES = sorted(list(set(COLOR_TO_CLASS.values())))

print(f"클래스 수: {len(CLASS_NAMES)}")
print(f"클래스 목록: {CLASS_NAMES}")

def rgb_to_class_mask(mask_rgb):
    """
    RGB 마스크를 클래스 ID 마스크로 변환

    Args:
        mask_rgb: RGB mask (H, W, 3)

    Returns:
        class_mask: Class ID mask (H, W), 배경은 255
    """
    h, w = mask_rgb.shape[:2]
    class_mask = np.full((h, w), 255, dtype=np.uint8)  # 255 = 배경

    # 각 RGB 컬러를 클래스 ID로 매핑
    for color_bgr, class_name in COLOR_TO_CLASS.items():
        # OpenCV는 BGR 순서
        color_bgr_cv = (color_bgr[2], color_bgr[1], color_bgr[0])

        # 해당 색상의 픽셀 찾기 (약간의 tolerance 허용)
        mask_match = np.all(np.abs(mask_rgb - color_bgr_cv) < 10, axis=-1)

        class_id = CLASS_NAMES.index(class_name)
        class_mask[mask_match] = class_id

    return class_mask

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
        if area < 50:  # threshold를 100에서 50으로 낮춤
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

    total_objects = 0

    for mask_file in tqdm(mask_files):
        # Mask 읽기 (RGB)
        mask_rgb = cv2.imread(str(mask_file))

        if mask_rgb is None:
            print(f"Warning: 읽기 실패 - {mask_file}")
            continue

        # RGB 마스크를 클래스 ID 마스크로 변환
        class_mask = rgb_to_class_mask(mask_rgb)

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

        # 각 클래스별로 처리
        unique_classes = np.unique(class_mask)

        for class_id in unique_classes:
            if class_id == 255:  # 배경 제외
                continue

            # 해당 클래스만 추출
            binary_mask = (class_mask == class_id).astype(np.uint8) * 255

            # Bounding boxes 생성
            bboxes = mask_to_bbox(binary_mask, class_id)
            all_bboxes.extend(bboxes)

        total_objects += len(all_bboxes)

        # YOLO 라벨 파일 저장
        label_file = labels_output / f"{image_name}.txt"

        with open(label_file, 'w') as f:
            for bbox in all_bboxes:
                # YOLO 형식: class_id x_center y_center width height
                f.write(f"{int(bbox[0])} {bbox[1]:.6f} {bbox[2]:.6f} {bbox[3]:.6f} {bbox[4]:.6f}\n")

    print(f"  ✅ 총 {total_objects}개 객체 검출됨")

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
    print("PCBSegClassNet → YOLO Detection 형식 변환 (수정 버전)")
    print("=" * 70)

    # 기존 출력 삭제
    import shutil
    if Path(output_dir).exists():
        print(f"\n기존 출력 디렉토리 삭제: {output_dir}")
        shutil.rmtree(output_dir)

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
