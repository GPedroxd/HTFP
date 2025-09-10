using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using HTFP.Shared.Bus.Messages;
using HTFP.SubFileProcessor.Services;
using MassTransit;
using Microsoft.Extensions.Logging;
using OpenTelemetry;

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
        var fileId = Baggage.Current.GetBaggage("file.id");
        var activity = Activity.Current;
        activity?.SetTag("file.id", fileId);
        
        _logger.LogInformation("Processing message {mid} from {fileid} file.", context.MessageId, fileId);

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