-- ========================================
-- PCB 불량 검사 시스템 - 데이터베이스 스키마 (완전 버전)
-- ========================================
-- 버전: 2.0
-- 마지막 업데이트: 2025-10-31
-- 실행 방법: MySQL Workbench에서 root 또는 pcb_admin 계정으로 실행
-- ========================================

-- 데이터베이스 생성
CREATE DATABASE IF NOT EXISTS pcb_inspection
CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci;

USE pcb_inspection;

-- ========================================
-- 기존 테이블 삭제 (외래 키 제약 조건 순서 고려)
-- ========================================

DROP TABLE IF EXISTS user_logs;
DROP TABLE IF EXISTS oht_operations;
DROP TABLE IF EXISTS defect_images;
DROP TABLE IF EXISTS alerts;
DROP TABLE IF EXISTS statistics_hourly;
DROP TABLE IF EXISTS statistics_daily;
DROP TABLE IF EXISTS system_logs;
DROP TABLE IF EXISTS system_config;
DROP TABLE IF EXISTS box_status;
DROP TABLE IF EXISTS inspections;
DROP TABLE IF EXISTS users;

-- ========================================
-- 1. inspections (검사 결과 이력)
-- ========================================

CREATE TABLE IF NOT EXISTS inspections (
    id INT AUTO_INCREMENT PRIMARY KEY,
    camera_id VARCHAR(20) NOT NULL COMMENT '카메라 ID (left/right)',
    defect_type VARCHAR(50) NOT NULL COMMENT '불량 유형 (정상/부품불량/납땜불량/폐기)',
    confidence DECIMAL(5,4) NOT NULL COMMENT '신뢰도 (0.0000 ~ 1.0000)',
    inspection_time DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '검사 시간',
    image_path VARCHAR(500) NULL COMMENT '불량 이미지 파일 경로',
    boxes JSON NULL COMMENT '바운딩 박스 정보 (JSON)',
    gpio_pin INT NULL COMMENT '활성화된 GPIO 핀 번호',
    gpio_duration_ms INT NULL COMMENT 'GPIO 신호 지속 시간 (밀리초)',
    user_id INT NULL COMMENT '작업자 ID',
    notes TEXT NULL COMMENT '비고',

    INDEX idx_inspection_time (inspection_time),
    INDEX idx_defect_type (defect_type),
    INDEX idx_camera_id (camera_id),
    INDEX idx_user_id (user_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='PCB 검사 결과 이력';

-- ========================================
-- 2. defect_images (불량 이미지 메타데이터)
-- ========================================

CREATE TABLE IF NOT EXISTS defect_images (
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

-- ========================================
-- 3. statistics_daily (일별 통계)
-- ========================================

CREATE TABLE IF NOT EXISTS statistics_daily (
    id INT AUTO_INCREMENT PRIMARY KEY,
    stat_date DATE NOT NULL UNIQUE COMMENT '통계 날짜',
    total_inspections INT NOT NULL DEFAULT 0 COMMENT '총 검사 수',
    normal_count INT NOT NULL DEFAULT 0 COMMENT '정상 개수',
    component_defect_count INT NOT NULL DEFAULT 0 COMMENT '부품 불량 개수',
    solder_defect_count INT NOT NULL DEFAULT 0 COMMENT '납땜 불량 개수',
    discard_count INT NOT NULL DEFAULT 0 COMMENT '폐기 개수',
    defect_rate DECIMAL(5,2) GENERATED ALWAYS AS (
        CASE
            WHEN total_inspections > 0 THEN
                (component_defect_count + solder_defect_count + discard_count) * 100.0 / total_inspections
            ELSE 0
        END
    ) STORED COMMENT '불량률 (%, 자동 계산)',
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    INDEX idx_stat_date (stat_date)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='일별 검사 통계';

-- ========================================
-- 4. statistics_hourly (시간별 통계)
-- ========================================

CREATE TABLE IF NOT EXISTS statistics_hourly (
    id INT AUTO_INCREMENT PRIMARY KEY,
    stat_datetime DATETIME NOT NULL COMMENT '통계 시간 (YYYY-MM-DD HH:00:00)',
    total_inspections INT NOT NULL DEFAULT 0,
    normal_count INT NOT NULL DEFAULT 0,
    component_defect_count INT NOT NULL DEFAULT 0,
    solder_defect_count INT NOT NULL DEFAULT 0,
    discard_count INT NOT NULL DEFAULT 0,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    UNIQUE KEY uk_stat_datetime (stat_datetime),
    INDEX idx_stat_datetime (stat_datetime)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='시간별 검사 통계';

-- ========================================
-- 5. system_logs (시스템 로그)
-- ========================================

CREATE TABLE IF NOT EXISTS system_logs (
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

-- ========================================
-- 6. system_config (시스템 설정)
-- ========================================

CREATE TABLE IF NOT EXISTS system_config (
    id INT AUTO_INCREMENT PRIMARY KEY,
    config_key VARCHAR(100) NOT NULL UNIQUE COMMENT '설정 키',
    config_value TEXT NOT NULL COMMENT '설정 값',
    description VARCHAR(500) NULL COMMENT '설명',
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    updated_by INT NULL COMMENT '수정한 사용자 ID',

    INDEX idx_config_key (config_key)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='시스템 설정';

-- ========================================
-- 7. users (사용자/작업자)
-- ========================================

CREATE TABLE IF NOT EXISTS users (
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

-- ========================================
-- 8. alerts (알람/알림)
-- ========================================

CREATE TABLE IF NOT EXISTS alerts (
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

-- ========================================
-- 9. box_status (로봇팔 박스 상태 관리)
-- ========================================

CREATE TABLE IF NOT EXISTS box_status (
    id INT AUTO_INCREMENT PRIMARY KEY,

    -- 박스 정보
    box_id VARCHAR(20) NOT NULL UNIQUE COMMENT '박스 ID (NORMAL, COMPONENT_DEFECT, SOLDER_DEFECT)',
    category VARCHAR(50) NOT NULL COMMENT '분류 카테고리 (normal/component_defect/solder_defect)',

    -- 슬롯 상태
    current_slot INT NOT NULL DEFAULT 0 COMMENT '현재 사용 중인 슬롯 번호 (0-4, 수평 5슬롯)',
    max_slots INT NOT NULL DEFAULT 5 COMMENT '최대 슬롯 개수 (5개, 수평 배치)',
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
COMMENT='로봇팔 박스 슬롯 상태 관리 테이블 (3개 박스 × 5개 슬롯 = 15개 슬롯, 폐기는 슬롯 관리 안 함)';

-- ========================================
-- 10. oht_operations (OHT 운영 이력)
-- ========================================

CREATE TABLE IF NOT EXISTS oht_operations (
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

-- ========================================
-- 11. user_logs (사용자 활동 로그)
-- ========================================

CREATE TABLE IF NOT EXISTS user_logs (
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

-- ========================================
-- 초기 데이터 삽입
-- ========================================

-- 시스템 설정 기본값
INSERT INTO system_config (config_key, config_value, description) VALUES
('server_url', 'http://100.64.1.1:5000', 'Flask 서버 URL'),
('fps', '10', '카메라 FPS'),
('jpeg_quality', '85', 'JPEG 압축 품질'),
('defect_threshold', '0.70', '불량 판정 임계값 (신뢰도)'),
('gpio_duration_ms', '500', 'GPIO 신호 지속 시간 (밀리초)'),
('max_image_retention_days', '90', '불량 이미지 보관 기간 (일)'),
('alert_defect_rate_threshold', '10.0', '알람 발생 불량률 임계값 (%)');

-- 기본 사용자 생성 (비밀번호: admin123, 실제로는 해시 사용)
INSERT INTO users (username, password_hash, full_name, role) VALUES
('admin', '$2b$12$examplehashedpassword', '관리자', 'admin'),
('operator1', '$2b$12$examplehashedpassword', '작업자1', 'operator'),
('viewer1', '$2b$12$examplehashedpassword', '조회자1', 'viewer');

-- 박스 상태 초기화 (3개 박스, DISCARD는 제외)
INSERT INTO box_status (box_id, category, max_slots) VALUES
    ('NORMAL', 'normal', 5),
    ('COMPONENT_DEFECT', 'component_defect', 5),
    ('SOLDER_DEFECT', 'solder_defect', 5);

-- ========================================
-- 뷰 정의
-- ========================================

-- 실시간 통계 뷰
CREATE OR REPLACE VIEW v_realtime_statistics AS
SELECT
    DATE(inspection_time) AS stat_date,
    HOUR(inspection_time) AS stat_hour,
    COUNT(*) AS total_inspections,
    SUM(CASE WHEN defect_type = '정상' THEN 1 ELSE 0 END) AS normal_count,
    SUM(CASE WHEN defect_type = '부품불량' THEN 1 ELSE 0 END) AS component_defect_count,
    SUM(CASE WHEN defect_type = '납땜불량' THEN 1 ELSE 0 END) AS solder_defect_count,
    SUM(CASE WHEN defect_type = '폐기' THEN 1 ELSE 0 END) AS discard_count,
    ROUND(
        (SUM(CASE WHEN defect_type != '정상' THEN 1 ELSE 0 END) * 100.0 / COUNT(*)),
        2
    ) AS defect_rate
FROM inspections
WHERE inspection_time >= CURDATE()
GROUP BY stat_date, stat_hour
ORDER BY stat_date DESC, stat_hour DESC;

-- ========================================
-- 저장 프로시저
-- ========================================

-- 1. 일별 통계 업데이트
DELIMITER $$

DROP PROCEDURE IF EXISTS update_daily_statistics$$

CREATE PROCEDURE update_daily_statistics(IN target_date DATE)
BEGIN
    INSERT INTO statistics_daily (
        stat_date,
        total_inspections,
        normal_count,
        component_defect_count,
        solder_defect_count,
        discard_count
    )
    SELECT
        DATE(inspection_time) AS stat_date,
        COUNT(*) AS total_inspections,
        SUM(CASE WHEN defect_type = '정상' THEN 1 ELSE 0 END) AS normal_count,
        SUM(CASE WHEN defect_type = '부품불량' THEN 1 ELSE 0 END) AS component_defect_count,
        SUM(CASE WHEN defect_type = '납땜불량' THEN 1 ELSE 0 END) AS solder_defect_count,
        SUM(CASE WHEN defect_type = '폐기' THEN 1 ELSE 0 END) AS discard_count
    FROM inspections
    WHERE DATE(inspection_time) = target_date
    GROUP BY DATE(inspection_time)
    ON DUPLICATE KEY UPDATE
        total_inspections = VALUES(total_inspections),
        normal_count = VALUES(normal_count),
        component_defect_count = VALUES(component_defect_count),
        solder_defect_count = VALUES(solder_defect_count),
        discard_count = VALUES(discard_count),
        updated_at = CURRENT_TIMESTAMP;
END$$

-- 2. 불량률 알람 체크
DROP PROCEDURE IF EXISTS check_defect_rate_alert$$

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

-- ========================================
-- 트리거
-- ========================================

-- 검사 결과 삽입 시 시간별 통계 자동 업데이트
DELIMITER $$

DROP TRIGGER IF EXISTS after_inspection_insert$$

CREATE TRIGGER after_inspection_insert
AFTER INSERT ON inspections
FOR EACH ROW
BEGIN
    DECLARE stat_hour DATETIME;

    -- 시간 단위로 반올림 (예: 2025-10-31 14:35:20 → 2025-10-31 14:00:00)
    SET stat_hour = DATE_FORMAT(NEW.inspection_time, '%Y-%m-%d %H:00:00');

    -- 시간별 통계 업데이트
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
        CASE WHEN NEW.defect_type = '정상' THEN 1 ELSE 0 END,
        CASE WHEN NEW.defect_type = '부품불량' THEN 1 ELSE 0 END,
        CASE WHEN NEW.defect_type = '납땜불량' THEN 1 ELSE 0 END,
        CASE WHEN NEW.defect_type = '폐기' THEN 1 ELSE 0 END
    )
    ON DUPLICATE KEY UPDATE
        total_inspections = total_inspections + 1,
        normal_count = normal_count + CASE WHEN NEW.defect_type = '정상' THEN 1 ELSE 0 END,
        component_defect_count = component_defect_count + CASE WHEN NEW.defect_type = '부품불량' THEN 1 ELSE 0 END,
        solder_defect_count = solder_defect_count + CASE WHEN NEW.defect_type = '납땜불량' THEN 1 ELSE 0 END,
        discard_count = discard_count + CASE WHEN NEW.defect_type = '폐기' THEN 1 ELSE 0 END,
        updated_at = CURRENT_TIMESTAMP;
END$$

DELIMITER ;

-- ========================================
-- 인덱스 최적화 (복합 인덱스)
-- ========================================

-- 날짜 범위 조회용
DROP INDEX IF EXISTS idx_inspection_time_defect_type ON inspections;
CREATE INDEX idx_inspection_time_defect_type
ON inspections (inspection_time, defect_type);

-- 카메라별 검색용
DROP INDEX IF EXISTS idx_camera_defect_type ON inspections;
CREATE INDEX idx_camera_defect_type
ON inspections (camera_id, defect_type, inspection_time);

-- ========================================
-- 이벤트 스케줄러 설정 (자동 삭제)
-- ========================================

-- 이벤트 스케줄러 활성화
SET GLOBAL event_scheduler = ON;

-- 90일 이상 된 시스템 로그 자동 삭제 (매일 새벽 2시)
DROP EVENT IF EXISTS delete_old_system_logs;

CREATE EVENT IF NOT EXISTS delete_old_system_logs
ON SCHEDULE EVERY 1 DAY
STARTS CONCAT(CURDATE() + INTERVAL 1 DAY, ' 02:00:00')
DO
    DELETE FROM system_logs
    WHERE created_at < DATE_SUB(NOW(), INTERVAL 90 DAY);

-- 설정된 기간 이상 된 불량 이미지 메타데이터 삭제 (매일 새벽 2시 30분)
DROP EVENT IF EXISTS delete_old_defect_images;

CREATE EVENT IF NOT EXISTS delete_old_defect_images
ON SCHEDULE EVERY 1 DAY
STARTS CONCAT(CURDATE() + INTERVAL 1 DAY, ' 02:30:00')
DO
    DELETE di FROM defect_images di
    INNER JOIN inspections i ON di.inspection_id = i.id
    WHERE i.inspection_time < DATE_SUB(NOW(), INTERVAL (
        SELECT CAST(config_value AS UNSIGNED)
        FROM system_config
        WHERE config_key = 'max_image_retention_days'
    ) DAY);

-- ========================================
-- 테이블 확인 및 완료 메시지
-- ========================================

-- 모든 테이블 목록 표시
SHOW TABLES;

-- 각 테이블 구조 확인
SELECT 'inspections' AS 'Table', COUNT(*) AS 'Row Count' FROM inspections
UNION ALL
SELECT 'defect_images', COUNT(*) FROM defect_images
UNION ALL
SELECT 'statistics_daily', COUNT(*) FROM statistics_daily
UNION ALL
SELECT 'statistics_hourly', COUNT(*) FROM statistics_hourly
UNION ALL
SELECT 'system_logs', COUNT(*) FROM system_logs
UNION ALL
SELECT 'system_config', COUNT(*) FROM system_config
UNION ALL
SELECT 'users', COUNT(*) FROM users
UNION ALL
SELECT 'alerts', COUNT(*) FROM alerts
UNION ALL
SELECT 'box_status', COUNT(*) FROM box_status
UNION ALL
SELECT 'oht_operations', COUNT(*) FROM oht_operations
UNION ALL
SELECT 'user_logs', COUNT(*) FROM user_logs;

-- 박스 상태 확인
SELECT * FROM box_status ORDER BY box_id;

-- 시스템 설정 확인
SELECT * FROM system_config ORDER BY id;

-- 사용자 목록 확인
SELECT id, username, full_name, role, is_active, created_at FROM users;

-- 완료 메시지
SELECT '===================================================' AS '';
SELECT 'PCB 불량 검사 시스템 데이터베이스 스키마 생성 완료!' AS 'Status';
SELECT '===================================================' AS '';
SELECT '총 11개 테이블 생성됨:' AS 'Info';
SELECT '1. inspections (검사 결과 이력)' AS '';
SELECT '2. defect_images (불량 이미지 메타데이터)' AS '';
SELECT '3. statistics_daily (일별 통계)' AS '';
SELECT '4. statistics_hourly (시간별 통계)' AS '';
SELECT '5. system_logs (시스템 로그)' AS '';
SELECT '6. system_config (시스템 설정)' AS '';
SELECT '7. users (사용자/작업자)' AS '';
SELECT '8. alerts (알람/알림)' AS '';
SELECT '9. box_status (로봇팔 박스 상태 관리)' AS '';
SELECT '10. oht_operations (OHT 운영 이력)' AS '';
SELECT '11. user_logs (사용자 활동 로그)' AS '';
SELECT '===================================================' AS '';
