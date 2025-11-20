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

                    // 동적 쿼리 생성
                    string query = @"
                        SELECT id, camera_id, defect_type, confidence, inspection_time,
                               image_path, boxes, gpio_pin, gpio_duration_ms, user_id, notes
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
                            query += " AND defect_type = @defectType";
                            parameters.Add(new MySqlParameter("@defectType", filter.DefectType));
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
                                    Id = reader.GetInt32("id"),
                                    CameraId = reader.GetString("camera_id"),
                                    DefectType = reader.GetString("defect_type"),
                                    Confidence = reader.GetDouble("confidence"),
                                    InspectionTime = reader.GetDateTime("inspection_time"),
                                    ImagePath = reader.IsDBNull(reader.GetOrdinal("image_path")) ? null : reader.GetString("image_path"),
                                    Boxes = reader.IsDBNull(reader.GetOrdinal("boxes")) ? null : reader.GetString("boxes"),
                                    GpioPin = reader.IsDBNull(reader.GetOrdinal("gpio_pin")) ? (int?)null : reader.GetInt32("gpio_pin"),
                                    GpioDurationMs = reader.IsDBNull(reader.GetOrdinal("gpio_duration_ms")) ? (int?)null : reader.GetInt32("gpio_duration_ms"),
                                    UserId = reader.IsDBNull(reader.GetOrdinal("user_id")) ? (int?)null : reader.GetInt32("user_id"),
                                    Notes = reader.IsDBNull(reader.GetOrdinal("notes")) ? null : reader.GetString("notes")
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

        /// 특정 검사 ID로 검사 이력 조회
        /// <param name="id">검사 ID</param>
        /// <returns>검사 이력 객체 (없으면 null)</returns>
        public Inspection GetInspectionById(int id)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    string query = @"
                        SELECT id, camera_id, defect_type, confidence, inspection_time,
                               image_path, boxes, gpio_pin, gpio_duration_ms, user_id, notes
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
                                    Id = reader.GetInt32("id"),
                                    CameraId = reader.GetString("camera_id"),
                                    DefectType = reader.GetString("defect_type"),
                                    Confidence = reader.GetDouble("confidence"),
                                    InspectionTime = reader.GetDateTime("inspection_time"),
                                    ImagePath = reader.IsDBNull(reader.GetOrdinal("image_path")) ? null : reader.GetString("image_path"),
                                    Boxes = reader.IsDBNull(reader.GetOrdinal("boxes")) ? null : reader.GetString("boxes"),
                                    GpioPin = reader.IsDBNull(reader.GetOrdinal("gpio_pin")) ? (int?)null : reader.GetInt32("gpio_pin"),
                                    GpioDurationMs = reader.IsDBNull(reader.GetOrdinal("gpio_duration_ms")) ? (int?)null : reader.GetInt32("gpio_duration_ms"),
                                    UserId = reader.IsDBNull(reader.GetOrdinal("user_id")) ? (int?)null : reader.GetInt32("user_id"),
                                    Notes = reader.IsDBNull(reader.GetOrdinal("notes")) ? null : reader.GetString("notes")
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
                            query += " AND defect_type = @defectType";
                            parameters.Add(new MySqlParameter("@defectType", filter.DefectType));
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
        public List<HourlyStatistics> GetHourlyStatistics(DateTime start, DateTime end)    //민준코드
        {
            var list = new List<HourlyStatistics>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    string query = @"
                SELECT
                    stat_datetime,
                    total_inspections,
                    normal_count,
                    component_defect_count,
                    solder_defect_count,
                    discard_count,
                    created_at,
                    updated_at
                FROM statistics_hourly
                WHERE stat_datetime >= @start AND stat_datetime < @end
                ORDER BY stat_datetime;
            ";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@start", start);
                        cmd.Parameters.AddWithValue("@end", end);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var item = new HourlyStatistics
                                {
                                    StatDatetime = reader.GetDateTime("stat_datetime"),
                                    TotalInspections = reader.GetInt32("total_inspections"),
                                    NormalCount = reader.GetInt32("normal_count"),
                                    ComponentDefectCount = reader.GetInt32("component_defect_count"),
                                    SolderDefectCount = reader.GetInt32("solder_defect_count"),
                                    DiscardCount = reader.GetInt32("discard_count"),
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

                    string query = @"
                        SELECT
                            COUNT(*) as total,
                            SUM(CASE WHEN defect_type = '정상' THEN 1 ELSE 0 END) as normal,
                            SUM(CASE WHEN defect_type = '부품불량' THEN 1 ELSE 0 END) as component,
                            SUM(CASE WHEN defect_type = '납땜불량' THEN 1 ELSE 0 END) as solder,
                            SUM(CASE WHEN defect_type = '폐기' THEN 1 ELSE 0 END) as discard
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
