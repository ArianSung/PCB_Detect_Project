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
        private readonly System.Windows.Forms.Timer _refreshTimer;
        private List<DailyStatistics> _yearDailyStats = new List<DailyStatistics>();
        private int _currentYear;  // 일단 2025년 기준 (원하면 DateTime.Today.Year 쓰면 됨)
        private int _currentMonth;   // 일단 10월 기준으로 테스트
        private readonly ToolTip _monthlyChartToolTip = new ToolTip();
        private string _lastToolTipKey = null;
        private string _selectedDefectCategoryName = "정상";    // 불량 유형 도넛
        private string _selectedMonthlyTargetCategoryName = "달성"; // 월 목표 도넛
                                                                  // 도넛(불량 유형) 초기화 여부 + 레이블 캐시
        private bool _isDefectPieInitialized = false;
        private Label _defectPieRateLabel;
        private readonly Dictionary<string, Label> _defectPieItemLabels = new Dictionary<string, Label>();
        // 월 목표 도넛: '달성', '미달성' 라인 Label 캐싱용
        private readonly Dictionary<string, Label> _monthlyTargetItemLabels = new();

        // 도넛(월 목표) 초기화 여부 + 레이블 캐시
        private bool _isMonthlyTargetInitialized = false;
        private Label _monthlyTargetRateLabel;
        private Label _monthlyTargetTargetLabel;
        private Label _monthlyTargetActualLabel;




        public StatisticsView()
        {
            InitializeComponent();
            // ★ 타이머 생성 및 기본 설정
            _refreshTimer = new System.Windows.Forms.Timer();
            _refreshTimer.Interval = 5_000; // 5초마다 새로고침
            _refreshTimer.Tick += RefreshTimer_Tick;

            // 이 컨트롤이 dispose 되면 타이머도 정리
            this.Disposed += (s, e) =>
            {
                _refreshTimer.Stop();
                _refreshTimer.Dispose();
            };

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

            _monthlyChartToolTip.AutoPopDelay = 5000;
            _monthlyChartToolTip.InitialDelay = 200;
            _monthlyChartToolTip.ReshowDelay = 100;

            UiStyleHelper.MakeRoundedPanel(cardMonthlyLine, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardMonthlyAccum, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardDefectPie, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardMonthlyTarget, radius: 16, back: Color.FromArgb(44, 44, 44));

            UiStyleHelper.AddShadowRoundedPanel(cardMonthlyLine, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardMonthlyAccum, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardDefectPie, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardMonthlyTarget, 16);

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
            SetupMonthlyTargetChart();
            UpdateMonthLabels();

            _refreshTimer.Start();
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
        private void RefreshTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                // 현재 선택된 연도 기준으로 다시 로드 (연도가 바뀐 건 dtpMonth_ValueChanged에서 이미 반영됨)
                LoadDailyStatisticsForYear(_currentYear);

                // 현재 _currentYear / _currentMonth 기준으로 차트 다시 그림
                SetupMonthlyLineChart();
                SetupMonthlyAccumChart();
                SetupDefectTypePieChart();
                SetupMonthlyTargetChart();
                // 월/연도 텍스트는 그대로라서 굳이 다시 안 불러도 됨 (원하면 UpdateMonthLabels() 호출해도 무방)
            }
            catch (Exception ex)
            {
                // 타이머 도는 중에 계속 MessageBox 뜨면 짜증나니까 로그만 남기기
                Console.WriteLine($"[StatisticsView.RefreshTimer_Tick] {ex.Message}");
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

            ApplyMonthlyLineLegendFilter();
        }
        private void ApplyMonthlyLineLegendFilter()
        {
            var chart = MonthlyLineChart;

            var selectedSet = chart.Tag as HashSet<string>;
            int totalSeries = chart.Series.Count;

            // 선택된 게 없거나, 전부 선택된 상태면 => 전체 보기
            bool showAll = (selectedSet == null) ||
                           (selectedSet.Count == 0) ||
                           (selectedSet.Count >= totalSeries);

            foreach (var s in chart.Series)
            {
                bool isSelected = showAll || selectedSet.Contains(s.Name);

                if (isSelected)
                {
                    // 👉 이름별 원래 색 복원
                    if (s.Name == "정상")
                        s.Color = Color.FromArgb(100, 181, 246);
                    else if (s.Name == "부품불량")
                        s.Color = Color.Orange;
                    else if (s.Name == "납땜불량")
                        s.Color = Color.FromArgb(158, 158, 158);
                    else if (s.Name == "폐기")
                        s.Color = Color.Red;

                    s.MarkerStyle = MarkerStyle.Circle;
                    s.MarkerSize = 7;   // 네가 설정한 도트 크기
                }
                else
                {
                    // 숨기기
                    s.Color = Color.Transparent;
                    s.MarkerStyle = MarkerStyle.None;
                }

                // 레전드는 항상 보이게
                s.IsVisibleInLegend = true;
            }
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
            s.MarkerSize = 7;
            s.MarkerColor = color;

            // 기본 툴팁 포맷 (백업용)
            //s.ToolTip = "#SERIESNAME\n#VALX일: #VALY개";

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
            area.AxisY.Maximum = 40000;  // 연간 누적 기준, 필요하면 나중에 동적으로 조정
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
            sNormal["PointWidth"] = "0.7";

            Series sPartDefect = new Series("부품불량")
            {
                ChartArea = "Main",
                ChartType = SeriesChartType.StackedColumn,
                Color = partDefectColor
            };
            sPartDefect["PointWidth"] = "0.7";

            Series sSolderDefect = new Series("납땜불량")
            {
                ChartArea = "Main",
                ChartType = SeriesChartType.StackedColumn,
                Color = solderDefColor
            };
            sSolderDefect["PointWidth"] = "0.7";

            Series sScrap = new Series("폐기")
            {
                ChartArea = "Main",
                ChartType = SeriesChartType.StackedColumn,
                Color = scrapColor
            };
            sScrap["PointWidth"] = "0.7";

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

            // 🔥 현재 월 데이터 집계
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

            // =============================
            // 1) 처음 한 번만 차트 & 레전드 구조 생성
            // =============================
            if (!_isDefectPieInitialized)
            {
                _isDefectPieInitialized = true;

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

                // 포인트 생성 (이름 고정)
                foreach (var item in categories)
                {
                    int pt = s.Points.AddXY(item.name, item.value);
                    s.Points[pt].Color = item.color;
                    s.Points[pt].AxisLabel = item.name;
                }
                chart.Series.Add(s);

                // 선택값 없으면 기본 "정상"
                if (_selectedDefectCategoryName != "정상" &&
                     _selectedDefectCategoryName != "부품불량" &&
                     _selectedDefectCategoryName != "납땜불량" &&
                     _selectedDefectCategoryName != "폐기")
                {
                    _selectedDefectCategoryName = "정상";
                }


                // 커스텀 레전드(flowPie) 한 번만 만들어두고 라벨만 캐시
                flowPie.SuspendLayout();
                flowPie.Controls.Clear();
                flowPie.FlowDirection = FlowDirection.TopDown;
                flowPie.WrapContents = false;
                flowPie.AutoSize = false;
                flowPie.BackColor = Color.Transparent;

                // 상단 큰 % 라벨 패널
                var ratePanel = new Panel
                {
                    Height = 50,
                    Width = flowPie.Width - 16,
                    BackColor = Color.Transparent,
                    Margin = new Padding(0, 0, 0, 8),
                };

                var rateLabel = new Label
                {
                    AutoSize = false,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("맑은 고딕", 18, FontStyle.Bold),
                    ForeColor = normalColor
                };

                _defectPieRateLabel = rateLabel;

                ratePanel.Controls.Add(rateLabel);
                flowPie.Controls.Add(ratePanel);

                // 각 카테고리 라인(색 네모 + 텍스트 라벨)
                _defectPieItemLabels.Clear();

                foreach (var item in categories)
                {
                    string localName = item.name;
                    Color localColor = item.color;

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
                        BackColor = localColor,
                        Left = 0,
                        Top = (row.Height - 12) / 2
                    };

                    var lbl = new Label
                    {
                        AutoSize = true,
                        Left = 20,
                        Top = (row.Height - 16) / 2,
                        ForeColor = Color.Gainsboro
                    };

                    _defectPieItemLabels[localName] = lbl;

                    EventHandler clickHandler = (_, __) =>
                    {
                        _selectedDefectCategoryName = localName;

                        // 1) 도넛 조각 하이라이트
                        foreach (var p in s.Points) p.BorderWidth = 0;
                        var target = s.Points.FirstOrDefault(p => p.AxisLabel == localName);
                        if (target != null)
                        {
                            target.BorderColor = Color.White;
                            target.BorderWidth = 3;
                        }

                        // 2) 현재 값 기준 퍼센트 다시 계산
                        double total = s.Points.Sum(p => p.YValues[0]);
                        double val = target?.YValues[0] ?? 0;
                        double rate = total > 0 ? (val * 100.0 / total) : 0.0;

                        if (_defectPieRateLabel != null)
                        {
                            _defectPieRateLabel.Text = $"{rate:0.0}%";
                            _defectPieRateLabel.ForeColor = target?.Color ?? localColor;
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
                    flowPie.Controls.Add(row);
                }

                flowPie.ResumeLayout();
            }

            // =============================
            // 2) 여기부터는 "데이터만 업데이트"
            // =============================

            var series = chart.Series["불량 카테고리"];

            // 값 업데이트
            foreach (var item in categories)
            {
                var point = series.Points.FirstOrDefault(p => p.AxisLabel == item.name);
                if (point != null)
                {
                    point.YValues[0] = item.value;
                    point.Color = item.color;
                }

                if (_defectPieItemLabels.TryGetValue(item.name, out var lbl))
                {
                    lbl.Text = $"{item.name}  {item.value}개";
                }
            }

            // 선택된 조각 다시 하이라이트 (없으면 기본 정상)
            foreach (var p in series.Points) p.BorderWidth = 0;

            var selected = series.Points.FirstOrDefault(p => p.AxisLabel == _selectedDefectCategoryName);
            if (selected == null)
            {
                selected = series.Points.FirstOrDefault(p => p.AxisLabel == "정상");
                _selectedDefectCategoryName = selected?.AxisLabel ?? "정상";
            }

            if (selected != null)
            {
                selected.BorderColor = Color.White;
                selected.BorderWidth = 3;
            }

            // 상단 % 라벨 갱신
            if (_defectPieRateLabel != null)
            {
                double total = series.Points.Sum(p => p.YValues[0]);
                double val = selected?.YValues[0] ?? 0;
                double rate = total > 0 ? (val * 100.0 / total) : 0.0;

                _defectPieRateLabel.Text = $"{rate:0.0}%";
                _defectPieRateLabel.ForeColor = selected?.Color ?? normalColor;
            }
        }


        private void SetupMonthlyTargetChart()
        {
            // 🔹 1) 월 목표 / 실제 생산량 계산
            int targetMonthly = 30000; // 월 목표 생산량

            var monthData = _yearDailyStats
                .Where(d => d.StatDate.Year == _currentYear && d.StatDate.Month == _currentMonth)
                .ToList();

            int actualProduction = monthData.Sum(d => d.NormalCount);
            if (actualProduction > targetMonthly)
                actualProduction = targetMonthly;

            int remaining = Math.Max(targetMonthly - actualProduction, 0);

            var chart = MonthlyTargetChart;

            // =============================
            // 1) 처음 한 번만 차트 & FlowPanel 구조 생성
            // =============================
            if (!_isMonthlyTargetInitialized)
            {
                _isMonthlyTargetInitialized = true;

                chart.Series.Clear();
                chart.ChartAreas.Clear();
                chart.Legends.Clear();
                chart.BackColor = Color.Transparent;
                chart.BorderlineWidth = 0;

                var area = new ChartArea("area");
                area.BackColor = Color.Transparent;

                area.Position.Auto = false;
                area.Position.X = 5;
                area.Position.Y = 5;
                area.Position.Width = 90;
                area.Position.Height = 90;

                area.InnerPlotPosition.Auto = false;
                area.InnerPlotPosition.X = 10;
                area.InnerPlotPosition.Y = 10;
                area.InnerPlotPosition.Width = 80;
                area.InnerPlotPosition.Height = 80;

                chart.ChartAreas.Add(area);

                var s = new Series("월 목표 생산량")
                {
                    ChartType = SeriesChartType.Doughnut
                };
                s["DoughnutRadius"] = "50";
                s["PieLabelStyle"] = "Disabled";
                s.IsValueShownAsLabel = false;

                // 초깃값 포인트 추가
                var categories = new (string name, int value, Color color)[]
                {
            ("달성",   actualProduction, Color.FromArgb(100, 181, 246)),
            ("미달성", remaining,        Color.FromArgb(66, 66, 66)),
                };

                foreach (var item in categories)
                {
                    int idx = s.Points.AddXY(item.name, item.value);
                    s.Points[idx].Color = item.color;
                    s.Points[idx].AxisLabel = item.name;
                }

                chart.Series.Add(s);

                // 기본 선택값
                if (_selectedMonthlyTargetCategoryName != "달성" &&
                    _selectedMonthlyTargetCategoryName != "미달성")
                {
                    _selectedMonthlyTargetCategoryName = "달성";
                }

                // FlowLayoutPanel 초기 구성
                flowLegendMonthlyTarget.SuspendLayout();
                flowLegendMonthlyTarget.Controls.Clear();
                flowLegendMonthlyTarget.FlowDirection = FlowDirection.TopDown;
                flowLegendMonthlyTarget.WrapContents = false;
                flowLegendMonthlyTarget.AutoSize = false;
                flowLegendMonthlyTarget.BackColor = Color.Transparent;

                // 1) 퍼센트 라벨
                var ratePanel = new Panel
                {
                    Height = 50,
                    Width = flowLegendMonthlyTarget.Width - 16,
                    BackColor = Color.Transparent,
                    Margin = new Padding(0, 0, 0, 8),
                };

                var rateLabel = new Label
                {
                    AutoSize = false,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("맑은 고딕", 18, FontStyle.Bold),
                };

                _monthlyTargetRateLabel = rateLabel;

                ratePanel.Controls.Add(rateLabel);
                flowLegendMonthlyTarget.Controls.Add(ratePanel);

                // 2) 월 목표 라벨
                var targetPanel = new Panel
                {
                    Height = 24,
                    Width = flowLegendMonthlyTarget.Width - 16,
                    BackColor = Color.Transparent,
                    Margin = new Padding(0, 0, 0, 2),
                };

                var targetLabel = new Label
                {
                    AutoSize = false,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new Font("맑은 고딕", 9),
                    ForeColor = Color.Gainsboro
                };

                _monthlyTargetTargetLabel = targetLabel;

                targetPanel.Controls.Add(targetLabel);
                flowLegendMonthlyTarget.Controls.Add(targetPanel);

                // 3) 실제 생산량 라벨
                var actualPanel = new Panel
                {
                    Height = 24,
                    Width = flowLegendMonthlyTarget.Width - 16,
                    BackColor = Color.Transparent,
                    Margin = new Padding(0, 0, 0, 8),
                };

                var actualLabel = new Label
                {
                    AutoSize = false,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new Font("맑은 고딕", 9),
                    ForeColor = Color.Gainsboro
                };

                _monthlyTargetActualLabel = actualLabel;

                actualPanel.Controls.Add(actualLabel);
                flowLegendMonthlyTarget.Controls.Add(actualPanel);

                // 4) 달성/미달성 라인들 (클릭 이벤트)
                foreach (var item in categories)
                {
                    string localName = item.name;
                    int localValue = item.value;
                    Color localColor = item.color;

                    var row = new Panel
                    {
                        Height = 24,
                        Width = flowLegendMonthlyTarget.Width - 16,
                        BackColor = Color.Transparent,
                        Margin = new Padding(0, 2, 0, 2),
                    };

                    var swatch = new Panel
                    {
                        Width = 12,
                        Height = 12,
                        BackColor = localColor,
                        Left = 0,
                        Top = (row.Height - 12) / 2
                    };

                    var lbl = new Label
                    {
                        AutoSize = true,
                        Left = 20,
                        Top = (row.Height - 16) / 2,
                        ForeColor = Color.Gainsboro,
                        Text = $"{localName}  {localValue:N0}개"   // ✅ 초기 텍스트 세팅
                    };

                    // ✅ 나중에 값 업데이트할 수 있도록 캐싱
                    _monthlyTargetItemLabels[localName] = lbl;

                    EventHandler clickHandler = (_, __) =>
                    {
                        _selectedMonthlyTargetCategoryName = localName;

                        foreach (var p in s.Points) p.BorderWidth = 0;
                        var targetPoint = s.Points.FirstOrDefault(p => p.AxisLabel == localName);
                        if (targetPoint != null)
                        {
                            targetPoint.BorderColor = Color.White;
                            targetPoint.BorderWidth = 3;
                        }

                        double total = s.Points.Sum(p => p.YValues[0]);
                        double val = targetPoint?.YValues[0] ?? 0;
                        double rate = total > 0 ? (val * 100.0 / total) : 0.0;

                        if (_monthlyTargetRateLabel != null)
                        {
                            _monthlyTargetRateLabel.Text = $"{rate:0.#}%";
                            _monthlyTargetRateLabel.ForeColor =
                                (localName == "달성")
                                ? Color.FromArgb(100, 181, 246)
                                : Color.FromArgb(244, 67, 54);
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
                    flowLegendMonthlyTarget.Controls.Add(row);
                }

                flowLegendMonthlyTarget.ResumeLayout();
            }

            // =============================
            // 2) 데이터만 업데이트
            // =============================

            var series = chart.Series["월 목표 생산량"];

            var pAch = series.Points.First(p => p.AxisLabel == "달성");
            var pMiss = series.Points.First(p => p.AxisLabel == "미달성");

            pAch.YValues[0] = actualProduction;
            pMiss.YValues[0] = remaining;

            // 선택 하이라이트 복원
            foreach (var p in series.Points) p.BorderWidth = 0;

            var selectedPoint = series.Points.FirstOrDefault(p => p.AxisLabel == _selectedMonthlyTargetCategoryName);
            if (selectedPoint == null)
            {
                selectedPoint = series.Points.FirstOrDefault(p => p.AxisLabel == "달성");
                _selectedMonthlyTargetCategoryName = selectedPoint?.AxisLabel ?? "달성";
            }

            if (selectedPoint != null)
            {
                selectedPoint.BorderColor = Color.White;
                selectedPoint.BorderWidth = 3;
            }

            // 텍스트 라벨 갱신
            if (_monthlyTargetTargetLabel != null)
                _monthlyTargetTargetLabel.Text = $"월 목표: {targetMonthly:N0}개";

            if (_monthlyTargetActualLabel != null)
                _monthlyTargetActualLabel.Text = $"실제: {actualProduction:N0}개";

            if (_monthlyTargetItemLabels.TryGetValue("달성", out var achLbl))
                achLbl.Text = $"달성  {actualProduction:N0}개";

            if (_monthlyTargetItemLabels.TryGetValue("미달성", out var missLbl))
                missLbl.Text = $"미달성  {remaining:N0}개"; 

            // 퍼센트 라벨 갱신
            if (_monthlyTargetRateLabel != null)
            {
                double total = series.Points.Sum(p => p.YValues[0]);
                double val = selectedPoint?.YValues[0] ?? 0;
                double rate = total > 0 ? (val * 100.0 / total) : 0.0;

                _monthlyTargetRateLabel.Text = $"{rate:0.#}%";
                _monthlyTargetRateLabel.ForeColor =
                    (_selectedMonthlyTargetCategoryName == "달성")
                    ? Color.FromArgb(100, 181, 246)
                    : Color.FromArgb(244, 67, 54);
            }
        }



        private void UpdateMonthLabels()
        {
            // 예: 2025년 11월
            string ymText = $"{_currentYear}년 {_currentMonth:00}월";
            string ymText1 = $"{_currentMonth:00}월";

            // 각 카드별 제목 텍스트
            if (lblMonthlyLineTitle != null)
                lblMonthlyLineTitle.Text = $"{ymText} 일별 생산/불량 추이";

            if (lblMonthlyAccumTitle != null)
                lblMonthlyAccumTitle.Text = $"{_currentYear}년 월별 누적 현황";

            if (lblDefectPieTitle != null)
                lblDefectPieTitle.Text = $"{ymText1} 불량 유형 비율";

            if (lblMonthlyTargetTitle != null)
                lblMonthlyTargetTitle.Text = $"{ymText1} 생산 목표 달성률";     
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

            // 4) 선택 상태를 HashSet<string> 으로 관리
            var selectedSet = chart.Tag as HashSet<string>;
            if (selectedSet == null)
            {
                selectedSet = new HashSet<string>();
                chart.Tag = selectedSet;
            }

            int totalSeries = chart.Series.Count;

            // 🔹 현재 상태가 "4개 다 보이는 상태"인지 판단
            //  - selectedSet.Count == 0  → Tag로 필터를 안 쓰는 '전체 보기'
            //  - selectedSet.Count == totalSeries → 4개를 전부 '선택'한 상태
            bool allVisibleBefore =
                (selectedSet.Count == 0) ||
                (selectedSet.Count >= totalSeries);

            if (allVisibleBefore)
            {
                // ✅ 4개 다 보이는 상태에서 클릭 → 클릭한 것만 남기기
                selectedSet.Clear();
                selectedSet.Add(clickedSeriesName);
            }
            else
            {
                // ✅ 일부만 보이는 상태 → 토글 동작
                if (selectedSet.Contains(clickedSeriesName))
                    selectedSet.Remove(clickedSeriesName);
                else
                    selectedSet.Add(clickedSeriesName);
            }
            ApplyMonthlyLineLegendFilter(); 
        }
        

        private void MonthlyLineChart_MouseMove(object sender, MouseEventArgs e)
        {
            var chart = (Chart)sender;

            // 마우스 위치로 히트 테스트
            var hit = chart.HitTest(e.X, e.Y);

            // 1) 레전드 위에 있으면 손가락 커서
            if (hit.ChartElementType == ChartElementType.LegendItem)
            {
                chart.Cursor = Cursors.Hand;
            }
            else
            {
                chart.Cursor = Cursors.Default;
            }

            // 2) 데이터 포인트 위에 있으면 “고급” 툴팁 보여주기
            if (hit.ChartElementType == ChartElementType.DataPoint &&
                hit.PointIndex >= 0 &&
                hit.Series != null)
            {
                var series = hit.Series;
                var point = series.Points[hit.PointIndex];

                int day = (int)point.XValue;         // 1 ~ 31
                int value = (int)point.YValues[0];   // 해당 시리즈 값

                // ✅ 같은 포인트에서 계속 움직일 땐 다시 툴팁 안 띄우기
                string key = $"{series.Name}_{day}";
                if (_lastToolTipKey == key)
                {
                    return; // 이미 이 포인트에 대한 툴팁이 떠 있는 상태
                }
                _lastToolTipKey = key;

                // 현재 월 데이터에서 그 날짜 레코드 찾기
                var rec = _yearDailyStats
                    .FirstOrDefault(d =>
                        d.StatDate.Year == _currentYear &&
                        d.StatDate.Month == _currentMonth &&
                        d.StatDate.Day == day);

                if (rec != null)
                {
                    int total = rec.TotalInspections;
                    int normal = rec.NormalCount;
                    int comp = rec.ComponentDefectCount;
                    int solder = rec.SolderDefectCount;
                    int scrap = rec.DiscardCount;
                    int defectSum = comp + solder + scrap;

                    double defectRate = 0;
                    if (total > 0)
                        defectRate = defectSum * 100.0 / total;

                    // 고급 툴팁 텍스트 구성
                    var sb = new StringBuilder();
                    sb.AppendLine($"{rec.StatDate:yyyy-MM-dd}");
                    sb.AppendLine($"[{series.Name}] {value:N0}개");
                    sb.AppendLine($"총 검사: {total:N0}개");
                    sb.AppendLine($"정상: {normal:N0} / 부품: {comp:N0} / 납땜: {solder:N0} / 폐기: {scrap:N0}");
                    sb.AppendLine($"불량률: {defectRate:0.00}% ({defectSum:N0}개)");

                    // 마우스 위치 기준으로 약간 옆/위에 표시
                    _monthlyChartToolTip.Show(
                        sb.ToString(),
                        chart,
                        e.Location.X + 15,
                        e.Location.Y - 15
                    );
                }
                else
                {
                    _monthlyChartToolTip.Hide(chart);
                    _lastToolTipKey = null;
                }
            }
            else
            {
                // 포인트 위가 아니면 툴팁 숨김 + 상태 리셋
                _monthlyChartToolTip.Hide(chart);
                _lastToolTipKey = null;
            }
        }

        private void MonthlyAccumChart_MouseMove(object sender, MouseEventArgs e)
        {
            var chart = (Chart)sender;

            // 마우스 위치로 히트 테스트
            var hit = chart.HitTest(e.X, e.Y);

            // 1) 데이터 포인트 위에 있는지 확인
            if (hit.ChartElementType == ChartElementType.DataPoint &&
                hit.PointIndex >= 0 &&
                hit.Series != null)
            {
                var series = hit.Series;
                var point = series.Points[hit.PointIndex];

                int month = (int)point.XValue;      // 1~12
                int value = (int)point.YValues[0];  // 해당 시리즈 값

                // 깜빡임 방지용 키 (같은 월/같은 시리즈면 재생성 안 함)
                string key = $"Accum_{series.Name}_{month}";
                if (_lastToolTipKey == key)
                    return;

                _lastToolTipKey = key;

                // 🔹 이 월 데이터 전체 다시 합산
                var monthGroup = _yearDailyStats
                    .Where(d => d.StatDate.Year == _currentYear &&
                                d.StatDate.Month == month)
                    .ToList();

                int sumNormal = monthGroup.Sum(d => d.NormalCount);
                int sumPartDefect = monthGroup.Sum(d => d.ComponentDefectCount);
                int sumSolderDefect = monthGroup.Sum(d => d.SolderDefectCount);
                int sumScrap = monthGroup.Sum(d => d.DiscardCount);

                int total = sumNormal + sumPartDefect + sumSolderDefect + sumScrap;
                int defectSum = sumPartDefect + sumSolderDefect + sumScrap;

                double defectRate = 0;
                if (total > 0)
                    defectRate = defectSum * 100.0 / total;

                // 예쁘게 텍스트 구성
                var sb = new StringBuilder();
                sb.AppendLine($"{_currentYear}년 {month:00}월");
                sb.AppendLine($"[{series.Name}] {value:N0}개");
                sb.AppendLine($"정상: {sumNormal:N0} / 부품: {sumPartDefect:N0} / 납땜: {sumSolderDefect:N0} / 폐기: {sumScrap:N0}");
                sb.AppendLine($"총 검사: {total:N0}개");
                sb.AppendLine($"불량률: {defectRate:0.00}% ({defectSum:N0}개)");

                _monthlyChartToolTip.Show(
                    sb.ToString(),
                    chart,
                    e.Location.X + 15,
                    e.Location.Y - 15,
                    3000 // 3초 유지
                );
            }
            else
            {
                // 포인트 위가 아니면 툴팁 숨기고 상태 리셋
                _monthlyChartToolTip.Hide(chart);
                _lastToolTipKey = null;
            }
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
                    using (var wb = new XLWorkbook())
                    {
                        var sheetName = $"{_currentYear}-{_currentMonth:00}";
                        var ws = wb.Worksheets.Add(sheetName);

                        // ─────────────────────
                        // 3-1) 헤더 행
                        // ─────────────────────
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
                        headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        headerRange.Style.Font.FontColor = XLColor.Black;

                        // ─────────────────────
                        // 3-2) 데이터 행
                        // ─────────────────────
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

                            double defectRate = 0;
                            int defectSum = d.ComponentDefectCount + d.SolderDefectCount + d.DiscardCount;
                            if (d.TotalInspections > 0)
                                defectRate = defectSum * 1.0 / d.TotalInspections;

                            ws.Cell(row, 7).Value = defectRate;                  // 0.1234
                            ws.Cell(row, 7).Style.NumberFormat.Format = "0.00%"; // 12.34%

                            row++;
                        }

                        int lastRow = row - 1;

                        // 숫자 컬럼 B~F: 정수 형식
                        var dataNumberRange = ws.Range(2, 2, lastRow, 6);
                        dataNumberRange.Style.NumberFormat.Format = "0";
                        dataNumberRange.Style.Font.FontColor = XLColor.Black;

                        // 불량률 컬럼 G: 퍼센트 형식은 위에서 설정, 색만 맞춰줌
                        ws.Column(7).Style.Font.FontColor = XLColor.Black;

                        // ─────────────────────
                        // 3-3) 월 생산 목표 요약 (정상 기준)
                        // ─────────────────────
                        int targetMonthly = 30000; // 월 목표 생산량
                        int actualProduction = monthData.Sum(d => d.NormalCount);

                        if (actualProduction > targetMonthly)
                            actualProduction = targetMonthly;

                        double achievementRate = targetMonthly > 0
                            ? (actualProduction * 1.0 / targetMonthly)
                            : 0.0;

                        int summaryRow = lastRow + 2;

                        ws.Cell(summaryRow, 1).Value = "월 생산 목표";
                        ws.Cell(summaryRow, 2).Value = targetMonthly;

                        ws.Cell(summaryRow + 1, 1).Value = "월 실제 생산량";
                        ws.Cell(summaryRow + 1, 2).Value = actualProduction;

                        ws.Cell(summaryRow + 2, 1).Value = "달성률";
                        ws.Cell(summaryRow + 2, 2).Value = achievementRate;
                        ws.Cell(summaryRow + 2, 2).Style.NumberFormat.Format = "0.00%";

                        // 요약 라벨/값 스타일 (폰트만, 정렬은 아래에서 한 번에 중앙정렬)
                        var summaryLabelRange = ws.Range(summaryRow, 1, summaryRow + 2, 1);
                        summaryLabelRange.Style.Font.Bold = true;
                        summaryLabelRange.Style.Font.FontColor = XLColor.Black;

                        var summaryValueRange = ws.Range(summaryRow, 2, summaryRow + 2, 2);
                        summaryValueRange.Style.Font.FontColor = XLColor.Black;

                        // 숫자 2개(목표/실제)는 정수로
                        ws.Cell(summaryRow, 2).Style.NumberFormat.Format = "0";
                        ws.Cell(summaryRow + 1, 2).Style.NumberFormat.Format = "0";
                        // 달성률은 이미 % 형식

                        // ─────────────────────
                        // 3-4) 전체 중앙 정렬 + 컬럼 너비
                        // ─────────────────────

                        // 메인 테이블 + 요약까지 전체 중앙 정렬
                        var allRange = ws.Range(1, 1, summaryRow + 2, 7); // 1~7열만 사용 중
                        allRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        allRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                        // 컬럼 너비 자동 맞춤
                        ws.Columns().AdjustToContents();

                        // 기본 최소 폭 보정
                        for (int col = 1; col <= 7; col++)
                        {
                            if (ws.Column(col).Width < 12)
                                ws.Column(col).Width = 12;
                        }

                        // 요약 텍스트용 1열 약간 넓게
                        if (ws.Column(1).Width < 17)
                            ws.Column(1).Width = 17;

                        // 숫자 많이 들어가는 2열도 넉넉하게
                        if (ws.Column(2).Width < 15)
                            ws.Column(2).Width = 15;

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
            SetupMonthlyTargetChart();
            UpdateMonthLabels();
        }
    }
} 
