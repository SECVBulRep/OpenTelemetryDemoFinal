using WeatherApp.Libs.Services;

namespace WeatherApp.BackEnd.Api;

public class MyHostedService(WeatherService weatherService) : IHostedService, IDisposable
{
    public async Task StartAsync(CancellationToken stoppingToken)
    {
        await weatherService.GenerateRandomWeatherDataAsync();
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
    }
}