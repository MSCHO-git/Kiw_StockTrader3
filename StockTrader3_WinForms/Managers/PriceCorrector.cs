using System;

namespace StockTrader3_Shared.Utility // 또는 프로젝트에 맞는 네임스페이스
{
    /// <summary>
    /// 주식 호가단위에 맞게 가격을 보정하는 유틸리티 클래스
    /// </summary>
    public static class PriceCorrector
    {
        /// <summary>
        /// 주어진 가격을 대한민국 주식 시장의 호가단위에 맞게 보정합니다.
        /// </summary>
        /// <param name="price">보정할 원본 가격</param>
        /// <returns>호가단위에 맞게 보정된 가격</returns>
        public static int GetCorrectedPrice(int price)
        {
            if (price <= 0) return 0;

            int tick;

            if (price < 2000)
                tick = 1;
            else if (price < 5000)
                tick = 5;
            else if (price < 20000)
                tick = 10;
            else if (price < 50000)
                tick = 50;
            else if (price < 200000)
                tick = 100;
            else if (price < 500000)
                tick = 500;
            else
                tick = 1000;

            // 가격을 호가단위로 나눈 나머지를 구해서, 원래 가격에서 빼주어 호가단위에 맞춤
            return price - (price % tick);
        }
    }
}