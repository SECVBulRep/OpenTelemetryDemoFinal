using WeatherApp.Libs.Models;

namespace WeatherApp.FrontEnd.Api.Services;

public class WeatherApiService(IHttpClientFactory httpClientFactory) : IWeatherApiService
{
    public async Task<WeatherData?> GetWeatherByCityAsync(string city)
    {
        var client = httpClientFactory.CreateClient("WeatherApiClient");
        HttpResponseMessage response = await client.GetAsync($"GetByCity?city={city}");

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<WeatherData>();
        }
        else
        {
            Console.WriteLine($"Error: {response.StatusCode}");
            return null;
        }
    }
    
    public async Task<List<WeatherData>?> GetAllCitiesAsync()
    {
        var client = httpClientFactory.CreateClient("WeatherApiClient");
        HttpResponseMessage response = await client.GetAsync("GetAll");

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<List<WeatherData>>();
        }
        else
        {
            return null;
        }
    }
}