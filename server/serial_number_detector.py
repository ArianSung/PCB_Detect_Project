#!/usr/bin/env python3
"""
시리얼 넘버 OCR 검출 모듈 (PaddleOCR 버전)

기능:
    - PaddleOCR을 이용한 시리얼 넘버 텍스트 인식
    - 정규식 기반 시리얼 넘버 파싱 (S/N MBXX-00000001 형식)
    - 제품 코드 추출 (MBXX에서 XX 추출)
    - 딥러닝 기반 고정확도 OCR

예시:
    S/N MBBC-00000001 → 제품 코드: BC
    S/N MBFT-12345678 → 제품 코드: FT
    S/N MBRS-99999999 → 제품 코드: RS

PaddleOCR 장점:
    - 딥러닝 기반, 매우 높은 정확도
    - 다양한 폰트와 각도에 강인함
    - CPU/GPU 모두 지원
    - 영어/숫자 인식 우수
"""

import re
import cv2
import numpy as np
from paddleocr import PaddleOCR
import logging
from typing import Optional, Tuple, Dict

logger = logging.getLogger(__name__)


class SerialNumberDetector:
    """시리얼 넘버 OCR 검출기 (PaddleOCR 버전)"""

    # 시리얼 넘버 정규식 패턴 (OCR 오인식 패턴 포함)
    # 형식: S/N MBXX-00000001
    # S/N의 /를 I, l, |, 1 등으로 오인식 가능
    SERIAL_PATTERN = re.compile(
        r'S[/\\ILl|1]N[\s:]*MB([A-Z]{2})[\s-]*(\d{6,10})',
        re.IGNORECASE
    )

    # 간단한 패턴 (S/N 없이, 숫자 6~10자리)
    SIMPLE_PATTERN = re.compile(
        r'(?<![A-Z])MB([A-Z]{2})[\s-]*(\d{6,10})(?!\d)',
        re.IGNORECASE
    )

    # 유연한 패턴 (구분자 관대)
    FLEXIBLE_PATTERN = re.compile(
        r'(?<![A-Z])MB[\s]*([A-Z]{2})[\s\-_:]*(\d{6,10})(?!\d)',
        re.IGNORECASE
    )

    # 초완화 패턴 (4~10자리로 제한)
    ULTRA_FLEXIBLE_PATTERN = re.compile(
        r'(?<![A-Z])MB[\s]*([A-Z]{2})[\s\-_:]*(\d{4,10})(?!\d)',
        re.IGNORECASE
    )

    def __init__(self, use_gpu=False, lang='en', det_db_thresh=0.3, det_db_box_thresh=0.5):
        """
        Args:
            use_gpu: GPU 사용 여부 (기본값: False, CPU 사용)
            lang: OCR 언어 ('en' = 영어, 'ch' = 중국어+영어)
            det_db_thresh: 텍스트 검출 임계값 (낮을수록 더 많이 검출, 기본 0.3)
            det_db_box_thresh: 박스 임계값 (낮을수록 더 많이 검출, 기본 0.5)
        """
        self.use_gpu = use_gpu
        self.lang = lang
        self.det_db_thresh = det_db_thresh
        self.det_db_box_thresh = det_db_box_thresh
        self.ocr = None

        logger.info("🔤 시리얼 넘버 OCR 검출기 초기화 중 (PaddleOCR 버전)...")
        self._initialize_paddleocr()

    def _initialize_paddleocr(self):
        """PaddleOCR 초기화"""
        try:
            # PaddleOCR 초기화
            # use_angle_cls=True: 텍스트 각도 분류 사용 (회전된 텍스트 인식 향상)
            # lang='en': 영어 모델 사용
            # show_log=False: 로그 최소화
            self.ocr = PaddleOCR(
                use_angle_cls=True,  # 각도 분류 활성화 (회전 텍스트 인식)
                lang=self.lang,
                use_gpu=self.use_gpu,
                show_log=False,  # PaddleOCR 내부 로그 비활성화
                det_db_thresh=self.det_db_thresh,  # 검출 임계값
                det_db_box_thresh=self.det_db_box_thresh,  # 박스 임계값
                # rec_algorithm='CRNN',  # 인식 알고리즘 (기본값)
                # det_algorithm='DB',    # 검출 알고리즘 (기본값)
            )
            logger.info(f"✅ PaddleOCR 초기화 완료")
            logger.info(f"   - GPU 사용: {self.use_gpu}")
            logger.info(f"   - 언어: {self.lang}")
            logger.info(f"   - 각도 분류: True (회전 텍스트 지원)")
            logger.info(f"   - 검출 임계값: {self.det_db_thresh}")
            logger.info(f"   - 박스 임계값: {self.det_db_box_thresh}")
        except Exception as e:
            logger.error(f"❌ PaddleOCR 초기화 실패: {e}")
            raise

    def preprocess_image(self, image: np.ndarray) -> np.ndarray:
        """
        OCR 전처리 (90도 회전 + 업스케일링 + CLAHE + 선명화)

        PaddleOCR는 그레이스케일보다 컬러 이미지에서 더 잘 작동하므로
        이진화는 하지 않음

        Args:
            image: 입력 이미지

        Returns:
            전처리된 이미지
        """
        # **1단계: 오른쪽으로 90도 회전** (시리얼 넘버가 옆으로 누워있음)
        rotated = cv2.rotate(image, cv2.ROTATE_90_CLOCKWISE)

        # **2단계: 그레이스케일 변환**
        if len(rotated.shape) == 3:
            gray = cv2.cvtColor(rotated, cv2.COLOR_BGR2GRAY)
        else:
            gray = rotated

        # **3단계: 업스케일링 (3배)** - 텍스트를 더 크게
        scale_factor = 3.0
        upscaled = cv2.resize(
            gray,
            None,
            fx=scale_factor,
            fy=scale_factor,
            interpolation=cv2.INTER_CUBIC
        )

        # **4단계: 대비 향상 (CLAHE)**
        clahe = cv2.createCLAHE(clipLimit=2.0, tileGridSize=(8, 8))
        enhanced = clahe.apply(upscaled)

        # **5단계: 선명화 (Sharpening)**
        kernel = np.array([[0, -1, 0],
                          [-1, 5, -1],
                          [0, -1, 0]])
        sharpened = cv2.filter2D(enhanced, -1, kernel)

        # PaddleOCR는 그레이스케일도 잘 인식하므로 반환
        # 이진화는 하지 않음 (딥러닝 모델은 그레이스케일/컬러에서 더 잘 작동)
        return sharpened

    def detect_text(self, image: np.ndarray) -> tuple:
        """
        이미지에서 텍스트 검출 (PaddleOCR)

        Args:
            image: 입력 이미지

        Returns:
            (검출된 텍스트, 신뢰도, 전처리된 이미지)
        """
        if self.ocr is None:
            raise RuntimeError("PaddleOCR이 초기화되지 않았습니다")

        try:
            # 전처리
            preprocessed = self.preprocess_image(image)

            # OCR 수행
            # result = [([[x1, y1], [x2, y2], [x3, y3], [x4, y4]], (text, confidence)), ...]
            result = self.ocr.ocr(preprocessed, cls=True)

            if not result or not result[0]:
                logger.warning("[PaddleOCR] 텍스트 검출 실패 (빈 결과)")
                return "", 0.0, preprocessed

            # 검출된 텍스트와 신뢰도 추출
            texts = []
            confidences = []

            for line in result[0]:
                if line and len(line) >= 2:
                    bbox, (text, confidence) = line
                    texts.append(text)
                    confidences.append(confidence)

            # 전체 텍스트 결합
            full_text = ' '.join(texts)

            # 평균 신뢰도 계산
            avg_confidence = sum(confidences) / len(confidences) if confidences else 0.0

            logger.debug(f"[PaddleOCR] 검출 텍스트: '{full_text}' (신뢰도: {avg_confidence:.2%})")

            return full_text, avg_confidence, preprocessed

        except Exception as e:
            logger.error(f"❌ PaddleOCR 실패: {e}", exc_info=True)
            return "", 0.0, preprocessed

    def normalize_serial(self, text: str) -> str:
        """
        시리얼 넘버 정규화

        OCR 오인식 문자 교정:
            - O/o → 0 (알파벳 O를 숫자 0으로)
            - I/l/L → 1 (알파벳 I, l, L을 숫자 1로)
            - Z → 2 (알파벳 Z를 숫자 2로)
            - S → 5 (알파벳 S를 숫자 5로)

        Args:
            text: 원본 텍스트

        Returns:
            정규화된 시리얼 넘버 (숫자만, 8자리)
        """
        # **OCR 오인식 문자 치환** (O/o → 0, I/l → 1)
        # 시리얼 넘버는 숫자만 포함하므로 알파벳을 숫자로 변환
        corrected = text.upper()
        corrected = corrected.replace('O', '0')  # 알파벳 O → 숫자 0
        corrected = corrected.replace('I', '1')  # 알파벳 I → 숫자 1
        corrected = corrected.replace('L', '1')  # 알파벳 L → 숫자 1
        corrected = corrected.replace('Z', '2')  # 알파벳 Z → 숫자 2
        corrected = corrected.replace('S', '5')  # 알파벳 S → 숫자 5

        # 숫자만 추출 (하이픈, 공백 제거)
        digits_only = ''.join(c for c in corrected if c.isdigit())

        # 8자리 시리얼 넘버 추출
        if len(digits_only) >= 8:
            # 가장 긴 연속된 8자리 이상 숫자 찾기
            return digits_only[:8] if len(digits_only) == 8 else digits_only[-8:]
        else:
            return digits_only

    def detect_serial_number(self, image: np.ndarray) -> Dict:
        """
        시리얼 넘버 검출 메인 함수

        Args:
            image: 입력 이미지 (뒷면 PCB 이미지)

        Returns:
            검출 결과 딕셔너리:
            {
                'status': 'ok' 또는 'error',
                'serial_number': 'MBBC-00000001',
                'product_code': 'BC',
                'sequence_number': '00000001',
                'confidence': 0.95,
                'detected_text': '원본 검출 텍스트',
                'preprocessed_image': 전처리된 이미지 (numpy array)
            }
        """
        try:
            # OCR 수행
            detected_text, confidence, preprocessed = self.detect_text(image)

            # /tmp/ocr_debug.jpg에 전처리 이미지 저장 ⭐
            try:
                cv2.imwrite('/tmp/ocr_debug.jpg', preprocessed)
                logger.debug(f"[OCR-DEBUG] 전처리 이미지 저장: /tmp/ocr_debug.jpg (shape: {preprocessed.shape})")
            except Exception as save_err:
                logger.warning(f"[OCR-DEBUG] 이미지 저장 실패: {save_err}")

            if not detected_text:
                return {
                    'status': 'error',
                    'error': 'OCR 텍스트 검출 실패',
                    'confidence': 0.0,
                    'detected_text': '',
                    'preprocessed_image': preprocessed
                }

            logger.info(f"[OCR] 검출 텍스트: '{detected_text}' (신뢰도: {confidence:.2%})")

            # 정규식 패턴 매칭 (여러 패턴 시도)
            patterns = [
                ('SERIAL_PATTERN', self.SERIAL_PATTERN),
                ('SIMPLE_PATTERN', self.SIMPLE_PATTERN),
                ('FLEXIBLE_PATTERN', self.FLEXIBLE_PATTERN),
                ('ULTRA_FLEXIBLE_PATTERN', self.ULTRA_FLEXIBLE_PATTERN)
            ]

            for pattern_name, pattern in patterns:
                match = pattern.search(detected_text)
                if match:
                    product_code = match.group(1).upper()  # BC, FT, RS 등
                    sequence_number_raw = match.group(2)  # 00000001

                    # 시리얼 넘버 정규화 (8자리)
                    sequence_number = self.normalize_serial(sequence_number_raw)

                    # 최종 시리얼 넘버 구성
                    serial_number = f"MB{product_code}-{sequence_number}"

                    logger.info(
                        f"✅ [{pattern_name}] 시리얼 넘버 검출 성공: {serial_number} "
                        f"(제품: {product_code}, 일련번호: {sequence_number}, 신뢰도: {confidence:.2%})"
                    )

                    return {
                        'status': 'ok',
                        'serial_number': serial_number,
                        'product_code': product_code,
                        'sequence_number': sequence_number,
                        'confidence': confidence,
                        'detected_text': detected_text,
                        'preprocessed_image': preprocessed
                    }

            # 모든 패턴 실패
            logger.warning(f"⚠️ 시리얼 넘버 패턴 매칭 실패 (검출 텍스트: '{detected_text}')")
            return {
                'status': 'error',
                'error': f"시리얼 넘버 패턴 미발견 (검출: '{detected_text}')",
                'confidence': confidence,
                'detected_text': detected_text,
                'preprocessed_image': preprocessed
            }

        except Exception as e:
            logger.error(f"❌ 시리얼 넘버 검출 중 예외 발생: {e}", exc_info=True)
            return {
                'status': 'error',
                'error': f"OCR 처리 중 오류: {str(e)}",
                'confidence': 0.0,
                'detected_text': '',
                'preprocessed_image': None
            }


# 테스트 코드
if __name__ == '__main__':
    import sys

    # 로깅 설정
    logging.basicConfig(
        level=logging.DEBUG,
        format='[%(levelname)s] %(message)s'
    )

    # PaddleOCR 검출기 초기화
    detector = SerialNumberDetector(use_gpu=False)  # CPU 모드

    # 테스트 이미지 로드
    if len(sys.argv) > 1:
        image_path = sys.argv[1]
        image = cv2.imread(image_path)

        if image is None:
            print(f"❌ 이미지 로드 실패: {image_path}")
            sys.exit(1)

        # 시리얼 넘버 검출
        result = detector.detect_serial_number(image)

        print("\n" + "=" * 60)
        print("시리얼 넘버 검출 결과:")
        print("=" * 60)
        for key, value in result.items():
            if key != 'preprocessed_image':
                print(f"{key}: {value}")
        print("=" * 60)

        # 전처리 이미지는 이미 /tmp/ocr_debug.jpg에 저장됨
        print("✅ 전처리 이미지 저장: /tmp/ocr_debug.jpg")
    else:
        print("사용법: python serial_number_detector.py <이미지 경로>")
