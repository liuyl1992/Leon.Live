using Leon.Live.API.Live;
using System.IO;
using WebApiClientCore;

namespace Leon.Live.API
{
    public interface ILiveRemoting: IHttpApi
    {
        [WebApiClientCore.Attributes.HttpGet("https://anthonygiretti.blob.core.windows.net/videos/earth.mp4")]
        //[ApiClientFilter]
        //ApiClientFilter内部有读取body操作，避免多次读取报错，注释ApiClientFilter
        ITask<System.IO.Stream> GetVideoAsync();

    }

    public interface ISRSRemoting : IHttpApi
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [WebApiClientCore.Attributes.HttpGet("/api/v1/streams/{id}")]
        ITask<StreamResponse> GetStreamsBySRSAsync(string id = "");

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [WebApiClientCore.Attributes.HttpGet("/api/v1/clients/{id}")]
        ITask<ClientsResponse> GetClientsBySRSAsync(string id = "");
    }

    public interface IZLMediaKitRemoting : IHttpApi
    {
        /// <summary>
        /// Secret is stored in the config.ini file
        /// The config.ini file path is /opt/media/conf secret 
        /// reference:https://github.com/ZLMediaKit/ZLMediaKit/wiki/MediaServer%E6%94%AF%E6%8C%81%E7%9A%84HTTP-API#12indexapiaddstreamproxy
        /// </summary>
        /// <returns></returns>
        [WebApiClientCore.Attributes.HttpGet("/index/api/addStreamProxy")]
        ITask<ZLMediaKitResponse> AddStreamProxyAsync(string secret,string schema,string vhost, string app,
            string? rtp_type=null,string? timeout_sec=null,string? retry_count=null);

    }
}
