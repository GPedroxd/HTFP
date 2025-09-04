using System;
using System.Threading.Tasks;
using HTFP.Shared.Bus.Messages;
using HTFP.SubFileProcessor.Services;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HTFP.SubFileProcessor.Consumers;

public sealed class SubFileConsumer : IConsumer<ProcessSubFile>
{
    private readonly ILogger<SubFileConsumer> _logger;
    private readonly SubfileService _subfileService;
    public SubFileConsumer(ILogger<SubFileConsumer> logger, SubfileService subfileService)
    {
        _logger = logger;
        _subfileService = subfileService;
    }

    public async Task Consume(ConsumeContext<ProcessSubFile> context)
    {
        _logger.LogInformation("Processing message {mid}", context.MessageId);

        try
        {
            await _subfileService.ProcessSubfileAsync(context.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing sub-file: {FileName}", context.Message.FilePath);
            throw;
        }
    }
}