namespace HTFP.Shared.Storage;

public interface IFileSpliter
{
    IAsyncEnumerable<Stream> SplitAsync(string path, int linePerFile = 1000);
}