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

    public class Download : IFileAction
    {
        private readonly string path;

        private readonly Uri requestUri;

        public Download(FileInfo local, File file, Addon addon, Uri remote)
        {
            this.path = local.FullName;
            this.requestUri = new Uri(remote, addon.Name + file.Path);
        }

        public async Task DoAsync(IProgress<double> progress, CancellationToken token)
        {
            var client = new HttpClient();

            var response =
                await client.GetAsync(this.requestUri, HttpCompletionOption.ResponseHeadersRead, token)
                    .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    string.Format("The request returned with HTTP status code {0}", response.StatusCode));
            }

            var total = response.Content.Headers.ContentLength ?? -1L;
            var canReportProgress = total != -1 && progress != null;

            using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
            {
                var totalRead = 0L;
                var buffer = new byte[4096];
                var isMoreToRead = true;
                var fileStream = new FileStream(
                    this.path,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    4096,
                    true);

                do
                {
                    token.ThrowIfCancellationRequested();

                    var read = await stream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false);

                    if (read == 0)
                    {
                        isMoreToRead = false;
                    }
                    else
                    {
                        var data = new byte[read];
                        buffer.ToList().CopyTo(0, data, 0, read);

                        // TODO: put here the code to write the file to disk
                        await fileStream.WriteAsync(data, 0, read, token).ConfigureAwait(false);

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