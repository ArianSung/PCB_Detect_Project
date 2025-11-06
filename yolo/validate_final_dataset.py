#!/usr/bin/env python3
"""
최종 데이터셋 검증 스크립트
- 파일 무결성 확인
- 라벨 형식 검증
- 클래스 분포 확인
- 학습 준비 상태 점검
"""

import os
import yaml
from pathlib import Path
from collections import defaultdict

class DatasetValidator:
    def __init__(self):
        self.dataset_dir = Path("/home/sys1041/work_project/data/processed/final_balanced_pcb_model")
        self.data_yaml = self.dataset_dir / "data.yaml"
        self.errors = []
        self.warnings = []

    def load_config(self):
        """data.yaml 로드"""
        print("="*80)
        print("1. data.yaml 설정 확인")
        print("="*80)

        if not self.data_yaml.exists():
            self.errors.append(f"❌ data.yaml 파일이 없습니다: {self.data_yaml}")
            return None

        with open(self.data_yaml, 'r') as f:
            config = yaml.safe_load(f)

        print(f"\n✅ data.yaml 로드 성공")
        print(f"  경로: {config.get('path', 'N/A')}")
        print(f"  클래스 수: {config.get('nc', 'N/A')}")
        print(f"  Train: {config.get('train', 'N/A')}")
        print(f"  Val: {config.get('val', 'N/A')}")
        print(f"  Test: {config.get('test', 'N/A')}")

        # 경로 확인
        if config.get('path') != str(self.dataset_dir):
            self.warnings.append(f"⚠️ data.yaml의 path가 현재 디렉토리와 다릅니다")

        return config

    def validate_file_pairs(self, split):
        """이미지-라벨 파일 쌍 검증"""
        images_dir = self.dataset_dir / split / 'images'
        labels_dir = self.dataset_dir / split / 'labels'

        if not images_dir.exists():
            self.errors.append(f"❌ {split} 이미지 디렉토리 없음: {images_dir}")
            return 0, 0, 0

        if not labels_dir.exists():
            self.errors.append(f"❌ {split} 라벨 디렉토리 없음: {labels_dir}")
            return 0, 0, 0

        # 이미지 파일 목록
        image_files = set()
        for ext in ['*.jpg', '*.png', '*.jpeg']:
            image_files.update([f.stem for f in images_dir.glob(ext)])

        # 라벨 파일 목록
        label_files = set([f.stem for f in labels_dir.glob('*.txt')])

        # 매칭 확인
        orphan_images = image_files - label_files
        orphan_labels = label_files - image_files
        matched = image_files & label_files

        if orphan_images:
            self.warnings.append(f"⚠️ {split}: 라벨이 없는 이미지 {len(orphan_images)}개")

        if orphan_labels:
            self.warnings.append(f"⚠️ {split}: 이미지가 없는 라벨 {len(orphan_labels)}개")

        return len(matched), len(orphan_images), len(orphan_labels)

    def validate_labels(self, split, config):
        """라벨 파일 형식 검증"""
        labels_dir = self.dataset_dir / split / 'labels'

        if not labels_dir.exists():
            return {}

        class_counts = defaultdict(int)
        invalid_count = 0
        total_objects = 0
        empty_files = 0

        nc = config.get('nc', 22)

        for label_file in labels_dir.glob('*.txt'):
            with open(label_file, 'r') as f:
                lines = f.readlines()

            if not lines:
                empty_files += 1
                continue

            for line_num, line in enumerate(lines, 1):
                parts = line.strip().split()

                if len(parts) < 5:
                    invalid_count += 1
                    self.errors.append(f"❌ {split}/{label_file.name}:{line_num} - 형식 오류 (필드 부족)")
                    continue

                try:
                    class_id = int(parts[0])
                    x, y, w, h = map(float, parts[1:5])

                    # 클래스 ID 범위 확인
                    if class_id < 0 or class_id >= nc:
                        invalid_count += 1
                        self.errors.append(f"❌ {split}/{label_file.name}:{line_num} - 잘못된 클래스 ID: {class_id}")
                        continue

                    # 좌표 범위 확인 (0-1 정규화)
                    if not (0 <= x <= 1 and 0 <= y <= 1 and 0 <= w <= 1 and 0 <= h <= 1):
                        invalid_count += 1
                        self.errors.append(f"❌ {split}/{label_file.name}:{line_num} - 잘못된 좌표 범위")
                        continue

                    class_counts[class_id] += 1
                    total_objects += 1

                except ValueError as e:
                    invalid_count += 1
                    self.errors.append(f"❌ {split}/{label_file.name}:{line_num} - 변환 오류: {e}")

        return {
            'class_counts': class_counts,
            'total_objects': total_objects,
            'invalid_count': invalid_count,
            'empty_files': empty_files,
        }

    def print_class_distribution(self, split, stats, config):
        """클래스별 분포 출력"""
        class_counts = stats.get('class_counts', {})

        if not class_counts:
            return

        names = config.get('names', [])
        sorted_classes = sorted(class_counts.items(), key=lambda x: x[1], reverse=True)

        print(f"\n  클래스별 분포:")
        for class_id, count in sorted_classes:
            class_name = names[class_id] if class_id < len(names) else f"Unknown_{class_id}"
            print(f"    {class_id:2d}. {class_name:22s}: {count:6,}개")

        # 불균형 비율
        if len(class_counts) > 1:
            max_count = max(class_counts.values())
            min_count = min(class_counts.values())
            print(f"\n  불균형 비율: {max_count/min_count:.1f}:1")
            print(f"    최대 샘플: {max_count:,}개")
            print(f"    최소 샘플: {min_count:,}개")
            print(f"    평균 샘플: {sum(class_counts.values())/len(class_counts):,.0f}개")

    def check_training_readiness(self, all_stats):
        """학습 준비 상태 점검"""
        print("\n" + "="*80)
        print("5. 학습 준비 상태 점검")
        print("="*80)

        ready = True

        # 최소 샘플 수 확인 (각 split별)
        for split in ['train', 'val', 'test']:
            stats = all_stats.get(split, {})
            class_counts = stats.get('class_counts', {})

            if not class_counts:
                ready = False
                self.errors.append(f"❌ {split} set에 데이터가 없습니다")
                continue

            min_samples = min(class_counts.values())
            if min_samples < 10:
                self.warnings.append(f"⚠️ {split} set에 샘플이 10개 미만인 클래스가 있습니다 (최소: {min_samples}개)")

        # Train set 크기 확인
        train_stats = all_stats.get('train', {})
        train_total = train_stats.get('total_objects', 0)
        if train_total < 1000:
            self.warnings.append(f"⚠️ Train set 객체 수가 너무 적습니다: {train_total}개")

        # Val set 크기 확인
        val_stats = all_stats.get('val', {})
        val_total = val_stats.get('total_objects', 0)
        if val_total < 100:
            self.warnings.append(f"⚠️ Validation set 객체 수가 너무 적습니다: {val_total}개")

        if self.errors:
            print(f"\n❌ 심각한 오류 {len(self.errors)}개:")
            for error in self.errors[:10]:  # 최대 10개만 표시
                print(f"  {error}")
            if len(self.errors) > 10:
                print(f"  ... 외 {len(self.errors) - 10}개")
            ready = False

        if self.warnings:
            print(f"\n⚠️ 경고 {len(self.warnings)}개:")
            for warning in self.warnings[:10]:
                print(f"  {warning}")
            if len(self.warnings) > 10:
                print(f"  ... 외 {len(self.warnings) - 10}개")

        if ready and not self.errors:
            print("\n✅ 학습 준비 완료!")
            print(f"  총 오류: 0개")
            print(f"  총 경고: {len(self.warnings)}개")
        else:
            print("\n❌ 학습 준비 미완료 - 오류 수정 필요")

        return ready

    def run(self):
        """전체 검증 실행"""
        print("\n" + "="*80)
        print("최종 데이터셋 검증 시작")
        print("="*80)
        print(f"데이터셋 경로: {self.dataset_dir}\n")

        # 1. data.yaml 검증
        config = self.load_config()
        if config is None:
            print("\n❌ data.yaml을 로드할 수 없어 검증을 중단합니다.")
            return False

        all_stats = {}

        # 2-4. 각 split 검증
        for split in ['train', 'val', 'test']:
            print("\n" + "="*80)
            print(f"{split.upper()} SET 검증")
            print("="*80)

            # 파일 쌍 검증
            matched, orphan_images, orphan_labels = self.validate_file_pairs(split)
            print(f"\n  파일 매칭:")
            print(f"    ✅ 정상 쌍: {matched:,}개")
            if orphan_images > 0:
                print(f"    ⚠️ 라벨 없는 이미지: {orphan_images}개")
            if orphan_labels > 0:
                print(f"    ⚠️ 이미지 없는 라벨: {orphan_labels}개")

            # 라벨 형식 검증
            stats = self.validate_labels(split, config)
            all_stats[split] = stats

            if stats:
                print(f"\n  라벨 통계:")
                print(f"    총 객체: {stats['total_objects']:,}개")
                print(f"    빈 라벨 파일: {stats['empty_files']}개")
                if stats['invalid_count'] > 0:
                    print(f"    ❌ 오류 라벨: {stats['invalid_count']}개")

                # 클래스 분포
                self.print_class_distribution(split, stats, config)

        # 5. 학습 준비 상태
        ready = self.check_training_readiness(all_stats)

        print("\n" + "="*80)
        print("검증 완료")
        print("="*80)

        return ready

if __name__ == "__main__":
    validator = DatasetValidator()
    ready = validator.run()

    if ready:
        print("\n🎉 데이터셋이 학습 준비 완료 상태입니다!")
        print(f"\n다음 명령으로 학습을 시작할 수 있습니다:")
        print(f"  cd /home/sys1041/work_project")
        print(f"  yolo detect train data=data/processed/final_balanced_pcb_model/data.yaml \\")
        print(f"    model=yolov8l.pt epochs=150 imgsz=640 batch=32 device=0")
    else:
        print("\n⚠️ 오류를 수정한 후 다시 검증해주세요.")
