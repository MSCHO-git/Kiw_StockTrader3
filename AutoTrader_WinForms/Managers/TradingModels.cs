using System;
using System.Collections.Generic;

namespace AutoTrader_WinForms.Managers
{
    /// <summary>
    /// 주문 결과 정보
    /// </summary>
    public class OrderResult
    {
        public bool Success { get; set; }
        public string OrderNo { get; set; }
        public string Status { get; set; }
        public int FilledQuantity { get; set; }
        public int RemainQuantity { get; set; }
        public int AvgPrice { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime OrderTime { get; set; } = DateTime.Now;

        public double FilledRatio =>
            (FilledQuantity + RemainQuantity) > 0 ?
            (double)FilledQuantity / (FilledQuantity + RemainQuantity) : 0;
    }

    /// <summary>
    /// 매수 결과 - 이름 변경으로 충돌 방지
    /// </summary>
    public class BuyResult
    {
        public bool Success;
        public string Reason;
        public int FilledQuantity;
        public int AvgPrice;

        public bool Failed
        {
            get { return !Success; }
        }

        public BuyResult()
        {
            Success = false;
            Reason = "";
            FilledQuantity = 0;
            AvgPrice = 0;
        }

        // --- 여기부터 메서드 이름이 수정되었습니다 ---

        public static BuyResult CreateBlocked(string reason)
        {
            var result = new BuyResult();
            result.Success = false;
            result.Reason = reason;
            result.FilledQuantity = 0;
            result.AvgPrice = 0;
            return result;
        }

        public static BuyResult CreateFailed(string reason)
        {
            var result = new BuyResult();
            result.Success = false;
            result.Reason = reason;
            result.FilledQuantity = 0;
            result.AvgPrice = 0;
            return result;
        }

        public static BuyResult CreatePartialGiveUp(string reason)
        {
            var result = new BuyResult();
            result.Success = false;
            result.Reason = reason;
            result.FilledQuantity = 0;
            result.AvgPrice = 0;
            return result;
        }

        public static BuyResult CreateSuccess(int quantity, int avgPrice)
        {
            var result = new BuyResult();
            result.Success = true;
            result.Reason = "매수 성공";
            result.FilledQuantity = quantity;
            result.AvgPrice = avgPrice;
            return result;
        }
    }

    /// <summary>
    /// 주문 체결 정보
    /// </summary>
    public class OrderExecutionInfo
    {
        public string OrderNo { get; set; }
        public string StockCode { get; set; }
        public string StockName { get; set; }
        public string OrderType { get; set; }
        public int OrderQuantity { get; set; }
        public int OrderPrice { get; set; }
        public int FilledQuantity { get; set; }
        public int AvgFillPrice { get; set; }
        public string Status { get; set; }
        public DateTime OrderTime { get; set; }
        public DateTime? FillTime { get; set; }

        public int RemainQuantity => OrderQuantity - FilledQuantity;
    }

    /// <summary>
    /// 일일 거래 관리자
    /// </summary>
    public class DailyTradingManager
    {
        private HashSet<string> tradedToday = new HashSet<string>();
        private decimal dailyRealized = 0m;
        private int tradingCount = 0;
        private readonly decimal DAILY_LOSS_LIMIT = -600_000m;
        private readonly int MAX_DAILY_TRADES = 20;

        public bool CanTrade(string stockCode)
        {
            if (tradedToday.Contains(stockCode))
            {
                return false;
            }

            if (dailyRealized <= DAILY_LOSS_LIMIT)
            {
                return false;
            }

            if (tradingCount >= MAX_DAILY_TRADES)
            {
                return false;
            }

            return true;
        }

        public void AddTrade(string stockCode, decimal realizedPL)
        {
            tradedToday.Add(stockCode);
            dailyRealized += realizedPL;
            tradingCount++;
        }

        public void ResetDaily()
        {
            tradedToday.Clear();
            dailyRealized = 0m;
            tradingCount = 0;
        }

        public int TradedStockCount => tradedToday.Count;
        public decimal DailyRealized => dailyRealized;
        public int TradingCount => tradingCount;
        public bool IsLossLimitReached => dailyRealized <= DAILY_LOSS_LIMIT;
        public bool IsTradeCountLimitReached => tradingCount >= MAX_DAILY_TRADES;
    }

    /// <summary>
    /// 매매 신호 타입
    /// </summary>
    public enum SellSignal
    {
        Hold,
        ProfitTarget,
        StopLoss,
        TimeLimit,
        EmergencyExit
    }

    /// <summary>
    /// 매도 결정 클래스
    /// </summary>
    public class SellDecision
    {
        public bool ShouldSell { get; set; }
        public string Reason { get; set; }
        public double SellRatio { get; set; } = 1.0;

        public static SellDecision Hold => new SellDecision { ShouldSell = false, Reason = "보유 유지" };

        public static SellDecision ImmediateSell(string reason, double ratio = 1.0) => new SellDecision
        {
            ShouldSell = true,
            Reason = reason,
            SellRatio = ratio
        };
    }

    /// <summary>
    /// 매수 결정 클래스
    /// </summary>
    public class BuyDecision
    {
        public bool ShouldBuy { get; set; }
        public decimal InvestmentAmount { get; set; }
        public int Priority { get; set; }
        public string Reason { get; set; }

        public static BuyDecision Reject => new BuyDecision { ShouldBuy = false, Reason = "매수 조건 불충족" };
    }

    /// <summary>
    /// 실제 매매용 포지션
    /// </summary>

    // TradingModels.cs -> RealTradingPosition 클래스

    /// <summary>
    /// 실제 매매용 포지션
    /// </summary>
    public class RealTradingPosition
    {
        public string StockCode { get; set; }
        public string StockName { get; set; }
        public TradingStock Stock { get; set; }

        public int PlannedQuantity { get; set; }
        public int ActualQuantity { get; set; }
        public int PlannedBuyPrice { get; set; }
        public int ActualAvgBuyPrice { get; set; }

        public string BuyOrderNo { get; set; }
        public string SellOrderNo { get; set; }
        public List<OrderExecutionInfo> OrderHistory { get; set; } = new List<OrderExecutionInfo>();

        public int CurrentPrice { get; set; }
        public int MaxPrice { get; set; }
        public int MinPrice { get; set; }

        public DateTime BuyTime { get; set; }
        public DateTime? SellTime { get; set; }

        public PositionStatus Status { get; set; }

        // --- 여기부터 UI 표시용 계산 속성들 ---

        public decimal UnrealizedPL => ActualQuantity > 0 ? (decimal)(CurrentPrice - ActualAvgBuyPrice) * ActualQuantity : 0;

        public double ReturnRate => ActualAvgBuyPrice > 0 ? (double)(CurrentPrice - ActualAvgBuyPrice) / ActualAvgBuyPrice : 0;

        public decimal InvestmentAmount => (decimal)ActualAvgBuyPrice * ActualQuantity;

        public string StatusDisplay
        {
            get
            {
                switch (Status)
                {
                    case PositionStatus.Watching: return "🔍 감시중";
                    case PositionStatus.Buying: return "🟡 매수중";
                    case PositionStatus.Holding: return "🟢 보유중";
                    case PositionStatus.Selling: return "🔵 매도중";
                    case PositionStatus.Completed: return "⚫ 완료";
                    case PositionStatus.Cancelled: return "⚪ 취소";
                    default: return "⚪ 대기";
                }
            }
        }

        public string HoldingMinutesDisplay
        {
            get
            {
                if (Status != PositionStatus.Holding && Status != PositionStatus.Selling) return "-";
                var elapsed = (SellTime ?? DateTime.Now) - BuyTime;
                if (elapsed.TotalMinutes < 60)
                    return $"{Math.Round(elapsed.TotalMinutes)}분";
                else
                    return $"{elapsed.Hours}시간 {elapsed.Minutes}분";
            }
        }

        public SellSignal CheckSellSignal()
        {
            if (CurrentPrice >= Stock.SellPrice)
                return SellSignal.ProfitTarget;

            if (CurrentPrice <= Stock.StopLossPrice)
                return SellSignal.StopLoss;

            if (MaxPrice > ActualAvgBuyPrice * 1.1 &&
                CurrentPrice < MaxPrice * 0.95)
                return SellSignal.EmergencyExit;

            if (CurrentPrice <= ActualAvgBuyPrice * 0.95)
                return SellSignal.EmergencyExit;

            var holdingMinutes = (DateTime.Now - BuyTime).TotalMinutes;
            if (holdingMinutes > 180)
                return SellSignal.TimeLimit;

            return SellSignal.Hold;
        }
    }


  
    /// <summary>
    /// 포지션 상태
    /// </summary>
    public enum PositionStatus
    {
        Ready,
        Watching,
        Buying,
        Holding,
        ProfitTaken,
        StopLoss,
        ForceClosed,
        Selling,
        Completed,
        Cancelled
    }
}