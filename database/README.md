# 데이터베이스 설정 가이드 (Product Verification Architecture)

PCB 제품별 부품 위치 검증 시스템이 사용하는 **MySQL 8.0** 스키마 설명입니다. 모든 데이터는 Tailscale VPN 으로 연결된 Windows PC(MySQL 서버)에 저장합니다. 문자셋은 `utf8mb4` 입니다.

---

## 🚀 초기 설정 절차

1. **사용자 생성**  
   MySQL Workbench 에서 root 계정으로 접속한 뒤 `create_users.sql` 을 실행합니다.

   | Username | Password | 권한 | 용도 |
   |----------|----------|------|------|
   | `pcb_admin`  | `1234` | ALL | 테이블/트리거 생성 및 유지보수 |
   | `pcb_server` | `1234` | SELECT, INSERT, UPDATE | Flask 추론 서버 |
   | `pcb_viewer` | `1234` | SELECT | C# WinForms 모니터링 앱 |
   | `pcb_data`   | `1234` | SELECT, INSERT, UPDATE | 데이터 수집/검증 스크립트 |
   | `pcb_test`   | `1234` | SELECT, INSERT | 테스트용 스크립트 |

2. **스키마 생성**  
   `pcb_admin` 계정으로 로그인하여 `schema.sql` (v3.0) 실행 → 제품 식별 + 부품 검증용 테이블이 모두 생성됩니다.

3. **집계 트리거/이벤트**  
   필요 시 `triggers_v3.0.sql` 과 `events_v3.0.sql` 을 실행하여 일/월별 통계를 자동 갱신합니다.

---

## 📦 생성되는 테이블

1. `products` – 제품 기본 정보 및 시리얼/QR 템플릿
2. `product_components` – 제품별 기준 부품 좌표
3. `inspections` – 뒷면 식별 + 앞면 검증 결과 (메인 로그)
4. `inspection_summary_hourly` – 시간 단위 집계
5. `inspection_summary_daily` – 일 단위 집계
6. `inspection_summary_monthly` – 월 단위 집계

아래에서 주요 컬럼과 예시 조회 쿼리를 정리했습니다.

---

### 1. `products`
제품 코드(FT/RS/BC 등)별 기본 정보를 보관합니다.

```sql
product_code        VARCHAR(10) PK  -- FT, RS, BC
product_name        VARCHAR(100)    -- 제품명
serial_prefix       VARCHAR(4)      -- 시리얼 접두사 (예: MBFT)
component_count     INT             -- 기준 부품 개수
qr_url_template     VARCHAR(255)    -- QR 코드 템플릿 (예: http://.../{serial})
description         TEXT            -- 설명
```

샘플 조회:
```sql
SELECT product_code, product_name, component_count
FROM products ORDER BY product_code;
```

---

### 2. `product_components`
제품별 정상 부품 배치 (YOLO 기준) 좌표를 저장합니다.

```sql
product_code        VARCHAR(10) FK → products
component_class     VARCHAR(50)    -- resistor, ic_socket 등
center_x, center_y  FLOAT          -- 기준 중심 좌표 (px)
bbox_x1 ~ bbox_y2   FLOAT          -- 기준 바운딩 박스 (px)
tolerance_px        FLOAT          -- 허용 오차 (기본 20px)
```

특정 제품 좌표 확인:
```sql
SELECT component_class, center_x, center_y
FROM product_components
WHERE product_code = 'FT';
```

---

### 3. `inspections`
뒷면 시리얼·QR 식별 결과와 앞면 부품 검증 요약을 모두 저장합니다. `decision` 필드는 `normal / missing / position_error / discard` 중 하나입니다.

```sql
id                      BIGINT PK AUTO_INCREMENT
serial_number           VARCHAR(20)   -- MBFT12345678
product_code            VARCHAR(10)
qr_data                 TEXT          -- QR payload (선택)
qr_detected             BOOLEAN
serial_detected         BOOLEAN

decision                VARCHAR(20)
missing_count           INT
position_error_count    INT
extra_count             INT
correct_count           INT
missing_components      JSON          -- 누락 상세
position_errors         JSON          -- 위치 오차 상세
extra_components        JSON          -- 기준 외 부품

yolo_detections         JSON          -- 원본 YOLO 출력
inference_time_ms       FLOAT
verification_time_ms    FLOAT
total_time_ms           FLOAT
left_image_path         VARCHAR(255)
right_image_path        VARCHAR(255)
inspection_time         TIMESTAMP DEFAULT CURRENT_TIMESTAMP
```

마지막 20건 확인:
```sql
SELECT serial_number, product_code, decision,
       missing_count, position_error_count,
       inspection_time
FROM inspections
ORDER BY inspection_time DESC
LIMIT 20;
```

누락/위치 오류 합산이 7개 이상이면 `decision = 'discard'` 가 됩니다 (Flask 서버 로직과 동일하게 정의됨).

---

### 4~6. 집계 테이블
세 집계 테이블은 구조가 동일하며 기준 열만 다릅니다.

| 테이블 | 기준 열 | 설명 |
|--------|---------|------|
| `inspection_summary_hourly`  | `hour_timestamp` (YYYY-MM-DD HH:00:00) | 제품별 시간당 실적 |
| `inspection_summary_daily`   | `date` (YYYY-MM-DD)                    | 일별 집계 |
| `inspection_summary_monthly` | `year`, `month`                        | 월별 집계 |

공통 컬럼:
```sql
total_inspections INT
normal_count INT
missing_count INT
position_error_count INT
discard_count INT
avg_inference_time_ms FLOAT
avg_total_time_ms FLOAT
avg_detection_count FLOAT
avg_confidence FLOAT
defect_rate FLOAT (생성 열)
```

예시 – 최근 7일 제품별 통계:
```sql
SELECT date, product_code,
       total_inspections,
       missing_count,
       position_error_count,
       defect_rate
FROM inspection_summary_daily
ORDER BY date DESC, product_code
LIMIT 21;  -- 3개 제품 × 7일
```

---

## 🔌 애플리케이션 연결

### Flask 서버 (`server/.env`)
```env
DB_HOST=100.x.x.x
DB_PORT=3306
DB_NAME=pcb_inspection
DB_USER=pcb_server
DB_PASSWORD=1234
```

### Python 예시
```python
import pymysql

conn = pymysql.connect(
    host="100.x.x.x",
    port=3306,
    user="pcb_server",
    password="1234",
    database="pcb_inspection",
    charset="utf8mb4",
    cursorclass=pymysql.cursors.DictCursor,
)

with conn.cursor() as cursor:
    cursor.execute(
        "SELECT decision, missing_count FROM inspections ORDER BY inspection_time DESC LIMIT 5"
    )
    print(cursor.fetchall())
```

### C# WinForms 연결 문자열
```csharp
string cs = "Server=100.x.x.x;Port=3306;Database=pcb_inspection;Uid=pcb_viewer;Pwd=1234;";
```
집계 API 없이도 `inspection_summary_daily` 를 직접 조회해 차트를 그릴 수 있습니다.

---

## ✅ 운영 체크리스트
- `products`, `product_components` 는 제품 Golden Sample 변경 시 반드시 업데이트
- `inspections` 는 10년 보관 기준, 주기적 백업 권장 (mysqldump)
- 요약 테이블은 `events_v3.0.sql` 의 Event Scheduler 로 자동 관리 가능
- 테이블 구조 변경 시 Flask 서버 `db_manager.py` 의 컬럼 매핑도 함께 수정
