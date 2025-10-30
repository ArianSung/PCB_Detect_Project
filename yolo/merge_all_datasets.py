#!/usr/bin/env python3
"""
모든 PCB 데이터셋 통합 스크립트

통합할 데이터셋:
1. DeepPCB (현재): PCB 패턴 불량
2. SolDef_AI (신규): 납땜 불량 + 부품 위치 불량
3. PCBA-Dataset (신규): 부품 실장 불량
"""

import os
import shutil
import yaml
from pathlib import Path
from tqdm import tqdm
import cv2
import json


class PCBDatasetMerger:
    def __init__(self):
        self.project_root = Path("/home/sys1041/work_project")
        self.raw_data_dir = self.project_root / "data" / "raw"
        self.output_dir = self.project_root / "data" / "processed" / "complete_pcb_dataset"

        # 최종 클래스 정의 (모든 데이터셋 통합)
        self.final_classes = {
            # PCB 패턴 불량 (DeepPCB) - 기존
            'open': 0,
            'short': 1,
            'mousebite': 2,
            'spur': 3,
            'copper': 4,
            'pin-hole': 5,

            # 부품 실장 불량 (PCBA-Dataset) - 신규
            'missing_component': 6,
            'misaligned_component': 7,
            'damaged_component': 8,

            # 납땜 불량 (SolDef_AI) - 신규
            'solder_bridge': 9,
            'insufficient_solder': 10,
            'excess_solder': 11,
            'cold_joint': 12,
        }

    def setup_output_structure(self):
        """출력 디렉토리 구조 생성"""
        print("\n📁 출력 디렉토리 구조 생성 중...")

        for split in ['train', 'val', 'test']:
            (self.output_dir / split / 'images').mkdir(parents=True, exist_ok=True)
            (self.output_dir / split / 'labels').mkdir(parents=True, exist_ok=True)

        print("✅ 디렉토리 구조 생성 완료!")

    def merge_deeppcb(self):
        """DeepPCB 데이터셋 복사 (이미 YOLO 형식으로 변환됨)"""
        print("\n📦 1. DeepPCB 데이터셋 통합 중...")

        source_dir = self.project_root / "data" / "processed" / "combined_pcb_dataset"

        if not source_dir.exists():
            print("⚠️  DeepPCB 데이터셋을 찾을 수 없습니다. 건너뜁니다.")
            return

        stats = {'train': 0, 'val': 0, 'test': 0}

        for split in ['train', 'val', 'test']:
            src_images = source_dir / split / 'images'
            src_labels = source_dir / split / 'labels'

            if not src_images.exists():
                continue

            dst_images = self.output_dir / split / 'images'
            dst_labels = self.output_dir / split / 'labels'

            # 이미지 및 라벨 복사
            for img_file in tqdm(list(src_images.glob('*.jpg')), desc=f"DeepPCB {split}"):
                # 이미지 복사
                shutil.copy2(img_file, dst_images / f"deeppcb_{img_file.name}")

                # 라벨 복사 (클래스 ID는 그대로 유지, 0-5)
                label_file = src_labels / img_file.with_suffix('.txt').name
                if label_file.exists():
                    shutil.copy2(label_file, dst_labels / f"deeppcb_{label_file.name}")
                    stats[split] += 1

        print(f"✅ DeepPCB 통합 완료: {sum(stats.values())} 이미지")
        print(f"   Train: {stats['train']}, Val: {stats['val']}, Test: {stats['test']}")

    def process_soldef_ai(self):
        """SolDef_AI 데이터셋 처리 (납땜 불량)"""
        print("\n📦 2. SolDef_AI 데이터셋 처리 중...")

        source_dir = self.raw_data_dir / "soldef_ai"

        if not source_dir.exists():
            print("⚠️  SolDef_AI 데이터셋을 찾을 수 없습니다.")
            print("   먼저 scripts/download_component_solder_datasets.sh를 실행하세요.")
            return

        print("⚠️  SolDef_AI 데이터셋은 수동 라벨링이 필요합니다.")
        print("   Roboflow 또는 LabelImg를 사용하여 라벨링하세요.")
        print(f"   데이터 위치: {source_dir}")

        # TODO: 라벨링 완료 후 변환 로직 구현

    def process_pcba_dataset(self):
        """PCBA-Dataset 처리 (부품 실장 불량)"""
        print("\n📦 3. PCBA-Dataset 처리 중...")

        source_dir = self.raw_data_dir / "pcba_dataset" / "PCBA-Dataset"

        if not source_dir.exists():
            print("⚠️  PCBA-Dataset을 찾을 수 없습니다.")
            print("   먼저 scripts/download_component_solder_datasets.sh를 실행하세요.")
            return

        print("ℹ️  PCBA-Dataset YOLO 형식 변환 중...")

        # PCBA-Dataset의 클래스 매핑
        # 원본 데이터셋의 클래스를 우리의 최종 클래스로 매핑
        pcba_class_mapping = {
            'missing_screw': 'missing_component',  # 6
            'loose_screw': 'misaligned_component',  # 7
            'scratch': 'damaged_component',         # 8
        }

        # TODO: YOLO 라벨 파일 읽고 클래스 ID 변환
        print("⚠️  PCBA-Dataset은 클래스 매핑 검토가 필요합니다.")
        print(f"   데이터 위치: {source_dir}")

    def create_data_yaml(self):
        """최종 data.yaml 생성"""
        print("\n📝 data.yaml 생성 중...")

        data_yaml = {
            'path': str(self.output_dir),
            'train': 'train/images',
            'val': 'val/images',
            'test': 'test/images',
            'nc': len(self.final_classes),
            'names': {v: k for k, v in self.final_classes.items()}
        }

        yaml_path = self.output_dir / 'data.yaml'
        with open(yaml_path, 'w') as f:
            yaml.dump(data_yaml, f, default_flow_style=False, sort_keys=False)

        print(f"✅ data.yaml 생성 완료: {yaml_path}")

    def print_summary(self):
        """통합 결과 요약 출력"""
        print("\n" + "="*60)
        print("📊 데이터셋 통합 결과 요약")
        print("="*60)

        print(f"\n출력 경로: {self.output_dir}")

        print(f"\n최종 클래스 수: {len(self.final_classes)}")
        print("\n클래스 목록:")
        for name, idx in sorted(self.final_classes.items(), key=lambda x: x[1]):
            print(f"  {idx:2d}: {name}")

        # 각 split별 이미지 수 계산
        print("\n이미지 수:")
        for split in ['train', 'val', 'test']:
            img_dir = self.output_dir / split / 'images'
            if img_dir.exists():
                num_images = len(list(img_dir.glob('*.jpg')))
                print(f"  {split:5s}: {num_images:4d} 이미지")

        print("\n" + "="*60)
        print("✅ 데이터셋 통합 완료!")
        print("="*60)

        print("\n다음 단계:")
        print("1. SolDef_AI 데이터셋 라벨링 (Roboflow 또는 LabelImg)")
        print("2. PCBA-Dataset 클래스 매핑 검토 및 변환")
        print("3. 통합 데이터셋으로 YOLO 모델 학습:")
        print(f"   python yolo/train_complete_pcb.py")
        print()

    def run(self):
        """전체 프로세스 실행"""
        print("="*60)
        print("🚀 PCB 데이터셋 통합 시작")
        print("="*60)

        self.setup_output_structure()
        self.merge_deeppcb()
        self.process_soldef_ai()
        self.process_pcba_dataset()
        self.create_data_yaml()
        self.print_summary()


if __name__ == '__main__':
    merger = PCBDatasetMerger()
    merger.run()
