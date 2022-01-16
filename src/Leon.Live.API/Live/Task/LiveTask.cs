
using Leon.VideoStream;
using System.IO;
using System.Threading;

namespace Leon.Live.API
{
    public class LiveTask : IStartupTask
    {
        public int Order => 0;

        public void Execute()
        {
            //RTSP
            var rtspinputSource = new StreamInputSource("rtsp://184.72.239.149/vod/mp4://BigBuckBunny_175k.mov");

            var rtspcancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            var rtspclient = new VideoStreamClient("D:/ffmpeg-5.0/bin/ffmpeg.exe");
            rtspclient.NewImageReceived += RTSPNewImageReceived;
            var rtsptask = rtspclient.StartFrameReaderAsync(rtspinputSource, OutputImageFormat.Bmp, rtspcancellationTokenSource.Token);
            rtspclient.NewImageReceived -= RTSPNewImageReceived;

            rtsptask.GetAwaiter().GetResult();

            void RTSPNewImageReceived(byte[] imageData)
            {
                File.WriteAllBytes($@"{DateTime.Now.Ticks}.bmp", imageData);
            }
        }
    }
}
