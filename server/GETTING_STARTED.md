# Flask 서버 팀 시작 가이드

> PCB 불량 검사 시스템 Flask 서버 개발을 시작하는 팀원을 위한 빠른 시작 가이드입니다.

---

## 🎯 Flask 서버 팀의 역할

- **Flask 웹서버 개발**: REST API 엔드포인트 구현
- **AI 모델 통합**: YOLO v8 + 이상 탐지 모델 로드 및 추론 실행
- **데이터베이스 연동**: MySQL에 검사 이력 저장 및 조회
- **API 제공**: 라즈베리파이 클라이언트 및 C# 모니터링 앱과의 통신

---

## 📚 반드시 읽어야 할 문서

### 필수 문서 (우선순위 순)

1. **[Flask_Server_Setup.md](../../docs/Flask_Server_Setup.md)** ⭐ 가장 중요!
   - Flask 서버 전체 구조 및 구현 가이드

2. **[API_Contract.md](../../docs/API_Contract.md)** ⭐ 팀 전체 계약!
   - Flask API 공식 명세서 (라즈베리파이/C# 팀과의 계약)
   - API 변경 시 반드시 이 문서 업데이트 필수

3. **[database/README.md](../../database/README.md)**
   - MySQL 데이터베이스 설정 가이드
   - 테이블 구조 및 사용자 권한

4. **[Team_Collaboration_Guide.md](../../docs/Team_Collaboration_Guide.md)**
   - 팀 협업 규칙 및 일일 워크플로우

5. **[Git_Workflow.md](../../docs/Git_Workflow.md)**
   - Git 브랜치 전략 및 PR 규칙

### 참고 문서

- [Development_Setup.md](../../docs/Development_Setup.md) - 로컬 환경 구성
- [Logging_Strategy.md](../../docs/Logging_Strategy.md) - 통합 로깅 전략
- [database/schema.sql](../../database/schema.sql) - DB 스키마

---

## ⚙️ 개발 환경 설정

### 시스템 요구사항

- **OS**: Ubuntu 20.04 / 22.04
- **GPU**: NVIDIA RTX 4080 Super (16GB VRAM)
- **Python**: 3.10 (Conda 가상환경)
- **MySQL**: 8.0 (Windows PC - Tailscale VPN 연결)

### 1. Conda 가상환경 생성 및 활성화

```bash
# 가상환경 생성 (최초 1회)
conda create -n pcb_defect python=3.10 -y

# 가상환경 활성화
conda activate pcb_defect

# 확인
python --version  # Python 3.10.x
```

### 2. 프로젝트 패키지 설치

```bash
# 프로젝트 루트로 이동
cd /path/to/PCB_Detect_Project

# 패키지 설치
pip install -r requirements.txt

# 주요 패키지 확인
pip list | grep -E "flask|torch|ultralytics|mysql"
```

### 3. 환경 변수 설정

```bash
# 1. 환경 설정 스크립트 실행
bash scripts/setup_env.sh

# 2. .env 파일 수정
nano src/server/.env

# 3. 아래 내용으로 수정:
```

**`src/server/.env` 파일 내용:**

```bash
# MySQL 데이터베이스 (Windows PC - Tailscale)
DB_HOST=100.x.x.x          # Windows PC의 Tailscale IP로 변경
DB_PORT=3306
DB_NAME=pcb_inspection
DB_USER=pcb_server
DB_PASSWORD=1234

# Flask 서버
SERVER_HOST=0.0.0.0
SERVER_PORT=5000

# GPU 설정
GPU_DEVICE=cuda:0          # 또는 cpu

# YOLO 모델
YOLO_MODEL_PATH=models/yolo/final/yolo_best.pt
ANOMALY_MODEL_PATH=models/anomaly/padim/model.pth
```

### 4. MySQL 연결 테스트

```bash
# Windows PC의 Tailscale IP 확인 (Windows에서 실행)
tailscale ip -4

# Ubuntu (GPU PC)에서 MySQL 연결 테스트
mysql -h 100.x.x.x -u pcb_server -p
# 비밀번호: 1234

# MySQL 접속 후 확인
USE pcb_inspection;
SHOW TABLES;
SELECT * FROM inspection_history LIMIT 5;
```

---

## 🚀 Flask 서버 실행 및 테스트

### 1. Flask 서버 실행

```bash
# 가상환경 활성화
conda activate pcb_defect

# Flask 서버 디렉토리로 이동
cd src/server

# 서버 실행
python app.py

# 예상 출력:
# * Running on http://0.0.0.0:5000
# * GPU 사용 가능: True
# * YOLO 모델 로드 완료
```

### 2. 서버 상태 확인

```bash
# 다른 터미널에서 실행
curl http://localhost:5000/api/v1/health

# 예상 응답:
{
  "status": "healthy",
  "server_time": "2025-10-25T14:30:00",
  "gpu_available": true,
  "models_loaded": {
    "yolo": true,
    "anomaly": true
  },
  "version": "1.0.0"
}
```

### 3. API 테스트 (Mock 클라이언트)

Mock 클라이언트는 아직 생성되지 않았으므로, 직접 `curl`로 테스트:

```bash
# 1. 검사 이력 조회
curl -X GET "http://localhost:5000/api/v1/history?page=1&limit=10"

# 2. 통계 데이터 조회
curl -X GET "http://localhost:5000/api/v1/statistics?start_date=2025-10-01&end_date=2025-10-25"

# 3. 단일 프레임 검사 (Base64 인코딩 필요)
# (라즈베리파이 클라이언트 구현 후 테스트 가능)
```

---

## 📝 첫 번째 작업 제안

### 작업 1: Flask 서버 기본 구조 생성

**목표**: `src/server/app.py` 기본 코드 작성

```python
# src/server/app.py
from flask import Flask, jsonify
from flask_cors import CORS
import os
from dotenv import load_dotenv

# 환경 변수 로드
load_dotenv()

app = Flask(__name__)
CORS(app)

# Health Check API
@app.route('/api/v1/health', methods=['GET'])
def health():
    return jsonify({
        "status": "healthy",
        "server_time": "2025-10-25T14:30:00",
        "gpu_available": True,
        "models_loaded": {
            "yolo": False,
            "anomaly": False
        },
        "version": "1.0.0"
    })

if __name__ == '__main__':
    host = os.getenv('SERVER_HOST', '0.0.0.0')
    port = int(os.getenv('SERVER_PORT', 5000))
    app.run(host=host, port=port, debug=True)
```

### 작업 2: MySQL 연결 테스트

**목표**: `src/server/database.py` 생성 및 DB 연결 확인

```python
# src/server/database.py
import pymysql
import os
from dotenv import load_dotenv

load_dotenv()

def get_db_connection():
    """MySQL 데이터베이스 연결 생성"""
    try:
        conn = pymysql.connect(
            host=os.getenv('DB_HOST'),
            port=int(os.getenv('DB_PORT', 3306)),
            user=os.getenv('DB_USER'),
            password=os.getenv('DB_PASSWORD'),
            database=os.getenv('DB_NAME'),
            charset='utf8mb4'
        )
        print("✓ MySQL 연결 성공!")
        return conn
    except Exception as e:
        print(f"✗ MySQL 연결 실패: {e}")
        return None

# 테스트 실행
if __name__ == '__main__':
    conn = get_db_connection()
    if conn:
        cursor = conn.cursor()
        cursor.execute("SELECT COUNT(*) FROM inspection_history")
        count = cursor.fetchone()[0]
        print(f"검사 이력 개수: {count}")
        conn.close()
```

### 작업 3: `/predict` API 구현

**목표**: `docs/API_Contract.md` 참고하여 `/predict` 엔드포인트 구현

1. `docs/API_Contract.md` 읽기
2. 요청 형식 확인 (camera_id, image, timestamp)
3. 응답 형식 확인 (classification, confidence, defects, gpio_action)
4. `src/server/app.py`에 `/predict` 엔드포인트 추가

---

## 🤖 AI에게 물어볼 프롬프트

### 시작 프롬프트 (복사해서 사용하세요)

```
안녕! 나는 PCB 불량 검사 시스템의 Flask 서버 팀원이야.

**내 역할:**
- Flask 웹서버 개발 (REST API)
- AI 모델 (YOLO, 이상 탐지) 로드 및 추론 실행
- MySQL 데이터베이스 연동 (검사 이력 저장)
- 라즈베리파이 및 C# 앱과의 API 통신

**읽어야 할 핵심 문서:**
1. `docs/Flask_Server_Setup.md` - Flask 서버 구축 가이드 (필수!)
2. `docs/API_Contract.md` - 공식 API 명세서 (팀 전체 계약)
3. `database/README.md` - MySQL 데이터베이스 설정 가이드
4. `database/schema.sql` - 데이터베이스 스키마
5. `src/server/.env.example` - 환경 변수 템플릿

**개발 환경:**
- OS: Ubuntu 22.04 (GPU PC)
- GPU: NVIDIA RTX 4080 Super
- Python: 3.10 (Conda 가상환경 `pcb_defect`)
- 데이터베이스: MySQL 8.0 (Windows PC - Tailscale 100.x.x.x:3306)
- DB 계정: `pcb_server` / 비밀번호: `1234`

**환경 변수 설정 (src/server/.env):**
```
DB_HOST=100.x.x.x          # Windows PC의 Tailscale IP
DB_PORT=3306
DB_NAME=pcb_inspection
DB_USER=pcb_server
DB_PASSWORD=1234

SERVER_HOST=0.0.0.0
SERVER_PORT=5000
GPU_DEVICE=cuda:0
```

**첫 번째 작업:**
1. Conda 가상환경 활성화: `conda activate pcb_defect`
2. Flask 서버 실행: `cd src/server && python app.py`
3. 서버 상태 확인: `curl http://localhost:5000/api/v1/health`
4. MySQL 연결 테스트

위 정보를 바탕으로, Flask 서버를 처음 실행하고 테스트하는 과정을 단계별로 안내해줘.
특히 MySQL 원격 연결이 제대로 되는지 확인하는 방법도 알려줘.
```

---

## ✅ 체크리스트

### 환경 설정 완료 체크리스트

- [ ] Conda 가상환경 생성 및 활성화 완료
- [ ] `requirements.txt` 패키지 설치 완료
- [ ] `src/server/.env` 파일 설정 완료
- [ ] MySQL 데이터베이스 연결 테스트 성공
- [ ] Flask 서버 실행 확인 (`/health` API 응답)
- [ ] GPU 사용 가능 확인 (`torch.cuda.is_available()`)

### 문서 읽기 체크리스트

- [ ] `docs/Flask_Server_Setup.md` 읽기 완료
- [ ] `docs/API_Contract.md` 읽기 완료
- [ ] `database/README.md` 읽기 완료
- [ ] `docs/Team_Collaboration_Guide.md` 읽기 완료
- [ ] `docs/Git_Workflow.md` 읽기 완료

### Git 설정 체크리스트

- [ ] `develop` 브랜치에서 `feature/flask-server` 브랜치 생성 완료
- [ ] `.env` 파일이 `.gitignore`에 포함되어 있는지 확인
- [ ] 첫 번째 커밋 및 푸시 완료

---

## 🚨 자주 발생하는 문제 및 해결

### 문제 1: MySQL 연결 실패

**에러**: `Can't connect to MySQL server on '100.x.x.x'`

**해결 방법:**
1. Tailscale VPN 연결 확인: `tailscale status`
2. Windows PC의 MySQL 서버 실행 중인지 확인
3. Windows 방화벽에서 3306 포트 허용
4. MySQL `bind-address` 설정 확인 (Windows: `my.ini` 파일)

### 문제 2: GPU 인식 안 됨

**에러**: `torch.cuda.is_available()` 반환값이 `False`

**해결 방법:**
1. CUDA 드라이버 설치 확인: `nvidia-smi`
2. PyTorch CUDA 버전 재설치:
   ```bash
   pip uninstall torch torchvision torchaudio
   pip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cu118
   ```

### 문제 3: 환경 변수 로드 안 됨

**문제**: `.env` 파일의 환경 변수가 인식되지 않음

**해결 방법:**
1. `python-dotenv` 설치 확인: `pip install python-dotenv`
2. `.env` 파일 위치 확인 (반드시 `src/server/.env`)
3. 코드에서 `load_dotenv()` 호출 확인

---

## 📞 도움 요청

- **Flask 팀 리더**: [연락처]
- **전체 팀 채팅방**: [링크]
- **긴급 문제**: `docs/Team_Collaboration_Guide.md` 참조

---

**마지막 업데이트**: 2025-10-25
**작성자**: 팀 리더
