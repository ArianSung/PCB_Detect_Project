/**
 * test_gripper.ino
 * 그리퍼 서보 테스트
 *
 * 그리퍼의 열기/닫기 동작을 반복하여
 * PCB를 잡는 힘과 각도를 캘리브레이션합니다.
 *
 * 연결:
 * - 그리퍼 서보를 핀 7에 연결
 * - 외부 전원 5V 연결
 * - GND 공통 연결
 *
 * 사용법:
 * 1. 그리퍼를 핀 7에 연결
 * 2. 업로드 후 시리얼 모니터 열기 (115200 baud)
 * 3. PCB 또는 테스트 물체를 그리퍼 사이에 배치
 * 4. 그리퍼가 자동으로 열기/닫기 반복
 * 5. 적절한 닫기 각도를 찾아서 GRIPPER_CLOSE_ANGLE 조정
 */

#include <Servo.h>

Servo gripperServo;

const int GRIPPER_PIN = 7;
const int GRIPPER_OPEN_ANGLE = 0;    // 완전히 열림
int GRIPPER_CLOSE_ANGLE = 75;        // 닫기 각도 (조정 가능)

const int HOLD_TIME = 3000;           // PCB 잡고 유지하는 시간 (ms)
const int MOVE_SPEED = 20;            // 서보 이동 딜레이 (ms)

void setup() {
  Serial.begin(115200);
  Serial.println("=================================");
  Serial.println("Gripper Test");
  Serial.println("=================================");

  gripperServo.attach(GRIPPER_PIN);

  Serial.print("Gripper attached to pin ");
  Serial.println(GRIPPER_PIN);
  Serial.print("Open angle: ");
  Serial.println(GRIPPER_OPEN_ANGLE);
  Serial.print("Close angle: ");
  Serial.println(GRIPPER_CLOSE_ANGLE);
  Serial.println();
  Serial.println("Commands:");
  Serial.println("  o - Open gripper");
  Serial.println("  c - Close gripper");
  Serial.println("  + - Increase close angle by 5");
  Serial.println("  - - Decrease close angle by 5");
  Serial.println("  a - Auto test (open/close loop)");
  Serial.println("=================================");

  // 초기 위치: 열림
  gripperServo.write(GRIPPER_OPEN_ANGLE);
  delay(1000);
}

void loop() {
  // 시리얼 명령어 처리
  if (Serial.available() > 0) {
    char command = Serial.read();

    switch (command) {
      case 'o':
      case 'O':
        openGripper();
        break;

      case 'c':
      case 'C':
        closeGripper();
        break;

      case '+':
        GRIPPER_CLOSE_ANGLE += 5;
        if (GRIPPER_CLOSE_ANGLE > 180) GRIPPER_CLOSE_ANGLE = 180;
        Serial.print("Close angle increased to: ");
        Serial.println(GRIPPER_CLOSE_ANGLE);
        break;

      case '-':
        GRIPPER_CLOSE_ANGLE -= 5;
        if (GRIPPER_CLOSE_ANGLE < 0) GRIPPER_CLOSE_ANGLE = 0;
        Serial.print("Close angle decreased to: ");
        Serial.println(GRIPPER_CLOSE_ANGLE);
        break;

      case 'a':
      case 'A':
        autoTest();
        break;

      default:
        break;
    }
  }
}

// 그리퍼 열기
void openGripper() {
  Serial.println("Opening gripper...");
  smoothMove(gripperServo.read(), GRIPPER_OPEN_ANGLE);
  Serial.println("Gripper opened");
}

// 그리퍼 닫기
void closeGripper() {
  Serial.println("Closing gripper...");
  smoothMove(gripperServo.read(), GRIPPER_CLOSE_ANGLE);
  Serial.println("Gripper closed");
  Serial.println("Check if PCB is held securely");
}

// 부드러운 서보 이동
void smoothMove(int fromAngle, int toAngle) {
  int step = (fromAngle < toAngle) ? 1 : -1;

  for (int angle = fromAngle; angle != toAngle; angle += step) {
    gripperServo.write(angle);
    delay(MOVE_SPEED);
  }

  gripperServo.write(toAngle);
}

// 자동 테스트
void autoTest() {
  Serial.println("=================================");
  Serial.println("Starting auto test (3 cycles)");
  Serial.println("=================================");

  for (int i = 1; i <= 3; i++) {
    Serial.print("Cycle ");
    Serial.print(i);
    Serial.println("/3");

    // 열기
    Serial.println("  Opening gripper...");
    smoothMove(gripperServo.read(), GRIPPER_OPEN_ANGLE);
    Serial.println("  Place PCB between gripper jaws");
    delay(2000);

    // 닫기
    Serial.println("  Closing gripper...");
    smoothMove(gripperServo.read(), GRIPPER_CLOSE_ANGLE);
    Serial.println("  Gripper closed - Check grip strength");

    // PCB 잡고 유지
    Serial.print("  Holding for ");
    Serial.print(HOLD_TIME / 1000);
    Serial.println(" seconds...");
    delay(HOLD_TIME);

    // 열기
    Serial.println("  Opening gripper...");
    smoothMove(gripperServo.read(), GRIPPER_OPEN_ANGLE);
    Serial.println("  PCB released");

    delay(2000);
    Serial.println();
  }

  Serial.println("=================================");
  Serial.println("Auto test completed");
  Serial.print("Final close angle: ");
  Serial.println(GRIPPER_CLOSE_ANGLE);
  Serial.println("Adjust GRIPPER_CLOSE_ANGLE in config.h if needed");
  Serial.println("=================================");
}
