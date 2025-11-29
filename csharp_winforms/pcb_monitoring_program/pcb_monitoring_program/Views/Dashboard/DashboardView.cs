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
using pcb_monitoring_program.DatabaseManager;
using pcb_monitoring_program.DatabaseManager.Models;

namespace pcb_monitoring_program.Views.Dashboard
{

    public partial class DashboardView : UserControl
    {
        private readonly ToolTip _defectTrendToolTip = new ToolTip();
        private readonly ToolTip _hourlyChartToolTip = new ToolTip();
        private bool _isRefreshing = false;
        private System.Windows.Forms.Timer _refreshTimer;
        private string _lastDefectTrendKey = null;
        private string? _lastHourlyToolTipKey = null;
        private int[] _hourlyNormal;
        private int[] _hourlyPartDefect;
        private int[] _hourlySolderDefect;
        private int[] _hourlyScrap;

        private bool _defectRateInitialized = false;
        private bool _defectCategoryInitialized = false;
        private bool _dailyTargetInitialized = false;
        private Dictionary<string, Label> _defectCategoryLegendLabels = new Dictionary<string, Label>();
        private Dictionary<string, Label> _dailyTargetLegendLabels = new Dictionary<string, Label>();

        private Label _lblDailyRate;
        private Label _lblDailyTarget;
        private Label _lblDailyActual;
        private Label _lblDefectRate;
        private Label _lblTotal;
        private Label _lblNormal;
        private Label _lblDefect;

        // "정상" / "불량" 레전드 라벨 캐시 (텍스트만 업데이트용)
        private Dictionary<string, Label> _defectRateLegendLabels = new Dictionary<string, Label>();

        public DashboardView()
        {
            InitializeComponent();
            this.DoubleBuffered = true; 
          
        }

        private void DashboardView_Load(object sender, EventArgs e)
        {


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

            ReloadHourlyDataFromDb();

            RedrawDashboardCharts();

            SetupDefectRateChart();
            SetupDefectCategoryCharts();
            SetupDailyTargetCharts();
            SetupBoxRateChart();
            SetupDefectTrendChart();
            SetupHourlyInspectionChart();

            _defectTrendToolTip.AutoPopDelay = 5000;
            _defectTrendToolTip.InitialDelay = 200;
            _defectTrendToolTip.ReshowDelay = 100;

            _hourlyChartToolTip.AutoPopDelay = 4000;
            _hourlyChartToolTip.InitialDelay = 200;
            _hourlyChartToolTip.ReshowDelay = 100;

            _refreshTimer = new System.Windows.Forms.Timer();
            _refreshTimer.Interval = 5_000; // 10초마다 새로고침 (원하면 3초/5초 등으로 변경)
            _refreshTimer.Tick += (s, ev) =>
            {
                ReloadHourlyDataFromDb();
                RedrawDashboardCharts();
            };
            _refreshTimer.Start();
        }


        // 0) 오늘 기준 시간별 데이터 다시 로드 (v3.0 스키마: 제품별 데이터 합산)
        private void ReloadHourlyDataFromDb()
        {
            // 0~23시 (길이 24) 초기화
            _hourlyNormal = new int[24];
            _hourlyPartDefect = new int[24];
            _hourlySolderDefect = new int[24];
            _hourlyScrap = new int[24];

            try
            {
                string connectionString =
                    "Server=100.80.24.53;Port=3306;Database=pcb_inspection;Uid=pcb_admin;Pwd=1234;CharSet=utf8mb4;";

                using (var db = new DatabaseManager.DatabaseManager(connectionString))
                {
                    DateTime today = DateTime.Today;
                    DateTime tomorrow = today.AddDays(1);

                    // v3.0: 제품별로 데이터가 분리되어 있으므로 전체 조회 (productCode = null)
                    List<HourlyStatistics> stats = db.GetHourlyStatistics(today, tomorrow, productCode: null);

                    // v3.0: 같은 시간대에 여러 제품 데이터가 있을 수 있으므로 합산
                    foreach (var item in stats)
                    {
                        int h = item.StatDatetime.Hour;   // 0~23

                        if (h < 0 || h > 23)
                            continue;

                        // 시간대별로 모든 제품 데이터를 합산
                        _hourlyNormal[h] += item.NormalCount;
                        _hourlyPartDefect[h] += item.ComponentDefectCount;  // missing_count
                        _hourlySolderDefect[h] += item.SolderDefectCount;  // position_error_count
                        _hourlyScrap[h] += item.DiscardCount;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ReloadHourlyDataFromDb 실패] {ex.Message}");
            }
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            if (_isRefreshing) return;   // 아직 이전 작업이 안 끝났으면 그냥 패스

            try
            {
                _isRefreshing = true;
                ReloadHourlyDataFromDb();
                RedrawDashboardCharts();
            }
            finally
            {
                _isRefreshing = false;
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_refreshTimer != null)
                {
                    _refreshTimer.Stop();
                    _refreshTimer.Tick -= RefreshTimer_Tick; // 이벤트 핸들러 분리
                    _refreshTimer.Dispose();
                    _refreshTimer = null;
                }
            }
            base.Dispose(disposing);
        }
        // 1) 현재 _hourlyXXX 기준으로 모든 대시보드 차트 다시 그리기
        private void RedrawDashboardCharts()
        {

            SetupDefectRateChart();
            SetupDefectCategoryCharts();
            SetupDailyTargetCharts();
            SetupBoxRateChart();
            SetupDefectTrendChart();
            SetupHourlyInspectionChart();

        }


        private void SetupDefectRateChart()
        {
            // 1) 집계 데이터 계산
            int totalNormal = _hourlyNormal.Sum();
            int totalPartDefect = _hourlyPartDefect.Sum();
            int totalSolderDefect = _hourlySolderDefect.Sum();
            int totalScrap = _hourlyScrap.Sum();

            int normalCount = totalNormal;
            int defectCount = totalPartDefect + totalSolderDefect + totalScrap;
            int totalCount = normalCount + defectCount;

            double defectRate = 0.0;
            if (totalCount > 0)
                defectRate = (defectCount * 100.0) / totalCount;

            var chart = DefectRateChart;

            // ─────────────────────────────────────────────
            // 2) 처음 한 번만 차트 / 레전드 레이아웃 구성
            // ─────────────────────────────────────────────
            if (!_defectRateInitialized)
            {
                // ▷ 차트 기본 레이아웃 세팅 (1회)
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
                chart.Series.Add(s);

                // ▷ FlowLayoutPanel 안의 기본 구조도 1회만 생성
                flowLegendRate.SuspendLayout();
                flowLegendRate.Controls.Clear();
                flowLegendRate.FlowDirection = FlowDirection.TopDown;
                flowLegendRate.WrapContents = false;
                flowLegendRate.AutoSize = false;
                flowLegendRate.BackColor = Color.Transparent;

                // ─ 큰 퍼센트 라벨
                var ratePanel = new Panel
                {
                    Height = 50,
                    Width = flowLegendRate.Width - 16,
                    BackColor = Color.Transparent,
                    Margin = new Padding(0, 0, 0, 8),
                };

                _lblDefectRate = new Label
                {
                    AutoSize = false,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("맑은 고딕", 18, FontStyle.Bold),
                    ForeColor = Color.FromArgb(244, 67, 54)  // 빨간색
                };

                ratePanel.Controls.Add(_lblDefectRate);
                flowLegendRate.Controls.Add(ratePanel);

                // ─ 전체 개수 라벨
                var totalPanel = new Panel
                {
                    Height = 24,
                    Width = flowLegendRate.Width - 16,
                    BackColor = Color.Transparent,
                    Margin = new Padding(0, 0, 0, 2),
                };

                _lblTotal = new Label
                {
                    AutoSize = false,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new Font("맑은 고딕", 9, FontStyle.Regular),
                    ForeColor = Color.Gainsboro
                };

                totalPanel.Controls.Add(_lblTotal);
                flowLegendRate.Controls.Add(totalPanel);

                // ─ 정상 개수 라벨
                var normalPanel = new Panel
                {
                    Height = 24,
                    Width = flowLegendRate.Width - 16,
                    BackColor = Color.Transparent,
                    Margin = new Padding(0, 0, 0, 2),
                };

                _lblNormal = new Label
                {
                    AutoSize = false,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new Font("맑은 고딕", 9, FontStyle.Regular),
                    ForeColor = Color.Gainsboro
                };

                normalPanel.Controls.Add(_lblNormal);
                flowLegendRate.Controls.Add(normalPanel);

                // ─ 불량 개수 라벨
                var defectPanel = new Panel
                {
                    Height = 24,
                    Width = flowLegendRate.Width - 16,
                    BackColor = Color.Transparent,
                    Margin = new Padding(0, 0, 0, 8),
                };

                _lblDefect = new Label
                {
                    AutoSize = false,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new Font("맑은 고딕", 9, FontStyle.Regular),
                    ForeColor = Color.Gainsboro
                };

                defectPanel.Controls.Add(_lblDefect);
                flowLegendRate.Controls.Add(defectPanel);

                // ─ "정상 / 불량" 색상 네모 + 텍스트 + 클릭 기능 있는 레전드 행 생성
                var sRef = chart.Series["전체 불량률"];  // 클릭 핸들러에서 사용할 참조

                var rateMeta = new (string name, Color color)[]
                {
            ("정상", Color.FromArgb(100, 181, 246)),
            ("불량", Color.FromArgb(244, 67, 54))
                };

                foreach (var item in rateMeta)
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
                        Text = $"{item.name}  0개",  // 실제 값은 아래에서 갱신
                        ForeColor = Color.Gainsboro
                    };

                    // 나중에 Text만 갱신할 수 있도록 캐싱
                    _defectRateLegendLabels[item.name] = lbl;

                    // 클릭 시 해당 도넛 조각 강조
                    string categoryName = item.name;
                    EventHandler clickHandler = (_, __) =>
                    {
                        // 모든 포인트 테두리 초기화
                        foreach (var p in sRef.Points)
                        {
                            p.BorderWidth = 0;
                        }

                        // 현재 카테고리 이름과 같은 포인트 찾아서 강조
                        var target = sRef.Points.FirstOrDefault(p => p.AxisLabel == categoryName);
                        if (target != null)
                        {
                            target.BorderColor = Color.White;
                            target.BorderWidth = 3;
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
                    flowLegendRate.Controls.Add(row);
                }

                flowLegendRate.ResumeLayout();

                _defectRateInitialized = true;
            }

            var series = chart.Series["전체 불량률"];
            series.Points.Clear();

            int idx = series.Points.AddXY("정상", normalCount);
            series.Points[idx].Color = Color.FromArgb(100, 181, 246);

            idx = series.Points.AddXY("불량", defectCount);
            series.Points[idx].Color = Color.FromArgb(244, 67, 54);

            // ▷ 상단 요약 라벨 텍스트 업데이트
            _lblDefectRate.Text = $"{defectRate:0.#}%";
            _lblTotal.Text = $"전체: {totalCount}개";
            _lblNormal.Text = $"정상: {normalCount}개";
            _lblDefect.Text = $"불량: {defectCount}개";

            // ▷ 레전드 행의 텍스트도 현재 값으로 갱신
            if (_defectRateLegendLabels.TryGetValue("정상", out var lblNormalLegend))
            {
                lblNormalLegend.Text = $"정상  {normalCount}개";
            }

            if (_defectRateLegendLabels.TryGetValue("불량", out var lblDefectLegend))
            {
                lblDefectLegend.Text = $"불량  {defectCount}개";
            }
        }


        private void SetupDefectCategoryCharts()
        {
            var chart = DefectCategoryChart;

            // 1) 합계 계산
            int totalPartDefect = _hourlyPartDefect.Sum();
            int totalSolderDefect = _hourlySolderDefect.Sum();
            int totalScrap = _hourlyScrap.Sum();

            var categories = new (string name, int value, Color color)[]
            {
        ("부품불량", totalPartDefect,   Color.FromArgb(255, 167, 38)),
        ("S/N불량", totalSolderDefect, Color.FromArgb(158, 158, 158)),
        ("폐기",     totalScrap,        Color.FromArgb(244, 67, 54))
            };

            // ─────────────────────────────────────────────
            // 2) 처음 1번만 차트 / 레전드 레이아웃 구성
            // ─────────────────────────────────────────────
            if (!_defectCategoryInitialized)
            {
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

                var s = new Series("불량 카테고리")
                {
                    ChartType = SeriesChartType.Doughnut
                };
                s["DoughnutRadius"] = "50";
                s["PieLabelStyle"] = "Disabled";
                s.IsValueShownAsLabel = false;
                chart.Series.Add(s);

                // ─ 커스텀 레전드(FlowLayoutPanel) 초기 세팅
                flowLegend.SuspendLayout();
                flowLegend.Controls.Clear();
                flowLegend.FlowDirection = FlowDirection.TopDown;
                flowLegend.WrapContents = false;
                flowLegend.AutoSize = false;
                flowLegend.BackColor = Color.Transparent;

                // series 참조 (클릭 핸들러에서 사용)
                var sRef = chart.Series["불량 카테고리"];

                // "부품불량 / S/N불량 / 폐기" 행 생성 (한 번만)
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
                        Text = $"{item.name}  0개",   // 실제 값은 아래에서 갱신
                        ForeColor = Color.Gainsboro
                    };

                    // 나중에 Text만 바꿔주려고 캐싱
                    _defectCategoryLegendLabels[item.name] = lbl;

                    // 클릭 시 해당 조각만 강조
                    string categoryName = item.name;
                    EventHandler clickHandler = (_, __) =>
                    {
                        foreach (var p in sRef.Points) { p.BorderWidth = 0; }

                        var target = sRef.Points.FirstOrDefault(p => p.AxisLabel == categoryName);
                        if (target != null)
                        {
                            target.BorderColor = Color.White;
                            target.BorderWidth = 3;
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
                    flowLegend.Controls.Add(row);
                }

                flowLegend.ResumeLayout();

                _defectCategoryInitialized = true;
            }

            // ─────────────────────────────────────────────
            // 3) 여기부터는 5초마다 "데이터만" 갱신
            // ─────────────────────────────────────────────

            // 도넛 데이터 갱신
            var series = chart.Series["불량 카테고리"];
            series.Points.Clear();

            foreach (var item in categories)
            {
                int idx = series.Points.AddXY(item.name, item.value);
                series.Points[idx].Color = item.color;
            }

            // 레전드 텍스트 갱신
            foreach (var item in categories)
            {
                if (_defectCategoryLegendLabels.TryGetValue(item.name, out var lbl))
                {
                    lbl.Text = $"{item.name}  {item.value}개";
                }
            }
        }


        private void SetupDailyTargetCharts()
        {
            // 1) 목표 및 실제 생산량 데이터
            int targetProduction = 1000;                  // 목표 생산량
            int actualProduction = _hourlyNormal.Sum();   // 실제 생산량

            // 남은 물량 계산 (음수 방지)
            int remaining = targetProduction - actualProduction;
            if (remaining < 0) remaining = 0;

            if (actualProduction > targetProduction)
                actualProduction = targetProduction;

            // 2) 차트용 데이터 (달성 vs 미달성)
            var categories = new (string name, int value, Color color)[]
            {
        ("달성",   actualProduction, Color.FromArgb(100, 181, 246)),
        ("미달성", remaining,        Color.FromArgb(66, 66, 66))
            };

            var chart = DailyTargetChart;

            // ─────────────────────────────────────────────
            // 3) 처음 1번만 차트 / 레전드 레이아웃 구성
            // ─────────────────────────────────────────────
            if (!_dailyTargetInitialized)
            {
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
                chart.Series.Add(s);

                // ─ FlowLayoutPanel(legendTarget) 초기 세팅
                flowLegendTarget.SuspendLayout();
                flowLegendTarget.Controls.Clear();
                flowLegendTarget.FlowDirection = FlowDirection.TopDown;
                flowLegendTarget.WrapContents = false;
                flowLegendTarget.AutoSize = false;
                flowLegendTarget.BackColor = Color.Transparent;

                // ▶ 달성률 큰 글씨
                var ratePanel = new Panel
                {
                    Height = 50,
                    Width = flowLegendTarget.Width - 16,
                    BackColor = Color.Transparent,
                    Margin = new Padding(0, 0, 0, 8),
                };

                _lblDailyRate = new Label
                {
                    AutoSize = false,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("맑은 고딕", 18, FontStyle.Bold),
                    ForeColor = Color.FromArgb(100, 181, 246)
                };

                ratePanel.Controls.Add(_lblDailyRate);
                flowLegendTarget.Controls.Add(ratePanel);

                // ▶ 목표 개수 라벨
                var targetPanel = new Panel
                {
                    Height = 24,
                    Width = flowLegendTarget.Width - 16,
                    BackColor = Color.Transparent,
                    Margin = new Padding(0, 0, 0, 2),
                };

                _lblDailyTarget = new Label
                {
                    AutoSize = false,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new Font("맑은 고딕", 9, FontStyle.Regular),
                    ForeColor = Color.Gainsboro
                };

                targetPanel.Controls.Add(_lblDailyTarget);
                flowLegendTarget.Controls.Add(targetPanel);

                // ▶ 실제 개수 라벨
                var actualPanel = new Panel
                {
                    Height = 24,
                    Width = flowLegendTarget.Width - 16,
                    BackColor = Color.Transparent,
                    Margin = new Padding(0, 0, 0, 8),
                };

                _lblDailyActual = new Label
                {
                    AutoSize = false,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new Font("맑은 고딕", 9, FontStyle.Regular),
                    ForeColor = Color.Gainsboro
                };

                actualPanel.Controls.Add(_lblDailyActual);
                flowLegendTarget.Controls.Add(actualPanel);

                // ▶ "달성 / 미달성" 색상 네모 + 텍스트 + 클릭 강조 레전드
                var sRef = chart.Series["목표 생산량"];

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
                        Text = $"{item.name}  0개",   // 실제 값은 아래에서 갱신
                        ForeColor = Color.Gainsboro
                    };

                    _dailyTargetLegendLabels[item.name] = lbl;

                    string categoryName = item.name;
                    EventHandler clickHandler = (_, __) =>
                    {
                        foreach (var p in sRef.Points) { p.BorderWidth = 0; }

                        var targetPoint = sRef.Points.FirstOrDefault(p => p.AxisLabel == categoryName);
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

                _dailyTargetInitialized = true;
            }

            // ─────────────────────────────────────────────
            // 4) 여기부터는 5초마다 "데이터만" 갱신
            // ─────────────────────────────────────────────

            // 도넛 데이터 갱신
            var series = chart.Series["목표 생산량"];
            series.Points.Clear();

            foreach (var item in categories)
            {
                int idx = series.Points.AddXY(item.name, item.value);
                series.Points[idx].Color = item.color;
            }

            // 달성률 계산 (0으로 나누기 방지)
            double achievementRate = targetProduction > 0
                ? (actualProduction * 100.0) / targetProduction
                : 0.0;

            // 상단 텍스트 갱신
            _lblDailyRate.Text = $"{achievementRate:0.#}%";
            _lblDailyTarget.Text = $"목표: {targetProduction}개";
            _lblDailyActual.Text = $"실제: {actualProduction}개";

            // 레전드 행 텍스트 갱신
            foreach (var item in categories)
            {
                if (_dailyTargetLegendLabels.TryGetValue(item.name, out var lbl))
                {
                    lbl.Text = $"{item.name}  {item.value}개";
                }
            }
        }


        private void SetupBoxRateChart()
        {
            // 1) 데이터: 위에서 아래로 "정상 → 부품불량 → 납땜불량"
            var boxData = new (string name, int current, int max, Color color)[]
            {
                ("정상",     2, 3, Color.FromArgb(100, 181, 246)),
                ("부품불량",  3, 3, Color.FromArgb(255, 167, 38)),
                ("S/N불량",  1, 3, Color.FromArgb(158, 158, 158)),
            };

            var chart = BoxRateChart;

            // 2) 완전 초기화 + 데이터바인딩 끊기
            chart.DataSource = null;
            chart.Series.Clear();
            chart.ChartAreas.Clear();
            chart.Legends.Clear();

            chart.BackColor = Color.FromArgb(40, 40, 40);
            chart.BorderlineWidth = 0;

            // 3) ChartArea + 축 설정
            var area = new ChartArea("Main");
            area.BackColor = Color.Transparent;

            // ─ 가로축(X) : 값 (0 ~ 3)
            area.AxisX.Minimum = 0;
            area.AxisX.Maximum = 4;
            area.AxisX.Interval = 1;                      // 0,1,2,3
            area.AxisX.MajorGrid.Enabled = true;
            area.AxisX.MajorGrid.LineColor = Color.FromArgb(70, 70, 70);
            area.AxisX.LabelStyle.ForeColor = Color.Gainsboro;
            area.AxisX.LabelStyle.Font = new Font("맑은 고딕", 9);

            // ─ 세로축(Y) : 카테고리 (정상 / 부품불량 / 납땜불량)
            area.AxisY.MajorGrid.Enabled = false;
            area.AxisY.LabelStyle.ForeColor = Color.Gainsboro;
            area.AxisY.LabelStyle.Font = new Font("맑은 고딕", 9);
            area.AxisY.Interval = 1;          // 한 칸씩
            area.AxisY.IsReversed = false;    // 0번째가 맨 위


            chart.ChartAreas.Add(area);

            // 4) Series 설정 (가로 막대)
            var series = new Series("BoxRate");
            series.ChartArea = "Main";
            series.ChartType = SeriesChartType.Bar;   // 가로 막대
            series["PointWidth"] = "0.6";
            series.IsValueShownAsLabel = true;
            series.LabelForeColor = Color.Gainsboro;
            series.Font = new Font("맑은 고딕", 8, FontStyle.Bold);

            // ⚠ 자동 인덱싱 / 자동 X값 사용 안 함
            series.IsXValueIndexed = true;         // 기본값대로 두는 게 안전
            series.XValueType = ChartValueType.String;
            series.YValueType = ChartValueType.Int32;

            // 5) 포인트 추가 – DataPoint로 직접
            foreach (var item in boxData)
            {
                var p = new DataPoint();
                // X값은 안 쓰고, Y값만 사용 (막대 길이)
                p.SetValueY(item.current);          // 0~3
                p.AxisLabel = item.name;            // 세로축에 보일 텍스트
                p.Color = item.color;
                p.Label = $"{item.current}/{item.max}";  // 막대 위/옆 텍스트

                if (item.current >= item.max)
                {
                    p.BorderColor = Color.Yellow;
                    p.BorderWidth = 3;
                }

                series.Points.Add(p);
            }

            chart.Series.Add(series);
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

            // X축: 0~23시
            area.AxisX.Minimum = 0;
            area.AxisX.Maximum = 23;
            area.AxisX.Interval = 1;
            area.AxisX.MajorGrid.Enabled = false;
            area.AxisX.LabelStyle.ForeColor = Color.Gainsboro;
            area.AxisX.LabelStyle.Font = new Font("맑은 고딕", 9);
            area.AxisX.LabelStyle.Format = "0시";

            // ✅ Y축: 0.0 ~ 0.5 (0% ~ 50%), 값은 '비율'로, 표시만 %로
            area.AxisY.Minimum = 0.0;
            area.AxisY.Maximum = 0.5;      // 50%
            area.AxisY.Interval = 0.1;     // 10% 단위
            area.AxisY.MajorGrid.Enabled = true;
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(70, 70, 70);
            area.AxisY.LabelStyle.ForeColor = Color.Gainsboro;
            area.AxisY.LabelStyle.Font = new Font("맑은 고딕", 9);
            area.AxisY.LabelStyle.Format = "0%";   // ← 비율(0~1)을 %로 표시
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

            // 4) 불량률 계산 (✅ 이제 0~1 사이 '비율'로 저장)
            double[] defectRates = new double[24];

            for (int hour = 0; hour < 24; hour++)
            {
                int normal = _hourlyNormal[hour];
                int part = _hourlyPartDefect[hour];
                int solder = _hourlySolderDefect[hour];
                int scrap = _hourlyScrap[hour];

                int total = normal + part + solder + scrap;
                int defects = part + solder + scrap;

                // 🔥 여기에서 더 이상 *100 안 함! (그냥 비율)
                defectRates[hour] = total == 0 ? 0.0 : defects * 1.0 / total;
            }

            // 평균/관리선도 비율(0~1)로
            double avg = 0.20; // 20%
            double ucl = 0.30; // 30%
            double lcl = 0.10; // 10%

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
            area.AxisX.Minimum = -1;
            area.AxisX.Maximum = 24;
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
                Color = Color.FromArgb(255, 167, 38)
            };

            Series sSolderDefect = new Series("S/N불량")
            {
                ChartArea = "Main",
                ChartType = SeriesChartType.StackedColumn,
                Color = Color.FromArgb(158, 158, 158)
            };

            Series sScrap = new Series("폐기")
            {
                ChartArea = "Main",
                ChartType = SeriesChartType.StackedColumn,
                Color = Color.Red
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

        private void DefectTrendChart_MouseMove(object sender, MouseEventArgs e)
        {
            var chart = (Chart)sender;

            // 마우스 위치로 히트 테스트
            var hit = chart.HitTest(e.X, e.Y);

            // 기본 커서
            chart.Cursor = Cursors.Default;

            // 데이터 포인트 아니면 툴팁 숨김 + 상태 리셋
            if (hit.ChartElementType != ChartElementType.DataPoint ||
                hit.PointIndex < 0 ||
                hit.Series == null)
            {
                _defectTrendToolTip.Hide(chart);
                _lastDefectTrendKey = null;
                return;
            }

            var series = hit.Series;

            // 🔹 "실제 불량률" 라인에만 툴팁 표시
            if (series.Name != "실제 불량률")
            {
                _defectTrendToolTip.Hide(chart);
                _lastDefectTrendKey = null;
                return;
            }

            chart.Cursor = Cursors.Hand;

            var point = series.Points[hit.PointIndex];

            int hour = (int)point.XValue;        // 0 ~ 23시
            double rate = point.YValues[0];      // 0.0 ~ 0.5 (비율)

            // 같은 포인트에서 계속 움직이면 다시 툴팁 안 띄우게
            string key = $"actual_{hour}";
            if (_lastDefectTrendKey == key)
                return;

            _lastDefectTrendKey = key;

            // ✅ DashboardView에 이미 있는 시간별 배열 사용 (너가 DailyTargetChart에서 쓰던 그거)
            int normal = _hourlyNormal[hour];
            int part = _hourlyPartDefect[hour];
            int solder = _hourlySolderDefect[hour];
            int scrap = _hourlyScrap[hour];

            int total = normal + part + solder + scrap;
            int defects = part + solder + scrap;

            var sb = new StringBuilder();
            sb.AppendLine($"{hour:00}시");
            sb.AppendLine($"실제 불량률: {rate * 100:0.0}%");
            sb.AppendLine($"총 검사: {total}개");
            sb.AppendLine($"정상: {normal} / 부품: {part} / S/N: {solder} / 폐기: {scrap}");
            sb.AppendLine($"불량 합계: {defects}개");

            _defectTrendToolTip.Show(
                sb.ToString(),
                chart,
                e.Location.X + 15,
                e.Location.Y - 15
            );
        }

        private void HourlyInspectionChart_MouseMove(object sender, MouseEventArgs e)
        {
            var chart = (Chart)sender;

            // 마우스 위치로 히트 테스트
            var hit = chart.HitTest(e.X, e.Y);

            // 데이터 포인트 위에 있을 때만 처리
            if (hit.ChartElementType == ChartElementType.DataPoint &&
                hit.Series != null &&
                hit.PointIndex >= 0)
            {
                var series = hit.Series;
                int pointIndex = hit.PointIndex;

                // X값 = 시간(0~23), Y값 = 해당 시리즈 개수
                int hour = (int)series.Points[pointIndex].XValue;
                int value = (int)series.Points[pointIndex].YValues[0];

                // 🔹 같은 포인트에서 계속 움직일 때 깜빡임 방지
                string key = $"{series.Name}_{pointIndex}";
                if (_lastHourlyToolTipKey == key)
                    return;

                _lastHourlyToolTipKey = key;

                // ─ 시간대별 전체 합산 (스택형 막대 기준)
                int normal = 0, comp = 0, solder = 0, scrap = 0;

                if (chart.Series.IndexOf("정상") >= 0)
                    normal = (int)chart.Series["정상"].Points[pointIndex].YValues[0];
                if (chart.Series.IndexOf("부품불량") >= 0)
                    comp = (int)chart.Series["부품불량"].Points[pointIndex].YValues[0];
                if (chart.Series.IndexOf("S/N불량") >= 0)
                    solder = (int)chart.Series["S/N불량"].Points[pointIndex].YValues[0];
                if (chart.Series.IndexOf("폐기") >= 0)
                    scrap = (int)chart.Series["폐기"].Points[pointIndex].YValues[0];

                int total = normal + comp + solder + scrap;
                int defectSum = comp + solder + scrap;
                double defectRate = total > 0 ? defectSum * 100.0 / total : 0.0;

                // 🔹 툴팁 텍스트 예쁘게 구성
                var sb = new StringBuilder();
                sb.AppendLine($"{hour:00}시");
                sb.AppendLine($"[{series.Name}] {value}개");
                sb.AppendLine($"총 검사: {total}개");
                sb.AppendLine($"정상: {normal} / 부품: {comp} / S/N: {solder} / 폐기: {scrap}");
                sb.AppendLine($"불량률: {defectRate:0.0}% ({defectSum}개)");

                _hourlyChartToolTip.Show(
                    sb.ToString(),
                    chart,
                    e.Location.X + 15,
                    e.Location.Y - 15
                );
            }
            else
            {
                // 포인트 위가 아니면 툴팁 숨김 + 상태 리셋
                _hourlyChartToolTip.Hide(chart);
                _lastHourlyToolTipKey = null;
            }
        }
    }
}
