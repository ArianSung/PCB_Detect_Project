-- =====================================================
-- PCB 불량 검사 시스템 트리거 v3.1 (Extended for WinForms)
-- Product Verification Architecture + Defect Details
-- =====================================================
-- 작성일: 2025-11-28 (v3.0)
-- 확장일: 2025-11-28 (v3.1)
-- 설명: inspections 테이블 INSERT 시 자동으로 집계 테이블 및 상세 정보 업데이트
-- 트리거 종류: 4개
--   1. update_hourly_summary: 시간별 집계 업데이트
--   2. update_daily_summary: 일별 집계 업데이트
--   3. update_monthly_summary: 월별 집계 업데이트
--   4. insert_defect_details: 불량 클래스별 상세 정보 자동 삽입 (v3.1 추가)
-- =====================================================

USE pcb_inspection;

-- =====================================================
-- 기존 트리거 삭제 (재설치 시)
-- =====================================================

DROP TRIGGER IF EXISTS update_hourly_summary;
DROP TRIGGER IF EXISTS update_daily_summary;
DROP TRIGGER IF EXISTS update_monthly_summary;
DROP TRIGGER IF EXISTS insert_defect_details;


-- =====================================================
-- 1. 시간별 집계 트리거
-- =====================================================
-- 설명: inspections 테이블 INSERT 시 시간별 집계 자동 업데이트
-- 동작: UPSERT 패턴 (INSERT ... ON DUPLICATE KEY UPDATE)
-- =====================================================

DELIMITER $$

CREATE TRIGGER update_hourly_summary
AFTER INSERT ON inspections
FOR EACH ROW
BEGIN
    DECLARE hour_ts DATETIME;

    -- 시간 단위로 자르기 (분/초 제거)
    SET hour_ts = DATE_FORMAT(NEW.inspection_time, '%Y-%m-%d %H:00:00');

    -- UPSERT: 기존 레코드가 있으면 UPDATE, 없으면 INSERT
    INSERT INTO inspection_summary_hourly (
        hour_timestamp,
        product_code,
        total_inspections,
        normal_count,
        missing_count,
        position_error_count,
        discard_count,
        avg_inference_time_ms,
        avg_total_time_ms,
        avg_detection_count,
        avg_confidence
    ) VALUES (
        hour_ts,
        NEW.product_code,
        1,
        IF(NEW.decision = 'normal', 1, 0),
        IF(NEW.decision = 'missing', 1, 0),
        IF(NEW.decision = 'position_error', 1, 0),
        IF(NEW.decision = 'discard', 1, 0),
        NEW.inference_time_ms,
        NEW.total_time_ms,
        NEW.detection_count,
        NEW.avg_confidence
    )
    ON DUPLICATE KEY UPDATE
        total_inspections = total_inspections + 1,
        normal_count = normal_count + IF(NEW.decision = 'normal', 1, 0),
        missing_count = missing_count + IF(NEW.decision = 'missing', 1, 0),
        position_error_count = position_error_count + IF(NEW.decision = 'position_error', 1, 0),
        discard_count = discard_count + IF(NEW.decision = 'discard', 1, 0),

        -- 평균값 재계산 (누적합 방식)
        avg_inference_time_ms = (
            (COALESCE(avg_inference_time_ms, 0) * (total_inspections - 1) + COALESCE(NEW.inference_time_ms, 0))
            / total_inspections
        ),
        avg_total_time_ms = (
            (COALESCE(avg_total_time_ms, 0) * (total_inspections - 1) + COALESCE(NEW.total_time_ms, 0))
            / total_inspections
        ),
        avg_detection_count = (
            (COALESCE(avg_detection_count, 0) * (total_inspections - 1) + NEW.detection_count)
            / total_inspections
        ),
        avg_confidence = (
            (COALESCE(avg_confidence, 0) * (total_inspections - 1) + COALESCE(NEW.avg_confidence, 0))
            / total_inspections
        ),

        updated_at = CURRENT_TIMESTAMP;
END$$

DELIMITER ;


-- =====================================================
-- 2. 일별 집계 트리거
-- =====================================================
-- 설명: inspections 테이블 INSERT 시 일별 집계 자동 업데이트
-- 동작: UPSERT 패턴 (INSERT ... ON DUPLICATE KEY UPDATE)
-- =====================================================

DELIMITER $$

CREATE TRIGGER update_daily_summary
AFTER INSERT ON inspections
FOR EACH ROW
BEGIN
    DECLARE date_val DATE;

    -- 날짜만 추출
    SET date_val = DATE(NEW.inspection_time);

    -- UPSERT: 기존 레코드가 있으면 UPDATE, 없으면 INSERT
    INSERT INTO inspection_summary_daily (
        date,
        product_code,
        total_inspections,
        normal_count,
        missing_count,
        position_error_count,
        discard_count,
        avg_inference_time_ms,
        avg_total_time_ms,
        avg_detection_count,
        avg_confidence
    ) VALUES (
        date_val,
        NEW.product_code,
        1,
        IF(NEW.decision = 'normal', 1, 0),
        IF(NEW.decision = 'missing', 1, 0),
        IF(NEW.decision = 'position_error', 1, 0),
        IF(NEW.decision = 'discard', 1, 0),
        NEW.inference_time_ms,
        NEW.total_time_ms,
        NEW.detection_count,
        NEW.avg_confidence
    )
    ON DUPLICATE KEY UPDATE
        total_inspections = total_inspections + 1,
        normal_count = normal_count + IF(NEW.decision = 'normal', 1, 0),
        missing_count = missing_count + IF(NEW.decision = 'missing', 1, 0),
        position_error_count = position_error_count + IF(NEW.decision = 'position_error', 1, 0),
        discard_count = discard_count + IF(NEW.decision = 'discard', 1, 0),

        -- 평균값 재계산 (누적합 방식)
        avg_inference_time_ms = (
            (COALESCE(avg_inference_time_ms, 0) * (total_inspections - 1) + COALESCE(NEW.inference_time_ms, 0))
            / total_inspections
        ),
        avg_total_time_ms = (
            (COALESCE(avg_total_time_ms, 0) * (total_inspections - 1) + COALESCE(NEW.total_time_ms, 0))
            / total_inspections
        ),
        avg_detection_count = (
            (COALESCE(avg_detection_count, 0) * (total_inspections - 1) + NEW.detection_count)
            / total_inspections
        ),
        avg_confidence = (
            (COALESCE(avg_confidence, 0) * (total_inspections - 1) + COALESCE(NEW.avg_confidence, 0))
            / total_inspections
        ),

        updated_at = CURRENT_TIMESTAMP;
END$$

DELIMITER ;


-- =====================================================
-- 3. 월별 집계 트리거
-- =====================================================
-- 설명: inspections 테이블 INSERT 시 월별 집계 자동 업데이트
-- 동작: UPSERT 패턴 (INSERT ... ON DUPLICATE KEY UPDATE)
-- =====================================================

DELIMITER $$

CREATE TRIGGER update_monthly_summary
AFTER INSERT ON inspections
FOR EACH ROW
BEGIN
    DECLARE year_val INT;
    DECLARE month_val INT;

    -- 년도와 월 추출
    SET year_val = YEAR(NEW.inspection_time);
    SET month_val = MONTH(NEW.inspection_time);

    -- UPSERT: 기존 레코드가 있으면 UPDATE, 없으면 INSERT
    INSERT INTO inspection_summary_monthly (
        year,
        month,
        product_code,
        total_inspections,
        normal_count,
        missing_count,
        position_error_count,
        discard_count,
        avg_inference_time_ms,
        avg_total_time_ms,
        avg_detection_count,
        avg_confidence
    ) VALUES (
        year_val,
        month_val,
        NEW.product_code,
        1,
        IF(NEW.decision = 'normal', 1, 0),
        IF(NEW.decision = 'missing', 1, 0),
        IF(NEW.decision = 'position_error', 1, 0),
        IF(NEW.decision = 'discard', 1, 0),
        NEW.inference_time_ms,
        NEW.total_time_ms,
        NEW.detection_count,
        NEW.avg_confidence
    )
    ON DUPLICATE KEY UPDATE
        total_inspections = total_inspections + 1,
        normal_count = normal_count + IF(NEW.decision = 'normal', 1, 0),
        missing_count = missing_count + IF(NEW.decision = 'missing', 1, 0),
        position_error_count = position_error_count + IF(NEW.decision = 'position_error', 1, 0),
        discard_count = discard_count + IF(NEW.decision = 'discard', 1, 0),

        -- 평균값 재계산 (누적합 방식)
        avg_inference_time_ms = (
            (COALESCE(avg_inference_time_ms, 0) * (total_inspections - 1) + COALESCE(NEW.inference_time_ms, 0))
            / total_inspections
        ),
        avg_total_time_ms = (
            (COALESCE(avg_total_time_ms, 0) * (total_inspections - 1) + COALESCE(NEW.total_time_ms, 0))
            / total_inspections
        ),
        avg_detection_count = (
            (COALESCE(avg_detection_count, 0) * (total_inspections - 1) + NEW.detection_count)
            / total_inspections
        ),
        avg_confidence = (
            (COALESCE(avg_confidence, 0) * (total_inspections - 1) + COALESCE(NEW.avg_confidence, 0))
            / total_inspections
        ),

        updated_at = CURRENT_TIMESTAMP;
END$$

DELIMITER ;


-- =====================================================
-- 4. 불량 클래스별 상세 정보 자동 삽입 트리거 (v3.1 추가)
-- =====================================================
-- 설명: inspections 테이블 INSERT 시 yolo_detections JSON 파싱하여
--       defect_details 테이블에 클래스별 통계 자동 삽입
-- 동작: yolo_detections 컬럼의 JSON 배열을 파싱하여 각 클래스별로 레코드 생성
-- JSON 형식: [{"class_name": "...", "count": 5, "confidence": 0.95}, ...]
-- =====================================================

DELIMITER $$

CREATE TRIGGER insert_defect_details
AFTER INSERT ON inspections
FOR EACH ROW
BEGIN
    -- yolo_detections가 NULL이거나 빈 배열이 아닌 경우만 처리
    IF NEW.yolo_detections IS NOT NULL AND JSON_LENGTH(NEW.yolo_detections) > 0 THEN
        -- JSON 배열을 파싱하여 defect_details에 삽입
        INSERT INTO defect_details (inspection_id, class_name, count, avg_confidence)
        SELECT
            NEW.id,
            jt.class_name,
            jt.count,
            jt.confidence
        FROM JSON_TABLE(
            NEW.yolo_detections,
            '$[*]' COLUMNS(
                class_name VARCHAR(100) PATH '$.class_name',
                count INT PATH '$.count',
                confidence DECIMAL(5,4) PATH '$.confidence'
            )
        ) AS jt;
    END IF;
END$$

DELIMITER ;


-- =====================================================
-- 트리거 확인
-- =====================================================
-- 다음 쿼리로 트리거 생성 확인 가능:
-- SHOW TRIGGERS FROM pcb_inspection;
--
-- 트리거 상세 정보:
-- SHOW CREATE TRIGGER update_hourly_summary;
-- SHOW CREATE TRIGGER update_daily_summary;
-- SHOW CREATE TRIGGER update_monthly_summary;
-- SHOW CREATE TRIGGER insert_defect_details;
-- =====================================================

-- 완료 메시지
SELECT 'v3.1 트리거 생성 완료 (4개): update_hourly_summary, update_daily_summary, update_monthly_summary, insert_defect_details' AS status;
