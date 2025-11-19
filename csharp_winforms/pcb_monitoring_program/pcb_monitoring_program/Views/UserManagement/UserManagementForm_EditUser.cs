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

            btn_UM_Edit_PW.Click += btn_UM_Edit_PW_Click;
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


            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("아이디를 입력하세요.", "알림",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

            var confirm = MessageBox.Show("수정 내용을 저장할까요?",
                   "수정 확인",
                   MessageBoxButtons.YesNo,
                   MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            bool ok = _userRepo.UpdateUser(_userId, username, fullName, role, isActive);

            if (ok)
            {
                IsUpdated = true;
                MessageBox.Show("사용자 정보가 수정되었습니다.", "완료",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 🔔 여기! - 메인에 "DB 바뀜" 알림
                UserActionCompleted?.Invoke(this, EventArgs.Empty);

                this.DialogResult = DialogResult.OK;  // 모달일 때 쓰던 거라 있어도 되고 없어도 됨
                this.Close();
            }
            else
            {
                MessageBox.Show("수정에 실패했습니다.", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_UM_Edit_PW_Click(object sender, EventArgs e)
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
    }
}
