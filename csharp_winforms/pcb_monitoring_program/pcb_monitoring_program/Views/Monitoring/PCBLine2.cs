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
    public partial class PCBLine2 : UserControl
    {
        public PCBLine2()
        {
            InitializeComponent();
            UiStyleHelper.MakeRoundedPanel(cardPCBFrontLine2, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardPCBBackLine2, radius: 16, back: Color.FromArgb(44, 44, 44));

            UiStyleHelper.AddShadowRoundedPanel(cardPCBFrontLine2, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardPCBBackLine2, 16);
        }
    }
}
