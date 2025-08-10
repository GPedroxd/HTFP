namespace HTFP.Shared.Bus.Messages;

public record ProcessFile
{
    public string Path { get; init; } = default!;
}