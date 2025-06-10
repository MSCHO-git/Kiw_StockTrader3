using System;
using System.Collections.Generic;
using System.Linq;
using StockTrader3.Models;

namespace StockTrader3.Indicators
{
    /// <summary>
    /// RSI (Relative Strength Index) 계산 클래스
    /// </summary>
    public static class RSI
    {
        /// <summary>
        /// RSI 계산
        /// </summary>
        /// <param name="dailyPrices">일봉 데이터</param>
        /// <param name="period">RSI 기간 (보통 14일)</param>
        /// <returns>현재 RSI 값</returns>
        public static double? Calculate(List<DailyPrice> dailyPrices, int period = 14)
        {
            if (dailyPrices == null || dailyPrices.Count < period + 1)
                return null;

            // 날짜순 정렬
            var sortedPrices = dailyPrices.OrderBy(x => x.Date).ToList();

            // 가격 변화량 계산
            var priceChanges = new List<decimal>();
            for (int i = 1; i < sortedPrices.Count; i++)
            {
                priceChanges.Add(sortedPrices[i].Close - sortedPrices[i - 1].Close);
            }

            if (priceChanges.Count < period)
                return null;

            // 최근 period 개의 변화량만 사용
            var recentChanges = priceChanges.Skip(priceChanges.Count - period).ToList();

            // 상승/하락 분리
            var gains = recentChanges.Where(x => x > 0).Sum();
            var losses = Math.Abs(recentChanges.Where(x => x < 0).Sum());

            if (losses == 0)
                return 100; // 모든 기간 상승 시

            // RSI 계산
            var rs = gains / losses;
            var rsi = 100 - (100 / (1 + rs));

            return (double)rsi;
        }

        /// <summary>
        /// RSI 상태 판단
        /// </summary>
        /// <param name="rsi">RSI 값</param>
        /// <returns>과매수/과매도 상태</returns>
        public static string GetRSIStatus(double rsi)
        {
            if (rsi >= 70)
                return "과매수"; // Overbought
            else if (rsi <= 30)
                return "과매도"; // Oversold
            else
                return "중립"; // Neutral
        }

        /// <summary>
        /// RSI 점수 계산 (0~10점)
        /// </summary>
        /// <param name="dailyPrices">일봉 데이터</param>
        /// <returns>RSI 기반 점수</returns>

        /// <summary>
        /// RSI 점수 계산 (0~10점)
        /// </summary>
        public static double CalculateScore(List<DailyPrice> dailyPrices)
        {
            var rsi = Calculate(dailyPrices);
            if (!rsi.HasValue)
                return 0;

            var rsiValue = rsi.Value;

            // RSI 30-40 구간에서 높은 점수 (매수 기회)
            if (rsiValue >= 30 && rsiValue <= 40)
                return 10; // 최고점
            else if (rsiValue >= 40 && rsiValue <= 50)
                return 7;
            else if (rsiValue >= 50 && rsiValue <= 60)
                return 5;
            else if (rsiValue >= 60 && rsiValue <= 70)
                return 3;
            else if (rsiValue > 70)
                return 0; // 과매수
            else // rsiValue < 30
                return 8; // 과매도이지만 위험
        }

      
    }
}