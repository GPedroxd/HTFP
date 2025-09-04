using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HTFP.Shared.Bus.Messages;
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

    public async Task ProcessSubfileAsync(ProcessSubFile processSubFile)
    {
        _logger.LogInformation("Processing sub-file: {FileName}", processSubFile.FilePath);

        var ordersExecuted = new List<ExecutionOrder>();

        await foreach (var order in _orderExtractor.ExtractOrdersAsync(processSubFile.FilePath))
            ordersExecuted.Add(order);

        var minDateFilter = ordersExecuted.Min(o => o.DateTime);
        var maxDateFilter = ordersExecuted.Max(o => o.DateTime);

        var existingOrders = await (await _dbContext.ExecutionOrder
            .FindAsync(o => o.DateTime >= minDateFilter && o.DateTime <= maxDateFilter))
            .ToListAsync();

        var ordersDivergents = OrderComparer.GetDivergentOrders(ordersExecuted, existingOrders);

        var subFile = await _dbContext.SubFile.Find(f => f.Id == processSubFile.Id).FirstOrDefaultAsync();

        subFile.MarkasAsProcessed(ordersDivergents.Count);

        if (ordersDivergents.Any())
            SaveDivergentOrders(subFile, ordersDivergents);

        await _dbContext.SubFile.ReplaceOneAsync(f => f.Id == subFile.Id, subFile);
    }

    private void SaveDivergentOrders(SubFile subFile, IEnumerable<(ExecutionOrder executedOrder, ExecutionOrder expectedOrder)> divergentOrders)
    {

    }
}
