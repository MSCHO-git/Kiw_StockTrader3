using System;

namespace StockTrader3.Models
{
    /// <summary>
    /// 뉴스 데이터를 담는 클래스 (WebNewsCollector 완전 호환)
    /// </summary>
    public class News
    {
        /// <summary>뉴스 ID (DB 자동 생성)</summary>
        public int Id { get; set; }

        /// <summary>제목</summary>
        public string Title { get; set; } = "";

        /// <summary>내용</summary>
        public string Content { get; set; } = "";

        /// <summary>발행 날짜</summary>
        public DateTime PublishDate { get; set; } = DateTime.Now;

        /// <summary>출처 (언론사)</summary>
        public string Source { get; set; } = "";

        /// <summary>뉴스 URL</summary>
        public string Url { get; set; } = "";

        /// <summary>감성 점수 (-1.0 ~ 1.0)</summary>
        public double SentimentScore { get; set; } = 0.0;

        /// <summary>중요도 점수 (0.0 ~ 1.0)</summary>
        public double ImportanceScore { get; set; } = 0.0;

        /// <summary>관련 종목 코드</summary>
        public string RelatedStockCode { get; set; } = "";

        /// <summary>
        /// 기본 생성자
        /// </summary>
        public News()
        {
            Title = "";
            Content = "";
            Source = "";
            Url = "";
            RelatedStockCode = "";
            PublishDate = DateTime.Now;
            SentimentScore = 0.0;
            ImportanceScore = 0.0;
        }

        /// <summary>
        /// 매개변수가 있는 생성자
        /// </summary>
        public News(string title, string content, string source = "", string url = "", string relatedStockCode = "")
        {
            Title = title ?? "";
            Content = content ?? "";
            Source = source ?? "";
            Url = url ?? "";
            RelatedStockCode = relatedStockCode ?? "";
            PublishDate = DateTime.Now;
            SentimentScore = 0.0;
            ImportanceScore = 0.0;
        }

        /// <summary>
        /// 뉴스 데이터 유효성 검사
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Title) && Title.Length >= 5;
        }

        /// <summary>
        /// 뉴스 요약 정보 반환
        /// </summary>
        public override string ToString()
        {
            return $"[{Source}] {Title} ({PublishDate:yyyy-MM-dd})";
        }
    }
}