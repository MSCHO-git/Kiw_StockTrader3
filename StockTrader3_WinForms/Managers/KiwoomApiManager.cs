using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AxKHOpenAPILib;
using StockTrader3_WinForms.Modules;


namespace StockTrader3_WinForms
{
    #region 데이터 모델 클래스들

    /// <summary>
    /// 계좌 정보 모델
    /// </summary>
    public class AccountInfo
    {
        public string AccountNumber { get; set; }
        public long Balance { get; set; }
        public long TotalValue { get; set; }
        public long TotalProfit { get; set; }
    }

    /// <summary>
    /// 조건검색 결과 모델
    /// </summary>
    public class ConditionSearchResult
    {
        public string ConditionName { get; set; }
        public int ConditionIndex { get; set; }
        public string[] StockCodes { get; set; }
        public int Count { get; set; }
        public DateTime CompletedAt { get; set; }
        public TimeSpan ElapsedTime { get; set; }
    }

    #endregion

    /// <summary>
    /// 키움 API 완전 연동 및 모든 키움 관련 기능 담당 (일반 조건검색 방식으로 수정)
    /// </summary>
    public class KiwoomApiManager
    {
        #region 이벤트 정의

        public event Action<bool, string> ConnectionStatusChanged;
        public event Action<AccountInfo> AccountInfoReceived;
        public event Action<int> ConditionListLoaded;
        public event Action<ConditionSearchResult> ConditionSearchResultReceived;
        public event Action<string, int, int, string, TimeSpan> ProgressUpdated;
        public event Action<string> StatusMessageUpdated;
        public event Action<string[], string> AccountListReceived;  // 🆕 추가됨

        #endregion

        #region 필드 및 생성자

        private readonly MainForm _mainForm;
        private readonly AxKHOpenAPI _kiwoomApi;
        private bool _isConnected = false;
        private string _currentAccount = "";

        // 조건검색 관련 변수
        private List<string> _conditionStockCodes = new List<string>();
        private System.Windows.Forms.Timer _conditionCompleteTimer;
        private int _currentConditionIndex = -1;
        private string _currentConditionName = "";
        private DateTime _conditionSearchStartTime;

        public KiwoomApiManager(MainForm mainForm, AxKHOpenAPI kiwoomApi)
        {
            _mainForm = mainForm ?? throw new ArgumentNullException(nameof(mainForm));
            _kiwoomApi = kiwoomApi ?? throw new ArgumentNullException(nameof(kiwoomApi));
            SetupEventHandlers();
        }

        #endregion

        #region 공개 속성

        public bool IsConnected => _isConnected;
        public string CurrentAccount => _currentAccount;

        #endregion

        #region 이벤트 핸들러 설정

        private void SetupEventHandlers()
        {
            try
            {
                _kiwoomApi.OnEventConnect += KiwoomApi_OnEventConnect;
                _kiwoomApi.OnReceiveConditionVer += KiwoomApi_OnReceiveConditionVer;
                _kiwoomApi.OnReceiveTrData += KiwoomApi_OnReceiveTrData;
                _kiwoomApi.OnReceiveRealCondition += KiwoomApi_OnReceiveRealCondition; // 실시간용 (보조)
                _kiwoomApi.OnReceiveTrCondition += KiwoomApi_OnReceiveTrCondition;     // 일반 조건검색용 (메인)

                System.Diagnostics.Debug.WriteLine("✅ 키움 API 이벤트 핸들러 설정 완료 (일반 조건검색 포함)");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ 키움 API 이벤트 핸들러 설정 실패: {ex.Message}");
            }
        }

        #endregion

        #region 키움 API 연결

        public bool Connect()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== 키움 API 연결 시도 ===");
                System.Diagnostics.Debug.WriteLine($"현재 시간: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

                StatusMessageUpdated?.Invoke("키움증권 서버 연결 시도 중...");
                ProgressUpdated?.Invoke("키움 연결", 0, 1, "서버 연결 시도 중...", TimeSpan.FromSeconds(10));

                int result = _kiwoomApi.CommConnect();
                System.Diagnostics.Debug.WriteLine($"CommConnect 결과: {result}");

                if (result == 0)
                {
                    StatusMessageUpdated?.Invoke("연결 요청 전송 완료 - 응답 대기 중...");
                    System.Diagnostics.Debug.WriteLine("✅ 연결 요청 전송 성공 - 응답 대기 중");
                    return true;
                }
                else
                {
                    StatusMessageUpdated?.Invoke("연결 요청 실패");
                    System.Diagnostics.Debug.WriteLine($"❌ 연결 요청 실패: {result}");
                    MessageBox.Show($"키움 API 연결 요청에 실패했습니다. 결과코드: {result}", "연결 실패");
                    return false;
                }
            }
            catch (Exception ex)
            {
                StatusMessageUpdated?.Invoke("연결 중 오류 발생");
                System.Diagnostics.Debug.WriteLine($"❌ Connect 예외 발생: {ex.Message}");
                MessageBox.Show("키움 API 연결 중 오류가 발생했습니다:\n" + ex.Message, "오류");
                return false;
            }
        }

        public void Disconnect()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== 키움 API 연결 해제 ===");

                if (_isConnected)
                {
                    _kiwoomApi.CommTerminate();
                    _isConnected = false;
                    System.Diagnostics.Debug.WriteLine("✅ 키움 API 연결 해제 완료");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ 키움 API 해제 실패: {ex.Message}");
            }
        }

        #endregion

        #region 조건검색 관리

        public bool RefreshConditionList()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== 조건검색식 새로고침 ===");

                StatusMessageUpdated?.Invoke("조건검색 목록 새로고침 중...");
                int result = _kiwoomApi.GetConditionLoad();

                System.Diagnostics.Debug.WriteLine($"GetConditionLoad 결과: {result}");

                if (result == 1)
                {
                    StatusMessageUpdated?.Invoke("조건검색식 새로고침 요청 완료");
                    System.Diagnostics.Debug.WriteLine("✅ 조건검색식 새로고침 요청 성공");
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"❌ 조건검색식 새로고침 실패: {result}");
                    MessageBox.Show("조건검색식 새로고침 실패", "오류");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ RefreshConditionList 예외 발생: {ex.Message}");
                MessageBox.Show("새로고침 중 오류: " + ex.Message, "오류");
                return false;
            }
        }

        public string[] GetConditionList()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== 조건검색식 목록 조회 ===");

                string strCondition = _kiwoomApi.GetConditionNameList();
                System.Diagnostics.Debug.WriteLine($"GetConditionNameList 결과: '{strCondition}'");

                if (string.IsNullOrEmpty(strCondition))
                {
                    System.Diagnostics.Debug.WriteLine("❌ 조건검색식 목록이 비어있음");
                    return new string[0];
                }

                if (strCondition.EndsWith(";"))
                {
                    strCondition = strCondition.Remove(strCondition.Length - 1);
                    System.Diagnostics.Debug.WriteLine($"세미콜론 제거 후: '{strCondition}'");
                }

                string[] result = strCondition.Split(';');
                System.Diagnostics.Debug.WriteLine($"✅ 조건검색식 {result.Length}개 반환");

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ GetConditionList 실패: {ex.Message}");
                return new string[0];
            }
        }

        #endregion

        #region 조건검색 실행 (일반 조건검색 방식으로 수정)

        public void ExecuteConditionSearch(int conditionIndex, string conditionName)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== 조건검색 실행 전 상태 체크 ===");
                System.Diagnostics.Debug.WriteLine($"키움 연결 상태: {_kiwoomApi.GetConnectState()}");
                System.Diagnostics.Debug.WriteLine($"조건검색 인덱스: {conditionIndex}");
                System.Diagnostics.Debug.WriteLine($"조건검색 이름: {conditionName}");
                System.Diagnostics.Debug.WriteLine($"현재 실행 중인 조건: {_currentConditionIndex}");

                if (_currentConditionIndex >= 0)
                {
                    StatusMessageUpdated?.Invoke("이전 조건검색을 중단하고 새로 시작합니다.");
                    System.Diagnostics.Debug.WriteLine("⚠️ 이전 조건검색 중단 처리");
                    StopConditionSearch();
                }

                InitializeConditionSearch(conditionIndex, conditionName);

                StatusMessageUpdated?.Invoke($"조건검색 실행 중: {conditionName}");
                ProgressUpdated?.Invoke("조건검색", 0, 100, "조건검색 요청 중...", TimeSpan.FromSeconds(30));

                string screenNo = "0150";
                int isRealTime = 0; // ✅ 일반 조건검색으로 변경 (HTS와 동일)

                System.Diagnostics.Debug.WriteLine("=== SendCondition 호출 (일반 조건검색) ===");
                System.Diagnostics.Debug.WriteLine($"screenNo: {screenNo}");
                System.Diagnostics.Debug.WriteLine($"conditionName: {conditionName}");
                System.Diagnostics.Debug.WriteLine($"conditionIndex: {conditionIndex}");
                System.Diagnostics.Debug.WriteLine($"isRealTime: {isRealTime} (일반 조건검색 - HTS와 동일)");

                int result = _kiwoomApi.SendCondition(screenNo, conditionName, conditionIndex, isRealTime);

                System.Diagnostics.Debug.WriteLine($"🔥 SendCondition 결과: {result}");

                if (result == 1)
                {
                    StatusMessageUpdated?.Invoke($"조건검색 요청 전송 완료: {conditionName}");
                    System.Diagnostics.Debug.WriteLine("✅ 일반 조건검색 요청 성공 - OnReceiveTrCondition 이벤트 대기 중");
                    StartConditionTimeoutTimer();
                }
                else
                {
                    string errorMsg = GetConditionSearchErrorMessage(result);
                    StatusMessageUpdated?.Invoke($"조건검색 요청 실패: {errorMsg}");
                    System.Diagnostics.Debug.WriteLine($"❌ 조건검색 요청 실패: {errorMsg} (코드: {result})");
                    ResetConditionSearch();
                    throw new Exception($"조건검색 요청 실패: {errorMsg}");
                }
            }
            catch (Exception ex)
            {
                StatusMessageUpdated?.Invoke("조건검색 실행 실패");
                System.Diagnostics.Debug.WriteLine($"❌ ExecuteConditionSearch 예외 발생: {ex.Message}");
                ResetConditionSearch();
                MessageBox.Show($"조건검색 실행 실패: {ex.Message}", "오류");
            }
        }

        private void InitializeConditionSearch(int conditionIndex, string conditionName)
        {
            System.Diagnostics.Debug.WriteLine("=== 조건검색 초기화 ===");

            _currentConditionIndex = conditionIndex;
            _currentConditionName = conditionName;
            _conditionSearchStartTime = DateTime.Now;
            _conditionStockCodes.Clear();

            _conditionCompleteTimer?.Stop();
            _conditionCompleteTimer?.Dispose();
            _conditionCompleteTimer = null;

            System.Diagnostics.Debug.WriteLine($"조건 인덱스 설정: {_currentConditionIndex}");
            System.Diagnostics.Debug.WriteLine($"조건 이름 설정: {_currentConditionName}");
            System.Diagnostics.Debug.WriteLine($"검색 시작 시간: {_conditionSearchStartTime:HH:mm:ss}");
            System.Diagnostics.Debug.WriteLine("✅ 조건검색 초기화 완료");
        }

        private void StopConditionSearch()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== 조건검색 중단 ===");

                if (_currentConditionIndex >= 0)
                {
                    string screenNo = "0150";
                    System.Diagnostics.Debug.WriteLine($"SendConditionStop 호출: {_currentConditionName} (인덱스: {_currentConditionIndex})");
                    _kiwoomApi.SendConditionStop(screenNo, _currentConditionName, _currentConditionIndex);
                    System.Diagnostics.Debug.WriteLine("✅ SendConditionStop 호출 완료");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ StopConditionSearch 실패: {ex.Message}");
            }
            finally
            {
                ResetConditionSearch();
            }
        }

        private void ResetConditionSearch()
        {
            System.Diagnostics.Debug.WriteLine("=== 조건검색 상태 리셋 ===");

            _currentConditionIndex = -1;
            _currentConditionName = "";
            _conditionStockCodes.Clear();

            _conditionCompleteTimer?.Stop();
            _conditionCompleteTimer?.Dispose();
            _conditionCompleteTimer = null;

            System.Diagnostics.Debug.WriteLine("✅ 조건검색 상태 리셋 완료");
        }

        private void StartConditionTimeoutTimer()
        {
            System.Diagnostics.Debug.WriteLine("=== 조건검색 타임아웃 타이머 시작 (60초) ===");

            var timeoutTimer = new System.Windows.Forms.Timer();
            timeoutTimer.Interval = 60000; // ✅ 60초로 연장 (일반 조건검색은 더 오래 걸릴 수 있음)
            timeoutTimer.Tick += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine("⏰ 조건검색 타임아웃 발생 (60초 경과)");
                timeoutTimer.Stop();
                timeoutTimer.Dispose();

                if (_currentConditionIndex >= 0)
                {
                    if (_conditionStockCodes.Count > 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"타임아웃 - 부분 결과로 완료: {_conditionStockCodes.Count}개 종목");
                        CompleteConditionSearch("타임아웃 - 부분 결과");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("타임아웃 - 결과 없음");
                        CompleteConditionSearch("타임아웃 - 결과 없음");
                    }
                }
            };
            timeoutTimer.Start();
        }

        #endregion

        #region 조건검색 결과 처리 (일반 조건검색용 - 새로 추가)

        /// <summary>
        /// 일반 조건검색 결과 처리 (OnReceiveTrCondition) - HTS와 동일한 방식
        /// </summary>
        private void KiwoomApi_OnReceiveTrCondition(object sender, _DKHOpenAPIEvents_OnReceiveTrConditionEvent e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== OnReceiveTrCondition 이벤트 수신 (일반 조건검색) ===");
                System.Diagnostics.Debug.WriteLine($"화면번호: {e.sScrNo}");
                System.Diagnostics.Debug.WriteLine($"종목리스트: '{e.strCodeList}'");
                System.Diagnostics.Debug.WriteLine($"조건명: {e.strConditionName}");
                System.Diagnostics.Debug.WriteLine($"조건인덱스: {e.nIndex}");
                System.Diagnostics.Debug.WriteLine($"다음여부: {e.nNext}");

                if (e.strConditionName == _currentConditionName && e.nIndex == _currentConditionIndex)
                {
                    System.Diagnostics.Debug.WriteLine("✅ 조건명 및 인덱스 일치 - 결과 처리 진행");

                    // 종목 코드 리스트 파싱 (세미콜론으로 구분)
                    if (!string.IsNullOrEmpty(e.strCodeList))
                    {
                        // 세미콜론 제거 및 분할
                        string cleanCodeList = e.strCodeList.TrimEnd(';');
                        string[] stockCodes = cleanCodeList.Split(';');

                        System.Diagnostics.Debug.WriteLine($"원본 종목리스트: '{e.strCodeList}'");
                        System.Diagnostics.Debug.WriteLine($"정리된 종목리스트: '{cleanCodeList}'");
                        System.Diagnostics.Debug.WriteLine($"분할된 종목 수: {stockCodes.Length}");

                        // 빈 코드 제외하고 추가
                        var validCodes = stockCodes.Where(code => !string.IsNullOrEmpty(code.Trim())).ToList();
                        _conditionStockCodes.AddRange(validCodes);

                        System.Diagnostics.Debug.WriteLine($"✅ 유효한 종목 {validCodes.Count}개 수신:");
                        for (int i = 0; i < validCodes.Count; i++)
                        {
                            System.Diagnostics.Debug.WriteLine($"  {i + 1}. {validCodes[i]}");
                        }

                        // 추가 결과가 있는지 확인 (nNext)
                        if (e.nNext == 0)
                        {
                            System.Diagnostics.Debug.WriteLine("📋 모든 조건검색 결과 수신 완료 (nNext = 0)");
                            CompleteConditionSearch("일반 조건검색 완료");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("📋 추가 결과 대기 중 (nNext = 1)");
                            // 추가 결과가 있으면 대기 (자동으로 다음 이벤트가 발생함)
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("❌ 조건에 맞는 종목이 없음 (빈 결과)");
                        CompleteConditionSearch("조건에 맞는 종목 없음");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"❌ 조건 불일치:");
                    System.Diagnostics.Debug.WriteLine($"  수신 조건명: '{e.strConditionName}' vs 대기 조건명: '{_currentConditionName}'");
                    System.Diagnostics.Debug.WriteLine($"  수신 인덱스: {e.nIndex} vs 대기 인덱스: {_currentConditionIndex}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ OnReceiveTrCondition 처리 실패: {ex.Message}");
                CompleteConditionSearch("조건검색 처리 오류");
            }
        }

        /// <summary>
        /// 실시간 조건검색 결과 처리 (OnReceiveRealCondition) - 보조용으로 유지
        /// </summary>
        private void KiwoomApi_OnReceiveRealCondition(object sender, _DKHOpenAPIEvents_OnReceiveRealConditionEvent e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== OnReceiveRealCondition 이벤트 수신 (실시간 - 보조) ===");
                System.Diagnostics.Debug.WriteLine($"수신 종목코드: {e.sTrCode}");
                System.Diagnostics.Debug.WriteLine($"편입/편출 타입: {e.strType}");
                System.Diagnostics.Debug.WriteLine($"조건검색명: {e.strConditionName}");
                System.Diagnostics.Debug.WriteLine("⚠️ 현재 일반 조건검색 모드이므로 실시간 이벤트는 무시됩니다.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ OnReceiveRealCondition 처리 실패: {ex.Message}");
            }
        }

        private void CompleteConditionSearch(string reason)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== 조건검색 완료 처리 ===");
                System.Diagnostics.Debug.WriteLine($"완료 사유: {reason}");
                System.Diagnostics.Debug.WriteLine($"현재 조건 인덱스: {_currentConditionIndex}");
                System.Diagnostics.Debug.WriteLine($"수집된 종목 수: {_conditionStockCodes.Count}");

                if (_currentConditionIndex < 0)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ 이미 리셋된 상태 - 완료 처리 중단");
                    return;
                }

                if (_conditionStockCodes.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine("📝 최종 수집된 종목 목록:");
                    for (int i = 0; i < _conditionStockCodes.Count; i++)
                    {
                        System.Diagnostics.Debug.WriteLine($"  {i + 1}. {_conditionStockCodes[i]}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("❌ 수집된 종목이 없음");
                }

                var result = new ConditionSearchResult
                {
                    ConditionName = _currentConditionName,
                    ConditionIndex = _currentConditionIndex,
                    StockCodes = _conditionStockCodes.ToArray(),
                    Count = _conditionStockCodes.Count,
                    CompletedAt = DateTime.Now,
                    ElapsedTime = DateTime.Now - _conditionSearchStartTime
                };

                System.Diagnostics.Debug.WriteLine($"✅ 결과 객체 생성 완료: {result.Count}개 종목");

                ProgressUpdated?.Invoke("조건검색 완료", result.Count, result.Count,
                    $"{result.Count}개 종목 수신 완료", TimeSpan.Zero);

                StatusMessageUpdated?.Invoke($"조건검색 완료: {result.Count}개 종목");

                System.Diagnostics.Debug.WriteLine("🚀 ConditionSearchResultReceived 이벤트 발생");
                ConditionSearchResultReceived?.Invoke(result);

                ResetConditionSearch();
                System.Diagnostics.Debug.WriteLine("✅ 조건검색 상태 리셋 완료");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ CompleteConditionSearch 실패: {ex.Message}");
                ResetConditionSearch();
            }
        }

        #endregion

        #region 계좌 정보 관리

        public void RequestAccountBalance()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== 계좌 정보 요청 ===");

                if (string.IsNullOrEmpty(_currentAccount))
                {
                    System.Diagnostics.Debug.WriteLine("❌ 현재 계좌가 설정되지 않음");
                    return;
                }

                string requestName = "OPW00001";
                string screenNo = "2000";

                System.Diagnostics.Debug.WriteLine($"계좌번호: {_currentAccount}");
                System.Diagnostics.Debug.WriteLine($"요청명: {requestName}");

                _kiwoomApi.SetInputValue("계좌번호", _currentAccount);
                _kiwoomApi.SetInputValue("비밀번호", "");
                _kiwoomApi.SetInputValue("비밀번호입력매체구분", "00");
                _kiwoomApi.SetInputValue("조회구분", "2");

                int result = _kiwoomApi.CommRqData("예수금상세현황요청", requestName, 0, screenNo);
                System.Diagnostics.Debug.WriteLine($"CommRqData 결과: {result}");

                if (result != 0)
                {
                    StatusMessageUpdated?.Invoke($"계좌 정보 조회 요청 실패: {result}");
                    System.Diagnostics.Debug.WriteLine($"❌ 계좌 정보 조회 요청 실패: {result}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("✅ 계좌 정보 조회 요청 전송 완료");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ RequestAccountBalance 실패: {ex.Message}");
            }
        }

        #endregion

        #region 키움 API 이벤트 핸들러

        private void KiwoomApi_OnEventConnect(object sender, _DKHOpenAPIEvents_OnEventConnectEvent e)
        {
            System.Diagnostics.Debug.WriteLine("=== OnEventConnect 이벤트 수신 ===");
            System.Diagnostics.Debug.WriteLine($"연결 결과 코드: {e.nErrCode}");

            if (e.nErrCode == 0)
            {
                _isConnected = true;
                System.Diagnostics.Debug.WriteLine("✅ 키움 연결 성공");

                try
                {
                    if (_kiwoomApi.GetConnectState() == 1)
                    {
                        // KiwoomApiManager.cs의 KiwoomApi_OnEventConnect 메서드에서 
                        // 기존 계좌 처리 부분을 이 코드로 교체하세요:

                        string userId = _kiwoomApi.GetLoginInfo("USER_ID");
                        string userName = _kiwoomApi.GetLoginInfo("USER_NAME");
                        string accountList = _kiwoomApi.GetLoginInfo("ACCLIST");

                        System.Diagnostics.Debug.WriteLine($"사용자 ID: {userId}");
                        System.Diagnostics.Debug.WriteLine($"사용자 이름: {userName}");
                        System.Diagnostics.Debug.WriteLine($"계좌 목록: {accountList}");

                        // 🆕 실제 투자 타입 확인
                        string actualInvestmentType = GetActualInvestmentType();
                        System.Diagnostics.Debug.WriteLine($"실제 로그인 타입: {actualInvestmentType}");

                        // 🆕 계좌 목록 파싱 및 정리
                        string[] accounts = ParseAccountList(accountList);

                        if (accounts.Length > 0)
                        {
                            // 🆕 첫 번째 계좌를 기본 선택 (추후 사용자 설정으로 변경 가능)
                            _currentAccount = accounts[0];
                            System.Diagnostics.Debug.WriteLine($"기본 계좌 설정: {_currentAccount}");

                            // 🆕 개선된 연결 상태 알림 (실제 사용자명 + 실제 투자타입)
                            ConnectionStatusChanged?.Invoke(true, $"{userName}({userId}) - {actualInvestmentType}");

                            // 🆕 계좌 목록을 MainForm에 전달하는 새로운 이벤트 (추가 필요)
                            AccountListReceived?.Invoke(accounts, actualInvestmentType);

                            // 기존: 계좌 잔고 요청
                            RequestAccountBalance();
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("❌ 사용 가능한 계좌가 없음");
                            ConnectionStatusChanged?.Invoke(false, "계좌 정보 없음");
                        }



                        System.Diagnostics.Debug.WriteLine("조건검색식 로드 시도");
                        int conditionResult = _kiwoomApi.GetConditionLoad();
                        System.Diagnostics.Debug.WriteLine($"GetConditionLoad 결과: {conditionResult}");

                        if (conditionResult == 1)
                        {
                            StatusMessageUpdated?.Invoke("조건검색식 로드 요청 완료");
                            System.Diagnostics.Debug.WriteLine("✅ 조건검색식 로드 요청 완료");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"❌ 조건검색식 로드 요청 실패: {conditionResult}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ 연결 후 초기화 처리 실패: {ex.Message}");
                    MessageBox.Show("계좌 정보 조회 중 오류: " + ex.Message, "오류");
                }
            }
            else
            {
                _isConnected = false;
                string errorMsg = GetConnectionErrorMessage(e.nErrCode);
                System.Diagnostics.Debug.WriteLine($"❌ 키움 연결 실패: {errorMsg} (코드: {e.nErrCode})");
                ConnectionStatusChanged?.Invoke(false, errorMsg);
                MessageBox.Show($"키움증권 서버 연결 실패: {errorMsg}", "연결 실패");
            }
        }

        private void KiwoomApi_OnReceiveTrData(object sender, _DKHOpenAPIEvents_OnReceiveTrDataEvent e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== OnReceiveTrData 이벤트 수신 ===");
                System.Diagnostics.Debug.WriteLine($"요청명: {e.sRQName}");
                System.Diagnostics.Debug.WriteLine($"TR코드: {e.sTrCode}");

                if (e.sRQName == "예수금상세현황요청" && e.sTrCode == "OPW00001")
                {
                    System.Diagnostics.Debug.WriteLine("계좌 정보 처리 시작");
                    ProcessAccountBalance(e.sTrCode, e.sRQName);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ OnReceiveTrData 처리 실패: {ex.Message}");
            }
        }

        private void KiwoomApi_OnReceiveConditionVer(object sender, _DKHOpenAPIEvents_OnReceiveConditionVerEvent e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== OnReceiveConditionVer 이벤트 수신 ===");

                string strCondition = _kiwoomApi.GetConditionNameList();
                System.Diagnostics.Debug.WriteLine($"조건검색식 원본 문자열: '{strCondition}'");

                if (!string.IsNullOrEmpty(strCondition))
                {
                    if (strCondition.EndsWith(";"))
                    {
                        strCondition = strCondition.Remove(strCondition.Length - 1);
                        System.Diagnostics.Debug.WriteLine($"세미콜론 제거 후: '{strCondition}'");
                    }

                    string[] condList = strCondition.Split(';');
                    System.Diagnostics.Debug.WriteLine($"분할된 조건검색식 개수: {condList.Length}");

                    int loadedCount = 0;
                    foreach (var item in condList)
                    {
                        if (!string.IsNullOrEmpty(item.Trim()))
                        {
                            loadedCount++;
                            System.Diagnostics.Debug.WriteLine($"  {loadedCount}. {item}");
                        }
                    }

                    if (loadedCount > 0)
                    {
                        StatusMessageUpdated?.Invoke($"조건검색식 {loadedCount}개 로드 완료");
                        System.Diagnostics.Debug.WriteLine($"✅ 유효한 조건검색식 {loadedCount}개 로드됨");
                        ConditionListLoaded?.Invoke(loadedCount);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("❌ 유효한 조건검색식이 없음");
                        MessageBox.Show("조건검색식을 파싱할 수 없습니다.", "오류");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("❌ 조건검색식 문자열이 비어있음");
                    MessageBox.Show("등록된 조건검색식이 없습니다.", "알림");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ OnReceiveConditionVer 처리 실패: {ex.Message}");
                MessageBox.Show("조건검색식 로드 중 오류: " + ex.Message, "오류");
            }
        }

        #endregion

        #region 데이터 처리

        private void ProcessAccountBalance(string trCode, string requestName)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== 계좌 정보 처리 ===");

                string 예수금 = _kiwoomApi.GetCommData(trCode, requestName, 0, "예수금").Trim();
                string 유가잔고평가액 = _kiwoomApi.GetCommData(trCode, requestName, 0, "유가잔고평가액").Trim();
                string 총평가손익금액 = _kiwoomApi.GetCommData(trCode, requestName, 0, "총평가손익금액").Trim();

                System.Diagnostics.Debug.WriteLine($"예수금: {예수금}");
                System.Diagnostics.Debug.WriteLine($"유가잔고평가액: {유가잔고평가액}");
                System.Diagnostics.Debug.WriteLine($"총평가손익금액: {총평가손익금액}");

                long 예수금액 = ParseSafeNumber(예수금);
                long 총평가액 = ParseSafeNumber(유가잔고평가액);
                long 손익금액 = ParseSafeNumber(총평가손익금액);

                var accountInfo = new AccountInfo
                {
                    AccountNumber = _currentAccount,
                    Balance = 예수금액,
                    TotalValue = 총평가액 + 예수금액,
                    TotalProfit = 손익금액
                };

                System.Diagnostics.Debug.WriteLine($"✅ 계좌 정보 파싱 완료");
                System.Diagnostics.Debug.WriteLine($"  계좌: {accountInfo.AccountNumber}");
                System.Diagnostics.Debug.WriteLine($"  예수금: {accountInfo.Balance:N0}원");
                System.Diagnostics.Debug.WriteLine($"  총평가: {accountInfo.TotalValue:N0}원");
                System.Diagnostics.Debug.WriteLine($"  손익: {accountInfo.TotalProfit:N0}원");

                AccountInfoReceived?.Invoke(accountInfo);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ProcessAccountBalance 실패: {ex.Message}");
            }
        }

        #endregion

        #region 유틸리티

        private string GetConnectionErrorMessage(int errorCode)
        {
            switch (errorCode)
            {
                case -100: return "사용자 정보교환 실패";
                case -101: return "서버접속 실패";
                case -102: return "버전처리 실패";
                default: return "알 수 없는 오류";
            }
        }

        private string GetConditionSearchErrorMessage(int errorCode)
        {
            switch (errorCode)
            {
                case 0: return "조건검색 실행 실패";
                case 1: return "성공";
                case -1: return "조건검색식이 없습니다";
                case -2: return "조건검색 실행 중 오류 발생";
                default: return $"알 수 없는 오류 (코드: {errorCode})";
            }
        }

        private long ParseSafeNumber(string numberStr)
        {
            if (string.IsNullOrEmpty(numberStr)) return 0;
            if (long.TryParse(numberStr, out long result)) return result;
            return 0;
        }

        #endregion




        // 🆕 여기에 새로운 region을 추가하세요 (리소스 정리 바로 위에)

        #region 🆕 계좌 관리 및 투자 타입 관련 메서드들

        /// <summary>
        /// 실제 키움 로그인 타입 확인
        /// </summary>
        private string GetActualInvestmentType()
        {
            try
            {
                // 키움 API에서 실제 로그인 타입 확인
                string serverGubun = _kiwoomApi.GetLoginInfo("GetServerGubun");

                // "1"이면 모의투자서버, "0"이면 실서버
                if (serverGubun == "1")
                {
                    return "모의투자";
                }
                else
                {
                    return "실전투자";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"투자 타입 확인 실패: {ex.Message}");
                return "모의투자"; // 안전한 기본값
            }
        }

        /// <summary>
        /// 계좌 목록 파싱 및 정리
        /// </summary>
        private string[] ParseAccountList(string accountList)
        {
            try
            {
                if (string.IsNullOrEmpty(accountList))
                {
                    return new string[0];
                }

                // 세미콜론으로 분할
                string[] rawAccounts = accountList.Split(';');

                // 빈 계좌번호 제거 및 정리
                var cleanAccounts = rawAccounts
                    .Where(account => !string.IsNullOrEmpty(account.Trim()))
                    .Select(account => account.Trim())
                    .ToArray();

                System.Diagnostics.Debug.WriteLine($"파싱된 계좌 목록: {cleanAccounts.Length}개");
                for (int i = 0; i < cleanAccounts.Length; i++)
                {
                    System.Diagnostics.Debug.WriteLine($"  {i + 1}. {cleanAccounts[i]}");
                }

                return cleanAccounts;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"계좌 목록 파싱 실패: {ex.Message}");
                return new string[0];
            }
        }

        /// <summary>
        /// 현재 계좌 변경
        /// </summary>
        public void ChangeCurrentAccount(string accountNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(accountNumber))
                {
                    System.Diagnostics.Debug.WriteLine("❌ 빈 계좌번호로 변경 시도");
                    return;
                }

                if (_currentAccount == accountNumber)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ 동일한 계좌로 변경 시도: {accountNumber}");
                    return;
                }

                string previousAccount = _currentAccount;
                _currentAccount = accountNumber;

                System.Diagnostics.Debug.WriteLine($"계좌 변경: {previousAccount} → {_currentAccount}");

                // 계좌 변경 후 새로운 잔고 정보 요청
                if (_isConnected)
                {
                    RequestAccountBalance();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ 계좌 변경 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 투자 타입 검증 (MainForm에서 선택한 타입과 실제 로그인 타입 비교)
        /// </summary>
        public bool ValidateInvestmentType(string selectedType)
        {
            try
            {
                string actualType = GetActualInvestmentType();

                System.Diagnostics.Debug.WriteLine($"투자 타입 검증: 선택={selectedType}, 실제={actualType}");

                return selectedType == actualType;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"투자 타입 검증 실패: {ex.Message}");
                return false; // 검증 실패 시 안전하게 false 반환
            }
        }

        /// <summary>
        /// 실제 사용자명 반환
        /// </summary>
        public string GetActualUserName()
        {
            try
            {
                if (_isConnected)
                {
                    return _kiwoomApi.GetLoginInfo("USER_NAME");
                }
                return "연결 안됨";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"사용자명 조회 실패: {ex.Message}");
                return "조회 실패";
            }
        }

        #endregion

       

        #region 리소스 정리

        public void Dispose()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== KiwoomApiManager 리소스 정리 ===");

                StopConditionSearch();
                if (_isConnected) Disconnect();

                System.Diagnostics.Debug.WriteLine("✅ KiwoomApiManager 리소스 정리 완료");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ KiwoomApiManager 리소스 정리 실패: {ex.Message}");
            }
        }

        #endregion

        // KiwoomApiManager.cs 클래스 내부에 추가할 메서드들

        #region 과거 데이터 수집 메서드들 (HistoricalDataModule용)

        /// <summary>
        /// OPT10081 TR로 일봉 데이터 수집 (HistoricalDataModule에서 호출)
        /// </summary>
        /// <param name="stockCode">종목코드</param>
        /// <param name="days">조회 일수</param>
        /// <returns>일봉 데이터 리스트</returns>
        public async Task<List<StockTrader3.Models.DailyPrice>> GetHistoricalDataViaOPT10081Async(string stockCode, int days = 60)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== OPT10081 일봉 데이터 수집: {stockCode} ({days}일) ===");

                if (!_isConnected)
                {
                    throw new Exception("키움 API가 연결되지 않았습니다");
                }

                // 수집된 데이터를 저장할 리스트
                var dailyPrices = new List<StockTrader3.Models.DailyPrice>();
                var dataReceived = false;
                var waitHandle = new ManualResetEventSlim(false);

                // 임시 이벤트 핸들러 (일회용)
                _DKHOpenAPIEvents_OnReceiveTrDataEventHandler tempHandler = null;
                tempHandler = (sender, e) =>
                {
                    try
                    {
                        if (e.sTrCode == "OPT10081")
                        {
                            System.Diagnostics.Debug.WriteLine("OPT10081 응답 수신 - 일봉 데이터 파싱 시작");

                            // 수신 데이터 개수 확인
                            int dataCount = _kiwoomApi.GetRepeatCnt(e.sTrCode, e.sRQName);
                            System.Diagnostics.Debug.WriteLine($"일봉 수신 데이터 개수: {dataCount}");

                            for (int i = 0; i < dataCount; i++)
                            {
                                try
                                {
                                    string dateStr = _kiwoomApi.GetCommData(e.sTrCode, e.sRQName, i, "일자").Trim();
                                    string openStr = _kiwoomApi.GetCommData(e.sTrCode, e.sRQName, i, "시가").Trim();
                                    string highStr = _kiwoomApi.GetCommData(e.sTrCode, e.sRQName, i, "고가").Trim();
                                    string lowStr = _kiwoomApi.GetCommData(e.sTrCode, e.sRQName, i, "저가").Trim();
                                    string closeStr = _kiwoomApi.GetCommData(e.sTrCode, e.sRQName, i, "현재가").Trim();
                                    string volumeStr = _kiwoomApi.GetCommData(e.sTrCode, e.sRQName, i, "거래량").Trim();

                                    // 데이터 변환
                                    if (DateTime.TryParseExact(dateStr, "yyyyMMdd", null,
                                        System.Globalization.DateTimeStyles.None, out DateTime date) &&
                                        decimal.TryParse(openStr, out decimal open) &&
                                        decimal.TryParse(highStr, out decimal high) &&
                                        decimal.TryParse(lowStr, out decimal low) &&
                                        decimal.TryParse(closeStr, out decimal close) &&
                                        long.TryParse(volumeStr, out long volume))
                                    {
                                        var dailyPrice = new StockTrader3.Models.DailyPrice
                                        {
                                            Date = date,
                                            Open = Math.Abs(open),
                                            High = Math.Abs(high),
                                            Low = Math.Abs(low),
                                            Close = Math.Abs(close),
                                            Volume = Math.Abs(volume)
                                        };
                                        dailyPrices.Add(dailyPrice);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"일봉 개별 데이터 파싱 실패 [{i}]: {ex.Message}");
                                }
                            }

                            dataReceived = true;
                            waitHandle.Set();

                            // 임시 핸들러 제거
                            _kiwoomApi.OnReceiveTrData -= tempHandler;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"OPT10081 데이터 처리 실패: {ex.Message}");
                        waitHandle.Set();
                        _kiwoomApi.OnReceiveTrData -= tempHandler;
                    }
                };

                // 임시 이벤트 핸들러 등록
                _kiwoomApi.OnReceiveTrData += tempHandler;

                // OPT10081 TR 요청
                _kiwoomApi.SetInputValue("종목코드", stockCode);
                _kiwoomApi.SetInputValue("기준일자", DateTime.Now.ToString("yyyyMMdd"));
                _kiwoomApi.SetInputValue("수정주가구분", "1");

                string requestName = "주식일봉차트조회";
                string trCode = "OPT10081";
                string screenNo = "1001"; // 기존 조건검색과 다른 화면번호

                int result = _kiwoomApi.CommRqData(requestName, trCode, 0, screenNo);

                if (result != 0)
                {
                    _kiwoomApi.OnReceiveTrData -= tempHandler;
                    throw new Exception($"OPT10081 TR 요청 실패: {result}");
                }

                // 응답 대기 (최대 30초)
                bool completed = await Task.Run(() => waitHandle.Wait(30000));

                if (!completed || !dataReceived)
                {
                    _kiwoomApi.OnReceiveTrData -= tempHandler;
                    throw new Exception("일봉 데이터 수신 타임아웃");
                }

                System.Diagnostics.Debug.WriteLine($"✅ {stockCode} 일봉 데이터 {dailyPrices.Count}개 수집 완료");

                // 요청한 일수만큼만 반환
                return dailyPrices.Count > days ? dailyPrices.Take(days).ToList() : dailyPrices;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ GetHistoricalDataViaOPT10081Async 실패: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// OPT10080 TR로 분봉 데이터 수집 (HistoricalDataModule에서 호출)
        /// </summary>
        /// <param name="stockCode">종목코드</param>
        /// <param name="minuteInterval">분봉 간격 (1, 3, 5, 15, 30, 60)</param>
        /// <param name="days">조회 일수</param>
        /// <returns>분봉 데이터 리스트</returns>
        public async Task<List<StockTrader3.Models.MinutePrice>> GetMinuteDataViaOPT10080Async(string stockCode, int minuteInterval = 1, int days = 3)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== OPT10080 분봉 데이터 수집: {stockCode} ({minuteInterval}분봉, {days}일) ===");

                if (!_isConnected)
                {
                    throw new Exception("키움 API가 연결되지 않았습니다");
                }

                // 수집된 데이터를 저장할 리스트
                var minutePrices = new List<StockTrader3.Models.MinutePrice>();
                var dataReceived = false;
                var waitHandle = new ManualResetEventSlim(false);

                // 임시 이벤트 핸들러 (일회용)
                _DKHOpenAPIEvents_OnReceiveTrDataEventHandler tempHandler = null;
                tempHandler = (sender, e) =>
                {
                    try
                    {
                        if (e.sTrCode == "OPT10080")
                        {
                            System.Diagnostics.Debug.WriteLine("OPT10080 응답 수신 - 분봉 데이터 파싱 시작");

                            // 수신 데이터 개수 확인
                            int dataCount = _kiwoomApi.GetRepeatCnt(e.sTrCode, e.sRQName);
                            System.Diagnostics.Debug.WriteLine($"분봉 수신 데이터 개수: {dataCount}");

                            for (int i = 0; i < dataCount; i++)
                            {
                                try
                                {
                                    string dateStr = _kiwoomApi.GetCommData(e.sTrCode, e.sRQName, i, "체결시간").Trim();
                                    string openStr = _kiwoomApi.GetCommData(e.sTrCode, e.sRQName, i, "시가").Trim();
                                    string highStr = _kiwoomApi.GetCommData(e.sTrCode, e.sRQName, i, "고가").Trim();
                                    string lowStr = _kiwoomApi.GetCommData(e.sTrCode, e.sRQName, i, "저가").Trim();
                                    string closeStr = _kiwoomApi.GetCommData(e.sTrCode, e.sRQName, i, "현재가").Trim();
                                    string volumeStr = _kiwoomApi.GetCommData(e.sTrCode, e.sRQName, i, "거래량").Trim();

                                    // 시간 데이터 파싱 (yyyyMMddHHmmss 형식)
                                    if (dateStr.Length >= 14 &&
                                        DateTime.TryParseExact(dateStr, "yyyyMMddHHmmss", null,
                                        System.Globalization.DateTimeStyles.None, out DateTime dateTime) &&
                                        decimal.TryParse(openStr, out decimal open) &&
                                        decimal.TryParse(highStr, out decimal high) &&
                                        decimal.TryParse(lowStr, out decimal low) &&
                                        decimal.TryParse(closeStr, out decimal close) &&
                                        long.TryParse(volumeStr, out long volume))
                                    {
                                        var minutePrice = new StockTrader3.Models.MinutePrice
                                        {
                                            DateTime = dateTime,
                                            MinuteInterval = minuteInterval,
                                            Open = Math.Abs(open),
                                            High = Math.Abs(high),
                                            Low = Math.Abs(low),
                                            Close = Math.Abs(close),
                                            Volume = Math.Abs(volume),
                                            ChangeAmount = 0, // 일단 0으로 설정
                                            ChangeRate = 0,   // 일단 0으로 설정
                                            TradingValue = (long)(Math.Abs(close) * Math.Abs(volume))
                                        };
                                        minutePrices.Add(minutePrice);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"분봉 개별 데이터 파싱 실패 [{i}]: {ex.Message}");
                                }
                            }

                            dataReceived = true;
                            waitHandle.Set();

                            // 임시 핸들러 제거
                            _kiwoomApi.OnReceiveTrData -= tempHandler;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"OPT10080 데이터 처리 실패: {ex.Message}");
                        waitHandle.Set();
                        _kiwoomApi.OnReceiveTrData -= tempHandler;
                    }
                };

                // 임시 이벤트 핸들러 등록
                _kiwoomApi.OnReceiveTrData += tempHandler;

                // OPT10080 TR 요청
                _kiwoomApi.SetInputValue("종목코드", stockCode);
                _kiwoomApi.SetInputValue("틱범위", minuteInterval.ToString());
                _kiwoomApi.SetInputValue("수정주가구분", "1");

                string requestName = "주식분봉차트조회";
                string trCode = "OPT10080";
                string screenNo = "1002"; // 기존 조건검색과 다른 화면번호

                int result = _kiwoomApi.CommRqData(requestName, trCode, 0, screenNo);

                if (result != 0)
                {
                    _kiwoomApi.OnReceiveTrData -= tempHandler;
                    throw new Exception($"OPT10080 TR 요청 실패: {result}");
                }

                // 응답 대기 (최대 30초)
                bool completed = await Task.Run(() => waitHandle.Wait(30000));

                if (!completed || !dataReceived)
                {
                    _kiwoomApi.OnReceiveTrData -= tempHandler;
                    throw new Exception("분봉 데이터 수신 타임아웃");
                }

                System.Diagnostics.Debug.WriteLine($"✅ {stockCode} {minuteInterval}분봉 데이터 {minutePrices.Count}개 수집 완료");

                // 요청한 일수에 해당하는 데이터만 반환
                int minutesPerDay = (int)(6.5 * 60 / minuteInterval);
                int maxCount = days * minutesPerDay;

                return minutePrices.Count > maxCount ? minutePrices.Take(maxCount).ToList() : minutePrices;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ GetMinuteDataViaOPT10080Async 실패: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// HistoricalDataModule용 과거 데이터 수집 통합 메서드
        /// </summary>
        /// <param name="stockCodes">수집할 종목 코드들</param>
        /// <param name="cancellationToken">취소 토큰</param>
        /// <param name="progressCallback">진행률 콜백</param>
        public async Task CollectHistoricalDataAsync(List<string> stockCodes,
            CancellationToken cancellationToken, Action<int, int, string> progressCallback = null)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== KiwoomApiManager 과거 데이터 수집 시작: {stockCodes.Count}개 종목 ===");

                if (!_isConnected)
                {
                    throw new Exception("키움 API가 연결되지 않았습니다");
                }

                // HistoricalDataModule 생성 및 실행
                var connectionString = $"Data Source=StockTrader3.db;Version=3;";
                using (var historicalModule = new HistoricalDataModule(this, connectionString))
                {
                    // 이벤트 연결
                    historicalModule.StatusUpdated += (status) => StatusMessageUpdated?.Invoke(status);
                    historicalModule.ProgressUpdated += (current, total, stockCode) =>
                        progressCallback?.Invoke(current, total, stockCode);

                    // 실제 데이터 수집 실행
                    await historicalModule.CollectHistoricalDataAsync(stockCodes, cancellationToken, progressCallback);
                }

                System.Diagnostics.Debug.WriteLine("✅ KiwoomApiManager 과거 데이터 수집 완료");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ CollectHistoricalDataAsync 실패: {ex.Message}");
                throw;
            }
        }

        #endregion
    }
}