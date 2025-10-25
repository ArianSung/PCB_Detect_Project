# 데이터베이스 설정 가이드

PCB 불량 검사 시스템 MySQL 데이터베이스 설정 및 사용 가이드

---

## 📋 개요

- **데이터베이스**: MySQL 8.0
- **위치**: Windows PC (원격)
- **연결 방식**: Tailscale VPN
- **문자셋**: utf8mb4 (한글 지원)

---

## 🚀 초기 설정 (최초 1회만)

### 1. 사용자 생성

**MySQL Workbench**를 열고 **root 계정**으로 접속한 후, `create_users.sql` 실행:

```sql
-- File → Open SQL Script → create_users.sql 선택
-- 실행: Ctrl+Shift+Enter
```

**생성되는 사용자:**
| Username | Password | 권한 | 용도 |
|----------|----------|------|------|
| `pcb_admin` | `1234` | ALL | 관리자 (테이블 생성/삭제) |
| `pcb_server` | `1234` | SELECT, INSERT, UPDATE | Flask 서버 |
| `pcb_viewer` | `1234` | SELECT only | C# 모니터링 앱 |
| `pcb_data` | `1234` | SELECT, INSERT, UPDATE | AI 모델 팀 |
| `pcb_test` | `1234` | SELECT, INSERT | 테스트용 |

---

### 2. 데이터베이스 스키마 생성

**MySQL Workbench**에서 **pcb_admin 계정**으로 접속한 후, `schema.sql` 실행:

```sql
-- File → Open SQL Script → schema.sql 선택
-- 실행: Ctrl+Shift+Enter
```

**생성되는 테이블:**
1. `inspection_history`: 검사 이력 (메인 테이블)
2. `daily_statistics`: 일별 통계
3. `defect_type_statistics`: 불량 유형별 통계
4. `system_logs`: 시스템 로그 (선택)

---

## 📊 테이블 구조

### 1. inspection_history (검사 이력)

PCB 검사 결과를 실시간으로 저장하는 메인 테이블

**주요 컬럼:**
```sql
id                   BIGINT       - 고유 ID (자동 증가)
camera_id            VARCHAR(10)  - 카메라 ID (left/right)
timestamp            DATETIME     - 검사 시각
classification       VARCHAR(20)  - 분류 결과 (normal/component_defect/solder_defect/discard)
confidence           DECIMAL(5,4) - 신뢰도 (0.0000 ~ 1.0000)
total_defects        INT          - 검출된 불량 개수
defects_json         JSON         - 불량 상세 정보
anomaly_score        DECIMAL(5,4) - 이상 탐지 점수
inference_time_ms    DECIMAL(7,2) - 추론 시간 (ms)
gpio_pin             INT          - GPIO 핀 번호
image_path           VARCHAR(255) - 이미지 경로 (선택)
```

**샘플 조회:**
```sql
SELECT
    id,
    camera_id,
    timestamp,
    classification,
    confidence,
    total_defects,
    inference_time_ms
FROM inspection_history
ORDER BY timestamp DESC
LIMIT 10;
```

---

### 2. daily_statistics (일별 통계)

C# 모니터링 앱 대시보드용 일별 집계 데이터

**주요 컬럼:**
```sql
date                      DATE         - 통계 날짜
total_inspections         INT          - 전체 검사 수
normal_count              INT          - 정상 개수
component_defect_count    INT          - 부품 불량 개수
solder_defect_count       INT          - 납땜 불량 개수
discard_count             INT          - 폐기 개수
defect_rate               DECIMAL(5,4) - 불량률
avg_inference_time_ms     DECIMAL(7,2) - 평균 추론 시간
```

**샘플 조회:**
```sql
SELECT
    date,
    total_inspections,
    normal_count,
    solder_defect_count,
    defect_rate
FROM daily_statistics
ORDER BY date DESC
LIMIT 7;  -- 최근 7일
```

---

### 3. defect_type_statistics (불량 유형별 통계)

불량 유형별 발생 빈도 및 심각도 분석

**주요 컬럼:**
```sql
date                   DATE         - 통계 날짜
defect_type            VARCHAR(50)  - 불량 유형 (cold_joint, solder_bridge, etc.)
count                  INT          - 발생 횟수
low_severity_count     INT          - 낮은 심각도
medium_severity_count  INT          - 중간 심각도
high_severity_count    INT          - 높은 심각도
```

**불량 유형 목록:**
- `cold_joint`: Cold Joint (차가운 납땜)
- `solder_bridge`: Solder Bridge (땜납 다리)
- `insufficient_solder`: 불충분한 납땜
- `excess_solder`: 과도한 납땜
- `missing_component`: 부품 누락
- `misalignment`: 부품 위치 불량
- `wrong_component`: 잘못된 부품
- `damaged_component`: 손상된 부품
- `trace_damage`: 회로 선로 손상
- `pad_damage`: 패드 손상
- `scratch`: 스크래치

---

## 🔌 연결 방법

### Flask 서버 (Python)

**`src/server/.env` 파일:**
```bash
DB_HOST=100.x.x.x          # Windows PC의 Tailscale IP
DB_PORT=3306
DB_NAME=pcb_inspection
DB_USER=pcb_server
DB_PASSWORD=1234
```

**연결 코드 예시:**
```python
import pymysql

conn = pymysql.connect(
    host='100.x.x.x',
    port=3306,
    user='pcb_server',
    password='1234',
    database='pcb_inspection',
    charset='utf8mb4'
)

cursor = conn.cursor()
cursor.execute("SELECT * FROM inspection_history LIMIT 1")
print(cursor.fetchone())
```

---

### C# WinForms (모니터링 앱)

**연결 문자열:**
```csharp
string connectionString = "Server=100.x.x.x;Port=3306;Database=pcb_inspection;Uid=pcb_viewer;Pwd=1234;";

using (MySqlConnection conn = new MySqlConnection(connectionString))
{
    conn.Open();
    MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM inspection_history", conn);
    int count = Convert.ToInt32(cmd.ExecuteScalar());
    Console.WriteLine($"Total records: {count}");
}
```

---

### MySQL Workbench (팀원)

**연결 정보:**
- **Connection Name**: PCB Inspection (본인 이름)
- **Hostname**: `100.x.x.x` (Windows PC의 Tailscale IP)
- **Port**: `3306`
- **Username**: 팀별 계정 (위 표 참조)
- **Password**: 팀별 비밀번호

---

## 📝 자주 사용하는 SQL 쿼리

### 검사 이력 조회

```sql
-- 최근 100개 검사 이력
SELECT * FROM inspection_history
ORDER BY timestamp DESC
LIMIT 100;

-- 불량만 조회
SELECT * FROM inspection_history
WHERE classification != 'normal'
ORDER BY timestamp DESC;

-- 특정 날짜 조회
SELECT * FROM inspection_history
WHERE DATE(timestamp) = '2025-10-25'
ORDER BY timestamp DESC;
```

### 통계 조회

```sql
-- 오늘 통계
SELECT * FROM daily_statistics
WHERE date = CURDATE();

-- 불량률 높은 날짜 TOP 10
SELECT date, total_inspections, defect_rate
FROM daily_statistics
ORDER BY defect_rate DESC
LIMIT 10;

-- 불량 유형별 발생 빈도 (최근 7일)
SELECT
    defect_type,
    SUM(count) AS total_count
FROM defect_type_statistics
WHERE date >= DATE_SUB(CURDATE(), INTERVAL 7 DAY)
GROUP BY defect_type
ORDER BY total_count DESC;
```

---

## 🔧 유지보수

### 데이터 백업

```bash
# MySQL Workbench: Server → Data Export
# 또는 명령줄:
mysqldump -h 100.x.x.x -u pcb_admin -p pcb_inspection > backup_$(date +%Y%m%d).sql
```

### 데이터 복원

```bash
mysql -h 100.x.x.x -u pcb_admin -p pcb_inspection < backup_20251025.sql
```

### 오래된 데이터 삭제 (선택)

```sql
-- 30일 이전 데이터 삭제
DELETE FROM inspection_history
WHERE timestamp < DATE_SUB(NOW(), INTERVAL 30 DAY);
```

---

## ⚠️ 주의사항

1. **비밀번호 변경 권장**: 프로덕션 환경에서는 반드시 강력한 비밀번호로 변경
2. **백업 주기**: 주 1회 이상 백업 권장
3. **용량 관리**: `inspection_history` 테이블은 빠르게 증가하므로 주기적으로 정리
4. **인덱스 최적화**: 대용량 데이터 조회 시 인덱스 추가 고려

---

**마지막 업데이트**: 2025-10-25
**담당**: Flask 팀 + 전체 팀
