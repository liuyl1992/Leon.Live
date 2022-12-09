using System.Linq;
using System.Text.Json;

namespace Leon.Live.ZLMedia
{
    [ApiController]
    [Route("ZLMedia/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly IHostEnvironment _hostEnvironment;
        private readonly ILogger<WebhookController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IZLMediaService _zLMediaServicel;

        public WebhookController(IHostEnvironment hostEnvironment,
            ILogger<WebhookController> logger,
            IConfiguration configuration,
            IZLMediaService zLMediaServicel
            )
        {
            _hostEnvironment = hostEnvironment;
            _logger = logger;
            _configuration = configuration;
            _zLMediaServicel = zLMediaServicel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data">https://github.com/zlmediakit/ZLMediaKit/wiki/MediaServer%E6%94%AF%E6%8C%81%E7%9A%84HTTP-HOOK-API#16on_server_keepalive</param>
        /// <returns></returns>
        [HttpPost("on_server_keepalive")]
        public async Task<IActionResult> on_server_keepalive(dynamic data)
        {
            var dataDynamic = JsonSerializer.Deserialize<dynamic>(data);
            var effectCount = _zLMediaServicel.on_server_keepaliveAsync(dataDynamic);
            return Ok(effectCount);
        }
    }
}
