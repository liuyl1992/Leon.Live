using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Linq;
using System.Diagnostics;

namespace Leon.Live.API.Examples.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LiveController : ControllerBase
    {
        private readonly IHostEnvironment _hostEnvironment;
        private readonly ILogger<LiveController> _logger;
        private readonly ILiveRemoting _liveProxy;
        private readonly ISRSRemoting _sRSRemoting;
        private readonly ILiveService _liveService;
        private readonly IZLMediaKitRemoting _zLMediaKitRemoting;
        private readonly IOnvifService _onvifService;
        private readonly IConfiguration _configuration;

        public LiveController(IHostEnvironment hostEnvironment,
            ILogger<LiveController> logger,
            ILiveRemoting liveProxy,
            ISRSRemoting sRSRemoting,
            ILiveService liveService,
            IZLMediaKitRemoting zLMediaKitRemoting,
            IOnvifService onvifService,
            IConfiguration configuration
            )
        {
            _hostEnvironment = hostEnvironment;
            _logger = logger;
            _liveProxy = liveProxy;
            _sRSRemoting = sRSRemoting;
            _liveService = liveService;
            _zLMediaKitRemoting = zLMediaKitRemoting;
            _onvifService = onvifService;
            _configuration = configuration;
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
        ///  Exchange the RTSP address to the video address
        /// </summary>
        /// <param name="strRtsp"></param>
        ///  <param name="group"></param>
        /// <param name="token">auth token</param>
        /// <returns>
        /// {
        /// "rtmp": "rtmp://192.168.56.56/srs/live/E0F08248A3CBC3CA5D58F6C52E4DCC5E88CEC097D34FE81C8EFC7B60AC91DC06?token=",
        /// "flv": "http://192.168.56.56:8080/srs/live/E0F08248A3CBC3CA5D58F6C52E4DCC5E88CEC097D34FE81C8EFC7B60AC91DC06.flv?token=",
        /// "hls": "http://192.168.56.56:8080/srs/live/E0F08248A3CBC3CA5D58F6C52E4DCC5E88CEC097D34FE81C8EFC7B60AC91DC06.m3u8?token="
        /// }
        /// </returns>
        [HttpGet("video/srs/StrPlay")]
        public async Task<IActionResult> GetStrViewRtmpBySRS(string strRtsp, string group = "default", string token = "")
        {
            //TODO
            var streams = await _sRSRemoting.GetStreamsBySRSAsync();
            var onlyPushClients = streams.Streams.Where(s => s.Clients > 0).Select(s => s.Name).ToList();

            var hashRtspKey = onlyPushClients.Select(name =>
            {
                var index = name.LastIndexOf('/');
                return name.Substring(index + 1, name.Length - index - 1);
            });

            string hashRtsp = EncryptHelper.Sha256(strRtsp + group);//The RTSP in the same group is unique
            var convertLink = _liveService.ConvertLinkPath(hashRtsp, group);
            if (hashRtspKey.Where(s => s == hashRtsp).Any())
            {
                return Ok(new
                {
                    Rtmp = $"{convertLink.OutrtmpLink}?token={token}",
                    Flv = $"{convertLink.OutFlvLink}?token={token}",
                    Hls = $"{convertLink.OutHlsLink}?token={token}"
                });
            }

            //streams.Streams.Where(s=>s.Name==hashRtsp).FirstOrDefault()?
            _liveService.GetStrViewRtmp(strRtsp, out string outrtmpLink, out string outFlvLink, out string outHlsLink, group, token);
            return Ok(new { Rtmp = outrtmpLink, Flv = outFlvLink, Hls = outHlsLink });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strRtsp"></param>
        /// <param name="group"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet("video/ZLMediaKit/StrPlay")]
        public async Task<IActionResult> AddStreamPusherProxyAsync(string strRtsp, string group = "default", string token = "")
        {
            var result = await _zLMediaKitRemoting.AddStreamProxyAsync(secret: "035c73f7-bb6b-4889-a715-d9eb2d1925cc",
                 vhost: "__defaultVhost__",//$"{Process.GetCurrentProcess().ProcessName.Replace(".","")}",
                 app: $"ZLMediaKit/{group}/live",
                 stream: $"{EncryptHelper.Sha256(strRtsp + group)}",
                 url: strRtsp,
                 enable_hls: true,
                 enable_rtmp: true,
                 enable_audio: false
                 );

            // reference: url rule https://github.com/ZLMediaKit/ZLMediaKit/wiki/%E6%92%AD%E6%94%BEurl%E8%A7%84%E5%88%99

            var httpHost = _configuration.GetValue<string>("Remoting:IZLMediaKitRemoting:HttpHost");
            var index = result.Data.Key.IndexOf('/');
            var vhost = result.Data.Key.Substring(0, index - 1);
            var path = result.Data.Key.Substring(index + 1, result.Data.Key.Length - index - 1);
            return Ok(new
            {
                Rtmp = $"rtmp://{new Uri(httpHost).Host}/{path}?vhost={vhost}&token={token}",
                Flv = $"{httpHost}/{path}.live.flv?vhost={vhost}&token={token}",
                Hls = $"{httpHost}/{path}/hls.m3u8?vhost={vhost}&token={token}"
            });
        }
    }
}
