namespace A417Sync.Client
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Media;
    using System.Windows.Threading;

    using A417Sync.Client.Annotations;
    using A417Sync.Client.Models;
    using A417Sync.Client.Models.FileActions;

    using DerAtrox.Arma3LauncherLib.SSQLib.Model;

    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private long bytesDownloaded;

        private long bytesToDownload;

        private bool canDownload = false;

        private bool canCheck;

        private bool canLoadRepo = true;

        private bool canStart;

        private string downloadInfo;

        private long lastSpeedUpdateDownloaded;

        private DateTime lastSpeedUpdateTime;

        private double progress;

        private Repo repo;

        private Modpack selectedModpack;

        private Brush startColor = new SolidColorBrush(Color.FromRgb(221, 221, 221));

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

        public bool CanCheck
        {
            get
            {
                return this.canCheck;
            }

            set
            {
                this.canCheck = value;
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
                this.StartColor = value ? new SolidColorBrush(Color.FromRgb(221, 221, 221)) : Brushes.LightGreen;
                OnPropertyChanged();
            }
        }

        public Brush StartColor
        {
            get
            {
                return this.startColor;
            }
            set
            {
                this.startColor = value;
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

        public ConfiguredTaskAwaitable DownloadTask { get; set; } = Task.CompletedTask.ConfigureAwait(false);

        public CancellationTokenSource DownloadTaskCancel { get; set; } = new CancellationTokenSource();

        public List<string> Params
            => new List<string>()
            {
                this.UserParams,
                this.SelectedModpack.AdditionalParams
            };

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

        public string UserParams
        {
            get
            {
                return Properties.Settings.Default.userParameters;
            }

            set
            {
                Properties.Settings.Default.userParameters = value;
                Properties.Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Recalculate()
        {
            this.Progress = 1D * this.BytesDownloaded / this.BytesToDownload * 100D;
            var interval = (DateTime.Now - this.lastSpeedUpdateTime).TotalSeconds;
            if (interval > 1)
            {
                var speed = (this.bytesDownloaded - this.lastSpeedUpdateDownloaded) / interval;
                var sb = new StringBuilder();
                var remaining = new TimeSpan(0, 0, 0, (int)(this.bytesToDownload / speed));
                sb.Append("Download progress: ");
                sb.AppendFormat(new FileSizeFormatProvider(), "{0:fs}", this.bytesDownloaded);
                sb.Append(" / ");
                sb.AppendFormat(new FileSizeFormatProvider(), "{0:fs}", this.bytesToDownload);
                sb.AppendLine();
                sb.Append("Download speed: ");
                sb.AppendFormat(new DownloadSpeedFormatProvider(), "{0:sp}", speed);
                sb.AppendLine();
                sb.Append("Remaining time: ");
                sb.AppendFormat("{0:g}", remaining);
                this.DownloadInfo = sb.ToString();
                this.lastSpeedUpdateTime = DateTime.Now;
                this.lastSpeedUpdateDownloaded = this.bytesDownloaded;
            }
        }
    }
}