namespace A417Sync.Core
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    using A417Sync.Core.Models;

    public class Delete : IFileAction
    {
        public string Action => "Delete";

        public string Path => File.FullName;

        public double Progress { get; private set; } = 0;

        public Delete(FileInfo file)
        {
            this.File = file;
        }

        private FileInfo File { get; set; }

        public Task DoAsync(CancellationToken token)
        {
            if (!token.IsCancellationRequested)
            {
                this.File.Delete();
            }

            Progress = 100;
            
            return Task.CompletedTask;
        }

        public override string ToString()
        {
            return "Delete " + this.File.FullName;
        }
    }
}