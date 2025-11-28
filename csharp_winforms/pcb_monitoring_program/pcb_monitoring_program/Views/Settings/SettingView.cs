using MySql.Data.MySqlClient; // MySQL 연결을 위해 필요합니다.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pcb_monitoring_program.Views.Settings
{
    // partial 클래스이므로, Designer.cs에 선언된 컨트롤을 여기서 직접 접근할 수 있습니다.
    public partial class SettingView : UserControl
    {
        public SettingView()
        {
            InitializeComponent();

            // defectrate TextBox에 숫자 및 0~100 제한을 위한 이벤트 핸들러 추가
            // Designer.cs에 textBox_defectrate가 있다고 가정하고 이벤트 핸들러를 연결합니다.
            this.textBox_defectrate.KeyPress += new KeyPressEventHandler(textBox_defectrate_KeyPress);
            this.textBox_defectrate.Validating += new CancelEventHandler(textBox_defectrate_Validating);

            UiStyleHelper.MakeRoundedPanel(cardSetting, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardFlaskServer, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardAlarm, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardTimeout, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardLoglevel, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardMySQL, radius: 16, back: Color.FromArgb(44, 44, 44));

            UiStyleHelper.AddShadowRoundedPanel(cardSetting, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardFlaskServer, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardAlarm, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardTimeout, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardLoglevel, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardMySQL, 16);

            UiStyleHelper.MakeRoundedButton(btn_Setting_Connectiontest, 24);
            UiStyleHelper.MakeRoundedButton(btn_Setting_save, 24);
            UiStyleHelper.MakeRoundedButton(btn_Setting_cancel, 24);
            UiStyleHelper.AttachDropShadow(btn_Setting_Connectiontest, radius: 12, offset: 6);
            UiStyleHelper.AttachDropShadow(btn_Setting_save, radius: 12, offset: 6);
            UiStyleHelper.AttachDropShadow(btn_Setting_cancel, radius: 12, offset: 6);
        }

        // --- 숫자 제한 기능 구현 ---
        private void textBox_defectrate_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 숫자가 아니거나 (e.KeyChar >= '0' && e.KeyChar <= '9')
            // 컨트롤 문자가 아닌 경우 (e.KeyChar == 8은 백스페이스)
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != 8)
            {
                e.Handled = true; // 입력 무시
            }
        }

        // Validating 이벤트: 포커스가 벗어날 때 0~100 범위 검사를 수행합니다.
        private void textBox_defectrate_Validating(object sender, CancelEventArgs e)
        {
            if (int.TryParse(textBox_defectrate.Text, out int value))
            {
                // 0보다 작거나 100보다 큰 경우
                if (value < 0 || value > 100)
                {
                    MessageBox.Show("불량률은 0에서 100 사이의 값이어야 합니다.", "입력 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    e.Cancel = true; // 포커스가 다른 곳으로 이동하는 것을 막습니다.
                    textBox_defectrate.Focus(); // 다시 해당 텍스트 박스로 포커스를 이동합니다.
                    textBox_defectrate.SelectAll(); // 텍스트를 전체 선택하여 수정하기 쉽게 합니다.
                }
                // 값이 0~100 사이에 있다면 유효성 검사 통과
            }
            else
            {
                // 숫자로 변환할 수 없는 경우 (KeyPress에서 대부분 걸러지지만, 빈 문자열 등을 처리)
                if (!string.IsNullOrEmpty(textBox_defectrate.Text))
                {
                    MessageBox.Show("유효한 숫자를 입력해야 합니다.", "입력 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    e.Cancel = true; // 포커스 이동 막음
                    textBox_defectrate.Focus();
                    textBox_defectrate.SelectAll();
                }
                // 빈 문자열은 허용하거나, 필요에 따라 0으로 설정하는 등의 추가 처리가 필요할 수 있습니다.
            }
        }

        private void btn_Setting_Connectiontest_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("MySQL 연결 테스트를 진행할까요?",
                   "설정",
                   MessageBoxButtons.YesNo,
                   MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                MessageBox.Show("연결 테스트가 취소되었습니다.", "설정",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 1. UI에서 연결 정보 가져오기 
            string host = textBox_host.Text.Trim();
            string port = textBox_port.Text.Trim();
            string database = textBox_DB.Text.Trim();
            string userId = textBox_username.Text.Trim();
            string password = textBox_pw.Text;

            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(port) ||
                string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("호스트, 포트, 사용자명, 비밀번호는 반드시 입력해야 합니다.",
                    "연결 테스트 실패", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ✅ 1단계: 서버 + 계정/비밀번호 테스트 (Database는 지정 안 함)
            string baseConnStr =
                $"Server={host};Port={port};Uid={userId};Pwd={password};CharSet=utf8mb4;";

            try
            {
                using (var conn = new MySqlConnection(baseConnStr))
                {
                    conn.Open();          // 서버 + 로그인 테스트
                    conn.Ping();          // 한 번 더 체크

                    // 여기까지 오면 "서버 접속 + 로그인" 은 성공
                    if (string.IsNullOrWhiteSpace(database))
                    {
                        MessageBox.Show(
                            "서버 연결 및 로그인까지는 성공했습니다.\n" +
                            "테스트할 데이터베이스 이름(textBox_DB)을 입력하면 DB 권한까지 확인할 수 있습니다.",
                            "연결 테스트 결과", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // ✅ 2단계: DB 존재 여부 확인
                    using (var cmdSchema = new MySqlCommand(
                        "SELECT COUNT(*) FROM information_schema.SCHEMATA WHERE SCHEMA_NAME = @db;", conn))
                    {
                        cmdSchema.Parameters.AddWithValue("@db", database);
                        int exists = Convert.ToInt32(cmdSchema.ExecuteScalar());

                        if (exists == 0)
                        {
                            MessageBox.Show(
                                $"서버에는 '{database}' 데이터베이스가 존재하지 않습니다.\n" +
                                $"(예: 실제 스키마 이름이 'pcb_inspection' 인지 확인해 보세요.)",
                                "DB 존재 여부 확인 실패", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    // ✅ 3단계: 해당 DB에 대한 권한 테스트
                    try
                    {
                        using (var cmdUse = new MySqlCommand($"USE `{database}`;", conn))
                        {
                            cmdUse.ExecuteNonQuery();
                        }

                        using (var cmdTest = new MySqlCommand("SELECT 1;", conn))
                        {
                            cmdTest.ExecuteScalar();
                        }

                        MessageBox.Show(
                            "서버 연결, 로그인, 지정 DB에 대한 권한까지 모두 정상입니다.",
                            "연결 테스트 성공", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (MySqlException exDb)
                    {
                        MessageBox.Show(
                            "서버 연결 및 로그인은 성공했지만,\n" +
                            $"'{database}' 데이터베이스에 대한 권한이 없습니다.\n\n" +
                            $"[상세]\n{exDb.Message}",
                            "DB 권한 테스트 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (MySqlException exLogin)
            {
                MessageBox.Show(
                    "서버 연결 또는 로그인 단계에서 실패했습니다.\n\n" +
                    $"[상세]\n{exLogin.Message}",
                    "연결 테스트 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "연결 테스트 중 알 수 없는 오류가 발생했습니다.\n\n" +
                    $"[상세]\n{ex.Message}",
                    "연결 테스트 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_Setting_save_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("설정을 저장할까요?",
             "설정",
             MessageBoxButtons.YesNo,
             MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                // TODO: 여기에 실제로 설정 값을 파일이나 DB에 저장하는 로직을 추가합니다.
                MessageBox.Show("설정이 저장되었습니다.", "설정", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("설정 저장이 취소되었습니다.", "설정", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

        }

        private void btn_Setting_cancel_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("추가를 취소하고 창을 닫을까요?",
                             "설정",
                             MessageBoxButtons.YesNo,
                             MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
                MessageBox.Show("설정이 취소되었습니다.", "설정", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}