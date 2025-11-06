#!/usr/bin/env python3
"""
Complete PCB Model 최종 데이터셋 분석

모든 32개 클래스의 이름, 설명, 개수를 출력
"""

from pathlib import Path
from collections import Counter


def main():
    project_root = Path("/home/sys1041/work_project")
    dataset_dir = project_root / "data" / "processed" / "complete_pcb_model"

    # 32개 클래스 정의 (ID: (이름, 설명))
    class_info = {
        # DeepPCB 결함 클래스 (0-5)
        0: ('def_open', 'PCB 오픈 결함 (연결 끊김)'),
        1: ('def_short', 'PCB 단락 결함'),
        2: ('def_mousebite', 'PCB 마우스바이트 결함'),
        3: ('def_spur', 'PCB 스퍼 결함 (돌출부)'),
        4: ('def_spurious_copper', 'PCB 스퓨리어스 구리 결함'),
        5: ('def_missing_hole', 'PCB 홀 누락 결함'),

        # SolDef 납땜 클래스 (6-9)
        6: ('solder_no_good', '납땜 불량 (일반)'),
        7: ('solder_exc_solder', '납땜 과다'),
        8: ('solder_spike', '납땜 스파이크'),
        9: ('solder_poor_solder', '납땜 불충분'),

        # Components 부품 클래스 (10-31)
        10: ('comp_battery', '배터리 부품'),
        11: ('comp_button', '버튼 부품'),
        12: ('comp_buzzer', '부저 부품'),
        13: ('comp_capacitor', '커패시터 부품'),
        14: ('comp_clock', '클럭 부품'),
        15: ('comp_connector', '커넥터 부품'),
        16: ('comp_diode', '다이오드 부품'),
        17: ('comp_display', '디스플레이 부품'),
        18: ('comp_fuse', '퓨즈 부품'),
        19: ('comp_heatsink', '히트싱크 부품'),
        20: ('comp_ic', 'IC 부품'),
        21: ('comp_inductor', '인덕터 부품'),
        22: ('comp_led', 'LED 부품'),
        23: ('comp_pads', '패드 부품'),
        24: ('comp_pins', '핀 부품'),
        25: ('comp_potentiometer', '가변저항 부품'),
        26: ('comp_relay', '릴레이 부품'),
        27: ('comp_resistor', '저항 부품'),
        28: ('comp_switch', '스위치 부품'),
        29: ('comp_transducer', '변환기 부품'),
        30: ('comp_transformer', '변압기 부품'),
        31: ('comp_transistor', '트랜지스터 부품'),
    }

    # 통계 수집
    stats = {
        'train': Counter(),
        'val': Counter(),
        'test': Counter()
    }

    total_images = 0
    total_objects = 0

    for split in ['train', 'val', 'test']:
        labels_dir = dataset_dir / split / 'labels'
        images_dir = dataset_dir / split / 'images'

        # 이미지 개수
        image_count = len(list(images_dir.glob('*.jpg'))) + len(list(images_dir.glob('*.png')))
        total_images += image_count

        # 라벨 분석
        for label_file in labels_dir.glob('*.txt'):
            with open(label_file, 'r') as f:
                for line in f:
                    parts = line.strip().split()
                    if len(parts) >= 5:
                        class_id = int(parts[0])
                        stats[split][class_id] += 1
                        total_objects += 1

    # 전체 통계 통합
    total_stats = Counter()
    for split in ['train', 'val', 'test']:
        total_stats.update(stats[split])

    # 결과 출력
    print("=" * 80)
    print("📊 Complete PCB Model 최종 데이터셋 분석")
    print("=" * 80)

    print(f"\n전체 이미지 개수: {total_images:,}개")
    print(f"전체 객체 개수: {total_objects:,}개")

    print("\n" + "=" * 80)
    print("클래스별 상세 정보 (32개 클래스)")
    print("=" * 80)

    # 카테고리별로 그룹화
    categories = [
        ("DeepPCB 결함 클래스 (0-5)", range(0, 6)),
        ("SolDef 납땜 클래스 (6-9)", range(6, 10)),
        ("Components 부품 클래스 (10-31)", range(10, 32)),
    ]

    for category_name, class_range in categories:
        print(f"\n{category_name}")
        print("-" * 80)

        for class_id in class_range:
            name, desc = class_info[class_id]
            count = total_stats[class_id]

            # Split별 분포
            train_count = stats['train'][class_id]
            val_count = stats['val'][class_id]
            test_count = stats['test'][class_id]

            print(f"\n  클래스 {class_id:2d}: {name}")
            print(f"    설명: {desc}")
            print(f"    총 개수: {count:7,}개")
            print(f"    분포: Train={train_count:5,} | Val={val_count:5,} | Test={test_count:5,}")

    # 클래스 개수별 요약
    print("\n" + "=" * 80)
    print("클래스별 통계 요약")
    print("=" * 80)

    ranges = [
        ("10,000개 이상", lambda c: c >= 10000),
        ("5,000~9,999개", lambda c: 5000 <= c < 10000),
        ("1,000~4,999개", lambda c: 1000 <= c < 5000),
        ("500~999개", lambda c: 500 <= c < 1000),
        ("100~499개", lambda c: 100 <= c < 500),
        ("50~99개", lambda c: 50 <= c < 100),
        ("10~49개", lambda c: 10 <= c < 50),
        ("1~9개", lambda c: 1 <= c < 10),
        ("0개", lambda c: c == 0),
    ]

    for range_name, range_func in ranges:
        matching_classes = [class_id for class_id in range(32) if range_func(total_stats[class_id])]
        if matching_classes:
            print(f"\n{range_name}: {len(matching_classes)}개 클래스")
            for class_id in matching_classes:
                name, _ = class_info[class_id]
                count = total_stats[class_id]
                print(f"  - 클래스 {class_id:2d} ({name}): {count:,}개")

    print("\n" + "=" * 80)
    print("✅ 분석 완료!")
    print("=" * 80)


if __name__ == '__main__':
    main()
