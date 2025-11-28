using System;

namespace pcb_monitoring_program.DatabaseManager.Models
{
    /// <summary>
    /// 시간별 검사 집계 모델 (v3.1 스키마)
    /// 트리거로 자동 업데이트됨
    /// </summary>
    public class InspectionSummaryHourly
    {
        public long Id { get; set; }                    // 고유 ID
        public DateTime HourTimestamp { get; set; }     // 시간 (분/초 제거)
        public string ProductCode { get; set; }         // 제품 코드

        // 집계 데이터
        public int TotalInspections { get; set; }       // 총 검사 수
        public int NormalCount { get; set; }            // 정상 개수
        public int MissingCount { get; set; }           // 누락 개수
        public int PositionErrorCount { get; set; }     // 위치 오류 개수
        public int DiscardCount { get; set; }           // 폐기 개수

        // 평균값
        public float? AvgInferenceTimeMs { get; set; }  // 평균 추론 시간
        public float? AvgTotalTimeMs { get; set; }      // 평균 총 처리 시간
        public float? AvgDetectionCount { get; set; }   // 평균 검출 개수
        public float? AvgConfidence { get; set; }       // 평균 신뢰도

        public DateTime CreatedAt { get; set; }         // 생성일
        public DateTime UpdatedAt { get; set; }         // 수정일

        public InspectionSummaryHourly()
        {
            TotalInspections = 0;
            NormalCount = 0;
            MissingCount = 0;
            PositionErrorCount = 0;
            DiscardCount = 0;
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }

        /// <summary>
        /// 불량률 계산 (%)
        /// </summary>
        public double GetDefectRate()
        {
            if (TotalInspections == 0) return 0.0;
            int defects = MissingCount + PositionErrorCount + DiscardCount;
            return (double)defects / TotalInspections * 100.0;
        }

        /// <summary>
        /// 정상률 계산 (%)
        /// </summary>
        public double GetNormalRate()
        {
            if (TotalInspections == 0) return 0.0;
            return (double)NormalCount / TotalInspections * 100.0;
        }
    }
}
