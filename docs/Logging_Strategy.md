# PCB 불량 검사 시스템 통합 로깅 전략

## 개요

이 문서는 PCB 불량 검사 시스템의 통합 로깅 전략을 정의합니다. 각 서비스(Flask 서버, 라즈베리파이 클라이언트, C# WinForms 앱)에서 일관된 로그 형식과 로그 레벨을 사용하여 시스템 전체의 디버깅, 모니터링, 오류 추적을 용이하게 합니다.

### 네트워크 환경

프로젝트는 **원격 네트워크 환경 (Tailscale VPN)**을 기본으로 사용합니다:
- **GPU PC**: 원격지 (같은 도시 내), Tailscale IP (100.x.x.x)
- **라즈베리파이 1, 2**: Tailscale IP (100.x.x.x)
- **Windows PC**: Tailscale IP (100.x.x.x)

상세한 네트워크 설정 및 방화벽 구성은 `docs/Remote_Network_Setup.md`를 참조하세요.

---

## 로그 레벨 정의

모든 서비스에서 다음 5가지 로그 레벨을 사용합니다:

| 레벨 | 설명 | 사용 예시 |
|------|------|-----------|
| **DEBUG** | 디버깅 정보 (개발 환경 전용) | 함수 호출, 변수 값, 상세 추론 과정 |
| **INFO** | 일반 정보 (정상 동작) | 서버 시작, 프레임 수신, 검사 완료 |
| **WARNING** | 경고 (문제는 아니지만 주의 필요) | 프레임 스킵, 네트워크 지연, 낮은 신뢰도 |
| **ERROR** | 오류 (기능 일부 실패) | 프레임 디코딩 실패, 데이터베이스 연결 실패 |
| **CRITICAL** | 심각한 오류 (시스템 중단) | 모델 로드 실패, 서버 충돌 |

---

## 로그 형식 표준

### 표준 로그 포맷

```
[YYYY-MM-DD HH:MM:SS,mmm] [레벨] [서비스명] [모듈명] - 메시지 (추가 정보)
```

**예시**:
```
[2025-10-23 14:30:45,123] [INFO] [Flask-Server] [frontscan.py] - Frontscan completed (decision: position_error, missing_count: 1, position_error_count: 2, time: 85ms)
[2025-10-23 14:30:46,456] [WARNING] [RaspberryPi-1 (Left)] [camera_client.py] - Frame skip detected (network delay: 150ms)
[2025-10-23 14:30:47,789] [ERROR] [Flask-Server] [database.py] - Database connection failed (MySQL error: 2003)
```

### 추가 메타데이터 (선택)

- **camera_id**: 'left', 'right'
- **inspection_id**: 검사 ID (데이터베이스 PK)
- **client_ip**: 클라이언트 IP 주소 (Tailscale VPN: 100.x.x.x)
- **timestamp**: ISO 8601 형식 (YYYY-MM-DDTHH:MM:SS.sssZ)

---

## 서비스별 로깅 구현

### 1. Flask 추론 서버 (GPU PC)

**서비스명**: `Flask-Server`
**로그 파일 경로**: `/var/log/pcb_inspection/flask_server.log`

**Flask 서버 구축 및 설정 상세 가이드**: `docs/Flask_Server_Setup.md` 참조

#### Python 로깅 설정 (server/app.py)

```python
import logging
from logging.handlers import RotatingFileHandler
import os

# 로그 디렉토리 생성
LOG_DIR = '/var/log/pcb_inspection'
os.makedirs(LOG_DIR, exist_ok=True)

# 로거 설정
logger = logging.getLogger('Flask-Server')
logger.setLevel(logging.INFO)  # 프로덕션: INFO, 개발: DEBUG

# 파일 핸들러 (로테이션: 10MB, 최대 5개 백업)
file_handler = RotatingFileHandler(
    os.path.join(LOG_DIR, 'flask_server.log'),
    maxBytes=10 * 1024 * 1024,  # 10MB
    backupCount=5
)
file_handler.setLevel(logging.INFO)

# 콘솔 핸들러 (개발 환경)
console_handler = logging.StreamHandler()
console_handler.setLevel(logging.DEBUG)

# 로그 포맷 정의
formatter = logging.Formatter(
    '[%(asctime)s] [%(levelname)s] [Flask-Server] [%(module)s] - %(message)s',
    datefmt='%Y-%m-%d %H:%M:%S'
)
file_handler.setFormatter(formatter)
console_handler.setFormatter(formatter)

# 핸들러 추가
logger.addHandler(file_handler)
logger.addHandler(console_handler)

# 사용 예시
logger.info("Flask server started on 0.0.0.0:5000")
logger.debug(f"Loaded YOLO model: {model_path}")
logger.warning(f"High inference time detected: {inference_time_ms}ms")
logger.error(f"Database connection failed: {str(e)}")
```

#### 주요 로그 이벤트

| 이벤트 | 레벨 | 메시지 예시 |
|--------|------|-------------|
| 서버 시작 | INFO | "Flask server started on 0.0.0.0:5000" |
| 모델 로드 | INFO | "YOLO model loaded: models/yolo_best.pt" |
| 프레임 수신 | DEBUG | "Received frame from left (shape: 640x480)" |
| 추론 완료 | INFO | "Inference completed (defect: 납땜불량, conf: 0.95, time: 85ms)" |
| 데이터베이스 저장 | INFO | "Inspection saved to DB (id: 12345)" |
| GPIO 신호 전송 | INFO | "GPIO signal sent to RaspberryPi-1 (Left) (pin: 27, duration: 500ms)" |
| 프레임 디코딩 실패 | ERROR | "Failed to decode frame from right" |
| 데이터베이스 오류 | ERROR | "Database INSERT failed: {error_message}" |

---

### 2. 라즈베리파이 클라이언트

**서비스명**: `RaspberryPi-1 (Left)` (좌측 + GPIO), `RaspberryPi-2 (Right)` (우측 전용)
**로그 파일 경로**: `/home/pi/pcb_inspection_client/camera_client.log`

#### Python 로깅 설정 (camera_client.py)

```python
import logging
from logging.handlers import RotatingFileHandler

# 서비스명 (환경변수 또는 설정 파일에서 읽기)
SERVICE_NAME = 'RaspberryPi-1 (Left)'  # 또는 'RaspberryPi-2 (Right)'

# 로거 설정
logger = logging.getLogger(SERVICE_NAME)
logger.setLevel(logging.INFO)

# 파일 핸들러 (로테이션: 5MB, 최대 3개 백업)
file_handler = RotatingFileHandler(
    '/home/pi/pcb_inspection_client/camera_client.log',
    maxBytes=5 * 1024 * 1024,  # 5MB
    backupCount=3
)
file_handler.setLevel(logging.INFO)

# 콘솔 핸들러
console_handler = logging.StreamHandler()
console_handler.setLevel(logging.INFO)

# 로그 포맷
formatter = logging.Formatter(
    f'[%(asctime)s] [%(levelname)s] [{SERVICE_NAME}] [%(module)s] - %(message)s',
    datefmt='%Y-%m-%d %H:%M:%S'
)
file_handler.setFormatter(formatter)
console_handler.setFormatter(formatter)

# 핸들러 추가
logger.addHandler(file_handler)
logger.addHandler(console_handler)

# 사용 예시
logger.info("Camera client started (camera_id: left)")
logger.debug(f"Frame captured (size: {frame.shape})")
logger.warning(f"Network latency high: {latency_ms}ms")
logger.error(f"Failed to send frame to server: {str(e)}")
```

#### 주요 로그 이벤트

| 이벤트 | 레벨 | 메시지 예시 |
|--------|------|-------------|
| 클라이언트 시작 | INFO | "Camera client started (camera_id: left, server: 100.64.1.1:5000)" |
| 웹캠 초기화 | INFO | "Webcam initialized (index: 0, resolution: 640x480)" |
| 프레임 캡처 | DEBUG | "Frame captured (size: 640x480, fps: 10)" |
| 프레임 전송 | DEBUG | "Frame sent to server (size: 45KB, encoding: JPEG)" |
| 추론 결과 수신 | INFO | "Inference result received (defect: 정상, conf: 0.98)" |
| GPIO 제어 (RaspberryPi-1 (Left) 전용) | INFO | "GPIO triggered (pin: 23, duration: 300ms)" |
| 네트워크 지연 | WARNING | "High network latency detected: 120ms" |
| 전송 실패 | ERROR | "Failed to send frame: Connection timeout" |

---

### 3. C# WinForms 모니터링 앱

**서비스명**: `WinForms-App`
**로그 파일 경로**: `C:\PCB_Inspection\Logs\winforms_app.log`

#### C# 로깅 설정 (NLog 사용)

**NLog.config**:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">

  <targets>
    <!-- 파일 로그 -->
    <target name="logfile" xsi:type="File"
            fileName="C:\PCB_Inspection\Logs\winforms_app.log"
            layout="[${longdate}] [${level:uppercase=true}] [WinForms-App] [${logger:shortName=true}] - ${message} ${exception:format=tostring}"
            archiveFileName="C:\PCB_Inspection\Logs\archives\winforms_app.{#}.log"
            archiveEvery="Day"
            archiveNumbering="Date"
            maxArchiveFiles="7" />

    <!-- 콘솔 로그 (디버그용) -->
    <target name="console" xsi:type="Console"
            layout="[${longdate}] [${level:uppercase=true}] - ${message}" />
  </targets>

  <rules>
    <logger name="*" minlevel="Info" writeTo="logfile" />
    <logger name="*" minlevel="Debug" writeTo="console" />
  </rules>
</nlog>
```

#### C# 코드 사용 예시

```csharp
using NLog;

public class MainForm : Form
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public MainForm()
    {
        InitializeComponent();
        Logger.Info("WinForms app started");
    }

    private void LoadInspectionHistory()
    {
        try
        {
            Logger.Debug("Loading inspection history from database");
            var inspections = _dbService.GetInspections(page: 1, limit: 50);
            Logger.Info($"Loaded {inspections.Count} inspection records");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to load inspection history");
            MessageBox.Show("검사 이력 조회 실패", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
```

#### 주요 로그 이벤트

| 이벤트 | 레벨 | 메시지 예시 |
|--------|------|-------------|
| 앱 시작 | INFO | "WinForms app started (version: 1.0)" |
| 로그인 성공 | INFO | "User logged in successfully (username: operator1)" |
| 데이터베이스 조회 | DEBUG | "Loading inspection history (page: 1, limit: 50)" |
| 통계 조회 | INFO | "Statistics loaded (date_range: 2025-10-01 to 2025-10-23)" |
| Excel 내보내기 | INFO | "Inspection data exported to Excel (file: inspections_20251023.xlsx)" |
| API 통신 오류 | ERROR | "Failed to fetch data from Flask server: Connection refused" |
| 데이터베이스 연결 실패 | ERROR | "Database connection failed: MySQL error 2003" |

---

## MySQL 데이터베이스 로그 활용

시스템 오류 및 중요 이벤트는 `system_logs` 테이블에도 저장됩니다.

**중요 이벤트 기록**:
- 심각한 오류 (CRITICAL, ERROR)
- 시스템 상태 변경 (서버 시작/중단, 모델 재로드)
- 보안 이벤트 (로그인 실패, 권한 위반)

**Python 예시 (Flask 서버)**:

```python
def log_to_database(level, message, details=None):
    """시스템 로그를 데이터베이스에 저장"""
    try:
        conn = get_db_connection()
        cursor = conn.cursor()

        query = """
        INSERT INTO system_logs (log_level, log_message, details, timestamp)
        VALUES (%s, %s, %s, NOW())
        """
        cursor.execute(query, (level, message, details))
        conn.commit()
    except Exception as e:
        logger.error(f"Failed to log to database: {str(e)}")
    finally:
        conn.close()

# 사용 예시
try:
    model = YOLO(model_path)
    logger.info("YOLO model loaded successfully")
except Exception as e:
    logger.critical(f"Failed to load YOLO model: {str(e)}")
    log_to_database('CRITICAL', 'YOLO model load failed', str(e))
```

---

## 오류 알림 시스템

심각한 오류 발생 시 `alerts` 테이블에 기록하여 관리자에게 알림을 제공합니다.

**알림 조건**:
- CRITICAL 레벨 로그
- 연속 10회 이상 ERROR 발생
- 추론 지연 시간 > 500ms (10회 연속)
- 데이터베이스 연결 실패

**Python 예시**:

```python
def create_alert(alert_type, message, priority='medium'):
    """알림 생성 및 데이터베이스 저장"""
    try:
        conn = get_db_connection()
        cursor = conn.cursor()

        query = """
        INSERT INTO alerts (alert_type, alert_message, priority, status, created_at)
        VALUES (%s, %s, %s, 'unread', NOW())
        """
        cursor.execute(query, (alert_type, message, priority))
        conn.commit()

        logger.warning(f"Alert created: {alert_type} - {message}")
    except Exception as e:
        logger.error(f"Failed to create alert: {str(e)}")
    finally:
        conn.close()

# 사용 예시
if inference_time_ms > 500:
    slow_inference_count += 1
    if slow_inference_count >= 10:
        create_alert(
            alert_type='performance',
            message=f'Slow inference detected: avg {inference_time_ms}ms',
            priority='high'
        )
        slow_inference_count = 0
```

---

## 로그 파일 관리

### 로그 로테이션 정책

| 서비스 | 최대 파일 크기 | 백업 개수 | 보관 기간 |
|--------|----------------|-----------|-----------|
| Flask 서버 | 10 MB | 5개 | 7일 |
| 라즈베리파이 | 5 MB | 3개 | 5일 |
| C# WinForms | 10 MB | 7개 | 7일 |

### 로그 파일 정리 (cron)

**Flask 서버 (GPU PC)**:

```bash
# 로그 정리 스크립트 생성
sudo nano /home/pcb_user/cleanup_logs.sh
```

내용:
```bash
#!/bin/bash
LOG_DIR="/var/log/pcb_inspection"

# 7일 이상 된 로그 파일 삭제
find $LOG_DIR -name "flask_server.log.*" -mtime +7 -delete

echo "Log cleanup completed at $(date)"
```

실행 권한 부여:
```bash
chmod +x /home/pcb_user/cleanup_logs.sh
```

cron 등록 (매일 새벽 3시):
```bash
crontab -e

# 추가
0 3 * * * /home/pcb_user/cleanup_logs.sh >> /home/pcb_user/cleanup.log 2>&1
```

---

## 로그 분석 및 모니터링

### 실시간 로그 확인

**Flask 서버**:
```bash
tail -f /var/log/pcb_inspection/flask_server.log
```

**라즈베리파이**:
```bash
tail -f /home/pi/pcb_inspection_client/camera_client.log
```

**systemd 서비스 로그**:
```bash
sudo journalctl -u camera-client-left.service -f
```

### 오류 로그 필터링

**ERROR 이상 로그만 보기**:
```bash
grep -E "\[ERROR\]|\[CRITICAL\]" /var/log/pcb_inspection/flask_server.log
```

**특정 날짜 로그 검색**:
```bash
grep "2025-10-23" /var/log/pcb_inspection/flask_server.log
```

**추론 시간 분석**:
```bash
grep "Inference completed" /var/log/pcb_inspection/flask_server.log | awk -F'time: ' '{print $2}' | awk '{print $1}' | sed 's/ms//' | awk '{sum+=$1; n++} END {print "Average:", sum/n, "ms"}'
```

---

## 개발 vs 프로덕션 환경

### 개발 환경
- **로그 레벨**: DEBUG
- **콘솔 출력**: 활성화
- **파일 로그**: 활성화
- **데이터베이스 로그**: 비활성화 (선택)

### 프로덕션 환경
- **로그 레벨**: INFO
- **콘솔 출력**: 비활성화
- **파일 로그**: 활성화 (로테이션 적용)
- **데이터베이스 로그**: 활성화 (ERROR 이상만)

**환경 변수로 제어**:

```python
import os

# 환경 변수에서 로그 레벨 읽기
LOG_LEVEL = os.getenv('LOG_LEVEL', 'INFO')  # 기본: INFO
logger.setLevel(getattr(logging, LOG_LEVEL))

# 프로덕션 환경에서는 콘솔 핸들러 비활성화
if os.getenv('ENVIRONMENT') == 'production':
    logger.removeHandler(console_handler)
```

---

## 로그 보안

### 민감 정보 마스킹

로그에 민감 정보가 포함되지 않도록 마스킹 처리:

```python
import re

def mask_sensitive_data(message):
    """민감 정보 마스킹"""
    # IP 주소 마스킹 (마지막 옥텟만 표시)
    message = re.sub(r'(\d{1,3}\.){3}\d{1,3}', lambda m: '.'.join(m.group(0).split('.')[:-1]) + '.***', message)

    # 비밀번호 마스킹
    message = re.sub(r'password[=:]?\s*\S+', 'password=***', message, flags=re.IGNORECASE)

    return message

# 사용 예시
logger.info(mask_sensitive_data(f"User connected from {client_ip}"))
```

---

## 관련 문서

이 로깅 전략은 다음 문서들과 연계되어 있습니다:

1. **PCB_Defect_Detection_Project.md** - 전체 시스템 아키텍처
2. **Flask_Server_Setup.md** - Flask 서버 로깅 구현
3. **RaspberryPi_Setup.md** - 라즈베리파이 클라이언트 로깅
4. **MySQL_Database_Design.md** - 데이터베이스 로그 및 알림 테이블
5. **CSharp_WinForms_Guide.md** - C# WinForms 로깅 (NLog)

---

**작성일**: 2025-10-23
**버전**: 1.0
**목적**: 시스템 전체의 통합 로깅 전략 수립 및 디버깅 지원
