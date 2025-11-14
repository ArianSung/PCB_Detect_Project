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
            userIdTextBox = new TextBox();
            passwordTextBox = new TextBox();
            cardLogin = new Panel();
            btn_login = new Button();
            pictureBox6 = new PictureBox();
            pictureBox7 = new PictureBox();
            panel3 = new Panel();
            textBox1 = new TextBox();
            panel4 = new Panel();
            textBox2 = new TextBox();
            label4 = new Label();
            label5 = new Label();
            label6 = new Label();
            pictureBox3 = new PictureBox();
            textBox4 = new TextBox();
            cardLogin.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox6).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox7).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).BeginInit();
            SuspendLayout();
            // 
            // userIdTextBox
            // 
            userIdTextBox.BackColor = Color.FromArgb(64, 64, 64);
            userIdTextBox.BorderStyle = BorderStyle.None;
            userIdTextBox.Font = new Font("Arial", 24F, FontStyle.Regular, GraphicsUnit.Point, 0);
            userIdTextBox.ForeColor = SystemColors.Window;
            userIdTextBox.Location = new Point(868, 528);
            userIdTextBox.Name = "userIdTextBox";
            userIdTextBox.Size = new Size(312, 37);
            userIdTextBox.TabIndex = 8;
            // 
            // passwordTextBox
            // 
            passwordTextBox.BackColor = Color.FromArgb(64, 64, 64);
            passwordTextBox.BorderStyle = BorderStyle.None;
            passwordTextBox.Font = new Font("Arial", 24F, FontStyle.Regular, GraphicsUnit.Point, 0);
            passwordTextBox.ForeColor = SystemColors.Window;
            passwordTextBox.Location = new Point(868, 608);
            passwordTextBox.Name = "passwordTextBox";
            passwordTextBox.Size = new Size(312, 37);
            passwordTextBox.TabIndex = 9;
            passwordTextBox.UseSystemPasswordChar = true;
            // 
            // cardLogin
            // 
            cardLogin.BackColor = Color.FromArgb(44, 44, 44);
            cardLogin.Controls.Add(btn_login);
            cardLogin.Controls.Add(pictureBox6);
            cardLogin.Controls.Add(pictureBox7);
            cardLogin.Controls.Add(panel3);
            cardLogin.Controls.Add(textBox1);
            cardLogin.Controls.Add(panel4);
            cardLogin.Controls.Add(textBox2);
            cardLogin.Controls.Add(label4);
            cardLogin.Controls.Add(label5);
            cardLogin.Controls.Add(label6);
            cardLogin.Controls.Add(pictureBox3);
            cardLogin.Controls.Add(textBox4);
            cardLogin.Location = new Point(635, 260);
            cardLogin.Name = "cardLogin";
            cardLogin.Size = new Size(630, 600);
            cardLogin.TabIndex = 19;
            // 
            // btn_login
            // 
            btn_login.BackColor = Color.FromArgb(44, 44, 44);
            btn_login.FlatAppearance.BorderSize = 0;
            btn_login.FlatStyle = FlatStyle.Flat;
            btn_login.Font = new Font("Arial", 24F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btn_login.ForeColor = Color.White;
            btn_login.Location = new Point(225, 500);
            btn_login.Name = "btn_login";
            btn_login.Size = new Size(173, 57);
            btn_login.TabIndex = 38;
            btn_login.Text = "Log In";
            btn_login.UseVisualStyleBackColor = false;
            btn_login.Click += btn_login_Click;
            // 
            // pictureBox6
            // 
            pictureBox6.Image = Properties.Resources.Login_mail_white;
            pictureBox6.Location = new Point(90, 275);
            pictureBox6.Name = "pictureBox6";
            pictureBox6.Size = new Size(40, 40);
            pictureBox6.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox6.TabIndex = 21;
            pictureBox6.TabStop = false;
            // 
            // pictureBox7
            // 
            pictureBox7.Image = Properties.Resources.Login_unlock_white;
            pictureBox7.Location = new Point(83, 343);
            pictureBox7.Name = "pictureBox7";
            pictureBox7.Size = new Size(54, 50);
            pictureBox7.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox7.TabIndex = 20;
            pictureBox7.TabStop = false;
            // 
            // panel3
            // 
            panel3.BackColor = Color.Silver;
            panel3.ForeColor = Color.Silver;
            panel3.Location = new Point(80, 402);
            panel3.Name = "panel3";
            panel3.Size = new Size(455, 5);
            panel3.TabIndex = 37;
            // 
            // textBox1
            // 
            textBox1.BackColor = Color.FromArgb(44, 44, 44);
            textBox1.BorderStyle = BorderStyle.None;
            textBox1.Font = new Font("Arial", 24F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBox1.ForeColor = SystemColors.Window;
            textBox1.Location = new Point(223, 279);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(312, 37);
            textBox1.TabIndex = 34;
            // 
            // panel4
            // 
            panel4.BackColor = Color.Silver;
            panel4.ForeColor = Color.Silver;
            panel4.Location = new Point(80, 322);
            panel4.Name = "panel4";
            panel4.Size = new Size(455, 5);
            panel4.TabIndex = 36;
            // 
            // textBox2
            // 
            textBox2.BackColor = Color.FromArgb(44, 44, 44);
            textBox2.BorderStyle = BorderStyle.None;
            textBox2.Font = new Font("Arial", 24F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBox2.ForeColor = SystemColors.Window;
            textBox2.Location = new Point(223, 359);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(312, 37);
            textBox2.TabIndex = 35;
            textBox2.UseSystemPasswordChar = true;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Arial", 24F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label4.ForeColor = SystemColors.Window;
            label4.Location = new Point(136, 349);
            label4.Name = "label4";
            label4.Size = new Size(68, 36);
            label4.TabIndex = 33;
            label4.Text = "PW";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Arial", 24F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label5.ForeColor = SystemColors.Window;
            label5.Location = new Point(136, 269);
            label5.Name = "label5";
            label5.Size = new Size(47, 36);
            label5.TabIndex = 32;
            label5.Text = "ID";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Arial", 44.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label6.ForeColor = SystemColors.Window;
            label6.Location = new Point(167, 139);
            label6.Name = "label6";
            label6.Size = new Size(315, 66);
            label6.TabIndex = 20;
            label6.Text = "User Login";
            // 
            // pictureBox3
            // 
            pictureBox3.Image = (Image)resources.GetObject("pictureBox3.Image");
            pictureBox3.Location = new Point(261, 39);
            pictureBox3.Name = "pictureBox3";
            pictureBox3.Size = new Size(100, 97);
            pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox3.TabIndex = 31;
            pictureBox3.TabStop = false;
            // 
            // textBox4
            // 
            textBox4.BackColor = Color.FromArgb(44, 44, 44);
            textBox4.BorderStyle = BorderStyle.None;
            textBox4.Font = new Font("Arial", 24F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBox4.ForeColor = SystemColors.Window;
            textBox4.Location = new Point(223, 198);
            textBox4.Name = "textBox4";
            textBox4.Size = new Size(340, 37);
            textBox4.TabIndex = 13;
            // 
            // LoginForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(64, 64, 64);
            ClientSize = new Size(1904, 1041);
            Controls.Add(cardLogin);
            Controls.Add(userIdTextBox);
            Controls.Add(passwordTextBox);
            Name = "LoginForm";
            Text = "LoginForm";
            cardLogin.ResumeLayout(false);
            cardLogin.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox6).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox7).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TextBox userIdTextBox;
        private TextBox passwordTextBox;
        private Panel cardLogin;
        private Label label6;
        private PictureBox pictureBox3;
        private TextBox textBox4;
        private PictureBox pictureBox6;
        private PictureBox pictureBox7;
        private Panel panel3;
        private TextBox textBox1;
        private Panel panel4;
        private TextBox textBox2;
        private Label label4;
        private Label label5;
        private Button btn_login;
    }
}