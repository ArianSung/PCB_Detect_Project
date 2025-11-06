#!/usr/bin/env python3
"""
FPIC-Component (SMD) + ElectroCom61 (Through-hole) 데이터셋 통합

목적: SMD 부품 + Through-hole 부품을 모두 검출할 수 있는 통합 모델 학습
"""

import os
import shutil
import yaml
from pathlib import Path
from tqdm import tqdm

# 통합 설정
OUTPUT_DIR = "/home/sys1041/work_project/data/processed/unified_component_yolo"
FPIC_DIR = "/home/sys1041/work_project/data/processed/fpic_component_yolo"
ELECTROCOM_DIR = "/home/sys1041/work_project/data/raw/ElectroCom61/dataset/ElectroCom-61_v2"

# ElectroCom61의 Through-hole 클래스 (원래 클래스 ID → 클래스 이름)
THROUGH_HOLE_CLASSES = {
    50: 'Resistor_TH',
    20: 'Film-Capacitor_TH',
    13: 'Capacitor-10mf_TH',
    14: 'Capacitor-470mf_TH',
    37: 'LED_TH',
    16: 'Diode_TH',
    60: 'Zener-Diode_TH',
    7: 'BJT-Transistor_TH',
    33: 'Inductor_TH',
    21: 'Fuse_TH'
}

def load_fpic_classes():
    """FPIC-Component 클래스 로드"""
    data_yaml = os.path.join(FPIC_DIR, 'data.yaml')
    with open(data_yaml, 'r') as f:
        data = yaml.safe_load(f)
    return data['names']

def create_unified_dataset():
    """통합 데이터셋 생성"""
    print("=" * 80)
    print("FPIC-Component (SMD) + ElectroCom61 (Through-hole) 데이터셋 통합")
    print("=" * 80)

    # 1. FPIC 클래스 로드
    fpic_classes = load_fpic_classes()
    print(f"\n✅ FPIC-Component (SMD): {len(fpic_classes)}개 클래스")

    # 2. 통합 클래스 목록 생성 (FPIC + Through-hole)
    unified_classes = fpic_classes.copy()  # 0-24: FPIC 클래스

    # Through-hole 클래스를 25-34번으로 추가
    th_class_mapping = {}  # 원래 ID → 새 ID
    for old_id, class_name in sorted(THROUGH_HOLE_CLASSES.items()):
        new_id = len(unified_classes)
        unified_classes.append(class_name)
        th_class_mapping[old_id] = new_id

    print(f"✅ Through-hole 부품: {len(THROUGH_HOLE_CLASSES)}개 클래스")
    print(f"✅ 통합 데이터셋: {len(unified_classes)}개 클래스\n")

    # 3. 출력 디렉토리 생성
    os.makedirs(OUTPUT_DIR, exist_ok=True)
    for split in ['train', 'valid', 'test']:
        os.makedirs(os.path.join(OUTPUT_DIR, split, 'images'), exist_ok=True)
        os.makedirs(os.path.join(OUTPUT_DIR, split, 'labels'), exist_ok=True)

    # 4. FPIC 데이터 복사 (클래스 ID 그대로 유지)
    print("📋 FPIC-Component 데이터 복사 중...")
    for split in ['train', 'val']:
        split_name = 'valid' if split == 'val' else split

        src_img_dir = os.path.join(FPIC_DIR, split, 'images')
        src_lbl_dir = os.path.join(FPIC_DIR, split, 'labels')
        dst_img_dir = os.path.join(OUTPUT_DIR, split_name, 'images')
        dst_lbl_dir = os.path.join(OUTPUT_DIR, split_name, 'labels')

        if not os.path.exists(src_img_dir):
            continue

        for img_file in tqdm(os.listdir(src_img_dir), desc=f"  {split}"):
            # 이미지 복사
            shutil.copy2(
                os.path.join(src_img_dir, img_file),
                os.path.join(dst_img_dir, f"fpic_{img_file}")
            )

            # 라벨 복사 (클래스 ID 그대로)
            lbl_file = img_file.replace('.jpg', '.txt')
            shutil.copy2(
                os.path.join(src_lbl_dir, lbl_file),
                os.path.join(dst_lbl_dir, f"fpic_{lbl_file}")
            )

    # 5. ElectroCom61 Through-hole 데이터 추가 (클래스 ID 재매핑)
    print("\n📋 ElectroCom61 Through-hole 데이터 추가 중...")

    stats = {'train': 0, 'valid': 0, 'test': 0}

    for split in ['train', 'valid', 'test']:
        src_img_dir = os.path.join(ELECTROCOM_DIR, split, 'images')
        src_lbl_dir = os.path.join(ELECTROCOM_DIR, split, 'labels')
        dst_img_dir = os.path.join(OUTPUT_DIR, split, 'images')
        dst_lbl_dir = os.path.join(OUTPUT_DIR, split, 'labels')

        if not os.path.exists(src_lbl_dir):
            continue

        for lbl_file in tqdm(os.listdir(src_lbl_dir), desc=f"  {split}"):
            src_lbl_path = os.path.join(src_lbl_dir, lbl_file)

            # 라벨 파일 읽기 및 Through-hole 클래스 포함 여부 확인
            with open(src_lbl_path, 'r') as f:
                lines = f.readlines()

            # Through-hole 클래스만 필터링하고 ID 재매핑
            new_lines = []
            has_th_class = False

            for line in lines:
                parts = line.strip().split()
                if len(parts) < 5:
                    continue

                old_class_id = int(parts[0])

                # Through-hole 클래스인 경우
                if old_class_id in th_class_mapping:
                    has_th_class = True
                    new_class_id = th_class_mapping[old_class_id]
                    new_lines.append(f"{new_class_id} {' '.join(parts[1:])}\n")

            # Through-hole 클래스가 있는 경우만 복사
            if has_th_class:
                # 이미지 복사
                img_file = lbl_file.replace('.txt', '.jpg')
                shutil.copy2(
                    os.path.join(src_img_dir, img_file),
                    os.path.join(dst_img_dir, f"ec61_{img_file}")
                )

                # 재매핑된 라벨 저장
                dst_lbl_path = os.path.join(dst_lbl_dir, f"ec61_{lbl_file}")
                with open(dst_lbl_path, 'w') as f:
                    f.writelines(new_lines)

                stats[split] += 1

    # 6. data.yaml 생성
    print("\n📋 data.yaml 생성 중...")

    data_yaml = {
        'path': OUTPUT_DIR,
        'train': 'train/images',
        'val': 'valid/images',
        'test': 'test/images',
        'nc': len(unified_classes),
        'names': unified_classes
    }

    yaml_path = os.path.join(OUTPUT_DIR, 'data.yaml')
    with open(yaml_path, 'w') as f:
        yaml.dump(data_yaml, f, default_flow_style=False, allow_unicode=True)

    print(f"✅ {yaml_path}")

    # 7. 통계 출력
    print("\n" + "=" * 80)
    print("통합 완료!")
    print("=" * 80)

    print(f"\n📊 FPIC-Component (SMD):")
    print(f"  - Train: ~5,008 이미지")
    print(f"  - Valid: ~1,252 이미지")
    print(f"  - 클래스: {len(fpic_classes)}개 (0-{len(fpic_classes)-1})")

    print(f"\n📊 ElectroCom61 (Through-hole):")
    print(f"  - Train: {stats['train']} 이미지")
    print(f"  - Valid: {stats['valid']} 이미지")
    print(f"  - Test: {stats['test']} 이미지")
    print(f"  - 클래스: {len(THROUGH_HOLE_CLASSES)}개 ({len(fpic_classes)}-{len(unified_classes)-1})")

    print(f"\n📊 통합 데이터셋:")
    print(f"  - 총 클래스: {len(unified_classes)}개")
    print(f"  - 경로: {OUTPUT_DIR}")

    print("\n✅ 통합 데이터셋 생성 완료!")
    print(f"   다음 명령으로 학습 시작:")
    print(f"   yolo detect train data={OUTPUT_DIR}/data.yaml model=yolov8l.pt ...")
    print("=" * 80)

    # 클래스 목록 출력
    print("\n📋 통합 클래스 목록:")
    print("\nSMD 부품 (FPIC-Component, 0-24):")
    for i, name in enumerate(fpic_classes):
        print(f"  {i:2d}. {name}")

    print("\nThrough-hole 부품 (ElectroCom61, 25-34):")
    for i in range(len(fpic_classes), len(unified_classes)):
        print(f"  {i:2d}. {unified_classes[i]}")

    print("\n" + "=" * 80)

if __name__ == "__main__":
    create_unified_dataset()
