using System.Threading.Tasks;
using MassTransit;
using HTFP.Shared.Bus.Messages;
using System;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using OpenTelemetry;
using HTFP.Services;

namespace HTFP.Coordinator.Consumers;

public record FileSplitConsumer : IConsumer<SplitFile>
{
    private readonly ILogger<FileSplitConsumer> _logger;
    private readonly CoordinatorService _coordinatorService;

    public FileSplitConsumer(ILogger<FileSplitConsumer> logger, CoordinatorService coordinatorService)
    {
        _logger = logger;
        _coordinatorService = coordinatorService;
    }

    public async Task Consume(ConsumeContext<SplitFile> context)
    {
        var fileId = Baggage.Current.GetBaggage("file.id");
        var activity = Activity.Current;
        activity?.SetTag("file.id", fileId);

        try
        {
            _logger.LogInformation("FileSplit {fileid} message processing.", context.Message.ReconciliationId);
            await _coordinatorService.StartAsync(context.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error processing FileSplit {fileid} message: {Error}", context.Message.ReconciliationId, ex.Message);
            throw;
        }
    }
}