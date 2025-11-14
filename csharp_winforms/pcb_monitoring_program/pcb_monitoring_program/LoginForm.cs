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

namespace pcb_monitoring_program
{
    public partial class LoginForm : Form
    {
        public event EventHandler OpenDetailsRequested;
        public event EventHandler<LoginSucceededEventArgs>? LoginSucceeded;

        public LoginForm()
        {
            InitializeComponent();

            // 버튼은 반드시 카드의 자식
            btn_login.Parent = cardLogin;

            // 카드 스타일/섀도우: Paint에서 하지 말고 한 번만
            UiStyleHelper.MakeRoundedPanel(cardLogin, 16, Color.FromArgb(44, 44, 44));
            UiStyleHelper.AddShadowRoundedPanel(cardLogin, 16);

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
            string enteredUserId = userIdTextBox.Text;
            string enteredPassword = passwordTextBox.Text;

            var sampleUsers = new Dictionary<string, (string Password, string Role, string Display)>(StringComparer.OrdinalIgnoreCase)
            {
                ["admin"] = ("qwer", "Admin", "관리자"),
                ["operator"] = ("pass1234", "Operator", "오퍼레이터"),
                ["viewer"] = ("viewonly", "Viewer", "뷰어")
            };

            if (sampleUsers.TryGetValue(enteredUserId, out var account) &&
                account.Password == enteredPassword)
            {
                MessageBox.Show($"{account.Display} 권한으로 로그인 성공!", "로그인 성공");
                OnLoginSucceeded(enteredUserId, account.Role);
                Close();
            }
            else
            {
                MessageBox.Show("아이디 또는 비밀번호가 일치하지 않습니다.", "로그인 실패");
            }
        }

        private void OnLoginSucceeded(string userId, string role)
        {
            LoginSucceeded?.Invoke(this, new LoginSucceededEventArgs(userId, role));
        }
    }

    public sealed class LoginSucceededEventArgs : EventArgs
    {
        public string UserId { get; }
        public string Role { get; }

        public LoginSucceededEventArgs(string userId, string role)
        {
            UserId = userId;
            Role = role;
        }
    }
}
