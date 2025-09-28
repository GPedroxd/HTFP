using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using HTFP.FileAggregator.Services;
using HTFP.Shared.Bus.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;
using OpenTelemetry;

namespace HTFP.FileAggregator.Consumers;

public class SubFilesProcessedConsumer : IConsumer<AggregateReconciliationResult>
{
    private readonly ILogger<SubFilesProcessedConsumer> _logger;
    private readonly FileAggregatorService _fileAggregatorService;

    public SubFilesProcessedConsumer(ILogger<SubFilesProcessedConsumer> logger, FileAggregatorService fileAggregatorService)
    {
        _logger = logger;
        _fileAggregatorService = fileAggregatorService;
    }

    public async Task Consume(ConsumeContext<AggregateReconciliationResult> context)
    {
        var fileId = Baggage.Current.GetBaggage("file.id");
        var activity = Activity.Current;
        activity?.SetTag("file.id", fileId);

        _logger.LogInformation("Received SubFilesProcessed message for ReconciliationId {ReconciliationId}", context.Message.ReconciliationId);

        try
        {
            await _fileAggregatorService.AggregateFilesAsync(context.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing SubFilesProcessed message for ReconciliationId {ReconciliationId}", context.Message.ReconciliationId);
            throw;
        }
    }
}