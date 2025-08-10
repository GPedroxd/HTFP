using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using HTFP.Shared.Bus;

namespace HTFP.FileSpliter
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    var enviroment = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
                    var rabbitConfig = enviroment.GetSection(nameof(RabbitMQConfig)).Get<RabbitMQConfig>();

                    services.AddMassTransit(x =>
                    {
                        x.SetKebabCaseEndpointNameFormatter();

                        // By default, sagas are in-memory, but should be changed to a durable
                        // saga repository.
                        x.SetInMemorySagaRepositoryProvider();

                        var entryAssembly = Assembly.GetEntryAssembly();

                        x.AddConsumers(entryAssembly);
                        x.AddSagaStateMachines(entryAssembly);
                        x.AddSagas(entryAssembly);
                        x.AddActivities(entryAssembly);

                        x.UsingRabbitMq((hostContext, cfg) =>
                        {
                            cfg.Host(rabbitConfig.Host, "/", h =>
                            {
                                h.Username(rabbitConfig.Username);
                                h.Password(rabbitConfig.Password);
                            });

                            cfg.ConfigureEndpoints(hostContext);
                        });
                    });
                });
    }
}
