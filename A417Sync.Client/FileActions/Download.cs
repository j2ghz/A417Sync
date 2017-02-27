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

        private double progress = 0;

        private string speed;

        public Download(FileInfo local, File file, Addon addon, Uri remote)
        {
            this.Path = local.FullName;
            this.requestUri = new Uri(remote, addon.Name + file.Path);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Action => "Download";

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

        private DateTime lastTime = DateTime.Now;

        private long lastSize = 0;

        public async Task DoAsync(CancellationToken token)
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(this.Path));

            var client = new WebClient();

            client.DownloadProgressChanged += Update;

            await client.DownloadFileTaskAsync(this.requestUri, this.Path);
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
            Progress = (e.BytesReceived * 1.0 / e.TotalBytesToReceive) * 100;
            var speed = (e.BytesReceived - this.lastSize) / (DateTime.Now - lastTime).TotalSeconds / 1024;
            Speed = $"{speed:N2} kB/s";
            this.lastTime = DateTime.Now;
            this.lastSize = e.BytesReceived;
        }
    }
}