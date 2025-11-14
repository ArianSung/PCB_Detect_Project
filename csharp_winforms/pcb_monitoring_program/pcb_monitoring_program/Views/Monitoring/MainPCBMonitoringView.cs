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
    public partial class MainPCBMonitoringView : UserControl
    {
        private PCBMonitoringView pcbMonitoringView;
        private PCBLine2 pcbLine2;
        private PCBLine3 pcbLine3;
        public MainPCBMonitoringView()
        {
            InitializeComponent();

            pcbMonitoringView = new PCBMonitoringView();
            pcbLine2 = new PCBLine2();
            pcbLine3 = new PCBLine3();

            InitChildView(pcbMonitoringView);
            InitChildView(pcbLine2);
            InitChildView(pcbLine3);

            ShowView(pcbMonitoringView);

            UiStyleHelper.MakeRoundedPanel(cardPCBLineChoice, radius: 16, back: Color.FromArgb(44, 44, 44));

            UiStyleHelper.AddShadowRoundedPanel(cardPCBLineChoice, 16);

            UiStyleHelper.MakeRoundedButton(btnPCBLine1, 24);
            UiStyleHelper.MakeRoundedButton(btnPCBLine2, 24);
            UiStyleHelper.MakeRoundedButton(btnPCBLine3, 24);

            UiStyleHelper.AttachDropShadow(btnPCBLine1, radius: 16, offset: 4);
            UiStyleHelper.AttachDropShadow(btnPCBLine2, radius: 16, offset: 4);
            UiStyleHelper.AttachDropShadow(btnPCBLine3, radius: 16, offset: 4);

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
            UiStyleHelper.HighlightButton(btnPCBLine1);
        }
        private void InitChildView(UserControl child)
        {
            child.Dock = DockStyle.Fill;
            child.Visible = false;        // 처음엔 안 보이게
            PCBMonitoringpanel.Controls.Add(child);
        }
        private void ShowView(UserControl view)
        {
            foreach (Control c in PCBMonitoringpanel.Controls)
                c.Visible = false;        // 다 숨기고

            view.Visible = true;          // 이 놈만 보여주기
            view.BringToFront();
        }

        private void btnPCBLine1_Click(object sender, EventArgs e)
        {
            ShowView(pcbMonitoringView);
            UiStyleHelper.HighlightButton((Button)sender);
        }

        private void btnPCBLine2_Click(object sender, EventArgs e)
        {
            ShowView(pcbLine2);
            UiStyleHelper.HighlightButton((Button)sender);
        }

        private void btnPCBLine3_Click(object sender, EventArgs e)
        {
            ShowView(pcbLine3);
            UiStyleHelper.HighlightButton((Button)sender);
        }
    }
}
