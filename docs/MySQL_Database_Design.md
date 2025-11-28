# MySQL 데이터베이스 설계 - PCB 검사 시스템 v3.0 ⭐ (제품별 부품 위치 검증)

## 개요

PCB 불량 검사 시스템의 **제품별 부품 위치 검증**, 검사 이력, 통계를 저장하는 MySQL 데이터베이스 스키마 설계입니다.

**⭐ v3.0 Product Verification Architecture 특징**:
- **제품 식별**: 시리얼 넘버 OCR + QR 코드 스캔
- **제품별 부품 배치 검증**: 제품 코드별 기준 위치와 비교
- **4단계 판정**: normal (정상) / missing (부품 누락) / position_error (위치 오류) / discard (폐기)
- **시간별/일별/월별 집계**: 트리거를 통한 자동 업데이트
- **10년 데이터 보관**: 이벤트 스케줄러를 통한 자동 정리

**주요 변경사항 (v2.0 → v3.0)**:
- 이중 YOLO 모델 아키텍처 → 제품별 부품 위치 검증
- 융합 판정 (4가지) → 검증 판정 (4가지)
- component_defects, solder_defects → missing_components, position_errors, extra_components
- 시간별 집계 추가, 월별 집계 추가
- 트리거 기반 자동 집계 시스템
- 10년 자동 데이터 정리 시스템

---

## 데이터베이스 생성

```sql
CREATE DATABASE IF NOT EXISTS pcb_inspection
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE pcb_inspection;
```

**문자 인코딩**: UTF-8 (utf8mb4) - 한글 및 이모지 지원

---

## 테이블 스키마

### 1. products (제품 정보)

제품 코드별 기본 정보를 저장하는 마스터 테이블입니다.

```sql
CREATE TABLE products (
    product_code VARCHAR(10) PRIMARY KEY COMMENT '제품 코드 (예: FT, RS, BC)',
    product_name VARCHAR(100) NOT NULL COMMENT '제품명',
    description TEXT NULL COMMENT '제품 설명',
    serial_prefix VARCHAR(4) NOT NULL COMMENT '시리얼 넘버 접두사 (예: MBFT, MBRS)',
    component_count INT NOT NULL DEFAULT 0 COMMENT '기준 부품 개수',
    qr_url_template VARCHAR(255) NULL COMMENT 'QR 코드 URL 템플릿',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP COMMENT '생성일',
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '수정일',

    INDEX idx_serial_prefix (serial_prefix),
    INDEX idx_created_at (created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='제품 정보 테이블 (3개 제품 타입)';
```

**샘플 데이터**:
```sql
INSERT INTO products (product_code, product_name, description, serial_prefix, component_count, qr_url_template) VALUES
('FT', 'Fast Type PCB', '고속 처리용 PCB (25개 부품)', 'MBFT', 25, 'http://localhost:8080/product/{serial}'),
('RS', 'Reliable Stable PCB', '안정성 중심 PCB (30개 부품)', 'MBRS', 30, 'http://localhost:8080/product/{serial}'),
('BC', 'Budget Compact PCB', '저가형 소형 PCB (18개 부품)', 'MBBC', 18, 'http://localhost:8080/product/{serial}');
```

**설명**:
- `product_code`: 제품 코드 (FT, RS, BC 등), 시리얼 넘버 3-4번째 문자에서 추출
- `serial_prefix`: 시리얼 넘버 접두사 (MBFT12345678 → MBFT)
- `component_count`: 정상 부품 개수 (검증 기준)
- `qr_url_template`: QR 코드 URL 템플릿 (C# 로컬 웹 서버)

---

### 2. product_components (제품별 부품 배치 기준)

제품별 정상 부품 배치 위치 정보를 저장하는 기준 데이터 테이블입니다.

```sql
CREATE TABLE product_components (
    id INT AUTO_INCREMENT PRIMARY KEY COMMENT '고유 ID',
    product_code VARCHAR(10) NOT NULL COMMENT '제품 코드 (FK)',
    component_class VARCHAR(50) NOT NULL COMMENT '부품 클래스명 (예: capacitor, resistor)',
    center_x FLOAT NOT NULL COMMENT '부품 중심 X 좌표 (픽셀)',
    center_y FLOAT NOT NULL COMMENT '부품 중심 Y 좌표 (픽셀)',
    bbox_x1 FLOAT NOT NULL COMMENT '바운딩 박스 좌상단 X',
    bbox_y1 FLOAT NOT NULL COMMENT '바운딩 박스 좌상단 Y',
    bbox_x2 FLOAT NOT NULL COMMENT '바운딩 박스 우하단 X',
    bbox_y2 FLOAT NOT NULL COMMENT '바운딩 박스 우하단 Y',
    tolerance_px FLOAT NOT NULL DEFAULT 20.0 COMMENT '위치 허용 오차 (픽셀, 기본 20px)',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP COMMENT '생성일',

    FOREIGN KEY (product_code) REFERENCES products(product_code)
        ON DELETE CASCADE
        ON UPDATE CASCADE,

    INDEX idx_product_code (product_code),
    INDEX idx_component_class (component_class),
    INDEX idx_center_coords (center_x, center_y)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='제품별 부품 배치 기준 테이블 (기준 위치 정보)';
```

**설명**:
- `product_code`: 어떤 제품의 기준 위치인지
- `component_class`: 부품 클래스명 (YOLO 검출 클래스와 매칭)
- `center_x`, `center_y`: 정상 위치의 중심 좌표
- `bbox_x1`, `bbox_y1`, `bbox_x2`, `bbox_y2`: 바운딩 박스 좌표
- `tolerance_px`: 위치 허용 오차 (기본 20px, 이 범위 내면 정상)

**사용 예시**:
```sql
-- 제품 FT의 부품 배치 기준 조회
SELECT component_class, center_x, center_y, tolerance_px
FROM product_components
WHERE product_code = 'FT'
ORDER BY component_class;

-- 특정 부품 클래스의 기준 위치 조회
SELECT product_code, center_x, center_y
FROM product_components
WHERE component_class = 'capacitor';
```

---

### 3. inspections (메인 검사 이력)

모든 PCB 검사 데이터를 저장하는 메인 테이블입니다. **10년 보관 정책** 적용.

```sql
CREATE TABLE inspections (
    id BIGINT AUTO_INCREMENT PRIMARY KEY COMMENT '검사 ID',

    -- 제품 식별 정보 (뒷면)
    serial_number VARCHAR(20) NOT NULL COMMENT '시리얼 넘버 (MBXX12345678)',
    product_code VARCHAR(10) NOT NULL COMMENT '제품 코드 (시리얼에서 추출)',
    qr_data TEXT NULL COMMENT 'QR 코드 데이터 (URL 또는 JSON)',
    qr_detected BOOLEAN DEFAULT FALSE COMMENT 'QR 코드 검출 성공 여부',
    serial_detected BOOLEAN DEFAULT FALSE COMMENT '시리얼 넘버 검출 성공 여부',

    -- 검사 결과 (앞면)
    decision VARCHAR(20) NOT NULL COMMENT '최종 판정 (normal/missing/position_error/discard)',
    missing_count INT NOT NULL DEFAULT 0 COMMENT '누락 부품 개수',
    position_error_count INT NOT NULL DEFAULT 0 COMMENT '위치 오류 부품 개수',
    extra_count INT NOT NULL DEFAULT 0 COMMENT '추가 부품 개수 (기준에 없음)',
    correct_count INT NOT NULL DEFAULT 0 COMMENT '정상 부품 개수',

    -- 상세 결과 (JSON)
    missing_components JSON NULL COMMENT '누락 부품 상세 (class_name, expected_position)',
    position_errors JSON NULL COMMENT '위치 오류 상세 (class_name, expected, actual, offset)',
    extra_components JSON NULL COMMENT '추가 부품 상세 (class_name, position)',

    -- YOLO 검출 결과
    yolo_detections JSON NULL COMMENT 'YOLO 전체 검출 결과 (bbox, confidence, class)',
    detection_count INT NOT NULL DEFAULT 0 COMMENT '총 검출 부품 개수',
    avg_confidence FLOAT NULL COMMENT '평균 신뢰도',

    -- 처리 성능
    inference_time_ms FLOAT NULL COMMENT 'AI 추론 시간 (밀리초)',
    verification_time_ms FLOAT NULL COMMENT '검증 처리 시간 (밀리초)',
    total_time_ms FLOAT NULL COMMENT '총 처리 시간 (밀리초)',

    -- 이미지 정보
    left_image_path VARCHAR(255) NULL COMMENT '좌측 카메라 이미지 경로',
    right_image_path VARCHAR(255) NULL COMMENT '우측 카메라 이미지 경로',
    image_width INT NULL COMMENT '이미지 너비',
    image_height INT NULL COMMENT '이미지 높이',

    -- 시스템 정보
    camera_id VARCHAR(20) NULL COMMENT '카메라 ID (left/right)',
    client_ip VARCHAR(50) NULL COMMENT '클라이언트 IP (라즈베리파이)',
    server_version VARCHAR(20) NULL COMMENT '서버 버전',

    -- 시간 정보
    inspection_time TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '검사 시간',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP COMMENT '레코드 생성 시간',

    -- 인덱스
    INDEX idx_serial_number (serial_number),
    INDEX idx_product_code (product_code),
    INDEX idx_decision (decision),
    INDEX idx_inspection_time (inspection_time),
    INDEX idx_created_at (created_at),
    INDEX idx_product_time (product_code, inspection_time),
    INDEX idx_decision_time (decision, inspection_time),

    FOREIGN KEY (product_code) REFERENCES products(product_code)
        ON DELETE RESTRICT
        ON UPDATE CASCADE

) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='메인 검사 이력 테이블 (10년 보관)';
```

**JSON 데이터 구조 예시**:

**missing_components** (누락 부품):
```json
[
  {
    "class_name": "capacitor",
    "expected_position": {
      "center": [120, 85],
      "bbox": [100, 70, 140, 100]
    }
  },
  {
    "class_name": "resistor",
    "expected_position": {
      "center": [200, 150],
      "bbox": [180, 135, 220, 165]
    }
  }
]
```

**position_errors** (위치 오류):
```json
[
  {
    "class_name": "capacitor",
    "expected": {"center": [120, 85]},
    "actual": {"center": [145, 90]},
    "offset": 25.5
  }
]
```

**extra_components** (추가 부품 - 기준에 없음):
```json
[
  {
    "class_name": "resistor",
    "position": {"center": [310, 220]},
    "confidence": 0.92
  }
]
```

**설명**:
- `decision`: 최종 판정
  - `normal`: 정상 (모든 부품 정상 위치)
  - `missing`: 부품 누락 (3개 이상 누락)
  - `position_error`: 위치 오류 (5개 이상 위치 오류)
  - `discard`: 폐기 (누락 + 위치 오류 합계 7개 이상)
- `missing_count`, `position_error_count`, `extra_count`, `correct_count`: 각 카테고리 개수
- JSON 필드: 상세 정보 저장 (WinForms에서 상세 조회 시 사용)

---

### 4. inspection_summary_hourly (시간별 집계)

시간 단위 검사 집계 테이블입니다. **10년 보관 정책** 적용.

```sql
CREATE TABLE inspection_summary_hourly (
    id BIGINT AUTO_INCREMENT PRIMARY KEY COMMENT '집계 ID',

    -- 집계 기준
    hour_timestamp DATETIME NOT NULL COMMENT '시간 (YYYY-MM-DD HH:00:00)',
    product_code VARCHAR(10) NOT NULL COMMENT '제품 코드',

    -- 집계 데이터
    total_inspections INT NOT NULL DEFAULT 0 COMMENT '총 검사 수',
    normal_count INT NOT NULL DEFAULT 0 COMMENT '정상 수',
    missing_count INT NOT NULL DEFAULT 0 COMMENT '부품 누락 수',
    position_error_count INT NOT NULL DEFAULT 0 COMMENT '위치 오류 수',
    discard_count INT NOT NULL DEFAULT 0 COMMENT '폐기 수',

    -- 통계
    avg_inference_time_ms FLOAT NULL COMMENT '평균 추론 시간',
    avg_total_time_ms FLOAT NULL COMMENT '평균 총 처리 시간',
    avg_detection_count FLOAT NULL COMMENT '평균 검출 부품 수',
    avg_confidence FLOAT NULL COMMENT '평균 신뢰도',

    -- 불량률 (자동 계산)
    defect_rate FLOAT GENERATED ALWAYS AS (
        CASE
            WHEN total_inspections > 0
            THEN ((missing_count + position_error_count + discard_count) / total_inspections * 100)
            ELSE 0
        END
    ) STORED COMMENT '불량률 (%)',

    -- 시간 정보
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP COMMENT '집계 생성 시간',
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '마지막 업데이트',

    -- 유니크 제약조건 (시간 + 제품코드)
    UNIQUE KEY uk_hour_product (hour_timestamp, product_code),

    -- 인덱스
    INDEX idx_hour_timestamp (hour_timestamp),
    INDEX idx_product_code (product_code),
    INDEX idx_hour_product (hour_timestamp, product_code),

    FOREIGN KEY (product_code) REFERENCES products(product_code)
        ON DELETE CASCADE
        ON UPDATE CASCADE

) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='시간별 검사 집계 테이블 (10년 보관)';
```

**쿼리 예시**:
```sql
-- 오늘 시간별 통계 조회
SELECT hour_timestamp, product_code, total_inspections,
       normal_count, missing_count, position_error_count, discard_count, defect_rate
FROM inspection_summary_hourly
WHERE DATE(hour_timestamp) = CURDATE()
ORDER BY hour_timestamp DESC;

-- 특정 제품의 최근 24시간 통계
SELECT hour_timestamp, total_inspections, defect_rate
FROM inspection_summary_hourly
WHERE product_code = 'FT'
  AND hour_timestamp >= DATE_SUB(NOW(), INTERVAL 24 HOUR)
ORDER BY hour_timestamp ASC;
```

---

### 5. inspection_summary_daily (일별 집계)

일 단위 검사 집계 테이블입니다. **10년 보관 정책** 적용.

```sql
CREATE TABLE inspection_summary_daily (
    id BIGINT AUTO_INCREMENT PRIMARY KEY COMMENT '집계 ID',

    -- 집계 기준
    date DATE NOT NULL COMMENT '날짜 (YYYY-MM-DD)',
    product_code VARCHAR(10) NOT NULL COMMENT '제품 코드',

    -- 집계 데이터
    total_inspections INT NOT NULL DEFAULT 0 COMMENT '총 검사 수',
    normal_count INT NOT NULL DEFAULT 0 COMMENT '정상 수',
    missing_count INT NOT NULL DEFAULT 0 COMMENT '부품 누락 수',
    position_error_count INT NOT NULL DEFAULT 0 COMMENT '위치 오류 수',
    discard_count INT NOT NULL DEFAULT 0 COMMENT '폐기 수',

    -- 통계
    avg_inference_time_ms FLOAT NULL COMMENT '평균 추론 시간',
    avg_total_time_ms FLOAT NULL COMMENT '평균 총 처리 시간',
    avg_detection_count FLOAT NULL COMMENT '평균 검출 부품 수',
    avg_confidence FLOAT NULL COMMENT '평균 신뢰도',

    -- 불량률 (자동 계산)
    defect_rate FLOAT GENERATED ALWAYS AS (
        CASE
            WHEN total_inspections > 0
            THEN ((missing_count + position_error_count + discard_count) / total_inspections * 100)
            ELSE 0
        END
    ) STORED COMMENT '불량률 (%)',

    -- 시간 정보
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP COMMENT '집계 생성 시간',
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '마지막 업데이트',

    -- 유니크 제약조건 (날짜 + 제품코드)
    UNIQUE KEY uk_date_product (date, product_code),

    -- 인덱스
    INDEX idx_date (date),
    INDEX idx_product_code (product_code),
    INDEX idx_date_product (date, product_code),

    FOREIGN KEY (product_code) REFERENCES products(product_code)
        ON DELETE CASCADE
        ON UPDATE CASCADE

) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='일별 검사 집계 테이블 (10년 보관)';
```

**쿼리 예시**:
```sql
-- 최근 30일 일별 통계 조회
SELECT date, product_code, total_inspections, defect_rate
FROM inspection_summary_daily
WHERE date >= DATE_SUB(CURDATE(), INTERVAL 30 DAY)
ORDER BY date DESC, product_code;

-- 월별 집계 (일별 데이터에서 계산)
SELECT YEAR(date) as year, MONTH(date) as month, product_code,
       SUM(total_inspections) as total,
       AVG(defect_rate) as avg_defect_rate
FROM inspection_summary_daily
WHERE date >= DATE_SUB(CURDATE(), INTERVAL 12 MONTH)
GROUP BY YEAR(date), MONTH(date), product_code
ORDER BY year DESC, month DESC;
```

---

### 6. inspection_summary_monthly (월별 집계)

월 단위 검사 집계 테이블입니다. **10년 보관 정책** 적용.

```sql
CREATE TABLE inspection_summary_monthly (
    id BIGINT AUTO_INCREMENT PRIMARY KEY COMMENT '집계 ID',

    -- 집계 기준
    year INT NOT NULL COMMENT '년도',
    month INT NOT NULL COMMENT '월 (1-12)',
    product_code VARCHAR(10) NOT NULL COMMENT '제품 코드',

    -- 집계 데이터
    total_inspections INT NOT NULL DEFAULT 0 COMMENT '총 검사 수',
    normal_count INT NOT NULL DEFAULT 0 COMMENT '정상 수',
    missing_count INT NOT NULL DEFAULT 0 COMMENT '부품 누락 수',
    position_error_count INT NOT NULL DEFAULT 0 COMMENT '위치 오류 수',
    discard_count INT NOT NULL DEFAULT 0 COMMENT '폐기 수',

    -- 통계
    avg_inference_time_ms FLOAT NULL COMMENT '평균 추론 시간',
    avg_total_time_ms FLOAT NULL COMMENT '평균 총 처리 시간',
    avg_detection_count FLOAT NULL COMMENT '평균 검출 부품 수',
    avg_confidence FLOAT NULL COMMENT '평균 신뢰도',

    -- 불량률 (자동 계산)
    defect_rate FLOAT GENERATED ALWAYS AS (
        CASE
            WHEN total_inspections > 0
            THEN ((missing_count + position_error_count + discard_count) / total_inspections * 100)
            ELSE 0
        END
    ) STORED COMMENT '불량률 (%)',

    -- 시간 정보
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP COMMENT '집계 생성 시간',
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '마지막 업데이트',

    -- 유니크 제약조건 (년월 + 제품코드)
    UNIQUE KEY uk_year_month_product (year, month, product_code),

    -- 인덱스
    INDEX idx_year_month (year, month),
    INDEX idx_product_code (product_code),
    INDEX idx_year_month_product (year, month, product_code),

    FOREIGN KEY (product_code) REFERENCES products(product_code)
        ON DELETE CASCADE
        ON UPDATE CASCADE

) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='월별 검사 집계 테이블 (10년 보관)';
```

**쿼리 예시**:
```sql
-- 최근 12개월 월별 통계
SELECT year, month, product_code, total_inspections, defect_rate
FROM inspection_summary_monthly
WHERE (year = YEAR(CURDATE()) AND month <= MONTH(CURDATE()))
   OR (year = YEAR(CURDATE()) - 1 AND month > MONTH(CURDATE()))
ORDER BY year DESC, month DESC, product_code;

-- 연도별 통계 (월별 데이터에서 계산)
SELECT year, product_code,
       SUM(total_inspections) as total,
       AVG(defect_rate) as avg_defect_rate
FROM inspection_summary_monthly
GROUP BY year, product_code
ORDER BY year DESC;
```

---

## 트리거 (자동 집계)

`inspections` 테이블에 새로운 검사 결과가 INSERT될 때 자동으로 3개의 집계 테이블을 업데이트하는 트리거가 설정되어 있습니다.

**트리거 목록**:
1. `update_hourly_summary`: 시간별 집계 자동 업데이트
2. `update_daily_summary`: 일별 집계 자동 업데이트
3. `update_monthly_summary`: 월별 집계 자동 업데이트

**트리거 확인**:
```sql
-- 트리거 목록 확인
SHOW TRIGGERS FROM pcb_inspection;

-- 트리거 상세 정보
SHOW CREATE TRIGGER update_hourly_summary;
SHOW CREATE TRIGGER update_daily_summary;
SHOW CREATE TRIGGER update_monthly_summary;
```

**동작 방식**:
- UPSERT 패턴 사용 (INSERT ... ON DUPLICATE KEY UPDATE)
- 기존 레코드가 있으면 UPDATE, 없으면 INSERT
- 평균값은 누적합 방식으로 재계산

**트리거 파일**: `database/triggers_v3.0.sql`

---

## 이벤트 스케줄러 (자동 데이터 정리)

**10년 이상 된 데이터를 자동으로 삭제**하는 이벤트 스케줄러가 설정되어 있습니다.

**이벤트 목록**:
1. `cleanup_old_inspections`: 검사 이력 정리 (매일 02:10)
2. `cleanup_old_hourly_summary`: 시간별 집계 정리 (매일 02:20)
3. `cleanup_old_daily_summary`: 일별 집계 정리 (매일 02:30)
4. `cleanup_old_monthly_summary`: 월별 집계 정리 (매일 02:40)

**이벤트 스케줄러 활성화**:
```sql
-- 이벤트 스케줄러 활성화 (서버 설정)
SET GLOBAL event_scheduler = ON;

-- 상태 확인
SELECT @@event_scheduler;
```

**이벤트 확인**:
```sql
-- 이벤트 목록 확인
SHOW EVENTS FROM pcb_inspection;

-- 이벤트 상세 정보
SHOW CREATE EVENT cleanup_old_inspections;
```

**이벤트 파일**: `database/events_v3.0.sql`

**my.cnf 설정 (서버 재시작 시 자동 활성화)**:
```ini
[mysqld]
event_scheduler = ON
```

---

## 기타 테이블

### 7. system_logs (시스템 로그)

```sql
CREATE TABLE system_logs (
    id INT AUTO_INCREMENT PRIMARY KEY,
    log_level ENUM('DEBUG', 'INFO', 'WARNING', 'ERROR', 'CRITICAL') NOT NULL DEFAULT 'INFO',
    source VARCHAR(100) NOT NULL COMMENT '로그 소스 (server/raspberry-pi-1/raspberry-pi-2/winforms)',
    message TEXT NOT NULL COMMENT '로그 메시지',
    details JSON NULL COMMENT '상세 정보 (JSON)',
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    INDEX idx_log_level (log_level),
    INDEX idx_source (source),
    INDEX idx_created_at (created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='시스템 로그';
```

---

### 8. system_config (시스템 설정)

```sql
CREATE TABLE system_config (
    id INT AUTO_INCREMENT PRIMARY KEY,
    config_key VARCHAR(100) NOT NULL UNIQUE COMMENT '설정 키',
    config_value TEXT NOT NULL COMMENT '설정 값',
    description VARCHAR(500) NULL COMMENT '설명',
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    updated_by INT NULL COMMENT '수정한 사용자 ID',

    INDEX idx_config_key (config_key)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='시스템 설정';
```

**초기 설정 값**:
```sql
INSERT INTO system_config (config_key, config_value, description) VALUES
('server_url', 'http://100.64.1.1:5000', 'Flask 서버 URL'),
('fps', '10', '카메라 FPS'),
('jpeg_quality', '85', 'JPEG 압축 품질'),
('position_threshold', '20.0', '위치 오차 허용 임계값 (픽셀)'),
('gpio_duration_ms', '500', 'GPIO 신호 지속 시간 (밀리초)'),
('max_image_retention_days', '90', '불량 이미지 보관 기간 (일)');
```

---

### 9. users (사용자/작업자)

```sql
CREATE TABLE users (
    id INT AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE COMMENT '사용자명',
    password_hash VARCHAR(255) NOT NULL COMMENT '비밀번호 해시',
    full_name VARCHAR(100) NULL COMMENT '전체 이름',
    role ENUM('admin', 'operator', 'viewer') NOT NULL DEFAULT 'viewer' COMMENT '권한',
    is_active BOOLEAN NOT NULL DEFAULT TRUE COMMENT '활성화 여부',
    last_login DATETIME NULL COMMENT '마지막 로그인',
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    INDEX idx_username (username),
    INDEX idx_role (role)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='사용자 계정';
```

**샘플 데이터**:
```sql
-- 비밀번호: admin123 (실제로는 해시 사용)
INSERT INTO users (username, password_hash, full_name, role) VALUES
('admin', '$2b$12$examplehashedpassword', '관리자', 'admin'),
('operator1', '$2b$12$examplehashedpassword', '작업자1', 'operator'),
('viewer1', '$2b$12$examplehashedpassword', '조회자1', 'viewer');
```

---

## 뷰 (View)

### 실시간 통계 뷰

```sql
CREATE VIEW v_realtime_statistics AS
SELECT
    DATE(inspection_time) AS stat_date,
    HOUR(inspection_time) AS stat_hour,
    product_code,
    COUNT(*) AS total_inspections,
    SUM(CASE WHEN decision = 'normal' THEN 1 ELSE 0 END) AS normal_count,
    SUM(CASE WHEN decision = 'missing' THEN 1 ELSE 0 END) AS missing_count,
    SUM(CASE WHEN decision = 'position_error' THEN 1 ELSE 0 END) AS position_error_count,
    SUM(CASE WHEN decision = 'discard' THEN 1 ELSE 0 END) AS discard_count,
    ROUND(
        (SUM(CASE WHEN decision != 'normal' THEN 1 ELSE 0 END) * 100.0 / COUNT(*)),
        2
    ) AS defect_rate,
    AVG(inference_time_ms) AS avg_inference_ms,
    AVG(total_time_ms) AS avg_total_ms
FROM inspections
WHERE inspection_time >= CURDATE()
GROUP BY stat_date, stat_hour, product_code
ORDER BY stat_date DESC, stat_hour DESC;
```

**사용 예시**:
```sql
-- 오늘 실시간 통계 조회
SELECT * FROM v_realtime_statistics
WHERE stat_date = CURDATE();

-- 특정 제품의 실시간 통계
SELECT * FROM v_realtime_statistics
WHERE product_code = 'FT'
  AND stat_date = CURDATE();
```

---

## Python 연결 예제

### PyMySQL 사용

```python
import pymysql
import json
from datetime import datetime

# 연결
conn = pymysql.connect(
    host='localhost',
    user='root',
    password='your_password',
    database='pcb_inspection',
    charset='utf8mb4',
    cursorclass=pymysql.cursors.DictCursor
)

try:
    with conn.cursor() as cursor:
        # 검사 결과 삽입
        sql = """INSERT INTO inspections
                 (serial_number, product_code, qr_data,
                  qr_detected, serial_detected,
                  decision, missing_count, position_error_count,
                  extra_count, correct_count,
                  missing_components, position_errors, extra_components,
                  yolo_detections, detection_count, avg_confidence,
                  inference_time_ms, verification_time_ms, total_time_ms,
                  left_image_path, right_image_path,
                  image_width, image_height,
                  camera_id, client_ip)
                 VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s,
                         %s, %s, %s, %s, %s, %s, %s, %s, %s, %s,
                         %s, %s, %s, %s, %s)"""

        # JSON 데이터 준비
        missing_components = json.dumps([
            {
                "class_name": "capacitor",
                "expected_position": {
                    "center": [120, 85],
                    "bbox": [100, 70, 140, 100]
                }
            }
        ])

        position_errors = json.dumps([
            {
                "class_name": "resistor",
                "expected": {"center": [200, 150]},
                "actual": {"center": [225, 155]},
                "offset": 25.5
            }
        ])

        cursor.execute(sql, (
            'MBFT12345678',      # serial_number
            'FT',                # product_code
            'http://localhost:8080/product/MBFT12345678',  # qr_data
            True,                # qr_detected
            True,                # serial_detected
            'position_error',    # decision
            1,                   # missing_count
            1,                   # position_error_count
            0,                   # extra_count
            23,                  # correct_count
            missing_components,  # missing_components (JSON)
            position_errors,     # position_errors (JSON)
            None,                # extra_components
            None,                # yolo_detections
            24,                  # detection_count
            0.89,                # avg_confidence
            45.2,                # inference_time_ms
            12.3,                # verification_time_ms
            65.5,                # total_time_ms
            '/images/left/pcb_001.jpg',   # left_image_path
            '/images/right/pcb_001.jpg',  # right_image_path
            640, 480,            # image_width, image_height
            'left',              # camera_id
            '100.64.1.2'         # client_ip
        ))
    conn.commit()

    # 검사 이력 조회
    with conn.cursor() as cursor:
        sql = """SELECT id, serial_number, product_code, decision,
                        missing_count, position_error_count,
                        total_time_ms, inspection_time
                 FROM inspections
                 ORDER BY inspection_time DESC
                 LIMIT 10"""
        cursor.execute(sql)
        results = cursor.fetchall()
        for row in results:
            print(f"ID: {row['id']}, Serial: {row['serial_number']}, "
                  f"Product: {row['product_code']}, Decision: {row['decision']}, "
                  f"Missing: {row['missing_count']}, "
                  f"Position Error: {row['position_error_count']}, "
                  f"Time: {row['total_time_ms']}ms")

finally:
    conn.close()
```

---

## C# 연결 예제

### MySql.Data 사용

```csharp
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

string connStr = "server=localhost;user=root;database=pcb_inspection;password=your_password;";
MySqlConnection conn = new MySqlConnection(connStr);

try
{
    conn.Open();

    // JSON 데이터 준비
    var missingComponents = new List<object>
    {
        new {
            class_name = "capacitor",
            expected_position = new {
                center = new[] { 120, 85 },
                bbox = new[] { 100, 70, 140, 100 }
            }
        }
    };

    var positionErrors = new List<object>
    {
        new {
            class_name = "resistor",
            expected = new { center = new[] { 200, 150 } },
            actual = new { center = new[] { 225, 155 } },
            offset = 25.5
        }
    };

    // 검사 결과 삽입
    string sql = @"INSERT INTO inspections
                   (serial_number, product_code, qr_data,
                    qr_detected, serial_detected,
                    decision, missing_count, position_error_count,
                    extra_count, correct_count,
                    missing_components, position_errors,
                    detection_count, avg_confidence,
                    inference_time_ms, verification_time_ms, total_time_ms,
                    left_image_path, right_image_path,
                    image_width, image_height,
                    camera_id, client_ip)
                   VALUES (@serial_number, @product_code, @qr_data,
                           @qr_detected, @serial_detected,
                           @decision, @missing_count, @position_error_count,
                           @extra_count, @correct_count,
                           @missing_components, @position_errors,
                           @detection_count, @avg_confidence,
                           @inference_time_ms, @verification_time_ms, @total_time_ms,
                           @left_image_path, @right_image_path,
                           @image_width, @image_height,
                           @camera_id, @client_ip)";

    MySqlCommand cmd = new MySqlCommand(sql, conn);
    cmd.Parameters.AddWithValue("@serial_number", "MBFT12345678");
    cmd.Parameters.AddWithValue("@product_code", "FT");
    cmd.Parameters.AddWithValue("@qr_data", "http://localhost:8080/product/MBFT12345678");
    cmd.Parameters.AddWithValue("@qr_detected", true);
    cmd.Parameters.AddWithValue("@serial_detected", true);
    cmd.Parameters.AddWithValue("@decision", "position_error");
    cmd.Parameters.AddWithValue("@missing_count", 1);
    cmd.Parameters.AddWithValue("@position_error_count", 1);
    cmd.Parameters.AddWithValue("@extra_count", 0);
    cmd.Parameters.AddWithValue("@correct_count", 23);
    cmd.Parameters.AddWithValue("@missing_components", JsonConvert.SerializeObject(missingComponents));
    cmd.Parameters.AddWithValue("@position_errors", JsonConvert.SerializeObject(positionErrors));
    cmd.Parameters.AddWithValue("@detection_count", 24);
    cmd.Parameters.AddWithValue("@avg_confidence", 0.89);
    cmd.Parameters.AddWithValue("@inference_time_ms", 45.2);
    cmd.Parameters.AddWithValue("@verification_time_ms", 12.3);
    cmd.Parameters.AddWithValue("@total_time_ms", 65.5);
    cmd.Parameters.AddWithValue("@left_image_path", "/images/left/pcb_001.jpg");
    cmd.Parameters.AddWithValue("@right_image_path", "/images/right/pcb_001.jpg");
    cmd.Parameters.AddWithValue("@image_width", 640);
    cmd.Parameters.AddWithValue("@image_height", 480);
    cmd.Parameters.AddWithValue("@camera_id", "left");
    cmd.Parameters.AddWithValue("@client_ip", "100.64.1.2");

    cmd.ExecuteNonQuery();

    // 시간별 집계 조회 (WinForms 필터링)
    string selectSql = @"SELECT hour_timestamp, product_code,
                                total_inspections, normal_count,
                                missing_count, position_error_count,
                                discard_count, defect_rate
                         FROM inspection_summary_hourly
                         WHERE DATE(hour_timestamp) = CURDATE()
                         ORDER BY hour_timestamp DESC";

    MySqlCommand selectCmd = new MySqlCommand(selectSql, conn);
    using (MySqlDataReader reader = selectCmd.ExecuteReader())
    {
        while (reader.Read())
        {
            Console.WriteLine($"Time: {reader["hour_timestamp"]}, " +
                            $"Product: {reader["product_code"]}, " +
                            $"Total: {reader["total_inspections"]}, " +
                            $"Defect Rate: {reader["defect_rate"]}%");
        }
    }
}
finally
{
    conn.Close();
}
```

---

## 보안 설정

### 사용자 계정 및 권한 관리

#### 1. Flask 서버용 MySQL 사용자 생성 (읽기/쓰기)

```sql
-- Flask 서버 전용 사용자 생성
CREATE USER 'flask_server'@'100.64.1.1' IDENTIFIED BY 'STRONG_PASSWORD_HERE';

-- pcb_inspection 데이터베이스에 대한 권한 부여
GRANT SELECT, INSERT, UPDATE ON pcb_inspection.* TO 'flask_server'@'100.64.1.1';

-- 변경사항 적용
FLUSH PRIVILEGES;
```

#### 2. C# WinForms 모니터링 앱용 MySQL 사용자 생성 (읽기 전용)

```sql
-- C# WinForms 앱 전용 사용자 생성
CREATE USER 'winforms_app'@'100.64.1.5' IDENTIFIED BY 'STRONG_PASSWORD_HERE';

-- 읽기 전용 권한 부여
GRANT SELECT ON pcb_inspection.* TO 'winforms_app'@'100.64.1.5';

-- 변경사항 적용
FLUSH PRIVILEGES;
```

#### 3. 관리자용 사용자 (전체 권한)

```sql
-- 관리자 계정 생성
CREATE USER 'pcb_admin'@'localhost' IDENTIFIED BY 'ADMIN_STRONG_PASSWORD_HERE';

-- 모든 권한 부여 (백업, 복구, 스키마 변경 등)
GRANT ALL PRIVILEGES ON pcb_inspection.* TO 'pcb_admin'@'localhost';

-- 변경사항 적용
FLUSH PRIVILEGES;
```

---

### 네트워크 보안 설정

#### MySQL 외부 접속 허용 설정

```bash
# MySQL 설정 파일 편집 (Ubuntu/Debian)
sudo nano /etc/mysql/mysql.conf.d/mysqld.cnf

# [mysqld] 섹션에서 bind-address 수정
# 변경 전: bind-address = 127.0.0.1
# 변경 후: bind-address = 0.0.0.0

# MySQL 재시작
sudo systemctl restart mysql
```

#### 방화벽 설정 (Ubuntu/Debian)

```bash
# MySQL 포트 (3306) 개방 - 특정 IP만 허용
sudo ufw allow from 100.64.1.1 to any port 3306 comment 'Flask Server'
sudo ufw allow from 100.64.1.5 to any port 3306 comment 'Windows PC'

# 방화벽 규칙 확인
sudo ufw status
```

---

## 백업 전략

### 정기 백업 (cron 사용)

```bash
#!/bin/bash
# backup_database.sh

DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_DIR="/home/backup/mysql"
DB_NAME="pcb_inspection"
DB_USER="pcb_admin"
DB_PASS="your_password"

mkdir -p $BACKUP_DIR

# 전체 데이터베이스 백업
mysqldump -u$DB_USER -p$DB_PASS $DB_NAME > $BACKUP_DIR/pcb_inspection_$DATE.sql

# 7일 이상 된 백업 파일 삭제
find $BACKUP_DIR -name "*.sql" -mtime +7 -delete

echo "Backup completed: pcb_inspection_$DATE.sql"
```

**cron 등록 (매일 새벽 1시)**:
```bash
crontab -e

# 추가
0 1 * * * /home/backup/backup_database.sh
```

---

## 스키마 설치

### 전체 스키마 설치 순서

```bash
# 1. 스키마 생성 (테이블)
mysql -u root -p pcb_inspection < database/schema_v3.0_product_verification.sql

# 2. 트리거 생성
mysql -u root -p pcb_inspection < database/triggers_v3.0.sql

# 3. 이벤트 스케줄러 생성
mysql -u root -p pcb_inspection < database/events_v3.0.sql

# 4. 사용자 계정 생성 (보안 설정)
mysql -u root -p pcb_inspection < database/create_users.sql
```

---

## 관련 문서

이 데이터베이스 설계는 다음 문서들과 연계되어 있습니다:

1. **PCB_Defect_Detection_Project.md** - 전체 시스템 아키텍처 및 프로젝트 개요
2. **Flask_Server_Setup.md** - Flask 서버에서 MySQL 연동 (PyMySQL)
3. **CSharp_WinForms_Design_Specification.md** - C# WinForms에서 MySQL 연동 (집계 테이블 조회)
4. **RaspberryPi_Setup.md** - 라즈베리파이 클라이언트 (간접적으로 Flask 서버를 통해 연동)

각 문서에서 이 데이터베이스 스키마를 참조하여 시스템 통합을 구현합니다.

---

**작성일**: 2025-11-28
**버전**: 3.0 ⭐ (Product Verification Architecture)
**데이터베이스**: MySQL 8.0
**문자 인코딩**: UTF-8 (utf8mb4)

**주요 변경사항**:
- **3.0 (2025-11-28)**: Product Verification Architecture 적용
  - 제품 식별 시스템 (시리얼 넘버 + QR 코드)
  - 제품별 부품 배치 검증
  - products, product_components 테이블 신규 추가
  - inspections 테이블 완전 재설계
  - 시간별/일별/월별 집계 테이블 추가
  - 트리거 기반 자동 집계 시스템
  - 이벤트 스케줄러 기반 10년 자동 정리 시스템
  - 4단계 판정: normal/missing/position_error/discard
  - JSON 필드: missing_components, position_errors, extra_components
  - Python/C# 연결 예제 완전 업데이트
