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

            Task<YoutubeLive.Data> task = a.FetchFirstLive();
            task.Wait();
            Console.WriteLine(task.Result.ToString());
        }
    }
}
