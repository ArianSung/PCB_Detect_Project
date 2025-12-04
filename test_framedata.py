#!/usr/bin/env python3
"""
frameData 필드 확인용 테스트 스크립트
"""

import socketio
import json

# SocketIO 클라이언트 생성
sio = socketio.Client(logger=True, engineio_logger=False)

@sio.on('connect')
def on_connect():
    print('[Test] ✅ WebSocket 연결 성공')

@sio.on('frame_data')
def on_frame_data(data):
    print('\n[Test] ========== frame_data 수신 ==========')
    print(f'[Test] 데이터 타입: {type(data)}')
    print(f'[Test] 키 목록: {list(data.keys())}')

    # 각 필드 확인
    for key, value in data.items():
        if key == 'frameData':
            print(f'[Test] {key}: {value[:50]}... (길이: {len(value)})')
        else:
            print(f'[Test] {key}: {value}')

    # frameData 필드가 있는지 확인
    if 'frameData' in data:
        print('[Test] ✅ frameData 필드 존재!')
    else:
        print('[Test] ❌ frameData 필드 없음!')

    # frame 필드가 있는지 확인 (이전 버전)
    if 'frame' in data:
        print('[Test] ⚠️  frame 필드 존재 (이전 버전)')

    sio.disconnect()

def main():
    server_url = 'http://100.123.23.111:5000'

    try:
        sio.connect(server_url)
        print('[Test] 프레임 요청 중...')
        sio.emit('request_frame', {'camera_id': 'left'})
        sio.sleep(2)  # 응답 대기

    except Exception as e:
        print(f'[Test] ❌ 오류: {e}')
        import traceback
        traceback.print_exc()

if __name__ == '__main__':
    main()
