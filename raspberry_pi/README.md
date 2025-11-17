# 라즈베리파이 클라이언트

PCB 검사 시스템의 라즈베리파이 카메라 클라이언트입니다.

## 📋 요구사항

### 하드웨어
- 라즈베리파이 4 Model B (4GB+ 권장)
- USB 웹캠 2대 (좌측/우측)
- GPIO 릴레이 모듈 (4채널)

### 소프트웨어
- Raspberry Pi OS (64-bit 권장)
- Python 3.9+
- Tailscale (VPN, 원격 연결용)

## 🚀 빠른 시작

### 1️⃣ 가상환경 설정

```bash
# 자동 설치 스크립트 실행
chmod +x setup_venv.sh
./setup_venv.sh

# 가상환경 활성화
source venv/bin/activate
```

### 2️⃣ 환경 변수 설정

```bash
# .env 파일 생성
cp .env.example .env

# .env 파일 편집
nano .env
```

`.env` 파일 내용:
```bash
# Flask 서버 URL (Tailscale IP)
SERVER_URL=http://100.123.23.111:5000

# 카메라 설정
CAMERA_ID=left  # 또는 right
CAMERA_INDEX=0

# 카메라 해상도
FRAME_SIZE=720
JPEG_QUALITY=85
TARGET_FPS=30
```

### 3️⃣ 카메라 테스트

```bash
# 카메라 장치 확인
ls /dev/video*

# v4l2-ctl로 카메라 정보 확인
v4l2-ctl -d /dev/video0 --list-formats-ext
```

### 4️⃣ 클라이언트 실행

```bash
# 가상환경 활성화 (필요 시)
source venv/bin/activate

# 클라이언트 실행
python camera_client.py
```

## 🔧 수동 설치

### 시스템 패키지 설치

```bash
sudo apt-get update
sudo apt-get install -y \
    python3-venv \
    python3-pip \
    python3-dev \
    libatlas-base-dev \
    libhdf5-dev \
    v4l-utils
```

### 가상환경 생성 및 활성화

```bash
python3 -m venv venv
source venv/bin/activate
```

### Python 패키지 설치

```bash
pip install --upgrade pip
pip install -r requirements.txt
```

## 📁 파일 구조

```
raspberry_pi/
├── .env.example          # 환경 변수 예시
├── requirements.txt      # Python 의존성
├── setup_venv.sh         # 자동 설정 스크립트
├── README.md            # 이 파일
├── GETTING_STARTED.md   # 시작 가이드
└── camera_client.py     # 카메라 클라이언트 (작성 예정)
```

## 🔍 문제 해결

### OpenCV 설치 오류

라즈베리파이에서 OpenCV 설치가 느리거나 실패하는 경우:

```bash
# 시스템 패키지로 설치 (더 빠름)
sudo apt-get install -y python3-opencv

# 또는 미리 컴파일된 휠 사용
pip install opencv-python-headless
```

### RPi.GPIO 권한 오류

GPIO 접근 권한이 없는 경우:

```bash
# 사용자를 gpio 그룹에 추가
sudo usermod -a -G gpio $USER

# 재로그인 필요
```

### v4l2-ctl 명령어 없음

```bash
sudo apt-get install -y v4l-utils
```

## 📝 참고 문서

- [GETTING_STARTED.md](GETTING_STARTED.md) - 상세 시작 가이드
- [Flask 서버 문서](../server/README.md)
- [프로젝트 전체 문서](../docs/)

## 🌐 네트워크 설정

### Tailscale 설치 (VPN)

```bash
# Tailscale 설치
curl -fsSL https://tailscale.com/install.sh | sh

# Tailscale 시작
sudo tailscale up

# IP 확인
tailscale ip -4
```

### 방화벽 설정

라즈베리파이는 클라이언트 전용이므로 인바운드 포트 개방 불필요합니다.

## 💡 팁

- **자동 시작**: systemd 서비스로 등록하여 부팅 시 자동 실행
- **로그 확인**: 로그 파일은 `logs/` 디렉토리에 저장
- **성능 최적화**: 해상도를 640x640으로 낮추면 전송 속도 향상

## 📞 지원

문제가 발생하면 [Issues](https://github.com/ArianSung/PCB_Detect_Project/issues)에 등록해주세요.
