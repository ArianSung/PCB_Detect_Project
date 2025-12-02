using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SocketIOClient;

namespace pcb_monitoring_program.Views.Monitoring
{
    

    public partial class PCBMonitoringView : UserControl
    {
        // SocketIO 클라이언트
        private SocketIOClient.SocketIO _socket;

        // 프레임 요청 타이머 (33ms 간격 = 30 FPS)
        private System.Windows.Forms.Timer _frameRequestTimer;

        // 프레임 카운트
        private int _leftFrameCount = 0;
        private int _rightFrameCount = 0;

        // 프레임 드롭을 위한 마지막 업데이트 시간 (UI 과부하 방지)
        private DateTime _lastLeftUpdate = DateTime.MinValue;
        private DateTime _lastRightUpdate = DateTime.MinValue;
        private const int MIN_UPDATE_INTERVAL_MS = 33;  // 최소 33ms 간격 (30 FPS)

        // Flask 서버 URL (나중에 config에서 읽도록 변경 예정)
        private const string SERVER_URL = "http://100.80.24.53:5000";

        public PCBMonitoringView()
        {
            InitializeComponent();

            // PictureBox 설정 (StretchImage: 고품질 스무딩 비활성화로 성능 향상)
            pb_LINE1PCBFRONT.SizeMode = PictureBoxSizeMode.StretchImage;
            pb_LINE1PCBBACK.SizeMode = PictureBoxSizeMode.StretchImage;

            // 더블 버퍼링 활성화 (깜빡거림 방지)
            EnableDoubleBuffering(pb_LINE1PCBFRONT);
            EnableDoubleBuffering(pb_LINE1PCBBACK);

            // 컨트롤 파괴 시 스트림 정리
            this.HandleDestroyed += OnHandleDestroyed;
        }

        private async void PCBMonitoringView_Load(object sender, EventArgs e)
        {
            UiStyleHelper.MakeRoundedPanel(cardPCBFrontMonitoring, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardPCBBackMonitoring, radius: 16, back: Color.FromArgb(44, 44, 44));

            UiStyleHelper.AddShadowRoundedPanel(cardPCBFrontMonitoring, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardPCBBackMonitoring, 16);

            // WebSocket 연결 시작
            await InitializeWebSocket();
        }

        private async Task InitializeWebSocket()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[PCBMonitoringView] WebSocket 연결 시작...");

                // SocketIO 클라이언트 생성
                _socket = new SocketIOClient.SocketIO(SERVER_URL);

                // 연결 이벤트 핸들러
                _socket.OnConnected += async (sender, e) =>
                {
                    System.Diagnostics.Debug.WriteLine($"[PCBMonitoringView] WebSocket 연결 성공: {SERVER_URL}");

                    // 연결 성공 시 프레임 요청 타이머 시작
                    if (InvokeRequired)
                    {
                        Invoke(new Action(StartFrameRequestTimer));
                    }
                    else
                    {
                        StartFrameRequestTimer();
                    }
                };

                // 연결 해제 이벤트 핸들러
                _socket.OnDisconnected += (sender, e) =>
                {
                    System.Diagnostics.Debug.WriteLine("[PCBMonitoringView] WebSocket 연결 해제");

                    // 연결 해제 시 타이머 중지
                    if (InvokeRequired)
                    {
                        Invoke(new Action(StopFrameRequestTimer));
                    }
                    else
                    {
                        StopFrameRequestTimer();
                    }
                };

                // frame_update 이벤트 핸들러 (프레임 수신) ⭐ Flask v3.0 호환
                _socket.On("frame_update", response =>
                {
                    try
                    {
                        // DTO 객체로 데이터 수신
                        var data = response.GetValue<FrameData>();

                        string cameraId = data.camera_id;
                        string frameBase64 = data.image;  // Flask v3.0: frameData → image

                        // 프레임 드롭: 마지막 업데이트 후 33ms 이내면 스킵 (UI 과부하 방지)
                        if (cameraId == "left")
                        {
                            if ((DateTime.Now - _lastLeftUpdate).TotalMilliseconds < MIN_UPDATE_INTERVAL_MS)
                            {
                                return;  // 프레임 스킵
                            }
                            _lastLeftUpdate = DateTime.Now;
                        }
                        else if (cameraId == "right")
                        {
                            if ((DateTime.Now - _lastRightUpdate).TotalMilliseconds < MIN_UPDATE_INTERVAL_MS)
                            {
                                return;  // 프레임 스킵
                            }
                            _lastRightUpdate = DateTime.Now;
                        }

                        // Base64 디코딩
                        byte[] frameBytes = Convert.FromBase64String(frameBase64);

                        // JPEG 바이트를 Image로 변환
                        using (MemoryStream ms = new MemoryStream(frameBytes))
                        {
                            using (Image tempImage = Image.FromStream(ms))
                            {
                                // 복사본 생성 (원본은 스트림과 함께 해제되므로)
                                Bitmap bitmap = new Bitmap(tempImage);

                                //bitmap.Save(@"C:\Test.bmp");

                                // UI 스레드에서 PictureBox 업데이트
                                if (cameraId == "left")
                                {
                                    _leftFrameCount++;
                                    if (InvokeRequired)
                                    {
                                        BeginInvoke(new Action(() => UpdatePictureBox(pb_LINE1PCBFRONT, bitmap, "좌측")));
                                    }
                                    else
                                    {
                                        UpdatePictureBox(pb_LINE1PCBFRONT, bitmap, "좌측");
                                    }
                                }
                                else if (cameraId == "right")
                                {
                                    _rightFrameCount++;
                                    if (InvokeRequired)
                                    {
                                        BeginInvoke(new Action(() => UpdatePictureBox(pb_LINE1PCBBACK, bitmap, "우측")));
                                    }
                                    else
                                    {
                                        UpdatePictureBox(pb_LINE1PCBBACK, bitmap, "우측");
                                    }
                                }

                                // 10프레임마다 로그 출력
                                int frameCount = (cameraId == "left") ? _leftFrameCount : _rightFrameCount;
                                if (frameCount % 10 == 0)
                                {
                                    System.Diagnostics.Debug.WriteLine($"[PCBMonitoringView] {cameraId} 프레임 수신: {frameCount}개 (크기: {frameBytes.Length} bytes)");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[PCBMonitoringView] frame_update 처리 실패: {ex.Message}");
                    }
                });

                // connection_response 이벤트 핸들러 (선택적)
                _socket.On("connection_response", response =>
                {
                    var data = response.GetValue<ConnectionResponse>();
                    System.Diagnostics.Debug.WriteLine($"[PCBMonitoringView] 연결 응답: {data.status} - {data.message}");
                });

                // error 이벤트 핸들러 (선택적)
                _socket.On("error", response =>
                {
                    var data = response.GetValue<ErrorResponse>();
                    System.Diagnostics.Debug.WriteLine($"[PCBMonitoringView] 서버 에러: {data.message}");
                });

                // WebSocket 연결
                System.Diagnostics.Debug.WriteLine($"[PCBMonitoringView] ConnectAsync() 호출 직전...");
                await _socket.ConnectAsync();
                System.Diagnostics.Debug.WriteLine($"[PCBMonitoringView] ConnectAsync() 완료");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PCBMonitoringView] WebSocket 초기화 실패: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[PCBMonitoringView] 스택 트레이스: {ex.StackTrace}");

                // UI 스레드에서 MessageBox 표시
                if (InvokeRequired)
                {
                    Invoke(new Action(() =>
                        MessageBox.Show($"WebSocket 연결 실패:\n{ex.Message}\n\n{ex.StackTrace}",
                            "오류", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    ));
                }
                else
                {
                    MessageBox.Show($"WebSocket 연결 실패:\n{ex.Message}\n\n{ex.StackTrace}",
                        "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void StartFrameRequestTimer()
        {
            if (_frameRequestTimer == null)
            {
                _frameRequestTimer = new System.Windows.Forms.Timer();
                _frameRequestTimer.Interval = 33;  // 33ms (양쪽 카메라 요청 시 초당 30 FPS)
                _frameRequestTimer.Tick += OnFrameRequestTimerTick;
            }

            _frameRequestTimer.Start();
            System.Diagnostics.Debug.WriteLine("[PCBMonitoringView] 프레임 요청 타이머 시작 (33ms 간격, 30 FPS)");
        }

        private void StopFrameRequestTimer()
        {
            if (_frameRequestTimer != null)
            {
                _frameRequestTimer.Stop();
                System.Diagnostics.Debug.WriteLine("[PCBMonitoringView] 프레임 요청 타이머 중지");
            }
        }

        private async void OnFrameRequestTimerTick(object sender, EventArgs e)
        {
            try
            {
                if (_socket != null && _socket.Connected)
                {
                    // 양쪽 카메라에 대해 프레임 요청
                    await _socket.EmitAsync("request_frame", new { camera_id = "left" });
                    await _socket.EmitAsync("request_frame", new { camera_id = "right" });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PCBMonitoringView] 프레임 요청 실패: {ex.Message}");
            }
        }

        private void UpdatePictureBox(PictureBox pictureBox, Image newFrame, string camera)
        {
            try
            {
                // 이전 이미지 해제
                var oldImage = pictureBox.Image;
                pictureBox.Image = newFrame;
                oldImage?.Dispose();

                // 첫 10개 프레임만 업데이트 로그 출력
                int frameCount = (camera == "좌측") ? _leftFrameCount : _rightFrameCount;
                if (frameCount <= 10 || frameCount % 50 == 0)
                {
                    System.Diagnostics.Debug.WriteLine($"[PCBMonitoringView] {camera} PictureBox 업데이트 성공: {pictureBox.Name} (프레임 #{frameCount})");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PCBMonitoringView] {camera} PictureBox 업데이트 실패: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void OnHandleDestroyed(object sender, EventArgs e)
        {
            // WebSocket 정리
            CleanupWebSocket();
        }

        private async void CleanupWebSocket()
        {
            try
            {
                // 타이머 중지
                StopFrameRequestTimer();
                _frameRequestTimer?.Dispose();
                _frameRequestTimer = null;

                // WebSocket 연결 해제
                if (_socket != null)
                {
                    await _socket.DisconnectAsync();
                    _socket.Dispose();
                    _socket = null;
                }

                System.Diagnostics.Debug.WriteLine("[PCBMonitoringView] WebSocket 정리 완료");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PCBMonitoringView] WebSocket 정리 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// PictureBox에 더블 버퍼링을 활성화하여 깜빡거림 방지
        /// </summary>
        private void EnableDoubleBuffering(PictureBox pictureBox)
        {
            try
            {
                typeof(PictureBox).InvokeMember("DoubleBuffered",
                    System.Reflection.BindingFlags.SetProperty |
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic,
                    null, pictureBox, new object[] { true });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"더블 버퍼링 설정 실패: {ex.Message}");
            }
        }
    }
    // SocketIO 데이터 전송 객체 (DTO) - Flask v3.0 호환 ⭐
    public class FrameData
    {
        public string camera_id { get; set; }
        public string image { get; set; }  // Flask v3.0: frameData → image
        public string defect_type { get; set; }  // 불량 유형 (추가됨)
        public double confidence { get; set; }  // 신뢰도 (추가됨)
        public int boxes_count { get; set; }  // 검출 박스 수 (추가됨)
        public string roi_status { get; set; }  // ROI 상태 (추가됨)
        public string timestamp { get; set; }  // ISO 포맷으로 변경
        public string type { get; set; }  // 프레임 타입 (final_frame)
    }

    public class ConnectionResponse
    {
        public string status { get; set; }
        public string message { get; set; }
    }

    public class ErrorResponse
    {
        public string message { get; set; }
    }
}
