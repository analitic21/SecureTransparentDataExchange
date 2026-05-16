using System;
using System.Collections.Generic;
using SecureTransparentDataExchange.Models;

namespace SecureTransparentDataExchange.Services
{
    public class WeatherForecastService
    {
        public IEnumerable<WeatherForecast> GetForecast()
        {
            var rng = new Random();
            return new List<WeatherForecast>
            {
                new WeatherForecast { Date = DateTime.Now.AddDays(1), TemperatureC = rng.Next(-20, 55), Summary = "Warm" },
                new WeatherForecast { Date = DateTime.Now.AddDays(2), TemperatureC = rng.Next(-20, 55), Summary = "Cold" },
                new WeatherForecast { Date = DateTime.Now.AddDays(3), TemperatureC = rng.Next(-20, 55), Summary = "Mild" },
            };
        }
    }
}
