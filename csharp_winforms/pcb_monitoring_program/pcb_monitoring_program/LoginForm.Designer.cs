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
            cardLogin = new Panel();
            passwordTextBox = new TextBox();
            userIdTextBox = new TextBox();
            btn_login = new Button();
            pictureBox_Message = new PictureBox();
            pictureBox_lock = new PictureBox();
            panel_PW = new Panel();
            textBox_ID = new TextBox();
            panel_ID = new Panel();
            textBox_PW = new TextBox();
            label_PW = new Label();
            label_ID = new Label();
            label_UserLogin = new Label();
            pictureBox_user_image = new PictureBox();
            textBox4 = new TextBox();
            cardLogin.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox_Message).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox_lock).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox_user_image).BeginInit();
            SuspendLayout();
            // 
            // cardLogin
            // 
            cardLogin.BackColor = Color.FromArgb(44, 44, 44);
            cardLogin.Controls.Add(passwordTextBox);
            cardLogin.Controls.Add(userIdTextBox);
            cardLogin.Controls.Add(btn_login);
            cardLogin.Controls.Add(pictureBox_Message);
            cardLogin.Controls.Add(pictureBox_lock);
            cardLogin.Controls.Add(panel_PW);
            cardLogin.Controls.Add(textBox_ID);
            cardLogin.Controls.Add(panel_ID);
            cardLogin.Controls.Add(textBox_PW);
            cardLogin.Controls.Add(label_PW);
            cardLogin.Controls.Add(label_ID);
            cardLogin.Controls.Add(label_UserLogin);
            cardLogin.Controls.Add(pictureBox_user_image);
            cardLogin.Controls.Add(textBox4);
            cardLogin.Location = new Point(635, 260);
            cardLogin.Name = "cardLogin";
            cardLogin.Size = new Size(630, 600);
            cardLogin.TabIndex = 19;
            // 
            // passwordTextBox
            // 
            passwordTextBox.BackColor = Color.FromArgb(44, 44, 44);
            passwordTextBox.BorderStyle = BorderStyle.None;
            passwordTextBox.Font = new Font("Arial", 24F, FontStyle.Regular, GraphicsUnit.Point, 0);
            passwordTextBox.ForeColor = Color.White;
            passwordTextBox.Location = new Point(223, 359);
            passwordTextBox.Name = "passwordTextBox";
            passwordTextBox.Size = new Size(312, 37);
            passwordTextBox.TabIndex = 40;
            passwordTextBox.UseSystemPasswordChar = true;
            // 
            // userIdTextBox
            // 
            userIdTextBox.BackColor = Color.FromArgb(44, 44, 44);
            userIdTextBox.BorderStyle = BorderStyle.None;
            userIdTextBox.Font = new Font("Arial", 24F, FontStyle.Regular, GraphicsUnit.Point, 0);
            userIdTextBox.ForeColor = Color.White;
            userIdTextBox.Location = new Point(223, 279);
            userIdTextBox.Name = "userIdTextBox";
            userIdTextBox.Size = new Size(312, 37);
            userIdTextBox.TabIndex = 39;
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
            // pictureBox_Message
            // 
            pictureBox_Message.Image = Properties.Resources.Login_mail_white;
            pictureBox_Message.Location = new Point(90, 275);
            pictureBox_Message.Name = "pictureBox_Message";
            pictureBox_Message.Size = new Size(40, 40);
            pictureBox_Message.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox_Message.TabIndex = 21;
            pictureBox_Message.TabStop = false;
            // 
            // pictureBox_lock
            // 
            pictureBox_lock.Image = Properties.Resources.Login_unlock_white;
            pictureBox_lock.Location = new Point(83, 343);
            pictureBox_lock.Name = "pictureBox_lock";
            pictureBox_lock.Size = new Size(54, 50);
            pictureBox_lock.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox_lock.TabIndex = 20;
            pictureBox_lock.TabStop = false;
            // 
            // panel_PW
            // 
            panel_PW.BackColor = Color.Silver;
            panel_PW.ForeColor = Color.Silver;
            panel_PW.Location = new Point(80, 402);
            panel_PW.Name = "panel_PW";
            panel_PW.Size = new Size(455, 5);
            panel_PW.TabIndex = 37;
            // 
            // textBox_ID
            // 
            textBox_ID.BackColor = Color.FromArgb(44, 44, 44);
            textBox_ID.BorderStyle = BorderStyle.None;
            textBox_ID.Font = new Font("Arial", 24F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBox_ID.ForeColor = Color.White;
            textBox_ID.Location = new Point(223, 279);
            textBox_ID.Name = "textBox_ID";
            textBox_ID.Size = new Size(312, 37);
            textBox_ID.TabIndex = 34;
            // 
            // panel_ID
            // 
            panel_ID.BackColor = Color.Silver;
            panel_ID.ForeColor = Color.Silver;
            panel_ID.Location = new Point(80, 322);
            panel_ID.Name = "panel_ID";
            panel_ID.Size = new Size(455, 5);
            panel_ID.TabIndex = 36;
            // 
            // textBox_PW
            // 
            textBox_PW.BackColor = Color.FromArgb(44, 44, 44);
            textBox_PW.BorderStyle = BorderStyle.None;
            textBox_PW.Font = new Font("Arial", 24F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBox_PW.ForeColor = Color.White;
            textBox_PW.Location = new Point(223, 359);
            textBox_PW.Name = "textBox_PW";
            textBox_PW.Size = new Size(312, 37);
            textBox_PW.TabIndex = 35;
            textBox_PW.UseSystemPasswordChar = true;
            // 
            // label_PW
            // 
            label_PW.AutoSize = true;
            label_PW.Font = new Font("Arial", 24F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_PW.ForeColor = SystemColors.Window;
            label_PW.Location = new Point(136, 349);
            label_PW.Name = "label_PW";
            label_PW.Size = new Size(68, 36);
            label_PW.TabIndex = 33;
            label_PW.Text = "PW";
            // 
            // label_ID
            // 
            label_ID.AutoSize = true;
            label_ID.Font = new Font("Arial", 24F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_ID.ForeColor = SystemColors.Window;
            label_ID.Location = new Point(136, 269);
            label_ID.Name = "label_ID";
            label_ID.Size = new Size(47, 36);
            label_ID.TabIndex = 32;
            label_ID.Text = "ID";
            // 
            // label_UserLogin
            // 
            label_UserLogin.AutoSize = true;
            label_UserLogin.Font = new Font("Arial", 44.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_UserLogin.ForeColor = SystemColors.Window;
            label_UserLogin.Location = new Point(167, 139);
            label_UserLogin.Name = "label_UserLogin";
            label_UserLogin.Size = new Size(315, 66);
            label_UserLogin.TabIndex = 20;
            label_UserLogin.Text = "User Login";
            // 
            // pictureBox_user_image
            // 
            pictureBox_user_image.Image = (Image)resources.GetObject("pictureBox_user_image.Image");
            pictureBox_user_image.Location = new Point(261, 39);
            pictureBox_user_image.Name = "pictureBox_user_image";
            pictureBox_user_image.Size = new Size(100, 97);
            pictureBox_user_image.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox_user_image.TabIndex = 31;
            pictureBox_user_image.TabStop = false;
            // 
            // LoginForm
            // 
            AcceptButton = btn_login;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(64, 64, 64);
            ClientSize = new Size(1904, 1041);
            Controls.Add(cardLogin);
            Name = "LoginForm";
            Text = "LoginForm";
            cardLogin.ResumeLayout(false);
            cardLogin.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox_Message).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox_lock).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox_user_image).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel cardLogin;
        private Label label_UserLogin;
        private PictureBox pictureBox_user_image;
        private TextBox textBox4;
        private PictureBox pictureBox_Message;
        private PictureBox pictureBox_lock;
        private Panel panel_PW;
        private TextBox textBox_ID;
        private Panel panel_ID;
        private TextBox textBox_PW;
        private Label label_PW;
        private Label label_ID;
        private Button btn_login;
        private TextBox passwordTextBox;
        private TextBox userIdTextBox;
    }
}