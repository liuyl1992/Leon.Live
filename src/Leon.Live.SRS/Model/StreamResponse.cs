using System.Collections.Generic;

namespace Leon.Live.SRS.Model
{// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Audio
    {
        [JsonProperty("codec")]
        public string Codec { get; set; }

        [JsonProperty("sample_rate")]
        public int SampleRate { get; set; }

        [JsonProperty("channel")]
        public int Channel { get; set; }

        [JsonProperty("profile")]
        public string Profile { get; set; }
    }

    public class Kbps
    {
        [JsonProperty("recv_30s")]
        public int Recv30s { get; set; }

        [JsonProperty("send_30s")]
        public int Send30s { get; set; }
    }

    public class Publish
    {
        [JsonProperty("active")]
        public bool Active { get; set; }

        [JsonProperty("cid")]
        public string Cid { get; set; }
    }

    public class StreamResponse
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("server")]
        public string Server { get; set; }

        [JsonProperty("streams")]
        public List<Stream> Streams { get; set; }
    }

    public class Stream
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("vhost")]
        public string Vhost { get; set; }

        [JsonProperty("app")]
        public string App { get; set; }

        [JsonProperty("live_ms")]
        public object LiveMs { get; set; }

        [JsonProperty("clients")]
        public int Clients { get; set; }

        [JsonProperty("frames")]
        public int Frames { get; set; }

        [JsonProperty("send_bytes")]
        public int SendBytes { get; set; }

        [JsonProperty("recv_bytes")]
        public int RecvBytes { get; set; }

        [JsonProperty("kbps")]
        public Kbps Kbps { get; set; }

        [JsonProperty("publish")]
        public Publish Publish { get; set; }

        [JsonProperty("video")]
        public Video Video { get; set; }

        [JsonProperty("audio")]
        public Audio Audio { get; set; }
    }

    public class Video
    {
        [JsonProperty("codec")]
        public string Codec { get; set; }

        [JsonProperty("profile")]
        public string Profile { get; set; }

        [JsonProperty("level")]
        public string Level { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }
    }


}
