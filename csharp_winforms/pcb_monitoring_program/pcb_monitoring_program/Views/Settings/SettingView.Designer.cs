namespace pcb_monitoring_program.Views.Settings
{
    partial class SettingView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingView));
            cardSetting = new Panel();
            cardMySQL = new Panel();
            textBox_pw = new TextBox();
            textBox_username = new TextBox();
            textBox_DB = new TextBox();
            textBox_port = new TextBox();
            textBox_host = new TextBox();
            btn_Setting_Connectiontest = new Button();
            label_MySQL_PW = new Label();
            panel_MySQL_PW = new Panel();
            label_MySQL_UserName = new Label();
            panel_MySQL_UserName = new Panel();
            label_MySQL_DB = new Label();
            panel_MySQL_DB = new Panel();
            label_MySQL_port = new Label();
            panel_MySQL_port = new Panel();
            label_MySQL_host = new Label();
            panel_MySQL_host = new Panel();
            label_MySQL = new Label();
            cardLoglevel = new Panel();
            label_LogLevel_Level = new Label();
            kComboBox_Loglevel = new ComponentFactory.Krypton.Toolkit.KryptonComboBox();
            label_LogLevel = new Label();
            panel_Loglevel = new Panel();
            cardTimeout = new Panel();
            panel_Timeout = new Panel();
            label_TimeOut = new Label();
            textBox_timeout = new TextBox();
            label_TimeOut_Minute = new Label();
            cardAlarm = new Panel();
            checkBox_popup = new CheckBox();
            label_Alarm = new Label();
            checkBox_email = new CheckBox();
            textBox_defectrate = new TextBox();
            label_Alarm_Alarmmethod = new Label();
            panel_Alarm_defectrate = new Panel();
            panel_Alarm_Alarmmethod = new Panel();
            label_Alarm_defectrate = new Label();
            cardFlaskServer = new Panel();
            label_flaskServer_URL = new Label();
            TextBox_flaskserver = new TextBox();
            label_flaskServer = new Label();
            panel_flaskServer = new Panel();
            pictureBox1 = new PictureBox();
            btn_Setting_cancel = new Button();
            btn_Setting_save = new Button();
            textBox1 = new TextBox();
            cardSetting.SuspendLayout();
            cardMySQL.SuspendLayout();
            cardLoglevel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)kComboBox_Loglevel).BeginInit();
            cardTimeout.SuspendLayout();
            cardAlarm.SuspendLayout();
            cardFlaskServer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // cardSetting
            // 
            cardSetting.BackColor = Color.FromArgb(44, 44, 44);
            cardSetting.Controls.Add(cardMySQL);
            cardSetting.Controls.Add(cardLoglevel);
            cardSetting.Controls.Add(cardTimeout);
            cardSetting.Controls.Add(cardAlarm);
            cardSetting.Controls.Add(cardFlaskServer);
            cardSetting.Controls.Add(pictureBox1);
            cardSetting.Controls.Add(btn_Setting_cancel);
            cardSetting.Controls.Add(btn_Setting_save);
            cardSetting.Controls.Add(textBox1);
            cardSetting.Location = new Point(280, 100);
            cardSetting.Name = "cardSetting";
            cardSetting.Size = new Size(1100, 700);
            cardSetting.TabIndex = 6;
            // 
            // cardMySQL
            // 
            cardMySQL.BackColor = Color.FromArgb(44, 44, 44);
            cardMySQL.Controls.Add(textBox_pw);
            cardMySQL.Controls.Add(textBox_username);
            cardMySQL.Controls.Add(textBox_DB);
            cardMySQL.Controls.Add(textBox_port);
            cardMySQL.Controls.Add(textBox_host);
            cardMySQL.Controls.Add(btn_Setting_Connectiontest);
            cardMySQL.Controls.Add(label_MySQL_PW);
            cardMySQL.Controls.Add(panel_MySQL_PW);
            cardMySQL.Controls.Add(label_MySQL_UserName);
            cardMySQL.Controls.Add(panel_MySQL_UserName);
            cardMySQL.Controls.Add(label_MySQL_DB);
            cardMySQL.Controls.Add(panel_MySQL_DB);
            cardMySQL.Controls.Add(label_MySQL_port);
            cardMySQL.Controls.Add(panel_MySQL_port);
            cardMySQL.Controls.Add(label_MySQL_host);
            cardMySQL.Controls.Add(panel_MySQL_host);
            cardMySQL.Controls.Add(label_MySQL);
            cardMySQL.Location = new Point(567, 109);
            cardMySQL.Name = "cardMySQL";
            cardMySQL.Size = new Size(506, 373);
            cardMySQL.TabIndex = 53;
            // 
            // textBox_pw
            // 
            textBox_pw.BackColor = Color.FromArgb(44, 44, 44);
            textBox_pw.BorderStyle = BorderStyle.None;
            textBox_pw.Font = new Font("Arial", 15.75F);
            textBox_pw.ForeColor = SystemColors.Window;
            textBox_pw.Location = new Point(200, 303);
            textBox_pw.Name = "textBox_pw";
            textBox_pw.Size = new Size(254, 25);
            textBox_pw.TabIndex = 68;
            // 
            // textBox_username
            // 
            textBox_username.BackColor = Color.FromArgb(44, 44, 44);
            textBox_username.BorderStyle = BorderStyle.None;
            textBox_username.Font = new Font("Arial", 15.75F);
            textBox_username.ForeColor = SystemColors.Window;
            textBox_username.Location = new Point(200, 255);
            textBox_username.Name = "textBox_username";
            textBox_username.Size = new Size(254, 25);
            textBox_username.TabIndex = 67;
            // 
            // textBox_DB
            // 
            textBox_DB.BackColor = Color.FromArgb(44, 44, 44);
            textBox_DB.BorderStyle = BorderStyle.None;
            textBox_DB.Font = new Font("Arial", 15.75F);
            textBox_DB.ForeColor = SystemColors.Window;
            textBox_DB.Location = new Point(199, 202);
            textBox_DB.Name = "textBox_DB";
            textBox_DB.Size = new Size(254, 25);
            textBox_DB.TabIndex = 66;
            // 
            // textBox_port
            // 
            textBox_port.BackColor = Color.FromArgb(44, 44, 44);
            textBox_port.BorderStyle = BorderStyle.None;
            textBox_port.Font = new Font("Arial", 15.75F);
            textBox_port.ForeColor = SystemColors.Window;
            textBox_port.Location = new Point(200, 147);
            textBox_port.Name = "textBox_port";
            textBox_port.Size = new Size(254, 25);
            textBox_port.TabIndex = 65;
            // 
            // textBox_host
            // 
            textBox_host.BackColor = Color.FromArgb(44, 44, 44);
            textBox_host.BorderStyle = BorderStyle.None;
            textBox_host.Font = new Font("Arial", 15.75F);
            textBox_host.ForeColor = SystemColors.Window;
            textBox_host.Location = new Point(200, 94);
            textBox_host.Name = "textBox_host";
            textBox_host.Size = new Size(254, 25);
            textBox_host.TabIndex = 64;
            // 
            // btn_Setting_Connectiontest
            // 
            btn_Setting_Connectiontest.BackColor = Color.FromArgb(44, 44, 44);
            btn_Setting_Connectiontest.FlatAppearance.BorderSize = 0;
            btn_Setting_Connectiontest.FlatStyle = FlatStyle.Flat;
            btn_Setting_Connectiontest.Font = new Font("Arial", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_Setting_Connectiontest.ForeColor = Color.White;
            btn_Setting_Connectiontest.Location = new Point(360, 21);
            btn_Setting_Connectiontest.Name = "btn_Setting_Connectiontest";
            btn_Setting_Connectiontest.Size = new Size(115, 50);
            btn_Setting_Connectiontest.TabIndex = 63;
            btn_Setting_Connectiontest.Text = "연결 테스트";
            btn_Setting_Connectiontest.UseVisualStyleBackColor = false;
            btn_Setting_Connectiontest.Click += btn_Setting_Connectiontest_Click;
            // 
            // label_MySQL_PW
            // 
            label_MySQL_PW.AutoSize = true;
            label_MySQL_PW.Font = new Font("맑은 고딕", 15.75F);
            label_MySQL_PW.ForeColor = SystemColors.Window;
            label_MySQL_PW.Location = new Point(26, 289);
            label_MySQL_PW.Name = "label_MySQL_PW";
            label_MySQL_PW.Size = new Size(113, 30);
            label_MySQL_PW.TabIndex = 62;
            label_MySQL_PW.Text = "- 비밀번호";
            // 
            // panel_MySQL_PW
            // 
            panel_MySQL_PW.BackColor = Color.Silver;
            panel_MySQL_PW.ForeColor = Color.Silver;
            panel_MySQL_PW.Location = new Point(20, 336);
            panel_MySQL_PW.Name = "panel_MySQL_PW";
            panel_MySQL_PW.Size = new Size(450, 4);
            panel_MySQL_PW.TabIndex = 61;
            // 
            // label_MySQL_UserName
            // 
            label_MySQL_UserName.AutoSize = true;
            label_MySQL_UserName.Font = new Font("맑은 고딕", 15.75F);
            label_MySQL_UserName.ForeColor = SystemColors.Window;
            label_MySQL_UserName.Location = new Point(26, 236);
            label_MySQL_UserName.Name = "label_MySQL_UserName";
            label_MySQL_UserName.Size = new Size(113, 30);
            label_MySQL_UserName.TabIndex = 60;
            label_MySQL_UserName.Text = "- 사용자명";
            // 
            // panel_MySQL_UserName
            // 
            panel_MySQL_UserName.BackColor = Color.Silver;
            panel_MySQL_UserName.ForeColor = Color.Silver;
            panel_MySQL_UserName.Location = new Point(20, 283);
            panel_MySQL_UserName.Name = "panel_MySQL_UserName";
            panel_MySQL_UserName.Size = new Size(450, 4);
            panel_MySQL_UserName.TabIndex = 59;
            // 
            // label_MySQL_DB
            // 
            label_MySQL_DB.AutoSize = true;
            label_MySQL_DB.Font = new Font("맑은 고딕", 15.75F);
            label_MySQL_DB.ForeColor = SystemColors.Window;
            label_MySQL_DB.Location = new Point(26, 183);
            label_MySQL_DB.Name = "label_MySQL_DB";
            label_MySQL_DB.Size = new Size(155, 30);
            label_MySQL_DB.TabIndex = 58;
            label_MySQL_DB.Text = "- 데이터베이스";
            // 
            // panel_MySQL_DB
            // 
            panel_MySQL_DB.BackColor = Color.Silver;
            panel_MySQL_DB.ForeColor = Color.Silver;
            panel_MySQL_DB.Location = new Point(20, 230);
            panel_MySQL_DB.Name = "panel_MySQL_DB";
            panel_MySQL_DB.Size = new Size(450, 4);
            panel_MySQL_DB.TabIndex = 57;
            // 
            // label_MySQL_port
            // 
            label_MySQL_port.AutoSize = true;
            label_MySQL_port.Font = new Font("맑은 고딕", 15.75F);
            label_MySQL_port.ForeColor = SystemColors.Window;
            label_MySQL_port.Location = new Point(26, 130);
            label_MySQL_port.Name = "label_MySQL_port";
            label_MySQL_port.Size = new Size(71, 30);
            label_MySQL_port.TabIndex = 56;
            label_MySQL_port.Text = "- 포트";
            // 
            // panel_MySQL_port
            // 
            panel_MySQL_port.BackColor = Color.Silver;
            panel_MySQL_port.ForeColor = Color.Silver;
            panel_MySQL_port.Location = new Point(20, 177);
            panel_MySQL_port.Name = "panel_MySQL_port";
            panel_MySQL_port.Size = new Size(450, 4);
            panel_MySQL_port.TabIndex = 55;
            // 
            // label_MySQL_host
            // 
            label_MySQL_host.AutoSize = true;
            label_MySQL_host.Font = new Font("맑은 고딕", 15.75F);
            label_MySQL_host.ForeColor = SystemColors.Window;
            label_MySQL_host.Location = new Point(26, 77);
            label_MySQL_host.Name = "label_MySQL_host";
            label_MySQL_host.Size = new Size(99, 30);
            label_MySQL_host.TabIndex = 54;
            label_MySQL_host.Text = "- 호스트 ";
            // 
            // panel_MySQL_host
            // 
            panel_MySQL_host.BackColor = Color.Silver;
            panel_MySQL_host.ForeColor = Color.Silver;
            panel_MySQL_host.Location = new Point(20, 124);
            panel_MySQL_host.Name = "panel_MySQL_host";
            panel_MySQL_host.Size = new Size(450, 4);
            panel_MySQL_host.TabIndex = 53;
            // 
            // label_MySQL
            // 
            label_MySQL.AutoSize = true;
            label_MySQL.Font = new Font("Arial", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_MySQL.ForeColor = SystemColors.Window;
            label_MySQL.Location = new Point(20, 16);
            label_MySQL.Name = "label_MySQL";
            label_MySQL.Size = new Size(268, 32);
            label_MySQL.TabIndex = 52;
            label_MySQL.Text = " MySQL 연결 정보 설정";
            // 
            // cardLoglevel
            // 
            cardLoglevel.BackColor = Color.FromArgb(44, 44, 44);
            cardLoglevel.Controls.Add(label_LogLevel_Level);
            cardLoglevel.Controls.Add(kComboBox_Loglevel);
            cardLoglevel.Controls.Add(label_LogLevel);
            cardLoglevel.Controls.Add(panel_Loglevel);
            cardLoglevel.Location = new Point(22, 549);
            cardLoglevel.Name = "cardLoglevel";
            cardLoglevel.Size = new Size(529, 113);
            cardLoglevel.TabIndex = 54;
            // 
            // label_LogLevel_Level
            // 
            label_LogLevel_Level.AutoSize = true;
            label_LogLevel_Level.Font = new Font("맑은 고딕", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 129);
            label_LogLevel_Level.ForeColor = SystemColors.Window;
            label_LogLevel_Level.Location = new Point(14, 52);
            label_LogLevel_Level.Name = "label_LogLevel_Level";
            label_LogLevel_Level.Size = new Size(71, 30);
            label_LogLevel_Level.TabIndex = 45;
            label_LogLevel_Level.Text = "- 레벨";
            // 
            // kComboBox_Loglevel
            // 
            kComboBox_Loglevel.DropDownWidth = 200;
            kComboBox_Loglevel.Location = new Point(173, 59);
            kComboBox_Loglevel.Name = "kComboBox_Loglevel";
            kComboBox_Loglevel.Size = new Size(336, 33);
            kComboBox_Loglevel.StateCommon.ComboBox.Back.Color1 = Color.FromArgb(44, 44, 44);
            kComboBox_Loglevel.StateCommon.ComboBox.Border.Color1 = Color.FromArgb(44, 44, 44);
            kComboBox_Loglevel.StateCommon.ComboBox.Border.Color2 = Color.FromArgb(44, 44, 44);
            kComboBox_Loglevel.StateCommon.ComboBox.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Top | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Bottom | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Left | ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.Right;
            kComboBox_Loglevel.StateCommon.ComboBox.Content.Color1 = Color.White;
            kComboBox_Loglevel.StateCommon.ComboBox.Content.Font = new Font("맑은 고딕", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 129);
            kComboBox_Loglevel.TabIndex = 44;
            kComboBox_Loglevel.Text = "Debug";
            // 
            // label_LogLevel
            // 
            label_LogLevel.AutoSize = true;
            label_LogLevel.Font = new Font("맑은 고딕", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            label_LogLevel.ForeColor = SystemColors.Window;
            label_LogLevel.Location = new Point(9, 5);
            label_LogLevel.Name = "label_LogLevel";
            label_LogLevel.Size = new Size(197, 37);
            label_LogLevel.TabIndex = 43;
            label_LogLevel.Text = "로그 레벨 설정";
            // 
            // panel_Loglevel
            // 
            panel_Loglevel.BackColor = Color.Silver;
            panel_Loglevel.ForeColor = Color.Silver;
            panel_Loglevel.Location = new Point(14, 99);
            panel_Loglevel.Name = "panel_Loglevel";
            panel_Loglevel.Size = new Size(500, 4);
            panel_Loglevel.TabIndex = 42;
            // 
            // cardTimeout
            // 
            cardTimeout.BackColor = Color.FromArgb(44, 44, 44);
            cardTimeout.Controls.Add(panel_Timeout);
            cardTimeout.Controls.Add(label_TimeOut);
            cardTimeout.Controls.Add(textBox_timeout);
            cardTimeout.Controls.Add(label_TimeOut_Minute);
            cardTimeout.Location = new Point(22, 428);
            cardTimeout.Name = "cardTimeout";
            cardTimeout.Size = new Size(529, 106);
            cardTimeout.TabIndex = 53;
            // 
            // panel_Timeout
            // 
            panel_Timeout.BackColor = Color.Silver;
            panel_Timeout.ForeColor = Color.Silver;
            panel_Timeout.Location = new Point(17, 92);
            panel_Timeout.Name = "panel_Timeout";
            panel_Timeout.Size = new Size(500, 4);
            panel_Timeout.TabIndex = 42;
            // 
            // label_TimeOut
            // 
            label_TimeOut.AutoSize = true;
            label_TimeOut.Font = new Font("맑은 고딕", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            label_TimeOut.ForeColor = SystemColors.Window;
            label_TimeOut.Location = new Point(9, 5);
            label_TimeOut.Name = "label_TimeOut";
            label_TimeOut.Size = new Size(260, 37);
            label_TimeOut.TabIndex = 41;
            label_TimeOut.Text = "세션 타임아웃  설정";
            // 
            // textBox_timeout
            // 
            textBox_timeout.BackColor = Color.FromArgb(44, 44, 44);
            textBox_timeout.BorderStyle = BorderStyle.None;
            textBox_timeout.Font = new Font("Arial", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBox_timeout.ForeColor = SystemColors.Window;
            textBox_timeout.Location = new Point(161, 64);
            textBox_timeout.Name = "textBox_timeout";
            textBox_timeout.Size = new Size(65, 25);
            textBox_timeout.TabIndex = 22;
            // 
            // label_TimeOut_Minute
            // 
            label_TimeOut_Minute.AutoSize = true;
            label_TimeOut_Minute.Font = new Font("맑은 고딕", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 129);
            label_TimeOut_Minute.ForeColor = SystemColors.Window;
            label_TimeOut_Minute.Location = new Point(17, 48);
            label_TimeOut_Minute.Name = "label_TimeOut_Minute";
            label_TimeOut_Minute.Size = new Size(246, 30);
            label_TimeOut_Minute.TabIndex = 43;
            label_TimeOut_Minute.Text = "- 타임 아웃               분";
            // 
            // cardAlarm
            // 
            cardAlarm.BackColor = Color.FromArgb(44, 44, 44);
            cardAlarm.Controls.Add(checkBox_popup);
            cardAlarm.Controls.Add(label_Alarm);
            cardAlarm.Controls.Add(checkBox_email);
            cardAlarm.Controls.Add(textBox_defectrate);
            cardAlarm.Controls.Add(label_Alarm_Alarmmethod);
            cardAlarm.Controls.Add(panel_Alarm_defectrate);
            cardAlarm.Controls.Add(panel_Alarm_Alarmmethod);
            cardAlarm.Controls.Add(label_Alarm_defectrate);
            cardAlarm.Location = new Point(22, 241);
            cardAlarm.Name = "cardAlarm";
            cardAlarm.Size = new Size(529, 172);
            cardAlarm.TabIndex = 53;
            // 
            // checkBox_popup
            // 
            checkBox_popup.AutoSize = true;
            checkBox_popup.Font = new Font("맑은 고딕", 15.75F);
            checkBox_popup.ForeColor = Color.White;
            checkBox_popup.Location = new Point(358, 110);
            checkBox_popup.Name = "checkBox_popup";
            checkBox_popup.Size = new Size(74, 34);
            checkBox_popup.TabIndex = 47;
            checkBox_popup.Text = "팝업";
            checkBox_popup.UseVisualStyleBackColor = true;
            // 
            // label_Alarm
            // 
            label_Alarm.AutoSize = true;
            label_Alarm.Font = new Font("맑은 고딕", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            label_Alarm.ForeColor = SystemColors.Window;
            label_Alarm.Location = new Point(9, 5);
            label_Alarm.Name = "label_Alarm";
            label_Alarm.Size = new Size(393, 37);
            label_Alarm.TabIndex = 41;
            label_Alarm.Text = "알림 임계값 설정 (불량률 알림)";
            // 
            // checkBox_email
            // 
            checkBox_email.AutoSize = true;
            checkBox_email.Font = new Font("맑은 고딕", 15.75F);
            checkBox_email.ForeColor = Color.White;
            checkBox_email.Location = new Point(214, 110);
            checkBox_email.Name = "checkBox_email";
            checkBox_email.Size = new Size(95, 34);
            checkBox_email.TabIndex = 46;
            checkBox_email.Text = "이메일";
            checkBox_email.UseVisualStyleBackColor = true;
            // 
            // textBox_defectrate
            // 
            textBox_defectrate.BackColor = Color.FromArgb(44, 44, 44);
            textBox_defectrate.BorderStyle = BorderStyle.None;
            textBox_defectrate.Font = new Font("Arial", 15.75F);
            textBox_defectrate.ForeColor = SystemColors.Window;
            textBox_defectrate.Location = new Point(228, 71);
            textBox_defectrate.Name = "textBox_defectrate";
            textBox_defectrate.Size = new Size(262, 25);
            textBox_defectrate.TabIndex = 40;
            // 
            // label_Alarm_Alarmmethod
            // 
            label_Alarm_Alarmmethod.AutoSize = true;
            label_Alarm_Alarmmethod.Font = new Font("맑은 고딕", 15.75F);
            label_Alarm_Alarmmethod.ForeColor = SystemColors.Window;
            label_Alarm_Alarmmethod.Location = new Point(18, 108);
            label_Alarm_Alarmmethod.Name = "label_Alarm_Alarmmethod";
            label_Alarm_Alarmmethod.Size = new Size(120, 30);
            label_Alarm_Alarmmethod.TabIndex = 45;
            label_Alarm_Alarmmethod.Text = "- 알림 방식";
            // 
            // panel_Alarm_defectrate
            // 
            panel_Alarm_defectrate.BackColor = Color.Silver;
            panel_Alarm_defectrate.ForeColor = Color.Silver;
            panel_Alarm_defectrate.Location = new Point(13, 99);
            panel_Alarm_defectrate.Name = "panel_Alarm_defectrate";
            panel_Alarm_defectrate.Size = new Size(500, 4);
            panel_Alarm_defectrate.TabIndex = 42;
            // 
            // panel_Alarm_Alarmmethod
            // 
            panel_Alarm_Alarmmethod.BackColor = Color.Silver;
            panel_Alarm_Alarmmethod.ForeColor = Color.Silver;
            panel_Alarm_Alarmmethod.Location = new Point(14, 156);
            panel_Alarm_Alarmmethod.Name = "panel_Alarm_Alarmmethod";
            panel_Alarm_Alarmmethod.Size = new Size(500, 4);
            panel_Alarm_Alarmmethod.TabIndex = 44;
            // 
            // label_Alarm_defectrate
            // 
            label_Alarm_defectrate.AutoSize = true;
            label_Alarm_defectrate.Font = new Font("맑은 고딕", 15.75F);
            label_Alarm_defectrate.ForeColor = SystemColors.Window;
            label_Alarm_defectrate.Location = new Point(18, 52);
            label_Alarm_defectrate.Name = "label_Alarm_defectrate";
            label_Alarm_defectrate.Size = new Size(183, 30);
            label_Alarm_defectrate.TabIndex = 43;
            label_Alarm_defectrate.Text = "- 불량률 임계값수";
            // 
            // cardFlaskServer
            // 
            cardFlaskServer.BackColor = Color.FromArgb(44, 44, 44);
            cardFlaskServer.Controls.Add(label_flaskServer_URL);
            cardFlaskServer.Controls.Add(TextBox_flaskserver);
            cardFlaskServer.Controls.Add(label_flaskServer);
            cardFlaskServer.Controls.Add(panel_flaskServer);
            cardFlaskServer.Location = new Point(22, 109);
            cardFlaskServer.Name = "cardFlaskServer";
            cardFlaskServer.Size = new Size(529, 117);
            cardFlaskServer.TabIndex = 52;
            // 
            // label_flaskServer_URL
            // 
            label_flaskServer_URL.AutoSize = true;
            label_flaskServer_URL.Font = new Font("맑은 고딕", 15.75F);
            label_flaskServer_URL.ForeColor = SystemColors.Window;
            label_flaskServer_URL.Location = new Point(9, 58);
            label_flaskServer_URL.Name = "label_flaskServer_URL";
            label_flaskServer_URL.Size = new Size(67, 30);
            label_flaskServer_URL.TabIndex = 55;
            label_flaskServer_URL.Text = "- URL";
            // 
            // TextBox_flaskserver
            // 
            TextBox_flaskserver.BackColor = Color.FromArgb(44, 44, 44);
            TextBox_flaskserver.BorderStyle = BorderStyle.None;
            TextBox_flaskserver.Font = new Font("Arial", 15.75F);
            TextBox_flaskserver.ForeColor = SystemColors.Window;
            TextBox_flaskserver.Location = new Point(104, 78);
            TextBox_flaskserver.Name = "TextBox_flaskserver";
            TextBox_flaskserver.Size = new Size(405, 25);
            TextBox_flaskserver.TabIndex = 13;
            // 
            // label_flaskServer
            // 
            label_flaskServer.AutoSize = true;
            label_flaskServer.Font = new Font("맑은 고딕", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            label_flaskServer.ForeColor = SystemColors.Window;
            label_flaskServer.Location = new Point(9, 5);
            label_flaskServer.Name = "label_flaskServer";
            label_flaskServer.Size = new Size(256, 37);
            label_flaskServer.TabIndex = 15;
            label_flaskServer.Text = "flask 서버 URL 설정";
            // 
            // panel_flaskServer
            // 
            panel_flaskServer.BackColor = Color.Silver;
            panel_flaskServer.ForeColor = Color.Silver;
            panel_flaskServer.Location = new Point(9, 106);
            panel_flaskServer.Name = "panel_flaskServer";
            panel_flaskServer.Size = new Size(500, 4);
            panel_flaskServer.TabIndex = 30;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.FromArgb(44, 44, 44);
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(22, 19);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(74, 74);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 32;
            pictureBox1.TabStop = false;
            // 
            // btn_Setting_cancel
            // 
            btn_Setting_cancel.BackColor = Color.FromArgb(44, 44, 44);
            btn_Setting_cancel.FlatAppearance.BorderSize = 0;
            btn_Setting_cancel.FlatStyle = FlatStyle.Flat;
            btn_Setting_cancel.Font = new Font("Arial", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_Setting_cancel.ForeColor = Color.White;
            btn_Setting_cancel.Location = new Point(973, 629);
            btn_Setting_cancel.Name = "btn_Setting_cancel";
            btn_Setting_cancel.Size = new Size(100, 50);
            btn_Setting_cancel.TabIndex = 29;
            btn_Setting_cancel.Text = "취소";
            btn_Setting_cancel.UseVisualStyleBackColor = false;
            btn_Setting_cancel.Click += btn_Setting_cancel_Click;
            // 
            // btn_Setting_save
            // 
            btn_Setting_save.BackColor = Color.FromArgb(44, 44, 44);
            btn_Setting_save.FlatAppearance.BorderSize = 0;
            btn_Setting_save.FlatStyle = FlatStyle.Flat;
            btn_Setting_save.Font = new Font("Arial", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_Setting_save.ForeColor = Color.White;
            btn_Setting_save.Location = new Point(852, 629);
            btn_Setting_save.Name = "btn_Setting_save";
            btn_Setting_save.Size = new Size(100, 50);
            btn_Setting_save.TabIndex = 28;
            btn_Setting_save.Text = "저장";
            btn_Setting_save.UseVisualStyleBackColor = false;
            btn_Setting_save.Click += btn_Setting_save_Click;
            // 
            // textBox1
            // 
            textBox1.BackColor = Color.FromArgb(44, 44, 44);
            textBox1.BorderStyle = BorderStyle.None;
            textBox1.Font = new Font("Arial", 24F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBox1.ForeColor = SystemColors.Window;
            textBox1.Location = new Point(746, 177);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(340, 37);
            textBox1.TabIndex = 16;
            // 
            // SettingView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(64, 64, 64);
            Controls.Add(cardSetting);
            Name = "SettingView";
            Size = new Size(1600, 900);
            cardSetting.ResumeLayout(false);
            cardSetting.PerformLayout();
            cardMySQL.ResumeLayout(false);
            cardMySQL.PerformLayout();
            cardLoglevel.ResumeLayout(false);
            cardLoglevel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)kComboBox_Loglevel).EndInit();
            cardTimeout.ResumeLayout(false);
            cardTimeout.PerformLayout();
            cardAlarm.ResumeLayout(false);
            cardAlarm.PerformLayout();
            cardFlaskServer.ResumeLayout(false);
            cardFlaskServer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel cardSetting;
        private Panel panel_flaskServer;
        private Button btn_Setting_cancel;
        private Button btn_Setting_save;
        private TextBox textBox_timeout;
        private TextBox textBox1;
        private Label label_flaskServer;
        private TextBox TextBox_flaskserver;
        private PictureBox pictureBox1;
        private Panel cardFlaskServer;
        private Panel cardMySQL;
        private TextBox textBox_host;
        private Button btn_Setting_Connectiontest;
        private Label label_MySQL_PW;
        private Panel panel_MySQL_PW;
        private Label label_MySQL_UserName;
        private Panel panel_MySQL_UserName;
        private Label label_MySQL_DB;
        private Panel panel_MySQL_DB;
        private Label label_MySQL_port;
        private Panel panel_MySQL_port;
        private Label label_MySQL_host;
        private Panel panel_MySQL_host;
        private Label label_MySQL;
        private Panel cardLoglevel;
        private Label label_LogLevel_Level;
        private ComponentFactory.Krypton.Toolkit.KryptonComboBox kComboBox_Loglevel;
        private Label label_LogLevel;
        private Panel panel_Loglevel;
        private Panel cardTimeout;
        private Panel panel_Timeout;
        private Label label_TimeOut;
        private Label label_TimeOut_Minute;
        private Panel cardAlarm;
        private CheckBox checkBox_popup;
        private Label label_Alarm;
        private CheckBox checkBox_email;
        private TextBox textBox_defectrate;
        private Label label_Alarm_Alarmmethod;
        private Panel panel_Alarm_defectrate;
        private Panel panel_Alarm_Alarmmethod;
        private Label label_Alarm_defectrate;
        private TextBox textBox_pw;
        private TextBox textBox_username;
        private TextBox textBox_DB;
        private TextBox textBox_port;
        private Label label_flaskServer_URL;
    }
}
