namespace HTFP.Shared.Bus.Messages;

public record SplitFile
{
    public Guid ReconciliationId { get; init; }
    public int TotalSubFiles { get; init; }
    public int TotalLines { get; init; }
}