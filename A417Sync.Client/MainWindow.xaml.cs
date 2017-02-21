namespace A417Sync.Client
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;

    using A417Sync.Core;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadRepo(object sender, RoutedEventArgs e)
        {
            LoadRepo().GetAwaiter();
        }

        private async Task LoadRepo()
        {
            var uri = new Uri(this.InputUrl.Text);
            var repo = await Client.DownloadRepo(uri);
            var path = new DirectoryInfo(InputPath.Text);
            var client = new Client(path, uri);
            var actions = client.CollectActions(repo.Addons);
            queueListView.ItemsSource = actions;
            await client.Update(actions, CancellationToken.None);
        }
    }
}