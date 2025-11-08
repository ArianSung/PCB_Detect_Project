# PCB 일련번호 검출 가이드 (YOLO + OCR)

## 개요

PCB 앞면에서 일련번호를 자동으로 검출하고 인식하는 시스템입니다.

**2단계 접근법**:
1. **YOLO**: 일련번호 영역(ROI) 검출 → bbox 좌표 반환
2. **OCR**: YOLO가 찾은 영역 내의 텍스트 인식 → 일련번호 문자열 반환

---

## 왜 YOLO + OCR 조합인가?

### ❌ OCR만 사용하는 경우 (문제점)

```
전체 PCB 이미지
├── "R1" (저항 라벨)
├── "IC1" (칩 라벨)
├── "PCB-2025-001234" ← 일련번호 (찾아야 함)
├── "3.3V" (전압 표시)
└── "GND" (접지 표시)
```

**문제**: OCR이 모든 텍스트를 인식 → 어떤 것이 일련번호인지 구분 어려움
**해결**: YOLO로 일련번호 영역만 먼저 찾기!

### ✅ YOLO + OCR 조합 (해결책)

```
YOLO 단계:
전체 이미지 → YOLO → [일련번호 영역 bbox 검출]
                         ↓
                    x1=100, y1=50
                    x2=300, y2=80

OCR 단계:
일련번호 영역만 잘라내기 → OCR → "PCB-2025-001234"
```

**장점**:
- OCR이 일련번호 영역만 보기 때문에 정확도 ↑↑
- 다른 텍스트(R1, IC1 등)에 혼동되지 않음
- 배경 노이즈 최소화

---

## 1단계: 데이터셋 준비 및 라벨링 ⭐

### 1.1. 이미지 수집

**필요 이미지 수**: 최소 **100-300장** (일련번호가 같은 위치에 있으므로 적은 수로도 충분)

**수집 방법**:
```bash
# 프로젝트 디렉토리
/home/sys1041/work_project/data/raw/serial_number_detection/
├── train/
│   ├── images/       # 학습 이미지 (200장)
│   └── labels/       # YOLO 라벨 (200개 txt 파일)
├── valid/
│   ├── images/       # 검증 이미지 (50장)
│   └── labels/       # YOLO 라벨 (50개 txt 파일)
└── test/
    ├── images/       # 테스트 이미지 (50장)
    └── labels/       # YOLO 라벨 (50개 txt 파일)
```

### 1.2. 라벨링 도구 설치

**LabelImg** 사용 (YOLO 포맷 지원):
```bash
conda activate pcb_defect

# LabelImg 설치
pip install labelImg

# 실행
labelImg
```

**Roboflow** 사용 (웹 기반, 더 편리):
- https://roboflow.com/
- 무료 계정 생성
- YOLO 포맷으로 Export 가능

### 1.3. 라벨링 방법 ⭐⭐⭐

**중요**: 일련번호 **영역(텍스트가 있는 박스)**만 라벨링하면 됩니다!

#### LabelImg에서 라벨링 순서:

1. **이미지 열기**: `Open Dir` → `data/raw/serial_number_detection/train/images` 선택
2. **저장 경로 설정**: `Change Save Dir` → `data/raw/serial_number_detection/train/labels` 선택
3. **YOLO 포맷 선택**: 왼쪽 메뉴에서 `PascalVOC` 클릭 → `YOLO` 선택
4. **클래스 정의**: `View` → `Create Classes` → `serial_number` 입력

5. **박스 그리기**:
   - 단축키 `W` 누르기
   - 일련번호가 있는 영역을 마우스로 드래그하여 박스 그리기
   - **텍스트 전체를 포함**하도록 박스 크기 조정

   ```
   예시:
   ┌──────────────────────────────┐
   │                              │
   │  [박스 그리기]               │
   │  ┌─────────────────┐        │
   │  │ PCB-2025-001234 │  ← 이 영역만 선택!
   │  └─────────────────┘        │
   │                              │
   │  R1  R2  IC1                 │  ← 이런 텍스트는 무시
   │                              │
   └──────────────────────────────┘
   ```

6. **클래스 선택**: 박스를 그린 후 `serial_number` 클래스 선택
7. **저장**: `Ctrl + S` (자동으로 YOLO 포맷으로 저장됨)
8. **다음 이미지**: `D` 키 → 반복

#### 라벨 파일 형식 (YOLO)

**파일 구조**:
```
train/
├── images/
│   ├── pcb_001.jpg
│   ├── pcb_002.jpg
│   └── ...
└── labels/
    ├── pcb_001.txt
    ├── pcb_002.txt
    └── ...
```

**pcb_001.txt 내용** (YOLO 포맷):
```
0 0.5 0.15 0.4 0.08
```

**형식 설명**:
```
class_id  center_x  center_y  width  height
   ↓         ↓         ↓        ↓       ↓
   0       0.5       0.15     0.4     0.08

- class_id: 0 (serial_number 클래스, 1개만 있으므로 항상 0)
- center_x: 박스 중심 X 좌표 (이미지 너비 대비 비율, 0.0~1.0)
- center_y: 박스 중심 Y 좌표 (이미지 높이 대비 비율, 0.0~1.0)
- width: 박스 너비 (이미지 너비 대비 비율)
- height: 박스 높이 (이미지 높이 대비 비율)
```

**예시**:
- 이미지 크기: 640 × 480
- 일련번호 위치: (200, 50) ~ (400, 100)

계산:
```python
center_x = ((200 + 400) / 2) / 640 = 0.46875
center_y = ((50 + 100) / 2) / 480 = 0.15625
width = (400 - 200) / 640 = 0.3125
height = (100 - 50) / 480 = 0.10417

# pcb_001.txt 내용:
# 0 0.46875 0.15625 0.3125 0.10417
```

### 1.4. data.yaml 생성

**data.yaml** (YOLO 학습 설정 파일):
```yaml
# 경로
path: /home/sys1041/work_project/data/raw/serial_number_detection
train: train/images
val: valid/images
test: test/images

# 클래스
nc: 1  # 클래스 개수 (serial_number 1개만)
names: ['serial_number']  # 클래스 이름
```

---

## 2단계: YOLO 모델 학습

### 2.1. 학습 스크립트 작성

**scripts/train_serial_number_detector.py**:
```python
#!/usr/bin/env python3
"""
일련번호 영역 검출 YOLO 모델 학습
"""

from ultralytics import YOLO

def train_serial_number_detector():
    """일련번호 검출 YOLO 모델 학습"""

    # YOLOv11n 사용 (Nano - 가볍고 빠름, 일련번호는 1개 클래스만 있으므로 충분)
    model = YOLO('yolo11n.pt')  # Nano 모델 (가장 가벼움)

    # 학습 설정
    results = model.train(
        data='/home/sys1041/work_project/data/raw/serial_number_detection/data.yaml',
        epochs=50,           # 50 에폭 (데이터 적고 위치 고정이므로 적은 에폭으로 충분)
        batch=16,
        imgsz=640,
        device=0,            # GPU 0

        # 파일명
        project='runs/detect',
        name='serial_number_detector',
        exist_ok=True,

        # 최적화
        optimizer='AdamW',
        lr0=0.001,
        lrf=0.01,
        weight_decay=0.0005,
        patience=20,         # Early stopping

        # 기타
        amp=True,            # Mixed Precision
        verbose=True,
        plots=True,
        cache=False,
        workers=8,

        # 데이터 증강 (위치가 고정이므로 증강 최소화)
        degrees=5,           # 회전 ±5도만
        translate=0.05,      # 이동 5%만
        scale=0.2,           # 크기 조정 최소
        flipud=0.0,          # 상하 반전 안 함
        fliplr=0.0,          # 좌우 반전 안 함 (일련번호는 항상 정방향)
        mosaic=0.0,          # Mosaic 사용 안 함
        mixup=0.0,           # MixUp 사용 안 함
    )

    print("\n" + "="*80)
    print("일련번호 검출 모델 학습 완료!")
    print("="*80)
    print(f"Best 모델: runs/detect/serial_number_detector/weights/best.pt")
    print(f"Last 모델: runs/detect/serial_number_detector/weights/last.pt")
    print("="*80)

    return results

if __name__ == "__main__":
    train_serial_number_detector()
```

### 2.2. 학습 실행

```bash
conda activate pcb_defect
python scripts/train_serial_number_detector.py
```

**예상 학습 시간**:
- 데이터셋: 200장
- 에폭: 50
- RTX 4080 Super: **약 5-10분** (매우 빠름!)

**왜 빠른가?**:
- 클래스 1개만 (serial_number)
- YOLOv11n (Nano 모델 - 가벼움)
- 데이터 적음 (200장)
- 증강 최소화

---

## 3단계: YOLO + OCR 통합 코드

### 3.1. 일련번호 검출기 (YOLO + OCR)

**server/serial_number_detector.py**:
```python
#!/usr/bin/env python3
"""
YOLO + OCR 기반 일련번호 검출 시스템
"""

import cv2
import numpy as np
import easyocr
import re
from ultralytics import YOLO
from pathlib import Path

class SerialNumberDetector:
    """YOLO + OCR 기반 일련번호 검출기"""

    def __init__(self, yolo_model_path, languages=['en']):
        """
        Args:
            yolo_model_path: YOLO 모델 경로
            languages: OCR 언어 (기본: 영어)
        """
        # YOLO 모델 로드 (일련번호 영역 검출)
        self.yolo_model = YOLO(yolo_model_path)

        # EasyOCR 리더 초기화 (텍스트 인식)
        self.ocr_reader = easyocr.Reader(languages, gpu=True)

        print(f"✓ YOLO 모델 로드: {yolo_model_path}")
        print(f"✓ OCR 리더 초기화: {languages}")

    def detect(self, image, conf_threshold=0.5, ocr_threshold=0.7):
        """
        일련번호 검출 및 인식

        Args:
            image: OpenCV 이미지 (BGR) 또는 numpy 배열
            conf_threshold: YOLO 신뢰도 임계값 (기본: 0.5)
            ocr_threshold: OCR 신뢰도 임계값 (기본: 0.7)

        Returns:
            dict: {
                'serial_number': str,     # 인식된 일련번호
                'confidence': float,      # OCR 신뢰도
                'yolo_bbox': list,        # YOLO bbox [x1, y1, x2, y2]
                'yolo_confidence': float, # YOLO 신뢰도
                'success': bool           # 성공 여부
            }
        """
        # 1단계: YOLO로 일련번호 영역 검출
        yolo_results = self.yolo_model.predict(image, conf=conf_threshold, verbose=False)

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
        yolo_conf = float(best_box.conf[0])

        x1, y1, x2, y2 = bbox

        # 2단계: ROI 추출 (일련번호 영역만 잘라내기)
        roi = image[y1:y2, x1:x2]

        if roi.size == 0:
            return {
                'serial_number': None,
                'confidence': 0.0,
                'yolo_bbox': bbox.tolist(),
                'yolo_confidence': yolo_conf,
                'success': False,
                'error': 'ROI 크기가 0입니다.'
            }

        # 3단계: OCR로 텍스트 인식
        ocr_results = self.ocr_reader.readtext(roi, detail=1)

        if len(ocr_results) == 0:
            return {
                'serial_number': None,
                'confidence': 0.0,
                'yolo_bbox': bbox.tolist(),
                'yolo_confidence': yolo_conf,
                'success': False,
                'error': 'OCR이 텍스트를 인식하지 못했습니다.'
            }

        # 가장 신뢰도 높은 텍스트 선택
        best_text = max(ocr_results, key=lambda x: x[2])  # (bbox, text, confidence)
        serial_number = best_text[1]
        ocr_conf = best_text[2]

        # 4단계: 일련번호 유효성 검사 (선택 사항)
        if not self.is_valid_serial_number(serial_number):
            return {
                'serial_number': serial_number,
                'confidence': ocr_conf,
                'yolo_bbox': bbox.tolist(),
                'yolo_confidence': yolo_conf,
                'success': False,
                'error': f'유효하지 않은 일련번호 형식: {serial_number}'
            }

        # 5단계: 신뢰도 체크
        if ocr_conf < ocr_threshold:
            return {
                'serial_number': serial_number,
                'confidence': ocr_conf,
                'yolo_bbox': bbox.tolist(),
                'yolo_confidence': yolo_conf,
                'success': False,
                'error': f'OCR 신뢰도가 낮습니다 ({ocr_conf:.3f} < {ocr_threshold})'
            }

        # 성공!
        return {
            'serial_number': serial_number.strip(),
            'confidence': ocr_conf,
            'yolo_bbox': bbox.tolist(),
            'yolo_confidence': yolo_conf,
            'success': True
        }

    def is_valid_serial_number(self, text):
        """
        일련번호 유효성 검사 (정규식)

        Args:
            text: 인식된 텍스트

        Returns:
            bool: 유효한 일련번호인지 여부
        """
        # 예시 패턴: PCB-2025-001234 또는 PCB2025001234
        # 실제 프로젝트에 맞게 패턴 수정 필요
        patterns = [
            r'^PCB[-_]?\d{4}[-_]?\d{6}$',  # PCB-2025-001234
            r'^[A-Z]{2,4}\d{6,12}$',        # ABC123456789
            r'^\d{10,15}$',                 # 1234567890123
        ]

        for pattern in patterns:
            if re.match(pattern, text, re.IGNORECASE):
                return True

        # 패턴 매칭 실패 시, 길이만 체크 (최소 6자 이상)
        return len(text) >= 6

    def visualize(self, image, result):
        """
        검출 결과 시각화

        Args:
            image: 원본 이미지
            result: detect() 반환값

        Returns:
            numpy.ndarray: 시각화된 이미지
        """
        vis_image = image.copy()

        if result['yolo_bbox'] is not None:
            x1, y1, x2, y2 = result['yolo_bbox']

            # 박스 그리기
            color = (0, 255, 0) if result['success'] else (0, 0, 255)
            cv2.rectangle(vis_image, (x1, y1), (x2, y2), color, 2)

            # 텍스트 표시
            if result['serial_number']:
                label = f"{result['serial_number']} ({result['confidence']:.2f})"
                cv2.putText(vis_image, label, (x1, y1 - 10),
                           cv2.FONT_HERSHEY_SIMPLEX, 0.6, color, 2)

        return vis_image


# ================================================================
# 테스트 코드
# ================================================================

if __name__ == "__main__":
    # 테스트용
    detector = SerialNumberDetector(
        yolo_model_path='runs/detect/serial_number_detector/weights/best.pt',
        languages=['en']
    )

    # 이미지 로드
    test_image_path = 'data/raw/serial_number_detection/test/images/pcb_001.jpg'
    image = cv2.imread(test_image_path)

    # 검출
    result = detector.detect(image)

    # 결과 출력
    print("\n검출 결과:")
    print(f"  성공: {result['success']}")
    print(f"  일련번호: {result['serial_number']}")
    print(f"  OCR 신뢰도: {result['confidence']:.3f}")
    print(f"  YOLO 신뢰도: {result['yolo_confidence']:.3f}")
    print(f"  YOLO bbox: {result['yolo_bbox']}")

    if not result['success']:
        print(f"  오류: {result.get('error', 'Unknown')}")

    # 시각화
    vis_image = detector.visualize(image, result)
    cv2.imwrite('serial_number_detection_result.jpg', vis_image)
    print("\n✓ 시각화 결과 저장: serial_number_detection_result.jpg")
```

---

## 4단계: Flask 서버 통합

### 4.1. Flask 서버에 일련번호 검출 추가

**server/app.py 수정**:
```python
from serial_number_detector import SerialNumberDetector

# 전역 변수로 일련번호 검출기 추가
serial_detector = None

@app.before_first_request
def initialize():
    """서버 시작 시 모델 로드"""
    global component_model, solder_model, serial_detector

    # 기존 모델 로드
    component_model = YOLO('models/fpic_component_best.pt')
    solder_model = YOLO('models/soldef_ai_best.pt')

    # 일련번호 검출기 로드
    serial_detector = SerialNumberDetector(
        yolo_model_path='runs/detect/serial_number_detector/weights/best.pt',
        languages=['en']
    )

@app.route('/predict_dual', methods=['POST'])
def predict_dual():
    """양면 동시 검사 + 일련번호 검출"""

    # 1. 일련번호 검출 (좌측 프레임)
    serial_result = serial_detector.detect(left_frame)

    # 2. 부품 검출 (기존)
    component_result = component_model.predict(left_frame)

    # 3. 납땜 검출 (기존)
    solder_result = solder_model.predict(right_frame)

    # 4. 융합 판정 (기존)
    fusion_result = fuse_results(component_result, solder_result)

    # 5. DB 저장 (일련번호 포함)
    save_to_db(
        serial_number=serial_result['serial_number'],
        serial_confidence=serial_result['confidence'],
        serial_bbox=serial_result['yolo_bbox'],
        ...
    )

    return jsonify({
        'serial_number': serial_result['serial_number'],
        'serial_confidence': serial_result['confidence'],
        'fusion_decision': fusion_result['decision'],
        ...
    })
```

---

## 요약

### 라벨링 핵심 정리 ⭐

1. **라벨링 대상**: 일련번호가 있는 **영역(박스)**만 그리면 됨
2. **클래스**: `serial_number` 1개만
3. **도구**: LabelImg 또는 Roboflow
4. **필요 이미지**: 100-300장 (위치 고정이므로 적은 수로 충분)
5. **포맷**: YOLO (.txt 파일)

### 학습 핵심 정리 ⭐

1. **모델**: YOLOv11n (Nano - 가볍고 빠름)
2. **에폭**: 50 (데이터 적으므로 적은 에폭 OK)
3. **증강**: 최소화 (위치 고정이므로)
4. **학습 시간**: 5-10분 (RTX 4080 Super 기준)

### YOLO + OCR 워크플로우 ⭐

```
좌측 카메라 프레임 입력
        ↓
[1단계] YOLO: 일련번호 영역 검출 (bbox)
        ↓
[2단계] ROI 추출 (bbox 영역만 자르기)
        ↓
[3단계] OCR: 텍스트 인식
        ↓
[4단계] 유효성 검사 (정규식 패턴)
        ↓
결과 반환 (serial_number, confidence, bbox)
```

---

**작성일**: 2025-11-07
**문서 버전**: 1.0
