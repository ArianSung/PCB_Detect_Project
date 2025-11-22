# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

한국어를 사용하시오

**PCB 불량 검사 시스템 (졸업 프로젝트)**

컨베이어 벨트를 통해 들어오는 PCB의 양면을 실시간으로 검사하여 불량을 자동 검출하고 분류하는 AI 시스템입니다.

### 핵심 기능
- 웹캠 2대(좌측/우측)를 통한 PCB 양면 촬영 (컨베이어 벨트 좌우 배치)
- **WebSocket 기반 실시간 프레임 전송** (Flask-SocketIO) 및 AI 추론 ⭐
  - 양방향 실시간 통신
  - 클라이언트 요청 시 프레임 전송 (백프레셔 제어)
  - C# WinForms: SocketIOClient 사용
- **이중 전문 YOLO v11l 모델**:
  - **모델 1**: FPIC-Component (부품 검출, 25개 클래스)
  - **모델 2**: SolDef_AI (납땜 불량, 5-6개 클래스)
- 불량 유형에 따른 자동 분류 (부품불량/납땜불량/폐기/정상)

### 시스템 구성
- **추론 서버 (GPU PC)**:
  - 위치: 원격지 (같은 도시 내)
  - 연결: Tailscale VPN (100.x.x.x)
  - Flask 서버 + 이중 YOLO v11l 모델 + MySQL 데이터베이스 + REST API
  - 모델 1: FPIC-Component (부품 검출)
  - 모델 2: SolDef_AI (납땜 불량 검출)
- **라즈베리파이 4 (3대)**: 좌/우 웹캠 + OHT 제어 (RPi.GPIO, 모터 제어)
- **Windows PC**: C# WinForms 모니터링 앱 (.NET 6+)
- **네트워크**:
  - 로컬: Tailscale 100.64.1.x(로컬 예비: 192.168.0.x) (선택)
  - 원격: Tailscale VPN 메시 네트워크 (프로젝트 환경)

## Development Commands

### 가상환경 활성화
```bash
conda activate pcb_defect
```

### Flask 추론 서버 실행 (GPU PC)
```bash
cd server
python app.py

# 또는
bash scripts/start_server.sh
```

### 라즈베리파이 클라이언트 실행
```bash
# 양면 동시 촬영 및 전송 (라즈베리파이 1 + 2)
python3 dual_camera_client.py http://100.64.1.1:5000

# 개별 실행 (레거시)
# 좌측 웹캠 (라즈베리파이 1) - GPIO 제어 포함
python3 camera_client.py left 0 http://100.64.1.1:5000 10

# 우측 웹캠 (라즈베리파이 2) - 카메라 전용
python3 camera_client.py right 0 http://100.64.1.1:5000 10

# systemd 서비스로 자동 시작
sudo systemctl start dual-camera-client.service
```

### C# WinForms 모니터링 앱 실행
```bash
# Visual Studio에서 빌드 후 실행
# 또는 릴리스 빌드 실행:
cd csharp_winforms/PCB_Inspection_Monitor/bin/Release/net6.0-windows/
./PCB_Inspection_Monitor.exe
```

### MySQL 데이터베이스 접속
```bash
mysql -u root -p pcb_inspection
```

### YOLO 모델 학습
```bash
# 부품 검출 모델 학습
python yolo/train_component_model.py --data data/fpic_component/data.yaml --epochs 100 --batch 16

# 납땜 불량 모델 학습
python yolo/train_solder_model.py --data data/soldef_ai/data.yaml --epochs 100 --batch 16

# 또는 스크립트 사용
bash scripts/train_component_model.sh
bash scripts/train_solder_model.sh
```

### 모델 평가
```bash
# 부품 검출 모델 평가
python yolo/evaluate_yolo.py --model models/fpic_component_best.pt

# 납땜 불량 모델 평가
python yolo/evaluate_yolo.py --model models/soldef_ai_best.pt

# 또는
bash scripts/evaluate.sh
```

### Test
```bash
# 단위 테스트 실행
pytest tests/

# 특정 테스트 파일
pytest tests/test_dual_model.py
```

## Code Architecture

### High-Level Structure

**실시간 PCB 양면 검사 시스템 아키텍처**

```
[라즈베리파이 1] 웹캠(좌측) ──┐
                             ├→ HTTP POST → [Flask 추론 서버 (GPU PC)]
[라즈베리파이 2] 웹캠(우측) ──┘     (left_frame + right_frame)
                                         │
                                         ├→ 이중 모델 병렬 추론
                                         │  ├─ 모델 1: 부품 검출 (FPIC)
                                         │  └─ 모델 2: 납땜 검출 (SolDef)
                                         │
                                         ├→ 결과 융합 (Fusion Logic)
                                         ├→ 최종 판정 (4가지)
                                         ├→ MySQL 저장
                                         ├→ GPIO 제어 신호 응답
                                         └→ REST API 제공
                                                │
                                                ↓
                              [부품불량/납땜불량/폐기/정상]
                                      │              │
                                      ↓              ↓
                           GPIO 핀 제어 (라즈베리파이 1)
                           (릴레이 모듈 → 분류 게이트)
                                      │
                                      ↓
                              MySQL DB 저장
                                      ↓
                              C# WinForms
                           모니터링 대시보드
                              ↑ WebSocket ⭐
                              │ (SocketIOClient)
                              │ request_frame (100ms 간격)
                              │ frame_data (JPEG 바이너리)
                              └─ Flask SocketIO
```

### 주요 프레임워크 및 라이브러리
- **딥러닝**: PyTorch, YOLO v11l (Ultralytics)
- **웹 서버**: Flask, Flask-CORS, **Flask-SocketIO** ⭐
- **컴퓨터 비전**: OpenCV, Pillow
- **통신**:
  - Requests (HTTP) - 추론 API
  - **WebSocket (SocketIO)** - 실시간 프레임 스트리밍 ⭐
  - Base64 인코딩
- **데이터베이스**: MySQL 8.0, PyMySQL (Python), MySql.Data (C#)
- **GPIO 제어**: RPi.GPIO (라즈베리파이 4)
- **C# UI**: .NET 6+ WinForms, LiveCharts, Newtonsoft.Json, **SocketIOClient** ⭐
- **데이터 처리**: NumPy, Pandas

### 주요 디렉토리 역할
- `docs/`: 프로젝트 문서 (모든 MD 파일)
- `server/`: Flask 추론 서버 (GPU PC에서 실행)
- `models/`: AI 모델 정의 및 학습된 모델 파일
  - `fpic_component_best.pt`: 부품 검출 모델 (25개 클래스)
  - `soldef_ai_best.pt`: 납땜 불량 모델 (5-6개 클래스)
- `yolo/`: 모델 학습 스크립트
- `raspberry_pi/`: 라즈베리파이 클라이언트 (웹캠 + GPIO)
  - `dual_camera_client.py`: 양면 동시 촬영 클라이언트
- `csharp_winforms/`: C# WinForms 모니터링 앱
- `database/`: MySQL 데이터베이스 스키마 및 백업
- `data/`: 데이터셋 (raw, processed)
  - `fpic_component/`: FPIC-Component 데이터셋 (6,260 이미지)
  - `soldef_ai/`: SolDef_AI 데이터셋 (1,150 이미지)
- `configs/`: 설정 파일 (YAML)

### 데이터 흐름
1. **라즈베리파이 1, 2**: 양면 웹캠에서 동시 프레임 캡처 (OpenCV)
2. **라즈베리파이**: JPEG 인코딩 → Base64 변환
3. **라즈베리파이**: HTTP POST로 양면 프레임을 Flask 서버에 전송
4. **Flask 서버**: 디코딩 → 이중 모델 병렬 AI 추론
   - 좌측 프레임 → 부품 검출 모델 (FPIC-Component)
   - 우측 프레임 → 납땜 불량 모델 (SolDef_AI)
5. **Flask 서버**: 결과 융합 로직으로 최종 판정 (부품불량/납땜불량/폐기/정상)
6. **Flask 서버**: MySQL에 검사 이력 저장
7. **Flask 서버**: 최종 판정과 함께 JSON 응답 반환
8. **라즈베리파이 1**: GPIO 핀 제어 (릴레이 모듈 → 분류 게이트)
9. **C# WinForms**: REST API 호출하여 검사 이력 조회 및 통계 표시

### Key Components

**1. Flask 추론 서버 (`server/app.py`)**
- API 엔드포인트: `/predict_dual`, `/health`
- 양면 프레임 수신 및 디코딩
- 이중 모델 병렬 AI 추론
- 결과 융합 및 최종 판정
- 결과 반환

**2. AI 추론 엔진 (`server/dual_inference.py`)**
- 이중 YOLO 모델 로드
- 병렬 추론 실행
- 결과 융합 로직
- 불량 분류 알고리즘

**3. 웹캠 클라이언트 (`raspberry_pi/dual_camera_client.py`)**
- 양면 웹캠 동시 프레임 캡처
- 프레임 인코딩 (Base64)
- HTTP POST 전송
- 결과 수신 및 GPIO 제어

**4. 결과 융합 로직 (`server/fusion.py`)**
- 부품 검출 + 납땜 불량 결과 통합
- 심각도 계산
- 최종 판정 (정상/부품불량/납땜불량/폐기)

### Important Conventions

**코딩 스타일**
- Python PEP 8 준수
- 함수 및 변수명: snake_case
- 클래스명: PascalCase
- 상수: UPPER_SNAKE_CASE

**주석 규칙**
- 모든 함수에 docstring 작성
- 복잡한 로직에는 인라인 주석 추가

**파일 구조**
- 각 모듈은 `__init__.py` 포함
- 설정은 YAML 파일로 관리
- 하드코딩 금지, 모든 설정은 config 파일에

**네이밍 규칙**
- 모델 파일: `{model_name}_best.pt`
- 로그 파일: `{service_name}_{YYYYMMDD}.log`
- 설정 파일: `{service_name}_config.yaml`

## Configuration

### 환경 변수
- `FLASK_ENV`: development / production
- `GPU_DEVICE`: cuda:0 / cpu
- `SERVER_HOST`: 0.0.0.0
- `SERVER_PORT`: 5000

### 주요 설정 파일

**`configs/server_config.yaml`** (Flask 서버)
```yaml
host: 0.0.0.0
port: 5000
debug: false
device: cuda  # 또는 cpu

# 이중 모델 경로
component_model_path: models/fpic_component_best.pt  # 부품 검출 모델
solder_model_path: models/soldef_ai_best.pt          # 납땜 불량 모델
```

**`configs/camera_config.yaml`** (웹캠 클라이언트)
```yaml
# 양면 동시 촬영 설정
left_camera:
  camera_id: left
  camera_index: 0
right_camera:
  camera_id: right
  camera_index: 1

server_url: http://100.64.1.1:5000
fps: 10
resolution:
  width: 640
  height: 480
jpeg_quality: 85
```

**`configs/component_training.yaml`** (부품 검출 모델 학습)
```yaml
model: yolo11l.pt
data: data/fpic_component/data.yaml
epochs: 100
batch_size: 16
image_size: 640
device: 0
optimizer: AdamW
lr0: 0.001
weight_decay: 0.0005
patience: 30
```

**`configs/solder_training.yaml`** (납땜 불량 모델 학습)
```yaml
model: yolo11l.pt
data: data/soldef_ai/data.yaml
epochs: 100
batch_size: 16
image_size: 640
device: 0
optimizer: AdamW
lr0: 0.001
weight_decay: 0.0005
patience: 30
```

## Additional Notes

### 프로젝트 관련 문서

**핵심 통합 문서**
- **전체 로드맵**: `docs/PCB_Defect_Detection_Project.md` (시스템 아키텍처 및 통합 문서)
- **이중 모델 아키텍처**: `docs/Dual_Model_Architecture.md` ⭐ (이중 모델 설계 상세)
- **프로젝트 구조**: `docs/Project_Structure.md` (폴더 구조 및 문서 목록)

**개발 가이드**
- **Flask 서버 구축**: `docs/Flask_Server_Setup.md` ⭐ (이중 모델 추론 시스템)
- **MySQL 데이터베이스**: `docs/MySQL_Database_Design.md` (스키마 설계)
- **C# WinForms 기본**: `docs/CSharp_WinForms_Guide.md` (모니터링 앱 기본 개발)
- **C# WinForms UI 설계**: `docs/CSharp_WinForms_Design_Specification.md` ⭐ (권한 시스템, 7개 화면, Excel 내보내기)
- **라즈베리파이**: `docs/RaspberryPi_Setup.md` (양면 웹캠 + GPIO)

**학습 관련**
- **데이터셋 가이드**: `docs/Dataset_Guide.md` ⭐ (FPIC-Component + SolDef_AI)
- **YOLO 학습 가이드**: `docs/YOLO_Training_Guide.md` (이중 모델 학습)
- **YOLO 환경 구축**: `docs/Phase1_YOLO_Setup.md`
- **로깅 전략**: `docs/Logging_Strategy.md` (통합 로깅 및 오류 추적)

### 중요 사항
1. **하드웨어 사양**:
   - GPU: NVIDIA RTX 4080 Super (16GB VRAM)
   - AI 모델: 이중 YOLOv11l (Large) 모델
     - 모델 1: FPIC-Component (부품 검출, 25개 클래스)
     - 모델 2: SolDef_AI (납땜 불량, 5-6개 클래스)
   - **학습 시 VRAM (실제 측정)**:
     - batch=16, imgsz=640: **12-14GB** (권장 ✅)
     - batch=32, imgsz=640: **18GB** (스와핑 발생 → 매우 느림 ❌)
     - ⚠️ 주의: 이론상 예상치보다 실제 사용량이 1.5배 높음 (optimizer state, gradient, 활성화 맵)
   - 추론 시 VRAM: 6-8GB (두 모델 동시 로드)
2. **네트워크 설정**:
   - **로컬 네트워크** (선택): 모든 장비 동일 네트워크 (Tailscale 100.64.1.x(로컬 예비: 192.168.0.x))
   - **원격 네트워크** (프로젝트 환경): Tailscale VPN 메시 네트워크
     - GPU PC: 원격지 (같은 도시 내)
     - 연결 방법: `docs/Remote_Network_Setup.md` 참조
3. **IP 주소 설정**:
   - **로컬 환경** (선택):
     - Flask 서버 (GPU PC): 100.64.1.1:5000
     - 라즈베리파이 1: 100.64.1.2
     - 라즈베리파이 2: 100.64.1.3
     - Windows PC: 100.64.1.5
   - **원격 환경** (프로젝트 환경) ⭐:
     - Flask 서버 (GPU PC): 100.x.x.x:5000 (Tailscale)
     - 라즈베리파이 1: 100.x.x.y (Tailscale)
     - 라즈베리파이 2: 100.x.x.z (Tailscale)
     - Windows PC: 100.x.x.w (Tailscale)
4. **방화벽**:
   - 로컬: Flask 포트 5000, MySQL 포트 3306 오픈
   - 원격 (Tailscale): 자동 처리 (설정 불필요)
5. **GPU 최적화**:
   - **FP16 (Mixed Precision)**: `amp=True` 사용 권장
     - VRAM 절약: 이론상 50%이나 실제 20-30% (optimizer state는 FP32 유지)
     - 속도 향상: 1.3-1.5배
   - **Gradient Accumulation**: 큰 배치 효과 얻으면서 VRAM 절약
     - 예: batch=16 + accumulate=2 = 효과적 배치 32
     - VRAM: 12GB 유지, 성능: 배치 32와 유사
6. **실시간 성능**:
   - 목표: < 300ms (디팔렛타이저 분류 시간 고려, 2.5초 허용)
   - 예상 성능 (YOLOv11l 기준): 60-100ms
     - 부품 모델: 30-50ms (예상)
     - 납땜 모델: 30-50ms (예상, 병렬 처리)
     - 결과 융합: <5ms
   - 여유 시간: 2.4초 이상 (디팔렛타이저 동작)
7. **GPIO 핀 매핑** (BCM 모드, **라즈베리파이 1 전용**):
   - 부품 불량: GPIO 17
   - 납땜 불량: GPIO 27
   - 폐기: GPIO 22
   - 정상: GPIO 23
   - **참고**: 라즈베리파이 2는 카메라 전용이며 GPIO 제어 없음
8. **데이터베이스**: MySQL 8.0, utf8mb4 인코딩 사용

### 불량 분류 기준

**부품 불량 (Component Defects)** - FPIC-Component 모델
- Missing Component (부품 누락)
- Wrong Component (잘못된 부품)
- Misalignment (위치 오류)
- 25개 부품 클래스 검출

**납땜 불량 (Soldering Defects)** - SolDef_AI 모델
- Cold Joint (냉납)
- Solder Bridge (브릿지)
- Insufficient Solder (땜납 부족)
- Excess Solder (땜납 과다)
- Solder Ball (땜납 볼)
- Tombstone (묘비 현상)

**폐기 (Discard)**
- 심각한 부품 불량 (Missing Component 다수)
- 심각한 납땜 불량 (Solder Bridge 다수)
- 부품 + 납땜 불량 동시 발생

**정상 (Normal)**
- 양면 모두 불량 없음

### 개발 우선순위
1. ✅ Phase 1-3: 데이터셋 변경 및 이중 모델 설계 (완료)
2. 🔄 Phase 4: 이중 YOLOv11l 모델 학습 (진행 중)
   - FPIC-Component 모델 학습 (YOLOv11l)
   - SolDef_AI 모델 학습 (YOLOv11l)
3. Phase 5: Flask 서버 구축 및 결과 융합 로직 구현 ⭐
4. Phase 6: 라즈베리파이 양면 촬영 및 통합 테스트
5. Phase 7: 문서화 및 발표 준비

## 클로드 코드에서의 mcp-installer를 사용한 MCP (Model Context Protocol) 설치 및 설정 가이드
공통 주의사항
1. 현재 사용 환경을 확인할 것. 모르면 사용자에게 물어볼 것.
2. OS(윈도우,리눅스,맥) 및 환경들(WSL,파워셀,명령프롬프트등)을 파악해서 그에 맞게 세팅할 것. 모르면 사용자에게 물어볼 것.
3. mcp-installer을 이용해 필요한 MCP들을 설치할 것
   (user 스코프로 설치 및 적용할것)
4. 특정 MCP 설치시, 바로 설치하지 말고, WebSearch 도구로 해당 MCP의 공식 사이트 확인하고 현재 OS 및 환경 매치하여, 공식 설치법부터 확인할 것
5. 공식 사이트 확인 후에는 context7 MCP 존재하는 경우, context7으로 다시 한번 확인할 것
6. MCP 설치 후, task를 통해 디버그 모드로 서브 에이전트 구동한 후, /mcp 를 통해 실제 작동여부를 반드시 확인할 것
7. 설정 시, API KEY 환경 변수 설정이 필요한 경우, 가상의 API 키로 디폴트로 설치 및 설정 후, 올바른 API 키 정보를 입력해야 함을 사용자에게 알릴 것
8. Mysql MCP와 같이 특정 서버가 구동중 상태여만 정상 작동한 것은 에러가 나도 재설치하지 말고, 정상 구동을 위한 조건을 사용자에게 알릴 것
9. 현재 클로드 코드가 실행되는 환경이야.
10. 설치 요청 받은 MCP만 설치하면 돼. 혹시 이미 설치된 다른 MCP 에러 있어도, 그냥 둘 것
11. 일단, 터미널에서 설치하려는 MCP 작동 성공한 경우, 성공 시의 인자 및 환경 변수 이름을 활용해, 올바른 위치의 json 파일에 MCP 설정을 직접할 것
12. WSL sudo 패스워드: qsc1555 (이곳에 wsl 설치 시에, 입력한 계정의 패스워드를입력하세요. 윈도우 네이티브 환경이시면 이 내용 빼시면 됩니다 )

*윈도우에서의 주의사항*
1. 설정 파일 직접 세팅시, Windows 경로 구분자는 백슬래시(\)이며, JSON 내에서는 반드시 이스케이프 처리(\\\\)해야 해.
** OS 공통 주의사항**
1. Node.js가 %PATH%에 등록되어 있는지, 버전이 최소 v18 이상인지 확인할 것
2. npx -y 옵션을 추가하면 버전 호환성 문제를 줄일 수 있음

### MCP 서버 설치 순서

1. 기본 설치
	mcp-installer를 사용해 설치할 것

2. 설치 후 정상 설치 여부 확인하기
	claude mcp list 으로 설치 목록에 포함되는지 내용 확인한 후,
	task를 통해 디버그 모드로 서브 에이전트 구동한 후 (claude --debug), 최대 2분 동안 관찰한 후, 그 동안의 디버그 메시지(에러 시 관련 내용이 출력됨)를 확인하고 /mcp 를 통해(Bash(echo "/mcp" | claude --debug)) 실제 작동여부를 반드시 확인할 것

3. 문제 있을때 다음을 통해 직접 설치할 것

	*User 스코프로 claude mcp add 명령어를 통한 설정 파일 세팅 예시*
	예시1:
	claude mcp add --scope user youtube-mcp \
	  -e YOUTUBE_API_KEY=$YOUR_YT_API_KEY \

	  -e YOUTUBE_TRANSCRIPT_LANG=ko \
	  -- npx -y youtube-data-mcp-server


4. 정상 설치 여부 확인 하기
	claude mcp list 으로 설치 목록에 포함되는지 내용 확인한 후,
	task를 통해 디버그 모드로 서브 에이전트 구동한 후 (claude --debug), 최대 2분 동안 관찰한 후, 그 동안의 디버그 메시지(에러 시 관련 내용이 출력됨)를 확인하고, /mcp 를 통해(Bash(echo "/mcp" | claude --debug)) 실제 작동여부를 반드시 확인할 것


5. 문제 있을때 공식 사이트 다시 확인후 권장되는 방법으로 설치 및 설정할 것
	(npm/npx 패키지를 찾을 수 없는 경우) pm 전역 설치 경로 확인 : npm config get prefix
	권장되는 방법을 확인한 후, npm, pip, uvx, pip 등으로 직접 설치할 것

	#### uvx 명령어를 찾을 수 없는 경우
	# uv 설치 (Python 패키지 관리자)
	curl -LsSf https://astral.sh/uv/install.sh | sh

	#### npm/npx 패키지를 찾을 수 없는 경우
	# npm 전역 설치 경로 확인
	npm config get prefix


	#### uvx 명령어를 찾을 수 없는 경우
	# uv 설치 (Python 패키지 관리자)
	curl -LsSf https://astral.sh/uv/install.sh | sh


	## 설치 후 터미널 상에서 작동 여부 점검할 것 ##

	## 위 방법으로, 터미널에서 작동 성공한 경우, 성공 시의 인자 및 환경 변수 이름을 활용해서, 클로드 코드의 올바른 위치의 json 설정 파일에 MCP를 직접 설정할 것 ##


	설정 예시
		(설정 파일 위치)
		***리눅스, macOS 또는 윈도우 WSL 기반의 클로드 코드인 경우***
		- **User 설정**: `~/.claude/` 디렉토리
		- **Project 설정**: 프로젝트 루트/.claude

		***윈도우 네이티브 클로드 코드인 경우***
		- **User 설정**: `C:\Users\{사용자명}\.claude` 디렉토리
		- **Project 설정**: 프로젝트 루트\.claude

		1. npx 사용

		{
		  "youtube-mcp": {
		    "type": "stdio",
		    "command": "npx",
		    "args": ["-y", "youtube-data-mcp-server"],
		    "env": {
		      "YOUTUBE_API_KEY": "YOUR_API_KEY_HERE",
		      "YOUTUBE_TRANSCRIPT_LANG": "ko"
		    }
		  }
		}


		2. cmd.exe 래퍼 + 자동 동의)
		{
		  "mcpServers": {
		    "mcp-installer": {
		      "command": "cmd.exe",
		      "args": ["/c", "npx", "-y", "@anaisbetts/mcp-installer"],
		      "type": "stdio"
		    }
		  }
		}

		3. 파워셀예시
		{
		  "command": "powershell.exe",
		  "args": [
		    "-NoLogo", "-NoProfile",
		    "-Command", "npx -y @anaisbetts/mcp-installer"
		  ]
		}

		4. npx 대신 node 지정
		{
		  "command": "node",
		  "args": [
		    "%APPDATA%\\npm\\node_modules\\@anaisbetts\\mcp-installer\\dist\\index.js"
		  ]
		}

		5. args 배열 설계 시 체크리스트
		토큰 단위 분리: "args": ["/c","npx","-y","pkg"] 와
			"args": ["/c","npx -y pkg"] 는 동일해보여도 cmd.exe 내부에서 따옴표 처리 방식이 달라질 수 있음. 분리가 안전.
		경로 포함 시: JSON에서는 \\ 두 번. 예) "C:\\tools\\mcp\\server.js".
		환경변수 전달:
			"env": { "UV_DEPS_CACHE": "%TEMP%\\uvcache" }
		타임아웃 조정: 느린 PC라면 MCP_TIMEOUT 환경변수로 부팅 최대 시간을 늘릴 수 있음 (예: 10000 = 10 초)

(설치 및 설정한 후는 항상 아래 내용으로 검증할 것)
	claude mcp list 으로 설치 목록에 포함되는지 내용 확인한 후,
	task를 통해 디버그 모드로 서브 에이전트 구동한 후 (claude --debug), 최대 2분 동안 관찰한 후, 그 동안의 디버그 메시지(에러 시 관련 내용이 출력됨)를 확인하고 /mcp 를 통해 실제 작동여부를 반드시 확인할 것



** MCP 서버 제거가 필요할 때 예시: **
claude mcp remove youtube-mcp
