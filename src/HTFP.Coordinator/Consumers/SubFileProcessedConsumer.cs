using System;
using System.Threading.Tasks;
using DnsClient.Internal;
using HTFP.Services;
using HTFP.Shared.Bus.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HTFP.Coordinator.Consumers;

public class SubFileProcessedConsumer : IConsumer<SubFileProcessed>
{
    private readonly CoordinatorService _coordinatorService;
    private readonly ILogger<SubFileProcessedConsumer> _logger;

    public SubFileProcessedConsumer(CoordinatorService coordinatorService, ILogger<SubFileProcessedConsumer> logger)
    {
        _coordinatorService = coordinatorService;
        _logger = logger;
    }

    public Task Consume(ConsumeContext<SubFileProcessed> context)
    {
        try
        {
            _logger.LogInformation("SubFileProcessed {Id} message processing.", context.Message.Id);
            return _coordinatorService.SetFinishedSubFile(context.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error processing SubFileProcessed {Id} message: {Error}", context.Message.Id, ex.Message);
            throw;
        }
    }
}