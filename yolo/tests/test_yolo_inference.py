from ultralytics import YOLO

# YOLOv8n 모델 로드
model = YOLO('yolov8n.pt')

# 이미지 추론 실행하고 'runs/detect/predict/' 폴더에 자동 저장
results = model('test_images/bus.jpg', save=True)

# 이제 결과가 어디 저장되었는지 간단히 확인만 합니다.
if results:
    print("결과가 'runs/detect/predict' 폴더에 저장되었습니다.")