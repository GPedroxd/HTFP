using System;
using System.Threading.Tasks;
using HTFP.Shared.Db;
using Microsoft.Extensions.Logging;

namespace HTFP.FileAggregator.Services;

public class FileAggregatorService
{
    private readonly ILogger<FileAggregatorService> _logger;
    private readonly MongoDbContext _dbContext;

    public FileAggregatorService(ILogger<FileAggregatorService> logger, MongoDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public Task AggregateFilesAsync()
    {
        throw new NotImplementedException();
    }
}