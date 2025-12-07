namespace pcb_monitoring_program.Views.Statistics
{
    partial class InspectionHistoryView
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
            components = new System.ComponentModel.Container();
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            cardfilter = new Panel();
            btn_AllSearch = new Button();
            btn_TodaySearch = new Button();
            btn_ThisMonthSearch = new Button();
            btn_Last7DaysSearch = new Button();
            cardCameraID = new Panel();
            CB_IH_CameraID_CAM03 = new CheckBox();
            CB_IH_CameraID_All = new CheckBox();
            CB_IH_CameraID_CAM02 = new CheckBox();
            CB_IH_CameraID_CAM01 = new CheckBox();
            label_IH_CameraID = new Label();
            cardDefectType = new Panel();
            CB_IH_DefectType_All = new CheckBox();
            CB_IH_DefectType_Scrap = new CheckBox();
            CB_IH_DefectType_SolderingDefect = new CheckBox();
            CB_IH_DefectType_ComponentDefect = new CheckBox();
            CB_IH_DefectType_Normal = new CheckBox();
            label_IH_DefectType = new Label();
            btn_filterSearch = new Button();
            label_IH = new Label();
            cardday = new Panel();
            DTP_IH_EndDate = new ComponentFactory.Krypton.Toolkit.KryptonDateTimePicker();
            label_IH_DatelRange = new Label();
            DTP_IH_StartDate = new ComponentFactory.Krypton.Toolkit.KryptonDateTimePicker();
            cardSearchresult = new Panel();
            DGV_IH_result = new ComponentFactory.Krypton.Toolkit.KryptonDataGridView();
            button1 = new Button();
            iGrid1DefaultCellStyle1 = new TenTec.Windows.iGridLib.iGCellStyle(true);
            iGrid1DefaultColHdrStyle1 = new TenTec.Windows.iGridLib.iGColHdrStyle(true);
            kryptonManager1 = new ComponentFactory.Krypton.Toolkit.KryptonManager(components);
            cardfilter.SuspendLayout();
            cardCameraID.SuspendLayout();
            cardDefectType.SuspendLayout();
            cardday.SuspendLayout();
            cardSearchresult.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)DGV_IH_result).BeginInit();
            SuspendLayout();
            // 
            // cardfilter
            // 
            cardfilter.Controls.Add(btn_AllSearch);
            cardfilter.Controls.Add(btn_TodaySearch);
            cardfilter.Controls.Add(btn_ThisMonthSearch);
            cardfilter.Controls.Add(btn_Last7DaysSearch);
            cardfilter.Controls.Add(cardCameraID);
            cardfilter.Controls.Add(cardDefectType);
            cardfilter.Controls.Add(btn_filterSearch);
            cardfilter.Controls.Add(label_IH);
            cardfilter.Controls.Add(cardday);
            cardfilter.Location = new Point(0, 3);
            cardfilter.Name = "cardfilter";
            cardfilter.Size = new Size(1600, 133);
            cardfilter.TabIndex = 4;
            // 
            // btn_AllSearch
            // 
            btn_AllSearch.Font = new Font("Arial", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btn_AllSearch.Location = new Point(607, 8);
            btn_AllSearch.Name = "btn_AllSearch";
            btn_AllSearch.Size = new Size(81, 34);
            btn_AllSearch.TabIndex = 15;
            btn_AllSearch.Text = "All Days";
            btn_AllSearch.UseVisualStyleBackColor = true;
            btn_AllSearch.Click += btn_AllSearch_Click;
            // 
            // btn_TodaySearch
            // 
            btn_TodaySearch.Font = new Font("Arial", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btn_TodaySearch.Location = new Point(238, 8);
            btn_TodaySearch.Name = "btn_TodaySearch";
            btn_TodaySearch.Size = new Size(100, 34);
            btn_TodaySearch.TabIndex = 14;
            btn_TodaySearch.Text = "Today";
            btn_TodaySearch.UseVisualStyleBackColor = true;
            btn_TodaySearch.Click += btn_TodaySearch_Click;
            // 
            // btn_ThisMonthSearch
            // 
            btn_ThisMonthSearch.Font = new Font("Arial", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btn_ThisMonthSearch.Location = new Point(361, 8);
            btn_ThisMonthSearch.Name = "btn_ThisMonthSearch";
            btn_ThisMonthSearch.Size = new Size(100, 34);
            btn_ThisMonthSearch.TabIndex = 13;
            btn_ThisMonthSearch.Text = "This Month";
            btn_ThisMonthSearch.UseVisualStyleBackColor = true;
            btn_ThisMonthSearch.Click += btn_ThisMonthSearch_Click;
            // 
            // btn_Last7DaysSearch
            // 
            btn_Last7DaysSearch.Font = new Font("Arial", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btn_Last7DaysSearch.Location = new Point(484, 8);
            btn_Last7DaysSearch.Name = "btn_Last7DaysSearch";
            btn_Last7DaysSearch.Size = new Size(100, 34);
            btn_Last7DaysSearch.TabIndex = 12;
            btn_Last7DaysSearch.Text = "Last 7 Days";
            btn_Last7DaysSearch.UseVisualStyleBackColor = true;
            btn_Last7DaysSearch.Click += btn_Last7DaysSearch_Click;
            // 
            // cardCameraID
            // 
            cardCameraID.Controls.Add(CB_IH_CameraID_CAM03);
            cardCameraID.Controls.Add(CB_IH_CameraID_All);
            cardCameraID.Controls.Add(CB_IH_CameraID_CAM02);
            cardCameraID.Controls.Add(CB_IH_CameraID_CAM01);
            cardCameraID.Controls.Add(label_IH_CameraID);
            cardCameraID.Location = new Point(1085, 46);
            cardCameraID.Name = "cardCameraID";
            cardCameraID.Size = new Size(496, 72);
            cardCameraID.TabIndex = 11;
            // 
            // CB_IH_CameraID_CAM03
            // 
            CB_IH_CameraID_CAM03.AutoSize = true;
            CB_IH_CameraID_CAM03.Font = new Font("맑은 고딕", 14.25F, FontStyle.Bold);
            CB_IH_CameraID_CAM03.ForeColor = Color.White;
            CB_IH_CameraID_CAM03.Location = new Point(295, 36);
            CB_IH_CameraID_CAM03.Name = "CB_IH_CameraID_CAM03";
            CB_IH_CameraID_CAM03.Size = new Size(96, 29);
            CB_IH_CameraID_CAM03.TabIndex = 10;
            CB_IH_CameraID_CAM03.Text = "CAM03";
            CB_IH_CameraID_CAM03.UseVisualStyleBackColor = true;
            CB_IH_CameraID_CAM03.CheckedChanged += CB_CameraID_CAM03_CheckedChanged;
            // 
            // CB_IH_CameraID_All
            // 
            CB_IH_CameraID_All.AutoSize = true;
            CB_IH_CameraID_All.Font = new Font("맑은 고딕", 14.25F, FontStyle.Bold);
            CB_IH_CameraID_All.ForeColor = Color.White;
            CB_IH_CameraID_All.Location = new Point(16, 36);
            CB_IH_CameraID_All.Name = "CB_IH_CameraID_All";
            CB_IH_CameraID_All.Size = new Size(69, 29);
            CB_IH_CameraID_All.TabIndex = 9;
            CB_IH_CameraID_All.Text = "전체";
            CB_IH_CameraID_All.UseVisualStyleBackColor = true;
            CB_IH_CameraID_All.CheckedChanged += CB_CameraID_All_CheckedChanged;
            // 
            // CB_IH_CameraID_CAM02
            // 
            CB_IH_CameraID_CAM02.AutoSize = true;
            CB_IH_CameraID_CAM02.Font = new Font("맑은 고딕", 14.25F, FontStyle.Bold);
            CB_IH_CameraID_CAM02.ForeColor = Color.White;
            CB_IH_CameraID_CAM02.Location = new Point(193, 36);
            CB_IH_CameraID_CAM02.Name = "CB_IH_CameraID_CAM02";
            CB_IH_CameraID_CAM02.Size = new Size(96, 29);
            CB_IH_CameraID_CAM02.TabIndex = 8;
            CB_IH_CameraID_CAM02.Text = "CAM02";
            CB_IH_CameraID_CAM02.UseVisualStyleBackColor = true;
            CB_IH_CameraID_CAM02.CheckedChanged += CB_CameraID_CAM02_CheckedChanged;
            // 
            // CB_IH_CameraID_CAM01
            // 
            CB_IH_CameraID_CAM01.AutoSize = true;
            CB_IH_CameraID_CAM01.Font = new Font("맑은 고딕", 14.25F, FontStyle.Bold);
            CB_IH_CameraID_CAM01.ForeColor = Color.White;
            CB_IH_CameraID_CAM01.Location = new Point(91, 36);
            CB_IH_CameraID_CAM01.Name = "CB_IH_CameraID_CAM01";
            CB_IH_CameraID_CAM01.Size = new Size(96, 29);
            CB_IH_CameraID_CAM01.TabIndex = 7;
            CB_IH_CameraID_CAM01.Text = "CAM01";
            CB_IH_CameraID_CAM01.UseVisualStyleBackColor = true;
            CB_IH_CameraID_CAM01.CheckedChanged += CB_CameraID_CAM01_CheckedChanged;
            // 
            // label_IH_CameraID
            // 
            label_IH_CameraID.AutoSize = true;
            label_IH_CameraID.Font = new Font("맑은 고딕", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label_IH_CameraID.ForeColor = Color.White;
            label_IH_CameraID.Location = new Point(3, -1);
            label_IH_CameraID.Name = "label_IH_CameraID";
            label_IH_CameraID.Size = new Size(106, 30);
            label_IH_CameraID.TabIndex = 0;
            label_IH_CameraID.Text = "카메라 ID";
            // 
            // cardDefectType
            // 
            cardDefectType.Controls.Add(CB_IH_DefectType_All);
            cardDefectType.Controls.Add(CB_IH_DefectType_Scrap);
            cardDefectType.Controls.Add(CB_IH_DefectType_SolderingDefect);
            cardDefectType.Controls.Add(CB_IH_DefectType_ComponentDefect);
            cardDefectType.Controls.Add(CB_IH_DefectType_Normal);
            cardDefectType.Controls.Add(label_IH_DefectType);
            cardDefectType.Location = new Point(571, 46);
            cardDefectType.Name = "cardDefectType";
            cardDefectType.Size = new Size(506, 72);
            cardDefectType.TabIndex = 10;
            // 
            // CB_IH_DefectType_All
            // 
            CB_IH_DefectType_All.AutoSize = true;
            CB_IH_DefectType_All.Font = new Font("맑은 고딕", 14.25F, FontStyle.Bold);
            CB_IH_DefectType_All.ForeColor = Color.White;
            CB_IH_DefectType_All.Location = new Point(16, 35);
            CB_IH_DefectType_All.Name = "CB_IH_DefectType_All";
            CB_IH_DefectType_All.Size = new Size(69, 29);
            CB_IH_DefectType_All.TabIndex = 9;
            CB_IH_DefectType_All.Text = "전체";
            CB_IH_DefectType_All.UseVisualStyleBackColor = true;
            CB_IH_DefectType_All.CheckedChanged += CB_DefectType_All_CheckedChanged;
            // 
            // CB_IH_DefectType_Scrap
            // 
            CB_IH_DefectType_Scrap.AutoSize = true;
            CB_IH_DefectType_Scrap.Font = new Font("맑은 고딕", 14.25F, FontStyle.Bold);
            CB_IH_DefectType_Scrap.ForeColor = Color.White;
            CB_IH_DefectType_Scrap.Location = new Point(420, 35);
            CB_IH_DefectType_Scrap.Name = "CB_IH_DefectType_Scrap";
            CB_IH_DefectType_Scrap.Size = new Size(69, 29);
            CB_IH_DefectType_Scrap.TabIndex = 8;
            CB_IH_DefectType_Scrap.Text = "폐기";
            CB_IH_DefectType_Scrap.UseVisualStyleBackColor = true;
            CB_IH_DefectType_Scrap.CheckedChanged += CB_DefectType_Scrap_CheckedChanged;
            // 
            // CB_IH_DefectType_SolderingDefect
            // 
            CB_IH_DefectType_SolderingDefect.AutoSize = true;
            CB_IH_DefectType_SolderingDefect.Font = new Font("맑은 고딕", 14.25F, FontStyle.Bold);
            CB_IH_DefectType_SolderingDefect.ForeColor = Color.White;
            CB_IH_DefectType_SolderingDefect.Location = new Point(300, 35);
            CB_IH_DefectType_SolderingDefect.Name = "CB_IH_DefectType_SolderingDefect";
            CB_IH_DefectType_SolderingDefect.Size = new Size(96, 29);
            CB_IH_DefectType_SolderingDefect.TabIndex = 7;
            CB_IH_DefectType_SolderingDefect.Text = "QR불량";
            CB_IH_DefectType_SolderingDefect.UseVisualStyleBackColor = true;
            CB_IH_DefectType_SolderingDefect.CheckedChanged += CB_DefectType_SolderingDefect_CheckedChanged;
            // 
            // CB_IH_DefectType_ComponentDefect
            // 
            CB_IH_DefectType_ComponentDefect.AutoSize = true;
            CB_IH_DefectType_ComponentDefect.Font = new Font("맑은 고딕", 14.25F, FontStyle.Bold);
            CB_IH_DefectType_ComponentDefect.ForeColor = Color.White;
            CB_IH_DefectType_ComponentDefect.Location = new Point(180, 35);
            CB_IH_DefectType_ComponentDefect.Name = "CB_IH_DefectType_ComponentDefect";
            CB_IH_DefectType_ComponentDefect.Size = new Size(107, 29);
            CB_IH_DefectType_ComponentDefect.TabIndex = 6;
            CB_IH_DefectType_ComponentDefect.Text = "부품불량";
            CB_IH_DefectType_ComponentDefect.UseVisualStyleBackColor = true;
            CB_IH_DefectType_ComponentDefect.CheckedChanged += CB_DefectType_ComponentDefect_CheckedChanged;
            // 
            // CB_IH_DefectType_Normal
            // 
            CB_IH_DefectType_Normal.AutoSize = true;
            CB_IH_DefectType_Normal.Font = new Font("맑은 고딕", 14.25F, FontStyle.Bold);
            CB_IH_DefectType_Normal.ForeColor = Color.White;
            CB_IH_DefectType_Normal.Location = new Point(98, 35);
            CB_IH_DefectType_Normal.Name = "CB_IH_DefectType_Normal";
            CB_IH_DefectType_Normal.Size = new Size(69, 29);
            CB_IH_DefectType_Normal.TabIndex = 5;
            CB_IH_DefectType_Normal.Text = "정상";
            CB_IH_DefectType_Normal.UseVisualStyleBackColor = true;
            CB_IH_DefectType_Normal.CheckedChanged += CB_DefectType_Normal_CheckedChanged;
            // 
            // label_IH_DefectType
            // 
            label_IH_DefectType.AutoSize = true;
            label_IH_DefectType.Font = new Font("맑은 고딕", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label_IH_DefectType.ForeColor = Color.White;
            label_IH_DefectType.Location = new Point(3, -1);
            label_IH_DefectType.Name = "label_IH_DefectType";
            label_IH_DefectType.Size = new Size(104, 30);
            label_IH_DefectType.TabIndex = 0;
            label_IH_DefectType.Text = "불량 유형";
            // 
            // btn_filterSearch
            // 
            btn_filterSearch.Font = new Font("Arial", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btn_filterSearch.Location = new Point(142, 8);
            btn_filterSearch.Name = "btn_filterSearch";
            btn_filterSearch.Size = new Size(73, 34);
            btn_filterSearch.TabIndex = 5;
            btn_filterSearch.Text = "Search";
            btn_filterSearch.UseVisualStyleBackColor = true;
            btn_filterSearch.Click += btn_filterSearch_Click;
            // 
            // label_IH
            // 
            label_IH.AutoSize = true;
            label_IH.Font = new Font("맑은 고딕", 20.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label_IH.ForeColor = Color.White;
            label_IH.Location = new Point(0, 0);
            label_IH.Name = "label_IH";
            label_IH.Size = new Size(134, 37);
            label_IH.TabIndex = 0;
            label_IH.Text = "검색 필터";
            // 
            // cardday
            // 
            cardday.Controls.Add(DTP_IH_EndDate);
            cardday.Controls.Add(label_IH_DatelRange);
            cardday.Controls.Add(DTP_IH_StartDate);
            cardday.Location = new Point(3, 55);
            cardday.Name = "cardday";
            cardday.Size = new Size(555, 65);
            cardday.TabIndex = 7;
            // 
            // DTP_IH_EndDate
            // 
            DTP_IH_EndDate.Location = new Point(294, 21);
            DTP_IH_EndDate.Name = "DTP_IH_EndDate";
            DTP_IH_EndDate.Size = new Size(248, 31);
            DTP_IH_EndDate.StateCommon.Back.Color1 = Color.FromArgb(44, 44, 44);
            DTP_IH_EndDate.StateCommon.Border.Color1 = Color.FromArgb(44, 44, 44);
            DTP_IH_EndDate.StateCommon.Border.Color2 = Color.FromArgb(44, 44, 44);
            DTP_IH_EndDate.StateCommon.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DTP_IH_EndDate.StateCommon.Content.Color1 = Color.White;
            DTP_IH_EndDate.StateCommon.Content.Font = new Font("맑은 고딕", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            DTP_IH_EndDate.TabIndex = 9;
            // 
            // label_IH_DatelRange
            // 
            label_IH_DatelRange.AutoSize = true;
            label_IH_DatelRange.Font = new Font("맑은 고딕", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label_IH_DatelRange.ForeColor = Color.White;
            label_IH_DatelRange.Location = new Point(252, 11);
            label_IH_DatelRange.Name = "label_IH_DatelRange";
            label_IH_DatelRange.Size = new Size(26, 25);
            label_IH_DatelRange.TabIndex = 3;
            label_IH_DatelRange.Text = "~";
            // 
            // DTP_IH_StartDate
            // 
            DTP_IH_StartDate.Location = new Point(13, 21);
            DTP_IH_StartDate.Name = "DTP_IH_StartDate";
            DTP_IH_StartDate.Size = new Size(248, 31);
            DTP_IH_StartDate.StateCommon.Back.Color1 = Color.FromArgb(44, 44, 44);
            DTP_IH_StartDate.StateCommon.Border.Color1 = Color.FromArgb(44, 44, 44);
            DTP_IH_StartDate.StateCommon.Border.Color2 = Color.FromArgb(44, 44, 44);
            DTP_IH_StartDate.StateCommon.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DTP_IH_StartDate.StateCommon.Content.Color1 = Color.White;
            DTP_IH_StartDate.StateCommon.Content.Font = new Font("맑은 고딕", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            DTP_IH_StartDate.TabIndex = 8;
            // 
            // cardSearchresult
            // 
            cardSearchresult.Controls.Add(button1);
            cardSearchresult.Controls.Add(DGV_IH_result);
            cardSearchresult.Location = new Point(0, 162);
            cardSearchresult.Name = "cardSearchresult";
            cardSearchresult.Size = new Size(1600, 600);
            cardSearchresult.TabIndex = 5;
            // 
            // DGV_IH_result
            // 
            DGV_IH_result.AllowUserToAddRows = false;
            DGV_IH_result.AllowUserToResizeColumns = false;
            DGV_IH_result.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(44, 44, 44);
            dataGridViewCellStyle1.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            dataGridViewCellStyle1.ForeColor = Color.White;
            dataGridViewCellStyle1.SelectionBackColor = Color.FromArgb(44, 44, 44);
            dataGridViewCellStyle1.SelectionForeColor = Color.White;
            DGV_IH_result.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            DGV_IH_result.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            DGV_IH_result.ClipboardCopyMode = DataGridViewClipboardCopyMode.Disable;
            DGV_IH_result.ColumnHeadersHeight = 41;
            DGV_IH_result.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            DGV_IH_result.Location = new Point(16, 13);
            DGV_IH_result.Name = "DGV_IH_result";
            DGV_IH_result.PaletteMode = ComponentFactory.Krypton.Toolkit.PaletteMode.Office2010Black;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(44, 44, 44);
            dataGridViewCellStyle2.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            dataGridViewCellStyle2.ForeColor = Color.White;
            dataGridViewCellStyle2.SelectionBackColor = Color.FromArgb(44, 44, 44);
            dataGridViewCellStyle2.SelectionForeColor = Color.White;
            DGV_IH_result.RowsDefaultCellStyle = dataGridViewCellStyle2;
            DGV_IH_result.RowTemplate.DefaultCellStyle.BackColor = Color.FromArgb(44, 44, 44);
            DGV_IH_result.RowTemplate.DefaultCellStyle.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            DGV_IH_result.RowTemplate.DefaultCellStyle.ForeColor = Color.White;
            DGV_IH_result.RowTemplate.DefaultCellStyle.SelectionBackColor = Color.FromArgb(44, 44, 44);
            DGV_IH_result.RowTemplate.DefaultCellStyle.SelectionForeColor = Color.White;
            DGV_IH_result.RowTemplate.Height = 41;
            DGV_IH_result.ScrollBars = ScrollBars.Vertical;
            DGV_IH_result.Size = new Size(1565, 563);
            DGV_IH_result.StateCommon.Background.Color1 = Color.FromArgb(44, 44, 44);
            DGV_IH_result.StateCommon.Background.Color2 = Color.FromArgb(44, 44, 44);
            DGV_IH_result.StateCommon.BackStyle = ComponentFactory.Krypton.Toolkit.PaletteBackStyle.GridBackgroundList;
            DGV_IH_result.StateCommon.DataCell.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_IH_result.StateCommon.DataCell.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_IH_result.StateCommon.DataCell.Border.Color1 = Color.White;
            DGV_IH_result.StateCommon.DataCell.Border.Color2 = Color.White;
            DGV_IH_result.StateCommon.DataCell.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_IH_result.StateCommon.DataCell.Content.Color1 = Color.White;
            DGV_IH_result.StateCommon.DataCell.Content.Color2 = Color.White;
            DGV_IH_result.StateCommon.DataCell.Content.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            DGV_IH_result.StateCommon.DataCell.Content.TextH = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
            DGV_IH_result.StateCommon.DataCell.Content.TextV = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
            DGV_IH_result.StateCommon.HeaderColumn.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_IH_result.StateCommon.HeaderColumn.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_IH_result.StateCommon.HeaderColumn.Border.Color1 = Color.White;
            DGV_IH_result.StateCommon.HeaderColumn.Border.Color2 = Color.White;
            DGV_IH_result.StateCommon.HeaderColumn.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_IH_result.StateCommon.HeaderColumn.Content.Color1 = Color.White;
            DGV_IH_result.StateCommon.HeaderColumn.Content.Color2 = Color.White;
            DGV_IH_result.StateCommon.HeaderColumn.Content.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            DGV_IH_result.StateCommon.HeaderColumn.Content.TextH = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
            DGV_IH_result.StateCommon.HeaderColumn.Content.TextV = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
            DGV_IH_result.StateCommon.HeaderRow.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_IH_result.StateCommon.HeaderRow.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_IH_result.StateCommon.HeaderRow.Border.Color1 = Color.White;
            DGV_IH_result.StateCommon.HeaderRow.Border.Color2 = Color.White;
            DGV_IH_result.StateCommon.HeaderRow.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_IH_result.StateCommon.HeaderRow.Content.Color1 = Color.White;
            DGV_IH_result.StateCommon.HeaderRow.Content.Color2 = Color.White;
            DGV_IH_result.StateCommon.HeaderRow.Content.Font = new Font("맑은 고딕", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            DGV_IH_result.StateDisabled.Background.Color1 = Color.FromArgb(44, 44, 44);
            DGV_IH_result.StateDisabled.Background.Color2 = Color.FromArgb(44, 44, 44);
            DGV_IH_result.StateDisabled.DataCell.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_IH_result.StateDisabled.DataCell.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_IH_result.StateDisabled.DataCell.Border.Color1 = Color.White;
            DGV_IH_result.StateDisabled.DataCell.Border.Color2 = Color.White;
            DGV_IH_result.StateDisabled.DataCell.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_IH_result.StateDisabled.DataCell.Content.Color1 = Color.White;
            DGV_IH_result.StateDisabled.DataCell.Content.Color2 = Color.White;
            DGV_IH_result.StateDisabled.HeaderColumn.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_IH_result.StateDisabled.HeaderColumn.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_IH_result.StateDisabled.HeaderColumn.Border.Color1 = Color.White;
            DGV_IH_result.StateDisabled.HeaderColumn.Border.Color2 = Color.White;
            DGV_IH_result.StateDisabled.HeaderColumn.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_IH_result.StateDisabled.HeaderColumn.Content.Color1 = Color.White;
            DGV_IH_result.StateDisabled.HeaderColumn.Content.Color2 = Color.White;
            DGV_IH_result.StateDisabled.HeaderRow.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_IH_result.StateDisabled.HeaderRow.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_IH_result.StateDisabled.HeaderRow.Border.Color1 = Color.White;
            DGV_IH_result.StateDisabled.HeaderRow.Border.Color2 = Color.White;
            DGV_IH_result.StateDisabled.HeaderRow.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_IH_result.StateDisabled.HeaderRow.Content.Color1 = Color.White;
            DGV_IH_result.StateDisabled.HeaderRow.Content.Color2 = Color.White;
            DGV_IH_result.StateNormal.Background.Color1 = Color.FromArgb(44, 44, 44);
            DGV_IH_result.StateNormal.Background.Color2 = Color.FromArgb(44, 44, 44);
            DGV_IH_result.StateNormal.DataCell.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_IH_result.StateNormal.DataCell.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_IH_result.StateNormal.DataCell.Border.Color1 = Color.White;
            DGV_IH_result.StateNormal.DataCell.Border.Color2 = Color.White;
            DGV_IH_result.StateNormal.DataCell.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_IH_result.StateNormal.DataCell.Content.Color1 = Color.White;
            DGV_IH_result.StateNormal.DataCell.Content.Color2 = Color.White;
            DGV_IH_result.StateNormal.HeaderColumn.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_IH_result.StateNormal.HeaderColumn.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_IH_result.StateNormal.HeaderColumn.Border.Color1 = Color.White;
            DGV_IH_result.StateNormal.HeaderColumn.Border.Color2 = Color.White;
            DGV_IH_result.StateNormal.HeaderColumn.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_IH_result.StateNormal.HeaderColumn.Content.Color1 = Color.White;
            DGV_IH_result.StateNormal.HeaderColumn.Content.Color2 = Color.White;
            DGV_IH_result.StateNormal.HeaderRow.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_IH_result.StateNormal.HeaderRow.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_IH_result.StateNormal.HeaderRow.Border.Color1 = Color.White;
            DGV_IH_result.StateNormal.HeaderRow.Border.Color2 = Color.White;
            DGV_IH_result.StateNormal.HeaderRow.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_IH_result.StateNormal.HeaderRow.Content.Color1 = Color.White;
            DGV_IH_result.StateNormal.HeaderRow.Content.Color2 = Color.White;
            DGV_IH_result.StatePressed.HeaderColumn.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_IH_result.StatePressed.HeaderColumn.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_IH_result.StatePressed.HeaderColumn.Border.Color1 = Color.White;
            DGV_IH_result.StatePressed.HeaderColumn.Border.Color2 = Color.White;
            DGV_IH_result.StatePressed.HeaderColumn.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_IH_result.StatePressed.HeaderColumn.Content.Color1 = Color.White;
            DGV_IH_result.StatePressed.HeaderColumn.Content.Color2 = Color.White;
            DGV_IH_result.StatePressed.HeaderRow.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_IH_result.StatePressed.HeaderRow.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_IH_result.StatePressed.HeaderRow.Border.Color1 = Color.White;
            DGV_IH_result.StatePressed.HeaderRow.Border.Color2 = Color.White;
            DGV_IH_result.StatePressed.HeaderRow.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_IH_result.StatePressed.HeaderRow.Content.Color1 = Color.White;
            DGV_IH_result.StatePressed.HeaderRow.Content.Color2 = Color.White;
            DGV_IH_result.StateSelected.DataCell.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_IH_result.StateSelected.DataCell.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_IH_result.StateSelected.DataCell.Border.Color1 = Color.White;
            DGV_IH_result.StateSelected.DataCell.Border.Color2 = Color.White;
            DGV_IH_result.StateSelected.DataCell.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_IH_result.StateSelected.DataCell.Content.Color1 = Color.White;
            DGV_IH_result.StateSelected.DataCell.Content.Color2 = Color.White;
            DGV_IH_result.StateSelected.HeaderColumn.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_IH_result.StateSelected.HeaderColumn.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_IH_result.StateSelected.HeaderColumn.Border.Color1 = Color.White;
            DGV_IH_result.StateSelected.HeaderColumn.Border.Color2 = Color.White;
            DGV_IH_result.StateSelected.HeaderColumn.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_IH_result.StateSelected.HeaderColumn.Content.Color1 = Color.White;
            DGV_IH_result.StateSelected.HeaderColumn.Content.Color2 = Color.White;
            DGV_IH_result.StateSelected.HeaderRow.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_IH_result.StateSelected.HeaderRow.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_IH_result.StateSelected.HeaderRow.Border.Color1 = Color.White;
            DGV_IH_result.StateSelected.HeaderRow.Border.Color2 = Color.White;
            DGV_IH_result.StateSelected.HeaderRow.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_IH_result.StateSelected.HeaderRow.Content.Color1 = Color.White;
            DGV_IH_result.StateSelected.HeaderRow.Content.Color2 = Color.White;
            DGV_IH_result.StateTracking.HeaderColumn.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_IH_result.StateTracking.HeaderColumn.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_IH_result.StateTracking.HeaderColumn.Border.Color1 = Color.White;
            DGV_IH_result.StateTracking.HeaderColumn.Border.Color2 = Color.White;
            DGV_IH_result.StateTracking.HeaderColumn.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_IH_result.StateTracking.HeaderColumn.Content.Color1 = Color.White;
            DGV_IH_result.StateTracking.HeaderColumn.Content.Color2 = Color.White;
            DGV_IH_result.StateTracking.HeaderRow.Back.Color1 = Color.FromArgb(44, 44, 44);
            DGV_IH_result.StateTracking.HeaderRow.Back.Color2 = Color.FromArgb(44, 44, 44);
            DGV_IH_result.StateTracking.HeaderRow.Border.Color1 = Color.White;
            DGV_IH_result.StateTracking.HeaderRow.Border.Color2 = Color.White;
            DGV_IH_result.StateTracking.HeaderRow.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            DGV_IH_result.StateTracking.HeaderRow.Content.Color1 = Color.White;
            DGV_IH_result.StateTracking.HeaderRow.Content.Color2 = Color.White;
            DGV_IH_result.TabIndex = 0;
            // 
            // button1
            // 
            button1.Location = new Point(1506, -16);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 6;
            button1.Text = "button1";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // kryptonManager1
            // 
            kryptonManager1.GlobalPaletteMode = ComponentFactory.Krypton.Toolkit.PaletteModeManager.Office2010Black;
            // 
            // InspectionHistoryView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(64, 64, 64);
            Controls.Add(cardSearchresult);
            Controls.Add(cardfilter);
            Name = "InspectionHistoryView";
            Size = new Size(1600, 800);
            Load += InspectionHistoryView_Load;
            cardfilter.ResumeLayout(false);
            cardfilter.PerformLayout();
            cardCameraID.ResumeLayout(false);
            cardCameraID.PerformLayout();
            cardDefectType.ResumeLayout(false);
            cardDefectType.PerformLayout();
            cardday.ResumeLayout(false);
            cardday.PerformLayout();
            cardSearchresult.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)DGV_IH_result).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel cardfilter;
        private Panel cardSearchresult;
        private Label label_IH;
        private Label label_IH_DatelRange;
        private Button btn_filterSearch;
        private ComponentFactory.Krypton.Toolkit.KryptonManager kryptonManager1;
        private ComponentFactory.Krypton.Toolkit.KryptonDateTimePicker DTP_IH_StartDate;
        private ComponentFactory.Krypton.Toolkit.KryptonDateTimePicker DTP_IH_EndDate;
        private Panel cardday;
        private Panel cardCameraID;
        private CheckBox CB_IH_CameraID_All;
        private CheckBox CB_IH_CameraID_CAM02;
        private CheckBox CB_IH_CameraID_CAM01;
        private Label label_IH_CameraID;
        private Panel cardDefectType;
        private CheckBox CB_IH_DefectType_All;
        private CheckBox CB_IH_DefectType_Scrap;
        private CheckBox CB_IH_DefectType_SolderingDefect;
        private CheckBox CB_IH_DefectType_ComponentDefect;
        private CheckBox CB_IH_DefectType_Normal;
        private Label label_IH_DefectType;
        private TenTec.Windows.iGridLib.iGCellStyle iGrid1DefaultCellStyle1;
        private TenTec.Windows.iGridLib.iGColHdrStyle iGrid1DefaultColHdrStyle1;
        private ComponentFactory.Krypton.Toolkit.KryptonDataGridView DGV_IH_result;
        private Button button1;
        private CheckBox CB_IH_CameraID_CAM03;
        private Button btn_Last7DaysSearch;
        private Button btn_ThisMonthSearch;
        private Button btn_TodaySearch;
        private Button btn_AllSearch;
    }
}
