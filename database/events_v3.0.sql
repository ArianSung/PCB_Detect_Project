-- =====================================================
-- PCB 불량 검사 시스템 이벤트 스케줄러 v3.1 (Extended for WinForms)
-- Product Verification Architecture + User Logs + Alerts + OHT
-- =====================================================
-- 작성일: 2025-11-28 (v3.0)
-- 확장일: 2025-11-28 (v3.1)
-- 설명: 10년 이상 된 데이터 자동 삭제 (매일 새벽 2시~3시 실행)
-- 대상 테이블: 7개
--   1. inspections (10년 이상) - 매일 02:10
--   2. inspection_summary_hourly (10년 이상) - 매일 02:20
--   3. inspection_summary_daily (10년 이상) - 매일 02:30
--   4. inspection_summary_monthly (10년 이상) - 매일 02:40
--   5. user_logs (10년 이상) - 매일 02:50 (v3.1 추가)
--   6. alerts (10년 이상) - 매일 03:00 (v3.1 추가)
--   7. oht_operations (10년 이상) - 매일 03:10 (v3.1 추가)
-- =====================================================

USE pcb_inspection;

-- =====================================================
-- Event Scheduler 활성화
-- =====================================================
-- Event Scheduler가 비활성화되어 있으면 이벤트가 실행되지 않음
-- 서버 재시작 시에도 자동 활성화되도록 my.cnf 또는 my.ini에 추가:
-- [mysqld]
-- event_scheduler = ON
-- =====================================================

SET GLOBAL event_scheduler = ON;

SELECT @@event_scheduler AS event_scheduler_status;


-- =====================================================
-- 기존 이벤트 삭제 (재설치 시)
-- =====================================================

DROP EVENT IF EXISTS cleanup_old_inspections;
DROP EVENT IF EXISTS cleanup_old_hourly_summary;
DROP EVENT IF EXISTS cleanup_old_daily_summary;
DROP EVENT IF EXISTS cleanup_old_monthly_summary;
DROP EVENT IF EXISTS cleanup_old_user_logs;
DROP EVENT IF EXISTS cleanup_old_alerts;
DROP EVENT IF EXISTS cleanup_old_oht_operations;


-- =====================================================
-- 1. 메인 검사 이력 정리 이벤트
-- =====================================================
-- 설명: inspections 테이블에서 10년 이상 된 레코드 삭제
-- 실행: 매일 새벽 2시 10분
-- 보관 기간: 10년 (3650일)
-- =====================================================

DELIMITER $$

CREATE EVENT cleanup_old_inspections
ON SCHEDULE
    EVERY 1 DAY
    STARTS CONCAT(CURDATE() + INTERVAL 1 DAY, ' 02:10:00')
ON COMPLETION PRESERVE
ENABLE
COMMENT '10년 이상 된 검사 이력 자동 삭제 (매일 02:10)'
DO
BEGIN
    DECLARE deleted_rows INT DEFAULT 0;
    DECLARE cutoff_date DATETIME;

    -- 10년 전 날짜 계산
    SET cutoff_date = DATE_SUB(NOW(), INTERVAL 10 YEAR);

    -- 10년 이상 된 데이터 삭제
    DELETE FROM inspections
    WHERE inspection_time < cutoff_date;

    -- 삭제된 행 수 확인
    SET deleted_rows = ROW_COUNT();

    -- 로그 기록 (임시 테이블 사용 시)
    -- INSERT INTO cleanup_log (table_name, deleted_rows, cutoff_date)
    -- VALUES ('inspections', deleted_rows, cutoff_date);

    -- 삭제 완료 메시지 (로그에 기록됨)
    IF deleted_rows > 0 THEN
        SELECT CONCAT('inspections 테이블에서 ', deleted_rows, '개 레코드 삭제 완료 (기준: ', cutoff_date, ')') AS cleanup_status;
    END IF;
END$$

DELIMITER ;


-- =====================================================
-- 2. 시간별 집계 정리 이벤트
-- =====================================================
-- 설명: inspection_summary_hourly 테이블에서 10년 이상 된 레코드 삭제
-- 실행: 매일 새벽 2시 20분
-- 보관 기간: 10년
-- =====================================================

DELIMITER $$

CREATE EVENT cleanup_old_hourly_summary
ON SCHEDULE
    EVERY 1 DAY
    STARTS CONCAT(CURDATE() + INTERVAL 1 DAY, ' 02:20:00')
ON COMPLETION PRESERVE
ENABLE
COMMENT '10년 이상 된 시간별 집계 자동 삭제 (매일 02:20)'
DO
BEGIN
    DECLARE deleted_rows INT DEFAULT 0;
    DECLARE cutoff_timestamp DATETIME;

    -- 10년 전 날짜 계산
    SET cutoff_timestamp = DATE_SUB(NOW(), INTERVAL 10 YEAR);

    -- 10년 이상 된 데이터 삭제
    DELETE FROM inspection_summary_hourly
    WHERE hour_timestamp < cutoff_timestamp;

    -- 삭제된 행 수 확인
    SET deleted_rows = ROW_COUNT();

    -- 삭제 완료 메시지
    IF deleted_rows > 0 THEN
        SELECT CONCAT('inspection_summary_hourly 테이블에서 ', deleted_rows, '개 레코드 삭제 완료 (기준: ', cutoff_timestamp, ')') AS cleanup_status;
    END IF;
END$$

DELIMITER ;


-- =====================================================
-- 3. 일별 집계 정리 이벤트
-- =====================================================
-- 설명: inspection_summary_daily 테이블에서 10년 이상 된 레코드 삭제
-- 실행: 매일 새벽 2시 30분
-- 보관 기간: 10년
-- =====================================================

DELIMITER $$

CREATE EVENT cleanup_old_daily_summary
ON SCHEDULE
    EVERY 1 DAY
    STARTS CONCAT(CURDATE() + INTERVAL 1 DAY, ' 02:30:00')
ON COMPLETION PRESERVE
ENABLE
COMMENT '10년 이상 된 일별 집계 자동 삭제 (매일 02:30)'
DO
BEGIN
    DECLARE deleted_rows INT DEFAULT 0;
    DECLARE cutoff_date DATE;

    -- 10년 전 날짜 계산
    SET cutoff_date = DATE_SUB(CURDATE(), INTERVAL 10 YEAR);

    -- 10년 이상 된 데이터 삭제
    DELETE FROM inspection_summary_daily
    WHERE date < cutoff_date;

    -- 삭제된 행 수 확인
    SET deleted_rows = ROW_COUNT();

    -- 삭제 완료 메시지
    IF deleted_rows > 0 THEN
        SELECT CONCAT('inspection_summary_daily 테이블에서 ', deleted_rows, '개 레코드 삭제 완료 (기준: ', cutoff_date, ')') AS cleanup_status;
    END IF;
END$$

DELIMITER ;


-- =====================================================
-- 4. 월별 집계 정리 이벤트
-- =====================================================
-- 설명: inspection_summary_monthly 테이블에서 10년 이상 된 레코드 삭제
-- 실행: 매일 새벽 2시 40분
-- 보관 기간: 10년
-- =====================================================

DELIMITER $$

CREATE EVENT cleanup_old_monthly_summary
ON SCHEDULE
    EVERY 1 DAY
    STARTS CONCAT(CURDATE() + INTERVAL 1 DAY, ' 02:40:00')
ON COMPLETION PRESERVE
ENABLE
COMMENT '10년 이상 된 월별 집계 자동 삭제 (매일 02:40)'
DO
BEGIN
    DECLARE deleted_rows INT DEFAULT 0;
    DECLARE cutoff_year INT;
    DECLARE cutoff_month INT;
    DECLARE cutoff_date DATE;

    -- 10년 전 날짜 계산
    SET cutoff_date = DATE_SUB(CURDATE(), INTERVAL 10 YEAR);
    SET cutoff_year = YEAR(cutoff_date);
    SET cutoff_month = MONTH(cutoff_date);

    -- 10년 이상 된 데이터 삭제
    DELETE FROM inspection_summary_monthly
    WHERE (year < cutoff_year)
       OR (year = cutoff_year AND month < cutoff_month);

    -- 삭제된 행 수 확인
    SET deleted_rows = ROW_COUNT();

    -- 삭제 완료 메시지
    IF deleted_rows > 0 THEN
        SELECT CONCAT('inspection_summary_monthly 테이블에서 ', deleted_rows, '개 레코드 삭제 완료 (기준: ', cutoff_year, '-', cutoff_month, ')') AS cleanup_status;
    END IF;
END$$

DELIMITER ;


-- =====================================================
-- 5. 사용자 활동 로그 정리 이벤트 (v3.1 추가)
-- =====================================================
-- 설명: user_logs 테이블에서 10년 이상 된 레코드 삭제
-- 실행: 매일 새벽 2시 50분
-- 보관 기간: 10년
-- =====================================================

DELIMITER $$

CREATE EVENT cleanup_old_user_logs
ON SCHEDULE
    EVERY 1 DAY
    STARTS CONCAT(CURDATE() + INTERVAL 1 DAY, ' 02:50:00')
ON COMPLETION PRESERVE
ENABLE
COMMENT '10년 이상 된 사용자 로그 자동 삭제 (매일 02:50)'
DO
BEGIN
    DECLARE deleted_rows INT DEFAULT 0;
    DECLARE cutoff_date DATETIME;

    -- 10년 전 날짜 계산
    SET cutoff_date = DATE_SUB(NOW(), INTERVAL 10 YEAR);

    -- 10년 이상 된 데이터 삭제
    DELETE FROM user_logs
    WHERE created_at < cutoff_date;

    -- 삭제된 행 수 확인
    SET deleted_rows = ROW_COUNT();

    -- 삭제 완료 메시지
    IF deleted_rows > 0 THEN
        SELECT CONCAT('user_logs 테이블에서 ', deleted_rows, '개 레코드 삭제 완료 (기준: ', cutoff_date, ')') AS cleanup_status;
    END IF;
END$$

DELIMITER ;


-- =====================================================
-- 6. 알림 정리 이벤트 (v3.1 추가)
-- =====================================================
-- 설명: alerts 테이블에서 10년 이상 된 레코드 삭제
-- 실행: 매일 새벽 3시 00분
-- 보관 기간: 10년
-- =====================================================

DELIMITER $$

CREATE EVENT cleanup_old_alerts
ON SCHEDULE
    EVERY 1 DAY
    STARTS CONCAT(CURDATE() + INTERVAL 1 DAY, ' 03:00:00')
ON COMPLETION PRESERVE
ENABLE
COMMENT '10년 이상 된 알림 자동 삭제 (매일 03:00)'
DO
BEGIN
    DECLARE deleted_rows INT DEFAULT 0;
    DECLARE cutoff_date DATETIME;

    -- 10년 전 날짜 계산
    SET cutoff_date = DATE_SUB(NOW(), INTERVAL 10 YEAR);

    -- 10년 이상 된 데이터 삭제
    DELETE FROM alerts
    WHERE created_at < cutoff_date;

    -- 삭제된 행 수 확인
    SET deleted_rows = ROW_COUNT();

    -- 삭제 완료 메시지
    IF deleted_rows > 0 THEN
        SELECT CONCAT('alerts 테이블에서 ', deleted_rows, '개 레코드 삭제 완료 (기준: ', cutoff_date, ')') AS cleanup_status;
    END IF;
END$$

DELIMITER ;


-- =====================================================
-- 7. OHT 운영 이력 정리 이벤트 (v3.1 추가)
-- =====================================================
-- 설명: oht_operations 테이블에서 10년 이상 된 레코드 삭제
-- 실행: 매일 새벽 3시 10분
-- 보관 기간: 10년
-- =====================================================

DELIMITER $$

CREATE EVENT cleanup_old_oht_operations
ON SCHEDULE
    EVERY 1 DAY
    STARTS CONCAT(CURDATE() + INTERVAL 1 DAY, ' 03:10:00')
ON COMPLETION PRESERVE
ENABLE
COMMENT '10년 이상 된 OHT 운영 이력 자동 삭제 (매일 03:10)'
DO
BEGIN
    DECLARE deleted_rows INT DEFAULT 0;
    DECLARE cutoff_date DATETIME;

    -- 10년 전 날짜 계산
    SET cutoff_date = DATE_SUB(NOW(), INTERVAL 10 YEAR);

    -- 10년 이상 된 데이터 삭제
    DELETE FROM oht_operations
    WHERE created_at < cutoff_date;

    -- 삭제된 행 수 확인
    SET deleted_rows = ROW_COUNT();

    -- 삭제 완료 메시지
    IF deleted_rows > 0 THEN
        SELECT CONCAT('oht_operations 테이블에서 ', deleted_rows, '개 레코드 삭제 완료 (기준: ', cutoff_date, ')') AS cleanup_status;
    END IF;
END$$

DELIMITER ;


-- =====================================================
-- 이벤트 확인
-- =====================================================
-- 다음 쿼리로 이벤트 생성 확인 가능:
-- SHOW EVENTS FROM pcb_inspection;
--
-- 이벤트 상세 정보:
-- SHOW CREATE EVENT cleanup_old_inspections;
-- SHOW CREATE EVENT cleanup_old_hourly_summary;
-- SHOW CREATE EVENT cleanup_old_daily_summary;
-- SHOW CREATE EVENT cleanup_old_monthly_summary;
-- SHOW CREATE EVENT cleanup_old_user_logs;
-- SHOW CREATE EVENT cleanup_old_alerts;
-- SHOW CREATE EVENT cleanup_old_oht_operations;
--
-- 이벤트 실행 이력 확인 (performance_schema 활성화 필요):
-- SELECT * FROM performance_schema.events_statements_history
-- WHERE sql_text LIKE '%cleanup_old%';
-- =====================================================

-- =====================================================
-- 수동 실행 (테스트용)
-- =====================================================
-- 이벤트를 즉시 실행하려면:
-- CALL mysql.rds_run_scheduled_event('pcb_inspection.cleanup_old_inspections');
--
-- 또는 수동 삭제:
-- DELETE FROM inspections WHERE inspection_time < DATE_SUB(NOW(), INTERVAL 10 YEAR);
-- DELETE FROM inspection_summary_hourly WHERE hour_timestamp < DATE_SUB(NOW(), INTERVAL 10 YEAR);
-- DELETE FROM inspection_summary_daily WHERE date < DATE_SUB(CURDATE(), INTERVAL 10 YEAR);
-- DELETE FROM inspection_summary_monthly WHERE (year < YEAR(DATE_SUB(CURDATE(), INTERVAL 10 YEAR)));
-- DELETE FROM user_logs WHERE created_at < DATE_SUB(NOW(), INTERVAL 10 YEAR);
-- DELETE FROM alerts WHERE created_at < DATE_SUB(NOW(), INTERVAL 10 YEAR);
-- DELETE FROM oht_operations WHERE created_at < DATE_SUB(NOW(), INTERVAL 10 YEAR);
-- =====================================================

-- 완료 메시지
SELECT 'v3.1 Event Scheduler 생성 완료: 매일 새벽 2시 10분~3시 10분, 10년 이상 된 데이터 자동 삭제 (7개 테이블)' AS status;
SELECT 'Event 목록: cleanup_old_inspections, cleanup_old_hourly_summary, cleanup_old_daily_summary, cleanup_old_monthly_summary, cleanup_old_user_logs, cleanup_old_alerts, cleanup_old_oht_operations' AS events;
