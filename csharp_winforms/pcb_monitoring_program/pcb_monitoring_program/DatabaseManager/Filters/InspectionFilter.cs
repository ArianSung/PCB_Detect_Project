using System;

namespace pcb_monitoring_program.DatabaseManager.Filters
{
    /// <summary>
    /// 검사 이력 필터 클래스
    /// </summary>
    public class InspectionFilter
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string DefectType { get; set; }  // null = 전체, "정상", "부품불량", "납땜불량", "폐기"
        public string CameraId { get; set; }     // null = 전체, "left", "right"
        public int? UserId { get; set; }         // null = 전체

        public InspectionFilter()
        {
            // 기본값: 오늘 날짜
            StartDate = DateTime.Today;
            EndDate = DateTime.Now;
        }

        /// <summary>
        /// 필터가 비어있는지 확인
        /// </summary>
        public bool IsEmpty()
        {
            return !StartDate.HasValue &&
                   !EndDate.HasValue &&
                   string.IsNullOrEmpty(DefectType) &&
                   string.IsNullOrEmpty(CameraId) &&
                   !UserId.HasValue;
        }
    }
}
