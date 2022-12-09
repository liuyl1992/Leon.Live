using WebApiClientCore;

namespace Leon.Live.ZLMedia
{
    public interface IZLMediaKitRemoting : IHttpApi
    {
        /// <summary>
        /// Secret is stored in the config.ini file
        /// The config.ini file path is /opt/media/conf secret 
        /// reference:https://github.com/ZLMediaKit/ZLMediaKit/wiki/MediaServer%E6%94%AF%E6%8C%81%E7%9A%84HTTP-API#12indexapiaddstreamproxy
        /// </summary>
        /// <param name="secret">api操作密钥(配置文件配置)，如果操作ip是127.0.0.1，则不需要此参数</param>
        /// <param name="vhost">添加的流的虚拟主机，例如__defaultVhost__</param>
        /// <param name="app">添加的流的应用名，例如srs/group</param>
        /// <param name="stream">添加的流的id名，例如test</param>
        /// <param name="url">拉流地址，例如rtsp://live.hkstv.hk.lxdns.com/live/hks2</param>
        /// <param name="retry_count">拉流重试次数，默认为-1无限重试</param>
        /// <param name="rtp_type">rtsp拉流时，拉流方式，0：tcp，1：udp，2：组播</param>
        /// <param name="timeout_sec">拉流超时时间，单位秒，float类型</param>
        /// <param name="enable_hls">是否转换成hls协议</param>
        /// <param name="enable_mp4">是否允许mp4录制</param>
        /// <param name="enable_rtsp">是否转rtsp协议</param>
        /// <param name="enable_rtmp">是否转rtmp/flv协议</param>
        /// <param name="enable_ts">是否转http-ts/ws-ts协议</param>
        /// <param name="enable_fmp4">是否转http-fmp4/ws-fmp4协议</param>
        /// <param name="enable_audio">转协议时是否开启音频</param>
        /// <param name="add_mute_audio">转协议时，无音频是否添加静音aac音频</param>
        /// <param name="mp4_save_path">mp4录制文件保存根目录，置空使用默认</param>
        /// <param name="mp4_max_second">mp4录制切片大小，单位秒</param>
        /// <param name="hls_save_path">hls文件保存保存根目录，置空使用默认</param>
        /// <returns></returns>
        [WebApiClientCore.Attributes.HttpGet("/index/api/addStreamProxy")]
        ITask<ZLMediaKitResponse> AddStreamProxyAsync(string secret, string vhost, string app,
           string stream, string url = "", int? retry_count = null, int? rtp_type = null, int? timeout_sec = null, bool? enable_hls = null,
           bool? enable_mp4 = null, bool? enable_rtsp = null, bool? enable_rtmp = null, bool? enable_ts = null, bool? enable_fmp4 = null, bool? enable_audio = null,
           bool? add_mute_audio = null, string? mp4_save_path = null, int? mp4_max_second = null, string? hls_save_path = null);
    }
}
