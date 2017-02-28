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
        const double SMOOTHING_FACTOR = 0.005;

        private readonly Uri requestUri;

        private long lastSize = 0;

        private double lastSpeed;

        private DateTime lastTime = DateTime.Now;

        private DateTime lastWrite;

        private double progress = 0;

        private string speed;

        private DateTime start;

        public Download(FileInfo local, File file, Addon addon, Uri remote, DateTime lastWrite)
        {
            this.lastWrite = lastWrite;
            this.Path = local.FullName;
            this.requestUri = new Uri(remote, addon.Name + file.Path);
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

        public string Speed
        {
            get
            {
                return this.speed;
            }

            private set
            {
                this.speed = value;
                OnPropertyChanged();
            }
        }

        public async Task DoAsync(CancellationToken token)
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(this.Path));

            var client = new WebClient();

            client.DownloadProgressChanged += Update;

            this.start = DateTime.Now;

            await client.DownloadFileTaskAsync(this.requestUri, this.Path).ConfigureAwait(false);

            new FileInfo(this.Path).LastWriteTimeUtc = this.lastWrite;
        }

        public override string ToString()
        {
            return "Download " + this.requestUri;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Update(object sender, DownloadProgressChangedEventArgs e)
        {
            this.Progress = (e.BytesReceived * 1.0 / e.TotalBytesToReceive) * 100;
            if ((DateTime.Now - this.lastTime).TotalMilliseconds > 100)
            {
                var speed = (e.BytesReceived - this.lastSize) / (DateTime.Now - this.lastTime).TotalSeconds / 1024;
                if (double.IsInfinity(this.lastSpeed) || double.IsNaN(this.lastSpeed))
                {
                    this.lastSpeed = speed;
                }
                else
                {
                    this.lastSpeed = (SMOOTHING_FACTOR * speed) + ((1 - SMOOTHING_FACTOR) * this.lastSpeed);
                }

                var overallSpeed = e.BytesReceived / (DateTime.Now - this.start).TotalSeconds / 1024;
                this.Speed = $"{speed:N2} kB/s - {this.lastSpeed:N2} kB/s - {overallSpeed:N2} kB/s";
                this.lastTime = DateTime.Now;
                this.lastSize = e.BytesReceived;
            }
        }
    }
}