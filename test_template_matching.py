"""
템플릿 매칭 기반 정렬 시스템 테스트

PCB 이미지에서 나사 구멍을 찾아 기준점으로 설정하고,
상대 좌표 시스템을 테스트합니다.
"""

import cv2
import numpy as np
import sys
import os
from pathlib import Path
from typing import Tuple

sys.path.append(str(Path(__file__).parent / 'server'))
from template_based_alignment import TemplateBasedAlignment, create_template_from_coords


def extract_screw_hole_template(image_path: str, output_dir: str = 'templates'):
    """
    PCB 이미지에서 나사 구멍 템플릿 추출

    이미지를 확인하고 대략적인 나사 구멍 위치를 추정하여 템플릿 생성
    """
    img = cv2.imread(image_path)
    if img is None:
        print(f"❌ 이미지 로드 실패: {image_path}")
        return None

    print(f"이미지 크기: {img.shape}")

    # 640x640으로 리사이즈
    img_resized = cv2.resize(img, (640, 640))

    # 이미지 분석 결과:
    # PCB의 4개 코너 근처에 나사 구멍이 있음
    # 좌상단 나사 구멍을 기준점으로 사용
    # 640x640 이미지 기준으로 대략적인 위치 추정

    # 좌상단 나사 구멍 추정 위치 (이미지 확인 후)
    # PCB가 중앙에 있고, 좌상단 코너는 대략 (150, 150) 근처
    screw_candidates = [
        {'name': 'top_left', 'pos': (150, 150), 'desc': '좌상단 나사 구멍'},
        {'name': 'top_right', 'pos': (490, 150), 'desc': '우상단 나사 구멍'},
        {'name': 'bottom_left', 'pos': (150, 490), 'desc': '좌하단 나사 구멍'},
        {'name': 'bottom_right', 'pos': (490, 490), 'desc': '우하단 나사 구멍'},
    ]

    os.makedirs(output_dir, exist_ok=True)

    print(f"\n{'='*60}")
    print("나사 구멍 템플릿 추출")
    print(f"{'='*60}\n")

    templates = []
    for candidate in screw_candidates:
        name = candidate['name']
        x, y = candidate['pos']
        desc = candidate['desc']

        # 템플릿 크기: 40x40 (나사 구멍 + 주변 영역)
        template = create_template_from_coords(img_resized, x, y, size=40)

        output_path = f"{output_dir}/screw_{name}.jpg"
        cv2.imwrite(output_path, template)

        templates.append({
            'name': name,
            'path': output_path,
            'position': (x, y),
            'desc': desc
        })

        print(f"✅ {desc}: {output_path}")
        print(f"   추정 위치: ({x}, {y})")
        print(f"   템플릿 크기: {template.shape}\n")

    # 시각화: 추정 위치 표시
    vis_img = img_resized.copy()
    for candidate in screw_candidates:
        x, y = candidate['pos']
        cv2.circle(vis_img, (x, y), 20, (0, 255, 0), 2)
        cv2.putText(vis_img, candidate['name'], (x+25, y),
                   cv2.FONT_HERSHEY_SIMPLEX, 0.4, (0, 255, 0), 1)

    vis_path = f"{output_dir}/screw_positions_estimated.jpg"
    cv2.imwrite(vis_path, vis_img)
    print(f"📁 추정 위치 시각화: {vis_path}")

    return templates


def test_template_matching(image_path: str, template_info: dict):
    """
    템플릿 매칭 테스트
    """
    img = cv2.imread(image_path)
    img_resized = cv2.resize(img, (640, 640))

    print(f"\n{'='*60}")
    print(f"템플릿 매칭 테스트: {template_info['desc']}")
    print(f"{'='*60}\n")

    # 정렬 시스템 생성
    alignment = TemplateBasedAlignment()
    alignment.load_template(template_info['path'])

    # 기준점 찾기
    reference_point = alignment.find_reference_point(img_resized)

    if reference_point:
        print(f"✅ 기준점 검출 성공: {reference_point}")
        print(f"   추정 위치: {template_info['position']}")
        print(f"   오차: ({reference_point[0] - template_info['position'][0]}, "
              f"{reference_point[1] - template_info['position'][1]})")

        # 시각화
        vis_img = alignment.visualize_reference_point(img_resized, reference_point)
        output_path = f"template_matching_results/{template_info['name']}_result.jpg"
        os.makedirs('template_matching_results', exist_ok=True)
        cv2.imwrite(output_path, vis_img)
        print(f"📁 결과 저장: {output_path}")

        return reference_point
    else:
        print(f"❌ 기준점 검출 실패")
        return None


def simulate_yolo_detections(reference_point: Tuple[int, int]):
    """
    YOLO 검출 결과 시뮬레이션 및 상대 좌표 변환 테스트
    """
    print(f"\n{'='*60}")
    print("YOLO 검출 결과 → 상대 좌표 변환 테스트")
    print(f"{'='*60}\n")

    # 가상의 YOLO 검출 결과
    fake_detections = [
        {'class': 'capacitor', 'bbox': [200, 200, 20, 20], 'confidence': 0.95},
        {'class': 'resistor', 'bbox': [300, 250, 15, 10], 'confidence': 0.92},
        {'class': 'ic', 'bbox': [400, 300, 50, 50], 'confidence': 0.98},
    ]

    alignment = TemplateBasedAlignment()
    relative_dets = alignment.convert_to_relative_coords(fake_detections, reference_point)

    print(f"기준점: {reference_point}\n")
    for det in relative_dets:
        print(f"부품: {det['class']}")
        print(f"  절대 좌표: {det['absolute_position']}")
        print(f"  상대 좌표: {det['relative_position']}")
        print(f"  신뢰도: {det['confidence']:.2f}\n")


if __name__ == '__main__':
    image_path = 'test_pcb_image.jpg'

    if not os.path.exists(image_path):
        print(f"❌ 이미지 없음: {image_path}")
        exit(1)

    # 1. 나사 구멍 템플릿 추출
    print("=" * 60)
    print("STEP 1: 나사 구멍 템플릿 추출")
    print("=" * 60)
    templates = extract_screw_hole_template(image_path)

    # 2. 각 템플릿으로 매칭 테스트
    print(f"\n{'='*60}")
    print("STEP 2: 템플릿 매칭 테스트")
    print(f"{'='*60}")

    best_template = None
    best_ref_point = None

    for template_info in templates:
        ref_point = test_template_matching(image_path, template_info)

        if ref_point and best_ref_point is None:
            best_template = template_info
            best_ref_point = ref_point

    # 3. 상대 좌표 변환 테스트
    if best_ref_point:
        simulate_yolo_detections(best_ref_point)

        print(f"\n{'='*60}")
        print("최적 템플릿 및 기준점")
        print(f"{'='*60}")
        print(f"템플릿: {best_template['desc']}")
        print(f"템플릿 파일: {best_template['path']}")
        print(f"기준점 좌표: {best_ref_point}")
        print(f"\n✅ 이 템플릿을 사용하면 됩니다!")
    else:
        print(f"\n❌ 템플릿 매칭 실패")

    print(f"\n{'='*60}")
    print("✅ 테스트 완료!")
    print("📁 결과 폴더: templates/, template_matching_results/")
    print(f"{'='*60}\n")
