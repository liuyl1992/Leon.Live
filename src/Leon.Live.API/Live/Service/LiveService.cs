using CliWrap;
using Leon.VideoStream;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Leon.Live.API
{
    public interface ILiveService
    {
        Task<Stream> GetVideoAsync();
        Task GetRealTimeVideoAsync();
        void GetStrViewRtmp(string strRtsp, out string outrtmpLink, out string outFlvLink, out string outHlsLink, string group = "", string token = "");
        (string OutrtmpLink, string OutFlvLink, string OutHlsLink, string RtmpPushAdr) ConvertLinkPath(string hashRtsp, string group = "default");
    }

    public class LiveService : ILiveService, ISingletonDependency
    {
        private static List<int> TaskIdList = new List<int>();
        private HttpClient _client;
        private readonly ISRSRemoting _sRSRemoting;
        private readonly RedisClient _redisClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public LiveService(
            ISRSRemoting sRSRemoting
            , IConfiguration configuration
            , RedisClient redisClient
            , ILogger<LiveService> logger)
        {
            _sRSRemoting = sRSRemoting;
            _configuration = configuration;
            _redisClient = redisClient;
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

            var client = new VideoStreamClient(_configuration.GetValue<string>("ffmpeg:Path", "ffmpeg"));
            client.NewImageReceived += NewImageReceived;
            var task = client.StartFrameReaderAsync(inputSource, OutputImageFormat.Bmp, cancellationTokenSource.Token);

            client.NewImageReceived -= NewImageReceived;

            void NewImageReceived(byte[] imageData)
            {
                Stream stream = new MemoryStream(imageData);
                File.WriteAllBytes($@"{DateTime.Now.Ticks}.bmp", imageData);
            }
        }

        /// <summary>
        /// Exchange the RTSP address to the video address
        /// </summary>
        /// <param name="strRtsp">resp address
        /// All lowercase except the password
        /// 
        /// </param>
        /// <param name="outrtmpLink"></param>
        /// <param name="outFlvLink"></param>
        /// <param name="outHlsLink"></param>
        /// <param name="group">The RTSP address in the same group is unique</param>
        /// <param name="token"></param>
        public void GetStrViewRtmp(string strRtsp, out string outrtmpLink, out string outFlvLink, out string outHlsLink, string group = "default", string token = "")
        {
            string hashRtsp = EncryptHelper.Sha256(strRtsp + group);

            var convertLink = ConvertLinkPath(hashRtsp, group);

            var _rtmpPushAdr = $"{convertLink.RtmpPushAdr}?token={token}";
            outrtmpLink = $"{convertLink.OutrtmpLink}?token={token}";
            outFlvLink = $"{convertLink.OutFlvLink}?token={token}";
            outHlsLink = $"{convertLink.OutHlsLink}?token={token}";

            _logger.LogInformation($"[push stream address] --> {outrtmpLink}");

            var task = Task.Factory.StartNew(() =>
             {
                 try
                 {
                     System.Diagnostics.Process process = new System.Diagnostics.Process();
                     process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                     process.StartInfo.FileName = _configuration.GetValue<string>("ffmpeg:Path", "ffmpeg");

                     var argumentFirst = _configuration.GetValue<string>("ffmpeg:ArgumentFirst", " -r 5 ");
                     var argumentSecond = _configuration.GetValue<string>("ffmpeg:ArgumentSecond", " -vcodec copy ");
                     var argumentLast = _configuration.GetValue<string>("ffmpeg:ArgumentLast", "");

                     var arguments = $"""{argumentFirst} -i "{strRtsp}" {argumentSecond} -f flv rtmp://{_rtmpPushAdr}{argumentLast}""";
                     _logger.LogInformation($"[GetStrViewRtmp] ffmpeg command= {arguments}");
                     process.StartInfo.Arguments = arguments;//reference http://www.wisestudy.cn/opentech/FFmpeg_send_streaming_media.html
                     process.StartInfo.UseShellExecute = false;
                     process.StartInfo.RedirectStandardInput = true;
                     process.StartInfo.RedirectStandardOutput = true;
                     process.StartInfo.RedirectStandardError = true;
                     process.StartInfo.CreateNoWindow = false;//true:Process依托在当前主进程下，主进程销毁都销毁
                     process.Start();
                     ProcessManager.ProcessIdDic.TryAdd(hashRtsp, process.Id);
                     _logger.LogInformation($"[GetStrViewRtmp] rtsp={hashRtsp}--hashRtsp={hashRtsp}");
                     Console.WriteLine($"{process.Id}");

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
                Console.WriteLine($"[Error] {failedTask?.Exception?.Message}");
            },
            TaskContinuationOptions.OnlyOnFaulted);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="hashRtsp"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public (string OutrtmpLink, string OutFlvLink, string OutHlsLink, string RtmpPushAdr) ConvertLinkPath(string hashRtsp, string group = "default")
        {
            var mediaPushAddr = _configuration.GetValue<string>("SRS:PushServer");//for example ：srs;rtsp simple server...
            var httpPort = _configuration.GetValue<int>("SRS:HttpPort", 8080);
            var serverType = "srs";
            string outPutPath = $"{serverType}/{group}/live/{hashRtsp}";//{Guid.NewGuid();
            if (string.IsNullOrWhiteSpace(group))
            {
                outPutPath = $"{serverType}/default/live/{hashRtsp}";//{Guid.NewGuid()
            }

            var _rtmpPushAdr = $"{mediaPushAddr}/{outPutPath}";
            var outrtmpLink = $"rtmp://{_rtmpPushAdr}";
            var outFlvLink = $"http://{mediaPushAddr}:{httpPort}/{outPutPath}.flv";
            var outHlsLink = $"http://{mediaPushAddr}:{httpPort}/{outPutPath}.m3u8";
            return (outrtmpLink, outFlvLink, outHlsLink, _rtmpPushAdr);
        }
        ~LiveService()
        {
            if (_client != null)
                _client.Dispose();
        }
    }
}
