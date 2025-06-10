using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using StockTrader3.Models;

namespace StockTrader3.Services
{
    /// <summary>
    /// 웹 크롤링을 통한 뉴스 수집 클래스 (단순화 버전)
    /// 구글 뉴스 RSS를 통해 종목별 관련 뉴스를 수집합니다.
    /// </summary>
    public class WebNewsCollector : IDisposable
    {
        private readonly HttpClient _httpClient;

        public WebNewsCollector()
        {
            // HttpClient 설정
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
            _httpClient.Timeout = TimeSpan.FromSeconds(15);
        }

        /// <summary>
        /// 종목명으로 관련 뉴스를 수집합니다.
        /// </summary>
        /// <param name="stockName">종목명 (예: "삼성전자")</param>
        /// <returns>관련 뉴스 리스트</returns>
        public async Task<List<News>> GetStockNews(string stockName)
        {
            if (string.IsNullOrEmpty(stockName))
                return new List<News>();

            try
            {
                System.Diagnostics.Debug.WriteLine($"📰 {stockName} 구글 뉴스 수집 시작...");

                // 1. 구글 뉴스 RSS URL 생성
                var searchUrl = BuildGoogleNewsUrl(stockName);
                System.Diagnostics.Debug.WriteLine($"🔍 구글 RSS URL: {searchUrl}");

                // 2. RSS XML 가져오기
                var rssXml = await FetchRssXmlAsync(searchUrl);

                if (string.IsNullOrEmpty(rssXml))
                {
                    System.Diagnostics.Debug.WriteLine($"❌ {stockName} RSS XML 가져오기 실패");
                    return new List<News>();
                }

                // 3. RSS XML에서 뉴스 파싱
                var newsItems = ParseNewsFromRss(rssXml, stockName);

                // 4. 🆕 단순한 관련성 필터링 (종목명만 체크)
                var relevantNews = FilterRelevantNewsSimple(newsItems, stockName);

                System.Diagnostics.Debug.WriteLine($"✅ {stockName} 구글 뉴스 수집 완료: {relevantNews.Count}개");

                return relevantNews;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ {stockName} 구글 뉴스 수집 실패: {ex.Message}");
                return new List<News>();
            }
        }

        /// <summary>
        /// 구글 뉴스 RSS URL 생성
        /// </summary>
        private string BuildGoogleNewsUrl(string stockName)
        {
            var encodedStockName = Uri.EscapeDataString($"{stockName}");

            // 구글 뉴스 RSS URL (한국어, 최근 관련 뉴스)
            return $"https://news.google.com/rss/search?q={encodedStockName}&hl=ko&gl=KR&ceid=KR:ko";
        }

        /// <summary>
        /// HTTP 요청으로 RSS XML 가져오기
        /// </summary>
        private async Task<string> FetchRssXmlAsync(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"📄 RSS XML 길이: {content.Length}자");
                return content;
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"🌐 HTTP 요청 실패: {ex.Message}");
                return null;
            }
            catch (TaskCanceledException ex)
            {
                System.Diagnostics.Debug.WriteLine($"⏰ 요청 타임아웃: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// RSS XML에서 뉴스 항목들을 파싱
        /// </summary>
        private List<News> ParseNewsFromRss(string rssXml, string stockName)
        {
            var newsList = new List<News>();

            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(rssXml);

                // RSS 네임스페이스 관리자 설정
                var namespaceManager = new XmlNamespaceManager(doc.NameTable);
                namespaceManager.AddNamespace("atom", "http://www.w3.org/2005/Atom");

                // RSS 아이템들 선택 (Google News는 Atom 형식 사용)
                var itemNodes = doc.SelectNodes("//item") ?? doc.SelectNodes("//atom:entry", namespaceManager);

                if (itemNodes == null || itemNodes.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("🔍 RSS 아이템을 찾을 수 없습니다.");
                    return newsList;
                }

                System.Diagnostics.Debug.WriteLine($"📋 RSS에서 {itemNodes.Count}개 아이템 발견");

                foreach (XmlNode itemNode in itemNodes.Cast<XmlNode>().Take(20)) // 최대 20개까지
                {
                    try
                    {
                        var news = ExtractNewsFromRssItem(itemNode, stockName, namespaceManager);
                        if (news != null)
                        {
                            newsList.Add(news);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"⚠️ 개별 RSS 아이템 파싱 실패: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ RSS XML 파싱 실패: {ex.Message}");
            }

            return newsList;
        }

        /// <summary>
        /// 개별 RSS 아이템에서 뉴스 정보 추출
        /// </summary>
        private News ExtractNewsFromRssItem(XmlNode itemNode, string stockName, XmlNamespaceManager namespaceManager)
        {
            try
            {
                // 제목 추출
                var titleNode = itemNode.SelectSingleNode("title") ?? itemNode.SelectSingleNode("atom:title", namespaceManager);
                var title = titleNode?.InnerText?.Trim() ?? "";

                // URL 추출
                var linkNode = itemNode.SelectSingleNode("link") ?? itemNode.SelectSingleNode("atom:link/@href", namespaceManager);
                var url = linkNode?.InnerText?.Trim() ?? linkNode?.Value?.Trim() ?? "";

                // 내용 추출
                var descriptionNode = itemNode.SelectSingleNode("description") ??
                                     itemNode.SelectSingleNode("atom:summary", namespaceManager) ??
                                     itemNode.SelectSingleNode("atom:content", namespaceManager);
                var content = descriptionNode?.InnerText?.Trim() ?? "";

                // HTML 태그 제거
                content = CleanText(content);

                // 출처 추출 (구글 뉴스는 source 태그 사용)
                var sourceNode = itemNode.SelectSingleNode("source") ?? itemNode.SelectSingleNode("atom:source", namespaceManager);
                var source = sourceNode?.InnerText?.Trim() ?? "구글뉴스";

                // 발행 시간 추출
                var pubDateNode = itemNode.SelectSingleNode("pubDate") ?? itemNode.SelectSingleNode("atom:published", namespaceManager);
                var publishDate = ParsePublishDate(pubDateNode?.InnerText);

                // 기본 유효성 검사
                if (string.IsNullOrEmpty(title) || title.Length < 5)
                {
                    return null;
                }

                // 새로운 News 생성
                var news = new News(title, content, source, url, stockName);
                news.PublishDate = publishDate;

                return news;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ RSS 아이템 추출 실패: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 🆕 단순한 종목 관련성 필터링 (종목명만 체크)
        /// </summary>
        private List<News> FilterRelevantNewsSimple(List<News> newsList, string stockName)
        {
            var relevantNews = new List<News>();

            foreach (var news in newsList)
            {
                if (IsRelevantToStockSimple(news, stockName))
                {
                    relevantNews.Add(news);
                    System.Diagnostics.Debug.WriteLine($"✅ 관련 뉴스: {news.Title.Substring(0, Math.Min(50, news.Title.Length))}...");
                }
            }

            return relevantNews;
        }

        /// <summary>
        /// 🆕 단순한 뉴스 관련성 판단 (종목명만 포함되면 OK)
        /// </summary>

        /// <summary>
        /// 🆕 공백 제거 후 종목명 매칭 (삼성전자 = 삼성 전자 = 삼 성전자)
        /// </summary>
        private bool IsRelevantToStockSimple(News news, string stockName)
        {
            if (news == null || string.IsNullOrEmpty(news.Title))
                return false;

            var fullText = (news.Title + " " + news.Content).ToLower();
            var normalizedStockName = stockName.ToLower();

            // 🆕 공백 제거 후 비교
            var fullTextNoSpaces = fullText.Replace(" ", "").Replace("\t", "").Replace("\n", "");
            var stockNameNoSpaces = normalizedStockName.Replace(" ", "").Replace("\t", "").Replace("\n", "");

            // 공백 제거한 종목명이 포함되면 관련 뉴스
            bool isRelevant = fullTextNoSpaces.Contains(stockNameNoSpaces);

            if (isRelevant)
            {
                System.Diagnostics.Debug.WriteLine($"✅ 매칭: '{stockName}' ↔ '{news.Title.Substring(0, Math.Min(50, news.Title.Length))}...'");
            }

            return isRelevant;
        }

      
        /// <summary>
        /// 발행 시간 파싱 (RFC 822 또는 ISO 8601 형식)
        /// </summary>
        private DateTime ParsePublishDate(string timeText)
        {
            if (string.IsNullOrEmpty(timeText))
                return DateTime.Now;

            try
            {
                // RFC 822 형식 (RSS 표준)
                if (DateTime.TryParse(timeText, out DateTime parsedDate))
                    return parsedDate;

                // ISO 8601 형식 (Atom 표준)
                if (DateTimeOffset.TryParse(timeText, out DateTimeOffset parsedOffset))
                    return parsedOffset.DateTime;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ 시간 파싱 실패: {timeText}, {ex.Message}");
            }

            return DateTime.Now; // 파싱 실패 시 현재 시간
        }

        /// <summary>
        /// 텍스트 정리 (HTML 태그 제거, 공백 정리 등)
        /// </summary>
        private string CleanText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            // HTML 태그 제거
            text = Regex.Replace(text, @"<[^>]+>", "");

            // 연속 공백을 단일 공백으로
            text = Regex.Replace(text, @"\s+", " ");

            // 앞뒤 공백 제거
            text = text.Trim();

            // HTML 엔티티 디코딩
            text = System.Net.WebUtility.HtmlDecode(text);

            return text;
        }

        /// <summary>
        /// 리소스 해제
        /// </summary>
        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}