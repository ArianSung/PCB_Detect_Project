#!/usr/bin/env python3
"""
클래스별 가중치 계산 스크립트
클래스 불균형 문제 해결을 위한 가중치 자동 계산
"""

import yaml
import numpy as np
from pathlib import Path
from collections import Counter

def calculate_class_weights(data_yaml_path, method='inverse_freq', power=1.0):
    """
    클래스 가중치 계산

    Args:
        data_yaml_path: data.yaml 경로
        method: 'inverse_freq' (역빈도) 또는 'balanced' (sklearn 방식)
        power: 가중치 강도 조절 (1.0=기본, 0.5=완화, 2.0=강화)

    Returns:
        dict: {class_id: weight}
    """
    data_yaml_path = Path(data_yaml_path)

    # data.yaml 읽기
    with open(data_yaml_path, 'r') as f:
        data_config = yaml.safe_load(f)

    class_names = data_config.get('names', [])
    num_classes = len(class_names)

    # Train 세트에서 클래스 분포 계산
    train_labels_path = data_yaml_path.parent / 'train' / 'labels'

    class_counter = Counter()
    total_objects = 0

    for label_file in train_labels_path.glob('*.txt'):
        with open(label_file, 'r') as f:
            for line in f:
                parts = line.strip().split()
                if len(parts) >= 5:
                    class_id = int(parts[0])
                    class_counter[class_id] += 1
                    total_objects += 1

    print("=" * 80)
    print("클래스별 샘플 수")
    print("=" * 80)

    class_counts = []
    for class_id in range(num_classes):
        count = class_counter.get(class_id, 0)
        class_counts.append(count)
        percentage = (count / total_objects * 100) if total_objects > 0 else 0
        print(f"  [{class_id:2d}] {class_names[class_id]:20s}: {count:5,} ({percentage:5.2f}%)")

    # 가중치 계산
    class_counts = np.array(class_counts, dtype=float)

    if method == 'inverse_freq':
        # 역빈도 가중치
        # weight_i = (total / n_classes) / count_i
        weights = (total_objects / num_classes) / (class_counts + 1e-6)  # 0으로 나누기 방지

    elif method == 'balanced':
        # sklearn 스타일 balanced 가중치
        # weight_i = total / (n_classes * count_i)
        weights = total_objects / (num_classes * (class_counts + 1e-6))

    else:
        raise ValueError(f"Unknown method: {method}")

    # 가중치 강도 조절
    if power != 1.0:
        weights = weights ** power

    # 정규화 (평균=1.0)
    weights = weights / weights.mean()

    # 결과 출력
    print("\n" + "=" * 80)
    print(f"클래스 가중치 (method={method}, power={power})")
    print("=" * 80)

    class_weights = {}
    for class_id in range(num_classes):
        weight = weights[class_id]
        class_weights[class_id] = float(weight)

        # 가중치 시각화
        bar_length = int(weight * 20)
        bar = '█' * bar_length

        print(f"  [{class_id:2d}] {class_names[class_id]:20s}: {weight:6.3f} {bar}")

    # 권장사항
    print("\n" + "=" * 80)
    print("해석 및 권장사항")
    print("=" * 80)

    max_weight_class = np.argmax(weights)
    min_weight_class = np.argmin(weights)

    print(f"\n✓ 가장 높은 가중치 (소수 클래스):")
    print(f"   [{max_weight_class}] {class_names[max_weight_class]}: {weights[max_weight_class]:.3f}x")
    print(f"   → 모델이 이 클래스에 {weights[max_weight_class]:.1f}배 더 집중합니다")

    print(f"\n✓ 가장 낮은 가중치 (다수 클래스):")
    print(f"   [{min_weight_class}] {class_names[min_weight_class]}: {weights[min_weight_class]:.3f}x")
    print(f"   → 과적합을 방지하기 위해 기여도를 {1/weights[min_weight_class]:.1f}배 감소시킵니다")

    print(f"\n✓ 가중치 범위: {weights.min():.3f} ~ {weights.max():.3f} (평균=1.0)")

    if weights.max() / weights.min() > 10:
        print(f"\n⚠️  가중치 차이가 매우 큽니다 ({weights.max()/weights.min():.1f}배)")
        print(f"   → power 파라미터를 낮춰서 완화하는 것을 권장합니다 (예: power=0.5)")

    return class_weights

def generate_loss_file(class_weights, output_path):
    """
    YOLO용 클래스 가중치 파일 생성

    Args:
        class_weights: {class_id: weight} 딕셔너리
        output_path: 출력 파일 경로
    """
    output_path = Path(output_path)
    output_path.parent.mkdir(parents=True, exist_ok=True)

    # YAML 형식으로 저장
    config = {
        'class_weights': class_weights,
        'description': 'Class weights for imbalanced dataset',
        'usage': 'Use with YOLO training by setting cls_weight parameter'
    }

    with open(output_path, 'w') as f:
        yaml.dump(config, f, default_flow_style=False, sort_keys=False)

    print(f"\n✓ 클래스 가중치 파일 저장: {output_path}")

    # Python dict 형식으로도 출력
    print("\n" + "=" * 80)
    print("Python dict 형식 (코드에 직접 사용)")
    print("=" * 80)
    print("class_weights = {")
    for class_id, weight in class_weights.items():
        print(f"    {class_id}: {weight:.6f},")
    print("}")

def main():
    import argparse

    parser = argparse.ArgumentParser(description="Calculate class weights for imbalanced dataset")
    parser.add_argument("--data", type=str,
                       default="/home/sys1041/work_project/data/raw/roboflow_pcb/data.yaml",
                       help="Path to data.yaml")
    parser.add_argument("--method", type=str, default="inverse_freq",
                       choices=['inverse_freq', 'balanced'],
                       help="Weight calculation method")
    parser.add_argument("--power", type=float, default=0.5,
                       help="Weight strength (1.0=full, 0.5=moderate, 0.25=mild)")
    parser.add_argument("--output", type=str,
                       default="/home/sys1041/work_project/configs/class_weights.yaml",
                       help="Output path for weights file")

    args = parser.parse_args()

    # 가중치 계산
    class_weights = calculate_class_weights(args.data, args.method, args.power)

    # 파일로 저장
    generate_loss_file(class_weights, args.output)

    print("\n" + "=" * 80)
    print("완료!")
    print("=" * 80)

if __name__ == "__main__":
    main()
