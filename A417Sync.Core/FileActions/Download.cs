namespace A417Sync.Core
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
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
            ////var httpClient = new HttpClient();
            ////var request = new HttpRequestMessage(HttpMethod.Get, this.requestUri);
            ////var response = await httpClient.SendAsync(request).ConfigureAwait(false);
            ////var contentStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            ////var stream = new FileStream(this.path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);

            ////await contentStream.CopyToAsync(stream);
            var p = new Progress<double>();
            p.ProgressChanged += (sender, d) => Console.WriteLine(d);
            await DownloadFileAsync(this.requestUri, p, CancellationToken.None);

        }

        private async Task DownloadFileAsync(Uri url, IProgress<double> progress, CancellationToken token)
        {
            var client = new HttpClient();

            var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(string.Format("The request returned with HTTP status code {0}", response.StatusCode));
            }

            var total = response.Content.Headers.ContentLength ?? -1L;
            var canReportProgress = total != -1 && progress != null;

            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                var totalRead = 0L;
                var buffer = new byte[4096];
                var isMoreToRead = true;
                var fileStream = new FileStream(this.path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);

                do
                {
                    token.ThrowIfCancellationRequested();

                    var read = await stream.ReadAsync(buffer, 0, buffer.Length, token);

                    if (read == 0)
                    {
                        isMoreToRead = false;
                    }
                    else
                    {
                        var data = new byte[read];
                        buffer.ToList().CopyTo(0, data, 0, read);

                        // TODO: put here the code to write the file to disk
                        await fileStream.WriteAsync(data, 0, read, token);

                        totalRead += read;

                        if (canReportProgress)
                        {
                            progress.Report((totalRead * 1d) / (total * 1d) * 100);
                        }
                    }
                }
                while (isMoreToRead);
            }
        }

        public override string ToString()
        {
            return "Download " + this.requestUri;
        }
    }
}