using System;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace KYapp.Cshchat
{
    public class YoutubeLive
    {
        public Data YoutubeChatData;

        public YoutubeLive()
        {

        }

        public async Task<Data> FetchFirstLive()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add(
        "User-Agent",
        "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.111 Safari/537.36");

            var param = new Dictionary<string, string>()
            {
                ["v"] = "5qap5aO4i9A"
            };
            var content = await new FormUrlEncodedContent(param).ReadAsStringAsync();
            var result = await client.GetAsync(@"https://www.youtube.com/live_chat?" + content);
            var data = await result.Content.ReadAsStringAsync();


            Match matchedKey = Regex.Match(data, "\"INNERTUBE_API_KEY\":\"(.+?)\"");
            Match matchedCtn = Regex.Match(data, "\"continuation\":\"(.+?)\"");
            Match matchedVisitor = Regex.Match(data, "\"visitorData\":\"(.+?)\"");
            Match matchedClient = Regex.Match(data, "\"clientVersion\":\"(.+?)\"");

            YoutubeChatData = new Data()
            {
                Key = matchedKey.Groups[1].Value,
                Ctn = matchedCtn.Groups[1].Value,
                VisitorData = matchedVisitor.Groups[1].Value,
                ClientVersion = matchedClient.Groups[1].Value,
            };

            return YoutubeChatData;
        }

        public class Data
        {
            public string Key;
            public string Ctn;
            public string VisitorData;
            public string ClientVersion;

            public override string ToString()
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(Key);
                builder.Append(",\n");
                builder.Append(Ctn);
                builder.Append(",\n");
                builder.Append(VisitorData);
                builder.Append(",\n");
                builder.Append(ClientVersion);
                return builder.ToString();
            }
        }
    }
}
