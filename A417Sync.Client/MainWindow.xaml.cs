namespace A417Sync.Client
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;

    using Microsoft.HockeyApp;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            HockeyClient.Current.TrackPageView(nameof(MainWindow));
            InitializeComponent();
        }

        private async void LoadRepo(object sender, RoutedEventArgs e)
        {
            this.btnLoad.IsEnabled = false;
            var uri = new Uri(this.InputUrl.Text);
            var path = new DirectoryInfo(this.InputPath.Text);
            var client = new Client(path, uri);
            this.DataContext = client.Model;
            var repo = await Client.DownloadRepo(uri).ConfigureAwait(false);
            var actions = client.CollectActions(repo.Addons);
            await client.Update(actions, CancellationToken.None).ConfigureAwait(false);
        }

        private void Start(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ConsoleToggle(object sender, RoutedEventArgs e)
        {
            ConsoleManager.Toggle();
        }
    }
}