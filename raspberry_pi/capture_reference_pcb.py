#!/usr/bin/env python3
"""
기준 PCB 촬영 스크립트 (라즈베리파이)

정상 PCB를 촬영하여 기준 데이터 생성용 이미지를 저장합니다.
좌측/우측 카메라로 각각 촬영하여 저장합니다.

사용법:
    # Interactive 모드 (GUI 프리뷰) - 권장!
    python3 capture_reference_pcb.py --side left --camera-id 0 --output ./reference_images

    # Headless 모드 (자동 촬영, GUI 없음)
    python3 capture_reference_pcb.py --side left --camera-id 0 --output ./reference_images --headless

원격 접속 방법:
    1. VNC (권장):
       - 라즈베리파이: sudo raspi-config -> Interface Options -> VNC 활성화
       - Windows/Mac: VNC Viewer 설치 (RealVNC)
       - 접속: vnc://라즈베리파이IP:5900
       - 스크립트 실행 시 OpenCV 창이 자동으로 나타남

    2. 원격 데스크톱 (RDP):
       - 라즈베리파이: sudo apt install xrdp
       - Windows: 원격 데스크톱 연결 (mstsc.exe)
       - Mac: Microsoft Remote Desktop 설치

    3. SSH X11 forwarding:
       - ssh -X pi@라즈베리파이
       - DISPLAY 환경변수 확인: echo $DISPLAY
"""

import cv2
import argparse
import os
import sys
from datetime import datetime
import time

# GUI 백엔드 설정 (VNC/원격 데스크톱 환경 지원)
# Qt, GTK 순서로 시도
try:
    cv2.namedWindow("test")
    cv2.destroyWindow("test")
except:
    pass  # 백엔드 자동 선택


def capture_reference_images(side, camera_id, output_dir, num_images=5, headless=False, interval=2):
    """
    기준 PCB 이미지 촬영

    Args:
        side (str): 'left' 또는 'right'
        camera_id (int): 카메라 인덱스
        output_dir (str): 저장 디렉토리
        num_images (int): 촬영할 이미지 수 (기본 5장)
        headless (bool): SSH 환경용 headless 모드 (GUI 없음)
        interval (int): headless 모드에서 촬영 간격 (초)

    Returns:
        list: 저장된 이미지 파일 경로 리스트
    """
    # 출력 디렉토리 생성
    os.makedirs(output_dir, exist_ok=True)

    # 카메라 초기화
    print(f"[INFO] {side} 카메라 ({camera_id}) 초기화 중...")
    cap = cv2.VideoCapture(camera_id)

    if not cap.isOpened():
        print(f"[ERROR] 카메라를 열 수 없습니다: {camera_id}")
        return []

    # 카메라 설정
    cap.set(cv2.CAP_PROP_FRAME_WIDTH, 640)
    cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 480)
    cap.set(cv2.CAP_PROP_FPS, 30)

    # 워밍업 (카메라 안정화)
    print("[INFO] 카메라 워밍업 중... (3초)")
    for _ in range(30):
        cap.read()
    time.sleep(3)

    saved_files = []

    print(f"\n{'='*60}")
    print(f"기준 PCB 촬영 모드 ({side})")
    if headless:
        print(f"모드: Headless (SSH 환경용)")
        print(f"촬영 간격: {interval}초")
    else:
        print(f"모드: Interactive (대화형)")
    print(f"{'='*60}")
    print(f"총 {num_images}장의 이미지를 촬영합니다.")
    print(f"PCB를 카메라 앞에 정확히 위치시켜주세요.")
    print(f"{'='*60}\n")

    for i in range(num_images):
        print(f"\n[{i+1}/{num_images}] 촬영 준비...")

        if headless:
            # Headless 모드: 자동 촬영
            print(f"{interval}초 후 자동 촬영합니다...")

            # 카운트다운
            for countdown in range(interval, 0, -1):
                print(f"  {countdown}초...", end='\r')
                time.sleep(1)
            print("  촬영!      ")

            # 프레임 읽기
            ret, frame = cap.read()
            if not ret:
                print("[ERROR] 프레임 캡처 실패")
                continue
        else:
            # Interactive 모드: GUI 프리뷰
            print("GUI 창에서 PCB 위치를 확인하세요")
            print("스페이스바: 촬영 | q: 종료")

            window_name = f'Reference PCB Capture - {side.upper()}'

            # 프리뷰 루프
            while True:
                ret, frame = cap.read()
                if not ret:
                    print("[ERROR] 프레임을 읽을 수 없습니다.")
                    time.sleep(0.1)
                    continue

                # 화면에 가이드 표시
                display_frame = frame.copy()

                # 상단 텍스트
                cv2.putText(
                    display_frame,
                    f"[{i+1}/{num_images}] SPACE: Capture | Q: Quit",
                    (10, 30),
                    cv2.FONT_HERSHEY_SIMPLEX,
                    0.7,
                    (0, 255, 0),
                    2
                )

                # 중앙 십자선 (PCB 중앙 맞추기 가이드)
                h, w = display_frame.shape[:2]
                cv2.line(display_frame, (w//2, 0), (w//2, h), (0, 255, 0), 1)
                cv2.line(display_frame, (0, h//2), (w, h//2), (0, 255, 0), 1)

                # 코너 마커 (PCB 위치 가이드)
                margin = 50
                marker_size = 20
                # 좌상단
                cv2.line(display_frame, (margin, margin), (margin + marker_size, margin), (0, 255, 255), 2)
                cv2.line(display_frame, (margin, margin), (margin, margin + marker_size), (0, 255, 255), 2)
                # 우상단
                cv2.line(display_frame, (w - margin, margin), (w - margin - marker_size, margin), (0, 255, 255), 2)
                cv2.line(display_frame, (w - margin, margin), (w - margin, margin + marker_size), (0, 255, 255), 2)
                # 좌하단
                cv2.line(display_frame, (margin, h - margin), (margin + marker_size, h - margin), (0, 255, 255), 2)
                cv2.line(display_frame, (margin, h - margin), (margin, h - margin - marker_size), (0, 255, 255), 2)
                # 우하단
                cv2.line(display_frame, (w - margin, h - margin), (w - margin - marker_size, h - margin), (0, 255, 255), 2)
                cv2.line(display_frame, (w - margin, h - margin), (w - margin, h - margin - marker_size), (0, 255, 255), 2)

                # GUI 창에 표시
                cv2.imshow(window_name, display_frame)

                # 키 입력 대기 (30ms)
                key = cv2.waitKey(30) & 0xFF

                if key == ord('q') or key == ord('Q'):
                    print("[INFO] 촬영을 중단합니다.")
                    cv2.destroyAllWindows()
                    cap.release()
                    return saved_files
                elif key == ord(' '):  # 스페이스바
                    print("  촬영!")
                    break

            # 촬영
            ret, frame = cap.read()
            if not ret:
                print("[ERROR] 프레임 캡처 실패")
                continue

        # 파일명 생성 (타임스탬프 포함)
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        filename = f"reference_{side}_{timestamp}_{i+1}.jpg"
        filepath = os.path.join(output_dir, filename)

        # 이미지 저장
        cv2.imwrite(filepath, frame, [cv2.IMWRITE_JPEG_QUALITY, 95])
        saved_files.append(filepath)

        print(f"[SUCCESS] 저장 완료: {filepath}")
        print(f"  - 해상도: {frame.shape[1]}x{frame.shape[0]}")
        print(f"  - 크기: {os.path.getsize(filepath) / 1024:.2f} KB")

        # 다음 촬영 전 대기
        if i < num_images - 1:
            time.sleep(1)

    # 정리
    cap.release()
    cv2.destroyAllWindows()

    print(f"\n{'='*60}")
    print(f"촬영 완료!")
    print(f"총 {len(saved_files)}장의 이미지가 저장되었습니다.")
    print(f"저장 위치: {output_dir}")
    print(f"{'='*60}\n")

    # 저장된 파일 목록 출력
    print("저장된 파일:")
    for f in saved_files:
        print(f"  - {f}")

    return saved_files


def main():
    parser = argparse.ArgumentParser(
        description='기준 PCB 촬영 스크립트 (라즈베리파이)'
    )

    parser.add_argument(
        '--side',
        type=str,
        required=True,
        choices=['left', 'right'],
        help='카메라 위치 (left: 좌측, right: 우측)'
    )

    parser.add_argument(
        '--camera-id',
        type=int,
        default=0,
        help='카메라 인덱스 (기본값: 0)'
    )

    parser.add_argument(
        '--output',
        type=str,
        default='./reference_images',
        help='이미지 저장 디렉토리 (기본값: ./reference_images)'
    )

    parser.add_argument(
        '--num-images',
        type=int,
        default=5,
        help='촬영할 이미지 수 (기본값: 5)'
    )

    parser.add_argument(
        '--headless',
        action='store_true',
        help='Headless 모드 (SSH 환경용, GUI 없이 자동 촬영)'
    )

    parser.add_argument(
        '--interval',
        type=int,
        default=2,
        help='Headless 모드에서 촬영 간격 (초, 기본값: 2)'
    )

    args = parser.parse_args()

    # 촬영 시작
    saved_files = capture_reference_images(
        side=args.side,
        camera_id=args.camera_id,
        output_dir=args.output,
        num_images=args.num_images,
        headless=args.headless,
        interval=args.interval
    )

    if saved_files:
        print("\n[다음 단계]")
        print("1. 촬영된 이미지를 서버 폴더로 복사하세요:")
        print(f"   scp {args.output}/reference_{args.side}_*.jpg user@server:/path/to/server/reference_images/")
        print("2. 서버에서 기준 데이터 생성 도구를 실행하세요:")
        print("   python tools/generate_reference_data.py --images /path/to/reference_images")
        print("")
        return 0
    else:
        print("[ERROR] 촬영된 이미지가 없습니다.")
        return 1


if __name__ == '__main__':
    sys.exit(main())
