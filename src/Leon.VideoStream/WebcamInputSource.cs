namespace Leon.VideoStream
{
    public class WebcamInputSource : InputSource
    {
        private WebcamInputSource() { }
        internal override string InputCommand { get; }

        public WebcamInputSource(string busDeviceName)
        {
            this.InputCommand = $"-f dshow -i video=\"{busDeviceName}\"";
        }
    }
}
