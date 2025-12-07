using ComponentFactory.Krypton.Toolkit;
using pcb_monitoring_program;
using pcb_monitoring_program.DatabaseManager.Repositories; // DB 연동
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

        private readonly UserRepository _userRepo = new UserRepository();

        public UserManagementView()
        {
            InitializeComponent();
            this.Load += UserManagementView_Load;

            DGV_UserManagement.CellPainting += DGV_UserManagement_CellPainting;
            DGV_UserManagement.SelectionChanged += DGV_UserManagement_SelectionChanged;

            TextBox_UM_ID.KeyDown += TextBox_UM_ID_KeyDown;
            kComboBox_UM_Role.SelectedIndexChanged += kComboBox_UM_Role_SelectedIndexChanged;
        }

        private void DGV_UserManagement_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            var grid = (KryptonDataGridView)sender;

            // 1. 선택된 행인지 확인
            if (e.RowIndex >= 0 && grid.Rows[e.RowIndex].Selected)
            {
                // ... (선택된 셀에 테두리를 그리는 기존 로직) ...
                Color highlightColor = Color.FromArgb(255, 180, 0);
                int borderWidth = 2;

                // 3. 셀의 기본 그리기 작업을 수행합니다. (배경과 내용을 먼저 그립니다.)
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.Border);

                using (Pen p = new Pen(highlightColor, borderWidth))
                {
                    Rectangle rect = e.CellBounds;

                    // ... (A, B, C, D 테두리 그리기 로직) ...

                    // A) 윗 테두리
                    if (e.RowIndex == 0 || !grid.Rows[e.RowIndex - 1].Selected)
                    {
                        e.Graphics.DrawLine(p, rect.Left, rect.Top, rect.Right, rect.Top);
                    }

                    // B) 아랫 테두리
                    if (e.RowIndex == grid.RowCount - 1 || !grid.Rows[e.RowIndex + 1].Selected)
                    {
                        e.Graphics.DrawLine(p, rect.Left, rect.Bottom - borderWidth / 2, rect.Right, rect.Bottom - borderWidth / 2);
                    }

                    // C) 좌측 테두리
                    if (e.ColumnIndex == 0)
                    {
                        e.Graphics.DrawLine(p, rect.Left, rect.Top, rect.Left, rect.Bottom);
                    }

                    // D) 우측 테두리
                    if (e.ColumnIndex == grid.ColumnCount - 1)
                    {
                        e.Graphics.DrawLine(p, rect.Right - borderWidth / 2, rect.Top, rect.Right - borderWidth / 2, rect.Bottom);
                    }
                }

                // 렌더링이 완료되었음을 알리고, 기본 렌더링을 막습니다.
                e.Handled = true;
            }
        }

        private void DGV_UserManagement_SelectionChanged(object sender, EventArgs e)
        {
            // 선택 영역이 바뀔 때마다 전체 그리드를 다시 그리도록 요청합니다.
            DGV_UserManagement.Invalidate();
            // 또는 DGV_UserManagement.Refresh();
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

            // 🎨 행 전체 선택 모드로 설정
            DGV_UserManagement.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DGV_UserManagement.MultiSelect = false;  // 한 행만 선택

            // 1. 콤보박스 항목 초기화 로직 추가 (필수)
            InitializeRoleComboBox();

            StyleAllButtons(this);

            LoadUsersFromDB();
        }

        // 💡 새로 추가할 메서드
        private void InitializeRoleComboBox()
        {
            // 콤보박스 초기화
            kComboBox_UM_Role.Items.Clear();

            // 🚨 여기서 실제 필요한 권한 목록을 추가해야 합니다.
            // 만약 DB에서 권한 목록을 가져와야 한다면, _userRepo에 해당 메서드를 구현해야 합니다.

            // 예시: 권한이 고정되어 있는 경우
            kComboBox_UM_Role.Items.Add("전체"); // 검색 필터용
            kComboBox_UM_Role.Items.Add("Admin");
            kComboBox_UM_Role.Items.Add("Operator");
            kComboBox_UM_Role.Items.Add("Viewer");

            // 초기 선택 값 설정 (SelectedIndexChanged 이벤트가 발생할 수 있습니다)
            if (kComboBox_UM_Role.Items.Count > 0)
            {
                kComboBox_UM_Role.SelectedIndex = 0; // '전체'가 첫 번째 항목이라고 가정
            }
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
            var grid = DGV_UserManagement;

            // 🔒 읽기 전용
            DGV_UserManagement.ReadOnly = true;

            // 🔁 카드 안 폭에 맞춰서 열 자동으로 채우기
            DGV_UserManagement.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            DGV_UserManagement.ScrollBars = ScrollBars.Vertical; // 가로 스크롤 안 쓰면 Vertical 만

            // 열 헤더 텍스트
            DGV_UserManagement.Columns["username"].HeaderText = "아이디";
            DGV_UserManagement.Columns["full_name"].HeaderText = "사용자 이름";
            DGV_UserManagement.Columns["role"].HeaderText = "권한";
            DGV_UserManagement.Columns["status_text"].HeaderText = "상태";
            DGV_UserManagement.Columns["last_login"].HeaderText = "마지막 로그인";
            DGV_UserManagement.Columns["created_at"].HeaderText = "생성일";

            // 🔢 너비 대신 FillWeight로 비율만 지정 (원하면)
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
            var grid = DGV_UserManagement;

            if (grid.CurrentRow == null)
            {
                MessageBox.Show("삭제할 사용자를 선택하세요.", "알림",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // ⚠ 컬럼 이름은 DataTable에 바인딩된 실제 컬럼명과 같아야 함 (ApplyGridStyle에서 이미 "id" 사용 중)
            int id = Convert.ToInt32(grid.CurrentRow.Cells["id"].Value);
            string username = grid.CurrentRow.Cells["username"].Value?.ToString();

            // (선택) 관리자 계정 삭제 막고 싶으면
            string role = grid.CurrentRow.Cells["role"].Value?.ToString();
            if (role == "Admin")
            {
                MessageBox.Show("관리자(Admin) 계정은 삭제할 수 없습니다.", "권한 제한",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 진짜 삭제할 건지 한번 더 확인
            var confirm = MessageBox.Show(
                $"'{username}' 사용자를 정말 삭제하시겠습니까?",
                "사용자 삭제 확인",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (confirm != DialogResult.Yes)
                return;

            // ✅ DB 삭제 실행
            bool success = _userRepo.DeleteUser(id);

            if (success)
            {
                MessageBox.Show($"'{username}' 사용자가 삭제되었습니다.", "삭제 완료",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // ✅ 그리드 새로고침
                LoadUsersFromDB();
            }
            else
            {
                MessageBox.Show("삭제 중 오류가 발생했습니다.\n다시 시도해 주세요.", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_UserManage_ResetPW_Click(object sender, EventArgs e)
        {
            var grid = DGV_UserManagement;

            // 1. 선택된 행이 있는지 확인
            if (grid.CurrentRow == null)
            {
                MessageBox.Show("비밀번호를 초기화할 사용자를 선택하세요.", "알림",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 2. 선택된 사용자의 ID와 사용자 이름 가져오기
            // "id" 및 "username" 컬럼명은 ApplyGridStyle에서 사용 중인 이름을 따릅니다.
            int id = Convert.ToInt32(grid.CurrentRow.Cells["id"].Value);
            string username = grid.CurrentRow.Cells["username"].Value?.ToString();

            // 3. 초기화할 비밀번호 및 확인 메시지 설정
            const string newPassword = "temp1234"; // 💡 초기화할 임시 비밀번호

            var confirm = MessageBox.Show(
                $"'{username}' 사용자의 비밀번호를 '{newPassword}'로 초기화하시겠습니까?",
                "비밀번호 초기화 확인",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (confirm != DialogResult.Yes)
                return;

            // 4. ✅ DB 업데이트 실행 (UserRepository의 메서드를 호출)
            bool success = _userRepo.ResetPassword(id, newPassword);

            if (success)
            {
                MessageBox.Show($"'{username}'의 비밀번호가 '{newPassword}'로 성공적으로 초기화되었습니다.", "비밀번호 초기화 완료",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("비밀번호 초기화 중 오류가 발생했습니다.\nUserRepository에서 DB 연동을 확인하세요.", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void kComboBox_UM_Role_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 1. 텍스트박스 초기화
            TextBox_UM_ID.Text = string.Empty;

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
