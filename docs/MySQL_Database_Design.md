# MySQL 데이터베이스 설계 - PCB 검사 시스템

## 개요

PCB 불량 검사 시스템의 검사 이력, 통계, 시스템 로그를 저장하는 MySQL 데이터베이스 스키마 설계입니다.

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

### 1. inspections (검사 결과 이력)

```sql
CREATE TABLE inspections (
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

### 3. statistics_daily (일별 통계)

```sql
CREATE TABLE statistics_daily (
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
```

### 4. statistics_hourly (시간별 통계)

```sql
CREATE TABLE statistics_hourly (
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
    alert_type ENUM('defect_rate_high', 'system_error', 'camera_offline', 'server_offline') NOT NULL,
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
COMMENT='알람 및 알림';
```

---

## 초기 데이터 삽입

### 시스템 설정 기본값

```sql
INSERT INTO system_config (config_key, config_value, description) VALUES
('server_url', 'http://192.168.0.10:5000', 'Flask 서버 URL'),
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

---

## 뷰 (View) 정의

### 실시간 통계 뷰

```sql
CREATE VIEW v_realtime_statistics AS
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
```

---

## 저장 프로시저

### 1. 일별 통계 업데이트

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

### 검사 결과 삽입 시 시간별 통계 업데이트

```sql
DELIMITER $$

CREATE TRIGGER after_inspection_insert
AFTER INSERT ON inspections
FOR EACH ROW
BEGIN
    DECLARE stat_hour DATETIME;

    -- 시간 단위로 반올림 (예: 2025-10-22 14:35:20 → 2025-10-22 14:00:00)
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
```

---

## 인덱스 최적화

### 복합 인덱스

```sql
-- 날짜 범위 조회용
CREATE INDEX idx_inspection_time_defect_type
ON inspections (inspection_time, defect_type);

-- 카메라별 검색용
CREATE INDEX idx_camera_defect_type
ON inspections (camera_id, defect_type, inspection_time);
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

### PyMySQL 사용

```python
import pymysql

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
                 (camera_id, defect_type, confidence, image_path, boxes, gpio_pin, gpio_duration_ms)
                 VALUES (%s, %s, %s, %s, %s, %s, %s)"""
        cursor.execute(sql, ('left', '부품불량', 0.95, '/path/to/image.jpg', '[]', 17, 500))
    conn.commit()

    # 검사 이력 조회
    with conn.cursor() as cursor:
        sql = "SELECT * FROM inspections ORDER BY inspection_time DESC LIMIT 10"
        cursor.execute(sql)
        results = cursor.fetchall()
        for row in results:
            print(row)

finally:
    conn.close()
```

---

## C# 연결 예제

### MySql.Data 사용

```csharp
using MySql.Data.MySqlClient;

string connStr = "server=localhost;user=root;database=pcb_inspection;password=your_password;";
MySqlConnection conn = new MySqlConnection(connStr);

try
{
    conn.Open();

    // 검사 결과 삽입
    string sql = @"INSERT INTO inspections
                   (camera_id, defect_type, confidence, image_path, gpio_pin, gpio_duration_ms)
                   VALUES (@camera_id, @defect_type, @confidence, @image_path, @gpio_pin, @gpio_duration_ms)";

    MySqlCommand cmd = new MySqlCommand(sql, conn);
    cmd.Parameters.AddWithValue("@camera_id", "left");
    cmd.Parameters.AddWithValue("@defect_type", "부품불량");
    cmd.Parameters.AddWithValue("@confidence", 0.95);
    cmd.Parameters.AddWithValue("@image_path", "/path/to/image.jpg");
    cmd.Parameters.AddWithValue("@gpio_pin", 17);
    cmd.Parameters.AddWithValue("@gpio_duration_ms", 500);

    cmd.ExecuteNonQuery();
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
CREATE USER 'flask_server'@'192.168.0.10' IDENTIFIED BY 'STRONG_PASSWORD_HERE';

-- pcb_inspection 데이터베이스에 대한 권한 부여
GRANT SELECT, INSERT, UPDATE ON pcb_inspection.* TO 'flask_server'@'192.168.0.10';

-- 변경사항 적용
FLUSH PRIVILEGES;
```

**참고**:
- `192.168.0.10`은 Flask 서버의 IP 주소
- 실제 사용 시 `STRONG_PASSWORD_HERE`를 강력한 비밀번호로 변경

#### 2. C# WinForms 모니터링 앱용 MySQL 사용자 생성 (읽기 전용)

모니터링 앱은 검사 이력 및 통계 조회만 필요하므로 읽기 전용 권한 부여.

```sql
-- C# WinForms 앱 전용 사용자 생성
CREATE USER 'winforms_app'@'192.168.0.30' IDENTIFIED BY 'STRONG_PASSWORD_HERE';

-- 읽기 전용 권한 부여
GRANT SELECT ON pcb_inspection.* TO 'winforms_app'@'192.168.0.30';

-- 변경사항 적용
FLUSH PRIVILEGES;
```

**참고**:
- `192.168.0.30`은 Windows PC의 IP 주소
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
SHOW GRANTS FOR 'flask_server'@'192.168.0.10';
SHOW GRANTS FOR 'winforms_app'@'192.168.0.30';
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
# 변경 후: bind-address = 0.0.0.0  # 모든 IP 허용 (또는 192.168.0.10 등 특정 IP만)

# MySQL 재시작
sudo systemctl restart mysql
```

#### 2. 방화벽 설정 (Ubuntu/Debian)

```bash
# MySQL 포트 (3306) 개방 - 특정 IP만 허용 권장
sudo ufw allow from 192.168.0.10 to any port 3306 comment 'Flask Server'
sudo ufw allow from 192.168.0.30 to any port 3306 comment 'Windows PC'

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
ALTER USER 'flask_server'@'192.168.0.10' IDENTIFIED BY 'NEW_STRONG_PASSWORD';
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

**작성일**: 2025-10-22
**최종 수정일**: 2025-10-23
**버전**: 1.1
**데이터베이스**: MySQL 8.0
**문자 인코딩**: UTF-8 (utf8mb4)
**주요 변경사항**:
- 보안 설정 섹션 추가 (사용자 계정 관리, 권한 설정)
- 네트워크 보안 및 방화벽 설정 추가
- 백업 및 복구 전략 추가
