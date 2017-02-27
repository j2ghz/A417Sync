namespace A417Sync.Client
{
    using System;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Navigation;

    using Microsoft.HockeyApp;
    using Microsoft.HockeyApp.DataContracts;
    using Microsoft.HockeyApp.Internal;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            HockeyClient.Current.Configure("fcc1c7f687e344eb8e805ae492daf0c2").SetContactInfo(Environment.UserName, (Properties["Contact"] ?? Environment.MachineName).ToString());

            HockeyClient.Current.SendCrashesAsync(true);

            base.OnStartup(e);

            HockeyClient.Current.TrackEvent("Launch");

            HockeyClient.Current.Flush();
        }
    }
}