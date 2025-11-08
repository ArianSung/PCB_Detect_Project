import cv2
import sys

def test_camera(device_id=0):
    """웹캠 테스트"""
    cap = cv2.VideoCapture(device_id)

    if not cap.isOpened():
        print(f"Error: Cannot open camera {device_id}")
        return False

    # 해상도 설정
    cap.set(cv2.CAP_PROP_FRAME_WIDTH, 1280)
    cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 720)

    # 프레임 읽기
    ret, frame = cap.read()
    if ret:
        print(f"✅ Camera {device_id} working!")
        print(f"   Resolution: {frame.shape[1]}x{frame.shape[0]}")

        # 프레임 저장
        cv2.imwrite('test_frame.jpg', frame)
        print("   Test frame saved as 'test_frame.jpg'")
    else:
        print(f"❌ Cannot read frame from camera {device_id}")
        return False

    cap.release()
    return True

if __name__ == '__main__':
    device_id = int(sys.argv[1]) if len(sys.argv) > 1 else 0
    test_camera(device_id)