using System;
using System.Collections.Generic;
using System.Linq;
using StockTrader3.Models;

namespace StockTrader3.Indicators
{
    /// <summary>
    /// ATR (Average True Range) 계산 클래스
    /// 중소형주 단타 매매 최적화 (1.5-2% 목표)
    /// </summary>
    public static class ATR
    {
        /// <summary>
        /// ATR 계산
        /// </summary>
        /// <param name="dailyPrices">일봉 데이터</param>
        /// <param name="period">ATR 기간 (기본 14일)</param>
        /// <returns>ATR 값</returns>
        public static decimal? Calculate(List<DailyPrice> dailyPrices, int period = 14)
        {
            if (dailyPrices == null || dailyPrices.Count < period + 1)
                return null;

            // 날짜순 정렬
            var sortedPrices = dailyPrices.OrderBy(x => x.Date).ToList();

            // True Range 계산
            var trueRanges = new List<decimal>();

            for (int i = 1; i < sortedPrices.Count; i++)
            {
                var current = sortedPrices[i];
                var previous = sortedPrices[i - 1];

                // True Range = MAX(고가-저가, |고가-전일종가|, |저가-전일종가|)
                var tr1 = current.High - current.Low;
                var tr2 = Math.Abs(current.High - previous.Close);
                var tr3 = Math.Abs(current.Low - previous.Close);

                var trueRange = Math.Max(tr1, Math.Max(tr2, tr3));
                trueRanges.Add(trueRange);
            }

            if (trueRanges.Count < period)
                return null;

            // 최근 period개의 True Range 평균
            var recentTR = new List<decimal>();
            for (int i = trueRanges.Count - period; i < trueRanges.Count; i++)
            {
                recentTR.Add(trueRanges[i]);
            }

            return recentTR.Average();
        }

        /// <summary>
        /// 5일 ATR 계산 (우리 전략의 핵심)
        /// </summary>
        /// <param name="dailyPrices">일봉 데이터</param>
        /// <returns>5일 ATR 값</returns>
        public static decimal? Calculate5DayATR(List<DailyPrice> dailyPrices)
        {
            return Calculate(dailyPrices, 5);
        }

        /// <summary>
        /// 🆕 3일 ATR 계산 (중소형주 특화)
        /// </summary>
        /// <param name="dailyPrices">일봉 데이터</param>
        /// <returns>3일 ATR 값</returns>
        public static decimal? Calculate3DayATR(List<DailyPrice> dailyPrices)
        {
            return Calculate(dailyPrices, 3); // 기존 Calculate 메서드 활용
        }

        /// <summary>
        /// ATR 기반 매수가 계산
        /// </summary>
        /// <param name="dailyPrices">일봉 데이터</param>
        /// <param name="multiplier">ATR 곱수 (기본 0.7)</param>
        /// <returns>예상 매수가</returns>
        public static decimal? CalculateBuyPrice(List<DailyPrice> dailyPrices, decimal multiplier = 0.7m)
        {
            if (dailyPrices == null || dailyPrices.Count < 5)
                return null;

            var atr5 = Calculate5DayATR(dailyPrices);
            if (!atr5.HasValue)
                return null;

            // 5일 이동평균 계산
            var ma5 = MovingAverage.GetCurrentSMA(dailyPrices, 5);
            if (!ma5.HasValue)
                return null;

            // 매수가 = 5일선 - (5일 ATR × 배수)
            return ma5.Value - (atr5.Value * multiplier);
        }

        /// <summary>
        /// 🔄 수정된 ATR 기반 손절가 계산 (최대 1.5% 제한)
        /// </summary>
        /// <param name="buyPrice">매수가</param>
        /// <param name="dailyPrices">일봉 데이터</param>
        /// <param name="multiplier">ATR 곱수 (기본 1.0)</param>
        /// <returns>손절가</returns>
        public static decimal? CalculateStopLoss(decimal buyPrice, List<DailyPrice> dailyPrices, decimal multiplier = 1.0m)
        {
            var atr3 = Calculate3DayATR(dailyPrices); // 3일 ATR 사용 (중소형주 특화)
            if (!atr3.HasValue)
                return buyPrice * 0.985m; // ATR 없으면 1.5% 고정 손절

            var currentPrice = dailyPrices.OrderByDescending(x => x.Date).First().Close;
            var atrPercent = atr3.Value / currentPrice;

            // ATR 기반 손절폭 계산 (최대 1.5% 제한)
            var stopLossRate = Math.Min(atrPercent * 0.8m, 0.015m);

            return buyPrice * (1 - stopLossRate);
        }

        /// <summary>
        /// 🔄 수정된 목표가 계산 (1.5-2% 고정 목표)
        /// </summary>
        /// <param name="buyPrice">매수가</param>
        /// <param name="dailyPrices">일봉 데이터</param>
        /// <param name="technicalGrade">기술적 등급 (S/A/B)</param>
        /// <returns>목표가</returns>
        public static decimal? CalculateTargetPrice(decimal buyPrice, List<DailyPrice> dailyPrices, string technicalGrade = "B")
        {
            // 🆕 등급별 목표 수익률 (ATR 무관, 고정 비율)
            var targetRate = GetTargetRate(technicalGrade);
            return buyPrice * (1 + targetRate);
        }

        /// <summary>
        /// 🆕 등급별 목표 수익률 계산
        /// </summary>
        /// <param name="technicalGrade">기술적 등급</param>
        /// <returns>목표 수익률 (0.015 = 1.5%)</returns>
        public static decimal GetTargetRate(string technicalGrade)
        {
            // ✅ 수정된 코드 (C# 7.3 호환)
            switch (technicalGrade)
            {
                case "S": return 0.020m; // 2.0%
                case "A": return 0.018m; // 1.8%
                case "B": return 0.015m; // 1.5%
                default: return 0.015m;  // 기본 1.5%
            }
        }

        /// <summary>
        /// 🆕 변동성 기반 최적 손절폭 계산
        /// </summary>
        /// <param name="dailyPrices">일봉 데이터</param>
        /// <returns>손절폭 비율 (0.01 = 1%)</returns>
        public static decimal CalculateOptimalStopLossRate(List<DailyPrice> dailyPrices)
        {
            var atr3 = Calculate3DayATR(dailyPrices);
            if (!atr3.HasValue)
                return 0.015m; // 기본 1.5%

            var currentPrice = dailyPrices.OrderByDescending(x => x.Date).First().Close;
            var atrPercent = atr3.Value / currentPrice;

            // ATR의 80% 또는 최대 1.5% 제한
            return Math.Min(atrPercent * 0.8m, 0.015m);
        }

        /// <summary>
        /// 변동성 상태 판단 (중소형주 기준 조정)
        /// </summary>
        /// <param name="dailyPrices">일봉 데이터</param>
        /// <returns>변동성 수준</returns>
        public static string GetVolatilityLevel(List<DailyPrice> dailyPrices)
        {
            var atr = Calculate(dailyPrices);
            if (!atr.HasValue)
                return "데이터부족";

            var currentPrice = dailyPrices.OrderByDescending(x => x.Date).First().Close;
            var atrPercent = (atr.Value / currentPrice) * 100;

            // 🔄 중소형주 기준으로 조정
            if (atrPercent > 8)
                return "극고변동성"; // 8% 이상
            else if (atrPercent > 5)
                return "고변동성";   // 5-8%
            else if (atrPercent > 3)
                return "중변동성";   // 3-5%
            else
                return "저변동성";   // 3% 미만
        }

        /// <summary>
        /// 🔄 수정된 매매 계획 계산 (1.5-2% 목표)
        /// </summary>
        /// <param name="dailyPrices">일봉 데이터</param>
        /// <param name="technicalGrade">기술적 등급</param>
        /// <returns>매수가, 목표가, 손절가</returns>
        public static (decimal? buyPrice, decimal? targetPrice, decimal? stopLoss) CalculateTradePlan(List<DailyPrice> dailyPrices, string technicalGrade = "B")
        {
            var buyPrice = CalculateBuyPrice(dailyPrices);
            if (!buyPrice.HasValue)
                return (null, null, null);

            var targetPrice = CalculateTargetPrice(buyPrice.Value, dailyPrices, technicalGrade);
            var stopLoss = CalculateStopLoss(buyPrice.Value, dailyPrices);

            return (buyPrice, targetPrice, stopLoss);
        }

        /// <summary>
        /// 🆕 리스크/리워드 비율 계산
        /// </summary>
        /// <param name="buyPrice">매수가</param>
        /// <param name="targetPrice">목표가</param>
        /// <param name="stopLoss">손절가</param>
        /// <returns>리스크/리워드 비율</returns>
        public static double CalculateRiskRewardRatio(decimal buyPrice, decimal targetPrice, decimal stopLoss)
        {
            if (buyPrice <= 0 || buyPrice <= stopLoss)
                return 0;

            var potentialGain = targetPrice - buyPrice;
            var potentialLoss = buyPrice - stopLoss;

            if (potentialLoss <= 0)
                return double.MaxValue;

            return (double)(potentialGain / potentialLoss);
        }

        /// <summary>
        /// 🆕 중소형주 단타 적합성 점수 (0-10점)
        /// </summary>
        /// <param name="dailyPrices">일봉 데이터</param>
        /// <returns>단타 적합성 점수</returns>
        public static double GetDayTradingSuitability(List<DailyPrice> dailyPrices)
        {
            if (dailyPrices.Count < 5)
                return 0;

            double score = 0;
            var recent5 = dailyPrices.OrderByDescending(x => x.Date).Take(5).ToList();

            // 1. 변동성 점수 (4점) - 중소형주 기준
            var volatility = GetVolatilityLevel(dailyPrices);
            switch (volatility)
            {
                case "극고변동성": score += 3; break; // 너무 위험
                case "고변동성": score += 4; break;   // 최적
                case "중변동성": score += 3.5; break; // 좋음
                case "저변동성": score += 2; break;   // 수익 한계
                default: score += 0; break;
            }

            // 2. 거래량 점수 (3점)
            if (recent5.Count >= 2)
            {
                var avgVolume = recent5.Skip(1).Average(x => (double)x.Volume);
                var todayVolume = (double)recent5[0].Volume;

                if (todayVolume > avgVolume * 2)
                    score += 3;
                else if (todayVolume > avgVolume * 1.5)
                    score += 2.5;
                else if (todayVolume > avgVolume)
                    score += 2;
                else
                    score += 1;
            }

            // 3. 추세 점수 (3점)
            if (recent5.Count >= 5)
            {
                var trend = (recent5[0].Close - recent5[4].Close) / recent5[4].Close;
                if (trend > 0.05m) // 5일간 5% 이상 상승
                    score += 3;
                else if (trend > 0.02m) // 2% 이상 상승
                    score += 2.5;
                else if (trend > -0.02m) // 횡보
                    score += 2;
                else
                    score += 1; // 하락 추세
            }

            return Math.Round(score, 1);
        }
    }
}