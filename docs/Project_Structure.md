# PCB 불량 검사 프로젝트 구조 가이드

## 권장 프로젝트 폴더 구조

```
C:\work_project\                         # Windows 경로
~/work_project/                          # WSL2 경로 (실제 작업 공간)
│
├── README.md                            # 프로젝트 개요 및 사용법
├── requirements.txt                     # Python 패키지 의존성
├── .gitignore                           # Git 무시 파일 목록
│
├── docs/                                # 📚 문서
│   ├── PCB_Defect_Detection_Project.md # 전체 프로젝트 로드맵
│   ├── Phase1_YOLO_Setup.md            # YOLO 환경 구축 가이드
│   ├── Dataset_Guide.md                # 데이터셋 가이드
│   ├── Project_Structure.md            # 프로젝트 구조 설명 (이 파일)
│   ├── CSharp_WinForms_Guide.md        # C# WinForms 개발 기본 가이드
│   ├── CSharp_WinForms_Design_Specification.md  # C# WinForms UI 상세 설계
│   ├── RaspberryPi_Setup.md            # 라즈베리파이 클라이언트 설정
│   ├── Remote_Network_Setup.md         # 원격 네트워크 연결 가이드 (Tailscale VPN)
│   ├── MySQL_Database_Design.md        # MySQL 데이터베이스 스키마
│   ├── Flask_Server_Setup.md           # Flask 추론 서버 설정
│   ├── meeting_notes/                  # 회의 및 진행 노트
│   │   ├── 2025-10-22_kickoff.md
│   │   └── weekly_progress.md
│   └── references/                     # 참고 자료
│       ├── papers/                     # 논문 PDF
│       │   ├── yolov8_paper.pdf
│       │   ├── padim_paper.pdf
│       │   └── deeppcb_paper.pdf
│       └── tutorials/                  # 튜토리얼 링크 모음
│           └── resources.md
│
├── data/                                # 📊 데이터셋
│   ├── raw/                            # 원본 데이터 (다운로드한 그대로)
│   │   ├── DeepPCB/
│   │   │   ├── PCBData/
│   │   │   │   ├── train/
│   │   │   │   └── test/
│   │   │   └── README.md
│   │   ├── kaggle_pcb_defects/
│   │   │   ├── images/
│   │   │   ├── annotations.csv
│   │   │   └── README.md
│   │   └── roboflow_pcb/
│   │       └── (YOLO 형식으로 다운로드)
│   │
│   ├── processed/                      # 전처리된 데이터 (YOLO 형식)
│   │   ├── train/
│   │   │   ├── images/
│   │   │   │   ├── img_001.jpg
│   │   │   │   ├── img_002.jpg
│   │   │   │   └── ...
│   │   │   └── labels/
│   │   │       ├── img_001.txt
│   │   │       ├── img_002.txt
│   │   │       └── ...
│   │   ├── val/
│   │   │   ├── images/
│   │   │   └── labels/
│   │   └── test/
│   │       ├── images/
│   │       └── labels/
│   │
│   ├── augmented/                      # 증강된 데이터 (선택)
│   │   └── ...
│   │
│   ├── anomaly_data/                   # 이상 탐지용 데이터
│   │   ├── normal/                     # 정상 이미지만
│   │   └── abnormal/                   # 불량 이미지
│   │
│   └── pcb_defects.yaml                # YOLO 학습용 설정 파일
│
├── models/                              # 🤖 학습된 모델
│   ├── yolo/
│   │   ├── experiments/                # 실험별 모델
│   │   │   ├── exp1_yolov8n/
│   │   │   │   ├── weights/
│   │   │   │   │   ├── best.pt
│   │   │   │   │   └── last.pt
│   │   │   │   ├── results.png
│   │   │   │   ├── confusion_matrix.png
│   │   │   │   └── args.yaml
│   │   │   ├── exp2_yolov8s/
│   │   │   └── exp3_yolov8m/
│   │   │
│   │   └── final/                      # 최종 선정 모델
│   │       ├── yolo_best.pt
│   │       └── model_info.txt
│   │
│   ├── anomaly/                        # 이상 탐지 모델
│   │   ├── padim/
│   │   │   ├── model.pth
│   │   │   └── config.yaml
│   │   ├── patchcore/
│   │   └── autoencoder/
│   │
│   └── hybrid/                         # 하이브리드 모델
│       ├── ensemble_config.yaml
│       └── fusion_weights.json
│
├── notebooks/                           # 📓 Jupyter 노트북
│   ├── 01_data_exploration.ipynb       # 데이터 탐색 및 시각화
│   ├── 02_data_preprocessing.ipynb     # 데이터 전처리
│   ├── 03_yolo_training.ipynb          # YOLO 학습 실험
│   ├── 04_yolo_evaluation.ipynb        # YOLO 성능 평가
│   ├── 05_anomaly_detection.ipynb      # 이상 탐지 모델 실험
│   ├── 06_hybrid_system.ipynb          # 하이브리드 시스템 통합
│   └── 07_final_results.ipynb          # 최종 결과 정리
│
├── src/                                 # 💻 소스 코드
│   ├── __init__.py
│   │
│   ├── data/                           # 데이터 처리
│   │   ├── __init__.py
│   │   ├── download_data.py            # 데이터셋 다운로드
│   │   ├── preprocess.py               # 전처리 (리사이징, 정규화)
│   │   ├── augmentation.py             # 데이터 증강
│   │   ├── convert_voc_to_yolo.py      # 어노테이션 변환
│   │   ├── split_dataset.py            # Train/Val/Test 분할
│   │   └── visualize.py                # 데이터 시각화
│   │
│   ├── models/                         # 모델 정의
│   │   ├── __init__.py
│   │   ├── yolo_detector.py            # YOLO 래퍼 클래스
│   │   ├── anomaly_detector.py         # 이상 탐지 모델 래퍼
│   │   ├── padim_model.py              # PaDiM 구현
│   │   ├── patchcore_model.py          # PatchCore 구현
│   │   ├── autoencoder_model.py        # AutoEncoder 구현
│   │   └── hybrid_model.py             # 하이브리드 시스템
│   │
│   ├── training/                       # 학습 관련
│   │   ├── __init__.py
│   │   ├── train_yolo.py               # YOLO 학습 스크립트
│   │   ├── train_anomaly.py            # 이상 탐지 학습
│   │   ├── callbacks.py                # 커스텀 콜백 (로깅 등)
│   │   └── schedulers.py               # Learning rate scheduler
│   │
│   ├── evaluation/                     # 평가 관련
│   │   ├── __init__.py
│   │   ├── metrics.py                  # 평가 지표 (mAP, Precision, Recall)
│   │   ├── evaluate_yolo.py            # YOLO 평가
│   │   ├── evaluate_anomaly.py         # 이상 탐지 평가
│   │   └── compare_models.py           # 모델 비교
│   │
│   ├── inference/                      # 추론 관련
│   │   ├── __init__.py
│   │   ├── yolo_inference.py           # YOLO 추론
│   │   ├── anomaly_inference.py        # 이상 탐지 추론
│   │   ├── hybrid_inference.py         # 하이브리드 추론
│   │   └── postprocess.py              # 후처리 (NMS, 결과 융합)
│   │
│   ├── server/                         # 🌐 Flask 웹서버 (실시간 추론)
│   │   ├── __init__.py
│   │   ├── app.py                      # Flask 메인 애플리케이션
│   │   ├── inference.py                # AI 추론 엔진
│   │   ├── config.py                   # 서버 설정
│   │   ├── routes/                     # API 라우트
│   │   │   ├── __init__.py
│   │   │   ├── predict.py              # 추론 API
│   │   │   └── health.py               # 헬스 체크 API
│   │   └── database.py                 # 데이터베이스 (검사 이력)
│   │
│   ├── client/                         # 📹 웹캠 클라이언트
│   │   ├── __init__.py
│   │   ├── camera_client.py            # 웹캠 프레임 전송
│   │   ├── config.py                   # 클라이언트 설정
│   │   └── frame_processor.py          # 프레임 전처리
│   │
│   ├── utils/                          # 유틸리티
│   │   ├── __init__.py
│   │   ├── config.py                   # 설정 관리
│   │   ├── logger.py                   # 로깅
│   │   ├── visualize.py                # 결과 시각화
│   │   ├── file_utils.py               # 파일 입출력
│   │   └── metrics_utils.py            # 지표 계산 헬퍼
│   │
│   └── main.py                         # 메인 실행 스크립트
│
├── scripts/                             # 🔧 실행 스크립트
│   ├── setup_environment.sh            # 환경 구축 스크립트
│   ├── download_datasets.sh            # 데이터셋 다운로드
│   ├── preprocess_data.sh              # 전처리 실행
│   ├── train_yolo.sh                   # YOLO 학습 실행
│   ├── train_anomaly.sh                # 이상 탐지 학습
│   ├── evaluate.sh                     # 모델 평가
│   ├── inference.sh                    # 추론 실행
│   ├── export_model.sh                 # 모델 내보내기 (ONNX, TensorRT)
│   ├── start_server.sh                 # Flask 추론 서버 시작
│   ├── start_camera_client.sh          # 웹캠 클라이언트 시작
│   └── monitor_system.sh               # 시스템 모니터링
│
├── configs/                             # ⚙️ 설정 파일
│   ├── yolo_config.yaml                # YOLO 학습 설정
│   ├── anomaly_config.yaml             # 이상 탐지 설정
│   ├── hybrid_config.yaml              # 하이브리드 시스템 설정
│   ├── data_config.yaml                # 데이터 관련 설정
│   ├── server_config.yaml              # Flask 서버 설정
│   ├── camera_config.yaml              # 웹캠 클라이언트 설정
│   └── robot_arm_config.yaml           # 로봇팔 좌표 설정 ⭐ 신규
│
├── results/                             # 📈 실험 결과
│   ├── figures/                        # 그래프 및 시각화
│   │   ├── training_curves/
│   │   │   ├── exp1_loss.png
│   │   │   ├── exp1_map.png
│   │   │   └── ...
│   │   ├── confusion_matrices/
│   │   ├── pr_curves/
│   │   └── class_distribution.png
│   │
│   ├── metrics/                        # 성능 지표
│   │   ├── yolo_metrics.csv
│   │   ├── anomaly_metrics.csv
│   │   ├── hybrid_metrics.csv
│   │   └── comparison.xlsx
│   │
│   ├── predictions/                    # 예측 결과
│   │   ├── test_images/
│   │   │   ├── img_001_pred.jpg
│   │   │   ├── img_002_pred.jpg
│   │   │   └── ...
│   │   └── videos/
│   │       └── demo_video.mp4
│   │
│   └── reports/                        # 실험 리포트
│       ├── experiment_log.md
│       └── final_report.pdf
│
├── tests/                               # 🧪 테스트 코드
│   ├── __init__.py
│   ├── test_data_preprocessing.py
│   ├── test_yolo_model.py
│   ├── test_anomaly_model.py
│   └── test_hybrid_inference.py
│
├── logs/                                # 📝 로그
│   ├── training_logs/
│   │   ├── yolo_train_20251022.log
│   │   └── anomaly_train_20251022.log
│   ├── inference_logs/
│   │   └── inference_20251022.log
│   ├── server_logs/                    # Flask 서버 로그
│   │   └── server_20251022.log
│   └── camera_logs/                    # 웹캠 클라이언트 로그
│       ├── camera_left_20251022.log
│       └── camera_right_20251022.log
│
├── csharp_winforms/                     # 🖥️ C# WinForms 모니터링 앱
│   └── PCB_Inspection_Monitor/
│       ├── PCB_Inspection_Monitor.sln  # Visual Studio 솔루션
│       ├── PCB_Inspection_Monitor.csproj
│       ├── Forms/                      # UI 폼
│       │   ├── MainForm.cs             # 메인 대시보드
│       │   ├── MainForm.Designer.cs
│       │   ├── InspectionHistoryForm.cs # 검사 이력 조회
│       │   ├── DefectImageViewerForm.cs # 불량 이미지 뷰어
│       │   ├── StatisticsForm.cs       # 통계 화면
│       │   └── SettingsForm.cs         # 시스템 설정
│       ├── Models/                     # 데이터 모델
│       │   ├── Inspection.cs
│       │   ├── DefectImage.cs
│       │   ├── Statistics.cs
│       │   └── SystemStatus.cs
│       ├── Services/                   # 서비스 계층
│       │   ├── ApiService.cs           # REST API 통신
│       │   ├── DatabaseService.cs      # MySQL 연동
│       │   └── ImageService.cs         # 이미지 처리
│       ├── Utils/                      # 유틸리티
│       │   ├── Config.cs
│       │   └── Logger.cs
│       ├── App.config                  # 앱 설정
│       ├── Program.cs                  # 진입점
│       └── packages.config             # NuGet 패키지
│
├── raspberry_pi/                        # 🍓 라즈베리파이 클라이언트
│   ├── camera_client.py                # 웹캠 + GPIO + 로봇팔 통합 클라이언트
│   ├── gpio_controller.py              # GPIO 제어 모듈
│   ├── serial_controller.py            # Arduino 시리얼 통신 모듈 ⭐ 신규
│   ├── config.py                       # 설정 파일
│   ├── test_camera.py                  # 카메라 테스트
│   ├── test_gpio.py                    # GPIO 테스트
│   ├── test_serial.py                  # 시리얼 통신 테스트 ⭐ 신규
│   ├── start.sh                        # 자동 시작 스크립트
│   ├── camera-client.service           # systemd 서비스 파일
│   └── requirements_rpi.txt            # 라즈베리파이용 패키지
│
├── arduino/                             # 🤖 Arduino 로봇팔 제어 ⭐ 신규
│   ├── robot_arm_controller/           # Arduino 스케치
│   │   ├── robot_arm_controller.ino    # 메인 스케치 파일
│   │   ├── config.h                    # 설정 (핀, 좌표 테이블)
│   │   ├── servo_control.h             # 서보 모터 제어
│   │   ├── serial_handler.h            # 시리얼 통신 핸들러
│   │   └── box_manager.h               # 박스 좌표 관리
│   ├── libraries/                      # 필요한 라이브러리
│   │   ├── Servo/                      # 서보 라이브러리
│   │   └── ArduinoJson/                # JSON 파싱 라이브러리
│   ├── test_sketches/                  # 테스트용 스케치
│   │   ├── test_servo.ino              # 서보 테스트
│   │   └── test_serial.ino             # 시리얼 테스트
│   └── README.md                       # Arduino 설정 가이드
│
├── database/                            # 🗄️ MySQL 데이터베이스
│   ├── schemas/                        # 스키마 정의
│   │   ├── create_database.sql         # 데이터베이스 생성
│   │   ├── create_tables.sql           # 테이블 생성
│   │   ├── create_views.sql            # 뷰 생성
│   │   ├── create_procedures.sql       # 저장 프로시저
│   │   └── create_triggers.sql         # 트리거
│   ├── migrations/                     # 마이그레이션 스크립트
│   │   ├── 001_initial_schema.sql
│   │   ├── 002_add_indexes.sql
│   │   └── 003_update_constraints.sql
│   ├── seed_data/                      # 초기 데이터
│   │   ├── system_config.sql
│   │   └── users.sql
│   ├── backups/                        # 데이터베이스 백업
│   │   └── backup_20251022.sql
│   └── backup_database.sh              # 백업 스크립트
│
├── checkpoints/                         # 💾 체크포인트 (백업)
│   ├── yolo_checkpoint_epoch50.pt
│   └── anomaly_checkpoint_epoch30.pt
│
├── docker/                              # 🐳 Docker 설정 (선택)
│   ├── Dockerfile
│   └── docker-compose.yml
│
└── presentation/                        # 🎤 발표 자료
    ├── slides.pptx
    ├── demo_video.mp4
    └── thesis_draft.docx
```

---

## 폴더별 상세 설명

### 1. `docs/` - 문서
- **목적**: 프로젝트 관련 모든 문서 보관
- **주요 파일**:
  - `PCB_Defect_Detection_Project.md`: 전체 프로젝트 로드맵
  - `Phase1_YOLO_Setup.md`: YOLO 환경 구축 가이드
  - `Dataset_Guide.md`: 데이터셋 수집 및 전처리 가이드

### 2. `data/` - 데이터셋
- **raw/**: 다운로드한 원본 데이터 (수정하지 않음)
- **processed/**: YOLO 형식으로 전처리된 데이터
  - `train/`, `val/`, `test/` 각각 `images/`와 `labels/` 폴더 포함
- **pcb_defects.yaml**: YOLO 학습용 데이터 설정 파일

**중요**: `raw/` 데이터는 절대 수정하지 말고, 전처리는 항상 `processed/`에 저장

### 3. `models/` - 학습된 모델
- **yolo/experiments/**: 실험별로 폴더 분리 (exp1, exp2, ...)
- **yolo/final/**: 최종 선정된 최고 성능 모델
- **anomaly/**: 이상 탐지 모델 저장
- **hybrid/**: 하이브리드 시스템 설정

**팁**: 각 실험마다 폴더를 만들어 결과를 정리하면 나중에 비교 용이

### 4. `notebooks/` - Jupyter 노트북
- **용도**: 데이터 탐색, 프로토타이핑, 실험, 결과 분석
- **권장 순서**:
  1. `01_data_exploration.ipynb`: 데이터셋 탐색
  2. `02_data_preprocessing.ipynb`: 전처리 테스트
  3. `03_yolo_training.ipynb`: YOLO 학습 실험
  4. `04_yolo_evaluation.ipynb`: 성능 평가
  5. `05_anomaly_detection.ipynb`: 이상 탐지 모델
  6. `06_hybrid_system.ipynb`: 하이브리드 통합
  7. `07_final_results.ipynb`: 최종 결과 정리

### 5. `src/` - 소스 코드
- **모듈화된 코드**: 재사용 가능한 함수와 클래스
- **하위 폴더**:
  - `data/`: 데이터 처리
  - `models/`: 모델 정의
  - `training/`: 학습 로직
  - `evaluation/`: 평가 로직
  - `inference/`: 추론 로직
  - `utils/`: 유틸리티 함수

**Best Practice**: Jupyter 노트북에서 프로토타이핑 → 검증된 코드는 `src/`로 이동

### 6. `scripts/` - 실행 스크립트
- **bash 스크립트**: 반복 작업 자동화
- **예시**:
  ```bash
  # scripts/train_yolo.sh
  #!/bin/bash
  python src/training/train_yolo.py \
      --data data/pcb_defects.yaml \
      --model yolov8s.pt \
      --epochs 100 \
      --batch 16 \
      --imgsz 640 \
      --name exp_yolov8s_100epochs
  ```

### 7. `configs/` - 설정 파일
- **YAML 형식**: 하이퍼파라미터 및 설정 관리
- **장점**: 코드 수정 없이 설정만 변경 가능

**예시** (`configs/yolo_config.yaml`):
```yaml
model: yolov8s.pt
data: data/pcb_defects.yaml
epochs: 100
batch_size: 16
image_size: 640
learning_rate: 0.01
optimizer: SGD
device: 0  # GPU ID
```

### 8. `results/` - 실험 결과
- **figures/**: 모든 시각화 저장
- **metrics/**: CSV, Excel 형식의 성능 지표
- **predictions/**: 테스트 이미지에 대한 예측 결과
- **reports/**: 실험 리포트

**팁**: 실험마다 날짜와 버전을 파일명에 포함 (예: `yolo_metrics_20251022_v1.csv`)

### 9. `tests/` - 테스트 코드
- **단위 테스트**: 각 모듈의 정확성 검증
- **pytest** 사용 권장

**예시**:
```python
# tests/test_data_preprocessing.py
import pytest
from src.data.preprocess import resize_image

def test_resize_image():
    # 테스트 코드
    pass
```

### 10. `logs/` - 로그
- **학습 로그**: 학습 과정 기록
- **추론 로그**: 추론 시 발생한 이벤트 기록

### 11. `csharp_winforms/` - C# WinForms 모니터링 앱
- **목적**: Windows PC에서 실행되는 실시간 모니터링 대시보드
- **프레임워크**: .NET 6+
- **주요 기능**:
  - MySQL 데이터베이스 연동하여 검사 이력 조회
  - Flask 서버 REST API 호출
  - LiveCharts를 이용한 실시간 통계 그래프
  - 불량 이미지 뷰어
- **참고 문서**: `CSharp_WinForms_Guide.md`

### 12. `raspberry_pi/` - 라즈베리파이 클라이언트
- **목적**: 웹캠 프레임 캡처, GPIO 제어, Arduino 로봇팔 제어
- **하드웨어**: Raspberry Pi 4 Model B
- **주요 기능**:
  - 웹캠 프레임 캡처 및 Flask 서버로 전송
  - Flask 서버 응답 기반 GPIO 신호 출력
  - 4채널 릴레이 모듈 제어
  - USB 시리얼 통신 (Arduino Mega 제어) ⭐ 신규
  - systemd 서비스로 자동 시작
- **참고 문서**: `RaspberryPi_Setup.md`

### 13. `arduino/` - Arduino 로봇팔 제어 ⭐ 신규
- **목적**: 5-6축 로봇팔 제어 및 PCB 분류
- **하드웨어**: Arduino Mega 2560 + 서보 모터 6개
- **주요 기능**:
  - USB 시리얼 통신 (JSON 프로토콜)
  - 40개 박스 슬롯 좌표 관리
  - PCB 픽업 및 배치 자동화
  - 안전 기능 (충돌 방지, 리미트 스위치)
- **라이브러리**:
  - Servo.h - 서보 모터 제어
  - ArduinoJson.h - JSON 파싱
- **참고 문서**: `Arduino_RobotArm_Setup.md` (신규 작성 필요)

### 14. `database/` - MySQL 데이터베이스
- **목적**: 검사 이력, 통계, 시스템 로그 저장
- **데이터베이스**: MySQL 8.0
- **schemas/**: 테이블, 뷰, 프로시저, 트리거 SQL 스크립트
- **migrations/**: 데이터베이스 스키마 변경 이력
- **seed_data/**: 초기 데이터 (설정, 사용자)
- **backups/**: mysqldump 백업 파일
- **참고 문서**: `MySQL_Database_Design.md`

---

## 폴더 생성 스크립트

### WSL2에서 실행

```bash
#!/bin/bash
# create_project_structure.sh

# 메인 프로젝트 폴더 (이미 존재)
cd ~/work_project

# 문서 폴더
mkdir -p docs/meeting_notes
mkdir -p docs/references/papers
mkdir -p docs/references/tutorials

# 데이터 폴더
mkdir -p data/raw
mkdir -p data/processed/{train,val,test}/{images,labels}
mkdir -p data/augmented
mkdir -p data/anomaly_data/{normal,abnormal}

# 모델 폴더
mkdir -p models/yolo/{experiments,final}
mkdir -p models/anomaly/{padim,patchcore,autoencoder}
mkdir -p models/hybrid

# 노트북 폴더
mkdir -p notebooks

# 소스 코드 폴더
mkdir -p src/{data,models,training,evaluation,inference,utils}
touch src/__init__.py
touch src/data/__init__.py
touch src/models/__init__.py
touch src/training/__init__.py
touch src/evaluation/__init__.py
touch src/inference/__init__.py
touch src/utils/__init__.py

# 스크립트 폴더
mkdir -p scripts

# 설정 폴더
mkdir -p configs

# 결과 폴더
mkdir -p results/{figures,metrics,predictions,reports}
mkdir -p results/figures/{training_curves,confusion_matrices,pr_curves}
mkdir -p results/predictions/{test_images,videos}

# 테스트 폴더
mkdir -p tests
touch tests/__init__.py

# 로그 폴더
mkdir -p logs/{training_logs,inference_logs}

# 체크포인트 폴더
mkdir -p checkpoints

# 발표 자료 폴더
mkdir -p presentation

# C# WinForms 폴더
mkdir -p csharp_winforms/PCB_Inspection_Monitor/{Forms,Models,Services,Utils}

# 라즈베리파이 클라이언트 폴더
mkdir -p raspberry_pi

# 데이터베이스 폴더
mkdir -p database/{schemas,migrations,seed_data,backups}

# 기본 파일 생성
touch README.md
touch requirements.txt
touch .gitignore

echo "프로젝트 폴더 구조 생성 완료!"
echo "작업 디렉토리: $(pwd)"
tree -L 2
```

**실행**:
```bash
cd ~/work_project
bash create_project_structure.sh
```

---

## `.gitignore` 파일 예시

```gitignore
# Python
__pycache__/
*.py[cod]
*$py.class
*.so
.Python
env/
venv/
ENV/
*.egg-info/
dist/
build/

# Jupyter Notebook
.ipynb_checkpoints
*.ipynb_checkpoints

# 데이터
data/raw/
data/processed/
data/augmented/
*.jpg
*.png
*.jpeg
*.mp4
*.avi

# 모델 파일 (대용량)
models/**/*.pt
models/**/*.pth
models/**/*.onnx
models/**/*.engine
checkpoints/

# 로그
logs/
runs/
*.log

# 결과 파일
results/predictions/
*.csv
*.xlsx

# 시스템 파일
.DS_Store
Thumbs.db
*.swp
*.swo

# IDE
.vscode/
.idea/
*.code-workspace

# 환경 변수
.env

# 임시 파일
tmp/
temp/
```

---

## `README.md` 템플릿

```markdown
# PCB Defect Detection using YOLO v8 and Anomaly Detection

## 프로젝트 개요
PCB(인쇄회로기판) 이미지에서 부품 및 납땜 불량을 자동으로 검출하는 하이브리드 딥러닝 시스템

## 주요 기능
- YOLO v8 기반 객체 탐지 (Object Detection)
- 이상 탐지 모델 (Anomaly Detection)
- 병렬 처리를 통한 결과 융합

## 환경 요구사항
- Python 3.10+
- PyTorch 2.0+
- CUDA 11.8+ (GPU 사용 시)

## 설치 방법
```bash
# 저장소 클론
git clone <repository-url>
cd work_project

# 가상환경 생성
conda create -n pcb_defect python=3.10
conda activate pcb_defect

# 패키지 설치
pip install -r requirements.txt
```

## 사용법
### 1. 데이터 준비
```bash
bash scripts/download_datasets.sh
bash scripts/preprocess_data.sh
```

### 2. 모델 학습
```bash
bash scripts/train_yolo.sh
```

### 3. 모델 평가
```bash
bash scripts/evaluate.sh
```

### 4. 추론
```bash
python src/main.py --image path/to/image.jpg
```

## 프로젝트 구조
자세한 내용은 `docs/Project_Structure.md` 참고

## 성능
- YOLO mAP@0.5: 0.XX
- 하이브리드 mAP@0.5: 0.XX

## 참고 자료
- [YOLO v8 Documentation](https://docs.ultralytics.com/)
- [DeepPCB Paper](링크)

## 라이선스
MIT License

## 연락처
- 이름: XXX
- 이메일: xxx@example.com
```

---

## `requirements.txt` 예시

```txt
# Core
torch>=2.0.0
torchvision>=0.15.0
ultralytics>=8.0.0

# Data processing
opencv-python-headless>=4.8.0
pillow>=10.0.0
numpy>=1.24.0
pandas>=2.0.0

# Visualization
matplotlib>=3.7.0
seaborn>=0.12.0

# Training
tensorboard>=2.13.0
tqdm>=4.65.0

# Augmentation
albumentations>=1.3.0

# Anomaly Detection
anomalib>=0.7.0

# Utils
pyyaml>=6.0
scipy>=1.10.0
scikit-learn>=1.3.0

# Testing
pytest>=7.4.0

# Development
jupyter>=1.0.0
ipykernel>=6.25.0
black>=23.7.0
flake8>=6.1.0
```

---

## 문서 목록 및 설명

프로젝트의 각 문서는 특정 영역을 담당하며, 상호 참조를 통해 통합된 가이드를 제공합니다:

### 핵심 문서

1. **PCB_Defect_Detection_Project.md** - 전체 시스템 아키텍처 및 프로젝트 로드맵
   - 시스템 개요, 하드웨어/소프트웨어 구성
   - 통신 프로토콜 및 데이터 흐름
   - Phase별 체크리스트

2. **Project_Structure.md** (이 문서) - 프로젝트 폴더 구조 및 개발 워크플로우
   - 권장 폴더 구조
   - 파일 생성 스크립트
   - 의존성 패키지 목록

### 개발 가이드 문서

3. **CSharp_WinForms_Guide.md** - C# WinForms 모니터링 앱 기본 개발 가이드
   - Visual Studio 프로젝트 생성
   - MySQL 연동, REST API 통신
   - 기본 UI 구현 (대시보드, 검사 이력)

4. **CSharp_WinForms_Design_Specification.md** - C# WinForms UI 상세 설계 명세
   - 사용자 권한 시스템 (Admin/Operator/Viewer)
   - 7개 화면 상세 설계 (와이어프레임 + 코드)
   - Excel 내보내기 기능 (ClosedXML)
   - 보안 설계 (BCrypt, 세션 관리)

5. **RaspberryPi_Setup.md** - 라즈베리파이 카메라 클라이언트 설정
   - 웹캠 + GPIO 통합 클라이언트
   - systemd 자동 시작 설정
   - 릴레이 모듈 제어

6. **Remote_Network_Setup.md** - 원격 네트워크 연결 가이드
   - Tailscale VPN 설치 및 설정 (모든 장비)
   - 네트워크 지연 측정 및 최적화
   - 방화벽 및 보안 설정
   - 트러블슈팅 가이드

7. **MySQL_Database_Design.md** - MySQL 데이터베이스 스키마 설계
   - 테이블 스키마 (inspections, users, system_logs 등)
   - 인덱스 및 저장 프로시저
   - 백업 전략

8. **Flask_Server_Setup.md** - Flask 추론 서버 설정
   - YOLO + 이상 탐지 통합
   - REST API 엔드포인트
   - MySQL 연동

### 학습 관련 문서

9. **Phase1_YOLO_Setup.md** - YOLO v8 환경 구축 및 학습 가이드
10. **Dataset_Guide.md** - 데이터셋 수집, 라벨링, 전처리 가이드

---

## 개발 워크플로우

### 1. 새로운 실험 시작

```bash
# 1. 브랜치 생성 (Git 사용 시)
git checkout -b experiment/yolov8s_100epochs

# 2. 설정 파일 작성
vi configs/yolo_config_exp1.yaml

# 3. 학습 실행
bash scripts/train_yolo.sh

# 4. 결과 정리
jupyter notebook notebooks/04_yolo_evaluation.ipynb

# 5. 커밋
git add .
git commit -m "Add experiment: YOLOv8s 100 epochs"
```

### 2. 실험 결과 관리

```bash
# 실험 폴더 구조
models/yolo/experiments/
├── exp1_yolov8n_baseline/
│   ├── weights/best.pt
│   ├── results.png
│   └── notes.txt              # 실험 노트
├── exp2_yolov8s_augmented/
└── exp3_yolov8m_tuned/
```

**실험 노트 예시** (`notes.txt`):
```
실험명: YOLOv8s Baseline
날짜: 2025-10-25
목적: 기본 YOLOv8s 성능 측정

설정:
- Model: YOLOv8s
- Epochs: 100
- Batch size: 16
- Image size: 640
- Augmentation: YOLO default

결과:
- mAP@0.5: 0.82
- mAP@0.5:0.95: 0.61
- Training time: 2.5 hours

관찰:
- 'spur' 클래스의 recall이 낮음 (0.65)
- Overfitting 징후 없음
- 다음 실험: 데이터 증강 강화
```

---

## 팀 협업 가이드 (선택)

### 코드 스타일
```bash
# Black으로 코드 포맷팅
black src/

# Flake8으로 린팅
flake8 src/
```

### 커밋 메시지 컨벤션
```
feat: 새로운 기능 추가
fix: 버그 수정
docs: 문서 수정
style: 코드 포맷팅
refactor: 코드 리팩토링
test: 테스트 추가
chore: 기타 작업
```

**예시**:
```bash
git commit -m "feat: Add PaDiM anomaly detection model"
git commit -m "fix: Fix YOLO inference batch processing bug"
git commit -m "docs: Update Dataset_Guide.md with Kaggle dataset"
```

---

## 다음 단계

프로젝트 구조가 준비되면:

1. **환경 구축**
   - `Phase1_YOLO_Setup.md` 따라 YOLO 설치
   - 폴더 구조 생성 스크립트 실행

2. **데이터 준비**
   - `Dataset_Guide.md` 참고하여 데이터 다운로드
   - `data/processed/` 폴더에 전처리

3. **첫 실험 시작**
   - Jupyter 노트북으로 프로토타이핑
   - 검증된 코드는 `src/`로 이동

---

**작성일**: 2025-10-22
**버전**: 1.0
**다음 단계**: Phase 1 YOLO 환경 구축 시작
