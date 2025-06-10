namespace AutoTrader_WinForms.Forms
{
    partial class TradingPlanForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.grpSettings = new System.Windows.Forms.GroupBox();
            this.lblTotalInvestmentLabel = new System.Windows.Forms.Label();
            this.numTotalInvestment = new System.Windows.Forms.NumericUpDown();
            this.lblTargetReturnLabel = new System.Windows.Forms.Label();
            this.numTargetReturn = new System.Windows.Forms.NumericUpDown();
            this.lblMaxDailyLossLabel = new System.Windows.Forms.Label();
            this.numMaxDailyLoss = new System.Windows.Forms.NumericUpDown();
            this.lblSGradeWeightLabel = new System.Windows.Forms.Label();
            this.numSGradeWeight = new System.Windows.Forms.NumericUpDown();
            this.lblAGradeWeightLabel = new System.Windows.Forms.Label();
            this.numAGradeWeight = new System.Windows.Forms.NumericUpDown();

            this.grpAvailableStocks = new System.Windows.Forms.GroupBox();
            this.dgvAvailableStocks = new System.Windows.Forms.DataGridView();
            this.btnAutoSelect = new System.Windows.Forms.Button();
            this.btnClearSelection = new System.Windows.Forms.Button();
            this.lblTotalStocks = new System.Windows.Forms.Label();

            this.grpSelectedStocks = new System.Windows.Forms.GroupBox();
            this.dgvSelectedStocks = new System.Windows.Forms.DataGridView();
            this.lblSelectedStocks = new System.Windows.Forms.Label();
            this.lblSGradeSelected = new System.Windows.Forms.Label();
            this.lblAGradeSelected = new System.Windows.Forms.Label();

            this.grpInvestmentPlan = new System.Windows.Forms.GroupBox();
            this.lblTotalInvestment = new System.Windows.Forms.Label();
            this.lblAllocatedAmount = new System.Windows.Forms.Label();
            this.lblRemainingAmount = new System.Windows.Forms.Label();
            this.lblExpectedProfit = new System.Windows.Forms.Label();
            this.lblExpectedReturn = new System.Windows.Forms.Label();

            this.grpLog = new System.Windows.Forms.GroupBox();
            this.txtLog = new System.Windows.Forms.TextBox();

            this.btnCreatePlan = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();

            ((System.ComponentModel.ISupportInitialize)(this.numTotalInvestment)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTargetReturn)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxDailyLoss)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSGradeWeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numAGradeWeight)).BeginInit();
            this.grpSettings.SuspendLayout();
            this.grpAvailableStocks.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAvailableStocks)).BeginInit();
            this.grpSelectedStocks.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSelectedStocks)).BeginInit();
            this.grpInvestmentPlan.SuspendLayout();
            this.grpLog.SuspendLayout();
            this.SuspendLayout();

            // 
            // TradingPlanForm
            // 
            this.Text = "매매 계획 생성";
            this.Size = new System.Drawing.Size(1200, 800);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.MinimumSize = new System.Drawing.Size(1000, 600);

            // 
            // grpSettings (투자 설정)
            // 
            this.grpSettings.Text = "투자 설정";
            this.grpSettings.Location = new System.Drawing.Point(10, 10);
            this.grpSettings.Size = new System.Drawing.Size(1160, 80);
            this.grpSettings.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.grpSettings.Controls.Add(this.lblTotalInvestmentLabel);
            this.grpSettings.Controls.Add(this.numTotalInvestment);
            this.grpSettings.Controls.Add(this.lblTargetReturnLabel);
            this.grpSettings.Controls.Add(this.numTargetReturn);
            this.grpSettings.Controls.Add(this.lblMaxDailyLossLabel);
            this.grpSettings.Controls.Add(this.numMaxDailyLoss);
            this.grpSettings.Controls.Add(this.lblSGradeWeightLabel);
            this.grpSettings.Controls.Add(this.numSGradeWeight);
            this.grpSettings.Controls.Add(this.lblAGradeWeightLabel);
            this.grpSettings.Controls.Add(this.numAGradeWeight);

            // 투자 설정 컨트롤들
            this.lblTotalInvestmentLabel.Text = "총 투자금(만원)";
            this.lblTotalInvestmentLabel.Location = new System.Drawing.Point(10, 25);
            this.lblTotalInvestmentLabel.Size = new System.Drawing.Size(80, 20);

            this.numTotalInvestment.Location = new System.Drawing.Point(10, 45);
            this.numTotalInvestment.Size = new System.Drawing.Size(80, 20);
            this.numTotalInvestment.Minimum = 100;
            this.numTotalInvestment.Maximum = 100000;
            this.numTotalInvestment.Value = 1000;
            this.numTotalInvestment.Increment = 100;
            this.numTotalInvestment.ValueChanged += new System.EventHandler(this.numTotalInvestment_ValueChanged);

            this.lblTargetReturnLabel.Text = "목표수익률(%)";
            this.lblTargetReturnLabel.Location = new System.Drawing.Point(110, 25);
            this.lblTargetReturnLabel.Size = new System.Drawing.Size(80, 20);

            this.numTargetReturn.Location = new System.Drawing.Point(110, 45);
            this.numTargetReturn.Size = new System.Drawing.Size(80, 20);
            this.numTargetReturn.Minimum = 1;
            this.numTargetReturn.Maximum = 50;
            this.numTargetReturn.Value = 5;
            this.numTargetReturn.DecimalPlaces = 1;

            this.lblMaxDailyLossLabel.Text = "최대손실(%)";
            this.lblMaxDailyLossLabel.Location = new System.Drawing.Point(210, 25);
            this.lblMaxDailyLossLabel.Size = new System.Drawing.Size(80, 20);

            this.numMaxDailyLoss.Location = new System.Drawing.Point(210, 45);
            this.numMaxDailyLoss.Size = new System.Drawing.Size(80, 20);
            this.numMaxDailyLoss.Minimum = 1;
            this.numMaxDailyLoss.Maximum = 50;
            this.numMaxDailyLoss.Value = 10;
            this.numMaxDailyLoss.DecimalPlaces = 1;

            this.lblSGradeWeightLabel.Text = "S등급 가중치";
            this.lblSGradeWeightLabel.Location = new System.Drawing.Point(310, 25);
            this.lblSGradeWeightLabel.Size = new System.Drawing.Size(80, 20);

            this.numSGradeWeight.Location = new System.Drawing.Point(310, 45);
            this.numSGradeWeight.Size = new System.Drawing.Size(80, 20);
            this.numSGradeWeight.Minimum = 0.5m;
            this.numSGradeWeight.Maximum = 3.0m;
            this.numSGradeWeight.Value = 1.5m;
            this.numSGradeWeight.DecimalPlaces = 1;
            this.numSGradeWeight.Increment = 0.1m;
            this.numSGradeWeight.ValueChanged += new System.EventHandler(this.numSGradeWeight_ValueChanged);

            this.lblAGradeWeightLabel.Text = "A등급 가중치";
            this.lblAGradeWeightLabel.Location = new System.Drawing.Point(410, 25);
            this.lblAGradeWeightLabel.Size = new System.Drawing.Size(80, 20);

            this.numAGradeWeight.Location = new System.Drawing.Point(410, 45);
            this.numAGradeWeight.Size = new System.Drawing.Size(80, 20);
            this.numAGradeWeight.Minimum = 0.5m;
            this.numAGradeWeight.Maximum = 2.0m;
            this.numAGradeWeight.Value = 1.0m;
            this.numAGradeWeight.DecimalPlaces = 1;
            this.numAGradeWeight.Increment = 0.1m;
            this.numAGradeWeight.ValueChanged += new System.EventHandler(this.numAGradeWeight_ValueChanged);

            // 
            // grpAvailableStocks (전체 종목)
            // 
            this.grpAvailableStocks.Text = "전체 종목 (더블클릭으로 선택)";
            this.grpAvailableStocks.Location = new System.Drawing.Point(10, 100);
            this.grpAvailableStocks.Size = new System.Drawing.Size(570, 300);
            this.grpAvailableStocks.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Bottom;
            this.grpAvailableStocks.Controls.Add(this.dgvAvailableStocks);
            this.grpAvailableStocks.Controls.Add(this.btnAutoSelect);
            this.grpAvailableStocks.Controls.Add(this.btnClearSelection);
            this.grpAvailableStocks.Controls.Add(this.lblTotalStocks);

            this.dgvAvailableStocks.Location = new System.Drawing.Point(10, 50);
            this.dgvAvailableStocks.Size = new System.Drawing.Size(550, 240);
            this.dgvAvailableStocks.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            this.dgvAvailableStocks.ReadOnly = true;
            this.dgvAvailableStocks.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvAvailableStocks.AllowUserToAddRows = false;
            this.dgvAvailableStocks.RowHeadersVisible = false;
            this.dgvAvailableStocks.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvAvailableStocks_CellDoubleClick);

            this.btnAutoSelect.Text = "자동 선택";
            this.btnAutoSelect.Location = new System.Drawing.Point(10, 20);
            this.btnAutoSelect.Size = new System.Drawing.Size(80, 25);
            this.btnAutoSelect.Click += new System.EventHandler(this.btnAutoSelect_Click);

            this.btnClearSelection.Text = "전체 해제";
            this.btnClearSelection.Location = new System.Drawing.Point(100, 20);
            this.btnClearSelection.Size = new System.Drawing.Size(80, 25);
            this.btnClearSelection.Click += new System.EventHandler(this.btnClearSelection_Click);

            this.lblTotalStocks.Text = "전체 종목: 0개";
            this.lblTotalStocks.Location = new System.Drawing.Point(200, 25);
            this.lblTotalStocks.Size = new System.Drawing.Size(100, 20);

            // 
            // grpSelectedStocks (선택된 종목)
            // 
            this.grpSelectedStocks.Text = "선택된 종목 (더블클릭으로 제거)";
            this.grpSelectedStocks.Location = new System.Drawing.Point(590, 100);
            this.grpSelectedStocks.Size = new System.Drawing.Size(580, 300);
            this.grpSelectedStocks.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom;
            this.grpSelectedStocks.Controls.Add(this.dgvSelectedStocks);
            this.grpSelectedStocks.Controls.Add(this.lblSelectedStocks);
            this.grpSelectedStocks.Controls.Add(this.lblSGradeSelected);
            this.grpSelectedStocks.Controls.Add(this.lblAGradeSelected);

            this.dgvSelectedStocks.Location = new System.Drawing.Point(10, 50);
            this.dgvSelectedStocks.Size = new System.Drawing.Size(560, 240);
            this.dgvSelectedStocks.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            this.dgvSelectedStocks.ReadOnly = true;
            this.dgvSelectedStocks.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvSelectedStocks.AllowUserToAddRows = false;
            this.dgvSelectedStocks.RowHeadersVisible = false;
            this.dgvSelectedStocks.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvSelectedStocks_CellDoubleClick);

            this.lblSelectedStocks.Text = "선택 종목: 0개";
            this.lblSelectedStocks.Location = new System.Drawing.Point(10, 25);
            this.lblSelectedStocks.Size = new System.Drawing.Size(100, 20);

            this.lblSGradeSelected.Text = "S급: 0개";
            this.lblSGradeSelected.Location = new System.Drawing.Point(120, 25);
            this.lblSGradeSelected.Size = new System.Drawing.Size(60, 20);
            this.lblSGradeSelected.ForeColor = System.Drawing.Color.Red;
            this.lblSGradeSelected.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);

            this.lblAGradeSelected.Text = "A급: 0개";
            this.lblAGradeSelected.Location = new System.Drawing.Point(190, 25);
            this.lblAGradeSelected.Size = new System.Drawing.Size(60, 20);
            this.lblAGradeSelected.ForeColor = System.Drawing.Color.Blue;
            this.lblAGradeSelected.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);

            // 
            // grpInvestmentPlan (투자 계획 요약)
            // 
            this.grpInvestmentPlan.Text = "투자 계획 요약";
            this.grpInvestmentPlan.Location = new System.Drawing.Point(10, 410);
            this.grpInvestmentPlan.Size = new System.Drawing.Size(570, 100);
            this.grpInvestmentPlan.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            this.grpInvestmentPlan.Controls.Add(this.lblTotalInvestment);
            this.grpInvestmentPlan.Controls.Add(this.lblAllocatedAmount);
            this.grpInvestmentPlan.Controls.Add(this.lblRemainingAmount);
            this.grpInvestmentPlan.Controls.Add(this.lblExpectedProfit);
            this.grpInvestmentPlan.Controls.Add(this.lblExpectedReturn);

            this.lblTotalInvestment.Text = "총 투자금: 0원";
            this.lblTotalInvestment.Location = new System.Drawing.Point(10, 25);
            this.lblTotalInvestment.Size = new System.Drawing.Size(150, 20);

            this.lblAllocatedAmount.Text = "배분 완료: 0원";
            this.lblAllocatedAmount.Location = new System.Drawing.Point(170, 25);
            this.lblAllocatedAmount.Size = new System.Drawing.Size(150, 20);

            this.lblRemainingAmount.Text = "잔여 자금: 0원";
            this.lblRemainingAmount.Location = new System.Drawing.Point(330, 25);
            this.lblRemainingAmount.Size = new System.Drawing.Size(150, 20);

            this.lblExpectedProfit.Text = "예상 수익: 0원";
            this.lblExpectedProfit.Location = new System.Drawing.Point(10, 50);
            this.lblExpectedProfit.Size = new System.Drawing.Size(150, 20);

            this.lblExpectedReturn.Text = "예상 수익률: 0%";
            this.lblExpectedReturn.Location = new System.Drawing.Point(170, 50);
            this.lblExpectedReturn.Size = new System.Drawing.Size(150, 20);
            this.lblExpectedReturn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.lblExpectedReturn.ForeColor = System.Drawing.Color.Green;

            // 
            // grpLog (로그)
            // 
            this.grpLog.Text = "로그";
            this.grpLog.Location = new System.Drawing.Point(590, 410);
            this.grpLog.Size = new System.Drawing.Size(580, 100);
            this.grpLog.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            this.grpLog.Controls.Add(this.txtLog);

            this.txtLog.Location = new System.Drawing.Point(10, 20);
            this.txtLog.Size = new System.Drawing.Size(560, 70);
            this.txtLog.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            this.txtLog.Multiline = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.ReadOnly = true;
            this.txtLog.BackColor = System.Drawing.Color.Black;
            this.txtLog.ForeColor = System.Drawing.Color.Lime;
            this.txtLog.Font = new System.Drawing.Font("Consolas", 8);

            // 
            // 하단 버튼들
            // 
            this.btnCreatePlan.Text = "매매 계획 생성";
            this.btnCreatePlan.Location = new System.Drawing.Point(930, 720);
            this.btnCreatePlan.Size = new System.Drawing.Size(120, 35);
            this.btnCreatePlan.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            this.btnCreatePlan.UseVisualStyleBackColor = true;
            this.btnCreatePlan.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.btnCreatePlan.BackColor = System.Drawing.Color.LightGreen;
            this.btnCreatePlan.Click += new System.EventHandler(this.btnCreatePlan_Click);

            this.btnCancel.Text = "취소";
            this.btnCancel.Location = new System.Drawing.Point(1060, 720);
            this.btnCancel.Size = new System.Drawing.Size(80, 35);
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);

            // 
            // 폼에 컨트롤 추가
            // 
            this.Controls.Add(this.grpSettings);
            this.Controls.Add(this.grpAvailableStocks);
            this.Controls.Add(this.grpSelectedStocks);
            this.Controls.Add(this.grpInvestmentPlan);
            this.Controls.Add(this.grpLog);
            this.Controls.Add(this.btnCreatePlan);
            this.Controls.Add(this.btnCancel);

            ((System.ComponentModel.ISupportInitialize)(this.numTotalInvestment)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTargetReturn)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxDailyLoss)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSGradeWeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numAGradeWeight)).EndInit();
            this.grpSettings.ResumeLayout(false);
            this.grpAvailableStocks.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvAvailableStocks)).EndInit();
            this.grpSelectedStocks.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvSelectedStocks)).EndInit();
            this.grpInvestmentPlan.ResumeLayout(false);
            this.grpLog.ResumeLayout(false);
            this.grpLog.PerformLayout();
            this.ResumeLayout(false);
        }

        #region 컨트롤 변수들

        private System.Windows.Forms.GroupBox grpSettings;
        private System.Windows.Forms.Label lblTotalInvestmentLabel;
        private System.Windows.Forms.NumericUpDown numTotalInvestment;
        private System.Windows.Forms.Label lblTargetReturnLabel;
        private System.Windows.Forms.NumericUpDown numTargetReturn;
        private System.Windows.Forms.Label lblMaxDailyLossLabel;
        private System.Windows.Forms.NumericUpDown numMaxDailyLoss;
        private System.Windows.Forms.Label lblSGradeWeightLabel;
        private System.Windows.Forms.NumericUpDown numSGradeWeight;
        private System.Windows.Forms.Label lblAGradeWeightLabel;
        private System.Windows.Forms.NumericUpDown numAGradeWeight;

        private System.Windows.Forms.GroupBox grpAvailableStocks;
        private System.Windows.Forms.DataGridView dgvAvailableStocks;
        private System.Windows.Forms.Button btnAutoSelect;
        private System.Windows.Forms.Button btnClearSelection;
        private System.Windows.Forms.Label lblTotalStocks;

        private System.Windows.Forms.GroupBox grpSelectedStocks;
        private System.Windows.Forms.DataGridView dgvSelectedStocks;
        private System.Windows.Forms.Label lblSelectedStocks;
        private System.Windows.Forms.Label lblSGradeSelected;
        private System.Windows.Forms.Label lblAGradeSelected;

        private System.Windows.Forms.GroupBox grpInvestmentPlan;
        private System.Windows.Forms.Label lblTotalInvestment;
        private System.Windows.Forms.Label lblAllocatedAmount;
        private System.Windows.Forms.Label lblRemainingAmount;
        private System.Windows.Forms.Label lblExpectedProfit;
        private System.Windows.Forms.Label lblExpectedReturn;

        private System.Windows.Forms.GroupBox grpLog;
        private System.Windows.Forms.TextBox txtLog;

        private System.Windows.Forms.Button btnCreatePlan;
        private System.Windows.Forms.Button btnCancel;

        #endregion
    }
}