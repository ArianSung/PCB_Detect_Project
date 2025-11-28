namespace pcb_monitoring_program.Views.Monitoring
{
    partial class MainPCBMonitoringView
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
            cardPCBLineChoice = new Panel();
            btnPCBLine1 = new Button();
            btnPCBLine3 = new Button();
            btnPCBLine2 = new Button();
            PCBMonitoringpanel = new Panel();
            cardPCBLineChoice.SuspendLayout();
            SuspendLayout();
            // 
            // cardPCBLineChoice
            // 
            cardPCBLineChoice.Controls.Add(btnPCBLine1);
            cardPCBLineChoice.Controls.Add(btnPCBLine3);
            cardPCBLineChoice.Controls.Add(btnPCBLine2);
            cardPCBLineChoice.Location = new Point(13, 12);
            cardPCBLineChoice.Name = "cardPCBLineChoice";
            cardPCBLineChoice.Size = new Size(358, 61);
            cardPCBLineChoice.TabIndex = 10;
            // 
            // btnPCBLine1
            // 
            btnPCBLine1.FlatAppearance.BorderSize = 0;
            btnPCBLine1.FlatStyle = FlatStyle.Flat;
            btnPCBLine1.Font = new Font("Arial", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnPCBLine1.Location = new Point(13, 10);
            btnPCBLine1.Name = "btnPCBLine1";
            btnPCBLine1.Size = new Size(82, 40);
            btnPCBLine1.TabIndex = 6;
            btnPCBLine1.Text = "LINE 1";
            btnPCBLine1.UseVisualStyleBackColor = true;
            btnPCBLine1.Click += btnPCBLine1_Click;
            // 
            // btnPCBLine3
            // 
            btnPCBLine3.FlatAppearance.BorderSize = 0;
            btnPCBLine3.FlatStyle = FlatStyle.Flat;
            btnPCBLine3.Font = new Font("Arial", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnPCBLine3.Location = new Point(263, 10);
            btnPCBLine3.Name = "btnPCBLine3";
            btnPCBLine3.Size = new Size(82, 40);
            btnPCBLine3.TabIndex = 8;
            btnPCBLine3.Text = "LINE 3";
            btnPCBLine3.UseVisualStyleBackColor = true;
            btnPCBLine3.Click += btnPCBLine3_Click;
            // 
            // btnPCBLine2
            // 
            btnPCBLine2.FlatAppearance.BorderSize = 0;
            btnPCBLine2.FlatStyle = FlatStyle.Flat;
            btnPCBLine2.Font = new Font("Arial", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnPCBLine2.Location = new Point(138, 10);
            btnPCBLine2.Name = "btnPCBLine2";
            btnPCBLine2.Size = new Size(82, 40);
            btnPCBLine2.TabIndex = 7;
            btnPCBLine2.Text = "LINE 2";
            btnPCBLine2.UseVisualStyleBackColor = true;
            btnPCBLine2.Click += btnPCBLine2_Click;
            // 
            // PCBMonitoringpanel
            // 
            PCBMonitoringpanel.Location = new Point(0, 79);
            PCBMonitoringpanel.Name = "PCBMonitoringpanel";
            PCBMonitoringpanel.Size = new Size(1600, 720);
            PCBMonitoringpanel.TabIndex = 11;
            // 
            // MainPCBMonitoringView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(64, 64, 64);
            Controls.Add(PCBMonitoringpanel);
            Controls.Add(cardPCBLineChoice);
            Name = "MainPCBMonitoringView";
            Size = new Size(1600, 800);
            cardPCBLineChoice.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel cardPCBLineChoice;
        private Button btnPCBLine1;
        private Button btnPCBLine3;
        private Button btnPCBLine2;
        private Panel PCBMonitoringpanel;
    }
}
