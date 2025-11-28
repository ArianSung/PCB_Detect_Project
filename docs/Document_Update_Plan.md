# 이중 모델 아키텍처 전환 - 문서 업데이트 계획

## 변경 사항 요약

### 핵심 변경
- **기존**: 단일 하이브리드 모델 (YOLO + 이상 탐지)
- **신규**: 이중 전문 YOLO 모델
  - 모델 1: FPIC-Component (부품 검출, 25개 클래스)
  - 모델 2: SolDef_AI (납땜 불량, 5-6개 클래스)

### 데이터셋 변경
- **기존**: 병합 데이터셋 (29개 클래스, 심각한 불균형)
- **신규**:
  - FPIC-Component: 6,260 이미지, 25개 부품 클래스
  - SolDef_AI: 1,150 이미지, 5-6개 납땜 불량 클래스

### 시스템 아키텍처 변경
- **기존**: 단일 프레임 → 단일 모델 추론
- **신규**: 양면 동시 캡처 → 이중 모델 병렬 추론 → 결과 융합

---

## 수정이 필요한 문서 (우선순위별)

### 🔴 **Priority 1: 핵심 아키텍처 문서** (필수 수정)

#### 1. `/CLAUDE.md` ⭐⭐⭐
**영향도**: 매우 높음 (모든 개발자가 참조)

**수정 항목**:
- [ ] Line 16: 모델 설명 변경
- [ ] Line 23: Flask 서버 설명 업데이트
- [ ] Line 104-121: 아키텍처 다이어그램 완전 재작성
- [ ] Line 124: Anomalib 제거
- [ ] Line 136-144: models/ 디렉토리 구조 변경
- [ ] Line 150: AI 추론 설명 변경
- [ ] Line 165-180: Key Components 재작성
- [ ] Line 215-222: 설정 파일 예시 변경 (2개 모델 경로)
- [ ] Line 273-274: AI 모델 사양 업데이트
- [ ] Line 310-312: 불량 분류 기준 업데이트
- [ ] Line 316-319: 개발 우선순위 업데이트

**예상 작업 시간**: 30분

---

#### 2. `docs/PCB_Defect_Detection_Project.md` ⭐⭐⭐
**영향도**: 매우 높음 (프로젝트 전체 개요)

**확인 필요**:
- [ ] 프로젝트 개요 및 목표
- [ ] 시스템 아키텍처 다이어그램
- [ ] AI 모델 구조 설명
- [ ] 데이터셋 정보
- [ ] Phase별 개발 계획

**예상 작업 시간**: 40분

---

#### 3. `docs/Flask_Server_Setup.md` ⭐⭐⭐
**영향도**: 매우 높음 (Flask 서버 구현 가이드)

**수정 항목**:
- [ ] 이중 모델 로딩 코드
- [ ] `/predict_dual` 엔드포인트 추가
- [ ] 결과 융합 로직 설명
- [ ] 모델 경로 설정 (2개)
- [ ] GPU 메모리 사용량 업데이트

**예상 작업 시간**: 30분

---

#### 4. `docs/Dataset_Guide.md` ⭐⭐⭐
**영향도**: 매우 높음 (데이터셋 완전 변경)

**수정 항목**:
- [ ] 기존 데이터셋 설명 삭제 또는 아카이브
- [ ] FPIC-Component 설명 추가
- [ ] SolDef_AI 설명 추가
- [ ] 다운로드 방법
- [ ] YOLO 형식 변환 가이드
- [ ] 클래스 정의 (25 + 5-6개)

**예상 작업 시간**: 45분

---

### 🟡 **Priority 2: API 및 통합 문서** (중요 수정)

#### 5. `docs/API_Contract.md` ⭐⭐
**영향도**: 높음 (API 스펙 변경)

**수정 항목**:
- [ ] `/predict_dual` 엔드포인트 추가
- [ ] Request body 변경 (left_frame, right_frame)
- [ ] Response body 변경 (component_defects, solder_defects)
- [ ] 결과 융합 로직 설명

**예상 작업 시간**: 20분

---

#### 6. `docs/RaspberryPi_Setup.md` ⭐⭐
**영향도**: 높음 (클라이언트 코드 변경)

**수정 항목**:
- [ ] 양면 동시 캡처 코드
- [ ] 양면 동시 전송 코드
- [ ] GPIO 제어 로직 (융합 결과 기반)

**예상 작업 시간**: 25분

---

#### 7. `docs/YOLO_Training_Guide.md` ⭐⭐
**영향도**: 높음 (학습 프로세스 변경)

**수정 항목**:
- [ ] 이중 모델 독립 학습 가이드
- [ ] 모델 1 학습 (FPIC-Component)
- [ ] 모델 2 학습 (SolDef_AI)
- [ ] 각 모델별 하이퍼파라미터
- [ ] 성능 평가 기준

**예상 작업 시간**: 30분

---

#### 8. `docs/MySQL_Database_Design.md` ⭐
**영향도**: 중간 (스키마 약간 변경)

**수정 항목**:
- [ ] inspection_results 테이블 확인
- [ ] component_defects 컬럼 추가 (JSON)
- [ ] solder_defects 컬럼 추가 (JSON)
- [ ] 결과 융합 정보 저장

**예상 작업 시간**: 15분

---

### 🟢 **Priority 3: 보조 문서** (선택 수정)

#### 9. `docs/Component_Solder_Dataset_Integration.md` ⭐
**영향도**: 중간 (데이터셋 관련)

**조치**:
- [ ] 새로운 데이터셋으로 완전 재작성 또는
- [ ] 아카이브 후 새 문서 작성

**예상 작업 시간**: 20분

---

#### 10. `docs/Training_Issue_Analysis_Report.md` ⭐
**영향도**: 낮음 (구 데이터셋 분석)

**조치**:
- [ ] 아카이브 표시 추가
- [ ] "이 문서는 구 데이터셋 분석 결과입니다" 경고 추가
- [ ] 새 데이터셋으로 교체됨 명시

**예상 작업 시간**: 5분

---

#### 11. `docs/Phase1_YOLO_Setup.md` ⭐
**영향도**: 중간

**수정 항목**:
- [ ] 이중 모델 설정 가이드
- [ ] 환경 변수 (2개 모델)
- [ ] requirements.txt 업데이트 (Anomalib 제거)

**예상 작업 시간**: 15분

---

#### 12. `docs/CSharp_WinForms_*.md` (3개 파일)
**영향도**: 낮음 (UI만 영향)

**수정 항목**:
- [ ] 결과 표시 형식 변경 (component_defects, solder_defects)
- [ ] 통계 차트 업데이트

**예상 작업 시간**: 10분 (전체)

---

### ⚪ **Priority 4: 영향 없는 문서** (수정 불필요)

다음 문서들은 이중 모델 아키텍처 변경과 무관:
- `docs/Arduino_RobotArm_Setup.md`
- `docs/Development_Setup.md`
- `docs/Git_Workflow.md`
- `docs/Kaggle_Setup_Guide.md`
- `docs/Logging_Strategy.md`
- `docs/OHT_System_Setup.md`
- `docs/Project_Structure.md` (문서 목록만 업데이트)
- `docs/RaspberryPi_OHT_Setup.md`
- `docs/Remote_Network_Setup.md`
- `docs/Team_Collaboration_Guide.md`
- `docs/Team_Onboarding_Prompts.md`
- `docs/프로젝트_설계보고서.md` (필요 시 업데이트)

---

## 수정 순서 (권장)

### Phase 1: 핵심 문서 (필수, 1-2시간)
1. ✅ `docs/Dual_Model_Architecture.md` (이미 작성 완료)
2. `CLAUDE.md`
3. `docs/PCB_Defect_Detection_Project.md`
4. `docs/Dataset_Guide.md`

### Phase 2: 구현 가이드 (1시간)
5. `docs/Flask_Server_Setup.md`
6. `docs/YOLO_Training_Guide.md`
7. `docs/API_Contract.md`

### Phase 3: 클라이언트 및 DB (30분)
8. `docs/RaspberryPi_Setup.md`
9. `docs/MySQL_Database_Design.md`

### Phase 4: 정리 및 아카이브 (30분)
10. `docs/Training_Issue_Analysis_Report.md` (아카이브)
11. `docs/Component_Solder_Dataset_Integration.md` (재작성)
12. 기타 문서 마무리

---

## 총 예상 작업 시간

- **Priority 1**: 2시간 25분
- **Priority 2**: 1시간 30분
- **Priority 3**: 50분
- **총계**: 약 **4시간 45분**

---

## 검증 체크리스트

수정 완료 후 확인 사항:

- [ ] 모든 문서에서 "이상 탐지" 또는 "Anomalib" 언급 제거
- [ ] 모든 문서에서 "29개 클래스" 또는 "22개 클래스" 언급 제거
- [ ] 새로운 데이터셋 (FPIC-Component, SolDef_AI) 정확히 명시
- [ ] 이중 모델 아키텍처 일관되게 설명
- [ ] API 엔드포인트 `/predict_dual` 모든 곳에 반영
- [ ] GPIO 핀 매핑 일관성 (부품:17, 납땜:27, 폐기:22, 정상:23)
- [ ] 성능 목표 일관성 (80-100ms 추론, 300ms 이내)

---

## 다음 단계

1. 이 계획서 검토 및 승인
2. Phase 1부터 순차적으로 문서 수정
3. 각 Phase 완료 후 검증
4. 최종 완료 후 전체 문서 일관성 재확인
