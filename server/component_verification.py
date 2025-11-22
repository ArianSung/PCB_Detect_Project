"""
컴포넌트 위치 검증 모듈

정렬된 PCB 이미지에서 YOLO 검출 결과와 기준 위치를 비교하여
위치 오류 및 누락된 컴포넌트를 검출합니다.

주요 기능:
1. 검출된 컴포넌트와 기준 위치 매칭
2. 위치 오류 검출 (Misplaced Components)
3. 누락 컴포넌트 검출 (Missing Components)
4. 추가 컴포넌트 검출 (Extra Components)
"""

import numpy as np
from typing import List, Dict, Tuple, Optional
import logging
from scipy.spatial.distance import cdist

# 로깅 설정
logger = logging.getLogger(__name__)


class ComponentVerifier:
    """컴포넌트 위치 검증 클래스"""

    def __init__(
        self,
        reference_components: List[Dict],
        position_threshold: float = 20.0,
        confidence_threshold: float = 0.25
    ):
        """
        Args:
            reference_components (list): 기준 컴포넌트 정보
                [
                    {
                        'class_name': str,
                        'bbox': [x1, y1, x2, y2],
                        'center': [cx, cy],
                        'confidence': float
                    },
                    ...
                ]
            position_threshold (float): 위치 오류 판정 임계값 (픽셀 단위, 기본 20px)
            confidence_threshold (float): YOLO 신뢰도 임계값 (기본 0.25)
        """
        self.reference_components = reference_components
        self.position_threshold = position_threshold
        self.confidence_threshold = confidence_threshold

        # 기준 컴포넌트를 클래스별로 그룹화
        self.reference_by_class = self._group_by_class(reference_components)

        logger.info(
            f"ComponentVerifier 초기화 완료 "
            f"(기준 컴포넌트: {len(reference_components)}개, "
            f"위치 임계값: {position_threshold}px)"
        )

    def _group_by_class(
        self,
        components: List[Dict]
    ) -> Dict[str, List[Dict]]:
        """
        컴포넌트를 클래스별로 그룹화

        Args:
            components (list): 컴포넌트 리스트

        Returns:
            dict: {class_name: [component1, component2, ...]}
        """
        grouped = {}

        for comp in components:
            class_name = comp['class_name']

            if class_name not in grouped:
                grouped[class_name] = []

            grouped[class_name].append(comp)

        return grouped

    def verify_components(
        self,
        detected_components: List[Dict],
        debug: bool = False
    ) -> Dict:
        """
        컴포넌트 위치 검증

        Args:
            detected_components (list): YOLO 검출 결과
                [
                    {
                        'class_name': str,
                        'bbox': [x1, y1, x2, y2],
                        'center': [cx, cy],
                        'confidence': float
                    },
                    ...
                ]
            debug (bool): 디버그 정보 출력

        Returns:
            dict: 검증 결과
                {
                    'misplaced': [
                        {
                            'reference': {...},
                            'detected': {...},
                            'offset': float  # 픽셀 단위 거리
                        },
                        ...
                    ],
                    'missing': [
                        {...},  # 기준 컴포넌트 정보
                        ...
                    ],
                    'extra': [
                        {...},  # 검출된 컴포넌트 정보 (기준에 없음)
                        ...
                    ],
                    'correct': [
                        {
                            'reference': {...},
                            'detected': {...},
                            'offset': float
                        },
                        ...
                    ],
                    'summary': {
                        'total_reference': int,
                        'total_detected': int,
                        'misplaced_count': int,
                        'missing_count': int,
                        'extra_count': int,
                        'correct_count': int
                    }
                }
        """
        # 신뢰도 필터링
        detected_components = [
            comp for comp in detected_components
            if comp['confidence'] >= self.confidence_threshold
        ]

        # 검출된 컴포넌트를 클래스별로 그룹화
        detected_by_class = self._group_by_class(detected_components)

        misplaced = []
        missing = []
        extra = []
        correct = []

        # 기준 컴포넌트 순회
        for ref_comp in self.reference_components:
            class_name = ref_comp['class_name']
            ref_center = np.array(ref_comp['center'])

            # 동일 클래스 검출 결과
            detected_same_class = detected_by_class.get(class_name, [])

            if not detected_same_class:
                # 누락 (해당 클래스가 검출되지 않음)
                missing.append(ref_comp)
                continue

            # 가장 가까운 검출 결과 찾기
            closest_det, min_distance = self._find_closest_detection(
                ref_center,
                detected_same_class
            )

            if closest_det is None:
                # 누락
                missing.append(ref_comp)
                continue

            # 거리 임계값 비교
            if min_distance > self.position_threshold:
                # 위치 오류
                misplaced.append({
                    'reference': ref_comp,
                    'detected': closest_det,
                    'offset': float(min_distance)
                })
            else:
                # 정상
                correct.append({
                    'reference': ref_comp,
                    'detected': closest_det,
                    'offset': float(min_distance)
                })

            # 매칭된 검출 결과 제거 (중복 매칭 방지)
            detected_same_class.remove(closest_det)

        # 남은 검출 결과 = 추가 컴포넌트 (기준에 없음)
        for class_name, det_list in detected_by_class.items():
            for det_comp in det_list:
                extra.append(det_comp)

        # 요약 통계
        summary = {
            'total_reference': len(self.reference_components),
            'total_detected': len(detected_components),
            'misplaced_count': len(misplaced),
            'missing_count': len(missing),
            'extra_count': len(extra),
            'correct_count': len(correct)
        }

        if debug:
            logger.debug(f"컴포넌트 검증 완료: {summary}")

        return {
            'misplaced': misplaced,
            'missing': missing,
            'extra': extra,
            'correct': correct,
            'summary': summary
        }

    def _find_closest_detection(
        self,
        ref_center: np.ndarray,
        detected_list: List[Dict]
    ) -> Tuple[Optional[Dict], float]:
        """
        기준 위치에 가장 가까운 검출 결과 찾기

        Args:
            ref_center (np.ndarray): 기준 중심점 [cx, cy]
            detected_list (list): 동일 클래스 검출 결과 리스트

        Returns:
            tuple: (가장 가까운 검출 결과, 거리)
        """
        if not detected_list:
            return None, float('inf')

        # 모든 검출 결과의 중심점
        det_centers = np.array([det['center'] for det in detected_list])

        # 유클리드 거리 계산
        distances = np.linalg.norm(det_centers - ref_center, axis=1)

        # 최소 거리 찾기
        min_idx = np.argmin(distances)
        min_distance = distances[min_idx]

        closest_det = detected_list[min_idx]

        return closest_det, float(min_distance)

    def is_critical_defect(self, verification_result: Dict) -> Tuple[bool, str]:
        """
        치명적 불량 판정

        다음 경우 치명적 불량:
        1. 누락 컴포넌트 3개 이상
        2. 위치 오류 컴포넌트 5개 이상
        3. 누락 + 위치 오류 합계 7개 이상

        Args:
            verification_result (dict): verify_components() 결과

        Returns:
            tuple: (치명적 불량 여부, 사유)
        """
        missing_count = verification_result['summary']['missing_count']
        misplaced_count = verification_result['summary']['misplaced_count']
        total_issues = missing_count + misplaced_count

        # 판정 기준
        if missing_count >= 3:
            return True, f"누락 컴포넌트 {missing_count}개 (>= 3개)"

        if misplaced_count >= 5:
            return True, f"위치 오류 컴포넌트 {misplaced_count}개 (>= 5개)"

        if total_issues >= 7:
            return True, f"총 불량 {total_issues}개 (누락 {missing_count} + 위치 오류 {misplaced_count})"

        return False, "정상 범위"

    def generate_report(self, verification_result: Dict) -> str:
        """
        검증 결과 리포트 생성

        Args:
            verification_result (dict): verify_components() 결과

        Returns:
            str: 텍스트 리포트
        """
        summary = verification_result['summary']

        report_lines = [
            "=" * 60,
            "컴포넌트 위치 검증 리포트",
            "=" * 60,
            f"기준 컴포넌트: {summary['total_reference']}개",
            f"검출 컴포넌트: {summary['total_detected']}개",
            "",
            "[ 검증 결과 ]",
            f"✓ 정상: {summary['correct_count']}개",
            f"⚠ 위치 오류: {summary['misplaced_count']}개",
            f"✗ 누락: {summary['missing_count']}개",
            f"+ 추가: {summary['extra_count']}개",
            "",
        ]

        # 치명적 불량 판정
        is_critical, reason = self.is_critical_defect(verification_result)

        if is_critical:
            report_lines.append(f"[ 최종 판정 ] 🔴 치명적 불량")
            report_lines.append(f"사유: {reason}")
        else:
            if summary['misplaced_count'] > 0 or summary['missing_count'] > 0:
                report_lines.append(f"[ 최종 판정 ] 🟡 경미한 불량")
                report_lines.append(f"사유: {reason}")
            else:
                report_lines.append(f"[ 최종 판정 ] 🟢 정상")

        report_lines.append("=" * 60)

        # 위치 오류 상세
        if verification_result['misplaced']:
            report_lines.append("\n[ 위치 오류 상세 ]")

            for i, item in enumerate(verification_result['misplaced'], 1):
                ref = item['reference']
                det = item['detected']
                offset = item['offset']

                report_lines.append(
                    f"{i}. {ref['class_name']} - "
                    f"기준: {ref['center']}, "
                    f"검출: {det['center']}, "
                    f"오프셋: {offset:.1f}px"
                )

        # 누락 컴포넌트 상세
        if verification_result['missing']:
            report_lines.append("\n[ 누락 컴포넌트 상세 ]")

            for i, comp in enumerate(verification_result['missing'], 1):
                report_lines.append(
                    f"{i}. {comp['class_name']} - "
                    f"기준 위치: {comp['center']}"
                )

        # 추가 컴포넌트 상세
        if verification_result['extra']:
            report_lines.append("\n[ 추가 컴포넌트 상세 ]")

            for i, comp in enumerate(verification_result['extra'], 1):
                report_lines.append(
                    f"{i}. {comp['class_name']} - "
                    f"검출 위치: {comp['center']} "
                    f"(신뢰도: {comp['confidence']:.2f})"
                )

        return "\n".join(report_lines)


def euclidean_distance(point1: Tuple[float, float], point2: Tuple[float, float]) -> float:
    """
    두 점 간 유클리드 거리 계산

    Args:
        point1 (tuple): (x1, y1)
        point2 (tuple): (x2, y2)

    Returns:
        float: 거리
    """
    return np.sqrt((point1[0] - point2[0])**2 + (point1[1] - point2[1])**2)
