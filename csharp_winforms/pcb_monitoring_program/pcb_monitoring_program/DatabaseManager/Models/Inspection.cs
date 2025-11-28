using System;

namespace pcb_monitoring_program.DatabaseManager.Models
{
    /// <summary>
    /// PCB 검사 이력 모델 (v3.1 스키마)
    /// Product Verification Architecture - 제품별 부품 위치 검증 시스템
    /// </summary>
    public class Inspection
    {
        // 기본 정보
        public long Id { get; set; }

        // 제품 식별 정보 (뒷면 - QR/Serial)
        public string SerialNumber { get; set; }  // 시리얼 넘버 (MBXX12345678)
        public string ProductCode { get; set; }    // 제품 코드 (FT, RS, BC)
        public string QrData { get; set; }         // QR 코드 데이터 (URL 또는 JSON)
        public bool QrDetected { get; set; }       // QR 코드 검출 성공 여부
        public bool SerialDetected { get; set; }   // 시리얼 넘버 검출 성공 여부

        // 검사 결과 (앞면 - 부품 검증)
        public string Decision { get; set; }              // 최종 판정: normal/missing/position_error/discard
        public int MissingCount { get; set; }             // 누락 부품 개수
        public int PositionErrorCount { get; set; }       // 위치 오류 부품 개수
        public int ExtraCount { get; set; }               // 추가 부품 개수 (기준에 없음)
        public int CorrectCount { get; set; }             // 정상 부품 개수

        // 상세 결과 (JSON 문자열)
        public string MissingComponents { get; set; }     // 누락 부품 상세 JSON
        public string PositionErrors { get; set; }        // 위치 오류 상세 JSON
        public string ExtraComponents { get; set; }       // 추가 부품 상세 JSON

        // YOLO 검출 결과
        public string YoloDetections { get; set; }        // YOLO 전체 검출 결과 JSON
        public int DetectionCount { get; set; }           // 총 검출 부품 개수
        public float? AvgConfidence { get; set; }         // 평균 신뢰도

        // 처리 성능
        public float? InferenceTimeMs { get; set; }       // AI 추론 시간 (밀리초)
        public float? VerificationTimeMs { get; set; }    // 검증 처리 시간 (밀리초)
        public float? TotalTimeMs { get; set; }           // 총 처리 시간 (밀리초)

        // 이미지 정보
        public string LeftImagePath { get; set; }         // 좌측 카메라 이미지 경로
        public string RightImagePath { get; set; }        // 우측 카메라 이미지 경로
        public int? ImageWidth { get; set; }              // 이미지 너비
        public int? ImageHeight { get; set; }             // 이미지 높이

        // 시스템 정보
        public string CameraId { get; set; }              // 카메라 ID (left/right)
        public string ClientIp { get; set; }              // 클라이언트 IP (라즈베리파이)
        public string ServerVersion { get; set; }         // 서버 버전

        // 사용자 정보 (v3.1 추가)
        public int? UserId { get; set; }                  // 검사 확인 사용자 ID (FK)
        public string Notes { get; set; }                 // 추가 메모

        // 시간 정보
        public DateTime InspectionTime { get; set; }      // 검사 시간
        public DateTime CreatedAt { get; set; }           // 레코드 생성 시간

        public Inspection()
        {
            // 기본값 설정
            QrDetected = false;
            SerialDetected = false;
            MissingCount = 0;
            PositionErrorCount = 0;
            ExtraCount = 0;
            CorrectCount = 0;
            DetectionCount = 0;
            InspectionTime = DateTime.Now;
            CreatedAt = DateTime.Now;
        }

        /// <summary>
        /// 불량 여부 확인
        /// </summary>
        public bool IsDefective()
        {
            return Decision != "normal";
        }

        /// <summary>
        /// 폐기 대상 여부 확인
        /// </summary>
        public bool IsDiscard()
        {
            return Decision == "discard";
        }

        /// <summary>
        /// 총 불량 개수 (누락 + 위치 오류)
        /// </summary>
        public int GetTotalDefectCount()
        {
            return MissingCount + PositionErrorCount;
        }
    }
}
