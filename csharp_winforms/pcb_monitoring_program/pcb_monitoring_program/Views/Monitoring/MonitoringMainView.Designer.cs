namespace pcb_monitoring_program.Views.Monitoring
{
    partial class MonitoringMainView
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
            btn_BoxMonitoringView = new Button();
            btn_PCBMonitoringView = new Button();
            btn_OHTMonitoringView = new Button();
            MonitoringPanel = new Panel();
            SuspendLayout();
            // 
            // btn_BoxMonitoringView
            // 
            btn_BoxMonitoringView.Font = new Font("Arial", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_BoxMonitoringView.Location = new Point(117, 51);
            btn_BoxMonitoringView.Name = "btn_BoxMonitoringView";
            btn_BoxMonitoringView.Size = new Size(75, 40);
            btn_BoxMonitoringView.TabIndex = 4;
            btn_BoxMonitoringView.Text = "BOX";
            btn_BoxMonitoringView.UseVisualStyleBackColor = true;
            btn_BoxMonitoringView.Click += btn_BoxMonitoringView_Click;
            // 
            // btn_PCBMonitoringView
            // 
            btn_PCBMonitoringView.Font = new Font("Arial", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_PCBMonitoringView.Location = new Point(25, 51);
            btn_PCBMonitoringView.Name = "btn_PCBMonitoringView";
            btn_PCBMonitoringView.Size = new Size(75, 40);
            btn_PCBMonitoringView.TabIndex = 3;
            btn_PCBMonitoringView.Text = "PCB";
            btn_PCBMonitoringView.UseVisualStyleBackColor = true;
            btn_PCBMonitoringView.Click += btn_PCBMonitoringView_Click;
            // 
            // btn_OHTMonitoringView
            // 
            btn_OHTMonitoringView.Font = new Font("Arial", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_OHTMonitoringView.Location = new Point(209, 51);
            btn_OHTMonitoringView.Name = "btn_OHTMonitoringView";
            btn_OHTMonitoringView.Size = new Size(75, 40);
            btn_OHTMonitoringView.TabIndex = 5;
            btn_OHTMonitoringView.Text = "OHT";
            btn_OHTMonitoringView.UseVisualStyleBackColor = true;
            btn_OHTMonitoringView.Click += btn_OHTMonitoringView_Click;
            // 
            // MonitoringPanel
            // 
            MonitoringPanel.Location = new Point(3, 97);
            MonitoringPanel.Name = "MonitoringPanel";
            MonitoringPanel.Size = new Size(1600, 800);
            MonitoringPanel.TabIndex = 6;
            // 
            // MonitoringMainView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(64, 64, 64);
            Controls.Add(MonitoringPanel);
            Controls.Add(btn_OHTMonitoringView);
            Controls.Add(btn_BoxMonitoringView);
            Controls.Add(btn_PCBMonitoringView);
            Name = "MonitoringMainView";
            Size = new Size(1600, 900);
            ResumeLayout(false);
        }

        #endregion

        private Button btn_BoxMonitoringView;
        private Button btn_PCBMonitoringView;
        private Button btn_OHTMonitoringView;
        private Panel MonitoringPanel;
    }
}
