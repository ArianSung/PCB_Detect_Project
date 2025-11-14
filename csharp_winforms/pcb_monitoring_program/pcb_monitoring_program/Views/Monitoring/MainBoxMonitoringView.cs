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
    public partial class MainBoxMonitoringView : UserControl
    {
        private BoxMonitoringView boxMonitoringView;
        private BoxLine2 boxLine2;
        private BoxLine3 boxLine3;

        public MainBoxMonitoringView()
        {
            InitializeComponent();
            boxMonitoringView = new BoxMonitoringView();
            InitChildView(boxMonitoringView);
            ShowView(boxMonitoringView);

            boxMonitoringView = new BoxMonitoringView();
            boxLine2 = new BoxLine2();
            boxLine3 = new BoxLine3();

            InitChildView(boxMonitoringView);
            InitChildView(boxLine2);
            InitChildView(boxLine3);

            ShowView(boxMonitoringView);
            UiStyleHelper.MakeRoundedPanel(cardBoxLineChoice, radius: 16, back: Color.FromArgb(44, 44, 44));

            UiStyleHelper.AddShadowRoundedPanel(cardBoxLineChoice, 16);

            UiStyleHelper.MakeRoundedButton(btnBoxLine1, 24);
            UiStyleHelper.MakeRoundedButton(btnBoxLine2, 24);
            UiStyleHelper.MakeRoundedButton(btnBoxLine3, 24);

            UiStyleHelper.AttachDropShadow(btnBoxLine1, radius: 16, offset: 4);
            UiStyleHelper.AttachDropShadow(btnBoxLine2, radius: 16, offset: 4);
            UiStyleHelper.AttachDropShadow(btnBoxLine3, radius: 16, offset: 4);
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
            UiStyleHelper.HighlightButton(btnBoxLine1);
        }
        private void InitChildView(UserControl child)
        {
            child.Dock = DockStyle.Fill;
            child.Visible = false;        // 처음엔 안 보이게
            BoxMonitoringpanel.Controls.Add(child);
        }
        private void ShowView(UserControl view)
        {
            foreach (Control c in BoxMonitoringpanel.Controls)
                c.Visible = false;        // 다 숨기고

            view.Visible = true;          // 이 놈만 보여주기
            view.BringToFront();
        }

        private void btnBoxLine1_Click(object sender, EventArgs e)
        {
            UiStyleHelper.HighlightButton((Button)sender);
            ShowView(boxMonitoringView);
        }

        private void btnBoxLine2_Click(object sender, EventArgs e)
        {
            UiStyleHelper.HighlightButton((Button)sender);
            ShowView(boxLine2);
        }

        private void btnBoxLine3_Click(object sender, EventArgs e)
        {
            UiStyleHelper.HighlightButton((Button)sender);
            ShowView(boxLine3);
        }
    }
}
