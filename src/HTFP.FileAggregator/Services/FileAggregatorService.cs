using System;
using System.IO;
using System.Threading.Tasks;
using HTFP.Shared.Bus.Messages;
using HTFP.Shared.Db;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace HTFP.FileAggregator.Services;

public class FileAggregatorService
{
    private readonly string _dataFolder = Environment.GetEnvironmentVariable("DATA_FOLDER") ?? "/etc/data";
    private readonly ILogger<FileAggregatorService> _logger;
    private readonly MongoDbContext _dbContext;

    public FileAggregatorService(ILogger<FileAggregatorService> logger, MongoDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task AggregateFilesAsync(AggregateReconciliationResult message)
    {
        _logger.LogInformation("Starting file aggregation process for reconciliation {id}.", message.ReconciliationId);

        var reconciliation = _dbContext.Reconciliation.Find(r => r.Id == message.ReconciliationId).FirstOrDefault();

        if (message.Divergents < 1)
        {
            _logger.LogInformation("No divergents found for reconciliation {id}. Skipping file aggregation.", message.ReconciliationId);

            reconciliation.SetAsSuccessfullyFinished();
            await _dbContext.Reconciliation.ReplaceOneAsync(r => r.Id == reconciliation.Id, reconciliation);
            return;
        }

        var outputPath = Path.Combine(_dataFolder, reconciliation.OutputPath);

        MergeFiles(outputPath, Path.Combine(_dataFolder, $"{reconciliation.Id}/subfilesOutput/"));

        reconciliation.SetAsSuccessfullyFinished();

        await _dbContext.Reconciliation.ReplaceOneAsync(r => r.Id == reconciliation.Id, reconciliation);
    }

    private void MergeFiles(string outputPath, string inputDirectory)
    {
        var intputFiles = Directory.GetFiles(inputDirectory, "*.csv");

        using var writer = new StreamWriter(outputPath);

        Parallel.ForEach(intputFiles, file =>
        {
            foreach (var line in File.ReadLines(file))
            {
                lock (writer)
                {
                    writer.WriteLine(line);
                }
            }
        });
    }
}