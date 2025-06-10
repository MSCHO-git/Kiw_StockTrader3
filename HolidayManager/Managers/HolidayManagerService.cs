using System;
using System.Collections.Generic;
using System.Linq;
using HolidayManager.Models;
using HolidayManager.Managers;

namespace HolidayManager.Managers
{
    /// <summary>
    /// 휴일 관리 서비스
    /// </summary>
    public static class HolidayManagerService
    {
        /// <summary>
        /// 새 휴일 추가
        /// </summary>
        /// <param name="날짜">휴일 날짜</param>
        /// <param name="휴일명">휴일 이름</param>
        /// <param name="유형">휴일 유형</param>
        /// <param name="비고">비고</param>
        public static void AddHoliday(DateTime 날짜, string 휴일명, string 유형 = "기타", string 비고 = null)
        {
            var sql = @"INSERT OR REPLACE INTO Holidays 
                       (HolidayDate, HolidayName, HolidayType, Notes, UpdatedAt) 
                       VALUES (@date, @name, @type, @notes, @updated)";

            DatabaseManager.Execute(sql, new
            {
                date = 날짜.ToString("yyyy-MM-dd"),
                name = 휴일명,
                type = 유형,
                notes = 비고,
                updated = DateTime.Now
            });

            Console.WriteLine($"휴일 추가: {날짜:yyyy-MM-dd} ({휴일명})");
        }

        /// <summary>
        /// 휴일 삭제
        /// </summary>
        /// <param name="날짜">삭제할 휴일 날짜</param>
        /// <returns>삭제된 행 수</returns>
        public static int RemoveHoliday(DateTime 날짜)
        {
            var sql = "DELETE FROM Holidays WHERE HolidayDate = @date";
            var affected = DatabaseManager.Execute(sql, new { date = 날짜.ToString("yyyy-MM-dd") });

            if (affected > 0)
                Console.WriteLine($"휴일 삭제: {날짜:yyyy-MM-dd}");
            else
                Console.WriteLine($"삭제할 휴일 없음: {날짜:yyyy-MM-dd}");

            return affected;
        }

        /// <summary>
        /// 특정 연도의 모든 휴일 조회
        /// </summary>
        /// <param name="연도">조회할 연도</param>
        /// <returns>휴일 목록</returns>
        public static List<Holiday> GetHolidays(int 연도)
        {
            var sql = @"SELECT HolidayDate, HolidayName, HolidayType, Notes, CreatedAt, UpdatedAt 
                       FROM Holidays 
                       WHERE HolidayDate LIKE @year 
                       ORDER BY HolidayDate";

            return DatabaseManager.Query<Holiday>(sql, new { year = $"{연도}%" }).ToList();
        }

        /// <summary>
        /// 모든 휴일 조회
        /// </summary>
        /// <returns>모든 휴일 목록</returns>
        public static List<Holiday> GetAllHolidays()
        {
            var sql = @"SELECT HolidayDate, HolidayName, HolidayType, Notes, CreatedAt, UpdatedAt 
                       FROM Holidays 
                       ORDER BY HolidayDate";

            return DatabaseManager.Query<Holiday>(sql).ToList();
        }

        /// <summary>
        /// 특정 날짜의 휴일 정보 조회
        /// </summary>
        /// <param name="날짜">조회할 날짜</param>
        /// <returns>휴일 정보 (없으면 null)</returns>
        public static Holiday GetHoliday(DateTime 날짜)
        {
            var sql = @"SELECT HolidayDate, HolidayName, HolidayType, Notes, CreatedAt, UpdatedAt 
                       FROM Holidays 
                       WHERE HolidayDate = @date";

            return DatabaseManager.QueryFirstOrDefault<Holiday>(sql,
                   new { date = 날짜.ToString("yyyy-MM-dd") });
        }

        /// <summary>
        /// 연도별 고정 공휴일 초기화
        /// </summary>
        /// <param name="연도">초기화할 연도</param>
        public static void InitializeFixedHolidays(int 연도)
        {
            var 고정공휴일 = new Dictionary<string, string>
            {
                {"01-01", "신정"},
                {"03-01", "삼일절"},
                {"05-05", "어린이날"},
                {"06-06", "현충일"},
                {"08-15", "광복절"},
                {"10-03", "개천절"},
                {"10-09", "한글날"},
                {"12-25", "크리스마스"}
            };

            foreach (var 휴일 in 고정공휴일)
            {
                try
                {
                    var 월 = int.Parse(휴일.Key.Split('-')[0]);
                    var 일 = int.Parse(휴일.Key.Split('-')[1]);
                    var 날짜 = new DateTime(연도, 월, 일);

                    AddHoliday(날짜, 휴일.Value, "고정");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"휴일 추가 실패: {휴일.Key} - {ex.Message}");
                }
            }

            Console.WriteLine($"{연도}년 고정공휴일 초기화 완료. 음력공휴일은 수동 추가 필요.");
        }

        /// <summary>
        /// 특정 연도의 모든 휴일 삭제 (재초기화용)
        /// </summary>
        /// <param name="연도">삭제할 연도</param>
        /// <returns>삭제된 행 수</returns>
        public static int ClearHolidays(int 연도)
        {
            var sql = "DELETE FROM Holidays WHERE HolidayDate LIKE @year";
            var affected = DatabaseManager.Execute(sql, new { year = $"{연도}%" });

            Console.WriteLine($"{연도}년 휴일 {affected}개 삭제 완료");
            return affected;
        }

        /// <summary>
        /// 휴일 개수 조회
        /// </summary>
        /// <param name="연도">연도 (전체는 0)</param>
        /// <returns>휴일 개수</returns>
        public static int GetHolidayCount(int 연도 = 0)
        {
            string sql;
            object parameters = null;

            if (연도 == 0)
            {
                sql = "SELECT COUNT(*) FROM Holidays";
            }
            else
            {
                sql = "SELECT COUNT(*) FROM Holidays WHERE HolidayDate LIKE @year";
                parameters = new { year = $"{연도}%" };
            }

            return DatabaseManager.ExecuteScalar<int>(sql, parameters);
        }

        /// <summary>
        /// 등록된 연도 목록 조회
        /// </summary>
        /// <returns>연도 목록</returns>
        public static List<int> GetRegisteredYears()
        {
            var sql = @"SELECT DISTINCT SUBSTR(HolidayDate, 1, 4) as Year 
                       FROM Holidays 
                       ORDER BY Year";

            return DatabaseManager.Query<string>(sql)
                                 .Select(year => int.Parse(year))
                                 .ToList();
        }
    }
}