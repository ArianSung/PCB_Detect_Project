#!/bin/bash
################################################################################
# 데이터셋 다운로드 안내 (ARCHIVED)
################################################################################

cat <<'MSG'
⚠️  v3.0부터 공개 SolDef_AI/FPIC 데이터셋을 통합하는 방식은 사용하지 않습니다.

현재 파이프라인에서는 다음이 필요합니다:
1. 앞면 부품 검출용 커스텀 데이터셋 (제품 FT/RS/BC) → `data/processed/pcb_components/`
2. 뒷면 Backscan용 시리얼/QR 데이터셋 → `data/raw/serial_number_detection/`

데이터 수집/라벨링 절차는 `docs/Dataset_Guide.md` 와 `docs/Serial_Number_Detection_Guide.md` 를 참고하세요.
이 스크립트는 역사적 참조용으로만 남겨둡니다.
MSG

exit 1
