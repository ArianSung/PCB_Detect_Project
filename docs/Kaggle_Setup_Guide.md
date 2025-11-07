# Kaggle API 설정 가이드

## Kaggle API 토큰 설정

### 1. Kaggle API 토큰 발급

1. **Kaggle 웹사이트** 접속: https://www.kaggle.com/
2. 로그인 (계정이 없으면 가입)
3. 우측 상단 프로필 아이콘 클릭 → **Account** 선택
4. API 섹션에서 **Create New API Token** 클릭
5. `kaggle.json` 파일 자동 다운로드

### 2. WSL/Linux에서 설정

```bash
# kaggle.json을 WSL로 복사 (Windows에서 다운로드한 경우)
# Windows 다운로드 폴더 경로 예시: /mnt/c/Users/<사용자명>/Downloads/kaggle.json

# 1. .kaggle 디렉토리 생성
mkdir -p ~/.kaggle

# 2. kaggle.json 파일 이동 (Windows Downloads에서)
cp /mnt/c/Users/<사용자명>/Downloads/kaggle.json ~/.kaggle/

# 또는 직접 파일 생성 (API 키 정보를 알고 있는 경우)
# cat > ~/.kaggle/kaggle.json <<EOF
# {"username":"YOUR_USERNAME","key":"YOUR_API_KEY"}
# EOF

# 3. 권한 설정 (보안상 필수)
chmod 600 ~/.kaggle/kaggle.json

# 4. 확인
ls -la ~/.kaggle/
# -rw------- 1 sys1041 sys1041 73 Oct 28 16:50 kaggle.json

# 5. 테스트
kaggle datasets list
```

### 3. kaggle.json 파일 형식

```json
{
  "username": "your_kaggle_username",
  "key": "your_api_key_here"
}
```

## Kaggle PCB Defects 데이터셋 다운로드

### 옵션 1: akhatova/pcb-defects (추천, 1,386장)

```bash
# 작업 디렉토리로 이동
cd /home/sys1041/work_project/data/raw

# 데이터셋 다운로드
kaggle datasets download -d akhatova/pcb-defects

# 압축 해제
unzip pcb-defects.zip -d kaggle_pcb_defects

# 확인
ls -lh kaggle_pcb_defects/
```

### 옵션 2: tanishqgautam/pcb-defect-detection (693장)

```bash
cd /home/sys1041/work_project/data/raw

kaggle datasets download -d tanishqgautam/pcb-defect-detection

unzip pcb-defect-detection.zip -d kaggle_pcb_tanishq

ls -lh kaggle_pcb_tanishq/
```

## 문제 해결

### 401 Unauthorized Error

```
401 - Unauthorized
```

**해결 방법**:
1. `~/.kaggle/kaggle.json` 파일이 존재하는지 확인
2. 파일 권한이 `600`인지 확인
3. API 토큰이 최신인지 확인 (만료되었으면 재발급)

### 403 Forbidden Error

```
403 - Forbidden
```

**해결 방법**:
1. Kaggle 웹사이트에서 해당 데이터셋 페이지 방문
2. 데이터셋 라이선스 동의 (Accept 버튼 클릭)
3. 다시 다운로드 시도

### OSError: [Errno 30] Read-only file system

**해결 방법**:
WSL의 /tmp 디렉토리 권한 문제일 수 있습니다.

```bash
# 다운로드 경로 직접 지정
kaggle datasets download -d akhatova/pcb-defects -p /home/sys1041/work_project/data/raw
```

## 다음 단계

Kaggle 데이터셋 다운로드 후:

1. **YOLO 형식 변환**
   ```bash
   cd /home/sys1041/work_project
   python yolo/convert_kaggle_to_yolo.py
   ```

2. **DeepPCB + Kaggle 데이터셋 통합**
   ```bash
   python yolo/merge_datasets.py
   ```

3. **YOLO 학습 시작**
   ```bash
   yolo detect train data=data/processed/combined_pcb_dataset/data.yaml \\
     model=yolo11l.pt epochs=150 batch=32 imgsz=640
   ```

## 참고 자료

- Kaggle API 문서: https://github.com/Kaggle/kaggle-api
- PCB Defects by Akhatova: https://www.kaggle.com/datasets/akhatova/pcb-defects
- PCB Defect Detection by Tanishq: https://www.kaggle.com/datasets/tanishqgautam/pcb-defect-detection
