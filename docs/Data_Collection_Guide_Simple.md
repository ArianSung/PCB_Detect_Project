# Through-hole 부품 데이터 수집 간단 가이드

**프로젝트**: PCB 불량 검사 시스템
**목적**: Through-hole 부품 검출 (있다/없다, 종류만 구분)
**작성일**: 2025-01-05

---

## 📑 목차

1. [빠른 시작](#빠른-시작)
2. [부품 준비](#부품-준비)
3. [촬영 설정](#촬영-설정)
4. [촬영 방법](#촬영-방법)
5. [Roboflow 라벨링](#roboflow-라벨링)
6. [자동 증강](#자동-증강)

---

## 빠른 시작

### ⚡ 3줄 요약
1. **부품 준비**: 다이소 전자부품 키트 (1-2만원) 또는 집에 있는 부품
2. **촬영**: 실제 사용할 웹캠으로 200-300장 (2-4시간)
3. **라벨링**: Roboflow에서 박스 그리기 (6-12시간)

### 🎯 목표
- **이미지**: 200-300장
- **클래스**: 6-8개 (저항, LED, IC 등)
- **시간**: 총 1-2일
- **예상 성능**: mAP 85-90%

---

## 부품 준비

### 🛒 간단 쇼핑 리스트

**옵션 1: 다이소 키트** (가장 쉬움) ⭐
```
□ 전자부품 키트 (15,000-20,000원)
  → 저항, LED, 다이오드, 트랜지스터 등 모두 포함
  → 아무거나 섞여있어도 OK!
```

**옵션 2: 집에 있는 부품 활용**
```
□ 고장난 전자기기 분해
□ 예전 아두이노/전자 프로젝트 남은 부품
□ 학교 실습 남은 부품
→ 최소 50-100개 부품 있으면 충분
```

**옵션 3: 온라인 구매** (1-2만원)
```
디바이스마트/아이씨뱅큐:
□ 저항 키트 (100개, 3,000원)
□ LED 키트 (50개, 5색 혼합, 3,000원)
□ 다이오드 20개 (2,000원)
□ 트랜지스터 20개 (2,000원)
□ IC 소켓/칩 10개 (3,000원)
□ 기타 (스위치, 센서 등) (5,000원)
---
총 18,000원
```

### 📦 필요한 클래스 (간소화)

**최소 (6개 클래스)**
```
1. Resistor       - 저항 (아무거나 20-30개)
2. LED            - LED (아무 색이나 20-30개)
3. Diode          - 다이오드 (아무거나 15-20개)
4. Transistor     - 트랜지스터 (아무거나 15-20개)
5. IC             - IC 칩 (아무 핀 수나 10-15개)
6. Switch         - 스위치 (택타일 등 10-15개)
```

**권장 (8개 클래스)** ⭐
```
위 6개 +
7. Capacitor      - 커패시터 (전해/세라믹 아무거나 15-20개)
8. Sensor         - 센서 (CdS, 온도센서 등 10-15개)
```

**💡 핵심 원칙**:
- 종류만 구분, 세부 스펙은 무시
- 저항값 몰라도 됨 (100Ω이든 1kΩ이든 → 그냥 "Resistor")
- LED 색상 상관없음 (빨강이든 파랑이든 → 그냥 "LED")
- IC 핀 수 상관없음 (8핀이든 28핀이든 → 그냥 "IC")

### 🎨 PCB 보드 (배경)

**최소**
```
□ PCB 보드 1-2장 (녹색 또는 파란색)
□ 크기: 10x15cm 이상
□ 다이소/전자상가 (장당 1,000-2,000원)
```

**권장** ⭐
```
□ PCB 보드 3-5장
  - 녹색 2-3장
  - 파란색 1-2장
□ 다양한 크기면 더 좋음
```

---

## 촬영 설정

### 🎥 카메라

**필수 조건** ⭐
```
✓ 프로젝트에서 실제 사용할 웹캠 (이게 가장 중요!)
✓ 해상도: 640x480 (VGA) 이상
✓ 고정 위치 (마운트/삼각대)
```

**권장 설정**
```
- 높이: PCB 위 40-60cm
- 각도: 정면 (90도)
- 초점: 자동 또는 고정
```

### 💡 조명

**최소**
```
□ 실내 형광등
□ 창문 자연광
→ 그림자만 심하지 않으면 OK
```

**권장** ⭐
```
□ USB LED 램프 2개 (다이소, 각 5,000원)
□ 위치: PCB 좌우 양쪽
□ 밝기: 중간 (너무 밝으면 반사)
```

### 📐 촬영 공간

```
┌─────────────────────┐
│    💡         💡    │  ← 조명 2개 (좌우)
│                     │
│       📷           │  ← 웹캠 (40-60cm 높이)
│                     │
│    [PCB 보드]      │  ← PCB + 부품
│                     │
└─────────────────────┘

- 깨끗한 책상/테이블
- 주변 정리 (깔끔한 배경)
- 진동 없는 곳
```

---

## 촬영 방법

### 📸 부품 배치 전략

**패턴 1: 적당히 섞기** (권장) ⭐
```
┌──────────────────────┐
│ 🔴 저항              │
│      🟢 LED          │
│ 🔵 다이오드          │
│           🟡 IC      │
│ 🟣 트랜지스터        │
│      🟠 스위치       │
│ 🔴 저항  🟢 LED     │
└──────────────────────┘

- 한 이미지당 10-20개 부품
- 아무렇게나 배치 OK
- 겹치지만 않으면 됨
- 다양한 종류 섞어서
```

**핵심**:
- ✅ 실제 PCB처럼 자연스럽게
- ✅ 랜덤하게 배치 (패턴 없이)
- ❌ 일렬로 정렬 금지 (비현실적)
- ❌ 부품끼리 겹치기 금지

### 🎬 촬영 시나리오

**간단 버전 (200장, 2-3시간)**
```
준비:
- PCB 2장 (녹색 + 파란색)
- 부품 총 50-100개

촬영:
1. PCB 1에 부품 10-15개 배치
2. 웹캠으로 20장 촬영
   - 그대로 10장
   - 약간 각도 바꿔서 10장
3. 부품 위치 바꾸기
4. 20장 촬영
5. 5회 반복 → 100장
6. PCB 2로 교체하고 반복 → 100장
---
총 200장, 2-3시간
```

**추천 버전 (300장, 4-6시간)** ⭐
```
준비:
- PCB 3장
- 부품 총 100개

촬영:
1. PCB별로 100장씩
2. 부품 배치 다양하게 (7-8회)
3. 조명 약간 조정 (밝게/어둡게)
4. 카메라 높이 약간 조정
---
총 300장, 4-6시간
```

### 🤖 자동 촬영 스크립트

**간단 버전**
```python
# scripts/capture_simple.py

import cv2
import os
from pathlib import Path

# 설정
OUTPUT = "data/custom_throughhole/raw"
CAMERA = 0  # 실제 사용할 웹캠 번호!
WIDTH = 640
HEIGHT = 480

Path(OUTPUT).mkdir(parents=True, exist_ok=True)

cap = cv2.VideoCapture(CAMERA)
cap.set(cv2.CAP_PROP_FRAME_WIDTH, WIDTH)
cap.set(cv2.CAP_PROP_FRAME_HEIGHT, HEIGHT)

print("=" * 60)
print("📸 촬영 시작")
print("=" * 60)
print("SPACE: 사진 찍기")
print("Q: 종료")
print("=" * 60)

count = 0

while True:
    ret, frame = cap.read()
    if not ret:
        break

    # 화면 표시
    cv2.putText(frame, f"Count: {count}", (10, 30),
               cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)
    cv2.putText(frame, "SPACE: Capture | Q: Quit", (10, HEIGHT - 10),
               cv2.FONT_HERSHEY_SIMPLEX, 0.5, (255, 255, 255), 1)
    cv2.imshow('Capture', frame)

    key = cv2.waitKey(1) & 0xFF

    if key == ord(' '):
        filename = f"{OUTPUT}/pcb_{count:04d}.jpg"
        cv2.imwrite(filename, frame)
        print(f"✅ {count}: {filename}")
        count += 1

    elif key == ord('q'):
        break

cap.release()
cv2.destroyAllWindows()
print(f"\n✅ 촬영 완료: {count}장")
```

**사용법**:
```bash
conda activate pcb_defect
python scripts/capture_simple.py

# 부품 배치 → SPACE → 부품 이동 → SPACE → 반복
# 200-300장 촬영
```

### ✅ 촬영 체크리스트

```
□ 실제 사용 웹캠으로 촬영 (필수!)
□ 해상도 640x480 이상
□ 조명 균일 (그림자 적게)
□ PCB 배경 깨끗
□ 부품 겹치지 않게
□ 200-300장 촬영
□ 다양한 배치 (최소 6-8번)
□ 모든 클래스 골고루 포함
```

---

## Roboflow 라벨링

### 🚀 시작하기

**1. 가입 및 프로젝트 생성 (5분)**
```
1. https://roboflow.com 접속
2. "Sign Up" → Google 계정 가입
3. "Create New Project"
   - Name: PCB_ThroughHole
   - Type: Object Detection
4. "Create" 클릭
```

**2. 이미지 업로드 (5-10분)**
```
1. "Upload" 클릭
2. 촬영한 200-300장 선택
3. "Upload" → 대기
```

### 🏷️ 클래스 설정

**간소화된 클래스 (6-8개)** ⭐
```
최소 (6개):
- Resistor
- LED
- Diode
- Transistor
- IC
- Switch

권장 (8개):
- 위 6개 +
- Capacitor
- Sensor
```

**💡 중요**:
- 세부 구분 안 함!
- 저항이면 그냥 "Resistor" (값 상관없이)
- LED면 그냥 "LED" (색 상관없이)
- IC면 그냥 "IC" (핀 수 상관없이)

### 📦 라벨링 작업

**기본 워크플로우**
```
1. "Annotate" 클릭
2. 첫 이미지 열기
3. 'B' 키 누르기 (박스 모드)
4. 부품 주변 드래그해서 박스 그리기
5. 클래스 선택 (예: Resistor)
6. 다음 부품 반복
7. 모든 부품 완료 → Ctrl+S (저장 & 다음)
```

**단축키** ⭐
```
B - 박스 그리기
1-9 - 클래스 빠른 선택
D - 박스 삭제
Ctrl+Z - 실행 취소
Ctrl+S - 저장 & 다음 이미지
← → - 이전/다음 이미지
```

**속도 향상 팁**
```
1. 동일 클래스 연속 라벨링
   예: 이미지당 저항 5개 → 저항만 다 그리기 → 다음 클래스
2. 단축키 숙달 (1-9번 키)
3. 30분 작업 → 5분 휴식
4. 하루 2-3시간씩
```

**예상 시간**
```
- 초보: 이미지당 3-4분 → 200장 = 10-13시간
- 숙련: 이미지당 2분 → 200장 = 6-7시간
- 전문: 이미지당 1-1.5분 → 200장 = 3-5시간
```

### 🤖 스마트 라벨링 (선택)

**Auto-label 활용** (50% 시간 절약)
```
조건: 50-100장 수동 라벨링 완료

방법:
1. 50-100장 수동 라벨링
2. "Generate" → "Train Model" (빠른 모델)
3. 학습 완료 대기 (5-10분)
4. "Auto-label" 활성화
5. 나머지 이미지 자동 라벨링
6. 수동 검수 (20% 정도 수정 필요)
```

### 📊 데이터 분할

**자동 분할** (권장) ⭐
```
"Generate" → "Add Split":
- Train: 70%
- Valid: 20%
- Test: 10%

"Auto-split" 선택
```

---

## 자동 증강

### 🔧 Roboflow 증강 (권장)

**간단 설정** ⭐
```
Preprocessing:
✓ Auto-Orient
✓ Resize: Stretch to 640x640

Augmentation:
- Outputs per training example: 3x

Flip:
✓ Horizontal: 50%
✓ Vertical: 50%

Rotate:
✓ Between -15° and +15°

Brightness:
✓ Between -20% and +20%
```

**결과**:
- 200장 → 600장 (3배)
- 300장 → 900장 (3배)

### 📥 Export

**다운로드**
```
1. "Generate" 클릭
2. "Export Dataset"
3. Format: "YOLO v8 PyTorch"
4. "Download ZIP"
```

**서버 업로드**
```bash
# 로컬에서 압축 해제
unzip dataset.zip

# 서버로 복사
scp -r PCB-ThroughHole-1/ server:/path/to/project/data/

# 또는 Roboflow SDK
pip install roboflow

python
from roboflow import Roboflow
rf = Roboflow(api_key="YOUR_KEY")
project = rf.workspace().project("pcb-throughhole")
dataset = project.version(1).download("yolov8")
```

---

## 품질 체크

### ✅ 최종 확인

**Roboflow Health Check**
```
"Health Check" 탭:
✓ Null annotations: 0 (모든 이미지 라벨 있음)
✓ Class balance: 각 클래스 10% 이상
✓ Average image size: 적절
```

**수동 확인**
```
랜덤 30장 열어서:
✓ 누락된 부품 없는가?
✓ 클래스 맞는가?
✓ 박스 크기 적절한가?
```

### 📊 목표 통계

**최소**
```
- 이미지: 200장
- 객체: 2,000개
- 클래스: 6개
- 클래스당: 최소 300개
```

**권장** ⭐
```
- 이미지: 300장
- 객체: 3,000-4,000개
- 클래스: 6-8개
- 클래스당: 최소 400개
```

---

## 요약

### 🎯 핵심 3단계

```
1️⃣ 부품 준비 (1시간)
   → 다이소 키트 또는 집 부품 50-100개

2️⃣ 촬영 (2-4시간)
   → 실제 웹캠으로 200-300장
   → 부품 아무렇게나 배치
   → SPACE 누르면 촬영

3️⃣ 라벨링 (6-12시간)
   → Roboflow 업로드
   → 박스 그리기 (세부 스펙 무시)
   → 증강 3배 → Export
```

### 💡 핵심 원칙

```
✓ 세부 스펙 무시 (저항값, LED 색, IC 핀 수 등)
✓ 종류만 구분 (저항, LED, IC, 다이오드...)
✓ 실제 사용 웹캠으로 촬영
✓ 원본만 촬영, 증강은 Roboflow
✓ 자연스러운 배치 (랜덤)
```

### ⏱️ 예상 일정

**Day 1 (4-6시간)**
```
- 부품 준비: 1시간
- 촬영 환경 설정: 1시간
- 촬영: 2-4시간 (200-300장)
```

**Day 2 (6-8시간)**
```
- Roboflow 설정: 30분
- 라벨링: 5-7시간
- Export: 30분
```

**총 10-14시간 (1-2일)**

### 🚀 다음 단계

```
✅ 데이터 수집 완료
↓
학습 (YOLO_Training_Guide.md)
↓
Flask 서버 통합
↓
라즈베리파이 배포
↓
완성! 🎉
```

---

**작성자**: Claude Code
**최종 업데이트**: 2025-01-05
**관련 문서**: `YOLO_Training_Guide.md`, `Flask_Server_Setup.md`

**💬 질문/피드백**: GitHub Issues
