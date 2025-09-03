using System;
using System.Collections.Generic;
using System.IO;
using DnsClient.Internal;
using HTFP.Shared.Models;
using HTFP.Shared.Storage;
using Microsoft.Extensions.Logging;

namespace HTFP.SubFileProcessor.OrderExtractor;

public sealed class LocalStorageOrderExtractor : IOrderExtractor
{
    private readonly ILogger<LocalStorageOrderExtractor> _logger;

    public LocalStorageOrderExtractor(ILogger<LocalStorageOrderExtractor> logger)
    {
        _logger = logger;
    }

    public async IAsyncEnumerable<ExecutionOrder> ExtractOrdersAsync(string filePath)
    {
        _logger.LogInformation("Extracting orders from file: {FilePath}", filePath);

        var fileInfo = new FileInfo(filePath);
        using var reader = fileInfo.OpenRead();
        using var streamReader = new StreamReader(reader);

        string line;

        while ((line = await streamReader.ReadLineAsync()) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            yield return ParseLineToOrder(line);
        }
        
        _logger.LogInformation("Finished extracting orders from file: {FilePath}", filePath);
    }

    private ExecutionOrder ParseLineToOrder(string line)
    {
        var parts = line.Split(',');

        var executionOrder = new ExecutionOrder();

        executionOrder.Id = parts[0];
        executionOrder.ExternalId = parts[1];
        executionOrder.DateTime = DateTime.Parse(parts[2]);
        executionOrder.AssetId = parts[3];
        executionOrder.TradingAccount = parts[4];
        executionOrder.Quantity = int.Parse(parts[5]);
        executionOrder.UnitPrice = decimal.Parse(parts[6]);

        return executionOrder;
    }
}