# C# WinForms 앱 팀 시작 가이드

> PCB 불량 검사 시스템 C# WinForms 모니터링 앱 개발을 시작하는 팀원을 위한 빠른 시작 가이드입니다.

---

## 🎯 C# 앱 팀의 역할

- **WinForms UI 개발**: 7개 화면 구현 (로그인, 대시보드, 검사 이력 등)
- **Flask API 통신**: REST API 호출하여 데이터 조회
- **MySQL 연결**: 데이터베이스에서 검사 이력 및 통계 조회
- **실시간 차트**: LiveCharts로 실시간 통계 시각화
- **Excel 내보내기**: EPPlus로 검사 이력 Excel 파일 생성
- **권한 시스템**: Admin/Operator/Viewer 3단계 권한 관리

---

## 📚 반드시 읽어야 할 문서

### 필수 문서 (우선순위 순)

1. **[CSharp_WinForms_Design_Specification.md](../docs/CSharp_WinForms_Design_Specification.md)** ⭐ 가장 중요!
   - UI 설계 명세서 (7개 화면, 권한 시스템, Excel 내보내기)

2. **[CSharp_WinForms_Guide.md](../docs/CSharp_WinForms_Guide.md)**
   - C# WinForms 기본 개발 가이드

3. **[API_Contract.md](../docs/API_Contract.md)** ⭐ Flask API 명세!
   - Flask API 엔드포인트 및 응답 형식

4. **[database/README.md](../database/README.md)**
   - MySQL 데이터베이스 설정 가이드

5. **[csharp_winforms/.env.example](.env.example)**
   - 환경 변수 템플릿

### 참고 문서

- [Team_Collaboration_Guide.md](../docs/Team_Collaboration_Guide.md) - 팀 협업 규칙
- [Git_Workflow.md](../docs/Git_Workflow.md) - Git 브랜치 전략
- [Development_Setup.md](../docs/Development_Setup.md) - 로컬 환경 구성

---

## ⚙️ 개발 환경 설정

### 시스템 요구사항

- **OS**: Windows 10 / 11
- **IDE**: Visual Studio 2022 Community (무료)
- **.NET SDK**: .NET 6.0 이상
- **RAM**: 8GB 이상 (16GB 권장)

### 1. Visual Studio 2022 설치

```powershell
# 1. 다운로드: https://visualstudio.microsoft.com/vs/community/

# 2. 워크로드 선택:
# - .NET 데스크톱 개발
# - .NET Core 크로스 플랫폼 개발

# 3. 설치 완료 후 재부팅
```

### 2. .NET SDK 확인

```powershell
# PowerShell에서 실행
dotnet --version

# 예상 출력: 6.0.x 이상

# 설치되지 않았다면:
# https://dotnet.microsoft.com/download/dotnet/6.0
```

### 3. 프로젝트 클론 및 빌드

```powershell
# Git Bash 또는 PowerShell에서 실행
git clone https://github.com/ArianSung/PCB_Detect_Project.git
cd PCB_Detect_Project

# 브랜치 전환
git checkout develop
git checkout feature/csharp-app

# 프로젝트 디렉토리로 이동
cd csharp_winforms/PCB_Inspection_Monitor

# NuGet 패키지 복원
dotnet restore

# 빌드
dotnet build

# 실행
dotnet run
```

---

## 📦 NuGet 패키지 설치

### 필수 패키지

| 패키지 | 버전 | 용도 |
|--------|------|------|
| `MySql.Data` | 8.0.32 | MySQL 데이터베이스 연결 |
| `Newtonsoft.Json` | 13.0.3 | JSON 직렬화/역직렬화 |
| `LiveCharts.WinForms` | 0.9.7 | 실시간 차트 |
| `EPPlus` | 5.8.14 | Excel 내보내기 |

### NuGet 패키지 관리자로 설치

```powershell
# Visual Studio에서:
# 도구 → NuGet 패키지 관리자 → 패키지 관리자 콘솔

Install-Package MySql.Data -Version 8.0.32
Install-Package Newtonsoft.Json -Version 13.0.3
Install-Package LiveCharts.WinForms -Version 0.9.7
Install-Package EPPlus -Version 5.8.14
```

또는 `PCB_Inspection_Monitor.csproj` 파일에 직접 추가:

```xml
<ItemGroup>
  <PackageReference Include="MySql.Data" Version="8.0.32" />
  <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
  <PackageReference Include="LiveCharts.WinForms" Version="0.9.7" />
  <PackageReference Include="EPPlus" Version="5.8.14" />
</ItemGroup>
```

---

## 🌐 환경 변수 설정

### `.env` 파일 생성

```powershell
# csharp_winforms/ 디렉토리에서
cp .env.example .env
notepad .env
```

**`csharp_winforms/.env` 파일 내용:**

```bash
# Flask API
API_BASE_URL=http://100.x.x.x:5000  # GPU PC의 Tailscale IP로 변경

# MySQL 데이터베이스
DB_HOST=100.x.x.x          # Windows PC의 Tailscale IP (또는 localhost)
DB_PORT=3306
DB_NAME=pcb_inspection
DB_USER=pcb_viewer
DB_PASSWORD=1234
```

---

## 🖥️ 7개 화면 구조

### 화면 목록 (우선순위 순)

1. **로그인 화면** (`LoginForm.cs`)
   - 사용자 인증 (Admin/Operator/Viewer)
   - 권한 확인

2. **메인 대시보드** (`MainDashboard.cs`)
   - 실시간 통계 (오늘 검사 수, 불량률 등)
   - LiveCharts 차트 (시간별 불량 추이)

3. **검사 이력 조회** (`InspectionHistory.cs`)
   - DataGridView + 페이지네이션
   - 검색 및 필터링 (날짜, 분류 타입)

4. **상세 결과 뷰어** (`DetailViewer.cs`)
   - 선택한 검사 결과의 상세 정보
   - 이미지 표시 (원본 + 결과 표시)

5. **통계 및 차트** (`StatisticsForm.cs`)
   - 일별/주별/월별 통계
   - LiveCharts 다양한 차트 (막대, 선, 파이)

6. **설정 화면** (`SettingsForm.cs`)
   - API URL, DB 연결 설정
   - Admin 전용

7. **Excel 내보내기** (`ExportForm.cs`)
   - 검사 이력 Excel 파일 생성
   - EPPlus 사용

---

## 🗄️ MySQL 연결 테스트

### 1. 연결 문자열 생성

```csharp
// 예시 코드
using MySql.Data.MySqlClient;

string connectionString = "Server=localhost;Port=3306;Database=pcb_inspection;Uid=pcb_viewer;Pwd=1234;";

using (MySqlConnection conn = new MySqlConnection(connectionString))
{
    try
    {
        conn.Open();
        Console.WriteLine("✓ MySQL 연결 성공!");

        // 간단한 쿼리 테스트
        MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM inspection_history", conn);
        int count = Convert.ToInt32(cmd.ExecuteScalar());
        Console.WriteLine($"검사 이력 개수: {count}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ MySQL 연결 실패: {ex.Message}");
    }
}
```

### 2. 환경 변수 로드 (.env 파일)

C#에서 `.env` 파일 로드를 위한 `DotNetEnv` 패키지 사용:

```powershell
Install-Package DotNetEnv -Version 2.5.0
```

```csharp
using DotNetEnv;

// .env 파일 로드
Env.Load("../../.env");

// 환경 변수 읽기
string dbHost = Environment.GetEnvironmentVariable("DB_HOST");
string dbUser = Environment.GetEnvironmentVariable("DB_USER");
string dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");

string connectionString = $"Server={dbHost};Port=3306;Database=pcb_inspection;Uid={dbUser};Pwd={dbPassword};";
```

---

## 🌐 Flask API 호출 테스트

### 1. Health Check API

```csharp
using System.Net.Http;
using Newtonsoft.Json;

public async Task TestFlaskAPI()
{
    using (var client = new HttpClient())
    {
        try
        {
            var response = await client.GetAsync("http://100.x.x.x:5000/health");
            var json = await response.Content.ReadAsStringAsync();

            Console.WriteLine("✓ Flask API 연결 성공!");
            Console.WriteLine(json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Flask API 연결 실패: {ex.Message}");
        }
    }
}
```

### 2. 검사 이력 조회 API

```csharp
public async Task<List<InspectionRecord>> GetInspectionHistory(int page, int limit)
{
    using (var client = new HttpClient())
    {
        var response = await client.GetAsync(
            $"http://100.x.x.x:5000/history?page={page}&limit={limit}"
        );

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<HistoryResponse>(json);

        return result.Records;
    }
}

// 데이터 모델
public class InspectionRecord
{
    public int Id { get; set; }
    public string Timestamp { get; set; }
    public string CameraId { get; set; }
    public string Classification { get; set; }
    public double Confidence { get; set; }
    public int TotalDefects { get; set; }
}

public class HistoryResponse
{
    public bool Success { get; set; }
    public int Page { get; set; }
    public int TotalRecords { get; set; }
    public List<InspectionRecord> Records { get; set; }
}
```

---

## 📊 LiveCharts 실시간 차트 예시

### 1. NuGet 패키지 참조

```xml
<PackageReference Include="LiveCharts.WinForms" Version="0.9.7" />
```

### 2. 간단한 차트 예시

```csharp
using LiveCharts;
using LiveCharts.WinForms;

// Form 디자이너에서 CartesianChart 추가

public void InitializeChart()
{
    // 데이터 생성
    var values = new ChartValues<int> { 10, 20, 15, 30, 25 };

    // 시리즈 설정
    cartesianChart1.Series = new SeriesCollection
    {
        new LineSeries
        {
            Title = "불량 개수",
            Values = values
        }
    };

    // X축 라벨
    cartesianChart1.AxisX.Add(new Axis
    {
        Title = "날짜",
        Labels = new[] { "10/21", "10/22", "10/23", "10/24", "10/25" }
    });

    // Y축 설정
    cartesianChart1.AxisY.Add(new Axis
    {
        Title = "개수"
    });
}
```

---

## 📝 첫 번째 작업 제안

### 작업 1: 로그인 화면 구현

**목표**: 사용자 인증 및 권한 확인

```csharp
// LoginForm.cs
public partial class LoginForm : Form
{
    public string UserRole { get; private set; }

    private void btnLogin_Click(object sender, EventArgs e)
    {
        string username = txtUsername.Text;
        string password = txtPassword.Text;

        // 간단한 하드코딩 인증 (나중에 DB 연동)
        if (username == "admin" && password == "1234")
        {
            UserRole = "Admin";
            this.DialogResult = DialogResult.OK;
        }
        else if (username == "operator" && password == "1234")
        {
            UserRole = "Operator";
            this.DialogResult = DialogResult.OK;
        }
        else if (username == "viewer" && password == "1234")
        {
            UserRole = "Viewer";
            this.DialogResult = DialogResult.OK;
        }
        else
        {
            MessageBox.Show("잘못된 사용자명 또는 비밀번호입니다.", "로그인 실패");
        }
    }
}
```

### 작업 2: 메인 대시보드 구현

**목표**: Flask API에서 실시간 통계 조회

```csharp
// MainDashboard.cs
public async Task LoadDashboardData()
{
    using (var client = new HttpClient())
    {
        // 오늘 통계 조회
        var response = await client.GetAsync(
            $"{apiBaseUrl}/statistics?start_date={DateTime.Today:yyyy-MM-dd}&end_date={DateTime.Today:yyyy-MM-dd}"
        );

        var json = await response.Content.ReadAsStringAsync();
        var stats = JsonConvert.DeserializeObject<Statistics>(json);

        // UI 업데이트
        lblTotalInspections.Text = stats.TotalInspections.ToString();
        lblDefectRate.Text = $"{stats.DefectRate * 100:F2}%";
        lblNormalCount.Text = stats.ClassificationCounts.Normal.ToString();
    }
}
```

### 작업 3: 검사 이력 DataGridView

**목표**: 검사 이력을 DataGridView에 표시

```csharp
// InspectionHistory.cs
public async Task LoadInspectionHistory(int page = 1)
{
    var records = await GetInspectionHistory(page, 20);

    // DataGridView에 바인딩
    dataGridView1.DataSource = records;

    // 컬럼 헤더 설정
    dataGridView1.Columns["Id"].HeaderText = "ID";
    dataGridView1.Columns["Timestamp"].HeaderText = "검사 시각";
    dataGridView1.Columns["CameraId"].HeaderText = "카메라";
    dataGridView1.Columns["Classification"].HeaderText = "분류";
    dataGridView1.Columns["Confidence"].HeaderText = "신뢰도";
}
```

---

## 🤖 AI에게 물어볼 프롬프트

### 시작 프롬프트 (복사해서 사용하세요)

```
안녕! 나는 PCB 불량 검사 시스템의 C# WinForms 모니터링 앱 팀원이야.

**내 역할:**
- WinForms UI 개발 (7개 화면)
- Flask REST API 호출하여 데이터 조회
- MySQL 데이터베이스 연결 (검사 이력 조회)
- LiveCharts로 실시간 차트 표시
- Excel 내보내기 기능 (EPPlus)
- 권한 시스템 구현 (Admin/Operator/Viewer)

**읽어야 할 핵심 문서:**
1. `docs/CSharp_WinForms_Design_Specification.md` - UI 설계 명세서 (7개 화면, 권한 시스템)
2. `docs/CSharp_WinForms_Guide.md` - C# WinForms 기본 개발 가이드
3. `docs/API_Contract.md` - Flask API 명세서
4. `database/README.md` - MySQL 데이터베이스 설정 가이드
5. `csharp_winforms/.env.example` - 환경 변수 템플릿

**개발 환경:**
- OS: Windows 10 / 11
- IDE: Visual Studio 2022 Community
- .NET SDK: .NET 6.0
- 데이터베이스: MySQL 8.0 (Windows PC - localhost 또는 Tailscale 100.x.x.x:3306)
- DB 계정: `pcb_viewer` / 비밀번호: `1234`

**NuGet 패키지:**
- `MySql.Data` (8.0.32) - MySQL 연결
- `Newtonsoft.Json` (13.0.3) - JSON 처리
- `LiveCharts.WinForms` (0.9.7) - 실시간 차트
- `EPPlus` (5.8.14) - Excel 내보내기

**환경 변수 설정 (csharp_winforms/.env):**
```
API_BASE_URL=http://100.x.x.x:5000
DB_HOST=100.x.x.x          # 또는 localhost
DB_PORT=3306
DB_NAME=pcb_inspection
DB_USER=pcb_viewer
DB_PASSWORD=1234
```

**7개 화면:**
1. 로그인 화면 (권한 시스템)
2. 메인 대시보드 (실시간 통계)
3. 검사 이력 조회 (DataGridView + 페이지네이션)
4. 상세 결과 뷰어 (이미지 + 불량 정보)
5. 통계 및 차트 (LiveCharts)
6. 설정 화면 (Admin 전용)
7. Excel 내보내기

**첫 번째 작업:**
1. Visual Studio에서 프로젝트 열기
2. NuGet 패키지 복원: `dotnet restore`
3. 빌드: `dotnet build`
4. MySQL 연결 테스트
5. Flask API 호출 테스트

위 정보를 바탕으로, C# WinForms 프로젝트를 처음 설정하고 MySQL 및 Flask API 연결을 테스트하는 과정을 안내해줘.
특히 7개 화면의 구조와 권한 시스템을 어떻게 설계할지도 설명해줘.
```

---

## ✅ 체크리스트

### 개발 환경 설정 완료 체크리스트

- [ ] Visual Studio 2022 설치 완료
- [ ] .NET 6.0 SDK 설치 확인
- [ ] 프로젝트 클론 및 브랜치 전환 완료
- [ ] NuGet 패키지 복원 및 빌드 성공
- [ ] `csharp_winforms/.env` 파일 설정 완료

### 연결 테스트 완료 체크리스트

- [ ] MySQL 연결 테스트 성공
- [ ] Flask API 호출 테스트 성공 (`/health`)
- [ ] 검사 이력 조회 API 테스트 성공 (`/history`)

### 문서 읽기 체크리스트

- [ ] `docs/CSharp_WinForms_Design_Specification.md` 읽기 완료
- [ ] `docs/CSharp_WinForms_Guide.md` 읽기 완료
- [ ] `docs/API_Contract.md` 읽기 완료
- [ ] `database/README.md` 읽기 완료

---

## 🚨 자주 발생하는 문제 및 해결

### 문제 1: NuGet 패키지 복원 실패

**에러**: `Unable to find package`

**해결 방법:**
1. NuGet 소스 확인: `dotnet nuget list source`
2. 캐시 정리: `dotnet nuget locals all --clear`
3. 재시도: `dotnet restore`

### 문제 2: MySQL 연결 실패

**에러**: `Unable to connect to any of the specified MySQL hosts`

**해결 방법:**
1. MySQL 서버 실행 중인지 확인 (Windows 서비스)
2. Tailscale VPN 연결 확인
3. `.env` 파일의 `DB_HOST` 확인
4. 방화벽에서 3306 포트 허용

### 문제 3: Flask API CORS 오류

**에러**: `Access to XMLHttpRequest has been blocked by CORS policy`

**해결 방법:**
- Flask 서버에서 CORS 설정 확인 (`flask-cors` 패키지)
- Flask 팀에게 CORS 허용 요청

---

## 📞 도움 요청

- **C# 앱 팀 리더**: [연락처]
- **Flask 팀 (API)**: [연락처]
- **전체 팀 채팅방**: [링크]

---

## 🔗 추가 참고 자료

### C# WinForms 공식 문서

- [.NET WinForms Docs](https://docs.microsoft.com/en-us/dotnet/desktop/winforms/)
- [LiveCharts Documentation](https://lvcharts.net/)
- [EPPlus Documentation](https://github.com/EPPlusSoftware/EPPlus)

---

**마지막 업데이트**: 2025-10-25
**작성자**: 팀 리더
