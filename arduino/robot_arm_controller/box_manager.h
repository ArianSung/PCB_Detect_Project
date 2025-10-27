/**
 * box_manager.h
 * PCB Defect Detection - Box Manager
 *
 * 박스 ID를 좌표로 매핑하는 함수
 */

#ifndef BOX_MANAGER_H
#define BOX_MANAGER_H

#include "config.h"

// ========================================
// 박스 ID 열거형
// ========================================
enum BoxID {
  NORMAL,
  COMPONENT_DEFECT,
  SOLDER_DEFECT,
  DISCARD,
  UNKNOWN_BOX
};

// ========================================
// 문자열을 BoxID로 변환
// ========================================
BoxID stringToBoxID(const char* boxIdStr) {
  if (strcmp(boxIdStr, "NORMAL") == 0) {
    return NORMAL;
  } else if (strcmp(boxIdStr, "COMPONENT_DEFECT") == 0) {
    return COMPONENT_DEFECT;
  } else if (strcmp(boxIdStr, "SOLDER_DEFECT") == 0) {
    return SOLDER_DEFECT;
  } else if (strcmp(boxIdStr, "DISCARD") == 0) {
    return DISCARD;
  } else {
    return UNKNOWN_BOX;
  }
}

// ========================================
// BoxID를 문자열로 변환
// ========================================
const char* boxIDToString(BoxID boxId) {
  switch (boxId) {
    case NORMAL:            return "NORMAL";
    case COMPONENT_DEFECT:  return "COMPONENT_DEFECT";
    case SOLDER_DEFECT:     return "SOLDER_DEFECT";
    case DISCARD:           return "DISCARD";
    default:                return "UNKNOWN";
  }
}

// ========================================
// 박스 ID와 슬롯 번호로 좌표 가져오기
// ========================================
bool getBoxCoordinate(const char* boxIdStr, int slotIndex, Coordinate &outCoord) {
  BoxID boxId = stringToBoxID(boxIdStr);

  // 폐기는 슬롯 인덱스 무시하고 폐기 위치로
  if (boxId == DISCARD) {
    outCoord = DISCARD_POSITION;
    Serial.println("[INFO] Retrieved DISCARD position (no slot management)");
    return true;
  }

  // 슬롯 인덱스 유효성 검사 (0-1)
  if (slotIndex < 0 || slotIndex >= 2) {
    Serial.print("[ERROR] Invalid slot index: ");
    Serial.print(slotIndex);
    Serial.println(" (valid range: 0-1)");
    return false;
  }

  // 박스 ID에 따라 좌표 배열 선택
  const Coordinate* coordArray = nullptr;

  switch (boxId) {
    case NORMAL:
      coordArray = NORMAL_SLOTS;
      break;
    case COMPONENT_DEFECT:
      coordArray = COMPONENT_DEFECT_SLOTS;
      break;
    case SOLDER_DEFECT:
      coordArray = SOLDER_DEFECT_SLOTS;
      break;
    default:
      Serial.print("[ERROR] Unknown box ID: ");
      Serial.println(boxIdStr);
      return false;
  }

  // 좌표 복사
  outCoord = coordArray[slotIndex];

  // 캘리브레이션 오프셋 적용
  int boxIndex = (int)boxId;
  if (boxIndex >= 0 && boxIndex < 4) {
    outCoord.base += CALIBRATION_OFFSETS[boxIndex].base;
    outCoord.shoulder += CALIBRATION_OFFSETS[boxIndex].shoulder;
    outCoord.elbow += CALIBRATION_OFFSETS[boxIndex].elbow;
    outCoord.wrist_pitch += CALIBRATION_OFFSETS[boxIndex].wrist_pitch;
    outCoord.wrist_roll += CALIBRATION_OFFSETS[boxIndex].wrist_roll;
  }

  Serial.print("[INFO] Retrieved coordinate for ");
  Serial.print(boxIdStr);
  Serial.print(" slot ");
  Serial.println(slotIndex);

  return true;
}

// ========================================
// 박스 ID 유효성 검사
// ========================================
bool isValidBoxID(const char* boxIdStr) {
  return stringToBoxID(boxIdStr) != UNKNOWN_BOX;
}

// ========================================
// 박스 카테고리 가져오기
// ========================================
const char* getBoxCategory(const char* boxIdStr) {
  BoxID boxId = stringToBoxID(boxIdStr);

  switch (boxId) {
    case NORMAL:
      return "NORMAL";
    case COMPONENT_DEFECT:
      return "COMPONENT_DEFECT";
    case SOLDER_DEFECT:
      return "SOLDER_DEFECT";
    case DISCARD:
      return "DISCARD";
    default:
      return "UNKNOWN";
  }
}

// ========================================
// 디버깅: 모든 박스 좌표 출력
// ========================================
void printAllBoxCoordinates() {
  Serial.println("[DEBUG] All box coordinates:");

  const char* boxNames[3] = {
    "NORMAL",
    "COMPONENT_DEFECT",
    "SOLDER_DEFECT"
  };

  // 슬롯 관리되는 3개 박스
  for (int i = 0; i < 3; i++) {
    Serial.print("  ");
    Serial.print(boxNames[i]);
    Serial.println(":");

    for (int slot = 0; slot < 2; slot++) {
      Coordinate coord;
      if (getBoxCoordinate(boxNames[i], slot, coord)) {
        Serial.print("    Slot ");
        Serial.print(slot);
        Serial.print(": Base=");
        Serial.print(coord.base);
        Serial.print(", Shoulder=");
        Serial.print(coord.shoulder);
        Serial.print(", Elbow=");
        Serial.print(coord.elbow);
        Serial.print(", WristPitch=");
        Serial.print(coord.wrist_pitch);
        Serial.print(", WristRoll=");
        Serial.println(coord.wrist_roll);
      }
    }
  }

  // 폐기 위치 (슬롯 관리 안 함)
  Serial.println("  DISCARD (no slot management):");
  Coordinate discardCoord;
  if (getBoxCoordinate("DISCARD", 0, discardCoord)) {
    Serial.print("    Position: Base=");
    Serial.print(discardCoord.base);
    Serial.print(", Shoulder=");
    Serial.print(discardCoord.shoulder);
    Serial.print(", Elbow=");
    Serial.print(discardCoord.elbow);
    Serial.print(", WristPitch=");
    Serial.print(discardCoord.wrist_pitch);
    Serial.print(", WristRoll=");
    Serial.println(discardCoord.wrist_roll);
  }
}

#endif // BOX_MANAGER_H
