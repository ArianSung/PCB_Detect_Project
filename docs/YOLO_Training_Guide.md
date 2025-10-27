# YOLO v8 Large 모델 학습 완전 가이드

PCB 불량 검사를 위한 YOLOv8 Large 모델 학습 가이드입니다.

## 목차
1. [시작하기 전에](#시작하기-전에)
2. [데이터셋 다운로드](#데이터셋-다운로드)
3. [모델 학습](#모델-학습)
4. [GPU 모니터링](#gpu-모니터링)
5. [학습 결과 확인](#학습-결과-확인)
6. [모델 평가](#모델-평가)
7. [문제 해결](#문제-해결)

---

## 시작하기 전에

### 필수 조건

1. **Conda 환경 활성화**
   ```bash
   conda activate pcb_defect
   ```

2. **GPU 확인**
   ```bash
   nvidia-smi
   ```
   - GPU: NVIDIA RTX 4080 Super (16GB VRAM)
   - CUDA 버전: 11.8 이상

3. **디스크 공간**
   - 최소 10GB 이상 여유 공간

---

## 데이터셋 다운로드

### 단계 1: Roboflow 계정 생성

1. https://roboflow.com/ 접속
2. 회원가입 (무료)
3. 로그인 후 **Settings → API → Private API Key** 복사

### 단계 2: 데이터셋 다운로드 스크립트 실행

```bash
# 프로젝트 루트에서 실행
bash scripts/download_pcb_dataset.sh
```

**실행 화면:**
```
======================================
PCB 불량 데이터셋 다운로드
======================================
프로젝트 루트: /home/sys1041/work_project

[1/5] Roboflow 라이브러리 확인...
✓ Roboflow 이미 설치됨

[2/5] 데이터 폴더 생성...

[3/5] Roboflow에서 PCB 데이터셋 다운로드...
주의: Roboflow API 키가 필요합니다.

API 키 받는 방법:
1. https://roboflow.com/ 회원가입 (무료)
2. 로그인 후 Settings → API → Private API Key 복사

Roboflow API 키를 입력하세요: [여기에 API 키 붙여넣기]
```

### 단계 3: 다운로드 확인

```bash
# 다운로드된 데이터 확인
ls -lh data/raw/pcb_defects/
```

**예상 출력:**
```
train/
  images/  (약 500-1000장)
  labels/
valid/
  images/  (약 100-200장)
  labels/
test/
  images/  (약 100-200장)
  labels/
data.yaml
```

### 대안: 수동 다운로드

자동 다운로드 실패 시:

1. https://universe.roboflow.com/roboflow-100/pcb-defects 방문
2. **Download** 버튼 클릭
3. **YOLO v8** 형식 선택
4. 다운로드 후 압축 해제
5. `data/raw/pcb_defects/` 폴더에 복사

---

## 모델 학습

### 단계 1: 학습 스크립트 실행

```bash
# 프로젝트 루트에서 실행
bash scripts/train_yolo.sh
```

### 단계 2: 학습 파라미터 확인

**실행 화면:**
```
======================================
YOLO v8 Large 모델 학습 시작
======================================
프로젝트 루트: /home/sys1041/work_project

[1/4] 환경 확인...
✓ Conda 환경: pcb_defect
✓ CUDA 사용 가능
NVIDIA GeForce RTX 4080 Super, 16376 MiB, 15234 MiB

[2/4] 데이터셋 확인...
✓ 데이터셋 설정 파일: data/pcb_defects_roboflow.yaml

[3/4] 학습 파라미터 설정...
  모델: yolov8l.pt
  에포크: 150
  배치 사이즈: 32
  이미지 크기: 640
  디바이스: GPU 0

학습을 시작하시겠습니까? (y/n): y
```

### 단계 3: 학습 진행 확인

학습이 시작되면 다음과 같은 화면이 표시됩니다:

```
[4/4] 학습 시작...
======================================

주의사항:
  - 학습은 약 1-2시간 소요됩니다
  - 학습 중 GPU 사용률을 모니터링하려면:
    watch -n 1 nvidia-smi
  - 학습 중단: Ctrl+C

======================================

Downloading yolov8l.pt...
100%|██████████| 83.7M/83.7M [00:05<00:00, 15.2MB/s]

Epoch    GPU_mem   box_loss   cls_loss   dfl_loss  Instances       Size
  1/150      8.2G      1.234      2.345      1.567        128        640
  2/150      8.3G      1.198      2.289      1.512        128        640
  3/150      8.3G      1.156      2.201      1.478        128        640
...
```

---

## GPU 모니터링

### 새 터미널 창에서 실행

학습 중 GPU 사용률을 실시간으로 모니터링:

```bash
# 1초마다 GPU 상태 확인
watch -n 1 nvidia-smi
```

**예상 출력:**
```
Every 1.0s: nvidia-smi

+-----------------------------------------------------------------------------+
| NVIDIA-SMI 525.147.05   Driver Version: 525.147.05   CUDA Version: 12.0   |
|-------------------------------+----------------------+----------------------+
| GPU  Name        Persistence-M| Bus-Id        Disp.A | Volatile Uncorr. ECC |
| Fan  Temp  Perf  Pwr:Usage/Cap|         Memory-Usage | GPU-Util  Compute M. |
|===============================+======================+======================|
|   0  NVIDIA GeForce ...  Off  | 00000000:01:00.0 Off |                  N/A |
| 45%   72C    P2   250W / 320W |  12234MiB / 16376MiB |     98%      Default |
+-------------------------------+----------------------+----------------------+
```

**확인 사항:**
- ✅ **GPU-Util**: 95-100% (정상)
- ✅ **Memory-Usage**: 10-14GB / 16GB (정상)
- ✅ **Temperature**: 70-85°C (정상)
- ⚠️ **Temperature > 85°C**: 쿨링 확인 필요

---

## 학습 결과 확인

### 학습 완료 후

```
======================================
학습 완료!
======================================

결과 폴더: yolo/runs/train/pcb_defect
생성된 파일:
-rw-r--r-- 1 sys1041 sys1041 167M Oct 27 18:30 best.pt
-rw-r--r-- 1 sys1041 sys1041 167M Oct 27 18:30 last.pt

다음 단계:
  1. 학습 그래프 확인: yolo/runs/train/pcb_defect/results.png
  2. 모델 평가:
     python yolo/evaluate_yolo.py --model yolo/runs/train/pcb_defect/weights/best.pt --data data/pcb_defects_roboflow.yaml
```

### 학습 그래프 확인

```bash
# 이미지 뷰어로 열기
xdg-open yolo/runs/train/pcb_defect/results.png
```

**확인 사항:**
- **box_loss**: 감소 추세 ✅
- **cls_loss**: 감소 추세 ✅
- **mAP50**: 증가 추세 (목표: > 0.8) ✅
- **mAP50-95**: 증가 추세 (목표: > 0.6) ✅

### Confusion Matrix 확인

```bash
xdg-open yolo/runs/train/pcb_defect/confusion_matrix.png
```

---

## 모델 평가

### 평가 실행

```bash
python yolo/evaluate_yolo.py \
    --model yolo/runs/train/pcb_defect/weights/best.pt \
    --data data/pcb_defects_roboflow.yaml
```

### 예상 출력

```
모델 로드: yolo/runs/train/pcb_defect/weights/best.pt

평가 시작:
  - 데이터셋: data/pcb_defects_roboflow.yaml
  - 이미지 크기: 640
  - 디바이스: 0

Validation: 100%|██████████| 10/10 [00:05<00:00,  1.85it/s]

==================================================
평가 결과
==================================================

전체 성능:
  - mAP50-95: 0.6234
  - mAP50: 0.8456
  - mAP75: 0.7012
  - Precision: 0.8123
  - Recall: 0.7890

클래스별 성능:
  - cold_joint: AP=0.7234
  - solder_bridge: AP=0.6890
  - insufficient_solder: AP=0.6456
  - excess_solder: AP=0.7012
  - missing_component: AP=0.8234
  - misalignment: AP=0.7890
  - wrong_component: AP=0.6123
  - damaged_component: AP=0.5890
  - trace_damage: AP=0.6234
  - pad_damage: AP=0.5678
  - scratch: AP=0.7456

추론 속도:
  - Preprocess: 2.3ms
  - Inference: 85.6ms
  - Postprocess: 3.1ms
  - Total: 91.0ms
```

**목표 달성 확인:**
- ✅ **추론 속도 < 100ms**: 91.0ms (목표 달성!)
- ✅ **mAP50 > 0.8**: 0.8456 (목표 달성!)
- ✅ **RTX 4080 Super 스펙 적합**: VRAM 12GB 사용 (16GB 중)

---

## 문제 해결

### 문제 1: CUDA Out of Memory

**증상:**
```
RuntimeError: CUDA out of memory
```

**해결 방법:**
1. 배치 사이즈 줄이기:
   ```bash
   # scripts/train_yolo.sh 파일 수정
   BATCH=16  # 32 → 16
   ```

2. 또는 이미지 크기 줄이기:
   ```bash
   IMGSZ=512  # 640 → 512
   ```

### 문제 2: 데이터셋 다운로드 실패

**해결 방법:**
1. 수동 다운로드:
   - https://universe.roboflow.com/roboflow-100/pcb-defects
   - Download → YOLO v8 → 압축 해제
   - `data/raw/pcb_defects/` 에 복사

2. data.yaml 경로 수정:
   ```bash
   nano data/pcb_defects_roboflow.yaml
   ```
   ```yaml
   train: /home/sys1041/work_project/data/raw/pcb_defects/train/images
   val: /home/sys1041/work_project/data/raw/pcb_defects/valid/images
   test: /home/sys1041/work_project/data/raw/pcb_defects/test/images
   ```

### 문제 3: 학습이 너무 느림

**확인 사항:**
1. GPU 사용 확인:
   ```bash
   nvidia-smi
   ```

2. CPU 모드로 실행 중인 경우:
   ```bash
   # CUDA 설치 확인
   python -c "import torch; print(torch.cuda.is_available())"
   ```

---

## 다음 단계

학습 완료 후:

1. **모델을 Flask 서버에 통합**
   - `server/app.py` 수정
   - YOLO 모델 로드 및 추론 구현

2. **실시간 추론 테스트**
   - 라즈베리파이 웹캠 연동
   - 추론 속도 최적화

3. **이상 탐지 모델 추가** (선택)
   - PaDiM 또는 PatchCore
   - YOLO + 이상 탐지 하이브리드

---

**작성일**: 2025-10-27
**버전**: 1.0
**다음 업데이트**: 이상 탐지 모델 통합
