using Leon.Live.API;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections.Concurrent;

public class TimerjobStartTask : IStartupTaskAsync
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly ISRSRemoting _sRSRemoting;
    public TimerjobStartTask(ILogger<TimerjobStartTask> logger, IConfiguration configuration, ISRSRemoting sRSRemoting)
    {
        _logger = logger;
        _configuration = configuration;
        _sRSRemoting = sRSRemoting;
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
        var onlyPushClients = streams.Streams.Where(s => s.Clients < 2).Select(s => s.Name).ToList();//Less than 2 means only push stream client

        var hashRtspKey = onlyPushClients.Select(name =>
        {
            //"name": "/live/26F3570DE6B9A31A19BF72FF74DE8712A81A75ABB71DD1BA0A902DB78F4991DB"
            var index = name.LastIndexOf('/');
            return name.Substring(index + 1, name.Length - index - 1);
        });

        foreach (var item in hashRtspKey)
        {
            if (ProcessManager.ProcessIdDic.TryGetValue(item, out int value))//key is hash for rtsp,value is ffmpeg processId
            {
                try
                {
                    var process = Process.GetProcessById(value);
                    process.Kill();
                    process.WaitForExit();
                    process.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ProcessId={value} not found;message={ex.Message}");
                }
                finally
                {
                    ProcessManager.ProcessIdDic.Remove(item, out int vale);
                }

                _logger.LogWarning($"ffmpeg process has been killed;processId={value};hashRtsp={item}");
            }
        }
    }
}
public static class ProcessManager
{
    /// <summary>
    /// key is hash,value is processId
    /// </summary>
    public static ConcurrentDictionary<string, int> ProcessIdDic = new ConcurrentDictionary<string, int>();
}
