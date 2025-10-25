-- PCB 불량 검사 시스템 - MySQL 사용자 생성 스크립트
-- 실행 방법: MySQL Workbench에서 이 파일을 열고 실행 (Ctrl+Shift+Enter)
-- 또는 명령줄: mysql -u root -p < create_users.sql

-- ========================================
-- 1. 데이터베이스 생성
-- ========================================

CREATE DATABASE IF NOT EXISTS pcb_inspection
CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci;

USE pcb_inspection;

-- ========================================
-- 2. 사용자 생성 (팀별)
-- ========================================

-- 2-1. 관리자 계정 (프로젝트 리더)
-- 모든 권한, 데이터베이스 구조 변경 가능
DROP USER IF EXISTS 'pcb_admin'@'%';
CREATE USER 'pcb_admin'@'%' IDENTIFIED BY '1234';
GRANT ALL PRIVILEGES ON pcb_inspection.* TO 'pcb_admin'@'%';

-- 2-2. Flask 서버용 계정
-- 읽기, 쓰기, 업데이트 가능 (DELETE 제외 - 안전성)
DROP USER IF EXISTS 'pcb_server'@'%';
CREATE USER 'pcb_server'@'%' IDENTIFIED BY '1234';
GRANT SELECT, INSERT, UPDATE ON pcb_inspection.* TO 'pcb_server'@'%';

-- 2-3. C# 앱 조회 전용 계정 (C# 모니터링 앱)
-- 읽기만 가능
DROP USER IF EXISTS 'pcb_viewer'@'%';
CREATE USER 'pcb_viewer'@'%' IDENTIFIED BY '1234';
GRANT SELECT ON pcb_inspection.* TO 'pcb_viewer'@'%';

-- 2-4. AI 모델 팀 계정 (데이터 입력 및 분석)
-- 읽기, 쓰기, 업데이트 가능
DROP USER IF EXISTS 'pcb_data'@'%';
CREATE USER 'pcb_data'@'%' IDENTIFIED BY '1234';
GRANT SELECT, INSERT, UPDATE ON pcb_inspection.* TO 'pcb_data'@'%';

-- 2-5. 라즈베리파이 팀 개발용 계정 (선택)
-- 테스트 데이터 입력용
DROP USER IF EXISTS 'pcb_test'@'%';
CREATE USER 'pcb_test'@'%' IDENTIFIED BY '1234';
GRANT SELECT, INSERT ON pcb_inspection.* TO 'pcb_test'@'%';

-- ========================================
-- 3. 권한 적용
-- ========================================

FLUSH PRIVILEGES;

-- ========================================
-- 4. 생성된 사용자 확인
-- ========================================

SELECT
    user AS 'Username',
    host AS 'Host',
    CASE
        WHEN user = 'pcb_admin' THEN 'Admin (All)'
        WHEN user = 'pcb_server' THEN 'Flask Server (SELECT, INSERT, UPDATE)'
        WHEN user = 'pcb_viewer' THEN 'C# App (SELECT only)'
        WHEN user = 'pcb_data' THEN 'AI Team (SELECT, INSERT, UPDATE)'
        WHEN user = 'pcb_test' THEN 'Test (SELECT, INSERT)'
        ELSE 'Unknown'
    END AS 'Role'
FROM mysql.user
WHERE user LIKE 'pcb%'
ORDER BY user;

-- ========================================
-- 5. 팀별 연결 정보 (참고용)
-- ========================================

/*
==============================================
팀별 MySQL 연결 정보
==============================================

공통:
- Host: 100.x.x.x (Windows PC의 Tailscale IP)
- Port: 3306
- Database: pcb_inspection

----------------------------------------------
1. Flask 팀 (서버 개발)
----------------------------------------------
Username: pcb_server
Password: 1234
권한: SELECT, INSERT, UPDATE
용도: Flask 서버에서 사용 (src/server/.env에 입력)

----------------------------------------------
2. Flask 팀 리더 (관리자)
----------------------------------------------
Username: pcb_admin
Password: 1234
권한: ALL PRIVILEGES
용도: 테이블 생성, 스키마 변경, 전체 관리

----------------------------------------------
3. AI 모델 팀 (데이터 분석)
----------------------------------------------
Username: pcb_data
Password: 1234
권한: SELECT, INSERT, UPDATE
용도: 학습 데이터 입력, 분석용 조회

----------------------------------------------
4. C# 앱 팀 (모니터링)
----------------------------------------------
Username: pcb_viewer
Password: 1234
권한: SELECT only (읽기 전용)
용도: C# WinForms 앱에서 데이터 조회

----------------------------------------------
5. 라즈베리파이 팀 (테스트)
----------------------------------------------
Username: pcb_test
Password: 1234
권한: SELECT, INSERT
용도: 테스트 데이터 입력 (개발용)

==============================================

연결 테스트:
mysql -h 100.x.x.x -u pcb_admin -p
Password: 1234

==============================================
*/
