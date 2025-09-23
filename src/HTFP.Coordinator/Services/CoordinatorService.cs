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
            new HashEntry("TotalFiles", splitFile.TotalSubFiles),
            new HashEntry("FilesProcessed", 0),
            new HashEntry("Divergents", 0),
            new HashEntry("Error", false)
        };

        await _cache.HashSetAsync(cacheKey, hashEntries);

        _logger.LogInformation("Initialized processing for file {ReconciliationId} with {TotalLines} expected lines.", splitFile.ReconciliationId, splitFile.TotalLines);
    }

    public async Task SetFinishedSubFile(SubFileProcessed subFileSplited)
    {
        var cacheKey = $"file:{subFileSplited.ReconciliationId}";
        await _cache.HashIncrementAsync(cacheKey, "FilesProcessed", 1);
        await _cache.HashIncrementAsync(cacheKey, "Divergents", subFileSplited.TotalDivergents);

        await _cache.HashGetAllAsync(cacheKey);

        var filesExpected = (int)await _cache.HashGetAsync(cacheKey, "TotalFiles");
        var fileProcessed = (int)await _cache.HashGetAsync(cacheKey, "FilesProcessed");
        var error = (bool)await _cache.HashGetAsync(cacheKey, "Error");
        
        if(subFileSplited.SuccessfullyProcessed is false && error is false)
            await _cache.HashSetAsync(cacheKey, "Error", true);
        
        _logger.LogInformation("Subfile {Id} processed for {ReconciliationId}. Total files processed: {TotalProcessed}/{Expected}.", subFileSplited.Id, subFileSplited.ReconciliationId, fileProcessed, filesExpected);

        if (filesExpected != fileProcessed)
            return;

        _logger.LogInformation("All subfiles for {ReconciliationId} have been processed. Total files processed: {TotalProcessed}.", subFileSplited.ReconciliationId, fileProcessed);

        var divergents = (int)await _cache.HashGetAsync(cacheKey, "Divergents");

        _logger.LogInformation("Total divergent lines for {ReconciliationId}: {Divergents}.", subFileSplited.ReconciliationId, divergents);

        await _bus.Publish(new SubFilesProcessed
        {
            ReconciliationId = subFileSplited.ReconciliationId,
            Divergents = divergents
        });
    }
}