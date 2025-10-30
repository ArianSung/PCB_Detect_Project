#!/usr/bin/env python3
"""
DeepPCB 데이터셋으로 YOLO 학습

RTX 4080 Super 최적화 설정
"""

from ultralytics import YOLO
import torch
import os


def main():
    # GPU 확인
    if torch.cuda.is_available():
        print(f"CUDA available: {torch.cuda.is_available()}")
        print(f"GPU: {torch.cuda.get_device_name(0)}")
        print(f"CUDA version: {torch.version.cuda}")
        print(f"PyTorch version: {torch.__version__}")
        print(f"GPU Memory: {torch.cuda.get_device_properties(0).total_memory / 1024**3:.2f} GB")
    else:
        print("Warning: CUDA not available, training on CPU")

    # 데이터셋 경로
    data_yaml = '/home/sys1041/work_project/data/processed/combined_pcb_dataset/data.yaml'

    # 모델 로드 (사전 학습된 YOLOv8l)
    print("\nLoading YOLOv8l model...")
    model = YOLO('yolov8l.pt')

    # 학습 설정
    training_args = {
        'data': data_yaml,
        'epochs': 300,
        'batch': 16,  # VRAM 사용량 최적화
        'imgsz': 640,
        'device': 0,  # GPU 0
        'workers': 8,
        'optimizer': 'AdamW',
        'lr0': 0.001,
        'lrf': 0.01,
        'momentum': 0.937,
        'weight_decay': 0.001,  # 과적합 방지 (0.0005 → 0.001)
        'warmup_epochs': 3.0,
        'patience': 100,  # Early stopping 여유 증가 (50 → 100)
        'save': True,
        'project': 'runs/detect',
        'name': 'deeppcb_yolov8l',
        'exist_ok': True,
        'pretrained': True,
        'verbose': True,
        'seed': 42,
        'deterministic': True,
        'amp': True,  # FP16 mixed precision
        'plots': True,
        'dropout': 0.1,  # 과적합 방지 - Dropout 추가
        # Data augmentation (과적합 방지 강화)
        'hsv_h': 0.015,
        'hsv_s': 0.7,
        'hsv_v': 0.4,
        'degrees': 0.0,
        'translate': 0.1,
        'scale': 0.5,
        'shear': 0.0,
        'perspective': 0.0,
        'flipud': 0.0,
        'fliplr': 0.5,
        'mosaic': 1.0,
        'mixup': 0.15,  # 과적합 방지 - Mixup 활성화 (0.0 → 0.15)
        'copy_paste': 0.1,  # 과적합 방지 - Copy-paste 추가
        'close_mosaic': 20,  # 마지막 20 epoch에서 mosaic 비활성화
    }

    print("\n=== Training Configuration ===")
    for key, value in training_args.items():
        print(f"  {key}: {value}")

    print("\n=== Starting Training ===")
    print(f"Dataset: {data_yaml}")
    print(f"Model: YOLOv8l")
    print(f"Epochs: 300 (with early stopping patience=100)")
    print(f"Batch size: 16")
    print(f"Device: GPU (CUDA)")
    print(f"Overfitting prevention: Dropout=0.1, Mixup=0.15, Copy-paste=0.1, Weight_decay=0.001")
    print()

    # 학습 시작
    results = model.train(**training_args)

    print("\n=== Training Complete ===")
    print(f"Best model saved to: {model.trainer.best}")
    print(f"Results saved to: {model.trainer.save_dir}")

    # 검증
    print("\n=== Validation ===")
    metrics = model.val()
    print(f"mAP50: {metrics.box.map50:.3f}")
    print(f"mAP50-95: {metrics.box.map:.3f}")

    return results


if __name__ == '__main__':
    main()
