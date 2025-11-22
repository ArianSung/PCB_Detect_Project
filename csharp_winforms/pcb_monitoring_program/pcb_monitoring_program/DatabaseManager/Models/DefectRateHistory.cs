using System;

namespace pcb_monitoring_program.DatabaseManager.Models
{
    /// 불량률 추이 이력 모델 (defect_rate_history 테이블)
    /// 시간대별 불량률 추적 (불량률 모니터링 그래프, 이탈 포인트 분석용)
    public class DefectRateHistory
    {
        /// 고유 ID
        public int Id { get; set; }

        /// 불량률 (%)
        public decimal DefectRate { get; set; }

        /// 해당 시점의 누적 총 검사 수
        public int TotalInspections { get; set; }

        /// 해당 시점의 누적 불량 수
        public int DefectCount { get; set; }

        /// 기록 시각
        public DateTime RecordedAt { get; set; }
    }
}
