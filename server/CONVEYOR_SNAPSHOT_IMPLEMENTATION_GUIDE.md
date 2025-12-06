# 컨베이어 벨트 스냅샷 시스템 구현 가이드

## 📌 문서 개요

이 문서는 PCB 불량 검사 시스템에서 컨베이어 벨트 환경에 대응하기 위한 스냅샷 캡처 시스템 구현 가이드입니다.

**작성일**: 2025-12-05
**마지막 업데이트**: 2025-12-05
**관련 커밋**: d53d9ad (ComponentVerifier 좌표 우선순위 수정)

---

## 🎯 현재 시스템 상태 (이미 구현 완료)

### 1. ComponentVerifier 좌표 시스템 수정 (Commit d53d9ad)

#### 문제 상황
- YOLO 검출 결과 21개 중 실제 부품 20개와 비교 시 "위치오류 21개"로 잘못 판정
- 모든 컴포넌트가 위치 오류로 표시되고 누락/정상 판정이 작동하지 않음

#### 근본 원인
`component_verification.py` 163-168줄에서 좌표 우선순위 로직 오류:
```python
# ❌ 잘못된 코드 (커밋 전)
if 'center' in det_comp:
    det_copy['center'] = det_comp['center']
elif 'relative_center' in det_comp:
    det_copy['center'] = det_comp['relative_center']
```
- `boxes_data`는 항상 'center' 키를 포함 (절대좌표)
- 'relative_center' 체크는 **데드 코드**로 절대 실행되지 않음
- ComponentVerifier가 절대좌표로 비교하여 모든 부품이 오류로 판정됨

#### 해결 방법
**우선순위 변경**: 'relative_center' 우선 체크 → 'center' 백업
```python
# ✅ 수정된 코드 (커밋 후)
if 'relative_center' in det_comp:
    det_copy['center'] = det_comp['relative_center']
elif 'center' in det_comp:
    cx, cy = det_comp['center']
    det_copy['center'] = [cx - ref_x, cy - ref_y]
```

#### 결과
- ✅ 템플릿 기준 상대좌표로 올바른 비교 수행
- ✅ 누락 부품, 위치 오류, 정상 부품 정확히 분류
- ✅ DB 저장 좌표는 이미 상대좌표이므로 추가 변환 불필요 (176-189줄 단순화)

---

### 2. boxes_data 이중 좌표 시스템

#### 설계 의도
- **절대좌표 ('center')**: 디버그 뷰어, WinForms 모니터링용 - 화면 픽셀 위치
- **상대좌표 ('relative_center')**: ComponentVerifier용 - 템플릿 기준점 (0,0) 기준

#### 구현 위치: `app.py` 1107-1127줄
```python
boxes_data = []
for box in smoothed_boxes:
    cx = (box['x1'] + box['x2']) / 2
    cy = (box['y1'] + box['y2']) / 2

    box_data = {
        'class_name': box['class_name'],
        'bbox': [box['x1'], box['y1'], box['x2'], box['y2']],
        'center': [cx, cy],  # 절대좌표 (디버그 뷰어용)
        'confidence': box['confidence']
    }

    # 템플릿 기준점을 (0,0)으로 하는 상대 좌표 추가
    if reference_point:
        ref_x, ref_y = reference_point
        rel_x = cx - ref_x
        rel_y = cy - ref_y
        box_data['relative_center'] = [rel_x, rel_y]

    boxes_data.append(box_data)
```

#### ComponentVerifier 초기화: `app.py` 1215-1221줄
```python
verifier = ComponentVerifier(
    reference_components=reference_components,
    position_threshold=20.0,  # 20픽셀 허용 오차
    confidence_threshold=0.25,
    reference_point=reference_point  # ⭐ 템플릿 기준점 전달
)
```

---

## 🚨 컨베이어 벨트 문제 정의

### 현재 시스템 가정
- PCB가 정지 상태에서 프레임 캡처
- 매 프레임마다 `/predict_dual` 엔드포인트 호출하여 실시간 추론

### 실제 환경 문제점

#### 1. 모션 블러 (Motion Blur)
- 컨베이어 벨트가 움직이는 동안 프레임 캡처 시 흐릿한 이미지
- YOLO 검출 정확도 하락
- OCR 시리얼 넘버 인식 실패 가능

#### 2. 타이밍 문제
- PCB가 최적 위치(템플릿 ROI 중앙)에 있지 않은 프레임에서 추론 시도
- 부분적으로 프레임 밖에 있는 PCB 검출

#### 3. 중복 처리 문제 (⚠️ 핵심)
- 컨베이어 속도가 느릴 경우 동일 PCB가 10-20프레임 동안 ROI에 머무름
- 같은 PCB를 여러 번 검증하여 DB에 중복 저장
- 통계 왜곡 및 GPIO 신호 중복 발생

---

## 💡 제안 솔루션: 스냅샷 캡처 시스템

### 핵심 아이디어
1. **ROI 진입 감지**: 템플릿이 템플릿 ROI에 **최초 진입**할 때 감지
2. **스냅샷 캡처**: 해당 순간의 양면 프레임을 메모리에 저장
3. **1회 처리**: 저장된 스냅샷으로 YOLO + 검증 수행 (1회만)
4. **중복 방지**: 타임스탬프 + 위치 조합으로 동일 PCB 재처리 차단
5. **스트리밍 유지**: WinForms 모니터링용 실시간 스트리밍은 계속 전송

### 장점
- ✅ 모션 블러 최소화 (최적 위치에서 캡처)
- ✅ 중복 처리 방지 (1 PCB = 1 검증)
- ✅ 기존 코드 최소 수정
- ✅ Git 롤백 가능 (브랜치 전략 사용)

---

## 🔧 구현 상세

### 1. Git 브랜치 전략

```bash
# 현재 브랜치: develop
# 새 기능 브랜치 생성
git checkout -b feature/conveyor-snapshot

# 구현 후 테스트
# 문제 발생 시 롤백
git checkout develop
# 또는 성공 시 머지
git merge feature/conveyor-snapshot
```

### 2. 상태 관리 구조

#### 전역 변수 추가 (`app.py` 상단)
```python
# 스냅샷 상태 관리
pcb_snapshot_state = {
    'last_snapshot_time': 0,          # 마지막 스냅샷 캡처 시간 (timestamp)
    'last_reference_point': None,     # 마지막 기준점 위치 (x, y)
    'snapshot_frames': {              # 저장된 스냅샷 프레임
        'left': None,
        'right': None
    },
    'processing_in_progress': False,  # 검증 진행 중 플래그
    'cooldown_time': 3.0,             # 재캡처 방지 쿨다운 (초)
    'position_threshold': 100         # 위치 변화 임계값 (픽셀)
}

# Lock 추가
snapshot_lock = threading.Lock()
```

### 3. 헬퍼 함수 구현

#### 3-1. 새로운 PCB 감지 함수
```python
def is_new_pcb(current_ref_point, current_time):
    """
    새로운 PCB인지 판단 (중복 방지)

    판단 기준:
    1. 충분한 시간이 지났는가? (cooldown_time)
    2. 기준점이 충분히 이동했는가? (position_threshold)

    Args:
        current_ref_point (tuple): 현재 템플릿 기준점 (x, y)
        current_time (float): 현재 타임스탬프

    Returns:
        bool: True면 새로운 PCB, False면 이전 PCB
    """
    with snapshot_lock:
        last_time = pcb_snapshot_state['last_snapshot_time']
        last_point = pcb_snapshot_state['last_reference_point']

        # 1. 시간 체크
        time_elapsed = current_time - last_time
        if time_elapsed < pcb_snapshot_state['cooldown_time']:
            return False  # 쿨다운 시간 미달

        # 2. 위치 체크 (이전 기준점이 있는 경우)
        if last_point is not None:
            distance = np.sqrt(
                (current_ref_point[0] - last_point[0])**2 +
                (current_ref_point[1] - last_point[1])**2
            )

            if distance < pcb_snapshot_state['position_threshold']:
                return False  # 위치 변화 미달 (같은 PCB)

        return True  # 새로운 PCB
```

#### 3-2. 스냅샷 저장 함수
```python
def save_snapshot(left_frame, right_frame, reference_point):
    """
    양면 프레임 스냅샷 저장

    Args:
        left_frame (np.ndarray): 좌측(앞면) 프레임
        right_frame (np.ndarray): 우측(뒷면) 프레임
        reference_point (tuple): 템플릿 기준점 (x, y)
    """
    with snapshot_lock:
        pcb_snapshot_state['snapshot_frames']['left'] = left_frame.copy()
        pcb_snapshot_state['snapshot_frames']['right'] = right_frame.copy()
        pcb_snapshot_state['last_snapshot_time'] = time.time()
        pcb_snapshot_state['last_reference_point'] = reference_point
        pcb_snapshot_state['processing_in_progress'] = True

        logger.info(
            f"✅ 스냅샷 저장 완료 "
            f"(기준점: {reference_point}, "
            f"시간: {time.strftime('%H:%M:%S')})"
        )
```

#### 3-3. 스냅샷 처리 완료 함수
```python
def mark_snapshot_processed():
    """
    스냅샷 처리 완료 마킹
    """
    with snapshot_lock:
        pcb_snapshot_state['processing_in_progress'] = False
        logger.info("✅ 스냅샷 처리 완료")
```

### 4. `/predict_dual` 엔드포인트 수정

#### 수정 전략
- 기존 실시간 스트리밍 유지 (WinForms 모니터링용)
- ROI 진입 감지 시 스냅샷 캡처
- 스냅샷이 있으면 해당 프레임으로 검증 수행

#### 수정 코드 (`app.py` `/predict_dual` 내부)

```python
@app.route('/predict_dual', methods=['POST'])
def predict_dual():
    try:
        # ... (기존 프레임 수신 코드 유지) ...

        # 6-1. 템플릿 매칭 + ROI 체크
        should_run_yolo = False
        roi_status = "unknown"
        reference_point = None

        if template_alignment and template_alignment.template is not None:
            # ... (기존 ROI 정의 코드 유지) ...

            # 템플릿 매칭
            reference_point = template_alignment.find_reference_point(
                left_frame,
                method=cv2.TM_CCORR_NORMED,
                roi=None
            )

            if reference_point:
                ref_x, ref_y = reference_point
                is_in_roi = (roi_x1 <= ref_x <= roi_x2 and roi_y1 <= ref_y <= roi_y2)

                # ⭐ 스냅샷 캡처 로직 추가 ⭐
                current_time = time.time()

                if is_in_roi:
                    # ROI 진입 감지 + 새로운 PCB 확인
                    if is_new_pcb(reference_point, current_time):
                        # 새로운 PCB → 스냅샷 저장
                        save_snapshot(left_frame, right_frame, reference_point)
                        should_run_yolo = True
                        roi_status = "snapshot_captured"
                        logger.info(
                            f"📸 새로운 PCB 감지 → 스냅샷 캡처 "
                            f"(기준점: {reference_point})"
                        )
                    elif pcb_snapshot_state['processing_in_progress']:
                        # 이전 PCB의 스냅샷 처리 진행 중 → 스냅샷 사용
                        left_frame = pcb_snapshot_state['snapshot_frames']['left']
                        right_frame = pcb_snapshot_state['snapshot_frames']['right']
                        should_run_yolo = True
                        roi_status = "using_snapshot"
                        logger.info(
                            f"🔄 기존 스냅샷 사용 "
                            f"(같은 PCB, 중복 방지)"
                        )
                    else:
                        # 같은 PCB + 이미 처리 완료 → YOLO 스킵
                        should_run_yolo = False
                        roi_status = "duplicate_pcb"
                        logger.info(
                            f"⏭️ 중복 PCB 감지 → YOLO 건너뛰기"
                        )
                else:
                    should_run_yolo = False
                    roi_status = "out_of_roi"

        # ... (기존 YOLO 검출 코드 유지) ...

        # 검증 완료 후 스냅샷 상태 업데이트
        if should_run_yolo and roi_status in ["snapshot_captured", "using_snapshot"]:
            mark_snapshot_processed()

        # ... (기존 응답 반환 코드 유지) ...

    except Exception as e:
        logger.error(f"예측 오류: {str(e)}")
        return jsonify({'error': str(e)}), 500
```

---

## ⚙️ 파라미터 튜닝 가이드

### 컨베이어 속도별 권장 설정

| 컨베이어 속도 | cooldown_time (초) | position_threshold (픽셀) | 설명 |
|--------------|-------------------|-------------------------|------|
| **매우 느림** (5cm/s) | 5.0 | 150 | 같은 PCB가 오래 머무름 → 긴 쿨다운 |
| **느림** (10cm/s) | 3.0 | 100 | 기본 설정 (권장) |
| **보통** (20cm/s) | 2.0 | 80 | 빠른 이동 → 짧은 쿨다운 |
| **빠름** (30cm/s) | 1.5 | 60 | 모션 블러 주의 필요 |

### 튜닝 방법
1. 실제 컨베이어 속도 측정 (cm/s)
2. 위 표에서 가장 가까운 값 선택
3. 테스트 실행 후 로그 확인:
   - "중복 PCB 감지" 메시지가 너무 많으면 → `cooldown_time` 감소
   - "새로운 PCB 감지" 메시지가 너무 자주 나오면 → `cooldown_time` 증가

### 코드 수정 위치
```python
# app.py 상단
pcb_snapshot_state = {
    # ... (다른 설정) ...
    'cooldown_time': 3.0,             # ⬅️ 여기 수정
    'position_threshold': 100         # ⬅️ 여기 수정
}
```

---

## 🧪 테스트 절차

### 1. 기능 브랜치 생성
```bash
git checkout -b feature/conveyor-snapshot
```

### 2. 코드 수정 적용
- 위의 "구현 상세" 섹션 코드 적용
- 파라미터 초기값 설정

### 3. 로컬 테스트
```bash
# Flask 서버 재시작
cd /home/sys1041/work_project/server
conda activate pcb_defect
python app.py
```

### 4. 로그 모니터링
```bash
# 새 터미널에서 로그 실시간 확인
tail -f logs/flask_server_*.log | grep -E "(스냅샷|PCB 감지|중복)"
```

### 5. 예상 로그 출력
```
[19:40:12] INFO: 📸 새로운 PCB 감지 → 스냅샷 캡처 (기준점: (320, 240))
[19:40:12] INFO: ✅ 스냅샷 저장 완료 (기준점: (320, 240), 시간: 19:40:12)
[19:40:12] INFO: [DUAL-LEFT] YOLO 검출 완료: 21개 부품
[19:40:12] INFO: ✅ 스냅샷 처리 완료
[19:40:13] INFO: 🔄 기존 스냅샷 사용 (같은 PCB, 중복 방지)
[19:40:14] INFO: ⏭️ 중복 PCB 감지 → YOLO 건너뛰기
```

### 6. 성공 기준
- [ ] 동일 PCB에 대해 1회만 검증 수행
- [ ] "중복 PCB 감지" 로그 출력
- [ ] DB에 중복 레코드 없음
- [ ] WinForms 실시간 스트리밍 정상 작동
- [ ] 모션 블러 감소 확인

---

## 🔄 롤백 방법

### 문제 발생 시 원래 상태로 복구

#### 방법 1: Git 브랜치 전환 (권장)
```bash
# develop 브랜치로 돌아가기
git checkout develop

# 서버 재시작
cd /home/sys1041/work_project/server
conda activate pcb_defect
python app.py
```

#### 방법 2: 특정 커밋으로 롤백
```bash
# 현재 커밋 확인
git log --oneline -5

# 특정 커밋으로 복구 (예: d53d9ad)
git reset --hard d53d9ad

# 서버 재시작
python app.py
```

#### 방법 3: 임시 비활성화 (코드 수정)
`app.py`에서 스냅샷 로직 주석 처리:
```python
# ⭐ 임시 비활성화: 기존 방식으로 동작 ⭐
should_run_yolo = is_in_roi  # 스냅샷 로직 무시
roi_status = "in_roi" if is_in_roi else "out_of_roi"
```

---

## 📚 추가 참고 사항

### 관련 파일
- `server/app.py`: Flask 서버 메인 로직
- `server/component_verification.py`: 부품 위치 검증
- `server/pcb_alignment.py`: PCB 정렬 (템플릿 매칭)
- `docs/Flask_Server_Setup.md`: Flask 서버 구축 가이드
- `docs/Serial_Number_Detection_Guide.md`: 시리얼 넘버 검출 가이드

### 성능 고려사항
- **메모리 사용량**: 스냅샷 프레임 저장 시 추가 메모리 필요 (약 2-3MB per PCB)
- **처리 속도**: 스냅샷 캡처는 10ms 미만 (copy 연산만)
- **쿨다운 시간**: 너무 짧으면 중복 처리, 너무 길면 다음 PCB 놓칠 수 있음

### 향후 개선 아이디어
1. **적응형 쿨다운**: 컨베이어 속도 실시간 감지 후 자동 조정
2. **큐 시스템**: 여러 PCB 스냅샷을 큐에 저장하여 순차 처리
3. **하드웨어 트리거**: 물리적 센서로 PCB 진입 감지 (광센서, 근접 센서)

---

## 📝 체크리스트

구현 전 확인사항:
- [ ] Git 브랜치 생성 (`feature/conveyor-snapshot`)
- [ ] 기존 코드 백업 (`git stash` 또는 커밋)
- [ ] 파라미터 초기값 결정 (컨베이어 속도 기반)

구현 후 확인사항:
- [ ] 로그에 "새로운 PCB 감지" 메시지 확인
- [ ] 로그에 "중복 PCB 감지" 메시지 확인
- [ ] DB 중복 레코드 없음 검증
- [ ] WinForms 실시간 스트리밍 정상 작동
- [ ] YOLO 검출 정확도 유지/개선 확인

문제 발생 시:
- [ ] 로그 파일 저장 (`logs/flask_server_*.log`)
- [ ] 스크린샷 캡처 (WinForms, 디버그 뷰어)
- [ ] Git 롤백 수행 (`git checkout develop`)
- [ ] 문제 원인 분석 후 재시도

---

**문서 끝**

궁금한 점이나 문제가 발생하면 이 문서의 "롤백 방법" 섹션을 참고하여 안전하게 이전 상태로 복구할 수 있습니다.
