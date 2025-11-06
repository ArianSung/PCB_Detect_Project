#!/usr/bin/env python3
"""
Roboflow에서 PCB 부품 데이터셋 다운로드

사용법:
    python download_roboflow_dataset.py --api-key YOUR_KEY --workspace WORKSPACE --project PROJECT --version 1
"""

import argparse
from roboflow import Roboflow

def download_dataset(api_key, workspace, project, version=1, format="yolov8"):
    """
    Roboflow에서 데이터셋 다운로드

    Args:
        api_key: Roboflow API 키
        workspace: 워크스페이스 이름
        project: 프로젝트 이름
        version: 버전 번호
        format: 데이터셋 포맷 (yolov8, yolov5, coco, etc.)
    """
    # Roboflow 초기화
    rf = Roboflow(api_key=api_key)

    # 프로젝트 가져오기
    project_obj = rf.workspace(workspace).project(project)

    # 버전 선택
    dataset = project_obj.version(version)

    # 다운로드
    print(f"Downloading {workspace}/{project} v{version}...")
    dataset.download(format)

    print(f"✅ Download complete: {project}-{version}")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Download PCB dataset from Roboflow")
    parser.add_argument("--api-key", required=True, help="Roboflow API key")
    parser.add_argument("--workspace", default="research-pbbdl",
                       help="Workspace name (default: research-pbbdl)")
    parser.add_argument("--project", default="pcb-component-detection-dre7a",
                       help="Project name (default: pcb-component-detection-dre7a)")
    parser.add_argument("--version", type=int, default=1, help="Dataset version")
    parser.add_argument("--format", default="yolov8", help="Dataset format")

    args = parser.parse_args()

    download_dataset(args.api_key, args.workspace, args.project, args.version, args.format)
