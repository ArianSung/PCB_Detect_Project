-- ================================================================
-- 방법 2: 별도 pcb_products 테이블 생성 (고급, 제품 정보 중앙 관리)
-- ================================================================

USE pcb_inspection;

-- PCB 제품 마스터 테이블 생성
CREATE TABLE IF NOT EXISTS pcb_products (
    id INT AUTO_INCREMENT PRIMARY KEY,

    -- 일련번호 (고유키)
    serial_number VARCHAR(50) NOT NULL UNIQUE COMMENT 'PCB 일련번호',

    -- 제품 정보
    product_type VARCHAR(100) NULL COMMENT '제품 유형',
    product_model VARCHAR(100) NULL COMMENT '제품 모델',
    manufacture_date DATE NULL COMMENT '제조 날짜',
    lot_number VARCHAR(50) NULL COMMENT '로트 번호',

    -- 검사 통계 (캐시)
    total_inspections INT NOT NULL DEFAULT 0 COMMENT '총 검사 횟수',
    defect_count INT NOT NULL DEFAULT 0 COMMENT '불량 검출 횟수',
    last_inspection_date DATETIME NULL COMMENT '마지막 검사 일시',
    last_inspection_result VARCHAR(50) NULL COMMENT '마지막 검사 결과',

    -- 타임스탬프
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '최초 등록 일시',
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '최종 수정 일시',

    -- 인덱스
    INDEX idx_serial_number (serial_number),
    INDEX idx_product_type (product_type),
    INDEX idx_manufacture_date (manufacture_date),
    INDEX idx_last_inspection_date (last_inspection_date)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='PCB 제품 마스터 테이블 (일련번호 기반)';

-- inspections 테이블에 pcb_product_id FK 추가
ALTER TABLE inspections
ADD COLUMN pcb_product_id INT NULL COMMENT 'PCB 제품 ID (FK)' AFTER id;

-- 외래 키 제약 조건 추가
ALTER TABLE inspections
ADD CONSTRAINT fk_inspections_pcb_product
FOREIGN KEY (pcb_product_id) REFERENCES pcb_products(id)
ON DELETE SET NULL
ON UPDATE CASCADE;

-- 인덱스 추가
CREATE INDEX idx_pcb_product_id ON inspections(pcb_product_id);

-- ================================================================
-- 트리거: 검사 결과 삽입 시 pcb_products 업데이트
-- ================================================================

DELIMITER $$

CREATE TRIGGER after_inspection_insert_update_product
AFTER INSERT ON inspections
FOR EACH ROW
BEGIN
    -- pcb_product_id가 있는 경우에만 업데이트
    IF NEW.pcb_product_id IS NOT NULL THEN
        UPDATE pcb_products
        SET total_inspections = total_inspections + 1,
            defect_count = defect_count + CASE WHEN NEW.fusion_decision != 'normal' THEN 1 ELSE 0 END,
            last_inspection_date = NEW.inspection_time,
            last_inspection_result = NEW.fusion_decision,
            updated_at = CURRENT_TIMESTAMP
        WHERE id = NEW.pcb_product_id;
    END IF;
END$$

DELIMITER ;

-- ================================================================
-- 저장 프로시저: 일련번호로 제품 찾기 또는 생성
-- ================================================================

DELIMITER $$

CREATE PROCEDURE get_or_create_product(
    IN p_serial_number VARCHAR(50),
    OUT p_product_id INT
)
BEGIN
    -- 일련번호로 제품 조회
    SELECT id INTO p_product_id
    FROM pcb_products
    WHERE serial_number = p_serial_number
    LIMIT 1;

    -- 없으면 새로 생성
    IF p_product_id IS NULL THEN
        INSERT INTO pcb_products (serial_number)
        VALUES (p_serial_number);

        SET p_product_id = LAST_INSERT_ID();
    END IF;
END$$

DELIMITER ;

-- ================================================================
-- 뷰: 제품별 검사 통계
-- ================================================================

CREATE VIEW v_product_inspection_statistics AS
SELECT
    p.serial_number,
    p.product_type,
    p.total_inspections,
    p.defect_count,
    ROUND(
        CASE
            WHEN p.total_inspections > 0 THEN (p.defect_count * 100.0 / p.total_inspections)
            ELSE 0
        END,
        2
    ) AS defect_rate,
    p.last_inspection_date,
    p.last_inspection_result,
    p.created_at,
    p.updated_at
FROM pcb_products p
ORDER BY p.last_inspection_date DESC;

-- ================================================================
-- 확인 쿼리
-- ================================================================

-- 테이블 구조 확인
DESCRIBE pcb_products;
DESCRIBE inspections;

-- 외래 키 확인
SELECT
    CONSTRAINT_NAME,
    TABLE_NAME,
    REFERENCED_TABLE_NAME
FROM information_schema.KEY_COLUMN_USAGE
WHERE TABLE_SCHEMA = 'pcb_inspection'
AND REFERENCED_TABLE_NAME = 'pcb_products';

-- ================================================================
-- 사용 예시
-- ================================================================

-- 1. 제품 등록 또는 조회
-- CALL get_or_create_product('PCB-2025-001234', @product_id);
-- SELECT @product_id;

-- 2. 검사 결과 삽입 시 (Python에서)
-- INSERT INTO inspections (pcb_product_id, serial_number, fusion_decision, ...)
-- VALUES (@product_id, 'PCB-2025-001234', 'normal', ...);

-- 3. 특정 제품의 검사 이력 조회
-- SELECT i.*
-- FROM inspections i
-- INNER JOIN pcb_products p ON i.pcb_product_id = p.id
-- WHERE p.serial_number = 'PCB-2025-001234'
-- ORDER BY i.inspection_time DESC;

-- 4. 제품별 검사 통계 조회
-- SELECT * FROM v_product_inspection_statistics
-- WHERE total_inspections > 0
-- ORDER BY defect_rate DESC;

-- 5. 최근 검사된 제품 목록
-- SELECT serial_number, last_inspection_date, last_inspection_result, total_inspections
-- FROM pcb_products
-- WHERE last_inspection_date >= DATE_SUB(NOW(), INTERVAL 7 DAY)
-- ORDER BY last_inspection_date DESC;
