#!/usr/bin/env python3
"""
시리얼 넘버 OCR 검출 모듈

기능:
    - EasyOCR을 이용한 시리얼 넘버 텍스트 인식 (더 큰 모델 사용)
    - 정규식 기반 시리얼 넘버 파싱 (S/N MBXX-00000001 형식)
    - 제품 코드 추출 (MBXX에서 XX 추출)
    - 신뢰도 기반 검증

예시:
    S/N MBBC-00000001 → 제품 코드: BC
    S/N MBFT-12345678 → 제품 코드: FT
    S/N MBRS-99999999 → 제품 코드: RS
"""

import re
import cv2
import numpy as np
import easyocr
import logging
from typing import Optional, Tuple, Dict

logger = logging.getLogger(__name__)


class SerialNumberDetector:
    """시리얼 넘버 OCR 검출기 (EasyOCR 개선 버전)"""

    # 시리얼 넘버 정규식 패턴 (숫자 개수 유연하게 - 6~10자리)
    # 형식: S/N MBXX-00000001 (XX는 2자리 제품 코드, 일련번호는 6~10자리)
    SERIAL_PATTERN = re.compile(
        r'S[/\\]?N[\s:]*MB([A-Z]{2})[\s-]*(\d{6,10})',
        re.IGNORECASE
    )

    # 간단한 패턴 (S/N 없이, 숫자 6~10자리)
    SIMPLE_PATTERN = re.compile(
        r'MB([A-Z]{2})[\s-]*(\d{6,10})',
        re.IGNORECASE
    )

    # 더 유연한 패턴 (숫자 6~10자리, 구분자 관대)
    FLEXIBLE_PATTERN = re.compile(
        r'MB[\s]*([A-Z]{2})[\s\-_:]*(\d{6,10})',
        re.IGNORECASE
    )

    # 초완화 패턴 (숫자만 4자리 이상이면 일단 허용)
    ULTRA_FLEXIBLE_PATTERN = re.compile(
        r'MB[\s]*([A-Z]{2})[\s\-_:]*(\d{4,12})',
        re.IGNORECASE
    )

    def __init__(self, languages=['en'], gpu=True, min_confidence=0.01,
                 detector='craft', recognizer='english_g2'):
        """
        Args:
            languages: OCR 언어 설정 (기본: 영어)
            gpu: GPU 사용 여부
            min_confidence: 최소 신뢰도 임계값
            detector: 텍스트 검출 모델 ('craft' 또는 'dbnet18' - craft가 더 정확)
            recognizer: 텍스트 인식 모델 ('english_g2'가 기본보다 더 정확)
        """
        self.languages = languages
        self.gpu = gpu
        self.min_confidence = min_confidence
        self.detector = detector
        self.recognizer = recognizer
        self.reader = None

        logger.info("🔤 시리얼 넘버 OCR 검출기 초기화 중 (EasyOCR 개선 버전)...")
        self._initialize_reader()

    def _initialize_reader(self):
        """EasyOCR Reader 초기화 (더 큰 모델 사용)"""
        try:
            # EasyOCR Reader 초기화
            # detector=True, recognizer=True로 커스텀 모델 사용
            # english_g2는 기본 모델보다 더 정확함
            self.reader = easyocr.Reader(
                lang_list=self.languages,
                gpu=self.gpu,
                verbose=False,
                detector=True,  # CRAFT 검출기 사용 (더 정확)
                recognizer=True,  # 더 큰 인식 모델 사용
                model_storage_directory='~/.EasyOCR/model',
                download_enabled=True
            )
            logger.info(f"✅ EasyOCR Reader 초기화 완료")
            logger.info(f"   - 언어: {self.languages}")
            logger.info(f"   - GPU: {self.gpu}")
            logger.info(f"   - 검출기: CRAFT (고성능)")
            logger.info(f"   - 인식기: 기본 영문 모델 (정확도 향상)")
        except Exception as e:
            logger.error(f"❌ EasyOCR Reader 초기화 실패: {e}")
            raise

    def preprocess_image(self, image: np.ndarray) -> np.ndarray:
        """
        OCR 전처리 (90도 회전 + 최소 전처리)

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

        # **3단계: 업스케일링 (2배)** - 텍스트를 더 크게
        scale_factor = 2.0
        upscaled = cv2.resize(
            gray,
            None,
            fx=scale_factor,
            fy=scale_factor,
            interpolation=cv2.INTER_CUBIC
        )

        return upscaled

    def detect_text(self, image: np.ndarray) -> list:
        """
        이미지에서 텍스트 검출 (EasyOCR)

        Args:
            image: 입력 이미지

        Returns:
            검출된 텍스트 리스트 [(bbox, text, confidence), ...]
        """
        if self.reader is None:
            raise RuntimeError("EasyOCR Reader가 초기화되지 않았습니다")

        try:
            # 전처리
            preprocessed = self.preprocess_image(image)

            # OCR 수행 (관대한 임계값으로 최대한 많이 검출)
            results = self.reader.readtext(
                preprocessed,
                detail=1,  # 상세 정보 포함
                paragraph=False,  # 단어 단위로 검출
                min_size=5,  # 최소 텍스트 크기 (10 → 5 픽셀)
                text_threshold=0.5,  # 텍스트 신뢰도 임계값 (0.6 → 0.5)
                low_text=0.2,  # 낮은 텍스트 점수 (0.3 → 0.2)
                link_threshold=0.2,  # 링크 임계값 (0.3 → 0.2)
                canvas_size=2560,  # 최대 이미지 크기
                mag_ratio=1.5,  # 확대 비율
                width_ths=0.5,  # 텍스트 너비 임계값 (더 관대하게)
                add_margin=0.1  # 텍스트 주변 마진
            )

            # 디버그: 모든 검출 결과 로깅 (신뢰도 무관)
            if results:
                logger.info(f"🔍 EasyOCR 원본 결과 (총 {len(results)}개):")
                for bbox, text, conf in results:
                    logger.info(f"   - 텍스트: '{text}' | 신뢰도: {conf:.2%}")
            else:
                logger.warning("⚠️  EasyOCR이 아무 텍스트도 검출하지 못했습니다")

            # 신뢰도 필터링
            filtered_results = [
                (bbox, text, conf)
                for bbox, text, conf in results
                if conf >= self.min_confidence
            ]

            if filtered_results:
                logger.info(f"✅ 신뢰도 필터링 후: {len(filtered_results)}개 유지 (임계값: {self.min_confidence:.0%})")
            else:
                logger.warning(f"⚠️  신뢰도 {self.min_confidence:.0%} 이상인 텍스트가 없습니다")

            return filtered_results

        except Exception as e:
            logger.error(f"텍스트 검출 실패: {e}")
            return []

    def parse_serial_number(self, text: str) -> Optional[Tuple[str, str, str]]:
        """
        시리얼 넘버 파싱 (4가지 패턴 시도 + 숫자 보정)

        Args:
            text: OCR로 검출된 텍스트

        Returns:
            (전체 시리얼 넘버, 제품 코드, 일련번호) 또는 None

        예시:
            "S/N MBBC-00000001" → ("MBBC-00000001", "BC", "00000001")
            "MBFT-12345678" → ("MBFT-12345678", "FT", "12345678")
            "MBBC 123456" → ("MBBC-00123456", "BC", "00123456") (6자리 → 8자리 보정)
        """
        def normalize_serial(serial_num: str) -> str:
            """일련번호를 8자리로 보정 (앞에 0 추가 또는 뒤에서 8자리만 사용)"""
            if len(serial_num) < 8:
                # 8자리보다 짧으면 앞에 0 추가
                return serial_num.zfill(8)
            elif len(serial_num) > 8:
                # 8자리보다 길면 뒤에서 8자리만 사용
                logger.warning(f"⚠️  일련번호가 8자리보다 김: {serial_num} → {serial_num[-8:]}")
                return serial_num[-8:]
            return serial_num

        # 1. 전체 패턴 매칭 (S/N 포함)
        match = self.SERIAL_PATTERN.search(text)
        if match:
            product_code = match.group(1).upper()
            serial_num = normalize_serial(match.group(2))
            full_serial = f"MB{product_code}-{serial_num}"
            logger.info(f"✅ SERIAL_PATTERN 매칭: {full_serial} (원본: {match.group(2)})")
            return (full_serial, product_code, serial_num)

        # 2. 간단한 패턴 매칭 (S/N 없이)
        match = self.SIMPLE_PATTERN.search(text)
        if match:
            product_code = match.group(1).upper()
            serial_num = normalize_serial(match.group(2))
            full_serial = f"MB{product_code}-{serial_num}"
            logger.info(f"✅ SIMPLE_PATTERN 매칭: {full_serial} (원본: {match.group(2)})")
            return (full_serial, product_code, serial_num)

        # 3. 유연한 패턴 매칭
        match = self.FLEXIBLE_PATTERN.search(text)
        if match:
            product_code = match.group(1).upper()
            serial_num = normalize_serial(match.group(2))
            full_serial = f"MB{product_code}-{serial_num}"
            logger.info(f"✅ FLEXIBLE_PATTERN 매칭: {full_serial} (원본: {match.group(2)})")
            return (full_serial, product_code, serial_num)

        # 4. 초완화 패턴 매칭 (4~12자리 숫자)
        match = self.ULTRA_FLEXIBLE_PATTERN.search(text)
        if match:
            product_code = match.group(1).upper()
            serial_num = normalize_serial(match.group(2))
            full_serial = f"MB{product_code}-{serial_num}"
            logger.info(f"✅ ULTRA_FLEXIBLE_PATTERN 매칭: {full_serial} (원본: {match.group(2)})")
            return (full_serial, product_code, serial_num)

        logger.warning(f"⚠️  모든 패턴 매칭 실패. 원본 텍스트: '{text}'")
        return None

    def detect_serial_number(self, image: np.ndarray) -> Dict:
        """
        이미지에서 시리얼 넘버 검출 및 제품 코드 추출

        Args:
            image: 입력 이미지 (BGR 또는 Gray)

        Returns:
            검출 결과 딕셔너리
            {
                'status': 'ok' or 'error',
                'serial_number': 전체 시리얼 넘버 (MBBC-00000001),
                'product_code': 제품 코드 (BC),
                'sequence_number': 일련번호 (00000001),
                'confidence': OCR 신뢰도,
                'detected_text': 원본 OCR 텍스트,
                'error': 에러 메시지 (실패 시)
            }
        """
        try:
            # 텍스트 검출
            ocr_results = self.detect_text(image)

            if not ocr_results:
                return {
                    'status': 'error',
                    'error': '텍스트를 검출할 수 없습니다',
                    'serial_number': None,
                    'product_code': None,
                    'confidence': 0.0
                }

            # 검출된 모든 텍스트를 합쳐서 파싱
            all_text = ' '.join([text for _, text, _ in ocr_results])
            logger.info(f"검출된 텍스트: {all_text}")

            # 시리얼 넘버 파싱
            parsed = self.parse_serial_number(all_text)

            if parsed is None:
                return {
                    'status': 'error',
                    'error': '시리얼 넘버 형식을 찾을 수 없습니다',
                    'serial_number': None,
                    'product_code': None,
                    'confidence': 0.0,
                    'detected_text': all_text
                }

            full_serial, product_code, sequence_number = parsed

            # 평균 신뢰도 계산
            avg_confidence = np.mean([conf for _, _, conf in ocr_results])

            logger.info(
                f"✅ 시리얼 넘버 검출 성공: {full_serial} "
                f"(제품 코드: {product_code}, 신뢰도: {avg_confidence:.2%})"
            )

            return {
                'status': 'ok',
                'serial_number': full_serial,
                'product_code': product_code,
                'sequence_number': sequence_number,
                'confidence': float(avg_confidence),
                'detected_text': all_text
            }

        except Exception as e:
            logger.error(f"❌ 시리얼 넘버 검출 실패: {e}", exc_info=True)
            return {
                'status': 'error',
                'error': str(e),
                'serial_number': None,
                'product_code': None,
                'confidence': 0.0
            }


# 테스트 코드
if __name__ == '__main__':
    import sys

    logging.basicConfig(
        level=logging.INFO,
        format='[%(levelname)s] %(message)s'
    )

    # 검출기 초기화
    detector = SerialNumberDetector(gpu=True)

    # 테스트 이미지 경로
    if len(sys.argv) > 1:
        test_image_path = sys.argv[1]

        # 이미지 로드
        image = cv2.imread(test_image_path)
        if image is None:
            print(f"❌ 이미지를 읽을 수 없습니다: {test_image_path}")
            sys.exit(1)

        # 시리얼 넘버 검출
        result = detector.detect_serial_number(image)

        # 결과 출력
        print("\n" + "=" * 60)
        print("시리얼 넘버 검출 결과 (EasyOCR 개선)")
        print("=" * 60)
        print(f"상태: {result['status']}")
        if result['status'] == 'ok':
            print(f"시리얼 넘버: {result['serial_number']}")
            print(f"제품 코드: {result['product_code']}")
            print(f"일련번호: {result['sequence_number']}")
            print(f"신뢰도: {result['confidence']:.2%}")
            print(f"검출된 텍스트: {result['detected_text']}")
        else:
            print(f"에러: {result['error']}")
            if 'detected_text' in result:
                print(f"검출된 텍스트: {result['detected_text']}")
        print("=" * 60)
    else:
        print("사용법: python serial_number_detector.py <이미지 경로>")
