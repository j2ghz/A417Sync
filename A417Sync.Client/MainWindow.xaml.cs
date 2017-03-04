namespace A417Sync.Client
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Threading;

    using Microsoft.HockeyApp;

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            HockeyClient.Current.TrackPageView(nameof(MainWindow));
            this.ViewModel = new MainWindowViewModel();
            InitializeComponent();
            this.DataContext = this.ViewModel;
        }

        public MainWindowViewModel ViewModel { get; private set; }

        private void ConsoleToggle(object sender, RoutedEventArgs e)
        {
            ConsoleManager.Toggle();
        }

        private async void Download(object sender, RoutedEventArgs e)
        {
            this.ViewModel.CanDownload = false;
            if (this.ViewModel.DownloadTask.GetAwaiter().IsCompleted)
            {
                this.ViewModel.DownloadTaskCancel = new CancellationTokenSource();
                this.ViewModel.DownloadTask =
                    this.ViewModel.Client.Update(this.ViewModel.Actions, this.ViewModel.DownloadTaskCancel.Token)
                        .ConfigureAwait(false);
                this.ViewModel.CanDownload = true;
                await this.ViewModel.DownloadTask;
                this.ViewModel.CanDownload = true;
                this.ViewModel.CanStart = true;
            }
            else
            {
                this.ViewModel.DownloadTaskCancel.Cancel();
            }
        }

        private async void Feedback(object sender, RoutedEventArgs e)
        {
            await HockeyClient.Current.CreateFeedbackThread()
                .PostFeedbackMessageAsync(
                    Microsoft.VisualBasic.Interaction.InputBox("Message"),
                    Microsoft.VisualBasic.Interaction.InputBox("Email"),
                    Microsoft.VisualBasic.Interaction.InputBox("Subject"),
                    Microsoft.VisualBasic.Interaction.InputBox("Name"))
                .ContinueWith(task => MessageBox.Show("Feedback sent"));
        }

        private async void Check(object sender, RoutedEventArgs e)
        {
            this.ViewModel.CanCheck = false;
            await this.ViewModel.Client.CollectActions(
                this.ViewModel.SelectedModpack.Addons.Select(
                    name => this.ViewModel.Repo.Addons.Find(addon => addon.Name == name)),
                this.ViewModel.Actions);
            this.ViewModel.CanCheck = true;
            this.ViewModel.CanDownload = true;
        }

        private async void LoadRepo(object sender, RoutedEventArgs e)
        {
            this.ViewModel.CanLoadRepo = false;
            var uri = new Uri(this.ViewModel.Url);
            var path = new DirectoryInfo(this.ViewModel.Path);
            this.ViewModel.Client = new Client(path, uri, this.ViewModel);
            this.ViewModel.Repo = await Client.DownloadRepo(uri).ConfigureAwait(false);
            this.ViewModel.SelectedModpack = this.ViewModel.Repo.Modpacks[0];

            foreach (var m in this.ViewModel.Repo.Modpacks)
            {
                var serverInfo = ArmaHelpers.ServerInfo(m).GetServerInfoAsync();
                serverInfo.ContinueWith(
                    t =>
                        {
                            Serilog.Log.Debug("{@server}", t.Result);
                            return this.Dispatcher.BeginInvoke(
                                DispatcherPriority.Background,
                                new Action(() => this.ViewModel.Servers.Add(t.Result)));
                        }).ConfigureAwait(false).GetAwaiter();
            }

            this.ViewModel.CanLoadRepo = true;
            this.ViewModel.CanCheck = true;
        }

        private void Start(object sender, RoutedEventArgs e)
        {
            ArmaHelpers.StartArma(
                this.ViewModel.Repo.Modpacks.First(),
                this.ViewModel.Repo.Addons,
                new DirectoryInfo(this.ViewModel.Path),
                new List<string>());
        }

        private void ShowLogs(object sender, RoutedEventArgs e)
        {
            Process.Start(((App)Application.Current).LocalUserAppDataPath);
        }
    }
}