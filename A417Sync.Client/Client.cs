namespace A417Sync.Client
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

    using A417Sync.Core;
    using A417Sync.Core.Models;

    using Serilog;

    using File = A417Sync.Core.Models.File;

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
            var repo = RepoFactory.LoadRepo(contentStream);
            log.Verbose("{@repo}", repo);
            return repo;
        }

        public void CollectActions(IEnumerable<Addon> addons, ObservableCollection<IFileAction> actions)
        {
            this.log.Information("Checking {count} addons", addons.Count());
            foreach (var addon in addons)
            {
                foreach (var action in DecideAddon(addon))
                {
                    if (action != null)
                    {
                        Application.Current.Dispatcher.Invoke(() => actions.Add(action));
                    }
                }
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

        private IEnumerable<IFileAction> DecideAddon(Addon addon)
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

                yield return DecideFile(local, file, addon);
            }

            localFiles?.ForEach(f => f.Delete()); // Delete files found in filesystem but not in index
        }

        private IFileAction DecideFile(FileInfo local, File remote, Addon addon)
        {
            this.Model.BytesToDownload += remote.Size;
            if (!local.Exists)
            {
                this.log.Debug("{file} missing", local.FullName);
                return new Download(local, remote, addon, this.RepoRootUri, remote.LastChange) { Action = "Missing" };
            }

            if (local.Length != remote.Size)
            {
                this.log.Debug("{file} size: {local}, expected: {remote}", local.Name, local.Length, remote.Size);
                return new Download(local, remote, addon, this.RepoRootUri, remote.LastChange)
                           {
                               Action =
                                   $"size is differrent, local: {local.Length}, remote: {remote.Size}"
                           };
            }

            if (local.LastWriteTimeUtc.ToFileTimeUtc() != remote.LastChange)
            {
                this.log.Debug(
                    "{file} date: {local}, expected: {remote}",
                    local.Name,
                    local.LastWriteTimeUtc.ToFileTimeUtc(),
                    remote.Size);
                return new Download(local, remote, addon, this.RepoRootUri, remote.LastChange)
                           {
                               Action =
                                   $"date is different, local: {local.LastWriteTimeUtc.ToFileTimeUtc()}, remote: {remote.LastChange}"
                           };
            }

            this.Model.BytesToDownload -= remote.Size;
            return null;
        }
    }
}