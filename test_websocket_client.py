#!/usr/bin/env python3
"""
WebSocket 연결 테스트 스크립트
Flask 서버에 WebSocket으로 연결하여 프레임을 요청합니다.
"""

import socketio
import time

# SocketIO 클라이언트 생성
sio = socketio.Client(logger=True, engineio_logger=True)

@sio.on('connect')
def on_connect():
    print('[Python Client] ✅ WebSocket 연결 성공')

@sio.on('disconnect')
def on_disconnect():
    print('[Python Client] ❌ WebSocket 연결 종료')

@sio.on('connection_response')
def on_connection_response(data):
    print(f'[Python Client] 연결 응답: {data}')

@sio.on('frame_data')
def on_frame_data(data):
    camera_id = data.get('camera_id')
    frame_size = data.get('size')
    timestamp = data.get('timestamp')
    print(f'[Python Client] ✅ 프레임 수신: {camera_id} (크기: {frame_size} bytes, timestamp: {timestamp})')

@sio.on('error')
def on_error(data):
    print(f'[Python Client] ❌ 에러: {data}')

def main():
    server_url = 'http://100.123.23.111:5000'

    print(f'[Python Client] {server_url}에 연결 시도 중...')

    try:
        sio.connect(server_url)
        print('[Python Client] WebSocket 연결됨. 프레임 요청 시작...')

        # 10번 프레임 요청
        for i in range(10):
            print(f'\n[Python Client] === 요청 {i+1}/10 ===')
            sio.emit('request_frame', {'camera_id': 'left'})
            time.sleep(0.5)  # 500ms 간격

        print('\n[Python Client] 테스트 완료. 연결 종료...')
        sio.disconnect()

    except Exception as e:
        print(f'[Python Client] ❌ 오류 발생: {e}')
        import traceback
        traceback.print_exc()

if __name__ == '__main__':
    main()
