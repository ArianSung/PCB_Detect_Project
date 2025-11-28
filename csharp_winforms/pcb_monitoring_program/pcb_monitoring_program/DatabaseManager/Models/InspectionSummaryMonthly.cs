using System;

namespace pcb_monitoring_program.DatabaseManager.Models
{
    /// <summary>
    /// 월별 검사 집계 모델 (v3.1 스키마)
    /// 트리거로 자동 업데이트됨
    /// </summary>
    public class InspectionSummaryMonthly
    {
        public long Id { get; set; }                    // 고유 ID
        public int Year { get; set; }                   // 연도
        public int Month { get; set; }                  // 월 (1-12)
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

        public InspectionSummaryMonthly()
        {
            TotalInspections = 0;
            NormalCount = 0;
            MissingCount = 0;
            PositionErrorCount = 0;
            DiscardCount = 0;
            Year = DateTime.Now.Year;
            Month = DateTime.Now.Month;
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

        /// <summary>
        /// 해당 월의 첫 날짜 반환
        /// </summary>
        public DateTime GetMonthStartDate()
        {
            return new DateTime(Year, Month, 1);
        }

        /// <summary>
        /// 해당 월의 마지막 날짜 반환
        /// </summary>
        public DateTime GetMonthEndDate()
        {
            return new DateTime(Year, Month, DateTime.DaysInMonth(Year, Month));
        }
    }
}
