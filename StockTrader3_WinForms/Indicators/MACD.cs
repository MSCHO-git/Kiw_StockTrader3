using System;
using System.Collections.Generic;
using System.Linq;
using StockTrader3.Models;

namespace StockTrader3.Indicators
{
    /// <summary>
    /// MACD (Moving Average Convergence Divergence) 계산 클래스
    /// </summary>
    public static class MACD
    {
        /// <summary>
        /// MACD 결과 클래스
        /// </summary>
        public class MACDResult
        {
            public decimal MACD { get; set; }
            public decimal Signal { get; set; }
            public decimal Histogram { get; set; }
        }

        /// <summary>
        /// MACD 계산 (간단 버전)
        /// </summary>
        /// <param name="dailyPrices">일봉 데이터</param>
        /// <returns>MACD 결과</returns>
        public static MACDResult Calculate(List<DailyPrice> dailyPrices)
        {
            if (dailyPrices == null || dailyPrices.Count < 26)
                return null;

            // 날짜순 정렬
            var sortedPrices = dailyPrices.OrderBy(x => x.Date).ToList();
            var closePrices = sortedPrices.Select(x => x.Close).ToList();

            if (closePrices.Count < 26)
                return null;

            // 12일 EMA와 26일 EMA 계산 (간단 버전)
            var ema12 = CalculateSimpleEMA(closePrices, 12);
            var ema26 = CalculateSimpleEMA(closePrices, 26);

            if (ema12 == 0 || ema26 == 0)
                return null;

            var macdValue = ema12 - ema26;

            // Signal은 MACD의 9일 평균으로 간단히 계산
            var signalValue = macdValue; // 임시로 같은 값 사용

            return new MACDResult
            {
                MACD = macdValue,
                Signal = signalValue,
                Histogram = macdValue - signalValue
            };
        }

        /// <summary>
        /// 간단한 EMA 계산
        /// </summary>
        private static decimal CalculateSimpleEMA(List<decimal> prices, int period)
        {
            if (prices.Count < period)
                return 0;

            // 최근 period개의 평균으로 간단 계산
            var recentPrices = new List<decimal>();
            for (int i = prices.Count - period; i < prices.Count; i++)
            {
                recentPrices.Add(prices[i]);
            }

            return recentPrices.Average();
        }

        /// <summary>
        /// MACD 신호 판단
        /// </summary>
        /// <param name="macdResult">MACD 결과</param>
        /// <returns>매매 신호</returns>
        public static string GetSignal(MACDResult macdResult)
        {
            if (macdResult == null)
                return "데이터부족";

            if (macdResult.MACD > 0)
                return "상승";
            else
                return "하락";
        }

        /// <summary>
        /// MACD 점수 계산 (0~8점)
        /// </summary>
        /// <param name="dailyPrices">일봉 데이터</param>
        /// <returns>MACD 기반 점수</returns>
        public static double CalculateScore(List<DailyPrice> dailyPrices)
        {
            var macdResult = Calculate(dailyPrices);
            if (macdResult == null)
                return 0;

            var signal = GetSignal(macdResult);

            if (signal == "상승")
                return 6;
            else
                return 2;
        }

        /// <summary>
        /// 골든크로스 임박 여부 확인 (간단 버전)
        /// </summary>
        /// <param name="dailyPrices">일봉 데이터</param>
        /// <returns>true: 골든크로스 임박</returns>
        public static bool IsGoldenCrossImminent(List<DailyPrice> dailyPrices)
        {
            var macdResult = Calculate(dailyPrices);
            if (macdResult == null)
                return false;

            // 간단히 MACD가 양수면 골든크로스로 판단
            return macdResult.MACD > 0;
        }
    }
}