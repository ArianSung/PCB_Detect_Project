using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using pcb_monitoring_program.DatabaseManager.Repositories;
using System.Windows.Forms;

namespace pcb_monitoring_program.Views.UserManagement
{
    public partial class UserManagementForm_EditUser : Form
    {
        private readonly UserRepository _userRepo = new UserRepository();
        private readonly int _userId;

        public bool IsUpdated { get; private set; } = false;

        private Image _iconEyeClose;
        private Image _iconEyeOpen;

        // 🔔 작업 완료 이벤트 (수정/취소 후 메인에서 새로고침용)
        public event EventHandler UserActionCompleted;

        // 👀 비밀번호 보이기 상태
        private bool _isPasswordVisible = false;

        // ✅ 기본 생성자 (디자이너 + 스타일 초기화 공통)
        public UserManagementForm_EditUser()
        {
            InitializeComponent();

            UiStyleHelper.MakeRoundedPanel(cardEditUser, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.AddShadowRoundedPanel(cardEditUser, 16);

            UiStyleHelper.MakeRoundedButton(btn_UM_Edit_EditUser, 24);
            UiStyleHelper.MakeRoundedButton(btn_UM_Edit_cancel, 24);
            UiStyleHelper.AttachDropShadow(btn_UM_Edit_EditUser, 12, 6);
            UiStyleHelper.AttachDropShadow(btn_UM_Edit_cancel, 12, 6);

            // 비밀번호 기본은 가려진 상태 (●●●●)
            textbox_UM_Edit_PW.UseSystemPasswordChar = true;
            textbox_UM_Edit_VerifyPW.UseSystemPasswordChar = true;

            int iconSize = 32;  // 원하는 크기

            _iconEyeClose = new Bitmap(Properties.Resources.UM_eye_close, new Size(iconSize, iconSize));
            _iconEyeOpen = new Bitmap(Properties.Resources.UM_eye_open, new Size(iconSize, iconSize));

            btn_UM_Edit_PW.Image = _iconEyeClose;
            btn_UM_Edit_PW.FlatStyle = FlatStyle.Flat;
            btn_UM_Edit_PW.FlatAppearance.BorderSize = 0;
            btn_UM_Edit_PW.BackColor = Color.Transparent;
            btn_UM_Edit_PW.Text = "";
            btn_UM_Edit_PW.ImageAlign = ContentAlignment.MiddleCenter;
            btn_UM_Edit_PW.Click += Btn_UM_Edit_PW_Click;

        }

        // ✅ 선택된 유저 정보 받는 생성자
        public UserManagementForm_EditUser(int id, string username, string fullName, string role, bool isActive)
            : this()   // ← 위 기본 생성자도 같이 실행 (스타일 공통)
        {
            _userId = id;

            // 폼에 값 채우기
            textbox_UM_Edit_ID.Text = username;
            textbox_UM_Edit_Name.Text = fullName;
            kComboBox_UM_Edit_Role.SelectedItem = role;
            CB_UM_Edit_Active_True.Checked = isActive;
        }

        private void btn_UM_Edit_cancel_Click(object sender, EventArgs e)
        {
            var confirm = MessageBox.Show(
        "수정을 취소하고 창을 닫을까요?",
        "취소 확인",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                // 🔔 메인(UserManagementView)에게 "나 닫을게~" 알림
                UserActionCompleted?.Invoke(this, EventArgs.Empty);

                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        private void btn_UM_Edit_EditUser_Click(object sender, EventArgs e)
        {
            string username = textbox_UM_Edit_ID.Text.Trim();
            string pw = textbox_UM_Edit_PW.Text;
            string pwVerify = textbox_UM_Edit_VerifyPW.Text;
            string fullName = textbox_UM_Edit_Name.Text.Trim();
            string role = kComboBox_UM_Edit_Role.SelectedItem?.ToString();
            bool isActive = CB_UM_Edit_Active_True.Checked;

            // --- [1단계: 기본 유효성 검사] ---
            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("아이디를 입력하세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (_userRepo.IsUsernameTaken(username, _userId))
                {
                    MessageBox.Show("이미 존재하는 아이디입니다. 다른 아이디를 사용하세요.", "중복 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"아이디 중복 확인 중 오류가 발생했습니다: {ex.Message}", "DB 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(fullName))
            {
                MessageBox.Show("사용자 이름을 입력하세요.", "알림",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(role))
            {
                MessageBox.Show("권한을 선택하세요.", "알림",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // --- 🚨 1.5) 최종 아이디 중복 확인 (저장 전에 필수) ---
            try
            {
                // 👇 _userId를 전달하여 현재 사용자(자신)는 중복 검사에서 제외
                if (_userRepo.IsUsernameTaken(username, _userId))
                {
                    MessageBox.Show($"'{username}'은(는) 이미 사용 중인 아이디입니다. 다른 아이디를 사용하세요.", "중복 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"아이디 중복 확인 중 오류가 발생했습니다: {ex.Message}", "DB 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // --- [2단계: 비밀번호 변경 조건 검사] ---
            bool passwordChangeRequested = !string.IsNullOrEmpty(pw) || !string.IsNullOrEmpty(pwVerify);
            if (passwordChangeRequested)
            {
                if (pw != pwVerify)
                {
                    MessageBox.Show("입력된 새 비밀번호가 일치하지 않습니다.", "비밀번호 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (pw.Length < 4) // 최소 길이 제약 조건 추가 (예시)
                {
                    MessageBox.Show("비밀번호는 최소 4자 이상이어야 합니다.", "비밀번호 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            var confirm = MessageBox.Show("수정 내용을 저장할까요?",
                   "수정 확인",
                   MessageBoxButtons.YesNo,
                   MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            // --- [4단계: DB 업데이트 실행] ---
            bool userUpdateOk = false;
            bool pwUpdateOk = true; // 비밀번호 변경 요청이 없으면 true로 시작

            // 4.1 사용자 정보 업데이트 (필수)
            userUpdateOk = _userRepo.UpdateUser(_userId, username, fullName, role, isActive);

            // 4.2 비밀번호 업데이트 (선택적)
            if (passwordChangeRequested)
            {
                pwUpdateOk = _userRepo.ResetPassword(_userId, pw); // ResetPassword는 평문 비밀번호를 받아 해시 저장
            }

            // --- [5단계: 결과 처리] ---
            if (userUpdateOk && pwUpdateOk)
            {
                IsUpdated = true;
                MessageBox.Show("사용자 정보가 수정되었습니다.", "완료",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 🔔 메인에 "DB 바뀜" 알림
                UserActionCompleted?.Invoke(this, EventArgs.Empty);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                // 업데이트 중 하나라도 실패하면 오류 처리
                string failMessage = "수정에 실패했습니다.";
                if (passwordChangeRequested && !pwUpdateOk)
                {
                    failMessage += "\n(참고: 비밀번호 초기화 실패)";
                }
                else if (!userUpdateOk)
                {
                    failMessage += "\n(참고: 사용자 기본 정보 수정 실패)";
                }

                MessageBox.Show(failMessage, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Btn_UM_Edit_PW_Click(object? sender, EventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;

            if (_isPasswordVisible)
            {
                // 🔓 비밀번호 보이기 (숫자/문자 그대로 표시)
                textbox_UM_Edit_PW.UseSystemPasswordChar = false;
                textbox_UM_Edit_VerifyPW.UseSystemPasswordChar = false;

                // 아이콘 변경
                btn_UM_Edit_PW.Image = _iconEyeOpen;
            }
            else
            {
                // 🔒 다시 가리기 (시스템 기본 문자 사용)
                textbox_UM_Edit_PW.UseSystemPasswordChar = true;
                textbox_UM_Edit_VerifyPW.UseSystemPasswordChar = true;

                // Note: PasswordChar는 건드리지 않습니다.
                // UseSystemPasswordChar가 true이면 PasswordChar는 무시됩니다.

                // 아이콘 변경
                btn_UM_Edit_PW.Image = _iconEyeClose;
            }
        }

        private void btn_UM_Edit_ID_Check_Click(object sender, EventArgs e)
        {
            string username = textbox_UM_Edit_ID.Text.Trim();

            // 1. 입력값 검증
            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("확인할 아이디를 입력하세요.", "알림",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textbox_UM_Edit_ID.Focus();
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
                    textbox_UM_Edit_ID.Focus();
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
