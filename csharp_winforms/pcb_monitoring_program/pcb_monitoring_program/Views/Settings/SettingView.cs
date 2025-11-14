using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pcb_monitoring_program.Views.Settings
{
    public partial class SettingView : UserControl
    {
        public SettingView()
        {
            InitializeComponent();

            UiStyleHelper.MakeRoundedPanel(cardSetting, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardFlaskServer, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardAlarm, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardTimeout, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardLoglevel, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardMySQL, radius: 16, back: Color.FromArgb(44, 44, 44));

            UiStyleHelper.AddShadowRoundedPanel(cardSetting, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardFlaskServer, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardAlarm, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardTimeout, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardLoglevel, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardMySQL, 16);

            UiStyleHelper.MakeRoundedButton(btn_Setting_Connectiontest, 24);
            UiStyleHelper.MakeRoundedButton(btn_Setting_save, 24);
            UiStyleHelper.MakeRoundedButton(btn_Setting_cancel, 24);
            UiStyleHelper.AttachDropShadow(btn_Setting_Connectiontest, radius: 12, offset: 6);
            UiStyleHelper.AttachDropShadow(btn_Setting_save, radius: 12, offset: 6);
            UiStyleHelper.AttachDropShadow(btn_Setting_cancel, radius: 12, offset: 6);
        }

        private void btn_Setting_Connectiontest_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("MySQL과 연결 테스트를 하시겠습니까?",
         "설정",
         MessageBoxButtons.YesNo,
         MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                MessageBox.Show("연결 테스트에 성공하셨습니다.", "설정", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
                            {
                MessageBox.Show("연결 테스트가 취소되었습니다.", "설정", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btn_Setting_save_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("설정을 저장할까요?",
         "설정",
         MessageBoxButtons.YesNo,
         MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                MessageBox.Show("설정이 저장되었습니다.", "설정", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("설정 저장이 취소되었습니다.", "설정", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
                
        }

        private void btn_Setting_cancel_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("추가를 취소하고 창을 닫을까요?",
                     "설정",
                     MessageBoxButtons.YesNo,
                     MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
                MessageBox.Show("설정이 취소되었습니다.", "설정", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
