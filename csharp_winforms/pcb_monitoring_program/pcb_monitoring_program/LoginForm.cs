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

            // ✨ --- [1] 그림자 헬퍼 메서드 (MainForm에서 복사 및 수정) ---

            // ✨ 헬퍼: 둥근 모서리 Control 생성 (PictureBox용)
        private void MakeRoundedControl(Control ctl, int radius = 20)
        {
            GraphicsPath path = new GraphicsPath();
            path.StartFigure();
            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(ctl.Width - radius, 0, radius, radius, 270, 90);
            path.AddArc(ctl.Width - radius, ctl.Height - radius, radius, radius, 0, 90);
            path.AddArc(0, ctl.Height - radius, radius, radius, 90, 90);
            path.CloseFigure();
            ctl.Region = new Region(path);
        }

        // ✨ 헬퍼: 둥근 사각형 경로 생성
        private GraphicsPath GetRoundPath(Rectangle r, int radius)
        {
            int d = radius * 2;
            GraphicsPath path = new GraphicsPath();
            path.StartFigure();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        // ✨ 섀도우: 지정된 'shadowSurface'에 그리기 (Control로 일반화)
        private void AttachDropShadow(Control ctl, Control shadowSurface, int radius = 16, int offset = 6)
        {
            var parent = shadowSurface; // 그림자 그릴 표면

            // 중복 연결 방지
            parent.Paint -= Parent_PaintShadow;
            ctl.Move -= Child_RefreshParent;
            ctl.Resize -= Child_RefreshParent;

            parent.Paint += Parent_PaintShadow;
            ctl.Move += Child_RefreshParent;
            ctl.Resize += Child_RefreshParent;

            // 섀도우 정보 저장 (Tag에 그림자 표면(parent) 정보를 저장)
            ctl.Tag = parent;

            parent.Tag ??= new List<(Control c, int r, int off)>(); // ✨ Button b -> Control c
            var list = (List<(Control c, int r, int off)>)parent.Tag; // ✨ Button b -> Control c
            if (!list.Any(t => ReferenceEquals(t.c, ctl)))
                list.Add((ctl, radius, offset));

            // 섀도우 깜빡임 줄이기
            parent.GetType().GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(parent, true, null);
        }

        // ✨ 섀도우(오버로드): 폼('this')에 그리기 (Control로 일반화)
        private void AttachDropShadow(Control ctl, int radius = 16, int offset = 6)
        {
            AttachDropShadow(ctl, this, radius, offset);
        }

        // ✨ 섀도우: 자식 컨트롤(PB)이 움직일 때 섀도우 새로고침
        private void Child_RefreshParent(object? s, EventArgs e)
        {
            if (s is Control c && c.Tag is Control shadowSurface)
            {
                shadowSurface.Invalidate();
            }
        }

        // ✨ 섀도우: 'Parent_PaintShadow' (실제 그리기) (Control로 일반화)
        private void Parent_PaintShadow(object? s, PaintEventArgs e)
        {
            if (s is not Control parent || parent.Tag is not List<(Control c, int r, int off)> list) return; // ✨ Button b -> Control c

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            foreach (var (ctl, radius, offset) in list.ToList()) // ✨ Button btn -> Control ctl
            {
                if (ctl.Parent == null || !ctl.Visible) continue; // ✨ btn -> ctl

                Point btnPos = parent.PointToClient(ctl.PointToScreen(Point.Empty)); // ✨ btn -> ctl

                var shadowRect = new Rectangle(btnPos.X + offset, btnPos.Y + offset, ctl.Width, ctl.Height); // ✨ btn -> ctl

                var alphas = new[] { 60, 35, 20 };
                var grows = new[] { 0, 2, 4 };

                for (int i = 0; i < alphas.Length; i++)
                {
                    var grow = grows[i];
                    var rect = Rectangle.Inflate(shadowRect, grow, grow);
                    using var path = GetRoundPath(rect, radius + grow);
                    using var br = new SolidBrush(Color.FromArgb(alphas[i], 0, 0, 0));
                    e.Graphics.FillPath(br, path);
                }
            }
        }

        // ✨ --- [2] LoginForm_Load 이벤트 핸들러 ---

        // ✨ 폼이 로드될 때 PictureBox에 그림자를 적용합니다.
        private void LoginForm_Load(object sender, EventArgs e)
        {
            // 1. PictureBox 모서리 둥글게 (반지름 20)
            MakeRoundedControl(picturebox_Login_background, 20);

            // 2. PictureBox에 그림자 적용 (둥근 값 20, 12px 오프셋)
            AttachDropShadow(picturebox_Login_background, radius: 20, offset: 12);

            // 3. (중요) PictureBox를 맨 앞으로 가져와서 그림자 위에 보이게 함
            picturebox_Login_background.BringToFront();
        }
    }
}
