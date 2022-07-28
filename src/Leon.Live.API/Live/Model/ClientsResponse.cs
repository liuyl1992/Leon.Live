using System.Collections.Generic;

namespace Leon.Live.API.Live
{// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Client
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("vhost")]
        public string Vhost { get; set; }

        [JsonProperty("stream")]
        public string Stream { get; set; }

        [JsonProperty("ip")]
        public string Ip { get; set; }

        [JsonProperty("pageUrl")]
        public string PageUrl { get; set; }

        [JsonProperty("swfUrl")]
        public string SwfUrl { get; set; }

        [JsonProperty("tcUrl")]
        public string TcUrl { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("publish")]
        public bool Publish { get; set; }

        [JsonProperty("alive")]
        public double Alive { get; set; }
    }

    public class ClientsResponse
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("server")]
        public string Server { get; set; }

        [JsonProperty("clients")]
        public List<Client> Clients { get; set; }
    }


}
