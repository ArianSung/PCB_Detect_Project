namespace pcb_monitoring_program.Views.UserManagement
{
    partial class UserManagementView
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
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            card_userManagement = new Panel();
            btn_UserManage_Search = new Button();
            kComboBox_UM_Role = new ComponentFactory.Krypton.Toolkit.KryptonComboBox();
            label_UM_Role = new Label();
            panel_UM_Role = new Panel();
            btn_UserManage_Refresh = new Button();
            btn_UserManage_ResetPW = new Button();
            btn_UserManage_DeleteUser = new Button();
            btn_UserManage_EditUser = new Button();
            btn_UserManage_AddUser = new Button();
            DGV_UserManagement = new ComponentFactory.Krypton.Toolkit.KryptonDataGridView();
            label_UM_ID = new Label();
            TextBox_UM_ID = new TextBox();
            panel_UM_ID = new Panel();
            card_userManagement.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)kComboBox_UM_Role).BeginInit();
            ((System.ComponentModel.ISupportInitialize)DGV_UserManagement).BeginInit();
            SuspendLayout();
            // 
            // card_userManagement
            // 
            card_userManagement.BackColor = Color.FromArgb(64, 64, 64);
            card_userManagement.Controls.Add(btn_UserManage_Search);
            card_userManagement.Controls.Add(kComboBox_UM_Role);
            card_userManagement.Controls.Add(label_UM_Role);
            card_userManagement.Controls.Add(panel_UM_Role);
            card_userManagement.Controls.Add(btn_UserManage_Refresh);
            card_userManagement.Controls.Add(btn_UserManage_ResetPW);
            card_userManagement.Controls.Add(btn_UserManage_DeleteUser);
            card_userManagement.Controls.Add(btn_UserManage_EditUser);
            card_userManagement.Controls.Add(btn_UserManage_AddUser);
            card_userManagement.Controls.Add(DGV_UserManagement);
            card_userManagement.Controls.Add(label_UM_ID);
            card_userManagement.Controls.Add(TextBox_UM_ID);
            card_userManagement.Controls.Add(panel_UM_ID);
            card_userManagement.Location = new Point(104, 81);
            card_userManagement.Name = "card_userManagement";
            card_userManagement.Size = new Size(1400, 698);
            card_userManagement.TabIndex = 0;
            // 
            // btn_UserManage_Search
            // 
            btn_UserManage_Search.Font = new Font("Arial", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_UserManage_Search.Location = new Point(924, 46);
            btn_UserManage_Search.Name = "btn_UserManage_Search";
            btn_UserManage_Search.Size = new Size(150, 50);
            btn_UserManage_Search.TabIndex = 21;
            btn_UserManage_Search.Text = "검색";
            btn_UserManage_Search.UseVisualStyleBackColor = true;
            btn_UserManage_Search.Click += btn_UserManage_Search_Click;
            // 
            // kComboBox_UM_Role
            // 
            kComboBox_UM_Role.DropDownWidth = 200;
            kComboBox_UM_Role.Items.AddRange(new object[] { "전체", "admin", "operator", "viewer" });
            kComboBox_UM_Role.Location = new Point(613, 55);
            kComboBox_UM_Role.Name = "kComboBox_UM_Role";
            kComboBox_UM_Role.Size = new Size(199, 31);
            kComboBox_UM_Role.StateCommon.ComboBox.Back.Color1 = Color.FromArgb(44, 44, 44);
            kComboBox_UM_Role.StateCommon.ComboBox.Border.Color1 = Color.FromArgb(44, 44, 44);
            kComboBox_UM_Role.StateCommon.ComboBox.Border.Color2 = Color.FromArgb(44, 44, 44);
            kComboBox_UM_Role.StateCommon.ComboBox.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            kComboBox_UM_Role.StateCommon.ComboBox.Content.Color1 = Color.White;
            kComboBox_UM_Role.StateCommon.ComboBox.Content.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            kComboBox_UM_Role.TabIndex = 20;
            kComboBox_UM_Role.Text = "전체";
            // 
            // label_UM_Role
            // 
            label_UM_Role.AutoSize = true;
            label_UM_Role.Font = new Font("Arial", 24F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_UM_Role.ForeColor = SystemColors.Window;
            label_UM_Role.Location = new Point(527, 39);
            label_UM_Role.Name = "label_UM_Role";
            label_UM_Role.Size = new Size(63, 36);
            label_UM_Role.TabIndex = 19;
            label_UM_Role.Text = "권한";
            // 
            // panel_UM_Role
            // 
            panel_UM_Role.BackColor = Color.Silver;
            panel_UM_Role.ForeColor = Color.Silver;
            panel_UM_Role.Location = new Point(529, 93);
            panel_UM_Role.Name = "panel_UM_Role";
            panel_UM_Role.Size = new Size(300, 5);
            panel_UM_Role.TabIndex = 18;
            // 
            // btn_UserManage_Refresh
            // 
            btn_UserManage_Refresh.Font = new Font("Arial", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_UserManage_Refresh.Location = new Point(1145, 46);
            btn_UserManage_Refresh.Name = "btn_UserManage_Refresh";
            btn_UserManage_Refresh.Size = new Size(150, 50);
            btn_UserManage_Refresh.TabIndex = 17;
            btn_UserManage_Refresh.Text = "새로고침";
            btn_UserManage_Refresh.UseVisualStyleBackColor = true;
            btn_UserManage_Refresh.Click += btn_UserManage_Refresh_Click;
            // 
            // btn_UserManage_ResetPW
            // 
            btn_UserManage_ResetPW.Font = new Font("Arial", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_UserManage_ResetPW.Location = new Point(1095, 585);
            btn_UserManage_ResetPW.Name = "btn_UserManage_ResetPW";
            btn_UserManage_ResetPW.Size = new Size(200, 50);
            btn_UserManage_ResetPW.TabIndex = 16;
            btn_UserManage_ResetPW.Text = "비밀번호 초기화";
            btn_UserManage_ResetPW.UseVisualStyleBackColor = true;
            btn_UserManage_ResetPW.Click += btn_UserManage_ResetPW_Click;
            // 
            // btn_UserManage_DeleteUser
            // 
            btn_UserManage_DeleteUser.Font = new Font("Arial", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_UserManage_DeleteUser.Location = new Point(775, 585);
            btn_UserManage_DeleteUser.Name = "btn_UserManage_DeleteUser";
            btn_UserManage_DeleteUser.Size = new Size(200, 50);
            btn_UserManage_DeleteUser.TabIndex = 15;
            btn_UserManage_DeleteUser.Text = "삭제";
            btn_UserManage_DeleteUser.UseVisualStyleBackColor = true;
            btn_UserManage_DeleteUser.Click += btn_UserManage_DeleteUser_Click;
            // 
            // btn_UserManage_EditUser
            // 
            btn_UserManage_EditUser.Font = new Font("Arial", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_UserManage_EditUser.Location = new Point(460, 585);
            btn_UserManage_EditUser.Name = "btn_UserManage_EditUser";
            btn_UserManage_EditUser.Size = new Size(200, 50);
            btn_UserManage_EditUser.TabIndex = 14;
            btn_UserManage_EditUser.Text = "수정";
            btn_UserManage_EditUser.UseVisualStyleBackColor = true;
            btn_UserManage_EditUser.Click += btn_UserManage_EditUser_Click;
            // 
            // btn_UserManage_AddUser
            // 
            btn_UserManage_AddUser.Font = new Font("Arial", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_UserManage_AddUser.Location = new Point(145, 585);
            btn_UserManage_AddUser.Name = "btn_UserManage_AddUser";
            btn_UserManage_AddUser.Size = new Size(200, 50);
            btn_UserManage_AddUser.TabIndex = 13;
            btn_UserManage_AddUser.Text = "사용자 추가";
            btn_UserManage_AddUser.UseVisualStyleBackColor = true;
            btn_UserManage_AddUser.Click += btn_UserManage_AddUser_Click;
            // 
            // DGV_UserManagement
            // 
            DGV_UserManagement.AllowUserToAddRows = false;
            DGV_UserManagement.AllowUserToDeleteRows = false;
            DGV_UserManagement.AllowUserToResizeColumns = false;
            DGV_UserManagement.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(44, 44, 44);
            dataGridViewCellStyle1.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            dataGridViewCellStyle1.ForeColor = Color.White;
            dataGridViewCellStyle1.SelectionBackColor = Color.FromArgb(44, 44, 44);
            dataGridViewCellStyle1.SelectionForeColor = Color.White;
            DGV_UserManagement.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            DGV_UserManagement.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            DGV_UserManagement.ClipboardCopyMode = DataGridViewClipboardCopyMode.Disable;
            DGV_UserManagement.ColumnHeadersHeight = 41;
            DGV_UserManagement.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            DGV_UserManagement.EditMode = DataGridViewEditMode.EditProgrammatically;
            DGV_UserManagement.Location = new Point(145, 130);
            DGV_UserManagement.MultiSelect = false;
            DGV_UserManagement.Name = "DGV_UserManagement";
            DGV_UserManagement.PaletteMode = ComponentFactory.Krypton.Toolkit.PaletteMode.Office2010Black;
            DGV_UserManagement.ReadOnly = true;
            DGV_UserManagement.RowHeadersVisible = false;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(44, 44, 44);
            dataGridViewCellStyle2.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            dataGridViewCellStyle2.ForeColor = Color.White;
            dataGridViewCellStyle2.SelectionBackColor = Color.FromArgb(44, 44, 44);
            dataGridViewCellStyle2.SelectionForeColor = Color.White;
            DGV_UserManagement.RowsDefaultCellStyle = dataGridViewCellStyle2;
            DGV_UserManagement.RowTemplate.DefaultCellStyle.BackColor = Color.FromArgb(44, 44, 44);
            DGV_UserManagement.RowTemplate.DefaultCellStyle.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            DGV_UserManagement.RowTemplate.DefaultCellStyle.ForeColor = Color.White;
            DGV_UserManagement.RowTemplate.DefaultCellStyle.SelectionBackColor = Color.FromArgb(44, 44, 44);
            DGV_UserManagement.RowTemplate.DefaultCellStyle.SelectionForeColor = Color.White;
            DGV_UserManagement.RowTemplate.Height = 41;
            DGV_UserManagement.ScrollBars = ScrollBars.Vertical;
            DGV_UserManagement.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DGV_UserManagement.Size = new Size(1150, 420);
            DGV_UserManagement.StateCommon.Background.Color1 = Color.FromArgb(44, 44, 44);
            DGV_UserManagement.StateCommon.Background.Color2 = Color.FromArgb(44, 44, 44);
            DGV_UserManagement.StateCommon.BackStyle = ComponentFactory.Krypton.Toolkit.PaletteBackStyle.GridBackgroundList;
            DGV_UserManagement.StateCommon.DataCell.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_UserManagement.StateCommon.DataCell.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_UserManagement.StateCommon.DataCell.Border.Color1 = Color.White;
            DGV_UserManagement.StateCommon.DataCell.Border.Color2 = Color.White;
            DGV_UserManagement.StateCommon.DataCell.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_UserManagement.StateCommon.DataCell.Content.Color1 = Color.White;
            DGV_UserManagement.StateCommon.DataCell.Content.Color2 = Color.White;
            DGV_UserManagement.StateCommon.DataCell.Content.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            DGV_UserManagement.StateCommon.DataCell.Content.TextH = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
            DGV_UserManagement.StateCommon.DataCell.Content.TextV = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
            DGV_UserManagement.StateCommon.HeaderColumn.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_UserManagement.StateCommon.HeaderColumn.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_UserManagement.StateCommon.HeaderColumn.Border.Color1 = Color.White;
            DGV_UserManagement.StateCommon.HeaderColumn.Border.Color2 = Color.White;
            DGV_UserManagement.StateCommon.HeaderColumn.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_UserManagement.StateCommon.HeaderColumn.Content.Color1 = Color.White;
            DGV_UserManagement.StateCommon.HeaderColumn.Content.Color2 = Color.White;
            DGV_UserManagement.StateCommon.HeaderColumn.Content.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            DGV_UserManagement.StateCommon.HeaderColumn.Content.TextH = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
            DGV_UserManagement.StateCommon.HeaderColumn.Content.TextV = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
            DGV_UserManagement.StateCommon.HeaderRow.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_UserManagement.StateCommon.HeaderRow.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_UserManagement.StateCommon.HeaderRow.Border.Color1 = Color.White;
            DGV_UserManagement.StateCommon.HeaderRow.Border.Color2 = Color.White;
            DGV_UserManagement.StateCommon.HeaderRow.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_UserManagement.StateCommon.HeaderRow.Content.Color1 = Color.White;
            DGV_UserManagement.StateCommon.HeaderRow.Content.Color2 = Color.White;
            DGV_UserManagement.StateCommon.HeaderRow.Content.Font = new Font("맑은 고딕", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            DGV_UserManagement.StateDisabled.Background.Color1 = Color.FromArgb(44, 44, 44);
            DGV_UserManagement.StateDisabled.Background.Color2 = Color.FromArgb(44, 44, 44);
            DGV_UserManagement.StateDisabled.DataCell.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_UserManagement.StateDisabled.DataCell.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_UserManagement.StateDisabled.DataCell.Border.Color1 = Color.White;
            DGV_UserManagement.StateDisabled.DataCell.Border.Color2 = Color.White;
            DGV_UserManagement.StateDisabled.DataCell.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_UserManagement.StateDisabled.DataCell.Content.Color1 = Color.White;
            DGV_UserManagement.StateDisabled.DataCell.Content.Color2 = Color.White;
            DGV_UserManagement.StateDisabled.HeaderColumn.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_UserManagement.StateDisabled.HeaderColumn.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_UserManagement.StateDisabled.HeaderColumn.Border.Color1 = Color.White;
            DGV_UserManagement.StateDisabled.HeaderColumn.Border.Color2 = Color.White;
            DGV_UserManagement.StateDisabled.HeaderColumn.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_UserManagement.StateDisabled.HeaderColumn.Content.Color1 = Color.White;
            DGV_UserManagement.StateDisabled.HeaderColumn.Content.Color2 = Color.White;
            DGV_UserManagement.StateDisabled.HeaderRow.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_UserManagement.StateDisabled.HeaderRow.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_UserManagement.StateDisabled.HeaderRow.Border.Color1 = Color.White;
            DGV_UserManagement.StateDisabled.HeaderRow.Border.Color2 = Color.White;
            DGV_UserManagement.StateDisabled.HeaderRow.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_UserManagement.StateDisabled.HeaderRow.Content.Color1 = Color.White;
            DGV_UserManagement.StateDisabled.HeaderRow.Content.Color2 = Color.White;
            DGV_UserManagement.StateNormal.Background.Color1 = Color.FromArgb(44, 44, 44);
            DGV_UserManagement.StateNormal.Background.Color2 = Color.FromArgb(44, 44, 44);
            DGV_UserManagement.StateNormal.DataCell.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_UserManagement.StateNormal.DataCell.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_UserManagement.StateNormal.DataCell.Border.Color1 = Color.White;
            DGV_UserManagement.StateNormal.DataCell.Border.Color2 = Color.White;
            DGV_UserManagement.StateNormal.DataCell.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_UserManagement.StateNormal.DataCell.Content.Color1 = Color.White;
            DGV_UserManagement.StateNormal.DataCell.Content.Color2 = Color.White;
            DGV_UserManagement.StateNormal.HeaderColumn.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_UserManagement.StateNormal.HeaderColumn.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_UserManagement.StateNormal.HeaderColumn.Border.Color1 = Color.White;
            DGV_UserManagement.StateNormal.HeaderColumn.Border.Color2 = Color.White;
            DGV_UserManagement.StateNormal.HeaderColumn.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_UserManagement.StateNormal.HeaderColumn.Content.Color1 = Color.White;
            DGV_UserManagement.StateNormal.HeaderColumn.Content.Color2 = Color.White;
            DGV_UserManagement.StateNormal.HeaderRow.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_UserManagement.StateNormal.HeaderRow.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_UserManagement.StateNormal.HeaderRow.Border.Color1 = Color.White;
            DGV_UserManagement.StateNormal.HeaderRow.Border.Color2 = Color.White;
            DGV_UserManagement.StateNormal.HeaderRow.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_UserManagement.StateNormal.HeaderRow.Content.Color1 = Color.White;
            DGV_UserManagement.StateNormal.HeaderRow.Content.Color2 = Color.White;
            DGV_UserManagement.StatePressed.HeaderColumn.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_UserManagement.StatePressed.HeaderColumn.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_UserManagement.StatePressed.HeaderColumn.Border.Color1 = Color.White;
            DGV_UserManagement.StatePressed.HeaderColumn.Border.Color2 = Color.White;
            DGV_UserManagement.StatePressed.HeaderColumn.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_UserManagement.StatePressed.HeaderColumn.Content.Color1 = Color.White;
            DGV_UserManagement.StatePressed.HeaderColumn.Content.Color2 = Color.White;
            DGV_UserManagement.StatePressed.HeaderRow.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_UserManagement.StatePressed.HeaderRow.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_UserManagement.StatePressed.HeaderRow.Border.Color1 = Color.White;
            DGV_UserManagement.StatePressed.HeaderRow.Border.Color2 = Color.White;
            DGV_UserManagement.StatePressed.HeaderRow.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_UserManagement.StatePressed.HeaderRow.Content.Color1 = Color.White;
            DGV_UserManagement.StatePressed.HeaderRow.Content.Color2 = Color.White;
            DGV_UserManagement.StateSelected.DataCell.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_UserManagement.StateSelected.DataCell.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_UserManagement.StateSelected.DataCell.Border.Color1 = Color.White;
            DGV_UserManagement.StateSelected.DataCell.Border.Color2 = Color.White;
            DGV_UserManagement.StateSelected.DataCell.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_UserManagement.StateSelected.DataCell.Content.Color1 = Color.White;
            DGV_UserManagement.StateSelected.DataCell.Content.Color2 = Color.White;
            DGV_UserManagement.StateSelected.HeaderColumn.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_UserManagement.StateSelected.HeaderColumn.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_UserManagement.StateSelected.HeaderColumn.Border.Color1 = Color.White;
            DGV_UserManagement.StateSelected.HeaderColumn.Border.Color2 = Color.White;
            DGV_UserManagement.StateSelected.HeaderColumn.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_UserManagement.StateSelected.HeaderColumn.Content.Color1 = Color.White;
            DGV_UserManagement.StateSelected.HeaderColumn.Content.Color2 = Color.White;
            DGV_UserManagement.StateSelected.HeaderRow.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_UserManagement.StateSelected.HeaderRow.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_UserManagement.StateSelected.HeaderRow.Border.Color1 = Color.White;
            DGV_UserManagement.StateSelected.HeaderRow.Border.Color2 = Color.White;
            DGV_UserManagement.StateSelected.HeaderRow.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_UserManagement.StateSelected.HeaderRow.Content.Color1 = Color.White;
            DGV_UserManagement.StateSelected.HeaderRow.Content.Color2 = Color.White;
            DGV_UserManagement.StateTracking.HeaderColumn.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_UserManagement.StateTracking.HeaderColumn.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_UserManagement.StateTracking.HeaderColumn.Border.Color1 = Color.White;
            DGV_UserManagement.StateTracking.HeaderColumn.Border.Color2 = Color.White;
            DGV_UserManagement.StateTracking.HeaderColumn.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_UserManagement.StateTracking.HeaderColumn.Content.Color1 = Color.White;
            DGV_UserManagement.StateTracking.HeaderColumn.Content.Color2 = Color.White;
            DGV_UserManagement.StateTracking.HeaderRow.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_UserManagement.StateTracking.HeaderRow.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_UserManagement.StateTracking.HeaderRow.Border.Color1 = Color.White;
            DGV_UserManagement.StateTracking.HeaderRow.Border.Color2 = Color.White;
            DGV_UserManagement.StateTracking.HeaderRow.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_UserManagement.StateTracking.HeaderRow.Content.Color1 = Color.White;
            DGV_UserManagement.StateTracking.HeaderRow.Content.Color2 = Color.White;
            DGV_UserManagement.TabIndex = 1;
            DGV_UserManagement.CellDoubleClick += DGV_UserManagement_CellDoubleClick;
            // 
            // label_UM_ID
            // 
            label_UM_ID.AutoSize = true;
            label_UM_ID.Font = new Font("Arial", 24F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_UM_ID.ForeColor = SystemColors.Window;
            label_UM_ID.Location = new Point(147, 39);
            label_UM_ID.Name = "label_UM_ID";
            label_UM_ID.Size = new Size(47, 36);
            label_UM_ID.TabIndex = 12;
            label_UM_ID.Text = "ID";
            // 
            // TextBox_UM_ID
            // 
            TextBox_UM_ID.BackColor = Color.FromArgb(44, 44, 44);
            TextBox_UM_ID.BorderStyle = BorderStyle.None;
            TextBox_UM_ID.Font = new Font("Arial", 24F, FontStyle.Regular, GraphicsUnit.Point, 0);
            TextBox_UM_ID.ForeColor = SystemColors.Window;
            TextBox_UM_ID.Location = new Point(230, 49);
            TextBox_UM_ID.Name = "TextBox_UM_ID";
            TextBox_UM_ID.Size = new Size(218, 37);
            TextBox_UM_ID.TabIndex = 9;
            // 
            // panel_UM_ID
            // 
            panel_UM_ID.BackColor = Color.Silver;
            panel_UM_ID.ForeColor = Color.Silver;
            panel_UM_ID.Location = new Point(148, 93);
            panel_UM_ID.Name = "panel_UM_ID";
            panel_UM_ID.Size = new Size(300, 5);
            panel_UM_ID.TabIndex = 11;
            // 
            // UserManagementView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(64, 64, 64);
            Controls.Add(card_userManagement);
            Name = "UserManagementView";
            Size = new Size(1600, 900);
            Load += UserManagementView_Load;
            card_userManagement.ResumeLayout(false);
            card_userManagement.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)kComboBox_UM_Role).EndInit();
            ((System.ComponentModel.ISupportInitialize)DGV_UserManagement).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel card_userManagement;
        private Panel panel_UM_ID;
        private TextBox TextBox_UM_ID;
        private Label label_UM_ID;
        private ComponentFactory.Krypton.Toolkit.KryptonDataGridView DGV_UserManagement;
        private Label label_UM_Role;
        private Panel panel_UM_Role;
        private Button btn_UserManage_Refresh;
        private ComponentFactory.Krypton.Toolkit.KryptonComboBox kComboBox_UM_Role;
        private Button btn_UserManage_Search;
        private Button btn_UserManage_ResetPW;
        private Button btn_UserManage_DeleteUser;
        private Button btn_UserManage_EditUser;
        private Button btn_UserManage_AddUser;
    }
}
