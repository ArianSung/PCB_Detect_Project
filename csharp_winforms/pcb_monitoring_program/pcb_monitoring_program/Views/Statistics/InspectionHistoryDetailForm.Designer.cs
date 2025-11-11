namespace pcb_monitoring_program.Views.Statistics
{
    partial class InspectionHistoryDetailForm
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
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InspectionHistoryDetailForm));
            panel1 = new Panel();
            kryptonDataGridView1 = new ComponentFactory.Krypton.Toolkit.KryptonDataGridView();
            date = new DataGridViewTextBoxColumn();
            time = new DataGridViewTextBoxColumn();
            CameraID = new DataGridViewTextBoxColumn();
            PCBID = new DataGridViewTextBoxColumn();
            DefectType = new DataGridViewTextBoxColumn();
            DefectLocation = new DataGridViewTextBoxColumn();
            productionline = new DataGridViewTextBoxColumn();
            pictureBox1 = new PictureBox();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)kryptonDataGridView1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = Color.FromArgb(64, 64, 64);
            panel1.Controls.Add(kryptonDataGridView1);
            panel1.Controls.Add(pictureBox1);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(1084, 761);
            panel1.TabIndex = 0;
            // 
            // kryptonDataGridView1
            // 
            kryptonDataGridView1.AllowUserToAddRows = false;
            kryptonDataGridView1.AllowUserToResizeColumns = false;
            kryptonDataGridView1.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(44, 44, 44);
            dataGridViewCellStyle1.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            dataGridViewCellStyle1.ForeColor = Color.White;
            dataGridViewCellStyle1.SelectionBackColor = Color.FromArgb(44, 44, 44);
            dataGridViewCellStyle1.SelectionForeColor = Color.White;
            kryptonDataGridView1.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            kryptonDataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            kryptonDataGridView1.ClipboardCopyMode = DataGridViewClipboardCopyMode.Disable;
            kryptonDataGridView1.ColumnHeadersHeight = 41;
            kryptonDataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            kryptonDataGridView1.Columns.AddRange(new DataGridViewColumn[] { date, time, CameraID, PCBID, DefectType, DefectLocation, productionline });
            kryptonDataGridView1.Location = new Point(718, 67);
            kryptonDataGridView1.Name = "kryptonDataGridView1";
            kryptonDataGridView1.PaletteMode = ComponentFactory.Krypton.Toolkit.PaletteMode.Office2010Black;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(44, 44, 44);
            dataGridViewCellStyle2.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            dataGridViewCellStyle2.ForeColor = Color.White;
            dataGridViewCellStyle2.SelectionBackColor = Color.FromArgb(44, 44, 44);
            dataGridViewCellStyle2.SelectionForeColor = Color.White;
            kryptonDataGridView1.RowsDefaultCellStyle = dataGridViewCellStyle2;
            kryptonDataGridView1.RowTemplate.DefaultCellStyle.BackColor = Color.FromArgb(44, 44, 44);
            kryptonDataGridView1.RowTemplate.DefaultCellStyle.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            kryptonDataGridView1.RowTemplate.DefaultCellStyle.ForeColor = Color.White;
            kryptonDataGridView1.RowTemplate.DefaultCellStyle.SelectionBackColor = Color.FromArgb(44, 44, 44);
            kryptonDataGridView1.RowTemplate.DefaultCellStyle.SelectionForeColor = Color.White;
            kryptonDataGridView1.RowTemplate.Height = 41;
            kryptonDataGridView1.ScrollBars = ScrollBars.Vertical;
            kryptonDataGridView1.Size = new Size(310, 640);
            kryptonDataGridView1.StateCommon.Background.Color1 = Color.FromArgb(44, 44, 44);
            kryptonDataGridView1.StateCommon.Background.Color2 = Color.FromArgb(44, 44, 44);
            kryptonDataGridView1.StateCommon.BackStyle = ComponentFactory.Krypton.Toolkit.PaletteBackStyle.GridBackgroundList;
            kryptonDataGridView1.StateCommon.DataCell.Back.Color1 = Color.FromArgb(44, 44, 44);
            kryptonDataGridView1.StateCommon.DataCell.Back.Color2 = Color.FromArgb(44, 44, 44);
            kryptonDataGridView1.StateCommon.DataCell.Border.Color1 = Color.White;
            kryptonDataGridView1.StateCommon.DataCell.Border.Color2 = Color.White;
            kryptonDataGridView1.StateCommon.DataCell.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            kryptonDataGridView1.StateCommon.DataCell.Content.Color1 = Color.White;
            kryptonDataGridView1.StateCommon.DataCell.Content.Color2 = Color.White;
            kryptonDataGridView1.StateCommon.DataCell.Content.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            kryptonDataGridView1.StateCommon.DataCell.Content.TextH = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
            kryptonDataGridView1.StateCommon.DataCell.Content.TextV = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
            kryptonDataGridView1.StateCommon.HeaderColumn.Back.Color1 = Color.FromArgb(44, 44, 44);
            kryptonDataGridView1.StateCommon.HeaderColumn.Back.Color2 = Color.FromArgb(44, 44, 44);
            kryptonDataGridView1.StateCommon.HeaderColumn.Border.Color1 = Color.White;
            kryptonDataGridView1.StateCommon.HeaderColumn.Border.Color2 = Color.White;
            kryptonDataGridView1.StateCommon.HeaderColumn.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            kryptonDataGridView1.StateCommon.HeaderColumn.Content.Color1 = Color.White;
            kryptonDataGridView1.StateCommon.HeaderColumn.Content.Color2 = Color.White;
            kryptonDataGridView1.StateCommon.HeaderColumn.Content.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            kryptonDataGridView1.StateCommon.HeaderColumn.Content.TextH = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
            kryptonDataGridView1.StateCommon.HeaderColumn.Content.TextV = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
            kryptonDataGridView1.StateCommon.HeaderRow.Back.Color1 = Color.FromArgb(44, 44, 44);
            kryptonDataGridView1.StateCommon.HeaderRow.Back.Color2 = Color.FromArgb(44, 44, 44);
            kryptonDataGridView1.StateCommon.HeaderRow.Border.Color1 = Color.White;
            kryptonDataGridView1.StateCommon.HeaderRow.Border.Color2 = Color.White;
            kryptonDataGridView1.StateCommon.HeaderRow.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            kryptonDataGridView1.StateCommon.HeaderRow.Content.Color1 = Color.White;
            kryptonDataGridView1.StateCommon.HeaderRow.Content.Color2 = Color.White;
            kryptonDataGridView1.StateCommon.HeaderRow.Content.Font = new Font("맑은 고딕", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            kryptonDataGridView1.StateDisabled.Background.Color1 = Color.FromArgb(44, 44, 44);
            kryptonDataGridView1.StateDisabled.Background.Color2 = Color.FromArgb(44, 44, 44);
            kryptonDataGridView1.StateDisabled.DataCell.Back.Color1 = Color.FromArgb(44, 44, 44);
            kryptonDataGridView1.StateDisabled.DataCell.Back.Color2 = Color.FromArgb(44, 44, 44);
            kryptonDataGridView1.StateDisabled.DataCell.Border.Color1 = Color.White;
            kryptonDataGridView1.StateDisabled.DataCell.Border.Color2 = Color.White;
            kryptonDataGridView1.StateDisabled.DataCell.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            kryptonDataGridView1.StateDisabled.DataCell.Content.Color1 = Color.White;
            kryptonDataGridView1.StateDisabled.DataCell.Content.Color2 = Color.White;
            kryptonDataGridView1.StateDisabled.HeaderColumn.Back.Color1 = Color.FromArgb(44, 44, 44);
            kryptonDataGridView1.StateDisabled.HeaderColumn.Back.Color2 = Color.FromArgb(44, 44, 44);
            kryptonDataGridView1.StateDisabled.HeaderColumn.Border.Color1 = Color.White;
            kryptonDataGridView1.StateDisabled.HeaderColumn.Border.Color2 = Color.White;
            kryptonDataGridView1.StateDisabled.HeaderColumn.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            kryptonDataGridView1.StateDisabled.HeaderColumn.Content.Color1 = Color.White;
            kryptonDataGridView1.StateDisabled.HeaderColumn.Content.Color2 = Color.White;
            kryptonDataGridView1.StateDisabled.HeaderRow.Back.Color1 = Color.FromArgb(44, 44, 44);
            kryptonDataGridView1.StateDisabled.HeaderRow.Back.Color2 = Color.FromArgb(44, 44, 44);
            kryptonDataGridView1.StateDisabled.HeaderRow.Border.Color1 = Color.White;
            kryptonDataGridView1.StateDisabled.HeaderRow.Border.Color2 = Color.White;
            kryptonDataGridView1.StateDisabled.HeaderRow.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            kryptonDataGridView1.StateDisabled.HeaderRow.Content.Color1 = Color.White;
            kryptonDataGridView1.StateDisabled.HeaderRow.Content.Color2 = Color.White;
            kryptonDataGridView1.StateNormal.Background.Color1 = Color.FromArgb(44, 44, 44);
            kryptonDataGridView1.StateNormal.Background.Color2 = Color.FromArgb(44, 44, 44);
            kryptonDataGridView1.StateNormal.DataCell.Back.Color1 = Color.FromArgb(44, 44, 44);
            kryptonDataGridView1.StateNormal.DataCell.Back.Color2 = Color.FromArgb(44, 44, 44);
            kryptonDataGridView1.StateNormal.DataCell.Border.Color1 = Color.White;
            kryptonDataGridView1.StateNormal.DataCell.Border.Color2 = Color.White;
            kryptonDataGridView1.StateNormal.DataCell.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            kryptonDataGridView1.StateNormal.DataCell.Content.Color1 = Color.White;
            kryptonDataGridView1.StateNormal.DataCell.Content.Color2 = Color.White;
            kryptonDataGridView1.StateNormal.HeaderColumn.Back.Color1 = Color.FromArgb(44, 44, 44);
            kryptonDataGridView1.StateNormal.HeaderColumn.Back.Color2 = Color.FromArgb(44, 44, 44);
            kryptonDataGridView1.StateNormal.HeaderColumn.Border.Color1 = Color.White;
            kryptonDataGridView1.StateNormal.HeaderColumn.Border.Color2 = Color.White;
            kryptonDataGridView1.StateNormal.HeaderColumn.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            kryptonDataGridView1.StateNormal.HeaderColumn.Content.Color1 = Color.White;
            kryptonDataGridView1.StateNormal.HeaderColumn.Content.Color2 = Color.White;
            kryptonDataGridView1.StateNormal.HeaderRow.Back.Color1 = Color.FromArgb(44, 44, 44);
            kryptonDataGridView1.StateNormal.HeaderRow.Back.Color2 = Color.FromArgb(44, 44, 44);
            kryptonDataGridView1.StateNormal.HeaderRow.Border.Color1 = Color.White;
            kryptonDataGridView1.StateNormal.HeaderRow.Border.Color2 = Color.White;
            kryptonDataGridView1.StateNormal.HeaderRow.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            kryptonDataGridView1.StateNormal.HeaderRow.Content.Color1 = Color.White;
            kryptonDataGridView1.StateNormal.HeaderRow.Content.Color2 = Color.White;
            kryptonDataGridView1.StatePressed.HeaderColumn.Back.Color1 = Color.FromArgb(44, 44, 44);
            kryptonDataGridView1.StatePressed.HeaderColumn.Back.Color2 = Color.FromArgb(44, 44, 44);
            kryptonDataGridView1.StatePressed.HeaderColumn.Border.Color1 = Color.White;
            kryptonDataGridView1.StatePressed.HeaderColumn.Border.Color2 = Color.White;
            kryptonDataGridView1.StatePressed.HeaderColumn.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            kryptonDataGridView1.StatePressed.HeaderColumn.Content.Color1 = Color.White;
            kryptonDataGridView1.StatePressed.HeaderColumn.Content.Color2 = Color.White;
            kryptonDataGridView1.StatePressed.HeaderRow.Back.Color1 = Color.FromArgb(44, 44, 44);
            kryptonDataGridView1.StatePressed.HeaderRow.Back.Color2 = Color.FromArgb(44, 44, 44);
            kryptonDataGridView1.StatePressed.HeaderRow.Border.Color1 = Color.White;
            kryptonDataGridView1.StatePressed.HeaderRow.Border.Color2 = Color.White;
            kryptonDataGridView1.StatePressed.HeaderRow.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            kryptonDataGridView1.StatePressed.HeaderRow.Content.Color1 = Color.White;
            kryptonDataGridView1.StatePressed.HeaderRow.Content.Color2 = Color.White;
            kryptonDataGridView1.StateSelected.DataCell.Back.Color1 = Color.FromArgb(44, 44, 44);
            kryptonDataGridView1.StateSelected.DataCell.Back.Color2 = Color.FromArgb(44, 44, 44);
            kryptonDataGridView1.StateSelected.DataCell.Border.Color1 = Color.White;
            kryptonDataGridView1.StateSelected.DataCell.Border.Color2 = Color.White;
            kryptonDataGridView1.StateSelected.DataCell.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            kryptonDataGridView1.StateSelected.DataCell.Content.Color1 = Color.White;
            kryptonDataGridView1.StateSelected.DataCell.Content.Color2 = Color.White;
            kryptonDataGridView1.StateSelected.HeaderColumn.Back.Color1 = Color.FromArgb(44, 44, 44);
            kryptonDataGridView1.StateSelected.HeaderColumn.Back.Color2 = Color.FromArgb(44, 44, 44);
            kryptonDataGridView1.StateSelected.HeaderColumn.Border.Color1 = Color.White;
            kryptonDataGridView1.StateSelected.HeaderColumn.Border.Color2 = Color.White;
            kryptonDataGridView1.StateSelected.HeaderColumn.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            kryptonDataGridView1.StateSelected.HeaderColumn.Content.Color1 = Color.White;
            kryptonDataGridView1.StateSelected.HeaderColumn.Content.Color2 = Color.White;
            kryptonDataGridView1.StateSelected.HeaderRow.Back.Color1 = Color.FromArgb(44, 44, 44);
            kryptonDataGridView1.StateSelected.HeaderRow.Back.Color2 = Color.FromArgb(44, 44, 44);
            kryptonDataGridView1.StateSelected.HeaderRow.Border.Color1 = Color.White;
            kryptonDataGridView1.StateSelected.HeaderRow.Border.Color2 = Color.White;
            kryptonDataGridView1.StateSelected.HeaderRow.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            kryptonDataGridView1.StateSelected.HeaderRow.Content.Color1 = Color.White;
            kryptonDataGridView1.StateSelected.HeaderRow.Content.Color2 = Color.White;
            kryptonDataGridView1.StateTracking.HeaderColumn.Back.Color1 = Color.FromArgb(44, 44, 44);
            kryptonDataGridView1.StateTracking.HeaderColumn.Back.Color2 = Color.FromArgb(44, 44, 44);
            kryptonDataGridView1.StateTracking.HeaderColumn.Border.Color1 = Color.White;
            kryptonDataGridView1.StateTracking.HeaderColumn.Border.Color2 = Color.White;
            kryptonDataGridView1.StateTracking.HeaderColumn.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            kryptonDataGridView1.StateTracking.HeaderColumn.Content.Color1 = Color.White;
            kryptonDataGridView1.StateTracking.HeaderColumn.Content.Color2 = Color.White;
            kryptonDataGridView1.StateTracking.HeaderRow.Back.Color1 = Color.FromArgb(44, 44, 44);
            kryptonDataGridView1.StateTracking.HeaderRow.Back.Color2 = Color.FromArgb(44, 44, 44);
            kryptonDataGridView1.StateTracking.HeaderRow.Border.Color1 = Color.White;
            kryptonDataGridView1.StateTracking.HeaderRow.Border.Color2 = Color.White;
            kryptonDataGridView1.StateTracking.HeaderRow.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            kryptonDataGridView1.StateTracking.HeaderRow.Content.Color1 = Color.White;
            kryptonDataGridView1.StateTracking.HeaderRow.Content.Color2 = Color.White;
            kryptonDataGridView1.TabIndex = 1;
            // 
            // date
            // 
            date.HeaderText = "날짜";
            date.Name = "date";
            // 
            // time
            // 
            time.HeaderText = "시간";
            time.Name = "time";
            // 
            // CameraID
            // 
            CameraID.HeaderText = "카메라 ID";
            CameraID.Name = "CameraID";
            // 
            // PCBID
            // 
            PCBID.HeaderText = "PCB ID";
            PCBID.Name = "PCBID";
            // 
            // DefectType
            // 
            DefectType.HeaderText = "불량 유형";
            DefectType.Name = "DefectType";
            // 
            // DefectLocation
            // 
            DefectLocation.HeaderText = "불량 위치";
            DefectLocation.Name = "DefectLocation";
            // 
            // productionline
            // 
            productionline.HeaderText = "생산 라인";
            productionline.Name = "productionline";
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.Black;
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(53, 67);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(640, 640);
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // InspectionHistoryDetailForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1084, 761);
            Controls.Add(panel1);
            Name = "InspectionHistoryDetailForm";
            Text = "InspectionHistoryDetailForm";
            panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)kryptonDataGridView1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private PictureBox pictureBox1;
        private ComponentFactory.Krypton.Toolkit.KryptonDataGridView kryptonDataGridView1;
        private DataGridViewTextBoxColumn date;
        private DataGridViewTextBoxColumn time;
        private DataGridViewTextBoxColumn CameraID;
        private DataGridViewTextBoxColumn PCBID;
        private DataGridViewTextBoxColumn DefectType;
        private DataGridViewTextBoxColumn DefectLocation;
        private DataGridViewTextBoxColumn productionline;
    }
}