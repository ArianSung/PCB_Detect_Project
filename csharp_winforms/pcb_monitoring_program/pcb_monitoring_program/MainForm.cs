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
        private void HighlightButton(Button clickedButton) //버튼 하이라이트 처리 메서드
        {
            // 1. 모든 버튼을 다시 기본 색으로
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is Button btn)
                {
                    btn.BackColor = Color.Gray;
                    btn.ForeColor = Color.White;
                }
            }

            // 2. 클릭한 버튼만 반전 처리
            clickedButton.BackColor = Color.White;
            clickedButton.ForeColor = Color.Gray;
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


        private void MainForm_Load(object sender, EventArgs e)
        {
            labelTitle.Text = "Dashboard";
            MakeRoundedButton(btnDashboard);
            MakeRoundedButton(btnStatistics);
            MakeRoundedButton(btnMonitoring);
            MakeRoundedButton(btnUserManagement);
            MakeRoundedButton(btnSetting);
            timerClock.Start();

            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is Button btn)
                {
                    btn.BackColor = Color.Gray;
                    btn.ForeColor = Color.White;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;  // 테두리 제거
                    btn.Cursor = Cursors.Hand;          // 마우스 포인터
                }
            }
            HighlightButton(btnDashboard);
            ShowInPanel(_UcDashboard);
        }

        private void timerClock_Tick(object sender, EventArgs e)
        {
            labelTime.Text = DateTime.Now.ToString("yyyy년 MM월 dd일 HH : mm");
        }
    }
}
