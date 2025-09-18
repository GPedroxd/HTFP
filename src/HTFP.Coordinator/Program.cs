using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using HTFP.Shared.Bus;
using Serilog.Core;
using Serilog;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using MassTransit.Logging;
using StackExchange.Redis;
using HTFP.Services;

namespace HTFP.Coordinator;

public class Program
{
    private const string otelUrl = "http://aspire-dashboard:18889";
    private static Dictionary<string, object> OTAtt = new Dictionary<string, object>
                                        {
                                            {"service.name", "HTFP.Coordinator"},
                                            {"service.version", "1.0.0"},
                                            {"service.instance.id", Environment.MachineName}
                                        };
    private static Logger logger = new LoggerConfiguration()
                                .MinimumLevel.Information()
                                .WriteTo.Console()
                                .WriteTo.OpenTelemetry(opts =>
                                {
                                    opts.ResourceAttributes = OTAtt;
                                    opts.Endpoint = otelUrl;
                                }, ignoreEnvironment: true)
                                .CreateLogger();

    public static async Task Main(string[] args)
        => await CreateHostBuilder(args).Build().RunAsync();

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                Log.Logger = logger;
                logging.AddSerilog(logger);
            })
            .ConfigureAppConfiguration((context, config) =>
            {
                var env = context.HostingEnvironment;
                config.SetBasePath(env.ContentRootPath);
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddEnvironmentVariables();
            })
            .ConfigureServices((hostContext, services) =>
            {
                var enviroment = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
                var rabbitConfig = hostContext.Configuration.GetSection(nameof(RabbitMQConfig)).Get<RabbitMQConfig>();

                services.AddSingleton<IConnectionMultiplexer>(opts =>
                {
                    var redisConnectionString = enviroment.GetConnectionString("RedisConnection");
                    return ConnectionMultiplexer.Connect(redisConnectionString);
                });

                services.AddTransient<CoordinatorService>();

                services.AddOpenTelemetry()
                    .ConfigureResource(builder =>
                    {
                        builder.AddService("HTFP.Coordinator")
                            .AddAttributes(OTAtt);
                    })
                    .WithTracing(tracing =>
                    {
                        tracing.AddAspNetCoreInstrumentation();
                        // tracing.AddSource(SubFileProcessorDiagnosticsConfig.ServiceName);
                        tracing.AddSource(DiagnosticHeaders.DefaultListenerName);
                        tracing.AddOtlpExporter(options =>
                        {
                            options.Endpoint = new Uri(otelUrl);
                        });
                    });

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
