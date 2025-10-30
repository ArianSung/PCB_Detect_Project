# 부품 실장 & 납땜 불량 데이터셋 통합 가이드

**작성일**: 2025-10-28
**목표**: 앞판(부품 실장) + 뒷판(납땜) 검사를 위한 완전한 PCB 검사 시스템 구축

---

## 🎯 프로젝트 최종 목표

```
┌─────────────────────────────────────────────────────────────┐
│  양면 PCB 완전 검사 시스템                                  │
├─────────────────────────────────────────────────────────────┤
│  📷 앞판 (Top Side) - 좌측 웹캠                            │
│    ✅ 부품 실장 상태: Missing, Misalignment, Damaged        │
│    ✅ PCB 패턴 손상: Open, Short, Copper 등 (완료 ✅)       │
│                                                             │
│  📷 뒷판 (Bottom Side) - 우측 웹캠                         │
│    ✅ 납땜 품질: Bridge, Insufficient, Excess, Cold Joint  │
│    ✅ PCB 패턴 손상: Open, Short, Copper 등 (완료 ✅)       │
└─────────────────────────────────────────────────────────────┘
```

---

## 📊 통합할 데이터셋

### 1. **DeepPCB** - 현재 사용 중 ✅

- **이미지 수**: 1,500+
- **해상도**: 640×640
- **기능**: PCB 패턴 불량 검출
- **클래스**:
  - `open` (단선)
  - `short` (단락)
  - `mousebite` (가장자리 불량)
  - `spur` (돌기)
  - `copper` (구리 패턴 손상)
  - `pin-hole` (핀홀)
- **상태**: ✅ YOLO 형식 변환 완료, 98% 정확도 달성

---

### 2. **SolDef_AI** - 납땜 불량 🆕

- **출처**: Kaggle
- **이미지 수**: 1,150개 (3가지 각도에서 촬영)
- **기능**: 납땜 품질 검사
- **클래스**:
  - `solder_bridge` (납땜 브리지)
  - `insufficient_solder` (납땜 부족)
  - `excess_solder` (과다 납땜)
  - `cold_joint` (불완전한 납땜)
  - `misaligned_component` (부품 위치 불량)
- **형식**: 이미지 + 라벨 필요
- **다운로드**:
  ```bash
  kaggle datasets download -d mauriziocalabrese/soldef-ai-pcb-dataset-for-defect-detection
  ```
- **라이선스**: GPL 3.0
- **인용 필수**: https://doi.org/10.3390/jmmp8030117

---

### 3. **PCBA-Dataset** - 부품 실장 불량 🆕

- **출처**: GitHub (ismh16/PCBA-Dataset)
- **이미지 수**: 4,000+ (데이터 증강 포함 6,384개)
- **기능**: 부품 실장 상태 검사
- **클래스**:
  - `missing_component` (부품 누락)
  - `misaligned_component` (위치 불량)
  - `damaged_component` (손상된 부품)
- **형식**: YOLO 형식 제공
- **다운로드**:
  ```bash
  git clone https://github.com/ismh16/PCBA-Dataset.git
  ```

---

## 🚀 실행 계획

### **Phase 1: 데이터셋 다운로드** (1-2시간)

#### 1-1. Kaggle API 설정

```bash
# Kaggle CLI 설치
pip install kaggle

# API 토큰 설정
# 1. https://www.kaggle.com/ 로그인
# 2. Account → Settings → API → Create New API Token
# 3. kaggle.json 다운로드 후 설정
mkdir -p ~/.kaggle
cp /path/to/kaggle.json ~/.kaggle/
chmod 600 ~/.kaggle/kaggle.json
```

#### 1-2. 데이터셋 다운로드 실행

```bash
cd /home/sys1041/work_project

# 실행 권한 부여
chmod +x scripts/download_component_solder_datasets.sh

# 다운로드 실행
bash scripts/download_component_solder_datasets.sh
```

**예상 다운로드 용량**:
- SolDef_AI: ~1.2GB
- PCBA-Dataset: ~500MB
- **총합**: ~1.7GB

---

### **Phase 2: 데이터셋 통합** (2-3시간)

#### 2-1. 기존 DeepPCB 데이터셋 복사

```bash
cd /home/sys1041/work_project

# 통합 스크립트 실행
python yolo/merge_all_datasets.py
```

이 스크립트는:
1. ✅ DeepPCB 데이터를 새 통합 디렉토리로 복사
2. ⚠️ SolDef_AI 라벨링 필요 확인
3. ⚠️ PCBA-Dataset 클래스 매핑 검토

#### 2-2. 추가 작업 필요 사항

**SolDef_AI 라벨링**:
- 이미지는 제공되지만, YOLO 형식 라벨이 없을 수 있음
- **옵션 1**: Roboflow 사용 (https://roboflow.com/)
  - 이미지 업로드
  - Auto-labeling 기능 활용
  - YOLO 형식으로 내보내기
- **옵션 2**: LabelImg 사용
  ```bash
  pip install labelImg
  labelImg
  ```

**PCBA-Dataset 클래스 매핑**:
- 원본 클래스를 우리 프로젝트 클래스로 매핑
- 예: `missing_screw` → `missing_component`

---

### **Phase 3: 통합 모델 학습** (8-12시간, GPU 의존)

#### 3-1. 최종 클래스 정의

```yaml
총 13개 클래스:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
🔹 PCB 패턴 불량 (6개) - DeepPCB
  0: open
  1: short
  2: mousebite
  3: spur
  4: copper
  5: pin-hole

🔹 부품 실장 불량 (3개) - PCBA-Dataset
  6: missing_component
  7: misaligned_component
  8: damaged_component

🔹 납땜 불량 (4개) - SolDef_AI
  9: solder_bridge
  10: insufficient_solder
  11: excess_solder
  12: cold_joint
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

#### 3-2. 학습 스크립트 작성 (나중에 구현)

```bash
# 통합 데이터셋으로 학습
python yolo/train_complete_pcb.py
```

학습 설정:
- **모델**: YOLOv8l (현재 사용 중)
- **Epochs**: 300 (Early stopping patience=100)
- **Batch size**: 16 (RTX 4080 Super)
- **데이터**: 6,000+ 이미지 (통합 후)
- **예상 시간**: 8-12시간

---

### **Phase 4: 앞판/뒷판 분리 검사 로직**

#### 4-1. 검사 로직 설계

```python
# server/inference.py에 추가

def classify_defect_by_side(detections, camera_side):
    """
    카메라 위치에 따라 불량 분류

    Args:
        detections: YOLO 검출 결과
        camera_side: 'left' (앞판) 또는 'right' (뒷판)

    Returns:
        classification: '부품불량', '납땜불량', '폐기', '정상'
    """

    if camera_side == 'left':
        # 앞판 검사
        component_defects = [
            'missing_component',
            'misaligned_component',
            'damaged_component'
        ]

        pcb_critical_defects = [
            'open', 'short', 'copper'
        ]

        # 부품 불량 검출
        if any(d['class'] in component_defects for d in detections):
            return '부품불량'

        # PCB 치명적 불량 검출
        if any(d['class'] in pcb_critical_defects for d in detections):
            return '폐기'

        # 경미한 PCB 불량
        if len(detections) > 0:
            return '부품불량'  # 또는 '경미한불량'

        return '정상'

    elif camera_side == 'right':
        # 뒷판 검사
        solder_defects = [
            'solder_bridge',
            'insufficient_solder',
            'excess_solder',
            'cold_joint'
        ]

        # 납땜 불량 검출
        if any(d['class'] in solder_defects for d in detections):
            return '납땜불량'

        # PCB 치명적 불량 검출
        if any(d['class'] in ['open', 'short', 'copper'] for d in detections):
            return '폐기'

        # 경미한 불량
        if len(detections) > 0:
            return '납땜불량'

        return '정상'
```

---

## 📈 예상 결과

### **통합 데이터셋 규모**

| 데이터셋 | 이미지 수 | 클래스 수 | 기능 |
|---------|----------|----------|------|
| DeepPCB | 1,500+ | 6 | PCB 패턴 |
| SolDef_AI | 1,150 | 4-5 | 납땜 + 부품 위치 |
| PCBA-Dataset | 4,000+ | 3-8 | 부품 실장 |
| **총합** | **6,650+** | **13** | **완전한 PCB 검사** |

### **성능 목표**

```
✅ 앞판 검사 (부품 + PCB 패턴):
   - 부품 누락 검출율: > 95%
   - 위치 불량 검출율: > 90%
   - PCB 패턴 불량: 98% (현재 달성 ✅)

✅ 뒷판 검사 (납땜 + PCB 패턴):
   - 납땜 불량 검출율: > 92%
   - PCB 패턴 불량: 98% (현재 달성 ✅)

✅ 처리 속도:
   - 현재: 10.7ms/image (93 FPS)
   - 예상 (13 클래스): 15-20ms/image (50-66 FPS)
   - 목표 < 300ms: ✅ 충분히 달성 가능
```

---

## ⚠️ 주의 사항

### 1. **라벨링 작업량**

- SolDef_AI: 1,150개 이미지 라벨링 필요 (예상 3-5일)
- Roboflow Auto-labeling 활용 시 1-2일로 단축 가능

### 2. **클래스 불균형**

- 일부 불량 유형이 적을 수 있음
- 데이터 증강 (Augmentation) 필수
- Class weights 조정 필요

### 3. **메모리 요구사항**

- 6,650+ 이미지 학습: 10-12GB VRAM 필요
- RTX 4080 Super (16GB): ✅ 충분
- Batch size 조정 가능: 16 → 24

### 4. **학습 시간**

- 300 epochs: 약 10-15시간 예상
- Early stopping으로 단축 가능

---

## 📝 체크리스트

### Phase 1: 데이터 수집
- [ ] Kaggle API 설정 완료
- [ ] SolDef_AI 다운로드 완료
- [ ] PCBA-Dataset 다운로드 완료

### Phase 2: 데이터 전처리
- [ ] DeepPCB 데이터 복사 완료
- [ ] SolDef_AI 라벨링 완료
- [ ] PCBA-Dataset 클래스 매핑 완료
- [ ] 통합 data.yaml 생성 완료

### Phase 3: 모델 학습
- [ ] 학습 스크립트 작성
- [ ] 학습 실행 (300 epochs)
- [ ] 검증 성능 확인 (mAP > 90%)
- [ ] 최적 모델 저장

### Phase 4: 시스템 통합
- [ ] 앞판/뒷판 분리 로직 구현
- [ ] Flask 서버 업데이트
- [ ] 실제 웹캠 테스트
- [ ] GPIO 제어 연동

---

## 🎓 참고 자료

### 논문
- SolDef_AI: https://doi.org/10.3390/jmmp8030117
- DeepPCB: "DeepPCB: A Deep Learning Framework for PCB Defect Detection"

### 데이터셋
- Kaggle SolDef_AI: https://www.kaggle.com/datasets/mauriziocalabrese/soldef-ai-pcb-dataset-for-defect-detection
- GitHub PCBA-Dataset: https://github.com/ismh16/PCBA-Dataset

### 도구
- Roboflow: https://roboflow.com/
- LabelImg: https://github.com/heartexlabs/labelImg
- Ultralytics YOLO: https://docs.ultralytics.com/

---

## 📞 다음 단계

1. **즉시 실행 가능**:
   ```bash
   # 데이터셋 다운로드
   bash scripts/download_component_solder_datasets.sh
   ```

2. **라벨링 작업** (3-5일):
   - Roboflow 또는 LabelImg 사용
   - SolDef_AI 이미지 라벨링

3. **데이터 통합** (1일):
   ```bash
   python yolo/merge_all_datasets.py
   ```

4. **모델 학습** (1-2일):
   ```bash
   python yolo/train_complete_pcb.py  # 나중에 작성
   ```

**예상 전체 소요 시간**: 1-2주
