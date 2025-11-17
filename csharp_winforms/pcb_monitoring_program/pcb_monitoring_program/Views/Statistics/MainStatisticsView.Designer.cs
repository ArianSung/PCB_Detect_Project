namespace pcb_monitoring_program.Views.Statistics
{
    partial class MainStatisticsView
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
            StatisticsPanel = new Panel();
            btn_StatisticsView = new Button();
            btn_InspectionHistoryView = new Button();
            SuspendLayout();
            // 
            // StatisticsPanel
            // 
            StatisticsPanel.Location = new Point(3, 97);
            StatisticsPanel.Name = "StatisticsPanel";
            StatisticsPanel.Size = new Size(1600, 800);
            StatisticsPanel.TabIndex = 0;
            // 
            // btn_StatisticsView
            // 
            btn_StatisticsView.Font = new Font("Arial", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_StatisticsView.Location = new Point(25, 51);
            btn_StatisticsView.Name = "btn_StatisticsView";
            btn_StatisticsView.Size = new Size(75, 40);
            btn_StatisticsView.TabIndex = 1;
            btn_StatisticsView.Text = "통계";
            btn_StatisticsView.UseVisualStyleBackColor = true;
            btn_StatisticsView.Click += btn_StatisticsView_Click;
            // 
            // btn_InspectionHistoryView
            // 
            btn_InspectionHistoryView.Font = new Font("Arial", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_InspectionHistoryView.Location = new Point(115, 51);
            btn_InspectionHistoryView.Name = "btn_InspectionHistoryView";
            btn_InspectionHistoryView.Size = new Size(127, 40);
            btn_InspectionHistoryView.TabIndex = 2;
            btn_InspectionHistoryView.Text = "검사 이력 조회";
            btn_InspectionHistoryView.UseVisualStyleBackColor = true;
            btn_InspectionHistoryView.Click += btn_InspectionHistoryView_Click;
            // 
            // MainStatisticsView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(64, 64, 64);
            Controls.Add(btn_InspectionHistoryView);
            Controls.Add(btn_StatisticsView);
            Controls.Add(StatisticsPanel);
            Name = "MainStatisticsView";
            Size = new Size(1600, 900);
            ResumeLayout(false);
        }

        #endregion

        private Panel StatisticsPanel;
        private Button btn_StatisticsView;
        private Button btn_InspectionHistoryView;
    }
}
