using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;
using BCrypt.Net;

namespace pcb_monitoring_program.DatabaseManager.Repositories
{
    public class UserRepository
    {
        public DataTable GetAllUsers()
        {
            using (var conn = DB.GetConnection())
            {
                conn.Open();

                string query = @"
                    SELECT 
                        id,
                        username,
                        full_name,
                        role,
                        CASE WHEN is_active THEN '활성' ELSE '비활성' END AS status_text,
                        last_login,
                        created_at
                    FROM users
                    ORDER BY id;
                ";

                using (var cmd = new MySqlCommand(query, conn))
                using (var adapter = new MySqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
        }

        public DataTable GetUserById(int id)
        {
            using (var conn = DB.GetConnection())
            {
                conn.Open();
                string query = @"
            SELECT 
                id,
                username,
                full_name,
                role,
                CASE WHEN is_active THEN '활성' ELSE '비활성' END AS status_text,
                last_login,
                created_at
            FROM users
            WHERE id = @id;
        ";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    using (var adapter = new MySqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        return dt;
                    }
                }
            }
        }
        public DataTable GetUserByUsername(string username)
        {
            using (var conn = DB.GetConnection())
            {
                conn.Open();

                string query = @"
            SELECT 
                id,
                username,
                full_name,
                role,
                CASE WHEN is_active THEN '활성' ELSE '비활성' END AS status_text,
                last_login,
                created_at
            FROM users
            WHERE username = @username;
        ";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);

                    using (var adapter = new MySqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        return dt;
                    }
                }
            }
        }
        public DataTable SearchUsers(string username, string roleFilter)
        {
            using (var conn = DB.GetConnection())
            {
                conn.Open();

                string query = @"
            SELECT 
                id,
                username,
                full_name,
                role,
                CASE WHEN is_active THEN '활성' ELSE '비활성' END AS status_text,
                last_login,
                created_at
            FROM users
            WHERE 1 = 1
        ";

                var cmd = new MySqlCommand();
                cmd.Connection = conn;

                // username 부분 검색 (LIKE '%값%')
                if (!string.IsNullOrWhiteSpace(username))
                {
                    query += " AND username LIKE @username";
                    cmd.Parameters.AddWithValue("@username", "%" + username + "%");
                }

                // role 조건 (전체는 필터 안 함)
                if (!string.IsNullOrWhiteSpace(roleFilter) && roleFilter != "전체")
                {
                    query += " AND role = @role";
                    cmd.Parameters.AddWithValue("@role", roleFilter);
                }

                query += " ORDER BY id;";

                cmd.CommandText = query;

                using (var adapter = new MySqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
        }

        public bool AddUser(string username, string passwordHash, string fullName, string role, bool isActive)
        {
            using (var conn = DB.GetConnection())
            {
                conn.Open();

                string query = @"
                INSERT INTO users 
                    (username, password_hash, full_name, role, is_active, created_at)
                VALUES 
                    (@username, @password_hash, @full_name, @role, @is_active, NOW());
            ";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password_hash", passwordHash);
                    cmd.Parameters.AddWithValue("@full_name", fullName);
                    cmd.Parameters.AddWithValue("@role", role);
                    cmd.Parameters.AddWithValue("@is_active", isActive);

                    return cmd.ExecuteNonQuery() == 1;
                }
            }
        }

        public bool UpdateUser(int id, string username, string fullName, string role, bool isActive)
        {
            using (var conn = DB.GetConnection())
            {
                conn.Open();

                string query = @"
            UPDATE users
            SET 
                username   = @username,
                full_name  = @full_name,
                role       = @role,
                is_active  = @is_active
            WHERE id = @id;
        ";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@full_name", fullName);
                    cmd.Parameters.AddWithValue("@role", role);
                    cmd.Parameters.AddWithValue("@is_active", isActive);

                    return cmd.ExecuteNonQuery() == 1;
                }
            }
        }

        public bool DeleteUser(int id)
        {
            const string query = @"DELETE FROM users WHERE id = @id;";

            using (var conn = DB.GetConnection())
            using (var cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@id", id);

                conn.Open();
                int affected = cmd.ExecuteNonQuery();

                return affected > 0; // 삭제된 행이 1개 이상이면 성공
            }
        }

        public static class PasswordHelper
        {
            // 비밀번호 해시 생성
            public static string HashPassword(string password)
            {
                return BCrypt.Net.BCrypt.HashPassword(password);
            }

            // 비밀번호 검증
            public static bool VerifyPassword(string password, string hashedPassword)
            {
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
        }

        public bool ResetPassword(int userId, string newPlainPassword)
        {
            // 1) 평문 → 해시
            string newPasswordHash = PasswordHelper.HashPassword(newPlainPassword);

            const string query = @"
        UPDATE users
        SET password_hash = @NewPasswordHash
        WHERE id = @UserId;
    ";

            using (var conn = DB.GetConnection())
            {
                try
                {
                    conn.Open();

                    using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@NewPasswordHash", newPasswordHash);
                        cmd.Parameters.AddWithValue("@UserId", userId);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
                catch (MySql.Data.MySqlClient.MySqlException ex)
                {
                    Console.WriteLine($"[DB Error] 비밀번호 초기화 실패 (MySQL): {ex.Message}");
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Error] 비밀번호 초기화 중 일반 오류 발생: {ex.Message}");
                    return false;
                }
            }
        }

        public bool IsUsernameTaken(string username, int excludeUserId = 0)
        {
            using (var conn = DB.GetConnection())
            {
                conn.Open();

                // 특정 ID(수정 중인 사용자)는 제외하고 중복 검사
                string query = @"
            SELECT COUNT(*) 
            FROM users 
            WHERE username = @username AND id != @excludeUserId;
        ";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@excludeUserId", excludeUserId);

                    long count = (long)cmd.ExecuteScalar();
                    return count > 0; // 0보다 크면 중복
                }
            }
        }

    }
}
