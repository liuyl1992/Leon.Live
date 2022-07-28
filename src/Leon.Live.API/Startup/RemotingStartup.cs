﻿using WebApiClientCore;
using WebApiClientCore.Serialization.JsonConverters;

namespace Leon.Live.API
{
    /// <summary>
    /// 远程调用
    /// </summary>
    //[ReplaceStartup("")]
    public class RemotingStartup : INetProStartup
    {
        /// <summary>
        /// 执行顺序
        /// </summary>
        public double Order { get; set; } = int.MaxValue;

        /// <summary>
        /// 服务注入
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="typeFinder"></param>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration = null, ITypeFinder typeFinder = null)
        {
            var section = configuration.GetSection($"Remoting:{nameof(ILiveRemoting)}");

            services.AddHttpApi<ILiveRemoting>().ConfigureHttpApi(section).ConfigureHttpApi(o =>
            {
                // 符合国情的不标准时间格式，有些接口就是这么要求必须不标准
                o.JsonSerializeOptions.Converters.Add(new JsonDateTimeConverter("yyyy-MM-dd HH:mm:ss"));
            });

            var srsSection = configuration.GetSection($"Remoting:{nameof(ISRSRemoting)}");

            services.AddHttpApi<ISRSRemoting>().ConfigureHttpApi(srsSection);
        }

        /// <summary>
        /// 请求管道配置
        /// </summary>
        /// <param name="application"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder application, IWebHostEnvironment env)
        {
        }
    }
}
