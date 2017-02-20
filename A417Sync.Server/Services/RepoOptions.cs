using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace A417Sync.Server.Services
{
    public class RepoOptions
    {
        public string Path { get; set; }

        public string IndexPath => System.IO.Path.Combine(Path, "index.xml");
    }
}
