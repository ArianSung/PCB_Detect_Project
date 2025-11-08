# Roboflow 라벨링 가이드 (일련번호 검출)

## 개요

LabelImg보다 훨씬 편리한 **웹 기반 라벨링 도구**입니다.

**장점**:
- 설치 불필요 (웹 브라우저에서 사용)
- 직관적인 UI
- 자동 데이터 분할 (Train/Valid/Test)
- YOLO 포맷으로 바로 Export
- 무료 (Public 프로젝트)

---

## 1단계: 계정 생성 및 로그인

### 1.1. 로보플로우 사이트 접속

https://roboflow.com/

### 1.2. 계정 생성

1. 우측 상단 **"Sign Up"** 클릭
2. 다음 중 하나로 가입:
   - Google 계정
   - GitHub 계정
   - 이메일 + 비밀번호

### 1.3. Workspace 생성

- Workspace 이름: `pcb-inspection` (원하는 이름)
- 타입: **Public** (무료) 또는 Private (유료)

---

## 2단계: 프로젝트 생성

### 2.1. 새 프로젝트 생성

1. 대시보드에서 **"Create New Project"** 클릭
2. 프로젝트 정보 입력:
   ```
   Project Name: serial-number-detection
   Project Type: Object Detection
   ```
3. **"Create Project"** 클릭

### 2.2. 클래스 정의

1. "Add Class" 클릭
2. 클래스 이름 입력: `serial_number`
3. **"Save"** 클릭

---

## 3단계: 이미지 업로드

### 3.1. 이미지 준비

- PCB 앞면 이미지 **100-300장** 준비
- 폴더에 모아두기 (예: `~/Downloads/pcb_images/`)

### 3.2. 이미지 업로드

1. 프로젝트 페이지에서 **"Upload"** 클릭
2. 업로드 방법 선택:
   - **"Drag and Drop"**: 이미지 드래그해서 올리기
   - **"Browse"**: 폴더에서 선택
   - **API 업로드**: 대량 업로드 시 사용

3. 업로드 설정:
   ```
   ✅ Split images into train/valid/test
   Train: 70%
   Valid: 15%
   Test: 15%
   ```

4. **"Upload XX Images"** 클릭

### 3.3. 업로드 대기

- 업로드 진행률 표시됨
- 완료되면 **"Go to Dataset"** 클릭

---

## 4단계: 라벨링 (Annotation)

### 4.1. Annotate 모드 진입

1. 프로젝트 페이지에서 **"Annotate"** 탭 클릭
2. 첫 번째 이미지가 자동으로 표시됨

### 4.2. 박스 그리기 (일련번호 영역)

#### 방법 1: 마우스로 드래그
```
1. 좌측 툴바에서 "Bounding Box" 도구 선택 (사각형 아이콘)
2. 일련번호가 있는 영역을 마우스로 드래그
3. 박스가 생성되면 클래스 선택: "serial_number"
```

#### 방법 2: 단축키 사용
```
B 키: Bounding Box 도구 활성화
마우스 드래그: 박스 그리기
Esc: 취소
```

**중요**: 일련번호 **텍스트 전체**를 포함하도록 박스 크기 조정!

```
예시:
┌────────────────────────┐
│  PCB 이미지            │
│                        │
│  ┌───────────────┐    │
│  │ PCB-2025-0012 │ ← 이 영역만 박스로 그리기!
│  └───────────────┘    │
│                        │
│  R1  IC1  3.3V         │ ← 이런 텍스트는 무시
│                        │
└────────────────────────┘
```

### 4.3. 박스 수정/삭제

- **크기 조정**: 박스 모서리를 드래그
- **이동**: 박스 중앙을 드래그
- **삭제**: 박스 선택 후 `Delete` 키

### 4.4. 다음 이미지로 이동

1. 라벨링 완료 후 우측 하단 **"Save"** 클릭
2. 자동으로 다음 이미지로 이동
3. 반복 (100-300장 모두 라벨링)

### 4.5. 라벨링 진행률 확인

- 좌측 상단에 진행률 표시: `12 / 200 annotated`
- 언제든지 중단 가능 (자동 저장됨)

---

## 5단계: 데이터 증강 (선택 사항)

### 5.1. Preprocessing

로보플로우는 자동 전처리 기능을 제공합니다.

1. **"Generate"** 탭 클릭
2. Preprocessing 단계:
   ```
   ✅ Auto-Orient: 이미지 회전 자동 수정
   ✅ Resize: 640×640 (YOLO 기본 크기)
   ❌ Grayscale: 사용 안 함 (컬러 유지)
   ```

### 5.2. Augmentation (증강)

**중요**: 일련번호는 위치가 고정이므로 **증강 최소화**!

```
추천 설정:
❌ Flip: 사용 안 함 (일련번호는 항상 정방향)
✅ Rotation: ±5도만 (degrees=5)
✅ Crop: 0-5% (최소 크롭)
❌ Blur: 사용 안 함
❌ Brightness: 사용 안 함
❌ Cutout: 사용 안 함
```

### 5.3. Generate 실행

1. 설정 완료 후 **"Generate"** 클릭
2. Dataset 이름 입력: `v1` (또는 원하는 버전)
3. 생성 대기 (1-2분)

---

## 6단계: Export (YOLO 포맷)

### 6.1. Export 페이지 이동

1. **"Export"** 탭 클릭
2. 생성된 Dataset 버전 선택 (예: `v1`)

### 6.2. 포맷 선택

1. Format 선택: **"YOLOv11"** 또는 **"YOLOv8"** (둘 다 호환)
2. **"Show Download Code"** 클릭

### 6.3. 다운로드 코드 복사

로보플로우가 제공하는 Python 코드를 복사합니다:

```python
from roboflow import Roboflow

rf = Roboflow(api_key="YOUR_API_KEY_HERE")
project = rf.workspace("YOUR_WORKSPACE").project("serial-number-detection")
version = project.version(1)
dataset = version.download("yolov11")
```

---

## 7단계: 데이터셋 다운로드 및 학습

### 7.1. 다운로드 스크립트 생성

**scripts/download_serial_number_dataset.py**:
```python
#!/usr/bin/env python3
"""
Roboflow에서 일련번호 검출 데이터셋 다운로드
"""

from roboflow import Roboflow
from pathlib import Path

def download_dataset():
    """Roboflow에서 데이터셋 다운로드"""

    print("="*80)
    print("Roboflow 일련번호 검출 데이터셋 다운로드")
    print("="*80)

    # Roboflow API 초기화
    # 주의: YOUR_API_KEY를 실제 API 키로 변경하세요!
    rf = Roboflow(api_key="YOUR_API_KEY_HERE")

    # 프로젝트 및 버전 선택
    project = rf.workspace("YOUR_WORKSPACE").project("serial-number-detection")
    version = project.version(1)

    # 다운로드 경로
    output_dir = Path("/home/sys1041/work_project/data/raw")

    # YOLOv11 포맷으로 다운로드
    print("\n데이터셋 다운로드 중...")
    dataset = version.download("yolov11", location=str(output_dir))

    print("\n✓ 다운로드 완료!")
    print(f"  경로: {output_dir}/serial-number-detection")
    print("")
    print("다운로드된 파일 구조:")
    print("  serial-number-detection/")
    print("  ├── train/")
    print("  │   ├── images/")
    print("  │   └── labels/")
    print("  ├── valid/")
    print("  │   ├── images/")
    print("  │   └── labels/")
    print("  ├── test/")
    print("  │   ├── images/")
    print("  │   └── labels/")
    print("  └── data.yaml")
    print("")
    print("다음 단계:")
    print("  python scripts/train_serial_number_detector.py")

if __name__ == "__main__":
    download_dataset()
```

### 7.2. API 키 확인

1. Roboflow 웹사이트에서 우측 상단 프로필 클릭
2. **"Settings"** → **"API Keys"**
3. API 키 복사
4. 스크립트의 `YOUR_API_KEY_HERE`를 실제 키로 변경

### 7.3. 다운로드 실행

```bash
conda activate pcb_defect

# Roboflow 라이브러리 설치 (처음 한 번만)
pip install roboflow

# 다운로드 스크립트 실행
python scripts/download_serial_number_dataset.py
```

### 7.4. data.yaml 확인

다운로드된 데이터셋에는 `data.yaml` 파일이 자동으로 포함됩니다:

```yaml
path: /home/sys1041/work_project/data/raw/serial-number-detection
train: train/images
val: valid/images
test: test/images

nc: 1
names: ['serial_number']
```

### 7.5. 학습 실행

```bash
# data.yaml 경로를 로보플로우 다운로드 경로로 변경
python scripts/train_serial_number_detector.py
```

**또는** `train_serial_number_detector.py` 파일 수정:
```python
data_yaml = Path('/home/sys1041/work_project/data/raw/serial-number-detection/data.yaml')
```

---

## 로보플로우 vs LabelImg 비교

| 기능                | Roboflow                | LabelImg                |
|---------------------|-------------------------|-------------------------|
| **설치**            | 불필요 (웹)             | pip install 필요        |
| **UI**              | 직관적, 현대적          | 기본적                  |
| **데이터 분할**     | 자동 (70/15/15)         | 수동                    |
| **증강**            | 웹에서 설정 가능        | 별도 스크립트 필요      |
| **Export**          | 다양한 포맷 지원        | YOLO만                  |
| **협업**            | 여러 명 동시 라벨링     | 불가능                  |
| **가격**            | 무료 (Public 프로젝트)  | 완전 무료               |
| **인터넷**          | 필수                    | 불필요                  |

**추천**: 로보플로우 사용! (훨씬 편리)

---

## FAQ ❓

### Q1: API 키는 어디서 찾나요?

**A**: Roboflow 웹사이트 → 우측 상단 프로필 → Settings → API Keys

### Q2: 무료로 사용 가능한가요?

**A**: 네! Public 프로젝트는 완전 무료입니다.
- 무료: 10,000 이미지, Public 프로젝트
- 유료: Private 프로젝트, 더 많은 이미지

### Q3: 라벨링 도중 중단하면?

**A**: 자동 저장되므로 걱정 없습니다. 다음에 이어서 라벨링 가능합니다.

### Q4: 여러 명이 동시에 라벨링 가능한가요?

**A**: 네! 팀원을 초대하면 여러 명이 동시에 라벨링할 수 있습니다.

### Q5: 다운로드한 데이터셋 구조가 이상해요.

**A**: 로보플로우는 다음과 같이 다운로드됩니다:
```
serial-number-detection/
├── train/
│   ├── images/
│   └── labels/
├── valid/
│   ├── images/
│   └── labels/
├── test/
│   ├── images/
│   └── labels/
└── data.yaml  ← 이 파일 경로를 학습 스크립트에 지정
```

### Q6: 박스를 잘못 그렸어요!

**A**:
- 박스 선택 후 `Delete` 키
- 또는 박스 모서리를 드래그하여 크기 조정

### Q7: 일련번호 외에 다른 텍스트도 라벨링해야 하나요?

**A**: 아니요! 일련번호 영역만 라벨링하면 됩니다.

---

## 단계별 체크리스트 ✅

### Phase 1: 준비
- [ ] Roboflow 계정 생성
- [ ] Workspace 생성
- [ ] 프로젝트 생성 (serial-number-detection)
- [ ] 클래스 정의 (serial_number)

### Phase 2: 데이터
- [ ] PCB 이미지 100-300장 준비
- [ ] Roboflow에 업로드
- [ ] Train/Valid/Test 자동 분할 확인

### Phase 3: 라벨링
- [ ] Annotate 모드 진입
- [ ] 일련번호 영역에 박스 그리기
- [ ] 모든 이미지 라벨링 완료 (100-300장)

### Phase 4: Export
- [ ] Generate 실행 (증강 최소화 설정)
- [ ] YOLOv11 포맷으로 Export
- [ ] API 키 확인
- [ ] 다운로드 스크립트 작성

### Phase 5: 학습
- [ ] `pip install roboflow`
- [ ] 다운로드 스크립트 실행
- [ ] data.yaml 경로 확인
- [ ] YOLO 학습 실행

---

## 추가 팁 💡

### 1. 라벨링 속도 높이기

- **단축키 활용**:
  - `B`: Bounding Box 도구
  - `Delete`: 박스 삭제
  - `Ctrl + S`: 저장 (자동 저장되지만 확실히)
  - `→`: 다음 이미지
  - `←`: 이전 이미지

### 2. 라벨 품질 체크

Roboflow는 자동으로 라벨 품질을 체크합니다:
- 박스가 너무 작거나 큰 경우 경고
- 겹치는 박스가 있는 경우 경고

### 3. 데이터 증강 활용

일련번호는 위치가 고정이므로 증강을 최소화하되, 다음은 유용할 수 있습니다:
- **조명 변화**: Brightness ±10%
- **약간의 회전**: ±5도

### 4. 버전 관리

Roboflow는 여러 버전을 생성할 수 있습니다:
- `v1`: 초기 데이터셋
- `v2`: 증강 추가
- `v3`: 데이터 추가

각 버전별로 성능을 비교하세요!

---

## 문제 해결 🔧

### 문제 1: 업로드가 안 됩니다

**해결책**:
- 브라우저 새로고침
- 다른 브라우저 시도 (Chrome 권장)
- 이미지 파일 형식 확인 (.jpg, .png만 지원)

### 문제 2: 박스가 안 그려집니다

**해결책**:
- `B` 키를 눌러 Bounding Box 도구 활성화
- 좌측 툴바에서 사각형 아이콘 클릭

### 문제 3: 다운로드 스크립트가 안 됩니다

**해결책**:
```bash
# Roboflow 라이브러리 설치
pip install roboflow

# API 키 확인
# YOUR_API_KEY_HERE를 실제 키로 변경했는지 확인
```

---

## 요약 📌

1. **Roboflow 계정** 생성 (무료)
2. **프로젝트** 생성: serial-number-detection
3. **이미지 업로드**: 100-300장
4. **라벨링**: 일련번호 영역만 박스로 그리기
5. **Export**: YOLOv11 포맷
6. **다운로드**: Python 스크립트
7. **학습**: `python scripts/train_serial_number_detector.py`

**시간 예상**:
- 라벨링: 1-2시간 (200장 기준)
- 다운로드: 1-2분
- 학습: 5-10분

**총 시간**: 2시간 이내! 🚀

---

**작성일**: 2025-11-07
**문서 버전**: 1.0
