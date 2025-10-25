"""
YOLO v8 상세 테스트 스크립트
- 추론 성능 측정 (FPS, 지연시간)
- 검출 결과 분석
- 다양한 모델 크기 비교 (n, s, m, l)
"""

import time
from ultralytics import YOLO
import torch

def test_model_performance(model_name, image_path, num_runs=10):
    """모델 성능 측정"""
    print(f"\n{'='*60}")
    print(f"테스트 모델: {model_name}")
    print(f"{'='*60}")

    # 모델 로드
    model = YOLO(model_name)

    # GPU 사용 가능 여부 확인
    device = 'cuda' if torch.cuda.is_available() else 'cpu'
    print(f"사용 디바이스: {device}")
    if torch.cuda.is_available():
        print(f"GPU 이름: {torch.cuda.get_device_name(0)}")
        print(f"GPU 메모리: {torch.cuda.get_device_properties(0).total_memory / 1e9:.2f} GB")

    # Warm-up (첫 추론은 느릴 수 있음)
    print("\n🔥 Warm-up 실행 중...")
    _ = model(image_path, verbose=False)

    # 성능 측정
    print(f"\n📊 성능 측정 중 ({num_runs}회 반복)...")
    inference_times = []

    for i in range(num_runs):
        start_time = time.time()
        results = model(image_path, verbose=False)
        end_time = time.time()

        inference_time = (end_time - start_time) * 1000  # ms 단위
        inference_times.append(inference_time)
        print(f"  Run {i+1}/{num_runs}: {inference_time:.2f} ms")

    # 통계 계산
    avg_time = sum(inference_times) / len(inference_times)
    min_time = min(inference_times)
    max_time = max(inference_times)
    fps = 1000 / avg_time

    print(f"\n📈 성능 통계:")
    print(f"  평균 추론 시간: {avg_time:.2f} ms")
    print(f"  최소 추론 시간: {min_time:.2f} ms")
    print(f"  최대 추론 시간: {max_time:.2f} ms")
    print(f"  FPS (초당 프레임): {fps:.2f}")

    # 검출 결과 분석
    print(f"\n🎯 검출 결과:")
    results = model(image_path, save=True, verbose=False)

    for r in results:
        boxes = r.boxes
        if len(boxes) > 0:
            print(f"  총 검출 객체 수: {len(boxes)}")

            # 클래스별 개수
            class_counts = {}
            for box in boxes:
                cls = int(box.cls[0])
                class_name = r.names[cls]
                class_counts[class_name] = class_counts.get(class_name, 0) + 1

            print(f"  클래스별 검출:")
            for class_name, count in sorted(class_counts.items()):
                print(f"    - {class_name}: {count}개")

            # Confidence 통계
            confidences = [float(box.conf[0]) for box in boxes]
            avg_conf = sum(confidences) / len(confidences)
            print(f"  평균 Confidence: {avg_conf:.3f}")
            print(f"  최소 Confidence: {min(confidences):.3f}")
            print(f"  최대 Confidence: {max(confidences):.3f}")
        else:
            print("  검출된 객체 없음")

    return {
        'model': model_name,
        'avg_time': avg_time,
        'fps': fps,
        'device': device
    }


def compare_models(image_path):
    """여러 모델 크기 비교"""
    print("\n" + "="*60)
    print("🔍 YOLO v8 모델 크기별 성능 비교")
    print("="*60)

    # 사용 가능한 모델 파일 확인
    available_models = []
    model_sizes = ['yolov8n.pt', 'yolov8s.pt', 'yolov8m.pt', 'yolov8l.pt', 'yolo11n.pt']

    import os
    for model_file in model_sizes:
        if os.path.exists(model_file):
            available_models.append(model_file)

    if not available_models:
        print("⚠️ 사전 학습된 모델 파일을 찾을 수 없습니다.")
        print("   YOLOv8n 모델을 다운로드합니다...")
        available_models = ['yolov8n.pt']

    print(f"\n사용 가능한 모델: {', '.join(available_models)}")

    results_summary = []

    for model_name in available_models:
        result = test_model_performance(model_name, image_path, num_runs=5)
        results_summary.append(result)

    # 비교 결과 출력
    print("\n" + "="*60)
    print("📊 모델 비교 요약")
    print("="*60)
    print(f"{'모델':<15} {'평균 시간 (ms)':<20} {'FPS':<10} {'디바이스':<10}")
    print("-" * 60)

    for result in results_summary:
        print(f"{result['model']:<15} {result['avg_time']:<20.2f} {result['fps']:<10.2f} {result['device']:<10}")

    print("\n💡 결과 해석:")
    print("  - n (nano): 가장 빠름, 정확도 낮음")
    print("  - s (small): 빠름, 정확도 보통")
    print("  - m (medium): 중간 속도, 정확도 높음")
    print("  - l (large): 느림, 정확도 매우 높음 ⭐ (RTX 4080 Super 권장)")
    print(f"\n  📌 PCB 검사 프로젝트 권장: YOLOv8l (Large)")
    print(f"     - 목표 추론 시간: < 300ms")
    print(f"     - 실시간 처리 목표: > 10 FPS")


if __name__ == "__main__":
    import sys

    # 테스트 이미지 경로
    test_image = "test_images/bus.jpg"

    # 인자가 있으면 단일 모델 테스트, 없으면 비교 테스트
    if len(sys.argv) > 1:
        model_name = sys.argv[1]
        test_model_performance(model_name, test_image, num_runs=10)
    else:
        compare_models(test_image)

    print("\n✅ 테스트 완료!")
    print(f"📁 결과 이미지는 'runs/detect/predict*' 폴더에 저장되었습니다.")
