using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace A417Sync.Server
{
    using A417Sync.Core;

    public class Program
    {
        public static void Main(string[] args)
        {
            //var host = new WebHostBuilder()
            //    .UseKestrel()
            //    .UseContentRoot(Directory.GetCurrentDirectory())
            //    .UseIISIntegration()
            //    .UseStartup<Startup>()
            //    .UseApplicationInsights()
            //    .Build();

            //host.Run();

            var r = RepoFactory.MakeRepoDefaultModpack(new DirectoryInfo(args[0]));
            RepoFactory.SaveRepo(r, Path.Combine(args[0], "index.xml"));
        }
    }
}
