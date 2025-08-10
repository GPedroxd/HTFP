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

    public async Task ProcessFileAsync(ProcessFile fileToProcess)
    {
        _logger.LogInformation("Processing file {FilePath}", fileToProcess.Path);

        var mainFile = new MainFile { FilePath = fileToProcess.Path };

        await foreach (var splitedfile in _fileSpliter.SplitAsync(fileToProcess.Path))
        {
            await ProcessSplitFile(splitedfile, mainFile);

            mainFile.IncrementSplitFileCount();
        }

        //save main file
    }

    private async Task ProcessSplitFile(Stream subfile, MainFile mainFile)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
    }
}