using System.Threading.Tasks;
using MassTransit;
using HTFP.Shared.Bus.Messages;
using System;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using OpenTelemetry;
using HTFP.Services;

namespace HTFP.Coordinator.Consumers;

public record FileSplitConsumer : IConsumer<FileSplit>
{
    private readonly ILogger<FileSplitConsumer> _logger;
    private readonly CoordinatorService _coordinatorService;
    public async Task Consume(ConsumeContext<FileSplit> context)
    {
        var fileId = Baggage.Current.GetBaggage("file.id");
        var activity = Activity.Current;
        activity?.SetTag("file.id", fileId);

        try
        {
            await _coordinatorService.StartAsync(context.Message);
            _logger.LogInformation("FileSplit {fileid} message processed.", context.Message.ReconciliationId);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error processing FileSplit {fileid} message: {Error}", context.Message.ReconciliationId, ex.Message);
            throw;
        }
    }
}