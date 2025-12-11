#include <Servo.h>

// ==================================================================
// 1. 하드웨어 및 핀 설정
// ==================================================================
Servo servos[6];
int servoPins[6] = {2, 3, 4, 5, 6, 7};
int currentAngles[6];

// [속도 설정 1] 로봇팔 속도 (클수록 느림)
int speedDelay = 30;

// [속도 설정 2] 스텝 모터
#define STEPPER_DIR_PIN 8
#define STEPPER_STEP_PIN 9
#define STEPS_PER_REV 100
#define STEP_DELAY_US 2500

// [속도 설정 3] 컨베이어 벨트
#define CONVEYOR_PIN 11
int conveyorSpeed = 60;

// [위치 정의] 스마트 모드용 스텝모터 이동 거리
const int STEPS_POS_NORMAL = 0;     
const int STEPS_POS_FRONT  = 400;   
const int STEPS_POS_SERIAL = 830;   
const int STEPS_POS_TRASH  = 1200;  

// [위치 정의] 수동 모드용 거리
const int STEPS_MANUAL_GOOD = 0;
const int STEPS_MANUAL_MINOR = 400;

// ==================================================================
// 2. 상태 변수
// ==================================================================
int cntNormal = 0; bool fullNormal = false;
int cntFront  = 0; bool fullFront  = false;
int cntSerial = 0; bool fullSerial = false;
bool isConveyorOn = false;

// ==================================================================
// 3. 로봇 팔 각도 데이터 (배열 크기 11로 증가)
// ==================================================================
// 중간에 [2]번 상태가 추가되어, 기존 [2]번 이후는 모두 +1 되었습니다.
int stateAngles[11][6] = {
    {0, 20, 65, 0, 0, 45},       // [0] Home
    {0, 20, 65, 0, 0, 90},       // [1] Grab
    {0, 20, 130, 0, 0, 90},      // [2] Up (새로 추가됨!)
    {180, 150, 0, 80, 100, 90},  // [3] Wait (기존 [2])
    {180, 80, 20, 105, 100, 90}, // [4] 1F Go (기존 [3])
    {180, 80, 20, 105, 100, 60}, // [5] 1F Put (기존 [4])
    {180, 70, 37, 105, 100, 90}, // [6] 2F Go (기존 [5])
    {180, 70, 37, 105, 100, 60}, // [7] 2F Put (기존 [6])
    {180, 55, 48, 95, 100, 90},  // [8] 3F Go (기존 [7])
    {180, 55, 48, 95, 100, 60},  // [9] 3F Put (기존 [8])
    {0, 90, 0, 0, 0, 60}         // [10] Safe (기존 [9])
};

// ==================================================================
// 함수 선언
// ==================================================================
void conveyorControl(bool turnOn);
void moveToState(int stateIndex); 
void runSequence(int moveIdx, int putIdx, int stepSteps); // 전방 선언

void setup() {
    Serial.begin(9600);
    
    pinMode(CONVEYOR_PIN, OUTPUT);
    digitalWrite(CONVEYOR_PIN, HIGH);
    isConveyorOn = false;

    pinMode(STEPPER_STEP_PIN, OUTPUT);
    pinMode(STEPPER_DIR_PIN, OUTPUT);

    // [초기화] 쓰러짐 방지 로직
    for(int i=0; i<6; i++) {
        currentAngles[i] = stateAngles[0][i]; 
    }

    for (int i = 0; i < 6; i++) {
        servos[i].write(currentAngles[i]);
        if (i == 2) servos[i].attach(servoPins[i], 530, 2400);
        else servos[i].attach(servoPins[i]);
    }
    
    moveToState(0); // Home 위치 고정
    
    printMenu();

    Serial.println("System Ready. Starting Conveyor...");
    conveyorControl(true);
}

void printMenu() {
    Serial.println("=========== [Hybrid Control System] ===========");
    Serial.println("N/F/S: Auto Sort | D: Discard | R: Reset");
    Serial.println("Manual: 1~9, 0(Safe), q(Safe)");
    Serial.println("===============================================");
}

// ==================================================================
// 4. 메인 루프
// ==================================================================
void loop() {
    if (Serial.available() > 0) {
        char cmd = Serial.read();
        if(cmd == '\n' || cmd == '\r') return; 

        // [A] 스마트 로직
        if      (cmd == 'N') handleStacking(cntNormal, fullNormal, STEPS_POS_NORMAL, "Normal");
        else if (cmd == 'F') handleStacking(cntFront, fullFront, STEPS_POS_FRONT, "FrontBad");
        else if (cmd == 'S') handleStacking(cntSerial, fullSerial, STEPS_POS_SERIAL, "SerialBad");
        else if (cmd == 'D') { Serial.println("Auto: Discarding"); discardItem(STEPS_POS_TRASH); }
        else if (cmd == 'R') resetAllCounts();
        
        // --- 컨베이어 제어 ---
        else if (cmd == 'C') toggleConveyor(); // 토글 (멈춤/재개)
        else if (cmd == '[') conveyorControl(true);  
        else if (cmd == ']') conveyorControl(false); 
        else if (cmd == 'R') conveyorControl(true); // 'R'키: 컨베이어 재시작 (요청하신 기능)

        // [B] 수동 테스트 (인덱스 변경 반영)
        else if (cmd == '1') moveToState(0);
        else if (cmd == '2') moveToState(1);
        else if (cmd == '3') moveToState(2); // New: Up
        else if (cmd == '4') moveToState(3); // New: Wait
        else if (cmd == '5') moveToState(4);
        else if (cmd == '6') moveToState(5);
        else if (cmd == '7') moveToState(6);
        else if (cmd == '8') moveToState(7);
        else if (cmd == '9') moveToState(8);
        else if (cmd == '0') moveToState(9); // 3F Put
        else if (cmd == 'q') moveToState(10); // Safe
        else if (cmd == 'm') printMenu();
        else if (cmd == 's') runStepperBySteps(HIGH, 100);
        else if (cmd == 'x') runStepperBySteps(LOW, 100);
    }
}

// ==================================================================
// 5. 함수 구현부
// ==================================================================

void conveyorControl(bool turnOn) {
    if (turnOn) {
        int pwmValue = 255 - conveyorSpeed; 
        analogWrite(CONVEYOR_PIN, pwmValue);
        isConveyorOn = true;
        Serial.println("STATUS: Conveyor ON");
    } else {
        digitalWrite(CONVEYOR_PIN, HIGH);
        isConveyorOn = false;
        Serial.println("STATUS: Conveyor OFF");
    }
}

void toggleConveyor() {
    conveyorControl(!isConveyorOn);
}

// ==============================================================
// [수정됨] 적재 관리 함수 (인덱스 +1 반영)
// ==============================================================
void handleStacking(int &count, bool &isFull, int stepPos, String label) {
    conveyorControl(false);
    delay(500); 

    if (isFull) {
        Serial.print("WARNING: "); Serial.println("Full.");
        return; 
    }
    
    count++;
    Serial.print("Auto: "); Serial.print(label); Serial.print(" #"); Serial.println(count);

    int moveState, putState;
    
    // [인덱스 수정] 기존 번호에서 +1씩 밀림
    // 1층: 기존 3,4 -> 4,5
    // 2층: 기존 5,6 -> 6,7
    // 3층: 기존 7,8 -> 8,9
    if (count == 1)      { moveState = 4; putState = 5; }
    else if (count == 2) { moveState = 6; putState = 7; }
    else if (count == 3) { moveState = 8; putState = 9; }
    
    runSequence(moveState, putState, stepPos);

    if (count >= 3) {
        isFull = true;
        Serial.println("EVENT: FULL");
    } else {
        Serial.println("Restarting Conveyor...");
        conveyorControl(true);
    }
}

// ==============================================================
// [수정됨] 버리기 함수 (인덱스 +1 반영)
// ==============================================================
void discardItem(int stepPos) {
    conveyorControl(false);
    
    // 1. 집기 과정
    moveToState(0); // Home
    moveToState(1); // Grab
    moveToState(2); // Up (새로 추가된 동작 자연스럽게 수행)
    
    // 2. 이동
    moveToState(3); // Wait (기존 2번이 3번이 됨)
    runStepperBySteps(HIGH, stepPos);
    delay(10000);
    // 3. 버리기 동작 (1층 놓기 모션 활용)
    moveToState(4); // 1F Go (기존 3->4)
    delay(200);
    moveToState(5); // 1F Put (기존 4->5)
    delay(500);
    
    // 4. 복귀
    moveToState(3); // Wait (기존 2->3)
    runStepperBySteps(LOW, stepPos);
    moveToState(2);
    moveToState(1);
    moveToState(0);

    Serial.println("Discard Done.");
    conveyorControl(true);
}

void resetAllCounts() {
    cntNormal = 0; fullNormal = false;
    cntFront = 0;  fullFront = false;
    cntSerial = 0; fullSerial = false;
    Serial.println("RESET COMPLETE");
    conveyorControl(true);
}

// ==================================================================
// 하드웨어 제어 함수들
// ==================================================================

// [수정됨] 공통 시퀀스 (인덱스 +1 반영)
void runSequence(int moveIdx, int putIdx, int stepSteps) {
    moveToState(0); // Home
    moveToState(1); // Grab
    moveToState(2); // [New] Up (들고 나서 살짝 들어올림)
    moveToState(3); // Wait (기존 2->3번으로 변경)
    
    runStepperBySteps(HIGH, stepSteps); // 회전

    moveToState(moveIdx); delay(800); // 층 이동
    moveToState(putIdx);  delay(800); // 놓기
    moveToState(moveIdx); // 들기
    
    moveToState(3); // Wait (기존 2->3번으로 변경)
    runStepperBySteps(LOW, stepSteps); // 회전 복귀
    moveToState(2);
    moveToState(1);
    moveToState(0); // 완료
}

void runOldSequence(int moveIdx, int putIdx, int stepSteps) {
    // 수동 테스트용 래퍼 함수는 생략하거나 필요시 위 로직과 동일하게 인덱스 수정 필요
    // 현재 코드에서는 사용 빈도가 낮아보여 제거하지 않고 둠
    runSequence(moveIdx, putIdx, stepSteps);
    printMenu();
}

void moveToState(int stateIndex) {
    int targetAngles[6];
    for (int i = 0; i < 6; i++) targetAngles[i] = stateAngles[stateIndex][i];

    int priorityMotor = 3; 
    bool done = false;

    while (!done) {
        done = true;
        for (int i = 0; i < 6; i++) {
            int step = (i == priorityMotor) ? 2 : 1; 
            if (currentAngles[i] != targetAngles[i]) {
                done = false;
                if (currentAngles[i] < targetAngles[i]) 
                    currentAngles[i] += min(targetAngles[i] - currentAngles[i], step);
                else 
                    currentAngles[i] -= min(currentAngles[i] - targetAngles[i], step);
                servos[i].write(currentAngles[i]);
            }
        }
        delay(speedDelay);
    }
}

void runStepperBySteps(int direction, int steps) {
    if (steps <= 0) return;
    digitalWrite(STEPPER_DIR_PIN, direction);
    for (int x = 0; x < steps; x++) {
        digitalWrite(STEPPER_STEP_PIN, HIGH);
        delayMicroseconds(STEP_DELAY_US);
        digitalWrite(STEPPER_STEP_PIN, LOW);
        delayMicroseconds(STEP_DELAY_US);
    }
}