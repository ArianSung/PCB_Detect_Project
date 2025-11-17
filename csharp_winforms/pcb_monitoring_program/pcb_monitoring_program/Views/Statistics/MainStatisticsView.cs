using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using pcb_monitoring_program;

namespace pcb_monitoring_program.Views.Statistics
{
    public partial class MainStatisticsView : UserControl
    {
        private InspectionHistoryView inspectionHistoryView;
        private StatisticsView statisticsView;

        public MainStatisticsView()
        {
            InitializeComponent();

            // 1) 자식 뷰 인스턴스 생성
            inspectionHistoryView = new InspectionHistoryView();
            statisticsView = new StatisticsView();

            // 2) 패널에 등록(숨긴 상태로)
            InitChildView(inspectionHistoryView);
            InitChildView(statisticsView);

            // 3) 처음 들어오면 "통계" 화면부터 보이게
            ShowView(statisticsView);

            // 4) 탭 버튼 스타일 (메인폼이랑 비슷하게)
            UiStyleHelper.MakeRoundedButton(btn_InspectionHistoryView, 24);
            UiStyleHelper.MakeRoundedButton(btn_StatisticsView, 24);

            UiStyleHelper.AttachDropShadow(btn_InspectionHistoryView, radius: 16, offset: 4);
            UiStyleHelper.AttachDropShadow(btn_StatisticsView, radius: 16, offset: 4);

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

            // 6) 처음 들어왔을 때는 "통계" 버튼이 탭 선택된 상태로
            UiStyleHelper.HighlightButton(btn_StatisticsView);
        }
        private void InitChildView(UserControl child)
        {
            child.Dock = DockStyle.Fill;
            child.Visible = false;        // 처음엔 안 보이게
            StatisticsPanel.Controls.Add(child);
        }
        private void ShowView(UserControl view)
        {
            foreach (Control c in StatisticsPanel.Controls)
                c.Visible = false;        // 다 숨기고

            view.Visible = true;          // 이 놈만 보여주기
            view.BringToFront();
        }
        private void btn_StatisticsView_Click(object sender, EventArgs e)
        {
            ShowView(statisticsView);
            UiStyleHelper.HighlightButton((Button)sender);
        }

        private void btn_InspectionHistoryView_Click(object sender, EventArgs e)
        {
            ShowView(inspectionHistoryView);
            UiStyleHelper.HighlightButton((Button)sender);
        }
    }
}
