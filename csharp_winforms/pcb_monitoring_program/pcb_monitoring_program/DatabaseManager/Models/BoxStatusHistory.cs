using System;

namespace pcb_monitoring_program.DatabaseManager.Models
{
    /// <summary>
    /// 박스 상태 이력 모델 (box_status_history 테이블)
    /// 시간대별 박스 상태 추적 (모니터링 그래프, 이탈 포인트 분석용)
    /// </summary>
    public class BoxStatusHistory
    {
        /// <summary>
        /// 고유 ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 박스 ID (NORMAL, COMPONENT_DEFECT, SOLDER_DEFECT)
        /// </summary>
        public string BoxId { get; set; }

        /// <summary>
        /// 당시 사용 중인 슬롯 번호 (0-4)
        /// </summary>
        public int CurrentSlot { get; set; }

        /// <summary>
        /// 당시 박스에 저장된 총 PCB 개수
        /// </summary>
        public int TotalPcbCount { get; set; }

        /// <summary>
        /// 당시 박스 가득참 여부
        /// </summary>
        public bool IsFull { get; set; }

        /// <summary>
        /// 기록 시각
        /// </summary>
        public DateTime RecordedAt { get; set; }
    }
}
