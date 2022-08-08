using System.Linq;

namespace Leon.Live.SRS
{
    [ApiController]
    [Route("[controller]")]
    public class MediaController : ControllerBase
    {
        private readonly IHostEnvironment _hostEnvironment;
        private readonly ILogger<MediaController> _logger;
        private readonly IZLMediaKitRemoting _zLMediaKitRemoting;
        private readonly IConfiguration _configuration;

        public MediaController(IHostEnvironment hostEnvironment,
            ILogger<MediaController> logger,
            IZLMediaKitRemoting zLMediaKitRemoting,
            IConfiguration configuration
            )
        {
            _hostEnvironment = hostEnvironment;
            _logger = logger;
            _zLMediaKitRemoting = zLMediaKitRemoting;
            _configuration = configuration;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strRtsp"></param>
        /// <param name="group"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet("video/StrPlay")]
        public async Task<IActionResult> AddStreamPusherProxyAsync(string strRtsp, string group = "default", string token = "")
        {
            var result = await _zLMediaKitRemoting.AddStreamProxyAsync(secret: _configuration.GetValue<string>("IZLMediaKitRemoting:Secret"),
                 vhost: "__defaultVhost__",
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
