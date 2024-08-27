namespace WeatherApp.Libs.Models;

public record GetWeatherCastForAllCitiesResponse
{
    public List<WeatherData>? Data { get; set; }
}