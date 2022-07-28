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
        void GetStrViewRtmp(string strRtsp, out string outrtmpLink, out string outFlvLink, out string outHlsLink, string authToken = "");
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task GetRealTimeVideoAsync()
        {
            //webcam
            var inputSource = new WebcamInputSource("Lenovo EasyCamera");

            var cancellationTokenSource = new CancellationTokenSource();

            var client = new VideoStreamClient("ffmpeg");
            client.NewImageReceived += NewImageReceived;
            var task = client.StartFrameReaderAsync(inputSource, OutputImageFormat.Bmp, cancellationTokenSource.Token);

            client.NewImageReceived -= NewImageReceived;

            void NewImageReceived(byte[] imageData)
            {
                Stream stream = new MemoryStream(imageData);
                File.WriteAllBytes($@"{DateTime.Now.Ticks}.bmp", imageData);
            }
        }

        public void GetStrViewRtmp(string strRtsp, out string outrtmpLink, out string outFlvLink, out string outHlsLink, string authToken="")
        {
            //rtsp 唯一
            var mediaPushAddr = _configuration.GetValue<string>("SRS:PushServer");//for example ：srs;rtsp simple server...
            var httpPort = _configuration.GetValue<string>("SRS:HttpPort");
            var serverType = "srs";
            var outPutPath= $"live/{Guid.NewGuid()}";
            var _rtmpPushAdr = $"{mediaPushAddr}/{serverType}/{outPutPath}?authtoken={authToken}";
            outrtmpLink = $"rtmp://{_rtmpPushAdr}";
            outFlvLink = $"http://{mediaPushAddr}:{httpPort}/{serverType}/{outPutPath}.flv?authtoken={authToken}";
            outHlsLink = $"http://{mediaPushAddr}:{httpPort}/{serverType}/{outPutPath}.m3u8?authtoken={authToken}";
            _logger.LogInformation($"[push stream address] --> {outrtmpLink}");
            var task = Task.Factory.StartNew(() =>
             {
                 try
                 {
                     System.Diagnostics.Process process = new System.Diagnostics.Process();
                     process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                     process.StartInfo.FileName = "ffmpeg";
                     process.StartInfo.Arguments = $"""-i "{strRtsp}" -r 5 -c copy -f flv rtmp://{_rtmpPushAdr}""";
                     process.StartInfo.UseShellExecute = false;
                     process.StartInfo.RedirectStandardInput = true;
                     process.StartInfo.RedirectStandardOutput = true;
                     process.StartInfo.RedirectStandardError = true;
                     process.StartInfo.CreateNoWindow = false;//true:Process依托在当前主进程下，主进程销毁都销毁
                     process.Start();
                     Console.WriteLine($"{process}");

                     process.BeginOutputReadLine();
                     process.BeginErrorReadLine();

                     process.OutputDataReceived += (ss, ee) =>
                      {
                          _logger.LogDebug($"Std:{ee.Data}");
                      };

                     process.ErrorDataReceived += (ss, ee) =>
                      {
                          if (ee.Data?.Contains("Cannot open connection") ?? false)
                          {
                              _logger.LogError($"Cannot open connection");
                          }
                          _logger.LogDebug($"Er:{ee.Data}");
                      };

                     //wait exit
                     process.WaitForExit();
                     if (!process.HasExited)
                     {
                         process.Kill();
                     }

                     //close process
                     process.Dispose();
                     _logger.LogWarning("EXIT!");
                 }
                 catch (Exception ex)
                 {
                     Console.WriteLine(ex.Message);
                     throw;
                 }
             });//, TaskCreationOptions.LongRunning
            task.ContinueWith(failedTask =>
            {
                Console.WriteLine($"{failedTask?.Exception?.Message}");
            },
            TaskContinuationOptions.OnlyOnFaulted);
        }

        ~LiveService()
        {
            if (_client != null)
                _client.Dispose();
        }
    }
}
