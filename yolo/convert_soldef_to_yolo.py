#!/usr/bin/env python3
"""
SolDef_AI 데이터셋 JSON 라벨을 YOLO 형식으로 변환

LabelMe JSON → YOLO TXT
"""

import json
import os
from pathlib import Path
from tqdm import tqdm
import cv2
import numpy as np
import shutil


class SolDefToYOLO:
    def __init__(self):
        self.project_root = Path("/home/sys1041/work_project")
        self.source_dir = self.project_root / "data" / "raw" / "soldef_ai" / "SolDef_AI" / "Labeled"
        self.output_dir = self.project_root / "data" / "processed" / "soldef_yolo"

        # 클래스 매핑 (JSON 라벨 → YOLO 클래스 ID)
        # 실제 SolDef_AI 데이터셋의 클래스명 (good 제외)
        self.class_mapping = {
            'no_good': 0,       # 일반 불량
            'exc_solder': 1,    # 과다 납땜 (excess solder)
            'spike': 2,         # 스파이크 (돌출)
            'poor_solder': 3,   # 납땜 부족 (insufficient solder)
            # 'good'은 정상이므로 학습에서 제외
        }

        self.classes = list(self.class_mapping.keys())

    def setup_output_dirs(self):
        """출력 디렉토리 생성"""
        print("\n📁 출력 디렉토리 생성 중...")

        for split in ['train', 'val', 'test']:
            (self.output_dir / split / 'images').mkdir(parents=True, exist_ok=True)
            (self.output_dir / split / 'labels').mkdir(parents=True, exist_ok=True)

        print("✅ 디렉토리 생성 완료!")

    def polygon_to_bbox(self, points):
        """
        폴리곤 좌표를 YOLO 바운딩 박스로 변환

        Args:
            points: [[x1, y1], [x2, y2], ...] 형태의 폴리곤 좌표

        Returns:
            (x_center, y_center, width, height) - 정규화된 YOLO 형식
        """
        points = np.array(points)

        # 최소/최대 좌표 찾기
        x_min = points[:, 0].min()
        x_max = points[:, 0].max()
        y_min = points[:, 1].min()
        y_max = points[:, 1].max()

        return x_min, y_min, x_max, y_max

    def convert_json_to_yolo(self, json_path, image_path, output_label_path):
        """
        단일 JSON 파일을 YOLO TXT로 변환

        Args:
            json_path: JSON 라벨 파일 경로
            image_path: 대응하는 이미지 파일 경로
            output_label_path: 출력 TXT 파일 경로
        """
        try:
            # JSON 읽기
            with open(json_path, 'r') as f:
                data = json.load(f)

            # 이미지 크기 가져오기
            img = cv2.imread(str(image_path))
            if img is None:
                print(f"⚠️  이미지를 읽을 수 없음: {image_path}")
                return False

            img_height, img_width = img.shape[:2]

            # YOLO 라벨 생성
            yolo_labels = []

            for shape in data.get('shapes', []):
                label = shape.get('label', '').lower()

                # 클래스 매핑 확인
                if label not in self.class_mapping:
                    # 알 수 없는 클래스는 'no_good'으로 처리
                    class_id = 0
                else:
                    class_id = self.class_mapping[label]

                # 폴리곤 좌표
                points = shape.get('points', [])
                if len(points) < 3:
                    continue  # 폴리곤이 아님

                # 바운딩 박스 계산
                x_min, y_min, x_max, y_max = self.polygon_to_bbox(points)

                # YOLO 형식으로 변환 (정규화)
                x_center = ((x_min + x_max) / 2) / img_width
                y_center = ((y_min + y_max) / 2) / img_height
                width = (x_max - x_min) / img_width
                height = (y_max - y_min) / img_height

                # YOLO 형식: <class_id> <x_center> <y_center> <width> <height>
                yolo_labels.append(f"{class_id} {x_center:.6f} {y_center:.6f} {width:.6f} {height:.6f}")

            # TXT 파일로 저장
            if yolo_labels:
                with open(output_label_path, 'w') as f:
                    f.write('\n'.join(yolo_labels))
                return True
            else:
                # 라벨이 없는 경우 (정상 이미지?)
                # 빈 파일 생성
                output_label_path.touch()
                return True

        except Exception as e:
            print(f"❌ 변환 실패: {json_path.name} - {e}")
            return False

    def split_dataset(self, train_ratio=0.7, val_ratio=0.15):
        """
        데이터셋을 train/val/test로 분할

        Args:
            train_ratio: 학습 데이터 비율
            val_ratio: 검증 데이터 비율
        """
        print("\n📊 데이터셋 분할 중...")

        # 모든 JSON 파일 가져오기
        json_files = list(self.source_dir.glob('*.json'))

        total = len(json_files)
        train_size = int(total * train_ratio)
        val_size = int(total * val_ratio)

        # 랜덤 셔플
        import random
        random.seed(42)
        random.shuffle(json_files)

        # 분할
        train_files = json_files[:train_size]
        val_files = json_files[train_size:train_size + val_size]
        test_files = json_files[train_size + val_size:]

        splits = {
            'train': train_files,
            'val': val_files,
            'test': test_files
        }

        stats = {'train': 0, 'val': 0, 'test': 0}

        # 각 split별로 변환
        for split_name, json_files in splits.items():
            print(f"\n🔄 {split_name} 변환 중...")

            for json_file in tqdm(json_files, desc=f"{split_name}"):
                # 이미지 파일 경로
                image_file = json_file.with_suffix('.jpg')

                if not image_file.exists():
                    print(f"⚠️  이미지 없음: {image_file.name}")
                    continue

                # 출력 경로
                output_image = self.output_dir / split_name / 'images' / image_file.name
                output_label = self.output_dir / split_name / 'labels' / json_file.with_suffix('.txt').name

                # 이미지 복사
                shutil.copy2(image_file, output_image)

                # 라벨 변환
                if self.convert_json_to_yolo(json_file, image_file, output_label):
                    stats[split_name] += 1

        print(f"\n✅ 데이터셋 분할 완료!")
        print(f"   Train: {stats['train']} 이미지")
        print(f"   Val:   {stats['val']} 이미지")
        print(f"   Test:  {stats['test']} 이미지")
        print(f"   Total: {sum(stats.values())} 이미지")

        return stats

    def analyze_classes(self):
        """JSON 파일에서 실제 사용된 클래스 분석"""
        print("\n🔍 클래스 분석 중...")

        class_counts = {}

        for json_file in self.source_dir.glob('*.json'):
            try:
                with open(json_file, 'r') as f:
                    data = json.load(f)

                for shape in data.get('shapes', []):
                    label = shape.get('label', '').lower()
                    class_counts[label] = class_counts.get(label, 0) + 1

            except Exception as e:
                continue

        print("\n발견된 클래스:")
        for label, count in sorted(class_counts.items(), key=lambda x: x[1], reverse=True):
            print(f"  {label:20s}: {count:4d}개")

        return class_counts

    def create_data_yaml(self):
        """data.yaml 생성"""
        print("\n📝 data.yaml 생성 중...")

        import yaml

        data_yaml = {
            'path': str(self.output_dir),
            'train': 'train/images',
            'val': 'val/images',
            'test': 'test/images',
            'nc': len(self.classes),
            'names': {i: name for i, name in enumerate(self.classes)}
        }

        yaml_path = self.output_dir / 'data.yaml'
        with open(yaml_path, 'w') as f:
            yaml.dump(data_yaml, f, default_flow_style=False, sort_keys=False)

        print(f"✅ data.yaml 생성 완료: {yaml_path}")

    def run(self):
        """전체 프로세스 실행"""
        print("="*60)
        print("🚀 SolDef_AI → YOLO 변환 시작")
        print("="*60)

        # 1. 클래스 분석
        class_counts = self.analyze_classes()

        # 2. 출력 디렉토리 생성
        self.setup_output_dirs()

        # 3. 데이터셋 변환 및 분할
        stats = self.split_dataset(train_ratio=0.7, val_ratio=0.15)

        # 4. data.yaml 생성
        self.create_data_yaml()

        # 5. 요약
        print("\n" + "="*60)
        print("✅ SolDef_AI → YOLO 변환 완료!")
        print("="*60)
        print(f"\n출력 경로: {self.output_dir}")
        print(f"클래스 수: {len(self.classes)}")
        print(f"총 이미지: {sum(stats.values())}")

        print("\n다음 단계:")
        print("1. 데이터셋 확인:")
        print(f"   ls {self.output_dir}/train/images/ | head")
        print("2. 샘플 이미지로 테스트:")
        print(f"   yolo detect predict model=yolov8l.pt source={self.output_dir}/test/images/ save=True")
        print()


if __name__ == '__main__':
    converter = SolDefToYOLO()
    converter.run()
