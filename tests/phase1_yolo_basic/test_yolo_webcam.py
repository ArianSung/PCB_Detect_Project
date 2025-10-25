"""
YOLO v8 웹캠 실시간 추론 테스트
- 실시간 비디오 스트림 처리
- FPS 측정 및 표시
- ESC 키로 종료
"""

import cv2
import time
from ultralytics import YOLO
import torch

def test_webcam_inference(model_name='yolov8n.pt', camera_id=0, show_fps=True):
    """
    웹캠을 사용한 실시간 추론 테스트

    Args:
        model_name: YOLO 모델 파일 이름
        camera_id: 카메라 ID (0: 기본 웹캠, 1: 외장 웹캠)
        show_fps: FPS 표시 여부
    """
    print(f"\n{'='*60}")
    print(f"🎥 웹캠 실시간 추론 테스트")
    print(f"{'='*60}")
    print(f"모델: {model_name}")
    print(f"카메라 ID: {camera_id}")

    # GPU 확인
    device = 'cuda' if torch.cuda.is_available() else 'cpu'
    print(f"사용 디바이스: {device}")
    if torch.cuda.is_available():
        print(f"GPU: {torch.cuda.get_device_name(0)}")

    # 모델 로드
    print(f"\n⏳ 모델 로딩 중...")
    model = YOLO(model_name)
    print(f"✅ 모델 로딩 완료")

    # 웹캠 열기
    print(f"\n📷 웹캠 연결 중...")
    cap = cv2.VideoCapture(camera_id)

    if not cap.isOpened():
        print(f"❌ 웹캠 {camera_id}를 열 수 없습니다.")
        print(f"\n💡 해결 방법:")
        print(f"   1. WSL 환경에서는 웹캠 사용이 제한될 수 있습니다.")
        print(f"   2. 대신 비디오 파일 테스트를 권장합니다:")
        print(f"      python test_yolo_video.py <video_file>")
        return

    # 웹캠 해상도 설정
    cap.set(cv2.CAP_PROP_FRAME_WIDTH, 640)
    cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 480)

    width = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH))
    height = int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))
    print(f"✅ 웹캠 연결 성공 (해상도: {width}x{height})")

    print(f"\n🚀 실시간 추론 시작...")
    print(f"   - 'q' 또는 'ESC' 키를 누르면 종료됩니다.")

    # FPS 측정 변수
    fps_list = []
    frame_count = 0

    try:
        while True:
            # 프레임 읽기
            ret, frame = cap.read()
            if not ret:
                print("⚠️ 프레임을 읽을 수 없습니다.")
                break

            # 추론 시작 시간
            start_time = time.time()

            # YOLO 추론
            results = model(frame, verbose=False)

            # 추론 시간 계산
            inference_time = time.time() - start_time
            fps = 1.0 / inference_time
            fps_list.append(fps)

            # 결과 시각화
            annotated_frame = results[0].plot()

            # FPS 표시
            if show_fps:
                avg_fps = sum(fps_list[-30:]) / len(fps_list[-30:]) if len(fps_list) > 0 else 0
                cv2.putText(annotated_frame, f'FPS: {avg_fps:.2f}', (10, 30),
                           cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)
                cv2.putText(annotated_frame, f'Time: {inference_time*1000:.1f}ms', (10, 70),
                           cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)

            # 화면에 표시
            cv2.imshow('YOLO v8 Webcam Inference', annotated_frame)

            frame_count += 1

            # 콘솔에 통계 출력 (100 프레임마다)
            if frame_count % 100 == 0:
                avg_fps = sum(fps_list[-100:]) / len(fps_list[-100:])
                print(f"프레임 {frame_count}: 평균 FPS = {avg_fps:.2f}")

            # 'q' 또는 ESC 키로 종료
            key = cv2.waitKey(1) & 0xFF
            if key == ord('q') or key == 27:  # 27 = ESC
                print("\n⏹️ 사용자가 종료를 요청했습니다.")
                break

    except KeyboardInterrupt:
        print("\n⏹️ Ctrl+C로 종료되었습니다.")

    finally:
        # 정리
        cap.release()
        cv2.destroyAllWindows()

        # 최종 통계
        if len(fps_list) > 0:
            print(f"\n{'='*60}")
            print(f"📊 최종 통계")
            print(f"{'='*60}")
            print(f"총 프레임 수: {frame_count}")
            print(f"평균 FPS: {sum(fps_list) / len(fps_list):.2f}")
            print(f"최소 FPS: {min(fps_list):.2f}")
            print(f"최대 FPS: {max(fps_list):.2f}")
            print(f"평균 추론 시간: {1000 / (sum(fps_list) / len(fps_list)):.2f} ms")


if __name__ == "__main__":
    import sys

    # 기본값
    model_name = 'yolov8n.pt'
    camera_id = 0

    # 명령줄 인자 파싱
    if len(sys.argv) > 1:
        model_name = sys.argv[1]
    if len(sys.argv) > 2:
        camera_id = int(sys.argv[2])

    test_webcam_inference(model_name, camera_id)

    print("\n✅ 테스트 완료!")
