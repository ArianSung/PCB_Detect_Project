using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using pcb_monitoring_program.DatabaseManager.Models;
using pcb_monitoring_program.DatabaseManager;
using ClosedXML.Excel;

namespace pcb_monitoring_program.Views.Statistics
{
    public partial class StatisticsView : UserControl
    {
        private List<DailyStatistics> _yearDailyStats = new List<DailyStatistics>();
        private int _currentYear = 2025;  // 일단 2025년 기준 (원하면 DateTime.Today.Year 쓰면 됨)
        private int _currentMonth = 10;   // 일단 10월 기준으로 테스트
        public StatisticsView()
        {
            InitializeComponent();
        }

        private void StatisticsView_Load(object sender, EventArgs e)
        {
            // 🔹 기본 연/월 (지금은 오늘 기준)
            _currentYear = DateTime.Today.Year;
            _currentMonth = DateTime.Today.Month;

            // 🔹 Krypton DateTimePicker 초기 설정 (디자이너에서 했으면 생략 가능, 그래도 안전하게)
            dtpMonth.Format = DateTimePickerFormat.Custom;
            dtpMonth.CustomFormat = "yyyy년 MM월";
            dtpMonth.Value = new DateTime(_currentYear, _currentMonth, 1);
            UiStyleHelper.MakeRoundedPanel(cardMonthlyLine, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardMonthlyAccum, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardDefectPie, radius: 16, back: Color.FromArgb(44, 44, 44));

            UiStyleHelper.AddShadowRoundedPanel(cardMonthlyLine, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardMonthlyAccum, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardDefectPie, 16);

            UiStyleHelper.MakeRoundedButton(btn_Excel, 24);

            UiStyleHelper.AttachDropShadow(btn_Excel, radius: 16, offset: 4);
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is Button btn)
                {
                    btn.BackColor = Color.FromArgb(64, 64, 64);
                    btn.ForeColor = Color.White;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                    btn.Cursor = Cursors.Hand;
                }
            }
            LoadDailyStatisticsForYear(_currentYear);

            // 👉 공용 데이터(_yearDailyStats)를 기반으로 세 개 차트 그리기
            SetupMonthlyLineChart();
            SetupMonthlyAccumChart();
            SetupDefectTypePieChart();
            UpdateMonthLabels();
        }

        private void LoadDailyStatisticsForYear(int year)
        {
            try
            {
                string connectionString =
                    "Server=100.80.24.53;Port=3306;Database=pcb_inspection;Uid=pcb_admin;Pwd=1234;CharSet=utf8mb4;";

                using (var db = new DatabaseManager.DatabaseManager(connectionString))
                {
                    _yearDailyStats = db.GetDailyStatisticsForYear(year);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"통계 데이터 로드 중 오류: {ex.Message}", "오류");
                _yearDailyStats = new List<DailyStatistics>();
            }
        }

        private void SetupMonthlyLineChart()
        {
            var chart = MonthlyLineChart;

            chart.Series.Clear();
            chart.ChartAreas.Clear();
            chart.Legends.Clear();

            var area = new ChartArea("Main");
            area.BackColor = Color.Transparent;

            // 현재 월의 일수 계산 (예: 10월 → 31일)
            int daysInMonth = DateTime.DaysInMonth(_currentYear, _currentMonth);

            // X축: 1 ~ 해당 월 마지막 날
            area.AxisX.Minimum = 1;
            area.AxisX.Maximum = daysInMonth;
            area.AxisX.Interval = 1;
            area.AxisX.MajorGrid.Enabled = false;
            area.AxisX.IsLabelAutoFit = false;
            area.AxisX.LabelStyle.ForeColor = Color.Gainsboro;
            area.AxisX.LabelStyle.Font = new Font("맑은 고딕", 10);

            // Y축: 일일 생산량
            area.AxisY.Minimum = 0;
            area.AxisY.Maximum = 1000;      // 10월에 하루 1000개 이하로 맞춰놨으니 1000으로
            area.AxisY.Interval = 100;
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(70, 70, 70);
            area.AxisY.LabelStyle.ForeColor = Color.Gainsboro;
            area.AxisY.LabelStyle.Font = new Font("맑은 고딕", 8);

            chart.ChartAreas.Add(area);

            // 라인 시리즈 4개
            Series sNormal = CreateLineSeries("정상", Color.FromArgb(100, 181, 246));
            Series sPartDefect = CreateLineSeries("부품불량", Color.Orange);
            Series sSolderDefect = CreateLineSeries("납땜불량", Color.FromArgb(158, 158, 158));
            Series sScrap = CreateLineSeries("폐기", Color.Red);

            chart.Series.Add(sNormal);
            chart.Series.Add(sPartDefect);
            chart.Series.Add(sSolderDefect);
            chart.Series.Add(sScrap);

            // 🔥 현재 월(예: 10월) 데이터만 필터
            var monthData = _yearDailyStats
                .Where(d => d.StatDate.Month == _currentMonth && d.StatDate.Year == _currentYear)
                .ToList();

            // 1일 ~ 마지막 날까지 돌면서, 없는 날짜는 0으로 처리
            for (int day = 1; day <= daysInMonth; day++)
            {
                var rec = monthData.FirstOrDefault(d => d.StatDate.Day == day);

                int normal = rec?.NormalCount ?? 0;
                int partDefect = rec?.ComponentDefectCount ?? 0;
                int solderDefect = rec?.SolderDefectCount ?? 0;
                int scrap = rec?.DiscardCount ?? 0;

                sNormal.Points.AddXY(day, normal);
                sPartDefect.Points.AddXY(day, partDefect);
                sSolderDefect.Points.AddXY(day, solderDefect);
                sScrap.Points.AddXY(day, scrap);
            }

            var legend = new Legend
            {
                Docking = Docking.Top,
                Alignment = StringAlignment.Center,
                BackColor = Color.Transparent,
                ForeColor = Color.Gainsboro,
                Font = new Font("맑은 고딕", 8)
            };
            chart.Legends.Add(legend);
        }

        private Series CreateLineSeries(string name, Color color)
        {
            var s = new Series(name)
            {
                ChartArea = "Main",
                ChartType = SeriesChartType.Line,
                BorderWidth = 2,
                Color = color
            };
            s.MarkerStyle = MarkerStyle.Circle;
            s.MarkerSize = 4;
            s.MarkerColor = color;
            return s;
        }

        private void SetupMonthlyAccumChart()
        {
            var chart = MonthlyAccumChart;

            chart.DataSource = null;
            chart.Series.Clear();
            chart.ChartAreas.Clear();
            chart.Legends.Clear();

            chart.BackColor = Color.Transparent;
            chart.BorderlineWidth = 0;

            var area = new ChartArea("Main");
            area.BackColor = Color.Transparent;

            // X축: 1~12월
            area.AxisX.Minimum = 0;
            area.AxisX.Maximum = 13;
            area.AxisX.Interval = 1;
            area.AxisX.MajorGrid.Enabled = false;
            area.AxisX.LabelStyle.ForeColor = Color.Gainsboro;
            area.AxisX.LabelStyle.Font = new Font("맑은 고딕", 8);
            area.AxisX.LabelStyle.Format = "0월";

            // Y축: 검사 개수
            area.AxisY.Minimum = 0;
            area.AxisY.Maximum = 50000;  // 연간 누적 기준, 필요하면 나중에 동적으로 조정
            area.AxisY.Interval = 10000;
            area.AxisY.MajorGrid.Enabled = true;
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(70, 70, 70);
            area.AxisY.LabelStyle.ForeColor = Color.Gainsboro;
            area.AxisY.LabelStyle.Font = new Font("맑은 고딕", 8);

            chart.ChartAreas.Add(area);

            Color normalColor = Color.FromArgb(100, 181, 246);
            Color partDefectColor = Color.FromArgb(255, 167, 38);
            Color solderDefColor = Color.FromArgb(158, 158, 158);
            Color scrapColor = Color.Red;

            Series sNormal = new Series("정상")
            {
                ChartArea = "Main",
                ChartType = SeriesChartType.StackedColumn,
                Color = normalColor
            };

            Series sPartDefect = new Series("부품불량")
            {
                ChartArea = "Main",
                ChartType = SeriesChartType.StackedColumn,
                Color = partDefectColor
            };

            Series sSolderDefect = new Series("납땜불량")
            {
                ChartArea = "Main",
                ChartType = SeriesChartType.StackedColumn,
                Color = solderDefColor
            };

            Series sScrap = new Series("폐기")
            {
                ChartArea = "Main",
                ChartType = SeriesChartType.StackedColumn,
                Color = scrapColor
            };

            chart.Series.Add(sNormal);
            chart.Series.Add(sPartDefect);
            chart.Series.Add(sSolderDefect);
            chart.Series.Add(sScrap);

            // 🔥 연도별 daily 데이터를 월별로 합산
            for (int month = 1; month <= 12; month++)
            {
                var monthGroup = _yearDailyStats
                    .Where(d => d.StatDate.Year == _currentYear && d.StatDate.Month == month)
                    .ToList();

                int sumNormal = monthGroup.Sum(d => d.NormalCount);
                int sumPartDefect = monthGroup.Sum(d => d.ComponentDefectCount);
                int sumSolderDefect = monthGroup.Sum(d => d.SolderDefectCount);
                int sumScrap = monthGroup.Sum(d => d.DiscardCount);

                sNormal.Points.AddXY(month, sumNormal);
                sPartDefect.Points.AddXY(month, sumPartDefect);
                sSolderDefect.Points.AddXY(month, sumSolderDefect);
                sScrap.Points.AddXY(month, sumScrap);
            }

            var legend = new Legend
            {
                Docking = Docking.Top,
                Alignment = StringAlignment.Center,
                BackColor = Color.Transparent,
                ForeColor = Color.Gainsboro,
                Font = new Font("맑은 고딕", 8)
            };
            chart.Legends.Add(legend);
        }
        private void SetupDefectTypePieChart()
        {
            Color normalColor = Color.FromArgb(100, 181, 246);
            Color partDefectColor = Color.FromArgb(255, 167, 38);
            Color solderDefColor = Color.FromArgb(158, 158, 158);
            Color scrapColor = Color.FromArgb(244, 67, 54);

            // 🔥 현재 월 데이터만 집계
            var monthData = _yearDailyStats
                .Where(d => d.StatDate.Year == _currentYear && d.StatDate.Month == _currentMonth)
                .ToList();

            int sumNormal = monthData.Sum(d => d.NormalCount);
            int sumPartDefect = monthData.Sum(d => d.ComponentDefectCount);
            int sumSolderDefect = monthData.Sum(d => d.SolderDefectCount);
            int sumScrap = monthData.Sum(d => d.DiscardCount);

            var categories = new (string name, int value, Color color)[]
            {
        ("정상",     sumNormal,       normalColor),
        ("부품불량", sumPartDefect,   partDefectColor),
        ("납땜불량", sumSolderDefect, solderDefColor),
        ("폐기",     sumScrap,        scrapColor),
            };

            var chart = DefectTypePieChart;
            chart.Series.Clear();
            chart.Legends.Clear();
            chart.ChartAreas.Clear();

            var area = new ChartArea("area");
            area.BackColor = Color.Transparent;
            area.Position.Auto = false;
            area.Position.X = 5; area.Position.Y = 5;
            area.Position.Width = 90; area.Position.Height = 90;
            area.InnerPlotPosition.Auto = false;
            area.InnerPlotPosition.X = 10; area.InnerPlotPosition.Y = 10;
            area.InnerPlotPosition.Width = 80; area.InnerPlotPosition.Height = 80;
            chart.ChartAreas.Add(area);

            var s = new Series("불량 카테고리")
            {
                ChartType = SeriesChartType.Doughnut
            };
            s["DoughnutRadius"] = "50";
            s["PieLabelStyle"] = "Disabled";
            s.IsValueShownAsLabel = false;

            foreach (var item in categories)
            {
                var pt = s.Points.AddXY(item.name, item.value);
                s.Points[pt].Color = item.color;
            }
            chart.Series.Add(s);

            // 커스텀 레전드 (원래 코드 그대로 재사용)
            flowPie.SuspendLayout();
            flowPie.Controls.Clear();
            flowPie.FlowDirection = FlowDirection.TopDown;
            flowPie.WrapContents = false;
            flowPie.AutoSize = false;
            flowPie.BackColor = Color.Transparent;

            foreach (var item in categories)
            {
                var row = new Panel
                {
                    Height = 24,
                    Width = flowPie.Width - 16,
                    BackColor = Color.Transparent,
                    Margin = new Padding(0, 2, 0, 2),
                };

                var swatch = new Panel
                {
                    Width = 12,
                    Height = 12,
                    BackColor = item.color,
                    Left = 0,
                    Top = (row.Height - 12) / 2
                };

                var lbl = new Label
                {
                    AutoSize = true,
                    Left = 20,
                    Top = (row.Height - 16) / 2,
                    Text = $"{item.name}  {item.value}개",
                    ForeColor = Color.Gainsboro
                };

                EventHandler clickHandler = (_, __) =>
                {
                    foreach (var p in s.Points) { p.BorderWidth = 0; }
                    var target = s.Points.FirstOrDefault(p => p.AxisLabel == item.name);
                    if (target != null) { target.BorderColor = Color.White; target.BorderWidth = 3; }
                };

                row.Cursor = Cursors.Hand;
                swatch.Cursor = Cursors.Hand;
                lbl.Cursor = Cursors.Hand;

                row.Click += clickHandler;
                swatch.Click += clickHandler;
                lbl.Click += clickHandler;

                row.Controls.Add(swatch);
                row.Controls.Add(lbl);
                flowPie.Controls.Add(row);
            }
            flowPie.ResumeLayout();
        }

        private void UpdateMonthLabels()
        {
            // 예: 2025년 11월
            string ymText = $"{_currentYear}년 {_currentMonth:00}월";

            // 각 카드별 제목 텍스트
            if (lblMonthlyLineTitle != null)
                lblMonthlyLineTitle.Text = $"{ymText} 일별 생산/불량 추이";

            if (lblMonthlyAccumTitle != null)
                lblMonthlyAccumTitle.Text = $"{_currentYear}년 월별 누적 현황";

            if (lblDefectPieTitle != null)
                lblDefectPieTitle.Text = $"{ymText} 불량 유형 비율";
        }

        private void MonthlyLineChart_MouseClick(object sender, MouseEventArgs e)
        {
            var chart = (Chart)sender;

            // 1) 클릭 위치가 어디인지 판별
            HitTestResult hit = chart.HitTest(e.X, e.Y);

            // 2) 레전드 아이템이 아니면 무시
            if (hit.ChartElementType != ChartElementType.LegendItem)
                return;

            // 3) 어떤 시리즈의 레전드인지 확인
            var legendItem = (LegendItem)hit.Object;
            string clickedSeriesName = legendItem.SeriesName;

            // 4) 현재 필터 상태 확인 (chart.Tag 에 저장해두자)
            string? currentFilter = chart.Tag as string;   // null = 전체보기 상태

            if (currentFilter == null)
            {
                // 전체 보기 상태 → 클릭한 시리즈만 보이게 (필터 ON)
                chart.Tag = clickedSeriesName;

                foreach (var s in chart.Series)
                {
                    bool isClicked = (s.Name == clickedSeriesName);

                    if (isClicked)
                    {
                        // 클릭한 시리즈: 원래 스타일 유지
                        // (CreateLineSeries에서 이미 색/마커 설정해줬으니까 그대로 둬도 됨)
                    }
                    else
                    {
                        // 나머지 시리즈: 그래프만 안 보이게
                        s.Color = Color.Transparent;
                        s.MarkerStyle = MarkerStyle.None;
                    }

                    s.IsVisibleInLegend = true; // 레전드는 항상 보이게
                }
            }
            else if (currentFilter == clickedSeriesName)
            {
                // 이미 이 시리즈로 필터 중 → 다시 클릭하면 전체보기로 복귀
                chart.Tag = null;

                foreach (var s in chart.Series)
                {
                    // 여기서 각 시리즈 원래 색/마커를 다시 세팅해주기
                    // 예: 이름 기준으로 색 복원
                    if (s.Name == "정상")
                        s.Color = Color.FromArgb(100, 181, 246);
                    else if (s.Name == "부품불량")
                        s.Color = Color.Orange;
                    else if (s.Name == "납땜불량")
                        s.Color = Color.FromArgb(158, 158, 158);
                    else if (s.Name == "폐기")
                        s.Color = Color.Red;

                    s.MarkerStyle = MarkerStyle.Circle;
                    s.MarkerSize = 4;

                    s.IsVisibleInLegend = true;
                }
            }
            else
            {
                // 다른 시리즈로 필터 변경
                chart.Tag = clickedSeriesName;

                foreach (var s in chart.Series)
                {
                    bool isClicked = (s.Name == clickedSeriesName);

                    if (isClicked)
                    {
                        // 클릭된 시리즈: 색/마커 복원
                        if (s.Name == "정상")
                            s.Color = Color.FromArgb(100, 181, 246);
                        else if (s.Name == "부품불량")
                            s.Color = Color.Orange;
                        else if (s.Name == "납땜불량")
                            s.Color = Color.FromArgb(158, 158, 158);
                        else if (s.Name == "폐기")
                            s.Color = Color.Red;

                        s.MarkerStyle = MarkerStyle.Circle;
                        s.MarkerSize = 4;
                    }
                    else
                    {
                        // 나머지: 숨기기
                        s.Color = Color.Transparent;
                        s.MarkerStyle = MarkerStyle.None;
                    }

                    s.IsVisibleInLegend = true;
                }
            }
        }

        private void MonthlyLineChart_MouseMove(object sender, MouseEventArgs e)
        {
            var chart = (Chart)sender;

            // 마우스 위치로 히트 테스트
            var hit = chart.HitTest(e.X, e.Y);

            // 레전드 아이템 위에 있으면 손가락, 아니면 기본
            if (hit.ChartElementType == ChartElementType.LegendItem)
                chart.Cursor = Cursors.Hand;
            else
                chart.Cursor = Cursors.Default;
        }

        private void btn_Excel_Click(object sender, EventArgs e)
        {
            // 1) 현재 선택된 연/월 기준으로 데이터 필터
            var monthData = _yearDailyStats
                .Where(d => d.StatDate.Year == _currentYear && d.StatDate.Month == _currentMonth)
                .OrderBy(d => d.StatDate)
                .ToList();

            if (monthData.Count == 0)
            {
                MessageBox.Show("선택한 월에 해당하는 통계 데이터가 없습니다.", "엑셀 내보내기");
                return;
            }

            // 2) 저장 위치 선택
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Excel 파일 (*.xlsx)|*.xlsx";
                sfd.FileName = $"통계_{_currentYear}_{_currentMonth:00}.xlsx";

                if (sfd.ShowDialog() != DialogResult.OK)
                    return;

                try
                {
                    // 3) 엑셀 워크북/시트 생성
                    using (var wb = new XLWorkbook())
                    {
                        var sheetName = $"{_currentYear}-{_currentMonth:00}";
                        var ws = wb.Worksheets.Add(sheetName);

                        // 3-1) 헤더 행
                        ws.Cell(1, 1).Value = "날짜";
                        ws.Cell(1, 2).Value = "총 검사 수";
                        ws.Cell(1, 3).Value = "정상";
                        ws.Cell(1, 4).Value = "부품불량";
                        ws.Cell(1, 5).Value = "납땜불량";
                        ws.Cell(1, 6).Value = "폐기";
                        ws.Cell(1, 7).Value = "불량률";

                        var headerRange = ws.Range(1, 1, 1, 7);
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        // 3-2) 데이터 행
                        int row = 2;
                        foreach (var d in monthData)
                        {
                            ws.Cell(row, 1).Value = d.StatDate;
                            ws.Cell(row, 1).Style.DateFormat.Format = "yyyy-MM-dd";

                            ws.Cell(row, 2).Value = d.TotalInspections;
                            ws.Cell(row, 3).Value = d.NormalCount;
                            ws.Cell(row, 4).Value = d.ComponentDefectCount;
                            ws.Cell(row, 5).Value = d.SolderDefectCount;
                            ws.Cell(row, 6).Value = d.DiscardCount;

                            // 불량률 계산
                            double defectRate = 0;
                            int defectSum = d.ComponentDefectCount + d.SolderDefectCount + d.DiscardCount;
                            if (d.TotalInspections > 0)
                                defectRate = defectSum * 1.0 / d.TotalInspections;

                            ws.Cell(row, 7).Value = defectRate;                  // 0.1234
                            ws.Cell(row, 7).Style.NumberFormat.Format = "0.00%"; // 12.34%

                            row++;
                        }

                        // 3-3) 숫자 컬럼(B~F) 형식/정렬/색 지정
                        int lastRow = row - 1;
                        var dataNumberRange = ws.Range(2, 2, lastRow, 6); // B2 ~ F(lastRow)
                        dataNumberRange.Style.NumberFormat.Format = "0";  // 정수
                        dataNumberRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        dataNumberRange.Style.Font.FontColor = XLColor.Black;

                        // 불량률 컬럼 오른쪽 정렬
                        ws.Column(7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                        // 3-4) 컬럼 너비 자동 맞춤 + 최소 폭 보장
                        ws.Columns().AdjustToContents();

                        for (int col = 1; col <= 7; col++)
                        {
                            if (ws.Column(col).Width < 12)
                                ws.Column(col).Width = 12;   // 너무 좁으면 최소 12
                        }

                        // 4) 저장
                        wb.SaveAs(sfd.FileName);
                    }

                    MessageBox.Show("엑셀 파일로 내보내기가 완료되었습니다.", "엑셀 내보내기");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("엑셀 저장 중 오류가 발생했습니다.\n" + ex.Message, "오류");
                }
            }
        }

        private void dtpMonth_ValueChanged(object sender, EventArgs e)
        {
            DateTime selected = dtpMonth.Value;

            int newYear = selected.Year;
            int newMonth = selected.Month;

            bool yearChanged = (newYear != _currentYear);

            // 🔹 현재 선택 상태 갱신
            _currentYear = newYear;
            _currentMonth = newMonth;

            // 🔹 연도가 바뀌면 DB에서 해당 연도 daily 다시 가져오기
            if (yearChanged)
            {
                LoadDailyStatisticsForYear(_currentYear);
            }

            // 🔹 새 연/월 기준으로 3개 차트 모두 다시 그림
            SetupMonthlyLineChart();
            SetupMonthlyAccumChart();
            SetupDefectTypePieChart();
            UpdateMonthLabels();
        }
    }
} 
