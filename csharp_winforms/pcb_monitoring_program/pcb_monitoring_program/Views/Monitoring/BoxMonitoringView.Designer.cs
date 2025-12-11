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
            label2 = new Label();
            pictureBox1 = new PictureBox();
            label1 = new Label();
            BoxRateChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            cardBoxRate = new Panel();
            cardBOXMonitoring.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)BoxRateChart).BeginInit();
            cardBoxRate.SuspendLayout();
            SuspendLayout();
            // 
            // cardBOXMonitoring
            // 
            cardBOXMonitoring.Controls.Add(label2);
            cardBOXMonitoring.Controls.Add(pictureBox1);
            cardBOXMonitoring.Location = new Point(3, 3);
            cardBOXMonitoring.Name = "cardBOXMonitoring";
            cardBOXMonitoring.Size = new Size(755, 675);
            cardBOXMonitoring.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("맑은 고딕", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label2.ForeColor = SystemColors.Window;
            label2.Location = new Point(-3, 0);
            label2.Name = "label2";
            label2.Size = new Size(248, 30);
            label2.TabIndex = 4;
            label2.Text = "LINE 1 BOX Monitoring";
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.FromArgb(128, 128, 255);
            pictureBox1.Image = Properties.Resources.박스사진;
            pictureBox1.Location = new Point(51, 43);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(640, 616);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("맑은 고딕", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label1.ForeColor = SystemColors.Window;
            label1.Location = new Point(3, 0);
            label1.Name = "label1";
            label1.Size = new Size(197, 30);
            label1.TabIndex = 6;
            label1.Text = "LINE 1 박스 적재율";
            // 
            // BoxRateChart
            // 
            BoxRateChart.BackColor = Color.Transparent;
            chartArea1.Name = "ChartArea1";
            BoxRateChart.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            BoxRateChart.Legends.Add(legend1);
            BoxRateChart.Location = new Point(0, 79);
            BoxRateChart.Name = "BoxRateChart";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Bar;
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            BoxRateChart.Series.Add(series1);
            BoxRateChart.Size = new Size(740, 290);
            BoxRateChart.TabIndex = 5;
            BoxRateChart.Text = "chart1";
            // 
            // cardBoxRate
            // 
            cardBoxRate.Controls.Add(label1);
            cardBoxRate.Controls.Add(BoxRateChart);
            cardBoxRate.Location = new Point(799, 3);
            cardBoxRate.Name = "cardBoxRate";
            cardBoxRate.Size = new Size(743, 372);
            cardBoxRate.TabIndex = 7;
            // 
            // BoxMonitoringView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(64, 64, 64);
            Controls.Add(cardBoxRate);
            Controls.Add(cardBOXMonitoring);
            Name = "BoxMonitoringView";
            Size = new Size(1600, 720);
            Load += BoxMonitoringView_Load;
            cardBOXMonitoring.ResumeLayout(false);
            cardBOXMonitoring.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)BoxRateChart).EndInit();
            cardBoxRate.ResumeLayout(false);
            cardBoxRate.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel cardBOXMonitoring;
        private Label label2;
        private PictureBox pictureBox1;
        private System.Windows.Forms.DataVisualization.Charting.Chart BoxRateChart;
        private Label label1;
        private Panel cardBoxRate;
    }
}
