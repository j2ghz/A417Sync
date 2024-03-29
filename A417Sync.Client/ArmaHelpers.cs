﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using A417Sync.Client.Models;
using DerAtrox.Arma3LauncherLib.Exceptions;
using DerAtrox.Arma3LauncherLib.Model;
using Microsoft.Win32;
using Serilog;

namespace A417Sync.Client
{
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
                                .First(key => string.Equals(key, "Bohemia Interactive",
                                    StringComparison.OrdinalIgnoreCase)))
                        .OpenSubKey("arma 3");
                return registryKey.GetValue("MAIN").ToString();
            }
            catch
            {
                throw new ArmaNotFoundException("Arma 3 installation directory could not be found!");
            }
        }

        public static ArmaServer ServerInfo(Modpack modpack)
        {
            return new ArmaServer(modpack.IP, modpack.Port, modpack.Query, modpack.Password);
        }

        public static void StartArma(
            Modpack modpack,
            IEnumerable<Addon> addons,
            DirectoryInfo basePath,
            IEnumerable<string> arguments,
            bool connectIsChecked,
            IEnumerable<string> userAddons,
            bool set64bit,
            bool battleEye)
        {
            if (!Process.GetProcessesByName("Steam").Any())
            {
                MessageBox.Show(
                    "Steam is not running. Arma will probably not launch.",
                    "Steam not running",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }

            var addonFolders = basePath.EnumerateDirectories();
            var settings = new ArmaStartSettings();
            settings.Mods =
                addons.Select(addon => addonFolders.First(folder => folder.Name == addon.Name).FullName).ToList();
            settings.Mods.AddRange(userAddons);
            settings.OtherArgs.AddRange(arguments);
            settings.OtherArgs.Add(modpack.AdditionalParams);
            var server = ServerInfo(modpack);
            try
            {
                var exe = "arma3battleye.exe";
                if (!battleEye)
                {
                    exe = set64bit ? "arma3_x64.exe" : "arma3.exe";
                }
                new ArmaLauncher().Connect(
                    Path.Combine(GetArma3Path(), exe),
                    connectIsChecked ? server : null,
                    settings,
                    true,
                    set64bit: set64bit, battleEye: battleEye);
            }
            catch (ArmaRunningException ex)
            {
                Log.Warning(ex, "Arma already running");
                MessageBox.Show(
                    "Arma already running!",
                    "Error launching Arma",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (ArmaNotFoundException ex)
            {
                Log.Warning(ex, "Arma path not found");
                MessageBox.Show(
                    "Arma path not found in the registry! Have you launched Arma at least once without the launcher before?",
                    "Error launching Arma",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
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