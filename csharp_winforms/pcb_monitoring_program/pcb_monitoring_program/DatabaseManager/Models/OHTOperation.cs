using System;

namespace pcb_monitoring_program.DatabaseManager.Models
{
    /// <summary>
    /// OHT (Overhead Hoist Transport) 운영 이력 모델
    /// </summary>
    public class OHTOperation
    {
        public int Id { get; set; }
        public string OperationId { get; set; }  // UUID
        public OHTCategory Category { get; set; }  // NORMAL, COMPONENT_DEFECT, SOLDER_DEFECT
        public int? UserId { get; set; }  // NULL이면 시스템 자동
        public string UserRole { get; set; }  // "Admin", "Operator", "System"
        public bool IsAuto { get; set; }  // 자동 호출 여부
        public string TriggerReason { get; set; }  // "box_full" 등
        public OHTStatus Status { get; set; }  // pending, processing, completed, failed
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int PcbCount { get; set; }  // 수거한 PCB 개수
        public bool? Success { get; set; }
        public string ErrorMessage { get; set; }
        public double? ExecutionTimeSeconds { get; set; }

        public OHTOperation()
        {
            OperationId = Guid.NewGuid().ToString();
            CreatedAt = DateTime.Now;
            Status = OHTStatus.Pending;
            PcbCount = 0;
        }
    }

    /// <summary>
    /// OHT 카테고리 (수거할 PCB 종류)
    /// </summary>
    public enum OHTCategory
    {
        NORMAL,             // 정상 PCB
        COMPONENT_DEFECT,   // 부품 불량 PCB
        SOLDER_DEFECT       // 납땜 불량 PCB
    }

    /// <summary>
    /// OHT 운영 상태
    /// </summary>
    public enum OHTStatus
    {
        Pending,      // 대기 중
        Processing,   // 진행 중
        Completed,    // 완료
        Failed        // 실패
    }
}
