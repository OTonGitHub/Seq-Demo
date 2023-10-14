using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        _logger.LogInformation("_ILogger: Called Get() on WeaterForecastController");

        for (int i = 1; i <= 10; i++)
        {
            try
            {
                _logger.LogInformation("Current Value of i is {LoopCountValue}", i);
                if (i == 5)
                {
                    throw new Exception("User Prone Testing Exception");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Get() on WeatherForecastController");
            }
        }

        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }
}
