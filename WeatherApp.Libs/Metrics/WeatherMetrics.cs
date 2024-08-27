using System.Diagnostics.Metrics;
using WeatherApp.Libs.Models;

namespace WeatherApp.Libs.Metrics;

public class WeatherMetrics
{
    public static readonly string InstrumentSourceName = nameof(WeatherMetrics);

    private int _temperature;
    private readonly Meter _meter; 
    public Counter<int> SummaryRequestByCityCounter { get; }

    public WeatherMetrics(IMeterFactory meterFactory)
    {
        _meter = new Meter(InstrumentSourceName, "1.0.0");
        
        SummaryRequestByCityCounter = _meter.CreateCounter<int>(
            name: "weather_request_by_city_total",
            unit: "requests",
            description: "Total requests number of weather requests by city");

        _meter.CreateObservableGauge(
            name: "weather_forecast_last_requested_temperature_celsius",
            observeValue: () => _temperature,
            unit: "degrees",
            description: "Last requested temperature in Celsius");
    }

    public void SetWeather(WeatherData weatherData) => _temperature = weatherData.Temperature;
}