using StockTrader3.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;

namespace StockTrader3_WinForms
{
    /// <summary>
    /// 데이터베이스 관리 및 모든 DB 관련 작업을 담당하는 클래스 (타입 오류 완전 수정)
    /// </summary>
    public class DatabaseManager : IDisposable
    {
        #region 필드 및 생성자

        private readonly string _connectionString;
        private readonly string _databasePath;
        private SQLiteConnection _connection;
        private bool _disposed = false;

        /// <summary>
        /// 데이터베이스 연결 상태 변경 이벤트
        /// </summary>
        public event Action<bool, string> ConnectionStatusChanged;

        /// <summary>
        /// DB 작업 진행 상황 이벤트
        /// </summary>
        public event Action<string, int, int> ProgressUpdated;

        public DatabaseManager(string databasePath = null)
        {
            try
            {
                // 기본 경로 설정 (실행 파일과 같은 폴더)
                _databasePath = databasePath ?? Path.Combine(
                    Environment.CurrentDirectory, "StockTrader3.db");

                _connectionString = $"Data Source={_databasePath};Version=3;";

                System.Diagnostics.Debug.WriteLine($"데이터베이스 경로: {_databasePath}");
            }
            catch (Exception ex)
            {
                throw new Exception($"DatabaseManager 초기화 실패: {ex.Message}");
            }
        }

        #endregion

        #region 데이터베이스 연결 관리

        /// <summary>
        /// 데이터베이스 연결 및 테이블 생성
        /// </summary>
        public async Task<bool> InitializeAsync()
        {
            try
            {
                ConnectionStatusChanged?.Invoke(false, "데이터베이스 초기화 중...");

                // 데이터베이스 파일이 없으면 생성
                bool isNewDatabase = !File.Exists(_databasePath);

                _connection = new SQLiteConnection(_connectionString);
                await Task.Run(() => _connection.Open());

                if (isNewDatabase)
                {
                    ConnectionStatusChanged?.Invoke(false, "새 데이터베이스 생성 중...");
                    await CreateTablesAsync();
                }
                else
                {
                    ConnectionStatusChanged?.Invoke(false, "기존 데이터베이스 연결 중...");
                    await VerifyTablesAsync();
                }

                ConnectionStatusChanged?.Invoke(true, "데이터베이스 연결 완료");
                System.Diagnostics.Debug.WriteLine("✅ 데이터베이스 초기화 완료");
                return true;
            }
            catch (Exception ex)
            {
                ConnectionStatusChanged?.Invoke(false, $"데이터베이스 초기화 실패: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ 데이터베이스 초기화 실패: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 데이터베이스 연결 상태 확인
        /// </summary>
        public bool IsConnected => _connection?.State == ConnectionState.Open;

        #endregion

        #region 테이블 생성

        /// <summary>
        /// 모든 테이블 생성 (🔧 문법 오류 수정)
        /// </summary>
        /// <summary>
        /// 모든 테이블 생성 (🔧 뉴스 테이블 추가)
        /// </summary>
        private async Task CreateTablesAsync()
        {
            try
            {
                var tableQueries = new[]
                {
            CreateStockAnalysisTableQuery(),
            CreateConditionSearchHistoryTableQuery(),
            CreateAnalysisLogTableQuery(),
            CreateUserSettingsTableQuery(),
            CreateDailyPriceHistoryTableQuery(),
            CreateMinutePriceHistoryTableQuery(),
            CreateNewsTableQuery()  // 🆕 이 줄 추가
        };

                for (int i = 0; i < tableQueries.Length; i++)
                {
                    ProgressUpdated?.Invoke("테이블 생성", i + 1, tableQueries.Length);

                    using (var command = new SQLiteCommand(tableQueries[i], _connection))
                    {
                        await Task.Run(() => command.ExecuteNonQuery());
                    }
                }

                System.Diagnostics.Debug.WriteLine("✅ 모든 테이블 생성 완료 (일봉 + 분봉 + 뉴스)");
            }
            catch (Exception ex)
            {
                throw new Exception($"테이블 생성 실패: {ex.Message}");
            }
        }


        /// <summary>
        /// 🆕 뉴스 테이블 생성 쿼리
        /// </summary>

        /// <summary>
        /// 🆕 뉴스 테이블 생성 쿼리 (문법 오류 수정)
        /// </summary>
        private string CreateNewsTableQuery()
        {
            return @"
        CREATE TABLE IF NOT EXISTS News (
            Id                  INTEGER PRIMARY KEY AUTOINCREMENT,
            StockCode           TEXT NOT NULL,
            StockName           TEXT NOT NULL,
            Title               TEXT NOT NULL,
            Content             TEXT,
            Source              TEXT,
            Url                 TEXT,
            PublishDate         DATETIME NOT NULL,
            CollectedDate       DATETIME DEFAULT CURRENT_TIMESTAMP,
            SentimentScore      REAL DEFAULT 0,
            ImportanceScore     REAL DEFAULT 0,
            IsRelevant          BOOLEAN DEFAULT 1,
            
            -- 분석 결과와 연결
            AnalysisDate        DATE NOT NULL,
            ConditionIndex      INTEGER NOT NULL
        );
        
        CREATE INDEX IF NOT EXISTS idx_news_stock_date ON News(StockCode, AnalysisDate);
        CREATE INDEX IF NOT EXISTS idx_news_publish_date ON News(PublishDate);";
        }

    
        /// <summary>
        /// 메인 분석 테이블 생성 쿼리 (설계 명세서 기준 60개 필드)
        /// </summary>
        private string CreateStockAnalysisTableQuery()
        {
            return @"
                CREATE TABLE IF NOT EXISTS StockAnalysis (
                    -- 🔑 기본 키 및 식별 정보 (4개)
                    SearchDate           DATE NOT NULL,
                    StockCode           TEXT NOT NULL,
                    ConditionIndex      INTEGER NOT NULL,
                    ConditionName       TEXT,
                    
                    -- 📈 기본 종목 정보 (4개)
                    StockName           TEXT,
                    MarketType          TEXT,
                    Sector              TEXT,
                    MarketCap           INTEGER,
                    
                    -- 💰 가격 및 거래 정보 (10개)
                    ClosePrice          INTEGER,
                    OpenPrice           INTEGER,
                    HighPrice           INTEGER,
                    LowPrice            INTEGER,
                    Volume              INTEGER,
                    ChangeAmount        INTEGER,
                    ChangeRate          REAL,
                    Week52High          INTEGER,
                    Week52Low           INTEGER,
                    VolumeRatio         REAL,
                    
                    -- 🔄 진행 상태 관리 (3개)
                    ProcessStatus       TEXT,
                    LastUpdatedStep     TEXT,
                    AnalysisProgress    INTEGER,
                    
                    -- 📊 기본 재무 정보 (6개)
                    PER                 REAL,
                    PBR                 REAL,
                    EPS                 INTEGER,
                    BPS                 INTEGER,
                    ForeignOwnership    REAL,
                    InstitutionOwnership REAL,
                    
                    -- 🔧 1차 분석 (기술적 분석) 결과 (11개)
                    TechnicalScore      INTEGER,
                    RSI                 REAL,
                    MACD                REAL,
                    MACDSignal          REAL,
                    BollingerUpper      INTEGER,
                    BollingerLower      INTEGER,
                    BollingerPosition   REAL,
                    MA5                 INTEGER,
                    MA20                INTEGER,
                    MA60                INTEGER,
                    TechnicalGrade      TEXT,
                    
                    -- 📰 2차 분석 (뉴스 분석) 결과 (7개)
                    NewsScore           INTEGER,
                    PositiveNewsCount   INTEGER,
                    NegativeNewsCount   INTEGER,
                    NeutralNewsCount    INTEGER,
                    SentimentScore      REAL,
                    KeywordRelevance    REAL,
                    NewsImpactLevel     TEXT,
                    
                    -- 🎯 최종 결과 및 매매 계획 (8개)
                    FinalScore          INTEGER,
                    FinalGrade          TEXT,
                    BuyPrice            INTEGER,
                    SellPrice           INTEGER,
                    StopLossPrice       INTEGER,
                    ExpectedReturn      REAL,
                    RiskLevel           TEXT,
                    Recommendation      TEXT,
                    
                    -- ⏰ 시간 추적 (7개)
                    ConditionSearchAt   DATETIME,
                    TechnicalAnalysisAt DATETIME,
                    NewsAnalysisAt      DATETIME,
                    TradePlanAt         DATETIME,
                    TradingValueRank    INTEGER,
                    CreatedAt           DATETIME DEFAULT CURRENT_TIMESTAMP,
                    UpdatedAt           DATETIME DEFAULT CURRENT_TIMESTAMP,
                    
                    PRIMARY KEY (SearchDate, StockCode, ConditionIndex)
                );";
        }

        /// <summary>
        /// 과거 데이터 테이블 생성 쿼리
        /// </summary>
        private string CreateDailyPriceHistoryTableQuery()
        {
            return @"
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
        }

        /// <summary>
        /// 분봉 데이터 테이블 생성 쿼리 (🆕 새로 추가)
        /// </summary>
        private string CreateMinutePriceHistoryTableQuery()
        {
            return @"
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
        }

        /// <summary>
        /// 조건검색 이력 테이블 생성 쿼리
        /// </summary>
        private string CreateConditionSearchHistoryTableQuery()
        {
            return @"
                CREATE TABLE IF NOT EXISTS ConditionSearchHistory (
                    Id                  INTEGER PRIMARY KEY AUTOINCREMENT,
                    SearchDate          DATE NOT NULL,
                    ConditionIndex      INTEGER NOT NULL,
                    ConditionName       TEXT NOT NULL,
                    SearchTime          DATETIME NOT NULL,
                    ResultCount         INTEGER NOT NULL,
                    ExecutionTimeMs     INTEGER,
                    Success             BOOLEAN NOT NULL,
                    ErrorMessage        TEXT,
                    CreatedAt           DATETIME DEFAULT CURRENT_TIMESTAMP
                );";
        }

        /// <summary>
        /// 분석 로그 테이블 생성 쿼리
        /// </summary>
        private string CreateAnalysisLogTableQuery()
        {
            return @"
                CREATE TABLE IF NOT EXISTS AnalysisLog (
                    Id                  INTEGER PRIMARY KEY AUTOINCREMENT,
                    LogDate             DATE NOT NULL,
                    LogTime             DATETIME NOT NULL,
                    LogLevel            TEXT NOT NULL,  -- INFO, WARNING, ERROR
                    Category            TEXT NOT NULL,  -- CONDITION_SEARCH, TECHNICAL_ANALYSIS, NEWS_ANALYSIS, TRADE_PLAN
                    StockCode           TEXT,
                    Message             TEXT NOT NULL,
                    Details             TEXT,
                    ExecutionTimeMs     INTEGER,
                    CreatedAt           DATETIME DEFAULT CURRENT_TIMESTAMP
                );";
        }

        /// <summary>
        /// 사용자 설정 테이블 생성 쿼리
        /// </summary>
        private string CreateUserSettingsTableQuery()
        {
            return @"
                CREATE TABLE IF NOT EXISTS UserSettings (
                    SettingKey          TEXT PRIMARY KEY,
                    SettingValue        TEXT NOT NULL,
                    SettingType         TEXT NOT NULL,  -- STRING, INTEGER, BOOLEAN, DECIMAL
                    Description         TEXT,
                    UpdatedAt           DATETIME DEFAULT CURRENT_TIMESTAMP
                );";
        }

        /// <summary>
        /// 테이블 존재 여부 확인
        /// </summary>
        private async Task VerifyTablesAsync()
        {
            try
            {
                var requiredTables = new[]
                {
                    "StockAnalysis",
                    "ConditionSearchHistory",
                    "AnalysisLog",
                    "UserSettings",
                    "DailyPriceHistory",      // ✅ 추가
                    "MinutePriceHistory",      // ✅ 추가
                    "News"  // 🆕 이 줄 추가
                };

                foreach (var tableName in requiredTables)
                {
                    string query = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;";
                    using (var command = new SQLiteCommand(query, _connection))
                    {
                        command.Parameters.AddWithValue("@tableName", tableName);
                        var result = await Task.Run(() => command.ExecuteScalar());

                        if (result == null)
                        {
                            System.Diagnostics.Debug.WriteLine($"⚠️ 테이블이 없어서 생성 필요: {tableName}");
                            // 테이블이 없으면 다시 생성
                            await CreateTablesAsync();
                            break;
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine("✅ 모든 테이블 검증 완료");
            }
            catch (Exception ex)
            {
                throw new Exception($"테이블 검증 실패: {ex.Message}");
            }
        }

        #endregion

        #region 조건검색 결과 CRUD (타입 오류 완전 수정)

        /// <summary>
        /// 조건검색 결과 저장 (타입 오류 완전 해결)
        /// </summary>
        public async Task<int> SaveConditionSearchResultsAsync(DateTime searchDate, int conditionIndex,
            string conditionName, List<StockBasicInfo> stocks)
        {
            try
            {
                if (stocks == null || stocks.Count == 0)
                    return 0;

                ProgressUpdated?.Invoke("조건검색 결과 저장", 0, stocks.Count);

                using (var transaction = _connection.BeginTransaction())
                {
                    try
                    {
                        // 기존 데이터 삭제 (같은 날짜, 같은 조건)
                        await DeleteExistingConditionResultsAsync(searchDate, conditionIndex, transaction);

                        // ✅ 새 데이터 삽입 - 전일대비 필드 추가
                        string insertQuery = @"
                            INSERT INTO StockAnalysis (
                                SearchDate, StockCode, ConditionIndex, ConditionName,
                                StockName, MarketType, ClosePrice, ChangeAmount, OpenPrice, HighPrice, LowPrice, 
                                Volume, ChangeRate, ProcessStatus, AnalysisProgress, CreatedAt, UpdatedAt
                            ) VALUES (
                                @SearchDate, @StockCode, @ConditionIndex, @ConditionName,
                                @StockName, @MarketType, @ClosePrice, @ChangeAmount, @OpenPrice, @HighPrice, @LowPrice,
                                @Volume, @ChangeRate, 'CONDITION_SEARCH', 25, @CreatedAt, @UpdatedAt
                            );";

                        int insertedCount = 0;
                        foreach (var stock in stocks)
                        {
                            using (var command = new SQLiteCommand(insertQuery, _connection, transaction))
                            {
                                // 기존 파라미터들
                                command.Parameters.Add("@SearchDate", DbType.Date).Value = searchDate.Date;
                                command.Parameters.Add("@StockCode", DbType.String).Value = SafeString(stock.StockCode);
                                command.Parameters.Add("@ConditionIndex", DbType.Int64).Value = SafeInt64(conditionIndex);
                                command.Parameters.Add("@ConditionName", DbType.String).Value = SafeString(conditionName);
                                command.Parameters.Add("@StockName", DbType.String).Value = SafeString(stock.StockName);
                                command.Parameters.Add("@MarketType", DbType.String).Value = SafeString(stock.MarketType);
                                command.Parameters.Add("@ClosePrice", DbType.Int64).Value = SafeInt64(stock.ClosePrice);

                                // ✅ 전일대비와 시가/고가/저가 파라미터 추가
                                command.Parameters.Add("@ChangeAmount", DbType.Int64).Value = SafeInt64(stock.ChangeAmount);
                                command.Parameters.Add("@OpenPrice", DbType.Int64).Value = SafeInt64(stock.OpenPrice);
                                command.Parameters.Add("@HighPrice", DbType.Int64).Value = SafeInt64(stock.HighPrice);
                                command.Parameters.Add("@LowPrice", DbType.Int64).Value = SafeInt64(stock.LowPrice);

                                command.Parameters.Add("@Volume", DbType.Int64).Value = SafeInt64(stock.Volume);
                                command.Parameters.Add("@ChangeRate", DbType.Double).Value = SafeDouble(stock.ChangeRate);
                                command.Parameters.Add("@CreatedAt", DbType.DateTime).Value = DateTime.Now;
                                command.Parameters.Add("@UpdatedAt", DbType.DateTime).Value = DateTime.Now;

                                // ✅ 저장 전 디버그 출력 (전일대비 포함)
                                System.Diagnostics.Debug.WriteLine($"💾 DB 저장: {stock.StockCode} - 현재가:{stock.ClosePrice}, 대비:{stock.ChangeAmount}, 시가:{stock.OpenPrice}, 고가:{stock.HighPrice}, 저가:{stock.LowPrice}");

                                await Task.Run(() => command.ExecuteNonQuery());
                                insertedCount++;

                                ProgressUpdated?.Invoke("조건검색 결과 저장", insertedCount, stocks.Count);
                            }
                        }

                        transaction.Commit();
                        System.Diagnostics.Debug.WriteLine($"✅ 조건검색 결과 {stocks.Count}개 저장 완료 (전일대비 포함)");
                        return stocks.Count;
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
                System.Diagnostics.Debug.WriteLine($"❌ 조건검색 결과 저장 실패: {ex.Message}");
                throw new Exception($"조건검색 결과 저장 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 기존 조건검색 결과 삭제 (타입 오류 해결)
        /// </summary>
        private async Task DeleteExistingConditionResultsAsync(DateTime searchDate, int conditionIndex,
            SQLiteTransaction transaction)
        {
            string deleteQuery = @"
                DELETE FROM StockAnalysis 
                WHERE SearchDate = @SearchDate AND ConditionIndex = @ConditionIndex;";

            using (var command = new SQLiteCommand(deleteQuery, _connection, transaction))
            {
                command.Parameters.Add("@SearchDate", DbType.Date).Value = searchDate.Date;
                command.Parameters.Add("@ConditionIndex", DbType.Int64).Value = SafeInt64(conditionIndex);

                await Task.Run(() => command.ExecuteNonQuery());
            }
        }

        /// <summary>
        /// 조건검색 결과 조회 (Nullable 처리 수정)
        /// </summary>
        public async Task<List<StockAnalysisResult>> GetConditionSearchResultsAsync(DateTime searchDate,
            int? conditionIndex = null)
        {
            try
            {
                var results = new List<StockAnalysisResult>();

                // ✅ 전일대비 필드 추가
                string query = @"
                    SELECT SearchDate, StockCode, ConditionIndex, ConditionName, StockName, 
                           MarketType, ClosePrice, ChangeAmount, OpenPrice, HighPrice, LowPrice, 
                           Volume, ChangeRate, ProcessStatus, AnalysisProgress,
                           TechnicalScore, FinalScore, FinalGrade, 
                           BuyPrice, SellPrice, StopLossPrice
                    FROM StockAnalysis 
                    WHERE SearchDate = @SearchDate";

                if (conditionIndex.HasValue)
                {
                    query += " AND ConditionIndex = @ConditionIndex";
                }

                query += " ORDER BY ConditionIndex, StockName;";

                using (var command = new SQLiteCommand(query, _connection))
                {
                    command.Parameters.Add("@SearchDate", DbType.Date).Value = searchDate.Date;
                    if (conditionIndex.HasValue)
                    {
                        command.Parameters.Add("@ConditionIndex", DbType.Int64).Value = SafeInt64(conditionIndex ?? 0);
                    }

                    using (var reader = await Task.Run(() => command.ExecuteReader()))
                    {
                        while (await Task.Run(() => reader.Read()))
                        {
                            var result = new StockAnalysisResult
                            {
                                SearchDate = SafeGetDateTime(reader, "SearchDate"),
                                StockCode = SafeGetString(reader, "StockCode"),
                                ConditionIndex = SafeGetInt(reader, "ConditionIndex"),
                                ConditionName = SafeGetString(reader, "ConditionName"),
                                StockName = SafeGetString(reader, "StockName"),
                                MarketType = SafeGetString(reader, "MarketType"),
                                ClosePrice = SafeGetInt(reader, "ClosePrice"),

                                // ✅ 전일대비와 시가/고가/저가 추가
                                ChangeAmount = SafeGetInt(reader, "ChangeAmount"),
                                OpenPrice = SafeGetInt(reader, "OpenPrice"),
                                HighPrice = SafeGetInt(reader, "HighPrice"),
                                LowPrice = SafeGetInt(reader, "LowPrice"),

                                Volume = SafeGetLong(reader, "Volume"),
                                ChangeRate = SafeGetDouble(reader, "ChangeRate"),
                                ProcessStatus = SafeGetString(reader, "ProcessStatus"),
                                AnalysisProgress = SafeGetInt(reader, "AnalysisProgress"),

                                // 분석 결과들
                                TechnicalScore = SafeGetNullableInt(reader, "TechnicalScore"),
                                FinalScore = SafeGetNullableInt(reader, "FinalScore"),
                                FinalGrade = SafeGetString(reader, "FinalGrade"),
                                BuyPrice = SafeGetNullableInt(reader, "BuyPrice"),
                                SellPrice = SafeGetNullableInt(reader, "SellPrice"),
                                StopLossPrice = SafeGetNullableInt(reader, "StopLossPrice")
                            };

                            results.Add(result);
                        }
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                throw new Exception($"조건검색 결과 조회 실패: {ex.Message}");
            }
        }

        // ✅ Nullable int 읽기 메서드 추가
        private int? SafeGetNullableInt(SQLiteDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                if (reader.IsDBNull(ordinal)) return null;

                long value = reader.GetInt64(ordinal);
                if (value > int.MaxValue) return int.MaxValue;
                if (value < int.MinValue) return int.MinValue;

                return (int)value;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region 분석 결과 업데이트 (타입 오류 완전 수정)

        /// <summary>
        /// 기술적 분석 결과 업데이트 (타입 오류 완전 해결)
        /// </summary>
        public async Task<bool> UpdateTechnicalAnalysisAsync(string stockCode, DateTime searchDate,
            int conditionIndex, TechnicalAnalysisResult analysis)
        {
            try
            {
                string updateQuery = @"
                    UPDATE StockAnalysis SET
                        TechnicalScore = @TechnicalScore,
                        RSI = @RSI,
                        MACD = @MACD,
                        MACDSignal = @MACDSignal,
                        BollingerUpper = @BollingerUpper,
                        BollingerLower = @BollingerLower,
                        BollingerPosition = @BollingerPosition,
                        MA5 = @MA5,
                        MA20 = @MA20,
                        MA60 = @MA60,
                        TechnicalGrade = @TechnicalGrade,
                        ProcessStatus = 'TECHNICAL_ANALYSIS',
                        AnalysisProgress = 50,
                        TechnicalAnalysisAt = @TechnicalAnalysisAt,
                        UpdatedAt = @UpdatedAt
                    WHERE StockCode = @StockCode 
                      AND SearchDate = @SearchDate 
                      AND ConditionIndex = @ConditionIndex;";

                using (var command = new SQLiteCommand(updateQuery, _connection))
                {
                    // ✅ 안전한 타입 변환 적용
                    command.Parameters.Add("@TechnicalScore", DbType.Int64).Value = SafeInt64(analysis.TechnicalScore);
                    command.Parameters.Add("@RSI", DbType.Double).Value = SafeDouble(analysis.RSI);
                    command.Parameters.Add("@MACD", DbType.Double).Value = SafeDouble(analysis.MACD);
                    command.Parameters.Add("@MACDSignal", DbType.Double).Value = SafeDouble(analysis.MACDSignal);
                    command.Parameters.Add("@BollingerUpper", DbType.Int64).Value = SafeInt64(analysis.BollingerUpper);
                    command.Parameters.Add("@BollingerLower", DbType.Int64).Value = SafeInt64(analysis.BollingerLower);
                    command.Parameters.Add("@BollingerPosition", DbType.Double).Value = SafeDouble(analysis.BollingerPosition);
                    command.Parameters.Add("@MA5", DbType.Int64).Value = SafeInt64(analysis.MA5);
                    command.Parameters.Add("@MA20", DbType.Int64).Value = SafeInt64(analysis.MA20);
                    command.Parameters.Add("@MA60", DbType.Int64).Value = SafeInt64(analysis.MA60);
                    command.Parameters.Add("@TechnicalGrade", DbType.String).Value = SafeString(analysis.TechnicalGrade);
                    command.Parameters.Add("@TechnicalAnalysisAt", DbType.DateTime).Value = DateTime.Now;
                    command.Parameters.Add("@UpdatedAt", DbType.DateTime).Value = DateTime.Now;
                    command.Parameters.Add("@StockCode", DbType.String).Value = SafeString(stockCode);
                    command.Parameters.Add("@SearchDate", DbType.Date).Value = searchDate.Date;
                    command.Parameters.Add("@ConditionIndex", DbType.Int64).Value = SafeInt64(conditionIndex);

                    int rowsAffected = await Task.Run(() => command.ExecuteNonQuery());
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"기술적 분석 결과 업데이트 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 뉴스 분석 결과 업데이트 (타입 오류 완전 해결)
        /// </summary>
        public async Task<bool> UpdateNewsAnalysisAsync(string stockCode, DateTime searchDate,
            int conditionIndex, NewsAnalysisResult analysis)
        {
            try
            {
                string updateQuery = @"
                    UPDATE StockAnalysis SET
                        NewsScore = @NewsScore,
                        PositiveNewsCount = @PositiveNewsCount,
                        NegativeNewsCount = @NegativeNewsCount,
                        NeutralNewsCount = @NeutralNewsCount,
                        SentimentScore = @SentimentScore,
                        KeywordRelevance = @KeywordRelevance,
                        NewsImpactLevel = @NewsImpactLevel,
                        FinalScore = @FinalScore,
                        FinalGrade = @FinalGrade,
                        ProcessStatus = 'NEWS_ANALYSIS',
                        AnalysisProgress = 75,
                        NewsAnalysisAt = @NewsAnalysisAt,
                        UpdatedAt = @UpdatedAt
                    WHERE StockCode = @StockCode 
                      AND SearchDate = @SearchDate 
                      AND ConditionIndex = @ConditionIndex;";

                using (var command = new SQLiteCommand(updateQuery, _connection))
                {
                    command.Parameters.Add("@NewsScore", DbType.Int64).Value = SafeInt64(analysis.NewsScore);
                    command.Parameters.Add("@PositiveNewsCount", DbType.Int64).Value = SafeInt64(analysis.PositiveNewsCount);
                    command.Parameters.Add("@NegativeNewsCount", DbType.Int64).Value = SafeInt64(analysis.NegativeNewsCount);
                    command.Parameters.Add("@NeutralNewsCount", DbType.Int64).Value = SafeInt64(analysis.NeutralNewsCount);
                    command.Parameters.Add("@SentimentScore", DbType.Double).Value = SafeDouble(analysis.SentimentScore);
                    command.Parameters.Add("@KeywordRelevance", DbType.Double).Value = SafeDouble(analysis.KeywordRelevance);
                    command.Parameters.Add("@NewsImpactLevel", DbType.String).Value = SafeString(analysis.NewsImpactLevel);
                    command.Parameters.Add("@FinalScore", DbType.Int64).Value = SafeInt64(analysis.FinalScore);
                    command.Parameters.Add("@FinalGrade", DbType.String).Value = SafeString(analysis.FinalGrade);
                    command.Parameters.Add("@NewsAnalysisAt", DbType.DateTime).Value = DateTime.Now;
                    command.Parameters.Add("@UpdatedAt", DbType.DateTime).Value = DateTime.Now;
                    command.Parameters.Add("@StockCode", DbType.String).Value = SafeString(stockCode);
                    command.Parameters.Add("@SearchDate", DbType.Date).Value = searchDate.Date;
                    command.Parameters.Add("@ConditionIndex", DbType.Int64).Value = SafeInt64(conditionIndex);

                    int rowsAffected = await Task.Run(() => command.ExecuteNonQuery());
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"뉴스 분석 결과 업데이트 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 매매 계획 업데이트 (타입 오류 완전 해결)
        /// </summary>
        public async Task<bool> UpdateTradePlanAsync(string stockCode, DateTime searchDate,
            int conditionIndex, TradePlanResult plan)
        {
            try
            {
                string updateQuery = @"
                    UPDATE StockAnalysis SET
                        BuyPrice = @BuyPrice,
                        SellPrice = @SellPrice,
                        StopLossPrice = @StopLossPrice,
                        ExpectedReturn = @ExpectedReturn,
                        RiskLevel = @RiskLevel,
                        Recommendation = @Recommendation,
                        ProcessStatus = 'TRADE_PLAN',
                        AnalysisProgress = 100,
                        TradePlanAt = @TradePlanAt,
                        UpdatedAt = @UpdatedAt
                    WHERE StockCode = @StockCode 
                      AND SearchDate = @SearchDate 
                      AND ConditionIndex = @ConditionIndex;";

                using (var command = new SQLiteCommand(updateQuery, _connection))
                {
                    command.Parameters.Add("@BuyPrice", DbType.Int64).Value = SafeInt64(plan.BuyPrice);
                    command.Parameters.Add("@SellPrice", DbType.Int64).Value = SafeInt64(plan.SellPrice);
                    command.Parameters.Add("@StopLossPrice", DbType.Int64).Value = SafeInt64(plan.StopLossPrice);
                    command.Parameters.Add("@ExpectedReturn", DbType.Double).Value = SafeDouble(plan.ExpectedReturn);
                    command.Parameters.Add("@RiskLevel", DbType.String).Value = SafeString(plan.RiskLevel);
                    command.Parameters.Add("@Recommendation", DbType.String).Value = SafeString(plan.Recommendation);
                    command.Parameters.Add("@TradePlanAt", DbType.DateTime).Value = DateTime.Now;
                    command.Parameters.Add("@UpdatedAt", DbType.DateTime).Value = DateTime.Now;
                    command.Parameters.Add("@StockCode", DbType.String).Value = SafeString(stockCode);
                    command.Parameters.Add("@SearchDate", DbType.Date).Value = searchDate.Date;
                    command.Parameters.Add("@ConditionIndex", DbType.Int64).Value = SafeInt64(conditionIndex);

                    int rowsAffected = await Task.Run(() => command.ExecuteNonQuery());
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"매매 계획 업데이트 실패: {ex.Message}");
            }
        }

        #endregion

        #region 통계 및 조회 (타입 오류 수정)

        /// <summary>
        /// 분석 통계 조회 (타입 오류 수정)
        /// </summary>
        public async Task<AnalysisStatistics> GetAnalysisStatisticsAsync(DateTime searchDate)
        {
            try
            {
                string query = @"
                    SELECT 
                        COUNT(*) as TotalCount,
                        COUNT(CASE WHEN FinalGrade = 'S' THEN 1 END) as SGradeCount,
                        COUNT(CASE WHEN FinalGrade LIKE 'A%' THEN 1 END) as AGradeCount,
                        AVG(CASE WHEN ExpectedReturn IS NOT NULL THEN ExpectedReturn ELSE 0 END) as AvgExpectedReturn,
                        COUNT(CASE WHEN ProcessStatus = 'TRADE_PLAN' THEN 1 END) as CompletedCount
                    FROM StockAnalysis 
                    WHERE SearchDate = @SearchDate;";

                using (var command = new SQLiteCommand(query, _connection))
                {
                    command.Parameters.Add("@SearchDate", DbType.Date).Value = searchDate.Date;

                    using (var reader = await Task.Run(() => command.ExecuteReader()))
                    {
                        if (await Task.Run(() => reader.Read()))
                        {
                            return new AnalysisStatistics
                            {
                                SearchDate = searchDate,
                                TotalStockCount = SafeGetInt(reader, "TotalCount"),
                                SGradeCount = SafeGetInt(reader, "SGradeCount"),
                                AGradeCount = SafeGetInt(reader, "AGradeCount"),
                                AverageExpectedReturn = SafeGetDouble(reader, "AvgExpectedReturn"),
                                CompletedAnalysisCount = SafeGetInt(reader, "CompletedCount")
                            };
                        }
                    }
                }

                return new AnalysisStatistics { SearchDate = searchDate };
            }
            catch (Exception ex)
            {
                throw new Exception($"분석 통계 조회 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 로그 기록 (타입 오류 수정)
        /// </summary>
        public async Task LogAsync(string level, string category, string message,
            string stockCode = null, string details = null, int? executionTimeMs = null)
        {
            try
            {
                string insertQuery = @"
                    INSERT INTO AnalysisLog (LogDate, LogTime, LogLevel, Category, StockCode, Message, Details, ExecutionTimeMs)
                    VALUES (@LogDate, @LogTime, @LogLevel, @Category, @StockCode, @Message, @Details, @ExecutionTimeMs);";

                using (var command = new SQLiteCommand(insertQuery, _connection))
                {
                    command.Parameters.Add("@LogDate", DbType.Date).Value = DateTime.Now.Date;
                    command.Parameters.Add("@LogTime", DbType.DateTime).Value = DateTime.Now;
                    command.Parameters.Add("@LogLevel", DbType.String).Value = SafeString(level);
                    command.Parameters.Add("@Category", DbType.String).Value = SafeString(category);
                    command.Parameters.Add("@StockCode", DbType.String).Value = stockCode != null ? SafeString(stockCode) : (object)DBNull.Value;
                    command.Parameters.Add("@Message", DbType.String).Value = SafeString(message);
                    command.Parameters.Add("@Details", DbType.String).Value = details != null ? SafeString(details) : (object)DBNull.Value;
                    command.Parameters.Add("@ExecutionTimeMs", DbType.Int64).Value =
                        executionTimeMs.HasValue ? (object)SafeInt64(executionTimeMs.Value) : DBNull.Value;

                    await Task.Run(() => command.ExecuteNonQuery());
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"로그 기록 실패: {ex.Message}");
            }
        }

        #endregion

        #region 설정 관리

        /// <summary>
        /// 설정값 저장
        /// </summary>
        public async Task<bool> SaveSettingAsync(string key, object value, string description = null)
        {
            try
            {
                string settingType = GetSettingType(value);
                string settingValue = value?.ToString() ?? "";

                string upsertQuery = @"
                    INSERT OR REPLACE INTO UserSettings (SettingKey, SettingValue, SettingType, Description, UpdatedAt)
                    VALUES (@SettingKey, @SettingValue, @SettingType, @Description, @UpdatedAt);";

                using (var command = new SQLiteCommand(upsertQuery, _connection))
                {
                    command.Parameters.Add("@SettingKey", DbType.String).Value = SafeString(key);
                    command.Parameters.Add("@SettingValue", DbType.String).Value = settingValue;
                    command.Parameters.Add("@SettingType", DbType.String).Value = settingType;
                    command.Parameters.Add("@Description", DbType.String).Value = description != null ? SafeString(description) : (object)DBNull.Value;
                    command.Parameters.Add("@UpdatedAt", DbType.DateTime).Value = DateTime.Now;

                    int rowsAffected = await Task.Run(() => command.ExecuteNonQuery());
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"설정 저장 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 설정값 조회
        /// </summary>
        public async Task<T> GetSettingAsync<T>(string key, T defaultValue = default)
        {
            try
            {
                string query = "SELECT SettingValue, SettingType FROM UserSettings WHERE SettingKey = @SettingKey;";

                using (var command = new SQLiteCommand(query, _connection))
                {
                    command.Parameters.Add("@SettingKey", DbType.String).Value = SafeString(key);

                    using (var reader = await Task.Run(() => command.ExecuteReader()))
                    {
                        if (await Task.Run(() => reader.Read()))
                        {
                            string value = SafeGetString(reader, "SettingValue");
                            string type = SafeGetString(reader, "SettingType");

                            return ConvertSetting<T>(value, type);
                        }
                    }
                }

                return defaultValue;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"설정 조회 실패: {ex.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// 설정 타입 확인
        /// </summary>
        private string GetSettingType(object value)
        {
            if (value == null) return "STRING";

            Type type = value.GetType();
            if (type == typeof(int) || type == typeof(long)) return "INTEGER";
            if (type == typeof(bool)) return "BOOLEAN";
            if (type == typeof(decimal) || type == typeof(double) || type == typeof(float)) return "DECIMAL";
            return "STRING";
        }

        /// <summary>
        /// 설정값 타입 변환
        /// </summary>
        private T ConvertSetting<T>(string value, string type)
        {
            try
            {
                Type targetType = typeof(T);

                if (targetType == typeof(string))
                    return (T)(object)value;

                if (targetType == typeof(int))
                    return (T)(object)int.Parse(value);

                if (targetType == typeof(bool))
                    return (T)(object)bool.Parse(value);

                if (targetType == typeof(decimal))
                    return (T)(object)decimal.Parse(value);

                if (targetType == typeof(double))
                    return (T)(object)double.Parse(value);

                return (T)(object)value;
            }
            catch
            {
                return default;
            }
        }

        #endregion

        #region 안전한 데이터 변환 유틸리티

        /// <summary>
        /// 안전한 문자열 변환
        /// </summary>
        private string SafeString(string value)
        {
            return value ?? "";
        }

        /// <summary>
        /// 안전한 Int64 변환
        /// </summary>
        private long SafeInt64(object value)
        {
            if (value == null) return 0;

            try
            {
                if (value is int intValue) return intValue;
                if (value is long longValue) return longValue;
                if (value is double doubleValue) return (long)doubleValue;
                if (value is decimal decimalValue) return (long)decimalValue;

                return Convert.ToInt64(value);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 안전한 Double 변환
        /// </summary>
        private double SafeDouble(object value)
        {
            if (value == null) return 0.0;

            try
            {
                return Convert.ToDouble(value);
            }
            catch
            {
                return 0.0;
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
        /// 안전한 DateTime 읽기
        /// </summary>
        private DateTime SafeGetDateTime(SQLiteDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                if (reader.IsDBNull(ordinal)) return DateTime.MinValue;

                return reader.GetDateTime(ordinal);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// 안전한 정수 읽기 (SQLite Int64 → Int32 변환 개선)
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

        #region 유틸리티 및 정리

        #region 영업일 관리 (holidays.db 연동)


        private string GetHolidaysDbPath()
        {
            try
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

                // 🔧 정확한 경로 (Data 폴더 안에 있음)
                string[] alternativePaths = {
            // Data 폴더를 포함한 정확한 경로들
            Path.Combine(baseDirectory, @"..\..\..\..\HolidayManager\bin\Debug\Data\holidays.db"),
            Path.Combine(baseDirectory, @"..\..\..\HolidayManager\bin\Debug\Data\holidays.db"),
            Path.Combine(baseDirectory, @"..\..\HolidayManager\bin\Debug\Data\holidays.db"),
            
            // 절대 경로 (확실한 경로)
            @"C:\Projects\StockTrader3_WinForms\HolidayManager\bin\Debug\Data\holidays.db",
            
            // 기타 가능한 경로들
            Path.Combine(baseDirectory, @"..\..\..\..\HolidayManager\bin\Debug\holidays.db"),
            Path.Combine(baseDirectory, "holidays.db"),
            Path.Combine(baseDirectory, "Data", "holidays.db")
        };

                foreach (string path in alternativePaths)
                {
                    try
                    {
                        string fullPath = Path.GetFullPath(path);
                        if (File.Exists(fullPath))
                        {
                            System.Diagnostics.Debug.WriteLine($"✅ holidays.db 발견: {fullPath}");
                            return fullPath;
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"❌ 경로 없음: {fullPath}");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"⚠️ 경로 체크 오류: {path} - {ex.Message}");
                    }
                }

                System.Diagnostics.Debug.WriteLine("⚠️ holidays.db 파일을 찾을 수 없습니다. 주말만 체크합니다.");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ 휴일 DB 경로 확인 중 오류: {ex.Message}");
                return null;
            }
        }

     
       
        /// <summary>
        /// 해당 날짜가 영업일인지 확인 (holidays.db 연동)
        /// </summary>
        public bool IsBusinessDay(DateTime date)
        {
            try
            {
                // 1️⃣ 주말 체크
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                {
                    return false;
                }

                // 2️⃣ 휴일 DB 체크
                string holidaysDbPath = GetHolidaysDbPath();
                if (string.IsNullOrEmpty(holidaysDbPath))
                {
                    // holidays.db가 없으면 주말만 체크
                    return true;
                }

                string connectionString = $"Data Source={holidaysDbPath};Version=3;";
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT COUNT(*) FROM Holidays WHERE HolidayDate = @date";
                    using (var cmd = new SQLiteCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@date", date.ToString("yyyy-MM-dd"));
                        var count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count == 0; // 휴일 테이블에 없으면 영업일
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ 영업일 체크 중 오류: {ex.Message}");
                // 오류 시 주말만 체크
                return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
            }
        }

        public DateTime GetLastTradingDay(DateTime currentDate = default)
        {
            if (currentDate == default)
                currentDate = DateTime.Now;

            DateTime candidateDate = currentDate.Date;

            // 🆕 키움 조건검색 갱신 시간 고려
            // 새벽 4시 이전이면 전일 영업일 기준
            if (currentDate.TimeOfDay < new TimeSpan(02, 00, 0))
            {
                candidateDate = candidateDate.AddDays(-1);
            }
            // 새벽 4시 ~ 장시작(09:00) 사이면 전일 영업일 기준  
            else if (currentDate.TimeOfDay < new TimeSpan(09, 00, 0))
            {
                candidateDate = candidateDate.AddDays(-1);
            }
            // 장시작 후면 당일 기준
            else
            {
                candidateDate = currentDate.Date;
            }

            // 영업일 찾기
            while (!IsBusinessDay(candidateDate))
            {
                candidateDate = candidateDate.AddDays(-1);
            }

            return candidateDate;
        }


        /// <summary>
        /// 다음 영업일 계산
        /// </summary>
        public DateTime GetNextBusinessDay(DateTime baseDate)
        {
            DateTime nextDate = baseDate.AddDays(1);

            while (!IsBusinessDay(nextDate))
            {
                nextDate = nextDate.AddDays(1);
            }

            return nextDate;
        }

        #endregion


        /// <summary>
        /// 데이터베이스 백업
        /// </summary>
        public async Task<bool> BackupDatabaseAsync(string backupPath = null)
        {
            try
            {
                if (string.IsNullOrEmpty(backupPath))
                {
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    backupPath = Path.Combine(
                        Path.GetDirectoryName(_databasePath),
                        $"StockTrader3_Backup_{timestamp}.db");
                }

                await Task.Run(() => File.Copy(_databasePath, backupPath, true));

                System.Diagnostics.Debug.WriteLine($"✅ 데이터베이스 백업 완료: {backupPath}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ 데이터베이스 백업 실패: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 오래된 로그 정리 (30일 이상 된 로그 삭제)
        /// </summary>
        public async Task<int> CleanupOldLogsAsync(int daysToKeep = 30)
        {
            try
            {
                DateTime cutoffDate = DateTime.Now.AddDays(-daysToKeep);

                string deleteQuery = "DELETE FROM AnalysisLog WHERE LogDate < @CutoffDate;";

                using (var command = new SQLiteCommand(deleteQuery, _connection))
                {
                    command.Parameters.Add("@CutoffDate", DbType.Date).Value = cutoffDate.Date;
                    int deletedRows = await Task.Run(() => command.ExecuteNonQuery());

                    System.Diagnostics.Debug.WriteLine($"✅ 오래된 로그 {deletedRows}개 정리 완료");
                    return deletedRows;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ 로그 정리 실패: {ex.Message}");
                return 0;
            }
        }

        #endregion

        #region 과거 데이터 관리

        /// <summary>
        /// 과거 일봉 데이터 저장
        /// </summary>
        public async Task<int> SaveHistoricalDataAsync(string stockCode, List<DailyPrice> dailyPrices)
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
                                command.Parameters.Add("@StockCode", DbType.String).Value = SafeString(stockCode);
                                command.Parameters.Add("@Date", DbType.Date).Value = price.Date.Date;
                                command.Parameters.Add("@Open", DbType.Int64).Value = SafeInt64((int)(price.Open));
                                command.Parameters.Add("@High", DbType.Int64).Value = SafeInt64((int)(price.High));
                                command.Parameters.Add("@Low", DbType.Int64).Value = SafeInt64((int)(price.Low));
                                command.Parameters.Add("@Close", DbType.Int64).Value = SafeInt64((int)(price.Close));
                                command.Parameters.Add("@Volume", DbType.Int64).Value = SafeInt64(price.Volume);
                                command.Parameters.Add("@CreatedAt", DbType.DateTime).Value = DateTime.Now;

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
                throw new Exception($"일봉 데이터 저장 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 과거 일봉 데이터 조회
        /// </summary>
        public async Task<List<DailyPrice>> GetHistoricalDataAsync(string stockCode, int days = 60)
        {
            try
            {
                var results = new List<DailyPrice>();

                string query = @"
                    SELECT Date, Open, High, Low, Close, Volume 
                    FROM DailyPriceHistory 
                    WHERE StockCode = @StockCode 
                    ORDER BY Date DESC 
                    LIMIT @Days;";

                using (var command = new SQLiteCommand(query, _connection))
                {
                    command.Parameters.Add("@StockCode", DbType.String).Value = SafeString(stockCode);
                    command.Parameters.Add("@Days", DbType.Int64).Value = SafeInt64(days);

                    using (var reader = await Task.Run(() => command.ExecuteReader()))
                    {
                        while (await Task.Run(() => reader.Read()))
                        {
                            var dailyPrice = new DailyPrice
                            {
                                Date = SafeGetDateTime(reader, "Date"),
                                Open = SafeGetInt(reader, "Open"),
                                High = SafeGetInt(reader, "High"),
                                Low = SafeGetInt(reader, "Low"),
                                Close = SafeGetInt(reader, "Close"),
                                Volume = SafeGetLong(reader, "Volume")
                            };
                            results.Add(dailyPrice);
                        }
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ 일봉 데이터 조회 실패: {ex.Message}");
                throw new Exception($"일봉 데이터 조회 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 과거 일봉 데이터 존재 여부 확인
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
                    command.Parameters.Add("@StockCode", DbType.String).Value = SafeString(stockCode);

                    var count = await Task.Run(() => command.ExecuteScalar());
                    int existingDays = (int)SafeInt64(count);
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
        /// 분봉 데이터 저장
        /// </summary>
        public async Task<int> SaveMinuteHistoricalDataAsync(string stockCode, int minuteInterval, List<StockTrader3.Models.MinutePrice> minutePrices)
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
                                command.Parameters.Add("@StockCode", DbType.String).Value = SafeString(stockCode);
                                command.Parameters.Add("@DateTime", DbType.String).Value = price.DateTime.ToString("yyyy-MM-dd HH:mm:ss");
                                command.Parameters.Add("@MinuteInterval", DbType.Int64).Value = SafeInt64(minuteInterval);
                                command.Parameters.Add("@Open", DbType.Int64).Value = SafeInt64((int)price.Open);
                                command.Parameters.Add("@High", DbType.Int64).Value = SafeInt64((int)price.High);
                                command.Parameters.Add("@Low", DbType.Int64).Value = SafeInt64((int)price.Low);
                                command.Parameters.Add("@Close", DbType.Int64).Value = SafeInt64((int)price.Close);
                                command.Parameters.Add("@Volume", DbType.Int64).Value = SafeInt64(price.Volume);
                                command.Parameters.Add("@ChangeAmount", DbType.Int64).Value = SafeInt64((int)price.ChangeAmount);
                                command.Parameters.Add("@ChangeRate", DbType.Double).Value = SafeDouble((double)price.ChangeRate);
                                command.Parameters.Add("@TradingValue", DbType.Int64).Value = SafeInt64(price.TradingValue);
                                command.Parameters.Add("@CreatedAt", DbType.DateTime).Value = DateTime.Now;

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
                throw new Exception($"분봉 데이터 저장 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 분봉 데이터 조회
        /// </summary>
        public async Task<List<StockTrader3.Models.MinutePrice>> GetMinuteHistoricalDataAsync(string stockCode, int minuteInterval, int days = 5)
        {
            try
            {
                var results = new List<StockTrader3.Models.MinutePrice>();

                string query = @"
                    SELECT DateTime, Open, High, Low, Close, Volume, ChangeAmount, ChangeRate, TradingValue
                    FROM MinutePriceHistory 
                    WHERE StockCode = @StockCode AND MinuteInterval = @MinuteInterval
                    ORDER BY DateTime DESC 
                    LIMIT @Limit;";

                // 일당 분봉 개수 계산 (장시간 6.5시간 기준)
                int minutesPerDay = (int)(6.5 * 60 / minuteInterval);
                int limit = days * minutesPerDay;

                using (var command = new SQLiteCommand(query, _connection))
                {
                    command.Parameters.Add("@StockCode", DbType.String).Value = SafeString(stockCode);
                    command.Parameters.Add("@MinuteInterval", DbType.Int64).Value = SafeInt64(minuteInterval);
                    command.Parameters.Add("@Limit", DbType.Int64).Value = SafeInt64(limit);

                    using (var reader = await Task.Run(() => command.ExecuteReader()))
                    {
                        while (await Task.Run(() => reader.Read()))
                        {
                            var minutePrice = new StockTrader3.Models.MinutePrice
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
                throw new Exception($"분봉 데이터 조회 실패: {ex.Message}");
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
                    command.Parameters.Add("@StockCode", DbType.String).Value = SafeString(stockCode);
                    command.Parameters.Add("@MinuteInterval", DbType.Int64).Value = SafeInt64(minuteInterval);

                    var count = await Task.Run(() => command.ExecuteScalar());
                    int existingCount = (int)SafeInt64(count);

                    // 일당 분봉 개수 계산
                    int minutesPerDay = (int)(6.5 * 60 / minuteInterval);
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

        #region IDisposable 구현

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    try
                    {
                        _connection?.Close();
                        _connection?.Dispose();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"DatabaseManager Dispose 오류: {ex.Message}");
                    }
                }

                _disposed = true;
            }
        }

        ~DatabaseManager()
        {
            Dispose(false);
        }

        #endregion
    }

    #region 데이터 모델 클래스들

    /// <summary>
    /// 종목 기본 정보
    /// </summary>
    public class StockBasicInfo
    {
        public string StockCode { get; set; }
        public string StockName { get; set; }
        public string MarketType { get; set; }
        public int ClosePrice { get; set; }
        public int ChangeAmount { get; set; }
        public int OpenPrice { get; set; }
        public int HighPrice { get; set; }
        public int LowPrice { get; set; }
        public long Volume { get; set; }
        public double ChangeRate { get; set; }
    }

    /// <summary>
    /// 종목 분석 결과
    /// </summary>
    public class StockAnalysisResult
    {
        public DateTime SearchDate { get; set; }
        public string StockCode { get; set; }
        public int ConditionIndex { get; set; }
        public string ConditionName { get; set; }
        public string StockName { get; set; }
        public string MarketType { get; set; }
        public int ClosePrice { get; set; }
        public long Volume { get; set; }
        public double ChangeRate { get; set; }
        public string ProcessStatus { get; set; }
        public int AnalysisProgress { get; set; }
        public int ChangeAmount { get; set; }
        public int OpenPrice { get; set; }
        public int HighPrice { get; set; }
        public int LowPrice { get; set; }

        // 분석 결과들
        public int? TechnicalScore { get; set; }
        public string TechnicalGrade { get; set; }
        public int? NewsScore { get; set; }
        public int? FinalScore { get; set; }
        public string FinalGrade { get; set; }

        // 매매 계획
        public int? BuyPrice { get; set; }
        public int? SellPrice { get; set; }
        public int? StopLossPrice { get; set; }
        public double? ExpectedReturn { get; set; }
        public string Recommendation { get; set; }
    }

    /// <summary>
    /// 기술적 분석 결과
    /// </summary>
    public class TechnicalAnalysisResult
    {
        public int TechnicalScore { get; set; }
        public double RSI { get; set; }
        public double MACD { get; set; }
        public double MACDSignal { get; set; }
        public int BollingerUpper { get; set; }
        public int BollingerLower { get; set; }
        public double BollingerPosition { get; set; }
        public int MA5 { get; set; }
        public int MA20 { get; set; }
        public int MA60 { get; set; }
        public string TechnicalGrade { get; set; }
    }

    /// <summary>
    /// 뉴스 분석 결과
    /// </summary>
    public class NewsAnalysisResult
    {
        public int NewsScore { get; set; }
        public int PositiveNewsCount { get; set; }
        public int NegativeNewsCount { get; set; }
        public int NeutralNewsCount { get; set; }
        public double SentimentScore { get; set; }
        public double KeywordRelevance { get; set; }
        public string NewsImpactLevel { get; set; }
        public int FinalScore { get; set; }
        public string FinalGrade { get; set; }
    }

    /// <summary>
    /// 매매 계획 결과
    /// </summary>
    public class TradePlanResult
    {
        public int BuyPrice { get; set; }
        public int SellPrice { get; set; }
        public int StopLossPrice { get; set; }
        public double ExpectedReturn { get; set; }
        public string RiskLevel { get; set; }
        public string Recommendation { get; set; }
    }

    /// <summary>
    /// 분석 통계
    /// </summary>
    public class AnalysisStatistics
    {
        public DateTime SearchDate { get; set; }
        public int TotalStockCount { get; set; }
        public int SGradeCount { get; set; }
        public int AGradeCount { get; set; }
        public double AverageExpectedReturn { get; set; }
        public int CompletedAnalysisCount { get; set; }
    }

    #endregion
}