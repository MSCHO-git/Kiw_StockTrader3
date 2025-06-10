using System;
using HolidayManager.Managers;

namespace HolidayManager.Managers
{
    /// <summary>
    /// 한국 증시 영업일 계산 관리자
    /// </summary>
    public static class TradingDayManager
    {
        /// <summary>
        /// 해당 날짜가 영업일인지 확인
        /// </summary>
        /// <param name="날짜">확인할 날짜</param>
        /// <returns>영업일이면 true, 휴일이면 false</returns>
        public static bool IsBusinessDay(DateTime 날짜)
        {
            // 1️⃣ 주말 체크 (토요일, 일요일)
            if (날짜.DayOfWeek == DayOfWeek.Saturday ||
                날짜.DayOfWeek == DayOfWeek.Sunday)
            {
                return false;
            }

            // 2️⃣ 휴일 테이블 체크
            string 날짜문자열 = 날짜.ToString("yyyy-MM-dd");
            var sql = "SELECT COUNT(*) FROM Holidays WHERE HolidayDate = @date";
            var count = DatabaseManager.ExecuteScalar<int>(sql, new { date = 날짜문자열 });

            return count == 0; // 휴일 테이블에 없으면 영업일
        }

        /// <summary>
        /// 가장 최근 완료된 거래일 반환 (분석일자 계산용)
        /// </summary>
        /// <returns>마지막 거래일</returns>
        public static DateTime GetLastTradingDate()
        {
            DateTime 오늘 = DateTime.Today;
            DateTime 현재시간 = DateTime.Now;

            // 오늘이 영업일이고 15:30 이후면 오늘이 마지막 거래일
            if (IsBusinessDay(오늘) && 현재시간.TimeOfDay >= TimeSpan.Parse("15:30"))
            {
                return 오늘;
            }

            // 이전 영업일 찾기
            return GetPreviousBusinessDay(오늘);
        }

        /// <summary>
        /// 이전 영업일 찾기
        /// </summary>
        /// <param name="기준일">기준 날짜</param>
        /// <returns>이전 영업일</returns>
        public static DateTime GetPreviousBusinessDay(DateTime 기준일)
        {
            DateTime 이전일 = 기준일.AddDays(-1);

            while (!IsBusinessDay(이전일))
            {
                이전일 = 이전일.AddDays(-1);
            }

            return 이전일;
        }

        /// <summary>
        /// 다음 영업일 찾기
        /// </summary>
        /// <param name="기준일">기준 날짜</param>
        /// <returns>다음 영업일</returns>
        public static DateTime GetNextBusinessDay(DateTime 기준일)
        {
            DateTime 다음일 = 기준일.AddDays(1);

            while (!IsBusinessDay(다음일))
            {
                다음일 = 다음일.AddDays(1);
            }

            return 다음일;
        }

        /// <summary>
        /// 두 날짜 사이의 영업일 수 계산
        /// </summary>
        /// <param name="시작일">시작 날짜</param>
        /// <param name="종료일">종료 날짜</param>
        /// <returns>영업일 수</returns>
        public static int GetBusinessDaysBetween(DateTime 시작일, DateTime 종료일)
        {
            int 영업일수 = 0;
            DateTime 현재일 = 시작일;

            while (현재일 <= 종료일)
            {
                if (IsBusinessDay(현재일))
                {
                    영업일수++;
                }
                현재일 = 현재일.AddDays(1);
            }

            return 영업일수;
        }

        /// <summary>
        /// 날짜의 한국어 요일명 반환
        /// </summary>
        /// <param name="날짜">날짜</param>
        /// <returns>한국어 요일</returns>
        public static string GetKoreanDayOfWeek(DateTime 날짜)
        {
            return 날짜.ToString("dddd", new System.Globalization.CultureInfo("ko-KR"));
        }

        /// <summary>
        /// 영업일 상태 문자열 반환
        /// </summary>
        /// <param name="날짜">날짜</param>
        /// <returns>상태 문자열</returns>
        public static string GetBusinessDayStatus(DateTime 날짜)
        {
            if (IsBusinessDay(날짜))
            {
                return "영업일";
            }
            else if (날짜.DayOfWeek == DayOfWeek.Saturday || 날짜.DayOfWeek == DayOfWeek.Sunday)
            {
                return "주말";
            }
            else
            {
                return "공휴일";
            }
        }
    }
}