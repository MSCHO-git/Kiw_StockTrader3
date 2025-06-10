using System;
using System.Linq;
using System.Windows.Forms;
using HolidayManager.Managers;
using HolidayManager.Models;

namespace HolidayManager
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            InitializeForm();
        }

        /// <summary>
        /// 폼 초기화
        /// </summary>
        private void InitializeForm()
        {
            try
            {
                // 이벤트 핸들러 연결
                this.Load += MainForm_Load;
                btnInitYear.Click += BtnInitYear_Click;
                btnAddHoliday.Click += BtnAddHoliday_Click;
                btnDeleteHoliday.Click += BtnDeleteHoliday_Click;
                numYear.ValueChanged += NumYear_ValueChanged;
                cmbHolidayType.SelectedIndex = 0; // 기본값: "고정"

                // DataGridView 컬럼 설정
                SetupDataGridView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"폼 초기화 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// DataGridView 컬럼 설정
        /// </summary>
        private void SetupDataGridView()
        {
            dgvHolidays.AutoGenerateColumns = false;
            dgvHolidays.Columns.Clear();

            // 날짜 컬럼
            var colDate = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "HolidayDate",
                HeaderText = "날짜",
                Width = 100,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };
            dgvHolidays.Columns.Add(colDate);

            // 요일 컬럼
            var colDayOfWeek = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "DayOfWeek",
                HeaderText = "요일",
                Width = 80,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };
            dgvHolidays.Columns.Add(colDayOfWeek);

            // 휴일명 컬럼
            var colName = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "HolidayName",
                HeaderText = "휴일명",
                Width = 150
            };
            dgvHolidays.Columns.Add(colName);

            // 유형 컬럼
            var colType = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "HolidayType",
                HeaderText = "유형",
                Width = 80,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };
            dgvHolidays.Columns.Add(colType);

            // 비고 컬럼
            var colNotes = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Notes",
                HeaderText = "비고",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            };
            dgvHolidays.Columns.Add(colNotes);
        }

        /// <summary>
        /// 폼 로드 이벤트
        /// </summary>
        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                numYear.Value = DateTime.Now.Year; // 현재 연도로 설정
                UpdateTradingDayInfo();
                LoadHolidays();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"폼 로드 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 연도 변경 이벤트
        /// </summary>
        private void NumYear_ValueChanged(object sender, EventArgs e)
        {
            LoadHolidays();
        }

        /// <summary>
        /// 연도 초기화 버튼 클릭
        /// </summary>
        private void BtnInitYear_Click(object sender, EventArgs e)
        {
            try
            {
                int selectedYear = (int)numYear.Value;

                var result = MessageBox.Show(
                    $"{selectedYear}년 고정공휴일을 초기화하시겠습니까?\n(기존 데이터는 삭제됩니다)",
                    "연도 초기화",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // 기존 휴일 삭제
                    int deletedCount = HolidayManagerService.ClearHolidays(selectedYear);

                    // 고정 공휴일 추가
                    HolidayManagerService.InitializeFixedHolidays(selectedYear);

                    MessageBox.Show(
                        $"{selectedYear}년 초기화 완료!\n삭제: {deletedCount}개\n고정공휴일 8개 추가됨\n\n음력공휴일(설날, 추석 등)은 수동으로 추가해주세요.",
                        "초기화 완료",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    LoadHolidays();
                    UpdateTradingDayInfo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"연도 초기화 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 휴일 추가 버튼 클릭
        /// </summary>
        private void BtnAddHoliday_Click(object sender, EventArgs e)
        {
            try
            {
                // 입력값 검증
                if (string.IsNullOrWhiteSpace(txtHolidayName.Text))
                {
                    MessageBox.Show("휴일명을 입력해주세요.", "입력 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtHolidayName.Focus();
                    return;
                }

                if (cmbHolidayType.SelectedIndex == -1)
                {
                    MessageBox.Show("휴일 유형을 선택해주세요.", "입력 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cmbHolidayType.Focus();
                    return;
                }

                // 휴일 추가
                HolidayManagerService.AddHoliday(
                    dtpHolidayDate.Value.Date,
                    txtHolidayName.Text.Trim(),
                    cmbHolidayType.Text,
                    string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text.Trim()
                );

                MessageBox.Show($"휴일이 추가되었습니다.\n{dtpHolidayDate.Value:yyyy-MM-dd} ({txtHolidayName.Text})",
                    "추가 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 입력 필드 초기화
                ClearInputFields();

                // 목록 새로고침
                LoadHolidays();
                UpdateTradingDayInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"휴일 추가 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 휴일 삭제 버튼 클릭
        /// </summary>
        private void BtnDeleteHoliday_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvHolidays.SelectedRows.Count == 0)
                {
                    MessageBox.Show("삭제할 휴일을 선택해주세요.", "선택 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var selectedHoliday = (Holiday)dgvHolidays.SelectedRows[0].DataBoundItem;

                var result = MessageBox.Show(
                    $"선택한 휴일을 삭제하시겠습니까?\n{selectedHoliday.HolidayDate} ({selectedHoliday.HolidayName})",
                    "휴일 삭제",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    int deletedCount = HolidayManagerService.RemoveHoliday(selectedHoliday.Date);

                    if (deletedCount > 0)
                    {
                        MessageBox.Show("휴일이 삭제되었습니다.", "삭제 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadHolidays();
                        UpdateTradingDayInfo();
                    }
                    else
                    {
                        MessageBox.Show("삭제할 휴일을 찾을 수 없습니다.", "삭제 실패", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"휴일 삭제 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 휴일 목록 로드
        /// </summary>
        private void LoadHolidays()
        {
            try
            {
                int selectedYear = (int)numYear.Value;
                var holidays = HolidayManagerService.GetHolidays(selectedYear);

                dgvHolidays.DataSource = holidays;

                // 휴일 개수 업데이트
                lblHolidayCount.Text = $"등록된 휴일 수: {holidays.Count}개";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"휴일 목록 로드 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 영업일 정보 업데이트
        /// </summary>
        private void UpdateTradingDayInfo()
        {
            try
            {
                DateTime today = DateTime.Today;

                // 오늘 날짜 정보
                lblToday.Text = $"오늘: {today:yyyy-MM-dd} ({TradingDayManager.GetKoreanDayOfWeek(today)})";

                // 영업일 여부
                bool isBusinessDay = TradingDayManager.IsBusinessDay(today);
                string businessDayStatus = TradingDayManager.GetBusinessDayStatus(today);
                lblBusinessDay.Text = $"영업일 여부: {businessDayStatus}";
                lblBusinessDay.ForeColor = isBusinessDay ? System.Drawing.Color.Blue : System.Drawing.Color.Red;

                // 마지막 거래일
                DateTime lastTradingDay = TradingDayManager.GetLastTradingDate();
                lblLastTradingDay.Text = $"마지막 거래일: {lastTradingDay:yyyy-MM-dd} ({TradingDayManager.GetKoreanDayOfWeek(lastTradingDay)})";

                // 다음 영업일
                DateTime nextTradingDay = TradingDayManager.GetNextBusinessDay(today);
                lblNextTradingDay.Text = $"다음 영업일: {nextTradingDay:yyyy-MM-dd} ({TradingDayManager.GetKoreanDayOfWeek(nextTradingDay)})";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"영업일 정보 업데이트 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 입력 필드 초기화
        /// </summary>
        private void ClearInputFields()
        {
            txtHolidayName.Clear();
            txtNotes.Clear();
            cmbHolidayType.SelectedIndex = 0; // "고정"으로 초기화
            dtpHolidayDate.Value = DateTime.Today;
        }
    }
}