using pcb_monitoring_program.DatabaseManager.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using BCrypt.Net;

namespace pcb_monitoring_program.Views.UserManagement
{
    public partial class UserManagementForm_AddUser : Form
    {
        private readonly UserRepository _userRepo = new UserRepository();

        private Image _iconEyeClose;
        private Image _iconEyeOpen;

        // 👀 비밀번호 보이기 상태
        private bool _isPasswordVisible = false;

        // ✅ 아이디 중복확인 상태
        private bool _isIdChecked = false;
        private string _checkedUsername = string.Empty;

        public UserManagementForm_AddUser()
        {
            InitializeComponent();

            // 🔒 비밀번호 입력 마스킹 처리
            textbox_UM_ADD_PW.UseSystemPasswordChar = true;
            textbox_UM_ADD_VerifyPW.UseSystemPasswordChar = true;

            UiStyleHelper.MakeRoundedPanel(cardAddUser, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.AddShadowRoundedPanel(cardAddUser, 16);

            UiStyleHelper.MakeRoundedButton(btn_UM_ADD_AddUser, 24);
            UiStyleHelper.MakeRoundedButton(btn_UM_ADD_cancel, 24);
            UiStyleHelper.MakeRoundedButton(btn_UM_Add_ID_Check, 24);
            UiStyleHelper.AttachDropShadow(btn_UM_ADD_AddUser, radius: 12, offset: 6);
            UiStyleHelper.AttachDropShadow(btn_UM_ADD_cancel, radius: 12, offset: 6);
            UiStyleHelper.AttachDropShadow(btn_UM_Add_ID_Check, radius: 12, offset: 6);

            CB_UM_ADD_Active_True.Checked = true;

            int iconSize = 32;

            // 리소스 이미지 리사이즈
            _iconEyeClose = new Bitmap(Properties.Resources.UM_eye_close, new Size(iconSize, iconSize));
            _iconEyeOpen = new Bitmap(Properties.Resources.UM_eye_open, new Size(iconSize, iconSize));

            // 버튼 기본 이미지는 눈 감김
            btn_UM_Add_PW.Image = _iconEyeClose;

            // 버튼 비주얼 초기화
            btn_UM_Add_PW.FlatStyle = FlatStyle.Flat;
            btn_UM_Add_PW.FlatAppearance.BorderSize = 0;
            btn_UM_Add_PW.BackColor = Color.Transparent;
            btn_UM_Add_PW.Text = "";
            btn_UM_Add_PW.ImageAlign = ContentAlignment.MiddleCenter;

            // 아이디가 바뀌면 중복확인 상태 초기화
            textbox_UM_ADD_ID.TextChanged += textbox_UM_ADD_ID_TextChanged;
        }

        private void textbox_UM_ADD_ID_TextChanged(object sender, EventArgs e)
        {
            // 아이디가 한 글자라도 바뀌면 다시 중복확인 필요
            _isIdChecked = false;
            _checkedUsername = string.Empty;
        }

        // ✅ 비밀번호 해시 (SHA256)
        private string HashPassword(string plainText)
        {
            // cost 12로 맞추기 (DB에 $2b$12$ 라고 되어 있으니까)
            return BCrypt.Net.BCrypt.HashPassword(plainText, workFactor: 12);
        }

        private void btn_UserManage_AddUser_Click(object sender, EventArgs e)
        {
            string username = textbox_UM_ADD_ID.Text.Trim();
            string pw = textbox_UM_ADD_PW.Text;
            string pwVerify = textbox_UM_ADD_VerifyPW.Text;
            string fullName = textbox_UM_ADD_Name.Text.Trim();   // ← 실제 텍스트박스 이름에 맞게 수정
            string role = kComboBox_UM_ADD_Role.SelectedItem?.ToString();
            bool isActive = CB_UM_ADD_Active_True.Checked;

            // 0) 중복확인 성공했는지 먼저 체크
            if (!_isIdChecked || !string.Equals(_checkedUsername, username, StringComparison.Ordinal))
            {
                MessageBox.Show("아이디 중복확인을 먼저 진행하세요.", "알림",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textbox_UM_ADD_ID.Focus();
                return;
            }

            // 1) 기본 검증
            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("아이디를 입력하세요.", "알림",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textbox_UM_ADD_ID.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(fullName))
            {
                MessageBox.Show("사용자 이름을 입력하세요.", "알림",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textbox_UM_ADD_Name.Focus();
                return;
            }

            // --- 🚨 1.5) 최종 아이디 중복 확인 (저장 전에 필수) ---
            try
            {
                if (_userRepo.IsUsernameTaken(username, 0))
                {
                    MessageBox.Show($"'{username}'은(는) 이미 사용 중인 아이디입니다. 다른 아이디를 사용하세요.", "중복 오류",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textbox_UM_ADD_ID.Focus();
                    return; // 중복이므로 저장 중단
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"사용자 추가 중 아이디 확인에 DB 오류가 발생했습니다: {ex.Message}", "DB 오류",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(pw) || string.IsNullOrEmpty(pwVerify))
            {
                MessageBox.Show("비밀번호와 비밀번호 확인을 모두 입력하세요.", "알림",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (pw != pwVerify)
            {
                MessageBox.Show("비밀번호와 확인 비밀번호가 일치하지 않습니다.", "알림",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textbox_UM_ADD_VerifyPW.Clear();
                textbox_UM_ADD_VerifyPW.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(role))
            {
                MessageBox.Show("권한을 선택하세요.", "알림",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2) 확인 메시지
            var confirm = MessageBox.Show(
                $"사용자 '{username}' 를 추가할까요?",
                "추가 확인",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
                return;

            // 3) 비밀번호 해시화
            string passwordHash = HashPassword(pw);

            // 4) DB Insert
            bool success = _userRepo.AddUser(username, passwordHash, fullName, role, isActive);

            if (success)
            {
                MessageBox.Show("사용자가 추가되었습니다.", "완료",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;  // 부모 폼에서 감지 가능
                this.Close();
            }
            else
            {
                MessageBox.Show("사용자 추가에 실패했습니다.", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_UserManage_cancel_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                            "추가를 취소하고 창을 닫을까요?",
                            "취소 확인",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        private void btn_UM_Add_PW_Click(object sender, EventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;

            if (_isPasswordVisible)
            {
                // 🔓 비밀번호 보이기 (숫자/문자 그대로 표시)
                textbox_UM_ADD_PW.UseSystemPasswordChar = false;
                textbox_UM_ADD_VerifyPW.UseSystemPasswordChar = false;

                // 아이콘 변경
                btn_UM_Add_PW.Image = _iconEyeOpen;
            }
            else
            {
                // 🔒 다시 가리기 (시스템 기본 문자 사용)
                textbox_UM_ADD_PW.UseSystemPasswordChar = true;
                textbox_UM_ADD_VerifyPW.UseSystemPasswordChar = true;

                // Note: PasswordChar는 건드리지 않습니다.
                // UseSystemPasswordChar가 true이면 PasswordChar는 무시됩니다.

                // 아이콘 변경
                btn_UM_Add_PW.Image = _iconEyeClose;
            }
        }

        private void btn_UM_Add_ID_Check_Click(object sender, EventArgs e)
        {
            string username = textbox_UM_ADD_ID.Text.Trim();

            // 1. 입력값 검증
            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("확인할 아이디를 입력하세요.", "알림",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textbox_UM_ADD_ID.Focus();
                return;
            }

            // 2. DB 중복 확인 실행
            try
            {
                // IsUsernameTaken(username, 0) 호출 (0은 새 사용자 추가를 의미)
                bool isTaken = _userRepo.IsUsernameTaken(username, 0);

                if (isTaken)
                {
                    MessageBox.Show($"'{username}'은(는) 이미 사용 중인 아이디입니다. 다른 아이디를 사용하세요.", "중복 오류",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textbox_UM_ADD_ID.Focus();
                }
                else
                {
                    MessageBox.Show($"'{username}'은(는) 사용 가능한 아이디입니다.", "확인 완료",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                // DB 연동 오류 처리
                MessageBox.Show($"아이디 확인 중 DB 오류가 발생했습니다: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
