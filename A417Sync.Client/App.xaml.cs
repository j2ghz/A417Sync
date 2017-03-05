namespace A417Sync.Client
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Windows;

    using Loggly;
    using Loggly.Config;

    using Microsoft.HockeyApp;

    using Serilog;

    using Squirrel;

    public partial class App : Application
    {
        private ILogger log;

        public string LocalUserAppDataPath
            =>
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    Assembly.GetExecutingAssembly().GetName().Name);

        private string LogFile => Path.Combine(this.LocalUserAppDataPath, "{Date}.log");

        protected override async void OnStartup(StartupEventArgs e)
        {
            ConsoleManager.Show();
            Serilog.Debugging.SelfLog.Enable(Console.Error);
            SetupLogging();
            this.log = Log.ForContext<App>();
            LogLaunchMessage();
            HockeyApp();
            base.OnStartup(e);
            this.log.Information("{method} finished", nameof(OnStartup));
            using (var mgr = new UpdateManager("D:\\Source\\A417Sync\\A417Sync.Client\\Releases"))
            {
                await mgr.UpdateApp().ConfigureAwait(false);
            }

            ConsoleManager.Hide();
        }

        private void HockeyApp()
        {
            this.log.Information("HockeyApp initializing");
            HockeyClient.Current.Configure("9f734b2f3449467687667e99c7cdc532")
                .SetContactInfo(
                    Environment.UserName,
                    (this.Properties["Contact"] ?? Environment.MachineName).ToString());
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

            HockeyClient.Current.Flush();

            this.log.Information(
                "HockeyApp initialized, app version {version}",
                Assembly.GetExecutingAssembly().GetName().Version.ToString());
        }

        private void LogLaunchMessage()
        {
            this.log.Information("Starting {name}", Assembly.GetExecutingAssembly().GetName().FullName);
            this.log.Information("Logging to {filePath}", this.LogFile);
        }

        private void SetupLogging()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(this.LogFile));
            var config = LogglyConfig.Instance;
            config.CustomerToken = "de5800d3-c799-4865-8f20-d8106467c08b";
            config.ApplicationName = "A417Sync";
            config.Transport = new TransportConfiguration()
                                   {
                                       EndpointHostname = "logs-01.loggly.com",
                                       EndpointPort = 443,
                                       LogTransport = LogTransport.Https
                                   };
            config.ThrowExceptions = true;

            // Define Tags sent to Loggly
            config.TagConfig.Tags.AddRange(
                new ITag[]
                    {
                        new ApplicationNameTag { Formatter = "application-{0}" },
                        new HostnameTag { Formatter = "host-{0}" },
                        new OperatingSystemVersionTag { Formatter = "os-{0}" }
                    });

            Log.Logger =
                new LoggerConfiguration().WriteTo.LiterateConsole(
                        outputTemplate: "{Timestamp:HH:mm:ss} {Level:u3} [{SourceContext}] {Message}{NewLine}{Exception}")
                    .WriteTo.RollingFile(
                        this.LogFile,
                        outputTemplate: "{Timestamp:o} [{Level:u3}] ({SourceContext}) {Message}{NewLine}{Exception}")
                    .WriteTo.Loggly(bufferBaseFilename: Path.GetTempFileName())
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