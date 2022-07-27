using CliWrap;
using Leon.VideoStream;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace Leon.Live.API
{
    public interface ILiveService
    {
        Task<Stream> GetVideoAsync();
        Task GetRealTimeVideoAsync();
        void GetStrViewRtmp(string strRtsp,out string outPutLink,  string type = "rtmp");
    }

    public class LiveService : ILiveService, ISingletonDependency
    {
        private HttpClient _client;

        public LiveService()
        {
            _client = new HttpClient();
        }

        public async Task<Stream> GetVideoAsync()
        {
            var urlBlob = "https://anthonygiretti.blob.core.windows.net/videos/earth.mp4";

            return await _client.GetStreamAsync(urlBlob);
        }

        public async Task GetRealTimeVideoAsync()
        {
            //webcam
            var inputSource = new WebcamInputSource("Lenovo EasyCamera");

            var cancellationTokenSource = new CancellationTokenSource();

            var client = new VideoStreamClient("D:/ffmpeg-5.0/bin/ffmpeg.exe");
            client.NewImageReceived += NewImageReceived;
            var task = client.StartFrameReaderAsync(inputSource, OutputImageFormat.Bmp, cancellationTokenSource.Token);

            client.NewImageReceived -= NewImageReceived;

            void NewImageReceived(byte[] imageData)
            {
                Stream stream = new MemoryStream(imageData);
                File.WriteAllBytes($@"{DateTime.Now.Ticks}.bmp", imageData);
            }
        }

        public void GetStrViewRtmp(string strRtsp, out string outPutLink, string type = "rtmp")
        {
            //rtsp 唯一
            outPutLink = "rtmp://192.168.150.223/live/livestream";
            string outPutLinkTemp = outPutLink;
            Task.Factory.StartNew(() => {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                process.StartInfo.FileName = "ffmpeg";
                process.StartInfo.Arguments = $"""-i "{strRtsp}" -r 5 -c copy -f flv {outPutLinkTemp}""";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardError = true;
                process.Start();

                StreamReader readerOut = process.StandardOutput;
                StreamReader readerErr = process.StandardError;
                // Process the readers e.g. like follows

                while (!process.HasExited)
                { 
                    //string errors = readerErr.ReadToEnd().ToString();
                    string output = readerOut.ReadToEnd();
                    Console.WriteLine($"标准输出{output}");
                }
                process.Dispose();
                // ffmpeg -i "rtsp://admin:owd@196.118.37.8:555/Streaming/Channels/103?transportmode=unicast&profile=Profile_3" -r 5 -c copy -f flv rtmp://192.168.236.789/live/livestream
                //Console.WriteLine($"异常{errors}");
            }, TaskCreationOptions.LongRunning);
        }

        ~LiveService()
        {
            if (_client != null)
                _client.Dispose();
        }
    }
}
