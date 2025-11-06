#!/usr/bin/env python3
"""
객체 단위 언더샘플링 (전략 A)
각 이미지에서 resistor/capacitor 객체를 랜덤하게 일부만 선택
"""

import os
import random
import yaml
from pathlib import Path
from collections import defaultdict
from tqdm import tqdm

class ObjectLevelUndersampler:
    def __init__(self):
        self.base_dir = Path("/home/sys1041/work_project/data/processed/balanced_pcb_model")
        self.output_dir = Path("/home/sys1041/work_project/data/processed/final_balanced_pcb_model")

        # 객체 단위 언더샘플링 대상 (현재 데이터셋의 class ID)
        self.target_classes = {
            19: 5000,  # comp_resistor: 26,113 → 5,000
            10: 5000,  # comp_capacitor: 23,254 → 5,000
        }

        random.seed(42)

    def calculate_per_image_limit(self, split):
        """각 이미지당 유지할 객체 수 계산"""
        labels_dir = self.base_dir / split / 'labels'

        # 클래스별 총 객체 수 및 이미지당 객체 수 카운트
        class_stats = defaultdict(lambda: {'total': 0, 'images': defaultdict(int)})

        print(f"  분석 중: {split} set의 클래스별 분포...")
        for label_file in labels_dir.glob('*.txt'):
            with open(label_file, 'r') as f:
                for line in f:
                    parts = line.strip().split()
                    if len(parts) >= 5:
                        class_id = int(parts[0])
                        class_stats[class_id]['total'] += 1
                        class_stats[class_id]['images'][label_file.stem] += 1

        # 각 클래스별 이미지당 평균 유지 비율 계산
        sampling_ratios = {}
        for class_id, target_count in self.target_classes.items():
            current_total = class_stats[class_id]['total']
            num_images = len(class_stats[class_id]['images'])

            if current_total > target_count:
                # 전체 비율로 계산
                ratio = target_count / current_total
                sampling_ratios[class_id] = ratio
                print(f"    클래스 {class_id}: {current_total:,}개 → {target_count:,}개 (비율: {ratio:.2%})")
            else:
                sampling_ratios[class_id] = 1.0
                print(f"    클래스 {class_id}: {current_total:,}개 (유지)")

        return sampling_ratios

    def process_split(self, split):
        """Train/Val/Test 각 split 처리"""
        print(f"\n{'='*80}")
        print(f"처리 중: {split} set")
        print(f"{'='*80}")

        # 샘플링 비율 계산
        sampling_ratios = self.calculate_per_image_limit(split)

        images_dir = self.base_dir / split / 'images'
        labels_dir = self.base_dir / split / 'labels'

        output_images_dir = self.output_dir / split / 'images'
        output_labels_dir = self.output_dir / split / 'labels'

        output_images_dir.mkdir(parents=True, exist_ok=True)
        output_labels_dir.mkdir(parents=True, exist_ok=True)

        # 통계
        stats = defaultdict(lambda: {'before': 0, 'after': 0})
        processed_images = 0

        print(f"\n  처리 중: 라벨 파일 언더샘플링...")

        for label_file in tqdm(list(labels_dir.glob('*.txt'))):
            image_name = label_file.stem
            image_file = images_dir / f"{image_name}.jpg"

            if not image_file.exists():
                image_file = images_dir / f"{image_name}.png"

            if not image_file.exists():
                continue

            # 라벨 읽기 및 클래스별 분류
            lines_by_class = defaultdict(list)

            with open(label_file, 'r') as f:
                for line in f:
                    parts = line.strip().split()
                    if len(parts) >= 5:
                        class_id = int(parts[0])
                        lines_by_class[class_id].append(line)
                        stats[class_id]['before'] += 1

            # 새 라벨 생성
            new_lines = []

            for class_id, lines in lines_by_class.items():
                if class_id in self.target_classes:
                    # 언더샘플링 적용
                    ratio = sampling_ratios.get(class_id, 1.0)
                    num_to_keep = max(1, int(len(lines) * ratio))  # 최소 1개는 유지

                    if num_to_keep < len(lines):
                        selected_lines = random.sample(lines, num_to_keep)
                    else:
                        selected_lines = lines

                    new_lines.extend(selected_lines)
                    stats[class_id]['after'] += len(selected_lines)
                else:
                    # 다른 클래스는 모두 유지
                    new_lines.extend(lines)
                    stats[class_id]['after'] += len(lines)

            # 파일 저장
            if new_lines:
                # 이미지 복사 (심볼릭 링크로 공간 절약)
                import shutil
                shutil.copy2(image_file, output_images_dir / image_file.name)

                # 라벨 저장
                with open(output_labels_dir / label_file.name, 'w') as f:
                    f.writelines(new_lines)

                processed_images += 1

        print(f"\n  완료: {processed_images:,}개 이미지 처리")

        # 클래스별 통계 출력
        print(f"\n  클래스별 객체 수 변화:")
        for class_id in sorted(stats.keys()):
            before = stats[class_id]['before']
            after = stats[class_id]['after']
            diff = before - after
            if before > 0:
                reduction = (1 - after/before) * 100
                print(f"    클래스 {class_id:2d}: {before:6,}개 → {after:6,}개 (-{diff:6,}개, -{reduction:5.1f}%)")

        return stats

    def analyze_final_distribution(self):
        """최종 데이터셋 분포 분석"""
        print("\n" + "="*80)
        print("최종 데이터셋 불균형 분석")
        print("="*80)

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
                # 불균형 비율
                max_count = max(class_counts.values())
                min_count = min(class_counts.values())
                avg_count = sum(class_counts.values()) / len(class_counts)

                print(f"\n  📊 불균형 통계:")
                print(f"    최대 샘플: {max_count:,}개")
                print(f"    최소 샘플: {min_count:,}개")
                print(f"    평균 샘플: {avg_count:,.0f}개")
                print(f"    불균형 비율: {max_count/min_count:.1f}:1")

                # 상위/하위 클래스
                sorted_classes = sorted(class_counts.items(), key=lambda x: x[1], reverse=True)

                print(f"\n  상위 5개 클래스:")
                for class_id, count in sorted_classes[:5]:
                    print(f"    클래스 {class_id:2d}: {count:6,}개")

                print(f"\n  하위 5개 클래스:")
                for class_id, count in sorted_classes[-5:]:
                    print(f"    클래스 {class_id:2d}: {count:6,}개")

    def create_data_yaml(self):
        """data.yaml 복사"""
        print("\ndata.yaml 복사 중...")

        import shutil
        src_yaml = self.base_dir / 'data.yaml'
        dst_yaml = self.output_dir / 'data.yaml'

        # 경로만 수정해서 복사
        with open(src_yaml, 'r') as f:
            data = yaml.safe_load(f)

        data['path'] = str(self.output_dir.absolute())

        with open(dst_yaml, 'w') as f:
            yaml.dump(data, f, default_flow_style=False, sort_keys=False)

        print(f"  저장 완료: {dst_yaml}")

    def run(self):
        """전체 프로세스 실행"""
        print("="*80)
        print("객체 단위 언더샘플링 시작 (전략 A)")
        print("="*80)
        print(f"\n입력: {self.base_dir}")
        print(f"출력: {self.output_dir}")
        print(f"\n목표:")
        for class_id, target in self.target_classes.items():
            print(f"  클래스 {class_id}: {target:,}개로 제한")

        # 출력 디렉토리 생성
        self.output_dir.mkdir(parents=True, exist_ok=True)

        # 각 split 처리
        all_stats = {}
        for split in ['train', 'val', 'test']:
            all_stats[split] = self.process_split(split)

        # data.yaml 복사
        self.create_data_yaml()

        # 최종 통계
        self.analyze_final_distribution()

        print("\n" + "="*80)
        print("객체 단위 언더샘플링 완료!")
        print("="*80)
        print(f"\n새 데이터셋 경로: {self.output_dir}")
        print(f"새 data.yaml: {self.output_dir / 'data.yaml'}")

if __name__ == "__main__":
    undersampler = ObjectLevelUndersampler()
    undersampler.run()
