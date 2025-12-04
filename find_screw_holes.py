"""
PCB 이미지에서 나사 구멍 자동 검출

Hough Circle Transform을 사용하여 원형 나사 구멍을 찾고 시각화합니다.
"""

import cv2
import numpy as np
import os


def find_circles_adaptive(image_path: str, output_dir: str = 'circle_detection'):
    """
    다양한 파라미터로 원 검출 시도
    """
    img = cv2.imread(image_path)
    if img is None:
        print(f"❌ 이미지 로드 실패: {image_path}")
        return

    img_resized = cv2.resize(img, (640, 640))
    gray = cv2.cvtColor(img_resized, cv2.COLOR_BGR2GRAY)

    # 노이즈 제거
    gray_blur = cv2.GaussianBlur(gray, (9, 9), 2)

    os.makedirs(output_dir, exist_ok=True)

    print(f"\n{'='*60}")
    print("원 검출 (다양한 파라미터)")
    print(f"{'='*60}\n")

    # 여러 파라미터 조합 시도
    param_sets = [
        {'name': 'strict', 'dp': 1, 'minDist': 100, 'param1': 50, 'param2': 30, 'minR': 3, 'maxR': 15},
        {'name': 'medium', 'dp': 1, 'minDist': 80, 'param1': 40, 'param2': 25, 'minR': 3, 'maxR': 20},
        {'name': 'relaxed', 'dp': 1, 'minDist': 60, 'param1': 30, 'param2': 20, 'minR': 2, 'maxR': 25},
        {'name': 'very_relaxed', 'dp': 1, 'minDist': 50, 'param1': 20, 'param2': 15, 'minR': 2, 'maxR': 30},
    ]

    best_result = None
    best_count = 0

    for params in param_sets:
        name = params['name']

        circles = cv2.HoughCircles(
            gray_blur,
            cv2.HOUGH_GRADIENT,
            dp=params['dp'],
            minDist=params['minDist'],
            param1=params['param1'],
            param2=params['param2'],
            minRadius=params['minR'],
            maxRadius=params['maxR']
        )

        vis_img = img_resized.copy()

        if circles is not None:
            circles = np.round(circles[0, :]).astype("int")
            count = len(circles)

            print(f"설정: {name}")
            print(f"  파라미터: minDist={params['minDist']}, param1={params['param1']}, param2={params['param2']}")
            print(f"  반지름: {params['minR']}~{params['maxR']}")
            print(f"  ✅ {count}개 원 검출\n")

            # 원 그리기
            for i, (x, y, r) in enumerate(circles):
                cv2.circle(vis_img, (x, y), r, (0, 255, 0), 2)
                cv2.circle(vis_img, (x, y), 2, (0, 0, 255), 3)
                cv2.putText(vis_img, f"#{i+1} ({x},{y})", (x+15, y),
                           cv2.FONT_HERSHEY_SIMPLEX, 0.4, (0, 255, 0), 1)

            # 좌표 출력
            print(f"  검출된 원 좌표:")
            for i, (x, y, r) in enumerate(circles):
                print(f"    #{i+1}: center=({x}, {y}), radius={r}")
            print()

            if count >= 4 and (best_result is None or count < best_count or (count == 4 and best_count > 4)):
                best_result = {
                    'name': name,
                    'circles': circles,
                    'image': vis_img.copy(),
                    'count': count
                }
                best_count = count
        else:
            print(f"설정: {name}")
            print(f"  ❌ 원 검출 실패\n")

        # 결과 저장
        output_path = f"{output_dir}/circles_{name}.jpg"
        cv2.imwrite(output_path, vis_img)
        print(f"  📁 저장: {output_path}\n")

    # 최적 결과
    if best_result:
        print(f"\n{'='*60}")
        print("최적 결과")
        print(f"{'='*60}")
        print(f"설정: {best_result['name']}")
        print(f"검출 개수: {best_result['count']}개")
        print(f"\n검출된 원 좌표:")
        for i, (x, y, r) in enumerate(best_result['circles']):
            print(f"  #{i+1}: center=({x}, {y}), radius={r}")

        cv2.imwrite(f"{output_dir}/BEST_circles.jpg", best_result['image'])
        print(f"\n📁 최적 결과 저장: {output_dir}/BEST_circles.jpg")

        # 4개 코너에 가까운 원 선택 (나사 구멍일 가능성이 높음)
        if best_result['count'] >= 4:
            print(f"\n{'='*60}")
            print("코너 근처 원 선택 (나사 구멍 후보)")
            print(f"{'='*60}")

            corners_img = img_resized.copy()
            circles = best_result['circles']

            # 좌상단, 우상단, 좌하단, 우하단에 가까운 원 찾기
            corner_circles = []

            # 좌상단 (x<320, y<320)
            tl_candidates = [(x, y, r, np.sqrt(x**2 + y**2)) for x, y, r in circles if x < 320 and y < 320]
            if tl_candidates:
                tl = min(tl_candidates, key=lambda c: c[3])
                corner_circles.append(('top_left', tl[0], tl[1], tl[2]))

            # 우상단 (x>320, y<320)
            tr_candidates = [(x, y, r, np.sqrt((640-x)**2 + y**2)) for x, y, r in circles if x > 320 and y < 320]
            if tr_candidates:
                tr = min(tr_candidates, key=lambda c: c[3])
                corner_circles.append(('top_right', tr[0], tr[1], tr[2]))

            # 좌하단 (x<320, y>320)
            bl_candidates = [(x, y, r, np.sqrt(x**2 + (640-y)**2)) for x, y, r in circles if x < 320 and y > 320]
            if bl_candidates:
                bl = min(bl_candidates, key=lambda c: c[3])
                corner_circles.append(('bottom_left', bl[0], bl[1], bl[2]))

            # 우하단 (x>320, y>320)
            br_candidates = [(x, y, r, np.sqrt((640-x)**2 + (640-y)**2)) for x, y, r in circles if x > 320 and y > 320]
            if br_candidates:
                br = min(br_candidates, key=lambda c: c[3])
                corner_circles.append(('bottom_right', br[0], br[1], br[2]))

            # 시각화
            colors = {
                'top_left': (255, 0, 0),      # 파랑
                'top_right': (0, 255, 0),     # 초록
                'bottom_left': (0, 255, 255), # 노랑
                'bottom_right': (0, 0, 255)   # 빨강
            }

            for name, x, y, r in corner_circles:
                cv2.circle(corners_img, (x, y), r, colors[name], 3)
                cv2.circle(corners_img, (x, y), 2, (255, 255, 255), -1)
                cv2.putText(corners_img, name, (x+15, y),
                           cv2.FONT_HERSHEY_SIMPLEX, 0.5, colors[name], 2)
                print(f"  {name:15s}: ({x}, {y}), r={r}")

            cv2.imwrite(f"{output_dir}/CORNER_screws.jpg", corners_img)
            print(f"\n📁 코너 나사 구멍 저장: {output_dir}/CORNER_screws.jpg")

            return corner_circles
    else:
        print("❌ 원을 찾지 못했습니다.")
        return None


def detect_with_edge(image_path: str, output_dir: str = 'circle_detection'):
    """
    엣지 검출 기반 원 찾기
    """
    img = cv2.imread(image_path)
    img_resized = cv2.resize(img, (640, 640))
    gray = cv2.cvtColor(img_resized, cv2.COLOR_BGR2GRAY)

    # Canny 엣지 검출
    edges = cv2.Canny(gray, 50, 150)

    # 엣지 이미지 저장
    os.makedirs(output_dir, exist_ok=True)
    cv2.imwrite(f"{output_dir}/edges.jpg", edges)
    print(f"📁 엣지 이미지 저장: {output_dir}/edges.jpg")

    # 엣지에서 원 검출
    circles = cv2.HoughCircles(
        edges,
        cv2.HOUGH_GRADIENT,
        dp=1,
        minDist=50,
        param1=50,
        param2=20,
        minRadius=2,
        maxRadius=30
    )

    if circles is not None:
        circles = np.round(circles[0, :]).astype("int")
        vis_img = img_resized.copy()

        print(f"\n엣지 기반 검출: {len(circles)}개 원")
        for i, (x, y, r) in enumerate(circles):
            cv2.circle(vis_img, (x, y), r, (0, 255, 0), 2)
            cv2.putText(vis_img, f"{i+1}", (x-5, y+5),
                       cv2.FONT_HERSHEY_SIMPLEX, 0.4, (0, 255, 0), 1)
            print(f"  #{i+1}: ({x}, {y}), r={r}")

        cv2.imwrite(f"{output_dir}/circles_edge_based.jpg", vis_img)
        print(f"📁 저장: {output_dir}/circles_edge_based.jpg")


if __name__ == '__main__':
    image_path = 'test_pcb_image.jpg'

    if not os.path.exists(image_path):
        print(f"❌ 이미지 없음: {image_path}")
        exit(1)

    # 1. 다양한 파라미터로 원 검출
    corner_circles = find_circles_adaptive(image_path)

    # 2. 엣지 기반 검출도 시도
    print(f"\n{'='*60}")
    print("엣지 기반 원 검출")
    print(f"{'='*60}\n")
    detect_with_edge(image_path)

    print(f"\n{'='*60}")
    print("✅ 검출 완료!")
    print("📁 결과 폴더: circle_detection/")
    print(f"{'='*60}\n")
