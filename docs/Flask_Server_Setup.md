# Flask 웹서버 기반 실시간 PCB 검사 시스템 구축 가이드

## 개요

이 가이드는 컨베이어 벨트 시스템에서 웹캠으로 촬영한 PCB 이미지를 실시간으로 Flask 서버로 전송하여 AI 추론을 수행하는 시스템 구축 방법을 설명합니다.

---

## 시스템 구성

### 하드웨어 구성
- **추론 서버 (GPU PC)**:
  - GPU: NVIDIA RTX 4080 Super (16GB VRAM)
  - AI 모델: YOLOv8l (Large) + 이상 탐지 하이브리드
  - 위치: 원격지 (같은 도시 내)
  - Flask 서버 실행:
    - 로컬: 192.168.0.10:5000 (선택)
    - 원격 (Tailscale): 100.x.x.x:5000 ⭐
- **라즈베리파이 1**: 좌측 웹캠 연결 + GPIO 제어
- **라즈베리파이 2**: 우측 웹캠 연결 전용
- **Windows PC**: C# WinForms 모니터링 앱
- **네트워크**:
  - 로컬 환경 (선택): LAN (192.168.0.x)
  - 원격 환경 (프로젝트): Tailscale VPN 메시 네트워크 ⭐

**참고**: 상세한 라즈베리파이 설정은 `RaspberryPi_Setup.md`, 데이터베이스 설계는 `MySQL_Database_Design.md` 참조

### 소프트웨어 구성
- **추론 서버**: Flask + PyTorch + YOLO v8
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
├── src/
│   ├── server/                 # Flask 추론 서버 (GPU PC)
│   │   ├── app.py              # Flask 메인 애플리케이션
│   │   ├── inference.py        # AI 추론 로직
│   │   └── config.py           # 서버 설정
│   │
│   └── client/                 # 웹캠 클라이언트 (라즈베리파이용)
│       ├── camera_client.py    # 웹캠 프레임 전송
│       └── config.py           # 클라이언트 설정
│
├── models/                     # 학습된 모델 파일
│   ├── yolo/
│   │   └── final/
│   │       └── yolo_best.pt
│   └── anomaly/
│       └── padim/
│           └── model.pth
│
├── data/
│   └── pcb_defects.yaml        # YOLO 클래스 정의 (통일된 참조)
│
└── configs/                    # 설정 파일
    ├── server_config.yaml      # Flask 서버 설정
    └── camera_config.yaml      # 카메라 클라이언트 설정

참고: routes/ 폴더는 사용하지 않음 (단순화)
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

# AI 추론 엔진 초기화
inference_engine = PCBInferenceEngine(
    yolo_model_path='models/yolo_best.pt',
    anomaly_model_path='models/anomaly_model.pth',
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
    양면(좌측+우측) 동시 검사

    설계 방식:
    1. 라즈베리파이 1 (좌측)과 라즈베리파이 2 (우측)가 각각 /predict로 프레임 전송
    2. Flask 서버가 양쪽 결과를 메모리에 일시 저장 (예: Redis 또는 dict)
    3. 양면 결과가 모두 수신되면 통합 판정 수행
    4. 최종 불량 분류 결과를 **라즈베리파이 1에만** GPIO 제어 신호로 전송

    참고: 현재는 동기 방식 예시이며, 실제로는 비동기 처리 권장
    """
    try:
        data = request.get_json()
        frame_left_base64 = data.get('frame_left')
        frame_right_base64 = data.get('frame_right')

        if not frame_left_base64 or not frame_right_base64:
            return jsonify({'error': 'Missing frame data'}), 400

        # 좌측 프레임 디코딩
        frame_left = decode_frame(frame_left_base64)
        # 우측 프레임 디코딩
        frame_right = decode_frame(frame_right_base64)

        # 양면 동시 추론
        result_left = inference_engine.predict(frame_left, 'left')
        result_right = inference_engine.predict(frame_right, 'right')

        # 결과 통합 (양면 모두 정상이어야 정상 판정)
        final_defect_type, gpio_signal = integrate_results(result_left, result_right)

        response = {
            'status': 'ok',
            'final_defect_type': final_defect_type,
            'gpio_signal': gpio_signal,  # 라즈베리파이 1 전용 GPIO 제어 신호
            'left': result_left,
            'right': result_right,
            'note': 'GPIO 제어는 라즈베리파이 1 (192.168.0.20)만 수행'
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

class PCBInferenceEngine:
    def __init__(self, yolo_model_path, anomaly_model_path, device='cuda'):
        """
        AI 추론 엔진 초기화

        Args:
            yolo_model_path: YOLO 모델 경로
            anomaly_model_path: 이상 탐지 모델 경로
            device: 'cuda' 또는 'cpu'
        """
        self.device = device

        # YOLO 모델 로드
        self.yolo_model = YOLO(yolo_model_path)
        self.yolo_model.to(device)

        # 이상 탐지 모델 로드 (나중에 구현)
        # self.anomaly_model = load_anomaly_model(anomaly_model_path, device)

        print(f"Models loaded on {device}")

    def predict(self, frame, camera_id):
        """
        PCB 이미지 추론

        Args:
            frame: OpenCV 이미지 (numpy array)
            camera_id: 'left' or 'right'

        Returns:
            dict: 추론 결과
        """
        start_time = time.time()

        # YOLO 추론
        results = self.yolo_model(frame, verbose=False)

        # 결과 파싱
        boxes = []
        defect_classes = []
        confidences = []

        for result in results:
            for box in result.boxes:
                x1, y1, x2, y2 = box.xyxy[0].cpu().numpy()
                conf = float(box.conf[0])
                cls = int(box.cls[0])
                cls_name = result.names[cls]

                boxes.append({
                    'x1': float(x1), 'y1': float(y1),
                    'x2': float(x2), 'y2': float(y2),
                    'confidence': conf,
                    'class': cls_name
                })

                defect_classes.append(cls_name)
                confidences.append(conf)

        # 불량 유형 판정
        defect_type, confidence = self._classify_defect(defect_classes, confidences)

        inference_time_ms = (time.time() - start_time) * 1000

        return {
            'defect_type': defect_type,
            'confidence': confidence,
            'boxes': boxes,
            'inference_time_ms': inference_time_ms
        }

    def _classify_defect(self, defect_classes, confidences):
        """
        불량 유형 분류

        참고: data/pcb_defects.yaml의 defect_categories 매핑 사용
        (실제 구현 시 YAML 파일을 로드하여 사용)

        불량 우선순위:
        1. 심각한 불량 → 폐기
        2. 납땜 불량 → 납땜 재작업
        3. 부품 불량 → 부품 재작업
        4. 정상
        """
        if not defect_classes:
            return '정상', 1.0

        # data/pcb_defects.yaml 클래스 정의 참조
        # (실제 구현 시 yaml.safe_load로 로드)
        component_defects = ['missing_component']
        solder_defects = ['open_circuit', 'short', 'cold_joint', 'solder_bridge', 'insufficient_solder']
        critical_defects = ['damaged_pad', 'spurious_copper']  # 심각한 불량
        pcb_defects = ['mouse_bite', 'spur', 'pin_hole']  # 기판 불량 (경미)

        max_confidence = max(confidences)

        # 심각한 불량 확인 (폐기)
        for cls in defect_classes:
            if cls in critical_defects:
                return '폐기', max_confidence

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

### 2-1. 웹캠 클라이언트 코드 (client/camera_client.py)

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
            server_url: Flask 서버 URL (예: http://192.168.0.10:5000)
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
    SERVER_URL = 'http://192.168.0.10:5000'  # 추론 서버 IP
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
python camera_client.py --camera_id left --camera_index 0 --server_url http://192.168.0.10:5000 --fps 10
```

**라즈베리파이 2 (우측 웹캠)**
```bash
python camera_client.py --camera_id right --camera_index 0 --server_url http://192.168.0.10:5000 --fps 10
```

### 3-3. 네트워크 설정

#### 로컬 네트워크 (선택)

1. **추론 서버 IP 확인**
```bash
# Linux/WSL
ip addr show

# 출력 예시: inet 192.168.0.10/24
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
# client/camera_client.py 수정

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

## Phase 5: 모니터링 및 로깅

### 5-1. 실시간 대시보드 (선택)

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

### 5-2. 로그 기록

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
- YOLOv8l + 이상 탐지 + 양면 동시: 6-8GB
- 여유 메모리: 8-10GB
- 결론: 메모리 부족 가능성 매우 낮음

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
            'raspberry_pi_1_online': check_raspberry_pi('192.168.0.20'),  # 좌측 카메라
            'raspberry_pi_2_online': check_raspberry_pi('192.168.0.21'),  # 우측 카메라
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

### 실제 달성 예상 (원격 연결 + RTX 4080 Super + YOLOv8l)
- **총 처리 시간**: 100-200ms ✅
  - 이미지 인코딩: 10-20ms
  - 네트워크 왕복: 40-100ms (같은 도시 Tailscale VPN)
  - AI 추론: 15-20ms
  - GPIO 제어: 1-5ms
- **디팔렛타이저 허용 시간**: 2.5초
- **여유 시간**: 2.3초 이상 (10배 이상 여유) ✅
- **VRAM 사용**:
  - 학습 시: 10-14GB (배치 32 기준)
  - 추론 시: 6-8GB (YOLO + 이상 탐지 + 양면 동시)
  - 여유: 8-10GB (안정적 운영 가능)
- **FP16 최적화**: VRAM 50% 절약 + 속도 1.5배 향상 가능 ✅

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
4. **MySQL_Database_Design.md** - 데이터베이스 스키마 및 연동 가이드
5. **CSharp_WinForms_Guide.md** - C# WinForms 모니터링 앱 개발 기본
6. **CSharp_WinForms_Design_Specification.md** - UI 상세 설계 (권한 시스템, 7개 화면)
7. **Logging_Strategy.md** - 통합 로깅 전략 (Flask 서버 로깅 포함)

---

**작성일**: 2025-10-22
**최종 수정일**: 2025-10-23
**버전**: 1.1
**주요 변경사항**:
- IP 주소 명시 (192.168.0.10, 192.168.0.20, 192.168.0.21)
- 양면 통합 로직 명확화 (라즈베리파이 1만 GPIO 제어)
- YOLO 클래스 이름 통일 (data/pcb_defects.yaml 참조)
- 폴더 구조 단순화 (routes/ 제거)
- 관련 문서 참조 추가
