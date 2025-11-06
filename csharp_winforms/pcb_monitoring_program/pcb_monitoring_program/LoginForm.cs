using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace pcb_monitoring_program
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private void btn_login_Click(object sender, EventArgs e)
        {
            string enteredUserId = userIdTextBox.Text;
            string enteredPassword = passwordTextBox.Text;

            // TODO: 🚨 실제 비밀번호는 해시 처리 필요! 
            // 현재는 테스트를 위해 평문(qwer)을 가정합니다.

            // 1. 데이터베이스 연결 문자열 (이 부분은 사용자 환경에 맞게 수정해야 합니다.)
            string connectionString = "Server=localhost;Database=userdb;Uid=root;Pwd=moble;";

            // 2. SQL 쿼리 작성 (변경 없음)
            string query = "SELECT role FROM users WHERE user_id = @userId AND password = @password";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    MySqlCommand command = new MySqlCommand(query, connection);

                    // 3. 파라미터 값 설정 (변경 없음)
                    command.Parameters.AddWithValue("@userId", enteredUserId);
                    // ⚠️ 여기서 실제로는 enteredPassword를 해시하여 DB의 해시 값과 비교해야 합니다!
                    command.Parameters.AddWithValue("@password", enteredPassword);

                    // 4. 쿼리 실행 및 결과 가져오기 (이하 변경 없음)
                    object result = command.ExecuteScalar();

                    if (result != null)
                    {
                        // 로그인 성공!
                        string userRole = result.ToString();
                        MessageBox.Show($"{userRole} 권한으로 로그인 성공!", "로그인 성공");

                        // 5. Form2 열기 및 Form1 숨기기
                        MainForm nextForm = new MainForm();
                        nextForm.Show();
                        this.Hide();
                    }
                    else
                    {
                        // 로그인 실패
                        MessageBox.Show("아이디 또는 비밀번호가 일치하지 않습니다.", "로그인 실패");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"데이터베이스 연결 오류 또는 쿼리 오류: {ex.Message}", "오류");
                }
            }
        }
    }
}
