using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;

namespace DataCollector.Managers
{
    /// <summary>
    /// StockTrader3.db에서 종목 코드 읽기 전용 클래스
    /// </summary>
    public class StockDataReader : IDisposable
    {
        #region 필드 및 생성자

        private readonly string _stockTrader3DbPath;
        private readonly string _connectionString;
        private SQLiteConnection _connection;
        private bool _disposed = false;

        public StockDataReader()
        {
            try
            {
                // StockTrader3.db 경로 설정 (실제 StockTrader3 프로젝트의 실행 폴더)
                string currentDir = Environment.CurrentDirectory; // DataCollector\bin\x86\Debug
                string stockTrader3Dir = Path.GetFullPath(Path.Combine(currentDir, @"..\..\..\..\StockTrader3_WinForms\bin\x86\Debug"));
                _stockTrader3DbPath = Path.Combine(stockTrader3Dir, "StockTrader3.db");
                _connectionString = $"Data Source={_stockTrader3DbPath};Version=3;ReadOnly=True;";

                System.Diagnostics.Debug.WriteLine($"현재 디렉토리: {currentDir}");
                System.Diagnostics.Debug.WriteLine($"StockTrader3 디렉토리: {stockTrader3Dir}");
                System.Diagnostics.Debug.WriteLine($"StockTrader3 DB 경로: {_stockTrader3DbPath}");
                System.Diagnostics.Debug.WriteLine($"파일 존재 여부: {File.Exists(_stockTrader3DbPath)}");
            }
            catch (Exception ex)
            {
                throw new Exception($"StockDataReader 초기화 실패: {ex.Message}");
            }
        }

        #endregion

        #region 데이터베이스 연결

        /// <summary>
        /// StockTrader3.db 연결 (읽기 전용)
        /// </summary>
        public async Task<bool> ConnectAsync()
        {
            try
            {
                // 파일 존재 여부 확인
                if (!File.Exists(_stockTrader3DbPath))
                {
                    System.Diagnostics.Debug.WriteLine($"❌ StockTrader3.db 파일이 없습니다: {_stockTrader3DbPath}");
                    return false;
                }

                _connection = new SQLiteConnection(_connectionString);
                await Task.Run(() => _connection.Open());

                System.Diagnostics.Debug.WriteLine("✅ StockTrader3.db 연결 성공 (읽기 전용)");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ StockTrader3.db 연결 실패: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 연결 상태 확인
        /// </summary>
        public bool IsConnected => _connection?.State == ConnectionState.Open;

        #endregion

        #region 종목 코드 조회

        /// <summary>
        /// StockTrader3.db에서 첫 번째 종목 코드 반환
        /// </summary>
        public async Task<string> GetFirstStockCodeAsync()
        {
            try
            {
                if (!IsConnected)
                {
                    bool connected = await ConnectAsync();
                    if (!connected)
                    {
                        System.Diagnostics.Debug.WriteLine("❌ DB 연결 실패로 종목 코드 조회 불가");
                        return null;
                    }
                }

                // 가장 최근 날짜의 첫 번째 종목 코드 조회
                string query = @"
                    SELECT StockCode 
                    FROM StockAnalysis 
                    WHERE SearchDate = (SELECT MAX(SearchDate) FROM StockAnalysis)
                    ORDER BY StockCode 
                    LIMIT 1;";

                using (var command = new SQLiteCommand(query, _connection))
                {
                    var result = await Task.Run(() => command.ExecuteScalar());

                    if (result != null)
                    {
                        string stockCode = result.ToString();
                        System.Diagnostics.Debug.WriteLine($"✅ 첫 번째 종목 코드 조회 성공: {stockCode}");
                        return stockCode;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("❌ 조회된 종목 코드가 없습니다");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ 첫 번째 종목 코드 조회 실패: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 향후 확장용: 모든 종목 코드 반환
        /// </summary>
        public async Task<List<string>> GetAllStockCodesAsync()
        {
            try
            {
                var stockCodes = new List<string>();

                if (!IsConnected)
                {
                    bool connected = await ConnectAsync();
                    if (!connected) return stockCodes;
                }

                // 가장 최근 날짜의 모든 종목 코드 조회
                string query = @"
                    SELECT DISTINCT StockCode 
                    FROM StockAnalysis 
                    WHERE SearchDate = (SELECT MAX(SearchDate) FROM StockAnalysis)
                    ORDER BY StockCode;";

                using (var command = new SQLiteCommand(query, _connection))
                {
                    using (var reader = await Task.Run(() => command.ExecuteReader()))
                    {
                        while (await Task.Run(() => reader.Read()))
                        {
                            if (!reader.IsDBNull(0))
                            {
                                string stockCode = reader.GetString(0);
                                if (!string.IsNullOrEmpty(stockCode))
                                {
                                    stockCodes.Add(stockCode);
                                }
                            }
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"✅ 전체 종목 코드 {stockCodes.Count}개 조회 성공");
                return stockCodes;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ 전체 종목 코드 조회 실패: {ex.Message}");
                return new List<string>();
            }
        }

        /// <summary>
        /// 특정 날짜의 조건검색 결과 종목들 반환
        /// </summary>
        public async Task<List<string>> GetStockCodesByDateAsync(DateTime searchDate)
        {
            try
            {
                var stockCodes = new List<string>();

                if (!IsConnected)
                {
                    bool connected = await ConnectAsync();
                    if (!connected) return stockCodes;
                }

                string query = @"
                    SELECT DISTINCT StockCode 
                    FROM StockAnalysis 
                    WHERE SearchDate = @SearchDate
                    ORDER BY StockCode;";

                using (var command = new SQLiteCommand(query, _connection))
                {
                    command.Parameters.AddWithValue("@SearchDate", searchDate.Date);

                    using (var reader = await Task.Run(() => command.ExecuteReader()))
                    {
                        while (await Task.Run(() => reader.Read()))
                        {
                            if (!reader.IsDBNull(0))
                            {
                                string stockCode = reader.GetString(0);
                                if (!string.IsNullOrEmpty(stockCode))
                                {
                                    stockCodes.Add(stockCode);
                                }
                            }
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"✅ {searchDate:yyyy-MM-dd} 종목 코드 {stockCodes.Count}개 조회 성공");
                return stockCodes;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ 특정 날짜 종목 코드 조회 실패: {ex.Message}");
                return new List<string>();
            }
        }

        #endregion

        #region 디버그 및 상태 확인

        /// <summary>
        /// StockTrader3.db 상태 확인 (디버그용)
        /// </summary>
        public async Task<DatabaseStatus> CheckDatabaseStatusAsync()
        {
            try
            {
                var status = new DatabaseStatus
                {
                    FileExists = File.Exists(_stockTrader3DbPath),
                    FilePath = _stockTrader3DbPath
                };

                if (!status.FileExists)
                {
                    return status;
                }

                if (!IsConnected)
                {
                    await ConnectAsync();
                }

                if (IsConnected)
                {
                    // 테이블 존재 여부 확인
                    string tableQuery = "SELECT name FROM sqlite_master WHERE type='table' AND name='StockAnalysis';";
                    using (var command = new SQLiteCommand(tableQuery, _connection))
                    {
                        var result = await Task.Run(() => command.ExecuteScalar());
                        status.HasStockAnalysisTable = result != null;
                    }

                    // 데이터 개수 확인
                    if (status.HasStockAnalysisTable)
                    {
                        string countQuery = "SELECT COUNT(*) FROM StockAnalysis;";
                        using (var command = new SQLiteCommand(countQuery, _connection))
                        {
                            var result = await Task.Run(() => command.ExecuteScalar());
                            status.TotalRecords = Convert.ToInt32(result);
                        }

                        // 최근 날짜 확인
                        string dateQuery = "SELECT MAX(SearchDate) FROM StockAnalysis;";
                        using (var command = new SQLiteCommand(dateQuery, _connection))
                        {
                            var result = await Task.Run(() => command.ExecuteScalar());
                            if (result != null && result != DBNull.Value)
                            {
                                if (DateTime.TryParse(result.ToString(), out DateTime parsedDate))
                                {
                                    status.LatestSearchDate = parsedDate;
                                }
                            }
                        }
                    }
                }

                return status;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ DB 상태 확인 실패: {ex.Message}");
                return new DatabaseStatus
                {
                    FileExists = File.Exists(_stockTrader3DbPath),
                    FilePath = _stockTrader3DbPath,
                    ErrorMessage = ex.Message
                };
            }
        }

        #endregion

        #region IDisposable 구현

        public void Dispose()
        {
            if (!_disposed)
            {
                try
                {
                    _connection?.Close();
                    _connection?.Dispose();
                    System.Diagnostics.Debug.WriteLine("✅ StockDataReader 리소스 정리 완료");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ StockDataReader 리소스 정리 실패: {ex.Message}");
                }
                _disposed = true;
            }
        }

        #endregion
    }

    #region 지원 클래스

    /// <summary>
    /// 데이터베이스 상태 정보
    /// </summary>
    public class DatabaseStatus
    {
        public bool FileExists { get; set; }
        public string FilePath { get; set; }
        public bool HasStockAnalysisTable { get; set; }
        public int TotalRecords { get; set; }
        public DateTime? LatestSearchDate { get; set; }
        public string ErrorMessage { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
                return $"에러: {ErrorMessage}";

            if (!FileExists)
                return "StockTrader3.db 파일 없음";

            if (!HasStockAnalysisTable)
                return "StockAnalysis 테이블 없음";

            return $"정상 - 총 {TotalRecords}개 레코드, 최근 날짜: {LatestSearchDate:yyyy-MM-dd}";
        }
    }

    #endregion
}