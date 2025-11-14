namespace pcb_monitoring_program.Views.Monitoring
{
    partial class MainBoxMonitoringView
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
            BoxMonitoringpanel = new Panel();
            cardBoxLineChoice = new Panel();
            btnBoxLine1 = new Button();
            btnBoxLine3 = new Button();
            btnBoxLine2 = new Button();
            cardBoxLineChoice.SuspendLayout();
            SuspendLayout();
            // 
            // BoxMonitoringpanel
            // 
            BoxMonitoringpanel.Location = new Point(0, 100);
            BoxMonitoringpanel.Name = "BoxMonitoringpanel";
            BoxMonitoringpanel.Size = new Size(1600, 700);
            BoxMonitoringpanel.TabIndex = 13;
            // 
            // cardBoxLineChoice
            // 
            cardBoxLineChoice.Controls.Add(btnBoxLine1);
            cardBoxLineChoice.Controls.Add(btnBoxLine3);
            cardBoxLineChoice.Controls.Add(btnBoxLine2);
            cardBoxLineChoice.Location = new Point(13, 12);
            cardBoxLineChoice.Name = "cardBoxLineChoice";
            cardBoxLineChoice.Size = new Size(358, 73);
            cardBoxLineChoice.TabIndex = 12;
            // 
            // btnBoxLine1
            // 
            btnBoxLine1.FlatAppearance.BorderSize = 0;
            btnBoxLine1.FlatStyle = FlatStyle.Flat;
            btnBoxLine1.Font = new Font("Arial", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnBoxLine1.Location = new Point(21, 17);
            btnBoxLine1.Name = "btnBoxLine1";
            btnBoxLine1.Size = new Size(82, 40);
            btnBoxLine1.TabIndex = 6;
            btnBoxLine1.Text = "LINE 1";
            btnBoxLine1.UseVisualStyleBackColor = true;
            btnBoxLine1.Click += btnBoxLine1_Click;
            // 
            // btnBoxLine3
            // 
            btnBoxLine3.FlatAppearance.BorderSize = 0;
            btnBoxLine3.FlatStyle = FlatStyle.Flat;
            btnBoxLine3.Font = new Font("Arial", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnBoxLine3.Location = new Point(259, 17);
            btnBoxLine3.Name = "btnBoxLine3";
            btnBoxLine3.Size = new Size(82, 40);
            btnBoxLine3.TabIndex = 8;
            btnBoxLine3.Text = "LINE 3";
            btnBoxLine3.UseVisualStyleBackColor = true;
            btnBoxLine3.Click += btnBoxLine3_Click;
            // 
            // btnBoxLine2
            // 
            btnBoxLine2.FlatAppearance.BorderSize = 0;
            btnBoxLine2.FlatStyle = FlatStyle.Flat;
            btnBoxLine2.Font = new Font("Arial", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnBoxLine2.Location = new Point(140, 17);
            btnBoxLine2.Name = "btnBoxLine2";
            btnBoxLine2.Size = new Size(82, 40);
            btnBoxLine2.TabIndex = 7;
            btnBoxLine2.Text = "LINE 2";
            btnBoxLine2.UseVisualStyleBackColor = true;
            btnBoxLine2.Click += btnBoxLine2_Click;
            // 
            // MainBoxMonitoringView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(64, 64, 64);
            Controls.Add(BoxMonitoringpanel);
            Controls.Add(cardBoxLineChoice);
            Name = "MainBoxMonitoringView";
            Size = new Size(1600, 800);
            cardBoxLineChoice.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel BoxMonitoringpanel;
        private Panel cardBoxLineChoice;
        private Button btnBoxLine1;
        private Button btnBoxLine3;
        private Button btnBoxLine2;
    }
}
