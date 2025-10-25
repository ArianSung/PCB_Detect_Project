"""
YOLO v8 비디오 파일 추론 테스트
- 비디오 파일을 읽어서 추론 수행
- 결과 비디오 저장
- FPS 및 성능 측정
"""

import cv2
import time
from ultralytics import YOLO
import torch
import os


def test_video_inference(model_name, video_path, save_output=True, show_video=False):
    """
    비디오 파일을 사용한 추론 테스트

    Args:
        model_name: YOLO 모델 파일 이름
        video_path: 비디오 파일 경로
        save_output: 결과 비디오 저장 여부
        show_video: 비디오 화면 표시 여부 (WSL에서는 False 권장)
    """
    print(f"\n{'='*60}")
    print(f"🎬 비디오 파일 추론 테스트")
    print(f"{'='*60}")
    print(f"모델: {model_name}")
    print(f"비디오: {video_path}")

    # 비디오 파일 존재 확인
    if not os.path.exists(video_path):
        print(f"❌ 비디오 파일을 찾을 수 없습니다: {video_path}")
        return

    # GPU 확인
    device = 'cuda' if torch.cuda.is_available() else 'cpu'
    print(f"사용 디바이스: {device}")
    if torch.cuda.is_available():
        print(f"GPU: {torch.cuda.get_device_name(0)}")

    # 모델 로드
    print(f"\n⏳ 모델 로딩 중...")
    model = YOLO(model_name)
    print(f"✅ 모델 로딩 완료")

    # 비디오 열기
    print(f"\n📹 비디오 파일 열기...")
    cap = cv2.VideoCapture(video_path)

    if not cap.isOpened():
        print(f"❌ 비디오 파일을 열 수 없습니다: {video_path}")
        return

    # 비디오 정보
    width = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH))
    height = int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))
    fps = cap.get(cv2.CAP_PROP_FPS)
    total_frames = int(cap.get(cv2.CAP_PROP_FRAME_COUNT))

    print(f"✅ 비디오 정보:")
    print(f"   - 해상도: {width}x{height}")
    print(f"   - FPS: {fps:.2f}")
    print(f"   - 총 프레임: {total_frames}")
    print(f"   - 길이: {total_frames/fps:.2f}초")

    # 출력 비디오 설정
    out = None
    if save_output:
        output_path = f"runs/detect/video_output_{int(time.time())}.mp4"
        os.makedirs(os.path.dirname(output_path), exist_ok=True)
        fourcc = cv2.VideoWriter_fourcc(*'mp4v')
        out = cv2.VideoWriter(output_path, fourcc, fps, (width, height))
        print(f"\n📁 출력 비디오 경로: {output_path}")

    # 추론 시작
    print(f"\n🚀 추론 시작...")
    frame_count = 0
    inference_times = []
    detection_counts = []

    try:
        while True:
            ret, frame = cap.read()
            if not ret:
                break

            # 추론
            start_time = time.time()
            results = model(frame, verbose=False)
            inference_time = (time.time() - start_time) * 1000  # ms

            inference_times.append(inference_time)

            # 검출 개수
            num_detections = len(results[0].boxes)
            detection_counts.append(num_detections)

            # 결과 시각화
            annotated_frame = results[0].plot()

            # FPS 표시
            cv2.putText(annotated_frame, f'Frame: {frame_count}/{total_frames}', (10, 30),
                       cv2.FONT_HERSHEY_SIMPLEX, 0.8, (0, 255, 0), 2)
            cv2.putText(annotated_frame, f'Inference: {inference_time:.1f}ms', (10, 60),
                       cv2.FONT_HERSHEY_SIMPLEX, 0.8, (0, 255, 0), 2)
            cv2.putText(annotated_frame, f'Detections: {num_detections}', (10, 90),
                       cv2.FONT_HERSHEY_SIMPLEX, 0.8, (0, 255, 0), 2)

            # 비디오 저장
            if save_output and out is not None:
                out.write(annotated_frame)

            # 화면 표시 (선택적)
            if show_video:
                cv2.imshow('YOLO Video Inference', annotated_frame)
                if cv2.waitKey(1) & 0xFF == ord('q'):
                    print("\n⏹️ 사용자가 종료를 요청했습니다.")
                    break

            frame_count += 1

            # 진행률 표시 (10% 단위)
            progress = (frame_count / total_frames) * 100
            if frame_count % max(1, total_frames // 10) == 0:
                avg_time = sum(inference_times) / len(inference_times)
                avg_fps = 1000 / avg_time
                print(f"진행률: {progress:.1f}% ({frame_count}/{total_frames}) | "
                      f"평균 추론: {avg_time:.2f}ms | FPS: {avg_fps:.2f}")

    except KeyboardInterrupt:
        print("\n⏹️ Ctrl+C로 종료되었습니다.")

    finally:
        # 정리
        cap.release()
        if out is not None:
            out.release()
        if show_video:
            cv2.destroyAllWindows()

        # 최종 통계
        if len(inference_times) > 0:
            print(f"\n{'='*60}")
            print(f"📊 최종 통계")
            print(f"{'='*60}")
            print(f"처리된 프레임 수: {frame_count}/{total_frames}")
            print(f"평균 추론 시간: {sum(inference_times) / len(inference_times):.2f} ms")
            print(f"최소 추론 시간: {min(inference_times):.2f} ms")
            print(f"최대 추론 시간: {max(inference_times):.2f} ms")
            print(f"평균 FPS: {1000 / (sum(inference_times) / len(inference_times)):.2f}")
            print(f"평균 검출 개수: {sum(detection_counts) / len(detection_counts):.2f}")

            if save_output:
                print(f"\n📁 결과 비디오 저장 완료: {output_path}")


if __name__ == "__main__":
    import sys

    if len(sys.argv) < 2:
        print("사용법: python test_yolo_video.py <video_file> [model_name]")
        print("\n예시:")
        print("  python test_yolo_video.py video.mp4")
        print("  python test_yolo_video.py video.mp4 yolov8n.pt")
        print("\n💡 팁: 테스트용 비디오는 다음에서 다운로드할 수 있습니다:")
        print("  https://pixabay.com/videos/")
        sys.exit(1)

    video_path = sys.argv[1]
    model_name = sys.argv[2] if len(sys.argv) > 2 else 'yolov8n.pt'

    # WSL 환경에서는 show_video=False 권장
    test_video_inference(model_name, video_path, save_output=True, show_video=False)

    print("\n✅ 테스트 완료!")
