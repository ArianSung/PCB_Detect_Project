namespace pcb_monitoring_program.Views.Monitoring
{
    partial class BoxLine3
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            cardBoxRateLine3 = new Panel();
            label1 = new Label();
            BoxRateChartLine3 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            cardBOXLine3 = new Panel();
            label2 = new Label();
            pictureBox1 = new PictureBox();
            cardBoxRateLine3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)BoxRateChartLine3).BeginInit();
            cardBOXLine3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // cardBoxRateLine3
            // 
            cardBoxRateLine3.Controls.Add(label1);
            cardBoxRateLine3.Controls.Add(BoxRateChartLine3);
            cardBoxRateLine3.Location = new Point(799, 3);
            cardBoxRateLine3.Name = "cardBoxRateLine3";
            cardBoxRateLine3.Size = new Size(743, 372);
            cardBoxRateLine3.TabIndex = 9;
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
            label1.Text = "LINE 3 박스 적재율";
            // 
            // BoxRateChartLine3
            // 
            BoxRateChartLine3.BackColor = Color.Transparent;
            chartArea2.Name = "ChartArea1";
            BoxRateChartLine3.ChartAreas.Add(chartArea2);
            legend2.Name = "Legend1";
            BoxRateChartLine3.Legends.Add(legend2);
            BoxRateChartLine3.Location = new Point(0, 79);
            BoxRateChartLine3.Name = "BoxRateChartLine3";
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Bar;
            series2.Legend = "Legend1";
            series2.Name = "Series1";
            BoxRateChartLine3.Series.Add(series2);
            BoxRateChartLine3.Size = new Size(740, 290);
            BoxRateChartLine3.TabIndex = 5;
            BoxRateChartLine3.Text = "chart1";
            // 
            // cardBOXLine3
            // 
            cardBOXLine3.Controls.Add(label2);
            cardBOXLine3.Controls.Add(pictureBox1);
            cardBOXLine3.Location = new Point(3, 3);
            cardBOXLine3.Name = "cardBOXLine3";
            cardBOXLine3.Size = new Size(755, 675);
            cardBOXLine3.TabIndex = 8;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("맑은 고딕", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label2.ForeColor = SystemColors.Window;
            label2.Location = new Point(0, 0);
            label2.Name = "label2";
            label2.Size = new Size(248, 30);
            label2.TabIndex = 4;
            label2.Text = "LINE 3 BOX Monitoring";
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.FromArgb(128, 128, 255);
            pictureBox1.Location = new Point(51, 43);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(640, 616);
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // BoxLine3
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(64, 64, 64);
            Controls.Add(cardBoxRateLine3);
            Controls.Add(cardBOXLine3);
            Name = "BoxLine3";
            Size = new Size(1600, 700);
            Load += BoxLine3_Load;
            cardBoxRateLine3.ResumeLayout(false);
            cardBoxRateLine3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)BoxRateChartLine3).EndInit();
            cardBOXLine3.ResumeLayout(false);
            cardBOXLine3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel cardBoxRateLine3;
        private Label label1;
        private System.Windows.Forms.DataVisualization.Charting.Chart BoxRateChartLine3;
        private Panel cardBOXLine3;
        private Label label2;
        private PictureBox pictureBox1;
    }
}
