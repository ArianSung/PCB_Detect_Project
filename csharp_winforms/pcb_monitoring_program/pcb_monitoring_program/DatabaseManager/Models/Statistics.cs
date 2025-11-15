using System;
using System.Collections.Generic;

namespace pcb_monitoring_program.DatabaseManager.Models
{
    /// 통계 데이터 모델
    public class Statistics
    {
        public int TotalInspections { get; set; }
        public int NormalCount { get; set; }
        public int ComponentDefectCount { get; set; }
        public int SolderDefectCount { get; set; }
        public int DiscardCount { get; set; }
        public double DefectRate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // 시간대별 통계 (선택적)
        public Dictionary<DateTime, int> HourlyInspections { get; set; }
        public Dictionary<string, int> DefectTypeCounts { get; set; }

        public Statistics()
        {
            HourlyInspections = new Dictionary<DateTime, int>();
            DefectTypeCounts = new Dictionary<string, int>();
        }

        /// 불량률 계산 (%)
        public void CalculateDefectRate()
        {
            if (TotalInspections > 0)
            {
                int totalDefects = ComponentDefectCount + SolderDefectCount + DiscardCount;
                DefectRate = (double)totalDefects / TotalInspections * 100.0;
            }
            else
            {
                DefectRate = 0.0;
            }
        }
    }

    /// 일별 통계 모델
    public class DailyStatistics
    {
        public DateTime StatDate { get; set; }
        public int TotalInspections { get; set; }
        public int NormalCount { get; set; }
        public int ComponentDefectCount { get; set; }
        public int SolderDefectCount { get; set; }
        public int DiscardCount { get; set; }
        public double DefectRate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// 시간별 통계 모델
    public class HourlyStatistics
    {
        public DateTime StatDatetime { get; set; }
        public int TotalInspections { get; set; }
        public int NormalCount { get; set; }
        public int ComponentDefectCount { get; set; }
        public int SolderDefectCount { get; set; }
        public int DiscardCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
