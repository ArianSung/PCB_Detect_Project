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
    public partial class OHTMonitoringView : UserControl
    {
        public OHTMonitoringView()
        {
            InitializeComponent();
        }

        private void OHTMonitoringView_Load(object sender, EventArgs e)
        {
            UiStyleHelper.MakeRoundedPanel(cardOHTMonitoring, radius: 16, back: Color.FromArgb(44, 44, 44));

            UiStyleHelper.AddShadowRoundedPanel(cardOHTMonitoring, 16);
        }
    }
}
