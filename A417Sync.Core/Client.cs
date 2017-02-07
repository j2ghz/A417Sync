namespace A417Sync.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
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

        public async Task Update(Modpack modpack, Repo repo)
        {
            await Update(repo.Addons.Where(x => modpack.Addons.Contains(x.Name))).ConfigureAwait(false);
        }

        public async Task Update(IEnumerable<Addon> addons)
        {
            var tasks = new List<Task>();
            var actions = new List<IFileAction>();
            foreach (var addon in addons)
            {
                actions.AddRange(DecideAddon(addon));
            }

            foreach (var fileAction in actions)
            {
                if (fileAction == null)
                {
                    continue;
                }

                Console.WriteLine(fileAction);
                await fileAction.DoAsync(CancellationToken.None).ConfigureAwait(false);
            }
        }

        private IEnumerable<IFileAction> DecideAddon(Addon addon)
        {
            var localFiles = new DirectoryInfo(Path.Combine(this.Local.FullName, addon.Name)).EnumerateFiles().ToList();
            foreach (var file in addon.Files)
            {
                var local = new FileInfo(Path.Combine(this.Local.FullName, addon.Name, file.Path.Trim('/', '\\')));
                if (local.Exists)
                {
                    localFiles.RemoveAll(x => x.FullName == local.FullName);
                }

                yield return DecideFile(local, file, addon);
            }

            localFiles.ForEach(f => f.Delete()); // Delete files found in filesystem but not in index
        }

        private IFileAction DecideFile(FileInfo local, File remote, Addon addon)
        {
            if (!local.Exists)
            {
                Console.WriteLine($"File {local.FullName} does not exist, scheduling download.");
                return new Download(local, remote, addon, this.RepoRootUri);
            }

            if (local.Length != remote.Size)
            {
                Console.WriteLine(
                    $"File {local.FullName} size is differrent, local: {local.Length}, remote: {remote.Size}");
                return new Download(local, remote, addon, this.RepoRootUri);
            }

            if (local.LastWriteTimeUtc.CompareTo(remote.LastChange) != 0)
            {
                Console.WriteLine(
                    $"File {local.FullName} date is different, local: {local.LastWriteTimeUtc}, remote: {remote.LastChange}");
                return new Download(local, remote, addon, this.RepoRootUri);
            }

            Console.WriteLine($"File {local.FullName} OK");
            return null;
        }
    }
}