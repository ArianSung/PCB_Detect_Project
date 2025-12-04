"""
실제 나사 구멍 템플릿으로 매칭 테스트
"""

import cv2
import numpy as np
import sys
import os
from pathlib import Path

sys.path.append(str(Path(__file__).parent / 'server'))
from template_based_alignment import TemplateBasedAlignment


def test_with_real_template(image_path: str, template_path: str, output_dir: str = 'real_template_results'):
    """
    실제 나사 구멍 템플릿으로 매칭
    """
    print(f"\n{'='*60}")
    print("실제 나사 구멍 템플릿 매칭 테스트")
    print(f"{'='*60}\n")

    # 이미지 로드
    img = cv2.imread(image_path)
    if img is None:
        print(f"❌ 이미지 로드 실패: {image_path}")
        return

    # 640x640 리사이즈
    img_resized = cv2.resize(img, (640, 640))
    print(f"이미지 크기: {img_resized.shape}")

    # 정렬 시스템 생성
    alignment = TemplateBasedAlignment()
    if not alignment.load_template(template_path):
        print(f"❌ 템플릿 로드 실패")
        return

    print(f"템플릿 크기: {alignment.template.shape}")

    # 여러 매칭 방법 테스트
    methods = [
        ('TM_CCOEFF_NORMED', cv2.TM_CCOEFF_NORMED),
        ('TM_CCORR_NORMED', cv2.TM_CCORR_NORMED),
        ('TM_SQDIFF_NORMED', cv2.TM_SQDIFF_NORMED),
    ]

    os.makedirs(output_dir, exist_ok=True)

    best_result = None
    best_confidence = 0

    for method_name, method in methods:
        print(f"\n{'='*60}")
        print(f"매칭 방법: {method_name}")
        print(f"{'='*60}")

        # 기준점 찾기
        reference_point = alignment.find_reference_point(img_resized, method=method)

        if reference_point:
            # 시각화
            vis_img = alignment.visualize_reference_point(img_resized, reference_point)

            # 원본 이미지에 템플릿 위치 표시
            template_h, template_w = alignment.template.shape[:2]
            ref_x, ref_y = reference_point
            top_left_x = ref_x - template_w // 2
            top_left_y = ref_y - template_h // 2

            # 템플릿 영역 강조
            cv2.rectangle(
                vis_img,
                (top_left_x, top_left_y),
                (top_left_x + template_w, top_left_y + template_h),
                (255, 0, 255),  # 보라색
                3
            )

            # 매칭 스코어 계산
            gray = cv2.cvtColor(img_resized, cv2.COLOR_BGR2GRAY)
            template_gray = cv2.cvtColor(alignment.template, cv2.COLOR_BGR2GRAY)
            result = cv2.matchTemplate(gray, template_gray, method)
            min_val, max_val, min_loc, max_loc = cv2.minMaxLoc(result)

            if method in [cv2.TM_SQDIFF, cv2.TM_SQDIFF_NORMED]:
                confidence = 1 - min_val
            else:
                confidence = max_val

            print(f"✅ 기준점 검출 성공: {reference_point}")
            print(f"   매칭 신뢰도: {confidence:.4f}")
            print(f"   템플릿 영역: ({top_left_x}, {top_left_y}) ~ ({top_left_x + template_w}, {top_left_y + template_h})")

            # 결과 저장
            output_path = f"{output_dir}/{method_name}.jpg"
            cv2.imwrite(output_path, vis_img)
            print(f"📁 결과 저장: {output_path}")

            if confidence > best_confidence:
                best_confidence = confidence
                best_result = {
                    'method': method_name,
                    'reference_point': reference_point,
                    'confidence': confidence,
                    'image': vis_img.copy()
                }
        else:
            print(f"❌ 기준점 검출 실패")

    # 최적 결과
    if best_result:
        print(f"\n{'='*60}")
        print("최적 매칭 결과")
        print(f"{'='*60}")
        print(f"매칭 방법: {best_result['method']}")
        print(f"기준점: {best_result['reference_point']}")
        print(f"신뢰도: {best_result['confidence']:.4f}")

        cv2.imwrite(f"{output_dir}/BEST_MATCH.jpg", best_result['image'])
        print(f"\n📁 최적 결과 저장: {output_dir}/BEST_MATCH.jpg")

        # 기준점을 (0,0)으로 설정한 좌표계 시각화
        print(f"\n{'='*60}")
        print("상대 좌표 시스템")
        print(f"{'='*60}")
        print(f"기준점(REF): {best_result['reference_point']} → (0, 0)")
        print(f"\n예시: YOLO가 (300, 300)에서 부품을 검출했다면")
        print(f"  절대 좌표: (300, 300)")
        print(f"  상대 좌표: ({300 - best_result['reference_point'][0]}, {300 - best_result['reference_point'][1]})")

        return best_result
    else:
        print(f"\n❌ 매칭 실패")
        return None


if __name__ == '__main__':
    image_path = 'test_pcb_image.jpg'
    template_path = 'screw_hole_template.jpg'

    if not os.path.exists(image_path):
        print(f"❌ 이미지 없음: {image_path}")
        exit(1)

    if not os.path.exists(template_path):
        print(f"❌ 템플릿 없음: {template_path}")
        exit(1)

    result = test_with_real_template(image_path, template_path)

    print(f"\n{'='*60}")
    print("✅ 테스트 완료!")
    print("📁 결과 폴더: real_template_results/")
    print(f"{'='*60}\n")
