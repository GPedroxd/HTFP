namespace HTFP.Shared.Bus.Messages;

public record SubFileProcessed
{
    public string ReconciliationId { get; init; } = default!;
    public Guid Id { get; init; }
    public int TotalProcessed { get; init; }
}