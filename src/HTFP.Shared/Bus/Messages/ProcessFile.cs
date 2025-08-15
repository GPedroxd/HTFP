namespace HTFP.Shared.Bus.Messages;

public record ProcessFile
{
    public string Name { get; init; } = default!;
}