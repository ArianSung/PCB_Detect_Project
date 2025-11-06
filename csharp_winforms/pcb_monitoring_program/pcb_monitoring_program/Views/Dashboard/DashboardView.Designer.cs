namespace pcb_monitoring_program.Views.Dashboard
{
    partial class DashboardView
    {
        /// <summary> 
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 구성 요소 디자이너에서 생성한 코드

        /// <summary> 
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea3 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend3 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
            DefectRateChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            DefectCategoryChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            chart3 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            cardRate = new Panel();
            cardTarget = new Panel();
            cardCategory = new Panel();
            flowLegend = new FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)DefectRateChart).BeginInit();
            ((System.ComponentModel.ISupportInitialize)DefectCategoryChart).BeginInit();
            ((System.ComponentModel.ISupportInitialize)chart3).BeginInit();
            cardRate.SuspendLayout();
            cardTarget.SuspendLayout();
            cardCategory.SuspendLayout();
            SuspendLayout();
            // 
            // DefectRateChart
            // 
            DefectRateChart.BackColor = Color.Transparent;
            chartArea1.Name = "ChartArea1";
            DefectRateChart.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            DefectRateChart.Legends.Add(legend1);
            DefectRateChart.Location = new Point(3, 0);
            DefectRateChart.Name = "DefectRateChart";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Doughnut;
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            DefectRateChart.Series.Add(series1);
            DefectRateChart.Size = new Size(240, 240);
            DefectRateChart.TabIndex = 0;
            DefectRateChart.Text = "chart1";
            // 
            // DefectCategoryChart
            // 
            chartArea2.Name = "ChartArea1";
            DefectCategoryChart.ChartAreas.Add(chartArea2);
            legend2.Name = "Legend1";
            DefectCategoryChart.Legends.Add(legend2);
            DefectCategoryChart.Location = new Point(3, -3);
            DefectCategoryChart.Name = "DefectCategoryChart";
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Doughnut;
            series2.Legend = "Legend1";
            series2.Name = "Series1";
            DefectCategoryChart.Series.Add(series2);
            DefectCategoryChart.Size = new Size(240, 240);
            DefectCategoryChart.TabIndex = 1;
            DefectCategoryChart.Text = "chart2";
            // 
            // chart3
            // 
            chartArea3.Name = "ChartArea1";
            chart3.ChartAreas.Add(chartArea3);
            legend3.Name = "Legend1";
            chart3.Legends.Add(legend3);
            chart3.Location = new Point(3, -3);
            chart3.Name = "chart3";
            series3.ChartArea = "ChartArea1";
            series3.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Doughnut;
            series3.Legend = "Legend1";
            series3.Name = "Series1";
            chart3.Series.Add(series3);
            chart3.Size = new Size(240, 240);
            chart3.TabIndex = 2;
            chart3.Text = "chart3";
            // 
            // cardRate
            // 
            cardRate.Controls.Add(flowLegend);
            cardRate.Controls.Add(DefectRateChart);
            cardRate.Location = new Point(24, 29);
            cardRate.Name = "cardRate";
            cardRate.Size = new Size(360, 240);
            cardRate.TabIndex = 3;
            // 
            // cardTarget
            // 
            cardTarget.Controls.Add(chart3);
            cardTarget.Location = new Point(852, 29);
            cardTarget.Name = "cardTarget";
            cardTarget.Size = new Size(360, 240);
            cardTarget.TabIndex = 0;
            // 
            // cardCategory
            // 
            cardCategory.Controls.Add(DefectCategoryChart);
            cardCategory.Location = new Point(438, 29);
            cardCategory.Name = "cardCategory";
            cardCategory.Size = new Size(360, 240);
            cardCategory.TabIndex = 0;
            // 
            // flowLegend
            // 
            flowLegend.Location = new Point(249, 13);
            flowLegend.Name = "flowLegend";
            flowLegend.Size = new Size(108, 215);
            flowLegend.TabIndex = 4;
            // 
            // DashboardView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(64, 64, 64);
            Controls.Add(cardTarget);
            Controls.Add(cardCategory);
            Controls.Add(cardRate);
            Name = "DashboardView";
            Size = new Size(1600, 900);
            Load += DashboardView_Load;
            ((System.ComponentModel.ISupportInitialize)DefectRateChart).EndInit();
            ((System.ComponentModel.ISupportInitialize)DefectCategoryChart).EndInit();
            ((System.ComponentModel.ISupportInitialize)chart3).EndInit();
            cardRate.ResumeLayout(false);
            cardTarget.ResumeLayout(false);
            cardCategory.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart DefectRateChart;
        private System.Windows.Forms.DataVisualization.Charting.Chart DefectCategoryChart;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart3;
        private Panel cardRate;
        private Panel cardTarget;
        private Panel cardCategory;
        private FlowLayoutPanel flowLegend;
    }
}
