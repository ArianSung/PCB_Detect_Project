namespace pcb_monitoring_program.Views.UserManagement
{
    partial class UserManagementForm_EditUser
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserManagementForm_EditUser));
            cardEditUserBackground = new Panel();
            cardEditUser = new Panel();
            btn_UM_Edit_PW = new Button();
            CB_UM_Edit_Active_True = new CheckBox();
            label_UM_ADD_Active = new Label();
            panel_UM_Edit_Active = new Panel();
            pictureBox_UM_Edit_User = new PictureBox();
            panel_UM_Edit_NAME = new Panel();
            panel_UM_Edit_ID = new Panel();
            panel_UM_Edit_PW = new Panel();
            panel_UM_Edit_VerifyPW = new Panel();
            kComboBox_UM_Edit_Role = new ComponentFactory.Krypton.Toolkit.KryptonComboBox();
            label_UM_Edit_AddUser = new Label();
            btn_UM_Edit_cancel = new Button();
            btn_UM_Edit_EditUser = new Button();
            label_UM_Edit_Role = new Label();
            panel_UM_Edit_Role = new Panel();
            label_UM_Edit_VerifyPW = new Label();
            textbox_UM_Edit_VerifyPW = new TextBox();
            label_UM_Edit_PW = new Label();
            textbox_UM_Edit_PW = new TextBox();
            label_UM_Edit_Name = new Label();
            textbox_UM_Edit_Name = new TextBox();
            label_UM_Edit_ID = new Label();
            textbox_UM_Edit_ID = new TextBox();
            button1 = new Button();
            cardEditUserBackground.SuspendLayout();
            cardEditUser.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox_UM_Edit_User).BeginInit();
            ((System.ComponentModel.ISupportInitialize)kComboBox_UM_Edit_Role).BeginInit();
            SuspendLayout();
            // 
            // cardEditUserBackground
            // 
            cardEditUserBackground.BackColor = Color.FromArgb(64, 64, 64);
            cardEditUserBackground.Controls.Add(cardEditUser);
            cardEditUserBackground.Dock = DockStyle.Fill;
            cardEditUserBackground.Location = new Point(0, 0);
            cardEditUserBackground.Name = "cardEditUserBackground";
            cardEditUserBackground.Size = new Size(1084, 861);
            cardEditUserBackground.TabIndex = 2;
            // 
            // cardEditUser
            // 
            cardEditUser.BackColor = Color.FromArgb(44, 44, 44);
            cardEditUser.Controls.Add(button1);
            cardEditUser.Controls.Add(btn_UM_Edit_PW);
            cardEditUser.Controls.Add(CB_UM_Edit_Active_True);
            cardEditUser.Controls.Add(label_UM_ADD_Active);
            cardEditUser.Controls.Add(panel_UM_Edit_Active);
            cardEditUser.Controls.Add(pictureBox_UM_Edit_User);
            cardEditUser.Controls.Add(panel_UM_Edit_NAME);
            cardEditUser.Controls.Add(panel_UM_Edit_ID);
            cardEditUser.Controls.Add(panel_UM_Edit_PW);
            cardEditUser.Controls.Add(panel_UM_Edit_VerifyPW);
            cardEditUser.Controls.Add(kComboBox_UM_Edit_Role);
            cardEditUser.Controls.Add(label_UM_Edit_AddUser);
            cardEditUser.Controls.Add(btn_UM_Edit_cancel);
            cardEditUser.Controls.Add(btn_UM_Edit_EditUser);
            cardEditUser.Controls.Add(label_UM_Edit_Role);
            cardEditUser.Controls.Add(panel_UM_Edit_Role);
            cardEditUser.Controls.Add(label_UM_Edit_VerifyPW);
            cardEditUser.Controls.Add(textbox_UM_Edit_VerifyPW);
            cardEditUser.Controls.Add(label_UM_Edit_PW);
            cardEditUser.Controls.Add(textbox_UM_Edit_PW);
            cardEditUser.Controls.Add(label_UM_Edit_Name);
            cardEditUser.Controls.Add(textbox_UM_Edit_Name);
            cardEditUser.Controls.Add(label_UM_Edit_ID);
            cardEditUser.Controls.Add(textbox_UM_Edit_ID);
            cardEditUser.Location = new Point(240, 60);
            cardEditUser.Name = "cardEditUser";
            cardEditUser.Size = new Size(665, 740);
            cardEditUser.TabIndex = 5;
            // 
            // btn_UM_Edit_PW
            // 
            btn_UM_Edit_PW.BackColor = Color.FromArgb(44, 44, 44);
            btn_UM_Edit_PW.BackgroundImage = (Image)resources.GetObject("btn_UM_Edit_PW.BackgroundImage");
            btn_UM_Edit_PW.FlatAppearance.BorderSize = 0;
            btn_UM_Edit_PW.FlatStyle = FlatStyle.Flat;
            btn_UM_Edit_PW.Font = new Font("Arial", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_UM_Edit_PW.ForeColor = Color.White;
            btn_UM_Edit_PW.Location = new Point(476, 342);
            btn_UM_Edit_PW.Name = "btn_UM_Edit_PW";
            btn_UM_Edit_PW.Size = new Size(100, 50);
            btn_UM_Edit_PW.TabIndex = 35;
            btn_UM_Edit_PW.UseVisualStyleBackColor = false;
            btn_UM_Edit_PW.Click += btn_UM_Edit_PW_Click;
            // 
            // CB_UM_Edit_Active_True
            // 
            CB_UM_Edit_Active_True.AutoSize = true;
            CB_UM_Edit_Active_True.Font = new Font("맑은 고딕", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            CB_UM_Edit_Active_True.ForeColor = Color.White;
            CB_UM_Edit_Active_True.Location = new Point(223, 511);
            CB_UM_Edit_Active_True.Name = "CB_UM_Edit_Active_True";
            CB_UM_Edit_Active_True.Size = new Size(117, 41);
            CB_UM_Edit_Active_True.TabIndex = 34;
            CB_UM_Edit_Active_True.Text = "활성화";
            CB_UM_Edit_Active_True.UseVisualStyleBackColor = true;
            // 
            // label_UM_ADD_Active
            // 
            label_UM_ADD_Active.AutoSize = true;
            label_UM_ADD_Active.Font = new Font("맑은 고딕", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            label_UM_ADD_Active.ForeColor = SystemColors.Window;
            label_UM_ADD_Active.Location = new Point(76, 511);
            label_UM_ADD_Active.Name = "label_UM_ADD_Active";
            label_UM_ADD_Active.Size = new Size(71, 37);
            label_UM_ADD_Active.TabIndex = 33;
            label_UM_ADD_Active.Text = "상태";
            // 
            // panel_UM_Edit_Active
            // 
            panel_UM_Edit_Active.BackColor = Color.Silver;
            panel_UM_Edit_Active.ForeColor = Color.Silver;
            panel_UM_Edit_Active.Location = new Point(76, 566);
            panel_UM_Edit_Active.Name = "panel_UM_Edit_Active";
            panel_UM_Edit_Active.Size = new Size(500, 4);
            panel_UM_Edit_Active.TabIndex = 32;
            // 
            // pictureBox_UM_Edit_User
            // 
            pictureBox_UM_Edit_User.Image = (Image)resources.GetObject("pictureBox_UM_Edit_User.Image");
            pictureBox_UM_Edit_User.Location = new Point(283, 14);
            pictureBox_UM_Edit_User.Name = "pictureBox_UM_Edit_User";
            pictureBox_UM_Edit_User.Size = new Size(100, 97);
            pictureBox_UM_Edit_User.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox_UM_Edit_User.TabIndex = 31;
            pictureBox_UM_Edit_User.TabStop = false;
            // 
            // panel_UM_Edit_NAME
            // 
            panel_UM_Edit_NAME.BackColor = Color.Silver;
            panel_UM_Edit_NAME.ForeColor = Color.Silver;
            panel_UM_Edit_NAME.Location = new Point(76, 238);
            panel_UM_Edit_NAME.Name = "panel_UM_Edit_NAME";
            panel_UM_Edit_NAME.Size = new Size(500, 4);
            panel_UM_Edit_NAME.TabIndex = 30;
            // 
            // panel_UM_Edit_ID
            // 
            panel_UM_Edit_ID.BackColor = Color.Silver;
            panel_UM_Edit_ID.ForeColor = Color.Silver;
            panel_UM_Edit_ID.Location = new Point(78, 320);
            panel_UM_Edit_ID.Name = "panel_UM_Edit_ID";
            panel_UM_Edit_ID.Size = new Size(500, 4);
            panel_UM_Edit_ID.TabIndex = 29;
            // 
            // panel_UM_Edit_PW
            // 
            panel_UM_Edit_PW.BackColor = Color.Silver;
            panel_UM_Edit_PW.ForeColor = Color.Silver;
            panel_UM_Edit_PW.Location = new Point(77, 402);
            panel_UM_Edit_PW.Name = "panel_UM_Edit_PW";
            panel_UM_Edit_PW.Size = new Size(500, 4);
            panel_UM_Edit_PW.TabIndex = 28;
            // 
            // panel_UM_Edit_VerifyPW
            // 
            panel_UM_Edit_VerifyPW.BackColor = Color.Silver;
            panel_UM_Edit_VerifyPW.ForeColor = Color.Silver;
            panel_UM_Edit_VerifyPW.Location = new Point(77, 484);
            panel_UM_Edit_VerifyPW.Name = "panel_UM_Edit_VerifyPW";
            panel_UM_Edit_VerifyPW.Size = new Size(500, 4);
            panel_UM_Edit_VerifyPW.TabIndex = 27;
            // 
            // kComboBox_UM_Edit_Role
            // 
            kComboBox_UM_Edit_Role.DropDownWidth = 200;
            kComboBox_UM_Edit_Role.Items.AddRange(new object[] { "admin", "operator", "viewer" });
            kComboBox_UM_Edit_Role.Location = new Point(223, 595);
            kComboBox_UM_Edit_Role.Name = "kComboBox_UM_Edit_Role";
            kComboBox_UM_Edit_Role.Size = new Size(340, 41);
            kComboBox_UM_Edit_Role.StateCommon.ComboBox.Back.Color1 = Color.FromArgb(44, 44, 44);
            kComboBox_UM_Edit_Role.StateCommon.ComboBox.Border.Color1 = Color.FromArgb(44, 44, 44);
            kComboBox_UM_Edit_Role.StateCommon.ComboBox.Border.Color2 = Color.FromArgb(44, 44, 44);
            kComboBox_UM_Edit_Role.StateCommon.ComboBox.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            kComboBox_UM_Edit_Role.StateCommon.ComboBox.Content.Color1 = Color.White;
            kComboBox_UM_Edit_Role.StateCommon.ComboBox.Content.Font = new Font("맑은 고딕", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            kComboBox_UM_Edit_Role.TabIndex = 30;
            // 
            // label_UM_Edit_AddUser
            // 
            label_UM_Edit_AddUser.AutoSize = true;
            label_UM_Edit_AddUser.Font = new Font("Arial", 36F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_UM_Edit_AddUser.ForeColor = SystemColors.Window;
            label_UM_Edit_AddUser.Location = new Point(232, 103);
            label_UM_Edit_AddUser.Name = "label_UM_Edit_AddUser";
            label_UM_Edit_AddUser.Size = new Size(221, 55);
            label_UM_Edit_AddUser.TabIndex = 18;
            label_UM_Edit_AddUser.Text = "Edit User";
            // 
            // btn_UM_Edit_cancel
            // 
            btn_UM_Edit_cancel.BackColor = Color.FromArgb(44, 44, 44);
            btn_UM_Edit_cancel.FlatAppearance.BorderSize = 0;
            btn_UM_Edit_cancel.FlatStyle = FlatStyle.Flat;
            btn_UM_Edit_cancel.Font = new Font("Arial", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_UM_Edit_cancel.ForeColor = Color.White;
            btn_UM_Edit_cancel.Location = new Point(550, 671);
            btn_UM_Edit_cancel.Name = "btn_UM_Edit_cancel";
            btn_UM_Edit_cancel.Size = new Size(100, 50);
            btn_UM_Edit_cancel.TabIndex = 29;
            btn_UM_Edit_cancel.Text = "취소";
            btn_UM_Edit_cancel.UseVisualStyleBackColor = false;
            btn_UM_Edit_cancel.Click += btn_UM_Edit_cancel_Click;
            // 
            // btn_UM_Edit_EditUser
            // 
            btn_UM_Edit_EditUser.BackColor = Color.FromArgb(44, 44, 44);
            btn_UM_Edit_EditUser.FlatAppearance.BorderSize = 0;
            btn_UM_Edit_EditUser.FlatStyle = FlatStyle.Flat;
            btn_UM_Edit_EditUser.Font = new Font("Arial", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_UM_Edit_EditUser.ForeColor = Color.White;
            btn_UM_Edit_EditUser.Location = new Point(433, 671);
            btn_UM_Edit_EditUser.Name = "btn_UM_Edit_EditUser";
            btn_UM_Edit_EditUser.Size = new Size(100, 50);
            btn_UM_Edit_EditUser.TabIndex = 28;
            btn_UM_Edit_EditUser.Text = "수정";
            btn_UM_Edit_EditUser.UseVisualStyleBackColor = false;
            btn_UM_Edit_EditUser.Click += btn_UM_Edit_EditUser_Click;
            // 
            // label_UM_Edit_Role
            // 
            label_UM_Edit_Role.AutoSize = true;
            label_UM_Edit_Role.Font = new Font("맑은 고딕", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            label_UM_Edit_Role.ForeColor = SystemColors.Window;
            label_UM_Edit_Role.Location = new Point(76, 590);
            label_UM_Edit_Role.Name = "label_UM_Edit_Role";
            label_UM_Edit_Role.Size = new Size(71, 37);
            label_UM_Edit_Role.TabIndex = 27;
            label_UM_Edit_Role.Text = "권한";
            // 
            // panel_UM_Edit_Role
            // 
            panel_UM_Edit_Role.BackColor = Color.Silver;
            panel_UM_Edit_Role.ForeColor = Color.Silver;
            panel_UM_Edit_Role.Location = new Point(76, 648);
            panel_UM_Edit_Role.Name = "panel_UM_Edit_Role";
            panel_UM_Edit_Role.Size = new Size(500, 4);
            panel_UM_Edit_Role.TabIndex = 26;
            // 
            // label_UM_Edit_VerifyPW
            // 
            label_UM_Edit_VerifyPW.AutoSize = true;
            label_UM_Edit_VerifyPW.Font = new Font("맑은 고딕", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            label_UM_Edit_VerifyPW.ForeColor = SystemColors.Window;
            label_UM_Edit_VerifyPW.Location = new Point(76, 427);
            label_UM_Edit_VerifyPW.Name = "label_UM_Edit_VerifyPW";
            label_UM_Edit_VerifyPW.Size = new Size(121, 37);
            label_UM_Edit_VerifyPW.TabIndex = 24;
            label_UM_Edit_VerifyPW.Text = "PW 확인";
            // 
            // textbox_UM_Edit_VerifyPW
            // 
            textbox_UM_Edit_VerifyPW.BackColor = Color.FromArgb(44, 44, 44);
            textbox_UM_Edit_VerifyPW.BorderStyle = BorderStyle.None;
            textbox_UM_Edit_VerifyPW.Font = new Font("Arial", 24F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textbox_UM_Edit_VerifyPW.ForeColor = SystemColors.Window;
            textbox_UM_Edit_VerifyPW.Location = new Point(224, 444);
            textbox_UM_Edit_VerifyPW.Name = "textbox_UM_Edit_VerifyPW";
            textbox_UM_Edit_VerifyPW.Size = new Size(229, 37);
            textbox_UM_Edit_VerifyPW.TabIndex = 22;
            // 
            // label_UM_Edit_PW
            // 
            label_UM_Edit_PW.AutoSize = true;
            label_UM_Edit_PW.Font = new Font("Arial", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_UM_Edit_PW.ForeColor = SystemColors.Window;
            label_UM_Edit_PW.Location = new Point(76, 348);
            label_UM_Edit_PW.Name = "label_UM_Edit_PW";
            label_UM_Edit_PW.Size = new Size(59, 32);
            label_UM_Edit_PW.TabIndex = 21;
            label_UM_Edit_PW.Text = "PW";
            // 
            // textbox_UM_Edit_PW
            // 
            textbox_UM_Edit_PW.BackColor = Color.FromArgb(44, 44, 44);
            textbox_UM_Edit_PW.BorderStyle = BorderStyle.None;
            textbox_UM_Edit_PW.Font = new Font("Arial", 24F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textbox_UM_Edit_PW.ForeColor = SystemColors.Window;
            textbox_UM_Edit_PW.Location = new Point(224, 362);
            textbox_UM_Edit_PW.Name = "textbox_UM_Edit_PW";
            textbox_UM_Edit_PW.Size = new Size(229, 37);
            textbox_UM_Edit_PW.TabIndex = 19;
            // 
            // label_UM_Edit_Name
            // 
            label_UM_Edit_Name.AutoSize = true;
            label_UM_Edit_Name.Font = new Font("맑은 고딕", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_UM_Edit_Name.ForeColor = SystemColors.Window;
            label_UM_Edit_Name.Location = new Point(76, 264);
            label_UM_Edit_Name.Name = "label_UM_Edit_Name";
            label_UM_Edit_Name.Size = new Size(71, 37);
            label_UM_Edit_Name.TabIndex = 18;
            label_UM_Edit_Name.Text = "이름";
            // 
            // textbox_UM_Edit_Name
            // 
            textbox_UM_Edit_Name.BackColor = Color.FromArgb(44, 44, 44);
            textbox_UM_Edit_Name.BorderStyle = BorderStyle.None;
            textbox_UM_Edit_Name.Font = new Font("Arial", 24F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textbox_UM_Edit_Name.ForeColor = SystemColors.Window;
            textbox_UM_Edit_Name.Location = new Point(224, 280);
            textbox_UM_Edit_Name.Name = "textbox_UM_Edit_Name";
            textbox_UM_Edit_Name.Size = new Size(340, 37);
            textbox_UM_Edit_Name.TabIndex = 16;
            // 
            // label_UM_Edit_ID
            // 
            label_UM_Edit_ID.AutoSize = true;
            label_UM_Edit_ID.Font = new Font("Arial", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_UM_Edit_ID.ForeColor = SystemColors.Window;
            label_UM_Edit_ID.Location = new Point(76, 185);
            label_UM_Edit_ID.Name = "label_UM_Edit_ID";
            label_UM_Edit_ID.Size = new Size(42, 32);
            label_UM_Edit_ID.TabIndex = 15;
            label_UM_Edit_ID.Text = "ID";
            // 
            // textbox_UM_Edit_ID
            // 
            textbox_UM_Edit_ID.BackColor = Color.FromArgb(44, 44, 44);
            textbox_UM_Edit_ID.BorderStyle = BorderStyle.None;
            textbox_UM_Edit_ID.Font = new Font("Arial", 24F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textbox_UM_Edit_ID.ForeColor = SystemColors.Window;
            textbox_UM_Edit_ID.Location = new Point(224, 198);
            textbox_UM_Edit_ID.Name = "textbox_UM_Edit_ID";
            textbox_UM_Edit_ID.Size = new Size(340, 37);
            textbox_UM_Edit_ID.TabIndex = 13;
            // 
            // button1
            // 
            button1.BackColor = Color.FromArgb(44, 44, 44);
            button1.BackgroundImage = (Image)resources.GetObject("button1.BackgroundImage");
            button1.FlatAppearance.BorderSize = 0;
            button1.FlatStyle = FlatStyle.Flat;
            button1.Font = new Font("Arial", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            button1.ForeColor = Color.White;
            button1.Location = new Point(476, 426);
            button1.Name = "button1";
            button1.Size = new Size(100, 50);
            button1.TabIndex = 36;
            button1.UseVisualStyleBackColor = false;
            // 
            // UserManagementForm_EditUser
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1084, 861);
            Controls.Add(cardEditUserBackground);
            Name = "UserManagementForm_EditUser";
            Text = "UserManagementForm_EditUser";
            cardEditUserBackground.ResumeLayout(false);
            cardEditUser.ResumeLayout(false);
            cardEditUser.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox_UM_Edit_User).EndInit();
            ((System.ComponentModel.ISupportInitialize)kComboBox_UM_Edit_Role).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel cardEditUserBackground;
        private Panel cardEditUser;
        private PictureBox pictureBox_UM_Edit_User;
        private Panel panel_UM_Edit_NAME;
        private Panel panel_UM_Edit_ID;
        private Panel panel_UM_Edit_PW;
        private Panel panel_UM_Edit_VerifyPW;
        private ComponentFactory.Krypton.Toolkit.KryptonComboBox kComboBox_UM_Edit_Role;
        private Label label_UM_Edit_AddUser;
        private Button btn_UM_Edit_cancel;
        private Button btn_UM_Edit_EditUser;
        private Label label_UM_Edit_Role;
        private Panel panel_UM_Edit_Role;
        private Label label_UM_Edit_VerifyPW;
        private TextBox textbox_UM_Edit_VerifyPW;
        private Label label_UM_Edit_PW;
        private TextBox textbox_UM_Edit_PW;
        private Label label_UM_Edit_Name;
        private TextBox textbox_UM_Edit_Name;
        private Label label_UM_Edit_ID;
        private TextBox textbox_UM_Edit_ID;
        private Panel panel_UM_Edit_Active;
        private CheckBox CB_UM_Edit_Active_True;
        private Label label_UM_ADD_Active;
        private Button btn_UM_Edit_PW;
        private Button button1;
    }
}