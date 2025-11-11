namespace pcb_monitoring_program.Views.Monitoring
{
    partial class PCBMonitoringView
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
            pictureBox1 = new PictureBox();
            pictureBox2 = new PictureBox();
            cardPCBMonitoring = new Panel();
            label1 = new Label();
            label2 = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            cardPCBMonitoring.SuspendLayout();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.FromArgb(128, 128, 255);
            pictureBox1.Location = new Point(52, 73);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(640, 616);
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            pictureBox2.BackColor = Color.FromArgb(128, 128, 255);
            pictureBox2.Location = new Point(903, 73);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(640, 616);
            pictureBox2.TabIndex = 1;
            pictureBox2.TabStop = false;
            // 
            // cardPCBMonitoring
            // 
            cardPCBMonitoring.Controls.Add(label1);
            cardPCBMonitoring.Controls.Add(label2);
            cardPCBMonitoring.Controls.Add(pictureBox1);
            cardPCBMonitoring.Controls.Add(pictureBox2);
            cardPCBMonitoring.Location = new Point(0, 38);
            cardPCBMonitoring.Name = "cardPCBMonitoring";
            cardPCBMonitoring.Size = new Size(1597, 715);
            cardPCBMonitoring.TabIndex = 2;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("맑은 고딕", 12F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label1.ForeColor = SystemColors.Window;
            label1.Location = new Point(903, 40);
            label1.Name = "label1";
            label1.Size = new Size(82, 21);
            label1.TabIndex = 5;
            label1.Text = "PCB Back";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("맑은 고딕", 12F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label2.ForeColor = SystemColors.Window;
            label2.Location = new Point(52, 40);
            label2.Name = "label2";
            label2.Size = new Size(86, 21);
            label2.TabIndex = 4;
            label2.Text = "PCB Front";
            // 
            // PCBMonitoringView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(64, 64, 64);
            Controls.Add(cardPCBMonitoring);
            Name = "PCBMonitoringView";
            Size = new Size(1600, 800);
            Load += PCBMonitoringView_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            cardPCBMonitoring.ResumeLayout(false);
            cardPCBMonitoring.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox pictureBox1;
        private PictureBox pictureBox2;
        private Panel cardPCBMonitoring;
        private Label label1;
        private Label label2;
    }
}
