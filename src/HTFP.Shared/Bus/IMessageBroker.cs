namespace HTFP.Shared.Bus;

public interface IMessageBroker
{
    Task SendAsync<T>(string queueName, T message) where T:struct;
}