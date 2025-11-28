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
            // 1) 전체 데이터 먼저 가져오기
            DataTable dt = _repo.GetAllInspectionHistory();

            // 2) DateTimePicker에서 기간 가져오기
            DateTime from = DTP_IH_StartDate.Value.Date;                       // 시작일 00:00:00
            DateTime to = DTP_IH_EndDate.Value.Date.AddDays(1).AddTicks(-1); // 종료일 23:59:59

            // 2-1) 🔹 불량 유형 체크박스 상태로 필터 리스트 만들기
            // DB 값 기준: '정상', '부품불량', '납땜불량', '폐기'
            var defectTypes = new List<string>();

            if (CB_IH_DefectType_Normal.Checked)
                defectTypes.Add("정상");

            if (CB_IH_DefectType_ComponentDefect.Checked)
                defectTypes.Add("부품불량");

            if (CB_IH_DefectType_SolderingDefect.Checked)
                defectTypes.Add("납땜불량");

            if (CB_IH_DefectType_Scrap.Checked)
                defectTypes.Add("폐기");

            // "전체" 체크되었거나, 아무 것도 안 고르면 → 불량 유형 필터는 적용 안 함
            bool useDefectFilter = defectTypes.Count > 0 && !CB_IH_DefectType_All.Checked;

            // 3) LINQ로 "검사 시각" + (선택 시) "불량 유형" 컬럼 기준 필터링
            var filteredRows = dt.AsEnumerable()
                .Where(row =>
                {
                    DateTime t = row.Field<DateTime>("검사 시각"); // alias 맞게 변경

                    // 날짜 범위 필터
                    if (t < from || t > to)
                        return false;

                    // 불량 유형 필터 (필요할 때만)
                    if (useDefectFilter)
                    {
                        // 👉 여기 컬럼 이름을 실제 SELECT에 맞게 수정
                        // ex) SELECT defect_type AS '불량 유형' 이면 "불량 유형"
                        //     alias 없이 그냥 가져오면 "defect_type"
                        string defect = row.Field<string>("불량 유형"); // 또는 "defect_type"

                        if (!defectTypes.Contains(defect))
                            return false;
                    }

                    return true;
                });

            DataTable view;

            // 4) 결과가 있으면 테이블로 만들고, 없으면 빈 테이블(컬럼 구조만) 생성
            if (filteredRows.Any())
                view = filteredRows.CopyToDataTable();
            else
                view = dt.Clone();   // 컬럼 구조만 복사, 행 0개

            // 5) 그리드에 바인딩 (새로고침 느낌 확실히)
            DGV_IH_result.DataSource = null;
            DGV_IH_result.AutoGenerateColumns = true;
            DGV_IH_result.DataSource = view;
            DGV_IH_result.Refresh();

            // 6) 보기용 설정
            DGV_IH_result.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            DGV_IH_result.RowHeadersVisible = false;

            if (DGV_IH_result.Columns.Contains("검사 시각"))
                DGV_IH_result.Columns["검사 시각"].FillWeight = 180;
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
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenDetailsRequested?.Invoke(this, EventArgs.Empty);

            if (sender is Button btn)
                UiStyleHelper.HighlightButton(btn);

            // 🔹 InspectionHistoryForm 열기
            InspectionHistoryDetailForm form = new InspectionHistoryDetailForm();
            form.StartPosition = FormStartPosition.CenterParent; // 부모 기준 중앙 정렬
            form.Show();
        }

        //////////////////////////////////// 불량 유형 : 네 개의 개별 체크박스 상태가 변경될 때마다 All 체크박스 상태 업데이트
        private void UpdateDefectTypeAllState()
        {
            if (_isInternalUpdate) return;

            _isInternalUpdate = true;

            // 네 개가 모두 체크되어 있으면 All 체크, 아니면 All 해제
            CB_IH_DefectType_All.Checked =
                CB_IH_DefectType_Normal.Checked &&
                CB_IH_DefectType_ComponentDefect.Checked &&
                CB_IH_DefectType_SolderingDefect.Checked &&
                CB_IH_DefectType_Scrap.Checked;

            _isInternalUpdate = false;
        }

        private void CB_DefectType_All_CheckedChanged(object sender, EventArgs e)
        {
            if (_isInternalUpdate) return; // 내부 업데이트면 무시 (옵션이지만 깔끔)

            bool isChecked = CB_IH_DefectType_All.Checked;

            // 이벤트 루프 방지 위해 temporarily flag 사용
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

            // 네 개가 모두 체크되어 있으면 All 체크, 아니면 All 해제
            CB_IH_CameraID_All.Checked =
                CB_IH_CameraID_CAM01.Checked &&
                CB_IH_CameraID_CAM02.Checked &&
                CB_IH_CameraID_CAM03.Checked;

            _isInternalUpdate = false;
        }

        private void CB_CameraID_All_CheckedChanged(object sender, EventArgs e)
        {
            if (_isInternalUpdate) return; // 내부 업데이트면 무시 (옵션이지만 깔끔)

            bool isChecked = CB_IH_CameraID_All.Checked;

            // 이벤트 루프 방지 위해 temporarily flag 사용
            _isInternalUpdate = true;

            CB_IH_CameraID_CAM01.Checked = isChecked;
            CB_IH_CameraID_CAM02.Checked = isChecked;
            CB_IH_CameraID_CAM03.Checked = isChecked;

            _isInternalUpdate = false;
        }

        private void CB_CameraID_CAM01_CheckedChanged(object sender, EventArgs e)
        {
            UpdateCameraIDAllState();
        }

        private void CB_CameraID_CAM02_CheckedChanged(object sender, EventArgs e)
        {
            UpdateCameraIDAllState();
        }

        private void CB_CameraID_CAM03_CheckedChanged(object sender, EventArgs e)
        {
            UpdateCameraIDAllState();
        }

        private void btn_filterSearch_Click(object sender, EventArgs e)
        {
            LoadInspectionHistoryGridByDateRange();
        }

        private void btn_Last7DaysSearch_Click(object sender, EventArgs e)
        {
            var today = DateTime.Today;
            var from = today.AddDays(-6);  // 오늘 포함해서 7일

            DTP_IH_StartDate.Value = from;
            DTP_IH_EndDate.Value = today;
            // 👉 ValueChanged 이벤트에서 자동으로 LoadInspectionHistoryGridByDateRange() 호출됨
        }

        private void btn_ThisMonthSearch_Click(object sender, EventArgs e)
        {
            var today = DateTime.Today;

            var firstDay = new DateTime(today.Year, today.Month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1); // 말일

            DTP_IH_StartDate.Value = firstDay;
            DTP_IH_EndDate.Value = lastDay;
            // 여기서도 DateRange_ValueChanged가 자동으로 실행돼서 그리드 갱신 됨
        }

        private void btn_TodaySearch_Click(object sender, EventArgs e)
        {
            // 오늘 날짜
            DateTime today = DateTime.Now.Date;

            // 기간 선택 UI가 있다면 자동 설정
            DTP_IH_StartDate.Value = today;
            DTP_IH_EndDate.Value = today;

            LoadInspectionHistoryGridByDateRange();
        }

        private void btn_AllSearch_Click(object sender, EventArgs e)
        {
            OpenDetailsRequested?.Invoke(this, EventArgs.Empty);

            // 전체 조회 + 기간 표시
            LoadInspectionHistoryGrid();
        }
    }
}
