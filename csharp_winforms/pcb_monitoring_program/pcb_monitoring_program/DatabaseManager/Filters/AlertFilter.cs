using System;
using pcb_monitoring_program.DatabaseManager.Models;

namespace pcb_monitoring_program.DatabaseManager.Filters
{
    /// <summary>
    /// 알람 필터 클래스
    /// </summary>
    public class AlertFilter
    {
        public bool? IsResolved { get; set; }
        public AlertSeverity? Severity { get; set; }
        public AlertType? AlertType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public AlertFilter()
        {
            // 기본값: 미해결 알람만
            IsResolved = false;
        }

        /// <summary>
        /// 필터가 비어있는지 확인
        /// </summary>
        public bool IsEmpty()
        {
            return !IsResolved.HasValue &&
                   !Severity.HasValue &&
                   !AlertType.HasValue &&
                   !StartDate.HasValue &&
                   !EndDate.HasValue;
        }
    }
}
