using MySqlX.XDevAPI.Common;
using pcb_monitoring_program.Views.Dashboard;
using pcb_monitoring_program.Views.Monitoring;
using pcb_monitoring_program.Views.Settings;
using pcb_monitoring_program.Views.Statistics;
using pcb_monitoring_program.Views.UserManagement;
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


namespace pcb_monitoring_program
{
    public partial class MainForm : Form
    {
        private DashboardView _UcDashboard = new DashboardView();
        private StatisticsView _UcStatistics = new StatisticsView();
        private MonitoringView _UcMonitoring = new MonitoringView();
        private UserManagementView _UcUserManagement = new UserManagementView();
        private SettingView _UcSetting = new SettingView();
        public MainForm()
        {
            InitializeComponent();
        }
        private void ShowInPanel(UserControl page) //콘텐츠 패널 전환 메서드
        {
            // 1) 기존 컨트롤 모두 제거
            panelContent.Controls.Clear();

            // 2) 패널에 추가
            panelContent.Controls.Add(page);

            page.Visible = true;
            page.BringToFront();
        }

        private GraphicsPath GetRoundPath(Rectangle r, int radius) // 모서리 둥근 사각형 경로 생성 메서드
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


        private void HighlightButton(Button clickedButton) //버튼 하이라이트 처리 메서드
        {
            // 1. 모든 버튼을 다시 기본 색으로
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is Button btn)
                {
                    btn.BackColor = Color.FromArgb(64, 64, 64); ;
                    btn.ForeColor = Color.White;
                }
            }

            // 2. 클릭한 버튼만 반전 처리
            clickedButton.BackColor = Color.White;
            clickedButton.ForeColor = Color.FromArgb(64, 64, 64); ;
        }

        private void MakeRoundedButton(Button btn, int radius = 40) //버튼 모서리 둥글게
        {
            GraphicsPath path = new GraphicsPath();
            path.StartFigure();
            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(btn.Width - radius, 0, radius, radius, 270, 90);
            path.AddArc(btn.Width - radius, btn.Height - radius, radius, radius, 0, 90);
            path.AddArc(0, btn.Height - radius, radius, radius, 90, 90);
            path.CloseFigure();
            btn.Region = new Region(path);
        }
        private void MakeRoundedLogoutButton(Button btn, int radius = 20) //Logout버튼 모서리 둥글게
        {
            GraphicsPath path = new GraphicsPath();
            path.StartFigure();
            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(btn.Width - radius, 0, radius, radius, 270, 90);
            path.AddArc(btn.Width - radius, btn.Height - radius, radius, radius, 0, 90);
            path.AddArc(0, btn.Height - radius, radius, radius, 90, 90);
            path.CloseFigure();
            btn.Region = new Region(path);
        }

        // 🔸 부모(패널/폼)에, 버튼의 우하단으로 드롭섀도우를 그려준다
        private void AttachDropShadow(Button btn, int radius = 16, int offset = 6)
        {
            var parent = btn.Parent;

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

            // 섀도우 깜빡임 줄이기
            parent.GetType().GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(parent, true, null);
        }

        private void Child_RefreshParent(object? s, EventArgs e)
        {
            if (s is Control c && c.Parent != null) c.Parent.Invalidate();
        }

        private void Parent_PaintShadow(object? s, PaintEventArgs e)
        {
            if (s is not Control parent || parent.Tag is not List<(Button b, int r, int off)> list) return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            foreach (var (btn, radius, offset) in list.ToList())
            {
                if (btn.Parent != parent || !btn.Visible) continue;

                // 버튼 위치 기준으로 우하단(offset,offset) 위치에 섀도우
                var shadowRect = new Rectangle(btn.Left + offset, btn.Top + offset, btn.Width, btn.Height);

                // ▶ 간단한 가짜 블러: 바깥으로 3겹 정도 겹쳐 채우기
                //   (알파 낮추고 점점 크게)
                var alphas = new[] { 60, 35, 20 }; // 투명도(0~255)
                var grows = new[] { 0, 2, 4 };    // 겹칠 때 팽창량

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


        private void btnDashboard_Click(object sender, EventArgs e)
        {
            labelTitle.Text = "Dashboard";
            HighlightButton((Button)sender);
            ShowInPanel(_UcDashboard);
        }

        private void btnStatistics_Click(object sender, EventArgs e)
        {
            labelTitle.Text = "Statistics";
            HighlightButton((Button)sender);
            ShowInPanel(_UcStatistics);
        }

        private void btnMonitoring_Click(object sender, EventArgs e)
        {
            labelTitle.Text = "Monitoring";
            HighlightButton((Button)sender);
            ShowInPanel(_UcMonitoring);
        }

        private void btnUserManagement_Click(object sender, EventArgs e)
        {
            labelTitle.Text = "User Management";
            HighlightButton((Button)sender);
            ShowInPanel(_UcUserManagement);
        }

        private void btnSetting_Click(object sender, EventArgs e)
        {
            labelTitle.Text = "Setting";
            HighlightButton((Button)sender);
            ShowInPanel(_UcSetting);
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            labelusername.Text = "이   름";
            labeluserauthority.Text = "[ 권   한 ]";
            labelTitle.Text = "Login";

            // 로그인 폼으로 돌아가기
            LoginForm loginForm = new LoginForm();
            loginForm.Show();
            this.Hide();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            labelTitle.Text = "Dashboard";
            MakeRoundedButton(btnDashboard);
            MakeRoundedButton(btnStatistics);
            MakeRoundedButton(btnMonitoring);
            MakeRoundedButton(btnUserManagement);
            MakeRoundedButton(btnSetting);
            MakeRoundedLogoutButton(btnLogout);

            // 우하단 드롭섀도우 적용(버튼이 놓인 같은 부모 컨테이너 기준)
            AttachDropShadow(btnDashboard, radius: 20, offset: 6);
            AttachDropShadow(btnStatistics, 20, 6);
            AttachDropShadow(btnMonitoring, 20, 6);
            AttachDropShadow(btnUserManagement, 20, 6);
            AttachDropShadow(btnSetting, 20, 6);
            AttachDropShadow(btnLogout, 20, 5);

            timerClock.Start();

            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is Button btn)
                {
                    btn.BackColor = Color.Gray;
                    btn.ForeColor = Color.White;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;  // 테두리 제거
                    btn.Cursor = Cursors.Hand;                  // 마우스 포인터
                }
            }
            HighlightButton(btnDashboard);
            ShowInPanel(_UcDashboard);
        }

        private void timerClock_Tick(object sender, EventArgs e)
        {
            labelTime.Text = DateTime.Now.ToString("yyyy '/' MM '/' dd\n HH : mm");
        }
    }
}
