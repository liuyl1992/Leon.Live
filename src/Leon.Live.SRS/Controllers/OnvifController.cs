using System.Collections.Generic;

namespace Leon.Live.SRS
{
    [ApiController]
    [Route("[controller]")]
    public class OnvifController : ControllerBase
    {
        private readonly IHostEnvironment _hostEnvironment;
        private readonly ILogger<OnvifController> _logger;
        private readonly IOnvifService _onvifService;

        public OnvifController(IHostEnvironment hostEnvironment,
            ILogger<OnvifController> logger,
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
