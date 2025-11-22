using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;

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

    }
}
