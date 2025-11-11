# Phase 1: YOLO v11l 환경 구축 및 테스트 가이드 ⭐ (이중 모델 아키텍처)

## 목표
WSL2 환경에서 YOLO v11l을 설치하고 기본 테스트를 완료하여 프로젝트 개발 환경을 준비합니다.

**프로젝트 아키텍처**: 이중 YOLO 모델
- **Component Model**: FPIC-Component 데이터셋 (25개 클래스, 6,260 이미지)
- **Solder Model**: SolDef_AI 데이터셋 (5-6개 클래스, 429/1,150 이미지)
- **추론 방식**: 병렬 추론 + Flask 서버 결과 융합

---

## 체크리스트

### 1. 환경 확인
- [x] WSL2 설치 완료
- [x] Miniconda 설치 완료
- [x] 가상환경 생성 완료
- [ ] Python 버전 확인 (3.10 권장)
- [ ] GPU 사용 가능 여부 확인

---

## Step 1: WSL2에서 GPU 확인 (선택 사항)

### 1-1. NVIDIA 드라이버 확인
WSL2 터미널에서 다음 명령어 실행:

```bash
nvidia-smi
```

**정상 출력 예시**:
```
+-----------------------------------------------------------------------------+
| NVIDIA-SMI 535.xx.xx    Driver Version: 535.xx.xx    CUDA Version: 12.2     |
|-------------------------------+----------------------+----------------------+
| GPU  Name        Persistence-M| Bus-Id        Disp.A | Volatile Uncorr. ECC |
| Fan  Temp  Perf  Pwr:Usage/Cap|         Memory-Usage | GPU-Util  Compute M. |
```

**만약 오류가 발생하면**:
- Windows에서 NVIDIA GPU 드라이버 최신 버전 설치
- [NVIDIA CUDA on WSL 가이드](https://docs.nvidia.com/cuda/wsl-user-guide/index.html) 참고

### 1-2. CUDA 설치 (GPU 사용 시)

```bash
# WSL2에는 CUDA Toolkit을 별도로 설치할 필요 없음
# Windows의 NVIDIA 드라이버가 WSL2로 자동 연동됨
# PyTorch 설치 시 CUDA가 포함되어 설치됨
```

---

## Step 2: Python 가상환경 활성화 및 확인

### 2-1. Miniconda 가상환경 활성화

```bash
# 가상환경 이름이 'pcb_defect'라고 가정
conda activate pcb_defect

# 가상환경이 없다면 새로 생성
conda create -n pcb_defect python=3.10 -y
conda activate pcb_defect
```

### 2-2. Python 버전 확인

```bash
python --version
# 출력 예시: Python 3.10.13
```

### 2-3. pip 업그레이드

```bash
pip install --upgrade pip
```

---

## Step 3: PyTorch 설치

### 3-1. GPU 버전 설치 (NVIDIA GPU 있는 경우)

```bash
# CUDA 11.8 버전
pip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cu118

# 또는 CUDA 12.1 버전
pip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cu121
```

### 3-2. CPU 버전 설치 (GPU 없는 경우)

```bash
pip install torch torchvision torchaudio
```

### 3-3. PyTorch 설치 확인

```bash
python -c "import torch; print(f'PyTorch version: {torch.__version__}')"
python -c "import torch; print(f'CUDA available: {torch.cuda.is_available()}')"
python -c "import torch; print(f'CUDA version: {torch.version.cuda if torch.cuda.is_available() else \"N/A\"}')"
```

**정상 출력 예시** (GPU):
```
PyTorch version: 2.1.0+cu118
CUDA available: True
CUDA version: 11.8
```

**정상 출력 예시** (CPU):
```
PyTorch version: 2.1.0
CUDA available: False
CUDA version: N/A
```

---

## Step 4: YOLO v11l (Ultralytics) 설치

### 4-1. Ultralytics 패키지 설치

```bash
pip install ultralytics
```

### 4-2. 추가 필수 패키지 설치

```bash
pip install opencv-python-headless
pip install matplotlib
pip install pillow
pip install pyyaml
pip install scipy
```

### 4-3. 설치 확인

```bash
yolo version
# 또는
python -c "from ultralytics import YOLO; print(YOLO.__version__)"
```

**정상 출력 예시**:
```
8.0.200
```

---

## Step 5: YOLO v11l 기본 테스트

### 5-1. 사전 학습된 모델로 이미지 추론

#### 테스트 이미지 다운로드

```bash
# 프로젝트 폴더로 이동
cd ~/work_project

# 테스트용 폴더 생성
mkdir -p test_images
cd test_images

# 샘플 이미지 다운로드 (예시)
wget https://ultralytics.com/images/bus.jpg -O bus.jpg
# 또는 curl 사용
curl -o bus.jpg https://ultralytics.com/images/bus.jpg
```

#### Python으로 추론 실행

```bash
cd ~/work_project
```

`test_yolo_inference.py` 파일 생성:

```python
from ultralytics import YOLO
import os

# YOLOv8n 모델 로드 (처음 실행 시 자동 다운로드)
model = YOLO('yolov8n.pt')

# 이미지 추론
results = model('test_images/bus.jpg')

# 결과 출력
for r in results:
    print(f"Detected {len(r.boxes)} objects")
    print(r.boxes)  # 바운딩 박스 정보

# 결과 이미지 저장
results[0].save('results/bus_result.jpg')
print("Results saved to results/bus_result.jpg")
```

실행:

```bash
python test_yolo_inference.py
```

**정상 출력 예시**:
```
Downloading https://github.com/ultralytics/assets/releases/download/v0.0.0/yolov8n.pt to yolov8n.pt...
100%|████████████████████████████████████████| 6.23M/6.23M [00:01<00:00, 5.12MB/s]

image 1/1: 640x480 4 persons, 1 bus
Speed: 3.5ms preprocess, 15.2ms inference, 2.1ms postprocess per image at shape (1, 3, 640, 480)
Detected 5 objects
Results saved to results/bus_result.jpg
```

### 5-2. 명령줄(CLI)로 추론 실행

```bash
# 단일 이미지 추론
yolo detect predict model=yolov8n.pt source=test_images/bus.jpg

# 결과는 runs/detect/predict/ 폴더에 저장됨
```

### 5-3. 비디오 추론 테스트 (선택)

```bash
# 웹캠 사용 (WSL2에서는 제한적)
yolo detect predict model=yolov8n.pt source=0 show=True

# 비디오 파일 사용
yolo detect predict model=yolov8n.pt source=test_video.mp4
```

---

## Step 6: 커스텀 데이터로 학습 테스트

### 6-1. COCO128 샘플 데이터셋으로 학습

```bash
cd ~/work_project
```

`test_yolo_training.py` 파일 생성:

```python
from ultralytics import YOLO

# YOLOv8n 모델 로드
model = YOLO('yolov8n.pt')

# COCO128 데이터셋으로 학습 (자동 다운로드)
# epochs를 1로 설정하여 빠른 테스트
results = model.train(
    data='coco128.yaml',
    epochs=1,
    imgsz=640,
    batch=8,
    name='yolo_test_train'
)

print("Training completed!")
print(f"Results saved in: runs/detect/yolo_test_train")
```

실행:

```bash
python test_yolo_training.py
```

**정상 출력 예시**:
```
Downloading COCO128 dataset...
Ultralytics YOLOv8.0.200 🚀 Python-3.10.13 torch-2.1.0+cu118 CUDA:0 (NVIDIA GeForce RTX 3060, 12288MiB)

      Epoch    GPU_mem   box_loss   cls_loss   dfl_loss  Instances       Size
        1/1      3.45G      1.235      1.876      1.145        128        640: 100%|██| 16/16

Speed: 0.2ms preprocess, 15.3ms inference, 0.0ms loss, 1.5ms postprocess per image
Results saved to runs/detect/yolo_test_train
```

### 6-2. 학습 결과 확인

```bash
# TensorBoard로 학습 과정 확인 (선택)
pip install tensorboard
tensorboard --logdir runs/detect/yolo_test_train

# 결과 이미지 확인
ls runs/detect/yolo_test_train/
# 출력: weights/ train_batch0.jpg val_batch0_labels.jpg results.png 등
```

---

## Step 7: 환경 설정 파일 저장

### 7-1. requirements.txt 생성

```bash
cd ~/work_project
pip freeze > requirements.txt
```

### 7-2. 핵심 패키지만 포함한 requirements.txt 생성

`requirements.txt` 파일 수동 작성:

```txt
# 딥러닝 프레임워크
torch>=2.0.0
torchvision>=0.15.0

# YOLO v11l (이중 모델 아키텍처)
ultralytics>=8.0.0

# 컴퓨터 비전
opencv-python-headless>=4.8.0
pillow>=10.0.0

# 데이터 처리 및 시각화
matplotlib>=3.7.0
pandas>=2.0.0
seaborn>=0.12.0
numpy>=1.24.0

# 설정 및 유틸리티
pyyaml>=6.0
scipy>=1.10.0
tensorboard>=2.13.0

# Flask 서버 (추론 서버)
flask>=3.0.0
flask-cors>=4.0.0

# 데이터베이스
pymysql>=1.1.0

# 라즈베리파이 GPIO (라즈베리파이에서만)
# RPi.GPIO>=0.7.1  # 주석 처리 (라즈베리파이에서 설치)
```

**참고**:
- ⚠️ **Anomalib은 제외**: 이중 YOLO 모델 아키텍처에서는 이상 탐지 라이브러리 불필요
- RTX 4080 Super (16GB VRAM)로 두 모델 동시 로드 가능 (~8GB VRAM 사용)

---

## Step 8: 다양한 YOLO 모델 크기 테스트

### 8-1. 모델 크기별 비교

```python
from ultralytics import YOLO
import time

models = ['yolov8n.pt', 'yolov8s.pt', 'yolov8m.pt']

for model_name in models:
    print(f"\n테스트 중: {model_name}")
    model = YOLO(model_name)

    # 추론 속도 측정
    start = time.time()
    results = model('test_images/bus.jpg')
    end = time.time()

    print(f"추론 시간: {(end - start) * 1000:.2f}ms")
    print(f"검출된 객체 수: {len(results[0].boxes)}")
```

**예상 출력**:
```
테스트 중: yolov8n.pt
추론 시간: 15.23ms
검출된 객체 수: 5

테스트 중: yolov8s.pt
추론 시간: 22.45ms
검출된 객체 수: 5

테스트 중: yolov8m.pt
추론 시간: 35.67ms
검출된 객체 수: 6
```

### 8-2. 모델 크기별 특징

| 모델 | 파라미터 수 | mAP | 속도 (RTX 4080 Super) | VRAM (추론) | 권장 용도 |
|------|------------|-----|----------------------|------------|-----------|
| YOLOv8n | 3.2M | 37.3% | 250+ FPS | <1GB | 실시간 추론, 임베디드 |
| YOLOv8s | 11.2M | 44.9% | 180+ FPS | 1-2GB | 균형잡힌 성능 |
| YOLOv8m | 25.9M | 50.2% | 130+ FPS | 2-4GB | 정확도 우선 |
| **YOLOv11l** | **43.7M** | **52.9%** | **90+ FPS** | **3-5GB** | **고성능 PCB 검사** ⭐ |
| YOLOv8x | 68.2M | 53.9% | 70+ FPS | 4-6GB | 최고 정확도 필요 |

**PCB 프로젝트 권장 (RTX 4080 Super 기준)**:
- **YOLOv11l (Large)** - 정확도와 속도의 최적 균형 ⭐⭐⭐⭐⭐
- YOLOv8x (Extra Large) - 최고 정확도 필요 시 ⭐⭐⭐⭐

**이중 모델 아키텍처 고려사항**:
- RTX 4080 Super (16GB VRAM)로 **두 모델 동시 로드 가능**
- Component Model (YOLOv11l): ~5-6GB VRAM
- Solder Model (YOLOv11l): ~4-5GB VRAM
- **총 VRAM 사용량**: ~8GB (여유 충분)

**이유**:
- RTX 4080 Super (16GB VRAM)로 실시간 처리 충분
- 원격 연결 시에도 100-200ms 처리 (목표 300ms 대비 충분한 여유)
- 양면 동시 검사 + 병렬 추론: 80-100ms/request 달성
- 작은 납땜 불량(브릿지, 크랙)도 높은 정확도로 검출
- 디팔렛타이저 분류 시간 (2.5초) 내에 충분히 처리 완료

---

## Step 9: 문제 해결 (Troubleshooting)

### 문제 1: `nvidia-smi` 명령어가 작동하지 않음

**원인**: Windows에 NVIDIA 드라이버가 설치되지 않았거나 버전이 낮음

**해결**:
1. Windows에서 최신 NVIDIA GPU 드라이버 설치
2. WSL2 재시작: `wsl --shutdown` (Windows PowerShell에서 실행)
3. WSL2 다시 시작

### 문제 2: `torch.cuda.is_available()` 이 False 반환

**원인**: PyTorch가 CPU 버전으로 설치됨

**해결**:
```bash
# 기존 PyTorch 제거
pip uninstall torch torchvision torchaudio

# GPU 버전 재설치
pip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cu118
```

### 문제 3: 메모리 부족 오류

**원인**: GPU 메모리 또는 RAM 부족

**해결**:
```python
# batch size 줄이기
model.train(data='coco128.yaml', epochs=1, batch=4)  # 기본 8에서 4로 감소

# 이미지 크기 줄이기
model.train(data='coco128.yaml', epochs=1, imgsz=416)  # 기본 640에서 416으로 감소
```

### 문제 4: WSL2에서 이미지 시각화가 안 됨

**원인**: WSL2는 GUI를 직접 지원하지 않음

**해결**:
1. Windows에 VcXsrv 또는 X410 설치 (X Server)
2. 또는 결과를 이미지 파일로 저장 후 Windows에서 확인:
```python
results[0].save('result.jpg')  # WSL 내에서 실행
# Windows 탐색기에서 \\wsl$\Ubuntu\home\사용자명\work_project\result.jpg 확인
```

### 문제 5: `wget` 또는 `curl` 명령어가 없음

**해결**:
```bash
sudo apt update
sudo apt install wget curl -y
```

---

## Step 10: 다음 단계 준비

### 체크리스트 확인

- [ ] PyTorch 설치 완료 및 CUDA 작동 확인
- [ ] Ultralytics YOLO 설치 완료
- [ ] 사전 학습 모델로 이미지 추론 성공
- [ ] COCO128 데이터로 학습 테스트 성공
- [ ] 다양한 모델 크기 테스트 완료
- [ ] requirements.txt 생성 완료

### 다음 Phase 준비

Phase 1이 완료되면 다음 단계로 진행:

1. **Phase 2 시작**: PCB 불량 **자체 데이터셋** 수집 (이중 모델)
   - `Dataset_Guide.md` 참고 ⭐
   - 실제 웹캠으로 부품/납땜 이미지를 촬영 (각 200-300장)
   - Roboflow에 업로드하여 라벨링 및 증강 (3배 이상)
   - YOLO 형식으로 Export 후 `data/custom_component`, `data/custom_solder`에 배치

2. **프로젝트 폴더 구조 생성** (이중 모델):
```bash
cd ~/work_project

# Component Model 데이터
mkdir -p data/custom_component/images/{train,valid,test}
mkdir -p data/custom_component/labels/{train,valid,test}

# Solder Model 데이터
mkdir -p data/custom_solder/images/{train,valid,test}
mkdir -p data/custom_solder/labels/{train,valid,test}

# 모델 저장 폴더
mkdir -p models/component_model models/solder_model

# 결과 및 로그
mkdir -p results logs
```

3. **이중 모델 학습 준비**:
   - `YOLO_Training_Guide.md` 참고
   - Component Model 학습 (custom_component, 프로젝트 클래스)
   - Solder Model 학습 (custom_solder, 프로젝트 클래스)
   - 각 모델을 독립적으로 학습 및 평가

---

## 유용한 YOLO v11l 명령어 모음

### 모델 추론

```bash
# 단일 이미지
yolo detect predict model=yolov8n.pt source=image.jpg

# 폴더 내 모든 이미지
yolo detect predict model=yolov8n.pt source=images/

# 비디오
yolo detect predict model=yolov8n.pt source=video.mp4

# 웹캠
yolo detect predict model=yolov8n.pt source=0

# YouTube 비디오
yolo detect predict model=yolov8n.pt source='https://youtube.com/watch?v=...'
```

### 모델 학습

```bash
# 기본 학습
yolo detect train data=coco128.yaml model=yolov8n.pt epochs=100 imgsz=640

# GPU 지정 (멀티 GPU 사용 시)
yolo detect train data=coco128.yaml model=yolov8n.pt device=0,1

# Resume (중단된 학습 재개)
yolo detect train resume model=runs/detect/train/weights/last.pt
```

### 모델 검증

```bash
yolo detect val model=yolov8n.pt data=coco128.yaml
```

### 모델 내보내기 (ONNX, TensorRT 등)

```bash
# ONNX
yolo export model=yolov8n.pt format=onnx

# TensorRT
yolo export model=yolov8n.pt format=engine
```

---

## 참고 자료

### 공식 문서
- [Ultralytics YOLO Docs](https://docs.ultralytics.com/)
- [YOLO Quickstart](https://docs.ultralytics.com/quickstart/)
- [YOLO Train Guide](https://docs.ultralytics.com/modes/train/)

### 튜토리얼 영상
- [Ultralytics YouTube Channel](https://www.youtube.com/@Ultralytics)

### GitHub
- [Ultralytics GitHub](https://github.com/ultralytics/ultralytics)
- [YOLO Issues](https://github.com/ultralytics/ultralytics/issues)

---

**작성일**: 2025-10-22
**수정일**: 2025-10-31
**버전**: 2.0 (이중 YOLO 모델 아키텍처)
**다음 단계**: `Dataset_Guide.md` (FPIC-Component + SolDef_AI)

**주요 변경사항 (v2.0)**:
- ✅ 이중 YOLO 모델 아키텍처 적용
- ✅ Anomalib 제거 (YOLO v11l 두 모델만 사용)
- ✅ Component Model + Solder Model 독립 학습
- ✅ VRAM 요구사항 업데이트 (~8GB for dual models)
- ✅ Flask 서버 결과 융합 방식 설명 추가
