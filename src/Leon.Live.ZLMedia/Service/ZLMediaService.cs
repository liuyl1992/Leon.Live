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
        Task<int> on_server_keepaliveAsync(dynamic data);
    }

    public class ZLMediaService : IZLMediaService, IScopedDependency
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly IdleBus<IFreeSql> _busFsql;

        public ZLMediaService(
             IConfiguration configuration
            , ILogger<ZLMediaService> logger
            , IdleBus<IFreeSql> busFsql)
        {
            _configuration = configuration;
            _logger = logger;
            _busFsql = busFsql;
        }
        public async Task<int> on_server_keepaliveAsync(dynamic data)
        {
            var serverId= data.data.mediaServerId;
            var freql = _busFsql.Get(Globals.DB_KEY_ZL);
            var effectCount =await freql.Insert<On_server_keepalive>(new On_server_keepalive { }).ExecuteAffrowsAsync();
            return effectCount;
        }
    }
}
