# MySQL 데이터베이스 설계 - PCB 검사 시스템 ⭐ (이중 모델 아키텍처)

## 개요

PCB 불량 검사 시스템의 **양면 동시 검사 이력**, 통계, 시스템 로그를 저장하는 MySQL 데이터베이스 스키마 설계입니다.

**⭐ 이중 모델 아키텍처 특징**:
- **Component Model (부품 검출)**: FPIC-Component, 25 클래스
- **Solder Model (납땜 불량)**: SolDef_AI, 5-6 클래스
- **Result Fusion (결과 융합)**: Flask 서버에서 두 모델 결과를 융합하여 최종 판정 (normal, component_defect, solder_defect, discard)

---

## 데이터베이스 생성

```sql
CREATE DATABASE IF NOT EXISTS pcb_inspection
CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci;

USE pcb_inspection;
```

---

## 테이블 스키마

### 1. inspections (양면 동시 검사 결과 이력) ⭐ 이중 모델

```sql
CREATE TABLE inspections (
    id INT AUTO_INCREMENT PRIMARY KEY,

    -- 융합 결과 (최종 판정)
    fusion_decision VARCHAR(50) NOT NULL COMMENT '융합 결과 판정 (normal/component_defect/solder_defect/discard)',
    fusion_severity_level INT NOT NULL DEFAULT 0 COMMENT '융합 심각도 레벨 (0-3)',

    -- Component Model 결과 (좌측 카메라, 부품 검출)
    component_defects JSON NULL COMMENT '부품 불량 목록 (JSON 배열)',
    component_defect_count INT NOT NULL DEFAULT 0 COMMENT '부품 불량 개수',
    component_inference_time_ms DECIMAL(6,2) NULL COMMENT 'Component 모델 추론 시간 (밀리초)',

    -- Solder Model 결과 (우측 카메라, 납땜 검출)
    solder_defects JSON NULL COMMENT '납땜 불량 목록 (JSON 배열)',
    solder_defect_count INT NOT NULL DEFAULT 0 COMMENT '납땜 불량 개수',
    solder_inference_time_ms DECIMAL(6,2) NULL COMMENT 'Solder 모델 추론 시간 (밀리초)',

    -- 검사 메타데이터
    inspection_time DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '검사 시간',
    total_inference_time_ms DECIMAL(6,2) NULL COMMENT '전체 추론 시간 (양면 병렬)',

    -- 이미지 정보
    left_image_path VARCHAR(500) NULL COMMENT '좌측 카메라 이미지 경로 (부품면)',
    right_image_path VARCHAR(500) NULL COMMENT '우측 카메라 이미지 경로 (납땜면)',

    -- GPIO 제어 정보
    gpio_pin INT NULL COMMENT '활성화된 GPIO 핀 번호',
    gpio_duration_ms INT NULL COMMENT 'GPIO 신호 지속 시간 (밀리초)',

    -- 사용자 및 비고
    user_id INT NULL COMMENT '작업자 ID',
    notes TEXT NULL COMMENT '비고',

    INDEX idx_inspection_time (inspection_time),
    INDEX idx_fusion_decision (fusion_decision),
    INDEX idx_component_defect_count (component_defect_count),
    INDEX idx_solder_defect_count (solder_defect_count),
    INDEX idx_user_id (user_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='PCB 양면 동시 검사 결과 이력 (이중 YOLO 모델 아키텍처)';
```

**JSON 데이터 구조 예시**:

**component_defects** (부품 불량):
```json
[
  {
    "type": "missing_component",
    "confidence": 0.95,
    "bbox": [120, 85, 150, 110],
    "class_id": 3,
    "class_name": "resistor"
  },
  {
    "type": "misalignment",
    "confidence": 0.88,
    "bbox": [200, 150, 230, 175],
    "class_id": 7,
    "class_name": "capacitor"
  }
]
```

**solder_defects** (납땜 불량):
```json
[
  {
    "type": "cold_joint",
    "confidence": 0.92,
    "bbox": [310, 220, 335, 245],
    "class_id": 1,
    "class_name": "solder_joint"
  },
  {
    "type": "insufficient_solder",
    "confidence": 0.87,
    "bbox": [405, 180, 425, 200],
    "class_id": 2,
    "class_name": "solder_pad"
  },
  {
    "type": "solder_bridge",
    "confidence": 0.93,
    "bbox": [125, 95, 160, 115],
    "class_id": 3,
    "class_name": "solder_bridge"
  }
]
```

### 2. defect_images (불량 이미지 메타데이터)

```sql
CREATE TABLE defect_images (
    id INT AUTO_INCREMENT PRIMARY KEY,
    inspection_id INT NOT NULL COMMENT '검사 ID (FK)',
    image_path VARCHAR(500) NOT NULL COMMENT '이미지 파일 경로',
    image_size_bytes INT NULL COMMENT '파일 크기 (바이트)',
    image_width INT NULL COMMENT '이미지 너비',
    image_height INT NULL COMMENT '이미지 높이',
    upload_time DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '업로드 시간',

    FOREIGN KEY (inspection_id) REFERENCES inspections(id)
        ON DELETE CASCADE
        ON UPDATE CASCADE,

    INDEX idx_inspection_id (inspection_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='불량 이미지 파일 정보';
```

### 3. statistics_daily (일별 통계) ⭐ 이중 모델 융합 결과 기반

```sql
CREATE TABLE statistics_daily (
    id INT AUTO_INCREMENT PRIMARY KEY,
    stat_date DATE NOT NULL UNIQUE COMMENT '통계 날짜',

    -- 검사 통계 (융합 결과 기반)
    total_inspections INT NOT NULL DEFAULT 0 COMMENT '총 검사 수 (양면 동시)',
    normal_count INT NOT NULL DEFAULT 0 COMMENT '정상 개수 (fusion_decision=normal)',
    component_defect_count INT NOT NULL DEFAULT 0 COMMENT '부품 불량 개수 (fusion_decision=component_defect)',
    solder_defect_count INT NOT NULL DEFAULT 0 COMMENT '납땜 불량 개수 (fusion_decision=solder_defect)',
    discard_count INT NOT NULL DEFAULT 0 COMMENT '폐기 개수 (fusion_decision=discard)',

    -- 불량률 (자동 계산)
    defect_rate DECIMAL(5,2) GENERATED ALWAYS AS (
        CASE
            WHEN total_inspections > 0 THEN
                (component_defect_count + solder_defect_count + discard_count) * 100.0 / total_inspections
            ELSE 0
        END
    ) STORED COMMENT '불량률 (%, 자동 계산)',

    -- 평균 추론 시간
    avg_component_inference_ms DECIMAL(6,2) NULL COMMENT 'Component 모델 평균 추론 시간 (밀리초)',
    avg_solder_inference_ms DECIMAL(6,2) NULL COMMENT 'Solder 모델 평균 추론 시간 (밀리초)',
    avg_total_inference_ms DECIMAL(6,2) NULL COMMENT '전체 평균 추론 시간 (밀리초)',

    -- 타임스탬프
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    INDEX idx_stat_date (stat_date)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='일별 검사 통계 (이중 모델 융합 결과 기반)';
```

### 4. statistics_hourly (시간별 통계) ⭐ 이중 모델 융합 결과 기반

```sql
CREATE TABLE statistics_hourly (
    id INT AUTO_INCREMENT PRIMARY KEY,
    stat_datetime DATETIME NOT NULL COMMENT '통계 시간 (YYYY-MM-DD HH:00:00)',

    -- 검사 통계 (융합 결과 기반)
    total_inspections INT NOT NULL DEFAULT 0 COMMENT '총 검사 수',
    normal_count INT NOT NULL DEFAULT 0 COMMENT '정상 개수 (fusion_decision=normal)',
    component_defect_count INT NOT NULL DEFAULT 0 COMMENT '부품 불량 개수 (fusion_decision=component_defect)',
    solder_defect_count INT NOT NULL DEFAULT 0 COMMENT '납땜 불량 개수 (fusion_decision=solder_defect)',
    discard_count INT NOT NULL DEFAULT 0 COMMENT '폐기 개수 (fusion_decision=discard)',

    -- 타임스탬프
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    UNIQUE KEY uk_stat_datetime (stat_datetime),
    INDEX idx_stat_datetime (stat_datetime)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='시간별 검사 통계 (이중 모델 융합 결과 기반)';
```

### 5. system_logs (시스템 로그)

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

### 6. system_config (시스템 설정)

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

### 7. users (사용자/작업자)

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

### 8. alerts (알람/알림)

```sql
CREATE TABLE alerts (
    id INT AUTO_INCREMENT PRIMARY KEY,
    alert_type ENUM('defect_rate_high', 'system_error', 'camera_offline', 'server_offline', 'box_full') NOT NULL,
    severity ENUM('low', 'medium', 'high', 'critical') NOT NULL DEFAULT 'medium',
    message TEXT NOT NULL COMMENT '알람 메시지',
    details JSON NULL COMMENT '상세 정보',
    is_resolved BOOLEAN NOT NULL DEFAULT FALSE COMMENT '해결 여부',
    resolved_at DATETIME NULL COMMENT '해결 시간',
    resolved_by INT NULL COMMENT '해결한 사용자 ID',
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    INDEX idx_alert_type (alert_type),
    INDEX idx_severity (severity),
    INDEX idx_is_resolved (is_resolved),
    INDEX idx_created_at (created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='알람 및 알림 (box_full 추가)';
```

### 9. box_status (로봇팔 박스 상태 관리) ⭐ 신규

```sql
CREATE TABLE box_status (
    id INT AUTO_INCREMENT PRIMARY KEY,

    -- 박스 정보
    box_id VARCHAR(20) NOT NULL UNIQUE COMMENT '박스 ID (NORMAL, COMPONENT_DEFECT, SOLDER_DEFECT)',
    category VARCHAR(50) NOT NULL COMMENT '분류 카테고리 (normal/component_defect/solder_defect)',

    -- 슬롯 상태
    current_slot INT NOT NULL DEFAULT 0 COMMENT '현재 사용 중인 슬롯 번호 (0-2, 수평 3슬롯)',
    max_slots INT NOT NULL DEFAULT 3 COMMENT '최대 슬롯 개수 (3개, 수평 배치)',
    is_full BOOLEAN NOT NULL DEFAULT FALSE COMMENT '박스 가득참 여부',

    -- 통계
    total_pcb_count INT NOT NULL DEFAULT 0 COMMENT '박스에 저장된 총 PCB 개수',

    -- 타임스탬프
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP COMMENT '박스 생성 시각',
    last_updated DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '마지막 업데이트 시각',

    -- 인덱스
    INDEX idx_category (category),
    INDEX idx_is_full (is_full)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='로봇팔 박스 슬롯 상태 관리 테이블 (3개 박스 × 3개 슬롯 = 9개 슬롯, 폐기는 슬롯 관리 안 함)';
```

**설명**:
- **총 3개 박스**: 정상, 부품불량, 납땜불량
- **각 박스: 3개 슬롯** (수평 배치, slot 0~2)
- **총 9개 슬롯** = 3 카테고리 × 3 슬롯
- **DISCARD 처리**: 슬롯 관리 없이 고정 위치에 떨어뜨리기 (프로젝트 데모용)
- **슬롯 할당 로직**:
  1. 각 박스는 slot 0부터 2까지 순차 채움
  2. 사용률은 WinForms + Flask 서버에서 0/3 → 3/3로 표시
  3. 박스가 가득 차면(3/3): LED 알림 + WinForms 알림 + OHT 자동 호출(`trigger_reason='box_full'`)

**박스 ID 구조**:
- `NORMAL`: 정상 PCB (3개 슬롯)
- `COMPONENT_DEFECT`: 부품 불량 (3개 슬롯)
- `SOLDER_DEFECT`: 납땜 불량 (3개 슬롯)
- `DISCARD`: 폐기 (슬롯 관리 안 함, box_status 테이블에 저장 안 함)

### 10. oht_operations (OHT 운영 이력) ⭐ 신규

```sql
CREATE TABLE oht_operations (
    id INT AUTO_INCREMENT PRIMARY KEY,

    -- 요청 정보
    operation_id VARCHAR(36) NOT NULL UNIQUE COMMENT 'OHT 운영 UUID',
    category ENUM('NORMAL', 'COMPONENT_DEFECT', 'SOLDER_DEFECT') NOT NULL COMMENT 'PCB 카테고리',

    -- 사용자 정보
    user_id INT NULL COMMENT '요청한 사용자 ID (NULL이면 시스템 자동)',
    user_role ENUM('Admin', 'Operator', 'System') NOT NULL COMMENT '사용자 역할',
    is_auto BOOLEAN NOT NULL DEFAULT FALSE COMMENT '자동 호출 여부',
    trigger_reason VARCHAR(50) NULL COMMENT '트리거 사유 (box_full 등)',

    -- 상태
    status ENUM('pending', 'processing', 'completed', 'failed') NOT NULL DEFAULT 'pending' COMMENT '운영 상태',

    -- 타임스탬프
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '요청 생성 시간',
    started_at DATETIME NULL COMMENT '운영 시작 시간',
    completed_at DATETIME NULL COMMENT '운영 완료 시간',

    -- 결과
    pcb_count INT NOT NULL DEFAULT 0 COMMENT '수거한 PCB 개수',
    success BOOLEAN NULL COMMENT '성공 여부',
    error_message TEXT NULL COMMENT '오류 메시지',
    execution_time_seconds DECIMAL(5, 2) NULL COMMENT '실행 시간 (초)',

    -- 인덱스
    INDEX idx_operation_id (operation_id),
    INDEX idx_category (category),
    INDEX idx_status (status),
    INDEX idx_is_auto (is_auto),
    INDEX idx_created_at (created_at),

    -- 외래 키
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='OHT (Overhead Hoist Transport) 운영 이력 테이블';
```

**설명**:
- **operation_id**: UUID 형식의 고유 식별자
- **category**: 수거할 PCB 카테고리 (정상/부품불량/납땜불량)
- **user_role**: Admin/Operator (수동 호출) 또는 System (자동 호출)
- **is_auto**: true = 박스 꽉 참 자동 호출, false = WinForms 수동 호출
- **trigger_reason**: 자동 호출 사유 (예: 'box_full')
- **status**: pending (대기) → processing (진행 중) → completed/failed (완료/실패)
- **execution_time_seconds**: 창고 → 분류박스 → 적재 → 창고 전체 시간

**쿼리 예시**:
```sql
-- 최근 OHT 운영 이력 (최근 10건)
SELECT operation_id, category, user_role, is_auto, status,
       created_at, execution_time_seconds
FROM oht_operations
ORDER BY created_at DESC
LIMIT 10;

-- 자동 호출 통계
SELECT category, COUNT(*) as auto_calls
FROM oht_operations
WHERE is_auto = TRUE
GROUP BY category;

-- 평균 실행 시간
SELECT category, AVG(execution_time_seconds) as avg_time
FROM oht_operations
WHERE status = 'completed'
GROUP BY category;

-- 실패 이력
SELECT operation_id, category, error_message, created_at
FROM oht_operations
WHERE status = 'failed'
ORDER BY created_at DESC;
```

---

### 11. user_logs (사용자 활동 로그) ⭐ 신규

```sql
CREATE TABLE user_logs (
    id INT AUTO_INCREMENT PRIMARY KEY,

    -- 사용자 정보
    user_id INT NOT NULL COMMENT '사용자 ID',
    username VARCHAR(50) NOT NULL COMMENT '사용자명 (참조용)',
    user_role ENUM('Admin', 'Operator', 'Viewer') NOT NULL COMMENT '사용자 권한',

    -- 활동 정보
    action_type ENUM(
        'login',
        'logout',
        'create_user',
        'update_user',
        'delete_user',
        'reset_password',
        'call_oht',
        'export_data',
        'view_inspection',
        'change_settings',
        'other'
    ) NOT NULL COMMENT '활동 유형',
    action_description VARCHAR(255) NULL COMMENT '활동 상세 설명',

    -- 시스템 정보
    ip_address VARCHAR(45) NULL COMMENT 'IP 주소 (IPv4/IPv6)',
    user_agent VARCHAR(255) NULL COMMENT 'User Agent (브라우저/클라이언트 정보)',

    -- 상세 정보
    details JSON NULL COMMENT '추가 상세 정보 (JSON 형식)',

    -- 타임스탬프
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '활동 발생 시간',

    -- 인덱스
    INDEX idx_user_id (user_id),
    INDEX idx_action_type (action_type),
    INDEX idx_created_at (created_at),
    INDEX idx_user_action (user_id, action_type),

    -- 외래 키
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='사용자 활동 이력 로그 테이블';
```

**설명**:
- **user_id**: 활동을 수행한 사용자 ID
- **username**: 사용자명 (users 테이블 변경 시에도 이력 유지)
- **action_type**: 활동 유형 (로그인, 사용자 관리, OHT 호출, 데이터 내보내기 등)
- **action_description**: 활동에 대한 상세 설명 (예: "사용자 'operator2' 생성")
- **ip_address**: 클라이언트 IP 주소 (IPv4/IPv6 지원)
- **user_agent**: 클라이언트 정보 (WinForms 앱, 브라우저 등)
- **details**: JSON 형식의 추가 정보 (예: 변경 전/후 값, OHT 카테고리 등)
- **created_at**: 활동 발생 시간

**쿼리 예시**:
```sql
-- 특정 사용자의 최근 활동 이력 (최근 20건)
SELECT action_type, action_description, ip_address, created_at
FROM user_logs
WHERE user_id = 1
ORDER BY created_at DESC
LIMIT 20;

-- 로그인 이력 조회
SELECT username, ip_address, created_at
FROM user_logs
WHERE action_type = 'login'
ORDER BY created_at DESC;

-- OHT 호출 이력 조회 (수동 호출만)
SELECT username, user_role, action_description, details, created_at
FROM user_logs
WHERE action_type = 'call_oht'
ORDER BY created_at DESC;

-- 사용자 관리 활동 이력
SELECT username, action_type, action_description, created_at
FROM user_logs
WHERE action_type IN ('create_user', 'update_user', 'delete_user', 'reset_password')
ORDER BY created_at DESC;

-- 날짜 범위별 활동 통계
SELECT action_type, COUNT(*) as count
FROM user_logs
WHERE created_at BETWEEN '2025-10-01' AND '2025-10-31'
GROUP BY action_type
ORDER BY count DESC;

-- 데이터 내보내기 이력
SELECT username, user_role, action_description, created_at
FROM user_logs
WHERE action_type = 'export_data'
ORDER BY created_at DESC;
```

**details 필드 JSON 예시**:
```json
// 사용자 생성
{
  "new_username": "operator2",
  "new_role": "Operator",
  "created_by": "admin"
}

// 비밀번호 초기화
{
  "target_username": "operator1",
  "reset_to": "temp1234"
}

// OHT 호출
{
  "category": "NORMAL",
  "is_auto": false,
  "operation_id": "550e8400-e29b-41d4-a716-446655440000"
}

// 데이터 내보내기
{
  "export_type": "excel",
  "date_range": "2025-10-01 ~ 2025-10-22",
  "row_count": 1523
}
```

---

## 초기 데이터 삽입

### 시스템 설정 기본값

```sql
INSERT INTO system_config (config_key, config_value, description) VALUES
('server_url', 'http://100.64.1.1:5000', 'Flask 서버 URL'),
('fps', '10', '카메라 FPS'),
('jpeg_quality', '85', 'JPEG 압축 품질'),
('defect_threshold', '0.70', '불량 판정 임계값 (신뢰도)'),
('gpio_duration_ms', '500', 'GPIO 신호 지속 시간 (밀리초)'),
('max_image_retention_days', '90', '불량 이미지 보관 기간 (일)'),
('alert_defect_rate_threshold', '10.0', '알람 발생 불량률 임계값 (%)');
```

### 기본 사용자 생성

```sql
-- 비밀번호: admin123 (실제로는 해시 사용)
INSERT INTO users (username, password_hash, full_name, role) VALUES
('admin', '$2b$12$examplehashedpassword', '관리자', 'admin'),
('operator1', '$2b$12$examplehashedpassword', '작업자1', 'operator'),
('viewer1', '$2b$12$examplehashedpassword', '조회자1', 'viewer');
```

### 박스 상태 초기화 ⭐ 신규

```sql
-- 3개 박스 초기 데이터 삽입 (DISCARD는 제외)
INSERT INTO box_status (box_id, category, max_slots) VALUES
    ('NORMAL', 'normal', 5),
    ('COMPONENT_DEFECT', 'component_defect', 5),
    ('SOLDER_DEFECT', 'solder_defect', 5);
```

**박스 상태 확인 쿼리**:
```sql
-- 전체 박스 상태 조회
SELECT box_id, category, current_slot, max_slots, is_full, total_pcb_count
FROM box_status
ORDER BY box_id;

-- 가득 찬 박스 조회
SELECT box_id, category, total_pcb_count
FROM box_status
WHERE is_full = TRUE;

-- 특정 카테고리의 박스 상태 조회
SELECT box_id, current_slot, max_slots, is_full
FROM box_status
WHERE category = 'normal';
```

---

## 뷰 (View) 정의

### 실시간 통계 뷰 ⭐ 이중 모델 융합 결과 기반

```sql
CREATE VIEW v_realtime_statistics AS
SELECT
    DATE(inspection_time) AS stat_date,
    HOUR(inspection_time) AS stat_hour,
    COUNT(*) AS total_inspections,
    SUM(CASE WHEN fusion_decision = 'normal' THEN 1 ELSE 0 END) AS normal_count,
    SUM(CASE WHEN fusion_decision = 'component_defect' THEN 1 ELSE 0 END) AS component_defect_count,
    SUM(CASE WHEN fusion_decision = 'solder_defect' THEN 1 ELSE 0 END) AS solder_defect_count,
    SUM(CASE WHEN fusion_decision = 'discard' THEN 1 ELSE 0 END) AS discard_count,
    ROUND(
        (SUM(CASE WHEN fusion_decision != 'normal' THEN 1 ELSE 0 END) * 100.0 / COUNT(*)),
        2
    ) AS defect_rate,
    AVG(component_inference_time_ms) AS avg_component_inference_ms,
    AVG(solder_inference_time_ms) AS avg_solder_inference_ms,
    AVG(total_inference_time_ms) AS avg_total_inference_ms
FROM inspections
WHERE inspection_time >= CURDATE()
GROUP BY stat_date, stat_hour
ORDER BY stat_date DESC, stat_hour DESC;
```

**뷰 사용 예시**:
```sql
-- 오늘 실시간 통계 조회
SELECT * FROM v_realtime_statistics
WHERE stat_date = CURDATE();

-- 최근 1시간 통계
SELECT * FROM v_realtime_statistics
WHERE stat_date = CURDATE()
AND stat_hour = HOUR(NOW());
```

---

## 저장 프로시저

### 1. 일별 통계 업데이트 ⭐ 이중 모델 융합 결과 기반

```sql
DELIMITER $$

CREATE PROCEDURE update_daily_statistics(IN target_date DATE)
BEGIN
    INSERT INTO statistics_daily (
        stat_date,
        total_inspections,
        normal_count,
        component_defect_count,
        solder_defect_count,
        discard_count,
        avg_component_inference_ms,
        avg_solder_inference_ms,
        avg_total_inference_ms
    )
    SELECT
        DATE(inspection_time) AS stat_date,
        COUNT(*) AS total_inspections,
        SUM(CASE WHEN fusion_decision = 'normal' THEN 1 ELSE 0 END) AS normal_count,
        SUM(CASE WHEN fusion_decision = 'component_defect' THEN 1 ELSE 0 END) AS component_defect_count,
        SUM(CASE WHEN fusion_decision = 'solder_defect' THEN 1 ELSE 0 END) AS solder_defect_count,
        SUM(CASE WHEN fusion_decision = 'discard' THEN 1 ELSE 0 END) AS discard_count,
        AVG(component_inference_time_ms) AS avg_component_inference_ms,
        AVG(solder_inference_time_ms) AS avg_solder_inference_ms,
        AVG(total_inference_time_ms) AS avg_total_inference_ms
    FROM inspections
    WHERE DATE(inspection_time) = target_date
    GROUP BY DATE(inspection_time)
    ON DUPLICATE KEY UPDATE
        total_inspections = VALUES(total_inspections),
        normal_count = VALUES(normal_count),
        component_defect_count = VALUES(component_defect_count),
        solder_defect_count = VALUES(solder_defect_count),
        discard_count = VALUES(discard_count),
        avg_component_inference_ms = VALUES(avg_component_inference_ms),
        avg_solder_inference_ms = VALUES(avg_solder_inference_ms),
        avg_total_inference_ms = VALUES(avg_total_inference_ms),
        updated_at = CURRENT_TIMESTAMP;
END$$

DELIMITER ;
```

### 2. 불량률 알람 체크

```sql
DELIMITER $$

CREATE PROCEDURE check_defect_rate_alert(IN target_date DATE)
BEGIN
    DECLARE current_defect_rate DECIMAL(5,2);
    DECLARE threshold DECIMAL(5,2);

    -- 현재 불량률 조회
    SELECT defect_rate INTO current_defect_rate
    FROM statistics_daily
    WHERE stat_date = target_date
    LIMIT 1;

    -- 임계값 조회
    SELECT CAST(config_value AS DECIMAL(5,2)) INTO threshold
    FROM system_config
    WHERE config_key = 'alert_defect_rate_threshold'
    LIMIT 1;

    -- 불량률이 임계값 초과 시 알람 생성
    IF current_defect_rate > threshold THEN
        INSERT INTO alerts (alert_type, severity, message, details)
        VALUES (
            'defect_rate_high',
            'high',
            CONCAT('불량률이 임계값을 초과했습니다: ', current_defect_rate, '%'),
            JSON_OBJECT(
                'defect_rate', current_defect_rate,
                'threshold', threshold,
                'date', target_date
            )
        );
    END IF;
END$$

DELIMITER ;
```

---

## 트리거

### 검사 결과 삽입 시 시간별 통계 업데이트 ⭐ 이중 모델 융합 결과 기반

```sql
DELIMITER $$

CREATE TRIGGER after_inspection_insert
AFTER INSERT ON inspections
FOR EACH ROW
BEGIN
    DECLARE stat_hour DATETIME;

    -- 시간 단위로 반올림 (예: 2025-10-31 14:35:20 → 2025-10-31 14:00:00)
    SET stat_hour = DATE_FORMAT(NEW.inspection_time, '%Y-%m-%d %H:00:00');

    -- 시간별 통계 업데이트 (융합 결과 기반)
    INSERT INTO statistics_hourly (
        stat_datetime,
        total_inspections,
        normal_count,
        component_defect_count,
        solder_defect_count,
        discard_count
    ) VALUES (
        stat_hour,
        1,
        CASE WHEN NEW.fusion_decision = 'normal' THEN 1 ELSE 0 END,
        CASE WHEN NEW.fusion_decision = 'component_defect' THEN 1 ELSE 0 END,
        CASE WHEN NEW.fusion_decision = 'solder_defect' THEN 1 ELSE 0 END,
        CASE WHEN NEW.fusion_decision = 'discard' THEN 1 ELSE 0 END
    )
    ON DUPLICATE KEY UPDATE
        total_inspections = total_inspections + 1,
        normal_count = normal_count + CASE WHEN NEW.fusion_decision = 'normal' THEN 1 ELSE 0 END,
        component_defect_count = component_defect_count + CASE WHEN NEW.fusion_decision = 'component_defect' THEN 1 ELSE 0 END,
        solder_defect_count = solder_defect_count + CASE WHEN NEW.fusion_decision = 'solder_defect' THEN 1 ELSE 0 END,
        discard_count = discard_count + CASE WHEN NEW.fusion_decision = 'discard' THEN 1 ELSE 0 END,
        updated_at = CURRENT_TIMESTAMP;
END$$

DELIMITER ;
```

---

## 인덱스 최적화

### 복합 인덱스 ⭐ 이중 모델 융합 결과 기반

```sql
-- 날짜 범위 조회용 (융합 결과 기반)
CREATE INDEX idx_inspection_time_fusion
ON inspections (inspection_time, fusion_decision);

-- 불량 개수 기반 검색
CREATE INDEX idx_component_solder_counts
ON inspections (component_defect_count, solder_defect_count, inspection_time);

-- 성능 분석용
CREATE INDEX idx_inference_times
ON inspections (total_inference_time_ms, inspection_time);
```

---

## 데이터 정리 (자동 삭제)

### 오래된 로그 삭제 이벤트

```sql
-- 이벤트 스케줄러 활성화
SET GLOBAL event_scheduler = ON;

-- 90일 이상 된 시스템 로그 자동 삭제 (매일 새벽 2시)
CREATE EVENT IF NOT EXISTS delete_old_system_logs
ON SCHEDULE EVERY 1 DAY
STARTS '2025-10-23 02:00:00'
DO
    DELETE FROM system_logs
    WHERE created_at < DATE_SUB(NOW(), INTERVAL 90 DAY);

-- 설정된 기간 이상 된 불량 이미지 메타데이터 삭제
CREATE EVENT IF NOT EXISTS delete_old_defect_images
ON SCHEDULE EVERY 1 DAY
STARTS '2025-10-23 02:30:00'
DO
    DELETE di FROM defect_images di
    INNER JOIN inspections i ON di.inspection_id = i.id
    WHERE i.inspection_time < DATE_SUB(NOW(), INTERVAL (
        SELECT CAST(config_value AS UNSIGNED)
        FROM system_config
        WHERE config_key = 'max_image_retention_days'
    ) DAY);
```

---

## 백업 전략

### mysqldump 사용

```bash
#!/bin/bash
# backup_database.sh

DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_DIR="/home/backup/mysql"
DB_NAME="pcb_inspection"
DB_USER="root"
DB_PASS="your_password"

mkdir -p $BACKUP_DIR

# 전체 데이터베이스 백업
mysqldump -u$DB_USER -p$DB_PASS $DB_NAME > $BACKUP_DIR/pcb_inspection_$DATE.sql

# 7일 이상 된 백업 파일 삭제
find $BACKUP_DIR -name "*.sql" -mtime +7 -delete

echo "Backup completed: pcb_inspection_$DATE.sql"
```

cron 등록 (매일 새벽 1시):
```bash
crontab -e

# 추가
0 1 * * * /home/pi/backup_database.sh
```

---

## Python 연결 예제

### PyMySQL 사용 ⭐ 이중 모델 융합 결과 기반

```python
import pymysql
import json

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
        # 양면 동시 검사 결과 삽입 (이중 모델 융합)
        sql = """INSERT INTO inspections
                 (fusion_decision, fusion_severity_level,
                  component_defects, component_defect_count, component_inference_time_ms,
                  solder_defects, solder_defect_count, solder_inference_time_ms,
                  total_inference_time_ms,
                  left_image_path, right_image_path,
                  gpio_pin, gpio_duration_ms)
                 VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s)"""

        # Component Model 결과
        component_defects = json.dumps([
            {"type": "missing_component", "confidence": 0.95, "bbox": [120, 85, 150, 110], "class_name": "resistor"}
        ])

        # Solder Model 결과
        solder_defects = json.dumps([
            {"type": "cold_joint", "confidence": 0.92, "bbox": [310, 220, 335, 245], "class_name": "solder_joint"}
        ])

        cursor.execute(sql, (
            'component_defect',  # fusion_decision
            2,                    # fusion_severity_level
            component_defects,    # component_defects (JSON)
            1,                    # component_defect_count
            65.5,                 # component_inference_time_ms
            solder_defects,       # solder_defects (JSON)
            1,                    # solder_defect_count
            45.2,                 # solder_inference_time_ms
            85.3,                 # total_inference_time_ms
            '/images/left/pcb_001.jpg',   # left_image_path
            '/images/right/pcb_001.jpg',  # right_image_path
            17,                   # gpio_pin (component_defect = GPIO 17)
            500                   # gpio_duration_ms
        ))
    conn.commit()

    # 검사 이력 조회 (융합 결과 기반)
    with conn.cursor() as cursor:
        sql = """SELECT id, fusion_decision,
                        component_defect_count, solder_defect_count,
                        total_inference_time_ms, inspection_time
                 FROM inspections
                 ORDER BY inspection_time DESC
                 LIMIT 10"""
        cursor.execute(sql)
        results = cursor.fetchall()
        for row in results:
            print(f"ID: {row['id']}, Decision: {row['fusion_decision']}, "
                  f"Component: {row['component_defect_count']}, "
                  f"Solder: {row['solder_defect_count']}, "
                  f"Time: {row['total_inference_time_ms']}ms")

finally:
    conn.close()
```

---

## C# 연결 예제

### MySql.Data 사용 ⭐ 이중 모델 융합 결과 기반

```csharp
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Collections.Generic;

string connStr = "server=localhost;user=root;database=pcb_inspection;password=your_password;";
MySqlConnection conn = new MySqlConnection(connStr);

try
{
    conn.Open();

    // Component Model 결과 (JSON)
    var componentDefects = new List<object>
    {
        new { type = "missing_component", confidence = 0.95, bbox = new[] { 120, 85, 150, 110 }, class_name = "resistor" }
    };

    // Solder Model 결과 (JSON)
    var solderDefects = new List<object>
    {
        new { type = "cold_joint", confidence = 0.92, bbox = new[] { 310, 220, 335, 245 }, class_name = "solder_joint" }
    };

    // 양면 동시 검사 결과 삽입 (이중 모델 융합)
    string sql = @"INSERT INTO inspections
                   (fusion_decision, fusion_severity_level,
                    component_defects, component_defect_count, component_inference_time_ms,
                    solder_defects, solder_defect_count, solder_inference_time_ms,
                    total_inference_time_ms,
                    left_image_path, right_image_path,
                    gpio_pin, gpio_duration_ms)
                   VALUES (@fusion_decision, @fusion_severity_level,
                           @component_defects, @component_defect_count, @component_inference_time_ms,
                           @solder_defects, @solder_defect_count, @solder_inference_time_ms,
                           @total_inference_time_ms,
                           @left_image_path, @right_image_path,
                           @gpio_pin, @gpio_duration_ms)";

    MySqlCommand cmd = new MySqlCommand(sql, conn);
    cmd.Parameters.AddWithValue("@fusion_decision", "component_defect");
    cmd.Parameters.AddWithValue("@fusion_severity_level", 2);
    cmd.Parameters.AddWithValue("@component_defects", JsonConvert.SerializeObject(componentDefects));
    cmd.Parameters.AddWithValue("@component_defect_count", 1);
    cmd.Parameters.AddWithValue("@component_inference_time_ms", 65.5);
    cmd.Parameters.AddWithValue("@solder_defects", JsonConvert.SerializeObject(solderDefects));
    cmd.Parameters.AddWithValue("@solder_defect_count", 1);
    cmd.Parameters.AddWithValue("@solder_inference_time_ms", 45.2);
    cmd.Parameters.AddWithValue("@total_inference_time_ms", 85.3);
    cmd.Parameters.AddWithValue("@left_image_path", "/images/left/pcb_001.jpg");
    cmd.Parameters.AddWithValue("@right_image_path", "/images/right/pcb_001.jpg");
    cmd.Parameters.AddWithValue("@gpio_pin", 17);  // component_defect = GPIO 17
    cmd.Parameters.AddWithValue("@gpio_duration_ms", 500);

    cmd.ExecuteNonQuery();

    // 검사 이력 조회 (융합 결과 기반)
    string selectSql = @"SELECT id, fusion_decision,
                                component_defect_count, solder_defect_count,
                                total_inference_time_ms, inspection_time
                         FROM inspections
                         ORDER BY inspection_time DESC
                         LIMIT 10";

    MySqlCommand selectCmd = new MySqlCommand(selectSql, conn);
    using (MySqlDataReader reader = selectCmd.ExecuteReader())
    {
        while (reader.Read())
        {
            Console.WriteLine($"ID: {reader["id"]}, " +
                            $"Decision: {reader["fusion_decision"]}, " +
                            $"Component: {reader["component_defect_count"]}, " +
                            $"Solder: {reader["solder_defect_count"]}, " +
                            $"Time: {reader["total_inference_time_ms"]}ms");
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

**중요**: 프로덕션 환경에서는 root 계정을 직접 사용하지 말고, 각 서비스별로 전용 계정을 생성하여 최소 권한 원칙을 적용해야 합니다.

#### 1. Flask 서버용 MySQL 사용자 생성 (읽기/쓰기)

Flask 서버는 검사 결과를 데이터베이스에 저장하고 조회할 수 있어야 합니다.

```sql
-- Flask 서버 전용 사용자 생성
CREATE USER 'flask_server'@'100.64.1.1' IDENTIFIED BY 'STRONG_PASSWORD_HERE';

-- pcb_inspection 데이터베이스에 대한 권한 부여
GRANT SELECT, INSERT, UPDATE ON pcb_inspection.* TO 'flask_server'@'100.64.1.1';

-- 변경사항 적용
FLUSH PRIVILEGES;
```

**참고**:
- `100.64.1.1`은 Flask 서버의 IP 주소
- 실제 사용 시 `STRONG_PASSWORD_HERE`를 강력한 비밀번호로 변경

#### 2. C# WinForms 모니터링 앱용 MySQL 사용자 생성 (읽기 전용)

모니터링 앱은 검사 이력 및 통계 조회만 필요하므로 읽기 전용 권한 부여.

```sql
-- C# WinForms 앱 전용 사용자 생성
CREATE USER 'winforms_app'@'100.64.1.5' IDENTIFIED BY 'STRONG_PASSWORD_HERE';

-- 읽기 전용 권한 부여
GRANT SELECT ON pcb_inspection.* TO 'winforms_app'@'100.64.1.5';

-- 변경사항 적용
FLUSH PRIVILEGES;
```

**참고**:
- `100.64.1.5`은 Windows PC의 IP 주소
- 권한 수준별 사용자 (Admin/Operator/Viewer)는 C# 앱 내부에서 관리

#### 3. 관리자용 사용자 (전체 권한)

데이터베이스 관리 및 백업/복구를 위한 관리자 계정.

```sql
-- 관리자 계정 생성
CREATE USER 'pcb_admin'@'localhost' IDENTIFIED BY 'ADMIN_STRONG_PASSWORD_HERE';

-- 모든 권한 부여 (백업, 복구, 스키마 변경 등)
GRANT ALL PRIVILEGES ON pcb_inspection.* TO 'pcb_admin'@'localhost';

-- 변경사항 적용
FLUSH PRIVILEGES;
```

#### 4. 사용자 계정 확인

```sql
-- 현재 생성된 사용자 목록 확인
SELECT User, Host FROM mysql.user WHERE User LIKE 'flask%' OR User LIKE 'winforms%' OR User LIKE 'pcb%';

-- 특정 사용자의 권한 확인
SHOW GRANTS FOR 'flask_server'@'100.64.1.1';
SHOW GRANTS FOR 'winforms_app'@'100.64.1.5';
```

---

### 네트워크 보안 설정

#### 1. MySQL 외부 접속 허용 설정

기본적으로 MySQL은 localhost만 허용하므로, 네트워크 접속을 활성화해야 합니다.

```bash
# MySQL 설정 파일 편집 (Ubuntu/Debian)
sudo nano /etc/mysql/mysql.conf.d/mysqld.cnf

# [mysqld] 섹션에서 bind-address 수정
# 변경 전: bind-address = 127.0.0.1
# 변경 후: bind-address = 0.0.0.0  # 모든 IP 허용 (또는 100.64.1.1 등 특정 IP만)

# MySQL 재시작
sudo systemctl restart mysql
```

#### 2. 방화벽 설정 (Ubuntu/Debian)

```bash
# MySQL 포트 (3306) 개방 - 특정 IP만 허용 권장
sudo ufw allow from 100.64.1.1 to any port 3306 comment 'Flask Server'
sudo ufw allow from 100.64.1.5 to any port 3306 comment 'Windows PC'

# 방화벽 규칙 확인
sudo ufw status
```

---

### 비밀번호 정책

**권장 비밀번호 규칙**:
- 최소 12자 이상
- 대소문자, 숫자, 특수문자 조합
- 주기적 변경 (3-6개월)
- 기본 비밀번호 사용 금지

**비밀번호 변경**:
```sql
-- 사용자 비밀번호 변경
ALTER USER 'flask_server'@'100.64.1.1' IDENTIFIED BY 'NEW_STRONG_PASSWORD';
FLUSH PRIVILEGES;
```

---

### 백업 및 복구 전략

#### 1. 정기 백업 (cron 사용)

```bash
# 백업 스크립트 생성
sudo nano /home/pcb_user/backup_mysql.sh
```

내용:
```bash
#!/bin/bash
BACKUP_DIR="/home/pcb_user/mysql_backups"
DATE=$(date +%Y%m%d_%H%M%S)
mkdir -p $BACKUP_DIR

# 데이터베이스 백업
mysqldump -u pcb_admin -p'ADMIN_PASSWORD' pcb_inspection > $BACKUP_DIR/pcb_inspection_$DATE.sql

# 7일 이상 된 백업 파일 자동 삭제
find $BACKUP_DIR -name "*.sql" -mtime +7 -delete

echo "Backup completed: pcb_inspection_$DATE.sql"
```

실행 권한 부여:
```bash
chmod +x /home/pcb_user/backup_mysql.sh
```

cron 등록 (매일 새벽 2시 백업):
```bash
crontab -e

# 추가
0 2 * * * /home/pcb_user/backup_mysql.sh >> /home/pcb_user/backup.log 2>&1
```

#### 2. 복구 방법

```bash
# SQL 파일로부터 복구
mysql -u pcb_admin -p pcb_inspection < /home/pcb_user/mysql_backups/pcb_inspection_20251023_020000.sql
```

---

## 관련 문서

이 데이터베이스 설계는 다음 문서들과 연계되어 있습니다:

1. **PCB_Defect_Detection_Project.md** - 전체 시스템 아키텍처 및 프로젝트 개요
2. **Flask_Server_Setup.md** - Flask 서버에서 MySQL 연동 (PyMySQL)
3. **CSharp_WinForms_Guide.md** - C# WinForms에서 MySQL 연동 (MySql.Data)
4. **RaspberryPi_Setup.md** - 라즈베리파이 클라이언트 (간접적으로 Flask 서버를 통해 연동)

각 문서에서 이 데이터베이스 스키마를 참조하여 시스템 통합을 구현합니다.

---

**작성일**: 2025-10-28
**최종 수정일**: 2025-10-31
**버전**: 2.0 ⭐ (이중 모델 아키텍처)
**데이터베이스**: MySQL 8.0
**문자 인코딩**: UTF-8 (utf8mb4)
**주요 변경사항**:
- **2.0 (2025-10-31)**: 이중 YOLO 모델 아키텍처 적용
  - inspections 테이블 완전 재설계: 양면 동시 검사 결과 저장
  - 융합 결과 기반 통계 (fusion_decision: normal/component_defect/solder_defect/discard)
  - Component Model (부품 검출) + Solder Model (납땜 검출) 결과 분리 저장
  - JSON 필드 추가: component_defects, solder_defects
  - 추론 시간 필드 추가: component_inference_time_ms, solder_inference_time_ms, total_inference_time_ms
  - 이미지 경로 분리: left_image_path (부품면), right_image_path (납땜면)
  - 통계 테이블, 뷰, 프로시저, 트리거 모두 융합 결과 기반으로 업데이트
  - Python/C# 연결 예제 업데이트 (이중 모델 데이터 삽입/조회)
- **1.1 (2025-10-23)**: 보안 설정 섹션 추가 (사용자 계정 관리, 권한 설정)
  - 네트워크 보안 및 방화벽 설정 추가
  - 백업 및 복구 전략 추가
