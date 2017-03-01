namespace A417Sync.Client
{
    using System;
    using System.Deployment.Application;
    using System.IO;
    using System.Reflection;
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
        {
            get
            {
                string v;
                try
                {
                    v = ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
                }
                catch (Exception e)
                {
                    Log.Error(e, "Couldn't determine version");
                    v = "0.0.0.0";
                }

                return v;
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            SetupLogging();
            LogLaunchMessage();
            HockeyApp();
            base.OnStartup(e);
        }

        private void HockeyApp()
        {
            HockeyClient.Current.Configure("fcc1c7f687e344eb8e805ae492daf0c2")
                .SetContactInfo(
                    Environment.UserName,
                    (this.Properties["Contact"] ?? Environment.MachineName).ToString());

            ((HockeyClient)HockeyClient.Current).VersionInfo = this.Version;

            HockeyClient.Current.SendCrashesAsync(false).ContinueWith(
                task =>
                    {
                        if (task.Result)
                        {
                            MessageBox.Show(
                                "Processing finished",
                                "Crash report",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                        }
                    }).GetAwaiter();

            HockeyClient.Current.TrackEvent("Launch");

            HockeyClient.Current.Flush();
        }

        private void LogLaunchMessage()
        {
            Log.Information("Starting {name} {version}", this.Name, this.Version);
            Log.Information("Logging to {filePath}", this.LogFile);
        }

        private void SetupLogging()
        {
            Directory.CreateDirectory(LogFile);
            Log.Logger =
                new LoggerConfiguration().WriteTo.LiterateConsole(
                        outputTemplate: "[{Timestamp:HH:mm:ss} {Level} {SourceContext}] {Message}{NewLine}{Exception}")
                    .WriteTo.RollingFile(
                        this.LogFile,
                        outputTemplate:
                        "{Timestamp:o} [{Level:u3}] ({SourceContext}/{ThreadId}) {Message}{NewLine}{Exception}")
                    .WriteTo.Trace()
                    .Enrich.FromLogContext()
                    .CreateLogger();

            AppDomain.CurrentDomain.UnhandledException +=
                (sender, args) =>
                    Log.Fatal(args.ExceptionObject as Exception, nameof(AppDomain.CurrentDomain.UnhandledException));
        }
    }
}