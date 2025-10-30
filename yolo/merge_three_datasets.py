#!/usr/bin/env python3
"""
3개 데이터셋 통합 (DeepPCB + SolDef_AI + PCB Components)

최종 목표: 32개 클래스의 통합 PCB 검사 모델
- PCB 패턴 불량 (6개)
- 납땜 불량 (4개)
- 부품 종류 (22개)
"""

import os
import shutil
import yaml
from pathlib import Path
from tqdm import tqdm
import random


class ThreeDatasetMerger:
    def __init__(self):
        self.project_root = Path("/home/sys1041/work_project")
        self.output_dir = self.project_root / "data" / "processed" / "complete_pcb_model"

        # 최종 클래스 정의 (32개)
        self.final_classes = {}
        current_id = 0

        # 1. DeepPCB 클래스 (0-5)
        deeppcb_classes = ['open', 'short', 'mousebite', 'spur', 'copper', 'pin-hole']
        for cls in deeppcb_classes:
            self.final_classes[f'pcb_{cls}'] = current_id
            current_id += 1

        # 2. SolDef_AI 클래스 (6-9)
        soldef_classes = ['no_good', 'exc_solder', 'spike', 'poor_solder']
        for cls in soldef_classes:
            self.final_classes[f'solder_{cls}'] = current_id
            current_id += 1

        # 3. PCB Components 클래스 (10-31) - 22개
        component_classes = [
            'battery', 'button', 'buzzer', 'capacitor', 'clock', 'connector',
            'diode', 'display', 'fuse', 'heatsink', 'ic', 'inductor',
            'led', 'pads', 'pins', 'potentiometer', 'relay', 'resistor',
            'switch', 'transducer', 'transformer', 'transistor'
        ]
        for cls in component_classes:
            self.final_classes[f'comp_{cls}'] = current_id
            current_id += 1

        print(f"\n총 클래스 수: {len(self.final_classes)}")

    def setup_output_structure(self):
        """출력 디렉토리 구조 생성"""
        print("\n📁 출력 디렉토리 구조 생성 중...")

        for split in ['train', 'val', 'test']:
            (self.output_dir / split / 'images').mkdir(parents=True, exist_ok=True)
            (self.output_dir / split / 'labels').mkdir(parents=True, exist_ok=True)

        print("✅ 디렉토리 생성 완료!")

    def merge_deeppcb(self):
        """DeepPCB 데이터셋 통합 (클래스 ID: 0-5)"""
        print("\n📦 1. DeepPCB 데이터셋 통합 중...")

        source_dir = self.project_root / "data" / "processed" / "combined_pcb_dataset"

        if not source_dir.exists():
            print("⚠️  DeepPCB 데이터셋을 찾을 수 없습니다.")
            return {'train': 0, 'val': 0, 'test': 0}

        stats = {'train': 0, 'val': 0, 'test': 0}

        for split in ['train', 'val', 'test']:
            src_images = source_dir / split / 'images'
            src_labels = source_dir / split / 'labels'

            if not src_images.exists():
                continue

            dst_images = self.output_dir / split / 'images'
            dst_labels = self.output_dir / split / 'labels'

            for img_file in tqdm(list(src_images.glob('*.jpg')), desc=f"DeepPCB {split}"):
                # 이미지 복사
                shutil.copy2(img_file, dst_images / f"deeppcb_{img_file.name}")

                # 라벨 복사 (클래스 ID는 그대로 0-5)
                label_file = src_labels / img_file.with_suffix('.txt').name
                if label_file.exists():
                    shutil.copy2(label_file, dst_labels / f"deeppcb_{label_file.name}")
                    stats[split] += 1

        print(f"✅ DeepPCB 통합 완료: {sum(stats.values())} 이미지")
        return stats

    def merge_soldef(self):
        """SolDef_AI 데이터셋 통합 (클래스 ID: 6-9)"""
        print("\n📦 2. SolDef_AI 데이터셋 통합 중...")

        source_dir = self.project_root / "data" / "processed" / "soldef_yolo"

        if not source_dir.exists():
            print("⚠️  SolDef_AI 데이터셋을 찾을 수 없습니다.")
            return {'train': 0, 'val': 0, 'test': 0}

        # SolDef 클래스 매핑: 원본 ID → 새 ID
        # 원본: 0: no_good, 1: exc_solder, 2: spike, 3: poor_solder
        # 새 ID: 6-9
        class_mapping = {
            0: 6,  # no_good → 6
            1: 7,  # exc_solder → 7
            2: 8,  # spike → 8
            3: 9,  # poor_solder → 9
        }

        stats = {'train': 0, 'val': 0, 'test': 0}

        for split in ['train', 'val', 'test']:
            src_images = source_dir / split / 'images'
            src_labels = source_dir / split / 'labels'

            if not src_images.exists():
                continue

            dst_images = self.output_dir / split / 'images'
            dst_labels = self.output_dir / split / 'labels'

            for img_file in tqdm(list(src_images.glob('*.jpg')), desc=f"SolDef {split}"):
                # 이미지 복사
                dst_img = dst_images / f"soldef_{img_file.name}"
                shutil.copy2(img_file, dst_img)

                # 라벨 변환 (클래스 ID 재매핑)
                label_file = src_labels / img_file.with_suffix('.txt').name
                if label_file.exists():
                    dst_label = dst_labels / f"soldef_{label_file.name}"

                    with open(label_file, 'r') as f_in, open(dst_label, 'w') as f_out:
                        for line in f_in:
                            parts = line.strip().split()
                            if not parts:
                                continue

                            old_class_id = int(parts[0])

                            # 클래스 ID 재매핑
                            if old_class_id in class_mapping:
                                new_class_id = class_mapping[old_class_id]
                                parts[0] = str(new_class_id)
                                f_out.write(' '.join(parts) + '\n')

                    stats[split] += 1

        print(f"✅ SolDef_AI 통합 완료: {sum(stats.values())} 이미지")
        return stats

    def merge_components(self):
        """PCB Components 데이터셋 통합 (클래스 ID: 10-31)"""
        print("\n📦 3. PCB Components 데이터셋 통합 중...")

        source_dir = self.project_root / "data" / "raw" / "pcb_components" / "components_data_uncropped"

        if not source_dir.exists():
            print("⚠️  PCB Components 데이터셋을 찾을 수 없습니다.")
            return {'train': 0, 'val': 0, 'test': 0}

        stats = {'train': 0, 'val': 0, 'test': 0}

        # split 매핑: valid → val
        split_mapping = {
            'train': 'train',
            'valid': 'val',
            'test': 'test'
        }

        for src_split, dst_split in split_mapping.items():
            src_images = source_dir / src_split / 'images'
            src_labels = source_dir / src_split / 'labels'

            if not src_images.exists():
                continue

            dst_images = self.output_dir / dst_split / 'images'
            dst_labels = self.output_dir / dst_split / 'labels'

            # jpg, png 모두 지원
            image_files = list(src_images.glob('*.jpg')) + list(src_images.glob('*.png'))

            for img_file in tqdm(image_files, desc=f"Components {dst_split}"):
                # 이미지 복사
                dst_img = dst_images / f"comp_{img_file.name}"
                shutil.copy2(img_file, dst_img)

                # 라벨 변환 (클래스 ID +10)
                label_file = src_labels / img_file.with_suffix('.txt').name
                if label_file.exists():
                    dst_label = dst_labels / f"comp_{label_file.name}"

                    with open(label_file, 'r') as f_in, open(dst_label, 'w') as f_out:
                        for line in f_in:
                            parts = line.strip().split()
                            if not parts:
                                continue

                            old_class_id = int(parts[0])
                            new_class_id = old_class_id + 10  # Components는 10부터 시작
                            parts[0] = str(new_class_id)
                            f_out.write(' '.join(parts) + '\n')

                    stats[dst_split] += 1

        print(f"✅ PCB Components 통합 완료: {sum(stats.values())} 이미지")
        return stats

    def create_data_yaml(self):
        """최종 data.yaml 생성"""
        print("\n📝 data.yaml 생성 중...")

        # 클래스 이름 리스트 생성 (ID 순서대로)
        names_list = [''] * len(self.final_classes)
        for name, idx in self.final_classes.items():
            names_list[idx] = name

        data_yaml = {
            'path': str(self.output_dir),
            'train': 'train/images',
            'val': 'val/images',
            'test': 'test/images',
            'nc': len(self.final_classes),
            'names': names_list
        }

        yaml_path = self.output_dir / 'data.yaml'
        with open(yaml_path, 'w') as f:
            yaml.dump(data_yaml, f, default_flow_style=False, sort_keys=False)

        print(f"✅ data.yaml 생성 완료: {yaml_path}")

    def print_summary(self, stats_dict):
        """통합 결과 요약 출력"""
        print("\n" + "="*70)
        print("📊 3개 데이터셋 통합 결과 요약")
        print("="*70)

        print(f"\n출력 경로: {self.output_dir}")

        print(f"\n클래스 구성:")
        print("  1. PCB 패턴 불량 (DeepPCB):     6개 (ID 0-5)")
        print("  2. 납땜 불량 (SolDef_AI):       4개 (ID 6-9)")
        print("  3. 부품 종류 (Components):     22개 (ID 10-31)")
        print(f"  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")
        print(f"  총 클래스 수: {len(self.final_classes)}")

        print("\n데이터셋별 통합 결과:")
        for dataset, stats in stats_dict.items():
            if stats:
                total = sum(stats.values())
                print(f"  {dataset:20s}: {total:5d} 이미지")
                print(f"    └─ train: {stats['train']}, val: {stats['val']}, test: {stats['test']}")

        # 각 split별 총 이미지 수
        print("\n최종 Split별 이미지 수:")
        for split in ['train', 'val', 'test']:
            img_dir = self.output_dir / split / 'images'
            if img_dir.exists():
                num_images = len(list(img_dir.glob('*.jpg')) + list(img_dir.glob('*.png')))
                print(f"  {split:5s}: {num_images:5d} 이미지")

        print("\n" + "="*70)
        print("✅ 전체 데이터셋 통합 완료!")
        print("="*70)

        print("\n📝 클래스 목록 (상세):")
        print("\n  [PCB 패턴 불량]")
        for idx in range(6):
            class_name = [name for name, id in self.final_classes.items() if id == idx][0]
            print(f"    {idx:2d}: {class_name}")

        print("\n  [납땜 불량]")
        for idx in range(6, 10):
            class_name = [name for name, id in self.final_classes.items() if id == idx][0]
            print(f"    {idx:2d}: {class_name}")

        print("\n  [부품 종류]")
        for idx in range(10, 32):
            class_name = [name for name, id in self.final_classes.items() if id == idx][0]
            print(f"    {idx:2d}: {class_name}")

        print("\n" + "="*70)
        print("🚀 다음 단계:")
        print("="*70)
        print("\n1. 데이터셋 확인:")
        print(f"   ls {self.output_dir}/train/images/ | head -20")
        print("\n2. Base Model 학습 시작:")
        print(f"   yolo detect train data={self.output_dir}/data.yaml model=yolov8l.pt epochs=150 batch=32 imgsz=640")
        print("\n3. (나중에) 실제 PCB로 Fine-tuning:")
        print(f"   yolo detect train data=project_pcb.yaml model=runs/detect/train/weights/best.pt epochs=50")
        print()

    def run(self):
        """전체 프로세스 실행"""
        print("="*70)
        print("🚀 3개 데이터셋 통합 시작")
        print("   DeepPCB + SolDef_AI + PCB Components")
        print("="*70)

        self.setup_output_structure()

        stats_dict = {}
        stats_dict['DeepPCB'] = self.merge_deeppcb()
        stats_dict['SolDef_AI'] = self.merge_soldef()
        stats_dict['PCB_Components'] = self.merge_components()

        self.create_data_yaml()
        self.print_summary(stats_dict)


if __name__ == '__main__':
    merger = ThreeDatasetMerger()
    merger.run()
