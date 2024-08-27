using Microsoft.AspNetCore.Mvc;
using WeatherApp.Libs.Models;
using WeatherApp.Libs.Services;

namespace WeatherApp.BackEnd.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController(
    ILogger<WeatherForecastController> logger,
    WeatherService weatherService)
    : ControllerBase
{
    private readonly ILogger<WeatherForecastController> _logger = logger;

    [HttpGet("GetAll")]
    public async Task<List<WeatherData>?> GetAll()
    {
        var result = await weatherService.GetWeatherInAllCitiesAsync();
        return result;
    }

    [HttpGet("GetByCity")]
    public async Task<WeatherData?> GetByCity(string city)
    {
        var result = await weatherService.GetWeatherByCityAsync(city);
        return result;
    }
}