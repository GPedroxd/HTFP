using HTFP.Shared.Models;

namespace HTFP.Shared.Storage;

public interface IOrderExtractor
{
    public IAsyncEnumerable<ExecutionOrder> ExtractOrdersAsync(string filePath);
}