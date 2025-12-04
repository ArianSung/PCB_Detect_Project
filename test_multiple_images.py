"""
여러 PCB 이미지에 대해 템플릿 매칭 테스트
"""

import cv2
import numpy as np
import sys
import os
from pathlib import Path

sys.path.append(str(Path(__file__).parent / 'server'))
from template_based_alignment import TemplateBasedAlignment


def test_single_image(image_path: str, template_path: str, output_name: str, output_dir: str = 'multi_image_results'):
    """
    단일 이미지 테스트
    """
    print(f"\n{'='*60}")
    print(f"테스트: {image_path}")
    print(f"{'='*60}\n")

    # 이미지 로드
    img = cv2.imread(image_path)
    if img is None:
        print(f"❌ 이미지 로드 실패: {image_path}")
        return None

    # 원본 크기 확인
    print(f"원본 크기: {img.shape}")

    # 640x640 리사이즈
    img_resized = cv2.resize(img, (640, 640))

    # 정렬 시스템 생성
    alignment = TemplateBasedAlignment()
    if not alignment.load_template(template_path):
        print(f"❌ 템플릿 로드 실패")
        return None

    # 기준점 찾기 (TM_CCORR_NORMED 사용 - 이전 테스트에서 가장 좋았음)
    reference_point = alignment.find_reference_point(img_resized, method=cv2.TM_CCORR_NORMED)

    if reference_point:
        # 시각화
        vis_img = alignment.visualize_reference_point(img_resized, reference_point)

        # 템플릿 영역도 표시
        template_h, template_w = alignment.template.shape[:2]
        ref_x, ref_y = reference_point
        top_left_x = ref_x - template_w // 2
        top_left_y = ref_y - template_h // 2

        cv2.rectangle(
            vis_img,
            (top_left_x, top_left_y),
            (top_left_x + template_w, top_left_y + template_h),
            (255, 0, 255),
            3
        )

        # 매칭 신뢰도 계산
        gray = cv2.cvtColor(img_resized, cv2.COLOR_BGR2GRAY)
        template_gray = cv2.cvtColor(alignment.template, cv2.COLOR_BGR2GRAY)
        result = cv2.matchTemplate(gray, template_gray, cv2.TM_CCORR_NORMED)
        min_val, max_val, min_loc, max_loc = cv2.minMaxLoc(result)
        confidence = max_val

        print(f"✅ 기준점 검출 성공!")
        print(f"   위치: {reference_point}")
        print(f"   신뢰도: {confidence:.4f} ({confidence*100:.2f}%)")

        # 결과 저장
        os.makedirs(output_dir, exist_ok=True)
        output_path = f"{output_dir}/{output_name}.jpg"
        cv2.imwrite(output_path, vis_img)
        print(f"📁 결과 저장: {output_path}")

        return {
            'image_path': image_path,
            'reference_point': reference_point,
            'confidence': confidence,
            'output_path': output_path
        }
    else:
        print(f"❌ 기준점 검출 실패")
        return None


if __name__ == '__main__':
    template_path = 'screw_hole_template.jpg'

    if not os.path.exists(template_path):
        print(f"❌ 템플릿 없음: {template_path}")
        exit(1)

    # 테스트할 이미지 목록
    test_images = [
        ('test_pcb_image.jpg', 'image1'),
        ('test_pcb_image2.jpg', 'image2'),
    ]

    results = []

    for image_path, output_name in test_images:
        if os.path.exists(image_path):
            result = test_single_image(image_path, template_path, output_name)
            if result:
                results.append(result)
        else:
            print(f"\n⚠️  이미지 없음: {image_path}")

    # 결과 요약
    print(f"\n{'='*60}")
    print("전체 테스트 요약")
    print(f"{'='*60}\n")

    if results:
        print(f"테스트 이미지: {len(results)}개")
        for i, result in enumerate(results, 1):
            print(f"\n{i}. {result['image_path']}")
            print(f"   기준점: {result['reference_point']}")
            print(f"   신뢰도: {result['confidence']:.4f} ({result['confidence']*100:.2f}%)")
            print(f"   결과: {result['output_path']}")

        # 평균 신뢰도
        avg_confidence = sum(r['confidence'] for r in results) / len(results)
        print(f"\n평균 매칭 신뢰도: {avg_confidence:.4f} ({avg_confidence*100:.2f}%)")
    else:
        print("❌ 성공한 테스트 없음")

    print(f"\n{'='*60}")
    print("✅ 테스트 완료!")
    print("📁 결과 폴더: multi_image_results/")
    print(f"{'='*60}\n")
