namespace A417Sync.Core.Models
{
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class FileAction
    {
        string Path { get;}
        string Action { get; }
        double Progress { get; }
        abstract Task DoAsync(CancellationToken token);
    }
}