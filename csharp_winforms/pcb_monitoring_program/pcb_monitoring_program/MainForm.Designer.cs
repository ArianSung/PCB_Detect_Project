namespace pcb_monitoring_program
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            btnDashboard = new Button();
            btnStatistics = new Button();
            btnMonitoring = new Button();
            btnUserManagement = new Button();
            btnSetting = new Button();
            panelContent = new Panel();
            labelTitle = new Label();
            labelTime = new Label();
            timerClock = new System.Windows.Forms.Timer(components);
            SuspendLayout();
            // 
            // btnDashboard
            // 
            btnDashboard.Font = new Font("Arial", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnDashboard.Location = new Point(60, 193);
            btnDashboard.Name = "btnDashboard";
            btnDashboard.Size = new Size(140, 100);
            btnDashboard.TabIndex = 0;
            btnDashboard.Text = "Dashboard";
            btnDashboard.UseVisualStyleBackColor = true;
            btnDashboard.Click += btnDashboard_Click;
            // 
            // btnStatistics
            // 
            btnStatistics.Font = new Font("Arial", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnStatistics.Location = new Point(60, 324);
            btnStatistics.Name = "btnStatistics";
            btnStatistics.Size = new Size(140, 100);
            btnStatistics.TabIndex = 1;
            btnStatistics.Text = "Statistics";
            btnStatistics.UseVisualStyleBackColor = true;
            btnStatistics.Click += btnStatistics_Click;
            // 
            // btnMonitoring
            // 
            btnMonitoring.Font = new Font("Arial", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnMonitoring.Location = new Point(60, 455);
            btnMonitoring.Name = "btnMonitoring";
            btnMonitoring.Size = new Size(140, 100);
            btnMonitoring.TabIndex = 2;
            btnMonitoring.Text = "Monitoring";
            btnMonitoring.UseVisualStyleBackColor = true;
            btnMonitoring.Click += btnMonitoring_Click;
            // 
            // btnUserManagement
            // 
            btnUserManagement.Font = new Font("Arial", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnUserManagement.Location = new Point(60, 586);
            btnUserManagement.Name = "btnUserManagement";
            btnUserManagement.Size = new Size(140, 100);
            btnUserManagement.TabIndex = 3;
            btnUserManagement.Text = "User Management";
            btnUserManagement.UseVisualStyleBackColor = true;
            btnUserManagement.Click += btnUserManagement_Click;
            // 
            // btnSetting
            // 
            btnSetting.Font = new Font("Arial", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnSetting.Location = new Point(60, 717);
            btnSetting.Name = "btnSetting";
            btnSetting.Size = new Size(140, 100);
            btnSetting.TabIndex = 4;
            btnSetting.Text = "Setting";
            btnSetting.UseVisualStyleBackColor = true;
            btnSetting.Click += btnSetting_Click;
            // 
            // panelContent
            // 
            panelContent.Location = new Point(263, 129);
            panelContent.Name = "panelContent";
            panelContent.Size = new Size(1600, 900);
            panelContent.TabIndex = 5;
            // 
            // labelTitle
            // 
            labelTitle.AutoSize = true;
            labelTitle.Font = new Font("Arial", 36F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelTitle.ForeColor = SystemColors.Window;
            labelTitle.Location = new Point(910, 28);
            labelTitle.Name = "labelTitle";
            labelTitle.Size = new Size(248, 56);
            labelTitle.TabIndex = 6;
            labelTitle.Text = "Loading...";
            // 
            // labelTime
            // 
            labelTime.AutoSize = true;
            labelTime.Font = new Font("Arial", 24F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelTime.Location = new Point(1445, 67);
            labelTime.Name = "labelTime";
            labelTime.Size = new Size(231, 36);
            labelTime.TabIndex = 7;
            labelTime.Text = "Time Loading...";
            // 
            // timerClock
            // 
            timerClock.Enabled = true;
            timerClock.Interval = 1000;
            timerClock.Tick += timerClock_Tick;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(64, 64, 64);
            ClientSize = new Size(1904, 1051);
            Controls.Add(labelTime);
            Controls.Add(labelTitle);
            Controls.Add(panelContent);
            Controls.Add(btnSetting);
            Controls.Add(btnUserManagement);
            Controls.Add(btnMonitoring);
            Controls.Add(btnStatistics);
            Controls.Add(btnDashboard);
            ForeColor = SystemColors.Window;
            Name = "MainForm";
            Text = "PCB_Detect_Monitoring";
            WindowState = FormWindowState.Maximized;
            Load += MainForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnDashboard;
        private Button btnStatistics;
        private Button btnMonitoring;
        private Button btnUserManagement;
        private Button btnSetting;
        private Panel panelContent;
        private Label labelTitle;
        private Label labelTime;
        private System.Windows.Forms.Timer timerClock;
    }
}
