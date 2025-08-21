using System.Threading.Tasks;
using HTFP.Shared.Bus.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HTFP.SubFileProcessor.Consumers;

public sealed class SubFileConsumer : IConsumer<ProcessSubFile>
{
    private readonly ILogger<SubFileConsumer> _logger;

    public SubFileConsumer(ILogger<SubFileConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProcessSubFile> context)
    {
        await Task.CompletedTask;
        _logger.LogInformation("Processing sub-file: {FileName}", context.Message.FilePath);
    }
}