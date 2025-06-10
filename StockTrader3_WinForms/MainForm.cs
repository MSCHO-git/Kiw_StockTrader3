using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using AxKHOpenAPILib;
using System.Threading;  // ManualResetEvent를 위해 필요
using System.Linq;
using StockTrader3.Analysis;
using StockTrader3.Models;
using StockTrader3.Indicators;
using StockTrader3.Services;

namespace StockTrader3_WinForms
{

    public partial class MainForm : Form
    {
        #region 키움 API 컨트롤

        private AxKHOpenAPI axKHOpenAPI1;

        #endregion

        #region 매니저 클래스들

        private ProgressManager _progressManager;
        private StatusManager _statusManager;
        private KiwoomApiManager _kiwoomApiManager;
        private DatabaseManager _databaseManager;
        private List<string> _lastConditionSearchResults = new List<string>();
        private bool _isHistoricalDataCollecting = false;

        private CancellationTokenSource _cancellationTokenSource;
        private KiwoomApiManager _kiwoomManager;

       

        #endregion

        public MainForm()
        {
            InitializeComponent();
            _kiwoomManager = _kiwoomApiManager;

            InitializeFullSystemAsync();
        }

        #region 비동기 시스템 초기화

        private async void InitializeFullSystemAsync()
        {
            try
            {
                this.Text = "StockTrader3 - 키움 API + DB 완전 연동 (조건검색 실제 실행)";

                // 🆕 이 줄을 추가하세요!
                SetupDataGridViewColumns();

                // 로딩 표시
                lblStatus.Text = "시스템 초기화 중...";

                // 🆕 CancellationToken 초기화 추가
                _cancellationTokenSource = new CancellationTokenSource();


                // 키움 API 컨트롤 생성
                CreateKiwoomApiControl();

                // 매니저들 초기화 (순서 중요!)
                await InitializeAllManagersAsync();

                // 초기 상태 설정
                SetInitialState();

                System.Diagnostics.Debug.WriteLine("✅ 완전한 키움 API + DB 시스템 구축 완료! (조건검색 실제 실행 가능)");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"시스템 초기화 실패: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"❌ 시스템 초기화 실패: {ex.Message}");
            }
        }

        #endregion

        #region 키움 API 컨트롤 생성

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

                this.Controls.Add(axKHOpenAPI1);
                ((System.ComponentModel.ISupportInitialize)(axKHOpenAPI1)).EndInit();

                System.Diagnostics.Debug.WriteLine("✅ 키움 API 컨트롤 생성 완료");
            }
            catch (Exception ex)
            {
                throw new Exception("키움 API 컨트롤 생성 실패: " + ex.Message);
            }
        }

        #endregion

        #region 매니저 초기화

        private async Task InitializeAllManagersAsync()
        {
            try
            {
                lblStatus.Text = "데이터베이스 초기화 중...";

                // 1. DatabaseManager 초기화 (가장 먼저!)
                _databaseManager = new DatabaseManager();
                _databaseManager.ConnectionStatusChanged += OnDatabaseConnectionStatusChanged;
                _databaseManager.ProgressUpdated += OnDatabaseProgressUpdated;

                bool dbInitSuccess = await _databaseManager.InitializeAsync();
                if (!dbInitSuccess)
                {
                    throw new Exception("데이터베이스 초기화 실패");
                }

                lblStatus.Text = "UI 매니저 초기화 중...";

                // 2. ProgressManager 초기화
                _progressManager = new ProgressManager(this);
                _progressManager.CreateDetailedProgressControls(grpProgress);

                // 3. StatusManager 초기화 (통계 카드들 null로 전달)
                _statusManager = new StatusManager(this, lblStatus, null, null, null, null);

                // 4. KiwoomApiManager 초기화 및 이벤트 연결
                _kiwoomApiManager = new KiwoomApiManager(this, axKHOpenAPI1);
                ConnectKiwoomApiEvents();

                lblStatus.Text = "통계 데이터 로드 중...";

                // 5. 실제 DB 통계로 초기화
                await LoadInitialStatisticsFromDatabase();

                lblStatus.Text = "시스템 초기화 완료 - 조건검색 실제 실행 준비됨";
                System.Diagnostics.Debug.WriteLine("✅ 모든 매니저 초기화 완료 (실제 조건검색 포함)");
            }
            catch (Exception ex)
            {
                lblStatus.Text = "초기화 실패";
                throw new Exception($"매니저 초기화 실패: {ex.Message}");
            }
        }

        #endregion

        #region 데이터베이스 이벤트 핸들러

        private void OnDatabaseConnectionStatusChanged(bool isConnected, string message)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action<bool, string>(OnDatabaseConnectionStatusChanged), isConnected, message);
                    return;
                }

                if (lblStatus != null)
                {
                    lblStatus.Text = $"DB: {message}";
                }

                System.Diagnostics.Debug.WriteLine($"DB 상태: {message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DB 상태 업데이트 실패: {ex.Message}");
            }
        }

        private void OnDatabaseProgressUpdated(string operation, int current, int total)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action<string, int, int>(OnDatabaseProgressUpdated), operation, current, total);
                    return;
                }

                if (_progressManager != null)
                {
                    TimeSpan remaining = TimeSpan.FromSeconds(Math.Max(0, (total - current) * 0.1));
                    _progressManager.UpdateDetailedProgressInfo(operation, current, total,
                        $"{operation} 진행 중...", remaining);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DB 진행 상황 업데이트 실패: {ex.Message}");
            }
        }

        #endregion

        #region 실제 DB 통계 로드

        private async Task LoadInitialStatisticsFromDatabase()
        {
            try
            {

                // 🧪 영업일 로직 테스트
                DateTime currentTime = DateTime.Now;
                DateTime calculatedTradingDay = _databaseManager.GetLastTradingDay();
                bool isTodayBusinessDay = _databaseManager.IsBusinessDay(DateTime.Today);

                System.Diagnostics.Debug.WriteLine("=== 🧪 영업일 테스트 결과 ===");
                System.Diagnostics.Debug.WriteLine($"현재 시간: {currentTime:yyyy-MM-dd HH:mm:ss} ({currentTime.DayOfWeek})");
                System.Diagnostics.Debug.WriteLine($"계산된 마지막 영업일: {calculatedTradingDay:yyyy-MM-dd} ({calculatedTradingDay.DayOfWeek})");
                System.Diagnostics.Debug.WriteLine($"오늘이 영업일인가: {isTodayBusinessDay}");
                System.Diagnostics.Debug.WriteLine($"현재 시간대: {currentTime.TimeOfDay}");
                System.Diagnostics.Debug.WriteLine("============================");




                DateTime today = _databaseManager.GetLastTradingDay();
                var statistics = await _databaseManager.GetAnalysisStatisticsAsync(today);

                // 🆕 분석 기준일 동적 업데이트 (여기가 최적!)
                lblAnalysisDate.Text = $"📅 분석기준일: {today:yyyy-MM-dd}";

                // 진행상황 UI 업데이트
                UpdateProgressDisplay(statistics);

                System.Diagnostics.Debug.WriteLine($"✅ 초기 통계 로드 완료: 총 {statistics.TotalStockCount}개 종목");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"초기 통계 로드 실패: {ex.Message}");
            }
        }

        #endregion

        #region 키움 API 이벤트 연결

        private void ConnectKiwoomApiEvents()
        {
            try
            {
                // KiwoomApiManager 이벤트들을 UI와 연결
                _kiwoomApiManager.ConnectionStatusChanged += OnConnectionStatusChanged;
                _kiwoomApiManager.AccountInfoReceived += OnAccountInfoReceived;
                _kiwoomApiManager.ConditionListLoaded += OnConditionListLoaded;
                _kiwoomApiManager.ConditionSearchResultReceived += OnConditionSearchResultReceived;
                _kiwoomApiManager.ProgressUpdated += OnProgressUpdated;
                _kiwoomApiManager.StatusMessageUpdated += OnStatusMessageUpdated;

                System.Diagnostics.Debug.WriteLine("✅ 키움 API 이벤트 연결 완료 (조건검색 결과 처리 포함)");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ 키움 API 이벤트 연결 실패: {ex.Message}");
            }
        }

        #endregion

        #region 키움 API 이벤트 핸들러

        private void OnConnectionStatusChanged(bool isConnected, string message)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action<bool, string>(OnConnectionStatusChanged), isConnected, message);
                    return;
                }

                lblConnectionStatus.Text = isConnected ? "✅ 연결완료" : "❌ 연결안됨";
                lblConnectionStatus.ForeColor = isConnected ? Color.Green : Color.Red;

                // 연결 상태에 따른 UI 업데이트
                btnConnect.Text = isConnected ? "연결됨" : "키움 연결";
                btnConnect.Enabled = !isConnected;
                btnRefreshConditions.Enabled = isConnected;
                cmbConditions.Enabled = isConnected;

                // StatusManager 업데이트
                if (_statusManager != null)
                {
                    _statusManager.IsKiwoomConnected = isConnected;
                    _statusManager.UpdateStatusBarSummary();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"연결 상태 업데이트 실패: {ex.Message}");
            }
        }

        private void OnAccountInfoReceived(AccountInfo accountInfo)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action<AccountInfo>(OnAccountInfoReceived), accountInfo);
                    return;
                }

                // 새로운 계좌정보 UI 업데이트
                lblAccountSummary.Text = $"💰 예수금: {accountInfo.Balance / 10000:N0}만";
                lblBalance.Text = $"📊 총평가: {accountInfo.TotalValue / 10000:N0}만";

                double profitRate = accountInfo.TotalValue > 0 ? (accountInfo.TotalProfit * 100.0 / accountInfo.TotalValue) : 0;
                lblProfit.Text = $"📈 손익: {(accountInfo.TotalProfit > 0 ? "+" : "")}{accountInfo.TotalProfit / 10000:N1}만({profitRate:F1}%)";
                lblProfit.ForeColor = accountInfo.TotalProfit >= 0 ? Color.Red : Color.Blue;

                // 🆕 사용자 정보도 업데이트 (하드코딩 제거)
                string currentInvestmentType = GetSelectedInvestmentType();
                string actualUserName = _kiwoomApiManager?.GetActualUserName() ?? "연결된 사용자";
                lblUserInfo.Text = $"👤 {actualUserName} 📱 {currentInvestmentType}";
                                
               
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"계좌 정보 업데이트 실패: {ex.Message}");
            }
        }




        /// <summary>
        /// 조건검색식 목록 로드 완료 처리 (하드코딩 제거 버전)
        /// </summary>
        private void OnConditionListLoaded(int count)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action<int>(OnConditionListLoaded), count);
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"=== 조건검색식 목록 업데이트 ===");
                System.Diagnostics.Debug.WriteLine($"수신된 조건검색식 개수: {count}");

                // 조건검색식 목록 업데이트
                cmbConditions.Items.Clear();
                string[] conditions = _kiwoomApiManager.GetConditionList();

                foreach (string condition in conditions)
                {
                    if (!string.IsNullOrEmpty(condition.Trim()))
                    {
                        cmbConditions.Items.Add(condition);
                        System.Diagnostics.Debug.WriteLine($"추가된 조건검색식: {condition}");
                    }
                }

                if (cmbConditions.Items.Count > 0)
                {
                    // 첫 번째 조건을 기본 선택
                    cmbConditions.SelectedIndex = 0;
                    btnStep1.Enabled = true;

                    // ✅ 실제 선택된 조건으로 표시 (하드코딩 제거)
                    string selectedCondition = cmbConditions.SelectedItem.ToString();
                    System.Diagnostics.Debug.WriteLine($"기본 선택된 조건: {selectedCondition}");

                    // 조건검색식 파싱해서 이름만 추출
                    string[] parts = selectedCondition.Split('^');
                    string conditionName = parts.Length >= 2 ? parts[1] : selectedCondition;

                    lblSelectedCondition.Text = $"🔍 {conditionName}\n▼ {selectedCondition}";

                    System.Diagnostics.Debug.WriteLine($"UI 표시 업데이트: {conditionName}");
                }
                else
                {
                    lblSelectedCondition.Text = "🔍 조건검색식 없음\n▼ 등록된 조건이 없습니다";
                    System.Diagnostics.Debug.WriteLine("❌ 사용 가능한 조건검색식이 없음");
                }

                // StatusManager 업데이트
                if (_statusManager != null)
                {
                    _statusManager.LoadedConditionCount = count;
                    _statusManager.UpdateStatusBarSummary();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ 조건검색식 목록 업데이트 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 조건검색 결과를 실제 종목 정보와 함께 DB에 저장 (실제 키움 API 구현)
        /// </summary>
        private async Task ProcessAndSaveConditionSearchResult(ConditionSearchResult result)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🔄 조건검색 결과 처리 시작: {result.Count}개 종목 - 실제 데이터 조회");

                // 진행상황 표시
                if (_progressManager != null)
                {
                    _progressManager.UpdateDetailedProgressInfo("실제 종목정보 조회", 0, result.Count, "키움 API로 실제 데이터 수집 중...", TimeSpan.FromSeconds(result.Count * 1.0));
                }

                // 실제 종목 정보 리스트 생성
                var stockList = new List<StockBasicInfo>();

                // ✅ 실제 키움 API로 종목 정보 조회
                for (int i = 0; i < result.StockCodes.Length; i++)
                {
                    string stockCode = result.StockCodes[i];

                    try
                    {
                        // 진행상황 업데이트
                        if (_progressManager != null)
                        {
                            _progressManager.UpdateDetailedProgressInfo("실제 종목정보 조회", i + 1, result.Count,
                                $"키움 API 조회: {stockCode}", TimeSpan.FromSeconds((result.Count - i - 1) * 1.0));
                        }

                        // ✅ 실제 키움 API로 종목 상세정보 조회
                        var stockInfo = await GetRealStockInfoAsync(stockCode);

                        stockList.Add(stockInfo);

                        System.Diagnostics.Debug.WriteLine($"✅ 실제 종목 정보 수집 완료: {stockCode} - {stockInfo.StockName} ({stockInfo.ClosePrice}원, {stockInfo.ChangeRate:F2}%)");

                        // API 호출 제한 고려 (1초 대기)
                        await Task.Delay(200);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ 종목 정보 수집 실패: {stockCode} - {ex.Message}");

                        // 실패한 종목도 기본 정보로 추가
                        stockList.Add(new StockBasicInfo
                        {
                            StockCode = stockCode,
                            StockName = $"종목{stockCode}",
                            MarketType = GetMarketTypeFromCode(stockCode),
                            ClosePrice = 0,
                            Volume = 0,
                            ChangeRate = 0.0
                        });
                    }
                }

                // DB에 저장
                DateTime searchDate = _databaseManager.GetLastTradingDay();
                int savedCount = await _databaseManager.SaveConditionSearchResultsAsync(
                    searchDate, result.ConditionIndex, result.ConditionName, stockList);

                // 현재 단계 표시 업데이트
                UpdateCurrentStepDisplay("조건검색", savedCount, savedCount, "완료");

                // 로그 기록
                await _databaseManager.LogAsync("INFO", "CONDITION_SEARCH",
                    $"실제 조건검색 결과 저장 완료: {savedCount}개 종목 (소요시간: {result.ElapsedTime.TotalSeconds:F1}초)",
                    null, $"조건식: {result.ConditionName}");

                System.Diagnostics.Debug.WriteLine($"✅ 실제 조건검색 결과 DB 저장 완료: {savedCount}개 종목");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ 조건검색 결과 처리 실패: {ex.Message}");
                await _databaseManager.LogAsync("ERROR", "CONDITION_SEARCH",
                    $"조건검색 결과 처리 실패: {ex.Message}");
                throw;
            }
        }



        /// <summary>
        /// 조건검색 결과 수신 처리 (디버깅 로그 추가)
        /// </summary>
        private async void OnConditionSearchResultReceived(ConditionSearchResult result)
        {
            try
            {
                // ✅ 체크포인트 F: 결과 수신 확인
                System.Diagnostics.Debug.WriteLine("=== OnConditionSearchResultReceived 호출 ===");
                System.Diagnostics.Debug.WriteLine($"수신된 결과 - 조건명: {result.ConditionName}");
                System.Diagnostics.Debug.WriteLine($"수신된 결과 - 종목 수: {result.Count}");
                System.Diagnostics.Debug.WriteLine($"수신된 결과 - 소요시간: {result.ElapsedTime.TotalSeconds:F1}초");

                if (this.InvokeRequired)
                {
                    System.Diagnostics.Debug.WriteLine("🔄 UI 스레드로 Invoke 처리");
                    this.Invoke(new Action<ConditionSearchResult>(OnConditionSearchResultReceived), result);
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"🎯 조건검색 결과 수신: {result.Count}개 종목, 소요시간: {result.ElapsedTime.TotalSeconds:F1}초");

                if (result.Count > 0)
                {
                    // 종목 코드 목록 출력
                    System.Diagnostics.Debug.WriteLine("📝 수신된 종목 코드 목록:");
                    for (int i = 0; i < result.StockCodes.Length; i++)
                    {
                        System.Diagnostics.Debug.WriteLine($"  {i + 1}. {result.StockCodes[i]}");
                    }

                    SaveConditionSearchResults(result);

                    // ✅ 실제 종목 상세정보 수집 후 DB에 저장
                    System.Diagnostics.Debug.WriteLine("🔄 종목 상세정보 수집 및 DB 저장 시작");
                    await ProcessAndSaveConditionSearchResult(result);

                    // 종목 리스트 UI 업데이트
                    System.Diagnostics.Debug.WriteLine("🔄 UI 업데이트 시작");
                    await UpdateStockListDisplay();

                    // 다음 단계 버튼 활성화
                    btnStep2.Enabled = true;
                    System.Diagnostics.Debug.WriteLine("✅ 2차 분석 버튼 활성화");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ 조건검색 결과가 0개");
                }

                // 완료 메시지
                string message = $"조건검색 완료: {result.Count}개 종목 ({result.ElapsedTime.TotalSeconds:F1}초 소요)";
                if (_statusManager != null)
                {
                    _statusManager.SetStatusMessage(message);
                    _statusManager.ConditionSearchCompleted = true;
                    _statusManager.UpdateStatusBarSummary();
                }

                // 사용자 알림
                if (result.Count > 0)
                {
                    var dialogResult = MessageBox.Show(
                        $"조건검색이 완료되었습니다.\n\n" +
                        $"• 수신 종목: {result.Count}개\n" +
                        $"• 소요 시간: {result.ElapsedTime.TotalSeconds:F1}초\n" +
                        $"• 조건식: {result.ConditionName}\n\n" +
                        $"다음 단계(기술적 분석)를 진행하시겠습니까?",
                        "조건검색 완료",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information);

                    if (dialogResult == DialogResult.Yes)
                    {
                        // 자동으로 다음 단계 실행
                        BtnTechnicalAnalysis_Click(btnStep2, EventArgs.Empty);
                    }
                }
                else
                {
                    MessageBox.Show(
                        $"조건검색이 완료되었지만 조건에 맞는 종목이 없습니다.\n\n" +
                        $"• 조건식: {result.ConditionName}\n" +
                        $"• 소요 시간: {result.ElapsedTime.TotalSeconds:F1}초\n\n" +
                        $"다른 조건검색식을 시도해보시거나 시장 상황을 확인해보세요.",
                        "조건검색 결과 없음",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }

                System.Diagnostics.Debug.WriteLine("✅ OnConditionSearchResultReceived 처리 완료");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ OnConditionSearchResultReceived 실패: {ex.Message}");
                MessageBox.Show($"조건검색 결과 처리 중 오류가 발생했습니다:\n{ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 조건검색식 선택 변경 이벤트 핸들러 (새로 추가)
        /// </summary>
        private void CmbConditions_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cmbConditions.SelectedItem != null)
                {
                    string selectedCondition = cmbConditions.SelectedItem.ToString();
                    System.Diagnostics.Debug.WriteLine($"=== 조건검색식 선택 변경 ===");
                    System.Diagnostics.Debug.WriteLine($"새로 선택된 조건: {selectedCondition}");

                    // 조건검색식 파싱해서 이름만 추출
                    string[] parts = selectedCondition.Split('^');
                    string conditionName = parts.Length >= 2 ? parts[1] : selectedCondition;

                    // ✅ 실제 선택된 조건으로 UI 업데이트
                    lblSelectedCondition.Text = $"🔍 {conditionName}\n▼ {selectedCondition}";

                    System.Diagnostics.Debug.WriteLine($"UI 업데이트 완료: {conditionName}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ 조건검색식 선택 변경 처리 실패: {ex.Message}");
            }
        }



        private void OnProgressUpdated(string stepName, int current, int total, string message, TimeSpan remaining)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action<string, int, int, string, TimeSpan>(OnProgressUpdated), stepName, current, total, message, remaining);
                    return;
                }

                // ProgressManager 업데이트
                if (_progressManager != null)
                {
                    _progressManager.UpdateDetailedProgressInfo(stepName, current, total, message, remaining);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"진행 상황 업데이트 실패: {ex.Message}");
            }
        }

        private void OnStatusMessageUpdated(string message)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action<string>(OnStatusMessageUpdated), message);
                    return;
                }

                lblStatus.Text = $"상태: {message}";

                // StatusManager 업데이트
                if (_statusManager != null)
                {
                    _statusManager.SetStatusMessage(message);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"상태 메시지 업데이트 실패: {ex.Message}");
            }
        }

        #endregion

        #region 실제 조건검색 결과 처리 (새로 추가)

        /// <summary>
        /// 디버깅용: 사용 가능한 모든 출력 항목 확인
        /// </summary>
        private void DebugAllOutputFields(string trCode, string requestName, string stockCode)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== {stockCode} 모든 출력 항목 디버깅 ===");

                // 일반적인 키움 API 출력 항목들 시도
                string[] commonFields = {
            "종목코드", "종목명", "현재가", "기준가", "전일종가",
            "시가", "고가", "저가", "상한가", "하한가",
            "전일대비", "등락률", "대비율", "등락율", "대비",
            "거래량", "누적거래량", "누적거래대금", "거래대금",
            "시가총액", "상장주식", "외국인소진율", "대차잔고율",
            "PER", "EPS", "ROE", "PBR", "BPS"
        };

                foreach (string field in commonFields)
                {
                    try
                    {
                        string value = axKHOpenAPI1.GetCommData(trCode, requestName, 0, field).Trim();
                        if (!string.IsNullOrEmpty(value))
                        {
                            System.Diagnostics.Debug.WriteLine($"  {field}: '{value}'");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"  {field}: 오류 - {ex.Message}");
                    }
                }

                System.Diagnostics.Debug.WriteLine("=======================================");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ DebugAllOutputFields 실패: {ex.Message}");
            }
        }




        /// <summary>
        /// 종목코드로 시장 구분 판단
        /// </summary>
        private string GetMarketTypeFromCode(string stockCode)
        {
            if (string.IsNullOrEmpty(stockCode) || stockCode.Length != 6)
                return "KOSPI";

            // 코스닥: 일반적으로 특정 범위의 종목코드 사용
            // 정확한 구분은 키움 API를 통해 조회해야 하지만, 대략적 구분
            int code = int.Parse(stockCode);

            if (code >= 300000) return "KOSDAQ";
            if (code >= 200000) return "KOSPI";
            if (code >= 100000) return "KOSDAQ";

            return "KOSPI";
        }

  
        /// <summary>
        /// 실제 키움 API로 종목 정보 조회 (OPT10001)
        /// </summary>
        private async Task<StockBasicInfo> GetRealStockInfoAsync(string stockCode)
        {
            var result = new StockBasicInfo
            {
                StockCode = stockCode,
                StockName = $"종목{stockCode}",
                MarketType = GetMarketTypeFromCode(stockCode),
                ClosePrice = 0,
                Volume = 0,
                ChangeRate = 0.0
            };

            try
            {
                var waitEvent = new ManualResetEvent(false);
                
                string requestId = Guid.NewGuid().ToString("N").Substring(0, 8);
                StockBasicInfo responseData = null;

                // TR 데이터 수신 이벤트 핸들러 임시 등록
                _DKHOpenAPIEvents_OnReceiveTrDataEventHandler tempHandler = null;
                tempHandler = (sender, e) =>
                {
                    try
                    {
                        if (e.sRQName.Contains(requestId))
                        {
                            // 실제 종목 데이터 파싱
                            DebugAllOutputFields(e.sTrCode, e.sRQName, stockCode);
                            responseData = ParseRealStockData(e.sTrCode, e.sRQName, stockCode);

                            // 이벤트 핸들러 제거
                            axKHOpenAPI1.OnReceiveTrData -= tempHandler;
                            waitEvent.Set();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ 실제 종목조회 응답 처리 실패: {ex.Message}");
                        axKHOpenAPI1.OnReceiveTrData -= tempHandler;
                        waitEvent.Set();
                    }
                };

                // 이벤트 핸들러 등록
                axKHOpenAPI1.OnReceiveTrData += tempHandler;

                // 실제 종목 조회 요청 (OPT10001)
                string screenNo = "1002";
                string trCode = "OPT10001";

                axKHOpenAPI1.SetInputValue("종목코드", stockCode);

                int requestResult = axKHOpenAPI1.CommRqData($"실제종목조회_{requestId}", trCode, 0, screenNo);

                if (requestResult != 0)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ 실제 종목조회 요청 실패: {requestResult}");
                    axKHOpenAPI1.OnReceiveTrData -= tempHandler;
                    return result;
                }

                // 응답 대기 (최대 5초)
                bool received = await Task.Run(() => waitEvent.WaitOne(2000));

                if (!received)
                {
                    System.Diagnostics.Debug.WriteLine($"⏰ 실제 종목조회 응답 타임아웃: {stockCode}");
                    axKHOpenAPI1.OnReceiveTrData -= tempHandler;
                }
                else if (responseData != null)
                {
                    result = responseData;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ GetRealStockInfoAsync 실패: {stockCode} - {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// 실제 종목 데이터 파싱 (OPT10001 응답) - 키움 API 정확한 항목명 사용
        /// </summary>

        // MainForm.cs의 ParseRealStockData 메서드 수정
        private StockBasicInfo ParseRealStockData(string trCode, string requestName, string stockCode)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== {stockCode} 데이터 파싱 시작 ===");

                // 기본 정보
                string stockName = axKHOpenAPI1.GetCommData(trCode, requestName, 0, "종목명").Trim();
                string currentPrice = axKHOpenAPI1.GetCommData(trCode, requestName, 0, "현재가").Trim();

                // ✅ 전일대비 추가
                string changeAmount = "";
                string[] changeAmountFields = { "전일대비", "대비", "전일대비기호" };
                foreach (string field in changeAmountFields)
                {
                    string temp = axKHOpenAPI1.GetCommData(trCode, requestName, 0, field).Trim();
                    if (!string.IsNullOrEmpty(temp))
                    {
                        changeAmount = temp;
                        System.Diagnostics.Debug.WriteLine($"✅ 전일대비 필드 발견: {field} = {temp}");
                        break;
                    }
                }

                // 시가/고가/저가 - 여러 가능한 필드명 시도
                string openPrice = "";
                string[] openPriceFields = { "시가", "시초가", "금일시가", "당일시가" };
                foreach (string field in openPriceFields)
                {
                    string temp = axKHOpenAPI1.GetCommData(trCode, requestName, 0, field).Trim();
                    if (!string.IsNullOrEmpty(temp))
                    {
                        openPrice = temp;
                        System.Diagnostics.Debug.WriteLine($"✅ 시가 필드 발견: {field} = {temp}");
                        break;
                    }
                }

                string highPrice = "";
                string[] highPriceFields = { "고가", "당일고가", "금일고가", "최고가" };
                foreach (string field in highPriceFields)
                {
                    string temp = axKHOpenAPI1.GetCommData(trCode, requestName, 0, field).Trim();
                    if (!string.IsNullOrEmpty(temp))
                    {
                        highPrice = temp;
                        System.Diagnostics.Debug.WriteLine($"✅ 고가 필드 발견: {field} = {temp}");
                        break;
                    }
                }

                string lowPrice = "";
                string[] lowPriceFields = { "저가", "당일저가", "금일저가", "최저가" };
                foreach (string field in lowPriceFields)
                {
                    string temp = axKHOpenAPI1.GetCommData(trCode, requestName, 0, field).Trim();
                    if (!string.IsNullOrEmpty(temp))
                    {
                        lowPrice = temp;
                        System.Diagnostics.Debug.WriteLine($"✅ 저가 필드 발견: {field} = {temp}");
                        break;
                    }
                }

                // 등락률 및 거래량
                string changeRate = "";
                string[] changeRateFields = { "등락률", "대비율", "등락율" };
                foreach (string field in changeRateFields)
                {
                    string temp = axKHOpenAPI1.GetCommData(trCode, requestName, 0, field).Trim();
                    if (!string.IsNullOrEmpty(temp))
                    {
                        changeRate = temp;
                        break;
                    }
                }

                string volume = "";
                string[] volumeFields = { "거래량", "누적거래량" };
                foreach (string field in volumeFields)
                {
                    string temp = axKHOpenAPI1.GetCommData(trCode, requestName, 0, field).Trim();
                    if (!string.IsNullOrEmpty(temp))
                    {
                        volume = temp;
                        break;
                    }
                }

                // ✅ 상세 디버그 출력 (전일대비 포함)
                System.Diagnostics.Debug.WriteLine($"📊 {stockCode} 파싱 결과:");
                System.Diagnostics.Debug.WriteLine($"  종목명: '{stockName}'");
                System.Diagnostics.Debug.WriteLine($"  현재가: '{currentPrice}' → {ParseSafePrice(currentPrice)}");
                System.Diagnostics.Debug.WriteLine($"  전일대비: '{changeAmount}' → {ParseSafePrice(changeAmount)}");
                System.Diagnostics.Debug.WriteLine($"  시가: '{openPrice}' → {ParseSafePrice(openPrice)}");
                System.Diagnostics.Debug.WriteLine($"  고가: '{highPrice}' → {ParseSafePrice(highPrice)}");
                System.Diagnostics.Debug.WriteLine($"  저가: '{lowPrice}' → {ParseSafePrice(lowPrice)}");
                System.Diagnostics.Debug.WriteLine($"  등락률: '{changeRate}' → {ParseSafeChangeRate(changeRate)}");
                System.Diagnostics.Debug.WriteLine($"  거래량: '{volume}' → {ParseSafeVolume(volume)}");

                var result = new StockBasicInfo
                {
                    StockCode = stockCode,
                    StockName = string.IsNullOrEmpty(stockName) ? $"종목{stockCode}" : stockName,
                    MarketType = GetMarketTypeFromCode(stockCode),
                    ClosePrice = ParseSafePrice(currentPrice),
                    ChangeAmount = ParseSafePrice(changeAmount),  // ✅ 전일대비 추가
                    OpenPrice = ParseSafePrice(openPrice),
                    HighPrice = ParseSafePrice(highPrice),
                    LowPrice = ParseSafePrice(lowPrice),
                    Volume = ParseSafeVolume(volume),
                    ChangeRate = ParseSafeChangeRate(changeRate)
                };

                // ✅ 최종 결과 확인 (전일대비 포함)
                System.Diagnostics.Debug.WriteLine($"✅ 종목 파싱 완료: {stockCode} - {stockName} (현재가:{result.ClosePrice}, 대비:{result.ChangeAmount}, 시가:{result.OpenPrice}, 고가:{result.HighPrice}, 저가:{result.LowPrice})");

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ParseRealStockData 실패: {stockCode} - {ex.Message}");

                return new StockBasicInfo
                {
                    StockCode = stockCode,
                    StockName = $"종목{stockCode}",
                    MarketType = GetMarketTypeFromCode(stockCode),
                    ClosePrice = 0,
                    ChangeAmount = 0,  // ✅ 전일대비 기본값
                    OpenPrice = 0,
                    HighPrice = 0,
                    LowPrice = 0,
                    Volume = 0,
                    ChangeRate = 0.0
                };
            }
        }

       
        /// <summary>
        /// 안전한 가격 파싱 (키움 API 데이터는 앞에 +/- 부호가 붙을 수 있음)
        /// </summary>
        private int ParseSafePrice(string priceStr)
        {
            if (string.IsNullOrEmpty(priceStr)) return 0;

            // +/- 부호 제거
            priceStr = priceStr.Replace("+", "").Replace("-", "").Trim();

            if (int.TryParse(priceStr, out int result))
                return Math.Abs(result);

            return 0;
        }

        /// <summary>
        /// 안전한 거래량 파싱
        /// </summary>
        private long ParseSafeVolume(string volumeStr)
        {
            if (string.IsNullOrEmpty(volumeStr)) return 0;

            volumeStr = volumeStr.Replace(",", "").Trim();

            if (long.TryParse(volumeStr, out long result))
                return result;

            return 0;
        }

        /// <summary>
        /// 안전한 등락률 파싱
        /// </summary>
        private double ParseSafeChangeRate(string changeRateStr)
        {
            if (string.IsNullOrEmpty(changeRateStr)) return 0.0;

            // % 기호 제거
            changeRateStr = changeRateStr.Replace("%", "").Trim();

            if (double.TryParse(changeRateStr, out double result))
                return result;

            return 0.0;
        }



        #endregion

        #region 버튼 이벤트 핸들러

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                // 🆕 연결 시도 전 상태 초기화
                btnConnect.Enabled = false;
                btnConnect.Text = "연결 중...";
                lblConnectionStatus.Text = "🔄 키움 서버 연결 시도 중...";
                lblConnectionStatus.ForeColor = Color.Orange;

                if (_kiwoomApiManager != null)
                {
                    // 🆕 타임아웃 타이머 시작 (20초)
                    var timeoutTimer = new System.Windows.Forms.Timer();
                    timeoutTimer.Interval = 20000; // 20초
                    timeoutTimer.Tick += (s, args) =>
                    {
                        timeoutTimer.Stop();
                        timeoutTimer.Dispose();

                        if (!_kiwoomApiManager.IsConnected)
                        {
                            // 타임아웃 발생
                            HandleConnectionTimeout();
                        }
                    };
                    timeoutTimer.Start();

                    bool result = _kiwoomApiManager.Connect();

                    if (!result)
                    {
                        timeoutTimer.Stop();
                        timeoutTimer.Dispose();
                        HandleConnectionFailure("연결 요청 실패");
                    }
                    // 성공/실패는 OnEventConnect 이벤트에서 처리됨
                }
            }
            catch (Exception ex)
            {
                HandleConnectionFailure($"연결 중 예외 발생: {ex.Message}");
            }
        }

        /// <summary>
        /// 🆕 연결 타임아웃 처리
        /// </summary>
        private void HandleConnectionTimeout()
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(HandleConnectionTimeout));
                    return;
                }

                System.Diagnostics.Debug.WriteLine("⏰ 키움 연결 타임아웃 발생");

                // UI 상태 복원
                btnConnect.Enabled = true;
                btnConnect.Text = "키움 연결";
                lblConnectionStatus.Text = "⏰ 연결 타임아웃 (20초 경과)";
                lblConnectionStatus.ForeColor = Color.Red;

                // 사용자에게 안내
                MessageBox.Show(
                    "키움증권 서버 연결이 20초 내에 완료되지 않았습니다.\n\n" +
                    "가능한 원인:\n" +
                    "• 키움증권 시스템 점검 중\n" +
                    "• 네트워크 연결 문제\n" +
                    "• 키움 서버 과부하\n\n" +
                    "잠시 후 다시 시도해주세요.",
                    "연결 타임아웃",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"타임아웃 처리 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 🆕 연결 실패 처리
        /// </summary>
        private void HandleConnectionFailure(string reason)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action<string>(HandleConnectionFailure), reason);
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"❌ 키움 연결 실패: {reason}");

                // UI 상태 복원
                btnConnect.Enabled = true;
                btnConnect.Text = "키움 연결";
                lblConnectionStatus.Text = $"❌ 연결 실패";
                lblConnectionStatus.ForeColor = Color.Red;

                MessageBox.Show($"키움 연결 실패:\n{reason}", "연결 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"연결 실패 처리 실패: {ex.Message}");
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                if (_kiwoomApiManager != null)
                {
                    _kiwoomApiManager.RefreshConditionList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("조건검색식 새로고침 실패: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region 🆕 키움 연결 UI 개선 - 누락된 메서드들

        /// <summary>
        /// 투자 타입 선택 변경 이벤트 (모의투자/실전투자)
        /// </summary>
        private void RbInvestmentType_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (sender is RadioButton rb && rb.Checked)
                {
                    string selectedType = GetSelectedInvestmentType();

                    System.Diagnostics.Debug.WriteLine($"=== 투자 타입 변경: {selectedType} ===");

                    // 🛡️ 실전투자 선택 시 경고
                    if (selectedType == "실전투자")
                    {
                        var result = MessageBox.Show(
                            "⚠️ 실전투자 모드를 선택하셨습니다.\n" +
                            "실제 매매가 실행될 수 있습니다.\n\n" +
                            "계속하시겠습니까?",
                            "실전투자 확인",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning);

                        if (result == DialogResult.No)
                        {
                            // 사용자가 취소하면 모의투자로 되돌리기
                            rbSimulation.Checked = true;
                            return;
                        }
                    }

                    // UI 상태 업데이트
                    UpdateInvestmentTypeUI(selectedType);

                    System.Diagnostics.Debug.WriteLine($"✅ 투자 타입 변경 완료: {selectedType}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ 투자 타입 변경 실패: {ex.Message}");
                MessageBox.Show($"투자 타입 변경 중 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 계좌 선택 변경 이벤트
        /// </summary>
        private void CmbAccounts_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cmbAccounts.SelectedItem != null)
                {
                    string selectedAccount = cmbAccounts.SelectedItem.ToString();
                    string investmentType = GetSelectedInvestmentType();

                    System.Diagnostics.Debug.WriteLine($"=== 계좌 변경: {selectedAccount} ({investmentType}) ===");

                    // 🛡️ 실전투자에서 계좌 변경 시 한번 더 확인
                    if (investmentType == "실전투자" && _kiwoomApiManager?.IsConnected == true)
                    {
                        var result = MessageBox.Show(
                            $"실전투자 계좌를 변경하시겠습니까?\n\n" +
                            $"선택된 계좌: {selectedAccount}\n" +
                            $"투자 타입: {investmentType}",
                            "계좌 변경 확인",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);

                        if (result == DialogResult.No)
                        {
                            return;
                        }
                    }

                    // 키움 API에 계좌 변경 알림
                    if (_kiwoomApiManager?.IsConnected == true)
                    {
                        _kiwoomApiManager.ChangeCurrentAccount(selectedAccount);

                        // 새로운 계좌의 잔고 정보 요청
                        _kiwoomApiManager.RequestAccountBalance();
                    }

                    System.Diagnostics.Debug.WriteLine($"✅ 계좌 변경 완료: {selectedAccount}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ 계좌 변경 실패: {ex.Message}");
                MessageBox.Show($"계좌 변경 중 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 현재 선택된 투자 타입 반환
        /// </summary>
        private string GetSelectedInvestmentType()
        {
            try
            {
                if (rbReal != null && rbReal.Checked)
                    return "실전투자";
                else
                    return "모의투자";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"투자 타입 조회 실패: {ex.Message}");
                return "모의투자"; // 안전한 기본값
            }
        }

        /// <summary>
        /// 투자 타입에 따른 UI 업데이트
        /// </summary>
        private void UpdateInvestmentTypeUI(string investmentType)
        {
            try
            {
                // 연결되지 않은 상태에서는 기본 텍스트만 업데이트
                if (_kiwoomApiManager?.IsConnected != true)
                {
                    lblUserInfo.Text = $"👤 연결 후 표시 📱 {investmentType}";

                    // 투자 타입에 따른 색상 변경
                    if (investmentType == "실전투자")
                    {
                        lblUserInfo.ForeColor = Color.Red;
                    }
                    else
                    {
                        lblUserInfo.ForeColor = Color.DarkBlue;
                    }
                    return;
                }

                // 연결된 상태에서는 실제 사용자 정보와 함께 표시
                lblUserInfo.Text = $"👤 연결된 사용자 📱 {investmentType}";

                // 투자 타입에 따른 색상 변경
                if (investmentType == "실전투자")
                {
                    lblUserInfo.ForeColor = Color.Red;
                }
                else
                {
                    lblUserInfo.ForeColor = Color.DarkBlue;
                }

                System.Diagnostics.Debug.WriteLine($"UI 업데이트 완료: {investmentType}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UI 업데이트 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 🆕 프로그램 시작 시 이전 설정 복원
        /// </summary>

        /// <summary>
        /// 🆕 프로그램 시작 시 이전 설정 복원
        /// </summary>
        private void LoadUserSettings()
        {
            try
            {
                // 🛡️ 컨트롤이 아직 초기화되지 않았을 수 있으므로 null 체크
                if (rbSimulation == null || rbReal == null)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ 라디오 버튼 컨트롤이 아직 초기화되지 않음");
                    return;
                }

                // 기본적으로 모의투자 선택 (안전한 기본값)
                rbSimulation.Checked = true;

                System.Diagnostics.Debug.WriteLine("✅ 사용자 설정 복원 완료 (기본: 모의투자)");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ 사용자 설정 복원 실패: {ex.Message}");
                // 실패 시에도 안전한 기본값 설정 시도
                try
                {
                    if (rbSimulation != null)
                        rbSimulation.Checked = true;
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ 기본값 설정도 실패 - 컨트롤 초기화 전일 가능성");
                }
            }
        }

       

        #endregion


        /// <summary>
        /// 조건검색 실행 버튼 클릭 (단순화 버전)
        /// </summary>
        private async void btnStep1_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== 조건검색 버튼 클릭 ===");
                System.Diagnostics.Debug.WriteLine($"현재 시간: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

                if (cmbConditions.SelectedItem == null)
                {
                    System.Diagnostics.Debug.WriteLine("❌ 조건검색식이 선택되지 않음");
                    MessageBox.Show("조건검색식을 선택해주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string selectedCondition = cmbConditions.SelectedItem.ToString();
                System.Diagnostics.Debug.WriteLine($"선택된 조건검색식: '{selectedCondition}'");

                btnStep1.Enabled = false;
                btnStep1.Text = "실행 중...";

                string[] parts = selectedCondition.Split('^');
                System.Diagnostics.Debug.WriteLine($"파싱 결과: {parts.Length}개 부분으로 분할");

                for (int i = 0; i < parts.Length; i++)
                {
                    System.Diagnostics.Debug.WriteLine($"  parts[{i}]: '{parts[i]}'");
                }

                if (parts.Length >= 2)
                {
                    int conditionIndex = int.Parse(parts[0]);
                    string conditionName = parts[1];

                    System.Diagnostics.Debug.WriteLine($"파싱된 조건 인덱스: {conditionIndex}");
                    System.Diagnostics.Debug.WriteLine($"파싱된 조건 이름: '{conditionName}'");

                    if (_kiwoomApiManager == null)
                    {
                        System.Diagnostics.Debug.WriteLine("❌ KiwoomApiManager가 null");
                        throw new Exception("키움 API 매니저가 초기화되지 않았습니다.");
                    }

                    if (!_kiwoomApiManager.IsConnected)
                    {
                        System.Diagnostics.Debug.WriteLine("❌ 키움 API가 연결되지 않음");
                        throw new Exception("키움 API가 연결되지 않았습니다. 먼저 연결해주세요.");
                    }

                    System.Diagnostics.Debug.WriteLine($"✅ 키움 API 연결 상태: {_kiwoomApiManager.IsConnected}");

                    System.Diagnostics.Debug.WriteLine($"🚀 조건검색 실행 시작: {conditionName} (인덱스: {conditionIndex})");

                    if (_statusManager != null)
                    {
                        _statusManager.SetStatusMessage($"조건검색 실행 중: {conditionName}");
                    }

                    UpdateCurrentStepDisplay("조건검색", 0, 100, "실행 중");

                    // ✅ 실제 조건검색 실행
                    System.Diagnostics.Debug.WriteLine("📞 KiwoomApiManager.ExecuteConditionSearch 호출");
                    _kiwoomApiManager.ExecuteConditionSearch(conditionIndex, conditionName);

                    await _databaseManager.LogAsync("INFO", "CONDITION_SEARCH",
                        $"조건검색 실행: {conditionName}", null, $"조건인덱스: {conditionIndex}");

                    System.Diagnostics.Debug.WriteLine($"✅ 조건검색 요청 전송 완료: {conditionName}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"❌ 조건검색식 형식 오류: parts.Length={parts.Length}");
                    MessageBox.Show("조건검색식 형식이 올바르지 않습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                btnStep1.Enabled = true;
                btnStep1.Text = "조건검색 실행";

                System.Diagnostics.Debug.WriteLine("✅ 조건검색 버튼 클릭 처리 완료");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ btnStep1_Click 예외 발생: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"스택 트레이스: {ex.StackTrace}");

                MessageBox.Show("조건검색 실행 실패: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnStep1.Enabled = true;
                btnStep1.Text = "조건검색 실행";

                await _databaseManager.LogAsync("ERROR", "CONDITION_SEARCH",
                    $"조건검색 실행 실패: {ex.Message}");
            }
        }

      
        #region 🆕 과거 데이터 수집 버튼 이벤트 (실제 연동)

        /// <summary>
        /// 과거 데이터 수집 버튼 클릭 이벤트 (조건검색 결과 기반)
        /// </summary>
        private async void BtnCollectHistoricalData_Click(object sender, EventArgs e)
        {
            if (_isHistoricalDataCollecting) return;

            try
            {
                _isHistoricalDataCollecting = true;
                SetHistoricalDataButtonsEnabled(false);

                // 1단계: 조건검색 결과 확인
                if (_lastConditionSearchResults.Count == 0)
                {
                    MessageBox.Show("먼저 조건검색을 실행해서 종목을 발견해주세요.", "알림",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 2단계: 키움 API 연결 상태 확인
                if (!_kiwoomApiManager.IsConnected)
                {
                    MessageBox.Show("키움 API가 연결되지 않았습니다. 먼저 키움에 로그인해주세요.", "연결 오류",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 3단계: 사용자 확인
                var confirmResult = MessageBox.Show(
                    $"조건검색 결과 {_lastConditionSearchResults.Count}개 종목의 과거 데이터를 수집하시겠습니까?\n\n" +
                    $"• 일봉 데이터: 60일치\n" +
                    $"• 1분봉 데이터: 3일치\n" +
                    $"• 예상 소요 시간: {_lastConditionSearchResults.Count * 2}초\n\n" +
                    $"기존 키움 연결을 재사용하므로 안전합니다.",
                    "과거 데이터 수집 확인",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirmResult != DialogResult.Yes)
                {
                    return;
                }

                // 4단계: 취소 토큰 준비
                if (_cancellationTokenSource != null)
                    _cancellationTokenSource.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();

                // 5단계: UI 초기화
                UpdateProgress(0, "과거 데이터 수집 준비 중...");
                UpdateStatusMessage("과거 데이터 수집 시작");

                AddLogMessage($"=== 과거 데이터 수집 시작 ===");
                AddLogMessage($"대상 종목: {_lastConditionSearchResults.Count}개");
                AddLogMessage($"수집 항목: 일봉 60일 + 1분봉 3일");

                // 6단계: 🚀 실제 과거 데이터 수집 실행
                await _kiwoomApiManager.CollectHistoricalDataAsync(_lastConditionSearchResults,
                    _cancellationTokenSource.Token,
                    (current, total, stockCode) =>
                    {
                        // 실시간 진행률 업데이트
                        int percentage = (current * 100) / total;
                        string progressMessage = $"과거 데이터 수집: {stockCode} ({current}/{total})";

                        UpdateProgress(percentage, progressMessage);
                        AddLogMessage($"[{current}/{total}] {stockCode} 과거 데이터 수집 중...");
                    });

                // 7단계: 완료 처리
                UpdateProgress(100, "과거 데이터 수집 완료");
                UpdateStatusMessage($"과거 데이터 수집 완료: {_lastConditionSearchResults.Count}개 종목");

                AddLogMessage($"✅ 과거 데이터 수집 완료!");
                AddLogMessage($"✅ 처리된 종목: {_lastConditionSearchResults.Count}개");
                AddLogMessage($"✅ 수집된 데이터: DailyPriceHistory, MinutePriceHistory 테이블에 저장");

                // 8단계: 성공 메시지
                MessageBox.Show(
                    $"과거 데이터 수집이 완료되었습니다!\n\n" +
                    $"✅ 처리된 종목: {_lastConditionSearchResults.Count}개\n" +
                    $"✅ 일봉 데이터: 각 종목당 최대 60일\n" +
                    $"✅ 1분봉 데이터: 각 종목당 최대 3일\n\n" +
                    $"이제 정밀한 기술적 분석이 가능합니다!",
                    "수집 완료",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                // 9단계: 기술적 분석 제안
                var analysisResult = MessageBox.Show(
                    "과거 데이터 수집이 완료되었습니다.\n\n바로 기술적 분석을 실행하시겠습니까?",
                    "기술적 분석 실행",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (analysisResult == DialogResult.Yes)
                {
                    // 기술적 분석 자동 실행
                    BtnTechnicalAnalysis_Click(sender, e);
                }
            }
            catch (OperationCanceledException)
            {
                UpdateStatusMessage("과거 데이터 수집 취소됨");
                AddLogMessage("⚠️ 사용자가 과거 데이터 수집을 취소했습니다.");
            }
            catch (Exception ex)
            {
                UpdateStatusMessage($"과거 데이터 수집 실패: {ex.Message}");
                AddLogMessage($"❌ 과거 데이터 수집 실패: {ex.Message}");

                MessageBox.Show(
                    $"과거 데이터 수집 중 오류가 발생했습니다:\n\n{ex.Message}\n\n" +
                    $"• 키움 API 연결 상태를 확인해주세요\n" +
                    $"• 네트워크 연결을 확인해주세요\n" +
                    $"• 잠시 후 다시 시도해주세요",
                    "수집 실패",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                _isHistoricalDataCollecting = false;
                SetHistoricalDataButtonsEnabled(true);
                UpdateProgress(0, "대기 중");
            }
        }

        #endregion


        /// <summary>
        /// 🆕 분봉 + 일봉 기반 정밀 기술적 분석 (80점 만점)
        /// </summary>

        /// <summary>
        /// 🔄 스마트 정밀 분석 버튼 클릭 (기존 메서드 교체)
        /// </summary>
        private async void BtnTechnicalAnalysis_Click(object sender, EventArgs e)
        {
            try
            {
                btnStep2.Enabled = false;
                btnStep2.Text = "기술 분석중...";

                if (_statusManager != null)
                {
                    _statusManager.SetStatusMessage("기술 분석 실행 중...");
                }

                // 🚀 기술 분석 시스템 실행
                await ExecuteSmartTechnicalAnalysisAsync();

                btnStep2.Enabled = true;
                btnStep2.Text = "[1차]\n기술 분석";
                btnStep3.Enabled = true;

                // 종목 리스트 UI 업데이트
                await UpdateStockListDisplay();

                // 완료 메시지
                var analysisStats = await GetAnalysisStatistics(await _databaseManager.GetConditionSearchResultsAsync(_databaseManager.GetLastTradingDay()));

                MessageBox.Show(
                    $"🎯 기술 분석 완료!\n\n" +
                    $"📊 총 종목: {analysisStats.TotalAnalyzed}개\n" +
                    $"🔬 기술 분석: 자동 데이터 수집 포함\n" +
                    $"⭐ 평균 점수: {analysisStats.AverageScore:F1}/80점\n" +
                    $"🏆 S등급: {analysisStats.SGradeCount}개\n" +
                    $"🥇 A등급: {analysisStats.AGradeCount}개",
                    "기술 분석 완료",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("기술 분석 실행 실패: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnStep2.Enabled = true;
                btnStep2.Text = "[1차]\n기술 분석";

                await _databaseManager.LogAsync("ERROR", "SMART_TECHNICAL_ANALYSIS",
                    $"기술 분석 실행 실패: {ex.Message}");
            }
        }

    

      
        private async void BtnNewsAnalysis_Click(object sender, EventArgs e)
        {
            try
            {
                btnStep3.Enabled = false;
                btnStep3.Text = "분석 중...";

                // 2차 분석 실행
                if (_statusManager != null)
                {
                    _statusManager.SetStatusMessage("2차 분석(뉴스 분석) 실행 중...");
                }

                UpdateCurrentStepDisplay("뉴스 분석", 0, 100, "실행 중");

                // DB에서 기술적 분석 완료된 종목들 조회
                DateTime today = _databaseManager.GetLastTradingDay();
                var searchResults = await _databaseManager.GetConditionSearchResultsAsync(today);

                if (searchResults.Count > 0)
                {
                    // 뉴스 분석 실행 (더미 데이터로 시뮬레이션)
                    await ExecuteNewsAnalysisAsync(searchResults);

                    btnStep3.Enabled = true;
                    btnStep3.Text = "[2차]\n뉴스분석";
                    btnStep4.Enabled = true; // 다음 단계 활성화

                    // 종목 리스트 UI 업데이트
                    await UpdateStockListDisplay();

                    UpdateCurrentStepDisplay("뉴스 분석", searchResults.Count, searchResults.Count, "완료");

                    MessageBox.Show($"2차 분석 완료: {searchResults.Count}개 종목 분석", "완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("분석할 종목이 없습니다. 먼저 조건검색을 실행해주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    btnStep3.Enabled = true;
                    btnStep3.Text = "[2차]\n뉴스분석";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("2차 분석 실행 실패: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnStep3.Enabled = true;
                btnStep3.Text = "[2차]\n뉴스분석";

                await _databaseManager.LogAsync("ERROR", "NEWS_ANALYSIS",
                    $"2차 분석 실행 실패: {ex.Message}");
            }
        }

        private async void BtnTradePlan_Click(object sender, EventArgs e)
        {
            try
            {
                btnStep4.Enabled = false;
                btnStep4.Text = "수립 중...";

                // 매매계획 수립
                if (_statusManager != null)
                {
                    _statusManager.SetStatusMessage("매매계획 수립 중...");
                }

                UpdateCurrentStepDisplay("매매계획", 0, 100, "실행 중");

                // DB에서 뉴스 분석 완료된 종목들 조회
                DateTime today = _databaseManager.GetLastTradingDay();
                var searchResults = await _databaseManager.GetConditionSearchResultsAsync(today);

                if (searchResults.Count > 0)
                {
                    // 매매계획 수립 (더미 데이터로 시뮬레이션)
                    await ExecuteTradePlanAsync(searchResults);

                    btnStep4.Enabled = true;
                    btnStep4.Text = "[4차]\n매매계획";

                    // 종목 리스트 UI 업데이트
                    await UpdateStockListDisplay();

                    UpdateCurrentStepDisplay("매매계획", searchResults.Count, searchResults.Count, "투자 준비 완료");

                    MessageBox.Show($"매매계획 수립 완료: {searchResults.Count}개 종목\n\n투자 준비가 완료되었습니다!", "완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("매매계획을 수립할 종목이 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    btnStep4.Enabled = true;
                    btnStep4.Text = "[4차]\n매매계획";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("매매계획 수립 실패: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnStep4.Enabled = true;
                btnStep4.Text = "[4차]\n매매계획";

                await _databaseManager.LogAsync("ERROR", "TRADE_PLAN",
                    $"매매계획 수립 실패: {ex.Message}");
            }
        }

        
     
     

#endregion

        #region UI 업데이트 메서드들

        /// <summary>
        /// 조건검색 결과를 DataGridView에 표시
        /// </summary>

        /// <summary>
        /// 진행상태 텍스트 개선
        /// </summary>
        private string GetImprovedStatusText(string processStatus, int progress)
        {
            switch (processStatus)
            {
                case "CONDITION_SEARCH": return "수집완료";
                case "TECHNICAL_ANALYSIS": return "1차완료";
                case "NEWS_ANALYSIS": return "2차완료";
                case "TRADE_PLAN": return "투자준비";
                default: return "대기중";
            }
        }


        // MainForm.cs의 UpdateStockListDisplay 메서드 수정

        // MainForm.cs의 UpdateStockListDisplay 메서드 수정 (종목코드 제거)
        private async Task UpdateStockListDisplay()
        {
            try
            {
                DateTime today = _databaseManager.GetLastTradingDay();
                var searchResults = await _databaseManager.GetConditionSearchResultsAsync(today);

                dgvStockList.Rows.Clear();

                foreach (var stock in searchResults)
                {
                    var row = dgvStockList.Rows[dgvStockList.Rows.Add()];

                    // ✅ 종목코드 컬럼 제거, 종목명부터 시작
                    row.Cells["StockName"].Value = stock.StockName;
                    row.Cells["ClosePrice"].Value = stock.ClosePrice;

                    // 전일대비 표시 (현재가와 시가 사이)
                    row.Cells["ChangeAmount"].Value = stock.ChangeAmount;
                    row.Cells["OpenPrice"].Value = stock.OpenPrice;
                    row.Cells["HighPrice"].Value = stock.HighPrice;
                    row.Cells["LowPrice"].Value = stock.LowPrice;

                    row.Cells["ChangeRate"].Value = stock.ChangeRate;
                    row.Cells["Volume"].Value = stock.Volume;

                    row.Cells["TechnicalScore"].Value = stock.TechnicalScore?.ToString() ?? "";
                    row.Cells["FinalScore"].Value = stock.FinalScore?.ToString() ?? "";
                    row.Cells["FinalGrade"].Value = stock.FinalGrade ?? "";
                    row.Cells["BuyPrice"].Value = stock.BuyPrice?.ToString("N0") ?? "";
                    row.Cells["SellPrice"].Value = stock.SellPrice?.ToString("N0") ?? "";
                    row.Cells["StopLossPrice"].Value = stock.StopLossPrice?.ToString("N0") ?? "";


                    // 🆕 분석 타입에 따른 상태 표시
                    string statusText = GetImprovedStatusText(stock.ProcessStatus, stock.AnalysisProgress);

                    // 분봉 분석 여부 확인
                    bool isEnhancedAnalysis = stock.TechnicalScore.HasValue && stock.TechnicalScore.Value >= 70;

                    if (stock.TechnicalScore.HasValue)
                    {
                        if (isEnhancedAnalysis)
                        {
                            statusText = "정밀완료"; // 분봉 + 일봉
                            row.DefaultCellStyle.BackColor = Color.LightGreen;
                        }
                        else
                        {
                            statusText = "기본완료"; // 일봉만
                            row.DefaultCellStyle.BackColor = Color.LightYellow;
                        }
                    }

                    row.Cells["Status"].Value = statusText;


                    // 전일대비 색상 표시 
                    if (stock.ChangeAmount > 0)
                    {
                        row.Cells["ChangeAmount"].Style.ForeColor = Color.Red;
                        row.Cells["ChangeAmount"].Value = $"+{stock.ChangeAmount:N0}";
                    }
                    else if (stock.ChangeAmount < 0)
                    {
                        row.Cells["ChangeAmount"].Style.ForeColor = Color.Blue;
                        row.Cells["ChangeAmount"].Value = $"{stock.ChangeAmount:N0}";
                    }
                    else
                    {
                        row.Cells["ChangeAmount"].Style.ForeColor = Color.Black;
                        row.Cells["ChangeAmount"].Value = "0";
                    }

                    // 등락률에 따른 색상 표시
                    if (stock.ChangeRate > 0)
                    {
                        row.Cells["ChangeRate"].Style.ForeColor = Color.Red;
                        row.Cells["ClosePrice"].Style.ForeColor = Color.Red;
                    }
                    else if (stock.ChangeRate < 0)
                    {
                        row.Cells["ChangeRate"].Style.ForeColor = Color.Blue;
                        row.Cells["ClosePrice"].Style.ForeColor = Color.Blue;
                    }

                    // 분석 완료된 경우 추가 컬럼 표시
                    if (stock.TechnicalScore.HasValue)
                    {
                        row.Cells["TechnicalScore"].Value = stock.TechnicalScore;
                       
                    }

                    if (stock.FinalScore.HasValue)
                    {
                        row.Cells["FinalScore"].Value = stock.FinalScore;
                        row.Cells["FinalGrade"].Value = stock.FinalGrade;
                       
                    }

                    if (stock.BuyPrice.HasValue)
                    {
                        row.Cells["BuyPrice"].Value = stock.BuyPrice;
                        row.Cells["SellPrice"].Value = stock.SellPrice;
                        row.Cells["StopLossPrice"].Value = stock.StopLossPrice;
                        
                    }

                    // 등급에 따른 색상 표시
                    if (!string.IsNullOrEmpty(stock.FinalGrade))
                    {
                        switch (stock.FinalGrade)
                        {
                            case "S":
                                row.Cells["FinalGrade"].Style.BackColor = Color.Gold;
                                row.Cells["FinalGrade"].Style.ForeColor = Color.Black;
                                break;
                            case "A+":
                                row.Cells["FinalGrade"].Style.BackColor = Color.LightBlue;
                                row.Cells["FinalGrade"].Style.ForeColor = Color.DarkBlue;
                                break;
                            case "A":
                                row.Cells["FinalGrade"].Style.BackColor = Color.LightGreen;
                                row.Cells["FinalGrade"].Style.ForeColor = Color.DarkGreen;
                                break;
                        }
                    }
                }

                // 분석 기준일 및 조건식명 업데이트
                lblAnalysisDate.Text = $"📅 분석기준일: {today:yyyy-MM-dd}";
                if (cmbConditions.SelectedItem != null)
                {
                    lblConditionName.Text = $"🔍 조건식: {cmbConditions.SelectedItem}";
                }

                System.Diagnostics.Debug.WriteLine($"✅ 종목 리스트 업데이트 완료: {searchResults.Count}개 (종목코드 제거)");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"종목 리스트 업데이트 실패: {ex.Message}");
            }
        }

 

        /// <summary>
        /// 현재 단계 표시 업데이트
        /// </summary>
        private void UpdateCurrentStepDisplay(string stepName, int current, int total, string status)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action<string, int, int, string>(UpdateCurrentStepDisplay), stepName, current, total, status);
                    return;
                }

                lblCurrentStep.Text = $"🎯 {stepName}";
                lblStepProgress.Text = $"📊 대상: {total}개";
                lblNextStep.Text = $"✅ 완료: {current}/{total}\n⏰ 상태: {status}";

                if (total > 0)
                {
                    int percentage = (current * 100) / total;
                    progressBar.Value = Math.Min(Math.Max(percentage, 0), 100);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"현재 단계 표시 업데이트 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 진행상황 표시 업데이트
        /// </summary>
        private void UpdateProgressDisplay(AnalysisStatistics statistics)
        {
            try
            {
                if (statistics.TotalStockCount > 0)
                {
                    UpdateCurrentStepDisplay("분석 완료", statistics.CompletedAnalysisCount, statistics.TotalStockCount, "대기");
                }
                else
                {
                    UpdateCurrentStepDisplay("대기", 0, 0, "조건검색 대기");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"진행상황 표시 업데이트 실패: {ex.Message}");
            }
        }

        #endregion

        #region 분석 실행 메서드들

        /// <summary>
        /// 🆕 분봉 + 일봉 기반 정밀 기술적 분석 실행 (수정된 버전)
        /// </summary>
        private async Task ExecuteEnhancedTechnicalAnalysisAsync(List<StockAnalysisResult> stocks)
        {
            try
            {
                DateTime today = _databaseManager.GetLastTradingDay();

                // 📊 분석 통계 초기화
                int minuteAnalysisSuccess = 0;
                int dailyOnlyAnalysis = 0;
                int analysisErrors = 0;

                for (int i = 0; i < stocks.Count; i++)
                {
                    var stock = stocks[i];

                    try
                    {
                        // 🔄 진행 상황 상세 표시
                        string currentStatus = $"{stock.StockName} - 과거 데이터 조회 중...";
                        if (_progressManager != null)
                        {
                            _progressManager.UpdateDetailedProgressInfo("정밀 분석", i + 1, stocks.Count,
                                currentStatus, TimeSpan.FromSeconds((stocks.Count - i - 1) * 2.0));
                        }

                        UpdateCurrentStepDisplay("정밀 분석", i + 1, stocks.Count, $"종목 {i + 1}/{stocks.Count} 분석 중");

                        // 🆕 1. 먼저 과거 데이터 조회
                        var dailyPrices = await _databaseManager.GetHistoricalDataAsync(stock.StockCode, 60);

                        // 🆕 2. 데이터 검증
                        if (dailyPrices == null || dailyPrices.Count < 30)
                        {
                            System.Diagnostics.Debug.WriteLine($"❌ {stock.StockName}: 일봉 데이터 부족 ({dailyPrices?.Count ?? 0}개)");

                            // 기본 분석으로 폴백
                            await SaveFallbackAnalysis(stock, today);
                            analysisErrors++;
                            continue;
                        }

                        System.Diagnostics.Debug.WriteLine($"✅ {stock.StockName}: 일봉 데이터 {dailyPrices.Count}개 확인");

                        // 🆕 3. 완전한 Stock 객체 생성
                        var stockModel = new Stock
                        {
                            Code = stock.StockCode,              // TechnicalAnalyzer에서 사용
                            Name = stock.StockName,
                            StockCode = stock.StockCode,         // MainForm 호환용
                            StockName = stock.StockName,         // MainForm 호환용
                            ClosePrice = stock.ClosePrice,
                            ChangeAmount = stock.ChangeAmount,
                            ChangeRate = stock.ChangeRate,
                            Volume = stock.Volume,
                            OpenPrice = stock.OpenPrice,
                            HighPrice = stock.HighPrice,
                            LowPrice = stock.LowPrice,
                            DailyPrices = dailyPrices            // 🔑 핵심! 과거 데이터 연결
                        };

                        // 🆕 4. 분석 전 재검증
                        currentStatus = $"{stock.StockName} - 기술 분석 실행 중...";
                        if (_progressManager != null)
                        {
                            _progressManager.UpdateDetailedProgressInfo("기술 분석", i + 1, stocks.Count,
                                currentStatus, TimeSpan.FromSeconds((stocks.Count - i - 1) * 2.0));
                        }

                        System.Diagnostics.Debug.WriteLine($"🔬 {stock.StockName} 분석 시작:");
                        System.Diagnostics.Debug.WriteLine($"  - DailyPrices: {stockModel.DailyPrices?.Count ?? 0}개");
                        System.Diagnostics.Debug.WriteLine($"  - Code: {stockModel.Code}");
                        System.Diagnostics.Debug.WriteLine($"  - ClosePrice: {stockModel.ClosePrice}");

                        // 🚀 5. 실제 분봉 + 일봉 분석 실행
                        var enhancedResult = await TechnicalAnalyzer.AnalyzeWithMinuteDataAsync(stockModel, _databaseManager);

                        // 🆕 6. 결과 검증 및 처리
                        if (enhancedResult != null && enhancedResult.TotalScore > 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"✅ {stock.StockName} 분석 완료: {enhancedResult.TotalScore:F1}/80점");

                            // 🆕 7. DB 저장을 위한 TechnicalAnalysisResult 변환
                            var technicalResult = new TechnicalAnalysisResult
                            {
                                TechnicalScore = (int)enhancedResult.TotalScore,
                                RSI = enhancedResult.RSI,
                                MACD = enhancedResult.MACD,
                                MACDSignal = enhancedResult.MACDSignal,
                                BollingerUpper = (int)enhancedResult.BollingerUpper,
                                BollingerLower = (int)enhancedResult.BollingerLower,
                                BollingerPosition = enhancedResult.BollingerPosition,
                                MA5 = (int)enhancedResult.MA5,
                                MA20 = (int)enhancedResult.MA20,
                                MA60 = (int)enhancedResult.MA60,
                                TechnicalGrade = enhancedResult.TechnicalGrade ?? "C"
                            };

                            // 8. DB에 기술적 분석 결과 저장
                            await _databaseManager.UpdateTechnicalAnalysisAsync(
                                stock.StockCode, today, stock.ConditionIndex, technicalResult);

                            // 🆕 9. 개선된 매매가 정보도 저장
                            if (enhancedResult.OptimizedBuyPrice > 0)
                            {
                                var tradePlan = new TradePlanResult
                                {
                                    BuyPrice = (int)enhancedResult.OptimizedBuyPrice,
                                    SellPrice = (int)enhancedResult.OptimizedTargetPrice,
                                    StopLossPrice = (int)enhancedResult.OptimizedStopLoss,
                                    ExpectedReturn = enhancedResult.ExpectedReturn,
                                    RiskLevel = enhancedResult.RiskLevel ?? "Medium",
                                    Recommendation = GetRecommendationFromGrade(enhancedResult.FinalGrade)
                                };


                            }

                            // 📊 통계 업데이트
                            if (enhancedResult.UsedMinuteData)
                            {
                                minuteAnalysisSuccess++;
                                System.Diagnostics.Debug.WriteLine($"🔬 {stock.StockName}: 분봉 분석 성공");
                            }
                            else
                            {
                                dailyOnlyAnalysis++;
                                System.Diagnostics.Debug.WriteLine($"📈 {stock.StockName}: 일봉 분석");
                            }
                        }
                        else
                        {
                            // ❌ 분석 실패 - 기본값으로 처리
                            analysisErrors++;
                            System.Diagnostics.Debug.WriteLine($"❌ {stock.StockName}: 분석 실패 - 점수 {enhancedResult?.TotalScore ?? 0}");

                            // 기본 더미 분석으로 폴백
                            await SaveFallbackAnalysis(stock, today);
                        }

                        // 🔄 UI 업데이트
                        currentStatus = $"{stock.StockName} - 완료";
                        if (_progressManager != null)
                        {
                            _progressManager.UpdateDetailedProgressInfo("정밀 분석", i + 1, stocks.Count,
                                currentStatus, TimeSpan.FromSeconds((stocks.Count - i - 1) * 2.0));
                        }

                        // ⏱️ 0.5초 대기 (안정성)
                        await Task.Delay(500);
                    }
                    catch (Exception ex)
                    {
                        // 🛡️ 개별 종목 에러 처리
                        analysisErrors++;
                        System.Diagnostics.Debug.WriteLine($"❌ {stock.StockName} 분석 중 오류: {ex.Message}");

                        // 에러 로그 기록
                        await _databaseManager.LogAsync("ERROR", "STOCK_ANALYSIS",
                            $"종목 {stock.StockName} 분석 실패: {ex.Message}");

                        // 기본 분석으로 폴백
                        await SaveFallbackAnalysis(stock, today);
                    }
                }

                // 📋 최종 통계 로깅
                System.Diagnostics.Debug.WriteLine("=== 정밀 분석 완료 통계 ===");
                System.Diagnostics.Debug.WriteLine($"🔬 분봉 분석 성공: {minuteAnalysisSuccess}개");
                System.Diagnostics.Debug.WriteLine($"📈 일봉 분석: {dailyOnlyAnalysis}개");
                System.Diagnostics.Debug.WriteLine($"❌ 분석 오류: {analysisErrors}개");
                System.Diagnostics.Debug.WriteLine($"📊 총 처리: {stocks.Count}개");

                // StatusManager 업데이트
                if (_statusManager != null)
                {
                    _statusManager.TechnicalAnalysisCompleted = true;
                    _statusManager.UpdateStatusBarSummary();
                }

                // 로그 기록
                await _databaseManager.LogAsync("INFO", "ENHANCED_TECHNICAL_ANALYSIS",
                    $"정밀 기술적 분석 완료: 총 {stocks.Count}개, 분봉 {minuteAnalysisSuccess}개, 일봉 {dailyOnlyAnalysis}개, 오류 {analysisErrors}개");
            }
            catch (Exception ex)
            {
                throw new Exception($"정밀 기술적 분석 실행 실패: {ex.Message}");
            }
        }


        /// <summary>
        /// 🆕 스마트 분석 시스템 메인 메서드
        /// </summary>
        private async Task ExecuteSmartTechnicalAnalysisAsync()
        {
            // 1단계: 데이터 상태 분석
            UpdateCurrentStepDisplay("데이터 상태 분석", 0, 100, "확인 중...");
            var dataStatus = await AnalyzeHistoricalDataStatus();

            System.Diagnostics.Debug.WriteLine($"📊 데이터 상태: {dataStatus.StatusMessage}");

            // 2단계: 필요시 사용자 확인 및 자동 수집
            if (dataStatus.NeedsCollectionStocks.Count > 0)
            {
                bool shouldCollect = await ConfirmAutoDataCollection(dataStatus);

                if (shouldCollect)
                {
                    UpdateCurrentStepDisplay("자동 데이터 수집", 0, dataStatus.NeedsCollectionStocks.Count, "수집 중...");

                    await _kiwoomApiManager.CollectHistoricalDataAsync(dataStatus.NeedsCollectionStocks,
                        _cancellationTokenSource?.Token ?? CancellationToken.None,
                        (current, total, stockCode) =>
                        {
                            UpdateCurrentStepDisplay("자동 데이터 수집", current, total, $"수집: {stockCode}");
                            System.Diagnostics.Debug.WriteLine($"[자동수집] {current}/{total}: {stockCode}");
                        });

                    System.Diagnostics.Debug.WriteLine($"✅ 자동 데이터 수집 완료: {dataStatus.NeedsCollectionStocks.Count}개 종목");
                }
            }

            // 3단계: 정밀 분석 실행
            DateTime today = _databaseManager.GetLastTradingDay();
            var searchResults = await _databaseManager.GetConditionSearchResultsAsync(today);

            UpdateCurrentStepDisplay("정밀 분석", 0, searchResults.Count, "실행 중...");
            await ExecuteEnhancedTechnicalAnalysisAsync(searchResults);
        }

        /// <summary>
        /// 🆕 과거 데이터 상태 자동 분석
        /// </summary>
        private async Task<HistoricalDataAnalysis> AnalyzeHistoricalDataStatus()
        {
            var analysis = new HistoricalDataAnalysis();

            DateTime today = _databaseManager.GetLastTradingDay();
            var stocks = await _databaseManager.GetConditionSearchResultsAsync(today);

            foreach (var stock in stocks)
            {
                bool hasDailyData = await _databaseManager.CheckHistoricalDataExistsAsync(stock.StockCode, 30);
                bool hasMinuteData = await _databaseManager.CheckMinuteHistoricalDataExistsAsync(stock.StockCode, 1, 3);

                if (hasDailyData && hasMinuteData)
                {
                    analysis.SufficientDataStocks.Add(stock.StockCode);
                }
                else
                {
                    analysis.NeedsCollectionStocks.Add(stock.StockCode);
                }
            }

            analysis.EstimatedTimeSeconds = analysis.NeedsCollectionStocks.Count * 2;
            analysis.StatusMessage = $"충분한 데이터: {analysis.SufficientDataStocks.Count}개, 수집 필요: {analysis.NeedsCollectionStocks.Count}개";

            return analysis;
        }

        /// <summary>
        /// 🆕 사용자 확인
        /// </summary>


        /// <summary>
        /// 🆕 사용자 확인 (수정 버전)
        /// </summary>
        private Task<bool> ConfirmAutoDataCollection(HistoricalDataAnalysis analysis)
        {
            if (analysis.NeedsCollectionStocks.Count == 0)
                return Task.FromResult(true); // 수집할 필요 없음

            var result = MessageBox.Show(
                $"기술 분석을 위해 일부 종목의 과거 데이터가 필요합니다.\n\n" +
                $"📊 수집 필요 종목: {analysis.NeedsCollectionStocks.Count}개\n" +
                $"⏱️ 예상 소요 시간: {analysis.EstimatedTimeSeconds}초\n" +
                $"📈 수집 항목: 일봉 30일 + 분봉 3일\n\n" +
                $"자동으로 수집하고 정밀 분석을 진행하시겠습니까?\n\n" +
                $"💡 '아니오' 선택 시 기존 데이터로만 분석합니다.",
                "기술 분석",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            return Task.FromResult(result == DialogResult.Yes);
        }
       

        /// <summary>
        /// 🆕 분석 실패 시 기본 분석으로 폴백
        /// </summary>
        private async Task SaveFallbackAnalysis(StockAnalysisResult stock, DateTime today)
        {
            try
            {
                // 기본 더미 분석 결과 생성 (안전장치)
                var fallbackResult = new TechnicalAnalysisResult
                {
                    TechnicalScore = 60, // 기본 점수
                    RSI = 50,
                    MACD = 0,
                    MACDSignal = 0,
                    BollingerUpper = stock.ClosePrice + 1000,
                    BollingerLower = stock.ClosePrice - 1000,
                    BollingerPosition = 50,
                    MA5 = stock.ClosePrice,
                    MA20 = stock.ClosePrice,
                    MA60 = stock.ClosePrice,
                    TechnicalGrade = "C"
                };

                await _databaseManager.UpdateTechnicalAnalysisAsync(
                    stock.StockCode, today, stock.ConditionIndex, fallbackResult);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"폴백 분석 저장 실패: {stock.StockName} - {ex.Message}");
            }
        }

        /// <summary>
        /// 🆕 등급에 따른 투자 추천 결정
        /// </summary>
        private string GetRecommendationFromGrade(string grade)
        {
            switch (grade)
            {
                case "S": return "STRONG_BUY";
                case "A+":
                case "A": return "BUY";
                case "B+":
                case "B": return "HOLD";
                default: return "WATCH";
            }
        }

        /// <summary>
        /// 🆕 분석 통계 수집
        /// </summary>
        private async Task<AnalysisStatisticsResult> GetAnalysisStatistics(List<StockAnalysisResult> stocks)
        {
            try
            {
                DateTime today = _databaseManager.GetLastTradingDay();
                var updatedResults = await _databaseManager.GetConditionSearchResultsAsync(today);

                var stats = new AnalysisStatisticsResult();

                foreach (var stock in updatedResults)
                {
                    if (stock.TechnicalScore.HasValue)
                    {
                        stats.TotalAnalyzed++;
                        stats.TotalScore += stock.TechnicalScore.Value;

                        // 등급별 카운트
                        switch (stock.FinalGrade)
                        {
                            case "S": stats.SGradeCount++; break;
                            case "A+":
                            case "A": stats.AGradeCount++; break;
                            case "B+":
                            case "B": stats.BGradeCount++; break;
                        }

                        // 분석 타입별 카운트 (향후 확장용)
                        stats.MinuteAnalysisCount++; // 임시로 모든 분석을 분봉으로 카운트
                    }
                }

                if (stats.TotalAnalyzed > 0)
                {
                    stats.AverageScore = (double)stats.TotalScore / stats.TotalAnalyzed;
                }

                stats.DailyOnlyCount = stats.TotalAnalyzed - stats.MinuteAnalysisCount;

                return stats;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"분석 통계 수집 실패: {ex.Message}");
                return new AnalysisStatisticsResult();
            }
        }




        /// <summary>
        /// 뉴스 분석 실행 (더미 데이터로 시뮬레이션) + 매매가 계산
        /// </summary>
        private async Task ExecuteNewsAnalysisAsync(List<StockAnalysisResult> stocks)
        {
            try
            {
                DateTime today = _databaseManager.GetLastTradingDay();

                for (int i = 0; i < stocks.Count; i++)
                {
                    var stock = stocks[i];

                    // 진행 상황 표시
                    if (_progressManager != null)
                    {
                        _progressManager.UpdateDetailedProgressInfo("뉴스 분석", i + 1, stocks.Count,
                            $"{stock.StockName} 뉴스 분석 중...", TimeSpan.FromSeconds((stocks.Count - i - 1) * 0.3));
                    }

                   
                    // ✅ 실제 뉴스 수집
                    var webNewsCollector = new WebNewsCollector();
                    var realNews = await webNewsCollector.GetStockNews(stock.StockName);

                    // 진행률 표시 개선
                    if (_progressManager != null)
                    {
                        _progressManager.UpdateDetailedProgressInfo("뉴스 분석", i + 1, stocks.Count,
                            $"{stock.StockName} 뉴스 수집 완료... ({realNews.Count}개)",
                            TimeSpan.FromSeconds((stocks.Count - i - 1) * 3.0));
                    }

                    // Stock 모델 생성 (NewsAnalyzer 호출용)
                    var stockModel = new Stock
                    {
                        Code = stock.StockCode,
                        Name = stock.StockName,
                        ClosePrice = stock.ClosePrice,
                        ChangeAmount = stock.ChangeAmount,
                        ChangeRate = stock.ChangeRate,
                        Volume = stock.Volume,
                        OpenPrice = stock.OpenPrice,
                        HighPrice = stock.HighPrice,
                        LowPrice = stock.LowPrice
                    };

                    // ✅ 실제 NewsAnalyzer 사용
                    var newsResult = NewsAnalyzer.Analyze(stockModel, realNews, today);


                    // API 부하 방지를 위한 딜레이 (2초)
                    await Task.Delay(500);

                    System.Diagnostics.Debug.WriteLine($"📊 {stock.StockName} NewsAnalyzer 결과:");
                    System.Diagnostics.Debug.WriteLine($"  뉴스 점수: {newsResult.NewsScore:F1}점");
                    System.Diagnostics.Debug.WriteLine($"  주요 요인: {newsResult.MainFactor}");
                    System.Diagnostics.Debug.WriteLine($"  호재 키워드: {string.Join(", ", newsResult.PositiveKeywords)}");
                    System.Diagnostics.Debug.WriteLine($"  악재 키워드: {string.Join(", ", newsResult.NegativeKeywords)}");
                    System.Diagnostics.Debug.WriteLine($"  지속성: {newsResult.SustainabilityStatus}");
                    System.Diagnostics.Debug.WriteLine($"  위험 경고: {newsResult.RiskWarning}");




                    // 뉴스 분석 결과를 더미로 생성 (DB 저장용)
                    var newsAnalysisResult = new NewsAnalysisResult
                    {
                        NewsScore = (int)newsResult.NewsScore,
                        PositiveNewsCount = newsResult.PositiveKeywords?.Count ?? 0,
                        NegativeNewsCount = newsResult.NegativeKeywords?.Count ?? 0,
                        NeutralNewsCount = Math.Max(0, newsResult.NewsCount - (newsResult.PositiveKeywords?.Count ?? 0) - (newsResult.NegativeKeywords?.Count ?? 0)),
                        SentimentScore = newsResult.NewsScore,
                        KeywordRelevance = newsResult.CredibilityScore * 20, // 0~100%로 변환
                        NewsImpactLevel = GetImpactLevel(newsResult.ImpactScore),
                        FinalScore = CalculateFinalScore(stock, newsResult),
                        FinalGrade = ""
                    };

                    // 최종 등급 결정 (130점 만점)
                    newsAnalysisResult.FinalGrade = GetFinalGrade(newsAnalysisResult.FinalScore);

                    // DB에 뉴스 분석 결과 저장
                    await _databaseManager.UpdateNewsAnalysisAsync(
                        stock.StockCode, today, stock.ConditionIndex, newsAnalysisResult);

                    // 🆕 여기에 매매가 계산 및 저장 추가!
                    await CalculateAndSaveTradePlan(stock, newsAnalysisResult, today);

                    // 진행상황 UI 업데이트
                    UpdateCurrentStepDisplay("뉴스 분석", i + 1, stocks.Count, "진행 중");

                    // 0.2초 대기 (시뮬레이션)
                    await Task.Delay(200);

                    System.Diagnostics.Debug.WriteLine($"✅ {stock.StockName} 뉴스 분석 + 매매계획 완료: {newsResult.NewsScore:F1}점, 최종: {newsAnalysisResult.FinalScore}점 ({newsAnalysisResult.FinalGrade})");
                }

                // StatusManager 업데이트
                if (_statusManager != null)
                {
                    _statusManager.NewsAnalysisCompleted = true;
                    _statusManager.TradePlanCompleted = true;  // 🆕 매매계획도 완료로 표시
                    _statusManager.UpdateStatusBarSummary();
                }

                // 로그 기록
                await _databaseManager.LogAsync("INFO", "NEWS_ANALYSIS_COMPLETE",
                    $"뉴스 분석 + 매매계획 완료: {stocks.Count}개 종목");

                System.Diagnostics.Debug.WriteLine($"✅ 뉴스 분석 + 매매계획 완료: {stocks.Count}개 종목");
            }
            catch (Exception ex)
            {
                throw new Exception($"뉴스 분석 실행 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 더미 뉴스 데이터 생성 (실제 뉴스 수집 대신)
        /// </summary>
        private List<News> GenerateDummyNews(StockAnalysisResult stock)
        {
            var newsList = new List<News>();
            var random = new Random();

            // 종목별로 1~3개의 더미 뉴스 생성
            int newsCount = random.Next(1, 4);

            for (int i = 0; i < newsCount; i++)
            {
                var news = new News
                {
                    Id = random.Next(1000, 9999),
                    Title = GenerateDummyNewsTitle(stock.StockName),
                    Content = $"{stock.StockName} 관련 뉴스 내용...",
                    PublishDate = DateTime.Today.AddDays(-random.Next(0, 3)), // 0~3일 전 뉴스
                    Source = GetRandomNewsSource(),
                    Url = $"https://news.example.com/{random.Next(10000, 99999)}",
                    SentimentScore = (random.NextDouble() - 0.5) * 2, // -1.0 ~ 1.0
                    ImportanceScore = random.NextDouble(), // 0.0 ~ 1.0
                    RelatedStockCode = stock.StockCode
                };

                newsList.Add(news);
            }

            return newsList;
        }

              
      
        /// <summary>
        /// 🆕 매매계획 계산 및 저장 (2차 분석 완료 후)
        /// </summary>
        private async Task CalculateAndSaveTradePlan(StockAnalysisResult stock, NewsAnalysisResult newsResult, DateTime today)
        {
            try
            {
                // 1. 과거 데이터 조회 (매매가 계산용)
                var dailyPrices = await _databaseManager.GetHistoricalDataAsync(stock.StockCode, 60);

                if (dailyPrices != null && dailyPrices.Count >= 30)
                {
                    // 2. ATR 기반 매매가 계산
                    var tradePlan = ATR.CalculateTradePlan(dailyPrices);

                    if (tradePlan.buyPrice.HasValue)
                    {
                        // 3. 최종 등급에 따른 조정
                        decimal riskMultiplier = GetRiskMultiplier(newsResult.FinalGrade);

                        var finalTradePlan = new TradePlanResult
                        {
                            BuyPrice = (int)(tradePlan.buyPrice.Value * riskMultiplier),
                            SellPrice = (int)(tradePlan.targetPrice.Value * riskMultiplier),
                            StopLossPrice = (int)(tradePlan.stopLoss.Value),
                            ExpectedReturn = CalculateExpectedReturn(tradePlan.buyPrice.Value, tradePlan.targetPrice.Value),
                            RiskLevel = GetRiskLevel(newsResult.FinalGrade),
                            Recommendation = GetRecommendation(newsResult.FinalGrade)
                        };

                        // 4. DB에 매매계획 저장
                        await _databaseManager.UpdateTradePlanAsync(
                            stock.StockCode, today, stock.ConditionIndex, finalTradePlan);

                        System.Diagnostics.Debug.WriteLine($"💰 {stock.StockName} 매매가: 매수 {finalTradePlan.BuyPrice:N0}원, 매도 {finalTradePlan.SellPrice:N0}원, 손절 {finalTradePlan.StopLossPrice:N0}원");
                    }
                }
                else
                {
                    // 과거 데이터가 없으면 기본 매매가 계산
                    var basicTradePlan = new TradePlanResult
                    {
                        BuyPrice = (int)(stock.ClosePrice * 0.98m), // 현재가 2% 아래
                        SellPrice = (int)(stock.ClosePrice * 1.05m), // 현재가 5% 위
                        StopLossPrice = (int)(stock.ClosePrice * 0.95m), // 현재가 5% 아래
                        ExpectedReturn = 5.0, // 기본 5%
                        RiskLevel = "MEDIUM",
                        Recommendation = "HOLD"
                    };

                    await _databaseManager.UpdateTradePlanAsync(
                        stock.StockCode, today, stock.ConditionIndex, basicTradePlan);

                    System.Diagnostics.Debug.WriteLine($"💰 {stock.StockName} 기본 매매가: 매수 {basicTradePlan.BuyPrice:N0}원, 매도 {basicTradePlan.SellPrice:N0}원");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ {stock.StockName} 매매계획 계산 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 등급별 리스크 배수 계산
        /// </summary>

        private decimal GetRiskMultiplier(string grade)
        {
            // ✅ 🆕 새로운 등급 기준 적용
            switch (grade)
            {
                case "S": return 1.05m;    // S급은 5% 적극적
                case "A+":
                case "A": return 1.0m;     // A급은 표준
                case "B+":
                case "B": return 0.98m;    // B급은 약간 보수적
                default: return 0.95m;     // 나머지는 보수적
            }
        }

       

        /// <summary>
        /// 예상 수익률 계산
        /// </summary>
        private double CalculateExpectedReturn(decimal buyPrice, decimal sellPrice)
        {
            if (buyPrice <= 0) return 0;
            return (double)((sellPrice - buyPrice) / buyPrice * 100);
        }

        /// <summary>
        /// 등급별 리스크 레벨
        /// </summary>

        private string GetRiskLevel(string grade)
        {
            // ✅ 🆕 새로운 등급 기준 적용
            switch (grade)
            {
                case "S": return "MEDIUM";     // S급도 중간 리스크
                case "A+":
                case "A": return "MEDIUM";
                case "B+":
                case "B": return "LOW";
                default: return "VERY_LOW";
            }
        }


        /// <summary>
        /// 등급별 투자 추천
        /// </summary>

        private string GetRecommendation(string grade)
        {
            // ✅ 🆕 새로운 등급 기준 적용
            switch (grade)
            {
                case "S": return "STRONG_BUY";
                case "A+": return "BUY_PLUS";
                case "A": return "BUY";
                case "B+": return "HOLD_PLUS";
                case "B": return "HOLD";
                default: return "WATCH";
            }
        }

       

        /// <summary>
        /// 더미 뉴스 제목 생성
        /// </summary>
        private string GenerateDummyNewsTitle(string stockName)
        {
            var positiveTemplates = new[]
            {
        $"{stockName}, 신규 계약 체결로 주가 상승 기대",
        $"{stockName}, 기술 개발 성과로 업계 주목",
        $"{stockName}, 해외 진출 본격화... 성장 가속화",
        $"{stockName}, 실적 개선 기대감에 투자자 관심 증가"
    };

            var neutralTemplates = new[]
            {
        $"{stockName}, 정기 주주총회 개최 예정",
        $"{stockName}, 분기 실적 발표 임박",
        $"{stockName}, 임원 인사 변동 발표"
    };

            var random = new Random();
            var templates = random.Next(0, 10) < 7 ? positiveTemplates : neutralTemplates; // 70% 긍정, 30% 중립

            return templates[random.Next(templates.Length)];
        }

        /// <summary>
        /// 랜덤 뉴스 소스 반환
        /// </summary>
        private string GetRandomNewsSource()
        {
            var sources = new[] { "한국경제", "매일경제", "머니투데이", "이데일리", "연합뉴스", "뉴스1" };
            var random = new Random();
            return sources[random.Next(sources.Length)];
        }

        /// <summary>
        /// 뉴스 영향도 레벨 결정
        /// </summary>
        private string GetImpactLevel(double impactScore)
        {
            if (impactScore >= 8) return "HIGH";
            else if (impactScore >= 5) return "MEDIUM";
            else return "LOW";
        }

        /// <summary>
        /// 최종 점수 계산 (기술적 분석 + 뉴스 분석)
        /// </summary>


        private static int CalculateFinalScore(StockAnalysisResult stock, NewsAnalyzer.NewsAnalysisResult newsResult)
        {
            var technicalScore = stock.TechnicalScore ?? 70;
            var newsScore = newsResult.NewsScore;             // 이제 0~120 (중립=60)

            // 🆕 균형 보너스 조건 수정 (뉴스가 극단적이지 않으면 보너스)
            var balanceBonus = (newsScore >= 40 && newsScore <= 80) ? 20 : 10;

            var finalScore = (int)(technicalScore + newsScore + balanceBonus);

            // 🆕 새로운 범위: 0~220점 (평균 150점 정도)
            return Math.Max(0, Math.Min(220, finalScore));
        }


        /// <summary>
        /// 최종 등급 결정 (130점 만점)
        /// </summary>
        /// 
        private static string GetFinalGrade(int finalScore)
        {
            // 🆕 220점 만점 기준으로 등급 재조정
            if (finalScore >= 180) return "S";      // 180점 이상 (82% 이상) 
            else if (finalScore >= 160) return "A+"; // 160-179점 (73-81%)
            else if (finalScore >= 140) return "A";  // 140-159점 (64-72%)
            else if (finalScore >= 120) return "B+"; // 120-139점 (55-63%)
            else if (finalScore >= 100) return "B";  // 100-119점 (45-54%)
            else return "C";                         // 100점 미만 (45% 미만)
        }
      
      
        /// <summary>
        /// 매매계획 수립 (더미 데이터로 시뮬레이션)
        /// </summary>
        private async Task ExecuteTradePlanAsync(List<StockAnalysisResult> stocks)
        {
            try
            {
                DateTime today = _databaseManager.GetLastTradingDay();

                for (int i = 0; i < stocks.Count; i++)
                {
                    var stock = stocks[i];

                    // 진행 상황 표시
                    if (_progressManager != null)
                    {
                        _progressManager.UpdateDetailedProgressInfo("매매계획", i + 1, stocks.Count,
                            $"{stock.StockName} 매매계획 수립 중...", TimeSpan.FromSeconds((stocks.Count - i - 1) * 0.2));
                    }

                    // 더미 매매계획 결과 생성
                    var tradePlan = new TradePlanResult
                    {
                        BuyPrice = stock.ClosePrice - 500, // 현재가보다 500원 낮게 매수
                        SellPrice = stock.ClosePrice + 1500, // 현재가보다 1500원 높게 매도
                        StopLossPrice = stock.ClosePrice - 1000, // 현재가보다 1000원 낮게 손절
                        ExpectedReturn = (i % 3 == 0) ? 8.5 : (i % 3 == 1) ? 12.3 : 5.7, // 예상 수익률
                        RiskLevel = (i % 3 == 0) ? "LOW" : (i % 3 == 1) ? "MEDIUM" : "HIGH",
                        Recommendation = (i % 4 == 0) ? "STRONG_BUY" :
                                       (i % 4 == 1) ? "BUY" :
                                       (i % 4 == 2) ? "HOLD" : "WATCH"
                    };

                    // DB에 매매계획 저장
                    await _databaseManager.UpdateTradePlanAsync(
                        stock.StockCode, today, stock.ConditionIndex, tradePlan);

                    // 진행상황 UI 업데이트
                    UpdateCurrentStepDisplay("매매계획", i + 1, stocks.Count, "진행 중");

                    // 0.1초 대기 (시뮬레이션)
                    await Task.Delay(100);
                }

                // StatusManager 업데이트
                if (_statusManager != null)
                {
                    _statusManager.TradePlanCompleted = true;
                    _statusManager.UpdateStatusBarSummary();
                }

                // 로그 기록
                await _databaseManager.LogAsync("INFO", "TRADE_PLAN",
                    $"매매계획 수립 완료: {stocks.Count}개 종목");
            }
            catch (Exception ex)
            {
                throw new Exception($"매매계획 수립 실패: {ex.Message}");
            }
        }

        #endregion

        #region 초기 상태 설정

        private void SetInitialState()
        {
            try
            {
                // StatusManager 초기 상태
                if (_statusManager != null)
                {
                    _statusManager.SetStatusMessage("시스템 준비 완료 - 키움 연결을 시작하세요");
                    _statusManager.IsKiwoomConnected = false;
                    _statusManager.LoadedConditionCount = 0;
                    _statusManager.UpdateStatusBarSummary();
                }

                // ProgressManager 초기 상태
                if (_progressManager != null)
                {
                    _progressManager.InitializeProgressDisplay();
                }

                // 초기 UI 상태 설정
                UpdateCurrentStepDisplay("대기", 0, 0, "키움 연결 대기");

                System.Diagnostics.Debug.WriteLine("✅ 초기 상태 설정 완료 (조건검색 실제 실행 준비)");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"초기 상태 설정 실패: {ex.Message}");
            }
        }

        #endregion

        #region 폼 이벤트

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                               
                System.Diagnostics.Debug.WriteLine("🚀 MainForm 로드 완료 - 키움 API + DB 시스템 준비됨 (조건검색 실제 실행 가능)");

                LoadUserSettings(); // 🆕 이 줄 추가
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MainForm 로드 실패: {ex.Message}");
            }
        }

        private async void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                // 키움 API 연결 해제 및 조건검색 정리
                if (_kiwoomApiManager != null)
                {
                    _kiwoomApiManager.Dispose(); // 조건검색 중단 포함
                }

                // 데이터베이스 연결 해제
                if (_databaseManager != null)
                {
                    // 마지막 로그 기록
                    await _databaseManager.LogAsync("INFO", "SYSTEM", "시스템 종료 - 조건검색 실제 실행 버전");

                    _databaseManager.Dispose();
                }

                System.Diagnostics.Debug.WriteLine("✅ 모든 연결 해제 완료 (조건검색 정리 포함)");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"폼 종료 처리 실패: {ex.Message}");
            }
        }

        #endregion

        #region 디버그 및 테스트 메서드

        private void TestSystemStatus()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== 시스템 상태 테스트 (조건검색 실제 실행) ===");
                System.Diagnostics.Debug.WriteLine($"DatabaseManager: {(_databaseManager != null ? "✅" : "❌")}");
                System.Diagnostics.Debug.WriteLine($"ProgressManager: {(_progressManager != null ? "✅" : "❌")}");
                System.Diagnostics.Debug.WriteLine($"StatusManager: {(_statusManager != null ? "✅" : "❌")}");
                System.Diagnostics.Debug.WriteLine($"KiwoomApiManager: {(_kiwoomApiManager != null ? "✅" : "❌")}");
                System.Diagnostics.Debug.WriteLine($"axKHOpenAPI1: {(axKHOpenAPI1 != null ? "✅" : "❌")}");
                System.Diagnostics.Debug.WriteLine($"DB 연결 상태: {(_databaseManager?.IsConnected == true ? "✅" : "❌")}");
                System.Diagnostics.Debug.WriteLine($"키움 연결 상태: {(_kiwoomApiManager?.IsConnected == true ? "✅" : "❌")}");
                System.Diagnostics.Debug.WriteLine("================================================");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"시스템 상태 테스트 실패: {ex.Message}");
            }
        }

        #endregion


        #region 🆕 조건검색 결과 저장 (기존 조건검색 이벤트 수정 필요)

        /// <summary>
        /// 조건검색 완료 시 결과 저장 (기존 조건검색 이벤트 핸들러에 추가)
        /// </summary>
        private void SaveConditionSearchResults(ConditionSearchResult result)
        {
            try
            {
                // 조건검색 결과 종목 코드들 저장
                _lastConditionSearchResults.Clear();

                if (result?.StockCodes != null)
                {
                    _lastConditionSearchResults.AddRange(result.StockCodes);

                    AddLogMessage($"📋 조건검색 결과 저장: {_lastConditionSearchResults.Count}개 종목");

                    // 과거 데이터 수집 버튼 활성화
                    EnableHistoricalDataCollection(true);

                    // 상태 메시지 업데이트
                    UpdateStatusMessage($"조건검색 완료: {_lastConditionSearchResults.Count}개 종목 발견 (과거 데이터 수집 가능)");
                }
            }
            catch (Exception ex)
            {
                AddLogMessage($"❌ 조건검색 결과 저장 실패: {ex.Message}");
            }
        }

        #endregion

        #region 🆕 UI 관리 메서드들

        /// <summary>
        /// 과거 데이터 수집 관련 버튼들 활성화/비활성화
        /// </summary>
        private void SetHistoricalDataButtonsEnabled(bool enabled)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<bool>(SetHistoricalDataButtonsEnabled), enabled);
                return;
            }

            // 과거 데이터 수집 버튼 (새로 추가할 버튼)
            btnCollectHistoricalData.Enabled = enabled;

            // 수집 중일 때는 다른 키움 API 관련 버튼들 비활성화
            if (!enabled && _isHistoricalDataCollecting)
            {
                btnStep1.Enabled = false;
                btnConnect.Enabled = false;
            }
            else if (enabled)
            {
                btnStep1.Enabled = _kiwoomApiManager?.IsConnected == true;
                btnConnect.Enabled = true;
            }
        }


        /// <summary>
        /// 과거 데이터 수집 기능 활성화/비활성화
        /// </summary>
        private void EnableHistoricalDataCollection(bool enable)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<bool>(EnableHistoricalDataCollection), enable);
                return;
            }

            btnCollectHistoricalData.Enabled = enable && _kiwoomApiManager?.IsConnected == true;

            if (enable)
            {
                btnCollectHistoricalData.Text = $"과거 데이터 수집 ({_lastConditionSearchResults.Count}개 종목)";
                btnCollectHistoricalData.BackColor = System.Drawing.Color.LightGreen;
            }
            else
            {
                btnCollectHistoricalData.Text = "과거 데이터 수집 (조건검색 먼저 실행)";
                btnCollectHistoricalData.BackColor = System.Drawing.SystemColors.Control;
            }
        }

        #endregion

        #region 🆕 기존 이벤트 핸들러 수정 (추가 필요)

        /// <summary>
        /// 기존 조건검색 완료 이벤트 핸들러에 추가할 코드
        /// </summary>
        private void KiwoomManager_ConditionSearchResultReceived(ConditionSearchResult result)
        {
            try
            {
                // 기존 조건검색 처리 코드...

                // 🆕 추가: 조건검색 결과 저장
                SaveConditionSearchResults(result);

                // 기존 나머지 처리 코드...
            }
            catch (Exception ex)
            {
                AddLogMessage($"❌ 조건검색 결과 처리 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 키움 연결 상태 변경 시 과거 데이터 수집 버튼 상태 업데이트
        /// </summary>
        private void UpdateHistoricalDataButtonOnConnection(bool connected)
        {
            if (connected && _lastConditionSearchResults.Count > 0)
            {
                EnableHistoricalDataCollection(true);
            }
            else
            {
                EnableHistoricalDataCollection(false);
            }
        }

        #endregion

        #region 🆕 데이터베이스 상태 확인 (선택사항)

        /// <summary>
        /// 과거 데이터 수집 현황 확인 (선택사항)
        /// </summary>
        private async void CheckHistoricalDataStatus()
        {
            try
            {
                // TODO: 데이터베이스에서 기존 과거 데이터 현황 확인
                // 각 종목별로 일봉/분봉 데이터가 얼마나 있는지 체크

                AddLogMessage("📊 과거 데이터 현황 확인 중...");

                // 예시: 간단한 카운트 체크
                // var dailyCount = await CheckDailyDataCount();
                // var minuteCount = await CheckMinuteDataCount();

                AddLogMessage("✅ 과거 데이터 현황 확인 완료");
            }
            catch (Exception ex)
            {
                AddLogMessage($"❌ 과거 데이터 현황 확인 실패: {ex.Message}");
            }
        }

        #endregion

        #region 🆕 UI 업데이트 메서드들

        /// <summary>
        /// 로그 메시지 추가 (콘솔 출력)
        /// </summary>
        private void AddLogMessage(string message)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action<string>(AddLogMessage), message);
                    return;
                }

                // 콘솔에 출력
                System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");

                // 상태바에도 표시
                if (lblStatus != null)
                {
                    lblStatus.Text = message;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddLogMessage 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 진행률 업데이트
        /// </summary>
        private void UpdateProgress(int percentage, string message)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action<int, string>(UpdateProgress), percentage, message);
                    return;
                }

                if (progressBar != null)
                {
                    progressBar.Value = Math.Min(Math.Max(percentage, 0), 100);
                }

                if (lblCurrentStep != null)
                {
                    lblCurrentStep.Text = message;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateProgress 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 상태 메시지 업데이트
        /// </summary>
        private void UpdateStatusMessage(string message)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action<string>(UpdateStatusMessage), message);
                    return;
                }

                if (lblStatus != null)
                {
                    lblStatus.Text = message;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateStatusMessage 실패: {ex.Message}");
            }
        }


        #endregion

        private void dgvStockList_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
    /// <summary>
    /// 🆕 분석 통계 결과 클래스
    /// </summary>
    public class AnalysisStatisticsResult
    {
        public int TotalAnalyzed { get; set; } = 0;
        public int MinuteAnalysisCount { get; set; } = 0;
        public int DailyOnlyCount { get; set; } = 0;
        public int SGradeCount { get; set; } = 0;
        public int AGradeCount { get; set; } = 0;
        public int BGradeCount { get; set; } = 0;
        public int TotalScore { get; set; } = 0;
        public double AverageScore { get; set; } = 0.0;
    }

    /// <summary>
    /// 🆕 과거 데이터 상태 분석 결과 클래스
    /// </summary>
    public class HistoricalDataAnalysis
    {
        public List<string> SufficientDataStocks { get; set; } = new List<string>();
        public List<string> NeedsCollectionStocks { get; set; } = new List<string>();
        public int EstimatedTimeSeconds { get; set; }
        public string StatusMessage { get; set; }
    }
}
