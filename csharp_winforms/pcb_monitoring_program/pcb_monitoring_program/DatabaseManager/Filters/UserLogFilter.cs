using System;
using pcb_monitoring_program.DatabaseManager.Models;

namespace pcb_monitoring_program.DatabaseManager.Filters
{
    /// <summary>
    /// 사용자 활동 로그 필터 클래스
    /// </summary>
    public class UserLogFilter
    {
        public int? UserId { get; set; }
        public UserActionType? ActionType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public UserRole? UserRole { get; set; }

        public UserLogFilter()
        {
            // 기본값: 최근 7일
            StartDate = DateTime.Today.AddDays(-7);
            EndDate = DateTime.Now;
        }

        /// <summary>
        /// 필터가 비어있는지 확인
        /// </summary>
        public bool IsEmpty()
        {
            return !UserId.HasValue &&
                   !ActionType.HasValue &&
                   !StartDate.HasValue &&
                   !EndDate.HasValue &&
                   !UserRole.HasValue;
        }
    }
}
