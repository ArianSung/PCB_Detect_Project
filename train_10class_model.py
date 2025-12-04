#!/usr/bin/env python3
"""
10클래스 YOLO 모델 학습 (ceramic capacitor, zener diode 제거)

작성자: Claude Code
날짜: 2025-12-03
"""

from pathlib import Path
from ultralytics import YOLO
import logging

# 로깅 설정
logging.basicConfig(
    level=logging.INFO,
    format='[%(asctime)s] [%(levelname)s] %(message)s',
    datefmt='%Y-%m-%d %H:%M:%S'
)
logger = logging.getLogger(__name__)

def main():
    print("=" * 80)
    print("10클래스 PCB 부품 검출 모델 학습 시작")
    print("=" * 80)
    print("\n제거된 클래스: ceramic capacitor, zener diode")
    print("남은 클래스 (10개):")
    print("  0. Electrolytic capacitor")
    print("  1. IC")
    print("  2. cds")
    print("  3. diode")
    print("  4. led")
    print("  5. pinheader")
    print("  6. pinsocket")
    print("  7. resistor")
    print("  8. switch")
    print("  9. transistor")
    print()

    # 1. data.yaml 경로 확인
    data_yaml_path = Path('/home/sys1041/work_project/PCB_defect_detect-3/data.yaml')

    if not data_yaml_path.exists():
        logger.error(f"data.yaml 파일을 찾을 수 없습니다: {data_yaml_path}")
        return

    logger.info(f"✅ data.yaml 확인: {data_yaml_path}")

    # 2. YOLOv11l 모델 초기화
    logger.info("YOLOv11l 모델 초기화 중...")
    model = YOLO('yolo11l.pt')
    logger.info("✅ YOLOv11l 모델 로드 완료")

    # 3. 학습 파라미터 출력
    print("\n학습 파라미터:")
    print("  - 모델: YOLOv11l")
    print("  - 클래스 수: 10개")
    print("  - Epochs: 100")
    print("  - Batch: 16")
    print("  - Image Size: 640")
    print("  - Device: GPU (cuda:0)")
    print("  - Optimizer: AdamW")
    print("  - 초기 Learning Rate: 0.001")
    print("  - Patience: 30 (Early Stopping)")
    print()

    # 4. 학습 시작
    logger.info("모델 학습 시작...")

    results = model.train(
        data=str(data_yaml_path),
        epochs=100,
        batch=16,
        imgsz=640,
        device=0,  # GPU
        optimizer='AdamW',
        lr0=0.001,
        patience=30,
        project='runs/detect',
        name='pcb_defect_v4_10class',  # 새 버전
        exist_ok=False,  # 덮어쓰기 방지
        verbose=True,
        plots=True  # 학습 그래프 생성
    )

    logger.info("✅ 학습 완료!")

    # 5. 결과 출력
    print("\n" + "=" * 80)
    print("학습 결과")
    print("=" * 80)

    save_dir = Path(results.save_dir)
    best_model = save_dir / "weights" / "best.pt"
    last_model = save_dir / "weights" / "last.pt"

    print(f"  📂 저장 디렉토리: {save_dir}")
    print(f"  🏆 Best 모델: {best_model}")
    print(f"  📦 Last 모델: {last_model}")
    print()
    print("  📊 학습 그래프:")
    print(f"     - {save_dir}/results.png")
    print(f"     - {save_dir}/confusion_matrix.png")
    print(f"     - {save_dir}/BoxF1_curve.png")
    print(f"     - {save_dir}/BoxPR_curve.png")
    print()

    # 6. app.py 업데이트 안내
    print("=" * 80)
    print("다음 단계: Flask 서버 모델 경로 업데이트")
    print("=" * 80)
    print("1. app.py에서 model_path를 다음으로 변경:")
    print(f"   model_path = '../runs/detect/pcb_defect_v4_10class/weights/best.pt'")
    print()
    print("2. 서버 재시작:")
    print("   cd server && python app.py")
    print("=" * 80)

if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        print("\n\n⚠️  학습이 사용자에 의해 중단되었습니다.")
    except Exception as e:
        logger.error(f"오류 발생: {e}")
        import traceback
        traceback.print_exc()
