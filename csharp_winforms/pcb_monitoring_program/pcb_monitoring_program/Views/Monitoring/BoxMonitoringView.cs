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

            UiStyleHelper.AddShadowRoundedPanel(cardBOXMonitoring, 16);

            SetupBoxRateChart();
        }

        private void SetupBoxRateChart()
        {
            // 1) 데이터: 위에서 아래로 "정상 → 부품불량 → 납땜불량"
            var boxData = new (string name, int current, int max, Color color)[]
            {
                ("정상",     2, 3, Color.FromArgb(100, 181, 246)),
                ("부품불량",  3, 3, Color.FromArgb(238,  99,  99)),
                ("납땜불량",  1, 3, Color.FromArgb(255, 170,   0)),
            };

            var chart = BoxRateChart;

            // 2) 완전 초기화 + 데이터바인딩 끊기
            chart.DataSource = null;
            chart.Series.Clear();
            chart.ChartAreas.Clear();
            chart.Legends.Clear();

            chart.BackColor = Color.FromArgb(40, 40, 40);
            chart.BorderlineWidth = 0;

            // 3) ChartArea + 축 설정
            var area = new ChartArea("Main");
            area.BackColor = Color.Transparent;

            // ─ 가로축(X) : 값 (0 ~ 3)
            area.AxisX.Minimum = 0;
            area.AxisX.Maximum = 4;
            area.AxisX.Interval = 1;                      // 0,1,2,3
            area.AxisX.MajorGrid.Enabled = true;
            area.AxisX.MajorGrid.LineColor = Color.FromArgb(70, 70, 70);
            area.AxisX.LabelStyle.ForeColor = Color.Gainsboro;
            area.AxisX.LabelStyle.Font = new Font("맑은 고딕", 9);

            // ─ 세로축(Y) : 카테고리 (정상 / 부품불량 / 납땜불량)
            area.AxisY.MajorGrid.Enabled = false;
            area.AxisY.LabelStyle.ForeColor = Color.Gainsboro;
            area.AxisY.LabelStyle.Font = new Font("맑은 고딕", 9);
            area.AxisY.Interval = 1;          // 한 칸씩
            area.AxisY.IsReversed = false;    // 0번째가 맨 위


            chart.ChartAreas.Add(area);

            // 4) Series 설정 (가로 막대)
            var series = new Series("BoxRate");
            series.ChartArea = "Main";
            series.ChartType = SeriesChartType.Bar;   // 가로 막대
            series["PointWidth"] = "0.6";
            series.IsValueShownAsLabel = true;
            series.LabelForeColor = Color.Gainsboro;
            series.Font = new Font("맑은 고딕", 8, FontStyle.Bold);

            // ⚠ 자동 인덱싱 / 자동 X값 사용 안 함
            series.IsXValueIndexed = true;         // 기본값대로 두는 게 안전
            series.XValueType = ChartValueType.String;
            series.YValueType = ChartValueType.Int32;

            // 5) 포인트 추가 – DataPoint로 직접
            foreach (var item in boxData)
            {
                var p = new DataPoint();
                // X값은 안 쓰고, Y값만 사용 (막대 길이)
                p.SetValueY(item.current);          // 0~3
                p.AxisLabel = item.name;            // 세로축에 보일 텍스트
                p.Color = item.color;
                p.Label = $"{item.current}/{item.max}";  // 막대 위/옆 텍스트

                if (item.current >= item.max)
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
