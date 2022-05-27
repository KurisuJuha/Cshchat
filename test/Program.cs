using System;
using KYapp.Cshchat;
using System.Threading.Tasks;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            YoutubeLive a = new YoutubeLive();

            Task<YoutubeLive.Data> task = a.FetchFirstLive("5qap5aO4i9A");
            task.Wait();

            Task<string> task2 = a.FetchChat();
            task2.Wait();
            Console.WriteLine(task2.Result.ToString());
            
        }
    }
}
