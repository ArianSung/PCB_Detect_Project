#!/usr/bin/env python3
"""
기준 PCB 데이터 생성 도구

촬영된 정상 PCB 이미지로부터 기준 데이터를 생성합니다:
- 나사 구멍 4개 위치 (Hough Circle Transform)
- 컴포넌트 위치 (YOLO 검출 결과)
- PCB 크기 및 구멍 간 거리

생성된 데이터는 JSON 형식으로 저장됩니다.

사용법:
    python tools/generate_reference_data.py --images server/reference_images --side left --output server/reference_data/reference_left.json
    python tools/generate_reference_data.py --images server/reference_images --side right --output server/reference_data/reference_right.json
"""

import cv2
import numpy as np
import json
import argparse
import os
import sys
from pathlib import Path
from ultralytics import YOLO


def detect_mounting_holes(image, debug=False):
    """
    Hough Circle Transform으로 나사 구멍 검출

    Args:
        image (np.ndarray): 입력 이미지 (BGR)
        debug (bool): 디버그 정보 출력 여부

    Returns:
        list: 검출된 구멍 좌표 [(x1, y1), (x2, y2), (x3, y3), (x4, y4)]
              또는 None (검출 실패 시)
    """
    # 그레이스케일 변환
    gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)

    # 가우시안 블러 (노이즈 제거)
    blurred = cv2.GaussianBlur(gray, (9, 9), 2)

    # Hough Circle Transform
    circles = cv2.HoughCircles(
        blurred,
        cv2.HOUGH_GRADIENT,
        dp=1,
        minDist=50,  # 구멍 간 최소 거리
        param1=50,   # Canny edge detection threshold
        param2=30,   # Circle detection threshold
        minRadius=5,  # 최소 반지름
        maxRadius=20  # 최대 반지름
    )

    if circles is None:
        if debug:
            print("[WARNING] 나사 구멍을 검출할 수 없습니다.")
        return None

    circles = np.uint16(np.around(circles))
    holes = [(int(x), int(y)) for x, y, r in circles[0, :]]

    if debug:
        print(f"[DEBUG] 검출된 원형 객체: {len(holes)}개")
        for i, (x, y) in enumerate(holes):
            print(f"  [{i+1}] x={x}, y={y}")

    # 4개 코너 구멍 선택 (가장 외곽에 있는 4점)
    if len(holes) < 4:
        if debug:
            print("[WARNING] 4개 미만의 구멍이 검출되었습니다.")
        return None

    corner_holes = select_corner_holes(holes, debug=debug)

    if len(corner_holes) != 4:
        if debug:
            print("[WARNING] 4개의 코너 구멍을 선택할 수 없습니다.")
        return None

    return corner_holes


def select_corner_holes(holes, debug=False):
    """
    검출된 구멍 중 4개 코너 구멍 선택

    Args:
        holes (list): 검출된 모든 구멍 좌표
        debug (bool): 디버그 정보 출력

    Returns:
        list: 4개 코너 구멍 [top_left, top_right, bottom_right, bottom_left]
    """
    holes = np.array(holes)

    # x + y 최소 → top-left
    top_left = holes[np.argmin(holes[:, 0] + holes[:, 1])]

    # x - y 최대 → top-right
    top_right = holes[np.argmax(holes[:, 0] - holes[:, 1])]

    # x + y 최대 → bottom-right
    bottom_right = holes[np.argmax(holes[:, 0] + holes[:, 1])]

    # y - x 최대 → bottom-left
    bottom_left = holes[np.argmax(holes[:, 1] - holes[:, 0])]

    corner_holes = [
        (int(top_left[0]), int(top_left[1])),
        (int(top_right[0]), int(top_right[1])),
        (int(bottom_right[0]), int(bottom_right[1])),
        (int(bottom_left[0]), int(bottom_left[1]))
    ]

    if debug:
        print("[DEBUG] 선택된 코너 구멍:")
        print(f"  Top-Left: {corner_holes[0]}")
        print(f"  Top-Right: {corner_holes[1]}")
        print(f"  Bottom-Right: {corner_holes[2]}")
        print(f"  Bottom-Left: {corner_holes[3]}")

    return corner_holes


def calculate_hole_distances(holes):
    """
    4개 구멍 간 거리 계산

    Args:
        holes (list): 4개 구멍 좌표 [(x1,y1), (x2,y2), (x3,y3), (x4,y4)]

    Returns:
        dict: 거리 정보
            - width: 상단 가로 거리
            - height: 좌측 세로 거리
            - diagonal: 대각선 거리
    """
    p1, p2, p3, p4 = holes  # top_left, top_right, bottom_right, bottom_left

    width = np.sqrt((p2[0] - p1[0])**2 + (p2[1] - p1[1])**2)
    height = np.sqrt((p4[0] - p1[0])**2 + (p4[1] - p1[1])**2)
    diagonal = np.sqrt((p3[0] - p1[0])**2 + (p3[1] - p1[1])**2)

    return {
        'width': float(width),
        'height': float(height),
        'diagonal': float(diagonal)
    }


def detect_components_with_yolo(image, model_path, debug=False):
    """
    YOLO 모델로 컴포넌트 검출

    Args:
        image (np.ndarray): 입력 이미지
        model_path (str): YOLO 모델 경로
        debug (bool): 디버그 정보 출력

    Returns:
        list: 검출된 컴포넌트 정보
            [{'class_name': str, 'bbox': [x1, y1, x2, y2], 'center': [cx, cy], 'confidence': float}, ...]
    """
    if debug:
        print(f"[DEBUG] YOLO 모델 로드: {model_path}")

    # YOLO 모델 로드
    model = YOLO(model_path)

    # 추론
    results = model.predict(image, conf=0.25, verbose=False)

    components = []

    if len(results) > 0 and len(results[0].boxes) > 0:
        boxes = results[0].boxes

        for box in boxes:
            # 바운딩 박스 좌표
            x1, y1, x2, y2 = box.xyxy[0].cpu().numpy()

            # 중심점
            cx = (x1 + x2) / 2
            cy = (y1 + y2) / 2

            # 클래스 이름 및 신뢰도
            class_id = int(box.cls[0])
            class_name = model.names[class_id]
            confidence = float(box.conf[0])

            components.append({
                'class_name': class_name,
                'bbox': [float(x1), float(y1), float(x2), float(y2)],
                'center': [float(cx), float(cy)],
                'confidence': confidence
            })

    if debug:
        print(f"[DEBUG] 검출된 컴포넌트: {len(components)}개")

    return components


def generate_reference_data(image_paths, side, model_path, debug=False):
    """
    기준 PCB 데이터 생성

    Args:
        image_paths (list): 이미지 파일 경로 리스트
        side (str): 'left' 또는 'right'
        model_path (str): YOLO 모델 경로
        debug (bool): 디버그 모드

    Returns:
        dict: 기준 데이터
    """
    print(f"\n{'='*60}")
    print(f"기준 데이터 생성 시작 ({side})")
    print(f"{'='*60}")

    # 여러 이미지 중 가장 좋은 결과 선택
    best_result = None
    best_score = 0

    for img_path in image_paths:
        print(f"\n[INFO] 처리 중: {img_path}")

        # 이미지 읽기
        image = cv2.imread(img_path)
        if image is None:
            print(f"[ERROR] 이미지를 읽을 수 없습니다: {img_path}")
            continue

        h, w = image.shape[:2]
        print(f"  - 해상도: {w}x{h}")

        # 1. 나사 구멍 검출
        holes = detect_mounting_holes(image, debug=debug)
        if holes is None:
            print(f"[WARNING] 나사 구멍 검출 실패: {img_path}")
            continue

        # 2. 컴포넌트 검출
        components = detect_components_with_yolo(image, model_path, debug=debug)

        # 점수 계산 (구멍 4개 + 컴포넌트 개수)
        score = 100 + len(components)

        print(f"  - 나사 구멍: {len(holes)}개")
        print(f"  - 컴포넌트: {len(components)}개")
        print(f"  - 점수: {score}")

        if score > best_score:
            best_score = score
            best_result = {
                'image_path': img_path,
                'image_size': {'width': w, 'height': h},
                'mounting_holes': holes,
                'hole_distances': calculate_hole_distances(holes),
                'components': components,
                'side': side
            }

    if best_result is None:
        print("[ERROR] 유효한 기준 데이터를 생성할 수 없습니다.")
        return None

    print(f"\n{'='*60}")
    print(f"최적 이미지 선택: {best_result['image_path']}")
    print(f"  - 나사 구멍: {len(best_result['mounting_holes'])}개")
    print(f"  - 컴포넌트: {len(best_result['components'])}개")
    print(f"{'='*60}")

    return best_result


def main():
    parser = argparse.ArgumentParser(
        description='기준 PCB 데이터 생성 도구'
    )

    parser.add_argument(
        '--images',
        type=str,
        required=True,
        help='촬영된 이미지 디렉토리 또는 파일 경로'
    )

    parser.add_argument(
        '--side',
        type=str,
        required=True,
        choices=['left', 'right'],
        help='카메라 위치 (left: 좌측, right: 우측)'
    )

    parser.add_argument(
        '--model',
        type=str,
        default='../runs/detect/roboflow_pcb_balanced/weights/best.pt',
        help='YOLO 모델 경로 (기본값: ../runs/detect/roboflow_pcb_balanced/weights/best.pt)'
    )

    parser.add_argument(
        '--output',
        type=str,
        default=None,
        help='출력 JSON 파일 경로 (기본값: server/reference_data/reference_{side}.json)'
    )

    parser.add_argument(
        '--debug',
        action='store_true',
        help='디버그 모드 활성화'
    )

    args = parser.parse_args()

    # 이미지 파일 목록 수집
    images_path = Path(args.images)

    if images_path.is_file():
        image_paths = [str(images_path)]
    elif images_path.is_dir():
        # 디렉토리 내 모든 JPG/PNG 파일
        image_paths = []
        for ext in ['*.jpg', '*.jpeg', '*.png']:
            image_paths.extend([str(p) for p in images_path.glob(ext)])

        # side에 해당하는 이미지만 필터링
        image_paths = [p for p in image_paths if args.side in p]

        if not image_paths:
            print(f"[ERROR] {images_path}에서 {args.side} 이미지를 찾을 수 없습니다.")
            return 1
    else:
        print(f"[ERROR] 유효하지 않은 경로: {args.images}")
        return 1

    print(f"[INFO] 총 {len(image_paths)}개의 이미지 발견")

    # 기준 데이터 생성
    reference_data = generate_reference_data(
        image_paths=image_paths,
        side=args.side,
        model_path=args.model,
        debug=args.debug
    )

    if reference_data is None:
        return 1

    # 출력 파일 경로 결정
    if args.output is None:
        output_dir = Path('server/reference_data')
        output_dir.mkdir(parents=True, exist_ok=True)
        output_path = output_dir / f'reference_{args.side}.json'
    else:
        output_path = Path(args.output)
        output_path.parent.mkdir(parents=True, exist_ok=True)

    # JSON 저장
    with open(output_path, 'w', encoding='utf-8') as f:
        json.dump(reference_data, f, indent=2, ensure_ascii=False)

    print(f"\n[SUCCESS] 기준 데이터 저장 완료: {output_path}")
    print(f"  - 나사 구멍: {len(reference_data['mounting_holes'])}개")
    print(f"  - 컴포넌트: {len(reference_data['components'])}개")
    print(f"  - PCB 크기: {reference_data['image_size']['width']}x{reference_data['image_size']['height']}")

    return 0


if __name__ == '__main__':
    sys.exit(main())
