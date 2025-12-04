"""
커스텀 ROI로 PCB 테두리 검출 테스트
실제 PCB 위치를 보고 ROI 영역을 재조정합니다.
"""

import cv2
import numpy as np
import sys
import os
from pathlib import Path

sys.path.append(str(Path(__file__).parent / 'server'))
from pcb_edge_detector import detect_pcb_edges


def test_custom_rois(image_path: str):
    """
    실제 이미지 분석 후 커스텀 ROI로 테스트
    """
    # 이미지 로드
    img = cv2.imread(image_path)
    if img is None:
        print(f"❌ 이미지 로드 실패: {image_path}")
        return

    print(f"원본 이미지 크기: {img.shape}")

    # 640x640 리사이즈
    img_resized = cv2.resize(img, (640, 640))
    print(f"리사이즈 후: {img_resized.shape}")

    # 이미지 분석 결과 기반 커스텀 ROI
    # PCB가 중앙에 위치하고 약간 기울어져 있음
    custom_rois = {
        # Top: PCB 상단 테두리 (중앙 상단, 약간 넓게)
        'top':    (200, 80,  240, 80),   # (x, y, width, height)

        # Bottom: PCB 하단 테두리 (중앙 하단, 약간 넓게)
        'bottom': (200, 480, 240, 80),

        # Left: PCB 좌측 테두리 (중앙 좌측, 약간 넓게)
        'left':   (80,  200, 80, 240),

        # Right: PCB 우측 테두리 (중앙 우측, 약간 넓게)
        'right':  (480, 200, 80, 240)
    }

    # 여러 임계값 조합 테스트
    threshold_sets = [
        {'name': 'very_low', 'values': {'top': 10, 'bottom': 25, 'left': 8, 'right': 30}},
        {'name': 'low', 'values': {'top': 15, 'bottom': 30, 'left': 10, 'right': 40}},
        {'name': 'medium', 'values': {'top': 20, 'bottom': 40, 'left': 15, 'right': 50}},
        {'name': 'high', 'values': {'top': 30, 'bottom': 50, 'left': 20, 'right': 60}},
    ]

    os.makedirs('custom_roi_results', exist_ok=True)

    print(f"\n{'='*60}")
    print("커스텀 ROI 테스트 시작")
    print(f"{'='*60}")
    print(f"\nROI 설정:")
    for direction, (x, y, w, h) in custom_rois.items():
        print(f"  {direction:8s}: x={x:3d}, y={y:3d}, w={w:3d}, h={h:3d}")

    best_result = None
    best_corners = 0

    for threshold_set in threshold_sets:
        name = threshold_set['name']
        thresholds = threshold_set['values']

        print(f"\n{'='*60}")
        print(f"테스트: {name}")
        print(f"{'='*60}")
        print(f"임계값: {thresholds}")

        # 검출
        corners, debug_img = detect_pcb_edges(
            img_resized,
            thresholds=thresholds,
            rois=custom_rois,
            draw_debug=True
        )

        # 결과
        if corners:
            num_corners = len(corners)
            print(f"✅ 코너 {num_corners}개 검출:")
            for corner_name, (x, y) in corners.items():
                print(f"   - {corner_name}: ({x:3d}, {y:3d})")

            if num_corners > best_corners:
                best_corners = num_corners
                best_result = {
                    'name': name,
                    'thresholds': thresholds,
                    'corners': corners,
                    'image': debug_img.copy()
                }
        else:
            print(f"❌ 코너 검출 실패")

        # 저장
        output_path = f"custom_roi_results/{name}.jpg"
        cv2.imwrite(output_path, debug_img)
        print(f"📁 저장: {output_path}")

    # 최적 결과 출력
    print(f"\n{'='*60}")
    print("최적 결과")
    print(f"{'='*60}")
    if best_result:
        print(f"✅ 최적 설정: {best_result['name']}")
        print(f"   임계값: {best_result['thresholds']}")
        print(f"   검출 코너: {best_corners}개")
        print(f"\n코너 좌표:")
        for corner_name, (x, y) in best_result['corners'].items():
            print(f"   - {corner_name}: ({x:3d}, {y:3d})")

        # 최적 결과를 별도로 저장
        cv2.imwrite('custom_roi_results/BEST_RESULT.jpg', best_result['image'])
        print(f"\n📁 최적 결과 저장: custom_roi_results/BEST_RESULT.jpg")
    else:
        print("❌ 코너를 검출하지 못했습니다.")

    return best_result


def visualize_roi_only(image_path: str):
    """
    ROI 영역만 시각화 (검출 없이)
    """
    img = cv2.imread(image_path)
    img_resized = cv2.resize(img, (640, 640))

    # 커스텀 ROI
    custom_rois = {
        'top':    (200, 80,  240, 80),
        'bottom': (200, 480, 240, 80),
        'left':   (80,  200, 80, 240),
        'right':  (480, 200, 80, 240)
    }

    # ROI 영역 그리기
    colors = {
        'top': (0, 0, 255),      # 빨강
        'bottom': (255, 0, 0),   # 파랑
        'left': (0, 255, 0),     # 초록
        'right': (0, 255, 255)   # 노랑
    }

    result_img = img_resized.copy()
    for direction, (x, y, w, h) in custom_rois.items():
        color = colors[direction]
        cv2.rectangle(result_img, (x, y), (x + w, y + h), color, 2)
        cv2.putText(result_img, direction, (x + 5, y + 20),
                   cv2.FONT_HERSHEY_SIMPLEX, 0.6, color, 2)

    os.makedirs('custom_roi_results', exist_ok=True)
    cv2.imwrite('custom_roi_results/ROI_VISUALIZATION.jpg', result_img)
    print(f"📁 ROI 시각화 저장: custom_roi_results/ROI_VISUALIZATION.jpg")

    return result_img


if __name__ == '__main__':
    image_path = 'test_pcb_image.jpg'

    if not os.path.exists(image_path):
        print(f"❌ 이미지 없음: {image_path}")
        exit(1)

    # ROI 영역 시각화
    print("1. ROI 영역 시각화 중...")
    visualize_roi_only(image_path)

    # 커스텀 ROI로 테스트
    print("\n2. 커스텀 ROI로 테두리 검출 테스트 중...")
    best_result = test_custom_rois(image_path)

    print(f"\n{'='*60}")
    print("✅ 테스트 완료!")
    print("📁 결과 폴더: custom_roi_results/")
    print(f"{'='*60}\n")
