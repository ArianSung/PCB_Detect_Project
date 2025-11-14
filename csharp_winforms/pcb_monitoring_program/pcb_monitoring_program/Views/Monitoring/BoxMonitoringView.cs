using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace pcb_monitoring_program.Views.Monitoring
{
    public partial class BoxMonitoringView : UserControl
    {
        public BoxMonitoringView()
        {
            InitializeComponent();
        }
        private void BoxMonitoringView_Load(object sender, EventArgs e)
        {
            UiStyleHelper.MakeRoundedPanel(cardBOXMonitoring, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardBoxRate, radius: 16, back: Color.FromArgb(44, 44, 44));

            UiStyleHelper.AddShadowRoundedPanel(cardBOXMonitoring, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardBoxRate, 16);

            SetupBoxRateChart();
        }

        private void SetupBoxRateChart()
        {
            var boxData = new (string name, int current, int max, Color color)[]
            {
                ("정상",     2, 3, Color.FromArgb(100, 181, 246)),
                ("부품불량",  3, 3, Color.FromArgb(238,  99,  99)),
                ("납땜불량",  1, 3, Color.FromArgb(255, 170,   0)),
            };

            UiStyleHelper.ConfigureBoxRateChart(BoxRateChart, boxData);
        }
    }
}
