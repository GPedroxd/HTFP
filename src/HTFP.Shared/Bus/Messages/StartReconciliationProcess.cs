namespace HTFP.Shared.Bus.Messages;

public record StartReconciliationProcess
{
    public string Path { get; init; } = default!;
}

