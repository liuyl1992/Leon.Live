using FreeSql;
using IdGen;
using IdGen.DependencyInjection;
using MySql.Data.MySqlClient;
using System.Data.SQLite;
using System.Text.RegularExpressions;
using DataType = FreeSql.DataType;

namespace Leon.Live.ZLMedia
{
    public class ZLMediaDBStartup : INetProStartup
    {
        public double Order { get; set; } = int.MaxValue;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="typeFinder"></param>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration = null, ITypeFinder typeFinder = null)
        {
            //primary key init
            services.AddIdGen(Convert.ToInt32(DateTimeOffset.Now.ToUnixTimeMilliseconds() % 1024));

            #region Freesql初始化
            var idleBus = new IdleBus<IFreeSql>(TimeSpan.FromDays(365));
            idleBus.Register(Globals.DB_KEY_ZL, () =>
            {
                var sqlConnectionString = configuration.GetConnectionString("ZLMediaConnection");

                using (var connection = new SQLiteConnection(sqlConnectionString))
                {
                    connection.Open();
                }
                return new FreeSqlBuilder()
             .UseConnectionString(DataType.Sqlite, sqlConnectionString)
             .UseAutoSyncStructure(true)
             .Build();
            });

            services.AddSingleton(idleBus);

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="application"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder application, IWebHostEnvironment env)
        {
        }
    }

    internal static class Globals
    {
        public const string DB_KEY_ZL = "zl";
    }
}
