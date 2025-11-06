namespace pcb_monitoring_program
{
    partial class LoginForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginForm));
            panel2 = new Panel();
            userIdTextBox = new TextBox();
            panel1 = new Panel();
            passwordTextBox = new TextBox();
            label2 = new Label();
            label1 = new Label();
            btn_login = new Button();
            pictureBox1 = new PictureBox();
            pictureBox2 = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            SuspendLayout();
            // 
            // panel2
            // 
            panel2.BackColor = Color.Silver;
            panel2.ForeColor = Color.Silver;
            panel2.Location = new Point(813, 581);
            panel2.Name = "panel2";
            panel2.Size = new Size(455, 5);
            panel2.TabIndex = 11;
            // 
            // userIdTextBox
            // 
            userIdTextBox.BackColor = Color.FromArgb(64, 64, 64);
            userIdTextBox.BorderStyle = BorderStyle.None;
            userIdTextBox.Font = new Font("맑은 고딕", 24F, FontStyle.Regular, GraphicsUnit.Point, 129);
            userIdTextBox.ForeColor = SystemColors.Window;
            userIdTextBox.Location = new Point(820, 455);
            userIdTextBox.Name = "userIdTextBox";
            userIdTextBox.Size = new Size(426, 43);
            userIdTextBox.TabIndex = 8;
            // 
            // panel1
            // 
            panel1.BackColor = Color.Silver;
            panel1.ForeColor = Color.Silver;
            panel1.Location = new Point(813, 498);
            panel1.Name = "panel1";
            panel1.Size = new Size(455, 5);
            panel1.TabIndex = 10;
            // 
            // passwordTextBox
            // 
            passwordTextBox.BackColor = Color.FromArgb(64, 64, 64);
            passwordTextBox.BorderStyle = BorderStyle.None;
            passwordTextBox.Font = new Font("맑은 고딕", 24F, FontStyle.Regular, GraphicsUnit.Point, 129);
            passwordTextBox.ForeColor = SystemColors.Window;
            passwordTextBox.Location = new Point(820, 537);
            passwordTextBox.Name = "passwordTextBox";
            passwordTextBox.Size = new Size(426, 43);
            passwordTextBox.TabIndex = 9;
            passwordTextBox.UseSystemPasswordChar = true;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Arial", 24F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.ForeColor = SystemColors.Window;
            label2.Location = new Point(626, 543);
            label2.Name = "label2";
            label2.Size = new Size(191, 37);
            label2.TabIndex = 7;
            label2.Text = "PassWord :";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Arial", 24F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = SystemColors.Window;
            label1.Location = new Point(750, 459);
            label1.Name = "label1";
            label1.Size = new Size(67, 37);
            label1.TabIndex = 6;
            label1.Text = "ID :";
            // 
            // btn_login
            // 
            btn_login.Font = new Font("맑은 고딕", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            btn_login.Location = new Point(1317, 455);
            btn_login.Name = "btn_login";
            btn_login.Size = new Size(158, 131);
            btn_login.TabIndex = 12;
            btn_login.Text = "Sign In";
            btn_login.UseVisualStyleBackColor = true;
            btn_login.Click += btn_login_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(694, 455);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(50, 50);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 13;
            pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            pictureBox2.Image = (Image)resources.GetObject("pictureBox2.Image");
            pictureBox2.Location = new Point(566, 537);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(54, 50);
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox2.TabIndex = 14;
            pictureBox2.TabStop = false;
            // 
            // LoginForm
            // 
            AcceptButton = btn_login;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(64, 64, 64);
            ClientSize = new Size(1904, 1041);
            Controls.Add(pictureBox2);
            Controls.Add(pictureBox1);
            Controls.Add(btn_login);
            Controls.Add(panel2);
            Controls.Add(userIdTextBox);
            Controls.Add(panel1);
            Controls.Add(passwordTextBox);
            Controls.Add(label2);
            Controls.Add(label1);
            Name = "LoginForm";
            Text = "LoginForm";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panel2;
        private TextBox userIdTextBox;
        private Panel panel1;
        private TextBox passwordTextBox;
        private Label label2;
        private Label label1;
        private Button btn_login;
        private PictureBox pictureBox1;
        private PictureBox pictureBox2;
    }
}