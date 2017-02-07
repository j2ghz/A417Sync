namespace A417Sync.Core
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using A417Sync.Core.Models;

    public class Delete : IFileAction
    {
        public Delete(FileInfo file)
        {
            this.File = file;
        }

        private FileInfo File { get; set; }

        public Task DoAsync(IProgress<double> progress, CancellationToken token)
        {
            if (!token.IsCancellationRequested)
            {
                this.File.Delete();
            }

            return Task.CompletedTask;
        }

        public override string ToString()
        {
            return "Delete " + this.File.FullName;
        }
    }
}