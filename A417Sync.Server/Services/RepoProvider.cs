using A417Sync.Core;
using A417Sync.Core.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace A417Sync.Server.Services
{
    public class RepoProvider
    {
        RepoOptions RepoConfig { get; }

        public RepoProvider(IOptions<RepoOptions> config)
        {
            RepoConfig = config.Value;
        }
        public Repo Load() {
            return RepoFactory.LoadRepo(new FileInfo(RepoConfig.IndexPath).OpenRead());
        }
        public void Save(Repo r) {
            RepoFactory.SaveRepo(r, RepoConfig.IndexPath);
        }
    }
}
