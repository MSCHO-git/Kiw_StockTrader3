using System;

namespace StockTrader3.Models
{
    /// <summary>
    /// 매매 계획을 담는 클래스
    /// </summary>
    public class TradePlan
    {
        /// <summary>계획 ID</summary>
        public int Id { get; set; }

        /// <summary>종목 코드</summary>
        public string StockCode { get; set; }

        /// <summary>종목명</summary>
        public string StockName { get; set; }

        /// <summary>분석 날짜</summary>
        public DateTime AnalysisDate { get; set; }

        /// <summary>현재가</summary>
        public decimal CurrentPrice { get; set; }

        /// <summary>매수 목표가</summary>
        public decimal BuyTargetPrice { get; set; }

        /// <summary>매도 목표가</summary>
        public decimal SellTargetPrice { get; set; }

        /// <summary>손절가</summary>
        public decimal StopLossPrice { get; set; }

        /// <summary>기술적 분석 점수</summary>
        public double TechnicalScore { get; set; }

        /// <summary>뉴스 분석 점수</summary>
        public double NewsScore { get; set; }

        /// <summary>종합 점수</summary>
        public double TotalScore { get; set; }

        /// <summary>등급 (S, A, B, 관심)</summary>
        public string Grade { get; set; }

        /// <summary>상승 요인 (호재 내용)</summary>
        public string PositiveFactor { get; set; }

        /// <summary>주의사항</summary>
        public string WarningNote { get; set; }

        /// <summary>예상 수익률 (%)</summary>
        public double ExpectedReturn { get; set; }

        /// <summary>매매 우선순위 (1~50)</summary>
        public int Priority { get; set; }
    }
}