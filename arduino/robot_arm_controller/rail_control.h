/**
 * rail_control.h
 * PCB Defect Detection - Rail Control (Stepper Motor)
 *
 * 로봇팔 플랫폼 레일 이동 제어
 * - NEMA 17 스텝모터 + A4988 드라이버
 * - GT2 타이밍 벨트
 * - 리미트 스위치 2개 (HOME, END)
 */

#ifndef RAIL_CONTROL_H
#define RAIL_CONTROL_H

#include <Arduino.h>
#include "config.h"

// ========================================
// 전역 변수
// ========================================
int currentRailPosition = 0;  // 현재 레일 위치 (스텝 수)
bool railCalibrated = false;  // 레일 캘리브레이션 완료 여부

// ========================================
// 레일 초기화
// ========================================
void initRail() {
  // 핀 모드 설정
  pinMode(RAIL_STEP_PIN, OUTPUT);
  pinMode(RAIL_DIR_PIN, OUTPUT);
  pinMode(RAIL_ENABLE_PIN, OUTPUT);

  pinMode(RAIL_LIMIT_HOME_PIN, INPUT_PULLUP);
  pinMode(RAIL_LIMIT_END_PIN, INPUT_PULLUP);

  // 모터 비활성화 (초기 상태)
  digitalWrite(RAIL_ENABLE_PIN, HIGH);

  Serial.println("[RAIL] Rail system initialized");
}

// ========================================
// 스텝모터 이동 (저수준)
// ========================================
void moveRailSteps(int steps, bool clockwise) {
  // 모터 활성화
  digitalWrite(RAIL_ENABLE_PIN, LOW);

  // 방향 설정
  digitalWrite(RAIL_DIR_PIN, clockwise ? HIGH : LOW);

  // 스텝 이동
  for (int i = 0; i < abs(steps); i++) {
    // 긴급 정지 확인
    if (digitalRead(EMERGENCY_STOP_PIN) == LOW) {
      Serial.println("[RAIL] Emergency stop detected!");
      digitalWrite(RAIL_ENABLE_PIN, HIGH);
      return;
    }

    // 리미트 스위치 확인 (안전)
    if (clockwise && digitalRead(RAIL_LIMIT_END_PIN) == LOW) {
      Serial.println("[RAIL] End limit switch triggered!");
      break;
    }
    if (!clockwise && digitalRead(RAIL_LIMIT_HOME_PIN) == LOW) {
      Serial.println("[RAIL] Home limit switch triggered!");
      break;
    }

    digitalWrite(RAIL_STEP_PIN, HIGH);
    delayMicroseconds(RAIL_SPEED_DELAY_US);
    digitalWrite(RAIL_STEP_PIN, LOW);
    delayMicroseconds(RAIL_SPEED_DELAY_US);
  }

  // 모터 비활성화 (전력 절약)
  digitalWrite(RAIL_ENABLE_PIN, HIGH);
}

// ========================================
// 레일 홈 위치로 이동 (캘리브레이션)
// ========================================
bool homeRail() {
  Serial.println("[RAIL] Homing rail to HOME position...");

  // 이미 홈 위치에 있는지 확인
  if (digitalRead(RAIL_LIMIT_HOME_PIN) == LOW) {
    Serial.println("[RAIL] Already at HOME position");
    currentRailPosition = RAIL_POS_HOME;
    railCalibrated = true;
    return true;
  }

  // 모터 활성화
  digitalWrite(RAIL_ENABLE_PIN, LOW);
  digitalWrite(RAIL_DIR_PIN, LOW);  // 반시계방향 (HOME 방향)

  unsigned long startTime = millis();
  int stepCount = 0;

  // 홈 리미트 스위치까지 이동
  while (digitalRead(RAIL_LIMIT_HOME_PIN) == HIGH) {
    // 타임아웃 확인
    if (millis() - startTime > RAIL_MOVE_TIMEOUT_MS) {
      Serial.println("[RAIL] ERROR: Homing timeout!");
      digitalWrite(RAIL_ENABLE_PIN, HIGH);
      return false;
    }

    // 긴급 정지 확인
    if (digitalRead(EMERGENCY_STOP_PIN) == LOW) {
      Serial.println("[RAIL] Emergency stop during homing!");
      digitalWrite(RAIL_ENABLE_PIN, HIGH);
      return false;
    }

    digitalWrite(RAIL_STEP_PIN, HIGH);
    delayMicroseconds(RAIL_SPEED_DELAY_US);
    digitalWrite(RAIL_STEP_PIN, LOW);
    delayMicroseconds(RAIL_SPEED_DELAY_US);

    stepCount++;
  }

  // 모터 비활성화
  digitalWrite(RAIL_ENABLE_PIN, HIGH);

  // 위치 초기화
  currentRailPosition = RAIL_POS_HOME;
  railCalibrated = true;

  Serial.print("[RAIL] Homing completed (");
  Serial.print(stepCount);
  Serial.println(" steps moved)");

  return true;
}

// ========================================
// 레일 절대 위치로 이동
// ========================================
bool moveRailToPosition(int targetPosition) {
  if (!railCalibrated) {
    Serial.println("[RAIL] ERROR: Rail not calibrated! Run homeRail() first.");
    return false;
  }

  int stepsToMove = targetPosition - currentRailPosition;

  if (stepsToMove == 0) {
    Serial.println("[RAIL] Already at target position");
    return true;
  }

  bool clockwise = (stepsToMove > 0);

  Serial.print("[RAIL] Moving from ");
  Serial.print(currentRailPosition);
  Serial.print(" to ");
  Serial.print(targetPosition);
  Serial.print(" (");
  Serial.print(abs(stepsToMove));
  Serial.println(" steps)");

  // 모터 활성화
  digitalWrite(RAIL_ENABLE_PIN, LOW);
  digitalWrite(RAIL_DIR_PIN, clockwise ? HIGH : LOW);

  unsigned long startTime = millis();

  for (int i = 0; i < abs(stepsToMove); i++) {
    // 타임아웃 확인
    if (millis() - startTime > RAIL_MOVE_TIMEOUT_MS) {
      Serial.println("[RAIL] ERROR: Move timeout!");
      digitalWrite(RAIL_ENABLE_PIN, HIGH);
      return false;
    }

    // 긴급 정지 확인
    if (digitalRead(EMERGENCY_STOP_PIN) == LOW) {
      Serial.println("[RAIL] Emergency stop during move!");
      digitalWrite(RAIL_ENABLE_PIN, HIGH);
      return false;
    }

    // 리미트 스위치 안전 체크
    if (clockwise && digitalRead(RAIL_LIMIT_END_PIN) == LOW) {
      Serial.println("[RAIL] ERROR: End limit reached!");
      digitalWrite(RAIL_ENABLE_PIN, HIGH);
      return false;
    }
    if (!clockwise && digitalRead(RAIL_LIMIT_HOME_PIN) == LOW) {
      Serial.println("[RAIL] ERROR: Home limit reached!");
      digitalWrite(RAIL_ENABLE_PIN, HIGH);
      return false;
    }

    digitalWrite(RAIL_STEP_PIN, HIGH);
    delayMicroseconds(RAIL_SPEED_DELAY_US);
    digitalWrite(RAIL_STEP_PIN, LOW);
    delayMicroseconds(RAIL_SPEED_DELAY_US);
  }

  // 모터 비활성화
  digitalWrite(RAIL_ENABLE_PIN, HIGH);

  // 위치 업데이트
  currentRailPosition = targetPosition;

  Serial.println("[RAIL] Move completed");
  return true;
}

// ========================================
// 레일 이름으로 이동 (편의 함수)
// ========================================
bool moveRailTo(const char* positionName) {
  int targetPosition;

  if (strcmp(positionName, "home") == 0) {
    targetPosition = RAIL_POS_HOME;
  } else if (strcmp(positionName, "box1") == 0) {
    targetPosition = RAIL_POS_BOX1;
  } else if (strcmp(positionName, "box2") == 0) {
    targetPosition = RAIL_POS_BOX2;
  } else if (strcmp(positionName, "box3") == 0) {
    targetPosition = RAIL_POS_BOX3;
  } else {
    Serial.print("[RAIL] ERROR: Unknown position '");
    Serial.print(positionName);
    Serial.println("'");
    return false;
  }

  return moveRailToPosition(targetPosition);
}

// ========================================
// 레일 상태 조회
// ========================================
void getRailStatus(int& position, bool& calibrated, bool& homeSwitch, bool& endSwitch) {
  position = currentRailPosition;
  calibrated = railCalibrated;
  homeSwitch = (digitalRead(RAIL_LIMIT_HOME_PIN) == LOW);
  endSwitch = (digitalRead(RAIL_LIMIT_END_PIN) == LOW);
}

#endif // RAIL_CONTROL_H
