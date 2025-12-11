using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pcb_monitoring_program.Views.Monitoring
{
    public partial class OHTMonitoringView : UserControl
    {
        private class OhtStatusInfo
        {
            public string OhtId { get; set; } = "";
            public string Status { get; set; } = "";      // "대기", "이동중" 등
            public string Position { get; set; } = "";    // "Line 1"
            public string Destination { get; set; } = ""; // "BOX-001" ...
            public string CurrentJob { get; set; } = "";  // "정상", "납땜불량" ...
            public DateTime UpdatedAt { get; set; }
        }

        // OHT 전체 리스트
        private readonly List<OhtStatusInfo> _ohtList = new List<OhtStatusInfo>();

        // ★ 박스 → 작업유형 매핑
        private readonly Dictionary<string, string> _boxToJob =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["BOX-001"] = "정상",
                ["BOX-002"] = "부품불량",
                ["BOX-003"] = "S/N 불량",
                ["BOX-004"] = "폐기",
            };

        private bool _autoCallEnabled = false;

        public OHTMonitoringView()
        {
            InitializeComponent();
        }

        private void AddCallHistory(string ohtId, string boxId, string jobName, string note)
        {
            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            var item = new ListViewItem(time);  // 1열: 시간
            item.SubItems.Add(ohtId);           // 2열: OHT ID
            item.SubItems.Add(boxId);           // 3열: BOX ID
            item.SubItems.Add(jobName);         // 4열: 작업 종류
            item.SubItems.Add(note);            // 5열: 비고

            // 위로 쌓기
            lvCallHistory.Items.Insert(0, item);

            if (lvCallHistory.Items.Count > 200)
                lvCallHistory.Items.RemoveAt(lvCallHistory.Items.Count - 1);
        }

        private void OHTMonitoringView_Load(object sender, EventArgs e)
        {
            UiStyleHelper.MakeRoundedPanel(cardOHTMonitoring, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardOHTCall, radius: 16, back: Color.FromArgb(44, 44, 44));

            UiStyleHelper.MakeRoundedPanel(cardOHTCallLog, radius: 16, back: Color.FromArgb(44, 44, 44));

            UiStyleHelper.AddShadowRoundedPanel(cardOHTMonitoring, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardOHTCall, 16);

            UiStyleHelper.AddShadowRoundedPanel(cardOHTCallLog, 16);

            UiStyleHelper.MakeRoundedButton(btnAutoCall, 24);
            UiStyleHelper.MakeRoundedButton(btnOhtCall, 24);

            UiStyleHelper.AttachDropShadow(btnAutoCall, radius: 16, offset: 4);
            UiStyleHelper.AttachDropShadow(btnOhtCall, radius: 16, offset: 4);

            // ▼ 작업유형 콤보는 더이상 사용하지 않음 (숨김)

            // OHT 대상 콤보
            cmbOhtId.Items.Clear();
            cmbOhtId.Items.AddRange(new object[] { "OHT-01", "OHT-02", "OHT-03" });
            cmbOhtId.SelectedIndex = 0;

            // 박스 선택 콤보 (001~004)
            cmbBoxId.Items.Clear();
            cmbBoxId.Items.AddRange(new object[] { "BOX-001", "BOX-002", "BOX-003", "BOX-004" });
            if (cmbBoxId.Items.Count > 0)
                cmbBoxId.SelectedIndex = 0;

            // 자동 호출 버튼 초기 상태
            _autoCallEnabled = false;
            lblLastCall.Text = "최근 호출 :  - ";
            UpdateAutoCallLabel();

            // 초기 OHT 상태
            _ohtList.Add(new OhtStatusInfo { OhtId = "OHT-01", Status = "대기", Position = "Line 1", UpdatedAt = DateTime.Now });
            _ohtList.Add(new OhtStatusInfo { OhtId = "OHT-02", Status = "대기", Position = "Line 1", UpdatedAt = DateTime.Now });
            _ohtList.Add(new OhtStatusInfo { OhtId = "OHT-03", Status = "대기", Position = "Line 1", UpdatedAt = DateTime.Now });

            RefreshOhtGrid();
        }

        private void RefreshOhtGrid()
        {
            dgvOhtStatus.Rows.Clear();

            foreach (var o in _ohtList)
            {
                dgvOhtStatus.Rows.Add(
                    o.OhtId,                        // OHT ID
                    o.Status,                       // 상태
                    o.Position,                     // 위치
                    o.Destination,                  // 목적지
                    o.CurrentJob,                   // 현재 작업
                    o.UpdatedAt.ToString("HH:mm:ss")// 업데이트 시간
                );
            }
        }

        private void UpdateAutoCallLabel()
        {
            if (_autoCallEnabled)
            {
                lblAutoCallStatus.Text = "자동 호출 ON";
                lblAutoCallStatus.ForeColor = Color.LimeGreen;
            }
            else
            {
                lblAutoCallStatus.Text = "자동 호출 OFF";
                lblAutoCallStatus.ForeColor = Color.Gainsboro;
            }
        }

        private void btnAutoCall_Click(object sender, EventArgs e)
        {
            _autoCallEnabled = !_autoCallEnabled;
            UpdateAutoCallLabel();
        }

        // ★ 박스ID로 작업유형 얻기
        private string GetJobByBoxId(string boxId)
        {
            if (string.IsNullOrWhiteSpace(boxId)) return "정상";

            // 1) 사전 매핑 우선
            if (_boxToJob.TryGetValue(boxId.Trim(), out var job))
                return job;

            // 2) (옵션) 숫자 규칙 순환 매핑
            var digits = new string(boxId.Where(char.IsDigit).ToArray());
            if (int.TryParse(digits, out int n))
            {
                switch (((n - 1) % 4 + 4) % 4)
                {
                    case 0: return "정상";
                    case 1: return "부품불량";
                    case 2: return "S/N 불량";
                    case 3: return "폐기";
                }
            }
            return "정상";
        }

        private void btnOhtCall_Click(object sender, EventArgs e)
        {
            if (cmbOhtId.SelectedItem == null || cmbBoxId.SelectedItem == null)
            {
                MessageBox.Show("OHT와 Box를 선택해주세요.");
                return;
            }

            string ohtId = cmbOhtId.SelectedItem.ToString();
            string boxId = cmbBoxId.SelectedItem.ToString();

            // ★ 박스에 따라 작업유형 자동 결정
            string jobName = GetJobByBoxId(boxId);

            // OHT 한 줄 찾기
            var info = _ohtList.FirstOrDefault(x => x.OhtId == ohtId);
            if (info == null)
            {
                info = new OhtStatusInfo { OhtId = ohtId };
                _ohtList.Add(info);
            }

            // 상태 업데이트
            info.Status = "이동중";
            info.Position = "Line 1";
            info.Destination = boxId;
            info.CurrentJob = jobName;
            info.UpdatedAt = DateTime.Now;

            // 그리드 갱신
            RefreshOhtGrid();

            // 최근 호출 라벨
            lblLastCall.Text = $"최근 호출 : {DateTime.Now:HH:mm:ss}";

            // 호출 이력 기록 (자동 결정된 jobName 사용)
            AddCallHistory(ohtId, boxId, jobName, "즉시 배정");
        }
    }
}
