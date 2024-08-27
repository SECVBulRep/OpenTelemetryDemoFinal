

using WeatherApp.Libs.Models;

namespace WeatherApp.FrontEnd.Api.Services;

public interface IWeatherApiService
{
    Task<WeatherData?> GetWeatherByCityAsync(string city);
    Task<List<WeatherData>?> GetAllCitiesAsync();
}