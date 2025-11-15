using System;
using System.Collections.Generic;

namespace pcb_monitoring_program.DatabaseManager.Models
{
    /// 검사 결과 이력 모델
    public class Inspection
    {
        public int Id { get; set; }
        public string CameraId { get; set; }  // "left" or "right"
        public string DefectType { get; set; } // "정상", "부품불량", "납땜불량", "폐기"
        public double Confidence { get; set; }
        public DateTime InspectionTime { get; set; }
        public string ImagePath { get; set; }
        public string Boxes { get; set; }  // JSON 문자열
        public int? GpioPin { get; set; }
        public int? GpioDurationMs { get; set; }
        public int? UserId { get; set; }
        public string Notes { get; set; }

        public Inspection() { }

        public Inspection(int id, string cameraId, string defectType, double confidence,
                         DateTime inspectionTime, string imagePath = null)
        {
            Id = id;
            CameraId = cameraId;
            DefectType = defectType;
            Confidence = confidence;
            InspectionTime = inspectionTime;
            ImagePath = imagePath;
        }
    }
}
