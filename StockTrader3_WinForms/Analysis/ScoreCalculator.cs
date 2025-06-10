using System;
using System.Collections.Generic;
using System.Linq;
using StockTrader3.Models;

namespace StockTrader3.Analysis
{
    /// <summary>
    /// 종합 점수 계산 및 종목 등급 산정 클래스
    /// 기술적 분석(70점) + 뉴스 분석(60점) = 총 130점 만점
    /// </summary>
    public class ScoreCalculator
    {
        /// <summary>
        /// 종합 분석 결과 클래스
        /// </summary>
        public class ComprehensiveAnalysisResult
        {
            // 점수 정보
            public double TechnicalScore { get; set; }       // 기술적 분석 점수 (0~70점)
            public double NewsScore { get; set; }            // 뉴스 분석 점수 (-30~+30점)
            public double TotalScore { get; set; }           // 총점 (30~100점)
            public string Grade { get; set; }                // 등급 (S, A, B, 관심)
            public int Priority { get; set; }                // 우선순위 (1~50)

            // 상세 분석 결과
            public TechnicalAnalyzer.TechnicalAnalysisResult TechnicalAnalysis { get; set; }
            public NewsAnalyzer.NewsAnalysisResult NewsAnalysis { get; set; }

            // 매매 계획
            public decimal? BuyPrice { get; set; }
            public decimal? TargetPrice { get; set; }
            public decimal? StopLossPrice { get; set; }
            public decimal? ExpectedReturn { get; set; }     // 예상 수익률

            // 종합 판단
            public string OverallStatus { get; set; }        // 종합 상태
            public string MainFactor { get; set; }           // 주요 상승 요인
            public string RiskWarning { get; set; }          // 종합 위험 경고
            public string Recommendation { get; set; }       // 매매 추천

            // 추가 정보
            public bool HasNegativeNews { get; set; }        // 악재 뉴스 여부
            public bool HasFadingRisk { get; set; }          // 호재 소멸 위험
            public double VolatilityLevel { get; set; }      // 변동성 수준
        }

        /// <summary>
        /// 종목의 종합 분석 수행
        /// </summary>
        /// <param name="stock">주식 데이터</param>
        /// <param name="newsItems">관련 뉴스</param>
        /// <param name="analysisDate">분석 날짜</param>
        /// <returns>종합 분석 결과</returns>
        public static ComprehensiveAnalysisResult AnalyzeStock(Stock stock, List<News> newsItems, DateTime analysisDate)
        {
            // 1. 기술적 분석 수행 (70점 만점)
            var technicalResult = TechnicalAnalyzer.Analyze(stock);

            // 2. 뉴스 분석 수행 (60점 범위: -30~+30)
            var newsResult = NewsAnalyzer.Analyze(stock, newsItems, analysisDate);

            // 3. 종합 결과 생성
            var result = new ComprehensiveAnalysisResult
            {
                TechnicalScore = technicalResult.TotalScore,
                NewsScore = newsResult.NewsScore,
                TechnicalAnalysis = technicalResult,
                NewsAnalysis = newsResult
            };

            // 4. 총점 계산 및 보정
            CalculateTotalScore(result);

            // 5. 등급 및 우선순위 결정
            DetermineGradeAndPriority(result);

            // 6. 매매 계획 설정
            SetTradePlan(result);

            // 7. 종합 상태 및 추천 생성
            GenerateOverallAssessment(result);

            return result;
        }

        /// <summary>
        /// 총점 계산 및 보정
        /// </summary>
        private static void CalculateTotalScore(ComprehensiveAnalysisResult result)
        {
            // 기본 총점 = 기술적 분석 + 뉴스 분석
            double rawTotal = result.TechnicalScore + result.NewsScore;

            // 악재 발생 시 추가 패널티
            if (result.NewsAnalysis.NegativeRisk < -20)
            {
                rawTotal -= 15; // 심각한 악재 시 추가 15점 감점
                result.HasNegativeNews = true;
            }
            else if (result.NewsAnalysis.NegativeRisk < -10)
            {
                rawTotal -= 10; // 일반 악재 시 추가 10점 감점
                result.HasNegativeNews = true;
            }

            // 호재 소멸 위험 시 추가 패널티
            if (result.NewsAnalysis.FadingRisk < -10)
            {
                rawTotal -= 10;
                result.HasFadingRisk = true;
            }

            // 기술적 분석과 뉴스 분석 균형 보정
            double balanceBonus = CalculateBalanceBonus(result);
            rawTotal += balanceBonus;

            // 최종 점수를 30~100 범위로 조정
            result.TotalScore = Math.Max(30, Math.Min(100, rawTotal));
        }

        /// <summary>
        /// 기술적 분석과 뉴스 분석의 균형 보너스 계산
        /// </summary>
        private static double CalculateBalanceBonus(ComprehensiveAnalysisResult result)
        {
            // 기술적 분석이 50점 이상이고 뉴스 분석이 15점 이상일 때 보너스
            if (result.TechnicalScore >= 50 && result.NewsScore >= 15)
                return 5; // 둘 다 우수할 때 보너스

            // 기술적 분석이 60점 이상이고 뉴스 분석이 10점 이상일 때
            else if (result.TechnicalScore >= 60 && result.NewsScore >= 10)
                return 3;

            // 한쪽이 매우 우수하고 다른 쪽이 나쁘지 않을 때
            else if ((result.TechnicalScore >= 65 && result.NewsScore >= 0) ||
                     (result.NewsScore >= 20 && result.TechnicalScore >= 40))
                return 2;

            return 0;
        }

        /// <summary>
        /// 등급 및 우선순위 결정
        /// </summary>
        private static void DetermineGradeAndPriority(ComprehensiveAnalysisResult result)
        {
            // 악재가 있으면 등급 하향
            if (result.HasNegativeNews)
            {
                result.Grade = "배제";
                result.Priority = 999;
                return;
            }

            // 점수 기반 등급 결정
            if (result.TotalScore >= 90)
            {
                result.Grade = "S";
                result.Priority = 1;
            }
            else if (result.TotalScore >= 80)
            {
                result.Grade = "A";
                result.Priority = 2;
            }
            else if (result.TotalScore >= 70)
            {
                result.Grade = "B";
                result.Priority = 3;
            }
            else if (result.TotalScore >= 60)
            {
                result.Grade = "관심";
                result.Priority = 4;
            }
            else
            {
                result.Grade = "배제";
                result.Priority = 999;
            }

            // 추가 조건 확인
            ApplyAdditionalGradeRules(result);
        }

        /// <summary>
        /// 추가 등급 규칙 적용
        /// </summary>
        private static void ApplyAdditionalGradeRules(ComprehensiveAnalysisResult result)
        {
            // S급 추가 조건 확인
            if (result.Grade == "S")
            {
                if (result.TechnicalScore < 60 || result.NewsScore < 15)
                {
                    result.Grade = "A"; // S급 조건 미달 시 A급으로 강등
                    result.Priority = 2;
                }
            }

            // A급 추가 조건 확인
            if (result.Grade == "A")
            {
                if (result.TechnicalScore < 50 || result.NewsScore < 10)
                {
                    result.Grade = "B"; // A급 조건 미달 시 B급으로 강등
                    result.Priority = 3;
                }
            }

            // 호재 소멸 위험이 있으면 등급 하향
            if (result.HasFadingRisk && result.Grade != "배제")
            {
                if (result.Grade == "S") result.Grade = "A";
                else if (result.Grade == "A") result.Grade = "B";
                else if (result.Grade == "B") result.Grade = "관심";

                result.Priority += 1;
            }
        }

        /// <summary>
        /// 매매 계획 설정
        /// </summary>
        private static void SetTradePlan(ComprehensiveAnalysisResult result)
        {
            // 기술적 분석에서 계산된 매매 계획 사용
            result.BuyPrice = result.TechnicalAnalysis.BuyPrice;
            result.TargetPrice = result.TechnicalAnalysis.TargetPrice;
            result.StopLossPrice = result.TechnicalAnalysis.StopLossPrice;

            // 예상 수익률 계산
            if (result.BuyPrice.HasValue && result.TargetPrice.HasValue && result.BuyPrice.Value > 0)
            {
                result.ExpectedReturn = ((result.TargetPrice.Value - result.BuyPrice.Value) / result.BuyPrice.Value) * 100;
            }

            // 뉴스 분석 결과에 따라 매매 계획 조정
            AdjustTradePlanByNews(result);
        }

        /// <summary>
        /// 뉴스 분석 결과에 따른 매매 계획 조정
        /// </summary>
        private static void AdjustTradePlanByNews(ComprehensiveAnalysisResult result)
        {
            if (!result.BuyPrice.HasValue || !result.TargetPrice.HasValue || !result.StopLossPrice.HasValue)
                return;

            // 뉴스 점수가 높을 때 목표가 상향 조정
            if (result.NewsScore >= 20)
            {
                result.TargetPrice = result.TargetPrice.Value * 1.1m; // 10% 상향
            }
            else if (result.NewsScore >= 15)
            {
                result.TargetPrice = result.TargetPrice.Value * 1.05m; // 5% 상향
            }

            // 호재 소멸 위험이 있을 때 목표가 하향 조정
            if (result.HasFadingRisk)
            {
                result.TargetPrice = result.TargetPrice.Value * 0.9m; // 10% 하향
                result.StopLossPrice = result.StopLossPrice.Value * 1.1m; // 손절폭 확대
            }

            // 예상 수익률 재계산
            if (result.BuyPrice.Value > 0)
            {
                result.ExpectedReturn = ((result.TargetPrice.Value - result.BuyPrice.Value) / result.BuyPrice.Value) * 100;
            }
        }

        /// <summary>
        /// 종합 평가 생성
        /// </summary>
        private static void GenerateOverallAssessment(ComprehensiveAnalysisResult result)
        {
            // 종합 상태 결정
            result.OverallStatus = DetermineOverallStatus(result);

            // 주요 상승 요인
            result.MainFactor = result.NewsAnalysis.MainFactor ?? "기술적 요인";

            // 종합 위험 경고
            var warnings = new List<string>();
            if (!string.IsNullOrEmpty(result.TechnicalAnalysis.RiskWarning) && result.TechnicalAnalysis.RiskWarning != "없음")
                warnings.Add(result.TechnicalAnalysis.RiskWarning);
            if (!string.IsNullOrEmpty(result.NewsAnalysis.RiskWarning) && result.NewsAnalysis.RiskWarning != "없음")
                warnings.Add(result.NewsAnalysis.RiskWarning);

            result.RiskWarning = warnings.Count > 0 ? string.Join(", ", warnings) : "없음";

            // 매매 추천
            result.Recommendation = GenerateRecommendation(result);
        }

        /// <summary>
        /// 종합 상태 결정
        /// </summary>
        private static string DetermineOverallStatus(ComprehensiveAnalysisResult result)
        {
            if (result.Grade == "배제")
                return "매매부적합";
            else if (result.Grade == "S")
                return "적극매수";
            else if (result.Grade == "A")
                return "매수권장";
            else if (result.Grade == "B")
                return "신중매수";
            else if (result.Grade == "관심")
                return "관심종목";
            else
                return "검토필요";
        }

        /// <summary>
        /// 매매 추천 생성
        /// </summary>
        private static string GenerateRecommendation(ComprehensiveAnalysisResult result)
        {
            if (result.Grade == "배제")
                return "매매 금지";

            var recommendations = new List<string>();

            // 기본 추천
            if (result.Grade == "S")
                recommendations.Add("우선 매수 대상");
            else if (result.Grade == "A")
                recommendations.Add("적극 매수 고려");
            else if (result.Grade == "B")
                recommendations.Add("신중한 매수 검토");
            else
                recommendations.Add("관심 종목으로 모니터링");

            // 추가 권고사항
            if (result.HasFadingRisk)
                recommendations.Add("호재 소멸 주의");

            if (result.ExpectedReturn.HasValue && result.ExpectedReturn.Value > 15)
                recommendations.Add("고수익 기대");

            if (result.TechnicalAnalysis.TrendStatus == "강한상승추세")
                recommendations.Add("추세 추종 전략");

            return string.Join(", ", recommendations);
        }

        /// <summary>
        /// 복수 종목 분석 및 순위 결정
        /// </summary>
        /// <param name="stocks">종목 리스트</param>
        /// <param name="stockNews">종목별 뉴스 딕셔너리</param>
        /// <param name="analysisDate">분석 날짜</param>
        /// <returns>순위별 분석 결과 (상위 50개)</returns>
        public static List<ComprehensiveAnalysisResult> AnalyzeMultipleStocks(
            List<Stock> stocks,
            Dictionary<string, List<News>> stockNews,
            DateTime analysisDate)
        {
            var results = new List<ComprehensiveAnalysisResult>();

            foreach (var stock in stocks)
            {
                var newsItems = stockNews.ContainsKey(stock.Code) ? stockNews[stock.Code] : new List<News>();
                var result = AnalyzeStock(stock, newsItems, analysisDate);

                // 종목 정보 추가
                result.TechnicalAnalysis.BuyPrice = result.BuyPrice;
                result.TechnicalAnalysis.TargetPrice = result.TargetPrice;
                result.TechnicalAnalysis.StopLossPrice = result.StopLossPrice;

                results.Add(result);
            }

            // 점수 기준 정렬 후 우선순위 재할당
            var sortedResults = results
                .Where(r => r.Grade != "배제")
                .OrderByDescending(r => r.TotalScore)
                .ThenByDescending(r => r.NewsScore)
                .ThenByDescending(r => r.TechnicalScore)
                .Take(50)
                .ToList();

            // 우선순위 재할당
            for (int i = 0; i < sortedResults.Count; i++)
            {
                sortedResults[i].Priority = i + 1;
            }

            return sortedResults;
        }
    }
}