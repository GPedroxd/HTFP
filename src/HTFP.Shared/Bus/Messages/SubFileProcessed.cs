namespace HTFP.Shared.Bus.Messages;

public record SubFileProcessed
{
    public Guid ReconciliationId { get; init; } = default!;
    public Guid Id { get; init; }
    public bool SuccessfullyProcessed { get; init; }
    public int TotalDivergents { get; init; }
}