using System;
using System.Collections.Generic;
using System.Linq;
using StockTrader3.Models;
// --- PriceCorrector를 사용하기 위해 네임스페이스 추가 ---
using StockTrader3_Shared.Utility; // 또는 PriceCorrector가 있는 실제 네임스페이스

namespace StockTrader3.Indicators
{
    /// <summary>
    /// ATR (Average True Range) 계산 클래스
    /// 중소형주 단타 매매 최적화 (1.5-2% 목표)
    /// </summary>
    public static class ATR
    {
        // ... Calculate, Calculate5DayATR, Calculate3DayATR 메서드는 그대로 둡니다 ...
        // (이전 코드와 동일하므로 생략)
        public static decimal? Calculate(List<DailyPrice> dailyPrices, int period = 14)
        {
            if (dailyPrices == null || dailyPrices.Count < period + 1)
                return null;
            var sortedPrices = dailyPrices.OrderBy(x => x.Date).ToList();
            var trueRanges = new List<decimal>();
            for (int i = 1; i < sortedPrices.Count; i++)
            {
                var current = sortedPrices[i];
                var previous = sortedPrices[i - 1];
                var tr1 = current.High - current.Low;
                var tr2 = Math.Abs(current.High - previous.Close);
                var tr3 = Math.Abs(current.Low - previous.Close);
                var trueRange = Math.Max(tr1, Math.Max(tr2, tr3));
                trueRanges.Add(trueRange);
            }
            if (trueRanges.Count < period)
                return null;
            var recentTR = new List<decimal>();
            for (int i = trueRanges.Count - period; i < trueRanges.Count; i++)
            {
                recentTR.Add(trueRanges[i]);
            }
            return recentTR.Average();
        }
        public static decimal? Calculate5DayATR(List<DailyPrice> dailyPrices)
        {
            return Calculate(dailyPrices, 5);
        }
        public static decimal? Calculate3DayATR(List<DailyPrice> dailyPrices)
        {
            return Calculate(dailyPrices, 3);
        }


        /// <summary>
        /// ATR 기반 매수가 계산 (호가 단위 보정 추가)
        /// </summary>
        public static decimal? CalculateBuyPrice(List<DailyPrice> dailyPrices, decimal multiplier = 0.7m)
        {
            if (dailyPrices == null || dailyPrices.Count < 5)
                return null;

            var atr5 = Calculate5DayATR(dailyPrices);
            if (!atr5.HasValue)
                return null;

            var ma5 = MovingAverage.GetCurrentSMA(dailyPrices, 5);
            if (!ma5.HasValue)
                return null;

            // 매수가 = 5일선 - (5일 ATR × 배수)
            decimal calculatedPrice = ma5.Value - (atr5.Value * multiplier);

            // --- 수정된 부분 ---
            // 계산된 가격을 호가 단위에 맞게 보정
            return PriceCorrector.GetCorrectedPrice((int)calculatedPrice);
        }

        /// <summary>
        /// 수정된 ATR 기반 손절가 계산 (호가 단위 보정 추가)
        /// </summary>
        public static decimal? CalculateStopLoss(decimal buyPrice, List<DailyPrice> dailyPrices, decimal multiplier = 1.0m)
        {
            var atr3 = Calculate3DayATR(dailyPrices);
            decimal calculatedPrice;

            if (!atr3.HasValue)
            {
                calculatedPrice = buyPrice * 0.985m; // ATR 없으면 1.5% 고정 손절
            }
            else
            {
                var currentPrice = dailyPrices.OrderByDescending(x => x.Date).First().Close;
                var atrPercent = atr3.Value / currentPrice;
                var stopLossRate = Math.Min(atrPercent * 0.8m, 0.015m);
                calculatedPrice = buyPrice * (1 - stopLossRate);
            }

            // --- 수정된 부분 ---
            // 계산된 가격을 호가 단위에 맞게 보정
            return PriceCorrector.GetCorrectedPrice((int)calculatedPrice);
        }

        /// <summary>
        /// 수정된 목표가 계산 (호가 단위 보정 추가)
        /// </summary>
        public static decimal? CalculateTargetPrice(decimal buyPrice, List<DailyPrice> dailyPrices, string technicalGrade = "B")
        {
            var targetRate = GetTargetRate(technicalGrade);
            decimal calculatedPrice = buyPrice * (1 + targetRate);

            // --- 수정된 부분 ---
            // 계산된 가격을 호가 단위에 맞게 보정
            return PriceCorrector.GetCorrectedPrice((int)calculatedPrice);
        }

        // ... GetTargetRate, CalculateOptimalStopLossRate, GetVolatilityLevel 등 나머지 메서드는 그대로 둡니다 ...
        // (이전 코드와 동일하므로 생략)
        public static decimal GetTargetRate(string technicalGrade)
        {
            switch (technicalGrade)
            {
                case "S": return 0.020m;
                case "A": return 0.018m;
                case "B": return 0.015m;
                default: return 0.015m;
            }
        }
        public static decimal CalculateOptimalStopLossRate(List<DailyPrice> dailyPrices)
        {
            var atr3 = Calculate3DayATR(dailyPrices);
            if (!atr3.HasValue) return 0.015m;
            var currentPrice = dailyPrices.OrderByDescending(x => x.Date).First().Close;
            var atrPercent = atr3.Value / currentPrice;
            return Math.Min(atrPercent * 0.8m, 0.015m);
        }
        public static string GetVolatilityLevel(List<DailyPrice> dailyPrices)
        {
            var atr = Calculate(dailyPrices);
            if (!atr.HasValue) return "데이터부족";
            var currentPrice = dailyPrices.OrderByDescending(x => x.Date).First().Close;
            var atrPercent = (atr.Value / currentPrice) * 100;
            if (atrPercent > 8) return "극고변동성";
            else if (atrPercent > 5) return "고변동성";
            else if (atrPercent > 3) return "중변동성";
            else return "저변동성";
        }


        /// <summary>
        /// 🔄 수정된 매매 계획 계산 (1.5-2% 목표)
        /// </summary>
        public static (decimal? buyPrice, decimal? targetPrice, decimal? stopLoss) CalculateTradePlan(List<DailyPrice> dailyPrices, string technicalGrade = "B")
        {
            var buyPrice = CalculateBuyPrice(dailyPrices);
            if (!buyPrice.HasValue)
                return (null, null, null);

            var targetPrice = CalculateTargetPrice(buyPrice.Value, dailyPrices, technicalGrade);
            var stopLoss = CalculateStopLoss(buyPrice.Value, dailyPrices);

            return (buyPrice, targetPrice, stopLoss);
        }

        // ... CalculateRiskRewardRatio, GetDayTradingSuitability 등 나머지 메서드도 그대로 둡니다 ...
        public static double CalculateRiskRewardRatio(decimal buyPrice, decimal targetPrice, decimal stopLoss)
        {
            if (buyPrice <= 0 || buyPrice <= stopLoss) return 0;
            var potentialGain = targetPrice - buyPrice;
            var potentialLoss = buyPrice - stopLoss;
            if (potentialLoss <= 0) return double.MaxValue;
            return (double)(potentialGain / potentialLoss);
        }
        public static double GetDayTradingSuitability(List<DailyPrice> dailyPrices)
        {
            if (dailyPrices.Count < 5) return 0;
            double score = 0;
            var recent5 = dailyPrices.OrderByDescending(x => x.Date).Take(5).ToList();
            var volatility = GetVolatilityLevel(dailyPrices);
            switch (volatility)
            {
                case "극고변동성": score += 3; break;
                case "고변동성": score += 4; break;
                case "중변동성": score += 3.5; break;
                case "저변동성": score += 2; break;
                default: score += 0; break;
            }
            if (recent5.Count >= 2)
            {
                var avgVolume = recent5.Skip(1).Average(x => (double)x.Volume);
                var todayVolume = (double)recent5[0].Volume;
                if (todayVolume > avgVolume * 2) score += 3;
                else if (todayVolume > avgVolume * 1.5) score += 2.5;
                else if (todayVolume > avgVolume) score += 2;
                else score += 1;
            }
            if (recent5.Count >= 5)
            {
                var trend = (recent5[0].Close - recent5[4].Close) / recent5[4].Close;
                if (trend > 0.05m) score += 3;
                else if (trend > 0.02m) score += 2.5;
                else if (trend > -0.02m) score += 2;
                else score += 1;
            }
            return Math.Round(score, 1);
        }
    }
}