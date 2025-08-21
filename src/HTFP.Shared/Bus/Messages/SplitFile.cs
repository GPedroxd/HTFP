namespace HTFP.Shared.Bus.Messages;

public record SplitFile
{
    public string Name { get; init; } = default!;
}

