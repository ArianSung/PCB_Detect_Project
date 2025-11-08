#!/usr/bin/env python3
"""
일련번호 영역 검출 YOLO 모델 학습

일련번호는 같은 위치에 있을 가능성이 매우 높으므로:
- 적은 데이터셋 (100-300장)으로도 충분
- 증강 최소화
- YOLOv11n (Nano 모델) 사용
"""

from ultralytics import YOLO
from pathlib import Path

def train_serial_number_detector():
    """일련번호 검출 YOLO 모델 학습"""

    print("=" * 80)
    print("일련번호 영역 검출 YOLO 모델 학습 시작")
    print("=" * 80)

    # 데이터셋 경로
    data_yaml = Path('/home/sys1041/work_project/data/raw/serial_number_detection/data.yaml')

    if not data_yaml.exists():
        print(f"❌ 데이터셋을 찾을 수 없습니다: {data_yaml}")
        print("\n먼저 다음 단계를 수행하세요:")
        print("1. data/raw/serial_number_detection 디렉토리 생성")
        print("2. train/images, train/labels, valid/images, valid/labels 디렉토리 생성")
        print("3. LabelImg 또는 Roboflow로 일련번호 영역 라벨링")
        print("4. data.yaml 파일 생성")
        print("\n자세한 내용은 docs/Serial_Number_Detection_Guide.md 참조")
        return False

    # YOLOv11n 사용 (Nano - 가볍고 빠름, 클래스 1개만 있으므로 충분)
    model = YOLO('yolo11n.pt')

    print("\n학습 설정:")
    print("  - 모델: YOLOv11n (Nano)")
    print("  - 에폭: 50")
    print("  - 배치: 16")
    print("  - 이미지 크기: 640")
    print("  - 증강: 최소화 (위치 고정)")
    print("")

    # 학습 실행
    results = model.train(
        data=str(data_yaml),
        epochs=50,           # 50 에폭 (데이터 적고 위치 고정이므로 충분)
        batch=16,
        imgsz=640,
        device=0,            # GPU 0

        # 파일명
        project='runs/detect',
        name='serial_number_detector',
        exist_ok=True,

        # 최적화
        optimizer='AdamW',
        lr0=0.001,
        lrf=0.01,
        weight_decay=0.0005,
        patience=20,         # Early stopping

        # 기타
        amp=True,            # Mixed Precision
        verbose=True,
        plots=True,
        cache=False,
        workers=8,

        # 데이터 증강 (위치가 고정이므로 증강 최소화)
        degrees=5,           # 회전 ±5도만
        translate=0.05,      # 이동 5%만
        scale=0.2,           # 크기 조정 최소
        flipud=0.0,          # 상하 반전 안 함
        fliplr=0.0,          # 좌우 반전 안 함 (일련번호는 항상 정방향)
        mosaic=0.0,          # Mosaic 사용 안 함
        mixup=0.0,           # MixUp 사용 안 함
        copy_paste=0.0,      # CopyPaste 사용 안 함
    )

    print("\n" + "="*80)
    print("✓ 일련번호 검출 모델 학습 완료!")
    print("="*80)
    print(f"Best 모델: runs/detect/serial_number_detector/weights/best.pt")
    print(f"Last 모델: runs/detect/serial_number_detector/weights/last.pt")
    print(f"결과 그래프: runs/detect/serial_number_detector/results.png")
    print("="*80)

    # 검증
    print("\n검증 실행 중...")
    val_results = model.val()

    print("\n검증 결과:")
    print(f"  mAP50: {val_results.box.map50:.4f}")
    print(f"  mAP50-95: {val_results.box.map:.4f}")
    print(f"  Precision: {val_results.box.mp:.4f}")
    print(f"  Recall: {val_results.box.mr:.4f}")

    return results

def main():
    train_serial_number_detector()

if __name__ == "__main__":
    main()
