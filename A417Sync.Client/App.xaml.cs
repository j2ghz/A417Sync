using Microsoft.HockeyApp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
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
            HockeyClient.Current.Configure("fcc1c7f687e344eb8e805ae492daf0c2");
            HockeyClient.Current.SendCrashesAsync(true).GetAwaiter().GetResult();
            HockeyAppWorkaroundInitializer.InitializeAsync().GetAwaiter().GetResult();
            HockeyClient.Current.TrackEvent("Launch");
        }
    }
}
