-- =====================================================
-- PCB 불량 검사 시스템 데이터베이스 스키마 v3.1 (Extended for WinForms)
-- Product Verification Architecture + User Management + OHT + Alerts
-- =====================================================
-- 작성일: 2025-11-28 (v3.0)
-- 확장일: 2025-11-28 (v3.1)
-- 설명: 시리얼 넘버 + QR 코드 기반 제품 식별 후 부품 위치 검증 시스템
-- 주요 변경사항 (v3.0):
--   - 이중 YOLO 모델 아키텍처 → 제품별 부품 배치 검증
--   - 4단계 판정: normal/missing/position_error/discard
--   - 시간별/일별/월별 집계 테이블 추가 (10년 보관)
--   - 자동 데이터 정리 (Event Scheduler)
-- 추가 변경사항 (v3.1):
--   - 사용자 관리 시스템 (users, user_logs)
--   - OHT 운영 관리 (oht_operations, box_status)
--   - 알림 시스템 (alerts)
--   - 불량 클래스별 상세 정보 (defect_details)
--   - inspections 테이블 확장 (user_id, notes)
-- =====================================================

-- 데이터베이스 생성 (존재하지 않을 경우)
CREATE DATABASE IF NOT EXISTS pcb_inspection
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE pcb_inspection;

-- =====================================================
-- 1. 제품 정보 테이블 (products)
-- =====================================================
-- 설명: 제품 코드별 기본 정보 (3개 제품 타입)
-- =====================================================

DROP TABLE IF EXISTS products;

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


-- =====================================================
-- 2. 제품별 부품 배치 기준 테이블 (product_components)
-- =====================================================
-- 설명: 제품별 정상 부품 배치 위치 정보 (기준 데이터)
-- =====================================================

DROP TABLE IF EXISTS product_components;

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


-- =====================================================
-- 3. 메인 검사 이력 테이블 (inspections)
-- =====================================================
-- 설명: 모든 PCB 검사 데이터 (10년 보관)
-- =====================================================

DROP TABLE IF EXISTS inspections;

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

    -- 사용자 정보 (v3.1 추가)
    user_id INT NULL COMMENT '검사 확인 사용자 (FK)',
    notes TEXT NULL COMMENT '추가 메모',

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


-- =====================================================
-- 4. 시간별 집계 테이블 (inspection_summary_hourly)
-- =====================================================
-- 설명: 시간 단위 집계 (제품별, 10년 보관)
-- =====================================================

DROP TABLE IF EXISTS inspection_summary_hourly;

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

    -- 불량률
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


-- =====================================================
-- 5. 일별 집계 테이블 (inspection_summary_daily)
-- =====================================================
-- 설명: 일 단위 집계 (제품별, 10년 보관)
-- =====================================================

DROP TABLE IF EXISTS inspection_summary_daily;

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

    -- 불량률
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


-- =====================================================
-- 6. 월별 집계 테이블 (inspection_summary_monthly)
-- =====================================================
-- 설명: 월 단위 집계 (제품별, 10년 보관)
-- =====================================================

DROP TABLE IF EXISTS inspection_summary_monthly;

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

    -- 불량률
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


-- =====================================================
-- 7. 사용자 계정 테이블 (users) - v3.1 추가
-- =====================================================
-- 설명: 사용자 로그인 및 권한 관리
-- =====================================================

DROP TABLE IF EXISTS users;

CREATE TABLE users (
    id INT AUTO_INCREMENT PRIMARY KEY COMMENT '사용자 ID',
    username VARCHAR(50) UNIQUE NOT NULL COMMENT '사용자명 (로그인 ID)',
    password_hash VARCHAR(255) NOT NULL COMMENT '비밀번호 해시',
    full_name VARCHAR(100) NOT NULL COMMENT '전체 이름',
    role ENUM('Admin', 'Operator', 'Viewer') NOT NULL DEFAULT 'Viewer' COMMENT '권한 레벨',
    is_active BOOLEAN DEFAULT TRUE COMMENT '계정 활성화 여부',
    last_login DATETIME NULL COMMENT '마지막 로그인 시간',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP COMMENT '계정 생성일',
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '마지막 수정일',

    INDEX idx_username (username),
    INDEX idx_role (role),
    INDEX idx_is_active (is_active)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='사용자 계정 테이블 (로그인 및 권한 관리)';


-- =====================================================
-- 8. 사용자 활동 로그 테이블 (user_logs) - v3.1 추가
-- =====================================================
-- 설명: 감사 추적 (Audit Trail) 및 사용자 활동 이력
-- =====================================================

DROP TABLE IF EXISTS user_logs;

CREATE TABLE user_logs (
    id BIGINT AUTO_INCREMENT PRIMARY KEY COMMENT '로그 ID',
    user_id INT NOT NULL COMMENT '사용자 ID (FK)',
    username VARCHAR(50) NOT NULL COMMENT '사용자명 (중복 저장)',
    user_role ENUM('Admin', 'Operator', 'Viewer') NOT NULL COMMENT '사용자 권한 (중복 저장)',
    action_type ENUM('Login', 'Logout', 'CreateUser', 'UpdateUser', 'DeleteUser',
                     'ResetPassword', 'CallOHT', 'ExportData', 'ViewInspection',
                     'ChangeSettings', 'Other') NOT NULL COMMENT '활동 유형',
    action_description VARCHAR(255) NOT NULL COMMENT '활동 설명',
    ip_address VARCHAR(50) NULL COMMENT 'IP 주소',
    user_agent VARCHAR(255) NULL COMMENT 'User Agent',
    details TEXT NULL COMMENT '상세 정보 (JSON)',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP COMMENT '활동 시간',

    FOREIGN KEY (user_id) REFERENCES users(id)
        ON DELETE CASCADE
        ON UPDATE CASCADE,

    INDEX idx_user_id (user_id),
    INDEX idx_action_type (action_type),
    INDEX idx_created_at (created_at),
    INDEX idx_user_action (user_id, action_type, created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='사용자 활동 로그 테이블 (감사 추적)';


-- =====================================================
-- 9. 시스템 알림 테이블 (alerts) - v3.1 추가
-- =====================================================
-- 설명: 시스템 경고 및 알림 관리
-- =====================================================

DROP TABLE IF EXISTS alerts;

CREATE TABLE alerts (
    id INT AUTO_INCREMENT PRIMARY KEY COMMENT '알림 ID',
    alert_type ENUM('DefectRateHigh', 'SystemError', 'CameraOffline',
                    'ServerOffline', 'BoxFull') NOT NULL COMMENT '알림 유형',
    severity ENUM('Low', 'Medium', 'High', 'Critical') NOT NULL DEFAULT 'Medium' COMMENT '심각도',
    message VARCHAR(255) NOT NULL COMMENT '알림 메시지',
    details TEXT NULL COMMENT '상세 정보 (JSON)',
    is_resolved BOOLEAN DEFAULT FALSE COMMENT '해결 여부',
    resolved_at DATETIME NULL COMMENT '해결 시간',
    resolved_by INT NULL COMMENT '해결한 사용자 (FK)',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP COMMENT '알림 생성 시간',

    FOREIGN KEY (resolved_by) REFERENCES users(id)
        ON DELETE SET NULL
        ON UPDATE CASCADE,

    INDEX idx_alert_type (alert_type),
    INDEX idx_severity (severity),
    INDEX idx_is_resolved (is_resolved),
    INDEX idx_created_at (created_at),
    INDEX idx_alert_status (alert_type, is_resolved, severity)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='시스템 알림 테이블';


-- =====================================================
-- 10. OHT 운영 이력 테이블 (oht_operations) - v3.1 추가
-- =====================================================
-- 설명: OHT (Overhead Hoist Transport) 작업 이력 관리
-- =====================================================

DROP TABLE IF EXISTS oht_operations;

CREATE TABLE oht_operations (
    id INT AUTO_INCREMENT PRIMARY KEY COMMENT 'OHT 작업 ID',
    operation_id VARCHAR(36) UNIQUE NOT NULL COMMENT 'UUID',
    category ENUM('normal', 'missing', 'position_error') NOT NULL COMMENT 'PCB 카테고리',
    user_id INT NULL COMMENT '호출한 사용자 (FK, NULL=자동 호출)',
    user_role VARCHAR(20) NULL COMMENT '사용자 권한',
    is_auto BOOLEAN DEFAULT FALSE COMMENT '자동 호출 여부',
    trigger_reason VARCHAR(50) NULL COMMENT '호출 사유 (box_full 등)',
    status ENUM('Pending', 'Processing', 'Completed', 'Failed') DEFAULT 'Pending' COMMENT '작업 상태',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP COMMENT '호출 시간',
    started_at DATETIME NULL COMMENT '시작 시간',
    completed_at DATETIME NULL COMMENT '완료 시간',
    pcb_count INT DEFAULT 0 COMMENT '수거한 PCB 개수',
    success BOOLEAN NULL COMMENT '성공 여부',
    error_message TEXT NULL COMMENT '오류 메시지',
    execution_time_seconds DOUBLE NULL COMMENT '실행 시간 (초)',

    FOREIGN KEY (user_id) REFERENCES users(id)
        ON DELETE SET NULL
        ON UPDATE CASCADE,

    INDEX idx_operation_id (operation_id),
    INDEX idx_category (category),
    INDEX idx_status (status),
    INDEX idx_created_at (created_at),
    INDEX idx_user_id (user_id),
    INDEX idx_oht_status (category, status, created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='OHT 운영 이력 테이블';


-- =====================================================
-- 11. 박스 상태 관리 테이블 (box_status) - v3.1 추가
-- =====================================================
-- 설명: 3개 분류 박스 (normal/missing/position_error) 슬롯 상태 관리
-- =====================================================

DROP TABLE IF EXISTS box_status;

CREATE TABLE box_status (
    id INT AUTO_INCREMENT PRIMARY KEY COMMENT '박스 ID',
    box_id VARCHAR(20) UNIQUE NOT NULL COMMENT '박스 식별자',
    category ENUM('normal', 'missing', 'position_error') NOT NULL COMMENT 'PCB 카테고리',
    current_slot INT DEFAULT 0 COMMENT '현재 사용 중인 슬롯 번호 (0-4)',
    max_slots INT DEFAULT 5 COMMENT '최대 슬롯 개수',
    is_full BOOLEAN DEFAULT FALSE COMMENT '박스 가득참 여부',
    total_pcb_count INT DEFAULT 0 COMMENT '총 PCB 개수',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP COMMENT '생성일',
    last_updated TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '마지막 업데이트',

    INDEX idx_box_id (box_id),
    INDEX idx_category (category),
    INDEX idx_is_full (is_full)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='박스 상태 관리 테이블 (3개 박스 × 5개 슬롯)';


-- =====================================================
-- 12. 불량 클래스별 상세 정보 테이블 (defect_details) - v3.1 추가
-- =====================================================
-- 설명: YOLO 검출 클래스별 통계 (inspections의 상세 분석)
-- =====================================================

DROP TABLE IF EXISTS defect_details;

CREATE TABLE defect_details (
    id BIGINT AUTO_INCREMENT PRIMARY KEY COMMENT '상세 정보 ID',
    inspection_id BIGINT NOT NULL COMMENT '검사 ID (FK)',
    class_name VARCHAR(100) NOT NULL COMMENT 'YOLO 클래스명',
    count INT NOT NULL COMMENT '검출된 객체 개수',
    avg_confidence DECIMAL(5,4) NULL COMMENT '평균 신뢰도 (0.0000~1.0000)',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP COMMENT '생성일',

    FOREIGN KEY (inspection_id) REFERENCES inspections(id)
        ON DELETE CASCADE
        ON UPDATE CASCADE,

    INDEX idx_inspection_id (inspection_id),
    INDEX idx_class_name (class_name),
    INDEX idx_inspection_class (inspection_id, class_name)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='불량 클래스별 상세 정보 테이블';


-- =====================================================
-- inspections 테이블에 외래 키 추가 (v3.1)
-- =====================================================
-- users 테이블 생성 후 외래 키 제약조건 추가
-- =====================================================

ALTER TABLE inspections
    ADD CONSTRAINT fk_inspections_user
    FOREIGN KEY (user_id) REFERENCES users(id)
    ON DELETE SET NULL
    ON UPDATE CASCADE;


-- =====================================================
-- 샘플 데이터 삽입
-- =====================================================

-- 제품 정보 샘플 (3개 제품)
INSERT INTO products (product_code, product_name, description, serial_prefix, component_count, qr_url_template) VALUES
('FT', 'Fast Type PCB', '고속 처리용 PCB (25개 부품)', 'MBFT', 25, 'http://localhost:8080/product/{serial}'),
('RS', 'Reliable Stable PCB', '안정성 중심 PCB (30개 부품)', 'MBRS', 30, 'http://localhost:8080/product/{serial}'),
('BC', 'Budget Compact PCB', '저가형 소형 PCB (18개 부품)', 'MBBC', 18, 'http://localhost:8080/product/{serial}');

-- v3.1 샘플 데이터 추가
-- 기본 관리자 계정 (username: admin, password: admin123)
-- 비밀번호 해시는 bcrypt 알고리즘 사용 (실제 운영 시 변경 필요)
INSERT INTO users (username, password_hash, full_name, role, is_active) VALUES
('admin', '$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', '시스템 관리자', 'Admin', TRUE);
-- 참고: 위 해시는 'admin123'의 bcrypt 해시값 (실제 환경에서는 C# 앱에서 생성)

-- 박스 상태 초기화 (3개 박스 × 5개 슬롯)
INSERT INTO box_status (box_id, category, current_slot, max_slots, is_full, total_pcb_count) VALUES
('NORMAL', 'normal', 0, 5, FALSE, 0),
('MISSING', 'missing', 0, 5, FALSE, 0),
('POSITION_ERROR', 'position_error', 0, 5, FALSE, 0);

-- =====================================================
-- 완료 메시지
-- =====================================================
-- v3.1 스키마 생성 완료
-- 테이블: 12개 (v3.0 6개 + v3.1 6개)
--   v3.0: products, product_components, inspections, inspection_summary_hourly/daily/monthly
--   v3.1: users, user_logs, alerts, oht_operations, box_status, defect_details
-- 다음 단계: triggers_v3.0.sql 실행하여 자동 집계 트리거 설정
-- =====================================================
