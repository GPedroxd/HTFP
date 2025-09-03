using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HTFP.Shared.Db;
using HTFP.Shared.Models;
using HTFP.Shared.Storage;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace HTFP.SubFileProcessor.Services;

public sealed class SubfileService
{
    private readonly ILogger<SubfileService> _logger;
    private readonly IOrderExtractor _orderExtractor;
    private readonly MongoDbContext _dbContext;

    public SubfileService(ILogger<SubfileService> logger, IOrderExtractor orderExtractor, MongoDbContext dbContext)
    {
        _logger = logger;
        _orderExtractor = orderExtractor;
        _dbContext = dbContext;
    }

    public async Task ProcessSubfileAsync(string filePath)
    {
        _logger.LogInformation("Processing sub-file: {FileName}", filePath);

        var ordersExecuted = new List<ExecutionOrder>();

        await foreach (var order in _orderExtractor.ExtractOrdersAsync(filePath))
            ordersExecuted.Add(order);

        var minDateFilter = ordersExecuted.Min(o => o.DateTime);
        var maxDateFilter = ordersExecuted.Max(o => o.DateTime);

        var existingOrders = await (await _dbContext.ExecutionOrder
            .FindAsync(o => o.DateTime >= minDateFilter && o.DateTime <= maxDateFilter))
            .ToListAsync();

        //compare each order(can be done in parallel)

        //save file with divergents
    }
}
