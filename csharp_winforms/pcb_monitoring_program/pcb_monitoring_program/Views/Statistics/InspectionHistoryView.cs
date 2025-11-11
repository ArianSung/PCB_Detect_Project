using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using pcb_monitoring_program;
using pcb_monitoring_program.Views.Statistics;

namespace pcb_monitoring_program.Views.Statistics
{
    public partial class InspectionHistoryView : UserControl
    {
        public event EventHandler OpenDetailsRequested;

        public InspectionHistoryView()
        {
            InitializeComponent();
        }

        private void ApplyButtonStyle(Control parent)
        {
            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl is Button btn)
                {
                    btn.BackColor = Color.FromArgb(64, 64, 64);
                    btn.ForeColor = Color.White;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                    btn.Cursor = Cursors.Hand;
                }
                else if (ctrl.HasChildren)
                {
                    ApplyButtonStyle(ctrl); // 내부 컨트롤(패널 등) 재귀 호출
                }
            }
        }

        private void InspectionHistoryView_Load(object sender, EventArgs e)
        {
            UiStyleHelper.MakeRoundedPanel(cardfilter, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardSearchresult, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardday, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardDefectType, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardCameraID, radius: 16, back: Color.FromArgb(44, 44, 44));

            UiStyleHelper.AddShadowRoundedPanel(cardfilter, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardSearchresult, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardday, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardDefectType, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardCameraID, 16);

            UiStyleHelper.MakeRoundedButton(btn_filterSearch, 24);
            UiStyleHelper.AttachDropShadow(btn_filterSearch, radius: 16, offset: 4);

            // 🔹 이 한 줄로 모든 버튼 스타일 적용
            ApplyButtonStyle(this);
        }

        private void cardfilter_Paint(object sender, PaintEventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenDetailsRequested?.Invoke(this, EventArgs.Empty);

            if (sender is Button btn)
                UiStyleHelper.HighlightButton(btn);

            // 🔹 InspectionHistoryForm 열기
            InspectionHistoryDetailForm form = new InspectionHistoryDetailForm();
            form.StartPosition = FormStartPosition.CenterParent; // 부모 기준 중앙 정렬
             form.Show();
        }
    }
}
