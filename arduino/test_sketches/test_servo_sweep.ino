/**
 * test_servo_sweep.ino
 * 서보 모터 스위프 테스트
 *
 * 각 서보를 0도에서 180도까지 천천히 움직여서
 * 서보의 동작 범위와 응답을 확인합니다.
 *
 * 연결:
 * - 서보 1개를 핀 9에 연결
 * - 외부 전원 5V 연결 (서보 파워)
 * - GND 공통 연결
 *
 * 사용법:
 * 1. 테스트할 서보를 핀 9에 연결
 * 2. 업로드 후 시리얼 모니터 열기 (115200 baud)
 * 3. 서보가 0도 → 180도 → 0도로 반복 동작
 */

#include <Servo.h>

Servo testServo;

const int SERVO_PIN = 9;
const int SWEEP_DELAY = 15;  // 각도당 딜레이 (ms)

void setup() {
  Serial.begin(115200);
  Serial.println("=================================");
  Serial.println("Servo Sweep Test");
  Serial.println("=================================");

  testServo.attach(SERVO_PIN);

  Serial.print("Servo attached to pin ");
  Serial.println(SERVO_PIN);
  Serial.println("Starting sweep test...");
}

void loop() {
  // 0도 → 180도
  Serial.println("Sweeping 0 -> 180 degrees");
  for (int angle = 0; angle <= 180; angle++) {
    testServo.write(angle);
    Serial.print("Angle: ");
    Serial.println(angle);
    delay(SWEEP_DELAY);
  }

  delay(1000);

  // 180도 → 0도
  Serial.println("Sweeping 180 -> 0 degrees");
  for (int angle = 180; angle >= 0; angle--) {
    testServo.write(angle);
    Serial.print("Angle: ");
    Serial.println(angle);
    delay(SWEEP_DELAY);
  }

  delay(1000);

  Serial.println("Sweep cycle completed");
  Serial.println("---");
  delay(2000);
}
