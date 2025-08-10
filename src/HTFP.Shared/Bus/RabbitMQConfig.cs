namespace HTFP.Shared.Bus;

public struct RabbitMQConfig
{
    public const string Position = nameof(RabbitMQConfig);
    public string Host { get; set; }
    public string Username { get; init; }
    public string Password { get; init; }
}