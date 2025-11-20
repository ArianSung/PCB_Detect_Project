using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using pcb_monitoring_program.DatabaseManager;
using pcb_monitoring_program.DatabaseManager.Models;

namespace pcb_monitoring_program
{
    public partial class LoginForm : Form
    {
        public event EventHandler OpenDetailsRequested;

        private int _failedLoginCount = 0;
        private DateTime? _lockoutUntil = null;

        public LoginForm()
        {
            InitializeComponent();

            // 버튼은 반드시 카드의 자식
            btn_login.Parent = cardLogin;

            // 카드 스타일/섀도우: Paint에서 하지 말고 한 번만
            Color cardBgColor = Color.FromArgb(44, 44, 44);
            UiStyleHelper.MakeRoundedPanel(cardLogin, 16, Color.FromArgb(44, 44, 44));
            UiStyleHelper.AddShadowRoundedPanel(cardLogin, 16);

            // --- 테두리 문제 해결을 위한 코드 추가 시작 ---
            // 1. FlatStyle을 적용하여 버튼의 외관을 단순화합니다.
            btn_login.FlatStyle = FlatStyle.Flat;

            // 2. 기본 버튼의 테두리를 0으로 설정하여 충돌을 방지합니다.
            btn_login.FlatAppearance.BorderSize = 0;

            // 3. 마우스 상태 변화 시(Hover/Down) 발생할 수 있는 테두리나 배경 변화를 
            //    버튼의 BackColor(44, 44, 44)와 동일하게 설정하여 완전히 마스킹합니다.
            //    (Designer에서 BackColor가 44, 44, 44로 설정되어 있다고 가정합니다.)
            btn_login.FlatAppearance.BorderColor = cardBgColor;
            btn_login.FlatAppearance.MouseDownBackColor = cardBgColor;
            btn_login.FlatAppearance.MouseOverBackColor = cardBgColor;
            // --- 테두리 문제 해결을 위한 코드 추가 끝 ---

            // 버튼 둥글게 + 그림자 초기화 (한 번만)
            UiStyleHelper.MakeRoundedButton(btn_login, 24);
            UiStyleHelper.AttachDropShadow(btn_login, radius: 12, offset: 6);

            // 버튼이 움직이거나 크기 바뀌면 카드 다시 그리기
            btn_login.Move += (_, __) => cardLogin.Invalidate();
            btn_login.Resize += (_, __) => cardLogin.Invalidate();

            // 깜빡임 줄이기
            typeof(Panel).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(cardLogin, true, null);
        }

        private void btn_login_Click(object sender, EventArgs e)
        {
            string enteredUserId = userIdTextBox.Text.Trim();      // ← username
            string enteredPassword = passwordTextBox.Text;         // ← 평문 비번

            if (_lockoutUntil.HasValue && DateTime.Now < _lockoutUntil.Value)
            {
                var remaining = _lockoutUntil.Value - DateTime.Now;
                int seconds = (int)Math.Ceiling(remaining.TotalSeconds);
                MessageBox.Show(
                    $"로그인 시도가 잠겨 있습니다.\n{seconds}초 후에 다시 시도해주세요.",
                    "로그인 잠금",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(enteredUserId) || string.IsNullOrEmpty(enteredPassword))
            {
                MessageBox.Show("아이디와 비밀번호를 입력하세요.", "로그인 실패");
                return;
            }

            try
            {
                // ✅ 새 DB (pcb_inspection)용 연결 문자열
                string connectionString = "Server=100.80.24.53;Port=3306;Database=pcb_inspection;Uid=pcb_admin;Pwd=1234;CharSet=utf8mb4;";

                // ✅ DatabaseManager 사용해서 로그인 검증
                using (var db = new DatabaseManager.DatabaseManager(connectionString))
                {
                    // 여기서 username / password 넘기면 내부에서 bcrypt로 검증해줌
                    User user = db.ValidateLogin(enteredUserId, enteredPassword);

                    if (user != null)
                    {
                        // 로그인 성공!
                        MessageBox.Show($"{user.Role} 권한으로 로그인 성공!", "로그인 성공");

                        // 여기서 user 정보를 MainForm에 넘기고 싶으면 생성자 수정해서 전달해도 됨
                        MainForm nextForm = new MainForm(enteredUserId, user.Role.ToString());
                        nextForm.Show();
                        this.Hide();
                    }
                    else
                    {
                        _failedLoginCount++;

                        if (_failedLoginCount >= 5)
                        {
                            // 5회 연속 실패 → 1분 잠금
                            _lockoutUntil = DateTime.Now.AddMinutes(1);
                            _failedLoginCount = 0; // 잠금 후 카운트 초기화 (1분 뒤 다시 5번 기회)

                            MessageBox.Show(
                                "로그인 5회 연속 실패로 1분 동안 로그인 시도가 잠깁니다.",
                                "로그인 잠금",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                        else
                        {
                            int remain = 5 - _failedLoginCount;
                            MessageBox.Show(
                                $"아이디 또는 비밀번호가 일치하지 않습니다.\n남은 시도 횟수: {remain}회",
                                "로그인 실패",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                        }
                    }
                }   
            }
            catch (Exception ex)
            {
                MessageBox.Show($"로그인 처리 중 오류 발생: {ex.Message}", "오류");
            }
        }
    }
}
