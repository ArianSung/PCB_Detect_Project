using MySqlX.XDevAPI.Common;
using pcb_monitoring_program.Views.Dashboard;
using pcb_monitoring_program.Views.Monitoring;
using pcb_monitoring_program.Views.Settings;
using pcb_monitoring_program.Views.Statistics;
using pcb_monitoring_program.Views.UserManagement;
using pcb_monitoring_program;
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
        private MainStatisticsView _UcStatistics = new MainStatisticsView();
        private MonitoringMainView _UcMonitoring = new MonitoringMainView();
        private UserManagementView _UcUserManagement = new UserManagementView();
        private SettingView _UcSetting = new SettingView();

        private readonly string _currentUsername;
        private readonly string _currentUserRole;   // admin / operator / viewer

        public MainForm()
        {
            InitializeComponent();

            pnlStatus.Resize += pnlStatus_Resize;
            pnlStatus_Resize(pnlStatus, EventArgs.Empty);
        }

        // ⭐ 새로 추가된 생성자: 로그인 폼에서 호출될 예정
        public MainForm(string username, string role) : this() // 기존 기본 생성자(InitializeComponent)를 호출
        {
            _currentUsername = username;
            _currentUserRole = role;

            labelusername.Text = username;
            labeluserauthority.Text = $"[ {role} ]";
        }

        private void ApplyRolePermissions()
        {
            // null 대비 + 소문자로 통일
            string role = (_currentUserRole ?? "").ToLowerInvariant();

            bool isAdmin = role == "admin";
            bool isOperator = role == "operator";
            bool isViewer = role == "viewer" || string.IsNullOrEmpty(role);

            // ✅ 모두 공통으로 보여줄 버튼
            btnDashboard.Visible = true;
            btnStatistics.Visible = true;
            btnMonitoring.Visible = true;

            // ✅ 권한에 따라 달라지는 버튼
            btnUserManagement.Visible = isAdmin;               // Admin only
            btnSetting.Visible = isAdmin; // Admin 

            // 필요하면 Enabled도 같이 제어 가능
            // btnUserManagement.Enabled = isAdmin;
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

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            labelTitle.Text = "Dashboard";
            UiStyleHelper.HighlightButton((Button)sender);
            ShowInPanel(_UcDashboard);
        }

        private void btnStatistics_Click(object sender, EventArgs e)
        {
            labelTitle.Text = "Statistics";
            UiStyleHelper.HighlightButton((Button)sender);
            ShowInPanel(_UcStatistics);
        }

        private void btnMonitoring_Click(object sender, EventArgs e)
        {
            labelTitle.Text = "Monitoring";
            UiStyleHelper.HighlightButton((Button)sender);
            ShowInPanel(_UcMonitoring);
        }

        private void btnUserManagement_Click(object sender, EventArgs e)
        {
            labelTitle.Text = "User Management";
            UiStyleHelper.HighlightButton((Button)sender);
            ShowInPanel(_UcUserManagement);
        }

        private void btnSetting_Click(object sender, EventArgs e)
        {
            labelTitle.Text = "Setting";
            UiStyleHelper.HighlightButton((Button)sender);
            ShowInPanel(_UcSetting);
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            labelusername.Text = "이   름";
            labeluserauthority.Text = "[ 권   한 ]";
            //labelTitle.Text = "Login";

            // 로그인 폼으로 돌아가기
            LoginForm loginForm = new LoginForm();
            loginForm.Show();
            this.Hide();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            UiStyleHelper.MakeRoundedPanel(cardStatus, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardLogout, radius: 16, back: Color.FromArgb(44, 44, 44));

            UiStyleHelper.AddShadowRoundedPanel(cardLogout, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardStatus, 16);

            labelTitle.Text = "Dashboard";
            UiStyleHelper.MakeRoundedButton(btnDashboard);
            UiStyleHelper.MakeRoundedButton(btnStatistics);
            UiStyleHelper.MakeRoundedButton(btnMonitoring);
            UiStyleHelper.MakeRoundedButton(btnUserManagement);
            UiStyleHelper.MakeRoundedButton(btnSetting);
            UiStyleHelper.MakeRoundedLogoutButton(btnLogout);
            UiStyleHelper.MakeRoundedButton(btnStart);
            UiStyleHelper.MakeRoundedButton(btnStop);

            // 우하단 드롭섀도우 적용(버튼이 놓인 같은 부모 컨테이너 기준)
            UiStyleHelper.AttachDropShadow(btnDashboard, radius: 20, offset: 6);
            UiStyleHelper.AttachDropShadow(btnStatistics, 20, 6);
            UiStyleHelper.AttachDropShadow(btnMonitoring, 20, 6);
            UiStyleHelper.AttachDropShadow(btnUserManagement, 20, 6);
            UiStyleHelper.AttachDropShadow(btnSetting, 20, 6);
            UiStyleHelper.AttachDropShadow(btnLogout, 20, 5);
            UiStyleHelper.AttachDropShadow(btnStart, 20, 5);
            UiStyleHelper.AttachDropShadow(btnStop, 20, 5);

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
            UiStyleHelper.HighlightButton(btnDashboard);
            ShowInPanel(_UcDashboard);
            btnStart.Tag = "nohighlight";
            btnStop.Tag = "nohighlight";

            // 색을 명시적으로 고정 (UseVisualStyleBackColor 끄기)
            btnStart.UseVisualStyleBackColor = false;
            btnStart.ForeColor = Color.FromArgb(128, 255, 128);

            btnStop.UseVisualStyleBackColor = false;
            btnStop.ForeColor = Color.FromArgb(255, 128, 128);
            ApplyRolePermissions();
        }

        private void timerClock_Tick(object sender, EventArgs e)
        {
            labelTime.Text = DateTime.Now.ToString("yyyy '/' MM '/' dd\n HH : mm");
        }
        private void pnlStatus_Resize(object sender, EventArgs e)
        {
            var p = (Panel)sender;

            using (var path = new GraphicsPath())
            {
                path.AddEllipse(0, 0, p.Width - 1, p.Height - 1);
                p.Region = new Region(path);
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            pnlStatus.BackColor = Color.LimeGreen;  // 초록
            lblStatus.Text = "가동중";
            lblStatus.ForeColor = Color.White;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            pnlStatus.BackColor = Color.Red; // 빨강
            lblStatus.Text = "비상정지";
            lblStatus.ForeColor = Color.White;
        }
    }
}
