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

        // ✅ 선택된 유저 정보 받는 생성자
        public UserManagementForm_EditUser(int id, string username, string fullName, string role, bool isActive)
        {
            InitializeComponent();

            _userId = id;

            UiStyleHelper.MakeRoundedPanel(cardEditUser, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.AddShadowRoundedPanel(cardEditUser, 16);

            UiStyleHelper.MakeRoundedButton(btn_UM_Edit_EditUser, 24);
            UiStyleHelper.MakeRoundedButton(btn_UM_Edit_cancel, 24);
            UiStyleHelper.AttachDropShadow(btn_UM_Edit_EditUser, 12, 6);
            UiStyleHelper.AttachDropShadow(btn_UM_Edit_cancel, 12, 6);

            // 폼에 값 채우기
            textbox_UM_Edit_ID.Text = username;
            textbox_UM_Edit_Name.Text = fullName;

            // 콤보박스는 admin/operator/viewer가 들어있다고 했으니까
            kComboBox_UM_Edit_Role.SelectedItem = role;

            CB_UM_Edit_Active_True.Checked = isActive;
        }


        public UserManagementForm_EditUser()
        {
            InitializeComponent();

            UiStyleHelper.MakeRoundedPanel(cardEditUser, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.AddShadowRoundedPanel(cardEditUser, 16);

            UiStyleHelper.MakeRoundedButton(btn_UM_Edit_EditUser, 24);
            UiStyleHelper.MakeRoundedButton(btn_UM_Edit_cancel, 24);
            UiStyleHelper.AttachDropShadow(btn_UM_Edit_EditUser, radius: 12, offset: 6);
            UiStyleHelper.AttachDropShadow(btn_UM_Edit_cancel, radius: 12, offset: 6);
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
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        private void btn_UM_Edit_EditUser_Click(object sender, EventArgs e)
        {
            string username = textbox_UM_Edit_ID.Text.Trim();
            string pw = textbox_UM_Edit_PW.Text;
            string pwVerify = textbox_UM_Edit_VerifyPW.Text;
            string fullName = textbox_UM_Edit_Name.Text.Trim();   // ← 실제 텍스트박스 이름에 맞게 수정
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
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("수정에 실패했습니다.", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
