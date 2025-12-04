"""
새로운 Roboflow 데이터셋으로 YOLO 모델 학습

작성자: Claude Code
날짜: 2025-12-02
"""

import os
from pathlib import Path
from roboflow import Roboflow
from ultralytics import YOLO

def main():
    print("=" * 80)
    print("새 Roboflow 데이터셋 다운로드 및 YOLO 학습 시작")
    print("=" * 80)

    # 1. Roboflow 데이터셋 다운로드
    print("\n[1/4] Roboflow 데이터셋 다운로드 중...")
    rf = Roboflow(api_key="4EdUhVZ6LgtN2FvlVWNW")
    project = rf.workspace("arian-qfo7y").project("pcb_defect_detect-nitz0")
    version = project.version(3)
    dataset = version.download("yolov11")

    print(f"✅ 데이터셋 다운로드 완료: {dataset.location}")

    # 2. data.yaml 파일 경로 확인
    data_yaml_path = Path(dataset.location) / "data.yaml"

    if not data_yaml_path.exists():
        print(f"⚠️  data.yaml 파일을 찾을 수 없습니다: {data_yaml_path}")
        return

    print(f"✅ data.yaml 확인: {data_yaml_path}")

    # 3. YOLOv11l 모델 초기화
    print("\n[2/4] YOLOv11l 모델 초기화 중...")
    model = YOLO('yolo11l.pt')  # Pretrained YOLOv11 Large 모델
    print("✅ YOLOv11l 모델 로드 완료")

    # 4. 학습 시작
    print("\n[3/4] 모델 학습 시작...")
    print("학습 파라미터:")
    print("  - 모델: YOLOv11l")
    print("  - Epochs: 100")
    print("  - Batch: 16")
    print("  - Image Size: 640")
    print("  - Device: GPU (cuda:0)")
    print("  - Optimizer: AdamW")
    print("  - 초기 Learning Rate: 0.001")
    print()

    # 학습 실행
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
        name='pcb_defect_v3',
        exist_ok=True,
        verbose=True
    )

    print("\n✅ 학습 완료!")

    # 5. 모델 저장 위치 출력
    print("\n[4/4] 학습 결과:")
    save_dir = Path(results.save_dir)
    best_model = save_dir / "weights" / "best.pt"
    last_model = save_dir / "weights" / "last.pt"

    print(f"  - 저장 디렉토리: {save_dir}")
    print(f"  - Best 모델: {best_model}")
    print(f"  - Last 모델: {last_model}")

    # 6. app.py 모델 경로 업데이트 안내
    print("\n" + "=" * 80)
    print("다음 단계:")
    print("=" * 80)
    print("1. app.py 파일에서 model_path를 다음으로 변경하세요:")
    print(f"   model_path = '{str(best_model.relative_to(Path.cwd() / 'server'))}'")
    print()
    print("2. 서버를 재시작하세요:")
    print("   cd server && python app.py")
    print("=" * 80)

if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        print("\n\n학습이 중단되었습니다.")
    except Exception as e:
        print(f"\n\n오류 발생: {e}")
        import traceback
        traceback.print_exc()
