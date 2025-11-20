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
        // 🚨 모든 컨트롤 필드 선언을 제거합니다! 
        // Designer.cs 파일에 이미 선언되어 있습니다.
        // 이 부분을 제거해야 모호성 오류가 사라집니다.

        public SettingView()
        {
            InitializeComponent();

            // InitializeComponent()가 디자이너에서 생성된 실제 컨트롤을 사용합니다.
            // 따라서 별도의 초기화 코드를 제거하는 것이 올바른 방법입니다.

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

        private void btn_Setting_Connectiontest_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("MySQL과 연결 테스트를 하시겠습니까?",
             "설정",
             MessageBoxButtons.YesNo,
             MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // 1. UI에서 연결 정보 가져오기 
                string host = textBox_host.Text.Trim();
                string port = textBox_port.Text.Trim();
                string database = textBox_DB.Text.Trim();
                string userId = textBox_username.Text.Trim();
                string password = textBox_pw.Text;

                // 입력 필드 누락 검사
                if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(port) ||
                    string.IsNullOrEmpty(database) || string.IsNullOrEmpty(userId) ||
                    string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("모든 MySQL 연결 정보를 입력해야 합니다.", "연결 테스트 실패", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 2. 연결 문자열 생성
                string connectionString = $"Server={host};Port={port};Database={database};Uid={userId};Pwd={password};CharSet=utf8mb4;";

                // 3. MySQL 연결 시도
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    try
                    {
                        // 연결 열기 시도 (여기서 실제로 연결을 테스트합니다)
                        connection.Open();

                        // 연결 성공
                        MessageBox.Show("MySQL 연결 테스트에 성공하셨습니다.", "설정", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (MySqlException ex)
                    {
                        // 연결 실패 (MySQL 관련 오류)
                        MessageBox.Show($"MySQL 연결 테스트에 실패했습니다.\n\n오류 상세: {ex.Message}", "연결 테스트 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (Exception ex)
                    {
                        // 기타 연결 실패
                        MessageBox.Show($"연결 테스트 중 알 수 없는 오류가 발생했습니다.\n\n오류 상세: {ex.Message}", "연결 테스트 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        // 연결이 열려 있었다면 닫기
                        if (connection.State == ConnectionState.Open)
                        {
                            connection.Close();
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("연결 테스트가 취소되었습니다.", "설정", MessageBoxButtons.OK, MessageBoxIcon.Information);
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