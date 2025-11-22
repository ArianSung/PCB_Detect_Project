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

        # 가우시안 블러
        blurred = cv2.GaussianBlur(gray, (9, 9), 2)

        # Hough Circle Transform
        circles = cv2.HoughCircles(
            blurred,
            cv2.HOUGH_GRADIENT,
            dp=1,
            minDist=50,
            param1=50,
            param2=30,
            minRadius=5,
            maxRadius=20
        )

        if circles is None:
            if debug:
                logger.debug("나사 구멍 검출 실패")
            return None

        circles = np.uint16(np.around(circles))
        holes = [(int(x), int(y)) for x, y, r in circles[0, :]]

        if debug:
            logger.debug(f"검출된 원형 객체: {len(holes)}개")

        # 4개 코너 구멍 선택
        if len(holes) < 4:
            if debug:
                logger.debug(f"구멍이 4개 미만입니다: {len(holes)}개")
            return None

        corner_holes = self._select_corner_holes(holes)

        if len(corner_holes) != 4:
            return None

        return corner_holes

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

        # ±10% 범위 내
        if (0.9 <= width_ratio <= 1.1 and
            0.9 <= height_ratio <= 1.1 and
            0.9 <= diagonal_ratio <= 1.1):
            conditions['distances'] = True

        # 조건 3: PCB contour 면적 (옵션)
        if image is not None:
            pcb_area = self._estimate_pcb_area(image)
            expected_area = self.image_size[0] * self.image_size[1]

            if pcb_area >= expected_area * 0.95:
                conditions['area'] = True

        # 2개 이상 만족 → 전체 보임
        satisfied_count = sum(conditions.values())
        is_visible = satisfied_count >= 2

        return is_visible, conditions

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

    def detect_pcb_edges(
        self,
        image: np.ndarray,
        debug: bool = False
    ) -> Optional[np.ndarray]:
        """
        PCB 가장자리 검출 (Fallback 방법)

        Args:
            image (np.ndarray): 입력 이미지
            debug (bool): 디버그 정보 출력

        Returns:
            np.ndarray: 4개 코너 좌표 [(x1,y1), (x2,y2), (x3,y3), (x4,y4)]
                       또는 None (검출 실패)
        """
        # 그레이스케일 변환
        gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)

        # Canny Edge Detection
        edges = cv2.Canny(gray, 50, 150)

        # Contour 검출
        contours, _ = cv2.findContours(
            edges,
            cv2.RETR_EXTERNAL,
            cv2.CHAIN_APPROX_SIMPLE
        )

        if not contours:
            if debug:
                logger.debug("PCB 가장자리 검출 실패")
            return None

        # 가장 큰 contour = PCB
        largest_contour = max(contours, key=cv2.contourArea)

        # Contour를 사각형으로 근사
        epsilon = 0.02 * cv2.arcLength(largest_contour, True)
        approx = cv2.approxPolyDP(largest_contour, epsilon, True)

        # 4개 코너 추출
        if len(approx) >= 4:
            # 첫 4개 점 선택
            corners = approx[:4].reshape(-1, 2)

            # 시계방향 정렬: top-left, top-right, bottom-right, bottom-left
            corners = self._order_corners(corners)

            if debug:
                logger.debug(f"PCB 가장자리 검출 성공: {len(approx)}개 점")

            return corners

        if debug:
            logger.debug(f"PCB 가장자리가 사각형이 아님: {len(approx)}개 점")

        return None

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

        return [
            tuple(top_left),
            tuple(top_right),
            tuple(bottom_right),
            tuple(bottom_left)
        ]

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

        # 1차 시도: 나사 구멍 검출
        detected_holes = self.detect_mounting_holes(frame, debug=debug)

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
