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
            ////var r = new Repo();
            ////r.Modpacks = new List<Modpack>();
            ////var modpack = new Modpack();
            ////modpack.Addons = new List<string>();
            ////modpack.Addons.Add("Addon1");
            ////modpack.Name = "417";
            ////r.Modpacks.Add(modpack);
            ////r.Modpacks.Add(modpack);
            ////r.Addons = new List<Addon>();
            ////var item = new Addon();
            ////item.Files = new List<File>();
            ////var file = new File();
            ////file.LastChange = DateTime.Now;
            ////file.Path = "PAthAddon1";
            ////item.Files.Add(file);
            ////item.Name = "Addon1";
            ////r.Addons.Add(item);
            if (string.IsNullOrWhiteSpace(args.First()))
            {
                var client = new Client(new DirectoryInfo("addons"), @"https://addons.j2ghz.com/");
                
            }
            else
            {
                var r = RepoFactory.MakeRepoDefaultModpack(new DirectoryInfo(args.First()));
                RepoFactory.SaveRepo(r, Path.Combine(args.First(), "index.xml"));
            }
        }
    }
}