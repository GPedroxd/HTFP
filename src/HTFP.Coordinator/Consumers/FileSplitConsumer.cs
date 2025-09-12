using System.Threading.Tasks;
using MassTransit;
using HTFP.Shared.Bus.Messages;
using System;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using OpenTelemetry;

namespace HTFP.Coordinator.Consumers;

public record FileSplitConsumer : IConsumer<FileSplit>
{
    private readonly ILogger<FileSplitConsumer> _logger;

    public async Task Consume(ConsumeContext<FileSplit> context)
    {
        var fileId = Baggage.Current.GetBaggage("file.id");
        var activity = Activity.Current;
        activity?.SetTag("file.id", fileId);

        try
        {
            
        }
        catch (Exception ex)
        {
            _logger.LogError("Error processing FileSplit {fileid} message: {Error}", context.Message.ReconciliationId, ex.Message);
            throw;
        }
    }
}