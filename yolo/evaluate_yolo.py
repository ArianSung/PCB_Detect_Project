"""
YOLO v8 모델 평가 스크립트

사용법:
    python evaluate_yolo.py --model models/yolo/final/yolo_best.pt --data data/pcb_defects.yaml

필수 조건:
    - conda activate pcb_defect
    - 학습된 모델 파일 존재
"""

from ultralytics import YOLO
import argparse


def evaluate_yolo(model_path, data_yaml, imgsz=640, device='0'):
    """
    YOLO v8 모델 평가

    Args:
        model_path: 학습된 모델 파일 경로
        data_yaml: 데이터셋 YAML 파일 경로
        imgsz: 이미지 크기
        device: GPU 디바이스
    """

    print(f"모델 로드: {model_path}")
    model = YOLO(model_path)

    print(f"\n평가 시작:")
    print(f"  - 데이터셋: {data_yaml}")
    print(f"  - 이미지 크기: {imgsz}")
    print(f"  - 디바이스: {device}")

    # 검증 실행
    metrics = model.val(
        data=data_yaml,
        imgsz=imgsz,
        device=device,
        batch=16,
        conf=0.001,     # Confidence threshold
        iou=0.6,        # NMS IoU threshold
        plots=True,     # 그래프 생성
        save_json=True, # COCO JSON 저장
        save_hybrid=False,
        verbose=True
    )

    # 결과 출력
    print("\n" + "="*50)
    print("평가 결과")
    print("="*50)

    print(f"\n전체 성능:")
    print(f"  - mAP50-95: {metrics.box.map:.4f}")
    print(f"  - mAP50: {metrics.box.map50:.4f}")
    print(f"  - mAP75: {metrics.box.map75:.4f}")
    print(f"  - Precision: {metrics.box.mp:.4f}")
    print(f"  - Recall: {metrics.box.mr:.4f}")

    print(f"\n클래스별 성능:")
    for i, class_name in enumerate(model.names.values()):
        print(f"  - {class_name}: AP={metrics.box.ap[i]:.4f}")

    print(f"\n추론 속도:")
    print(f"  - Preprocess: {metrics.speed['preprocess']:.2f}ms")
    print(f"  - Inference: {metrics.speed['inference']:.2f}ms")
    print(f"  - Postprocess: {metrics.speed['postprocess']:.2f}ms")
    print(f"  - Total: {sum(metrics.speed.values()):.2f}ms")

    return metrics


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description='YOLO v8 평가 스크립트')
    parser.add_argument('--model', type=str, required=True,
                        help='학습된 모델 파일 경로')
    parser.add_argument('--data', type=str, required=True,
                        help='데이터셋 YAML 파일 경로')
    parser.add_argument('--imgsz', type=int, default=640,
                        help='이미지 크기 (기본값: 640)')
    parser.add_argument('--device', type=str, default='0',
                        help='GPU 디바이스 (기본값: 0)')

    args = parser.parse_args()

    evaluate_yolo(
        model_path=args.model,
        data_yaml=args.data,
        imgsz=args.imgsz,
        device=args.device
    )
