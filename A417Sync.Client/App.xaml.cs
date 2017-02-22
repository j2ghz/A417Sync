using Microsoft.HockeyApp;
using System;
using System.Windows;

namespace A417Sync.Client
{
    using System.Security.Permissions;

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

            AppDomain.CurrentDomain.UnhandledException += (sender, args) => HockeyClient.Current.TrackException(args.ExceptionObject as System.Exception);

            HockeyClient.Current.Flush();
        }
    }
}
