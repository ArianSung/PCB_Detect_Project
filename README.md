# 🔍 PCB 불량 검사 시스템 (PCB Defect Detection System)

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Python 3.8+](https://img.shields.io/badge/python-3.8+-blue.svg)](https://www.python.org/downloads/)
[![YOLOv8](https://img.shields.io/badge/YOLO-v8-00FFFF.svg)](https://github.com/ultralytics/ultralytics)
[![Flask](https://img.shields.io/badge/Flask-2.0+-000000.svg)](https://flask.palletsprojects.com/)

> 컨베이어 벨트를 통해 들어오는 PCB의 양면을 실시간으로 검사하여 불량을 자동 검출하고 분류하는 하이브리드 딥러닝 시스템

## 📋 목차

- [프로젝트 개요](#-프로젝트-개요)
- [주요 기능](#-주요-기능)
- [시스템 구성](#-시스템-구성)
- [기술 스택](#-기술-스택)
- [설치 방법](#-설치-방법)
- [사용법](#-사용법)
- [프로젝트 구조](#-프로젝트-구조)
- [성능](#-성능)
- [문서](#-문서)
- [라이선스](#-라이선스)

## 🎯 프로젝트 개요

본 프로젝트는 **YOLO v8 객체 탐지**와 **이상 탐지(Anomaly Detection)** 기술을 결합한 하이브리드 딥러닝 시스템으로, PCB(인쇄회로기판) 제조 공정에서 발생하는 다양한 불량을 실시간으로 검출하고 자동 분류합니다.

### 불량 검출 대상

- 🔧 **부품 불량**: Missing Component, Misalignment, Wrong Component
- 🔩 **납땜 불량**: Cold Joint, Solder Bridge, Insufficient Solder
- 💥 **PCB 손상**: Damaged Pad, Trace Damage, Scratches

## ✨ 주요 기능

- ✅ **양면 동시 검사**: 2대의 웹캠으로 PCB 좌우면 동시 촬영
- 🤖 **AI 하이브리드 모델**: YOLO v8 + 이상 탐지 결과 융합
- ⚡ **실시간 처리**: 100-200ms 추론 속도 (원격 연결)
- 🌐 **원격 GPU 추론**: Tailscale VPN을 통한 원격 서버 연결
- 🎛️ **자동 분류**: GPIO 제어를 통한 불량 유형별 자동 분류
- 📊 **모니터링 대시보드**: C# WinForms 실시간 모니터링 및 통계

## 🏗️ 시스템 구성

```
┌─────────────────────────────────────────────────────────┐
│                  컨베이어 벨트 시스템                     │
│                                                           │
│  📷 웹캠(좌측)          📷 웹캠(우측)                      │
│      ↓                      ↓                            │
│  [라즈베리파이 1]      [라즈베리파이 2]                   │
│   + GPIO 제어          (카메라 전용)                      │
└──────────┬─────────────────┬───────────────────────────┘
           │                 │
           │  Tailscale VPN  │
           │   (100.x.x.x)   │
           ↓                 ↓
    ┌─────────────────────────────────┐
    │   GPU PC (원격지 - 같은 도시)    │
    │  ┌───────────────────────────┐  │
    │  │   Flask 추론 서버         │  │
    │  │   • YOLO v8 추론          │  │
    │  │   • 이상 탐지             │  │
    │  │   • 결과 융합 및 분류     │  │
    │  └───────────────────────────┘  │
    │  ┌───────────────────────────┐  │
    │  │   MySQL 데이터베이스      │  │
    │  └───────────────────────────┘  │
    └──────────────┬──────────────────┘
                   │
                   │ REST API
                   ↓
    ┌─────────────────────────────────┐
    │  C# WinForms 모니터링 앱        │
    │  • 실시간 대시보드              │
    │  • 검사 이력 조회               │
    │  • 통계 및 분석                 │
    └─────────────────────────────────┘
```

## 🛠️ 기술 스택

### AI & Deep Learning
- **PyTorch** - 딥러닝 프레임워크
- **YOLO v8** (Ultralytics) - 객체 탐지
- **Anomalib** - 이상 탐지 (PaDiM)
- **OpenCV** - 컴퓨터 비전

### Backend & Server
- **Flask** - 추론 서버 및 REST API
- **MySQL 8.0** - 데이터베이스
- **Tailscale VPN** - 원격 네트워크

### Hardware & Embedded
- **Raspberry Pi 4** (2대) - 웹캠 클라이언트
- **RPi.GPIO** - GPIO 제어
- **NVIDIA RTX 4080 Super** - GPU 추론

### Frontend & Monitoring
- **C# WinForms** (.NET 6+) - 모니터링 앱
- **LiveCharts** - 실시간 차트

## 📦 설치 방법

### 1. GPU PC (추론 서버)

```bash
# 저장소 클론
git clone https://github.com/ArianSung/PCB_Detect_Project.git
cd PCB_Detect_Project

# Conda 가상환경 생성
conda create -n pcb_defect python=3.9
conda activate pcb_defect

# 필수 패키지 설치
pip install torch torchvision --index-url https://download.pytorch.org/whl/cu118
pip install ultralytics opencv-python flask flask-cors pymysql

# Flask 서버 실행
cd src/server
python app.py
```

### 2. 라즈베리파이 (웹캠 클라이언트)

```bash
# 저장소 클론
git clone https://github.com/ArianSung/PCB_Detect_Project.git
cd PCB_Detect_Project

# 필수 패키지 설치
pip3 install opencv-python requests RPi.GPIO

# 카메라 클라이언트 실행
python3 raspberry_pi/camera_client.py left 0 http://100.x.x.x:5000 10
```

### 3. Windows PC (모니터링 앱)

Visual Studio 2022에서 `csharp_winforms/PCB_Inspection_Monitor.sln` 열기 후 빌드

## 🚀 사용법

### Flask 서버 시작

```bash
conda activate pcb_defect
cd src/server
python app.py
```

### 라즈베리파이 클라이언트 실행

```bash
# 좌측 카메라 (GPIO 제어 포함)
python3 camera_client.py left 0 http://100.x.x.x:5000 10

# 우측 카메라 (카메라 전용)
python3 camera_client.py right 0 http://100.x.x.x:5000 10
```

### 모니터링 앱 실행

Visual Studio에서 빌드 후 실행 또는:
```bash
cd csharp_winforms/PCB_Inspection_Monitor/bin/Release/net6.0-windows/
./PCB_Inspection_Monitor.exe
```

## 📁 프로젝트 구조

```
PCB_Detect_Project/
│
├── docs/                    # 📚 프로젝트 문서
│   ├── PCB_Defect_Detection_Project.md
│   ├── Flask_Server_Setup.md
│   ├── RaspberryPi_Setup.md
│   └── ...
│
├── src/                     # 💻 소스 코드
│   ├── server/              # Flask 추론 서버
│   ├── models/              # AI 모델 정의
│   └── training/            # 모델 학습 스크립트
│
├── raspberry_pi/            # 🍓 라즈베리파이 클라이언트
│   └── camera_client.py
│
├── csharp_winforms/         # 🖥️ C# 모니터링 앱
│
├── data/                    # 📊 데이터셋
│   ├── raw/
│   └── processed/
│
├── models/                  # 🤖 학습된 모델
│   ├── yolo/
│   └── anomaly/
│
├── database/                # 🗄️ MySQL 스키마
│
└── configs/                 # ⚙️ 설정 파일
```

## 📊 성능

### 하드웨어 사양
- **GPU**: NVIDIA RTX 4080 Super (16GB VRAM)
- **AI 모델**: YOLOv8l (Large) + PaDiM

### 추론 성능
- **추론 시간**: 100-200ms (원격 VPN 포함)
- **목표 달성**: < 300ms ✅
- **FPS**: 5-10 FPS (양면 동시 처리)
- **정확도 목표**: mAP@0.5 > 0.85

### 네트워크
- **연결 방식**: Tailscale VPN (원격)
- **지연시간**: 20-50ms (같은 도시 내)
- **대역폭**: ~1 MB/s (2대 동시)

## 📖 문서

자세한 문서는 [`docs/`](docs/) 폴더를 참조하세요:

- [📘 전체 프로젝트 가이드](docs/PCB_Defect_Detection_Project.md)
- [🌐 Flask 서버 구축](docs/Flask_Server_Setup.md)
- [🍓 라즈베리파이 설정](docs/RaspberryPi_Setup.md)
- [🖥️ C# WinForms 가이드](docs/CSharp_WinForms_Guide.md)
- [🗄️ MySQL 데이터베이스 설계](docs/MySQL_Database_Design.md)
- [🌍 원격 네트워크 설정](docs/Remote_Network_Setup.md)

## 👨‍💻 개발자

**ArianSung**
- GitHub: [@ArianSung](https://github.com/ArianSung)
- Email: sys1041@naver.com

## 📄 라이선스

이 프로젝트는 MIT 라이선스 하에 배포됩니다. 자세한 내용은 [LICENSE](LICENSE) 파일을 참조하세요.

---

<div align="center">
  <strong>🎓 졸업 프로젝트 - PCB 불량 검사 시스템</strong>
  <br>
  Made with ❤️ by ArianSung
</div>
