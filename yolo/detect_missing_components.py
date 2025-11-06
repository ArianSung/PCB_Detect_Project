#!/usr/bin/env python3
"""
PCB 부품 누락 검출 시스템

Golden Template과 YOLO 검출 결과를 비교하여 누락된 부품을 찾습니다.
"""

import numpy as np
from ultralytics import YOLO
import cv2
from typing import List, Dict, Tuple
from dataclasses import dataclass

@dataclass
class Component:
    """부품 정보"""
    class_name: str  # 부품 클래스 (R, C, IC 등)
    x: float         # x 좌표 (normalized 0-1)
    y: float         # y 좌표 (normalized 0-1)
    w: float         # 너비 (normalized 0-1)
    h: float         # 높이 (normalized 0-1)
    confidence: float = 1.0  # 신뢰도 (Golden Template은 1.0)

class MissingComponentDetector:
    """
    PCB 부품 누락 검출기

    Golden Template과 YOLO 검출 결과를 비교하여:
    1. 누락된 부품 (Missing Component)
    2. 잘못된 위치의 부품 (Misaligned Component)
    3. 추가된 부품 (Extra Component)
    를 검출합니다.
    """

    def __init__(self, model_path: str, golden_template: List[Component],
                 position_threshold: float = 0.1):
        """
        Args:
            model_path: YOLO 모델 경로
            golden_template: 정상 PCB의 부품 목록
            position_threshold: 위치 오차 허용 범위 (normalized, 0-1)
                               예: 0.1 = 이미지 크기의 10% 이내
        """
        self.model = YOLO(model_path)
        self.golden_template = golden_template
        self.position_threshold = position_threshold

    def detect_components(self, image_path: str) -> List[Component]:
        """YOLO로 부품 검출"""
        results = self.model.predict(image_path, conf=0.25)

        detected_components = []
        for result in results:
            boxes = result.boxes
            for box in boxes:
                # YOLO 결과 파싱
                x1, y1, x2, y2 = box.xyxyn[0].cpu().numpy()  # normalized coordinates
                x_center = (x1 + x2) / 2
                y_center = (y1 + y2) / 2
                width = x2 - x1
                height = y2 - y1

                class_id = int(box.cls[0])
                class_name = result.names[class_id]
                confidence = float(box.conf[0])

                component = Component(
                    class_name=class_name,
                    x=x_center,
                    y=y_center,
                    w=width,
                    h=height,
                    confidence=confidence
                )
                detected_components.append(component)

        return detected_components

    def calculate_distance(self, comp1: Component, comp2: Component) -> float:
        """두 부품 간 거리 계산 (Euclidean distance)"""
        return np.sqrt((comp1.x - comp2.x)**2 + (comp1.y - comp2.y)**2)

    def match_components(self, detected: List[Component]) -> Dict[str, List[Component]]:
        """
        Golden Template과 검출된 부품을 매칭

        Returns:
            {
                'missing': 누락된 부품 목록,
                'misaligned': 위치가 잘못된 부품 목록,
                'extra': 추가된 부품 목록,
                'matched': 정상 매칭된 부품 목록
            }
        """
        missing = []
        misaligned = []
        extra = []
        matched = []

        # Golden Template의 각 부품에 대해
        golden_used = [False] * len(self.golden_template)
        detected_used = [False] * len(detected)

        for i, golden_comp in enumerate(self.golden_template):
            # 같은 클래스의 검출된 부품 중 가장 가까운 것 찾기
            best_match_idx = None
            best_distance = float('inf')

            for j, det_comp in enumerate(detected):
                if detected_used[j]:
                    continue

                # 같은 클래스인 경우만
                if det_comp.class_name == golden_comp.class_name:
                    distance = self.calculate_distance(golden_comp, det_comp)
                    if distance < best_distance:
                        best_distance = distance
                        best_match_idx = j

            # 매칭 결과 처리
            if best_match_idx is None:
                # 매칭 안됨 → 누락된 부품
                missing.append(golden_comp)
            elif best_distance > self.position_threshold:
                # 거리가 너무 멈 → 위치 오류
                misaligned.append({
                    'golden': golden_comp,
                    'detected': detected[best_match_idx],
                    'distance': best_distance
                })
                detected_used[best_match_idx] = True
                golden_used[i] = True
            else:
                # 정상 매칭
                matched.append({
                    'golden': golden_comp,
                    'detected': detected[best_match_idx],
                    'distance': best_distance
                })
                detected_used[best_match_idx] = True
                golden_used[i] = True

        # 매칭되지 않은 검출 부품 → 추가된 부품
        for j, det_comp in enumerate(detected):
            if not detected_used[j]:
                extra.append(det_comp)

        return {
            'missing': missing,
            'misaligned': misaligned,
            'extra': extra,
            'matched': matched
        }

    def visualize_results(self, image_path: str, match_results: Dict,
                         output_path: str = None):
        """
        검출 결과 시각화

        - 초록색 박스: 정상 매칭
        - 빨간색 박스: 누락된 부품 (Golden Template 위치)
        - 노란색 박스: 위치 오류
        - 파란색 박스: 추가된 부품
        """
        image = cv2.imread(image_path)
        h, w = image.shape[:2]

        # 정상 매칭 (초록색)
        for match in match_results['matched']:
            comp = match['detected']
            x1 = int((comp.x - comp.w/2) * w)
            y1 = int((comp.y - comp.h/2) * h)
            x2 = int((comp.x + comp.w/2) * w)
            y2 = int((comp.y + comp.h/2) * h)
            cv2.rectangle(image, (x1, y1), (x2, y2), (0, 255, 0), 2)
            cv2.putText(image, f"{comp.class_name} OK", (x1, y1-5),
                       cv2.FONT_HERSHEY_SIMPLEX, 0.5, (0, 255, 0), 2)

        # 누락된 부품 (빨간색)
        for comp in match_results['missing']:
            x1 = int((comp.x - comp.w/2) * w)
            y1 = int((comp.y - comp.h/2) * h)
            x2 = int((comp.x + comp.w/2) * w)
            y2 = int((comp.y + comp.h/2) * h)
            cv2.rectangle(image, (x1, y1), (x2, y2), (0, 0, 255), 3)
            cv2.putText(image, f"{comp.class_name} MISSING!", (x1, y1-5),
                       cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 0, 255), 2)

        # 위치 오류 (노란색)
        for mismatch in match_results['misaligned']:
            comp = mismatch['detected']
            x1 = int((comp.x - comp.w/2) * w)
            y1 = int((comp.y - comp.h/2) * h)
            x2 = int((comp.x + comp.w/2) * w)
            y2 = int((comp.y + comp.h/2) * h)
            cv2.rectangle(image, (x1, y1), (x2, y2), (0, 255, 255), 2)
            distance_px = mismatch['distance'] * np.sqrt(w**2 + h**2)
            cv2.putText(image, f"{comp.class_name} OFF {distance_px:.0f}px",
                       (x1, y1-5), cv2.FONT_HERSHEY_SIMPLEX, 0.5, (0, 255, 255), 2)

        # 추가된 부품 (파란색)
        for comp in match_results['extra']:
            x1 = int((comp.x - comp.w/2) * w)
            y1 = int((comp.y - comp.h/2) * h)
            x2 = int((comp.x + comp.w/2) * w)
            y2 = int((comp.y + comp.h/2) * h)
            cv2.rectangle(image, (x1, y1), (x2, y2), (255, 0, 0), 2)
            cv2.putText(image, f"{comp.class_name} EXTRA", (x1, y1-5),
                       cv2.FONT_HERSHEY_SIMPLEX, 0.5, (255, 0, 0), 2)

        if output_path:
            cv2.imwrite(output_path, image)

        return image

    def generate_report(self, match_results: Dict) -> str:
        """검사 보고서 생성"""
        report = "=" * 60 + "\n"
        report += "PCB 부품 검사 보고서\n"
        report += "=" * 60 + "\n\n"

        # 요약
        total_expected = len(self.golden_template)
        total_detected = len(match_results['matched']) + len(match_results['misaligned'])
        missing_count = len(match_results['missing'])
        misaligned_count = len(match_results['misaligned'])
        extra_count = len(match_results['extra'])

        report += f"예상 부품 수: {total_expected}\n"
        report += f"검출 부품 수: {total_detected}\n"
        report += f"정상 매칭: {len(match_results['matched'])}\n"
        report += f"누락: {missing_count}\n"
        report += f"위치 오류: {misaligned_count}\n"
        report += f"추가: {extra_count}\n\n"

        # 판정
        if missing_count == 0 and misaligned_count == 0 and extra_count == 0:
            report += "✅ 판정: 정상 (PASS)\n"
        else:
            report += "❌ 판정: 불량 (FAIL)\n"

        # 상세 내역
        if missing_count > 0:
            report += "\n🔴 누락된 부품:\n"
            for comp in match_results['missing']:
                report += f"  - {comp.class_name} at ({comp.x:.3f}, {comp.y:.3f})\n"

        if misaligned_count > 0:
            report += "\n🟡 위치 오류:\n"
            for mismatch in match_results['misaligned']:
                report += f"  - {mismatch['detected'].class_name} "
                report += f"distance: {mismatch['distance']:.3f}\n"

        if extra_count > 0:
            report += "\n🔵 추가된 부품:\n"
            for comp in match_results['extra']:
                report += f"  - {comp.class_name} at ({comp.x:.3f}, {comp.y:.3f})\n"

        report += "=" * 60 + "\n"
        return report


def create_golden_template_from_good_pcb(model_path: str,
                                         good_pcb_image: str) -> List[Component]:
    """
    정상 PCB 이미지로부터 Golden Template 생성

    Args:
        model_path: YOLO 모델 경로
        good_pcb_image: 정상 PCB 이미지 경로

    Returns:
        Golden Template (부품 목록)
    """
    model = YOLO(model_path)
    results = model.predict(good_pcb_image, conf=0.25)

    golden_template = []
    for result in results:
        boxes = result.boxes
        for box in boxes:
            x1, y1, x2, y2 = box.xyxyn[0].cpu().numpy()
            x_center = (x1 + x2) / 2
            y_center = (y1 + y2) / 2
            width = x2 - x1
            height = y2 - y1

            class_id = int(box.cls[0])
            class_name = result.names[class_id]

            component = Component(
                class_name=class_name,
                x=x_center,
                y=y_center,
                w=width,
                h=height,
                confidence=1.0
            )
            golden_template.append(component)

    return golden_template


# 사용 예시
if __name__ == "__main__":
    # 1. Golden Template 생성 (정상 PCB로부터)
    model_path = "runs/detect/component_model_optimized/weights/best.pt"
    good_pcb_path = "path/to/good_pcb.jpg"

    golden_template = create_golden_template_from_good_pcb(model_path, good_pcb_path)
    print(f"Golden Template 생성 완료: {len(golden_template)}개 부품")

    # 2. 검사 대상 PCB 검사
    detector = MissingComponentDetector(
        model_path=model_path,
        golden_template=golden_template,
        position_threshold=0.05  # 이미지 크기의 5% 이내
    )

    test_pcb_path = "path/to/test_pcb.jpg"

    # 검출
    detected_components = detector.detect_components(test_pcb_path)
    print(f"검출 완료: {len(detected_components)}개 부품")

    # 매칭
    match_results = detector.match_components(detected_components)

    # 보고서 출력
    report = detector.generate_report(match_results)
    print(report)

    # 시각화
    detector.visualize_results(test_pcb_path, match_results,
                              output_path="inspection_result.jpg")
    print("결과 이미지 저장: inspection_result.jpg")
