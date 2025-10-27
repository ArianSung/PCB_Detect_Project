/**
 * test_serial_communication.ino
 * 시리얼 통신 및 JSON 파싱 테스트
 *
 * Raspberry Pi와의 JSON 기반 시리얼 통신을 테스트합니다.
 * ArduinoJson 라이브러리가 필요합니다.
 *
 * 테스트 명령어 예시 (시리얼 모니터에 입력):
 * {"command":"test","value":123}
 * {"command":"echo","message":"Hello"}
 * {"command":"status"}
 *
 * 사용법:
 * 1. 업로드 후 시리얼 모니터 열기 (115200 baud)
 * 2. JSON 형식의 명령어 입력
 * 3. 응답 확인
 */

#include <ArduinoJson.h>

const int BAUDRATE = 115200;
const int JSON_BUFFER_SIZE = 512;

void setup() {
  Serial.begin(BAUDRATE);
  Serial.setTimeout(5000);

  delay(1000);

  Serial.println("=================================");
  Serial.println("Serial Communication Test");
  Serial.println("=================================");
  Serial.print("Baudrate: ");
  Serial.println(BAUDRATE);
  Serial.println("Send JSON commands via Serial Monitor");
  Serial.println("Example: {\"command\":\"test\",\"value\":123}");
  Serial.println("=================================");
}

void loop() {
  if (Serial.available() > 0) {
    // JSON 문자열 읽기
    String jsonString = Serial.readStringUntil('\n');
    jsonString.trim();

    if (jsonString.length() == 0) {
      return;
    }

    Serial.print("[RECEIVED] ");
    Serial.println(jsonString);

    // JSON 파싱
    StaticJsonDocument<JSON_BUFFER_SIZE> doc;
    DeserializationError error = deserializeJson(doc, jsonString);

    if (error) {
      Serial.print("[ERROR] JSON parsing failed: ");
      Serial.println(error.c_str());

      // 에러 응답 전송
      StaticJsonDocument<JSON_BUFFER_SIZE> response;
      response["status"] = "error";
      response["message"] = "Invalid JSON format";
      serializeJson(response, Serial);
      Serial.println();
      return;
    }

    // 명령어 추출
    const char* command = doc["command"];

    if (command == nullptr) {
      Serial.println("[ERROR] Missing command field");

      StaticJsonDocument<JSON_BUFFER_SIZE> response;
      response["status"] = "error";
      response["message"] = "Missing command field";
      serializeJson(response, Serial);
      Serial.println();
      return;
    }

    Serial.print("[COMMAND] ");
    Serial.println(command);

    // 명령어 처리
    if (strcmp(command, "test") == 0) {
      int value = doc["value"] | 0;
      Serial.print("[INFO] Test command with value: ");
      Serial.println(value);

      // 성공 응답
      StaticJsonDocument<JSON_BUFFER_SIZE> response;
      response["status"] = "ok";
      response["command"] = "test";
      response["received_value"] = value;
      response["result"] = value * 2;  // 값을 2배로 반환
      serializeJson(response, Serial);
      Serial.println();

    } else if (strcmp(command, "echo") == 0) {
      const char* message = doc["message"];
      Serial.print("[INFO] Echo command with message: ");
      Serial.println(message);

      // 에코 응답
      StaticJsonDocument<JSON_BUFFER_SIZE> response;
      response["status"] = "ok";
      response["command"] = "echo";
      response["echo"] = message;
      serializeJson(response, Serial);
      Serial.println();

    } else if (strcmp(command, "status") == 0) {
      Serial.println("[INFO] Status command");

      // 상태 응답
      StaticJsonDocument<JSON_BUFFER_SIZE> response;
      response["status"] = "ok";
      response["command"] = "status";
      response["uptime"] = millis();
      response["free_memory"] = freeMemory();
      serializeJson(response, Serial);
      Serial.println();

    } else {
      Serial.print("[ERROR] Unknown command: ");
      Serial.println(command);

      // 에러 응답
      StaticJsonDocument<JSON_BUFFER_SIZE> response;
      response["status"] = "error";
      response["message"] = "Unknown command";
      response["command"] = command;
      serializeJson(response, Serial);
      Serial.println();
    }
  }
}

// 여유 메모리 계산 (AVR 아키텍처)
int freeMemory() {
  extern int __heap_start, *__brkval;
  int v;
  return (int) &v - (__brkval == 0 ? (int) &__heap_start : (int) __brkval);
}
