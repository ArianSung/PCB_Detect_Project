using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using pcb_monitoring_program;

namespace pcb_monitoring_program.Views.Dashboard
{

    public partial class DashboardView : UserControl
    {
        private int[] _hourlyNormal;
        private int[] _hourlyPartDefect;
        private int[] _hourlySolderDefect;
        private int[] _hourlyScrap;
        public DashboardView()
        {
            InitializeComponent();
        }

        private void DashboardView_Load(object sender, EventArgs e)
        {
            InitDummyData();

            UiStyleHelper.MakeRoundedPanel(cardRate, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardCategory, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardTarget, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardBoxRate, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardTrend, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardFrontPCB, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardBackPCB, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardHourly, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardLog, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardTop, radius: 16, back: Color.FromArgb(44, 44, 44));

            UiStyleHelper.AddShadowRoundedPanel(cardRate, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardCategory, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardTarget, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardBoxRate, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardTrend, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardFrontPCB, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardBackPCB, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardHourly, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardLog, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardTop, 16);

            SetupDefectRateChart();
            SetupDefectCategoryCharts();
            SetupDailyTargetCharts();
            SetupBoxRateChart();
            SetupDefectTrendChart();
            SetupHourlyInspectionChart();
        }

        private void InitDummyData()
        {
            // 0~23시 (길이 24)
            _hourlyNormal = new int[]
            {
                30, 32, 28, 25, 23, 24, 26, 28, 32, 35, 40, 38,
                36, 34, 32, 30, 29, 31, 33, 37, 39, 41, 42, 40
            };

            _hourlyPartDefect = new int[]
            {
                5, 4, 3, 4, 3, 3, 4, 5, 14, 7, 6, 5,
                4, 4, 3, 3, 3, 4, 4, 5, 13, 5, 4, 4
            };

            _hourlySolderDefect = new int[]
            {
                2, 2, 1, 2, 1, 1, 1, 2, 10, 3, 3, 2,
                2, 1, 1, 1, 1, 2, 2, 2, 8, 2, 2, 2
            };

            _hourlyScrap = new int[]
            {
                1, 1, 2, 1, 5, 1, 2, 1, 5, 1, 1, 2,
                1, 3, 1, 4, 1, 1, 3, 1, 3, 1, 1, 4
            };
        }

        private void SetupDefectRateChart()
        {

            int totalNormal = _hourlyNormal.Sum();
            int totalPartDefect = _hourlyPartDefect.Sum();
            int totalSolderDefect = _hourlySolderDefect.Sum();
            int totalScrap = _hourlyScrap.Sum();

            int normalCount = totalNormal;
            int defectCount = totalPartDefect + totalSolderDefect + totalScrap;
            int totalCount = normalCount + defectCount;

            var rateData = new (string name, int value, Color color)[]
            {
                ("정상", normalCount, Color.FromArgb(76, 175, 80)),   // 초록
                ("불량", defectCount, Color.FromArgb(238, 99, 99))    // 빨강
            };

            // 2) 차트 초기화
            var chart = DefectRateChart;
            chart.Series.Clear();
            chart.Legends.Clear();
            chart.ChartAreas.Clear();

            var area = new ChartArea("area");
            area.BackColor = Color.Transparent;
            area.Position.Auto = false;
            area.Position.X = 5; area.Position.Y = 5; area.Position.Width = 90; area.Position.Height = 90;
            area.InnerPlotPosition.Auto = false;
            area.InnerPlotPosition.X = 10; area.InnerPlotPosition.Y = 10; area.InnerPlotPosition.Width = 80; area.InnerPlotPosition.Height = 80;
            chart.ChartAreas.Add(area);

            var s = new Series("전체 불량률")
            {
                ChartType = SeriesChartType.Doughnut
            };
            s["DoughnutRadius"] = "50";
            s["PieLabelStyle"] = "Disabled";
            s.IsValueShownAsLabel = false;

            foreach (var item in rateData)
            {
                var pt = s.Points.AddXY(item.name, item.value);
                s.Points[pt].Color = item.color;
            }
            chart.Series.Add(s);

            // 3) 커스텀 레전드(FlowLayoutPanel) 동적 생성 및 추가
            var flowLegendRate = new FlowLayoutPanel
            {
                Location = new Point(240, 12),
                Size = new Size(108, 215),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = false,
                BackColor = Color.Transparent
            };
            cardRate.Controls.Add(flowLegendRate);
            flowLegendRate.BringToFront();

            // 4) 불량률 표시 (크게, 눈에 띄게)
            double defectRate = (defectCount * 100.0) / totalCount;

            var ratePanel = new Panel
            {
                Height = 50,
                Width = flowLegendRate.Width - 16,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 8),
            };

            var rateLabel = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = $"{defectRate:0.#}%",
                Font = new Font("맑은 고딕", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(238, 99, 99)  // 빨간색
            };

            ratePanel.Controls.Add(rateLabel);
            flowLegendRate.Controls.Add(ratePanel);

            // 5) 전체/정상/불량 개수 표시
            var totalPanel = new Panel
            {
                Height = 24,
                Width = flowLegendRate.Width - 16,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 2),
            };

            var totalLabel = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Text = $"전체: {totalCount}개",
                Font = new Font("맑은 고딕", 9, FontStyle.Regular),
                ForeColor = Color.Gainsboro
            };

            totalPanel.Controls.Add(totalLabel);
            flowLegendRate.Controls.Add(totalPanel);

            var normalPanel = new Panel
            {
                Height = 24,
                Width = flowLegendRate.Width - 16,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 2),
            };

            var normalLabel = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Text = $"정상: {normalCount}개",
                Font = new Font("맑은 고딕", 9, FontStyle.Regular),
                ForeColor = Color.Gainsboro
            };

            normalPanel.Controls.Add(normalLabel);
            flowLegendRate.Controls.Add(normalPanel);

            var defectPanel = new Panel
            {
                Height = 24,
                Width = flowLegendRate.Width - 16,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 8),
            };

            var defectLabel = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Text = $"불량: {defectCount}개",
                Font = new Font("맑은 고딕", 9, FontStyle.Regular),
                ForeColor = Color.Gainsboro
            };

            defectPanel.Controls.Add(defectLabel);
            flowLegendRate.Controls.Add(defectPanel);

            // 6) 커스텀 아이템(색상 네모 + 텍스트) 추가
            foreach (var item in rateData)
            {
                var row = new Panel
                {
                    Height = 24,
                    Width = flowLegendRate.Width - 16,
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

                // 클릭 시 해당 조각 강조 효과
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
                flowLegendRate.Controls.Add(row);
            }
        }

        private void SetupDefectCategoryCharts()
        {
            var chart = DefectCategoryChart;

            int totalPartDefect = _hourlyPartDefect.Sum();
            int totalSolderDefect = _hourlySolderDefect.Sum();
            int totalScrap = _hourlyScrap.Sum();

            var categories = new (string name, int value, Color color)[]
            {
                ("부품불량", totalPartDefect,   Color.FromArgb(238, 99, 99)),
                ("납땜불량", totalSolderDefect, Color.FromArgb(255, 170, 0)),
                ("폐기",     totalScrap,        Color.FromArgb(120, 160, 255)),
            };

            // 2) 차트 초기화
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
            flowLegend.SuspendLayout();
            flowLegend.Controls.Clear();
            flowLegend.FlowDirection = FlowDirection.TopDown;
            flowLegend.WrapContents = false;
            flowLegend.AutoSize = false;    // Dock=Right라면 false가 깔끔
            flowLegend.BackColor = Color.Transparent;

            // 4) 커스텀 아이템(색상 네모 + 텍스트) 추가
            foreach (var item in categories)
            {
                var row = new Panel
                {
                    Height = 24,
                    Width = flowLegend.Width - 16,
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
                flowLegend.Controls.Add(row);
            }
            flowLegend.ResumeLayout();

        }

        private void SetupDailyTargetCharts()
        {
            // 1) 목표 및 실제 생산량 데이터
            int targetProduction = 1000;  // 목표 생산량
            int actualProduction = _hourlyNormal.Sum();  // 실제 생산량
            if (actualProduction > targetProduction)
                actualProduction = targetProduction;
            int remaining = Math.Max(0, targetProduction - actualProduction);  // 미달성

            // 2) 차트용 데이터 (달성 vs 미달성)
            var categories = new (string name, int value, Color color)[]
            {
                ("달성", actualProduction, Color.FromArgb(100, 181, 246)),    // 밝은 파란색
                ("미달성", remaining, Color.FromArgb(66, 66, 66))             // 어두운 회색
            };

            // 3) 차트 초기화
            var chart = DailyTargetChart;
            chart.Series.Clear();
            chart.Legends.Clear();
            chart.ChartAreas.Clear();

            var area = new ChartArea("area");
            area.BackColor = Color.Transparent;
            area.Position.Auto = false;
            area.Position.X = 5; area.Position.Y = 5; area.Position.Width = 90; area.Position.Height = 90;
            area.InnerPlotPosition.Auto = false;
            area.InnerPlotPosition.X = 10; area.InnerPlotPosition.Y = 10; area.InnerPlotPosition.Width = 80; area.InnerPlotPosition.Height = 80;
            chart.ChartAreas.Add(area);

            var s = new Series("목표 생산량")
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

            // 4) 커스텀 레전드(FlowLayoutPanel) 초기화
            flowLegendTarget.SuspendLayout();
            flowLegendTarget.Controls.Clear();
            flowLegendTarget.FlowDirection = FlowDirection.TopDown;
            flowLegendTarget.WrapContents = false;
            flowLegendTarget.AutoSize = false;
            flowLegendTarget.BackColor = Color.Transparent;

            // 5) 달성률 표시 (크게, 눈에 띄게)
            double achievementRate = (actualProduction * 100.0) / targetProduction;

            var ratePanel = new Panel
            {
                Height = 50,
                Width = flowLegendTarget.Width - 16,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 8),
            };

            var rateLabel = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = $"{achievementRate:0.#}%",
                Font = new Font("맑은 고딕", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 181, 246)  // 파란색
            };

            ratePanel.Controls.Add(rateLabel);
            flowLegendTarget.Controls.Add(ratePanel);

            // 6) 목표/실제 생산량 표시
            var targetPanel = new Panel
            {
                Height = 24,
                Width = flowLegendTarget.Width - 16,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 2),
            };

            var targetLabel = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Text = $"목표: {targetProduction}개",
                Font = new Font("맑은 고딕", 9, FontStyle.Regular),
                ForeColor = Color.Gainsboro
            };

            targetPanel.Controls.Add(targetLabel);
            flowLegendTarget.Controls.Add(targetPanel);

            var actualPanel = new Panel
            {
                Height = 24,
                Width = flowLegendTarget.Width - 16,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 8),
            };

            var actualLabel = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Text = $"실제: {actualProduction}개",
                Font = new Font("맑은 고딕", 9, FontStyle.Regular),
                ForeColor = Color.Gainsboro
            };

            actualPanel.Controls.Add(actualLabel);
            flowLegendTarget.Controls.Add(actualPanel);

            // 7) 커스텀 아이템(색상 네모 + 텍스트) 추가
            foreach (var item in categories)
            {
                var row = new Panel
                {
                    Height = 24,
                    Width = flowLegendTarget.Width - 16,
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

                // 클릭 시 해당 조각 강조 효과
                EventHandler clickHandler = (_, __) =>
                {
                    foreach (var p in s.Points) { p.BorderWidth = 0; }
                    var targetPoint = s.Points.FirstOrDefault(p => p.AxisLabel == item.name);
                    if (targetPoint != null)
                    {
                        targetPoint.BorderColor = Color.White;
                        targetPoint.BorderWidth = 3;
                    }
                };

                row.Cursor = Cursors.Hand;
                swatch.Cursor = Cursors.Hand;
                lbl.Cursor = Cursors.Hand;

                row.Click += clickHandler;
                swatch.Click += clickHandler;
                lbl.Click += clickHandler;

                row.Controls.Add(swatch);
                row.Controls.Add(lbl);
                flowLegendTarget.Controls.Add(row);
            }
            flowLegendTarget.ResumeLayout();
        }
        private void SetupBoxRateChart()
        {
            var boxData = new (string name, int current, int max, Color color)[]
            {
                ("정상",     2, 3, Color.FromArgb(100, 181, 246)),
                ("부품불량",  3, 3, Color.FromArgb(238,  99,  99)),
                ("납땜불량",  1, 3, Color.FromArgb(255, 170,   0)),
            };

            UiStyleHelper.ConfigureBoxRateChart(BoxRateChart, boxData);
        }
        private void SetupDefectTrendChart()
        {
            var chart = DefectTrendChart;   // 디자이너에서 만든 Chart 이름

            // 1) 초기화
            chart.DataSource = null;
            chart.Series.Clear();
            chart.ChartAreas.Clear();
            chart.Legends.Clear();

            chart.BackColor = Color.Transparent;
            chart.BorderlineWidth = 0;

            // 2) ChartArea + 축 설정
            var area = new ChartArea("Main");
            area.BackColor = Color.Transparent;

            // X축: 일자 (1~10 예시)
            area.AxisX.Minimum = 0;
            area.AxisX.Maximum = 23;
            area.AxisX.Interval = 1;
            area.AxisX.MajorGrid.Enabled = false;
            area.AxisX.LabelStyle.ForeColor = Color.Gainsboro;
            area.AxisX.LabelStyle.Font = new Font("맑은 고딕", 9);
            area.AxisX.LabelStyle.Format = "0시";

            // Y축: 불량률(%) 0~100
            area.AxisY.Minimum = 0;
            area.AxisY.Maximum = 60;
            area.AxisY.Interval = 10;
            area.AxisY.MajorGrid.Enabled = true;
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(70, 70, 70);
            area.AxisY.LabelStyle.ForeColor = Color.Gainsboro;
            area.AxisY.LabelStyle.Font = new Font("맑은 고딕", 9);

            chart.ChartAreas.Add(area);

            // 3) Series들 생성

            // 실제 불량률 (라인 + 작은 점)
            var sActual = new Series("실제 불량률")
            {
                ChartArea = "Main",
                ChartType = SeriesChartType.Line,
                BorderWidth = 2,
                Color = Color.SkyBlue
            };
            sActual.MarkerStyle = MarkerStyle.Circle;
            sActual.MarkerSize = 5;
            sActual.MarkerColor = sActual.Color;

            // 평균선
            var sAvg = new Series("평균")
            {
                ChartArea = "Main",
                ChartType = SeriesChartType.Line,
                BorderWidth = 2,
                Color = Color.Gold,
                BorderDashStyle = ChartDashStyle.Dash
            };

            // 상한선(UCL)
            var sUcl = new Series("상한선")
            {
                ChartArea = "Main",
                ChartType = SeriesChartType.Line,
                BorderWidth = 2,
                Color = Color.Red,
                BorderDashStyle = ChartDashStyle.Dot
            };

            // 하한선(LCL)
            var sLcl = new Series("하한선")
            {
                ChartArea = "Main",
                ChartType = SeriesChartType.Line,
                BorderWidth = 2,
                Color = Color.DeepSkyBlue,
                BorderDashStyle = ChartDashStyle.Dot
            };

            // 이탈 포인트 (큰 빨간 동그라미만)
            var sOutlier = new Series("이탈 포인트")
            {
                ChartArea = "Main",
                ChartType = SeriesChartType.Point,
                Color = Color.Red
            };
            sOutlier.MarkerStyle = MarkerStyle.Circle;
            sOutlier.MarkerSize = 10;
            sOutlier.MarkerBorderColor = Color.White;
            sOutlier.MarkerBorderWidth = 2;

            chart.Series.Add(sActual);
            chart.Series.Add(sAvg);
            chart.Series.Add(sUcl);
            chart.Series.Add(sLcl);
            chart.Series.Add(sOutlier);

            // 4) 임시 데이터 (나중에 DB 값으로 바꾸면 됨)
            double[] defectRates = new double[24];

            for (int hour = 0; hour < 24; hour++)
            {
                int normal = _hourlyNormal[hour];
                int part = _hourlyPartDefect[hour];
                int solder = _hourlySolderDefect[hour];
                int scrap = _hourlyScrap[hour];

                int total = normal + part + solder + scrap;
                int defects = part + solder + scrap;

                defectRates[hour] = total == 0 ? 0 : defects * 100.0 / total;
            }
            double avg = defectRates.Average();
            double ucl = 30.0;                      // 예시 상한선
            double lcl = 10.0;                      // 예시 하한선

            for (int hour = 0; hour < defectRates.Length; hour++)
            {
                double y = defectRates[hour];
                int x = hour;

                sActual.Points.AddXY(x, y);
                sAvg.Points.AddXY(x, avg);
                sUcl.Points.AddXY(x, ucl);
                sLcl.Points.AddXY(x, lcl);

                if (y > ucl)
                    sOutlier.Points.AddXY(x, y);
            }

            // 5) 레전드 기본 사용 (필요 없으면 지우거나 커스텀)
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
        private void SetupHourlyInspectionChart()
        {
            var chart = HourlyInspectionChart;

            // 1) 초기화
            chart.DataSource = null;
            chart.Series.Clear();
            chart.ChartAreas.Clear();
            chart.Legends.Clear();

            chart.BackColor = Color.Transparent;
            chart.BorderlineWidth = 0;

            // 2) ChartArea 설정
            var area = new ChartArea("Main");
            area.BackColor = Color.Transparent;

            // X축: 시간(0~23)
            area.AxisX.Minimum = 0;
            area.AxisX.Maximum = 23;
            area.AxisX.Interval = 1;
            area.AxisX.MajorGrid.Enabled = false;
            area.AxisX.LabelStyle.ForeColor = Color.Gainsboro;
            area.AxisX.LabelStyle.Font = new Font("맑은 고딕", 8);
            area.AxisX.LabelStyle.Format = "0시";

            // Y축: 검사 개수 (예시로 0~60)
            area.AxisY.Minimum = 0;
            area.AxisY.Maximum = 80;
            area.AxisY.Interval = 10;
            area.AxisY.MajorGrid.Enabled = true;
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(70, 70, 70);
            area.AxisY.LabelStyle.ForeColor = Color.Gainsboro;
            area.AxisY.LabelStyle.Font = new Font("맑은 고딕", 8);

            chart.ChartAreas.Add(area);

            // 3) Series 생성 (아래에서 위로 쌓이는 순서대로 추가하는 게 포인트)
            //    정상(파랑) → 부품불량(주황) → 납땜불량(노랑) → 폐기(빨강)
            Series sNormal = new Series("정상")
            {
                ChartArea = "Main",
                ChartType = SeriesChartType.StackedColumn,
                Color = Color.FromArgb(100, 181, 246)  // 파란색
            };

            Series sPartDefect = new Series("부품불량")
            {
                ChartArea = "Main",
                ChartType = SeriesChartType.StackedColumn,
                Color = Color.Orange  // 주황
            };

            Series sSolderDefect = new Series("납땜불량")
            {
                ChartArea = "Main",
                ChartType = SeriesChartType.StackedColumn,
                Color = Color.Yellow  // 노랑
            };

            Series sScrap = new Series("폐기")
            {
                ChartArea = "Main",
                ChartType = SeriesChartType.StackedColumn,
                Color = Color.Red     // 빨강
            };

            chart.Series.Add(sNormal);
            chart.Series.Add(sPartDefect);
            chart.Series.Add(sSolderDefect);
            chart.Series.Add(sScrap);

            // 4) 임시 데이터 (0~23시, 길이 24짜리 배열)
            //    나중에 DB 값으로 교체할 부분
            int[] normal = _hourlyNormal;
            int[] partDefect = _hourlyPartDefect;
            int[] solderDefect = _hourlySolderDefect;
            int[] scrap = _hourlyScrap;

            for (int hour = 0; hour < 24; hour++)
            {
                sNormal.Points.AddXY(hour, normal[hour]);
                sPartDefect.Points.AddXY(hour, partDefect[hour]);
                sSolderDefect.Points.AddXY(hour, solderDefect[hour]);
                sScrap.Points.AddXY(hour, scrap[hour]);
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
    }
}
