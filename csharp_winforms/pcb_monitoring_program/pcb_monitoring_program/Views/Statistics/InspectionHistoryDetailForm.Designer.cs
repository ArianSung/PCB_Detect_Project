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
            DataGridViewCellStyle dataGridViewCellStyle5 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle6 = new DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InspectionHistoryDetailForm));
            cardIHD = new Panel();
            DGV_IHD_result = new ComponentFactory.Krypton.Toolkit.KryptonDataGridView();
            date = new DataGridViewTextBoxColumn();
            time = new DataGridViewTextBoxColumn();
            CameraID = new DataGridViewTextBoxColumn();
            PCBID = new DataGridViewTextBoxColumn();
            DefectType = new DataGridViewTextBoxColumn();
            DefectLocation = new DataGridViewTextBoxColumn();
            productionline = new DataGridViewTextBoxColumn();
            pictureBox_IHD_Image = new PictureBox();
            cardIHD.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)DGV_IHD_result).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox_IHD_Image).BeginInit();
            SuspendLayout();
            // 
            // cardIHD
            // 
            cardIHD.BackColor = Color.FromArgb(64, 64, 64);
            cardIHD.Controls.Add(DGV_IHD_result);
            cardIHD.Controls.Add(pictureBox_IHD_Image);
            cardIHD.Dock = DockStyle.Fill;
            cardIHD.Location = new Point(0, 0);
            cardIHD.Name = "cardIHD";
            cardIHD.Size = new Size(1084, 761);
            cardIHD.TabIndex = 0;
            // 
            // DGV_IHD_result
            // 
            DGV_IHD_result.AllowUserToAddRows = false;
            DGV_IHD_result.AllowUserToResizeColumns = false;
            DGV_IHD_result.AllowUserToResizeRows = false;
            dataGridViewCellStyle5.BackColor = Color.FromArgb(44, 44, 44);
            dataGridViewCellStyle5.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            dataGridViewCellStyle5.ForeColor = Color.White;
            dataGridViewCellStyle5.SelectionBackColor = Color.FromArgb(44, 44, 44);
            dataGridViewCellStyle5.SelectionForeColor = Color.White;
            DGV_IHD_result.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle5;
            DGV_IHD_result.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            DGV_IHD_result.ClipboardCopyMode = DataGridViewClipboardCopyMode.Disable;
            DGV_IHD_result.ColumnHeadersHeight = 41;
            DGV_IHD_result.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            DGV_IHD_result.Columns.AddRange(new DataGridViewColumn[] { date, time, CameraID, PCBID, DefectType, DefectLocation, productionline });
            DGV_IHD_result.Location = new Point(718, 67);
            DGV_IHD_result.Name = "DGV_IHD_result";
            DGV_IHD_result.PaletteMode = ComponentFactory.Krypton.Toolkit.PaletteMode.Office2010Black;
            DGV_IHD_result.ReadOnly = true;
            DGV_IHD_result.RowHeadersVisible = false;
            dataGridViewCellStyle6.BackColor = Color.FromArgb(44, 44, 44);
            dataGridViewCellStyle6.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            dataGridViewCellStyle6.ForeColor = Color.White;
            dataGridViewCellStyle6.SelectionBackColor = Color.FromArgb(44, 44, 44);
            dataGridViewCellStyle6.SelectionForeColor = Color.White;
            DGV_IHD_result.RowsDefaultCellStyle = dataGridViewCellStyle6;
            DGV_IHD_result.RowTemplate.DefaultCellStyle.BackColor = Color.FromArgb(44, 44, 44);
            DGV_IHD_result.RowTemplate.DefaultCellStyle.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            DGV_IHD_result.RowTemplate.DefaultCellStyle.ForeColor = Color.White;
            DGV_IHD_result.RowTemplate.DefaultCellStyle.SelectionBackColor = Color.FromArgb(44, 44, 44);
            DGV_IHD_result.RowTemplate.DefaultCellStyle.SelectionForeColor = Color.White;
            DGV_IHD_result.RowTemplate.Height = 41;
            DGV_IHD_result.ScrollBars = ScrollBars.Vertical;
            DGV_IHD_result.Size = new Size(310, 640);
            DGV_IHD_result.StateCommon.Background.Color1 = Color.FromArgb(44, 44, 44);
            DGV_IHD_result.StateCommon.Background.Color2 = Color.FromArgb(44, 44, 44);
            DGV_IHD_result.StateCommon.BackStyle = ComponentFactory.Krypton.Toolkit.PaletteBackStyle.GridBackgroundList;
            DGV_IHD_result.StateCommon.DataCell.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_IHD_result.StateCommon.DataCell.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_IHD_result.StateCommon.DataCell.Border.Color1 = Color.White;
            DGV_IHD_result.StateCommon.DataCell.Border.Color2 = Color.White;
            DGV_IHD_result.StateCommon.DataCell.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_IHD_result.StateCommon.DataCell.Content.Color1 = Color.White;
            DGV_IHD_result.StateCommon.DataCell.Content.Color2 = Color.White;
            DGV_IHD_result.StateCommon.DataCell.Content.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            DGV_IHD_result.StateCommon.DataCell.Content.TextH = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
            DGV_IHD_result.StateCommon.DataCell.Content.TextV = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
            DGV_IHD_result.StateCommon.HeaderColumn.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_IHD_result.StateCommon.HeaderColumn.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_IHD_result.StateCommon.HeaderColumn.Border.Color1 = Color.White;
            DGV_IHD_result.StateCommon.HeaderColumn.Border.Color2 = Color.White;
            DGV_IHD_result.StateCommon.HeaderColumn.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_IHD_result.StateCommon.HeaderColumn.Content.Color1 = Color.White;
            DGV_IHD_result.StateCommon.HeaderColumn.Content.Color2 = Color.White;
            DGV_IHD_result.StateCommon.HeaderColumn.Content.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            DGV_IHD_result.StateCommon.HeaderColumn.Content.TextH = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
            DGV_IHD_result.StateCommon.HeaderColumn.Content.TextV = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
            DGV_IHD_result.StateCommon.HeaderRow.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_IHD_result.StateCommon.HeaderRow.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_IHD_result.StateCommon.HeaderRow.Border.Color1 = Color.White;
            DGV_IHD_result.StateCommon.HeaderRow.Border.Color2 = Color.White;
            DGV_IHD_result.StateCommon.HeaderRow.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_IHD_result.StateCommon.HeaderRow.Content.Color1 = Color.White;
            DGV_IHD_result.StateCommon.HeaderRow.Content.Color2 = Color.White;
            DGV_IHD_result.StateCommon.HeaderRow.Content.Font = new Font("맑은 고딕", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            DGV_IHD_result.StateDisabled.Background.Color1 = Color.FromArgb(44, 44, 44);
            DGV_IHD_result.StateDisabled.Background.Color2 = Color.FromArgb(44, 44, 44);
            DGV_IHD_result.StateDisabled.DataCell.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_IHD_result.StateDisabled.DataCell.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_IHD_result.StateDisabled.DataCell.Border.Color1 = Color.White;
            DGV_IHD_result.StateDisabled.DataCell.Border.Color2 = Color.White;
            DGV_IHD_result.StateDisabled.DataCell.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_IHD_result.StateDisabled.DataCell.Content.Color1 = Color.White;
            DGV_IHD_result.StateDisabled.DataCell.Content.Color2 = Color.White;
            DGV_IHD_result.StateDisabled.HeaderColumn.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_IHD_result.StateDisabled.HeaderColumn.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_IHD_result.StateDisabled.HeaderColumn.Border.Color1 = Color.White;
            DGV_IHD_result.StateDisabled.HeaderColumn.Border.Color2 = Color.White;
            DGV_IHD_result.StateDisabled.HeaderColumn.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_IHD_result.StateDisabled.HeaderColumn.Content.Color1 = Color.White;
            DGV_IHD_result.StateDisabled.HeaderColumn.Content.Color2 = Color.White;
            DGV_IHD_result.StateDisabled.HeaderRow.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_IHD_result.StateDisabled.HeaderRow.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_IHD_result.StateDisabled.HeaderRow.Border.Color1 = Color.White;
            DGV_IHD_result.StateDisabled.HeaderRow.Border.Color2 = Color.White;
            DGV_IHD_result.StateDisabled.HeaderRow.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_IHD_result.StateDisabled.HeaderRow.Content.Color1 = Color.White;
            DGV_IHD_result.StateDisabled.HeaderRow.Content.Color2 = Color.White;
            DGV_IHD_result.StateNormal.Background.Color1 = Color.FromArgb(44, 44, 44);
            DGV_IHD_result.StateNormal.Background.Color2 = Color.FromArgb(44, 44, 44);
            DGV_IHD_result.StateNormal.DataCell.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_IHD_result.StateNormal.DataCell.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_IHD_result.StateNormal.DataCell.Border.Color1 = Color.White;
            DGV_IHD_result.StateNormal.DataCell.Border.Color2 = Color.White;
            DGV_IHD_result.StateNormal.DataCell.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_IHD_result.StateNormal.DataCell.Content.Color1 = Color.White;
            DGV_IHD_result.StateNormal.DataCell.Content.Color2 = Color.White;
            DGV_IHD_result.StateNormal.HeaderColumn.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_IHD_result.StateNormal.HeaderColumn.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_IHD_result.StateNormal.HeaderColumn.Border.Color1 = Color.White;
            DGV_IHD_result.StateNormal.HeaderColumn.Border.Color2 = Color.White;
            DGV_IHD_result.StateNormal.HeaderColumn.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_IHD_result.StateNormal.HeaderColumn.Content.Color1 = Color.White;
            DGV_IHD_result.StateNormal.HeaderColumn.Content.Color2 = Color.White;
            DGV_IHD_result.StateNormal.HeaderRow.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_IHD_result.StateNormal.HeaderRow.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_IHD_result.StateNormal.HeaderRow.Border.Color1 = Color.White;
            DGV_IHD_result.StateNormal.HeaderRow.Border.Color2 = Color.White;
            DGV_IHD_result.StateNormal.HeaderRow.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_IHD_result.StateNormal.HeaderRow.Content.Color1 = Color.White;
            DGV_IHD_result.StateNormal.HeaderRow.Content.Color2 = Color.White;
            DGV_IHD_result.StatePressed.HeaderColumn.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_IHD_result.StatePressed.HeaderColumn.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_IHD_result.StatePressed.HeaderColumn.Border.Color1 = Color.White;
            DGV_IHD_result.StatePressed.HeaderColumn.Border.Color2 = Color.White;
            DGV_IHD_result.StatePressed.HeaderColumn.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_IHD_result.StatePressed.HeaderColumn.Content.Color1 = Color.White;
            DGV_IHD_result.StatePressed.HeaderColumn.Content.Color2 = Color.White;
            DGV_IHD_result.StatePressed.HeaderRow.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_IHD_result.StatePressed.HeaderRow.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_IHD_result.StatePressed.HeaderRow.Border.Color1 = Color.White;
            DGV_IHD_result.StatePressed.HeaderRow.Border.Color2 = Color.White;
            DGV_IHD_result.StatePressed.HeaderRow.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_IHD_result.StatePressed.HeaderRow.Content.Color1 = Color.White;
            DGV_IHD_result.StatePressed.HeaderRow.Content.Color2 = Color.White;
            DGV_IHD_result.StateSelected.DataCell.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_IHD_result.StateSelected.DataCell.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_IHD_result.StateSelected.DataCell.Border.Color1 = Color.White;
            DGV_IHD_result.StateSelected.DataCell.Border.Color2 = Color.White;
            DGV_IHD_result.StateSelected.DataCell.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_IHD_result.StateSelected.DataCell.Content.Color1 = Color.White;
            DGV_IHD_result.StateSelected.DataCell.Content.Color2 = Color.White;
            DGV_IHD_result.StateSelected.HeaderColumn.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_IHD_result.StateSelected.HeaderColumn.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_IHD_result.StateSelected.HeaderColumn.Border.Color1 = Color.White;
            DGV_IHD_result.StateSelected.HeaderColumn.Border.Color2 = Color.White;
            DGV_IHD_result.StateSelected.HeaderColumn.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_IHD_result.StateSelected.HeaderColumn.Content.Color1 = Color.White;
            DGV_IHD_result.StateSelected.HeaderColumn.Content.Color2 = Color.White;
            DGV_IHD_result.StateSelected.HeaderRow.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_IHD_result.StateSelected.HeaderRow.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_IHD_result.StateSelected.HeaderRow.Border.Color1 = Color.White;
            DGV_IHD_result.StateSelected.HeaderRow.Border.Color2 = Color.White;
            DGV_IHD_result.StateSelected.HeaderRow.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_IHD_result.StateSelected.HeaderRow.Content.Color1 = Color.White;
            DGV_IHD_result.StateSelected.HeaderRow.Content.Color2 = Color.White;
            DGV_IHD_result.StateTracking.HeaderColumn.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_IHD_result.StateTracking.HeaderColumn.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_IHD_result.StateTracking.HeaderColumn.Border.Color1 = Color.White;
            DGV_IHD_result.StateTracking.HeaderColumn.Border.Color2 = Color.White;
            DGV_IHD_result.StateTracking.HeaderColumn.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_IHD_result.StateTracking.HeaderColumn.Content.Color1 = Color.White;
            DGV_IHD_result.StateTracking.HeaderColumn.Content.Color2 = Color.White;
            DGV_IHD_result.StateTracking.HeaderRow.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_IHD_result.StateTracking.HeaderRow.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_IHD_result.StateTracking.HeaderRow.Border.Color1 = Color.White;
            DGV_IHD_result.StateTracking.HeaderRow.Border.Color2 = Color.White;
            DGV_IHD_result.StateTracking.HeaderRow.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_IHD_result.StateTracking.HeaderRow.Content.Color1 = Color.White;
            DGV_IHD_result.StateTracking.HeaderRow.Content.Color2 = Color.White;
            DGV_IHD_result.TabIndex = 1;
            // 
            // date
            // 
            date.HeaderText = "날짜";
            date.Name = "date";
            date.ReadOnly = true;
            // 
            // time
            // 
            time.HeaderText = "시간";
            time.Name = "time";
            time.ReadOnly = true;
            // 
            // CameraID
            // 
            CameraID.HeaderText = "카메라 ID";
            CameraID.Name = "CameraID";
            CameraID.ReadOnly = true;
            // 
            // PCBID
            // 
            PCBID.HeaderText = "PCB ID";
            PCBID.Name = "PCBID";
            PCBID.ReadOnly = true;
            // 
            // DefectType
            // 
            DefectType.HeaderText = "불량 유형";
            DefectType.Name = "DefectType";
            DefectType.ReadOnly = true;
            // 
            // DefectLocation
            // 
            DefectLocation.HeaderText = "불량 위치";
            DefectLocation.Name = "DefectLocation";
            DefectLocation.ReadOnly = true;
            // 
            // productionline
            // 
            productionline.HeaderText = "생산 라인";
            productionline.Name = "productionline";
            productionline.ReadOnly = true;
            // 
            // pictureBox_IHD_Image
            // 
            pictureBox_IHD_Image.BackColor = Color.Black;
            pictureBox_IHD_Image.Image = (Image)resources.GetObject("pictureBox_IHD_Image.Image");
            pictureBox_IHD_Image.Location = new Point(53, 67);
            pictureBox_IHD_Image.Name = "pictureBox_IHD_Image";
            pictureBox_IHD_Image.Size = new Size(640, 640);
            pictureBox_IHD_Image.TabIndex = 0;
            pictureBox_IHD_Image.TabStop = false;
            // 
            // InspectionHistoryDetailForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1084, 761);
            Controls.Add(cardIHD);
            Name = "InspectionHistoryDetailForm";
            Text = "InspectionHistoryDetailForm";
            cardIHD.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)DGV_IHD_result).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox_IHD_Image).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel cardIHD;
        private PictureBox pictureBox_IHD_Image;
        private ComponentFactory.Krypton.Toolkit.KryptonDataGridView DGV_IHD_result;
        private DataGridViewTextBoxColumn date;
        private DataGridViewTextBoxColumn time;
        private DataGridViewTextBoxColumn CameraID;
        private DataGridViewTextBoxColumn PCBID;
        private DataGridViewTextBoxColumn DefectType;
        private DataGridViewTextBoxColumn DefectLocation;
        private DataGridViewTextBoxColumn productionline;
    }
}