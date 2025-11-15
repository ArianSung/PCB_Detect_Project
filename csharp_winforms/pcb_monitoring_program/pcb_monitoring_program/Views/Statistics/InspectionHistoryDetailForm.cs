using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pcb_monitoring_program.Views.Statistics
{
    public partial class InspectionHistoryDetailForm : Form
    {
        // 전치를 한 번만 수행하기 위한 가드
        private bool _transposedOnce = false;

        public InspectionHistoryDetailForm()
        {
            InitializeComponent();

            // 폼 로드 시: 스타일 설정 → 데이터 채우기 → 전치 1회 자동 실행
            this.Load += (s, e) =>
            {
                // ✅ 셀 간격/행 높이/헤더 패딩 설정
                DGV_IHD_result.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                DGV_IHD_result.DefaultCellStyle.Padding = new Padding(12, 10, 12, 10);
                DGV_IHD_result.RowTemplate.Height = 44;
                DGV_IHD_result.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None; // 고정 높이
                DGV_IHD_result.RowHeadersVisible = true;
                DGV_IHD_result.RowHeadersWidth = 48;
                DGV_IHD_result.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;

                // (Krypton 전용: 가시적 효과 확실)
                DGV_IHD_result.StateCommon.DataCell.Content.Padding = new Padding(12, 10, 12, 10);

                // 1) 그리드 준비 (행헤더 보이기 등)
                DGV_IHD_result.RowHeadersVisible = true;

                // 2) (안전장치) 디자이너에서 열을 안 만들었다면 자동 생성
                if (DGV_IHD_result.Columns.Count == 0)
                {
                    // Name은 코드 접근용, HeaderText는 사용자 표시용
                    DGV_IHD_result.Columns.Add("date", "날짜");
                    DGV_IHD_result.Columns.Add("time", "시간");
                    DGV_IHD_result.Columns.Add("CameraID", "카메라 ID");
                    DGV_IHD_result.Columns.Add("PCBID", "PCB ID");
                    DGV_IHD_result.Columns.Add("DefectType", "불량 유형");
                    DGV_IHD_result.Columns.Add("DefectLocation", "불량 위치");
                    DGV_IHD_result.Columns.Add("productionline", "생산 라인");
                }

                // 3) 임의의 테스트 데이터 주입 (원하면 삭제/수정)
                //    디자이너에서 만든 열 순서: date, time, CameraID, PCBID, DefectType, DefectLocation, productionline
                DGV_IHD_result.Rows.Add("2025-11-11", "10:30", "CAM01", "PCB001", "스크래치", "A-5", "1라인");

                // 4) 컨트롤/데이터가 모두 준비된 직후 전치 수행 (최초 1회)
                this.BeginInvoke((Action)EnsureTransposedOnce);

            };

            // DataSource 바인딩을 쓰는 경우에도 바인딩 완료 후 최초 1회 전치
            if (DGV_IHD_result != null)
                DGV_IHD_result.DataBindingComplete += (s, e) => EnsureTransposedOnce();
        }

        /// 전치 작업을 폼 수명 동안 딱 한 번만 수행
        private void EnsureTransposedOnce()
        {
            if (_transposedOnce) return;
            if (DGV_IHD_result == null) return;

            _transposedOnce = true;
            DGV_IHD_result.AllowUserToAddRows = false;

            // 1. 전치 실행
            TransposeInPlace(DGV_IHD_result, "항목");

            // 2. 오른쪽 컬럼 헤더 이름 변경
            if (DGV_IHD_result.Columns.Count > 1)
                DGV_IHD_result.Columns[1].HeaderText = "값";

            // 3. 기본 설정
            DGV_IHD_result.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            DGV_IHD_result.ScrollBars = ScrollBars.Horizontal;
            DGV_IHD_result.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            DGV_IHD_result.AllowUserToResizeRows = false;

            // 4. 실제 행들만 가져오기
            var dataRows = DGV_IHD_result.Rows
                .Cast<DataGridViewRow>()
                .Where(r => !r.IsNewRow)
                .ToList();

            int rowCount = dataRows.Count;
            if (rowCount == 0) return;

            // 5. 전체 행 높이 계산 (헤더 제외)
            int totalHeightForRows = DGV_IHD_result.ClientSize.Height;
            if (DGV_IHD_result.ColumnHeadersVisible)
                totalHeightForRows -= DGV_IHD_result.ColumnHeadersHeight;

            if (totalHeightForRows <= 0) return;

            // 6. 기본 균등 분배
            int baseHeight = totalHeightForRows / rowCount;
            if (baseHeight < 1) baseHeight = 1;

            DGV_IHD_result.RowTemplate.Height = baseHeight;
            foreach (var r in dataRows)
                r.Height = baseHeight;

            // 7. 실제 그려진 높이 측정 후 보정
            DGV_IHD_result.PerformLayout();

            Rectangle firstRect = DGV_IHD_result.GetRowDisplayRectangle(dataRows.First().Index, true);
            Rectangle lastRect = DGV_IHD_result.GetRowDisplayRectangle(dataRows.Last().Index, true);
            int currentHeight = lastRect.Bottom - firstRect.Top;

            int diff = totalHeightForRows - currentHeight;
            if (diff != 0)
            {
                int newLast = dataRows.Last().Height + diff;
                if (newLast < 1) newLast = 1;
                dataRows.Last().Height = newLast;
            }

            // 8. 스크롤 초기화
            DGV_IHD_result.FirstDisplayedScrollingRowIndex = 0;
        }


        // ===================== 전치(가로/세로 반전) 유틸 =====================

        /// source의 현재 표시 데이터를 읽어 target에 가로/세로 반전하여 채웁니다.
        /// 수동 열/행 방식과 DataSource 바인딩 방식 모두 지원.
        /// 숨김행/열은 제외(원하면 관련 조건 제거).
        private void TransposeGrid(DataGridView source, DataGridView target, string firstColHeader = "항목")
        {
            // 1) 실제 표시되는 행(입력행/숨김행 제외)
            var rowIndexes = new List<int>();
            foreach (DataGridViewRow r in source.Rows)
            {
                if (r.IsNewRow) continue;  // 입력행은 안전하게 제외
                if (!r.Visible) continue;  // 숨김행 제외 (원하면 삭제)
                rowIndexes.Add(r.Index);
            }

            // 2) 실제 표시되는 열(숨김열 제외)
            var colIndexes = new List<int>();
            foreach (DataGridViewColumn c in source.Columns)
            {
                if (!c.Visible) continue; // 숨김열 제외 (원하면 삭제)
                colIndexes.Add(c.Index);
            }

            target.SuspendLayout();
            target.Columns.Clear();
            target.Rows.Clear();

            // 3) 새 컬럼 구성: 첫 컬럼은 "항목", 이후는 원본 각 행이 새 컬럼 헤더
            target.Columns.Add("item", firstColHeader);
            for (int i = 0; i < rowIndexes.Count; i++)
            {
                int r = rowIndexes[i];
                string rowHeader = source.Rows[r].HeaderCell?.Value?.ToString();
                //if (string.IsNullOrWhiteSpace(rowHeader)) rowHeader = $"Row{i + 1}";
                //target.Columns.Add($"row{i}", rowHeader);
                if (string.IsNullOrWhiteSpace(rowHeader))
                    rowHeader = "";
                target.Columns.Add($"row{i}", rowHeader);
            }

            // 4) 데이터 전치
            for (int ci = 0; ci < colIndexes.Count; ci++)
            {
                int c = colIndexes[ci];
                int newRow = target.Rows.Add();

                // 항목명: 원본 열의 HeaderText(없으면 Name)
                string header = string.IsNullOrWhiteSpace(source.Columns[c].HeaderText)
                                ? source.Columns[c].Name
                                : source.Columns[c].HeaderText;
                target.Rows[newRow].Cells[0].Value = header;

                // 각 행 값 채우기
                for (int i = 0; i < rowIndexes.Count; i++)
                {
                    int r = rowIndexes[i];
                    target.Rows[newRow].Cells[i + 1].Value = source.Rows[r].Cells[c].Value;
                }
            }

            // 보기용 설정
            target.RowHeadersVisible = false;

            // ✅ 전치 결과(target)에 입력행이 생기지 않게
            target.AllowUserToAddRows = false;

            target.ResumeLayout();
        }

        /// 같은 그리드에 덮어쓰기(인플레이스) 전치.
        private void TransposeInPlace(DataGridView grid, string firstColHeader = "항목")
        {
            using (var temp = new DataGridView())
            {
                TransposeGrid(grid, temp, firstColHeader);

                grid.SuspendLayout();
                grid.Columns.Clear();
                grid.Rows.Clear();

                // 컬럼 복제
                foreach (DataGridViewColumn col in temp.Columns)
                {
                    var clone = (DataGridViewColumn)col.Clone();
                    clone.HeaderText = col.HeaderText;
                    clone.Name = col.Name;
                    grid.Columns.Add(clone);
                }

                // ✅ NewRow(입력행) 스킵하면서 복사
                foreach (DataGridViewRow row in temp.Rows)
                {
                    if (row.IsNewRow) continue;   // ← 빈 입력행 복사 방지
                    int idx = grid.Rows.Add();
                    for (int i = 0; i < row.Cells.Count; i++)
                        grid.Rows[idx].Cells[i].Value = row.Cells[i].Value;
                }

                // ✅ 전치 후에도 입력행이 다시 생기지 않게
                grid.AllowUserToAddRows = false;

                // ✅ 줄 간격(행 높이) 확실 강제
                int h = 44; // 원하는 값으로 조정
                grid.RowTemplate.Height = h;          // 이후 추가되는 행의 높이
                foreach (DataGridViewRow r in grid.Rows)
                    r.Height = h;                     // 현재 존재하는 모든 행에 강제 적용

                grid.RowHeadersVisible = false; // 필요 시 true로
                grid.ResumeLayout();
            }
        }
        // =================== /전치 유틸 ===================
    }
}
