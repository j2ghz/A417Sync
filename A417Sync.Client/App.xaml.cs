using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace A417Sync.Client
{
    using Microsoft.HockeyApp;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            HockeyClient.Current.Configure("fcc1c7f687e344eb8e805ae492daf0c2");
            HockeyClient.Current.SendCrashesAsync(true).GetAwaiter();
            ////HockeyAppWorkaroundInitializer.InitializeAsync().GetAwaiter().GetResult();
            HockeyClient.Current.TrackEvent("Launch");

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
                {
                    HockeyClient.Current.TrackException(args.ExceptionObject as System.Exception);
                    HockeyClient.Current.Flush();
                };

            HockeyClient.Current.Flush();
        }
    }
}

