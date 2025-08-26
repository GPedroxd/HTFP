using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace HTFP.Shared.Db;

public static class MongodbExtensions
{
    public static IServiceCollection AddMongoDbContext<TContext>(
        this IServiceCollection services)
        where TContext : MongoDbContext
    {

        var environment = services.BuildServiceProvider()
            .GetRequiredService<IConfiguration>();

        var connectionString = environment.GetConnectionString("MongoDbConnection");
        var databaseName = environment.GetValue<string>("DatabaseName");

        services.AddSingleton<IMongoClient>(_ => new MongoClient(connectionString));

        services.AddScoped<TContext>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return (TContext)Activator.CreateInstance(typeof(TContext), client, databaseName)!;
        });

        return services;
    }
}