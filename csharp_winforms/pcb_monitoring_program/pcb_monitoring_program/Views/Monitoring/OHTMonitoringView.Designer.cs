namespace pcb_monitoring_program.Views.Monitoring
{
    partial class OHTMonitoringView
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
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            label2 = new Label();
            cardOHTCallLog = new Panel();
            lvCallHistory = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            columnHeader3 = new ColumnHeader();
            columnHeader4 = new ColumnHeader();
            cardOHTMonitoring = new Panel();
            dgvOhtStatus = new DataGridView();
            colOhtId = new DataGridViewTextBoxColumn();
            colStatus = new DataGridViewTextBoxColumn();
            colLocation = new DataGridViewTextBoxColumn();
            colTarget = new DataGridViewTextBoxColumn();
            colJob = new DataGridViewTextBoxColumn();
            colUpdatedAt = new DataGridViewTextBoxColumn();
            cardOHTCall = new Panel();
            lblLastCall = new Label();
            btnOhtCall = new Button();
            cmbBoxId = new ComboBox();
            cmbOhtId = new ComboBox();
            label7 = new Label();
            label6 = new Label();
            label5 = new Label();
            label4 = new Label();
            lblAutoCallStatus = new Label();
            btnAutoCall = new Button();
            label1 = new Label();
            label8 = new Label();
            cardOHTCallLog.SuspendLayout();
            cardOHTMonitoring.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvOhtStatus).BeginInit();
            cardOHTCall.SuspendLayout();
            SuspendLayout();
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("맑은 고딕", 12F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label2.ForeColor = SystemColors.Window;
            label2.Location = new Point(445, 59);
            label2.Name = "label2";
            label2.Size = new Size(81, 21);
            label2.TabIndex = 4;
            label2.Text = "OHT 호출";
            // 
            // cardOHTCallLog
            // 
            cardOHTCallLog.Controls.Add(lvCallHistory);
            cardOHTCallLog.Location = new Point(1146, 83);
            cardOHTCallLog.Name = "cardOHTCallLog";
            cardOHTCallLog.Size = new Size(421, 634);
            cardOHTCallLog.TabIndex = 11;
            // 
            // lvCallHistory
            // 
            lvCallHistory.BackColor = Color.FromArgb(44, 44, 44);
            lvCallHistory.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2, columnHeader3, columnHeader4 });
            lvCallHistory.Dock = DockStyle.Fill;
            lvCallHistory.ForeColor = Color.White;
            lvCallHistory.FullRowSelect = true;
            lvCallHistory.GridLines = true;
            lvCallHistory.Location = new Point(0, 0);
            lvCallHistory.MultiSelect = false;
            lvCallHistory.Name = "lvCallHistory";
            lvCallHistory.Size = new Size(421, 634);
            lvCallHistory.TabIndex = 0;
            lvCallHistory.UseCompatibleStateImageBehavior = false;
            lvCallHistory.View = View.Details;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "시간";
            columnHeader1.Width = 140;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "OHT ID";
            columnHeader2.Width = 70;
            // 
            // columnHeader3
            // 
            columnHeader3.Text = "BOX ID";
            columnHeader3.Width = 80;
            // 
            // columnHeader4
            // 
            columnHeader4.Text = "작업";
            columnHeader4.Width = 80;
            // 
            // cardOHTMonitoring
            // 
            cardOHTMonitoring.Controls.Add(dgvOhtStatus);
            cardOHTMonitoring.Location = new Point(207, 427);
            cardOHTMonitoring.Name = "cardOHTMonitoring";
            cardOHTMonitoring.Size = new Size(773, 290);
            cardOHTMonitoring.TabIndex = 10;
            // 
            // dgvOhtStatus
            // 
            dgvOhtStatus.AllowUserToAddRows = false;
            dgvOhtStatus.AllowUserToDeleteRows = false;
            dgvOhtStatus.BackgroundColor = Color.FromArgb(44, 44, 44);
            dgvOhtStatus.BorderStyle = BorderStyle.None;
            dgvOhtStatus.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvOhtStatus.Columns.AddRange(new DataGridViewColumn[] { colOhtId, colStatus, colLocation, colTarget, colJob, colUpdatedAt });
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(44, 44, 44);
            dataGridViewCellStyle1.Font = new Font("맑은 고딕", 9F);
            dataGridViewCellStyle1.ForeColor = Color.White;
            dataGridViewCellStyle1.SelectionBackColor = Color.FromArgb(44, 44, 44);
            dataGridViewCellStyle1.SelectionForeColor = Color.White;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.False;
            dgvOhtStatus.DefaultCellStyle = dataGridViewCellStyle1;
            dgvOhtStatus.Location = new Point(74, 43);
            dgvOhtStatus.MultiSelect = false;
            dgvOhtStatus.Name = "dgvOhtStatus";
            dgvOhtStatus.ReadOnly = true;
            dgvOhtStatus.RowHeadersVisible = false;
            dgvOhtStatus.RowTemplate.Height = 45;
            dgvOhtStatus.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvOhtStatus.Size = new Size(632, 219);
            dgvOhtStatus.TabIndex = 0;
            // 
            // colOhtId
            // 
            colOhtId.HeaderText = "OHT ID";
            colOhtId.Name = "colOhtId";
            colOhtId.ReadOnly = true;
            colOhtId.Width = 80;
            // 
            // colStatus
            // 
            colStatus.HeaderText = "상태";
            colStatus.Name = "colStatus";
            colStatus.ReadOnly = true;
            // 
            // colLocation
            // 
            colLocation.HeaderText = "위치";
            colLocation.Name = "colLocation";
            colLocation.ReadOnly = true;
            colLocation.Width = 80;
            // 
            // colTarget
            // 
            colTarget.HeaderText = "목적지";
            colTarget.Name = "colTarget";
            colTarget.ReadOnly = true;
            colTarget.Width = 80;
            // 
            // colJob
            // 
            colJob.HeaderText = "현재 작업";
            colJob.Name = "colJob";
            colJob.ReadOnly = true;
            colJob.Width = 150;
            // 
            // colUpdatedAt
            // 
            colUpdatedAt.HeaderText = "업데이트 시간";
            colUpdatedAt.Name = "colUpdatedAt";
            colUpdatedAt.ReadOnly = true;
            colUpdatedAt.Width = 140;
            // 
            // cardOHTCall
            // 
            cardOHTCall.Controls.Add(lblLastCall);
            cardOHTCall.Controls.Add(btnOhtCall);
            cardOHTCall.Controls.Add(cmbBoxId);
            cardOHTCall.Controls.Add(cmbOhtId);
            cardOHTCall.Controls.Add(label7);
            cardOHTCall.Controls.Add(label6);
            cardOHTCall.Controls.Add(label5);
            cardOHTCall.Controls.Add(label4);
            cardOHTCall.Controls.Add(lblAutoCallStatus);
            cardOHTCall.Controls.Add(btnAutoCall);
            cardOHTCall.Location = new Point(445, 83);
            cardOHTCall.Name = "cardOHTCall";
            cardOHTCall.Size = new Size(535, 284);
            cardOHTCall.TabIndex = 8;
            // 
            // lblLastCall
            // 
            lblLastCall.AutoSize = true;
            lblLastCall.Font = new Font("맑은 고딕", 12F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblLastCall.ForeColor = SystemColors.Window;
            lblLastCall.Location = new Point(14, 244);
            lblLastCall.Name = "lblLastCall";
            lblLastCall.Size = new Size(103, 21);
            lblLastCall.TabIndex = 21;
            lblLastCall.Text = "최근 호출 : -";
            // 
            // btnOhtCall
            // 
            btnOhtCall.BackColor = Color.FromArgb(64, 64, 64);
            btnOhtCall.Font = new Font("맑은 고딕", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnOhtCall.ForeColor = Color.White;
            btnOhtCall.Location = new Point(393, 164);
            btnOhtCall.Name = "btnOhtCall";
            btnOhtCall.Size = new Size(120, 60);
            btnOhtCall.TabIndex = 20;
            btnOhtCall.Text = "호출";
            btnOhtCall.UseVisualStyleBackColor = false;
            btnOhtCall.Click += btnOhtCall_Click;
            // 
            // cmbBoxId
            // 
            cmbBoxId.FormattingEnabled = true;
            cmbBoxId.Location = new Point(174, 199);
            cmbBoxId.Name = "cmbBoxId";
            cmbBoxId.Size = new Size(162, 23);
            cmbBoxId.TabIndex = 19;
            // 
            // cmbOhtId
            // 
            cmbOhtId.FormattingEnabled = true;
            cmbOhtId.Location = new Point(174, 164);
            cmbOhtId.Name = "cmbOhtId";
            cmbOhtId.Size = new Size(162, 23);
            cmbOhtId.TabIndex = 18;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("맑은 고딕", 12F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label7.ForeColor = SystemColors.Window;
            label7.Location = new Point(14, 185);
            label7.Name = "label7";
            label7.Size = new Size(148, 21);
            label7.TabIndex = 17;
            label7.Text = "Box ID 입력 선택 :";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("맑은 고딕", 12F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label6.ForeColor = SystemColors.Window;
            label6.Location = new Point(14, 150);
            label6.Name = "label6";
            label6.Size = new Size(128, 21);
            label6.TabIndex = 16;
            label6.Text = "호출 대상 선택 :";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("맑은 고딕", 12F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label5.ForeColor = SystemColors.Window;
            label5.Location = new Point(3, 11);
            label5.Name = "label5";
            label5.Size = new Size(104, 21);
            label5.TabIndex = 15;
            label5.Text = "[ 자동 호출 ]";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("맑은 고딕", 12F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label4.ForeColor = SystemColors.Window;
            label4.Location = new Point(3, 115);
            label4.Name = "label4";
            label4.Size = new Size(104, 21);
            label4.TabIndex = 14;
            label4.Text = "[ 수동 호출 ]";
            // 
            // lblAutoCallStatus
            // 
            lblAutoCallStatus.AutoSize = true;
            lblAutoCallStatus.Font = new Font("맑은 고딕", 12F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblAutoCallStatus.ForeColor = SystemColors.Window;
            lblAutoCallStatus.Location = new Point(174, 45);
            lblAutoCallStatus.Name = "lblAutoCallStatus";
            lblAutoCallStatus.Size = new Size(114, 21);
            lblAutoCallStatus.TabIndex = 13;
            lblAutoCallStatus.Text = "자동 호출 OFF";
            // 
            // btnAutoCall
            // 
            btnAutoCall.BackColor = Color.FromArgb(64, 64, 64);
            btnAutoCall.Font = new Font("맑은 고딕", 9F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnAutoCall.ForeColor = Color.White;
            btnAutoCall.Location = new Point(14, 48);
            btnAutoCall.Name = "btnAutoCall";
            btnAutoCall.Size = new Size(120, 35);
            btnAutoCall.TabIndex = 0;
            btnAutoCall.Text = "자동 호출 활성화";
            btnAutoCall.UseVisualStyleBackColor = false;
            btnAutoCall.Click += btnAutoCall_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("맑은 고딕", 12F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label1.ForeColor = SystemColors.Window;
            label1.Location = new Point(207, 403);
            label1.Name = "label1";
            label1.Size = new Size(134, 21);
            label1.TabIndex = 12;
            label1.Text = "OHT Monitoring";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("맑은 고딕", 12F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label8.ForeColor = SystemColors.Window;
            label8.Location = new Point(1146, 59);
            label8.Name = "label8";
            label8.Size = new Size(118, 21);
            label8.TabIndex = 14;
            label8.Text = "최근 호출 이력";
            // 
            // OHTMonitoringView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(64, 64, 64);
            Controls.Add(label8);
            Controls.Add(label1);
            Controls.Add(cardOHTCallLog);
            Controls.Add(cardOHTMonitoring);
            Controls.Add(cardOHTCall);
            Controls.Add(label2);
            Name = "OHTMonitoringView";
            Size = new Size(1600, 800);
            Load += OHTMonitoringView_Load;
            cardOHTCallLog.ResumeLayout(false);
            cardOHTMonitoring.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvOhtStatus).EndInit();
            cardOHTCall.ResumeLayout(false);
            cardOHTCall.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label label2;
        private Panel cardOHTCallLog;
        private Panel cardOHTMonitoring;
        private Panel cardOHTCall;
        private Label lblAutoCallStatus;
        private Button btnAutoCall;
        private Label label1;
        private ComboBox cmbOhtId;
        private Label label7;
        private Label label6;
        private Label label5;
        private Label label4;
        private Button btnOhtCall;
        private ComboBox cmbBoxId;
        private Label lblLastCall;
        private DataGridView dgvOhtStatus;
        private DataGridViewTextBoxColumn colOhtId;
        private DataGridViewTextBoxColumn colStatus;
        private DataGridViewTextBoxColumn colLocation;
        private DataGridViewTextBoxColumn colTarget;
        private DataGridViewTextBoxColumn colJob;
        private DataGridViewTextBoxColumn colUpdatedAt;
        private Label label8;
        private ListView lvCallHistory;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private ColumnHeader columnHeader3;
        private ColumnHeader columnHeader4;
    }
}
