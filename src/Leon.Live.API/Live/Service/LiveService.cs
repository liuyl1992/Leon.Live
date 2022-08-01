using Leon.VideoStream;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
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

    public class LiveService : ILiveService, IScopedDependency
    {
        private static List<int> TaskIdList = new List<int>();
        private IHttpClientFactory _client;
        private readonly ISRSRemoting _sRSRemoting;
        private readonly RedisClient _redisClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly IMemoryCache _memoryCache;

        public LiveService(
            IHttpClientFactory client,
            ISRSRemoting sRSRemoting
            , IConfiguration configuration
            , RedisClient redisClient
            , ILogger<LiveService> logger,
            IMemoryCache memoryCache)
        {
            _sRSRemoting = sRSRemoting;
            _configuration = configuration;
            _redisClient = redisClient;
            _client = client;
            _logger = logger;
            _memoryCache = memoryCache;
        }

        public async Task<Stream> GetVideoAsync()
        {
            var urlBlob = "https://anthonygiretti.blob.core.windows.net/videos/earth.mp4";
            return await _client.CreateClient().GetStreamAsync(urlBlob);
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

            _logger.LogInformation($"[push stream address] --> {_rtmpPushAdr}");

            var task = Task.Factory.StartNew(() =>
            {
                var traceId = Guid.NewGuid().ToString();
                _logger.LogDebug($"traceId={traceId} [1]Has entered the task block ");
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                try
                {
                    //process.StartInfo.UserName = hashRtsp;
                    process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    process.StartInfo.FileName = _configuration.GetValue<string>("ffmpeg:Path", "ffmpeg");

                    var argumentFirst = _configuration.GetValue<string>("ffmpeg:ArgumentFirst", " -r 5 ");
                    var argumentSecond = _configuration.GetValue<string>("ffmpeg:ArgumentSecond", " -vcodec copy ");
                    var argumentLast = _configuration.GetValue<string>("ffmpeg:ArgumentLast", "");

                    var arguments = $"""{argumentFirst} -i "{strRtsp}" {argumentSecond} -f flv rtmp://{_rtmpPushAdr}/{argumentLast}""";
                    _logger.LogDebug($"traceId={traceId} [2][GetStrViewRtmp] ffmpeg command={process.StartInfo.FileName} {arguments}");
                    process.StartInfo.Arguments = arguments;//reference http://www.wisestudy.cn/opentech/FFmpeg_send_streaming_media.html
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardInput = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.CreateNoWindow = false;
                    process.Start();

                    _memoryCache.Set<int>(hashRtsp, process.Id);
                    //ProcessManager.ProcessIdDic.TryAdd(hashRtsp, process.Id);
                    _logger.LogDebug($"traceId={traceId} [3] has been registered processId; rtsp={hashRtsp}--hashRtsp={hashRtsp}");
                    Console.WriteLine($"process.Id={process.Id}; process.MachineName={process.MachineName}");

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    process.OutputDataReceived += (ss, ee) =>
                     {
                         _logger.LogDebug($"Std:{ee.Data}");
                     };

                    process.ErrorDataReceived += (ss, ee) =>
                     {
                         _logger.LogDebug($"Er:{ee.Data}");
                     };

                    //wait exit
                    process.WaitForExit();
                    if (!process.HasExited)
                    {
                        _logger.LogDebug($"traceId={traceId} [4] process has killed;processId={process?.Id}");
                        process.Kill();
                    }

                    //close process
                    process.Dispose();
                    _logger.LogWarning($"traceId={traceId} [5]EXIT!  processId={process?.Id}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"{ex.Message};strRtsp={strRtsp}");
                    throw;
                }
                finally
                {
                    process.Dispose();
                    _memoryCache.Remove(hashRtsp);
                    //ProcessManager.ProcessIdDic.Remove(hashRtsp, out int vale);
                }
            }, TaskCreationOptions.LongRunning);
            //task.ContinueWith(failedTask =>
            //{
            //    _logger.LogError(failedTask?.Exception,$"{failedTask?.Exception?.Message}");
            //},
            //TaskContinuationOptions.OnlyOnFaulted);
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
                outPutPath = $"{serverType}/default/live/{hashRtsp}";
            }

            var pushPort = _configuration.GetValue<int>("SRS:PushPort");
            var _rtmpPushAdr = $"{mediaPushAddr}:{pushPort}/{outPutPath}";
            var outrtmpLink = $"rtmp://{_rtmpPushAdr}";
            var outFlvLink = $"http://{mediaPushAddr}:{httpPort}/{outPutPath}.flv";
            var outHlsLink = $"http://{mediaPushAddr}:{httpPort}/{outPutPath}.m3u8";
            return (outrtmpLink, outFlvLink, outHlsLink, _rtmpPushAdr);
        }
    }
}
