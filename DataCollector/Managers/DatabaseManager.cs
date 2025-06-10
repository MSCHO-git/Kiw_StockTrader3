using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;

namespace DataCollector
{
    /// <summary>
    /// 데이터베이스 관리 및 과거 데이터 수집 전용 (일봉 + 분봉 지원)
    /// </summary>
    public class DatabaseManager : IDisposable
    {
        private readonly string _connectionString;
        private readonly string _databasePath;
        private SQLiteConnection _connection;
        private bool _disposed = false;

        public DatabaseManager(string databasePath = null)
        {
            _databasePath = databasePath ?? Path.Combine(
                Environment.CurrentDirectory, "StockTrader3.db");
            _connectionString = $"Data Source={_databasePath};Version=3;";
        }

        public async Task<bool> InitializeAsync()
        {
            try
            {
                _connection = new SQLiteConnection(_connectionString);
                await Task.Run(() => _connection.Open());
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ DB 초기화 실패: {ex.Message}");
                return false;
            }
        }

        public bool IsConnected => _connection?.State == ConnectionState.Open;

        #region 일봉 데이터 관리 (기존)

        /// <summary>
        /// DailyPriceHistory 테이블 생성
        /// </summary>
        public async Task CreateDailyPriceHistoryTableAsync()
        {
            try
            {
                string query = @"
                    CREATE TABLE IF NOT EXISTS DailyPriceHistory (
                        StockCode           TEXT NOT NULL,
                        Date               DATE NOT NULL,
                        Open               INTEGER NOT NULL,
                        High               INTEGER NOT NULL,
                        Low                INTEGER NOT NULL,
                        Close              INTEGER NOT NULL,
                        Volume             INTEGER NOT NULL,
                        CreatedAt          DATETIME DEFAULT CURRENT_TIMESTAMP,
                        
                        PRIMARY KEY (StockCode, Date)
                    );";

                using (var command = new SQLiteCommand(query, _connection))
                {
                    await Task.Run(() => command.ExecuteNonQuery());
                }
                System.Diagnostics.Debug.WriteLine("✅ DailyPriceHistory 테이블 생성 완료");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ 일봉 테이블 생성 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 일봉 데이터 저장
        /// </summary>
        public async Task<int> SaveHistoricalDataAsync(string stockCode, List<Models.DailyPrice> dailyPrices)
        {
            try
            {
                if (dailyPrices == null || dailyPrices.Count == 0)
                    return 0;

                using (var transaction = _connection.BeginTransaction())
                {
                    try
                    {
                        string insertQuery = @"
                            INSERT OR REPLACE INTO DailyPriceHistory 
                            (StockCode, Date, Open, High, Low, Close, Volume, CreatedAt)
                            VALUES (@StockCode, @Date, @Open, @High, @Low, @Close, @Volume, @CreatedAt);";

                        int insertedCount = 0;
                        foreach (var price in dailyPrices)
                        {
                            using (var command = new SQLiteCommand(insertQuery, _connection, transaction))
                            {
                                command.Parameters.AddWithValue("@StockCode", stockCode ?? "");
                                command.Parameters.AddWithValue("@Date", price.Date.Date);
                                command.Parameters.AddWithValue("@Open", (int)price.Open);
                                command.Parameters.AddWithValue("@High", (int)price.High);
                                command.Parameters.AddWithValue("@Low", (int)price.Low);
                                command.Parameters.AddWithValue("@Close", (int)price.Close);
                                command.Parameters.AddWithValue("@Volume", price.Volume);
                                command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                                await Task.Run(() => command.ExecuteNonQuery());
                                insertedCount++;
                            }
                        }

                        transaction.Commit();
                        System.Diagnostics.Debug.WriteLine($"✅ {stockCode} 일봉 데이터 {insertedCount}개 저장 완료");
                        return insertedCount;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ 일봉 데이터 저장 실패: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// 일봉 데이터 존재 여부 확인
        /// </summary>
        public async Task<bool> CheckHistoricalDataExistsAsync(string stockCode, int requiredDays = 60)
        {
            try
            {
                string query = @"
                    SELECT COUNT(*) 
                    FROM DailyPriceHistory 
                    WHERE StockCode = @StockCode;";

                using (var command = new SQLiteCommand(query, _connection))
                {
                    command.Parameters.AddWithValue("@StockCode", stockCode ?? "");

                    var count = await Task.Run(() => command.ExecuteScalar());
                    int existingDays = Convert.ToInt32(count);

                    System.Diagnostics.Debug.WriteLine($"📊 {stockCode} 일봉 데이터: {existingDays}일 (필요: {requiredDays}일)");

                    return existingDays >= requiredDays;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ 일봉 데이터 존재 확인 실패: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region 분봉 데이터 관리 (🆕 새로 추가)

        /// <summary>
        /// MinutePriceHistory 테이블 생성
        /// </summary>
        public async Task CreateMinutePriceHistoryTableAsync()
        {
            try
            {
                string query = @"
                    CREATE TABLE IF NOT EXISTS MinutePriceHistory (
                        StockCode           TEXT NOT NULL,
                        DateTime            TEXT NOT NULL,      -- 'YYYY-MM-DD HH:MM:SS' 형식
                        MinuteInterval      INTEGER NOT NULL,   -- 1, 3, 5, 15, 30, 60 (분 단위)
                        Open                INTEGER NOT NULL,
                        High                INTEGER NOT NULL,
                        Low                 INTEGER NOT NULL,
                        Close               INTEGER NOT NULL,
                        Volume              INTEGER NOT NULL,
                        ChangeAmount        INTEGER DEFAULT 0,  -- 전일대비
                        ChangeRate          REAL DEFAULT 0,     -- 전일대비율
                        TradingValue        INTEGER DEFAULT 0,  -- 거래대금
                        CreatedAt           DATETIME DEFAULT CURRENT_TIMESTAMP,
                        
                        PRIMARY KEY (StockCode, DateTime, MinuteInterval)
                    );";

                using (var command = new SQLiteCommand(query, _connection))
                {
                    await Task.Run(() => command.ExecuteNonQuery());
                }
                System.Diagnostics.Debug.WriteLine("✅ MinutePriceHistory 테이블 생성 완료");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ 분봉 테이블 생성 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 분봉 데이터 저장
        /// </summary>
        public async Task<int> SaveMinuteHistoricalDataAsync(string stockCode, int minuteInterval, List<Models.MinutePrice> minutePrices)
        {
            try
            {
                if (minutePrices == null || minutePrices.Count == 0)
                    return 0;

                using (var transaction = _connection.BeginTransaction())
                {
                    try
                    {
                        string insertQuery = @"
                            INSERT OR REPLACE INTO MinutePriceHistory 
                            (StockCode, DateTime, MinuteInterval, Open, High, Low, Close, Volume, 
                             ChangeAmount, ChangeRate, TradingValue, CreatedAt)
                            VALUES (@StockCode, @DateTime, @MinuteInterval, @Open, @High, @Low, @Close, @Volume,
                                    @ChangeAmount, @ChangeRate, @TradingValue, @CreatedAt);";

                        int insertedCount = 0;
                        foreach (var price in minutePrices)
                        {
                            using (var command = new SQLiteCommand(insertQuery, _connection, transaction))
                            {
                                command.Parameters.AddWithValue("@StockCode", stockCode ?? "");
                                command.Parameters.AddWithValue("@DateTime", price.DateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                                command.Parameters.AddWithValue("@MinuteInterval", minuteInterval);
                                command.Parameters.AddWithValue("@Open", (int)price.Open);
                                command.Parameters.AddWithValue("@High", (int)price.High);
                                command.Parameters.AddWithValue("@Low", (int)price.Low);
                                command.Parameters.AddWithValue("@Close", (int)price.Close);
                                command.Parameters.AddWithValue("@Volume", price.Volume);
                                command.Parameters.AddWithValue("@ChangeAmount", (int)price.ChangeAmount);
                                command.Parameters.AddWithValue("@ChangeRate", (double)price.ChangeRate);
                                command.Parameters.AddWithValue("@TradingValue", price.TradingValue);
                                command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                                await Task.Run(() => command.ExecuteNonQuery());
                                insertedCount++;
                            }
                        }

                        transaction.Commit();
                        System.Diagnostics.Debug.WriteLine($"✅ {stockCode} {minuteInterval}분봉 데이터 {insertedCount}개 저장 완료");
                        return insertedCount;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ 분봉 데이터 저장 실패: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// 분봉 데이터 조회
        /// </summary>
        public async Task<List<Models.MinutePrice>> GetMinuteHistoricalDataAsync(string stockCode, int minuteInterval, int days = 3)
        {
            try
            {
                var results = new List<Models.MinutePrice>();

                string query = @"
                    SELECT DateTime, Open, High, Low, Close, Volume, ChangeAmount, ChangeRate, TradingValue
                    FROM MinutePriceHistory 
                    WHERE StockCode = @StockCode AND MinuteInterval = @MinuteInterval
                    ORDER BY DateTime DESC 
                    LIMIT @Limit;";

                // 일당 분봉 개수 계산 (장시간 6.5시간 기준)
                int minutesPerDay = (int)(6.5 * 60 / minuteInterval); // 예: 1분봉이면 390개/일
                int limit = days * minutesPerDay;

                using (var command = new SQLiteCommand(query, _connection))
                {
                    command.Parameters.AddWithValue("@StockCode", stockCode ?? "");
                    command.Parameters.AddWithValue("@MinuteInterval", minuteInterval);
                    command.Parameters.AddWithValue("@Limit", limit);

                    using (var reader = await Task.Run(() => command.ExecuteReader()))
                    {
                        while (await Task.Run(() => reader.Read()))
                        {
                            var minutePrice = new Models.MinutePrice
                            {
                                StockCode = stockCode,
                                DateTime = DateTime.Parse(SafeGetString(reader, "DateTime")),
                                MinuteInterval = minuteInterval,
                                Open = SafeGetInt(reader, "Open"),
                                High = SafeGetInt(reader, "High"),
                                Low = SafeGetInt(reader, "Low"),
                                Close = SafeGetInt(reader, "Close"),
                                Volume = SafeGetLong(reader, "Volume"),
                                ChangeAmount = SafeGetInt(reader, "ChangeAmount"),
                                ChangeRate = (decimal)SafeGetDouble(reader, "ChangeRate"),
                                TradingValue = SafeGetLong(reader, "TradingValue")
                            };
                            results.Add(minutePrice);
                        }
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ 분봉 데이터 조회 실패: {ex.Message}");
                return new List<Models.MinutePrice>();
            }
        }

        /// <summary>
        /// 분봉 데이터 존재 여부 확인
        /// </summary>
        public async Task<bool> CheckMinuteHistoricalDataExistsAsync(string stockCode, int minuteInterval, int requiredDays = 3)
        {
            try
            {
                string query = @"
                    SELECT COUNT(*) 
                    FROM MinutePriceHistory 
                    WHERE StockCode = @StockCode AND MinuteInterval = @MinuteInterval;";

                using (var command = new SQLiteCommand(query, _connection))
                {
                    command.Parameters.AddWithValue("@StockCode", stockCode ?? "");
                    command.Parameters.AddWithValue("@MinuteInterval", minuteInterval);

                    var count = await Task.Run(() => command.ExecuteScalar());
                    int existingCount = Convert.ToInt32(count);

                    // 일당 분봉 개수 계산
                    int minutesPerDay = (int)(6.5 * 60 / minuteInterval); // 예: 1분봉이면 390개/일
                    int requiredCount = requiredDays * minutesPerDay;

                    System.Diagnostics.Debug.WriteLine($"📊 {stockCode} {minuteInterval}분봉: {existingCount}개 (필요: {requiredCount}개)");

                    return existingCount >= requiredCount;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ 분봉 데이터 존재 확인 실패: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region 공통 테이블 생성

        /// <summary>
        /// 모든 테이블 생성 (일봉 + 분봉)
        /// </summary>
        public async Task CreateAllTablesAsync()
        {
            try
            {
                await CreateDailyPriceHistoryTableAsync();
                await CreateMinutePriceHistoryTableAsync();
                System.Diagnostics.Debug.WriteLine("✅ 모든 테이블 생성 완료 (일봉 + 분봉)");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ 테이블 생성 실패: {ex.Message}");
            }
        }

        #endregion

        #region 안전한 데이터 읽기 유틸리티

        /// <summary>
        /// 안전한 문자열 읽기
        /// </summary>
        private string SafeGetString(SQLiteDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? "" : reader.GetString(ordinal);
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 안전한 정수 읽기
        /// </summary>
        private int SafeGetInt(SQLiteDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                if (reader.IsDBNull(ordinal)) return 0;

                long value = reader.GetInt64(ordinal);
                if (value > int.MaxValue) return int.MaxValue;
                if (value < int.MinValue) return int.MinValue;

                return (int)value;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 안전한 긴 정수 읽기
        /// </summary>
        private long SafeGetLong(SQLiteDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? 0 : reader.GetInt64(ordinal);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 안전한 실수 읽기
        /// </summary>
        private double SafeGetDouble(SQLiteDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? 0.0 : reader.GetDouble(ordinal);
            }
            catch
            {
                return 0.0;
            }
        }

        #endregion

        #region IDisposable 구현

        public void Dispose()
        {
            if (!_disposed)
            {
                _connection?.Close();
                _connection?.Dispose();
                _disposed = true;
            }
        }

        #endregion
    }
}