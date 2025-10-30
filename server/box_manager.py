# server/box_manager.py

import requests
import logging

logger = logging.getLogger(__name__)

class BoxManager:
    """
    박스 상태 관리 매니저

    - 3개 박스 (NORMAL, COMPONENT_DEFECT, SOLDER_DEFECT)
    - 각 박스 5슬롯
    - 박스 가득 찬 경우 OHT 자동 호출
    """

    def __init__(self):
        self.boxes = {
            'NORMAL': {
                'slots': [None] * 5,
                'current_slot': 0,
                'is_full': False
            },
            'COMPONENT_DEFECT': {
                'slots': [None] * 5,
                'current_slot': 0,
                'is_full': False
            },
            'SOLDER_DEFECT': {
                'slots': [None] * 5,
                'current_slot': 0,
                'is_full': False
            },
            'DISCARD': {
                'slots': [None],  # 슬롯 관리 없음
                'current_slot': 0,
                'is_full': False
            }
        }

    def assign_slot(self, box_id):
        """
        슬롯 할당

        Args:
            box_id: 'NORMAL', 'COMPONENT_DEFECT', 'SOLDER_DEFECT', 'DISCARD'

        Returns:
            int: 할당된 슬롯 번호 (0-4)

        Raises:
            BoxFullException: 박스가 가득 찬 경우
        """
        if box_id == 'DISCARD':
            return 0  # 슬롯 관리 없음

        box = self.boxes[box_id]

        # 박스 가득 찬지 확인
        if box['current_slot'] >= 5:
            box['is_full'] = True
            # OHT 자동 호출 트리거
            self._trigger_auto_oht(box_id)
            raise BoxFullException(f"{box_id} 박스가 가득 찼습니다 (5/5 슬롯)")

        # 슬롯 할당
        slot = box['current_slot']
        box['current_slot'] += 1

        logger.info(f"{box_id} 박스 슬롯 {slot} 할당 ({box['current_slot']}/5)")

        return slot

    def update_box_status(self, box_id, slot_index, pcb_id):
        """
        박스 상태 업데이트

        Args:
            box_id: 박스 ID
            slot_index: 슬롯 번호 (0-4)
            pcb_id: PCB ID
        """
        if box_id == 'DISCARD':
            return  # 슬롯 관리 없음

        box = self.boxes[box_id]
        box['slots'][slot_index] = pcb_id

        # 박스 가득 찬지 확인 (5/5)
        if all(slot is not None for slot in box['slots']):
            box['is_full'] = True
            logger.warning(f"⚠️ {box_id} 박스가 가득 찼습니다! (5/5)")
            # OHT 자동 호출
            self._trigger_auto_oht(box_id)

    def _trigger_auto_oht(self, category):
        """
        OHT 자동 호출 트리거

        Args:
            category: 박스 카테고리
        """
        try:
            payload = {
                'category': category,
                'trigger_reason': 'box_full'
            }
            response = requests.post(
                'http://localhost:5000/api/oht/auto_trigger',
                json=payload,
                timeout=5
            )

            if response.status_code == 200:
                logger.info(f"✅ {category} 박스에 대한 OHT 자동 호출 성공")
            else:
                logger.error(f"❌ OHT 자동 호출 실패: {response.status_code}")

        except Exception as e:
            logger.error(f"❌ OHT 자동 호출 오류: {e}")

    def reset_box(self, box_id):
        """
        박스 리셋 (비우기)

        Args:
            box_id: 박스 ID
        """
        if box_id == 'DISCARD':
            return

        box = self.boxes[box_id]
        box['slots'] = [None] * 5
        box['current_slot'] = 0
        box['is_full'] = False

        logger.info(f"{box_id} 박스 리셋 완료")

    def get_box_status(self, box_id):
        """
        박스 상태 조회

        Args:
            box_id: 박스 ID

        Returns:
            dict: 박스 상태 정보
        """
        if box_id not in self.boxes:
            return None

        box = self.boxes[box_id]
        return {
            'box_id': box_id,
            'total_slots': 5 if box_id != 'DISCARD' else 0,
            'current_slot': box['current_slot'],
            'is_full': box['is_full'],
            'slots': box['slots']
        }

    def get_all_box_status(self):
        """
        모든 박스 상태 조회

        Returns:
            list: 모든 박스 상태 정보
        """
        return [
            self.get_box_status('NORMAL'),
            self.get_box_status('COMPONENT_DEFECT'),
            self.get_box_status('SOLDER_DEFECT')
        ]


class BoxFullException(Exception):
    """박스 가득 찬 예외"""
    pass


# 글로벌 BoxManager 인스턴스
box_manager = BoxManager()
