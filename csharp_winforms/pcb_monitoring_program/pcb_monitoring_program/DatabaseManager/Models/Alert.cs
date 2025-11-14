using System;

namespace pcb_monitoring_program.DatabaseManager.Models
{
    /// <summary>
    /// 알람/알림 모델
    /// </summary>
    public class Alert
    {
        public int Id { get; set; }
        public AlertType AlertType { get; set; }
        public AlertSeverity Severity { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }  // JSON 문자열
        public bool IsResolved { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public int? ResolvedBy { get; set; }
        public DateTime CreatedAt { get; set; }

        public Alert()
        {
            IsResolved = false;
            CreatedAt = DateTime.Now;
            Severity = AlertSeverity.Medium;
        }
    }

    /// <summary>
    /// 알람 유형
    /// </summary>
    public enum AlertType
    {
        DefectRateHigh,     // 불량률 높음
        SystemError,        // 시스템 오류
        CameraOffline,      // 카메라 오프라인
        ServerOffline,      // 서버 오프라인
        BoxFull             // 박스 가득참
    }

    /// <summary>
    /// 알람 심각도
    /// </summary>
    public enum AlertSeverity
    {
        Low,        // 낮음
        Medium,     // 중간
        High,       // 높음
        Critical    // 심각
    }
}
