using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using Dapper;

namespace AutoTrader_WinForms.Managers
{
    /// <summary>
    /// StockTrader3.db 읽기 전용 연동 및 AutoTrader.db 관리
    /// </summary>
    public static class DatabaseManager
    {
        #region 연결 문자열

        // StockTrader3.db 경로 (읽기 전용)
        private static readonly string StockTrader3DbPath = GetStockTrader3DbPath();

        // AutoTrader.db 경로 (읽기/쓰기)
        private static readonly string AutoTraderDbPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "AutoTrader.db");

        private static string StockTrader3ConnectionString =>
            $"Data Source={StockTrader3DbPath};Version=3;Read Only=True;";

        private static string AutoTraderConnectionString =>
            $"Data Source={AutoTraderDbPath};Version=3;";

        #endregion

        #region 초기화

        /// <summary>
        /// StockTrader3.db 경로 자동 탐지
        /// </summary>

        private static string GetStockTrader3DbPath()
        {
            // 🔧 여러 경로 시도하여 StockTrader3.db 찾기
            List<string> candidatePaths = new List<string>();

            // 1. 현재 실행 경로 기준 상대 경로들
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;

            // x86 고려한 상대 경로
            candidatePaths.Add(Path.Combine(currentDir, @"..\..\..\..\StockTrader3_WinForms\bin\x86\Debug\StockTrader3.db"));
            candidatePaths.Add(Path.Combine(currentDir, @"..\..\..\..\StockTrader3_WinForms\bin\Debug\StockTrader3.db"));

            // 2. 프로젝트 루트에서 찾기
            string projectsRoot = Directory.GetParent(currentDir)?.Parent?.Parent?.Parent?.FullName;
            if (projectsRoot != null)
            {
                candidatePaths.Add(Path.Combine(projectsRoot, "StockTrader3_WinForms", "bin", "x86", "Debug", "StockTrader3.db"));
                candidatePaths.Add(Path.Combine(projectsRoot, "StockTrader3_WinForms", "bin", "Debug", "StockTrader3.db"));
            }

            // 3. 기본 경로 (문서에서 확인된 경로)
            candidatePaths.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Projects", "StockTrader3_WinForms", "bin", "x86", "Debug", "StockTrader3.db"));

            // 4. 절대 경로 (확실한 경로)
            candidatePaths.Add(@"C:\Projects\StockTrader3_WinForms\bin\x86\Debug\StockTrader3.db");

            // 🔍 각 경로를 순서대로 체크
            foreach (string path in candidatePaths)
            {
                try
                {
                    string fullPath = Path.GetFullPath(path);
                    Console.WriteLine($"DB 경로 체크: {fullPath}");

                    if (File.Exists(fullPath))
                    {
                        Console.WriteLine($"✅ DB 파일 발견: {fullPath}");
                        return fullPath;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"경로 체크 오류 ({path}): {ex.Message}");
                }
            }

            // 모든 경로에서 찾지 못한 경우
            string defaultPath = candidatePaths.First();
            Console.WriteLine($"❌ DB 파일을 찾을 수 없음. 기본 경로 반환: {defaultPath}");
            return defaultPath;
        }


      

        /// <summary>
        /// 데이터베이스 연결 상태 확인 (자동 초기화 포함)
        /// </summary>
        public static bool TestConnection(out string message)
        {
            try
            {
                // StockTrader3.db 존재 확인
                if (!File.Exists(StockTrader3DbPath))
                {
                    message = $"StockTrader3.db 파일을 찾을 수 없습니다: {StockTrader3DbPath}";
                    return false;
                }

                // StockTrader3.db 연결 테스트
                using (var connection = new SQLiteConnection(StockTrader3ConnectionString))
                {
                    connection.Open();
                    var count = connection.QuerySingle<int>("SELECT COUNT(*) FROM StockAnalysis");

                    // AutoTrader.db 자동 초기화
                    EnsureAutoTraderDbExists();

                    message = $"연결 성공! StockAnalysis 테이블에 {count:N0}개 레코드 발견";
                    return true;
                }
            }
            catch (Exception ex)
            {
                message = $"연결 실패: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// AutoTrader.db 존재 확인 및 자동 생성
        /// </summary>
        private static void EnsureAutoTraderDbExists()
        {
            try
            {
                // DB 파일이 없거나 테이블이 없으면 생성
                if (!File.Exists(AutoTraderDbPath) || !CheckAutoTraderTables())
                {
                    Console.WriteLine("AutoTrader.db 자동 초기화 중...");
                    InitializeAutoTraderDb();
                    Console.WriteLine("AutoTrader.db 자동 초기화 완료");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AutoTrader.db 자동 초기화 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// AutoTrader.db 테이블 존재 확인
        /// </summary>
        private static bool CheckAutoTraderTables()
        {
            try
            {
                using (var connection = new SQLiteConnection(AutoTraderConnectionString))
                {
                    connection.Open();
                    var tableCount = connection.QuerySingle<int>(
                        @"SELECT COUNT(*) FROM sqlite_master 
                          WHERE type='table' AND name IN ('TradingPlans', 'TradingPlanItems', 'TradingPositions', 'TradingOrders', 'DailyPerformance')");

                    return tableCount == 5; // 5개 테이블 모두 존재해야 함
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// AutoTrader.db 초기화 및 테이블 생성
        /// </summary>
        public static void InitializeAutoTraderDb()
        {
            try
            {
                using (var connection = new SQLiteConnection(AutoTraderConnectionString))
                {
                    connection.Open();

                    // 테이블 생성
                    CreateTradingPlansTables(connection);
                    CreateTradingPositionsTables(connection);
                    CreateTradingOrdersTables(connection);
                    CreateDailyPerformanceTables(connection);
                    CreateIndexes(connection);

                    Console.WriteLine("AutoTrader.db 초기화 완료");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AutoTrader.db 초기화 오류: {ex.Message}");
                throw;
            }
        }

        #region AutoTrader.db 테이블 생성

        private static void CreateTradingPlansTables(SQLiteConnection connection)
        {
            string sql = @"
                CREATE TABLE IF NOT EXISTS TradingPlans (
                    PlanID INTEGER PRIMARY KEY AUTOINCREMENT,
                    PlanDate TEXT NOT NULL,
                    AnalysisDate TEXT NOT NULL,
                    TotalInvestment INTEGER NOT NULL,
                    TargetReturn REAL DEFAULT 5.0,
                    MaxDailyLoss REAL DEFAULT 10.0,
                    SelectedStockCount INTEGER,
                    SGradeCount INTEGER,
                    AGradeCount INTEGER,
                    SGradeWeight REAL DEFAULT 1.5,
                    AGradeWeight REAL DEFAULT 1.0,
                    Status TEXT DEFAULT 'Draft',
                    Notes TEXT,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                );

                CREATE TABLE IF NOT EXISTS TradingPlanItems (
                    ItemID INTEGER PRIMARY KEY AUTOINCREMENT,
                    PlanID INTEGER NOT NULL,
                    StockCode TEXT NOT NULL,
                    StockName TEXT NOT NULL,
                    FinalGrade TEXT NOT NULL,
                    FinalScore INTEGER NOT NULL,
                    TechnicalScore INTEGER,
                    NewsScore INTEGER,
                    ClosePrice INTEGER NOT NULL,
                    Volume BIGINT,
                    CalculatedBuyPrice INTEGER,
                    CalculatedSellPrice INTEGER,
                    CalculatedStopPrice INTEGER,
                    ExpectedReturn REAL,
                    IsSelected INTEGER DEFAULT 0,
                    PlannedInvestment INTEGER,
                    PlannedQuantity INTEGER,
                    Priority INTEGER DEFAULT 0,
                    RiskLevel TEXT,
                    Sector TEXT,
                    MarketCap TEXT,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (PlanID) REFERENCES TradingPlans(PlanID)
                );";

            connection.Execute(sql);
        }

        private static void CreateTradingPositionsTables(SQLiteConnection connection)
        {
            string sql = @"
                CREATE TABLE IF NOT EXISTS TradingPositions (
                    PositionID INTEGER PRIMARY KEY AUTOINCREMENT,
                    PlanID INTEGER NOT NULL,
                    ItemID INTEGER NOT NULL,
                    StockCode TEXT NOT NULL,
                    StockName TEXT NOT NULL,
                    PlannedBuyPrice INTEGER NOT NULL,
                    PlannedSellPrice INTEGER NOT NULL,
                    PlannedStopPrice INTEGER NOT NULL,
                    PlannedQuantity INTEGER NOT NULL,
                    PlannedInvestment INTEGER NOT NULL,
                    BuyOrderID TEXT,
                    SellOrderID TEXT,
                    BuyFilledQty INTEGER DEFAULT 0,
                    SellFilledQty INTEGER DEFAULT 0,
                    CurrentHoldingQty INTEGER DEFAULT 0,
                    AvgBuyPrice INTEGER DEFAULT 0,
                    AvgSellPrice INTEGER DEFAULT 0,
                    CurrentPrice INTEGER DEFAULT 0,
                    UnrealizedPL INTEGER DEFAULT 0,
                    UnrealizedRate REAL DEFAULT 0,
                    MaxUnrealizedPL INTEGER DEFAULT 0,
                    MinUnrealizedPL INTEGER DEFAULT 0,
                    Status TEXT DEFAULT 'Ready',
                    BuyOrderTime DATETIME,
                    FirstFillTime DATETIME,
                    LastFillTime DATETIME,
                    SellOrderTime DATETIME,
                    CloseTime DATETIME,
                    HoldingMinutes INTEGER DEFAULT 0,
                    LastUpdated DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (PlanID) REFERENCES TradingPlans(PlanID),
                    FOREIGN KEY (ItemID) REFERENCES TradingPlanItems(ItemID)
                );";

            connection.Execute(sql);
        }

        private static void CreateTradingOrdersTables(SQLiteConnection connection)
        {
            string sql = @"
                CREATE TABLE IF NOT EXISTS TradingOrders (
                    OrderID INTEGER PRIMARY KEY AUTOINCREMENT,
                    PositionID INTEGER NOT NULL,
                    OrderType TEXT NOT NULL,
                    OrderMethod TEXT NOT NULL,
                    OrderPrice INTEGER NOT NULL,
                    OrderQuantity INTEGER NOT NULL,
                    FilledQuantity INTEGER DEFAULT 0,
                    RemainQuantity INTEGER DEFAULT 0,
                    AvgFillPrice INTEGER DEFAULT 0,
                    KiwoomOrderNo TEXT,
                    OrderStatus TEXT DEFAULT 'Pending',
                    OrderTime DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FirstFillTime DATETIME,
                    LastFillTime DATETIME,
                    CancelTime DATETIME,
                    Commission INTEGER DEFAULT 0,
                    Tax INTEGER DEFAULT 0,
                    Notes TEXT,
                    FOREIGN KEY (PositionID) REFERENCES TradingPositions(PositionID)
                );";

            connection.Execute(sql);
        }

        private static void CreateDailyPerformanceTables(SQLiteConnection connection)
        {
            string sql = @"
                CREATE TABLE IF NOT EXISTS DailyPerformance (
                    PerformanceID INTEGER PRIMARY KEY AUTOINCREMENT,
                    TradingDate TEXT NOT NULL,
                    PlanID INTEGER,
                    PlannedStockCount INTEGER,
                    ExecutedStockCount INTEGER,
                    TotalPlannedInvestment INTEGER,
                    TotalActualInvestment INTEGER,
                    BuySuccessCount INTEGER DEFAULT 0,
                    BuyFailCount INTEGER DEFAULT 0,
                    BuySuccessRate REAL DEFAULT 0,
                    PartialFillCount INTEGER DEFAULT 0,
                    ProfitCount INTEGER DEFAULT 0,
                    LossCount INTEGER DEFAULT 0,
                    BreakevenCount INTEGER DEFAULT 0,
                    WinRate REAL DEFAULT 0,
                    TotalRealizedPL INTEGER DEFAULT 0,
                    TotalReturnRate REAL DEFAULT 0,
                    MaxSingleProfit INTEGER DEFAULT 0,
                    MaxSingleLoss INTEGER DEFAULT 0,
                    AvgHoldingMinutes REAL DEFAULT 0,
                    AvgProfitRate REAL DEFAULT 0,
                    AvgLossRate REAL DEFAULT 0,
                    TotalCommission INTEGER DEFAULT 0,
                    TotalTax INTEGER DEFAULT 0,
                    NetProfit INTEGER DEFAULT 0,
                    KospiStartPrice REAL,
                    KospiEndPrice REAL,
                    KospiReturnRate REAL,
                    MarketVolume BIGINT,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (PlanID) REFERENCES TradingPlans(PlanID)
                );";

            connection.Execute(sql);
        }

        private static void CreateIndexes(SQLiteConnection connection)
        {
            string[] indexes = {
                "CREATE INDEX IF NOT EXISTS idx_trading_plans_date ON TradingPlans(PlanDate);",
                "CREATE INDEX IF NOT EXISTS idx_trading_plans_status ON TradingPlans(Status);",
                "CREATE INDEX IF NOT EXISTS idx_plan_items_plan_id ON TradingPlanItems(PlanID);",
                "CREATE INDEX IF NOT EXISTS idx_plan_items_stock_code ON TradingPlanItems(StockCode);",
                "CREATE INDEX IF NOT EXISTS idx_plan_items_grade ON TradingPlanItems(FinalGrade);",
                "CREATE INDEX IF NOT EXISTS idx_plan_items_selected ON TradingPlanItems(IsSelected);",
                "CREATE INDEX IF NOT EXISTS idx_positions_plan_id ON TradingPositions(PlanID);",
                "CREATE INDEX IF NOT EXISTS idx_positions_stock_code ON TradingPositions(StockCode);",
                "CREATE INDEX IF NOT EXISTS idx_positions_status ON TradingPositions(Status);",
                "CREATE INDEX IF NOT EXISTS idx_orders_position_id ON TradingOrders(PositionID);",
                "CREATE INDEX IF NOT EXISTS idx_orders_kiwoom_no ON TradingOrders(KiwoomOrderNo);",
                "CREATE INDEX IF NOT EXISTS idx_orders_status ON TradingOrders(OrderStatus);",
                "CREATE INDEX IF NOT EXISTS idx_performance_date ON DailyPerformance(TradingDate);",
                "CREATE INDEX IF NOT EXISTS idx_performance_plan_id ON DailyPerformance(PlanID);"
            };

            foreach (string indexSql in indexes)
            {
                connection.Execute(indexSql);
            }
        }

        #endregion

        #endregion

        #region StockTrader3 데이터 조회

        /// <summary>
        /// 최신 분석일자 조회 (영업일 보정 적용)
        /// </summary>
        public static string GetLatestAnalysisDate()
        {
            try
            {
                using (var connection = new SQLiteConnection(StockTrader3ConnectionString))
                {
                    connection.Open();

                    // 모든 분석일자 확인 (디버깅용)
                    var allDates = connection.Query<string>(
                        "SELECT DISTINCT SearchDate FROM StockAnalysis WHERE SearchDate IS NOT NULL ORDER BY SearchDate DESC LIMIT 5");

                    Console.WriteLine($"=== 디버깅: 최근 5개 분석일자 ===");
                    foreach (var date in allDates)
                    {
                        var count = connection.QuerySingle<int>(
                            "SELECT COUNT(*) FROM StockAnalysis WHERE SearchDate = @date", new { date });
                        Console.WriteLine($"날짜: {date}, 종목 수: {count}개");
                    }

                    var rawDate = connection.QuerySingleOrDefault<string>(
                        "SELECT MAX(SearchDate) FROM StockAnalysis WHERE SearchDate IS NOT NULL");

                    if (string.IsNullOrEmpty(rawDate))
                    {
                        Console.WriteLine("❌ 분석 데이터가 없습니다.");
                        return DateTime.Today.ToString("yyyy-MM-dd");
                    }

                    Console.WriteLine($"원본 최신 분석일자: {rawDate}");

                    // 임시로 영업일 보정 비활성화 (디버깅용)
                    // TODO: 영업일 보정 로직 수정 후 다시 활성화
                    /*
                    // 영업일 보정 로직 적용
                    DateTime analysisDate = DateTime.Parse(rawDate);
                    string correctedDate = CorrectToBusinessDay(analysisDate);
                    
                    Console.WriteLine($"원본 분석일자: {rawDate} → 보정된 분석일자: {correctedDate}");
                    return correctedDate;
                    */

                    // 임시: 원본 날짜 그대로 반환
                    Console.WriteLine($"임시: 원본 날짜 그대로 사용 - {rawDate}");
                    return rawDate;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"최신 분석일자 조회 오류: {ex.Message}");
                return DateTime.Today.ToString("yyyy-MM-dd");
            }
        }

        /// <summary>
        /// 영업일 보정 (휴일/주말을 이전 영업일로 변환)
        /// </summary>
        private static string CorrectToBusinessDay(DateTime inputDate)
        {
            try
            {
                // 입력된 날짜가 영업일이면 그대로 반환
                if (IsBusinessDay(inputDate))
                    return inputDate.ToString("yyyy-MM-dd");

                // 영업일이 아니면 이전 영업일 찾기
                DateTime correctedDate = GetPreviousBusinessDay(inputDate);
                return correctedDate.ToString("yyyy-MM-dd");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"영업일 보정 오류: {ex.Message}");
                return inputDate.ToString("yyyy-MM-dd");
            }
        }

        /// <summary>
        /// 영업일 여부 확인 (간단한 로직)
        /// </summary>
        private static bool IsBusinessDay(DateTime date)
        {
            // 주말 체크
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                return false;

            // 2025년 주요 휴일 체크 (간단 버전)
            string dateStr = date.ToString("MM-dd");
            string[] holidays2025 = {
                "01-01", "03-01", "05-05", "06-06", "08-15", "10-03", "10-09", "12-25",
                "01-28", "01-29", "01-30", // 설날 연휴
                "09-28", "09-29", "09-30"  // 추석 연휴
            };

            return !holidays2025.Contains(dateStr);
        }

        /// <summary>
        /// 이전 영업일 찾기
        /// </summary>
        private static DateTime GetPreviousBusinessDay(DateTime date)
        {
            DateTime previousDay = date.AddDays(-1);

            while (!IsBusinessDay(previousDay))
            {
                previousDay = previousDay.AddDays(-1);
            }

            return previousDay;
        }

        /// <summary>
        /// 특정 등급의 종목 조회
        /// </summary>
        public static List<TradingStock> GetStocksByGrade(string grade, string analysisDate = null)
        {
            try
            {
                if (string.IsNullOrEmpty(analysisDate))
                    analysisDate = GetLatestAnalysisDate();

                using (var connection = new SQLiteConnection(StockTrader3ConnectionString))
                {
                    connection.Open();

                    string sql = @"
                        SELECT 
                            SearchDate as AnalysisDate,
                            StockCode, 
                            StockName,
                            FinalScore,
                            FinalGrade,
                            TechnicalScore,
                            NewsScore,
                            ClosePrice,
                            ChangeAmount,
                            ChangeRate,
                            Volume,
                            BuyPrice,
                            SellPrice,
                            StopLossPrice,
                            ExpectedReturn,
                            RiskLevel,
                            Recommendation
                        FROM StockAnalysis 
                        WHERE SearchDate = @analysisDate 
                          AND FinalGrade = @grade
                          AND BuyPrice > 0 
                          AND SellPrice > 0
                          AND StopLossPrice > 0
                        ORDER BY FinalScore DESC";

                    var stocks = connection.Query<TradingStock>(sql, new
                    {
                        analysisDate = analysisDate,
                        grade = grade
                    }).ToList();

                    return stocks;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"종목 조회 오류 ({grade}등급): {ex.Message}");
                return new List<TradingStock>();
            }
        }

        /// <summary>
        /// S/A등급 전체 종목 조회
        /// </summary>
        public static List<TradingStock> GetTopGradeStocks(string analysisDate = null)
        {
            var sStocks = GetStocksByGrade("S", analysisDate);
            var aStocks = GetStocksByGrade("A", analysisDate);

            var allStocks = new List<TradingStock>();
            allStocks.AddRange(sStocks);
            allStocks.AddRange(aStocks);

            return allStocks.OrderByDescending(s => s.FinalScore).ToList();
        }

        /// <summary>
        /// 분석 통계 조회
        /// </summary>
        public static AnalysisStatistics GetAnalysisStatistics(string analysisDate = null)
        {
            try
            {
                if (string.IsNullOrEmpty(analysisDate))
                    analysisDate = GetLatestAnalysisDate();

                using (var connection = new SQLiteConnection(StockTrader3ConnectionString))
                {
                    connection.Open();

                    string sql = @"
                        SELECT 
                            COUNT(*) as TotalCount,
                            COUNT(CASE WHEN FinalGrade = 'S' THEN 1 END) as SGradeCount,
                            COUNT(CASE WHEN FinalGrade = 'A' THEN 1 END) as AGradeCount,
                            COUNT(CASE WHEN FinalGrade = 'B' THEN 1 END) as BGradeCount,
                            COUNT(CASE WHEN FinalGrade = 'C' THEN 1 END) as CGradeCount,
                            COUNT(CASE WHEN BuyPrice > 0 AND SellPrice > 0 THEN 1 END) as TradableCount,
                            AVG(CASE WHEN FinalGrade IN ('S','A') THEN FinalScore END) as AvgTopScore
                        FROM StockAnalysis 
                        WHERE SearchDate = @analysisDate";

                    var stats = connection.QuerySingle<AnalysisStatistics>(sql, new
                    {
                        analysisDate = analysisDate
                    });

                    stats.AnalysisDate = analysisDate;
                    return stats;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"분석 통계 조회 오류: {ex.Message}");
                return new AnalysisStatistics { AnalysisDate = analysisDate ?? "Unknown" };
            }
        }

        #endregion

        #region 유틸리티

        /// <summary>
        /// 데이터베이스 파일 정보 조회
        /// </summary>
        public static DatabaseInfo GetDatabaseInfo()
        {
            var info = new DatabaseInfo();

            // StockTrader3.db 정보
            if (File.Exists(StockTrader3DbPath))
            {
                var fileInfo = new FileInfo(StockTrader3DbPath);
                info.StockTrader3DbPath = StockTrader3DbPath;
                info.StockTrader3DbSize = fileInfo.Length;
                info.StockTrader3LastModified = fileInfo.LastWriteTime;
                info.StockTrader3Exists = true;
            }

            // AutoTrader.db 정보
            if (File.Exists(AutoTraderDbPath))
            {
                var fileInfo = new FileInfo(AutoTraderDbPath);
                info.AutoTraderDbPath = AutoTraderDbPath;
                info.AutoTraderDbSize = fileInfo.Length;
                info.AutoTraderLastModified = fileInfo.LastWriteTime;
                info.AutoTraderExists = true;
            }
            else
            {
                info.AutoTraderDbPath = AutoTraderDbPath;
            }

            return info;
        }

        #endregion
    }

    #region 데이터 모델

    /// <summary>
    /// 매매 대상 종목 정보
    /// </summary>
    public class TradingStock
    {
        public string AnalysisDate { get; set; }
        public string StockCode { get; set; }
        public string StockName { get; set; }

        // 200점 분석 결과
        public int FinalScore { get; set; }
        public string FinalGrade { get; set; }
        public int TechnicalScore { get; set; }
        public int NewsScore { get; set; }

        // 현재 주가 정보
        public int ClosePrice { get; set; }
        public int ChangeAmount { get; set; }
        public double ChangeRate { get; set; }
        public long Volume { get; set; }

        // ATR 기반 매매가
        public int BuyPrice { get; set; }
        public int SellPrice { get; set; }
        public int StopLossPrice { get; set; }
        public double ExpectedReturn { get; set; }

        // 추가 정보
        public string RiskLevel { get; set; }
        public string Recommendation { get; set; }

        // AutoTrader 설정
        public bool IsSelected { get; set; }
        public decimal InvestmentAmount { get; set; }
        public int Priority { get; set; }
    }

    /// <summary>
    /// 분석 통계 정보
    /// </summary>
    public class AnalysisStatistics
    {
        public string AnalysisDate { get; set; }
        public int TotalCount { get; set; }
        public int SGradeCount { get; set; }
        public int AGradeCount { get; set; }
        public int BGradeCount { get; set; }
        public int CGradeCount { get; set; }
        public int TradableCount { get; set; }
        public double AvgTopScore { get; set; }
    }

    /// <summary>
    /// 데이터베이스 파일 정보
    /// </summary>
    public class DatabaseInfo
    {
        public string StockTrader3DbPath { get; set; }
        public long StockTrader3DbSize { get; set; }
        public DateTime StockTrader3LastModified { get; set; }
        public bool StockTrader3Exists { get; set; }

        public string AutoTraderDbPath { get; set; }
        public long AutoTraderDbSize { get; set; }
        public DateTime AutoTraderLastModified { get; set; }
        public bool AutoTraderExists { get; set; }
    }

    #endregion
}