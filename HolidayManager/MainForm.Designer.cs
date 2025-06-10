namespace HolidayManager
{
    partial class MainForm
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            // DPI 스케일링 비활성화
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnDeleteHoliday = new System.Windows.Forms.Button();
            this.btnAddHoliday = new System.Windows.Forms.Button();
            this.btnInitYear = new System.Windows.Forms.Button();
            this.numYear = new System.Windows.Forms.NumericUpDown();
            this.lblYear = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txtNotes = new System.Windows.Forms.TextBox();
            this.lblNotes = new System.Windows.Forms.Label();
            this.cmbHolidayType = new System.Windows.Forms.ComboBox();
            this.lblType = new System.Windows.Forms.Label();
            this.txtHolidayName = new System.Windows.Forms.TextBox();
            this.lblName = new System.Windows.Forms.Label();
            this.dtpHolidayDate = new System.Windows.Forms.DateTimePicker();
            this.lblDate = new System.Windows.Forms.Label();
            this.dgvHolidays = new System.Windows.Forms.DataGridView();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.lblHolidayCount = new System.Windows.Forms.Label();
            this.lblNextTradingDay = new System.Windows.Forms.Label();
            this.lblLastTradingDay = new System.Windows.Forms.Label();
            this.lblBusinessDay = new System.Windows.Forms.Label();
            this.lblToday = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numYear)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvHolidays)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnDeleteHoliday);
            this.groupBox1.Controls.Add(this.btnAddHoliday);
            this.groupBox1.Controls.Add(this.btnInitYear);
            this.groupBox1.Controls.Add(this.numYear);
            this.groupBox1.Controls.Add(this.lblYear);
            this.groupBox1.Location = new System.Drawing.Point(26, 22);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.groupBox1.Size = new System.Drawing.Size(1850, 111);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "📅 휴일 관리";
            // 
            // btnDeleteHoliday
            // 
            this.btnDeleteHoliday.Location = new System.Drawing.Point(650, 39);
            this.btnDeleteHoliday.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.btnDeleteHoliday.Name = "btnDeleteHoliday";
            this.btnDeleteHoliday.Size = new System.Drawing.Size(150, 46);
            this.btnDeleteHoliday.TabIndex = 4;
            this.btnDeleteHoliday.Text = "휴일 삭제";
            this.btnDeleteHoliday.UseVisualStyleBackColor = true;
            // 
            // btnAddHoliday
            // 
            this.btnAddHoliday.Location = new System.Drawing.Point(500, 39);
            this.btnAddHoliday.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.btnAddHoliday.Name = "btnAddHoliday";
            this.btnAddHoliday.Size = new System.Drawing.Size(150, 46);
            this.btnAddHoliday.TabIndex = 3;
            this.btnAddHoliday.Text = "휴일 추가";
            this.btnAddHoliday.UseVisualStyleBackColor = true;
            // 
            // btnInitYear
            // 
            this.btnInitYear.Location = new System.Drawing.Point(334, 39);
            this.btnInitYear.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.btnInitYear.Name = "btnInitYear";
            this.btnInitYear.Size = new System.Drawing.Size(150, 46);
            this.btnInitYear.TabIndex = 2;
            this.btnInitYear.Text = "연도 초기화";
            this.btnInitYear.UseVisualStyleBackColor = true;
            // 
            // numYear
            // 
            this.numYear.Location = new System.Drawing.Point(126, 42);
            this.numYear.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.numYear.Maximum = new decimal(new int[] {
            2030,
            0,
            0,
            0});
            this.numYear.Minimum = new decimal(new int[] {
            2020,
            0,
            0,
            0});
            this.numYear.Name = "numYear";
            this.numYear.Size = new System.Drawing.Size(173, 35);
            this.numYear.TabIndex = 1;
            this.numYear.Value = new decimal(new int[] {
            2025,
            0,
            0,
            0});
            // 
            // lblYear
            // 
            this.lblYear.AutoSize = true;
            this.lblYear.Location = new System.Drawing.Point(32, 46);
            this.lblYear.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblYear.Name = "lblYear";
            this.lblYear.Size = new System.Drawing.Size(66, 24);
            this.lblYear.TabIndex = 0;
            this.lblYear.Text = "연도:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.txtNotes);
            this.groupBox2.Controls.Add(this.lblNotes);
            this.groupBox2.Controls.Add(this.cmbHolidayType);
            this.groupBox2.Controls.Add(this.lblType);
            this.groupBox2.Controls.Add(this.txtHolidayName);
            this.groupBox2.Controls.Add(this.lblName);
            this.groupBox2.Controls.Add(this.dtpHolidayDate);
            this.groupBox2.Controls.Add(this.lblDate);
            this.groupBox2.Location = new System.Drawing.Point(26, 144);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.groupBox2.Size = new System.Drawing.Size(1850, 185);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "📝 새 휴일 추가";
            // 
            // txtNotes
            // 
            this.txtNotes.Location = new System.Drawing.Point(126, 98);
            this.txtNotes.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.txtNotes.Multiline = true;
            this.txtNotes.Name = "txtNotes";
            this.txtNotes.Size = new System.Drawing.Size(1100, 61);
            this.txtNotes.TabIndex = 7;
            // 
            // lblNotes
            // 
            this.lblNotes.AutoSize = true;
            this.lblNotes.Location = new System.Drawing.Point(32, 102);
            this.lblNotes.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblNotes.Name = "lblNotes";
            this.lblNotes.Size = new System.Drawing.Size(66, 24);
            this.lblNotes.TabIndex = 6;
            this.lblNotes.Text = "비고:";
            // 
            // cmbHolidayType
            // 
            this.cmbHolidayType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbHolidayType.FormattingEnabled = true;
            this.cmbHolidayType.Items.AddRange(new object[] {
            "고정",
            "대체",
            "임시",
            "연휴",
            "기타"});
            this.cmbHolidayType.Location = new System.Drawing.Point(1014, 42);
            this.cmbHolidayType.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.cmbHolidayType.Name = "cmbHolidayType";
            this.cmbHolidayType.Size = new System.Drawing.Size(212, 32);
            this.cmbHolidayType.TabIndex = 5;
            // 
            // lblType
            // 
            this.lblType.AutoSize = true;
            this.lblType.Location = new System.Drawing.Point(921, 46);
            this.lblType.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(66, 24);
            this.lblType.TabIndex = 4;
            this.lblType.Text = "유형:";
            // 
            // txtHolidayName
            // 
            this.txtHolidayName.Location = new System.Drawing.Point(552, 42);
            this.txtHolidayName.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.txtHolidayName.Name = "txtHolidayName";
            this.txtHolidayName.Size = new System.Drawing.Size(320, 35);
            this.txtHolidayName.TabIndex = 3;
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(433, 46);
            this.lblName.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(90, 24);
            this.lblName.TabIndex = 2;
            this.lblName.Text = "휴일명:";
            // 
            // dtpHolidayDate
            // 
            this.dtpHolidayDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpHolidayDate.Location = new System.Drawing.Point(126, 42);
            this.dtpHolidayDate.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.dtpHolidayDate.Name = "dtpHolidayDate";
            this.dtpHolidayDate.Size = new System.Drawing.Size(255, 35);
            this.dtpHolidayDate.TabIndex = 1;
            // 
            // lblDate
            // 
            this.lblDate.AutoSize = true;
            this.lblDate.Location = new System.Drawing.Point(32, 46);
            this.lblDate.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblDate.Name = "lblDate";
            this.lblDate.Size = new System.Drawing.Size(66, 24);
            this.lblDate.TabIndex = 0;
            this.lblDate.Text = "날짜:";
            // 
            // dgvHolidays
            // 
            // dgvHolidays
            // 
            this.dgvHolidays.AllowUserToAddRows = false;
            this.dgvHolidays.AllowUserToDeleteRows = false;
            this.dgvHolidays.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvHolidays.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvHolidays.Location = new System.Drawing.Point(26, 340);
            this.dgvHolidays.MultiSelect = false;
            this.dgvHolidays.Name = "dgvHolidays";
            this.dgvHolidays.ReadOnly = true;
            this.dgvHolidays.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvHolidays.Size = new System.Drawing.Size(1850, 200);
            this.dgvHolidays.TabIndex = 2;
            // CellContentClick 이벤트 줄은 삭제됨


           
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.lblHolidayCount);
            this.groupBox3.Controls.Add(this.lblNextTradingDay);
            this.groupBox3.Controls.Add(this.lblLastTradingDay);
            this.groupBox3.Controls.Add(this.lblBusinessDay);
            this.groupBox3.Controls.Add(this.lblToday);
            this.groupBox3.Location = new System.Drawing.Point(830, 40);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.groupBox3.Size = new System.Drawing.Size(600,120);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "🎯 영업일 정보";
            // 
            // lblHolidayCount
            // 
            this.lblHolidayCount.AutoSize = true;
            this.lblHolidayCount.Location = new System.Drawing.Point(370, 90);
            this.lblHolidayCount.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblHolidayCount.Name = "lblHolidayCount";
            this.lblHolidayCount.Size = new System.Drawing.Size(178, 24);
            this.lblHolidayCount.TabIndex = 4;
            this.lblHolidayCount.Text = "등록된 휴일 수:";
            // 
            // lblNextTradingDay
            // 
            this.lblNextTradingDay.AutoSize = true;
            this.lblNextTradingDay.Location = new System.Drawing.Point(370, 70);
            this.lblNextTradingDay.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblNextTradingDay.Name = "lblNextTradingDay";
            this.lblNextTradingDay.Size = new System.Drawing.Size(146, 24);
            this.lblNextTradingDay.TabIndex = 3;
            this.lblNextTradingDay.Text = "다음 영업일:";
            // 
            // lblLastTradingDay
            // 
            this.lblLastTradingDay.AutoSize = true;
            this.lblLastTradingDay.Location = new System.Drawing.Point(32, 92);
            this.lblLastTradingDay.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblLastTradingDay.Name = "lblLastTradingDay";
            this.lblLastTradingDay.Size = new System.Drawing.Size(170, 24);
            this.lblLastTradingDay.TabIndex = 2;
            this.lblLastTradingDay.Text = "마지막 거래일:";
            // 
            // lblBusinessDay
            // 
            this.lblBusinessDay.AutoSize = true;
            this.lblBusinessDay.Location = new System.Drawing.Point(370, 46);
            this.lblBusinessDay.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblBusinessDay.Name = "lblBusinessDay";
            this.lblBusinessDay.Size = new System.Drawing.Size(146, 24);
            this.lblBusinessDay.TabIndex = 1;
            this.lblBusinessDay.Text = "영업일 여부:";
            // 
            // lblToday
            // 
            this.lblToday.AutoSize = true;
            this.lblToday.Location = new System.Drawing.Point(32, 46);
            this.lblToday.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblToday.Name = "lblToday";
            this.lblToday.Size = new System.Drawing.Size(66, 24);
            this.lblToday.TabIndex = 0;
            this.lblToday.Text = "오늘:";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1500,750);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.dgvHolidays);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "한국 증시 휴일 관리 시스템";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numYear)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvHolidays)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnDeleteHoliday;
        private System.Windows.Forms.Button btnAddHoliday;
        private System.Windows.Forms.Button btnInitYear;
        private System.Windows.Forms.NumericUpDown numYear;
        private System.Windows.Forms.Label lblYear;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox txtNotes;
        private System.Windows.Forms.Label lblNotes;
        private System.Windows.Forms.ComboBox cmbHolidayType;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.TextBox txtHolidayName;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.DateTimePicker dtpHolidayDate;
        private System.Windows.Forms.Label lblDate;
        private System.Windows.Forms.DataGridView dgvHolidays;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label lblHolidayCount;
        private System.Windows.Forms.Label lblNextTradingDay;
        private System.Windows.Forms.Label lblLastTradingDay;
        private System.Windows.Forms.Label lblBusinessDay;
        private System.Windows.Forms.Label lblToday;
    }
}