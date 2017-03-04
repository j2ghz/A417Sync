namespace A417Sync.Client
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Text;

    using A417Sync.Client.Annotations;
    using A417Sync.Client.Models;
    using A417Sync.Client.Models.FileActions;

    using DerAtrox.Arma3LauncherLib.SSQLib.Model;

    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private long bytesDownloaded;

        private long bytesToDownload;

        private bool canDownload = false;

        private bool canLoadRepo = true;

        private bool canStart;

        private string downloadInfo;

        private long lastSpeedUpdateDownloaded;

        private DateTime lastSpeedUpdateTime;

        private double progress;

        private Repo repo;

        private Modpack selectedModpack;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<IFileAction> Actions { get; set; } = new ObservableCollection<IFileAction>();

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

        public bool CanDownload
        {
            get
            {
                return this.canDownload;
            }

            set
            {
                this.canDownload = value;
                OnPropertyChanged();
            }
        }

        public bool CanLoadRepo
        {
            get
            {
                return this.canLoadRepo;
            }

            set
            {
                this.canLoadRepo = value;
                OnPropertyChanged();
            }
        }

        public bool CanStart
        {
            get
            {
                return this.canStart;
            }

            set
            {
                this.canStart = value;
                OnPropertyChanged();
            }
        }

        public Client Client { get; set; }

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

        public string Path
        {
            get
            {
                return Properties.Settings.Default.path;
            }

            set
            {
                Properties.Settings.Default.path = value;
                Properties.Settings.Default.Save();
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
                if (double.IsNaN(value)) return;
                this.progress = value;
                OnPropertyChanged();
            }
        }

        public Repo Repo
        {
            get
            {
                return this.repo;
            }

            set
            {
                this.repo = value;
                OnPropertyChanged();
            }
        }

        public Modpack SelectedModpack
        {
            get
            {
                return this.selectedModpack;
            }

            set
            {
                this.selectedModpack = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ServerInfo> Servers { get; set; } = new ObservableCollection<ServerInfo>();

        public string Url
        {
            get
            {
                return Properties.Settings.Default.url;
            }

            set
            {
                Properties.Settings.Default.url = value;
                Properties.Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
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
                var remaining = new TimeSpan(0, 0, 0, (int)(this.bytesToDownload / speed));
                sb.AppendFormat(new FileSizeFormatProvider(), "{0:fs}", this.bytesDownloaded);
                sb.Append(" / ");
                sb.AppendFormat(new FileSizeFormatProvider(), "{0:fs}", this.bytesToDownload);
                sb.AppendLine();
                sb.AppendFormat(new DownloadSpeedFormatProvider(), "{0:sp}", speed);
                sb.AppendLine();
                sb.AppendFormat("{0:g}", remaining);
                this.DownloadInfo = sb.ToString();
                this.lastSpeedUpdateTime = DateTime.Now;
                this.lastSpeedUpdateDownloaded = this.bytesDownloaded;
            }
        }
    }
}