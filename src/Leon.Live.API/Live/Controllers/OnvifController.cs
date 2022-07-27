using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace Leon.Live.API.Examples.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OnvifController : ControllerBase
    {
        private readonly IHostEnvironment _hostEnvironment;
        private readonly ILogger<LiveController> _logger;
        private readonly IOnvifService _onvifService;

        public OnvifController(IHostEnvironment hostEnvironment,
            ILogger<LiveController> logger,
            IOnvifService onvifService
            )
        {
            _hostEnvironment = hostEnvironment;
            _logger = logger;
            _onvifService = onvifService;
        }

        /// <summary>
        /// DiscoverCameras
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="name"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        [HttpGet("DiscoverCameras")]
        public async Task<List<string>> DiscoverCamerasByOnvifAsync(string ip, string name, string pwd)
        {
            var result = await _onvifService.DiscoverCamerasAsync(ip, name, pwd);
            return result;
        }
    }
}
