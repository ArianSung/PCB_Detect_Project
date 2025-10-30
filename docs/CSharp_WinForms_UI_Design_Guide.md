# C# WinForms UI 디자인 가이드

> **작성일**: 2025-10-30
> **프로젝트**: PCB 불량 검사 시스템
> **목적**: C# WinForms 모니터링 애플리케이션의 UI 디자인 구현 가이드

---

## 목차
1. [폼 구조 개요](#1-폼-구조-개요)
2. [로그인 폼 (LoginForm)](#2-로그인-폼-loginform)
3. [메인 대시보드 (MainForm)](#3-메인-대시보드-mainform)
4. [검사 이력 조회 (InspectionHistoryForm)](#4-검사-이력-조회-inspectionhistoryform)
5. [불량 이미지 뷰어 (DefectImageViewerForm)](#5-불량-이미지-뷰어-defectimageviewerform)
6. [통계 화면 (StatisticsForm)](#6-통계-화면-statisticsform)
7. [모니터링 화면 (MonitoringForm)](#7-모니터링-화면-monitoringform)
8. [사용자 관리 (UserManagementForm)](#8-사용자-관리-usermanagementform)
9. [시스템 설정 (SettingsForm)](#9-시스템-설정-settingsform)
10. [UI 컴포넌트 공통 가이드](#10-ui-컴포넌트-공통-가이드)

---

## 1. 폼 구조 개요

### Forms 디렉토리 구조
```
Forms/
├── LoginForm.cs                   # 로그인 폼
├── MainForm.cs                    # 메인 대시보드 (통합 폼)
├── InspectionHistoryForm.cs       # 검사 이력 조회 (MainForm 내부 패널)
├── DefectImageViewerForm.cs       # 불량 이미지 뷰어 (독립 창)
├── StatisticsForm.cs              # 통계 화면 (MainForm 내부 패널)
├── MonitoringForm.cs              # 모니터링 화면 (MainForm 내부 패널)
├── UserManagementForm.cs          # 사용자 관리 (MainForm 내부 패널)
└── SettingsForm.cs                # 시스템 설정 (MainForm 내부 패널)
```

### 화면 전환 방식
- **MainForm**이 메인 컨테이너 역할
- 좌측 사이드바 버튼 클릭 시 우측 패널 내용 교체 (Panel Switching)
- 독립 창: DefectImageViewerForm만 별도 Form으로 띄움

---

## 2. 로그인 폼 (LoginForm)

### 2.1 디자인 개요
**레퍼런스 이미지**: 페이지 3 참조 (그라데이션 배경 디자인)

### 2.2 레이아웃 구성

#### 배경 스타일
```csharp
// 그라데이션 배경 (보라색 → 하늘색)
BackColor = Color.Transparent;
BackgroundImage = CreateGradientBackground(
    Color.FromArgb(200, 150, 255),  // 보라색 (상단)
    Color.FromArgb(150, 220, 255)   // 하늘색 (하단)
);
```

#### 중앙 로그인 패널
```
┌────────────────────────────────┐
│                                │
│        👤 (사용자 아이콘)        │
│        User Login              │
│                                │
│  📧 ____________________       │
│     로그인 ID                   │
│                                │
│  🔒 ____________________       │
│     Password                   │
│                                │
│    ┌─────────────────┐         │
│    │     LOGIN       │         │
│    └─────────────────┘         │
│                                │
└────────────────────────────────┘
```

### 2.3 UI 컴포넌트

#### 로그인 ID TextBox
```csharp
var txtLoginId = new TextBox
{
    Location = new Point(100, 150),
    Size = new Size(350, 40),
    Font = new Font("Segoe UI", 12F),
    PlaceholderText = "로그인 ID",
    BorderStyle = BorderStyle.FixedSingle
};
```

#### Password TextBox
```csharp
var txtPassword = new TextBox
{
    Location = new Point(100, 210),
    Size = new Size(350, 40),
    Font = new Font("Segoe UI", 12F),
    PlaceholderText = "Password",
    UseSystemPasswordChar = true,
    BorderStyle = BorderStyle.FixedSingle
};
```

#### Login Button
```csharp
var btnLogin = new Button
{
    Location = new Point(150, 280),
    Size = new Size(250, 50),
    Text = "LOGIN",
    Font = new Font("Segoe UI", 14F, FontStyle.Bold),
    BackColor = Color.FromArgb(50, 100, 200),
    ForeColor = Color.White,
    FlatStyle = FlatStyle.Flat,
    Cursor = Cursors.Hand
};
btnLogin.FlatAppearance.BorderSize = 0;
```

### 2.4 보안 기능

#### 로그인 실패 제한
```csharp
private int loginFailCount = 0;
private DateTime lockoutTime = DateTime.MinValue;

private void btnLogin_Click(object sender, EventArgs e)
{
    // 잠금 확인
    if (DateTime.Now < lockoutTime)
    {
        int remainingSeconds = (int)(lockoutTime - DateTime.Now).TotalSeconds;
        MessageBox.Show(
            $"로그인이 잠겨있습니다.\n남은 시간: {remainingSeconds}초",
            "로그인 잠금",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning
        );
        return;
    }

    // 로그인 시도
    if (AuthenticateUser(txtLoginId.Text, txtPassword.Text))
    {
        loginFailCount = 0;
        this.Hide();
        new MainForm(currentUser).Show();
    }
    else
    {
        loginFailCount++;

        if (loginFailCount >= 5)
        {
            lockoutTime = DateTime.Now.AddMinutes(5);
            MessageBox.Show(
                "로그인 5회 실패하였습니다.\n5분간 로그인이 제한됩니다.",
                "로그인 제한",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
        else
        {
            MessageBox.Show(
                $"로그인 실패 ({loginFailCount}/5)",
                "로그인 실패",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }
    }
}
```

### 2.5 디자인 변경 사항 (레퍼런스 대비)
| 레퍼런스 이미지 | 실제 구현 |
|---|---|
| Email ID | **로그인 ID** |
| Remember me 체크박스 | **없음** (제거) |
| Forgot Password 링크 | **없음** (관리자 페이지에서 비밀번호 초기화) |

---

## 3. 메인 대시보드 (MainForm)

### 3.1 디자인 개요
**레퍼런스 이미지**: 페이지 4-5 참조 (AURA 스마트 팜 시스템 디자인)

### 3.2 전체 레이아웃
```
┌─────────────────────────────────────────────────────────────┐
│ 로그인 사용자: OOO (권한)                  [로그아웃 버튼]   │
├──────────┬──────────────────────────────────────────────────┤
│          │                                                  │
│  대시보드  │                                                  │
│          │                                                  │
│  검사이력  │          [동적 콘텐츠 영역]                        │
│          │                                                  │
│  통계화면  │       (대시보드/검사이력/통계/모니터링            │
│          │        /사용자관리/시스템설정)                     │
│  모니터링  │                                                  │
│          │                                                  │
│ 사용자관리 │                                                  │
│ (Admin)  │                                                  │
│          │                                                  │
│ 시스템설정 │                                                  │
│ (Admin)  │                                                  │
└──────────┴──────────────────────────────────────────────────┘
```

### 3.3 상단 헤더 패널

#### 사용자 정보 표시
```csharp
// 상단 헤더 (오렌지색 배경)
var pnlHeader = new Panel
{
    Dock = DockStyle.Top,
    Height = 60,
    BackColor = Color.FromArgb(255, 120, 50)  // 오렌지색
};

var lblUserInfo = new Label
{
    Text = $"로그인 사용자: {currentUser.Name} ({currentUser.Role})",
    Font = new Font("Segoe UI", 11F),
    ForeColor = Color.White,
    Location = new Point(20, 20),
    AutoSize = true
};

var btnLogout = new Button
{
    Text = "로그아웃",
    Size = new Size(100, 35),
    Location = new Point(pnlHeader.Width - 120, 12),
    BackColor = Color.FromArgb(200, 80, 30),
    ForeColor = Color.White,
    FlatStyle = FlatStyle.Flat
};
```

### 3.4 좌측 사이드바 (네비게이션 메뉴)

#### 사이드바 스타일
```csharp
// 좌측 사이드바 (어두운 네이비)
var pnlSidebar = new Panel
{
    Dock = DockStyle.Left,
    Width = 200,
    BackColor = Color.FromArgb(45, 50, 80)  // 네이비
};
```

#### 메뉴 버튼 구성
```csharp
private void CreateMenuButton(string text, int index, bool requiresAdmin = false)
{
    var btn = new Button
    {
        Text = text,
        Size = new Size(200, 50),
        Location = new Point(0, index * 50),
        BackColor = Color.FromArgb(45, 50, 80),
        ForeColor = Color.White,
        FlatStyle = FlatStyle.Flat,
        TextAlign = ContentAlignment.MiddleLeft,
        Padding = new Padding(20, 0, 0, 0),
        Font = new Font("Segoe UI", 11F),
        Cursor = Cursors.Hand
    };

    // 권한 체크
    if (requiresAdmin && currentUser.Role != "Admin")
    {
        btn.Enabled = false;
        btn.ForeColor = Color.Gray;
    }

    // 호버 효과
    btn.MouseEnter += (s, e) => {
        if (btn.Enabled)
            btn.BackColor = Color.FromArgb(60, 70, 110);
    };
    btn.MouseLeave += (s, e) => {
        if (btn.Enabled && btn.BackColor != Color.FromArgb(80, 90, 130))
            btn.BackColor = Color.FromArgb(45, 50, 80);
    };

    // 선택 효과
    btn.Click += (s, e) => {
        foreach (Control c in pnlSidebar.Controls)
        {
            if (c is Button)
                c.BackColor = Color.FromArgb(45, 50, 80);
        }
        btn.BackColor = Color.FromArgb(80, 90, 130);

        SwitchPanel(text);
    };

    pnlSidebar.Controls.Add(btn);
}
```

#### 메뉴 항목 생성
```csharp
CreateMenuButton("대시보드", 0);
CreateMenuButton("검사 이력 조회", 1);
CreateMenuButton("통계 화면", 2);
CreateMenuButton("모니터링", 3);
CreateMenuButton("사용자 관리", 4, requiresAdmin: true);  // Admin 전용
CreateMenuButton("시스템 설정", 5, requiresAdmin: true);  // Admin 전용
```

### 3.5 대시보드 메인 화면

#### 오늘 통계 카드
```
┌──────────────────────────────────────────────────────────┐
│  오늘 검사 통계 (1초마다 자동 새로고침)                       │
├──────────────────────────────────────────────────────────┤
│  총 검사 건수: 1,234건                                     │
│  정상: 1,100건 (89.1%)                                    │
│  불량: 134건 (10.9%)                                      │
│    - 앞면 불량: 65건                                       │
│    - 뒷면 불량: 69건                                       │
└──────────────────────────────────────────────────────────┘
```

#### 파이 차트 (불량률)
```csharp
// LiveCharts 라이브러리 사용
var pieChart = new LiveCharts.WinForms.PieChart
{
    Location = new Point(450, 150),
    Size = new Size(400, 400),
    LegendLocation = LegendLocation.Right
};

// 데이터 구성
pieChart.Series = new SeriesCollection
{
    new PieSeries
    {
        Title = "정상",
        Values = new ChartValues<double> { normalCount },
        Fill = new SolidColorBrush(Color.FromArgb(100, 150, 255))  // 파란색
    },
    new PieSeries
    {
        Title = "앞면 불량",
        Values = new ChartValues<double> { frontDefectCount },
        Fill = new SolidColorBrush(Color.FromArgb(150, 150, 150))  // 회색
    },
    new PieSeries
    {
        Title = "뒷면 불량",
        Values = new ChartValues<double> { backDefectCount },
        Fill = new SolidColorBrush(Color.FromArgb(255, 150, 100))  // 오렌지
    },
    new PieSeries
    {
        Title = "불량",
        Values = new ChartValues<double> { otherDefectCount },
        Fill = new SolidColorBrush(Color.FromArgb(255, 220, 100))  // 노란색
    }
};
```

#### 시스템 상태 표시
```csharp
var lblSystemStatus = new Label
{
    Text = "시스템 상태: 정상 운영 중 ✅",
    Font = new Font("Segoe UI", 12F),
    ForeColor = Color.Green,
    Location = new Point(50, 600),
    AutoSize = true
};

var lblFlaskServer = new Label
{
    Text = $"Flask 서버: {flaskServerUrl} (연결됨)",
    Font = new Font("Segoe UI", 10F),
    Location = new Point(50, 630),
    AutoSize = true
};

var lblDatabase = new Label
{
    Text = "MySQL 데이터베이스: 연결됨",
    Font = new Font("Segoe UI", 10F),
    Location = new Point(50, 655),
    AutoSize = true
};
```

#### OHT 상태 표시
```csharp
var lblOhtStatus = new Label
{
    Text = "OHT 제어: 정상",
    Font = new Font("Segoe UI", 10F),
    Location = new Point(50, 680),
    AutoSize = true
};
```

#### 자동 새로고침
```csharp
private Timer refreshTimer;

private void InitializeDashboard()
{
    refreshTimer = new Timer();
    refreshTimer.Interval = 1000;  // 1초
    refreshTimer.Tick += RefreshDashboard;
    refreshTimer.Start();
}

private async void RefreshDashboard(object sender, EventArgs e)
{
    try
    {
        var stats = await apiClient.GetTodayStatistics();

        lblTotalCount.Text = $"총 검사 건수: {stats.TotalCount:N0}건";
        lblNormalCount.Text = $"정상: {stats.NormalCount:N0}건 ({stats.NormalRate:F1}%)";
        lblDefectCount.Text = $"불량: {stats.DefectCount:N0}건 ({stats.DefectRate:F1}%)";
        lblFrontDefect.Text = $"  - 앞면 불량: {stats.FrontDefectCount:N0}건";
        lblBackDefect.Text = $"  - 뒷면 불량: {stats.BackDefectCount:N0}건";

        UpdatePieChart(stats);
        UpdateSystemStatus();
    }
    catch (Exception ex)
    {
        lblSystemStatus.Text = "시스템 상태: 오류 ⚠️";
        lblSystemStatus.ForeColor = Color.Red;
        LogError($"대시보드 새로고침 오류: {ex.Message}");
    }
}
```

---

## 4. 검사 이력 조회 (InspectionHistoryForm)

### 4.1 디자인 개요
**레퍼런스 이미지**: 페이지 6 참조

### 4.2 레이아웃 구성
```
┌──────────────────────────────────────────────────────────┐
│  검사 이력 조회                                             │
├──────────────────────────────────────────────────────────┤
│  [ 검사 이력 DataGridView ]                                │
│  ┌────────────────────────────────────────────────────┐  │
│  │ ID │ 카메라 │ 불량유형 │ 신뢰도 │ 검사시간 │ 이미지 │  │
│  ├────┼────────┼──────────┼────────┼──────────┼────────┤  │
│  │ 1  │ 좌측   │ 납땜불량 │ 95.3%  │ 10:23:45 │ [보기] │  │
│  │ 2  │ 우측   │ 부품불량 │ 88.7%  │ 10:23:50 │ [보기] │  │
│  │ 3  │ 좌측   │ 정상     │ -      │ 10:23:55 │ [보기] │  │
│  └────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────┘
```

### 4.3 DataGridView 구성

#### DataGridView 초기화
```csharp
var dgvInspectionHistory = new DataGridView
{
    Location = new Point(20, 80),
    Size = new Size(900, 500),
    AutoGenerateColumns = false,
    AllowUserToAddRows = false,
    AllowUserToDeleteRows = false,
    ReadOnly = true,
    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
    MultiSelect = false,
    BackgroundColor = Color.White,
    BorderStyle = BorderStyle.Fixed3D,
    RowHeadersVisible = false,
    Font = new Font("Segoe UI", 10F)
};
```

#### 컬럼 정의
```csharp
// ID 컬럼
dgvInspectionHistory.Columns.Add(new DataGridViewTextBoxColumn
{
    Name = "colId",
    HeaderText = "ID",
    DataPropertyName = "InspectionId",
    Width = 80,
    DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
});

// 카메라 컬럼
dgvInspectionHistory.Columns.Add(new DataGridViewTextBoxColumn
{
    Name = "colCamera",
    HeaderText = "카메라",
    DataPropertyName = "CameraPosition",
    Width = 100,
    DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
});

// 불량 유형 컬럼
dgvInspectionHistory.Columns.Add(new DataGridViewTextBoxColumn
{
    Name = "colDefectType",
    HeaderText = "불량 유형",
    DataPropertyName = "DefectType",
    Width = 150
});

// 신뢰도 컬럼
dgvInspectionHistory.Columns.Add(new DataGridViewTextBoxColumn
{
    Name = "colConfidence",
    HeaderText = "신뢰도",
    DataPropertyName = "Confidence",
    Width = 100,
    DefaultCellStyle = new DataGridViewCellStyle
    {
        Alignment = DataGridViewContentAlignment.MiddleCenter,
        Format = "P1"  // 퍼센트 1자리
    }
});

// 검사 시간 컬럼
dgvInspectionHistory.Columns.Add(new DataGridViewTextBoxColumn
{
    Name = "colInspectionTime",
    HeaderText = "검사 시간",
    DataPropertyName = "InspectionTime",
    Width = 180,
    DefaultCellStyle = new DataGridViewCellStyle
    {
        Format = "yyyy-MM-dd HH:mm:ss"
    }
});

// 불량 이미지 뷰어 버튼 컬럼
var btnColumn = new DataGridViewButtonColumn
{
    Name = "colViewImage",
    HeaderText = "이미지",
    Text = "보기",
    UseColumnTextForButtonValue = true,
    Width = 80
};
dgvInspectionHistory.Columns.Add(btnColumn);
```

#### 이미지 뷰어 버튼 클릭 이벤트
```csharp
dgvInspectionHistory.CellContentClick += (s, e) =>
{
    if (e.ColumnIndex == dgvInspectionHistory.Columns["colViewImage"].Index && e.RowIndex >= 0)
    {
        var inspectionId = (int)dgvInspectionHistory.Rows[e.RowIndex].Cells["colId"].Value;

        var imageViewerForm = new DefectImageViewerForm(inspectionId);
        imageViewerForm.ShowDialog();
    }
};
```

#### 행 색상 지정 (불량 유형별)
```csharp
dgvInspectionHistory.CellFormatting += (s, e) =>
{
    if (e.RowIndex < 0) return;

    var defectType = dgvInspectionHistory.Rows[e.RowIndex].Cells["colDefectType"].Value?.ToString();

    Color rowColor = Color.White;
    switch (defectType)
    {
        case "정상":
            rowColor = Color.FromArgb(230, 255, 230);  // 연한 초록
            break;
        case "부품 불량":
            rowColor = Color.FromArgb(255, 240, 230);  // 연한 오렌지
            break;
        case "납땜 불량":
            rowColor = Color.FromArgb(255, 245, 230);  // 연한 노랑
            break;
        case "폐기":
            rowColor = Color.FromArgb(255, 230, 230);  // 연한 빨강
            break;
    }

    dgvInspectionHistory.Rows[e.RowIndex].DefaultCellStyle.BackColor = rowColor;
};
```

---

## 5. 불량 이미지 뷰어 (DefectImageViewerForm)

### 5.1 디자인 개요
독립 Form으로 표시되는 이미지 뷰어

### 5.2 레이아웃
```
┌──────────────────────────────────────┐
│  불량 이미지 뷰어 - ID: 1234           │
├──────────────────────────────────────┤
│                                      │
│                                      │
│      [PCB 불량 이미지 표시]            │
│      (YOLO 바운딩 박스 포함)          │
│                                      │
│                                      │
├──────────────────────────────────────┤
│  카메라: 좌측                          │
│  불량 유형: 납땜 불량                  │
│  신뢰도: 95.3%                        │
│  검사 시간: 2025-10-30 10:23:45      │
│                                      │
│            [닫기]                     │
└──────────────────────────────────────┘
```

### 5.3 PictureBox 구성
```csharp
var pbDefectImage = new PictureBox
{
    Location = new Point(20, 60),
    Size = new Size(760, 500),
    SizeMode = PictureBoxSizeMode.Zoom,  // 비율 유지하며 확대/축소
    BorderStyle = BorderStyle.FixedSingle
};
```

### 5.4 이미지 로드
```csharp
private async void LoadDefectImage(int inspectionId)
{
    try
    {
        // REST API에서 이미지 가져오기
        var imageData = await apiClient.GetDefectImage(inspectionId);

        using (var ms = new MemoryStream(imageData.ImageBytes))
        {
            pbDefectImage.Image = Image.FromStream(ms);
        }

        // 메타데이터 표시
        lblCamera.Text = $"카메라: {imageData.CameraPosition}";
        lblDefectType.Text = $"불량 유형: {imageData.DefectType}";
        lblConfidence.Text = $"신뢰도: {imageData.Confidence:P1}";
        lblInspectionTime.Text = $"검사 시간: {imageData.InspectionTime:yyyy-MM-dd HH:mm:ss}";
    }
    catch (Exception ex)
    {
        MessageBox.Show(
            $"이미지를 불러오는 중 오류가 발생했습니다.\n{ex.Message}",
            "오류",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error
        );
    }
}
```

---

## 6. 통계 화면 (StatisticsForm)

### 6.1 디자인 개요
**레퍼런스 이미지**: 페이지 7 참조

### 6.2 레이아웃 구성
```
┌──────────────────────────────────────────────────────────┐
│  통계 화면                            [Excel 내보내기]     │
├──────────────────────────────────────────────────────────┤
│  날짜 범위: [____] ~ [____]  [조회]                        │
├──────────────────────────────────────────────────────────┤
│  [ 불량 유형별 파이 차트 ]     [ 시간대별 라인 차트 ]      │
│  ┌──────────────────┐         ┌──────────────────┐      │
│  │                  │         │                  │      │
│  │   Pie Chart      │         │   Line Chart     │      │
│  │                  │         │                  │      │
│  └──────────────────┘         └──────────────────┘      │
├──────────────────────────────────────────────────────────┤
│  [ 카메라별 통계 비교 ]                                    │
│  ┌──────────────────────────────────────────────────┐  │
│  │             Bar Chart (카메라별)                   │  │
│  └──────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────┘
```

### 6.3 날짜 범위 선택
```csharp
var dtpStartDate = new DateTimePicker
{
    Location = new Point(80, 20),
    Size = new Size(150, 25),
    Format = DateTimePickerFormat.Short,
    Value = DateTime.Now.AddDays(-7)  // 기본값: 1주일 전
};

var dtpEndDate = new DateTimePicker
{
    Location = new Point(260, 20),
    Size = new Size(150, 25),
    Format = DateTimePickerFormat.Short,
    Value = DateTime.Now
};

var btnSearch = new Button
{
    Text = "조회",
    Location = new Point(430, 18),
    Size = new Size(80, 30),
    BackColor = Color.FromArgb(50, 100, 200),
    ForeColor = Color.White,
    FlatStyle = FlatStyle.Flat
};
```

### 6.4 불량 유형별 파이 차트
```csharp
var pieChartDefectType = new LiveCharts.WinForms.PieChart
{
    Location = new Point(30, 80),
    Size = new Size(400, 300),
    LegendLocation = LegendLocation.Bottom
};

pieChartDefectType.Series = new SeriesCollection
{
    new PieSeries { Title = "정상", Values = new ChartValues<double> { normalCount } },
    new PieSeries { Title = "부품 불량", Values = new ChartValues<double> { componentDefectCount } },
    new PieSeries { Title = "납땜 불량", Values = new ChartValues<double> { solderDefectCount } },
    new PieSeries { Title = "폐기", Values = new ChartValues<double> { discardCount } }
};
```

### 6.5 시간대별 라인 차트
```csharp
var lineChartTimeSeries = new LiveCharts.WinForms.CartesianChart
{
    Location = new Point(470, 80),
    Size = new Size(450, 300),
    LegendLocation = LegendLocation.Top
};

lineChartTimeSeries.Series = new SeriesCollection
{
    new LineSeries
    {
        Title = "정상",
        Values = new ChartValues<double>(normalCountsByHour),
        PointGeometry = DefaultGeometries.Circle,
        PointGeometrySize = 8
    },
    new LineSeries
    {
        Title = "불량",
        Values = new ChartValues<double>(defectCountsByHour),
        PointGeometry = DefaultGeometries.Square,
        PointGeometrySize = 8
    }
};

lineChartTimeSeries.AxisX.Add(new Axis
{
    Title = "시간",
    Labels = hourLabels  // ["00:00", "01:00", ..., "23:00"]
});

lineChartTimeSeries.AxisY.Add(new Axis
{
    Title = "검사 건수",
    MinValue = 0
});
```

### 6.6 카메라별 통계 비교 (바 차트)
```csharp
var barChartCamera = new LiveCharts.WinForms.CartesianChart
{
    Location = new Point(30, 410),
    Size = new Size(890, 250),
    LegendLocation = LegendLocation.Top
};

barChartCamera.Series = new SeriesCollection
{
    new ColumnSeries
    {
        Title = "정상",
        Values = new ChartValues<double> { leftNormalCount, rightNormalCount }
    },
    new ColumnSeries
    {
        Title = "부품 불량",
        Values = new ChartValues<double> { leftComponentDefect, rightComponentDefect }
    },
    new ColumnSeries
    {
        Title = "납땜 불량",
        Values = new ChartValues<double> { leftSolderDefect, rightSolderDefect }
    },
    new ColumnSeries
    {
        Title = "폐기",
        Values = new ChartValues<double> { leftDiscard, rightDiscard }
    }
};

barChartCamera.AxisX.Add(new Axis
{
    Title = "카메라",
    Labels = new[] { "좌측 카메라", "우측 카메라" }
});

barChartCamera.AxisY.Add(new Axis
{
    Title = "검사 건수",
    MinValue = 0
});
```

### 6.7 Excel 다중 시트 내보내기
```csharp
private async void btnExportExcel_Click(object sender, EventArgs e)
{
    // Operator 이상만 가능
    if (currentUser.Role == "Viewer")
    {
        MessageBox.Show(
            "Excel 내보내기는 Operator 이상 권한이 필요합니다.",
            "권한 부족",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning
        );
        return;
    }

    try
    {
        var saveDialog = new SaveFileDialog
        {
            Filter = "Excel 파일 (*.xlsx)|*.xlsx",
            FileName = $"PCB검사통계_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
        };

        if (saveDialog.ShowDialog() == DialogResult.OK)
        {
            using (var package = new OfficeOpenXml.ExcelPackage())
            {
                // 시트 1: 전체 통계
                var wsOverall = package.Workbook.Worksheets.Add("전체 통계");
                ExportOverallStatistics(wsOverall);

                // 시트 2: 불량 유형별
                var wsDefectType = package.Workbook.Worksheets.Add("불량 유형별");
                ExportDefectTypeStatistics(wsDefectType);

                // 시트 3: 시간대별
                var wsTimeSeries = package.Workbook.Worksheets.Add("시간대별");
                ExportTimeSeriesStatistics(wsTimeSeries);

                // 시트 4: 카메라별
                var wsCamera = package.Workbook.Worksheets.Add("카메라별");
                ExportCameraStatistics(wsCamera);

                package.SaveAs(new FileInfo(saveDialog.FileName));
            }

            MessageBox.Show(
                "Excel 파일이 성공적으로 생성되었습니다.",
                "성공",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            // 사용자 활동 로그 기록
            await LogUserActivity("EXPORT_EXCEL", $"통계 데이터 내보내기: {saveDialog.FileName}");
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show(
            $"Excel 파일 생성 중 오류가 발생했습니다.\n{ex.Message}",
            "오류",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error
        );
    }
}
```

---

## 7. 모니터링 화면 (MonitoringForm)

### 7.1 디자인 개요
**레퍼런스 이미지**: 페이지 8-9 참조

### 7.2 레이아웃 구성
```
┌──────────────────────────────────────────────────────────┐
│  모니터링  [ 박스 상태 카메라 ]  [ OHT 제어 패널 카메라 ]  │
├──────────────────────────────────────────────────────────┤
│                                                          │
│  ┌────────────────┐        ┌────────────────┐          │
│  │                │        │                │          │
│  │  앞면 카메라    │        │  뒷면 카메라    │          │
│  │ (YOLO 적용)    │        │ (YOLO 적용)    │          │
│  │                │        │                │          │
│  └────────────────┘        └────────────────┘          │
│                                                          │
└──────────────────────────────────────────────────────────┘
```

### 7.3 탭 전환 버튼
```csharp
var btnBoxCamera = new Button
{
    Text = "박스 상태 카메라",
    Location = new Point(150, 15),
    Size = new Size(150, 35),
    BackColor = Color.FromArgb(80, 90, 130),
    ForeColor = Color.White,
    FlatStyle = FlatStyle.Flat
};

var btnOhtCamera = new Button
{
    Text = "OHT 제어 패널 카메라",
    Location = new Point(310, 15),
    Size = new Size(180, 35),
    BackColor = Color.FromArgb(45, 50, 80),
    ForeColor = Color.White,
    FlatStyle = FlatStyle.Flat
};

btnBoxCamera.Click += (s, e) =>
{
    pnlBoxCamera.Visible = true;
    pnlOhtCamera.Visible = false;
    btnBoxCamera.BackColor = Color.FromArgb(80, 90, 130);
    btnOhtCamera.BackColor = Color.FromArgb(45, 50, 80);
};

btnOhtCamera.Click += (s, e) =>
{
    pnlBoxCamera.Visible = false;
    pnlOhtCamera.Visible = true;
    btnBoxCamera.BackColor = Color.FromArgb(45, 50, 80);
    btnOhtCamera.BackColor = Color.FromArgb(80, 90, 130);
};
```

### 7.4 박스 상태 카메라 (앞면/뒷면)
```csharp
var pnlBoxCamera = new Panel
{
    Location = new Point(0, 60),
    Size = new Size(950, 600),
    Visible = true
};

// 앞면 카메라 PictureBox
var pbFrontCamera = new PictureBox
{
    Location = new Point(30, 20),
    Size = new Size(440, 550),
    SizeMode = PictureBoxSizeMode.Zoom,
    BorderStyle = BorderStyle.FixedSingle
};

var lblFrontCamera = new Label
{
    Text = "앞면 카메라 (YOLO 적용 화면)",
    Location = new Point(30, 0),
    Size = new Size(440, 20),
    TextAlign = ContentAlignment.MiddleCenter,
    Font = new Font("Segoe UI", 11F, FontStyle.Bold)
};

// 뒷면 카메라 PictureBox
var pbBackCamera = new PictureBox
{
    Location = new Point(480, 20),
    Size = new Size(440, 550),
    SizeMode = PictureBoxSizeMode.Zoom,
    BorderStyle = BorderStyle.FixedSingle
};

var lblBackCamera = new Label
{
    Text = "뒷면 카메라 (YOLO 적용 화면)",
    Location = new Point(480, 0),
    Size = new Size(440, 20),
    TextAlign = ContentAlignment.MiddleCenter,
    Font = new Font("Segoe UI", 11F, FontStyle.Bold)
};

pnlBoxCamera.Controls.AddRange(new Control[]
{
    lblFrontCamera, pbFrontCamera,
    lblBackCamera, pbBackCamera
});
```

### 7.5 OHT 제어 패널 카메라
```csharp
var pnlOhtCamera = new Panel
{
    Location = new Point(0, 60),
    Size = new Size(950, 600),
    Visible = false
};

var pbOhtCamera = new PictureBox
{
    Location = new Point(50, 20),
    Size = new Size(850, 550),
    SizeMode = PictureBoxSizeMode.Zoom,
    BorderStyle = BorderStyle.FixedSingle
};

var lblOhtCamera = new Label
{
    Text = "OHT 제어 패널 카메라 화면 (YOLO 적용 화면)",
    Location = new Point(50, 0),
    Size = new Size(850, 20),
    TextAlign = ContentAlignment.MiddleCenter,
    Font = new Font("Segoe UI", 11F, FontStyle.Bold)
};

pnlOhtCamera.Controls.AddRange(new Control[]
{
    lblOhtCamera, pbOhtCamera
});
```

### 7.6 실시간 스트리밍 업데이트
```csharp
private Timer streamingTimer;

private void InitializeMonitoring()
{
    streamingTimer = new Timer();
    streamingTimer.Interval = 100;  // 100ms (10 FPS)
    streamingTimer.Tick += UpdateCameraStreams;
    streamingTimer.Start();
}

private async void UpdateCameraStreams(object sender, EventArgs e)
{
    try
    {
        // 박스 상태 카메라 프레임 가져오기
        if (pnlBoxCamera.Visible)
        {
            var frontFrame = await apiClient.GetCameraFrame("front");
            var backFrame = await apiClient.GetCameraFrame("back");

            pbFrontCamera.Image?.Dispose();
            pbBackCamera.Image?.Dispose();

            pbFrontCamera.Image = ByteArrayToImage(frontFrame);
            pbBackCamera.Image = ByteArrayToImage(backFrame);
        }

        // OHT 제어 패널 카메라 프레임 가져오기
        if (pnlOhtCamera.Visible)
        {
            var ohtFrame = await apiClient.GetCameraFrame("oht");

            pbOhtCamera.Image?.Dispose();
            pbOhtCamera.Image = ByteArrayToImage(ohtFrame);
        }
    }
    catch (Exception ex)
    {
        LogError($"카메라 스트리밍 오류: {ex.Message}");
    }
}

private Image ByteArrayToImage(byte[] byteArray)
{
    using (var ms = new MemoryStream(byteArray))
    {
        return Image.FromStream(ms);
    }
}
```

---

## 8. 사용자 관리 (UserManagementForm)

### 8.1 디자인 개요
**레퍼런스 이미지**: 페이지 10-15 참조

### 8.2 레이아웃 구성
```
┌──────────────────────────────────────────────────────────┐
│  사용자 관리 (Admin 전용)                 [⟳ 새로고침]    │
├──────────────────────────────────────────────────────────┤
│  검색: [________] [검색]    권한 필터: [전체 ▼]           │
│                                                          │
│  [ 사용자 목록 DataGridView ]                             │
│  ┌─────────────────────────────────────────────────┐   │
│  │ ID │ 사용자명 │ 이름 │ 권한 │ 상태 │ 마지막 로그인 │   │
│  ├────┼──────────┼──────┼──────┼──────┼────────────┤   │
│  │ 1  │ admin    │ 관리자│Admin │활성  │2025-10-30  │   │
│  │ 2  │ operator1│홍길동│Operator│활성│2025-10-29  │   │
│  └─────────────────────────────────────────────────┘   │
│                                                          │
│  [사용자 추가] [사용자 수정] [사용자 삭제]                 │
│  [비밀번호 초기화] [활동 로그 조회]                       │
└──────────────────────────────────────────────────────────┘
```

### 8.3 검색 및 필터

#### 검색 TextBox
```csharp
var txtSearch = new TextBox
{
    Location = new Point(80, 20),
    Size = new Size(200, 25),
    Font = new Font("Segoe UI", 10F),
    PlaceholderText = "사용자명 또는 이름 검색"
};

var btnSearch = new Button
{
    Text = "검색",
    Location = new Point(290, 18),
    Size = new Size(70, 30),
    BackColor = Color.FromArgb(50, 100, 200),
    ForeColor = Color.White,
    FlatStyle = FlatStyle.Flat
};

btnSearch.Click += async (s, e) =>
{
    await LoadUsers(txtSearch.Text, cboRoleFilter.SelectedItem?.ToString());
};
```

#### 권한 필터 ComboBox
```csharp
var cboRoleFilter = new ComboBox
{
    Location = new Point(500, 20),
    Size = new Size(150, 25),
    DropDownStyle = ComboBoxStyle.DropDownList,
    Font = new Font("Segoe UI", 10F)
};

cboRoleFilter.Items.AddRange(new object[] { "전체", "Admin", "Operator", "Viewer" });
cboRoleFilter.SelectedIndex = 0;

cboRoleFilter.SelectedIndexChanged += async (s, e) =>
{
    await LoadUsers(txtSearch.Text, cboRoleFilter.SelectedItem?.ToString());
};
```

#### 새로고침 버튼
```csharp
var btnRefresh = new Button
{
    Text = "⟳",
    Location = new Point(880, 18),
    Size = new Size(40, 30),
    Font = new Font("Segoe UI", 14F),
    BackColor = Color.FromArgb(50, 100, 200),
    ForeColor = Color.White,
    FlatStyle = FlatStyle.Flat,
    Cursor = Cursors.Hand
};

btnRefresh.Click += async (s, e) =>
{
    txtSearch.Clear();
    cboRoleFilter.SelectedIndex = 0;
    await LoadUsers();
};
```

### 8.4 사용자 목록 DataGridView
```csharp
var dgvUsers = new DataGridView
{
    Location = new Point(20, 70),
    Size = new Size(900, 400),
    AutoGenerateColumns = false,
    AllowUserToAddRows = false,
    AllowUserToDeleteRows = false,
    ReadOnly = true,
    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
    MultiSelect = false,
    BackgroundColor = Color.White,
    BorderStyle = BorderStyle.Fixed3D,
    RowHeadersVisible = false,
    Font = new Font("Segoe UI", 10F)
};

// 컬럼 정의
dgvUsers.Columns.Add(new DataGridViewTextBoxColumn
{
    Name = "colUserId",
    HeaderText = "ID",
    DataPropertyName = "UserId",
    Width = 60
});

dgvUsers.Columns.Add(new DataGridViewTextBoxColumn
{
    Name = "colUsername",
    HeaderText = "사용자명",
    DataPropertyName = "Username",
    Width = 150
});

dgvUsers.Columns.Add(new DataGridViewTextBoxColumn
{
    Name = "colFullName",
    HeaderText = "이름",
    DataPropertyName = "FullName",
    Width = 150
});

dgvUsers.Columns.Add(new DataGridViewTextBoxColumn
{
    Name = "colRole",
    HeaderText = "권한",
    DataPropertyName = "Role",
    Width = 100
});

dgvUsers.Columns.Add(new DataGridViewTextBoxColumn
{
    Name = "colStatus",
    HeaderText = "상태",
    DataPropertyName = "Status",
    Width = 100
});

dgvUsers.Columns.Add(new DataGridViewTextBoxColumn
{
    Name = "colLastLogin",
    HeaderText = "마지막 로그인",
    DataPropertyName = "LastLoginTime",
    Width = 180,
    DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd HH:mm:ss" }
});
```

### 8.5 사용자 추가 다이얼로그
**레퍼런스 이미지**: 페이지 11 참조

```csharp
private void btnAddUser_Click(object sender, EventArgs e)
{
    var addUserDialog = new Form
    {
        Text = "사용자 생성",
        Size = new Size(400, 300),
        FormBorderStyle = FormBorderStyle.FixedDialog,
        StartPosition = FormStartPosition.CenterParent,
        MaximizeBox = false,
        MinimizeBox = false
    };

    var lblId = new Label { Text = "ID:", Location = new Point(30, 30), AutoSize = true };
    var txtId = new TextBox { Location = new Point(100, 27), Size = new Size(200, 25) };
    var btnCheckDuplicate = new Button
    {
        Text = "중복 체크",
        Location = new Point(310, 25),
        Size = new Size(70, 30)
    };

    var lblPw = new Label { Text = "PW:", Location = new Point(30, 70), AutoSize = true };
    var txtPw = new TextBox
    {
        Location = new Point(100, 67),
        Size = new Size(280, 25),
        UseSystemPasswordChar = true
    };

    var lblRole = new Label { Text = "권한:", Location = new Point(30, 110), AutoSize = true };
    var cboRole = new ComboBox
    {
        Location = new Point(100, 107),
        Size = new Size(280, 25),
        DropDownStyle = ComboBoxStyle.DropDownList
    };
    cboRole.Items.AddRange(new object[] { "Admin", "Operator", "Viewer" });
    cboRole.SelectedIndex = 2;  // 기본값: Viewer

    var btnOk = new Button
    {
        Text = "확인",
        Location = new Point(120, 180),
        Size = new Size(80, 35),
        DialogResult = DialogResult.OK
    };

    var btnCancel = new Button
    {
        Text = "취소",
        Location = new Point(220, 180),
        Size = new Size(80, 35),
        DialogResult = DialogResult.Cancel
    };

    // 중복 체크 버튼
    bool isIdChecked = false;
    btnCheckDuplicate.Click += async (s, args) =>
    {
        if (string.IsNullOrWhiteSpace(txtId.Text))
        {
            MessageBox.Show("사용자 ID를 입력하세요.", "입력 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        bool isDuplicate = await apiClient.CheckUsernameDuplicate(txtId.Text);

        if (isDuplicate)
        {
            MessageBox.Show("이미 존재하는 사용자 ID입니다.", "중복", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            isIdChecked = false;
        }
        else
        {
            MessageBox.Show("사용 가능한 ID입니다.", "확인", MessageBoxButtons.OK, MessageBoxIcon.Information);
            isIdChecked = true;
        }
    };

    // 확인 버튼
    btnOk.Click += async (s, args) =>
    {
        if (!isIdChecked)
        {
            MessageBox.Show("ID 중복 체크를 먼저 수행하세요.", "확인 필요", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            addUserDialog.DialogResult = DialogResult.None;
            return;
        }

        if (string.IsNullOrWhiteSpace(txtPw.Text))
        {
            MessageBox.Show("비밀번호를 입력하세요.", "입력 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            addUserDialog.DialogResult = DialogResult.None;
            return;
        }

        try
        {
            await apiClient.CreateUser(new User
            {
                Username = txtId.Text,
                Password = txtPw.Text,
                Role = cboRole.SelectedItem.ToString()
            });

            MessageBox.Show("사용자가 성공적으로 생성되었습니다.", "성공", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadUsers();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"사용자 생성 중 오류가 발생했습니다.\n{ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            addUserDialog.DialogResult = DialogResult.None;
        }
    };

    addUserDialog.Controls.AddRange(new Control[]
    {
        lblId, txtId, btnCheckDuplicate,
        lblPw, txtPw,
        lblRole, cboRole,
        btnOk, btnCancel
    });

    addUserDialog.ShowDialog();
}
```

### 8.6 사용자 수정 다이얼로그
**레퍼런스 이미지**: 페이지 12 참조

(사용자 추가와 거의 동일하지만, 기존 데이터를 불러와서 표시)

### 8.7 사용자 삭제 확인 다이얼로그
**레퍼런스 이미지**: 페이지 13 참조

```csharp
private void btnDeleteUser_Click(object sender, EventArgs e)
{
    if (dgvUsers.SelectedRows.Count == 0)
    {
        MessageBox.Show("삭제할 사용자를 선택하세요.", "선택 필요", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
    }

    var selectedUser = dgvUsers.SelectedRows[0].Cells["colUsername"].Value.ToString();

    var result = MessageBox.Show(
        $"{selectedUser}을 진짜 삭제하시겠습니까?",
        "사용자 삭제",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question
    );

    if (result == DialogResult.Yes)
    {
        try
        {
            var userId = (int)dgvUsers.SelectedRows[0].Cells["colUserId"].Value;
            await apiClient.DeleteUser(userId);

            MessageBox.Show("사용자가 성공적으로 삭제되었습니다.", "성공", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadUsers();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"사용자 삭제 중 오류가 발생했습니다.\n{ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
```

### 8.8 비밀번호 초기화 다이얼로그
**레퍼런스 이미지**: 페이지 14 참조

```csharp
private void btnResetPassword_Click(object sender, EventArgs e)
{
    if (dgvUsers.SelectedRows.Count == 0)
    {
        MessageBox.Show("비밀번호를 초기화할 사용자를 선택하세요.", "선택 필요", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
    }

    var selectedUser = dgvUsers.SelectedRows[0].Cells["colUsername"].Value.ToString();

    var result = MessageBox.Show(
        $"{selectedUser}의 비밀번호를 기본값으로 초기화합니다.",
        "비밀번호 초기화",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question
    );

    if (result == DialogResult.Yes)
    {
        try
        {
            var userId = (int)dgvUsers.SelectedRows[0].Cells["colUserId"].Value;
            await apiClient.ResetPassword(userId);

            MessageBox.Show("비밀번호가 성공적으로 초기화되었습니다.\n기본 비밀번호: password123", "성공", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"비밀번호 초기화 중 오류가 발생했습니다.\n{ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
```

### 8.9 사용자 활동 로그 조회
**레퍼런스 이미지**: 페이지 15 참조

```csharp
private void btnViewActivityLog_Click(object sender, EventArgs e)
{
    if (dgvUsers.SelectedRows.Count == 0)
    {
        MessageBox.Show("활동 로그를 조회할 사용자를 선택하세요.", "선택 필요", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
    }

    var userId = (int)dgvUsers.SelectedRows[0].Cells["colUserId"].Value;
    var username = dgvUsers.SelectedRows[0].Cells["colUsername"].Value.ToString();

    var activityLogDialog = new Form
    {
        Text = $"사용자 활동 로그 조회 - {username}",
        Size = new Size(800, 600),
        StartPosition = FormStartPosition.CenterParent
    };

    var dgvActivityLog = new DataGridView
    {
        Location = new Point(20, 20),
        Size = new Size(750, 520),
        AutoGenerateColumns = false,
        AllowUserToAddRows = false,
        ReadOnly = true,
        BackgroundColor = Color.White,
        BorderStyle = BorderStyle.Fixed3D,
        Font = new Font("Segoe UI", 10F)
    };

    dgvActivityLog.Columns.Add(new DataGridViewTextBoxColumn
    {
        Name = "colLogId",
        HeaderText = "ID",
        DataPropertyName = "LogId",
        Width = 60
    });

    dgvActivityLog.Columns.Add(new DataGridViewTextBoxColumn
    {
        Name = "colAction",
        HeaderText = "작업",
        DataPropertyName = "Action",
        Width = 150
    });

    dgvActivityLog.Columns.Add(new DataGridViewTextBoxColumn
    {
        Name = "colDetails",
        HeaderText = "상세",
        DataPropertyName = "Details",
        Width = 350
    });

    dgvActivityLog.Columns.Add(new DataGridViewTextBoxColumn
    {
        Name = "colTimestamp",
        HeaderText = "시간",
        DataPropertyName = "Timestamp",
        Width = 160,
        DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd HH:mm:ss" }
    });

    activityLogDialog.Controls.Add(dgvActivityLog);

    // 데이터 로드
    LoadActivityLog(dgvActivityLog, userId);

    activityLogDialog.ShowDialog();
}

private async void LoadActivityLog(DataGridView dgv, int userId)
{
    try
    {
        var logs = await apiClient.GetUserActivityLog(userId);
        dgv.DataSource = logs;
    }
    catch (Exception ex)
    {
        MessageBox.Show($"활동 로그를 불러오는 중 오류가 발생했습니다.\n{ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
```

---

## 9. 시스템 설정 (SettingsForm)

### 9.1 디자인 개요
**레퍼런스 이미지**: 페이지 16 참조

### 9.2 레이아웃 구성
```
┌──────────────────────────────────────────────────────────┐
│  시스템 설정 (Admin 전용)                 [⟳ 새로고침]    │
├──────────────────────────────────────────────────────────┤
│  Flask 서버 URL 설정: _________________________________  │
│                                                          │
│  MySQL 연결 정보 설정                                     │
│    - 호스트: _______________  포트: ______               │
│    - 데이터베이스: _______________                        │
│    - 사용자명: _______________                           │
│    - 비밀번호: _______________  [연결 테스트]             │
│                                                          │
│  알림 임계값 설정 (불량률 알림)                           │
│    - 불량률 임계값: ______ %                              │
│    - 알림 방식: [이메일 ☐] [팝업 ☑]                      │
│                                                          │
│  세션 타임아웃 설정                                       │
│    - 타임아웃: ______ 분                                  │
│                                                          │
│  로그 레벨 설정                                           │
│    - 레벨: [Debug ▼]                                     │
│                                                          │
│                    [저장]  [취소]                         │
└──────────────────────────────────────────────────────────┘
```

### 9.3 Flask 서버 URL 설정
```csharp
var lblFlaskUrl = new Label
{
    Text = "Flask 서버 URL 설정:",
    Location = new Point(30, 30),
    AutoSize = true,
    Font = new Font("Segoe UI", 11F, FontStyle.Bold)
};

var txtFlaskUrl = new TextBox
{
    Location = new Point(250, 27),
    Size = new Size(400, 25),
    Font = new Font("Segoe UI", 10F),
    Text = "http://100.64.1.1:5000"  // 기본값
};
```

### 9.4 MySQL 연결 정보 설정
```csharp
var grpMysql = new GroupBox
{
    Text = "MySQL 연결 정보 설정",
    Location = new Point(30, 80),
    Size = new Size(650, 180),
    Font = new Font("Segoe UI", 11F, FontStyle.Bold)
};

var lblHost = new Label { Text = "호스트:", Location = new Point(20, 35), AutoSize = true };
var txtHost = new TextBox { Location = new Point(100, 32), Size = new Size(200, 25), Text = "localhost" };

var lblPort = new Label { Text = "포트:", Location = new Point(330, 35), AutoSize = true };
var txtPort = new TextBox { Location = new Point(380, 32), Size = new Size(80, 25), Text = "3306" };

var lblDatabase = new Label { Text = "데이터베이스:", Location = new Point(20, 75), AutoSize = true };
var txtDatabase = new TextBox { Location = new Point(130, 72), Size = new Size(200, 25), Text = "pcb_inspection" };

var lblUsername = new Label { Text = "사용자명:", Location = new Point(20, 115), AutoSize = true };
var txtUsername = new TextBox { Location = new Point(100, 112), Size = new Size(200, 25), Text = "root" };

var lblPassword = new Label { Text = "비밀번호:", Location = new Point(330, 115), AutoSize = true };
var txtPassword = new TextBox { Location = new Point(410, 112), Size = new Size(200, 25), UseSystemPasswordChar = true };

var btnTestConnection = new Button
{
    Text = "연결 테스트",
    Location = new Point(500, 140),
    Size = new Size(100, 30),
    BackColor = Color.FromArgb(50, 100, 200),
    ForeColor = Color.White,
    FlatStyle = FlatStyle.Flat
};

btnTestConnection.Click += async (s, e) =>
{
    try
    {
        bool isConnected = await TestMySqlConnection(
            txtHost.Text,
            int.Parse(txtPort.Text),
            txtDatabase.Text,
            txtUsername.Text,
            txtPassword.Text
        );

        if (isConnected)
        {
            MessageBox.Show("MySQL 연결 성공!", "성공", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
        {
            MessageBox.Show("MySQL 연결 실패!", "실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"연결 테스트 중 오류가 발생했습니다.\n{ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
};

grpMysql.Controls.AddRange(new Control[]
{
    lblHost, txtHost, lblPort, txtPort,
    lblDatabase, txtDatabase,
    lblUsername, txtUsername, lblPassword, txtPassword,
    btnTestConnection
});
```

### 9.5 알림 임계값 설정
```csharp
var grpAlert = new GroupBox
{
    Text = "알림 임계값 설정 (불량률 알림)",
    Location = new Point(30, 280),
    Size = new Size(650, 100),
    Font = new Font("Segoe UI", 11F, FontStyle.Bold)
};

var lblThreshold = new Label
{
    Text = "불량률 임계값:",
    Location = new Point(20, 35),
    AutoSize = true
};

var numThreshold = new NumericUpDown
{
    Location = new Point(130, 32),
    Size = new Size(80, 25),
    Minimum = 0,
    Maximum = 100,
    Value = 10,  // 기본값: 10%
    DecimalPlaces = 1
};

var lblPercent = new Label
{
    Text = "%",
    Location = new Point(220, 35),
    AutoSize = true
};

var lblAlertMethod = new Label
{
    Text = "알림 방식:",
    Location = new Point(20, 70),
    AutoSize = true
};

var chkEmail = new CheckBox
{
    Text = "이메일",
    Location = new Point(130, 68),
    AutoSize = true
};

var chkPopup = new CheckBox
{
    Text = "팝업",
    Location = new Point(220, 68),
    AutoSize = true,
    Checked = true  // 기본값: 팝업 활성화
};

grpAlert.Controls.AddRange(new Control[]
{
    lblThreshold, numThreshold, lblPercent,
    lblAlertMethod, chkEmail, chkPopup
});
```

### 9.6 세션 타임아웃 설정
```csharp
var grpSession = new GroupBox
{
    Text = "세션 타임아웃 설정",
    Location = new Point(30, 400),
    Size = new Size(650, 80),
    Font = new Font("Segoe UI", 11F, FontStyle.Bold)
};

var lblTimeout = new Label
{
    Text = "타임아웃:",
    Location = new Point(20, 35),
    AutoSize = true
};

var numTimeout = new NumericUpDown
{
    Location = new Point(100, 32),
    Size = new Size(80, 25),
    Minimum = 5,
    Maximum = 1440,  // 최대 24시간
    Value = 30,  // 기본값: 30분
    Increment = 5
};

var lblMinutes = new Label
{
    Text = "분",
    Location = new Point(190, 35),
    AutoSize = true
};

grpSession.Controls.AddRange(new Control[]
{
    lblTimeout, numTimeout, lblMinutes
});
```

### 9.7 로그 레벨 설정
```csharp
var grpLog = new GroupBox
{
    Text = "로그 레벨 설정",
    Location = new Point(30, 500),
    Size = new Size(650, 80),
    Font = new Font("Segoe UI", 11F, FontStyle.Bold)
};

var lblLogLevel = new Label
{
    Text = "레벨:",
    Location = new Point(20, 35),
    AutoSize = true
};

var cboLogLevel = new ComboBox
{
    Location = new Point(80, 32),
    Size = new Size(150, 25),
    DropDownStyle = ComboBoxStyle.DropDownList,
    Font = new Font("Segoe UI", 10F)
};

cboLogLevel.Items.AddRange(new object[] { "Debug", "Info", "Warning", "Error" });
cboLogLevel.SelectedIndex = 1;  // 기본값: Info

grpLog.Controls.AddRange(new Control[]
{
    lblLogLevel, cboLogLevel
});
```

### 9.8 저장 및 취소 버튼
```csharp
var btnSave = new Button
{
    Text = "저장",
    Location = new Point(400, 610),
    Size = new Size(100, 40),
    BackColor = Color.FromArgb(50, 150, 50),
    ForeColor = Color.White,
    Font = new Font("Segoe UI", 11F, FontStyle.Bold),
    FlatStyle = FlatStyle.Flat,
    Cursor = Cursors.Hand
};

btnSave.Click += async (s, e) =>
{
    try
    {
        var settings = new SystemSettings
        {
            FlaskServerUrl = txtFlaskUrl.Text,
            MySqlHost = txtHost.Text,
            MySqlPort = int.Parse(txtPort.Text),
            MySqlDatabase = txtDatabase.Text,
            MySqlUsername = txtUsername.Text,
            MySqlPassword = txtPassword.Text,
            DefectRateThreshold = (double)numThreshold.Value,
            EnableEmailAlert = chkEmail.Checked,
            EnablePopupAlert = chkPopup.Checked,
            SessionTimeout = (int)numTimeout.Value,
            LogLevel = cboLogLevel.SelectedItem.ToString()
        };

        await apiClient.SaveSystemSettings(settings);

        MessageBox.Show("시스템 설정이 성공적으로 저장되었습니다.", "성공", MessageBoxButtons.OK, MessageBoxIcon.Information);

        // 사용자 활동 로그 기록
        await LogUserActivity("UPDATE_SETTINGS", "시스템 설정 변경");
    }
    catch (Exception ex)
    {
        MessageBox.Show($"설정 저장 중 오류가 발생했습니다.\n{ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
};

var btnCancel = new Button
{
    Text = "취소",
    Location = new Point(520, 610),
    Size = new Size(100, 40),
    BackColor = Color.FromArgb(150, 150, 150),
    ForeColor = Color.White,
    Font = new Font("Segoe UI", 11F, FontStyle.Bold),
    FlatStyle = FlatStyle.Flat,
    Cursor = Cursors.Hand
};

btnCancel.Click += async (s, e) =>
{
    await LoadSettings();  // 기존 설정 다시 로드
};
```

---

## 10. UI 컴포넌트 공통 가이드

### 10.1 색상 팔레트

#### 주요 색상
```csharp
// 네이비 (사이드바, 헤더)
Color.FromArgb(45, 50, 80)      // 기본
Color.FromArgb(60, 70, 110)     // 호버
Color.FromArgb(80, 90, 130)     // 선택

// 오렌지 (강조, 헤더)
Color.FromArgb(255, 120, 50)    // 기본
Color.FromArgb(200, 80, 30)     // 어두운

// 파란색 (버튼, 차트)
Color.FromArgb(50, 100, 200)    // 기본
Color.FromArgb(100, 150, 255)   // 밝은

// 초록색 (성공, 정상)
Color.FromArgb(50, 150, 50)     // 기본
Color.FromArgb(230, 255, 230)   // 연한

// 빨간색 (오류, 폐기)
Color.FromArgb(200, 50, 50)     // 기본
Color.FromArgb(255, 230, 230)   // 연한

// 회색 (비활성화, 배경)
Color.FromArgb(150, 150, 150)   // 기본
Color.FromArgb(245, 245, 245)   // 연한
```

### 10.2 폰트 스타일

#### 표준 폰트
```csharp
// 제목 (Form 제목, 그룹박스)
new Font("Segoe UI", 11F, FontStyle.Bold)

// 본문 (레이블, TextBox, DataGridView)
new Font("Segoe UI", 10F)

// 버튼
new Font("Segoe UI", 11F, FontStyle.Bold)

// 헤더 (상단 사용자 정보)
new Font("Segoe UI", 11F)

// 차트 제목
new Font("Segoe UI", 12F, FontStyle.Bold)
```

### 10.3 버튼 스타일

#### 표준 버튼
```csharp
var standardButton = new Button
{
    Size = new Size(100, 35),
    BackColor = Color.FromArgb(50, 100, 200),
    ForeColor = Color.White,
    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
    FlatStyle = FlatStyle.Flat,
    Cursor = Cursors.Hand
};
standardButton.FlatAppearance.BorderSize = 0;
```

#### 큰 버튼 (강조)
```csharp
var largeButton = new Button
{
    Size = new Size(150, 50),
    BackColor = Color.FromArgb(50, 100, 200),
    ForeColor = Color.White,
    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
    FlatStyle = FlatStyle.Flat,
    Cursor = Cursors.Hand
};
largeButton.FlatAppearance.BorderSize = 0;
```

#### 성공 버튼 (저장 등)
```csharp
var successButton = new Button
{
    Size = new Size(100, 40),
    BackColor = Color.FromArgb(50, 150, 50),
    ForeColor = Color.White,
    Font = new Font("Segoe UI", 11F, FontStyle.Bold),
    FlatStyle = FlatStyle.Flat,
    Cursor = Cursors.Hand
};
successButton.FlatAppearance.BorderSize = 0;
```

#### 위험 버튼 (삭제 등)
```csharp
var dangerButton = new Button
{
    Size = new Size(100, 35),
    BackColor = Color.FromArgb(200, 50, 50),
    ForeColor = Color.White,
    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
    FlatStyle = FlatStyle.Flat,
    Cursor = Cursors.Hand
};
dangerButton.FlatAppearance.BorderSize = 0;
```

### 10.4 DataGridView 스타일

#### 표준 스타일
```csharp
var dgv = new DataGridView
{
    AutoGenerateColumns = false,
    AllowUserToAddRows = false,
    AllowUserToDeleteRows = false,
    ReadOnly = true,
    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
    MultiSelect = false,
    BackgroundColor = Color.White,
    BorderStyle = BorderStyle.Fixed3D,
    RowHeadersVisible = false,
    Font = new Font("Segoe UI", 10F),
    AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
    {
        BackColor = Color.FromArgb(245, 245, 245)
    },
    ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
    {
        BackColor = Color.FromArgb(45, 50, 80),
        ForeColor = Color.White,
        Font = new Font("Segoe UI", 10F, FontStyle.Bold),
        Alignment = DataGridViewContentAlignment.MiddleCenter
    },
    EnableHeadersVisualStyles = false
};
```

### 10.5 그룹박스 스타일
```csharp
var groupBox = new GroupBox
{
    Font = new Font("Segoe UI", 11F, FontStyle.Bold),
    ForeColor = Color.FromArgb(45, 50, 80)
};
```

### 10.6 패널 스타일

#### 헤더 패널
```csharp
var headerPanel = new Panel
{
    Dock = DockStyle.Top,
    Height = 60,
    BackColor = Color.FromArgb(255, 120, 50)  // 오렌지
};
```

#### 사이드바 패널
```csharp
var sidebarPanel = new Panel
{
    Dock = DockStyle.Left,
    Width = 200,
    BackColor = Color.FromArgb(45, 50, 80)  // 네이비
};
```

#### 콘텐츠 패널
```csharp
var contentPanel = new Panel
{
    Dock = DockStyle.Fill,
    BackColor = Color.White
};
```

### 10.7 MessageBox 스타일

#### 정보 메시지
```csharp
MessageBox.Show(
    "작업이 성공적으로 완료되었습니다.",
    "성공",
    MessageBoxButtons.OK,
    MessageBoxIcon.Information
);
```

#### 경고 메시지
```csharp
MessageBox.Show(
    "입력값을 확인하세요.",
    "경고",
    MessageBoxButtons.OK,
    MessageBoxIcon.Warning
);
```

#### 오류 메시지
```csharp
MessageBox.Show(
    $"오류가 발생했습니다.\n{ex.Message}",
    "오류",
    MessageBoxButtons.OK,
    MessageBoxIcon.Error
);
```

#### 확인 메시지
```csharp
var result = MessageBox.Show(
    "정말 삭제하시겠습니까?",
    "확인",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question
);

if (result == DialogResult.Yes)
{
    // 삭제 처리
}
```

---

## 11. 추가 참고 사항

### 11.1 권한별 UI 제어
```csharp
private void ApplyRoleBasedUI()
{
    switch (currentUser.Role)
    {
        case "Admin":
            // 모든 기능 사용 가능
            btnUserManagement.Visible = true;
            btnSettings.Visible = true;
            btnExportExcel.Enabled = true;
            break;

        case "Operator":
            // 일부 기능 제한
            btnUserManagement.Visible = false;
            btnSettings.Visible = false;
            btnExportExcel.Enabled = true;
            break;

        case "Viewer":
            // 조회 기능만 사용 가능
            btnUserManagement.Visible = false;
            btnSettings.Visible = false;
            btnExportExcel.Enabled = false;
            break;
    }
}
```

### 11.2 반응형 레이아웃 (리사이즈)
```csharp
private void MainForm_Resize(object sender, EventArgs e)
{
    // 패널 크기 조정
    pnlContent.Width = this.ClientSize.Width - pnlSidebar.Width;
    pnlContent.Height = this.ClientSize.Height - pnlHeader.Height;

    // 차트 크기 조정
    if (pieChart != null)
    {
        pieChart.Width = pnlContent.Width / 2 - 60;
        pieChart.Height = Math.Min(pnlContent.Height / 2, 400);
    }
}
```

### 11.3 로딩 스피너 표시
```csharp
private async Task LoadDataWithSpinner(Func<Task> loadAction)
{
    var loadingForm = new Form
    {
        FormBorderStyle = FormBorderStyle.None,
        StartPosition = FormStartPosition.CenterParent,
        Size = new Size(200, 100),
        BackColor = Color.White
    };

    var lblLoading = new Label
    {
        Text = "로딩 중...",
        Font = new Font("Segoe UI", 12F),
        TextAlign = ContentAlignment.MiddleCenter,
        Dock = DockStyle.Fill
    };

    loadingForm.Controls.Add(lblLoading);
    loadingForm.Show(this);

    try
    {
        await loadAction();
    }
    finally
    {
        loadingForm.Close();
    }
}
```

---

## 참조 문서
- `CSharp_WinForms_Guide.md`: C# WinForms 기본 개발 가이드
- `CSharp_WinForms_Design_Specification.md`: 상세 설계 명세
- `MySQL_Database_Design.md`: 데이터베이스 스키마
- `Flask_Server_Setup.md`: Flask REST API 명세
