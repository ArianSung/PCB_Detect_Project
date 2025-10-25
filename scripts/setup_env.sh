#!/bin/bash

# PCB 불량 검사 시스템 - 환경 설정 자동화 스크립트
# 이 스크립트는 개발 환경을 자동으로 감지하고 .env 파일을 생성합니다

set -e  # 오류 시 스크립트 중단

echo "=========================================="
echo "PCB 불량 검사 시스템 - 환경 설정"
echo "=========================================="
echo ""

# 색상 정의
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# 프로젝트 루트 디렉토리
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$PROJECT_ROOT"

# ===== 1. 네트워크 환경 감지 =====
echo -e "${YELLOW}[1/5] 네트워크 환경 감지 중...${NC}"

# Tailscale VPN 확인
if command -v tailscale &> /dev/null && tailscale status &> /dev/null; then
    TAILSCALE_IP=$(tailscale ip -4 2>/dev/null | head -n1)
    if [ -n "$TAILSCALE_IP" ]; then
        echo -e "${GREEN}✓ Tailscale VPN 감지됨: $TAILSCALE_IP${NC}"
        NETWORK_MODE="remote"
        SERVER_IP="$TAILSCALE_IP"
    else
        echo -e "${YELLOW}⚠ Tailscale 설치되어 있으나 연결 안 됨${NC}"
        NETWORK_MODE="local"
        SERVER_IP="192.168.0.10"
    fi
else
    echo -e "${YELLOW}⚠ Tailscale VPN 감지 안 됨 (로컬 네트워크 사용)${NC}"
    NETWORK_MODE="local"
    SERVER_IP="192.168.0.10"
fi

# ===== 2. GPU 환경 감지 =====
echo -e "${YELLOW}[2/5] GPU 환경 감지 중...${NC}"

if command -v nvidia-smi &> /dev/null; then
    GPU_NAME=$(nvidia-smi --query-gpu=name --format=csv,noheader 2>/dev/null | head -n1)
    GPU_MEMORY=$(nvidia-smi --query-gpu=memory.total --format=csv,noheader 2>/dev/null | head -n1)
    echo -e "${GREEN}✓ NVIDIA GPU 감지됨: $GPU_NAME ($GPU_MEMORY)${NC}"
    GPU_DEVICE="cuda:0"
    USE_FP16="true"
else
    echo -e "${YELLOW}⚠ GPU 감지 안 됨 (CPU 모드)${NC}"
    GPU_DEVICE="cpu"
    USE_FP16="false"
fi

# ===== 3. 라즈베리파이 감지 =====
echo -e "${YELLOW}[3/5] 시스템 타입 감지 중...${NC}"

if [ -f /proc/device-tree/model ] && grep -q "Raspberry Pi" /proc/device-tree/model; then
    echo -e "${GREEN}✓ 라즈베리파이 감지됨${NC}"
    SYSTEM_TYPE="raspberry_pi"

    # 카메라 ID 선택
    echo ""
    echo "이 라즈베리파이의 역할을 선택하세요:"
    echo "  1) 좌측 카메라 + GPIO 제어 (라즈베리파이 1)"
    echo "  2) 우측 카메라 전용 (라즈베리파이 2)"
    read -p "선택 (1 또는 2): " CAMERA_CHOICE

    if [ "$CAMERA_CHOICE" = "1" ]; then
        CAMERA_ID="left"
        GPIO_ENABLED="true"
        echo -e "${GREEN}✓ 좌측 카메라 + GPIO 제어 설정${NC}"
    else
        CAMERA_ID="right"
        GPIO_ENABLED="false"
        echo -e "${GREEN}✓ 우측 카메라 전용 설정${NC}"
    fi
else
    echo -e "${GREEN}✓ 일반 PC 감지됨${NC}"
    SYSTEM_TYPE="pc"
fi

# ===== 4. .env 파일 생성 =====
echo -e "${YELLOW}[4/5] .env 파일 생성 중...${NC}"

# Flask 서버 .env 생성 (PC만 해당)
if [ "$SYSTEM_TYPE" = "pc" ] && [ ! -f "src/server/.env" ]; then
    echo -e "${GREEN}✓ Flask 서버 .env 파일 생성 중...${NC}"
    cat > src/server/.env << EOF
# Flask 서버 환경 변수 (자동 생성됨)
FLASK_ENV=development
SERVER_HOST=0.0.0.0
SERVER_PORT=5000

GPU_DEVICE=$GPU_DEVICE
USE_FP16=$USE_FP16

YOLO_MODEL_PATH=models/yolo/final/yolo_best.pt
ANOMALY_MODEL_PATH=models/anomaly/padim/model.pth

CONFIDENCE_THRESHOLD=0.25
IOU_THRESHOLD=0.45
MAX_BATCH_SIZE=2

DB_HOST=localhost
DB_PORT=3306
DB_NAME=pcb_inspection
DB_USER=root
DB_PASSWORD=your_password_here

LOG_LEVEL=INFO
LOG_FILE=logs/server.log

CORS_ORIGINS=*
EOF
    echo -e "${GREEN}  → src/server/.env 생성 완료${NC}"
else
    echo -e "${YELLOW}  → src/server/.env 이미 존재 (건너뜀)${NC}"
fi

# 라즈베리파이 클라이언트 .env 생성
if [ "$SYSTEM_TYPE" = "raspberry_pi" ] && [ ! -f "raspberry_pi/.env" ]; then
    echo -e "${GREEN}✓ 라즈베리파이 클라이언트 .env 파일 생성 중...${NC}"
    cat > raspberry_pi/.env << EOF
# 라즈베리파이 클라이언트 환경 변수 (자동 생성됨)
CAMERA_ID=$CAMERA_ID
CAMERA_INDEX=0
CAMERA_WIDTH=640
CAMERA_HEIGHT=480
CAMERA_FPS=10

SERVER_URL=http://$SERVER_IP:5000
API_ENDPOINT=/predict

JPEG_QUALITY=85
FRAME_INTERVAL=0.1

GPIO_ENABLED=$GPIO_ENABLED
GPIO_MODE=BCM
GPIO_PIN_COMPONENT_DEFECT=17
GPIO_PIN_SOLDER_DEFECT=27
GPIO_PIN_DISCARD=22
GPIO_PIN_NORMAL=23
GPIO_RELAY_DURATION=0.5

LOG_LEVEL=INFO
LOG_FILE=logs/camera_client_$CAMERA_ID.log

MAX_RETRIES=3
RETRY_DELAY=2
REQUEST_TIMEOUT=5
EOF
    echo -e "${GREEN}  → raspberry_pi/.env 생성 완료${NC}"
else
    echo -e "${YELLOW}  → raspberry_pi/.env 이미 존재 (건너뜀)${NC}"
fi

# C# WinForms .env 생성 (PC만 해당)
if [ "$SYSTEM_TYPE" = "pc" ] && [ ! -f "csharp_winforms/.env" ]; then
    echo -e "${GREEN}✓ C# WinForms .env 파일 생성 중...${NC}"
    cat > csharp_winforms/.env << EOF
# C# WinForms 환경 변수 (자동 생성됨)
API_BASE_URL=http://$SERVER_IP:5000
API_TIMEOUT=10

DB_HOST=localhost
DB_PORT=3306
DB_NAME=pcb_inspection
DB_USER=root
DB_PASSWORD=your_password_here

REFRESH_INTERVAL=5000
MAX_DISPLAY_RECORDS=100

LOG_LEVEL=Information
LOG_FILE=logs/winforms_app.log

EXPORT_DEFAULT_PATH=C:\\PCB_Reports
DEFAULT_USER_ROLE=Viewer
SESSION_TIMEOUT=3600
EOF
    echo -e "${GREEN}  → csharp_winforms/.env 생성 완료${NC}"
else
    echo -e "${YELLOW}  → csharp_winforms/.env 이미 존재 (건너뜀)${NC}"
fi

# ===== 5. 요약 출력 =====
echo ""
echo -e "${YELLOW}[5/5] 설정 요약${NC}"
echo "=========================================="
echo "시스템 타입: $SYSTEM_TYPE"
echo "네트워크 모드: $NETWORK_MODE"
echo "서버 IP: $SERVER_IP"
if [ "$SYSTEM_TYPE" = "pc" ]; then
    echo "GPU 디바이스: $GPU_DEVICE"
    echo "FP16 사용: $USE_FP16"
fi
if [ "$SYSTEM_TYPE" = "raspberry_pi" ]; then
    echo "카메라 ID: $CAMERA_ID"
    echo "GPIO 제어: $GPIO_ENABLED"
fi
echo "=========================================="
echo ""

# ===== 6. 다음 단계 안내 =====
echo -e "${GREEN}✓ 환경 설정 완료!${NC}"
echo ""
echo -e "${YELLOW}다음 단계:${NC}"
echo "1. .env 파일에서 DB_PASSWORD를 실제 비밀번호로 변경하세요"
echo "2. 필요한 경우 .env 파일의 다른 설정도 수정하세요"
if [ "$SYSTEM_TYPE" = "pc" ]; then
    echo "3. Flask 서버 실행: bash scripts/start_server.sh"
elif [ "$SYSTEM_TYPE" = "raspberry_pi" ]; then
    echo "3. 카메라 클라이언트 실행: python3 raspberry_pi/camera_client.py"
fi
echo ""
