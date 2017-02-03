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
            if (string.IsNullOrWhiteSpace(args.First()))
            {
                var client = new Client(new DirectoryInfo("addons"), new Uri(@"https://addons.j2ghz.com/"));
                
            }
            else
            {
                var r = RepoFactory.MakeRepoDefaultModpack(new DirectoryInfo(args.First()));
                RepoFactory.SaveRepo(r, Path.Combine(args.First(), "index.xml"));
            }
        }
    }
}