using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrivateSocial.ApiService.Data;
using PrivateSocial.ApiService.Data.Entities;

namespace PrivateSocial.ApiService.Controllers;

/// <summary>
/// Controller for weather forecast operations
/// </summary>
[Route("weatherforecast")]
public class WeatherForecastController : BaseApiController
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    private readonly ApplicationDbContext _context;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, ApplicationDbContext context) 
        : base(logger)
    {
        _context = context;
    }

    /// <summary>
    /// Gets the weather forecast for the next 5 days
    /// </summary>
    /// <returns>A collection of weather forecasts</returns>
    /// <response code="200">Returns the weather forecast</response>
    [HttpGet(Name = "GetWeatherForecast")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IEnumerable<WeatherForecast>> Get()
    {
        Logger.LogInformation("Getting weather forecast from database");
        
        // Check if we have recent forecasts in the database
        var recentForecasts = await _context.WeatherForecasts
            .Where(f => f.Date >= DateOnly.FromDateTime(DateTime.UtcNow))
            .OrderBy(f => f.Date)
            .Take(5)
            .ToListAsync();

        if (recentForecasts.Count >= 5)
        {
            return recentForecasts.Select(f => new WeatherForecast
            {
                Date = f.Date,
                TemperatureC = f.TemperatureC,
                Summary = f.Summary
            });
        }

        // Generate new forecasts if we don't have enough
        Logger.LogInformation("Generating new weather forecasts");
        var newForecasts = new List<WeatherForecastEntity>();
        
        for (int i = 1; i <= 5; i++)
        {
            var forecast = new WeatherForecastEntity
            {
                Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(i)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            };
            newForecasts.Add(forecast);
            _context.WeatherForecasts.Add(forecast);
        }

        await _context.SaveChangesAsync();

        return newForecasts.Select(f => new WeatherForecast
        {
            Date = f.Date,
            TemperatureC = f.TemperatureC,
            Summary = f.Summary
        });
    }
}