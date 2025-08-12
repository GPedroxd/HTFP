using System;
using System.Threading.Tasks;
using HTFP.FileSpliter.Services;
using HTFP.Shared.Bus.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HTFP.FileSpliter.Consumers;

public sealed class ProcessFileConsumer : IConsumer<ProcessFile>
{
    private readonly ILogger<ProcessFileConsumer> _logger;
    private readonly FileSpliterService _fileSpliterService;
    public ProcessFileConsumer(ILogger<ProcessFileConsumer> logger, FileSpliterService fileSpliterService)
    {
        _logger = logger;
        _fileSpliterService = fileSpliterService;
    }

    public async Task Consume(ConsumeContext<ProcessFile> context)
    {
        _logger.LogInformation("File {filepath} received to process at {date}.", context.Message.Path, DateTime.UtcNow);
        
        try
        {
            await _fileSpliterService.SplitAsync(context.Message);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Fail to process file {filepath}.", context.Message.Path);

            throw;
        }
        
        throw new NotImplementedException();
    }
}