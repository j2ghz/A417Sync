namespace A417Sync.Client
{
    using System;
    using System.ComponentModel;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IFileAction : INotifyPropertyChanged
    {
        string Action { get; }

        string Path { get; }

        Task DoAsync(CancellationToken token, IProgress<long> progress);
    }
}