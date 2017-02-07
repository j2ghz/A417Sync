namespace A417Sync.Server
{
    using System;
    using System.IO;
    using System.Linq;

    using A417Sync.Core;

    public static class Program
    {
        public static void Main(string[] args)
        {
            if (string.IsNullOrWhiteSpace(args.FirstOrDefault()))
            {
                var client = new Client(
                    new DirectoryInfo(@"D:\417Launcher\417Launcher\417addons"),
                    new Uri(@"https://addons.j2ghz.com/"));
                var repo = Client.DownloadRepo(new Uri(@"https://addons.j2ghz.com/index.xml")).Result;
                client.Update(repo.Addons).GetAwaiter().GetResult();
            }
            else
            {
                var r = RepoFactory.MakeRepoDefaultModpack(new DirectoryInfo(args[0]));
                RepoFactory.SaveRepo(r, Path.Combine(args[0], "index.xml"));
            }
        }
    }
}