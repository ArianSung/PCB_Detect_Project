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
using pcb_monitoring_program.DatabaseManager;
using pcb_monitoring_program.DatabaseManager.Models;

namespace pcb_monitoring_program.Views.Monitoring
{
    public partial class BoxMonitoringView : UserControl
    {
        private readonly string _connectionString =
            "Server=100.80.24.53;Port=3306;Database=pcb_inspection;Uid=pcb_admin;Pwd=1234;CharSet=utf8mb4;";

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
            // 1) DB에서 box_status 읽어오기
            (string name, int current, int max, Color color)[] boxData;

            try
            {
                using (var db = new DatabaseManager.DatabaseManager(_connectionString))
                {
                    var list = db.GetAllBoxStatus();

                    var normal = list.FirstOrDefault(b => b.BoxId == "NORMAL")
                                 ?? new BoxStatus { BoxId = "NORMAL", Category = "normal", CurrentSlot = 0, MaxSlots = 3 };
                    var component = list.FirstOrDefault(b => b.BoxId == "COMPONENT_DEFECT")
                                 ?? new BoxStatus { BoxId = "COMPONENT_DEFECT", Category = "component_defect", CurrentSlot = 0, MaxSlots = 3 };
                    var solder = list.FirstOrDefault(b => b.BoxId == "SOLDER_DEFECT")
                                 ?? new BoxStatus { BoxId = "SOLDER_DEFECT", Category = "solder_defect", CurrentSlot = 0, MaxSlots = 3 };

                    // 🔹 current_slot 그대로 = 찬 슬롯 개수 (0 ~ max_slots)
                    int normalUsed = Math.Min(normal.CurrentSlot, normal.MaxSlots);
                    int componentUsed = Math.Min(component.CurrentSlot, component.MaxSlots);
                    int solderUsed = Math.Min(solder.CurrentSlot, solder.MaxSlots);

                    boxData = new (string name, int current, int max, Color color)[]
                    {
                ("정상",     normalUsed,    normal.MaxSlots,    Color.FromArgb(100, 181, 246)),
                ("부품불량", componentUsed, component.MaxSlots, Color.FromArgb(238,  99,  99)),
                ("납땜불량", solderUsed,    solder.MaxSlots,    Color.FromArgb(255, 170,   0)),
                    };
                }
            }
            catch (Exception ex)
            {
                boxData = new (string name, int current, int max, Color color)[]
                {
            ("정상",     0, 3, Color.FromArgb(100, 181, 246)),
            ("부품불량", 0, 3, Color.FromArgb(238,  99,  99)),
            ("납땜불량", 0, 3, Color.FromArgb(255, 170,   0)),
                };

                MessageBox.Show($"박스 상태를 불러오는 중 오류가 발생했습니다.\n{ex.Message}", "박스 상태 모니터링");
            }

            var chart = BoxRateChart;

            chart.DataSource = null;
            chart.Series.Clear();
            chart.ChartAreas.Clear();
            chart.Legends.Clear();

            chart.BackColor = Color.FromArgb(40, 40, 40);
            chart.BorderlineWidth = 0;

            var area = new ChartArea("Main");
            area.BackColor = Color.Transparent;

            // 🔹 AxisX = 카테고리 축 (세로) – 숫자 필요 없음
            area.AxisX.MajorGrid.Enabled = false;
            area.AxisX.LabelStyle.ForeColor = Color.Gainsboro;
            area.AxisX.LabelStyle.Font = new Font("맑은 고딕", 9);
            area.AxisX.Interval = 1;
            area.AxisX.IsReversed = false;

            // 🔹 AxisY = 값 축 (가로) – 여기서 0~3으로 고정
            area.AxisY.Minimum = 0;
            area.AxisY.Maximum = 3;     // ← 항상 0~3
            area.AxisY.Interval = 1;
            area.AxisY.MajorGrid.Enabled = true;
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(70, 70, 70);
            area.AxisY.LabelStyle.ForeColor = Color.Gainsboro;
            area.AxisY.LabelStyle.Font = new Font("맑은 고딕", 9);

            chart.ChartAreas.Add(area);

            var series = new Series("BoxRate");
            series.ChartArea = "Main";
            series.ChartType = SeriesChartType.Bar;   // 가로 막대
            series["PointWidth"] = "0.6";
            series.IsValueShownAsLabel = true;
            series.LabelForeColor = Color.Gainsboro;
            series.Font = new Font("맑은 고딕", 8, FontStyle.Bold);

            series.IsXValueIndexed = true;
            series.XValueType = ChartValueType.String;
            series.YValueType = ChartValueType.Int32;

            foreach (var item in boxData)
            {
                var p = new DataPoint();
                p.SetValueY(item.current);                 // 0~3 (찬 슬롯 개수)
                p.AxisLabel = item.name;                   // 세로축: 정상/부품불량/납땜불량
                p.Color = item.color;
                p.Label = $"{item.current}/{item.max}";    // 예: 1/3, 3/3

                if (item.current >= item.max && item.max > 0)
                {
                    p.BorderColor = Color.Yellow;
                    p.BorderWidth = 3;
                }

                series.Points.Add(p);
            }

            chart.Series.Add(series);
        }


    }
}
