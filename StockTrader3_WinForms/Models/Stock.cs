using System;
using System.Collections.Generic;

namespace StockTrader3.Models
{
    /// <summary>
    /// 주식 데이터를 담는 기본 모델 클래스
    /// </summary>
    public class Stock
    {
        /// <summary>종목 코드 (예: 005930)</summary>
        public string Code { get; set; }

        /// <summary>종목명 (예: 삼성전자)</summary>
        public string Name { get; set; }

        /// <summary>현재가 (MainForm 호환용)</summary>
        public int ClosePrice { get; set; }

        /// <summary>전일대비</summary>
        public int ChangeAmount { get; set; }

        /// <summary>등락률 (%)</summary>
        public double ChangeRate { get; set; }        // decimal → double로 변경

        /// <summary>거래량</summary>
        public long Volume { get; set; }

        /// <summary>시가</summary>
        public int OpenPrice { get; set; }

        /// <summary>고가</summary>
        public int HighPrice { get; set; }

        /// <summary>저가</summary>
        public int LowPrice { get; set; }

        /// <summary>현재가 (기존 호환성 유지)</summary>
        public decimal CurrentPrice
        {
            get { return ClosePrice; }
            set { ClosePrice = (int)value; }
        }

        /// <summary>전일 종가</summary>
        public decimal PreviousClose { get; set; }

        /// <summary>거래대금</summary>
        public long TradingValue { get; set; }

        /// <summary>시가총액</summary>
        public long MarketCap { get; set; }

        /// <summary>업종</summary>
        public string Sector { get; set; }

        /// <summary>시장 구분 (KOSPI/KOSDAQ)</summary>
        public string Market { get; set; }

        /// <summary>일봉 데이터 리스트</summary>
        public List<DailyPrice> DailyPrices { get; set; }

        /// <summary>기술적 분석 점수</summary>
        public double TechnicalScore { get; set; }

        /// <summary>뉴스 분석 점수</summary>
        public double NewsScore { get; set; }

        /// <summary>종합 점수</summary>
        public double TotalScore { get; set; }

        /// <summary>등급 (S, A, B, C)</summary>
        public string Grade { get; set; }

        // ===== MainWindow.xaml.cs에서 사용하는 추가 속성들 =====

        /// <summary>순위 (UI 표시용)</summary>
        public int Rank { get; set; }

        /// <summary>종목명 (UI 바인딩용 - Name과 동일하지만 명확한 구분)</summary>
        public string StockName
        {
            get { return Name; }
            set { Name = value; }
        }

        /// <summary>종목코드 (UI 바인딩용 - Code와 동일하지만 명확한 구분)</summary>
        public string StockCode
        {
            get { return Code; }
            set { Code = value; }
        }

        /// <summary>매수 목표가</summary>
        public decimal BuyTargetPrice { get; set; }

        /// <summary>매도 목표가</summary>
        public decimal SellTargetPrice { get; set; }

        /// <summary>손절가</summary>
        public decimal StopLossPrice { get; set; }

        /// <summary>예상 수익률 (%)</summary>
        public decimal ExpectedReturn { get; set; }

        /// <summary>상승 요인</summary>
        public string PositiveFactor { get; set; }

        /// <summary>기술적 선정 이유</summary>
        public string TechnicalReason { get; set; }

        /// <summary>뉴스 선정 이유</summary>
        public string NewsReason { get; set; }

        /// <summary>신뢰도 (0~100%)</summary>
        public decimal ConfidenceLevel { get; set; }

        /// <summary>매매 시나리오</summary>
        public string TradingScenario { get; set; }

        /// <summary>예상 매수 시간</summary>
        public DateTime? ExpectedBuyTime { get; set; }

        /// <summary>상태 (대기/분석중/완료/오류)</summary>
        public string Status { get; set; } = "대기";

        /// <summary>마지막 업데이트 시간</summary>
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        /// <summary>생성자</summary>
        public Stock()
        {
            DailyPrices = new List<DailyPrice>();
            Grade = "분석중";
            PositiveFactor = "";
            TechnicalReason = "";
            NewsReason = "";
            TradingScenario = "";
        }

        /// <summary>
        /// 종합 등급 자동 계산
        /// </summary>
        public void CalculateGrade()
        {
            if (TotalScore >= 120)
                Grade = "S";
            else if (TotalScore >= 110)
                Grade = "A";
            else if (TotalScore >= 100)
                Grade = "B";
            else if (TotalScore >= 80)
                Grade = "관심";
            else
                Grade = "배제";
        }

        /// <summary>
        /// ATR 기반 기본 매매 계획 계산
        /// </summary>
        public void CalculateBasicTradingPlan()
        {
            if (ClosePrice > 0)  // CurrentPrice → ClosePrice로 변경
            {
                // 기본적인 매매 계획 (실제 분석 엔진에서 더 정교하게 계산)
                BuyTargetPrice = ClosePrice * 0.98m;  // 현재가 2% 아래
                SellTargetPrice = ClosePrice * 1.05m; // 현재가 5% 위
                StopLossPrice = ClosePrice * 0.95m;   // 현재가 5% 아래

                // 예상 수익률 계산
                if (BuyTargetPrice > 0)
                {
                    ExpectedReturn = ((SellTargetPrice - BuyTargetPrice) / BuyTargetPrice) * 100;
                }
            }
        }

        /// <summary>
        /// 객체 정보 문자열 반환
        /// </summary>
        public override string ToString()
        {
            return $"{Name}({Code}) - {Grade}급 {TotalScore:F1}점";
        }
    }
}