using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StockTrader3.Models;
using StockTrader3.Indicators;

namespace StockTrader3.Analysis
{
    /// <summary>
    /// 업그레이드된 기술적 분석 종합 클래스 (강세 종목 + 눌림목 매수 최적화)
    /// 일봉(75점) + 분봉(20점) = 80점 만점
    /// </summary>
    public class TechnicalAnalyzer
    {
        /// <summary>
        /// 업그레이드된 기술적 분석 결과 클래스
        /// </summary>
        public class TechnicalAnalysisResult
        {
            // 개선된 점수 체계 (80점 만점)
            public double TrendScore { get; set; }           // 추세 점수 (25점) - 상향
            public double MomentumScore { get; set; }        // 모멘텀 점수 (25점) - 상향
            public double PatternScore { get; set; }         // 패턴 점수 (15점) - 상향
            public double SupportResistanceScore { get; set; } // 지지/저항 점수 (10점)
            public double SupplyDemandScore { get; set; }    // 수급 점수 (5점)

            // 🆕 분봉 분석 (최대 20점 추가, 없으면 일봉 보정)
            public double MinuteTrendScore { get; set; }     // 분봉 추세 점수 (8점)
            public double MinuteMomentumScore { get; set; }  // 분봉 모멘텀 점수 (7점)
            public double MinuteTimingScore { get; set; }    // 진입 타이밍 점수 (5점)

            public double TotalScore { get; set; }           // 총점 (80점 만점)

            // 상세 정보
            public string TrendStatus { get; set; }
            public string MomentumStatus { get; set; }
            public string PatternStatus { get; set; }
            public string MinuteAnalysis { get; set; }
            public string RiskWarning { get; set; }

            // 🆕 정밀한 매매 계획 (분봉 기반)
            public decimal? BuyPrice { get; set; }
            public decimal? TargetPrice { get; set; }
            public decimal? StopLossPrice { get; set; }
            public decimal? DailyATR { get; set; }
            public decimal? MinuteATR { get; set; }
            public string TradingStrategy { get; set; }

            // 🆕 다중 시간대 확인
            public bool IsMultiTimeFrameAligned { get; set; }
            public string TimeFrameAnalysis { get; set; }

            // MainForm.cs 호환용 속성들
            public double RSI { get; set; }
            public double MACD { get; set; }
            public double MACDSignal { get; set; }
            public decimal BollingerUpper { get; set; }
            public decimal BollingerLower { get; set; }
            public double BollingerPosition { get; set; }

            // 이동평균선 정보
            public decimal MA5 { get; set; }
            public decimal MA20 { get; set; }
            public decimal MA60 { get; set; }

            // 등급 및 분석 품질
            public string TechnicalGrade { get; set; }
            public string FinalGrade { get; set; }
            public bool UsedMinuteData { get; set; }
            public string AnalysisType { get; set; }
            public string Confidence { get; set; }

            // 🚀 정밀한 매매가 (MainForm.cs에서 OptimizedXXX로 접근)
            public decimal OptimizedBuyPrice
            {
                get { return BuyPrice ?? 0; }
                set { BuyPrice = value; }
            }

            public decimal OptimizedTargetPrice
            {
                get { return TargetPrice ?? 0; }
                set { TargetPrice = value; }
            }

            public decimal OptimizedStopLoss
            {
                get { return StopLossPrice ?? 0; }
                set { StopLossPrice = value; }
            }

            // 투자 정보
            public double ExpectedReturn { get; set; }
            public string RiskLevel { get; set; }
        }

        /// <summary>
        /// 🆕 업그레이드된 종합 기술적 분석 수행 (일봉 + 분봉)
        /// </summary>
        public static async Task<TechnicalAnalysisResult> AnalyzeWithMinuteDataAsync(Stock stock, StockTrader3_WinForms.DatabaseManager databaseManager)
        {
            if (stock?.DailyPrices == null || stock.DailyPrices.Count < 30)
            {
                return new TechnicalAnalysisResult
                {
                    TotalScore = 0,
                    TrendStatus = "데이터부족",
                    MomentumStatus = "데이터부족",
                    PatternStatus = "데이터부족",
                    MinuteAnalysis = "분봉 데이터 부족",
                    RiskWarning = "분석을 위한 충분한 데이터가 없습니다.",
                    AnalysisType = "데이터부족",
                    Confidence = "Low"
                };
            }

            var result = new TechnicalAnalysisResult();

            // 🆕 1단계: 분봉 데이터 조회
            List<MinutePrice> minuteData = null;
            bool hasMinuteData = false;

            try
            {
                minuteData = await databaseManager.GetMinuteHistoricalDataAsync(stock.Code, 1, 3); // 1분봉 3일치
                hasMinuteData = minuteData != null && minuteData.Count >= 100;
                System.Diagnostics.Debug.WriteLine($"✅ {stock.Code} 분봉 데이터 {minuteData?.Count}개 조회 완료");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ {stock.Code} 분봉 데이터 조회 실패: {ex.Message}");
                minuteData = new List<MinutePrice>();
            }

            // 2단계: 일봉 기반 분석 (75점 또는 60점)
            double dailyMaxScore = hasMinuteData ? 60 : 75; // 분봉 있으면 60점, 없으면 75점으로 보정

            result.TrendScore = CalculateEnhancedTrendScore(stock.DailyPrices, dailyMaxScore * 25.0 / 60.0);
            result.TrendStatus = GetTrendStatus(stock.DailyPrices);

            result.MomentumScore = CalculateEnhancedMomentumScore(stock.DailyPrices, dailyMaxScore * 25.0 / 60.0);
            result.MomentumStatus = GetMomentumStatus(stock.DailyPrices);

            result.PatternScore = CalculateEnhancedPatternScore(stock.DailyPrices, dailyMaxScore * 15.0 / 60.0);
            result.PatternStatus = GetPatternStatus(stock.DailyPrices);

            result.SupportResistanceScore = CalculateEnhancedSupportResistanceScore(stock.DailyPrices, dailyMaxScore * 10.0 / 60.0);
            result.SupplyDemandScore = CalculateEnhancedSupplyDemandScore(stock, dailyMaxScore * 5.0 / 60.0);

            // 🆕 3단계: 분봉 기반 분석 (20점 추가)
            if (hasMinuteData)
            {
                result.MinuteTrendScore = CalculateMinuteTrendScore(minuteData, stock.DailyPrices);
                result.MinuteMomentumScore = CalculateMinuteMomentumScore(minuteData);
                result.MinuteTimingScore = CalculateMinuteTimingScore(minuteData, stock.DailyPrices);
                result.MinuteAnalysis = GetMinuteAnalysis(minuteData);
                result.IsMultiTimeFrameAligned = CheckMultiTimeFrameAlignment(stock.DailyPrices, minuteData);
                result.TimeFrameAnalysis = GetTimeFrameAnalysis(stock.DailyPrices, minuteData);
                result.UsedMinuteData = true;
                result.AnalysisType = "정밀완료(분봉+일봉)";
                result.Confidence = "High";
            }
            else
            {
                result.MinuteAnalysis = "분봉 데이터 없음 - 일봉 보정 적용";
                result.IsMultiTimeFrameAligned = false;
                result.TimeFrameAnalysis = "분봉 데이터 부족으로 시간대별 분석 불가";
                result.UsedMinuteData = false;
                result.AnalysisType = "기본완료(일봉)";
                result.Confidence = "Medium";
            }

            // 4단계: 총점 계산 (80점 만점)
            result.TotalScore = result.TrendScore + result.MomentumScore +
                               result.PatternScore + result.SupportResistanceScore +
                               result.SupplyDemandScore + result.MinuteTrendScore +
                               result.MinuteMomentumScore + result.MinuteTimingScore;

            // 🆕 5단계: 정밀한 매매 계획 계산 (분봉 + 일봉 ATR 결합)
            var tradePlan = await CalculatePreciseTradePlanAsync(stock.DailyPrices, minuteData);
            result.BuyPrice = tradePlan.buyPrice;
            result.TargetPrice = tradePlan.targetPrice;
            result.StopLossPrice = tradePlan.stopLoss;
            result.DailyATR = tradePlan.dailyATR;
            result.MinuteATR = tradePlan.minuteATR;
            result.TradingStrategy = tradePlan.strategy;

            // 6단계: MainForm 호환 정보 설정
            SetMainFormCompatibleData(result, stock.DailyPrices, minuteData);

            // 7단계: 기술적 등급 산정
            result.TechnicalGrade = CalculateTechnicalGrade(result.TotalScore);

            // 8단계: 강화된 위험 경고 체크
            result.RiskWarning = CheckEnhancedRiskWarnings(stock.DailyPrices, minuteData, result.TotalScore);

            return result;
        }

        /// <summary>
        /// 🆕 기존 호환용 - 일봉만 사용하는 분석 (80점 만점으로 보정)
        /// </summary>
        public static TechnicalAnalysisResult Analyze(Stock stock)
        {
            if (stock?.DailyPrices == null || stock.DailyPrices.Count < 30)
            {
                return new TechnicalAnalysisResult
                {
                    TotalScore = 0,
                    TrendStatus = "데이터부족",
                    RiskWarning = "분석을 위한 충분한 데이터가 없습니다.",
                    AnalysisType = "데이터부족",
                    Confidence = "Low"
                };
            }

            var result = new TechnicalAnalysisResult
            {
                // 분봉 없을 때 일봉 점수를 80점 기준으로 보정
                TrendScore = CalculateEnhancedTrendScore(stock.DailyPrices, 25),
                MomentumScore = CalculateEnhancedMomentumScore(stock.DailyPrices, 25),
                PatternScore = CalculateEnhancedPatternScore(stock.DailyPrices, 15),
                SupportResistanceScore = CalculateEnhancedSupportResistanceScore(stock.DailyPrices, 10),
                SupplyDemandScore = CalculateEnhancedSupplyDemandScore(stock, 5),
                TrendStatus = GetTrendStatus(stock.DailyPrices),
                MomentumStatus = GetMomentumStatus(stock.DailyPrices),
                PatternStatus = GetPatternStatus(stock.DailyPrices),
                MinuteAnalysis = "일봉 전용 분석 - 분봉 미사용",
                UsedMinuteData = false,
                AnalysisType = "기본완료(일봉)",
                Confidence = "Medium"
            };

            result.TotalScore = result.TrendScore + result.MomentumScore +
                               result.PatternScore + result.SupportResistanceScore +
                               result.SupplyDemandScore;

            // 기본 매매 계획
            var tradePlan = ATR.CalculateTradePlan(stock.DailyPrices);
            result.BuyPrice = tradePlan.buyPrice;
            result.TargetPrice = tradePlan.targetPrice;
            result.StopLossPrice = tradePlan.stopLoss;
            result.TradingStrategy = "일봉 ATR 기반";

            // MainForm 호환 정보 설정
            SetMainFormCompatibleData(result, stock.DailyPrices, null);

            // 기술적 등급 산정
            result.TechnicalGrade = CalculateTechnicalGrade(result.TotalScore);

            result.RiskWarning = CheckEnhancedRiskWarnings(stock.DailyPrices, null, result.TotalScore);

            return result;
        }

        #region 🆕 강화된 일봉 분석 메서드들

        /// <summary>
        /// 강화된 추세 분석 점수 계산 (25점 만점)
        /// </summary>
        private static double CalculateEnhancedTrendScore(List<DailyPrice> dailyPrices, double maxScore = 25)
        {
            double score = 0;
            double ratio = maxScore / 25.0;

            // 1. 이동평균선 정배열 (12점 - 대폭 상향)
            var ma5 = MovingAverage.GetCurrentSMA(dailyPrices, 5);
            var ma10 = MovingAverage.GetCurrentSMA(dailyPrices, 10);
            var ma20 = MovingAverage.GetCurrentSMA(dailyPrices, 20);

            if (ma5.HasValue && ma10.HasValue && ma20.HasValue)
            {
                if (ma5.Value > ma10.Value && ma10.Value > ma20.Value)
                    score += 12 * ratio; // 완전 정배열 (강세의 기본 조건)
                else if (ma5.Value > ma10.Value)
                    score += 8 * ratio;  // 부분 정배열
                else if (ma5.Value > ma20.Value)
                    score += 4 * ratio;  // 최소 조건
                else
                    score += 1 * ratio;
            }

            // 2. 강화된 MACD 분석 (8점)
            score += CalculateEnhancedMACDScore(dailyPrices) * ratio;

            // 3. 🎯 눌림목 매수 보너스 (5점 - 신설)
            score += CalculateNullimokBonus(dailyPrices) * ratio;

            return Math.Round(score, 1);
        }

        /// <summary>
        /// 강화된 모멘텀 분석 점수 계산 (25점 만점)
        /// </summary>
        private static double CalculateEnhancedMomentumScore(List<DailyPrice> dailyPrices, double maxScore = 25)
        {
            double score = 0;
            double ratio = maxScore / 25.0;

            // 1. RSI 상승 반전 (10점 - 이미 수정됨)
            score += RSI.CalculateScore(dailyPrices) * ratio;

            // 2. 강화된 거래량 분석 (10점 - 대폭 상향)
            score += CalculateEnhancedVolumeScore(dailyPrices) * ratio;

            // 3. 스토캐스틱 대체 - 볼린저밴드 모멘텀 (5점)
            score += CalculateBollingerMomentumScore(dailyPrices) * ratio;

            return Math.Round(score, 1);
        }

        /// <summary>
        /// 강화된 패턴 분석 점수 계산 (15점 만점)
        /// </summary>
        private static double CalculateEnhancedPatternScore(List<DailyPrice> dailyPrices, double maxScore = 15)
        {
            double score = 0;
            double ratio = maxScore / 15.0;

            // 1. 강한 상승 반전 캔들패턴 (6점)
            score += AnalyzeEnhancedCandlePattern(dailyPrices) * ratio;

            // 2. 🆕 연속 상승 패턴 (5점 - 신설)
            score += AnalyzeContinuousRisePattern(dailyPrices) * ratio;

            // 3. 갭 상승 패턴 (4점)
            score += AnalyzeGapPattern(dailyPrices) * ratio;

            return Math.Round(score, 1);
        }

        /// <summary>
        /// 강화된 지지/저항 분석 점수 계산 (10점 만점)
        /// </summary>
        private static double CalculateEnhancedSupportResistanceScore(List<DailyPrice> dailyPrices, double maxScore = 10)
        {
            double score = 0;
            double ratio = maxScore / 10.0;

            // 1. 강화된 볼린저밴드 분석 (6점)
            score += CalculateEnhancedBollingerScore(dailyPrices) * ratio;

            // 2. 피보나치 되돌림 (4점)
            score += CalculateFibonacciScore(dailyPrices) * ratio;

            return Math.Round(score, 1);
        }

        /// <summary>
        /// 강화된 수급 분석 점수 계산 (5점 만점)
        /// </summary>
        private static double CalculateEnhancedSupplyDemandScore(Stock stock, double maxScore = 5)
        {
            double score = 2.5; // 기본 점수
            double ratio = maxScore / 5.0;

            if (stock.DailyPrices.Count >= 3)
            {
                var recent3Days = stock.DailyPrices.OrderByDescending(x => x.Date).Take(3).ToList();
                var avgVolume = recent3Days.Skip(1).Average(x => (double)x.Volume);
                var todayVolume = (double)recent3Days[0].Volume;

                // 강화된 거래량 기준
                if (todayVolume > avgVolume * 2)
                    score += 2.5 * ratio; // 대폭 증가
                else if (todayVolume > avgVolume * 1.5)
                    score += 1.5 * ratio; // 상당한 증가
                else if (todayVolume > avgVolume * 1.2)
                    score += 0.5 * ratio; // 약간 증가
            }

            return Math.Round(score * ratio, 1);
        }

        #endregion

        #region 🆕 새로운 분석 메서드들

        /// <summary>
        /// 강화된 MACD 점수 계산 (8점 만점)
        /// </summary>
        private static double CalculateEnhancedMACDScore(List<DailyPrice> dailyPrices)
        {
            var macdResult = MACD.Calculate(dailyPrices);
            if (macdResult == null) return 0;

            // 실제 골든크로스 여부 확인 (이전 데이터와 비교)
            bool isRealGoldenCross = false;
            if (dailyPrices.Count >= 2)
            {
                var prevData = dailyPrices.Take(dailyPrices.Count - 1).ToList();
                var prevMacd = MACD.Calculate(prevData);

                if (prevMacd != null)
                {
                    // 현재: MACD > Signal, 이전: MACD < Signal = 골든크로스
                    isRealGoldenCross = (macdResult.MACD > macdResult.Signal) &&
                                       (prevMacd.MACD <= prevMacd.Signal);
                }
            }

            if (isRealGoldenCross)
                return 8; // 실제 골든크로스
            else if (macdResult.MACD > 0 && macdResult.MACD > macdResult.Signal)
                return 6; // MACD > 0 + 상승 추세
            else if (macdResult.MACD > 0)
                return 4; // MACD > 0 + 횡보
            else if (macdResult.MACD > macdResult.Signal)
                return 3; // MACD < 0이지만 상승 전환
            else
                return 1; // 하락 추세
        }

        /// <summary>
        /// 눌림목 매수 보너스 점수 계산 (5점 만점)
        /// </summary>
        private static double CalculateNullimokBonus(List<DailyPrice> dailyPrices)
        {
            if (dailyPrices.Count < 10) return 0;

            double bonus = 0;
            var recent = dailyPrices.OrderByDescending(x => x.Date).Take(10).ToList();
            var currentPrice = recent[0].Close;

            // 7일선 계산
            var ma7 = recent.Take(7).Average(x => x.Close);

            // 1. 7일선 근접도 (3점) - 핵심 조건
            var distanceFrom7MA = Math.Abs(currentPrice - ma7) / ma7;
            if (distanceFrom7MA <= 0.02m) // 2% 이내
                bonus += 3;
            else if (distanceFrom7MA <= 0.05m) // 5% 이내
                bonus += 1.5;

            // 2. 최근 조정 후 반등 신호 (2점)
            var recent3Days = recent.Take(3).ToList();
            bool hasRecentCorrection = false;
            bool hasReboundSignal = false;

            // 최근 2일 중 조정이 있었는지 확인
            for (int i = 1; i < 3; i++)
            {
                if (recent3Days[i].Close < recent3Days[i - 1].Close)
                {
                    hasRecentCorrection = true;
                    break;
                }
            }

            // 오늘 반등 신호
            if (recent.Count >= 2)
            {
                var today = recent[0];
                var yesterday = recent[1];

                if (today.Close > yesterday.Close && today.Volume > yesterday.Volume)
                    hasReboundSignal = true;
            }

            if (hasRecentCorrection && hasReboundSignal)
                bonus += 2;
            else if (hasReboundSignal)
                bonus += 1;

            return bonus;
        }

        /// <summary>
        /// 강화된 거래량 점수 계산 (10점 만점)
        /// </summary>
        private static double CalculateEnhancedVolumeScore(List<DailyPrice> dailyPrices)
        {
            if (dailyPrices.Count < 5) return 0;

            var recentVolumes = dailyPrices.OrderByDescending(x => x.Date).Take(3).Select(x => (double)x.Volume).ToList();
            var avgVolume = dailyPrices.OrderByDescending(x => x.Date).Skip(3).Take(10).Select(x => (double)x.Volume).Average();
            var todayVolume = recentVolumes[0];

            // 강세 종목 특화 - 거래량 기준 완화 및 점수 상향
            if (todayVolume > avgVolume * 3) return 10;   // 폭증
            else if (todayVolume > avgVolume * 2.5) return 8;  // 대폭 증가
            else if (todayVolume > avgVolume * 2) return 6;    // 상당한 증가
            else if (todayVolume > avgVolume * 1.5) return 4;  // 증가
            else if (todayVolume > avgVolume * 1.2) return 2;  // 약간 증가
            else if (todayVolume > avgVolume) return 1;        // 평균 이상
            else return 0;
        }

        /// <summary>
        /// 볼린저밴드 모멘텀 점수 (5점 만점)
        /// </summary>
        private static double CalculateBollingerMomentumScore(List<DailyPrice> dailyPrices)
        {
            var bbResult = BollingerBands.Calculate(dailyPrices);
            if (bbResult == null) return 0;

            var currentPrice = dailyPrices.OrderByDescending(x => x.Date).First().Close;

            // 중간선(20일선) 돌파 상황
            if (currentPrice > bbResult.Middle * 1.02m) // 2% 이상 돌파
                return 5;
            else if (currentPrice > bbResult.Middle)
                return 3;
            else if (bbResult.Position >= 0.3m && bbResult.Position <= 0.7m)
                return 2; // 중간 구간
            else
                return 1;
        }

        /// <summary>
        /// 강화된 볼린저밴드 점수 (6점 만점)
        /// </summary>
        private static double CalculateEnhancedBollingerScore(List<DailyPrice> dailyPrices)
        {
            var bbResult = BollingerBands.Calculate(dailyPrices);
            if (bbResult == null) return 0;

            var currentPrice = dailyPrices.OrderByDescending(x => x.Date).First().Close;

            // 밴드 폭 확장 확인
            bool isBandExpanding = false;
            if (dailyPrices.Count >= 25)
            {
                var prevBB = BollingerBands.Calculate(dailyPrices.Take(dailyPrices.Count - 1).ToList());
                if (prevBB != null)
                    isBandExpanding = bbResult.Width > prevBB.Width;
            }

            // 새로운 점수 체계
            if (isBandExpanding && currentPrice > bbResult.Middle)
                return 6; // 밴드 확장 + 중간선 돌파
            else if (bbResult.Position <= 0.3m && currentPrice > bbResult.Lower * 1.01m)
                return 5; // 하단 터치 후 반등
            else if (currentPrice > bbResult.Middle && bbResult.Position <= 0.8m)
                return 4; // 중간선 위 + 상단 여유
            else if (bbResult.Position <= 0.3m)
                return 3; // 하단 근접
            else if (bbResult.Position >= 0.8m)
                return 1; // 상단 근접
            else
                return 2; // 중간 구간
        }

        /// <summary>
        /// 강화된 캔들 패턴 분석 (6점 만점)
        /// </summary>
        private static double AnalyzeEnhancedCandlePattern(List<DailyPrice> dailyPrices)
        {
            if (dailyPrices.Count < 3) return 0;

            var recent3 = dailyPrices.OrderByDescending(x => x.Date).Take(3).ToList();
            var today = recent3[0];
            var yesterday = recent3[1];
            var dayBefore = recent3[2];

            double score = 0;

            // 1. 강한 양봉 (3점)
            if (today.Close > today.Open)
            {
                var bodySize = today.Close - today.Open;
                var totalRange = today.High - today.Low;

                if (totalRange > 0)
                {
                    var bodyRatio = bodySize / totalRange;
                    if (bodyRatio > 0.7m)
                        score += 3; // 강한 양봉
                    else if (bodyRatio > 0.5m)
                        score += 2; // 양봉
                    else
                        score += 1; // 약한 양봉
                }
            }

            // 2. 상승 연속성 (3점)
            if (today.Close > yesterday.Close && yesterday.Close >= dayBefore.Close)
                score += 3; // 2일 연속 상승
            else if (today.Close > yesterday.Close)
                score += 1.5; // 당일 상승

            return Math.Min(score, 6);
        }

        /// <summary>
        /// 연속 상승 패턴 분석 (5점 만점)
        /// </summary>
        private static double AnalyzeContinuousRisePattern(List<DailyPrice> dailyPrices)
        {
            if (dailyPrices.Count < 5) return 0;

            var recent5 = dailyPrices.OrderByDescending(x => x.Date).Take(5).ToList();
            int consecutiveRiseDays = 0;

            // 연속 상승일 계산
            for (int i = 0; i < recent5.Count - 1; i++)
            {
                if (recent5[i].Close > recent5[i + 1].Close)
                    consecutiveRiseDays++;
                else
                    break;
            }

            // 거래량 증가 동반 여부
            bool hasVolumeSupport = false;
            if (recent5.Count >= 2)
            {
                var todayVolume = recent5[0].Volume;
                var avgPrevVolume = recent5.Skip(1).Take(3).Average(x => (double)x.Volume);
                hasVolumeSupport = todayVolume > avgPrevVolume * 1.2;
            }

            if (consecutiveRiseDays >= 3 && hasVolumeSupport)
                return 5; // 3일+ 연속 상승 + 거래량 증가
            else if (consecutiveRiseDays >= 3)
                return 4; // 3일+ 연속 상승
            else if (consecutiveRiseDays >= 2 && hasVolumeSupport)
                return 3; // 2일 연속 상승 + 거래량 증가
            else if (consecutiveRiseDays >= 2)
                return 2; // 2일 연속 상승
            else if (consecutiveRiseDays >= 1)
                return 1; // 1일 상승
            else
                return 0;
        }

        #endregion

        #region 🆕 분봉 분석 메서드들 (기존과 동일하게 유지)

        /// <summary>
        /// 분봉 추세 점수 계산 (8점 만점)
        /// </summary>
        private static double CalculateMinuteTrendScore(List<MinutePrice> minuteData, List<DailyPrice> dailyData)
        {
            if (minuteData.Count < 100) return 0;

            double score = 0;

            // 1. 단기 추세 일치 (4점)
            var minuteTrend = CalculateMinuteTrend(minuteData.Take(60).ToList());
            var dailyTrend = CalculateRecentTrend(dailyData);

            if ((minuteTrend > 0 && dailyTrend > 0) || (minuteTrend < 0 && dailyTrend < 0))
                score += 4;
            else if (Math.Abs(minuteTrend) < 1 && Math.Abs(dailyTrend) < 1)
                score += 2;

            // 2. 분봉 이동평균 정배열 (4점)
            var minuteMA5 = CalculateMinuteMA(minuteData, 5);
            var minuteMA15 = CalculateMinuteMA(minuteData, 15);
            var minuteMA30 = CalculateMinuteMA(minuteData, 30);

            if (minuteMA5 > minuteMA15 && minuteMA15 > minuteMA30)
                score += 4;
            else if (minuteMA5 > minuteMA15)
                score += 2;

            return Math.Round(score, 1);
        }

        /// <summary>
        /// 분봉 모멘텀 점수 계산 (7점 만점)
        /// </summary>
        private static double CalculateMinuteMomentumScore(List<MinutePrice> minuteData)
        {
            if (minuteData.Count < 50) return 0;

            double score = 0;

            // 1. 분봉 RSI 분석 (4점)
            var minuteRSI = CalculateMinuteRSI(minuteData, 14);
            if (minuteRSI >= 30 && minuteRSI <= 60)
                score += 4;
            else if (minuteRSI >= 60 && minuteRSI <= 70)
                score += 2;

            // 2. 분봉 거래량 급증 (3점)
            var recentVolume = minuteData.Take(10).Sum(x => x.Volume);
            var avgVolume = minuteData.Skip(10).Take(50).Sum(x => x.Volume) / 5;

            if (recentVolume > avgVolume * 2)
                score += 3;
            else if (recentVolume > avgVolume * 1.5)
                score += 1.5;

            return Math.Round(score, 1);
        }

        /// <summary>
        /// 분봉 진입 타이밍 점수 계산 (5점 만점)
        /// </summary>
        private static double CalculateMinuteTimingScore(List<MinutePrice> minuteData, List<DailyPrice> dailyData)
        {
            if (minuteData.Count < 30) return 0;

            double score = 0;

            // 1. 최근 분봉 상승 패턴 (3점)
            var recent5Minutes = minuteData.Take(5).ToList();
            var upCount = 0;
            for (int i = 0; i < recent5Minutes.Count - 1; i++)
            {
                if (recent5Minutes[i].Close > recent5Minutes[i + 1].Close)
                    upCount++;
            }

            if (upCount >= 3) score += 3;
            else if (upCount >= 2) score += 1.5;

            // 2. 분봉 지지선 반등 (2점)
            var currentPrice = minuteData.First().Close;
            var dailyLow = dailyData.First().Low;
            var priceFromLow = (currentPrice - dailyLow) / dailyLow;

            if (priceFromLow <= 0.02m && priceFromLow > 0)
                score += 2;
            else if (priceFromLow <= 0.05m && priceFromLow > 0)
                score += 1;

            return Math.Round(score, 1);
        }

        /// <summary>
        /// 다중 시간대 정렬 확인
        /// </summary>
        private static bool CheckMultiTimeFrameAlignment(List<DailyPrice> dailyData, List<MinutePrice> minuteData)
        {
            if (minuteData == null || minuteData.Count < 100) return false;

            var dailyTrend = CalculateRecentTrend(dailyData);
            var minuteTrend = CalculateMinuteTrend(minuteData.Take(60).ToList());

            return (dailyTrend > 0 && minuteTrend > 0) || (dailyTrend < 0 && minuteTrend < 0);
        }

        /// <summary>
        /// 🆕 사전 예측 매매가 계산 (내일 7일선 기반)
        /// </summary>
        /// <param name="stock">종목 정보</param>
        /// <param name="dailyPrices">일봉 데이터</param>
        /// <param name="technicalGrade">기술적 등급</param>
        /// <returns>사전 설정 매매가</returns>
        private static (decimal buyPrice, decimal sellPrice, decimal stopLoss)
            CalculatePresetTradePlan(Stock stock, List<DailyPrice> dailyPrices, string technicalGrade)
        {
            // 1. 내일 7일선 예측
            var tomorrow7MA = PredictTomorrow7DayMA(dailyPrices);
            if (tomorrow7MA <= 0)
                return (0, 0, 0);

            // 2. 매수가 = 내일 7일선 + 0.2% (7일선 살짝 위에서 매수)
            var buyPrice = tomorrow7MA * 1.002m;

            // 3. 목표가 = ATR.cs의 등급별 고정 수익률 사용
            var targetRate = ATR.GetTargetRate(technicalGrade);
            var sellPrice = buyPrice * (1 + targetRate);

            // 4. 손절가 = ATR.cs의 최적 손절폭 사용
            var stopLossRate = ATR.CalculateOptimalStopLossRate(dailyPrices);
            var stopLoss = buyPrice * (1 - stopLossRate);

            return (buyPrice, sellPrice, stopLoss);
        }



        /// <summary>
        /// 정밀한 매매 계획 계산 (분봉 + 일봉 ATR 결합)
        /// </summary>
        /// 
        /// <summary>
        /// 🔄 수정된 정밀 매매 계획 (1.5-2% 목표로 변경)
        /// </summary>
        private static async Task<(decimal? buyPrice, decimal? targetPrice, decimal? stopLoss,
            decimal? dailyATR, decimal? minuteATR, string strategy)>
            CalculatePreciseTradePlanAsync(List<DailyPrice> dailyData, List<MinutePrice> minuteData,
            string technicalGrade = "B")
        {
            var dailyATR = ATR.Calculate(dailyData, 14);
            var currentPrice = dailyData.OrderByDescending(x => x.Date).First().Close;

            decimal? minuteATR = null;
            string strategy = "일봉 기반 1.5-2% 목표";

            if (minuteData != null && minuteData.Count >= 100)
            {
                minuteATR = CalculateMinuteATR(minuteData, 14);
                strategy = "분봉+일봉 기반 1.5-2% 목표";
            }

            // 🆕 ATR.cs의 새로운 메서드들 활용
            var atr3 = ATR.Calculate3DayATR(dailyData);
            if (atr3.HasValue)
            {
                // 매수가: 현재가 기준 (실제로는 내일 7일선 근처)
                var buyPrice = currentPrice;

                // 🔄 핵심 변경: ATR 기반 → 고정 비율 기반
                var targetRate = ATR.GetTargetRate(technicalGrade);
                var targetPrice = buyPrice * (1 + targetRate);

                // 손절가: ATR 기반 최적화 + 1.5% 제한
                var stopLossRate = ATR.CalculateOptimalStopLossRate(dailyData);
                var stopLoss = buyPrice * (1 - stopLossRate);

                return (buyPrice, targetPrice, stopLoss, dailyATR, minuteATR, strategy);
            }

            return (null, null, null, null, null, "ATR 계산 불가");
        }
             
      

      
        #endregion

        #region 🆕 분봉 계산 유틸리티 (기존과 동일)

        private static double CalculateMinuteTrend(List<MinutePrice> minuteData)
        {
            if (minuteData.Count < 2) return 0;

            var firstPrice = minuteData.Last().Close;
            var lastPrice = minuteData.First().Close;

            if (firstPrice == 0) return 0;

            return (double)((lastPrice - firstPrice) / firstPrice) * 100;
        }

        private static decimal CalculateMinuteMA(List<MinutePrice> minuteData, int period)
        {
            if (minuteData.Count < period) return 0;
            return minuteData.Take(period).Average(x => x.Close);
        }

        private static double CalculateMinuteRSI(List<MinutePrice> minuteData, int period = 14)
        {
            if (minuteData.Count < period + 1) return 50;

            var gains = new List<decimal>();
            var losses = new List<decimal>();

            for (int i = 0; i < period; i++)
            {
                var change = minuteData[i].Close - minuteData[i + 1].Close;
                gains.Add(change > 0 ? change : 0);
                losses.Add(change < 0 ? Math.Abs(change) : 0);
            }

            var avgGain = gains.Average();
            var avgLoss = losses.Average();

            if (avgLoss == 0) return 100;

            var rs = avgGain / avgLoss;
            return (double)(100 - (100 / (1 + rs)));
        }

        private static decimal? CalculateMinuteATR(List<MinutePrice> minuteData, int period = 14)
        {
            if (minuteData.Count < period + 1) return null;

            var trueRanges = new List<decimal>();

            for (int i = 0; i < period; i++)
            {
                var current = minuteData[i];
                var previous = minuteData[i + 1];

                var tr1 = current.High - current.Low;
                var tr2 = Math.Abs(current.High - previous.Close);
                var tr3 = Math.Abs(current.Low - previous.Close);

                trueRanges.Add(Math.Max(tr1, Math.Max(tr2, tr3)));
            }

            return trueRanges.Average();
        }

        private static string GetMinuteAnalysis(List<MinutePrice> minuteData)
        {
            if (minuteData == null || minuteData.Count < 30)
                return "분봉 데이터 부족";

            var trend = CalculateMinuteTrend(minuteData.Take(30).ToList());
            var rsi = CalculateMinuteRSI(minuteData);

            if (trend > 2 && rsi < 70)
                return "분봉 상승추세, 매수 타이밍 양호";
            else if (trend > 0 && rsi < 60)
                return "분봉 약한 상승, 진입 가능";
            else if (Math.Abs(trend) < 1)
                return "분봉 횡보, 관망";
            else if (trend < -2)
                return "분봉 하락추세, 매수 대기";
            else
                return "분봉 혼조, 신중 진입";
        }

        private static string GetTimeFrameAnalysis(List<DailyPrice> dailyData, List<MinutePrice> minuteData)
        {
            var dailyTrend = CalculateRecentTrend(dailyData);
            var minuteTrend = minuteData != null && minuteData.Count > 30 ?
                CalculateMinuteTrend(minuteData.Take(30).ToList()) : 0;

            return $"일봉추세:{(dailyTrend > 0 ? "상승" : dailyTrend < 0 ? "하락" : "횡보")}, " +
                   $"분봉추세:{(minuteTrend > 0 ? "상승" : minuteTrend < 0 ? "하락" : "횡보")}, " +
                   $"정렬도:{(CheckMultiTimeFrameAlignment(dailyData, minuteData) ? "일치" : "불일치")}";
        }

        #endregion

        #region 🆕 MainForm 호환성 및 유틸리티

        /// <summary>
        /// MainForm.cs 호환 데이터 설정
        /// </summary>
        private static void SetMainFormCompatibleData(TechnicalAnalysisResult result, List<DailyPrice> dailyPrices, List<MinutePrice> minuteData)
        {
            // RSI 설정
            var rsi = RSI.Calculate(dailyPrices);
            result.RSI = rsi ?? 0;

            // MACD 설정
            var macdResult = MACD.Calculate(dailyPrices);
            if (macdResult != null)
            {
                result.MACD = (double)macdResult.MACD;
                result.MACDSignal = (double)macdResult.Signal;
            }

            // 볼린저밴드 설정
            var bbResult = BollingerBands.Calculate(dailyPrices);
            if (bbResult != null)
            {
                result.BollingerUpper = bbResult.Upper;
                result.BollingerLower = bbResult.Lower;
                result.BollingerPosition = (double)bbResult.Position;
            }

            // 이동평균선 설정
            result.MA5 = MovingAverage.GetCurrentSMA(dailyPrices, 5) ?? 0;
            result.MA20 = MovingAverage.GetCurrentSMA(dailyPrices, 20) ?? 0;
            result.MA60 = MovingAverage.GetCurrentSMA(dailyPrices, 60) ?? 0;
        }

        /// <summary>
        /// 기술적 등급 산정
        /// </summary>
        private static string CalculateTechnicalGrade(double totalScore)
        {
            if (totalScore >= 70) return "S";
            else if (totalScore >= 60) return "A";
            else if (totalScore >= 50) return "B";
            else if (totalScore >= 40) return "C";
            else return "D";
        }

        /// <summary>
        /// 강화된 위험 경고 체크
        /// </summary>
        private static string CheckEnhancedRiskWarnings(List<DailyPrice> dailyPrices, List<MinutePrice> minuteData, double totalScore)
        {
            var warnings = new List<string>();

            // 기존 일봉 기반 위험 체크
            var rsi = RSI.Calculate(dailyPrices);
            if (rsi.HasValue && rsi.Value > 80)
                warnings.Add("RSI과매수");

            var recentTrend = CalculateRecentTrend(dailyPrices);
            if (recentTrend > 15)
                warnings.Add("급등후조정위험");

            // 🆕 새로운 위험 신호들
            if (recentTrend > 20)
                warnings.Add("과도한급등");

            // 거래량 없는 상승 감지
            if (dailyPrices.Count >= 3)
            {
                var recent3 = dailyPrices.OrderByDescending(x => x.Date).Take(3).ToList();
                var priceRise = (recent3[0].Close - recent3[2].Close) / recent3[2].Close * 100;
                var avgVolume = recent3.Average(x => (double)x.Volume);
                var prevAvgVolume = dailyPrices.OrderByDescending(x => x.Date).Skip(3).Take(5).Average(x => (double)x.Volume);

                if (priceRise > 5 && avgVolume < prevAvgVolume * 0.8)
                    warnings.Add("거래량부족상승");
            }

            // 분봉 기반 위험 체크
            if (minuteData != null && minuteData.Count > 50)
            {
                var minuteRSI = CalculateMinuteRSI(minuteData);
                if (minuteRSI > 80)
                    warnings.Add("분봉RSI과매수");

                var minuteTrend = CalculateMinuteTrend(minuteData.Take(30).ToList());
                if (minuteTrend > 10)
                    warnings.Add("분봉급등위험");
            }

            // 점수와 실제 지표 불일치 경고
            if (totalScore < 40 && recentTrend > 5)
                warnings.Add("기술적약세주의");

            return warnings.Count > 0 ? string.Join(", ", warnings) : "없음";
        }

        #endregion

        #region 기존 유틸리티 메서드들

        private static double CalculateRecentTrend(List<DailyPrice> dailyPrices)
        {
            if (dailyPrices.Count < 5) return 0;

            var recent5Days = dailyPrices.OrderByDescending(x => x.Date).Take(5).ToList();
            var firstPrice = recent5Days.Last().Close;
            var lastPrice = recent5Days.First().Close;

            if (firstPrice == 0) return 0;

            return (double)((lastPrice - firstPrice) / firstPrice) * 100;
        }

        /// <summary>
        /// 🆕 내일 7일선 위치 예측 (핵심 혁신 기능)
        /// </summary>
        /// <param name="dailyPrices">일봉 데이터</param>
        /// <returns>내일 예상 7일선 가격</returns>

        /// <summary>
        /// 🆕 내일 7일선 위치 예측 (핵심 혁신 기능)
        /// </summary>
        /// <param name="dailyPrices">일봉 데이터</param>
        /// <returns>내일 예상 7일선 가격</returns>
        private static decimal PredictTomorrow7DayMA(List<DailyPrice> dailyPrices)
        {
            if (dailyPrices.Count < 8) return 0;

            var recent = dailyPrices.OrderByDescending(x => x.Date).Take(8).ToList();

            // 현재 7일선 계산
            var current7MA = recent.Take(7).Average(x => x.Close);

            // 8일전 가격 (내일 제외될 가격)
            var price8DaysAgo = recent[7].Close;

            // 오늘 종가 (내일 추가될 가격으로 가정)
            var todayClose = recent[0].Close;

            // 내일 7일선 = (현재7일선×7 - 8일전가격 + 오늘종가) ÷ 7
            return (current7MA * 7 - price8DaysAgo + todayClose) / 7;
        }



        /// <summary>
        /// 🆕 내일 조정 확률 분석
        /// </summary>
        /// <param name="dailyPrices">일봉 데이터</param>
        /// <returns>조정 확률 (0-100)</returns>
        /// <summary>
        /// 🆕 내일 조정 확률 분석
        /// </summary>
        /// <param name="dailyPrices">일봉 데이터</param>
        /// <returns>조정 확률 (0-100)</returns>
        private static double AnalyzeCorrectionProbability(List<DailyPrice> dailyPrices)
        {
            if (dailyPrices.Count < 5) return 50; // 기본값

            double riskFactor = 0;
            var recent5 = dailyPrices.OrderByDescending(x => x.Date).Take(5).ToList();

            // 1. 연속 상승일 수 (위험 증가)
            int consecutiveUpDays = 0;
            for (int i = 0; i < recent5.Count - 1; i++)
            {
                if (recent5[i].Close > recent5[i + 1].Close)
                    consecutiveUpDays++;
                else break;
            }
            riskFactor += consecutiveUpDays * 15; // 3일 연속 상승시 45% 위험

            // 2. RSI 과매수 (위험 증가)
            var rsi = RSI.Calculate(dailyPrices);
            if (rsi.HasValue)
            {
                if (rsi.Value > 70) riskFactor += 20;
                else if (rsi.Value > 60) riskFactor += 10;
            }

            // 3. 3일 ATR 대비 변동성 (안정성 요소)
            var atr3 = ATR.Calculate3DayATR(dailyPrices);
            var currentPrice = recent5[0].Close;
            if (atr3.HasValue && currentPrice > 0)
            {
                var volatilityRate = (double)(atr3.Value / currentPrice) * 100;
                if (volatilityRate < 3) riskFactor -= 10; // 저변동성 = 안정적
                else if (volatilityRate > 8) riskFactor += 15; // 고변동성 = 위험
            }

            return Math.Max(0, Math.Min(100, riskFactor));
        }


   
        private static double AnalyzeGapPattern(List<DailyPrice> dailyPrices)
        {
            if (dailyPrices.Count < 2) return 0;

            var today = dailyPrices.OrderByDescending(x => x.Date).First();
            var yesterday = dailyPrices.OrderByDescending(x => x.Date).Skip(1).First();

            if (today.Low > yesterday.High) return 4; // 상승 갭
            else if (today.Open > yesterday.Close * 1.02m) return 2; // 갭 상승
            else return 0;
        }

        private static double CalculateFibonacciScore(List<DailyPrice> dailyPrices)
        {
            if (dailyPrices.Count < 10) return 0;

            var recent10 = dailyPrices.OrderByDescending(x => x.Date).Take(10).ToList();
            var high = recent10.Max(x => x.High);
            var low = recent10.Min(x => x.Low);
            var currentPrice = recent10.First().Close;

            if (high == low) return 0;

            var fibonacci50 = low + (high - low) * 0.5m;
            var distanceFromFib = Math.Abs(currentPrice - fibonacci50) / (high - low);

            if (distanceFromFib < 0.05m) return 4;
            else if (distanceFromFib < 0.1m) return 2;
            else return 0;
        }

        private static string GetTrendStatus(List<DailyPrice> dailyPrices)
        {
            var recentTrend = CalculateRecentTrend(dailyPrices);

            if (recentTrend > 5) return "강한상승추세";
            else if (recentTrend > 2) return "상승추세";
            else if (recentTrend > -2) return "횡보";
            else if (recentTrend > -5) return "약한하락추세";
            else return "하락추세";
        }

        private static string GetMomentumStatus(List<DailyPrice> dailyPrices)
        {
            var rsi = RSI.Calculate(dailyPrices);

            if (!rsi.HasValue) return "데이터부족";

            if (rsi.Value >= 70) return "과매수";
            else if (rsi.Value >= 55) return "강세";
            else if (rsi.Value >= 45) return "중립상승";
            else if (rsi.Value >= 30) return "중립";
            else return "과매도";
        }

        private static string GetPatternStatus(List<DailyPrice> dailyPrices)
        {
            var patternScore = CalculateEnhancedPatternScore(dailyPrices, 15);

            if (patternScore >= 12) return "매우강한상승패턴";
            else if (patternScore >= 9) return "강한상승패턴";
            else if (patternScore >= 6) return "상승패턴";
            else if (patternScore >= 3) return "약한상승패턴";
            else return "패턴없음";
        }

        #endregion
    }
}