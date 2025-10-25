# Phase 1: YOLO 기본 테스트

이 폴더는 **Phase 1 환경 구축 및 YOLO v8 테스트** 과정에서 생성된 테스트 스크립트와 결과를 포함합니다.

---

## 📁 폴더 내용

### 테스트 스크립트

1. **test_yolo_inference.py**
   - 기본 이미지 추론 테스트
   - 결과 자동 저장
   - 사용법: `python test_yolo_inference.py`

2. **test_yolo_detailed.py** ⭐ 추천
   - 상세 성능 측정 (FPS, 지연시간)
   - 여러 모델 크기 비교 (n, s, m, l)
   - 검출 결과 분석
   - 사용법:
     ```bash
     python test_yolo_detailed.py              # 모든 모델 비교
     python test_yolo_detailed.py yolov8n.pt   # 특정 모델 테스트
     ```

3. **test_yolo_webcam.py**
   - 실시간 웹캠 추론
   - FPS 실시간 표시
   - 사용법: `python test_yolo_webcam.py [model] [camera_id]`
   - 참고: WSL에서는 웹캠 접근 제한될 수 있음

4. **test_yolo_video.py**
   - 비디오 파일 추론
   - 결과 비디오 저장
   - 사용법: `python test_yolo_video.py <video_file> [model]`

5. **test_yolo_training.py**
   - COCO128 데이터셋 학습 테스트
   - 1 epoch 빠른 테스트
   - 사용법: `python test_yolo_training.py`

### 문서

- **PHASE1_TEST_RESULTS.md**
  - Phase 1 테스트 결과 요약
  - 성능 측정 데이터
  - PCB 프로젝트 적용 분석

- **YOLO11_vs_YOLOv8.md** 🆕
  - YOLO11 소개 및 비교
  - YOLOv8 vs YOLO11 성능 비교
  - PCB 프로젝트 모델 선택 가이드

- **README.md** (이 파일)
  - 폴더 구조 및 사용 가이드

### 백업 모델

- **models_backup/** 📦
  - `yolov8n.pt` (6.3MB) - YOLOv8 Nano 모델
  - `yolo11n.pt` (5.4MB) - YOLO11 Nano 모델
  - 참고용으로 보관 (프로젝트에서는 Large 모델 사용)

---

## 🚀 빠른 시작

### 환경 활성화
```bash
conda activate pcb_defect
```

### 테스트 실행
```bash
# 상세 성능 측정 (권장)
cd tests/phase1_yolo_basic
python test_yolo_detailed.py

# 특정 모델 테스트 (백업 모델 사용 시)
python test_yolo_detailed.py models_backup/yolov8n.pt
python test_yolo_detailed.py models_backup/yolo11n.pt
```

---

## 📊 주요 테스트 결과

### YOLOv8n
- 평균 추론 시간: **14.60 ms**
- FPS: **68.48**
- Confidence: 0.656

### YOLO11n
- 평균 추론 시간: **24.21 ms**
- FPS: **41.31**
- Confidence: **0.837** (28% 향상)

**자세한 내용**: `PHASE1_TEST_RESULTS.md` 참조

---

## 🎯 PCB 프로젝트 권장 모델

### 1순위: **YOLOv8l (Large)** ⭐
- 검증된 안정성
- 뛰어난 속도
- 풍부한 학습 자료

### 2순위: **YOLO11l (Large)** 🆕
- 최신 아키텍처
- 파라미터 22% 감소
- 정확도 향상 (실험적)

**자세한 비교**: `YOLO11_vs_YOLOv8.md` 참조

---

## 📚 관련 문서

- **프로젝트 로드맵**: `../../docs/PCB_Defect_Detection_Project.md`
- **Phase 1 가이드**: `../../docs/Phase1_YOLO_Setup.md`
- **데이터셋 가이드**: `../../docs/Dataset_Guide.md`

---

**작성일**: 2025-10-25
**업데이트**: Phase 1 완료
