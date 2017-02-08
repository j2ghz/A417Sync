namespace A417Sync.Core.Models
{
    using System.ComponentModel;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IFileAction : INotifyPropertyChanged

    {
    string Path { get; }

    string Action { get; }

    double Progress { get; }

    Task DoAsync(CancellationToken token);
    }
}