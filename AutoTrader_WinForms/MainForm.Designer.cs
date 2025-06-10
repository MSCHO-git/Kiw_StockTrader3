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
            this.grpLog = new System.Windows.Forms.GroupBox();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.mainSplitContainer = new System.Windows.Forms.SplitContainer();
            this.dgvStocks = new System.Windows.Forms.DataGridView();
            this.grpMonitoring = new System.Windows.Forms.GroupBox();
            this.dgvMonitoring = new System.Windows.Forms.DataGridView();
            this.lblProgress = new System.Windows.Forms.Label();
            this.lblReturnRate = new System.Windows.Forms.Label();
            this.lblCurrentProfit = new System.Windows.Forms.Label();
            this.lblTotalInvestment = new System.Windows.Forms.Label();
            this.lblMonitoringTitle = new System.Windows.Forms.Label();
            this.grpControl.SuspendLayout();
            this.pnlSystemStatus.SuspendLayout();
            this.pnlMarketStatus.SuspendLayout();
            this.pnlDailyLoss.SuspendLayout();
            this.grpStatistics.SuspendLayout();
            this.grpFilter.SuspendLayout();
            this.grpLog.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).BeginInit();
            this.mainSplitContainer.Panel1.SuspendLayout();
            this.mainSplitContainer.Panel2.SuspendLayout();
            this.mainSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvStocks)).BeginInit();
            this.grpMonitoring.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMonitoring)).BeginInit();
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
            this.grpControl.Location = new System.Drawing.Point(12, 12);
            this.grpControl.Name = "grpControl";
            this.grpControl.Size = new System.Drawing.Size(1372, 110);
            this.grpControl.TabIndex = 0;
            this.grpControl.TabStop = false;
            this.grpControl.Text = "매매 제어 및 핵심 상태 보드";
            // 
            // pnlSystemStatus
            // 
            this.pnlSystemStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.pnlSystemStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlSystemStatus.Controls.Add(this.lblSystemStatus);
            this.pnlSystemStatus.Location = new System.Drawing.Point(785, 42);
            this.pnlSystemStatus.Margin = new System.Windows.Forms.Padding(2);
            this.pnlSystemStatus.Name = "pnlSystemStatus";
            this.pnlSystemStatus.Size = new System.Drawing.Size(155, 28);
            this.pnlSystemStatus.TabIndex = 15;
            // 
            // lblSystemStatus
            // 
            this.lblSystemStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSystemStatus.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold);
            this.lblSystemStatus.Location = new System.Drawing.Point(0, 0);
            this.lblSystemStatus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblSystemStatus.Name = "lblSystemStatus";
            this.lblSystemStatus.Size = new System.Drawing.Size(153, 26);
            this.lblSystemStatus.TabIndex = 0;
            this.lblSystemStatus.Text = "매매 대기";
            this.lblSystemStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblSystemStatusTitle
            // 
            this.lblSystemStatusTitle.AutoSize = true;
            this.lblSystemStatusTitle.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblSystemStatusTitle.Location = new System.Drawing.Point(783, 21);
            this.lblSystemStatusTitle.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblSystemStatusTitle.Name = "lblSystemStatusTitle";
            this.lblSystemStatusTitle.Size = new System.Drawing.Size(104, 12);
            this.lblSystemStatusTitle.TabIndex = 14;
            this.lblSystemStatusTitle.Text = "현재 시스템 상태";
            // 
            // pnlMarketStatus
            // 
            this.pnlMarketStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(235)))), ((int)(((byte)(255)))));
            this.pnlMarketStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlMarketStatus.Controls.Add(this.lblMarketStatus);
            this.pnlMarketStatus.Location = new System.Drawing.Point(623, 42);
            this.pnlMarketStatus.Margin = new System.Windows.Forms.Padding(2);
            this.pnlMarketStatus.Name = "pnlMarketStatus";
            this.pnlMarketStatus.Size = new System.Drawing.Size(155, 28);
            this.pnlMarketStatus.TabIndex = 13;
            // 
            // lblMarketStatus
            // 
            this.lblMarketStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMarketStatus.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold);
            this.lblMarketStatus.Location = new System.Drawing.Point(0, 0);
            this.lblMarketStatus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblMarketStatus.Name = "lblMarketStatus";
            this.lblMarketStatus.Size = new System.Drawing.Size(153, 26);
            this.lblMarketStatus.TabIndex = 0;
            this.lblMarketStatus.Text = "확인 중...";
            this.lblMarketStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblMarketStatusTitle
            // 
            this.lblMarketStatusTitle.AutoSize = true;
            this.lblMarketStatusTitle.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblMarketStatusTitle.Location = new System.Drawing.Point(621, 21);
            this.lblMarketStatusTitle.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblMarketStatusTitle.Name = "lblMarketStatusTitle";
            this.lblMarketStatusTitle.Size = new System.Drawing.Size(57, 12);
            this.lblMarketStatusTitle.TabIndex = 12;
            this.lblMarketStatusTitle.Text = "시장 상태";
            // 
            // pnlDailyLoss
            // 
            this.pnlDailyLoss.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(255)))), ((int)(((byte)(220)))));
            this.pnlDailyLoss.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlDailyLoss.Controls.Add(this.lblDailyLossStatus);
            this.pnlDailyLoss.Location = new System.Drawing.Point(385, 42);
            this.pnlDailyLoss.Margin = new System.Windows.Forms.Padding(2);
            this.pnlDailyLoss.Name = "pnlDailyLoss";
            this.pnlDailyLoss.Size = new System.Drawing.Size(230, 28);
            this.pnlDailyLoss.TabIndex = 11;
            // 
            // lblDailyLossStatus
            // 
            this.lblDailyLossStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDailyLossStatus.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold);
            this.lblDailyLossStatus.Location = new System.Drawing.Point(0, 0);
            this.lblDailyLossStatus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblDailyLossStatus.Name = "lblDailyLossStatus";
            this.lblDailyLossStatus.Size = new System.Drawing.Size(228, 26);
            this.lblDailyLossStatus.TabIndex = 0;
            this.lblDailyLossStatus.Text = "0 / -600,000 원";
            this.lblDailyLossStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblDailyLossTitle
            // 
            this.lblDailyLossTitle.AutoSize = true;
            this.lblDailyLossTitle.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblDailyLossTitle.Location = new System.Drawing.Point(383, 21);
            this.lblDailyLossTitle.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblDailyLossTitle.Name = "lblDailyLossTitle";
            this.lblDailyLossTitle.Size = new System.Drawing.Size(83, 12);
            this.lblDailyLossTitle.TabIndex = 10;
            this.lblDailyLossTitle.Text = "일일 손실 한도";
            // 
            // cmbAccount
            // 
            this.cmbAccount.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbAccount.Enabled = false;
            this.cmbAccount.FormattingEnabled = true;
            this.cmbAccount.Location = new System.Drawing.Point(815, 80);
            this.cmbAccount.Name = "cmbAccount";
            this.cmbAccount.Size = new System.Drawing.Size(138, 20);
            this.cmbAccount.TabIndex = 9;
            this.cmbAccount.SelectedIndexChanged += new System.EventHandler(this.cmbAccount_SelectedIndexChanged);
            // 
            // lblAccountLabel
            // 
            this.lblAccountLabel.Location = new System.Drawing.Point(750, 82);
            this.lblAccountLabel.Name = "lblAccountLabel";
            this.lblAccountLabel.Size = new System.Drawing.Size(56, 15);
            this.lblAccountLabel.TabIndex = 8;
            this.lblAccountLabel.Text = "계좌 선택:";
            this.lblAccountLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblAccountInfo
            // 
            this.lblAccountInfo.ForeColor = System.Drawing.Color.Gray;
            this.lblAccountInfo.Location = new System.Drawing.Point(415, 82);
            this.lblAccountInfo.Name = "lblAccountInfo";
            this.lblAccountInfo.Size = new System.Drawing.Size(222, 15);
            this.lblAccountInfo.TabIndex = 7;
            this.lblAccountInfo.Text = "계좌: - | 예수금: -";
            // 
            // lblKiwoomStatus
            // 
            this.lblKiwoomStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblKiwoomStatus.ForeColor = System.Drawing.Color.Red;
            this.lblKiwoomStatus.Location = new System.Drawing.Point(10, 82);
            this.lblKiwoomStatus.Name = "lblKiwoomStatus";
            this.lblKiwoomStatus.Size = new System.Drawing.Size(278, 15);
            this.lblKiwoomStatus.TabIndex = 6;
            this.lblKiwoomStatus.Text = "키움 연결: ❌ 미연결 (매매시작 시 자동 연결)";
            // 
            // lblLastUpdate
            // 
            this.lblLastUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLastUpdate.ForeColor = System.Drawing.Color.Gray;
            this.lblLastUpdate.Location = new System.Drawing.Point(1118, 45);
            this.lblLastUpdate.Name = "lblLastUpdate";
            this.lblLastUpdate.Size = new System.Drawing.Size(242, 15);
            this.lblLastUpdate.TabIndex = 5;
            this.lblLastUpdate.Text = "마지막 업데이트: -";
            this.lblLastUpdate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblConnectionStatus
            // 
            this.lblConnectionStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblConnectionStatus.ForeColor = System.Drawing.Color.Gray;
            this.lblConnectionStatus.Location = new System.Drawing.Point(1118, 24);
            this.lblConnectionStatus.Name = "lblConnectionStatus";
            this.lblConnectionStatus.Size = new System.Drawing.Size(242, 15);
            this.lblConnectionStatus.TabIndex = 4;
            this.lblConnectionStatus.Text = "DB 연결: 확인 중...";
            this.lblConnectionStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnStopTrading
            // 
            this.btnStopTrading.BackColor = System.Drawing.Color.LightCoral;
            this.btnStopTrading.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.btnStopTrading.Location = new System.Drawing.Point(131, 28);
            this.btnStopTrading.Name = "btnStopTrading";
            this.btnStopTrading.Size = new System.Drawing.Size(111, 38);
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
            this.btnStartTrading.Location = new System.Drawing.Point(131, 28);
            this.btnStartTrading.Name = "btnStartTrading";
            this.btnStartTrading.Size = new System.Drawing.Size(111, 38);
            this.btnStartTrading.TabIndex = 2;
            this.btnStartTrading.Text = "🚀 매매시작";
            this.btnStartTrading.UseVisualStyleBackColor = false;
            this.btnStartTrading.Click += new System.EventHandler(this.btnStartTrading_Click);
            // 
            // btnLoadData
            // 
            this.btnLoadData.BackColor = System.Drawing.Color.LightBlue;
            this.btnLoadData.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.btnLoadData.Location = new System.Drawing.Point(10, 28);
            this.btnLoadData.Name = "btnLoadData";
            this.btnLoadData.Size = new System.Drawing.Size(111, 38);
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
            this.grpStatistics.Location = new System.Drawing.Point(12, 131);
            this.grpStatistics.Name = "grpStatistics";
            this.grpStatistics.Size = new System.Drawing.Size(1372, 66);
            this.grpStatistics.TabIndex = 1;
            this.grpStatistics.TabStop = false;
            this.grpStatistics.Text = "분석 통계";
            // 
            // lblAnalysisDate
            // 
            this.lblAnalysisDate.Location = new System.Drawing.Point(10, 28);
            this.lblAnalysisDate.Name = "lblAnalysisDate";
            this.lblAnalysisDate.Size = new System.Drawing.Size(134, 22);
            this.lblAnalysisDate.TabIndex = 7;
            this.lblAnalysisDate.Text = "분석일자: -";
            // 
            // lblTotalCount
            // 
            this.lblTotalCount.Location = new System.Drawing.Point(155, 28);
            this.lblTotalCount.Name = "lblTotalCount";
            this.lblTotalCount.Size = new System.Drawing.Size(83, 22);
            this.lblTotalCount.TabIndex = 8;
            this.lblTotalCount.Text = "전체: 0개";
            // 
            // lblSGrade
            // 
            this.lblSGrade.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.lblSGrade.ForeColor = System.Drawing.Color.Red;
            this.lblSGrade.Location = new System.Drawing.Point(248, 28);
            this.lblSGrade.Name = "lblSGrade";
            this.lblSGrade.Size = new System.Drawing.Size(72, 22);
            this.lblSGrade.TabIndex = 9;
            this.lblSGrade.Text = "S급: 0개";
            // 
            // lblAGrade
            // 
            this.lblAGrade.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.lblAGrade.ForeColor = System.Drawing.Color.Blue;
            this.lblAGrade.Location = new System.Drawing.Point(330, 28);
            this.lblAGrade.Name = "lblAGrade";
            this.lblAGrade.Size = new System.Drawing.Size(72, 22);
            this.lblAGrade.TabIndex = 10;
            this.lblAGrade.Text = "A급: 0개";
            // 
            // lblTradable
            // 
            this.lblTradable.Location = new System.Drawing.Point(413, 28);
            this.lblTradable.Name = "lblTradable";
            this.lblTradable.Size = new System.Drawing.Size(103, 22);
            this.lblTradable.TabIndex = 11;
            this.lblTradable.Text = "매매가능: 0개";
            // 
            // lblAvgScore
            // 
            this.lblAvgScore.Location = new System.Drawing.Point(526, 28);
            this.lblAvgScore.Name = "lblAvgScore";
            this.lblAvgScore.Size = new System.Drawing.Size(124, 22);
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
            this.grpFilter.Location = new System.Drawing.Point(12, 203);
            this.grpFilter.Name = "grpFilter";
            this.grpFilter.Size = new System.Drawing.Size(1372, 55);
            this.grpFilter.TabIndex = 2;
            this.grpFilter.TabStop = false;
            this.grpFilter.Text = "종목 필터";
            // 
            // rbAll
            // 
            this.rbAll.Checked = true;
            this.rbAll.Location = new System.Drawing.Point(10, 22);
            this.rbAll.Name = "rbAll";
            this.rbAll.Size = new System.Drawing.Size(62, 22);
            this.rbAll.TabIndex = 14;
            this.rbAll.TabStop = true;
            this.rbAll.Text = "전체";
            this.rbAll.UseVisualStyleBackColor = true;
            // 
            // rbSGrade
            // 
            this.rbSGrade.ForeColor = System.Drawing.Color.Red;
            this.rbSGrade.Location = new System.Drawing.Point(83, 22);
            this.rbSGrade.Name = "rbSGrade";
            this.rbSGrade.Size = new System.Drawing.Size(83, 22);
            this.rbSGrade.TabIndex = 15;
            this.rbSGrade.Text = "S등급만";
            this.rbSGrade.UseVisualStyleBackColor = true;
            // 
            // rbAGrade
            // 
            this.rbAGrade.ForeColor = System.Drawing.Color.Blue;
            this.rbAGrade.Location = new System.Drawing.Point(175, 22);
            this.rbAGrade.Name = "rbAGrade";
            this.rbAGrade.Size = new System.Drawing.Size(83, 22);
            this.rbAGrade.TabIndex = 16;
            this.rbAGrade.Text = "A등급만";
            this.rbAGrade.UseVisualStyleBackColor = true;
            // 
            // rbTopGrades
            // 
            this.rbTopGrades.ForeColor = System.Drawing.Color.Purple;
            this.rbTopGrades.Location = new System.Drawing.Point(268, 22);
            this.rbTopGrades.Name = "rbTopGrades";
            this.rbTopGrades.Size = new System.Drawing.Size(83, 22);
            this.rbTopGrades.TabIndex = 17;
            this.rbTopGrades.Text = "S+A등급";
            this.rbTopGrades.UseVisualStyleBackColor = true;
            // 
            // btnApplyFilter
            // 
            this.btnApplyFilter.Location = new System.Drawing.Point(372, 19);
            this.btnApplyFilter.Name = "btnApplyFilter";
            this.btnApplyFilter.Size = new System.Drawing.Size(83, 28);
            this.btnApplyFilter.TabIndex = 18;
            this.btnApplyFilter.Text = "필터 적용";
            this.btnApplyFilter.UseVisualStyleBackColor = true;
            this.btnApplyFilter.Click += new System.EventHandler(this.btnApplyFilter_Click);
            // 
            // grpLog
            // 
            this.grpLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpLog.Controls.Add(this.txtLog);
            this.grpLog.Location = new System.Drawing.Point(12, 715);
            this.grpLog.Name = "grpLog";
            this.grpLog.Size = new System.Drawing.Size(1372, 88);
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
            this.txtLog.Location = new System.Drawing.Point(10, 22);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(1349, 53);
            this.txtLog.TabIndex = 21;
            // 
            // mainSplitContainer
            // 
            this.mainSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mainSplitContainer.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.mainSplitContainer.Location = new System.Drawing.Point(12, 264);
            this.mainSplitContainer.Name = "mainSplitContainer";
            // 
            // mainSplitContainer.Panel1
            // 
            this.mainSplitContainer.Panel1.Controls.Add(this.dgvStocks);
            // 
            // mainSplitContainer.Panel2
            // 
            this.mainSplitContainer.Panel2.Controls.Add(this.grpMonitoring);
            this.mainSplitContainer.Size = new System.Drawing.Size(1372, 445);
            this.mainSplitContainer.SplitterDistance = 790;
            this.mainSplitContainer.TabIndex = 6;
            // 
            // dgvStocks
            // 
            this.dgvStocks.AllowUserToAddRows = false;
            this.dgvStocks.AllowUserToDeleteRows = false;
            this.dgvStocks.BackgroundColor = System.Drawing.Color.White;
            this.dgvStocks.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvStocks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvStocks.Location = new System.Drawing.Point(0, 0);
            this.dgvStocks.Name = "dgvStocks";
            this.dgvStocks.RowHeadersVisible = false;
            this.dgvStocks.RowHeadersWidth = 82;
            this.dgvStocks.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvStocks.Size = new System.Drawing.Size(646, 441);
            this.dgvStocks.TabIndex = 3;
            this.dgvStocks.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvStocks_CellContentClick);
            this.dgvStocks.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgvStocks_CellFormatting);
            // 
            // grpMonitoring
            // 
            this.grpMonitoring.Controls.Add(this.dgvMonitoring);
            this.grpMonitoring.Controls.Add(this.lblProgress);
            this.grpMonitoring.Controls.Add(this.lblReturnRate);
            this.grpMonitoring.Controls.Add(this.lblCurrentProfit);
            this.grpMonitoring.Controls.Add(this.lblTotalInvestment);
            this.grpMonitoring.Controls.Add(this.lblMonitoringTitle);
            this.grpMonitoring.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpMonitoring.Location = new System.Drawing.Point(0, 0);
            this.grpMonitoring.Name = "grpMonitoring";
            this.grpMonitoring.Size = new System.Drawing.Size(714, 441);
            this.grpMonitoring.TabIndex = 4;
            this.grpMonitoring.TabStop = false;
            this.grpMonitoring.Text = "실시간 매매 모니터링";
            this.grpMonitoring.Visible = false;
            // 
            // dgvMonitoring
            // 
            this.dgvMonitoring.AllowUserToAddRows = false;
            this.dgvMonitoring.AllowUserToDeleteRows = false;
            this.dgvMonitoring.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvMonitoring.BackgroundColor = System.Drawing.Color.White;
            this.dgvMonitoring.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMonitoring.Location = new System.Drawing.Point(10, 80);
            this.dgvMonitoring.Name = "dgvMonitoring";
            this.dgvMonitoring.ReadOnly = true;
            this.dgvMonitoring.RowHeadersVisible = false;
            this.dgvMonitoring.RowHeadersWidth = 82;
            this.dgvMonitoring.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvMonitoring.Size = new System.Drawing.Size(692, 350);
            this.dgvMonitoring.TabIndex = 25;
            // 
            // lblProgress
            // 
            this.lblProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblProgress.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.lblProgress.ForeColor = System.Drawing.Color.Blue;
            this.lblProgress.Location = new System.Drawing.Point(359, 58);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(343, 14);
            this.lblProgress.TabIndex = 24;
            this.lblProgress.Text = "🎯 진행률: 0/0 완료";
            // 
            // lblReturnRate
            // 
            this.lblReturnRate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblReturnRate.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.lblReturnRate.ForeColor = System.Drawing.Color.Green;
            this.lblReturnRate.Location = new System.Drawing.Point(10, 58);
            this.lblReturnRate.Name = "lblReturnRate";
            this.lblReturnRate.Size = new System.Drawing.Size(326, 14);
            this.lblReturnRate.TabIndex = 23;
            this.lblReturnRate.Text = "📊 수익률: +0.0%";
            // 
            // lblCurrentProfit
            // 
            this.lblCurrentProfit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCurrentProfit.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.lblCurrentProfit.ForeColor = System.Drawing.Color.Green;
            this.lblCurrentProfit.Location = new System.Drawing.Point(359, 39);
            this.lblCurrentProfit.Name = "lblCurrentProfit";
            this.lblCurrentProfit.Size = new System.Drawing.Size(343, 14);
            this.lblCurrentProfit.TabIndex = 22;
            this.lblCurrentProfit.Text = "📈 현재수익: +0원";
            // 
            // lblTotalInvestment
            // 
            this.lblTotalInvestment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTotalInvestment.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.lblTotalInvestment.Location = new System.Drawing.Point(10, 39);
            this.lblTotalInvestment.Name = "lblTotalInvestment";
            this.lblTotalInvestment.Size = new System.Drawing.Size(326, 14);
            this.lblTotalInvestment.TabIndex = 21;
            this.lblTotalInvestment.Text = "💰 투자금액: 0원";
            // 
            // lblMonitoringTitle
            // 
            this.lblMonitoringTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMonitoringTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.lblMonitoringTitle.ForeColor = System.Drawing.Color.DarkBlue;
            this.lblMonitoringTitle.Location = new System.Drawing.Point(10, 16);
            this.lblMonitoringTitle.Name = "lblMonitoringTitle";
            this.lblMonitoringTitle.Size = new System.Drawing.Size(692, 16);
            this.lblMonitoringTitle.TabIndex = 20;
            this.lblMonitoringTitle.Text = "📊 매매 현황 (0개 선택)";
            this.lblMonitoringTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1396, 811);
            this.Controls.Add(this.mainSplitContainer);
            this.Controls.Add(this.grpLog);
            this.Controls.Add(this.grpFilter);
            this.Controls.Add(this.grpStatistics);
            this.Controls.Add(this.grpControl);
            this.MinimumSize = new System.Drawing.Size(1400, 680);
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
            this.grpLog.ResumeLayout(false);
            this.grpLog.PerformLayout();
            this.mainSplitContainer.Panel1.ResumeLayout(false);
            this.mainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).EndInit();
            this.mainSplitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvStocks)).EndInit();
            this.grpMonitoring.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvMonitoring)).EndInit();
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
        private System.Windows.Forms.SplitContainer mainSplitContainer;
        private System.Windows.Forms.DataGridView dgvStocks;
        private System.Windows.Forms.GroupBox grpMonitoring;
        private System.Windows.Forms.DataGridView dgvMonitoring;
        private System.Windows.Forms.Label lblProgress;
        private System.Windows.Forms.Label lblReturnRate;
        private System.Windows.Forms.Label lblCurrentProfit;
        private System.Windows.Forms.Label lblTotalInvestment;
        private System.Windows.Forms.Label lblMonitoringTitle;
    }
}