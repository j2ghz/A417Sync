using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace A417Sync.Client
{
    using System.IO;
    using System.Net;
    using System.Threading;
    using System.Windows.Threading;

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
            InitializeComponent();
        }

        private void LoadRepo(object sender, RoutedEventArgs e)
        {
            this.btnLoad.IsEnabled = false;
            LoadRepo();
        }

        private async Task LoadRepo()
        {
            var uri = new Uri(this.InputUrl.Text);
            var path = new DirectoryInfo(InputPath.Text);
            var repo = await Client.DownloadRepo(uri);
            var client = new Client(path, uri);
            var actions = client.CollectActions(repo.Addons);
            queueListView.ItemsSource = actions;
            await client.Update(actions, CancellationToken.None, int.Parse(this.concurrent.Text)).ConfigureAwait(false);
        }
    }
}
