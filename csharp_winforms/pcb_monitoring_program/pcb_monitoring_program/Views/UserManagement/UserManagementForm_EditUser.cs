using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using pcb_monitoring_program.DatabaseManager.Repositories;

namespace pcb_monitoring_program.Views.UserManagement
{
    public partial class UserManagementForm_EditUser : Form
    {
        private readonly UserRepository _userRepo = new UserRepository();

        // 🔑 PK 역할을 하는 아이디(username)만 보관
        private readonly string _username;

        public bool IsUpdated { get; private set; } = false;

        private Image _iconEyeClose;
        private Image _iconEyeOpen;

        // 👀 비밀번호 보이기 상태
        private bool _isPasswordVisible = false;

        // 🔔 작업 완료 이벤트 (수정/취소 후 메인에서 새로고침용)
        public event EventHandler UserActionCompleted;

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

            int iconSize = 32;

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

        // ✅ 선택된 유저 정보 받는 생성자 (View에서 호출)
        public UserManagementForm_EditUser(string username, string fullName, string role, bool isActive)
            : this()   // 위 기본 생성자도 같이 실행 (스타일 공통)
        {
            _username = username;

            // 폼에 값 채우기
            textbox_UM_Edit_ID.Text = username;
            textbox_UM_Edit_ID.ReadOnly = true;          // 🔒 아이디는 수정 불가
            textbox_UM_Edit_ID.Enabled = false;         // (원하면 ReadOnly만 두고 Enabled는 true로 놔도 됨)

            textbox_UM_Edit_Name.Text = fullName;
            kComboBox_UM_Edit_Role.SelectedItem = role;
            CB_UM_Edit_Active_True.Checked = isActive;

            // 아이디 중복확인 버튼은 의미 없으므로 비활성화
            btn_UM_Edit_ID_Check.Enabled = false;
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
                UserActionCompleted?.Invoke(this, EventArgs.Empty);

                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        private void btn_UM_Edit_EditUser_Click(object sender, EventArgs e)
        {
            // 아이디는 수정 불가이므로 _username 사용
            string username = _username;

            string pw = textbox_UM_Edit_PW.Text;
            string pwVerify = textbox_UM_Edit_VerifyPW.Text;
            string fullName = textbox_UM_Edit_Name.Text.Trim();
            string role = kComboBox_UM_Edit_Role.SelectedItem?.ToString();
            bool isActive = CB_UM_Edit_Active_True.Checked;

            // --- [1단계: 기본 유효성 검사] ---
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

            // --- [2단계: 비밀번호 변경 조건 검사] ---
            bool passwordChangeRequested = !string.IsNullOrEmpty(pw) || !string.IsNullOrEmpty(pwVerify);
            if (passwordChangeRequested)
            {
                if (pw != pwVerify)
                {
                    MessageBox.Show("입력된 새 비밀번호가 일치하지 않습니다.", "비밀번호 오류",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (pw.Length < 4)
                {
                    MessageBox.Show("비밀번호는 최소 4자 이상이어야 합니다.", "비밀번호 오류",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            var confirm = MessageBox.Show(
                "수정 내용을 저장할까요?",
                "수정 확인",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            // --- [3단계: DB 업데이트 실행] ---
            bool userUpdateOk = false;
            bool pwUpdateOk = true; // 비밀번호 변경 요청이 없으면 true

            // 3.1 사용자 기본 정보 업데이트
            userUpdateOk = _userRepo.UpdateUserByUsername(username, fullName, role, isActive);

            // 3.2 비밀번호 변경 (선택)
            if (passwordChangeRequested)
            {
                pwUpdateOk = _userRepo.ResetPassword(username, pw); // 평문 넘기면 내부에서 해시
            }

            // --- [4단계: 결과 처리] ---
            if (userUpdateOk && pwUpdateOk)
            {
                IsUpdated = true;
                MessageBox.Show("사용자 정보가 수정되었습니다.", "완료",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                UserActionCompleted?.Invoke(this, EventArgs.Empty);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                string failMessage = "수정에 실패했습니다.";
                if (passwordChangeRequested && !pwUpdateOk)
                {
                    failMessage += "\n(참고: 비밀번호 변경 실패)";
                }
                else if (!userUpdateOk)
                {
                    failMessage += "\n(참고: 사용자 기본 정보 수정 실패)";
                }

                MessageBox.Show(failMessage, "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Btn_UM_Edit_PW_Click(object? sender, EventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;

            if (_isPasswordVisible)
            {
                textbox_UM_Edit_PW.UseSystemPasswordChar = false;
                textbox_UM_Edit_VerifyPW.UseSystemPasswordChar = false;
                btn_UM_Edit_PW.Image = _iconEyeOpen;
            }
            else
            {
                textbox_UM_Edit_PW.UseSystemPasswordChar = true;
                textbox_UM_Edit_VerifyPW.UseSystemPasswordChar = true;
                btn_UM_Edit_PW.Image = _iconEyeClose;
            }
        }

        // 🔒 아이디는 수정/중복체크 할 수 없도록 처리
        private void btn_UM_Edit_ID_Check_Click(object sender, EventArgs e)
        {
            MessageBox.Show("이미 등록된 아이디는 수정할 수 없습니다.", "안내",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
