namespace A417Sync.Server
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Serialization;

    using A417Sync.Core;
    using A417Sync.Core.Models;

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
            var r = RepoFactory.MakeRepo(new DirectoryInfo(args.First()));

            r.Modpacks = new List<Modpack>
                             {
                                 new Modpack()
                                     {
                                         Name = "417",
                                         Addons = r.Addons.Select(x => x.Name).ToList()
                                     }
                             };

            var xml = new XmlSerializer(typeof(Repo));

            xml.Serialize(new StreamWriter(System.IO.File.CreateText("index.xml").BaseStream), r);
        }
    }
}