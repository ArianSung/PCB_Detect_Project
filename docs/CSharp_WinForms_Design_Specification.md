# C# WinForms PCB 검사 모니터링 시스템 - 설계 사양서

## 문서 정보

**작성일**: 2025-10-22
**버전**: 1.0
**관련 문서**:
- `CSharp_WinForms_Guide.md` - 기본 개발 가이드
- `MySQL_Database_Design.md` - 데이터베이스 스키마
- `PCB_Defect_Detection_Project.md` - 전체 시스템 아키텍처

---

## 개요

### 시스템 역할

- **실시간 모니터링**: Flask 서버로부터 검사 현황을 실시간으로 표시
- **데이터 조회**: MySQL 데이터베이스에서 검사 이력 및 통계 조회
- **데이터 내보내기**: Excel, CSV 형식으로 데이터 내보내기
- **시스템 관리**: 사용자 관리, 시스템 설정 (관리자 전용)

---

## 사용자 권한 체계

### 권한 레벨

#### 1. Admin (관리자)
- 모든 기능 접근
- 사용자 계정 관리
- 시스템 설정 변경

#### 2. Operator (작업자)
- 검사 이력 조회 및 통계 분석
- Excel/CSV 내보내기
- 불량 이미지 확인
- **제한**: 사용자 관리, 시스템 설정 불가

#### 3. Viewer (조회자)
- 읽기 전용 접근
- **제한**: 데이터 내보내기, 설정 변경 불가

### 권한별 기능 접근 매트릭스

| 기능 | Admin | Operator | Viewer |
|------|-------|----------|--------|
| 메인 대시보드 | ✅ | ✅ | ✅ |
| 검사 이력 조회 | ✅ | ✅ | ✅ (읽기만) |
| 통계 분석 | ✅ | ✅ | ✅ (읽기만) |
| Excel 내보내기 | ✅ | ✅ | ❌ |
| **OHT 호출** ⭐ | ✅ | ✅ | ❌ |
| 사용자 관리 | ✅ | ❌ | ❌ |
| 시스템 설정 | ✅ | ❌ | ❌ |

---

## 화면 설계

### 1. LoginForm (로그인 화면)

#### 주요 기능
- 사용자 인증 (BCrypt 비밀번호 해싱)
- 로그인 시도 횟수 제한 (5회)
- 로그인 실패 시 임시 잠금 (5분)

#### 핵심 코드

```csharp
private async void btnLogin_Click(object sender, EventArgs e)
{
    try
    {
        var user = await AuthService.LoginAsync(txtUsername.Text, txtPassword.Text);

        if (user != null)
        {
            SessionManager.CurrentUser = user;
            SessionManager.LastActivity = DateTime.Now;

            MainForm mainForm = new MainForm();
            mainForm.Show();
            this.Hide();
        }
        else
        {
            LoginAttempts++;
            if (LoginAttempts >= 5)
            {
                lblError.Text = "로그인 시도 횟수 초과. 5분 후 다시 시도하세요.";
                btnLogin.Enabled = false;
                StartLockoutTimer();
            }
        }
    }
    catch (Exception ex)
    {
        Logger.LogError("Login error", ex);
    }
}
```

---

### 2. MainForm (메인 대시보드)

#### 주요 컴포넌트

**실시간 통계 카드:**
- 총 검사 건수, 정상, 불량, 불량률
- 1초마다 자동 새로고침

**차트:**
- LiveCharts.WinForms.PieChart (불량 유형별 비율)
- LiveCharts.WinForms.CartesianChart (시간대별 검사 현황)
- 범례: `LegendItem` 커스텀 컨트롤 사용

**시스템 상태:**
- `StatusIndicator` 커스텀 컨트롤 사용 (색상 동그라미)
- 서버, DB, 라즈베리파이 온라인 상태
- CPU/RAM/GPU 사용률 (ProgressBar)

**최근 검사 이력:**
- DataGridView (최근 10건)

**박스 상태 모니터링:** ⭐ 로봇팔 시스템
- 3개 박스(정상/부품불량/납땜불량) 실시간 상태 표시
- 각 박스의 슬롯 사용 현황 (2/2 표시, 수직 2단 적재)
- DISCARD는 슬롯 관리 안 함 (고정 위치에 떨어뜨리기)
- 꽉 찬 박스 빨간색 LED 경고
- "박스 리셋" 버튼 (박스 교체 후)
- 2초마다 자동 새로고침

#### 권한별 메뉴 표시

```csharp
private void InitializeMenuByRole()
{
    var currentUser = SessionManager.CurrentUser;

    // Admin만 표시
    menuUserManagement.Visible = currentUser.Role == UserRole.Admin;

    // Operator 이상만 표시
    menuSettings.Visible = currentUser.Role == UserRole.Admin ||
                            currentUser.Role == UserRole.Operator;
}
```

#### 박스 상태 모니터링 UI

3개 박스(정상/부품불량/납땜불량)의 실시간 상태를 표시합니다.
DISCARD는 슬롯 관리 없이 고정 위치에 떨어뜨리므로 UI에 표시하지 않습니다.

**UI 레이아웃:**

```
┌─────────────────────────────────────────────────────────────┐
│ 박스 상태 모니터링 (로봇팔 시스템)                           │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  [정상]  ●●  2/2  [꽉 참!] [리셋]                            │
│                                                               │
│  [부품불량]  ●○  1/2                                         │
│                                                               │
│  [납땜불량]  ○○  0/2                                         │
│                                                               │
│  [전체 박스 상태] 꽉 참: 1개 | 사용 중: 1개 | 빈 박스: 1개    │
│                                                               │
│  참고: 폐기(DISCARD)는 슬롯 관리 없음 (고정 위치 떨어뜨리기)  │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

**BoxStatusPanel 커스텀 컨트롤:**

```csharp
public class BoxStatusPanel : Panel
{
    private Label lblBoxId;
    private Panel pnlSlots;
    private Label lblCount;
    private Button btnReset;
    private Label lblFullAlert;

    private string boxId;
    private int currentSlot;
    private int maxSlots = 2;  // 수직 2단 적재
    private bool isFull;

    public BoxStatusPanel(string boxId)
    {
        this.boxId = boxId;
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        this.Size = new Size(400, 50);
        this.BorderStyle = BorderStyle.FixedSingle;

        // 박스 ID 라벨
        lblBoxId = new Label
        {
            Text = GetBoxDisplayName(boxId),
            Font = new Font("맑은 고딕", 10, FontStyle.Bold),
            Location = new Point(10, 15),
            AutoSize = true
        };
        this.Controls.Add(lblBoxId);

        // 슬롯 표시 패널 (동그라미 2개)
        pnlSlots = new Panel
        {
            Location = new Point(120, 10),
            Size = new Size(60, 30)  // 2개만 표시하므로 너비 줄임
        };
        pnlSlots.Paint += PnlSlots_Paint;
        this.Controls.Add(pnlSlots);

        // 슬롯 카운트 라벨
        lblCount = new Label
        {
            Text = "0/2",
            Font = new Font("맑은 고딕", 10),
            Location = new Point(200, 15),  // 위치 조정
            AutoSize = true
        };
        this.Controls.Add(lblCount);

        // 꽉 참 경고
        lblFullAlert = new Label
        {
            Text = "꽉 참!",
            Font = new Font("맑은 고딕", 9, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.Red,
            Location = new Point(260, 12),  // 위치 조정
            Size = new Size(50, 25),
            TextAlign = ContentAlignment.MiddleCenter,
            Visible = false
        };
        this.Controls.Add(lblFullAlert);

        // 리셋 버튼 (Admin/Operator만)
        btnReset = new Button
        {
            Text = "리셋",
            Location = new Point(320, 12),  // 위치 조정
            Size = new Size(50, 25),
            Enabled = false,
            Visible = false
        };
        btnReset.Click += BtnReset_Click;
        this.Controls.Add(btnReset);
    }

    public void UpdateStatus(int currentSlot, bool isFull)
    {
        this.currentSlot = currentSlot;
        this.isFull = isFull;

        lblCount.Text = $"{currentSlot}/{maxSlots}";
        pnlSlots.Invalidate();

        // 꽉 참 표시
        if (isFull)
        {
            lblFullAlert.Visible = true;
            btnReset.Visible = true;
            btnReset.Enabled = true;
            this.BackColor = Color.FromArgb(255, 230, 230);  // 연한 빨강
        }
        else
        {
            lblFullAlert.Visible = false;
            btnReset.Visible = false;
            this.BackColor = Color.White;
        }
    }

    private void PnlSlots_Paint(object sender, PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        int circleSize = 18;
        int spacing = 25;
        int startX = 10;
        int startY = 5;

        // 2개 슬롯 동그라미 그리기 (수직 2단)
        for (int i = 0; i < maxSlots; i++)
        {
            int x = startX + (i * spacing);
            int y = startY;

            Color fillColor;
            if (i < currentSlot)
            {
                // 사용 중인 슬롯 (채워진 동그라미)
                fillColor = GetCategoryColor(boxId);
            }
            else
            {
                // 빈 슬롯 (빈 동그라미)
                fillColor = Color.LightGray;
            }

            using (SolidBrush brush = new SolidBrush(fillColor))
            {
                g.FillEllipse(brush, x, y, circleSize, circleSize);
            }

            // 테두리
            using (Pen pen = new Pen(Color.Gray, 1))
            {
                g.DrawEllipse(pen, x, y, circleSize, circleSize);
            }
        }
    }

    private Color GetCategoryColor(string boxId)
    {
        if (boxId.Contains("NORMAL")) return Color.Green;
        if (boxId.Contains("COMPONENT")) return Color.Orange;
        if (boxId.Contains("SOLDER")) return Color.Blue;
        if (boxId.Contains("DISCARD")) return Color.Red;
        return Color.Gray;
    }

    private string GetBoxDisplayName(string boxId)
    {
        return boxId.Replace("_", " ");
    }

    private async void BtnReset_Click(object sender, EventArgs e)
    {
        // 확인 대화상자
        var result = MessageBox.Show(
            $"박스 {boxId}를 리셋하시겠습니까?\n(OHT가 박스를 교체한 후에만 리셋하세요)",
            "박스 리셋 확인",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result == DialogResult.Yes)
        {
            // API 호출
            bool success = await ApiClient.ResetBoxAsync(boxId);

            if (success)
            {
                MessageBox.Show($"박스 {boxId}가 리셋되었습니다.", "성공",
                               MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 상태 갱신
                UpdateStatus(0, false);
            }
            else
            {
                MessageBox.Show("박스 리셋 실패", "오류",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
```

**MainForm에 박스 모니터링 통합:**

```csharp
public partial class MainForm : Form
{
    private System.Windows.Forms.Timer timerBoxStatus;
    private Dictionary<string, BoxStatusPanel> boxPanels = new Dictionary<string, BoxStatusPanel>();

    private void InitializeBoxStatusMonitoring()
    {
        // 3개 박스 ID (DISCARD는 슬롯 관리 안 함)
        string[] boxIds = {
            "NORMAL",
            "COMPONENT_DEFECT",
            "SOLDER_DEFECT"
        };

        // GroupBox에 박스 패널 추가
        GroupBox grpBoxStatus = new GroupBox
        {
            Text = "박스 상태 모니터링 (로봇팔 시스템)",
            Location = new Point(20, 400),
            Size = new Size(450, 300),  // 높이 조정 (3개 박스만)
            Font = new Font("맑은 고딕", 10, FontStyle.Bold)
        };

        int yPos = 30;
        foreach (string boxId in boxIds)
        {
            BoxStatusPanel panel = new BoxStatusPanel(boxId)
            {
                Location = new Point(10, yPos)
            };

            boxPanels[boxId] = panel;
            grpBoxStatus.Controls.Add(panel);

            yPos += 55;
        }

        this.Controls.Add(grpBoxStatus);

        // 요약 라벨 추가
        Label lblSummary = new Label
        {
            Name = "lblBoxSummary",
            Location = new Point(20, yPos + 10),
            Size = new Size(420, 30),
            Font = new Font("맑은 고딕", 9),
            TextAlign = ContentAlignment.MiddleLeft
        };
        grpBoxStatus.Controls.Add(lblSummary);

        // 2초마다 갱신 타이머
        timerBoxStatus = new System.Windows.Forms.Timer();
        timerBoxStatus.Interval = 2000;  // 2초
        timerBoxStatus.Tick += TimerBoxStatus_Tick;
        timerBoxStatus.Start();

        // 초기 로드
        LoadBoxStatus();
    }

    private async void TimerBoxStatus_Tick(object sender, EventArgs e)
    {
        await LoadBoxStatus();
    }

    private async Task LoadBoxStatus()
    {
        try
        {
            // API 호출: /box_status
            var response = await ApiClient.GetBoxStatusAsync();

            if (response != null && response.Status == "ok")
            {
                // 각 박스 패널 업데이트
                foreach (var box in response.Boxes)
                {
                    if (boxPanels.ContainsKey(box.BoxId))
                    {
                        boxPanels[box.BoxId].UpdateStatus(box.CurrentSlot, box.IsFull);
                    }
                }

                // 요약 정보 업데이트
                UpdateBoxSummary(response.Summary);

                // 박스 꽉 참 알림
                CheckBoxFullAlert(response.Boxes);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("박스 상태 조회 실패", ex);
        }
    }

    private void UpdateBoxSummary(BoxSummary summary)
    {
        Label lblSummary = this.Controls.Find("lblBoxSummary", true).FirstOrDefault() as Label;
        if (lblSummary != null)
        {
            lblSummary.Text = $"[전체 박스 상태] 꽉 참: {summary.FullBoxes}개 | " +
                             $"사용 중: {summary.TotalBoxes - summary.EmptyBoxes - summary.FullBoxes}개 | " +
                             $"빈 박스: {summary.EmptyBoxes}개";

            // 시스템 정지 경고
            if (summary.SystemStopped)
            {
                lblSummary.ForeColor = Color.Red;
                lblSummary.Font = new Font(lblSummary.Font, FontStyle.Bold);

                // 알림 표시
                ShowSystemStoppedAlert();
            }
            else
            {
                lblSummary.ForeColor = Color.Black;
                lblSummary.Font = new Font(lblSummary.Font, FontStyle.Regular);
            }
        }
    }

    private void CheckBoxFullAlert(List<BoxInfo> boxes)
    {
        // 꽉 찬 박스가 있으면 경고음 (한 번만)
        var fullBoxes = boxes.Where(b => b.IsFull).ToList();

        if (fullBoxes.Any() && !alertShown)
        {
            alertShown = true;
            System.Media.SystemSounds.Exclamation.Play();

            // 토스트 알림 (선택)
            ShowBoxFullNotification(fullBoxes);
        }
        else if (!fullBoxes.Any())
        {
            alertShown = false;
        }
    }

    private bool alertShown = false;

    private void ShowBoxFullNotification(List<BoxInfo> fullBoxes)
    {
        string message = "다음 박스가 꽉 찼습니다:\n" +
                        string.Join(", ", fullBoxes.Select(b => b.BoxId));

        // 우측 하단에 토스트 알림 표시 (3초 후 자동 사라짐)
        Form toastForm = new Form
        {
            FormBorderStyle = FormBorderStyle.None,
            BackColor = Color.FromArgb(255, 200, 200),
            Size = new Size(300, 100),
            StartPosition = FormStartPosition.Manual,
            Location = new Point(
                Screen.PrimaryScreen.WorkingArea.Right - 320,
                Screen.PrimaryScreen.WorkingArea.Bottom - 120),
            TopMost = true,
            ShowInTaskbar = false
        };

        Label lblMessage = new Label
        {
            Text = message,
            Font = new Font("맑은 고딕", 10),
            AutoSize = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        };

        toastForm.Controls.Add(lblMessage);
        toastForm.Show();

        // 3초 후 자동 닫기
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        timer.Interval = 3000;
        timer.Tick += (s, e) => {
            toastForm.Close();
            timer.Stop();
        };
        timer.Start();
    }

    private void ShowSystemStoppedAlert()
    {
        // 시스템 정지 대화상자 (한 번만 표시)
        if (!systemStopAlertShown)
        {
            systemStopAlertShown = true;

            MessageBox.Show(
                "모든 박스가 꽉 찼습니다!\n" +
                "시스템이 정지되었습니다.\n\n" +
                "OHT를 호출하여 박스를 교체한 후\n" +
                "박스 리셋 버튼을 눌러주세요.",
                "시스템 정지",
                MessageBoxButtons.OK,
                MessageBoxIcon.Stop);
        }
    }

    private bool systemStopAlertShown = false;

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // 타이머 정지
        timerBoxStatus?.Stop();
        base.OnFormClosing(e);
    }
}
```

**ApiClient 클래스 (박스 상태 API):**

```csharp
public static class ApiClient
{
    private static readonly HttpClient client = new HttpClient();
    // 기본: http://100.64.1.1:5000 (Tailscale)
    // 원격: http://100.x.x.x:5000 (Tailscale VPN - 프로젝트 환경)
    private const string BASE_URL = "http://100.64.1.1:5000";  // Flask 서버

    public static async Task<BoxStatusResponse> GetBoxStatusAsync()
    {
        try
        {
            var response = await client.GetAsync($"{BASE_URL}/box_status");
            response.EnsureSuccessStatusCode();

            string jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<BoxStatusResponse>(jsonResponse);
        }
        catch (Exception ex)
        {
            Logger.LogError("박스 상태 조회 API 오류", ex);
            return null;
        }
    }

    public static async Task<bool> ResetBoxAsync(string boxId)
    {
        try
        {
            var requestData = new { box_id = boxId };
            string jsonContent = JsonConvert.SerializeObject(requestData);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{BASE_URL}/box_status/reset", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Logger.LogError("박스 리셋 API 오류", ex);
            return false;
        }
    }
}

// DTO 클래스
public class BoxStatusResponse
{
    [JsonProperty("status")]
    public string Status { get; set; }

    [JsonProperty("boxes")]
    public List<BoxInfo> Boxes { get; set; }

    [JsonProperty("summary")]
    public BoxSummary Summary { get; set; }
}

public class BoxInfo
{
    [JsonProperty("box_id")]
    public string BoxId { get; set; }

    [JsonProperty("category")]
    public string Category { get; set; }

    [JsonProperty("box_type")]
    public string BoxType { get; set; }

    [JsonProperty("current_slot")]
    public int CurrentSlot { get; set; }

    [JsonProperty("max_slots")]
    public int MaxSlots { get; set; }

    [JsonProperty("is_full")]
    public bool IsFull { get; set; }

    [JsonProperty("total_pcb_count")]
    public int TotalPcbCount { get; set; }

    [JsonProperty("utilization_rate")]
    public double UtilizationRate { get; set; }
}

public class BoxSummary
{
    [JsonProperty("total_boxes")]
    public int TotalBoxes { get; set; }

    [JsonProperty("full_boxes")]
    public int FullBoxes { get; set; }

    [JsonProperty("empty_boxes")]
    public int EmptyBoxes { get; set; }

    [JsonProperty("system_stopped")]
    public bool SystemStopped { get; set; }
}
```

#### OHT (Overhead Hoist Transport) 제어 패널 ⭐ 신규

**주요 기능:**
- 박스 꽉 참 시 자동 OHT 호출 (시스템 자동)
- 수동 OHT 호출 (Admin/Operator 권한 필요)
- OHT 상태 실시간 모니터링
- 대기 큐 표시

**UI 레이아웃:**

```
┌────────────────────────────────────────────────────────────┐
│ OHT 시스템 제어 (Admin/Operator 전용)                       │
├────────────────────────────────────────────────────────────┤
│                                                              │
│  [정상 호출]  [부품불량 호출]  [납땜불량 호출]  [긴급정지]   │
│                                                              │
│  현재 상태: ● 대기 중    대기 큐: 0건                        │
│                                                              │
│  최근 호출 이력:                                             │
│  - 2025-10-28 14:23:15  정상 (자동)      완료 (2.3초)       │
│  - 2025-10-28 14:15:32  부품불량 (수동)  완료 (2.5초)       │
│                                                              │
│  참고: Viewer 권한은 OHT 호출 불가 (조회만 가능)            │
│                                                              │
└────────────────────────────────────────────────────────────┘
```

**OHTControlPanel 커스텀 컨트롤:**

```csharp
public class OHTControlPanel : Panel
{
    private Button btnCallNormal;
    private Button btnCallComponentDefect;
    private Button btnCallSolderDefect;
    private Button btnEmergencyStop;
    private Label lblStatus;
    private Label lblQueueLength;
    private DataGridView dgvRecentHistory;
    private System.Windows.Forms.Timer statusTimer;

    private readonly HttpClient httpClient;
    private readonly string serverUrl;

    public OHTControlPanel(string serverUrl)
    {
        this.serverUrl = serverUrl;
        this.httpClient = new HttpClient();
        InitializeComponents();
        InitializePermissions();
        StartStatusMonitoring();
    }

    private void InitializeComponents()
    {
        this.Size = new Size(600, 250);
        this.BorderStyle = BorderStyle.FixedSingle;

        // 호출 버튼들
        btnCallNormal = new Button
        {
            Text = "정상 호출",
            Location = new Point(10, 40),
            Size = new Size(120, 40),
            Font = new Font("맑은 고딕", 10, FontStyle.Bold),
            BackColor = Color.LightGreen
        };
        btnCallNormal.Click += async (s, e) => await CallOHT("NORMAL");
        this.Controls.Add(btnCallNormal);

        btnCallComponentDefect = new Button
        {
            Text = "부품불량 호출",
            Location = new Point(140, 40),
            Size = new Size(120, 40),
            Font = new Font("맑은 고딕", 10, FontStyle.Bold),
            BackColor = Color.LightYellow
        };
        btnCallComponentDefect.Click += async (s, e) => await CallOHT("COMPONENT_DEFECT");
        this.Controls.Add(btnCallComponentDefect);

        btnCallSolderDefect = new Button
        {
            Text = "납땜불량 호출",
            Location = new Point(270, 40),
            Size = new Size(120, 40),
            Font = new Font("맑은 고딕", 10, FontStyle.Bold),
            BackColor = Color.LightCoral
        };
        btnCallSolderDefect.Click += async (s, e) => await CallOHT("SOLDER_DEFECT");
        this.Controls.Add(btnCallSolderDefect);

        btnEmergencyStop = new Button
        {
            Text = "긴급 정지",
            Location = new Point(400, 40),
            Size = new Size(120, 40),
            Font = new Font("맑은 고딕", 10, FontStyle.Bold),
            BackColor = Color.Red,
            ForeColor = Color.White
        };
        btnEmergencyStop.Click += BtnEmergencyStop_Click;
        this.Controls.Add(btnEmergencyStop);

        // 상태 표시
        lblStatus = new Label
        {
            Text = "현재 상태: ● 대기 중",
            Location = new Point(10, 90),
            AutoSize = true,
            Font = new Font("맑은 고딕", 10)
        };
        this.Controls.Add(lblStatus);

        lblQueueLength = new Label
        {
            Text = "대기 큐: 0건",
            Location = new Point(200, 90),
            AutoSize = true,
            Font = new Font("맑은 고딕", 10)
        };
        this.Controls.Add(lblQueueLength);

        // 최근 이력 DataGridView
        dgvRecentHistory = new DataGridView
        {
            Location = new Point(10, 120),
            Size = new Size(580, 120),
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            RowHeadersVisible = false
        };
        dgvRecentHistory.Columns.Add("time", "시각");
        dgvRecentHistory.Columns.Add("category", "종류");
        dgvRecentHistory.Columns.Add("type", "유형");
        dgvRecentHistory.Columns.Add("status", "상태");
        dgvRecentHistory.Columns.Add("duration", "소요시간");
        this.Controls.Add(dgvRecentHistory);

        // 권한 경고 라벨
        if (!SessionManager.HasPermission(Permission.CallOHT))
        {
            Label lblPermissionWarning = new Label
            {
                Text = "⚠ OHT 호출 권한이 없습니다 (Admin/Operator 전용)",
                Location = new Point(10, 10),
                ForeColor = Color.Red,
                Font = new Font("맑은 고딕", 9, FontStyle.Bold),
                AutoSize = true
            };
            this.Controls.Add(lblPermissionWarning);
        }
    }

    private void InitializePermissions()
    {
        // Admin/Operator만 호출 버튼 활성화
        bool hasPermission = SessionManager.HasPermission(Permission.CallOHT);

        btnCallNormal.Enabled = hasPermission;
        btnCallComponentDefect.Enabled = hasPermission;
        btnCallSolderDefect.Enabled = hasPermission;
        btnEmergencyStop.Enabled = hasPermission;
    }

    private void StartStatusMonitoring()
    {
        statusTimer = new System.Windows.Forms.Timer();
        statusTimer.Interval = 5000;  // 5초마다
        statusTimer.Tick += async (s, e) => await RefreshOHTStatus();
        statusTimer.Start();

        // 초기 로드
        Task.Run(async () => await RefreshOHTStatus());
    }

    private async Task CallOHT(string category)
    {
        try
        {
            var payload = new
            {
                category = category,
                user_id = SessionManager.CurrentUser.UserId,
                user_role = SessionManager.CurrentUser.Role.ToString()
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync($"{serverUrl}/api/oht/request", content);

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                MessageBox.Show("OHT 호출 권한이 없습니다.", "권한 오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            response.EnsureSuccessStatusCode();

            MessageBox.Show($"{GetCategoryDisplayName(category)} OHT가 호출되었습니다.",
                "성공", MessageBoxButtons.OK, MessageBoxIcon.Information);

            await RefreshOHTStatus();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"OHT 호출 실패: {ex.Message}", "오류",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            Logger.LogError("OHT call failed", ex);
        }
    }

    private async Task RefreshOHTStatus()
    {
        try
        {
            var response = await httpClient.GetAsync($"{serverUrl}/api/oht/status");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var status = JsonConvert.DeserializeObject<OHTStatus>(json);

            // UI 업데이트 (UI 스레드에서)
            this.Invoke(new Action(() =>
            {
                lblQueueLength.Text = $"대기 큐: {status.QueueLength}건";

                if (status.CurrentRequest != null)
                {
                    lblStatus.Text = $"현재 상태: ● 진행 중 ({status.CurrentRequest.Category})";
                    lblStatus.ForeColor = Color.Orange;
                }
                else
                {
                    lblStatus.Text = "현재 상태: ● 대기 중";
                    lblStatus.ForeColor = Color.Green;
                }

                // 최근 이력 업데이트
                dgvRecentHistory.Rows.Clear();
                foreach (var req in status.RecentRequests.Take(3))
                {
                    dgvRecentHistory.Rows.Add(
                        req.Timestamp,
                        GetCategoryDisplayName(req.Category),
                        req.IsAuto ? "자동" : "수동",
                        req.Status == "completed" ? "완료" : req.Status,
                        req.ExecutionTimeSeconds > 0 ? $"{req.ExecutionTimeSeconds:F1}초" : "-"
                    );
                }
            }));
        }
        catch (Exception ex)
        {
            Logger.LogError("Failed to refresh OHT status", ex);
        }
    }

    private void BtnEmergencyStop_Click(object sender, EventArgs e)
    {
        var result = MessageBox.Show("OHT를 긴급 정지하시겠습니까?",
            "긴급 정지 확인",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result == DialogResult.Yes)
        {
            // OHT 긴급 정지 로직 (라즈베리파이에 신호 전송)
            MessageBox.Show("OHT 긴급 정지 신호를 전송했습니다.", "정보",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private string GetCategoryDisplayName(string category)
    {
        return category switch
        {
            "NORMAL" => "정상",
            "COMPONENT_DEFECT" => "부품불량",
            "SOLDER_DEFECT" => "납땜불량",
            _ => category
        };
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            statusTimer?.Stop();
            statusTimer?.Dispose();
            httpClient?.Dispose();
        }
        base.Dispose(disposing);
    }
}

public class OHTStatus
{
    [JsonProperty("queue_length")]
    public int QueueLength { get; set; }

    [JsonProperty("current_request")]
    public OHTRequest CurrentRequest { get; set; }

    [JsonProperty("recent_requests")]
    public List<OHTRequestHistory> RecentRequests { get; set; }
}

public class OHTRequest
{
    [JsonProperty("request_id")]
    public string RequestId { get; set; }

    [JsonProperty("category")]
    public string Category { get; set; }

    [JsonProperty("is_auto")]
    public bool IsAuto { get; set; }
}

public class OHTRequestHistory
{
    [JsonProperty("request_id")]
    public string RequestId { get; set; }

    [JsonProperty("category")]
    public string Category { get; set; }

    [JsonProperty("is_auto")]
    public bool IsAuto { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; }

    [JsonProperty("timestamp")]
    public string Timestamp { get; set; }

    [JsonProperty("execution_time_seconds")]
    public double ExecutionTimeSeconds { get; set; }
}
```

**MainForm에 OHT 패널 추가:**

```csharp
// MainForm.cs

private OHTControlPanel ohtPanel;

private void InitializeMainForm()
{
    // ... 기존 컴포넌트 초기화 ...

    // OHT 제어 패널 추가 (박스 상태 패널 아래)
    ohtPanel = new OHTControlPanel("http://100.64.1.1:5000")
    {
        Location = new Point(20, 600)
    };
    this.Controls.Add(ohtPanel);

    // 권한 없으면 비활성화 (Viewer는 조회만 가능)
    if (!SessionManager.HasPermission(Permission.CallOHT))
    {
        // UI는 표시하되 버튼은 비활성화 (InitializePermissions에서 처리)
    }
}
```

---

### 3. InspectionHistoryForm (검사 이력)

#### 주요 기능
- 날짜 범위 필터
- 불량 유형 필터 (전체/정상/부품불량/납땜불량/폐기)
- 카메라 ID 필터 (전체/left/right)
- 페이징 (50건씩)
- Excel 내보내기 (Operator 이상)

#### DataGridView 커스텀 렌더링

불량 유형 열에 색상 동그라미 + 텍스트 표시:

```csharp
private void InitializeDataGridView()
{
    dgvInspections.CellPainting += DgvInspections_CellPainting;
}

private void DgvInspections_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
{
    if (e.RowIndex < 0 || dgvInspections.Columns[e.ColumnIndex].Name != "DefectType")
        return;

    e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);

    string defectType = e.Value?.ToString() ?? "";
    Color circleColor = DefectTypeHelper.GetDefectColor(defectType);

    // 동그라미 그리기
    Graphics g = e.Graphics;
    g.SmoothingMode = SmoothingMode.AntiAlias;

    int circleSize = 10;
    int circleX = e.CellBounds.X + 8;
    int circleY = e.CellBounds.Y + (e.CellBounds.Height - circleSize) / 2;

    using (SolidBrush brush = new SolidBrush(circleColor))
    {
        g.FillEllipse(brush, circleX, circleY, circleSize, circleSize);
    }

    // 텍스트 그리기
    string displayText = DefectTypeHelper.GetDefectDisplayName(defectType);
    int textX = circleX + circleSize + 6;
    Rectangle textRect = new Rectangle(textX, e.CellBounds.Y,
                                      e.CellBounds.Width - (textX - e.CellBounds.X),
                                      e.CellBounds.Height);

    TextRenderer.DrawText(g, displayText, e.CellStyle.Font, textRect,
                         e.CellStyle.ForeColor,
                         TextFormatFlags.VerticalCenter | TextFormatFlags.Left);

    e.Handled = true;
}
```

---

### 4. StatisticsForm (통계 화면)

#### 주요 기능
- 날짜 범위별 통계 조회
- 불량 유형별 파이 차트
- 시간대별 라인 차트
- 카메라별 통계 비교
- Excel 다중 시트 내보내기 (Operator 이상)

---

### 5. DefectImageViewerForm (불량 이미지 뷰어)

#### 주요 기능
- 이미지 확대/축소 (Zoom)
- 이미지 회전
- 검사 정보 표시 (ID, 시간, 불량 유형, 신뢰도)

---

### 6. UserManagementForm (사용자 관리) - Admin 전용

#### 주요 기능
- 사용자 생성/수정/삭제
- 권한 레벨 설정 (Admin/Operator/Viewer)
- 비밀번호 초기화 (기본: 'temp1234')
- 사용자 활동 로그 조회
- 사용자 검색 및 필터
- 활성화/비활성화 관리

#### 권한 제한
- Admin만 접근 가능
- MainForm에서 메뉴 클릭 시 권한 체크
- Operator/Viewer는 메뉴 비활성화

#### UI 구성 요소
- 검색 TextBox (사용자명/이름 검색)
- 권한 필터 ComboBox (전체/Admin/Operator/Viewer)
- DataGridView (사용자 목록 표시: ID, 사용자명, 이름, 권한, 상태, 마지막 로그인)
- 버튼: 사용자 추가, 수정, 삭제, 비밀번호 초기화, 활동 로그, 새로고침

#### 핵심 구현 코드

```csharp
public partial class UserManagementForm : Form
{
    private readonly HttpClient _httpClient;
    private readonly string _serverUrl;
    private DataGridView dgvUsers;
    private TextBox txtSearch;
    private ComboBox cmbRoleFilter;

    public UserManagementForm(string serverUrl, HttpClient httpClient)
    {
        InitializeComponent();
        _serverUrl = serverUrl;
        _httpClient = httpClient;

        // 권한 체크
        if (!SessionManager.HasPermission(Permission.ManageUsers))
        {
            MessageBox.Show("사용자 관리 권한이 없습니다.", "권한 없음",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            this.Close();
            return;
        }

        InitializeUI();  // UI 초기화 (상세 코드는 별도 제공)
        LoadUsers();
    }

    // 사용자 목록 조회
    private async void LoadUsers()
    {
        try
        {
            this.Cursor = Cursors.WaitCursor;
            var response = await _httpClient.GetAsync($"{_serverUrl}/api/users");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<UsersResponse>(json);

                dgvUsers.Rows.Clear();
                foreach (var user in result.Users)
                {
                    dgvUsers.Rows.Add(
                        user.Id,
                        user.Username,
                        user.FullName,
                        user.Role,
                        user.IsActive ? "활성" : "비활성",
                        user.LastLogin?.ToString("yyyy-MM-dd HH:mm") ?? "-"
                    );

                    // 비활성 사용자 표시 (회색)
                    if (!user.IsActive)
                    {
                        dgvUsers.Rows[dgvUsers.Rows.Count - 1].DefaultCellStyle.ForeColor = Color.Gray;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }

    // 검색 및 필터링
    private void FilterUsers()
    {
        string searchText = txtSearch.Text.ToLower();
        string roleFilter = cmbRoleFilter.SelectedItem?.ToString();

        foreach (DataGridViewRow row in dgvUsers.Rows)
        {
            bool matchSearch = string.IsNullOrEmpty(searchText) ||
                               row.Cells["Username"].Value.ToString().ToLower().Contains(searchText) ||
                               row.Cells["FullName"].Value.ToString().ToLower().Contains(searchText);

            bool matchRole = roleFilter == "전체" ||
                             row.Cells["Role"].Value.ToString() == roleFilter;

            row.Visible = matchSearch && matchRole;
        }
    }

    // 사용자 추가
    private void AddUser()
    {
        var dialog = new UserEditDialog(_serverUrl, _httpClient, null);
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            LoadUsers();
        }
    }

    // 사용자 수정
    private void EditUser()
    {
        if (dgvUsers.SelectedRows.Count == 0) return;

        var row = dgvUsers.SelectedRows[0];
        var user = new UserModel
        {
            Id = Convert.ToInt32(row.Cells["Id"].Value),
            Username = row.Cells["Username"].Value.ToString(),
            FullName = row.Cells["FullName"].Value.ToString(),
            Role = row.Cells["Role"].Value.ToString(),
            IsActive = row.Cells["IsActive"].Value.ToString() == "활성"
        };

        var dialog = new UserEditDialog(_serverUrl, _httpClient, user);
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            LoadUsers();
        }
    }

    // 사용자 삭제
    private async void DeleteUser()
    {
        if (dgvUsers.SelectedRows.Count == 0) return;

        var row = dgvUsers.SelectedRows[0];
        int userId = Convert.ToInt32(row.Cells["Id"].Value);
        string username = row.Cells["Username"].Value.ToString();

        // 자기 자신 삭제 방지
        if (userId == SessionManager.CurrentUser.Id)
        {
            MessageBox.Show("자기 자신은 삭제할 수 없습니다.", "삭제 불가",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var confirm = MessageBox.Show(
            $"사용자 '{username}'을(를) 삭제하시겠습니까?",
            "사용자 삭제 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Warning
        );

        if (confirm == DialogResult.Yes)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_serverUrl}/api/users/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("사용자가 삭제되었습니다.", "성공",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadUsers();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"오류 발생: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // 비밀번호 초기화
    private async void ResetPassword()
    {
        if (dgvUsers.SelectedRows.Count == 0) return;

        var row = dgvUsers.SelectedRows[0];
        int userId = Convert.ToInt32(row.Cells["Id"].Value);
        string username = row.Cells["Username"].Value.ToString();

        var confirm = MessageBox.Show(
            $"사용자 '{username}'의 비밀번호를 'temp1234'로 초기화하시겠습니까?",
            "비밀번호 초기화", MessageBoxButtons.YesNo, MessageBoxIcon.Question
        );

        if (confirm == DialogResult.Yes)
        {
            try
            {
                var response = await _httpClient.PostAsync(
                    $"{_serverUrl}/api/users/{userId}/reset-password", null
                );

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("비밀번호가 'temp1234'로 초기화되었습니다.", "성공",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"오류 발생: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // 활동 로그 조회
    private void ViewUserLogs()
    {
        if (dgvUsers.SelectedRows.Count == 0) return;

        var row = dgvUsers.SelectedRows[0];
        int userId = Convert.ToInt32(row.Cells["Id"].Value);
        string username = row.Cells["Username"].Value.ToString();

        var logsDialog = new UserLogsDialog(_serverUrl, _httpClient, userId, username);
        logsDialog.ShowDialog();
    }

    // 데이터 모델
    public class UserModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLogin { get; set; }
    }

    public class UsersResponse
    {
        public string Status { get; set; }
        public List<UserModel> Users { get; set; }
        public int Total { get; set; }
    }

    public class ErrorResponse
    {
        public string Error { get; set; }
        public string Message { get; set; }
    }
}
```

---

### 6-1. UserEditDialog (사용자 추가/수정 다이얼로그)

#### UI 구성 요소
- 사용자명 TextBox (영문, 숫자, _ 만 허용, 수정 시 ReadOnly)
- 비밀번호 TextBox (추가 시에만 표시 및 입력)
- 이름 TextBox
- 권한 ComboBox (Admin, Operator, Viewer)
- 활성화 CheckBox
- 버튼: 저장, 취소

#### 핵심 구현 코드

```csharp
public class UserEditDialog : Form
{
    private readonly HttpClient _httpClient;
    private readonly string _serverUrl;
    private readonly UserModel _existingUser;  // null이면 추가, 아니면 수정

    private TextBox txtUsername, txtPassword, txtFullName;
    private ComboBox cmbRole;
    private CheckBox chkIsActive;

    public UserEditDialog(string serverUrl, HttpClient httpClient, UserModel existingUser)
    {
        _serverUrl = serverUrl;
        _httpClient = httpClient;
        _existingUser = existingUser;

        InitializeComponent();
        InitializeUI();  // UI 초기화 (상세 코드는 별도 제공)

        if (_existingUser != null)
        {
            LoadExistingUser();
        }
    }

    // 기존 사용자 정보 로드
    private void LoadExistingUser()
    {
        txtUsername.Text = _existingUser.Username;
        txtUsername.ReadOnly = true;  // 수정 시 사용자명 변경 불가
        txtFullName.Text = _existingUser.FullName;
        cmbRole.SelectedItem = _existingUser.Role;
        chkIsActive.Checked = _existingUser.IsActive;
    }

    // 사용자 저장 (추가 또는 수정)
    private async Task SaveUser()
    {
        // 유효성 검사
        if (string.IsNullOrWhiteSpace(txtUsername.Text) ||
            (_existingUser == null && string.IsNullOrWhiteSpace(txtPassword.Text)) ||
            string.IsNullOrWhiteSpace(txtFullName.Text))
        {
            MessageBox.Show("필수 항목을 입력하세요.", "입력 오류",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            var userData = new
            {
                username = txtUsername.Text.Trim(),
                password = _existingUser == null ? txtPassword.Text : null,
                full_name = txtFullName.Text.Trim(),
                role = cmbRole.SelectedItem.ToString(),
                is_active = chkIsActive.Checked
            };

            var json = JsonConvert.SerializeObject(userData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            if (_existingUser == null)
            {
                response = await _httpClient.PostAsync($"{_serverUrl}/api/users", content);
            }
            else
            {
                response = await _httpClient.PutAsync(
                    $"{_serverUrl}/api/users/{_existingUser.Id}", content
                );
            }

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show(
                    _existingUser == null ? "사용자가 추가되었습니다." : "사용자 정보가 수정되었습니다.",
                    "성공", MessageBoxButtons.OK, MessageBoxIcon.Information
                );
                this.DialogResult = DialogResult.OK;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"오류 발생: {ex.Message}", "오류",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
```

---

### 6-2. UserLogsDialog (사용자 활동 로그 다이얼로그)

#### UI 구성 요소
- 날짜 범위 선택 (DateTimePicker 2개: 시작일, 종료일)
- 활동 유형 필터 ComboBox (전체/로그인/로그아웃/사용자 생성/수정/삭제/비밀번호 초기화/OHT 호출/데이터 내보내기)
- DataGridView (로그 목록 표시: 날짜/시간, 활동 유형, 상세 내용, IP 주소)
- 버튼: 조회, 닫기

#### 핵심 구현 코드

```csharp
public class UserLogsDialog : Form
{
    private readonly HttpClient _httpClient;
    private readonly string _serverUrl;
    private readonly int _userId;
    private readonly string _username;

    private DateTimePicker dtpStartDate, dtpEndDate;
    private ComboBox cmbActionType;
    private DataGridView dgvLogs;

    public UserLogsDialog(string serverUrl, HttpClient httpClient, int userId, string username)
    {
        _serverUrl = serverUrl;
        _httpClient = httpClient;
        _userId = userId;
        _username = username;

        InitializeComponent();
        InitializeUI();  // UI 초기화 (상세 코드는 별도 제공)
        LoadLogs();
    }

    // 활동 로그 조회
    private async void LoadLogs()
    {
        try
        {
            var startDate = dtpStartDate.Value.ToString("yyyy-MM-dd");
            var endDate = dtpEndDate.Value.ToString("yyyy-MM-dd");
            string url = $"{_serverUrl}/api/users/{_userId}/logs?start_date={startDate}&end_date={endDate}&limit=100";

            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<LogsResponse>(json);

                dgvLogs.Rows.Clear();
                foreach (var log in result.Logs)
                {
                    dgvLogs.Rows.Add(
                        log.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                        TranslateActionType(log.ActionType),
                        log.ActionDescription,
                        log.IpAddress ?? "-"
                    );
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"오류 발생: {ex.Message}", "오류",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private string TranslateActionType(string actionType)
    {
        return actionType switch
        {
            "login" => "로그인",
            "logout" => "로그아웃",
            "create_user" => "사용자 생성",
            "update_user" => "사용자 수정",
            "delete_user" => "사용자 삭제",
            "reset_password" => "비밀번호 초기화",
            "call_oht" => "OHT 호출",
            "export_data" => "데이터 내보내기",
            "view_inspection" => "검사 조회",
            "change_settings" => "설정 변경",
            _ => actionType
        };
    }

    public class LogModel
    {
        public int Id { get; set; }
        public string ActionType { get; set; }
        public string ActionDescription { get; set; }
        public string IpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class LogsResponse
    {
        public string Status { get; set; }
        public List<LogModel> Logs { get; set; }
        public int Total { get; set; }
    }
}
```

---

### 7. SettingsForm (시스템 설정) - Admin 전용

#### 주요 기능
- Flask 서버 URL 설정
- MySQL 연결 정보 설정
- 알림 임계값 설정 (불량률 알림)
- 세션 타임아웃 설정
- 로그 레벨 설정

---

## 커스텀 UI 컨트롤

### StatusIndicator (상태 동그라미)

```csharp
public class StatusIndicator : Control
{
    public bool IsOnline { get; set; }
    public Color IndicatorColor { get; set; }
    public int CircleSize { get; set; } = 12;

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        int x = (this.Width - CircleSize) / 2;
        int y = (this.Height - CircleSize) / 2;

        using (SolidBrush brush = new SolidBrush(IndicatorColor))
        {
            g.FillEllipse(brush, x, y, CircleSize, CircleSize);
        }
    }
}
```

**사용 예:**
```csharp
statusIndicator.IsOnline = status.ServerOnline;
lblServerStatus.Text = status.ServerOnline ? "서버: 온라인" : "서버: 오프라인";
```

---

### LegendItem (차트 범례)

```csharp
public class LegendItem : Control
{
    public Color CircleColor { get; set; }
    public string LegendText { get; set; }
    public int CircleSize { get; set; } = 10;

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // 동그라미
        int circleY = (this.Height - CircleSize) / 2;
        using (SolidBrush brush = new SolidBrush(CircleColor))
        {
            g.FillEllipse(brush, 0, circleY, CircleSize, CircleSize);
        }

        // 텍스트
        int textX = CircleSize + 6;
        using (SolidBrush textBrush = new SolidBrush(this.ForeColor))
        {
            g.DrawString(LegendText, this.Font, textBrush, textX, 0);
        }
    }
}
```

---

### DefectTypeHelper (색상 매핑)

```csharp
public static class DefectTypeHelper
{
    public static Color GetDefectColor(string defectType)
    {
        return defectType switch
        {
            "정상" => Color.FromArgb(76, 175, 80),      // 초록
            "부품불량" => Color.FromArgb(255, 152, 0),  // 주황
            "납땜불량" => Color.FromArgb(255, 235, 59), // 노랑
            "폐기" => Color.FromArgb(244, 67, 54),      // 빨강
            _ => Color.Gray
        };
    }

    public static string GetDefectDisplayName(string defectType)
    {
        return defectType switch
        {
            "정상" => "정상",
            "부품불량" => "부품 불량",
            "납땜불량" => "납땜 불량",
            "폐기" => "폐기",
            _ => "알 수 없음"
        };
    }
}
```

---

## Excel 내보내기 기능

### ExcelExportService 클래스

```csharp
public class ExcelExportService
{
    public void ExportInspectionHistory(List<Inspection> inspections, string filePath)
    {
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("검사 이력");

            // 헤더
            worksheet.Cell(1, 1).Value = "검사 ID";
            worksheet.Cell(1, 2).Value = "카메라 ID";
            worksheet.Cell(1, 3).Value = "불량 유형";
            worksheet.Cell(1, 4).Value = "신뢰도";
            worksheet.Cell(1, 5).Value = "검사 시간";

            // 데이터 및 조건부 서식
            int row = 2;
            foreach (var inspection in inspections)
            {
                worksheet.Cell(row, 1).Value = inspection.Id;
                worksheet.Cell(row, 2).Value = inspection.CameraId;
                worksheet.Cell(row, 3).Value = inspection.DefectType;
                worksheet.Cell(row, 4).Value = inspection.Confidence;
                worksheet.Cell(row, 5).Value = inspection.InspectionTime;

                // 불량 유형별 조건부 서식
                var rowRange = worksheet.Range(row, 1, row, 5);
                switch (inspection.DefectType)
                {
                    case "정상":
                        rowRange.Style.Fill.BackgroundColor = XLColor.LightGreen;
                        break;
                    case "부품불량":
                        rowRange.Style.Fill.BackgroundColor = XLColor.Orange;
                        break;
                    case "납땜불량":
                        rowRange.Style.Fill.BackgroundColor = XLColor.Yellow;
                        break;
                    case "폐기":
                        rowRange.Style.Fill.BackgroundColor = XLColor.Red;
                        break;
                }
                row++;
            }

            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(filePath);
        }
    }

    public void ExportStatistics(Statistics stats, string filePath)
    {
        using (var workbook = new XLWorkbook())
        {
            // 시트 1: 요약 통계
            var summarySheet = workbook.Worksheets.Add("요약");
            summarySheet.Cell(1, 1).Value = "총 검사";
            summarySheet.Cell(1, 2).Value = stats.TotalInspections;
            summarySheet.Cell(2, 1).Value = "정상";
            summarySheet.Cell(2, 2).Value = stats.NormalCount;
            summarySheet.Cell(3, 1).Value = "부품불량";
            summarySheet.Cell(3, 2).Value = stats.ComponentDefectCount;
            summarySheet.Cell(4, 1).Value = "납땜불량";
            summarySheet.Cell(4, 2).Value = stats.SolderDefectCount;
            summarySheet.Cell(5, 1).Value = "폐기";
            summarySheet.Cell(5, 2).Value = stats.DiscardCount;
            summarySheet.Cell(6, 1).Value = "불량률";
            summarySheet.Cell(6, 2).Value = $"{stats.DefectRate:F2}%";

            // 시트 2: 시간대별 데이터
            var hourlySheet = workbook.Worksheets.Add("시간대별");
            hourlySheet.Cell(1, 1).Value = "시간";
            hourlySheet.Cell(1, 2).Value = "검사 건수";

            int row = 2;
            foreach (var item in stats.HourlyInspections)
            {
                hourlySheet.Cell(row, 1).Value = item.Key;
                hourlySheet.Cell(row, 2).Value = item.Value;
                row++;
            }

            workbook.SaveAs(filePath);
        }
    }
}
```

---

## UI 디자인 가이드

### 컬러 팔레트

| 용도 | RGB | C# 코드 |
|------|-----|---------|
| 정상 | (76, 175, 80) | `Color.FromArgb(76, 175, 80)` |
| 부품 불량 | (255, 152, 0) | `Color.FromArgb(255, 152, 0)` |
| 납땜 불량 | (255, 235, 59) | `Color.FromArgb(255, 235, 59)` |
| 폐기 | (244, 67, 54) | `Color.FromArgb(244, 67, 54)` |
| 온라인 | (76, 175, 80) | `Color.FromArgb(76, 175, 80)` |
| 오프라인 | (244, 67, 54) | `Color.FromArgb(244, 67, 54)` |

### 폰트

- **제목**: Segoe UI, 14pt, Bold
- **본문**: Segoe UI, 10pt, Regular
- **데이터 (표)**: Consolas, 9pt
- **버튼**: Segoe UI, 10pt, Bold

### UI 컨트롤 사용 권장사항

- **이모지 대신 커스텀 컨트롤 사용** - 폰트 의존성 제거
- **StatusIndicator**: 상태 표시용 동그라미
- **LegendItem**: 범례용 동그라미 + 텍스트
- **DataGridView CellPainting**: 불량 유형 셀 커스텀 렌더링

---

## 기술 스택

### 프레임워크
- **.NET 6+** (Windows Forms)
- **C# 10+**
- **Visual Studio 2022**

### NuGet 패키지

```xml
<PackageReference Include="MySql.Data" Version="8.0.33" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
<PackageReference Include="LiveCharts.WinForms" Version="0.9.7" />
<PackageReference Include="ClosedXML" Version="0.102.1" />
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
```

### 디자인 패턴
- **Repository 패턴**: DatabaseService, ApiService
- **Singleton 패턴**: SessionManager
- **Factory 패턴**: Form 생성

---

## 보안 및 세션 관리

### SessionManager (핵심 메서드)

```csharp
public static class SessionManager
{
    public static User CurrentUser { get; set; }
    public static DateTime LastActivity { get; set; }
    private static readonly int SessionTimeoutMinutes = 30;

    public static bool IsSessionExpired()
    {
        if (CurrentUser == null) return true;
        TimeSpan inactiveTime = DateTime.Now - LastActivity;
        return inactiveTime.TotalMinutes > SessionTimeoutMinutes;
    }

    public static bool HasPermission(Permission permission)
    {
        if (CurrentUser == null) return false;

        switch (permission)
        {
            case Permission.ExportData:
            case Permission.CallOHT:  // ⭐ 신규 추가
                return CurrentUser.Role == UserRole.Admin ||
                       CurrentUser.Role == UserRole.Operator;

            case Permission.ManageUsers:
            case Permission.ChangeSettings:
                return CurrentUser.Role == UserRole.Admin;

            case Permission.ViewData:
                return true;

            default:
                return false;
        }
    }

    public static void Logout()
    {
        CurrentUser = null;
        LastActivity = DateTime.MinValue;
    }
}
```

### 비밀번호 해싱 (BCrypt)

```csharp
// 회원가입 시
string hashedPassword = BCrypt.Net.BCrypt.HashPassword(plainPassword);

// 로그인 시
bool isValid = BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);
```

---

## 추가 기능

### 알림 시스템

- 불량률이 임계값 초과 시 알림
- 시스템 오프라인 시 알림
- Toast 알림 또는 MessageBox 표시

```csharp
if (stats.DefectRate > AlarmThreshold)
{
    NotificationManager.ShowWarning($"불량률 경고: {stats.DefectRate:F1}%");
}
```

### 로깅

```csharp
public static class Logger
{
    private static readonly string LogPath = "logs/app.log";

    public static void LogError(string message, Exception ex)
    {
        string logMessage = $"[ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}\n{ex}\n";
        File.AppendAllText(LogPath, logMessage);
    }

    public static void LogInfo(string message)
    {
        string logMessage = $"[INFO] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}\n";
        File.AppendAllText(LogPath, logMessage);
    }
}
```

---

## 개발 가이드라인

### 코드 컨벤션
- **네이밍**: PascalCase (클래스, 메서드), camelCase (변수)
- **비동기 메서드**: Async 접미사 사용
- **예외 처리**: try-catch로 감싸고 Logger에 기록

### 권장 사항
1. **권한 체크**: 모든 민감한 작업 전에 `SessionManager.HasPermission()` 호출
2. **세션 검증**: 각 Form Load 시 `SessionManager.IsSessionExpired()` 체크
3. **리소스 해제**: using 문 또는 Dispose 패턴 사용
4. **비동기 작업**: UI 블로킹 방지를 위해 async/await 사용

---

## 구현 우선순위

### Phase 1 (핵심 기능)
1. LoginForm + SessionManager
2. MainForm (기본 대시보드)
3. InspectionHistoryForm (조회만)

### Phase 2 (데이터 내보내기)
4. ExcelExportService
5. StatisticsForm

### Phase 3 (관리 기능)
6. UserManagementForm (Admin)
7. SettingsForm (Admin)

### Phase 4 (추가 기능)
8. DefectImageViewerForm
9. 알림 시스템
10. 다크 모드 (선택)

---

**문서 끝**
