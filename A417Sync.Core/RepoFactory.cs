namespace A417Sync.Core
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Serialization;

    using A417Sync.Core.Models;

    using File = A417Sync.Core.Models.File;

    public static class RepoFactory
    {
        public static Repo LoadRepo(Stream s)
        {
            var xml = new XmlSerializer(typeof(Repo));

            return (Repo)xml.Deserialize(s);
        }

        public static Addon MakeAddon(DirectoryInfo path)
        {
            var a = new Addon
                        {
                            Name = path.Name,
                            Files =
                                path.GetFiles("*", SearchOption.AllDirectories)
                                    .Select(
                                        x =>
                                            new File()
                                                {
                                                    Path = x.FullName.Substring(path.FullName.Length),
                                                    LastChange = x.LastWriteTimeUtc,
                                                    Size = x.Length
                                                })
                                    .ToList()
                        };
            return a;
        }

        public static Repo MakeRepo(DirectoryInfo path)
        {
            var r = new Repo
                        {
                            Addons =
                                path.GetDirectories().Where(x => x.Name.StartsWith("@")).Select(MakeAddon).ToList()
                        };

            return r;
        }

        public static Repo MakeRepoDefaultModpack(DirectoryInfo path)
        {
            var r = MakeRepo(path);

            r.Modpacks = new List<Modpack>
                             {
                                 new Modpack()
                                     {
                                         Name = "417",
                                         Addons = r.Addons.Select(x => x.Name).ToList()
                                     }
                             };
            return r;
        }

        public static void SaveRepo(Repo r, string path)
        {
            var xml = new XmlSerializer(typeof(Repo));

            xml.Serialize(new StreamWriter(System.IO.File.CreateText(path).BaseStream), r);
        }
    }
}