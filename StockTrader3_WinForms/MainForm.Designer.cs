using System.Drawing;
using System.Windows.Forms;

namespace StockTrader3_WinForms
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.grpKiwoomConnection = new System.Windows.Forms.GroupBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.lblConnectionStatus = new System.Windows.Forms.Label();
            this.lblUserInfo = new System.Windows.Forms.Label();

            // 🆕 여기에 새로운 컨트롤 생성 코드 4줄 추가:
            this.rbSimulation = new System.Windows.Forms.RadioButton();
            this.rbReal = new System.Windows.Forms.RadioButton();
            this.cmbAccounts = new System.Windows.Forms.ComboBox();
            this.lblAccountSelect = new System.Windows.Forms.Label();


            this.grpConditionSearch = new System.Windows.Forms.GroupBox();
            this.cmbConditions = new System.Windows.Forms.ComboBox();
            this.btnRefreshConditions = new System.Windows.Forms.Button();
            this.lblSelectedCondition = new System.Windows.Forms.Label();
            this.grpAnalysisSteps = new System.Windows.Forms.GroupBox();
            this.btnStep1 = new System.Windows.Forms.Button();
            this.btnStep2 = new System.Windows.Forms.Button();
            this.btnStep3 = new System.Windows.Forms.Button();
            this.btnStep4 = new System.Windows.Forms.Button();
            this.btnCollectHistoricalData = new System.Windows.Forms.Button();
            this.lblStepsInfo = new System.Windows.Forms.Label();
            this.grpAccountInfo = new System.Windows.Forms.GroupBox();
            this.lblAccountSummary = new System.Windows.Forms.Label();
            this.lblBalance = new System.Windows.Forms.Label();
            this.lblTotalValue = new System.Windows.Forms.Label();
            this.lblProfit = new System.Windows.Forms.Label();
            //this.btnAccountDetail = new System.Windows.Forms.Button();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.pnlLeft = new System.Windows.Forms.Panel();
            this.grpProgress = new System.Windows.Forms.GroupBox();
            this.lblCurrentStep = new System.Windows.Forms.Label();
            this.lblStepProgress = new System.Windows.Forms.Label();
            this.lblNextStep = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            //this.grpMyStocks = new System.Windows.Forms.GroupBox();
            //this.lblMyStockCount = new System.Windows.Forms.Label();
            //this.lstMyStocks = new System.Windows.Forms.ListBox();
            //this.btnMyStockDetail = new System.Windows.Forms.Button();
            this.pnlRight = new System.Windows.Forms.Panel();
            this.lblAnalysisDate = new System.Windows.Forms.Label();
            this.lblConditionName = new System.Windows.Forms.Label();
            this.dgvStockList = new System.Windows.Forms.DataGridView();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            //this.btnDebugDB = new System.Windows.Forms.Button();
            this.grpKiwoomConnection.SuspendLayout();
            this.grpConditionSearch.SuspendLayout();
            this.grpAnalysisSteps.SuspendLayout();
            this.grpAccountInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            this.pnlLeft.SuspendLayout();
            this.grpProgress.SuspendLayout();
            //this.grpMyStocks.SuspendLayout();
            this.pnlRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvStockList)).BeginInit();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpKiwoomConnection
            // 
            // 
            // grpKiwoomConnection
            // 
            this.grpKiwoomConnection.Controls.Add(this.rbSimulation);      // 🆕 추가
            this.grpKiwoomConnection.Controls.Add(this.rbReal);           // 🆕 추가
            this.grpKiwoomConnection.Controls.Add(this.btnConnect);
            this.grpKiwoomConnection.Controls.Add(this.lblConnectionStatus);
            this.grpKiwoomConnection.Controls.Add(this.lblUserInfo);
            this.grpKiwoomConnection.Controls.Add(this.cmbAccounts);       // 🆕 추가
            this.grpKiwoomConnection.Controls.Add(this.lblAccountSelect); // 🆕 추가
            this.grpKiwoomConnection.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.grpKiwoomConnection.Location = new System.Drawing.Point(20, 20);
            this.grpKiwoomConnection.Name = "grpKiwoomConnection";
            this.grpKiwoomConnection.Size = new System.Drawing.Size(320, 160);     // 🔄 크기 변경 (280,120 → 400,160)
            this.grpKiwoomConnection.TabIndex = 0;
            this.grpKiwoomConnection.TabStop = false;
            this.grpKiwoomConnection.Text = "키움 API 연결";


          
            // 
            // btnConnect
            // 
            this.btnConnect.Font = new System.Drawing.Font("맑은 고딕", 9F);
            
            this.btnConnect.Name = "btnConnect";
            
            this.btnConnect.Location = new System.Drawing.Point(200, 22);
            this.btnConnect.Size = new System.Drawing.Size(100, 35);
            this.btnConnect.TabIndex = 0;
            this.btnConnect.Text = "키움 연결";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.BtnConnect_Click);
            // 
            // lblConnectionStatus
            // 
            this.lblConnectionStatus.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.lblConnectionStatus.ForeColor = System.Drawing.Color.Red;
            this.lblConnectionStatus.Location = new System.Drawing.Point(15, 65);
            this.lblConnectionStatus.Name = "lblConnectionStatus";
            this.lblConnectionStatus.Size = new System.Drawing.Size(180, 20);
            this.lblConnectionStatus.TabIndex = 1;
            this.lblConnectionStatus.Text = "❌ 연결 대기중";
            
            // 
            // lblUserInfo
            // 
            this.lblUserInfo.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.lblUserInfo.ForeColor = System.Drawing.Color.DarkBlue;
            this.lblUserInfo.Location = new System.Drawing.Point(190, 65);
            this.lblUserInfo.Name = "lblUserInfo";
            this.lblUserInfo.Size = new System.Drawing.Size(130, 20);
            this.lblUserInfo.TabIndex = 2;
            this.lblUserInfo.Text = "👤 연결 후 표시";

            // 
            // rbSimulation
            // 
            this.rbSimulation.AutoSize = true;
            this.rbSimulation.Checked = true;
            this.rbSimulation.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.rbSimulation.Location = new System.Drawing.Point(15, 25);
            this.rbSimulation.Name = "rbSimulation";
            this.rbSimulation.Size = new System.Drawing.Size(75, 19);
            this.rbSimulation.TabIndex = 10;
            this.rbSimulation.TabStop = true;
            this.rbSimulation.Text = "모의투자";
            this.rbSimulation.UseVisualStyleBackColor = true;
            this.rbSimulation.CheckedChanged += new System.EventHandler(this.RbInvestmentType_CheckedChanged);

            // 
            // rbReal
            // 
            this.rbReal.AutoSize = true;
            this.rbReal.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.rbReal.Location = new System.Drawing.Point(100, 25);
            this.rbReal.Name = "rbReal";
            this.rbReal.Size = new System.Drawing.Size(75, 19);
            this.rbReal.TabIndex = 11;
            this.rbReal.Text = "실전투자";
            this.rbReal.UseVisualStyleBackColor = true;
            this.rbReal.CheckedChanged += new System.EventHandler(this.RbInvestmentType_CheckedChanged);

            // 
            // lblAccountSelect
            // 
            this.lblAccountSelect.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.lblAccountSelect.ForeColor = System.Drawing.Color.DarkGreen;
            this.lblAccountSelect.Location = new System.Drawing.Point(15, 95);
            this.lblAccountSelect.Name = "lblAccountSelect";
            this.lblAccountSelect.Size = new System.Drawing.Size(120, 20);
            this.lblAccountSelect.TabIndex = 12;
            this.lblAccountSelect.Text = "계좌: 연결 후 표시";

            // 
            // cmbAccounts
            // 
            this.cmbAccounts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbAccounts.Enabled = false;
            this.cmbAccounts.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.cmbAccounts.Location = new System.Drawing.Point(15, 115);
            this.cmbAccounts.Name = "cmbAccounts";
            this.cmbAccounts.Size = new System.Drawing.Size(180, 23);
            this.cmbAccounts.TabIndex = 13;
            this.cmbAccounts.SelectedIndexChanged += new System.EventHandler(this.CmbAccounts_SelectedIndexChanged);


            // 
            // grpConditionSearch
            // 
            this.grpConditionSearch.Controls.Add(this.cmbConditions);
            this.grpConditionSearch.Controls.Add(this.btnRefreshConditions);
            this.grpConditionSearch.Controls.Add(this.lblSelectedCondition);
            this.grpConditionSearch.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.grpConditionSearch.Location = new System.Drawing.Point(350, 20);
            this.grpConditionSearch.Name = "grpConditionSearch";
            this.grpConditionSearch.Size = new System.Drawing.Size(350, 120);
            this.grpConditionSearch.TabIndex = 1;
            this.grpConditionSearch.TabStop = false;
            this.grpConditionSearch.Text = "조건검색 선택";
            // 
            // cmbConditions
            // 
            this.cmbConditions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbConditions.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.cmbConditions.Location = new System.Drawing.Point(15, 30);
            this.cmbConditions.Name = "cmbConditions";
            this.cmbConditions.Size = new System.Drawing.Size(220, 40);
            this.cmbConditions.TabIndex = 0;
            this.cmbConditions.SelectedIndexChanged += new System.EventHandler(this.CmbConditions_SelectedIndexChanged);
            // 
            // btnRefreshConditions
            // 
            this.btnRefreshConditions.Font = new System.Drawing.Font("맑은 고딕", 8F);
            this.btnRefreshConditions.Location = new System.Drawing.Point(250, 30);
            this.btnRefreshConditions.Name = "btnRefreshConditions";
            this.btnRefreshConditions.Size = new System.Drawing.Size(80, 25);
            this.btnRefreshConditions.TabIndex = 1;
            this.btnRefreshConditions.Text = "🔄 새로고침";
            this.btnRefreshConditions.UseVisualStyleBackColor = true;
            this.btnRefreshConditions.Click += new System.EventHandler(this.BtnRefresh_Click);
            // 
            // lblSelectedCondition
            // 
            this.lblSelectedCondition.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.lblSelectedCondition.ForeColor = System.Drawing.Color.Blue;
            this.lblSelectedCondition.Location = new System.Drawing.Point(15, 70);
            this.lblSelectedCondition.Name = "lblSelectedCondition";
            this.lblSelectedCondition.Size = new System.Drawing.Size(320, 40);
            this.lblSelectedCondition.TabIndex = 2;
            this.lblSelectedCondition.Text = "🔍 조건검색식 선택\n▼ 조건을 선택해주세요";
            // 
            // grpAnalysisSteps
            // 
            this.grpAnalysisSteps.Controls.Add(this.btnStep1);
            this.grpAnalysisSteps.Controls.Add(this.btnStep2);
            this.grpAnalysisSteps.Controls.Add(this.btnStep3);
            this.grpAnalysisSteps.Controls.Add(this.btnStep4);
            this.grpAnalysisSteps.Controls.Add(this.btnCollectHistoricalData);
            this.grpAnalysisSteps.Controls.Add(this.lblStepsInfo);
            this.grpAnalysisSteps.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.grpAnalysisSteps.Location = new System.Drawing.Point(710, 20);
            this.grpAnalysisSteps.Name = "grpAnalysisSteps";
            this.grpAnalysisSteps.Size = new System.Drawing.Size(490, 120);
            this.grpAnalysisSteps.TabIndex = 2;
            this.grpAnalysisSteps.TabStop = false;
            this.grpAnalysisSteps.Text = "분석 단계 실행";
            // 
            // btnStep1
            // 
            this.btnStep1.Font = new System.Drawing.Font("맑은 고딕", 8F, System.Drawing.FontStyle.Bold);
            this.btnStep1.Location = new System.Drawing.Point(15, 25);
            this.btnStep1.Name = "btnStep1";
            this.btnStep1.Size = new System.Drawing.Size(80, 50);
            this.btnStep1.TabIndex = 0;
            this.btnStep1.Text = "조건검색";
            this.btnStep1.UseVisualStyleBackColor = true;
            this.btnStep1.Click += new System.EventHandler(this.btnStep1_Click);
            // 
            // btnStep2
            // 
            this.btnStep2.Font = new System.Drawing.Font("맑은 고딕", 8F, System.Drawing.FontStyle.Bold);
            this.btnStep2.Location = new System.Drawing.Point(110, 25);
            this.btnStep2.Name = "btnStep2";
            this.btnStep2.Size = new System.Drawing.Size(80, 50);
            this.btnStep2.TabIndex = 1;
            this.btnStep2.Text = "[1차]\n기술적분석";
            this.btnStep2.UseVisualStyleBackColor = true;
            this.btnStep2.Click += new System.EventHandler(this.BtnTechnicalAnalysis_Click);
            // 
            // btnStep3
            // 
            this.btnStep3.Font = new System.Drawing.Font("맑은 고딕", 8F, System.Drawing.FontStyle.Bold);
            this.btnStep3.Location = new System.Drawing.Point(205, 25);
            this.btnStep3.Name = "btnStep3";
            this.btnStep3.Size = new System.Drawing.Size(80, 50);
            this.btnStep3.TabIndex = 2;
            this.btnStep3.Text = "[2차]\n뉴스분석";
            this.btnStep3.UseVisualStyleBackColor = true;
            this.btnStep3.Click += new System.EventHandler(this.BtnNewsAnalysis_Click);
            // 
            // btnStep4
            // 
            this.btnStep4.Font = new System.Drawing.Font("맑은 고딕", 8F, System.Drawing.FontStyle.Bold);
            this.btnStep4.Location = new System.Drawing.Point(300, 25);
            this.btnStep4.Name = "btnStep4";
            this.btnStep4.Size = new System.Drawing.Size(80, 50);
            this.btnStep4.TabIndex = 3;
            this.btnStep4.Text = "자동매매 시스템";
            this.btnStep4.UseVisualStyleBackColor = true;
            this.btnStep4.Click += new System.EventHandler(this.BtnTradePlan_Click);
            // 
            // btnCollectHistoricalData
            // 
            this.btnCollectHistoricalData.BackColor = System.Drawing.Color.LightCyan;
            this.btnCollectHistoricalData.Font = new System.Drawing.Font("맑은 고딕", 8F, System.Drawing.FontStyle.Bold);
            this.btnCollectHistoricalData.Location = new System.Drawing.Point(395, 25);
            this.btnCollectHistoricalData.Name = "btnCollectHistoricalData";
            this.btnCollectHistoricalData.Size = new System.Drawing.Size(80, 50);
            this.btnCollectHistoricalData.TabIndex = 4;
            this.btnCollectHistoricalData.Text = "과거데이터\n수집";
            this.btnCollectHistoricalData.UseVisualStyleBackColor = false;
            this.btnCollectHistoricalData.Click += new System.EventHandler(this.BtnCollectHistoricalData_Click);
            // 
            // lblStepsInfo
            // 
            this.lblStepsInfo.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.lblStepsInfo.ForeColor = System.Drawing.Color.DarkBlue;
            this.lblStepsInfo.Location = new System.Drawing.Point(15, 85);
            this.lblStepsInfo.Name = "lblStepsInfo";
            this.lblStepsInfo.Size = new System.Drawing.Size(590, 25);
            this.lblStepsInfo.TabIndex = 5;
            this.lblStepsInfo.Text = "단계: DB 초기화 → 키움 연결 → 조건선택 → 1차분석(기술) → 2차분석(뉴스) ";
            // 
            // grpAccountInfo
            // 
            this.grpAccountInfo.Controls.Add(this.lblAccountSummary);
            this.grpAccountInfo.Controls.Add(this.lblBalance);
            this.grpAccountInfo.Controls.Add(this.lblTotalValue);
            this.grpAccountInfo.Controls.Add(this.lblProfit);
            //this.grpAccountInfo.Controls.Add(this.btnAccountDetail);
            this.grpAccountInfo.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.grpAccountInfo.Location = new System.Drawing.Point(1210, 20);
            this.grpAccountInfo.Name = "grpAccountInfo";
            this.grpAccountInfo.Size = new System.Drawing.Size(150, 120);
            this.grpAccountInfo.TabIndex = 3;
            this.grpAccountInfo.TabStop = false;
            this.grpAccountInfo.Text = "계좌 정보";
            // 
            // lblAccountSummary
            // 
            this.lblAccountSummary.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.lblAccountSummary.Location = new System.Drawing.Point(10, 20);
            this.lblAccountSummary.Name = "lblAccountSummary";
            this.lblAccountSummary.Size = new System.Drawing.Size(200, 20);
            this.lblAccountSummary.TabIndex = 0;
            this.lblAccountSummary.Text = "💰 예수금: 106만";
            // 
            // lblBalance
            // 
            this.lblBalance.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.lblBalance.Location = new System.Drawing.Point(10, 40);
            this.lblBalance.Name = "lblBalance";
            this.lblBalance.Size = new System.Drawing.Size(230, 20);
            this.lblBalance.TabIndex = 1;
            this.lblBalance.Text = "📊 총평가: 524만";
            // 
            // lblTotalValue
            // 
            this.lblTotalValue.Location = new System.Drawing.Point(0, 0);
            this.lblTotalValue.Name = "lblTotalValue";
            this.lblTotalValue.Size = new System.Drawing.Size(100, 23);
            this.lblTotalValue.TabIndex = 2;
            // 
            // lblProfit
            // 
            this.lblProfit.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.lblProfit.ForeColor = System.Drawing.Color.Red;
            this.lblProfit.Location = new System.Drawing.Point(10, 60);
            this.lblProfit.Name = "lblProfit";
            this.lblProfit.Size = new System.Drawing.Size(230, 20);
            this.lblProfit.TabIndex = 2;
            this.lblProfit.Text = "📈 손익: +18만(+3.6%)";
            // 
            // btnAccountDetail
            // 
           
            // 
            // splitMain
            // 
            this.splitMain.Location = new System.Drawing.Point(20, 200);
            this.splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            this.splitMain.Panel1.Controls.Add(this.pnlLeft);
            // 
            // splitMain.Panel2
            // 
            this.splitMain.Panel2.Controls.Add(this.pnlRight);
            this.splitMain.Size = new System.Drawing.Size(1370, 580);
            this.splitMain.SplitterDistance = 200;
            this.splitMain.SplitterWidth = 5;
            this.splitMain.TabIndex = 4;
            // 
            // pnlLeft
            // 
            this.pnlLeft.Controls.Add(this.grpProgress);
            //this.pnlLeft.Controls.Add(this.grpMyStocks);
            this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLeft.Location = new System.Drawing.Point(0, 0);
            this.pnlLeft.Name = "pnlLeft";
            this.pnlLeft.Size = new System.Drawing.Size(200, 580);
            this.pnlLeft.TabIndex = 0;
            // 
            // grpProgress
            // 
            this.grpProgress.Controls.Add(this.lblCurrentStep);
            this.grpProgress.Controls.Add(this.lblStepProgress);
            this.grpProgress.Controls.Add(this.lblNextStep);
            this.grpProgress.Controls.Add(this.progressBar);
            this.grpProgress.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.grpProgress.Location = new System.Drawing.Point(10, 10);
            this.grpProgress.Name = "grpProgress";
            this.grpProgress.Size = new System.Drawing.Size(190, 150);
            this.grpProgress.TabIndex = 0;
            this.grpProgress.TabStop = false;
            this.grpProgress.Text = "진행 상황";
            // 
            // lblCurrentStep
            // 
            this.lblCurrentStep.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.lblCurrentStep.ForeColor = System.Drawing.Color.Blue;
            this.lblCurrentStep.Location = new System.Drawing.Point(10, 25);
            this.lblCurrentStep.Name = "lblCurrentStep";
            this.lblCurrentStep.Size = new System.Drawing.Size(260, 20);
            this.lblCurrentStep.TabIndex = 0;
            this.lblCurrentStep.Text = "🎯 1단계: 조건검색";
            // 
            // lblStepProgress
            // 
            this.lblStepProgress.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.lblStepProgress.Location = new System.Drawing.Point(10, 50);
            this.lblStepProgress.Name = "lblStepProgress";
            this.lblStepProgress.Size = new System.Drawing.Size(260, 20);
            this.lblStepProgress.TabIndex = 1;
            this.lblStepProgress.Text = "📊 대상: 47개";
            // 
            // lblNextStep
            // 
            this.lblNextStep.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.lblNextStep.Location = new System.Drawing.Point(10, 75);
            this.lblNextStep.Name = "lblNextStep";
            this.lblNextStep.Size = new System.Drawing.Size(260, 40);
            this.lblNextStep.TabIndex = 2;
            this.lblNextStep.Text = "✅ 완료: 1/4\n⏰ 다음: 1차분석";
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(10, 120);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(260, 20);
            this.progressBar.TabIndex = 3;
            this.progressBar.Value = 25;
            // 
            // grpMyStocks
            // 
           
            // 
            // lblMyStockCount
            // 
         
            // 
            // lstMyStocks
       
            // 
            // btnMyStockDetail
            // 
           
            // pnlRight
            // 
            this.pnlRight.Controls.Add(this.lblAnalysisDate);
            this.pnlRight.Controls.Add(this.lblConditionName);
            this.pnlRight.Controls.Add(this.dgvStockList);
            this.pnlRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRight.Location = new System.Drawing.Point(0, 0);
            this.pnlRight.Name = "pnlRight";
            this.pnlRight.Size = new System.Drawing.Size(1355, 580);
            this.pnlRight.TabIndex = 0;
            // 
            // lblAnalysisDate
            // 
            this.lblAnalysisDate.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold);
            this.lblAnalysisDate.ForeColor = System.Drawing.Color.DarkBlue;
            this.lblAnalysisDate.Location = new System.Drawing.Point(20, 20);
            this.lblAnalysisDate.Name = "lblAnalysisDate";
            this.lblAnalysisDate.Size = new System.Drawing.Size(300, 25);
            this.lblAnalysisDate.TabIndex = 0;
            this.lblAnalysisDate.Text = "📅 분석기준일: 로딩 중...";
            // 
            // lblConditionName
            // 
            this.lblConditionName.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold);
            this.lblConditionName.ForeColor = System.Drawing.Color.DarkGreen;
            this.lblConditionName.Location = new System.Drawing.Point(350, 20);
            this.lblConditionName.Name = "lblConditionName";
            this.lblConditionName.Size = new System.Drawing.Size(400, 25);
            this.lblConditionName.TabIndex = 1;
            this.lblConditionName.Text = "🔍 조건식: 001^급등주포착";
            // 
            // dgvStockList
            // 
            this.dgvStockList.AllowUserToAddRows = false;
            this.dgvStockList.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.AliceBlue;
            this.dgvStockList.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvStockList.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.None;
            this.dgvStockList.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgvStockList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("맑은 고딕", 9F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.LightSkyBlue;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvStockList.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvStockList.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.dgvStockList.GridColor = System.Drawing.Color.LightGray;
            this.dgvStockList.Location = new System.Drawing.Point(20, 60);
            this.dgvStockList.Name = "dgvStockList";
            this.dgvStockList.ReadOnly = true;
            this.dgvStockList.RowHeadersVisible = false;
            this.dgvStockList.RowHeadersWidth = 82;
            this.dgvStockList.RowTemplate.Height = 23;
            this.dgvStockList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvStockList.Size = new System.Drawing.Size(1320, 500);
            this.dgvStockList.TabIndex = 2;
            this.dgvStockList.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvStockList_CellContentClick);
            // 
            // statusStrip
            // 
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus});
            this.statusStrip.Location = new System.Drawing.Point(0, 958);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1600, 42);
            this.statusStrip.TabIndex = 5;
            this.statusStrip.Text = "statusStrip";
            // 
            // lblStatus
            // 
            this.lblStatus.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(1585, 32);
            this.lblStatus.Spring = true;
            this.lblStatus.Text = "시스템 준비 완료 - 키움 연결을 시작하세요";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnDebugDB
            // 
          
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(14F, 32F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1430, 800);
            this.Controls.Add(this.grpKiwoomConnection);
            this.Controls.Add(this.grpConditionSearch);
            this.Controls.Add(this.grpAnalysisSteps);
            this.Controls.Add(this.grpAccountInfo);
            this.Controls.Add(this.splitMain);
            this.Controls.Add(this.statusStrip);
            //this.Controls.Add(this.btnDebugDB);
            this.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "StockTrader3 - 키움 API + 3단계 분석 시스템";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.grpKiwoomConnection.ResumeLayout(false);
            this.grpConditionSearch.ResumeLayout(false);
            this.grpAnalysisSteps.ResumeLayout(false);
            this.grpAccountInfo.ResumeLayout(false);
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            this.pnlLeft.ResumeLayout(false);
            this.grpProgress.ResumeLayout(false);
            
            this.pnlRight.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvStockList)).EndInit();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        #region UI 컨트롤 멤버 변수들

        // 상단 4개 그룹박스
        private System.Windows.Forms.GroupBox grpKiwoomConnection;
        private System.Windows.Forms.GroupBox grpConditionSearch;
        private System.Windows.Forms.GroupBox grpAnalysisSteps;
        private System.Windows.Forms.GroupBox grpAccountInfo;

        // 메인 콘텐츠
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.Panel pnlLeft;
        private System.Windows.Forms.Panel pnlRight;

        // 좌측 패널
        private System.Windows.Forms.GroupBox grpProgress;
        //private System.Windows.Forms.GroupBox grpMyStocks;

        // 우측 패널 (핵심)
        private System.Windows.Forms.Label lblAnalysisDate;
        private System.Windows.Forms.Label lblConditionName;
        private System.Windows.Forms.DataGridView dgvStockList;

        // 키움연결 그룹
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Label lblConnectionStatus;
        private System.Windows.Forms.Label lblUserInfo;
        private System.Windows.Forms.RadioButton rbSimulation;     // 🆕 추가
        private System.Windows.Forms.RadioButton rbReal;           // 🆕 추가
        private System.Windows.Forms.ComboBox cmbAccounts;         // 🆕 추가
        private System.Windows.Forms.Label lblAccountSelect;       // 🆕 추가

        // 조건검색 그룹
        private System.Windows.Forms.ComboBox cmbConditions;
        private System.Windows.Forms.Button btnRefreshConditions;
        private System.Windows.Forms.Label lblSelectedCondition;

        // 분석단계 그룹
        private System.Windows.Forms.Button btnStep1;
        private System.Windows.Forms.Button btnStep2;
        private System.Windows.Forms.Button btnStep3;
        private System.Windows.Forms.Button btnStep4;
        private System.Windows.Forms.Button btnCollectHistoricalData; // 🆕 추가
        private System.Windows.Forms.Label lblStepsInfo;

        // 계좌정보 그룹
        private System.Windows.Forms.Label lblAccountSummary;
        private System.Windows.Forms.Label lblBalance;
        private System.Windows.Forms.Label lblTotalValue;
        private System.Windows.Forms.Label lblProfit;
        //private System.Windows.Forms.Button btnAccountDetail;

        // 진행상황 그룹
        private System.Windows.Forms.Label lblCurrentStep;
        private System.Windows.Forms.Label lblStepProgress;
        private System.Windows.Forms.Label lblNextStep;
        private System.Windows.Forms.ProgressBar progressBar;

        // 보유종목 그룹
        //private System.Windows.Forms.Label lblMyStockCount;
        //private System.Windows.Forms.ListBox lstMyStocks;
        //private System.Windows.Forms.Button btnMyStockDetail;

        // 상태바
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;

        // ✅ 임시 DB 확인 버튼 추가
        //private System.Windows.Forms.Button btnDebugDB;

        #endregion

        #region DataGridView 컬럼 설정

        /// <summary>
        /// 종목 리스트 DataGridView 컬럼 설정
        /// </summary>

        private void SetupDataGridViewColumns()
        {
            dgvStockList.Columns.Clear();
            dgvStockList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            // 종목명
            var colStockName = new DataGridViewTextBoxColumn();
            colStockName.Name = "StockName";
            colStockName.HeaderText = "종목명";
            colStockName.Width = 130;
            dgvStockList.Columns.Add(colStockName);

            // 현재가
            var colClosePrice = new DataGridViewTextBoxColumn();
            colClosePrice.Name = "ClosePrice";
            colClosePrice.HeaderText = "현재가";
            colClosePrice.Width = 70;
            colClosePrice.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colClosePrice.DefaultCellStyle.Format = "N0";
            dgvStockList.Columns.Add(colClosePrice);

            // 전일대비
            var colChangeAmount = new DataGridViewTextBoxColumn();
            colChangeAmount.Name = "ChangeAmount";
            colChangeAmount.HeaderText = "대비";
            colChangeAmount.Width = 75;
            colChangeAmount.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvStockList.Columns.Add(colChangeAmount);

            // 시가
            var colOpenPrice = new DataGridViewTextBoxColumn();
            colOpenPrice.Name = "OpenPrice";
            colOpenPrice.HeaderText = "시가";
            colOpenPrice.Width = 60;
            colOpenPrice.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colOpenPrice.DefaultCellStyle.Format = "N0";
            dgvStockList.Columns.Add(colOpenPrice);

            // 고가
            var colHighPrice = new DataGridViewTextBoxColumn();
            colHighPrice.Name = "HighPrice";
            colHighPrice.HeaderText = "고가";
            colHighPrice.Width = 60;
            colHighPrice.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colHighPrice.DefaultCellStyle.Format = "N0";
            dgvStockList.Columns.Add(colHighPrice);

            // 저가
            var colLowPrice = new DataGridViewTextBoxColumn();
            colLowPrice.Name = "LowPrice";
            colLowPrice.HeaderText = "저가";
            colLowPrice.Width = 60;
            colLowPrice.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colLowPrice.DefaultCellStyle.Format = "N0";
            dgvStockList.Columns.Add(colLowPrice);

            // 등락률
            var colChangeRate = new DataGridViewTextBoxColumn();
            colChangeRate.Name = "ChangeRate";
            colChangeRate.HeaderText = "등락률";
            colChangeRate.Width = 70;
            colChangeRate.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colChangeRate.DefaultCellStyle.Format = "F2";
            dgvStockList.Columns.Add(colChangeRate);

            // 거래량
            var colVolume = new DataGridViewTextBoxColumn();
            colVolume.Name = "Volume";
            colVolume.HeaderText = "거래량";
            colVolume.Width = 80;
            colVolume.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colVolume.DefaultCellStyle.Format = "N0";
            dgvStockList.Columns.Add(colVolume);

            // 진행상태
            var colStatus = new DataGridViewTextBoxColumn();
            colStatus.Name = "Status";
            colStatus.HeaderText = "진행상태";
            colStatus.Width = 100;
            colStatus.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvStockList.Columns.Add(colStatus);

            // 기술점수 (처음부터 표시)
            var colTechnicalScore = new DataGridViewTextBoxColumn();
            colTechnicalScore.Name = "TechnicalScore";
            colTechnicalScore.HeaderText = "기술점수";
            colTechnicalScore.Width = 80;
            colTechnicalScore.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvStockList.Columns.Add(colTechnicalScore);

            // 최종점수 (처음부터 표시)
            var colFinalScore = new DataGridViewTextBoxColumn();
            colFinalScore.Name = "FinalScore";
            colFinalScore.HeaderText = "최종점수";
            colFinalScore.Width = 80;
            colFinalScore.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvStockList.Columns.Add(colFinalScore);

            // 등급 (처음부터 표시)
            var colFinalGrade = new DataGridViewTextBoxColumn();
            colFinalGrade.Name = "FinalGrade";
            colFinalGrade.HeaderText = "등급";
            colFinalGrade.Width = 60;
            colFinalGrade.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvStockList.Columns.Add(colFinalGrade);

            // 매수가 (처음부터 표시)
            var colBuyPrice = new DataGridViewTextBoxColumn();
            colBuyPrice.Name = "BuyPrice";
            colBuyPrice.HeaderText = "매수가";
            colBuyPrice.Width = 70;
            colBuyPrice.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colBuyPrice.DefaultCellStyle.Format = "N0";
            colBuyPrice.DefaultCellStyle.ForeColor = Color.Blue;
            dgvStockList.Columns.Add(colBuyPrice);

            // 매도가 (처음부터 표시)
            var colSellPrice = new DataGridViewTextBoxColumn();
            colSellPrice.Name = "SellPrice";
            colSellPrice.HeaderText = "매도가";
            colSellPrice.Width = 70;
            colSellPrice.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colSellPrice.DefaultCellStyle.Format = "N0";
            colSellPrice.DefaultCellStyle.ForeColor = Color.Red;
            dgvStockList.Columns.Add(colSellPrice);

            // 손절가 (처음부터 표시)
            var colStopLossPrice = new DataGridViewTextBoxColumn();
            colStopLossPrice.Name = "StopLossPrice";
            colStopLossPrice.HeaderText = "손절가";
            colStopLossPrice.Width = 70;
            colStopLossPrice.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colStopLossPrice.DefaultCellStyle.Format = "N0";
            colStopLossPrice.DefaultCellStyle.ForeColor = Color.Gray;
            dgvStockList.Columns.Add(colStopLossPrice);
        }

       

        #endregion
    }
}