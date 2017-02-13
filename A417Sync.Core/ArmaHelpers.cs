namespace A417Sync.Core
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using A417Sync.Core.Models;

    public static class ArmaHelpers
    {
        public static string AddonsLaunchParameter(IEnumerable<Addon> addons, string prefix)
        {
            return string.Join(";", addons.Select(addon => Path.Combine(prefix, addon.Name)));
        }
    }
}