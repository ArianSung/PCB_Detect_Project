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
    public partial class PCBLine3 : UserControl
    {
        public PCBLine3()
        {
            InitializeComponent();
            UiStyleHelper.MakeRoundedPanel(cardPCBFrontLine3, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardPCBBackLine3, radius: 16, back: Color.FromArgb(44, 44, 44));

            UiStyleHelper.AddShadowRoundedPanel(cardPCBFrontLine3, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardPCBBackLine3, 16);
        }
    }
}
