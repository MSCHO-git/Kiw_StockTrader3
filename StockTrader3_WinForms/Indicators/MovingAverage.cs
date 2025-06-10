using System;
using System.Collections.Generic;
using System.Linq;
using StockTrader3.Models;

namespace StockTrader3.Indicators
{
    public static class MovingAverage
    {
        public static decimal? GetCurrentSMA(List<DailyPrice> dailyPrices, int period)
        {
            if (dailyPrices == null || dailyPrices.Count < period)
                return null;

            var recentPrices = dailyPrices
                .OrderByDescending(x => x.Date)
                .Take(period)
                .Select(x => x.Close)
                .ToList();

            return recentPrices.Average();
        }
    }
}