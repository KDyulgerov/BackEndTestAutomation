using _01.BuildInJSONSupport;
using System.Text.Json;

class Program
{
    static void Main(string[] args)
    {
        WeatherForecast forecast = new WeatherForecast()
        {
            Date = DateTime.Now,
            Temperature = 32,
            Summary = "Example Random Summary"
        };

        string weatherInfo = JsonSerializer.Serialize(forecast);

        Console.WriteLine(weatherInfo);

        string jsonString = File.ReadAllText(Path.Combine(Environment.CurrentDirectory + "/../../../weatherForecast.json"));

        var weatherForecastObj = JsonSerializer.Deserialize<List<WeatherForecast>>(jsonString);

        foreach (var forecastItem in weatherForecastObj)
        {
            Console.WriteLine($"Date: {forecastItem.Date.ToShortDateString()}, {Environment.NewLine}" +
                             $"Temp (C): {forecastItem.Temperature}, {Environment.NewLine}" +
                             $"Summary: {forecastItem.Summary}");
        }
    }
}
