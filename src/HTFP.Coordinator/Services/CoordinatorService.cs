using System.Threading.Tasks;
using HTFP.Shared.Bus.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace HTFP.Services;

public class CoordinatorService
{
    private readonly ILogger<CoordinatorService> _logger;
    private readonly IDatabase _cache;
    private readonly IBus _bus;


    public CoordinatorService(ILogger<CoordinatorService> logger, IConnectionMultiplexer connectionMultiplexer, IBus bus)
    {
        _logger = logger;
        _cache = connectionMultiplexer.GetDatabase();
        _bus = bus;
    }

    public async Task StartAsync(FileSplit splitFile)
    {
        var cacheKey = $"file:{splitFile.ReconciliationId}";

        var hashEntries = new HashEntry[]
        {
            new HashEntry("ReconciliationId", splitFile.ReconciliationId.ToString()),
            new HashEntry("Expected", splitFile.TotalLines),
            new HashEntry("Processed", 0),
            new HashEntry("FilesProcessed", 0)
        };

        await _cache.HashSetAsync(cacheKey, hashEntries);

        _logger.LogInformation("Initialized processing for file {ReconciliationId} with {TotalLines} expected lines.", splitFile.ReconciliationId, splitFile.TotalLines);
    }

    public async Task SetFinishedSubFile(SubFileProcessed subFileSplited)
    {
        var cacheKey = $"file:{subFileSplited.ReconciliationId}";
        await _cache.HashIncrementAsync(cacheKey, "Processed", subFileSplited.TotalProcessed);
        await _cache.HashIncrementAsync(cacheKey, "FilesProcessed", 1);

        await _cache.HashGetAllAsync(cacheKey);

        var expected = (int)await _cache.HashGetAsync(cacheKey, "Expected");
        var processed = (int)await _cache.HashGetAsync(cacheKey, "Processed");

        _logger.LogInformation("Subfile {Id} processed for {ReconciliationId}. Total processed lines: {TotalProcessed}/{Expected}.", subFileSplited.Id, subFileSplited.ReconciliationId, processed, expected);

        if (expected != processed)
            return;

        _logger.LogInformation("All subfiles for {ReconciliationId} have been processed. Total lines processed: {TotalProcessed}.", subFileSplited.ReconciliationId, processed);

        await _bus.Publish(new SubFilesProcessed
        {
            ReconciliationId = subFileSplited.ReconciliationId
        });
    }
}