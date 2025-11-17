# GPU 메모리 최적화 가이드

## 실제 VRAM 사용량 vs 이론상 예상치

### YOLOv11l 학습 시 실제 VRAM 사용량

**RTX 4080 Super (16GB VRAM) 기준 실제 측정**:

| 배치 사이즈 | 이미지 크기 | 예상 VRAM | 실제 VRAM | 상태 | 학습 속도 |
|------------|------------|-----------|-----------|------|----------|
| batch=16   | 640×640    | 8-10GB    | **11.4-11.8GB** ⭐ | ✅ 안정 | 빠름 |
| batch=32   | 640×640    | 12-14GB   | **18GB** | ❌ 스와핑 | 매우 느림 |
| batch=16   | 800×800    | 10-12GB   | **14-16GB** (추정) | ⚠️ 여유 적음 | 보통 |
| batch=32   | 800×800    | 14-16GB   | **20GB+** (추정) | ❌ OOM | 불가능 |

**⭐ 2025-11-07 실제 측정**: YOLOv11l, batch=16, imgsz=640, SolDef_AI 데이터셋 (299 images)

### 왜 실제 VRAM 사용량이 더 높을까?

**배치 32 기준 실제 메모리 구성**:
```
총 VRAM 사용량: ~18GB (실제 측정)

1. 모델 가중치 (YOLOv11l):           ~2.0GB
2. Optimizer State (AdamW):
   - Momentum 버퍼:                  ~2.0GB 
   - Variance 버퍼:                  ~2.0GB
3. 배치 데이터 (32 × 640×640×3):     ~3.0GB
4. Gradient 버퍼:                    ~2.0GB
5. 활성화 맵 (forward pass):         ~5.0GB
6. 데이터 augmentation 버퍼:         ~2.0GB
────────────────────────────────────────────
합계:                                ~18GB
```

**배치 16으로 줄이면** (YOLOv11l 실제 측정 ⭐):
```
1. 모델 가중치:                      ~2.0GB
2. Optimizer State (변화 없음):      ~4.0GB
3. 배치 데이터 (16 × 640×640×3):     ~1.5GB
4. Gradient 버퍼:                    ~2.0GB
5. 활성화 맵 (절반 감소):            ~2.5GB
6. 데이터 augmentation 버퍼:         ~1.0GB
────────────────────────────────────────────
합계:                                ~13GB
실제 측정값 (2025-11-07):           11.4-11.8GB ✅
```

**측정 조건**: SolDef_AI 데이터셋 (299 images), amp=True (FP16), AdamW optimizer

## CUDA Unified Memory (메모리 스와핑)

### 증상
- VRAM이 16GB를 초과해도 **Out of Memory 에러가 안 남**
- 대신 **학습 속도가 10-20배 느려짐**
- GPU-Z나 nvidia-smi로 보면 VRAM 16GB 꽉 참

### 원인
```
VRAM 16GB 초과 시:
CUDA → 자동으로 시스템 RAM으로 스와핑
     → PCIe 버스를 통한 데이터 전송 (매우 느림)
     → GPU 대기 시간 증가
     → 학습 속도 급감
```

**PCIe 대역폭**:
- PCIe 4.0 x16: ~32GB/s
- GPU 메모리 대역폭: ~700GB/s (RTX 4080 Super)
- **속도 차이: 약 20배 느림**

### 해결 방법
1. **배치 사이즈 감소**: 32 → 16
2. **이미지 해상도 감소**: 800 → 640
3. **Gradient Accumulation 사용** (권장)

## Gradient Accumulation (권장)

### 개념
작은 배치로 여러 번 forward/backward를 수행한 후 한 번에 가중치 업데이트

### 장점
- **VRAM 절약**: 작은 배치 사용
- **큰 배치 효과**: 여러 배치의 gradient 누적
- **학습 안정성**: 큰 배치와 유사한 효과

### 사용 방법
```bash
# YOLOv11l 학습 (Gradient Accumulation)
yolo detect train \
  data=data.yaml \
  model=yolo11l.pt \
  epochs=200 \
  batch=16 \           # 실제 배치 (VRAM 12GB)
  accumulate=2 \       # 2번 누적 (효과적 배치 32)
  imgsz=640 \
  device=0 \
  amp=True
```

**효과**:
- VRAM 사용량: ~12GB (batch 16과 동일)
- 학습 효과: batch 32와 유사
- 속도: batch 16 대비 약간 느림 (10-15%)

## Mixed Precision (FP16)

### 실제 효과

**이론상**:
- VRAM 절약: 50%
- 속도 향상: 2배

**실제**:
- VRAM 절약: **20-30%**
- 속도 향상: **1.3-1.5배**

### 왜 차이가 날까?

```
FP16 (Mixed Precision) 실제 동작:

1. Forward Pass:        FP16 (절약 ✅)
2. 활성화 맵 저장:      FP16 (절약 ✅)
3. Backward Pass:       FP16 (절약 ✅)
4. Gradient:            FP16 (절약 ✅)
5. Optimizer State:     FP32 (절약 ❌) ← 핵심!
6. 가중치 업데이트:     FP32 (절약 ❌)
```

**AdamW Optimizer State**:
- Momentum: FP32 (정확도 필요)
- Variance: FP32 (정확도 필요)
- 총 크기: 모델 가중치의 2배 (~4GB)
- **이 부분은 FP16 적용 불가**

## 권장 설정 (RTX 4080 Super 16GB)

### Component 모델 (YOLOv11l, 6,260 이미지)
```bash
yolo detect train \
  data=data/processed/fpic_component_yolo/data.yaml \
  model=yolo11l.pt \
  epochs=200 \
  batch=16 \           # VRAM 12-14GB (안정)
  accumulate=2 \       # 효과적 배치 32
  imgsz=640 \
  device=0 \
  amp=True \           # FP16 사용
  optimizer=AdamW \
  lr0=0.002 \
  patience=100
```

**예상 VRAM**: 12-14GB
**예상 학습 시간**: 2-3시간

### Solder 모델 (YOLOv11l, 1,150 이미지)
```bash
yolo detect train \
  data=data/processed/soldef_ai_yolo/data.yaml \
  model=yolo11l.pt \
  epochs=200 \
  batch=16 \           # VRAM 12-14GB (안정)
  accumulate=2 \       # 효과적 배치 32
  imgsz=640 \
  device=0 \
  amp=True \
  optimizer=AdamW \
  lr0=0.002 \
  patience=100
```

**예상 VRAM**: 12-14GB
**예상 학습 시간**: 30-45분

## VRAM 모니터링 방법

### nvidia-smi로 실시간 모니터링
```bash
# 1초마다 VRAM 사용량 출력
watch -n 1 nvidia-smi

# 또는 로그 파일로 저장
nvidia-smi --query-gpu=timestamp,memory.used,memory.total,utilization.gpu \
  --format=csv -l 1 > gpu_usage.csv
```

### Python으로 모니터링
```python
import torch

# 현재 VRAM 사용량 확인
print(f"Allocated: {torch.cuda.memory_allocated(0) / 1024**3:.2f} GB")
print(f"Reserved:  {torch.cuda.memory_reserved(0) / 1024**3:.2f} GB")

# 최대 VRAM 사용량 확인
print(f"Max Allocated: {torch.cuda.max_memory_allocated(0) / 1024**3:.2f} GB")

# VRAM 캐시 비우기
torch.cuda.empty_cache()
```

## 배치 사이즈 자동 탐색

### Ultralytics autobatch
```python
from ultralytics import YOLO
from ultralytics.utils.autobatch import autobatch

# 모델 로드
model = YOLO("yolo11l.pt")

# 최적 배치 사이즈 자동 계산
optimal_batch = autobatch(model, imgsz=640, device=0)
print(f"최적 배치 사이즈: {optimal_batch}")

# 학습 시 자동 배치 사용
model.train(data="data.yaml", batch=-1, imgsz=640)  # batch=-1: 자동
```

## 트러블슈팅

### 문제 1: 학습이 매우 느림 (epoch당 1시간 이상)

**원인**: VRAM 초과로 스와핑 발생

**해결**:
```bash
# nvidia-smi로 VRAM 사용량 확인
nvidia-smi

# VRAM이 16GB 근처면 배치 감소
batch=16 → batch=8
```

### 문제 2: Out of Memory 에러

**원인**: VRAM 완전 초과

**해결**:
```bash
# 1. 배치 감소
batch=16 → batch=8

# 2. 이미지 크기 감소
imgsz=640 → imgsz=512

# 3. workers 감소 (데이터 로딩 메모리)
workers=8 → workers=4
```

### 문제 3: GPU 사용률이 낮음 (30-40%)

**원인**: CPU 병목 (데이터 로딩)

**해결**:
```bash
# workers 증가
workers=4 → workers=8

# 또는 데이터 캐싱 사용
cache=True  # RAM에 데이터셋 캐싱 (RAM 충분 시)
```

## 결론

### 핵심 교훈
1. **이론상 예상치는 최소값**: 실제는 1.5-2배 높음
2. **VRAM 16GB 초과 시 스와핑**: Out of Memory 대신 매우 느림
3. **배치 16 권장**: RTX 4080 Super에서 안정적
4. **Gradient Accumulation 활용**: 큰 배치 효과 + VRAM 절약
5. **FP16 효과는 제한적**: 20-30% 절약 (50% 아님)

### 권장 학습 설정
```yaml
# configs/yolo_config.yaml
batch_size: 16        # 안정적 (12-14GB)
accumulate: 2         # 효과적 배치 32
imgsz: 640           # 표준 해상도
amp: true            # FP16 사용
workers: 8           # 데이터 로딩 병렬화
optimizer: AdamW     # 추천
```

**예상 학습 시간**:
- Component 모델 (6,260 이미지): 2-3시간
- Solder 모델 (1,150 이미지): 30-45분

**VRAM 사용량**: 12-14GB (여유 2-4GB)
