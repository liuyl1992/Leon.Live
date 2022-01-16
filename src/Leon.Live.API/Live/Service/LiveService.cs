using Leon.VideoStream;
using System.IO;
using System.Net.Http;
using System.Threading;

namespace Leon.Live.API
{
    public interface ILiveService
    {
        Task<Stream> GetVideoAsync();
        Task GetRealTimeVideoAsync();
    }

    public class LiveService : ILiveService
    {
        private HttpClient _client;

        public LiveService()
        {
            _client = new HttpClient();
        }

        public async Task<Stream> GetVideoAsync()
        {
            var urlBlob = "https://anthonygiretti.blob.core.windows.net/videos/earth.mp4";

            return await _client.GetStreamAsync(urlBlob);
        }

        public async Task GetRealTimeVideoAsync()
        {
            //webcam
            var inputSource = new WebcamInputSource("Lenovo EasyCamera");

            var cancellationTokenSource = new CancellationTokenSource();

            var client = new VideoStreamClient("D:/ffmpeg-5.0/bin/ffmpeg.exe");
            client.NewImageReceived += NewImageReceived;
            var task = client.StartFrameReaderAsync(inputSource, OutputImageFormat.Bmp, cancellationTokenSource.Token);

            client.NewImageReceived -= NewImageReceived;

            void NewImageReceived(byte[] imageData)
            {
                Stream stream = new MemoryStream(imageData);
                File.WriteAllBytes($@"{DateTime.Now.Ticks}.bmp", imageData);
            }
        }

        ~LiveService()
        {
            if (_client != null)
                _client.Dispose();
        }
    }
}
