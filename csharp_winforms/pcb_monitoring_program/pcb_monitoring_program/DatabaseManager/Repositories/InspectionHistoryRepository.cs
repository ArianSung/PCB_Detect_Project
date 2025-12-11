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
                inspection_time        AS `검사 시각`,
                camera_id              AS `카메라 ID`,
                product_code           AS `제품 코드`,    -- 여기 추가됨

                CASE decision
                    WHEN 'normal' THEN '정상'
                    WHEN 'missing' THEN '부품불량'
                    WHEN 'position_error' THEN 'S/N 불량'
                    WHEN 'discard' THEN '폐기'
                    ELSE decision
                END AS `불량 유형`,

                detection_count        AS `검출 개수`,
                avg_confidence         AS `평균 정확도`,

                left_image_path        AS `좌측 이미지`,
                right_image_path       AS `우측 이미지`,

                missing_count          AS `누락 개수`,
                position_error_count   AS `위치오류 개수`,
                extra_count            AS `추가 부품 개수`,
                notes                  AS `내용`
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
