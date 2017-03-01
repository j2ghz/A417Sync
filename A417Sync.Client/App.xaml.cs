namespace A417Sync.Client
{
    using System;
    using System.Deployment.Application;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Windows;

    using Microsoft.HockeyApp;

    using Serilog;

    public partial class App : Application
    {
        public string LocalUserAppDataPath
            =>
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    this.Name,
                    this.Version);

        public string LogFile => Path.Combine(this.LocalUserAppDataPath, "{Date}.log");

        public string Name => Assembly.GetEntryAssembly().GetName().Name;

        public string Version
            =>
                ApplicationDeployment.IsNetworkDeployed
                    ? ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString()
                    : "Not versioned";

        [DllImport("Kernel32")]
        public static extern void FreeConsole();

        protected override void OnStartup(StartupEventArgs e)
        {
#if DEBUG
            ConsoleManager.Show(); //FIX the console doesn't show anything
#endif
            this.SetupLogging();
            this.LogLaunchMessage();
            this.HockeyApp();
            base.OnStartup(e);
        }

        [DllImport("Kernel32")]
        private static extern void AllocConsole();

        private void HockeyApp()
        {
            HockeyClient.Current.Configure("fcc1c7f687e344eb8e805ae492daf0c2")
                .SetContactInfo(
                    Environment.UserName,
                    (this.Properties["Contact"] ?? Environment.MachineName).ToString());

            ((HockeyClient)HockeyClient.Current).VersionInfo = this.Version;

            HockeyClient.Current.SendCrashesAsync(true);

            HockeyClient.Current.TrackEvent("Launch");

            HockeyClient.Current.Flush();
        }

        private void LogLaunchMessage()
        {
            Log.Information("Starting {name} {version}", this.Name, this.Version);
            Log.Information("Logging to {filePath}", LogFile);
            Console.WriteLine("a");
        }

        private void SetupLogging()
        {
            Log.Logger =
                new LoggerConfiguration()
                    .WriteTo.LiterateConsole(
                        outputTemplate: "[{Timestamp:HH:mm:ss} {Level} {SourceContext}] {Message}{NewLine}{Exception}")
                    .WriteTo.RollingFile(
                        this.LogFile,
                        outputTemplate: "{Timestamp:o} [{Level:u3}] ({SourceContext}/{ThreadId}) {Message}{NewLine}{Exception}")
                    .WriteTo.Trace()
                    .Enrich.FromLogContext()
                    .CreateLogger();
        }
    }
}