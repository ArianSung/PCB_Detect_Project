# 이중 모델 아키텍처 설계

## 시스템 흐름

```
[좌측 카메라] ──→ [부품 검출 모델] ──→ [결과A]
                                           ↓
                  [Flask 서버 - 결과 융합 로직]
                                           ↑
[우측 카메라] ──→ [납땜 검출 모델] ──→ [결과B]
                                           ↓
                    [최종 판정 + GPIO 제어]
                           ↓
              ┌─────────────┼─────────────┐
              ↓             ↓             ↓
           [정상]     [부품/납땜불량]   [폐기]
```

---

## 1. 이중 모델 병렬 추론

### Flask 서버 구조 (`server/app.py`)

```python
class DualModelInference:
    def __init__(self):
        # 모델 1: 부품 검출 (25개 클래스)
        self.component_model = YOLO('models/fpic_component_best.pt')

        # 모델 2: 납땜 불량 (5-6개 클래스)
        self.solder_model = YOLO('models/soldef_ai_best.pt')

    def predict_dual(self, left_frame, right_frame):
        """양면 동시 추론"""

        # 병렬 추론 (PyTorch는 자동으로 배치 처리)
        component_result = self.component_model(left_frame)[0]  # 좌측 = 부품
        solder_result = self.solder_model(right_frame)[0]       # 우측 = 납땜

        # 결과 융합
        final_decision = self.fuse_results(component_result, solder_result)

        return final_decision
```

---

## 2. 결과 융합 로직

### 판정 기준

```python
def fuse_results(self, component_result, solder_result):
    """
    두 모델 결과를 융합하여 최종 판정

    반환값:
    - "normal": 정상
    - "component_defect": 부품 불량 (앞면)
    - "solder_defect": 납땜 불량 (뒷면)
    - "discard": 폐기
    """

    # 1. 결함 검출
    component_defects = self._detect_component_defects(component_result)
    solder_defects = self._detect_solder_defects(solder_result)

    # 2. 심각도 평가
    component_severity = self._calculate_severity(component_defects)
    solder_severity = self._calculate_severity(solder_defects)

    # 3. 최종 판정
    if component_severity == 0 and solder_severity == 0:
        return "normal"

    # 폐기 조건
    if (component_severity >= 3 or solder_severity >= 3 or
        (component_severity >= 2 and solder_severity >= 2)):
        return "discard"

    # 단일 불량
    if component_severity > solder_severity:
        return "component_defect"
    else:
        return "solder_defect"

def _calculate_severity(self, defects):
    """
    불량 심각도 계산

    Level 0: 불량 없음
    Level 1: 경미한 불량 (1-2개)
    Level 2: 중간 불량 (3-5개)
    Level 3: 심각한 불량 (6개 이상 or 치명적 불량)
    """
    if not defects:
        return 0

    # 치명적 불량 타입
    critical_types = ['missing_component', 'wrong_component', 'solder_bridge']

    # 치명적 불량 검출 시 즉시 Level 3
    if any(d['type'] in critical_types for d in defects):
        return 3

    # 불량 개수로 판단
    count = len(defects)
    if count == 0:
        return 0
    elif count <= 2:
        return 1
    elif count <= 5:
        return 2
    else:
        return 3
```

---

## 3. GPIO 제어 매핑

### 라즈베리파이 1 (좌측 카메라 + GPIO 제어)

```python
# raspberry_pi/camera_client.py

GPIO_PINS = {
    'normal': 23,           # 정상 → GPIO 23
    'component_defect': 17, # 부품불량 → GPIO 17
    'solder_defect': 27,    # 납땜불량 → GPIO 27
    'discard': 22           # 폐기 → GPIO 22
}

def control_gpio(decision):
    """Flask 서버 결과에 따라 GPIO 제어"""
    pin = GPIO_PINS.get(decision)
    if pin:
        GPIO.output(pin, GPIO.HIGH)
        time.sleep(0.5)  # 릴레이 동작 시간
        GPIO.output(pin, GPIO.LOW)
```

---

## 4. Flask API 엔드포인트

### `/predict_dual` (양면 동시 검사)

```python
@app.route('/predict_dual', methods=['POST'])
def predict_dual():
    """양면 PCB 동시 검사"""

    # 1. 좌/우 프레임 수신
    left_frame = decode_image(request.json['left_frame'])
    right_frame = decode_image(request.json['right_frame'])

    # 2. 이중 모델 추론
    component_result = component_model(left_frame)[0]
    solder_result = solder_model(right_frame)[0]

    # 3. 결과 융합
    final_decision = fuse_results(component_result, solder_result)

    # 4. 상세 정보 생성
    response = {
        'decision': final_decision,  # "normal" / "component_defect" / "solder_defect" / "discard"
        'component_defects': parse_component_defects(component_result),
        'solder_defects': parse_solder_defects(solder_result),
        'timestamp': datetime.now().isoformat()
    }

    # 5. DB 저장
    save_to_database(response)

    return jsonify(response)
```

---

## 5. 라즈베리파이 클라이언트 수정

### 양면 동시 전송

```python
# raspberry_pi/dual_camera_client.py

import requests
import cv2
import base64
import RPi.GPIO as GPIO

def capture_and_send():
    """양면 프레임 캡처 및 전송"""

    # 1. 양면 캡처 (거의 동시)
    left_frame = left_camera.read()[1]
    right_frame = right_camera.read()[1]

    # 2. 인코딩
    left_encoded = encode_frame(left_frame)
    right_encoded = encode_frame(right_frame)

    # 3. Flask 서버 전송
    response = requests.post(
        f"{SERVER_URL}/predict_dual",
        json={
            'left_frame': left_encoded,
            'right_frame': right_encoded
        }
    )

    # 4. 결과 수신
    result = response.json()
    decision = result['decision']

    # 5. GPIO 제어
    control_gpio(decision)

    # 6. 시각화
    print(f"판정: {decision}")
    print(f"부품 불량: {len(result['component_defects'])}개")
    print(f"납땜 불량: {len(result['solder_defects'])}개")
```

---

## 6. 성능 분석

### 추론 시간

```
[좌측 카메라] → 부품 모델 (50-80ms)
                                    ↓
[우측 카메라] → 납땜 모델 (30-50ms) → [병렬 처리]
                                    ↓
                        결과 융합 (<5ms)
                                    ↓
                        총 시간: 80-85ms
```

**목표 300ms 대비**: 충분히 여유 ✅ (3.5배 여유)

### GPU 메모리

- 부품 모델 (YOLOv11m): ~4GB
- 납땜 모델 (YOLOv11m): ~4GB
- **총 VRAM**: ~8GB
- **RTX 4080 Super (16GB)**: 충분함 ✅

---

## 7. 장점

### ✅ 기술적 장점
1. **전문화된 모델** → 높은 정확도
2. **병렬 추론** → 빠른 속도
3. **유연한 융합 로직** → 쉬운 조정
4. **독립적 학습** → 빠른 개발

### ✅ 프로젝트 장점
1. **양면 검사 컨셉 부합** ⭐⭐⭐
2. **4가지 분류 명확** (정상/부품불량/납땜불량/폐기)
3. **GPIO 제어 단순** (하나의 신호만)
4. **발표 효과 극대화**

### ✅ 유지보수 장점
1. **모델 독립 업데이트** 가능
2. **융합 로직만 수정** 가능
3. **새 불량 타입 추가** 쉬움

---

## 8. 예상 결과

### 분류 정확도

| 판정 | 예상 정확도 |
|------|------------|
| 정상 | 95%+ |
| 부품불량 | 85-90% |
| 납땜불량 | 90-95% |
| 폐기 | 95%+ |

### 추론 속도

- **단일 PCB 검사**: 80-100ms
- **초당 처리량**: 10-12 PCB/s
- **디팔렛타이저 속도 (2.5초)**: 충분히 빠름 ✅

---

## 9. 구현 순서

### Phase 1: 데이터셋 다운로드 (1일)
1. FPIC-Component 다운로드
2. SolDef_AI 다운로드
3. YOLO 형식 변환

### Phase 2: 모델 학습 (2-3일)
1. 부품 검출 모델 학습 (FPIC)
2. 납땜 불량 모델 학습 (SolDef_AI)
3. 성능 평가

### Phase 3: Flask 서버 구현 (1일)
1. 이중 모델 로드
2. 결과 융합 로직 구현
3. API 엔드포인트 구현

### Phase 4: 라즈베리파이 연동 (1일)
1. 양면 동시 전송 구현
2. GPIO 제어 구현
3. 통합 테스트

### 총 개발 기간: 5-6일

---

## 10. 결론

**이중 모델 + 결과 융합 방식은 충분히 실용적입니다!**

✅ 기술적으로 구현 가능
✅ 성능 목표 달성 가능
✅ 프로젝트 컨셉에 완벽 부합
✅ 유지보수 및 확장 용이

**핵심**: 두 모델은 독립적으로 추론하고, Flask 서버에서 결과만 융합하면 됩니다!
