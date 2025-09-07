using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HTFP.Shared.Bus.Messages;
using HTFP.Shared.Db;
using HTFP.Shared.Models;
using HTFP.Shared.Storage;
using MassTransit;
using Microsoft.Extensions.Logging;

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

    public async Task SplitAsync(SplitFile fileToProcess)
    {
        var toPublish = new List<ProcessSubFile>();
        var insertSubFileTasks = new List<Task>();

        var mainFile = new ReconciliationFile($"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}.{Guid.NewGuid()}", fileToProcess.Path);

        await _mongoDbContext.StartTransactionAsync();

        await foreach (var (splitedfile, lineCount) in _fileSpliter.SplitAsync(Path.Combine(_dataFolder,fileToProcess.Path), 10000))
        {
            mainFile.IncrementSplitFileCount();
            mainFile.IncrementLineCount(lineCount);

            var fileName = $"{mainFile.Id}.part-{mainFile.TotalSubFiles}.csv";
            var filePath = Path.Combine(
                        _dataFolder,
                        mainFile.SubfilePath,
                        fileName
                    );

            var subFile = new SubFile
            {
                Name = fileName,
                Path = filePath,
                ReconciliationFileId = mainFile.Id,
                TotalLines = lineCount
            };  

            await SaveSplitFile(subFile.Path, splitedfile);

            insertSubFileTasks.Add(_mongoDbContext.SubFile.InsertOneAsync(subFile));

            toPublish.Add(new ProcessSubFile
            {
                ParentFileId = mainFile.Id,
                FilePath = subFile.Path,
                Id = subFile.Id
            });
        }

        await _bus.PublishBatch(toPublish);
        await Task.WhenAll(insertSubFileTasks);
        await _mongoDbContext.Reconciliation.InsertOneAsync(_mongoDbContext.Session, mainFile);

        await _mongoDbContext.CommitAsync();
        
        _logger.LogInformation("File {FilePath} processed successfully", fileToProcess.Path);
        _logger.LogInformation("Total subfiles created: {Count}", mainFile.TotalSubFiles);
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