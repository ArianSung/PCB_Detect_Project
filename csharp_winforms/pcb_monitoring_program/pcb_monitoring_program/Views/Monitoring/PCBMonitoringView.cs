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

        // Flask 서버 URL (나중에 config에서 읽도록 변경 예정)
        private const string SERVER_URL = "http://100.123.23.111:5000";

        public PCBMonitoringView()
        {
            InitializeComponent();

            // PictureBox 설정
            pb_LINE1PCBFRONT.SizeMode = PictureBoxSizeMode.Zoom;
            pb_LINE1PCBBACK.SizeMode = PictureBoxSizeMode.Zoom;

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
                // 좌측 카메라 스트림 (LINE 1 PCB Front)
                _leftCameraStream = new MJPEGStreamReader();
                _leftCameraStream.FrameReceived += OnLeftFrameReceived;
                _leftCameraStream.ErrorOccurred += OnStreamError;
                _leftCameraStream.Start($"{SERVER_URL}/video_feed/left");

                // 우측 카메라 스트림 (LINE 1 PCB Back)
                _rightCameraStream = new MJPEGStreamReader();
                _rightCameraStream.FrameReceived += OnRightFrameReceived;
                _rightCameraStream.ErrorOccurred += OnStreamError;
                _rightCameraStream.Start($"{SERVER_URL}/video_feed/right");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"비디오 스트림 시작 실패: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnLeftFrameReceived(object sender, FrameReceivedEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => UpdatePictureBox(pb_LINE1PCBFRONT, e.Frame)));
            }
            else
            {
                UpdatePictureBox(pb_LINE1PCBFRONT, e.Frame);
            }
        }

        private void OnRightFrameReceived(object sender, FrameReceivedEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => UpdatePictureBox(pb_LINE1PCBBACK, e.Frame)));
            }
            else
            {
                UpdatePictureBox(pb_LINE1PCBBACK, e.Frame);
            }
        }

        private void UpdatePictureBox(PictureBox pictureBox, Image newFrame)
        {
            try
            {
                // 이전 이미지 해제
                var oldImage = pictureBox.Image;
                pictureBox.Image = newFrame;
                oldImage?.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PictureBox 업데이트 실패: {ex.Message}");
            }
        }

        private void OnStreamError(object sender, System.IO.ErrorEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"스트림 에러: {e.GetException()?.Message}");
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
    }
}
