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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea4 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend4 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series4 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea5 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend5 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series5 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea6 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend6 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series6 = new System.Windows.Forms.DataVisualization.Charting.Series();
            DefectRateChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            DefectCategoryChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            DailyTargetChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            cardRate = new Panel();
            flowLegendRate = new FlowLayoutPanel();
            label1 = new Label();
            flowLegend = new FlowLayoutPanel();
            cardTarget = new Panel();
            label3 = new Label();
            flowLegendTarget = new FlowLayoutPanel();
            cardCategory = new Panel();
            label2 = new Label();
            cardBoxRate = new Panel();
            label4 = new Label();
            BoxRateChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            cardTrend = new Panel();
            DefectTrendChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            label5 = new Label();
            cardFrontPCB = new Panel();
            pictureBox2 = new PictureBox();
            label6 = new Label();
            cardBackPCB = new Panel();
            pictureBox3 = new PictureBox();
            label7 = new Label();
            cardHourly = new Panel();
            panel2 = new Panel();
            panel1 = new Panel();
            HourlyInspectionChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            label8 = new Label();
            cardLog = new Panel();
            dataGridView1 = new DataGridView();
            label9 = new Label();
            cardTop = new Panel();
            label10 = new Label();
            ((System.ComponentModel.ISupportInitialize)DefectRateChart).BeginInit();
            ((System.ComponentModel.ISupportInitialize)DefectCategoryChart).BeginInit();
            ((System.ComponentModel.ISupportInitialize)DailyTargetChart).BeginInit();
            cardRate.SuspendLayout();
            cardTarget.SuspendLayout();
            cardCategory.SuspendLayout();
            cardBoxRate.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)BoxRateChart).BeginInit();
            cardTrend.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)DefectTrendChart).BeginInit();
            cardFrontPCB.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            cardBackPCB.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).BeginInit();
            cardHourly.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)HourlyInspectionChart).BeginInit();
            cardLog.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            cardTop.SuspendLayout();
            SuspendLayout();
            // 
            // DefectRateChart
            // 
            DefectRateChart.BackColor = Color.Transparent;
            chartArea1.Name = "ChartArea1";
            DefectRateChart.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            DefectRateChart.Legends.Add(legend1);
            DefectRateChart.Location = new Point(3, 40);
            DefectRateChart.Name = "DefectRateChart";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Doughnut;
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            DefectRateChart.Series.Add(series1);
            DefectRateChart.Size = new Size(200, 200);
            DefectRateChart.TabIndex = 0;
            DefectRateChart.Text = "chart1";
            // 
            // DefectCategoryChart
            // 
            DefectCategoryChart.BackColor = Color.Transparent;
            chartArea2.Name = "ChartArea1";
            DefectCategoryChart.ChartAreas.Add(chartArea2);
            legend2.Name = "Legend1";
            DefectCategoryChart.Legends.Add(legend2);
            DefectCategoryChart.Location = new Point(3, 40);
            DefectCategoryChart.Name = "DefectCategoryChart";
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Doughnut;
            series2.Legend = "Legend1";
            series2.Name = "Series1";
            DefectCategoryChart.Series.Add(series2);
            DefectCategoryChart.Size = new Size(200, 200);
            DefectCategoryChart.TabIndex = 1;
            DefectCategoryChart.Text = "chart2";
            // 
            // DailyTargetChart
            // 
            DailyTargetChart.BackColor = Color.Transparent;
            chartArea3.Name = "ChartArea1";
            DailyTargetChart.ChartAreas.Add(chartArea3);
            legend3.Name = "Legend1";
            DailyTargetChart.Legends.Add(legend3);
            DailyTargetChart.Location = new Point(3, 40);
            DailyTargetChart.Name = "DailyTargetChart";
            series3.ChartArea = "ChartArea1";
            series3.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Doughnut;
            series3.Legend = "Legend1";
            series3.Name = "Series1";
            DailyTargetChart.Series.Add(series3);
            DailyTargetChart.Size = new Size(200, 200);
            DailyTargetChart.TabIndex = 2;
            DailyTargetChart.Text = "chart3";
            // 
            // cardRate
            // 
            cardRate.Controls.Add(flowLegendRate);
            cardRate.Controls.Add(label1);
            cardRate.Controls.Add(DefectRateChart);
            cardRate.Location = new Point(7, 26);
            cardRate.Name = "cardRate";
            cardRate.Size = new Size(360, 243);
            cardRate.TabIndex = 3;
            // 
            // flowLegendRate
            // 
            flowLegendRate.Location = new Point(230, 12);
            flowLegendRate.Name = "flowLegendRate";
            flowLegendRate.Size = new Size(127, 215);
            flowLegendRate.TabIndex = 5;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("맑은 고딕", 12F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label1.ForeColor = SystemColors.Window;
            label1.Location = new Point(0, 0);
            label1.Name = "label1";
            label1.Size = new Size(96, 21);
            label1.TabIndex = 1;
            label1.Text = "전체 불량률";
            // 
            // flowLegend
            // 
            flowLegend.Location = new Point(230, 12);
            flowLegend.Name = "flowLegend";
            flowLegend.Size = new Size(127, 215);
            flowLegend.TabIndex = 4;
            // 
            // cardTarget
            // 
            cardTarget.Controls.Add(label3);
            cardTarget.Controls.Add(flowLegendTarget);
            cardTarget.Controls.Add(DailyTargetChart);
            cardTarget.Location = new Point(811, 29);
            cardTarget.Name = "cardTarget";
            cardTarget.Size = new Size(360, 240);
            cardTarget.TabIndex = 0;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("맑은 고딕", 12F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label3.ForeColor = SystemColors.Window;
            label3.Location = new Point(0, 0);
            label3.Name = "label3";
            label3.Size = new Size(172, 21);
            label3.TabIndex = 5;
            label3.Text = "일일 검사 목표 달성률";
            // 
            // flowLegendTarget
            // 
            flowLegendTarget.Location = new Point(230, 12);
            flowLegendTarget.Name = "flowLegendTarget";
            flowLegendTarget.Size = new Size(127, 215);
            flowLegendTarget.TabIndex = 5;
            // 
            // cardCategory
            // 
            cardCategory.Controls.Add(label2);
            cardCategory.Controls.Add(flowLegend);
            cardCategory.Controls.Add(DefectCategoryChart);
            cardCategory.Location = new Point(409, 29);
            cardCategory.Name = "cardCategory";
            cardCategory.Size = new Size(360, 240);
            cardCategory.TabIndex = 0;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("맑은 고딕", 12F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label2.ForeColor = SystemColors.Window;
            label2.Location = new Point(0, 0);
            label2.Name = "label2";
            label2.Size = new Size(150, 21);
            label2.TabIndex = 2;
            label2.Text = "불량 카테고리 분포";
            // 
            // cardBoxRate
            // 
            cardBoxRate.Controls.Add(label4);
            cardBoxRate.Controls.Add(BoxRateChart);
            cardBoxRate.Location = new Point(1213, 29);
            cardBoxRate.Name = "cardBoxRate";
            cardBoxRate.Size = new Size(360, 243);
            cardBoxRate.TabIndex = 4;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("맑은 고딕", 12F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label4.ForeColor = SystemColors.Window;
            label4.Location = new Point(1, 0);
            label4.Name = "label4";
            label4.Size = new Size(112, 21);
            label4.TabIndex = 6;
            label4.Text = "적재함 적재율";
            // 
            // BoxRateChart
            // 
            BoxRateChart.BackColor = Color.Transparent;
            chartArea4.Name = "ChartArea1";
            BoxRateChart.ChartAreas.Add(chartArea4);
            legend4.Name = "Legend1";
            BoxRateChart.Legends.Add(legend4);
            BoxRateChart.Location = new Point(3, 40);
            BoxRateChart.Name = "BoxRateChart";
            series4.ChartArea = "ChartArea1";
            series4.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Bar;
            series4.Legend = "Legend1";
            series4.Name = "Series1";
            BoxRateChart.Series.Add(series4);
            BoxRateChart.Size = new Size(337, 200);
            BoxRateChart.TabIndex = 0;
            BoxRateChart.Text = "chart1";
            // 
            // cardTrend
            // 
            cardTrend.Controls.Add(DefectTrendChart);
            cardTrend.Controls.Add(label5);
            cardTrend.Location = new Point(7, 299);
            cardTrend.Name = "cardTrend";
            cardTrend.Size = new Size(500, 250);
            cardTrend.TabIndex = 4;
            // 
            // DefectTrendChart
            // 
            DefectTrendChart.BackColor = Color.Transparent;
            chartArea5.Name = "ChartArea1";
            DefectTrendChart.ChartAreas.Add(chartArea5);
            legend5.Name = "Legend1";
            DefectTrendChart.Legends.Add(legend5);
            DefectTrendChart.Location = new Point(3, 38);
            DefectTrendChart.Name = "DefectTrendChart";
            series5.ChartArea = "ChartArea1";
            series5.Legend = "Legend1";
            series5.Name = "Series1";
            DefectTrendChart.Series.Add(series5);
            DefectTrendChart.Size = new Size(500, 212);
            DefectTrendChart.TabIndex = 2;
            DefectTrendChart.Text = "chart1";
            DefectTrendChart.MouseMove += DefectTrendChart_MouseMove;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("맑은 고딕", 12F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label5.ForeColor = SystemColors.Window;
            label5.Location = new Point(0, 0);
            label5.Name = "label5";
            label5.Size = new Size(166, 21);
            label5.TabIndex = 1;
            label5.Text = "불량률 상태 모니터링";
            // 
            // cardFrontPCB
            // 
            cardFrontPCB.Controls.Add(pictureBox2);
            cardFrontPCB.Controls.Add(label6);
            cardFrontPCB.Location = new Point(540, 299);
            cardFrontPCB.Name = "cardFrontPCB";
            cardFrontPCB.Size = new Size(500, 250);
            cardFrontPCB.TabIndex = 5;
            // 
            // pictureBox2
            // 
            pictureBox2.Image = Properties.Resources.PCBFrontEx;
            pictureBox2.Location = new Point(84, 7);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(400, 240);
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox2.TabIndex = 2;
            pictureBox2.TabStop = false;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("맑은 고딕", 12F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label6.ForeColor = SystemColors.Window;
            label6.Location = new Point(0, 0);
            label6.Name = "label6";
            label6.Size = new Size(78, 21);
            label6.TabIndex = 1;
            label6.Text = "PCB 앞면";
            // 
            // cardBackPCB
            // 
            cardBackPCB.Controls.Add(pictureBox3);
            cardBackPCB.Controls.Add(label7);
            cardBackPCB.Location = new Point(1073, 299);
            cardBackPCB.Name = "cardBackPCB";
            cardBackPCB.Size = new Size(500, 250);
            cardBackPCB.TabIndex = 6;
            // 
            // pictureBox3
            // 
            pictureBox3.Image = Properties.Resources.PCBBackEx;
            pictureBox3.Location = new Point(84, 7);
            pictureBox3.Name = "pictureBox3";
            pictureBox3.Size = new Size(400, 240);
            pictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox3.TabIndex = 3;
            pictureBox3.TabStop = false;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("맑은 고딕", 12F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label7.ForeColor = SystemColors.Window;
            label7.Location = new Point(0, 0);
            label7.Name = "label7";
            label7.Size = new Size(78, 21);
            label7.TabIndex = 1;
            label7.Text = "PCB 뒷면";
            // 
            // cardHourly
            // 
            cardHourly.Controls.Add(panel2);
            cardHourly.Controls.Add(panel1);
            cardHourly.Controls.Add(HourlyInspectionChart);
            cardHourly.Controls.Add(label8);
            cardHourly.Location = new Point(7, 579);
            cardHourly.Name = "cardHourly";
            cardHourly.Size = new Size(500, 250);
            cardHourly.TabIndex = 7;
            // 
            // panel2
            // 
            panel2.Location = new Point(459, 221);
            panel2.Name = "panel2";
            panel2.Size = new Size(25, 26);
            panel2.TabIndex = 6;
            // 
            // panel1
            // 
            panel1.Location = new Point(40, 202);
            panel1.Name = "panel1";
            panel1.Size = new Size(21, 20);
            panel1.TabIndex = 5;
            // 
            // HourlyInspectionChart
            // 
            HourlyInspectionChart.BackColor = Color.Transparent;
            chartArea6.Name = "ChartArea1";
            HourlyInspectionChart.ChartAreas.Add(chartArea6);
            legend6.Name = "Legend1";
            HourlyInspectionChart.Legends.Add(legend6);
            HourlyInspectionChart.Location = new Point(3, 36);
            HourlyInspectionChart.Name = "HourlyInspectionChart";
            series6.ChartArea = "ChartArea1";
            series6.Legend = "Legend1";
            series6.Name = "Series1";
            HourlyInspectionChart.Series.Add(series6);
            HourlyInspectionChart.Size = new Size(500, 212);
            HourlyInspectionChart.TabIndex = 2;
            HourlyInspectionChart.Text = "chart1";
            HourlyInspectionChart.MouseMove += HourlyInspectionChart_MouseMove;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("맑은 고딕", 12F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label8.ForeColor = SystemColors.Window;
            label8.Location = new Point(0, 0);
            label8.Name = "label8";
            label8.Size = new Size(150, 21);
            label8.TabIndex = 1;
            label8.Text = "시간대별 검사 추이";
            // 
            // cardLog
            // 
            cardLog.Controls.Add(dataGridView1);
            cardLog.Controls.Add(label9);
            cardLog.Location = new Point(1073, 579);
            cardLog.Name = "cardLog";
            cardLog.Size = new Size(500, 250);
            cardLog.TabIndex = 8;
            // 
            // dataGridView1
            // 
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(15, 36);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.Size = new Size(468, 202);
            dataGridView1.TabIndex = 2;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font("맑은 고딕", 12F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label9.ForeColor = SystemColors.Window;
            label9.Location = new Point(0, 0);
            label9.Name = "label9";
            label9.Size = new Size(172, 21);
            label9.TabIndex = 1;
            label9.Text = "실시간 검사 로그 기록";
            // 
            // cardTop
            // 
            cardTop.Controls.Add(label10);
            cardTop.Location = new Point(540, 579);
            cardTop.Name = "cardTop";
            cardTop.Size = new Size(500, 250);
            cardTop.TabIndex = 8;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Font = new Font("맑은 고딕", 12F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label10.ForeColor = SystemColors.Window;
            label10.Location = new Point(0, 0);
            label10.Name = "label10";
            label10.Size = new Size(170, 21);
            label10.TabIndex = 1;
            label10.Text = "상위 불량 원인 TOP 7";
            // 
            // DashboardView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(64, 64, 64);
            Controls.Add(cardTop);
            Controls.Add(cardLog);
            Controls.Add(cardHourly);
            Controls.Add(cardBackPCB);
            Controls.Add(cardFrontPCB);
            Controls.Add(cardTrend);
            Controls.Add(cardBoxRate);
            Controls.Add(cardTarget);
            Controls.Add(cardCategory);
            Controls.Add(cardRate);
            Name = "DashboardView";
            Size = new Size(1600, 900);
            Load += DashboardView_Load;
            ((System.ComponentModel.ISupportInitialize)DefectRateChart).EndInit();
            ((System.ComponentModel.ISupportInitialize)DefectCategoryChart).EndInit();
            ((System.ComponentModel.ISupportInitialize)DailyTargetChart).EndInit();
            cardRate.ResumeLayout(false);
            cardRate.PerformLayout();
            cardTarget.ResumeLayout(false);
            cardTarget.PerformLayout();
            cardCategory.ResumeLayout(false);
            cardCategory.PerformLayout();
            cardBoxRate.ResumeLayout(false);
            cardBoxRate.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)BoxRateChart).EndInit();
            cardTrend.ResumeLayout(false);
            cardTrend.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)DefectTrendChart).EndInit();
            cardFrontPCB.ResumeLayout(false);
            cardFrontPCB.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            cardBackPCB.ResumeLayout(false);
            cardBackPCB.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).EndInit();
            cardHourly.ResumeLayout(false);
            cardHourly.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)HourlyInspectionChart).EndInit();
            cardLog.ResumeLayout(false);
            cardLog.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            cardTop.ResumeLayout(false);
            cardTop.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart DefectRateChart;
        private System.Windows.Forms.DataVisualization.Charting.Chart DefectCategoryChart;
        private System.Windows.Forms.DataVisualization.Charting.Chart DailyTargetChart;
        private Panel cardRate;
        private Panel cardTarget;
        private Panel cardCategory;
        private FlowLayoutPanel flowLegend;
        private FlowLayoutPanel flowLegendTarget;
        private Label label1;
        private Label label3;
        private Label label2;
        private Panel cardBoxRate;
        private System.Windows.Forms.DataVisualization.Charting.Chart BoxRateChart;
        private Label label4;
        private Panel cardTrend;
        private Label label5;
        private System.Windows.Forms.DataVisualization.Charting.Chart DefectTrendChart;
        private Panel cardFrontPCB;
        private Label label6;
        private Panel cardBackPCB;
        private Label label7;
        private PictureBox pictureBox2;
        private PictureBox pictureBox3;
        private Panel cardHourly;
        private Label label8;
        private System.Windows.Forms.DataVisualization.Charting.Chart HourlyInspectionChart;
        private Panel cardLog;
        private DataGridView dataGridView1;
        private Label label9;
        private Panel cardTop;
        private Label label10;
        private Panel panel2;
        private Panel panel1;
        private FlowLayoutPanel flowLegendRate;
    }
}
