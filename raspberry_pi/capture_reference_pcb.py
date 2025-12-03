#!/usr/bin/env python3
"""
기준 PCB 촬영 스크립트 (라즈베리파이) - 무한 촬영 + 노출/초점 제어

기능:
    - 밝기/대비/채도 (SW 보정)
    - 노출 (HW 제어)
    - 초점 (HW 제어 - 지원 카메라만 동작)
    - 무한 촬영 모드 (Space: 저장, Q: 종료)
"""

import cv2
import numpy as np
import argparse
import os
import sys
from datetime import datetime
import time

# GUI 백엔드 설정
try:
    cv2.namedWindow("test")
    cv2.destroyWindow("test")
except:
    pass 

# 카메라 파라미터 기본값
DEFAULT_CAMERA_PARAMS = {
    'brightness': 50,
    'contrast': 50,
    'saturation': 50,
    
    # 노출 관련
    'exposure_auto': 0,  # 1: 자동, 0: 수동
    'exposure_abs': 750, # 수동 노출 값
    
    # 초점 관련 (하드웨어 지원 필수)
    'focus_auto': 0,     # 1: 자동 초점, 0: 수동 초점
    'focus_abs': 0,      # 수동 초점 값 (보통 0~255 또는 0~1023)
}

def nothing(x):
    pass

def setup_camera_controls(window_name):
    """트랙바 설정 (초점 기능 추가됨)"""
    # 1. 소프트웨어 보정
    cv2.createTrackbar('Brightness', window_name, DEFAULT_CAMERA_PARAMS['brightness'], 100, nothing)
    cv2.createTrackbar('Contrast', window_name, DEFAULT_CAMERA_PARAMS['contrast'], 100, nothing)
    cv2.createTrackbar('Saturation', window_name, DEFAULT_CAMERA_PARAMS['saturation'], 100, nothing)

    # 2. 하드웨어 제어 - 노출
    cv2.createTrackbar('Auto Exposure', window_name, DEFAULT_CAMERA_PARAMS['exposure_auto'], 1, nothing)
    cv2.createTrackbar('Exposure (Man)', window_name, DEFAULT_CAMERA_PARAMS['exposure_abs'], 5000, nothing)

    # 3. 하드웨어 제어 - 초점 (New!)
    # Auto Focus: 1 (On), 0 (Off)
    cv2.createTrackbar('Auto Focus', window_name, DEFAULT_CAMERA_PARAMS['focus_auto'], 1, nothing)
    # Manual Focus Value: 0 ~ 255 (카메라마다 범위 다름, 보통 255가 10cm, 0이 무한대인 경우도 있음)
    cv2.createTrackbar('Focus (Man)', window_name, DEFAULT_CAMERA_PARAMS['focus_abs'], 500, nothing)

    print("[INFO] 컨트롤 설명:")
    print("  - [SW] B/C/S: 이미지 후처리")
    print("  - [HW] Exposure: Auto(0)으로 내리면 수동 조절 가능")
    print("  - [HW] Focus: Auto(0)으로 내리면 수동 조절 가능 (지원 카메라만!)")


def apply_image_adjustments(frame, window_name):
    """이미지 후처리 (밝기/대비/채도)"""
    brightness = cv2.getTrackbarPos('Brightness', window_name)
    contrast = cv2.getTrackbarPos('Contrast', window_name)
    saturation = cv2.getTrackbarPos('Saturation', window_name)

    alpha = contrast / 50.0 
    beta = (brightness - 50) * 2 
    adjusted = cv2.convertScaleAbs(frame, alpha=alpha, beta=beta)

    if saturation != 50:
        hsv = cv2.cvtColor(adjusted, cv2.COLOR_BGR2HSV).astype("float32")
        h, s, v = cv2.split(hsv)
        s = s * (saturation / 50.0)
        s = np.clip(s, 0, 255)
        hsv = cv2.merge([h, s, v])
        adjusted = cv2.cvtColor(hsv.astype("uint8"), cv2.COLOR_HSV2BGR)

    return adjusted


def capture_reference_images(side, camera_id, output_dir, headless=False, interval=2):
    os.makedirs(output_dir, exist_ok=True)

    print(f"[INFO] {side} 카메라 ({camera_id}) 초기화 중...")
    cap = cv2.VideoCapture(camera_id)

    if not cap.isOpened():
        print(f"[ERROR] 카메라를 열 수 없습니다: {camera_id}")
        return []

    # 해상도 설정
    cap.set(cv2.CAP_PROP_FRAME_WIDTH, 640)
    cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 480)
    cap.set(cv2.CAP_PROP_FPS, 30)

    # 워밍업
    print("[INFO] 카메라 워밍업 중... (3초)")
    for _ in range(30):
        cap.read()
    time.sleep(3)

    saved_files = []
    saved_count = 0
    window_name = f'Reference PCB Capture - {side.upper()}'
    window_initialized = False

    print(f"\n{'='*60}")
    print(f"기준 PCB 촬영 모드 ({side}) - [무한 모드]")
    print(f"{'='*60}")

    while True:
        print(f"\n[현재 저장된 이미지: {saved_count}장]")
        
        if headless:
            # Headless 모드 (초점 조절 불가, 자동 촬영만)
            print(f"{interval}초 후 자동 촬영합니다...")
            time.sleep(interval)
            ret, frame = cap.read()
            if not ret: continue
            final_frame = frame 

        else:
            # Interactive 모드
            print("스페이스바: 촬영 및 저장 | q: 종료")

            if not window_initialized:
                cv2.namedWindow(window_name, cv2.WINDOW_NORMAL)
                setup_camera_controls(window_name)
                window_initialized = True

            captured = False
            while True:
                # --- [하드웨어 제어: 노출 & 초점] ---
                try:
                    # 1. 노출 제어
                    auto_exp = cv2.getTrackbarPos('Auto Exposure', window_name)
                    exp_val = cv2.getTrackbarPos('Exposure (Man)', window_name)
                    
                    if auto_exp == 1:
                        cap.set(cv2.CAP_PROP_AUTO_EXPOSURE, 3) # Auto
                    else:
                        cap.set(cv2.CAP_PROP_AUTO_EXPOSURE, 1) # Manual
                        cap.set(cv2.CAP_PROP_EXPOSURE, exp_val)

                    # 2. 초점 제어 (New!)
                    auto_focus = cv2.getTrackbarPos('Auto Focus', window_name)
                    focus_val = cv2.getTrackbarPos('Focus (Man)', window_name)

                    if auto_focus == 1:
                        cap.set(cv2.CAP_PROP_AUTOFOCUS, 1) # Auto Focus ON
                    else:
                        cap.set(cv2.CAP_PROP_AUTOFOCUS, 0) # Auto Focus OFF
                        cap.set(cv2.CAP_PROP_FOCUS, focus_val) # 수동 값 적용
                except:
                    pass

                # 프레임 읽기
                ret, frame = cap.read()
                if not ret:
                    time.sleep(0.1)
                    continue

                # 소프트웨어 후처리
                adjusted_frame = apply_image_adjustments(frame, window_name)
                display_frame = adjusted_frame.copy()

                # UI 그리기
                info_text = f"[Saved: {saved_count}] SPACE: Save | Q: Quit"
                cv2.putText(display_frame, info_text, (10, 30), cv2.FONT_HERSHEY_SIMPLEX, 0.7, (0, 255, 0), 2)
                
                # 십자선 및 가이드
                h, w = display_frame.shape[:2]
                cv2.line(display_frame, (w//2, 0), (w//2, h), (0, 255, 0), 1)
                cv2.line(display_frame, (0, h//2), (w, h//2), (0, 255, 0), 1)
                
                margin = 50
                pts = [(margin, margin), (w-margin, margin), (margin, h-margin), (w-margin, h-margin)]
                for px, py in pts:
                    cv2.line(display_frame, (px-10, py), (px+10, py), (0, 255, 255), 2)
                    cv2.line(display_frame, (px, py-10), (px, py+10), (0, 255, 255), 2)

                cv2.imshow(window_name, display_frame)

                key = cv2.waitKey(30) & 0xFF
                if key == ord('q') or key == ord('Q'):
                    cap.release()
                    cv2.destroyAllWindows()
                    return saved_files
                elif key == ord(' '):
                    final_frame = adjusted_frame
                    captured = True
                    print("  찰칵!")
                    break

            if not captured:
                break

        # 저장
        saved_count += 1
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        filename = f"reference_{side}_{timestamp}_{saved_count}.jpg"
        filepath = os.path.join(output_dir, filename)

        cv2.imwrite(filepath, final_frame, [cv2.IMWRITE_JPEG_QUALITY, 95])
        print(f"[SUCCESS] 저장 완료: {filepath}")
        time.sleep(0.5)

    cap.release()
    cv2.destroyAllWindows()
    return saved_files

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--side', type=str, required=True, choices=['left', 'right'])
    parser.add_argument('--camera-id', type=int, default=0)
    parser.add_argument('--output', type=str, default='./reference_images')
    parser.add_argument('--headless', action='store_true')
    parser.add_argument('--interval', type=int, default=2)

    args = parser.parse_args()

    saved_files = capture_reference_images(
        side=args.side,
        camera_id=args.camera_id,
        output_dir=args.output,
        headless=args.headless,
        interval=args.interval
    )

    if saved_files:
        print(f"\n총 {len(saved_files)}장 저장됨. 경로: {args.output}")

if __name__ == '__main__':
    sys.exit(main())