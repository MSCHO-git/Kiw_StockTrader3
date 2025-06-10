using System;

namespace HolidayManager.Models
{
    public class Holiday
    {
        /// <summary>
        /// 휴일 날짜 (yyyy-MM-dd 형식)
        /// </summary>
        public string HolidayDate { get; set; }

        /// <summary>
        /// 휴일 이름
        /// </summary>
        public string HolidayName { get; set; }

        /// <summary>
        /// 휴일 유형 (고정, 대체, 임시, 연휴)
        /// </summary>
        public string HolidayType { get; set; }

        /// <summary>
        /// 비고
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// 생성일시
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 수정일시
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// 문자열 날짜를 DateTime으로 변환
        /// </summary>
        public DateTime Date
        {
            get
            {
                if (DateTime.TryParse(HolidayDate, out DateTime result))
                    return result;
                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// 요일 반환
        /// </summary>
        public string DayOfWeek
        {
            get
            {
                return Date.ToString("dddd", new System.Globalization.CultureInfo("ko-KR"));
            }
        }

        /// <summary>
        /// 화면 표시용 문자열
        /// </summary>
        public override string ToString()
        {
            return $"{HolidayDate} ({HolidayName})";
        }

        /// <summary>
        /// 생성자
        /// </summary>
        public Holiday()
        {
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }
    }
}