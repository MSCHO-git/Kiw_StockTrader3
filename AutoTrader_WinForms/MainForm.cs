using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;
using AutoTrader_WinForms.Managers;
using AxKHOpenAPILib;


namespace AutoTrader_WinForms
{
    public partial class MainForm : Form
    {
        #region 필드

        // 🆕 추가: ActiveX 컨트롤 필드 (맨 위에 추가)
        private AxKHOpenAPI axKHOpenAPI1;

        // 기존 필드들
        private List<TradingStock> currentStocks;
        private AnalysisStatistics currentStats;
        private List<TradingPosition> tradingPositions;
        private Timer monitoringTimer;
        private Random random = new Random(); // 시뮬레이션용

        // 🆕 키움 API 관련 필드
        private KiwoomApiManager kiwoomApi;
        private bool isKiwoomConnected = false;

        // 🆕 자동 초기화 상태
        private bool isDbConnected = false;
        private bool isInitializing = true;

        // 🆕 실제 매매용 필드 추가
        private List<RealTradingPosition> realTradingPositions;
        private DailyTradingManager dailyTradingManager;
        private Timer realTradingTimer;
        private bool isRealTradingActive = false;

        #endregion

        #region 생성자 및 초기화

        public MainForm()
        {
            InitializeComponent();

            // 🆕 추가: ActiveX 컨트롤 먼저 생성
            CreateKiwoomApiControl();

            InitializeUI();
            InitializeMonitoring();
            InitializeKiwoom();

            // 🆕 자동 DB 연결 체크 (백그라운드)
            Task.Run(() => AutoInitializeDatabase());
        }

        /// <summary>
        /// 🆕 추가: StockTrader3과 동일한 ActiveX 생성 방식
        /// </summary>
        private void CreateKiwoomApiControl()
        {
            try
            {
                // 키움 API 컨트롤 생성
                axKHOpenAPI1 = new AxKHOpenAPI();
                ((System.ComponentModel.ISupportInitialize)(axKHOpenAPI1)).BeginInit();

                axKHOpenAPI1.Enabled = true;
                axKHOpenAPI1.Location = new Point(0, 0);
                axKHOpenAPI1.Name = "axKHOpenAPI1";
                axKHOpenAPI1.OcxState = null;
                axKHOpenAPI1.Size = new Size(100, 50);
                axKHOpenAPI1.TabIndex = 0;
                axKHOpenAPI1.Visible = false; // 보이지 않게 설정

                // 🔑 핵심: Form에 추가!
                this.Controls.Add(axKHOpenAPI1);
                ((System.ComponentModel.ISupportInitialize)(axKHOpenAPI1)).EndInit();

                System.Diagnostics.Debug.WriteLine("✅ AutoTrader 키움 API 컨트롤 생성 완료");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ 키움 API 컨트롤 생성 실패: {ex.Message}");
                throw new Exception("키움 API 컨트롤 생성 실패: " + ex.Message);
            }
        }

        private void InitializeUI()
        {
            // DataGridView 컬럼 설정
            SetupDataGridView();
            SetupMonitoringGridView();

            // 🆕 초기 상태 설정
            btnLoadData.Enabled = false; // DB 연결 완료 후 활성화
            btnStartTrading.Enabled = false;
            grpFilter.Enabled = false;
            btnApplyFilter.Enabled = false;

            // 초기 메시지
            AddLog("🚀 AutoTrader 시스템 시작");
            AddLog("🔄 데이터베이스 연결을 확인하는 중...");
        }

        private void InitializeMonitoring()
        {
            tradingPositions = new List<TradingPosition>();

            // 🆕 실제 매매용 초기화 추가
            realTradingPositions = new List<RealTradingPosition>();
            dailyTradingManager = new DailyTradingManager();

            // 모니터링 타이머 설정 (3초마다 업데이트)
            monitoringTimer = new Timer();
            monitoringTimer.Interval = 3000;
            monitoringTimer.Tick += MonitoringTimer_Tick;

            // 🆕 실제 매매 타이머 설정 (30초마다)
            realTradingTimer = new Timer();
            realTradingTimer.Interval = 30000; // 30초
            realTradingTimer.Tick += RealTradingTimer_Tick;
        }


        private void InitializeKiwoom()
        {
            try
            {
                // 🆕 ActiveX 컨트롤을 매개변수로 전달
                kiwoomApi = new KiwoomApiManager(this, axKHOpenAPI1);

                if (kiwoomApi.Initialize()) // 파라미터 없는 Initialize 호출
                {
                    AddLog("🔧 키움 API 관리자 준비 완료 (ActiveX 연결됨)");
                    UpdateKiwoomStatus();
                }
                else
                {
                    AddLog("⚠️ 키움 API 관리자 초기화 실패");
                }
            }
            catch (Exception ex)
            {
                AddLog($"⚠️ 키움 API 초기화 중 오류: {ex.Message}");
            }
        }

       

        /// <summary>
        /// 🆕 백그라운드에서 자동 DB 연결 체크
        /// </summary>
        private async Task AutoInitializeDatabase()
        {
            try
            {
                await Task.Delay(1000); // 1초 대기 (UI 로딩 완료 후)

                // DB 연결 테스트
                bool success = DatabaseManager.TestConnection(out string message);

                // UI 스레드에서 결과 처리
                this.Invoke(new Action(() =>
                {
                    isInitializing = false;

                    if (success)
                    {
                        // ✅ DB 연결 성공
                        isDbConnected = true;
                        lblConnectionStatus.Text = "DB 연결: ✅ 성공";
                        lblConnectionStatus.ForeColor = Color.Green;
                        btnLoadData.Enabled = true;

                        AddLog($"✅ {message}");
                        AddLog("📋 AutoTrader.db 자동 초기화 완료");

                        var dbInfo = DatabaseManager.GetDatabaseInfo();
                        AddLog($"📂 StockTrader3.db: {dbInfo.StockTrader3DbSize / (1024 * 1024):F1}MB");
                        AddLog($"📅 마지막 수정: {dbInfo.StockTrader3LastModified:yyyy-MM-dd HH:mm:ss}");

                        if (dbInfo.AutoTraderExists)
                        {
                            AddLog($"💾 AutoTrader.db: {dbInfo.AutoTraderDbSize / 1024:F1}KB");
                        }

                        AddLog("🎯 [데이터 로드] 버튼을 클릭하여 최신 분석 결과를 확인하세요.");
                    }
                    else
                    {
                        // ❌ DB 연결 실패
                        isDbConnected = false;
                        lblConnectionStatus.Text = "DB 연결: ❌ 실패";
                        lblConnectionStatus.ForeColor = Color.Red;
                        btnLoadData.Enabled = false;

                        AddLog($"❌ {message}");
                        AddLog("⚠️ StockTrader3.db 파일을 찾을 수 없습니다.");
                        AddLog("📁 StockTrader3 프로그램을 먼저 실행하여 분석 데이터를 생성해주세요.");
                    }
                }));
            }
            catch (Exception ex)
            {
                this.Invoke(new Action(() =>
                {
                    isInitializing = false;
                    AddLog($"❌ 자동 초기화 중 오류: {ex.Message}");
                    lblConnectionStatus.Text = "DB 연결: ❌ 오류";
                    lblConnectionStatus.ForeColor = Color.Red;
                }));
            }
        }

        #endregion

        #region DataGridView 설정

        private void SetupDataGridView()
        {
            // DataGridView 기본 설정
            dgvStocks.AutoGenerateColumns = false;
            dgvStocks.Columns.Clear();

            // 🆕 체크박스 컬럼 추가 (첫 번째)
            dgvStocks.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "IsSelected",
                DataPropertyName = "IsSelected",
                HeaderText = "선택",
                Width = 50,
                ReadOnly = false
            });

            // 기존 컬럼들 (크기 조정)
            dgvStocks.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "StockCode",
                DataPropertyName = "StockCode",
                HeaderText = "코드",
                Width = 70
            });

            dgvStocks.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "StockName",
                DataPropertyName = "StockName",
                HeaderText = "종목명",
                Width = 100
            });

            dgvStocks.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "FinalGrade",
                DataPropertyName = "FinalGrade",
                HeaderText = "등급",
                Width = 40
            });

            dgvStocks.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "FinalScore",
                DataPropertyName = "FinalScore",
                HeaderText = "점수",
                Width = 50
            });

            dgvStocks.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ClosePrice",
                DataPropertyName = "ClosePrice",
                HeaderText = "현재가",
                Width = 70,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" }
            });

            dgvStocks.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "BuyPrice",
                DataPropertyName = "BuyPrice",
                HeaderText = "매수가",
                Width = 70,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" }
            });

            dgvStocks.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "SellPrice",
                DataPropertyName = "SellPrice",
                HeaderText = "목표가",
                Width = 70,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" }
            });

            dgvStocks.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ExpectedReturn",
                DataPropertyName = "ExpectedReturn",
                HeaderText = "예상수익률",
                Width = 80,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "P2" }
            });
        }

        private void SetupMonitoringGridView()
        {
            dgvMonitoring.AutoGenerateColumns = false;
            dgvMonitoring.Columns.Clear();

            dgvMonitoring.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "StockName",
                DataPropertyName = "StockName",
                HeaderText = "종목명",
                Width = 80
            });

            dgvMonitoring.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Status",
                DataPropertyName = "StatusDisplay",
                HeaderText = "상태",
                Width = 80
            });

            dgvMonitoring.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CurrentPrice",
                DataPropertyName = "CurrentPrice",
                HeaderText = "현재가",
                Width = 70,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" }
            });

            dgvMonitoring.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ProfitLoss",
                DataPropertyName = "ProfitLoss",
                HeaderText = "손익",
                Width = 70,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" }
            });

            dgvMonitoring.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ReturnRate",
                DataPropertyName = "ReturnRate",
                HeaderText = "수익률",
                Width = 60,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "P2" }
            });

            dgvMonitoring.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ElapsedTime",
                DataPropertyName = "ElapsedTimeDisplay",
                HeaderText = "경과시간",
                Width = 80
            });
        }

        #endregion

        #region 🆕 간소화된 이벤트 핸들러

        /// <summary>
        /// 🆕 데이터 로드 (바로 실행)
        /// </summary>
        private void btnLoadData_Click(object sender, EventArgs e)
        {
            if (!isDbConnected)
            {
                AddLog("❌ 데이터베이스 연결이 필요합니다.");
                return;
            }

            try
            {
                AddLog("📊 분석 데이터 로드 중...");

                string latestDate = DatabaseManager.GetLatestAnalysisDate();
                AddLog($"📅 최신 분석일자: {latestDate}");

                currentStats = DatabaseManager.GetAnalysisStatistics(latestDate);
                UpdateStatisticsUI();

                currentStocks = DatabaseManager.GetTopGradeStocks(latestDate);

                // 🆕 S급 종목 자동 선택
                AutoSelectSGradeStocks();

                AddLog($"🎯 로드 완료: 총 {currentStocks.Count}개 종목 (S급: {currentStocks.Count(s => s.FinalGrade == "S")}개, A급: {currentStocks.Count(s => s.FinalGrade == "A")}개)");

                ApplyCurrentFilter();
                lblLastUpdate.Text = $"마지막 업데이트: {DateTime.Now:HH:mm:ss}";

                // UI 활성화
                btnApplyFilter.Enabled = true;
                grpFilter.Enabled = true;
                btnStartTrading.Enabled = true;

                AddLog("✅ 종목 선택 후 [매매시작] 버튼을 클릭하세요.");
            }
            catch (Exception ex)
            {
                AddLog($"❌ 데이터 로드 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// 🆕 매매 시작 (키움 연결 상태 자동 체크)
        /// </summary>
        private async void btnStartTrading_Click(object sender, EventArgs e)
        {
            if (currentStocks == null || currentStocks.Count == 0)
            {
                AddLog("⚠️ 먼저 분석 데이터를 로드해주세요.");
                return;
            }

            var selectedStocks = currentStocks.Where(s => s.IsSelected).ToList();

            if (selectedStocks.Count == 0)
            {
                AddLog("⚠️ 매매할 종목을 선택해주세요. (체크박스 클릭)");
                MessageBox.Show("매매할 종목을 선택해주세요.\n\n좌측 체크박스를 클릭하여 원하는 종목을 선택하세요.",
                               "종목 선택 필요", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 🆕 키움 연결 상태 자동 체크
            if (!isKiwoomConnected)
            {
                AddLog("🔐 키움증권 연결이 필요합니다. 자동으로 연결을 시도합니다...");

                // 키움 연결 시도
                bool loginSuccess = await TryKiwoomLogin();

                if (!loginSuccess)
                {
                    var result = MessageBox.Show(
                        "키움증권 연결에 실패했습니다.\n\n" +
                        "시뮬레이션 모드로 진행하시겠습니까?",
                        "연결 실패",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result != DialogResult.Yes)
                    {
                        return; // 사용자가 취소
                    }
                }
            }

            // 매매 모드 확인 및 시작
            string modeText = isKiwoomConnected ? "실제 매매" : "시뮬레이션";
            string warningText = isKiwoomConnected ?
                "⚠️ 실제 계좌에서 매매가 실행됩니다! (모의투자)" :
                "ℹ️ 시뮬레이션 모드로 실행됩니다.";

            string cashInfo = isKiwoomConnected ?
                $"사용 가능 금액: {kiwoomApi.AvailableCash:N0}원\n" : "";

            var confirmResult = MessageBox.Show(
                $"{modeText}를 시작하시겠습니까?\n\n" +
                $"선택 종목: {selectedStocks.Count}개\n" +
                $"대상 종목: {string.Join(", ", selectedStocks.Take(3).Select(s => s.StockName))}" +
                (selectedStocks.Count > 3 ? "..." : "") + "\n" +
                cashInfo + "\n" +
                warningText,
                $"{modeText} 시작 확인",
                MessageBoxButtons.YesNo,
                isKiwoomConnected ? MessageBoxIcon.Warning : MessageBoxIcon.Question);

            if (confirmResult == DialogResult.Yes)
            {
                if (isKiwoomConnected)
                {
                    StartRealTrading(selectedStocks);
                }
                else
                {
                    StartSimulationTrading(selectedStocks);
                }
            }
        }

        /// <summary>
        /// 🆕 키움 로그인 시도
        /// </summary>
        private async Task<bool> TryKiwoomLogin()
        {
            try
            {
                AddLog("🔐 키움증권 로그인 시도 중...");
                AddLog("⏳ 키움 로그인 창이 나타날 예정입니다. 잠시만 기다려주세요...");

                // 비동기로 로그인 처리
                bool loginResult = await Task.Run(() => kiwoomApi.Login());

                if (loginResult)
                {
                    // 로그인 성공
                    isKiwoomConnected = true;
                    UpdateKiwoomStatus();
                    AddLog("✅ 키움증권 연결 완료! 실제 매매가 가능합니다.");
                    return true;
                }
                else
                {
                    // 로그인 실패
                    AddLog("❌ 키움증권 로그인 실패");
                    return false;
                }
            }
            catch (Exception ex)
            {
                AddLog($"❌ 키움 연결 중 오류: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region 매매 관련 로직

        /// <summary>
        /// 실제 매매 시작 (완전히 새로운 버전)
        /// </summary>
        private async void StartRealTrading(List<TradingStock> selectedStocks)
        {
            AddLog($"🚀 실제 매매 시작! {selectedStocks.Count}개 종목으로 키움 API를 통한 매매를 시작합니다.");
            AddLog($"💰 사용 가능 금액: {kiwoomApi.AvailableCash:N0}원");
            AddLog($"🏦 계좌: {kiwoomApi.CurrentAccount}");
            AddLog($"🎯 모의투자 모드로 안전하게 실행됩니다.");

            // UI 상태 변경
            SwitchToTradingMode(isRealTrading: true);

            // [버그 수정] 매매 시작 시 대시보드 상태 즉시 업데이트
            UpdateStrategyDashboard();

            // 실제 매매 포지션 초기화
            InitializeRealTradingPositions(selectedStocks);

            // 실제 매매 플래그 설정
            isRealTradingActive = true;

            // 실제 매매 타이머 시작
            realTradingTimer.Start();

            AddLog("📊 실제 매매 모니터링이 시작되었습니다. (30초마다 업데이트)");

            // 첫 번째 매수 시도
            await ExecuteInitialBuyOrders();
        }

        /// <summary>
        /// 시뮬레이션 매매 시작
        /// </summary>
        private void StartSimulationTrading(List<TradingStock> selectedStocks, bool isRealTrading = false)
        {

            // [버그 수정] 실제 매매 타이머가 동작하고 있을 경우를 대비해 명시적으로 중지
            realTradingTimer.Stop();

            string modeText = isRealTrading ? "실제 매매" : "시뮬레이션";
            AddLog($"🎮 {modeText} 시작! {selectedStocks.Count}개 종목으로 {modeText.ToLower()}을 시작합니다.");

            // UI 상태 변경
            SwitchToTradingMode(isRealTrading);

            // 포지션 초기화
            InitializeTradingPositions(selectedStocks);

            // 모니터링 시작
            monitoringTimer.Start();

            AddLog("📊 실시간 모니터링이 시작되었습니다. (3초마다 업데이트)");
        }

        private void btnStopTrading_Click(object sender, EventArgs e)
        {
            string modeText = isKiwoomConnected ? "실제 매매" : "시뮬레이션";

            var result = MessageBox.Show(
                $"{modeText}를 중단하시겠습니까?\n\n현재 진행 중인 포지션들이 모두 정리됩니다.",
                $"{modeText} 중단 확인",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                StopTrading();
            }
        }

        private void StopTrading()
        {
            // 타이머 중지
            monitoringTimer.Stop();

            // 🆕 실제 매매 중단 추가
            if (isRealTradingActive)
            {
                isRealTradingActive = false;
                realTradingTimer?.Stop();
                AddLog("🛑 실제 매매 중단됨");
            }

            // 모든 포지션 정리
            foreach (var position in tradingPositions)
            {
                if (position.Status == PositionStatus.Holding || position.Status == PositionStatus.Buying)
                {
                    position.Status = PositionStatus.ForceClosed;
                    position.EndTime = DateTime.Now;
                }
            }

            // [1단계] 매매 중단 시 대시보드 상태 즉시 업데이트
            UpdateStrategyDashboard();

            // UI 상태 변경
            SwitchToSelectionMode();

            string modeText = isKiwoomConnected ? "실제 매매" : "시뮬레이션";
            AddLog($"🛑 {modeText}가 중단되었습니다. 모든 포지션이 정리되었습니다.");
        }

        #endregion

        /// <summary>
        /// [1단계] 전략 제어 및 핵심 상태 보드를 업데이트하는 메서드
        /// </summary>
        private void UpdateStrategyDashboard()
        {
            // UI 컨트롤은 메인 스레드에서만 접근해야 하므로,
            // 다른 스레드에서 호출될 경우를 대비해 Invoke를 사용합니다.
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(UpdateStrategyDashboard));
                return;
            }

            // --- 1. 일일 손실 한도 패널 업데이트 ---
            if (dailyTradingManager != null)
            {
                decimal currentRealizedPL = dailyTradingManager.DailyRealized;
                // TradingModels.cs에 정의된 일일 손실 한도 금액
                decimal lossLimit = -600000m;

                // 라벨 텍스트 업데이트 (N0 포맷은 천단위 콤마)
                lblDailyLossStatus.Text = $"{currentRealizedPL:N0} / {lossLimit:N0} 원";

                // 손실률에 따라 배경색 변경
                if (currentRealizedPL >= 0) // 수익 중
                {
                    pnlDailyLoss.BackColor = Color.FromArgb(220, 255, 220); // 연한 녹색
                }
                else // 손실 중
                {
                    // 손실 한도 대비 현재 손실 비율 계산
                    double lossRatio = (double)(currentRealizedPL / lossLimit);

                    if (lossRatio < 0.5) // 손실 한도의 50% 미만
                    {
                        pnlDailyLoss.BackColor = Color.FromArgb(220, 255, 220); // 연한 녹색
                    }
                    else if (lossRatio < 0.8) // 손실 한도의 80% 미만
                    {
                        pnlDailyLoss.BackColor = Color.FromArgb(255, 255, 200); // 노란색
                    }
                    else // 손실 한도의 80% 이상
                    {
                        pnlDailyLoss.BackColor = Color.FromArgb(255, 220, 220); // 연한 붉은색
                    }
                }

                // 최종적으로 손실 한도에 도달했는지 체크
                if (dailyTradingManager.IsLossLimitReached)
                {
                    lblDailyLossStatus.Text = "일일 한도 도달";
                    pnlDailyLoss.BackColor = Color.Red; // 강렬한 빨간색
                    lblDailyLossStatus.ForeColor = Color.White;
                }
                else
                {
                    lblDailyLossStatus.ForeColor = Color.Black;
                }
            }

            // --- 2. 시스템 상태 패널 업데이트 ---
            if (isRealTradingActive)
            {
                lblSystemStatus.Text = "실제 매매 진행 중";
                pnlSystemStatus.BackColor = Color.LightCoral;
            }
            else
            {
                lblSystemStatus.Text = "매매 대기";
                pnlSystemStatus.BackColor = Color.FromArgb(230, 230, 230); // 회색
            }

            // --- 3. 시장 상태 패널 업데이트 ---
            // (이 부분은 향후 코스피/코스닥 지수 API 연동 시 구현 예정)
            // 예시: lblMarketStatus.Text = $"코스피: +0.5%";
        }



        #region 🆕 실제 매매 로직

        /// <summary>
        /// 실제 매매 포지션 초기화
        /// </summary>
        private void InitializeRealTradingPositions(List<TradingStock> selectedStocks)
        {
            realTradingPositions.Clear();
            dailyTradingManager.ResetDaily();

            foreach (var stock in selectedStocks)
            {
                var position = new RealTradingPosition
                {
                    Stock = stock,
                    StockCode = stock.StockCode,
                    StockName = stock.StockName,
                    PlannedBuyPrice = stock.BuyPrice,
                    PlannedQuantity = CalculateQuantity(stock.BuyPrice),
                    CurrentPrice = stock.ClosePrice,
                    Status = PositionStatus.Ready,
                    BuyTime = DateTime.Now,
                    MaxPrice = stock.ClosePrice,
                    MinPrice = stock.ClosePrice
                };

                realTradingPositions.Add(position);
            }

            AddLog($"✅ {realTradingPositions.Count}개 종목 매매 준비 완료");
        }

        /// <summary>
        /// 수량 계산 (400만원 기준)
        /// </summary>
        private int CalculateQuantity(int price)
        {
            const decimal targetInvestment = 4_000_000m; // 400만원
            if (price <= 0) return 0;
            return (int)(targetInvestment / price);
        }

        /// <summary>
        /// 초기 매수 주문 실행
        /// </summary>
        private async Task ExecuteInitialBuyOrders()
        {
            AddLog("🔵 초기 매수 주문 시작...");

            var readyPositions = realTradingPositions
                .Where(p => p.Status == PositionStatus.Ready)
                .OrderByDescending(p => p.Stock.FinalScore)
                .Take(3) // 처음에는 3개만
                .ToList();

            foreach (var position in readyPositions)
            {
                if (!dailyTradingManager.CanTrade(position.StockCode))
                {
                    AddLog($"⚠️ {position.StockName} 거래 제한 (재매수 방지)");
                    continue;
                }

                await ExecuteRealBuyOrder(position);
                await Task.Delay(1000); // 1초 간격
            }
        }

        /// <summary>
        /// 실제 매수 주문 실행
        /// </summary>
        private async Task<bool> ExecuteRealBuyOrder(RealTradingPosition position)
        {
            try
            {
                AddLog($"🔵 매수 시도: {position.StockName} {position.PlannedQuantity}주 @ {position.PlannedBuyPrice:N0}원");

                position.Status = PositionStatus.Buying;

                // 실제 키움 API 주문 전송
                var orderResult = await kiwoomApi.SendBuyOrder(
                    position.StockCode,
                    position.PlannedQuantity,
                    position.PlannedBuyPrice
                );

                if (orderResult.Success)
                {
                    position.BuyOrderNo = orderResult.OrderNo;

                    // 부분 체결 처리
                    if (orderResult.FilledRatio < 0.7) // 70% 미만 체결
                    {
                        AddLog($"🔶 {position.StockName} 부분체결 포기 (체결률: {orderResult.FilledRatio:P0})");
                        position.Status = PositionStatus.Cancelled;
                        await kiwoomApi.CancelOrder(orderResult.OrderNo);
                        return false;
                    }

                    // 매수 성공
                    position.ActualQuantity = orderResult.FilledQuantity;
                    position.ActualAvgBuyPrice = orderResult.AvgPrice;
                    position.Status = PositionStatus.Holding;

                    dailyTradingManager.AddTrade(position.StockCode, 0); // 매수는 손익 0

                    AddLog($"✅ {position.StockName} 매수 완료: {position.ActualQuantity}주 @ {position.ActualAvgBuyPrice:N0}원");
                    return true;
                }
                else
                {
                    AddLog($"❌ {position.StockName} 매수 실패: {orderResult.ErrorMessage}");
                    position.Status = PositionStatus.Ready; // 다시 시도 가능
                    return false;
                }
            }
            catch (Exception ex)
            {
                AddLog($"❌ {position.StockName} 매수 오류: {ex.Message}");
                position.Status = PositionStatus.Ready;
                return false;
            }
        }

        /// <summary>
        /// 실제 매매 타이머 이벤트
        /// </summary>
        private async void RealTradingTimer_Tick(object sender, EventArgs e)
        {
            // [1단계] 대시보드 UI 업데이트 호출 추가
            UpdateStrategyDashboard();

            if (!isRealTradingActive) return;

            try
            {
                // 1. 기존 포지션 모니터링
                await MonitorRealPositions();

                // 2. 새로운 매수 기회 탐색
                await SearchNewBuyOpportunities();

                // 3. UI 업데이트
                UpdateRealTradingUI();

                // 4. 장 마감 체크 (14:45)
                if (IsCloseToMarketEnd())
                {
                    await ForceCloseAllPositions();
                }
            }
            catch (Exception ex)
            {
                AddLog($"❌ 실제 매매 모니터링 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// 실제 포지션 모니터링
        /// </summary>
        private async Task MonitorRealPositions()
        {
            var holdingPositions = realTradingPositions
                .Where(p => p.Status == PositionStatus.Holding)
                .ToList();

            foreach (var position in holdingPositions)
            {
                // 현재가 조회
                int currentPrice = await kiwoomApi.GetCurrentPrice(position.StockCode);
                if (currentPrice > 0)
                {
                    position.CurrentPrice = currentPrice;
                    position.MaxPrice = Math.Max(position.MaxPrice, currentPrice);
                    position.MinPrice = Math.Min(position.MinPrice, currentPrice);
                }

                // 매도 신호 확인
                var sellSignal = position.CheckSellSignal();

                if (sellSignal != SellSignal.Hold)
                {
                    await ExecuteRealSellOrder(position, sellSignal);
                }
            }
        }

        /// <summary>
        /// 실제 매도 주문 실행
        /// </summary>
        private async Task<bool> ExecuteRealSellOrder(RealTradingPosition position, SellSignal signal)
        {
            try
            {
                string reason = GetSellReasonText(signal);
                int sellPrice = signal == SellSignal.EmergencyExit ? 0 : position.CurrentPrice; // 비상시 시장가

                AddLog($"🔴 매도 시도: {position.StockName} {position.ActualQuantity}주 @ {sellPrice:N0}원 ({reason})");

                position.Status = PositionStatus.Selling;

                var orderResult = await kiwoomApi.SendSellOrder(
                    position.StockCode,
                    position.ActualQuantity,
                    sellPrice
                );

                if (orderResult.Success)
                {
                    position.SellOrderNo = orderResult.OrderNo;
                    position.SellTime = DateTime.Now;
                    position.Status = PositionStatus.Completed;

                    // 실현 손익 계산
                    decimal realizedPL = (orderResult.AvgPrice - position.ActualAvgBuyPrice) * orderResult.FilledQuantity;
                    dailyTradingManager.AddTrade(position.StockCode, realizedPL);

                    AddLog($"✅ {position.StockName} 매도 완료: {orderResult.FilledQuantity}주 @ {orderResult.AvgPrice:N0}원");
                    AddLog($"💰 실현손익: {realizedPL:+#,0;-#,0;0}원 ({position.ReturnRate:+0.00%;-0.00%;0.00%})");

                    return true;
                }
                else
                {
                    AddLog($"❌ {position.StockName} 매도 실패: {orderResult.ErrorMessage}");
                    position.Status = PositionStatus.Holding; // 다시 시도
                    return false;
                }
            }
            catch (Exception ex)
            {
                AddLog($"❌ {position.StockName} 매도 오류: {ex.Message}");
                position.Status = PositionStatus.Holding;
                return false;
            }
        }

        /// <summary>
        /// 새로운 매수 기회 탐색
        /// </summary>
        private async Task SearchNewBuyOpportunities()
        {
            var holdingCount = realTradingPositions.Count(p => p.Status == PositionStatus.Holding);
            if (holdingCount >= 10) return; // 최대 10개 보유

            var readyPositions = realTradingPositions
                .Where(p => p.Status == PositionStatus.Ready)
                .Where(p => dailyTradingManager.CanTrade(p.StockCode))
                .OrderByDescending(p => p.Stock.FinalScore)
                .Take(1) // 한 번에 하나씩
                .ToList();

            foreach (var position in readyPositions)
            {
                await ExecuteRealBuyOrder(position);
                break; // 하나만 실행
            }
        }

        /// <summary>
        /// 강제 청산 (14:45 이후)
        /// </summary>
        private async Task ForceCloseAllPositions()
        {
            AddLog("🔔 장 마감 15분 전: 모든 포지션 강제 정리 시작");

            var holdingPositions = realTradingPositions
                .Where(p => p.Status == PositionStatus.Holding)
                .ToList();

            foreach (var position in holdingPositions)
            {
                await ExecuteRealSellOrder(position, SellSignal.TimeLimit);
                await Task.Delay(500); // 0.5초 간격
            }

            // 매매 중단
            isRealTradingActive = false;
            realTradingTimer.Stop();

            AddLog("🏁 실제 매매 완료!");

            // [1단계] 매매 완료 시 대시보드 상태 즉시 업데이트
            UpdateStrategyDashboard();

            ShowRealTradingResults();
        }

        /// <summary>
        /// 실제 매매 UI 업데이트
        /// </summary>
        private void UpdateRealTradingUI()
        {
            if (realTradingPositions == null || realTradingPositions.Count == 0) return;

            // 기존 모니터링 UI를 실제 매매 데이터로 업데이트
            var displayPositions = realTradingPositions.Select(rp => new TradingPosition
            {
                Stock = rp.Stock,
                InvestmentAmount = rp.InvestmentAmount,
                BuyPrice = rp.ActualAvgBuyPrice > 0 ? rp.ActualAvgBuyPrice : rp.PlannedBuyPrice,
                TargetPrice = rp.Stock.SellPrice,
                CurrentPrice = rp.CurrentPrice,
                Quantity = rp.ActualQuantity > 0 ? rp.ActualQuantity : rp.PlannedQuantity,
                Status = rp.Status,
                StartTime = rp.BuyTime,
                EndTime = rp.SellTime
            }).ToList();

            dgvMonitoring.DataSource = null;
            dgvMonitoring.DataSource = displayPositions;

            // 통계 업데이트
            var totalInvestment = realTradingPositions.Sum(p => p.InvestmentAmount);
            var totalProfit = realTradingPositions.Sum(p => p.UnrealizedPL);
            var dailyRealized = dailyTradingManager.DailyRealized;

            lblTotalInvestment.Text = $"💰 투자금액: {totalInvestment:N0}원";
            lblCurrentProfit.Text = $"📈 현재수익: {totalProfit:+#,0;-#,0;0}원";
            lblReturnRate.Text = $"📊 실현손익: {dailyRealized:+#,0;-#,0;0}원";
            lblProgress.Text = $"🎯 거래: {dailyTradingManager.TradingCount}회";
        }

        /// <summary>
        /// 실제 매매 결과 표시
        /// </summary>
        private void ShowRealTradingResults()
        {
            var completedCount = realTradingPositions.Count(p => p.Status == PositionStatus.Completed);
            var totalRealized = dailyTradingManager.DailyRealized;
            var tradingCount = dailyTradingManager.TradingCount;

            string resultMessage = $"실제 매매 결과 요약\n\n" +
                                 $"총 거래: {tradingCount}회\n" +
                                 $"완료: {completedCount}개 종목\n" +
                                 $"실현손익: {totalRealized:+#,0;-#,0;0}원\n\n" +
                                 $"모의투자 모드로 안전하게 실행되었습니다.";

            MessageBox.Show(resultMessage, "실제 매매 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 유틸리티 메서드들
        /// </summary>
        private string GetSellReasonText(SellSignal signal)
        {
            switch (signal)
            {
                case SellSignal.ProfitTarget: return "목표가 달성";
                case SellSignal.StopLoss: return "손절가 도달";
                case SellSignal.EmergencyExit: return "비상 매도";
                case SellSignal.TimeLimit: return "시간 초과";
                default: return "기타";
            }
        }

        private bool IsCloseToMarketEnd()
        {
            var now = DateTime.Now.TimeOfDay;
            return now >= TimeSpan.Parse("14:45"); // 14:45 이후
        }

        #endregion

        #region UI 상태 관리

        private void SwitchToTradingMode(bool isRealTrading = false)
        {
            // DataGridView 크기 조정
            dgvStocks.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;

            // 모니터링 패널 표시
            grpMonitoring.Visible = true;

            // 실제 매매인 경우 제목 변경
            if (isRealTrading)
            {
                grpMonitoring.Text = "🔴 실시간 실제 매매 모니터링";
                grpMonitoring.ForeColor = Color.Red;
            }
            else
            {
                grpMonitoring.Text = "🎮 실시간 시뮬레이션 모니터링";
                grpMonitoring.ForeColor = Color.Blue;
            }

            // 버튼 상태 변경
            btnStartTrading.Visible = false;
            btnStopTrading.Visible = true;
            btnStopTrading.Enabled = true;

            // 종목 선택 비활성화
            grpFilter.Enabled = false;
            btnApplyFilter.Enabled = false;
            btnLoadData.Enabled = false;
        }

        private void SwitchToSelectionMode()
        {
            // DataGridView 크기 복원
            dgvStocks.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // 모니터링 패널 숨김
            grpMonitoring.Visible = false;

            // 버튼 상태 복원
            btnStartTrading.Visible = true;
            btnStopTrading.Visible = false;

            // 종목 선택 활성화
            grpFilter.Enabled = true;
            btnApplyFilter.Enabled = true;
            btnLoadData.Enabled = true;
        }

        #endregion

        #region 기존 UI 이벤트 핸들러

        private void dgvStocks_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 0) // 체크박스 클릭
            {
                var selectedStock = (TradingStock)dgvStocks.Rows[e.RowIndex].DataBoundItem;

                this.BeginInvoke(new Action(() =>
                {
                    var checkedCount = currentStocks?.Count(s => s.IsSelected) ?? 0;
                    AddLog($"💡 {selectedStock.StockName} {(selectedStock.IsSelected ? "선택" : "해제")} (총 {checkedCount}개 선택됨)");
                }));
            }
            else if (e.RowIndex >= 0)
            {
                var selectedStock = (TradingStock)dgvStocks.Rows[e.RowIndex].DataBoundItem;
                AddLog($"📊 종목 정보: {selectedStock.StockName} ({selectedStock.FinalGrade}등급, {selectedStock.FinalScore}점) - 매수가: {selectedStock.BuyPrice:N0}원");
            }
        }

        private void dgvStocks_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            e.CellStyle.ForeColor = Color.Black;

            var grade = dgvStocks.Rows[e.RowIndex].Cells["FinalGrade"].Value?.ToString();
            if (grade == "S" || grade == "A")
            {
                e.CellStyle.Font = new Font(dgvStocks.Font, FontStyle.Bold);
            }
            else
            {
                e.CellStyle.Font = new Font(dgvStocks.Font, FontStyle.Regular);
            }
        }

        private void btnApplyFilter_Click(object sender, EventArgs e)
        {
            if (currentStocks == null || currentStocks.Count == 0)
            {
                AddLog("⚠️ 먼저 데이터를 로드해주세요.");
                return;
            }

            ApplyCurrentFilter();
        }

        /// <summary>
        /// 계좌 변경 이벤트
        /// </summary>
        private void cmbAccount_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isKiwoomConnected && cmbAccount.SelectedItem != null)
            {
                string selectedAccount = cmbAccount.SelectedItem.ToString();
                if (kiwoomApi.ChangeAccount(selectedAccount))
                {
                    UpdateKiwoomStatus();
                    AddLog($"🔄 계좌 변경됨: {selectedAccount}");
                }
            }
        }

        #endregion

        #region 시뮬레이션 로직

        private void InitializeTradingPositions(List<TradingStock> selectedStocks)
        {
            tradingPositions.Clear();

            foreach (var stock in selectedStocks)
            {
                var position = new TradingPosition
                {
                    Stock = stock,
                    InvestmentAmount = 1000000, // 100만원
                    BuyPrice = stock.BuyPrice,
                    TargetPrice = stock.SellPrice,
                    CurrentPrice = stock.ClosePrice,
                    Status = PositionStatus.Buying,
                    StartTime = DateTime.Now,
                    Quantity = 1000000 / stock.BuyPrice // 대략적인 수량
                };

                tradingPositions.Add(position);
            }

            UpdateMonitoringUI();
        }

        private void MonitoringTimer_Tick(object sender, EventArgs e)
        {
            SimulateTradingProgress();
            UpdateMonitoringUI();
        }

        private void SimulateTradingProgress()
        {
            foreach (var position in tradingPositions.ToList())
            {
                switch (position.Status)
                {
                    case PositionStatus.Buying:
                        // 30% 확률로 매수 완료
                        if (random.NextDouble() < 0.3)
                        {
                            position.Status = PositionStatus.Holding;
                            AddLog($"✅ {position.Stock.StockName} 매수 완료 - {position.BuyPrice:N0}원");
                        }
                        break;

                    case PositionStatus.Holding:
                        // 가격 변동 시뮬레이션 (-3% ~ +3%)
                        double priceChangeRate = (random.NextDouble() - 0.5) * 0.06; // -3% ~ +3%
                        position.CurrentPrice = (int)(position.BuyPrice * (1 + priceChangeRate));

                        // 목표가 도달 시 익절 (10% 확률)
                        if (position.CurrentPrice >= position.TargetPrice && random.NextDouble() < 0.1)
                        {
                            position.Status = PositionStatus.ProfitTaken;
                            position.EndTime = DateTime.Now;
                            AddLog($"🎉 {position.Stock.StockName} 익절 완료 - {position.CurrentPrice:N0}원 (+{position.ReturnRate:P2})");
                        }
                        // 손절가 도달 시 손절 (5% 확률)
                        else if (position.CurrentPrice <= position.Stock.StopLossPrice && random.NextDouble() < 0.05)
                        {
                            position.Status = PositionStatus.StopLoss;
                            position.EndTime = DateTime.Now;
                            AddLog($"⛔ {position.Stock.StockName} 손절 완료 - {position.CurrentPrice:N0}원 ({position.ReturnRate:P2})");
                        }
                        break;
                }
            }

            // 모든 포지션이 완료되면 타이머 중지
            if (tradingPositions.All(p => p.Status == PositionStatus.ProfitTaken ||
                                         p.Status == PositionStatus.StopLoss ||
                                         p.Status == PositionStatus.ForceClosed))
            {
                monitoringTimer.Stop();
                string modeText = isKiwoomConnected ? "실제 매매" : "시뮬레이션";
                AddLog($"🏁 모든 {modeText.ToLower()}가 완료되었습니다!");

                // 결과 요약
                ShowTradingResults();
            }
        }

        private void ShowTradingResults()
        {
            var profitCount = tradingPositions.Count(p => p.Status == PositionStatus.ProfitTaken);
            var lossCount = tradingPositions.Count(p => p.Status == PositionStatus.StopLoss);
            var totalProfit = tradingPositions.Sum(p => p.ProfitLoss);
            var totalInvestment = tradingPositions.Sum(p => p.InvestmentAmount);
            var totalReturn = totalInvestment > 0 ? (double)totalProfit / (double)totalInvestment : 0;

            string modeText = isKiwoomConnected ? "실제 매매" : "시뮬레이션";

            string resultMessage = $"{modeText} 결과 요약\n\n" +
                                 $"총 {tradingPositions.Count}개 종목\n" +
                                 $"익절: {profitCount}개, 손절: {lossCount}개\n" +
                                 $"총 수익: {totalProfit:N0}원\n" +
                                 $"수익률: {totalReturn:P2}\n\n" +
                                 $"계속 매매하시겠습니까?";

            var result = MessageBox.Show(resultMessage, $"{modeText} 완료",
                                       MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (result == DialogResult.No)
            {
                SwitchToSelectionMode();
            }
        }

        #endregion

        #region UI 업데이트

        private void UpdateMonitoringUI()
        {
            if (tradingPositions == null || tradingPositions.Count == 0) return;

            // 통계 업데이트
            var totalInvestment = tradingPositions.Sum(p => p.InvestmentAmount);
            var totalProfit = tradingPositions.Sum(p => p.ProfitLoss);
            var totalReturn = totalInvestment > 0 ? (double)totalProfit / (double)totalInvestment : 0;
            var completedCount = tradingPositions.Count(p => p.Status == PositionStatus.ProfitTaken ||
                                                            p.Status == PositionStatus.StopLoss ||
                                                            p.Status == PositionStatus.ForceClosed);

            string modeText = isKiwoomConnected ? "실제" : "시뮬";
            lblMonitoringTitle.Text = $"📊 {modeText} 매매 현황 ({tradingPositions.Count}개 선택)";
            lblTotalInvestment.Text = $"💰 투자금액: {totalInvestment:N0}원";
            lblCurrentProfit.Text = $"📈 현재수익: {totalProfit:+#,0;-#,0;0}원";
            lblReturnRate.Text = $"📊 수익률: {totalReturn:+0.00%;-0.00%;0.00%}";
            lblProgress.Text = $"🎯 진행률: {completedCount}/{tradingPositions.Count} 완료";

            // 수익률에 따른 색상 변경
            lblCurrentProfit.ForeColor = totalProfit >= 0 ? Color.Green : Color.Red;
            lblReturnRate.ForeColor = totalReturn >= 0 ? Color.Green : Color.Red;

            // 그리드 업데이트
            dgvMonitoring.DataSource = null;
            dgvMonitoring.DataSource = tradingPositions.ToList();
        }

        private void UpdateStatisticsUI()
        {
            if (currentStats == null) return;

            lblAnalysisDate.Text = $"분석일자: {currentStats.AnalysisDate}";
            lblTotalCount.Text = $"전체: {currentStats.TotalCount}개";
            lblSGrade.Text = $"S급: {currentStats.SGradeCount}개";
            lblAGrade.Text = $"A급: {currentStats.AGradeCount}개";
            lblTradable.Text = $"매매가능: {currentStats.TradableCount}개";
            lblAvgScore.Text = $"평균점수: {currentStats.AvgTopScore:F1}점";
        }

        private void ApplyCurrentFilter()
        {
            if (currentStocks == null) return;

            List<TradingStock> filteredStocks = currentStocks;
            string filterInfo = "";

            if (rbSGrade.Checked)
            {
                filteredStocks = currentStocks.Where(s => s.FinalGrade == "S").ToList();
                filterInfo = "S등급만";
            }
            else if (rbAGrade.Checked)
            {
                filteredStocks = currentStocks.Where(s => s.FinalGrade == "A").ToList();
                filterInfo = "A등급만";
            }
            else if (rbTopGrades.Checked)
            {
                filteredStocks = currentStocks.Where(s => s.FinalGrade == "S" || s.FinalGrade == "A").ToList();
                filterInfo = "S+A등급";
            }
            else
            {
                filterInfo = "전체";
            }

            dgvStocks.DataSource = filteredStocks;
            AddLog($"🔍 필터 적용: {filterInfo} ({filteredStocks.Count}개 종목 표시)");
        }

        /// <summary>
        /// 키움 연결 상태 UI 업데이트
        /// </summary>
        private void UpdateKiwoomStatus()
        {
            if (isKiwoomConnected && kiwoomApi != null)
            {
                // 연결 상태 표시
                lblKiwoomStatus.Text = $"키움 연결: ✅ {kiwoomApi.CurrentUserId}";
                lblKiwoomStatus.ForeColor = Color.Green;

                // 계좌 정보 표시
                lblAccountInfo.Text = $"계좌: {kiwoomApi.CurrentAccount} | 예수금: {kiwoomApi.AvailableCash:N0}원";
                lblAccountInfo.ForeColor = Color.Blue;

                // 계좌 선택 콤보박스 활성화 (여러 계좌가 있는 경우)
                if (kiwoomApi.AccountList.Count > 1)
                {
                    cmbAccount.Enabled = true;
                    cmbAccount.Items.Clear();
                    foreach (string account in kiwoomApi.AccountList)
                    {
                        cmbAccount.Items.Add(account);
                    }
                    cmbAccount.SelectedItem = kiwoomApi.CurrentAccount;
                }
                else
                {
                    cmbAccount.Enabled = false;
                }
            }
            else
            {
                lblKiwoomStatus.Text = "키움 연결: ❌ 미연결 (매매시작 시 자동 연결)";
                lblKiwoomStatus.ForeColor = Color.Red;
                lblAccountInfo.Text = "계좌: - | 예수금: -";
                lblAccountInfo.ForeColor = Color.Gray;
                cmbAccount.Enabled = false;
                cmbAccount.Items.Clear();
            }
        }

        /// <summary>
        /// 로그 메시지 추가
        /// </summary>
        public void AddLog(string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(AddLog), message);
                return;
            }

            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            txtLog.AppendText($"[{timestamp}] {message}\r\n");
            txtLog.ScrollToCaret();
        }

        /// <summary>
        /// S급 종목들 자동 선택
        /// </summary>
        private void AutoSelectSGradeStocks()
        {
            if (currentStocks == null) return;

            int selectedCount = 0;
            foreach (var stock in currentStocks)
            {
                if (stock.FinalGrade == "S")
                {
                    stock.IsSelected = true;
                    selectedCount++;
                }
                else
                {
                    stock.IsSelected = false; // A급은 체크 해제
                }
            }

            AddLog($"✅ S급 {selectedCount}개 종목 자동 선택됨");
        }

        #endregion

        #region Form 종료 시 정리

        /// <summary>
        /// 폼 종료 시 키움 API 정리
        /// </summary>
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try
            {
                // 모니터링 타이머 중지
                monitoringTimer?.Stop();
                monitoringTimer?.Dispose();

                // 실제 매매 타이머 중지
                realTradingTimer?.Stop();
                realTradingTimer?.Dispose();

                // 키움 API 정리
                if (kiwoomApi != null)
                {
                    AddLog("🔌 키움 API 연결 해제 중...");
                    kiwoomApi.Dispose();
                }
            }
            catch (Exception ex)
            {
                AddLog($"❌ 리소스 정리 중 오류: {ex.Message}");
            }

            base.OnFormClosed(e);
        }

        #endregion
    }

    #region 기존 모니터링 모델

    public class TradingPosition
    {
        public TradingStock Stock { get; set; }
        public decimal InvestmentAmount { get; set; }
        public int BuyPrice { get; set; }
        public int TargetPrice { get; set; }
        public int CurrentPrice { get; set; }
        public int Quantity { get; set; }
        public PositionStatus Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        // 계산된 속성들
        public string StockName => Stock?.StockName ?? "";

        public string StatusDisplay
        {
            get
            {
                switch (Status)
                {
                    case PositionStatus.Buying: return "🟡 매수중";
                    case PositionStatus.Holding: return "🟢 보유중";
                    case PositionStatus.ProfitTaken: return "🔵 익절완료";
                    case PositionStatus.StopLoss: return "🔴 손절완료";
                    case PositionStatus.ForceClosed: return "⚫ 강제종료";
                    default: return "⚪ 대기중";
                }
            }
        }

        public decimal ProfitLoss
        {
            get
            {
                if (Status == PositionStatus.Buying) return 0;
                return (CurrentPrice - BuyPrice) * Quantity;
            }
        }

        public double ReturnRate
        {
            get
            {
                if (BuyPrice <= 0) return 0;
                return (double)(CurrentPrice - BuyPrice) / BuyPrice;
            }
        }

        public string ElapsedTimeDisplay
        {
            get
            {
                var elapsed = (EndTime ?? DateTime.Now) - StartTime;
                if (elapsed.TotalMinutes < 60)
                    return $"{elapsed.Minutes}분";
                else
                    return $"{elapsed.Hours}시간{elapsed.Minutes}분";
            }
        }
    }

  

    #endregion
}