using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using A417Sync.Client.Annotations;
using A417Sync.Client.Models;
using A417Sync.Client.Models.FileActions;

namespace A417Sync.Client
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private bool _battleEye = false;

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

        private string serverInfo;

        private Brush startColor = new SolidColorBrush(Color.FromRgb(221, 221, 221));

        private string userAddons;

        public ObservableCollection<IFileAction> Actions { get; set; } = new ObservableCollection<IFileAction>();

        public long BytesDownloaded
        {
            get { return this.bytesDownloaded; }

            set
            {
                this.bytesDownloaded = value;
                Recalculate();
                OnPropertyChanged();
            }
        }

        public bool BattleEye
        {
            get { return this._battleEye; }
            set
            {
                this._battleEye = value;
                OnPropertyChanged();
            }
        }

        public long BytesToDownload
        {
            get { return this.bytesToDownload; }

            set
            {
                this.bytesToDownload = value;
                Recalculate();
                OnPropertyChanged();
            }
        }

        public long BytesToDownloadRemaining => this.BytesToDownload - this.BytesDownloaded;

        public bool CanDownload
        {
            get { return this.canDownload; }

            set
            {
                this.canDownload = value;
                OnPropertyChanged();
            }
        }

        public bool CanCheck
        {
            get { return this.canCheck; }

            set
            {
                this.canCheck = value;
                OnPropertyChanged();
            }
        }

        public bool CanLoadRepo
        {
            get { return this.canLoadRepo; }

            set
            {
                this.canLoadRepo = value;
                OnPropertyChanged();
            }
        }

        public bool CanStart
        {
            get { return this.canStart; }

            set
            {
                this.canStart = value;
                this.StartColor = value ? new SolidColorBrush(Color.FromRgb(221, 221, 221)) : Brushes.LightGreen;
                OnPropertyChanged();
            }
        }

        public Client Client { get; set; }

        public string DownloadInfo
        {
            get { return this.downloadInfo; }

            set
            {
                this.downloadInfo = value;
                OnPropertyChanged();
            }
        }

        public ConfiguredTaskAwaitable DownloadTask { get; set; } = Task.WhenAll().ConfigureAwait(false);

        public CancellationTokenSource DownloadTaskCancel { get; set; } = new CancellationTokenSource();

        public List<string> Params => new List<string>() {this.UserParams, this.SelectedModpack.AdditionalParams};

        public string Path
        {
            get { return Properties.Settings.Default.path; }

            set
            {
                Properties.Settings.Default.path = value;
                Properties.Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public double Progress
        {
            get { return this.progress; }

            set
            {
                if (double.IsNaN(value)) return;
                this.progress = value;
                OnPropertyChanged();
            }
        }

        public Repo Repo
        {
            get { return this.repo; }

            set
            {
                this.repo = value;
                OnPropertyChanged();
            }
        }

        public Modpack SelectedModpack
        {
            get { return this.selectedModpack; }

            set
            {
                this.selectedModpack = value;
                OnPropertyChanged();
            }
        }

        public string ServerInfo
        {
            get { return this.serverInfo; }

            set
            {
                this.serverInfo = value;
                OnPropertyChanged();
            }
        }

        public bool Set64Bit
        {
            get { return Properties.Settings.Default.Set64Bit; }

            set
            {
                Properties.Settings.Default.Set64Bit = value;
                Properties.Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public Brush StartColor
        {
            get { return this.startColor; }

            set
            {
                this.startColor = value;
                OnPropertyChanged();
            }
        }

        ////public ObservableCollection<ServerInfo> Servers { get; set; } = new ObservableCollection<ServerInfo>();
        public string Url
        {
            get { return Properties.Settings.Default.url; }

            set
            {
                Properties.Settings.Default.url = value;
                Properties.Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public string UserAddons
        {
            get { return this.userAddons; }

            set
            {
                this.userAddons = value;
                OnPropertyChanged();
            }
        }

        public string UserParams
        {
            get { return Properties.Settings.Default.userParameters; }

            set
            {
                Properties.Settings.Default.userParameters = value;
                Properties.Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Recalculate(bool force = false)
        {
            this.Progress = 1D * this.BytesDownloaded / this.BytesToDownload * 100D;
            var interval = (DateTime.Now - this.lastSpeedUpdateTime).TotalSeconds;
            if (force || interval > 1)
            {
                var speed = (this.bytesDownloaded - this.lastSpeedUpdateDownloaded) / interval;
                var sb = new StringBuilder();
                var remaining = new TimeSpan((long) (this.BytesToDownloadRemaining / speed * TimeSpan.TicksPerSecond));
                if (remaining.CompareTo(TimeSpan.Zero) < 0)
                {
                    remaining = new TimeSpan(0);
                }

                sb.AppendFormat("{0:P0} ", this.Progress / 100);
                sb.AppendFormat(
                    new FileSizeFormatProvider(),
                    "({0:fs} / {1:fs})",
                    this.bytesDownloaded,
                    this.bytesToDownload);
                sb.Append("\t");
                sb.AppendFormat(new DownloadSpeedFormatProvider(), "{0:sp}", speed);
                sb.Append("\tETA: ");
                sb.AppendFormat(new DynamicTimeSpanFormatProvider(), "{0:ts}", remaining);
                this.DownloadInfo = sb.ToString();
                this.lastSpeedUpdateTime = DateTime.Now;
                this.lastSpeedUpdateDownloaded = this.bytesDownloaded;
            }
        }

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}