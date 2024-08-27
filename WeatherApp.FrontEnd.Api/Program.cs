using MassTransit;
using Serilog;
using WeatherApp.FrontEnd.Api.Services;
using WeatherApp.Libs.Metrics;

namespace WeatherApp.FrontEnd.Api;

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


        builder.Services.AddHttpClient("WeatherApiClient",
            client => { client.BaseAddress = new Uri("http://localhost:5254/WeatherForecast/"); });

        builder.Services.AddScoped<IWeatherApiService, WeatherApiService>();
        builder.Services.AddSingleton<WeatherMetrics>();

        builder.Services.AddMassTransit(x =>
        {
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