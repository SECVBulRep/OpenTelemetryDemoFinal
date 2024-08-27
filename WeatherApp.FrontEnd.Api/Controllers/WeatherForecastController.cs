using System.Diagnostics;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using WeatherApp.FrontEnd.Api.Services;
using WeatherApp.Libs.Metrics;
using WeatherApp.Libs.Models;

namespace WeatherApp.FrontEnd.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController(
    ILogger<WeatherForecastController> logger,
    IWeatherApiService weatherApiService,
    IRequestClient<GetWeatherCastForAllCitiesRequest> client,
    WeatherMetrics metrics)
    : ControllerBase
{
    private readonly ILogger<WeatherForecastController> _logger = logger;

    private static readonly ActivitySource _activitySource =
        new ActivitySource(nameof(WeatherForecastController), "1.0.0");

    [HttpGet("GetAllFromBackEndApi")]
    public async Task<List<WeatherData>?> GetAll()
    {
        metrics.SummaryRequestByCityCounter.Add(1, new KeyValuePair<string, object?>("city", "all"));
        var result = await weatherApiService.GetAllCitiesAsync();
        return result;
    }

    [HttpGet("GetAllFromBackEndApiEventually")]
    public async Task<List<WeatherData>?> GetAllEventually(string? orderId)
    {
        metrics.SummaryRequestByCityCounter.Add(1, new KeyValuePair<string, object?>("city", "all"));
        
        if (!string.IsNullOrEmpty(orderId))
            Activity.Current.SetTag("orderId", orderId);
        
        var response =
            await client.GetResponse<GetWeatherCastForAllCitiesResponse>(new GetWeatherCastForAllCitiesRequest());
        return response.Message.Data;
    }

    [HttpGet("GetByCityFromBackEndApi")]
    public async Task<WeatherData?> GetByCity(string city)
    {
        try
        {
            //using var activity = _activitySource.StartActivity(nameof(GetAll));
            WeatherData? result = await weatherApiService.GetWeatherByCityAsync(city);
            metrics.SummaryRequestByCityCounter.Add(1, new KeyValuePair<string, object?>("city", result?.City));
            metrics.SetWeather(result!);
            return result;
        }
        catch (Exception)
        {
            return null;
        }
    }
}