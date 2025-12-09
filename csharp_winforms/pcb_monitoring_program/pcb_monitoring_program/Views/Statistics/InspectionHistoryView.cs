using ComponentFactory.Krypton.Toolkit;
using pcb_monitoring_program;
using pcb_monitoring_program.Views.Statistics;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using pcb_monitoring_program.DatabaseManager.Repositories;

namespace pcb_monitoring_program.Views.Statistics
{
    public partial class InspectionHistoryView : UserControl
    {
        private bool _isInternalDateUpdate = false;  // 👉 날짜 업데이트 감지용 플래그 추가

        private bool _isInternalUpdate = false;

        public event EventHandler OpenDetailsRequested;

        private readonly InspectionHistoryRepository _repo = new();

        public InspectionHistoryView()
        {
            InitializeComponent();

            // 🔹 날짜 바뀔 때마다 자동으로 그리드 새로고침
            DTP_IH_StartDate.ValueChanged += DateRange_ValueChanged;
            DTP_IH_EndDate.ValueChanged += DateRange_ValueChanged;

            // 🔹 그리드 선택 테두리 강조용 이벤트
            DGV_IH_result.CellPainting += DGV_IH_result_CellPainting;
            DGV_IH_result.SelectionChanged += DGV_IH_result_SelectionChanged;

            // 🔹 행 더블클릭 → 상세 폼 열기
            DGV_IH_result.CellDoubleClick += DGV_IH_result_CellDoubleClick;
        }

        private void ApplyButtonStyle(Control parent)
        {
            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl is Button btn)
                {
                    btn.BackColor = Color.FromArgb(64, 64, 64);
                    btn.ForeColor = Color.White;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                    btn.Cursor = Cursors.Hand;
                }
                else if (ctrl.HasChildren)
                {
                    ApplyButtonStyle(ctrl); // 내부 컨트롤(패널 등) 재귀 호출
                }
            }
        }

        // 🔹 날짜 범위가 바뀔 때마다 호출되는 공통 핸들러
        private void DateRange_ValueChanged(object sender, EventArgs e)
        {
            // 코드에서 내부적으로 날짜 바꾸는 중이면 이벤트 무시
            if (_isInternalDateUpdate)
                return;

            if (DTP_IH_StartDate.Value.Date > DTP_IH_EndDate.Value.Date)
            {
                DTP_IH_EndDate.Value = DTP_IH_StartDate.Value.Date;
            }

            LoadInspectionHistoryGridByDateRange();
        }

        // 🔹 기간 필터를 적용해서 검사 이력 로드
        private void LoadInspectionHistoryGridByDateRange()
        {
            DataTable dt = _repo.GetAllInspectionHistory();

            DateTime from = DTP_IH_StartDate.Value.Date;
            DateTime to = DTP_IH_EndDate.Value.Date.AddDays(1).AddTicks(-1);

            // ---- 컬럼 자동 감지 ----
            string timeColumn = dt.Columns.Contains("검사 시각") ? "검사 시각"
                              : dt.Columns.Contains("inspection_time") ? "inspection_time"
                              : dt.Columns.Contains("inspectionTime") ? "inspectionTime"
                              : dt.Columns.Contains("time") ? "time"
                              : null;

            string defectColumn = dt.Columns.Contains("불량 유형") ? "불량 유형"
                                : dt.Columns.Contains("defect_type") ? "defect_type"
                                : dt.Columns.Contains("defect") ? "defect"
                                : null;

            string productColumn = dt.Columns.Contains("제품 코드") ? "제품 코드"
                                 : dt.Columns.Contains("product_code") ? "product_code"
                                 : dt.Columns.Contains("product") ? "product"
                                 : dt.Columns.Contains("serial_number") ? "serial_number"
                                 : null;

            // ---- 불량 유형 필터 준비 ----
            var defectTypes = new List<string>();
            if (CB_IH_DefectType_Normal.Checked) defectTypes.Add("정상");
            if (CB_IH_DefectType_ComponentDefect.Checked) defectTypes.Add("부품불량");
            if (CB_IH_DefectType_SolderingDefect.Checked) defectTypes.Add("S/N 불량");
            if (CB_IH_DefectType_Scrap.Checked) defectTypes.Add("폐기");

            bool useDefectFilter = defectTypes.Count > 0 && !CB_IH_DefectType_All.Checked && defectColumn != null;

            // ---- product_code 필터 준비 (기존 카메라 체크박스 재활용) ----
            List<string> selectedProductCodes = new List<string>();

            string GetCheckboxValue(CheckBox cb)
            {
                if (cb == null) return null;
                if (cb.Tag != null) return cb.Tag.ToString();
                return cb.Text;
            }

            try
            {
                if (CB_IH_CameraID_CAM01.Checked) selectedProductCodes.Add(GetCheckboxValue(CB_IH_CameraID_CAM01));
                if (CB_IH_CameraID_CAM02.Checked) selectedProductCodes.Add(GetCheckboxValue(CB_IH_CameraID_CAM02));
                if (CB_IH_CameraID_CAM03.Checked) selectedProductCodes.Add(GetCheckboxValue(CB_IH_CameraID_CAM03));
            }
            catch
            {
                // 체크박스 컨트롤이 다르면 안전하게 무시
            }

            // 정규화: 대문자, 공백 제거(비교를 쉽게)
            selectedProductCodes = selectedProductCodes
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim().ToUpperInvariant())
                .Distinct()
                .ToList();

            bool useProductFilter = selectedProductCodes.Count > 0 && !CB_IH_CameraID_All.Checked && productColumn != null;

            // 디버그 출력(개발 중 확인용)
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"[Debug] timeColumn={timeColumn}, defectColumn={defectColumn}, productColumn={productColumn}");
            System.Diagnostics.Debug.WriteLine($"[Debug] useDefectFilter={useDefectFilter}, useProductFilter={useProductFilter}");
            System.Diagnostics.Debug.WriteLine($"[Debug] selectedProductCodes=[{string.Join(",", selectedProductCodes)}]");
#endif

            // ---- 필터링 수행 ----
            var filteredRows = dt.AsEnumerable().Where(row =>
            {
                // 시간 필터 (만약 timeColumn이 없으면 시간 필터 생략)
                if (!string.IsNullOrEmpty(timeColumn))
                {
                    var timeObj = row[timeColumn];
                    if (timeObj == DBNull.Value) return false;

                    DateTime t;
                    try { t = Convert.ToDateTime(timeObj); }
                    catch { return false; }

                    if (t < from || t > to) return false;
                }

                // 불량 유형 필터
                if (useDefectFilter)
                {
                    var val = row[defectColumn];
                    string defect = val == DBNull.Value ? string.Empty : Convert.ToString(val);

                    // 정규화해서 비교 (공백 제거, 대문자)
                    defect = defect?.Trim().ToUpperInvariant() ?? string.Empty;
                    var normalizedDefs = defectTypes.Select(d => d.Trim().ToUpperInvariant()).ToList();

                    if (!normalizedDefs.Contains(defect))
                        return false;
                }

                // 제품 코드 필터
                if (useProductFilter)
                {
                    var val = row[productColumn];
                    string prod = val == DBNull.Value ? string.Empty : Convert.ToString(val);
                    prod = prod?.Trim().ToUpperInvariant() ?? string.Empty;

                    // 비어있거나 리스트에 없으면 제외
                    if (string.IsNullOrEmpty(prod) || !selectedProductCodes.Contains(prod))
                        return false;
                }

                return true;
            });

            DataTable view;
            if (filteredRows.Any()) view = filteredRows.CopyToDataTable();
            else view = dt.Clone();

            // 바인딩
            DGV_IH_result.DataSource = null;
            DGV_IH_result.AutoGenerateColumns = true;
            DGV_IH_result.DataSource = view;
            DGV_IH_result.Refresh();

            DGV_IH_result.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            DGV_IH_result.RowHeadersVisible = false;

            if (DGV_IH_result.Columns.Contains("검사 시각"))
                DGV_IH_result.Columns["검사 시각"].FillWeight = 180;
            else if (!string.IsNullOrEmpty(timeColumn) && DGV_IH_result.Columns.Contains(timeColumn))
                DGV_IH_result.Columns[timeColumn].FillWeight = 180;
        }

        // ⚠ 현재는 사용하지 않음 (전체 로드용)
        // 필요 시 전체 조회 버튼 등에서 활용 가능
        private void LoadInspectionHistoryGrid()
        {
            // DB에서 inspections 전체 조회
            DataTable dt = _repo.GetAllInspectionHistory();

            // 그리드에 바인딩
            DGV_IH_result.DataSource = dt;

            // 보기 좋게 설정
            DGV_IH_result.AutoGenerateColumns = true;
            DGV_IH_result.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            DGV_IH_result.RowHeadersVisible = false;  // 🔸 맨 왼쪽 삼각형 제거

            // 🔹 검사 시각 컬럼 폭 늘리기
            if (DGV_IH_result.Columns.Contains("검사 시각"))
                DGV_IH_result.Columns["검사 시각"].FillWeight = 180; // 기본 100보다 크게

            // 🔹 여기서 "전체 기간" 찾아서 DateTimePicker에 반영
            if (dt.Rows.Count > 0 && dt.Columns.Contains("검사 시각"))
            {
                var minTime = dt.AsEnumerable()
                                .Min(row => row.Field<DateTime>("검사 시각"));
                var maxTime = dt.AsEnumerable()
                                .Max(row => row.Field<DateTime>("검사 시각"));

                // 날짜만 사용 (시분초 제거)
                var minDate = minTime.Date;
                var maxDate = maxTime.Date;

                // 내부 업데이트 플래그 설정 (ValueChanged 이벤트 막기)
                _isInternalDateUpdate = true;

                // MinDate/MaxDate 범위를 넘어가면 클램프
                if (minDate < DTP_IH_StartDate.MinDate) minDate = DTP_IH_StartDate.MinDate;
                if (maxDate > DTP_IH_EndDate.MaxDate) maxDate = DTP_IH_EndDate.MaxDate;

                DTP_IH_StartDate.Value = minDate;
                DTP_IH_EndDate.Value = maxDate;

                _isInternalDateUpdate = false;
            }
        }

        private void InspectionHistoryView_Load(object sender, EventArgs e)
        {
            UiStyleHelper.MakeRoundedPanel(cardfilter, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardSearchresult, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardday, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardDefectType, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardCameraID, radius: 16, back: Color.FromArgb(44, 44, 44));

            UiStyleHelper.AddShadowRoundedPanel(cardfilter, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardSearchresult, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardday, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardDefectType, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardCameraID, 16);

            UiStyleHelper.MakeRoundedButton(btn_filterSearch, 24);
            UiStyleHelper.MakeRoundedButton(btn_Last7DaysSearch, 24);
            UiStyleHelper.MakeRoundedButton(btn_ThisMonthSearch, 24);
            UiStyleHelper.MakeRoundedButton(btn_TodaySearch, 24);
            UiStyleHelper.MakeRoundedButton(btn_AllSearch, 24);

            UiStyleHelper.AttachDropShadow(btn_filterSearch, radius: 16, offset: 4);
            UiStyleHelper.AttachDropShadow(btn_Last7DaysSearch, radius: 16, offset: 4);
            UiStyleHelper.AttachDropShadow(btn_ThisMonthSearch, radius: 16, offset: 4);
            UiStyleHelper.AttachDropShadow(btn_TodaySearch, radius: 16, offset: 4);
            UiStyleHelper.AttachDropShadow(btn_AllSearch, radius: 16, offset: 4);

            // 🔹 DataGridView 컬럼 자동 생성 (DB 컬럼 그대로 출력)
            DGV_IH_result.AutoGenerateColumns = true;

            // ✅ 행 단위 선택 + 단일 선택
            DGV_IH_result.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DGV_IH_result.MultiSelect = false;

            // 🔹 이 한 줄로 모든 버튼 스타일 적용
            ApplyButtonStyle(this);

            // 🔹 DateTimePicker 기본값: 오늘 하루
            var today = DateTime.Today;

            // MinDate/MaxDate 범위 안에 맞게 조정 (있으면)
            if (today < DTP_IH_StartDate.MinDate) today = DTP_IH_StartDate.MinDate;
            if (today > DTP_IH_StartDate.MaxDate) today = DTP_IH_StartDate.MaxDate;

            DTP_IH_StartDate.Value = today;
            DTP_IH_EndDate.Value = today;

            // 🔹 화면 켜자마자 '오늘 하루치' 검사 이력 로드
            LoadInspectionHistoryGridByDateRange();

            // 수정 막기
            DGV_IH_result.ReadOnly = true;
        }

        // ✅ 선택된 행에 노란 테두리 그리기
        private void DGV_IH_result_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            var grid = (KryptonDataGridView)sender; // 그냥 DataGridView면 DataGridView로 바꿔도 됨

            // 유효한 데이터 셀 + 선택된 행만 처리
            if (e.RowIndex >= 0 && grid.Rows[e.RowIndex].Selected)
            {
                Color highlightColor = Color.FromArgb(255, 180, 0); // 노란색
                int borderWidth = 2;

                // 기본 그리기(내용/배경) 먼저, 기본 테두리는 빼고 그림
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.Border);

                using (Pen p = new Pen(highlightColor, borderWidth))
                {
                    Rectangle rect = e.CellBounds;

                    // 🔼 위쪽 테두리 (위 행이 선택 안 되어 있을 때만)
                    if (e.RowIndex == 0 || !grid.Rows[e.RowIndex - 1].Selected)
                    {
                        e.Graphics.DrawLine(p, rect.Left, rect.Top, rect.Right, rect.Top);
                    }

                    // 🔽 아래쪽 테두리 (아래 행이 선택 안 되어 있을 때만)
                    if (e.RowIndex == grid.RowCount - 1 || !grid.Rows[e.RowIndex + 1].Selected)
                    {
                        e.Graphics.DrawLine(p,
                            rect.Left,
                            rect.Bottom - borderWidth / 2,
                            rect.Right,
                            rect.Bottom - borderWidth / 2);
                    }

                    // ◀ 왼쪽 테두리 (첫 번째 컬럼일 때만)
                    if (e.ColumnIndex == 0)
                    {
                        e.Graphics.DrawLine(p, rect.Left, rect.Top, rect.Left, rect.Bottom);
                    }

                    // ▶ 오른쪽 테두리 (마지막 컬럼일 때만)
                    if (e.ColumnIndex == grid.ColumnCount - 1)
                    {
                        e.Graphics.DrawLine(p,
                            rect.Right - borderWidth / 2,
                            rect.Top,
                            rect.Right - borderWidth / 2,
                            rect.Bottom);
                    }
                }

                // 기본 그리기 종료 (우리가 다 했다고 알림)
                e.Handled = true;
            }
        }

        // ✅ 선택 바뀔 때마다 다시 그리기
        private void DGV_IH_result_SelectionChanged(object sender, EventArgs e)
        {
            DGV_IH_result.Invalidate();   // 또는 DGV_IH_result.Refresh();
        }

        // ✅ 행 더블클릭 → 클릭한 행 DataRow 한 줄 상세 폼으로 전달
        private void DGV_IH_result_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // 헤더 더블클릭 방지
            if (e.RowIndex < 0)
                return;

            var grid = DGV_IH_result;

            if (grid.DataSource is DataTable dt)
            {
                var rowView = grid.Rows[e.RowIndex].DataBoundItem as DataRowView;
                if (rowView == null) return;

                var row = rowView.Row;   // 여기가 진짜 DataRow

                using (var form = new InspectionHistoryDetailForm(row))
                {
                    form.StartPosition = FormStartPosition.CenterParent;
                    form.ShowDialog();
                }
            }
        }

        //////////////////////////////////// 불량 유형 : 네 개의 개별 체크박스 상태가 변경될 때마다 All 체크박스 상태 업데이트
        private void UpdateDefectTypeAllState()
        {
            if (_isInternalUpdate) return;

            _isInternalUpdate = true;

            CB_IH_DefectType_All.Checked =
                CB_IH_DefectType_Normal.Checked &&
                CB_IH_DefectType_ComponentDefect.Checked &&
                CB_IH_DefectType_SolderingDefect.Checked &&
                CB_IH_DefectType_Scrap.Checked;

            _isInternalUpdate = false;
        }

        private void CB_DefectType_All_CheckedChanged(object sender, EventArgs e)
        {
            if (_isInternalUpdate) return;

            bool isChecked = CB_IH_DefectType_All.Checked;

            _isInternalUpdate = true;

            CB_IH_DefectType_Normal.Checked = isChecked;
            CB_IH_DefectType_ComponentDefect.Checked = isChecked;
            CB_IH_DefectType_SolderingDefect.Checked = isChecked;
            CB_IH_DefectType_Scrap.Checked = isChecked;

            _isInternalUpdate = false;
        }

        private void CB_DefectType_Normal_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDefectTypeAllState();
            if (!_isInternalUpdate)
                LoadInspectionHistoryGridByDateRange();
        }

        private void CB_DefectType_ComponentDefect_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDefectTypeAllState();
            if (!_isInternalUpdate)
                LoadInspectionHistoryGridByDateRange();
        }

        private void CB_DefectType_SolderingDefect_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDefectTypeAllState();
            if (!_isInternalUpdate)
                LoadInspectionHistoryGridByDateRange();
        }

        private void CB_DefectType_Scrap_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDefectTypeAllState();
            if (!_isInternalUpdate)
                LoadInspectionHistoryGridByDateRange();
        }

        //////////////////////////////////////////// 카메라 ID : 네 개의 개별 체크박스 상태가 변경될 때마다 All 체크박스 상태 업데이트
        private void UpdateCameraIDAllState()
        {
            if (_isInternalUpdate) return;

            _isInternalUpdate = true;

            CB_IH_CameraID_All.Checked =
                CB_IH_CameraID_CAM01.Checked &&
                CB_IH_CameraID_CAM02.Checked &&
                CB_IH_CameraID_CAM03.Checked;

            _isInternalUpdate = false;
        }

        private void CB_CameraID_All_CheckedChanged(object sender, EventArgs e)
        {
            if (_isInternalUpdate) return;

            bool isChecked = CB_IH_CameraID_All.Checked;

            _isInternalUpdate = true;

            CB_IH_CameraID_CAM01.Checked = isChecked;
            CB_IH_CameraID_CAM02.Checked = isChecked;
            CB_IH_CameraID_CAM03.Checked = isChecked;

            _isInternalUpdate = false;

            LoadInspectionHistoryGridByDateRange();
        }

        private void CB_CameraID_CAM01_CheckedChanged(object sender, EventArgs e)
        {
            UpdateCameraIDAllState();
            if (!_isInternalUpdate)
                LoadInspectionHistoryGridByDateRange();
        }

        private void CB_CameraID_CAM02_CheckedChanged(object sender, EventArgs e)
        {
            UpdateCameraIDAllState();
            if (!_isInternalUpdate)
                LoadInspectionHistoryGridByDateRange();
        }

        private void CB_CameraID_CAM03_CheckedChanged(object sender, EventArgs e)
        {
            UpdateCameraIDAllState();
            if (!_isInternalUpdate)
                LoadInspectionHistoryGridByDateRange();
        }

        private void btn_filterSearch_Click(object sender, EventArgs e)
        {
            LoadInspectionHistoryGridByDateRange();
        }

        private void btn_Last7DaysSearch_Click(object sender, EventArgs e)
        {
            var today = DateTime.Today;
            var from = today.AddDays(-6);

            DTP_IH_StartDate.Value = from;
            DTP_IH_EndDate.Value = today;
        }

        private void btn_ThisMonthSearch_Click(object sender, EventArgs e)
        {
            var today = DateTime.Today;

            var firstDay = new DateTime(today.Year, today.Month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);

            DTP_IH_StartDate.Value = firstDay;
            DTP_IH_EndDate.Value = lastDay;
        }

        private void btn_TodaySearch_Click(object sender, EventArgs e)
        {
            DateTime today = DateTime.Now.Date;

            DTP_IH_StartDate.Value = today;
            DTP_IH_EndDate.Value = today;

            LoadInspectionHistoryGridByDateRange();
        }

        private void btn_AllSearch_Click(object sender, EventArgs e)
        {
            OpenDetailsRequested?.Invoke(this, EventArgs.Empty);

            LoadInspectionHistoryGrid();
        }
    }
}
