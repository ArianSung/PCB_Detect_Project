using System;
using System.Data;
using MySql.Data.MySqlClient;
using BCrypt.Net;

namespace pcb_monitoring_program.DatabaseManager.Repositories
{
    public class UserRepository
    {
        // 🔌 DB 연결 헬퍼
        private MySqlConnection GetConnection()
        {
            // 기존에 쓰던 DB 헬퍼 클래스 그대로 사용
            return DB.GetConnection();
        }

        // ✅ 전체 사용자 조회 (id 안 가져옴)
        public DataTable GetAllUsers()
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                string sql = @"
                    SELECT
                        username,
                        full_name,
                        role,
                        CASE WHEN is_active THEN '활성' ELSE '비활성' END AS status_text,
                        last_login,
                        created_at
                    FROM users
                    ORDER BY username;
                ";

                using (var cmd = new MySqlCommand(sql, conn))
                using (var da = new MySqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        // ✅ 검색 (아이디 부분검색 + 권한 필터)
        public DataTable SearchUsers(string usernameKeyword, string roleFilter)
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                string sql = @"
                    SELECT
                        username,
                        full_name,
                        role,
                        CASE WHEN is_active THEN '활성' ELSE '비활성' END AS status_text,
                        last_login,
                        created_at
                    FROM users
                    WHERE 1 = 1
                ";

                if (!string.IsNullOrWhiteSpace(usernameKeyword))
                {
                    sql += " AND username LIKE @username ";
                }

                if (!string.IsNullOrWhiteSpace(roleFilter) && roleFilter != "전체")
                {
                    sql += " AND role = @role ";
                }

                sql += " ORDER BY username;";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    if (!string.IsNullOrWhiteSpace(usernameKeyword))
                        cmd.Parameters.AddWithValue("@username", "%" + usernameKeyword + "%");

                    if (!string.IsNullOrWhiteSpace(roleFilter) && roleFilter != "전체")
                        cmd.Parameters.AddWithValue("@role", roleFilter);

                    using (var da = new MySqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        return dt;
                    }
                }
            }
        }

        // ✅ 아이디 중복 확인 (추가/수정 공통)
        // 두 번째 파라미터는 기존 시그니처 맞추려고 남겨둔 더미 파라미터야
        public bool IsUsernameTaken(string username, int ignore = 0)
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                string sql = @"
                    SELECT COUNT(*)
                    FROM users
                    WHERE username = @username;
                ";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    long count = (long)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        // ✅ 사용자 추가
        public bool AddUser(string username, string passwordHash, string fullName, string role, bool isActive)
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                string sql = @"
                    INSERT INTO users
                        (username, password_hash, full_name, role, is_active, created_at)
                    VALUES
                        (@username, @password_hash, @full_name, @role, @is_active, NOW());
                ";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password_hash", passwordHash);
                    cmd.Parameters.AddWithValue("@full_name", fullName);
                    cmd.Parameters.AddWithValue("@role", role);
                    cmd.Parameters.AddWithValue("@is_active", isActive);

                    int affected = cmd.ExecuteNonQuery();
                    return affected > 0;
                }
            }
        }

        // ✅ username 기준 정보 수정 (EditUser 폼에서 사용 예정)
        public bool UpdateUserByUsername(string username, string fullName, string role, bool isActive)
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                string sql = @"
                    UPDATE users
                    SET 
                        full_name = @full_name,
                        role      = @role,
                        is_active = @is_active
                    WHERE username = @username;
                ";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@full_name", fullName);
                    cmd.Parameters.AddWithValue("@role", role);
                    cmd.Parameters.AddWithValue("@is_active", isActive);
                    cmd.Parameters.AddWithValue("@username", username);

                    int affected = cmd.ExecuteNonQuery();
                    return affected > 0;
                }
            }
        }

        // ✅ username 기준 삭제 (지금 View에서 쓰는 DeleteUser(username)용)
        public bool DeleteUser(string username)
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                string sql = @"
                    DELETE FROM users
                    WHERE username = @username;
                ";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    int affected = cmd.ExecuteNonQuery();
                    return affected > 0;
                }
            }
        }

        // ✅ username 기준 비밀번호 초기화
        public bool ResetPassword(string username, string newPlainPassword)
        {
            // AddUser 때와 동일하게 Bcrypt 해시
            string hash = BCrypt.Net.BCrypt.HashPassword(newPlainPassword, workFactor: 12);

            using (var conn = GetConnection())
            {
                conn.Open();

                string sql = @"
                    UPDATE users
                    SET password_hash = @hash
                    WHERE username = @username;
                ";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@hash", hash);
                    cmd.Parameters.AddWithValue("@username", username);

                    int affected = cmd.ExecuteNonQuery();
                    return affected > 0;
                }
            }
        }
    }
}
