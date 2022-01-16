
## Examples of use

### Network Camera (RTSP Stream)
```cs
var inputSource = new StreamInputSource("rtsp://wowzaec2demo.streamlock.net/vod/mp4:BigBuckBunny_115k.mov");

var cancellationTokenSource = new CancellationTokenSource();

var client = new VideoStreamClient();
client.NewImageReceived += NewImageReceived;
var task = client.StartFrameReaderAsync(inputSource, OutputImageFormat.Bmp, cancellationTokenSource.Token);

//wait for exit
Console.ReadLine();

client.NewImageReceived -= NewImageReceived;

void NewImageReceived(byte[] imageData)
{
    File.WriteAllBytes($@"{DateTime.Now.Ticks}.bmp", imageData);
}
```

### Webcam
You can find out the name of your webcam in the `Windows Device Manager` in the section `Camer`

```cs
var inputSource = new WebcamInputSource("HP HD Camera");

var cancellationTokenSource = new CancellationTokenSource();

var client = new VideoStreamClient();
client.NewImageReceived += NewImageReceived;
var task = client.StartFrameReaderAsync(inputSource, OutputImageFormat.Bmp, cancellationTokenSource.Token);

//wait for exit
Console.ReadLine();

client.NewImageReceived -= NewImageReceived;

void NewImageReceived(byte[] imageData)
{
    File.WriteAllBytes($@"{DateTime.Now.Ticks}.bmp", imageData);
}
```
