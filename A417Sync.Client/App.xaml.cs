namespace A417Sync.Client
{
    using System;
    using System.Deployment.Application;
    using System.IO;
    using System.Reflection;
    using System.Windows;

    using Microsoft.HockeyApp;

    using Serilog;
    using Serilog.Events;

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
        {
            get
            {
                string v = "0.0.0.0";
                try
                {
                    if (!System.Diagnostics.Debugger.IsAttached)
                    {
                        v = ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e, "Couldn't determine version");
                }

                return v;
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            ConsoleManager.Show();
            SetupLogging();
            LogLaunchMessage();
            HockeyApp();
            base.OnStartup(e);
            Log.Information("{method} finished", nameof(OnStartup));
        }

        private void HockeyApp()
        {
            Log.Information("HockeyApp initializing");
            HockeyClient.Current.Configure("9f734b2f3449467687667e99c7cdc532")
                .SetContactInfo(
                    Environment.UserName,
                    (this.Properties["Contact"] ?? Environment.MachineName).ToString());

            ((HockeyClient)HockeyClient.Current).VersionInfo = this.Version;
            Log.Debug("Checking for pending crashes");
            HockeyClient.Current.SendCrashesAsync().ContinueWith(
                task =>
                    {
                        if (!task.Result)
                        {
                            return;
                        }

                        Log.Information("Crash processing finished");
                        MessageBox.Show(
                            "Processing finished",
                            "Crash report",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }).GetAwaiter();

            HockeyClient.Current.TrackEvent("Launch");
            HockeyClient.Current.Flush();

            Log.Information("HockeyApp initialized");
        }

        private void LogLaunchMessage()
        {
            Log.Information("Starting {name} {version}", this.Name, this.Version);
            Log.Information("Logging to {filePath}", this.LogFile);
        }

        private void SetupLogging()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(this.LogFile));
            Log.Logger =
                new LoggerConfiguration().WriteTo.LiterateConsole(
                        LogEventLevel.Debug,
                        "{Timestamp:HH:mm:ss} {Level} [{SourceContext}] {Message}{NewLine}{Exception}")
                    .WriteTo.RollingFile(
                        this.LogFile,
                        LogEventLevel.Debug,
                        "{Timestamp:o} [{Level:u3}] ({SourceContext}) {Message}{NewLine}{Exception}")
                    .Enrich.FromLogContext()
                    .CreateLogger();

            AppDomain.CurrentDomain.UnhandledException +=
                (sender, args) =>
                    Log.Fatal(args.ExceptionObject as Exception, nameof(AppDomain.CurrentDomain.UnhandledException));
        }
    }
}