# Arduino Mega 2560 로봇팔 제어 시스템 설정 가이드

## 개요

이 가이드는 Arduino Mega 2560을 사용하여 5-6축 로봇팔을 제어하고, 라즈베리파이와 USB 시리얼 통신을 통해 PCB 분류 시스템을 구축하는 방법을 설명합니다.

**시스템 구성**:
- Arduino Mega 2560 (마이크로컨트롤러)
- 5-6축 로봇팔 (서보 모터 6개)
- 라즈베리파이 1 (USB 시리얼 통신)
- 6개 박스 슬롯 (3 카테고리 × 2 슬롯, 수직 2단 적재) + DISCARD 위치

---

## 하드웨어 요구사항

### 1. Arduino Mega 2560
- **마이크로컨트롤러**: ATmega2560
- **디지털 I/O 핀**: 54개
- **PWM 핀**: 15개 (서보 제어용)
- **메모리**: 256KB Flash, 8KB SRAM
- **USB 포트**: Type-B (라즈베리파이 연결용)

### 2. 서보 모터 (6개)
- **모델 권장**: MG996R 또는 DS3218 (20kg·cm 토크)
- **전압**: 4.8V ~ 6.0V
- **PWM 신호**: 50Hz (20ms 주기)
- **각도 범위**: 0° ~ 180°

**서보 모터 용도**:
1. **Base (베이스)**: 로봇팔 회전 (0-180°)
2. **Shoulder (어깨)**: 로봇팔 들어올리기 (0-180°)
3. **Elbow (팔꿈치)**: 로봇팔 굽히기 (0-180°)
4. **Wrist Pitch (손목 피치)**: 손목 상하 (0-180°)
5. **Wrist Roll (손목 롤)**: 손목 회전 (0-180°)
6. **Gripper (그리퍼)**: PCB 잡기/놓기 (0-90°)

### 3. 전원 공급 장치
- **Arduino 전원**: USB 5V (라즈베리파이) 또는 별도 어댑터 (7-12V)
- **서보 전원**: 5V 10A 전원 공급기 (서보 6개 동시 구동)
  - **중요**: 서보와 Arduino 전원 분리 필수 (노이즈 및 전압 강하 방지)
  - **Ground 공통 연결**: Arduino GND와 서보 전원 GND 연결

### 4. 추가 부품
- 점퍼 와이어 (M-M, M-F)
- 브레드보드 또는 PCB 보드
- USB Type-B 케이블 (Arduino ↔ 라즈베리파이)
- 비상 정지 버튼 (선택)
- 리미트 스위치 (선택, 안전 기능)

---

## 소프트웨어 환경

### 1. Arduino IDE 설치

**Windows/Linux/Mac 공통**:
1. Arduino IDE 다운로드: https://www.arduino.cc/en/software
2. Arduino Mega 2560 보드 선택:
   - Tools → Board → Arduino Mega or Mega 2560
3. 시리얼 포트 선택:
   - Tools → Port → /dev/ttyACM0 (Linux) 또는 COM3 (Windows)

### 2. 필수 라이브러리 설치

#### 2-1. Servo 라이브러리 (기본 포함)
Arduino IDE에 기본 포함되어 있으므로 별도 설치 불필요

#### 2-2. ArduinoJson 라이브러리
**설치 방법 1: Library Manager 사용**
```
Arduino IDE → Tools → Manage Libraries
검색: "ArduinoJson"
작성자: Benoit Blanchon
버전: 6.21.0 이상 설치
```

**설치 방법 2: ZIP 파일 수동 설치**
```bash
# GitHub에서 다운로드
wget https://github.com/bblanchon/ArduinoJson/releases/download/v6.21.0/ArduinoJson-v6.21.0.zip

# Arduino IDE → Sketch → Include Library → Add .ZIP Library
# 다운로드한 ZIP 파일 선택
```

---

## 하드웨어 연결

### 서보 모터 연결 (PWM 핀)

| 서보 모터 | Arduino 핀 | 용도 | 각도 범위 |
|-----------|-----------|------|-----------|
| Base | 2번 핀 | 베이스 회전 | 0-180° |
| Shoulder | 3번 핀 | 어깨 상하 | 0-180° |
| Elbow | 4번 핀 | 팔꿈치 굽히기 | 0-180° |
| Wrist Pitch | 5번 핀 | 손목 피치 | 0-180° |
| Wrist Roll | 6번 핀 | 손목 롤 | 0-180° |
| Gripper | 7번 핀 | 그리퍼 열기/닫기 | 0-90° |

**연결 방법**:
```
서보 모터 핀:
- 주황색/노란색 (신호): Arduino PWM 핀 (2~7)
- 빨간색 (VCC): 5V 전원 공급기 (+)
- 갈색/검은색 (GND): 5V 전원 공급기 (-) 및 Arduino GND
```

**전원 분리 회로**:
```
Arduino Mega 2560
    |
    ├─ USB 5V ← 라즈베리파이
    │
    └─ GND ──┐
             │
             ├─────── 서보 GND (-)
             │
5V 10A PSU ──┘
    |
    └─ VCC (+) ──→ 서보 VCC (빨간색)
```

### 비상 정지 버튼 (선택)
```
Arduino 핀 8 ──→ 버튼 ──→ GND
(내부 풀업 저항 사용)
```

---

## 프로젝트 구조

```
~/work_project/arduino/
├── robot_arm_controller/
│   ├── robot_arm_controller.ino    # 메인 스케치 파일
│   ├── config.h                    # 설정 (핀, 좌표 테이블)
│   ├── servo_control.h             # 서보 제어 함수
│   ├── serial_handler.h            # 시리얼 통신 핸들러
│   └── box_manager.h               # 박스 좌표 매핑
├── libraries/                      # 필요한 라이브러리
│   ├── Servo/                      # 서보 라이브러리 (기본)
│   └── ArduinoJson/                # JSON 라이브러리
└── test_sketches/                  # 테스트 스케치
    ├── test_servo.ino              # 서보 단독 테스트
    └── test_serial.ino             # 시리얼 통신 테스트
```

---

## 스케치 파일 상세

### 1. `robot_arm_controller.ino` (메인 스케치)

**주요 기능**:
- 라즈베리파이로부터 JSON 명령 수신
- 서보 모터 제어
- 박스 좌표 계산 및 이동
- PCB 픽업 및 배치

**메인 루프**:
```cpp
void loop() {
  // 1. 시리얼 데이터 수신
  if (Serial.available() > 0) {
    String jsonString = Serial.readStringUntil('\n');

    // 2. JSON 파싱
    StaticJsonDocument<512> doc;
    deserializeJson(doc, jsonString);

    // 3. 명령 처리
    String command = doc["command"];
    if (command == "place_pcb") {
      place_pcb(doc);
    } else if (command == "home") {
      move_to_home();
    }
  }
}
```

### 2. `config.h` (설정 파일)

**서보 핀 정의**:
```cpp
#define SERVO_PIN_BASE          2
#define SERVO_PIN_SHOULDER      3
#define SERVO_PIN_ELBOW         4
#define SERVO_PIN_WRIST_PITCH   5
#define SERVO_PIN_WRIST_ROLL    6
#define SERVO_PIN_GRIPPER       7
```

**홈 포지션 (초기 위치)**:
```cpp
#define HOME_BASE       90  // 중앙
#define HOME_SHOULDER   90
#define HOME_ELBOW      90
#define HOME_WRIST_PITCH 90
#define HOME_WRIST_ROLL  90
#define HOME_GRIPPER     0  // 열림
```

**6개 슬롯 좌표 테이블** (예시):
```cpp
// 박스 좌표 구조체
struct Coordinate {
  int base;         // 베이스 각도 (0-180)
  int shoulder;     // 어깨 각도 (0-180)
  int elbow;        // 팔꿈치 각도 (0-180)
  int wrist_pitch;  // 손목 피치 각도 (0-180)
  int wrist_roll;   // 손목 롤 각도 (0-180)
};

// NORMAL 박스 (슬롯 0-1, 수직 2단)
const Coordinate NORMAL_SLOTS[2] = {
  {90, 115, 85, 90, 90},  // 슬롯 0 (하단)
  {90, 115, 75, 90, 90}   // 슬롯 1 (상단, elbow -10도)
};

// COMPONENT_DEFECT 박스 (슬롯 0-1, 수직 2단)
const Coordinate COMPONENT_DEFECT_SLOTS[2] = {
  {90, 105, 85, 90, 90},  // 슬롯 0 (하단)
  {90, 105, 75, 90, 90}   // 슬롯 1 (상단, elbow -10도)
};

// SOLDER_DEFECT 박스 (슬롯 0-1, 수직 2단)
const Coordinate SOLDER_DEFECT_SLOTS[2] = {
  {90, 95, 85, 90, 90},   // 슬롯 0 (하단)
  {90, 95, 75, 90, 90}    // 슬롯 1 (상단, elbow -10도)
};

// DISCARD 위치 (슬롯 관리 안 함)
const Coordinate DISCARD_POSITION = {90, 85, 70, 90, 90};
```

### 3. `servo_control.h` (서보 제어)

**주요 함수**:
```cpp
// 서보 초기화
void initServos() {
  servo_base.attach(SERVO_PIN_BASE);
  servo_shoulder.attach(SERVO_PIN_SHOULDER);
  // ... 나머지 서보 attach

  // 홈 포지션으로 이동
  moveToHome();
}

// 부드러운 서보 이동 (속도 제어)
void moveServoSmooth(Servo &servo, int targetAngle, int speed) {
  int currentAngle = servo.read();
  int step = (currentAngle < targetAngle) ? 1 : -1;

  for (int angle = currentAngle; angle != targetAngle; angle += step) {
    servo.write(angle);
    delay(speed);  // speed: 10~50ms (낮을수록 빠름)
  }
}

// 모든 서보 동시 이동
void moveToPosition(Coordinate coord) {
  servo_base.write(coord.base);
  servo_shoulder.write(coord.shoulder);
  servo_elbow.write(coord.elbow);
  servo_wrist_pitch.write(coord.wrist);
  delay(2000);  // 도착 대기
}
```

### 4. `serial_handler.h` (시리얼 통신)

**JSON 명령 수신 및 파싱**:
```cpp
void handleSerialCommand() {
  if (Serial.available() > 0) {
    String jsonString = Serial.readStringUntil('\n');

    StaticJsonDocument<512> doc;
    DeserializationError error = deserializeJson(doc, jsonString);

    if (error) {
      sendErrorResponse("JSON parse error");
      return;
    }

    // 명령 처리
    const char* command = doc["command"];
    if (strcmp(command, "place_pcb") == 0) {
      placePCB(doc);
    } else if (strcmp(command, "home") == 0) {
      moveToHome();
      sendSuccessResponse("Moved to home position");
    }
  }
}

// 성공 응답 전송
void sendSuccessResponse(const char* message) {
  StaticJsonDocument<256> response;
  response["status"] = "success";
  response["message"] = message;
  response["execution_time_ms"] = millis() - startTime;

  serializeJson(response, Serial);
  Serial.println();
}
```

### 5. `box_manager.h` (박스 좌표 관리)

**박스 ID → 좌표 변환**:
```cpp
Coordinate getBoxSlotCoordinate(String box_id, int slot_number) {
  if (box_id == "NORMAL_A") {
    return NORMAL_A_SLOTS[slot_number];
  } else if (box_id == "NORMAL_B") {
    return NORMAL_B_SLOTS[slot_number];
  } else if (box_id == "COMPONENT_DEFECT_A") {
    return COMPONENT_DEFECT_A_SLOTS[slot_number];
  }
  // ... 나머지 박스

  // 기본값 (에러 시 홈 포지션 반환)
  return {HOME_BASE, HOME_SHOULDER, HOME_ELBOW, HOME_WRIST_PITCH};
}
```

---

## 시리얼 통신 프로토콜

### 라즈베리파이 → Arduino (명령)

**PCB 배치 명령**:
```json
{
  "command": "place_pcb",
  "box_id": "NORMAL_A",
  "slot_number": 2,
  "coordinates": {"x": 240.0, "y": 100.0, "z": 30.0}
}
```

**홈 포지션 이동 명령**:
```json
{
  "command": "home"
}
```

### Arduino → 라즈베리파이 (응답)

**성공 응답**:
```json
{
  "status": "success",
  "message": "PCB placed successfully",
  "execution_time_ms": 2350
}
```

**실패 응답**:
```json
{
  "status": "error",
  "message": "Invalid box_id",
  "error_code": 1001
}
```

---

## 좌표 캘리브레이션

### 1. 서보 각도 테스트

**test_servo.ino** 스케치 사용:
```cpp
// 서보 각도 테스트
void testServo() {
  // 90도 (중앙) 테스트
  servo_base.write(90);
  delay(2000);

  // 0도 (최소) 테스트
  servo_base.write(0);
  delay(2000);

  // 180도 (최대) 테스트
  servo_base.write(180);
  delay(2000);
}
```

### 2. 박스 슬롯 좌표 캘리브레이션 절차

**단계 1: 픽업 위치 캘리브레이션**
```
1. 로봇팔을 컨베이어 벨트 위치로 수동 이동
2. 각 서보 각도 측정 (Serial Monitor에서 읽기)
3. config.h에 픽업 좌표 저장
```

**단계 2: 슬롯 좌표 캘리브레이션**
```
1. 각 박스의 슬롯 0 위치로 수동 이동
2. 서보 각도 측정 및 저장
3. 슬롯 1-4는 규칙적인 간격으로 계산 (베이스 각도 +5도씩)
```

**단계 3: 미세 조정**
```
1. 실제 PCB로 테스트
2. 정확도 확인 (±2mm 이내)
3. 필요 시 각도 조정
```

### 3. 캘리브레이션 도구

**수동 제어 스케치**:
```cpp
// 시리얼 모니터에서 각도 입력 (예: B90, S120, E80)
void serialControl() {
  if (Serial.available()) {
    char cmd = Serial.read();
    int angle = Serial.parseInt();

    switch(cmd) {
      case 'B': servo_base.write(angle); break;
      case 'S': servo_shoulder.write(angle); break;
      case 'E': servo_elbow.write(angle); break;
      case 'W': servo_wrist_pitch.write(angle); break;
    }

    Serial.print("Base: "); Serial.print(servo_base.read());
    Serial.print(" Shoulder: "); Serial.println(servo_shoulder.read());
  }
}
```

---

## 안전 기능

### 1. 비상 정지 버튼

**config.h**:
```cpp
#define EMERGENCY_STOP_PIN 8

void checkEmergencyStop() {
  if (digitalRead(EMERGENCY_STOP_PIN) == LOW) {
    // 모든 서보 정지
    stopAllServos();

    // 에러 응답 전송
    sendErrorResponse("Emergency stop activated");

    // 무한 루프 (리셋 필요)
    while(true) {
      delay(100);
    }
  }
}
```

### 2. 작업 공간 제한

```cpp
bool isWithinWorkspace(Coordinate coord) {
  // 안전 범위 확인
  if (coord.base < 0 || coord.base > 180) return false;
  if (coord.shoulder < 30 || coord.shoulder > 150) return false;
  if (coord.elbow < 30 || coord.elbow > 150) return false;

  return true;
}
```

### 3. 타임아웃 처리

```cpp
unsigned long startTime;
#define TIMEOUT_MS 5000

void placePCB(JsonDocument &doc) {
  startTime = millis();

  // 동작 실행...

  // 타임아웃 체크
  if (millis() - startTime > TIMEOUT_MS) {
    sendErrorResponse("Operation timeout");
    return;
  }
}
```

---

## 테스트 절차

### 1. 서보 단독 테스트

```bash
# test_servo.ino 업로드
arduino-cli upload -p /dev/ttyACM0 --fqbn arduino:avr:mega test_servo.ino

# 시리얼 모니터 실행
arduino-cli monitor -p /dev/ttyACM0 -b 115200
```

### 2. 시리얼 통신 테스트

**라즈베리파이에서**:
```python
from serial_controller import get_arduino_controller

arduino = get_arduino_controller('/dev/ttyACM0', 115200)

# 홈 포지션 명령 전송
response = arduino.send_command({"command": "home"})
print(response)  # {"status": "success", ...}
```

### 3. PCB 배치 통합 테스트

```python
# 테스트 명령: NORMAL_A 슬롯 0에 배치
test_command = {
    "command": "place_pcb",
    "box_id": "NORMAL_A",
    "slot_number": 0,
    "coordinates": {"x": 200.0, "y": 100.0, "z": 30.0}
}

response = arduino.send_command(test_command)

if response.get('status') == 'success':
    print(f"✅ 성공! 실행 시간: {response['execution_time_ms']}ms")
else:
    print(f"❌ 실패: {response['message']}")
```

---

## 문제 해결 (Troubleshooting)

### 1. Arduino가 인식되지 않음

**증상**: `/dev/ttyACM0` 또는 COM 포트가 없음

**해결 방법**:
```bash
# 라즈베리파이에서
ls /dev/ttyUSB* /dev/ttyACM*

# 권한 확인
sudo usermod -a -G dialout $USER
sudo reboot
```

### 2. 서보가 떨림 (Jittering)

**원인**: 전원 부족 또는 노이즈

**해결 방법**:
1. 서보 전원 분리 (5V 10A PSU 사용)
2. GND 공통 연결 확인
3. 전원 케이블 굵기 확인 (최소 18AWG)
4. 캐패시터 추가 (1000μF, 서보 VCC-GND 사이)

### 3. JSON 파싱 오류

**증상**: "JSON parse error" 응답

**해결 방법**:
1. JSON 문자열 끝에 `\n` (newline) 추가 확인
2. ArduinoJson 버전 확인 (6.21.0 이상)
3. StaticJsonDocument 크기 증가 (512 → 1024)

### 4. 좌표 정확도 문제

**증상**: PCB가 슬롯에 정확히 배치되지 않음

**해결 방법**:
1. 서보 캘리브레이션 재실행
2. 슬롯 좌표 미세 조정 (±2~5도)
3. 그리퍼 닫힘 각도 조정
4. 배치 높이 오프셋 조정

---

## 성능 최적화

### 1. 동작 속도 최적화

```cpp
// 빠른 이동 (정확도 낮음)
#define SPEED_FAST 10  // 10ms delay

// 보통 이동 (균형)
#define SPEED_NORMAL 20  // 20ms delay

// 느린 이동 (정확도 높음)
#define SPEED_SLOW 50  // 50ms delay
```

### 2. 메모리 최적화

```cpp
// 큰 배열은 PROGMEM에 저장
const Coordinate NORMAL_A_SLOTS[5] PROGMEM = {
  {90, 120, 80, 90},
  // ...
};

// 읽기
Coordinate coord;
memcpy_P(&coord, &NORMAL_A_SLOTS[slot], sizeof(Coordinate));
```

---

## 다음 단계

1. **스케치 파일 작성**: `arduino/robot_arm_controller/` 디렉토리에 모든 파일 생성
2. **라이브러리 설치**: ArduinoJson 라이브러리 설치
3. **하드웨어 연결**: 서보 모터 6개 Arduino에 연결
4. **캘리브레이션**: 픽업 및 슬롯 좌표 캘리브레이션
5. **통합 테스트**: 라즈베리파이와 시리얼 통신 테스트

---

## 관련 문서

- [라즈베리파이 설정 가이드](RaspberryPi_Setup.md) - Phase 6: USB 시리얼 통신
- [로봇팔 설정 파일](../configs/robot_arm_config.yaml)
- [Arduino 공식 문서](https://www.arduino.cc/reference/en/)
- [ArduinoJson 문서](https://arduinojson.org/)

---

**작성일**: 2025-10-27
**버전**: 1.0
**하드웨어**: Arduino Mega 2560 + 6축 로봇팔
**소프트웨어**: Arduino IDE 2.3.0, ArduinoJson 6.21.0
