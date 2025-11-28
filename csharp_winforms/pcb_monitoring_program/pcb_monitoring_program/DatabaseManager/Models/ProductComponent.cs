using System;

namespace pcb_monitoring_program.DatabaseManager.Models
{
    /// <summary>
    /// 제품별 부품 배치 기준 모델 (v3.1 스키마)
    /// 제품별 정상 부품 배치 위치 정보 (기준 데이터)
    /// </summary>
    public class ProductComponent
    {
        public int Id { get; set; }                     // 고유 ID
        public string ProductCode { get; set; }         // 제품 코드 (FK)
        public string ComponentClass { get; set; }      // 부품 클래스명 (예: capacitor, resistor)

        // 부품 중심 좌표
        public float CenterX { get; set; }              // 부품 중심 X 좌표 (픽셀)
        public float CenterY { get; set; }              // 부품 중심 Y 좌표 (픽셀)

        // 바운딩 박스
        public float BboxX1 { get; set; }               // 바운딩 박스 좌상단 X
        public float BboxY1 { get; set; }               // 바운딩 박스 좌상단 Y
        public float BboxX2 { get; set; }               // 바운딩 박스 우하단 X
        public float BboxY2 { get; set; }               // 바운딩 박스 우하단 Y

        public float TolerancePx { get; set; }          // 위치 허용 오차 (픽셀, 기본 20px)
        public DateTime CreatedAt { get; set; }         // 생성일

        public ProductComponent()
        {
            TolerancePx = 20.0f;  // 기본 허용 오차 20픽셀
            CreatedAt = DateTime.Now;
        }

        /// <summary>
        /// 바운딩 박스 너비 계산
        /// </summary>
        public float GetWidth()
        {
            return BboxX2 - BboxX1;
        }

        /// <summary>
        /// 바운딩 박스 높이 계산
        /// </summary>
        public float GetHeight()
        {
            return BboxY2 - BboxY1;
        }

        /// <summary>
        /// 바운딩 박스 면적 계산
        /// </summary>
        public float GetArea()
        {
            return GetWidth() * GetHeight();
        }

        /// <summary>
        /// 특정 좌표가 허용 오차 범위 내인지 확인
        /// </summary>
        public bool IsWithinTolerance(float x, float y)
        {
            float distance = (float)Math.Sqrt(
                Math.Pow(x - CenterX, 2) + Math.Pow(y - CenterY, 2)
            );
            return distance <= TolerancePx;
        }
    }
}
