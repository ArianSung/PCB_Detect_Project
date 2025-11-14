namespace pcb_monitoring_program.Views.Monitoring
{
    partial class PCBLine3
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
            cardPCBBackLine3 = new Panel();
            label1 = new Label();
            pictureBox2 = new PictureBox();
            cardPCBFrontLine3 = new Panel();
            label2 = new Label();
            pictureBox1 = new PictureBox();
            cardPCBBackLine3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            cardPCBFrontLine3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // cardPCBBackLine3
            // 
            cardPCBBackLine3.Controls.Add(label1);
            cardPCBBackLine3.Controls.Add(pictureBox2);
            cardPCBBackLine3.Location = new Point(847, 3);
            cardPCBBackLine3.Name = "cardPCBBackLine3";
            cardPCBBackLine3.Size = new Size(752, 682);
            cardPCBBackLine3.TabIndex = 7;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("맑은 고딕", 12F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label1.ForeColor = SystemColors.Window;
            label1.Location = new Point(3, 0);
            label1.Name = "label1";
            label1.Size = new Size(138, 21);
            label1.TabIndex = 4;
            label1.Text = "LINE 3 PCB Back";
            // 
            // pictureBox2
            // 
            pictureBox2.BackColor = Color.FromArgb(128, 128, 255);
            pictureBox2.Location = new Point(52, 40);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(640, 616);
            pictureBox2.TabIndex = 0;
            pictureBox2.TabStop = false;
            // 
            // cardPCBFrontLine3
            // 
            cardPCBFrontLine3.Controls.Add(label2);
            cardPCBFrontLine3.Controls.Add(pictureBox1);
            cardPCBFrontLine3.Location = new Point(3, 3);
            cardPCBFrontLine3.Name = "cardPCBFrontLine3";
            cardPCBFrontLine3.Size = new Size(752, 682);
            cardPCBFrontLine3.TabIndex = 6;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("맑은 고딕", 12F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label2.ForeColor = SystemColors.Window;
            label2.Location = new Point(3, 0);
            label2.Name = "label2";
            label2.Size = new Size(142, 21);
            label2.TabIndex = 4;
            label2.Text = "LINE 3 PCB Front";
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.FromArgb(128, 128, 255);
            pictureBox1.Location = new Point(52, 40);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(640, 616);
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // PCBLine3
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(64, 64, 64);
            Controls.Add(cardPCBBackLine3);
            Controls.Add(cardPCBFrontLine3);
            Name = "PCBLine3";
            Size = new Size(1600, 700);
            cardPCBBackLine3.ResumeLayout(false);
            cardPCBBackLine3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            cardPCBFrontLine3.ResumeLayout(false);
            cardPCBFrontLine3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel cardPCBBackLine3;
        private Label label1;
        private PictureBox pictureBox2;
        private Panel cardPCBFrontLine3;
        private Label label2;
        private PictureBox pictureBox1;
    }
}
