#!/usr/bin/env python3
"""
데이터셋 재균형 스크립트 (전략 1)
- 극소 클래스 7개 제거
- 과다 클래스 언더샘플링
"""

import os
import shutil
import random
import yaml
from pathlib import Path
from collections import defaultdict
from tqdm import tqdm

class DatasetRebalancer:
    def __init__(self):
        self.base_dir = Path("/home/sys1041/work_project/data/processed/complete_pcb_model")
        self.output_dir = Path("/home/sys1041/work_project/data/processed/balanced_pcb_model")

        # 제거할 클래스 (극소 샘플 7개)
        self.remove_classes = [10, 11, 12, 14, 17, 18, 23]  # battery, button, buzzer, clock, display, fuse, pins

        # 언더샘플링 대상 및 상한선
        self.undersample_targets = {
            26: 5000,  # comp_resistor: 30,316 → 5,000
            13: 5000,  # comp_capacitor: 30,690 → 5,000
            19: 3000,  # comp_ic: 7,763 → 3,000
            15: 2500,  # comp_connector: 4,781 → 2,500
        }

        # 클래스 ID 재매핑 (29개 → 22개)
        self.class_mapping = self._create_class_mapping()

        # 새로운 클래스 이름
        self.new_class_names = [
            'pcb_open',           # 0
            'pcb_short',          # 1
            'pcb_mousebite',      # 2
            'pcb_spur',           # 3
            'pcb_copper',         # 4
            'pcb_pin-hole',       # 5
            'solder_no_good',     # 6
            'solder_exc_solder',  # 7
            'solder_spike',       # 8
            'solder_poor_solder', # 9
            'comp_capacitor',     # 10 (기존 13)
            'comp_connector',     # 11 (기존 15)
            'comp_diode',         # 12 (기존 16)
            'comp_ic',            # 13 (기존 19)
            'comp_inductor',      # 14 (기존 20)
            'comp_led',           # 15 (기존 21)
            'comp_pads',          # 16 (기존 22)
            'comp_potentiometer', # 17 (기존 24)
            'comp_relay',         # 18 (기존 25)
            'comp_resistor',      # 19 (기존 26)
            'comp_switch',        # 20 (기존 27)
            'comp_transistor',    # 21 (기존 28)
        ]

        random.seed(42)

    def _create_class_mapping(self):
        """클래스 ID 재매핑 생성"""
        mapping = {}
        new_id = 0

        for old_id in range(29):
            if old_id in self.remove_classes:
                continue  # 제거 클래스는 매핑 안함

            mapping[old_id] = new_id
            new_id += 1

        return mapping

    def process_split(self, split):
        """Train/Val/Test 각 split 처리"""
        print(f"\n처리 중: {split} set")

        images_dir = self.base_dir / split / 'images'
        labels_dir = self.base_dir / split / 'labels'

        output_images_dir = self.output_dir / split / 'images'
        output_labels_dir = self.output_dir / split / 'labels'

        output_images_dir.mkdir(parents=True, exist_ok=True)
        output_labels_dir.mkdir(parents=True, exist_ok=True)

        # 클래스별로 이미지 파일 그룹화
        class_to_images = defaultdict(list)

        print("  1단계: 라벨 스캔 및 분류...")
        for label_file in tqdm(list(labels_dir.glob('*.txt'))):
            with open(label_file, 'r') as f:
                lines = f.readlines()

            if not lines:
                continue

            # 이미지에 포함된 클래스들
            image_classes = set()
            for line in lines:
                parts = line.strip().split()
                if len(parts) >= 5:
                    class_id = int(parts[0])
                    image_classes.add(class_id)

            # 제거 클래스 포함 여부 확인
            if any(cls in self.remove_classes for cls in image_classes):
                continue  # 제거 클래스 포함 이미지는 건너뜀

            # 언더샘플링 대상 클래스별로 그룹화
            for cls in image_classes:
                if cls in self.undersample_targets:
                    class_to_images[cls].append(label_file.stem)
                    break  # 하나라도 해당되면 추가 (중복 방지)
            else:
                # 언더샘플링 대상이 아닌 이미지
                class_to_images[-1].append(label_file.stem)  # -1은 '기타' 의미

        # 2단계: 언더샘플링 적용
        print("  2단계: 언더샘플링 적용...")
        selected_images = set()

        # 언더샘플링 대상 클래스
        for cls_id, max_count in self.undersample_targets.items():
            images_list = class_to_images[cls_id]

            if len(images_list) <= max_count:
                selected_images.update(images_list)
                print(f"    클래스 {cls_id}: {len(images_list)}개 (모두 유지)")
            else:
                sampled = random.sample(images_list, max_count)
                selected_images.update(sampled)
                print(f"    클래스 {cls_id}: {len(images_list)}개 → {max_count}개 ({len(images_list) - max_count}개 제거)")

        # 기타 이미지는 모두 포함
        selected_images.update(class_to_images[-1])
        print(f"    기타 이미지: {len(class_to_images[-1])}개 (모두 유지)")

        # 3단계: 파일 복사 및 클래스 ID 재매핑
        print("  3단계: 파일 복사 및 클래스 ID 재매핑...")

        copied_count = 0
        removed_objects = 0
        total_objects = 0

        for image_name in tqdm(selected_images):
            label_file = labels_dir / f"{image_name}.txt"
            image_file = images_dir / f"{image_name}.jpg"

            if not image_file.exists():
                image_file = images_dir / f"{image_name}.png"

            if not image_file.exists() or not label_file.exists():
                continue

            # 라벨 처리
            new_lines = []
            with open(label_file, 'r') as f:
                for line in f:
                    parts = line.strip().split()
                    if len(parts) >= 5:
                        old_class_id = int(parts[0])
                        total_objects += 1

                        # 제거 클래스 체크
                        if old_class_id in self.remove_classes:
                            removed_objects += 1
                            continue

                        # 클래스 ID 재매핑
                        new_class_id = self.class_mapping.get(old_class_id)
                        if new_class_id is not None:
                            parts[0] = str(new_class_id)
                            new_lines.append(' '.join(parts) + '\n')

            # 새 라벨이 있을 때만 저장
            if new_lines:
                # 이미지 복사
                shutil.copy2(image_file, output_images_dir / image_file.name)

                # 라벨 저장
                with open(output_labels_dir / label_file.name, 'w') as f:
                    f.writelines(new_lines)

                copied_count += 1

        print(f"  완료: {copied_count}개 이미지 복사, {removed_objects}/{total_objects}개 객체 제거")

        return {
            'images': copied_count,
            'removed_objects': removed_objects,
            'total_objects': total_objects,
        }

    def create_data_yaml(self):
        """새로운 data.yaml 생성"""
        print("\ndata.yaml 생성 중...")

        data_config = {
            'path': str(self.output_dir.absolute()),
            'train': 'train/images',
            'val': 'val/images',
            'test': 'test/images',
            'nc': len(self.new_class_names),
            'names': self.new_class_names,
        }

        yaml_path = self.output_dir / 'data.yaml'
        with open(yaml_path, 'w') as f:
            yaml.dump(data_config, f, default_flow_style=False, sort_keys=False)

        print(f"  저장 완료: {yaml_path}")

    def analyze_final_distribution(self):
        """최종 데이터셋 분포 분석"""
        print("\n" + "=" * 80)
        print("최종 데이터셋 분포 분석")
        print("=" * 80)

        for split in ['train', 'val', 'test']:
            labels_dir = self.output_dir / split / 'labels'

            class_counts = defaultdict(int)
            total_images = 0
            total_objects = 0

            for label_file in labels_dir.glob('*.txt'):
                total_images += 1
                with open(label_file, 'r') as f:
                    for line in f:
                        parts = line.strip().split()
                        if len(parts) >= 5:
                            class_id = int(parts[0])
                            class_counts[class_id] += 1
                            total_objects += 1

            print(f"\n{split.upper()} SET:")
            print(f"  총 이미지: {total_images:,}개")
            print(f"  총 객체: {total_objects:,}개")
            print(f"  평균 객체/이미지: {total_objects/total_images if total_images > 0 else 0:.2f}")

            if class_counts:
                sorted_classes = sorted(class_counts.items(), key=lambda x: x[1], reverse=True)
                print(f"\n  클래스별 분포 (상위 10개):")
                for class_id, count in sorted_classes[:10]:
                    class_name = self.new_class_names[class_id] if class_id < len(self.new_class_names) else f"Unknown_{class_id}"
                    print(f"    {class_id:2d}. {class_name:20s}: {count:5,}개")

                # 불균형 비율
                max_count = max(class_counts.values())
                min_count = min(class_counts.values())
                print(f"\n  불균형 비율: {max_count/min_count:.1f}:1 (최대 {max_count:,}개 / 최소 {min_count:,}개)")

    def run(self):
        """전체 프로세스 실행"""
        print("=" * 80)
        print("데이터셋 재균형 시작 (전략 1)")
        print("=" * 80)
        print(f"\n입력: {self.base_dir}")
        print(f"출력: {self.output_dir}")
        print(f"\n제거 클래스 (7개): {self.remove_classes}")
        print(f"언더샘플링 대상: {self.undersample_targets}")

        # 출력 디렉토리 생성
        self.output_dir.mkdir(parents=True, exist_ok=True)

        # 각 split 처리
        stats = {}
        for split in ['train', 'val', 'test']:
            stats[split] = self.process_split(split)

        # data.yaml 생성
        self.create_data_yaml()

        # 최종 통계
        self.analyze_final_distribution()

        print("\n" + "=" * 80)
        print("재균형 완료!")
        print("=" * 80)

        total_images = sum(s['images'] for s in stats.values())
        total_removed = sum(s['removed_objects'] for s in stats.values())
        total_objects = sum(s['total_objects'] for s in stats.values())

        print(f"\n총 처리 이미지: {total_images:,}개")
        print(f"제거된 객체: {total_removed:,}개 / {total_objects:,}개 ({total_removed/total_objects*100:.1f}%)")
        print(f"\n새 데이터셋 경로: {self.output_dir}")
        print(f"새 data.yaml: {self.output_dir / 'data.yaml'}")

if __name__ == "__main__":
    rebalancer = DatasetRebalancer()
    rebalancer.run()
