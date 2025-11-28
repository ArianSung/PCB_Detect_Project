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
            pb_LINE1PCBFRONT = new PictureBox();
            cardPCBFrontMonitoring = new Panel();
            label2 = new Label();
            cardPCBBackMonitoring = new Panel();
            label1 = new Label();
            pb_LINE1PCBBACK = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)pb_LINE1PCBFRONT).BeginInit();
            cardPCBFrontMonitoring.SuspendLayout();
            cardPCBBackMonitoring.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pb_LINE1PCBBACK).BeginInit();
            SuspendLayout();
            // 
            // pb_LINE1PCBFRONT
            // 
            pb_LINE1PCBFRONT.BackColor = Color.FromArgb(128, 128, 255);
            pb_LINE1PCBFRONT.Location = new Point(52, 40);
            pb_LINE1PCBFRONT.Name = "pb_LINE1PCBFRONT";
            pb_LINE1PCBFRONT.Size = new Size(640, 640);
            pb_LINE1PCBFRONT.TabIndex = 0;
            pb_LINE1PCBFRONT.TabStop = false;
            // 
            // cardPCBFrontMonitoring
            // 
            cardPCBFrontMonitoring.Controls.Add(label2);
            cardPCBFrontMonitoring.Controls.Add(pb_LINE1PCBFRONT);
            cardPCBFrontMonitoring.Location = new Point(3, 3);
            cardPCBFrontMonitoring.Name = "cardPCBFrontMonitoring";
            cardPCBFrontMonitoring.Size = new Size(752, 682);
            cardPCBFrontMonitoring.TabIndex = 2;
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
            label2.Text = "LINE 1 PCB Front";
            // 
            // cardPCBBackMonitoring
            // 
            cardPCBBackMonitoring.Controls.Add(label1);
            cardPCBBackMonitoring.Controls.Add(pb_LINE1PCBBACK);
            cardPCBBackMonitoring.Location = new Point(847, 3);
            cardPCBBackMonitoring.Name = "cardPCBBackMonitoring";
            cardPCBBackMonitoring.Size = new Size(752, 682);
            cardPCBBackMonitoring.TabIndex = 5;
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
            label1.Text = "LINE 1 PCB Back";
            // 
            // pb_LINE1PCBBACK
            // 
            pb_LINE1PCBBACK.BackColor = Color.FromArgb(128, 128, 255);
            pb_LINE1PCBBACK.Location = new Point(52, 40);
            pb_LINE1PCBBACK.Name = "pb_LINE1PCBBACK";
            pb_LINE1PCBBACK.Size = new Size(640, 640);
            pb_LINE1PCBBACK.TabIndex = 0;
            pb_LINE1PCBBACK.TabStop = false;
            // 
            // PCBMonitoringView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(64, 64, 64);
            Controls.Add(cardPCBBackMonitoring);
            Controls.Add(cardPCBFrontMonitoring);
            Name = "PCBMonitoringView";
            Size = new Size(1600, 720);
            Load += PCBMonitoringView_Load;
            ((System.ComponentModel.ISupportInitialize)pb_LINE1PCBFRONT).EndInit();
            cardPCBFrontMonitoring.ResumeLayout(false);
            cardPCBFrontMonitoring.PerformLayout();
            cardPCBBackMonitoring.ResumeLayout(false);
            cardPCBBackMonitoring.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pb_LINE1PCBBACK).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox pb_LINE1PCBFRONT;
        private Panel cardPCBFrontMonitoring;
        private Label label2;
        private Panel cardPCBBackMonitoring;
        private Label label1;
        private PictureBox pb_LINE1PCBBACK;
    }
}
