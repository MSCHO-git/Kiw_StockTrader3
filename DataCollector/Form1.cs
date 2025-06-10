using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DataCollector.Managers; // DatabaseManager, StockDataReader, SimpleKiwoomManager

namespace DataCollector
{
    public partial class Form1 : Form
    {
        #region 필드

        private DatabaseManager _databaseManager;
        private StockDataReader _stockDataReader;
        private SimpleKiwoomManager _kiwoomApiManager; // ✅ SimpleKiwoomManager 사용
        private bool _isCollecting = false;
        private CancellationTokenSource _cancellationTokenSource;

        #endregion

        #region 생성자 및 초기화

        public Form1()
        {
            InitializeComponent(); // Designer에서 자동 생성된 것 호출
            InitializeDatabase();
            InitializeStockDataReader();
            InitializeKiwoomApiManager(); // ✅ SimpleKiwoomManager 초기화
        }

        #endregion

        #region 이벤트 핸들러

        private async void Form1_Load(object sender, EventArgs e)
        {
            AddLog("시스템 초기화 완료 (일봉 + 분봉 지원)");
            AddLog("데이터베이스 연결 상태: " + (_databaseManager?.IsConnected == true ? "연결됨" : "연결 안됨"));

            // StockTrader3 DB 상태 확인
            if (_stockDataReader != null)
            {
                var status = await _stockDataReader.CheckDatabaseStatusAsync();
                AddLog($"StockTrader3 DB 상태: {status}");

                // 전체 종목 수 미리 확인
                var allStocks = await _stockDataReader.GetAllStockCodesAsync();
                if (allStocks != null && allStocks.Count > 0)
                {
                    AddLog($"🎯 수집 대상 종목: {allStocks.Count}개 발견");
                    AddLog($"📊 예상 데이터량: 일봉 {allStocks.Count * 60}개 + 1분봉 {allStocks.Count * 1170}개");
                }
                else
                {
                    AddLog("⚠️ StockTrader3에서 조회할 종목이 없습니다");
                }
            }
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            if (_isCollecting) return;

            try
            {
                _isCollecting = true;
                btnStart.Enabled = false;
                btnStop.Enabled = true;
                progressBar1.Value = 0;
                UpdateProgress(0, "데이터 수집 시작...");

                _cancellationTokenSource = new CancellationTokenSource();

                AddLog("=== 일봉 + 분봉 데이터 수집 시작 ===");
                await StartDataCollection(_cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                AddLog($"❌ 오류 발생: {ex.Message}");
                MessageBox.Show($"데이터 수집 중 오류가 발생했습니다: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isCollecting = false;
                btnStart.Enabled = true;
                btnStop.Enabled = false;
                UpdateProgress(100, "완료");
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (!_isCollecting) return;

            AddLog("🛑 사용자가 수집 중지 요청");
            _cancellationTokenSource?.Cancel();

            _isCollecting = false;
            btnStart.Enabled = true;
            btnStop.Enabled = false;
            UpdateProgress(0, "중지됨");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_isCollecting)
            {
                var result = MessageBox.Show("데이터 수집이 진행 중입니다. 정말 종료하시겠습니까?",
                    "확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }

                _cancellationTokenSource?.Cancel();
            }

            // 리소스 정리
            _kiwoomApiManager?.Dispose();
            _stockDataReader?.Dispose();
            _databaseManager?.Dispose();
        }

        #endregion

        #region 초기화 메서드들

        private void InitializeDatabase()
        {
            try
            {
                Task.Run(async () =>
                {
                    _databaseManager = new DatabaseManager();
                    bool success = await _databaseManager.InitializeAsync();

                    if (success)
                    {
                        // 🆕 일봉 + 분봉 테이블 모두 생성
                        await _databaseManager.CreateAllTablesAsync();
                        AddLog("✅ DataCollector 데이터베이스 초기화 성공 (일봉 + 분봉)");
                    }
                    else
                    {
                        AddLog("❌ DataCollector 데이터베이스 초기화 실패");
                    }
                });
            }
            catch (Exception ex)
            {
                AddLog($"❌ DataCollector 데이터베이스 초기화 오류: {ex.Message}");
            }
        }

        private void InitializeStockDataReader()
        {
            try
            {
                _stockDataReader = new StockDataReader();
                AddLog("✅ StockDataReader 초기화 완료");
            }
            catch (Exception ex)
            {
                AddLog($"❌ StockDataReader 초기화 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// SimpleKiwoomManager 초기화
        /// </summary>
        private void InitializeKiwoomApiManager()
        {
            try
            {
                _kiwoomApiManager = new SimpleKiwoomManager(this); // ✅ 현재 Form 전달

                // 이벤트 연결
                _kiwoomApiManager.StatusUpdated += OnStatusUpdated;
                _kiwoomApiManager.ConnectionChanged += OnConnectionChanged;

                AddLog("✅ SimpleKiwoomManager 초기화 완료 (일봉 + 분봉)");
            }
            catch (Exception ex)
            {
                AddLog($"❌ SimpleKiwoomManager 초기화 오류: {ex.Message}");
            }
        }

        #endregion

        #region 키움 API 이벤트 핸들러

        private void OnStatusUpdated(string status)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(OnStatusUpdated), status);
                return;
            }

            AddLog($"키움: {status}");
        }

        private void OnConnectionChanged(bool connected)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<bool>(OnConnectionChanged), connected);
                return;
            }

            AddLog($"키움 연결 상태 변경: {(connected ? "연결됨" : "연결 해제됨")}");
        }

        #endregion

        #region 데이터 수집 로직 (✅ 일봉 + 분봉 통합)

        private async Task StartDataCollection(CancellationToken cancellationToken)
        {
            try
            {
                // 1단계: StockTrader3 DB에서 모든 조건검색 종목 가져오기
                AddLog("📋 StockTrader3에서 모든 조건검색 종목 확인 중...");
                UpdateProgress(5, "종목 코드 조회 중...");

                var allStockCodes = await _stockDataReader.GetAllStockCodesAsync();

                if (allStockCodes == null || allStockCodes.Count == 0)
                {
                    AddLog("❌ StockTrader3에서 조건검색 종목을 찾을 수 없습니다");
                    MessageBox.Show("StockTrader3.db에서 조건검색 결과를 찾을 수 없습니다.\n먼저 StockTrader3에서 조건검색을 실행해주세요.",
                        "데이터 없음", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                AddLog($"🎯 총 {allStockCodes.Count}개 조건검색 종목 발견");
                for (int i = 0; i < Math.Min(allStockCodes.Count, 10); i++)
                {
                    AddLog($"  └ {i + 1}. {allStockCodes[i]}");
                }
                if (allStockCodes.Count > 10)
                {
                    AddLog($"  └ ... 외 {allStockCodes.Count - 10}개 종목");
                }

                UpdateProgress(10, $"총 {allStockCodes.Count}개 종목 확인 완료");

                if (cancellationToken.IsCancellationRequested) return;

                // 2단계: 일봉 데이터 체크 및 수집
                AddLog("📊 === 1단계: 일봉 데이터 수집 ===");
                await CollectDailyData(allStockCodes, cancellationToken);

                if (cancellationToken.IsCancellationRequested) return;

                // 3단계: 분봉 데이터 체크 및 수집 🆕
                AddLog("📈 === 2단계: 분봉 데이터 수집 ===");
                await CollectMinuteData(allStockCodes, cancellationToken);

                // 4단계: 최종 결과 정리
                AddLog("📋 === 데이터 수집 완료 ===");
                await ShowFinalResults(allStockCodes);

            }
            catch (OperationCanceledException)
            {
                AddLog("🛑 데이터 수집이 사용자에 의해 중단되었습니다.");
            }
            catch (Exception ex)
            {
                AddLog($"❌ 데이터 수집 중 오류: {ex.Message}");

                // 에러 발생 시에도 키움 API 연결 해제
                try
                {
                    _kiwoomApiManager?.Disconnect();
                }
                catch { }

                throw;
            }
        }

        /// <summary>
        /// 일봉 데이터 수집 (기존 로직)
        /// </summary>
        private async Task CollectDailyData(List<string> allStockCodes, CancellationToken cancellationToken)
        {
            try
            {
                AddLog("🔍 각 종목별 일봉 데이터 존재 여부 확인 중...");

                var stocksNeedingDailyData = new List<string>();

                for (int i = 0; i < allStockCodes.Count; i++)
                {
                    if (cancellationToken.IsCancellationRequested) return;

                    string stockCode = allStockCodes[i];
                    bool hasData = await _databaseManager.CheckHistoricalDataExistsAsync(stockCode, 60);

                    if (!hasData)
                    {
                        stocksNeedingDailyData.Add(stockCode);
                        AddLog($"  └ {stockCode}: 일봉 데이터 부족 (수집 필요)");
                    }
                    else
                    {
                        AddLog($"  └ {stockCode}: 일봉 데이터 충분 (스킵)");
                    }

                    int checkProgress = 10 + (i + 1) * 15 / allStockCodes.Count;
                    UpdateProgress(checkProgress, $"일봉 데이터 확인 중... ({i + 1}/{allStockCodes.Count})");
                }

                AddLog($"📊 일봉 수집 필요 종목: {stocksNeedingDailyData.Count}개");

                if (stocksNeedingDailyData.Count == 0)
                {
                    AddLog("ℹ️ 모든 종목의 일봉 데이터가 충분합니다.");
                    UpdateProgress(40, "일봉 데이터 충분 - 분봉 단계로");
                    return;
                }

                // 키움 API 연결
                if (!_kiwoomApiManager.IsConnected)
                {
                    AddLog($"🔄 키움 API 연결 중... ({stocksNeedingDailyData.Count}개 일봉 수집 예정)");
                    UpdateProgress(25, "키움 API 연결 중...");

                    bool connected = await _kiwoomApiManager.ConnectAsync();
                    if (!connected)
                    {
                        throw new Exception("키움증권 연결에 실패했습니다.");
                    }
                }

                // 일봉 데이터 수집
                AddLog("🚀 일봉 데이터 수집 시작");
                int dailySuccessCount = 0;
                int dailyFailCount = 0;

                for (int i = 0; i < stocksNeedingDailyData.Count; i++)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    string stockCode = stocksNeedingDailyData[i];

                    try
                    {
                        AddLog($"📈 [{i + 1}/{stocksNeedingDailyData.Count}] {stockCode} 일봉 60일 데이터 수집 중...");

                        int progressBase = 25 + (i * 15 / stocksNeedingDailyData.Count);
                        UpdateProgress(progressBase, $"일봉 수집 중... ({i + 1}/{stocksNeedingDailyData.Count}) {stockCode}");

                        var historicalData = await _kiwoomApiManager.GetHistoricalDataAsync(stockCode, 60);

                        if (cancellationToken.IsCancellationRequested) break;

                        if (historicalData != null && historicalData.Count > 0)
                        {
                            int savedCount = await _databaseManager.SaveHistoricalDataAsync(stockCode, historicalData);

                            if (savedCount > 0)
                            {
                                AddLog($"  ✅ {stockCode}: 일봉 {savedCount}일치 저장 완료");
                                dailySuccessCount++;
                            }
                            else
                            {
                                AddLog($"  ❌ {stockCode}: 일봉 저장 실패");
                                dailyFailCount++;
                            }
                        }
                        else
                        {
                            AddLog($"  ❌ {stockCode}: 일봉 수집 실패");
                            dailyFailCount++;
                        }

                        // API 안정성을 위한 대기
                        if (i < stocksNeedingDailyData.Count - 1)
                        {
                            await Task.Delay(2000, cancellationToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        AddLog($"  ❌ {stockCode}: 일봉 수집 실패 - {ex.Message}");
                        dailyFailCount++;
                        continue;
                    }
                }

                AddLog($"✅ 일봉 수집 완료: 성공 {dailySuccessCount}개, 실패 {dailyFailCount}개");
                UpdateProgress(40, "일봉 수집 완료 - 분봉 단계로");

            }
            catch (Exception ex)
            {
                throw new Exception($"일봉 데이터 수집 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 분봉 데이터 수집 🆕 새로 추가
        /// </summary>
        private async Task CollectMinuteData(List<string> allStockCodes, CancellationToken cancellationToken)
        {
            try
            {
                AddLog("🔍 각 종목별 1분봉 데이터 존재 여부 확인 중...");

                var stocksNeedingMinuteData = new List<string>();

                for (int i = 0; i < allStockCodes.Count; i++)
                {
                    if (cancellationToken.IsCancellationRequested) return;

                    string stockCode = allStockCodes[i];
                    bool hasData = await _databaseManager.CheckMinuteHistoricalDataExistsAsync(stockCode, 1, 3); // 1분봉 3일

                    if (!hasData)
                    {
                        stocksNeedingMinuteData.Add(stockCode);
                        AddLog($"  └ {stockCode}: 1분봉 데이터 부족 (수집 필요)");
                    }
                    else
                    {
                        AddLog($"  └ {stockCode}: 1분봉 데이터 충분 (스킵)");
                    }

                    int checkProgress = 40 + (i + 1) * 15 / allStockCodes.Count;
                    UpdateProgress(checkProgress, $"분봉 데이터 확인 중... ({i + 1}/{allStockCodes.Count})");
                }

                AddLog($"📊 1분봉 수집 필요 종목: {stocksNeedingMinuteData.Count}개");
                AddLog($"📊 예상 1분봉 데이터: {stocksNeedingMinuteData.Count * 1170}개 (3일치)");

                if (stocksNeedingMinuteData.Count == 0)
                {
                    AddLog("ℹ️ 모든 종목의 1분봉 데이터가 충분합니다.");
                    UpdateProgress(90, "분봉 데이터 충분 - 수집 완료");
                    return;
                }

                // 키움 API 연결 확인 (이미 연결되어 있을 수 있음)
                if (!_kiwoomApiManager.IsConnected)
                {
                    AddLog($"🔄 키움 API 연결 중... ({stocksNeedingMinuteData.Count}개 분봉 수집 예정)");
                    UpdateProgress(55, "키움 API 연결 중...");

                    bool connected = await _kiwoomApiManager.ConnectAsync();
                    if (!connected)
                    {
                        throw new Exception("키움증권 연결에 실패했습니다.");
                    }
                }

                // 분봉 데이터 수집
                AddLog("🚀 1분봉 데이터 수집 시작 (3일치)");
                int minuteSuccessCount = 0;
                int minuteFailCount = 0;

                for (int i = 0; i < stocksNeedingMinuteData.Count; i++)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    string stockCode = stocksNeedingMinuteData[i];

                    try
                    {
                        AddLog($"📈 [{i + 1}/{stocksNeedingMinuteData.Count}] {stockCode} 1분봉 3일 데이터 수집 중...");

                        int progressBase = 60 + (i * 30 / stocksNeedingMinuteData.Count);
                        UpdateProgress(progressBase, $"분봉 수집 중... ({i + 1}/{stocksNeedingMinuteData.Count}) {stockCode}");

                        // 🆕 1분봉 3일치 수집
                        var minuteData = await _kiwoomApiManager.GetMinuteHistoricalDataAsync(stockCode, 1, 3);

                        if (cancellationToken.IsCancellationRequested) break;

                        if (minuteData != null && minuteData.Count > 0)
                        {
                            // 🆕 분봉 데이터 저장
                            int savedCount = await _databaseManager.SaveMinuteHistoricalDataAsync(stockCode, 1, minuteData);

                            if (savedCount > 0)
                            {
                                AddLog($"  ✅ {stockCode}: 1분봉 {savedCount}개 저장 완료");
                                minuteSuccessCount++;

                                // 첫 번째 종목의 데이터 샘플만 출력
                                if (i == 0)
                                {
                                    AddLog($"  📊 1분봉 샘플 (최근 3개):");
                                    for (int j = 0; j < Math.Min(3, minuteData.Count); j++)
                                    {
                                        var data = minuteData[j];
                                        AddLog($"    {data.DateTime:yyyy-MM-dd HH:mm}: 종가 {data.Close}, 거래량 {data.Volume:N0}");
                                    }
                                }
                            }
                            else
                            {
                                AddLog($"  ❌ {stockCode}: 1분봉 저장 실패");
                                minuteFailCount++;
                            }
                        }
                        else
                        {
                            AddLog($"  ❌ {stockCode}: 1분봉 수집 실패");
                            minuteFailCount++;
                        }

                        // API 안정성을 위한 대기
                        if (i < stocksNeedingMinuteData.Count - 1)
                        {
                            AddLog($"  ⏳ API 안정성을 위해 2초 대기...");
                            await Task.Delay(2000, cancellationToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        AddLog($"  ❌ {stockCode}: 1분봉 수집 실패 - {ex.Message}");
                        minuteFailCount++;
                        continue;
                    }
                }

                AddLog($"✅ 1분봉 수집 완료: 성공 {minuteSuccessCount}개, 실패 {minuteFailCount}개");
                UpdateProgress(90, "분봉 수집 완료");

            }
            catch (Exception ex)
            {
                throw new Exception($"분봉 데이터 수집 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 최종 결과 표시
        /// </summary>
        private async Task ShowFinalResults(List<string> allStockCodes)
        {
            try
            {
                UpdateProgress(95, "결과 정리 중...");

                // 키움 API 연결 해제
                _kiwoomApiManager?.Disconnect();

                // 최종 통계 계산 (간단히)
                int totalDaily = 0;
                int totalMinute = 0;

                for (int i = 0; i < Math.Min(5, allStockCodes.Count); i++) // 처음 5개 종목만 체크
                {
                    string stockCode = allStockCodes[i];

                    // 일봉 개수 체크
                    bool hasDailyData = await _databaseManager.CheckHistoricalDataExistsAsync(stockCode, 60);
                    if (hasDailyData) totalDaily++;

                    // 분봉 개수 체크  
                    bool hasMinuteData = await _databaseManager.CheckMinuteHistoricalDataExistsAsync(stockCode, 1, 3);
                    if (hasMinuteData) totalMinute++;
                }

                UpdateProgress(100, "모든 데이터 수집 완료");

                AddLog("=== 📊 최종 수집 결과 ===");
                AddLog($"📋 총 대상 종목: {allStockCodes.Count}개");
                AddLog($"📈 일봉 데이터: 수집 완료 (60일치)");
                AddLog($"📊 1분봉 데이터: 수집 완료 (3일치, 약 {allStockCodes.Count * 1170}개 예상)");
                AddLog($"🔌 데이터 소스: 키움증권 실시간 API");
                AddLog($"💾 저장 위치: StockTrader3.db");

                string resultMessage = $"일봉 + 1분봉 데이터 수집이 완료되었습니다!\n\n" +
                                     $"📊 수집 결과:\n" +
                                     $"• 총 조건검색 종목: {allStockCodes.Count}개\n" +
                                     $"• 일봉 데이터: 60일치 수집 완료\n" +
                                     $"• 1분봉 데이터: 3일치 수집 완료\n" +
                                     $"• 예상 총 데이터: 약 {allStockCodes.Count * (60 + 1170):N0}개\n\n" +
                                     $"🎯 이제 StockTrader3에서 정교한 분석이 가능합니다!\n" +
                                     $"• 일봉: 기본 기술적 분석\n" +
                                     $"• 1분봉: 정밀한 매수/매도 타이밍\n" +
                                     $"• 분봉 변환: 1분→3분→5분→15분 가능";

                MessageBox.Show(resultMessage, "수집 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (Exception ex)
            {
                AddLog($"❌ 결과 정리 중 오류: {ex.Message}");
            }
        }

        #endregion

        #region UI 업데이트

        private void AddLog(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(AddLog), message);
                return;
            }

            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            txtLog.AppendText($"[{timestamp}] {message}\r\n");
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
        }

        private void UpdateProgress(int percentage, string status)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<int, string>(UpdateProgress), percentage, status);
                return;
            }

            progressBar1.Value = Math.Min(percentage, 100);
            lblProgress.Text = $"진행률: {percentage}% - {status}";
        }

        #endregion
    }
}