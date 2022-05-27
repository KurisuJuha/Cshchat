using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace KYapp.Cshchat
{
    public class YoutubeLive
    {
        public YoutubeLive()
        {

        }

        public async Task<string> FetchFirstLive()
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
            Console.WriteLine(data);
            return matchedKey.Groups[1].Value;
        }
    }
}
