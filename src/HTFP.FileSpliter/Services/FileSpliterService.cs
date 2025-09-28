using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using HTFP.Shared.Bus.Messages;
using HTFP.Shared.Db;
using HTFP.Shared.Models;
using HTFP.Shared.Storage;
using MassTransit;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace HTFP.FileSpliter.Services;

public sealed class FileSpliterService
{
    private readonly string _dataFolder = Environment.GetEnvironmentVariable("DATA_FOLDER") ?? "/etc/data";
    private readonly ILogger<FileSpliterService> _logger;
    private readonly MongoDbContext _mongoDbContext;
    private readonly IFileSpliter _fileSpliter;
    private readonly IBus _bus;
    public FileSpliterService(ILogger<FileSpliterService> logger, IFileSpliter fileSpliter, IBus bus, MongoDbContext mongoDbContext)
    {
        _logger = logger;
        _fileSpliter = fileSpliter;
        _bus = bus;
        _mongoDbContext = mongoDbContext;
    }

    public async Task SplitAsync(StartReconciliationProcess fileToProcess)
    {
        var reconcilitonFile = new ReconciliationFile($"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}.{Guid.NewGuid()}", fileToProcess.Path);

        var currentActivity = Activity.Current;
        currentActivity?.SetTag("file.id", reconcilitonFile.Id);
        currentActivity?.SetBaggage("file.id", reconcilitonFile.Id.ToString());

        await _mongoDbContext.Reconciliation.InsertOneAsync(reconcilitonFile);

        await _mongoDbContext.StartTransactionAsync();

        try
        {
            var (toPublish, insertSubFileTasks) = await ExtractSubfilesAsync(reconcilitonFile, fileToProcess);
            await Task.WhenAll(insertSubFileTasks);
            await _bus.PublishBatch(toPublish);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing file {FilePath} with id {id}", fileToProcess.Path, reconcilitonFile.Id);
            reconcilitonFile.SetAsError(ex.Message);
            await _mongoDbContext.Reconciliation.ReplaceOneAsync(r => r.Id == reconcilitonFile.Id, reconcilitonFile);
            throw;
        }

        reconcilitonFile.SetAsProcessing();

        await _mongoDbContext.Reconciliation.ReplaceOneAsync(r => r.Id == reconcilitonFile.Id, reconcilitonFile);
        
        await _bus.Publish(new SplitFile
        {
            ReconciliationId = reconcilitonFile.Id,
            TotalSubFiles = reconcilitonFile.TotalSubFiles,
            TotalLines = reconcilitonFile.TotalLines
        });

        await _mongoDbContext.CommitAsync();

        _logger.LogInformation("File {FilePath} processed successfully", fileToProcess.Path);
        _logger.LogInformation("Total subfiles created: {Count}", reconcilitonFile.TotalSubFiles);
    }

    private async Task<(List<ProcessSubFile> toPublish, List<Task> insertSubFileTasks)> ExtractSubfilesAsync(ReconciliationFile mainFile, StartReconciliationProcess fileToProcess)
    {
        var toPublish = new List<ProcessSubFile>();
        var insertSubFileTasks = new List<Task>();

        await foreach (var (splitedfile, lineCount) in _fileSpliter.SplitAsync(Path.Combine(_dataFolder, fileToProcess.Path), 10000))
        {
            mainFile.IncrementSplitFileCount();
            mainFile.IncrementLineCount(lineCount);

            var fileName = $"{mainFile.Id}.part-{mainFile.TotalSubFiles}.csv";
            var filePath = Path.Combine(
                        _dataFolder,
                        mainFile.SubfilePath,
                        fileName
                    );

            var subFile = new SubFile(
                fileName,
                filePath,
                mainFile.Id,
                lineCount
            );

            await SaveSplitFile(subFile.Path, splitedfile);

            insertSubFileTasks.Add(_mongoDbContext.SubFile.InsertOneAsync(_mongoDbContext.Session, subFile));

            toPublish.Add(new ProcessSubFile
            {
                ReconciliationId = mainFile.Id,
                FilePath = subFile.Path,
                Id = subFile.Id
            });
        }

        return (toPublish, insertSubFileTasks);
    }

    private async Task SaveSplitFile(string filePath, Stream subfile)
    {
        var dir = Path.GetDirectoryName(filePath);

        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        using var fileStream = new FileStream($"{filePath}", FileMode.Create, FileAccess.Write);

        await subfile.CopyToAsync(fileStream);
    }
}