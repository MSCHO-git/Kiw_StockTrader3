using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace StockTrader3_WinForms
{
    /// <summary>
    /// 진행 상황 표시 및 상태 관리를 담당하는 클래스
    /// </summary>
    public class ProgressManager
    {
        #region 생성자 및 초기화

        private readonly MainForm _mainForm;

        // 진행 상황 표시 컨트롤들
        private GroupBox grpDetailedProgress;
        private Label lblDetailStepInfo;
        private Label lblDetailCurrentItem;
        private ProgressBar progressBarDetail;
        private Label lblDetailEstimatedTime;

        // 완료된 작업 추적 변수들
        private bool _isKiwoomConnected = false;
        private int _loadedConditionCount = 0;
        private bool _conditionSearchCompleted = false;
        private bool _technicalAnalysisCompleted = false;
        private bool _newsAnalysisCompleted = false;
        private bool _tradePlanCompleted = false;

        public ProgressManager(MainForm mainForm)
        {
            _mainForm = mainForm ?? throw new ArgumentNullException(nameof(mainForm));
        }

        #endregion

        #region 공개 속성 (MainForm에서 접근용)

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

        #region 진행 상황 표시 컨트롤 생성

        /// <summary>
        /// 상세 진행 상황 표시 컨트롤들을 생성하고 부모 컨트롤에 추가
        /// </summary>
        public void CreateDetailedProgressControls(GroupBox parentGroupBox)
        {
            try
            {
                // 상세 진행 상황 그룹박스 생성
                grpDetailedProgress = new GroupBox();
                grpDetailedProgress.Text = "상세 진행 상황";
                grpDetailedProgress.Location = new Point(500, 25);
                grpDetailedProgress.Size = new Size(600, 110);
                grpDetailedProgress.BackColor = Color.AliceBlue;

                // 단계 정보 라벨
                lblDetailStepInfo = new Label();
                lblDetailStepInfo.AutoSize = true;
                lblDetailStepInfo.Font = new Font("굴림", 9F, FontStyle.Bold);
                lblDetailStepInfo.ForeColor = Color.Blue;
                lblDetailStepInfo.Location = new Point(10, 20);
                lblDetailStepInfo.Text = "분석 대기 중...";

                // 현재 처리 항목 라벨
                lblDetailCurrentItem = new Label();
                lblDetailCurrentItem.AutoSize = true;
                lblDetailCurrentItem.Location = new Point(10, 45);
                lblDetailCurrentItem.Text = "현재 처리: 대기 중...";

                // 상세 진행률 바
                progressBarDetail = new ProgressBar();
                progressBarDetail.Location = new Point(10, 68);
                progressBarDetail.Size = new Size(400, 18);
                progressBarDetail.Style = ProgressBarStyle.Continuous;

                // 예상 시간 라벨
                lblDetailEstimatedTime = new Label();
                lblDetailEstimatedTime.AutoSize = true;
                lblDetailEstimatedTime.ForeColor = Color.Gray;
                lblDetailEstimatedTime.Location = new Point(420, 70);
                lblDetailEstimatedTime.Text = "예상 시간: 계산 중...";

                // 그룹박스에 컨트롤들 추가
                grpDetailedProgress.Controls.Add(lblDetailStepInfo);
                grpDetailedProgress.Controls.Add(lblDetailCurrentItem);
                grpDetailedProgress.Controls.Add(progressBarDetail);
                grpDetailedProgress.Controls.Add(lblDetailEstimatedTime);

                // 부모 컨트롤에 추가
                parentGroupBox.Controls.Add(grpDetailedProgress);

                System.Diagnostics.Debug.WriteLine("진행 상황 컨트롤 생성 완료");
            }
            catch (Exception ex)
            {
                MessageBox.Show("진행 상황 컨트롤 생성 중 오류: " + ex.Message, "오류");
            }
        }

        #endregion

        #region 단계별 아이콘 및 색상

        /// <summary>
        /// 단계명에 따른 아이콘 반환
        /// </summary>
        private string GetStepIcon(string stepName)
        {
            switch (stepName)
            {
                case "조건검색": return "검색";
                case "조건검색 준비": return "준비";
                case "기술적 분석": return "분석";
                case "뉴스 분석": return "뉴스";
                case "매매계획": return "계획";
                case "대기": return "대기";
                case "완료": return "완료";
                case "연결 완료": return "연결";
                case "계좌 정보 조회": return "계좌";
                case "계좌 정보 완료": return "계좌";
                case "조건검색식 로드": return "조건";
                default: return "진행";
            }
        }

        /// <summary>
        /// 단계명에 따른 색상 반환
        /// </summary>
        private Color GetStepColor(string stepName)
        {
            switch (stepName)
            {
                case "조건검색": return Color.Blue;
                case "조건검색 준비": return Color.DarkBlue;
                case "기술적 분석": return Color.Green;
                case "뉴스 분석": return Color.Orange;
                case "매매계획": return Color.Red;
                case "완료": return Color.Purple;
                case "연결 완료": return Color.DarkGreen;
                case "계좌 정보 조회": return Color.DarkCyan;
                case "계좌 정보 완료": return Color.DarkGreen;
                case "조건검색식 로드": return Color.DarkViolet;
                default: return Color.Gray;
            }
        }

        #endregion

        #region 진행 상황 업데이트

        /// <summary>
        /// 상세 진행 상황 정보 업데이트
        /// </summary>
        public void UpdateDetailedProgressInfo(string stepName, int current, int total, string currentItem, TimeSpan estimatedTime)
        {
            // UI 스레드에서 실행되도록 보장
            if (_mainForm.InvokeRequired)
            {
                try
                {
                    _mainForm.Invoke(new Action(() => UpdateDetailedProgressInfo(stepName, current, total, currentItem, estimatedTime)));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Invoke 실패: " + ex.Message);
                }
                return;
            }

            try
            {
                // 컨트롤이 생성되지 않은 경우 무시
                if (lblDetailStepInfo == null || lblDetailCurrentItem == null ||
                    progressBarDetail == null || lblDetailEstimatedTime == null)
                    return;

                string icon = GetStepIcon(stepName);
                Color stepColor = GetStepColor(stepName);

                int percentage = total > 0 ? (current * 100) / total : 0;

                // 단계 정보 업데이트
                if (total > 0)
                {
                    lblDetailStepInfo.Text = string.Format("[{0}] {1}: {2}/{3} 종목 완료 ({4}%)",
                        icon, stepName, current, total, percentage);
                }
                else
                {
                    lblDetailStepInfo.Text = string.Format("[{0}] {1}", icon, stepName);
                }

                // 현재 처리 항목 업데이트
                lblDetailCurrentItem.Text = string.Format("현재 처리: {0}", currentItem);

                // 진행률 바 업데이트
                progressBarDetail.Maximum = 100;
                progressBarDetail.Value = Math.Min(Math.Max(percentage, 0), 100);

                // 예상 시간 업데이트
                UpdateEstimatedTime(estimatedTime);

                // 색상 및 배경 업데이트
                lblDetailStepInfo.ForeColor = stepColor;
                UpdateBackgroundColor(percentage);

                Application.DoEvents();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("UpdateDetailedProgressInfo 오류: " + ex.Message);
            }
        }

        /// <summary>
        /// 예상 시간 표시 업데이트
        /// </summary>
        private void UpdateEstimatedTime(TimeSpan estimatedTime)
        {
            if (estimatedTime.TotalSeconds > 0)
            {
                if (estimatedTime.TotalHours >= 1)
                {
                    lblDetailEstimatedTime.Text = string.Format("예상 시간: {0}시간 {1:D2}분",
                        (int)estimatedTime.TotalHours, estimatedTime.Minutes);
                }
                else if (estimatedTime.TotalMinutes >= 1)
                {
                    lblDetailEstimatedTime.Text = string.Format("예상 시간: {0:D2}분 {1:D2}초",
                        estimatedTime.Minutes, estimatedTime.Seconds);
                }
                else
                {
                    lblDetailEstimatedTime.Text = string.Format("예상 시간: {0}초", estimatedTime.Seconds);
                }
            }
            else
            {
                lblDetailEstimatedTime.Text = "예상 시간: 계산 중...";
            }
        }

        /// <summary>
        /// 진행률에 따른 배경색 업데이트
        /// </summary>
        private void UpdateBackgroundColor(int percentage)
        {
            if (percentage == 100)
            {
                grpDetailedProgress.BackColor = Color.LightGreen;
            }
            else if (percentage > 0)
            {
                grpDetailedProgress.BackColor = Color.LightBlue;
            }
            else
            {
                grpDetailedProgress.BackColor = Color.AliceBlue;
            }
        }

        #endregion

        #region 단계 완료 처리

        /// <summary>
        /// 단계 완료 처리 및 상태 추적 업데이트
        /// </summary>
        public void CompleteStep(string stepName, int totalProcessed)
        {
            UpdateDetailedProgressInfo("완료", totalProcessed, totalProcessed, stepName + " 완료", TimeSpan.Zero);

            // 완료 상태 추적 변수 업데이트
            UpdateCompletionStatus(stepName);

            // 8초간 완료 상태 유지 후 대기 상태로 변경
            System.Threading.Timer timer = null;
            timer = new System.Threading.Timer((obj) =>
            {
                try
                {
                    _mainForm.Invoke(new Action(() =>
                    {
                        UpdateDetailedProgressInfo("대기", 0, 0, "다음 단계 대기 중...", TimeSpan.Zero);
                    }));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("CompleteStep Invoke 실패: " + ex.Message);
                }
                timer?.Dispose();
            }, null, 8000, System.Threading.Timeout.Infinite);
        }

        /// <summary>
        /// 단계명에 따른 완료 상태 업데이트
        /// </summary>
        private void UpdateCompletionStatus(string stepName)
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
        }

        #endregion

        #region 상태바 요약 정보

        /// <summary>
        /// 완료된 작업들의 요약 정보 생성
        /// </summary>
        public string GetStatusBarSummary()
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
                    string timeInfo = $"시간: {DateTime.Now:HH:mm:ss}";
                    return $"{summary} | {timeInfo}";
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("상태바 요약 생성 실패: " + ex.Message);
                return string.Empty;
            }
        }

        #endregion

        #region 초기화

        /// <summary>
        /// 진행 상황 표시 초기화
        /// </summary>
        public void InitializeProgressDisplay()
        {
            UpdateDetailedProgressInfo("대기", 0, 0, "시스템 준비 중...", TimeSpan.Zero);
        }

        #endregion
    }
}