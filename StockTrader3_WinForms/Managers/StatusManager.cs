using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace StockTrader3_WinForms
{
    /// <summary>
    /// 상태바 관리 및 통계 카드 업데이트를 담당하는 클래스 (Null 체크 완전 수정)
    /// </summary>
    public class StatusManager
    {
        #region 생성자 및 초기화

        private readonly MainForm _mainForm;

        // 상태바 및 통계 카드 컨트롤들 (null 허용)
        private readonly ToolStripStatusLabel _lblStatus;
        private readonly Label _lblSearchCount;
        private readonly Label _lblSGradeCount;
        private readonly Label _lblAGradeCount;
        private readonly Label _lblAvgProfit;

        // 완료된 작업 추적 변수들 (MainForm에서 이동)
        private bool _isKiwoomConnected = false;
        private int _loadedConditionCount = 0;
        private bool _conditionSearchCompleted = false;
        private bool _technicalAnalysisCompleted = false;
        private bool _newsAnalysisCompleted = false;
        private bool _tradePlanCompleted = false;

        public StatusManager(MainForm mainForm, ToolStripStatusLabel lblStatus,
                           Label lblSearchCount, Label lblSGradeCount,
                           Label lblAGradeCount, Label lblAvgProfit)
        {
            _mainForm = mainForm ?? throw new ArgumentNullException(nameof(mainForm));
            _lblStatus = lblStatus ?? throw new ArgumentNullException(nameof(lblStatus));

            // null 허용으로 수정 (통계 카드들이 없을 수 있음)
            _lblSearchCount = lblSearchCount;
            _lblSGradeCount = lblSGradeCount;
            _lblAGradeCount = lblAGradeCount;
            _lblAvgProfit = lblAvgProfit;
        }

        #endregion

        #region 공개 속성 (상태 추적용)

        /// <summary>
        /// 키움 연결 상태
        /// </summary>
        public bool IsKiwoomConnected
        {
            get => _isKiwoomConnected;
            set => _isKiwoomConnected = value;
        }

        /// <summary>
        /// 로드된 조건검색식 개수
        /// </summary>
        public int LoadedConditionCount
        {
            get => _loadedConditionCount;
            set => _loadedConditionCount = value;
        }

        /// <summary>
        /// 조건검색 완료 여부
        /// </summary>
        public bool ConditionSearchCompleted
        {
            get => _conditionSearchCompleted;
            set => _conditionSearchCompleted = value;
        }

        /// <summary>
        /// 1차 분석 완료 여부
        /// </summary>
        public bool TechnicalAnalysisCompleted
        {
            get => _technicalAnalysisCompleted;
            set => _technicalAnalysisCompleted = value;
        }

        /// <summary>
        /// 2차 분석 완료 여부
        /// </summary>
        public bool NewsAnalysisCompleted
        {
            get => _newsAnalysisCompleted;
            set => _newsAnalysisCompleted = value;
        }

        /// <summary>
        /// 매매계획 완료 여부
        /// </summary>
        public bool TradePlanCompleted
        {
            get => _tradePlanCompleted;
            set => _tradePlanCompleted = value;
        }

        #endregion

        #region 상태바 요약 정보 업데이트

        /// <summary>
        /// 상태바에 완료된 작업들의 요약 정보를 표시
        /// </summary>
        public void UpdateStatusBarSummary()
        {
            try
            {
                List<string> completedTasks = new List<string>();

                if (_isKiwoomConnected)
                {
                    completedTasks.Add("키움 연결 완료");
                }

                if (_loadedConditionCount > 0)
                {
                    completedTasks.Add($"조건검색식 {_loadedConditionCount}개 로드 완료");
                }

                if (_conditionSearchCompleted)
                {
                    completedTasks.Add("조건검색 완료");
                }

                if (_technicalAnalysisCompleted)
                {
                    completedTasks.Add("1차 분석 완료");
                }

                if (_newsAnalysisCompleted)
                {
                    completedTasks.Add("2차 분석 완료");
                }

                if (_tradePlanCompleted)
                {
                    completedTasks.Add("매매계획 완료");
                }

                if (completedTasks.Count > 0)
                {
                    string summary = string.Join(" | ", completedTasks);

                    // 현재 시간 추가
                    string timeInfo = $"시간: {DateTime.Now:HH:mm:ss}";

                    // 상태바에 요약 정보 표시
                    UpdateStatusBarText(summary, timeInfo);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("상태바 요약 업데이트 실패: " + ex.Message);
            }
        }

        /// <summary>
        /// 상태바 텍스트 업데이트 (현재 진행 중인 작업 고려)
        /// </summary>
        private void UpdateStatusBarText(string summary, string timeInfo)
        {
            try
            {
                if (_lblStatus == null) return;

                // UI 스레드에서 실행되도록 보장
                if (_mainForm.InvokeRequired)
                {
                    _mainForm.Invoke(new Action(() => UpdateStatusBarText(summary, timeInfo)));
                    return;
                }

                // 현재 진행 중인 작업이 있으면 그것을 우선 표시하고, 요약은 뒤에
                string currentStatus = _lblStatus.Text ?? "";
                if (currentStatus.Contains("중...") || currentStatus.Contains("대기"))
                {
                    _lblStatus.Text = $"{currentStatus} | {summary} | {timeInfo}";
                }
                else
                {
                    _lblStatus.Text = $"{summary} | {timeInfo}";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("상태바 텍스트 업데이트 실패: " + ex.Message);
            }
        }

        /// <summary>
        /// 상태바에 단순 메시지 표시
        /// </summary>
        public void SetStatusMessage(string message)
        {
            try
            {
                if (_lblStatus == null) return;

                // UI 스레드에서 실행되도록 보장
                if (_mainForm.InvokeRequired)
                {
                    _mainForm.Invoke(new Action(() => SetStatusMessage(message)));
                    return;
                }

                _lblStatus.Text = message;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("상태 메시지 설정 실패: " + ex.Message);
            }
        }

        #endregion

        #region 통계 카드 업데이트 (Null 체크 추가)

        /// <summary>
        /// 검색 종목 수 업데이트 (Null 체크 추가)
        /// </summary>
        public void UpdateSearchCount(int count)
        {
            try
            {
                // Null 체크 추가
                if (_lblSearchCount == null) return;

                // UI 스레드에서 실행되도록 보장
                if (_mainForm.InvokeRequired)
                {
                    _mainForm.Invoke(new Action(() => UpdateSearchCount(count)));
                    return;
                }

                _lblSearchCount.Text = $"검색종목: {count}개";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("검색 종목 수 업데이트 실패: " + ex.Message);
            }
        }

        /// <summary>
        /// S급 종목 수 업데이트 (Null 체크 추가)
        /// </summary>
        public void UpdateSGradeCount(int count)
        {
            try
            {
                // Null 체크 추가
                if (_lblSGradeCount == null) return;

                // UI 스레드에서 실행되도록 보장
                if (_mainForm.InvokeRequired)
                {
                    _mainForm.Invoke(new Action(() => UpdateSGradeCount(count)));
                    return;
                }

                _lblSGradeCount.Text = $"S급: {count}개";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("S급 종목 수 업데이트 실패: " + ex.Message);
            }
        }

        /// <summary>
        /// A급 종목 수 업데이트 (Null 체크 추가)
        /// </summary>
        public void UpdateAGradeCount(int count)
        {
            try
            {
                // Null 체크 추가
                if (_lblAGradeCount == null) return;

                // UI 스레드에서 실행되도록 보장
                if (_mainForm.InvokeRequired)
                {
                    _mainForm.Invoke(new Action(() => UpdateAGradeCount(count)));
                    return;
                }

                _lblAGradeCount.Text = $"A급: {count}개";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("A급 종목 수 업데이트 실패: " + ex.Message);
            }
        }

        /// <summary>
        /// 평균 수익률 업데이트 (Null 체크 추가)
        /// </summary>
        public void UpdateAverageProfit(double profitRate)
        {
            try
            {
                // Null 체크 추가
                if (_lblAvgProfit == null) return;

                // UI 스레드에서 실행되도록 보장
                if (_mainForm.InvokeRequired)
                {
                    _mainForm.Invoke(new Action(() => UpdateAverageProfit(profitRate)));
                    return;
                }

                _lblAvgProfit.Text = $"평균수익률: {profitRate:F1}%";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("평균 수익률 업데이트 실패: " + ex.Message);
            }
        }

        /// <summary>
        /// 모든 통계 카드를 한번에 업데이트 (Null 체크 포함)
        /// </summary>
        public void UpdateAllStatistics(int searchCount, int sGradeCount, int aGradeCount, double avgProfit)
        {
            UpdateSearchCount(searchCount);
            UpdateSGradeCount(sGradeCount);
            UpdateAGradeCount(aGradeCount);
            UpdateAverageProfit(avgProfit);
        }

        #endregion

        #region 단계 완료 상태 업데이트

        /// <summary>
        /// 단계별 완료 상태 업데이트 (진행 상황에 따라 자동 호출)
        /// </summary>
        public void UpdateStepCompletion(string stepName)
        {
            try
            {
                if (stepName.Contains("조건검색"))
                {
                    _conditionSearchCompleted = true;
                }
                else if (stepName.Contains("기술적 분석") || stepName.Contains("1차 분석"))
                {
                    _technicalAnalysisCompleted = true;
                }
                else if (stepName.Contains("뉴스 분석") || stepName.Contains("2차 분석"))
                {
                    _newsAnalysisCompleted = true;
                }
                else if (stepName.Contains("매매계획"))
                {
                    _tradePlanCompleted = true;
                }

                // 상태 변경 후 요약 정보 업데이트
                UpdateStatusBarSummary();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("단계 완료 상태 업데이트 실패: " + ex.Message);
            }
        }

        #endregion

        #region 초기화

        /// <summary>
        /// 통계 카드를 초기 상태로 설정 (Null 체크 포함)
        /// </summary>
        public void InitializeStatistics()
        {
            try
            {
                UpdateSearchCount(0);
                UpdateSGradeCount(0);
                UpdateAGradeCount(0);
                UpdateAverageProfit(0.0);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("통계 초기화 실패: " + ex.Message);
            }
        }

        /// <summary>
        /// 모든 완료 상태를 초기화
        /// </summary>
        public void ResetAllCompletionStatus()
        {
            _isKiwoomConnected = false;
            _loadedConditionCount = 0;
            _conditionSearchCompleted = false;
            _technicalAnalysisCompleted = false;
            _newsAnalysisCompleted = false;
            _tradePlanCompleted = false;

            UpdateStatusBarSummary();
        }

        #endregion

        #region 유틸리티

        /// <summary>
        /// 현재 완료된 단계 수 반환
        /// </summary>
        public int GetCompletedStepsCount()
        {
            int count = 0;
            if (_conditionSearchCompleted) count++;
            if (_technicalAnalysisCompleted) count++;
            if (_newsAnalysisCompleted) count++;
            if (_tradePlanCompleted) count++;
            return count;
        }

        /// <summary>
        /// 전체 진행률 반환 (0~100%)
        /// </summary>
        public int GetOverallProgress()
        {
            int totalSteps = 4; // 조건검색, 1차분석, 2차분석, 매매계획
            int completedSteps = GetCompletedStepsCount();
            return (completedSteps * 100) / totalSteps;
        }

        #endregion
    }
}