using System;
using System.Collections.Generic;
using System.Linq;
using StockTrader3.Models;

namespace StockTrader3.Indicators
{
    /// <summary>
    /// 볼린저 밴드 계산 클래스
    /// </summary>
    public static class BollingerBands
    {
        /// <summary>
        /// 볼린저 밴드 결과 클래스
        /// </summary>
        public class BollingerBandsResult
        {
            public decimal Upper { get; set; }      // 상단 밴드
            public decimal Middle { get; set; }     // 중간선 (이동평균)
            public decimal Lower { get; set; }      // 하단 밴드
            public decimal Width { get; set; }      // 밴드 폭
            public decimal Position { get; set; }   // 현재가 위치 (0~1)
        }

        /// <summary>
        /// 볼린저 밴드 계산
        /// </summary>
        /// <param name="dailyPrices">일봉 데이터</param>
        /// <param name="period">이동평균 기간 (기본 20일)</param>
        /// <param name="deviation">표준편차 배수 (기본 2.0)</param>
        /// <returns>볼린저 밴드 결과</returns>
        public static BollingerBandsResult Calculate(List<DailyPrice> dailyPrices, int period = 20, decimal deviation = 2.0m)
        {
            if (dailyPrices == null || dailyPrices.Count < period)
                return null;

            // 날짜순 정렬 후 최근 데이터 사용
            var sortedPrices = dailyPrices.OrderBy(x => x.Date).ToList();
            var recentPrices = new List<decimal>();

            // 최근 period개의 종가 수집
            for (int i = sortedPrices.Count - period; i < sortedPrices.Count; i++)
            {
                recentPrices.Add(sortedPrices[i].Close);
            }

            // 중간선 (이동평균) 계산
            var middle = recentPrices.Average();

            // 표준편차 계산
            var variance = recentPrices.Select(price => Math.Pow((double)(price - middle), 2)).Average();
            var standardDeviation = (decimal)Math.Sqrt(variance);

            // 상단/하단 밴드 계산
            var upper = middle + (standardDeviation * deviation);
            var lower = middle - (standardDeviation * deviation);

            // 현재가
            var currentPrice = sortedPrices.Last().Close;

            // 밴드 폭 계산
            var width = upper - lower;

            // 현재가 위치 계산 (0: 하단, 0.5: 중간, 1: 상단)
            var position = width > 0 ? (currentPrice - lower) / width : 0.5m;

            return new BollingerBandsResult
            {
                Upper = upper,
                Middle = middle,
                Lower = lower,
                Width = width,
                Position = position
            };
        }

        /// <summary>
        /// 볼린저 밴드 위치 상태 판단
        /// </summary>
        /// <param name="result">볼린저 밴드 결과</param>
        /// <returns>위치 상태</returns>
        public static string GetPositionStatus(BollingerBandsResult result)
        {
            if (result == null)
                return "데이터부족";

            if (result.Position >= 0.8m)
                return "상단근접"; // 과매수 구간
            else if (result.Position <= 0.2m)
                return "하단근접"; // 과매도 구간
            else if (result.Position >= 0.4m && result.Position <= 0.6m)
                return "중립"; // 중간 구간
            else if (result.Position > 0.6m)
                return "상단권"; // 상단 구간
            else
                return "하단권"; // 하단 구간
        }

        /// <summary>
        /// 볼린저 밴드 기반 점수 계산 (0~6점)
        /// </summary>
        /// <param name="dailyPrices">일봉 데이터</param>
        /// <returns>볼린저 밴드 점수</returns>
        public static double CalculateScore(List<DailyPrice> dailyPrices)
        {
            var result = Calculate(dailyPrices);
            if (result == null)
                return 0;

            var status = GetPositionStatus(result);

            switch (status)
            {
                case "하단근접":
                    return 6; // 과매도 구간 - 매수 기회
                case "하단권":
                    return 4;
                case "중립":
                    return 2;
                case "상단권":
                    return 1;
                case "상단근접":
                    return 0; // 과매수 구간 - 매수 지양
                default:
                    return 0;
            }
        }

        /// <summary>
        /// 볼린저 밴드 하단 근접 여부 (매수 신호)
        /// </summary>
        /// <param name="dailyPrices">일봉 데이터</param>
        /// <returns>true: 하단 근접</returns>
        public static bool IsNearLowerBand(List<DailyPrice> dailyPrices)
        {
            var result = Calculate(dailyPrices);
            if (result == null)
                return false;

            return result.Position <= 0.3m; // 하단 30% 이내
        }

        /// <summary>
        /// 볼린저 밴드 상단 돌파 여부 (매도 신호)
        /// </summary>
        /// <param name="dailyPrices">일봉 데이터</param>
        /// <returns>true: 상단 돌파</returns>
        public static bool IsAboveUpperBand(List<DailyPrice> dailyPrices)
        {
            var result = Calculate(dailyPrices);
            if (result == null)
                return false;

            var currentPrice = dailyPrices.OrderByDescending(x => x.Date).First().Close;
            return currentPrice > result.Upper;
        }

        /// <summary>
        /// 볼린저 밴드 수축 여부 (변동성 감소)
        /// </summary>
        /// <param name="dailyPrices">일봉 데이터</param>
        /// <returns>true: 밴드 수축 중</returns>
        public static bool IsBandContraction(List<DailyPrice> dailyPrices)
        {
            if (dailyPrices.Count < 25)
                return false;

            var currentBB = Calculate(dailyPrices);
            var previousBB = Calculate(dailyPrices.Take(dailyPrices.Count - 1).ToList());

            if (currentBB == null || previousBB == null)
                return false;

            // 현재 밴드 폭이 이전보다 작으면 수축
            return currentBB.Width < previousBB.Width;
        }

        /// <summary>
        /// 매수가 보정 (볼린저 밴드 하단 고려)
        /// </summary>
        /// <param name="originalBuyPrice">원래 매수가</param>
        /// <param name="dailyPrices">일봉 데이터</param>
        /// <returns>보정된 매수가</returns>
        public static decimal? AdjustBuyPrice(decimal originalBuyPrice, List<DailyPrice> dailyPrices)
        {
            var result = Calculate(dailyPrices);
            if (result == null)
                return originalBuyPrice;

            // 볼린저 하단이 원래 매수가보다 높으면 하단 사용
            if (result.Lower > originalBuyPrice)
                return result.Lower;

            // 볼린저 하단과 원래 매수가의 중간값 사용
            return (originalBuyPrice + result.Lower) / 2;
        }

        /// <summary>
        /// 변동성 상태 판단
        /// </summary>
        /// <param name="dailyPrices">일봉 데이터</param>
        /// <returns>변동성 상태</returns>
        public static string GetVolatilityState(List<DailyPrice> dailyPrices)
        {
            var result = Calculate(dailyPrices);
            if (result == null)
                return "데이터부족";

            var currentPrice = dailyPrices.OrderByDescending(x => x.Date).First().Close;
            var widthPercent = (result.Width / currentPrice) * 100;

            if (widthPercent > 10)
                return "고변동성";
            else if (widthPercent > 5)
                return "중변동성";
            else
                return "저변동성";
        }
    }
}