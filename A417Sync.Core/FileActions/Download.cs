namespace A417Sync.Core
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;

    using A417Sync.Core.Models;

    using File = A417Sync.Core.Models.File;

    class Download : IFileAction
    {
        private readonly string path;

        private readonly Uri requestUri;

        public Download(FileInfo local, File file, Addon addon, Uri remote)
        {
            this.path = local.FullName;
            this.requestUri = new Uri(remote, addon.Name + file.Path);
        }

        public async Task DoAsync()
        {
            var httpClient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, this.requestUri);
            var response = await httpClient.SendAsync(request).ConfigureAwait(false);
            var contentStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var stream = new FileStream(this.path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);

            await contentStream.CopyToAsync(stream);
        }

        public override string ToString()
        {
            return "Download " + this.requestUri;
        }
    }
}