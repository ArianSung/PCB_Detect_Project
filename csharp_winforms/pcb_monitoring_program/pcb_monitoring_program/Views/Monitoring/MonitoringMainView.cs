using pcb_monitoring_program.Views.Statistics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pcb_monitoring_program.Views.Monitoring
{
    public partial class MonitoringMainView : UserControl
    {
        private MainPCBMonitoringView mainpcbMonitoringView;
        private MainBoxMonitoringView mainboxMonitoringView;
        private OHTMonitoringView ohtMonitoringView;
        private LineMonitoringView lineMonitoringView;
        public MonitoringMainView()
        {
            InitializeComponent();
            // 1) 자식 뷰 인스턴스 생성
            mainpcbMonitoringView = new MainPCBMonitoringView();
            mainboxMonitoringView = new MainBoxMonitoringView();
            ohtMonitoringView = new OHTMonitoringView();
            lineMonitoringView = new LineMonitoringView();

            // 2) 패널에 등록(숨긴 상태로)
            InitChildView(mainpcbMonitoringView);
            InitChildView(mainboxMonitoringView);
            InitChildView(ohtMonitoringView);
            InitChildView(lineMonitoringView);

            // 3) 처음 들어오면 "통계" 화면부터 보이게
            ShowView(mainpcbMonitoringView);

            // 4) 탭 버튼 스타일 (메인폼이랑 비슷하게)
            UiStyleHelper.MakeRoundedButton(btn_PCBMonitoringView, 24);
            UiStyleHelper.MakeRoundedButton(btn_BoxMonitoringView, 24);
            UiStyleHelper.MakeRoundedButton(btn_OHTMonitoringView, 24);
            UiStyleHelper.MakeRoundedButton(btn_LineMonitoringView, 24);

            UiStyleHelper.AttachDropShadow(btn_PCBMonitoringView, radius: 16, offset: 4);
            UiStyleHelper.AttachDropShadow(btn_BoxMonitoringView, radius: 16, offset: 4);
            UiStyleHelper.AttachDropShadow(btn_OHTMonitoringView, radius: 16, offset: 4);
            UiStyleHelper.AttachDropShadow(btn_LineMonitoringView, radius: 16, offset: 4);

            // 5) 버튼 기본 색/스타일(원하면)
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
            UiStyleHelper.HighlightButton(btn_PCBMonitoringView);
        }

        private void InitChildView(UserControl child)
        {
            child.Dock = DockStyle.Fill;
            child.Visible = false;        // 처음엔 안 보이게
            MonitoringPanel.Controls.Add(child);
        }
        private void ShowView(UserControl view)
        {
            foreach (Control c in MonitoringPanel.Controls)
                c.Visible = false;        // 다 숨기고

            view.Visible = true;          // 이 놈만 보여주기
            view.BringToFront();
        }

        private void btn_PCBMonitoringView_Click(object sender, EventArgs e)
        {
            ShowView(mainpcbMonitoringView);
            UiStyleHelper.HighlightButton((Button)sender);
        }

        private void btn_BoxMonitoringView_Click(object sender, EventArgs e)
        {
            ShowView(mainboxMonitoringView);
            UiStyleHelper.HighlightButton((Button)sender);
        }

        private void btn_OHTMonitoringView_Click(object sender, EventArgs e)
        {
            ShowView(ohtMonitoringView);
            UiStyleHelper.HighlightButton((Button)sender);
        }

        private void btn_LineMonitoringView_Click(object sender, EventArgs e)
        {
            ShowView(lineMonitoringView);
            UiStyleHelper.HighlightButton((Button)sender);
        }
    }
}
