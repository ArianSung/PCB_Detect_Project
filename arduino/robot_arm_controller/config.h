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
// 6개 박스 슬롯 좌표 + 폐기 위치
// 3개 카테고리 × 2개 슬롯 (수직 2단) = 6개 슬롯
// 폐기는 슬롯 관리 안 함
// ========================================

// 정상 박스 (NORMAL) - 2개 슬롯 (수직 2단)
const Coordinate NORMAL_SLOTS[2] = {
  {90, 115, 85, 90, 90},   // slot 0 (하단)
  {90, 115, 75, 90, 90}    // slot 1 (상단, elbow -10도)
};

// 부품 불량 박스 (COMPONENT_DEFECT) - 2개 슬롯 (수직 2단)
const Coordinate COMPONENT_DEFECT_SLOTS[2] = {
  {90, 105, 85, 90, 90},   // slot 0 (하단)
  {90, 105, 75, 90, 90}    // slot 1 (상단, elbow -10도)
};

// 납땜 불량 박스 (SOLDER_DEFECT) - 2개 슬롯 (수직 2단)
const Coordinate SOLDER_DEFECT_SLOTS[2] = {
  {90, 95, 85, 90, 90},    // slot 0 (하단)
  {90, 95, 75, 90, 90}     // slot 1 (상단, elbow -10도)
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

#endif // CONFIG_H
