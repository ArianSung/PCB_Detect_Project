# Git 워크플로우 가이드

PCB 불량 검사 시스템 팀 프로젝트를 위한 Git 브랜치 전략 및 협업 규칙

---

## 📌 브랜치 전략

### 브랜치 구조

```
main (프로덕션 - 최종 배포 버전)
  └── develop (개발 통합 브랜치)
        ├── feature/flask-server (Flask 서버 팀)
        ├── feature/raspberry-pi (라즈베리파이 팀)
        ├── feature/ai-model (AI 모델 팀)
        ├── feature/csharp-app (C# 모니터링 앱 팀)
        └── hotfix/버그명 (긴급 버그 수정)
```

### 브랜치별 역할

| 브랜치 | 역할 | 병합 규칙 |
|--------|------|-----------|
| `main` | 최종 배포 버전 (안정적인 릴리스) | PR + 전체 팀 리뷰 필수 |
| `develop` | 개발 통합 브랜치 (모든 기능 병합) | PR + 담당 팀 리뷰 필수 |
| `feature/*` | 기능별 개발 브랜치 | PR + 코드 리뷰 필수 |
| `hotfix/*` | 긴급 버그 수정 | PR + 팀 리더 승인 필수 |

---

## 🚀 작업 프로세스

### 1. 새로운 작업 시작

```bash
# 1. develop 브랜치 최신 상태로 업데이트
git checkout develop
git pull origin develop

# 2. 기능 브랜치 생성 (팀별 브랜치에서 분기)
git checkout -b feature/flask-server  # Flask 팀
git checkout -b feature/raspberry-pi  # 라즈베리파이 팀
git checkout -b feature/ai-model      # AI 모델 팀
git checkout -b feature/csharp-app    # C# 앱 팀

# 3. 개인 작업 브랜치 생성 (선택 사항)
git checkout -b feature/flask-server/add-prediction-api
```

### 2. 작업 중 커밋

```bash
# 변경사항 스테이징
git add src/server/app.py

# 커밋 메시지 작성 (규칙 준수)
git commit -m "feat: Add /predict API endpoint for PCB defect detection"
```

**커밋 메시지 규칙:**
```
<타입>: <제목>

<본문> (선택)

<푸터> (선택)
```

**타입 종류:**
- `feat`: 새로운 기능 추가
- `fix`: 버그 수정
- `docs`: 문서 수정
- `style`: 코드 포맷팅 (기능 변경 없음)
- `refactor`: 코드 리팩토링
- `test`: 테스트 코드 추가/수정
- `chore`: 빌드 설정, 패키지 업데이트 등

**예시:**
```
feat: Add real-time PCB defect detection API

- Implement /predict endpoint for single frame inference
- Add YOLO model loading and caching
- Return JSON response with defect classification

Closes #15
```

### 3. 원격 저장소에 푸시

```bash
# 원격 브랜치에 푸시
git push origin feature/flask-server

# 최초 푸시 시 upstream 설정
git push -u origin feature/flask-server
```

### 4. Pull Request (PR) 생성

1. GitHub에서 `feature/flask-server` → `develop` PR 생성
2. PR 템플릿에 따라 체크리스트 작성
3. 담당 팀원을 Reviewer로 지정
4. API 변경이 있다면 **반드시 명시**

### 5. 코드 리뷰 및 병합

- **최소 1명 이상의 팀원 승인** 필요
- API 변경 시: 영향받는 팀 모두 승인 필요
- 충돌 발생 시: 본인이 해결 후 재요청

```bash
# 충돌 해결 프로세스
git checkout develop
git pull origin develop
git checkout feature/flask-server
git merge develop  # 충돌 발생 시 수동 해결
git push origin feature/flask-server
```

---

## ⚠️ 충돌 방지 규칙

### 1. 매일 develop 브랜치 동기화

```bash
# 매일 오전, 작업 시작 전 실행
git checkout develop
git pull origin develop
git checkout feature/flask-server
git merge develop
```

### 2. 작은 단위로 자주 커밋 & PR

- **권장**: 하루 1~2개의 작은 PR
- **지양**: 1주일 작업한 큰 PR (충돌 위험 ↑)

### 3. 파일별 담당 영역 준수

| 팀 | 담당 디렉토리 | 절대 수정 금지 |
|----|--------------|---------------|
| Flask 팀 | `src/server/`, `src/inference/` | `raspberry_pi/`, `csharp_winforms/` |
| 라즈베리파이 팀 | `raspberry_pi/`, `configs/camera_config.yaml` | `src/server/`, `csharp_winforms/` |
| AI 모델 팀 | `src/models/`, `src/training/`, `src/evaluation/` | `raspberry_pi/`, `csharp_winforms/` |
| C# 앱 팀 | `csharp_winforms/` | `src/server/`, `raspberry_pi/` |

**예외:** 공통 설정 파일 (`configs/`, `docs/API_Contract.md`)은 전체 팀 합의 필요

---

## 🔗 API 변경 시 특별 규칙

### API 변경이 발생하는 경우

1. **Flask 팀이 API 응답 형식 변경**
2. **라즈베리파이 팀이 요청 형식 변경**
3. **데이터베이스 스키마 변경으로 API 영향**

### 필수 절차

1. **사전 공지**: 팀 채팅방에 변경 계획 공유
2. **API 계약 문서 업데이트**: `docs/API_Contract.md` 수정
3. **Mock 서버 업데이트**: `tests/mock_server.py` 반영
4. **계약 테스트 실행**: `pytest tests/api_contract_test.py`
5. **PR에 명시**: "⚠️ API 변경 있음" 체크
6. **영향받는 팀 모두 승인**: Flask + 라즈베리파이 + C# 팀

---

## 🛠️ 긴급 버그 수정 (Hotfix)

### 프로세스

```bash
# 1. main 브랜치에서 hotfix 브랜치 생성
git checkout main
git checkout -b hotfix/fix-gpio-pin-error

# 2. 버그 수정 후 커밋
git commit -m "fix: Correct GPIO pin mapping for defect classification"

# 3. main과 develop 양쪽에 병합
git checkout main
git merge hotfix/fix-gpio-pin-error
git push origin main

git checkout develop
git merge hotfix/fix-gpio-pin-error
git push origin develop

# 4. hotfix 브랜치 삭제
git branch -d hotfix/fix-gpio-pin-error
```

---

## 📝 .gitignore 주의사항

### 절대 Git에 올리면 안 되는 파일

```gitignore
# 환경 변수 (개인 설정)
.env
*.env

# 모델 파일 (용량 큰 파일)
*.pt
*.pth
*.onnx

# 데이터셋 (Git LFS 사용 권장)
data/raw/
data/processed/

# 실행 결과
runs/
logs/*.log

# IDE 설정
.vscode/
.idea/
```

### Git에 올려야 하는 파일

```
.env.example        # 환경 변수 템플릿
*.yaml              # 설정 파일
requirements.txt    # 패키지 의존성
README.md           # 문서
```

---

## 🚨 충돌 발생 시 해결 방법

### 충돌 예시

```bash
<<<<<<< HEAD
server_url = "http://192.168.0.10:5000"
=======
server_url = "http://100.x.x.x:5000"  # Tailscale
>>>>>>> develop
```

### 해결 방법

1. **환경 변수로 변경** (권장)
```python
import os
server_url = os.getenv("SERVER_URL", "http://192.168.0.10:5000")
```

2. **설정 파일 분리**
```yaml
# configs/server_config.yaml
server:
  host: ${SERVER_HOST:-0.0.0.0}
  port: ${SERVER_PORT:-5000}
```

3. **수동 병합**
```bash
# 충돌 파일 수정 후
git add <파일명>
git commit -m "merge: Resolve conflict in server URL configuration"
```

---

## 📊 브랜치 상태 확인

```bash
# 현재 브랜치 확인
git branch

# 원격 브랜치 확인
git branch -r

# 브랜치 간 차이 확인
git diff develop..feature/flask-server

# 커밋 히스토리 확인
git log --oneline --graph --all
```

---

## 🎯 팀원별 체크리스트

### 작업 시작 전
- [ ] `git pull origin develop` 실행
- [ ] 최신 `.env.example` 파일 확인
- [ ] API 계약 문서 (`docs/API_Contract.md`) 확인

### 작업 완료 후
- [ ] 코드 스타일 검사 (PEP 8)
- [ ] 단위 테스트 작성
- [ ] API 변경 여부 확인
- [ ] PR 템플릿 작성 완료
- [ ] 담당 팀원에게 리뷰 요청

### PR 승인 전
- [ ] 모든 테스트 통과 확인
- [ ] 충돌 해결 완료
- [ ] API 변경 시 영향받는 팀 승인 대기

---

## 🔄 정기 동기화 일정

| 시간 | 작업 | 담당 |
|------|------|------|
| 매일 오전 9시 | develop 브랜치 pull | 전체 팀원 |
| 매주 월요일 | 주간 개발 계획 공유 | 팀 리더 |
| 매주 금요일 | develop → main 병합 검토 | 전체 팀 |

---

## 📞 문제 발생 시

1. **충돌 해결 안 됨**: 팀 리더에게 연락
2. **API 변경 필요**: 전체 팀 회의 소집
3. **긴급 버그**: Hotfix 브랜치 생성 및 즉시 수정

---

**마지막 업데이트**: 2025-10-25
**문서 관리**: 팀 리더
