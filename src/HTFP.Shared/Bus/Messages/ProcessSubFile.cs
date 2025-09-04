namespace HTFP.Shared.Bus.Messages;

public record ProcessSubFile
{
    public Guid ParentFileId { get; init; }
    public Guid Id{ get; init; }
    public string FilePath { get; init; } = default!;
}