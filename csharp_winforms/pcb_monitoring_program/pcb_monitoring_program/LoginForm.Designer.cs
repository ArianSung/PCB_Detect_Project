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
            picturebox_Login_background = new PictureBox();
            label3 = new Label();
            pictureBox4 = new PictureBox();
            pictureBox5 = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picturebox_Login_background).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox4).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox5).BeginInit();
            SuspendLayout();
            // 
            // panel2
            // 
            panel2.BackColor = Color.Silver;
            panel2.ForeColor = Color.Silver;
            panel2.Location = new Point(725, 651);
            panel2.Name = "panel2";
            panel2.Size = new Size(455, 5);
            panel2.TabIndex = 11;
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
            // panel1
            // 
            panel1.BackColor = Color.Silver;
            panel1.ForeColor = Color.Silver;
            panel1.Location = new Point(725, 571);
            panel1.Name = "panel1";
            panel1.Size = new Size(455, 5);
            panel1.TabIndex = 10;
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
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Arial", 24F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label2.ForeColor = SystemColors.Window;
            label2.Location = new Point(781, 608);
            label2.Name = "label2";
            label2.Size = new Size(68, 36);
            label2.TabIndex = 7;
            label2.Text = "PW";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Arial", 24F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label1.ForeColor = SystemColors.Window;
            label1.Location = new Point(781, 528);
            label1.Name = "label1";
            label1.Size = new Size(47, 36);
            label1.TabIndex = 6;
            label1.Text = "ID";
            // 
            // btn_login
            // 
            btn_login.BackColor = Color.FromArgb(64, 64, 64);
            btn_login.FlatAppearance.BorderColor = Color.FromArgb(64, 64, 64);
            btn_login.FlatAppearance.BorderSize = 0;
            btn_login.FlatStyle = FlatStyle.Flat;
            btn_login.Font = new Font("Arial", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btn_login.ForeColor = Color.Transparent;
            btn_login.Location = new Point(852, 720);
            btn_login.Name = "btn_login";
            btn_login.Size = new Size(201, 54);
            btn_login.TabIndex = 12;
            btn_login.TabStop = false;
            btn_login.Text = "Sign In";
            btn_login.UseVisualStyleBackColor = false;
            btn_login.Click += btn_login_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(905, 304);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(100, 100);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 13;
            pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            pictureBox2.Image = Properties.Resources.Login_unlock_white;
            pictureBox2.Location = new Point(724, 596);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(54, 50);
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox2.TabIndex = 14;
            pictureBox2.TabStop = false;
            // 
            // picturebox_Login_background
            // 
            picturebox_Login_background.Image = (Image)resources.GetObject("picturebox_Login_background.Image");
            picturebox_Login_background.Location = new Point(645, 240);
            picturebox_Login_background.Name = "picturebox_Login_background";
            picturebox_Login_background.Size = new Size(630, 600);
            picturebox_Login_background.SizeMode = PictureBoxSizeMode.Zoom;
            picturebox_Login_background.TabIndex = 15;
            picturebox_Login_background.TabStop = false;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Arial", 44.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label3.ForeColor = SystemColors.Window;
            label3.Location = new Point(806, 407);
            label3.Name = "label3";
            label3.Size = new Size(315, 66);
            label3.TabIndex = 16;
            label3.Text = "User Login";
            // 
            // pictureBox4
            // 
            pictureBox4.Image = Properties.Resources.Login_mail_white;
            pictureBox4.Location = new Point(731, 528);
            pictureBox4.Name = "pictureBox4";
            pictureBox4.Size = new Size(40, 40);
            pictureBox4.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox4.TabIndex = 17;
            pictureBox4.TabStop = false;
            // 
            // pictureBox5
            // 
            pictureBox5.Image = Properties.Resources.Login_btn_background_black1;
            pictureBox5.Location = new Point(832, 715);
            pictureBox5.Name = "pictureBox5";
            pictureBox5.Size = new Size(250, 75);
            pictureBox5.SizeMode = PictureBoxSizeMode.AutoSize;
            pictureBox5.TabIndex = 18;
            pictureBox5.TabStop = false;
            // 
            // LoginForm
            // 
            AcceptButton = btn_login;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(64, 64, 64);
            ClientSize = new Size(1904, 1041);
            Controls.Add(pictureBox4);
            Controls.Add(label3);
            Controls.Add(pictureBox2);
            Controls.Add(pictureBox1);
            Controls.Add(btn_login);
            Controls.Add(panel2);
            Controls.Add(userIdTextBox);
            Controls.Add(panel1);
            Controls.Add(passwordTextBox);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(pictureBox5);
            Controls.Add(picturebox_Login_background);
            Name = "LoginForm";
            Text = "LoginForm";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ((System.ComponentModel.ISupportInitialize)picturebox_Login_background).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox4).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox5).EndInit();
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
        private PictureBox picturebox_Login_background;
        private Label label3;
        private PictureBox pictureBox4;
        private PictureBox pictureBox5;
    }
}