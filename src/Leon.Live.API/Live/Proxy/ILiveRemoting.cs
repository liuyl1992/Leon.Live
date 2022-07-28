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
}
