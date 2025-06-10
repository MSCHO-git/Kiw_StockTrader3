using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Data.SQLite;
using StockTrader3.Models;
using StockTrader3_WinForms;

namespace StockTrader3_WinForms.Modules
{
    /// <summary>
    /// 과거 데이터 수집 전문 모듈
    /// KiwoomApiManager의 기존 연결을 재사용하여 과거 데이터 수집
    /// </summary>
    
    public class HistoricalDataModule : IDisposable  // ✅ IDisposable 추가
    {
        #region 이벤트 및 진행률
        public event Action<string> StatusUpdated;
        public event Action<int, int, string> ProgressUpdated; // (current, total, stockCode)
        #endregion

        #region 필드
        private readonly KiwoomApiManager _kiwoomManager;
        private readonly string _connectionString;
        private SQLiteConnection _connection;
        #endregion

        #region 생성자
        /// <summary>
        /// 기존 KiwoomApiManager를 받아서 연결 재사용
        /// </summary>
        /// <param name="kiwoomManager">이미 연결된 키움 매니저</param>
        /// <param name="connectionString">DB 연결 문자열</param>
        public HistoricalDataModule(KiwoomApiManager kiwoomManager, string connectionString)
        {
            _kiwoomManager = kiwoomManager ?? throw new ArgumentNullException(nameof(kiwoomManager));
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

            InitializeDatabase();
        }
        #endregion

        #region 데이터베이스 초기화
        private async void InitializeDatabase()
        {
            try
            {
                _connection = new SQLiteConnection(_connectionString);
                await Task.Run(() => _connection.Open());
                await CreateHistoricalDataTablesAsync();
                UpdateStatus("HistoricalDataModule 초기화 완료");
            }
            catch (Exception ex)
            {
                UpdateStatus($"HistoricalDataModule 초기화 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 과거 데이터용 테이블 생성
        /// </summary>
        private async Task CreateHistoricalDataTablesAsync()
        {
            try
            {
                // 일봉 테이블
                string dailyTableQuery = @"
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

                // 분봉 테이블
                string minuteTableQuery = @"
                    CREATE TABLE IF NOT EXISTS MinutePriceHistory (
                        StockCode           TEXT NOT NULL,
                        DateTime            TEXT NOT NULL,
                        MinuteInterval      INTEGER NOT NULL,
                        Open                INTEGER NOT NULL,
                        High                INTEGER NOT NULL,
                        Low                 INTEGER NOT NULL,
                        Close               INTEGER NOT NULL,
                        Volume              INTEGER NOT NULL,
                        ChangeAmount        INTEGER DEFAULT 0,
                        ChangeRate          REAL DEFAULT 0,
                        TradingValue        INTEGER DEFAULT 0,
                        CreatedAt           DATETIME DEFAULT CURRENT_TIMESTAMP,
                        PRIMARY KEY (StockCode, DateTime, MinuteInterval)
                    );";

                using (var command = new SQLiteCommand(dailyTableQuery, _connection))
                {
                    await Task.Run(() => command.ExecuteNonQuery());
                }

                using (var command = new SQLiteCommand(minuteTableQuery, _connection))
                {
                    await Task.Run(() => command.ExecuteNonQuery());
                }

                UpdateStatus("과거 데이터 테이블 생성 완료");
            }
            catch (Exception ex)
            {
                UpdateStatus($"테이블 생성 실패: {ex.Message}");
            }
        }
        #endregion

        #region 🎯 핵심 메서드 - 과거 데이터 수집

        /// <summary>
        /// 🚀 과거 데이터 수집 실행 (KiwoomApiManager 연결 재사용)
        /// </summary>
        /// <param name="stockCodes">수집할 종목 코드들</param>
        /// <param name="cancellationToken">취소 토큰</param>
        /// <param name="progressCallback">진행률 콜백</param>
        public async Task CollectHistoricalDataAsync(List<string> stockCodes,
            CancellationToken cancellationToken, Action<int, int, string> progressCallback = null)
        {
            try
            {
                UpdateStatus($"과거 데이터 수집 시작: {stockCodes.Count}개 종목");

                // 🎯 핵심: 기존 KiwoomApiManager 연결 상태 확인
                if (!_kiwoomManager.IsConnected)
                {
                    throw new Exception("KiwoomApiManager가 연결되지 않았습니다. 먼저 키움 API에 연결해주세요.");
                }

                UpdateStatus("기존 키움 연결 재사용 - 과거 데이터 수집 시작");

                // 각 종목별 데이터 수집
                for (int i = 0; i < stockCodes.Count; i++)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    string stockCode = stockCodes[i];
                    progressCallback?.Invoke(i + 1, stockCodes.Count, stockCode);

                    try
                    {
                        // 1단계: 일봉 데이터 체크 및 수집
                        await ProcessDailyDataForStock(stockCode, cancellationToken);

                        // 2단계: 분봉 데이터 체크 및 수집  
                        await ProcessMinuteDataForStock(stockCode, cancellationToken);

                        UpdateStatus($"{stockCode} 과거 데이터 처리 완료");

                        // API 안정성을 위한 대기 (기존 KiwoomApiManager와 충돌 방지)
                        if (i < stockCodes.Count - 1)
                        {
                            await Task.Delay(2000, cancellationToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        UpdateStatus($"{stockCode} 데이터 수집 실패: {ex.Message}");
                        // 개별 종목 실패해도 계속 진행
                    }
                }

                UpdateStatus($"과거 데이터 수집 완료: {stockCodes.Count}개 종목 처리");
            }
            catch (Exception ex)
            {
                UpdateStatus($"과거 데이터 수집 실패: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 특정 종목의 일봉 데이터 처리
        /// </summary>
        private async Task ProcessDailyDataForStock(string stockCode, CancellationToken cancellationToken)
        {
            try
            {
                // 기존 일봉 데이터 확인
                bool needsDailyData = !await CheckDailyDataExistsAsync(stockCode, 60);

                if (!needsDailyData)
                {
                    UpdateStatus($"{stockCode} 일봉 데이터 충분 - 스킵");
                    return;
                }

                UpdateStatus($"{stockCode} 일봉 데이터 수집 중...");

                // 🎯 기존 KiwoomApiManager 사용해서 일봉 데이터 수집
                var dailyData = await _kiwoomManager.GetHistoricalDataViaOPT10081Async(stockCode, 60);

                if (dailyData?.Count > 0)
                {
                    await SaveDailyDataAsync(stockCode, dailyData);
                    UpdateStatus($"{stockCode} 일봉 {dailyData.Count}일 저장 완료");
                }
                else
                {
                    UpdateStatus($"{stockCode} 일봉 데이터 수집 실패");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"{stockCode} 일봉 처리 실패: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 특정 종목의 분봉 데이터 처리
        /// </summary>
        private async Task ProcessMinuteDataForStock(string stockCode, CancellationToken cancellationToken)
        {
            try
            {
                // 기존 분봉 데이터 확인
                bool needsMinuteData = !await CheckMinuteDataExistsAsync(stockCode, 1, 3);

                if (!needsMinuteData)
                {
                    UpdateStatus($"{stockCode} 1분봉 데이터 충분 - 스킵");
                    return;
                }

                UpdateStatus($"{stockCode} 1분봉 데이터 수집 중...");

                // 🎯 기존 KiwoomApiManager 사용해서 분봉 데이터 수집
                var minuteData = await _kiwoomManager.GetMinuteDataViaOPT10080Async(stockCode, 1, 3);

                if (minuteData?.Count > 0)
                {
                    await SaveMinuteDataAsync(stockCode, 1, minuteData);
                    UpdateStatus($"{stockCode} 1분봉 {minuteData.Count}개 저장 완료");
                }
                else
                {
                    UpdateStatus($"{stockCode} 1분봉 데이터 수집 실패");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"{stockCode} 분봉 처리 실패: {ex.Message}");
                // 분봉 실패해도 치명적이지 않으므로 계속 진행
            }
        }

        #endregion

        #region 데이터 존재 여부 확인

        /// <summary>
        /// 일봉 데이터 존재 여부 확인
        /// </summary>
        public async Task<bool> CheckDailyDataExistsAsync(string stockCode, int requiredDays = 60)
        {
            try
            {
                string query = @"
                    SELECT COUNT(*) 
                    FROM DailyPriceHistory 
                    WHERE StockCode = @StockCode;";

                using (var command = new SQLiteCommand(query, _connection))
                {
                    command.Parameters.AddWithValue("@StockCode", stockCode);
                    var count = await Task.Run(() => command.ExecuteScalar());
                    int existingDays = Convert.ToInt32(count);

                    return existingDays >= requiredDays;
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"일봉 데이터 체크 실패 ({stockCode}): {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 분봉 데이터 존재 여부 확인
        /// </summary>
        public async Task<bool> CheckMinuteDataExistsAsync(string stockCode, int minuteInterval, int requiredDays = 3)
        {
            try
            {
                string query = @"
                    SELECT COUNT(*) 
                    FROM MinutePriceHistory 
                    WHERE StockCode = @StockCode AND MinuteInterval = @MinuteInterval;";

                using (var command = new SQLiteCommand(query, _connection))
                {
                    command.Parameters.AddWithValue("@StockCode", stockCode);
                    command.Parameters.AddWithValue("@MinuteInterval", minuteInterval);
                    var count = await Task.Run(() => command.ExecuteScalar());
                    int existingCount = Convert.ToInt32(count);

                    // 일당 분봉 개수 계산
                    int minutesPerDay = (int)(6.5 * 60 / minuteInterval);
                    int requiredCount = requiredDays * minutesPerDay;

                    return existingCount >= requiredCount;
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"분봉 데이터 체크 실패 ({stockCode}): {ex.Message}");
                return false;
            }
        }

        #endregion

        #region 데이터 저장

        /// <summary>
        /// 일봉 데이터 저장
        /// </summary>
        private async Task<int> SaveDailyDataAsync(string stockCode, List<DailyPrice> dailyPrices)
        {
            try
            {
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
                                command.Parameters.AddWithValue("@StockCode", stockCode);
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
                UpdateStatus($"일봉 데이터 저장 실패: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// 분봉 데이터 저장
        /// </summary>
        private async Task<int> SaveMinuteDataAsync(string stockCode, int minuteInterval, List<MinutePrice> minutePrices)
        {
            try
            {
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
                                command.Parameters.AddWithValue("@StockCode", stockCode);
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
                UpdateStatus($"분봉 데이터 저장 실패: {ex.Message}");
                return 0;
            }
        }

        #endregion

        #region 유틸리티 메서드

        private void UpdateStatus(string status)
        {
            StatusUpdated?.Invoke(status);
            System.Diagnostics.Debug.WriteLine($"[HistoricalDataModule] {status}");
        }

        #endregion

        #region 리소스 정리

        public void Dispose()
        {
            try
            {
                _connection?.Close();
                _connection?.Dispose();
                UpdateStatus("HistoricalDataModule 리소스 정리 완료");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HistoricalDataModule 리소스 정리 실패: {ex.Message}");
            }
        }

        #endregion
    }
}