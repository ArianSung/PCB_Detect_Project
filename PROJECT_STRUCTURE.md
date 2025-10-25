# PCB 불량 검사 프로젝트 구조

**생성일**: 2025-10-25
**업데이트**: Phase 2 시작

---

## 📁 프로젝트 폴더 구조

```
work_project/
├── CLAUDE.md                      # Claude Code 가이드
├── README.md                      # 프로젝트 개요
├── PROJECT_STRUCTURE.md           # 프로젝트 구조 (이 파일)
├── requirements.txt               # Python 패키지 목록
├── .gitignore                     # Git 무시 파일
│
├── docs/                          # 📚 프로젝트 문서 (11개)
│   ├── PCB_Defect_Detection_Project.md  # 전체 로드맵
│   ├── Phase1_YOLO_Setup.md
│   ├── Dataset_Guide.md
│   ├── Project_Structure.md
│   ├── CSharp_WinForms_Guide.md
│   ├── CSharp_WinForms_Design_Specification.md
│   ├── RaspberryPi_Setup.md
│   ├── Remote_Network_Setup.md
│   ├── MySQL_Database_Design.md
│   ├── Flask_Server_Setup.md
│   └── Logging_Strategy.md
│
├── src/                           # 💻 소스 코드
│   ├── __init__.py
│   ├── data/                     # 데이터 처리
│   │   ├── __init__.py
│   │   ├── download_data.py      # (미래) 데이터 다운로드
│   │   ├── preprocess.py         # (미래) 전처리
│   │   └── ...
│   ├── models/                   # 모델 정의
│   │   ├── __init__.py
│   │   ├── yolo_detector.py      # (미래) YOLO 래퍼
│   │   └── ...
│   ├── training/                 # 학습
│   │   ├── __init__.py
│   │   ├── train_yolo.py         # (미래) YOLO 학습
│   │   └── ...
│   ├── evaluation/               # 평가
│   │   ├── __init__.py
│   │   └── ...
│   ├── inference/                # 추론
│   │   ├── __init__.py
│   │   └── ...
│   ├── server/                   # Flask 서버 (Phase 5)
│   │   ├── __init__.py
│   │   ├── app.py                # (미래) Flask 메인
│   │   ├── routes/
│   │   │   └── __init__.py
│   │   └── ...
│   ├── client/                   # 웹캠 클라이언트
│   │   ├── __init__.py
│   │   └── ...
│   └── utils/                    # 유틸리티
│       ├── __init__.py
│       └── ...
│
├── configs/                       # ⚙️ 설정 파일
│   ├── yolo_config.yaml          ✅ YOLOv8l 학습 설정
│   ├── server_config.yaml        ✅ Flask 서버 설정
│   └── camera_config.yaml        ✅ 웹캠 클라이언트 설정
│
├── scripts/                       # 🔧 실행 스크립트
│   ├── train_yolo.sh             ✅ YOLO 학습 실행
│   ├── start_server.sh           ✅ Flask 서버 시작
│   └── (미래 추가 예정)
│
├── data/                          # 📊 데이터셋
│   ├── pcb_defects.yaml          ✅ YOLO 데이터셋 설정
│   ├── raw/                      # 원본 데이터
│   ├── processed/                # 전처리 데이터
│   │   ├── train/
│   │   ├── val/
│   │   └── test/
│   └── anomaly_data/             # 이상 탐지용
│
├── models/                        # 🤖 학습된 모델
│   ├── yolo/
│   │   ├── experiments/          # 실험별 모델
│   │   └── final/                # 최종 모델
│   ├── anomaly/                  # 이상 탐지 (Phase 4)
│   └── hybrid/                   # 하이브리드 (Phase 5)
│
├── notebooks/                     # 📓 Jupyter 노트북
│   └── (미래 추가 예정)
│
├── results/                       # 📈 실험 결과
│   ├── figures/                  # 그래프
│   ├── metrics/                  # 성능 지표
│   ├── predictions/              # 예측 결과
│   └── reports/                  # 리포트
│
├── logs/                          # 📝 로그
│   ├── training_logs/
│   ├── inference_logs/
│   ├── server_logs/
│   └── camera_logs/
│
├── tests/                         # 🧪 테스트
│   └── api/                      ✅ Flask API 테스트
│       ├── mock_server.py        # Mock Flask 서버
│       └── test_api_contract.py  # API 계약 테스트
│
├── yolo/                          # 🎯 YOLO 작업 디렉토리 (AI 모델 팀)
│   ├── README.md                 ✅ YOLO 디렉토리 가이드
│   ├── datasets/                 # YOLO 데이터셋 (Git 무시)
│   │   └── coco128/              # COCO128 샘플
│   ├── runs/                     # YOLO 학습 결과 (Git 무시)
│   │   └── detect/
│   ├── test_images/              # 테스트 이미지 (Git 무시)
│   │   └── bus.jpg
│   └── tests/                    ✅ Phase 1 YOLO 테스트
│       ├── README.md
│       ├── PHASE1_TEST_RESULTS.md
│       ├── YOLO11_vs_YOLOv8.md
│       ├── models_backup/        # 백업 모델
│       │   ├── yolov8n.pt
│       │   └── yolo11n.pt
│       └── test_*.py             # 테스트 스크립트
│
├── raspberry_pi/                  # 🍓 라즈베리파이 클라이언트
│   └── .env.example              ✅ 환경 변수 템플릿
│
├── csharp_winforms/              # 🖥️ C# WinForms 앱
│   └── .env.example              ✅ 환경 변수 템플릿
│
├── .github/                       # 📋 GitHub 설정
│   ├── CODEOWNERS                ✅ 코드 소유자 정의
│   └── pull_request_template.md  ✅ PR 템플릿
│
└── database/                      # 🗄️ MySQL 데이터베이스
    └── (미래 추가 예정)
