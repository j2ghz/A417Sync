using Microsoft.HockeyApp;
using RollbarDotNet;
using System;
using System.Windows;

namespace A417Sync.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Rollbar.Init(new RollbarConfig
            {
                AccessToken = "d5d2bfe7fa624f2790ed45bd2a2d8d96",
#if DEBUG
                Environment = "Debug"
#else
                Environment = "Release"
#endif
            });
            Rollbar.PersonData(() => new Person($"{Environment.UserName}@{Environment.CurrentDirectory}@{Environment.MachineName}")
                {
                    UserName = Environment.UserName,
                    Email = $"{Environment.UserName}@{Environment.UserDomainName}"
                }
            );
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Rollbar.Report(args.ExceptionObject as System.Exception);
            };

            HockeyClient.Current.Configure("fcc1c7f687e344eb8e805ae492daf0c2");
            HockeyClient.Current.SendCrashesAsync(true).GetAwaiter().GetResult();
            HockeyAppWorkaroundInitializer.InitializeAsync().GetAwaiter().GetResult();
            HockeyClient.Current.TrackEvent("Launch");

            AppDomain.CurrentDomain.UnhandledException += (sender, args) => HockeyClient.Current.TrackException(args.ExceptionObject as System.Exception);
        }
    }
}
