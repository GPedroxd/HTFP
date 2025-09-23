namespace HTFP.Shared.Models;

public sealed class StatusRecord
{
    public FileStatus Status { get; init; } = default!;
    public string StatusString { get => Status.ToString(); private set { } }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string? Message { get; init; }
}