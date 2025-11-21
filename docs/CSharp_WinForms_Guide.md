# C# WinForms PCB 검사 모니터링 시스템 개발 가이드 ⭐ (이중 모델 아키텍처)

## 개요

이 가이드는 PCB 불량 검사 시스템의 모니터링 및 관리를 위한 C# WinForms 애플리케이션 개발 방법을 설명합니다.

**프로젝트 아키텍처** (2025-10-31 업데이트):
- **이중 YOLO 모델**: Component Model (부품 검출) + Solder Model (납땜 검출)
- **결과 융합**: Flask 서버에서 두 모델 결과를 융합하여 최종 판정
- **4가지 분류**: normal (정상), component_defect (부품불량), solder_defect (납땜불량), discard (폐기)
- **데이터베이스**: MySQL에서 융합 결과 및 개별 모델 결과 저장

---

## 시스템 요구사항

### 개발 환경
- **Windows 10/11**
- **Visual Studio 2022** (Community, Professional, Enterprise)
- **.NET Framework 4.8** 또는 **.NET 6+**
- **MySQL 8.0** (또는 연결할 MySQL 서버)

### NuGet 패키지
```
MySql.Data (또는 MySqlConnector)
Newtonsoft.Json
LiveCharts.WinForms
SocketIOClient  # WebSocket 실시간 프레임 수신 ⭐
System.Net.Http (내장)
```

---

## 프로젝트 생성

### 1. Visual Studio에서 새 프로젝트 생성

1. Visual Studio 2022 실행
2. "새 프로젝트 만들기"
3. "Windows Forms 앱 (.NET Framework)" 또는 "Windows Forms 앱" 선택
4. 프로젝트 이름: `PCB_Inspection_Monitor`
5. 위치: 원하는 폴더
6. 프레임워크: **.NET Framework 4.8** 또는 **.NET 6**

### 2. NuGet 패키지 설치

**도구 → NuGet 패키지 관리자 → 패키지 관리자 콘솔**

```powershell
# MySQL 연동
Install-Package MySql.Data
# 또는
Install-Package MySqlConnector

# JSON 처리
Install-Package Newtonsoft.Json

# 차트 라이브러리
Install-Package LiveCharts.WinForms
Install-Package LiveCharts.Wpf

# WebSocket 실시간 프레임 수신 ⭐
Install-Package SocketIOClient
```

---

## 프로젝트 구조

```
PCB_Inspection_Monitor/
├── Forms/
│   ├── MainForm.cs               # 메인 대시보드
│   ├── InspectionHistoryForm.cs  # 검사 이력 조회
│   ├── DefectImageViewerForm.cs  # 불량 이미지 뷰어
│   ├── StatisticsForm.cs         # 통계 화면
│   └── SettingsForm.cs           # 시스템 설정
│
├── Models/
│   ├── Inspection.cs             # 검사 결과 모델
│   ├── DefectImage.cs            # 불량 이미지 모델
│   ├── Statistics.cs             # 통계 모델
│   └── SystemStatus.cs           # 시스템 상태 모델
│
├── Services/
│   ├── ApiService.cs             # REST API 통신
│   ├── DatabaseService.cs        # MySQL 데이터베이스 연동
│   └── ImageService.cs           # 이미지 로드 및 표시
│
├── Utils/
│   ├── Config.cs                 # 설정 관리
│   └── Logger.cs                 # 로깅
│
└── Program.cs                    # 진입점
```

---

## 데이터 모델 정의

### Models/Inspection.cs (이중 모델 아키텍처)

```csharp
using System;
using System.Collections.Generic;

namespace PCB_Inspection_Monitor.Models
{
    public class Inspection
    {
        public int Id { get; set; }

        // 융합 결과 (최종 판정)
        public string FusionDecision { get; set; }  // "normal", "component_defect", "solder_defect", "discard"
        public int FusionSeverityLevel { get; set; } // 0-3 (심각도)

        // Component Model 결과
        public List<Defect> ComponentDefects { get; set; }  // JSON 역직렬화
        public int ComponentDefectCount { get; set; }
        public double? ComponentInferenceTimeMs { get; set; }

        // Solder Model 결과
        public List<Defect> SolderDefects { get; set; }  // JSON 역직렬화
        public int SolderDefectCount { get; set; }
        public double? SolderInferenceTimeMs { get; set; }

        // 검사 메타데이터
        public DateTime InspectionTime { get; set; }
        public double? TotalInferenceTimeMs { get; set; }
        public string LeftImagePath { get; set; }   // 좌측 카메라 (부품)
        public string RightImagePath { get; set; }  // 우측 카메라 (납땜)
        public int? GpioPin { get; set; }
        public int? UserId { get; set; }

        public Inspection()
        {
            ComponentDefects = new List<Defect>();
            SolderDefects = new List<Defect>();
        }
    }

    // 개별 불량 정보
    public class Defect
    {
        public string Type { get; set; }      // 불량 타입
        public double Confidence { get; set; } // 신뢰도
        public double[] BBox { get; set; }     // [x1, y1, x2, y2]
    }
}
```

**변경 사항**:
- ✅ `DefectType` → `FusionDecision` (융합 결과)
- ✅ `ComponentDefects`, `SolderDefects` 추가 (각 모델의 상세 결과)
- ✅ `LeftImagePath`, `RightImagePath` 분리 (양면 이미지)
- ✅ 추론 시간 필드 추가 (성능 모니터링)

### Models/Statistics.cs

```csharp
using System;
using System.Collections.Generic;

namespace PCB_Inspection_Monitor.Models
{
    public class Statistics
    {
        public int TotalInspections { get; set; }
        public int NormalCount { get; set; }
        public int ComponentDefectCount { get; set; }
        public int SolderDefectCount { get; set; }
        public int DiscardCount { get; set; }
        public double DefectRate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // 시간대별 통계
        public Dictionary<DateTime, int> HourlyInspections { get; set; }
        public Dictionary<string, int> DefectTypeCounts { get; set; }
    }
}
```

### Models/SystemStatus.cs

```csharp
using System;

namespace PCB_Inspection_Monitor.Models
{
    public class SystemStatus
    {
        public bool ServerOnline { get; set; }
        public bool DatabaseOnline { get; set; }
        public bool RaspberryPi1Online { get; set; }
        public bool RaspberryPi2Online { get; set; }
        public double ServerCpuUsage { get; set; }
        public double ServerGpuUsage { get; set; }
        public double ServerMemoryUsage { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}
```

---

## MySQL 데이터베이스 연동

**자세한 데이터베이스 스키마 설계는 `docs/MySQL_Database_Design.md`를 참조하세요.**

### Services/DatabaseService.cs

```csharp
using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using PCB_Inspection_Monitor.Models;

namespace PCB_Inspection_Monitor.Services
{
    public class DatabaseService
    {
        private string connectionString;

        public DatabaseService(string server, string database, string user, string password)
        {
            connectionString = $"Server={server};Database={database};Uid={user};Pwd={password};";
        }

        /// <summary>
        /// 데이터베이스 연결 테스트
        /// </summary>
        public bool TestConnection()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DB 연결 실패: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 검사 이력 조회 (페이징) - 이중 모델 아키텍처
        /// </summary>
        public List<Inspection> GetInspections(int page = 1, int pageSize = 50)
        {
            List<Inspection> inspections = new List<Inspection>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT id, fusion_decision, fusion_severity_level,
                               component_defects, component_defect_count, component_inference_time_ms,
                               solder_defects, solder_defect_count, solder_inference_time_ms,
                               inspection_time, total_inference_time_ms,
                               left_image_path, right_image_path, gpio_pin
                        FROM inspections
                        ORDER BY inspection_time DESC
                        LIMIT @pageSize OFFSET @offset";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@pageSize", pageSize);
                        cmd.Parameters.AddWithValue("@offset", (page - 1) * pageSize);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                inspections.Add(new Inspection
                                {
                                    Id = reader.GetInt32("id"),
                                    FusionDecision = reader.GetString("fusion_decision"),
                                    FusionSeverityLevel = reader.GetInt32("fusion_severity_level"),
                                    ComponentDefectCount = reader.GetInt32("component_defect_count"),
                                    ComponentInferenceTimeMs = reader.IsDBNull(reader.GetOrdinal("component_inference_time_ms"))
                                        ? null : reader.GetDouble("component_inference_time_ms"),
                                    SolderDefectCount = reader.GetInt32("solder_defect_count"),
                                    SolderInferenceTimeMs = reader.IsDBNull(reader.GetOrdinal("solder_inference_time_ms"))
                                        ? null : reader.GetDouble("solder_inference_time_ms"),
                                    InspectionTime = reader.GetDateTime("inspection_time"),
                                    TotalInferenceTimeMs = reader.IsDBNull(reader.GetOrdinal("total_inference_time_ms"))
                                        ? null : reader.GetDouble("total_inference_time_ms"),
                                    LeftImagePath = reader.IsDBNull(reader.GetOrdinal("left_image_path"))
                                        ? null : reader.GetString("left_image_path"),
                                    RightImagePath = reader.IsDBNull(reader.GetOrdinal("right_image_path"))
                                        ? null : reader.GetString("right_image_path"),
                                    GpioPin = reader.IsDBNull(reader.GetOrdinal("gpio_pin"))
                                        ? null : reader.GetInt32("gpio_pin"),
                                    // ComponentDefects, SolderDefects는 JSON 역직렬화 필요 (Newtonsoft.Json)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"검사 이력 조회 실패: {ex.Message}");
            }

            return inspections;
        }

        /// <summary>
        /// 통계 조회
        /// </summary>
        public Statistics GetStatistics(DateTime startDate, DateTime endDate)
        {
            Statistics stats = new Statistics
            {
                StartDate = startDate,
                EndDate = endDate,
                DefectTypeCounts = new Dictionary<string, int>()
            };

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    // 전체 통계 (이중 모델 아키텍처)
                    string query = @"
                        SELECT
                            COUNT(*) as total,
                            SUM(CASE WHEN fusion_decision = 'normal' THEN 1 ELSE 0 END) as normal,
                            SUM(CASE WHEN fusion_decision = 'component_defect' THEN 1 ELSE 0 END) as component,
                            SUM(CASE WHEN fusion_decision = 'solder_defect' THEN 1 ELSE 0 END) as solder,
                            SUM(CASE WHEN fusion_decision = 'discard' THEN 1 ELSE 0 END) as discard
                        FROM inspections
                        WHERE inspection_time BETWEEN @startDate AND @endDate";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@startDate", startDate);
                        cmd.Parameters.AddWithValue("@endDate", endDate);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                stats.TotalInspections = reader.GetInt32("total");
                                stats.NormalCount = reader.GetInt32("normal");
                                stats.ComponentDefectCount = reader.GetInt32("component");
                                stats.SolderDefectCount = reader.GetInt32("solder");
                                stats.DiscardCount = reader.GetInt32("discard");

                                int defectTotal = stats.ComponentDefectCount +
                                                 stats.SolderDefectCount +
                                                 stats.DiscardCount;
                                stats.DefectRate = stats.TotalInspections > 0
                                    ? (double)defectTotal / stats.TotalInspections * 100
                                    : 0;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"통계 조회 실패: {ex.Message}");
            }

            return stats;
        }

        /// <summary>
        /// 검사 이력 총 개수 조회
        /// </summary>
        public int GetTotalInspectionCount()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM inspections";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        return Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"총 개수 조회 실패: {ex.Message}");
                return 0;
            }
        }
    }
}
```

---

## REST API 통신

**Flask 서버 REST API 엔드포인트 상세 사양은 `docs/Flask_Server_Setup.md`를 참조하세요.**

### Services/ApiService.cs

```csharp
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PCB_Inspection_Monitor.Models;

namespace PCB_Inspection_Monitor.Services
{
    public class ApiService
    {
        private readonly HttpClient httpClient;
        private readonly string baseUrl;

        public ApiService(string serverUrl)
        {
            this.baseUrl = serverUrl;
            this.httpClient = new HttpClient();
            this.httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        /// <summary>
        /// 시스템 상태 조회
        /// </summary>
        public async Task<SystemStatus> GetSystemStatusAsync()
        {
            try
            {
                string url = $"{baseUrl}/api/system-status";
                HttpResponseMessage response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<SystemStatus>(jsonResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"시스템 상태 조회 실패: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 검사 이력 조회 (API 버전)
        /// </summary>
        public async Task<List<Inspection>> GetInspectionsAsync(int page, int pageSize)
        {
            try
            {
                string url = $"{baseUrl}/api/inspections?page={page}&limit={pageSize}";
                HttpResponseMessage response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponse<List<Inspection>>>(jsonResponse);
                return result.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"검사 이력 조회 실패: {ex.Message}");
                return new List<Inspection>();
            }
        }

        /// <summary>
        /// 통계 조회 (API 버전)
        /// </summary>
        public async Task<Statistics> GetStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                string url = $"{baseUrl}/api/statistics?start_date={startDate:yyyy-MM-dd}&end_date={endDate:yyyy-MM-dd}";
                HttpResponseMessage response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Statistics>(jsonResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"통계 조회 실패: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 시스템 설정 변경
        /// </summary>
        public async Task<bool> UpdateConfigAsync(object config)
        {
            try
            {
                string url = $"{baseUrl}/api/config";
                string jsonContent = JsonConvert.SerializeObject(config);
                HttpContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await httpClient.PostAsync(url, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"설정 변경 실패: {ex.Message}");
                return false;
            }
        }

        // API 응답 래퍼
        private class ApiResponse<T>
        {
            public string Status { get; set; }
            public T Data { get; set; }
        }
    }
}
```

---

## 메인 대시보드 UI

### Forms/MainForm.cs (Designer 코드 제외)

```csharp
using System;
using System.Windows.Forms;
using System.Drawing;
using System.Threading.Tasks;
using LiveCharts;
using LiveCharts.WinForms;
using LiveCharts.Wpf;
using PCB_Inspection_Monitor.Services;
using PCB_Inspection_Monitor.Models;

namespace PCB_Inspection_Monitor.Forms
{
    public partial class MainForm : Form
    {
        private DatabaseService dbService;
        private ApiService apiService;
        private Timer refreshTimer;

        // LiveCharts 컴포넌트
        private PieChart pieChart;
        private CartesianChart lineChart;

        public MainForm()
        {
            InitializeComponent();
            InitializeServices();
            InitializeCharts();
            InitializeTimer();
        }

        private void InitializeServices()
        {
            // MySQL 연결
            dbService = new DatabaseService(
                server: "localhost",  // 또는 GPU PC IP
                database: "pcb_inspection",
                user: "root",
                password: "your_password"
            );

            // Flask API 연결
            // 기본: http://100.64.1.1:5000 (Tailscale)
            // 원격: http://100.x.x.x:5000 (Tailscale VPN - 프로젝트 환경)
            apiService = new ApiService("http://100.64.1.1:5000");
        }

        private void InitializeCharts()
        {
            // 불량 유형별 파이 차트
            pieChart = new PieChart
            {
                Location = new Point(20, 100),
                Size = new Size(400, 300)
            };
            this.Controls.Add(pieChart);

            // 시간대별 검사 현황 라인 차트
            lineChart = new CartesianChart
            {
                Location = new Point(450, 100),
                Size = new Size(600, 300)
            };
            this.Controls.Add(lineChart);
        }

        private void InitializeTimer()
        {
            // 1초마다 대시보드 업데이트
            refreshTimer = new Timer();
            refreshTimer.Interval = 1000;
            refreshTimer.Tick += async (sender, e) => await RefreshDashboardAsync();
            refreshTimer.Start();
        }

        private async Task RefreshDashboardAsync()
        {
            try
            {
                // 오늘 통계 조회
                DateTime today = DateTime.Today;
                Statistics stats = dbService.GetStatistics(today, DateTime.Now);

                // 파이 차트 업데이트
                UpdatePieChart(stats);

                // 시스템 상태 조회
                SystemStatus status = await apiService.GetSystemStatusAsync();
                UpdateSystemStatus(status);

                // 라벨 업데이트
                lblTotalInspections.Text = $"총 검사: {stats.TotalInspections}";
                lblDefectRate.Text = $"불량률: {stats.DefectRate:F2}%";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"대시보드 새로고침 실패: {ex.Message}");
            }
        }

        private void UpdatePieChart(Statistics stats)
        {
            SeriesCollection series = new SeriesCollection
            {
                new PieSeries
                {
                    Title = "정상",
                    Values = new ChartValues<int> { stats.NormalCount },
                    Fill = System.Windows.Media.Brushes.Green
                },
                new PieSeries
                {
                    Title = "부품불량",
                    Values = new ChartValues<int> { stats.ComponentDefectCount },
                    Fill = System.Windows.Media.Brushes.Orange
                },
                new PieSeries
                {
                    Title = "납땜불량",
                    Values = new ChartValues<int> { stats.SolderDefectCount },
                    Fill = System.Windows.Media.Brushes.Yellow
                },
                new PieSeries
                {
                    Title = "폐기",
                    Values = new ChartValues<int> { stats.DiscardCount },
                    Fill = System.Windows.Media.Brushes.Red
                }
            };

            pieChart.Series = series;
        }

        private void UpdateSystemStatus(SystemStatus status)
        {
            if (status == null) return;

            lblServerStatus.Text = status.ServerOnline ? "서버: 온라인" : "서버: 오프라인";
            lblServerStatus.ForeColor = status.ServerOnline ? Color.Green : Color.Red;

            lblRaspberryPi1Status.Text = status.RaspberryPi1Online ? "라즈베리파이1: 온라인" : "라즈베리파이1: 오프라인";
            lblRaspberryPi1Status.ForeColor = status.RaspberryPi1Online ? Color.Green : Color.Red;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            refreshTimer?.Stop();
            base.OnFormClosing(e);
        }
    }
}
```

---

## 검사 이력 조회 화면

### Forms/InspectionHistoryForm.cs

```csharp
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using PCB_Inspection_Monitor.Services;
using PCB_Inspection_Monitor.Models;

namespace PCB_Inspection_Monitor.Forms
{
    public partial class InspectionHistoryForm : Form
    {
        private DatabaseService dbService;
        private int currentPage = 1;
        private int pageSize = 50;

        public InspectionHistoryForm(DatabaseService dbService)
        {
            InitializeComponent();
            this.dbService = dbService;
            LoadInspections();
        }

        private void LoadInspections()
        {
            try
            {
                List<Inspection> inspections = dbService.GetInspections(currentPage, pageSize);

                dataGridView1.DataSource = inspections;
                dataGridView1.Columns["Id"].HeaderText = "ID";
                dataGridView1.Columns["CameraId"].HeaderText = "카메라";
                dataGridView1.Columns["DefectType"].HeaderText = "불량 유형";
                dataGridView1.Columns["Confidence"].HeaderText = "신뢰도";
                dataGridView1.Columns["InspectionTime"].HeaderText = "검사 시간";

                // 페이징 정보
                int totalCount = dbService.GetTotalInspectionCount();
                int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                lblPaging.Text = $"페이지 {currentPage} / {totalPages}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"데이터 로드 실패: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnPrevPage_Click(object sender, EventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                LoadInspections();
            }
        }

        private void btnNextPage_Click(object sender, EventArgs e)
        {
            currentPage++;
            LoadInspections();
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                Inspection selectedInspection = (Inspection)dataGridView1.Rows[e.RowIndex].DataBoundItem;

                // 불량 이미지 뷰어 열기
                if (!string.IsNullOrEmpty(selectedInspection.ImagePath))
                {
                    DefectImageViewerForm viewer = new DefectImageViewerForm(selectedInspection.ImagePath);
                    viewer.ShowDialog();
                }
            }
        }
    }
}
```

---

## 설정 및 실행

### App.config

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="MySQL_Server" value="localhost"/>  <!-- 또는 Tailscale: 100.x.x.x -->
    <add key="MySQL_Database" value="pcb_inspection"/>
    <add key="MySQL_User" value="root"/>
    <add key="MySQL_Password" value="your_password"/>
    <add key="Flask_Server_URL" value="http://100.64.1.1:5000"/>  <!-- 로컬 또는 Tailscale: http://100.x.x.x:5000 -->
  </appSettings>
</configuration>
```

### Program.cs

```csharp
using System;
using System.Windows.Forms;
using PCB_Inspection_Monitor.Forms;

namespace PCB_Inspection_Monitor
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
```

---

## WebSocket 실시간 프레임 수신 ⭐ (바운딩 박스 많을 때 UI 멈춤 해결)

### 개요

기존 HTTP MJPEG 스트리밍 방식은 서버 푸시 모델로 인해 클라이언트 처리 속도가 느릴 때 네트워크 버퍼가 쌓여 UI가 멈추는 문제가 발생합니다. **WebSocket (SocketIO) 방식**으로 변경하여 **클라이언트가 프레임을 요청하는 방식 (Pull Model)**으로 변경합니다.

### 주요 변경사항

**Before (HTTP MJPEG)**:
- Server Push: 서버가 계속 프레임 전송
- 문제: 바운딩 박스 20+ 개일 때 UI 처리 느려지면 버퍼 누적 → UI 멈춤

**After (WebSocket)**:
- Client Pull: 클라이언트가 100ms마다 프레임 요청
- 해결: 백프레셔 제어 - 클라이언트 속도에 맞춰 프레임 수신

### PCBMonitoringView.cs 구현 (전체 코드)

```csharp
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using SocketIOClient;

namespace pcb_monitoring_program.Views.Monitoring
{
    public partial class PCBMonitoringView : UserControl
    {
        // SocketIO 클라이언트
        private SocketIOClient.SocketIO _socket;

        // 프레임 요청 타이머 (100ms 간격 = 10 FPS)
        private Timer _frameRequestTimer;

        // 프레임 카운트
        private int _leftFrameCount = 0;
        private int _rightFrameCount = 0;

        // Flask 서버 URL
        private const string SERVER_URL = "http://100.123.23.111:5000";

        public PCBMonitoringView()
        {
            InitializeComponent();

            // PictureBox 설정
            pb_LINE1PCBFRONT.SizeMode = PictureBoxSizeMode.Zoom;
            pb_LINE1PCBBACK.SizeMode = PictureBoxSizeMode.Zoom;

            // 더블 버퍼링 활성화
            EnableDoubleBuffering(pb_LINE1PCBFRONT);
            EnableDoubleBuffering(pb_LINE1PCBBACK);

            // 컨트롤 파괴 시 WebSocket 정리
            this.HandleDestroyed += OnHandleDestroyed;
        }

        private async void PCBMonitoringView_Load(object sender, EventArgs e)
        {
            // WebSocket 연결 시작
            await InitializeWebSocket();
        }

        private async Task InitializeWebSocket()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[PCBMonitoringView] WebSocket 연결 시작...");

                // SocketIO 클라이언트 생성
                _socket = new SocketIOClient.SocketIO(SERVER_URL);

                // 연결 성공 이벤트
                _socket.OnConnected += async (sender, e) =>
                {
                    System.Diagnostics.Debug.WriteLine($"[PCBMonitoringView] WebSocket 연결 성공: {SERVER_URL}");

                    // 프레임 요청 타이머 시작
                    if (InvokeRequired)
                        Invoke(new Action(StartFrameRequestTimer));
                    else
                        StartFrameRequestTimer();
                };

                // 연결 해제 이벤트
                _socket.OnDisconnected += (sender, e) =>
                {
                    System.Diagnostics.Debug.WriteLine("[PCBMonitoringView] WebSocket 연결 해제");

                    // 타이머 중지
                    if (InvokeRequired)
                        Invoke(new Action(StopFrameRequestTimer));
                    else
                        StopFrameRequestTimer();
                };

                // frame_data 이벤트 핸들러 (프레임 수신)
                _socket.On("frame_data", response =>
                {
                    try
                    {
                        string cameraId = response.GetValue<string>("camera_id");
                        string frameBase64 = response.GetValue<string>("frame");  // Base64 인코딩된 문자열

                        // Base64 디코딩
                        byte[] frameBytes = Convert.FromBase64String(frameBase64);

                        // JPEG 바이트를 Image로 변환
                        using (MemoryStream ms = new MemoryStream(frameBytes))
                        using (Image tempImage = Image.FromStream(ms))
                        {
                            // 복사본 생성 (원본은 스트림과 함께 해제)
                            Bitmap bitmap = new Bitmap(tempImage);

                            // UI 스레드에서 PictureBox 업데이트
                            if (cameraId == "left")
                            {
                                _leftFrameCount++;
                                if (InvokeRequired)
                                    BeginInvoke(new Action(() => UpdatePictureBox(pb_LINE1PCBFRONT, bitmap, "좌측")));
                                else
                                    UpdatePictureBox(pb_LINE1PCBFRONT, bitmap, "좌측");
                            }
                            else if (cameraId == "right")
                            {
                                _rightFrameCount++;
                                if (InvokeRequired)
                                    BeginInvoke(new Action(() => UpdatePictureBox(pb_LINE1PCBBACK, bitmap, "우측")));
                                else
                                    UpdatePictureBox(pb_LINE1PCBBACK, bitmap, "우측");
                            }

                            // 10프레임마다 로그
                            int frameCount = (cameraId == "left") ? _leftFrameCount : _rightFrameCount;
                            if (frameCount % 10 == 0)
                                System.Diagnostics.Debug.WriteLine($"[PCBMonitoringView] {cameraId} 프레임 수신: {frameCount}개");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[PCBMonitoringView] frame_data 처리 실패: {ex.Message}");
                    }
                });

                // WebSocket 연결
                await _socket.ConnectAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PCBMonitoringView] WebSocket 초기화 실패: {ex.Message}");
                MessageBox.Show($"WebSocket 연결 실패: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StartFrameRequestTimer()
        {
            if (_frameRequestTimer == null)
            {
                _frameRequestTimer = new Timer();
                _frameRequestTimer.Interval = 100;  // 100ms = 10 FPS
                _frameRequestTimer.Tick += OnFrameRequestTimerTick;
            }

            _frameRequestTimer.Start();
            System.Diagnostics.Debug.WriteLine("[PCBMonitoringView] 프레임 요청 타이머 시작 (100ms)");
        }

        private void StopFrameRequestTimer()
        {
            _frameRequestTimer?.Stop();
        }

        private async void OnFrameRequestTimerTick(object sender, EventArgs e)
        {
            try
            {
                if (_socket != null && _socket.Connected)
                {
                    // 양쪽 카메라에 대해 프레임 요청
                    await _socket.EmitAsync("request_frame", new { camera_id = "left" });
                    await _socket.EmitAsync("request_frame", new { camera_id = "right" });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PCBMonitoringView] 프레임 요청 실패: {ex.Message}");
            }
        }

        private void UpdatePictureBox(PictureBox pictureBox, Image newFrame, string camera)
        {
            try
            {
                // 이전 이미지 해제
                var oldImage = pictureBox.Image;
                pictureBox.Image = newFrame;
                oldImage?.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PCBMonitoringView] {camera} PictureBox 업데이트 실패: {ex.Message}");
            }
        }

        private void OnHandleDestroyed(object sender, EventArgs e)
        {
            CleanupWebSocket();
        }

        private async void CleanupWebSocket()
        {
            try
            {
                StopFrameRequestTimer();
                _frameRequestTimer?.Dispose();

                if (_socket != null)
                {
                    await _socket.DisconnectAsync();
                    _socket.Dispose();
                    _socket = null;
                }

                System.Diagnostics.Debug.WriteLine("[PCBMonitoringView] WebSocket 정리 완료");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PCBMonitoringView] WebSocket 정리 실패: {ex.Message}");
            }
        }

        private void EnableDoubleBuffering(PictureBox pictureBox)
        {
            typeof(PictureBox).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic,
                null, pictureBox, new object[] { true });
        }
    }
}
```

### 핵심 포인트

1. **SocketIO 클라이언트 초기화**: `new SocketIOClient.SocketIO(SERVER_URL)`
2. **연결 이벤트**: `OnConnected` → 타이머 시작
3. **프레임 요청**: 100ms마다 `request_frame` 이벤트 발송
4. **프레임 수신**: `frame_data` 이벤트 수신 → JPEG 디코딩 → PictureBox 업데이트
5. **메모리 관리**: `using` 구문으로 MemoryStream, Image 자동 해제

### 테스트 방법

1. Flask 서버 시작 (feature/flask-server 브랜치)
2. Visual Studio에서 C# 프로젝트 빌드
3. PCBMonitoringView 화면 열기
4. 디버그 콘솔에서 "WebSocket 연결 성공" 확인
5. 프레임 수신 로그 확인

---

## 다음 단계

1. **상세 UI 설계 참고**: `CSharp_WinForms_Design_Specification.md` - 관리자/운영자/조회자 권한 시스템, 7개 화면 상세 설계, Excel 내보내기 기능
2. **라즈베리파이 설정**: `RaspberryPi_Setup.md` 참고
3. **MySQL 데이터베이스 설계**: `MySQL_Database_Design.md` 참고
4. **Flask 서버 업데이트**: `Flask_Server_Setup.md` 참고

---

**작성일**: 2025-10-28
**수정일**: 2025-10-31
**버전**: 2.0 (이중 YOLO 모델 아키텍처)

**주요 변경사항 (v2.0)**:
- ✅ Inspection 모델 업데이트 (FusionDecision, ComponentDefects, SolderDefects)
- ✅ 데이터베이스 쿼리 업데이트 (fusion_decision, component_defect_count, solder_defect_count)
- ✅ 양면 이미지 경로 분리 (LeftImagePath, RightImagePath)
- ✅ 추론 시간 모니터링 필드 추가 (성능 분석용)

**관련 문서**:
- `PCB_Defect_Detection_Project.md` - 전체 시스템 아키텍처
- `CSharp_WinForms_Design_Specification.md` - 상세 UI 설계 및 권한 시스템
- `MySQL_Database_Design.md` - 데이터베이스 스키마 (이중 모델 아키텍처)
- `Flask_Server_Setup.md` - Flask 서버 설정 (결과 융합)
