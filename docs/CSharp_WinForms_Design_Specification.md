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
            // API 호출: /api/v1/box_status
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
    // 로컬: http://192.168.0.10:5000/api/v1 (선택)
    // 원격: http://100.x.x.x:5000/api/v1 (Tailscale VPN - 프로젝트 환경)
    private const string BASE_URL = "http://192.168.0.10:5000/api/v1";  // Flask 서버

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
- 권한 레벨 설정
- 비밀번호 초기화
- 사용자 활동 로그 조회

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
