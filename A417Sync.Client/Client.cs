namespace A417Sync.WPF
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Threading;
    using System.Xml.Serialization;

    using A417Sync.WPF.Models;
    using A417Sync.WPF.Models.FileActions;

    using Serilog;

    using File = A417Sync.WPF.Models.File;

    public class Client
    {
        private ILogger log = Log.ForContext<Client>();

        public Client(DirectoryInfo local, Uri repoRootUri, MainWindowViewModel model)
        {
            this.Local = local;
            this.RepoRootUri = repoRootUri;
            this.Model = model;
        }

        public MainWindowViewModel Model { get; }

        private DirectoryInfo Local { get; }

        private Uri RepoRootUri { get; }

        public static async Task<Repo> DownloadRepo(Uri repoUri)
        {
            var log = Log.ForContext<Client>();
            log.Information("Downloading repo {url}", repoUri);
            var httpClient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, repoUri);
            var response = await httpClient.SendAsync(request).ConfigureAwait(false);
            var contentStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return LoadRepo(contentStream);
        }

        public static Repo LoadRepo(Stream s)
        {
            var xml = new XmlSerializer(typeof(Repo));

            return (Repo)xml.Deserialize(s);
        }

        public async Task CollectActions(IEnumerable<Addon> addons, ObservableCollection<IFileAction> actions)
        {
            this.log.Information("Checking {count} addons", addons.Count());
            foreach (var addon in addons)
            {
                await DecideAddon(addon, actions);
            }
        }

        public async Task Update(IEnumerable<IFileAction> actions, CancellationToken token)
        {
            this.log.Information("Processing {count} actions", actions.Count());
            foreach (var action in actions)
            {
                if (action != null)
                {
                    await action.DoAsync(token, new Progress<long>(x => this.Model.BytesDownloaded += x))
                        .ConfigureAwait(false);
                }
            }
        }

        private async Task AddActionAsync(IFileAction action, ObservableCollection<IFileAction> actions)
        {
            await Application.Current.Dispatcher.InvokeAsync(() => actions.Add(action), DispatcherPriority.Background);
        }

        private async Task DecideAddon(Addon addon, ObservableCollection<IFileAction> actions)
        {
            this.log.Information("Checking {addon}", addon.Name);
            var localFolder = new DirectoryInfo(Path.Combine(this.Local.FullName, addon.Name));
            List<FileInfo> localFiles = null;
            if (localFolder.Exists)
            {
                localFiles = localFolder.EnumerateFiles().ToList();
            }

            foreach (var file in addon.Files)
            {
                var local = new FileInfo(Path.Combine(this.Local.FullName, addon.Name, file.Path.Trim('/', '\\')));
                if (local.Exists)
                {
                    localFiles?.RemoveAll(x => x.FullName == local.FullName);
                }

                await DecideFile(local, file, addon, actions);
            }

            localFiles?.ForEach(f => f.Delete()); // Delete files found in filesystem but not in index
        }

        private async Task DecideFile(FileInfo local, File remote, Addon addon, ObservableCollection<IFileAction> actions)
        {
            this.Model.BytesToDownload += remote.Size;
            if (!local.Exists)
            {
                this.log.Debug("{file} missing", local.FullName);
                await AddActionAsync(
                    new Download(local, remote, addon, this.RepoRootUri, remote.LastChange) { Action = "Missing" },
                    actions);
            }
            else if (local.Length != remote.Size)
            {
                this.log.Debug("{file} size: {local}, expected: {remote}", local.Name, local.Length, remote.Size);
                await AddActionAsync(
                    new Download(local, remote, addon, this.RepoRootUri, remote.LastChange)
                    {
                        Action =
                                $"size is differrent, local: {local.Length}, remote: {remote.Size}"
                    },
                    actions);
            }
            else if (local.LastWriteTimeUtc.ToFileTimeUtc() != remote.LastChange)
            {
                this.log.Debug(
                    "{file} date: {local}, expected: {remote}",
                    local.Name,
                    local.LastWriteTimeUtc.ToFileTimeUtc(),
                    remote.Size);
                await AddActionAsync(
                    new Download(local, remote, addon, this.RepoRootUri, remote.LastChange)
                    {
                        Action =
                                $"date is different, local: {local.LastWriteTimeUtc.ToFileTimeUtc()}, remote: {remote.LastChange}"
                    },
                    actions);
            }
            else
            {
                this.Model.BytesToDownload -= remote.Size;
            }
        }
    }
}