"""
PCB 이미지에서 나사 구멍 템플릿 추출

사용자가 마우스로 나사 구멍 영역을 선택하면 템플릿으로 저장합니다.
"""

import cv2
import numpy as np
import os


# 전역 변수
img = None
img_display = None
template_coords = []
selecting = False
start_point = None


def mouse_callback(event, x, y, flags, param):
    """마우스 콜백 함수"""
    global img_display, template_coords, selecting, start_point

    if event == cv2.EVENT_LBUTTONDOWN:
        selecting = True
        start_point = (x, y)

    elif event == cv2.EVENT_MOUSEMOVE and selecting:
        img_display = img.copy()
        cv2.rectangle(img_display, start_point, (x, y), (0, 255, 0), 2)
        cv2.imshow('Select Screw Hole', img_display)

    elif event == cv2.EVENT_LBUTTONUP:
        selecting = False
        end_point = (x, y)

        # 좌표 정렬 (top-left, bottom-right)
        x1 = min(start_point[0], end_point[0])
        y1 = min(start_point[1], end_point[1])
        x2 = max(start_point[0], end_point[0])
        y2 = max(start_point[1], end_point[1])

        template_coords.append((x1, y1, x2, y2))

        # 영역 표시
        img_display = img.copy()
        for i, (tx1, ty1, tx2, ty2) in enumerate(template_coords):
            cv2.rectangle(img_display, (tx1, ty1), (tx2, ty2), (0, 255, 0), 2)
            cv2.putText(img_display, f"#{i+1}", (tx1, ty1-5),
                       cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 255, 0), 2)

        cv2.imshow('Select Screw Hole', img_display)
        print(f"템플릿 #{len(template_coords)} 선택 완료: ({x1}, {y1}) ~ ({x2}, {y2})")


def extract_templates_interactive(image_path: str, output_dir: str = 'templates'):
    """
    대화형으로 나사 구멍 템플릿 추출
    """
    global img, img_display

    img = cv2.imread(image_path)
    if img is None:
        print(f"❌ 이미지 로드 실패: {image_path}")
        return

    img_display = img.copy()
    os.makedirs(output_dir, exist_ok=True)

    print(f"\n{'='*60}")
    print("나사 구멍 템플릿 추출 도구")
    print(f"{'='*60}")
    print("사용법:")
    print("  1. 마우스로 나사 구멍 영역을 드래그하여 선택")
    print("  2. 여러 개 선택 가능 (4개 나사 구멍 모두 선택 권장)")
    print("  3. 'q' 키를 눌러 저장 및 종료")
    print("  4. 'r' 키를 눌러 초기화")
    print(f"{'='*60}\n")

    cv2.namedWindow('Select Screw Hole')
    cv2.setMouseCallback('Select Screw Hole', mouse_callback)
    cv2.imshow('Select Screw Hole', img_display)

    while True:
        key = cv2.waitKey(1) & 0xFF

        if key == ord('q'):
            # 템플릿 저장
            if template_coords:
                for i, (x1, y1, x2, y2) in enumerate(template_coords):
                    template = img[y1:y2, x1:x2]
                    output_path = f"{output_dir}/screw_hole_{i+1}.jpg"
                    cv2.imwrite(output_path, template)
                    print(f"✅ 템플릿 #{i+1} 저장: {output_path} ({template.shape})")

                print(f"\n✅ 총 {len(template_coords)}개 템플릿 저장 완료!")
            else:
                print("❌ 선택된 템플릿이 없습니다.")
            break

        elif key == ord('r'):
            # 초기화
            template_coords.clear()
            img_display = img.copy()
            cv2.imshow('Select Screw Hole', img_display)
            print("⚠️  초기화됨")

    cv2.destroyAllWindows()


def auto_extract_screw_templates(image_path: str, output_dir: str = 'templates'):
    """
    자동으로 나사 구멍 영역 추출 (수동 선택용 대안)

    PCB 네 모서리 근처에서 원형 물체(나사 구멍)를 찾습니다.
    """
    img = cv2.imread(image_path)
    if img is None:
        print(f"❌ 이미지 로드 실패: {image_path}")
        return

    gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
    os.makedirs(output_dir, exist_ok=True)

    # 원 검출 (Hough Circle Transform)
    circles = cv2.HoughCircles(
        gray,
        cv2.HOUGH_GRADIENT,
        dp=1,
        minDist=50,
        param1=50,
        param2=30,
        minRadius=5,
        maxRadius=20
    )

    if circles is not None:
        circles = np.round(circles[0, :]).astype("int")
        print(f"✅ {len(circles)}개 원형 물체 검출됨")

        # 템플릿 크기 (나사 구멍 + 주변 영역)
        template_size = 40

        for i, (x, y, r) in enumerate(circles[:4]):  # 최대 4개만
            # 템플릿 영역 계산
            x1 = max(0, x - template_size // 2)
            y1 = max(0, y - template_size // 2)
            x2 = min(img.shape[1], x + template_size // 2)
            y2 = min(img.shape[0], y + template_size // 2)

            template = img[y1:y2, x1:x2]
            output_path = f"{output_dir}/screw_hole_auto_{i+1}.jpg"
            cv2.imwrite(output_path, template)
            print(f"  템플릿 #{i+1}: ({x}, {y}, r={r}) → {output_path}")

        print(f"\n✅ 자동 추출 완료: {output_dir}/")
    else:
        print("❌ 원형 물체를 찾지 못했습니다.")


if __name__ == '__main__':
    image_path = 'test_pcb_image.jpg'

    if not os.path.exists(image_path):
        print(f"❌ 이미지 없음: {image_path}")
        exit(1)

    print("\n템플릿 추출 방법 선택:")
    print("  1. 대화형 (마우스로 직접 선택) - 권장")
    print("  2. 자동 (원 검출)")
    choice = input("선택 (1/2): ").strip()

    if choice == '1':
        extract_templates_interactive(image_path)
    elif choice == '2':
        auto_extract_screw_templates(image_path)
    else:
        print("잘못된 선택입니다.")
