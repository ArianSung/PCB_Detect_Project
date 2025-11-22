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
            lblStatusText = new Label();
            lblLineName = new Label();
            pnlStatusDot = new Panel();
            cardLineStatus = new Panel();
            btnTestDown = new Button();
            btnTestIdle = new Button();
            btnTestRun = new Button();
            pnlStatusDot3 = new Panel();
            lblStatusText3 = new Label();
            pnlStatusDot2 = new Panel();
            label6 = new Label();
            lblStatusText2 = new Label();
            label4 = new Label();
            label3 = new Label();
            cardLineAlarm = new Panel();
            label1 = new Label();
            lvAlarmHistory = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            columnHeader3 = new ColumnHeader();
            columnHeader4 = new ColumnHeader();
            columnHeader5 = new ColumnHeader();
            cardLineMonitoring.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            cardLineStatus.SuspendLayout();
            cardLineAlarm.SuspendLayout();
            SuspendLayout();
            // 
            // cardLineMonitoring
            // 
            cardLineMonitoring.Controls.Add(label2);
            cardLineMonitoring.Controls.Add(pictureBox1);
            cardLineMonitoring.Location = new Point(0, 38);
            cardLineMonitoring.Name = "cardLineMonitoring";
            cardLineMonitoring.Size = new Size(755, 715);
            cardLineMonitoring.TabIndex = 4;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("맑은 고딕", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label2.ForeColor = SystemColors.Window;
            label2.Location = new Point(0, 0);
            label2.Name = "label2";
            label2.Size = new Size(179, 30);
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
            // lblStatusText
            // 
            lblStatusText.AutoSize = true;
            lblStatusText.Font = new Font("맑은 고딕", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 129);
            lblStatusText.ForeColor = Color.White;
            lblStatusText.Location = new Point(162, 93);
            lblStatusText.Name = "lblStatusText";
            lblStatusText.Size = new Size(76, 30);
            lblStatusText.TabIndex = 11;
            lblStatusText.Text = "가동중";
            // 
            // lblLineName
            // 
            lblLineName.AutoSize = true;
            lblLineName.Font = new Font("맑은 고딕", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 129);
            lblLineName.ForeColor = Color.White;
            lblLineName.Location = new Point(85, 93);
            lblLineName.Name = "lblLineName";
            lblLineName.Size = new Size(63, 30);
            lblLineName.TabIndex = 6;
            lblLineName.Text = "Line1";
            // 
            // pnlStatusDot
            // 
            pnlStatusDot.BackColor = Color.Lime;
            pnlStatusDot.Location = new Point(52, 117);
            pnlStatusDot.Name = "pnlStatusDot";
            pnlStatusDot.Size = new Size(14, 14);
            pnlStatusDot.TabIndex = 5;
            pnlStatusDot.Resize += pnlStatusDot_Resize;
            // 
            // cardLineStatus
            // 
            cardLineStatus.Controls.Add(btnTestDown);
            cardLineStatus.Controls.Add(btnTestIdle);
            cardLineStatus.Controls.Add(btnTestRun);
            cardLineStatus.Controls.Add(pnlStatusDot3);
            cardLineStatus.Controls.Add(lblStatusText3);
            cardLineStatus.Controls.Add(pnlStatusDot2);
            cardLineStatus.Controls.Add(label6);
            cardLineStatus.Controls.Add(lblStatusText2);
            cardLineStatus.Controls.Add(label4);
            cardLineStatus.Controls.Add(label3);
            cardLineStatus.Controls.Add(pnlStatusDot);
            cardLineStatus.Controls.Add(lblStatusText);
            cardLineStatus.Controls.Add(lblLineName);
            cardLineStatus.Location = new Point(845, 38);
            cardLineStatus.Name = "cardLineStatus";
            cardLineStatus.Size = new Size(755, 297);
            cardLineStatus.TabIndex = 5;
            // 
            // btnTestDown
            // 
            btnTestDown.Location = new Point(544, 154);
            btnTestDown.Name = "btnTestDown";
            btnTestDown.Size = new Size(75, 23);
            btnTestDown.TabIndex = 17;
            btnTestDown.Text = "down";
            btnTestDown.UseVisualStyleBackColor = true;
            btnTestDown.Click += btnTestDown_Click;
            // 
            // btnTestIdle
            // 
            btnTestIdle.Location = new Point(533, 108);
            btnTestIdle.Name = "btnTestIdle";
            btnTestIdle.Size = new Size(75, 23);
            btnTestIdle.TabIndex = 16;
            btnTestIdle.Text = "idle";
            btnTestIdle.UseVisualStyleBackColor = true;
            btnTestIdle.Click += btnTestIdle_Click;
            // 
            // btnTestRun
            // 
            btnTestRun.Location = new Point(519, 56);
            btnTestRun.Name = "btnTestRun";
            btnTestRun.Size = new Size(75, 23);
            btnTestRun.TabIndex = 15;
            btnTestRun.Text = "run";
            btnTestRun.UseVisualStyleBackColor = true;
            btnTestRun.Click += btnTestRun_Click;
            // 
            // pnlStatusDot3
            // 
            pnlStatusDot3.BackColor = Color.Lime;
            pnlStatusDot3.Location = new Point(52, 223);
            pnlStatusDot3.Name = "pnlStatusDot3";
            pnlStatusDot3.Size = new Size(14, 14);
            pnlStatusDot3.TabIndex = 12;
            // 
            // lblStatusText3
            // 
            lblStatusText3.AutoSize = true;
            lblStatusText3.Font = new Font("맑은 고딕", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 129);
            lblStatusText3.ForeColor = Color.White;
            lblStatusText3.Location = new Point(162, 197);
            lblStatusText3.Name = "lblStatusText3";
            lblStatusText3.Size = new Size(76, 30);
            lblStatusText3.TabIndex = 14;
            lblStatusText3.Text = "가동중";
            // 
            // pnlStatusDot2
            // 
            pnlStatusDot2.BackColor = Color.Lime;
            pnlStatusDot2.Location = new Point(52, 170);
            pnlStatusDot2.Name = "pnlStatusDot2";
            pnlStatusDot2.Size = new Size(14, 14);
            pnlStatusDot2.TabIndex = 12;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("맑은 고딕", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 129);
            label6.ForeColor = Color.White;
            label6.Location = new Point(85, 197);
            label6.Name = "label6";
            label6.Size = new Size(63, 30);
            label6.TabIndex = 13;
            label6.Text = "Line3";
            // 
            // lblStatusText2
            // 
            lblStatusText2.AutoSize = true;
            lblStatusText2.Font = new Font("맑은 고딕", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 129);
            lblStatusText2.ForeColor = Color.White;
            lblStatusText2.Location = new Point(162, 145);
            lblStatusText2.Name = "lblStatusText2";
            lblStatusText2.Size = new Size(76, 30);
            lblStatusText2.TabIndex = 14;
            lblStatusText2.Text = "가동중";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("맑은 고딕", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label4.ForeColor = SystemColors.Window;
            label4.Location = new Point(0, 0);
            label4.Name = "label4";
            label4.Size = new Size(128, 30);
            label4.TabIndex = 5;
            label4.Text = "LINE Status";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("맑은 고딕", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 129);
            label3.ForeColor = Color.White;
            label3.Location = new Point(85, 145);
            label3.Name = "label3";
            label3.Size = new Size(63, 30);
            label3.TabIndex = 13;
            label3.Text = "Line2";
            // 
            // cardLineAlarm
            // 
            cardLineAlarm.Controls.Add(label1);
            cardLineAlarm.Controls.Add(lvAlarmHistory);
            cardLineAlarm.Location = new Point(845, 376);
            cardLineAlarm.Name = "cardLineAlarm";
            cardLineAlarm.Size = new Size(755, 377);
            cardLineAlarm.TabIndex = 15;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("맑은 고딕", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label1.ForeColor = SystemColors.Window;
            label1.Location = new Point(0, 0);
            label1.Name = "label1";
            label1.Size = new Size(172, 30);
            label1.TabIndex = 18;
            label1.Text = "LINE Status Log";
            // 
            // lvAlarmHistory
            // 
            lvAlarmHistory.BackColor = Color.FromArgb(44, 44, 44);
            lvAlarmHistory.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2, columnHeader3, columnHeader4, columnHeader5 });
            lvAlarmHistory.ForeColor = Color.White;
            lvAlarmHistory.FullRowSelect = true;
            lvAlarmHistory.GridLines = true;
            lvAlarmHistory.Location = new Point(0, 68);
            lvAlarmHistory.MultiSelect = false;
            lvAlarmHistory.Name = "lvAlarmHistory";
            lvAlarmHistory.Size = new Size(755, 309);
            lvAlarmHistory.TabIndex = 0;
            lvAlarmHistory.UseCompatibleStateImageBehavior = false;
            lvAlarmHistory.View = View.Details;
            lvAlarmHistory.ColumnWidthChanging += lvAlarmHistory_ColumnWidthChanging;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "시간";
            columnHeader1.Width = 140;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "라인";
            // 
            // columnHeader3
            // 
            columnHeader3.Text = "상태";
            columnHeader3.Width = 80;
            // 
            // columnHeader4
            // 
            columnHeader4.Text = "메시지";
            columnHeader4.Width = 250;
            // 
            // columnHeader5
            // 
            columnHeader5.Text = "가동 시간";
            columnHeader5.Width = 100;
            // 
            // LineMonitoringView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(64, 64, 64);
            Controls.Add(cardLineAlarm);
            Controls.Add(cardLineStatus);
            Controls.Add(cardLineMonitoring);
            Name = "LineMonitoringView";
            Size = new Size(1600, 800);
            Load += LineMonitoringView_Load;
            cardLineMonitoring.ResumeLayout(false);
            cardLineMonitoring.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            cardLineStatus.ResumeLayout(false);
            cardLineStatus.PerformLayout();
            cardLineAlarm.ResumeLayout(false);
            cardLineAlarm.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel cardLineMonitoring;
        private Label label2;
        private PictureBox pictureBox1;
        private Label lblStatusText;
        private Label lblLineName;
        private Panel pnlStatusDot;
        private Panel cardLineStatus;
        private Label label4;
        private Panel pnlStatusDot3;
        private Label lblStatusText3;
        private Panel pnlStatusDot2;
        private Label label6;
        private Label lblStatusText2;
        private Label label3;
        private Panel cardLineAlarm;
        private ListView lvAlarmHistory;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private ColumnHeader columnHeader3;
        private ColumnHeader columnHeader4;
        private Button btnTestDown;
        private Button btnTestIdle;
        private Button btnTestRun;
        private ColumnHeader columnHeader5;
        private Label label1;
    }
}
