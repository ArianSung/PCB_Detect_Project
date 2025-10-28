# AI 모델 팀 시작 가이드

> PCB 불량 검사 시스템 AI 모델 (YOLO + 이상 탐지) 개발을 시작하는 팀원을 위한 빠른 시작 가이드입니다.

---

## 🎯 AI 모델 팀의 역할

- **YOLOv8 모델 학습**: PCB 불량 객체 탐지 모델 학습 및 최적화
- **이상 탐지 모델 구현**: PaDiM 기반 이상 탐지 시스템 구축
- **모델 성능 평가**: mAP, FPS, 정확도 측정 및 개선
- **모델 전달**: 학습된 모델 파일을 Flask 팀에 전달

---

## 📚 반드시 읽어야 할 문서

### 필수 문서 (우선순위 순)

1. **[Phase1_YOLO_Setup.md](../docs/Phase1_YOLO_Setup.md)** ⭐ 가장 중요!
   - YOLO 환경 구축 및 Phase 1 완료 가이드

2. **[yolo/tests/README.md](tests/README.md)** ⭐ Phase 1 테스트!
   - Phase 1 테스트 스크립트 및 결과 확인

3. **[yolo/tests/PHASE1_TEST_RESULTS.md](tests/PHASE1_TEST_RESULTS.md)**
   - Phase 1 테스트 결과 및 성공 기준

4. **[Dataset_Guide.md](../docs/Dataset_Guide.md)**
   - PCB 데이터셋 준비 및 YOLO 형식 변환

5. **[YOLO11_vs_YOLOv8.md](tests/YOLO11_vs_YOLOv8.md)**
   - YOLO 버전 비교 및 선택 가이드

### 참고 문서

- [Team_Collaboration_Guide.md](../docs/Team_Collaboration_Guide.md) - 팀 협업 규칙
- [Git_Workflow.md](../docs/Git_Workflow.md) - Git 브랜치 전략
- [Development_Setup.md](../docs/Development_Setup.md) - 로컬 환경 구성

---

## ⚙️ 개발 환경 설정

### 시스템 요구사항

- **OS**: Ubuntu 20.04 / 22.04
- **GPU**: NVIDIA RTX 4080 Super (16GB VRAM)
- **Python**: 3.10 (Conda 가상환경)
- **CUDA**: 11.8 이상
- **저장공간**: 100GB 이상 (데이터셋 + 모델)

### 1. Conda 가상환경 생성 및 활성화

```bash
# 가상환경 생성 (최초 1회)
conda create -n pcb_defect python=3.10 -y

# 가상환경 활성화
conda activate pcb_defect

# 확인
python --version  # Python 3.10.x
```

### 2. PyTorch + CUDA 설치

```bash
# CUDA 11.8 + PyTorch 설치
pip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cu118

# 설치 확인
python -c "import torch; print(f'PyTorch: {torch.__version__}'); print(f'CUDA: {torch.cuda.is_available()}')"

# 예상 출력:
# PyTorch: 2.7.1+cu118
# CUDA: True
```

### 3. YOLO 및 프로젝트 패키지 설치

```bash
# 프로젝트 루트로 이동
cd /path/to/PCB_Detect_Project

# 패키지 설치
pip install -r requirements.txt

# YOLO 패키지 확인
pip list | grep ultralytics
```

---

## 🧪 Phase 1 테스트 (필수!)

Phase 1은 YOLO 환경이 제대로 구축되었는지 확인하는 단계입니다.

### 1. 기본 YOLO 테스트

```bash
cd yolo/tests

# 1. 기본 YOLO 추론 테스트
python test_yolo_basic.py

# 예상 출력:
# ✓ YOLO 모델 로드 성공
# ✓ 추론 성공 (bus.jpg)
# ✓ 결과 저장: yolo/runs/detect/predict/bus.jpg
```

### 2. COCO128 데이터셋 학습 테스트

```bash
# COCO128 데이터셋으로 1 epoch 학습
python test_yolo_coco128.py

# 예상 출력:
# Downloading COCO128 dataset...
# Epoch 1/1: 100%|█████████| ...
# ✓ 학습 완료
# ✓ 결과: yolo/runs/detect/train
```

### 3. 결과 확인

```bash
# Phase 1 테스트 결과 확인
cat tests/PHASE1_TEST_RESULTS.md

# 학습 결과 확인
ls -l yolo/runs/detect/train/
# - weights/best.pt (최고 성능 모델)
# - weights/last.pt (마지막 에폭 모델)
# - results.png (학습 그래프)
```

---

## 📊 YOLO 작업 디렉토리 구조

```
yolo/
├── README.md                  # YOLO 디렉토리 가이드
├── datasets/                  # YOLO 데이터셋 (Git 무시)
│   └── coco128/              # COCO128 샘플 데이터셋
├── runs/                      # YOLO 학습 결과 (Git 무시)
│   └── detect/
│       ├── train/            # 학습 결과
│       └── predict/          # 추론 결과
├── test_images/               # 테스트 이미지 (Git 무시)
│   └── bus.jpg
└── tests/                     # Phase 1 테스트 스크립트
    ├── README.md
    ├── PHASE1_TEST_RESULTS.md
    ├── YOLO11_vs_YOLOv8.md
    ├── test_yolo_basic.py
    ├── test_yolo_coco128.py
    └── models_backup/         # 백업 모델
        ├── yolov8n.pt
        └── yolo11n.pt
```

---

## 🚀 첫 번째 작업 제안

### 작업 1: Phase 1 테스트 실행 및 확인

**목표**: YOLO 환경이 제대로 구축되었는지 확인

1. `yolo/tests/test_yolo_basic.py` 실행
2. `yolo/tests/test_yolo_coco128.py` 실행
3. `yolo/tests/PHASE1_TEST_RESULTS.md` 결과 확인
4. 팀 채팅방에 Phase 1 완료 보고

### 작업 2: YOLOv8 vs YOLO11 비교 실험

**목표**: 프로젝트에 적합한 YOLO 버전 선택

1. `yolo/tests/YOLO11_vs_YOLOv8.md` 읽기
2. 두 버전 모델로 COCO128 학습 비교
3. 추론 속도(FPS) 및 정확도(mAP) 측정
4. 결과를 팀과 공유하여 최종 버전 결정

### 작업 3: PCB 데이터셋 준비

**목표**: 실제 PCB 불량 데이터셋 수집 및 전처리

1. `docs/Dataset_Guide.md` 읽기
2. PCB 이미지 수집 (GitHub, Kaggle, Roboflow 등)
3. YOLO 형식으로 변환 (라벨링)
4. `data/pcb_defects.yaml` 설정 파일 작성

---

## 🤖 AI에게 물어볼 프롬프트

### 시작 프롬프트 (복사해서 사용하세요)

```
안녕! 나는 PCB 불량 검사 시스템의 AI 모델 팀원이야.

**내 역할:**
- YOLOv8 모델 학습 및 최적화
- 이상 탐지 모델 구현 (PaDiM)
- 모델 성능 평가 (mAP, FPS, 정확도)
- 학습된 모델 Flask 팀에 전달

**읽어야 할 핵심 문서:**
1. `docs/Phase1_YOLO_Setup.md` - YOLO 환경 구축 및 Phase 1 가이드
2. `docs/Dataset_Guide.md` - 데이터셋 준비 및 전처리
3. `yolo/README.md` - YOLO 작업 디렉토리 가이드
4. `yolo/tests/README.md` - Phase 1 테스트 가이드
5. `yolo/tests/YOLO11_vs_YOLOv8.md` - YOLO 버전 비교

**개발 환경:**
- OS: Ubuntu 22.04 (GPU PC)
- GPU: NVIDIA RTX 4080 Super (16GB VRAM)
- Python: 3.10 (Conda 가상환경 `pcb_defect`)
- YOLO 버전: YOLOv8l (Large 모델 권장)

**작업 디렉토리:**
```
yolo/
├── datasets/      # YOLO 데이터셋 (COCO128, 향후 PCB 데이터셋)
├── runs/          # YOLO 학습 결과 (Git 무시)
├── test_images/   # 테스트 이미지
└── tests/         # Phase 1 테스트 스크립트
```

**첫 번째 작업 (Phase 1 완료 확인):**
1. Conda 가상환경 활성화: `conda activate pcb_defect`
2. GPU 확인: `python -c "import torch; print(torch.cuda.is_available())"`
3. Phase 1 기본 테스트 실행:
   ```bash
   cd yolo/tests
   python test_yolo_basic.py
   python test_yolo_coco128.py
   ```
4. 결과 확인: `yolo/tests/PHASE1_TEST_RESULTS.md` 참고

위 정보를 바탕으로, YOLO 모델을 처음 테스트하고 학습을 시작하는 과정을 안내해줘.
특히 GPU가 제대로 인식되는지, 그리고 Phase 1 테스트가 성공하는지 확인하는 방법을 알려줘.
```

---

## ✅ 체크리스트

### 환경 설정 완료 체크리스트

- [ ] Conda 가상환경 생성 및 활성화 완료
- [ ] PyTorch + CUDA 설치 확인 (GPU 사용 가능)
- [ ] Ultralytics YOLO 설치 확인
- [ ] COCO128 데이터셋 다운로드 완료
- [ ] Phase 1 기본 테스트 성공 (`test_yolo_basic.py`)
- [ ] Phase 1 학습 테스트 성공 (`test_yolo_coco128.py`)

### 문서 읽기 체크리스트

- [ ] `docs/Phase1_YOLO_Setup.md` 읽기 완료
- [ ] `yolo/tests/README.md` 읽기 완료
- [ ] `yolo/tests/PHASE1_TEST_RESULTS.md` 읽기 완료
- [ ] `yolo/tests/YOLO11_vs_YOLOv8.md` 읽기 완료
- [ ] `docs/Dataset_Guide.md` 읽기 완료

### Git 설정 체크리스트

- [ ] `develop` 브랜치에서 `feature/ai-model` 브랜치 생성 완료
- [ ] `yolo/datasets/`, `yolo/runs/`, `yolo/test_images/`가 `.gitignore`에 포함되어 있는지 확인
- [ ] Phase 1 테스트 결과를 팀과 공유

---

## 🧠 YOLO 학습 팁

### 1. GPU 메모리 최적화

```bash
# RTX 4080 Super (16GB VRAM) 최적 설정
# configs/yolo_config.yaml

model: yolov8l.pt        # Large 모델
batch_size: 32           # 16GB VRAM 활용
image_size: 640
device: 0                # GPU ID
optimizer: AdamW
lr0: 0.001
```

### 2. FP16 (Half Precision) 사용

```python
# 학습 시 FP16 사용으로 VRAM 50% 절약
from ultralytics import YOLO

model = YOLO("yolov8l.pt")
model.train(
    data="data/pcb_defects.yaml",
    epochs=150,
    batch=32,
    imgsz=640,
    device=0,
    half=True  # FP16 활성화
)
```

### 3. 학습 중단 및 재개

```bash
# 학습 중단 시 자동 저장: yolo/runs/detect/train/weights/last.pt

# 재개:
python yolo/train_yolo.py --resume yolo/runs/detect/train/weights/last.pt
```

---

## 🚨 자주 발생하는 문제 및 해결

### 문제 1: CUDA Out of Memory

**에러**: `RuntimeError: CUDA out of memory`

**해결 방법:**
1. 배치 사이즈 줄이기: `batch_size: 16` (32 → 16)
2. 이미지 크기 줄이기: `image_size: 512` (640 → 512)
3. FP16 사용: `half=True`

### 문제 2: GPU 인식 안 됨

**에러**: `torch.cuda.is_available()` 반환값이 `False`

**해결 방법:**
1. CUDA 드라이버 확인: `nvidia-smi`
2. PyTorch CUDA 재설치:
   ```bash
   pip uninstall torch torchvision torchaudio
   pip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cu118
   ```

### 문제 3: COCO128 다운로드 실패

**에러**: `Dataset download failed`

**해결 방법:**
1. 수동 다운로드: https://github.com/ultralytics/yolov5/releases/download/v1.0/coco128.zip
2. 압축 해제: `unzip coco128.zip -d yolo/datasets/`

---

## 📞 도움 요청

- **AI 모델 팀 리더**: [연락처]
- **Flask 팀 (모델 통합)**: [연락처]
- **전체 팀 채팅방**: [링크]

---

## 🔗 추가 참고 자료

### YOLO 공식 문서

- [Ultralytics YOLOv8 Docs](https://docs.ultralytics.com/)
- [YOLO11 Release Notes](https://github.com/ultralytics/ultralytics/releases)

### PCB 데이터셋

- [PCB Defects Dataset (Kaggle)](https://www.kaggle.com/search?q=pcb+defects)
- [Roboflow PCB Datasets](https://universe.roboflow.com/search?q=pcb)

---

**마지막 업데이트**: 2025-10-25
**작성자**: 팀 리더
