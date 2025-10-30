/**
 * config.h
 * PCB Defect Detection - Robot Arm Configuration
 *
 * 핀 정의 및 40개 박스 슬롯 좌표 테이블
 */

#ifndef CONFIG_H
#define CONFIG_H

// ========================================
// 서보 모터 핀 정의 (PWM 핀)
// ========================================
#define SERVO_BASE_PIN        2    // 베이스 회전
#define SERVO_SHOULDER_PIN    3    // 어깨
#define SERVO_ELBOW_PIN       4    // 팔꿈치
#define SERVO_WRIST_PITCH_PIN 5    // 손목 피치
#define SERVO_WRIST_ROLL_PIN  6    // 손목 롤
#define SERVO_GRIPPER_PIN     7    // 그리퍼

// ========================================
// 레일 시스템 핀 정의 (스텝모터 + 리미트 스위치) ⭐ 신규
// ========================================
#define RAIL_STEP_PIN         10   // 스텝모터 STEP 신호
#define RAIL_DIR_PIN          11   // 스텝모터 DIR 신호
#define RAIL_ENABLE_PIN       12   // 스텝모터 ENABLE 신호

#define RAIL_LIMIT_HOME_PIN   14   // 홈 위치 리미트 스위치 (풀업)
#define RAIL_LIMIT_END_PIN    15   // 끝 위치 리미트 스위치 (풀업)

// ========================================
// 안전 설정
// ========================================
#define EMERGENCY_STOP_PIN    8    // 비상 정지 버튼 (풀업)
#define LED_STATUS_PIN        13   // 상태 표시 LED (내장 LED)

// ========================================
// 서보 각도 제한
// ========================================
#define SERVO_MIN_ANGLE       0
#define SERVO_MAX_ANGLE       180

// 홈 포지션 (초기 위치)
#define HOME_BASE             90
#define HOME_SHOULDER         90
#define HOME_ELBOW            90
#define HOME_WRIST_PITCH      90
#define HOME_WRIST_ROLL       90
#define HOME_GRIPPER          0    // 열림

// 그리퍼 각도
#define GRIPPER_OPEN          0    // 완전히 열림
#define GRIPPER_CLOSE         75   // PCB 잡기

// ========================================
// 동작 설정
// ========================================
#define SERVO_SPEED           50   // 서보 속도 (0-100, 낮을수록 느림)
#define MOVE_DELAY_MS         20   // 서보 이동 간 딜레이 (ms)
#define GRIPPER_DELAY_MS      500  // 그리퍼 동작 후 대기 시간

// ========================================
// 시리얼 통신 설정
// ========================================
#define SERIAL_BAUDRATE       115200
#define SERIAL_TIMEOUT_MS     5000
#define JSON_BUFFER_SIZE      512

// ========================================
// 좌표 구조체
// ========================================
struct Coordinate {
  int base;         // 베이스 회전 각도 (0-180)
  int shoulder;     // 어깨 각도 (0-180)
  int elbow;        // 팔꿈치 각도 (0-180)
  int wrist_pitch;  // 손목 피치 각도 (0-180)
  int wrist_roll;   // 손목 롤 각도 (0-180)
};

// ========================================
// 픽업 위치 (컨베이어 벨트)
// ========================================
const Coordinate PICKUP_POSITION = {90, 120, 80, 90, 90};

// ========================================
// 15개 박스 슬롯 좌표 + 폐기 위치
// 3개 카테고리 × 5개 슬롯 (수평 배치) = 15개 슬롯
// 폐기는 슬롯 관리 안 함
// ========================================

// 정상 박스 (NORMAL) - 5개 슬롯 (좌측 → 우측)
const Coordinate NORMAL_SLOTS[5] = {
  {88, 115, 85, 90, 90},   // slot 0
  {92, 115, 84, 90, 90},   // slot 1
  {96, 115, 83, 90, 90},   // slot 2
  {100, 115, 82, 90, 90},  // slot 3
  {104, 115, 81, 90, 90}   // slot 4
};

// 부품 불량 박스 (COMPONENT_DEFECT) - 5개 슬롯 (좌측 → 우측)
const Coordinate COMPONENT_DEFECT_SLOTS[5] = {
  {88, 105, 86, 90, 90},   // slot 0
  {92, 105, 85, 90, 90},   // slot 1
  {96, 105, 84, 90, 90},   // slot 2
  {100, 105, 83, 90, 90},  // slot 3
  {104, 105, 82, 90, 90}   // slot 4
};

// 납땜 불량 박스 (SOLDER_DEFECT) - 5개 슬롯 (좌측 → 우측)
const Coordinate SOLDER_DEFECT_SLOTS[5] = {
  {88, 95, 87, 90, 90},    // slot 0
  {92, 95, 86, 90, 90},    // slot 1
  {96, 95, 85, 90, 90},    // slot 2
  {100, 95, 84, 90, 90},   // slot 3
  {104, 95, 83, 90, 90}    // slot 4
};

// 폐기 위치 (슬롯 관리 안 함, 그냥 떨어뜨림)
const Coordinate DISCARD_POSITION = {90, 85, 70, 90, 90};  // 높이에서 떨어뜨리기

// ========================================
// 캘리브레이션 오프셋 (실제 테스트 후 조정)
// ========================================
struct CalibrationOffset {
  int base;
  int shoulder;
  int elbow;
  int wrist_pitch;
  int wrist_roll;
};

// 각 박스별 캘리브레이션 오프셋 (초기값: 모두 0)
CalibrationOffset CALIBRATION_OFFSETS[4] = {
  {0, 0, 0, 0, 0},  // NORMAL
  {0, 0, 0, 0, 0},  // COMPONENT_DEFECT
  {0, 0, 0, 0, 0},  // SOLDER_DEFECT
  {0, 0, 0, 0, 0}   // DISCARD
};

// ========================================
// 레일 시스템 설정 ⭐ 신규
// ========================================
#define STEPS_PER_REV         200    // NEMA 17 스텝모터 (1.8도)
#define MICROSTEPS            16     // A4988 마이크로스테핑 (1/16)
#define STEPS_PER_MM          10     // GT2 벨트 기준 (실측 후 조정 필요)

#define RAIL_SPEED_DELAY_US   1000   // 스텝 간 딜레이 (마이크로초, 1ms = 느린 속도)

// 레일 위치 (스텝 수)
#define RAIL_POS_HOME         0      // 홈 위치
#define RAIL_POS_BOX1         1500   // 박스1 위치 (약 150mm, 조정 필요)
#define RAIL_POS_BOX2         3000   // 박스2 위치 (약 300mm, 조정 필요)
#define RAIL_POS_BOX3         4500   // 박스3 위치 (약 450mm, 조정 필요)

// 레일 타임아웃 (밀리초)
#define RAIL_MOVE_TIMEOUT_MS  30000  // 30초

#endif // CONFIG_H
