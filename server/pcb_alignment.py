"""
PCB 정렬 모듈

나사 구멍 또는 PCB 가장자리를 이용하여 PCB를 정렬하고,
전체 PCB가 프레임에 포함되어 있는지 검증합니다.

주요 기능:
1. 나사 구멍 검출 (Hough Circle Transform)
2. PCB 전체 가시성 검증
3. Perspective Transform을 통한 정렬
4. Fallback: PCB Edge 검출
"""

import cv2
import numpy as np
from typing import Optional, Tuple, List, Dict
import logging

# 로깅 설정
logger = logging.getLogger(__name__)


class PCBAligner:
    """PCB 정렬 클래스"""

    def __init__(self, reference_data: Dict):
        """
        Args:
            reference_data (dict): 기준 PCB 데이터
                {
                    'mounting_holes': [(x1,y1), (x2,y2), (x3,y3), (x4,y4)],
                    'hole_distances': {'width': float, 'height': float, 'diagonal': float},
                    'image_size': {'width': int, 'height': int},
                    'components': [...],
                    'side': 'left' or 'right'
                }
        """
        self.reference_data = reference_data
        self.reference_holes = reference_data['mounting_holes']
        self.reference_distances = reference_data['hole_distances']
        self.image_size = (
            reference_data['image_size']['width'],
            reference_data['image_size']['height']
        )

        logger.info(f"PCBAligner 초기화 완료 (side: {reference_data.get('side', 'unknown')})")

    def detect_mounting_holes(
        self,
        image: np.ndarray,
        debug: bool = False
    ) -> Optional[List[Tuple[int, int]]]:
        """
        Hough Circle Transform으로 나사 구멍 검출

        Args:
            image (np.ndarray): 입력 이미지 (BGR)
            debug (bool): 디버그 정보 출력

        Returns:
            list: 4개 코너 구멍 좌표 [(x1,y1), (x2,y2), (x3,y3), (x4,y4)]
                  또는 None (검출 실패)
        """
        # 그레이스케일 변환
        gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
        h, w = gray.shape[:2]

        # 가우시안 블러
        blurred = cv2.GaussianBlur(gray, (9, 9), 2)

        # ROI 기반 구멍 검출 (각 코너 영역에서 개별 검출)
        # 코너 ROI 크기: 30% width, 35% height (축소하여 마운팅 홀만 검출)
        roi_w = int(w * 0.30)  # 192 pixels for 640
        roi_h = int(h * 0.35)  # 168 pixels for 480

        # 4개 코너 영역 정의 (name, x1, y1, x2, y2)
        corner_rois = [
            ('top_left', 0, 0, roi_w, roi_h),
            ('top_right', w - roi_w, 0, w, roi_h),
            ('bottom_right', w - roi_w, h - roi_h, w, h),
            ('bottom_left', 0, h - roi_h, roi_w, h)
        ]

        detected_holes = []

        # 디버그 시각화용 데이터 저장
        if debug:
            debug_img = image.copy()
            all_roi_circles = []  # [(name, x1, y1, x2, y2, circles, selected_circle)]

        for name, x1, y1, x2, y2 in corner_rois:
            # ROI 추출
            roi = blurred[y1:y2, x1:x2]

            # Hough Circle Transform
            circles = cv2.HoughCircles(
                roi,
                cv2.HOUGH_GRADIENT,
                dp=1,
                minDist=30,   # ROI 내에서는 더 가까워도 OK
                param1=50,
                param2=20,    # 더 완화하여 약한 원도 검출 (28→20)
                minRadius=5,  # 원래 값
                maxRadius=20  # 원래 값
            )

            if circles is None:
                if debug:
                    logger.debug(f"{name} ROI에서 구멍 검출 실패")
                return None

            circles = np.uint16(np.around(circles))

            # 해당 코너에 가장 가까운 원 선택
            # 각 ROI의 목표 코너 좌표 (ROI 내부 좌표계)
            roi_h_local = y2 - y1
            roi_w_local = x2 - x1

            if name == 'top_left':
                target_x, target_y = 0, 0
            elif name == 'top_right':
                target_x, target_y = roi_w_local, 0
            elif name == 'bottom_right':
                target_x, target_y = roi_w_local, roi_h_local
            elif name == 'bottom_left':
                target_x, target_y = 0, roi_h_local

            # 모든 검출된 원 중에서 목표 코너에 가장 가까운 원 찾기
            min_dist = float('inf')
            best_circle = None

            for circle in circles[0]:
                cx, cy, r = circle
                # 유클리드 거리 계산
                dist = np.sqrt((cx - target_x)**2 + (cy - target_y)**2)
                if dist < min_dist:
                    min_dist = dist
                    best_circle = circle

            if best_circle is None:
                if debug:
                    logger.debug(f"{name} ROI에서 적절한 원을 찾지 못함")
                return None

            cx, cy, r = best_circle

            # 전체 이미지 좌표로 변환
            global_x = int(x1 + cx)
            global_y = int(y1 + cy)

            detected_holes.append((global_x, global_y))

            if debug:
                logger.debug(f"{name}: ROI({x1},{y1},{x2},{y2}) → 구멍({global_x},{global_y})")
                logger.debug(f"  검출된 원 개수: {len(circles[0])}개")
                logger.debug(f"  목표 코너: ({target_x},{target_y}), 선택된 원까지 거리: {min_dist:.1f}px")

                # 시각화용 데이터 저장
                selected_circle = (cx, cy, r)
                all_roi_circles.append((name, x1, y1, x2, y2, circles, selected_circle))

        if len(detected_holes) != 4:
            if debug:
                logger.debug(f"4개 구멍 검출 실패: {len(detected_holes)}개")
            return (None, None)

        # 구멍을 일관된 순서로 정렬 (좌상, 우상, 우하, 좌하)
        sorted_holes = self._sort_holes(detected_holes)

        if debug:
            logger.debug(f"최종 정렬된 구멍: {sorted_holes}")

            # 디버그 이미지 생성
            import os
            debug_dir = os.path.join(os.path.dirname(os.path.dirname(__file__)), 'temp_frames')
            os.makedirs(debug_dir, exist_ok=True)

            for name, x1, y1, x2, y2, circles, selected_circle in all_roi_circles:
                # 1. ROI 경계선 그리기 (녹색)
                cv2.rectangle(debug_img, (x1, y1), (x2, y2), (0, 255, 0), 2)
                cv2.putText(debug_img, name, (x1 + 5, y1 + 20),
                           cv2.FONT_HERSHEY_SIMPLEX, 0.5, (0, 255, 0), 2)

                # 2. 모든 검출된 원 그리기 (파란색)
                for i, (cx, cy, r) in enumerate(circles[0]):
                    global_x = int(x1 + cx)
                    global_y = int(y1 + cy)
                    cv2.circle(debug_img, (global_x, global_y), int(r), (255, 0, 0), 2)
                    cv2.putText(debug_img, f"#{i}", (global_x - 10, global_y - 10),
                               cv2.FONT_HERSHEY_SIMPLEX, 0.4, (255, 0, 0), 1)

                # 3. 선택된 원 강조 (빨간색, 두껍게)
                scx, scy, sr = selected_circle
                selected_global_x = int(x1 + scx)
                selected_global_y = int(y1 + scy)
                cv2.circle(debug_img, (selected_global_x, selected_global_y), int(sr), (0, 0, 255), 3)
                cv2.putText(debug_img, "SELECTED", (selected_global_x + int(sr) + 5, selected_global_y),
                           cv2.FONT_HERSHEY_SIMPLEX, 0.5, (0, 0, 255), 2)

            # 디버그 이미지 반환 (파일 저장 안 함)
            return sorted_holes, debug_img
        else:
            return sorted_holes, None

    def detect_mounting_holes_with_contour(
        self,
        image: np.ndarray,
        debug: bool = False
    ) -> Optional[List[Tuple[int, int]]]:
        """
        PCB 윤곽선 기반 마운팅 홀 검출 (단순화 버전)

        1. PCB 윤곽선 검출 (Canny + findContours)
        2. 윤곽선의 4개 코너 추출
        3. 코너를 약간 안쪽으로 offset하여 마운팅 홀 위치로 사용

        Args:
            image (np.ndarray): 입력 이미지 (BGR)
            debug (bool): 디버그 정보 출력

        Returns:
            tuple: (마운팅 홀 좌표 리스트, 디버그 이미지)
                   또는 (None, None) (검출 실패)
        """
        # 1. PCB 윤곽선 검출
        pcb_corners = self.detect_pcb_edges(image, debug=debug)

        if pcb_corners is None:
            if debug:
                logger.debug("PCB 윤곽선 검출 실패 - ROI 기반 방법으로 폴백")
            return (None, None)

        # 2. PCB 코너를 약간 안쪽으로 offset (마운팅 홀은 코너 근처에 위치)
        # 이미 정렬된 순서: top-left, top-right, bottom-right, bottom-left
        offset = 15  # 픽셀 (코너에서 안쪽으로)

        mounting_holes = []
        h, w = image.shape[:2]

        for idx, corner in enumerate(pcb_corners):
            corner_x, corner_y = corner

            # 각 코너 방향에 따라 offset 방향 결정
            if idx == 0:  # top-left → 오른쪽 아래로
                hole_x = min(w - 1, corner_x + offset)
                hole_y = min(h - 1, corner_y + offset)
            elif idx == 1:  # top-right → 왼쪽 아래로
                hole_x = max(0, corner_x - offset)
                hole_y = min(h - 1, corner_y + offset)
            elif idx == 2:  # bottom-right → 왼쪽 위로
                hole_x = max(0, corner_x - offset)
                hole_y = max(0, corner_y - offset)
            else:  # bottom-left → 오른쪽 위로
                hole_x = min(w - 1, corner_x + offset)
                hole_y = max(0, corner_y - offset)

            mounting_holes.append((int(hole_x), int(hole_y)))

        # 3. 디버그 시각화
        if debug:
            debug_img = image.copy()

            # PCB 윤곽선 그리기 (하늘색)
            for i in range(4):
                pt1 = tuple(pcb_corners[i])
                pt2 = tuple(pcb_corners[(i + 1) % 4])
                cv2.line(debug_img, pt1, pt2, (255, 255, 0), 3)

            # 마운팅 홀 위치 그리기 (빨간 원)
            for idx, hole in enumerate(mounting_holes):
                cv2.circle(debug_img, hole, 10, (0, 0, 255), 3)
                cv2.putText(debug_img, f"H{idx+1}", (hole[0] + 15, hole[1]),
                           cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 0, 255), 2)

            logger.debug(f"PCB 코너: {pcb_corners}")
            logger.debug(f"마운팅 홀 (윤곽선 기반): {mounting_holes}")

            # 디버그 이미지 반환 (파일 저장 안 함)
            return mounting_holes, debug_img
        else:
            return mounting_holes, None

    def _select_corner_holes(
        self,
        holes: List[Tuple[int, int]]
    ) -> List[Tuple[int, int]]:
        """
        4개 코너 구멍 선택

        Args:
            holes (list): 검출된 모든 구멍 좌표

        Returns:
            list: [top_left, top_right, bottom_right, bottom_left]
        """
        holes = np.array(holes)

        # 4개 코너 찾기
        top_left = holes[np.argmin(holes[:, 0] + holes[:, 1])]
        top_right = holes[np.argmax(holes[:, 0] - holes[:, 1])]
        bottom_right = holes[np.argmax(holes[:, 0] + holes[:, 1])]
        bottom_left = holes[np.argmax(holes[:, 1] - holes[:, 0])]

        return [
            tuple(top_left),
            tuple(top_right),
            tuple(bottom_right),
            tuple(bottom_left)
        ]

    def is_full_board_visible(
        self,
        detected_holes: Optional[List[Tuple[int, int]]],
        image: Optional[np.ndarray] = None
    ) -> Tuple[bool, Dict[str, bool]]:
        """
        PCB 전체가 프레임에 포함되어 있는지 검증

        3가지 조건 중 2개 이상 만족 시 전체 보임:
        1. 나사 구멍 4개 모두 검출
        2. 구멍 간 거리 비율이 기준과 일치 (±10%)
        3. PCB contour 면적이 예상 면적의 95% 이상 (옵션)

        Args:
            detected_holes (list): 검출된 구멍 좌표 (4개)
            image (np.ndarray): 입력 이미지 (면적 검증용, 옵션)

        Returns:
            tuple: (전체 보임 여부, 조건별 결과)
                조건별 결과: {'holes': bool, 'distances': bool, 'area': bool}
        """
        conditions = {
            'holes': False,
            'distances': False,
            'area': False
        }

        # 조건 1: 나사 구멍 4개 모두 검출
        if detected_holes is not None and len(detected_holes) == 4:
            conditions['holes'] = True
        else:
            return False, conditions

        # 조건 2: 구멍 간 거리 비율 검증
        detected_distances = self._calculate_hole_distances(detected_holes)

        width_ratio = detected_distances['width'] / self.reference_distances['width']
        height_ratio = detected_distances['height'] / self.reference_distances['height']
        diagonal_ratio = detected_distances['diagonal'] / self.reference_distances['diagonal']

        # 디버그: 검출된 구멍 및 거리 출력
        logger.debug(f"[PCB Alignment Debug]")
        logger.debug(f"  검출된 구멍: {detected_holes}")
        logger.debug(f"  레퍼런스 거리: width={self.reference_distances['width']:.2f}, height={self.reference_distances['height']:.2f}, diagonal={self.reference_distances['diagonal']:.2f}")
        logger.debug(f"  검출된 거리: width={detected_distances['width']:.2f}, height={detected_distances['height']:.2f}, diagonal={detected_distances['diagonal']:.2f}")
        logger.debug(f"  거리 비율: width={width_ratio:.3f}, height={height_ratio:.3f}, diagonal={diagonal_ratio:.3f}")
        logger.debug(f"  비율 범위: 0.8 ~ 1.2")

        # ±20% 범위 내 (임시로 완화)
        if (0.8 <= width_ratio <= 1.2 and
            0.8 <= height_ratio <= 1.2 and
            0.8 <= diagonal_ratio <= 1.2):
            conditions['distances'] = True

        # 조건 3: PCB contour 면적 (옵션)
        if image is not None:
            pcb_area = self._estimate_pcb_area(image)
            expected_area = self.image_size[0] * self.image_size[1]

            area_ratio = pcb_area / expected_area
            logger.debug(f"  PCB 면적: {pcb_area:.0f} / {expected_area:.0f} = {area_ratio:.3f} (최소 0.95 필요)")

            if pcb_area >= expected_area * 0.95:
                conditions['area'] = True

        # 2개 이상 만족 → 전체 보임
        satisfied_count = sum(conditions.values())
        is_visible = satisfied_count >= 2

        return is_visible, conditions

    def _sort_holes(
        self,
        holes: List[Tuple[int, int]]
    ) -> List[Tuple[int, int]]:
        """
        검출된 구멍을 일관된 순서로 정렬

        PCB가 이동하거나 회전해도 항상 같은 순서를 보장:
        1. 왼쪽 상단 (top-left)
        2. 오른쪽 상단 (top-right)
        3. 오른쪽 하단 (bottom-right)
        4. 왼쪽 하단 (bottom-left)

        Args:
            holes: 4개 구멍의 (x, y) 좌표 리스트

        Returns:
            정렬된 구멍 좌표 리스트
        """
        if len(holes) != 4:
            logger.warning(f"구멍 정렬 실패: 4개가 아닌 {len(holes)}개 구멍")
            return holes

        # y좌표 중앙값 계산
        y_coords = [h[1] for h in holes]
        y_median = np.median(y_coords)

        # 상단(top)과 하단(bottom) 분리
        top_holes = [h for h in holes if h[1] < y_median]
        bottom_holes = [h for h in holes if h[1] >= y_median]

        # 예외 처리: 상/하단에 각각 2개씩 있어야 함
        if len(top_holes) != 2 or len(bottom_holes) != 2:
            logger.warning(f"구멍 배치 비정상: 상단 {len(top_holes)}개, 하단 {len(bottom_holes)}개")
            # fallback: y좌표로만 정렬
            sorted_holes = sorted(holes, key=lambda h: (h[1], h[0]))
            return sorted_holes

        # 각 그룹 내에서 x좌표로 정렬 (좌 → 우)
        top_holes.sort(key=lambda h: h[0])
        bottom_holes.sort(key=lambda h: h[0])

        # 최종 순서: 좌상, 우상, 우하, 좌하
        sorted_holes = [
            top_holes[0],      # 왼쪽 상단
            top_holes[1],      # 오른쪽 상단
            bottom_holes[1],   # 오른쪽 하단
            bottom_holes[0]    # 왼쪽 하단
        ]

        logger.debug(f"구멍 정렬 완료: {holes} → {sorted_holes}")

        return sorted_holes

    def _calculate_hole_distances(
        self,
        holes: List[Tuple[int, int]]
    ) -> Dict[str, float]:
        """
        4개 구멍 간 거리 계산

        Args:
            holes (list): [top_left, top_right, bottom_right, bottom_left]

        Returns:
            dict: {'width': float, 'height': float, 'diagonal': float}
        """
        p1, p2, p3, p4 = holes

        width = np.sqrt((p2[0] - p1[0])**2 + (p2[1] - p1[1])**2)
        height = np.sqrt((p4[0] - p1[0])**2 + (p4[1] - p1[1])**2)
        diagonal = np.sqrt((p3[0] - p1[0])**2 + (p3[1] - p1[1])**2)

        return {
            'width': float(width),
            'height': float(height),
            'diagonal': float(diagonal)
        }

    def _estimate_pcb_area(self, image: np.ndarray) -> float:
        """
        PCB contour 면적 추정

        Args:
            image (np.ndarray): 입력 이미지

        Returns:
            float: PCB 면적 (픽셀 단위)
        """
        # 그레이스케일 변환
        gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)

        # 이진화
        _, binary = cv2.threshold(gray, 127, 255, cv2.THRESH_BINARY)

        # Contour 검출
        contours, _ = cv2.findContours(
            binary,
            cv2.RETR_EXTERNAL,
            cv2.CHAIN_APPROX_SIMPLE
        )

        if not contours:
            return 0.0

        # 가장 큰 contour = PCB
        largest_contour = max(contours, key=cv2.contourArea)
        area = cv2.contourArea(largest_contour)

        return float(area)

    def align_pcb(
        self,
        image: np.ndarray,
        detected_holes: List[Tuple[int, int]]
    ) -> Tuple[np.ndarray, np.ndarray]:
        """
        Perspective Transform으로 PCB 정렬

        Args:
            image (np.ndarray): 입력 이미지
            detected_holes (list): 검출된 구멍 좌표 (4개)

        Returns:
            tuple: (정렬된 이미지, 변환 행렬 M)
        """
        # Source points (검출된 구멍)
        src_pts = np.float32(detected_holes)

        # Destination points (기준 구멍)
        dst_pts = np.float32(self.reference_holes)

        # Perspective Transform 행렬 계산
        M = cv2.getPerspectiveTransform(src_pts, dst_pts)

        # 이미지 변환
        aligned = cv2.warpPerspective(image, M, self.image_size)

        logger.debug(f"PCB 정렬 완료 (변환 행렬 계산됨)")

        return aligned, M

    def detect_pcb_contour_realtime(
        self,
        image: np.ndarray,
        debug: bool = False,
        debug_path: str = "/tmp/pcb_debug"
    ) -> Optional[Tuple[np.ndarray, bool]]:
        """
        ROI 기반 색상 + 엣지 실시간 PCB 컨투어 검출

        ROI 내부에서만 검출하여 PCB가 완전히 화면에 들어왔을 때만 검출

        1. ROI 정의 (화면 중앙 60% x 70%)
        2. HSV 색상 마스킹 (초록색 PCB 영역 추출)
        3. 마스킹된 영역에만 엣지 검출 적용
        4. 가우시안 블러 (노이즈 제거)
        5. Canny 엣지 검출
        6. 모폴로지 연산 (엣지 연결)
        7. 컨투어 검출 → 가장 큰 사각형
        8. 4개 코너 추출 및 강화된 검증
        9. ROI 내부 검증

        Args:
            image (np.ndarray): 입력 이미지 (BGR)
            debug (bool): 디버그 정보 출력
            debug_path (str): 디버그 이미지 저장 경로

        Returns:
            Tuple[np.ndarray, bool]: (4개 코너 좌표, 검증 통과 여부)
                                     또는 (None, False) (검출 실패)
        """
        import os

        # ROI 정의
        roi_rect = self._define_roi(image.shape)
        roi_x, roi_y, roi_w, roi_h = roi_rect

        # 디버그: 원본 이미지 저장
        if debug:
            os.makedirs(debug_path, exist_ok=True)
            debug_roi_img = image.copy()
            # ROI 박스 그리기 (녹색)
            cv2.rectangle(debug_roi_img, (roi_x, roi_y), (roi_x + roi_w, roi_y + roi_h), (0, 255, 0), 2)
            cv2.putText(debug_roi_img, "ROI", (roi_x + 5, roi_y + 20),
                       cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 255, 0), 2)
            cv2.imwrite(os.path.join(debug_path, "00_original.jpg"), debug_roi_img)
            logger.debug(f"💾 원본 이미지 저장 (ROI 표시): {debug_path}/00_original.jpg (shape: {image.shape})")
            logger.debug(f"🔲 ROI: x={roi_x}, y={roi_y}, w={roi_w}, h={roi_h}")

        # 1. HSV 색상 마스킹 (초록색 PCB 영역 추출)
        hsv = cv2.cvtColor(image, cv2.COLOR_BGR2HSV)

        # 초록색 범위 (밝은 청록색 ~ 진한 초록색) - 널널하게 설정
        lower_green = np.array([35, 15, 15])   # H: 35, S: 15 (↓), V: 15 (↓) - 더 낮은 채도/명도 포함
        upper_green = np.array([85, 255, 255]) # H: 85, S: 255, V: 255

        # HSV 마스크 생성
        hsv_mask = cv2.inRange(hsv, lower_green, upper_green)

        # 모폴로지 연산으로 노이즈 제거 및 연결 강화
        kernel_small = np.ones((3, 3), np.uint8)
        kernel_medium = np.ones((5, 5), np.uint8)  # 더 큰 커널 추가

        hsv_mask = cv2.morphologyEx(hsv_mask, cv2.MORPH_OPEN, kernel_small)   # 작은 노이즈 제거
        hsv_mask = cv2.morphologyEx(hsv_mask, cv2.MORPH_CLOSE, kernel_medium) # 큰 구멍 메우기 (5x5)
        hsv_mask = cv2.morphologyEx(hsv_mask, cv2.MORPH_CLOSE, kernel_medium) # 2회 반복으로 더 강하게

        # ROI 마스크 생성 (ROI 밖은 검은색으로)
        roi_mask = np.zeros(hsv_mask.shape, dtype=np.uint8)
        roi_mask[roi_y:roi_y+roi_h, roi_x:roi_x+roi_w] = 255

        # HSV 마스크에 ROI 마스크 적용 (ROI 안에서만 검출)
        hsv_mask = cv2.bitwise_and(hsv_mask, hsv_mask, mask=roi_mask)

        if debug:
            cv2.imwrite(os.path.join(debug_path, "01_hsv_mask.jpg"), hsv_mask)
            logger.debug(f"💾 HSV 마스크 저장 (ROI 적용): {debug_path}/01_hsv_mask.jpg")

        # 2. 그레이스케일 변환
        gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)

        # 3. 마스크를 그레이스케일에 적용 (PCB 영역만 남기고 나머지는 검은색)
        masked_gray = cv2.bitwise_and(gray, gray, mask=hsv_mask)

        if debug:
            cv2.imwrite(os.path.join(debug_path, "02_masked_gray.jpg"), masked_gray)
            logger.debug(f"💾 마스킹된 그레이스케일 저장: {debug_path}/02_masked_gray.jpg")

        # 4. 가우시안 블러 (노이즈 제거)
        blurred = cv2.GaussianBlur(masked_gray, (5, 5), 0)

        if debug:
            cv2.imwrite(os.path.join(debug_path, "03_blurred.jpg"), blurred)

        # 5. Canny 엣지 검출 (임계값 조정)
        edges = cv2.Canny(blurred, 30, 100)

        if debug:
            cv2.imwrite(os.path.join(debug_path, "04_canny_edges.jpg"), edges)
            logger.debug(f"💾 Canny 엣지 저장: {debug_path}/04_canny_edges.jpg")

        # 6. 모폴로지 연산 (엣지 연결 - 끊어진 부분 연결)
        kernel = np.ones((5, 5), np.uint8)
        edges_closed = cv2.morphologyEx(edges, cv2.MORPH_CLOSE, kernel)
        edges_dilated = cv2.dilate(edges_closed, kernel, iterations=1)  # 1회로 감소 (경계 왜곡 방지)

        if debug:
            cv2.imwrite(os.path.join(debug_path, "05_morphology.jpg"), edges_dilated)
            logger.debug(f"💾 모폴로지 연산 후 저장: {debug_path}/05_morphology.jpg")

        # 5. 컨투어 검출
        contours, _ = cv2.findContours(
            edges_dilated,
            cv2.RETR_EXTERNAL,
            cv2.CHAIN_APPROX_SIMPLE
        )

        logger.info(f"🔍 [엣지 검출] Contour 개수: {len(contours) if contours else 0}")

        if not contours:
            logger.warning("⚠️  컨투어를 찾을 수 없음")

            # 디버그 이미지 생성 (실패 상태 표시)
            if debug:
                debug_img = image.copy()

                # ROI 박스 그리기 (녹색)
                cv2.rectangle(debug_img, (roi_x, roi_y), (roi_x + roi_w, roi_y + roi_h), (0, 255, 0), 2)
                cv2.putText(debug_img, "ROI", (roi_x + 5, roi_y + 20),
                           cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 255, 0), 2)

                # NOT READY 메시지
                cv2.putText(debug_img, "NOT READY - No contours", (10, 30),
                           cv2.FONT_HERSHEY_SIMPLEX, 1.0, (0, 0, 255), 2)

                cv2.imwrite(os.path.join(debug_path, "06_detected_contour.jpg"), debug_img)
                logger.debug(f"💾 검출 실패 디버그 저장: {debug_path}/06_detected_contour.jpg")

            return (None, False)

        # 6. 가장 큰 컨투어 찾기 (PCB)
        largest_contour = max(contours, key=cv2.contourArea)

        # 면적 검증 (ROI 영역의 15% 이상)
        roi_area = roi_w * roi_h
        contour_area = cv2.contourArea(largest_contour)
        area_percentage_roi = (contour_area / roi_area) * 100
        area_percentage_img = (contour_area / (image.shape[0] * image.shape[1])) * 100

        logger.info(f"🔍 [엣지 검출] 최대 Contour 면적: {contour_area:.0f}px² (ROI의 {area_percentage_roi:.1f}%, 전체의 {area_percentage_img:.1f}%)")

        if contour_area < roi_area * 0.15:
            logger.warning(f"⚠️  PCB 면적이 너무 작음: {contour_area:.0f} < {roi_area * 0.15:.0f} (ROI의 15% 미만)")

            # 디버그 이미지 생성 (실패 상태 표시)
            if debug:
                debug_img = image.copy()

                # ROI 박스 그리기 (녹색)
                cv2.rectangle(debug_img, (roi_x, roi_y), (roi_x + roi_w, roi_y + roi_h), (0, 255, 0), 2)
                cv2.putText(debug_img, "ROI", (roi_x + 5, roi_y + 20),
                           cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 255, 0), 2)

                # 컨투어의 4개 코너 찾기 시도 (회색 박스로 표시)
                hull = cv2.convexHull(largest_contour)
                perimeter = cv2.arcLength(hull, True)

                # epsilon을 동적으로 조정하여 4개 코너 찾기
                corners_found = False
                for eps_mult in [0.02, 0.03, 0.04, 0.05, 0.06, 0.08, 0.10]:
                    epsilon = eps_mult * perimeter
                    approx = cv2.approxPolyDP(hull, epsilon, True)
                    if len(approx) == 4:
                        corners = approx.reshape(-1, 2)
                        ordered_corners = self._order_corners(corners)
                        # 회색 사각형으로 표시 (면적 부족 - 실패)
                        cv2.polylines(debug_img, [ordered_corners], True, (128, 128, 128), 2)
                        # 코너 포인트 표시
                        for i, corner in enumerate(ordered_corners):
                            cv2.circle(debug_img, tuple(corner), 5, (128, 128, 128), -1)
                        corners_found = True
                        break

                # 4개 코너를 못 찾은 경우 전체 컨투어 표시
                if not corners_found:
                    cv2.drawContours(debug_img, [largest_contour], -1, (128, 128, 128), 2)

                # NOT READY 메시지
                cv2.putText(debug_img, "NOT READY - Area too small", (10, 30),
                           cv2.FONT_HERSHEY_SIMPLEX, 1.0, (0, 0, 255), 2)
                cv2.putText(debug_img, f"ROI Area: {area_percentage_roi:.1f}% < 15%", (10, 70),
                           cv2.FONT_HERSHEY_SIMPLEX, 0.7, (0, 0, 255), 2)

                cv2.imwrite(os.path.join(debug_path, "06_detected_contour.jpg"), debug_img)
                logger.debug(f"💾 검출 실패 디버그 저장: {debug_path}/06_detected_contour.jpg")

            return (None, False)

        # 7. 컨벡스 헐 (볼록 껍질)
        hull = cv2.convexHull(largest_contour)

        # 8. epsilon을 동적으로 조정하여 정확히 4개 코너 찾기
        perimeter = cv2.arcLength(hull, True)
        logger.info(f"🔍 [엣지 검출] Convex Hull 둘레: {perimeter:.1f}px")

        # epsilon을 점진적으로 증가시키며 4개 코너 찾기
        for eps_multiplier in [0.02, 0.03, 0.04, 0.05, 0.06, 0.08, 0.10, 0.12, 0.15]:
            epsilon = eps_multiplier * perimeter
            approx = cv2.approxPolyDP(hull, epsilon, True)

            if len(approx) == 4:
                # 정확히 4개 코너 발견!
                corners = approx.reshape(-1, 2)

                # 9. 사각형 검증 (ROI 포함 강화된 검증)
                is_valid = self._validate_rectangle(corners, roi_rect)

                # 시계방향 정렬: top-left, top-right, bottom-right, bottom-left
                ordered_corners = self._order_corners(corners)

                # 디버그: 검출된 컨투어 표시
                if debug:
                    debug_img = image.copy()

                    # ROI 박스 그리기 (녹색)
                    cv2.rectangle(debug_img, (roi_x, roi_y), (roi_x + roi_w, roi_y + roi_h), (0, 255, 0), 2)
                    cv2.putText(debug_img, "ROI", (roi_x + 5, roi_y + 20),
                               cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 255, 0), 2)

                    # 컨투어 색상: 검증 통과 = Cyan, 실패 = 빨간색
                    contour_color = (255, 255, 0) if is_valid else (0, 0, 255)
                    cv2.polylines(debug_img, [ordered_corners], True, contour_color, 2)

                    # 코너 포인트 그리기 + H1, H2, H3, H4 라벨
                    labels = ['H1', 'H2', 'H3', 'H4']
                    for i, corner in enumerate(ordered_corners):
                        corner_color = (0, 0, 255) if is_valid else (128, 128, 128)
                        cv2.circle(debug_img, tuple(corner), 8, corner_color, -1)
                        cv2.putText(debug_img, labels[i], (corner[0]+10, corner[1]+10),
                                   cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 255, 255), 2)

                    # 검증 상태 메시지 표시
                    status_text = "PCB READY" if is_valid else "NOT READY"
                    status_color = (0, 255, 0) if is_valid else (0, 0, 255)
                    cv2.putText(debug_img, status_text, (10, 30),
                               cv2.FONT_HERSHEY_SIMPLEX, 1.0, status_color, 2)

                    cv2.imwrite(os.path.join(debug_path, "06_detected_contour.jpg"), debug_img)
                    logger.debug(f"💾 검출 결과 저장: {debug_path}/06_detected_contour.jpg")

                if is_valid:
                    logger.info(f"✅ [PCB 검출] 검증 통과 (eps={eps_multiplier:.2f}) - PCB READY")
                    logger.info(f"   코너: {list(ordered_corners)}")
                    return (ordered_corners, True)
                else:
                    logger.warning(f"⚠️  [PCB 검출] 검증 실패 (eps={eps_multiplier:.2f}) - NOT READY")
                    return (ordered_corners, False)

        logger.warning(f"⚠️  4개 코너를 찾을 수 없음 (검출된 코너 수: {len(approx)})")

        # 디버그 이미지 생성 (실패 상태 표시)
        if debug:
            debug_img = image.copy()

            # ROI 박스 그리기 (녹색)
            cv2.rectangle(debug_img, (roi_x, roi_y), (roi_x + roi_w, roi_y + roi_h), (0, 255, 0), 2)
            cv2.putText(debug_img, "ROI", (roi_x + 5, roi_y + 20),
                       cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 255, 0), 2)

            # NOT READY 메시지
            cv2.putText(debug_img, "NOT READY - 4 corners not found", (10, 30),
                       cv2.FONT_HERSHEY_SIMPLEX, 1.0, (0, 0, 255), 2)
            cv2.putText(debug_img, f"Detected corners: {len(approx)}", (10, 70),
                       cv2.FONT_HERSHEY_SIMPLEX, 0.7, (0, 0, 255), 2)

            # 검출된 컨투어 표시 (회색으로 - 실패)
            if len(approx) > 0:
                cv2.polylines(debug_img, [approx], True, (128, 128, 128), 2)

            cv2.imwrite(os.path.join(debug_path, "06_detected_contour.jpg"), debug_img)
            logger.debug(f"💾 검출 실패 디버그 저장: {debug_path}/06_detected_contour.jpg")

        return (None, False)

    def detect_pcb_edges(
        self,
        image: np.ndarray,
        debug: bool = False
    ) -> Optional[np.ndarray]:
        """
        초록색 PCB 가장자리 검출 (HSV 색상 기반)

        1. HSV 색상 마스킹으로 초록색 PCB만 추출
        2. findContours → 가장 큰 윤곽선
        3. convexHull로 볼록 껍질 변환
        4. epsilon을 동적 조정하여 정확히 4개 코너 추출
        5. 사각형 검증

        Args:
            image (np.ndarray): 입력 이미지 (BGR)
            debug (bool): 디버그 정보 출력

        Returns:
            np.ndarray: 4개 코너 좌표 [(x1,y1), (x2,y2), (x3,y3), (x4,y4)]
                       또는 None (검출 실패)
        """
        # 디버그: 원본 이미지 저장
        if debug:
            import os
            os.makedirs("/tmp/pcb_debug", exist_ok=True)
            cv2.imwrite("/tmp/pcb_debug/00_original.jpg", image)
            logger.debug(f"💾 원본 이미지 저장: /tmp/pcb_debug/00_original.jpg (shape: {image.shape})")

        # 1. HSV 색상 공간으로 변환
        hsv = cv2.cvtColor(image, cv2.COLOR_BGR2HSV)

        # 2. 초록색 범위 정의 (PCB 색상) - 밝은 청록색 PCB를 위한 넓은 범위
        # H: 25-110 (노란빛 초록~청록~파란빛 초록), S: 5-255 (매우 낮은 채도 포함), V: 30-255 (밝은 색상)
        # 밝은 청록색 PCB는 채도가 매우 낮고 명도가 높음
        lower_green = np.array([25, 5, 30])
        upper_green = np.array([110, 255, 255])

        # 3. 초록색 마스크 생성
        green_mask = cv2.inRange(hsv, lower_green, upper_green)

        # 디버그: HSV 마스크 저장
        if debug:
            import os
            os.makedirs("/tmp/pcb_debug", exist_ok=True)
            cv2.imwrite("/tmp/pcb_debug/01_hsv_mask.jpg", green_mask)
            logger.debug("💾 HSV 마스크 저장: /tmp/pcb_debug/01_hsv_mask.jpg")

        # 4. 형태학적 연산으로 노이즈 제거
        kernel = np.ones((5, 5), np.uint8)
        green_mask = cv2.morphologyEx(green_mask, cv2.MORPH_CLOSE, kernel)  # 구멍 메우기
        green_mask = cv2.morphologyEx(green_mask, cv2.MORPH_OPEN, kernel)   # 작은 노이즈 제거

        # 디버그: 형태학 연산 후 마스크 저장
        if debug:
            cv2.imwrite("/tmp/pcb_debug/02_morphology_mask.jpg", green_mask)
            logger.debug("💾 형태학 연산 후 마스크 저장: /tmp/pcb_debug/02_morphology_mask.jpg")

        # 5. Canny Edge Detection (마스크에서)
        edges = cv2.Canny(green_mask, 50, 150)

        # 디버그: Canny edges 저장
        if debug:
            cv2.imwrite("/tmp/pcb_debug/03_canny_edges.jpg", edges)
            logger.debug("💾 Canny edges 저장: /tmp/pcb_debug/03_canny_edges.jpg")

        # 6. Contour 검출
        contours, _ = cv2.findContours(
            edges,
            cv2.RETR_EXTERNAL,
            cv2.CHAIN_APPROX_SIMPLE
        )

        # 디버그: Contour 검출 결과 로깅
        logger.info(f"🔍 [HSV 검출] Contour 개수: {len(contours) if contours else 0}")

        if not contours:
            if debug:
                logger.warning("⚠️  초록색 PCB 윤곽선 검출 실패 - Contour가 없음")
            return None

        # 7. 가장 큰 contour = PCB (면적 기준)
        largest_contour = max(contours, key=cv2.contourArea)

        # 면적이 너무 작으면 무시 (이미지의 5% 이상)
        image_area = image.shape[0] * image.shape[1]
        contour_area = cv2.contourArea(largest_contour)

        # 디버그: 면적 비율 로깅
        area_percentage = (contour_area / image_area) * 100
        logger.info(f"🔍 [HSV 검출] 최대 Contour 면적: {contour_area:.0f}px² ({area_percentage:.1f}% of image)")

        if contour_area < image_area * 0.05:
            logger.warning(f"⚠️  초록색 PCB 면적이 너무 작음: {contour_area:.0f} < {image_area * 0.05:.0f} (5% 미만)")
            return None

        # 8. convexHull로 볼록 껍질 변환 (오목한 부분 제거)
        hull = cv2.convexHull(largest_contour)

        # 9. epsilon을 동적으로 조정하여 정확히 4개 코너 찾기
        perimeter = cv2.arcLength(hull, True)
        logger.info(f"🔍 [HSV 검출] Convex Hull 둘레: {perimeter:.1f}px")

        # epsilon을 0.02부터 0.15까지 증가시키며 4개 코너 찾기
        for eps_multiplier in [0.02, 0.03, 0.04, 0.05, 0.06, 0.08, 0.10, 0.12, 0.15]:
            epsilon = eps_multiplier * perimeter
            approx = cv2.approxPolyDP(hull, epsilon, True)

            if len(approx) == 4:
                # 정확히 4개 코너 발견!
                corners = approx.reshape(-1, 2)

                # 10. 사각형 검증 (4개 점이 너무 가까우면 제외)
                if self._validate_rectangle(corners):
                    # 시계방향 정렬: top-left, top-right, bottom-right, bottom-left
                    ordered_corners = self._order_corners(corners)

                    if debug:
                        logger.debug(f"초록색 PCB 가장자리 검출 성공: epsilon={eps_multiplier:.2f}, 4개 코너")
                        logger.debug(f"코너: {ordered_corners}")

                    return ordered_corners
                else:
                    if debug:
                        logger.debug(f"epsilon={eps_multiplier:.2f}에서 4개 코너 검출했으나 사각형 검증 실패")

        # 모든 epsilon에서 실패
        logger.warning(f"⚠️  초록색 PCB를 사각형으로 근사할 수 없음 (최종 점 개수: {len(approx)})")

        return None

    def _define_roi(self, image_shape: Tuple[int, int]) -> Tuple[int, int, int, int]:
        """
        화면 중앙에 ROI(Region of Interest) 정의

        PCB가 완전히 화면에 들어왔을 때만 검출하기 위한 영역

        Args:
            image_shape: (height, width) 이미지 크기

        Returns:
            (roi_x, roi_y, roi_width, roi_height): ROI 좌표 및 크기
        """
        height, width = image_shape[:2]

        # ROI 크기: 화면 중앙 60% x 70%
        roi_width = int(width * 0.6)
        roi_height = int(height * 0.7)

        # ROI 시작점 (중앙 기준)
        roi_x = int((width - roi_width) / 2)
        roi_y = int((height - roi_height) / 2)

        return (roi_x, roi_y, roi_width, roi_height)

    def _check_corners_in_roi(
        self,
        corners: np.ndarray,
        roi_rect: Tuple[int, int, int, int]
    ) -> bool:
        """
        4개 코너가 모두 ROI 내부에 있는지 확인

        Args:
            corners: 4개 코너 좌표
            roi_rect: (x, y, width, height) ROI 영역

        Returns:
            bool: 모든 코너가 ROI 내부에 있으면 True
        """
        roi_x, roi_y, roi_w, roi_h = roi_rect

        for corner in corners:
            x, y = corner
            if not (roi_x <= x <= roi_x + roi_w and roi_y <= y <= roi_y + roi_h):
                return False

        return True

    def _validate_rectangle(
        self,
        corners: np.ndarray,
        roi_rect: Optional[Tuple[int, int, int, int]] = None
    ) -> bool:
        """
        4개 코너가 유효한 사각형을 이루는지 강화된 검증

        검증 항목:
        1. 최소 거리 검증 (너무 가까운 점 제외)
        2. 대각선 길이 비율 검증
        3. 대변 길이 비율 검증 (직사각형 검증)
        4. 종횡비 검증 (PCB 실제 비율)
        5. ROI 내부 검증 (선택사항)

        Args:
            corners (np.ndarray): 4개 코너 좌표 [[x1,y1], [x2,y2], [x3,y3], [x4,y4]]
            roi_rect (Optional): ROI 영역 (x, y, width, height)

        Returns:
            bool: 유효한 사각형이면 True
        """
        if len(corners) != 4:
            return False

        # 1. 모든 점 간의 거리 계산
        min_dist = float('inf')
        for i in range(4):
            for j in range(i + 1, 4):
                dist = np.sqrt((corners[i][0] - corners[j][0])**2 +
                              (corners[i][1] - corners[j][1])**2)
                min_dist = min(min_dist, dist)

        # 최소 거리가 30픽셀 이상이어야 함 (너무 가까운 점 제외)
        if min_dist < 30:
            logger.debug(f"❌ 검증 실패: 최소 거리 너무 작음 ({min_dist:.1f} < 30)")
            return False

        # 2. 대각선 길이 비율 확인 (정사각형에 가까운지)
        # 임시로 중심점 기준 정렬
        center = np.mean(corners, axis=0)

        # 각 점과 중심 간의 각도 계산
        angles = []
        for corner in corners:
            angle = np.arctan2(corner[1] - center[1], corner[0] - center[0])
            angles.append(angle)

        # 각도 기준 정렬
        sorted_indices = np.argsort(angles)
        sorted_corners = corners[sorted_indices]

        # 대각선 2개의 길이 계산
        diag1 = np.sqrt((sorted_corners[0][0] - sorted_corners[2][0])**2 +
                       (sorted_corners[0][1] - sorted_corners[2][1])**2)
        diag2 = np.sqrt((sorted_corners[1][0] - sorted_corners[3][0])**2 +
                       (sorted_corners[1][1] - sorted_corners[3][1])**2)

        # 대각선 길이 비율이 0.8~1.2 범위 내 (너무 찌그러진 사각형 제외)
        diag_ratio = min(diag1, diag2) / max(diag1, diag2)
        if diag_ratio < 0.8:
            logger.debug(f"❌ 검증 실패: 대각선 비율 ({diag_ratio:.2f} < 0.8)")
            return False

        # 3. 대변 길이 비율 검증 (직사각형 검증)
        # 정렬된 코너로 변의 길이 계산
        side1 = np.linalg.norm(sorted_corners[0] - sorted_corners[1])  # 상변
        side2 = np.linalg.norm(sorted_corners[1] - sorted_corners[2])  # 우변
        side3 = np.linalg.norm(sorted_corners[2] - sorted_corners[3])  # 하변
        side4 = np.linalg.norm(sorted_corners[3] - sorted_corners[0])  # 좌변

        # 대변(opposite sides) 길이 비율
        horizontal_ratio = min(side1, side3) / max(side1, side3)
        vertical_ratio = min(side2, side4) / max(side2, side4)

        if horizontal_ratio < 0.80 or vertical_ratio < 0.80:  # 0.85→0.80 완화 (약간의 왜곡 허용)
            logger.debug(f"❌ 검증 실패: 대변 비율 (H:{horizontal_ratio:.2f}, V:{vertical_ratio:.2f})")
            return False

        # 4. 종횡비 검증 (PCB 실제 비율 약 1.5 ~ 1.8)
        width = max(side1, side3)
        height = max(side2, side4)
        aspect_ratio = width / height

        if not (1.3 < aspect_ratio < 2.0):
            logger.debug(f"❌ 검증 실패: 종횡비 ({aspect_ratio:.2f} not in 1.3~2.0)")
            return False

        # 5. ROI 내부 검증 (선택사항)
        if roi_rect is not None:
            if not self._check_corners_in_roi(corners, roi_rect):
                logger.debug(f"❌ 검증 실패: 코너가 ROI 밖에 있음")
                return False

        logger.debug(f"✅ 직사각형 검증 통과 (대각:{diag_ratio:.2f}, 대변:H{horizontal_ratio:.2f}/V{vertical_ratio:.2f}, 종횡비:{aspect_ratio:.2f})")
        return True

    def _order_corners(self, corners: np.ndarray) -> List[Tuple[int, int]]:
        """
        4개 코너를 시계방향으로 정렬

        Args:
            corners (np.ndarray): 4개 코너 좌표

        Returns:
            list: [top_left, top_right, bottom_right, bottom_left]
        """
        # x + y 최소 → top-left
        top_left = corners[np.argmin(corners[:, 0] + corners[:, 1])]

        # x - y 최대 → top-right
        top_right = corners[np.argmax(corners[:, 0] - corners[:, 1])]

        # x + y 최대 → bottom-right
        bottom_right = corners[np.argmax(corners[:, 0] + corners[:, 1])]

        # y - x 최대 → bottom-left
        bottom_left = corners[np.argmax(corners[:, 1] - corners[:, 0])]

        return np.array([
            top_left,
            top_right,
            bottom_right,
            bottom_left
        ], dtype=np.int32)

    def process_frame(
        self,
        frame: np.ndarray,
        debug: bool = False
    ) -> Dict:
        """
        프레임 처리 (통합 함수)

        1. 나사 구멍 검출 시도
        2. 전체 PCB 가시성 검증
        3. PCB 정렬
        4. 실패 시 Edge 검출 fallback

        Args:
            frame (np.ndarray): 입력 프레임
            debug (bool): 디버그 모드

        Returns:
            dict: 처리 결과
                {
                    'success': bool,
                    'aligned_frame': np.ndarray or None,
                    'transform_matrix': np.ndarray or None,
                    'method': 'holes' or 'edges' or None,
                    'error': str or None,
                    'debug_info': dict
                }
        """
        result = {
            'success': False,
            'aligned_frame': None,
            'transform_matrix': None,
            'method': None,
            'error': None,
            'debug_info': {}
        }

        # 1차 시도: 윤곽선 기반 구멍 검출 (개선된 방법)
        detected_holes, debug_image = self.detect_mounting_holes_with_contour(frame, debug=debug)

        # 디버그 이미지 저장 (있으면)
        if debug_image is not None:
            result['debug_info']['debug_image'] = debug_image

        # 1차 시도 실패 시 2차 시도: ROI 기반 구멍 검출 (폴백)
        if detected_holes is None:
            if debug:
                logger.debug("윤곽선 기반 검출 실패, ROI 기반 방법 시도...")

            detected_holes, debug_image = self.detect_mounting_holes(frame, debug=debug)

            # 디버그 이미지 업데이트 (있으면)
            if debug_image is not None:
                result['debug_info']['debug_image'] = debug_image

        if detected_holes is not None:
            # 전체 PCB 가시성 검증
            is_visible, conditions = self.is_full_board_visible(
                detected_holes,
                image=frame
            )

            result['debug_info']['visibility_check'] = conditions

            if is_visible:
                # PCB 정렬
                aligned_frame, M = self.align_pcb(frame, detected_holes)

                result['success'] = True
                result['aligned_frame'] = aligned_frame
                result['transform_matrix'] = M
                result['method'] = 'holes'

                if debug:
                    logger.debug("나사 구멍 기반 정렬 성공")

                return result
            else:
                result['error'] = "PCB 전체가 프레임에 포함되지 않음"

                if debug:
                    logger.debug(f"가시성 검증 실패: {conditions}")

                return result

        # 2차 시도: PCB Edge 검출 (Fallback)
        if debug:
            logger.debug("나사 구멍 검출 실패, Edge 검출 시도...")

        edge_corners = self.detect_pcb_edges(frame, debug=debug)

        if edge_corners is not None:
            # Edge 기반 정렬
            aligned_frame, M = self.align_pcb(frame, edge_corners)

            result['success'] = True
            result['aligned_frame'] = aligned_frame
            result['transform_matrix'] = M
            result['method'] = 'edges'

            if debug:
                logger.debug("Edge 기반 정렬 성공")

            return result

        # 모든 방법 실패
        result['error'] = "PCB를 검출할 수 없음 (나사 구멍 및 Edge 검출 실패)"

        if debug:
            logger.debug("PCB 정렬 실패")

        return result
