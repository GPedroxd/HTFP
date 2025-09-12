using System.Threading.Tasks;
using HTFP.Shared.Bus.Messages;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace HTFP.Services;

public class CoordinatorService
{
    private readonly ILogger<CoordinatorService> _logger;
    private readonly  IDatabase _cache;

    public CoordinatorService(ILogger<CoordinatorService> logger, IConnectionMultiplexer connectionMultiplexer)
    {
        _logger = logger;
        _cache = connectionMultiplexer.GetDatabase();
    }

    public async Task StartAsync(FileSplit splitFile)
    {
        var cacheKey = $"file:{splitFile.ReconciliationId}";
        
        var hashEntries = new HashEntry[]
        {
            new HashEntry("ReconciliationId", splitFile.ReconciliationId.ToString()),
            new HashEntry("Status", "Started"),
            new HashEntry("Expected", splitFile.TotalLines),
            new HashEntry("Processed", 0),
            new HashEntry("FilesProcessed", 0)
        };

        await _cache.HashSetAsync(cacheKey, hashEntries);
        _logger.LogInformation("Initialized processing for file {ReconciliationId} with {TotalLines} expected lines.", splitFile.ReconciliationId, splitFile.TotalLines);
    }
}