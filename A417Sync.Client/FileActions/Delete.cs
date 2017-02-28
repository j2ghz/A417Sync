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

    public class Delete : IFileAction
    {
        private double progress = 0;

        public Delete(FileInfo file)
        {
            this.File = file;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Action => "Delete";

        public string Path => this.File.FullName;

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

        private FileInfo File { get; set; }

        public Task DoAsync(CancellationToken token, IProgress<long> progress)
        {
            if (!token.IsCancellationRequested)
            {
                this.File.Delete();
            }

            this.Progress = 100;

            return Task.CompletedTask;
        }

        public override string ToString()
        {
            return "Delete " + this.File.FullName;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string Speed { get; }
    }
}