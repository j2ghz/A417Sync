namespace A417Sync.Client
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Text;

    using A417Sync.Client.Annotations;

    public class ClientViewModel : INotifyPropertyChanged
    {
        private long bytesDownloaded;

        private long bytesToDownload;

        private long lastSpeedUpdateDownloaded;

        private DateTime lastSpeedUpdateTime;

        private double progress;

        private string downloadInfo;

        public event PropertyChangedEventHandler PropertyChanged;

        public long BytesDownloaded
        {
            get
            {
                return this.bytesDownloaded;
            }

            set
            {
                this.bytesDownloaded = value;
                Recalculate();
                OnPropertyChanged();
            }
        }

        public long BytesToDownload
        {
            get
            {
                return this.bytesToDownload;
            }

            set
            {
                this.bytesToDownload = value;
                Recalculate();
                OnPropertyChanged();
            }
        }

        public double Progress
        {
            get
            {
                return this.progress;
            }

            set
            {
                this.progress = value;
                OnPropertyChanged();
            }
        }

        public string DownloadInfo
        {
            get
            {
                return this.downloadInfo;
            }

            set
            {
                this.downloadInfo = value;
                OnPropertyChanged();
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Recalculate()
        {
            this.Progress = 1D * this.BytesDownloaded / this.BytesToDownload * 100D;
            var interval = (DateTime.Now - this.lastSpeedUpdateTime).TotalSeconds;
            if (interval > 1)
            {
                var speed = (this.bytesDownloaded - this.lastSpeedUpdateDownloaded) / interval;
                var sb = new StringBuilder();
                sb.AppendFormat(new FileSizeFormatProvider(), "{0:fs}", this.bytesDownloaded);
                sb.Append(" / ");
                sb.AppendFormat(new FileSizeFormatProvider(), "{0:fs}", this.bytesToDownload);
                sb.AppendLine();
                sb.AppendFormat(new DownloadSpeedFormatProvider(), "{0:sp}", speed);
                this.DownloadInfo = sb.ToString();
                this.lastSpeedUpdateTime = DateTime.Now;
                this.lastSpeedUpdateDownloaded = this.bytesDownloaded;
            }
        }
    }
}