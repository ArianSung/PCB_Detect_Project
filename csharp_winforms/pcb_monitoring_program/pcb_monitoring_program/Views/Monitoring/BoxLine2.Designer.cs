namespace pcb_monitoring_program.Views.Monitoring
{
    partial class BoxLine2
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
            cardBoxRateLine2 = new Panel();
            label1 = new Label();
            BoxRateChartLine2 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            cardBOXLine2 = new Panel();
            label2 = new Label();
            pictureBox1 = new PictureBox();
            cardBoxRateLine2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)BoxRateChartLine2).BeginInit();
            cardBOXLine2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // cardBoxRateLine2
            // 
            cardBoxRateLine2.Controls.Add(label1);
            cardBoxRateLine2.Controls.Add(BoxRateChartLine2);
            cardBoxRateLine2.Location = new Point(799, 3);
            cardBoxRateLine2.Name = "cardBoxRateLine2";
            cardBoxRateLine2.Size = new Size(743, 372);
            cardBoxRateLine2.TabIndex = 9;
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
            label1.Text = "LINE 2 박스 적재율";
            // 
            // BoxRateChartLine2
            // 
            BoxRateChartLine2.BackColor = Color.Transparent;
            chartArea2.Name = "ChartArea1";
            BoxRateChartLine2.ChartAreas.Add(chartArea2);
            legend2.Name = "Legend1";
            BoxRateChartLine2.Legends.Add(legend2);
            BoxRateChartLine2.Location = new Point(0, 79);
            BoxRateChartLine2.Name = "BoxRateChartLine2";
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Bar;
            series2.Legend = "Legend1";
            series2.Name = "Series1";
            BoxRateChartLine2.Series.Add(series2);
            BoxRateChartLine2.Size = new Size(740, 290);
            BoxRateChartLine2.TabIndex = 5;
            BoxRateChartLine2.Text = "chart1";
            // 
            // cardBOXLine2
            // 
            cardBOXLine2.Controls.Add(label2);
            cardBOXLine2.Controls.Add(pictureBox1);
            cardBOXLine2.Location = new Point(3, 3);
            cardBOXLine2.Name = "cardBOXLine2";
            cardBOXLine2.Size = new Size(755, 675);
            cardBOXLine2.TabIndex = 8;
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
            label2.Text = "LINE 2 BOX Monitoring";
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
            // BoxLine2
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(64, 64, 64);
            Controls.Add(cardBoxRateLine2);
            Controls.Add(cardBOXLine2);
            Name = "BoxLine2";
            Size = new Size(1600, 700);
            Load += BoxLine2_Load;
            cardBoxRateLine2.ResumeLayout(false);
            cardBoxRateLine2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)BoxRateChartLine2).EndInit();
            cardBOXLine2.ResumeLayout(false);
            cardBOXLine2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel cardBoxRateLine2;
        private Label label1;
        private System.Windows.Forms.DataVisualization.Charting.Chart BoxRateChartLine2;
        private Panel cardBOXLine2;
        private Label label2;
        private PictureBox pictureBox1;
    }
}
