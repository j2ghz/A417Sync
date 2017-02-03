namespace A417Sync.Core
{
    using System.IO;
    using System.Linq;

    using A417Sync.Core.Models;

    using File = A417Sync.Core.Models.File;

    public static class RepoFactory
    {
        public static Addon MakeAddon(DirectoryInfo path)
        {
            var a = new Addon();
            a.Name = path.Name;
            a.Files =
                path.GetFiles("*", SearchOption.AllDirectories)
                    .Select(x => new File() { Path = x.FullName, LastChange = x.LastWriteTimeUtc })
                    .ToList();
            return a;
        }

        public static Repo MakeRepo(DirectoryInfo path)
        {
            var r = new Repo();
            r.Addons = path.GetDirectories().Where(x => x.Name.StartsWith("@")).Select(MakeAddon).ToList();

            return r;
        }
    }
}