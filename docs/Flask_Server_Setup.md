# Flask 웹서버 기반 실시간 PCB 검사 시스템 구축 가이드

## 개요

이 가이드는 컨베이어 벨트 시스템에서 웹캠으로 촬영한 PCB 이미지를 실시간으로 Flask 서버로 전송하여 AI 추론을 수행하는 시스템 구축 방법을 설명합니다.

---

## 시스템 구성

### 하드웨어 구성 ⭐ 업데이트
- **추론 서버 (GPU PC)**:
  - GPU: NVIDIA RTX 4080 Super (16GB VRAM)
  - **AI 모델**: 이중 YOLOv11l (Large) 모델 ⭐ 변경
    - **모델 1**: FPIC-Component (부품 검출, 25개 클래스)
    - **모델 2**: SolDef_AI (납땜 불량 검출, 5-6개 클래스)
  - 위치: 원격지 (같은 도시 내)
  - Flask 서버 실행:
    - 로컬: 100.64.1.1:5000 (선택)
    - 원격 (Tailscale): 100.x.x.x:5000 ⭐
- **라즈베리파이 1**: 좌측 웹캠 (부품면) + GPIO 제어
- **라즈베리파이 2**: 우측 웹캠 (납땜면) 전용
- **라즈베리파이 3번 (OHT 컨트롤러)**: OHT 시스템 전용 제어기 ⭐
- **Windows PC**: C# WinForms 모니터링 앱
- **네트워크**:
  - 로컬 환경 (선택): LAN (192.168.0.x)
  - 원격 환경 (프로젝트): Tailscale VPN 메시 네트워크 ⭐

**참고**:
- 상세한 라즈베리파이 설정: `RaspberryPi_Setup.md`
- OHT 시스템 설정: `OHT_System_Setup.md` ⭐
- 데이터베이스 설계: `MySQL_Database_Design.md`

### 소프트웨어 구성
- **추론 서버**: Flask + PyTorch + YOLO v11l
- **카메라 클라이언트**: OpenCV + Requests

---

## Phase 1: Flask 추론 서버 구축

### 1-1. 필수 패키지 설치

```bash
# 가상환경 활성화
conda activate pcb_defect

# Flask 및 관련 패키지 설치
pip install flask flask-cors
pip install pillow opencv-python-headless
pip install requests
```

### 1-2. 프로젝트 폴더 구조 (간소화)

```
~/work_project/
├── server/                     # Flask 추론 서버 (GPU PC)
│   ├── app.py                  # Flask 메인 애플리케이션
│   └── routes/                 # API 모듈화 (필요 시)
│
├── raspberry_pi/               # 웹캠/GPIO 클라이언트 (라즈베리파이)
│   └── GETTING_STARTED.md      # 카메라 클라이언트 가이드
│
├── yolo/                       # YOLO 학습 및 평가 스크립트
│   ├── train_yolo.py
│   └── tests/
│
├── models/                     # 학습된 모델 파일
│   ├── yolo/
│   └── anomaly/
│
├── data/
│   └── pcb_defects.yaml        # YOLO 클래스 정의 (통일된 참조)
│
├── configs/                    # 설정 파일
│   ├── server_config.yaml      # Flask 서버 설정
│   └── camera_config.yaml      # 카메라 클라이언트 설정
│
└── scripts/                    # 자동화 스크립트
    ├── train_yolo.sh
    ├── start_server.sh
    └── setup_env.sh

참고: 라즈베리파이 클라이언트 코드는 `raspberry_pi/` 디렉터리에 위치
```

### 1-3. Flask 추론 서버 코드 (server/app.py)

```python
from flask import Flask, request, jsonify
from flask_cors import CORS
import base64
import cv2
import numpy as np
from inference import PCBInferenceEngine
import logging
from datetime import datetime

app = Flask(__name__)
CORS(app)  # Cross-Origin 요청 허용

# 로깅 설정
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# 이중 모델 추론 엔진 초기화 ⭐ 업데이트
inference_engine = DualModelInferenceEngine(
    component_model_path='models/fpic_component_best.pt',  # 부품 검출 모델
    solder_model_path='models/soldef_ai_best.pt',         # 납땜 불량 모델
    device='cuda'  # GPU 사용
)

@app.route('/health', methods=['GET'])
def health_check():
    """서버 상태 확인"""
    return jsonify({
        'status': 'ok',
        'timestamp': datetime.now().isoformat()
    })

@app.route('/predict', methods=['POST'])
def predict():
    """PCB 불량 검사 추론"""
    try:
        # JSON 데이터 파싱
        data = request.get_json()
        camera_id = data.get('camera_id', 'unknown')  # 'left' or 'right'
        frame_base64 = data.get('frame')
        timestamp = data.get('timestamp')

        if not frame_base64:
            return jsonify({'error': 'No frame data'}), 400

        # Base64 디코딩
        frame_bytes = base64.b64decode(frame_base64)
        nparr = np.frombuffer(frame_bytes, np.uint8)
        frame = cv2.imdecode(nparr, cv2.IMREAD_COLOR)

        if frame is None:
            return jsonify({'error': 'Failed to decode frame'}), 400

        logger.info(f"Received frame from {camera_id}, shape: {frame.shape}")

        # AI 추론 실행
        result = inference_engine.predict(frame, camera_id)

        # 응답 생성
        response = {
            'status': 'ok',
            'camera_id': camera_id,
            'timestamp': timestamp,
            'defect_type': result['defect_type'],  # '정상', '부품불량', '납땜불량', '폐기'
            'confidence': float(result['confidence']),
            'boxes': result['boxes'],  # 불량 위치 좌표
            'inference_time_ms': float(result['inference_time_ms'])
        }

        logger.info(f"Inference result: {result['defect_type']} (confidence: {result['confidence']:.2f})")

        return jsonify(response)

    except Exception as e:
        logger.error(f"Error during prediction: {str(e)}")
        return jsonify({'error': str(e)}), 500

@app.route('/predict_dual', methods=['POST'])
def predict_dual():
    """
    양면(좌측+우측) 동시 검사 - 이중 YOLO 모델 ⭐ 업데이트

    아키텍처:
    1. 좌측 프레임 → FPIC-Component 모델 (부품 검출, 25 클래스)
    2. 우측 프레임 → SolDef_AI 모델 (납땜 불량, 5-6 클래스)
    3. 결과 융합 로직 → 최종 판정 (normal/component_defect/solder_defect/discard)
    4. 라즈베리파이 1에만 GPIO 제어 신호 전송

    추론 시간: 80-100ms (병렬 처리)
    """
    try:
        data = request.get_json()
        left_frame_base64 = data.get('left_frame')   # 부품면 (앞면)
        right_frame_base64 = data.get('right_frame')  # 납땜면 (뒷면)
        timestamp = data.get('timestamp')

        if not left_frame_base64 or not right_frame_base64:
            return jsonify({'error': 'Missing frame data'}), 400

        # 프레임 디코딩
        left_frame = decode_frame(left_frame_base64)
        right_frame = decode_frame(right_frame_base64)

        if left_frame is None or right_frame is None:
            return jsonify({'error': 'Failed to decode frames'}), 400

        logger.info(f"Dual inference - Left: {left_frame.shape}, Right: {right_frame.shape}")

        # 이중 모델 병렬 추론 (80-100ms)
        start_time = time.time()

        # 모델 1: 부품 검출 (50-80ms)
        component_result = inference_engine.predict_component(left_frame)

        # 모델 2: 납땜 불량 (30-50ms, 병렬)
        solder_result = inference_engine.predict_solder(right_frame)

        # 결과 융합 로직 (<5ms)
        final_decision = inference_engine.fuse_results(component_result, solder_result)

        inference_time = (time.time() - start_time) * 1000  # ms

        # 응답 생성
        response = {
            'status': 'ok',
            'decision': final_decision['type'],  # normal/component_defect/solder_defect/discard
            'component_defects': final_decision['component_defects'],
            'solder_defects': final_decision['solder_defects'],
            'component_severity': final_decision['component_severity'],  # 0-3
            'solder_severity': final_decision['solder_severity'],        # 0-3
            'gpio_signal': gpio_signal,  # 라즈베리파이 1 전용 GPIO 제어 신호
            'left': result_left,
            'right': result_right,
            'note': 'GPIO 제어는 라즈베리파이 1 (100.64.1.2)만 수행'
        }

        return jsonify(response)

    except Exception as e:
        logger.error(f"Error during dual prediction: {str(e)}")
        return jsonify({'error': str(e)}), 500

def decode_frame(base64_str):
    """Base64 문자열을 OpenCV 이미지로 변환"""
    frame_bytes = base64.b64decode(base64_str)
    nparr = np.frombuffer(frame_bytes, np.uint8)
    return cv2.imdecode(nparr, cv2.IMREAD_COLOR)

def integrate_results(result_left, result_right):
    """
    양면 검사 결과 통합

    우선순위:
    1. 폐기 (심각한 불량)
    2. 납땜불량
    3. 부품불량
    4. 정상

    Returns:
        tuple: (최종_불량_유형, GPIO_제어_신호)
    """
    # data/pcb_defects.yaml에 정의된 GPIO 매핑 사용
    gpio_mapping = {
        '부품불량': {'pin': 17, 'duration_ms': 500},
        '납땜불량': {'pin': 27, 'duration_ms': 500},
        '폐기': {'pin': 22, 'duration_ms': 500},
        '정상': {'pin': 23, 'duration_ms': 300}
    }

    # 둘 중 하나라도 불량이면 불량 판정
    if result_left['defect_type'] == '폐기' or result_right['defect_type'] == '폐기':
        final_type = '폐기'
    elif result_left['defect_type'] == '납땜불량' or result_right['defect_type'] == '납땜불량':
        final_type = '납땜불량'
    elif result_left['defect_type'] == '부품불량' or result_right['defect_type'] == '부품불량':
        final_type = '부품불량'
    else:
        final_type = '정상'

    return final_type, gpio_mapping[final_type]

if __name__ == '__main__':
    # 0.0.0.0으로 바인딩하여 외부 접속 허용
    app.run(host='0.0.0.0', port=5000, debug=False, threaded=True)
```

### 1-4. AI 추론 엔진 (server/inference.py)

```python
from ultralytics import YOLO
import torch
import time
import numpy as np

class DualModelInferenceEngine:
    """
    이중 YOLO 모델 추론 엔진 ⭐ 업데이트

    아키텍처:
    - 모델 1: FPIC-Component (부품 검출, 25 클래스)
    - 모델 2: SolDef_AI (납땜 불량, 5-6 클래스)
    - 결과 융합 로직
    """

    def __init__(self, component_model_path, solder_model_path, device='cuda'):
        """
        이중 모델 초기화

        Args:
            component_model_path: 부품 검출 모델 경로 (FPIC-Component)
            solder_model_path: 납땜 불량 모델 경로 (SolDef_AI)
            device: 'cuda' 또는 'cpu'
        """
        self.device = device

        # 모델 1: 부품 검출 (FPIC-Component, 25 클래스)
        self.component_model = YOLO(component_model_path)
        self.component_model.to(device)

        # 모델 2: 납땜 불량 (SolDef_AI, 5-6 클래스)
        self.solder_model = YOLO(solder_model_path)
        self.solder_model.to(device)

        print(f"✅ Dual models loaded on {device}")
        print(f"  - Component model: {component_model_path}")
        print(f"  - Solder model: {solder_model_path}")

    def predict_component(self, frame):
        """
        부품 검출 추론 (FPIC-Component 모델)

        Args:
            frame: 좌측 카메라 이미지 (부품면)

        Returns:
            dict: 부품 검출 결과 (25개 클래스)
        """
        start_time = time.time()

        # 모델 1 추론: 부품 검출
        results = self.component_model(frame, verbose=False)

        # 결과 파싱
        detections = []
        for result in results:
            for box in result.boxes:
                x1, y1, x2, y2 = box.xyxy[0].cpu().numpy()
                conf = float(box.conf[0])
                cls = int(box.cls[0])
                cls_name = result.names[cls]

                detections.append({
                    'type': cls_name,
                    'bbox': [float(x1), float(y1), float(x2), float(y2)],
                    'confidence': conf
                })

        inference_time = (time.time() - start_time) * 1000  # ms
        return {'detections': detections, 'inference_time_ms': inference_time}

    def predict_solder(self, frame):
        """
        납땜 불량 검출 추론 (SolDef_AI 모델)

        Args:
            frame: 우측 카메라 이미지 (납땜면)

        Returns:
            dict: 납땜 불량 결과 (5-6개 클래스)
        """
        start_time = time.time()

        # 모델 2 추론: 납땜 불량
        results = self.solder_model(frame, verbose=False)

        # 결과 파싱
        defects = []
        for result in results:
            for box in result.boxes:
                x1, y1, x2, y2 = box.xyxy[0].cpu().numpy()
                conf = float(box.conf[0])
                cls = int(box.cls[0])
                cls_name = result.names[cls]

                defects.append({
                    'type': cls_name,  # no_good, exc_solder, spike, poor_solder, solder_bridge
                    'bbox': [float(x1), float(y1), float(x2), float(y2)],
                    'confidence': conf,
                    'severity': 'critical' if cls_name == 'solder_bridge' else 'normal'
                })

        inference_time = (time.time() - start_time) * 1000  # ms
        return {'defects': defects, 'inference_time_ms': inference_time}

    def fuse_results(self, component_result, solder_result):
        """
        이중 모델 결과 융합 로직 ⭐ 핵심

        Args:
            component_result: 부품 모델 결과
            solder_result: 납땜 모델 결과

        Returns:
            dict: 최종 판정 결과
                {
                    'type': 'normal' | 'component_defect' | 'solder_defect' | 'discard',
                    'component_defects': [...],
                    'solder_defects': [...],
                    'component_severity': 0-3,
                    'solder_severity': 0-3
                }

        융합 알고리즘:
        1. 부품 불량 심각도 계산 (Level 0-3)
        2. 납땜 불량 심각도 계산 (Level 0-3)
        3. 최종 판정:
           - 양면 정상 → normal
           - 폐기 조건 (severity >= 3 or 양면 >= 2) → discard
           - 부품 심각도 > 납땜 → component_defect
           - 납땜 심각도 >= 부품 → solder_defect
        """
        component_defects = component_result.get('detections', [])
        solder_defects = solder_result.get('defects', [])

        # 심각도 계산
        component_severity = self._calculate_severity(component_defects)
        solder_severity = self._calculate_severity(solder_defects)

        # 최종 판정
        if component_severity == 0 and solder_severity == 0:
            decision_type = 'normal'
        elif (component_severity >= 3 or solder_severity >= 3 or
              (component_severity >= 2 and solder_severity >= 2)):
            decision_type = 'discard'
        elif component_severity > solder_severity:
            decision_type = 'component_defect'
        else:
            decision_type = 'solder_defect'

        return {
            'type': decision_type,
            'component_defects': component_defects,
            'solder_defects': solder_defects,
            'component_severity': component_severity,
            'solder_severity': solder_severity
        }

    def _calculate_severity(self, defects):
        """
        불량 심각도 계산

        Level 0: 불량 없음
        Level 1: 경미한 불량 (1-2개)
        Level 2: 중간 불량 (3-5개)
        Level 3: 심각한 불량 (6개 이상 or 치명적 불량)
        """
        if not defects:
            return 0

        # 치명적 불량 타입 (즉시 Level 3)
        critical_types = ['solder_bridge', 'missing_component', 'wrong_component']

        # 치명적 불량 검출 시 즉시 Level 3
        if any(d.get('type') in critical_types for d in defects):
            return 3

        # 불량 개수로 판단
        count = len(defects)
        if count == 0:
            return 0
        elif count <= 2:
            return 1
        elif count <= 5:
            return 2
        else:
            return 3

        # 납땜 불량 확인
        for cls in defect_classes:
            if cls in solder_defects:
                return '납땜불량', max_confidence

        # 부품 불량 확인
        for cls in defect_classes:
            if cls in component_defects:
                return '부품불량', max_confidence

        return '정상', max_confidence
```

---

## Phase 2: 웹캠 클라이언트 구축

### 2-1. 웹캠 클라이언트 코드 (raspberry_pi/camera_client.py)

```python
import cv2
import requests
import base64
import time
import logging
from datetime import datetime

# 로깅 설정
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

class CameraClient:
    def __init__(self, camera_id, camera_index, server_url, fps=10):
        """
        웹캠 클라이언트 초기화

        Args:
            camera_id: 'left' or 'right'
            camera_index: 웹캠 인덱스 (0, 1, 2, ...)
            server_url: Flask 서버 URL (예: http://100.64.1.1:5000)
            fps: 초당 전송 프레임 수
        """
        self.camera_id = camera_id
        self.camera_index = camera_index
        self.server_url = server_url
        self.fps = fps
        self.frame_interval = 1.0 / fps

        # 웹캠 초기화
        self.cap = cv2.VideoCapture(camera_index)
        if not self.cap.isOpened():
            raise RuntimeError(f"Failed to open camera {camera_index}")

        # 해상도 설정 (640x480 권장)
        self.cap.set(cv2.CAP_PROP_FRAME_WIDTH, 640)
        self.cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 480)

        logger.info(f"Camera {camera_id} initialized (index: {camera_index})")

    def encode_frame(self, frame):
        """프레임을 Base64로 인코딩"""
        _, buffer = cv2.imencode('.jpg', frame, [cv2.IMWRITE_JPEG_QUALITY, 85])
        frame_base64 = base64.b64encode(buffer).decode('utf-8')
        return frame_base64

    def send_frame(self, frame):
        """프레임을 서버로 전송"""
        try:
            # 프레임 인코딩
            frame_base64 = self.encode_frame(frame)

            # 요청 데이터 생성
            data = {
                'camera_id': self.camera_id,
                'frame': frame_base64,
                'timestamp': datetime.now().isoformat()
            }

            # POST 요청 전송
            response = requests.post(
                f"{self.server_url}/predict",
                json=data,
                timeout=5  # 5초 타임아웃
            )

            if response.status_code == 200:
                result = response.json()
                logger.info(f"[{self.camera_id}] Result: {result['defect_type']} "
                           f"(confidence: {result['confidence']:.2f}, "
                           f"inference: {result['inference_time_ms']:.1f}ms)")
                return result
            else:
                logger.error(f"Server error: {response.status_code}")
                return None

        except requests.exceptions.Timeout:
            logger.error("Request timeout")
            return None
        except Exception as e:
            logger.error(f"Error sending frame: {str(e)}")
            return None

    def run(self):
        """메인 루프: 웹캠 프레임 캡처 및 전송"""
        logger.info(f"Starting camera client [{self.camera_id}]...")

        frame_count = 0
        last_send_time = time.time()

        try:
            while True:
                # 프레임 읽기
                ret, frame = self.cap.read()
                if not ret:
                    logger.warning("Failed to read frame")
                    continue

                frame_count += 1
                current_time = time.time()

                # FPS 제어: 지정된 간격마다 전송
                if current_time - last_send_time >= self.frame_interval:
                    # 서버로 전송
                    result = self.send_frame(frame)
                    last_send_time = current_time

                    # 결과 시각화 (선택)
                    if result:
                        self.visualize_result(frame, result)

                # 'q' 키를 누르면 종료
                if cv2.waitKey(1) & 0xFF == ord('q'):
                    break

        finally:
            self.cap.release()
            cv2.destroyAllWindows()
            logger.info("Camera client stopped")

    def visualize_result(self, frame, result):
        """추론 결과 시각화 (선택)"""
        # 불량 유형 표시
        defect_type = result['defect_type']
        confidence = result['confidence']

        # 색상 설정
        color = (0, 255, 0) if defect_type == '정상' else (0, 0, 255)

        # 텍스트 표시
        text = f"{defect_type} ({confidence:.2f})"
        cv2.putText(frame, text, (10, 30), cv2.FONT_HERSHEY_SIMPLEX,
                    1, color, 2)

        # 바운딩 박스 표시
        for box in result['boxes']:
            x1, y1, x2, y2 = int(box['x1']), int(box['y1']), int(box['x2']), int(box['y2'])
            cv2.rectangle(frame, (x1, y1), (x2, y2), color, 2)
            cv2.putText(frame, box['class'], (x1, y1-10),
                        cv2.FONT_HERSHEY_SIMPLEX, 0.5, color, 2)

        # 화면에 표시
        cv2.imshow(f'Camera {self.camera_id}', frame)

if __name__ == '__main__':
    # 설정
    CAMERA_ID = 'left'  # 'left' or 'right'
    CAMERA_INDEX = 0   # 웹캠 인덱스
    SERVER_URL = 'http://100.64.1.1:5000'  # 추론 서버 IP
    FPS = 10  # 초당 전송 프레임 수

    # 클라이언트 실행
    client = CameraClient(CAMERA_ID, CAMERA_INDEX, SERVER_URL, FPS)
    client.run()
```

---

## Phase 3: 시스템 실행 및 테스트

### 3-1. 추론 서버 실행 (GPU PC)

```bash
# 가상환경 활성화
conda activate pcb_defect

# 서버 실행
cd ~/work_project/server
python app.py

# 출력:
#  * Running on http://0.0.0.0:5000/
#  * Restarting with stat
# Models loaded on cuda
```

### 3-2. 웹캠 클라이언트 실행 (라즈베리파이)

**라즈베리파이 1 (좌측 웹캠)**
```bash
# 가상환경 활성화 (라즈베리파이에도 Python 환경 필요)
conda activate pcb_defect

cd ~/work_project/client
python camera_client.py

# 또는 파라미터 지정
python camera_client.py --camera_id left --camera_index 0 --server_url http://100.64.1.1:5000 --fps 10
```

**라즈베리파이 2 (우측 웹캠)**
```bash
python camera_client.py --camera_id right --camera_index 0 --server_url http://100.64.1.1:5000 --fps 10
```

### 3-3. 네트워크 설정

#### 로컬 네트워크 (선택)

1. **추론 서버 IP 확인**
```bash
# Linux/WSL
ip addr show

# 출력 예시: inet 100.64.1.1/24
```

2. **방화벽 설정**
```bash
# Ubuntu/WSL에서 포트 5000 오픈
sudo ufw allow 5000/tcp
```

3. **Windows 방화벽 설정**
   - Windows Defender 방화벽 → 고급 설정
   - 인바운드 규칙 → 새 규칙 → 포트 5000 허용

#### 원격 네트워크 (Tailscale VPN) ⭐ 프로젝트 환경

**GPU PC가 원격지에 있을 경우, Tailscale VPN 사용**

1. **Tailscale 설치 (GPU PC - WSL/Linux)**:
```bash
curl -fsSL https://tailscale.com/install.sh | sh
sudo tailscale up
# Tailscale IP 확인
tailscale ip -4
# 출력 예시: 100.64.1.1
```

2. **Flask 서버 실행 (VPN 지원)**:
```bash
# 0.0.0.0으로 바인딩하여 모든 인터페이스에서 접근 가능
python app.py
# 출력:
#  * Running on http://0.0.0.0:5000/
#  * Running on http://100.64.1.1:5000/  ← Tailscale IP
```

3. **라즈베리파이에서 Tailscale 설치**:
```bash
curl -fsSL https://tailscale.com/install.sh | sh
sudo tailscale up
# Tailscale IP 확인: 예시 100.64.1.2, 100.64.1.3
```

4. **클라이언트 설정 (camera_client.py)**:
```python
# Tailscale IP로 서버 URL 설정
SERVER_URL = 'http://100.64.1.1:5000'  # GPU PC의 Tailscale IP
```

**상세 가이드**: `docs/Remote_Network_Setup.md` 참조

---

## Phase 4: 성능 최적화

### 4-1. GPU 최적화 (RTX 4080 Super 최적화)

```python
# server/inference.py 수정

class PCBInferenceEngine:
    def __init__(self, yolo_model_path, anomaly_model_path, device='cuda'):
        self.device = device

        # YOLO 모델 로드 및 최적화
        self.yolo_model = YOLO(yolo_model_path)
        self.yolo_model.to(device)

        # FP16 (Half Precision) 사용 ⭐ 강력 권장
        # RTX 4080 Super: VRAM 50% 절약 + 속도 1.5배 향상
        if device == 'cuda':
            self.yolo_model.half()
            print("✅ FP16 모드 활성화: VRAM 절약 + 속도 향상")

        # Warm-up (첫 추론 속도 개선)
        dummy_input = torch.randn(1, 3, 640, 640).to(device)
        if device == 'cuda':
            dummy_input = dummy_input.half()
        _ = self.yolo_model(dummy_input, verbose=False)
        print(f"✅ 모델 로드 완료 (VRAM 사용: {torch.cuda.memory_allocated() / 1024**3:.2f}GB)")
```

**RTX 4080 Super 최적화 팁**:
1. **FP16 사용**: `model.half()` - VRAM 50% 절약
2. **배치 처리**: 좌우 2개 이미지를 `batch=2`로 한 번에 처리 (순차 대비 30% 빠름)
3. **CUDA 스트림**: 고급 사용자용 병렬 처리
4. **모니터링**: `nvidia-smi` 또는 `torch.cuda.memory_allocated()` 실시간 확인

### 4-2. 배치 처리

```python
# server/app.py에 배치 처리 추가

from queue import Queue
import threading

# 프레임 큐
frame_queue = Queue(maxsize=10)
result_queue = Queue()

def batch_inference_worker():
    """배치 추론 워커 스레드"""
    batch_size = 4
    frames_batch = []
    metadata_batch = []

    while True:
        # 큐에서 프레임 수집
        if not frame_queue.empty():
            frame, metadata = frame_queue.get()
            frames_batch.append(frame)
            metadata_batch.append(metadata)

        # 배치 크기만큼 모이면 추론
        if len(frames_batch) >= batch_size:
            results = inference_engine.predict_batch(frames_batch)
            for result, metadata in zip(results, metadata_batch):
                result_queue.put((result, metadata))

            frames_batch = []
            metadata_batch = []

# 워커 스레드 시작
threading.Thread(target=batch_inference_worker, daemon=True).start()
```

### 4-3. 프레임 스킵 로직

```python
# raspberry_pi/camera_client.py 수정

class CameraClient:
    def __init__(self, camera_id, camera_index, server_url, fps=10, skip_on_delay=True):
        # ... 기존 코드 ...
        self.skip_on_delay = skip_on_delay
        self.max_delay_ms = 500  # 최대 허용 지연 (밀리초)

    def run(self):
        # ... 기존 코드 ...

        while True:
            ret, frame = self.cap.read()
            if not ret:
                continue

            current_time = time.time()

            # 지연 확인
            if self.skip_on_delay:
                # 이전 요청이 아직 처리 중이면 프레임 스킵
                if hasattr(self, 'last_request_time'):
                    delay_ms = (current_time - self.last_request_time) * 1000
                    if delay_ms < self.max_delay_ms:
                        continue  # 프레임 스킵

            # 프레임 전송
            self.last_request_time = current_time
            result = self.send_frame(frame)
```

---

## Phase 5: 로봇팔 및 박스 관리 시스템

### 5-1. 로봇팔 제어 모듈 (server/robot_arm.py)

```python
import serial
import json
import logging
import time
from threading import Lock

logger = logging.getLogger(__name__)

class RobotArmController:
    def __init__(self, port='/dev/ttyACM0', baudrate=115200, timeout=5):
        """
        로봇팔 컨트롤러 초기화

        Args:
            port: Arduino 시리얼 포트
            baudrate: 통신 속도
            timeout: 읽기 타임아웃 (초)
        """
        self.port = port
        self.baudrate = baudrate
        self.timeout = timeout
        self.serial = None
        self.lock = Lock()  # 스레드 안전성

        self.connect()

    def connect(self):
        """Arduino와 시리얼 연결"""
        try:
            self.serial = serial.Serial(
                port=self.port,
                baudrate=self.baudrate,
                timeout=self.timeout,
                write_timeout=2
            )
            time.sleep(2)  # Arduino 리셋 대기
            logger.info(f"Arduino 연결 성공: {self.port}")

            # 연결 확인 (status 명령)
            response = self.send_command('status')
            if response and response.get('status') == 'ok':
                logger.info("Arduino 상태 정상")
            else:
                logger.warning("Arduino 응답 없음")

        except serial.SerialException as e:
            logger.error(f"Arduino 연결 실패: {str(e)}")
            raise

    def send_command(self, command, **kwargs):
        """
        Arduino에 JSON 명령 전송

        Args:
            command: 명령어 ('place_pcb', 'home', 'status' 등)
            **kwargs: 명령어 파라미터

        Returns:
            dict: Arduino 응답 (JSON)
        """
        with self.lock:
            try:
                # JSON 명령 생성
                cmd_data = {'command': command, **kwargs}
                cmd_json = json.dumps(cmd_data) + '\n'

                # 전송
                self.serial.write(cmd_json.encode('utf-8'))
                logger.debug(f"명령 전송: {cmd_json.strip()}")

                # 응답 대기 (타임아웃 내)
                response_line = self.serial.readline().decode('utf-8').strip()

                if not response_line:
                    logger.warning("Arduino 응답 없음 (타임아웃)")
                    return None

                # JSON 파싱
                response = json.loads(response_line)
                logger.debug(f"Arduino 응답: {response}")

                return response

            except serial.SerialTimeoutException:
                logger.error("시리얼 타임아웃")
                return None
            except json.JSONDecodeError as e:
                logger.error(f"JSON 파싱 실패: {str(e)}")
                return None
            except Exception as e:
                logger.error(f"시리얼 통신 오류: {str(e)}")
                return None

    def place_pcb(self, box_id, slot_index):
        """
        PCB 배치 명령

        Args:
            box_id: 박스 ID ('NORMAL_A', 'COMPONENT_DEFECT_B' 등)
            slot_index: 슬롯 인덱스 (0-2)

        Returns:
            bool: 성공 여부
        """
        response = self.send_command('place_pcb', box_id=box_id, slot_index=slot_index)

        if response and response.get('status') == 'ok':
            logger.info(f"PCB 배치 성공: {box_id} slot {slot_index}")
            return True
        else:
            error_msg = response.get('message', 'Unknown error') if response else 'No response'
            logger.error(f"PCB 배치 실패: {error_msg}")
            return False

    def move_home(self):
        """홈 포지션으로 이동"""
        response = self.send_command('home')
        return response and response.get('status') == 'ok'

    def get_status(self):
        """Arduino 상태 조회"""
        return self.send_command('status')

    def close(self):
        """시리얼 연결 종료"""
        if self.serial and self.serial.is_open:
            self.serial.close()
            logger.info("Arduino 연결 종료")
```

### 5-2. 박스 관리 모듈 (server/box_manager.py)

```python
import logging
from datetime import datetime
import requests

logger = logging.getLogger(__name__)

class BoxManager:
    def __init__(self, db_service, oht_api_url='http://localhost:5000'):
        """
        박스 관리자 초기화

        Args:
            db_service: DatabaseService 인스턴스
            oht_api_url: OHT API 엔드포인트 URL
        """
        self.db = db_service
        self.max_slots = 3  # 박스당 최대 슬롯 수 (수평 3슬롯)
        self.oht_api_url = oht_api_url

    def get_next_available_slot(self, category):
        """
        지정된 카테고리의 다음 사용 가능한 슬롯 조회

        Args:
            category: 불량 카테고리 ('normal', 'component_defect', 'solder_defect', 'discard')

        Returns:
            tuple: (box_id, slot_index) 또는 (None, None) if 박스 꽉 참
                   DISCARD의 경우 (box_id, 0) 반환 (슬롯 관리 안 함)
        """
        try:
            # DISCARD는 슬롯 관리 안 함 (항상 같은 위치에 떨어뜨리기)
            if category == 'discard':
                return 'DISCARD', 0

            conn = self.db.get_connection()
            with conn.cursor() as cursor:
                # 박스 ID 생성 (더 이상 A/B 구분 없음)
                box_id = category.upper()
                sql = "SELECT current_slot, is_full FROM box_status WHERE box_id = %s"
                cursor.execute(sql, (box_id,))
                box = cursor.fetchone()

                if box and not box['is_full']:
                    return box_id, box['current_slot']

                # 박스가 꽉 참
                logger.warning(f"카테고리 {category} 박스가 꽉 참! (3개 슬롯 모두 사용됨)")
                return None, None

        except Exception as e:
            logger.error(f"슬롯 조회 실패: {str(e)}")
            return None, None
        finally:
            conn.close()

    def update_box_status(self, box_id, slot_index):
        """
        PCB 배치 후 박스 상태 업데이트

        Args:
            box_id: 박스 ID ('NORMAL', 'COMPONENT_DEFECT', 'SOLDER_DEFECT', 'DISCARD')
            slot_index: 사용된 슬롯 인덱스

        Returns:
            bool: 성공 여부
        """
        try:
            # DISCARD는 슬롯 관리 안 함 (상태 업데이트 안 함)
            if box_id == 'DISCARD':
                logger.info(f"DISCARD 박스는 슬롯 관리 안 함 (상태 업데이트 생략)")
                return True

            conn = self.db.get_connection()
            with conn.cursor() as cursor:
                # 다음 슬롯 계산
                next_slot = slot_index + 1
                is_full = (next_slot >= self.max_slots)

                # 박스 상태 업데이트
                sql = """UPDATE box_status
                         SET current_slot = %s,
                             is_full = %s,
                             total_pcb_count = total_pcb_count + 1,
                             last_updated = NOW()
                         WHERE box_id = %s"""

                cursor.execute(sql, (next_slot, is_full, box_id))

                # 박스가 꽉 찼으면 알림 및 자동 OHT 호출
                if is_full:
                    logger.warning(f"박스 {box_id}가 꽉 참! (3개 슬롯 모두 사용됨)")
                    self.send_box_full_alert(box_id)
                    # 자동 OHT 호출 (박스 3/3 꽉 참)
                    self._trigger_auto_oht(box_id)

            conn.commit()
            logger.info(f"박스 상태 업데이트: {box_id}, 다음 슬롯: {next_slot}, 꽉 참: {is_full}")
            return True

        except Exception as e:
            logger.error(f"박스 상태 업데이트 실패: {str(e)}")
            return False
        finally:
            conn.close()

    def reset_box(self, box_id):
        """
        박스 리셋 (OHT가 박스를 교체한 후)

        Args:
            box_id: 박스 ID

        Returns:
            bool: 성공 여부
        """
        try:
            conn = self.db.get_connection()
            with conn.cursor() as cursor:
                sql = """UPDATE box_status
                         SET current_slot = 0,
                             is_full = FALSE,
                             total_pcb_count = 0,
                             last_updated = NOW()
                         WHERE box_id = %s"""

                cursor.execute(sql, (box_id,))

            conn.commit()
            logger.info(f"박스 리셋 완료: {box_id}")
            return True

        except Exception as e:
            logger.error(f"박스 리셋 실패: {str(e)}")
            return False
        finally:
            conn.close()

    def get_all_box_status(self):
        """모든 박스 상태 조회 (3개 박스만)"""
        try:
            conn = self.db.get_connection()
            with conn.cursor() as cursor:
                sql = """SELECT box_id, category, current_slot, max_slots,
                               is_full, total_pcb_count, last_updated
                         FROM box_status
                         ORDER BY box_id"""

                cursor.execute(sql)
                boxes = cursor.fetchall()

                # 이용률 계산
                for box in boxes:
                    box['utilization_rate'] = (box['current_slot'] / box['max_slots'] * 100)

                return boxes

        except Exception as e:
            logger.error(f"박스 상태 조회 실패: {str(e)}")
            return []
        finally:
            conn.close()

    def _trigger_auto_oht(self, box_id):
        """
        자동 OHT 호출 (박스 꽉 찬 경우)

        Args:
            box_id: 박스 ID ('NORMAL', 'COMPONENT_DEFECT', 'SOLDER_DEFECT')
        """
        try:
            payload = {
                'category': box_id,  # box_id가 이미 UPPERCASE 카테고리
                'trigger_reason': 'box_full'
            }
            response = requests.post(
                f"{self.oht_api_url}/api/oht/auto_trigger",
                json=payload,
                timeout=5
            )

            if response.status_code == 200:
                logger.info(f"✅ 자동 OHT 호출 성공: {box_id} (박스 꽉 참)")
            else:
                logger.error(f"❌ 자동 OHT 호출 실패: HTTP {response.status_code}")

        except requests.exceptions.RequestException as e:
            logger.error(f"❌ 자동 OHT 호출 오류: {str(e)}")

    def send_box_full_alert(self, box_id):
        """
        박스 꽉 참 알림 전송

        Args:
            box_id: 박스 ID
        """
        # 시스템 로그 기록
        self.db.log_system_event(
            log_level='WARNING',
            source='box_manager',
            message=f'박스 {box_id}가 꽉 찼습니다. OHT 호출됨',
            details={'box_id': box_id, 'timestamp': datetime.now().isoformat()}
        )

        # 실제 프로젝트에서는 LED 점멸, 알림음, WinForms 알림 등 추가
        logger.warning(f"📦 박스 {box_id} 꽉 참! OHT 자동 호출됨")

    def check_system_capacity(self):
        """
        시스템 전체 박스 용량 확인

        Returns:
            dict: 시스템 상태 정보
        """
        boxes = self.get_all_box_status()

        total_boxes = len(boxes)
        full_boxes = sum(1 for box in boxes if box['is_full'])
        empty_boxes = sum(1 for box in boxes if box['current_slot'] == 0)

        # 카테고리별 꽉 참 여부
        categories_full = {}
        for category in ['normal', 'component_defect', 'solder_defect', 'discard']:
            cat_boxes = [box for box in boxes if box['category'] == category]
            categories_full[category] = all(box['is_full'] for box in cat_boxes)

        # 전체 시스템 정지 여부
        system_stopped = all(categories_full.values())

        return {
            'total_boxes': total_boxes,
            'full_boxes': full_boxes,
            'empty_boxes': empty_boxes,
            'categories_full': categories_full,
            'system_stopped': system_stopped,
            'boxes': boxes
        }
```

### 5-3. Flask 서버 통합 (server/app.py 업데이트)

```python
# 기존 import에 추가
from robot_arm import RobotArmController
from box_manager import BoxManager

# 로봇팔 및 박스 관리자 초기화
robot_arm = RobotArmController(port='/dev/ttyACM0', baudrate=115200)
box_manager = BoxManager(db_service)

@app.route('/predict', methods=['POST'])
def predict():
    """PCB 불량 검사 추론 (로봇팔 제어 통합)"""
    try:
        # ... 기존 프레임 디코딩 및 추론 코드 ...

        # AI 추론 실행
        result = inference_engine.predict(frame, camera_id)

        # 불량 유형에 따라 박스 할당
        defect_type_map = {
            '정상': 'normal',
            '부품불량': 'component_defect',
            '납땜불량': 'solder_defect',
            '폐기': 'discard'
        }

        category = defect_type_map.get(result['defect_type'], 'normal')

        # 다음 사용 가능한 슬롯 조회
        box_id, slot_index = box_manager.get_next_available_slot(category)

        if box_id is None:
            # 박스가 모두 꽉 참 - 시스템 정지
            logger.error(f"카테고리 {category} 박스 꽉 참! 시스템 정지")
            return jsonify({
                'status': 'error',
                'error': 'BOX_FULL',
                'message': f'{result["defect_type"]} 박스가 모두 꽉 찼습니다. OHT 호출 필요',
                'defect_type': result['defect_type'],
                'category': category
            }), 503  # Service Unavailable

        # 로봇팔 PCB 배치 명령
        place_success = robot_arm.place_pcb(box_id, slot_index)

        if not place_success:
            logger.error("로봇팔 PCB 배치 실패")
            return jsonify({
                'status': 'error',
                'error': 'ROBOT_ARM_FAILURE',
                'message': '로봇팔 동작 실패'
            }), 500

        # 박스 상태 업데이트
        box_manager.update_box_status(box_id, slot_index)

        # 불량 이미지 저장 (기존 코드)
        image_path = None
        if result['defect_type'] != '정상':
            save_dir = 'results/defect_images'
            os.makedirs(save_dir, exist_ok=True)
            filename = f"{camera_id}_{result['defect_type']}_{datetime.now().strftime('%Y%m%d_%H%M%S')}.jpg"
            image_path = os.path.join(save_dir, filename)
            cv2.imwrite(image_path, frame)

        # GPIO 핀 매핑 (기존 코드)
        gpio_pin_map = {'부품불량': 17, '납땜불량': 27, '폐기': 22, '정상': 23}
        gpio_pin = gpio_pin_map.get(result['defect_type'], 23)
        gpio_duration_ms = 500

        # MySQL에 검사 결과 저장 (기존 코드)
        inspection_id = db_service.save_inspection_result(
            camera_id=camera_id,
            defect_type=result['defect_type'],
            confidence=result['confidence'],
            image_path=image_path,
            boxes=result['boxes'],
            gpio_pin=gpio_pin,
            gpio_duration_ms=gpio_duration_ms
        )

        # 응답 생성 (로봇팔 정보 추가)
        response = {
            'status': 'ok',
            'camera_id': camera_id,
            'timestamp': timestamp,
            'defect_type': result['defect_type'],
            'confidence': float(result['confidence']),
            'boxes': result['boxes'],
            'box_placement': {
                'box_id': box_id,
                'slot_index': slot_index,
                'category': category
            },
            'gpio_signal': {
                'pin': gpio_pin,
                'action': 'HIGH',
                'duration_ms': gpio_duration_ms
            },
            'inspection_id': inspection_id,
            'inference_time_ms': float(result['inference_time_ms'])
        }

        return jsonify(response)

    except Exception as e:
        logger.error(f"추론 오류: {str(e)}")
        return jsonify({'error': str(e)}), 500


@app.route('/box_status', methods=['GET'])
def get_box_status():
    """모든 박스 상태 조회"""
    try:
        capacity_info = box_manager.check_system_capacity()

        return jsonify({
            'status': 'ok',
            'boxes': capacity_info['boxes'],
            'summary': {
                'total_boxes': capacity_info['total_boxes'],
                'full_boxes': capacity_info['full_boxes'],
                'empty_boxes': capacity_info['empty_boxes'],
                'system_stopped': capacity_info['system_stopped']
            }
        })

    except Exception as e:
        logger.error(f"박스 상태 조회 실패: {str(e)}")
        return jsonify({'error': str(e)}), 500


@app.route('/box_status/<box_id>', methods=['GET'])
def get_box_status_by_id(box_id):
    """특정 박스 상태 조회"""
    try:
        conn = db_service.get_connection()
        with conn.cursor() as cursor:
            sql = """SELECT box_id, category, current_slot, max_slots,
                           is_full, total_pcb_count, last_updated
                     FROM box_status
                     WHERE box_id = %s"""

            cursor.execute(sql, (box_id,))
            box = cursor.fetchone()

        if not box:
            return jsonify({'error': 'Box not found'}), 404

        box['utilization_rate'] = (box['current_slot'] / box['max_slots'] * 100)

        return jsonify({
            'status': 'ok',
            'box': box
        })

    except Exception as e:
        logger.error(f"박스 상태 조회 실패: {str(e)}")
        return jsonify({'error': str(e)}), 500
    finally:
        conn.close()


@app.route('/box_status/reset', methods=['POST'])
def reset_box_status():
    """박스 리셋 (OHT 교체 후)"""
    try:
        data = request.get_json()
        box_id = data.get('box_id')

        if not box_id:
            return jsonify({'error': 'Missing box_id'}), 400

        success = box_manager.reset_box(box_id)

        if success:
            return jsonify({
                'status': 'ok',
                'message': f'Box {box_id} reset successfully'
            })
        else:
            return jsonify({'error': 'Failed to reset box'}), 500

    except Exception as e:
        logger.error(f"박스 리셋 실패: {str(e)}")
        return jsonify({'error': str(e)}), 500


@app.route('/robot_arm/status', methods=['GET'])
def get_robot_arm_status():
    """로봇팔 상태 조회"""
    try:
        status = robot_arm.get_status()

        if status:
            return jsonify({
                'status': 'ok',
                'robot_arm': status
            })
        else:
            return jsonify({
                'status': 'error',
                'message': 'Robot arm not responding'
            }), 503

    except Exception as e:
        logger.error(f"로봇팔 상태 조회 실패: {str(e)}")
        return jsonify({'error': str(e)}), 500


@app.route('/robot_arm/home', methods=['POST'])
def move_robot_arm_home():
    """로봇팔 홈 포지션으로 이동"""
    try:
        success = robot_arm.move_home()

        if success:
            return jsonify({
                'status': 'ok',
                'message': 'Robot arm moved to home position'
            })
        else:
            return jsonify({
                'status': 'error',
                'message': 'Failed to move robot arm'
            }), 500

    except Exception as e:
        logger.error(f"로봇팔 홈 이동 실패: {str(e)}")
        return jsonify({'error': str(e)}), 500
```

### 5-4. 박스 꽉 참 감지 및 시스템 정지 로직

시스템은 다음과 같이 동작합니다:

1. **정상 동작**: 각 박스의 슬롯 0 → 슬롯 2 순서로 채움 (수평 3슬롯 적재)
2. **박스 꽉 참 감지**:
   - 슬롯 0~2가 모두 채워지면 `is_full = TRUE`로 마킹
   - Flask 서버가 자동으로 OHT 요청(`/api/oht/auto_trigger`) 전송
3. **시스템 정지**:
   - 해당 카테고리 박스가 3/3가 되면 LED/WinForms에 경고 노출
   - OHT 자동 호출 상태에서 추가 배치는 차단
4. **박스 교체 후 재시작**:
   - 작업자가 박스 교체 완료
   - WinForms 앱에서 "박스 리셋" 버튼 클릭
   - `/box_status/reset` API 호출
   - 박스 상태 초기화 → 시스템 재가동
5. **DISCARD 처리**:
   - DISCARD는 슬롯 관리 안 함
   - 로봇팔이 고정 위치에서 PCB를 박스에 떨어뜨리기만 함
   - 박스 꽉 참 감지 없음 (프로젝트 데모용 단순화)

---

## Phase 6: 모니터링 및 로깅

### 6-1. 실시간 대시보드 (선택)

```python
# server/dashboard.py

from flask import Flask, render_template
from flask_socketio import SocketIO, emit

app = Flask(__name__)
socketio = SocketIO(app, cors_allowed_origins="*")

# 검사 통계
stats = {
    'total_count': 0,
    '정상': 0,
    '부품불량': 0,
    '납땜불량': 0,
    '폐기': 0
}

@app.route('/')
def dashboard():
    return render_template('dashboard.html')

@socketio.on('connect')
def handle_connect():
    emit('stats_update', stats)

def update_stats(defect_type):
    """통계 업데이트"""
    stats['total_count'] += 1
    stats[defect_type] = stats.get(defect_type, 0) + 1
    socketio.emit('stats_update', stats)

if __name__ == '__main__':
    socketio.run(app, host='0.0.0.0', port=5001)
```

### 6-2. 로그 기록

```python
# server/app.py에 로깅 추가

import logging
from logging.handlers import RotatingFileHandler

# 파일 로그 핸들러
handler = RotatingFileHandler('logs/inference.log', maxBytes=10000000, backupCount=5)
handler.setFormatter(logging.Formatter(
    '%(asctime)s - %(name)s - %(levelname)s - %(message)s'
))
logger.addHandler(handler)
```

---

## Phase 6: 문제 해결

### 문제 1: 네트워크 지연이 심함

**해결책**:
1. 이미지 압축률 조정 (JPEG quality 낮추기)
```python
_, buffer = cv2.imencode('.jpg', frame, [cv2.IMWRITE_JPEG_QUALITY, 70])  # 85 → 70
```

2. 이미지 해상도 낮추기
```python
self.cap.set(cv2.CAP_PROP_FRAME_WIDTH, 416)  # 640 → 416
self.cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 416)
```

3. FPS 낮추기
```python
FPS = 5  # 10 → 5
```

### 문제 2: GPU 메모리 부족

**RTX 4080 Super (16GB)에서는 거의 발생하지 않음** ✅

혹시 메모리 부족 발생 시 해결책:
```python
# 1. FP16 사용 (VRAM 50% 절약) ⭐ 최우선
self.yolo_model.half()

# 2. 배치 크기 줄이기
batch_size = 2  # 4 → 2

# 3. 이미지 크기 줄이기 (최후의 수단)
image_size = 416  # 640 → 416

# 4. 메모리 정리
torch.cuda.empty_cache()

# 5. VRAM 사용량 모니터링
print(f"VRAM 사용: {torch.cuda.memory_allocated() / 1024**3:.2f}GB / 16GB")
```

**예상 VRAM 사용량 (RTX 4080 Super)**:
- Component Model (YOLOv11l): ~5-6GB
- Solder Model (YOLOv11l): ~4-5GB
- **총 VRAM 사용**: ~8GB (양면 동시 추론)
- 여유 메모리: 8GB
- 결론: 메모리 부족 가능성 매우 낮음 ✅

### 문제 3: 웹캠이 인식되지 않음

**해결책**:
```bash
# 사용 가능한 웹캠 확인
v4l2-ctl --list-devices

# 또는 Python으로 확인
python -c "import cv2; print([cv2.VideoCapture(i).isOpened() for i in range(5)])"
```

---

## 추가 기능

### 1. 결과 이미지 저장

```python
# server/app.py 수정

import os
from datetime import datetime

@app.route('/predict', methods=['POST'])
def predict():
    # ... 기존 코드 ...

    # 불량 이미지 저장
    if result['defect_type'] != '정상':
        save_dir = 'results/defect_images'
        os.makedirs(save_dir, exist_ok=True)

        filename = f"{camera_id}_{result['defect_type']}_{datetime.now().strftime('%Y%m%d_%H%M%S')}.jpg"
        cv2.imwrite(os.path.join(save_dir, filename), frame)

    return jsonify(response)
```

### 2. MySQL 데이터베이스 연동 ⭐

**설치**:
```bash
pip install pymysql
# 또는
pip install SQLAlchemy pymysql
```

**server/database.py** (MySQL 버전)

```python
import pymysql
from datetime import datetime
import json
import logging

logger = logging.getLogger(__name__)

class DatabaseService:
    def __init__(self, host, database, user, password):
        self.host = host
        self.database = database
        self.user = user
        self.password = password

    def get_connection(self):
        """MySQL 연결 생성"""
        return pymysql.connect(
            host=self.host,
            user=self.user,
            password=self.password,
            database=self.database,
            charset='utf8mb4',
            cursorclass=pymysql.cursors.DictCursor
        )

    def save_inspection_result(self, camera_id, defect_type, confidence,
                               image_path, boxes, gpio_pin, gpio_duration_ms):
        """검사 결과 저장"""
        try:
            conn = self.get_connection()
            with conn.cursor() as cursor:
                sql = """INSERT INTO inspections
                         (camera_id, defect_type, confidence, image_path,
                          boxes, gpio_pin, gpio_duration_ms)
                         VALUES (%s, %s, %s, %s, %s, %s, %s)"""

                cursor.execute(sql, (
                    camera_id,
                    defect_type,
                    confidence,
                    image_path,
                    json.dumps(boxes) if boxes else None,
                    gpio_pin,
                    gpio_duration_ms
                ))
                inspection_id = cursor.lastrowid
            conn.commit()
            return inspection_id

        except Exception as e:
            logger.error(f"DB 저장 실패: {str(e)}")
            return None
        finally:
            conn.close()

    def get_system_config(self, config_key):
        """시스템 설정 조회"""
        try:
            conn = self.get_connection()
            with conn.cursor() as cursor:
                sql = "SELECT config_value FROM system_config WHERE config_key = %s"
                cursor.execute(sql, (config_key,))
                result = cursor.fetchone()
                return result['config_value'] if result else None
        except Exception as e:
            logger.error(f"설정 조회 실패: {str(e)}")
            return None
        finally:
            conn.close()

    def log_system_event(self, log_level, source, message, details=None):
        """시스템 로그 기록"""
        try:
            conn = self.get_connection()
            with conn.cursor() as cursor:
                sql = """INSERT INTO system_logs
                         (log_level, source, message, details)
                         VALUES (%s, %s, %s, %s)"""
                cursor.execute(sql, (
                    log_level,
                    source,
                    message,
                    json.dumps(details) if details else None
                ))
            conn.commit()
        except Exception as e:
            logger.error(f"로그 기록 실패: {str(e)}")
        finally:
            conn.close()
```

**server/app.py 업데이트 (MySQL 통합)**

```python
from database import DatabaseService

# MySQL 연결
db_service = DatabaseService(
    host='localhost',
    database='pcb_inspection',
    user='root',
    password='your_password'
)

@app.route('/predict', methods=['POST'])
def predict():
    try:
        # ... 기존 프레임 디코딩 코드 ...

        # AI 추론 실행
        result = inference_engine.predict(frame, camera_id)

        # 불량 이미지 저장
        image_path = None
        if result['defect_type'] != '정상':
            save_dir = 'results/defect_images'
            os.makedirs(save_dir, exist_ok=True)
            filename = f"{camera_id}_{result['defect_type']}_{datetime.now().strftime('%Y%m%d_%H%M%S')}.jpg"
            image_path = os.path.join(save_dir, filename)
            cv2.imwrite(image_path, frame)

        # GPIO 핀 매핑
        gpio_pin_map = {'부품불량': 17, '납땜불량': 27, '폐기': 22, '정상': 23}
        gpio_pin = gpio_pin_map.get(result['defect_type'], 23)
        gpio_duration_ms = 500

        # MySQL에 검사 결과 저장
        inspection_id = db_service.save_inspection_result(
            camera_id=camera_id,
            defect_type=result['defect_type'],
            confidence=result['confidence'],
            image_path=image_path,
            boxes=result['boxes'],
            gpio_pin=gpio_pin,
            gpio_duration_ms=gpio_duration_ms
        )

        # 응답 생성 (GPIO 신호 포함)
        response = {
            'status': 'ok',
            'camera_id': camera_id,
            'timestamp': timestamp,
            'defect_type': result['defect_type'],
            'confidence': float(result['confidence']),
            'boxes': result['boxes'],
            'gpio_signal': {
                'pin': gpio_pin,
                'action': 'HIGH',
                'duration_ms': gpio_duration_ms
            },
            'inspection_id': inspection_id,
            'inference_time_ms': float(result['inference_time_ms'])
        }

        return jsonify(response)

    except Exception as e:
        logger.error(f"추론 오류: {str(e)}")
        db_service.log_system_event('ERROR', 'server', f"추론 오류: {str(e)}")
        return jsonify({'error': str(e)}), 500
```
```

---

## Phase 5-5: OHT 시스템 API 통합 ⭐

### 5-5-1. OHT API 엔드포인트 추가

**server/oht_api.py** (신규 파일)

```python
from flask import Blueprint, request, jsonify
from datetime import datetime
import uuid
import logging

oht_bp = Blueprint('oht', __name__, url_prefix='/api/oht')
logger = logging.getLogger(__name__)

# OHT 요청 큐 (실제 구현 시 Redis 또는 RabbitMQ 사용 권장)
oht_request_queue = []
oht_request_status = {}  # {request_id: {'status': 'pending'|'processing'|'completed', ...}}


@oht_bp.route('/request', methods=['POST'])
def request_oht():
    """
    OHT 호출 요청 (수동)

    요청:
        {
            "category": "NORMAL" | "COMPONENT_DEFECT" | "SOLDER_DEFECT",
            "user_id": "user_uuid",
            "user_role": "Admin" | "Operator"
        }

    응답:
        {
            "status": "ok",
            "request_id": "uuid",
            "message": "OHT request queued"
        }
    """
    try:
        data = request.get_json()
        category = data.get('category')
        user_id = data.get('user_id')
        user_role = data.get('user_role')

        # 권한 검증 ⭐
        if user_role not in ['Admin', 'Operator']:
            return jsonify({
                'error': 'Insufficient permissions',
                'message': 'Only Admin and Operator can call OHT'
            }), 403

        # 카테고리 검증
        if category not in ['NORMAL', 'COMPONENT_DEFECT', 'SOLDER_DEFECT']:
            return jsonify({'error': 'Invalid category'}), 400

        # 요청 생성
        request_id = str(uuid.uuid4())
        oht_request = {
            'request_id': request_id,
            'category': category,
            'user_id': user_id,
            'user_role': user_role,
            'is_auto': False,
            'timestamp': datetime.now().isoformat()
        }

        # 큐에 추가
        oht_request_queue.append(oht_request)
        oht_request_status[request_id] = {
            'status': 'pending',
            'created_at': datetime.now().isoformat()
        }

        logger.info(f"OHT request {request_id} queued by {user_role} (category: {category})")

        # MySQL에 기록 (실제 구현)
        # db.insert_oht_request(...)

        return jsonify({
            'status': 'ok',
            'request_id': request_id,
            'message': 'OHT request queued'
        }), 200

    except Exception as e:
        logger.error(f"OHT request failed: {e}")
        return jsonify({'error': str(e)}), 500


@oht_bp.route('/check_pending', methods=['GET'])
def check_pending_requests():
    """
    대기 중인 OHT 요청 확인 (라즈베리파이 3번 폴링용)

    응답:
        {
            "has_pending": true,
            "request": {...}
        }
    """
    if oht_request_queue:
        request_data = oht_request_queue[0]  # FIFO
        return jsonify({
            'has_pending': True,
            'request': request_data
        }), 200
    else:
        return jsonify({
            'has_pending': False
        }), 200


@oht_bp.route('/complete', methods=['POST'])
def complete_request():
    """
    OHT 요청 완료 보고 (라즈베리파이 3번에서 호출)

    요청:
        {
            "request_id": "uuid",
            "success": true,
            "error": null
        }
    """
    try:
        data = request.get_json()
        request_id = data.get('request_id')
        success = data.get('success')
        error = data.get('error')

        # 큐에서 제거
        if oht_request_queue and oht_request_queue[0]['request_id'] == request_id:
            oht_request_queue.pop(0)

        # 상태 업데이트
        if request_id in oht_request_status:
            oht_request_status[request_id]['status'] = 'completed' if success else 'failed'
            oht_request_status[request_id]['completed_at'] = datetime.now().isoformat()
            oht_request_status[request_id]['error'] = error

        logger.info(f"OHT request {request_id} completed (success: {success})")

        # MySQL 업데이트 (실제 구현)
        # db.update_oht_request(request_id, success, error)

        return jsonify({'status': 'ok'}), 200

    except Exception as e:
        logger.error(f"Failed to complete OHT request: {e}")
        return jsonify({'error': str(e)}), 500


@oht_bp.route('/status', methods=['GET'])
def get_oht_status():
    """
    OHT 시스템 상태 조회 (WinForms UI용)

    응답:
        {
            "queue_length": 2,
            "current_request": {...},
            "recent_requests": [...]
        }
    """
    current_request = oht_request_queue[0] if oht_request_queue else None

    return jsonify({
        'queue_length': len(oht_request_queue),
        'current_request': current_request,
        'recent_requests': list(oht_request_status.values())[-10:]  # 최근 10개
    }), 200


@oht_bp.route('/auto_trigger', methods=['POST'])
def auto_trigger():
    """
    자동 OHT 호출 (박스 꽉 찬 경우)

    요청:
        {
            "category": "NORMAL" | "COMPONENT_DEFECT" | "SOLDER_DEFECT",
            "trigger_reason": "box_full"
        }
    """
    try:
        data = request.get_json()
        category = data.get('category')

        # 요청 생성 (자동)
        request_id = str(uuid.uuid4())
        oht_request = {
            'request_id': request_id,
            'category': category,
            'user_id': 'system',
            'user_role': 'System',
            'is_auto': True,
            'trigger_reason': 'box_full',
            'timestamp': datetime.now().isoformat()
        }

        oht_request_queue.append(oht_request)
        oht_request_status[request_id] = {
            'status': 'pending',
            'created_at': datetime.now().isoformat()
        }

        logger.info(f"Auto OHT request {request_id} triggered for {category} (box full)")

        return jsonify({
            'status': 'ok',
            'request_id': request_id
        }), 200

    except Exception as e:
        logger.error(f"Auto OHT trigger failed: {e}")
        return jsonify({'error': str(e)}), 500
```

### 5-5-2. Flask 서버에 OHT API 등록

**server/app.py** (기존 파일 수정)

```python
# 기존 import에 추가
from oht_api import oht_bp

# Flask 앱 생성
app = Flask(__name__)
CORS(app)

# OHT API 블루프린트 등록 ⭐
app.register_blueprint(oht_bp)

# ... (기존 코드)
```

---

## Phase 5-6: 사용자 관리 API ⭐ 신규

### 5-6-1. 사용자 관리 API 엔드포인트 추가

**server/user_api.py** (신규 파일)

```python
from flask import Blueprint, request, jsonify
from datetime import datetime
import bcrypt
import logging
from functools import wraps

user_bp = Blueprint('users', __name__, url_prefix='/api/users')
auth_bp = Blueprint('auth', __name__, url_prefix='/api/auth')
logger = logging.getLogger(__name__)


# 권한 검증 데코레이터
def admin_required(f):
    """Admin 권한 체크 데코레이터"""
    @wraps(f)
    def decorated_function(*args, **kwargs):
        try:
            # 요청에서 사용자 정보 가져오기 (실제로는 세션/토큰 검증)
            user_role = request.headers.get('X-User-Role')

            if user_role != 'Admin':
                return jsonify({
                    'error': 'Insufficient permissions',
                    'message': 'Admin permission required'
                }), 403

            return f(*args, **kwargs)
        except Exception as e:
            logger.error(f"Permission check failed: {e}")
            return jsonify({'error': 'Authorization failed'}), 401

    return decorated_function


@user_bp.route('', methods=['GET'])
@admin_required
def get_users():
    """
    사용자 목록 조회 (Admin만)

    쿼리 파라미터:
        - role: 권한 필터 (admin/operator/viewer)
        - is_active: 활성화 상태 필터 (true/false)
        - search: 사용자명 또는 이름 검색

    응답:
        {
            "status": "ok",
            "users": [
                {
                    "id": 1,
                    "username": "admin",
                    "full_name": "관리자",
                    "role": "Admin",
                    "is_active": true,
                    "last_login": "2025-10-22T14:30:00",
                    "created_at": "2025-10-01T09:00:00"
                },
                ...
            ],
            "total": 15
        }
    """
    try:
        # 필터 파라미터
        role_filter = request.args.get('role')
        is_active = request.args.get('is_active')
        search_query = request.args.get('search')

        # MySQL 연결 (db_service는 app.py에서 초기화된 것 사용)
        from server.app import db_service
        conn = db_service.get_connection()

        with conn.cursor() as cursor:
            # 기본 쿼리
            query = "SELECT id, username, full_name, role, is_active, last_login, created_at FROM users WHERE 1=1"
            params = []

            # 필터 적용
            if role_filter:
                query += " AND role = %s"
                params.append(role_filter)

            if is_active is not None:
                query += " AND is_active = %s"
                params.append(is_active.lower() == 'true')

            if search_query:
                query += " AND (username LIKE %s OR full_name LIKE %s)"
                search_pattern = f"%{search_query}%"
                params.extend([search_pattern, search_pattern])

            query += " ORDER BY created_at DESC"

            cursor.execute(query, params)
            users = cursor.fetchall()

            # 비밀번호 해시 제거
            for user in users:
                user.pop('password_hash', None)

            return jsonify({
                'status': 'ok',
                'users': users,
                'total': len(users)
            }), 200

    except Exception as e:
        logger.error(f"Failed to get users: {e}")
        return jsonify({'error': str(e)}), 500


@user_bp.route('/<int:user_id>', methods=['GET'])
def get_user(user_id):
    """
    특정 사용자 조회

    응답:
        {
            "status": "ok",
            "user": {
                "id": 1,
                "username": "operator1",
                "full_name": "작업자1",
                "role": "Operator",
                "is_active": true,
                "last_login": "2025-10-22T14:30:00",
                "created_at": "2025-10-01T09:00:00"
            }
        }
    """
    try:
        from server.app import db_service
        conn = db_service.get_connection()

        with conn.cursor() as cursor:
            cursor.execute("""
                SELECT id, username, full_name, role, is_active, last_login, created_at
                FROM users
                WHERE id = %s
            """, (user_id,))

            user = cursor.fetchone()

            if not user:
                return jsonify({'error': 'User not found'}), 404

            return jsonify({
                'status': 'ok',
                'user': user
            }), 200

    except Exception as e:
        logger.error(f"Failed to get user {user_id}: {e}")
        return jsonify({'error': str(e)}), 500


@user_bp.route('', methods=['POST'])
@admin_required
def create_user():
    """
    새 사용자 생성 (Admin만)

    요청:
        {
            "username": "operator2",
            "password": "password123",
            "full_name": "작업자2",
            "role": "Operator",
            "is_active": true
        }

    응답:
        {
            "status": "ok",
            "user_id": 5,
            "message": "User created successfully"
        }
    """
    try:
        data = request.get_json()
        username = data.get('username')
        password = data.get('password')
        full_name = data.get('full_name')
        role = data.get('role', 'Viewer')
        is_active = data.get('is_active', True)

        # 유효성 검사
        if not username or not password:
            return jsonify({'error': 'Username and password are required'}), 400

        if role not in ['Admin', 'Operator', 'Viewer']:
            return jsonify({'error': 'Invalid role'}), 400

        # 비밀번호 해싱
        password_hash = bcrypt.hashpw(password.encode('utf-8'), bcrypt.gensalt()).decode('utf-8')

        from server.app import db_service
        conn = db_service.get_connection()

        with conn.cursor() as cursor:
            # 사용자명 중복 체크
            cursor.execute("SELECT id FROM users WHERE username = %s", (username,))
            if cursor.fetchone():
                return jsonify({'error': 'Username already exists'}), 409

            # 사용자 생성
            cursor.execute("""
                INSERT INTO users (username, password_hash, full_name, role, is_active)
                VALUES (%s, %s, %s, %s, %s)
            """, (username, password_hash, full_name, role, is_active))

            conn.commit()
            user_id = cursor.lastrowid

            logger.info(f"User created: {username} (ID: {user_id}, Role: {role})")

            # 활동 로그 기록
            log_user_action(
                user_id=request.headers.get('X-User-ID'),  # Admin ID
                action_type='create_user',
                action_description=f"사용자 '{username}' 생성 (권한: {role})",
                details={'new_username': username, 'new_role': role}
            )

            return jsonify({
                'status': 'ok',
                'user_id': user_id,
                'message': 'User created successfully'
            }), 201

    except Exception as e:
        logger.error(f"Failed to create user: {e}")
        return jsonify({'error': str(e)}), 500


@user_bp.route('/<int:user_id>', methods=['PUT'])
@admin_required
def update_user(user_id):
    """
    사용자 정보 수정 (Admin만)

    요청:
        {
            "full_name": "작업자1 수정",
            "role": "Admin",
            "is_active": false
        }

    응답:
        {
            "status": "ok",
            "message": "User updated successfully"
        }
    """
    try:
        data = request.get_json()
        full_name = data.get('full_name')
        role = data.get('role')
        is_active = data.get('is_active')

        # 유효성 검사
        if role and role not in ['Admin', 'Operator', 'Viewer']:
            return jsonify({'error': 'Invalid role'}), 400

        from server.app import db_service
        conn = db_service.get_connection()

        with conn.cursor() as cursor:
            # 사용자 존재 확인
            cursor.execute("SELECT username FROM users WHERE id = %s", (user_id,))
            user = cursor.fetchone()
            if not user:
                return jsonify({'error': 'User not found'}), 404

            # 업데이트 쿼리 동적 생성
            updates = []
            params = []

            if full_name is not None:
                updates.append("full_name = %s")
                params.append(full_name)

            if role is not None:
                updates.append("role = %s")
                params.append(role)

            if is_active is not None:
                updates.append("is_active = %s")
                params.append(is_active)

            if not updates:
                return jsonify({'error': 'No fields to update'}), 400

            params.append(user_id)
            query = f"UPDATE users SET {', '.join(updates)} WHERE id = %s"

            cursor.execute(query, params)
            conn.commit()

            logger.info(f"User updated: {user['username']} (ID: {user_id})")

            # 활동 로그 기록
            log_user_action(
                user_id=request.headers.get('X-User-ID'),
                action_type='update_user',
                action_description=f"사용자 '{user['username']}' 정보 수정",
                details={'target_user_id': user_id, 'updates': data}
            )

            return jsonify({
                'status': 'ok',
                'message': 'User updated successfully'
            }), 200

    except Exception as e:
        logger.error(f"Failed to update user {user_id}: {e}")
        return jsonify({'error': str(e)}), 500


@user_bp.route('/<int:user_id>', methods=['DELETE'])
@admin_required
def delete_user(user_id):
    """
    사용자 삭제 (Admin만)

    응답:
        {
            "status": "ok",
            "message": "User deleted successfully"
        }
    """
    try:
        from server.app import db_service
        conn = db_service.get_connection()

        with conn.cursor() as cursor:
            # 사용자 존재 확인
            cursor.execute("SELECT username FROM users WHERE id = %s", (user_id,))
            user = cursor.fetchone()
            if not user:
                return jsonify({'error': 'User not found'}), 404

            # 자기 자신 삭제 방지
            current_user_id = int(request.headers.get('X-User-ID', 0))
            if user_id == current_user_id:
                return jsonify({'error': 'Cannot delete yourself'}), 400

            # 사용자 삭제
            cursor.execute("DELETE FROM users WHERE id = %s", (user_id,))
            conn.commit()

            logger.info(f"User deleted: {user['username']} (ID: {user_id})")

            # 활동 로그 기록
            log_user_action(
                user_id=current_user_id,
                action_type='delete_user',
                action_description=f"사용자 '{user['username']}' 삭제",
                details={'deleted_user_id': user_id, 'deleted_username': user['username']}
            )

            return jsonify({
                'status': 'ok',
                'message': 'User deleted successfully'
            }), 200

    except Exception as e:
        logger.error(f"Failed to delete user {user_id}: {e}")
        return jsonify({'error': str(e)}), 500


@user_bp.route('/<int:user_id>/reset-password', methods=['POST'])
@admin_required
def reset_password(user_id):
    """
    비밀번호 초기화 (Admin만)
    기본 비밀번호: 'temp1234'

    응답:
        {
            "status": "ok",
            "message": "Password reset successfully",
            "new_password": "temp1234"
        }
    """
    try:
        DEFAULT_PASSWORD = 'temp1234'

        # 비밀번호 해싱
        password_hash = bcrypt.hashpw(DEFAULT_PASSWORD.encode('utf-8'), bcrypt.gensalt()).decode('utf-8')

        from server.app import db_service
        conn = db_service.get_connection()

        with conn.cursor() as cursor:
            # 사용자 존재 확인
            cursor.execute("SELECT username FROM users WHERE id = %s", (user_id,))
            user = cursor.fetchone()
            if not user:
                return jsonify({'error': 'User not found'}), 404

            # 비밀번호 업데이트
            cursor.execute("""
                UPDATE users
                SET password_hash = %s
                WHERE id = %s
            """, (password_hash, user_id))

            conn.commit()

            logger.info(f"Password reset for user: {user['username']} (ID: {user_id})")

            # 활동 로그 기록
            log_user_action(
                user_id=request.headers.get('X-User-ID'),
                action_type='reset_password',
                action_description=f"사용자 '{user['username']}' 비밀번호 초기화",
                details={'target_user_id': user_id, 'target_username': user['username'], 'reset_to': DEFAULT_PASSWORD}
            )

            return jsonify({
                'status': 'ok',
                'message': 'Password reset successfully',
                'new_password': DEFAULT_PASSWORD
            }), 200

    except Exception as e:
        logger.error(f"Failed to reset password for user {user_id}: {e}")
        return jsonify({'error': str(e)}), 500


@user_bp.route('/<int:user_id>/logs', methods=['GET'])
def get_user_logs(user_id):
    """
    사용자 활동 로그 조회

    쿼리 파라미터:
        - start_date: 시작 날짜 (YYYY-MM-DD)
        - end_date: 종료 날짜 (YYYY-MM-DD)
        - action_type: 활동 유형 필터
        - limit: 최대 개수 (기본: 50)

    응답:
        {
            "status": "ok",
            "logs": [
                {
                    "id": 123,
                    "action_type": "login",
                    "action_description": "로그인",
                    "ip_address": "100.64.1.10",
                    "created_at": "2025-10-22T14:30:00"
                },
                ...
            ],
            "total": 25
        }
    """
    try:
        # 쿼리 파라미터
        start_date = request.args.get('start_date')
        end_date = request.args.get('end_date')
        action_type = request.args.get('action_type')
        limit = int(request.args.get('limit', 50))

        from server.app import db_service
        conn = db_service.get_connection()

        with conn.cursor() as cursor:
            query = """
                SELECT id, action_type, action_description, ip_address, details, created_at
                FROM user_logs
                WHERE user_id = %s
            """
            params = [user_id]

            # 날짜 필터
            if start_date:
                query += " AND DATE(created_at) >= %s"
                params.append(start_date)

            if end_date:
                query += " AND DATE(created_at) <= %s"
                params.append(end_date)

            # 활동 유형 필터
            if action_type:
                query += " AND action_type = %s"
                params.append(action_type)

            query += " ORDER BY created_at DESC LIMIT %s"
            params.append(limit)

            cursor.execute(query, params)
            logs = cursor.fetchall()

            return jsonify({
                'status': 'ok',
                'logs': logs,
                'total': len(logs)
            }), 200

    except Exception as e:
        logger.error(f"Failed to get logs for user {user_id}: {e}")
        return jsonify({'error': str(e)}), 500


# 인증 API

@auth_bp.route('/login', methods=['POST'])
def login():
    """
    로그인

    요청:
        {
            "username": "admin",
            "password": "admin123"
        }

    응답:
        {
            "status": "ok",
            "user": {
                "id": 1,
                "username": "admin",
                "full_name": "관리자",
                "role": "Admin"
            },
            "token": "jwt_token_here"  // 실제로는 JWT 토큰 구현
        }
    """
    try:
        data = request.get_json()
        username = data.get('username')
        password = data.get('password')

        if not username or not password:
            return jsonify({'error': 'Username and password are required'}), 400

        from server.app import db_service
        conn = db_service.get_connection()

        with conn.cursor() as cursor:
            # 사용자 조회
            cursor.execute("""
                SELECT id, username, password_hash, full_name, role, is_active
                FROM users
                WHERE username = %s
            """, (username,))

            user = cursor.fetchone()

            if not user:
                return jsonify({'error': 'Invalid username or password'}), 401

            if not user['is_active']:
                return jsonify({'error': 'User account is disabled'}), 403

            # 비밀번호 검증
            if not bcrypt.checkpw(password.encode('utf-8'), user['password_hash'].encode('utf-8')):
                return jsonify({'error': 'Invalid username or password'}), 401

            # 마지막 로그인 시간 업데이트
            cursor.execute("""
                UPDATE users
                SET last_login = %s
                WHERE id = %s
            """, (datetime.now(), user['id']))
            conn.commit()

            logger.info(f"User logged in: {username} (ID: {user['id']})")

            # 로그인 로그 기록
            log_user_action(
                user_id=user['id'],
                action_type='login',
                action_description="로그인",
                details={'ip_address': request.remote_addr}
            )

            # 비밀번호 해시 제거
            user.pop('password_hash')

            return jsonify({
                'status': 'ok',
                'user': user,
                'token': f"fake_jwt_token_{user['id']}"  # 실제로는 JWT 구현
            }), 200

    except Exception as e:
        logger.error(f"Login failed: {e}")
        return jsonify({'error': str(e)}), 500


@auth_bp.route('/logout', methods=['POST'])
def logout():
    """
    로그아웃

    요청:
        {
            "user_id": 1
        }

    응답:
        {
            "status": "ok",
            "message": "Logged out successfully"
        }
    """
    try:
        data = request.get_json()
        user_id = data.get('user_id')

        if not user_id:
            return jsonify({'error': 'User ID is required'}), 400

        logger.info(f"User logged out: ID {user_id}")

        # 로그아웃 로그 기록
        log_user_action(
            user_id=user_id,
            action_type='logout',
            action_description="로그아웃",
            details={}
        )

        return jsonify({
            'status': 'ok',
            'message': 'Logged out successfully'
        }), 200

    except Exception as e:
        logger.error(f"Logout failed: {e}")
        return jsonify({'error': str(e)}), 500


# 활동 로그 기록 헬퍼 함수
def log_user_action(user_id, action_type, action_description, details=None):
    """사용자 활동 로그 기록"""
    try:
        from server.app import db_service
        conn = db_service.get_connection()

        with conn.cursor() as cursor:
            # 사용자 정보 조회
            cursor.execute("SELECT username, role FROM users WHERE id = %s", (user_id,))
            user = cursor.fetchone()

            if not user:
                logger.warning(f"User not found for log: {user_id}")
                return

            # 로그 삽입
            cursor.execute("""
                INSERT INTO user_logs
                (user_id, username, user_role, action_type, action_description, ip_address, details)
                VALUES (%s, %s, %s, %s, %s, %s, %s)
            """, (
                user_id,
                user['username'],
                user['role'],
                action_type,
                action_description,
                request.remote_addr if request else None,
                details if details else None
            ))

            conn.commit()
            logger.debug(f"User action logged: {user['username']} - {action_type}")

    except Exception as e:
        logger.error(f"Failed to log user action: {e}")
```

### 5-6-2. Flask 앱에 Blueprint 등록

**server/app.py 수정**

```python
from server.user_api import user_bp, auth_bp

# ... (기존 코드)

# 사용자 관리 API 블루프린트 등록 ⭐
app.register_blueprint(user_bp)
app.register_blueprint(auth_bp)

# ... (기존 코드)
```

### 5-6-3. API 엔드포인트 목록

| 메서드 | 엔드포인트 | 설명 | 권한 |
|--------|-----------|------|------|
| **사용자 관리** ||||
| GET | `/api/users` | 사용자 목록 조회 | Admin |
| GET | `/api/users/{id}` | 특정 사용자 조회 | All |
| POST | `/api/users` | 사용자 생성 | Admin |
| PUT | `/api/users/{id}` | 사용자 수정 | Admin |
| DELETE | `/api/users/{id}` | 사용자 삭제 | Admin |
| POST | `/api/users/{id}/reset-password` | 비밀번호 초기화 | Admin |
| GET | `/api/users/{id}/logs` | 활동 로그 조회 | All |
| **인증** ||||
| POST | `/api/auth/login` | 로그인 | Public |
| POST | `/api/auth/logout` | 로그아웃 | All |

### 5-6-4. 보안 고려사항

**비밀번호 해싱**:
```bash
# bcrypt 설치
pip install bcrypt
```

**권한 체크**:
- `@admin_required` 데코레이터로 Admin 전용 엔드포인트 보호
- HTTP 헤더 `X-User-Role`로 권한 검증 (실제로는 JWT 토큰 사용 권장)

**활동 로그**:
- 모든 중요한 작업은 `user_logs` 테이블에 기록
- IP 주소, 변경 내역(details) 포함

---

## C# WinForms용 REST API 엔드포인트

### 1. 검사 이력 조회 API

**server/app.py에 추가**

```python
@app.route('/api/inspections', methods=['GET'])
def get_inspections():
    """검사 이력 조회 (페이징)"""
    try:
        page = int(request.args.get('page', 1))
        limit = int(request.args.get('limit', 50))
        offset = (page - 1) * limit

        conn = db_service.get_connection()
        with conn.cursor() as cursor:
            # 총 개수
            cursor.execute("SELECT COUNT(*) as total FROM inspections")
            total = cursor.fetchone()['total']

            # 페이징 조회
            sql = """SELECT id, camera_id, defect_type, confidence,
                           inspection_time, image_path
                    FROM inspections
                    ORDER BY inspection_time DESC
                    LIMIT %s OFFSET %s"""
            cursor.execute(sql, (limit, offset))
            inspections = cursor.fetchall()

        return jsonify({
            'status': 'ok',
            'data': inspections,
            'pagination': {
                'page': page,
                'limit': limit,
                'total': total,
                'total_pages': (total + limit - 1) // limit
            }
        })

    except Exception as e:
        return jsonify({'error': str(e)}), 500
    finally:
        conn.close()


@app.route('/api/statistics', methods=['GET'])
def get_statistics():
    """통계 조회"""
    try:
        start_date = request.args.get('start_date')
        end_date = request.args.get('end_date')

        conn = db_service.get_connection()
        with conn.cursor() as cursor:
            sql = """SELECT
                        COUNT(*) as total_inspections,
                        SUM(CASE WHEN defect_type = '정상' THEN 1 ELSE 0 END) as normal_count,
                        SUM(CASE WHEN defect_type = '부품불량' THEN 1 ELSE 0 END) as component_defect_count,
                        SUM(CASE WHEN defect_type = '납땜불량' THEN 1 ELSE 0 END) as solder_defect_count,
                        SUM(CASE WHEN defect_type = '폐기' THEN 1 ELSE 0 END) as discard_count
                    FROM inspections
                    WHERE DATE(inspection_time) BETWEEN %s AND %s"""

            cursor.execute(sql, (start_date, end_date))
            stats = cursor.fetchone()

            # 불량률 계산
            total = stats['total_inspections']
            defect_total = stats['component_defect_count'] + stats['solder_defect_count'] + stats['discard_count']
            stats['defect_rate'] = (defect_total / total * 100) if total > 0 else 0

        return jsonify({
            'status': 'ok',
            'data': stats
        })

    except Exception as e:
        return jsonify({'error': str(e)}), 500
    finally:
        conn.close()


@app.route('/api/defect-images/<int:inspection_id>', methods=['GET'])
def get_defect_image(inspection_id):
    """불량 이미지 다운로드"""
    try:
        conn = db_service.get_connection()
        with conn.cursor() as cursor:
            sql = "SELECT image_path FROM inspections WHERE id = %s"
            cursor.execute(sql, (inspection_id,))
            result = cursor.fetchone()

        if not result or not result['image_path']:
            return jsonify({'error': 'Image not found'}), 404

        image_path = result['image_path']
        if not os.path.exists(image_path):
            return jsonify({'error': 'Image file not found'}), 404

        return send_file(image_path, mimetype='image/jpeg')

    except Exception as e:
        return jsonify({'error': str(e)}), 500
    finally:
        conn.close()


@app.route('/api/system-status', methods=['GET'])
def get_system_status():
    """시스템 상태 조회"""
    import psutil

    try:
        status = {
            'server_online': True,
            'database_online': db_service.get_connection() is not None,
            'raspberry_pi_1_online': check_raspberry_pi('100.64.1.2'),  # 좌측 카메라
            'raspberry_pi_2_online': check_raspberry_pi('100.64.1.3'),  # 우측 카메라
            'server_cpu_usage': psutil.cpu_percent(),
            'server_memory_usage': psutil.virtual_memory().percent,
            'server_gpu_usage': get_gpu_usage() if torch.cuda.is_available() else 0,
            'last_update': datetime.now().isoformat()
        }

        return jsonify(status)

    except Exception as e:
        return jsonify({'error': str(e)}), 500


def check_raspberry_pi(ip_address):
    """라즈베리파이 온라인 여부 확인 (ping)"""
    import subprocess
    try:
        result = subprocess.run(['ping', '-c', '1', '-W', '1', ip_address],
                               stdout=subprocess.DEVNULL,
                               stderr=subprocess.DEVNULL)
        return result.returncode == 0
    except:
        return False


def get_gpu_usage():
    """GPU 사용률 조회"""
    if torch.cuda.is_available():
        return torch.cuda.utilization()
    return 0
```

---

## 성능 목표 및 달성 가능성

### 목표
- **처리 시간**: < 300ms (디팔렛타이저 분류 고려, 2.5초 허용)
- **네트워크 지연**: < 50ms (같은 도시 VPN)
- **정확도**: mAP > 0.90

### 실제 달성 예상 (원격 연결 + RTX 4080 Super + YOLOv11l)
- **총 처리 시간**: 100-200ms ✅
  - 이미지 인코딩: 10-20ms
  - 네트워크 왕복: 40-100ms (같은 도시 Tailscale VPN)
  - AI 추론: 15-20ms
  - GPIO 제어: 1-5ms
- **디팔렛타이저 허용 시간**: 2.5초
- **여유 시간**: 2.3초 이상 (10배 이상 여유) ✅
- **VRAM 사용**:
  - 학습 시: 10-14GB (배치 32 기준, 각 모델 독립 학습)
  - 추론 시: ~8GB (Component + Solder 이중 모델 양면 동시)
  - 여유: 8-10GB (안정적 운영 가능)
- **FP16 최적화**: VRAM 50% 절약 + 속도 1.5배 향상 가능 ✅

---

## YOLO 어노테이션 이미지 실시간 스트리밍 ⭐ 신규

C# WinForms 모니터링 앱에서 **YOLO 바운딩 박스가 그려진 영상**을 실시간 30fps로 표시하기 위한 API 및 구현 가이드입니다.

### 아키텍처 개요

```
[라즈베리파이]
   │
   ├─► 스레드 1: 모니터링용 프레임 전송 (30fps)
   │      POST /upload_frame
   │      ↓
   ├─► 스레드 2: AI 추론 요청 (PCB 감지 시만)
   │      POST /predict_dual
   │      ↓
   │   [Flask 서버]
   │      ├─ YOLO 이중 모델 추론
   │      ├─ 어노테이션 이미지 생성 (바운딩 박스 그리기)
   │      ├─ latest_annotated_frames 업데이트
   │      └─ 응답에 Base64 어노테이션 이미지 포함
   │
   └───► [C# WinForms]
         │
         GET /video_feed_annotated/left (MJPEG)
         GET /video_feed_annotated/right
         ↓
         ✅ 실시간 30fps 어노테이션 영상 표시
```

---

### 1. `/predict_dual` API 응답 확장

**기존 응답에 어노테이션 이미지 추가**:

```json
{
  "status": "ok",
  "final_defect_type": "납땜불량",
  "final_confidence": 0.87,

  "left_annotated_image": "base64_encoded_jpeg...",   // ✅ 신규
  "right_annotated_image": "base64_encoded_jpeg...",  // ✅ 신규

  "left_result": {
    "defect_type": "정상",
    "confidence": 0.95,
    "boxes": []
  },
  "right_result": {
    "defect_type": "납땜불량",
    "confidence": 0.87,
    "boxes": [
      {
        "class": "cold_joint",
        "confidence": 0.87,
        "bbox": [120, 80, 200, 150]
      }
    ]
  },

  "gpio_signal": {"pin": 27, "duration_ms": 300},
  "inference_time_ms": 95.2
}
```

**server/app.py 구현 (일부)**:

```python
# 좌측 프레임: 부품 검출 모델
if yolo_component_model is not None:
    left_results = yolo_component_model(left_frame)

    # ✅ YOLO 어노테이션 이미지 생성
    left_annotated = left_results[0].plot()  # 바운딩 박스 그리기

    # ✅ JPEG → Base64 인코딩
    _, buffer = cv2.imencode('.jpg', left_annotated, [cv2.IMWRITE_JPEG_QUALITY, 85])
    left_annotated_base64 = base64.b64encode(buffer).decode('utf-8')

    # ✅ MJPEG 스트리밍용 최신 프레임 저장
    latest_annotated_frames['left'] = buffer.tobytes()

# 우측 프레임: 납땜 불량 모델
if yolo_solder_model is not None:
    right_results = yolo_solder_model(right_frame)
    right_annotated = right_results[0].plot()

    _, buffer = cv2.imencode('.jpg', right_annotated)
    right_annotated_base64 = base64.b64encode(buffer).decode('utf-8')
    latest_annotated_frames['right'] = buffer.tobytes()
```

---

### 2. MJPEG 스트리밍 엔드포인트 (신규)

**엔드포인트**: `/video_feed_annotated/<camera_id>`
**메서드**: `GET`
**설명**: YOLO 바운딩 박스가 그려진 실시간 MJPEG 스트림

#### 요청 예시

```http
GET /video_feed_annotated/left HTTP/1.1
Host: 100.64.1.1:5000
```

#### 응답

```
Content-Type: multipart/x-mixed-replace; boundary=frame

--frame
Content-Type: image/jpeg

[JPEG 이미지 데이터 (바운딩 박스 포함)]
--frame
Content-Type: image/jpeg

[JPEG 이미지 데이터]
--frame
...
(무한 반복, 30fps)
```

#### server/app.py 구현

```python
@app.route('/video_feed_annotated/<camera_id>', methods=['GET'])
def video_feed_annotated(camera_id):
    """
    YOLO 바운딩 박스가 그려진 MJPEG 스트림

    Args:
        camera_id: "left" (부품 검출) 또는 "right" (납땜 불량)
    """
    if camera_id not in ['left', 'right']:
        return jsonify({'error': 'Invalid camera_id'}), 400

    def generate_mjpeg_stream():
        while True:
            frame_data = latest_annotated_frames.get(camera_id)

            if frame_data is not None:
                yield (b'--frame\r\n'
                       b'Content-Type: image/jpeg\r\n\r\n' + frame_data + b'\r\n')
            else:
                # 프레임 없으면 빈 이미지
                empty_frame = np.zeros((480, 640, 3), dtype=np.uint8)
                cv2.putText(empty_frame, f'No frame from {camera_id}',
                           (50, 240), cv2.FONT_HERSHEY_SIMPLEX, 0.8, (255,255,255), 2)
                _, buffer = cv2.imencode('.jpg', empty_frame)
                yield (b'--frame\r\n'
                       b'Content-Type: image/jpeg\r\n\r\n' + buffer.tobytes() + b'\r\n')

            time.sleep(0.033)  # 30fps

    return Response(generate_mjpeg_stream(),
                   mimetype='multipart/x-mixed-replace; boundary=frame')
```

---

### 3. C# WinForms에서 MJPEG 스트림 표시

**필요한 NuGet 패키지**: `AForge.Video`

```bash
Install-Package AForge.Video
```

**C# 코드 예시**:

```csharp
using AForge.Video;
using System.Drawing;
using System.Windows.Forms;

public partial class MonitoringForm : Form
{
    private MJPEGStream leftStreamAnnotated;
    private MJPEGStream rightStreamAnnotated;

    private void StartAnnotatedStreaming()
    {
        // ✅ 좌측 YOLO 어노테이션 영상
        leftStreamAnnotated = new MJPEGStream("http://100.64.1.1:5000/video_feed_annotated/left");
        leftStreamAnnotated.NewFrame += (sender, eventArgs) => {
            pictureBoxLeft.Image?.Dispose();
            pictureBoxLeft.Image = (Bitmap)eventArgs.Frame.Clone();
        };
        leftStreamAnnotated.Start();

        // ✅ 우측 YOLO 어노테이션 영상
        rightStreamAnnotated = new MJPEGStream("http://100.64.1.1:5000/video_feed_annotated/right");
        rightStreamAnnotated.NewFrame += (sender, eventArgs) => {
            pictureBoxRight.Image?.Dispose();
            pictureBoxRight.Image = (Bitmap)eventArgs.Frame.Clone();
        };
        rightStreamAnnotated.Start();
    }

    private void StopStreaming()
    {
        leftStreamAnnotated?.Stop();
        rightStreamAnnotated?.Stop();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        StopStreaming();
        base.OnFormClosing(e);
    }
}
```

---

### 4. 라즈베리파이 멀티스레드 구조

라즈베리파이에서 2개의 스레드를 동시 실행:

1. **스레드 1**: 모니터링용 프레임 전송 (30fps)
2. **스레드 2**: AI 추론 요청 (PCB 감지 시만)

**예제 코드**:

```python
import threading
import cv2
import requests
import base64
import time

class DualPurposeClient:
    def __init__(self, server_url):
        self.server_url = server_url

    def monitoring_thread(self, camera_id, camera_index):
        """모니터링용 30fps 스트리밍"""
        cap = cv2.VideoCapture(camera_index)

        while True:
            ret, frame = cap.read()
            if ret:
                # /upload_frame에 전송
                _, buffer = cv2.imencode('.jpg', frame, [cv2.IMWRITE_JPEG_QUALITY, 85])
                image_base64 = base64.b64encode(buffer).decode('utf-8')

                requests.post(
                    f"{self.server_url}/upload_frame",
                    json={"camera_id": camera_id, "image": image_base64},
                    timeout=5
                )

            time.sleep(0.033)  # 30fps

    def inference_thread(self, left_index, right_index):
        """AI 추론용 (PCB 감지 시만)"""
        cap_left = cv2.VideoCapture(left_index)
        cap_right = cv2.VideoCapture(right_index)

        while True:
            # PCB 감지 (GPIO 센서 또는 움직임 감지)
            if pcb_detected():
                ret_left, left_frame = cap_left.read()
                ret_right, right_frame = cap_right.read()

                if ret_left and ret_right:
                    # Base64 인코딩
                    _, left_buffer = cv2.imencode('.jpg', left_frame, [cv2.IMWRITE_JPEG_QUALITY, 85])
                    _, right_buffer = cv2.imencode('.jpg', right_frame)

                    left_base64 = base64.b64encode(left_buffer).decode('utf-8')
                    right_base64 = base64.b64encode(right_buffer).decode('utf-8')

                    # AI 추론 요청
                    response = requests.post(
                        f"{self.server_url}/predict_dual",
                        json={
                            "left_image": left_base64,
                            "right_image": right_base64
                        },
                        timeout=10
                    )

                    result = response.json()

                    # GPIO 제어
                    control_gpio(result['gpio_signal']['pin'])

            time.sleep(0.1)  # 10Hz 체크

# 실행
client = DualPurposeClient("http://100.64.1.1:5000")

t1 = threading.Thread(target=client.monitoring_thread, args=('left', 0), daemon=True)
t2 = threading.Thread(target=client.monitoring_thread, args=('right', 1), daemon=True)
t3 = threading.Thread(target=client.inference_thread, args=(0, 1), daemon=True)

t1.start()
t2.start()
t3.start()

# 메인 스레드는 대기
while True:
    time.sleep(1)
```

---

### 5. 성능 고려사항

**네트워크 대역폭**:
- 640x480 JPEG (품질 85): ~30-50KB/프레임
- 30fps × 2개 카메라 = **1.8-3.0 MB/초**
- 로컬 LAN (100Mbps): 여유 충분 ✅
- Tailscale VPN (10-50Mbps): 주의 필요 ⚠️

**Flask 서버 부하**:
- MJPEG 스트림: 별도 스레드 (threaded=True)
- AI 추론: 독립 처리 (병렬 가능)
- 동시 접속: C# 1대 + 웹 브라우저 테스트 = 문제없음 ✅

**YOLO 추론 시간**:
- 부품 검출 모델: 30-50ms
- 납땜 불량 모델: 30-50ms
- 총합 (병렬): ~60ms
- 어노테이션 이미지 생성: +10ms
- **합계**: 70-80ms (목표 < 300ms) ✅

---

## 참고 자료

- [Flask 공식 문서](https://flask.palletsprojects.com/)
- [OpenCV 웹캠 가이드](https://docs.opencv.org/4.x/dd/d43/tutorial_py_video_display.html)
- [YOLO Ultralytics 문서](https://docs.ultralytics.com/)

---

## 관련 문서

본 Flask 서버 구축 가이드와 연관된 상세 문서:

1. **PCB_Defect_Detection_Project.md** - 전체 프로젝트 로드맵 및 시스템 아키텍처
2. **data/pcb_defects.yaml** - YOLO 클래스 정의 및 GPIO 매핑 (통일된 참조)
3. **RaspberryPi_Setup.md** - 라즈베리파이 웹캠 클라이언트 및 GPIO 제어 설정
4. **OHT_System_Setup.md** - OHT 시스템 하드웨어 및 제어 설계 ⭐
5. **MySQL_Database_Design.md** - 데이터베이스 스키마 및 연동 가이드
6. **CSharp_WinForms_Guide.md** - C# WinForms 모니터링 앱 개발 기본
7. **CSharp_WinForms_Design_Specification.md** - UI 상세 설계 (권한 시스템, 7개 화면)
8. **Logging_Strategy.md** - 통합 로깅 전략 (Flask 서버 로깅 포함)

---

**작성일**: 2025-10-28
**최종 수정일**: 2025-11-10
**버전**: 1.2 ⭐
**주요 변경사항**:
- **v1.2 (2025-11-10)**:
  - YOLO 어노테이션 이미지 실시간 스트리밍 API 추가
  - `/predict_dual` 응답에 어노테이션 이미지 Base64 포함
  - `/video_feed_annotated/<camera_id>` MJPEG 스트리밍 엔드포인트 추가
  - C# WinForms에서 AForge.Video 사용 방법 추가
  - 라즈베리파이 멀티스레드 구조 (모니터링 + 추론 분리) 추가
- **v1.1 (2025-10-23)**:
  - IP 주소 명시 (100.64.1.1, 100.64.1.2, 100.64.1.3)
  - 양면 통합 로직 명확화 (라즈베리파이 1만 GPIO 제어)
  - YOLO 클래스 이름 통일 (data/pcb_defects.yaml 참조)
  - 폴더 구조 단순화 (routes/ 제거)
  - 관련 문서 참조 추가
