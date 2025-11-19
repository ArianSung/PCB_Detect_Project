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
using pcb_monitoring_program.DatabaseManager.Repositories;
using BCrypt.Net;

namespace pcb_monitoring_program.Views.UserManagement
{
    public partial class UserManagementForm_AddUser : Form
    {
        private readonly UserRepository _userRepo = new UserRepository();

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
            UiStyleHelper.AttachDropShadow(btn_UM_ADD_AddUser, radius: 12, offset: 6);
            UiStyleHelper.AttachDropShadow(btn_UM_ADD_cancel, radius: 12, offset: 6);

            CB_UM_ADD_Active_True.Checked = true;
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
    }
}
