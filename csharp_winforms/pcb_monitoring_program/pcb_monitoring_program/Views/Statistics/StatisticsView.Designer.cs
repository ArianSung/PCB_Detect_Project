namespace pcb_monitoring_program.Views.Statistics
{
    partial class StatisticsView
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
            cardMonthlyLine = new Panel();
            label1 = new Label();
            MonthlyLineChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            cardMonthlyAccum = new Panel();
            panel2 = new Panel();
            panel1 = new Panel();
            label2 = new Label();
            MonthlyAccumChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            cardDefectPie = new Panel();
            flowPie = new FlowLayoutPanel();
            label3 = new Label();
            DefectTypePieChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            btn_Excel = new Button();
            cardMonthlyLine.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)MonthlyLineChart).BeginInit();
            cardMonthlyAccum.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)MonthlyAccumChart).BeginInit();
            cardDefectPie.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)DefectTypePieChart).BeginInit();
            SuspendLayout();
            // 
            // cardMonthlyLine
            // 
            cardMonthlyLine.Controls.Add(label1);
            cardMonthlyLine.Controls.Add(MonthlyLineChart);
            cardMonthlyLine.Location = new Point(20, 105);
            cardMonthlyLine.Name = "cardMonthlyLine";
            cardMonthlyLine.Size = new Size(794, 600);
            cardMonthlyLine.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("맑은 고딕", 12F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label1.ForeColor = SystemColors.Window;
            label1.Location = new Point(0, 0);
            label1.Name = "label1";
            label1.Size = new Size(140, 21);
            label1.TabIndex = 2;
            label1.Text = "날짜 범위 별 통계";
            // 
            // MonthlyLineChart
            // 
            MonthlyLineChart.BackColor = Color.Transparent;
            chartArea1.Name = "ChartArea1";
            MonthlyLineChart.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            MonthlyLineChart.Legends.Add(legend1);
            MonthlyLineChart.Location = new Point(19, 63);
            MonthlyLineChart.Name = "MonthlyLineChart";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            MonthlyLineChart.Series.Add(series1);
            MonthlyLineChart.Size = new Size(745, 481);
            MonthlyLineChart.TabIndex = 0;
            MonthlyLineChart.Text = "chart1";
            MonthlyLineChart.MouseClick += MonthlyLineChart_MouseClick;
            MonthlyLineChart.MouseMove += MonthlyLineChart_MouseMove;
            // 
            // cardMonthlyAccum
            // 
            cardMonthlyAccum.Controls.Add(panel2);
            cardMonthlyAccum.Controls.Add(panel1);
            cardMonthlyAccum.Controls.Add(label2);
            cardMonthlyAccum.Controls.Add(MonthlyAccumChart);
            cardMonthlyAccum.Location = new Point(851, 105);
            cardMonthlyAccum.Name = "cardMonthlyAccum";
            cardMonthlyAccum.Size = new Size(746, 294);
            cardMonthlyAccum.TabIndex = 2;
            // 
            // panel2
            // 
            panel2.Location = new Point(629, 250);
            panel2.Name = "panel2";
            panel2.Size = new Size(35, 30);
            panel2.TabIndex = 5;
            // 
            // panel1
            // 
            panel1.Location = new Point(102, 250);
            panel1.Name = "panel1";
            panel1.Size = new Size(44, 30);
            panel1.TabIndex = 4;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("맑은 고딕", 12F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label2.ForeColor = SystemColors.Window;
            label2.Location = new Point(0, 0);
            label2.Name = "label2";
            label2.Size = new Size(162, 21);
            label2.TabIndex = 3;
            label2.Text = "월 별 불량 누적 추이";
            // 
            // MonthlyAccumChart
            // 
            chartArea2.Name = "ChartArea1";
            MonthlyAccumChart.ChartAreas.Add(chartArea2);
            legend2.Name = "Legend1";
            MonthlyAccumChart.Legends.Add(legend2);
            MonthlyAccumChart.Location = new Point(65, 38);
            MonthlyAccumChart.Name = "MonthlyAccumChart";
            series2.ChartArea = "ChartArea1";
            series2.Legend = "Legend1";
            series2.Name = "Series1";
            MonthlyAccumChart.Series.Add(series2);
            MonthlyAccumChart.Size = new Size(623, 242);
            MonthlyAccumChart.TabIndex = 0;
            MonthlyAccumChart.Text = "chart1";
            // 
            // cardDefectPie
            // 
            cardDefectPie.Controls.Add(flowPie);
            cardDefectPie.Controls.Add(label3);
            cardDefectPie.Controls.Add(DefectTypePieChart);
            cardDefectPie.Location = new Point(851, 411);
            cardDefectPie.Name = "cardDefectPie";
            cardDefectPie.Size = new Size(746, 294);
            cardDefectPie.TabIndex = 3;
            // 
            // flowPie
            // 
            flowPie.Location = new Point(585, 41);
            flowPie.Name = "flowPie";
            flowPie.Size = new Size(137, 230);
            flowPie.TabIndex = 5;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("맑은 고딕", 12F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label3.ForeColor = SystemColors.Window;
            label3.Location = new Point(0, 0);
            label3.Name = "label3";
            label3.Size = new Size(172, 21);
            label3.TabIndex = 4;
            label3.Text = "불량 유형별 파이 차트";
            // 
            // DefectTypePieChart
            // 
            DefectTypePieChart.BackColor = Color.Transparent;
            chartArea3.Name = "ChartArea1";
            DefectTypePieChart.ChartAreas.Add(chartArea3);
            legend3.Name = "Legend1";
            DefectTypePieChart.Legends.Add(legend3);
            DefectTypePieChart.Location = new Point(65, 27);
            DefectTypePieChart.Name = "DefectTypePieChart";
            series3.ChartArea = "ChartArea1";
            series3.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series3.Legend = "Legend1";
            series3.Name = "Series1";
            DefectTypePieChart.Series.Add(series3);
            DefectTypePieChart.Size = new Size(476, 267);
            DefectTypePieChart.TabIndex = 1;
            DefectTypePieChart.Text = "chart2";
            // 
            // btn_Excel
            // 
            btn_Excel.Font = new Font("Arial", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_Excel.Location = new Point(1418, 33);
            btn_Excel.Name = "btn_Excel";
            btn_Excel.Size = new Size(179, 40);
            btn_Excel.TabIndex = 4;
            btn_Excel.Text = "엑셀 시트 내보내기";
            btn_Excel.UseVisualStyleBackColor = true;
            btn_Excel.Click += btn_Excel_Click;
            // 
            // StatisticsView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(64, 64, 64);
            Controls.Add(btn_Excel);
            Controls.Add(cardDefectPie);
            Controls.Add(cardMonthlyAccum);
            Controls.Add(cardMonthlyLine);
            Name = "StatisticsView";
            Size = new Size(1600, 800);
            Load += StatisticsView_Load;
            cardMonthlyLine.ResumeLayout(false);
            cardMonthlyLine.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)MonthlyLineChart).EndInit();
            cardMonthlyAccum.ResumeLayout(false);
            cardMonthlyAccum.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)MonthlyAccumChart).EndInit();
            cardDefectPie.ResumeLayout(false);
            cardDefectPie.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)DefectTypePieChart).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel cardMonthlyLine;
        private System.Windows.Forms.DataVisualization.Charting.Chart MonthlyLineChart;
        private Panel cardMonthlyAccum;
        private Panel cardDefectPie;
        private System.Windows.Forms.DataVisualization.Charting.Chart MonthlyAccumChart;
        private System.Windows.Forms.DataVisualization.Charting.Chart DefectTypePieChart;
        private Label label1;
        private Label label2;
        private Label label3;
        private FlowLayoutPanel flowPie;
        private Panel panel2;
        private Panel panel1;
        private Button btn_Excel;
    }
}
