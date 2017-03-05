namespace A417Sync.Client.Models.FileActions
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    using A417Sync.Client.Models;

    using Serilog;

    using File = A417Sync.Client.Models.File;

    public class Download : IFileAction
    {
        private readonly long lastRemoteWrite;

        private readonly Uri requestUri;

        private long lastSize = 0;

        private ILogger log = Log.ForContext<Download>();

        private double progress;

        public Download(FileInfo local, File file, Addon addon, Uri remote, long lastWrite)
        {
            this.lastRemoteWrite = lastWrite;
            this.Path = local.FullName;
            this.requestUri = new Uri(remote, addon.Name + file.Path);
            this.Size = string.Format(new FileSizeFormatProvider(), "{0:fs}", file.Size);
            this.log.Debug("Scheduling download of {url} to {file}", this.requestUri.ToString(), this.Path);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Action { get; set; } = "Download";

        public string Path { get; }

        public double Progress
        {
            get
            {
                return this.progress;
            }

            private set
            {
                this.progress = value;
                OnPropertyChanged();
            }
        }

        public string Size { get; private set; }

        public async Task DoAsync(CancellationToken token, IProgress<long> progress)
        {
            if (token.IsCancellationRequested)
            {
                this.log.Debug("Cancelled download of {file}", this.requestUri);
                return;
            }

            this.log.Information("Downloading {url}", this.requestUri);

            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(this.Path));

            var client = new WebClient();

            client.DownloadProgressChanged += (sender, args) =>
                {
                    progress.Report(args.BytesReceived - this.lastSize);
                    this.lastSize = args.BytesReceived;
                    this.Progress = 1D * args.BytesReceived / args.TotalBytesToReceive * 100;
                };

            await client.DownloadFileTaskAsync(this.requestUri, this.Path).ConfigureAwait(false);

            this.log.Information("Downloaded {url}", this.requestUri);

            this.log.Debug("Changing {property} to {value}", nameof(FileInfo.LastWriteTimeUtc), this.lastRemoteWrite);

            new FileInfo(this.Path).LastWriteTimeUtc = DateTime.FromFileTimeUtc(this.lastRemoteWrite);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}