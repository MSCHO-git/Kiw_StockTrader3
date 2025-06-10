using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using StockTrader3.Models;

namespace StockTrader3.Analysis
{
    /// <summary>
    /// 뉴스 분석 클래스 (120점 만점: 0~120) - 뉴스 없음 = 중립점수(60점)
    /// 핵심: 뉴스가 없으면 중립, 호재/악재 정도에 따라 상향/하향 조정
    /// </summary>
    public class NewsAnalyzer
    {
        /// <summary>
        /// 뉴스 분석 결과 클래스
        /// </summary>
        public class NewsAnalysisResult
        {
            public double NewsScore { get; set; }           // 뉴스 총점 (0~120) ✅ 개선
            public double FreshnessScore { get; set; }      // 호재 신선도 (0~60점) ✅ 확대
            public double ImpactScore { get; set; }         // 호재 파급력 (0~40점) ✅ 확대
            public double CredibilityScore { get; set; }    // 뉴스 신뢰도 (0~20점) ✅ 확대
            public double SustainabilityScore { get; set; } // 지속성 점수 (0~40점) ✅ 확대
            public double FadingRisk { get; set; }          // 호재 소멸 위험 (-80점) ✅ 확대
            public double NegativeRisk { get; set; }        // 악재 위험 (-120점) ✅ 확대

            // 상세 정보
            public string MainFactor { get; set; }          // 주요 상승 요인
            public string RiskWarning { get; set; }         // 위험 경고
            public string SustainabilityStatus { get; set; } // 지속성 상태
            public List<string> PositiveKeywords { get; set; } // 긍정 키워드
            public List<string> NegativeKeywords { get; set; } // 부정 키워드
            public int NewsCount { get; set; }               // 관련 뉴스 수
        }

        // 긍정적 키워드 사전 (기존 유지 - 최종 점수에서 4배 확대 적용)
        private static readonly Dictionary<string, int> PositiveKeywords = new Dictionary<string, int>
       {
           // 사업 확장 관련 (높은 점수)
           {"신규계약", 15}, {"대형계약", 15}, {"수주", 12}, {"계약체결", 12},
           {"해외진출", 12}, {"사업확장", 10}, {"투자유치", 10}, {"자금조달", 8},
           
           // 기술/특허 관련
           {"기술개발", 10}, {"특허출원", 10}, {"특허등록", 12}, {"신기술", 8},
           {"혁신", 8}, {"AI", 8}, {"인공지능", 8}, {"빅데이터", 6},
           
           // 실적 관련
           {"매출증가", 8}, {"수익개선", 8}, {"흑자전환", 12}, {"실적개선", 8},
           {"목표상향", 10}, {"가이던스상향", 10},
           
           // 파트너십/제휴
           {"제휴", 8}, {"파트너십", 8}, {"협력", 6}, {"MOU", 6}, {"업무협약", 6},
           
           // 정부 정책/규제 완화
           {"규제완화", 10}, {"정부지원", 8}, {"보조금", 6}, {"인센티브", 6},
           {"승인", 8}, {"허가", 8}, {"정책지원", 8},
           
           // 기타 긍정 요인
           {"신제품", 8}, {"출시", 6}, {"론칭", 6}, {"확대", 6}, {"성장", 6}
       };

        // 부정적 키워드 사전 (기존 유지 - 최종 점수에서 4배 확대 적용)
        private static readonly Dictionary<string, int> NegativeKeywords = new Dictionary<string, int>
       {
           // 심각한 악재 (높은 감점)
           {"횡령", -25}, {"분식회계", -25}, {"배임", -20}, {"사기", -20},
           {"리콜", -15}, {"결함", -12}, {"하자", -10},
           
           // 법적 문제
           {"소송", -15}, {"고발", -12}, {"기소", -15}, {"조사", -10},
           {"압수수색", -20}, {"구속", -25}, {"벌금", -10},
           
           // 사업 악화
           {"계약해지", -15}, {"사업중단", -12}, {"공장가동중단", -15},
           {"영업정지", -20}, {"허가취소", -18}, {"면허취소", -18},
           
           // 실적 악화
           {"적자", -10}, {"손실", -8}, {"매출감소", -8}, {"실적악화", -10},
           {"목표하향", -12}, {"가이던스하향", -12},
           
           // 기타 부정 요인
           {"파업", -8}, {"사고", -10}, {"화재", -12}, {"폭발", -15},
           {"규제강화", -8}, {"제재", -10}
       };

        /// <summary>
        /// 종목의 뉴스 분석 수행 (0~120점, 뉴스 없음 = 중립 60점)
        /// </summary>
        /// <param name="stock">주식 데이터</param>
        /// <param name="newsItems">관련 뉴스 리스트</param>
        /// <param name="analysisDate">분석 날짜</param>
        /// <returns>뉴스 분석 결과</returns>

        public static NewsAnalysisResult Analyze(Stock stock, List<News> newsItems, DateTime analysisDate)
        {
            var result = new NewsAnalysisResult
            {
                PositiveKeywords = new List<string>(),
                NegativeKeywords = new List<string>(),
                NewsCount = newsItems?.Count ?? 0
            };

            // 🆕 뉴스가 없을 때 중간 점수 (기존 0점에서 변경)
            if (newsItems == null || newsItems.Count == 0)
            {
                result.NewsScore = 60;  // 120점 만점의 50% (중립)
                result.MainFactor = "뉴스 정보 없음 (중립 평가)";
                result.SustainabilityStatus = "보통";
                result.RiskWarning = "없음";
                result.FreshnessScore = 30;    // 중간값
                result.ImpactScore = 15;       // 중간값  
                result.CredibilityScore = 10;  // 중간값
                result.SustainabilityScore = 15; // 중간값
                result.FadingRisk = 0;         // 위험 없음
                result.NegativeRisk = 0;       // 악재 없음

                System.Diagnostics.Debug.WriteLine($"📰 {stock?.Name ?? "종목"}: 뉴스 없음 → 중립 점수 60점 적용");
                return result;
            }

            System.Diagnostics.Debug.WriteLine($"📰 {stock?.Name ?? "종목"}: {newsItems.Count}개 뉴스로 분석 시작");

            // 1. 호재 신선도 평가 (0~60점)
            result.FreshnessScore = CalculateFreshnessScore(newsItems, analysisDate);

            // 2. 호재 파급력 평가 (0~40점)
            result.ImpactScore = CalculateImpactScore(newsItems, stock);

            // 3. 뉴스 신뢰도 평가 (0~20점)
            result.CredibilityScore = CalculateCredibilityScore(newsItems);

            // 4. 지속성 점수 계산 (0~40점)
            result.SustainabilityScore = CalculateSustainabilityScore(newsItems, stock);

            // 5. 호재 소멸 위험 감지 (-80점)
            result.FadingRisk = DetectFadingRisk(stock, newsItems);

            // 6. 악재 발생 감지 (-120점)
            result.NegativeRisk = DetectNegativeRisk(newsItems);

            // 키워드 추출
            ExtractKeywords(newsItems, result);

            // 🆕 악재 키워드가 하나라도 있으면 강제 저점수
            if (result.NegativeKeywords.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ {stock?.Name ?? "종목"}: 악재 키워드 감지 → 강제 저점수 적용");
                System.Diagnostics.Debug.WriteLine($"   악재 키워드: {string.Join(", ", result.NegativeKeywords)}");

                result.NewsScore = 20; // 강제로 20점 (매우 낮은 점수)
                result.MainFactor = $"악재 감지: {string.Join(", ", result.NegativeKeywords)}";
                result.RiskWarning = "심각한 악재";
                result.SustainabilityStatus = "매우낮음 (악재)";

                System.Diagnostics.Debug.WriteLine($"📊 {stock?.Name ?? "종목"} 악재로 인한 강제 저점수: {result.NewsScore}점");
                return result;
            }

            // 기존 점수 계산 로직 (악재가 없는 경우에만 실행)
            result.NewsScore = result.FreshnessScore + result.ImpactScore +
                              result.CredibilityScore + result.SustainabilityScore +
                              result.FadingRisk + result.NegativeRisk;

            // 4배 확대 적용
            result.NewsScore *= 4.0;

            // 점수 범위 재조정 (0~120, 중립=60)
            if (result.NewsScore < 0)
            {
                // 악재가 있는 경우: -120 ~ 0을 0 ~ 60으로 변환
                result.NewsScore = Math.Max(0, 60 + (result.NewsScore / 2));
            }
            else
            {
                // 호재가 있는 경우: 0 ~ 120을 60 ~ 120으로 변환  
                result.NewsScore = Math.Min(120, 60 + (result.NewsScore / 2));
            }

            // 최종 범위 보장
            result.NewsScore = Math.Max(0, Math.Min(120, result.NewsScore));

            // 상세 정보 생성
            result.MainFactor = DetermineMainFactor(newsItems, result);
            result.SustainabilityStatus = GetSustainabilityStatus(result);
            result.RiskWarning = GetRiskWarning(result);

            System.Diagnostics.Debug.WriteLine($"📊 {stock?.Name ?? "종목"} 뉴스 분석 완료: {result.NewsScore:F1}점 (호재:{result.PositiveKeywords.Count}, 악재:{result.NegativeKeywords.Count})");

            return result;
        }

 

        /// <summary>
        /// 호재 신선도 평가 (0~60점) ✅ 4배 확대
        /// </summary>
        private static double CalculateFreshnessScore(List<News> newsItems, DateTime analysisDate)
        {
            double score = 0;

            foreach (var news in newsItems)
            {
                var daysDiff = (analysisDate - news.PublishDate).TotalDays;

                if (daysDiff <= 1)      // 당일 뉴스
                    score = Math.Max(score, 60);   // 15 → 60
                else if (daysDiff <= 2) // 전일 뉴스
                    score = Math.Max(score, 40);   // 10 → 40
                else if (daysDiff <= 3) // 2-3일 전
                    score = Math.Max(score, 20);   // 5 → 20
                else if (daysDiff <= 7) // 일주일 이내
                    score = Math.Max(score, 8);    // 2 → 8
            }

            return score;
        }

        /// <summary>
        /// 호재 파급력 평가 (0~40점) ✅ 4배 확대
        /// </summary>
        private static double CalculateImpactScore(List<News> newsItems, Stock stock)
        {
            double score = 0;

            foreach (var news in newsItems)
            {
                var title = news.Title?.ToLower() ?? "";
                var content = news.Content?.ToLower() ?? "";
                var text = title + " " + content;

                // 업종 전체 영향 키워드
                if (ContainsAny(text, new[] { "정부", "정책", "규제완화", "법안", "업계", "전체" }))
                    score = Math.Max(score, 40);   // 10 → 40

                // 대형 계약/사업 키워드
                else if (ContainsAny(text, new[] { "대형", "수천억", "수백억", "조원", "글로벌" }))
                    score = Math.Max(score, 32);   // 8 → 32

                // 관련 종목 동반 상승 가능성
                else if (ContainsAny(text, new[] { "관련주", "테마주", "동반상승", "연관" }))
                    score = Math.Max(score, 28);   // 7 → 28

                // 개별 종목 한정
                else
                    score = Math.Max(score, 16);   // 4 → 16
            }

            return score;
        }

        /// <summary>
        /// 뉴스 신뢰도 평가 (0~20점) ✅ 4배 확대
        /// </summary>
        private static double CalculateCredibilityScore(List<News> newsItems)
        {
            double score = 0;

            foreach (var news in newsItems)
            {
                var source = news.Source?.ToLower() ?? "";

                // 공식 공시/IR
                if (ContainsAny(source, new[] { "공시", "ir", "사업보고서", "분기보고서" }))
                    score = Math.Max(score, 20);   // 5 → 20

                // 주요 경제지
                else if (ContainsAny(source, new[] { "한국경제", "매일경제", "머니투데이", "이데일리", "조선비즈" }))
                    score = Math.Max(score, 16);   // 4 → 16

                // 일반 경제지
                else if (ContainsAny(source, new[] { "연합뉴스", "뉴스1", "뉴시스", "파이낸셜뉴스" }))
                    score = Math.Max(score, 12);   // 3 → 12

                // 업계지/전문지
                else if (ContainsAny(source, new[] { "전자신문", "디지털타임스", "바이오스펙테이터" }))
                    score = Math.Max(score, 8);    // 2 → 8

                // 기타 또는 불명
                else
                    score = Math.Max(score, 4);    // 1 → 4
            }

            return score;
        }

        /// <summary>
        /// 지속성 점수 계산 (0~40점) ✅ 4배 확대
        /// </summary>
        private static double CalculateSustainabilityScore(List<News> newsItems, Stock stock)
        {
            double score = 0;

            // 뉴스 내용 기반 지속성 판단
            foreach (var news in newsItems)
            {
                var text = (news.Title + " " + news.Content).ToLower();

                // 장기적 호재 (높은 지속성)
                if (ContainsAny(text, new[] { "신규사업", "사업확장", "투자", "공장설립", "기술개발" }))
                    score = Math.Max(score, 40);   // 10 → 40

                // 중기적 호재
                else if (ContainsAny(text, new[] { "계약", "수주", "제휴", "협력", "승인" }))
                    score = Math.Max(score, 28);   // 7 → 28

                // 단기적 호재
                else if (ContainsAny(text, new[] { "실적", "매출", "이익", "목표" }))
                    score = Math.Max(score, 16);   // 4 → 16

                // 일회성 호재
                else if (ContainsAny(text, new[] { "배당", "자사주", "이벤트" }))
                    score = Math.Max(score, 8);    // 2 → 8
            }

            // 뉴스 개수가 많을수록 지속성 높음
            if (newsItems.Count >= 5)
                score += 8;    // 2 → 8
            else if (newsItems.Count >= 3)
                score += 4;    // 1 → 4

            return Math.Min(score, 40);  // 10 → 40
        }

        /// <summary>
        /// 호재 소멸 위험 감지 (-80점) ✅ 4배 확대
        /// </summary>
        private static double DetectFadingRisk(Stock stock, List<News> newsItems)
        {
            var risks = new List<string>();

            // 1. 이미 큰 상승 후 추가 호재 뉴스
            if (stock.ChangeRate > 15) // 15% 이상 상승
            {
                risks.Add("급등후추가호재");
            }

            // 2. 거래량 급감 패턴
            if (stock.DailyPrices?.Count >= 2)
            {
                var recent = stock.DailyPrices.OrderByDescending(x => x.Date).Take(2).ToList();
                if (recent.Count == 2 && recent[0].Volume < recent[1].Volume * 0.5) // 50% 이하로 감소
                {
                    risks.Add("거래량급감");
                }
            }

            // 3. 과거 동일 호재로 급등했던 이력 체크
            var recentNewsKeywords = string.Join(" ", newsItems.Select(n => n.Title + " " + n.Content));
            if (ContainsAny(recentNewsKeywords.ToLower(), new[] { "재발표", "재공지", "추가발표" }))
            {
                risks.Add("동일호재반복");
            }

            return risks.Count > 0 ? -60 : 0; // -15 → -60
        }

        /// <summary>
        /// 악재 발생 감지 (-120점) ✅ 4배 확대
        /// </summary>
        private static double DetectNegativeRisk(List<News> newsItems)
        {
            double penalty = 0;

            foreach (var news in newsItems)
            {
                var text = (news.Title + " " + news.Content).ToLower();

                // ✅ 🆕 확대된 부정적 키워드 사전 (4배 확대)
                var expandedNegativeKeywords = new Dictionary<string, int>
               {
                   // 심각한 악재 (4배 확대)
                   {"횡령", -120}, {"분식회계", -120}, {"배임", -80}, {"사기", -80},
                   {"리콜", -60}, {"결함", -48}, {"하자", -40},
                   
                   // 법적 문제 (4배 확대)
                   {"소송", -60}, {"고발", -48}, {"기소", -60}, {"조사", -40},
                   {"압수수색", -80}, {"구속", -100}, {"벌금", -40},
                   
                   // 사업 악화 (4배 확대)
                   {"계약해지", -60}, {"사업중단", -48}, {"공장가동중단", -60},
                   {"영업정지", -80}, {"허가취소", -72}, {"면허취소", -72},
                   
                   // 실적 악화 (4배 확대)
                   {"적자", -40}, {"손실", -32}, {"매출감소", -32}, {"실적악화", -40},
                   {"목표하향", -48}, {"가이던스하향", -48},
                   
                   // 기타 부정 요인 (4배 확대)
                   {"파업", -32}, {"사고", -40}, {"화재", -48}, {"폭발", -60},
                   {"규제강화", -32}, {"제재", -40}
               };

                foreach (var negativeKeyword in expandedNegativeKeywords)
                {
                    if (text.Contains(negativeKeyword.Key))
                    {
                        penalty += negativeKeyword.Value; // 음수 값이므로 penalty가 감소
                    }
                }
            }

            return Math.Max(penalty, -120); // 최대 -120점까지만
        }

        /// <summary>
        /// 키워드 추출 (기존과 동일)
        /// </summary>
        private static void ExtractKeywords(List<News> newsItems, NewsAnalysisResult result)
        {
            foreach (var news in newsItems)
            {
                var text = (news.Title + " " + news.Content).ToLower();

                // 긍정 키워드 추출
                foreach (var keyword in PositiveKeywords.Keys)
                {
                    if (text.Contains(keyword) && !result.PositiveKeywords.Contains(keyword))
                    {
                        result.PositiveKeywords.Add(keyword);
                    }
                }

                // 부정 키워드 추출
                foreach (var keyword in NegativeKeywords.Keys)
                {
                    if (text.Contains(keyword) && !result.NegativeKeywords.Contains(keyword))
                    {
                        result.NegativeKeywords.Add(keyword);
                    }
                }
            }
        }

        /// <summary>
        /// 주요 상승 요인 결정 (기존과 동일)
        /// </summary>
        private static string DetermineMainFactor(List<News> newsItems, NewsAnalysisResult result)
        {
            if (result.PositiveKeywords.Count == 0)
                return "구체적 호재 없음";

            // 가장 점수가 높은 키워드 찾기
            var topKeyword = result.PositiveKeywords
                .Where(k => PositiveKeywords.ContainsKey(k))
                .OrderByDescending(k => PositiveKeywords[k])
                .FirstOrDefault();

            return topKeyword ?? result.PositiveKeywords.First();
        }

        /// <summary>
        /// 지속성 상태 판단 (120점 기준으로 조정)
        /// </summary>
        private static string GetSustainabilityStatus(NewsAnalysisResult result)
        {
            if (result.SustainabilityScore >= 32)      // 8*4 = 32
                return "높음 (장기호재)";
            else if (result.SustainabilityScore >= 24) // 6*4 = 24
                return "보통 (중기호재)";
            else if (result.SustainabilityScore >= 12) // 3*4 = 12
                return "낮음 (단기호재)";
            else
                return "매우낮음 (일회성)";
        }

        /// <summary>
        /// 위험 경고 생성 (120점 기준으로 조정)
        /// </summary>
        private static string GetRiskWarning(NewsAnalysisResult result)
        {
            var warnings = new List<string>();

            if (result.FadingRisk < -40)  // -10*4 = -40
                warnings.Add("호재소멸위험");

            if (result.NegativeRisk < -60)      // -15*4 = -60
                warnings.Add("심각한악재");
            else if (result.NegativeRisk < -20) // -5*4 = -20
                warnings.Add("악재주의");

            if (result.FreshnessScore < 12)     // 3*4 = 12
                warnings.Add("호재신선도낮음");

            return warnings.Count > 0 ? string.Join(", ", warnings) : "없음";
        }

        /// <summary>
        /// 문자열에 특정 키워드들이 포함되어 있는지 확인 (기존과 동일)
        /// </summary>
        private static bool ContainsAny(string text, string[] keywords)
        {
            return keywords.Any(keyword => text.Contains(keyword));
        }
    }
}