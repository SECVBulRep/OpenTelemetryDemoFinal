using System.Diagnostics;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using StackExchange.Redis;
using WeatherApp.Libs.Models;

namespace WeatherApp.Libs.Services;

public class WeatherService
{
    private readonly ILogger<WeatherService> _logger;
    private readonly IMongoCollection<WeatherData> _weatherCollection;
    private readonly IDatabase _redisDatabase;

    public WeatherService(
        ILogger<WeatherService> logger,
        IConnectionMultiplexer connectionMultiplexer,
        MongoClient mongoClient)
    {
        _logger = logger;

        var database = mongoClient.GetDatabase("WeatherDb");
        _weatherCollection = database.GetCollection<WeatherData>("WeatherData");
        _redisDatabase = connectionMultiplexer.GetDatabase();
    }

    public async Task GenerateRandomWeatherDataAsync()
    {
        var cities = new[]
        {
            "Москва", "Санкт-Петербург", "Казань", "Новосибирск", "Екатеринбург", "Нижний Новгород", "Самара", "Омск",
            "Челябинск", "Ростов-на-Дону", "Уфа", "Волгоград", "Пермь", "Красноярск", "Воронеж", "Саратов", "Тольятти",
            "Краснодар", "Ижевск", "Ульяновск", "Барнаул", "Тюмень", "Иркутск", "Владивосток", "Ярославль", "Махачкала",
            "Хабаровск", "Оренбург", "Новокузнецк", "Кемерово", "Рязань", "Томск", "Астрахань", "Пенза", "Липецк",
            "Тула", "Курск", "Калининград", "Улан-Удэ", "Севастополь", "Ставрополь", "Магнитогорск", "Сочи", "Тверь",
            "Брянск", "Белгород", "Нижний Тагил", "Архангельск", "Вологда"
        };
        var random = new Random();

        await _weatherCollection.DeleteManyAsync(Builders<WeatherData>.Filter.Empty);

        var weatherDataList = new List<WeatherData>();

        foreach (var city in cities)
        {
            var weatherData = new WeatherData
            {
                Id = ObjectId.GenerateNewId().ToString(),
                City = city,
                Temperature = random.Next(-20, 40),
                Humidity = random.Next(0, 100),
                Condition = GenerateRandomCondition(random),
                Date = DateTime.Now
            };

            weatherDataList.Add(weatherData);
        }

        await _weatherCollection.InsertManyAsync(weatherDataList);
    }

    private string GenerateRandomCondition(Random random)
    {
        var conditions = new[] { "Sunny", "Rainy", "Cloudy", "Snowy", "Windy" };
        return conditions[random.Next(conditions.Length)];
    }

    public async Task<WeatherData?> GetWeatherByCityAsync(string city)
    {
        using (var activity = new ActivitySource("BackEnd.Api").StartActivity())
        {
            var cachedWeatherData = await _redisDatabase.StringGetAsync(city);
            WeatherData? result;

            if (cachedWeatherData.HasValue)
            {
                result = BsonSerializer.Deserialize<WeatherData>(cachedWeatherData.ToString());
            }
            else
            {
                var filter = Builders<WeatherData>.Filter.Eq(w => w.City, city);
                result = await _weatherCollection.Find(filter).FirstOrDefaultAsync();

                if (result != null)
                {
                    await _redisDatabase.StringSetAsync(city, result.ToJson(), TimeSpan.FromMinutes(30));
                }
            }

            _logger.LogInformation("weather in city {@City} is {@Result}", city, result);
            return result;
        }
    }


    public async Task<List<WeatherData>?> GetWeatherInAllCitiesAsync()
    {
        using (var activity = new ActivitySource("BackEnd.Api").StartActivity())
        {
            var cachedWeatherData = await _redisDatabase.StringGetAsync("all_cities");
            List<WeatherData>? result;

            if (cachedWeatherData.HasValue)
            {
                result = BsonSerializer.Deserialize<List<WeatherData>>(cachedWeatherData.ToString());
            }
            else
            {
                result = await _weatherCollection.Find(Builders<WeatherData>.Filter.Empty).ToListAsync();

                if (result is { Count: > 0 })
                {
                    await _redisDatabase.StringSetAsync("all_cities", result.ToJson(),
                        TimeSpan.FromMinutes(30));
                }
            }

            return result;
        }
    }
}