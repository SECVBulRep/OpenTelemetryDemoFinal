namespace WeatherApp.Libs.Models;

public class WeatherData
{
    public string? Id { get; set; }
    public string? City { get; set; }
    public int Temperature { get; set; }
    public int Humidity { get; set; }
    public string? Condition { get; set; }
    public DateTime Date { get; set; }
}