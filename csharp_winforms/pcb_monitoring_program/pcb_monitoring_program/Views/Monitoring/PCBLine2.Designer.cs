namespace pcb_monitoring_program.Views.Monitoring
{
    partial class PCBLine2
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
            cardPCBBackLine2 = new Panel();
            label1 = new Label();
            pictureBox2 = new PictureBox();
            cardPCBFrontLine2 = new Panel();
            label2 = new Label();
            pictureBox1 = new PictureBox();
            cardPCBBackLine2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            cardPCBFrontLine2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // cardPCBBackLine2
            // 
            cardPCBBackLine2.Controls.Add(label1);
            cardPCBBackLine2.Controls.Add(pictureBox2);
            cardPCBBackLine2.Location = new Point(847, 3);
            cardPCBBackLine2.Name = "cardPCBBackLine2";
            cardPCBBackLine2.Size = new Size(752, 682);
            cardPCBBackLine2.TabIndex = 7;
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
            label1.Text = "LINE 2 PCB Back";
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
            // cardPCBFrontLine2
            // 
            cardPCBFrontLine2.Controls.Add(label2);
            cardPCBFrontLine2.Controls.Add(pictureBox1);
            cardPCBFrontLine2.Location = new Point(3, 3);
            cardPCBFrontLine2.Name = "cardPCBFrontLine2";
            cardPCBFrontLine2.Size = new Size(752, 682);
            cardPCBFrontLine2.TabIndex = 6;
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
            label2.Text = "LINE 2 PCB Front";
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
            // PCBLine2
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(64, 64, 64);
            Controls.Add(cardPCBBackLine2);
            Controls.Add(cardPCBFrontLine2);
            Name = "PCBLine2";
            Size = new Size(1600, 700);
            cardPCBBackLine2.ResumeLayout(false);
            cardPCBBackLine2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            cardPCBFrontLine2.ResumeLayout(false);
            cardPCBFrontLine2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel cardPCBBackLine2;
        private Label label1;
        private PictureBox pictureBox2;
        private Panel cardPCBFrontLine2;
        private Label label2;
        private PictureBox pictureBox1;
    }
}
