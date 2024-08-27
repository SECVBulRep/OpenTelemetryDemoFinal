using MassTransit;
using MassTransit.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.OpenTelemetry;
using StackExchange.Redis;
using WeatherApp.BackEnd.Api.Consumers;
using WeatherApp.Libs.Services;

namespace WeatherApp.BackEnd.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.OpenTelemetry(options =>
            {
                options.Endpoint = "http://localhost:4317/v1/logs";
                options.Protocol = OtlpProtocol.Grpc;
                options.IncludedData = IncludedData.TraceIdField | IncludedData.SpanIdField;
                options.ResourceAttributes = new Dictionary<string, object>
                {
                    {"service.name", AppDomain.CurrentDomain.FriendlyName}
                };

            })
            .CreateLogger();

        builder.Services.AddSerilog();
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();


        builder.Services.AddSingleton<WeatherService>();
        builder.Services.AddHostedService<MyHostedService>();


        builder.Services.AddMassTransit(x =>
        {
            x.AddConsumers(typeof(GetWeatherCastForAllCitiesRequestConsumer).Assembly);
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("localhost", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        var redisConnection = ConnectionMultiplexer.Connect("localhost:6379");
        builder.Services.AddSingleton<IConnectionMultiplexer>(redisConnection);

        var clientSettings = MongoClientSettings.FromConnectionString("mongodb://admin:adminpassword@localhost:27017");
        var options = new InstrumentationOptions { CaptureCommandText = true };
        clientSettings.ClusterConfigurator = cb => cb.Subscribe(new DiagnosticsActivityEventSubscriber(options));
        MongoClient mongoClient = new MongoClient(clientSettings);
        builder.Services.AddSingleton<MongoClient>(mongoClient);

        builder.Services.AddOpenTelemetry()
            .WithTracing(tracerProviderBuilder =>
            {
                tracerProviderBuilder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("BackEnd.Api")) 
                    .AddSource("BackEnd.Api")
                    .SetSampler(new AlwaysOnSampler())
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddSource("MongoDB.Driver.Core.Extensions.DiagnosticSources")
                    .AddSource(DiagnosticHeaders.DefaultListenerName)
                    .AddRedisInstrumentation(redisConnection, opt =>
                    {
                        opt.Enrich = (activity, command) => activity.SetTag("redis.connection", "localhost:6379");
                        opt.Enrich = (activity, command) => activity.SetTag("peer.service", "redis");
                        opt.FlushInterval = TimeSpan.FromSeconds(1);
                        opt.EnrichActivityWithTimingEvents = true;
                    });
        
                tracerProviderBuilder.AddOtlpExporter(otlpOptions =>
                {
                    otlpOptions.Endpoint = new Uri("http://localhost:4317");
                });
            });
        
        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}