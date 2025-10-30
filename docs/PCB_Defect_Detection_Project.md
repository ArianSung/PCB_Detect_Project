# PCB 불량 검사 프로젝트 (졸업 프로젝트)

## 프로젝트 개요

**목표**: 컨베이어 벨트를 통해 들어오는 PCB의 양면을 실시간으로 검사하여 불량을 자동으로 검출하고 분류하는 하이브리드 딥러닝 시스템 개발

**핵심 기술**:
- YOLO v8 기반 객체 탐지 (Object Detection)
- 이상 탐지 모델 (Anomaly Detection)
- 병렬 처리를 통한 결과 융합
- 실시간 비디오 스트리밍 및 웹 서버 통신 (Flask)
- 웹캠 프레임 실시간 분석

**시스템 구성**:
- **라즈베리파이 3대** (카메라 클라이언트 + OHT 컨트롤러)
  - **라즈베리파이 1**: 좌측 웹캠 + GPIO 출력 (분류 게이트, LED 제어)
    - Tailscale VPN: 100.x.x.y
  - **라즈베리파이 2**: 우측 웹캠 전용 (카메라만)
    - Tailscale VPN: 100.x.x.z
  - **라즈베리파이 3**: OHT(Overhead Hoist Transport) 컨트롤러 ⭐ 신규
    - Tailscale VPN: 100.64.1.4 (또는 로컬 예비: 192.168.0.22)
    - 스텝모터 제어 (X축 레일 이동)
    - 서보모터 제어 (Z축 박스 상하 이동)
    - 리미트 스위치 및 센서 관리
    - Flask API 폴링 (OHT 작업 요청 확인)
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
  - PyTorch + YOLO v8l (Large 모델)
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

### 1. 납땜 불량 (Soldering Defects)
- Cold Joint (불완전 납땜)
- Solder Bridge (납땜 브릿지)
- Insufficient Solder (납 부족)
- Excess Solder (과다 납)

### 2. 부품 불량 (Component Defects)
- Missing Component (부품 누락)
- Misalignment (부품 오장착)
- Wrong Component (잘못된 부품)
- Damaged Component (부품 손상)

### 3. PCB 기판 불량
- Trace Damage (트레이스 손상)
- Pad Damage (패드 손상)
- Scratches (긁힘)

---

## 프로젝트 전체 로드맵

### Phase 1: 환경 구축 및 YOLO v8 테스트 (1-2주) ⭐ 현재 단계

#### 체크리스트
- [x] WSL2 설치
- [x] Miniconda 설치 및 가상환경 구축
- [x] YOLO v8 설치
  - [x] PyTorch 설치
  - [x] Ultralytics 패키지 설치
  - [x] 설치 확인 테스트
- [x] YOLO v8 기본 튜토리얼 실습
  - [x] 이미지 추론 테스트
  - [x] 비디오 추론 테스트
  - [x] 사전 학습 모델 테스트
- [x] 공개 데이터셋으로 학습 테스트
  - [x] COCO dataset 샘플 학습
  - [x] 커스텀 데이터 학습 연습

**참고 문서**: `Phase1_YOLO_Setup.md`

---

### Phase 2: PCB 데이터 준비 (2-3주)

#### 체크리스트
- [ ] 공개 PCB 불량 데이터셋 탐색
  - [ ] Kaggle PCB 데이터셋 조사
  - [ ] Roboflow PCB 데이터셋 조사
  - [ ] GitHub 오픈소스 데이터셋 조사
- [ ] 선정한 데이터셋 다운로드
- [ ] 데이터 분석
  - [ ] 이미지 해상도 확인
  - [ ] 불량 유형별 개수 파악
  - [ ] 클래스 불균형 확인
- [x] YOLO 형식으로 전처리
  - [x] 이미지 리사이징
  - [x] 어노테이션 변환 (YOLO format)
  - [x] 데이터 증강 (Augmentation) 적용
- [x] 데이터 분할
  - [x] Train: 70%
  - [x] Validation: 20%
  - [x] Test: 10%

**참고 문서**: `Dataset_Guide.md`

---

### Phase 3: YOLO 모델 학습 및 최적화 (3-4주)

#### 체크리스트
- [x] 기본 YOLO v8 모델 선택
  - [x] YOLOv8n (nano - 경량)
  - [ ] YOLOv8s (small)
  - [ ] YOLOv8m (medium)
  - [ ] 성능/속도 비교 실험
- [x] 초기 학습 실행
  - [x] 기본 하이퍼파라미터로 학습
  - [x] 학습 과정 모니터링 (loss, mAP)
  - [ ] TensorBoard 활용
- [ ] 하이퍼파라미터 튜닝
  - [ ] Learning rate 조정
  - [ ] Batch size 최적화
  - [ ] Epoch 수 결정
  - [ ] Augmentation 파라미터 조정
- [x] 성능 평가
  - [x] mAP@0.5 측정
  - [x] mAP@0.5:0.95 측정
  - [x] Precision/Recall 분석
  - [x] 클래스별 성능 분석
- [ ] 모델 개선
  - [ ] Hard negative mining
  - [ ] 클래스 불균형 해결 (가중치 조정)
  - [ ] Ensemble 실험
- [ ] 추론 최적화
  - [ ] TensorRT 변환 (선택)
  - [ ] ONNX 변환 (선택)
  - [ ] 추론 속도 측정

**예상 결과**: mAP@0.5 > 0.85 달성

---

### Phase 4: 이상 탐지 모델 (선택 사항 - 시간 여유 시) (2-3주)

**⚠️ 우선순위**: Phase 1-3 (YOLO 시스템) 완료 및 안정화 후 검토

**목적**: YOLO로 잡지 못한 새로운 불량 패턴 탐지 (시스템 정확도 향상)

**판단 기준**:
- ✅ YOLO Fine-tuning 완료 및 정확도 80% 이상
- ✅ Flask 서버 + 라즈베리파이 연동 완료
- ✅ 실시간 검사 시스템 작동 확인
- ✅ 졸업 발표 일정까지 4주 이상 남음

**구현하지 않아도 프로젝트 완성 가능** - YOLO만으로 충분히 작동

#### 이상 탐지 모델 후보

##### 옵션 1: PaDiM (Patch Distribution Modeling) ⭐ 추천
- **장점**: SOTA 성능, pre-trained 모델 활용, 구현 빠름
- **단점**: 메모리 사용량 높음
- **추천 이유**: Anomalib 라이브러리로 빠른 구현 가능

##### 옵션 2: PatchCore
- **장점**: 매우 높은 정확도, 산업 적용 사례 많음
- **단점**: 연산량 많음, 실시간 처리 어려울 수 있음

##### 옵션 3: AutoEncoder 기반
- **장점**: 구현 간단, 학습 빠름
- **단점**: 복잡한 패턴 학습 제한적, 성능 낮음

#### 체크리스트
- [ ] **Phase 1-3 완료 확인 필수**
- [ ] 시간 및 일정 검토
- [ ] Anomalib 라이브러리 설치 및 테스트
  - [ ] MVTec 샘플 데이터로 PaDiM 테스트
  - [ ] 추론 속도 측정 (실시간 처리 가능한지)
- [ ] PCB 정상 데이터로 이상 탐지 학습
  - [ ] 정상 이미지 20~30장만으로 학습
  - [ ] Threshold 결정
  - [ ] 이상 영역 시각화
- [ ] YOLO 결과와 융합 알고리즘 개발
  - [ ] 두 모델 결과 통합 로직
  - [ ] 최종 불량 판정 규칙

**참고 논문**:
- PaDiM: "PaDiM: a Patch Distribution Modeling Framework for Anomaly Detection"
- PatchCore: "Towards Total Recall in Industrial Anomaly Detection"

**예상 효과**:
- 정확도 5~10% 향상
- 새로운 불량 패턴 탐지 가능
- 졸업 프로젝트 차별화 포인트

---

### Phase 5: 하이브리드 시스템 통합 및 웹서버 구축 (3-4주)

#### 실시간 검사 시스템 아키텍처

```
[컨베이어 벨트 시스템]
    │
    ├─ 웹캠 1 (좌측) ──→ [라즈베리파이 1] (100.64.1.2)
    │                      │
    │                      ├──→ Flask Client (프레임 전송)
    │                      ├──→ GPIO 제어 모듈 (LED/릴레이) ⭐
    │                      └──→ USB 시리얼 통신 (Arduino Mega 제어) ⭐ 신규
    │                             │
    └─ 웹캠 2 (우측) ──→ [라즈베리파이 2] (100.64.1.3)
                         │        │
                         │        └──→ Flask Client (프레임 전송만)
                         │
                ┌────────┴─────────┐
                │                  │
        HTTP POST (프레임)    HTTP 응답 (제어 신호 - 라즈베리파이 1만)
                │                  │
                ↓                  ↑
        ┌─────────────────────────────────┐
        │  Flask 추론 서버 (GPU PC)       │
        │  Tailscale IP: 100.64.1.1:5000          │
        │  ┌──────────────────────────┐   │
        │  │  양면 프레임 수신        │   │
        │  │  ↓                       │   │
        │  │  YOLO v8 + 이상 탐지     │   │
        │  │  ↓                       │   │
        │  │  좌측+우측 결과 통합     │   │
        │  │  ↓                       │   │
        │  │  최종 불량 분류 판정     │   │
        │  │  ↓                       │   │
        │  │  박스 슬롯 할당 로직 ⭐  │   │
        │  └──────────────────────────┘   │
        │           │                      │
        │           ↓                      │
        │  ┌──────────────────────────┐   │
        │  │  MySQL 데이터베이스      │   │
        │  │  - 검사 이력 저장        │   │
        │  │  - 박스 상태 관리 ⭐     │   │
        │  │  - 불량 이미지 경로      │   │
        │  │  - 통계 데이터           │   │
        │  └──────────────────────────┘   │
        │           │                      │
        │           ↓                      │
        │  ┌──────────────────────────┐   │
        │  │  REST API 서버           │   │
        │  │  - /api/inspections      │   │
        │  │  - /api/statistics       │   │
        │  │  - /api/box_status ⭐    │   │
        │  │  - /gpio/control         │   │
        │  └──────────────────────────┘   │
        └─────────────────────────────────┘
                     │
                     │ HTTP REST API
                     ↓
        ┌─────────────────────────────────┐
        │  C# WinForms (Windows PC)       │
        │  - 실시간 모니터링 대시보드     │
        │  - 박스 상태 모니터링 ⭐        │
        │  - 검사 이력 조회 (MySQL)       │
        │  - 불량 이미지 뷰어             │
        │  - 시스템 설정 관리             │
        └─────────────────────────────────┘

[로봇팔 시스템 아키텍처] ⭐ 신규 추가

                 [컨베이어 벨트]
                        │
                        ↓ PCB 도착
              ┌─────────────────────┐
              │   5-6축 로봇팔      │
              │  (Arduino Mega 제어) │
              └─────────┬───────────┘
                        │ USB 시리얼
                        ↓
              ┌─────────────────────┐
              │  라즈베리파이 1     │
              │  (시리얼 컨트롤러)   │
              └─────────┬───────────┘
                        │ 로봇팔 명령
                        ↓
              [분류 박스 시스템 - 수직 2단 적재]
                        │
        ┌───────┬───────┬───────┬───────┐
        │       │       │       │       │
      [정상]  [부품불량] [납땜불량] [폐기]
     (슬롯2개) (슬롯2개) (슬롯2개) (슬롯없음)
        │       │       │       │
        └───────┴───────┴───────┴─→ 가득 참 → LED 알림 + WinForms 알림
                                             → 시스템 자동 정지
                                             → OHT 수동 트리거 (가득 찬 박스 교체)

[박스 시스템 구조] ⭐ 물리적 제약 반영 (2025-10 업데이트)
- 총 3개 박스 + 1개 폐기 위치
- 각 박스: 2개 슬롯 (수직 2단 적재)
- 총 6개 슬롯 = 3 박스 × 2 슬롯
- DISCARD: 슬롯 관리 없음 (고정 위치에 떨어뜨림)
- 로봇팔: 각 슬롯별 정확한 좌표 설정 (Arduino 좌표 테이블)

카테고리별 박스:
1. NORMAL (정상) - 2 슬롯 (수직 2단)
2. COMPONENT_DEFECT (부품 불량) - 2 슬롯 (수직 2단)
3. SOLDER_DEFECT (납땜 불량) - 2 슬롯 (수직 2단)
4. DISCARD (폐기) - 슬롯 관리 없음 (고정 위치)

슬롯 할당 로직:
1. 각 박스는 슬롯 1번(하단)부터 채움
2. 슬롯 1번이 가득 차면 자동으로 슬롯 2번(상단)으로 전환
3. 박스가 가득 차면 (2/2):
   - LED 알림 (라즈베리파이 GPIO)
   - WinForms 화면 알림 (빨간색 경고)
   - 해당 박스 is_full = TRUE
4. 모든 박스가 가득 차면:
   - 시스템 자동 정지 (system_stopped = TRUE)
   - OHT(Overhead Hoist Transport) 수동 트리거로 가득 찬 박스 픽업
   - 박스 교체 후 관리자가 "박스 리셋" 버튼 클릭

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
          └─ 배치 좌표: 6개 슬롯 + 1개 폐기 위치 좌표 테이블
              - NORMAL_SLOT_1 (하단): (x1, y1, z1)
              - NORMAL_SLOT_2 (상단): (x2, y2, z2)
              - COMPONENT_DEFECT_SLOT_1 (하단): (x3, y3, z3)
              - COMPONENT_DEFECT_SLOT_2 (상단): (x4, y4, z4)
              - SOLDER_DEFECT_SLOT_1 (하단): (x5, y5, z5)
              - SOLDER_DEFECT_SLOT_2 (상단): (x6, y6, z6)
              - DISCARD (고정 위치): (xd, yd, zd)

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
              │ 3개 박스 수평 배치 (각 5슬롯) │
              ├──────────────────────────┤
              │ [박스1]  [박스2]  [박스3]   │
              │  정상    부품불량  납땜불량   │
              │ (5슬롯)  (5슬롯)  (5슬롯)    │
              └──────────────────────────┘
                            ↓
                   [창고 (대기 위치)]

제어 흐름:
1. WinForms → Flask API (/api/oht/request)
   - 수동 호출 (Admin/Operator만 가능) ⭐ 권한 제한
   - 카테고리 선택 (정상/부품불량/납땜불량)

2. BoxManager 자동 감지 → Flask API (/api/oht/auto_trigger)
   - 박스 가득 참 (5/5 슬롯) 감지
   - 자동 OHT 호출 트리거

3. Flask 서버 → MySQL (oht_operations 테이블)
   - 작업 요청 저장 (status: pending)
   - 사용자 정보 및 카테고리 기록

4. 라즈베리파이 3 (OHT 컨트롤러)
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
   - 박스별 슬롯 사용 현황 (예: 3/5)
   - 권한별 제어 버튼 표시 (Viewer는 비활성화)

하드웨어 구성:
- X축 이동: NEMA 17 × 1 + A4988 드라이버
- Z축 이동: NEMA 17 × 2 (양쪽) + A4988 드라이버 × 2
- 베드 걸쇠: MG996R 서보모터 (L자 핀 회전)
- 위치 감지 (X축): 리미트 스위치 2개 (창고, 박스3 끝)
- 위치 감지 (Z축): 리미트 스위치 4개 (좌상, 좌하, 우상, 우하)
- 긴급 정지: 버튼 1개
- 박스 크기: PCB 5개 수납 가능 (세로 배치)
- 박스 배치: 수평으로 나란히 3개 (1m 간격)

권한 제어:
- Admin: OHT 호출, 긴급 정지, 상태 확인 모두 가능
- Operator: OHT 호출, 긴급 정지 가능
- Viewer: 상태 확인만 가능 (호출/제어 불가) ⭐

참고: 폐기(DISCARD) 카테고리는 OHT 대상이 아님 (별도 처리)
```

#### 웹서버 통신 프로토콜

**1. 라즈베리파이 → Flask 서버 (프레임 전송)**
- **요청**: HTTP POST `/predict`
```json
{
  "camera_id": "left" | "right",
  "frame": "base64_encoded_image",
  "timestamp": "2025-10-22T10:30:45.123Z"
}
```

- **응답**: JSON
```json
{
  "status": "ok",
  "defect_type": "정상" | "부품불량" | "납땜불량" | "폐기",
  "confidence": 0.95,
  "boxes": [...],
  "gpio_signal": {
    "pin": 17,
    "action": "HIGH",
    "duration_ms": 500
  },
  "robot_arm_command": {
    "action": "place_pcb",
    "box_id": "NORMAL_A",
    "slot_number": 2,
    "coordinates": {"x": 120.5, "y": 85.3, "z": 30.0}
  },
  "box_status": {
    "box_id": "NORMAL_A",
    "current_slot": 3,
    "is_full": false
  },
  "inspection_id": 12345
}
```

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
- **GET** `/api/oht/check_pending` - pending 작업 조회 (라즈베리파이 3) ⭐ 신규
- **POST** `/api/oht/complete` - OHT 작업 완료 보고 ⭐ 신규
- **GET** `/api/oht/status` - OHT 시스템 상태 조회 ⭐ 신규

**5. 라즈베리파이 3 → Flask 서버 (OHT 폴링)** ⭐ 신규
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

#### 체크리스트
- [ ] Flask 웹서버 구축 (상세: `Flask_Server_Setup.md`)
  - [ ] Flask 추론 서버 개발 (GPU PC)
  - [ ] 프레임 수신 API 엔드포인트 (/predict)
  - [ ] 양면(좌측/우측) 동시 처리 로직
  - [ ] MySQL 데이터베이스 연동
  - [ ] REST API 엔드포인트 개발 (/api/*)
  - [ ] GPIO 제어 응답 로직
  - [ ] 박스 상태 관리 로직 (BoxManager) ⭐ 신규
  - [ ] 슬롯 할당 알고리즘 ⭐ 신규
  - [ ] 박스 가득 찬 경우 알림 시스템 ⭐ 신규
  - [ ] 사용자 관리 API (user_api.py) ⭐ 신규
    - [ ] GET /api/users - 사용자 목록 조회
    - [ ] POST /api/users - 사용자 생성
    - [ ] PUT /api/users/{id} - 사용자 수정
    - [ ] DELETE /api/users/{id} - 사용자 삭제
    - [ ] POST /api/users/{id}/reset-password - 비밀번호 초기화
    - [ ] GET /api/users/{id}/logs - 활동 로그 조회
  - [ ] 인증 API (auth_bp) ⭐ 신규
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
- [ ] 라즈베리파이 3 OHT 컨트롤러 개발 (상세: `OHT_System_Setup.md`) ⭐ 신규
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
- [ ] 병렬 처리 파이프라인 구현
  - [ ] 멀티프로세싱/멀티스레딩 설계
  - [ ] YOLO 추론 모듈 (GPU 최적화)
  - [ ] 이상 탐지 추론 모듈
  - [ ] 프레임 큐 관리 (지연 최소화)
- [ ] 결과 융합 알고리즘 개발
  - [ ] YOLO bbox + 이상 탐지 heatmap 융합
  - [ ] 양면 검사 결과 통합 (좌측 + 우측)
  - [ ] Confidence score 조정
  - [ ] NMS (Non-Maximum Suppression) 적용
- [ ] 불량 분류 판정 로직
  - [ ] 부품 불량 분류 (Missing, Misalignment 등)
  - [ ] 납땜 불량 분류 (Bridge, Cold Joint 등)
  - [ ] 심각한 불량 판정 (폐기 기준)
  - [ ] 불량 유형별 우선순위 설정
  - [ ] Threshold 설정 (클래스별)
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

**목표 성능**:
- 최종 mAP: > 0.90
- 실시간 처리 속도: > 10 FPS (양면 동시 처리)
- 추론 지연시간: < 100ms/frame
- False Positive Rate: < 5%
- 네트워크 통신 안정성: 99% 이상

**참고 문서**: `Flask_Server_Setup.md` (웹서버 구축 가이드)

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

**하드웨어 사양** (GPU PC):
- **GPU**: NVIDIA RTX 4080 Super (16GB VRAM) ✅
- **AI 모델**: YOLOv8l (Large) + 이상 탐지 하이브리드
- **GPU 메모리 사용량**:
  - 학습 시: 10-14GB VRAM (배치 32 기준)
  - 추론 시 (양면 동시):
    - YOLOv8l 모델: ~3-4GB VRAM (FP16 최적화)
    - 이상 탐지 모델 (PaDiM): ~1.5-2GB VRAM
    - 양면 배치 처리: ~1-2GB VRAM
    - 총 예상: 6-8GB VRAM (16GB 중 50%, 여유 충분)
- **시스템 RAM**: 8GB+ (16GB 권장)
- **CPU**: 4코어 이상

**디스크 공간** (프로젝트 단위):
- **모델 저장**:
  - YOLO 모델: ~50 MB (yolov8s.pt)
  - 이상 탐지 모델: ~200 MB (PaDiM)
  - 총: ~300 MB
- **학습 데이터셋**: ~5 GB (이미지 + 어노테이션)
- **불량 이미지 저장** (프로젝트 단위):
  - 불량 이미지: 수백 장 × 100 KB = ~50 MB
  - 통계/로그 데이터: ~10 MB
- **총 예상 디스크 사용량**: ~20 GB (여유 공간 포함)

**추론 성능 목표 및 달성 예상**:
- **목표**: < 300ms (디팔렛타이저 분류 시간 고려, 2.5초 허용)
- **실제 달성 예상** (원격 연결 + RTX 4080 Super + YOLOv8l):
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
- **모델 선택**: YOLOv8l (정확도 52.9%, 90+ FPS) 권장
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

### 딥러닝 프레임워크
- **PyTorch**: 1.13+
- **Ultralytics**: YOLO v8 공식 라이브러리
- **Anomalib**: Intel의 이상 탐지 라이브러리

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

### YOLO v8 관련
- [Ultralytics YOLO 공식 문서](https://docs.ultralytics.com/)
- [YOLO v8 GitHub](https://github.com/ultralytics/ultralytics)
- [YOLO v8 논문](https://arxiv.org/abs/2305.09972)

### 이상 탐지 관련
- [Anomalib 공식 문서](https://anomalib.readthedocs.io/)
- [PaDiM 논문](https://arxiv.org/abs/2011.08785)
- [PatchCore 논문](https://arxiv.org/abs/2106.08265)
- [MVTec AD Dataset](https://www.mvtec.com/company/research/datasets/mvtec-ad)

### PCB 불량 검사 관련 논문
- "Automatic Optical Inspection for PCB Defect Detection Using Deep Learning"
- "PCB Defect Detection Using Deep Learning Techniques"
- "Hybrid Approach for PCB Defect Detection Using CNN and Traditional Image Processing"

### 데이터셋
- [Kaggle PCB Defects Dataset](https://www.kaggle.com/search?q=pcb+defect)
- [Roboflow PCB Dataset](https://universe.roboflow.com/search?q=pcb)
- [DeepPCB Dataset](https://github.com/tangsanli5201/DeepPCB)

---

## 예상 일정 (총 12-14주)

| 주차 | Phase | 주요 활동 |
|------|-------|-----------|
| 1-2주 | Phase 1 | YOLO v8 환경 구축 및 기본 테스트 |
| 3-5주 | Phase 2 | PCB 데이터 수집 및 전처리 |
| 6-9주 | Phase 3 | YOLO 모델 학습 및 최적화 |
| 10-13주 | Phase 4-5 | 이상 탐지 모델 구현 및 하이브리드 시스템 통합 |
| 14주 | Phase 6 | 문서화 및 발표 준비 |

---

## 성공 기준

### 최소 목표
- [x] YOLO v8 모델 학습 및 불량 검출 가능
- [ ] mAP@0.5 > 0.80
- [ ] 4가지 불량 유형 모두 검출 가능

### 목표
- [ ] YOLO + 이상 탐지 하이브리드 시스템 구현
- [ ] mAP@0.5 > 0.85
- [ ] False Positive Rate < 10%

### 우수 목표
- [ ] 실시간 추론 가능 (< 100ms/image)
- [ ] mAP@0.5 > 0.90
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
**최종 수정일**: 2025-10-22
**버전**: 1.0
