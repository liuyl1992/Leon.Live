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
        /// <param name="authToken">auth token</param>
        /// <returns>Rtmp;Flv;Hls</returns>
        [HttpGet("video/StrPlay")]
        public IActionResult GetStrViewRtmp(string strRtsp, string authToken = "")
        {
            //每次播放视频都用rtsp地址换rtmp
            //if 发现rtsp地址没有推过流，即进行推流换得rtmp；flv；m3u8地址
            // else 发现此rtsp有推过流，直接返回映射的rtmp；flv;m3u8地址
            _liveService.GetStrViewRtmp(strRtsp, out string outrtmpLink, out string outFlvLink, out string outHlsLink, authToken );
            return Ok(new {Rtmp= outrtmpLink,Flv= outFlvLink, Hls= outHlsLink});
        }
    }
}
