"""
YOLO v8 학습 스크립트

사용법:
    python train_yolo.py --data data/pcb_defects.yaml --epochs 150 --batch 32

필수 조건:
    - conda activate pcb_defect
    - data/pcb_defects.yaml 파일 존재
"""

from ultralytics import YOLO
import argparse
import yaml
import os


def train_yolo(data_yaml, model='yolov8l.pt', epochs=150, batch=32, imgsz=640, device='0'):
    """
    YOLO v8 모델 학습

    Args:
        data_yaml: 데이터셋 YAML 파일 경로
        model: 사전 학습된 모델 또는 모델 크기 (yolov8n/s/m/l/x)
        epochs: 학습 에포크 수
        batch: 배치 사이즈
        imgsz: 이미지 크기
        device: GPU 디바이스 (0, 1, cpu)
    """

    # YAML 파일 확인
    if not os.path.exists(data_yaml):
        raise FileNotFoundError(f"데이터셋 YAML 파일을 찾을 수 없습니다: {data_yaml}")

    # YAML 로드 및 검증
    with open(data_yaml, 'r') as f:
        data_config = yaml.safe_load(f)
        print(f"데이터셋 정보:")
        print(f"  - 클래스 수: {data_config['nc']}")
        print(f"  - 클래스 이름: {data_config['names']}")

    # YOLO 모델 로드
    print(f"\n모델 로드: {model}")
    yolo_model = YOLO(model)

    # 학습 시작
    print(f"\n학습 시작:")
    print(f"  - Epochs: {epochs}")
    print(f"  - Batch size: {batch}")
    print(f"  - Image size: {imgsz}")
    print(f"  - Device: {device}")

    results = yolo_model.train(
        data=data_yaml,
        epochs=epochs,
        batch=batch,
        imgsz=imgsz,
        device=device,
        # 최적화 설정
        patience=50,              # Early stopping patience
        save=True,                # 체크포인트 저장
        save_period=10,           # 10 에포크마다 저장
        cache=True,               # 데이터셋 캐싱
        # 증강 설정 (전략 3: 강화된 증강)
        augment=True,
        hsv_h=0.015,              # HSV-Hue 증강
        hsv_s=0.7,                # HSV-Saturation 증강
        hsv_v=0.4,                # HSV-Value 증강
        degrees=10.0,             # 회전 (±10도) - 강화!
        translate=0.1,            # 이동 (±fraction)
        scale=0.5,                # 스케일 (gain)
        shear=2.0,                # 전단 (±2도) - 강화!
        perspective=0.0001,       # 원근 변환 - 추가!
        flipud=0.0,               # 상하 반전 확률
        fliplr=0.5,               # 좌우 반전 확률
        mosaic=1.0,               # Mosaic 증강 확률
        mixup=0.0,                # MixUp 증강 확률
        # 학습 설정
        optimizer='AdamW',        # 옵티마이저
        lr0=0.001,                # 초기 학습률
        lrf=0.01,                 # 최종 학습률 (lr0 * lrf)
        momentum=0.937,           # SGD momentum/Adam beta1
        weight_decay=0.0005,      # 가중치 감쇠
        warmup_epochs=3.0,        # Warmup 에포크
        warmup_momentum=0.8,      # Warmup momentum
        warmup_bias_lr=0.1,       # Warmup bias 학습률
        # 검증 설정
        val=True,                 # 검증 활성화
        plots=True,               # 그래프 생성
        # 로깅
        verbose=True,
        project='yolo/runs/train',  # 프로젝트 폴더
        name='pcb_defect',          # 실험 이름
        exist_ok=False,             # 기존 폴더 덮어쓰기
    )

    # 결과 출력
    print("\n" + "="*50)
    print("학습 완료!")
    print("="*50)
    print(f"최적 모델: {results.save_dir / 'weights' / 'best.pt'}")
    print(f"최종 모델: {results.save_dir / 'weights' / 'last.pt'}")
    print(f"결과 폴더: {results.save_dir}")

    # 검증 결과
    metrics = yolo_model.val()
    print(f"\n검증 결과:")
    print(f"  - mAP50-95: {metrics.box.map:.4f}")
    print(f"  - mAP50: {metrics.box.map50:.4f}")
    print(f"  - mAP75: {metrics.box.map75:.4f}")
    print(f"  - Precision: {metrics.box.mp:.4f}")
    print(f"  - Recall: {metrics.box.mr:.4f}")

    return results


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description='YOLO v8 학습 스크립트')
    parser.add_argument('--data', type=str, required=True,
                        help='데이터셋 YAML 파일 경로 (예: data/pcb_defects.yaml)')
    parser.add_argument('--model', type=str, default='yolov8l.pt',
                        help='YOLO 모델 (yolov8n/s/m/l/x.pt) 기본값: yolov8l.pt')
    parser.add_argument('--epochs', type=int, default=150,
                        help='학습 에포크 수 (기본값: 150)')
    parser.add_argument('--batch', type=int, default=32,
                        help='배치 사이즈 (기본값: 32)')
    parser.add_argument('--imgsz', type=int, default=640,
                        help='이미지 크기 (기본값: 640)')
    parser.add_argument('--device', type=str, default='0',
                        help='GPU 디바이스 (0, 1, cpu) 기본값: 0')

    args = parser.parse_args()

    train_yolo(
        data_yaml=args.data,
        model=args.model,
        epochs=args.epochs,
        batch=args.batch,
        imgsz=args.imgsz,
        device=args.device
    )
