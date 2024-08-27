using MassTransit;
using Microsoft.AspNetCore.Mvc;
using WeatherApp.FrontEnd.Api.Services;
using WeatherApp.Libs.Models;

namespace WeatherApp.FrontEnd.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController(
    ILogger<WeatherForecastController> logger,
    IWeatherApiService weatherApiService,
    IRequestClient<GetWeatherCastForAllCitiesRequest> client)
    : ControllerBase
{
    private readonly ILogger<WeatherForecastController> _logger = logger;


    [HttpGet("GetAllFromBackEndApi")]
    public async Task<List<WeatherData>?> GetAll()
    {
        var result = await weatherApiService.GetAllCitiesAsync();
        return result;
    }

    [HttpGet("GetAllFromBackEndApiEventually")]
    public async Task<List<WeatherData>?> GetAllEventually(string? orderId)
    {
        var response =
            await client.GetResponse<GetWeatherCastForAllCitiesResponse>(new GetWeatherCastForAllCitiesRequest());
        return response.Message.Data;
    }

    [HttpGet("GetByCityFromBackEndApi")]
    public async Task<WeatherData?> GetByCity(string city)
    {
        try
        {
            WeatherData? result = await weatherApiService.GetWeatherByCityAsync(city);
            return result;
        }
        catch (Exception)
        {
            return null;
        }
    }
}