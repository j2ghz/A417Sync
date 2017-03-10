namespace A417Sync.Client
{
    using System;
    using System.Collections.Generic;
    using System.Deployment.Application;
    using System.IO;
    using System.Reflection;
    using System.Windows;

    using Exceptionless;

    using Microsoft.HockeyApp;

    using Serilog;

    public partial class App : Application
    {
        private ILogger log;

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
                string v = "1.2.3.4";
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
            Serilog.Debugging.SelfLog.Enable(Console.Error);
            SetupLogging();
            this.log = Log.ForContext<App>();
            LogLaunchMessage();
            HockeyApp();
            Exceptionless();
            base.OnStartup(e);
            this.log.Information("{method} finished", nameof(OnStartup));
            ConsoleManager.Hide();
        }

        private void Exceptionless()
        {
            ExceptionlessClient.Default.Register();
        }

        private void HockeyApp()
        {
            this.log.Information("HockeyApp initializing");
            HockeyClient.Current.Configure("9f734b2f3449467687667e99c7cdc532")
                .SetContactInfo(
                    Environment.UserName,
                    (this.Properties["Contact"] ?? Environment.MachineName).ToString());
            var hockeyInternal = (HockeyClient)HockeyClient.Current;
            hockeyInternal.VersionInfo = this.Version;
            this.log.Debug("Checking for pending crashes");
            HockeyClient.Current.SendCrashesAsync(true).ContinueWith(
                task =>
                    {
                        if (!task.Result)
                        {
                            return;
                        }

                        this.log.Information("Crash processing finished");
                        MessageBox.Show(
                            "Processing finished",
                            "Crash report",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }).GetAwaiter();

            hockeyInternal.OnHockeySDKInternalException +=
                (sender, args) =>
                    this.log.Warning(
                        args.Exception,
                        "Possible problem in HockeyApp (Crash Reporter) from: {@sender}",
                        sender);

            HockeyClient.Current.TrackEvent("Launch", new Dictionary<string, string> { ["Version"] = this.Version });
            HockeyClient.Current.Flush();

            this.log.Information("HockeyApp initialized, app version {version}", hockeyInternal.VersionInfo);
        }

        private void LogLaunchMessage()
        {
            this.log.Information("Starting {name} {version}", this.Name, this.Version);
            this.log.Information("Logging to {filePath}", this.LogFile);
        }

        private void SetupLogging()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(this.LogFile));

            Log.Logger =
                new LoggerConfiguration().WriteTo.LiterateConsole(
                        outputTemplate: "{Timestamp:HH:mm:ss} {Level:u3} [{SourceContext}] {Message}{NewLine}{Exception}")
                    .WriteTo.RollingFile(
                        this.LogFile,
                        outputTemplate: "{Timestamp:o} [{Level:u3}] ({SourceContext}) {Message}{NewLine}{Exception}")
                    .Enrich.FromLogContext()
                    .MinimumLevel.Verbose()
                    .CreateLogger();
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
                {
                    ConsoleManager.Show();
                    Log.Fatal(args.ExceptionObject as Exception, nameof(AppDomain.CurrentDomain.UnhandledException));
                };
        }
    }
}