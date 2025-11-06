#!/usr/bin/env python3
"""
학습 성능 저하 원인 분석 스크립트
"""

import os
import yaml
from pathlib import Path
from collections import defaultdict

def analyze_dataset_distribution():
    """데이터셋 클래스 분포 분석"""
    print("=" * 80)
    print("1. 데이터셋 클래스 분포 분석")
    print("=" * 80)

    base_dir = Path("/home/sys1041/work_project/data/processed/complete_pcb_model")

    # 각 split별 클래스 분포
    for split in ['train', 'val', 'test']:
        labels_dir = base_dir / split / 'labels'
        if not labels_dir.exists():
            continue

        class_counts = defaultdict(int)
        total_objects = 0
        total_images = 0

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
        print(f"  총 이미지: {total_images}")
        print(f"  총 객체: {total_objects}")
        print(f"  평균 객체/이미지: {total_objects/total_images if total_images > 0 else 0:.2f}")

        # 클래스별 분포
        if class_counts:
            sorted_classes = sorted(class_counts.items(), key=lambda x: x[1], reverse=True)
            print(f"\n  클래스별 분포 (상위 10개):")
            for class_id, count in sorted_classes[:10]:
                print(f"    클래스 {class_id}: {count:,}개")

            # 극소 샘플 클래스
            min_threshold = 50
            rare_classes = [c for c, cnt in class_counts.items() if cnt < min_threshold]
            if rare_classes:
                print(f"\n  ⚠️ {split} set에서 {min_threshold}개 미만 클래스:")
                for class_id in rare_classes:
                    print(f"    클래스 {class_id}: {class_counts[class_id]}개")

def analyze_class_imbalance():
    """클래스 불균형 심각도 분석"""
    print("\n" + "=" * 80)
    print("2. 클래스 불균형 분석")
    print("=" * 80)

    train_labels_dir = Path("/home/sys1041/work_project/data/processed/complete_pcb_model/train/labels")

    class_counts = defaultdict(int)
    for label_file in train_labels_dir.glob('*.txt'):
        with open(label_file, 'r') as f:
            for line in f:
                parts = line.strip().split()
                if len(parts) >= 5:
                    class_id = int(parts[0])
                    class_counts[class_id] += 1

    if not class_counts:
        print("클래스 카운트를 찾을 수 없습니다.")
        return

    max_count = max(class_counts.values())
    min_count = min(class_counts.values())

    print(f"\n최대 샘플 수: {max_count:,}개")
    print(f"최소 샘플 수: {min_count:,}개")
    print(f"불균형 비율: {max_count/min_count:.1f}:1")

    # 심각한 불균형 클래스
    imbalance_ratio = 10  # 10배 이상 차이
    avg_count = sum(class_counts.values()) / len(class_counts)

    underrepresented = {c: cnt for c, cnt in class_counts.items() if cnt < avg_count / imbalance_ratio}
    if underrepresented:
        print(f"\n⚠️ 평균의 1/{imbalance_ratio} 미만 클래스 ({len(underrepresented)}개):")
        for class_id, count in sorted(underrepresented.items(), key=lambda x: x[1]):
            print(f"  클래스 {class_id}: {count:,}개 (평균: {avg_count:.0f}개)")

def check_data_yaml():
    """data.yaml 설정 확인"""
    print("\n" + "=" * 80)
    print("3. data.yaml 설정 확인")
    print("=" * 80)

    yaml_path = Path("/home/sys1041/work_project/data/processed/complete_pcb_model/data.yaml")

    with open(yaml_path, 'r') as f:
        data_config = yaml.safe_load(f)

    print(f"\n클래스 수: {data_config.get('nc', 'Unknown')}")
    print(f"클래스 이름 ({len(data_config.get('names', []))}개):")

    names = data_config.get('names', [])
    for i, name in enumerate(names):
        print(f"  {i}: {name}")

def analyze_training_curves():
    """학습 곡선 분석"""
    print("\n" + "=" * 80)
    print("4. 학습 곡선 분석")
    print("=" * 80)

    results_path = Path("/home/sys1041/work_project/runs/detect/complete_pcb_final_29classes_optimized/results.csv")

    import csv
    epochs = []
    map50_values = []

    with open(results_path, 'r') as f:
        reader = csv.DictReader(f)
        for row in reader:
            epochs.append(int(float(row['epoch'])))
            map50_values.append(float(row['metrics/mAP50(B)']))

    # 주요 마일스톤
    milestones = [1, 10, 20, 30, 50, 70, 100, len(epochs)]

    print("\nmAP50 진행 상황:")
    for milestone in milestones:
        if milestone <= len(epochs):
            epoch_idx = milestone - 1
            map50 = map50_values[epoch_idx]
            if epoch_idx > 0:
                prev_map50 = map50_values[epoch_idx - 10] if epoch_idx >= 10 else map50_values[0]
                improvement = map50 - prev_map50
                print(f"  Epoch {milestone:3d}: {map50:.4f} (+{improvement:+.4f})")
            else:
                print(f"  Epoch {milestone:3d}: {map50:.4f}")

    # plateau 감지
    last_20_epochs = map50_values[-20:]
    variation = max(last_20_epochs) - min(last_20_epochs)
    print(f"\n최근 20 에포크 변동폭: {variation:.4f}")
    if variation < 0.01:
        print("⚠️ Plateau 상태 감지! (변동폭 < 1%)")

def suggest_improvements():
    """개선 방안 제시"""
    print("\n" + "=" * 80)
    print("5. 개선 방안 제시")
    print("=" * 80)

    suggestions = [
        "1. 데이터셋 품질 개선",
        "   - Validation set 수동 검증",
        "   - 라벨링 오류 수정",
        "   - 애매한 샘플 제거",
        "",
        "2. 클래스 불균형 해결",
        "   - 극소 클래스 추가 증강",
        "   - Class weights 적용",
        "   - Focal Loss 적용",
        "",
        "3. 하이퍼파라미터 조정",
        "   - Learning rate 감소 (0.001 → 0.0005)",
        "   - Batch size 증가 (16 → 32)",
        "   - Image size 증가 (640 → 1280)",
        "",
        "4. 모델 변경",
        "   - YOLOv8x (Extra Large) 사용",
        "   - 사전학습 모델 변경",
        "",
        "5. 데이터 증강 조정",
        "   - Mosaic 비율 감소 (1.0 → 0.5)",
        "   - Copy-paste 증가 (0.1 → 0.3)",
        "   - MixUp 조정",
    ]

    for suggestion in suggestions:
        print(suggestion)

if __name__ == "__main__":
    print("\n학습 성능 저하 원인 분석 시작...\n")

    analyze_dataset_distribution()
    analyze_class_imbalance()
    check_data_yaml()
    analyze_training_curves()
    suggest_improvements()

    print("\n" + "=" * 80)
    print("분석 완료!")
    print("=" * 80 + "\n")
