using System;
using System.Timers;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text.Json.Nodes;

namespace KYapp.Cshchat
{
    public class YoutubeLive
    {
        private Data YoutubeChatData;
        private HttpClient client = new HttpClient();

        private Timer timer;

        private Action<Comment> OnComment;

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
                else
                {

                    Task<HttpResponseMessage> chat = FetchChat();
                    chat.Wait();
                    Task<string> res = chat.Result.Content.ReadAsStringAsync();
                    res.Wait();

                    //パース
                    ChatParse(res.Result);

                    var _a = JsonNode.Parse(res.Result);
                    var _b = _a["continuationContents"];
                    var _c = _b["liveChatContinuation"];
                    var _d = _c["continuations"];
                    var _e = _d[0];
                    var _f = _e["invalidationContinuationData"];
                    //正直よくわかってないけどこれで動く（ヤバい）
                    if (_f == null)
                    {
                        _f = _e["timedContinuationData"];
                    }
                    var _g = _f["continuation"];


                    string ctn = _g.ToString();
                    YoutubeChatData.Ctn = ctn;

                }
            };
        }

        public List<Comment> ChatParse(string res)
        {
            List<Comment> comments = new List<Comment>();

            var node = JsonNode.Parse(res);
            var a = node["continuationContents"]["liveChatContinuation"]["actions"];
            if (a != null)
            {
                foreach (var item in a.AsArray())
                {
                    Comment com = new Comment();
                    bool ok = false;

                    JsonNode Chat = item["addChatItemAction"]?["item"]?["liveChatTextMessageRenderer"];
                    JsonNode SuperChat = item["addChatItemAction"]?["item"]?["liveChatPaidMessageRenderer"];


                    if (Chat != null)
                    {
                        //通常コメント
                        foreach (var item2 in Chat["message"]?["runs"].AsArray())
                        {
                            if (item2["text"] != null)
                            {
                                com.text = item2["text"].ToString();
                                ok = true;
                            }
                        }
                    }
                    else if(SuperChat != null)
                    {
                        //スパ茶
                        //Todo: スパ茶を確認できる環境がないため未実装
                    }

                    if (ok)
                    {
                        OnComment(com);
                        comments.Add(com);
                    }
                }
            }
            return comments;
        }
        public void Begin(Action<Comment> OnComment)
        {
            this.OnComment = OnComment;
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

    public class Comment
    {
        public string text;
        public string name;
        public string id;
        public bool isSuperChat;
    }
}
