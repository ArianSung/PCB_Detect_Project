namespace pcb_monitoring_program.Views.Monitoring
{
    partial class LineMonitoringView
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
            cardLineMonitoring = new Panel();
            label2 = new Label();
            pictureBox1 = new PictureBox();
            cardLineMonitoring.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // cardLineMonitoring
            // 
            cardLineMonitoring.Controls.Add(label2);
            cardLineMonitoring.Controls.Add(pictureBox1);
            cardLineMonitoring.Location = new Point(0, 38);
            cardLineMonitoring.Name = "cardLineMonitoring";
            cardLineMonitoring.Size = new Size(1597, 715);
            cardLineMonitoring.TabIndex = 4;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("맑은 고딕", 12F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label2.ForeColor = SystemColors.Window;
            label2.Location = new Point(52, 40);
            label2.Name = "label2";
            label2.Size = new Size(136, 21);
            label2.TabIndex = 4;
            label2.Text = "LINE Monitoring";
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
            // LineMonitoringView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(64, 64, 64);
            Controls.Add(cardLineMonitoring);
            Name = "LineMonitoringView";
            Size = new Size(1600, 800);
            Load += LineMonitoringView_Load;
            cardLineMonitoring.ResumeLayout(false);
            cardLineMonitoring.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel cardLineMonitoring;
        private Label label2;
        private PictureBox pictureBox1;
    }
}
