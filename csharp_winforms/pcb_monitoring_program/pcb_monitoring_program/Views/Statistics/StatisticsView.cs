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

namespace pcb_monitoring_program.Views.Statistics
{
    public partial class StatisticsView : UserControl
    {
        public StatisticsView()
        {
            InitializeComponent();
        }

        private void StatisticsView_Load(object sender, EventArgs e)
        {
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
            SetupMonthlyLineChart();
            SetupMonthlyAccumChart();
            SetupDefectTypePieChart();
        }
        private void SetupMonthlyLineChart()
        {
            var chart = MonthlyLineChart;

            chart.Series.Clear();
            chart.ChartAreas.Clear();
            chart.Legends.Clear();

            var area = new ChartArea("Main");
            area.BackColor = Color.Transparent;

            // X축: 1 ~ 31일
            area.AxisX.Minimum = 1;
            area.AxisX.Maximum = 31;
            area.AxisX.Interval = 1;
            area.AxisX.MajorGrid.Enabled = false;
            area.AxisX.IsLabelAutoFit = false;
            area.AxisX.LabelStyle.ForeColor = Color.Gainsboro;
            area.AxisX.LabelStyle.Font = new Font("맑은 고딕", 10);
            //area.AxisX.LabelStyle.Format = "0일";   // 1일, 2일, 3일 ... 이렇게 보이게

            // Y축: 일일 생산량(임시)
            area.AxisY.Minimum = 0;
            area.AxisY.Maximum = 60;      // 데이터 보고 조정
            area.AxisY.Interval = 10;
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(70, 70, 70);
            area.AxisY.LabelStyle.ForeColor = Color.Gainsboro;
            area.AxisY.LabelStyle.Font = new Font("맑은 고딕", 8);

            chart.ChartAreas.Add(area);

            // 라인 시리즈 4개 (정상/부품불량/납땜불량/폐기)
            Series sNormal = CreateLineSeries("정상", Color.FromArgb(100, 181, 246));
            Series sPartDefect = CreateLineSeries("부품불량", Color.Orange);
            Series sSolderDefect = CreateLineSeries("납땜불량", Color.FromArgb(158, 158, 158));
            Series sScrap = CreateLineSeries("폐기", Color.Red);

            chart.Series.Add(sNormal);
            chart.Series.Add(sPartDefect);
            chart.Series.Add(sSolderDefect);
            chart.Series.Add(sScrap);

            // ✅ 1~31일 “일일 생산량” 느낌으로 더미 데이터
            //    나중에 DB 붙이면 여기만 교체하면 됨
            double[] normal = { 40, 38, 42, 39, 41, 43, 44, 42, 40, 39, 38, 37, 36, 38, 39, 41, 42, 44, 45, 43, 42, 40, 39, 38, 37, 39, 40, 42, 43, 41, 40 };
            double[] partDefect = { 4, 3, 5, 4, 6, 5, 7, 6, 4, 5, 3, 4, 6, 5, 4, 7, 6, 5, 8, 4, 5, 4, 6, 5, 4, 7, 6, 5, 8, 5, 4 };
            double[] solderDefect = { 2, 3, 2, 4, 3, 2, 5, 3, 2, 4, 2, 3, 4, 3, 2, 5, 3, 2, 4, 3, 2, 4, 3, 2, 5, 3, 2, 4, 3, 2, 3 };
            double[] scrap = { 1, 2, 1, 3, 2, 1, 4, 2, 1, 3, 1, 2, 1, 4, 2, 1, 3, 2, 5, 1, 2, 1, 3, 2, 1, 4, 2, 1, 3, 2, 6 };

            for (int day = 1; day <= 31; day++)
            {
                int i = day - 1; // 배열 인덱스 0~30

                sNormal.Points.AddXY(day, normal[i]);
                sPartDefect.Points.AddXY(day, partDefect[i]);
                sSolderDefect.Points.AddXY(day, solderDefect[i]);
                sScrap.Points.AddXY(day, scrap[i]);
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

            // 1) 초기화
            chart.DataSource = null;
            chart.Series.Clear();
            chart.ChartAreas.Clear();
            chart.Legends.Clear();

            chart.BackColor = Color.Transparent;
            chart.BorderlineWidth = 0;

            // 2) ChartArea
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



            // Y축: 검사 개수 (예시 0~60)
            area.AxisY.Minimum = 0;
            area.AxisY.Maximum = 60; // 더미 기준, 나중에 데이터 보고 조정
            area.AxisY.Interval = 10;
            area.AxisY.MajorGrid.Enabled = true;
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(70, 70, 70);
            area.AxisY.LabelStyle.ForeColor = Color.Gainsboro;
            area.AxisY.LabelStyle.Font = new Font("맑은 고딕", 8);

            chart.ChartAreas.Add(area);

            // 3) 시리즈 생성 (아래에서 위로 쌓이는 순서: 정상 → 부품불량 → 납땜불량 → 폐기)
            Color normalColor = Color.FromArgb(100, 181, 246);   // 파랑
            Color partDefectColor = Color.FromArgb(255, 167, 38);   // 주황
            Color solderDefColor = Color.FromArgb(158, 158, 158);   // 회색
            Color scrapColor = Color.Red;                      // 빨강(폐기)

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

            // 4) 더미 데이터 (1~12월, 원하는 값으로 바꾸면 됨)
            int[] normal = { 40, 38, 35, 37, 39, 42, 44, 43, 45, 47, 46, 48 };
            int[] partDefect = { 3, 2, 3, 3, 4, 3, 2, 3, 4, 3, 2, 3 };
            int[] solderDefect = { 2, 2, 1, 2, 1, 2, 1, 1, 2, 2, 1, 1 };
            int[] scrap = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };

            for (int i = 0; i < 12; i++)
            {
                int month = i + 1; // 1~12

                sNormal.Points.AddXY(month, normal[i]);
                sPartDefect.Points.AddXY(month, partDefect[i]);
                sSolderDefect.Points.AddXY(month, solderDefect[i]);
                sScrap.Points.AddXY(month, scrap[i]);
            }

            // 5) 레전드
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
            Color normalColor = Color.FromArgb(100, 181, 246); // 파랑
            Color partDefectColor = Color.FromArgb(255, 167, 38); // 주황
            Color solderDefColor = Color.FromArgb(158, 158, 158); // 회색
            Color scrapColor = Color.FromArgb(244, 67, 54); // 빨강
            var categories = new (string name, int value, Color color)[]
            {
                ("정상", 820, normalColor),
                ("부품불량", 14, partDefectColor),
                ("납땜불량", 28, solderDefColor),
                ("폐기",     8, scrapColor),
            };

            // 2) 차트 초기화
            var chart = DefectTypePieChart;
            chart.Series.Clear();
            chart.Legends.Clear();              // ✅ 기본 레전드 제거
            chart.ChartAreas.Clear();

            var area = new ChartArea("area");
            area.BackColor = Color.Transparent;
            // 도넛 크기 동일하게 맞추고 싶으면 Position/InnerPlotPosition도 고정
            area.Position.Auto = false;
            area.Position.X = 5; area.Position.Y = 5; area.Position.Width = 90; area.Position.Height = 90;
            area.InnerPlotPosition.Auto = false;
            area.InnerPlotPosition.X = 10; area.InnerPlotPosition.Y = 10; area.InnerPlotPosition.Width = 80; area.InnerPlotPosition.Height = 80;
            chart.ChartAreas.Add(area);

            var s = new Series("불량 카테고리")
            {
                ChartType = SeriesChartType.Doughnut
            };
            s["DoughnutRadius"] = "50";         // 도넛 구멍 크기(작을수록 구멍 큼)
            s["PieLabelStyle"] = "Disabled";   // ✅ 조각 라벨도 끔 (겹침 방지)
            s.IsValueShownAsLabel = false;

            foreach (var item in categories)
            {
                var pt = s.Points.AddXY(item.name, item.value);
                s.Points[pt].Color = item.color;  // ✅ 차트 조각 색상
            }
            chart.Series.Add(s);

            // 3) 커스텀 레전드(FlowLayoutPanel) 초기화
            flowPie.SuspendLayout();
            flowPie.Controls.Clear();
            flowPie.FlowDirection = FlowDirection.TopDown;
            flowPie.WrapContents = false;
            flowPie.AutoSize = false;    // Dock=Right라면 false가 깔끔
            flowPie.BackColor = Color.Transparent;

            // 4) 커스텀 아이템(색상 네모 + 텍스트) 추가
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
                    ForeColor = Color.Gainsboro // 밝은 테마면 Black
                };

                // (선택) 클릭 시 해당 조각 강조 효과
                EventHandler clickHandler = (_, __) =>
                {
                    foreach (var p in s.Points) { p.BorderWidth = 0; }
                    // 같은 이름의 포인트 찾아 강조
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

        }
    }
} 
