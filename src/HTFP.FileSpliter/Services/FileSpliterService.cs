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

        var mainFile = new ReconciliationFile { Name = fileToProcess.Name };

        await _mongoDbContext.StartTransactionAsync();

        await foreach (var splitedfile in _fileSpliter.SplitAsync($"Samples/{fileToProcess.Name}", 10000))
        {
            mainFile.IncrementSplitFileCount();

            var filepath = await ProcessSplitFile(mainFile, splitedfile, mainFile.TotalSubFiles);

            insertSubFileTasks.Add(_mongoDbContext.SubFile.InsertOneAsync(new SubFile
            {
                Name = filepath,
                MainFileId = mainFile.Id
            }));

            toPublish.Add(new ProcessSubFile
            {
                ParentFileId = mainFile.Id,
                FilePath = filepath
            });
        }

        await _bus.PublishBatch(toPublish);
        await Task.WhenAll(insertSubFileTasks);
        await _mongoDbContext.Reconciliation.InsertOneAsync(_mongoDbContext.Session, mainFile);

        await _mongoDbContext.CommitAsync();
        
        _logger.LogInformation("File {FilePath} processed successfully", fileToProcess.Name);
        _logger.LogInformation("Total subfiles created: {Count}", mainFile.TotalSubFiles);
    }

    private async Task<string> ProcessSplitFile(ReconciliationFile mainFile, Stream subfile, int position)
    {
        var filePath = Path.Combine(
            "Output",
            mainFile.Id.ToString(),
            $"{mainFile.Id}.part-{position}.csv"
        );

        var dir = Path.GetDirectoryName(filePath);

        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        var subfileName = $"{mainFile.Id}.part-{position}.csv";

        using var fileStream = new FileStream($"Output/{mainFile.Id}/{subfileName}", FileMode.Create, FileAccess.Write);

        await subfile.CopyToAsync(fileStream);

        return subfileName;
    }
}