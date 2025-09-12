using System;
using System.Threading.Tasks;
using HTFP.Shared.Bus.Messages;
using MassTransit;

namespace HTFP.Coordinator.Consumers;

public class SubFileProcessedConsumer : IConsumer<SubFileProcessed>
{
    public Task Consume(ConsumeContext<SubFileProcessed> context)
    {
        throw new NotImplementedException();
    }
}