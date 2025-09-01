namespace HTFP.Shared.Bus.Messages;

public record SplitFile
{
    public string Path { get; init; } = default!;
}

