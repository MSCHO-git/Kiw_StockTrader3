using System;

namespace StockTrader3.Models
{
    /// <summary>
    /// 분봉 데이터 모델 (1분, 3분, 5분, 10분봉)
    /// </summary>
    public class MinutePrice
    {
        /// <summary>
        /// 고유 ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 종목 코드 (예: "005930")
        /// </summary>
        public string StockCode { get; set; }

        /// <summary>
        /// 날짜 및 시간 (예: 2025-05-24 14:30:00)
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// 분봉 간격 (1, 3, 5, 10분)
        /// </summary>
        public int MinuteInterval { get; set; }

        /// <summary>
        /// 시가
        /// </summary>
        public decimal Open { get; set; }

        /// <summary>
        /// 고가
        /// </summary>
        public decimal High { get; set; }

        /// <summary>
        /// 저가
        /// </summary>
        public decimal Low { get; set; }

        /// <summary>
        /// 종가 (현재가)
        /// </summary>
        public decimal Close { get; set; }

        /// <summary>
        /// 거래량
        /// </summary>
        public long Volume { get; set; }

        /// <summary>
        /// 전일 대비 변동액
        /// </summary>
        public decimal ChangeAmount { get; set; }

        /// <summary>
        /// 전일 대비 변동률 (%)
        /// </summary>
        public decimal ChangeRate { get; set; }

        /// <summary>
        /// 거래대금 (원)
        /// </summary>
        public long TradingValue { get; set; }

        /// <summary>
        /// 데이터 수집 시간
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 기본 생성자
        /// </summary>
        public MinutePrice()
        {
            CreatedAt = DateTime.Now;
        }

        /// <summary>
        /// 주요 데이터로 생성자
        /// </summary>
        public MinutePrice(string stockCode, DateTime dateTime, int minuteInterval,
                          decimal open, decimal high, decimal low, decimal close, long volume)
        {
            StockCode = stockCode;
            DateTime = dateTime;
            MinuteInterval = minuteInterval;
            Open = open;
            High = high;
            Low = low;
            Close = close;
            Volume = volume;
            CreatedAt = DateTime.Now;
        }

        /// <summary>
        /// 문자열 표현
        /// </summary>
        public override string ToString()
        {
            return $"{StockCode} {DateTime:yyyy-MM-dd HH:mm} ({MinuteInterval}분) " +
                   $"O:{Open} H:{High} L:{Low} C:{Close} V:{Volume:N0}";
        }

        /// <summary>
        /// 봉의 색깔 판단 (상승/하락/보합)
        /// </summary>
        public string CandleType
        {
            get
            {
                if (Close > Open) return "상승"; // 양봉
                else if (Close < Open) return "하락"; // 음봉
                else return "보합"; // 도지
            }
        }

        /// <summary>
        /// 봉의 몸통 크기 (절대값)
        /// </summary>
        public decimal BodySize
        {
            get { return Math.Abs(Close - Open); }
        }

        /// <summary>
        /// 상단 꼬리 길이
        /// </summary>
        public decimal UpperShadow
        {
            get { return High - Math.Max(Open, Close); }
        }

        /// <summary>
        /// 하단 꼬리 길이
        /// </summary>
        public decimal LowerShadow
        {
            get { return Math.Min(Open, Close) - Low; }
        }

        /// <summary>
        /// 전체 봉 길이 (고가 - 저가)
        /// </summary>
        public decimal TotalRange
        {
            get { return High - Low; }
        }

        /// <summary>
        /// 중간가 (고가 + 저가) / 2
        /// </summary>
        public decimal MiddlePrice
        {
            get { return (High + Low) / 2; }
        }

        /// <summary>
        /// 평균가 (시가 + 고가 + 저가 + 종가) / 4
        /// </summary>
        public decimal AveragePrice
        {
            get { return (Open + High + Low + Close) / 4; }
        }

        /// <summary>
        /// 해당 분봉이 유효한 데이터인지 확인
        /// </summary>
        public bool IsValid()
        {
            // 기본 유효성 검사
            if (string.IsNullOrEmpty(StockCode)) return false;
            if (DateTime == DateTime.MinValue) return false;
            if (MinuteInterval <= 0) return false;
            if (Open <= 0 || High <= 0 || Low <= 0 || Close <= 0) return false;
            if (Volume < 0) return false;

            // 가격 논리 검사
            if (High < Math.Max(Open, Close)) return false; // 고가가 시가나 종가보다 낮을 수 없음
            if (Low > Math.Min(Open, Close)) return false;  // 저가가 시가나 종가보다 높을 수 없음

            return true;
        }

        /// <summary>
        /// 분봉 간격별 시간 정렬을 위한 키 생성
        /// </summary>
        public string GetTimeKey()
        {
            return $"{StockCode}_{MinuteInterval}_{DateTime:yyyyMMddHHmm}";
        }
    }
}