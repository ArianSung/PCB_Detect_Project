# 라즈베리파이 팀원에게 전달 - 웹캠 클라이언트 개발 가이드

> Flask 서버 연동을 위한 필수 요구사항 및 API 명세

---

## 📌 중요 정보

### Flask 서버 정보
- **서버 URL**: `http://100.64.1.1:5000` (Tailscale VPN)
- **핵심 엔드포인트**: `/predict_dual` (양면 동시 검사)
- **상태 확인**: `/health`

### 시스템 구성
- **라즈베리파이 1**: 좌측 웹캠 (부품 검출) + 우측 웹캠 (납땜 검출) + GPIO 제어
- **Flask 서버**: 이중 YOLO 모델 추론 (현재 임시 응답)

---

## 🔧 1. 필수 요구사항

### 1-1. Python 라이브러리

```bash
# 필수 패키지 설치
pip3 install opencv-python requests RPi.GPIO python-dotenv

# 또는
pip3 install -r requirements.txt
```

**requirements.txt 내용:**
```
opencv-python>=4.5.0
requests>=2.25.0
RPi.GPIO>=0.7.0
python-dotenv>=0.19.0
numpy>=1.19.0
```

### 1-2. 웹캠 설정

```bash
# 웹캠 장치 확인
ls /dev/video*
# 출력 예: /dev/video0 /dev/video1

# 웹캠 권한 설정
sudo usermod -a -G video pi
sudo reboot
```

### 1-3. GPIO 권한 (라즈베리파이 1만)

```bash
# GPIO 그룹에 사용자 추가
sudo usermod -a -G gpio pi
sudo reboot
```

---

## 📡 2. API 명세 (핵심)

### 2-1. 엔드포인트: `/predict_dual` ⭐

**메서드**: POST
**Content-Type**: application/json

### 2-2. 요청 형식

```json
{
  "left_image": "base64_encoded_jpeg_string",
  "right_image": "base64_encoded_jpeg_string"
}
```

**필드:**
- `left_image` (string, 필수): 좌측 카메라 Base64 인코딩된 JPEG
- `right_image` (string, 필수): 우측 카메라 Base64 인코딩된 JPEG

### 2-3. 응답 형식

```json
{
  "status": "ok",
  "final_defect_type": "정상|부품불량|납땜불량|폐기",
  "final_confidence": 0.95,
  "left_result": {
    "defect_type": "정상",
    "confidence": 0.95,
    "boxes": []
  },
  "right_result": {
    "defect_type": "정상",
    "confidence": 0.95,
    "boxes": []
  },
  "gpio_signal": {
    "pin": 23,
    "duration_ms": 300
  },
  "robot_command": {
    "category": "NORMAL",
    "slot": 0
  },
  "inference_time_ms": 85.2,
  "timestamp": "2025-10-31T14:30:00.123456"
}
```

**핵심 필드:**
- `status`: "ok" (성공) 또는 "error" (실패)
- `final_defect_type`: 최종 판정 결과
- `gpio_signal.pin`: 활성화할 GPIO 핀 번호
- `robot_command.category`: 박스 카테고리
- `robot_command.slot`: 슬롯 번호

---

## 💻 3. 코드 예시

### 3-1. Base64 인코딩 (필수)

```python
import cv2
import base64

# 웹캠에서 프레임 캡처
cap = cv2.VideoCapture(0)
ret, frame = cap.read()

# JPEG 인코딩
_, buffer = cv2.imencode('.jpg', frame, [cv2.IMWRITE_JPEG_QUALITY, 85])

# Base64 변환
image_base64 = base64.b64encode(buffer).decode('utf-8')

cap.release()
```

### 3-2. API 호출 예시

```python
import cv2
import requests
import base64

# 양면 웹캠 캡처
cap_left = cv2.VideoCapture(0)   # 좌측 (부품)
cap_right = cv2.VideoCapture(1)  # 우측 (납땜)

ret_left, left_frame = cap_left.read()
ret_right, right_frame = cap_right.read()

# JPEG 인코딩 및 Base64 변환
_, left_buffer = cv2.imencode('.jpg', left_frame, [cv2.IMWRITE_JPEG_QUALITY, 85])
_, right_buffer = cv2.imencode('.jpg', right_frame, [cv2.IMWRITE_JPEG_QUALITY, 85])

left_base64 = base64.b64encode(left_buffer).decode('utf-8')
right_base64 = base64.b64encode(right_buffer).decode('utf-8')

# API 요청
response = requests.post(
    "http://100.64.1.1:5000/predict_dual",
    json={
        "left_image": left_base64,
        "right_image": right_base64
    },
    timeout=5
)

# 응답 처리
if response.status_code == 200:
    result = response.json()

    if result['status'] == 'ok':
        print(f"최종 판정: {result['final_defect_type']}")
        print(f"GPIO 핀: {result['gpio_signal']['pin']}")
        print(f"박스 슬롯: {result['robot_command']['category']} - {result['robot_command']['slot']}")
    else:
        print(f"오류: {result['error']}")
else:
    print(f"HTTP 오류: {response.status_code}")

cap_left.release()
cap_right.release()
```

### 3-3. GPIO 제어 예시 (라즈베리파이 1만)

```python
import RPi.GPIO as GPIO
import time

# GPIO 초기화
GPIO.setmode(GPIO.BCM)
GPIO.setwarnings(False)

# GPIO 핀 설정
pins = {
    '정상': 23,
    '부품불량': 17,
    '납땜불량': 27,
    '폐기': 22
}

for pin in pins.values():
    GPIO.setup(pin, GPIO.OUT)
    GPIO.output(pin, GPIO.LOW)

def trigger_gpio(defect_type, duration_ms=300):
    """GPIO 핀 활성화"""
    pin = pins.get(defect_type)

    if pin:
        print(f"GPIO {pin} 활성화 ({defect_type})")
        GPIO.output(pin, GPIO.HIGH)
        time.sleep(duration_ms / 1000.0)
        GPIO.output(pin, GPIO.LOW)

# 사용 예시
# Flask 응답에서 받은 defect_type으로 GPIO 제어
trigger_gpio('정상')

# 종료 시
GPIO.cleanup()
```

---

## 🎯 4. GPIO 핀 매핑 (BCM 모드)

| 불량 분류 | GPIO 핀 | 물리 핀 | 용도 |
|-----------|---------|---------|------|
| **정상** | GPIO 23 | Pin 16 | 릴레이 4 |
| **부품 불량** | GPIO 17 | Pin 11 | 릴레이 1 |
| **납땜 불량** | GPIO 27 | Pin 13 | 릴레이 2 |
| **폐기** | GPIO 22 | Pin 15 | 릴레이 3 |

**⚠️ 주의**: GPIO 제어는 **라즈베리파이 1에만** 구현. 라즈베리파이 2는 카메라 전용.

---

## 🧪 5. 테스트 방법

### 5-1. 웹캠 스트리밍 테스트 ⭐ (가장 먼저 테스트!)

**목적**: 웹캠이 제대로 작동하는지 웹 브라우저에서 실시간으로 확인

```bash
# 1. Flask 서버 실행 (GPU PC에서)
cd server
python app.py

# 2. 라즈베리파이에서 테스트 클라이언트 실행
# 단일 웹캠 (좌측)
python3 raspberry_pi/test_webcam_stream.py left 0 http://100.64.1.1:5000

# 또는 양면 웹캠 동시 전송
python3 raspberry_pi/test_webcam_stream.py dual 0 1 http://100.64.1.1:5000

# 3. 웹 브라우저에서 확인
# http://100.64.1.1:5000/viewer 접속
```

**웹 브라우저 화면**:
- 좌측/우측 카메라 실시간 영상 표시
- 1초마다 자동 갱신
- 상태 정보 표시

**예상 출력** (라즈베리파이):
```
🎥 단일 웹캠 스트리밍 시작
   카메라: left
   장치: /dev/video0
   FPS: 1
   서버: http://100.64.1.1:5000

💡 웹 브라우저에서 확인: http://100.64.1.1:5000/viewer
종료: Ctrl+C

[14:30:01] ✓ left 프레임 업로드 성공
[14:30:02] ✓ left 프레임 업로드 성공
...
```

---

### 5-2. Flask 서버 연결 테스트

```bash
# 서버 상태 확인
curl http://100.64.1.1:5000/health

# 예상 응답:
# {"status":"ok","timestamp":"2025-11-08T...","server":"Flask PCB Inspection Server"}
```

### 5-2. 웹캠 테스트

```python
import cv2

# 웹캠 열기
cap = cv2.VideoCapture(0)

if cap.isOpened():
    ret, frame = cap.read()
    if ret:
        print(f"✓ 웹캠 OK: 해상도 {frame.shape}")
    else:
        print("✗ 프레임 읽기 실패")
else:
    print("✗ 웹캠 열기 실패")

cap.release()
```

### 5-3. GPIO 테스트 (라즈베리파이 1만)

```python
import RPi.GPIO as GPIO
import time

GPIO.setmode(GPIO.BCM)

# 테스트할 핀 (정상 = GPIO 23)
test_pin = 23
GPIO.setup(test_pin, GPIO.OUT)

# 1초 깜빡임
GPIO.output(test_pin, GPIO.HIGH)
time.sleep(1)
GPIO.output(test_pin, GPIO.LOW)

print(f"✓ GPIO {test_pin} 테스트 완료")
GPIO.cleanup()
```

---

## 📋 6. 개발 체크리스트

### 환경 설정
- [ ] Python 3.10+ 설치 확인
- [ ] 필수 라이브러리 설치 (opencv-python, requests, RPi.GPIO)
- [ ] 웹캠 2대 연결 및 인식 확인 (/dev/video0, /dev/video1)
- [ ] Tailscale VPN 연결 (서버 IP: 100.64.1.1)

### 기능 구현
- [ ] 양면 웹캠 동시 캡처
- [ ] JPEG 인코딩 및 Base64 변환
- [ ] `/predict_dual` API 호출
- [ ] 응답 파싱 및 오류 처리
- [ ] GPIO 제어 (라즈베리파이 1만)

### 테스트
- [ ] Flask 서버 연결 테스트 (/health)
- [ ] 웹캠 캡처 테스트
- [ ] API 호출 테스트 (/predict_dual)
- [ ] GPIO 제어 테스트 (LED 또는 릴레이)

---

## 🚨 주의사항

### 1. Base64 인코딩
- **반드시** JPEG로 인코딩한 후 Base64 변환
- 품질: 85 권장 (파일 크기와 화질 균형)
- 해상도: 640x480 권장

### 2. 에러 처리
- 네트워크 타임아웃: 5초 설정
- 웹캠 오류 시 재시도 로직
- Flask 서버 오류 응답 처리

### 3. GPIO 제어
- **root 권한 필요**: `sudo python3 script.py` 또는 gpio 그룹 추가
- **정리 필수**: 종료 시 `GPIO.cleanup()` 호출

### 4. 현재 상태
- ⚠️ Flask 서버의 YOLO 모델은 아직 None (임시로 "정상" 반환)
- 실제 불량 검출은 YOLO 모델 학습 완료 후 가능
- 현재는 API 연동 및 GPIO 제어 테스트 가능

---

## 📚 참고 문서

자세한 내용은 다음 문서를 참조하세요:

1. **API 전체 명세서**: `docs/API_Contract.md`
2. **라즈베리파이 설정**: `docs/RaspberryPi_Setup.md`
3. **Flask 서버**: `server/app.py`

---

## 💬 질문 사항

- API 관련: Flask 서버 담당자
- 하드웨어 관련: 라즈베리파이 팀 리더
- 전체 시스템: 프로젝트 PM

---

**마지막 업데이트**: 2025-11-08
**작성자**: Claude Code AI Assistant
