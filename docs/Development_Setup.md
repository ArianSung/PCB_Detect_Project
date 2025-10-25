# 개발 환경 구성 가이드

팀 프로젝트 시작을 위한 로컬 개발 환경 설정 가이드

---

## 🎯 개요

이 문서는 PCB 불량 검사 시스템 프로젝트에 참여하는 모든 팀원이 **동일한 개발 환경**을 구성할 수 있도록 돕습니다.

---

## 💻 시스템 요구사항

### Flask 서버 팀 (GPU PC)

| 항목 | 요구사항 | 권장사항 |
|------|----------|----------|
| **OS** | Ubuntu 20.04 / 22.04 | Ubuntu 22.04 LTS |
| **GPU** | NVIDIA GPU (CUDA 지원) | RTX 4080 Super 이상 |
| **VRAM** | 8GB 이상 | 16GB 이상 |
| **RAM** | 16GB 이상 | 32GB 이상 |
| **저장공간** | 50GB 이상 | 100GB 이상 (SSD) |
| **Python** | 3.8 ~ 3.10 | Python 3.10 |
| **CUDA** | 11.8 이상 | CUDA 11.8 |

### AI 모델 팀 (GPU PC 공유 또는 개별)

- Flask 서버 팀과 동일한 요구사항
- 데이터셋 저장을 위한 추가 저장공간 (50GB 이상)

### 라즈베리파이 팀

| 항목 | 요구사항 |
|------|----------|
| **하드웨어** | Raspberry Pi 4 Model B (4GB 이상) |
| **OS** | Raspberry Pi OS (64-bit) |
| **웹캠** | USB 웹캠 (640x480 이상) |
| **릴레이 모듈** | 4채널 릴레이 모듈 (GPIO 제어용) |
| **네트워크** | Wi-Fi 또는 Ethernet |

### C# 앱 팀

| 항목 | 요구사항 | 권장사항 |
|------|----------|----------|
| **OS** | Windows 10 / 11 | Windows 11 |
| **IDE** | Visual Studio 2022 / Rider | Visual Studio 2022 Community |
| **.NET SDK** | .NET 6.0 이상 | .NET 6.0 |
| **RAM** | 8GB 이상 | 16GB 이상 |

---

## 🛠️ 공통 설정

### 1. Git 설치 및 설정

```bash
# Ubuntu/Linux
sudo apt update
sudo apt install git -y

# Windows
# https://git-scm.com/download/win 에서 다운로드 및 설치

# Git 사용자 정보 설정
git config --global user.name "Your Name"
git config --global user.email "your.email@example.com"

# Git 에디터 설정 (선택)
git config --global core.editor "vim"  # 또는 "code" (VS Code)

# 설정 확인
git config --list
```

### 2. 저장소 클론

```bash
# HTTPS 방식 (권장)
git clone https://github.com/ArianSung/PCB_Detect_Project.git
cd PCB_Detect_Project

# SSH 방식 (SSH 키 등록 필요)
# git clone git@github.com:ArianSung/PCB_Detect_Project.git
# cd PCB_Detect_Project

# 브랜치 확인
git branch -a
```

---

## 🐍 Flask 서버 팀 환경 설정

### 1. Miniconda 설치

```bash
# 1. Miniconda 다운로드
wget https://repo.anaconda.com/miniconda/Miniconda3-latest-Linux-x86_64.sh

# 2. 설치 스크립트 실행
bash Miniconda3-latest-Linux-x86_64.sh

# 3. 터미널 재시작 또는 소스 적용
source ~/.bashrc

# 4. Conda 버전 확인
conda --version
```

### 2. 가상환경 생성

```bash
# 1. pcb_defect 가상환경 생성 (Python 3.10)
conda create -n pcb_defect python=3.10 -y

# 2. 가상환경 활성화
conda activate pcb_defect

# 3. 확인
python --version  # Python 3.10.x
which python      # /home/사용자명/miniconda3/envs/pcb_defect/bin/python
```

### 3. PyTorch 및 CUDA 설치

```bash
# CUDA 11.8 + PyTorch 2.7.1 설치
pip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cu118

# 설치 확인
python -c "import torch; print(f'PyTorch: {torch.__version__}'); print(f'CUDA: {torch.cuda.is_available()}')"

# 예상 출력:
# PyTorch: 2.7.1+cu118
# CUDA: True
```

### 4. 프로젝트 패키지 설치

```bash
# requirements.txt 설치
pip install -r requirements.txt

# 주요 패키지 확인
pip list | grep -E "ultralytics|flask|opencv|mysql"
```

### 5. MySQL 데이터베이스 설정

```bash
# 1. MySQL 설치
sudo apt update
sudo apt install mysql-server -y

# 2. MySQL 보안 설정
sudo mysql_secure_installation

# 3. MySQL 접속
sudo mysql -u root -p

# 4. 데이터베이스 생성
CREATE DATABASE pcb_inspection CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

# 5. 사용자 생성 및 권한 부여 (선택)
CREATE USER 'pcb_user'@'localhost' IDENTIFIED BY 'your_password';
GRANT ALL PRIVILEGES ON pcb_inspection.* TO 'pcb_user'@'localhost';
FLUSH PRIVILEGES;

# 6. 스키마 적용
mysql -u root -p pcb_inspection < database/schema.sql
```

### 6. 환경 변수 설정

```bash
# 1. 환경 설정 스크립트 실행
bash scripts/setup_env.sh

# 2. .env 파일 수정
nano src/server/.env

# 3. 최소한 다음 항목 변경:
# - DB_PASSWORD: MySQL 비밀번호
# - SERVER_URL: Tailscale IP (원격 환경인 경우)
```

### 7. Flask 서버 실행 테스트

```bash
# 개발 모드로 실행
cd src/server
python app.py

# 예상 출력:
# * Running on http://0.0.0.0:5000
# * GPU 사용 가능: True

# 다른 터미널에서 테스트
curl http://localhost:5000/api/v1/health
```

---

## 🤖 AI 모델 팀 환경 설정

### 1~4. Flask 서버 팀과 동일

### 5. 데이터셋 준비

```bash
# 1. 데이터셋 디렉토리 생성
mkdir -p data/raw
mkdir -p data/processed

# 2. 데이터셋 다운로드 (예시)
# (실제 데이터셋은 팀에서 공유)

# 3. YOLO 형식으로 변환
# (Dataset_Guide.md 참조)

# 4. 데이터셋 구조 확인
tree data/processed -L 2
```

### 6. YOLO 모델 학습 테스트

```bash
# 1. YOLO 설정 파일 확인
cat configs/yolo_config.yaml

# 2. 학습 테스트 (1 epoch)
python src/training/train_yolo.py --config configs/yolo_config.yaml --epochs 1

# 3. 학습 결과 확인
ls runs/detect/train
```

---

## 🍓 라즈베리파이 팀 환경 설정

### 1. Raspberry Pi OS 설치

1. **Raspberry Pi Imager** 다운로드: https://www.raspberrypi.com/software/
2. OS 선택: Raspberry Pi OS (64-bit)
3. 고급 설정:
   - 호스트명: `pcb-pi-left` 또는 `pcb-pi-right`
   - SSH 활성화
   - Wi-Fi 설정
4. SD 카드에 설치 후 부팅

### 2. 초기 설정

```bash
# 1. SSH 접속
ssh pi@pcb-pi-left.local  # 또는 IP 주소

# 2. 시스템 업데이트
sudo apt update && sudo apt upgrade -y

# 3. Python 버전 확인
python3 --version  # Python 3.9 이상

# 4. pip 설치
sudo apt install python3-pip -y
```

### 3. 프로젝트 클론 및 패키지 설치

```bash
# 1. 프로젝트 클론
git clone https://github.com/ArianSung/PCB_Detect_Project.git
cd PCB_Detect_Project

# 2. 라즈베리파이용 패키지 설치
pip3 install opencv-python requests RPi.GPIO python-dotenv

# 3. 웹캠 접근 권한 설정
sudo usermod -a -G video pi
```

### 4. 웹캠 테스트

```bash
# 1. 웹캠 장치 확인
ls /dev/video*

# 2. 웹캠 캡처 테스트
python3 << EOF
import cv2
cap = cv2.VideoCapture(0)
ret, frame = cap.read()
if ret:
    print(f"✓ 웹캠 OK: {frame.shape}")
else:
    print("✗ 웹캠 Error")
cap.release()
EOF
```

### 5. GPIO 테스트 (라즈베리파이 1만 해당)

```bash
# ⚠️ 주의: 실제 릴레이 연결 전에는 LED로 먼저 테스트

python3 << EOF
import RPi.GPIO as GPIO
import time

GPIO.setmode(GPIO.BCM)
GPIO.setup(17, GPIO.OUT)  # 부품 불량 핀

# LED 또는 릴레이 테스트
GPIO.output(17, GPIO.HIGH)
time.sleep(1)
GPIO.output(17, GPIO.LOW)

GPIO.cleanup()
print("✓ GPIO 테스트 완료")
EOF
```

### 6. 환경 변수 설정

```bash
# 1. 환경 설정 스크립트 실행
bash scripts/setup_env.sh

# 2. .env 파일 수정
nano raspberry_pi/.env

# 3. 최소한 다음 항목 변경:
# - CAMERA_ID: left 또는 right
# - SERVER_URL: Flask 서버 IP
# - GPIO_ENABLED: true (라즈베리파이 1) 또는 false (라즈베리파이 2)
```

### 7. 카메라 클라이언트 실행 테스트

```bash
# Mock 서버 먼저 실행 (Flask 서버가 없는 경우)
# (Flask PC에서) python tests/api/mock_server.py

# 카메라 클라이언트 실행
python3 raspberry_pi/camera_client.py
```

---

## 🖥️ C# 앱 팀 환경 설정

### 1. Visual Studio 2022 설치

1. **다운로드**: https://visualstudio.microsoft.com/vs/community/
2. 워크로드 선택:
   - .NET 데스크톱 개발
   - .NET Core 크로스 플랫폼 개발
3. 설치 완료 후 재부팅

### 2. .NET SDK 설치

```powershell
# PowerShell에서 .NET SDK 버전 확인
dotnet --version

# 예상 출력: 6.0.x 이상

# 설치되지 않았다면:
# https://dotnet.microsoft.com/download/dotnet/6.0
```

### 3. MySQL Connector 설치

```powershell
# NuGet Package Manager Console에서 실행
Install-Package MySql.Data -Version 8.0.32
Install-Package Newtonsoft.Json -Version 13.0.3
Install-Package LiveCharts.WinForms -Version 0.9.7
Install-Package EPPlus -Version 5.8.14
```

또는 `csharp_winforms/PCB_Inspection_Monitor/PCB_Inspection_Monitor.csproj`에 추가:

```xml
<ItemGroup>
  <PackageReference Include="MySql.Data" Version="8.0.32" />
  <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
  <PackageReference Include="LiveCharts.WinForms" Version="0.9.7" />
  <PackageReference Include="EPPlus" Version="5.8.14" />
</ItemGroup>
```

### 4. 프로젝트 빌드 및 실행

```bash
# 1. 프로젝트 디렉토리로 이동
cd csharp_winforms/PCB_Inspection_Monitor

# 2. NuGet 패키지 복원
dotnet restore

# 3. 빌드
dotnet build

# 4. 실행
dotnet run
```

### 5. 데이터베이스 연결 설정

`App.config` 또는 `appsettings.json`에 다음 추가:

```xml
<connectionStrings>
  <add name="PCBDatabase"
       connectionString="Server=localhost;Port=3306;Database=pcb_inspection;Uid=root;Pwd=your_password;"
       providerName="MySql.Data.MySqlClient" />
</connectionStrings>
```

### 6. Flask API 연결 테스트

C# 코드에서 테스트:

```csharp
using System;
using System.Net.Http;
using System.Threading.Tasks;

public class ApiTest
{
    public static async Task Main()
    {
        using var client = new HttpClient();
        var response = await client.GetAsync("http://localhost:5000/api/v1/health");
        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine(content);
    }
}
```

---

## 🔗 네트워크 설정

### 로컬 네트워크 (선택)

**모든 장비가 같은 네트워크에 있는 경우**

1. **고정 IP 설정 권장**:
   - Flask 서버: `192.168.0.10`
   - 라즈베리파이 1: `192.168.0.20`
   - 라즈베리파이 2: `192.168.0.21`
   - Windows PC: `192.168.0.30`

2. **방화벽 설정**:
   ```bash
   # Flask 서버 (Ubuntu)
   sudo ufw allow 5000/tcp
   sudo ufw allow 3306/tcp  # MySQL
   ```

### 원격 네트워크 (Tailscale VPN) ⭐ 권장

**GPU PC가 원격지에 있는 경우 (프로젝트 환경)**

1. **Tailscale 설치** (모든 장비):
   ```bash
   # Ubuntu/Raspberry Pi
   curl -fsSL https://tailscale.com/install.sh | sh
   sudo tailscale up

   # Windows
   # https://tailscale.com/download/windows 에서 다운로드 및 설치
   ```

2. **Tailscale IP 확인**:
   ```bash
   tailscale ip -4
   # 예: 100.x.x.x
   ```

3. **.env 파일에 Tailscale IP 설정**:
   ```bash
   # src/server/.env
   SERVER_URL=http://100.x.x.x:5000

   # raspberry_pi/.env
   SERVER_URL=http://100.x.x.x:5000

   # csharp_winforms/.env
   API_BASE_URL=http://100.x.x.x:5000
   ```

---

## ✅ 설정 완료 확인 체크리스트

### Flask 서버 팀
- [ ] Conda 가상환경 생성 및 활성화 완료
- [ ] PyTorch + CUDA 설치 확인 (GPU 사용 가능)
- [ ] MySQL 데이터베이스 생성 완료
- [ ] Flask 서버 실행 확인 (`curl http://localhost:5000/api/v1/health`)
- [ ] .env 파일 설정 완료

### AI 모델 팀
- [ ] Flask 서버 팀과 동일한 환경 구성 완료
- [ ] 데이터셋 다운로드 및 준비 완료
- [ ] YOLO 모델 학습 테스트 성공

### 라즈베리파이 팀
- [ ] Raspberry Pi OS 설치 및 SSH 접속 완료
- [ ] 웹캠 캡처 테스트 성공
- [ ] GPIO 테스트 성공 (라즈베리파이 1)
- [ ] Flask 서버 연결 테스트 성공
- [ ] .env 파일 설정 완료

### C# 앱 팀
- [ ] Visual Studio 2022 설치 완료
- [ ] .NET 6.0 SDK 설치 확인
- [ ] NuGet 패키지 복원 완료
- [ ] 프로젝트 빌드 성공
- [ ] Flask API 연결 테스트 성공
- [ ] MySQL 데이터베이스 연결 성공

---

## 🚨 문제 해결

### CUDA 관련 오류

```bash
# CUDA 드라이버 버전 확인
nvidia-smi

# PyTorch CUDA 재설치
pip uninstall torch torchvision torchaudio
pip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cu118
```

### MySQL 연결 오류

```bash
# MySQL 상태 확인
sudo systemctl status mysql

# MySQL 재시작
sudo systemctl restart mysql

# 포트 확인
sudo netstat -tlnp | grep 3306
```

### 웹캠 인식 안 됨 (라즈베리파이)

```bash
# 웹캠 장치 확인
ls -l /dev/video*

# 권한 확인
groups pi  # video 그룹 포함 확인

# 웹캠 재연결 후 재시도
```

---

**마지막 업데이트**: 2025-10-25
**문서 관리**: 팀 리더
