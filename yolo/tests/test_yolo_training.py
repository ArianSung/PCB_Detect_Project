from ultralytics import YOLO

# YOLOv8n 모델 로드
model = YOLO('yolov8n.pt')

# COCO128 데이터셋으로 학습 (자동 다운로드)
# epochs를 1로 설정하여 빠른 테스트
results = model.train(
    data='coco128.yaml',
    epochs=1,
    imgsz=640,
    batch=8,
    name='yolo_test_train'
)

print("Training completed!")