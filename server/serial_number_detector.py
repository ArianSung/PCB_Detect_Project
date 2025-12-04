#!/usr/bin/env python3
"""
시리얼 넘버 OCR 검출 모듈

기능:
    - PaddleOCR을 이용한 시리얼 넘버 텍스트 인식 (EasyOCR 대비 빠르고 정확)
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
from paddleocr import PaddleOCR
import logging
from typing import Optional, Tuple, Dict

logger = logging.getLogger(__name__)


class SerialNumberDetector:
    """시리얼 넘버 OCR 검출기 (PaddleOCR 사용)"""

    # 시리얼 넘버 정규식 패턴
    # 형식: S/N MBXX-00000001 (XX는 2자리 제품 코드, 00000001은 8자리 일련번호)
    SERIAL_PATTERN = re.compile(
        r'S/N\s*MB([A-Z]{2})-(\d{8})',
        re.IGNORECASE
    )

    # 간단한 패턴 (S/N 없이 MBXX-00000001만)
    SIMPLE_PATTERN = re.compile(
        r'MB([A-Z]{2})-(\d{8})',
        re.IGNORECASE
    )

    def __init__(self, lang='en', use_gpu=True, min_confidence=0.5):
        """
        Args:
            lang: OCR 언어 설정 (기본: 영어)
            use_gpu: GPU 사용 여부
            min_confidence: 최소 신뢰도 임계값
        """
        self.lang = lang
        self.use_gpu = use_gpu
        self.min_confidence = min_confidence
        self.ocr = None

        logger.info("🔤 시리얼 넘버 OCR 검출기 초기화 중 (PaddleOCR)...")
        self._initialize_reader()

    def _initialize_reader(self):
        """PaddleOCR Reader 초기화"""
        try:
            self.ocr = PaddleOCR(
                use_angle_cls=True,  # 텍스트 회전 보정 활성화
                lang=self.lang,
                use_gpu=self.use_gpu,
                show_log=False,
                det_db_box_thresh=0.3,  # 텍스트 박스 검출 임계값 (낮춤)
                det_db_unclip_ratio=2.0,  # 텍스트 박스 확장 비율 (늘림)
                rec_batch_num=6  # 배치 크기
            )
            logger.info(f"✅ PaddleOCR Reader 초기화 완료 (언어: {self.lang}, GPU: {self.use_gpu})")
            logger.info(f"   - 성능: EasyOCR 대비 2-3배 빠름, 정확도 향상")
        except Exception as e:
            logger.error(f"❌ PaddleOCR Reader 초기화 실패: {e}")
            raise

    def preprocess_image(self, image: np.ndarray) -> np.ndarray:
        """
        OCR 전처리 (90도 회전 + ROI 추출 + 업스케일링)

        Args:
            image: 입력 이미지

        Returns:
            전처리된 이미지
        """
        # **1단계: 오른쪽으로 90도 회전** (시리얼 넘버가 옆으로 돌아가 있음)
        rotated = cv2.rotate(image, cv2.ROTATE_90_CLOCKWISE)

        # **2단계: ROI 추출 (상단 30% 영역만)** - 시리얼 넘버가 상단에 있음
        height, width = rotated.shape[:2]
        roi_height = int(height * 0.3)  # 상단 30%
        roi = rotated[0:roi_height, :]  # 상단 영역만 잘라냄

        # **3단계: 업스케일링 (2배 확대)** - 텍스트를 더 크게
        scale_factor = 2.0
        roi_upscaled = cv2.resize(
            roi,
            None,
            fx=scale_factor,
            fy=scale_factor,
            interpolation=cv2.INTER_CUBIC
        )

        # **4단계: 그레이스케일 변환**
        if len(roi_upscaled.shape) == 3:
            gray = cv2.cvtColor(roi_upscaled, cv2.COLOR_BGR2GRAY)
        else:
            gray = roi_upscaled

        # **5단계: 대비 향상 (CLAHE) - 약하게 적용**
        clahe = cv2.createCLAHE(clipLimit=1.5, tileGridSize=(8, 8))
        enhanced = clahe.apply(gray)

        # **6단계: 적응형 이진화 추가 (PaddleOCR는 이진화 이미지에서도 잘 작동)**
        # 가우시안 블러로 노이즈 제거 후 이진화
        blurred = cv2.GaussianBlur(enhanced, (3, 3), 0)
        binary = cv2.adaptiveThreshold(
            blurred,
            255,
            cv2.ADAPTIVE_THRESH_GAUSSIAN_C,
            cv2.THRESH_BINARY,
            11,
            2
        )

        return binary

    def detect_text(self, image: np.ndarray) -> list:
        """
        이미지에서 텍스트 검출 (PaddleOCR)

        Args:
            image: 입력 이미지

        Returns:
            검출된 텍스트 리스트 [(bbox, text, confidence), ...]
        """
        if self.ocr is None:
            raise RuntimeError("PaddleOCR Reader가 초기화되지 않았습니다")

        try:
            # 전처리
            preprocessed = self.preprocess_image(image)

            # OCR 수행
            # PaddleOCR 반환 형식: [line_result, ...]
            # line_result = [bbox, (text, confidence)]
            results = self.ocr.ocr(preprocessed, cls=True)

            # 결과가 None이거나 비어있는 경우 처리
            if not results or results[0] is None:
                logger.warning("⚠️  PaddleOCR이 아무 텍스트도 검출하지 못했습니다")
                return []

            # PaddleOCR 결과를 EasyOCR 형식으로 변환
            converted_results = []
            for line in results[0]:
                bbox = line[0]  # [[x1,y1], [x2,y2], [x3,y3], [x4,y4]]
                text = line[1][0]  # 텍스트
                conf = line[1][1]  # 신뢰도
                converted_results.append((bbox, text, conf))

            # 디버그: 모든 검출 결과 로깅 (신뢰도 무관)
            if converted_results:
                logger.info(f"🔍 PaddleOCR 원본 결과 (총 {len(converted_results)}개):")
                for bbox, text, conf in converted_results:
                    logger.info(f"   - 텍스트: '{text}' | 신뢰도: {conf:.2%}")
            else:
                logger.warning("⚠️  PaddleOCR이 아무 텍스트도 검출하지 못했습니다")

            # 신뢰도 필터링
            filtered_results = [
                (bbox, text, conf)
                for bbox, text, conf in converted_results
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
        시리얼 넘버 파싱

        Args:
            text: OCR로 검출된 텍스트

        Returns:
            (전체 시리얼 넘버, 제품 코드, 일련번호) 또는 None

        예시:
            "S/N MBBC-00000001" → ("MBBC-00000001", "BC", "00000001")
            "MBFT-12345678" → ("MBFT-12345678", "FT", "12345678")
        """
        # 우선 전체 패턴 매칭 (S/N 포함)
        match = self.SERIAL_PATTERN.search(text)
        if match:
            product_code = match.group(1).upper()  # BC, FT, RS 등
            serial_num = match.group(2)  # 8자리 숫자
            full_serial = f"MB{product_code}-{serial_num}"
            return (full_serial, product_code, serial_num)

        # 간단한 패턴 매칭 (S/N 없이)
        match = self.SIMPLE_PATTERN.search(text)
        if match:
            product_code = match.group(1).upper()
            serial_num = match.group(2)
            full_serial = f"MB{product_code}-{serial_num}"
            return (full_serial, product_code, serial_num)

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
    detector = SerialNumberDetector(use_gpu=True)

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
        print("시리얼 넘버 검출 결과 (PaddleOCR)")
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
