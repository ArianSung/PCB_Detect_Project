"""
PCB 테두리 검출 모듈

ROI 영역 기반 최소자승법을 사용하여 PCB의 4개 테두리를 검출하고
교점을 계산하여 4개 코너 좌표를 반환합니다.

작성자: Claude Code
날짜: 2025-11-29
"""

import cv2
import numpy as np
from typing import Optional, Tuple, Dict, List
import logging

logger = logging.getLogger(__name__)


class PCBEdgeDetector:
    """PCB 테두리 검출 클래스"""

    def __init__(self, image_size: Tuple[int, int] = (640, 640)):
        """
        초기화

        Args:
            image_size: 이미지 크기 (width, height)
        """
        self.image_size = image_size

        # 640x640 이미지에 최적화된 ROI 설정
        self.rois = {
            'top':    (250, 20,  140, 150),   # (x, y, width, height)
            'bottom': (250, 470, 140, 150),
            'left':   (20,  250, 150, 140),
            'right':  (470, 250, 150, 140)
        }

        # 기본 임계값
        self.default_thresholds = {
            'top': 21,
            'bottom': 48,
            'left': 13,
            'right': 63
        }

    def fit_line_safe(self, points: List[List[int]], is_vertical: bool = False) -> Optional[Tuple[float, float]]:
        """
        최소자승법을 사용하여 직선 피팅

        Args:
            points: 점 리스트 [[x, y], ...]
            is_vertical: 수직선 여부

        Returns:
            (기울기, 절편) 튜플 또는 None
        """
        if len(points) < 2:
            return None

        try:
            pts = np.array(points)
            x, y = pts[:, 0], pts[:, 1]

            if is_vertical:
                # 수직선: x = m*y + c 형태로 변환
                m, c = np.polyfit(y, x, 1)
                if abs(m) < 1e-9:
                    m = 1e-9
                # y = a*x + b 형태로 재변환
                a = 1 / m
                b = -c / m
            else:
                # 일반 직선: y = a*x + b
                a, b = np.polyfit(x, y, 1)

            return a, b
        except Exception as e:
            logger.warning(f"직선 피팅 실패: {e}")
            return None

    def get_intersection(self, line1: Optional[Tuple[float, float]],
                        line2: Optional[Tuple[float, float]]) -> Optional[Tuple[int, int]]:
        """
        두 직선의 교점 계산

        Args:
            line1: 첫 번째 직선 (a, b) -> y = ax + b
            line2: 두 번째 직선 (a, b) -> y = ax + b

        Returns:
            교점 좌표 (x, y) 또는 None
        """
        if line1 is None or line2 is None:
            return None

        a1, b1 = line1
        a2, b2 = line2

        # 평행선 체크 (기울기 차이가 거의 없음)
        if abs(a1 - a2) < 1e-9:
            return None

        # 교점 계산: a1*x + b1 = a2*x + b2
        # (a1 - a2)*x = b2 - b1
        x = (b2 - b1) / (a1 - a2)
        y = a1 * x + b1

        return int(x), int(y)

    def draw_line(self, img: np.ndarray, a: float, b: float,
                  is_vertical: bool, color: Tuple[int, int, int]) -> None:
        """
        이미지에 직선 그리기

        Args:
            img: 대상 이미지
            a: 기울기
            b: 절편
            is_vertical: 수직선 여부
            color: 선 색상 (B, G, R)
        """
        h, w = img.shape[:2]

        try:
            if is_vertical:
                y1, y2 = 0, h
                x1 = int((y1 - b) / a) if a != 0 else 0
                x2 = int((y2 - b) / a) if a != 0 else 0
                cv2.line(img, (x1, y1), (x2, y2), color, 2)
            else:
                x1, x2 = 0, w
                y1 = int(a * x1 + b)
                y2 = int(a * x2 + b)
                cv2.line(img, (x1, y1), (x2, y2), color, 2)
        except Exception as e:
            logger.warning(f"직선 그리기 실패: {e}")

    def detect_edges(self, image: np.ndarray,
                    thresholds: Optional[Dict[str, int]] = None,
                    rois: Optional[Dict[str, Tuple[int, int, int, int]]] = None,
                    draw_debug: bool = True) -> Tuple[Optional[Dict[str, Tuple[int, int]]], Optional[np.ndarray]]:
        """
        PCB 테두리 검출 및 코너 좌표 계산

        Args:
            image: 입력 이미지 (BGR)
            thresholds: 방향별 임계값 {'top': int, 'bottom': int, 'left': int, 'right': int}
            rois: ROI 영역 {'top': (x, y, w, h), 'bottom': (x, y, w, h), ...} (optional)
            draw_debug: 디버그 이미지 생성 여부

        Returns:
            (코너 좌표 딕셔너리, 디버그 이미지) 튜플
            코너 좌표: {'tl': (x, y), 'tr': (x, y), 'bl': (x, y), 'br': (x, y)}
        """
        if thresholds is None:
            thresholds = self.default_thresholds

        # ROI가 제공되지 않으면 기본값 사용
        if rois is None:
            rois = self.rois
        else:
            # 제공된 ROI와 기본 ROI 병합 (제공된 것만 덮어씀)
            rois = {**self.rois, **rois}

        h, w = image.shape[:2]
        gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
        gray = cv2.GaussianBlur(gray, (5, 5), 0)

        # 디버그 이미지 준비
        if draw_debug:
            result_img = image.copy()
        else:
            result_img = None

        directions = ['top', 'bottom', 'left', 'right']
        colors = {
            'top': (0, 0, 255),      # 빨강
            'bottom': (255, 0, 0),   # 파랑
            'left': (0, 255, 0),     # 초록
            'right': (0, 255, 255)   # 노랑
        }

        # 검출된 직선 저장
        lines_found = {}

        for direction in directions:
            x, y, roi_w, roi_h = self.rois[direction]
            th = thresholds.get(direction, self.default_thresholds[direction])

            # ROI 범위 체크
            x = max(0, min(x, w - 1))
            y = max(0, min(y, h - 1))
            roi_w = min(roi_w, w - x)
            roi_h = min(roi_h, h - y)

            # 디버그: ROI 박스 그리기
            if draw_debug:
                cv2.rectangle(result_img, (x, y), (x + roi_w, y + roi_h), (0, 255, 0), 1)

            # ROI 추출
            roi_img = gray[y:y + roi_h, x:x + roi_w]
            points = []

            # 방향별 엣지 스캔 (바깥 -> 안쪽)
            if direction == 'top':
                # 위에서 아래로 스캔
                for c in range(roi_w):
                    for r in range(1, roi_h):
                        if abs(int(roi_img[r, c]) - int(roi_img[r-1, c])) > th:
                            points.append([x + c, y + r])
                            break

            elif direction == 'bottom':
                # 아래에서 위로 스캔
                for c in range(roi_w):
                    for r in range(roi_h - 2, -1, -1):
                        if abs(int(roi_img[r, c]) - int(roi_img[r+1, c])) > th:
                            points.append([x + c, y + r])
                            break

            elif direction == 'left':
                # 왼쪽에서 오른쪽으로 스캔
                for r in range(roi_h):
                    for c in range(1, roi_w):
                        if abs(int(roi_img[r, c]) - int(roi_img[r, c-1])) > th:
                            points.append([x + c, y + r])
                            break

            elif direction == 'right':
                # 오른쪽에서 왼쪽으로 스캔
                for r in range(roi_h):
                    for c in range(roi_w - 2, -1, -1):
                        if abs(int(roi_img[r, c]) - int(roi_img[r, c+1])) > th:
                            points.append([x + c, y + r])
                            break

            # 디버그: 검출된 점 그리기
            if draw_debug:
                for p in points:
                    cv2.circle(result_img, tuple(p), 1, colors[direction], -1)

            # 직선 피팅
            is_vertical = (direction in ['left', 'right'])
            line_params = self.fit_line_safe(points, is_vertical=is_vertical)

            if line_params:
                calc_a, calc_b = line_params

                # Bottom 직선은 절편 +30 이동 (원본 코드 유지)
                if direction == 'bottom':
                    calc_b += 30

                # 직선 저장
                lines_found[direction] = (calc_a, calc_b)

                # 디버그: 직선 그리기
                if draw_debug:
                    self.draw_line(result_img, calc_a, calc_b, is_vertical, colors[direction])

        # 코너 좌표 계산 (교점)
        corners = {}

        # Top-Left
        tl = self.get_intersection(lines_found.get('top'), lines_found.get('left'))
        if tl:
            corners['tl'] = tl
            if draw_debug:
                cv2.circle(result_img, tl, 8, (255, 0, 255), -1)  # 보라색
                cv2.putText(result_img, "TL", (tl[0] + 10, tl[1]),
                           cv2.FONT_HERSHEY_SIMPLEX, 0.5, (255, 0, 255), 2)

        # Top-Right
        tr = self.get_intersection(lines_found.get('top'), lines_found.get('right'))
        if tr:
            corners['tr'] = tr
            if draw_debug:
                cv2.circle(result_img, tr, 8, (255, 0, 255), -1)
                cv2.putText(result_img, "TR", (tr[0] - 30, tr[1]),
                           cv2.FONT_HERSHEY_SIMPLEX, 0.5, (255, 0, 255), 2)

        # Bottom-Left
        bl = self.get_intersection(lines_found.get('bottom'), lines_found.get('left'))
        if bl:
            corners['bl'] = bl
            if draw_debug:
                cv2.circle(result_img, bl, 8, (255, 0, 255), -1)
                cv2.putText(result_img, "BL", (bl[0] + 10, bl[1]),
                           cv2.FONT_HERSHEY_SIMPLEX, 0.5, (255, 0, 255), 2)

        # Bottom-Right
        br = self.get_intersection(lines_found.get('bottom'), lines_found.get('right'))
        if br:
            corners['br'] = br
            if draw_debug:
                cv2.circle(result_img, br, 8, (255, 0, 255), -1)
                cv2.putText(result_img, "BR", (br[0] - 30, br[1]),
                           cv2.FONT_HERSHEY_SIMPLEX, 0.5, (255, 0, 255), 2)

        return corners if corners else None, result_img


# 전역 인스턴스 생성 (싱글톤 패턴)
_edge_detector = PCBEdgeDetector()


def detect_pcb_edges(image: np.ndarray,
                    thresholds: Optional[Dict[str, int]] = None,
                    rois: Optional[Dict[str, Tuple[int, int, int, int]]] = None,
                    draw_debug: bool = True) -> Tuple[Optional[Dict[str, Tuple[int, int]]], Optional[np.ndarray]]:
    """
    PCB 테두리 검출 함수 (전역 인스턴스 사용)

    Args:
        image: 입력 이미지 (BGR)
        thresholds: 방향별 임계값
        rois: ROI 영역 (optional)
        draw_debug: 디버그 이미지 생성 여부

    Returns:
        (코너 좌표, 디버그 이미지) 튜플
    """
    return _edge_detector.detect_edges(image, thresholds, rois, draw_debug)
