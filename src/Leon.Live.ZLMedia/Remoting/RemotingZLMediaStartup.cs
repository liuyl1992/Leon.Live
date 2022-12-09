using WebApiClientCore.Serialization.JsonConverters;

namespace Leon.Live.ZLMedia
{
    /// <summary>
    /// 远程调用
    /// </summary>
    public class RemotingZLMediaStartup : INetProStartup
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
            var section = configuration.GetSection($"Remoting:{nameof(IZLMediaKitRemoting)}");


            var zLMediaKitSection = configuration.GetSection($"Remoting:{nameof(IZLMediaKitRemoting)}");

            services.AddHttpApi<IZLMediaKitRemoting>().ConfigureHttpApi(zLMediaKitSection);
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
