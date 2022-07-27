using CliWrap;
using Leon.VideoStream;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace Leon.Live.API
{
    public interface ILiveService
    {
        Task<Stream> GetVideoAsync();
        Task GetRealTimeVideoAsync();
        void GetStrViewRtmp(string strRtsp, out string outPutLink, string type = "rtmp");
    }

    public class LiveService : ILiveService, IScopedDependency
    {
        private HttpClient _client;
        private IConfiguration _configuration;
        private ILogger _logger;

        public LiveService(IConfiguration configuration
            , ILogger<LiveService> logger)
        {
            _configuration = configuration;
            _client = new HttpClient();
            _logger = logger;
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

        public void GetStrViewRtmp(string strRtsp, out string outPutLink, string type = "rtmp")
        {
            //rtsp 唯一
            outPutLink = _configuration.GetValue<string>("srs:push");
            string outPutLinkTemp = outPutLink;
            var task = Task.Factory.StartNew(() =>
             {
                 try
                 {
                 //System.Diagnostics.Process.GetCurrentProcess(); 
                 System.Diagnostics.Process process = new System.Diagnostics.Process();
                 process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                 process.StartInfo.FileName = "ffmpeg";
                 process.StartInfo.Arguments = $"""-i "{strRtsp}" -r 5 -c copy -f flv {outPutLinkTemp}/srs/live/{Guid.NewGuid()}""";
                 process.StartInfo.UseShellExecute = false;
                 process.StartInfo.RedirectStandardInput = true;
                 process.StartInfo.RedirectStandardOutput = true;
                 process.StartInfo.RedirectStandardError = true;
                 process.StartInfo.CreateNoWindow = false;
                 process.Start();

                 process.BeginOutputReadLine();
                 process.BeginErrorReadLine();

                 process.OutputDataReceived += (ss, ee) =>
                  {
                      _logger.LogTrace($"{ee.Data}");
                  };

                 process.ErrorDataReceived += (ss, ee) =>
                  {
                      if (ee.Data?.Contains("Cannot open connection") ?? false)
                      {
                          _logger.LogError($"Cannot open connection");
                      }
                      _logger.LogTrace($"{ee.Data}");
                  };

                 //等待退出
                 process.WaitForExit();

                 //关闭进程
                 process.Dispose();
                 _logger.LogWarning("EXIT!");
                 }
                 catch (Exception ex)
                 {
                     Console.WriteLine(ex.Message);
                     throw;
                 }
             }, TaskCreationOptions.LongRunning);
            //task.ContinueWith(failedTask =>
            //{
            //    Console.WriteLine("异常");
            //},
            //TaskContinuationOptions.OnlyOnFaulted);
        }

        ~LiveService()
        {
            if (_client != null)
                _client.Dispose();
        }
    }
}
