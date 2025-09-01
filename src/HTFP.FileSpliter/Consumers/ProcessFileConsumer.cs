using System;
using System.Threading.Tasks;
using HTFP.FileSpliter.Services;
using HTFP.Shared.Bus.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HTFP.FileSpliter.Consumers;

public sealed class SplitFileConsumer : IConsumer<SplitFile>
{
    private readonly ILogger<SplitFileConsumer> _logger;
    private readonly FileSpliterService _fileSpliterService;
    public SplitFileConsumer(ILogger<SplitFileConsumer> logger, FileSpliterService fileSpliterService)
    {
        _logger = logger;
        _fileSpliterService = fileSpliterService;
    }

    public async Task Consume(ConsumeContext<SplitFile> context)
    {
        _logger.LogInformation("File {filepath} received to process at {date}.", context.Message.Path, DateTime.UtcNow);
        
        try
        {
            await _fileSpliterService.SplitAsync(context.Message);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Fail to process file {filepath}.", context.Message.Path);

            //throw;
        }
    }
}