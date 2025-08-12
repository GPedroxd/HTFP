using System.Collections.Generic;
using System.IO;
using HTFP.Shared.Storage;

namespace HTFP.FileSpliter;

public sealed class CloudStorageFileSpliter : IFileSpliter
{
    public IAsyncEnumerable<Stream> SplitAsync(string path, int linePerFile = 1000)
    {
        throw new System.NotImplementedException();
    }
}