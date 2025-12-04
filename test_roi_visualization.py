"""
ROI 영역 시각화 테스트
"""

import cv2
import numpy as np

def visualize_roi():
    """ROI 영역을 시각화하여 저장"""

    # 테스트 이미지 로드
    img = cv2.imread('test_new_pcb.jpg')
    if img is None:
        print("❌ 이미지 로드 실패")
        return

    # 640x640 리사이즈
    img_resized = cv2.resize(img, (640, 640))

    # ROI 영역 정의 (Flask와 동일)
    img_h, img_w = img_resized.shape[:2]
    roi_width = 200
    roi_height_margin = 30
    roi_x1 = (img_w - roi_width) // 2  # 220
    roi_x2 = roi_x1 + roi_width  # 420
    roi_y1 = roi_height_margin  # 30
    roi_y2 = img_h - roi_height_margin  # 610

    print(f"이미지 크기: {img_w} x {img_h}")
    print(f"ROI 영역: x={roi_x1}~{roi_x2}, y={roi_y1}~{roi_y2}")
    print(f"ROI 크기: {roi_x2-roi_x1} x {roi_y2-roi_y1}")

    # 시각화
    result_img = img_resized.copy()

    # ROI 영역 표시 (노란색)
    cv2.rectangle(result_img, (roi_x1, roi_y1), (roi_x2, roi_y2), (0, 255, 255), 3)

    # ROI 텍스트
    cv2.putText(
        result_img,
        "ROI (중앙, 세로 길게)",
        (roi_x1 + 10, roi_y1 + 30),
        cv2.FONT_HERSHEY_SIMPLEX,
        0.7,
        (0, 255, 255),
        2
    )

    # 기준점 위치 (171, 112) 표시 - 템플릿이 있는 곳
    template_pos = (171, 112)
    cv2.circle(result_img, template_pos, 10, (0, 0, 255), -1)
    cv2.putText(
        result_img,
        f"Template: {template_pos}",
        (template_pos[0] + 15, template_pos[1]),
        cv2.FONT_HERSHEY_SIMPLEX,
        0.6,
        (0, 0, 255),
        2
    )

    # 템플릿 영역 표시 (49x49)
    template_size = 49
    top_left_x = template_pos[0] - template_size // 2
    top_left_y = template_pos[1] - template_size // 2
    cv2.rectangle(
        result_img,
        (top_left_x, top_left_y),
        (top_left_x + template_size, top_left_y + template_size),
        (255, 0, 255),
        2
    )

    # 상태 텍스트
    status_text = "템플릿이 ROI 밖에 있음 → 거부됨"
    cv2.putText(
        result_img,
        status_text,
        (10, img_h - 20),
        cv2.FONT_HERSHEY_SIMPLEX,
        0.8,
        (0, 0, 255),
        2
    )

    # 저장
    output_path = 'roi_visualization.jpg'
    cv2.imwrite(output_path, result_img)
    print(f"\n✅ ROI 시각화 저장: {output_path}")

    # 정보 출력
    print(f"\n📊 분석:")
    print(f"  - 템플릿 위치: ({template_pos[0]}, {template_pos[1]})")
    print(f"  - ROI X 범위: {roi_x1} ~ {roi_x2}")
    print(f"  - ROI Y 범위: {roi_y1} ~ {roi_y2}")
    print(f"  - 템플릿이 ROI X 범위 안에 있나? {roi_x1 <= template_pos[0] <= roi_x2}")
    print(f"  - 템플릿이 ROI Y 범위 안에 있나? {roi_y1 <= template_pos[1] <= roi_y2}")
    print(f"  - 결론: ❌ ROI 밖 (x={template_pos[0]} < {roi_x1})")


if __name__ == '__main__':
    visualize_roi()
