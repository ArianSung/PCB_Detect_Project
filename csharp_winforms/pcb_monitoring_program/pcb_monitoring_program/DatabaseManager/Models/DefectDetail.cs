using System;

namespace pcb_monitoring_program.DatabaseManager.Models
{
    /// <summary>
    /// 상세 불량 유형별 검출 정보 모델 (defect_details 테이블)
    /// YOLO가 검출한 상세 클래스별 통계 정보 저장
    /// </summary>
    public class DefectDetail
    {
        /// <summary>
        /// 고유 ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 검사 ID (외래 키)
        /// </summary>
        public int InspectionId { get; set; }

        /// <summary>
        /// YOLO 검출 클래스명 (예: solder_bridge, capacitor_missing, poor_solder 등)
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// 검출된 객체 개수
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 평균 신뢰도 (0.0000 ~ 1.0000)
        /// </summary>
        public decimal? AvgConfidence { get; set; }

        /// <summary>
        /// 생성 시간
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
