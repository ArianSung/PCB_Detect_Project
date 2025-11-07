# 원격 네트워크 연결 가이드 (Tailscale VPN)

## 개요

이 가이드는 GPU PC (RTX 4080 Super 추론 서버)가 라즈베리파이 및 Windows PC와 **다른 위치**에 있을 때, Tailscale VPN을 사용하여 안전하고 간편하게 연결하는 방법을 설명합니다.

---

## 시스템 요구사항

### 네트워크 환경
- GPU PC: 원격지 (예: 집, 다른 캠퍼스, 연구실 등)
- 라즈베리파이 3대 + Windows PC: 같은 위치 (프로젝트 현장)
- 인터넷 연결 필수 (모든 장비)

### 실시간 처리 가능 여부 ✅
- **디팔렛타이저 허용 시간**: 2.5초
- **AI 처리 시간** (원격 연결 포함): 100-200ms
- **여유 시간**: 2.3초 이상 (10배 이상 여유!)

**결론: 원격 연결로도 충분히 실시간 처리 가능**

---

## 시간 분석 (원격 연결)

### 처리 단계별 소요 시간

| 단계 | 소요 시간 | 설명 |
|------|----------|------|
| 이미지 인코딩 (라즈베리파이) | 10-20ms | JPEG 압축 + Base64 인코딩 |
| 네트워크 전송 (업로드) | 20-50ms | 같은 도시 내 VPN 연결 |
| AI 추론 (GPU PC) | 15-20ms | YOLOv11m + 이상 탐지 |
| 네트워크 응답 (다운로드) | 20-50ms | 결과 JSON 전송 |
| GPIO 제어 | 1-5ms | 릴레이 모듈 제어 |
| **총 처리 시간** | **66-145ms** | **목표 300ms 대비 충분** ✅ |

### 디팔렛타이저 동작 흐름
```
1. PCB 컨베이어 벨트 진입
2. 웹캠 촬영 (즉시)
3. AI 추론 + 결과 수신 ──────────┐ 100-200ms
4. GPIO 신호 전송 ─────────────┘
5. 디팔렛타이저 이동 ───────────┐
6. PCB 집기 ───────────────────┤ 1-2초
7. 분류함에 배치 ──────────────┘
```

**총 소요 시간**: 약 1.1-2.2초 (2.5초 허용 시간 이내) ✅

---

## Tailscale VPN 선택 이유

### 장점 ⭐⭐⭐⭐⭐
1. **설치 5분 완료** - 초간단 설정
2. **무료** - 개인/소규모 프로젝트 무료
3. **낮은 지연** - 같은 도시 내 10-30ms
4. **자동 암호화** - 보안 우수 (WireGuard 기반)
5. **방화벽 불필요** - NAT Traversal 자동
6. **고정 IP** - 100.x.x.x 고정 (재부팅 후에도 유지)
7. **크로스 플랫폼** - Linux, Windows, macOS, iOS, Android 지원

### 단점
- 인터넷 연결 필수 (오프라인 불가)
- Tailscale 서비스 의존성

### 대안 비교

| 방법 | 난이도 | 보안 | 지연 | 비용 | 권장도 |
|------|--------|------|------|------|--------|
| **Tailscale** | ⭐ 쉬움 | ✅ 높음 | ✅ 낮음 | 무료 | ⭐⭐⭐⭐⭐ |
| WireGuard | ⭐⭐⭐ 어려움 | ✅ 높음 | ✅ 낮음 | 무료 | ⭐⭐⭐ |
| 포트포워딩 | ⭐⭐ 보통 | ⚠️ 낮음 | ✅ 낮음 | 무료 | ⭐⭐ |
| ngrok | ⭐ 쉬움 | ✅ 높음 | ⚠️ 높음 | 유료 | ⭐⭐ |

---

## Tailscale 설치 및 설정

### 1단계: GPU PC (WSL/Linux)에서 Tailscale 설치

```bash
# Tailscale 설치
curl -fsSL https://tailscale.com/install.sh | sh

# Tailscale 시작 및 로그인
sudo tailscale up

# 브라우저가 열리면 Google/GitHub/Microsoft 계정으로 로그인
# WSL 환경이라면 출력된 URL을 복사하여 Windows 브라우저에서 열기
```

**Tailscale IP 확인**:
```bash
# Tailscale 인터페이스 확인
ip addr show tailscale0

# 또는
tailscale ip -4

# 출력 예시: 100.64.1.1
```

### 2단계: 라즈베리파이 1 (좌측 웹캠)에서 설치

```bash
# Tailscale 설치
curl -fsSL https://tailscale.com/install.sh | sh

# Tailscale 시작 (같은 계정으로 로그인)
sudo tailscale up

# Tailscale IP 확인
tailscale ip -4
# 출력 예시: 100.64.1.2
```

### 3단계: 라즈베리파이 2 (우측 웹캠)에서 설치

```bash
# 동일한 명령어 실행
curl -fsSL https://tailscale.com/install.sh | sh
sudo tailscale up

# Tailscale IP 확인
tailscale ip -4
# 출력 예시: 100.64.1.3
```

### 4단계: 라즈베리파이 3번 (OHT 컨트롤러)에서 설치

```bash
# 동일한 명령어 실행
curl -fsSL https://tailscale.com/install.sh | sh
sudo tailscale up

# Tailscale IP 확인
tailscale ip -4
# 출력 예시: 100.64.1.4
```

### 5단계: Windows PC (모니터링 앱)에서 설치

1. **Tailscale 다운로드**:
   - https://tailscale.com/download/windows 접속
   - 설치 프로그램 다운로드 및 실행

2. **Tailscale 시작**:
   - 설치 후 자동 실행
   - 트레이 아이콘 클릭 → "Login" → 동일한 계정으로 로그인

3. **Tailscale IP 확인**:
   - 트레이 아이콘 클릭 → "My IP"
   - 출력 예시: 100.64.1.5

---

## IP 주소 매핑 예시

| 장비 | 역할 | Tailscale IP (예시) |
|------|------|---------------------|
| GPU PC | Flask 추론 서버 | 100.64.1.1 |
| 라즈베리파이 1 | 좌측 웹캠 + GPIO | 100.64.1.2 |
| 라즈베리파이 2 | 우측 웹캠 | 100.64.1.3 |
| 라즈베리파이 3번 | OHT 컨트롤러 | 100.64.1.4 |
| Windows PC | C# 모니터링 앱 | 100.64.1.5 |

**참고**: 실제 IP는 Tailscale이 자동 할당하므로 위 예시와 다를 수 있습니다.

---

## Flask 서버 설정 (GPU PC)

### Flask 서버 실행

```bash
# 가상환경 활성화
conda activate pcb_defect

# Flask 서버 실행 (모든 인터페이스에 바인딩)
cd ~/work_project/server
python app.py

# 출력:
#  * Running on http://0.0.0.0:5000/
#  * Running on http://100.64.1.1:5000/  ← Tailscale IP
```

**중요**: `host='0.0.0.0'`으로 설정하여 Tailscale 인터페이스에서도 접근 가능하도록 해야 합니다.

### 서버 접근 확인

**GPU PC 로컬에서 테스트**:
```bash
curl http://localhost:5000/health
curl http://100.64.1.1:5000/health
```

**라즈베리파이에서 테스트**:
```bash
curl http://100.64.1.1:5000/health

# 정상 응답 예시:
# {"status":"ok","timestamp":"2025-10-23T10:30:00"}
```

---

## 라즈베리파이 클라이언트 설정

### camera_client.py 수정

```python
# raspberry_pi/camera_client.py

# Tailscale IP로 서버 URL 설정
SERVER_URL = 'http://100.64.1.1:5000'  # GPU PC의 Tailscale IP

# 또는 환경 변수로 관리
import os
SERVER_URL = os.getenv('FLASK_SERVER_URL', 'http://100.64.1.1:5000')
```

### 환경 변수 설정 (권장)

```bash
# ~/.bashrc 또는 ~/.zshrc에 추가
export FLASK_SERVER_URL='http://100.64.1.1:5000'

# 적용
source ~/.bashrc
```

### 클라이언트 실행 및 테스트

```bash
cd ~/work_project/raspberry_pi
python3 camera_client.py left 0 http://100.64.1.1:5000 10

# 출력에서 네트워크 지연 확인:
# [left] Result: 정상 (confidence: 0.95, inference: 18.5ms)
# Total latency: 125ms  ← 전체 처리 시간
```

---

## C# WinForms 모니터링 앱 설정

### App.config 또는 appsettings.json 수정

```xml
<!-- App.config -->
<appSettings>
  <add key="FlaskServerUrl" value="http://100.64.1.1:5000" />
  <add key="MySqlHost" value="100.64.1.1" />
  <add key="MySqlPort" value="3306" />
</appSettings>
```

또는

```json
// appsettings.json
{
  "FlaskServer": {
    "Url": "http://100.64.1.1:5000"
  },
  "Database": {
    "Host": "100.64.1.1",
    "Port": 3306,
    "Database": "pcb_inspection"
  }
}
```

### 코드에서 URL 사용

```csharp
using System.Configuration;
using System.Net.Http;

// Flask 서버 URL 읽기
string flaskServerUrl = ConfigurationManager.AppSettings["FlaskServerUrl"];

// REST API 호출
HttpClient client = new HttpClient();
var response = await client.GetAsync($"{flaskServerUrl}/api/statistics");
```

---

## 네트워크 지연 측정

### ping 테스트

```bash
# 라즈베리파이 또는 Windows PC에서 실행
ping 100.64.1.1

# 출력 예시:
# 64 bytes from 100.64.1.1: icmp_seq=1 ttl=64 time=25.3 ms
# 64 bytes from 100.64.1.1: icmp_seq=2 ttl=64 time=23.8 ms
```

**정상 범위**: 10-50ms (같은 도시 내)

### HTTP 응답 시간 측정

```bash
# curl로 응답 시간 측정
curl -o /dev/null -s -w "Time: %{time_total}s\n" http://100.64.1.1:5000/health

# 출력 예시:
# Time: 0.045s  (45ms)
```

### Python 코드로 측정

```python
import requests
import time

SERVER_URL = 'http://100.64.1.1:5000'

# 10번 반복 측정
latencies = []
for i in range(10):
    start = time.time()
    response = requests.get(f"{SERVER_URL}/health", timeout=5)
    elapsed = (time.time() - start) * 1000
    latencies.append(elapsed)
    print(f"Attempt {i+1}: {elapsed:.2f}ms")

print(f"\nAverage: {sum(latencies)/len(latencies):.2f}ms")
print(f"Min: {min(latencies):.2f}ms")
print(f"Max: {max(latencies):.2f}ms")
```

---

## 문제 해결 (Troubleshooting)

### 문제 1: Tailscale에 연결할 수 없음

**증상**: `ping 100.x.x.x` 실패

**해결책**:
1. Tailscale 상태 확인:
```bash
sudo tailscale status

# 정상 출력:
# 100.64.1.1  gpu-pc    user@   linux   active; direct 192.168.1.100:41641
# 100.64.1.2  rpi-left  user@   linux   active; relay "tokyo", tx 1234 rx 5678
```

2. Tailscale 재시작:
```bash
sudo tailscale down
sudo tailscale up
```

3. 방화벽 확인:
```bash
# Ubuntu/WSL
sudo ufw status
sudo ufw allow 41641/udp  # Tailscale 포트

# 라즈베리파이
sudo iptables -L
```

### 문제 2: Flask 서버에 접근할 수 없음

**증상**: `curl http://100.64.1.1:5000/health` 실패

**해결책**:
1. Flask 서버가 실행 중인지 확인:
```bash
# GPU PC에서
ps aux | grep python | grep app.py
```

2. Flask가 올바른 인터페이스에 바인딩되었는지 확인:
```bash
# app.py에서 확인
app.run(host='0.0.0.0', port=5000)  # 모든 인터페이스
```

3. 방화벽에서 5000 포트 오픈:
```bash
sudo ufw allow 5000/tcp
```

4. Flask 로그 확인:
```bash
tail -f logs/flask.log
```

### 문제 3: 네트워크 지연이 너무 높음 (>200ms)

**원인**: 네트워크 품질 문제 또는 Relay 서버 경유

**해결책**:
1. 직접 연결 여부 확인:
```bash
sudo tailscale status | grep direct

# 직접 연결: "direct 192.168.1.100:41641"
# 릴레이 경유: "relay \"tokyo\""
```

2. 이미지 압축률 증가:
```python
# camera_client.py
_, buffer = cv2.imencode('.jpg', frame, [cv2.IMWRITE_JPEG_QUALITY, 70])  # 85 → 70
```

3. 이미지 해상도 낮추기:
```python
cap.set(cv2.CAP_PROP_FRAME_WIDTH, 416)  # 640 → 416
cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 416)  # 480 → 416
```

### 문제 4: MySQL 원격 접근 실패

**증상**: C# WinForms에서 데이터베이스 연결 실패

**해결책**:
1. MySQL이 외부 접근을 허용하도록 설정:
```bash
# GPU PC에서
sudo nano /etc/mysql/mysql.conf.d/mysqld.cnf

# bind-address 수정
bind-address = 0.0.0.0  # 127.0.0.1 → 0.0.0.0

# MySQL 재시작
sudo systemctl restart mysql
```

2. MySQL 사용자 권한 설정:
```sql
-- root 사용자에게 원격 접근 권한 부여
GRANT ALL PRIVILEGES ON pcb_inspection.* TO 'root'@'%' IDENTIFIED BY 'your_password';
FLUSH PRIVILEGES;
```

3. 방화벽에서 3306 포트 오픈:
```bash
sudo ufw allow 3306/tcp
```

---

## 보안 고려사항

### 1. Tailscale 장점 (기본 보안)
- ✅ WireGuard 기반 암호화 (end-to-end)
- ✅ 각 장비는 Tailscale 계정으로 인증
- ✅ 공용 IP 노출 없음 (NAT Traversal)

### 2. 추가 보안 (선택 사항)

#### API 인증 토큰 추가 (Flask)
```python
# server/app.py
from functools import wraps

API_TOKEN = 'your-secret-token-here'  # 환경 변수로 관리 권장

def require_auth(f):
    @wraps(f)
    def decorated_function(*args, **kwargs):
        token = request.headers.get('Authorization')
        if token != f'Bearer {API_TOKEN}':
            return jsonify({'error': 'Unauthorized'}), 401
        return f(*args, **kwargs)
    return decorated_function

@app.route('/predict', methods=['POST'])
@require_auth  # 인증 필수
def predict():
    # ...
```

#### 클라이언트 측 토큰 전송
```python
# camera_client.py
headers = {
    'Authorization': 'Bearer your-secret-token-here',
    'Content-Type': 'application/json'
}

response = requests.post(
    f"{SERVER_URL}/predict",
    json=data,
    headers=headers,
    timeout=5
)
```

---

## 성능 최적화

### 1. 이미지 전송 최적화

```python
# camera_client.py

# 해상도 최적화 (640x480 권장)
cap.set(cv2.CAP_PROP_FRAME_WIDTH, 640)
cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 480)

# JPEG 품질 조절 (70-85 권장)
_, buffer = cv2.imencode('.jpg', frame, [cv2.IMWRITE_JPEG_QUALITY, 85])

# 프레임 압축 예상 크기:
# - 640x480, 품질 85: 약 30-50 KB
# - 640x480, 품질 70: 약 20-30 KB
```

### 2. 네트워크 대역폭 계산

```
프레임당 크기: 40 KB (평균)
FPS: 10
초당 데이터: 40 KB × 10 = 400 KB/s = 3.2 Mbps

필요 업로드 속도: 최소 5 Mbps (여유 포함)
```

### 3. 비동기 처리 (고급)

```python
# camera_client.py에 비동기 전송 추가
import threading
from queue import Queue

# 프레임 큐
frame_queue = Queue(maxsize=5)

def sender_thread():
    """백그라운드 전송 스레드"""
    while True:
        frame = frame_queue.get()
        send_frame(frame)

# 스레드 시작
threading.Thread(target=sender_thread, daemon=True).start()

# 메인 루프에서 큐에 프레임 추가
while True:
    ret, frame = cap.read()
    if ret and not frame_queue.full():
        frame_queue.put(frame)
```

---

## 대안: 하이브리드 아키텍처 (선택 사항)

네트워크 지연이 문제가 될 경우, 로컬 경량 모델 + 원격 정밀 검증 구조 고려

### 구조
```
[라즈베리파이]
  ├─ YOLOv11n (경량 모델) 로컬 실행 (20-30ms)
  ├─ 빠른 판정 → 즉시 GPIO 제어
  └─ 백그라운드로 GPU PC 전송 → YOLOv11m 재검증 → DB 저장
```

### 장점
- 로컬 즉시 반응 (20-30ms)
- 원격 정밀 검증 (데이터 수집)
- 네트워크 끊김 시에도 동작

**참고**: 라즈베리파이에 YOLOv11n 배포는 `RaspberryPi_Setup.md` 참조

---

## 참고 자료

- [Tailscale 공식 문서](https://tailscale.com/kb/)
- [Tailscale 설치 가이드](https://tailscale.com/download)
- [WireGuard 프로토콜](https://www.wireguard.com/)

---

**작성일**: 2025-10-23
**버전**: 1.0
**관련 문서**: `Flask_Server_Setup.md`, `RaspberryPi_Setup.md`
