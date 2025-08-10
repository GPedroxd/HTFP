namespace HTFP.Shared.Bus;

public interface IMessageBroker
{
    Task SendAsync<T>(string queueName, QueueMessage<T> message) where T:struct;
}