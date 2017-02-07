namespace A417Sync.Core.Models
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IFileAction
    {
        Task DoAsync(IProgress<double> progress, CancellationToken token);
    }
}