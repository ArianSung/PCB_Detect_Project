#!/usr/bin/env python3
"""
YOLOv11 부품 검출 모델 v5 학습 스크립트
"""

import os
import torch
from ultralytics import YOLO
from datetime import datetime

def main():
    # GPU 확인
    device = 'cuda' if torch.cuda.is_available() else 'cpu'
    print(f"Using device: {device}")
    if device == 'cuda':
        print(f"GPU: {torch.cuda.get_device_name(0)}")
        print(f"VRAM Available: {torch.cuda.get_device_properties(0).total_memory / 1024**3:.2f} GB")

    # 데이터셋 경로
    data_yaml = "/home/sys1041/work_project/PCB_defect_detect-5/data.yaml"

    # 모델 로드 (YOLOv11l - Large 모델)
    model = YOLO("yolo11l.pt")

    # 학습 시작 시간
    start_time = datetime.now()
    print(f"\nTraining started at: {start_time}")

    # 학습 설정
    results = model.train(
        data=data_yaml,
        epochs=100,
        batch=16,         # RTX 4080 Super 16GB 기준 권장 배치 크기
        imgsz=640,
        device=device,
        amp=True,         # Mixed Precision (FP16) 활성화
        optimizer='AdamW',
        lr0=0.001,
        weight_decay=0.0005,
        patience=30,      # Early stopping patience
        save=True,
        save_period=10,   # 10 에폭마다 체크포인트 저장
        project='runs/detect',
        name='v5_component_detector',
        exist_ok=True,
        plots=True,
        val=True,
        verbose=True
    )

    # 학습 종료 시간
    end_time = datetime.now()
    duration = end_time - start_time

    print(f"\nTraining completed at: {end_time}")
    print(f"Total training time: {duration}")

    # 최고 성능 모델 경로
    best_model_path = os.path.join(results.save_dir, 'weights', 'best.pt')
    print(f"\nBest model saved at: {best_model_path}")

    # 모델 평가
    print("\n" + "="*50)
    print("Evaluating best model on validation set...")
    print("="*50 + "\n")

    best_model = YOLO(best_model_path)
    metrics = best_model.val(data=data_yaml)

    print(f"\nValidation Results:")
    print(f"mAP@0.5: {metrics.box.map50:.4f}")
    print(f"mAP@0.5:0.95: {metrics.box.map:.4f}")
    print(f"Precision: {metrics.box.mp:.4f}")
    print(f"Recall: {metrics.box.mr:.4f}")

    # models 폴더에 복사
    models_dir = "/home/sys1041/work_project/models"
    os.makedirs(models_dir, exist_ok=True)

    target_path = os.path.join(models_dir, "component_detector_v5_best.pt")

    import shutil
    shutil.copy2(best_model_path, target_path)
    print(f"\nBest model copied to: {target_path}")

if __name__ == "__main__":
    main()
