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
    using System.Threading;

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
