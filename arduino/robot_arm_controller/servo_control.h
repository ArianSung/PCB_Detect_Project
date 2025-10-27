/**
 * servo_control.h
 * PCB Defect Detection - Servo Control Functions
 *
 * 서보 모터 제어 및 로봇팔 동작 함수
 */

#ifndef SERVO_CONTROL_H
#define SERVO_CONTROL_H

#include <Servo.h>
#include "config.h"

// ========================================
// 서보 객체 선언
// ========================================
Servo servoBase;
Servo servoShoulder;
Servo servoElbow;
Servo servoWristPitch;
Servo servoWristRoll;
Servo servoGripper;

// 현재 서보 각도 (메모리)
int currentAngles[6] = {HOME_BASE, HOME_SHOULDER, HOME_ELBOW,
                        HOME_WRIST_PITCH, HOME_WRIST_ROLL, HOME_GRIPPER};

// ========================================
// 서보 초기화
// ========================================
void initServos() {
  servoBase.attach(SERVO_BASE_PIN);
  servoShoulder.attach(SERVO_SHOULDER_PIN);
  servoElbow.attach(SERVO_ELBOW_PIN);
  servoWristPitch.attach(SERVO_WRIST_PITCH_PIN);
  servoWristRoll.attach(SERVO_WRIST_ROLL_PIN);
  servoGripper.attach(SERVO_GRIPPER_PIN);

  // 홈 포지션으로 이동
  moveToHome();

  Serial.println("[INFO] Servos initialized");
}

// ========================================
// 각도 제한 함수
// ========================================
int constrainAngle(int angle) {
  return constrain(angle, SERVO_MIN_ANGLE, SERVO_MAX_ANGLE);
}

// ========================================
// 단일 서보 이동 (부드럽게)
// ========================================
void moveSingleServo(Servo &servo, int &currentAngle, int targetAngle, int servoIndex) {
  targetAngle = constrainAngle(targetAngle);

  if (currentAngle == targetAngle) {
    return;  // 이미 목표 각도
  }

  int step = (currentAngle < targetAngle) ? 1 : -1;

  while (currentAngle != targetAngle) {
    currentAngle += step;
    servo.write(currentAngle);
    delay(MOVE_DELAY_MS);

    // 비상 정지 확인
    if (digitalRead(EMERGENCY_STOP_PIN) == LOW) {
      Serial.println("[ERROR] Emergency stop activated!");
      stopAllServos();
      return;
    }
  }

  currentAngles[servoIndex] = currentAngle;
}

// ========================================
// 모든 서보를 목표 좌표로 이동 (동기화)
// ========================================
void moveToCoordinate(const Coordinate &target) {
  // 목표 각도 배열
  int targetAngles[5] = {
    constrainAngle(target.base),
    constrainAngle(target.shoulder),
    constrainAngle(target.elbow),
    constrainAngle(target.wrist_pitch),
    constrainAngle(target.wrist_roll)
  };

  // 각 서보의 이동 거리 계산
  int distances[5];
  int maxDistance = 0;

  for (int i = 0; i < 5; i++) {
    distances[i] = abs(targetAngles[i] - currentAngles[i]);
    if (distances[i] > maxDistance) {
      maxDistance = distances[i];
    }
  }

  // 모든 서보를 동시에 부드럽게 이동
  for (int step = 0; step <= maxDistance; step++) {
    // 비상 정지 확인
    if (digitalRead(EMERGENCY_STOP_PIN) == LOW) {
      Serial.println("[ERROR] Emergency stop activated!");
      stopAllServos();
      return;
    }

    // 각 서보의 현재 단계 각도 계산
    int stepAngles[5];
    for (int i = 0; i < 5; i++) {
      if (distances[i] == 0) {
        stepAngles[i] = currentAngles[i];
      } else {
        float progress = (float)step / maxDistance;
        stepAngles[i] = currentAngles[i] + (int)((targetAngles[i] - currentAngles[i]) * progress);
      }
    }

    // 서보에 각도 전송
    servoBase.write(stepAngles[0]);
    servoShoulder.write(stepAngles[1]);
    servoElbow.write(stepAngles[2]);
    servoWristPitch.write(stepAngles[3]);
    servoWristRoll.write(stepAngles[4]);

    delay(MOVE_DELAY_MS);
  }

  // 현재 각도 업데이트
  for (int i = 0; i < 5; i++) {
    currentAngles[i] = targetAngles[i];
  }

  Serial.println("[INFO] Moved to target coordinate");
}

// ========================================
// 홈 포지션으로 이동
// ========================================
void moveToHome() {
  Coordinate homePos = {HOME_BASE, HOME_SHOULDER, HOME_ELBOW,
                        HOME_WRIST_PITCH, HOME_WRIST_ROLL};
  moveToCoordinate(homePos);

  // 그리퍼 열기
  servoGripper.write(GRIPPER_OPEN);
  currentAngles[5] = GRIPPER_OPEN;
  delay(GRIPPER_DELAY_MS);

  Serial.println("[INFO] Moved to home position");
}

// ========================================
// 그리퍼 제어
// ========================================
void openGripper() {
  servoGripper.write(GRIPPER_OPEN);
  currentAngles[5] = GRIPPER_OPEN;
  delay(GRIPPER_DELAY_MS);
  Serial.println("[INFO] Gripper opened");
}

void closeGripper() {
  servoGripper.write(GRIPPER_CLOSE);
  currentAngles[5] = GRIPPER_CLOSE;
  delay(GRIPPER_DELAY_MS);
  Serial.println("[INFO] Gripper closed");
}

// ========================================
// 모든 서보 정지
// ========================================
void stopAllServos() {
  servoBase.write(currentAngles[0]);
  servoShoulder.write(currentAngles[1]);
  servoElbow.write(currentAngles[2]);
  servoWristPitch.write(currentAngles[3]);
  servoWristRoll.write(currentAngles[4]);
  servoGripper.write(currentAngles[5]);

  Serial.println("[WARNING] All servos stopped");
}

// ========================================
// PCB 픽업 동작
// ========================================
bool pickupPCB() {
  Serial.println("[INFO] Starting PCB pickup");

  // 1. 픽업 위치 위로 이동 (Z축 높게)
  Coordinate abovePickup = PICKUP_POSITION;
  abovePickup.elbow += 10;  // 10도 높게
  moveToCoordinate(abovePickup);

  // 2. 그리퍼 열기
  openGripper();

  // 3. 픽업 위치로 하강
  moveToCoordinate(PICKUP_POSITION);

  // 4. 그리퍼 닫기 (PCB 잡기)
  closeGripper();

  // 5. 위로 들어올리기
  moveToCoordinate(abovePickup);

  Serial.println("[INFO] PCB pickup completed");
  return true;
}

// ========================================
// PCB 배치 동작
// ========================================
bool placePCB(const Coordinate &placeCoord) {
  Serial.println("[INFO] Starting PCB placement");

  // 1. 배치 위치 위로 이동 (Z축 높게)
  Coordinate abovePlace = placeCoord;
  abovePlace.elbow += 10;  // 10도 높게
  moveToCoordinate(abovePlace);

  // 2. 배치 위치로 하강
  moveToCoordinate(placeCoord);

  // 3. 그리퍼 열기 (PCB 놓기)
  openGripper();
  delay(300);  // 안정화 대기

  // 4. 위로 들어올리기
  moveToCoordinate(abovePlace);

  // 5. 홈 포지션으로 복귀
  moveToHome();

  Serial.println("[INFO] PCB placement completed");
  return true;
}

// ========================================
// 현재 서보 각도 출력 (디버깅용)
// ========================================
void printCurrentAngles() {
  Serial.print("[DEBUG] Current angles: Base=");
  Serial.print(currentAngles[0]);
  Serial.print(", Shoulder=");
  Serial.print(currentAngles[1]);
  Serial.print(", Elbow=");
  Serial.print(currentAngles[2]);
  Serial.print(", WristPitch=");
  Serial.print(currentAngles[3]);
  Serial.print(", WristRoll=");
  Serial.print(currentAngles[4]);
  Serial.print(", Gripper=");
  Serial.println(currentAngles[5]);
}

#endif // SERVO_CONTROL_H
