namespace A417Sync.Client
{
    #region

    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Threading;

    using Microsoft.HockeyApp;
    using Microsoft.VisualBasic;

    using Serilog;

    #endregion

    public partial class MainWindow : Window
    {
        private ILogger log = Log.ForContext<MainWindow>();

        public MainWindow()
        {
            HockeyClient.Current.TrackPageView(nameof(MainWindow));
            this.ViewModel = new MainWindowViewModel();
            while (string.IsNullOrWhiteSpace(this.ViewModel.Path) || !new DirectoryInfo(this.ViewModel.Path).Exists)
                this.ViewModel.Path =
                    Interaction.InputBox(
                        "Your addon folder does not exist. Provide a directory that exists",
                        "Addon directory not found",
                        "C:\\417addons\\");
            this.InitializeComponent();
            this.DataContext = this.ViewModel;
        }

        public MainWindowViewModel ViewModel { get; }

        public static string GetLogFor(object target)
        {
            var properties =
                from property in target.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                select new { Name = property.Name, Value = property.GetValue(target, null) };

            var builder = new StringBuilder();

            foreach (var property in properties)
            {
                builder.Append(property.Name).Append(" = ").Append(property.Value).AppendLine();
            }

            return builder.ToString();
        }

        private void ConsoleToggle(object sender, RoutedEventArgs e)
        {
            ConsoleManager.Toggle();
        }

        private async void Download(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel.DownloadTask.GetAwaiter().IsCompleted)
            {
                this.ViewModel.CanCheck = false;
                this.ViewModel.DownloadTaskCancel = new CancellationTokenSource();
                this.ViewModel.DownloadTask =
                    this.ViewModel.Client.Update(this.ViewModel.Actions, this.ViewModel.DownloadTaskCancel.Token)
                        .ConfigureAwait(false);
                try
                {
                    await this.ViewModel.DownloadTask;
                }
                catch (UnauthorizedAccessException ex)
                {
                    this.log.Warning(ex, "Could not create direcotry for file download");
                    MessageBox.Show(
                        "Access to addon folder was denied." + Environment.NewLine + ex.Message,
                        "Folder access denied",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    this.ViewModel.CanDownload = false;
                    this.ViewModel.DownloadTaskCancel.Cancel();
                }

                this.ViewModel.CanCheck = true;
                this.ViewModel.CanDownload = false;
                if (!this.ViewModel.DownloadTaskCancel.IsCancellationRequested) this.ViewModel.CanStart = true;
            }
            else
            {
                this.ViewModel.CanDownload = false;
                this.ViewModel.DownloadTaskCancel.Cancel();
            }
        }

        private void Feedback(object sender, RoutedEventArgs e)
        {
            new Feedback().ShowDialog();
        }

        private void ChangeModpack(object sender, SelectionChangedEventArgs e)
        {
            this.ViewModel.CanStart = false;
            this.ViewModel.CanDownload = false;
            ArmaHelpers.ServerInfo(this.ViewModel.SelectedModpack).GetServerInfoAsync().ContinueWith(
                t =>
                    {
                        Log.Debug("{@server}", t.Result);
                        return this.Dispatcher.InvokeAsync(
                            () => this.ViewModel.ServerInfo = GetLogFor(t.Result),
                            DispatcherPriority.Background);
                    });
        }

        private async void Check(object sender, RoutedEventArgs e)
        {
            this.ViewModel.CanStart = false;
            var uri = new Uri(this.ViewModel.Url);
            var path = new DirectoryInfo(this.ViewModel.Path);
            this.ViewModel.Client = new Client(path, uri, this.ViewModel);
            this.ViewModel.CanCheck = false;
            this.ViewModel.Actions.Clear();
            this.ViewModel.BytesToDownload = 0;
            this.ViewModel.BytesDownloaded = 0;
            await this.ViewModel.Client.CollectActions(
                this.ViewModel.SelectedModpack.Addons.Select(
                    name => this.ViewModel.Repo.Addons.Find(addon => addon.Name == name)),
                this.ViewModel.Actions).ConfigureAwait(false);
            this.ViewModel.Recalculate();
            this.ViewModel.CanCheck = true;
            if (this.ViewModel.Actions.Any())
            {
                this.ViewModel.CanDownload = true;
            }
            else
            {
                this.ViewModel.CanStart = true;
            }
        }

        private async void LoadRepo(object sender, RoutedEventArgs e)
        {
            this.ViewModel.CanLoadRepo = false;
            this.ViewModel.Servers.Clear();
            var uri = new Uri(this.ViewModel.Url);
            this.ViewModel.Repo = await Client.DownloadRepo(uri).ConfigureAwait(false);
            this.ViewModel.SelectedModpack = this.ViewModel.Repo.Modpacks[0];

            await Task.WhenAll(
                this.ViewModel.Repo.Modpacks.Select(
                    m => ArmaHelpers.ServerInfo(m).GetServerInfoAsync().ContinueWith(
                        t =>
                            {
                                Log.Debug("{@server}", t.Result);
                                return this.Dispatcher.InvokeAsync(
                                    () => this.ViewModel.Servers.Add(t.Result),
                                    DispatcherPriority.Background);
                            }))).ConfigureAwait(false);

            this.ViewModel.CanLoadRepo = true;
            this.ViewModel.CanCheck = true;
        }

        private void PathChange(object sender, TextChangedEventArgs e)
        {
            this.ViewModel.CanStart = false;
            this.ViewModel.CanDownload = false;
        }

        private void ShowLogs(object sender, RoutedEventArgs e)
        {
            Process.Start(((App)Application.Current).LocalUserAppDataPath);
        }

        private void Start(object sender, RoutedEventArgs e)
        {
            ArmaHelpers.StartArma(
                this.ViewModel.SelectedModpack,
                this.ViewModel.Repo.Addons,
                new DirectoryInfo(this.ViewModel.Path),
                this.ViewModel.Params,
                this.Connect.IsChecked.Value,
                new DirectoryInfo(ViewModel.UserAddons).GetDirectories().Where(d => d.Name.StartsWith("@")).Select(d => d.FullName));
        }

        private void UnblockStart(object sender, RoutedEventArgs e)
        {
            this.ViewModel.CanStart = true;
        }
    }
}