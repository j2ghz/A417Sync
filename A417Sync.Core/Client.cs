namespace A417Sync.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    using A417Sync.Core.Models;

    using File = A417Sync.Core.Models.File;

    public class Client
    {
        public Client(DirectoryInfo local, Uri repoRootUri)
        {
            this.Local = local;
            this.RepoRootUri = repoRootUri;
        }

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

        public void Update(Modpack modpack,Repo repo)
        {
            Update(repo.Addons.Where(x => modpack.Addons.Contains(x.Name)));
        }

        public void Update(IEnumerable<Addon> addons)
        {
            var actions = new List<IFileAction>();
            foreach (var addon in addons)
            {
                actions.AddRange(DecideAddon(addon));
            }
        }

        private IEnumerable<IFileAction> DecideAddon(Addon addon)
        {
            var localFiles = new DirectoryInfo(Path.Combine(this.Local.FullName, addon.Name)).EnumerateFiles().ToList();
            foreach (var file in addon.Files)
            {
                var local = new FileInfo(Path.Combine(this.Local.FullName, addon.Name, file.Path));
                if (local.Exists)
                {
                    localFiles.RemoveAll(x => x.FullName == local.FullName);
                }
                yield return DecideFile(local, file, addon);
            }
            localFiles.ForEach(f => f.Delete()); //Delete files found in filesystem but not in index
        }

        private IFileAction DecideFile(FileInfo local, File remote, Addon addon)
        {
            if (!local.Exists) return new Download(local, remote, addon, this.RepoRootUri);
            if (local.Length != remote.Size) return new Download(local, remote, addon, this.RepoRootUri);
            if (local.LastWriteTimeUtc != remote.LastChange) return new Download(local, remote, addon, this.RepoRootUri);
            return null;
        }
    }
}