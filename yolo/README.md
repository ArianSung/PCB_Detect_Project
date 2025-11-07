# YOLO 작업 디렉토리

**AI 모델 팀** 전용 YOLO v11l 관련 모든 자료를 모아둔 폴더입니다.

---

## 📁 디렉토리 구조

```
yolo/
├── datasets/           # YOLO 학습/테스트용 데이터셋
│   └── coco128/        # COCO128 샘플 데이터셋
│
├── runs/               # YOLO 학습 및 추론 결과
│   └── detect/         # 탐지 결과 (학습, 검증, 예측)
│
├── test_images/        # 테스트용 이미지 파일
│   └── bus.jpg         # 샘플 테스트 이미지
│
├── tests/              # YOLO 테스트 스크립트 (Phase 1)
│   ├── test_yolo_inference.py     # 추론 성능 테스트
│   ├── test_yolo_training.py      # 학습 테스트
│   ├── test_yolo_detailed.py      # 상세 성능 분석
│   ├── test_yolo_webcam.py        # 웹캠 실시간 테스트
│   ├── test_yolo_video.py         # 비디오 테스트
│   ├── models_backup/             # 백업 모델 파일 (yolov8n.pt, yolo11n.pt)
│   ├── PHASE1_TEST_RESULTS.md     # Phase 1 테스트 결과
│   ├── YOLO11_vs_YOLOv8.md        # YOLO11 vs YOLOv8 비교 분석
│   └── README.md                  # 테스트 가이드
│
└── README.md           # 이 파일
```

---

## 🎯 주요 용도

### 1. datasets/ - 데이터셋 저장
- **COCO128**: YOLO 기본 테스트용 데이터셋 (128개 이미지)
- **PCB 데이터셋**: Phase 2에서 추가 예정 (`datasets/pcb_defects/`)

**⚠️ 주의**:
- `datasets/` 폴더는 `.gitignore`에 포함되어 Git에 올라가지 않습니다.
- 대용량 데이터셋은 팀 내부 공유 스토리지 사용 권장

### 2. runs/ - 학습 결과 저장
- YOLO 모델 학습 시 자동 생성되는 결과 폴더
- 학습된 모델 가중치 (`.pt` 파일)
- 학습 그래프, 검증 결과, 예측 이미지 등

**⚠️ 주의**:
- `runs/` 폴더는 `.gitignore`에 포함되어 Git에 올라가지 않습니다.
- 최종 학습 모델은 `/models/yolo/final/`로 복사하여 Git에 커밋

### 3. test_images/ - 테스트 이미지
- YOLO 추론 테스트용 샘플 이미지
- 새로운 테스트 이미지 추가 가능

### 4. tests/ - YOLO 테스트 스크립트
- Phase 1에서 작성한 YOLO 성능 테스트 코드
- 모델 학습 및 추론 검증용

---

## 🚀 사용 방법

### 1. YOLO 추론 테스트

```bash
# 기본 추론 테스트
python test_yolo_inference.py

# 상세 성능 분석
python test_yolo_detailed.py

# 웹캠 실시간 테스트
python test_yolo_webcam.py
```

### 2. YOLO 학습

```bash
# 프로젝트 루트에서 실행
python yolo/train_yolo.py --config configs/yolo_config.yaml

# 학습 결과 확인
ls yolo/runs/detect/train/weights/
# best.pt, last.pt 생성됨
```

### 3. 학습된 모델 배포

```bash
# 최종 모델을 프로젝트 models/ 폴더로 복사
cp yolo/runs/detect/train/weights/best.pt models/yolo/final/yolo_best.pt

# Git에 커밋 (선택 - 용량이 큰 경우 Git LFS 사용)
git add models/yolo/final/yolo_best.pt
git commit -m "feat: Add trained YOLO model (Phase 3)"
```

---

## 📊 Phase 1 테스트 결과

**YOLOv8n 성능** (RTX 4080 Super):
- 추론 시간: 14.60ms
- FPS: 68.48
- 신뢰도: 0.656

**YOLO11n 성능**:
- 추론 시간: 24.21ms
- FPS: 41.31
- 신뢰도: 0.837

**권장 모델**: YOLOv11l (Large) - 정확도와 효율성의 최적 균형

자세한 내용: [tests/PHASE1_TEST_RESULTS.md](tests/PHASE1_TEST_RESULTS.md)

---

## 🔗 관련 문서

- [Phase 1 YOLO 환경 구축](../docs/Phase1_YOLO_Setup.md)
- [데이터셋 가이드](../docs/Dataset_Guide.md)
- [프로젝트 전체 구조](../docs/Project_Structure.md)

---

## ⚠️ 주의사항

1. **용량 관리**:
   - `datasets/`와 `runs/` 폴더는 용량이 큽니다.
   - 정기적으로 오래된 학습 결과 삭제 권장

2. **Git 관리**:
   - 이 폴더의 대부분 내용은 `.gitignore`에 포함
   - Git에 올라가는 것: `tests/` 폴더의 스크립트와 문서만
   - Git에 안 올라가는 것: `datasets/`, `runs/`, `test_images/`

3. **팀 협업**:
   - 이 폴더는 **AI 모델 팀 전용**입니다.
   - 다른 팀은 최종 모델 파일(`/models/yolo/final/*.pt`)만 사용

---

**마지막 업데이트**: 2025-10-25
**담당 팀**: AI 모델 팀
