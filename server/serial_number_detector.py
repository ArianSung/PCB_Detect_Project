#!/usr/bin/env python3
"""
YOLO + OCR 기반 일련번호 검출 시스템

2단계 접근법:
1. YOLO: 일련번호 영역(ROI) 검출
2. OCR: ROI 내의 텍스트 인식
"""

import cv2
import numpy as np
import re
from pathlib import Path

# EasyOCR 임포트 (나중에 설치: pip install easyocr)
try:
    import easyocr
    EASYOCR_AVAILABLE = True
except ImportError:
    EASYOCR_AVAILABLE = False
    print("⚠️ Warning: EasyOCR not installed. Install with: pip install easyocr")

# YOLO 임포트
try:
    from ultralytics import YOLO
    YOLO_AVAILABLE = True
except ImportError:
    YOLO_AVAILABLE = False
    print("⚠️ Warning: Ultralytics not installed. Install with: pip install ultralytics")


class SerialNumberDetector:
    """YOLO + OCR 기반 일련번호 검출기"""

    def __init__(self, yolo_model_path, languages=['en'], use_gpu=True):
        """
        Args:
            yolo_model_path (str): YOLO 모델 경로
            languages (list): OCR 언어 리스트 (기본: ['en'])
            use_gpu (bool): GPU 사용 여부
        """
        if not YOLO_AVAILABLE:
            raise ImportError("Ultralytics YOLO가 설치되지 않았습니다: pip install ultralytics")

        if not EASYOCR_AVAILABLE:
            raise ImportError("EasyOCR이 설치되지 않았습니다: pip install easyocr")

        # YOLO 모델 로드 (일련번호 영역 검출)
        self.yolo_model = YOLO(yolo_model_path)
        print(f"✓ YOLO 모델 로드: {yolo_model_path}")

        # EasyOCR 리더 초기화 (텍스트 인식)
        self.ocr_reader = easyocr.Reader(languages, gpu=use_gpu)
        print(f"✓ OCR 리더 초기화: {languages} (GPU: {use_gpu})")

    def detect(self, image, yolo_conf=0.5, ocr_conf=0.7, validate=True):
        """
        일련번호 검출 및 인식

        Args:
            image (numpy.ndarray): OpenCV 이미지 (BGR)
            yolo_conf (float): YOLO 신뢰도 임계값 (기본: 0.5)
            ocr_conf (float): OCR 신뢰도 임계값 (기본: 0.7)
            validate (bool): 일련번호 유효성 검사 수행 여부

        Returns:
            dict: {
                'serial_number': str,          # 인식된 일련번호
                'confidence': float,           # OCR 신뢰도 (0.0~1.0)
                'yolo_bbox': list,             # YOLO bbox [x1, y1, x2, y2]
                'yolo_confidence': float,      # YOLO 신뢰도 (0.0~1.0)
                'success': bool,               # 성공 여부
                'error': str (optional)        # 실패 시 오류 메시지
            }
        """
        # 1단계: YOLO로 일련번호 영역 검출
        yolo_results = self.yolo_model.predict(
            image,
            conf=yolo_conf,
            verbose=False
        )

        # YOLO가 영역을 찾지 못한 경우
        if len(yolo_results[0].boxes) == 0:
            return {
                'serial_number': None,
                'confidence': 0.0,
                'yolo_bbox': None,
                'yolo_confidence': 0.0,
                'success': False,
                'error': 'YOLO가 일련번호 영역을 찾지 못했습니다.'
            }

        # 가장 신뢰도 높은 영역 선택
        best_box = yolo_results[0].boxes[0]
        bbox = best_box.xyxy[0].cpu().numpy().astype(int)  # [x1, y1, x2, y2]
        yolo_confidence = float(best_box.conf[0])

        x1, y1, x2, y2 = bbox

        # 2단계: ROI 추출 (일련번호 영역만 잘라내기)
        roi = image[y1:y2, x1:x2]

        if roi.size == 0:
            return {
                'serial_number': None,
                'confidence': 0.0,
                'yolo_bbox': bbox.tolist(),
                'yolo_confidence': yolo_confidence,
                'success': False,
                'error': 'ROI 크기가 0입니다.'
            }

        # 3단계: OCR로 텍스트 인식
        try:
            ocr_results = self.ocr_reader.readtext(roi, detail=1)
        except Exception as e:
            return {
                'serial_number': None,
                'confidence': 0.0,
                'yolo_bbox': bbox.tolist(),
                'yolo_confidence': yolo_confidence,
                'success': False,
                'error': f'OCR 오류: {str(e)}'
            }

        if len(ocr_results) == 0:
            return {
                'serial_number': None,
                'confidence': 0.0,
                'yolo_bbox': bbox.tolist(),
                'yolo_confidence': yolo_confidence,
                'success': False,
                'error': 'OCR이 텍스트를 인식하지 못했습니다.'
            }

        # 가장 신뢰도 높은 텍스트 선택
        best_text = max(ocr_results, key=lambda x: x[2])  # (bbox, text, confidence)
        serial_number = best_text[1].strip()
        ocr_confidence = best_text[2]

        # 4단계: 일련번호 유효성 검사 (선택 사항)
        if validate and not self.is_valid_serial_number(serial_number):
            return {
                'serial_number': serial_number,
                'confidence': ocr_confidence,
                'yolo_bbox': bbox.tolist(),
                'yolo_confidence': yolo_confidence,
                'success': False,
                'error': f'유효하지 않은 일련번호 형식: {serial_number}'
            }

        # 5단계: OCR 신뢰도 체크
        if ocr_confidence < ocr_conf:
            return {
                'serial_number': serial_number,
                'confidence': ocr_confidence,
                'yolo_bbox': bbox.tolist(),
                'yolo_confidence': yolo_confidence,
                'success': False,
                'error': f'OCR 신뢰도가 낮습니다 ({ocr_confidence:.3f} < {ocr_conf})'
            }

        # 성공!
        return {
            'serial_number': serial_number,
            'confidence': ocr_confidence,
            'yolo_bbox': bbox.tolist(),
            'yolo_confidence': yolo_confidence,
            'success': True
        }

    def is_valid_serial_number(self, text):
        """
        일련번호 유효성 검사 (정규식)

        Args:
            text (str): 인식된 텍스트

        Returns:
            bool: 유효한 일련번호인지 여부
        """
        # 예시 패턴들 (실제 프로젝트에 맞게 수정 필요)
        patterns = [
            r'^PCB[-_]?\d{4}[-_]?\d{6}$',  # PCB-2025-001234 또는 PCB2025001234
            r'^[A-Z]{2,4}\d{6,12}$',        # ABC123456789 (2-4글자 + 6-12숫자)
            r'^\d{10,15}$',                 # 1234567890123 (10-15자리 숫자)
            r'^[A-Z0-9]{8,20}$',            # 영문 대문자 + 숫자 조합 (8-20자)
        ]

        for pattern in patterns:
            if re.match(pattern, text, re.IGNORECASE):
                return True

        # 패턴 매칭 실패 시, 길이만 체크 (최소 6자 이상)
        if len(text) >= 6 and not text.isspace():
            return True

        return False

    def visualize(self, image, result, save_path=None):
        """
        검출 결과 시각화

        Args:
            image (numpy.ndarray): 원본 이미지
            result (dict): detect() 반환값
            save_path (str, optional): 저장 경로

        Returns:
            numpy.ndarray: 시각화된 이미지
        """
        vis_image = image.copy()

        if result['yolo_bbox'] is not None:
            x1, y1, x2, y2 = result['yolo_bbox']

            # 박스 색상 (성공: 초록, 실패: 빨강)
            color = (0, 255, 0) if result['success'] else (0, 0, 255)

            # 박스 그리기
            cv2.rectangle(vis_image, (x1, y1), (x2, y2), color, 2)

            # 텍스트 표시
            if result['serial_number']:
                label = f"{result['serial_number']} (OCR: {result['confidence']:.2f})"

                # 텍스트 배경
                (text_width, text_height), _ = cv2.getTextSize(
                    label, cv2.FONT_HERSHEY_SIMPLEX, 0.6, 2
                )
                cv2.rectangle(
                    vis_image,
                    (x1, y1 - text_height - 10),
                    (x1 + text_width, y1),
                    color,
                    -1
                )

                # 텍스트
                cv2.putText(
                    vis_image,
                    label,
                    (x1, y1 - 5),
                    cv2.FONT_HERSHEY_SIMPLEX,
                    0.6,
                    (255, 255, 255),
                    2
                )

            # 실패 시 오류 메시지
            if not result['success'] and 'error' in result:
                error_label = f"Error: {result['error'][:30]}..."
                cv2.putText(
                    vis_image,
                    error_label,
                    (x1, y2 + 20),
                    cv2.FONT_HERSHEY_SIMPLEX,
                    0.5,
                    (0, 0, 255),
                    1
                )

        # 저장
        if save_path:
            cv2.imwrite(save_path, vis_image)
            print(f"✓ 시각화 결과 저장: {save_path}")

        return vis_image


# ================================================================
# 테스트 코드
# ================================================================

if __name__ == "__main__":
    import sys

    print("="*80)
    print("일련번호 검출기 테스트")
    print("="*80)

    # 모델 경로
    yolo_model_path = 'runs/detect/serial_number_detector/weights/best.pt'

    if not Path(yolo_model_path).exists():
        print(f"❌ YOLO 모델을 찾을 수 없습니다: {yolo_model_path}")
        print("\n먼저 다음 명령어로 모델을 학습하세요:")
        print("  python scripts/train_serial_number_detector.py")
        sys.exit(1)

    # 검출기 초기화
    try:
        detector = SerialNumberDetector(
            yolo_model_path=yolo_model_path,
            languages=['en'],
            use_gpu=True
        )
    except ImportError as e:
        print(f"❌ {e}")
        sys.exit(1)

    # 테스트 이미지 경로
    test_image_path = 'data/raw/serial_number_detection/test/images/pcb_001.jpg'

    if not Path(test_image_path).exists():
        print(f"❌ 테스트 이미지를 찾을 수 없습니다: {test_image_path}")
        print("\n테스트 이미지를 준비하세요.")
        sys.exit(1)

    # 이미지 로드
    image = cv2.imread(test_image_path)

    # 검출
    print(f"\n이미지 검출 중: {test_image_path}")
    result = detector.detect(image, yolo_conf=0.5, ocr_conf=0.7, validate=True)

    # 결과 출력
    print("\n" + "="*80)
    print("검출 결과")
    print("="*80)
    print(f"  성공: {result['success']}")
    print(f"  일련번호: {result['serial_number']}")
    print(f"  OCR 신뢰도: {result['confidence']:.3f}")
    print(f"  YOLO 신뢰도: {result['yolo_confidence']:.3f}")
    print(f"  YOLO bbox: {result['yolo_bbox']}")

    if not result['success']:
        print(f"  ❌ 오류: {result.get('error', 'Unknown')}")

    # 시각화
    output_path = 'serial_number_detection_result.jpg'
    vis_image = detector.visualize(image, result, save_path=output_path)

    print("\n✓ 완료")
    print("="*80)
