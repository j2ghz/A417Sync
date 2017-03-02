namespace A417Sync.Core
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using A417Sync.Core.Models;

    using DerAtrox.Arma3LauncherLib.Model;
    using DerAtrox.Arma3LauncherLib.Utilities;

    public static class ArmaHelpers
    {
        public static void StartArma(IEnumerable<Addon> addons, DirectoryInfo basePath, IEnumerable<string> Arguments)
        {
            var addonFolders = basePath.EnumerateDirectories();
            var settings = new ArmaStartSettings();
            settings.Mods =
                addons.Select(addon => addonFolders.First(folder => folder.Name == addon.Name).FullName).ToList();
            settings.OtherArgs.AddRange(Arguments);
            new ArmaLauncher().Connect(ArmaUtils.GetArma3Path(), settings, true);
        }
    }
}