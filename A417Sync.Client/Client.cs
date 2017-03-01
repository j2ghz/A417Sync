namespace A417Sync.Client
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    using A417Sync.Core;
    using A417Sync.Core.Models;

    using Serilog;

    using File = A417Sync.Core.Models.File;

    public class Client
    {
        public Client(DirectoryInfo local, Uri repoRootUri)
        {
            this.Local = local;
            this.RepoRootUri = repoRootUri;
            this.Model = new ClientViewModel();
        }

        public ClientViewModel Model { get; }

        private DirectoryInfo Local { get; }

        private Uri RepoRootUri { get; }

        public static async Task<Repo> DownloadRepo(Uri repoUri)
        {
            var httpClient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, repoUri);
            var response = await httpClient.SendAsync(request).ConfigureAwait(false);
            var contentStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return RepoFactory.LoadRepo(contentStream);
        }

        public IEnumerable<IFileAction> CollectActions(IEnumerable<Addon> addons)
        {
            var actions = new List<IFileAction>();
            foreach (var addon in addons)
            {
                actions.AddRange(DecideAddon(addon));
            }

            return actions.Where(x => x != null);
        }

        public async Task Update(IEnumerable<IFileAction> actions, CancellationToken token)
        {
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
                return new Download(local, remote, addon, this.RepoRootUri, remote.LastChange) { Action = "Missing" };
            }

            if (local.Length != remote.Size)
            {
                return new Download(local, remote, addon, this.RepoRootUri, remote.LastChange)
                           {
                               Action =
                                   $"size is differrent, local: {local.Length}, remote: {remote.Size}"
                           };
            }

            if (local.LastWriteTimeUtc.CompareTo(remote.LastChange) != 0)
            {
                Log.Information("Scheduling {file} to download, date is different: {@local} vs {@remote}", local.Name, local.LastWriteTimeUtc, remote.LastChange);
                return new Download(local, remote, addon, this.RepoRootUri, remote.LastChange)
                           {
                               Action =
                                   $"date is different, local: {local.LastWriteTimeUtc}, remote: {remote.LastChange}"
                           };
            }

            this.Model.BytesToDownload -= remote.Size;
            return null;
        }
    }
}