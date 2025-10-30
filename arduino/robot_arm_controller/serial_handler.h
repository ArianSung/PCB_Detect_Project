/**
 * serial_handler.h
 * PCB Defect Detection - Serial Communication Handler
 *
 * JSON 기반 시리얼 통신 처리
 */

#ifndef SERIAL_HANDLER_H
#define SERIAL_HANDLER_H

#include <ArduinoJson.h>
#include "config.h"
#include "servo_control.h"
#include "box_manager.h"
#include "rail_control.h"  // ⭐ 신규

// ========================================
// JSON 응답 전송 함수
// ========================================
void sendJsonResponse(const char* status, const char* message, const char* command = "") {
  StaticJsonDocument<JSON_BUFFER_SIZE> doc;

  doc["status"] = status;
  doc["message"] = message;
  if (strlen(command) > 0) {
    doc["command"] = command;
  }
  doc["timestamp"] = millis();

  serializeJson(doc, Serial);
  Serial.println();
}

void sendErrorResponse(const char* message, const char* command = "") {
  sendJsonResponse("error", message, command);
}

void sendSuccessResponse(const char* message, const char* command = "") {
  sendJsonResponse("ok", message, command);
}

// ========================================
// place_pcb 명령 처리
// ========================================
void handlePlacePCB(JsonDocument &doc) {
  // 파라미터 추출
  const char* boxId = doc["box_id"];
  int slotIndex = doc["slot_index"];

  // 유효성 검사
  if (boxId == nullptr) {
    sendErrorResponse("Missing box_id parameter", "place_pcb");
    return;
  }

  if (!isValidBoxID(boxId)) {
    sendErrorResponse("Invalid box_id", "place_pcb");
    return;
  }

  if (slotIndex < 0 || slotIndex >= 5) {
    sendErrorResponse("Invalid slot_index (must be 0-4)", "place_pcb");
    return;
  }

  // 좌표 가져오기
  Coordinate targetCoord;
  if (!getBoxCoordinate(boxId, slotIndex, targetCoord)) {
    sendErrorResponse("Failed to get box coordinate", "place_pcb");
    return;
  }

  Serial.print("[INFO] Placing PCB to ");
  Serial.print(boxId);
  Serial.print(" slot ");
  Serial.println(slotIndex);

  // PCB 픽업
  if (!pickupPCB()) {
    sendErrorResponse("Failed to pickup PCB", "place_pcb");
    return;
  }

  // PCB 배치
  if (!placePCB(targetCoord)) {
    sendErrorResponse("Failed to place PCB", "place_pcb");
    return;
  }

  // 성공 응답
  sendSuccessResponse("PCB placed successfully", "place_pcb");
}

// ========================================
// move_to 명령 처리
// ========================================
void handleMoveTo(JsonDocument &doc) {
  // 파라미터 추출
  int base = doc["base"] | -1;
  int shoulder = doc["shoulder"] | -1;
  int elbow = doc["elbow"] | -1;
  int wrist_pitch = doc["wrist_pitch"] | -1;
  int wrist_roll = doc["wrist_roll"] | -1;

  // 유효성 검사
  if (base == -1 || shoulder == -1 || elbow == -1 || wrist_pitch == -1 || wrist_roll == -1) {
    sendErrorResponse("Missing coordinate parameters", "move_to");
    return;
  }

  // 좌표 생성
  Coordinate target = {base, shoulder, elbow, wrist_pitch, wrist_roll};

  Serial.println("[INFO] Moving to custom coordinate");
  printCurrentAngles();

  // 이동
  moveToCoordinate(target);

  // 성공 응답
  sendSuccessResponse("Moved to coordinate", "move_to");
}

// ========================================
// home 명령 처리
// ========================================
void handleHome(JsonDocument &doc) {
  Serial.println("[INFO] Moving to home position");

  moveToHome();

  sendSuccessResponse("Moved to home position", "home");
}

// ========================================
// gripper_open 명령 처리
// ========================================
void handleGripperOpen(JsonDocument &doc) {
  Serial.println("[INFO] Opening gripper");

  openGripper();

  sendSuccessResponse("Gripper opened", "gripper_open");
}

// ========================================
// gripper_close 명령 처리
// ========================================
void handleGripperClose(JsonDocument &doc) {
  Serial.println("[INFO] Closing gripper");

  closeGripper();

  sendSuccessResponse("Gripper closed", "gripper_close");
}

// ========================================
// status 명령 처리
// ========================================
void handleStatus(JsonDocument &doc) {
  StaticJsonDocument<JSON_BUFFER_SIZE> response;

  response["status"] = "ok";
  response["command"] = "status";
  response["timestamp"] = millis();

  JsonObject angles = response.createNestedObject("current_angles");
  angles["base"] = currentAngles[0];
  angles["shoulder"] = currentAngles[1];
  angles["elbow"] = currentAngles[2];
  angles["wrist_pitch"] = currentAngles[3];
  angles["wrist_roll"] = currentAngles[4];
  angles["gripper"] = currentAngles[5];

  response["emergency_stop"] = (digitalRead(EMERGENCY_STOP_PIN) == LOW);

  serializeJson(response, Serial);
  Serial.println();
}

// ========================================
// move_rail 명령 처리 ⭐ 신규
// ========================================
void handleMoveRail(JsonDocument &doc) {
  const char* position = doc["position"];

  if (position == nullptr) {
    sendErrorResponse("Missing position parameter", "move_rail");
    return;
  }

  Serial.print("[INFO] Moving rail to position: ");
  Serial.println(position);

  if (moveRailTo(position)) {
    sendSuccessResponse("Rail moved successfully", "move_rail");
  } else {
    sendErrorResponse("Rail move failed", "move_rail");
  }
}

// ========================================
// home_rail 명령 처리 ⭐ 신규
// ========================================
void handleHomeRail(JsonDocument &doc) {
  Serial.println("[INFO] Homing rail system");

  if (homeRail()) {
    sendSuccessResponse("Rail homed successfully", "home_rail");
  } else {
    sendErrorResponse("Rail homing failed", "home_rail");
  }
}

// ========================================
// rail_status 명령 처리 ⭐ 신규
// ========================================
void handleRailStatus(JsonDocument &doc) {
  StaticJsonDocument<JSON_BUFFER_SIZE> response;

  response["status"] = "ok";
  response["command"] = "rail_status";
  response["timestamp"] = millis();

  int railPos;
  bool railCal, homeSwitch, endSwitch;
  getRailStatus(railPos, railCal, homeSwitch, endSwitch);

  JsonObject rail = response.createNestedObject("rail");
  rail["position"] = railPos;
  rail["calibrated"] = railCal;
  rail["home_switch"] = homeSwitch;
  rail["end_switch"] = endSwitch;

  serializeJson(response, Serial);
  Serial.println();
}

// ========================================
// test 명령 처리 (테스트용)
// ========================================
void handleTest(JsonDocument &doc) {
  Serial.println("[INFO] Running test sequence");

  // 홈 포지션으로 이동
  moveToHome();
  delay(1000);

  // 그리퍼 테스트
  closeGripper();
  delay(500);
  openGripper();
  delay(500);

  // 모든 박스 좌표 출력
  printAllBoxCoordinates();

  sendSuccessResponse("Test completed", "test");
}

// ========================================
// 시리얼 명령 처리 (메인 핸들러)
// ========================================
void handleSerialCommand() {
  if (Serial.available() > 0) {
    // JSON 문자열 읽기 (개행 문자까지)
    String jsonString = Serial.readStringUntil('\n');
    jsonString.trim();

    if (jsonString.length() == 0) {
      return;  // 빈 문자열 무시
    }

    Serial.print("[DEBUG] Received: ");
    Serial.println(jsonString);

    // JSON 파싱
    StaticJsonDocument<JSON_BUFFER_SIZE> doc;
    DeserializationError error = deserializeJson(doc, jsonString);

    if (error) {
      Serial.print("[ERROR] JSON parsing failed: ");
      Serial.println(error.c_str());
      sendErrorResponse("Invalid JSON format");
      return;
    }

    // 명령 추출
    const char* command = doc["command"];
    if (command == nullptr) {
      sendErrorResponse("Missing command field");
      return;
    }

    // 비상 정지 확인
    if (digitalRead(EMERGENCY_STOP_PIN) == LOW) {
      sendErrorResponse("Emergency stop activated. Reset required.");
      return;
    }

    // 명령 라우팅
    if (strcmp(command, "place_pcb") == 0) {
      handlePlacePCB(doc);
    } else if (strcmp(command, "move_to") == 0) {
      handleMoveTo(doc);
    } else if (strcmp(command, "home") == 0) {
      handleHome(doc);
    } else if (strcmp(command, "gripper_open") == 0) {
      handleGripperOpen(doc);
    } else if (strcmp(command, "gripper_close") == 0) {
      handleGripperClose(doc);
    } else if (strcmp(command, "status") == 0) {
      handleStatus(doc);
    } else if (strcmp(command, "move_rail") == 0) {  // ⭐ 신규
      handleMoveRail(doc);
    } else if (strcmp(command, "home_rail") == 0) {  // ⭐ 신규
      handleHomeRail(doc);
    } else if (strcmp(command, "rail_status") == 0) {  // ⭐ 신규
      handleRailStatus(doc);
    } else if (strcmp(command, "test") == 0) {
      handleTest(doc);
    } else {
      sendErrorResponse("Unknown command", command);
    }
  }
}

// ========================================
// 시리얼 초기화
// ========================================
void initSerial() {
  Serial.begin(SERIAL_BAUDRATE);
  Serial.setTimeout(SERIAL_TIMEOUT_MS);

  // 시작 메시지
  delay(1000);  // 시리얼 안정화 대기
  Serial.println("[INFO] ========================================");
  Serial.println("[INFO] PCB Defect Detection - Robot Arm Controller");
  Serial.println("[INFO] Arduino Mega 2560");
  Serial.println("[INFO] ========================================");
  Serial.print("[INFO] Serial baudrate: ");
  Serial.println(SERIAL_BAUDRATE);
  Serial.println("[INFO] Waiting for commands...");
}

#endif // SERIAL_HANDLER_H
