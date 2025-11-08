#!/usr/bin/env python3
"""
Roboflow PCB 불량 검출 모델 학습 스크립트 (YOLOv11l)
저항 클래스 과적합 방지를 위한 강화된 데이터 증강 및 정규화 적용
"""

import os
import yaml
from pathlib import Path
from ultralytics import YOLO

# 프로젝트 루트 경로
PROJECT_ROOT = Path(__file__).parent.parent
CONFIG_PATH = PROJECT_ROOT / "configs" / "roboflow_pcb_training.yaml"
DATA_YAML = PROJECT_ROOT / "data" / "raw" / "roboflow_pcb" / "data.yaml"

def train_model(test_mode=False):
    """
    YOLOv11l 모델 학습

    Args:
        test_mode: True일 경우 5 epoch만 실행 (테스트용)
    """
    print("=" * 80)
    print("Roboflow PCB 불량 검출 모델 학습 (YOLOv11l)")
    print("=" * 80)

    # 설정 파일 로드
    with open(CONFIG_PATH, 'r') as f:
        config = yaml.safe_load(f)

    # 테스트 모드일 경우 epoch 줄이기
    if test_mode:
        print("\n⚠️  테스트 모드: 5 epoch만 실행합니다.")
        config['epochs'] = 5
        config['name'] = 'roboflow_pcb_yolo11l_test'

    # 과적합 방지를 위한 추가 설정
    print("\n【과적합 방지 설정】")
    print(f"  - MixUp 증강: {config.get('mixup', 0.0)} → 0.15 (활성화)")
    print(f"  - CopyPaste 증강: {config.get('copy_paste', 0.0)} → 0.1 (활성화)")
    print(f"  - 회전 각도: {config.get('degrees', 10)} → 15 (증가)")
    print(f"  - Early Stopping: patience={config.get('patience', 50)}")
    print(f"  - Weight Decay: {config.get('weight_decay', 0.0005)}")

    # 개선된 설정 적용
    config['mixup'] = 0.15      # MixUp 활성화 (클래스 혼합으로 과적합 방지)
    config['copy_paste'] = 0.1  # CopyPaste 활성화 (소수 클래스 보강)
    config['degrees'] = 15      # 회전 각도 증가

    # YOLO 모델 로드
    print(f"\n모델 로드: {config['model']}")
    model = YOLO(config['model'])

    # 데이터셋 경로 확인
    print(f"데이터셋: {DATA_YAML}")
    if not Path(DATA_YAML).exists():
        print(f"❌ 데이터셋 파일을 찾을 수 없습니다: {DATA_YAML}")
        return

    # GPU 확인
    import torch
    if torch.cuda.is_available():
        gpu_name = torch.cuda.get_device_name(0)
        gpu_memory = torch.cuda.get_device_properties(0).total_memory / 1024**3
        print(f"\n✓ GPU 사용 가능: {gpu_name} ({gpu_memory:.1f} GB)")
    else:
        print("\n⚠️  GPU를 사용할 수 없습니다. CPU로 학습합니다.")

    # 학습 시작
    print("\n" + "=" * 80)
    print("학습 시작")
    print("=" * 80)

    results = model.train(
        data=str(DATA_YAML),
        epochs=config['epochs'],
        batch=config['batch_size'],
        imgsz=config['image_size'],
        device=config['device'],
        project=config['project'],
        name=config['name'],
        exist_ok=config['exist_ok'],
        pretrained=config['pretrained'],
        optimizer=config['optimizer'],
        lr0=config['lr0'],
        lrf=config['lrf'],
        weight_decay=config['weight_decay'],
        patience=config['patience'],
        amp=config['amp'],
        verbose=config['verbose'],
        plots=config['plots'],
        cache=config['cache'],
        workers=config['workers'],
        # Data Augmentation (과적합 방지 강화)
        hsv_h=config['hsv_h'],
        hsv_s=config['hsv_s'],
        hsv_v=config['hsv_v'],
        degrees=config['degrees'],
        translate=config['translate'],
        scale=config['scale'],
        shear=config['shear'],
        perspective=config['perspective'],
        flipud=config['flipud'],
        fliplr=config['fliplr'],
        mosaic=config['mosaic'],
        mixup=config['mixup'],
        copy_paste=config['copy_paste'],
    )

    print("\n" + "=" * 80)
    print("✓ 학습 완료")
    print("=" * 80)

    # 결과 저장 위치 출력
    save_dir = Path(config['project']) / config['name']
    print(f"\n결과 저장 위치: {save_dir}")
    print(f"  - Best 모델: {save_dir / 'weights' / 'best.pt'}")
    print(f"  - Last 모델: {save_dir / 'weights' / 'last.pt'}")
    print(f"  - 학습 로그: {save_dir / 'results.csv'}")
    print(f"  - 플롯: {save_dir / '*.png'}")

    return results

def analyze_training_results(results_dir):
    """학습 결과 분석"""
    results_csv = Path(results_dir) / "results.csv"

    if not results_csv.exists():
        print(f"❌ 결과 파일을 찾을 수 없습니다: {results_csv}")
        return

    import pandas as pd
    df = pd.read_csv(results_csv)
    df.columns = df.columns.str.strip()  # 공백 제거

    print("\n" + "=" * 80)
    print("학습 결과 요약")
    print("=" * 80)

    # 최종 성능 지표
    last_row = df.iloc[-1]
    print(f"\n【최종 성능 (Epoch {int(last_row['epoch'])})】")
    print(f"  - mAP50: {last_row['metrics/mAP50(B)']:.4f}")
    print(f"  - mAP50-95: {last_row['metrics/mAP50-95(B)']:.4f}")
    print(f"  - Precision: {last_row['metrics/precision(B)']:.4f}")
    print(f"  - Recall: {last_row['metrics/recall(B)']:.4f}")

    # 최고 성능
    best_map50_idx = df['metrics/mAP50(B)'].idxmax()
    best_row = df.iloc[best_map50_idx]
    print(f"\n【최고 성능 (Epoch {int(best_row['epoch'])})】")
    print(f"  - mAP50: {best_row['metrics/mAP50(B)']:.4f}")
    print(f"  - mAP50-95: {best_row['metrics/mAP50-95(B)']:.4f}")

    # Loss 추세
    print(f"\n【Loss 추세】")
    print(f"  - 초기 Train Loss: {df.iloc[0]['train/box_loss']:.4f}")
    print(f"  - 최종 Train Loss: {last_row['train/box_loss']:.4f}")
    print(f"  - 초기 Val Loss: {df.iloc[0]['val/box_loss']:.4f}")
    print(f"  - 최종 Val Loss: {last_row['val/box_loss']:.4f}")

    # 과적합 여부 체크
    train_val_gap = abs(last_row['train/box_loss'] - last_row['val/box_loss'])
    if train_val_gap > 0.1:
        print(f"\n  ⚠️  Train/Val Loss 차이: {train_val_gap:.4f} (과적합 가능성)")
    else:
        print(f"\n  ✓ Train/Val Loss 차이: {train_val_gap:.4f} (정상)")

if __name__ == "__main__":
    import argparse

    parser = argparse.ArgumentParser(description="Train YOLOv11l on Roboflow PCB dataset")
    parser.add_argument("--test", action="store_true", help="테스트 모드 (5 epoch)")
    args = parser.parse_args()

    # 학습 실행
    results = train_model(test_mode=args.test)

    # 결과 분석
    if results:
        results_dir = Path("runs/detect") / ("roboflow_pcb_yolo11l_test" if args.test else "roboflow_pcb_yolo11l")
        if results_dir.exists():
            analyze_training_results(results_dir)
