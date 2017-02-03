using System;
using System.Collections.Generic;
using System.Text;

namespace A417Sync.Core.Models
{
    using System.Threading.Tasks;

    public interface IFileAction
    {
        Task DoAsync();
    }
}
