#!/usr/bin/env python3
"""
FPIC-Component (SMD) + 증강된 ElectroCom61 (Through-hole) 통합 스크립트 v2

변경사항:
- 증강된 ElectroCom61_augmented 사용
- Inductor/LED Valid/Test 분포 확인
"""

import os
import shutil
import yaml
from pathlib import Path
from collections import Counter

# 경로 설정
FPIC_DIR = "/home/sys1041/work_project/data/processed/fpic_component_yolo"
ELECTROCOM_DIR = "/home/sys1041/work_project/data/raw/ElectroCom61_augmented"
OUTPUT_DIR = "/home/sys1041/work_project/data/processed/unified_component_yolo_v2"

# Through-hole 클래스 매핑 (ElectroCom61 ID → 통합 데이터셋 ID)
TH_CLASS_MAPPING = {
    7: 25,   # BJT-Transistor → BJT-Transistor_TH
    13: 26,  # Capacitor-10mf → Capacitor-10mf_TH
    14: 27,  # Capacitor-470mf → Capacitor-470mf_TH
    16: 28,  # Diode → Diode_TH
    20: 29,  # Film-Capacitor → Film-Capacitor_TH
    21: 30,  # Fuse → Fuse_TH
    33: 31,  # Inductor → Inductor_TH
    37: 32,  # LED-Light → LED_TH
    50: 33,  # Resistor → Resistor_TH
    60: 34   # Zener-Diode → Zener-Diode_TH
}

TH_CLASS_NAMES = {
    25: 'BJT-Transistor_TH',
    26: 'Capacitor-10mf_TH',
    27: 'Capacitor-470mf_TH',
    28: 'Diode_TH',
    29: 'Film-Capacitor_TH',
    30: 'Fuse_TH',
    31: 'Inductor_TH',
    32: 'LED_TH',
    33: 'Resistor_TH',
    34: 'Zener-Diode_TH'
}

def process_split(split):
    """각 split 처리"""
    print(f"\n[{split.upper()}]")

    # FPIC는 'val' 사용, ElectroCom은 'valid' 사용
    fpic_split = 'val' if split == 'valid' else split

    # FPIC 데이터 복사 (test는 없음)
    fpic_img_dir = os.path.join(FPIC_DIR, fpic_split, 'images')
    fpic_lbl_dir = os.path.join(FPIC_DIR, fpic_split, 'labels')
    out_img_dir = os.path.join(OUTPUT_DIR, split, 'images')
    out_lbl_dir = os.path.join(OUTPUT_DIR, split, 'labels')

    fpic_count = 0

    # FPIC에 test 세트가 없으므로 스킵
    if os.path.exists(fpic_img_dir):
        for img_file in os.listdir(fpic_img_dir):
            src_img = os.path.join(fpic_img_dir, img_file)
            dst_img = os.path.join(out_img_dir, f"fpic_{img_file}")
            shutil.copy2(src_img, dst_img)

            label_file = img_file.replace('.jpg', '.txt').replace('.png', '.txt')
            src_lbl = os.path.join(fpic_lbl_dir, label_file)
            dst_lbl = os.path.join(out_lbl_dir, f"fpic_{label_file}")
            if os.path.exists(src_lbl):
                shutil.copy2(src_lbl, dst_lbl)
                fpic_count += 1

    print(f"  FPIC (SMD): {fpic_count}개")

    # ElectroCom61 데이터 처리 (클래스 ID 리매핑)
    ec_img_dir = os.path.join(ELECTROCOM_DIR, split, 'images')
    ec_lbl_dir = os.path.join(ELECTROCOM_DIR, split, 'labels')

    ec_count = 0
    th_class_counts = Counter()

    for label_file in os.listdir(ec_lbl_dir):
        if not label_file.endswith('.txt'):
            continue

        # 라벨 읽기 및 리매핑
        with open(os.path.join(ec_lbl_dir, label_file), 'r') as f:
            lines = f.readlines()

        new_lines = []
        has_th_class = False

        for line in lines:
            parts = line.strip().split()
            if len(parts) >= 5:
                old_class_id = int(float(parts[0]))

                # Through-hole 클래스인 경우
                if old_class_id in TH_CLASS_MAPPING:
                    has_th_class = True
                    new_class_id = TH_CLASS_MAPPING[old_class_id]
                    new_lines.append(f"{new_class_id} {' '.join(parts[1:])}\n")
                    th_class_counts[new_class_id] += 1

        # Through-hole 클래스를 포함한 이미지만 복사
        if has_th_class:
            # 라벨 저장
            dst_lbl = os.path.join(out_lbl_dir, f"ec_{label_file}")
            with open(dst_lbl, 'w') as f:
                f.writelines(new_lines)

            # 이미지 복사
            img_file = label_file.replace('.txt', '.jpg')
            src_img = os.path.join(ec_img_dir, img_file)
            dst_img = os.path.join(out_img_dir, f"ec_{img_file}")
            if os.path.exists(src_img):
                shutil.copy2(src_img, dst_img)
                ec_count += 1

    print(f"  ElectroCom61 (TH): {ec_count}개")

    # Through-hole 클래스별 통계
    print(f"  Through-hole 클래스별 객체 수:")
    for class_id in sorted(TH_CLASS_NAMES.keys()):
        count = th_class_counts.get(class_id, 0)
        name = TH_CLASS_NAMES[class_id]
        status = "✅" if count > 0 else "❌"
        print(f"    {name}: {count}개 {status}")

    return fpic_count, ec_count

def main():
    print("=" * 80)
    print("FPIC-Component (SMD) + 증강된 ElectroCom61 (Through-hole) 통합 v2")
    print("=" * 80)

    # 출력 디렉토리 생성
    for split in ['train', 'valid', 'test']:
        for subdir in ['images', 'labels']:
            os.makedirs(os.path.join(OUTPUT_DIR, split, subdir), exist_ok=True)

    # 각 split 처리
    total_stats = {}
    for split in ['train', 'valid', 'test']:
        fpic_count, ec_count = process_split(split)
        total_stats[split] = {'fpic': fpic_count, 'ec': ec_count}

    # data.yaml 생성
    with open(os.path.join(FPIC_DIR, 'data.yaml'), 'r') as f:
        fpic_yaml = yaml.safe_load(f)

    # 통합 클래스 이름
    unified_names = fpic_yaml['names'] + list(TH_CLASS_NAMES.values())

    unified_yaml = {
        'train': f'{OUTPUT_DIR}/train/images',
        'val': f'{OUTPUT_DIR}/valid/images',
        'test': f'{OUTPUT_DIR}/test/images',
        'nc': len(unified_names),
        'names': unified_names
    }

    with open(os.path.join(OUTPUT_DIR, 'data.yaml'), 'w') as f:
        yaml.dump(unified_yaml, f, sort_keys=False)

    print("\n" + "=" * 80)
    print("✅ 통합 완료!")
    print("=" * 80)

    print("\n📊 최종 통계:")
    print(f"{'Split':<10} {'FPIC (SMD)':>15} {'ElectroCom (TH)':>20} {'Total':>10}")
    print("-" * 80)

    for split in ['train', 'valid', 'test']:
        fpic = total_stats[split]['fpic']
        ec = total_stats[split]['ec']
        total = fpic + ec
        print(f"{split:<10} {fpic:>15,} {ec:>20,} {total:>10,}")

    print(f"\n총 클래스 수: {len(unified_names)}개")
    print(f"  - SMD (FPIC): {len(fpic_yaml['names'])}개")
    print(f"  - Through-hole (ElectroCom61): {len(TH_CLASS_NAMES)}개")

    print(f"\n📂 출력 디렉토리: {OUTPUT_DIR}")
    print(f"📄 data.yaml: {OUTPUT_DIR}/data.yaml")

if __name__ == '__main__':
    main()
