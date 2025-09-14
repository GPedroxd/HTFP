namespace HTFP.Shared.Bus.Messages;

public record SubFilesProcessed
{
    public Guid ReconciliationId { get; init; } = default!;
}