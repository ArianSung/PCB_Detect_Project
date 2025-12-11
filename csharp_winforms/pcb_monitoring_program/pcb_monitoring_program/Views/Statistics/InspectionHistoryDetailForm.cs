using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace pcb_monitoring_program.Views.Statistics
{
    public partial class InspectionHistoryDetailForm : Form
    {
        // 메인 그리드에서 넘어온 DataRow 한 줄
        private readonly DataRow _sourceRow;

        // 기본 생성자 (디자이너 / 테스트용)
        public InspectionHistoryDetailForm()
        {
            InitializeComponent();
            this.Load += InspectionHistoryDetailForm_Load;
        }

        // 메인 그리드에서 DataRow 하나를 받아오는 생성자
        public InspectionHistoryDetailForm(DataRow row) : this()
        {
            _sourceRow = row;
        }

        private void InspectionHistoryDetailForm_Load(object sender, EventArgs e)
        {
            var grid = DGV_IHD_result;

            // 🔹 그리드 스타일
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.MultiSelect = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.RowHeadersVisible = false;
            grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            grid.RowTemplate.Height = 40;
            grid.DefaultCellStyle.Padding = new Padding(12, 8, 12, 8);

            // 🔹 KryptonDataGridView인 경우(아니면 무시)
            try
            {
                dynamic krypton = grid;
                krypton.StateCommon.DataCell.Content.Padding = new Padding(12, 8, 12, 8);
            }
            catch { }

            // 🔹 넘어온 데이터 없으면 그냥 종료
            if (_sourceRow == null || _sourceRow.Table == null)
                return;

            // 👉 DataRow 한 줄을 "항목 / 값" 세로 테이블로 변환
            DataTable view = new DataTable();
            view.Columns.Add("항목");
            view.Columns.Add("값");

            foreach (DataColumn col in _sourceRow.Table.Columns)
            {
                DataRow r = view.NewRow();

                r["항목"] = string.IsNullOrWhiteSpace(col.Caption)
                           ? col.ColumnName
                           : col.Caption;

                object val = _sourceRow[col];

                // 🔹 검사 시각 컬럼이면 보기 좋은 포맷으로 변환
                if ((col.ColumnName == "검사 시각" || col.Caption == "검사 시각") && val is DateTime dt)
                {
                    r["값"] = dt.ToString("yyyy-MM-dd HH:mm:ss");  // 오후/오전 대신 24시간 포맷
                }
                else
                {
                    r["값"] = (val == DBNull.Value) ? null : val;
                }

                view.Rows.Add(r);
            }

            grid.DataSource = view;

            // 🔹 항목/값 컬럼 비율 조정 (항목 35%, 값 65%)
            if (grid.Columns.Contains("항목") && grid.Columns.Contains("값"))
            {
                grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                var colItem = grid.Columns["항목"];
                var colValue = grid.Columns["값"];

                colItem.FillWeight = 40;
                colValue.FillWeight = 60;
            }

            // 🔹 여기 두 줄이 "밑으로 넓히는 기능"
            AdjustRowHeightsToFill();
            grid.Resize += (s2, e2) => AdjustRowHeightsToFill();
        }
        /// DataGridView의 클라이언트 높이에 맞게 행 높이를 균등 분배해서 꽉 채움
        /// </summary>
        private void AdjustRowHeightsToFill()
        {
            var grid = DGV_IHD_result;
            if (grid == null) return;

            // 실제 데이터가 있는 행만 대상
            var rows = grid.Rows
                .Cast<DataGridViewRow>()
                .Where(r => !r.IsNewRow && r.Visible)
                .ToList();

            if (rows.Count == 0) return;

            // 기본 높이 (너무 작아지지 않도록 최소값)
            int minRowHeight = 32;
            int defaultRowHeight = 40;

            // 헤더를 제외한 그리드 표시 영역 높이
            int totalHeight = grid.ClientSize.Height;
            if (grid.ColumnHeadersVisible)
                totalHeight -= grid.ColumnHeadersHeight;

            if (totalHeight <= 0) return;

            // 현재 기본 높이로 필요한 전체 높이
            int needed = rows.Count * defaultRowHeight;

            int rowHeight;

            if (needed >= totalHeight)
            {
                // 기본 높이로도 이미 꽉 차거나 넘치면 그냥 기본값 사용
                rowHeight = defaultRowHeight;
            }
            else
            {
                // 남는 공간이 있으면 전체 높이를 행 개수로 나눠서 늘려줌
                rowHeight = totalHeight / rows.Count;
                if (rowHeight < minRowHeight)
                    rowHeight = minRowHeight;
            }

            grid.RowTemplate.Height = rowHeight;
            foreach (var r in rows)
                r.Height = rowHeight;
        }
    }
}
