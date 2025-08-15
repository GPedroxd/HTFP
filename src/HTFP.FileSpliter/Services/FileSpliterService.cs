using System;
using System.IO;
using System.Threading.Tasks;
using HTFP.Shared.Bus.Messages;
using HTFP.Shared.Models;
using HTFP.Shared.Storage;
using Microsoft.Extensions.Logging;

namespace HTFP.FileSpliter.Services;

public sealed class FileSpliterService
{
    private readonly ILogger<FileSpliterService> _logger;
    private readonly IFileSpliter _fileSpliter;

    public FileSpliterService(ILogger<FileSpliterService> logger, IFileSpliter fileSpliter)
    {
        _logger = logger;
        _fileSpliter = fileSpliter;
    }

    public async Task SplitAsync(ProcessFile fileToProcess)
    {
        _logger.LogInformation("Processing file {FilePath}", fileToProcess.Name);

        _logger.LogInformation("Application path: {path}", AppContext.BaseDirectory);

        var mainFile = new MainFile { Name = fileToProcess.Name };

        await foreach (var splitedfile in _fileSpliter.SplitAsync($"Samples/{fileToProcess.Name}"))
        {
            await ProcessSplitFile(mainFile, splitedfile, mainFile.TotalSubFiles);

            mainFile.IncrementSplitFileCount();
        }
        //save main file
    }

    private async Task ProcessSplitFile(MainFile mainFile, Stream subfile, int position)
    {
        await Task.CompletedTask;
        var subfileName = $"{mainFile.Id}.part-{position}.csv";
        //write to new file
        //publish message
        _logger.LogInformation("Processed split file {FilePath} for main file {MainFilePath}", subfileName, mainFile.Name);
    }
}