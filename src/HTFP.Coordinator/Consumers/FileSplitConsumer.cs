using System.Threading.Tasks;
using MassTransit;
using HTFP.Shared.Bus.Messages;

namespace HTFP.Coordinator.Consumers;

public record FileSplitConsumer : IConsumer<FileSplit>
{
    public Task Consume(ConsumeContext<FileSplit> context)
    {
        throw new System.NotImplementedException();
    }
}