using System;

namespace pcb_monitoring_program.DatabaseManager.Models
{
    /// <summary>
    /// 불량률 추이 이력 모델 (defect_rate_history 테이블)
    /// 시간대별 불량률 추적 (불량률 모니터링 그래프, 이탈 포인트 분석용)
    /// </summary>
    public class DefectRateHistory
    {
        /// <summary>
        /// 고유 ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 불량률 (%)
        /// </summary>
        public decimal DefectRate { get; set; }

        /// <summary>
        /// 해당 시점의 누적 총 검사 수
        /// </summary>
        public int TotalInspections { get; set; }

        /// <summary>
        /// 해당 시점의 누적 불량 수
        /// </summary>
        public int DefectCount { get; set; }

        /// <summary>
        /// 기록 시각
        /// </summary>
        public DateTime RecordedAt { get; set; }
    }
}
