using HTFP.Shared.Models;
using MongoDB.Driver;

namespace HTFP.Shared.Db;

public class MongoDbContext : IDisposable
{
    private readonly IMongoClient _client;
    private readonly IMongoDatabase _database;
    private IClientSessionHandle? _session;

    public MongoDbContext(IMongoClient client, string dbName)
    {
        _client = client;
        _database = _client.GetDatabase(dbName);
    }

    public IMongoCollection<ReconciliationFile> Users => _database.GetCollection<ReconciliationFile>("reconciliation_file");
    public IMongoCollection<SubFile> Orders => _database.GetCollection<SubFile>("sub_file");

    public async Task StartTransactionAsync()
    {
        _session = await _client.StartSessionAsync();
        _session.StartTransaction();
    }

    public async Task CommitAsync()
    {
        if (_session == null)
            throw new InvalidOperationException("No transaction started.");

        await _session.CommitTransactionAsync();
        _session.Dispose();
        _session = null;
    }

    public async Task AbortAsync()
    {
        if (_session == null)
            throw new InvalidOperationException("No transaction started.");

        await _session.AbortTransactionAsync();
        _session.Dispose();
        _session = null;
    }

    public IMongoCollection<T> GetCollection<T>(string name)
    {
        return _database.GetCollection<T>(name);
    }

    public IClientSessionHandle? Session => _session;

    public void Dispose()
    {
        _session?.Dispose();
    }
}
