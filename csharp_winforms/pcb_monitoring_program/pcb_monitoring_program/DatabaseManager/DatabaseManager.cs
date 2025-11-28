using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using pcb_monitoring_program.DatabaseManager.Models;
using pcb_monitoring_program.DatabaseManager.Filters;

namespace pcb_monitoring_program.DatabaseManager
{
    /// PCB 검사 시스템 데이터베이스 관리자
    /// MySQL 데이터베이스와의 모든 상호작용을 담당
    
    public class DatabaseManager : IDisposable
    {
        private readonly string _connectionString;
        private bool _disposed = false;

        #region 생성자 및 연결 관리

        /// DatabaseManager 생성자
        /// <param name="server">MySQL 서버 주소</param>
        /// <param name="database">데이터베이스 이름</param>
        /// <param name="user">사용자명</param>
        /// <param name="password">비밀번호</param>
        public DatabaseManager(string server, string database, string user, string password)
        {
            _connectionString = $"Server={server};Database={database};Uid={user};Pwd={password};CharSet=utf8mb4;";
        }

        /// ConnectionString을 직접 사용하는 생성자
        public DatabaseManager(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// 데이터베이스 연결 테스트
        /// <returns>연결 성공 여부</returns>
        public bool TestConnection()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    return conn.State == ConnectionState.Open;
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"[DB 연결 테스트 실패] {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB 연결 오류] {ex.Message}");
                return false;
            }
        }

        #endregion

        #region 검사 이력 (Inspections)

        /// 검사 이력 조회 (페이징 및 필터 지원)
        /// <param name="page">페이지 번호 (1부터 시작)</param>
        /// <param name="pageSize">페이지 크기</param>
        /// <param name="filter">필터 조건</param>
        /// <returns>검사 이력 리스트</returns>
        public List<Inspection> GetInspections(int page = 1, int pageSize = 50, InspectionFilter filter = null)
        {
            List<Inspection> inspections = new List<Inspection>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    // v3.0 스키마에 맞춘 쿼리 (30+ 컬럼)
                    string query = @"
                        SELECT id, serial_number, product_code, qr_data, qr_detected, serial_detected,
                               decision, missing_count, position_error_count, extra_count, correct_count,
                               missing_components, position_errors, extra_components,
                               yolo_detections, detection_count, avg_confidence,
                               inference_time_ms, verification_time_ms, total_time_ms,
                               left_image_path, right_image_path, image_width, image_height,
                               camera_id, client_ip, server_version,
                               user_id, notes, inspection_time, created_at
                        FROM inspections
                        WHERE 1=1";

                    List<MySqlParameter> parameters = new List<MySqlParameter>();

                    // 필터 적용
                    if (filter != null)
                    {
                        if (filter.StartDate.HasValue)
                        {
                            query += " AND inspection_time >= @startDate";
                            parameters.Add(new MySqlParameter("@startDate", filter.StartDate.Value));
                        }

                        if (filter.EndDate.HasValue)
                        {
                            query += " AND inspection_time <= @endDate";
                            parameters.Add(new MySqlParameter("@endDate", filter.EndDate.Value));
                        }

                        if (!string.IsNullOrEmpty(filter.DefectType))
                        {
                            query += " AND decision = @decision";
                            parameters.Add(new MySqlParameter("@decision", filter.DefectType));
                        }

                        if (!string.IsNullOrEmpty(filter.CameraId))
                        {
                            query += " AND camera_id = @cameraId";
                            parameters.Add(new MySqlParameter("@cameraId", filter.CameraId));
                        }

                        if (filter.UserId.HasValue)
                        {
                            query += " AND user_id = @userId";
                            parameters.Add(new MySqlParameter("@userId", filter.UserId.Value));
                        }
                    }

                    // 정렬 및 페이징
                    query += " ORDER BY inspection_time DESC LIMIT @pageSize OFFSET @offset";
                    parameters.Add(new MySqlParameter("@pageSize", pageSize));
                    parameters.Add(new MySqlParameter("@offset", (page - 1) * pageSize));

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddRange(parameters.ToArray());

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                inspections.Add(new Inspection
                                {
                                    Id = reader.GetInt64("id"),
                                    SerialNumber = reader.GetString("serial_number"),
                                    ProductCode = reader.GetString("product_code"),
                                    QrData = reader.IsDBNull(reader.GetOrdinal("qr_data")) ? null : reader.GetString("qr_data"),
                                    QrDetected = reader.GetBoolean("qr_detected"),
                                    SerialDetected = reader.GetBoolean("serial_detected"),
                                    Decision = reader.GetString("decision"),
                                    MissingCount = reader.GetInt32("missing_count"),
                                    PositionErrorCount = reader.GetInt32("position_error_count"),
                                    ExtraCount = reader.GetInt32("extra_count"),
                                    CorrectCount = reader.GetInt32("correct_count"),
                                    MissingComponents = reader.IsDBNull(reader.GetOrdinal("missing_components")) ? null : reader.GetString("missing_components"),
                                    PositionErrors = reader.IsDBNull(reader.GetOrdinal("position_errors")) ? null : reader.GetString("position_errors"),
                                    ExtraComponents = reader.IsDBNull(reader.GetOrdinal("extra_components")) ? null : reader.GetString("extra_components"),
                                    YoloDetections = reader.IsDBNull(reader.GetOrdinal("yolo_detections")) ? null : reader.GetString("yolo_detections"),
                                    DetectionCount = reader.GetInt32("detection_count"),
                                    AvgConfidence = reader.IsDBNull(reader.GetOrdinal("avg_confidence")) ? (float?)null : reader.GetFloat("avg_confidence"),
                                    InferenceTimeMs = reader.IsDBNull(reader.GetOrdinal("inference_time_ms")) ? (float?)null : reader.GetFloat("inference_time_ms"),
                                    VerificationTimeMs = reader.IsDBNull(reader.GetOrdinal("verification_time_ms")) ? (float?)null : reader.GetFloat("verification_time_ms"),
                                    TotalTimeMs = reader.IsDBNull(reader.GetOrdinal("total_time_ms")) ? (float?)null : reader.GetFloat("total_time_ms"),
                                    LeftImagePath = reader.IsDBNull(reader.GetOrdinal("left_image_path")) ? null : reader.GetString("left_image_path"),
                                    RightImagePath = reader.IsDBNull(reader.GetOrdinal("right_image_path")) ? null : reader.GetString("right_image_path"),
                                    ImageWidth = reader.IsDBNull(reader.GetOrdinal("image_width")) ? (int?)null : reader.GetInt32("image_width"),
                                    ImageHeight = reader.IsDBNull(reader.GetOrdinal("image_height")) ? (int?)null : reader.GetInt32("image_height"),
                                    CameraId = reader.IsDBNull(reader.GetOrdinal("camera_id")) ? null : reader.GetString("camera_id"),
                                    ClientIp = reader.IsDBNull(reader.GetOrdinal("client_ip")) ? null : reader.GetString("client_ip"),
                                    ServerVersion = reader.IsDBNull(reader.GetOrdinal("server_version")) ? null : reader.GetString("server_version"),
                                    UserId = reader.IsDBNull(reader.GetOrdinal("user_id")) ? (int?)null : reader.GetInt32("user_id"),
                                    Notes = reader.IsDBNull(reader.GetOrdinal("notes")) ? null : reader.GetString("notes"),
                                    InspectionTime = reader.GetDateTime("inspection_time"),
                                    CreatedAt = reader.GetDateTime("created_at")
                                });
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"[검사 이력 조회 실패] {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[검사 이력 조회 오류] {ex.Message}");
            }

            return inspections;
        }

        /// 특정 검사 ID로 검사 이력 조회 (v3.0 스키마)
        /// <param name="id">검사 ID (BIGINT)</param>
        /// <returns>검사 이력 객체 (없으면 null)</returns>
        public Inspection GetInspectionById(long id)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    string query = @"
                        SELECT id, serial_number, product_code, qr_data, qr_detected, serial_detected,
                               decision, missing_count, position_error_count, extra_count, correct_count,
                               missing_components, position_errors, extra_components,
                               yolo_detections, detection_count, avg_confidence,
                               inference_time_ms, verification_time_ms, total_time_ms,
                               left_image_path, right_image_path, image_width, image_height,
                               camera_id, client_ip, server_version,
                               user_id, notes, inspection_time, created_at
                        FROM inspections
                        WHERE id = @id";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Inspection
                                {
                                    Id = reader.GetInt64("id"),
                                    SerialNumber = reader.GetString("serial_number"),
                                    ProductCode = reader.GetString("product_code"),
                                    QrData = reader.IsDBNull(reader.GetOrdinal("qr_data")) ? null : reader.GetString("qr_data"),
                                    QrDetected = reader.GetBoolean("qr_detected"),
                                    SerialDetected = reader.GetBoolean("serial_detected"),
                                    Decision = reader.GetString("decision"),
                                    MissingCount = reader.GetInt32("missing_count"),
                                    PositionErrorCount = reader.GetInt32("position_error_count"),
                                    ExtraCount = reader.GetInt32("extra_count"),
                                    CorrectCount = reader.GetInt32("correct_count"),
                                    MissingComponents = reader.IsDBNull(reader.GetOrdinal("missing_components")) ? null : reader.GetString("missing_components"),
                                    PositionErrors = reader.IsDBNull(reader.GetOrdinal("position_errors")) ? null : reader.GetString("position_errors"),
                                    ExtraComponents = reader.IsDBNull(reader.GetOrdinal("extra_components")) ? null : reader.GetString("extra_components"),
                                    YoloDetections = reader.IsDBNull(reader.GetOrdinal("yolo_detections")) ? null : reader.GetString("yolo_detections"),
                                    DetectionCount = reader.GetInt32("detection_count"),
                                    AvgConfidence = reader.IsDBNull(reader.GetOrdinal("avg_confidence")) ? (float?)null : reader.GetFloat("avg_confidence"),
                                    InferenceTimeMs = reader.IsDBNull(reader.GetOrdinal("inference_time_ms")) ? (float?)null : reader.GetFloat("inference_time_ms"),
                                    VerificationTimeMs = reader.IsDBNull(reader.GetOrdinal("verification_time_ms")) ? (float?)null : reader.GetFloat("verification_time_ms"),
                                    TotalTimeMs = reader.IsDBNull(reader.GetOrdinal("total_time_ms")) ? (float?)null : reader.GetFloat("total_time_ms"),
                                    LeftImagePath = reader.IsDBNull(reader.GetOrdinal("left_image_path")) ? null : reader.GetString("left_image_path"),
                                    RightImagePath = reader.IsDBNull(reader.GetOrdinal("right_image_path")) ? null : reader.GetString("right_image_path"),
                                    ImageWidth = reader.IsDBNull(reader.GetOrdinal("image_width")) ? (int?)null : reader.GetInt32("image_width"),
                                    ImageHeight = reader.IsDBNull(reader.GetOrdinal("image_height")) ? (int?)null : reader.GetInt32("image_height"),
                                    CameraId = reader.IsDBNull(reader.GetOrdinal("camera_id")) ? null : reader.GetString("camera_id"),
                                    ClientIp = reader.IsDBNull(reader.GetOrdinal("client_ip")) ? null : reader.GetString("client_ip"),
                                    ServerVersion = reader.IsDBNull(reader.GetOrdinal("server_version")) ? null : reader.GetString("server_version"),
                                    UserId = reader.IsDBNull(reader.GetOrdinal("user_id")) ? (int?)null : reader.GetInt32("user_id"),
                                    Notes = reader.IsDBNull(reader.GetOrdinal("notes")) ? null : reader.GetString("notes"),
                                    InspectionTime = reader.GetDateTime("inspection_time"),
                                    CreatedAt = reader.GetDateTime("created_at")
                                };
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"[검사 조회 실패 - ID: {id}] {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[검사 조회 오류 - ID: {id}] {ex.Message}");
            }

            return null;
        }

        /// 검사 이력 총 개수 조회 (필터 지원)
        /// <param name="filter">필터 조건</param>
        /// <returns>총 개수</returns>
        public int GetTotalInspectionCount(InspectionFilter filter = null)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    string query = "SELECT COUNT(*) FROM inspections WHERE 1=1";
                    List<MySqlParameter> parameters = new List<MySqlParameter>();

                    // 필터 적용
                    if (filter != null)
                    {
                        if (filter.StartDate.HasValue)
                        {
                            query += " AND inspection_time >= @startDate";
                            parameters.Add(new MySqlParameter("@startDate", filter.StartDate.Value));
                        }

                        if (filter.EndDate.HasValue)
                        {
                            query += " AND inspection_time <= @endDate";
                            parameters.Add(new MySqlParameter("@endDate", filter.EndDate.Value));
                        }

                        if (!string.IsNullOrEmpty(filter.DefectType))
                        {
                            query += " AND decision = @decision";
                            parameters.Add(new MySqlParameter("@decision", filter.DefectType));
                        }

                        if (!string.IsNullOrEmpty(filter.CameraId))
                        {
                            query += " AND camera_id = @cameraId";
                            parameters.Add(new MySqlParameter("@cameraId", filter.CameraId));
                        }

                        if (filter.UserId.HasValue)
                        {
                            query += " AND user_id = @userId";
                            parameters.Add(new MySqlParameter("@userId", filter.UserId.Value));
                        }
                    }

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddRange(parameters.ToArray());
                        return Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"[검사 개수 조회 실패] {ex.Message}");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[검사 개수 조회 오류] {ex.Message}");
                return 0;
            }
        }

        #endregion

        #region 통계 (Statistics)

        /// 기간별 통계 조회
        /// <param name="startDate">시작 날짜</param>
        /// <param name="endDate">종료 날짜</param>
        /// <returns>통계 객체</returns>
        public List<DailyStatistics> GetDailyStatisticsForYear(int year)    //민준코드
        {
            List<DailyStatistics> list = new List<DailyStatistics>();

            DateTime startDate = new DateTime(year, 1, 1);
            DateTime endDate = new DateTime(year, 12, 31);

            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    string query = @"
                SELECT
                    stat_date,
                    total_inspections,
                    normal_count,
                    component_defect_count,
                    solder_defect_count,
                    discard_count,
                    defect_rate,
                    created_at,
                    updated_at
                FROM statistics_daily
                WHERE stat_date BETWEEN @startDate AND @endDate
                ORDER BY stat_date ASC";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@startDate", startDate);
                        cmd.Parameters.AddWithValue("@endDate", endDate);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var d = new DailyStatistics
                                {
                                    StatDate = reader.GetDateTime("stat_date").Date,
                                    TotalInspections = reader.GetInt32("total_inspections"),
                                    NormalCount = reader.GetInt32("normal_count"),
                                    ComponentDefectCount = reader.GetInt32("component_defect_count"),
                                    SolderDefectCount = reader.GetInt32("solder_defect_count"),
                                    DiscardCount = reader.GetInt32("discard_count"),
                                    DefectRate = reader.IsDBNull(reader.GetOrdinal("defect_rate"))
                                                            ? 0.0
                                                            : (double)reader.GetDecimal("defect_rate"),
                                    CreatedAt = reader.GetDateTime("created_at"),
                                    UpdatedAt = reader.GetDateTime("updated_at"),
                                };

                                list.Add(d);
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"[일별 통계 조회 실패] {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[일별 통계 조회 오류] {ex.Message}");
            }

            return list;
        }
        /// <summary>
        /// 시간별 통계 조회 (v3.0 스키마: inspection_summary_hourly)
        /// </summary>
        /// <param name="start">시작 시간</param>
        /// <param name="end">종료 시간</param>
        /// <param name="productCode">제품 코드 필터 (선택, null이면 전체)</param>
        /// <returns>시간별 통계 리스트</returns>
        public List<HourlyStatistics> GetHourlyStatistics(DateTime start, DateTime end, string productCode = null)
        {
            var list = new List<HourlyStatistics>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    // v3.0 스키마: inspection_summary_hourly 테이블 사용
                    // missing_count → ComponentDefectCount
                    // position_error_count → SolderDefectCount
                    string query = @"
                        SELECT
                            id,
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
                            avg_confidence,
                            defect_rate,
                            created_at,
                            updated_at
                        FROM inspection_summary_hourly
                        WHERE hour_timestamp >= @start AND hour_timestamp < @end";

                    // 제품 코드 필터 추가 (선택적)
                    if (!string.IsNullOrEmpty(productCode))
                    {
                        query += " AND product_code = @productCode";
                    }

                    query += " ORDER BY hour_timestamp, product_code;";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@start", start);
                        cmd.Parameters.AddWithValue("@end", end);

                        if (!string.IsNullOrEmpty(productCode))
                        {
                            cmd.Parameters.AddWithValue("@productCode", productCode);
                        }

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var item = new HourlyStatistics
                                {
                                    Id = reader.GetInt64("id"),
                                    StatDatetime = reader.GetDateTime("hour_timestamp"),
                                    ProductCode = reader.GetString("product_code"),
                                    TotalInspections = reader.GetInt32("total_inspections"),
                                    NormalCount = reader.GetInt32("normal_count"),
                                    ComponentDefectCount = reader.GetInt32("missing_count"),  // missing → 부품누락
                                    SolderDefectCount = reader.GetInt32("position_error_count"),  // position_error → 위치오류
                                    DiscardCount = reader.GetInt32("discard_count"),
                                    AvgInferenceTimeMs = reader.IsDBNull(reader.GetOrdinal("avg_inference_time_ms"))
                                        ? (float?)null
                                        : reader.GetFloat("avg_inference_time_ms"),
                                    AvgTotalTimeMs = reader.IsDBNull(reader.GetOrdinal("avg_total_time_ms"))
                                        ? (float?)null
                                        : reader.GetFloat("avg_total_time_ms"),
                                    AvgDetectionCount = reader.IsDBNull(reader.GetOrdinal("avg_detection_count"))
                                        ? (float?)null
                                        : reader.GetFloat("avg_detection_count"),
                                    AvgConfidence = reader.IsDBNull(reader.GetOrdinal("avg_confidence"))
                                        ? (float?)null
                                        : reader.GetFloat("avg_confidence"),
                                    DefectRate = reader.GetFloat("defect_rate"),
                                    CreatedAt = reader.GetDateTime("created_at"),
                                    UpdatedAt = reader.GetDateTime("updated_at")
                                };

                                list.Add(item);
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"[시간별 통계 조회 실패] {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[시간별 통계 조회 오류] {ex.Message}");
            }

            return list;
        }

        public Statistics GetStatistics(DateTime startDate, DateTime endDate)
        {
            Statistics stats = new Statistics
            {
                StartDate = startDate,
                EndDate = endDate
            };

            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    // v3.0 스키마: decision 컬럼 사용 (normal/missing/position_error/discard)
                    string query = @"
                        SELECT
                            COUNT(*) as total,
                            SUM(CASE WHEN decision = 'normal' THEN 1 ELSE 0 END) as normal,
                            SUM(CASE WHEN decision = 'missing' THEN 1 ELSE 0 END) as component,
                            SUM(CASE WHEN decision = 'position_error' THEN 1 ELSE 0 END) as solder,
                            SUM(CASE WHEN decision = 'discard' THEN 1 ELSE 0 END) as discard
                        FROM inspections
                        WHERE inspection_time BETWEEN @startDate AND @endDate";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@startDate", startDate);
                        cmd.Parameters.AddWithValue("@endDate", endDate);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                stats.TotalInspections = reader.GetInt32("total");
                                stats.NormalCount = reader.GetInt32("normal");
                                stats.ComponentDefectCount = reader.GetInt32("component");
                                stats.SolderDefectCount = reader.GetInt32("solder");
                                stats.DiscardCount = reader.GetInt32("discard");

                                stats.CalculateDefectRate();
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"[통계 조회 실패] {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[통계 조회 오류] {ex.Message}");
            }

            return stats;
        }

        #endregion

        #region 사용자 관리 (Users)

        /// 사용자 로그인 검증 (BCrypt 해싱)
        /// <param name="username">사용자명</param>
        /// <param name="password">비밀번호 (평문)</param>
        /// <returns>로그인 성공 시 User 객체, 실패 시 null</returns>
        public User ValidateLogin(string username, string password)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    string query = @"
                        SELECT id, username, password_hash, full_name, role, is_active, last_login, created_at
                        FROM users
                        WHERE username = @username AND is_active = TRUE";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string passwordHash = reader.GetString("password_hash");

                                // BCrypt 비밀번호 검증
                                if (BCrypt.Net.BCrypt.Verify(password, passwordHash))
                                {
                                    User user = new User
                                    {
                                        Id = reader.GetInt32("id"),
                                        Username = reader.GetString("username"),
                                        PasswordHash = passwordHash,
                                        FullName = reader.IsDBNull(reader.GetOrdinal("full_name")) ? null : reader.GetString("full_name"),
                                        Role = Enum.Parse<UserRole>(reader.GetString("role"), true),
                                        IsActive = reader.GetBoolean("is_active"),
                                        LastLogin = reader.IsDBNull(reader.GetOrdinal("last_login")) ? (DateTime?)null : reader.GetDateTime("last_login"),
                                        CreatedAt = reader.GetDateTime("created_at")
                                    };

                                    // 로그인 시간 업데이트 (별도 쿼리)
                                    UpdateLastLogin(user.Id);

                                    return user;
                                }
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"[로그인 검증 실패] {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[로그인 검증 오류] {ex.Message}");
            }

            return null;
        }

        /// 마지막 로그인 시간 업데이트
        private void UpdateLastLogin(int userId)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    string query = "UPDATE users SET last_login = @now WHERE id = @id";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@now", DateTime.Now);
                        cmd.Parameters.AddWithValue("@id", userId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[마지막 로그인 시간 업데이트 실패] {ex.Message}");
            }
        }

        #endregion

        #region 상세 불량 통계 (Defect Details)

        /// 상세 불량 유형별 통계 조회 (TOP N)
        /// <param name="startDate">시작 날짜</param>
        /// <param name="endDate">종료 날짜</param>
        /// <param name="topN">상위 N개 (기본값: 7)</param>
        /// <returns>상세 불량 통계 리스트 (클래스명, 총 개수, 평균 신뢰도)</returns>
        public List<(string ClassName, int TotalCount, decimal AvgConfidence)> GetDefectDetailStatistics(
            DateTime startDate,
            DateTime endDate,
            int topN = 7)
        {
            List<(string, int, decimal)> statistics = new List<(string, int, decimal)>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    string query = @"
                        SELECT
                            dd.class_name,
                            SUM(dd.count) as total_count,
                            AVG(dd.avg_confidence) as avg_confidence
                        FROM defect_details dd
                        INNER JOIN inspections i ON dd.inspection_id = i.id
                        WHERE i.inspection_time BETWEEN @startDate AND @endDate
                        GROUP BY dd.class_name
                        ORDER BY total_count DESC
                        LIMIT @topN";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@startDate", startDate);
                        cmd.Parameters.AddWithValue("@endDate", endDate);
                        cmd.Parameters.AddWithValue("@topN", topN);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string className = reader.GetString("class_name");
                                int totalCount = reader.GetInt32("total_count");
                                decimal avgConfidence = reader.IsDBNull(reader.GetOrdinal("avg_confidence"))
                                    ? 0.0m
                                    : reader.GetDecimal("avg_confidence");

                                statistics.Add((className, totalCount, avgConfidence));
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"[상세 불량 통계 조회 실패] {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[상세 불량 통계 조회 오류] {ex.Message}");
            }

            return statistics;
        }

        #endregion

        #region 불량률 추이 이력 (Defect Rate History)

        /// 불량률 추이 이력 조회 (시간대별)
        /// <param name="startDate">시작 날짜</param>
        /// <param name="endDate">종료 날짜</param>
        /// <returns>불량률 추이 이력 리스트</returns>
        public List<DefectRateHistory> GetDefectRateHistory(
            DateTime startDate,
            DateTime endDate)
        {
            List<DefectRateHistory> history = new List<DefectRateHistory>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    string query = @"
                        SELECT id, defect_rate, total_inspections, defect_count, recorded_at
                        FROM defect_rate_history
                        WHERE recorded_at BETWEEN @startDate AND @endDate
                        ORDER BY recorded_at ASC";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@startDate", startDate);
                        cmd.Parameters.AddWithValue("@endDate", endDate);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                history.Add(new DefectRateHistory
                                {
                                    Id = reader.GetInt32("id"),
                                    DefectRate = reader.GetDecimal("defect_rate"),
                                    TotalInspections = reader.GetInt32("total_inspections"),
                                    DefectCount = reader.GetInt32("defect_count"),
                                    RecordedAt = reader.GetDateTime("recorded_at")
                                });
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"[불량률 추이 이력 조회 실패] {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[불량률 추이 이력 조회 오류] {ex.Message}");
            }

            return history;
        }

        #endregion

        #region IDisposable 구현

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Managed 리소스 정리 (필요시)
                }

                _disposed = true;
            }
        }

        #endregion

        #region Product 관리 (v3.0 스키마)

        /// 모든 제품 정보 조회
        /// <returns>제품 리스트</returns>
        public List<Product> GetAllProducts()
        {
            List<Product> products = new List<Product>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    string query = @"
                        SELECT product_code, product_name, description, serial_prefix,
                               component_count, qr_url_template, created_at, updated_at
                        FROM products
                        ORDER BY product_code";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            products.Add(new Product
                            {
                                ProductCode = reader.GetString("product_code"),
                                ProductName = reader.GetString("product_name"),
                                Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString("description"),
                                SerialPrefix = reader.GetString("serial_prefix"),
                                ComponentCount = reader.GetInt32("component_count"),
                                QrUrlTemplate = reader.IsDBNull(reader.GetOrdinal("qr_url_template")) ? null : reader.GetString("qr_url_template"),
                                CreatedAt = reader.GetDateTime("created_at"),
                                UpdatedAt = reader.GetDateTime("updated_at")
                            });
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"[제품 목록 조회 실패] {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[제품 목록 조회 오류] {ex.Message}");
            }

            return products;
        }

        /// 특정 제품 코드로 제품 정보 조회
        /// <param name="productCode">제품 코드 (예: FT, RS, BC)</param>
        /// <returns>제품 객체 (없으면 null)</returns>
        public Product GetProductByCode(string productCode)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    string query = @"
                        SELECT product_code, product_name, description, serial_prefix,
                               component_count, qr_url_template, created_at, updated_at
                        FROM products
                        WHERE product_code = @productCode";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@productCode", productCode);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Product
                                {
                                    ProductCode = reader.GetString("product_code"),
                                    ProductName = reader.GetString("product_name"),
                                    Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString("description"),
                                    SerialPrefix = reader.GetString("serial_prefix"),
                                    ComponentCount = reader.GetInt32("component_count"),
                                    QrUrlTemplate = reader.IsDBNull(reader.GetOrdinal("qr_url_template")) ? null : reader.GetString("qr_url_template"),
                                    CreatedAt = reader.GetDateTime("created_at"),
                                    UpdatedAt = reader.GetDateTime("updated_at")
                                };
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"[제품 조회 실패 - Code: {productCode}] {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[제품 조회 오류 - Code: {productCode}] {ex.Message}");
            }

            return null;
        }

        #endregion

        #region ProductComponent 관리 (v3.0 스키마)

        /// 특정 제품의 부품 배치 기준 데이터 조회
        /// <param name="productCode">제품 코드</param>
        /// <returns>부품 배치 리스트</returns>
        public List<ProductComponent> GetProductComponents(string productCode)
        {
            List<ProductComponent> components = new List<ProductComponent>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    string query = @"
                        SELECT id, product_code, component_class, center_x, center_y,
                               bbox_x1, bbox_y1, bbox_x2, bbox_y2, tolerance_px, created_at
                        FROM product_components
                        WHERE product_code = @productCode
                        ORDER BY component_class, id";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@productCode", productCode);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                components.Add(new ProductComponent
                                {
                                    Id = reader.GetInt32("id"),
                                    ProductCode = reader.GetString("product_code"),
                                    ComponentClass = reader.GetString("component_class"),
                                    CenterX = reader.GetFloat("center_x"),
                                    CenterY = reader.GetFloat("center_y"),
                                    BboxX1 = reader.GetFloat("bbox_x1"),
                                    BboxY1 = reader.GetFloat("bbox_y1"),
                                    BboxX2 = reader.GetFloat("bbox_x2"),
                                    BboxY2 = reader.GetFloat("bbox_y2"),
                                    TolerancePx = reader.GetFloat("tolerance_px"),
                                    CreatedAt = reader.GetDateTime("created_at")
                                });
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"[부품 배치 조회 실패 - Product: {productCode}] {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[부품 배치 조회 오류 - Product: {productCode}] {ex.Message}");
            }

            return components;
        }

        /// 모든 제품의 부품 배치 기준 데이터 조회
        /// <returns>전체 부품 배치 리스트</returns>
        public List<ProductComponent> GetAllProductComponents()
        {
            List<ProductComponent> components = new List<ProductComponent>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    string query = @"
                        SELECT id, product_code, component_class, center_x, center_y,
                               bbox_x1, bbox_y1, bbox_x2, bbox_y2, tolerance_px, created_at
                        FROM product_components
                        ORDER BY product_code, component_class, id";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            components.Add(new ProductComponent
                            {
                                Id = reader.GetInt32("id"),
                                ProductCode = reader.GetString("product_code"),
                                ComponentClass = reader.GetString("component_class"),
                                CenterX = reader.GetFloat("center_x"),
                                CenterY = reader.GetFloat("center_y"),
                                BboxX1 = reader.GetFloat("bbox_x1"),
                                BboxY1 = reader.GetFloat("bbox_y1"),
                                BboxX2 = reader.GetFloat("bbox_x2"),
                                BboxY2 = reader.GetFloat("bbox_y2"),
                                TolerancePx = reader.GetFloat("tolerance_px"),
                                CreatedAt = reader.GetDateTime("created_at")
                            });
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"[전체 부품 배치 조회 실패] {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[전체 부품 배치 조회 오류] {ex.Message}");
            }

            return components;
        }

        #endregion

        #region InspectionSummary 조회 (v3.0 스키마)

        /// 시간별 검사 집계 조회
        /// <param name="startTime">시작 시간</param>
        /// <param name="endTime">종료 시간</param>
        /// <param name="productCode">제품 코드 (null이면 전체)</param>
        /// <returns>시간별 집계 리스트</returns>
        public List<InspectionSummaryHourly> GetHourlySummary(DateTime startTime, DateTime endTime, string productCode = null)
        {
            List<InspectionSummaryHourly> summaries = new List<InspectionSummaryHourly>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    string query = @"
                        SELECT id, hour_timestamp, product_code, total_inspections,
                               normal_count, missing_count, position_error_count, discard_count,
                               avg_inference_time_ms, avg_total_time_ms, avg_detection_count, avg_confidence,
                               created_at, updated_at
                        FROM inspection_summary_hourly
                        WHERE hour_timestamp BETWEEN @startTime AND @endTime";

                    if (!string.IsNullOrEmpty(productCode))
                    {
                        query += " AND product_code = @productCode";
                    }

                    query += " ORDER BY hour_timestamp DESC";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@startTime", startTime);
                        cmd.Parameters.AddWithValue("@endTime", endTime);
                        if (!string.IsNullOrEmpty(productCode))
                        {
                            cmd.Parameters.AddWithValue("@productCode", productCode);
                        }

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                summaries.Add(new InspectionSummaryHourly
                                {
                                    Id = reader.GetInt64("id"),
                                    HourTimestamp = reader.GetDateTime("hour_timestamp"),
                                    ProductCode = reader.GetString("product_code"),
                                    TotalInspections = reader.GetInt32("total_inspections"),
                                    NormalCount = reader.GetInt32("normal_count"),
                                    MissingCount = reader.GetInt32("missing_count"),
                                    PositionErrorCount = reader.GetInt32("position_error_count"),
                                    DiscardCount = reader.GetInt32("discard_count"),
                                    AvgInferenceTimeMs = reader.IsDBNull(reader.GetOrdinal("avg_inference_time_ms")) ? (float?)null : reader.GetFloat("avg_inference_time_ms"),
                                    AvgTotalTimeMs = reader.IsDBNull(reader.GetOrdinal("avg_total_time_ms")) ? (float?)null : reader.GetFloat("avg_total_time_ms"),
                                    AvgDetectionCount = reader.IsDBNull(reader.GetOrdinal("avg_detection_count")) ? (float?)null : reader.GetFloat("avg_detection_count"),
                                    AvgConfidence = reader.IsDBNull(reader.GetOrdinal("avg_confidence")) ? (float?)null : reader.GetFloat("avg_confidence"),
                                    CreatedAt = reader.GetDateTime("created_at"),
                                    UpdatedAt = reader.GetDateTime("updated_at")
                                });
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"[시간별 집계 조회 실패] {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[시간별 집계 조회 오류] {ex.Message}");
            }

            return summaries;
        }

        /// 일별 검사 집계 조회
        /// <param name="startDate">시작 날짜</param>
        /// <param name="endDate">종료 날짜</param>
        /// <param name="productCode">제품 코드 (null이면 전체)</param>
        /// <returns>일별 집계 리스트</returns>
        public List<InspectionSummaryDaily> GetDailySummary(DateTime startDate, DateTime endDate, string productCode = null)
        {
            List<InspectionSummaryDaily> summaries = new List<InspectionSummaryDaily>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    string query = @"
                        SELECT id, date, product_code, total_inspections,
                               normal_count, missing_count, position_error_count, discard_count,
                               avg_inference_time_ms, avg_total_time_ms, avg_detection_count, avg_confidence,
                               created_at, updated_at
                        FROM inspection_summary_daily
                        WHERE date BETWEEN @startDate AND @endDate";

                    if (!string.IsNullOrEmpty(productCode))
                    {
                        query += " AND product_code = @productCode";
                    }

                    query += " ORDER BY date DESC";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@startDate", startDate.Date);
                        cmd.Parameters.AddWithValue("@endDate", endDate.Date);
                        if (!string.IsNullOrEmpty(productCode))
                        {
                            cmd.Parameters.AddWithValue("@productCode", productCode);
                        }

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                summaries.Add(new InspectionSummaryDaily
                                {
                                    Id = reader.GetInt64("id"),
                                    Date = reader.GetDateTime("date"),
                                    ProductCode = reader.GetString("product_code"),
                                    TotalInspections = reader.GetInt32("total_inspections"),
                                    NormalCount = reader.GetInt32("normal_count"),
                                    MissingCount = reader.GetInt32("missing_count"),
                                    PositionErrorCount = reader.GetInt32("position_error_count"),
                                    DiscardCount = reader.GetInt32("discard_count"),
                                    AvgInferenceTimeMs = reader.IsDBNull(reader.GetOrdinal("avg_inference_time_ms")) ? (float?)null : reader.GetFloat("avg_inference_time_ms"),
                                    AvgTotalTimeMs = reader.IsDBNull(reader.GetOrdinal("avg_total_time_ms")) ? (float?)null : reader.GetFloat("avg_total_time_ms"),
                                    AvgDetectionCount = reader.IsDBNull(reader.GetOrdinal("avg_detection_count")) ? (float?)null : reader.GetFloat("avg_detection_count"),
                                    AvgConfidence = reader.IsDBNull(reader.GetOrdinal("avg_confidence")) ? (float?)null : reader.GetFloat("avg_confidence"),
                                    CreatedAt = reader.GetDateTime("created_at"),
                                    UpdatedAt = reader.GetDateTime("updated_at")
                                });
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"[일별 집계 조회 실패] {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[일별 집계 조회 오류] {ex.Message}");
            }

            return summaries;
        }

        /// 월별 검사 집계 조회
        /// <param name="startYear">시작 년도</param>
        /// <param name="startMonth">시작 월</param>
        /// <param name="endYear">종료 년도</param>
        /// <param name="endMonth">종료 월</param>
        /// <param name="productCode">제품 코드 (null이면 전체)</param>
        /// <returns>월별 집계 리스트</returns>
        public List<InspectionSummaryMonthly> GetMonthlySummary(int startYear, int startMonth, int endYear, int endMonth, string productCode = null)
        {
            List<InspectionSummaryMonthly> summaries = new List<InspectionSummaryMonthly>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    string query = @"
                        SELECT id, year, month, product_code, total_inspections,
                               normal_count, missing_count, position_error_count, discard_count,
                               avg_inference_time_ms, avg_total_time_ms, avg_detection_count, avg_confidence,
                               created_at, updated_at
                        FROM inspection_summary_monthly
                        WHERE (year > @startYear OR (year = @startYear AND month >= @startMonth))
                          AND (year < @endYear OR (year = @endYear AND month <= @endMonth))";

                    if (!string.IsNullOrEmpty(productCode))
                    {
                        query += " AND product_code = @productCode";
                    }

                    query += " ORDER BY year DESC, month DESC";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@startYear", startYear);
                        cmd.Parameters.AddWithValue("@startMonth", startMonth);
                        cmd.Parameters.AddWithValue("@endYear", endYear);
                        cmd.Parameters.AddWithValue("@endMonth", endMonth);
                        if (!string.IsNullOrEmpty(productCode))
                        {
                            cmd.Parameters.AddWithValue("@productCode", productCode);
                        }

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                summaries.Add(new InspectionSummaryMonthly
                                {
                                    Id = reader.GetInt64("id"),
                                    Year = reader.GetInt32("year"),
                                    Month = reader.GetInt32("month"),
                                    ProductCode = reader.GetString("product_code"),
                                    TotalInspections = reader.GetInt32("total_inspections"),
                                    NormalCount = reader.GetInt32("normal_count"),
                                    MissingCount = reader.GetInt32("missing_count"),
                                    PositionErrorCount = reader.GetInt32("position_error_count"),
                                    DiscardCount = reader.GetInt32("discard_count"),
                                    AvgInferenceTimeMs = reader.IsDBNull(reader.GetOrdinal("avg_inference_time_ms")) ? (float?)null : reader.GetFloat("avg_inference_time_ms"),
                                    AvgTotalTimeMs = reader.IsDBNull(reader.GetOrdinal("avg_total_time_ms")) ? (float?)null : reader.GetFloat("avg_total_time_ms"),
                                    AvgDetectionCount = reader.IsDBNull(reader.GetOrdinal("avg_detection_count")) ? (float?)null : reader.GetFloat("avg_detection_count"),
                                    AvgConfidence = reader.IsDBNull(reader.GetOrdinal("avg_confidence")) ? (float?)null : reader.GetFloat("avg_confidence"),
                                    CreatedAt = reader.GetDateTime("created_at"),
                                    UpdatedAt = reader.GetDateTime("updated_at")
                                });
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"[월별 집계 조회 실패] {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[월별 집계 조회 오류] {ex.Message}");
            }

            return summaries;
        }

        #endregion

        #region 박스 상태 (BoxStatus)

        /// 모든 박스 상태 조회 (NORMAL / COMPONENT_DEFECT / SOLDER_DEFECT)
        public List<BoxStatus> GetAllBoxStatus()
        {
            var list = new List<BoxStatus>();

            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    string sql = @"
                SELECT
                    id, box_id, category,
                    current_slot, max_slots,
                    is_full, total_pcb_count,
                    created_at, last_updated
                FROM box_status";

                    using (var cmd = new MySqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var item = new BoxStatus
                            {
                                Id = reader.GetInt32("id"),
                                BoxId = reader.GetString("box_id"),
                                Category = reader.GetString("category"),
                                CurrentSlot = reader.GetInt32("current_slot"),
                                MaxSlots = reader.GetInt32("max_slots"),
                                IsFull = reader.GetBoolean("is_full"),
                                TotalPcbCount = reader.GetInt32("total_pcb_count"),
                                CreatedAt = reader.GetDateTime("created_at"),
                                LastUpdated = reader.GetDateTime("last_updated")
                            };

                            list.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[GetAllBoxStatus 실패] " + ex.Message);
            }

            return list;
        }

        #endregion
    }
}
