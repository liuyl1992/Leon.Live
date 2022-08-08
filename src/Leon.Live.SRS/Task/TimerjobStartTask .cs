using Leon.Live.SRS;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using System.Linq;
using System.Threading;

public class TimerjobStartTask : IStartupTaskAsync
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly ISRSRemoting _sRSRemoting;
    private readonly IMemoryCache _memoryCache;
    public TimerjobStartTask(ILogger<TimerjobStartTask> logger,
        IConfiguration configuration,
        ISRSRemoting sRSRemoting,
       IMemoryCache memoryCache)
    {
        _logger = logger;
        _configuration = configuration;
        _sRSRemoting = sRSRemoting;
        _memoryCache = memoryCache;
    }
    public int Order => 0;

    public async Task ExecuteAsync()
    {
        await Task.Yield();
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(_configuration.GetValue<int>("IdleDuration", 10)));
        try
        {
            while (await timer.WaitForNextTickAsync())
            {
                await CheckInactiveStreams();
            }
        }

        catch (OperationCanceledException)
        {
            Console.WriteLine("【定时任务被取消】Operation cancelled");
        }
    }

    /// <summary>
    /// Check for inactive video streams
    /// </summary>
    /// <returns></returns>
    private async Task CheckInactiveStreams()
    {
        var streams = await _sRSRemoting.GetStreamsBySRSAsync();
        var onlyPushClients = streams.Streams.Where(s => s.Clients < 2 || s.Video == null).Select(s => s.Name).ToList();//Less than 2 means only push stream client

        var hashRtspKey = onlyPushClients.Select(name =>
        {
            //"name": "/live/26F3570DE6B9A31A19BF72FF74DE8712A81A75ABB71DD1BA0A902DB78F4991DB"
            var index = name.LastIndexOf('/');
            return name.Substring(index + 1, name.Length - index - 1);
        });

        foreach (var item in hashRtspKey)
        {
            //if (ProcessManager.ProcessIdDic.TryGetValue(item, out int value))//key is hash for rtsp,value is ffmpeg processId
            if (_memoryCache.TryGetValue(item, out int value))
            {
                try
                {
                    _memoryCache.Remove(item);
                    var process = Process.GetProcessById(value);
                    process.Kill();
                    process.WaitForExit();
                    process.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"ProcessId={value} not found;message={ex.Message}");
                }

                _logger.LogWarning($"ffmpeg process has been killed;processId={value};hashRtsp={item}");
            }
        }
    }
}
