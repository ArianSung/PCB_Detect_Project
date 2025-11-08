#!/usr/bin/env python3
"""
균형 데이터셋으로 YOLOv11l 학습 (로그 저장 포함)
실시간 로그 출력 + 파일 저장 + 구조화된 로깅
"""

import os
import sys
import subprocess
import logging
from datetime import datetime
from pathlib import Path

# 프로젝트 루트
PROJECT_ROOT = Path(__file__).parent.parent
DATA_YAML = PROJECT_ROOT / "data" / "processed" / "roboflow_pcb_balanced" / "data.yaml"
LOG_DIR = PROJECT_ROOT / "logs"

class TeeLogger:
    """실시간 출력 + 파일 저장을 동시에 하는 로거"""

    def __init__(self, log_file):
        self.terminal = sys.stdout
        self.log = open(log_file, 'w', encoding='utf-8')

    def write(self, message):
        self.terminal.write(message)
        self.log.write(message)
        self.log.flush()  # 즉시 파일에 쓰기

    def flush(self):
        self.terminal.flush()
        self.log.flush()

    def close(self):
        self.log.close()

def setup_logging(log_file):
    """로깅 설정"""
    LOG_DIR.mkdir(exist_ok=True)

    # 파일과 콘솔 모두에 로그 출력
    logging.basicConfig(
        level=logging.INFO,
        format='%(asctime)s [%(levelname)s] %(message)s',
        datefmt='%Y-%m-%d %H:%M:%S',
        handlers=[
            logging.FileHandler(log_file, encoding='utf-8'),
            logging.StreamHandler(sys.stdout)
        ]
    )

    return logging.getLogger(__name__)

def train_with_logging():
    """로깅과 함께 학습 실행"""

    # 타임스탬프
    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    log_file = LOG_DIR / f"training_balanced_{timestamp}.log"

    # 로거 설정
    logger = setup_logging(log_file)

    logger.info("=" * 80)
    logger.info("균형 데이터셋으로 YOLOv11l 학습 시작")
    logger.info("=" * 80)
    logger.info(f"데이터셋: {DATA_YAML}")
    logger.info(f"로그 파일: {log_file}")
    logger.info("=" * 80)

    # 데이터셋 존재 확인
    if not DATA_YAML.exists():
        logger.error(f"데이터셋을 찾을 수 없습니다: {DATA_YAML}")
        logger.error("먼저 'python scripts/balance_dataset.py'를 실행하세요.")
        return False

    logger.info("\n학습 설정:")
    logger.info("  - 배치 크기: 16")
    logger.info("  - 에폭: 100")
    logger.info("  - 이미지 크기: 640")
    logger.info("  - 옵티마이저: AdamW")
    logger.info("  - 학습률: 0.001 → 0.00001")
    logger.info("  - Early Stopping: patience=50")
    logger.info("  - Mixed Precision: True")
    logger.info("")
    logger.info("데이터 증강:")
    logger.info("  - MixUp: 0.15")
    logger.info("  - Copy-Paste: 0.1")
    logger.info("  - Mosaic: 1.0")
    logger.info("  - 좌우 반전: 0.5")
    logger.info("  - 회전: ±15도")
    logger.info("")

    # YOLO 학습 명령어
    cmd = [
        "yolo", "detect", "train",
        f"data={DATA_YAML}",
        "model=yolo11l.pt",
        "epochs=100",
        "batch=16",
        "imgsz=640",
        "device=0",
        "project=runs/detect",
        "name=roboflow_pcb_balanced",
        "exist_ok=True",
        "pretrained=True",
        "optimizer=AdamW",
        "lr0=0.001",
        "lrf=0.01",
        "weight_decay=0.0005",
        "patience=50",
        "amp=True",
        "verbose=True",
        "plots=True",
        "cache=False",
        "workers=8",
        # 데이터 증강
        "hsv_h=0.015",
        "hsv_s=0.7",
        "hsv_v=0.4",
        "degrees=15",
        "translate=0.1",
        "scale=0.5",
        "shear=0.0",
        "perspective=0.0",
        "flipud=0.0",
        "fliplr=0.5",
        "mosaic=1.0",
        "mixup=0.15",
        "copy_paste=0.1",
    ]

    logger.info("YOLO 학습 시작...\n")

    # 프로세스 실행 (실시간 출력)
    try:
        # TeeLogger로 stdout 리다이렉트
        yolo_log_file = LOG_DIR / f"yolo_output_{timestamp}.log"
        tee = TeeLogger(yolo_log_file)

        process = subprocess.Popen(
            cmd,
            stdout=subprocess.PIPE,
            stderr=subprocess.STDOUT,
            text=True,
            bufsize=1,
            universal_newlines=True
        )

        # 실시간 출력
        for line in process.stdout:
            tee.write(line)

        # 프로세스 종료 대기
        return_code = process.wait()
        tee.close()

        if return_code == 0:
            logger.info("\n" + "=" * 80)
            logger.info("✓ 학습 성공적으로 완료!")
            logger.info("=" * 80)
            logger.info(f"결과 저장 위치: runs/detect/roboflow_pcb_balanced")
            logger.info(f"YOLO 출력 로그: {yolo_log_file}")
            logger.info(f"전체 로그: {log_file}")
            logger.info("")
            logger.info("결과 확인 방법:")
            logger.info(f"  - 학습 로그: cat {log_file}")
            logger.info(f"  - YOLO 출력: cat {yolo_log_file}")
            logger.info(f"  - 결과 그래프: runs/detect/roboflow_pcb_balanced/results.png")
            logger.info(f"  - 최고 모델: runs/detect/roboflow_pcb_balanced/weights/best.pt")
            logger.info("=" * 80)
            return True
        else:
            logger.error("\n" + "=" * 80)
            logger.error(f"❌ 학습 실패 (return code: {return_code})")
            logger.error("=" * 80)
            logger.error(f"로그 확인: cat {yolo_log_file}")
            return False

    except KeyboardInterrupt:
        logger.warning("\n사용자에 의해 학습이 중단되었습니다.")
        return False
    except Exception as e:
        logger.error(f"\n오류 발생: {e}")
        return False

def main():
    success = train_with_logging()
    sys.exit(0 if success else 1)

if __name__ == "__main__":
    main()
