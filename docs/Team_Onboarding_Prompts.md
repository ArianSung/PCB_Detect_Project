# 팀원 AI 온보딩 프롬프트 모음집

> PCB 불량 검사 시스템 프로젝트에 참여하는 팀원들이 AI(Claude, ChatGPT 등)에게 물어볼 때 사용할 수 있는 프롬프트 모음입니다.
> **복사해서 그대로 붙여넣기만 하면 됩니다!**

---

## 📋 목차

1. [프로젝트 전체 이해하기 (최초 1회)](#1-프로젝트-전체-이해하기-최초-1회)
2. [개발 환경 초기 설정 (최초 1회)](#2-개발-환경-초기-설정-최초-1회)
3. [팀별 시작 프롬프트](#3-팀별-시작-프롬프트)
   - [Flask 서버 팀](#31-flask-서버-팀)
   - [AI 모델 팀](#32-ai-모델-팀)
   - [라즈베리파이 팀](#33-라즈베리파이-팀)
   - [Arduino 디팔렛타이저 팀](#34-arduino-디팔렛타이저-팀)
   - [C# 앱 팀](#35-c-앱-팀)
4. [일일 작업 시작 프롬프트](#4-일일-작업-시작-프롬프트)
5. [Pull Request 생성 프롬프트](#5-pull-request-생성-프롬프트)
6. [문제 해결 프롬프트](#6-문제-해결-프롬프트)

---

## 1. 프로젝트 전체 이해하기 (최초 1회)

### 🤖 프롬프트: 프로젝트 구조 및 시스템 이해

**사용 시기**: 프로젝트에 처음 참여할 때

```
안녕! 나는 PCB 불량 검사 시스템 졸업 프로젝트에 참여하게 된 팀원이야.

**프로젝트 정보:**
- GitHub 저장소: https://github.com/ArianSung/PCB_Detect_Project
- 프로젝트 목표: 컨베이어 벨트를 통해 들어오는 PCB의 양면을 실시간으로 검사하여 불량을 자동 검출하고 분류하는 시스템

**읽어야 할 문서 (저장소 내 경로):**
1. `README.md` - 프로젝트 개요 및 전체 구조
2. `docs/PCB_Defect_Detection_Project.md` - 전체 로드맵 및 시스템 아키텍처
3. `PROJECT_STRUCTURE.md` - 프로젝트 폴더 구조 및 각 디렉토리 역할
4. `docs/Team_Collaboration_Guide.md` - 팀 협업 가이드 (팀 구성, 역할, 워크플로우)

**시스템 구성:**
- 추론 서버: GPU PC (원격지) - Flask + YOLO v8 + MySQL
- 웹캠 클라이언트: 라즈베리파이 4 (2대) - 좌/우 카메라
- **디팔렛타이저**: Arduino Mega 2560 + 5-6축 로봇팔 - PCB 픽업 및 박스 분류 (2.5초/PCB)
- OHT 제어: 라즈베리파이 4 (1대) - 가득 찬 박스 운반 (GPIO 제어)
- 박스 시스템: 3개 박스 × 5개 슬롯 (정상/부품불량/납땜불량)
- 모니터링 앱: Windows PC - C# WinForms (박스 상태, OHT 제어, 사용자 관리)
- 네트워크: Tailscale VPN (100.x.x.x)

위 문서들을 읽고, 다음 질문에 답변해줘:
1. 이 시스템의 전체 데이터 흐름은 어떻게 되는가? (라즈베리파이 → Flask → Arduino 디팔렛타이저 → 박스 → OHT)
2. 각 팀(Flask, AI 모델, 라즈베리파이, Arduino, C# 앱)의 주요 책임은 무엇인가?
3. 프로젝트의 주요 디렉토리(`server/`, `yolo/`, `database/`, `raspberry_pi/`, `arduino/`, `csharp_winforms/`)는 각각 어떤 역할을 하는가?
4. **디팔렛타이저(Arduino 로봇팔)의 역할과 2.5초 동작 시간 제약**은 무엇인가?
5. 팀 간 협업 시 주의해야 할 점은 무엇인가? (API 변경, Git 충돌 등)
```

---

## 2. 개발 환경 초기 설정 (최초 1회)

### 🤖 프롬프트: Git 저장소 클론 및 환경 설정

**사용 시기**: 로컬 개발 환경을 처음 구성할 때

```
안녕! PCB 불량 검사 시스템 프로젝트의 로컬 개발 환경을 설정하려고 해.

**시스템 정보:**
- OS: [Ubuntu 22.04 / Windows 11 / Raspberry Pi OS 중 선택]
- 내 팀: [Flask 서버 / AI 모델 / 라즈베리파이 / C# 앱 중 선택]
- Git 설치 여부: [예 / 아니오]

**읽어야 할 문서:**
1. `docs/Development_Setup.md` - 팀별 로컬 환경 구성 가이드
2. `docs/Git_Workflow.md` - Git 브랜치 전략 및 협업 규칙

**진행 단계:**
1. Git 저장소 클론:
   ```bash
   git clone https://github.com/ArianSung/PCB_Detect_Project.git
   cd PCB_Detect_Project
   ```

2. 브랜치 확인 및 전환:
   - `develop` 브랜치로 전환
   - 내 팀 브랜치 (`feature/flask-server`, `feature/ai-model`, `feature/raspberry-pi`, `feature/csharp-app`) 확인

3. 환경 설정 스크립트 실행:
   ```bash
   bash scripts/setup_env.sh
   ```

4. `.env` 파일 설정:
   - 내 팀에 해당하는 `.env.example` 파일 확인 (예: `server/.env.example`)
   - 실제 `.env` 파일 생성 및 수정 (Tailscale IP, DB 비밀번호 등)

위 단계를 순서대로 진행하는데, 내 OS와 팀에 맞는 구체적인 명령어와 설정 방법을 알려줘.
특히 환경 변수(`.env` 파일)는 어떤 값들을 설정해야 하는지 상세히 설명해줘.
```

---

## 3. 팀별 시작 프롬프트

### 3.1. Flask 서버 팀

**사용 시기**: Flask 서버 개발을 시작할 때

```
안녕! 나는 PCB 불량 검사 시스템의 Flask 서버 팀원이야.

**내 역할:**
- Flask 웹서버 개발 (REST API)
- AI 모델 (YOLO, 이상 탐지) 로드 및 추론 실행
- MySQL 데이터베이스 연동 (검사 이력 저장)
- 라즈베리파이 및 C# 앱과의 API 통신

**읽어야 할 핵심 문서:**
1. `docs/Flask_Server_Setup.md` - Flask 서버 구축 가이드 (필수!)
2. `docs/API_Contract.md` - 공식 API 명세서 (팀 전체 계약, 박스 상태 API 포함)
3. `docs/MySQL_Database_Design.md` - MySQL 데이터베이스 설계 (11개 테이블)
4. `database/schema.sql` - 데이터베이스 스키마 (box_status, oht_operations, user_logs 포함)
5. `database/create_users.sql` - 데이터베이스 사용자 생성 스크립트 (5개 계정)
6. `server/.env.example` - 환경 변수 템플릿

**개발 환경:**
- OS: Ubuntu 22.04 (GPU PC)
- GPU: NVIDIA RTX 4080 Super
- Python: 3.10 (Conda 가상환경 `pcb_defect`)
- 데이터베이스: MySQL 8.0 (Windows PC - Tailscale 100.x.x.x:3306)
- DB 계정: `pcb_server` / 비밀번호: `1234` (읽기/쓰기/업데이트 권한)
- DB 테이블: 11개 (inspections, defect_images, statistics_daily/hourly, system_logs, system_config, users, alerts, box_status, oht_operations, user_logs)

**환경 변수 설정 (server/.env):**
```
DB_HOST=100.x.x.x          # Windows PC의 Tailscale IP
DB_PORT=3306
DB_NAME=pcb_inspection
DB_USER=pcb_server
DB_PASSWORD=1234

SERVER_HOST=0.0.0.0
SERVER_PORT=5000
GPU_DEVICE=cuda:0
```

**첫 번째 작업:**
1. Conda 가상환경 활성화: `conda activate pcb_defect`
2. Flask 서버 실행: `cd server && python app.py`
3. 서버 상태 확인: `curl http://localhost:5000/health`
4. Mock 클라이언트로 API 테스트: `python tests/api/mock_client.py` (생성 필요)

위 정보를 바탕으로, Flask 서버를 처음 실행하고 테스트하는 과정을 단계별로 안내해줘.
특히 MySQL 원격 연결이 제대로 되는지 확인하는 방법도 알려줘.
```

### 3.2. AI 모델 팀

**사용 시기**: YOLO 모델 학습 및 평가를 시작할 때

```
안녕! 나는 PCB 불량 검사 시스템의 AI 모델 팀원이야.

**내 역할:**
- YOLOv8 모델 학습 및 최적화
- 이상 탐지 모델 구현 (PaDiM)
- 모델 성능 평가 (mAP, FPS, 정확도)
- 학습된 모델 Flask 팀에 전달

**읽어야 할 핵심 문서:**
1. `docs/Phase1_YOLO_Setup.md` - YOLO 환경 구축 및 Phase 1 가이드
2. `docs/Dataset_Guide.md` - 데이터셋 준비 및 전처리
3. `yolo/README.md` - YOLO 작업 디렉토리 가이드
4. `yolo/tests/README.md` - Phase 1 테스트 가이드
5. `yolo/tests/YOLO11_vs_YOLOv8.md` - YOLO 버전 비교

**개발 환경:**
- OS: Ubuntu 22.04 (GPU PC)
- GPU: NVIDIA RTX 4080 Super (16GB VRAM)
- Python: 3.10 (Conda 가상환경 `pcb_defect`)
- YOLO 버전: YOLOv8l (Large 모델 권장)

**작업 디렉토리:**
```
yolo/
├── datasets/      # YOLO 데이터셋 (COCO128, 향후 PCB 데이터셋)
├── runs/          # YOLO 학습 결과 (Git 무시)
├── test_images/   # 테스트 이미지
└── tests/         # Phase 1 테스트 스크립트
```

**첫 번째 작업 (Phase 1 완료 확인):**
1. Conda 가상환경 활성화: `conda activate pcb_defect`
2. GPU 확인: `python -c "import torch; print(torch.cuda.is_available())"`
3. Phase 1 기본 테스트 실행:
   ```bash
   cd yolo/tests
   python test_yolo_basic.py
   python test_yolo_coco128.py
   ```
4. 결과 확인: `yolo/tests/PHASE1_TEST_RESULTS.md` 참고

위 정보를 바탕으로, YOLO 모델을 처음 테스트하고 학습을 시작하는 과정을 안내해줘.
특히 GPU가 제대로 인식되는지, 그리고 Phase 1 테스트가 성공하는지 확인하는 방법을 알려줘.
```

### 3.3. 라즈베리파이 팀

**사용 시기**: 라즈베리파이 웹캠 클라이언트 또는 OHT 제어 시스템 개발을 시작할 때

```
안녕! 나는 PCB 불량 검사 시스템의 라즈베리파이 팀원이야.

**라즈베리파이 구성 (총 3대):**
- **라즈베리파이 1 (좌측 웹캠)**: 웹캠 프레임 캡처 + Flask API 호출
- **라즈베리파이 2 (우측 웹캠)**: 웹캠 프레임 캡처 + Flask API 호출
- **라즈베리파이 3 (OHT 제어)**: GPIO 제어 (릴레이 모듈 → 분류 게이트) + Arduino 로봇팔 통신

**공통 역할 (웹캠 전용, 라즈베리파이 1·2):**
- 웹캠에서 PCB 프레임 캡처 (OpenCV)
- JPEG 인코딩 및 Base64 변환
- Flask API 호출 (`/predict`)

**OHT 제어 전용 (라즈베리파이 3):**
- GPIO 핀 제어 (릴레이 모듈 → 분류 게이트)
- Arduino 로봇팔과 시리얼 통신
- Flask API 호출 (`/box_status` 조회, OHT 동작 트리거)

**읽어야 할 핵심 문서:**
1. `docs/RaspberryPi_Setup.md` - 라즈베리파이 환경 설정 및 클라이언트 가이드
2. `docs/RaspberryPi_OHT_Setup.md` - OHT 제어 시스템 가이드 (라즈베리파이 3 전용)
3. `docs/API_Contract.md` - Flask API 명세서 (박스 상태 API 포함)
4. `raspberry_pi/.env.example` - 환경 변수 템플릿
5. `tests/api/mock_server.py` - Mock Flask 서버 (독립 개발용)

**개발 환경:**
- 하드웨어: Raspberry Pi 4 Model B (4GB) × 3대
- OS: Raspberry Pi OS (64-bit)
- 웹캠: USB 웹캠 (640x480) - 라즈베리파이 1·2만
- 릴레이: 4채널 릴레이 모듈 (라즈베리파이 3만 해당)
- Arduino: Arduino Uno/Mega (로봇팔 제어, 라즈베리파이 3와 시리얼 통신)
- 네트워크: Tailscale VPN (100.x.x.x)

**환경 변수 설정 (raspberry_pi/.env):**

**웹캠 전용 (라즈베리파이 1·2):**
```
CAMERA_ID=left             # 또는 right (라즈베리파이 2는 right)
CAMERA_INDEX=0
SERVER_URL=http://100.x.x.x:5000
FPS=10
JPEG_QUALITY=85
GPIO_ENABLED=false         # 웹캠 전용은 GPIO 비활성화
```

**OHT 제어 전용 (라즈베리파이 3):**
```
CAMERA_ID=oht              # OHT 제어 전용
SERVER_URL=http://100.x.x.x:5000
GPIO_ENABLED=true          # GPIO 활성화
ARDUINO_PORT=/dev/ttyACM0  # Arduino 시리얼 포트
ARDUINO_BAUD=9600          # 시리얼 통신 속도
```

**GPIO 핀 매핑 (BCM 모드, 라즈베리파이 3 전용):**
- GPIO 17: 부품 불량 게이트
- GPIO 27: 납땜 불량 게이트
- GPIO 22: 폐기 게이트
- GPIO 23: 정상 게이트

**첫 번째 작업:**

**웹캠 전용 (라즈베리파이 1·2):**
1. 웹캠 테스트:
   ```python
   python3 -c "import cv2; cap = cv2.VideoCapture(0); print('OK' if cap.isOpened() else 'Error')"
   ```
2. Flask API 연결 테스트:
   ```bash
   curl http://100.x.x.x:5000/health
   ```
3. Mock 서버로 테스트 (Flask 서버 없을 때):
   - GPU PC에서 `python tests/api/mock_server.py` 실행
   - 라즈베리파이에서 `python3 raspberry_pi/camera_client.py left` 또는 `right` 실행

**OHT 제어 전용 (라즈베리파이 3):**
1. GPIO 테스트:
   ```python
   python3 -c "import RPi.GPIO as GPIO; GPIO.setmode(GPIO.BCM); print('GPIO OK')"
   ```
2. Arduino 시리얼 통신 테스트:
   ```python
   python3 -c "import serial; s = serial.Serial('/dev/ttyACM0', 9600); print('Arduino OK')"
   ```
3. Flask API 박스 상태 조회 테스트:
   ```bash
   curl http://100.x.x.x:5000/box_status
   ```

위 정보를 바탕으로, 라즈베리파이에서 웹캠/GPIO/Arduino를 테스트하고 Flask API와 통신하는 과정을 안내해줘.
특히 웹캠 전용과 OHT 제어 전용의 차이점을 명확히 설명해줘.
```

### 3.4. Arduino 디팔렛타이저 팀

**사용 시기**: Arduino 로봇팔 제어 시스템 개발을 시작할 때

```
안녕! 나는 PCB 불량 검사 시스템의 Arduino 디팔렛타이저 팀원이야.

**내 역할:**
- Arduino Mega 2560 기반 5-6축 로봇팔 제어
- 컨베이어 벨트에서 PCB 픽업
- 불량 유형에 따라 박스 슬롯에 자동 배치
- 라즈베리파이 1과 USB 시리얼 통신
- **디팔렛타이저 역할**: PCB를 픽업하여 분류하는 핵심 하드웨어 (2.5초/PCB 목표)

**시스템 구성:**
- **Arduino Mega 2560**: 마이크로컨트롤러 (54개 디지털 I/O 핀)
- **서보 모터 6개**: MG996R 또는 DS3218 (Base, Shoulder, Elbow, Wrist Pitch, Wrist Roll, Gripper)
- **레일 시스템**: NEMA 17 스텝모터 + A4988 드라이버 (로봇팔 플랫폼 좌우 이동)
- **박스 시스템**: 3개 박스 × 5개 슬롯 = 15개 슬롯 + DISCARD 위치
- **통신**: USB 시리얼 (115200 baud) ↔ 라즈베리파이 1

**읽어야 할 핵심 문서:**
1. `docs/Arduino_RobotArm_Setup.md` - Arduino 로봇팔 전체 설정 가이드 (필수!)
2. `docs/API_Contract.md` - 박스 상태 API 명세서 (박스 ID, 슬롯 번호)
3. `arduino/robot_arm_controller/config.h` - 서보 핀 정의 및 좌표 테이블
4. `arduino/robot_arm_controller/robot_arm_controller.ino` - 메인 스케치 파일

**개발 환경:**
- **하드웨어**: Arduino Mega 2560
- **IDE**: Arduino IDE 2.3.0 이상
- **라이브러리**:
  - Servo (기본 포함)
  - ArduinoJson 6.21.0 (JSON 명령 파싱)
- **전원**: USB 5V (라즈베리파이) + 별도 서보 전원 (5V 10A)
- **시리얼 포트**: /dev/ttyACM0 (Linux) 또는 COM3 (Windows)

**서보 모터 연결 (PWM 핀):**
```
Base (베이스)        → 핀 2 (회전 0-180°)
Shoulder (어깨)      → 핀 3 (상하 0-180°)
Elbow (팔꿈치)       → 핀 4 (굽히기 0-180°)
Wrist Pitch (손목)   → 핀 5 (피치 0-180°)
Wrist Roll (손목)    → 핀 6 (롤 0-180°)
Gripper (그리퍼)     → 핀 7 (열기/닫기 0-90°)
```

**15개 슬롯 좌표 테이블:**
```cpp
// NORMAL 박스 (슬롯 0-4, 수평 배치)
const Coordinate NORMAL_SLOTS[5] = {
  {88, 115, 85, 90, 90},  // 슬롯 0 (좌측 끝)
  {92, 115, 84, 90, 90},  // 슬롯 1
  {96, 115, 83, 90, 90},  // 슬롯 2 (중앙)
  {100, 115, 82, 90, 90}, // 슬롯 3
  {104, 115, 81, 90, 90}  // 슬롯 4 (우측 끝)
};

// COMPONENT_DEFECT 박스, SOLDER_DEFECT 박스도 동일 구조
// DISCARD 위치: 고정 좌표 {90, 85, 70, 90, 90}
```

**JSON 시리얼 통신 프로토콜:**

**라즈베리파이 → Arduino (PCB 배치 명령):**
```json
{
  "command": "place_pcb",
  "box_id": "NORMAL_A",
  "slot_number": 2,
  "coordinates": {"x": 240.0, "y": 100.0, "z": 30.0}
}
```

**Arduino → 라즈베리파이 (성공 응답):**
```json
{
  "status": "success",
  "message": "PCB placed successfully",
  "execution_time_ms": 2350
}
```

**성능 목표:**
- **배치 시간**: < 2.5초/PCB (픽업 + 이동 + 배치)
- **정확도**: ±2mm 이내 (슬롯 중심 기준)
- **연속 작동**: 8시간 무중단 운영

**첫 번째 작업:**
1. Arduino IDE 설치 및 보드 선택:
   ```
   Tools → Board → Arduino Mega or Mega 2560
   Tools → Port → /dev/ttyACM0
   ```

2. ArduinoJson 라이브러리 설치:
   ```
   Tools → Manage Libraries → "ArduinoJson" 검색 → 6.21.0 이상 설치
   ```

3. 서보 단독 테스트:
   ```bash
   # arduino/test_sketches/test_servo_sweep.ino 업로드
   # 각 서보가 0-180도 스윕하는지 확인
   ```

4. 시리얼 통신 테스트:
   ```bash
   # arduino/test_sketches/test_serial_communication.ino 업로드
   # 라즈베리파이에서 JSON 명령 전송 및 응답 확인
   ```

5. 좌표 캘리브레이션:
   ```
   # 각 박스 슬롯 위치로 수동 이동
   # 서보 각도 측정 및 config.h에 저장
   # 정확도 ±2mm 이내 확인
   ```

위 정보를 바탕으로, Arduino 로봇팔 시스템을 처음 설정하고 테스트하는 과정을 안내해줘.
특히 서보 모터 캘리브레이션과 시리얼 통신 테스트 방법을 상세히 설명해줘.
또한, 디팔렛타이저가 2.5초 이내에 PCB를 배치해야 하는 이유(AI 추론 시간 고려)도 설명해줘.
```

### 3.5. C# 앱 팀

**사용 시기**: C# WinForms 모니터링 앱 개발을 시작할 때

```
안녕! 나는 PCB 불량 검사 시스템의 C# WinForms 모니터링 앱 팀원이야.

**내 역할:**
- WinForms UI 개발 (7개 화면)
- Flask REST API 호출하여 데이터 조회 (검사 이력, 박스 상태, OHT 운영 등)
- MySQL 데이터베이스 연결 (검사 이력, 통계, 사용자 관리, OHT 이력 조회)
- LiveCharts로 실시간 차트 표시 (검사 통계, 박스 사용률)
- Excel 내보내기 기능 (EPPlus)
- 권한 시스템 구현 (Admin/Operator/Viewer)
- 박스 상태 모니터링 및 OHT 호출 기능
- 사용자 활동 로그 조회 및 관리

**읽어야 할 핵심 문서:**
1. `docs/CSharp_WinForms_Guide.md` - C# WinForms 기본 개발 가이드
2. `docs/CSharp_WinForms_Design_Specification.md` - UI 설계 명세서 (7개 화면, 권한 시스템)
3. `docs/CSharp_WinForms_UI_Design_Guide.md` - UI 디자인 가이드 (박스 상태, OHT 제어 UI 포함)
4. `docs/API_Contract.md` - Flask API 명세서 (박스 상태 API, OHT API 포함)
5. `docs/MySQL_Database_Design.md` - MySQL 데이터베이스 설계 (11개 테이블)
6. `database/create_users.sql` - 데이터베이스 사용자 생성 스크립트
7. `csharp_winforms/.env.example` - 환경 변수 템플릿

**개발 환경:**
- OS: Windows 10 / 11
- IDE: Visual Studio 2022 Community
- .NET SDK: .NET 6.0
- 데이터베이스: MySQL 8.0 (Windows PC - localhost 또는 Tailscale 100.x.x.x:3306)
- DB 계정: `pcb_viewer` / 비밀번호: `1234` (읽기 전용 권한)
- DB 테이블: 11개 (inspections, defect_images, statistics_daily/hourly, system_logs, system_config, users, alerts, box_status, oht_operations, user_logs)

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

**7개 화면 (+박스/OHT 기능):**
1. 로그인 화면 (권한 시스템)
2. 메인 대시보드 (실시간 통계 + 박스 상태 요약)
3. 검사 이력 조회 (DataGridView + 페이지네이션)
4. 상세 결과 뷰어 (이미지 + 불량 정보)
5. 통계 및 차트 (LiveCharts - 검사 통계, 박스 사용률, OHT 운영 통계)
6. 박스 상태 모니터링 & OHT 제어 (3개 박스 슬롯 상태, 수동 OHT 호출, 운영 이력)
7. 설정 화면 (Admin 전용 - 사용자 관리, 시스템 설정, 박스 리셋)
8. Excel 내보내기 (검사 이력, 통계, OHT 운영 이력)

**첫 번째 작업:**
1. Visual Studio에서 프로젝트 열기
2. NuGet 패키지 복원: `dotnet restore`
3. 빌드: `dotnet build`
4. MySQL 연결 테스트 (C# 코드):
   ```csharp
   var conn = new MySqlConnection("Server=localhost;Port=3306;Database=pcb_inspection;Uid=pcb_viewer;Pwd=1234;");
   conn.Open();
   Console.WriteLine("MySQL 연결 성공!");
   ```
5. Flask API 호출 테스트 (health check):
   ```csharp
   var client = new HttpClient();
   var response = await client.GetAsync("http://100.x.x.x:5000/health");
   Console.WriteLine(await response.Content.ReadAsStringAsync());
   ```
6. Flask API 박스 상태 조회 테스트:
   ```csharp
   var response = await client.GetAsync("http://100.x.x.x:5000/box_status");
   var json = await response.Content.ReadAsStringAsync();
   var boxStatus = JsonConvert.DeserializeObject<BoxStatusResponse>(json);
   Console.WriteLine($"박스 개수: {boxStatus.Boxes.Count}");
   ```

위 정보를 바탕으로, C# WinForms 프로젝트를 처음 설정하고 MySQL 및 Flask API 연결을 테스트하는 과정을 안내해줘.
특히 8개 화면의 구조, 권한 시스템, 그리고 박스 상태/OHT 제어 기능을 어떻게 설계할지도 설명해줘.
```

---

## 4. 일일 작업 시작 프롬프트

### 🤖 프롬프트: 매일 오전 작업 시작 루틴

**사용 시기**: 매일 오전, 코딩 시작 전

```
안녕! 오늘 PCB 불량 검사 시스템 프로젝트 작업을 시작하려고 해.

**내 팀:** [Flask 서버 / AI 모델 / 라즈베리파이 / C# 앱]

**읽어야 할 문서:**
1. `docs/Git_Workflow.md` - Git 브랜치 전략 및 일일 워크플로우
2. `docs/Team_Collaboration_Guide.md` - 일일 작업 루틴

**오늘 작업 전 체크리스트:**
1. `develop` 브랜치 최신화:
   ```bash
   git checkout develop
   git pull origin develop
   ```

2. 내 브랜치로 전환 및 병합:
   ```bash
   git checkout feature/[팀명]
   git merge develop
   ```

3. 충돌 발생 시 해결:
   - 환경 변수 충돌 → `.env` 파일 사용 (Git에 올리지 않음)
   - API 변경 충돌 → `docs/API_Contract.md` 확인
   - 코드 충돌 → 수동 병합 후 커밋

4. 팀 채팅방에서 공지 확인:
   - API 변경 사항
   - 데이터베이스 스키마 변경
   - 긴급 버그 수정

**오늘 작업 계획:**
[오늘 할 작업을 구체적으로 적어주세요]

위 체크리스트를 순서대로 진행하는데, 특히 충돌이 발생했을 때 어떻게 해결해야 하는지 상세히 알려줘.
그리고 오늘 작업 계획을 바탕으로 어떤 순서로 작업하면 좋을지 추천해줘.
```

---

## 5. Pull Request 생성 프롬프트

### 🤖 프롬프트: PR 작성 및 체크리스트 확인

**사용 시기**: 작업 완료 후 PR을 생성할 때

```
안녕! PCB 불량 검사 시스템 프로젝트에서 작업을 완료했고, Pull Request를 생성하려고 해.

**내 팀:** [Flask 서버 / AI 모델 / 라즈베리파이 / C# 앱]

**읽어야 할 문서:**
1. `docs/Git_Workflow.md` - PR 생성 절차
2. `.github/pull_request_template.md` - PR 템플릿
3. `docs/API_Contract.md` - API 변경 여부 확인

**작업 내용:**
[완료한 작업을 구체적으로 적어주세요]

**PR 생성 전 체크리스트:**
- [ ] 코드 스타일 검사 완료 (PEP 8)
- [ ] 단위 테스트 작성 및 통과
- [ ] API 변경 여부 확인
  - [ ] API 변경 있음 → `docs/API_Contract.md` 업데이트 완료
  - [ ] Mock 서버 업데이트 (`tests/api/mock_server.py`)
  - [ ] 계약 테스트 실행 (`pytest tests/api/test_api_contract.py`)
- [ ] `.env.example` 파일 업데이트 (새로운 환경 변수 추가 시)
- [ ] 충돌 해결 완료
- [ ] 커밋 메시지 규칙 준수 (feat, fix, docs, refactor 등)

**PR 생성 절차:**
1. 최종 커밋 및 푸시:
   ```bash
   git add .
   git commit -m "[타입]: [작업 내용]"
   git push origin feature/[팀명]
   ```

2. GitHub에서 PR 생성:
   - `feature/[팀명]` → `develop`
   - PR 템플릿 작성
   - CODEOWNERS에 따라 담당 팀원 자동 지정

3. 팀 채팅방에 공지:
   ```
   오늘 [작업 내용] 완료. PR #[번호] 생성했습니다. 리뷰 부탁드립니다.
   ```

위 체크리스트를 확인하고, PR 템플릿에 맞춰 제목과 본문을 작성하는 것을 도와줘.
특히 API 변경이 있었다면 어떤 내용을 PR에 명시해야 하는지 알려줘.
```

---

## 6. 문제 해결 프롬프트

### 6.1. Git 충돌 해결

**사용 시기**: `git merge develop` 시 충돌 발생

```
안녕! PCB 불량 검사 시스템 프로젝트에서 Git 충돌이 발생했어.

**상황:**
- `git merge develop` 실행 시 충돌 발생
- 충돌 파일: [충돌이 발생한 파일 경로]

**읽어야 할 문서:**
1. `docs/Git_Workflow.md` - 충돌 해결 방법
2. `docs/Team_Collaboration_Guide.md` - 충돌 방지 규칙

**충돌 내용:**
```
<<<<<<< HEAD
[내 코드]
=======
[develop 브랜치 코드]
>>>>>>> develop
```

**해결 방법:**
1. 충돌 유형 확인:
   - 환경 변수 충돌 → `.env` 파일로 변경 (Git에 올리지 않음)
   - API 인터페이스 충돌 → `docs/API_Contract.md` 확인 후 팀 논의
   - 코드 로직 충돌 → 수동 병합

2. 충돌 해결 후:
   ```bash
   git add [충돌 파일]
   git commit -m "merge: Resolve conflict in [파일명]"
   ```

위 상황에서 어떻게 충돌을 해결해야 하는지 단계별로 안내해줘.
특히 어떤 코드를 선택해야 할지 판단하는 기준도 알려줘.
```

### 6.2. Flask API 연결 문제

**사용 시기**: 라즈베리파이 또는 C# 앱에서 Flask API 호출 실패

```
안녕! PCB 불량 검사 시스템에서 Flask API 연결이 안 돼.

**상황:**
- 클라이언트: [라즈베리파이 / C# 앱]
- Flask 서버 주소: http://100.x.x.x:5000
- 에러 메시지: [발생한 에러 메시지]

**읽어야 할 문서:**
1. `docs/API_Contract.md` - API 명세서
2. `docs/Development_Setup.md` - 네트워크 설정

**확인 사항:**
1. Flask 서버 실행 중인지 확인:
   ```bash
   curl http://100.x.x.x:5000/health
   ```

2. Tailscale VPN 연결 확인:
   ```bash
   tailscale status
   tailscale ip -4
   ```

3. 방화벽 설정 확인:
   ```bash
   sudo ufw status
   sudo ufw allow 5000/tcp
   ```

4. `.env` 파일 확인:
   - `SERVER_URL` 또는 `API_BASE_URL`이 올바른 Tailscale IP인지

위 확인 사항을 순서대로 체크하고, 문제를 해결하는 방법을 알려줘.
```

### 6.3. MySQL 데이터베이스 연결 문제

**사용 시기**: Flask 서버 또는 C# 앱에서 MySQL 연결 실패

```
안녕! PCB 불량 검사 시스템에서 MySQL 데이터베이스 연결이 안 돼.

**상황:**
- 클라이언트: [Flask 서버 / C# 앱]
- MySQL 서버 주소: 100.x.x.x:3306
- DB 계정: [pcb_server / pcb_viewer / pcb_admin]
- 에러 메시지: [발생한 에러 메시지]

**읽어야 할 문서:**
1. `database/README.md` - MySQL 설정 가이드
2. `database/create_users.sql` - 사용자 생성 스크립트

**확인 사항:**
1. MySQL 서버 실행 중인지 확인 (Windows PC):
   ```bash
   mysql -u root -p
   SHOW DATABASES;
   ```

2. Tailscale VPN 연결 확인:
   ```bash
   tailscale ip -4
   ```

3. MySQL 사용자 권한 확인:
   ```sql
   SELECT user, host FROM mysql.user WHERE user LIKE 'pcb%';
   SHOW GRANTS FOR 'pcb_server'@'%';
   ```

4. `.env` 파일 확인:
   ```
   DB_HOST=100.x.x.x  # Tailscale IP
   DB_PORT=3306
   DB_NAME=pcb_inspection
   DB_USER=pcb_server
   DB_PASSWORD=1234
   ```

5. 방화벽 설정 확인 (Windows):
   - 인바운드 규칙: 3306 포트 허용

위 확인 사항을 순서대로 체크하고, 문제를 해결하는 방법을 알려줘.
특히 원격 연결이 안 되는 경우 어떻게 해야 하는지 상세히 설명해줘.
```

### 6.4. 환경 변수 문제

**사용 시기**: `.env` 파일 설정 오류 또는 환경 변수 인식 안 됨

```
안녕! PCB 불량 검사 시스템에서 환경 변수 설정이 제대로 안 돼.

**상황:**
- 컴포넌트: [Flask 서버 / 라즈베리파이 / C# 앱]
- `.env` 파일 위치: [파일 경로]
- 문제: [환경 변수가 인식 안 됨 / 값이 잘못됨]

**읽어야 할 문서:**
1. `docs/Development_Setup.md` - 환경 변수 설정 가이드
2. `scripts/setup_env.sh` - 자동 설정 스크립트

**확인 사항:**
1. `.env.example` 파일과 비교:
   - 모든 필수 환경 변수가 있는지
   - 형식이 올바른지 (공백, 따옴표 등)

2. `.env` 파일이 `.gitignore`에 포함되어 있는지:
   ```bash
   cat .gitignore | grep .env
   ```

3. 환경 변수 로드 확인 (Python):
   ```python
   import os
   from dotenv import load_dotenv
   load_dotenv()
   print(os.getenv("SERVER_URL"))
   ```

4. 자동 설정 스크립트 재실행:
   ```bash
   bash scripts/setup_env.sh
   ```

위 확인 사항을 체크하고, `.env` 파일을 올바르게 설정하는 방법을 알려줘.
특히 Tailscale IP 주소와 MySQL 비밀번호를 어떻게 설정해야 하는지 상세히 설명해줘.
```

---

## 📚 추가 참고 자료

### 핵심 문서 링크

- **프로젝트 전체**: [`docs/PCB_Defect_Detection_Project.md`](PCB_Defect_Detection_Project.md)
- **프로젝트 구조**: [`PROJECT_STRUCTURE.md`](../PROJECT_STRUCTURE.md)
- **팀 협업 가이드**: [`docs/Team_Collaboration_Guide.md`](Team_Collaboration_Guide.md)
- **Git 워크플로우**: [`docs/Git_Workflow.md`](Git_Workflow.md)
- **개발 환경 설정**: [`docs/Development_Setup.md`](Development_Setup.md)
- **API 계약 명세서**: [`docs/API_Contract.md`](API_Contract.md)

### 팀별 상세 가이드

- **Flask 서버 팀**: [`docs/Flask_Server_Setup.md`](Flask_Server_Setup.md)
- **AI 모델 팀**: [`docs/Phase1_YOLO_Setup.md`](Phase1_YOLO_Setup.md), [`docs/Dataset_Guide.md`](Dataset_Guide.md)
- **라즈베리파이 팀**: [`docs/RaspberryPi_Setup.md`](RaspberryPi_Setup.md)
- **C# 앱 팀**: [`docs/CSharp_WinForms_Guide.md`](CSharp_WinForms_Guide.md), [`docs/CSharp_WinForms_Design_Specification.md`](CSharp_WinForms_Design_Specification.md)

### 데이터베이스

- **MySQL 설정**: [`database/README.md`](../database/README.md)
- **스키마**: [`database/schema.sql`](../database/schema.sql)

---

## 💡 프롬프트 사용 팁

1. **복사-붙여넣기**: 프롬프트를 그대로 복사해서 AI에게 붙여넣기
2. **[ ] 부분 수정**: 대괄호 `[ ]` 안의 내용은 본인 상황에 맞게 수정
3. **문서 읽기**: AI가 프로젝트 저장소 내 문서를 읽을 수 있도록 경로 포함
4. **구체적으로**: 작업 내용, 에러 메시지 등을 최대한 구체적으로 작성
5. **단계별 요청**: 한 번에 모든 걸 요청하지 말고, 단계별로 나눠서 질문

---

**마지막 업데이트**: 2025-10-31
**문서 관리**: 팀 리더
**주요 변경사항**:
- **Arduino 디팔렛타이저 팀 섹션 추가** ⭐ 신규 (3.4절)
  - Arduino Mega 2560 + 5-6축 로봇팔 제어 시스템
  - 디팔렛타이저 역할 명시: PCB 픽업 및 박스 분류 (2.5초/PCB 목표)
  - 15개 슬롯 좌표 테이블 및 JSON 시리얼 통신 프로토콜
  - 서보 모터 캘리브레이션 및 성능 목표
- 시스템 구성 업데이트: 디팔렛타이저 역할 및 2.5초 동작 시간 명시
- 프로젝트 전체 이해하기: 디팔렛타이저 관련 질문 추가 (데이터 흐름, 동작 시간 제약)
- 목차 업데이트: Arduino 디팔렛타이저 팀 추가 (C# 앱 팀은 3.5절로 변경)
- 시스템 구성: 라즈베리파이 3대 구분 (웹캠 2대, OHT 제어 1대), 로봇팔/OHT 시스템 추가
- 데이터베이스 스키마: 11개 테이블 (box_status, oht_operations, user_logs 추가)
- API 엔드포인트: 박스 상태 관리 API 3개 추가
- C# WinForms 기능: 박스 상태 모니터링, OHT 제어, 사용자 활동 로그 추가
- 데이터베이스 사용자 계정: 5개 계정 (pcb_admin, pcb_server, pcb_viewer, pcb_data, pcb_test)
