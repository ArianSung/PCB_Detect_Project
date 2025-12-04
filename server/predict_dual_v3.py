"""
/predict_dual 엔드포인트 새 버전 (제품별 부품 검증 워크플로우)

이 파일의 내용을 app.py의 기존 predict_dual() 함수와 교체하세요.
"""

# 이 함수를 app.py의 1009-1314줄 사이에 있는 기존 predict_dual() 함수와 교체
def predict_dual():
    """
    양면 동시 추론 (제품별 부품 검증 워크플로우)

    워크플로우:
    1. 뒷면 (우측) → 시리얼 넘버 OCR → 제품 코드 추출
    2. DB 조회 → 제품 코드별 부품 배치 기준 로드
    3. 앞면 (좌측) → 템플릿 매칭 → YOLO 부품 검출
    4. ComponentVerifier로 부품 위치 검증 (동적 생성)
    5. 최종 판정 (normal/missing/position_error/discard)
    6. v3.0 스키마로 DB 저장

    Request JSON:
        {
            "left_image": "base64_encoded_jpeg_image",   # 앞면 (부품 검증용)
            "right_image": "base64_encoded_jpeg_image"   # 뒷면 (시리얼 넘버 OCR용)
        }

    Response JSON:
        {
            "status": "ok",
            "serial_number": "MBBC-00000001",
            "product_code": "BC",
            "decision": "normal",  # normal/missing/position_error/discard
            "verification": {
                "missing_count": 0,
                "position_error_count": 0,
                "extra_count": 0,
                "correct_count": 18
            },
            "gpio_signal": {"pin": 23, "duration_ms": 300},
            "inference_time_ms": 150.5
        }
    """
    start_time = time.time()

    try:
        # ==================== 1. 요청 데이터 검증 ====================
        data = request.get_json()
        if not data:
            logger.error("요청 데이터가 비어있음")
            return jsonify({
                'status': 'error',
                'error': 'Request body is empty'
            }), 400

        left_image = data.get('left_image')
        right_image = data.get('right_image')

        if not left_image or not right_image:
            logger.error(f"필수 필드 누락: left_image={'있음' if left_image else '없음'}, right_image={'있음' if right_image else '없음'}")
            return jsonify({
                'status': 'error',
                'error': 'Missing required fields: left_image, right_image'
            }), 400

        # ==================== 2. 프레임 디코딩 ====================
        try:
            # 좌측 프레임 (앞면 - 부품 검증용)
            left_bytes = base64.b64decode(left_image)
            left_nparr = np.frombuffer(left_bytes, np.uint8)
            left_frame = cv2.imdecode(left_nparr, cv2.IMREAD_COLOR)

            if left_frame is None or left_frame.size == 0:
                raise ValueError("좌측 프레임 디코딩 실패")

            logger.info(f"좌측 프레임 수신 성공 (shape: {left_frame.shape})")

            # 우측 프레임 (뒷면 - 시리얼 넘버 OCR용)
            right_bytes = base64.b64decode(right_image)
            right_nparr = np.frombuffer(right_bytes, np.uint8)
            right_frame = cv2.imdecode(right_nparr, cv2.IMREAD_COLOR)

            if right_frame is None or right_frame.size == 0:
                raise ValueError("우측 프레임 디코딩 실패")

            logger.info(f"우측 프레임 수신 성공 (shape: {right_frame.shape})")

        except Exception as e:
            logger.error(f"프레임 디코딩 실패: {e}")
            return jsonify({
                'status': 'error',
                'error': f'Failed to decode frames: {str(e)}'
            }), 400

        # ==================== 3. STEP 1: 뒷면 시리얼 넘버 OCR ====================
        ocr_time_start = time.time()

        if serial_detector is None:
            logger.error("시리얼 넘버 검출기가 초기화되지 않았습니다")
            return jsonify({
                'status': 'error',
                'error': '시리얼 넘버 검출기가 초기화되지 않았습니다'
            }), 500

        ocr_result = serial_detector.detect_serial_number(right_frame)
        ocr_time = (time.time() - ocr_time_start) * 1000  # ms

        if ocr_result['status'] != 'ok':
            logger.error(f"시리얼 넘버 검출 실패: {ocr_result.get('error')}")
            return jsonify({
                'status': 'error',
                'error': f"시리얼 넘버 검출 실패: {ocr_result.get('error')}",
                'ocr_result': ocr_result
            }), 400

        serial_number = ocr_result['serial_number']  # "MBBC-00000001"
        product_code = ocr_result['product_code']    # "BC"

        logger.info(f"✅ 시리얼 넘버 검출 성공: {serial_number} (제품: {product_code}, OCR 시간: {ocr_time:.1f}ms)")

        # ==================== 4. STEP 2: DB에서 기준 부품 배치 로드 ====================
        db_time_start = time.time()

        reference_components = db.get_reference_components(product_code)

        if not reference_components:
            logger.error(f"제품 코드 '{product_code}'의 기준 데이터가 없습니다")
            return jsonify({
                'status': 'error',
                'error': f"제품 코드 '{product_code}'의 기준 데이터가 DB에 없습니다. 먼저 기준 데이터를 등록하세요.",
                'serial_number': serial_number,
                'product_code': product_code
            }), 404

        db_time = (time.time() - db_time_start) * 1000  # ms
        logger.info(f"✅ 제품 '{product_code}' 기준 부품 {len(reference_components)}개 로드 (DB 시간: {db_time:.1f}ms)")

        # ==================== 5. STEP 3: 앞면 템플릿 매칭 및 정렬 ====================
        alignment_time_start = time.time()

        aligned_frame = left_frame
        template_match_success = False

        if template_alignment is not None:
            try:
                template_result = template_alignment.align(left_frame)
                if template_result['success']:
                    aligned_frame = template_result['aligned_frame']
                    template_match_success = True
                    logger.info(f"✅ 템플릿 매칭 성공 (신뢰도: {template_result.get('confidence', 0):.2%})")
                else:
                    logger.warning(f"⚠️  템플릿 매칭 실패: {template_result.get('error', 'Unknown')}")
            except Exception as e:
                logger.error(f"템플릿 매칭 오류: {e}")
        else:
            logger.warning("템플릿 매칭 시스템이 초기화되지 않았습니다")

        alignment_time = (time.time() - alignment_time_start) * 1000  # ms

        # ==================== 6. STEP 4: YOLO 부품 검출 ====================
        inference_time_start = time.time()

        detected_components = []

        if yolo_model is not None:
            yolo_results = yolo_model.predict(aligned_frame, conf=0.25, iou=0.7, verbose=False)

            if len(yolo_results) > 0 and len(yolo_results[0].boxes) > 0:
                boxes = yolo_results[0].boxes
                for box in boxes:
                    x1, y1, x2, y2 = box.xyxy[0].cpu().numpy()
                    cx, cy = (x1 + x2) / 2, (y1 + y2) / 2
                    class_id = int(box.cls[0])
                    class_name = yolo_model.names[class_id]
                    confidence = float(box.conf[0])

                    detected_components.append({
                        'class_name': class_name,
                        'bbox': [float(x1), float(y1), float(x2), float(y2)],
                        'center': [float(cx), float(cy)],
                        'confidence': confidence
                    })

            logger.info(f"✅ YOLO 부품 검출 완료: {len(detected_components)}개 검출")
        else:
            logger.error("YOLO 모델이 초기화되지 않았습니다")
            return jsonify({
                'status': 'error',
                'error': 'YOLO 모델이 초기화되지 않았습니다'
            }), 500

        inference_time = (time.time() - inference_time_start) * 1000  # ms

        # ==================== 7. STEP 5: 부품 위치 검증 (동적 ComponentVerifier 생성) ====================
        verification_time_start = time.time()

        # 동적으로 ComponentVerifier 생성 (제품별 기준 데이터 사용)
        verifier = ComponentVerifier(
            reference_components=reference_components,
            position_threshold=20.0,  # 20픽셀 허용 오차
            confidence_threshold=0.25
        )

        verification_result = verifier.verify_components(detected_components, debug=False)

        verification_time = (time.time() - verification_time_start) * 1000  # ms

        logger.info(
            f"✅ 부품 검증 완료: 정상 {verification_result['summary']['correct_count']}개, "
            f"위치오류 {verification_result['summary']['misplaced_count']}개, "
            f"누락 {verification_result['summary']['missing_count']}개, "
            f"추가 {verification_result['summary']['extra_count']}개"
        )

        # ==================== 8. STEP 6: 최종 판정 ====================
        is_critical, reason = verifier.is_critical_defect(verification_result)

        if is_critical:
            decision = 'discard'  # 폐기 (누락 3개 이상, 위치오류 5개 이상, 합계 7개 이상)
            logger.warning(f"🔴 치명적 불량 (폐기): {reason}")
        elif verification_result['summary']['missing_count'] > 0:
            decision = 'missing'  # 부품 누락
            logger.warning(f"🟡 부품 누락: {verification_result['summary']['missing_count']}개")
        elif verification_result['summary']['misplaced_count'] > 0:
            decision = 'position_error'  # 위치 오류
            logger.warning(f"🟡 위치 오류: {verification_result['summary']['misplaced_count']}개")
        else:
            decision = 'normal'  # 정상
            logger.info("🟢 정상 제품")

        # GPIO 핀 결정 (라즈베리파이 BCM 모드)
        gpio_map = {
            'missing': 17,          # 부품 누락
            'position_error': 27,   # 위치 오류
            'discard': 22,          # 폐기
            'normal': 23            # 정상
        }
        gpio_pin = gpio_map.get(decision, 23)

        # ==================== 9. DB 저장 (v3.0 스키마) ====================
        try:
            # 평균 신뢰도 계산
            avg_confidence = (
                sum(c['confidence'] for c in detected_components) / len(detected_components)
                if detected_components else 0.0
            )

            inspection_id = db.insert_inspection_v3(
                serial_number=serial_number,
                product_code=product_code,
                decision=decision,
                missing_count=verification_result['summary']['missing_count'],
                position_error_count=verification_result['summary']['misplaced_count'],
                extra_count=verification_result['summary']['extra_count'],
                correct_count=verification_result['summary']['correct_count'],
                missing_components=verification_result['missing'],
                position_errors=verification_result['misplaced'],
                extra_components=verification_result['extra'],
                yolo_detections=detected_components,
                detection_count=len(detected_components),
                avg_confidence=avg_confidence,
                inference_time_ms=inference_time,
                verification_time_ms=verification_time,
                total_time_ms=(time.time() - start_time) * 1000,
                image_width=left_frame.shape[1],
                image_height=left_frame.shape[0],
                camera_id='dual',
                serial_detected=True,
                server_version='1.0.0-v3'
            )

            logger.info(f"✅ 검사 이력 저장 완료 (ID: {inspection_id})")

        except Exception as db_error:
            logger.error(f"❌ DB 저장 실패: {db_error}")

        # ==================== 10. 응답 생성 ====================
        total_time_ms = (time.time() - start_time) * 1000

        response = {
            'status': 'ok',
            # 제품 식별 정보
            'serial_number': serial_number,
            'product_code': product_code,
            # 최종 판정
            'decision': decision,
            'decision_reason': reason if is_critical else None,
            # 검증 결과 요약
            'verification': {
                'missing_count': verification_result['summary']['missing_count'],
                'position_error_count': verification_result['summary']['misplaced_count'],
                'extra_count': verification_result['summary']['extra_count'],
                'correct_count': verification_result['summary']['correct_count'],
                'total_reference': verification_result['summary']['total_reference'],
                'total_detected': verification_result['summary']['total_detected']
            },
            # 상세 정보
            'details': {
                'missing': verification_result['missing'][:3] if verification_result['missing'] else [],  # 최대 3개만
                'misplaced': verification_result['misplaced'][:3] if verification_result['misplaced'] else [],
                'extra': verification_result['extra'][:3] if verification_result['extra'] else []
            },
            # GPIO 제어
            'gpio_signal': {
                'pin': gpio_pin,
                'duration_ms': 300
            },
            # 성능 정보
            'performance': {
                'ocr_time_ms': round(ocr_time, 2),
                'db_query_time_ms': round(db_time, 2),
                'template_match_time_ms': round(alignment_time, 2),
                'inference_time_ms': round(inference_time, 2),
                'verification_time_ms': round(verification_time, 2),
                'total_time_ms': round(total_time_ms, 2)
            },
            # 시스템 정보
            'template_match_success': template_match_success,
            'timestamp': datetime.now().isoformat()
        }

        logger.info(
            f"✅ 양면 검사 완료: 시리얼={serial_number}, 제품={product_code}, "
            f"판정={decision}, GPIO={gpio_pin}, 총 시간={total_time_ms:.1f}ms"
        )

        return jsonify(response)

    except Exception as e:
        logger.error(f"❌ 양면 추론 실패: {str(e)}", exc_info=True)
        return jsonify({
            'status': 'error',
            'error': str(e)
        }), 500
