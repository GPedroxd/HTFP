namespace HTFP.Shared.Bus.Messages;

public record SubFilesProcessed
{
    public Guid ReconciliationId { get; init; } = default!;
    public int Divergents { get; init; }
}