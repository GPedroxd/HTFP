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
       var mainFile = new MainFile { Name = fileToProcess.Name };

        await foreach (var splitedfile in _fileSpliter.SplitAsync($"Samples/{fileToProcess.Name}", 100))
        {
            await ProcessSplitFile(mainFile, splitedfile, mainFile.TotalSubFiles);

            mainFile.IncrementSplitFileCount();
        }

        _logger.LogInformation("File {FilePath} processed successfully", fileToProcess.Name);
        _logger.LogInformation("Total subfiles created: {Count}", mainFile.TotalSubFiles);

        //save main file
    }

    private async Task ProcessSplitFile(MainFile mainFile, Stream subfile, int position)
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

        //publish message
    }
}