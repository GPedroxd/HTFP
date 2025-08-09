using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync();

//do logging

var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += (sender, args) =>
{
    return Task.CompletedTask;
};

await channel.BasicConsumeAsync("queuename", autoAck: false, consumer: consumer);
