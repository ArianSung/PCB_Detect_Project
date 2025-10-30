/**
 * robot_arm_controller.ino
 * PCB Defect Detection - Robot Arm Controller (Main Sketch)
 *
 * Arduino Mega 2560 기반 6축 로봇팔 제어
 * Raspberry Pi와 USB 시리얼 통신 (JSON 프로토콜)
 *
 * 하드웨어:
 * - Arduino Mega 2560
 * - 서보 모터 6개 (MG996R 또는 DS3218)
 * - 5V 10A 외부 전원
 * - 비상 정지 버튼 (GPIO 8, 풀업)
 *
 * 통신 프로토콜:
 * - Baudrate: 115200
 * - Format: JSON (UTF-8)
 * - Timeout: 5000ms
 *
 * 작성자: PCB Defect Detection Team
 * 버전: 1.0.0
 * 날짜: 2025-01-27
 */

#include <Servo.h>
#include <ArduinoJson.h>

// 헤더 파일 포함
#include "config.h"
#include "servo_control.h"
#include "box_manager.h"
#include "rail_control.h"     // ⭐ 신규: 레일 시스템
#include "serial_handler.h"

// ========================================
// 전역 변수
// ========================================
unsigned long lastHeartbeat = 0;
const unsigned long HEARTBEAT_INTERVAL = 5000;  // 5초마다 상태 출력

bool systemReady = false;

// ========================================
// setup() - 초기화
// ========================================
void setup() {
  // 1. 시리얼 통신 초기화
  initSerial();

  // 2. 핀 모드 설정
  pinMode(EMERGENCY_STOP_PIN, INPUT_PULLUP);  // 비상 정지 버튼 (풀업)
  pinMode(LED_STATUS_PIN, OUTPUT);            // 상태 LED

  // 3. LED 깜박임 (초기화 중)
  for (int i = 0; i < 3; i++) {
    digitalWrite(LED_STATUS_PIN, HIGH);
    delay(200);
    digitalWrite(LED_STATUS_PIN, LOW);
    delay(200);
  }

  // 4. 서보 초기화
  Serial.println("[INFO] Initializing servos...");
  initServos();

  // 5. 레일 시스템 초기화 ⭐ 신규
  Serial.println("[INFO] Initializing rail system...");
  initRail();

  // 6. 초기 자가 진단
  Serial.println("[INFO] Running self-test...");
  selfTest();

  // 7. 시스템 준비 완료
  systemReady = true;
  digitalWrite(LED_STATUS_PIN, HIGH);  // LED 켜기
  Serial.println("[INFO] ========================================");
  Serial.println("[INFO] System ready!");
  Serial.println("[INFO] Waiting for commands from Raspberry Pi...");
  Serial.println("[INFO] ========================================");

  lastHeartbeat = millis();
}

// ========================================
// loop() - 메인 루프
// ========================================
void loop() {
  // 1. 비상 정지 확인
  if (digitalRead(EMERGENCY_STOP_PIN) == LOW) {
    if (systemReady) {
      systemReady = false;
      stopAllServos();
      digitalWrite(LED_STATUS_PIN, LOW);
      Serial.println("[EMERGENCY] System stopped! Reset Arduino to resume.");
    }
    return;  // 비상 정지 상태에서는 명령 처리 안 함
  }

  // 2. 시리얼 명령 처리
  handleSerialCommand();

  // 3. 하트비트 (주기적 상태 출력)
  unsigned long currentTime = millis();
  if (currentTime - lastHeartbeat >= HEARTBEAT_INTERVAL) {
    sendHeartbeat();
    lastHeartbeat = currentTime;
  }

  // 4. LED 깜박임 (시스템 동작 중 표시)
  static unsigned long lastBlink = 0;
  static bool ledState = false;

  if (currentTime - lastBlink >= 1000) {
    ledState = !ledState;
    digitalWrite(LED_STATUS_PIN, ledState ? HIGH : LOW);
    lastBlink = currentTime;
  }
}

// ========================================
// 자가 진단 함수
// ========================================
void selfTest() {
  Serial.println("[TEST] Starting self-test...");

  // 1. 비상 정지 버튼 테스트
  if (digitalRead(EMERGENCY_STOP_PIN) == LOW) {
    Serial.println("[ERROR] Emergency stop button is pressed!");
    while (true) {
      digitalWrite(LED_STATUS_PIN, HIGH);
      delay(200);
      digitalWrite(LED_STATUS_PIN, LOW);
      delay(200);
    }
  }
  Serial.println("[TEST] Emergency stop button: OK");

  // 2. 서보 응답 테스트 (각 서보를 5도씩 움직여서 확인)
  Serial.println("[TEST] Testing servo response...");

  // 각 서보를 홈 포지션에서 ±5도 움직여보기
  int testAngles[6][3] = {
    {HOME_BASE - 5, HOME_BASE, HOME_BASE + 5},
    {HOME_SHOULDER - 5, HOME_SHOULDER, HOME_SHOULDER + 5},
    {HOME_ELBOW - 5, HOME_ELBOW, HOME_ELBOW + 5},
    {HOME_WRIST_PITCH - 5, HOME_WRIST_PITCH, HOME_WRIST_PITCH + 5},
    {HOME_WRIST_ROLL - 5, HOME_WRIST_ROLL, HOME_WRIST_ROLL + 5},
    {GRIPPER_OPEN, GRIPPER_OPEN, GRIPPER_CLOSE}
  };

  Servo* servos[6] = {&servoBase, &servoShoulder, &servoElbow,
                      &servoWristPitch, &servoWristRoll, &servoGripper};

  const char* servoNames[6] = {"Base", "Shoulder", "Elbow",
                               "WristPitch", "WristRoll", "Gripper"};

  for (int i = 0; i < 6; i++) {
    Serial.print("[TEST] Testing ");
    Serial.print(servoNames[i]);
    Serial.print("... ");

    for (int j = 0; j < 3; j++) {
      servos[i]->write(testAngles[i][j]);
      delay(200);
    }

    Serial.println("OK");
  }

  // 3. 홈 포지션으로 복귀
  moveToHome();

  // 4. 레일 홈 테스트 ⭐ 신규
  Serial.println("[TEST] Homing rail system...");
  if (homeRail()) {
    Serial.println("[TEST] Rail homing: OK");
  } else {
    Serial.println("[ERROR] Rail homing failed!");
  }

  Serial.println("[TEST] Self-test completed successfully");
}

// ========================================
// 하트비트 (주기적 상태 전송)
// ========================================
void sendHeartbeat() {
  StaticJsonDocument<JSON_BUFFER_SIZE> doc;

  doc["status"] = "ok";
  doc["type"] = "heartbeat";
  doc["uptime"] = millis();
  doc["system_ready"] = systemReady;

  JsonObject angles = doc.createNestedObject("current_angles");
  angles["base"] = currentAngles[0];
  angles["shoulder"] = currentAngles[1];
  angles["elbow"] = currentAngles[2];
  angles["wrist_pitch"] = currentAngles[3];
  angles["wrist_roll"] = currentAngles[4];
  angles["gripper"] = currentAngles[5];

  // 레일 상태 ⭐ 신규
  int railPos;
  bool railCal, homeSwitch, endSwitch;
  getRailStatus(railPos, railCal, homeSwitch, endSwitch);

  JsonObject rail = doc.createNestedObject("rail_status");
  rail["position"] = railPos;
  rail["calibrated"] = railCal;
  rail["home_switch"] = homeSwitch;
  rail["end_switch"] = endSwitch;

  serializeJson(doc, Serial);
  Serial.println();
}
