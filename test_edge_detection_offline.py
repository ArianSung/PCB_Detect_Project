"""
오프라인 PCB 테두리 검출 파라미터 테스트 스크립트

저장된 이미지를 불러와서 여러 ROI/임계값 조합을 테스트하고
결과를 시각적으로 확인할 수 있도록 이미지 파일로 저장합니다.

사용법:
    python test_edge_detection_offline.py
"""

import cv2
import numpy as np
import sys
import os
from pathlib import Path

# 서버 모듈 경로 추가
sys.path.append(str(Path(__file__).parent / 'server'))

from pcb_edge_detector import detect_pcb_edges


def test_with_default_params(image_path: str, output_dir: str = 'edge_detection_results'):
    """
    기본 파라미터로 테스트
    """
    print(f"\n{'='*60}")
    print("기본 파라미터 테스트")
    print(f"{'='*60}")

    # 이미지 로드
    img = cv2.imread(image_path)
    if img is None:
        print(f"❌ 이미지를 불러올 수 없습니다: {image_path}")
        return

    print(f"✅ 이미지 로드 성공: {img.shape}")

    # 640x640 리사이즈
    img_resized = cv2.resize(img, (640, 640))

    # 기본 파라미터로 검출
    corners, debug_img = detect_pcb_edges(img_resized, draw_debug=True)

    # 결과 출력
    if corners:
        print(f"✅ 코너 검출 성공: {len(corners)}개")
        for corner_name, (x, y) in corners.items():
            print(f"   - {corner_name}: ({x}, {y})")
    else:
        print("❌ 코너 검출 실패")

    # 결과 저장
    os.makedirs(output_dir, exist_ok=True)
    output_path = f"{output_dir}/default_params.jpg"
    cv2.imwrite(output_path, debug_img)
    print(f"📁 결과 저장: {output_path}")

    return corners, debug_img


def test_threshold_variations(image_path: str, output_dir: str = 'edge_detection_results'):
    """
    여러 임계값 조합 테스트
    """
    print(f"\n{'='*60}")
    print("임계값 변화 테스트")
    print(f"{'='*60}")

    # 이미지 로드
    img = cv2.imread(image_path)
    img_resized = cv2.resize(img, (640, 640))

    # 기본 임계값
    base_thresholds = {
        'top': 21,
        'bottom': 48,
        'left': 13,
        'right': 63
    }

    # 테스트할 임계값 변화 (±10, ±20)
    variations = [
        {'name': 'default', 'multiplier': 1.0},
        {'name': 'lower_10', 'multiplier': 0.8},
        {'name': 'higher_10', 'multiplier': 1.2},
        {'name': 'lower_20', 'multiplier': 0.6},
        {'name': 'higher_20', 'multiplier': 1.4},
    ]

    results = []
    os.makedirs(output_dir, exist_ok=True)

    for var in variations:
        # 임계값 조정
        thresholds = {
            direction: int(value * var['multiplier'])
            for direction, value in base_thresholds.items()
        }

        print(f"\n테스트: {var['name']} (multiplier={var['multiplier']})")
        print(f"  임계값: {thresholds}")

        # 검출
        corners, debug_img = detect_pcb_edges(img_resized, thresholds=thresholds, draw_debug=True)

        # 결과
        if corners:
            print(f"  ✅ 코너 {len(corners)}개 검출")
            results.append({'name': var['name'], 'corners': len(corners), 'success': True})
        else:
            print(f"  ❌ 검출 실패")
            results.append({'name': var['name'], 'corners': 0, 'success': False})

        # 저장
        output_path = f"{output_dir}/threshold_{var['name']}.jpg"
        cv2.imwrite(output_path, debug_img)
        print(f"  📁 저장: {output_path}")

    # 요약
    print(f"\n{'='*60}")
    print("임계값 테스트 요약")
    print(f"{'='*60}")
    for r in results:
        status = "✅" if r['success'] else "❌"
        print(f"{status} {r['name']:15s} - 코너 {r['corners']}개")

    return results


def test_roi_variations(image_path: str, output_dir: str = 'edge_detection_results'):
    """
    ROI 영역 변화 테스트
    """
    print(f"\n{'='*60}")
    print("ROI 영역 변화 테스트")
    print(f"{'='*60}")

    # 이미지 로드
    img = cv2.imread(image_path)
    img_resized = cv2.resize(img, (640, 640))

    # 기본 ROI
    base_rois = {
        'top':    (250, 20,  140, 150),
        'bottom': (250, 470, 140, 150),
        'left':   (20,  250, 150, 140),
        'right':  (470, 250, 150, 140)
    }

    # ROI 변화 테스트
    variations = [
        {'name': 'default', 'offset': (0, 0)},
        {'name': 'wider', 'offset': (20, 20)},  # 더 넓은 ROI
        {'name': 'narrower', 'offset': (-20, -20)},  # 더 좁은 ROI
    ]

    results = []
    os.makedirs(output_dir, exist_ok=True)

    for var in variations:
        # ROI 조정
        offset_w, offset_h = var['offset']
        rois = {}

        for direction, (x, y, w, h) in base_rois.items():
            if direction in ['top', 'bottom']:
                # 수평 방향: 너비 조정
                new_x = max(0, x - offset_w // 2)
                new_w = w + offset_w
            else:
                # 수직 방향: 높이 조정
                new_y = y - offset_h // 2
                new_h = h + offset_h
                new_x, new_w = x, w
                y = new_y
                h = new_h

            rois[direction] = (int(new_x), int(y), int(new_w), int(h))

        print(f"\n테스트: {var['name']}")
        print(f"  ROI offset: {var['offset']}")

        # 검출
        corners, debug_img = detect_pcb_edges(img_resized, rois=rois, draw_debug=True)

        # 결과
        if corners:
            print(f"  ✅ 코너 {len(corners)}개 검출")
            results.append({'name': var['name'], 'corners': len(corners), 'success': True})
        else:
            print(f"  ❌ 검출 실패")
            results.append({'name': var['name'], 'corners': 0, 'success': False})

        # 저장
        output_path = f"{output_dir}/roi_{var['name']}.jpg"
        cv2.imwrite(output_path, debug_img)
        print(f"  📁 저장: {output_path}")

    # 요약
    print(f"\n{'='*60}")
    print("ROI 테스트 요약")
    print(f"{'='*60}")
    for r in results:
        status = "✅" if r['success'] else "❌"
        print(f"{status} {r['name']:15s} - 코너 {r['corners']}개")

    return results


def main():
    # 테스트 이미지 경로
    image_path = 'test_pcb_image.jpg'

    if not os.path.exists(image_path):
        print(f"❌ 테스트 이미지가 없습니다: {image_path}")
        return

    output_dir = 'edge_detection_results'

    # 1. 기본 파라미터 테스트
    test_with_default_params(image_path, output_dir)

    # 2. 임계값 변화 테스트
    test_threshold_variations(image_path, output_dir)

    # 3. ROI 변화 테스트
    test_roi_variations(image_path, output_dir)

    print(f"\n{'='*60}")
    print(f"✅ 모든 테스트 완료!")
    print(f"📁 결과 폴더: {output_dir}/")
    print(f"{'='*60}\n")


if __name__ == '__main__':
    main()
