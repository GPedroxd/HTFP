namespace HTFP.Shared.Storage;

public interface IFileSpliter
{
    IAsyncEnumerable<Stream> Split(string path, int linePerFile = 1000);
}