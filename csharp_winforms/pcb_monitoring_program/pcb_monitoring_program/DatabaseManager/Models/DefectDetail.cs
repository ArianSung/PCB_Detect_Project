using System;

namespace pcb_monitoring_program.DatabaseManager.Models
{
    /// <summary>
    /// 상세 불량 유형별 검출 정보 모델 (v3.1 스키마)
    /// YOLO가 검출한 상세 클래스별 통계 정보 저장 (트리거로 자동 삽입)
    /// </summary>
    public class DefectDetail
    {
        /// 고유 ID
        public long Id { get; set; }

        /// 검사 ID (외래 키)
        public long InspectionId { get; set; }

        /// YOLO 검출 클래스명 (예: solder_bridge, capacitor_missing, poor_solder 등)
        public string ClassName { get; set; }

        /// 검출된 객체 개수
        public int Count { get; set; }

        /// 평균 신뢰도 (0.0000 ~ 1.0000)
        public decimal? AvgConfidence { get; set; }

        /// 생성 시간
        public DateTime CreatedAt { get; set; }
    }
}
