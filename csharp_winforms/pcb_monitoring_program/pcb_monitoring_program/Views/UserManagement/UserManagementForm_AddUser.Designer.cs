namespace pcb_monitoring_program.Views.UserManagement
{
    partial class UserManagementForm_AddUser
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserManagementForm_AddUser));
            cardAddUserBackground = new Panel();
            cardAddUser = new Panel();
            btn_UM_Add_ID_Check = new Button();
            btn_UM_Add_PW = new Button();
            panel1 = new Panel();
            label_UM_ADD_Name = new Label();
            textbox_UM_ADD_Name = new TextBox();
            CB_UM_ADD_Active_True = new CheckBox();
            pictureBox_UM_ADD_User = new PictureBox();
            panel_UM_ADD_NAME = new Panel();
            panel_UM_ADD_ID = new Panel();
            panel_UM_ADD_PW = new Panel();
            panel_UM_ADD_Active = new Panel();
            kComboBox_UM_ADD_Role = new ComponentFactory.Krypton.Toolkit.KryptonComboBox();
            label_UM_ADD_AddUser = new Label();
            btn_UM_ADD_cancel = new Button();
            btn_UM_ADD_AddUser = new Button();
            label_UM_ADD_Role = new Label();
            panel_UM_ADD_Role = new Panel();
            label_UM_ADD_VerifyPW = new Label();
            textbox_UM_ADD_VerifyPW = new TextBox();
            label_UM_ADD_PW = new Label();
            textbox_UM_ADD_PW = new TextBox();
            label_UM_ADD_ID = new Label();
            textbox_UM_ADD_ID = new TextBox();
            label_UM_ADD_Active = new Label();
            cardAddUserBackground.SuspendLayout();
            cardAddUser.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox_UM_ADD_User).BeginInit();
            ((System.ComponentModel.ISupportInitialize)kComboBox_UM_ADD_Role).BeginInit();
            SuspendLayout();
            // 
            // cardAddUserBackground
            // 
            cardAddUserBackground.BackColor = Color.FromArgb(64, 64, 64);
            cardAddUserBackground.Controls.Add(cardAddUser);
            cardAddUserBackground.Dock = DockStyle.Fill;
            cardAddUserBackground.Location = new Point(0, 0);
            cardAddUserBackground.Name = "cardAddUserBackground";
            cardAddUserBackground.Size = new Size(1084, 861);
            cardAddUserBackground.TabIndex = 1;
            // 
            // cardAddUser
            // 
            cardAddUser.BackColor = Color.FromArgb(44, 44, 44);
            cardAddUser.Controls.Add(btn_UM_Add_ID_Check);
            cardAddUser.Controls.Add(btn_UM_Add_PW);
            cardAddUser.Controls.Add(panel1);
            cardAddUser.Controls.Add(label_UM_ADD_Name);
            cardAddUser.Controls.Add(textbox_UM_ADD_Name);
            cardAddUser.Controls.Add(CB_UM_ADD_Active_True);
            cardAddUser.Controls.Add(pictureBox_UM_ADD_User);
            cardAddUser.Controls.Add(panel_UM_ADD_NAME);
            cardAddUser.Controls.Add(panel_UM_ADD_ID);
            cardAddUser.Controls.Add(panel_UM_ADD_PW);
            cardAddUser.Controls.Add(panel_UM_ADD_Active);
            cardAddUser.Controls.Add(kComboBox_UM_ADD_Role);
            cardAddUser.Controls.Add(label_UM_ADD_AddUser);
            cardAddUser.Controls.Add(btn_UM_ADD_cancel);
            cardAddUser.Controls.Add(btn_UM_ADD_AddUser);
            cardAddUser.Controls.Add(label_UM_ADD_Role);
            cardAddUser.Controls.Add(panel_UM_ADD_Role);
            cardAddUser.Controls.Add(label_UM_ADD_VerifyPW);
            cardAddUser.Controls.Add(textbox_UM_ADD_VerifyPW);
            cardAddUser.Controls.Add(label_UM_ADD_PW);
            cardAddUser.Controls.Add(textbox_UM_ADD_PW);
            cardAddUser.Controls.Add(label_UM_ADD_ID);
            cardAddUser.Controls.Add(textbox_UM_ADD_ID);
            cardAddUser.Controls.Add(label_UM_ADD_Active);
            cardAddUser.Location = new Point(240, 60);
            cardAddUser.Name = "cardAddUser";
            cardAddUser.Size = new Size(665, 740);
            cardAddUser.TabIndex = 5;
            // 
            // btn_UM_Add_ID_Check
            // 
            btn_UM_Add_ID_Check.BackColor = Color.FromArgb(44, 44, 44);
            btn_UM_Add_ID_Check.FlatAppearance.BorderSize = 0;
            btn_UM_Add_ID_Check.FlatStyle = FlatStyle.Flat;
            btn_UM_Add_ID_Check.Font = new Font("Arial", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_UM_Add_ID_Check.ForeColor = Color.White;
            btn_UM_Add_ID_Check.Location = new Point(475, 185);
            btn_UM_Add_ID_Check.Name = "btn_UM_Add_ID_Check";
            btn_UM_Add_ID_Check.Size = new Size(100, 50);
            btn_UM_Add_ID_Check.TabIndex = 1;
            btn_UM_Add_ID_Check.Text = "중복 확인";
            btn_UM_Add_ID_Check.UseVisualStyleBackColor = false;
            btn_UM_Add_ID_Check.Click += btn_UM_Add_ID_Check_Click;
            // 
            // btn_UM_Add_PW
            // 
            btn_UM_Add_PW.BackColor = Color.FromArgb(44, 44, 44);
            btn_UM_Add_PW.BackgroundImage = (Image)resources.GetObject("btn_UM_Add_PW.BackgroundImage");
            btn_UM_Add_PW.FlatAppearance.BorderSize = 0;
            btn_UM_Add_PW.FlatStyle = FlatStyle.Flat;
            btn_UM_Add_PW.Font = new Font("Arial", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_UM_Add_PW.ForeColor = Color.White;
            btn_UM_Add_PW.Location = new Point(476, 342);
            btn_UM_Add_PW.Name = "btn_UM_Add_PW";
            btn_UM_Add_PW.Size = new Size(100, 50);
            btn_UM_Add_PW.TabIndex = 7;
            btn_UM_Add_PW.UseVisualStyleBackColor = false;
            btn_UM_Add_PW.Click += btn_UM_Add_PW_Click;
            // 
            // panel1
            // 
            panel1.BackColor = Color.Silver;
            panel1.ForeColor = Color.Silver;
            panel1.Location = new Point(78, 320);
            panel1.Name = "panel1";
            panel1.Size = new Size(500, 4);
            panel1.TabIndex = 35;
            // 
            // label_UM_ADD_Name
            // 
            label_UM_ADD_Name.AutoSize = true;
            label_UM_ADD_Name.Font = new Font("맑은 고딕", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_UM_ADD_Name.ForeColor = SystemColors.Window;
            label_UM_ADD_Name.Location = new Point(76, 264);
            label_UM_ADD_Name.Name = "label_UM_ADD_Name";
            label_UM_ADD_Name.Size = new Size(71, 37);
            label_UM_ADD_Name.TabIndex = 34;
            label_UM_ADD_Name.Text = "이름";
            // 
            // textbox_UM_ADD_Name
            // 
            textbox_UM_ADD_Name.BackColor = Color.FromArgb(44, 44, 44);
            textbox_UM_ADD_Name.BorderStyle = BorderStyle.None;
            textbox_UM_ADD_Name.Font = new Font("Arial", 24F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textbox_UM_ADD_Name.ForeColor = SystemColors.Window;
            textbox_UM_ADD_Name.Location = new Point(224, 280);
            textbox_UM_ADD_Name.Name = "textbox_UM_ADD_Name";
            textbox_UM_ADD_Name.Size = new Size(340, 37);
            textbox_UM_ADD_Name.TabIndex = 2;
            // 
            // CB_UM_ADD_Active_True
            // 
            CB_UM_ADD_Active_True.AutoSize = true;
            CB_UM_ADD_Active_True.Font = new Font("맑은 고딕", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            CB_UM_ADD_Active_True.ForeColor = Color.White;
            CB_UM_ADD_Active_True.Location = new Point(223, 511);
            CB_UM_ADD_Active_True.Name = "CB_UM_ADD_Active_True";
            CB_UM_ADD_Active_True.Size = new Size(117, 41);
            CB_UM_ADD_Active_True.TabIndex = 5;
            CB_UM_ADD_Active_True.Text = "활성화";
            CB_UM_ADD_Active_True.UseVisualStyleBackColor = true;
            // 
            // pictureBox_UM_ADD_User
            // 
            pictureBox_UM_ADD_User.Image = (Image)resources.GetObject("pictureBox_UM_ADD_User.Image");
            pictureBox_UM_ADD_User.Location = new Point(283, 14);
            pictureBox_UM_ADD_User.Name = "pictureBox_UM_ADD_User";
            pictureBox_UM_ADD_User.Size = new Size(100, 97);
            pictureBox_UM_ADD_User.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox_UM_ADD_User.TabIndex = 31;
            pictureBox_UM_ADD_User.TabStop = false;
            // 
            // panel_UM_ADD_NAME
            // 
            panel_UM_ADD_NAME.BackColor = Color.Silver;
            panel_UM_ADD_NAME.ForeColor = Color.Silver;
            panel_UM_ADD_NAME.Location = new Point(76, 238);
            panel_UM_ADD_NAME.Name = "panel_UM_ADD_NAME";
            panel_UM_ADD_NAME.Size = new Size(500, 4);
            panel_UM_ADD_NAME.TabIndex = 30;
            // 
            // panel_UM_ADD_ID
            // 
            panel_UM_ADD_ID.BackColor = Color.Silver;
            panel_UM_ADD_ID.ForeColor = Color.Silver;
            panel_UM_ADD_ID.Location = new Point(77, 402);
            panel_UM_ADD_ID.Name = "panel_UM_ADD_ID";
            panel_UM_ADD_ID.Size = new Size(500, 4);
            panel_UM_ADD_ID.TabIndex = 29;
            // 
            // panel_UM_ADD_PW
            // 
            panel_UM_ADD_PW.BackColor = Color.Silver;
            panel_UM_ADD_PW.ForeColor = Color.Silver;
            panel_UM_ADD_PW.Location = new Point(77, 484);
            panel_UM_ADD_PW.Name = "panel_UM_ADD_PW";
            panel_UM_ADD_PW.Size = new Size(500, 4);
            panel_UM_ADD_PW.TabIndex = 28;
            // 
            // panel_UM_ADD_Active
            // 
            panel_UM_ADD_Active.BackColor = Color.Silver;
            panel_UM_ADD_Active.ForeColor = Color.Silver;
            panel_UM_ADD_Active.Location = new Point(76, 566);
            panel_UM_ADD_Active.Name = "panel_UM_ADD_Active";
            panel_UM_ADD_Active.Size = new Size(500, 4);
            panel_UM_ADD_Active.TabIndex = 27;
            // 
            // kComboBox_UM_ADD_Role
            // 
            kComboBox_UM_ADD_Role.DropDownWidth = 200;
            kComboBox_UM_ADD_Role.Items.AddRange(new object[] { "admin", "operator", "viewer" });
            kComboBox_UM_ADD_Role.Location = new Point(223, 595);
            kComboBox_UM_ADD_Role.Name = "kComboBox_UM_ADD_Role";
            kComboBox_UM_ADD_Role.Size = new Size(340, 41);
            kComboBox_UM_ADD_Role.StateCommon.ComboBox.Back.Color1 = Color.FromArgb(44, 44, 44);
            kComboBox_UM_ADD_Role.StateCommon.ComboBox.Border.Color1 = Color.FromArgb(44, 44, 44);
            kComboBox_UM_ADD_Role.StateCommon.ComboBox.Border.Color2 = Color.FromArgb(44, 44, 44);
            kComboBox_UM_ADD_Role.StateCommon.ComboBox.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            kComboBox_UM_ADD_Role.StateCommon.ComboBox.Content.Color1 = Color.White;
            kComboBox_UM_ADD_Role.StateCommon.ComboBox.Content.Font = new Font("맑은 고딕", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            kComboBox_UM_ADD_Role.TabIndex = 6;
            // 
            // label_UM_ADD_AddUser
            // 
            label_UM_ADD_AddUser.AutoSize = true;
            label_UM_ADD_AddUser.Font = new Font("Arial", 36F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_UM_ADD_AddUser.ForeColor = SystemColors.Window;
            label_UM_ADD_AddUser.Location = new Point(232, 103);
            label_UM_ADD_AddUser.Name = "label_UM_ADD_AddUser";
            label_UM_ADD_AddUser.Size = new Size(225, 55);
            label_UM_ADD_AddUser.TabIndex = 18;
            label_UM_ADD_AddUser.Text = "Add User";
            // 
            // btn_UM_ADD_cancel
            // 
            btn_UM_ADD_cancel.BackColor = Color.FromArgb(44, 44, 44);
            btn_UM_ADD_cancel.FlatAppearance.BorderSize = 0;
            btn_UM_ADD_cancel.FlatStyle = FlatStyle.Flat;
            btn_UM_ADD_cancel.Font = new Font("Arial", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_UM_ADD_cancel.ForeColor = Color.White;
            btn_UM_ADD_cancel.Location = new Point(550, 671);
            btn_UM_ADD_cancel.Name = "btn_UM_ADD_cancel";
            btn_UM_ADD_cancel.Size = new Size(100, 50);
            btn_UM_ADD_cancel.TabIndex = 9;
            btn_UM_ADD_cancel.Text = "취소";
            btn_UM_ADD_cancel.UseVisualStyleBackColor = false;
            btn_UM_ADD_cancel.Click += btn_UserManage_cancel_Click;
            // 
            // btn_UM_ADD_AddUser
            // 
            btn_UM_ADD_AddUser.BackColor = Color.FromArgb(44, 44, 44);
            btn_UM_ADD_AddUser.FlatAppearance.BorderSize = 0;
            btn_UM_ADD_AddUser.FlatStyle = FlatStyle.Flat;
            btn_UM_ADD_AddUser.Font = new Font("Arial", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_UM_ADD_AddUser.ForeColor = Color.White;
            btn_UM_ADD_AddUser.Location = new Point(433, 671);
            btn_UM_ADD_AddUser.Name = "btn_UM_ADD_AddUser";
            btn_UM_ADD_AddUser.Size = new Size(100, 50);
            btn_UM_ADD_AddUser.TabIndex = 8;
            btn_UM_ADD_AddUser.Text = "추가";
            btn_UM_ADD_AddUser.UseVisualStyleBackColor = false;
            btn_UM_ADD_AddUser.Click += btn_UserManage_AddUser_Click;
            // 
            // label_UM_ADD_Role
            // 
            label_UM_ADD_Role.AutoSize = true;
            label_UM_ADD_Role.Font = new Font("맑은 고딕", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            label_UM_ADD_Role.ForeColor = SystemColors.Window;
            label_UM_ADD_Role.Location = new Point(76, 590);
            label_UM_ADD_Role.Name = "label_UM_ADD_Role";
            label_UM_ADD_Role.Size = new Size(71, 37);
            label_UM_ADD_Role.TabIndex = 27;
            label_UM_ADD_Role.Text = "권한";
            // 
            // panel_UM_ADD_Role
            // 
            panel_UM_ADD_Role.BackColor = Color.Silver;
            panel_UM_ADD_Role.ForeColor = Color.Silver;
            panel_UM_ADD_Role.Location = new Point(76, 648);
            panel_UM_ADD_Role.Name = "panel_UM_ADD_Role";
            panel_UM_ADD_Role.Size = new Size(500, 4);
            panel_UM_ADD_Role.TabIndex = 26;
            // 
            // label_UM_ADD_VerifyPW
            // 
            label_UM_ADD_VerifyPW.AutoSize = true;
            label_UM_ADD_VerifyPW.Font = new Font("맑은 고딕", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            label_UM_ADD_VerifyPW.ForeColor = SystemColors.Window;
            label_UM_ADD_VerifyPW.Location = new Point(76, 427);
            label_UM_ADD_VerifyPW.Name = "label_UM_ADD_VerifyPW";
            label_UM_ADD_VerifyPW.Size = new Size(121, 37);
            label_UM_ADD_VerifyPW.TabIndex = 24;
            label_UM_ADD_VerifyPW.Text = "PW 확인";
            // 
            // textbox_UM_ADD_VerifyPW
            // 
            textbox_UM_ADD_VerifyPW.BackColor = Color.FromArgb(44, 44, 44);
            textbox_UM_ADD_VerifyPW.BorderStyle = BorderStyle.None;
            textbox_UM_ADD_VerifyPW.Font = new Font("Arial", 24F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textbox_UM_ADD_VerifyPW.ForeColor = SystemColors.Window;
            textbox_UM_ADD_VerifyPW.Location = new Point(224, 444);
            textbox_UM_ADD_VerifyPW.Name = "textbox_UM_ADD_VerifyPW";
            textbox_UM_ADD_VerifyPW.Size = new Size(246, 37);
            textbox_UM_ADD_VerifyPW.TabIndex = 4;
            // 
            // label_UM_ADD_PW
            // 
            label_UM_ADD_PW.AutoSize = true;
            label_UM_ADD_PW.Font = new Font("Arial", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_UM_ADD_PW.ForeColor = SystemColors.Window;
            label_UM_ADD_PW.Location = new Point(76, 348);
            label_UM_ADD_PW.Name = "label_UM_ADD_PW";
            label_UM_ADD_PW.Size = new Size(59, 32);
            label_UM_ADD_PW.TabIndex = 21;
            label_UM_ADD_PW.Text = "PW";
            // 
            // textbox_UM_ADD_PW
            // 
            textbox_UM_ADD_PW.BackColor = Color.FromArgb(44, 44, 44);
            textbox_UM_ADD_PW.BorderStyle = BorderStyle.None;
            textbox_UM_ADD_PW.Font = new Font("Arial", 24F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textbox_UM_ADD_PW.ForeColor = SystemColors.Window;
            textbox_UM_ADD_PW.Location = new Point(224, 362);
            textbox_UM_ADD_PW.Name = "textbox_UM_ADD_PW";
            textbox_UM_ADD_PW.Size = new Size(246, 37);
            textbox_UM_ADD_PW.TabIndex = 3;
            // 
            // label_UM_ADD_ID
            // 
            label_UM_ADD_ID.AutoSize = true;
            label_UM_ADD_ID.Font = new Font("Arial", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_UM_ADD_ID.ForeColor = SystemColors.Window;
            label_UM_ADD_ID.Location = new Point(76, 185);
            label_UM_ADD_ID.Name = "label_UM_ADD_ID";
            label_UM_ADD_ID.Size = new Size(42, 32);
            label_UM_ADD_ID.TabIndex = 18;
            label_UM_ADD_ID.Text = "ID";
            // 
            // textbox_UM_ADD_ID
            // 
            textbox_UM_ADD_ID.BackColor = Color.FromArgb(44, 44, 44);
            textbox_UM_ADD_ID.BorderStyle = BorderStyle.None;
            textbox_UM_ADD_ID.Font = new Font("Arial", 24F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textbox_UM_ADD_ID.ForeColor = SystemColors.Window;
            textbox_UM_ADD_ID.Location = new Point(224, 198);
            textbox_UM_ADD_ID.Name = "textbox_UM_ADD_ID";
            textbox_UM_ADD_ID.Size = new Size(246, 37);
            textbox_UM_ADD_ID.TabIndex = 0;
            // 
            // label_UM_ADD_Active
            // 
            label_UM_ADD_Active.AutoSize = true;
            label_UM_ADD_Active.Font = new Font("맑은 고딕", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            label_UM_ADD_Active.ForeColor = SystemColors.Window;
            label_UM_ADD_Active.Location = new Point(76, 511);
            label_UM_ADD_Active.Name = "label_UM_ADD_Active";
            label_UM_ADD_Active.Size = new Size(71, 37);
            label_UM_ADD_Active.TabIndex = 15;
            label_UM_ADD_Active.Text = "상태";
            // 
            // UserManagementForm_AddUser
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1084, 861);
            Controls.Add(cardAddUserBackground);
            Name = "UserManagementForm_AddUser";
            Text = "UserManagementForm_AddUser";
            cardAddUserBackground.ResumeLayout(false);
            cardAddUser.ResumeLayout(false);
            cardAddUser.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox_UM_ADD_User).EndInit();
            ((System.ComponentModel.ISupportInitialize)kComboBox_UM_ADD_Role).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel cardAddUserBackground;
        private Panel cardAddUser;
        private Label label_UM_ADD_Role;
        private Panel panel_UM_ADD_Role;
        private Label label_UM_ADD_VerifyPW;
        private TextBox textbox_UM_ADD_VerifyPW;
        private Label label_UM_ADD_PW;
        private TextBox textbox_UM_ADD_PW;
        private Label label_UM_ADD_ID;
        private TextBox textbox_UM_ADD_ID;
        private Label label_UM_ADD_Active;
        private Button btn_UM_ADD_cancel;
        private Button btn_UM_ADD_AddUser;
        private Label label_UM_ADD_AddUser;
        private ComponentFactory.Krypton.Toolkit.KryptonComboBox kComboBox_UM_ADD_Role;
        private Panel panel_UM_ADD_NAME;
        private Panel panel_UM_ADD_ID;
        private Panel panel_UM_ADD_PW;
        private Panel panel_UM_ADD_Active;
        private PictureBox pictureBox_UM_ADD_User;
        private CheckBox CB_UM_ADD_Active_True;
        private Panel panel1;
        private Label label_UM_ADD_Name;
        private TextBox textbox_UM_ADD_Name;
        private Button btn_UM_Add_PW;
        private Button btn_UM_Add_ID_Check;
    }
}