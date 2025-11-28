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

    /// 시간별 통계 모델 (v3.0 스키마: inspection_summary_hourly)
    public class HourlyStatistics
    {
        /// 고유 ID
        public long Id { get; set; }

        /// 시간 (YYYY-MM-DD HH:00:00)
        public DateTime StatDatetime { get; set; }

        /// 제품 코드 (FT, RS, BC)
        public string ProductCode { get; set; }

        /// 총 검사 수
        public int TotalInspections { get; set; }

        /// 정상 수
        public int NormalCount { get; set; }

        /// 부품 누락 수 (missing_count)
        public int ComponentDefectCount { get; set; }

        /// 위치 오류 수 (position_error_count)
        public int SolderDefectCount { get; set; }

        /// 폐기 수
        public int DiscardCount { get; set; }

        /// 평균 추론 시간 (ms)
        public float? AvgInferenceTimeMs { get; set; }

        /// 평균 총 처리 시간 (ms)
        public float? AvgTotalTimeMs { get; set; }

        /// 평균 검출 부품 수
        public float? AvgDetectionCount { get; set; }

        /// 평균 신뢰도
        public float? AvgConfidence { get; set; }

        /// 불량률 (%) - Computed Column
        public float DefectRate { get; set; }

        /// 생성 시간
        public DateTime CreatedAt { get; set; }

        /// 마지막 업데이트 시간
        public DateTime UpdatedAt { get; set; }
    }
}
