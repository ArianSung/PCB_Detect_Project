#!/usr/bin/env python3
"""
Roboflow PCB 불량 검출 데이터셋 다운로드 및 분석 스크립트
클래스 분포를 분석하여 데이터 불균형 문제를 파악합니다.
"""

import os
import sys
import yaml
from pathlib import Path
from collections import Counter
from roboflow import Roboflow

# 프로젝트 루트 경로
PROJECT_ROOT = Path(__file__).parent.parent
DATA_DIR = PROJECT_ROOT / "data" / "raw" / "roboflow_pcb"

def download_dataset():
    """Roboflow에서 PCB 데이터셋 다운로드"""
    print("=" * 80)
    print("Roboflow PCB 불량 검출 데이터셋 다운로드")
    print("=" * 80)

    # Roboflow API 초기화
    rf = Roboflow(api_key="4EdUhVZ6LgtN2FvlVWNW")
    project = rf.workspace("arian-qfo7y").project("pcb_defect_detect-nitz0")
    version = project.version(1)

    # 데이터셋 다운로드
    print(f"\n데이터셋 다운로드 위치: {DATA_DIR}")
    dataset = version.download("yolov11", location=str(DATA_DIR))

    print(f"\n✓ 데이터셋 다운로드 완료: {dataset.location}")
    return dataset

def analyze_class_distribution(dataset_path):
    """클래스 분포 분석"""
    print("\n" + "=" * 80)
    print("클래스 분포 분석")
    print("=" * 80)

    # data.yaml 파일 읽기
    data_yaml_path = Path(dataset_path) / "data.yaml"
    if not data_yaml_path.exists():
        print(f"❌ data.yaml 파일을 찾을 수 없습니다: {data_yaml_path}")
        return

    with open(data_yaml_path, 'r') as f:
        data_config = yaml.safe_load(f)

    # 클래스 정보 출력
    class_names = data_config.get('names', [])
    num_classes = data_config.get('nc', len(class_names))

    print(f"\n총 클래스 수: {num_classes}")
    print(f"클래스 목록: {class_names}")

    # 각 split별 클래스 분포 분석
    for split in ['train', 'valid', 'test']:
        split_path = Path(dataset_path) / split
        if not split_path.exists():
            continue

        labels_path = split_path / 'labels'
        if not labels_path.exists():
            continue

        # 라벨 파일에서 클래스 카운트
        class_counter = Counter()
        total_objects = 0

        for label_file in labels_path.glob('*.txt'):
            with open(label_file, 'r') as f:
                for line in f:
                    parts = line.strip().split()
                    if len(parts) >= 5:  # YOLO 형식: class x y w h
                        class_id = int(parts[0])
                        class_counter[class_id] += 1
                        total_objects += 1

        # 결과 출력
        print(f"\n【{split.upper()} 세트】")
        print(f"  총 객체 수: {total_objects:,}")
        print(f"  클래스별 분포:")

        # 클래스 ID별로 정렬하여 출력
        for class_id in sorted(class_counter.keys()):
            count = class_counter[class_id]
            percentage = (count / total_objects * 100) if total_objects > 0 else 0
            class_name = class_names[class_id] if class_id < len(class_names) else f"Class_{class_id}"
            print(f"    [{class_id:2d}] {class_name:20s}: {count:5,} ({percentage:5.2f}%)")

        # 가장 많은 클래스 찾기
        if class_counter:
            most_common_class_id, most_common_count = class_counter.most_common(1)[0]
            most_common_name = class_names[most_common_class_id] if most_common_class_id < len(class_names) else f"Class_{most_common_class_id}"
            most_common_percentage = (most_common_count / total_objects * 100) if total_objects > 0 else 0

            print(f"\n  ⚠️  가장 많은 클래스: [{most_common_class_id}] {most_common_name} - {most_common_count:,}개 ({most_common_percentage:.2f}%)")

            # 불균형 정도 평가
            if most_common_percentage > 50:
                print(f"  ❌ 심각한 클래스 불균형 감지! ({most_common_percentage:.1f}%가 단일 클래스)")
            elif most_common_percentage > 30:
                print(f"  ⚠️  클래스 불균형 주의 필요 ({most_common_percentage:.1f}%가 단일 클래스)")
            else:
                print(f"  ✓ 클래스 분포가 비교적 균형적입니다.")

def generate_training_recommendations(dataset_path):
    """학습 권장사항 생성"""
    print("\n" + "=" * 80)
    print("과적합 방지를 위한 학습 권장사항")
    print("=" * 80)

    recommendations = [
        "\n【데이터 증강 (Data Augmentation)】",
        "  - hsv_h: 0.015      # 색조 변화",
        "  - hsv_s: 0.7        # 채도 변화",
        "  - hsv_v: 0.4        # 명도 변화",
        "  - degrees: 10       # 회전 (±10도)",
        "  - translate: 0.1    # 이동",
        "  - scale: 0.5        # 스케일",
        "  - shear: 0.0        # 전단 변형",
        "  - perspective: 0.0  # 원근 변형",
        "  - flipud: 0.0       # 상하 반전 (PCB는 비권장)",
        "  - fliplr: 0.5       # 좌우 반전",
        "  - mosaic: 1.0       # Mosaic 증강",
        "  - mixup: 0.0        # Mixup 증강",
        "  - copy_paste: 0.0   # Copy-Paste 증강",
        "",
        "【정규화 (Regularization)】",
        "  - dropout: 0.0      # Dropout (YOLO는 기본 0)",
        "  - weight_decay: 0.0005  # L2 정규화",
        "",
        "【클래스 가중치 (Class Weights)】",
        "  - 저항 클래스의 loss weight를 낮추고, 소수 클래스의 weight를 높임",
        "  - YOLO에서는 자동으로 클래스 빈도에 반비례하는 가중치 사용 가능",
        "",
        "【조기 종료 (Early Stopping)】",
        "  - patience: 50      # 50 에폭 동안 개선 없으면 종료",
        "",
        "【학습률 스케줄링】",
        "  - lr0: 0.001        # 초기 학습률 (AdamW)",
        "  - lrf: 0.01         # 최종 학습률 (lr0 * lrf)",
        "  - warmup_epochs: 3  # 워밍업 에폭",
        "",
        "【배치 크기 및 에폭】",
        "  - batch: 16         # RTX 4080 Super에 적합",
        "  - epochs: 100       # 충분한 학습",
        "  - imgsz: 640        # 이미지 크기",
        "",
        "【모델 선택】",
        "  - YOLOv11l 사용 (Large 모델로 복잡한 PCB 패턴 학습)",
        "  - Pretrained weights 사용 (전이 학습)",
        "",
        "【검증 전략】",
        "  - K-Fold Cross Validation (k=5) 고려",
        "  - Stratified split으로 클래스 비율 유지",
    ]

    for line in recommendations:
        print(line)

def create_training_config(dataset_path):
    """학습 설정 파일 생성"""
    config_path = PROJECT_ROOT / "configs" / "roboflow_pcb_training.yaml"
    config_path.parent.mkdir(parents=True, exist_ok=True)

    # data.yaml에서 정보 읽기
    data_yaml_path = Path(dataset_path) / "data.yaml"
    with open(data_yaml_path, 'r') as f:
        data_config = yaml.safe_load(f)

    training_config = {
        "model": "yolo11l.pt",
        "data": str(data_yaml_path),
        "epochs": 100,
        "batch_size": 16,
        "image_size": 640,
        "device": 0,
        "project": "runs/detect",
        "name": "roboflow_pcb_yolo11l",
        "exist_ok": True,
        "pretrained": True,
        "optimizer": "AdamW",
        "lr0": 0.001,
        "lrf": 0.01,
        "weight_decay": 0.0005,
        "patience": 50,
        "amp": True,
        "verbose": True,
        "plots": True,
        "cache": False,
        "workers": 8,
        # Data Augmentation
        "hsv_h": 0.015,
        "hsv_s": 0.7,
        "hsv_v": 0.4,
        "degrees": 10,
        "translate": 0.1,
        "scale": 0.5,
        "shear": 0.0,
        "perspective": 0.0,
        "flipud": 0.0,
        "fliplr": 0.5,
        "mosaic": 1.0,
        "mixup": 0.0,
        "copy_paste": 0.0,
    }

    with open(config_path, 'w') as f:
        yaml.dump(training_config, f, default_flow_style=False, sort_keys=False)

    print(f"\n✓ 학습 설정 파일 생성: {config_path}")

def main():
    """메인 함수"""
    # 데이터셋 다운로드
    dataset = download_dataset()

    # 클래스 분포 분석
    analyze_class_distribution(dataset.location)

    # 학습 권장사항 출력
    generate_training_recommendations(dataset.location)

    # 학습 설정 파일 생성
    create_training_config(dataset.location)

    print("\n" + "=" * 80)
    print("✓ 데이터셋 분석 완료")
    print("=" * 80)

if __name__ == "__main__":
    main()
