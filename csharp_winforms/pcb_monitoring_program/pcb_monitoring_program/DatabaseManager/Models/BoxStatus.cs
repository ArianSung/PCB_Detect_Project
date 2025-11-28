using System;

namespace pcb_monitoring_program.DatabaseManager.Models
{
    /// <summary>
    /// 로봇팔 박스 상태 관리 모델 (v3.1 스키마)
    /// 3개 박스 (정상/누락/위치오류) × 5개 슬롯 = 15개 슬롯
    /// DISCARD는 슬롯 관리 안 함 (고정 위치에 떨어뜨리기)
    /// </summary>
    public class BoxStatus
    {
        public int Id { get; set; }
        public string BoxId { get; set; }       // "NORMAL", "MISSING", "POSITION_ERROR"
        public string Category { get; set; }    // "normal", "missing", "position_error" (v3.1 변경)
        public int CurrentSlot { get; set; }  // 현재 사용 중인 슬롯 번호 (0-4)
        public int MaxSlots { get; set; }  // 최대 슬롯 개수 (5개, 수평 배치)
        public bool IsFull { get; set; }  // 박스 가득참 여부
        public int TotalPcbCount { get; set; }  // 박스에 저장된 총 PCB 개수
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdated { get; set; }

        public BoxStatus()
        {
            CurrentSlot = 0;
            MaxSlots = 5;
            IsFull = false;
            TotalPcbCount = 0;
            CreatedAt = DateTime.Now;
            LastUpdated = DateTime.Now;
        }

        /// 슬롯 사용률 계산 (%)
        public double GetUtilizationRate()
        {
            if (MaxSlots > 0)
            {
                return (double)(CurrentSlot + 1) / MaxSlots * 100.0;
            }
            return 0.0;
        }

        /// 슬롯 사용 현황 문자열 (예: "3/5")
        public string GetSlotStatus()
        {
            return $"{CurrentSlot + 1}/{MaxSlots}";
        }
    }
}
