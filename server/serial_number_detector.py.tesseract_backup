#!/usr/bin/env python3
"""
시리얼 넘버 OCR 검출 모듈 (Tesseract OCR 버전)

기능:
    - Tesseract OCR을 이용한 시리얼 넘버 텍스트 인식
    - 정규식 기반 시리얼 넘버 파싱 (S/N MBXX-00000001 형식)
    - 제품 코드 추출 (MBXX에서 XX 추출)
    - PSM (Page Segmentation Mode) 최적화

예시:
    S/N MBBC-00000001 → 제품 코드: BC
    S/N MBFT-12345678 → 제품 코드: FT
    S/N MBRS-99999999 → 제품 코드: RS

Tesseract 장점:
    - EasyOCR보다 훨씬 빠름 (CPU에서도 실시간 처리 가능)
    - 영어/숫자 인식 정확도 매우 높음
    - PSM 모드로 단일 라인 텍스트 최적화
    - 화이트리스트로 인식 문자 제한 가능
"""

import re
import cv2
import numpy as np
import pytesseract
import logging
from typing import Optional, Tuple, Dict

logger = logging.getLogger(__name__)


class SerialNumberDetector:
    """시리얼 넘버 OCR 검출기 (Tesseract OCR 버전)"""

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

    def __init__(self, psm_mode=7, oem_mode=3, whitelist=None):
        """
        Args:
            psm_mode: Tesseract PSM (Page Segmentation Mode)
                - 3: Fully automatic page segmentation (기본값)
                - 6: Uniform block of text (단일 텍스트 블록)
                - 7: Single text line (단일 라인 - 시리얼 넘버에 최적) ⭐
                - 8: Single word (단일 단어)
                - 13: Raw line (원시 라인 - 매우 빠름)
            oem_mode: OCR Engine Mode
                - 0: Legacy engine only
                - 1: Neural nets LSTM engine only
                - 2: Legacy + LSTM engines
                - 3: Default, based on what is available ⭐
            whitelist: 인식할 문자 화이트리스트 (예: 'ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-/ ')
        """
        self.psm_mode = psm_mode
        self.oem_mode = oem_mode
        self.whitelist = whitelist

        logger.info("🔤 시리얼 넘버 OCR 검출기 초기화 중 (Tesseract OCR 버전)...")
        self._check_tesseract()

    def _check_tesseract(self):
        """Tesseract 설치 확인"""
        try:
            version = pytesseract.get_tesseract_version()
            logger.info(f"✅ Tesseract OCR 초기화 완료")
            logger.info(f"   - 버전: {version}")
            logger.info(f"   - PSM 모드: {self.psm_mode} (7=단일 라인 최적화)")
            logger.info(f"   - OEM 모드: {self.oem_mode} (3=자동 선택)")
            if self.whitelist:
                logger.info(f"   - 화이트리스트: {self.whitelist[:50]}...")
        except Exception as e:
            logger.error(f"❌ Tesseract OCR 초기화 실패: {e}")
            logger.error("   - 해결 방법: sudo apt install tesseract-ocr tesseract-ocr-eng")
            raise

    def preprocess_image(self, image: np.ndarray) -> np.ndarray:
        """
        OCR 전처리 (90도 회전 + 업스케일링 + CLAHE + 선명화 + 이진화)

        Tesseract는 이진화된 이미지에서 가장 잘 작동함

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

        # **6단계: Otsu 이진화** (Tesseract는 이진화 이미지에서 가장 잘 작동)
        # - 흰색 배경에 검은색 텍스트로 변환
        _, binary = cv2.threshold(sharpened, 0, 255, cv2.THRESH_BINARY + cv2.THRESH_OTSU)

        # **7단계: 노이즈 제거** (Morphological Opening)
        morph_kernel = cv2.getStructuringElement(cv2.MORPH_RECT, (2, 2))
        denoised = cv2.morphologyEx(binary, cv2.MORPH_OPEN, morph_kernel, iterations=1)

        return denoised

    def detect_text(self, image: np.ndarray) -> tuple:
        """
        이미지에서 텍스트 검출 (Tesseract OCR)

        Args:
            image: 입력 이미지

        Returns:
            (검출된 텍스트, 신뢰도, 전처리된 이미지)
        """
        try:
            # 전처리
            preprocessed = self.preprocess_image(image)

            # Tesseract 설정
            config = f'--oem {self.oem_mode} --psm {self.psm_mode}'

            # 화이트리스트 추가 (영어 대문자, 숫자, 특수문자만 인식)
            if self.whitelist:
                config += f' -c tessedit_char_whitelist={self.whitelist}'
            else:
                # 기본 화이트리스트: 영어 대문자, 숫자, 하이픈, 슬래시, 공백
                config += ' -c tessedit_char_whitelist=ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-/: '

            # OCR 수행 (텍스트 + 신뢰도)
            data = pytesseract.image_to_data(
                preprocessed,
                config=config,
                output_type=pytesseract.Output.DICT
            )

            # 검출된 텍스트 결합
            texts = []
            confidences = []
            for i, text in enumerate(data['text']):
                if text.strip():  # 빈 문자열 제외
                    texts.append(text)
                    confidences.append(data['conf'][i])

            # 전체 텍스트 결합
            full_text = ' '.join(texts)

            # 평균 신뢰도 계산
            avg_confidence = sum(confidences) / len(confidences) if confidences else 0.0

            logger.debug(f"[Tesseract] 검출 텍스트: '{full_text}' (신뢰도: {avg_confidence:.2f})")

            return full_text, avg_confidence / 100.0, preprocessed  # 신뢰도를 0~1 범위로 변환

        except Exception as e:
            logger.error(f"❌ Tesseract OCR 실패: {e}")
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

    # Tesseract 검출기 초기화
    detector = SerialNumberDetector(psm_mode=7)  # 단일 라인 모드

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

        # 전처리 이미지 저장
        if result.get('preprocessed_image') is not None:
            cv2.imwrite('/tmp/tesseract_preprocessed.jpg', result['preprocessed_image'])
            print("✅ 전처리 이미지 저장: /tmp/tesseract_preprocessed.jpg")
    else:
        print("사용법: python serial_number_detector.py <이미지 경로>")
