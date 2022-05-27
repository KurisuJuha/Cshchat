using System;
using System.Timers;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace KYapp.Cshchat
{
    public class YoutubeLive
    {
        private Data YoutubeChatData;
        private HttpClient client = new HttpClient();

        private Timer timer;

        public YoutubeLive(string v)
        {


            timer = new Timer(1000);
            timer.Elapsed += (sender, e) =>
            {
                //初回時にデータを取得する
                if (YoutubeChatData == null)
                {
                    Task<Data> task = FetchFirstLive(v);
                    task.Wait();
                    YoutubeChatData = task.Result;
                }

                Task<HttpResponseMessage> chat = FetchChat();
                chat.Wait();
                Task<string> res = chat.Result.Content.ReadAsStringAsync();
                res.Wait();

                //パース
                ChatParse(res.Result);

            };
        }

        public string ChatParse(string res)
        {
            var root = ParseJson(res);
            var continuationContents = ParseJson(root["continuationContents"].ToString());
            var liveChatContinuation = ParseJson(continuationContents["liveChatContinuation"].ToString());
            if (liveChatContinuation.TryGetValue("actions", out object actions))
            {
                Console.WriteLine(actions.ToString());
            }
            else
            {
                Console.WriteLine("None");
            }
            return "";
        }

        public Dictionary<string, object> ParseJson(string json)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        }

        public void Begin()
        {
            timer.Start();
        }

        public void End()
        {
            timer.Stop();
        }

        public async Task<HttpResponseMessage> FetchChat()
        {
            var param = new Dictionary<string, string>()
            {
                {"key",YoutubeChatData.Key}
            };
            var res = await client.PostAsync(
                "https://www.youtube.com/youtubei/v1/live_chat/get_live_chat?" + "key=" + param["key"],
                new StringContent(DataBuild())
            );

            if (res.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return res;
            }
            else
            {
                return null;
            }
        }

        public string DataBuild()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("{");
            builder.Append("\"context\":{");
            builder.Append("\"client\":{");

            builder.Append("\"visitorData\":");
            builder.Append("\"");
            builder.Append(YoutubeChatData.VisitorData);
            builder.Append("\",");
            builder.Append("\"User-Agent\":\"Mozilla / 5.0(Windows NT 6.3; Win64; x64) AppleWebKit / 537.36(KHTML, like Gecko) Chrome / 86.0.4240.111 Safari / 537.36\"");
            builder.Append(",");
            builder.Append("\"clientName\" : \"WEB\",");
            builder.Append("\"clientVersion\":");
            builder.Append("\"");
            builder.Append(YoutubeChatData.ClientVersion);
            builder.Append("\"");

            builder.Append("}");
            builder.Append("},");
            builder.Append("\"continuation\":");
            builder.Append("\"");
            builder.Append(YoutubeChatData.Ctn);
            builder.Append("\"");

            builder.Append("}");
            return builder.ToString();
        }

        public async Task<Data> FetchFirstLive(string v)
        {
            client.DefaultRequestHeaders.Add(
        "User-Agent",
        "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.111 Safari/537.36");

            var param = new Dictionary<string, string>()
            {
                ["v"] = v
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
