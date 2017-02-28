namespace A417Sync.Client
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    using A417Sync.Core.Models;

    using File = A417Sync.Core.Models.File;

    public class Download : IFileAction
    {

        private readonly Uri requestUri;

        private readonly DateTime lastRemoteWrite;

        private long lastSize  = 0;

        public Download(FileInfo local, File file, Addon addon, Uri remote, DateTime lastWrite)
        {
            this.lastRemoteWrite = lastWrite;
            this.Path = local.FullName;
            this.requestUri = new Uri(remote, addon.Name + file.Path);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Action { get; set; } = "Download";

        public string Path { get; }

        public async Task DoAsync(CancellationToken token, IProgress<long> progress)
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(this.Path));

            var client = new WebClient();

            client.DownloadProgressChanged += (sender, args) =>
                {
                    progress.Report(args.BytesReceived - lastSize);
                    this.lastSize = args.BytesReceived;
                };

            await client.DownloadFileTaskAsync(this.requestUri, this.Path).ConfigureAwait(false);

            new FileInfo(this.Path).LastWriteTimeUtc = this.lastRemoteWrite;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}