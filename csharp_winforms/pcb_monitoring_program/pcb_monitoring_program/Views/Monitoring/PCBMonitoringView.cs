using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pcb_monitoring_program.Views.Monitoring
{
    public partial class PCBMonitoringView : UserControl
    {
        private MJPEGStreamReader _leftCameraStream;
        private MJPEGStreamReader _rightCameraStream;
        private int _leftFrameCount = 0;  // 좌측 카메라 프레임 카운트
        private int _rightFrameCount = 0; // 우측 카메라 프레임 카운트

        // 프레임 드롭을 위한 마지막 업데이트 시간
        private DateTime _lastLeftUpdate = DateTime.MinValue;
        private DateTime _lastRightUpdate = DateTime.MinValue;
        private const int MIN_UPDATE_INTERVAL_MS = 100;  // 최소 100ms 간격 (10 FPS)

        // Flask 서버 URL (나중에 config에서 읽도록 변경 예정)
        private const string SERVER_URL = "http://100.123.23.111:5000";

        public PCBMonitoringView()
        {
            InitializeComponent();

            // PictureBox 설정
            pb_LINE1PCBFRONT.SizeMode = PictureBoxSizeMode.Zoom;
            pb_LINE1PCBBACK.SizeMode = PictureBoxSizeMode.Zoom;

            // 더블 버퍼링 활성화 (깜빡거림 방지)
            EnableDoubleBuffering(pb_LINE1PCBFRONT);
            EnableDoubleBuffering(pb_LINE1PCBBACK);

            // 컨트롤 파괴 시 스트림 정리
            this.HandleDestroyed += OnHandleDestroyed;
        }

        private void PCBMonitoringView_Load(object sender, EventArgs e)
        {
            UiStyleHelper.MakeRoundedPanel(cardPCBFrontMonitoring, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardPCBBackMonitoring, radius: 16, back: Color.FromArgb(44, 44, 44));

            UiStyleHelper.AddShadowRoundedPanel(cardPCBFrontMonitoring, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardPCBBackMonitoring, 16);

            // MJPEG 스트림 시작
            StartVideoStreams();
        }

        private void StartVideoStreams()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[PCBMonitoringView] 비디오 스트림 시작...");

                // 좌측 카메라 스트림 (LINE 1 PCB Front)
                _leftCameraStream = new MJPEGStreamReader();
                _leftCameraStream.FrameReceived += OnLeftFrameReceived;
                _leftCameraStream.ErrorOccurred += OnStreamError;
                System.Diagnostics.Debug.WriteLine($"[PCBMonitoringView] 좌측 카메라 스트림 시작: {SERVER_URL}/video_feed/left");
                _leftCameraStream.Start($"{SERVER_URL}/video_feed/left");

                // 우측 카메라 스트림 (LINE 1 PCB Back)
                _rightCameraStream = new MJPEGStreamReader();
                _rightCameraStream.FrameReceived += OnRightFrameReceived;
                _rightCameraStream.ErrorOccurred += OnStreamError;
                System.Diagnostics.Debug.WriteLine($"[PCBMonitoringView] 우측 카메라 스트림 시작: {SERVER_URL}/video_feed/right");
                _rightCameraStream.Start($"{SERVER_URL}/video_feed/right");

                System.Diagnostics.Debug.WriteLine("[PCBMonitoringView] 양측 스트림 시작 완료");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PCBMonitoringView] 스트림 시작 실패: {ex.Message}");
                MessageBox.Show($"비디오 스트림 시작 실패: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnLeftFrameReceived(object sender, FrameReceivedEventArgs e)
        {
            // 프레임 드롭: 마지막 업데이트 후 100ms 이내면 스킵 (UI 과부하 방지)
            if ((DateTime.Now - _lastLeftUpdate).TotalMilliseconds < MIN_UPDATE_INTERVAL_MS)
            {
                e.Frame?.Dispose();  // 메모리 누수 방지
                return;
            }

            _lastLeftUpdate = DateTime.Now;
            _leftFrameCount++;

            // 10프레임마다 로그 출력
            if (_leftFrameCount % 10 == 0)
            {
                System.Diagnostics.Debug.WriteLine($"[PCBMonitoringView] 좌측 프레임 수신: {_leftFrameCount}개 (크기: {e.Frame.Width}x{e.Frame.Height})");
            }

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => UpdatePictureBox(pb_LINE1PCBFRONT, e.Frame, "좌측")));
            }
            else
            {
                UpdatePictureBox(pb_LINE1PCBFRONT, e.Frame, "좌측");
            }
        }

        private void OnRightFrameReceived(object sender, FrameReceivedEventArgs e)
        {
            // 프레임 드롭: 마지막 업데이트 후 100ms 이내면 스킵 (UI 과부하 방지)
            if ((DateTime.Now - _lastRightUpdate).TotalMilliseconds < MIN_UPDATE_INTERVAL_MS)
            {
                e.Frame?.Dispose();  // 메모리 누수 방지
                return;
            }

            _lastRightUpdate = DateTime.Now;
            _rightFrameCount++;

            // 10프레임마다 로그 출력
            if (_rightFrameCount % 10 == 0)
            {
                System.Diagnostics.Debug.WriteLine($"[PCBMonitoringView] 우측 프레임 수신: {_rightFrameCount}개 (크기: {e.Frame.Width}x{e.Frame.Height})");
            }

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => UpdatePictureBox(pb_LINE1PCBBACK, e.Frame, "우측")));
            }
            else
            {
                UpdatePictureBox(pb_LINE1PCBBACK, e.Frame, "우측");
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

        private void OnStreamError(object sender, System.IO.ErrorEventArgs e)
        {
            var ex = e.GetException();
            System.Diagnostics.Debug.WriteLine($"[PCBMonitoringView] 스트림 에러 발생:");
            System.Diagnostics.Debug.WriteLine($"  메시지: {ex?.Message}");
            System.Diagnostics.Debug.WriteLine($"  스택 트레이스: {ex?.StackTrace}");
        }

        private void OnHandleDestroyed(object sender, EventArgs e)
        {
            // 스트림 정리
            CleanupStreams();
        }

        private void CleanupStreams()
        {
            try
            {
                _leftCameraStream?.Stop();
                _leftCameraStream?.Dispose();
                _leftCameraStream = null;

                _rightCameraStream?.Stop();
                _rightCameraStream?.Dispose();
                _rightCameraStream = null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"스트림 정리 실패: {ex.Message}");
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
}
