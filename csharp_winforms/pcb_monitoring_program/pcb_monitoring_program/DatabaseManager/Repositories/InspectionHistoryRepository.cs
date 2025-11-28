using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;

namespace pcb_monitoring_program.DatabaseManager.Repositories
{
    public class InspectionHistoryRepository
    {
        public DataTable GetAllInspectionHistory()
        {
            using (var conn = DB.GetConnection())
            {
                conn.Open();

                string query = @"
            SELECT
                inspection_time  AS '검사 시각',
                camera_id        AS '카메라 ID',
                defect_type      AS '불량 유형',
                confidence       AS '정확도',
                boxes            AS '박스',
                image_path       AS '이미지 경로',
                notes            AS '내용'
            FROM inspections
            ORDER BY inspection_time DESC;
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
    }
}
