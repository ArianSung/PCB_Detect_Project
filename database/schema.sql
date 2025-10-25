-- PCB 불량 검사 시스템 - 데이터베이스 스키마
-- 실행 방법: MySQL Workbench에서 pcb_admin 계정으로 실행

USE pcb_inspection;

-- ========================================
-- 1. 검사 이력 테이블 (inspection_history)
-- ========================================

CREATE TABLE IF NOT EXISTS inspection_history (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,

    -- 검사 정보
    camera_id VARCHAR(10) NOT NULL COMMENT '카메라 ID (left/right)',
    timestamp DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '검사 시각',
    request_id VARCHAR(36) COMMENT 'UUID 요청 ID',

    -- 검사 결과
    classification VARCHAR(20) NOT NULL COMMENT '분류 결과 (normal/component_defect/solder_defect/discard)',
    confidence DECIMAL(5, 4) NOT NULL COMMENT '신뢰도 (0.0000 ~ 1.0000)',
    total_defects INT NOT NULL DEFAULT 0 COMMENT '검출된 불량 개수',

    -- 불량 상세 정보 (JSON)
    defects_json JSON COMMENT '불량 상세 정보 (type, bbox, confidence, severity)',

    -- AI 모델 정보
    anomaly_score DECIMAL(5, 4) COMMENT '이상 탐지 점수 (0.0000 ~ 1.0000)',
    inference_time_ms DECIMAL(7, 2) COMMENT '추론 시간 (ms)',
    model_version VARCHAR(20) DEFAULT 'v1.0' COMMENT '모델 버전',

    -- GPIO 제어 정보
    gpio_pin INT COMMENT 'GPIO 핀 번호',
    gpio_action VARCHAR(20) COMMENT 'GPIO 동작 (activate/none)',

    -- 이미지 저장 경로 (선택)
    image_path VARCHAR(255) COMMENT '원본 이미지 경로',
    annotated_image_path VARCHAR(255) COMMENT '결과 표시 이미지 경로',

    -- 인덱스
    INDEX idx_timestamp (timestamp),
    INDEX idx_classification (classification),
    INDEX idx_camera_id (camera_id),
    INDEX idx_request_id (request_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='PCB 검사 이력 테이블';

-- ========================================
-- 2. 일별 통계 테이블 (daily_statistics)
-- ========================================

CREATE TABLE IF NOT EXISTS daily_statistics (
    id INT AUTO_INCREMENT PRIMARY KEY,

    -- 날짜
    date DATE NOT NULL UNIQUE COMMENT '통계 날짜',

    -- 전체 통계
    total_inspections INT NOT NULL DEFAULT 0 COMMENT '전체 검사 수',
    normal_count INT NOT NULL DEFAULT 0 COMMENT '정상 개수',
    component_defect_count INT NOT NULL DEFAULT 0 COMMENT '부품 불량 개수',
    solder_defect_count INT NOT NULL DEFAULT 0 COMMENT '납땜 불량 개수',
    discard_count INT NOT NULL DEFAULT 0 COMMENT '폐기 개수',

    -- 불량률
    defect_rate DECIMAL(5, 4) COMMENT '불량률 (0.0000 ~ 1.0000)',

    -- 성능 통계
    avg_inference_time_ms DECIMAL(7, 2) COMMENT '평균 추론 시간 (ms)',
    avg_confidence DECIMAL(5, 4) COMMENT '평균 신뢰도',

    -- 카메라별 통계
    left_camera_count INT DEFAULT 0 COMMENT '좌측 카메라 검사 수',
    right_camera_count INT DEFAULT 0 COMMENT '우측 카메라 검사 수',

    -- 생성/수정 시각
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    INDEX idx_date (date)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='일별 통계 테이블 (C# 앱 대시보드용)';

-- ========================================
-- 3. 불량 유형별 통계 테이블 (defect_type_statistics)
-- ========================================

CREATE TABLE IF NOT EXISTS defect_type_statistics (
    id INT AUTO_INCREMENT PRIMARY KEY,

    -- 날짜 및 불량 유형
    date DATE NOT NULL COMMENT '통계 날짜',
    defect_type VARCHAR(50) NOT NULL COMMENT '불량 유형',

    -- 통계
    count INT NOT NULL DEFAULT 0 COMMENT '발생 횟수',
    avg_confidence DECIMAL(5, 4) COMMENT '평균 신뢰도',

    -- 심각도별 통계
    low_severity_count INT DEFAULT 0 COMMENT '낮은 심각도',
    medium_severity_count INT DEFAULT 0 COMMENT '중간 심각도',
    high_severity_count INT DEFAULT 0 COMMENT '높은 심각도',

    -- 생성/수정 시각
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    UNIQUE KEY unique_date_type (date, defect_type),
    INDEX idx_date (date),
    INDEX idx_defect_type (defect_type)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='불량 유형별 통계 테이블';

-- ========================================
-- 4. 시스템 로그 테이블 (system_logs) - 선택
-- ========================================

CREATE TABLE IF NOT EXISTS system_logs (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,

    -- 로그 정보
    timestamp DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '로그 시각',
    level VARCHAR(10) NOT NULL COMMENT '로그 레벨 (DEBUG/INFO/WARNING/ERROR)',
    component VARCHAR(50) NOT NULL COMMENT '컴포넌트 (flask_server/raspberry_pi/etc)',
    message TEXT NOT NULL COMMENT '로그 메시지',

    -- 추가 정보
    details JSON COMMENT '상세 정보 (JSON)',

    INDEX idx_timestamp (timestamp),
    INDEX idx_level (level),
    INDEX idx_component (component)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='시스템 로그 테이블 (에러 추적용)';

-- ========================================
-- 5. 샘플 데이터 삽입 (테스트용)
-- ========================================

-- 샘플 검사 이력 (정상)
INSERT INTO inspection_history (
    camera_id, classification, confidence, total_defects,
    defects_json, anomaly_score, inference_time_ms,
    gpio_pin, gpio_action
) VALUES (
    'left', 'normal', 0.9850, 0,
    '[]', 0.1200, 95.23,
    23, 'activate'
);

-- 샘플 검사 이력 (납땜 불량)
INSERT INTO inspection_history (
    camera_id, classification, confidence, total_defects,
    defects_json, anomaly_score, inference_time_ms,
    gpio_pin, gpio_action
) VALUES (
    'right', 'solder_defect', 0.8720, 2,
    '[
        {"type": "cold_joint", "bbox": [120, 80, 200, 150], "confidence": 0.87, "severity": "medium"},
        {"type": "solder_bridge", "bbox": [300, 200, 380, 260], "confidence": 0.72, "severity": "low"}
    ]',
    0.6500, 120.45,
    27, 'activate'
);

-- 샘플 일별 통계
INSERT INTO daily_statistics (
    date, total_inspections, normal_count,
    component_defect_count, solder_defect_count, discard_count,
    defect_rate, avg_inference_time_ms, avg_confidence,
    left_camera_count, right_camera_count
) VALUES (
    CURDATE(), 250, 220,
    15, 12, 3,
    0.1200, 110.30, 0.9150,
    125, 125
);

-- ========================================
-- 6. 테이블 목록 및 구조 확인
-- ========================================

SHOW TABLES;

-- 각 테이블 구조 확인
DESCRIBE inspection_history;
DESCRIBE daily_statistics;
DESCRIBE defect_type_statistics;
DESCRIBE system_logs;

-- 샘플 데이터 조회
SELECT * FROM inspection_history LIMIT 5;
SELECT * FROM daily_statistics LIMIT 5;

-- ========================================
-- 완료 메시지
-- ========================================

SELECT 'PCB 불량 검사 시스템 데이터베이스 스키마 생성 완료!' AS message;
