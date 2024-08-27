using MassTransit;
using WeatherApp.Libs.Models;
using WeatherApp.Libs.Services;

namespace WeatherApp.BackEnd.Api.Consumers;

public class GetWeatherCastForAllCitiesRequestConsumer(WeatherService weatherService) :
    IConsumer<GetWeatherCastForAllCitiesRequest>
{
    public async Task Consume(ConsumeContext<GetWeatherCastForAllCitiesRequest> context)
    {
        List<WeatherData>? allcities = await weatherService.GetWeatherInAllCitiesAsync();
        var result = new GetWeatherCastForAllCitiesResponse
        {
            Data = allcities
        };
        await context.RespondAsync<GetWeatherCastForAllCitiesResponse>(result);
    }
}