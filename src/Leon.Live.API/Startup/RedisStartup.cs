

namespace Leon.Live.API
{
    /// <summary>
    /// redis
    /// </summary>
    //[ReplaceStartup("")]
    public class RedisStartup : INetProStartup
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
            var connectionString = configuration.GetSection("RedisOption").Get<string>();
            var redisClient = new RedisClient(connectionString);
            redisClient.Serialize = obj => JsonConvert.SerializeObject(obj);
            redisClient.Deserialize = (json, type) => JsonConvert.DeserializeObject(json, type);
            services.AddMemoryCache();
            services.TryAddSingleton(redisClient);

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
