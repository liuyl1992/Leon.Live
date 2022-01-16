using System.IO;
using WebApiClientCore;

namespace Leon.Live.API
{
    public interface ILiveProxy
    {
        [WebApiClientCore.Attributes.HttpGet("https://anthonygiretti.blob.core.windows.net/videos/earth.mp4")]
        //[ApiClientFilter]
        //ApiClientFilter内部有读取body操作，避免多次读取报错，注释ApiClientFilter
        ITask<Stream> GetVideoAsync();
    }
}
