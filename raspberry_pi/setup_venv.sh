#!/bin/bash
# 라즈베리파이 가상환경 설정 스크립트

set -e  # 오류 발생 시 중단

echo "=========================================="
echo "  PCB 검사 라즈베리파이 환경 설정"
echo "=========================================="

# 색상 정의
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# 1. Python 버전 확인
echo -e "${YELLOW}[1/6] Python 버전 확인 중...${NC}"
PYTHON_VERSION=$(python3 --version 2>&1 | awk '{print $2}')
echo -e "${GREEN}✓ Python 버전: $PYTHON_VERSION${NC}"

if ! python3 -c "import sys; exit(0 if sys.version_info >= (3, 9) else 1)"; then
    echo -e "${RED}✗ Python 3.9 이상이 필요합니다. 현재: $PYTHON_VERSION${NC}"
    exit 1
fi

# 2. venv 모듈 확인
echo -e "${YELLOW}[2/6] venv 모듈 확인 중...${NC}"
if ! python3 -m venv --help >/dev/null 2>&1; then
    echo -e "${RED}✗ python3-venv가 설치되지 않았습니다.${NC}"
    echo -e "${YELLOW}설치 중: sudo apt-get install -y python3-venv${NC}"
    sudo apt-get update
    sudo apt-get install -y python3-venv
fi
echo -e "${GREEN}✓ venv 모듈 확인 완료${NC}"

# 3. 시스템 패키지 설치 (OpenCV 의존성)
echo -e "${YELLOW}[3/6] 시스템 패키지 설치 중...${NC}"
echo -e "${YELLOW}다음 패키지가 필요합니다: libatlas-base-dev, libhdf5-dev, v4l-utils${NC}"
read -p "설치하시겠습니까? (y/N) " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    sudo apt-get update
    sudo apt-get install -y \
        libatlas-base-dev \
        libhdf5-dev \
        libhdf5-103 \
        libqtgui4 \
        libqt4-test \
        v4l-utils \
        python3-dev \
        python3-pip
    echo -e "${GREEN}✓ 시스템 패키지 설치 완료${NC}"
else
    echo -e "${YELLOW}건너뜀 (나중에 수동 설치 필요할 수 있음)${NC}"
fi

# 4. 가상환경 생성
VENV_DIR="venv"
echo -e "${YELLOW}[4/6] 가상환경 생성 중: $VENV_DIR${NC}"
if [ -d "$VENV_DIR" ]; then
    echo -e "${YELLOW}⚠ 가상환경이 이미 존재합니다. 삭제하고 다시 생성하시겠습니까? (y/N)${NC}"
    read -p "" -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        rm -rf "$VENV_DIR"
        python3 -m venv "$VENV_DIR"
        echo -e "${GREEN}✓ 가상환경 재생성 완료${NC}"
    else
        echo -e "${YELLOW}기존 가상환경 사용${NC}"
    fi
else
    python3 -m venv "$VENV_DIR"
    echo -e "${GREEN}✓ 가상환경 생성 완료${NC}"
fi

# 5. 가상환경 활성화 및 pip 업그레이드
echo -e "${YELLOW}[5/6] pip 업그레이드 중...${NC}"
source "$VENV_DIR/bin/activate"
pip install --upgrade pip setuptools wheel
echo -e "${GREEN}✓ pip 업그레이드 완료${NC}"

# 6. 의존성 설치
echo -e "${YELLOW}[6/6] 의존성 패키지 설치 중...${NC}"
if [ -f "requirements.txt" ]; then
    pip install -r requirements.txt
    echo -e "${GREEN}✓ 의존성 설치 완료${NC}"
else
    echo -e "${RED}✗ requirements.txt 파일이 없습니다.${NC}"
    exit 1
fi

echo ""
echo -e "${GREEN}=========================================="
echo "  ✅ 환경 설정 완료!"
echo "==========================================${NC}"
echo ""
echo "가상환경 활성화 방법:"
echo -e "${YELLOW}  source venv/bin/activate${NC}"
echo ""
echo "가상환경 비활성화:"
echo -e "${YELLOW}  deactivate${NC}"
echo ""
echo "설치된 패키지 확인:"
echo -e "${YELLOW}  pip list${NC}"
echo ""
