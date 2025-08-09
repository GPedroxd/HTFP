namespace HTFP.Shared.Queue;

public interface IMessageBroker
{
    Task SendAsync<T>(string queueName, QueueMessage<T> message) where T:struct;
}