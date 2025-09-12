namespace HTFP.Shared.Bus.Messages;

public record FileSplit
{
    public Guid ReconciliationId { get; init; }
    public int TotalSubFiles { get; init; }
    public int TotalLines { get; init; }
}