using System;

namespace DataCollector.Models
{
    /// <summary>
    /// 일봉 데이터를 담는 클래스
    /// </summary>
    public class DailyPrice
    {
        /// <summary>날짜</summary>
        public DateTime Date { get; set; }

        /// <summary>시가</summary>
        public decimal Open { get; set; }

        /// <summary>고가</summary>
        public decimal High { get; set; }

        /// <summary>저가</summary>
        public decimal Low { get; set; }

        /// <summary>종가</summary>
        public decimal Close { get; set; }

        /// <summary>거래량</summary>
        public long Volume { get; set; }
    }
}