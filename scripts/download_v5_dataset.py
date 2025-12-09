#!/usr/bin/env python3
"""
Roboflow에서 PCB Defect Detection v5 데이터셋 다운로드
"""

from roboflow import Roboflow
import os

def main():
    # Roboflow API 초기화
    rf = Roboflow(api_key="4EdUhVZ6LgtN2FvlVWNW")

    # 프로젝트 및 버전 선택
    project = rf.workspace("arian-qfo7y").project("pcb_defect_detect-nitz0")
    version = project.version(5)

    # YOLOv11 형식으로 다운로드
    print("Downloading PCB Defect Detection v5 dataset...")
    dataset = version.download("yolov11")

    print(f"\nDataset downloaded successfully!")
    print(f"Location: {dataset.location}")

    # data.yaml 경로 출력
    data_yaml = os.path.join(dataset.location, "data.yaml")
    print(f"data.yaml path: {data_yaml}")

    return dataset.location

if __name__ == "__main__":
    dataset_path = main()
