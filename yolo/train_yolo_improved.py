#!/usr/bin/env python3
"""
개선된 YOLO 학습 스크립트

개선사항:
1. Mixup 증강 활성화 (소수 클래스 학습 강화)
2. Copy-paste 증강 활성화
3. Patience 100으로 증가 (더 오래 학습)
4. 리밸런싱된 데이터셋 사용
"""

import argparse
from ultralytics import YOLO

def train_yolo(
    data,
    model='yolo11m.pt',
    epochs=150,
    batch=16,
    imgsz=640,
    device='0',
    project='yolo/runs/train',
    name='pcb_defect_improved'
):
    """
    YOLO 모델 학습

    Args:
        data: 데이터셋 YAML 경로
        model: 사전학습 모델 경로
        epochs: 학습 에포크 수
        batch: 배치 크기
        imgsz: 이미지 크기
        device: GPU 디바이스
        project: 프로젝트 경로
        name: 실행 이름
    """

    print("=" * 80)
    print("YOLO 개선 학습 시작")
    print("=" * 80)
    print(f"\n모델: {model}")
    print(f"데이터: {data}")
    print(f"에포크: {epochs}")
    print(f"배치 크기: {batch}")
    print(f"이미지 크기: {imgsz}")
    print(f"디바이스: {device}")
    print("\n개선사항:")
    print("  ✓ Mixup 증강 활성화 (0.15)")
    print("  ✓ Copy-paste 증강 활성화 (0.3)")
    print("  ✓ Patience 100으로 증가")
    print("  ✓ 리밸런싱된 데이터셋 사용")
    print("=" * 80 + "\n")

    # YOLO 모델 로드
    model = YOLO(model)

    # 학습 시작
    results = model.train(
        data=data,
        epochs=epochs,
        batch=batch,
        imgsz=imgsz,
        device=device,
        project=project,
        name=name,

        # 기본 설정
        patience=100,          # ← 50에서 100으로 증가
        save=True,
        save_period=10,
        cache=True,
        workers=8,

        # 옵티마이저
        optimizer='AdamW',
        lr0=0.001,
        lrf=0.01,
        momentum=0.937,
        weight_decay=0.0005,
        warmup_epochs=3.0,

        # 증강 파라미터
        hsv_h=0.015,
        hsv_s=0.7,
        hsv_v=0.4,
        degrees=10.0,
        translate=0.1,
        scale=0.5,
        shear=2.0,
        flipud=0.0,
        fliplr=0.5,
        mosaic=1.0,
        mixup=0.15,            # ← 0.0에서 0.15로 증가
        copy_paste=0.3,        # ← 0.0에서 0.3으로 증가
        auto_augment='randaugment',
        erasing=0.4,

        # AMP 활성화
        amp=True,

        # 검증
        val=True,
        plots=True,

        # 기타
        seed=0,
        deterministic=True,
        verbose=True
    )

    print("\n" + "=" * 80)
    print("학습 완료!")
    print("=" * 80)
    print(f"\n결과 저장 위치: {project}/{name}")
    print(f"Best 모델: {project}/{name}/weights/best.pt")

    return results

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description='YOLO 개선 학습 스크립트')
    parser.add_argument('--data', type=str, required=True,
                       help='데이터셋 YAML 파일 경로')
    parser.add_argument('--model', type=str, default='yolo11m.pt',
                       help='사전학습 모델 경로')
    parser.add_argument('--epochs', type=int, default=150,
                       help='학습 에포크 수')
    parser.add_argument('--batch', type=int, default=16,
                       help='배치 크기')
    parser.add_argument('--imgsz', type=int, default=640,
                       help='이미지 크기')
    parser.add_argument('--device', type=str, default='0',
                       help='GPU 디바이스')

    args = parser.parse_args()

    train_yolo(
        data=args.data,
        model=args.model,
        epochs=args.epochs,
        batch=args.batch,
        imgsz=args.imgsz,
        device=args.device
    )
