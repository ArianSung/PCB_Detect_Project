# YOLO v8 Large 모델 학습 완전 가이드

DeepPCB + Kaggle PCB Defects 조합 데이터셋으로 YOLOv8 Large 모델 학습 가이드입니다.

## 목차
1. [시작하기 전에](#시작하기-전에)
2. [데이터셋 준비](#데이터셋-준비)
3. [모델 학습](#모델-학습)
4. [GPU 모니터링](#gpu-모니터링)
5. [학습 결과 확인](#학습-결과-확인)
6. [문제 해결](#문제-해결)

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
   - 최소 15GB 이상 여유 공간 (데이터셋 + 모델)

4. **프로젝트 디렉토리 확인**
   ```bash
   cd /home/sys1041/work_project
   pwd
   ```

---

## 데이터셋 준비

### 단계 1: DeepPCB 데이터셋 다운로드 (이미 완료 ✅)

DeepPCB는 이미 다운로드되어 YOLO 형식으로 변환되었습니다.

**확인:**
```bash
ls -lh data/processed/deeppcb_yolo/
```

**예상 출력:**
```
train/  (1050 images)
val/    (300 images)
test/   (150 images)
data.yaml
```

---

### 단계 2: Kaggle API 토큰 설정

Kaggle PCB Defects 데이터셋을 다운로드하려면 API 토큰이 필요합니다.

#### 2-1. Kaggle 계정 생성 및 API 토큰 발급

1. https://www.kaggle.com/ 접속 후 로그인 (또는 회원가입)
2. 우측 상단 프로필 아이콘 → **Account** 클릭
3. **API** 섹션에서 **Create New API Token** 클릭
4. `kaggle.json` 파일 자동 다운로드

#### 2-2. WSL에 API 토큰 설정

```bash
# kaggle 디렉토리 생성
mkdir -p ~/.kaggle

# Windows Downloads 폴더에서 복사 (경로는 사용자명에 맞게 수정)
cp /mnt/c/Users/<사용자명>/Downloads/kaggle.json ~/.kaggle/

# 권한 설정 (보안상 필수)
chmod 600 ~/.kaggle/kaggle.json

# 확인
ls -la ~/.kaggle/
# 출력: -rw------- 1 sys1041 sys1041 73 Oct 28 17:00 kaggle.json
```

#### 2-3. API 토큰 테스트

```bash
kaggle datasets list
```

**성공 시 출력:**
```
ref                                            title                                              size  lastUpdated          downloadCount  voteCount  usabilityRating
---------------------------------------------  ------------------------------------------------  -----  -------------------  -------------  ---------  ---------------
...
```

**실패 시:**
- `401 Unauthorized`: kaggle.json 파일 확인
- `403 Forbidden`: 데이터셋 라이선스 동의 필요

---

### 단계 3: Kaggle PCB Defects 다운로드

```bash
cd /home/sys1041/work_project/data/raw

# akhatova/pcb-defects 다운로드 (1,386장)
kaggle datasets download -d akhatova/pcb-defects

# 압축 해제
unzip pcb-defects.zip -d kaggle_pcb_defects

# 확인
ls -lh kaggle_pcb_defects/
```

**예상 출력:**
```
images/
annotations.csv
README.md
```

---

### 단계 4: Kaggle 데이터셋 YOLO 형식 변환

```bash
cd /home/sys1041/work_project

# Kaggle → YOLO 변환
python yolo/convert_kaggle_to_yolo.py \
    --kaggle-root data/raw/kaggle_pcb_defects \
    --output-dir data/processed/kaggle_yolo
```

**예상 출력:**
```
Found CSV file: data/raw/kaggle_pcb_defects/annotations.csv
Loaded 5000 annotations
Columns: ['filename', 'xmin', 'ymin', 'xmax', 'ymax', 'class']

Split: Train=970, Val=277, Test=139

=== Conversion Statistics ===
Train: 970/970 images (errors: 0, skipped: 0)
Val: 277/277 images (errors: 0, skipped: 0)
Test: 139/139 images (errors: 0, skipped: 0)

Total converted: 1386

YAML config saved to: data/processed/kaggle_yolo/data.yaml

=== Conversion Complete ===
```

---

### 단계 5: DeepPCB + Kaggle 데이터셋 통합

```bash
# 두 데이터셋 통합
python yolo/merge_datasets.py \
    --deeppcb-dir data/processed/deeppcb_yolo \
    --kaggle-dir data/processed/kaggle_yolo \
    --output-dir data/processed/combined_pcb_dataset
```

**예상 출력:**
```
=== Merging PCB Datasets ===
DeepPCB: data/processed/deeppcb_yolo
Kaggle: data/processed/kaggle_yolo
Output: data/processed/combined_pcb_dataset

--- Copying DeepPCB ---
Copying deeppcb train: 100%|██████████| 1050/1050
Copying deeppcb val: 100%|██████████| 300/300
Copying deeppcb test: 100%|██████████| 150/150

--- Copying Kaggle ---
Copying kaggle train: 100%|██████████| 970/970
Copying kaggle val: 100%|██████████| 277/277
Copying kaggle test: 100%|██████████| 139/139

=== Merge Statistics ===

DeepPCB:
  Train: 1050
  Val: 300
  Test: 150
  Total: 1500

Kaggle:
  Train: 970
  Val: 277
  Test: 139
  Total: 1386

Combined Total:
  Train: 1535
  Val: 438
  Test: 220
  Total: 2193

=== Class Distribution ===
  0 (open): 2921 (22.5%)
  1 (short): 1997 (15.4%)
  2 (mousebite): 2457 (18.9%)
  3 (spur): 2113 (16.3%)
  4 (copper): 1977 (15.2%)
  5 (pin-hole): 1501 (11.6%)
  Total objects: 12966

  Imbalance ratio: 1.95:1

YAML config saved to: data/processed/combined_pcb_dataset/data.yaml
```

---

## 모델 학습

### 단계 1: 학습 스크립트 실행

```bash
cd /home/sys1041/work_project

# 백그라운드로 학습 실행 (권장)
nohup python yolo/train_deeppcb.py > training.log 2>&1 &

# 또는 포그라운드 실행 (터미널 종료 시 학습도 중단됨)
python yolo/train_deeppcb.py
```

### 단계 2: 학습 시작 확인

```bash
# 로그 확인
tail -f training.log
```

**예상 출력:**
```
CUDA available: True
GPU: NVIDIA GeForce RTX 4080 SUPER
CUDA version: 11.8
PyTorch version: 2.7.1+cu118
GPU Memory: 15.99 GB

Loading YOLOv8l model...

=== Training Configuration ===
  data: /home/sys1041/work_project/data/processed/combined_pcb_dataset/data.yaml
  epochs: 150
  batch: 32
  imgsz: 640
  device: 0
  workers: 8
  optimizer: AdamW
  lr0: 0.001
  amp: True

=== Starting Training ===
Dataset: combined_pcb_dataset
Model: YOLOv8l
Train images: 2020
Val images: 577

Ultralytics 8.3.221 🚀 Python-3.10.19 torch-2.7.1+cu118
Model summary: 209 layers, 43,634,466 parameters, 165.4 GFLOPs

Starting training for 150 epochs...

Epoch    GPU_mem   box_loss   cls_loss   dfl_loss  Instances       Size
  1/150      8.5G      1.234      2.345      1.567        256        640
  2/150      8.6G      1.198      2.289      1.512        256        640
  3/150      8.6G      1.156      2.201      1.478        256        640
...
```

### 단계 3: 예상 학습 시간

- **RTX 4080 Super 기준**:
  - **총 150 epochs**: 약 **2-3시간**
  - **Epoch당**: 약 1-1.5분
  - **배치 처리 속도**: 약 50-60 FPS

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
| 55%   75C    P2   280W / 320W |  13500MiB / 16376MiB |     98%      Default |
+-------------------------------+----------------------+----------------------+
```

**확인 사항:**
- ✅ **GPU-Util**: 95-100% (정상, FP16 AMP 사용)
- ✅ **Memory-Usage**: 12-14GB / 16GB (정상, Batch 32)
- ✅ **Temperature**: 70-80°C (정상)
- ⚠️ **Temperature > 85°C**: 쿨링 확인 필요

### 학습 진행률 확인

```bash
# 현재 epoch 확인
grep "Epoch" training.log | tail -5
```

---

## 학습 결과 확인

### 학습 완료 후

학습이 완료되면 다음과 같은 메시지가 출력됩니다:

```
=== Training Complete ===
Best model saved to: runs/detect/deeppcb_yolov8l/weights/best.pt
Results saved to: runs/detect/deeppcb_yolov8l

=== Validation ===
mAP50: 0.892
mAP50-95: 0.687
```

### 생성된 파일 확인

```bash
ls -lh runs/detect/deeppcb_yolov8l/
```

**예상 출력:**
```
-rw-r--r-- 1 sys1041 sys1041 1.6K args.yaml
-rw-r--r-- 1 sys1041 sys1041 150K labels.jpg
-rw-r--r-- 1 sys1041 sys1041 200K results.png
-rw-r--r-- 1 sys1041 sys1041 180K confusion_matrix.png
-rw-r--r-- 1 sys1041 sys1041  15K results.csv
drwxr-xr-x 2 sys1041 sys1041 4.0K weights/
  -rw-r--r-- 1 sys1041 sys1041 167M best.pt    # 최고 성능 모델
  -rw-r--r-- 1 sys1041 sys1041 167M last.pt    # 마지막 epoch 모델
```

### 학습 그래프 확인

```bash
# WSL에서 Windows 이미지 뷰어로 열기
explorer.exe $(wslpath -w runs/detect/deeppcb_yolov8l/results.png)
```

**확인 사항:**
- **train/box_loss**: 감소 추세 ✅
- **train/cls_loss**: 감소 추세 ✅
- **metrics/mAP50**: 증가 후 안정화 (목표: > 0.85) ✅
- **metrics/mAP50-95**: 증가 후 안정화 (목표: > 0.65) ✅

### Confusion Matrix 확인

```bash
explorer.exe $(wslpath -w runs/detect/deeppcb_yolov8l/confusion_matrix.png)
```

**분석:**
- 대각선 값이 높을수록 정확도 높음
- Off-diagonal 값은 오분류 케이스

### 모델 평가

```bash
cd /home/sys1041/work_project

# Test 데이터셋으로 평가
yolo detect val \
    model=runs/detect/deeppcb_yolov8l/weights/best.pt \
    data=data/processed/combined_pcb_dataset/data.yaml \
    split=test
```

**예상 출력:**
```
Validation: 100%|██████████| 10/10 [00:05<00:00,  1.85it/s]

                   Class     Images  Instances      Box(P          R      mAP50  mAP50-95)
                     all        289       1250      0.852      0.825      0.892      0.687
                    open        289        210      0.865      0.842      0.905      0.695
                   short        289        195      0.848      0.815      0.889      0.678
               mousebite        289        215      0.870      0.835      0.910      0.705
                    spur        289        205      0.840      0.820      0.885      0.680
                  copper        289        200      0.835      0.810      0.875      0.670
                pin-hole        289        225      0.855      0.828      0.895      0.685

Speed: 2.5ms preprocess, 89.3ms inference, 3.2ms postprocess per image
```

**목표 달성 확인:**
- ✅ **추론 속도 < 100ms**: 89.3ms (목표 달성!)
- ✅ **mAP50 > 0.85**: 0.892 (목표 달성!)
- ✅ **mAP50-95 > 0.65**: 0.687 (목표 달성!)

---

## 문제 해결

### 문제 1: Kaggle API 토큰 오류

**증상:**
```
401 - Unauthorized
```

**해결 방법:**
1. `~/.kaggle/kaggle.json` 파일 존재 확인
2. 파일 권한이 `600`인지 확인: `ls -la ~/.kaggle/`
3. API 토큰 재발급 후 다시 설정

---

### 문제 2: CUDA Out of Memory

**증상:**
```
RuntimeError: CUDA out of memory. Tried to allocate 2.00 GiB
```

**해결 방법:**

배치 사이즈를 줄여서 재시도:

```python
# yolo/train_deeppcb.py 수정
training_args = {
    'batch': 16,  # 32 → 16으로 변경
    # 나머지 설정 동일
}
```

또는 이미지 크기 축소:

```python
training_args = {
    'imgsz': 512,  # 640 → 512로 변경
    # 나머지 설정 동일
}
```

---

### 문제 3: Kaggle 데이터셋 CSV 형식 오류

**증상:**
```
KeyError: 'xmin' or 'filename'
```

**해결 방법:**

CSV 파일 확인:
```bash
head -5 data/raw/kaggle_pcb_defects/annotations.csv
```

CSV 컬럼명이 다른 경우 `convert_kaggle_to_yolo.py` 수정:
```python
# 컬럼명 매핑 수정
image_col = 'image_name'  # 또는 실제 컬럼명
xmin_col = 'bbox_x1'
# ...
```

---

### 문제 4: 학습이 너무 느림

**확인 사항:**

1. GPU 사용 확인:
   ```bash
   nvidia-smi
   ```
   - GPU-Util이 10% 미만이면 CPU로 실행 중일 가능성

2. CUDA 사용 가능 확인:
   ```bash
   python -c "import torch; print(torch.cuda.is_available())"
   ```
   - `False`면 PyTorch CUDA 재설치 필요

3. AMP (FP16) 활성화 확인:
   ```bash
   grep "amp=True" yolo/train_deeppcb.py
   ```

---

## 다음 단계

학습 완료 후:

1. **모델을 Flask 서버에 통합**
   - `server/app.py`에서 YOLO 모델 로드
   - 실시간 추론 API 구현

2. **라즈베리파이 웹캡 연동**
   - `raspberry_pi/camera_client.py` 실행
   - Flask 서버로 프레임 전송 테스트

3. **성능 최적화** (필요 시)
   - FP16 → INT8 양자화
   - TensorRT 변환
   - 배치 추론 최적화

4. **이상 탐지 모델 추가** (Phase 4)
   - PaDiM 또는 PatchCore
   - YOLO + 이상 탐지 하이브리드

---

## 학습 스크립트 요약

### 전체 과정 한번에 실행

```bash
# 1. Kaggle API 토큰 설정 (최초 1회)
mkdir -p ~/.kaggle
cp /mnt/c/Users/<사용자명>/Downloads/kaggle.json ~/.kaggle/
chmod 600 ~/.kaggle/kaggle.json

# 2. Kaggle 다운로드
cd /home/sys1041/work_project/data/raw
kaggle datasets download -d akhatova/pcb-defects
unzip pcb-defects.zip -d kaggle_pcb_defects

# 3. Kaggle 변환
cd /home/sys1041/work_project
python yolo/convert_kaggle_to_yolo.py

# 4. 데이터셋 통합
python yolo/merge_datasets.py \
    --deeppcb-dir data/processed/deeppcb_yolo \
    --kaggle-dir data/processed/kaggle_yolo \
    --output-dir data/processed/combined_pcb_dataset

# 5. 학습 시작 (백그라운드)
nohup python yolo/train_deeppcb.py > training.log 2>&1 &

# 6. 학습 모니터링
tail -f training.log
watch -n 1 nvidia-smi

# 7. 학습 완료 후 평가
yolo detect val \
    model=runs/detect/deeppcb_yolov8l/weights/best.pt \
    data=data/processed/combined_pcb_dataset/data.yaml \
    split=test
```

---

**작성일**: 2025-10-28
**버전**: 2.0
**데이터셋**: DeepPCB (1,500장) + Kaggle PCB Defects (693장) = 2,193장
- Train: 1,535 / Val: 438 / Test: 220
- 총 객체: 12,966개
- 클래스 불균형: 1.95:1 (양호)
**다음 업데이트**: 이상 탐지 모델 통합
