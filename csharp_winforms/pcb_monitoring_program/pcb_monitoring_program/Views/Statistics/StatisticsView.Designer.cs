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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea4 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend4 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series4 = new System.Windows.Forms.DataVisualization.Charting.Series();
            cardMonthlyLine = new Panel();
            lblMonthlyLineTitle = new Label();
            MonthlyLineChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            cardMonthlyAccum = new Panel();
            lblMonthlyAccumTitle = new Label();
            panel2 = new Panel();
            panel1 = new Panel();
            MonthlyAccumChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            cardDefectPie = new Panel();
            lblDefectPieTitle = new Label();
            flowPie = new FlowLayoutPanel();
            DefectTypePieChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            btn_Excel = new Button();
            dtpMonth = new ComponentFactory.Krypton.Toolkit.KryptonDateTimePicker();
            cardMonthlyTarget = new Panel();
            lblMonthlyTargetTitle = new Label();
            flowLegendMonthlyTarget = new FlowLayoutPanel();
            MonthlyTargetChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            cardMonthlyLine.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)MonthlyLineChart).BeginInit();
            cardMonthlyAccum.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)MonthlyAccumChart).BeginInit();
            cardDefectPie.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)DefectTypePieChart).BeginInit();
            cardMonthlyTarget.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)MonthlyTargetChart).BeginInit();
            SuspendLayout();
            // 
            // cardMonthlyLine
            // 
            cardMonthlyLine.Controls.Add(lblMonthlyLineTitle);
            cardMonthlyLine.Controls.Add(MonthlyLineChart);
            cardMonthlyLine.Location = new Point(20, 105);
            cardMonthlyLine.Name = "cardMonthlyLine";
            cardMonthlyLine.Size = new Size(794, 600);
            cardMonthlyLine.TabIndex = 1;
            // 
            // lblMonthlyLineTitle
            // 
            lblMonthlyLineTitle.AutoSize = true;
            lblMonthlyLineTitle.Font = new Font("맑은 고딕", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblMonthlyLineTitle.ForeColor = SystemColors.Window;
            lblMonthlyLineTitle.Location = new Point(0, -9);
            lblMonthlyLineTitle.Name = "lblMonthlyLineTitle";
            lblMonthlyLineTitle.Size = new Size(134, 30);
            lblMonthlyLineTitle.TabIndex = 3;
            lblMonthlyLineTitle.Text = "2000년 00월";
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
            cardMonthlyAccum.Controls.Add(lblMonthlyAccumTitle);
            cardMonthlyAccum.Controls.Add(panel2);
            cardMonthlyAccum.Controls.Add(panel1);
            cardMonthlyAccum.Controls.Add(MonthlyAccumChart);
            cardMonthlyAccum.Location = new Point(833, 411);
            cardMonthlyAccum.Name = "cardMonthlyAccum";
            cardMonthlyAccum.Size = new Size(764, 294);
            cardMonthlyAccum.TabIndex = 2;
            // 
            // lblMonthlyAccumTitle
            // 
            lblMonthlyAccumTitle.AutoSize = true;
            lblMonthlyAccumTitle.Font = new Font("맑은 고딕", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblMonthlyAccumTitle.ForeColor = SystemColors.Window;
            lblMonthlyAccumTitle.Location = new Point(0, -9);
            lblMonthlyAccumTitle.Name = "lblMonthlyAccumTitle";
            lblMonthlyAccumTitle.Size = new Size(82, 30);
            lblMonthlyAccumTitle.TabIndex = 6;
            lblMonthlyAccumTitle.Text = "2000년";
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
            panel1.Location = new Point(113, 250);
            panel1.Name = "panel1";
            panel1.Size = new Size(44, 30);
            panel1.TabIndex = 4;
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
            MonthlyAccumChart.MouseMove += MonthlyAccumChart_MouseMove;
            // 
            // cardDefectPie
            // 
            cardDefectPie.Controls.Add(lblDefectPieTitle);
            cardDefectPie.Controls.Add(flowPie);
            cardDefectPie.Controls.Add(DefectTypePieChart);
            cardDefectPie.Location = new Point(1220, 105);
            cardDefectPie.Name = "cardDefectPie";
            cardDefectPie.Size = new Size(377, 294);
            cardDefectPie.TabIndex = 3;
            // 
            // lblDefectPieTitle
            // 
            lblDefectPieTitle.AutoSize = true;
            lblDefectPieTitle.Font = new Font("맑은 고딕", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblDefectPieTitle.ForeColor = SystemColors.Window;
            lblDefectPieTitle.Location = new Point(0, -9);
            lblDefectPieTitle.Name = "lblDefectPieTitle";
            lblDefectPieTitle.Size = new Size(134, 30);
            lblDefectPieTitle.TabIndex = 6;
            lblDefectPieTitle.Text = "2000년 00월";
            // 
            // flowPie
            // 
            flowPie.Location = new Point(220, 46);
            flowPie.Name = "flowPie";
            flowPie.Size = new Size(140, 230);
            flowPie.TabIndex = 5;
            // 
            // DefectTypePieChart
            // 
            DefectTypePieChart.BackColor = Color.Transparent;
            chartArea3.Name = "ChartArea1";
            DefectTypePieChart.ChartAreas.Add(chartArea3);
            legend3.Name = "Legend1";
            DefectTypePieChart.Legends.Add(legend3);
            DefectTypePieChart.Location = new Point(3, 27);
            DefectTypePieChart.Name = "DefectTypePieChart";
            series3.ChartArea = "ChartArea1";
            series3.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series3.Legend = "Legend1";
            series3.Name = "Series1";
            DefectTypePieChart.Series.Add(series3);
            DefectTypePieChart.Size = new Size(211, 267);
            DefectTypePieChart.TabIndex = 1;
            DefectTypePieChart.Text = "chart2";
            // 
            // btn_Excel
            // 
            btn_Excel.Font = new Font("Arial", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_Excel.Location = new Point(209, 46);
            btn_Excel.Name = "btn_Excel";
            btn_Excel.Size = new Size(179, 40);
            btn_Excel.TabIndex = 4;
            btn_Excel.Text = "엑셀 시트 내보내기";
            btn_Excel.UseVisualStyleBackColor = true;
            btn_Excel.Click += btn_Excel_Click;
            // 
            // dtpMonth
            // 
            dtpMonth.CustomFormat = "yyyy년 - MM월";
            dtpMonth.Format = DateTimePickerFormat.Custom;
            dtpMonth.Location = new Point(20, 55);
            dtpMonth.Name = "dtpMonth";
            dtpMonth.Size = new Size(158, 31);
            dtpMonth.StateCommon.Back.Color1 = Color.FromArgb(44, 44, 44);
            dtpMonth.StateCommon.Border.Color1 = Color.FromArgb(44, 44, 44);
            dtpMonth.StateCommon.Border.Color2 = Color.FromArgb(44, 44, 44);
            dtpMonth.StateCommon.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            dtpMonth.StateCommon.Content.Color1 = Color.White;
            dtpMonth.StateCommon.Content.Font = new Font("맑은 고딕", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            dtpMonth.TabIndex = 9;
            dtpMonth.ValueChanged += dtpMonth_ValueChanged;
            // 
            // cardMonthlyTarget
            // 
            cardMonthlyTarget.Controls.Add(lblMonthlyTargetTitle);
            cardMonthlyTarget.Controls.Add(flowLegendMonthlyTarget);
            cardMonthlyTarget.Controls.Add(MonthlyTargetChart);
            cardMonthlyTarget.Location = new Point(833, 105);
            cardMonthlyTarget.Name = "cardMonthlyTarget";
            cardMonthlyTarget.Size = new Size(377, 294);
            cardMonthlyTarget.TabIndex = 7;
            // 
            // lblMonthlyTargetTitle
            // 
            lblMonthlyTargetTitle.AutoSize = true;
            lblMonthlyTargetTitle.Font = new Font("맑은 고딕", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblMonthlyTargetTitle.ForeColor = SystemColors.Window;
            lblMonthlyTargetTitle.Location = new Point(0, -9);
            lblMonthlyTargetTitle.Name = "lblMonthlyTargetTitle";
            lblMonthlyTargetTitle.Size = new Size(134, 30);
            lblMonthlyTargetTitle.TabIndex = 6;
            lblMonthlyTargetTitle.Text = "2000년 00월";
            // 
            // flowLegendMonthlyTarget
            // 
            flowLegendMonthlyTarget.Location = new Point(220, 46);
            flowLegendMonthlyTarget.Name = "flowLegendMonthlyTarget";
            flowLegendMonthlyTarget.Size = new Size(142, 230);
            flowLegendMonthlyTarget.TabIndex = 5;
            // 
            // MonthlyTargetChart
            // 
            MonthlyTargetChart.BackColor = Color.Transparent;
            chartArea4.Name = "ChartArea1";
            MonthlyTargetChart.ChartAreas.Add(chartArea4);
            legend4.Name = "Legend1";
            MonthlyTargetChart.Legends.Add(legend4);
            MonthlyTargetChart.Location = new Point(3, 27);
            MonthlyTargetChart.Name = "MonthlyTargetChart";
            series4.ChartArea = "ChartArea1";
            series4.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series4.Legend = "Legend1";
            series4.Name = "Series1";
            MonthlyTargetChart.Series.Add(series4);
            MonthlyTargetChart.Size = new Size(211, 267);
            MonthlyTargetChart.TabIndex = 1;
            MonthlyTargetChart.Text = "chart2";
            // 
            // StatisticsView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(64, 64, 64);
            Controls.Add(cardMonthlyTarget);
            Controls.Add(dtpMonth);
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
            cardMonthlyTarget.ResumeLayout(false);
            cardMonthlyTarget.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)MonthlyTargetChart).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel cardMonthlyLine;
        private System.Windows.Forms.DataVisualization.Charting.Chart MonthlyLineChart;
        private Panel cardMonthlyAccum;
        private Panel cardDefectPie;
        private System.Windows.Forms.DataVisualization.Charting.Chart MonthlyAccumChart;
        private System.Windows.Forms.DataVisualization.Charting.Chart DefectTypePieChart;
        private FlowLayoutPanel flowPie;
        private Panel panel2;
        private Panel panel1;
        private Button btn_Excel;
        private ComponentFactory.Krypton.Toolkit.KryptonDateTimePicker dtpMonth;
        private Label lblMonthlyLineTitle;
        private Label lblMonthlyAccumTitle;
        private Label lblDefectPieTitle;
        private Panel cardMonthlyTarget;
        private Label lblMonthlyTargetTitle;
        private FlowLayoutPanel flowLegendMonthlyTarget;
        private System.Windows.Forms.DataVisualization.Charting.Chart MonthlyTargetChart;
    }
}
