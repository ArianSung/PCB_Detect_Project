using System;

namespace pcb_monitoring_program.DatabaseManager.Models
{
    /// <summary>
    /// 제품 정보 모델 (v3.1 스키마)
    /// 3개 제품 타입: FT, RS, BC
    /// </summary>
    public class Product
    {
        public string ProductCode { get; set; }        // 제품 코드 (예: FT, RS, BC) - Primary Key
        public string ProductName { get; set; }         // 제품명
        public string Description { get; set; }         // 제품 설명
        public string SerialPrefix { get; set; }        // 시리얼 넘버 접두사 (예: MBFT, MBRS, MBBC)
        public int ComponentCount { get; set; }         // 기준 부품 개수
        public string QrUrlTemplate { get; set; }       // QR 코드 URL 템플릿
        public DateTime CreatedAt { get; set; }         // 생성일
        public DateTime UpdatedAt { get; set; }         // 수정일

        public Product()
        {
            ComponentCount = 0;
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }
    }
}
