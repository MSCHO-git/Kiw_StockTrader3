using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AxKHOpenAPILib;

namespace DataCollector.Managers
{
    /// <summary>
    /// DataCollector 전용 간단한 키움 API 매니저 (일봉 + 분봉 수집 특화)
    /// </summary>
    public class SimpleKiwoomManager : IDisposable
    {
        #region 이벤트 정의

        public event Action<string> StatusUpdated;
        public event Action<bool> ConnectionChanged;

        #endregion

        #region 필드 및 생성자

        private AxKHOpenAPI _kiwoomApi;
        private bool _isConnected = false;
        private bool _disposed = false;

        // TR 결과 대기용 (일봉)
        private List<DataCollector.Models.DailyPrice> _receivedDailyData;
        private ManualResetEventSlim _dailyTrCompleteEvent;
        private bool _dailyTrInProgress = false;

        // TR 결과 대기용 (분봉) 🆕
        private List<DataCollector.Models.MinutePrice> _receivedMinuteData;
        private ManualResetEventSlim _minuteTrCompleteEvent;
        private bool _minuteTrInProgress = false;
        private int _currentMinuteInterval = 1; // 현재 처리 중인 분봉 간격

        public SimpleKiwoomManager(Form parentForm)
        {
            try
            {
                CreateKiwoomControl(parentForm);
                SetupEventHandlers();
                System.Diagnostics.Debug.WriteLine("✅ SimpleKiwoomManager 초기화 완료 (일봉 + 분봉)");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ SimpleKiwoomManager 초기화 실패: {ex.Message}");
                throw new Exception($"SimpleKiwoomManager 초기화 실패: {ex.Message}");
            }
        }

        #endregion

        #region 공개 속성

        public bool IsConnected => _isConnected;

        #endregion

        #region 키움 컨트롤 생성

        private void CreateKiwoomControl(Form parentForm)
        {
            try
            {
                _kiwoomApi = new AxKHOpenAPI();
                ((System.ComponentModel.ISupportInitialize)(_kiwoomApi)).BeginInit();

                _kiwoomApi.Enabled = true;
                _kiwoomApi.Location = new System.Drawing.Point(0, 0);
                _kiwoomApi.Name = "axKHOpenAPI_DataCollector";
                _kiwoomApi.OcxState = null;
                _kiwoomApi.Size = new System.Drawing.Size(100, 50);
                _kiwoomApi.TabIndex = 0;
                _kiwoomApi.Visible = false; // 숨김 처리

                parentForm.Controls.Add(_kiwoomApi);
                ((System.ComponentModel.ISupportInitialize)(_kiwoomApi)).EndInit();

                System.Diagnostics.Debug.WriteLine("✅ 키움 ActiveX 컨트롤 생성 완료");
            }
            catch (Exception ex)
            {
                throw new Exception("키움 API 컨트롤 생성 실패: " + ex.Message);
            }
        }

        #endregion

        #region 이벤트 핸들러 설정

        private void SetupEventHandlers()
        {
            try
            {
                _kiwoomApi.OnEventConnect += KiwoomApi_OnEventConnect;
                _kiwoomApi.OnReceiveTrData += KiwoomApi_OnReceiveTrData;

                System.Diagnostics.Debug.WriteLine("✅ 키움 API 이벤트 핸들러 설정 완료");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ 키움 API 이벤트 핸들러 설정 실패: {ex.Message}");
            }
        }

        #endregion

        #region 키움 API 연결

        /// <summary>
        /// 키움 API 연결
        /// </summary>
        public async Task<bool> ConnectAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== SimpleKiwoom 연결 시작 ===");
                StatusUpdated?.Invoke("키움증권 서버 연결 중...");

                // 이미 연결되어 있으면 성공 반환
                if (_kiwoomApi.GetConnectState() == 1)
                {
                    _isConnected = true;
                    StatusUpdated?.Invoke("이미 연결되어 있음");
                    System.Diagnostics.Debug.WriteLine("✅ 이미 키움에 연결되어 있음");
                    return true;
                }

                int result = _kiwoomApi.CommConnect();
                System.Diagnostics.Debug.WriteLine($"CommConnect 결과: {result}");

                if (result == 0)
                {
                    StatusUpdated?.Invoke("연결 요청 전송 완료 - 응답 대기 중...");

                    // 연결 완료까지 최대 30초 대기
                    int waitCount = 0;
                    while (!_isConnected && waitCount < 30)
                    {
                        await Task.Delay(1000);
                        waitCount++;
                        StatusUpdated?.Invoke($"연결 대기 중... ({waitCount}/30초)");

                        // 사용자가 로그인을 완료했는지 확인
                        if (_kiwoomApi.GetConnectState() == 1)
                        {
                            _isConnected = true;
                            break;
                        }
                    }

                    if (_isConnected)
                    {
                        StatusUpdated?.Invoke("키움증권 연결 완료");
                        System.Diagnostics.Debug.WriteLine("✅ SimpleKiwoom 연결 성공");
                        return true;
                    }
                    else
                    {
                        StatusUpdated?.Invoke("연결 타임아웃 - 키움증권 로그인을 확인해주세요");
                        System.Diagnostics.Debug.WriteLine("❌ SimpleKiwoom 연결 타임아웃");
                        return false;
                    }
                }
                else
                {
                    StatusUpdated?.Invoke($"연결 요청 실패: {result}");
                    System.Diagnostics.Debug.WriteLine($"❌ SimpleKiwoom 연결 요청 실패: {result}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                StatusUpdated?.Invoke($"연결 중 오류: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ SimpleKiwoom ConnectAsync 예외: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 키움 API 연결 해제
        /// </summary>
        public void Disconnect()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== SimpleKiwoom 연결 해제 ===");

                if (_isConnected)
                {
                    _kiwoomApi.CommTerminate();
                    _isConnected = false;
                    StatusUpdated?.Invoke("키움증권 연결 해제됨");
                    System.Diagnostics.Debug.WriteLine("✅ SimpleKiwoom 연결 해제 완료");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ SimpleKiwoom 연결 해제 실패: {ex.Message}");
            }
        }

        #endregion

        #region 일봉 데이터 수집 (OPT10081) - 기존

        /// <summary>
        /// 일봉 데이터 수집 (OPT10081 - 주식일봉차트조회)
        /// </summary>
        public async Task<List<DataCollector.Models.DailyPrice>> GetHistoricalDataAsync(string stockCode, int days = 60)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== {stockCode} 일봉 {days}일 데이터 수집 시작 ===");

                if (!_isConnected)
                {
                    throw new Exception("키움 API가 연결되지 않았습니다");
                }

                if (_dailyTrInProgress || _minuteTrInProgress)
                {
                    throw new Exception("이미 TR이 진행 중입니다");
                }

                _receivedDailyData = new List<DataCollector.Models.DailyPrice>();
                _dailyTrCompleteEvent = new ManualResetEventSlim(false);
                _dailyTrInProgress = true;

                StatusUpdated?.Invoke($"{stockCode} 일봉 데이터 요청 중...");

                // OPT10081 TR 요청 설정
                _kiwoomApi.SetInputValue("종목코드", stockCode);
                _kiwoomApi.SetInputValue("기준일자", DateTime.Now.ToString("yyyyMMdd"));
                _kiwoomApi.SetInputValue("수정주가구분", "1"); // 1: 수정주가

                // TR 요청 전송
                string requestName = "주식일봉차트조회";
                string trCode = "OPT10081";
                string screenNo = "0101";

                System.Diagnostics.Debug.WriteLine($"일봉 TR 요청: {trCode}, 종목: {stockCode}");

                int result = _kiwoomApi.CommRqData(requestName, trCode, 0, screenNo);

                if (result != 0)
                {
                    _dailyTrInProgress = false;
                    throw new Exception($"일봉 TR 요청 실패: {GetErrorMessage(result)}");
                }

                StatusUpdated?.Invoke($"{stockCode} 일봉 데이터 수신 대기 중...");

                // TR 응답 대기 (최대 30초)
                bool completed = await Task.Run(() => _dailyTrCompleteEvent.Wait(30000));

                _dailyTrInProgress = false;

                if (!completed)
                {
                    System.Diagnostics.Debug.WriteLine("❌ 일봉 TR 응답 타임아웃");
                    throw new Exception("일봉 데이터 수신 타임아웃");
                }

                if (_receivedDailyData.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ {stockCode} 일봉 데이터 {_receivedDailyData.Count}개 수집 완료");
                    StatusUpdated?.Invoke($"{stockCode} 일봉 데이터 {_receivedDailyData.Count}개 수집 완료");

                    // 요청한 일수만큼만 반환 (최신 데이터부터)
                    var result_data = _receivedDailyData.Count > days ?
                        _receivedDailyData.GetRange(0, days) : _receivedDailyData;

                    return result_data;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"❌ {stockCode} 일봉 데이터 수신 실패");
                    throw new Exception("일봉 데이터를 수신하지 못했습니다");
                }
            }
            catch (Exception ex)
            {
                _dailyTrInProgress = false;
                System.Diagnostics.Debug.WriteLine($"❌ GetHistoricalDataAsync 실패: {ex.Message}");
                StatusUpdated?.Invoke($"일봉 데이터 수집 실패: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region 분봉 데이터 수집 (OPT10080) - 🆕 새로 추가

        /// <summary>
        /// 분봉 데이터 수집 (OPT10080 - 주식분봉차트조회)
        /// </summary>
        public async Task<List<DataCollector.Models.MinutePrice>> GetMinuteHistoricalDataAsync(string stockCode, int minuteInterval = 1, int days = 3)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== {stockCode} {minuteInterval}분봉 {days}일 데이터 수집 시작 ===");

                if (!_isConnected)
                {
                    throw new Exception("키움 API가 연결되지 않았습니다");
                }

                if (_dailyTrInProgress || _minuteTrInProgress)
                {
                    throw new Exception("이미 TR이 진행 중입니다");
                }

                _receivedMinuteData = new List<DataCollector.Models.MinutePrice>();
                _minuteTrCompleteEvent = new ManualResetEventSlim(false);
                _minuteTrInProgress = true;
                _currentMinuteInterval = minuteInterval;

                StatusUpdated?.Invoke($"{stockCode} {minuteInterval}분봉 데이터 요청 중...");

                // OPT10080 TR 요청 설정
                _kiwoomApi.SetInputValue("종목코드", stockCode);
                _kiwoomApi.SetInputValue("틱범위", minuteInterval.ToString());  // 분봉 간격
                _kiwoomApi.SetInputValue("수정주가구분", "1"); // 1: 수정주가

                // TR 요청 전송
                string requestName = "주식분봉차트조회";
                string trCode = "OPT10080";
                string screenNo = "0102"; // 일봉과 다른 화면번호 사용

                System.Diagnostics.Debug.WriteLine($"분봉 TR 요청: {trCode}, 종목: {stockCode}, 간격: {minuteInterval}분");

                int result = _kiwoomApi.CommRqData(requestName, trCode, 0, screenNo);

                if (result != 0)
                {
                    _minuteTrInProgress = false;
                    throw new Exception($"분봉 TR 요청 실패: {GetErrorMessage(result)}");
                }

                StatusUpdated?.Invoke($"{stockCode} {minuteInterval}분봉 데이터 수신 대기 중...");

                // TR 응답 대기 (최대 30초)
                bool completed = await Task.Run(() => _minuteTrCompleteEvent.Wait(30000));

                _minuteTrInProgress = false;

                if (!completed)
                {
                    System.Diagnostics.Debug.WriteLine("❌ 분봉 TR 응답 타임아웃");
                    throw new Exception("분봉 데이터 수신 타임아웃");
                }

                if (_receivedMinuteData.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ {stockCode} {minuteInterval}분봉 데이터 {_receivedMinuteData.Count}개 수집 완료");
                    StatusUpdated?.Invoke($"{stockCode} {minuteInterval}분봉 데이터 {_receivedMinuteData.Count}개 수집 완료");

                    // 요청한 일수에 해당하는 데이터만 반환
                    int minutesPerDay = (int)(6.5 * 60 / minuteInterval); // 하루 분봉 개수
                    int maxCount = days * minutesPerDay;

                    var result_data = _receivedMinuteData.Count > maxCount ?
                        _receivedMinuteData.GetRange(0, maxCount) : _receivedMinuteData;

                    return result_data;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"❌ {stockCode} {minuteInterval}분봉 데이터 수신 실패");
                    throw new Exception("분봉 데이터를 수신하지 못했습니다");
                }
            }
            catch (Exception ex)
            {
                _minuteTrInProgress = false;
                System.Diagnostics.Debug.WriteLine($"❌ GetMinuteHistoricalDataAsync 실패: {ex.Message}");
                StatusUpdated?.Invoke($"분봉 데이터 수집 실패: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region 키움 API 이벤트 핸들러

        private void KiwoomApi_OnEventConnect(object sender, _DKHOpenAPIEvents_OnEventConnectEvent e)
        {
            System.Diagnostics.Debug.WriteLine($"=== OnEventConnect: {e.nErrCode} ===");

            if (e.nErrCode == 0)
            {
                _isConnected = true;
                ConnectionChanged?.Invoke(true);
                StatusUpdated?.Invoke("키움증권 연결 완료");
                System.Diagnostics.Debug.WriteLine("✅ SimpleKiwoom 연결 성공");
            }
            else
            {
                _isConnected = false;
                ConnectionChanged?.Invoke(false);
                string errorMsg = GetConnectionErrorMessage(e.nErrCode);
                StatusUpdated?.Invoke($"연결 실패: {errorMsg}");
                System.Diagnostics.Debug.WriteLine($"❌ SimpleKiwoom 연결 실패: {errorMsg}");
            }
        }

        private void KiwoomApi_OnReceiveTrData(object sender, _DKHOpenAPIEvents_OnReceiveTrDataEvent e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== OnReceiveTrData: {e.sTrCode} ===");

                if (e.sTrCode == "OPT10081" && _dailyTrInProgress)
                {
                    System.Diagnostics.Debug.WriteLine("일봉 OPT10081 응답 처리 시작");
                    ProcessOPT10081Data(e);
                    _dailyTrCompleteEvent?.Set(); // 일봉 TR 완료 신호
                }
                else if (e.sTrCode == "OPT10080" && _minuteTrInProgress)
                {
                    System.Diagnostics.Debug.WriteLine("분봉 OPT10080 응답 처리 시작");
                    ProcessOPT10080Data(e); // 🆕 분봉 데이터 처리
                    _minuteTrCompleteEvent?.Set(); // 분봉 TR 완료 신호
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ OnReceiveTrData 처리 실패: {ex.Message}");

                // 에러 발생 시에도 대기 해제
                if (_dailyTrInProgress)
                    _dailyTrCompleteEvent?.Set();
                if (_minuteTrInProgress)
                    _minuteTrCompleteEvent?.Set();
            }
        }

        #endregion

        #region OPT10081 데이터 처리 (일봉) - 기존

        private void ProcessOPT10081Data(dynamic e)
        {
            try
            {
                string trCode = e.sTrCode;
                string requestName = e.sRQName;

                // 수신 데이터 개수 확인
                int dataCount = _kiwoomApi.GetRepeatCnt(trCode, requestName);
                System.Diagnostics.Debug.WriteLine($"일봉 수신된 데이터 개수: {dataCount}");

                StatusUpdated?.Invoke($"일봉 데이터 파싱 중... ({dataCount}개)");

                for (int i = 0; i < dataCount; i++)
                {
                    try
                    {
                        // 각 항목의 데이터 추출
                        string dateStr = _kiwoomApi.GetCommData(trCode, requestName, i, "일자").Trim();
                        string openStr = _kiwoomApi.GetCommData(trCode, requestName, i, "시가").Trim();
                        string highStr = _kiwoomApi.GetCommData(trCode, requestName, i, "고가").Trim();
                        string lowStr = _kiwoomApi.GetCommData(trCode, requestName, i, "저가").Trim();
                        string closeStr = _kiwoomApi.GetCommData(trCode, requestName, i, "현재가").Trim();
                        string volumeStr = _kiwoomApi.GetCommData(trCode, requestName, i, "거래량").Trim();

                        // 데이터 변환 및 검증
                        if (DateTime.TryParseExact(dateStr, "yyyyMMdd", null,
                            System.Globalization.DateTimeStyles.None, out DateTime date) &&
                            decimal.TryParse(openStr, out decimal open) &&
                            decimal.TryParse(highStr, out decimal high) &&
                            decimal.TryParse(lowStr, out decimal low) &&
                            decimal.TryParse(closeStr, out decimal close) &&
                            long.TryParse(volumeStr, out long volume))
                        {
                            // 음수 값을 양수로 변환 (키움에서 음수로 올 수 있음)
                            open = Math.Abs(open);
                            high = Math.Abs(high);
                            low = Math.Abs(low);
                            close = Math.Abs(close);
                            volume = Math.Abs(volume);

                            var dailyPrice = new DataCollector.Models.DailyPrice
                            {
                                Date = date,
                                Open = open,
                                High = high,
                                Low = low,
                                Close = close,
                                Volume = volume
                            };

                            _receivedDailyData.Add(dailyPrice);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"  ⚠️ 일봉 데이터 파싱 실패: {dateStr}");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"  ❌ 일봉 개별 데이터 처리 실패 [{i}]: {ex.Message}");
                    }
                }

                System.Diagnostics.Debug.WriteLine($"✅ 일봉 총 {_receivedDailyData.Count}개 데이터 파싱 완료");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ProcessOPT10081Data 실패: {ex.Message}");
            }
        }

        #endregion

        #region OPT10080 데이터 처리 (분봉) - 🆕 새로 추가

        private void ProcessOPT10080Data(dynamic e)
        {
            try
            {
                string trCode = e.sTrCode;
                string requestName = e.sRQName;

                // 수신 데이터 개수 확인
                int dataCount = _kiwoomApi.GetRepeatCnt(trCode, requestName);
                System.Diagnostics.Debug.WriteLine($"분봉 수신된 데이터 개수: {dataCount}");

                StatusUpdated?.Invoke($"분봉 데이터 파싱 중... ({dataCount}개)");

                for (int i = 0; i < dataCount; i++)
                {
                    try
                    {
                        // 분봉 데이터 추출
                        string dateStr = _kiwoomApi.GetCommData(trCode, requestName, i, "체결시간").Trim();
                        string openStr = _kiwoomApi.GetCommData(trCode, requestName, i, "시가").Trim();
                        string highStr = _kiwoomApi.GetCommData(trCode, requestName, i, "고가").Trim();
                        string lowStr = _kiwoomApi.GetCommData(trCode, requestName, i, "저가").Trim();
                        string closeStr = _kiwoomApi.GetCommData(trCode, requestName, i, "현재가").Trim();
                        string volumeStr = _kiwoomApi.GetCommData(trCode, requestName, i, "거래량").Trim();

                        System.Diagnostics.Debug.WriteLine($"  {i + 1}. 시간:{dateStr}, 종가:{closeStr}, 거래량:{volumeStr}");

                        // 시간 파싱 (yyyyMMddHHmmss 형식)
                        if (dateStr.Length >= 14 &&
                            DateTime.TryParseExact(dateStr, "yyyyMMddHHmmss", null,
                            System.Globalization.DateTimeStyles.None, out DateTime dateTime) &&
                            decimal.TryParse(openStr, out decimal open) &&
                            decimal.TryParse(highStr, out decimal high) &&
                            decimal.TryParse(lowStr, out decimal low) &&
                            decimal.TryParse(closeStr, out decimal close) &&
                            long.TryParse(volumeStr, out long volume))
                        {
                            // 음수 값을 양수로 변환
                            open = Math.Abs(open);
                            high = Math.Abs(high);
                            low = Math.Abs(low);
                            close = Math.Abs(close);
                            volume = Math.Abs(volume);

                            var minutePrice = new DataCollector.Models.MinutePrice
                            {
                                DateTime = dateTime,
                                MinuteInterval = _currentMinuteInterval,
                                Open = open,
                                High = high,
                                Low = low,
                                Close = close,
                                Volume = volume,
                                ChangeAmount = 0, // 일단 0으로 설정 (필요시 계산)
                                ChangeRate = 0,   // 일단 0으로 설정 (필요시 계산)
                                TradingValue = (long)(close * volume) // 거래대금 계산
                            };

                            _receivedMinuteData.Add(minutePrice);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"  ⚠️ 분봉 데이터 파싱 실패: {dateStr}");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"  ❌ 분봉 개별 데이터 처리 실패 [{i}]: {ex.Message}");
                    }
                }

                System.Diagnostics.Debug.WriteLine($"✅ 분봉 총 {_receivedMinuteData.Count}개 데이터 파싱 완료");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ProcessOPT10080Data 실패: {ex.Message}");
            }
        }

        #endregion

        #region 에러 메시지 처리

        private string GetConnectionErrorMessage(int errorCode)
        {
            switch (errorCode)
            {
                case -100: return "사용자 정보교환 실패";
                case -101: return "서버접속 실패";
                case -102: return "버전처리 실패";
                default: return $"알 수 없는 오류 (코드: {errorCode})";
            }
        }

        private string GetErrorMessage(int errorCode)
        {
            switch (errorCode)
            {
                case -200: return "시세 과부하";
                case -201: return "REQUEST_INPUT_st Failed";
                case 0: return "정상처리";
                default: return $"TR 오류 (코드: {errorCode})";
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
                    System.Diagnostics.Debug.WriteLine("=== SimpleKiwoomManager 리소스 정리 ===");

                    Disconnect();
                    _dailyTrCompleteEvent?.Dispose();
                    _minuteTrCompleteEvent?.Dispose(); // 🆕 분봉 이벤트 정리

                    if (_kiwoomApi != null)
                    {
                        _kiwoomApi.Dispose();
                        _kiwoomApi = null;
                    }

                    System.Diagnostics.Debug.WriteLine("✅ SimpleKiwoomManager 리소스 정리 완료");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ SimpleKiwoomManager 리소스 정리 실패: {ex.Message}");
                }
                _disposed = true;
            }
        }

        #endregion
    }
}