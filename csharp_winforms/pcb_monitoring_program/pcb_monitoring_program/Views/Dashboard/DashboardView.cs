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

    }
}
