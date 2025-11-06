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

namespace pcb_monitoring_program.Views.Dashboard
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
        }

        private void DashboardView_Load(object sender, EventArgs e)
        {
            MakeRoundedPanel(cardRate, radius: 16, back: Color.FromArgb(44, 44, 44));
            MakeRoundedPanel(cardCategory, radius: 16, back: Color.FromArgb(44, 44, 44));
            MakeRoundedPanel(cardTarget, radius: 16, back: Color.FromArgb(44, 44, 44));
            AddShadowRoundedPanel(cardRate, 16);
            AddShadowRoundedPanel(cardCategory, 16);
            AddShadowRoundedPanel(cardTarget, 16);

            SetupDefectCharts();
        }

        private GraphicsPath BuildRoundPath(Rectangle rect, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.StartFigure();
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void MakeRoundedPanel(Panel p, int radius, Color back)
        {
            p.BackColor = back;                // 카드 배경색
            p.Padding = new Padding(12);       // 카드 안쪽 여백(차트와 테두리 간격)
            p.Resize += (s, e) =>
            {
                using (var path = BuildRoundPath(new Rectangle(0, 0, p.Width, p.Height), radius))
                    p.Region = new Region(path);
                p.Invalidate();
            };
            // 한 번 적용
            using (var path = BuildRoundPath(new Rectangle(0, 0, p.Width, p.Height), radius))
                p.Region = new Region(path);
        }

        private Panel AddShadowRoundedPanel(Panel target, int radius, int offset = 4, int alpha = 60)
        {
            var shadow = new Panel
            {
                Size = target.Size,
                Location = new Point(target.Left + offset, target.Top + offset),
                BackColor = Color.FromArgb(alpha, 0, 0, 0),
                Enabled = false,
                Parent = target.Parent
            };
            using (var path = BuildRoundPath(new Rectangle(0, 0, shadow.Width, shadow.Height), radius))
                shadow.Region = new Region(path);

            shadow.SendToBack();
            target.BringToFront();

            // 동기화
            target.LocationChanged += (s, e) =>
                shadow.Location = new Point(target.Left + offset, target.Top + offset);

            target.Resize += (s, e) =>
            {
                shadow.Size = target.Size;
                using (var path = BuildRoundPath(new Rectangle(0, 0, shadow.Width, shadow.Height), radius))
                    shadow.Region = new Region(path);
                shadow.Invalidate();
            };

            return shadow;
        }

        private void SetupDefectCharts()
        {
            // 1) 임의 데이터 (이름, 개수, 색)
            var categories = new (string name, int value, Color color)[]
            {
                ("부품불량", 50, Color.FromArgb(238, 99, 99)),
                ("납땜불량", 40, Color.FromArgb(255, 170, 0)),
                ("폐기",     30, Color.FromArgb(120, 160, 255)),
            };

            // 2) 차트 초기화
            var chart = DefectRateChart;
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
                           // ✅ 차트 조각 색상
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
                row.Cursor = Cursors.Hand;
                row.Click += (_, __) =>
                {
                    foreach (var p in s.Points) { p.BorderWidth = 0; }
                    // 같은 이름의 포인트 찾아 강조
                    var target = s.Points.FirstOrDefault(p => p.AxisLabel == item.name);
                    if (target != null) { target.BorderColor = Color.White; target.BorderWidth = 3; }
                };

                row.Controls.Add(swatch);
                row.Controls.Add(lbl);
                flowLegend.Controls.Add(row);
            }
            flowLegend.ResumeLayout();

        }
    }
}
