using MassTransit;
using MongoDB.Driver;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
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
                    .AddAspNetCoreInstrumentation();
        
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