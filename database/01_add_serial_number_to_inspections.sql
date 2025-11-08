-- ================================================================
-- 방법 1: inspections 테이블에 일련번호 컬럼 추가 (간단, 권장)
-- ================================================================

USE pcb_inspection;

-- 일련번호 관련 컬럼 추가
ALTER TABLE inspections
ADD COLUMN serial_number VARCHAR(50) NULL COMMENT 'PCB 일련번호 (OCR 인식)' AFTER id,
ADD COLUMN serial_number_confidence DECIMAL(4,3) NULL COMMENT '일련번호 인식 신뢰도 (0.000-1.000)' AFTER serial_number,
ADD COLUMN serial_number_bbox JSON NULL COMMENT '일련번호 영역 좌표 (YOLO bbox)' AFTER serial_number_confidence;

-- 인덱스 추가 (일련번호로 빠른 검색)
CREATE INDEX idx_serial_number ON inspections(serial_number);

-- 복합 인덱스 (일련번호 + 검사 시간)
CREATE INDEX idx_serial_inspection_time ON inspections(serial_number, inspection_time);

-- ================================================================
-- 확인 쿼리
-- ================================================================

-- 테이블 구조 확인
DESCRIBE inspections;

-- 인덱스 확인
SHOW INDEX FROM inspections;

-- ================================================================
-- 사용 예시
-- ================================================================

-- 1. 특정 일련번호의 검사 이력 조회
-- SELECT * FROM inspections
-- WHERE serial_number = 'PCB-2025-001234'
-- ORDER BY inspection_time DESC;

-- 2. 특정 일련번호의 불량 이력만 조회
-- SELECT serial_number, fusion_decision, component_defects, solder_defects, inspection_time
-- FROM inspections
-- WHERE serial_number = 'PCB-2025-001234'
-- AND fusion_decision != 'normal';

-- 3. 일련번호별 불량 통계
-- SELECT serial_number,
--        COUNT(*) as total_inspections,
--        SUM(CASE WHEN fusion_decision = 'normal' THEN 1 ELSE 0 END) as normal_count,
--        SUM(CASE WHEN fusion_decision != 'normal' THEN 1 ELSE 0 END) as defect_count
-- FROM inspections
-- WHERE serial_number IS NOT NULL
-- GROUP BY serial_number;

-- 4. 일련번호 인식 실패 건수 (confidence < 0.7)
-- SELECT COUNT(*) as low_confidence_count
-- FROM inspections
-- WHERE serial_number_confidence < 0.700;
