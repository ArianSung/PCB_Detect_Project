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
            panel2 = new Panel();
            checkBox16 = new CheckBox();
            checkBox15 = new CheckBox();
            checkBox8 = new CheckBox();
            checkBox7 = new CheckBox();
            checkBox14 = new CheckBox();
            label7 = new Label();
            panel1 = new Panel();
            checkBox4 = new CheckBox();
            checkBox5 = new CheckBox();
            checkBox6 = new CheckBox();
            label5 = new Label();
            cardCameraID = new Panel();
            checkBox1 = new CheckBox();
            checkBox2 = new CheckBox();
            checkBox3 = new CheckBox();
            label3 = new Label();
            cardDefectType = new Panel();
            checkBox9 = new CheckBox();
            checkBox10 = new CheckBox();
            checkBox11 = new CheckBox();
            checkBox12 = new CheckBox();
            checkBox13 = new CheckBox();
            label6 = new Label();
            btn_filterSearch = new Button();
            label1 = new Label();
            cardday = new Panel();
            kryptonDateTimePicker2 = new ComponentFactory.Krypton.Toolkit.KryptonDateTimePicker();
            label2 = new Label();
            label4 = new Label();
            kryptonDateTimePicker1 = new ComponentFactory.Krypton.Toolkit.KryptonDateTimePicker();
            cardSearchresult = new Panel();
            kryptonDataGridView1 = new ComponentFactory.Krypton.Toolkit.KryptonDataGridView();
            date = new DataGridViewTextBoxColumn();
            time = new DataGridViewTextBoxColumn();
            CameraID = new DataGridViewTextBoxColumn();
            PCBID = new DataGridViewTextBoxColumn();
            DefectType = new DataGridViewTextBoxColumn();
            DefectLocation = new DataGridViewTextBoxColumn();
            productionline = new DataGridViewTextBoxColumn();
            iGrid1DefaultCellStyle1 = new TenTec.Windows.iGridLib.iGCellStyle(true);
            iGrid1DefaultColHdrStyle1 = new TenTec.Windows.iGridLib.iGColHdrStyle(true);
            kryptonManager1 = new ComponentFactory.Krypton.Toolkit.KryptonManager(components);
            button1 = new Button();
            cardfilter.SuspendLayout();
            panel2.SuspendLayout();
            panel1.SuspendLayout();
            cardCameraID.SuspendLayout();
            cardDefectType.SuspendLayout();
            cardday.SuspendLayout();
            cardSearchresult.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)kryptonDataGridView1).BeginInit();
            SuspendLayout();
            // 
            // cardfilter
            // 
            cardfilter.Controls.Add(panel2);
            cardfilter.Controls.Add(panel1);
            cardfilter.Controls.Add(cardCameraID);
            cardfilter.Controls.Add(cardDefectType);
            cardfilter.Controls.Add(btn_filterSearch);
            cardfilter.Controls.Add(label1);
            cardfilter.Controls.Add(cardday);
            cardfilter.Location = new Point(0, 40);
            cardfilter.Name = "cardfilter";
            cardfilter.Size = new Size(578, 700);
            cardfilter.TabIndex = 4;
            cardfilter.Paint += cardfilter_Paint;
            // 
            // panel2
            // 
            panel2.Controls.Add(checkBox16);
            panel2.Controls.Add(checkBox15);
            panel2.Controls.Add(checkBox8);
            panel2.Controls.Add(checkBox7);
            panel2.Controls.Add(checkBox14);
            panel2.Controls.Add(label7);
            panel2.Location = new Point(0, 535);
            panel2.Name = "panel2";
            panel2.Size = new Size(555, 111);
            panel2.TabIndex = 13;
            // 
            // checkBox16
            // 
            checkBox16.AutoSize = true;
            checkBox16.Font = new Font("맑은 고딕", 14.25F, FontStyle.Bold);
            checkBox16.ForeColor = Color.White;
            checkBox16.Location = new Point(359, 55);
            checkBox16.Name = "checkBox16";
            checkBox16.Size = new Size(80, 29);
            checkBox16.TabIndex = 12;
            checkBox16.Text = "라인4";
            checkBox16.UseVisualStyleBackColor = true;
            // 
            // checkBox15
            // 
            checkBox15.AutoSize = true;
            checkBox15.Font = new Font("맑은 고딕", 14.25F, FontStyle.Bold);
            checkBox15.ForeColor = Color.White;
            checkBox15.Location = new Point(273, 55);
            checkBox15.Name = "checkBox15";
            checkBox15.Size = new Size(80, 29);
            checkBox15.TabIndex = 11;
            checkBox15.Text = "라인3";
            checkBox15.UseVisualStyleBackColor = true;
            // 
            // checkBox8
            // 
            checkBox8.AutoSize = true;
            checkBox8.Font = new Font("맑은 고딕", 14.25F, FontStyle.Bold);
            checkBox8.ForeColor = Color.White;
            checkBox8.Location = new Point(187, 55);
            checkBox8.Name = "checkBox8";
            checkBox8.Size = new Size(80, 29);
            checkBox8.TabIndex = 10;
            checkBox8.Text = "라인2";
            checkBox8.UseVisualStyleBackColor = true;
            // 
            // checkBox7
            // 
            checkBox7.AutoSize = true;
            checkBox7.Font = new Font("맑은 고딕", 14.25F, FontStyle.Bold);
            checkBox7.ForeColor = Color.White;
            checkBox7.Location = new Point(15, 55);
            checkBox7.Name = "checkBox7";
            checkBox7.Size = new Size(69, 29);
            checkBox7.TabIndex = 9;
            checkBox7.Text = "전체";
            checkBox7.UseVisualStyleBackColor = true;
            // 
            // checkBox14
            // 
            checkBox14.AutoSize = true;
            checkBox14.Font = new Font("맑은 고딕", 14.25F, FontStyle.Bold);
            checkBox14.ForeColor = Color.White;
            checkBox14.Location = new Point(101, 55);
            checkBox14.Name = "checkBox14";
            checkBox14.Size = new Size(80, 29);
            checkBox14.TabIndex = 7;
            checkBox14.Text = "라인1";
            checkBox14.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("맑은 고딕", 18F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label7.ForeColor = Color.White;
            label7.Location = new Point(0, 3);
            label7.Name = "label7";
            label7.Size = new Size(118, 32);
            label7.TabIndex = 0;
            label7.Text = "생산 라인";
            // 
            // panel1
            // 
            panel1.Controls.Add(checkBox4);
            panel1.Controls.Add(checkBox5);
            panel1.Controls.Add(checkBox6);
            panel1.Controls.Add(label5);
            panel1.Location = new Point(0, 418);
            panel1.Name = "panel1";
            panel1.Size = new Size(555, 111);
            panel1.TabIndex = 12;
            // 
            // checkBox4
            // 
            checkBox4.AutoSize = true;
            checkBox4.Font = new Font("맑은 고딕", 14.25F, FontStyle.Bold);
            checkBox4.ForeColor = Color.White;
            checkBox4.Location = new Point(15, 55);
            checkBox4.Name = "checkBox4";
            checkBox4.Size = new Size(69, 29);
            checkBox4.TabIndex = 9;
            checkBox4.Text = "전체";
            checkBox4.UseVisualStyleBackColor = true;
            // 
            // checkBox5
            // 
            checkBox5.AutoSize = true;
            checkBox5.Font = new Font("맑은 고딕", 14.25F, FontStyle.Bold);
            checkBox5.ForeColor = Color.White;
            checkBox5.Location = new Point(254, 55);
            checkBox5.Name = "checkBox5";
            checkBox5.Size = new Size(126, 29);
            checkBox5.TabIndex = 8;
            checkBox5.Text = "뒷면카메라";
            checkBox5.UseVisualStyleBackColor = true;
            // 
            // checkBox6
            // 
            checkBox6.AutoSize = true;
            checkBox6.Font = new Font("맑은 고딕", 14.25F, FontStyle.Bold);
            checkBox6.ForeColor = Color.White;
            checkBox6.Location = new Point(101, 55);
            checkBox6.Name = "checkBox6";
            checkBox6.Size = new Size(126, 29);
            checkBox6.TabIndex = 7;
            checkBox6.Text = "앞면카메라";
            checkBox6.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("맑은 고딕", 18F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label5.ForeColor = Color.White;
            label5.Location = new Point(0, 3);
            label5.Name = "label5";
            label5.Size = new Size(118, 32);
            label5.TabIndex = 0;
            label5.Text = "불량 위치";
            // 
            // cardCameraID
            // 
            cardCameraID.Controls.Add(checkBox1);
            cardCameraID.Controls.Add(checkBox2);
            cardCameraID.Controls.Add(checkBox3);
            cardCameraID.Controls.Add(label3);
            cardCameraID.Location = new Point(0, 301);
            cardCameraID.Name = "cardCameraID";
            cardCameraID.Size = new Size(555, 111);
            cardCameraID.TabIndex = 11;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Font = new Font("맑은 고딕", 14.25F, FontStyle.Bold);
            checkBox1.ForeColor = Color.White;
            checkBox1.Location = new Point(15, 55);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(69, 29);
            checkBox1.TabIndex = 9;
            checkBox1.Text = "전체";
            checkBox1.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            checkBox2.AutoSize = true;
            checkBox2.Font = new Font("맑은 고딕", 14.25F, FontStyle.Bold);
            checkBox2.ForeColor = Color.White;
            checkBox2.Location = new Point(292, 55);
            checkBox2.Name = "checkBox2";
            checkBox2.Size = new Size(126, 29);
            checkBox2.TabIndex = 8;
            checkBox2.Text = "뒷면카메라";
            checkBox2.UseVisualStyleBackColor = true;
            // 
            // checkBox3
            // 
            checkBox3.AutoSize = true;
            checkBox3.Font = new Font("맑은 고딕", 14.25F, FontStyle.Bold);
            checkBox3.ForeColor = Color.White;
            checkBox3.Location = new Point(125, 55);
            checkBox3.Name = "checkBox3";
            checkBox3.Size = new Size(126, 29);
            checkBox3.TabIndex = 7;
            checkBox3.Text = "앞면카메라";
            checkBox3.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("맑은 고딕", 18F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label3.ForeColor = Color.White;
            label3.Location = new Point(0, 3);
            label3.Name = "label3";
            label3.Size = new Size(119, 32);
            label3.TabIndex = 0;
            label3.Text = "카메라 ID";
            // 
            // cardDefectType
            // 
            cardDefectType.Controls.Add(checkBox9);
            cardDefectType.Controls.Add(checkBox10);
            cardDefectType.Controls.Add(checkBox11);
            cardDefectType.Controls.Add(checkBox12);
            cardDefectType.Controls.Add(checkBox13);
            cardDefectType.Controls.Add(label6);
            cardDefectType.Location = new Point(0, 184);
            cardDefectType.Name = "cardDefectType";
            cardDefectType.Size = new Size(555, 111);
            cardDefectType.TabIndex = 10;
            // 
            // checkBox9
            // 
            checkBox9.AutoSize = true;
            checkBox9.Font = new Font("맑은 고딕", 14.25F, FontStyle.Bold);
            checkBox9.ForeColor = Color.White;
            checkBox9.Location = new Point(15, 55);
            checkBox9.Name = "checkBox9";
            checkBox9.Size = new Size(69, 29);
            checkBox9.TabIndex = 9;
            checkBox9.Text = "전체";
            checkBox9.UseVisualStyleBackColor = true;
            // 
            // checkBox10
            // 
            checkBox10.AutoSize = true;
            checkBox10.Font = new Font("맑은 고딕", 14.25F, FontStyle.Bold);
            checkBox10.ForeColor = Color.White;
            checkBox10.Location = new Point(435, 55);
            checkBox10.Name = "checkBox10";
            checkBox10.Size = new Size(69, 29);
            checkBox10.TabIndex = 8;
            checkBox10.Text = "폐기";
            checkBox10.UseVisualStyleBackColor = true;
            // 
            // checkBox11
            // 
            checkBox11.AutoSize = true;
            checkBox11.Font = new Font("맑은 고딕", 14.25F, FontStyle.Bold);
            checkBox11.ForeColor = Color.White;
            checkBox11.Location = new Point(311, 55);
            checkBox11.Name = "checkBox11";
            checkBox11.Size = new Size(107, 29);
            checkBox11.TabIndex = 7;
            checkBox11.Text = "납땜불량";
            checkBox11.UseVisualStyleBackColor = true;
            // 
            // checkBox12
            // 
            checkBox12.AutoSize = true;
            checkBox12.Font = new Font("맑은 고딕", 14.25F, FontStyle.Bold);
            checkBox12.ForeColor = Color.White;
            checkBox12.Location = new Point(187, 55);
            checkBox12.Name = "checkBox12";
            checkBox12.Size = new Size(107, 29);
            checkBox12.TabIndex = 6;
            checkBox12.Text = "부품불량";
            checkBox12.UseVisualStyleBackColor = true;
            // 
            // checkBox13
            // 
            checkBox13.AutoSize = true;
            checkBox13.Font = new Font("맑은 고딕", 14.25F, FontStyle.Bold);
            checkBox13.ForeColor = Color.White;
            checkBox13.Location = new Point(101, 55);
            checkBox13.Name = "checkBox13";
            checkBox13.Size = new Size(69, 29);
            checkBox13.TabIndex = 5;
            checkBox13.Text = "정상";
            checkBox13.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("맑은 고딕", 18F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label6.ForeColor = Color.White;
            label6.Location = new Point(0, 3);
            label6.Name = "label6";
            label6.Size = new Size(118, 32);
            label6.TabIndex = 0;
            label6.Text = "불량 유형";
            // 
            // btn_filterSearch
            // 
            btn_filterSearch.Font = new Font("Arial", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btn_filterSearch.Location = new Point(485, 21);
            btn_filterSearch.Name = "btn_filterSearch";
            btn_filterSearch.Size = new Size(73, 40);
            btn_filterSearch.TabIndex = 5;
            btn_filterSearch.Text = "Search";
            btn_filterSearch.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("맑은 고딕", 20.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label1.ForeColor = Color.White;
            label1.Location = new Point(0, 0);
            label1.Name = "label1";
            label1.Size = new Size(134, 37);
            label1.TabIndex = 0;
            label1.Text = "검색 필터";
            // 
            // cardday
            // 
            cardday.Controls.Add(kryptonDateTimePicker2);
            cardday.Controls.Add(label2);
            cardday.Controls.Add(label4);
            cardday.Controls.Add(kryptonDateTimePicker1);
            cardday.Location = new Point(0, 67);
            cardday.Name = "cardday";
            cardday.Size = new Size(555, 111);
            cardday.TabIndex = 7;
            // 
            // kryptonDateTimePicker2
            // 
            kryptonDateTimePicker2.Location = new Point(296, 60);
            kryptonDateTimePicker2.Name = "kryptonDateTimePicker2";
            kryptonDateTimePicker2.Size = new Size(248, 31);
            kryptonDateTimePicker2.StateCommon.Back.Color1 = Color.FromArgb(44, 44, 44);
            kryptonDateTimePicker2.StateCommon.Border.Color1 = Color.FromArgb(44, 44, 44);
            kryptonDateTimePicker2.StateCommon.Border.Color2 = Color.FromArgb(44, 44, 44);
            kryptonDateTimePicker2.StateCommon.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            kryptonDateTimePicker2.StateCommon.Content.Color1 = Color.White;
            kryptonDateTimePicker2.StateCommon.Content.Font = new Font("맑은 고딕", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            kryptonDateTimePicker2.TabIndex = 9;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("맑은 고딕", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label2.ForeColor = Color.White;
            label2.Location = new Point(254, 50);
            label2.Name = "label2";
            label2.Size = new Size(26, 25);
            label2.TabIndex = 3;
            label2.Text = "~";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("맑은 고딕", 18F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label4.ForeColor = Color.White;
            label4.Location = new Point(0, 3);
            label4.Name = "label4";
            label4.Size = new Size(62, 32);
            label4.TabIndex = 0;
            label4.Text = "날짜";
            // 
            // kryptonDateTimePicker1
            // 
            kryptonDateTimePicker1.Location = new Point(15, 60);
            kryptonDateTimePicker1.Name = "kryptonDateTimePicker1";
            kryptonDateTimePicker1.Size = new Size(248, 31);
            kryptonDateTimePicker1.StateCommon.Back.Color1 = Color.FromArgb(44, 44, 44);
            kryptonDateTimePicker1.StateCommon.Border.Color1 = Color.FromArgb(44, 44, 44);
            kryptonDateTimePicker1.StateCommon.Border.Color2 = Color.FromArgb(44, 44, 44);
            kryptonDateTimePicker1.StateCommon.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            kryptonDateTimePicker1.StateCommon.Content.Color1 = Color.White;
            kryptonDateTimePicker1.StateCommon.Content.Font = new Font("맑은 고딕", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            kryptonDateTimePicker1.TabIndex = 8;
            // 
            // cardSearchresult
            // 
            cardSearchresult.Controls.Add(kryptonDataGridView1);
            cardSearchresult.Location = new Point(599, 40);
            cardSearchresult.Name = "cardSearchresult";
            cardSearchresult.Size = new Size(1000, 700);
            cardSearchresult.TabIndex = 5;
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
            kryptonDataGridView1.Dock = DockStyle.Fill;
            kryptonDataGridView1.Location = new Point(0, 0);
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
            kryptonDataGridView1.Size = new Size(1000, 700);
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
            kryptonDataGridView1.TabIndex = 0;
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
            // kryptonManager1
            // 
            kryptonManager1.GlobalPaletteMode = ComponentFactory.Krypton.Toolkit.PaletteModeManager.Office2010Black;
            // 
            // button1
            // 
            button1.Location = new Point(601, 15);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 6;
            button1.Text = "button1";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // InspectionHistoryView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(64, 64, 64);
            Controls.Add(button1);
            Controls.Add(cardSearchresult);
            Controls.Add(cardfilter);
            Name = "InspectionHistoryView";
            Size = new Size(1600, 800);
            Load += InspectionHistoryView_Load;
            cardfilter.ResumeLayout(false);
            cardfilter.PerformLayout();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            cardCameraID.ResumeLayout(false);
            cardCameraID.PerformLayout();
            cardDefectType.ResumeLayout(false);
            cardDefectType.PerformLayout();
            cardday.ResumeLayout(false);
            cardday.PerformLayout();
            cardSearchresult.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)kryptonDataGridView1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel cardfilter;
        private Panel cardSearchresult;
        private Label label1;
        private Label label2;
        private Button btn_filterSearch;
        private ComponentFactory.Krypton.Toolkit.KryptonManager kryptonManager1;
        private ComponentFactory.Krypton.Toolkit.KryptonDateTimePicker kryptonDateTimePicker1;
        private ComponentFactory.Krypton.Toolkit.KryptonDateTimePicker kryptonDateTimePicker2;
        private Panel cardday;
        private Label label4;
        private Panel cardCameraID;
        private CheckBox checkBox1;
        private CheckBox checkBox2;
        private CheckBox checkBox3;
        private Label label3;
        private Panel cardDefectType;
        private CheckBox checkBox9;
        private CheckBox checkBox10;
        private CheckBox checkBox11;
        private CheckBox checkBox12;
        private CheckBox checkBox13;
        private Label label6;
        private TenTec.Windows.iGridLib.iGCellStyle iGrid1DefaultCellStyle1;
        private TenTec.Windows.iGridLib.iGColHdrStyle iGrid1DefaultColHdrStyle1;
        private ComponentFactory.Krypton.Toolkit.KryptonDataGridView kryptonDataGridView1;
        private Panel panel1;
        private CheckBox checkBox4;
        private CheckBox checkBox5;
        private CheckBox checkBox6;
        private Label label5;
        private Panel panel2;
        private CheckBox checkBox7;
        private CheckBox checkBox14;
        private Label label7;
        private DataGridViewTextBoxColumn date;
        private DataGridViewTextBoxColumn time;
        private DataGridViewTextBoxColumn CameraID;
        private DataGridViewTextBoxColumn PCBID;
        private DataGridViewTextBoxColumn DefectType;
        private DataGridViewTextBoxColumn DefectLocation;
        private DataGridViewTextBoxColumn productionline;
        private CheckBox checkBox16;
        private CheckBox checkBox15;
        private CheckBox checkBox8;
        private Button button1;
    }
}
