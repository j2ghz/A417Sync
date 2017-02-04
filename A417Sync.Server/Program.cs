namespace A417Sync.Server
{
    using System;
    using System.IO;
    using System.Linq;

    using A417Sync.Core;

    class Program
    {
        static void Main(string[] args)
        {
            if (string.IsNullOrWhiteSpace(args.FirstOrDefault()))
            {
                var client = new Client(new DirectoryInfo(@"D:\417Launcher\417Launcher\417addons"), new Uri(@"https://addons.j2ghz.com/"));
                var repo = Client.DownloadRepo(new Uri(@"https://addons.j2ghz.com/index.xml")).Result;
                client.Update(repo.Addons);
                
            }
            else
            {
                var r = RepoFactory.MakeRepoDefaultModpack(new DirectoryInfo(args.First()));
                RepoFactory.SaveRepo(r, Path.Combine(args.First(), "index.xml"));
            }
        }
    }
}