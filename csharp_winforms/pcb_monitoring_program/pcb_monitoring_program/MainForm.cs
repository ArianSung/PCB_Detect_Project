using pcb_monitoring_program.Views.Dashboard;
using pcb_monitoring_program.Views.Monitoring;
using pcb_monitoring_program.Views.Settings;
using pcb_monitoring_program.Views.Statistics;
using pcb_monitoring_program.Views.UserManagement;
using System;
using System.Drawing;
using System.Windows.Forms;


namespace pcb_monitoring_program
{
    public partial class MainForm : Form
    {
        public event EventHandler? LogoutRequested;

        private readonly DashboardView _UcDashboard = new DashboardView();
        private readonly MainStatisticsView _UcStatistics = new MainStatisticsView();
        private readonly MonitoringMainView _UcMonitoring = new MonitoringMainView();
        private readonly UserManagementView _UcUserManagement = new UserManagementView();
        private readonly SettingView _UcSetting = new SettingView();
        private readonly string _username;
        private readonly string _role;

        public MainForm() : this("이   름", "권   한")
        {
        }

        public MainForm(string username, string role)
        {
            InitializeComponent();
            _username = username;
            _role = role;
        }

        private void ShowInPanel(UserControl page) //콘텐츠 패널 전환 메서드
        {
            if (!panelContent.Controls.Contains(page))
            {
                page.Dock = DockStyle.Fill;
                panelContent.Controls.Add(page);
            }

            foreach (Control ctrl in panelContent.Controls)
            {
                ctrl.Visible = false;
            }

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
            LogoutRequested?.Invoke(this, EventArgs.Empty);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            UiStyleHelper.MakeRoundedPanel(cardLogout, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.AddShadowRoundedPanel(cardLogout, 16);
            labelusername.Text = _username;
            labeluserauthority.Text = $"[ {_role} ]";

            labelTitle.Text = "Dashboard";
            UiStyleHelper.MakeRoundedButton(btnDashboard);
            UiStyleHelper.MakeRoundedButton(btnStatistics);
            UiStyleHelper.MakeRoundedButton(btnMonitoring);
            UiStyleHelper.MakeRoundedButton(btnUserManagement);
            UiStyleHelper.MakeRoundedButton(btnSetting);
            UiStyleHelper.MakeRoundedLogoutButton(btnLogout);

            // 우하단 드롭섀도우 적용(버튼이 놓인 같은 부모 컨테이너 기준)
            UiStyleHelper.AttachDropShadow(btnDashboard, radius: 20, offset: 6);
            UiStyleHelper.AttachDropShadow(btnStatistics, 20, 6);
            UiStyleHelper.AttachDropShadow(btnMonitoring, 20, 6);
            UiStyleHelper.AttachDropShadow(btnUserManagement, 20, 6);
            UiStyleHelper.AttachDropShadow(btnSetting, 20, 6);
            UiStyleHelper.AttachDropShadow(btnLogout, 20, 5);

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
        }

        private void timerClock_Tick(object sender, EventArgs e)
        {
            labelTime.Text = DateTime.Now.ToString("yyyy '/' MM '/' dd\n HH : mm");
        }
    }
}
