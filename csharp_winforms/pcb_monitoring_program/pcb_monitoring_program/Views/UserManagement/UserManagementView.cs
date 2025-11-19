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
using pcb_monitoring_program.DatabaseManager.Repositories; // DB 연동

namespace pcb_monitoring_program.Views.UserManagement
{
    public partial class UserManagementView : UserControl
    {
        public event EventHandler OpenDetailsRequested;

        private readonly UserRepository _userRepo = new UserRepository();

        public UserManagementView()
        {
            InitializeComponent();
            this.Load += UserManagementView_Load;

            TextBox_UM_ID.KeyDown += TextBox_UM_ID_KeyDown;
            kComboBox_UM_Role.SelectedIndexChanged += kComboBox_UM_Role_SelectedIndexChanged;
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

            LoadUsersFromDB();
        }

        private void LoadUsersFromDB()
        {
            var dt = _userRepo.GetAllUsers();   // DB에서 가져오기
            DGV_UserManagement.DataSource = dt;

            ApplyGridStyle();

            DGV_UserManagement.ReadOnly = true;

            // (선택) 줄바꿈 방지
            DGV_UserManagement.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
        }

        private void ApplyGridStyle()
        {
            // 🔒 읽기 전용
            DGV_UserManagement.ReadOnly = true;

            // 🔁 카드 안 폭에 맞춰서 열 자동으로 채우기
            DGV_UserManagement.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            DGV_UserManagement.ScrollBars = ScrollBars.Vertical; // 가로 스크롤 안 쓰면 Vertical 만

            // 열 헤더 텍스트
            DGV_UserManagement.Columns["id"].HeaderText = "번호";
            DGV_UserManagement.Columns["username"].HeaderText = "아이디";
            DGV_UserManagement.Columns["full_name"].HeaderText = "사용자 이름";
            DGV_UserManagement.Columns["role"].HeaderText = "권한";
            DGV_UserManagement.Columns["status_text"].HeaderText = "상태";
            DGV_UserManagement.Columns["last_login"].HeaderText = "마지막 로그인";
            DGV_UserManagement.Columns["created_at"].HeaderText = "생성일";

            // 🔢 너비 대신 FillWeight로 비율만 지정 (원하면)
            DGV_UserManagement.Columns["id"].FillWeight = 40;
            DGV_UserManagement.Columns["username"].FillWeight = 120;
            DGV_UserManagement.Columns["full_name"].FillWeight = 120;
            DGV_UserManagement.Columns["role"].FillWeight = 80;
            DGV_UserManagement.Columns["status_text"].FillWeight = 80;
            DGV_UserManagement.Columns["last_login"].FillWeight = 140;
            DGV_UserManagement.Columns["created_at"].FillWeight = 140;

            // 📅 날짜 포맷
            DGV_UserManagement.Columns["last_login"].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm";
            DGV_UserManagement.Columns["created_at"].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm";
        }

        private void btn_UserManage_Search_Click(object sender, EventArgs e)
        {
            var result = PerformSearch();

            switch (result)
            {
                case SearchResultStatus.AllLoaded:
                    MessageBox.Show("전체 조회되었습니다.", "알림",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;

                case SearchResultStatus.NoMatch:
                    MessageBox.Show("없는 아이디 입니다.", "결과 없음",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;

                case SearchResultStatus.HasResult:
                    MessageBox.Show("조회되었습니다.", "알림",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
            }
        }

        private void btn_UserManage_AddUser_Click(object sender, EventArgs e)
        {
            OpenDetailsRequested?.Invoke(this, EventArgs.Empty);

            if (sender is Button btn)
                UiStyleHelper.HighlightButton(btn);

            // 🔹 UserManagementForm_AddUser 모달로 열기
            using (var form = new UserManagementForm_AddUser())
            {
                form.StartPosition = FormStartPosition.CenterParent; // 부모 기준 중앙 정렬

                // 저장 성공 시 AddUser 폼에서 DialogResult = OK 로 설정해 준다는 가정
                if (form.ShowDialog() == DialogResult.OK)
                {
                    // ✅ 새로고침 (DB 다시 읽기)
                    LoadUsersFromDB();
                }
            }
        }

        private void btn_UserManage_Refresh_Click(object sender, EventArgs e)
        {
            // 1. 텍스트박스 초기화
            TextBox_UM_ID.Text = string.Empty;

            // 2. 콤보박스 '전체'로 변경
            if (kComboBox_UM_Role.Items.Count > 0)
                kComboBox_UM_Role.SelectedIndex = 0;   // '전체'

            // 3. 표 전체 다시 로드
            LoadUsersFromDB();

            // 4. 안내 메시지
            MessageBox.Show("새로고침되었습니다.", "알림",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btn_UserManage_EditUser_Click(object sender, EventArgs e)
        {
            var grid = DGV_UserManagement;

            if (grid.CurrentRow == null)
            {
                MessageBox.Show("수정할 사용자를 선택하세요.", "알림",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // ⚠ 컬럼 이름 "Id" → "id" 로 통일 (ApplyGridStyle에서 소문자 id 사용 중)
            int id = Convert.ToInt32(grid.CurrentRow.Cells["id"].Value);
            string username = grid.CurrentRow.Cells["username"].Value?.ToString();
            string fullName = grid.CurrentRow.Cells["full_name"].Value?.ToString();
            string role = grid.CurrentRow.Cells["role"].Value?.ToString();

            // 상태값이 "활성"/"비활성" 같은 문자열이라면:
            string stateStr = grid.CurrentRow.Cells["status_text"].Value?.ToString();
            bool isActive = stateStr == "활성" || stateStr == "True" || stateStr == "1";

            using (var form = new UserManagementForm_EditUser(id, username, fullName, role, isActive))
            {
                form.StartPosition = FormStartPosition.CenterParent;

                // ✅ 수정 성공 & OK 일 때만 새로고침
                if (form.ShowDialog() == DialogResult.OK && form.IsUpdated)
                {
                    LoadUsersFromDB();   // DB 다시 읽어서 표 새로고침
                }
            }
        }

        private void btn_UserManage_DeleteUser_Click(object sender, EventArgs e)
        {
            MessageBox.Show("'윤영서'가 삭제되었습니다.", "사용자 삭제", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btn_UserManage_ResetPW_Click(object sender, EventArgs e)
        {
            MessageBox.Show("'윤영서'의 비밀번호가 'temp1234'로 리셋되었습니다.", "비밀번호 초기화", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void kComboBox_UM_Role_SelectedIndexChanged(object sender, EventArgs e)
        {
            PerformSearch();
        }

        private void TextBox_UM_ID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btn_UserManage_Search.PerformClick();  // 검색 버튼 강제 클릭
                e.SuppressKeyPress = true;             // 삑 소리/줄바꿈 방지
            }
        }

        private enum SearchResultStatus
        {
            AllLoaded,   // 전체 목록 로드
            NoMatch,     // 조건에 맞는 행 없음
            HasResult    // 정상적으로 필터된 결과 있음
        }

        private SearchResultStatus PerformSearch()
        {
            string username = TextBox_UM_ID.Text.Trim();
            string roleFilter = kComboBox_UM_Role.SelectedItem?.ToString() ?? "전체";

            // ✅ 1) 조건이 아무것도 없고, 권한도 '전체'면 → 전체 로드
            if (string.IsNullOrWhiteSpace(username) && roleFilter == "전체")
            {
                LoadUsersFromDB();
                return SearchResultStatus.AllLoaded;
            }

            // ✅ 2) DB에서 조건 검색 (부분검색 + 권한 필터는 UserRepository에서 처리)
            var dt = _userRepo.SearchUsers(username, roleFilter);

            // ✅ 3) 결과 없음
            if (dt.Rows.Count == 0)
            {
                DGV_UserManagement.DataSource = null;
                return SearchResultStatus.NoMatch;
            }

            // ✅ 4) 결과 있음 → 그리드에 바인딩
            DGV_UserManagement.DataSource = dt;
            ApplyGridStyle();
            return SearchResultStatus.HasResult;
        }

        private void DGV_UserManagement_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;   // 헤더 더블클릭 방지

            // 그냥 수정 버튼 클릭 재사용
            btn_UserManage_EditUser.PerformClick();
        }
    }
}
