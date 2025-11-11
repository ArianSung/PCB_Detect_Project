namespace pcb_monitoring_program.Views.Monitoring
{
    partial class BoxMonitoringView
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
            cardBOXMonitoring = new Panel();
            label1 = new Label();
            BoxRateChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            label2 = new Label();
            pictureBox1 = new PictureBox();
            cardBOXMonitoring.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)BoxRateChart).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // cardBOXMonitoring
            // 
            cardBOXMonitoring.Controls.Add(label1);
            cardBOXMonitoring.Controls.Add(BoxRateChart);
            cardBOXMonitoring.Controls.Add(label2);
            cardBOXMonitoring.Controls.Add(pictureBox1);
            cardBOXMonitoring.Location = new Point(0, 38);
            cardBOXMonitoring.Name = "cardBOXMonitoring";
            cardBOXMonitoring.Size = new Size(1597, 715);
            cardBOXMonitoring.TabIndex = 3;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("맑은 고딕", 12F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label1.ForeColor = SystemColors.Window;
            label1.Location = new Point(864, 40);
            label1.Name = "label1";
            label1.Size = new Size(96, 21);
            label1.TabIndex = 6;
            label1.Text = "박스 적재율";
            // 
            // BoxRateChart
            // 
            BoxRateChart.BackColor = Color.Transparent;
            chartArea1.Name = "ChartArea1";
            BoxRateChart.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            BoxRateChart.Legends.Add(legend1);
            BoxRateChart.Location = new Point(864, 73);
            BoxRateChart.Name = "BoxRateChart";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Bar;
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            BoxRateChart.Series.Add(series1);
            BoxRateChart.Size = new Size(683, 290);
            BoxRateChart.TabIndex = 5;
            BoxRateChart.Text = "chart1";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("맑은 고딕", 12F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label2.ForeColor = SystemColors.Window;
            label2.Location = new Point(52, 40);
            label2.Name = "label2";
            label2.Size = new Size(133, 21);
            label2.TabIndex = 4;
            label2.Text = "BOX Monitoring";
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.FromArgb(128, 128, 255);
            pictureBox1.Location = new Point(52, 73);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(640, 616);
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // BoxMonitoringView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(64, 64, 64);
            Controls.Add(cardBOXMonitoring);
            Name = "BoxMonitoringView";
            Size = new Size(1600, 800);
            Load += BoxMonitoringView_Load;
            cardBOXMonitoring.ResumeLayout(false);
            cardBOXMonitoring.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)BoxRateChart).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel cardBOXMonitoring;
        private Label label2;
        private PictureBox pictureBox1;
        private System.Windows.Forms.DataVisualization.Charting.Chart BoxRateChart;
        private Label label1;
    }
}
