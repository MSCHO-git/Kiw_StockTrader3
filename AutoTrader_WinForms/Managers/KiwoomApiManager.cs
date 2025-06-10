using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AxKHOpenAPILib;
using System.Collections.Concurrent;
using AutoTrader_WinForms.Managers;  // 🔧 이것 추가 (TradingStock 등을 위해)

namespace AutoTrader_WinForms.Managers
{
    /// <summary>
    /// 키움증권 OpenAPI 관리 클래스 (실제 매매 기능 완성)
    /// </summary>
    public class KiwoomApiManager
    {
        #region 필드 및 속성

        private AxKHOpenAPI axKHOpenAPI1;
        private MainForm mainForm;

        // 이벤트 동기화용
        private readonly AutoResetEvent loginEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent trDataEvent = new AutoResetEvent(false);
        private readonly Dictionary<string, AutoResetEvent> orderEvents = new Dictionary<string, AutoResetEvent>();

        // 상태 관리
        public bool IsConnected { get; private set; }
        public bool IsLoggedIn { get; private set; }
        public string CurrentUserId { get; private set; }
        public string CurrentAccount { get; private set; }
        public List<string> AccountList { get; private set; }

        // 계좌 정보
        public decimal AvailableCash { get; private set; }
        public decimal TotalAssets { get; private set; }

        // 화면번호 관리
        private int screenNoCounter = 9000;

        #endregion

        #region 🆕 실제 매매용 핵심 필드

        // 주문 결과 저장용 (화면번호별로 관리)
        private readonly ConcurrentDictionary<string, OrderResult> orderResults = new ConcurrentDictionary<string, OrderResult>();

        // 현재가 조회 결과 저장용
        private readonly ConcurrentDictionary<string, int> currentPrices = new ConcurrentDictionary<string, int>();
        private readonly ConcurrentDictionary<string, DateTime> priceUpdateTimes = new ConcurrentDictionary<string, DateTime>();

        // 체결 정보 실시간 추적
        private readonly ConcurrentDictionary<string, OrderExecutionInfo> liveOrders = new ConcurrentDictionary<string, OrderExecutionInfo>();

        // TR 요청 관리
        private readonly ConcurrentDictionary<string, string> trRequests = new ConcurrentDictionary<string, string>();

        #endregion

        #region 생성자 및 초기화

        public KiwoomApiManager(MainForm form, AxKHOpenAPI kiwoomControl)
        {
            mainForm = form;
            axKHOpenAPI1 = kiwoomControl;
            AccountList = new List<string>();

            // 이벤트 핸들러 연결
            ConnectEvents();
        }

        /// 키움 API 초기화
        /// </summary>
        public bool Initialize()
        {
            try
            {
                if (axKHOpenAPI1 == null)
                {
                    mainForm?.AddLog("❌ 키움 API 컨트롤이 제공되지 않았습니다.");
                    return false;
                }

                mainForm?.AddLog("🔧 키움 API 초기화 완료 (외부 ActiveX 사용)");
                return true;
            }
            catch (Exception ex)
            {
                mainForm?.AddLog($"❌ 키움 API 초기화 실패: {ex.Message}");
                return false;
            }
        }


        /// <summary>
        /// 이벤트 핸들러 연결
        /// </summary>
        private void ConnectEvents()
        {
            try
            {
                if (axKHOpenAPI1 != null)
                {
                    axKHOpenAPI1.OnEventConnect += OnEventConnect;
                    axKHOpenAPI1.OnReceiveTrData += OnReceiveTrData;
                    axKHOpenAPI1.OnReceiveChejanData += OnReceiveChejanData;
                    axKHOpenAPI1.OnReceiveMsg += OnReceiveMsg;
                }
            }
            catch (Exception ex)
            {
                mainForm?.AddLog($"❌ 이벤트 연결 실패: {ex.Message}");
            }
        }

        #endregion

        #region 로그인 및 연결

        /// <summary>
        /// 키움증권 로그인 실행
        /// </summary>
        public bool Login()
        {
            try
            {
                mainForm?.AddLog("🔐 키움증권 로그인 시도 중...");

                // 로그인 윈도우 호출
                int result = axKHOpenAPI1.CommConnect();

                if (result == 0)
                {
                    mainForm?.AddLog("⏳ 로그인 창이 열렸습니다. 로그인을 완료해주세요.");

                    // 로그인 완료까지 대기 (최대 30초)
                    bool loginSuccess = loginEvent.WaitOne(30000);

                    if (loginSuccess && IsLoggedIn)
                    {
                        // 로그인 성공 시 계좌 정보 조회
                        return GetAccountInfo();
                    }
                    else
                    {
                        mainForm?.AddLog("❌ 로그인 시간 초과 또는 실패");
                        return false;
                    }
                }
                else
                {
                    mainForm?.AddLog($"❌ 로그인 실행 실패. 에러코드: {result}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                mainForm?.AddLog($"❌ 로그인 중 오류 발생: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 계좌 정보 조회 및 설정
        /// </summary>
        private bool GetAccountInfo()
        {
            try
            {
                // 사용자 정보 조회
                CurrentUserId = axKHOpenAPI1.GetLoginInfo("USER_ID");
                string userName = axKHOpenAPI1.GetLoginInfo("USER_NAME");

                // 계좌번호 목록 조회
                string accountString = axKHOpenAPI1.GetLoginInfo("ACCLIST");
                if (!string.IsNullOrEmpty(accountString))
                {
                    AccountList = accountString.Split(';')
                                              .Where(acc => !string.IsNullOrEmpty(acc))
                                              .ToList();

                    if (AccountList.Count > 0)
                    {
                        CurrentAccount = AccountList[0]; // 첫 번째 계좌를 기본으로 설정

                        mainForm?.AddLog($"👤 사용자: {userName} ({CurrentUserId})");
                        mainForm?.AddLog($"📋 계좌 {AccountList.Count}개 발견");
                        mainForm?.AddLog($"🎯 기본 계좌: {CurrentAccount}");

                        // 예수금 조회
                        RequestAvailableCash();

                        return true;
                    }
                }

                mainForm?.AddLog("❌ 사용 가능한 계좌가 없습니다.");
                return false;
            }
            catch (Exception ex)
            {
                mainForm?.AddLog($"❌ 계좌 정보 조회 실패: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 예수금 조회 요청
        /// </summary>
        private void RequestAvailableCash()
        {
            try
            {
                string screenNo = GetNextScreenNo();
                trRequests[screenNo] = "예수금조회";

                axKHOpenAPI1.SetInputValue("계좌번호", CurrentAccount);
                axKHOpenAPI1.SetInputValue("비밀번호", "");
                axKHOpenAPI1.SetInputValue("비밀번호입력매체구분", "00");
                axKHOpenAPI1.SetInputValue("조회구분", "2");

                int result = axKHOpenAPI1.CommRqData("예수금조회", "opw00001", 0, screenNo);

                if (result == 0)
                {
                    mainForm?.AddLog("💰 예수금 조회 요청 중...");
                    // TR 데이터 수신 대기
                    trDataEvent.WaitOne(5000);
                }
                else
                {
                    mainForm?.AddLog($"❌ 예수금 조회 실패. 에러코드: {result}");
                }
            }
            catch (Exception ex)
            {
                mainForm?.AddLog($"❌ 예수금 조회 중 오류: {ex.Message}");
            }
        }

        #endregion

        #region 🆕 실제 매매 기능 (완전 구현)

        /// <summary>
        /// 매수 주문 전송 (완전 구현)
        /// </summary>
        public async Task<OrderResult> SendBuyOrder(string stockCode, int quantity, int price)
        {
            try
            {
                mainForm?.AddLog($"🔵 매수 주문 시작: {stockCode} {quantity}주 @ {price:N0}원");

                // 입력값 검증
                if (string.IsNullOrEmpty(stockCode) || quantity <= 0 || price <= 0)
                {
                    return new OrderResult
                    {
                        Success = false,
                        ErrorMessage = "입력값 오류: 종목코드, 수량, 가격을 확인해주세요"
                    };
                }

                // 잔고 확인
                decimal requiredAmount = (decimal)(quantity * price);
                if (AvailableCash < requiredAmount * 1.01m) // 1% 여유분 고려
                {
                    return new OrderResult
                    {
                        Success = false,
                        ErrorMessage = $"잔고 부족: 필요금액 {requiredAmount:N0}원, 보유금액 {AvailableCash:N0}원"
                    };
                }

                // 실제 키움 API 주문 전송
                string screenNo = GetNextScreenNo();
                orderResults[screenNo] = new OrderResult { OrderTime = DateTime.Now };

                int result = axKHOpenAPI1.SendOrder(
                    "매수주문",           // sRQName (주문명)
                    screenNo,           // sScreenNo (화면번호)
                    CurrentAccount,     // sAccNo (계좌번호)
                    1,                  // nOrderType (1:신규매수, 2:신규매도, 3:매수취소, 4:매도취소, 5:매수정정, 6:매도정정)
                    stockCode,          // sCode (종목코드)
                    quantity,           // nQty (주문수량)
                    price,              // nPrice (주문가격, 0이면 시장가)
                    "00",              // sHogaGb (00:지정가, 03:시장가, 05:조건부지정가, 06:최유리지정가, 07:최우선지정가)
                    ""                 // sOrgOrderNo (원주문번호, 신규주문시 공백)
                );

                if (result == 0)
                {
                    mainForm?.AddLog($"📤 매수 주문 전송 완료: {stockCode}");

                    // 주문 결과 대기 (최대 10초)
                    var orderResult = await WaitForOrderResult(screenNo, 10000);

                    if (orderResult.Success)
                    {
                        mainForm?.AddLog($"✅ 매수 주문 성공: {stockCode} - 주문번호: {orderResult.OrderNo}");

                        // 체결 대기 (최대 30초)
                        var executionResult = await WaitForExecution(orderResult.OrderNo, 30000);

                        // 최종 결과 반환
                        return new OrderResult
                        {
                            Success = true,
                            OrderNo = orderResult.OrderNo,
                            Status = executionResult.Status,
                            FilledQuantity = executionResult.FilledQuantity,
                            RemainQuantity = executionResult.RemainQuantity,
                            AvgPrice = executionResult.AvgFillPrice,
                            ErrorMessage = ""
                        };
                    }
                    else
                    {
                        return orderResult;
                    }
                }
                else
                {
                    string errorMsg = GetErrorMessage(result);
                    mainForm?.AddLog($"❌ 매수 주문 전송 실패: {errorMsg} (코드: {result})");

                    return new OrderResult
                    {
                        Success = false,
                        ErrorMessage = $"주문 전송 실패: {errorMsg}"
                    };
                }
            }
            catch (Exception ex)
            {
                mainForm?.AddLog($"❌ 매수 주문 오류: {ex.Message}");
                return new OrderResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// 매도 주문 전송 (완전 구현)
        /// </summary>
        public async Task<OrderResult> SendSellOrder(string stockCode, int quantity, int price)
        {
            try
            {
                string priceText = price == 0 ? "시장가" : $"{price:N0}원";
                mainForm?.AddLog($"🔴 매도 주문 시작: {stockCode} {quantity}주 @ {priceText}");

                // 입력값 검증
                if (string.IsNullOrEmpty(stockCode) || quantity <= 0)
                {
                    return new OrderResult
                    {
                        Success = false,
                        ErrorMessage = "입력값 오류: 종목코드, 수량을 확인해주세요"
                    };
                }

                // 실제 키움 API 주문 전송
                string screenNo = GetNextScreenNo();
                orderResults[screenNo] = new OrderResult { OrderTime = DateTime.Now };

                string hogaType = price == 0 ? "03" : "00"; // 03:시장가, 00:지정가

                int result = axKHOpenAPI1.SendOrder(
                    "매도주문",           // sRQName (주문명)
                    screenNo,           // sScreenNo (화면번호)
                    CurrentAccount,     // sAccNo (계좌번호)
                    2,                  // nOrderType (2:신규매도)
                    stockCode,          // sCode (종목코드)
                    quantity,           // nQty (주문수량)
                    price,              // nPrice (주문가격, 0이면 시장가)
                    hogaType,          // sHogaGb (00:지정가, 03:시장가)
                    ""                 // sOrgOrderNo (원주문번호, 신규주문시 공백)
                );

                if (result == 0)
                {
                    mainForm?.AddLog($"📤 매도 주문 전송 완료: {stockCode}");

                    // 주문 결과 대기
                    var orderResult = await WaitForOrderResult(screenNo, 10000);

                    if (orderResult.Success)
                    {
                        mainForm?.AddLog($"✅ 매도 주문 성공: {stockCode} - 주문번호: {orderResult.OrderNo}");

                        // 체결 대기
                        var executionResult = await WaitForExecution(orderResult.OrderNo, 30000);

                        return new OrderResult
                        {
                            Success = true,
                            OrderNo = orderResult.OrderNo,
                            Status = executionResult.Status,
                            FilledQuantity = executionResult.FilledQuantity,
                            RemainQuantity = executionResult.RemainQuantity,
                            AvgPrice = executionResult.AvgFillPrice,
                            ErrorMessage = ""
                        };
                    }
                    else
                    {
                        return orderResult;
                    }
                }
                else
                {
                    string errorMsg = GetErrorMessage(result);
                    mainForm?.AddLog($"❌ 매도 주문 전송 실패: {errorMsg} (코드: {result})");

                    return new OrderResult
                    {
                        Success = false,
                        ErrorMessage = $"주문 전송 실패: {errorMsg}"
                    };
                }
            }
            catch (Exception ex)
            {
                mainForm?.AddLog($"❌ 매도 주문 오류: {ex.Message}");
                return new OrderResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// 현재가 조회 (완전 구현)
        /// </summary>
        public async Task<int> GetCurrentPrice(string stockCode)
        {
            try
            {
                // 캐시된 데이터 확인 (3초 이내)
                if (currentPrices.ContainsKey(stockCode) &&
                    priceUpdateTimes.ContainsKey(stockCode) &&
                    (DateTime.Now - priceUpdateTimes[stockCode]).TotalSeconds < 3)
                {
                    return currentPrices[stockCode];
                }

                string screenNo = GetNextScreenNo();
                trRequests[screenNo] = $"현재가조회_{stockCode}";

                // 현재가 조회 TR 요청 (OPT10001)
                axKHOpenAPI1.SetInputValue("종목코드", stockCode);

                int result = axKHOpenAPI1.CommRqData("현재가조회", "OPT10001", 0, screenNo);

                if (result == 0)
                {
                    // TR 응답 대기 (최대 3초)
                    bool received = trDataEvent.WaitOne(3000);

                    if (received && currentPrices.ContainsKey(stockCode))
                    {
                        return currentPrices[stockCode];
                    }
                }

                mainForm?.AddLog($"⚠️ 현재가 조회 실패: {stockCode}");
                return 0;
            }
            catch (Exception ex)
            {
                mainForm?.AddLog($"❌ 현재가 조회 오류: {stockCode} - {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// 주문 취소 (완전 구현)
        /// </summary>
        public async Task<bool> CancelOrder(string orderNo)
        {
            try
            {
                mainForm?.AddLog($"🚫 주문 취소 시작: {orderNo}");

                if (string.IsNullOrEmpty(orderNo))
                {
                    mainForm?.AddLog("❌ 주문번호가 유효하지 않습니다.");
                    return false;
                }

                // 주문 정보 조회
                if (!liveOrders.ContainsKey(orderNo))
                {
                    mainForm?.AddLog($"❌ 주문 정보를 찾을 수 없습니다: {orderNo}");
                    return false;
                }

                var orderInfo = liveOrders[orderNo];
                string screenNo = GetNextScreenNo();
                orderResults[screenNo] = new OrderResult { OrderTime = DateTime.Now };

                // 취소 주문 전송
                int orderType = orderInfo.OrderType == "매수" ? 3 : 4; // 3:매수취소, 4:매도취소

                int result = axKHOpenAPI1.SendOrder(
                    "주문취소",           // sRQName
                    screenNo,           // sScreenNo
                    CurrentAccount,     // sAccNo
                    orderType,          // nOrderType (3:매수취소, 4:매도취소)
                    orderInfo.StockCode, // sCode
                    0,                  // nQty (취소시 0)
                    0,                  // nPrice (취소시 0)
                    "00",              // sHogaGb
                    orderNo            // sOrgOrderNo (원주문번호)
                );

                if (result == 0)
                {
                    mainForm?.AddLog($"📤 취소 주문 전송 완료: {orderNo}");

                    // 취소 결과 대기
                    var cancelResult = await WaitForOrderResult(screenNo, 10000);

                    if (cancelResult.Success)
                    {
                        mainForm?.AddLog($"✅ 주문 취소 완료: {orderNo}");
                        return true;
                    }
                    else
                    {
                        mainForm?.AddLog($"❌ 주문 취소 실패: {cancelResult.ErrorMessage}");
                        return false;
                    }
                }
                else
                {
                    string errorMsg = GetErrorMessage(result);
                    mainForm?.AddLog($"❌ 취소 주문 전송 실패: {errorMsg}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                mainForm?.AddLog($"❌ 주문 취소 오류: {orderNo} - {ex.Message}");
                return false;
            }
        }

        #endregion

        #region 🆕 이벤트 처리 (완전 구현)

        /// <summary>
        /// 로그인 상태 이벤트 핸들러
        /// </summary>
        private void OnEventConnect(object sender, _DKHOpenAPIEvents_OnEventConnectEvent e)
        {
            try
            {
                if (e.nErrCode == 0)
                {
                    IsConnected = true;
                    IsLoggedIn = true;
                    mainForm?.AddLog("✅ 키움증권 로그인 성공!");
                }
                else
                {
                    IsConnected = false;
                    IsLoggedIn = false;
                    string errorMsg = GetErrorMessage(e.nErrCode);
                    mainForm?.AddLog($"❌ 키움증권 로그인 실패: {errorMsg} (코드: {e.nErrCode})");
                }

                loginEvent.Set();
            }
            catch (Exception ex)
            {
                mainForm?.AddLog($"❌ 로그인 이벤트 처리 오류: {ex.Message}");
                loginEvent.Set();
            }
        }

        /// <summary>
        /// TR 데이터 수신 이벤트 핸들러 (완전 구현)
        /// </summary>
        private void OnReceiveTrData(object sender, _DKHOpenAPIEvents_OnReceiveTrDataEvent e)
        {
            try
            {
                string requestName = trRequests.ContainsKey(e.sScrNo) ? trRequests[e.sScrNo] : e.sRQName;

                if (requestName == "예수금조회")
                {
                    ProcessCashData(e.sTrCode, e.sRQName);
                }
                else if (requestName.StartsWith("현재가조회_"))
                {
                    string stockCode = requestName.Replace("현재가조회_", "");
                    ProcessCurrentPriceData(e.sTrCode, e.sRQName, stockCode);
                }

                // 화면번호별 요청 정리
                if (trRequests.ContainsKey(e.sScrNo))
                {
                    trRequests.TryRemove(e.sScrNo, out _);
                }

                trDataEvent.Set();
            }
            catch (Exception ex)
            {
                mainForm?.AddLog($"❌ TR 데이터 처리 오류: {ex.Message}");
                trDataEvent.Set();
            }
        }

        /// <summary>
        /// 주문 체결 이벤트 핸들러 (완전 구현)
        /// </summary>
        private void OnReceiveChejanData(object sender, _DKHOpenAPIEvents_OnReceiveChejanDataEvent e)
        {
            try
            {
                if (e.sGubun == "0") // 주문체결
                {
                    ProcessOrderExecution(e);
                }
                else if (e.sGubun == "1") // 잔고변경
                {
                    ProcessBalanceChange(e);
                }
            }
            catch (Exception ex)
            {
                mainForm?.AddLog($"❌ 체결 데이터 처리 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// 서버 메시지 수신 이벤트 핸들러
        /// </summary>
        private void OnReceiveMsg(object sender, _DKHOpenAPIEvents_OnReceiveMsgEvent e)
        {
            try
            {
                if (!string.IsNullOrEmpty(e.sMsg))
                {
                    // 에러 메시지 필터링
                    if (e.sMsg.Contains("정상처리"))
                    {
                        mainForm?.AddLog($"✅ {e.sMsg}");
                    }
                    else if (e.sMsg.Contains("오류") || e.sMsg.Contains("실패"))
                    {
                        mainForm?.AddLog($"❌ 서버 오류: {e.sMsg}");
                    }
                    else
                    {
                        mainForm?.AddLog($"📢 서버: {e.sMsg}");
                    }
                }
            }
            catch (Exception ex)
            {
                mainForm?.AddLog($"❌ 서버 메시지 처리 오류: {ex.Message}");
            }
        }

        #endregion

        #region 🆕 데이터 처리 (완전 구현)

        /// <summary>
        /// 예수금 데이터 처리
        /// </summary>
        private void ProcessCashData(string trCode, string rqName)
        {
            try
            {
                string cashData = axKHOpenAPI1.GetCommData(trCode, rqName, 0, "d+2추정예수금").Trim();
                string totalAssetData = axKHOpenAPI1.GetCommData(trCode, rqName, 0, "총평가금액").Trim();

                if (decimal.TryParse(cashData, out decimal cash))
                {
                    AvailableCash = cash;
                    mainForm?.AddLog($"💰 사용 가능 금액: {AvailableCash:N0}원");
                }

                if (decimal.TryParse(totalAssetData, out decimal totalAsset))
                {
                    TotalAssets = totalAsset;
                    mainForm?.AddLog($"💎 총 자산: {TotalAssets:N0}원");
                }
            }
            catch (Exception ex)
            {
                mainForm?.AddLog($"❌ 예수금 데이터 처리 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// 현재가 데이터 처리
        /// </summary>
        private void ProcessCurrentPriceData(string trCode, string rqName, string stockCode)
        {
            try
            {
                string priceData = axKHOpenAPI1.GetCommData(trCode, rqName, 0, "현재가").Trim();

                if (int.TryParse(priceData, out int price))
                {
                    // 음수 부호 제거 (키움 API는 하락시 음수로 표시)
                    price = Math.Abs(price);

                    currentPrices[stockCode] = price;
                    priceUpdateTimes[stockCode] = DateTime.Now;

                    // 너무 자주 로그를 찍지 않도록 조건부 로깅
                    if (DateTime.Now.Second % 10 == 0)
                    {
                        mainForm?.AddLog($"📊 {stockCode} 현재가: {price:N0}원");
                    }
                }
            }
            catch (Exception ex)
            {
                mainForm?.AddLog($"❌ 현재가 데이터 처리 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// 주문 체결 정보 처리
        /// </summary>
        private void ProcessOrderExecution(dynamic e)
        {
            try
            {
                string orderNo = axKHOpenAPI1.GetChejanData(9203).Trim(); // 주문번호
                string stockCode = axKHOpenAPI1.GetChejanData(9001).Trim(); // 종목코드
                string stockName = axKHOpenAPI1.GetChejanData(302).Trim(); // 종목명
                string orderType = axKHOpenAPI1.GetChejanData(905).Trim(); // 주문구분 (+매수, -매도)
                string orderStatus = axKHOpenAPI1.GetChejanData(913).Trim(); // 주문상태 (접수, 체결, 확인 등)

                int orderQty = Math.Abs(int.Parse(axKHOpenAPI1.GetChejanData(900).Trim())); // 주문수량
                int filledQty = Math.Abs(int.Parse(axKHOpenAPI1.GetChejanData(911).Trim())); // 체결수량
                int remainQty = Math.Abs(int.Parse(axKHOpenAPI1.GetChejanData(902).Trim())); // 미체결수량

                string priceStr = axKHOpenAPI1.GetChejanData(910).Trim(); // 체결가
                int filledPrice = string.IsNullOrEmpty(priceStr) ? 0 : Math.Abs(int.Parse(priceStr));

                // 주문 정보 업데이트
                var orderInfo = new OrderExecutionInfo
                {
                    OrderNo = orderNo,
                    StockCode = stockCode,
                    StockName = stockName,
                    OrderType = orderType.StartsWith("+") ? "매수" : "매도",
                    OrderQuantity = orderQty,
                    FilledQuantity = filledQty,
                    AvgFillPrice = filledPrice,
                    Status = orderStatus,
                    OrderTime = DateTime.Now
                };

                if (filledQty > 0)
                {
                    orderInfo.FillTime = DateTime.Now;
                }

                liveOrders[orderNo] = orderInfo;

                // 로깅
                if (filledQty > 0)
                {
                    string orderTypeText = orderInfo.OrderType;
                    mainForm?.AddLog($"✅ {orderTypeText} 체결: {stockName} {filledQty}주 @ {filledPrice:N0}원 (미체결: {remainQty}주)");
                }
                else
                {
                    mainForm?.AddLog($"📝 주문 접수: {stockName} {orderInfo.OrderType} {orderQty}주");
                }
            }
            catch (Exception ex)
            {
                mainForm?.AddLog($"❌ 주문 체결 정보 처리 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// 잔고 변경 정보 처리
        /// </summary>
        private void ProcessBalanceChange(dynamic e)
        {
            try
            {
                // 잔고 변경 시 예수금 자동 업데이트
                RequestAvailableCash();
            }
            catch (Exception ex)
            {
                mainForm?.AddLog($"❌ 잔고 변경 처리 오류: {ex.Message}");
            }
        }

        #endregion

        #region 🆕 대기 및 결과 처리 (완전 구현)

        /// <summary>
        /// 주문 결과 대기
        /// </summary>
        private async Task<OrderResult> WaitForOrderResult(string screenNo, int timeoutMs)
        {
            var startTime = DateTime.Now;

            while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
            {
                if (orderResults.ContainsKey(screenNo))
                {
                    var result = orderResults[screenNo];
                    if (!string.IsNullOrEmpty(result.OrderNo) || !string.IsNullOrEmpty(result.ErrorMessage))
                    {
                        return result;
                    }
                }

                await Task.Delay(100); // 100ms마다 체크
            }

            return new OrderResult
            {
                Success = false,
                ErrorMessage = "주문 결과 대기 시간 초과"
            };
        }

        /// <summary>
        /// 체결 대기
        /// </summary>
        private async Task<OrderExecutionInfo> WaitForExecution(string orderNo, int timeoutMs)
        {
            var startTime = DateTime.Now;

            while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
            {
                if (liveOrders.ContainsKey(orderNo))
                {
                    var execution = liveOrders[orderNo];

                    // 완전 체결 또는 부분 체결 확인
                    if (execution.FilledQuantity > 0)
                    {
                        // 부분 체결률 계산
                        double fillRatio = (double)execution.FilledQuantity / execution.OrderQuantity;

                        if (fillRatio >= 1.0)
                        {
                            execution.Status = "완전체결";
                        }
                        else if (fillRatio >= 0.7) // 70% 이상 체결
                        {
                            execution.Status = "부분체결";
                        }
                        else
                        {
                            execution.Status = "소량체결";
                        }

                        return execution;
                    }
                }

                await Task.Delay(500); // 0.5초마다 체크
            }

            // 시간 초과 시 미체결 상태 반환
            return new OrderExecutionInfo
            {
                OrderNo = orderNo,
                Status = "미체결",
                FilledQuantity = 0,
                AvgFillPrice = 0
            };
        }

        #endregion

        #region 유틸리티

        /// <summary>
        /// 고유한 화면번호 생성
        /// </summary>
        private string GetNextScreenNo()
        {
            return (++screenNoCounter).ToString();
        }

        /// <summary>
        /// 에러 코드를 메시지로 변환
        /// </summary>
        private string GetErrorMessage(int errorCode)
        {
            switch (errorCode)
            {
                case 0: return "정상처리";
                case -100: return "사용자정보교환실패";
                case -101: return "서버접속실패";
                case -102: return "버전처리실패";
                case -103: return "개인방화벽실패";
                case -104: return "메모리보호실패";
                case -105: return "함수입력값오류";
                case -106: return "통신연결종료";
                case -107: return "보안모듈오류";
                case -108: return "공인인증로그인필요";
                case -200: return "시세조회과부하";
                case -201: return "REQUEST_INPUT_st_req_over";
                default: return $"알 수 없는 오류 ({errorCode})";
            }
        }

        /// <summary>
        /// 계좌 변경
        /// </summary>
        public bool ChangeAccount(string accountNo)
        {
            if (AccountList.Contains(accountNo))
            {
                CurrentAccount = accountNo;
                mainForm?.AddLog($"🔄 계좌 변경: {accountNo}");

                // 새 계좌의 예수금 조회
                RequestAvailableCash();
                return true;
            }

            mainForm?.AddLog($"❌ 존재하지 않는 계좌: {accountNo}");
            return false;
        }

        /// <summary>
        /// 연결 해제
        /// </summary>
        public void Disconnect()
        {
            try
            {
                if (IsConnected)
                {
                    axKHOpenAPI1.CommTerminate();
                    IsConnected = false;
                    IsLoggedIn = false;
                    mainForm?.AddLog("🔌 키움 API 연결 해제");
                }
            }
            catch (Exception ex)
            {
                mainForm?.AddLog($"❌ 연결 해제 중 오류: {ex.Message}");
            }
        }

        #endregion

        #region IDisposable 구현

        private bool disposed = false;

        public void Dispose()
        {
            if (!disposed)
            {
                try
                {
                    Disconnect();

                    loginEvent?.Dispose();
                    trDataEvent?.Dispose();

                    foreach (var evt in orderEvents.Values)
                    {
                        evt?.Dispose();
                    }

                    if (axKHOpenAPI1 != null)
                    {
                        axKHOpenAPI1.OnEventConnect -= OnEventConnect;
                        axKHOpenAPI1.OnReceiveTrData -= OnReceiveTrData;
                        axKHOpenAPI1.OnReceiveChejanData -= OnReceiveChejanData;
                        axKHOpenAPI1.OnReceiveMsg -= OnReceiveMsg;
                    }
                }
                catch (Exception ex)
                {
                    mainForm?.AddLog($"❌ Dispose 중 오류: {ex.Message}");
                }

                disposed = true;
            }
        }

        #endregion
    }
}