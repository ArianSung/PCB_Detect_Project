using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace pcb_monitoring_program
{
    /// <summary>
    /// MJPEG 스트림을 읽고 프레임을 이미지로 변환하는 클래스
    /// Flask 서버의 /video_feed/<camera_id> 엔드포인트에서 스트림을 받음
    /// </summary>
    public class MJPEGStreamReader : IDisposable
    {
        private readonly HttpClient _httpClient;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _readTask;
        private bool _isRunning;
        private string _streamUrl; // 디버깅을 위한 스트림 URL 저장
        private int _frameCount; // 수신된 프레임 카운트

        /// <summary>
        /// 새로운 프레임을 받았을 때 발생하는 이벤트
        /// </summary>
        public event EventHandler<FrameReceivedEventArgs> FrameReceived;

        /// <summary>
        /// 에러가 발생했을 때 발생하는 이벤트
        /// </summary>
        public event EventHandler<ErrorEventArgs> ErrorOccurred;

        public MJPEGStreamReader()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _frameCount = 0;
        }

        /// <summary>
        /// MJPEG 스트림 읽기 시작
        /// </summary>
        /// <param name="streamUrl">스트림 URL (예: http://100.96.79.71:5000/video_feed/left)</param>
        public void Start(string streamUrl)
        {
            if (_isRunning)
            {
                Stop();
            }

            _streamUrl = streamUrl;
            _frameCount = 0;
            System.Diagnostics.Debug.WriteLine($"[MJPEGStreamReader] 스트림 시작: {_streamUrl}");

            _cancellationTokenSource = new CancellationTokenSource();
            _isRunning = true;
            _readTask = Task.Run(() => ReadStreamAsync(streamUrl, _cancellationTokenSource.Token));
        }

        /// <summary>
        /// MJPEG 스트림 읽기 중지
        /// </summary>
        public void Stop()
        {
            if (!_isRunning)
                return;

            _isRunning = false;
            _cancellationTokenSource?.Cancel();
            _readTask?.Wait(TimeSpan.FromSeconds(2));
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }

        private async Task ReadStreamAsync(string streamUrl, CancellationToken cancellationToken)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[MJPEGStreamReader] HTTP 연결 시작: {_streamUrl}");
                using (var response = await _httpClient.GetAsync(streamUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                {
                    response.EnsureSuccessStatusCode();
                    System.Diagnostics.Debug.WriteLine($"[MJPEGStreamReader] HTTP 연결 성공: {_streamUrl} (Status: {response.StatusCode})");

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        System.Diagnostics.Debug.WriteLine($"[MJPEGStreamReader] 스트림 파싱 시작: {_streamUrl}");
                        await ParseMJPEGStream(stream, cancellationToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // 정상적인 취소
                System.Diagnostics.Debug.WriteLine($"[MJPEGStreamReader] 스트림 취소됨: {_streamUrl}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MJPEGStreamReader] 스트림 에러: {_streamUrl} - {ex.Message}");
                OnErrorOccurred(new ErrorEventArgs(ex));
            }
        }

        private async Task ParseMJPEGStream(Stream stream, CancellationToken cancellationToken)
        {
            const string boundary = "--frame";
            byte[] boundaryBytes = System.Text.Encoding.UTF8.GetBytes(boundary);
            byte[] buffer = new byte[65536]; // 64KB 버퍼
            int bufferPos = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                int bytesRead = await stream.ReadAsync(buffer, bufferPos, buffer.Length - bufferPos, cancellationToken);
                if (bytesRead == 0)
                {
                    break; // 스트림 종료
                }

                bufferPos += bytesRead;

                // boundary 찾기
                int boundaryIndex = FindBoundary(buffer, bufferPos, boundaryBytes);
                while (boundaryIndex >= 0)
                {
                    // 다음 boundary 찾기
                    int nextBoundaryIndex = FindBoundary(buffer, bufferPos, boundaryBytes, boundaryIndex + boundaryBytes.Length);
                    if (nextBoundaryIndex < 0)
                    {
                        // 다음 boundary를 찾지 못함 -> 더 읽어야 함
                        break;
                    }

                    // boundary 사이의 데이터 추출
                    int frameStart = boundaryIndex + boundaryBytes.Length;
                    int frameLength = nextBoundaryIndex - frameStart;

                    // Content-Type 헤더 건너뛰기 (JPEG 데이터 시작점 찾기)
                    int jpegStart = FindJPEGStart(buffer, frameStart, frameLength);
                    if (jpegStart >= 0)
                    {
                        int jpegLength = nextBoundaryIndex - jpegStart;
                        byte[] jpegData = new byte[jpegLength];
                        Array.Copy(buffer, jpegStart, jpegData, 0, jpegLength);

                        // JPEG → Image 변환
                        try
                        {
                            // MemoryStream을 닫지 않고 Bitmap으로 복사
                            using (var ms = new MemoryStream(jpegData))
                            {
                                using (var tempImage = Image.FromStream(ms))
                                {
                                    // Bitmap으로 복사하여 MemoryStream 종속성 제거
                                    var frame = new Bitmap(tempImage);
                                    _frameCount++;

                                    // 10프레임마다 로그 출력 (너무 많은 로그 방지)
                                    if (_frameCount % 10 == 0)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"[MJPEGStreamReader] 프레임 수신 성공: {_streamUrl} (총 {_frameCount}개, JPEG 크기: {jpegLength} bytes)");
                                    }

                                    OnFrameReceived(new FrameReceivedEventArgs(frame));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[MJPEGStreamReader] JPEG 변환 실패: {_streamUrl} - {ex.Message}");
                            OnErrorOccurred(new ErrorEventArgs(ex));
                        }
                    }
                    else
                    {
                        // JPEG 시작 마커를 찾지 못한 경우 (가끔 발생 가능)
                        if (_frameCount == 0) // 첫 프레임에서만 경고
                        {
                            System.Diagnostics.Debug.WriteLine($"[MJPEGStreamReader] JPEG 시작점을 찾을 수 없음: {_streamUrl}");
                        }
                    }

                    // 처리된 데이터 제거
                    int remainingBytes = bufferPos - nextBoundaryIndex;
                    Array.Copy(buffer, nextBoundaryIndex, buffer, 0, remainingBytes);
                    bufferPos = remainingBytes;

                    // 다음 프레임 찾기
                    boundaryIndex = FindBoundary(buffer, bufferPos, boundaryBytes);
                }
            }
        }

        private int FindBoundary(byte[] buffer, int length, byte[] boundaryBytes, int startIndex = 0)
        {
            for (int i = startIndex; i <= length - boundaryBytes.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < boundaryBytes.Length; j++)
                {
                    if (buffer[i + j] != boundaryBytes[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                {
                    return i;
                }
            }
            return -1;
        }

        private int FindJPEGStart(byte[] buffer, int start, int length)
        {
            // JPEG 파일은 0xFF 0xD8로 시작
            for (int i = start; i < start + length - 1; i++)
            {
                if (buffer[i] == 0xFF && buffer[i + 1] == 0xD8)
                {
                    return i;
                }
            }
            return -1;
        }

        protected virtual void OnFrameReceived(FrameReceivedEventArgs e)
        {
            FrameReceived?.Invoke(this, e);
        }

        protected virtual void OnErrorOccurred(ErrorEventArgs e)
        {
            ErrorOccurred?.Invoke(this, e);
        }

        public void Dispose()
        {
            Stop();
            _httpClient?.Dispose();
        }
    }

    /// <summary>
    /// 프레임 수신 이벤트 인자
    /// </summary>
    public class FrameReceivedEventArgs : EventArgs
    {
        public Image Frame { get; }

        public FrameReceivedEventArgs(Image frame)
        {
            Frame = frame;
        }
    }
}
