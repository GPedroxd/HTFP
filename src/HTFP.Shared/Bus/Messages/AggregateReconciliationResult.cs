namespace HTFP.Shared.Bus.Messages;

public record AggregateReconciliationResult
{
    public Guid ReconciliationId { get; init; } = default!;
    public int Divergents { get; init; }
}