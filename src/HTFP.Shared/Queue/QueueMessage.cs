namespace HTFP.Shared.Queue;

public sealed class QueueMessage<T> where T : struct
{
    public Guid Id { get; } = Guid.NewGuid();
    public Guid CorrelationId { get; init; }
    public DateTime PublishDate { get; init; } = DateTime.UtcNow;
    public T Message { get; init; }
}