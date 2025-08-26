namespace HTFP.Shared.Storage;

public interface IFileSpliter
{
    IAsyncEnumerable<(Stream stream, int lineCount)> SplitAsync(string path, int linePerFile = 1000);
}