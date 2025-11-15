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
    public partial class UserManagementForm_AddUser : Form
    {
        public UserManagementForm_AddUser()
        {
            InitializeComponent();

            UiStyleHelper.MakeRoundedPanel(cardAddUser, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.AddShadowRoundedPanel(cardAddUser, 16);

            UiStyleHelper.MakeRoundedButton(btn_UM_ADD_AddUser, 24);
            UiStyleHelper.MakeRoundedButton(btn_UM_ADD_cancel, 24);
            UiStyleHelper.AttachDropShadow(btn_UM_ADD_AddUser, radius: 12, offset: 6);
            UiStyleHelper.AttachDropShadow(btn_UM_ADD_cancel, radius: 12, offset: 6);
        }
        private void btn_UserManage_AddUser_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("추가할까요?",
                     "추가 확인",
                     MessageBoxButtons.YesNo,
                     MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
                this.Close();
        }

        private void btn_UserManage_cancel_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("추가를 취소하고 창을 닫을까요?",
                                  "취소 확인",
                                  MessageBoxButtons.YesNo,
                                  MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
                this.Close();
        }
    }
}
