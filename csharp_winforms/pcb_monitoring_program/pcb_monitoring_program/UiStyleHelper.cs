using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace pcb_monitoring_program
{
    internal class UiStyleHelper
    {
        // ▽ 버튼 하이라이트 (탭 버튼 용)
        // container : 버튼들이 들어있는 Panel 또는 Form
        public static void HighlightButton(Button clickedButton)
        {
            // 컨테이너는 버튼이 들어있는 부모(Panel 등) 우선 사용,
            // 없으면 폼(FindForm) 사용
            Control? container = clickedButton.Parent ?? clickedButton.FindForm();
            if (container == null) return;

            HighlightButton(container, clickedButton);
        }

        public static void HighlightButton(Control container, Button clickedButton)
        {
            // 1. 컨테이너 안 모든 버튼 기본 색으로
            foreach (Control ctrl in container.Controls)
            {
                if (ctrl is Button btn)
                {
                    btn.BackColor = Color.FromArgb(64, 64, 64);
                    btn.ForeColor = Color.White;
                }
            }

            // 2. 클릭된 버튼만 반전
            clickedButton.BackColor = Color.White;
            clickedButton.ForeColor = Color.FromArgb(64, 64, 64);
        }

        // ▽ 둥근 사각형 GraphicsPath 만들기 (섀도우용)
        public static GraphicsPath GetRoundPath(Rectangle r, int radius)
        {
            int d = radius * 2;
            GraphicsPath path = new GraphicsPath();
            path.StartFigure();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        // ▽ 일반 둥근 버튼
        public static void MakeRoundedButton(Button btn, int radius = 40)
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                path.StartFigure();
                path.AddArc(0, 0, radius, radius, 180, 90);
                path.AddArc(btn.Width - radius, 0, radius, radius, 270, 90);
                path.AddArc(btn.Width - radius, btn.Height - radius, radius, radius, 0, 90);
                path.AddArc(0, btn.Height - radius, radius, radius, 90, 90);
                path.CloseFigure();
                btn.Region = new Region(path);
            }
        }

        // ▽ Logout 버튼 둥글게 (조금 더 작은 radius)
        public static void MakeRoundedLogoutButton(Button btn, int radius = 20)
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                path.StartFigure();
                path.AddArc(0, 0, radius, radius, 180, 90);
                path.AddArc(btn.Width - radius, 0, radius, radius, 270, 90);
                path.AddArc(btn.Width - radius, btn.Height - radius, radius, radius, 0, 90);
                path.AddArc(0, btn.Height - radius, radius, radius, 90, 90);
                path.CloseFigure();
                btn.Region = new Region(path);
            }
        }

        // ▽ 버튼 드롭 섀도우 붙이기
        public static void AttachDropShadow(Button btn, int radius = 16, int offset = 6)
        {
            var parent = btn.Parent;
            if (parent == null) return;

            // 중복 연결 방지
            parent.Paint -= Parent_PaintShadow;
            btn.Move -= Child_RefreshParent;
            btn.Resize -= Child_RefreshParent;

            parent.Paint += Parent_PaintShadow;
            btn.Move += Child_RefreshParent;
            btn.Resize += Child_RefreshParent;

            // 섀도우 정보 저장
            parent.Tag ??= new List<(Button b, int r, int off)>();
            var list = (List<(Button b, int r, int off)>)parent.Tag;
            if (!list.Any(t => ReferenceEquals(t.b, btn)))
                list.Add((btn, radius, offset));

            // 깜빡임 줄이기(더블버퍼링)
            parent.GetType().GetProperty("DoubleBuffered",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(parent, true, null);
        }

        public static void Child_RefreshParent(object? s, EventArgs e)
        {
            if (s is Control c && c.Parent != null)
                c.Parent.Invalidate();
        }

        public static void Parent_PaintShadow(object? s, PaintEventArgs e)
        {
            if (s is not Control parent || parent.Tag is not List<(Button b, int r, int off)> list)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            foreach (var (btn, radius, offset) in list.ToList())
            {
                if (btn.Parent != parent || !btn.Visible) continue;

                var shadowRect = new Rectangle(btn.Left + offset, btn.Top + offset, btn.Width, btn.Height);

                var alphas = new[] { 60, 35, 20 };
                var grows = new[] { 0, 2, 4 };

                for (int i = 0; i < alphas.Length; i++)
                {
                    var grow = grows[i];
                    var rect = Rectangle.Inflate(shadowRect, grow, grow);
                    using var path = GetRoundPath(rect, radius + grow);
                    using var br = new SolidBrush(Color.FromArgb(alphas[i], 0, 0, 0));
                    e.Graphics.FillPath(br, path);
                }
            }
        }
        public static void MakeRoundedPanel(Panel p, int radius, Color back, int padding = 12)
        {
            p.BackColor = back;                 // 카드 배경색
            p.Padding = new Padding(padding); // 안쪽 여백

            // 리사이즈 될 때마다 모양 다시 적용
            p.Resize += (s, e) =>
            {
                using (var path = GetRoundPath(new Rectangle(0, 0, p.Width, p.Height), radius))
                    p.Region = new Region(path);
                p.Invalidate();
            };

            // 최초 1회 적용
            using (var path = GetRoundPath(new Rectangle(0, 0, p.Width, p.Height), radius))
                p.Region = new Region(path);
        }

        // ▽ 둥근 카드 뒤에 그림자 패널 하나 생성
        public static Panel AddShadowRoundedPanel(Panel target, int radius, int offset = 4, int alpha = 60)
        {
            if (target.Parent == null)
                throw new InvalidOperationException("target 패널이 아직 Parent에 추가되지 않았습니다.");

            var shadow = new Panel
            {
                Size = target.Size,
                Location = new Point(target.Left + offset, target.Top + offset),
                BackColor = Color.FromArgb(alpha, 0, 0, 0),
                Enabled = false,
                Parent = target.Parent
            };

            using (var path = GetRoundPath(new Rectangle(0, 0, shadow.Width, shadow.Height), radius))
                shadow.Region = new Region(path);

            shadow.SendToBack();
            target.BringToFront();

            // 위치/크기 동기화
            target.LocationChanged += (s, e) =>
            {
                shadow.Location = new Point(target.Left + offset, target.Top + offset);
            };

            target.Resize += (s, e) =>
            {
                shadow.Size = target.Size;
                using (var path = GetRoundPath(new Rectangle(0, 0, shadow.Width, shadow.Height), radius))
                    shadow.Region = new Region(path);
                shadow.Invalidate();
            };

            return shadow;
        }

        public static void ConfigureBoxRateChart(Chart chart, IEnumerable<(string name, int current, int max, Color color)> boxData)
        {
            if (chart == null) throw new ArgumentNullException(nameof(chart));
            if (boxData == null) throw new ArgumentNullException(nameof(boxData));

            chart.DataSource = null;
            chart.Series.Clear();
            chart.ChartAreas.Clear();
            chart.Legends.Clear();

            chart.BackColor = Color.FromArgb(40, 40, 40);
            chart.BorderlineWidth = 0;

            var area = new ChartArea("Main")
            {
                BackColor = Color.Transparent
            };
            area.AxisX.Minimum = 0;
            area.AxisX.Maximum = 4;
            area.AxisX.Interval = 1;
            area.AxisX.MajorGrid.Enabled = true;
            area.AxisX.MajorGrid.LineColor = Color.FromArgb(70, 70, 70);
            area.AxisX.LabelStyle.ForeColor = Color.Gainsboro;
            area.AxisX.LabelStyle.Font = new Font("맑은 고딕", 9);

            area.AxisY.MajorGrid.Enabled = false;
            area.AxisY.LabelStyle.ForeColor = Color.Gainsboro;
            area.AxisY.LabelStyle.Font = new Font("맑은 고딕", 9);
            area.AxisY.Interval = 1;
            area.AxisY.IsReversed = false;

            chart.ChartAreas.Add(area);

            var series = new Series("BoxRate")
            {
                ChartArea = "Main",
                ChartType = SeriesChartType.Bar,
                IsValueShownAsLabel = true,
                LabelForeColor = Color.Gainsboro,
                Font = new Font("맑은 고딕", 8, FontStyle.Bold)
            };
            series["PointWidth"] = "0.6";
            series.IsXValueIndexed = true;
            series.XValueType = ChartValueType.String;
            series.YValueType = ChartValueType.Int32;

            foreach (var item in boxData)
            {
                var point = new DataPoint();
                point.SetValueY(Math.Max(0, item.current));
                point.AxisLabel = item.name;
                point.Color = item.color;
                point.Label = $"{item.current}/{item.max}";

                if (item.max > 0 && item.current >= item.max)
                {
                    point.BorderColor = Color.Yellow;
                    point.BorderWidth = 3;
                }

                series.Points.Add(point);
            }

            chart.Series.Add(series);
        }
    }
}
