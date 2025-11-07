# PCB 불량 검사 프로젝트 (졸업 프로젝트)

## 프로젝트 개요

**목표**: 컨베이어 벨트를 통해 들어오는 PCB의 양면을 실시간으로 검사하여 불량을 자동으로 검출하고 분류하는 이중 YOLO 딥러닝 시스템 개발

**핵심 기술**:
- 이중 YOLO v11m 모델 아키텍처 (Dual Model Architecture)
  - **모델 1**: FPIC-Component (부품 검출, 25개 클래스)
  - **모델 2**: SolDef_AI (납땜 불량 검출, 5-6개 클래스)
- 병렬 추론 및 결과 융합 로직 (Flask 서버)
- 실시간 비디오 스트리밍 및 웹 서버 통신 (Flask)
- 양면 동시 검사 및 통합 판정

**시스템 구성**:
- **라즈베리파이 3대** (카메라 클라이언트 + OHT 컨트롤러)
  - **라즈베리파이 1**: 좌측 웹캠 + GPIO 출력 (분류 게이트, LED 제어)
    - Tailscale VPN: 100.x.x.y
  - **라즈베리파이 2**: 우측 웹캠 전용 (카메라만)
    - Tailscale VPN: 100.x.x.z
  - **라즈베리파이 3번 (OHT 컨트롤러)** ⭐ 신규
    - Tailscale VPN: 100.64.1.4 (또는 로컬 예비: 192.168.0.22)
    - X축 스텝모터 제어 (천장 레일 이동)
    - Z축 좌/우 스텝모터 동기 제어 (베드 상하 이동)
    - 서보모터 걸쇠 제어 + 리미트 스위치 6개 감시
    - 긴급 정지 버튼 콜백 처리
    - Flask API 폴링 (OHT 작업 요청 확인, pigpio 기반)
  - Flask Client 실행 (프레임 전송)
  - GPIO 제어: **라즈베리파이 1만** 릴레이/LED 제어 수행
- **GPU PC (추론 서버)** - 원격지 (같은 도시 내)
  - Tailscale VPN: 100.x.x.x
  - Flask 웹서버 (AI 추론)
  - MySQL 데이터베이스 서버
  - REST API 서버
- **Windows PC (C# WinForms)**
  - 실시간 모니터링 대시보드
  - 검사 이력 조회 및 관리
  - 불량 이미지 뷰어
  - 시스템 설정 UI
- **분류 시스템** (GPIO 제어 - **라즈베리파이 1 전용**):
  - 부품 불량 → GPIO 핀 17 (BCM) HIGH (부품 재작업 라인)
  - 납땜 불량 → GPIO 핀 27 (BCM) HIGH (납땜 재작업 라인)
  - 심각한 불량 → GPIO 핀 22 (BCM) HIGH (폐기 라인)
  - 정상 → GPIO 핀 23 (BCM) HIGH (다음 공정)
  - **참고**: Flask 서버가 양면(좌측+우측) 결과를 통합 판정 후, 라즈베리파이 1에만 GPIO 제어 신호 전달

**개발 환경**:
- **GPU PC (추론 서버)**
  - GPU: NVIDIA RTX 4080 Super (16GB VRAM)
  - Windows 11 + WSL2 (Ubuntu) 또는 Linux
  - Python 3.10 (Miniconda)
  - PyTorch + YOLO v11ml (Large 모델)
  - Flask 웹 서버
  - MySQL 8.0 데이터베이스
- **라즈베리파이** (카메라 + GPIO)
  - Raspberry Pi OS (64-bit)
  - Python 3.10+
  - OpenCV + RPi.GPIO
- **Windows PC** (모니터링)
  - C# WinForms (.NET 6+)
  - Visual Studio 2022
  - MySQL Connector

---

## 불량 검출 대상

### 1. 부품 검출 (Component Detection) - FPIC-Component 데이터셋
**모델 1 (좌측 카메라)** - 25개 클래스
- 전자 부품 25종: capacitor, resistor, IC, LED, diode, transistor, connector, inductor, relay, switch, potentiometer, pads 등
- 부품 존재 여부 확인
- 부품 위치 정확도 검증
- 잘못된 부품 검출

**데이터셋 정보**:
- 출처: FPIC-Component (IIT, India)
- 이미지 수: 6,260장
- 라벨 객체 수: 29,639개
- 균형 잡힌 분포

### 2. 납땜 불량 검출 (Soldering Defects) - SolDef_AI 데이터셋
**모델 2 (우측 카메라)** - 5-6개 클래스
- **no_good**: 일반적인 납땜 불량
- **exc_solder**: 과다 납땜 (Excessive Solder)
- **spike**: 납땜 스파이크
- **poor_solder**: 불충분한 납땜 (Poor Solder Joint)
- **solder_bridge**: 납땜 브릿지 (치명적 결함)
- **tombstone**: 툼스톤 현상 (선택)

**데이터셋 정보**:
- 출처: SolDef_AI (Roboflow)
- 이미지 수: 1,150장 (429장 Roboflow 버전)
- 우주 항공 표준 (ECSS-Q-ST-70-38C) 기반
- 고품질 어노테이션

### 3. 결과 융합 로직
Flask 서버에서 두 모델의 결과를 통합하여 최종 판정:
- **정상 (normal)**: 부품 + 납땜 모두 정상
- **부품 불량 (component_defect)**: 부품 검출 문제
- **납땜 불량 (solder_defect)**: 납땜 품질 문제
- **폐기 (discard)**: 심각한 불량 (양면 모두 문제 또는 치명적 결함)

---

## 프로젝트 전체 로드맵

### Phase 1: 환경 구축 및 YOLO v11m 테스트 (1-2주) ⭐ 현재 단계

#### 체크리스트
- [x] WSL2 설치
- [x] Miniconda 설치 및 가상환경 구축
- [x] YOLO v11m 설치
  - [x] PyTorch 설치
  - [x] Ultralytics 패키지 설치
  - [x] 설치 확인 테스트
- [x] YOLO v11m 기본 튜토리얼 실습
  - [x] 이미지 추론 테스트
  - [x] 비디오 추론 테스트
  - [x] 사전 학습 모델 테스트
- [x] 공개 데이터셋으로 학습 테스트
  - [x] COCO dataset 샘플 학습
  - [x] 커스텀 데이터 학습 연습

**참고 문서**: `Phase1_YOLO_Setup.md`

---

### Phase 2: PCB 데이터 준비 (2-3주) ⭐ 업데이트

#### 선정 데이터셋
**이중 모델 아키텍처를 위한 전문 데이터셋 사용**:
1. **FPIC-Component** (부품 검출) - 모델 1
2. **SolDef_AI** (납땜 불량) - 모델 2

#### 체크리스트
- [x] 검증된 PCB 데이터셋 선정
  - [x] FPIC-Component 데이터셋 확인 (IIT, India)
  - [x] SolDef_AI 데이터셋 확인 (Roboflow, 우주항공 표준)
  - [x] 데이터셋 품질 및 균형 검증
- [ ] 데이터셋 1: FPIC-Component 다운로드 및 준비
  - [ ] 데이터셋 다운로드 (6,260 이미지)
  - [ ] 25개 부품 클래스 확인
  - [ ] YOLO 형식 변환 (필요 시)
  - [ ] Train/Val/Test 분할 확인
- [ ] 데이터셋 2: SolDef_AI 다운로드 및 준비
  - [ ] Roboflow에서 다운로드 (429-1,150 이미지)
  - [ ] 5-6개 납땜 클래스 확인
  - [ ] YOLO 형식 확인
  - [ ] Train/Val/Test 분할 확인
- [ ] 데이터 품질 검증
  - [ ] 이미지 해상도 확인
  - [ ] 라벨 형식 검증 (YOLO format)
  - [ ] 클래스 분포 분석
  - [ ] 중복 이미지 확인
- [ ] 데이터 증강 전략 (선택)
  - [ ] 회전, 반전, 밝기 조정
  - [ ] 노이즈 추가
  - [ ] Mosaic augmentation

**참고 문서**: `Dataset_Guide.md`, `Dual_Model_Architecture.md`

---

### Phase 3: 이중 YOLO 모델 학습 및 최적화 (3-4주) ⭐ 업데이트

**핵심 변경**: 두 개의 독립적인 YOLO 모델을 각각 학습

#### 모델 1: 부품 검출 모델 (FPIC-Component)
- [x] 데이터셋: FPIC-Component (6,260 이미지, 25 클래스)
- [ ] YOLOv11m 선택 (정확도 우선, RTX 4080 Super 최적화)
- [ ] 학습 설정
  - [ ] Epochs: 100-150
  - [ ] Batch size: 16-32 (VRAM 16GB 활용)
  - [ ] Image size: 640
  - [ ] Optimizer: AdamW
  - [ ] Learning rate: 0.001
- [ ] 성능 평가
  - [ ] mAP@0.5 목표: > 0.85
  - [ ] 부품별 검출 정확도 분석
  - [ ] Precision/Recall 분석
- [ ] 모델 저장: `models/fpic_component_best.pt`

#### 모델 2: 납땜 불량 모델 (SolDef_AI)
- [x] 데이터셋: SolDef_AI (1,150 이미지, 5-6 클래스)
- [ ] YOLOv11m 선택 (정확도 우선)
- [ ] 학습 설정
  - [ ] Epochs: 100-150
  - [ ] Batch size: 16-32
  - [ ] Image size: 640
  - [ ] Optimizer: AdamW
  - [ ] Learning rate: 0.001
  - [ ] Class weights 조정 (필요 시)
- [ ] 성능 평가
  - [ ] mAP@0.5 목표: > 0.90 (클래스 수 적어 높은 정확도 기대)
  - [ ] 납땜 불량 타입별 정확도
  - [ ] False Positive 최소화 중요
- [ ] 모델 저장: `models/soldef_ai_best.pt`

#### 공통 최적화
- [ ] 하이퍼파라미터 튜닝
  - [ ] Learning rate 스케줄러
  - [ ] Warm-up epochs
  - [ ] Weight decay 조정
- [ ] 추론 최적화
  - [ ] FP16 (Half Precision) 적용 ⭐ 권장
  - [ ] Batch inference (좌우 동시 추론)
  - [ ] TensorRT 변환 (선택)
- [ ] 성능 벤치마크
  - [ ] 단일 모델 추론 시간 측정
  - [ ] 병렬 추론 시간 측정 (목표: 80-100ms)
  - [ ] GPU 메모리 사용량 (목표: < 8GB)

**예상 결과**:
- 부품 모델: mAP@0.5 > 0.85
- 납땜 모델: mAP@0.5 > 0.90
- 총 추론 시간: 80-100ms (병렬)

---

### Phase 4: ~~이상 탐지 모델~~ → 이중 YOLO 모델로 대체 ⭐ 아키텍처 변경

**⚠️ 중요**: 이상 탐지 모델 접근 방식을 **이중 YOLO 모델 아키텍처**로 변경

**변경 이유**:
1. 데이터셋 불균형 문제 해결 불가 (22개 클래스, 451.9:1 불균형)
2. 검증된 전문 데이터셋 활용 (FPIC-Component, SolDef_AI)
3. 양면 검사 컨셉에 완벽 부합 (좌측=부품, 우측=납땜)
4. 유지보수 및 확장성 향상

**새로운 아키텍처**:
- **모델 1**: FPIC-Component (부품 검출, 25개 클래스) - 좌측 카메라
- **모델 2**: SolDef_AI (납땜 불량, 5-6개 클래스) - 우측 카메라
- **Flask 서버**: 결과 융합 로직 (단순 if-else, <5ms)

**기술적 장점**:
- ✅ 전문화된 모델 → 높은 정확도
- ✅ 병렬 추론 → 빠른 속도 (80-100ms)
- ✅ 독립적 학습 → 빠른 개발
- ✅ 유연한 융합 로직 → 쉬운 조정

**프로젝트 장점**:
- ✅ 양면 검사 컨셉 완벽 구현
- ✅ 4가지 분류 명확 (정상/부품불량/납땜불량/폐기)
- ✅ GPIO 제어 단순 (하나의 신호만)
- ✅ 발표 효과 극대화

**참고 문서**: `Dual_Model_Architecture.md` (상세 설계 문서)

---

**⚠️ 구 접근 방식 (아카이브)**

이전에는 YOLO + Anomalib 이상 탐지 하이브리드 방식을 고려했으나, 다음 이유로 폐기:
- 데이터셋 불균형 문제 지속
- 이상 탐지 모델의 실시간 처리 성능 불확실
- 두 모델 융합 복잡도 증가
- 유지보수 어려움

**이상 탐지 모델은 이 프로젝트에서 사용하지 않습니다.**

---

### Phase 5: 이중 모델 시스템 통합 및 웹서버 구축 (3-4주) ⭐ 업데이트

#### 실시간 검사 시스템 아키텍처

```
[컨베이어 벨트 시스템 - 양면 동시 검사]
    │
    ├─ 웹캠 1 (좌측) ──→ [라즈베리파이 1] (100.x.x.y)
    │   앞면(부품면)       │
    │                      ├──→ Flask Client (부품 프레임 전송)
    │                      ├──→ GPIO 제어 모듈 (LED/릴레이) ⭐
    │                      └──→ USB 시리얼 통신 (Arduino Mega 제어) ⭐
    │                             │
    └─ 웹캠 2 (우측) ──→ [라즈베리파이 2] (100.x.x.z)
        뒷면(납땜면)       │
                         │        └──→ Flask Client (납땜 프레임 전송만)
                         │
                ┌────────┴─────────┐
                │                  │
        HTTP POST             HTTP POST
        (left_frame)          (right_frame)
                │                  │
                ↓                  ↓
        ┌─────────────────────────────────────────┐
        │  Flask 추론 서버 (GPU PC)               │
        │  Tailscale VPN: 100.x.x.x:5000                │
        │  ┌──────────────────────────────────┐   │
        │  │  양면 프레임 수신 (/predict_dual) │   │
        │  │    - left_frame (부품)            │   │
        │  │    - right_frame (납땜)           │   │
        │  └──────────────────────────────────┘   │
        │                   ↓                      │
        │  ┌─────────────────────────────────┐    │
        │  │  이중 YOLO 병렬 추론 (80-100ms) │    │
        │  │                                  │    │
        │  │  ┌──────────────────────────┐   │    │
        │  │  │ 모델 1: FPIC-Component   │   │    │
        │  │  │ (부품 검출, 25 클래스)    │   │    │
        │  │  │ 추론 시간: 50-80ms        │   │    │
        │  │  └──────────────────────────┘   │    │
        │  │              ↓                   │    │
        │  │        component_result          │    │
        │  │                                  │    │
        │  │  ┌──────────────────────────┐   │    │
        │  │  │ 모델 2: SolDef_AI        │   │    │
        │  │  │ (납땜 불량, 5-6 클래스)   │   │    │
        │  │  │ 추론 시간: 30-50ms        │   │    │
        │  │  └──────────────────────────┘   │    │
        │  │              ↓                   │    │
        │  │        solder_result             │    │
        │  └─────────────────────────────────┘    │
        │                   ↓                      │
        │  ┌──────────────────────────────────┐   │
        │  │  결과 융합 로직 (<5ms)           │   │
        │  │  - component_defects 추출         │   │
        │  │  - solder_defects 추출            │   │
        │  │  - 심각도 계산                    │   │
        │  │  - 최종 판정                      │   │
        │  │    ├─ normal (정상)               │   │
        │  │    ├─ component_defect (부품불량) │   │
        │  │    ├─ solder_defect (납땜불량)    │   │
        │  │    └─ discard (폐기)              │   │
        │  └──────────────────────────────────┘   │
        │                   ↓                      │
        │  ┌──────────────────────────────────┐   │
        │  │  박스 슬롯 할당 로직 ⭐          │   │
        │  └──────────────────────────────────┘   │
        │                   ↓                      │
        │  ┌──────────────────────────────────┐   │
        │  │  MySQL 데이터베이스              │   │
        │  │  - 검사 이력 저장                │   │
        │  │  - 박스 상태 관리 ⭐             │   │
        │  │  - 부품/납땜 상세 결과           │   │
        │  │  - 통계 데이터                   │   │
        │  └──────────────────────────────────┘   │
        │                   ↓                      │
        │  ┌──────────────────────────────────┐   │
        │  │  REST API 서버                   │   │
        │  │  - /predict_dual ⭐ (양면 검사)  │   │
        │  │  - /api/inspections              │   │
        │  │  - /api/statistics               │   │
        │  │  - /api/box_status ⭐            │   │
        │  └──────────────────────────────────┘   │
        └─────────────────────────────────────────┘
                          ↓
              HTTP 응답 (라즈베리파이 1만)
              {
                "decision": "component_defect",
                "component_defects": [...],
                "solder_defects": [...],
                "gpio_signal": {...},
                "robot_arm_command": {...}
              }
                          ↓
                 ┌────────┴─────────┐
        라즈베리파이 1 (GPIO 제어)
                 │
                 ├─ GPIO 핀 제어 (LED/릴레이)
                 └─ USB 시리얼 (로봇팔)
                          ↓
                 HTTP REST API
                          ↓
        ┌─────────────────────────────────┐
        │  C# WinForms (Windows PC)       │
        │  - 실시간 모니터링 대시보드     │
        │  - 박스 상태 모니터링 ⭐        │
        │  - 부품/납땜 분리 표시 ⭐       │
        │  - 검사 이력 조회 (MySQL)       │
        │  - 불량 이미지 뷰어             │
        │  - 시스템 설정 관리             │
        └─────────────────────────────────┘

[로봇팔 시스템 아키텍처] ⭐ 업데이트 (레일 시스템 추가)

                 [컨베이어 벨트]
                        │
                        ↓ PCB 도착
        ┌───────────────────────────────────┐
        │  [레일 시스템 (X축)]              │ ⭐ 신규
        │    스텝모터 + GT2 벨트 (50-100cm)  │
        │         │                          │
        │    ┌────┴─────┐                    │
        │    │ 플랫폼    │                    │
        │    │  ┌──────┐│                    │
        │    │  │5-6축 ││ ← Arduino Mega 제어│
        │    │  │로봇팔││                    │
        │    │  └──────┘│                    │
        │    └──────────┘                    │
        └───────────┬───────────────────────┘
                    │ USB 시리얼
                    ↓
          ┌─────────────────────┐
          │  라즈베리파이 1     │
          │  (시리얼 컨트롤러)   │
          └─────────┬───────────┘
                    │ 로봇팔 + 레일 명령
                    ↓
          [분류 박스 시스템 - 수평 3슬롯 적재]
                        │
        ┌────────┬────────┬────────┬────────┐
        │        │        │        │        │
      [정상]   [부품불량] [납땜불량]  [폐기]
     (3슬롯)   (3슬롯)   (3슬롯)   (슬롯없음)
        │        │        │        │
        └────────┴────────┴────────┴─→ 슬롯 3/3 감지 → LED/WinForms 알림
                                                    → Flask 서버 OHT 자동 호출
                                                    → 창고 박스 교체 후 리셋

[박스 시스템 구조] ⭐ 물리적 제약 반영 (2025-10 업데이트)
- 총 3개 박스 + 1개 폐기 위치
- 각 박스: 3개 슬롯 (수평 나란히 적재)
- 총 9개 슬롯 = 3 박스 × 3 슬롯
- DISCARD: 슬롯 관리 없음 (고정 위치에 떨어뜨림)
- 로봇팔: 각 슬롯별 좌표는 Arduino 좌표 테이블에 저장

카테고리별 박스:
1. NORMAL (정상) - 3 슬롯 (수평)
2. COMPONENT_DEFECT (부품 불량) - 3 슬롯 (수평)
3. SOLDER_DEFECT (납땜 불량) - 3 슬롯 (수평)
4. DISCARD (폐기) - 슬롯 관리 없음 (고정 위치)

슬롯 할당 로직:
1. 각 박스는 슬롯 0번부터 2번까지 순차 채움
2. 슬롯 채울 때마다 WinForms와 Flask 서버가 사용률 업데이트
3. 박스가 가득 차면 (3/3):
   - LED 알림 (라즈베리파이 GPIO)
   - WinForms 화면 알림 (빨간색 경고)
   - 해당 박스 is_full = TRUE 처리
   - Flask 서버에서 `/api/oht/auto_trigger` 호출 → OHT 자동 호출
4. OHT가 박스를 창고로 교체한 뒤 관리자가 "박스 리셋" 버튼을 누르면 슬롯 0/3로 초기화

[GPIO + Arduino 병렬 사용] ⭐ 라즈베리파이 1 전용
양면 통합 판정 → Flask 서버 → HTTP 응답 (라즈베리파이 1만)
  │
  ├─ GPIO 제어 (LED/릴레이)
  │   ├─ 부품 불량 → GPIO 핀 17 (BCM) → LED/릴레이
  │   ├─ 납땜 불량 → GPIO 핀 27 (BCM) → LED/릴레이
  │   ├─ 폐기      → GPIO 핀 22 (BCM) → LED/릴레이
  │   └─ 정상      → GPIO 핀 23 (BCM) → LED/릴레이
  │
  └─ USB 시리얼 통신 (로봇팔 제어) ⭐
      └─ Arduino Mega 2560 ─→ 5-6축 로봇팔 (servo)
          │
          ├─ 픽업 좌표: (X, Y, Z) - 컨베이어 벨트 위치
          └─ 배치 좌표: 9개 슬롯 + 1개 폐기 위치 좌표 테이블
              - NORMAL_SLOT_0 ~ NORMAL_SLOT_2 (좌측 → 우측)
              - COMPONENT_DEFECT_SLOT_0 ~ COMPONENT_DEFECT_SLOT_2
              - SOLDER_DEFECT_SLOT_0 ~ SOLDER_DEFECT_SLOT_2
              - DISCARD_POSITION (고정 좌표)

참고: 라즈베리파이 2는 카메라 전용이며, GPIO 및 로봇팔 제어를 수행하지 않음
```

**[OHT 시스템 아키텍처]** ⭐ 업데이트 (2025-10-30)

```
[OHT (Overhead Hoist Transport) 시스템 - 수평 박스 배치]

                     [천장 레일 (X축)]
                            │
              ┌─────────────┴─────────────┐
              │     스텝모터 (NEMA 17)     │
              │     A4988 드라이버 × 1     │
              └─────────────┬─────────────┘
                            │ 수평 이동
                  ┌─────────┴─────────┐
                  │   OHT 베드         │
                  │   (양쪽 스텝모터)   │
                  └─────────┬─────────┘
                            │
                ┌───────────┼───────────┐
          좌측 모터          │       우측 모터
          (NEMA 17)         │      (NEMA 17)
          A4988 #2          │       A4988 #3
                │           │           │
            GT2 벨트    [베드 프레임]  GT2 벨트
            (감기)      걸쇠 서보모터   (감기)
                └───────────┴───────────┘
                            │ 상하 이동 (양쪽 동기화)
                            ↓
              ┌──────────────────────────┐
              │ 3개 박스 수평 배치 (각 3슬롯) │
              ├──────────────────────────┤
              │ [박스1]  [박스2]  [박스3]   │
              │  정상    부품불량  납땜불량   │
              │ (3슬롯)  (3슬롯)  (3슬롯)    │
              └──────────────────────────┘
                            ↓
                   [창고 (대기 위치)]

제어 흐름:
1. WinForms → Flask API (/api/oht/request)
   - 수동 호출 (Admin/Operator만 가능) ⭐ 권한 제한
   - 카테고리 선택 (정상/부품불량/납땜불량)

2. BoxManager 자동 감지 → Flask API (/api/oht/auto_trigger)
   - 박스 가득 참 (3/3 슬롯) 감지
   - 자동 OHT 호출 트리거

3. Flask 서버 → MySQL (oht_operations 테이블)
   - 작업 요청 저장 (status: pending)
   - 사용자 정보 및 카테고리 기록

4. 라즈베리파이 3번 (OHT 컨트롤러)
   - Flask API 폴링 (5초 간격)
   - GET /api/oht/check_pending
   - pending 작업 발견 시 실행

5. OHT 동작 시퀀스 (10단계):
   ① 창고 위치에서 대기
   ② 요청된 박스로 X축 수평 이동
   ③ Z축 양쪽 스텝모터 동기화하여 베드 내리기
   ④ 걸쇠 수평 위치 (0도) → 박스 구멍에 삽입
   ⑤ 걸쇠 회전 잠금 (90도)
   ⑥ Z축 양쪽 동기화하여 베드 올리기 (박스 픽업)
   ⑦ 창고로 X축 복귀
   ⑧ Z축 베드 내리기 (박스 내려놓기)
   ⑨ 걸쇠 해제 (0도)
   ⑩ Z축 베드 올리기 (완료)

6. Flask 서버
   - 작업 상태 업데이트 (status: completed)
   - 박스 슬롯 리셋 (current_slot = 0)
   - 실행 시간 및 결과 기록

7. WinForms 대시보드
   - 실시간 OHT 상태 모니터링
   - 최근 작업 이력 표시
   - 박스별 슬롯 사용 현황 (예: 2/3)
   - 권한별 제어 버튼 표시 (Viewer는 비활성화)

하드웨어 구성:
- X축 이동: NEMA 17 × 1 + A4988 드라이버
- Z축 이동: NEMA 17 × 2 (양쪽) + A4988 드라이버 × 2
- 베드 걸쇠: MG996R 서보모터 (L자 핀 회전)
- 위치 감지 (X축): 리미트 스위치 2개 (창고, 박스3 끝)
- 위치 감지 (Z축): 리미트 스위치 4개 (좌상, 좌하, 우상, 우하)
- 긴급 정지: 버튼 1개
- 박스 크기: PCB 3개 수납 가능 (세로 배치)
- 박스 배치: 수평으로 나란히 3개 (1m 간격)

권한 제어:
- Admin: OHT 호출, 긴급 정지, 상태 확인 모두 가능
- Operator: OHT 호출, 긴급 정지 가능
- Viewer: 상태 확인만 가능 (호출/제어 불가) ⭐

참고: 폐기(DISCARD) 카테고리는 OHT 대상이 아님 (별도 처리)
```

#### 웹서버 통신 프로토콜 ⭐ 업데이트

**1. 라즈베리파이 → Flask 서버 (양면 프레임 전송)**
- **요청**: HTTP POST `/predict_dual` ⭐ 신규
```json
{
  "left_frame": "base64_encoded_image",   // 좌측 카메라 (부품면)
  "right_frame": "base64_encoded_image",  // 우측 카메라 (납땜면)
  "timestamp": "2025-10-22T10:30:45.123Z"
}
```

- **응답**: JSON ⭐ 업데이트
```json
{
  "status": "ok",
  "decision": "normal" | "component_defect" | "solder_defect" | "discard",
  "component_defects": [
    {
      "type": "resistor",
      "bbox": [x, y, w, h],
      "confidence": 0.92,
      "issue": "misalignment"
    }
  ],
  "solder_defects": [
    {
      "type": "solder_bridge",
      "bbox": [x, y, w, h],
      "confidence": 0.88,
      "severity": "critical"
    }
  ],
  "component_severity": 1,  // 0-3
  "solder_severity": 2,     // 0-3
  "gpio_signal": {
    "pin": 27,
    "action": "HIGH",
    "duration_ms": 500
  },
  "robot_arm_command": {
    "action": "place_pcb",
    "box_id": "SOLDER_DEFECT",
    "slot_number": 2,
    "coordinates": {"x": 120.5, "y": 85.3, "z": 30.0}
  },
  "box_status": {
    "box_id": "SOLDER_DEFECT",
    "current_slot": 3,
    "is_full": false
  },
  "inspection_id": 12345,
  "inference_time_ms": 85
}
```

**참고**: 기존 `/predict` 엔드포인트는 하위 호환성을 위해 유지 가능

**2. Flask 서버 → MySQL (데이터 저장)**
- 검사 결과 INSERT (box_id, slot_number 포함) ⭐
- 박스 상태 업데이트 (current_slot, is_full) ⭐
- 불량 이미지 경로 저장
- 통계 업데이트

**3. 라즈베리파이 1 → Arduino Mega (USB 시리얼)** ⭐ 신규
- **프로토콜**: JSON over Serial (115200 baud)
- **명령 예시**:
```json
{
  "command": "place_pcb",
  "box_id": "NORMAL_A",
  "slot_number": 2,
  "coordinates": {"x": 120.5, "y": 85.3, "z": 30.0}
}
```
- **Arduino 응답**:
```json
{
  "status": "success",
  "message": "PCB placed successfully",
  "execution_time_ms": 2350
}
```

**4. C# WinForms → Flask 서버 (API 호출)**
- **GET** `/api/inspections?page=1&limit=50` - 검사 이력 조회
- **GET** `/api/statistics?start_date=2025-10-01&end_date=2025-10-22` - 통계 조회
- **GET** `/api/box_status` - 박스 상태 조회 (8개 박스) ⭐ 신규
- **GET** `/api/box_status/{box_id}` - 특정 박스 상태 조회 ⭐ 신규
- **POST** `/api/box_status/reset` - 박스 상태 리셋 (비우기) ⭐ 신규
- **GET** `/api/defect-images/{id}` - 불량 이미지 다운로드
- **GET** `/api/system-status` - 시스템 상태 확인
- **POST** `/api/config` - 시스템 설정 변경
- **POST** `/api/oht/request` - OHT 수동 호출 (Admin/Operator만) ⭐ 신규
- **POST** `/api/oht/auto_trigger` - OHT 자동 호출 (시스템 내부) ⭐ 신규
- **GET** `/api/oht/check_pending` - pending 작업 조회 (라즈베리파이 3번) ⭐ 신규
- **POST** `/api/oht/complete` - OHT 작업 완료 보고 ⭐ 신규
- **GET** `/api/oht/status` - OHT 시스템 상태 조회 ⭐ 신규

**5. 라즈베리파이 3번 → Flask 서버 (OHT 폴링)** ⭐ 신규
- **요청**: HTTP GET `/api/oht/check_pending`
- **응답 (작업 있음)**:
```json
{
  "has_pending": true,
  "operation": {
    "operation_id": "550e8400-e29b-41d4-a716-446655440000",
    "category": "NORMAL",
    "user_role": "Operator",
    "is_auto": false,
    "created_at": "2025-10-22T14:30:00"
  }
}
```
- **응답 (작업 없음)**:
```json
{
  "has_pending": false
}
```

#### 체크리스트 ⭐ 업데이트
- [ ] Flask 웹서버 구축 (상세: `Flask_Server_Setup.md`, `Dual_Model_Architecture.md`)
  - [ ] **이중 모델 로딩 및 추론** ⭐ 핵심
    - [ ] FPIC-Component 모델 로드 (부품 검출)
    - [ ] SolDef_AI 모델 로드 (납땜 불량)
    - [ ] 병렬 추론 로직 구현
    - [ ] GPU 메모리 관리 (8GB 이내)
  - [ ] **결과 융합 로직 구현** ⭐ 핵심
    - [ ] component_defects 추출 함수
    - [ ] solder_defects 추출 함수
    - [ ] 심각도 계산 알고리즘 (Level 0-3)
    - [ ] 최종 판정 로직 (normal/component/solder/discard)
  - [ ] **API 엔드포인트 개발**
    - [ ] POST /predict_dual - 양면 동시 검사 ⭐ 신규
    - [ ] POST /predict - 단일 프레임 (하위 호환)
    - [ ] GET /health - 서버 상태 확인
  - [ ] MySQL 데이터베이스 연동
    - [ ] 검사 이력 저장 (component/solder 분리)
    - [ ] 박스 상태 업데이트
  - [ ] REST API 엔드포인트 개발 (/api/*)
  - [ ] GPIO 제어 응답 로직
  - [ ] 박스 상태 관리 로직 (BoxManager) ⭐
  - [ ] 슬롯 할당 알고리즘 ⭐
  - [ ] 박스 가득 찬 경우 알림 시스템 ⭐
  - [ ] 사용자 관리 API (user_api.py) ⭐
    - [ ] GET /api/users - 사용자 목록 조회
    - [ ] POST /api/users - 사용자 생성
    - [ ] PUT /api/users/{id} - 사용자 수정
    - [ ] DELETE /api/users/{id} - 사용자 삭제
    - [ ] POST /api/users/{id}/reset-password - 비밀번호 초기화
    - [ ] GET /api/users/{id}/logs - 활동 로그 조회
  - [ ] 인증 API (auth_bp) ⭐
    - [ ] POST /api/auth/login - 로그인
    - [ ] POST /api/auth/logout - 로그아웃
- [ ] 라즈베리파이 클라이언트 개발 (상세: `RaspberryPi_Setup.md`)
  - [ ] 웹캠 프레임 캡처 및 전송
  - [ ] Base64 인코딩/디코딩
  - [ ] GPIO 제어 모듈 (RPi.GPIO)
  - [ ] 릴레이 제어 로직
  - [ ] USB 시리얼 통신 모듈 (pyserial) ⭐ 신규
  - [ ] Arduino Mega 명령 전송 ⭐ 신규
  - [ ] 로봇팔 응답 처리 ⭐ 신규
  - [ ] 자동 시작 스크립트
- [ ] 라즈베리파이 3번 OHT 컨트롤러 개발 (상세: `OHT_System_Setup.md`) ⭐ 신규
  - [ ] 스텝모터 제어 모듈 (X축 이동)
  - [ ] 서보모터 제어 모듈 (Z축 상하)
  - [ ] 리미트 스위치 및 센서 모듈
  - [ ] Flask API 폴링 로직 (5초 간격)
  - [ ] OHT 동작 시퀀스 구현
  - [ ] 긴급 정지 안전 기능
  - [ ] systemd 서비스 설정
  - [ ] 오류 처리 및 복구 로직
- [ ] Arduino 로봇팔 시스템 개발 (상세: `Arduino_RobotArm_Setup.md`) ⭐ 신규
  - [ ] Arduino Mega 2560 설정 ⭐ 신규
  - [ ] 5-6축 서보 제어 코드 ⭐ 신규
  - [ ] USB 시리얼 통신 핸들러 ⭐ 신규
  - [ ] 40개 슬롯 좌표 테이블 설정 ⭐ 신규
  - [ ] 픽업 및 배치 동작 구현 ⭐ 신규
  - [ ] 안전 기능 (충돌 방지, 리미트 스위치) ⭐ 신규
- [ ] C# WinForms 애플리케이션 개발 (상세: `CSharp_WinForms_Guide.md`, UI 설계: `CSharp_WinForms_Design_Specification.md`)
  - [ ] Visual Studio 프로젝트 생성
  - [ ] 사용자 인증 시스템 (로그인/권한 관리)
    - [ ] LoginForm - 로그인 화면
    - [ ] SessionManager - 세션 관리 (권한 체크)
    - [ ] Permission Enum - 권한 정의
  - [ ] MySQL 연동 (MySql.Data)
  - [ ] REST API 통신 (HttpClient)
  - [ ] 실시간 모니터링 대시보드 UI
  - [ ] 박스 상태 모니터링 UI ⭐ 신규
  - [ ] 박스 가득 찬 경우 알림 팝업 ⭐ 신규
  - [ ] OHT 제어 패널 (Admin/Operator만) ⭐ 신규
  - [ ] OHT 상태 모니터링 및 이력 표시 ⭐ 신규
  - [ ] 권한별 OHT 버튼 활성화/비활성화 ⭐ 신규
  - [ ] 검사 이력 조회 화면
  - [ ] 불량 이미지 뷰어
  - [ ] 통계 화면 (Excel 내보내기 포함)
  - [ ] 사용자 관리 화면 (Admin 전용) ⭐ 신규 상세 구현
    - [ ] UserManagementForm - 사용자 목록 및 관리
    - [ ] UserEditDialog - 사용자 추가/수정
    - [ ] UserLogsDialog - 활동 로그 뷰어
    - [ ] 사용자 검색 및 필터 (권한별, 활성 상태별)
    - [ ] 비밀번호 초기화 (temp1234)
    - [ ] 사용자 생성/수정/삭제
    - [ ] 활동 로그 조회 (날짜 범위, 활동 유형 필터)
  - [ ] 시스템 설정 화면
  - [ ] LiveCharts 통계 그래프
- [ ] MySQL 데이터베이스 설계 및 구축 (상세: `MySQL_Database_Design.md`)
  - [x] 테이블 스키마 설계
  - [x] 인덱스 최적화
  - [x] 초기 데이터 및 저장 프로시저
  - [x] 박스 상태 관리 테이블 (box_status) ⭐ 신규
  - [x] OHT 운영 관리 테이블 (oht_operations) ⭐ 신규
  - [x] 사용자 활동 로그 테이블 (user_logs) ⭐ 신규
  - [ ] 백업 전략 수립
- [ ] **병렬 처리 파이프라인 구현** ⭐ 업데이트
  - [ ] 이중 모델 병렬 추론 (PyTorch 자동 배치 처리)
  - [ ] 프레임 큐 관리 (지연 최소화)
  - [ ] GPU 메모리 최적화
- [ ] **결과 융합 알고리즘 개발** ⭐ 업데이트
  - [ ] 부품 모델 결과 파싱 (component_defects)
  - [ ] 납땜 모델 결과 파싱 (solder_defects)
  - [ ] 양면 검사 결과 통합 (좌측 + 우측)
  - [ ] 심각도 기반 융합 로직
  - [ ] NMS (Non-Maximum Suppression) 적용 (각 모델별)
- [ ] **불량 분류 판정 로직** ⭐ 업데이트
  - [ ] 부품 검출 분류 (25개 클래스)
  - [ ] 납땜 불량 분류 (5-6개 클래스)
  - [ ] 심각한 불량 판정 (폐기 기준)
    - [ ] component_severity >= 3
    - [ ] solder_severity >= 3
    - [ ] 양면 모두 Level 2 이상
  - [ ] 치명적 불량 타입 정의
    - [ ] missing_component
    - [ ] wrong_component
    - [ ] solder_bridge
  - [ ] Confidence threshold 설정 (클래스별)
- [ ] 실시간 처리 최적화
  - [ ] 프레임 스킵 로직 (처리 지연 시)
  - [ ] 배치 처리 (여러 프레임 동시 추론)
  - [ ] GPU 메모리 관리
  - [ ] 네트워크 지연 최소화
- [ ] 전체 시스템 테스트
  - [ ] End-to-end 추론 파이프라인
  - [ ] 실시간 처리 속도 측정 (FPS)
  - [ ] 네트워크 통신 안정성 테스트
  - [ ] 동시 접속 테스트 (2대 카메라)
  - [ ] 메모리 사용량 확인
- [ ] 모니터링 및 로깅
  - [ ] 실시간 검사 결과 시각화 (웹 대시보드)
  - [ ] 불량 통계 집계
  - [ ] 로그 기록 (검사 이력, 오류)
- [ ] 성능 비교
  - [ ] YOLO 단독 vs 하이브리드
  - [ ] 이상 탐지 단독 vs 하이브리드
  - [ ] 정량적 성능 비교표 작성

**목표 성능** ⭐ 업데이트:
- **부품 모델 (FPIC-Component)**: mAP@0.5 > 0.85
- **납땜 모델 (SolDef_AI)**: mAP@0.5 > 0.90
- **실시간 처리 속도**: 80-100ms (양면 병렬 추론)
  - 부품 모델: 50-80ms
  - 납땜 모델: 30-50ms (병렬)
  - 결과 융합: <5ms
- **추론 지연시간**: < 100ms (전체 파이프라인)
- **False Positive Rate**: < 5%
- **네트워크 통신 안정성**: 99% 이상
- **GPU 메모리 사용량**: < 8GB (16GB VRAM 중 50%)

**참고 문서**: `Flask_Server_Setup.md`, `Dual_Model_Architecture.md`

---

### 네트워크 요구사항

**네트워크 구성** (프로젝트 환경):
- **Tailscale VPN 메시 네트워크** 사용 (원격 연결) ⭐
- GPU PC는 **원격지** (같은 도시 내)에 위치
- 모든 장비는 Tailscale을 통해 **100.x.x.x** 대역으로 연결
- 고정 IP는 Tailscale에서 자동 할당

**IP 주소 할당** (Tailscale VPN):
- **Flask 서버 (GPU PC)**: 100.x.x.x (Tailscale 할당)
- **라즈베리파이 1 (좌측 카메라 + GPIO)**: 100.x.x.y (Tailscale 할당)
- **라즈베리파이 2 (우측 카메라)**: 100.x.x.z (Tailscale 할당)
- **Windows PC (모니터링)**: 100.x.x.w (Tailscale 할당)

**참고**: 로컬 네트워크(192.168.0.x)도 지원 가능하지만, 본 프로젝트는 Tailscale 원격 환경을 기본 사용

**포트 설정**:
- Flask 서버 포트: **5000** (TCP) - Tailscale 자동 처리 (방화벽 설정 불필요)
- MySQL 포트: **3306** (TCP) - Tailscale 네트워크 내 접근 허용

**대역폭 요구사항**:
- 웹캠 프레임 전송 (JPEG 압축, 640x480, 10 FPS):
  - 1대당: ~500 KB/s (0.5 MB/s)
  - 2대 동시: ~1 MB/s
- **권장 인터넷 속도** (원격 환경):
  - 업로드: 10 Mbps 이상 (라즈베리파이 측)
  - 다운로드: 10 Mbps 이상 (GPU PC 측)

**지연시간 (Latency)** (원격 환경):
- 라즈베리파이 ↔ Flask 서버 (Tailscale VPN): 20-50ms (같은 도시 내)
- 목표 End-to-End 지연: < 200ms (프레임 캡처 → 추론 → GPIO 제어)
- 실제 달성: 100-200ms ✅

**네트워크 안정성**:
- Tailscale VPN 자동 재연결 기능 활용
- 인터넷 연결 품질 확인 (유선 권장)
- 목표 통신 성공률: 99% 이상

**Tailscale 설정**:
- 상세 가이드: `docs/Remote_Network_Setup.md` 참조
- 모든 장비에 Tailscale 클라이언트 설치 필요

---

### 성능 벤치마크

**하드웨어 사양** (GPU PC) ⭐ 업데이트:
- **GPU**: NVIDIA RTX 4080 Super (16GB VRAM) ✅
- **AI 모델**: 이중 YOLOv11m (Large) 모델
  - 모델 1: FPIC-Component (부품 검출, 25 클래스)
  - 모델 2: SolDef_AI (납땜 불량, 5-6 클래스)
- **GPU 메모리 사용량**:
  - **학습 시** (각 모델 독립 학습):
    - 모델 1 학습: 10-12GB VRAM (배치 32 기준)
    - 모델 2 학습: 8-10GB VRAM (배치 32 기준)
    - 학습은 순차적으로 진행 (동시 학습 불필요)
  - **추론 시** (양면 동시):
    - 부품 모델 (YOLOv11m): ~4-5GB VRAM (FP16 최적화)
    - 납땜 모델 (YOLOv11m): ~3-4GB VRAM (FP16 최적화)
    - 양면 배치 처리: ~1GB VRAM
    - **총 예상**: ~8GB VRAM (16GB 중 50%, 여유 충분) ✅
- **시스템 RAM**: 8GB+ (16GB 권장)
- **CPU**: 4코어 이상

**디스크 공간** (프로젝트 단위) ⭐ 업데이트:
- **모델 저장**:
  - 부품 모델 (YOLOv11m): ~180 MB
  - 납땜 모델 (YOLOv11m): ~180 MB
  - 총: ~360 MB
- **학습 데이터셋**:
  - FPIC-Component: ~2-3 GB (6,260 이미지)
  - SolDef_AI: ~0.5-1 GB (1,150 이미지)
  - 총: ~3-4 GB
- **불량 이미지 저장** (프로젝트 단위):
  - 불량 이미지: 수백 장 × 100 KB = ~50 MB
  - 통계/로그 데이터: ~10 MB
- **총 예상 디스크 사용량**: ~20 GB (여유 공간 포함)

**추론 성능 목표 및 달성 예상**:
- **목표**: < 300ms (디팔렛타이저 분류 시간 고려, 2.5초 허용)
- **실제 달성 예상** (원격 연결 + RTX 4080 Super + YOLOv11m):
  - 원격 네트워크 (VPN): 100-200ms ✅
    - 이미지 인코딩: 10-20ms
    - 네트워크 왕복: 40-100ms (같은 도시)
    - AI 추론: 15-20ms
    - GPIO 제어: 1-5ms
  - 디팔렛타이저 허용: 2.5초
  - 여유 시간: 2.3초 이상 (10배 이상) ✅
- 메모리 효율: FP16 Precision 사용 권장

**최적화 권장사항** (RTX 4080 Super):
- **FP16 (Half Precision)**: VRAM 50% 절약 + 속도 1.5배 향상 ⭐ 강력 권장
- **배치 처리**: 좌우 2개 이미지를 batch=2로 동시 처리 (순차 대비 30% 빠름)
- **모델 선택**: YOLOv11m (정확도 52.9%, 90+ FPS) 권장
- **Batch Processing**: 여러 프레임 동시 추론 (효율 증가)
- **Model Quantization** (선택): INT8 양자화 (속도 2배, 정확도 소폭 감소)

**참고 문서**: `Flask_Server_Setup.md` (웹서버 구축 가이드)

---

### Phase 6: 문서화 및 발표 준비 (2주)

#### 체크리스트
- [x] 프로젝트 문서 작성
  - [x] README.md 작성
  - [x] 설치 가이드
  - [x] 사용법 가이드
  - [x] API 문서
- [ ] 코드 정리 및 리팩토링
  - [ ] 주석 추가
  - [ ] 함수 모듈화
  - [x] 설정 파일 분리
  - [ ] 에러 처리 강화
- [x] 실험 결과 정리
  - [x] 학습 그래프 저장
  - [ ] 성능 비교표
  - [ ] 불량 검출 예시 이미지
  - [x] Confusion Matrix
- [ ] 졸업 논문/보고서 작성
  - [ ] 서론 (연구 배경 및 목적)
  - [ ] 관련 연구 (YOLO, 이상 탐지 문헌 조사)
  - [ ] 제안 방법 (하이브리드 시스템 설명)
  - [ ] 실험 및 결과
  - [ ] 결론 및 향후 연구
- [ ] 발표 자료 준비
  - [ ] PPT 작성
  - [ ] 데모 영상 제작
  - [ ] 질의응답 대비

---

## 프로젝트 폴더 구조 (권장)

```
~/work_project/
│
├── docs/                          # 문서
│   ├── PCB_Defect_Detection_Project.md (이 파일)
│   ├── Phase1_YOLO_Setup.md
│   ├── Dataset_Guide.md
│   ├── Project_Structure.md
│   └── references/
│
├── data/                          # 데이터셋
│   ├── raw/
│   └── processed/
│
├── models/                        # 학습된 모델 저장
│   ├── yolo/
│   └── anomaly/
│
├── notebooks/                     # Jupyter 노트북
│   ├── 01_data_exploration.ipynb
│   └── 04_hybrid_system.ipynb
│
├── server/                        # Flask 추론 서버
│   ├── app.py
│   ├── README.md
│   └── routes/
│
├── yolo/                          # YOLO 학습 및 평가
│   ├── train_yolo.py
│   ├── evaluate_yolo.py
│   └── tests/
│
├── raspberry_pi/                  # 라즈베리파이 클라이언트 가이드
│   └── GETTING_STARTED.md
│
├── csharp_winforms/               # WinForms 모니터링 앱
│   └── GETTING_STARTED.md
│
├── database/                      # MySQL 스키마
│   ├── schema.sql
│   └── README.md
│
├── configs/                       # 설정 파일
│   ├── yolo_config.yaml
│   ├── server_config.yaml
│   └── camera_config.yaml
│
├── scripts/                       # 실행 스크립트
│   ├── train_yolo.sh
│   ├── start_server.sh
│   └── setup_env.sh
│
├── tests/                         # 통합 테스트
│   └── api/
│
├── results/                       # 실험 결과
│   └── figures/
│
├── logs/                          # 시스템 로그
├── README.md                      # 프로젝트 설명
├── requirements.txt               # Python 패키지 목록
└── .gitignore
```

**참고 문서**: `Project_Structure.md`

---

## 주요 기술 스택

### 딥러닝 프레임워크 ⭐ 업데이트
- **PyTorch**: 1.13+
- **Ultralytics**: YOLO v11m 공식 라이브러리 (이중 모델)
- ~~**Anomalib**: Intel의 이상 탐지 라이브러리~~ (사용하지 않음)

### 데이터 처리
- **OpenCV**: 이미지 전처리 및 웹캠 프레임 캡처
- **Albumentations**: 데이터 증강
- **Pillow**: 이미지 로딩
- **NumPy**: 배열 연산

### 웹 서버 및 통신
- **Flask**: 실시간 추론 서버 및 REST API
- **Flask-CORS**: Cross-Origin 요청 처리
- **Requests**: HTTP 클라이언트 (라즈베리파이)
- **Base64**: 이미지 인코딩/디코딩
- **Threading/Multiprocessing**: 병렬 처리

### 데이터베이스
- **MySQL 8.0**: 검사 이력, 통계, 시스템 로그 저장
- **PyMySQL** 또는 **SQLAlchemy**: Python MySQL 드라이버
- **MySql.Data** 또는 **MySqlConnector**: C# MySQL 드라이버

### 라즈베리파이
- **Raspberry Pi OS** (64-bit): 운영체제
- **OpenCV**: 웹캠 프레임 캡처
- **RPi.GPIO**: GPIO 핀 제어
- **Requests**: HTTP 통신

### C# WinForms
- **.NET 6+**: 프레임워크
- **MySql.Data** 또는 **MySqlConnector**: MySQL 연동
- **Newtonsoft.Json**: JSON 처리
- **LiveCharts**: 실시간 차트 및 그래프
- **HttpClient**: REST API 통신

### 시각화 및 모니터링
- **TensorBoard**: 학습 과정 모니터링
- **Matplotlib/Seaborn**: 결과 시각화
- **Flask Dashboard** (선택): 실시간 모니터링 웹 페이지
- **Weights & Biases** (선택): 실험 관리

### 평가 지표
- **scikit-learn**: Precision, Recall, F1
- **torchmetrics**: mAP, AUROC

---

## 참고 자료

### YOLO v11m 관련
- [Ultralytics YOLO 공식 문서](https://docs.ultralytics.com/)
- [YOLO v11m GitHub](https://github.com/ultralytics/ultralytics)
- [YOLO v11m 논문](https://arxiv.org/abs/2305.09972)

### ~~이상 탐지 관련~~ (아카이브)
- ~~[Anomalib 공식 문서](https://anomalib.readthedocs.io/)~~
- ~~[PaDiM 논문](https://arxiv.org/abs/2011.08785)~~
- ~~[PatchCore 논문](https://arxiv.org/abs/2106.08265)~~
- ~~[MVTec AD Dataset](https://www.mvtec.com/company/research/datasets/mvtec-ad)~~

**참고**: 이상 탐지 모델은 사용하지 않습니다. 이중 YOLO 모델로 대체되었습니다.

### PCB 불량 검사 관련 논문
- "Automatic Optical Inspection for PCB Defect Detection Using Deep Learning"
- "PCB Defect Detection Using Deep Learning Techniques"
- "Dual Model Approach for PCB Component and Soldering Defect Detection"

### 데이터셋 ⭐ 업데이트
**사용 중인 데이터셋**:
- **FPIC-Component** - 부품 검출 (IIT, India)
  - 6,260 이미지, 25개 클래스
  - 29,639 라벨 객체
  - 균형 잡힌 분포
- **SolDef_AI** - 납땜 불량 검출 (Roboflow)
  - 1,150 이미지 (429장 Roboflow 버전)
  - 5-6개 납땜 불량 클래스
  - 우주 항공 표준 (ECSS-Q-ST-70-38C)

**기타 참고 데이터셋**:
- [Kaggle PCB Defects Dataset](https://www.kaggle.com/search?q=pcb+defect)
- [Roboflow PCB Dataset](https://universe.roboflow.com/search?q=pcb)
- [DeepPCB Dataset](https://github.com/tangsanli5201/DeepPCB)

---

## 예상 일정 (총 12-14주)

| 주차 | Phase | 주요 활동 |
|------|-------|-----------|
| 1-2주 | Phase 1 | YOLO v11m 환경 구축 및 기본 테스트 |
| 3-5주 | Phase 2 | PCB 데이터 수집 및 전처리 |
| 6-9주 | Phase 3 | YOLO 모델 학습 및 최적화 |
| 10-13주 | Phase 4-5 | 이상 탐지 모델 구현 및 하이브리드 시스템 통합 |
| 14주 | Phase 6 | 문서화 및 발표 준비 |

---

## 성공 기준 ⭐ 업데이트

### 최소 목표
- [x] 이중 YOLO v11m 모델 선정 및 데이터셋 확보
- [ ] 부품 모델: mAP@0.5 > 0.80
- [ ] 납땜 모델: mAP@0.5 > 0.85
- [ ] 4가지 판정 가능 (정상/부품불량/납땜불량/폐기)

### 목표
- [ ] 이중 모델 시스템 완전 구현
  - [ ] 부품 모델: mAP@0.5 > 0.85
  - [ ] 납땜 모델: mAP@0.5 > 0.90
  - [ ] 결과 융합 로직 정상 작동
- [ ] 실시간 추론 (80-100ms)
- [ ] False Positive Rate < 10%
- [ ] Flask 서버 + 라즈베리파이 연동 완료

### 우수 목표
- [ ] 부품 모델: mAP@0.5 > 0.90
- [ ] 납땜 모델: mAP@0.5 > 0.95
- [ ] 추론 시간 < 80ms (최적화)
- [ ] False Positive Rate < 5%
- [ ] 양면 검사 시스템 완전 자동화
- [ ] 졸업 논문 우수 평가

---

## 트러블슈팅 가이드

### WSL2 GPU 사용 문제
- NVIDIA 드라이버 최신 버전 설치
- CUDA Toolkit WSL2 버전 설치
- `nvidia-smi` 명령어로 GPU 인식 확인

### 메모리 부족 문제
- Batch size 줄이기
- 이미지 해상도 조정
- Mixed precision training (FP16) 사용

### 학습이 수렴하지 않는 경우
- Learning rate 조정
- Warm-up 적용
- Gradient clipping
- 데이터 증강 강도 조절

---

## 관련 문서

본 프로젝트와 관련된 상세 가이드 문서:

1. **CSharp_WinForms_Guide.md** - C# WinForms 모니터링 앱 개발 기본 가이드
2. **CSharp_WinForms_Design_Specification.md** - UI 상세 설계 (권한 시스템, 7개 화면, Excel 내보내기)
3. **RaspberryPi_Setup.md** - 라즈베리파이 카메라 클라이언트 설정 (라즈베리파이 1, 2, 3 포함)
4. **OHT_System_Setup.md** - OHT 시스템 하드웨어 및 컨트롤러 설정 ⭐ 신규
5. **MySQL_Database_Design.md** - MySQL 데이터베이스 스키마 설계
6. **Flask_Server_Setup.md** - Flask 추론 서버 설정 (OHT API 포함)
7. **Project_Structure.md** - 전체 프로젝트 폴더 구조

---

## 연락처 및 협업

- **프로젝트 관리**: C:\work_project
- **실험 노트**: notebooks/ 폴더 활용
- **버전 관리**: Git 사용 권장

---

**작성일**: 2025-10-28
**최종 수정일**: 2025-10-31
**버전**: 2.0 ⭐ 이중 모델 아키텍처 업데이트

---

## 변경 이력

### v2.0 (2025-10-31) ⭐ 주요 아키텍처 변경
- **핵심 변경**: 단일 하이브리드 모델 → 이중 YOLO 모델 아키텍처
- **데이터셋**: 커스텀 병합 데이터셋 → FPIC-Component + SolDef_AI
- **모델**:
  - 모델 1: FPIC-Component (부품 검출, 25 클래스)
  - 모델 2: SolDef_AI (납땜 불량, 5-6 클래스)
- **추론 방식**: 병렬 추론 + 결과 융합 로직 (Flask 서버)
- **성능 목표**: 80-100ms (이전 100ms 목표 유지)
- **참고 문서**: `Dual_Model_Architecture.md` 추가

### v1.0 (2025-10-28)
- 초기 버전 작성
- YOLO + Anomalib 하이브리드 접근 방식
