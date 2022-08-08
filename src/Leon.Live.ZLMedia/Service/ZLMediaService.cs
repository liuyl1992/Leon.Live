//using Leon.VideoStream;
using Leon.VideoStream;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;

namespace Leon.Live.ZLMedia
{
    public interface IZLMediaService
    {
        (string OutrtmpLink, string OutFlvLink, string OutHlsLink, string RtmpPushAdr) ConvertLinkPath(string hashRtsp, string group = "default");
    }

    public class ZLMediaService : IZLMediaService, IScopedDependency
    {
        private static List<int> TaskIdList = new List<int>();
        private IHttpClientFactory _client;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly IMemoryCache _memoryCache;

        public ZLMediaService(
            IHttpClientFactory client
            , IConfiguration configuration
            , ILogger<ZLMediaService> logger,
            IMemoryCache memoryCache)
        {
            _configuration = configuration;
            _client = client;
            _logger = logger;
            _memoryCache = memoryCache;
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
