using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AutoTrader_WinForms.Managers;

namespace AutoTrader_WinForms.Forms
{
    public partial class TradingPlanForm : Form
    {
        private List<TradingStock> availableStocks;
        private List<TradingStock> selectedStocks;
        private decimal totalInvestment = 10000000; // 기본 1000만원

        public TradingPlanForm(List<TradingStock> stocks)
        {
            availableStocks = stocks ?? new List<TradingStock>();
            selectedStocks = new List<TradingStock>();

            InitializeComponent();
            InitializeData();
        }

        private void InitializeData()
        {
            // 기본 설정값
            numTotalInvestment.Value = totalInvestment / 10000; // 만원 단위로 표시
            numTargetReturn.Value = 5.0m;
            numMaxDailyLoss.Value = 10.0m;
            numSGradeWeight.Value = 1.5m;
            numAGradeWeight.Value = 1.0m;

            // DataGridView 설정
            SetupDataGridViews();

            // 초기 선택: S등급은 모두, A등급은 상위 점수만
            AutoSelectRecommendedStocks();

            // UI 업데이트
            UpdateStockSelection();
            CalculateInvestmentPlan();
        }

        private void SetupDataGridViews()
        {
            // 전체 종목 그리드 설정
            dgvAvailableStocks.AutoGenerateColumns = false;
            dgvAvailableStocks.Columns.Clear();
            dgvAvailableStocks.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "StockCode",
                DataPropertyName = "StockCode",
                HeaderText = "코드",
                Width = 60
            });
            dgvAvailableStocks.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "StockName",
                DataPropertyName = "StockName",
                HeaderText = "종목명",
                Width = 80
            });
            dgvAvailableStocks.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "FinalGrade",
                DataPropertyName = "FinalGrade",
                HeaderText = "등급",
                Width = 40
            });
            dgvAvailableStocks.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "FinalScore",
                DataPropertyName = "FinalScore",
                HeaderText = "점수",
                Width = 50
            });
            dgvAvailableStocks.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ClosePrice",
                DataPropertyName = "ClosePrice",
                HeaderText = "현재가",
                Width = 70,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" }
            });

            // 선택된 종목 그리드 설정
            dgvSelectedStocks.AutoGenerateColumns = false;
            dgvSelectedStocks.Columns.Clear();
            dgvSelectedStocks.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Priority",
                DataPropertyName = "Priority",
                HeaderText = "순위",
                Width = 40
            });
            dgvSelectedStocks.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "StockCode",
                DataPropertyName = "StockCode",
                HeaderText = "코드",
                Width = 60
            });
            dgvSelectedStocks.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "StockName",
                DataPropertyName = "StockName",
                HeaderText = "종목명",
                Width = 80
            });
            dgvSelectedStocks.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "FinalGrade",
                DataPropertyName = "FinalGrade",
                HeaderText = "등급",
                Width = 40
            });
            dgvSelectedStocks.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "FinalScore",
                DataPropertyName = "FinalScore",
                HeaderText = "점수",
                Width = 50
            });
            dgvSelectedStocks.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "InvestmentAmount",
                DataPropertyName = "InvestmentAmount",
                HeaderText = "투자금액",
                Width = 80,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" }
            });
        }

        private void AutoSelectRecommendedStocks()
        {
            selectedStocks.Clear();

            // S등급: 전체 선택
            var sStocks = availableStocks.Where(s => s.FinalGrade == "S").ToList();
            selectedStocks.AddRange(sStocks);

            // A등급: 상위 점수만 선택 (140점 이상)
            var topAStocks = availableStocks
                .Where(s => s.FinalGrade == "A" && s.FinalScore >= 140)
                .Take(Math.Max(1, sStocks.Count / 2)) // S등급의 절반 정도
                .ToList();
            selectedStocks.AddRange(topAStocks);

            // 우선순위 설정 (점수 순)
            for (int i = 0; i < selectedStocks.Count; i++)
            {
                selectedStocks[i].Priority = i + 1;
                selectedStocks[i].IsSelected = true;
            }

            AddLog($"🎯 자동 선택: S등급 {sStocks.Count}개, A등급 {topAStocks.Count}개 (총 {selectedStocks.Count}개)");
        }

        private void UpdateStockSelection()
        {
            // 전체 종목 리스트 업데이트
            dgvAvailableStocks.DataSource = null;
            dgvAvailableStocks.DataSource = availableStocks.ToList();

            // 선택된 종목 리스트 업데이트  
            dgvSelectedStocks.DataSource = null;
            dgvSelectedStocks.DataSource = selectedStocks.OrderBy(s => s.Priority).ToList();

            // 통계 업데이트
            lblTotalStocks.Text = $"전체 종목: {availableStocks.Count}개";
            lblSelectedStocks.Text = $"선택 종목: {selectedStocks.Count}개";
            lblSGradeSelected.Text = $"S급: {selectedStocks.Count(s => s.FinalGrade == "S")}개";
            lblAGradeSelected.Text = $"A급: {selectedStocks.Count(s => s.FinalGrade == "A")}개";
        }

        private void CalculateInvestmentPlan()
        {
            if (selectedStocks.Count == 0)
            {
                ClearInvestmentPlan();
                return;
            }

            try
            {
                decimal totalInvestment = numTotalInvestment.Value * 10000; // 만원을 원으로 변환
                decimal sGradeWeight = numSGradeWeight.Value;
                decimal aGradeWeight = numAGradeWeight.Value;

                // 총 가중치 계산
                decimal totalWeight = 0;
                foreach (var stock in selectedStocks)
                {
                    if (stock.FinalGrade == "S")
                        totalWeight += sGradeWeight;
                    else if (stock.FinalGrade == "A")
                        totalWeight += aGradeWeight;
                    else
                        totalWeight += 1.0m;
                }

                // 기본 투자 단위 계산
                decimal baseAmount = totalInvestment / totalWeight;

                // 각 종목별 투자 금액 계산
                decimal totalAllocated = 0;
                foreach (var stock in selectedStocks)
                {
                    decimal weight = 1.0m;
                    if (stock.FinalGrade == "S")
                        weight = sGradeWeight;
                    else if (stock.FinalGrade == "A")
                        weight = aGradeWeight;

                    stock.InvestmentAmount = Math.Round(baseAmount * weight, -4); // 만원 단위로 반올림
                    totalAllocated += stock.InvestmentAmount;
                }

                // 통계 업데이트
                lblTotalInvestment.Text = $"총 투자금: {totalInvestment:N0}원";
                lblAllocatedAmount.Text = $"배분 완료: {totalAllocated:N0}원";
                lblRemainingAmount.Text = $"잔여 자금: {totalInvestment - totalAllocated:N0}원";

                // 예상 수익 계산
                decimal expectedProfit = selectedStocks.Sum(s => s.InvestmentAmount * (decimal)s.ExpectedReturn / 100);
                decimal expectedReturn = totalInvestment > 0 ? expectedProfit / totalInvestment * 100 : 0;

                lblExpectedProfit.Text = $"예상 수익: {expectedProfit:N0}원";
                lblExpectedReturn.Text = $"예상 수익률: {expectedReturn:F2}%";

                // 그리드 갱신
                UpdateStockSelection();

                AddLog($"💰 투자 계획 업데이트: {selectedStocks.Count}종목, 총 {totalAllocated:N0}원");
            }
            catch (Exception ex)
            {
                AddLog($"❌ 투자 계획 계산 오류: {ex.Message}");
            }
        }

        private void ClearInvestmentPlan()
        {
            lblTotalInvestment.Text = "총 투자금: 0원";
            lblAllocatedAmount.Text = "배분 완료: 0원";
            lblRemainingAmount.Text = "잔여 자금: 0원";
            lblExpectedProfit.Text = "예상 수익: 0원";
            lblExpectedReturn.Text = "예상 수익률: 0%";
        }

        #region 이벤트 핸들러

        private void btnAutoSelect_Click(object sender, EventArgs e)
        {
            AutoSelectRecommendedStocks();
            UpdateStockSelection();
            CalculateInvestmentPlan();
        }

        private void btnClearSelection_Click(object sender, EventArgs e)
        {
            selectedStocks.Clear();
            foreach (var stock in availableStocks)
            {
                stock.IsSelected = false;
                stock.Priority = 0;
                stock.InvestmentAmount = 0;
            }

            UpdateStockSelection();
            CalculateInvestmentPlan();
            AddLog("🗑️ 모든 선택 해제됨");
        }

        private void dgvAvailableStocks_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var stock = availableStocks[e.RowIndex];
            if (!stock.IsSelected)
            {
                stock.IsSelected = true;
                stock.Priority = selectedStocks.Count + 1;
                selectedStocks.Add(stock);

                UpdateStockSelection();
                CalculateInvestmentPlan();
                AddLog($"➕ 종목 추가: {stock.StockName} ({stock.FinalGrade}등급)");
            }
        }

        private void dgvSelectedStocks_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var stock = selectedStocks[e.RowIndex];
            stock.IsSelected = false;
            stock.Priority = 0;
            stock.InvestmentAmount = 0;
            selectedStocks.Remove(stock);

            // 우선순위 재정렬
            for (int i = 0; i < selectedStocks.Count; i++)
            {
                selectedStocks[i].Priority = i + 1;
            }

            UpdateStockSelection();
            CalculateInvestmentPlan();
            AddLog($"➖ 종목 제거: {stock.StockName}");
        }

        private void numTotalInvestment_ValueChanged(object sender, EventArgs e)
        {
            CalculateInvestmentPlan();
        }

        private void numSGradeWeight_ValueChanged(object sender, EventArgs e)
        {
            CalculateInvestmentPlan();
        }

        private void numAGradeWeight_ValueChanged(object sender, EventArgs e)
        {
            CalculateInvestmentPlan();
        }

        private void btnCreatePlan_Click(object sender, EventArgs e)
        {
            if (selectedStocks.Count == 0)
            {
                MessageBox.Show("매매할 종목을 선택해주세요.", "종목 선택 필요",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // 매매 계획 저장
                SaveTradingPlan();

                AddLog("✅ 매매 계획이 저장되었습니다!");
                MessageBox.Show($"매매 계획이 성공적으로 생성되었습니다!\n\n" +
                              $"선택 종목: {selectedStocks.Count}개\n" +
                              $"총 투자금: {numTotalInvestment.Value:N0}만원\n" +
                              $"예상 수익률: {lblExpectedReturn.Text}",
                              "매매 계획 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                AddLog($"❌ 매매 계획 저장 오류: {ex.Message}");
                MessageBox.Show($"매매 계획 저장 중 오류가 발생했습니다:\n{ex.Message}",
                              "저장 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        #endregion

        #region 데이터 저장

        private void SaveTradingPlan()
        {
            // TODO: DatabaseManager에 TradingPlan 저장 메서드 추가
            // var planId = DatabaseManager.SaveTradingPlan(CreateTradingPlanData());

            AddLog("🚧 데이터베이스 저장 기능은 다음 단계에서 구현 예정");
        }

        #endregion

        private void AddLog(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            txtLog.AppendText($"[{timestamp}] {message}\r\n");
            txtLog.ScrollToCaret();
        }
    }
}