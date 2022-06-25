using System.IO;

namespace A417Sync.Server
{
    using A417Sync.Core;

    public class Program
    {
        public static void Main(string[] args)
        {
            var r = RepoFactory.MakeRepoDefaultModpack(new DirectoryInfo(args[0]));
            RepoFactory.SaveRepo(r, Path.Combine(args[0], "index.xml"));
        }
    }
}
