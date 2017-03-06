namespace A417Sync.WPF.Models.FileActions
{
    using System;
    using System.ComponentModel;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IFileAction : INotifyPropertyChanged
    {
        string Action { get; }

        string Path { get; }

        double Progress { get; }

        string Size { get; }

        Task DoAsync(CancellationToken token, IProgress<long> progress);
    }
}