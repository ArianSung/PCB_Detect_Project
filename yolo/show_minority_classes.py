#!/usr/bin/env python3
"""
샘플 수가 적은 클래스 현황 보고
"""

import yaml
from pathlib import Path
from collections import defaultdict

def analyze_minority_classes():
    """샘플이 적은 클래스 분석"""
    dataset_dir = Path("/home/sys1041/work_project/data/processed/final_balanced_pcb_model")
    data_yaml = dataset_dir / "data.yaml"

    # data.yaml 로드
    with open(data_yaml, 'r') as f:
        config = yaml.safe_load(f)

    class_names = config['names']

    print("="*100)
    print("샘플 수가 적은 클래스 분석 (22개 클래스)")
    print("="*100)

    all_class_counts = {}

    for split in ['train', 'val', 'test']:
        labels_dir = dataset_dir / split / 'labels'

        class_counts = defaultdict(int)
        for label_file in labels_dir.glob('*.txt'):
            with open(label_file, 'r') as f:
                for line in f:
                    parts = line.strip().split()
                    if len(parts) >= 5:
                        class_id = int(parts[0])
                        class_counts[class_id] += 1

        all_class_counts[split] = dict(class_counts)

    # 전체 클래스별 합계
    total_counts = defaultdict(int)
    for split_counts in all_class_counts.values():
        for class_id, count in split_counts.items():
            total_counts[class_id] += count

    # 샘플 수로 정렬 (적은 순)
    sorted_classes = sorted(total_counts.items(), key=lambda x: x[1])

    print("\n" + "="*100)
    print("📊 전체 클래스별 샘플 수 (적은 순서)")
    print("="*100)
    print(f"{'순위':<4} {'클래스ID':<8} {'클래스명':<25} {'Train':>8} {'Val':>8} {'Test':>8} {'Total':>8} {'비고':<20}")
    print("-"*100)

    for rank, (class_id, total) in enumerate(sorted_classes, 1):
        class_name = class_names[class_id]
        train_cnt = all_class_counts['train'].get(class_id, 0)
        val_cnt = all_class_counts['val'].get(class_id, 0)
        test_cnt = all_class_counts['test'].get(class_id, 0)

        # 비고 표시
        note = ""
        if total < 50:
            note = "⚠️ 매우 심각"
        elif total < 200:
            note = "⚠️ 심각"
        elif total < 500:
            note = "⚠️ 부족"
        elif total < 1000:
            note = "△ 주의"

        print(f"{rank:<4} {class_id:<8} {class_name:<25} {train_cnt:>8,} {val_cnt:>8,} {test_cnt:>8,} {total:>8,} {note:<20}")

    print("\n" + "="*100)
    print("⚠️ 샘플 부족 클래스 상세 분석")
    print("="*100)

    # 500개 미만 클래스만 상세 분석
    minority_threshold = 500
    minority_classes = [(cid, cnt) for cid, cnt in sorted_classes if cnt < minority_threshold]

    if minority_classes:
        print(f"\n총 {len(minority_classes)}개 클래스가 {minority_threshold}개 미만입니다:\n")

        for class_id, total in minority_classes:
            class_name = class_names[class_id]
            train_cnt = all_class_counts['train'].get(class_id, 0)
            val_cnt = all_class_counts['val'].get(class_id, 0)
            test_cnt = all_class_counts['test'].get(class_id, 0)

            print(f"클래스 {class_id:2d}: {class_name}")
            print(f"  Train: {train_cnt:4}개  Val: {val_cnt:3}개  Test: {test_cnt:3}개  Total: {total:5}개")

            # 학습 가능성 판단
            if val_cnt < 10:
                print(f"  ❌ Validation set 샘플이 {val_cnt}개로 매우 부족 - 성능 평가 불가능")
            elif val_cnt < 30:
                print(f"  ⚠️ Validation set 샘플이 {val_cnt}개로 부족 - 신뢰도 낮음")

            if train_cnt < 100:
                print(f"  ❌ Training set 샘플이 {train_cnt}개로 매우 부족 - 학습 어려움")
            elif train_cnt < 300:
                print(f"  ⚠️ Training set 샘플이 {train_cnt}개로 부족 - 성능 제한적")

            print()

    print("="*100)
    print("💡 권장 조치")
    print("="*100)

    # 매우 심각한 클래스 (total < 50)
    critical = [(cid, total_counts[cid]) for cid, _ in sorted_classes if total_counts[cid] < 50]
    if critical:
        print(f"\n1. 매우 심각 ({len(critical)}개 클래스) - 제거 또는 병합 권장:")
        for cid, cnt in critical:
            print(f"   - {class_names[cid]} (ID {cid}): {cnt}개")

    # 심각한 클래스 (50 <= total < 200)
    severe = [(cid, total_counts[cid]) for cid, _ in sorted_classes if 50 <= total_counts[cid] < 200]
    if severe:
        print(f"\n2. 심각 ({len(severe)}개 클래스) - 데이터 증강 필요:")
        for cid, cnt in severe:
            print(f"   - {class_names[cid]} (ID {cid}): {cnt}개 → 최소 500개 목표")

    # 부족한 클래스 (200 <= total < 500)
    insufficient = [(cid, total_counts[cid]) for cid, _ in sorted_classes if 200 <= total_counts[cid] < 500]
    if insufficient:
        print(f"\n3. 부족 ({len(insufficient)}개 클래스) - 모니터링 필요:")
        for cid, cnt in insufficient:
            print(f"   - {class_names[cid]} (ID {cid}): {cnt}개")

    print("\n" + "="*100)

if __name__ == "__main__":
    analyze_minority_classes()
