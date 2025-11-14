using ComponentFactory.Krypton.Toolkit;
using pcb_monitoring_program;
using pcb_monitoring_program.Views.Statistics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pcb_monitoring_program.Views.UserManagement
{
    public partial class UserManagementView : UserControl
    {
        public event EventHandler OpenDetailsRequested;
        public UserManagementView()
        {
            InitializeComponent();
        }

        private void StyleAllButtons(Control root)
        {
            foreach (Control ctrl in root.Controls)
            {
                if (ctrl is Button btn) // System.Windows.Forms.Button
                {
                    btn.UseVisualStyleBackColor = false;           // 시스템 테마 무시
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                    btn.BackColor = Color.FromArgb(64, 64, 64);
                    btn.ForeColor = Color.White;
                    btn.Cursor = Cursors.Hand;

                    // 필요 시 Hover 색
                    btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(80, 80, 80);
                    btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(56, 56, 56);
                }
                // 🔁 중요: 자식 컨테이너도 계속 탐색
                if (ctrl.HasChildren)
                    StyleAllButtons(ctrl);
            }
        }
        private void UserManagementView_Load(object sender, EventArgs e)
        {

            UiStyleHelper.MakeRoundedPanel(card_userManagement, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedButton(btn_UserManage_Search, 24);
            UiStyleHelper.MakeRoundedButton(btn_UserManage_AddUser, 24);
            UiStyleHelper.MakeRoundedButton(btn_UserManage_EditUser, 24);
            UiStyleHelper.MakeRoundedButton(btn_UserManage_DeleteUser, 24);
            UiStyleHelper.MakeRoundedButton(btn_UserManage_ResetPW, 24);
            UiStyleHelper.MakeRoundedButton(btn_UserManage_Refresh, 24);

            UiStyleHelper.AddShadowRoundedPanel(card_userManagement, 16);
            UiStyleHelper.AttachDropShadow(btn_UserManage_Search, radius: 16, offset: 4);
            UiStyleHelper.AttachDropShadow(btn_UserManage_AddUser, radius: 16, offset: 4);
            UiStyleHelper.AttachDropShadow(btn_UserManage_EditUser, radius: 16, offset: 4);
            UiStyleHelper.AttachDropShadow(btn_UserManage_DeleteUser, radius: 16, offset: 4);
            UiStyleHelper.AttachDropShadow(btn_UserManage_ResetPW, radius: 16, offset: 4);
            UiStyleHelper.AttachDropShadow(btn_UserManage_Refresh, radius: 16, offset: 4);

            StyleAllButtons(this);

            DGV_UserManagement.Rows.Add("윤영서", "1", "temp1234","Admin", "활성", "2025-11-11 18:10");
            DGV_UserManagement.Rows.Add("박민준", "2", "temp1234", "Operator", "활성", "2025-11-10 14:22");
            DGV_UserManagement.Rows.Add("성요셉", "3", "temp1234", "Operator", "비활성", "2025-11-10 14:22");
            DGV_UserManagement.Rows.Add("가대교", "4", "temp1234", "Operator", "활성", "2025-11-10 14:22");
            DGV_UserManagement.Rows.Add("차승우", "5", "temp1234", "Operator", "활성", "2025-11-10 14:22");
            DGV_UserManagement.Rows.Add("김찬송", "6", "temp1234", "Operator", "비활성", "2025-11-10 14:22");
            DGV_UserManagement.Rows.Add("황장보", "7", "temp1234", "Operator", "활성", "2025-11-10 14:22");
            DGV_UserManagement.Rows.Add("허주옥", "8", "temp1234", "Operator", "활성", "2025-11-10 14:22");
            DGV_UserManagement.Rows.Add("이서연", "9", "temp1234", "Operator", "활성", "2025-11-10 14:22");
        }
        private void btn_UserSearch_Click(object sender, EventArgs e)
        {

        }

        private void btn_UserManage_Search_Click(object sender, EventArgs e)
        {

        }

        private void btn_UserManage_AddUser_Click(object sender, EventArgs e)
        {
            OpenDetailsRequested?.Invoke(this, EventArgs.Empty);

            if (sender is Button btn)
                UiStyleHelper.HighlightButton(btn);

            // 🔹 UserManagementForm_AddUser 열기
            UserManagementForm_AddUser form = new UserManagementForm_AddUser();
            form.StartPosition = FormStartPosition.CenterParent; // 부모 기준 중앙 정렬
            form.Show();
        }

        private void btn_UserManage_EditUser_Click(object sender, EventArgs e)
        {
            OpenDetailsRequested?.Invoke(this, EventArgs.Empty);

            if (sender is Button btn)
                UiStyleHelper.HighlightButton(btn);

            // 🔹 UserManagementForm_EditUser 열기
            UserManagementForm_EditUser form = new UserManagementForm_EditUser();
            form.StartPosition = FormStartPosition.CenterParent; // 부모 기준 중앙 정렬
            form.Show();
        }

        private void btn_UserManage_DeleteUser_Click(object sender, EventArgs e)
        {
            MessageBox.Show("'윤영서'가 삭제되었습니다.", "사용자 삭제", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btn_UserManage_ResetPW_Click(object sender, EventArgs e)
        {
            MessageBox.Show("'윤영서'의 비밀번호가 'temp1234'로 리셋되었습니다.", "비밀번호 초기화", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btn_UserManage_Refresh_Click(object sender, EventArgs e)
        {

        }
    }
}
