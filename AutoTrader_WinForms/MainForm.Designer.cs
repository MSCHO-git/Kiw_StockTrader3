namespace AutoTrader_WinForms
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.grpControl = new System.Windows.Forms.GroupBox();
            this.pnlSystemStatus = new System.Windows.Forms.Panel();
            this.lblSystemStatus = new System.Windows.Forms.Label();
            this.lblSystemStatusTitle = new System.Windows.Forms.Label();
            this.pnlMarketStatus = new System.Windows.Forms.Panel();
            this.lblMarketStatus = new System.Windows.Forms.Label();
            this.lblMarketStatusTitle = new System.Windows.Forms.Label();
            this.pnlDailyLoss = new System.Windows.Forms.Panel();
            this.lblDailyLossStatus = new System.Windows.Forms.Label();
            this.lblDailyLossTitle = new System.Windows.Forms.Label();
            this.cmbAccount = new System.Windows.Forms.ComboBox();
            this.lblAccountLabel = new System.Windows.Forms.Label();
            this.lblAccountInfo = new System.Windows.Forms.Label();
            this.lblKiwoomStatus = new System.Windows.Forms.Label();
            this.lblLastUpdate = new System.Windows.Forms.Label();
            this.lblConnectionStatus = new System.Windows.Forms.Label();
            this.btnStopTrading = new System.Windows.Forms.Button();
            this.btnStartTrading = new System.Windows.Forms.Button();
            this.btnLoadData = new System.Windows.Forms.Button();
            this.grpStatistics = new System.Windows.Forms.GroupBox();
            this.lblAnalysisDate = new System.Windows.Forms.Label();
            this.lblTotalCount = new System.Windows.Forms.Label();
            this.lblSGrade = new System.Windows.Forms.Label();
            this.lblAGrade = new System.Windows.Forms.Label();
            this.lblTradable = new System.Windows.Forms.Label();
            this.lblAvgScore = new System.Windows.Forms.Label();
            this.grpFilter = new System.Windows.Forms.GroupBox();
            this.rbAll = new System.Windows.Forms.RadioButton();
            this.rbSGrade = new System.Windows.Forms.RadioButton();
            this.rbAGrade = new System.Windows.Forms.RadioButton();
            this.rbTopGrades = new System.Windows.Forms.RadioButton();
            this.btnApplyFilter = new System.Windows.Forms.Button();
            this.dgvStocks = new System.Windows.Forms.DataGridView();
            this.grpMonitoring = new System.Windows.Forms.GroupBox();
            this.lblMonitoringTitle = new System.Windows.Forms.Label();
            this.lblTotalInvestment = new System.Windows.Forms.Label();
            this.lblCurrentProfit = new System.Windows.Forms.Label();
            this.lblReturnRate = new System.Windows.Forms.Label();
            this.lblProgress = new System.Windows.Forms.Label();
            this.dgvMonitoring = new System.Windows.Forms.DataGridView();
            this.grpLog = new System.Windows.Forms.GroupBox();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.grpControl.SuspendLayout();
            this.pnlSystemStatus.SuspendLayout();
            this.pnlMarketStatus.SuspendLayout();
            this.pnlDailyLoss.SuspendLayout();
            this.grpStatistics.SuspendLayout();
            this.grpFilter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvStocks)).BeginInit();
            this.grpMonitoring.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMonitoring)).BeginInit();
            this.grpLog.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpControl
            // 
            this.grpControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpControl.Controls.Add(this.pnlSystemStatus);
            this.grpControl.Controls.Add(this.lblSystemStatusTitle);
            this.grpControl.Controls.Add(this.pnlMarketStatus);
            this.grpControl.Controls.Add(this.lblMarketStatusTitle);
            this.grpControl.Controls.Add(this.pnlDailyLoss);
            this.grpControl.Controls.Add(this.lblDailyLossTitle);
            this.grpControl.Controls.Add(this.cmbAccount);
            this.grpControl.Controls.Add(this.lblAccountLabel);
            this.grpControl.Controls.Add(this.lblAccountInfo);
            this.grpControl.Controls.Add(this.lblKiwoomStatus);
            this.grpControl.Controls.Add(this.lblLastUpdate);
            this.grpControl.Controls.Add(this.lblConnectionStatus);
            this.grpControl.Controls.Add(this.btnStopTrading);
            this.grpControl.Controls.Add(this.btnStartTrading);
            this.grpControl.Controls.Add(this.btnLoadData);
            this.grpControl.Location = new System.Drawing.Point(19, 20);
            this.grpControl.Margin = new System.Windows.Forms.Padding(6);
            this.grpControl.Name = "grpControl";
            this.grpControl.Padding = new System.Windows.Forms.Padding(6);
            this.grpControl.Size = new System.Drawing.Size(1783, 200);
            this.grpControl.TabIndex = 0;
            this.grpControl.TabStop = false;
            this.grpControl.Text = "매매 제어 및 핵심 상태 보드";
            // 
            // pnlSystemStatus
            // 
            this.pnlSystemStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.pnlSystemStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlSystemStatus.Controls.Add(this.lblSystemStatus);
            this.pnlSystemStatus.Location = new System.Drawing.Point(1020, 55);
            this.pnlSystemStatus.Name = "pnlSystemStatus";
            this.pnlSystemStatus.Size = new System.Drawing.Size(200, 50);
            this.pnlSystemStatus.TabIndex = 15;
            // 
            // lblSystemStatus
            // 
            this.lblSystemStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSystemStatus.Font = new System.Drawing.Font("굴림", 11F, System.Drawing.FontStyle.Bold);
            this.lblSystemStatus.Location = new System.Drawing.Point(0, 0);
            this.lblSystemStatus.Name = "lblSystemStatus";
            this.lblSystemStatus.Size = new System.Drawing.Size(198, 48);
            this.lblSystemStatus.TabIndex = 0;
            this.lblSystemStatus.Text = "매매 대기";
            this.lblSystemStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblSystemStatusTitle
            // 
            this.lblSystemStatusTitle.AutoSize = true;
            this.lblSystemStatusTitle.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblSystemStatusTitle.Location = new System.Drawing.Point(1016, 25);
            this.lblSystemStatusTitle.Name = "lblSystemStatusTitle";
            this.lblSystemStatusTitle.Size = new System.Drawing.Size(173, 24);
            this.lblSystemStatusTitle.TabIndex = 14;
            this.lblSystemStatusTitle.Text = "현재 시스템 상태";
            // 
            // pnlMarketStatus
            // 
            this.pnlMarketStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(235)))), ((int)(((byte)(255)))));
            this.pnlMarketStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlMarketStatus.Controls.Add(this.lblMarketStatus);
            this.pnlMarketStatus.Location = new System.Drawing.Point(800, 55);
            this.pnlMarketStatus.Name = "pnlMarketStatus";
            this.pnlMarketStatus.Size = new System.Drawing.Size(200, 50);
            this.pnlMarketStatus.TabIndex = 13;
            // 
            // lblMarketStatus
            // 
            this.lblMarketStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMarketStatus.Font = new System.Drawing.Font("굴림", 11F, System.Drawing.FontStyle.Bold);
            this.lblMarketStatus.Location = new System.Drawing.Point(0, 0);
            this.lblMarketStatus.Name = "lblMarketStatus";
            this.lblMarketStatus.Size = new System.Drawing.Size(198, 48);
            this.lblMarketStatus.TabIndex = 0;
            this.lblMarketStatus.Text = "확인 중...";
            this.lblMarketStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblMarketStatusTitle
            // 
            this.lblMarketStatusTitle.AutoSize = true;
            this.lblMarketStatusTitle.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblMarketStatusTitle.Location = new System.Drawing.Point(796, 25);
            this.lblMarketStatusTitle.Name = "lblMarketStatusTitle";
            this.lblMarketStatusTitle.Size = new System.Drawing.Size(103, 24);
            this.lblMarketStatusTitle.TabIndex = 12;
            this.lblMarketStatusTitle.Text = "시장 상태";
            // 
            // pnlDailyLoss
            // 
            this.pnlDailyLoss.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(255)))), ((int)(((byte)(220)))));
            this.pnlDailyLoss.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlDailyLoss.Controls.Add(this.lblDailyLossStatus);
            this.pnlDailyLoss.Location = new System.Drawing.Point(500, 55);
            this.pnlDailyLoss.Name = "pnlDailyLoss";
            this.pnlDailyLoss.Size = new System.Drawing.Size(280, 50);
            this.pnlDailyLoss.TabIndex = 11;
            // 
            // lblDailyLossStatus
            // 
            this.lblDailyLossStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDailyLossStatus.Font = new System.Drawing.Font("굴림", 11F, System.Drawing.FontStyle.Bold);
            this.lblDailyLossStatus.Location = new System.Drawing.Point(0, 0);
            this.lblDailyLossStatus.Name = "lblDailyLossStatus";
            this.lblDailyLossStatus.Size = new System.Drawing.Size(278, 48);
            this.lblDailyLossStatus.TabIndex = 0;
            this.lblDailyLossStatus.Text = "0 / -600,000 원";
            this.lblDailyLossStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblDailyLossTitle
            // 
            this.lblDailyLossTitle.AutoSize = true;
            this.lblDailyLossTitle.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblDailyLossTitle.Location = new System.Drawing.Point(496, 25);
            this.lblDailyLossTitle.Name = "lblDailyLossTitle";
            this.lblDailyLossTitle.Size = new System.Drawing.Size(147, 24);
            this.lblDailyLossTitle.TabIndex = 10;
            this.lblDailyLossTitle.Text = "일일 손실 한도";
            // 
            // cmbAccount
            // 
            this.cmbAccount.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbAccount.Enabled = false;
            this.cmbAccount.FormattingEnabled = true;
            this.cmbAccount.Location = new System.Drawing.Point(1060, 137);
            this.cmbAccount.Margin = new System.Windows.Forms.Padding(6);
            this.cmbAccount.Name = "cmbAccount";
            this.cmbAccount.Size = new System.Drawing.Size(250, 32);
            this.cmbAccount.TabIndex = 9;
            this.cmbAccount.SelectedIndexChanged += new System.EventHandler(this.cmbAccount_SelectedIndexChanged);
            // 
            // lblAccountLabel
            // 
            this.lblAccountLabel.Location = new System.Drawing.Point(950, 140);
            this.lblAccountLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblAccountLabel.Name = "lblAccountLabel";
            this.lblAccountLabel.Size = new System.Drawing.Size(100, 30);
            this.lblAccountLabel.TabIndex = 8;
            this.lblAccountLabel.Text = "계좌 선택:";
            this.lblAccountLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblAccountInfo
            // 
            this.lblAccountInfo.ForeColor = System.Drawing.Color.Gray;
            this.lblAccountInfo.Location = new System.Drawing.Point(540, 140);
            this.lblAccountInfo.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblAccountInfo.Name = "lblAccountInfo";
            this.lblAccountInfo.Size = new System.Drawing.Size(400, 30);
            this.lblAccountInfo.TabIndex = 7;
            this.lblAccountInfo.Text = "계좌: - | 예수금: -";
            // 
            // lblKiwoomStatus
            // 
            this.lblKiwoomStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblKiwoomStatus.ForeColor = System.Drawing.Color.Red;
            this.lblKiwoomStatus.Location = new System.Drawing.Point(19, 140);
            this.lblKiwoomStatus.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblKiwoomStatus.Name = "lblKiwoomStatus";
            this.lblKiwoomStatus.Size = new System.Drawing.Size(500, 30);
            this.lblKiwoomStatus.TabIndex = 6;
            this.lblKiwoomStatus.Text = "키움 연결: ❌ 미연결 (매매시작 시 자동 연결)";
            // 
            // lblLastUpdate
            // 
            this.lblLastUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLastUpdate.ForeColor = System.Drawing.Color.Gray;
            this.lblLastUpdate.Location = new System.Drawing.Point(1425, 70);
            this.lblLastUpdate.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblLastUpdate.Name = "lblLastUpdate";
            this.lblLastUpdate.Size = new System.Drawing.Size(346, 30);
            this.lblLastUpdate.TabIndex = 5;
            this.lblLastUpdate.Text = "마지막 업데이트: -";
            this.lblLastUpdate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblConnectionStatus
            // 
            this.lblConnectionStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblConnectionStatus.ForeColor = System.Drawing.Color.Gray;
            this.lblConnectionStatus.Location = new System.Drawing.Point(1425, 30);
            this.lblConnectionStatus.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblConnectionStatus.Name = "lblConnectionStatus";
            this.lblConnectionStatus.Size = new System.Drawing.Size(346, 30);
            this.lblConnectionStatus.TabIndex = 4;
            this.lblConnectionStatus.Text = "DB 연결: 확인 중...";
            this.lblConnectionStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnStopTrading
            // 
            this.btnStopTrading.BackColor = System.Drawing.Color.LightCoral;
            this.btnStopTrading.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.btnStopTrading.Location = new System.Drawing.Point(240, 50);
            this.btnStopTrading.Margin = new System.Windows.Forms.Padding(6);
            this.btnStopTrading.Name = "btnStopTrading";
            this.btnStopTrading.Size = new System.Drawing.Size(200, 70);
            this.btnStopTrading.TabIndex = 3;
            this.btnStopTrading.Text = "🛑 매매중단";
            this.btnStopTrading.UseVisualStyleBackColor = false;
            this.btnStopTrading.Visible = false;
            this.btnStopTrading.Click += new System.EventHandler(this.btnStopTrading_Click);
            // 
            // btnStartTrading
            // 
            this.btnStartTrading.BackColor = System.Drawing.Color.LightGreen;
            this.btnStartTrading.Enabled = false;
            this.btnStartTrading.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.btnStartTrading.Location = new System.Drawing.Point(240, 50);
            this.btnStartTrading.Margin = new System.Windows.Forms.Padding(6);
            this.btnStartTrading.Name = "btnStartTrading";
            this.btnStartTrading.Size = new System.Drawing.Size(200, 70);
            this.btnStartTrading.TabIndex = 2;
            this.btnStartTrading.Text = "🚀 매매시작";
            this.btnStartTrading.UseVisualStyleBackColor = false;
            this.btnStartTrading.Click += new System.EventHandler(this.btnStartTrading_Click);
            // 
            // btnLoadData
            // 
            this.btnLoadData.BackColor = System.Drawing.Color.LightBlue;
            this.btnLoadData.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.btnLoadData.Location = new System.Drawing.Point(19, 50);
            this.btnLoadData.Margin = new System.Windows.Forms.Padding(6);
            this.btnLoadData.Name = "btnLoadData";
            this.btnLoadData.Size = new System.Drawing.Size(200, 70);
            this.btnLoadData.TabIndex = 1;
            this.btnLoadData.Text = "📊 데이터 로드";
            this.btnLoadData.UseVisualStyleBackColor = false;
            this.btnLoadData.Click += new System.EventHandler(this.btnLoadData_Click);
            // 
            // grpStatistics
            // 
            this.grpStatistics.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpStatistics.Controls.Add(this.lblAnalysisDate);
            this.grpStatistics.Controls.Add(this.lblTotalCount);
            this.grpStatistics.Controls.Add(this.lblSGrade);
            this.grpStatistics.Controls.Add(this.lblAGrade);
            this.grpStatistics.Controls.Add(this.lblTradable);
            this.grpStatistics.Controls.Add(this.lblAvgScore);
            this.grpStatistics.Location = new System.Drawing.Point(19, 240);
            this.grpStatistics.Margin = new System.Windows.Forms.Padding(6);
            this.grpStatistics.Name = "grpStatistics";
            this.grpStatistics.Padding = new System.Windows.Forms.Padding(6);
            this.grpStatistics.Size = new System.Drawing.Size(1783, 120);
            this.grpStatistics.TabIndex = 1;
            this.grpStatistics.TabStop = false;
            this.grpStatistics.Text = "분석 통계";
            // 
            // lblAnalysisDate
            // 
            this.lblAnalysisDate.Location = new System.Drawing.Point(19, 50);
            this.lblAnalysisDate.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblAnalysisDate.Name = "lblAnalysisDate";
            this.lblAnalysisDate.Size = new System.Drawing.Size(241, 40);
            this.lblAnalysisDate.TabIndex = 7;
            this.lblAnalysisDate.Text = "분석일자: -";
            // 
            // lblTotalCount
            // 
            this.lblTotalCount.Location = new System.Drawing.Point(279, 50);
            this.lblTotalCount.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblTotalCount.Name = "lblTotalCount";
            this.lblTotalCount.Size = new System.Drawing.Size(149, 40);
            this.lblTotalCount.TabIndex = 8;
            this.lblTotalCount.Text = "전체: 0개";
            // 
            // lblSGrade
            // 
            this.lblSGrade.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.lblSGrade.ForeColor = System.Drawing.Color.Red;
            this.lblSGrade.Location = new System.Drawing.Point(446, 50);
            this.lblSGrade.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblSGrade.Name = "lblSGrade";
            this.lblSGrade.Size = new System.Drawing.Size(130, 40);
            this.lblSGrade.TabIndex = 9;
            this.lblSGrade.Text = "S급: 0개";
            // 
            // lblAGrade
            // 
            this.lblAGrade.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.lblAGrade.ForeColor = System.Drawing.Color.Blue;
            this.lblAGrade.Location = new System.Drawing.Point(594, 50);
            this.lblAGrade.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblAGrade.Name = "lblAGrade";
            this.lblAGrade.Size = new System.Drawing.Size(130, 40);
            this.lblAGrade.TabIndex = 10;
            this.lblAGrade.Text = "A급: 0개";
            // 
            // lblTradable
            // 
            this.lblTradable.Location = new System.Drawing.Point(743, 50);
            this.lblTradable.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblTradable.Name = "lblTradable";
            this.lblTradable.Size = new System.Drawing.Size(186, 40);
            this.lblTradable.TabIndex = 11;
            this.lblTradable.Text = "매매가능: 0개";
            // 
            // lblAvgScore
            // 
            this.lblAvgScore.Location = new System.Drawing.Point(947, 50);
            this.lblAvgScore.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblAvgScore.Name = "lblAvgScore";
            this.lblAvgScore.Size = new System.Drawing.Size(223, 40);
            this.lblAvgScore.TabIndex = 12;
            this.lblAvgScore.Text = "평균점수: 0점";
            // 
            // grpFilter
            // 
            this.grpFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpFilter.Controls.Add(this.rbAll);
            this.grpFilter.Controls.Add(this.rbSGrade);
            this.grpFilter.Controls.Add(this.rbAGrade);
            this.grpFilter.Controls.Add(this.rbTopGrades);
            this.grpFilter.Controls.Add(this.btnApplyFilter);
            this.grpFilter.Location = new System.Drawing.Point(19, 380);
            this.grpFilter.Margin = new System.Windows.Forms.Padding(6);
            this.grpFilter.Name = "grpFilter";
            this.grpFilter.Padding = new System.Windows.Forms.Padding(6);
            this.grpFilter.Size = new System.Drawing.Size(1783, 100);
            this.grpFilter.TabIndex = 2;
            this.grpFilter.TabStop = false;
            this.grpFilter.Text = "종목 필터";
            // 
            // rbAll
            // 
            this.rbAll.Checked = true;
            this.rbAll.Location = new System.Drawing.Point(19, 40);
            this.rbAll.Margin = new System.Windows.Forms.Padding(6);
            this.rbAll.Name = "rbAll";
            this.rbAll.Size = new System.Drawing.Size(111, 40);
            this.rbAll.TabIndex = 14;
            this.rbAll.TabStop = true;
            this.rbAll.Text = "전체";
            this.rbAll.UseVisualStyleBackColor = true;
            // 
            // rbSGrade
            // 
            this.rbSGrade.ForeColor = System.Drawing.Color.Red;
            this.rbSGrade.Location = new System.Drawing.Point(149, 40);
            this.rbSGrade.Margin = new System.Windows.Forms.Padding(6);
            this.rbSGrade.Name = "rbSGrade";
            this.rbSGrade.Size = new System.Drawing.Size(149, 40);
            this.rbSGrade.TabIndex = 15;
            this.rbSGrade.Text = "S등급만";
            this.rbSGrade.UseVisualStyleBackColor = true;
            // 
            // rbAGrade
            // 
            this.rbAGrade.ForeColor = System.Drawing.Color.Blue;
            this.rbAGrade.Location = new System.Drawing.Point(316, 40);
            this.rbAGrade.Margin = new System.Windows.Forms.Padding(6);
            this.rbAGrade.Name = "rbAGrade";
            this.rbAGrade.Size = new System.Drawing.Size(149, 40);
            this.rbAGrade.TabIndex = 16;
            this.rbAGrade.Text = "A등급만";
            this.rbAGrade.UseVisualStyleBackColor = true;
            // 
            // rbTopGrades
            // 
            this.rbTopGrades.ForeColor = System.Drawing.Color.Purple;
            this.rbTopGrades.Location = new System.Drawing.Point(483, 40);
            this.rbTopGrades.Margin = new System.Windows.Forms.Padding(6);
            this.rbTopGrades.Name = "rbTopGrades";
            this.rbTopGrades.Size = new System.Drawing.Size(149, 40);
            this.rbTopGrades.TabIndex = 17;
            this.rbTopGrades.Text = "S+A등급";
            this.rbTopGrades.UseVisualStyleBackColor = true;
            // 
            // btnApplyFilter
            // 
            this.btnApplyFilter.Location = new System.Drawing.Point(669, 34);
            this.btnApplyFilter.Margin = new System.Windows.Forms.Padding(6);
            this.btnApplyFilter.Name = "btnApplyFilter";
            this.btnApplyFilter.Size = new System.Drawing.Size(149, 50);
            this.btnApplyFilter.TabIndex = 18;
            this.btnApplyFilter.Text = "필터 적용";
            this.btnApplyFilter.UseVisualStyleBackColor = true;
            this.btnApplyFilter.Click += new System.EventHandler(this.btnApplyFilter_Click);
            // 
            // dgvStocks
            // 
            this.dgvStocks.AllowUserToAddRows = false;
            this.dgvStocks.AllowUserToDeleteRows = false;
            // --- 레이아웃 수정 ---
            this.dgvStocks.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.dgvStocks.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvStocks.BackgroundColor = System.Drawing.Color.White;
            this.dgvStocks.ColumnHeadersHeight = 46;
            this.dgvStocks.Location = new System.Drawing.Point(19, 500);
            this.dgvStocks.Margin = new System.Windows.Forms.Padding(6);
            this.dgvStocks.Name = "dgvStocks";
            this.dgvStocks.RowHeadersVisible = false;
            this.dgvStocks.RowHeadersWidth = 82;
            this.dgvStocks.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvStocks.Size = new System.Drawing.Size(900, 800); // 폭 조절
            this.dgvStocks.TabIndex = 3;
            this.dgvStocks.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvStocks_CellContentClick);
            this.dgvStocks.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgvStocks_CellFormatting);
            // 
            // grpMonitoring
            // 
            // --- 레이아웃 수정 ---
            this.grpMonitoring.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpMonitoring.Controls.Add(this.lblMonitoringTitle);
            this.grpMonitoring.Controls.Add(this.lblTotalInvestment);
            this.grpMonitoring.Controls.Add(this.lblCurrentProfit);
            this.grpMonitoring.Controls.Add(this.lblReturnRate);
            this.grpMonitoring.Controls.Add(this.lblProgress);
            this.grpMonitoring.Controls.Add(this.dgvMonitoring);
            this.grpMonitoring.Location = new System.Drawing.Point(940, 500); // 위치 조절
            this.grpMonitoring.Margin = new System.Windows.Forms.Padding(6);
            this.grpMonitoring.Name = "grpMonitoring";
            this.grpMonitoring.Padding = new System.Windows.Forms.Padding(6);
            this.grpMonitoring.Size = new System.Drawing.Size(862, 800); // 크기 조절
            this.grpMonitoring.TabIndex = 4;
            this.grpMonitoring.TabStop = false;
            this.grpMonitoring.Text = "실시간 매매 모니터링";
            this.grpMonitoring.Visible = false;
            // 
            // lblMonitoringTitle
            // 
            this.lblMonitoringTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.lblMonitoringTitle.ForeColor = System.Drawing.Color.DarkBlue;
            this.lblMonitoringTitle.Location = new System.Drawing.Point(19, 30);
            this.lblMonitoringTitle.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblMonitoringTitle.Name = "lblMonitoringTitle";
            this.lblMonitoringTitle.Size = new System.Drawing.Size(820, 30); // 크기 조절
            this.lblMonitoringTitle.TabIndex = 20;
            this.lblMonitoringTitle.Text = "📊 매매 현황 (0개 선택)";
            this.lblMonitoringTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblTotalInvestment
            // 
            this.lblTotalInvestment.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.lblTotalInvestment.Location = new System.Drawing.Point(19, 70);
            this.lblTotalInvestment.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblTotalInvestment.Name = "lblTotalInvestment";
            this.lblTotalInvestment.Size = new System.Drawing.Size(410, 25); // 크기 조절
            this.lblTotalInvestment.TabIndex = 21;
            this.lblTotalInvestment.Text = "💰 투자금액: 0원";
            // 
            // lblCurrentProfit
            // 
            this.lblCurrentProfit.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.lblCurrentProfit.ForeColor = System.Drawing.Color.Green;
            this.lblCurrentProfit.Location = new System.Drawing.Point(440, 70);
            this.lblCurrentProfit.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblCurrentProfit.Name = "lblCurrentProfit";
            this.lblCurrentProfit.Size = new System.Drawing.Size(400, 25); // 크기 조절
            this.lblCurrentProfit.TabIndex = 22;
            this.lblCurrentProfit.Text = "📈 현재수익: +0원";
            // 
            // lblReturnRate
            // 
            this.lblReturnRate.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.lblReturnRate.ForeColor = System.Drawing.Color.Green;
            this.lblReturnRate.Location = new System.Drawing.Point(19, 105);
            this.lblReturnRate.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblReturnRate.Name = "lblReturnRate";
            this.lblReturnRate.Size = new System.Drawing.Size(410, 25); // 크기 조절
            this.lblReturnRate.TabIndex = 23;
            this.lblReturnRate.Text = "📊 수익률: +0.0%";
            // 
            // lblProgress
            // 
            this.lblProgress.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.lblProgress.ForeColor = System.Drawing.Color.Blue;
            this.lblProgress.Location = new System.Drawing.Point(440, 105);
            this.lblProgress.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(400, 25); // 크기 조절
            this.lblProgress.TabIndex = 24;
            this.lblProgress.Text = "🎯 진행률: 0/0 완료";
            // 
            // dgvMonitoring
            // 
            this.dgvMonitoring.AllowUserToAddRows = false;
            this.dgvMonitoring.AllowUserToDeleteRows = false;
            this.dgvMonitoring.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvMonitoring.BackgroundColor = System.Drawing.Color.White;
            this.dgvMonitoring.ColumnHeadersHeight = 40;
            this.dgvMonitoring.Location = new System.Drawing.Point(19, 145);
            this.dgvMonitoring.Margin = new System.Windows.Forms.Padding(6);
            this.dgvMonitoring.Name = "dgvMonitoring";
            this.dgvMonitoring.ReadOnly = true;
            this.dgvMonitoring.RowHeadersVisible = false;
            this.dgvMonitoring.RowHeadersWidth = 82;
            this.dgvMonitoring.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvMonitoring.Size = new System.Drawing.Size(820, 635); // 크기 조절
            this.dgvMonitoring.TabIndex = 25;
            // 
            // grpLog
            // 
            this.grpLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpLog.Controls.Add(this.txtLog);
            this.grpLog.Location = new System.Drawing.Point(19, 1320);
            this.grpLog.Margin = new System.Windows.Forms.Padding(6);
            this.grpLog.Name = "grpLog";
            this.grpLog.Padding = new System.Windows.Forms.Padding(6);
            this.grpLog.Size = new System.Drawing.Size(1783, 160);
            this.grpLog.TabIndex = 5;
            this.grpLog.TabStop = false;
            this.grpLog.Text = "로그 메시지";
            // 
            // txtLog
            // 
            this.txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLog.BackColor = System.Drawing.Color.Black;
            this.txtLog.Font = new System.Drawing.Font("Consolas", 8F);
            this.txtLog.ForeColor = System.Drawing.Color.Lime;
            this.txtLog.Location = new System.Drawing.Point(19, 40);
            this.txtLog.Margin = new System.Windows.Forms.Padding(6);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(1742, 96);
            this.txtLog.TabIndex = 21;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1827, 1502);
            this.Controls.Add(this.grpLog);
            this.Controls.Add(this.grpMonitoring);
            this.Controls.Add(this.dgvStocks);
            this.Controls.Add(this.grpFilter);
            this.Controls.Add(this.grpStatistics);
            this.Controls.Add(this.grpControl);
            this.Margin = new System.Windows.Forms.Padding(6);
            this.MinimumSize = new System.Drawing.Size(1463, 1169);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AutoTrader - 스마트 자동매매 시스템";
            this.grpControl.ResumeLayout(false);
            this.grpControl.PerformLayout();
            this.pnlSystemStatus.ResumeLayout(false);
            this.pnlMarketStatus.ResumeLayout(false);
            this.pnlDailyLoss.ResumeLayout(false);
            this.grpStatistics.ResumeLayout(false);
            this.grpFilter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvStocks)).EndInit();
            this.grpMonitoring.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvMonitoring)).EndInit();
            this.grpLog.ResumeLayout(false);
            this.grpLog.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpControl;
        private System.Windows.Forms.Button btnLoadData;
        private System.Windows.Forms.Button btnStartTrading;
        private System.Windows.Forms.Button btnStopTrading;
        private System.Windows.Forms.Label lblConnectionStatus;
        private System.Windows.Forms.Label lblLastUpdate;
        private System.Windows.Forms.Label lblKiwoomStatus;
        private System.Windows.Forms.Label lblAccountInfo;
        private System.Windows.Forms.Label lblAccountLabel;
        private System.Windows.Forms.ComboBox cmbAccount;
        private System.Windows.Forms.GroupBox grpStatistics;
        private System.Windows.Forms.Label lblAnalysisDate;
        private System.Windows.Forms.Label lblTotalCount;
        private System.Windows.Forms.Label lblSGrade;
        private System.Windows.Forms.Label lblAGrade;
        private System.Windows.Forms.Label lblTradable;
        private System.Windows.Forms.Label lblAvgScore;
        private System.Windows.Forms.GroupBox grpFilter;
        private System.Windows.Forms.RadioButton rbAll;
        private System.Windows.Forms.RadioButton rbSGrade;
        private System.Windows.Forms.RadioButton rbAGrade;
        private System.Windows.Forms.RadioButton rbTopGrades;
        private System.Windows.Forms.Button btnApplyFilter;
        private System.Windows.Forms.DataGridView dgvStocks;
        private System.Windows.Forms.GroupBox grpMonitoring;
        private System.Windows.Forms.Label lblMonitoringTitle;
        private System.Windows.Forms.Label lblTotalInvestment;
        private System.Windows.Forms.Label lblCurrentProfit;
        private System.Windows.Forms.Label lblReturnRate;
        private System.Windows.Forms.Label lblProgress;
        private System.Windows.Forms.DataGridView dgvMonitoring;
        private System.Windows.Forms.GroupBox grpLog;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Panel pnlDailyLoss;
        private System.Windows.Forms.Label lblDailyLossStatus;
        private System.Windows.Forms.Label lblDailyLossTitle;
        private System.Windows.Forms.Panel pnlMarketStatus;
        private System.Windows.Forms.Label lblMarketStatus;
        private System.Windows.Forms.Label lblMarketStatusTitle;
        private System.Windows.Forms.Panel pnlSystemStatus;
        private System.Windows.Forms.Label lblSystemStatus;
        private System.Windows.Forms.Label lblSystemStatusTitle;
    }
}