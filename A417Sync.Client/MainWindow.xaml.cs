namespace A417Sync.Client
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Windows;

    using A417Sync.Core;

    using Microsoft.HockeyApp;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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
            await this.ViewModel.Client.Update(this.ViewModel.Actions, CancellationToken.None).ConfigureAwait(false);
            this.ViewModel.CanDownload = true;
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

        private async void LoadRepo(object sender, RoutedEventArgs e)
        {
            this.ViewModel.CanLoadRepo = false;
            var uri = new Uri(this.ViewModel.Url);
            var path = new DirectoryInfo(this.ViewModel.Path);
            this.ViewModel.Client = new Client(path, uri, this.ViewModel);
            this.ViewModel.Repo = await Client.DownloadRepo(uri).ConfigureAwait(false);
            this.ViewModel.Client.CollectActions(this.ViewModel.Repo.Addons, this.ViewModel.Actions);
            this.ViewModel.CanDownload = true;
        }

        private void Start(object sender, RoutedEventArgs e)
        {
            ArmaHelpers.StartArma(
                this.ViewModel.Repo.Modpacks.First(),
                this.ViewModel.Repo.Addons,
                new DirectoryInfo(this.ViewModel.Path),
                new List<string>());
        }
    }
}