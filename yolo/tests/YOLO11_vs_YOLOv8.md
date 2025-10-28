# YOLO11 vs YOLOv8 비교

**작성일**: 2025-10-25

---

## 🆕 YOLO11이란?

**YOLO11** (또는 YOLOv11)은 **2024년 9월 30일** Ultralytics가 YOLOVision 이벤트에서 공식 발표한 **최신 실시간 객체 탐지 모델**입니다.

- **출시**: 2024년 9월 30일
- **개발사**: Ultralytics
- **위치**: YOLOv8의 차세대 버전
- **목표**: 정확도, 속도, 효율성의 완벽한 균형

---

## 🔍 주요 개선 사항

### 1. **아키텍처 개선**

#### YOLOv8 → YOLO11 변경점
- **C2f → C3k2**: 더 컴팩트한 블록으로 교체 (파라미터 감소)
- **C2PSA 도입**: 공간 주의력(Spatial Attention) 강화
- **SPPF 유지**: 다중 스케일 특징 추출 유지
- **Neck 구조 개선**: 특징 추출 능력 향상

### 2. **성능 비교 (Nano 모델 기준)**

| 항목 | YOLOv8n | YOLO11n | 개선율 |
|------|---------|---------|--------|
| **레이어 수** | 129 | 181 | +40% |
| **파라미터 수** | 3,157,200 | 2,624,080 | **-17%** ⭐ |
| **연산량 (GFLOPs)** | 8.9 | 6.6 | **-26%** ⭐ |
| **추론 속도** | 14.60 ms | 24.21 ms | -66% |
| **FPS** | 68.48 | 41.31 | -40% |
| **Confidence** | 0.656 | 0.837 | **+28%** ⭐ |

### 3. **Medium 모델 비교 (공식 데이터)**

| 항목 | YOLOv8m | YOLO11m | 개선율 |
|------|---------|---------|--------|
| **파라미터 수** | 25.9M | 20.1M | **-22%** |
| **mAP (COCO)** | - | 더 높음 | ✅ |
| **추론 속도** | - | 2% 빠름 | ✅ |
| **지연시간** | - | 최대 25% 감소 | ✅ |

---

## 📊 우리의 테스트 결과 분석

### 테스트 환경
- **이미지**: test_images/bus.jpg (640x480)
- **GPU**: RTX 4080 Super (17.17 GB VRAM)
- **반복**: 5회

### YOLOv8n 성능
```
✅ 평균 추론 시간: 14.60 ms
✅ FPS: 68.48
⚠️ Confidence: 0.656 (낮음)
✅ 검출: 6개 (정확)
```

### YOLO11n 성능
```
⚠️ 평균 추론 시간: 24.21 ms (느림)
⚠️ FPS: 41.31 (낮음)
✅ Confidence: 0.837 (높음!)
✅ 검출: 5개 (정확)
```

### 🤔 왜 YOLO11n이 더 느릴까?

**예상 원인**:
1. **레이어 수 증가**: 129 → 181 (40% 증가)
2. **복잡한 구조**: C3k2, C2PSA 등 새로운 블록 추가
3. **최적화 부족**: 새로운 모델이라 GPU 최적화가 덜 됨
4. **테스트 환경**: 초기 버전이라 안정화 필요

**하지만**:
- **파라미터는 17% 감소** (메모리 효율적)
- **Confidence는 28% 향상** (더 정확한 검출)
- **공식 벤치마크에서는 더 빠르다고 발표** (Medium 모델 기준)

---

## 🎯 PCB 검사 프로젝트에 어떤 모델을 쓸까?

### 옵션 1: **YOLOv8l (Large)** ⭐ 추천
```
👍 장점:
- 검증된 안정성 (2023년 출시, 1년+ 검증)
- 뛰어난 속도 (14.60 ms @ Nano)
- 풍부한 커뮤니티 및 자료
- PCB 데이터셋 Fine-tuning 사례 많음

👎 단점:
- YOLO11 대비 파라미터 22% 더 많음
- 최신 기술은 아님
```

**예상 성능 (YOLOv8l @ RTX 4080 Super)**:
- 추론 시간: 20-30 ms (목표 300ms 대비 충분)
- FPS: 30-50 (목표 10 FPS 대비 충분)
- mAP: 0.90+ (Fine-tuning 후)

### 옵션 2: **YOLO11l (Large)** 🆕 실험적
```
👍 장점:
- 최신 아키텍처 (2024년 9월)
- 파라미터 22% 감소 (메모리 효율)
- 공식 벤치마크 성능 우수

👎 단점:
- 검증 부족 (출시 1개월)
- PCB Fine-tuning 사례 부족
- 커뮤니티 자료 부족
- 테스트에서 속도 느림 (하드웨어 최적화 필요)
```

**예상 성능 (YOLO11l @ RTX 4080 Super)**:
- 추론 시간: 25-40 ms (예상)
- FPS: 25-40 (예상)
- mAP: 0.92+ (예상, YOLOv8 대비 향상)

---

## 💡 최종 권장 사항

### Phase 3 (모델 학습) 전략

#### 1단계: **YOLOv8l로 시작** (안전한 선택)
```bash
# YOLOv8l Fine-tuning
python yolo/train_yolo.py \
    --model yolov8l.pt \
    --data data/pcb_defects.yaml \
    --epochs 150 \
    --batch 32
```

**이유**:
- ✅ 검증된 안정성
- ✅ 빠른 학습 속도
- ✅ 풍부한 학습 자료
- ✅ 목표 성능 달성 확실

#### 2단계: **YOLO11l 실험** (선택 사항, 시간 있을 때)
```bash
# YOLO11l Fine-tuning (실험)
python yolo/train_yolo.py \
    --model yolo11l.pt \
    --data data/pcb_defects.yaml \
    --epochs 150 \
    --batch 32
```

**조건**:
- ✅ YOLOv8l로 목표 mAP 0.85+ 달성 후
- ✅ Flask 서버 + 라즈베리파이 연동 완료 후
- ✅ 졸업 발표까지 4주 이상 남았을 때

#### 비교 및 선택
- 두 모델 성능 비교
- mAP, FPS, 추론 시간 비교
- 더 나은 모델 선택

---

## 📈 성능 예측 (PCB Fine-tuning 후)

### YOLOv8l (예상)
```
✅ mAP@0.5: 0.88-0.92
✅ 추론 시간: 20-30 ms
✅ FPS: 30-50
✅ 메모리: 8-10 GB VRAM
```

### YOLO11l (예상)
```
✅ mAP@0.5: 0.90-0.94 (소폭 향상)
⚠️ 추론 시간: 25-40 ms (약간 느림)
⚠️ FPS: 25-40
✅ 메모리: 6-8 GB VRAM (22% 감소!)
```

---

## 🔬 추가 테스트 필요 사항

### YOLO11의 잠재력 확인을 위해:

1. **다양한 크기 테스트**
   ```bash
   # YOLO11의 s, m, l 모델 모두 테스트
   python tests/phase1_yolo_basic/test_yolo_detailed.py yolo11s.pt
   python tests/phase1_yolo_basic/test_yolo_detailed.py yolo11m.pt
   python tests/phase1_yolo_basic/test_yolo_detailed.py yolo11l.pt
   ```

2. **배치 처리 테스트**
   ```python
   # 양면 동시 처리 (batch=2)
   results = model([left_frame, right_frame])
   ```

3. **FP16 최적화 테스트**
   ```python
   # Half Precision 사용
   model = YOLO('yolo11l.pt')
   model.half()  # FP16 변환
   ```

---

## 📚 참고 자료

### YOLO11 공식 문서
- [YOLO11 공식 발표](https://www.ultralytics.com/blog/ultralytics-yolo11-has-arrived-redefine-whats-possible-in-ai)
- [Ultralytics YOLO11 문서](https://docs.ultralytics.com/models/yolo11/)
- [YOLOv11 아키텍처 논문](https://arxiv.org/html/2410.17725v1)

### YOLOv8 vs YOLO11
- [Medium: YOLOv11 핵심 정리](https://medium.com/@zainshariff6506/yolov11-is-officially-out-what-you-need-to-know-6738c5d25be1)
- [MarkTechPost: YOLO11 릴리스](https://www.marktechpost.com/2024/10/03/yolo11-released-by-ultralytics-unveiling-next-gen-features-for-real-time-image-analysis-and-autonomous-systems/)

---

## 🎯 결론

### YOLO11은 무엇인가?
- ✅ Ultralytics의 최신 객체 탐지 모델 (2024년 9월)
- ✅ YOLOv8 대비 **파라미터 22% 감소**, **정확도 향상**
- ⚠️ 출시 1개월로 검증 부족
- ⚠️ 우리 테스트에서는 속도 느림 (하드웨어 최적화 필요)

### PCB 프로젝트 권장
1. **Phase 3**: **YOLOv8l**로 시작 (안전하고 검증됨) ⭐
2. **Phase 4**: 여유 있으면 **YOLO11l** 실험 (더 나은 정확도 기대)
3. **최종 선택**: 두 모델 비교 후 결정

### 핵심 메시지
> "**YOLOv8l**로 안전하게 목표 달성 후, 시간 여유가 있다면 **YOLO11l**로 성능 개선 시도"

---

**작성일**: 2025-10-25
**다음 업데이트**: YOLO11 Large 모델 테스트 후
