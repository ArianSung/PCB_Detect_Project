"""
템플릿 매칭 기반 PCB 정렬 시스템

하나의 기준점(나사 구멍)을 템플릿 매칭으로 찾아서 (0,0)으로 설정하고,
모든 YOLO 검출 결과를 상대 좌표로 변환합니다.

작성자: Claude Code
날짜: 2025-12-01
"""

import cv2
import numpy as np
from typing import Optional, Tuple, List, Dict
import logging

logger = logging.getLogger(__name__)


class TemplateBasedAlignment:
    """템플릿 매칭 기반 PCB 정렬 클래스"""

    def __init__(self, template_path: Optional[str] = None, threshold: float = 0.8):
        """
        초기화

        Args:
            template_path: 기준점 템플릿 이미지 경로 (나사 구멍 등)
            threshold: 템플릿 매칭 최소 신뢰도 (0.0~1.0, 기본값: 0.8)
        """
        self.template = None
        self.template_path = template_path
        self.threshold = threshold  # 신뢰도 임계값 추가

        if template_path:
            self.load_template(template_path)

    def load_template(self, template_path: str) -> bool:
        """
        템플릿 이미지 로드

        Args:
            template_path: 템플릿 이미지 경로

        Returns:
            성공 여부
        """
        try:
            self.template = cv2.imread(template_path)
            if self.template is None:
                logger.error(f"템플릿 로드 실패: {template_path}")
                return False

            logger.info(f"템플릿 로드 성공: {template_path} (shape={self.template.shape})")
            self.template_path = template_path
            return True
        except Exception as e:
            logger.error(f"템플릿 로드 중 오류: {e}", exc_info=True)
            return False

    def set_template_from_image(self, image: np.ndarray, x: int, y: int, width: int, height: int) -> bool:
        """
        이미지에서 직접 템플릿 영역 추출

        Args:
            image: 원본 이미지
            x, y: 템플릿 영역 좌상단 좌표
            width, height: 템플릿 영역 크기

        Returns:
            성공 여부
        """
        try:
            self.template = image[y:y+height, x:x+width].copy()
            logger.info(f"템플릿 설정 완료: ({x}, {y}, {width}, {height})")
            return True
        except Exception as e:
            logger.error(f"템플릿 설정 중 오류: {e}", exc_info=True)
            return False

    def find_reference_point(
        self,
        image: np.ndarray,
        method: int = cv2.TM_CCOEFF_NORMED,
        roi: Optional[Tuple[int, int, int, int]] = None
    ) -> Optional[Tuple[int, int]]:
        """
        템플릿 매칭으로 기준점 찾기 (ROI 검증 포함)

        Args:
            image: 검색할 이미지
            method: 매칭 방법 (cv2.TM_CCOEFF_NORMED, cv2.TM_CCORR_NORMED 등)
            roi: ROI 영역 (x1, y1, x2, y2). None이면 전체 이미지에서 검색

        Returns:
            기준점 좌표 (x, y) 또는 None (ROI 밖에 있으면 None)
        """
        if self.template is None:
            logger.warning("템플릿이 설정되지 않았습니다.")
            return None

        try:
            # 그레이스케일 변환
            img_gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
            template_gray = cv2.cvtColor(self.template, cv2.COLOR_BGR2GRAY)

            # 템플릿 매칭
            result = cv2.matchTemplate(img_gray, template_gray, method)
            min_val, max_val, min_loc, max_loc = cv2.minMaxLoc(result)

            # TM_SQDIFF, TM_SQDIFF_NORMED는 최소값이 최적 매칭
            if method in [cv2.TM_SQDIFF, cv2.TM_SQDIFF_NORMED]:
                match_loc = min_loc
                confidence = 1 - min_val
            else:
                match_loc = max_loc
                confidence = max_val

            # 신뢰도 검증 ⭐ (잘못된 매칭 방지)
            if confidence < self.threshold:
                logger.warning(
                    f"템플릿 매칭 신뢰도가 임계값보다 낮음: {confidence:.3f} < {self.threshold:.3f}"
                )
                return None

            # 템플릿 중심점 계산 (기준점)
            template_h, template_w = self.template.shape[:2]
            ref_x = match_loc[0] + template_w // 2
            ref_y = match_loc[1] + template_h // 2

            # ROI 검증
            if roi is not None:
                x1, y1, x2, y2 = roi
                if not (x1 <= ref_x <= x2 and y1 <= ref_y <= y2):
                    logger.warning(
                        f"기준점이 ROI 밖에 있음: ({ref_x}, {ref_y}), "
                        f"ROI=({x1}, {y1}, {x2}, {y2}), confidence={confidence:.3f}"
                    )
                    return None

            logger.info(f"기준점 검출 성공: ({ref_x}, {ref_y}), confidence={confidence:.3f}")
            return (ref_x, ref_y)

        except Exception as e:
            logger.error(f"기준점 검출 중 오류: {e}", exc_info=True)
            return None

    def convert_to_relative_coords(self, detections: List[Dict], reference_point: Tuple[int, int]) -> List[Dict]:
        """
        YOLO 검출 결과를 상대 좌표로 변환

        Args:
            detections: YOLO 검출 결과 리스트
                [{'class': 'component1', 'bbox': [x, y, w, h], 'confidence': 0.9}, ...]
            reference_point: 기준점 좌표 (x, y)

        Returns:
            상대 좌표로 변환된 검출 결과
        """
        ref_x, ref_y = reference_point
        relative_detections = []

        for det in detections:
            bbox = det['bbox']
            # bbox 중심점 계산
            center_x = bbox[0] + bbox[2] / 2
            center_y = bbox[1] + bbox[3] / 2

            # 상대 좌표 변환
            rel_x = center_x - ref_x
            rel_y = center_y - ref_y

            relative_det = det.copy()
            relative_det['relative_position'] = (rel_x, rel_y)
            relative_det['absolute_position'] = (center_x, center_y)
            relative_det['reference_point'] = reference_point

            relative_detections.append(relative_det)

        logger.info(f"{len(detections)}개 검출 결과를 상대 좌표로 변환 완료")
        return relative_detections

    def visualize_reference_point(
        self,
        image: np.ndarray,
        reference_point: Tuple[int, int],
        roi: Optional[Tuple[int, int, int, int]] = None
    ) -> np.ndarray:
        """
        기준점, 템플릿 영역, ROI를 시각화

        Args:
            image: 원본 이미지
            reference_point: 기준점 좌표
            roi: ROI 영역 (x1, y1, x2, y2)

        Returns:
            시각화된 이미지
        """
        if self.template is None:
            return image.copy()

        result_img = image.copy()
        ref_x, ref_y = reference_point

        # ROI 영역 표시 (노란색, 반투명)
        if roi is not None:
            x1, y1, x2, y2 = roi
            overlay = result_img.copy()
            cv2.rectangle(overlay, (x1, y1), (x2, y2), (0, 255, 255), 3)
            cv2.putText(
                overlay,
                "ROI",
                (x1 + 10, y1 + 30),
                cv2.FONT_HERSHEY_SIMPLEX,
                0.8,
                (0, 255, 255),
                2
            )
            # 반투명 효과
            result_img = cv2.addWeighted(overlay, 0.7, result_img, 0.3, 0)

        # 템플릿 영역 표시 (초록색)
        template_h, template_w = self.template.shape[:2]
        top_left_x = ref_x - template_w // 2
        top_left_y = ref_y - template_h // 2

        cv2.rectangle(
            result_img,
            (top_left_x, top_left_y),
            (top_left_x + template_w, top_left_y + template_h),
            (0, 255, 0),
            2
        )

        # 기준점 표시 (빨간색)
        cv2.circle(result_img, reference_point, 10, (0, 0, 255), -1)
        cv2.putText(
            result_img,
            "REF (0,0)",
            (ref_x + 15, ref_y),
            cv2.FONT_HERSHEY_SIMPLEX,
            0.6,
            (0, 0, 255),
            2
        )

        # 좌표축 표시
        cv2.arrowedLine(result_img, reference_point, (ref_x + 50, ref_y), (255, 0, 0), 2)  # X축
        cv2.arrowedLine(result_img, reference_point, (ref_x, ref_y + 50), (0, 255, 0), 2)  # Y축
        cv2.putText(result_img, "X", (ref_x + 55, ref_y), cv2.FONT_HERSHEY_SIMPLEX, 0.5, (255, 0, 0), 2)
        cv2.putText(result_img, "Y", (ref_x, ref_y + 65), cv2.FONT_HERSHEY_SIMPLEX, 0.5, (0, 255, 0), 2)

        return result_img


def create_template_from_coords(image: np.ndarray, x: int, y: int, size: int = 40) -> np.ndarray:
    """
    이미지에서 특정 좌표 주변 영역을 템플릿으로 추출

    Args:
        image: 원본 이미지
        x, y: 중심 좌표
        size: 템플릿 크기 (정사각형)

    Returns:
        템플릿 이미지
    """
    half_size = size // 2
    x1 = max(0, x - half_size)
    y1 = max(0, y - half_size)
    x2 = min(image.shape[1], x + half_size)
    y2 = min(image.shape[0], y + half_size)

    template = image[y1:y2, x1:x2].copy()
    return template


# 전역 인스턴스 (싱글톤 패턴)
_alignment_system = None


def get_alignment_system(template_path: Optional[str] = None) -> TemplateBasedAlignment:
    """
    전역 정렬 시스템 인스턴스 반환

    Args:
        template_path: 템플릿 경로 (최초 생성 시만 사용)

    Returns:
        TemplateBasedAlignment 인스턴스
    """
    global _alignment_system

    if _alignment_system is None:
        _alignment_system = TemplateBasedAlignment(template_path)

    return _alignment_system
