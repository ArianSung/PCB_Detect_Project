# 팀 협업 가이드

PCB 불량 검사 시스템 팀 프로젝트 협업 전략 및 실전 가이드

---

## 📋 목차

1. [팀 구성 및 역할](#팀-구성-및-역할)
2. [개발 환경 설정](#개발-환경-설정)
3. [일일 워크플로우](#일일-워크플로우)
4. [충돌 해결 전략](#충돌-해결-전략)
5. [커뮤니케이션 규칙](#커뮤니케이션-규칙)
6. [문제 발생 시 대응](#문제-발생-시-대응)

---

## 👥 팀 구성 및 역할

### 전체 팀 구성 (6~7명)

| 팀 | 인원 | 담당 업무 | 주요 작업 |
|----|------|-----------|-----------|
| **Flask 서버 팀** | 2명 | Flask 웹서버 + 추론 엔진 | API 개발, AI 모델 통합, DB 연동 |
| **AI 모델 팀** | 2명 | YOLO 학습 + 이상 탐지 | 모델 학습, 최적화, 평가 |
| **라즈베리파이 팀** | 3명 | 웹캠 클라이언트 + GPIO + OHT | 프레임 캡처, GPIO 제어, OHT 제어 |
| **C# 앱 팀** | 1~2명 | WinForms 모니터링 앱 | UI 개발, 데이터 시각화, Excel 내보내기 |

### 역할별 주요 책임

#### Flask 서버 팀
- **디렉토리**: `server/`, `database/`
- **주요 작업**:
  - Flask API 엔드포인트 개발 (`/predict`, `/history`, `/statistics`)
  - YOLO 모델 로드 및 추론 실행
  - MySQL 데이터베이스 CRUD 작업
  - CORS 설정 및 에러 핸들링
- **다른 팀과의 협업**:
  - **AI 모델 팀**: 학습된 모델 파일 통합
  - **라즈베리파이 팀**: API 요청/응답 형식 협의
  - **C# 앱 팀**: 데이터베이스 스키마 공유

#### AI 모델 팀
- **디렉토리**: `yolo/`, `configs/`, `models/`
- **주요 작업**:
  - YOLOv8 모델 학습 및 하이퍼파라미터 튜닝
  - 이상 탐지 모델 구현 (PaDiM, PatchCore)
  - 모델 성능 평가 (mAP, FPS, 정확도)
  - FP16 최적화 및 배치 처리
- **다른 팀과의 협업**:
  - **Flask 서버 팀**: 학습된 모델 파일 전달
  - **전체 팀**: 모델 성능 보고 및 개선 방향 논의

#### 라즈베리파이 팀
- **디렉토리**: `raspberry_pi/`
- **주요 작업**:
  - 웹캠 프레임 캡처 (OpenCV)
  - JPEG 인코딩 및 Base64 변환
  - Flask API 호출 (`/predict`)
  - GPIO 핀 제어 (릴레이 모듈)
  - Systemd 서비스 설정
- **다른 팀과의 협업**:
  - **Flask 서버 팀**: API 요청 형식 협의
  - **AI 모델 팀**: 이미지 해상도 및 전처리 방법 협의

#### C# 앱 팀
- **디렉토리**: `csharp_winforms/`
- **주요 작업**:
  - WinForms UI 개발 (7개 화면)
  - REST API 호출하여 데이터 조회
  - LiveCharts를 통한 실시간 차트
  - Excel 내보내기 기능 (EPPlus)
  - 권한 시스템 구현 (Admin/Operator/Viewer)
- **다른 팀과의 협업**:
  - **Flask 서버 팀**: API 엔드포인트 및 응답 형식 협의
  - **전체 팀**: UI/UX 요구사항 수집

---

## 🛠️ 개발 환경 설정

### 1. 초기 설정 (모든 팀원 공통)

```bash
# 1. 저장소 클론
git clone https://github.com/ArianSung/PCB_Detect_Project.git
cd PCB_Detect_Project

# 2. 환경 설정 스크립트 실행
bash scripts/setup_env.sh

# 3. .env 파일 확인 및 수정
# 각 팀별로 해당하는 .env 파일 수정
# - Flask 팀: server/.env
# - 라즈베리파이 팀: raspberry_pi/.env
# - C# 앱 팀: csharp_winforms/.env

# 4. Git 브랜치 생성
git checkout develop
git pull origin develop
git checkout -b feature/<팀명>  # 예: feature/flask-server
```

### 2. 팀별 추가 설정

#### Flask 서버 팀

```bash
# 가상환경 활성화
conda activate pcb_defect

# 패키지 설치 확인
pip install -r requirements.txt

# MySQL 데이터베이스 생성
mysql -u root -p < database/schema.sql

# Flask 서버 실행 테스트
cd server
python app.py
```

#### AI 모델 팀

```bash
# 가상환경 활성화
conda activate pcb_defect

# GPU 확인
python -c "import torch; print(torch.cuda.is_available())"

# 데이터셋 다운로드 및 준비
# (데이터셋 가이드 참조)

# YOLO 모델 학습 테스트
python yolo/train_yolo.py --config configs/yolo_config.yaml
```

#### 라즈베리파이 팀

```bash
# 라즈베리파이 OS 설치 및 업데이트
sudo apt update && sudo apt upgrade -y

# Python 패키지 설치
pip3 install opencv-python requests RPi.GPIO python-dotenv

# 웹캠 테스트
python3 -c "import cv2; cap = cv2.VideoCapture(0); print('Webcam OK' if cap.isOpened() else 'Webcam Error')"

# GPIO 핀 테스트 (주의: 실제 릴레이 연결 필요)
python3 -c "import RPi.GPIO as GPIO; GPIO.setmode(GPIO.BCM); print('GPIO OK')"
```

#### C# 앱 팀

```bash
# Visual Studio 2022 또는 Rider 설치
# .NET 6.0 SDK 설치 확인
dotnet --version

# NuGet 패키지 복원
cd csharp_winforms/PCB_Inspection_Monitor
dotnet restore

# 빌드 테스트
dotnet build

# 실행 테스트
dotnet run
```

---

## 📅 일일 워크플로우

### 아침 (작업 시작 전)

```bash
# 1. develop 브랜치 최신화
git checkout develop
git pull origin develop

# 2. 내 브랜치로 이동 및 병합
git checkout feature/<팀명>
git merge develop

# 3. 충돌 발생 시 해결
# (충돌 해결 전략 섹션 참조)

# 4. 팀 채팅방에서 오늘 작업 공유
# 예: "오늘은 /predict API 엔드포인트 개발 예정"
```

### 작업 중

```bash
# 1. 작은 단위로 자주 커밋
git add <파일명>
git commit -m "feat: Add /predict endpoint for single frame inference"

# 2. 원격 브랜치에 푸시 (백업)
git push origin feature/<팀명>

# 3. API 변경이 있다면 팀 채팅방에 즉시 공지
# 예: "⚠️ /predict API 응답 형식에 'anomaly_score' 필드 추가했습니다"
```

### 저녁 (작업 종료 전)

```bash
# 1. 최종 커밋 및 푸시
git add .
git commit -m "feat: Complete /predict API implementation"
git push origin feature/<팀명>

# 2. Pull Request 생성 (작업 완료 시)
# GitHub에서 feature/<팀명> → develop PR 생성
# PR 템플릿에 따라 체크리스트 작성

# 3. 팀 채팅방에서 오늘 작업 결과 공유
# 예: "오늘 /predict API 완료. PR #12 생성했습니다. 리뷰 부탁드립니다."
```

---

## ⚠️ 충돌 해결 전략

### 1. 코드 충돌 (Git Merge Conflict)

#### 충돌 예시
```python
<<<<<<< HEAD
server_url = "http://100.64.1.1:5000"
=======
server_url = "http://100.x.x.x:5000"  # Tailscale
>>>>>>> develop
```

#### 해결 방법 1: 환경 변수 사용
```python
import os
server_url = os.getenv("SERVER_URL", "http://100.64.1.1:5000")
```

#### 해결 방법 2: 설정 파일 분리
```yaml
# configs/server_config.yaml
server:
  host: ${SERVER_HOST:-0.0.0.0}
  port: ${SERVER_PORT:-5000}
```

### 2. 설정 파일 불일치

**문제**: 각 팀원이 다른 IP, 포트, 경로 사용

**해결책**:
1. `.env` 파일 사용 (Git에 올리지 않음)
2. `.env.example` 템플릿 제공
3. `scripts/setup_env.sh` 자동 설정 스크립트

**예시**:
```bash
# Flask 서버 .env
SERVER_URL=http://100.64.1.1:5000  # Tailscale 기본
# SERVER_URL=http://100.x.x.x:5000  # Tailscale (주석 처리)
```

### 3. API 인터페이스 변경 충돌

**문제**: Flask 팀이 API 응답 형식 변경 → 라즈베리파이/C# 팀 코드 깨짐

**해결책**:

#### Step 1: 사전 공지 (최소 1일 전)
```
[팀 채팅방]
Flask 팀: "⚠️ API 변경 예정 공지
- 엔드포인트: /predict
- 변경 내용: 응답에 'anomaly_score' 필드 추가
- 적용 예정일: 10월 26일
- 영향받는 팀: 라즈베리파이, C# 앱
- 테스트: Mock 서버에 먼저 반영했으니 테스트 가능합니다"
```

#### Step 2: API 계약 문서 업데이트
```bash
# 1. docs/API_Contract.md 수정
# 2. 변경 이력 섹션에 기록
# 3. PR 생성 및 영향받는 팀 리뷰 요청
```

#### Step 3: Mock 서버 먼저 업데이트
```bash
# Mock 서버에 변경사항 반영
python tests/api/mock_server.py

# 다른 팀이 Mock 서버로 테스트 가능
curl http://localhost:5000/predict
```

#### Step 4: 계약 테스트 실행
```bash
# API 계약이 깨지지 않았는지 검증
pytest tests/api/test_api_contract.py -v
```

#### Step 5: 실제 Flask 서버에 적용
```bash
# 모든 팀 승인 후 실제 서버 배포
```

---

## 💬 커뮤니케이션 규칙

### 1. 팀 채팅방 활용

#### 필수 공지 사항
- ⚠️ **API 변경**: 즉시 공지 (최소 1일 전)
- 🔧 **데이터베이스 스키마 변경**: 최소 2일 전 공지
- 🐛 **긴급 버그 수정**: 즉시 공지 및 Hotfix 브랜치 생성
- 📝 **설정 파일 변경**: 새로운 환경 변수 추가 시 공지

#### 공지 템플릿
```
[분류] 제목
- 담당 팀: Flask 서버 팀
- 변경 내용: /predict API에 'request_id' 필드 추가
- 영향받는 팀: 라즈베리파이 팀, C# 앱 팀
- 적용 예정일: 10월 26일
- 관련 문서: docs/API_Contract.md
- PR 링크: #15
```

### 2. 주간 회의 (매주 월요일)

**아젠다**:
1. 지난주 작업 결과 공유 (각 팀 5분)
2. 이번주 작업 계획 공유 (각 팀 5분)
3. 충돌 또는 이슈 논의 (20분)
4. API 변경 사항 협의 (10분)

### 3. PR 리뷰 규칙

- **리뷰 담당**: CODEOWNERS 파일 기준
- **리뷰 기한**: PR 생성 후 24시간 이내
- **승인 기준**: 최소 1명 이상 승인 (API 변경 시 영향받는 팀 모두 승인)

**리뷰 체크리스트**:
- [ ] 코드 스타일 준수 (PEP 8)
- [ ] API 계약 준수
- [ ] 테스트 통과
- [ ] 문서 업데이트 (API 변경 시)
- [ ] .env.example 업데이트 (설정 변경 시)

---

## 🚨 문제 발생 시 대응

### 문제 유형별 대응 방법

| 문제 | 심각도 | 대응 시간 | 대응 방법 |
|------|--------|----------|-----------|
| Git 충돌 해결 안 됨 | 중간 | 1시간 | 팀 리더에게 연락, 화상 회의 |
| API 변경으로 코드 깨짐 | 높음 | 즉시 | 긴급 회의, 롤백 검토 |
| 데이터베이스 스키마 충돌 | 높음 | 즉시 | 백업 복원, 마이그레이션 재검토 |
| 환경 설정 문제 | 낮음 | 1일 | `.env` 파일 재설정, setup_env.sh 재실행 |
| 모델 파일 용량 초과 | 중간 | 1일 | Git LFS 설정 또는 외부 스토리지 사용 |

### 긴급 연락망

```
팀 리더: [연락처]
Flask 팀 리더: [연락처]
AI 모델 팀 리더: [연락처]
라즈베리파이 팀 리더: [연락처]
C# 앱 팀 리더: [연락처]
```

### 롤백 절차

```bash
# 1. 문제가 있는 커밋 확인
git log --oneline

# 2. 이전 커밋으로 롤백
git revert <커밋 해시>

# 3. 원격 저장소에 푸시
git push origin feature/<팀명>

# 4. 팀 채팅방에 공지
# "⚠️ 롤백 완료: [이유] 때문에 [커밋] 되돌렸습니다"
```

---

## 📚 추가 참고 자료

- [Git 워크플로우 가이드](Git_Workflow.md)
- [API 계약 명세서](API_Contract.md)
- [Flask 서버 구축 가이드](Flask_Server_Setup.md)
- [라즈베리파이 설정 가이드](RaspberryPi_Setup.md)
- [C# WinForms 개발 가이드](CSharp_WinForms_Guide.md)

---

## ✅ 체크리스트

### 프로젝트 시작 전
- [ ] 저장소 클론 완료
- [ ] 가상환경 설정 완료
- [ ] .env 파일 설정 완료
- [ ] Git 브랜치 생성 완료
- [ ] 팀 채팅방 가입 완료
- [ ] CODEOWNERS 파일 확인 완료

### 매일 작업 전
- [ ] develop 브랜치 pull 완료
- [ ] 내 브랜치에 develop 병합 완료
- [ ] 충돌 해결 완료 (있는 경우)
- [ ] 오늘 작업 계획 공유 완료

### 작업 완료 후
- [ ] 코드 스타일 검사 완료
- [ ] 단위 테스트 통과 완료
- [ ] API 계약 준수 확인 완료
- [ ] .env.example 업데이트 완료 (설정 변경 시)
- [ ] PR 생성 및 리뷰 요청 완료

---

**마지막 업데이트**: 2025-10-25
**문서 관리**: 팀 리더
