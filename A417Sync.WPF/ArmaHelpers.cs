namespace A417Sync.WPF
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using A417Sync.WPF.Models;

    using DerAtrox.Arma3LauncherLib.Model;

    using Microsoft.Win32;

    public static class ArmaHelpers
    {
        public static string GetArma3Path()
        {
            try
            {
                RegistryKey registryKey =
                    Registry.LocalMachine.OpenSubKey(
                        Environment.Is64BitOperatingSystem ? "SOFTWARE\\Wow6432Node" : "SOFTWARE");
                registryKey =
                    registryKey.OpenSubKey(
                            registryKey.GetSubKeyNames()
                                .First(key => string.Equals(key, "Bohemia Interactive", StringComparison.OrdinalIgnoreCase)))
                        .OpenSubKey("arma 3");
                return registryKey.GetValue("MAIN").ToString();
            }
            catch
            {
                throw new ArmaNotFoundException("Arma 3 installation directory could not be found!");
            }
        }

        public static void StartArma(
            Modpack modpack,
            IEnumerable<Addon> addons,
            DirectoryInfo basePath,
            IEnumerable<string> arguments)
        {
            var addonFolders = basePath.EnumerateDirectories();
            var settings = new ArmaStartSettings();
            settings.Mods =
                addons.Select(addon => addonFolders.First(folder => folder.Name == addon.Name).FullName).ToList();
            settings.OtherArgs.AddRange(arguments);
            settings.OtherArgs.Add(modpack.AdditionalParams);
            var server = ServerInfo(modpack);
            new ArmaLauncher().Connect(Path.Combine(GetArma3Path(), "arma3battleye.exe"), server, settings, true);
        }

        public static ArmaServer ServerInfo(Modpack modpack)
        {
            return new ArmaServer(modpack.IP, modpack.Port, modpack.Query, modpack.Password);
        }
    }

    internal class ArmaNotFoundException : Exception
    {
        public ArmaNotFoundException()
        {
        }

        public ArmaNotFoundException(string message)
            : base(message)
        {
        }
    }
}