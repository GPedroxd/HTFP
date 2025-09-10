using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using HTFP.Shared.Bus;
using HTFP.Shared.Storage;
using HTFP.FileSpliter.Services;
using Serilog;
using System.Collections.Generic;
using Serilog.Core;
using System;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using HTFP.Shared.Db;
using MassTransit.Monitoring;
using HTFP.Shared.Models;
using MassTransit.Logging;

namespace HTFP.FileSpliter
{
    public class Program
    {
        private const string otelUrl = "http://aspire-dashboard:18889";
        private static Dictionary<string, object> OTAtt = new Dictionary<string, object>
                                        {
                                            {"service.name", "HTFP.FileSpliter"},
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

                    services.AddOpenTelemetry()
                        .ConfigureResource(builder =>
                        {
                            builder.AddService("HTFP.FileSpliter")
                                .AddAttributes(OTAtt);
                        })
                        .WithMetrics(metrics =>
                        {
                            metrics.AddAspNetCoreInstrumentation();
                            metrics.AddMeter(InstrumentationOptions.MeterName);
                            metrics.AddMeter(FileSpliterDiagnosticsConfig.ServiceName)
                                .AddView(instrumentName: "htfp.filespliter.file.size",
                                new ExplicitBucketHistogramConfiguration
                                {
                                    Boundaries = new double[]
                                    {
                                        1 * 1024 * 1024,        // 1 MB
                                        10 * 1024 * 1024,       // 10 MB
                                        50 * 1024 * 1024,       // 50 MB
                                        100 * 1024 * 1024,      // 100 MB
                                        200 * 1024 * 1024,      // 200 MB
                                        500 * 1024 * 1024,      // 500 MB
                                        1024 * 1024 * 1024      // 1 GB
                                    }
                                })
                                .AddView(instrumentName: "htfp.filespliter.file.splittime",
                                new ExplicitBucketHistogramConfiguration
                                {
                                    Boundaries = new double[]
                                    {
                                        1,     // 1s
                                        5,     // 5s
                                        10,    // 10s
                                        30,    // 30s
                                        60,    // 1m
                                        120,   // 2m
                                        300,   // 5m
                                        600    // 10m
                                    }
                                });

                            metrics.AddOtlpExporter(options =>
                            {
                                options.Endpoint = new Uri(otelUrl);
                            });
                            #if DEBUG
                                metrics.AddConsoleExporter();
                            #endif
                        })
                        .WithTracing(tracing =>
                        {
                            tracing.AddAspNetCoreInstrumentation();
                            
                            tracing.AddSource(FileSpliterDiagnosticsConfig.ServiceName);
                            tracing.AddSource(DiagnosticHeaders.DefaultListenerName);
                            tracing.AddOtlpExporter(options =>
                            {
                                options.Endpoint = new Uri(otelUrl);
                            });
                        });

                    services.AddScoped<FileSpliterService>();
                    services.AddScoped<IFileSpliter, LocalStorageFileSpliter>();
                    services.AddMongoDbContext<MongoDbContext>();

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
