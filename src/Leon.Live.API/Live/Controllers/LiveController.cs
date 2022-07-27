using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace Leon.Live.API.Examples.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LiveController : ControllerBase
    {
        private readonly IHostEnvironment _hostEnvironment;
        private readonly ILogger<LiveController> _logger;
        private readonly ILiveProxy _liveProxy;
        private readonly ILiveService _liveService;
        private readonly IOnvifService _onvifService;

        public LiveController(IHostEnvironment hostEnvironment,
            ILogger<LiveController> logger,
            ILiveProxy liveProxy,
            ILiveService liveService,
            IOnvifService onvifService
            )
        {
            _hostEnvironment = hostEnvironment;
            _logger = logger;
            _liveProxy = liveProxy;
            _liveService = liveService;
            _onvifService = onvifService;
        }

        /// <summary>
        /// get video real-time stream
        /// </summary>
        /// <returns></returns>
        [HttpGet()]
        public async Task<FileStreamResult> Get()
        {
            //reference https://anthonygiretti.com/2018/01/16/streaming-video-asynchronously-in-asp-net-core-2-with-web-api/
            var stream = await _liveProxy.GetVideoAsync();
            return new FileStreamResult(stream, "video/mp4");
        }

        /// <summary>
        /// RtspToRtmpAsync
        /// </summary>
        /// <param name="rtsp"></param>
        /// <param name="name"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        //[HttpGet("RtspToRtmp")]
        //public async Task RtspToRtmpAsync(string rtsp, string name, string pwd)
        //{
        //    var serverUri = new Uri(rtsp);
        //    var credentials = new NetworkCredential(name, pwd);
        //    var connectionParameters = new ConnectionParameters(serverUri, credentials);
        //    var cancellationTokenSource = new CancellationTokenSource();

        //    Task connectTask = ConnectAsync(connectionParameters, cancellationTokenSource.Token);

        //    //cancellationTokenSource.Cancel();

        //    Console.WriteLine("Canceling");
        //    connectTask.Wait(CancellationToken.None);
        //}

        /// <summary>
        /// get video real-time stream
        /// </summary>
        /// <returns></returns>
        [HttpGet("video/realtime")]
        public async Task<IActionResult> RealtimeVideo()
        {
            /* ffmpeg常用命令：
             * 1、枚举采集设备和采集数据
             * ffmpeg -list_devices true -f dshow -i dummy 
             * 2、
             */
            await _liveService.GetRealTimeVideoAsync();
            return Ok();
        }

        /// <summary>
        /// 播流地址
        /// </summary>
        /// <param name="strRtsp"></param>
        /// <param name="type">rtmp;flv;m3u8</param>
        /// <returns></returns>
        [HttpGet("video/StrPlay")]
        public async Task<IActionResult> GetStrViewRtmp(string strRtsp, string type = "rtmp")
        {
            //每次播放视频都用rtsp地址换rtmp
            //if 发现rtsp地址没有推过流，即进行推流换得rtmp；flv；m3u8地址
            // else 发现此rtsp有推过流，直接返回映射的rtmp；flv;m3u8地址
            _liveService.GetStrViewRtmp(strRtsp, out string outPutLink, type = "rtmp");
            return Ok(outPutLink);
        }

        //private static async Task ConnectAsync(ConnectionParameters connectionParameters, CancellationToken token)
        //{
        //    try
        //    {
        //        TimeSpan delay = TimeSpan.FromSeconds(5);

        //        using (var rtspClient = new RtspClient(connectionParameters))
        //        {
        //            rtspClient.FrameReceived +=
        //                (sender, frame) => Console.WriteLine($"New frame {frame.Timestamp}: {frame.GetType().Name}");

        //            while (true)
        //            {
        //                Console.WriteLine("Connecting...");

        //                try
        //                {
        //                    await rtspClient.ConnectAsync(token);
        //                }
        //                catch (OperationCanceledException)
        //                {
        //                    return;
        //                }
        //                catch (RtspClientException e)
        //                {
        //                    Console.WriteLine(e.ToString());
        //                    await Task.Delay(delay, token);
        //                    continue;
        //                }

        //                Console.WriteLine("Connected.");

        //                try
        //                {
        //                    await rtspClient.ReceiveAsync(token);
        //                }
        //                catch (OperationCanceledException)
        //                {
        //                    return;
        //                }
        //                catch (RtspClientException e)
        //                {
        //                    Console.WriteLine(e.ToString());
        //                    await Task.Delay(delay, token);
        //                }
        //            }
        //        }
        //    }
        //    catch (OperationCanceledException)
        //    {
        //    }
        //}

    }
}
